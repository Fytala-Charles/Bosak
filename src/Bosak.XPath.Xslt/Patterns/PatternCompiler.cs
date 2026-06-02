// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 mei 2026
// PURPOSE              : Compiles XSLT match patterns into executable node predicates.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 27-05-2026     | Added // prefix support in match patterns                                                |
//                      | Charles Korthout | 0.3   | 28-05-2026     | Added smart split, axis steps, node tests, set ops, variable patterns                    |
//                      | Charles Korthout | 0.4   | 31-05-2026     | Fixed bare predicate patterns ([foo]) compiling as self::node()[foo]                     |
//                      | Charles Korthout | 0.5   | 01-06-2026     | Axis-context predicate evaluation in path patterns; descendant:: direct-child fix      |
//                      | Charles Korthout | 0.6   | 01-06-2026     | Wrap compiled patterns with CurrentItem so fn:current() returns candidate node         |
//                      | Charles Korthout | 0.7   | 01-06-2026     | Propagate static XPath/XSLT errors (XPST/XTSE/XPTY) from pattern predicates            |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text.RegularExpressions;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;

namespace Bosak.XPath.Xslt.Patterns;

/// <summary>
/// Signature for a compiled match pattern predicate.
/// Receives the candidate node and the current evaluation context (needed for variable reference patterns).
/// </summary>
public delegate bool PatternPredicate(IXdmNode node, EvaluationContext context);

/// <summary>
/// Compiles XSLT match patterns (e.g. <c>foo[bar]</c>, <c>*</c>, <c>@id | ref</c>)
/// into <c>PatternPredicate</c> predicates.
/// </summary>
public sealed class PatternCompiler
{
    private static readonly Regex UnionPattern = new(@"\s*\|\s*", RegexOptions.Compiled);

    /// <summary>
    /// Compiles a match pattern string into a predicate function.
    /// </summary>
    public PatternPredicate Compile(string pattern)
    {
        var trimmed = StripXPathComments(pattern).Trim();

        // Normalize top-level "union" to "|" so both syntaxes split uniformly.
        var unionParts = SplitTopLevel(trimmed, "union");
        if (unionParts.Length > 1)
        {
            trimmed = string.Join("|", unionParts);
        }

        var branches = SplitTopLevel(trimmed, '|');
        if (branches.Length == 1)
        {
            return WrapWithCurrentItem(CompileSinglePattern(branches[0]));
        }

        var compiledBranches = branches.Select(CompileSinglePattern).ToArray();
        return WrapWithCurrentItem((node, ctx) => compiledBranches.Any(b => b(node, ctx)));
    }

    /// <summary>
    /// Wraps a compiled pattern so that <c>fn:current()</c> returns the candidate node
    /// being tested, as required by XSLT match-pattern semantics.
    /// </summary>
    private static PatternPredicate WrapWithCurrentItem(PatternPredicate inner)
    {
        return (node, ctx) =>
        {
            var saved = ctx.CurrentItem;
            try
            {
                ctx.WithCurrentItem(XdmValue.FromNode(node));
                return inner(node, ctx);
            }
            finally
            {
                ctx.WithCurrentItem(saved);
            }
        };
    }

