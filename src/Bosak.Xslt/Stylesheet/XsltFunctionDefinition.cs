// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 mei 2026
// PURPOSE              : Parsed representation of an xsl:function declaration
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 27-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 13-06-2026     | EQName support and reserved namespace validation for xsl:function/@name                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml;
using System.Xml.Linq;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Represents a parsed &lt;xsl:function&gt; declaration, including its parameters,
/// return type, and sequence-constructor body.
/// </summary>
public sealed class XsltFunctionDefinition
{
    /// <summary>Namespace URI of the function (resolved from the prefix in the name attribute).</summary>
    public string NamespaceUri { get; }

    /// <summary>Local name of the function.</summary>
    public string LocalName { get; }

    /// <summary>Number of parameters.</summary>
    public int Arity { get; }

    /// <summary>Parameter names in declaration order.</summary>
    public IReadOnlyList<string> ParameterNames { get; }

    /// <summary>Optional return type from the <c>as</c> attribute.</summary>
    public string? ReturnType { get; }

    /// <summary>The raw &lt;xsl:function&gt; element (body is in its children).</summary>
    public XElement Element { get; }

    /// <summary>Import precedence (0 = main stylesheet, higher = deeper imports).</summary>
    public int ImportPrecedence { get; }

    /// <summary>Parent stylesheet that declared this function.</summary>
    public Stylesheet Stylesheet { get; }

    /// <summary>
    /// Visibility of the function: <c>public</c>, <c>private</c>, <c>final</c>, or <c>abstract</c>.
    /// Defaults to <c>private</c> when the attribute is absent.
    /// </summary>
    public string Visibility { get; }

    private XsltFunctionDefinition(
        string namespaceUri,
        string localName,
        int arity,
        IReadOnlyList<string> parameterNames,
        string? returnType,
        XElement element,
        int importPrecedence,
        Stylesheet stylesheet,
        string visibility)
    {
        NamespaceUri = namespaceUri;
        LocalName = localName;
        Arity = arity;
        ParameterNames = parameterNames;
        ReturnType = returnType;
        Element = element;
        ImportPrecedence = importPrecedence;
        Stylesheet = stylesheet;
        Visibility = visibility;
    }

    /// <summary>
    /// Parses an &lt;xsl:function&gt; element into a <see cref="XsltFunctionDefinition"/>.
    /// Returns <c>null</c> if the element is missing required attributes.
    /// </summary>
    private static readonly HashSet<string> ReservedFunctionNamespaces = new(StringComparer.Ordinal)
    {
        "http://www.w3.org/2001/XMLSchema",
        "http://www.w3.org/1999/XSL/Transform",
        "http://www.w3.org/2005/xpath-functions",
        "http://www.w3.org/XML/1998/namespace"
    };

    public static XsltFunctionDefinition? FromElement(XElement element, Stylesheet stylesheet)
    {
        var nameAttr = element.Attribute("name")?.Value;
        if (string.IsNullOrEmpty(nameAttr))
            return null;

        // Resolve QName: prefix:local, Q{uri}local EQName, or just local.
        string nsUri;
        string localName;
        if (nameAttr.Length > 2 && nameAttr[0] == 'Q' && nameAttr[1] == '{')
        {
            int closeBrace = nameAttr.IndexOf('}');
            if (closeBrace < 2)
                throw new InvalidOperationException("XTSE0020: Invalid EQName in xsl:function/@name.");
            nsUri = nameAttr[2..closeBrace];
            localName = nameAttr[(closeBrace + 1)..];
        }
        else
        {
            var colonIndex = nameAttr.IndexOf(':');
            if (colonIndex >= 0)
            {
                var prefix = nameAttr[..colonIndex];
                localName = nameAttr[(colonIndex + 1)..];
                if (element.GetNamespaceOfPrefix(prefix) is not { } ns)
                    throw new InvalidOperationException($"XPST0081: Undefined namespace prefix '{prefix}' in xsl:function/@name.");
                nsUri = ns.NamespaceName;
            }
            else
            {
                localName = nameAttr;
                nsUri = string.Empty;
            }
        }

        if (string.IsNullOrEmpty(localName))
            throw new InvalidOperationException("XTSE0020: xsl:function/@name must have a local name.");

        try
        {
            XmlConvert.VerifyNCName(localName);
        }
        catch (XmlException)
        {
            throw new InvalidOperationException($"XTSE0020: '{nameAttr}' is not a valid QName in xsl:function/@name.");
        }

        if (string.IsNullOrEmpty(nsUri))
            throw new InvalidOperationException("XTSE0740: xsl:function name must be in a namespace.");

        if (ReservedFunctionNamespaces.Contains(nsUri))
            throw new InvalidOperationException("XTSE0080: xsl:function name must not use a reserved namespace.");

        // Collect parameter names from xsl:param children
        var paramNames = new List<string>();
        foreach (var param in element.Elements(XName.Get("param", Stylesheet.XslNamespace)))
        {
            var paramName = param.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(paramName))
                paramNames.Add(paramName);
        }

        var returnType = element.Attribute("as")?.Value;
        var visibility = element.Attribute("visibility")?.Value?.ToLowerInvariant() switch
        {
            "public" => "public",
            "final" => "final",
            "abstract" => "abstract",
            "private" => "private",
            _ => "private"
        };

        return new XsltFunctionDefinition(
            nsUri,
            localName,
            paramNames.Count,
            paramNames,
            returnType,
            element,
            stylesheet.ImportPrecedence,
            stylesheet,
            visibility);
    }
}
