// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 26 juni 2026
// PURPOSE              : Represents the xsl:context-item declaration inside an xsl:template.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 26-06-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Indicates whether a template requires, allows, or ignores the context item.
/// </summary>
public enum ContextItemUse
{
    /// <summary>A context item must be supplied.</summary>
    Required,

    /// <summary>A context item may be supplied; if absent, the template runs without one.</summary>
    Optional,

    /// <summary>The context item is ignored and treated as absent inside the template.</summary>
    Absent
}

/// <summary>
/// Represents a parsed &lt;xsl:context-item&gt; declaration: its <c>use</c> value and
/// optional required type (<c>as</c>).
/// </summary>
public sealed class ContextItemDeclaration
{
    /// <summary>The required/optional/absent behavior for the context item.</summary>
    public ContextItemUse Use { get; }

    /// <summary>The required item type, or <c>null</c> if no type was declared.</summary>
    public string? AsType { get; }

    private ContextItemDeclaration(ContextItemUse use, string? asType)
    {
        Use = use;
        AsType = asType;
    }

    /// <summary>
    /// Parses the optional &lt;xsl:context-item&gt; child of an &lt;xsl:template&gt; element,
    /// performing the static validation required by XSLT 3.0 §10.1.1.
    /// </summary>
    internal static ContextItemDeclaration? FromTemplate(XElement template)
    {
        var contextItems = template.Elements(XName.Get("context-item", Stylesheet.XslNamespace)).ToList();
        if (contextItems.Count == 0)
            return null;

        if (contextItems.Count > 1)
            throw new InvalidOperationException("XTSE0010: Only one xsl:context-item element is allowed inside xsl:template.");

        var element = contextItems[0];

        // xsl:context-item must be the first significant child of the template.
        foreach (var node in element.NodesBeforeSelf())
        {
            if (node is XElement)
                throw new InvalidOperationException("XTSE0010: xsl:context-item must precede all other child elements of xsl:template.");
            if (node is XText text && !string.IsNullOrWhiteSpace(text.Value))
                throw new InvalidOperationException("XTSE0010: xsl:context-item must precede any non-whitespace text in xsl:template.");
        }

        // @select is not permitted.
        if (element.Attribute("select") != null)
            throw new InvalidOperationException("XTSE0090: The select attribute is not permitted on xsl:context-item.");

        // Only @use and @as are allowed.
        foreach (var attr in element.Attributes())
        {
            var name = attr.Name.LocalName;
            var ns = attr.Name.NamespaceName;
            if (ns == "http://www.w3.org/XML/1998/namespace")
                continue; // xml:* standard attributes are ignored
            if (name != "use" && name != "as")
                throw new InvalidOperationException($"XTSE0090: Attribute '{name}' is not permitted on xsl:context-item.");
        }

        var useAttr = element.Attribute("use")?.Value?.Trim();
        var asAttr = element.Attribute("as")?.Value?.Trim();

        ContextItemUse use;
        if (string.IsNullOrEmpty(useAttr))
        {
            // Default is optional when omitted (XSLT 3.0 §10.1.1).
            use = ContextItemUse.Optional;
        }
        else
        {
            use = useAttr switch
            {
                "required" => ContextItemUse.Required,
                "optional" => ContextItemUse.Optional,
                "absent" => ContextItemUse.Absent,
                _ => throw new InvalidOperationException($"XTSE0020: Invalid value '{useAttr}' for xsl:context-item/@use. Must be 'required', 'optional', or 'absent'.")
            };
        }

        // use="absent" is incompatible with @as.
        if (use == ContextItemUse.Absent && !string.IsNullOrEmpty(asAttr))
            throw new InvalidOperationException("XTSE3089: xsl:context-item must not have an as attribute when use is absent.");

        string? validatedAs = null;
        if (!string.IsNullOrEmpty(asAttr))
        {
            var stripped = StripXPathComments(asAttr).Trim();
            var normalized = NormalizeSequenceType(stripped);
            if (HasTopLevelOccurrenceIndicator(normalized))
                throw new InvalidOperationException("XTSE0020: An occurrence indicator is not allowed in xsl:context-item/@as.");

            if (!IsKnownItemType(normalized, element))
                throw new InvalidOperationException($"XTSE0020: Unknown type '{asAttr}' in xsl:context-item/@as.");

            validatedAs = normalized;
        }

        return new ContextItemDeclaration(use, validatedAs);
    }

    /// <summary>
    /// Removes whitespace around grouping and separator punctuation so that a
    /// SequenceType such as "element ( doc )" is normalized to "element(doc)".
    /// </summary>
    private static string NormalizeSequenceType(string type)
    {
        var s = type.Trim();
        // Remove whitespace adjacent to parentheses and commas.
        s = Regex.Replace(s, @"(?<=\()[\s\t\r\n]+|[\s\t\r\n]+(?=\()|(?<=\))[\s\t\r\n]+|[\s\t\r\n]+(?=\))|(?<=,)[\s\t\r\n]+|[\s\t\r\n]+(?=,)", "");
        return s;
    }

