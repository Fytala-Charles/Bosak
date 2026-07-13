// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 11 juli 2026
// PURPOSE              : Serializes XDM values to JSON strings.
// SPECIAL NOTES        : Part of the standard XPath / XQuery function library.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 11-07-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 12-07-2026     | Added CharacterMap option and apply it during JSON string encoding.                     |
//                      | Charles Korthout | 0.3   | 13-07-2026     | Serialize sequence-valued array/map members via SerializeTopLevel.                     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Collections.Generic;
using System.Text;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Standard.Json;

/// <summary>
/// Options controlling XDM to JSON serialization.
/// </summary>
public sealed class XdmJsonOptions
{
    /// <summary>Whether to pretty-print the JSON output.</summary>
    public bool Indent { get; set; }

    /// <summary>Whether duplicate keys are permitted in JSON objects.</summary>
    public bool AllowDuplicateNames { get; set; }

    /// <summary>Whether forward slashes should be escaped as <c>\/</c>.</summary>
    public bool EscapeSolidus { get; set; }

    /// <summary>
    /// Optional callback used to serialize node values before they are JSON-escaped.
    /// If null, nodes are serialized using their XML string value.
    /// </summary>
    public Func<XdmValue, string>? NodeSerializer { get; set; }

    /// <summary>
    /// Optional character map applied while serializing JSON string values.
    /// Mapped characters are replaced before JSON escaping is applied. Keys are Unicode codepoints.
    /// </summary>
    public Dictionary<int, string>? CharacterMap { get; set; }
}

/// <summary>
/// Converts XDM values into JSON text according to the XPath/XSLT 3.1
/// serialization rules for the JSON output method.
/// </summary>
public static class XdmJsonSerializer
{
    /// <summary>
    /// Serializes an XDM value as JSON.
    /// </summary>
    public static string Serialize(XdmValue value, XdmJsonOptions? options = null)
    {
        options ??= new XdmJsonOptions();
        var sb = new StringBuilder();
        SerializeTopLevel(value, sb, options, 0);
        return sb.ToString();
    }

    private static void SerializeTopLevel(XdmValue value, StringBuilder sb, XdmJsonOptions options, int indent)
    {
        if (value.IsUndefined)
        {
            sb.Append("null");
            return;
        }

        if (value.IsSequence && value.SequenceValue != null)
        {
            var items = new List<XdmValue>();
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                items.Add(item);
            if (items.Count == 0)
            {
                sb.Append("null");
                return;
            }
            if (items.Count == 1)
            {
                SerializeValue(items[0], sb, options, indent);
                return;
            }
            SerializeArrayItems(items, sb, options, indent);
            return;
        }

        SerializeValue(value, sb, options, indent);
    }

    private static void SerializeValue(XdmValue value, StringBuilder sb, XdmJsonOptions options, int indent)
    {
        if (value.IsUndefined)
        {
            sb.Append("null");
            return;
        }

        if (value.IsMap)
        {
            SerializeMap(value.MapValue, sb, options, indent);
            return;
        }

        if (value.IsArray)
        {
            SerializeArray(value.ArrayValue, sb, options, indent);
            return;
        }

        if (value.IsNode)
        {
            var nodeText = options.NodeSerializer != null
                ? options.NodeSerializer(value)
                : value.NodeValue.ToXmlString();
            sb.Append(EncodeJsonString(nodeText, options.EscapeSolidus));
            return;
        }

        if (value.Kind == XdmValueKind.Boolean)
        {
            sb.Append(value.BooleanValue ? "true" : "false");
            return;
        }

        if (value.Kind == XdmValueKind.String)
        {
            sb.Append(EncodeJsonString(value.StringValue, options.EscapeSolidus, options.CharacterMap));
            return;
        }

        if (IsNumeric(value))
        {
            sb.Append(value.ToString());
            return;
        }

        // Array/map members are sequences; empty becomes null, one item becomes
        // that value, and multiple items become a nested JSON array.
        if (value.IsSequence && value.SequenceValue != null)
        {
            SerializeTopLevel(value, sb, options, indent);
            return;
        }

        sb.Append(EncodeJsonString(value.ToString(), options.EscapeSolidus, options.CharacterMap));
    }