    private PatternPredicate CompileSinglePattern(string pattern)
    {
        var trimmed = pattern.Trim();

        // Handle top-level except (not inside parentheses/brackets)
        var exceptParts = SplitTopLevel(trimmed, "except");
        if (exceptParts.Length == 2)
        {
            var left = CompileSinglePattern(exceptParts[0]);
            var right = CompileSinglePattern(exceptParts[1]);
            return (node, ctx) => left(node, ctx) && !right(node, ctx);
        }

        // Handle top-level intersect
        var intersectParts = SplitTopLevel(trimmed, "intersect");
        if (intersectParts.Length == 2)
        {
            var left = CompileSinglePattern(intersectParts[0]);
            var right = CompileSinglePattern(intersectParts[1]);
            return (node, ctx) => left(node, ctx) && right(node, ctx);
        }

        // Handle // prefix (e.g. //foo, //foo[bar]) — matches any descendant
        if (trimmed.StartsWith("//"))
        {
            trimmed = trimmed[2..].Trim();
        }
        // Handle / prefix (e.g. /doc) — matches from the root
        else if (trimmed.StartsWith('/'))
        {
            trimmed = trimmed[1..].Trim();
            if (string.IsNullOrEmpty(trimmed))
                return (node, ctx) => node.NodeKind == XdmNodeKind.Document;

            // For multi-step rooted paths like /*/* or /doc/foo, the document-root check
            // must apply to the first step (the root element), not the candidate node.
            bool isMultiStep = false;
            int braceDepth = 0;
            for (int pi = 0; pi < trimmed.Length; pi++)
            {
                if (trimmed[pi] == 'Q' && pi + 1 < trimmed.Length && trimmed[pi + 1] == '{')
                {
                    braceDepth++;
                    pi++;
                }
                else if (braceDepth > 0 && trimmed[pi] == '}')
                {
                    braceDepth--;
                }
                else if (braceDepth == 0 && trimmed[pi] == '/')
                {
                    isMultiStep = true;
                    break;
                }
            }

            if (isMultiStep)
            {
                return CompilePathPattern(trimmed, rootAtDocument: true);
            }

            var inner = CompileSinglePattern(trimmed);
            return (node, ctx) => inner(node, ctx) && node.Parent?.NodeKind == XdmNodeKind.Document;
        }

        // Variable reference pattern: $var or $var/path
        if (trimmed.StartsWith('$'))
        {
            return CompileVariablePattern(trimmed);
        }

        // Document/root pattern with path: doc('uri')/foo, root()/foo
        if ((trimmed.StartsWith("doc(") || trimmed.StartsWith("document(") || trimmed.StartsWith("root(")) && trimmed.Contains('/'))
        {
            return CompileDocumentPathPattern(trimmed);
        }

        // Simple document pattern: doc('uri') or document('uri')
        if (trimmed.StartsWith("doc(") && trimmed.EndsWith(')'))
        {
            return (node, ctx) => node.NodeKind == XdmNodeKind.Document;
        }
        if (trimmed.StartsWith("document(") && trimmed.EndsWith(')'))
        {
            return (node, ctx) => node.NodeKind == XdmNodeKind.Document;
        }

        // Parenthesized pattern: (pattern) or (pattern)[predicate]
        if (trimmed.StartsWith('('))
        {
            int closeParen = FindMatchingParen(trimmed, 0);
            if (closeParen > 0)
            {
                string inside = trimmed[1..closeParen].Trim();
                string after = closeParen + 1 < trimmed.Length ? trimmed[(closeParen + 1)..].Trim() : "";

                var innerPattern = Compile(inside);

                if (string.IsNullOrEmpty(after))
                    return innerPattern;

                // After the parens there might be predicates or path continuations
                if (after.StartsWith('['))
                {
                    return CompileParenthesizedWithPredicates(innerPattern, after);
                }
                if (after.StartsWith('/'))
                {
                    // (pattern)/step — combine as path
                    return CompilePathPattern(trimmed);
                }
            }
        }

        // Attribute pattern: @name or @*
        if (trimmed.StartsWith('@'))
        {
            var name = trimmed[1..].Trim();
            return CompileAttributePattern(name);
        }

        // Path pattern: a/b or a/b/c (but not axis steps handled above)
        // Ignore '/' inside Q{uri} braces.
        bool hasPathSlash = false;
        int qDepth = 0;
        for (int pi = 0; pi < trimmed.Length; pi++)
        {
            if (trimmed[pi] == 'Q' && pi + 1 < trimmed.Length && trimmed[pi + 1] == '{')
            {
                qDepth++;
                pi++;
            }
            else if (qDepth > 0 && trimmed[pi] == '}')
            {
                qDepth--;
            }
            else if (qDepth == 0 && trimmed[pi] == '/')
            {
                hasPathSlash = true;
                break;
            }
        }
        if (hasPathSlash && !trimmed.StartsWith("processing-instruction"))
        {
            return CompilePathPattern(trimmed);
        }

        // Predicate pattern: foo[bar]
        if (trimmed.Contains('['))
        {
            return CompilePredicatePattern(trimmed);
        }

        // Simple element name or wildcard, or axis step without predicate
        return CompileElementPattern(trimmed);
    }

    /// <summary>
    /// Compiles a variable reference pattern ($var) or variable path pattern ($var/foo).
    /// </summary>
    private PatternPredicate CompileVariablePattern(string pattern)
    {
        // Check if there's a path after the variable name
        int slashIdx = FindTopLevelSlash(pattern);
        if (slashIdx > 0)
        {
            string varPart = pattern[..slashIdx].Trim();
            string pathPart = pattern[slashIdx..].Trim();

            // Compile the path part as a pattern that would match from the variable's nodes
            var pathPattern = CompileSinglePattern(pathPart.TrimStart('/'));

            // Evaluate: match node N if N matches pathPattern and some node in $var is an ancestor
            var compiledVarCheck = XPath31Expression.Compile(varPart);
            bool isDescendant = pathPart.StartsWith("//");
            string pathNoSlash = pathPart.TrimStart('/').TrimStart('/');
            var pathCompiled = XPath31Expression.Compile(pathNoSlash);

            return (node, ctx) =>
            {
                try
                {
                    var varResult = compiledVarCheck.Evaluate(ctx);
                    if (!varResult.IsSequence && !varResult.IsNode)
                        return false;

                    // Check if node matches the path pattern
                    if (!pathPattern(node, ctx))
                        return false;

                    // Check if any node in the variable is an ancestor (or self) of node
                    if (varResult.IsSequence && varResult.SequenceValue != null)
                    {
                        foreach (var item in XdmSequence.FromSource(varResult.SequenceValue))
                        {
                            if (!item.IsNode || item.NodeValue is not IXdmNode vn)
                                continue;
                            if (isDescendant)
                            {
                                var current = node;
                                while (current != null)
                                {
                                    if (current.IsSameNode(vn))
                                        return true;
                                    current = current.Parent;
                                }
                            }
                            else
                            {
                                if (node.Parent != null && node.Parent.IsSameNode(vn))
                                    return true;
                            }
                        }
                    }
                    else if (varResult.IsNode && varResult.NodeValue is IXdmNode vn2)
                    {
                        if (isDescendant)
                        {
                            var current = node;
                            while (current != null)
                            {
                                if (current.IsSameNode(vn2))
                                    return true;
                                current = current.Parent;
                            }
                        }
                        else
                        {
                            if (node.Parent != null && node.Parent.IsSameNode(vn2))
                                return true;
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    if (IsStaticError(ex)) throw;
                    return false;
                }
            };
        }

        // Simple variable reference: $var
        var compiledVar = XPath31Expression.Compile(pattern);
        return (node, ctx) =>
        {
            try
            {
                var result = compiledVar.Evaluate(ctx);
                if (result.IsSequence && result.SequenceValue != null)
                {
                    foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                    {
                        if (item.IsNode && item.NodeValue is IXdmNode n && n.IsSameNode(node))
                            return true;
                    }
                }
                else if (result.IsNode && result.NodeValue is IXdmNode n2 && n2.IsSameNode(node))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                if (IsStaticError(ex)) throw;
                return false;
            }
        };
    }

    /// <summary>
    /// Compiles a document pattern with a path: doc('uri')/foo or doc('uri')//foo.
    /// </summary>
    private PatternPredicate CompileDocumentPathPattern(string pattern)
    {
        // Find the first / that separates the doc() call from the path
        int parenDepth = 0;
        int slashIdx = -1;
        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];
            if (c == '(') parenDepth++;
            else if (c == ')') parenDepth--;
            else if (c == '/' && parenDepth == 0)
            {
                slashIdx = i;
                break;
            }
        }

        if (slashIdx < 0)
        {
            // No slash found — simple document or root pattern
            if (pattern.StartsWith("root("))
            {
                return (node, ctx) =>
                {
                    var current = node;
                    while (current.Parent != null) current = current.Parent;
                    return current.IsSameNode(node);
                };
            }
            return (node, ctx) => node.NodeKind == XdmNodeKind.Document;
        }

        string docPart = pattern[..slashIdx].Trim();
        string pathPart = pattern[slashIdx..].Trim();

        // Compile the full path as an XPath expression: (docPart)pathPart
        // This correctly handles multi-step paths, predicates, and descendant axes.
        var fullPathCompiled = XPath31Expression.Compile($"({docPart}){pathPart}");

        return (node, ctx) =>
        {
            var savedItem = ctx.ContextItem;
            var savedPos = ctx.ContextPosition;
            var savedSize = ctx.ContextSize;
            try
            {
                var result = fullPathCompiled.Evaluate(ctx.WithFocus(XdmValue.FromNode(node), 1, 1));
                if (result.Kind == XdmValueKind.Sequence && result.SequenceValue != null)
                {
                    foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                    {
                        if (item.IsNode && item.NodeValue is IXdmNode n && n.IsSameNode(node))
                            return true;
                    }
                }
                else if (result.IsNode && result.NodeValue is IXdmNode n2 && n2.IsSameNode(node))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                if (IsStaticError(ex)) throw;
                return false;
            }
            finally
            {
                ctx.WithFocus(savedItem, savedPos, savedSize);
            }
        };
    }

