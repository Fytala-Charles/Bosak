// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 November 2026
// PURPOSE              : Checks named-template prefix diagnostics and preserved namespace boundaries.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.Xslt.Api;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Checks named-template prefix diagnostics and preserved namespace boundaries.</summary>
public class TemplateNameErrorCodeTests
{
    private static readonly XNamespace Xsl = "http://www.w3.org/1999/XSL/Transform";

    /// <summary>Well-formed unbound template names report the specific XSLT static code.</summary>
    /// <param name="version">The earlier stylesheet version.</param>
    /// <param name="name">The unbound name, including surrounding whitespace.</param>
    /// <param name="hasMatch">Whether the template also has a match pattern.</param>
    [Theory]
    [InlineData("2.0", "missing:entry", false)]
    [InlineData("2.0", "missing:entry", true)]
    [InlineData("3.0", "missing:entry", false)]
    [InlineData("3.0", "missing:entry", true)]
    [InlineData("2.0", "other:main", false)]
    [InlineData("2.0", "other:main", true)]
    [InlineData("3.0", "other:main", false)]
    [InlineData("3.0", "other:main", true)]
    [InlineData("2.0", "  missing:entry  ", false)]
    [InlineData("3.0", "  missing:entry  ", false)]
    public void UnboundTemplateNameReportsXsltCode(string version, string name, bool hasMatch)
    {
        var template = Template(name);
        if (hasMatch) template.Add(new XAttribute("match", "/"));
        var sheet = Sheet(template, Template("main", new XElement(Xsl + "text", "ok")));
        sheet.Root!.SetAttributeValue("version", version);
        Assert.Equal("XTSE0280", CompileCode(sheet));
    }

    /// <summary>Imported and included modules cannot borrow the caller's namespace binding.</summary>
    /// <param name="directive">The module linkage instruction.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void UnboundModuleNameUsesDefinitionContext(string directive)
    {
        var resolver = new MemoryResolver(Sheet(Template("missing:entry")));
        var sheet = Sheet(Link(directive), Template("main"));
        sheet.Root!.Add(new XAttribute(XNamespace.Xmlns + "missing", "urn:caller"));
        Assert.Equal("XTSE0280", CompileCode(sheet, resolver));
        Assert.Equal(1, resolver.Calls);
    }

    /// <summary>Legal lexical names and EQNames still identify callable templates.</summary>
    /// <param name="name">The declaration name.</param>
    /// <param name="reference">The corresponding invocation name.</param>
    [Theory]
    [InlineData("helper", "helper")]
    [InlineData("p:helper", "p:helper")]
    [InlineData("  p:helper  ", "p:helper")]
    [InlineData("Q{urn:bound}helper", "p:helper")]
    public void BoundNamesRemainCallable(string name, string reference)
    {
        var sheet = Sheet(Template(name, new XElement(Xsl + "text", "ok")),
            Template("main", Call(reference)));
        sheet.Root!.Add(new XAttribute("xmlns", "urn:unrelated-default"));
        Assert.Equal("ok", Run(sheet));
    }

    /// <summary>A declaration-local binding still takes precedence over the root binding.</summary>
    [Fact]
    public void LocalPrefixBindingRemainsEffective()
    {
        var template = Template("p:entry", new XElement(Xsl + "text", "local"));
        template.Add(new XAttribute(XNamespace.Xmlns + "p", "urn:local"));
        Assert.Equal("local", Run(Sheet(template, Template("main", Call("Q{urn:local}entry")))));
    }

    /// <summary>The implicit xml binding remains available without an explicit declaration.</summary>
    [Fact]
    public void ImplicitXmlBindingRemainsAvailable()
        => Assert.Equal("ok", Run(Sheet(Template("xml:entry", new XElement(Xsl + "text", "ok")),
            Template("main", Call("xml:entry")))));

    /// <summary>The permitted standard initial-template name keeps its reserved-name exemption.</summary>
    [Fact]
    public void InitialTemplateExemptionRemainsAvailable()
        => Assert.Equal("ok", new XsltCompiler().Compile(Sheet(Template("xsl:initial-template", new XElement(Xsl + "text", "ok"))))
            .TransformToString(null, initialTemplate: "xsl:initial-template"));

