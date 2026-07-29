// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 mei 2026
// PURPOSE              : Represents a QT3 test environment (source docs, namespaces, parameters).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 22-05-2026     | Added decimal-format parsing from QT3 test environments                                |
//                      | Charles Korthout | 0.3   | 27-05-2026     | Added default collation parsing from QT3 test environments                             |
//                      | Charles Korthout | 0.4   | 15-07-2026     | Parse <resource>/<source uri=> into a URI map installed as ctx.ResourceUriMapper       |
//                      | Charles Korthout | 0.5   | 15-07-2026     | Bind <source role="$var"> documents to variables (generalexpression, fn-transform)     |
//                      | Charles Korthout | 0.6   | 15-07-2026     | Roleless <source> no longer becomes the context item (URI-map only; d1e41648)          |
//                      | Charles Korthout | 0.7   | 15-07-2026     | LoadXml uses the published <source uri> as the document base URI                       |
//                      | Charles Korthout | 0.8   | 18-07-2026     | Parse <collection> elements into EvaluationContext.Collections                          |
//                      | Charles Korthout | 0.9   | 19-07-2026     | Parse <source validation> and <schema> for strict XML Schema validation of sources     |
//                      | Charles Korthout | 1.0   | 20-07-2026     | Map environment <namespace prefix=""> to EvaluationContext.DefaultElementNamespace    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.1   | 27-07-2026     | Apply inline <context-item select="..."/> as the initial focus                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.2   | 29-07-2026     | Prefixed <param> names resolve namespaces in scope on the param element itself |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using DecimalFormat = Bosak.XPath.Runtime.Vm.DecimalFormat;

namespace Bosak.XPath.Conformance;

internal sealed class TestEnvironment
{
    public List<SourceDocument> Sources { get; } = new();
    public List<SourceSchema> Schemas { get; } = new();
    public List<NamespaceBinding> Namespaces { get; } = new();
    public List<ExternalParameter> Parameters { get; } = new();
    public List<DecimalFormatEntry> DecimalFormats { get; } = new();
    /// <summary>Collection URI -> list of document file paths/URIs. Empty string key = default collection.</summary>
    public Dictionary<string, List<string>> Collections { get; } = new();
    public string? DefaultCollation { get; set; }
    public string? BaseUri { get; set; }
    /// <summary>The select expression of an inline <c>&lt;context-item select="..."/&gt;</c>, if any.</summary>
    public string? ContextItemSelect { get; set; }

    /// <summary>Maps published resource URIs (typically http:) to local suite files.</summary>
    public Dictionary<string, string> UriMap { get; } = new(StringComparer.Ordinal);