    private static void SerializeMap(XdmMap map, StringBuilder sb, XdmJsonOptions options, int indent)
    {
        sb.Append('{');
        var childIndent = options.Indent ? indent + 1 : 0;
        bool first = true;
        var entries = map.Entries.ToList();
        if (!options.AllowDuplicateNames)
        {
            var seen = new HashSet<string>();
            foreach (var entry in entries)
            {
                var key = entry.Key.ToString();
                if (!seen.Add(key))
                    throw new InvalidOperationException("SERE0022: Duplicate key in serialized JSON object.");
            }
        }

        foreach (var entry in entries)
        {
            if (!first)
                sb.Append(',');
            first = false;
            if (options.Indent)
            {
                sb.AppendLine();
                AppendIndent(sb, childIndent);
            }
            sb.Append(EncodeJsonString(entry.Key.ToString(), options.EscapeSolidus, options.CharacterMap));
            sb.Append(':');
            if (options.Indent)
                sb.Append(' ');
            SerializeValue(entry.Value, sb, options, childIndent);
        }
        if (options.Indent && entries.Count > 0)
        {
            sb.AppendLine();
            AppendIndent(sb, indent);
        }
        sb.Append('}');
    }

    private static void SerializeArray(XdmArray array, StringBuilder sb, XdmJsonOptions options, int indent)
    {
        SerializeArrayItems(array.Values.ToList(), sb, options, indent);
    }

    private static void SerializeArrayItems(IReadOnlyList<XdmValue> items, StringBuilder sb, XdmJsonOptions options, int indent)
    {
        sb.Append('[');
        var childIndent = options.Indent ? indent + 1 : 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0)
                sb.Append(',');
            if (options.Indent)
            {
                sb.AppendLine();
                AppendIndent(sb, childIndent);
            }
            SerializeValue(items[i], sb, options, childIndent);
        }
        if (options.Indent && items.Count > 0)
        {
            sb.AppendLine();
            AppendIndent(sb, indent);
        }
        sb.Append(']');
    }

    private static void AppendIndent(StringBuilder sb, int level)
    {
        for (int i = 0; i < level; i++)
            sb.Append("  ");
    }

    private static bool IsNumeric(XdmValue value)
    {
        return value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Float or XdmValueKind.Double;
    }

    /// <summary>
    /// Encodes a string as a JSON string literal, escaping quotes, backslashes,
    /// control characters and (optionally) forward slashes.
    /// </summary>
    public static string EncodeJsonString(string value, bool escapeSolidus = false)
    {
        return EncodeJsonString(value, escapeSolidus, null);
    }

    /// <summary>
    /// Encodes a string as a JSON string literal, applying any character map before
    /// escaping quotes, backslashes, control characters and (optionally) forward slashes.
    /// </summary>
    public static string EncodeJsonString(string value, bool escapeSolidus, Dictionary<int, string>? characterMap)
    {
        var sb = new StringBuilder();
        sb.Append('"');
        foreach (var rune in value.EnumerateRunes())
        {
            // Character maps are applied before JSON escaping. The replacement string is
            // output as-is and is not itself JSON-escaped or subject to further mapping.
            if (characterMap != null && characterMap.Count > 0 && characterMap.TryGetValue(rune.Value, out var replacement))
            {
                sb.Append(replacement);
                continue;
            }

            var ch = rune.Value;
            switch (ch)
            {
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '/':
                    sb.Append(escapeSolidus ? "\\/" : "/");
                    break;
                case '\b':
                    sb.Append("\\b");
                    break;
                case '\f':
                    sb.Append("\\f");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                default:
                    if (ch <= char.MaxValue && char.IsControl((char)ch))
                    {
                        sb.Append($"\\u{ch:X4}");
                    }
                    else
                    {
                        sb.Append(rune.ToString());
                    }
                    break;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }
}
