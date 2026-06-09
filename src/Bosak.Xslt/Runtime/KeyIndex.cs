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
//                      | Charles Korthout | 0.2   | 08-06-2026     | Added TotalEntryCount, ClearKey, BuildSingleKey for iterative cross-key builds          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.Xslt.Runtime;

/// <summary>
/// Maintains a per-document index for <c>key()</c> lookups based on <c>xsl:key</c> declarations.
/// </summary>
public sealed class KeyIndex
{
    // key name → key value string → list of nodes
    private readonly Dictionary<string, Dictionary<string, List<IXdmNode>>> _index = new();

    /// <summary>
    /// Returns the total number of (keyName, keyValue, node) entries in the index.
    /// </summary>
    public int TotalEntryCount
    {
        get
        {
            int count = 0;
            foreach (var keyDict in _index.Values)
                foreach (var nodes in keyDict.Values)
                    count += nodes.Count;
            return count;
        }
    }

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
    /// Removes all entries for the specified key name.
    /// </summary>
    public void ClearKey(string keyName)
    {
        if (_index.TryGetValue(keyName, out var keyDict))
        {
            keyDict.Clear();
        }
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
        => Build(sourceDocument, keyDefinitions, context, new KeyIndex());

    /// <summary>
    /// Builds the index into an existing <see cref="KeyIndex"/> instance, allowing
    /// recursive <c>key()</c> calls inside <c>xsl:key/@use</c> to access already-built keys.
    /// </summary>
    public static KeyIndex Build(IXdmNode sourceDocument, IEnumerable<Stylesheet.KeyDefinition> keyDefinitions, EvaluationContext context, KeyIndex index)
    {
        var patternCompiler = new Patterns.PatternCompiler();

        foreach (var keyDef in keyDefinitions)
        {
            var resolvedMatch = ResolveNamespacePrefixes(keyDef.Match, keyDef.Element);
            var compiledMatch = patternCompiler.Compile(resolvedMatch);
            var useExpr = XPath31Expression.Compile(keyDef.Use);

            // Walk the entire document tree to find matching nodes
            IndexNodes(sourceDocument, keyDef.Name, compiledMatch, useExpr, context, index);
        }

        return index;
    }

    /// <summary>
    /// Builds a single key definition into the given index, walking the document tree once.
    /// </summary>
    public static void BuildSingleKey(IXdmNode sourceDocument, Stylesheet.KeyDefinition keyDef, EvaluationContext context, KeyIndex index)
    {
        var patternCompiler = new Patterns.PatternCompiler();
        var resolvedMatch = ResolveNamespacePrefixes(keyDef.Match, keyDef.Element);
        var compiledMatch = patternCompiler.Compile(resolvedMatch);
        var useExpr = XPath31Expression.Compile(keyDef.Use);

        IndexNodes(sourceDocument, keyDef.Name, compiledMatch, useExpr, context, index);
    }

    private static void IndexNodes(IXdmNode node, string keyName, Patterns.PatternPredicate match, XPath31Expression useExpr, EvaluationContext context, KeyIndex index)
    {
        if (match(XdmValue.FromNode(node), context))
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

    /// <summary>
    /// Replaces prefix:local-name occurrences in a pattern with Q{uri}local-name,
    /// resolving prefixes using the namespace declarations in scope on the given element.
    /// Returns the pattern unchanged if contextElement is null.
    /// </summary>
    private static string ResolveNamespacePrefixes(string pattern, XElement? contextElement)
    {
        if (contextElement == null || !pattern.Contains(':'))
            return pattern;

        var sb = new System.Text.StringBuilder();
        int i = 0;
        while (i < pattern.Length)
        {
            char c = pattern[i];
            if (c == '\'' || c == '\"')
            {
                char quote = c;
                sb.Append(c);
                i++;
                while (i < pattern.Length && pattern[i] != quote)
                {
                    sb.Append(pattern[i]);
                    i++;
                }
                if (i < pattern.Length)
                {
                    sb.Append(pattern[i]);
                    i++;
                }
                continue;
            }
            if (c == 'Q' && i + 1 < pattern.Length && pattern[i + 1] == '{')
            {
                sb.Append(c);
                i++;
                continue;
            }
            if (char.IsLetter(c) || c == '_')
            {
                int start = i;
                while (i < pattern.Length && (char.IsLetterOrDigit(pattern[i]) || pattern[i] == '_' || pattern[i] == '-'))
                    i++;
                if (i < pattern.Length && pattern[i] == ':')
                {
                    if (i + 1 < pattern.Length && pattern[i + 1] == ':')
                    {
                        sb.Append(pattern[start..i]);
                        continue;
                    }
                    var prefix = pattern[start..i];
                    i++;
                    int localStart = i;
                    while (i < pattern.Length && (char.IsLetterOrDigit(pattern[i]) || pattern[i] == '_' || pattern[i] == '-' || pattern[i] == '.'))
                        i++;
                    var local = pattern[localStart..i];
                    var nsUri = contextElement.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";
                    if (!string.IsNullOrEmpty(nsUri))
                    {
                        sb.Append($"Q{{{nsUri}}}{local}");
                    }
                    else
                    {
                        sb.Append(prefix);
                        sb.Append(':');
                        sb.Append(local);
                    }
                }
                else
                {
                    sb.Append(pattern[start..i]);
                }
                continue;
            }
            sb.Append(c);
            i++;
        }
        return sb.ToString();
    }
}