    /// <summary>
    /// Compiles a simple path pattern like <c>a/b</c> or <c>a/b/c</c>.
    /// The last step is the node test; preceding steps are ancestor checks.
    /// </summary>
    private PatternPredicate CompilePathPattern(string pattern, bool rootAtDocument = false)
    {
        var steps = new List<string>();
        var separators = new List<bool>(); // true = /, false = //
        int i = 0;

        while (i < pattern.Length)
        {
            bool isDirect = true;
            if (i > 0)
            {
                if (i + 1 < pattern.Length && pattern[i] == '/' && pattern[i + 1] == '/')
                {
                    isDirect = false;
                    i += 2;
                }
                else if (pattern[i] == '/')
                {
                    isDirect = true;
                    i++;
                }
            }

            int start = i;
            int depth = 0;
            int qBraceDepth = 0;
            while (i < pattern.Length)
            {
                char c = pattern[i];
                if (qBraceDepth > 0)
                {
                    if (c == '}') qBraceDepth--;
                    i++;
                    continue;
                }
                if (c == 'Q' && i + 1 < pattern.Length && pattern[i + 1] == '{')
                {
                    qBraceDepth++;
                    i += 2;
                    continue;
                }
                if (c == '(' || c == '[') depth++;
                else if (c == ')' || c == ']') depth--;
                else if (depth == 0 && (pattern[i] == '/' || (i + 1 < pattern.Length && pattern[i] == '/' && pattern[i + 1] == '/')))
                    break;
                i++;
            }

            var step = pattern[start..i].Trim();
            if (!string.IsNullOrEmpty(step))
            {
                if (steps.Count > 0)
                    separators.Add(isDirect);
                steps.Add(step);
            }
        }

        if (steps.Count == 0)
            return (node, ctx) => false;

        // Determine the effective axis of each step so we can handle descendant::
        // correctly in ancestor checks (a/descendant::b should match deep descendants).
        var stepAxes = new List<string>();
        foreach (var step in steps)
        {
            if (step.Contains("::"))
            {
                int axisEnd = step.IndexOf("::");
                stepAxes.Add(step[..axisEnd].Trim().ToLowerInvariant());
            }
            else
            {
                stepAxes.Add("child");
            }
        }

        string lastStepStr = steps[^1];
        bool lastStepHasPredicate = lastStepStr.Contains('[');

        // Simple element/attribute predicates are handled correctly by CompilePredicatePattern
        // because it evaluates child::base[pred] from the parent. Axis steps and other
        // complex patterns need axis-context evaluation for correct position()/last() semantics.
        bool isSimpleStep = !lastStepStr.Contains("::") && !lastStepStr.StartsWith('$') && !lastStepStr.StartsWith('(');
        bool lastStepNeedsAxisContext = lastStepHasPredicate && !isSimpleStep;

        XPath31Expression? lastStepAxisExpr = null;
        PatternPredicate? lastStepPredicate = null;

        if (lastStepNeedsAxisContext && lastStepStr.Contains("::"))
        {
            lastStepAxisExpr = XPath31Expression.Compile(lastStepStr);
        }
        else if (lastStepHasPredicate)
        {
            lastStepPredicate = CompilePredicatePattern(lastStepStr);
        }
        else
        {
            lastStepPredicate = CompileSinglePattern(lastStepStr);
        }

        // Compile ancestor checks for preceding steps
        var ancestorTests = new List<PatternPredicate>();
        for (int s = steps.Count - 2; s >= 0; s--)
        {
            if (steps[s].Contains('['))
                ancestorTests.Add(CompilePredicatePattern(steps[s]));
            else
                ancestorTests.Add(CompileSinglePattern(steps[s]));
        }

        return (node, ctx) =>
        {
            // For simple or non-predicated last steps, check before ancestor walk
            if (lastStepPredicate != null && !lastStepPredicate(node, ctx))
                return false;

            var current = node.Parent;
            IXdmNode? stepContextNode = null;

            for (int s = steps.Count - 2; s >= 0; s--)
            {
                var test = ancestorTests[steps.Count - 2 - s];
                // If the next step uses descendant or descendant-or-self axis,
                // treat a / separator as non-direct so we walk up the tree.
                bool direct = separators[s]
                    && stepAxes[s + 1] != "descendant"
                    && stepAxes[s + 1] != "descendant-or-self";

                if (direct)
                {
                    if (current == null || !test(current, ctx))
                        return false;
                    stepContextNode = current;
                    current = current.Parent;
                }
                else
                {
                    bool found = false;
                    while (current != null)
                    {
                        if (test(current, ctx))
                        {
                            found = true;
                            stepContextNode = current;
                            current = current.Parent;
                            break;
                        }
                        current = current.Parent;
                    }
                    if (!found)
                        return false;
                }
            }

            // For axis-step predicates, evaluate the last step XPath from the matching
            // ancestor context so position() and last() reflect the correct list position.
            if (lastStepAxisExpr != null)
            {
                var savedItem = ctx.ContextItem;
                var savedPos = ctx.ContextPosition;
                var savedSize = ctx.ContextSize;
                try
                {
                    var focusNode = stepContextNode ?? node.Parent;
                    if (focusNode == null)
                        return false;

                    var result = lastStepAxisExpr.Evaluate(ctx.WithFocus(XdmValue.FromNode(focusNode), 1, 1));
                    if (result.Kind == XdmValueKind.Sequence && result.SequenceValue != null)
                    {
                        foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                        {
                            if (item.IsNode && item.NodeValue is IXdmNode n && n.IsSameNode(node))
                                return true;
                        }
                        return false;
                    }
                    else if (result.IsNode && result.NodeValue is IXdmNode n2 && n2.IsSameNode(node))
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    if (IsStaticError(ex)) throw;
                    return false;
                }
                finally
                {
                    ctx.WithFocus(savedItem, savedPos, savedSize);
                }
            }

            if (rootAtDocument)
            {
                // For rooted paths like /*/*, the first step must match a child of the document.
                // After walking ancestors, 'current' is the parent of the node that matched
                // the first step, which must be the document node.
                if (current?.NodeKind != XdmNodeKind.Document)
                    return false;
            }

            return true;
        };
    }

