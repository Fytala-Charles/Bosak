// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verify fixed namespace resolution for named type linkage across inspected modules.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Tests type aliases, module precedence and controlled snapshot validation.</summary>
public class Xslt40AuthoringTypeLinkageTests
{
    private static readonly Uri MainUri = new("file:///linked/main.xsl");
    private static string Sheet(string body, string bindings = "#standard t=urn:types") => "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' fixed-namespaces='" + bindings + "'>" + body + "</xsl:stylesheet>";
    private static string Output(string type = "t:value") => "<xsl:template match='/'><out><xsl:value-of select='7 instance of " + type + "'/></out></xsl:template>";
    private static AuthoringSnapshot Inspect(string text, Resolver resolver)
    {
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!, resolver, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(result.IsSuccess);
        return result.Snapshot!;
    }
    private static void AssertValid(string main, Dictionary<string, string> modules, string expected = "true")
    {
        var resolver = new Resolver(modules);
        var snapshot = Inspect(main, resolver);
        int requests = resolver.Requests;
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(requests, resolver.Requests);
        Assert.Equal(Encoding.UTF8.GetBytes(main), snapshot.ExportOriginal(MainUri));
        foreach (var module in snapshot.Modules.Where(m => !m.IsPrincipal))
            Assert.Equal(Encoding.UTF8.GetBytes(modules[module.ModuleUri.Segments.Last()]), snapshot.ExportOriginal(module.ModuleUri));
        var compiler = new Api.XsltCompiler { UriResolver = new AuthoringModuleUriResolverBridge(new Resolver(modules)) };
        var output = compiler.Compile(main, MainUri.AbsoluteUri).TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));
        Assert.Equal(expected, XElement.Parse(output).Value);
    }

    /// <summary>Included and imported alias declarations are visible in principal expressions.</summary>
    /// <param name="kind">The linkage instruction.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void LinkedAliasWorks(string kind)
        => AssertValid(Sheet("<xsl:" + kind + " href='types.xsl'/>" + Output()), new() { ["types.xsl"] = Sheet("<xsl:item-type name='t:value' as='xs:integer'/>") });

    /// <summary>Cross-module forward references use each declaration's namespace bindings.</summary>
    [Fact]
    public void CrossModuleForwardReferenceWorks()
        => AssertValid(Sheet("<xsl:include href='a.xsl'/><xsl:include href='b.xsl'/>" + Output()), new()
        {
            ["a.xsl"] = Sheet("<xsl:item-type name='q:value' as='q:base'/>", "#standard q=urn:types"),
            ["b.xsl"] = Sheet("<xsl:item-type name='t:base' as='xs:integer'/>")
        });

    /// <summary>The principal module overrides lower-precedence alias definitions.</summary>
    [Fact]
    public void PrincipalOverridesImport()
        => AssertValid(Sheet("<xsl:import href='types.xsl'/><xsl:item-type name='t:value' as='xs:integer'/>" + Output()), new() { ["types.xsl"] = Sheet("<xsl:item-type name='t:value' as='xs:string'/>") });

    /// <summary>Later sibling imports have higher precedence.</summary>
    [Fact]
    public void LaterImportWins()
        => AssertValid(Sheet("<xsl:import href='a.xsl'/><xsl:import href='b.xsl'/>" + Output()), new()
        {
            ["a.xsl"] = Sheet("<xsl:item-type name='t:value' as='xs:string'/>") ,
            ["b.xsl"] = Sheet("<xsl:item-type name='t:value' as='xs:integer'/>")
        });

    /// <summary>Imports inside an include have lower precedence than its shared parent.</summary>
    [Fact]
    public void IncludeKeepsParentPrecedence()
        => AssertValid(Sheet("<xsl:include href='a.xsl'/><xsl:item-type name='t:value' as='xs:integer'/>" + Output()), new()
        {
            ["a.xsl"] = Sheet("<xsl:import href='b.xsl'/>") ,
            ["b.xsl"] = Sheet("<xsl:item-type name='t:value' as='xs:string'/>")
        });

    /// <summary>Duplicate declarations in includes produce a source-local static error.</summary>
    /// <param name="repeat">Whether the same source is included twice.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DuplicateIncludesAreRejected(bool repeat)
    {
        var second = repeat ? "a.xsl" : "b.xsl";
        var main = Sheet("<xsl:include href='a.xsl'/><xsl:include href='" + second + "'/>" + Output());
        var modules = new Dictionary<string, string> { ["a.xsl"] = Sheet("<xsl:item-type name='t:value' as='xs:integer'/>"), ["b.xsl"] = Sheet("<xsl:item-type name='t:value' as='xs:string'/>") };
        var result = XsltValidation.Validate(Inspect(main, new Resolver(modules)));
        var diagnostic = Assert.Single(result.Diagnostics, d => d.Code == "XTSE4030");
        Assert.NotEqual(MainUri, diagnostic.ModuleUri);
        Assert.NotNull(diagnostic.Range);
    }

    /// <summary>Cross-module cycles report the definition's owning module.</summary>
    [Fact]
    public void AliasCycleIsReportedOnce()
    {
        var main = Sheet("<xsl:include href='a.xsl'/><xsl:include href='b.xsl'/>" + Output());
        var modules = new Dictionary<string, string> { ["a.xsl"] = Sheet("<xsl:item-type name='t:value' as='t:base'/>"), ["b.xsl"] = Sheet("<xsl:item-type name='t:base' as='t:value'/>") };
        var diagnostic = Assert.Single(XsltValidation.Validate(Inspect(main, new Resolver(modules))).Diagnostics, d => d.Code == "XTSE4035");
        Assert.NotEqual(MainUri, diagnostic.ModuleUri);
        Assert.NotNull(diagnostic.Range);
    }

    /// <summary>Principal as slots use the registry from included type declarations.</summary>
    [Fact]
    public void LinkedDeclarationTypeWorks()
        => AssertValid(Sheet("<xsl:include href='types.xsl'/><xsl:param name='p' as='t:value?'/>" + Output()), new() { ["types.xsl"] = Sheet("<xsl:item-type name='t:value' as='xs:integer'/>") });

    /// <summary>Unknown references are rejected using the linked host context.</summary>
    [Fact]
    public void UnknownLinkedTypeIsRejected()
    {
        var main = Sheet("<xsl:include href='types.xsl'/><xsl:param name='p' as='t:missing?'/>" + Output());
        var modules = new Dictionary<string, string> { ["types.xsl"] = Sheet("<xsl:item-type name='t:value' as='xs:integer'/>") };
        Assert.Contains(XsltValidation.Validate(Inspect(main, new Resolver(modules))).Diagnostics, d => d.Code == "XPST0051" && d.ModuleUri == MainUri && d.Range is not null);
    }


    /// <summary>The importing module outranks its own imported definitions.</summary>
    [Fact]
    public void ImportParentOutranksNestedImport()
        => AssertValid(Sheet("<xsl:import href='a.xsl'/>" + Output()), new()
        {
            ["a.xsl"] = Sheet("<xsl:import href='b.xsl'/><xsl:item-type name='t:value' as='xs:integer'/>") ,
            ["b.xsl"] = Sheet("<xsl:item-type name='t:value' as='xs:string'/>")
        });

    /// <summary>Known record identities can be referenced from another module.</summary>
    [Fact]
    public void LinkedRecordIdentityWorks()
        => AssertValid(Sheet("<xsl:include href='types.xsl'/><xsl:item-type name='t:value' as='t:r'/>" + Output()), new() { ["types.xsl"] = Sheet("<xsl:record-type name='t:r'/>") }, "false");

    /// <summary>Declaration namespace documents are captured once across the graph.</summary>
    [Fact]
    public void LinkedNamespaceDocumentIsCapturedOnce()
        => AssertValid(Sheet("<xsl:include href='types.xsl'/>" + Output()), new()
        {
            ["types.xsl"] = Sheet("<xsl:item-type name='q:value' as='xs:integer'/>", "#standard bindings.xml"),
            ["bindings.xml"] = "<bindings xmlns:q='urn:types'/>"
        });

    /// <summary>Invalid definitions receive one diagnostic in the included source.</summary>
    [Fact]
    public void UnknownDefinitionReportsOwningModule()
    {
        var main = Sheet("<xsl:include href='types.xsl'/>" + Output());
        var modules = new Dictionary<string, string> { ["types.xsl"] = Sheet("<xsl:item-type name='t:value' as='t:missing'/>") };
        var diagnostic = Assert.Single(XsltValidation.Validate(Inspect(main, new Resolver(modules))).Diagnostics, d => d.Code == "XPST0051");
        Assert.Equal(new Uri("file:///linked/types.xsl"), diagnostic.ModuleUri);
        Assert.NotNull(diagnostic.Range);
    }

    /// <summary>Conditional module edges retain an explicit linkage coverage gap.</summary>
    [Fact]
    public void ConditionalLinkageRemainsDeferred()
    {
        var main = Sheet("<xsl:include href='types.xsl' use-when='1 eq 1'/>" + Output());
        var modules = new Dictionary<string, string> { ["types.xsl"] = Sheet("<xsl:item-type name='t:value' as='xs:integer'/>") };
        Assert.Equal(XsltValidationOutcome.UnsupportedCoverage, XsltValidation.Validate(Inspect(main, new Resolver(modules))).Outcome);
    }

    /// <summary>Multiple import occurrences reuse source bytes while preserving their precedence.</summary>
    [Fact]
    public void RepeatedImportSourceWorks()
        => AssertValid(Sheet("<xsl:import href='types.xsl'/><xsl:import href='types.xsl'/>" + Output()), new() { ["types.xsl"] = Sheet("<xsl:item-type name='t:value' as='xs:integer'/>") });


    /// <summary>An include inside an earlier import inherits that import's precedence.</summary>
    [Fact]
    public void ImportedIncludeKeepsItsParentRank()
        => AssertValid(Sheet("<xsl:import href='a.xsl'/><xsl:import href='b.xsl'/>" + Output()), new()
        {
            ["a.xsl"] = Sheet("<xsl:include href='c.xsl'/>") ,
            ["b.xsl"] = Sheet("<xsl:item-type name='t:value' as='xs:integer'/>") ,
            ["c.xsl"] = Sheet("<xsl:item-type name='t:value' as='xs:string'/>")
        });

    private sealed class Resolver(Dictionary<string, string> modules) : IAuthoringModuleResolver
    {
        /// <summary>Gets the number of caller-controlled resource requests.</summary>
        public int Requests { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Requests++;
            if (!modules.TryGetValue(absoluteUri.Segments.Last(), out var text)) return null;
            Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), absoluteUri, out var source, out _));
            return source;
        }
    }
}
