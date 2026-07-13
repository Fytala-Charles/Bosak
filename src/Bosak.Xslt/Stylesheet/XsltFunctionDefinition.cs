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
//                      | Charles Korthout | 0.3   | 24-06-2026     | Evaluate _name AVTs to expanded QNames at parse time                                    |
//                      | Charles Korthout | 0.4   | 29-06-2026     | _name AVTs now use the stylesheet static context (external static parameters)         |
//                      | Charles Korthout | 0.5   | 26-06-2026     | Reject xsl:context-item inside xsl:function                                              |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

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
        "http://www.w3.org/2005/xpath-functions/map",
        "http://www.w3.org/2005/xpath-functions/array",
        "http://www.w3.org/2005/xpath-functions/math",
        "http://www.w3.org/XML/1998/namespace"
    };

    public static XsltFunctionDefinition? FromElement(XElement element, Stylesheet stylesheet)
    {
        var nameAttr = element.Attribute("name")?.Value;
        var underscoreNameAttr = element.Attribute("_name")?.Value;

        string? displayName;
        if (!string.IsNullOrEmpty(nameAttr))
        {
            displayName = nameAttr;
        }
        else if (!string.IsNullOrEmpty(underscoreNameAttr))
        {
            displayName = underscoreNameAttr;
        }
        else
        {
            return null;
        }

        // Resolve the (possibly AVT) name to an expanded QName.
        var (nsUri, localName) = ResolveFunctionName(element, nameAttr, underscoreNameAttr, stylesheet);

        if (string.IsNullOrEmpty(localName))
            return null;

        if (!Bosak.XPath.Providers.Xml.Xml11NameCodec.IsValidXml11NCName(localName))
        {
            throw new InvalidOperationException($"XTSE0020: '{displayName}' is not a valid QName in xsl:function/@name.");
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

        // xsl:context-item is not permitted inside xsl:function.
        if (element.Elements(XName.Get("context-item", Stylesheet.XslNamespace)).Any())
            throw new InvalidOperationException("XTSE0010: xsl:context-item is not permitted inside xsl:function.");

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

    /// <summary>
    /// Resolves the function name to an expanded QName. The <paramref name="nameAttr"/> is used
    /// when present; otherwise the <paramref name="underscoreNameAttr"/> AVT is evaluated.
    /// </summary>
    private static (string nsUri, string localName) ResolveFunctionName(
        XElement element,
        string? nameAttr,
        string? underscoreNameAttr,
        Stylesheet stylesheet)
    {
        if (!string.IsNullOrEmpty(nameAttr))
        {
            return ResolveQNameString(nameAttr, element);
        }

        if (!string.IsNullOrEmpty(underscoreNameAttr))
        {
            return EvaluateNameAvt(underscoreNameAttr, element, stylesheet);
        }

        return (string.Empty, string.Empty);
    }

    /// <summary>
    /// Evaluates an attribute value template used for <c>_name</c> and returns the resulting
    /// expanded QName. A single expression that yields an <c>xs:QName</c> is used directly;
    /// otherwise the result is atomized to a string and parsed as an EQName or lexical QName.
    /// </summary>
    private static (string nsUri, string localName) EvaluateNameAvt(
        string avt,
        XElement element,
        Stylesheet stylesheet)
    {
        var trimmed = avt.Trim();
        var staticCtx = new EvaluationContext();
        AddStaticVariables(staticCtx, stylesheet);

        // Single expression AVT: {expr}. If it evaluates to a QName, use it directly.
        if (trimmed.Length >= 2 &&
            trimmed[0] == '{' &&
            FindAvtExprEnd(trimmed, 1) == trimmed.Length - 1)
        {
            var expr = trimmed[1..^1];
            var result = EvaluateXPath(expr, element, staticCtx);
            if (result.Kind == XdmValueKind.QName)
            {
                var qn = result.QNameValue;
                return (qn.NamespaceUri, qn.LocalName);
            }
            return ResolveQNameString(result.ToString(), element);
        }

        // General AVT: concatenate string values of each evaluated expression.
        var expanded = EvaluateAvt(avt, element, staticCtx);
        return ResolveQNameString(expanded, element);
    }

    /// <summary>
    /// Resolves a name string in EQName or prefix:local form against the in-scope namespaces
    /// of the supplied element.
    /// </summary>
    private static (string nsUri, string localName) ResolveQNameString(string name, XElement element)
    {
        var trimmed = name.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return (string.Empty, string.Empty);

        if (trimmed.Length > 2 && trimmed[0] == 'Q' && trimmed[1] == '{')
        {
            int closeBrace = trimmed.IndexOf('}');
            if (closeBrace < 2)
                throw new InvalidOperationException("XTSE0020: Invalid EQName in xsl:function/@_name.");
            var nsUri = trimmed.Substring(2, closeBrace - 2);
            var localName = trimmed.Substring(closeBrace + 1);
            return (nsUri, localName);
        }

        var colonIndex = trimmed.IndexOf(':');
        if (colonIndex >= 0)
        {
            var prefix = trimmed.Substring(0, colonIndex);
            var localName = trimmed.Substring(colonIndex + 1);
            if (element.GetNamespaceOfPrefix(prefix) is not { } ns)
                throw new InvalidOperationException($"XPST0081: Undefined namespace prefix '{prefix}' in xsl:function/@_name.");
            return (ns.NamespaceName, localName);
        }

        return (string.Empty, trimmed);
    }

    /// <summary>
    /// Adds the stylesheet's evaluated static variables and parameters to the supplied
    /// evaluation context so that <c>_name</c> AVTs can reference them.
    /// </summary>
    private static void AddStaticVariables(EvaluationContext ctx, Stylesheet stylesheet)
    {
        foreach (var kv in stylesheet.StaticVariables)
        {
            ctx.WithVariable(kv.Key.LocalName, kv.Value, kv.Key.NamespaceUri);
        }
    }

    /// <summary>
    /// Evaluates a general attribute value template by compiling each <c>{expr}</c> fragment
    /// as an XPath expression and concatenating the atomized results.
    /// </summary>
    private static string EvaluateAvt(string avt, XElement element, EvaluationContext staticCtx)
    {
        if (string.IsNullOrEmpty(avt) || !avt.Contains('{'))
            return avt;

        var nsMap = ExtractNamespaces(element);

        var sb = new StringBuilder();
        int i = 0;
        while (i < avt.Length)
        {
            if (i + 1 < avt.Length && avt[i] == '{' && avt[i + 1] == '{')
            {
                sb.Append('{');
                i += 2;
            }
            else if (i + 1 < avt.Length && avt[i] == '}' && avt[i + 1] == '}')
            {
                sb.Append('}');
                i += 2;
            }
            else if (avt[i] == '{')
            {
                int end = FindAvtExprEnd(avt, i + 1);
                if (end < 0)
                {
                    sb.Append(avt[i]);
                    i++;
                }
                else
                {
                    var expr = avt.Substring(i + 1, end - i - 1);
                    if (!string.IsNullOrEmpty(expr))
                    {
                        var compiled = XPath31Expression.Compile(expr, new CompileOptions { Namespaces = nsMap });
                        var result = compiled.Evaluate(staticCtx);
                        sb.Append(AtomizedAvtString(result));
                    }
                    i = end + 1;
                }
            }
            else
            {
                sb.Append(avt[i]);
                i++;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Atomizes a value for AVT concatenation, returning the string value of each item
    /// without separators.
    /// </summary>
    private static string AtomizedAvtString(XdmValue value)
    {
        if (value.IsUndefined)
            return string.Empty;

        if (value.IsSequence && value.SequenceValue != null)
        {
            var sb = new StringBuilder();
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                sb.Append(item.ToString());
            return sb.ToString();
        }

        return value.ToString();
    }

    /// <summary>
    /// Evaluates a single XPath expression in the static context of the function declaration.
    /// </summary>
    private static XdmValue EvaluateXPath(string expr, XElement element, EvaluationContext staticCtx)
    {
        var nsMap = ExtractNamespaces(element);
        var compiled = XPath31Expression.Compile(expr, new CompileOptions { Namespaces = nsMap });
        return compiled.Evaluate(staticCtx);
    }

    /// <summary>
    /// Finds the index of the matching closing brace for an AVT expression, skipping
    /// string literals and nested braces.
    /// </summary>
    private static int FindAvtExprEnd(string value, int start)
    {
        char inString = '\0';
        int braceDepth = 1;
        for (int i = start; i < value.Length; i++)
        {
            char c = value[i];
            if (inString != '\0')
            {
                if (c == inString)
                {
                    if (i + 1 < value.Length && value[i + 1] == inString)
                    {
                        i++;
                    }
                    else
                    {
                        inString = '\0';
                    }
                }
                continue;
            }

            if (c == '\'' || c == '"')
            {
                inString = c;
                continue;
            }

            if (c == '{')
            {
                braceDepth++;
                continue;
            }

            if (c == '}')
            {
                braceDepth--;
                if (braceDepth == 0)
                    return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Collects the in-scope namespace declarations for an element and its ancestors.
    /// </summary>
    private static Dictionary<string, string> ExtractNamespaces(XElement element)
    {
        var dict = new Dictionary<string, string>();
        var current = element;
        while (current != null)
        {
            foreach (var attr in current.Attributes().Where(a => a.IsNamespaceDeclaration))
            {
                var prefix = attr.Name.LocalName;
                if (prefix == "xmlns")
                    prefix = "";
                if (!string.IsNullOrEmpty(prefix) && !dict.ContainsKey(prefix))
                    dict[prefix] = attr.Value;
            }
            current = current.Parent;
        }
        return dict;
    }
}