    /// <summary>Other reserved namespace names keep their independent static error.</summary>
    /// <param name="name">The reserved template name.</param>
    [Theory]
    [InlineData("xsl:other")]
    [InlineData("xs:entry")]
    public void ReservedNamesKeepExistingCode(string name)
        => Assert.Equal("XTSE0080", CompileCode(Sheet(Template(name))));

    /// <summary>The new guard does not reorder existing diagnostics for malformed lexical names.</summary>
    /// <param name="name">A malformed name with an unbound prefix.</param>
    [Theory]
    [InlineData("missing:bad:name")]
    [InlineData("missing:")]
    public void MalformedNamesKeepExistingDiagnosticPrecedence(string name)
        => Assert.Equal("XPST0081", CompileCode(Sheet(Template(name))));

    /// <summary>Discarded declarations do not undergo the new early namespace check.</summary>
    /// <param name="name">The discarded unbound name.</param>
    [Theory]
    [InlineData("missing:entry")]
    [InlineData("other:main")]
    public void DiscardedTemplatesRemainIgnored(string name)
    {
        var discarded = Template(name);
        discarded.Add(new XAttribute("use-when", "false()"));
        Assert.Equal("ok", Run(Sheet(discarded, Template("main", new XElement(Xsl + "text", "ok")))));
    }

    /// <summary>XPath expression diagnostics keep their original code outside template-name checking.</summary>
    [Fact]
    public void XPathExpressionDiagnosticIsNotRewritten()
    {
        var sheet = Sheet(Template("main", new XElement(Xsl + "value-of", new XAttribute("select", "missing:root"))));
        var result = XsltValidation.Validate(Encoding.UTF8.GetBytes(sheet.ToString()), new Uri("file:///template-name-test/main.xsl"));
        Assert.Equal("XPST0081", Assert.Single(result.Diagnostics).Code);
    }

    /// <summary>Module definitions retain their own bound names independently of caller bindings.</summary>
    /// <param name="directive">The module linkage instruction.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void BoundModuleNamesRemainIndependent(string directive)
    {
        var module = Sheet(Template("p:entry", new XElement(Xsl + "text", "module")));
        module.Root!.SetAttributeValue(XNamespace.Xmlns + "p", "urn:module");
        var resolver = new MemoryResolver(module);
        var sheet = Sheet(Link(directive), Template("main", Call("Q{urn:module}entry")));
        Assert.Equal("module", Run(sheet, resolver));
        Assert.Equal(1, resolver.Calls);
    }

    private static XElement Template(string name, params object[] content) => new(Xsl + "template", new XAttribute("name", name), content);
    private static XElement Call(string name) => new(Xsl + "call-template", new XAttribute("name", name));
    private static XElement Link(string directive) => new(Xsl + directive, new XAttribute("href", "module.xsl"));
    private static XDocument Sheet(params object[] content) => new(new XElement(Xsl + "stylesheet",
        new XAttribute("version", "3.0"), new XAttribute(XNamespace.Xmlns + "xsl", Xsl.NamespaceName),
        new XAttribute(XNamespace.Xmlns + "p", "urn:bound"),
        new XAttribute(XNamespace.Xmlns + "xs", "http://www.w3.org/2001/XMLSchema"),
        new XElement(Xsl + "output", new XAttribute("method", "text")), content));
    private static XsltCompiler Compiler(MemoryResolver? resolver) => new() { UriResolver = resolver };
    private static string Run(XDocument sheet, MemoryResolver? resolver = null)
        => Compiler(resolver).Compile(sheet).TransformToString(null, initialTemplate: "main");
    private static string CompileCode(XDocument sheet, MemoryResolver? resolver = null)
        => Assert.Throws<InvalidOperationException>(() => Compiler(resolver).Compile(sheet)).Message.Split(':', 2)[0];

    private sealed class MemoryResolver(XDocument module) : IXsltUriResolver
    {
        internal int Calls { get; private set; }

        /// <summary>Returns a fresh copy of the module and counts the resolution request.</summary>
        /// <param name="href">The requested module reference.</param>
        /// <param name="baseUri">The principal module base URI.</param>
        /// <returns>A fresh stylesheet module.</returns>
        public XDocument Resolve(string href, string? baseUri)
        {
            Calls++;
            return new XDocument(module);
        }
    }
}
