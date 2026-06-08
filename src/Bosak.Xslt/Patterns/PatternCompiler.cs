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
//                      | Charles Korthout | 0.8   | 01-06-2026     | Fix namespace wildcard patterns (prefix:* and Q{uri}*) in node tests                   |
//                      | Charles Korthout | 0.9   | 05-06-2026     | Static pattern validation (XTSE0340/XPST0017) for invalid constructs                   |
//                      | Charles Korthout | 1.0   | 05-06-2026     | Reject /[predicate] (XTSE0340); allow doc()/root() at pattern start                    |
//                      | Charles Korthout | 0.9   | 05-06-2026     | stepContextNode set from step-before-last, not first step; fixes match-125             |
//                      | Charles Korthout | 1.1   | 05-06-2026     | Added CompileAtomicMatch for .[expr] predicate patterns; fixes match-127/128/130       |
//                      | Charles Korthout | 1.2   | 07-06-2026     | CompileAtomicMatch: runtime numeric predicate check, whitespace/dot tolerance; +11 tests|
//                      | Charles Korthout | 1.3   | 07-06-2026     | attribute(*, type) comma-split in CompileNodeTest; fixes next-match-011                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text.RegularExpressions;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;

namespace Bosak.Xslt.Patterns;

/// <summary>
/// Signature for a compiled match pattern predicate.
/// Receives the candidate node and the current evaluation context (needed for variable reference patterns).
/// </summary>
public delegate bool PatternPredicate(XdmValue item, EvaluationContext context);

/// <summary>
/// Compiles XSLT match patterns (e.g. <c>foo[bar]</c>, <c>*</c>, <c>@id | ref</c>)
/// into <c>PatternPredicate</c> predicates.
/// </summary>
public sealed class PatternCompiler
{
    private static readonly Regex UnionPattern = new(@"\s*\|\s*", RegexOptions.Compiled);

    /// <summary>
    /// Extracts an IXdmNode from an XdmValue candidate, returning null if the value is not a node.
    /// </summary>
    private static IXdmNode? AsNode(XdmValue item)
        => item.IsNode ? item.NodeValue : null;

