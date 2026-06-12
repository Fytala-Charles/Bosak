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
//                      | Charles Korthout | 0.3   | 11-06-2026     | Expanded key names, default namespace, globals before build, no implicit clear           |
//                      | Charles Korthout | 0.4   | 11-06-2026     | Store typed key values; attribute nodes indexed; dedupe entries                         |
//                      | Charles Korthout | 0.5   | 11-06-2026     | Document-order lookup results; composite key support                                     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Globalization;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.Xslt.Runtime;

/// <summary>
/// Maintains a per-document index for <c>xsl:key</c> / <c>key()</c> lookups.
/// </summary>
public sealed class KeyIndex
{
    private readonly record struct KeyEntry(bool IsComposite, XdmValue[] KeyValues, IXdmNode Node);

    // key name → entries
    private readonly Dictionary<string, List<KeyEntry>> _index = new();

    /// <summary>
    /// Returns the total number of (keyName, keyValue, node) entries in the index.
    /// </summary>
    public int TotalEntryCount
    {
        get
        {
            int count = 0;
            foreach (var entries in _index.Values)
                count += entries.Count;
            return count;
        }
    }

    /// <summary>
    /// Adds a node under the given key name with a single typed key value.
    /// </summary>
    public void Add(string keyName, XdmValue keyValue, IXdmNode node)
    {
        Add(keyName, false, [keyValue], node);
    }

    /// <summary>
    /// Adds a node under the given key name with a composite (sequence) key value.
    /// </summary>
    public void AddComposite(string keyName, XdmValue[] keyValues, IXdmNode node)
    {
        Add(keyName, true, keyValues, node);
    }

    private void Add(string keyName, bool isComposite, XdmValue[] keyValues, IXdmNode node)
    {
        if (keyValues.Length == 0)
            return;

        if (!_index.TryGetValue(keyName, out var entries))
        {
            entries = new List<KeyEntry>();
            _index[keyName] = entries;
        }

        foreach (var entry in entries)
        {
            if (entry.IsComposite != isComposite)
                continue;
            if (!entry.Node.IsSameNode(node))
                continue;
            if (entry.KeyValues.Length != keyValues.Length)
                continue;
            bool same = true;
            for (int i = 0; i < keyValues.Length; i++)
            {
                if (!KeyValuesEqual(entry.KeyValues[i], keyValues[i]))
                {
                    same = false;
                    break;
                }
            }
            if (same)
                return;
        }

        entries.Add(new KeyEntry(isComposite, keyValues, node));
    }

    /// <summary>
    /// Removes all entries for the specified key name.
    /// </summary>
    public void ClearKey(string keyName)
    {
        if (_index.TryGetValue(keyName, out var entries))
        {
            entries.Clear();
        }
    }

    /// <summary>
    /// Looks up nodes by key name and typed key value using XPath <c>eq</c>-style comparison.
    /// Results are returned in document order and deduplicated.
    /// </summary>
    public IEnumerable<IXdmNode> Lookup(string keyName, XdmValue keyValue)
    {
        if (!_index.TryGetValue(keyName, out var entries))
            yield break;

        var matches = new List<KeyEntry>();
        foreach (var entry in entries)
        {
            if (entry.IsComposite)
                continue;
            if (KeyValuesEqual(entry.KeyValues[0], keyValue))
                matches.Add(entry);
        }

        matches.Sort((a, b) => a.Node.DocumentOrder.CompareTo(b.Node.DocumentOrder));
        var seen = new HashSet<IXdmNode>();
        foreach (var entry in matches)
        {
            if (seen.Add(entry.Node))
                yield return entry.Node;
        }
    }

    /// <summary>
    /// Looks up nodes by key name and a composite key tuple.
    /// Results are returned in document order and deduplicated.
    /// </summary>
    public IEnumerable<IXdmNode> LookupComposite(string keyName, XdmValue[] keyValues)
    {
        if (keyValues.Length == 0)
            yield break;

        if (!_index.TryGetValue(keyName, out var entries))
            yield break;

        var matches = new List<KeyEntry>();
        foreach (var entry in entries)
        {
            if (!entry.IsComposite)
                continue;
            if (entry.KeyValues.Length != keyValues.Length)
                continue;
            bool same = true;
            for (int i = 0; i < keyValues.Length; i++)
            {
                if (!KeyValuesEqual(entry.KeyValues[i], keyValues[i]))
                {
                    same = false;
                    break;
                }
            }
            if (same)
                matches.Add(entry);
        }

        matches.Sort((a, b) => a.Node.DocumentOrder.CompareTo(b.Node.DocumentOrder));
        var seen = new HashSet<IXdmNode>();
        foreach (var entry in matches)
        {
            if (seen.Add(entry.Node))
                yield return entry.Node;
        }
    }

