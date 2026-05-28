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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text.RegularExpressions;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;

namespace Bosak.XPath.Xslt.Patterns;

/// <summary>
/// Compiles XSLT match patterns (e.g. <c>foo[bar]</c>, <c>*</c>, <c>@id | ref</c>)
/// into <c>Func&lt;IXdmNode, bool&gt;</c> predicates.
/// </summary>
public sealed class PatternCompiler
{
    private static readonly Regex UnionPattern = new(@"\s*\|\s*", RegexOptions.Compiled);

    /// <summary>
    /// Compiles a match pattern string into a predicate function.
    /// </summary>
    public Func<IXdmNode, bool> Compile(string pattern)
    {
        var branches = UnionPattern.Split(pattern.Trim());
        if (branches.Length == 1)
        {
            return CompileSinglePattern(branches[0]);
        }

        var compiledBranches = branches.Select(CompileSinglePattern).ToArray();
        return node => compiledBranches.Any(b => b(node));
    }

    private Func<IXdmNode, bool> CompileSinglePattern(string pattern)
    {
        var trimmed = pattern.Trim();

        // Handle // prefix (e.g. //foo, //foo[bar]) — matches any descendant
        if (trimmed.StartsWith("//"))
        {
            trimmed = trimmed[2..].Trim();
        }
        // Handle / prefix (e.g. /doc) — matches from the root
        else if (trimmed.StartsWith('/'))
        {
            trimmed = trimmed[1..].Trim();
        }

        // Document pattern: doc('uri') or document('uri') — XSLT 3.0
        if (trimmed.StartsWith("doc(") && trimmed.EndsWith(')'))
        {
            return node => node.NodeKind == XdmNodeKind.Document;
        }
        if (trimmed.StartsWith("document(") && trimmed.EndsWith(')'))
        {
            return node => node.NodeKind == XdmNodeKind.Document;
        }

        // Attribute pattern: @name or @*
        if (trimmed.StartsWith('@'))
        {
            var name = trimmed[1..].Trim();
            return CompileAttributePattern(name);
        }

        // Namespace test: Q{uri}local or prefix:local or *:local or prefix:*
        // For now, handle simple cases
        if (trimmed.Contains('['))
        {
            // Pattern with predicate: foo[bar]
            return CompilePredicatePattern(trimmed);
        }

        // Simple element name or wildcard
        return CompileElementPattern(trimmed);
    }

    private Func<IXdmNode, bool> CompileElementPattern(string name)
    {
        if (name == "*")
        {
            // Any element
            return node => node.NodeKind == XdmNodeKind.Element;
        }

        if (name == "node()" || name == ".")
        {
            return node => true; // Any node
        }

        if (name == "text()")
        {
            return node => node.NodeKind == XdmNodeKind.Text;
        }

        if (name == "comment()")
        {
            return node => node.NodeKind == XdmNodeKind.Comment;
        }

        if (name.StartsWith("processing-instruction"))
        {
            return node => node.NodeKind == XdmNodeKind.ProcessingInstruction;
        }

        // Qualified name: prefix:local or Q{uri}local
        var (ns, local) = ParseQName(name);

        if (string.IsNullOrEmpty(ns))
        {
            // No namespace: match by local name only
            return node =>
                node.NodeKind == XdmNodeKind.Element &&
                node.LocalName == local;
        }

        return node =>
            node.NodeKind == XdmNodeKind.Element &&
            node.NamespaceUri == ns &&
            node.LocalName == local;
    }

    private Func<IXdmNode, bool> CompileAttributePattern(string name)
    {
        if (name == "*")
        {
            // Any attribute
            return node => node.NodeKind == XdmNodeKind.Attribute;
        }

        var (ns, local) = ParseQName(name);

        if (string.IsNullOrEmpty(ns))
        {
            return node =>
                node.NodeKind == XdmNodeKind.Attribute &&
                node.LocalName == local;
        }

        return node =>
            node.NodeKind == XdmNodeKind.Attribute &&
            node.NamespaceUri == ns &&
            node.LocalName == local;
    }

    private Func<IXdmNode, bool> CompilePredicatePattern(string pattern)
    {
        // Extract the base pattern and the predicate expression
        // e.g. "foo[bar]" → base="foo", predicate="bar"
        int bracketOpen = pattern.IndexOf('[');
        if (bracketOpen < 0)
            return CompileSinglePattern(pattern);

        int bracketClose = pattern.LastIndexOf(']');
        if (bracketClose < 0 || bracketClose < bracketOpen)
            throw new InvalidOperationException($"Invalid pattern: {pattern}");

        string basePattern = pattern[..bracketOpen].Trim();
        string predicateExpr = pattern[(bracketOpen + 1)..bracketClose].Trim();

        var basePredicate = CompileSinglePattern(basePattern);

        // For simple element/attribute patterns with predicates, evaluate from the parent
        // so that position() and last() reflect the node's position among its siblings.
        bool isSimpleElement = !basePattern.Contains('/') && !basePattern.Contains('|') && !basePattern.StartsWith('@');
        bool isSimpleAttribute = !basePattern.Contains('/') && !basePattern.Contains('|') && basePattern.StartsWith('@');

        if (isSimpleElement || isSimpleAttribute)
        {
            var axisStep = isSimpleAttribute ? $"attribute::{basePattern[1..]}[{predicateExpr}]" : $"child::{basePattern}[{predicateExpr}]";
            var compiledStep = XPath31Expression.Compile(axisStep);

            return node =>
            {
                if (!basePredicate(node))
                    return false;

                var parent = node.Parent;
                if (parent == null)
                    return false;

                try
                {
                    var ctx = new EvaluationContext();
                    FunctionLibrary.Populate(ctx);
                    ctx.WithFocus(XdmValue.FromNode(parent), 1, 1);
                    var result = compiledStep.Evaluate(ctx);

                    // Check if the candidate node is in the result
                    if (result.Kind == XdmValueKind.Sequence && result.SequenceValue != null)
                    {
                        foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                        {
                            if (item.IsNode && item.NodeValue is IXdmNode n && n.IsSameNode(node))
                                return true;
                        }
                    }
                    else if (result.IsNode && result.NodeValue is IXdmNode n && n.IsSameNode(node))
                    {
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            };
        }

        // Fallback: compile as self:: pattern (position()/last() will be 1/1)
        var compiledPredicate = XPath31Expression.Compile($"self::{basePattern}[{predicateExpr}]");

        return node =>
        {
            if (!basePredicate(node))
                return false;

            try
            {
                var ctx = new EvaluationContext();
                FunctionLibrary.Populate(ctx);
                ctx.WithFocus(XdmValue.FromNode(node), 1, 1);
                var result = compiledPredicate.Evaluate(ctx);
                return result.EffectiveBooleanValue();
            }
            catch
            {
                return false;
            }
        };
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
            // prefix:local - we don't resolve prefixes here; return empty namespace
            // Full prefix resolution requires namespace context at compile time
            var local = name[(colon + 1)..];
            return (string.Empty, local);
        }

        return (string.Empty, name);
    }
}