    /// <summary>
    /// Static validation for constructs that must be rejected at compile time.
    /// </summary>
    private static void ValidatePatternSyntax(string trimmed)
    {
        // 1.  Disallowed functions at pattern start (XTSE0340).
        //     Node tests (document-node, element, attribute, text, comment,
        //     processing-instruction, node, schema-element, schema-attribute,
        //     namespace-node) and the functions key() / id() are allowed.
        //     Any other function call at start or after '/' or '//' is an error.
        {
            var afterSlash = trimmed;
            if (afterSlash.StartsWith("//"))
                afterSlash = afterSlash[2..].TrimStart();
            else if (afterSlash.StartsWith("/"))
                afterSlash = afterSlash[1..].TrimStart();

            // Strip axis prefix (e.g., child::, attribute::, descendant::)
            var axisColon = afterSlash.IndexOf("::", StringComparison.Ordinal);
            if (axisColon > 0)
            {
                var axisName = afterSlash[..axisColon];
                if (IsAxisName(axisName))
                    afterSlash = afterSlash[(axisColon + 2)..];
            }

            // Match:  name(...) at the very start
            if (afterSlash.Length > 0 && char.IsLetter(afterSlash[0]))
            {
                var paren = afterSlash.IndexOf('(');
                if (paren > 0)
                {
                    var firstSpecial = afterSlash.IndexOfAny(new[] { ' ', '/', '|', '[', '@' });
                    if (firstSpecial < 0 || firstSpecial > paren)
                    {
                        var funcName = afterSlash[..paren];
                        var allowed = new HashSet<string>(StringComparer.Ordinal)
                        {
                            "document-node", "element", "attribute", "text",
                            "comment", "processing-instruction", "node",
                            "schema-element", "schema-attribute", "namespace-node",
                            "key", "id", "doc", "root"
                        };
                        if (!allowed.Contains(funcName))
                        {
                            throw new InvalidOperationException("XTSE0340: Function call not allowed at the start of a pattern.");
                        }
                    }
                }
            }
        }

        // 2.  key() second argument must be a literal string (XTSE0340).
        {
            var idx = 0;
            while ((idx = trimmed.IndexOf("key(", idx, StringComparison.Ordinal)) >= 0)
            {
                var close = FindMatchingParen(trimmed, idx + 3);
                if (close > 0)
                {
                    var args = trimmed[(idx + 4)..close];
                    var comma = FindTopLevelComma(args);
                    if (comma >= 0)
                    {
                        var secondArg = args[(comma + 1)..].Trim();
                        // Allow string literals and variable references; reject expressions.
                        if (!(secondArg.StartsWith('\'') || secondArg.StartsWith('"') || secondArg.StartsWith("$")))
                        {
                            throw new InvalidOperationException("XTSE0340: The second argument of key() in a pattern must be a literal string.");
                        }
                    }
                }
                idx += 4;
            }
        }

        // 3.  Invalid predicate patterns (XTSE0340).
        {
            // Parenthesized predicate pattern (.[...])
            if (trimmed.StartsWith("(.[") || trimmed.Contains("|(.["))
            {
                throw new InvalidOperationException("XTSE0340: Parenthesized predicate pattern is not a valid pattern.");
            }
            // Leading lone slash with predicate: /[expr] or //[expr]
            if (trimmed.StartsWith("/[") || trimmed.StartsWith("//["))
            {
                throw new InvalidOperationException("XTSE0340: Filtered leading lone slash is not a valid pattern.");
            }
        }

        // 4.  Union pattern constraints:
        //     a) Union of node pattern and type pattern is not allowed.
        //     b) Predicate patterns (.[...]) are not valid as union operands.
        {
            var branches = SplitTopLevel(trimmed, '|');
            if (branches.Length > 1)
            {
                bool hasNodePattern = false;
                bool hasTypePattern = false;
                bool hasPredicatePattern = false;
                foreach (var b in branches)
                {
                    var s = b.Trim();
                    if (LooksLikeTypePattern(s))
                        hasTypePattern = true;
                    else
                        hasNodePattern = true;
                    if (s.StartsWith(".["))
                        hasPredicatePattern = true;
                }
                if (hasNodePattern && hasTypePattern)
                {
                    throw new InvalidOperationException("XTSE0340: Union of node pattern and type pattern is not allowed.");
                }
                if (hasPredicatePattern)
                {
                    throw new InvalidOperationException("XTSE0340: Predicate pattern is not allowed in a union.");
                }
            }
        }

        // 5.  Undeclared function in predicate (XPST0017).
        //     NOTE: Accurate detection requires stylesheet context (declared functions,
        //     namespace prefixes). The XPath compiler currently does not validate
        //     function existence at compile time, so this check is deferred to runtime.
        //     When the XPath compiler gains static function resolution, this should be
        //     re-enabled with proper context.

    }

    /// <summary>
    /// Determines whether a pattern branch looks like a type pattern.
    /// </summary>
    private static bool LooksLikeTypePattern(string branch)
    {
        var s = branch.Trim();
        // .[. instance of xs:integer] or (.[. instance of xs:integer])
        if (s.StartsWith("(.[") || (s.StartsWith(".[") && s.Contains("instance of")))
            return true;
        // element(foo) or attribute(bar) etc.
        if (s.StartsWith("element(") || s.StartsWith("attribute(") ||
            s.StartsWith("text(") || s.StartsWith("comment(") ||
            s.StartsWith("processing-instruction(") || s.StartsWith("document-node(") ||
            s.StartsWith("node("))
        {
            // These are actually node tests, not type patterns.
            return false;
        }
        // type-name like xs:integer, but not node kinds
        // For simplicity, check if it matches the "instance of" form or starts with a type name.
        if (s.Contains("instance of"))
            return true;
        return false;
    }