    /// <summary>
    /// Returns true if the supplied type string ends with a top-level occurrence indicator.
    /// </summary>
    private static bool HasTopLevelOccurrenceIndicator(string type)
    {
        if (type.Length == 0)
            return false;

        int depth = 0;
        for (int i = 0; i < type.Length; i++)
        {
            var c = type[i];
            if (c == '(') depth++;
            else if (c == ')') depth--;
        }

        if (depth != 0)
            return false;

        var last = type[^1];
        return last is '?' or '*' or '+';
    }

    /// <summary>
    /// Performs a lightweight static check that the declared type is known to this
    /// processor. Schema-defined user types are not supported and will be rejected.
    /// </summary>
    private static bool IsKnownItemType(string type, XElement element)
    {
        var normalized = type.Trim().ToLowerInvariant();

        if (normalized == "item()" || normalized == "item")
            return true;

        // Node kinds.
        if (normalized is "node()" or "node" or "text()" or "text" or "comment()" or "comment"
            or "processing-instruction()" or "processing-instruction" or "namespace-node()" or "namespace-node")
            return true;

        // element() / attribute() / document-node() / schema-element() / schema-attribute()
        if (TryParseKindTest(normalized, out var inner))
        {
            if (string.IsNullOrEmpty(inner))
                return true;

            // element(name) or element(name, type) or element(*, type)
            var parts = SplitTopLevel(inner, ',');
            if (parts.Length == 1)
                return true; // element(name) or attribute(name)

            if (parts.Length >= 2)
            {
                var typePart = parts[1].Trim();
                return IsKnownItemType(typePart, element);
            }
            return true;
        }

        // function(*), function(args) as return
        if (normalized.StartsWith("function(") && normalized.EndsWith(')'))
            return true;

        // map(*), map(K,V)
        if (normalized.StartsWith("map(") && normalized.EndsWith(')'))
        {
            var innerMap = normalized.Substring(4, normalized.Length - 5).Trim();
            if (string.IsNullOrEmpty(innerMap) || innerMap == "*")
                return true;
            var parts = SplitTopLevel(innerMap, ',');
            if (parts.Length == 2)
                return IsKnownItemType(parts[0].Trim(), element) && IsKnownItemType(parts[1].Trim(), element);
            return false;
        }

        // array(*), array(T)
        if (normalized.StartsWith("array(") && normalized.EndsWith(')'))
        {
            var innerArray = normalized.Substring(6, normalized.Length - 7).Trim();
            if (string.IsNullOrEmpty(innerArray) || innerArray == "*")
                return true;
            return IsKnownItemType(innerArray, element);
        }

        // Atomic type.
        var atomic = normalized;
        if (atomic.StartsWith("xs:"))
            atomic = atomic[3..];
        else if (atomic.StartsWith("xsd:"))
            atomic = atomic[4..];

        return atomic switch
        {
            "string" or "boolean" or "integer" or "decimal" or "double" or "float" or "numeric"
            or "datetime" or "date" or "time" or "duration" or "daytimeduration" or "yearmonthduration"
            or "qname" or "anyuri" or "untypedatomic" or "anyatomictype"
            or "gyear" or "gyearmonth" or "gmonthday" or "gday" or "gmonth"
            or "hexbinary" or "base64binary" or "normalizedstring" or "token" or "language"
            or "nmtoken" or "name" or "ncname" or "id" or "idref" or "entity"
            or "int" or "long" or "short" or "byte"
            or "unsignedshort" or "unsignedint" or "unsignedlong" or "unsignedbyte"
            or "positiveinteger" or "negativeinteger" or "nonpositiveinteger" or "nonnegativeinteger" => true,
            _ => false
        };
    }

    private static bool TryParseKindTest(string normalized, out string inner)
    {
        inner = string.Empty;
        string[] kindNames = ["element(", "attribute(", "document-node(", "schema-element(", "schema-attribute("];
        foreach (var kind in kindNames)
        {
            if (normalized.StartsWith(kind) && normalized.EndsWith(')'))
            {
                inner = normalized.Substring(kind.Length, normalized.Length - kind.Length - 1).Trim();
                return true;
            }
        }
        return false;
    }

    private static string[] SplitTopLevel(string value, char separator)
    {
        var parts = new List<string>();
        int depth = 0;
        int start = 0;
        for (int i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c == '(') depth++;
            else if (c == ')') depth--;
            else if (c == separator && depth == 0)
            {
                parts.Add(value.Substring(start, i - start).Trim());
                start = i + 1;
            }
        }
        parts.Add(value.Substring(start).Trim());
        return parts.ToArray();
    }

    private static string StripXPathComments(string text)
    {
        var sb = new System.Text.StringBuilder();
        int i = 0;
        while (i < text.Length)
        {
            char c = text[i];
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
