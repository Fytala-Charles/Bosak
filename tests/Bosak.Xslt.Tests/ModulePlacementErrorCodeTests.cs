// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 November 2026
// PURPOSE              : Checks module placement diagnostics and preserved compilation boundaries.
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
//                      | Charles Korthout | 0.2   | 13-11-2026     | Make test serialization explicit; retain the initial baseline fixture attempt.             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Checks module placement diagnostics and preserved compilation boundaries.</summary>
public class ModulePlacementErrorCodeTests
{
    private static readonly XNamespace Xsl = "http://www.w3.org/1999/XSL/Transform";

    /// <summary>Existing placement rejections report specific codes before module resolution.</summary>
    /// <param name="directive">The module directive.</param>
    /// <param name="context">The nested placement, including an unused template.</param>
    /// <param name="code">The expected leading error code.</param>
    [Theory]
    [InlineData("include", "template", "XTSE0170")]
    [InlineData("import", "template", "XTSE0190")]
    [InlineData("include", "if", "XTSE0170")]
    [InlineData("import", "if", "XTSE0190")]
    [InlineData("include", "literal", "XTSE0170")]
    [InlineData("import", "literal", "XTSE0190")]
    [InlineData("include", "unused", "XTSE0170")]
    [InlineData("import", "unused", "XTSE0190")]
    public void NestedDirectiveReportsSpecificStaticCode(string directive, string context, string code)
    {
        var resolver = new MemoryResolver();
        XElement content = Directive(directive);
        if (context == "if") content = new XElement(Xsl + "if", new XAttribute("test", "true()"), content);
        if (context == "literal") content = new XElement("out", content);
        var template = Template(context == "unused" ? "unused" : "main", content);
        var error = Assert.Throws<InvalidOperationException>(() => Compiler(resolver).Compile(Stylesheet(template)));
        Assert.Equal(code, LeadingCode(error));
        Assert.Equal(0, resolver.Calls);
    }

    /// <summary>Valid top-level directives still load the module and execute its template.</summary>
    /// <param name="directive">The module directive.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void TopLevelDirectiveStillResolvesAndExecutes(string directive)
    {
        var resolver = new MemoryResolver();
        var executable = Compiler(resolver).Compile(Stylesheet(Directive(directive),
            Template("main", new XElement(Xsl + "call-template", new XAttribute("name", "helper")))));
        Assert.Equal("module", executable.TransformToString(null, initialTemplate: "main"));
        Assert.Equal(1, resolver.Calls);
    }

    /// <summary>Import precedence still lets the principal module override an imported template.</summary>
    [Fact]
    public void ImportPrecedenceRemainsUnchanged()
    {
        var resolver = new MemoryResolver();
        var executable = Compiler(resolver).Compile(Stylesheet(Directive("import"),
            Template("helper", new XElement(Xsl + "text", "principal")),
            Template("main", new XElement(Xsl + "call-template", new XAttribute("name", "helper")))));
        Assert.Equal("principal", executable.TransformToString(null, initialTemplate: "main"));
        Assert.Equal(1, resolver.Calls);
    }

    /// <summary>Use-when removal happens before structural validation or module acquisition.</summary>
    /// <param name="directive">The discarded module directive.</param>
    /// <param name="ancestor">Whether the removal is applied to an ancestor.</param>
    [Theory]
    [InlineData("include", false)]
    [InlineData("import", false)]
    [InlineData("include", true)]
    [InlineData("import", true)]
    public void DiscardedDirectiveDoesNotProducePlacementError(string directive, bool ancestor)
    {
        var resolver = new MemoryResolver();
        XElement discarded = Directive(directive);
        if (ancestor) discarded = new XElement(Xsl + "if", new XAttribute("test", "true()"), discarded);
        discarded.Add(new XAttribute("use-when", "false()"));
        var executable = Compiler(resolver).Compile(Stylesheet(Template("main", discarded, new XElement(Xsl + "text", "ok"))));
        Assert.Equal("ok", executable.TransformToString(null, initialTemplate: "main"));
        Assert.Equal(0, resolver.Calls);
    }

    /// <summary>Names in a foreign namespace remain literal result elements.</summary>
    /// <param name="directive">The local name shared with an XSLT directive.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void ForeignNamespaceNamesRemainLiteral(string directive)
    {
        var resolver = new MemoryResolver();
        var literal = new XElement(XName.Get(directive, "urn:literal"), new XAttribute("href", "module.xsl"));
        var result = Compiler(resolver).Compile(Stylesheet(Template("main", literal)))
            .TransformToString(null, initialTemplate: "main");
        Assert.Equal(literal.Name, XElement.Parse(result).Name);
        Assert.Equal(0, resolver.Calls);
    }

    /// <summary>Other top-level-only declarations keep the existing generic structural code.</summary>
    /// <param name="declaration">The misplaced declaration.</param>
    [Theory]
    [InlineData("output")]
    [InlineData("decimal-format")]
    public void OtherMisplacedDeclarationsKeepGenericCode(string declaration)
    {
        var resolver = new MemoryResolver();
        var error = Assert.Throws<InvalidOperationException>(() => Compiler(resolver)
            .Compile(Stylesheet(Template("main", new XElement(Xsl + declaration)))));
        Assert.Equal("XTSE0010", LeadingCode(error));
    }

    /// <summary>A missing href on a top-level directive retains its independent error code.</summary>
    /// <param name="directive">The incomplete module directive.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void MissingTopLevelHrefKeepsGenericCode(string directive)
    {
        var resolver = new MemoryResolver();
        var error = Assert.Throws<InvalidOperationException>(() => Compiler(resolver)
            .Compile(Stylesheet(new XElement(Xsl + directive), Template("main"))));
        Assert.Equal("XTSE0010", LeadingCode(error));
        Assert.Equal(0, resolver.Calls);
    }

    private static string LeadingCode(Exception error) => error.Message.Split(':', 2)[0];
    private static XsltCompiler Compiler(MemoryResolver resolver) => new() { UriResolver = resolver };
    private static XElement Directive(string name) => new(Xsl + name, new XAttribute("href", "module.xsl"));
    private static XElement Template(string name, params object[] content) => new(Xsl + "template", new XAttribute("name", name), content);
    private static XDocument Stylesheet(params object[] content) => new(new XElement(Xsl + "stylesheet",
        new XAttribute("version", "3.0"), new XAttribute(XNamespace.Xmlns + "xsl", Xsl.NamespaceName),
        new XElement(Xsl + "output", new XAttribute("omit-xml-declaration", "yes")), content));

    private sealed class MemoryResolver : IXsltUriResolver
    {
        internal int Calls { get; private set; }

        /// <summary>Returns the in-memory module and records the resolution request.</summary>
        /// <param name="href">The requested module reference.</param>
        /// <param name="baseUri">The caller's base URI.</param>
        /// <returns>A fresh module containing the helper template.</returns>
        public XDocument Resolve(string href, string? baseUri)
        {
            Calls++;
            return Stylesheet(Template("helper", new XElement(Xsl + "text", "module")));
        }
    }
}