    /// <summary>
    /// Checks whether a string is a valid XPath axis name.
    /// </summary>
    private static bool IsAxisName(string name)
    {
        return name == "child" || name == "descendant" || name == "attribute" ||
               name == "self" || name == "descendant-or-self" || name == "following-sibling" ||
               name == "following" || name == "namespace" || name == "parent" ||
               name == "ancestor" || name == "preceding-sibling" || name == "preceding" ||
               name == "ancestor-or-self";
    }

    /// <summary>
    /// Checks whether a namespace URI is a standard XPath / XSLT namespace.
    /// </summary>
    private static bool IsStandardNamespace(string ns)
    {
        return ns == "http://www.w3.org/2005/xpath-functions" ||
               ns == "http://www.w3.org/2005/xpath-functions/math" ||
               ns == "http://www.w3.org/2005/xpath-functions/map" ||
               ns == "http://www.w3.org/2005/xpath-functions/array" ||
               ns == "http://www.w3.org/2001/XMLSchema" ||
               ns == "http://www.w3.org/1999/XSL/Transform" ||
               ns == "http://www.w3.org/XML/1998/namespace";
    }

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

        ValidatePatternSyntax(trimmed);

        var branches = SplitTopLevel(trimmed, '|');
        if (branches.Length == 1)
        {
            var atomic = CompileAtomicMatch(branches[0]);
            if (atomic != null)
                return WrapWithCurrentItem(atomic);
            return WrapWithCurrentItem(CompileSinglePattern(branches[0]));
        }

