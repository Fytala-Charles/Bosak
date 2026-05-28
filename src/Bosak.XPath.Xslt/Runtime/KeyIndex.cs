// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 mei 2026
// PURPOSE              : Per-document index for xsl:key / key() lookups.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 24-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Xslt.Runtime;

/// <summary>
/// Maintains a per-document index for <c>key()</c> lookups based on <c>xsl:key</c> declarations.
/// </summary>
public sealed class KeyIndex
{
    // key name → key value string → list of nodes
    private readonly Dictionary<string, Dictionary<string, List<IXdmNode>>> _index = new();

    /// <summary>
    /// Adds a node under the given key name and key value.
    /// </summary>
    public void Add(string keyName, string keyValue, IXdmNode node)
    {
        if (!_index.TryGetValue(keyName, out var keyDict))
        {
            keyDict = new Dictionary<string, List<IXdmNode>>();
            _index[keyName] = keyDict;
        }

        if (!keyDict.TryGetValue(keyValue, out var nodes))
        {
            nodes = new List<IXdmNode>();
            keyDict[keyValue] = nodes;
        }

        nodes.Add(node);
    }

    /// <summary>
    /// Looks up nodes by key name and key value.
    /// </summary>
    public IEnumerable<IXdmNode> Lookup(string keyName, string keyValue)
    {
        if (_index.TryGetValue(keyName, out var keyDict))
        {
            if (keyDict.TryGetValue(keyValue, out var nodes))
            {
                return nodes;
            }
        }
        return Enumerable.Empty<IXdmNode>();
    }

    /// <summary>
    /// Builds the index for a source document against the given stylesheet key definitions.
    /// </summary>
    public static KeyIndex Build(IXdmNode sourceDocument, IEnumerable<Stylesheet.KeyDefinition> keyDefinitions, EvaluationContext context)
    {
        var index = new KeyIndex();
        var patternCompiler = new Patterns.PatternCompiler();

        foreach (var keyDef in keyDefinitions)
        {
            var compiledMatch = patternCompiler.Compile(keyDef.Match);
            var useExpr = XPath31Expression.Compile(keyDef.Use);

            // Walk the entire document tree to find matching nodes
            IndexNodes(sourceDocument, keyDef.Name, compiledMatch, useExpr, context, index);
        }

        return index;
    }

    private static void IndexNodes(IXdmNode node, string keyName, Patterns.PatternPredicate match, XPath31Expression useExpr, EvaluationContext context, KeyIndex index)
    {
        if (match(node, context))
        {
            var keyValues = useExpr.Evaluate(context.WithFocus(XdmValue.FromNode(node), 1, 1));
            foreach (var keyValue in ExtractKeyValues(keyValues))
            {
                index.Add(keyName, keyValue, node);
            }
        }

        foreach (var child in node.Axis(XdmAxis.Child))
        {
            if (child.NodeValue != null)
                IndexNodes(child.NodeValue, keyName, match, useExpr, context, index);
        }
    }

    private static IEnumerable<string> ExtractKeyValues(XdmValue value)
    {
        if (value.IsUndefined)
            yield break;

        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (!item.IsUndefined)
                    yield return item.ToString();
            }
        }
        else
        {
            yield return value.ToString();
        }
    }
}