    public static TestEnvironment FromElement(XElement element, string suitePath, string baseDir)
    {
        var env = new TestEnvironment();
        XNamespace ns = element.Name.Namespace;

        foreach (var source in element.Elements(ns + "source"))
        {
            string? file = (string?)source.Attribute("file");
            string? role = (string?)source.Attribute("role");
            string? uri = (string?)source.Attribute("uri");
            string? validation = (string?)source.Attribute("validation");
            if (file is not null)
            {
                string path = Path.IsPathRooted(file) ? file : Path.Combine(baseDir, file);
                if (!File.Exists(path))
                {
                    path = Path.Combine(suitePath, file);
                }
                // A source without an explicit role is only URI-mapped (available to
                // fn:doc/fn:collection); it must NOT become the context item. Only
                // role="." designates the context document (FOTS convention; d1e41648).
                env.Sources.Add(new SourceDocument(role ?? "", path, uri, validation));
                if (uri is not null && File.Exists(path))
                {
                    env.UriMap[uri] = path;
                }
            }
        }

        foreach (var schema in element.Elements(ns + "schema"))
        {
            string? file = (string?)schema.Attribute("file");
            string? uri = (string?)schema.Attribute("uri");
            if (file is not null)
            {
                string path = Path.IsPathRooted(file) ? file : Path.Combine(baseDir, file);
                if (!File.Exists(path))
                {
                    path = Path.Combine(suitePath, file);
                }
                env.Schemas.Add(new SourceSchema(uri, path));
            }
        }

        foreach (var resource in element.Elements(ns + "resource"))
        {
            string? file = (string?)resource.Attribute("file");
            string? uri = (string?)resource.Attribute("uri");
            if (file is not null && uri is not null)
            {
                string path = Path.IsPathRooted(file) ? file : Path.Combine(baseDir, file);
                if (!File.Exists(path))
                {
                    path = Path.Combine(suitePath, file);
                }
                // Only map URIs whose target actually exists; unmapped URIs fall through
                // to normal resolution so missing suite files keep their original behavior.
                if (File.Exists(path))
                {
                    env.UriMap[uri] = path;
                }
            }
        }

        foreach (var colElem in element.Elements(ns + "collection"))
        {
            string? uri = (string?)colElem.Attribute("uri");
            string key = uri ?? "";
            var docs = new List<string>();
            foreach (var source in colElem.Elements(ns + "source"))
            {
                string? file = (string?)source.Attribute("file");
                string? sourceUri = (string?)source.Attribute("uri");
                if (file is not null)
                {
                    string path = Path.IsPathRooted(file) ? file : Path.Combine(baseDir, file);
                    if (!File.Exists(path))
                        path = Path.Combine(suitePath, file);
                    if (File.Exists(path))
                        docs.Add(Path.GetFullPath(path));
                }
                else if (sourceUri is not null)
                {
                    docs.Add(sourceUri);
                }
            }
            env.Collections[key] = docs;
        }

        foreach (var nsElem in element.Elements(ns + "namespace"))
        {
            string? prefix = (string?)nsElem.Attribute("prefix");
            string? uri = (string?)nsElem.Attribute("uri");
            if (prefix is not null && uri is not null)
            {
                env.Namespaces.Add(new NamespaceBinding(prefix, uri));
            }
        }

        foreach (var param in element.Elements(ns + "param"))
        {
            string? name = (string?)param.Attribute("name");
            string? select = (string?)param.Attribute("select");
            if (name is not null)
            {
                // A prefixed parameter name resolves against the namespaces in scope on the
                // <param> element itself (extvardeclwithouttype-24's xmlns:test).
                string? paramNs = null;
                int colon = name.IndexOf(':');
                if (colon > 0)
                    paramNs = param.GetNamespaceOfPrefix(name[..colon])?.NamespaceName;
                env.Parameters.Add(new ExternalParameter(name, select ?? "", paramNs));
            }
        }

        var staticBaseUri = element.Element(ns + "static-base-uri");
        if (staticBaseUri is not null)
        {
            env.BaseUri = (string?)staticBaseUri.Attribute("uri");
        }

        var contextItemElem = element.Element(ns + "context-item");
        if (contextItemElem is not null)
        {
            env.ContextItemSelect = (string?)contextItemElem.Attribute("select");
        }

        foreach (var colElem in element.Elements(ns + "collation"))
        {
            string? uri = (string?)colElem.Attribute("uri");
            bool isDefault = (string?)colElem.Attribute("default") == "true";
            if (uri is not null && isDefault)
            {
                env.DefaultCollation = uri;
            }
        }

        foreach (var dfElem in element.Elements(ns + "decimal-format"))
        {
            var format = new DecimalFormat();

            string? decSep = (string?)dfElem.Attribute("decimal-separator");
            if (!string.IsNullOrEmpty(decSep)) format.DecimalSeparator = decSep;

            string? grpSep = (string?)dfElem.Attribute("grouping-separator");
            if (!string.IsNullOrEmpty(grpSep)) format.GroupingSeparator = grpSep;

            string? digit = (string?)dfElem.Attribute("digit");
            if (!string.IsNullOrEmpty(digit)) format.Digit = digit;

            string? zeroDigit = (string?)dfElem.Attribute("zero-digit");
            if (!string.IsNullOrEmpty(zeroDigit)) format.ZeroDigit = zeroDigit;

            string? patSep = (string?)dfElem.Attribute("pattern-separator");
            if (!string.IsNullOrEmpty(patSep)) format.PatternSeparator = patSep;

            string? minus = (string?)dfElem.Attribute("minus-sign");
            if (!string.IsNullOrEmpty(minus)) format.MinusSign = minus;

            string? pct = (string?)dfElem.Attribute("percent");
            if (!string.IsNullOrEmpty(pct)) format.Percent = pct;

            string? permille = (string?)dfElem.Attribute("per-mille");
            if (!string.IsNullOrEmpty(permille)) format.PerMille = permille;

            string? infinity = (string?)dfElem.Attribute("infinity");
            if (!string.IsNullOrEmpty(infinity)) format.Infinity = infinity;

            string? nan = (string?)dfElem.Attribute("NaN");
            if (!string.IsNullOrEmpty(nan)) format.NaN = nan;

            string? expSep = (string?)dfElem.Attribute("exponent-separator");
            if (!string.IsNullOrEmpty(expSep)) format.ExponentSeparator = expSep;

            string? name = (string?)dfElem.Attribute("name");
            string? resolvedLocalName = name;
            string? resolvedNamespace = "";

            if (!string.IsNullOrEmpty(name) && name.Contains(':'))
            {
                int colon = name.IndexOf(':');
                string prefix = name.Substring(0, colon);
                string local = name.Substring(colon + 1);
                var nsAttr = dfElem.Attributes()
                    .FirstOrDefault(a => a.Name.NamespaceName == "http://www.w3.org/2000/xmlns/" && a.Name.LocalName == prefix);
                if (nsAttr is not null)
                {
                    resolvedNamespace = nsAttr.Value;
                    resolvedLocalName = local;
                }
                else
                {
                    // Try inherited namespace from environment
                    var inheritedNs = element.Attributes()
                        .FirstOrDefault(a => a.Name.NamespaceName == "http://www.w3.org/2000/xmlns/" && a.Name.LocalName == prefix);
                    if (inheritedNs is not null)
                    {
                        resolvedNamespace = inheritedNs.Value;
                        resolvedLocalName = local;
                    }
                }
            }

            env.DecimalFormats.Add(new DecimalFormatEntry(resolvedLocalName ?? "", resolvedNamespace ?? "", format));
        }

        return env;
    }