        var compiledBranches = branches.Select(b =>
        {
            var atomic = CompileAtomicMatch(b);
            return atomic ?? CompileSinglePattern(b);
        }).ToArray();
        return WrapWithCurrentItem((item, ctx) => compiledBranches.Any(b => b(item, ctx)));
    }

    /// <summary>
    /// Compiles an atomic-value matcher for predicate patterns (.[expr]).
    /// Returns null if the pattern is not a predicate pattern that can match atomic values.
    /// </summary>
    public PatternPredicate? CompileAtomicMatch(string pattern)
    {
        var trimmed = StripXPathComments(pattern).Trim();

        // Strip outer parentheses
        if (trimmed.StartsWith('('))
        {
            int close = FindMatchingParen(trimmed, 0);
            if (close == trimmed.Length - 1)
                trimmed = trimmed[1..close].Trim();
        }

        // Only predicate patterns can match atomic values
        // Tolerate whitespace between '.' and '[' (e.g. ". [expr]" after comment stripping)
        if (!trimmed.StartsWith("."))
            return null;
        int afterDot = 1;
        while (afterDot < trimmed.Length && char.IsWhiteSpace(trimmed[afterDot]))
            afterDot++;
        if (afterDot >= trimmed.Length || trimmed[afterDot] != '[')
            return null;

        // Extract all predicates after the leading '.'
        var predicates = new List<string>();
        string rest = trimmed[1..].TrimStart();
        while (rest.StartsWith('['))
        {
            int close = FindMatchingBracket(rest, 0);
            if (close < 0)
                return null;
            predicates.Add(rest[1..close].Trim());
            rest = rest[(close + 1)..].TrimStart();
        }

        if (predicates.Count == 0)
            return null;

        // If there's anything after the predicates (e.g. /child::x), atomic values
        // cannot match, so let the normal node-based path handle it.
        if (!string.IsNullOrEmpty(rest))
            return null;

        // XSLT 3.0 §6.4: numeric predicate 1 always matches; any other numeric value never matches.
        // Pre-compile all predicates; apply numeric semantics at runtime for variable expressions.
        var compiledPredicates = new List<(string expr, bool isLiteralOne, bool isLiteralNever)>();
        foreach (var pred in predicates)
        {
            var stripped = StripXPathComments(pred).Trim();
            if (int.TryParse(stripped, out int n))
            {
                if (n != 1)
                    compiledPredicates.Add((pred, false, true)); // literal != 1: never matches
                else
                    compiledPredicates.Add((pred, true, false)); // literal 1: no-op
                continue;
            }
            compiledPredicates.Add((pred, false, false)); // non-literal: evaluate at runtime
        }

        // Pre-compile XPath expressions for non-literal predicates
        var predCompilers = compiledPredicates.Select(p => XPath31Expression.Compile(p.expr)).ToList();

        return (item, ctx) =>
        {
            var savedItem = ctx.ContextItem;
            var savedPos = ctx.ContextPosition;
            var savedSize = ctx.ContextSize;
            try
            {
                for (int i = 0; i < compiledPredicates.Count; i++)
                {
                    var (expr, isLiteralOne, isLiteralNever) = compiledPredicates[i];
                    if (isLiteralNever)
                        return false;
                    if (isLiteralOne)
                        continue;

                    var result = predCompilers[i].Evaluate(ctx.WithFocus(item, 1, 1));

                    // XSLT 3.0 §6.4: numeric predicate 1 always matches; any other numeric value never matches.
                    if (result.Kind == XdmValueKind.Integer)
                    {
                        if (result.IntegerValue != 1)
                            return false;
                        continue; // 1 is a no-op
                    }
                    if (result.Kind == XdmValueKind.Decimal)
                    {
                        if (result.DecimalValue != 1m)
                            return false;
                        continue;
                    }
                    if (result.Kind == XdmValueKind.Double || result.Kind == XdmValueKind.Float)
                    {
                        if (result.DoubleValue != 1.0)
                            return false;
                        continue;
                    }

                    if (!result.EffectiveBooleanValue())
                        return false;
                }
                return true;
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
    /// Wraps a compiled pattern so that <c>fn:current()</c> returns the candidate node
    /// being tested, as required by XSLT match-pattern semantics.
    /// </summary>
    private static PatternPredicate WrapWithCurrentItem(PatternPredicate inner)
    {
        return (item, ctx) =>
        {
            var saved = ctx.CurrentItem;
            try
            {
                ctx.WithCurrentItem(item);
                return inner(item, ctx);
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
            return (item, ctx) => left(item, ctx) && !right(item, ctx);
        }

        // Handle top-level intersect
        var intersectParts = SplitTopLevel(trimmed, "intersect");
        if (intersectParts.Length == 2)
        {
            var left = CompileSinglePattern(intersectParts[0]);
            var right = CompileSinglePattern(intersectParts[1]);
            return (item, ctx) => left(item, ctx) && right(item, ctx);
        }

        // Handle // prefix (e.g. //foo, //foo[bar]) — matches any descendant
        // Per XSLT spec, //P only matches nodes in a tree rooted at a document node.
        if (trimmed.StartsWith("//"))
        {
            var innerPattern = CompileSinglePattern(trimmed[2..].Trim());
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                // Walk to the root; //P only matches if root is a document node
                var root = node;
                while (root.Parent != null)
                    root = root.Parent;
                if (root.NodeKind != XdmNodeKind.Document)
                    return false;
                return innerPattern(item, ctx);
            };
        }
        // Handle / prefix (e.g. /doc) — matches from the root
        else if (trimmed.StartsWith('/'))
        {
            trimmed = trimmed[1..].Trim();
            if (string.IsNullOrEmpty(trimmed))
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    return node.NodeKind == XdmNodeKind.Document;
                };

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
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return inner(item, ctx) && node.Parent?.NodeKind == XdmNodeKind.Document;
            };
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
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Document;
            };
        }
        if (trimmed.StartsWith("document(") && trimmed.EndsWith(')'))
        {
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Document;
            };
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
                    return CompileParenthesizedWithPredicates(innerPattern, inside, after);
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

            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                try
                {
                    var varResult = compiledVarCheck.Evaluate(ctx);
                    if (!varResult.IsSequence && !varResult.IsNode)
                        return false;

                    // Check if node matches the path pattern
                    if (!pathPattern(item, ctx))
                        return false;

                    // Check if any node in the variable is an ancestor (or self) of node
                    if (varResult.IsSequence && varResult.SequenceValue != null)
                    {
                        foreach (var seqItem in XdmSequence.FromSource(varResult.SequenceValue))
                        {
                            if (!seqItem.IsNode || seqItem.NodeValue is not IXdmNode vn)
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
        return (item, ctx) =>
        {
            var node = AsNode(item);
            if (node == null) return false;
            try
            {
                var result = compiledVar.Evaluate(ctx);
                if (result.IsSequence && result.SequenceValue != null)
                {
                    foreach (var seqItem in XdmSequence.FromSource(result.SequenceValue))
                    {
                        if (seqItem.IsNode && seqItem.NodeValue is IXdmNode n && n.IsSameNode(node))
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
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    var current = node;
                    while (current.Parent != null) current = current.Parent;
                    return current.IsSameNode(node);
                };
            }
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Document;
            };
        }

        string docPart = pattern[..slashIdx].Trim();
        string pathPart = pattern[slashIdx..].Trim();

        // Compile the full path as an XPath expression: (docPart)pathPart
        // This correctly handles multi-step paths, predicates, and descendant axes.
        var fullPathCompiled = XPath31Expression.Compile($"({docPart}){pathPart}");

        return (item, ctx) =>
        {
            var node = AsNode(item);
            if (node == null) return false;
            var savedItem = ctx.ContextItem;
            var savedPos = ctx.ContextPosition;
            var savedSize = ctx.ContextSize;
            try
            {
                var result = fullPathCompiled.Evaluate(ctx.WithFocus(XdmValue.FromNode(node), 1, 1));
                if (result.Kind == XdmValueKind.Sequence && result.SequenceValue != null)
                {
                    foreach (var seqItem in XdmSequence.FromSource(result.SequenceValue))
                    {
                        if (seqItem.IsNode && seqItem.NodeValue is IXdmNode n && n.IsSameNode(node))
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
            return (item, ctx) => false;

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

        return (item, ctx) =>
        {
            var node = AsNode(item);
            if (node == null) return false;
            // For simple or non-predicated last steps, check before ancestor walk
            if (lastStepPredicate != null && !lastStepPredicate(item, ctx))
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
                    if (current == null || !test(XdmValue.FromNode(current), ctx))
                        return false;
                    if (s == steps.Count - 2)
                        stepContextNode = current;
                    current = current.Parent;
                }
                else
                {
                    bool found = false;
                    while (current != null)
                    {
                        if (test(XdmValue.FromNode(current), ctx))
                        {
                            found = true;
                            if (s == steps.Count - 2)
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
                        foreach (var seqItem in XdmSequence.FromSource(result.SequenceValue))
                        {
                            if (seqItem.IsNode && seqItem.NodeValue is IXdmNode n && n.IsSameNode(node))
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
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Element;
            };
        }

        if (name.StartsWith("*:"))
        {
            string local = name[2..];
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Element && node.LocalName == local;
            };
        }

        if (name == "node()" || name == ".")
        {
            return (item, ctx) => true;
        }

        if (name == "text()")
        {
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Text;
            };
        }

        if (name == "comment()")
        {
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Comment;
            };
        }

        if (name == "processing-instruction()")
        {
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.ProcessingInstruction;
            };
        }

        if (name.StartsWith("processing-instruction("))
        {
            // processing-instruction(name) or processing-instruction('name')
            var piName = ExtractFunctionArg(name);
            if (string.IsNullOrEmpty(piName))
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    return node.NodeKind == XdmNodeKind.ProcessingInstruction;
                };
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.ProcessingInstruction &&
                node.LocalName == piName.Trim('\'');
            };
        }

        if (name == "document-node()")
        {
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Document;
            };
        }

        if (name == "root()")
        {
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                var current = node;
                while (current.Parent != null) current = current.Parent;
                return current.IsSameNode(node);
            };
        }

        if (name == "element()")
        {
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Element;
            };
        }

        if (name.StartsWith("element("))
        {
            var arg = ExtractFunctionArg(name);
            // element(name) or element(QName); may include a type argument: element(name, type)
            var nameArg = string.IsNullOrEmpty(arg) ? "" : arg.Split(',')[0].Trim();
            if (string.IsNullOrEmpty(nameArg) || nameArg == "*")
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    return node.NodeKind == XdmNodeKind.Element;
                };
            var (ns, local) = ParseQName(nameArg);
            if (string.IsNullOrEmpty(ns))
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    return node.NodeKind == XdmNodeKind.Element && node.LocalName == local;
                };
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Element && node.NamespaceUri == ns && node.LocalName == local;
            };
        }

        if (name == "attribute()")
        {
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Attribute;
            };
        }

        if (name.StartsWith("attribute("))
        {
            var arg = ExtractFunctionArg(name);
            // attribute(name) or attribute(QName); may include a type argument: attribute(name, type)
            var nameArg = string.IsNullOrEmpty(arg) ? "" : arg.Split(',')[0].Trim();
            if (string.IsNullOrEmpty(nameArg) || nameArg == "*")
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    return node.NodeKind == XdmNodeKind.Attribute;
                };
            var (ns, local) = ParseQName(nameArg);
            if (string.IsNullOrEmpty(ns))
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    return node.NodeKind == XdmNodeKind.Attribute && node.LocalName == local;
                };
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Attribute && node.NamespaceUri == ns && node.LocalName == local;
            };
        }

        // id('x', $y) pattern — id() may return multiple nodes; check membership.
        if (name.StartsWith("id(") && name.EndsWith(')'))
        {
            var compiledId = XPath31Expression.Compile(name);
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                try
                {
                    var result = compiledId.Evaluate(ctx.WithFocus(XdmValue.FromNode(node), 1, 1));
                    if (result.IsSequence && result.SequenceValue != null)
                    {
                        foreach (var seqItem in XdmSequence.FromSource(result.SequenceValue))
                        {
                            if (seqItem.IsNode && seqItem.NodeValue is IXdmNode n && n.IsSameNode(node))
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

        // key('k', 'v') pattern — key() may return multiple nodes; check membership.
        if (name.StartsWith("key(") && name.EndsWith(')'))
        {
            var compiledKey = XPath31Expression.Compile(name);
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                try
                {
                    var result = compiledKey.Evaluate(ctx.WithFocus(XdmValue.FromNode(node), 1, 1));
                    if (result.IsSequence && result.SequenceValue != null)
                    {
                        foreach (var seqItem in XdmSequence.FromSource(result.SequenceValue))
                        {
                            if (seqItem.IsNode && seqItem.NodeValue is IXdmNode n && n.IsSameNode(node))
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

        // Qualified name: prefix:local or Q{uri}local
        var (nsUri, localName) = ParseQName(name);

        if (string.IsNullOrEmpty(nsUri))
        {
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Element &&
                node.LocalName == localName &&
                node.NamespaceUri == "";
            };
        }

        if (localName == "*")
        {
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Element &&
                node.NamespaceUri == nsUri;
            };
        }

        return (item, ctx) =>
        {
            var node = AsNode(item);
            if (node == null) return false;
            return node.NodeKind == XdmNodeKind.Element &&
            node.NamespaceUri == nsUri &&
            node.LocalName == localName;
        };
    }

    private PatternPredicate CompileAxisStep(string axis, string nodeTest)
    {
        axis = axis.ToLowerInvariant();

        switch (axis)
        {
            case "self":
                var selfTest = CompileNodeTestPredicate(nodeTest);
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    return selfTest(node);
                };

            case "child":
                var childTest = CompileNodeTestPredicate(nodeTest);
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    return node.NodeKind != XdmNodeKind.Document
                    && node.NodeKind != XdmNodeKind.Attribute
                    && node.NodeKind != XdmNodeKind.Namespace
                    && childTest(node);
                };

            case "descendant":
                // In a pattern step, descendant::foo matches foo that has an ancestor
                // that matches the preceding step. Since this is used as a step in a path,
                // we just test the node against the node test.
                var descTest = CompileNodeTestPredicate(nodeTest);
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    return descTest(node);
                };

            case "descendant-or-self":
                var dosTest = CompileNodeTestPredicate(nodeTest);
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    return dosTest(node);
                };

            case "attribute":
                var attrTest = CompileAttributeNodeTest(nodeTest);
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    return attrTest(node);
                };

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
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    return fallbackTest(node);
                };

            case "namespace":
                if (nodeTest == "*")
                    return (item, ctx) =>
                    {
                        var node = AsNode(item);
                        if (node == null) return false;
                        return node.NodeKind == XdmNodeKind.Namespace;
                    };
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    return node.NodeKind == XdmNodeKind.Namespace && node.LocalName == nodeTest;
                };

            default:
                var defaultTest = CompileNodeTestPredicate(nodeTest);
                return (item, ctx) =>
                {
                    var node = AsNode(item);
                    if (node == null) return false;
                    return defaultTest(node);
                };
        }
    }

    private Func<IXdmNode, bool> CompileNodeTestPredicate(string nodeTest)
    {
        if (nodeTest == "*")
            return node => node.NodeKind == XdmNodeKind.Element;
        if (nodeTest.StartsWith("*:"))
        {
            string local = nodeTest[2..];
            return node => node.NodeKind == XdmNodeKind.Element && node.LocalName == local;
        }
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
            var nameArg = string.IsNullOrEmpty(arg) ? "" : arg.Split(',')[0].Trim();
            if (string.IsNullOrEmpty(nameArg) || nameArg == "*")
                return node => node.NodeKind == XdmNodeKind.Element;
            var (ns, local) = ParseQName(nameArg);
            if (string.IsNullOrEmpty(ns))
                return node => node.NodeKind == XdmNodeKind.Element && node.LocalName == local;
            return node => node.NodeKind == XdmNodeKind.Element && node.NamespaceUri == ns && node.LocalName == local;
        }

        var (nsUri, localName) = ParseQName(nodeTest);
        if (string.IsNullOrEmpty(nsUri))
            return node => node.NodeKind == XdmNodeKind.Element && node.LocalName == localName && node.NamespaceUri == "";
        if (localName == "*")
            return node => node.NodeKind == XdmNodeKind.Element && node.NamespaceUri == nsUri;
        return node => node.NodeKind == XdmNodeKind.Element && node.NamespaceUri == nsUri && node.LocalName == localName;
    }

    private Func<IXdmNode, bool> CompileAttributeNodeTest(string nodeTest)
    {
        if (nodeTest == "*")
            return node => node.NodeKind == XdmNodeKind.Attribute;
        if (nodeTest.StartsWith("*:"))
        {
            string local = nodeTest[2..];
            return node => node.NodeKind == XdmNodeKind.Attribute && node.LocalName == local;
        }
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
            return node => node.NodeKind == XdmNodeKind.Attribute && node.LocalName == localName && node.NamespaceUri == "";
        if (localName == "*")
            return node => node.NodeKind == XdmNodeKind.Attribute && node.NamespaceUri == nsUri;
        return node => node.NodeKind == XdmNodeKind.Attribute && node.NamespaceUri == nsUri && node.LocalName == localName;
    }

    private PatternPredicate CompileAttributePattern(string name)
    {
        if (name == "*")
        {
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Attribute;
            };
        }

        if (name.StartsWith("*:"))
        {
            string localName2 = name[2..];
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Attribute && node.LocalName == localName2;
            };
        }

        // namespace::* 
        if (name == "namespace::*")
        {
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Namespace;
            };
        }

        var (ns, local) = ParseQName(name);

        if (string.IsNullOrEmpty(ns))
        {
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Attribute &&
                node.LocalName == local &&
                node.NamespaceUri == "";
            };
        }

        if (local == "*")
        {
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                return node.NodeKind == XdmNodeKind.Attribute &&
                node.NamespaceUri == ns;
            };
        }

        return (item, ctx) =>
        {
            var node = AsNode(item);
            if (node == null) return false;
            return node.NodeKind == XdmNodeKind.Attribute &&
            node.NamespaceUri == ns &&
            node.LocalName == local;
        };
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

            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
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
            string axisStep;
            if (basePattern == "attribute()" || basePattern.StartsWith("attribute("))
                axisStep = $"attribute::{basePattern}[{predicateExpr}]{remaining}";
            else if (basePattern == "namespace-node()" || basePattern.StartsWith("namespace-node("))
                axisStep = $"namespace::{basePattern}[{predicateExpr}]{remaining}";
            else if (isSimpleAttribute)
                axisStep = $"attribute::{basePattern[1..]}[{predicateExpr}]{remaining}";
            else
                axisStep = $"child::{basePattern}[{predicateExpr}]{remaining}";
            var compiledStep = XPath31Expression.Compile(axisStep);
            var fallbackPred = XPath31Expression.Compile($"self::node()[{predicateExpr}]{remaining}");

            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                if (!basePredicate(item, ctx))
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
                        foreach (var seqItem in XdmSequence.FromSource(result.SequenceValue))
                        {
                            if (seqItem.IsNode && seqItem.NodeValue is IXdmNode n && n.IsSameNode(node))
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

        return (item, ctx) =>
        {
            var node = AsNode(item);
            if (node == null) return false;
            if (!basePredicate(item, ctx))
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

    private PatternPredicate CompileParenthesizedWithPredicates(PatternPredicate innerPattern, string inside, string after)
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

        // If the inner pattern is a path expression (contains /), the predicate applies
        // to the sequence selected by the path, not to self::node(). For example,
        // (doc/descendant::foo)[2] must match the 2nd foo descendant of doc.
        bool isPathExpression = inside.Contains('/');
        if (isPathExpression)
        {
            var compiledPath = XPath31Expression.Compile($"({inside})[{predicateExpr}]{remaining}");
            return (item, ctx) =>
            {
                var node = AsNode(item);
                if (node == null) return false;
                try
                {
                    var root = node;
                    while (root.Parent != null)
                        root = root.Parent;
                    var result = compiledPath.Evaluate(ctx.WithFocus(XdmValue.FromNode(root), 1, 1));
                    if (result.Kind == XdmValueKind.Sequence && result.SequenceValue != null)
                    {
                        foreach (var seqItem in XdmSequence.FromSource(result.SequenceValue))
                        {
                            if (seqItem.IsNode && seqItem.NodeValue is IXdmNode n && n.IsSameNode(node))
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

        var compiledPred = XPath31Expression.Compile($"self::node()[{predicateExpr}]{remaining}");

        return (item, ctx) =>
        {
            var node = AsNode(item);
            if (node == null) return false;
            if (!innerPattern(item, ctx))
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
    /// Finds the index of the matching closing bracket for the bracket at startIndex.
    /// Returns -1 if not found.
    /// </summary>
    private static int FindMatchingBracket(string text, int startIndex)
    {
        if (text[startIndex] != '[') return -1;
        int depth = 1;
        for (int i = startIndex + 1; i < text.Length; i++)
        {
            if (text[i] == '[') depth++;
            else if (text[i] == ']') depth--;
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

    /// <summary>
    /// Finds the first comma at top level (not inside parentheses or brackets).
    /// Returns -1 if not found.
    /// </summary>
    private static int FindTopLevelComma(string text)
    {
        int depth = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '(' || c == '[') depth++;
            else if (c == ')' || c == ']') depth--;
            else if (c == ',' && depth == 0)
                return i;
        }
        return -1;
    }
}