    private PatternPredicate CompileElementPattern(string name)
    {
        // Handle axis steps: axis::nodetest
        if (name.Contains("::"))
        {
            int colonIdx = name.IndexOf("::");
            string axis = name[..colonIdx].Trim();
            string nodeTest = name[(colonIdx + 2)..].Trim();
            return CompileAxisStep(axis, nodeTest);
        }

        if (name == "*")
        {
            return (node, ctx) => node.NodeKind == XdmNodeKind.Element;
        }

        if (name == "node()" || name == ".")
        {
            return (node, ctx) => true;
        }

        if (name == "text()")
        {
            return (node, ctx) => node.NodeKind == XdmNodeKind.Text;
        }

        if (name == "comment()")
        {
            return (node, ctx) => node.NodeKind == XdmNodeKind.Comment;
        }

        if (name == "processing-instruction()")
        {
            return (node, ctx) => node.NodeKind == XdmNodeKind.ProcessingInstruction;
        }

        if (name.StartsWith("processing-instruction("))
        {
            // processing-instruction(name) or processing-instruction('name')
            var piName = ExtractFunctionArg(name);
            if (string.IsNullOrEmpty(piName))
                return (node, ctx) => node.NodeKind == XdmNodeKind.ProcessingInstruction;
            return (node, ctx) =>
                node.NodeKind == XdmNodeKind.ProcessingInstruction &&
                node.LocalName == piName.Trim('\'');
        }

        if (name == "document-node()")
        {
            return (node, ctx) => node.NodeKind == XdmNodeKind.Document;
        }

        if (name == "root()")
        {
            return (node, ctx) =>
            {
                var current = node;
                while (current.Parent != null) current = current.Parent;
                return current.IsSameNode(node);
            };
        }

        if (name == "element()")
        {
            return (node, ctx) => node.NodeKind == XdmNodeKind.Element;
        }

        if (name.StartsWith("element("))
        {
            var arg = ExtractFunctionArg(name);
            if (string.IsNullOrEmpty(arg) || arg == "*")
                return (node, ctx) => node.NodeKind == XdmNodeKind.Element;
            // element(name) or element(QName)
            var (ns, local) = ParseQName(arg);
            if (string.IsNullOrEmpty(ns))
                return (node, ctx) => node.NodeKind == XdmNodeKind.Element && node.LocalName == local;
            return (node, ctx) => node.NodeKind == XdmNodeKind.Element && node.NamespaceUri == ns && node.LocalName == local;
        }

        if (name == "attribute()")
        {
            return (node, ctx) => node.NodeKind == XdmNodeKind.Attribute;
        }

        if (name.StartsWith("attribute("))
        {
            var arg = ExtractFunctionArg(name);
            if (string.IsNullOrEmpty(arg) || arg == "*")
                return (node, ctx) => node.NodeKind == XdmNodeKind.Attribute;
            var (ns, local) = ParseQName(arg);
            if (string.IsNullOrEmpty(ns))
                return (node, ctx) => node.NodeKind == XdmNodeKind.Attribute && node.LocalName == local;
            return (node, ctx) => node.NodeKind == XdmNodeKind.Attribute && node.NamespaceUri == ns && node.LocalName == local;
        }

        // id('x', $y) pattern
        if (name.StartsWith("id(") && name.EndsWith(')'))
        {
            var compiledId = XPath31Expression.Compile($"self::node()[. is {name}]");
            return (node, ctx) =>
            {
                try
                {
                    var result = compiledId.Evaluate(ctx.WithFocus(XdmValue.FromNode(node), 1, 1));
                    return result.EffectiveBooleanValue();
                }
                catch (Exception ex)
                {
                    if (IsStaticError(ex)) throw;
                    return false;
                }
            };
        }

        // key('k', 'v') pattern
        if (name.StartsWith("key(") && name.EndsWith(')'))
        {
            var compiledKey = XPath31Expression.Compile($"self::node()[. is {name}]");
            return (node, ctx) =>
            {
                try
                {
                    var result = compiledKey.Evaluate(ctx.WithFocus(XdmValue.FromNode(node), 1, 1));
                    return result.EffectiveBooleanValue();
                }
                catch (Exception ex)
                {
                    if (IsStaticError(ex)) throw;
                    return false;
                }
            };
        }

        // Qualified name: prefix:local or Q{uri}local
        var (nsUri, localName) = ParseQName(name);

        if (string.IsNullOrEmpty(nsUri))
        {
            return (node, ctx) =>
                node.NodeKind == XdmNodeKind.Element &&
                node.LocalName == localName;
        }

        return (node, ctx) =>
            node.NodeKind == XdmNodeKind.Element &&
            node.NamespaceUri == nsUri &&
            node.LocalName == localName;
    }