    public EvaluationContext ApplyTo(EvaluationContext ctx)
    {
        foreach (var ns in Namespaces)
        {
            if (string.IsNullOrEmpty(ns.Prefix))
            {
                ctx.DefaultElementNamespace = ns.Uri;
            }
            else
            {
                ctx = ctx.WithNamespace(ns.Prefix, ns.Uri);
            }
        }

        foreach (var src in Sources)
        {
            XmlSchemaSet? strictSchemas = null;
            if (string.Equals(src.Validation, "strict", StringComparison.OrdinalIgnoreCase))
            {
                strictSchemas = BuildSchemaSet();
            }

            if (src.Role == "." && File.Exists(src.FilePath))
            {
                try
                {
                    var doc = XDocumentProvider.LoadXml(src.FilePath, src.Uri, strictSchemas);
                    ctx = ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);
                }
                catch (System.Xml.XmlException ex) when (ex.Message.Contains("1.1"))
                {
                    // XML 1.1 is not supported by XDocument; rethrow as a known exception
                    throw new NotSupportedException("XML 1.1 not supported");
                }
            }
            else if (src.Role.StartsWith("$", StringComparison.Ordinal) && File.Exists(src.FilePath))
            {
                // Sources with a variable role (role="$name") are bound to that variable as
                // document nodes. Non-XML resources stay unbound (previous behavior).
                try
                {
                    var doc = XDocumentProvider.LoadXml(src.FilePath, src.Uri, strictSchemas);
                    ctx = ctx.WithVariable(src.Role.Substring(1), XdmValue.FromNode(doc));
                }
                catch (Exception)
                {
                    // Leave the variable unbound; the test fails as before with XPST0008.
                }
            }
        }

        if (!string.IsNullOrEmpty(BaseUri) && BaseUri != "#UNDEFINED")
        {
            ctx.BaseUri = BaseUri;
        }

        if (UriMap.Count > 0)
        {
            var previous = ctx.ResourceUriMapper;
            ctx.ResourceUriMapper = u => UriMap.TryGetValue(u, out var path) ? path : previous?.Invoke(u);
        }

        foreach (var kvp in Collections)
        {
            ctx.Collections[kvp.Key] = kvp.Value;
        }

        foreach (var df in DecimalFormats)
        {
            if (string.IsNullOrEmpty(df.Name))
            {
                ctx.DefaultDecimalFormat = df.Format;
            }
            else
            {
                ctx.WithDecimalFormat(df.Name, df.NamespaceUri, df.Format);
            }
        }

        if (!string.IsNullOrEmpty(DefaultCollation))
        {
            ctx.WithDefaultCollation(DefaultCollation);
        }

        // An inline <context-item select="..."/> (no source document) is evaluated in the
        // prepared environment and becomes the initial focus (prod/ContextItemDecl).
        if (!string.IsNullOrEmpty(ContextItemSelect))
        {
            var contextItem = Bosak.XPath.Api.XPath31Expression.Compile(ContextItemSelect).Evaluate(ctx);
            ctx = ctx.WithFocus(contextItem, 1, 1);
        }

        // Note: External parameters are not yet supported; they require evaluating the select expression
        return ctx;
    }

    private XmlSchemaSet? BuildSchemaSet()
    {
        if (Schemas.Count == 0)
            return null;

        var schemaSet = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };
        foreach (var schema in Schemas)
        {
            using var stream = File.OpenRead(schema.FilePath);
            using var reader = XmlReader.Create(stream, null, new Uri(schema.FilePath).AbsoluteUri);
            schemaSet.Add(XmlSchema.Read(reader, null));
        }
        schemaSet.Compile();
        return schemaSet;
    }
}

internal sealed record SourceDocument(string Role, string FilePath, string? Uri, string? Validation);
internal sealed record SourceSchema(string? Uri, string FilePath);
internal sealed record NamespaceBinding(string Prefix, string Uri);
internal sealed record ExternalParameter(string Name, string SelectExpression, string? NamespaceUri = null);
internal sealed record DecimalFormatEntry(string Name, string NamespaceUri, DecimalFormat Format);
