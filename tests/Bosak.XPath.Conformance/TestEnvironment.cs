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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Conformance;

internal sealed class TestEnvironment
{
    public List<SourceDocument> Sources { get; } = new();
    public List<NamespaceBinding> Namespaces { get; } = new();
    public List<ExternalParameter> Parameters { get; } = new();
    public string? BaseUri { get; set; }

    public static TestEnvironment FromElement(XElement element, string suitePath, string baseDir)
    {
        var env = new TestEnvironment();
        XNamespace ns = element.Name.Namespace;

        foreach (var source in element.Elements(ns + "source"))
        {
            string? file = (string?)source.Attribute("file");
            string? role = (string?)source.Attribute("role");
            string? uri = (string?)source.Attribute("uri");
            if (file is not null)
            {
                string path = Path.IsPathRooted(file) ? file : Path.Combine(baseDir, file);
                if (!File.Exists(path))
                {
                    path = Path.Combine(suitePath, file);
                }
                env.Sources.Add(new SourceDocument(role ?? ".", path, uri));
            }
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
                env.Parameters.Add(new ExternalParameter(name, select ?? ""));
            }
        }

        var staticBaseUri = element.Element(ns + "static-base-uri");
        if (staticBaseUri is not null)
        {
            env.BaseUri = (string?)staticBaseUri.Attribute("uri");
        }

        return env;
    }

    public EvaluationContext ApplyTo(EvaluationContext ctx)
    {
        foreach (var ns in Namespaces)
        {
            ctx = ctx.WithNamespace(ns.Prefix, ns.Uri);
        }

        foreach (var src in Sources)
        {
            if (src.Role == "." && File.Exists(src.FilePath))
            {
                try
                {
                    var doc = XDocumentProvider.ParseXml(File.ReadAllText(src.FilePath));
                    ctx = ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);
                }
                catch (System.Xml.XmlException ex) when (ex.Message.Contains("1.1"))
                {
                    // XML 1.1 is not supported by XDocument; rethrow as a known exception
                    throw new NotSupportedException("XML 1.1 not supported");
                }
            }
        }

        if (!string.IsNullOrEmpty(BaseUri))
        {
            ctx.BaseUri = BaseUri;
        }

        // Note: External parameters are not yet supported; they require evaluating the select expression
        return ctx;
    }
}

internal sealed record SourceDocument(string Role, string FilePath, string? Uri);
internal sealed record NamespaceBinding(string Prefix, string Uri);
internal sealed record ExternalParameter(string Name, string SelectExpression);