    private PatternPredicate CompileAxisStep(string axis, string nodeTest)
    {
        axis = axis.ToLowerInvariant();

        switch (axis)
        {
            case "self":
                var selfTest = CompileNodeTestPredicate(nodeTest);
                return (node, ctx) => selfTest(node);

            case "child":
                var childTest = CompileNodeTestPredicate(nodeTest);
                return (node, ctx) =>
                    node.NodeKind != XdmNodeKind.Document
                    && node.NodeKind != XdmNodeKind.Attribute
                    && node.NodeKind != XdmNodeKind.Namespace
                    && childTest(node);

            case "descendant":
                // In a pattern step, descendant::foo matches foo that has an ancestor
                // that matches the preceding step. Since this is used as a step in a path,
                // we just test the node against the node test.
                var descTest = CompileNodeTestPredicate(nodeTest);
                return (node, ctx) => descTest(node);

            case "descendant-or-self":
                var dosTest = CompileNodeTestPredicate(nodeTest);
                return (node, ctx) => dosTest(node);

            case "attribute":
                var attrTest = CompileAttributeNodeTest(nodeTest);
                return (node, ctx) => attrTest(node);

            case "parent":
            case "ancestor":
            case "following-sibling":
            case "preceding-sibling":
            case "following":
            case "preceding":
            case "ancestor-or-self":
                // These axes are not allowed in patterns per XSLT spec,
                // but we compile them as node tests for graceful handling.
                var fallbackTest = CompileNodeTestPredicate(nodeTest);
                return (node, ctx) => fallbackTest(node);

            case "namespace":
                if (nodeTest == "*")
                    return (node, ctx) => node.NodeKind == XdmNodeKind.Namespace;
                return (node, ctx) => node.NodeKind == XdmNodeKind.Namespace && node.LocalName == nodeTest;

            default:
                var defaultTest = CompileNodeTestPredicate(nodeTest);
                return (node, ctx) => defaultTest(node);
        }
    }

    private Func<IXdmNode, bool> CompileNodeTestPredicate(string nodeTest)
    {
        if (nodeTest == "*")
            return node => node.NodeKind == XdmNodeKind.Element;
        if (nodeTest == "node()")
            return node => true;
        if (nodeTest == "text()")
            return node => node.NodeKind == XdmNodeKind.Text;
        if (nodeTest == "comment()")
            return node => node.NodeKind == XdmNodeKind.Comment;
        if (nodeTest == "processing-instruction()")
            return node => node.NodeKind == XdmNodeKind.ProcessingInstruction;
        if (nodeTest == "element()")
            return node => node.NodeKind == XdmNodeKind.Element;
        if (nodeTest == "document-node()")
            return node => node.NodeKind == XdmNodeKind.Document;

        if (nodeTest.StartsWith("element("))
        {
            var arg = ExtractFunctionArg(nodeTest);
            if (string.IsNullOrEmpty(arg) || arg == "*")
                return node => node.NodeKind == XdmNodeKind.Element;
            var (ns, local) = ParseQName(arg);
            if (string.IsNullOrEmpty(ns))
                return node => node.NodeKind == XdmNodeKind.Element && node.LocalName == local;
            return node => node.NodeKind == XdmNodeKind.Element && node.NamespaceUri == ns && node.LocalName == local;
        }

        var (nsUri, localName) = ParseQName(nodeTest);
        if (string.IsNullOrEmpty(nsUri))
            return node => node.NodeKind == XdmNodeKind.Element && node.LocalName == localName;
        return node => node.NodeKind == XdmNodeKind.Element && node.NamespaceUri == nsUri && node.LocalName == localName;
    }