    /// <summary>
    /// Compares two key values using the same rules as the XPath <c>eq</c> operator,
    /// including untyped-atomic casting and numeric promotion.
    /// </summary>
    private static bool KeyValuesEqual(XdmValue a, XdmValue b)
    {
        if (a.IsUndefined || b.IsUndefined)
            return false;

        var aKind = a.Kind;
        var bKind = b.Kind;

        // Both numeric: compare numeric values.
        if (IsNumeric(aKind) && IsNumeric(bKind))
        {
            double da = aKind switch
            {
                XdmValueKind.Integer => a.IntegerValue,
                XdmValueKind.Decimal => (double)a.DecimalValue,
                _ => a.DoubleValue
            };
            double db = bKind switch
            {
                XdmValueKind.Integer => b.IntegerValue,
                XdmValueKind.Decimal => (double)b.DecimalValue,
                _ => b.DoubleValue
            };
            if (double.IsNaN(da) || double.IsNaN(db))
                return false;
            return da == db;
        }

        // Same kind exact comparison.
        if (aKind == bKind)
        {
            switch (aKind)
            {
                case XdmValueKind.String:
                    return string.Equals(a.ToString(), b.ToString(), StringComparison.Ordinal);
                case XdmValueKind.Boolean:
                    return a.ToString() == b.ToString();
                case XdmValueKind.DateTime:
                case XdmValueKind.Date:
                case XdmValueKind.Time:
                    return NormalizeDateTime(a, aKind) == NormalizeDateTime(b, bKind);
                case XdmValueKind.Duration:
                    return string.Equals(a.ToString(), b.ToString(), StringComparison.Ordinal);
                case XdmValueKind.QName:
                    var qa = a.QNameValue;
                    var qb = b.QNameValue;
                    return qa.LocalName == qb.LocalName && qa.NamespaceUri == qb.NamespaceUri;
                case XdmValueKind.Uri:
                    return string.Equals(a.ToString(), b.ToString(), StringComparison.Ordinal);
            }
        }

        // untypedAtomic on either side: cast to the other operand's type.
        if (IsUntypedAtomic(a))
            return UntypedAtomicEqualsOther(a, b);
        if (IsUntypedAtomic(b))
            return UntypedAtomicEqualsOther(b, a);

        // String / URI cross-comparison.
        if ((aKind == XdmValueKind.String || aKind == XdmValueKind.Uri) &&
            (bKind == XdmValueKind.String || bKind == XdmValueKind.Uri))
        {
            return string.Equals(a.ToString(), b.ToString(), StringComparison.Ordinal);
        }

        return false;
    }

    private static bool IsNumeric(XdmValueKind kind)
        => kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float;

    private static bool IsUntypedAtomic(XdmValue value)
        => value.Kind == XdmValueKind.String &&
           string.Equals(value.SchemaTypeName, "untypedAtomic", StringComparison.OrdinalIgnoreCase);

    private static DateTimeOffset NormalizeDateTime(XdmValue value, XdmValueKind kind)
    {
        var dt = kind switch
        {
            XdmValueKind.DateTime => value.DateTimeValue,
            XdmValueKind.Date => value.DateValue,
            XdmValueKind.Time => value.TimeValue,
            _ => throw new InvalidOperationException()
        };
        return dt.ToUniversalTime();
    }

    private static bool UntypedAtomicEqualsOther(XdmValue untyped, XdmValue other)
    {
        // XSLT key() treats xs:untypedAtomic values as strings for comparison purposes:
        // they match string lookups but do not cast to numeric, boolean, or date/time types.
        var s = untyped.ToString();
        if (other.Kind is XdmValueKind.String or XdmValueKind.Uri)
            return string.Equals(s, other.ToString(), StringComparison.Ordinal);
        return false;
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
            BuildSingleKey(sourceDocument, keyDef, context, index, patternCompiler);
        }