    private Func<IXdmNode, bool> CompileAttributeNodeTest(string nodeTest)
    {
        if (nodeTest == "*")
            return node => node.NodeKind == XdmNodeKind.Attribute;
        if (nodeTest == "node()")
            return node => node.NodeKind == XdmNodeKind.Attribute;
        if (nodeTest == "text()")
            return node => node.NodeKind == XdmNodeKind.Attribute; // Unusual but valid
        if (nodeTest == "comment()")
            return node => node.NodeKind == XdmNodeKind.Attribute;
        if (nodeTest == "processing-instruction()")
            return node => node.NodeKind == XdmNodeKind.Attribute;
        if (nodeTest == "element()")
            return node => node.NodeKind == XdmNodeKind.Attribute;

        var (nsUri, localName) = ParseQName(nodeTest);
        if (string.IsNullOrEmpty(nsUri))
            return node => node.NodeKind == XdmNodeKind.Attribute && node.LocalName == localName;
        return node => node.NodeKind == XdmNodeKind.Attribute && node.NamespaceUri == nsUri && node.LocalName == localName;
    }

    private PatternPredicate CompileAttributePattern(string name)
    {
        if (name == "*")
        {
            return (node, ctx) => node.NodeKind == XdmNodeKind.Attribute;
        }

        // namespace::* 
        if (name == "namespace::*")
        {
            return (node, ctx) => node.NodeKind == XdmNodeKind.Namespace;
        }

        var (ns, local) = ParseQName(name);

        if (string.IsNullOrEmpty(ns))
        {
            return (node, ctx) =>
                node.NodeKind == XdmNodeKind.Attribute &&
                node.LocalName == local;
        }

        return (node, ctx) =>
            node.NodeKind == XdmNodeKind.Attribute &&
            node.NamespaceUri == ns &&
            node.LocalName == local;
    }

    private PatternPredicate CompilePredicatePattern(string pattern)
    {
        int bracketOpen = pattern.IndexOf('[');
        if (bracketOpen < 0)
            return CompileSinglePattern(pattern);

        // Find matching close bracket
        int bracketClose = -1;
        int depth = 0;
        for (int i = bracketOpen; i < pattern.Length; i++)
        {
            if (pattern[i] == '[') depth++;
            else if (pattern[i] == ']') depth--;
            if (depth == 0)
            {
                bracketClose = i;
                break;
            }
        }
        if (bracketClose < 0 || bracketClose < bracketOpen)
            throw new InvalidOperationException($"Invalid pattern: {pattern}");

        string basePattern = pattern[..bracketOpen].Trim();
        string predicateExpr = pattern[(bracketOpen + 1)..bracketClose].Trim();
        string remaining = bracketClose + 1 < pattern.Length ? pattern[(bracketClose + 1)..] : "";

        // Special case: .[predicate] or [predicate] matches any node where the predicate is true.
        // Handle . with trailing comments: strip leading '.' and check the rest is only comments/whitespace.
        bool IsDotPattern(string bp)
        {
            var t = bp.Trim();
            if (t == ".") return true;
            if (t.StartsWith("."))
            {
                // After '.', the rest should be whitespace and/or XPath comments
                int i = 1;
                while (i < t.Length)
                {
                    if (char.IsWhiteSpace(t[i])) { i++; continue; }
                    if (i + 1 < t.Length && t[i] == '(' && t[i + 1] == ':')
                    {
                        // Skip comment
                        i += 2;
                        int depth = 1;
                        while (i < t.Length && depth > 0)
                        {
                            if (i + 1 < t.Length && t[i] == ':' && t[i + 1] == ')') { depth--; i += 2; }
                            else if (i + 1 < t.Length && t[i] == '(' && t[i + 1] == ':') { depth++; i += 2; }
                            else i++;
                        }
                        continue;
                    }
                    return false;
                }
                return true;
            }
            return false;
        }

        if (IsDotPattern(basePattern) || string.IsNullOrEmpty(basePattern))
        {
            var fullPredicate = $"self::node()[{predicateExpr}]{remaining}";
            var dotCompiled = XPath31Expression.Compile(fullPredicate);

            return (node, ctx) =>
            {
                var savedItem = ctx.ContextItem;
                var savedPos = ctx.ContextPosition;
                var savedSize = ctx.ContextSize;
                try
                {
                    var result = dotCompiled.Evaluate(ctx.WithFocus(XdmValue.FromNode(node), 1, 1));
                    return result.EffectiveBooleanValue();
                }
                catch (Exception ex)
                {
                    if (IsStaticError(ex)) throw;
                    return false;
                }
                finally
                {
                    ctx.WithFocus(savedItem, savedPos, savedSize);
                }
            };
        }

        var basePredicate = CompileSinglePattern(basePattern);

        // For simple element/attribute patterns with predicates, evaluate from the parent
        // so that position() and last() reflect the node's position among its siblings.
        bool isSimpleElement = !basePattern.Contains('/') && !basePattern.Contains('|') && !basePattern.Contains("::") && !basePattern.StartsWith('$') && !basePattern.StartsWith('(') && !basePattern.StartsWith('@') && !basePattern.Contains('.');
        bool isSimpleAttribute = !basePattern.Contains('/') && !basePattern.Contains('|') && !basePattern.Contains("::") && basePattern.StartsWith('@');

        // Function-call patterns like root(), doc(), id(), key() are not valid node tests
        // and cannot be used in child::/attribute:: axis steps. Kind tests (node(), element() etc.) are OK.
        if ((isSimpleElement || isSimpleAttribute) && basePattern.Contains('(') && basePattern.EndsWith(')') && !IsKindTestPattern(basePattern))
        {
            isSimpleElement = false;
            isSimpleAttribute = false;
        }

        if (isSimpleElement || isSimpleAttribute)
        {
            var axisStep = isSimpleAttribute
                ? $"attribute::{basePattern[1..]}[{predicateExpr}]{remaining}"
                : $"child::{basePattern}[{predicateExpr}]{remaining}";
            var compiledStep = XPath31Expression.Compile(axisStep);
            var fallbackPred = XPath31Expression.Compile($"self::node()[{predicateExpr}]{remaining}");

            return (node, ctx) =>
            {
                if (!basePredicate(node, ctx))
                    return false;

                var parent = node.Parent;
                var savedItem = ctx.ContextItem;
                var savedPos = ctx.ContextPosition;
                var savedSize = ctx.ContextSize;
                try
                {
                    XdmValue result;
                    if (parent == null)
                    {
                        // Parentless node: evaluate predicate directly (position() is 1/1)
                        result = fallbackPred.Evaluate(ctx.WithFocus(XdmValue.FromNode(node), 1, 1));
                        return result.EffectiveBooleanValue();
                    }

                    result = compiledStep.Evaluate(ctx.WithFocus(XdmValue.FromNode(parent), 1, 1));

                    if (result.Kind == XdmValueKind.Sequence && result.SequenceValue != null)
                    {
                        foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                        {
                            if (item.IsNode && item.NodeValue is IXdmNode n && n.IsSameNode(node))
                                return true;
                        }
                    }
                    else if (result.IsNode && result.NodeValue is IXdmNode n2 && n2.IsSameNode(node))
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    if (IsStaticError(ex)) throw;
                    return false;
                }
                finally
                {
                    ctx.WithFocus(savedItem, savedPos, savedSize);
                }
            };
        }

        // For complex base patterns (unions, variable refs, parenthesized), evaluate predicate
        // with the node as focus. This means position()/last() will be 1/1, which is a limitation
        // for patterns like (foo|baz)[position()=2], but handles the common case of (foo|baz)[*].
        string predXPath = $"self::node()[{predicateExpr}]{remaining}";
        var compiledPred = XPath31Expression.Compile(predXPath);

        return (node, ctx) =>
        {
            if (!basePredicate(node, ctx))
                return false;

            var savedItem = ctx.ContextItem;
            var savedPos = ctx.ContextPosition;
            var savedSize = ctx.ContextSize;
            try
            {
                var result = compiledPred.Evaluate(ctx.WithFocus(XdmValue.FromNode(node), 1, 1));
                return result.EffectiveBooleanValue();
            }
            catch (Exception ex)
            {
                if (IsStaticError(ex)) throw;
                return false;
            }
            finally
            {
                ctx.WithFocus(savedItem, savedPos, savedSize);
            }
        };
    }

    private PatternPredicate CompileParenthesizedWithPredicates(PatternPredicate innerPattern, string after)
    {
        // after starts with [predicate...]
        int bracketOpen = after.IndexOf('[');
        if (bracketOpen < 0)
            return innerPattern;

        int bracketClose = -1;
        int depth = 0;
        for (int i = bracketOpen; i < after.Length; i++)
        {
            if (after[i] == '[') depth++;
            else if (after[i] == ']') depth--;
            if (depth == 0)
            {
                bracketClose = i;
                break;
            }
        }
        if (bracketClose < 0)
            return innerPattern;

        string predicateExpr = after[(bracketOpen + 1)..bracketClose].Trim();
        string remaining = bracketClose + 1 < after.Length ? after[(bracketClose + 1)..] : "";

        var compiledPred = XPath31Expression.Compile($"self::node()[{predicateExpr}]{remaining}");

        return (node, ctx) =>
        {
            if (!innerPattern(node, ctx))
                return false;

            try
            {
                var result = compiledPred.Evaluate(ctx.WithFocus(XdmValue.FromNode(node), 1, 1));
                return result.EffectiveBooleanValue();
            }
            catch (Exception ex)
            {
                if (IsStaticError(ex)) throw;
                return false;
            }
        };
    }

    // ------------------------------------------------------------------
    // Helper methods
    // ------------------------------------------------------------------