        return index;
    }

    /// <summary>
    /// Builds a single key definition into the given index, walking the document tree once.
    /// </summary>
    public static void BuildSingleKey(IXdmNode sourceDocument, Stylesheet.KeyDefinition keyDef, EvaluationContext context, KeyIndex index)
    {
        var patternCompiler = new Patterns.PatternCompiler();
        BuildSingleKey(sourceDocument, keyDef, context, index, patternCompiler);
    }

    private static void BuildSingleKey(IXdmNode sourceDocument, Stylesheet.KeyDefinition keyDef, EvaluationContext context, KeyIndex index, Patterns.PatternCompiler patternCompiler)
    {
        var defaultNs = Bosak.Xslt.Stylesheet.Stylesheet.GetXPathDefaultNamespace(keyDef.Element!);
        var resolvedMatch = ResolveNamespacePrefixes(keyDef.Match, keyDef.Element);
        var compiledMatch = patternCompiler.Compile(resolvedMatch, defaultNs);
        var useExpr = string.IsNullOrEmpty(keyDef.Use)
            ? null
            : CompileUseExpression(keyDef.Use, defaultNs);

        IndexNodes(sourceDocument, keyDef.Name, compiledMatch, useExpr, keyDef.Composite, context, index);
    }

    private static XPath31Expression CompileUseExpression(string use, string? defaultElementNamespace)
    {
        if (string.IsNullOrEmpty(defaultElementNamespace))
            return XPath31Expression.Compile(use);
        var options = new CompileOptions { DefaultElementNamespace = defaultElementNamespace };
        return XPath31Expression.Compile(use, options);
    }

    /// <summary>
    /// Builds a single key definition whose use value is supplied by a callback rather than @use.
    /// </summary>
    public static void BuildSingleKey(IXdmNode sourceDocument, Stylesheet.KeyDefinition keyDef, EvaluationContext context, KeyIndex index, Func<IXdmNode, XdmValue> useEvaluator)
    {
        var patternCompiler = new Patterns.PatternCompiler();
        var defaultNs = Bosak.Xslt.Stylesheet.Stylesheet.GetXPathDefaultNamespace(keyDef.Element!);
        var resolvedMatch = ResolveNamespacePrefixes(keyDef.Match, keyDef.Element);
        var compiledMatch = patternCompiler.Compile(resolvedMatch, defaultNs);

        IndexNodes(sourceDocument, keyDef.Name, compiledMatch, useEvaluator, keyDef.Composite, context, index);
    }

    private static void IndexNodes(IXdmNode node, string keyName, Patterns.PatternPredicate match, XPath31Expression? useExpr, bool composite, EvaluationContext context, KeyIndex index)
    {
        TryIndexNode(node, keyName, match, useExpr, composite, context, index);

        // Attributes can also match xsl:key/@match patterns such as @id.
        if (node.NodeKind == XdmNodeKind.Element)
        {
            foreach (var attr in node.Attributes())
            {
                if (attr.IsNode && attr.NodeValue != null)
                    TryIndexNode(attr.NodeValue, keyName, match, useExpr, composite, context, index);
            }
        }

        foreach (var child in node.Axis(XdmAxis.Child))
        {
            if (child.NodeValue != null)
                IndexNodes(child.NodeValue, keyName, match, useExpr, composite, context, index);
        }
    }

    private static void IndexNodes(IXdmNode node, string keyName, Patterns.PatternPredicate match, Func<IXdmNode, XdmValue> useEvaluator, bool composite, EvaluationContext context, KeyIndex index)
    {
        TryIndexNode(node, keyName, match, useEvaluator, composite, context, index);

        if (node.NodeKind == XdmNodeKind.Element)
        {
            foreach (var attr in node.Attributes())
            {
                if (attr.IsNode && attr.NodeValue != null)
                    TryIndexNode(attr.NodeValue, keyName, match, useEvaluator, composite, context, index);
            }
        }

        foreach (var child in node.Axis(XdmAxis.Child))
        {
            if (child.NodeValue != null)
                IndexNodes(child.NodeValue, keyName, match, useEvaluator, composite, context, index);
        }
    }

    private static void TryIndexNode(IXdmNode node, string keyName, Patterns.PatternPredicate match, XPath31Expression? useExpr, bool composite, EvaluationContext context, KeyIndex index)
    {
        if (match(XdmValue.FromNode(node), context))
        {
            var keyValues = useExpr != null
                ? useExpr.Evaluate(context.WithFocus(XdmValue.FromNode(node), 1, 1))
                : XdmValue.Undefined;
            AddKeyValues(index, keyName, keyValues, node, composite);
        }
    }

    private static void TryIndexNode(IXdmNode node, string keyName, Patterns.PatternPredicate match, Func<IXdmNode, XdmValue> useEvaluator, bool composite, EvaluationContext context, KeyIndex index)
    {
        if (match(XdmValue.FromNode(node), context))
        {
            var keyValues = useEvaluator(node);
            AddKeyValues(index, keyName, keyValues, node, composite);
        }
    }

    private static void AddKeyValues(KeyIndex index, string keyName, XdmValue keyValues, IXdmNode node, bool composite)
    {
        if (keyValues.IsUndefined)
            return;

        var items = new List<XdmValue>();
        if (keyValues.IsSequence && keyValues.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(keyValues.SequenceValue))
            {
                if (!item.IsUndefined)
                    items.Add(AtomizeKeyValue(item));
            }
        }
        else
        {
            items.Add(AtomizeKeyValue(keyValues));
        }

        if (items.Count == 0)
            return;

        if (composite)
        {
            index.AddComposite(keyName, items.ToArray(), node);
        }
        else
        {
            foreach (var item in items)
                index.Add(keyName, item, node);
        }
    }

    private static XdmValue AtomizeKeyValue(XdmValue value)
    {
        if (value.IsNode)
            return XdmValue.FromString(value.ToString(), "untypedAtomic");
        return value;
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
                while (i < pattern.Length && (char.IsLetterOrDigit(pattern[i]) || pattern[i] == '_' || pattern[i] == '-' || pattern[i] == '.'))
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