    /// <summary>
    /// Splits text on separator only at the top level (not inside parentheses, brackets, or braces).
    /// </summary>
    private static string[] SplitTopLevel(string text, char separator)
    {
        var parts = new List<string>();
        int start = 0;
        int depth = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '(' || c == '[' || c == '{') depth++;
            else if (c == ')' || c == ']' || c == '}') depth--;
            else if (c == separator && depth == 0)
            {
                parts.Add(text[start..i].Trim());
                start = i + 1;
            }
        }
        parts.Add(text[start..].Trim());
        return parts.Where(p => !string.IsNullOrEmpty(p)).ToArray();
    }

    /// <summary>
    /// Splits text on a word separator (like "except" or "intersect") only at the top level.
    /// The separator must be surrounded by non-word characters or string boundaries.
    /// </summary>
    private static string[] SplitTopLevel(string text, string separator)
    {
        var parts = new List<string>();
        int start = 0;
        int depth = 0;
        for (int i = 0; i <= text.Length - separator.Length; i++)
        {
            char c = text[i];
            if (c == '(' || c == '[' || c == '{') depth++;
            else if (c == ')' || c == ']' || c == '}') depth--;
            else if (depth == 0 && text.AsSpan(i, separator.Length).Equals(separator.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                // Check word boundaries
                bool leftBoundary = i == 0 || !char.IsLetterOrDigit(text[i - 1]);
                bool rightBoundary = i + separator.Length == text.Length || !char.IsLetterOrDigit(text[i + separator.Length]);
                if (leftBoundary && rightBoundary)
                {
                    parts.Add(text[start..i].Trim());
                    start = i + separator.Length;
                    i += separator.Length - 1;
                }
            }
        }
        parts.Add(text[start..].Trim());
        return parts.Where(p => !string.IsNullOrEmpty(p)).ToArray();
    }

    /// <summary>
    /// Finds the index of the matching closing parenthesis for the paren at startIndex.
    /// Returns -1 if not found.
    /// </summary>
    private static int FindMatchingParen(string text, int startIndex)
    {
        if (text[startIndex] != '(') return -1;
        int depth = 1;
        for (int i = startIndex + 1; i < text.Length; i++)
        {
            if (text[i] == '(') depth++;
            else if (text[i] == ')') depth--;
            if (depth == 0) return i;
        }
        return -1;
    }

    /// <summary>
    /// Checks if the outermost parentheses in text are balanced and at the end (modulo whitespace).
    /// </summary>
    private static bool IsBalancedAtEnd(string text, char open, char close)
    {
        if (text[0] != open) return false;
        int depth = 1;
        for (int i = 1; i < text.Length; i++)
        {
            if (text[i] == open) depth++;
            else if (text[i] == close) depth--;
            if (depth == 0)
            {
                // Check if only whitespace follows
                for (int j = i + 1; j < text.Length; j++)
                    if (!char.IsWhiteSpace(text[j])) return false;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Finds the first '/' at top level (not inside parentheses or brackets).
    /// </summary>
    private static int FindTopLevelSlash(string text)
    {
        int depth = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '(' || c == '[') depth++;
            else if (c == ')' || c == ']') depth--;
            else if (c == '/' && depth == 0)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Extracts the argument from a function-like syntax: func(arg).
    /// Handles nested parens.
    /// </summary>
    private static string ExtractFunctionArg(string text)
    {
        int open = text.IndexOf('(');
        if (open < 0) return "";
        int close = FindMatchingParen(text, open);
        if (close < 0) return "";
        return text[(open + 1)..close].Trim();
    }

    /// <summary>
    /// Parses a qualified name into (namespaceUri, localName).
    /// Supports prefix:local and Q{uri}local syntax.
    /// </summary>
    private static (string NamespaceUri, string LocalName) ParseQName(string name)
    {
        if (name.StartsWith("Q{"))
        {
            int closeBrace = name.IndexOf('}');
            if (closeBrace > 2)
            {
                var ns = name[2..closeBrace];
                var local = name[(closeBrace + 1)..];
                return (ns, local);
            }
        }

        int colon = name.IndexOf(':');
        if (colon > 0)
        {
            var local = name[(colon + 1)..];
            return (string.Empty, local);
        }

        return (string.Empty, name);
    }

    /// <summary>
    /// Returns whether an exception represents a static XPath or XSLT error that should
    /// propagate rather than being treated as a non-matching pattern.
    /// </summary>
    private static bool IsStaticError(Exception ex)
    {
        var msg = ex.Message;
        return msg.Contains("XPST") || msg.Contains("XTSE") || msg.Contains("XPTY");
    }

    /// <summary>
    /// Returns whether the pattern base is a kind test (node(), element(), etc.) rather than
    /// a function call pattern like root() or doc().
    /// </summary>
    private static bool IsKindTestPattern(string name)
    {
        if (!name.Contains('(') || !name.EndsWith(')')) return false;
        int open = name.IndexOf('(');
        string testName = name[..open];
        return testName switch
        {
            "node" or "text" or "comment" or "processing-instruction" or "element" or "attribute" or "schema-element" or "schema-attribute" or "document-node" => true,
            _ => false
        };
    }

    /// <summary>
    /// Removes XPath comments <c>(: ... :)</c> from the text, preserving string literals.
    /// </summary>
    private static string StripXPathComments(string text)
    {
        var sb = new System.Text.StringBuilder();
        int i = 0;
        while (i < text.Length)
        {
            char c = text[i];
            // Preserve string literals
            if (c == '\'' || c == '"')
            {
                char quote = c;
                sb.Append(c);
                i++;
                while (i < text.Length && text[i] != quote)
                {
                    sb.Append(text[i]);
                    i++;
                }
                if (i < text.Length)
                {
                    sb.Append(text[i]);
                    i++;
                }
                continue;
            }
            // Skip comment
            if (i + 1 < text.Length && text[i] == '(' && text[i + 1] == ':')
            {
                i += 2;
                int depth = 1;
                while (i < text.Length && depth > 0)
                {
                    if (i + 1 < text.Length && text[i] == ':' && text[i + 1] == ')')
                    {
                        depth--;
                        i += 2;
                    }
                    else if (i + 1 < text.Length && text[i] == '(' && text[i + 1] == ':')
                    {
                        depth++;
                        i += 2;
                    }
                    else
                    {
                        i++;
                    }
                }
                continue;
            }
            sb.Append(c);
            i++;
        }
        return sb.ToString();
    }
}
