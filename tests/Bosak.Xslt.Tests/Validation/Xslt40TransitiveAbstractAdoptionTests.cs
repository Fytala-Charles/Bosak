// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 October 2026
// PURPOSE              : Tests transitive abstract optional adoption and concrete replacement binding.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 15-10-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 15-10-2026     | Verify explicit principal-package lookup and public-original lookup isolation.             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================


using System.Xml.Linq;
using Bosak.Xslt.Api;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Verifies visibility, metadata and runtime binding across abstract adoption edges.</summary>
[Collection("XSLT package registry")]
public class Xslt40TransitiveAbstractAdoptionTests
{
    private const string LibraryA = """
        <xsl:package xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:transitive' version='4.0' name='urn:transitive:A' package-version='1.0'>
          <xsl:function name='p:f' visibility='abstract'><xsl:param name='a'/><xsl:param name='b' required='no' select="'B'"/><xsl:param name='c' required='no' select="'C'"/></xsl:function>
          <xsl:function name='p:from-a' visibility='public'><xsl:sequence select="p:f('A')"/></xsl:function>
        </xsl:package>
        """;
    private const string LibraryB = """
        <xsl:package xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:transitive' version='4.0' name='urn:transitive:B' package-version='1.0'>
          <xsl:use-package name='urn:transitive:A' package-version='1.0'>
            <xsl:accept component='function' names='p:f#3' visibility='abstract'/>
            <xsl:accept component='function' names='p:from-a#0' visibility='public'/>
          </xsl:use-package>
          <xsl:function name='p:from-b' visibility='public'><xsl:sequence select="p:f('A')"/></xsl:function>
        </xsl:package>
        """;
    private static string Principal(string package, string call, string body = "$x || $y || $z", string extra = "") => """
        <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:transitive' version='4.0'>
          <xsl:use-package name='urn:transitive:PACKAGE' package-version='1.0'><xsl:override>
            <xsl:function name='p:f' visibility='public'><xsl:param name='x'/><xsl:param name='y' required='no' select="'X'"/><xsl:param name='z' required='no' select="'Y'"/><xsl:sequence select="BODY"/></xsl:function>
          </xsl:override></xsl:use-package>
          <xsl:template match='/'><out><xsl:value-of select="CALL"/></out></xsl:template>
        </xsl:stylesheet>
        """.Replace("PACKAGE",package).Replace("BODY",body).Replace("CALL",call).Replace("<xsl:template match", extra + "<xsl:template match");
    private static string Run(string principal, bool compileOnly = false, string libraryA = LibraryA, string libraryB = LibraryB)
    {
        XsltFunctionLibrary.RegisterPackage("urn:transitive:A","1.0","file:///transitive/a.xsl");
        XsltFunctionLibrary.RegisterPackage("urn:transitive:B","1.0","file:///transitive/b.xsl");
        try
        {
            var compiled = new XsltCompiler { UriResolver = new Resolver(libraryA, libraryB) }.Compile(principal,"file:///transitive/c.xsl");
            return compileOnly ? "" : XElement.Parse(compiled.TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value;
        }
        finally { XsltFunctionLibrary.ClearPackages(); }
    }
    /// <summary>The integrated direct-family behavior is the control.</summary>
    [Fact]
    public void DirectControl() => Assert.Equal("AXY",Run(Principal("A","p:f('A')")));

    /// <summary>Transitive calls should use the concrete replacement at every layer.</summary>
    /// <param name="call">The principal, A or B call path.</param>
    /// <param name="expected">The replacement's values.</param>
    [Theory]
    [InlineData("p:f('A')","AXY")]
    [InlineData("p:f('A','D')","ADY")]
    [InlineData("'A' => p:f(c := 'E')","AXE")]
    [InlineData("(p:f#1)('A')","AXY")]
    [InlineData("(p:f#2)('A','D')","ADY")]
    [InlineData("let $f := p:f(?, 'D') return $f('A')","ADY")]
    [InlineData("function-lookup(QName('urn:transitive','f'),1)('A')","AXY")]
    [InlineData("p:f('A','D','E')","ADE")]
    [InlineData("p:f(a := 'A', c := 'E')","AXE")]
    [InlineData("p:from-a()","AXY")]
    [InlineData("p:from-b()","AXY")]
    public void TransitiveCalls(string call,string expected) => Assert.Equal(expected,Run(Principal("B",call)));

    /// <summary>The adopted abstract original still has no body.</summary>
    [Fact]
    public void TransitiveOriginalIsStaticError() => Assert.Contains("XTSE3075",Assert.ThrowsAny<Exception>(()=>Run(Principal("B","'unused'","xsl:original($x)"),true)).Message);

    /// <summary>Compatibility checking must find the adopted declaration first.</summary>
    [Fact]
    public void TransitiveMinimumArityError() => Assert.Contains("XTSE3070",Assert.ThrowsAny<Exception>(()=>Run(Principal("B","'unused'").Replace("name='y' required='no' select=\"'X'\"","name='y'"),true)).Message);

    /// <summary>Semantic original references are static errors through adoption.</summary>
    /// <param name="body">The expression referencing the abstract original.</param>
    [Theory]
    [InlineData("xsl:original#1")]
    [InlineData("xsl:original(?, 'D')")]
    [InlineData("if (false()) then xsl:original($x) else 'ok'")]
    [InlineData("function() { xsl:original($x) }")]
    public void TransitiveOriginalFormsAreStaticErrors(string body) => Assert.Contains("XTSE3075", Assert.ThrowsAny<Exception>(() => Run(Principal("B", "'unused'", body), true)).Message);

    /// <summary>Uncalled replacement defaults cannot reference an abstract original.</summary>
    [Fact]
    public void TransitiveOriginalDefaultIsStaticError() => Assert.Contains("XTSE3075", Assert.ThrowsAny<Exception>(() => Run(Principal("B", "'unused'").Replace("select=\"'X'\"", "select=\"xsl:original('A')\""), true)).Message);

    /// <summary>Replacement defaults keep the principal's private functions and globals.</summary>
    /// <param name="call">The package initiating the call.</param>
    [Theory]
    [InlineData("p:f('A')")]
    [InlineData("p:from-a()")]
    [InlineData("p:from-b()")]
    public void ReplacementDefaultsKeepPrincipalScope(string call)
    {
        const string extra = "<xsl:variable name='suffix' select=\"'LOCAL'\"/><xsl:function name='p:helper' visibility='private'><xsl:sequence select='$suffix'/></xsl:function>";
        Assert.Equal("ALOCALY", Run(Principal("B", call, extra: extra).Replace("select=\"'X'\"", "select='p:helper()'")));
    }

    /// <summary>Original keyword metadata remains visible inside the intermediate package.</summary>
    [Fact]
    public void IntermediateKeywordCallUsesOriginalNames() => Assert.Equal("AXE", Run(Principal("B", "p:from-b()"), libraryB: LibraryB.Replace("p:f('A')", "p:f(a := 'A', c := 'E')")));

    /// <summary>The replacement's implementation parameter names are not public keywords.</summary>
    [Fact]
    public void ImplementationKeywordIsRejected() => Assert.Contains("XPST0017", Assert.ThrowsAny<Exception>(() => Run(Principal("B", "p:f(x := 'A')"))).Message);

    /// <summary>A type mismatch retains the signature error through adoption.</summary>
    [Fact]
    public void TransitiveTypeMismatchIsRejected() => Assert.Contains("XTSE3070", Assert.ThrowsAny<Exception>(() => Run(Principal("B", "'unused'").Replace("name='x'", "name='x' as='element()'"), true)).Message);

    /// <summary>The original upper arity remains the component identifier.</summary>
    [Fact]
    public void TransitiveReducedUpperBoundIsRejected() => Assert.Contains("XTSE3058", Assert.ThrowsAny<Exception>(() => Run(Principal("B", "'unused'").Replace("<xsl:param name='z' required='no' select=\"'Y'\"/>", "").Replace("$x || $y || $z", "$x || $y"), true)).Message);

    private static string WithoutOverride(string call, bool acceptAbstract = false)
    {
        var doc = XDocument.Parse(Principal("B", call));
        XNamespace xsl = "http://www.w3.org/1999/XSL/Transform";
        doc.Descendants(xsl + "override").Remove();
        if (acceptAbstract) doc.Descendants(xsl + "use-package").Single().Add(XElement.Parse("<xsl:accept xmlns:xsl='http://www.w3.org/1999/XSL/Transform' component='function' names='p:f#3' visibility='abstract'/>"));
        return doc.ToString();
    }

    /// <summary>An unresolved explicitly accepted abstract stays a top-level static error.</summary>
    [Fact]
    public void UnresolvedAdoptedAbstractIsRejected() => Assert.Contains("XTSE3080", Assert.ThrowsAny<Exception>(() => Run(WithoutOverride("'unused'", true), true)).Message);

    /// <summary>A hidden absent implementation remains a dynamic error through its wrapper.</summary>
    [Fact]
    public void HiddenAbsentImplementationRemainsDynamicError() => Assert.Contains("XTDE3052", Assert.ThrowsAny<Exception>(() => Run(WithoutOverride("p:from-a()"))).Message);

    /// <summary>Hidden adoption does not provide an overridable abstract component.</summary>
    [Fact]
    public void HiddenAdoptionCannotBeOverridden() => Assert.Contains("XTSE3060", Assert.ThrowsAny<Exception>(() => Run(Principal("B", "'unused'"), true, libraryB: LibraryB.Replace("visibility='abstract'", "visibility='hidden'"))).Message);

    /// <summary>Private helpers from the original library are not exported by adoption.</summary>
    [Fact]
    public void PrivateLibraryHelperStaysHidden()
    {
        const string helper = "<xsl:function name='p:secret' visibility='private'><xsl:sequence select=\"'SECRET'\"/></xsl:function>";
        Assert.Equal("true", Run(Principal("B", "empty(function-lookup(QName('urn:transitive','secret'),0))"), libraryA: LibraryA.Replace("</xsl:package>", helper + "</xsl:package>")));
    }

    /// <summary>Package-local lookup does not dynamically expose an abstract declaration.</summary>
    /// <param name="library">The declaring or adopting package.</param>
    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    public void AbstractLookupRemainsEmptyInLibrary(string library)
    {
        const string lookup = "string(empty(function-lookup(QName('urn:transitive','f'),1)))";
        Assert.Equal("true", Run(Principal("B", library == "A" ? "p:from-a()" : "p:from-b()"),
            libraryA: library == "A" ? LibraryA.Replace("p:f('A')", lookup) : LibraryA,
            libraryB: library == "B" ? LibraryB.Replace("p:f('A')", lookup) : LibraryB));
    }

    /// <summary>Named and partial items keep the transitive concrete binding after leaving a library.</summary>
    /// <param name="library">The function item's creator.</param>
    /// <param name="item">The statically bound item expression.</param>
    /// <param name="expected">The concrete replacement result.</param>
    [Theory]
    [InlineData("A", "p:f#1", "AXY")]
    [InlineData("B", "p:f#1", "AXY")]
    [InlineData("A", "p:f(?, 'D')", "ADY")]
    [InlineData("B", "p:f(?, 'D')", "ADY")]
    public void EscapingNamedAndPartialItems(string library, string item, string expected)
    {
        var call = library == "A" ? "let $f := p:from-a() return $f('A')" : "let $f := p:from-b() return $f('A')";
        Assert.Equal(expected, Run(Principal("B", call),
            libraryA: library == "A" ? LibraryA.Replace("p:f('A')", item) : LibraryA,
            libraryB: library == "B" ? LibraryB.Replace("p:f('A')", item) : LibraryB));
    }

    /// <summary>Exposure cannot make a declared abstract function concrete.</summary>
    [Fact]
    public void AbstractExposureCannotMakeConcrete()
    {
        const string expose = "<xsl:expose component='function' names='p:f#3' visibility='public'/>";
        Assert.Contains("XTSE3010", Assert.ThrowsAny<Exception>(() => new XsltCompiler().Compile(
            LibraryA.Replace("</xsl:package>", expose + "</xsl:package>"), "file:///transitive/exposure-control.xsl")).Message);
    }

    /// <summary>Unresolved acceptance without an explicit upper arity still finds the adopted family.</summary>
    [Fact]
    public void UnresolvedAcceptanceWithoutArityIsRejected() => Assert.Contains("XTSE3080", Assert.ThrowsAny<Exception>(() => Run(WithoutOverride("'unused'", true).Replace("p:f#3", "p:f"), true)).Message);

    /// <summary>Return metadata remains compatible through an adoption edge.</summary>
    [Fact]
    public void TransitiveReturnTypeMismatchIsRejected() => Assert.Contains("XTSE3070", Assert.ThrowsAny<Exception>(() => Run(Principal("B", "'unused'").Replace("name='p:f' visibility='public'", "name='p:f' visibility='public' as='element()'"), true)).Message);

    /// <summary>Omitting abstract acceptance preserves the default hidden boundary.</summary>
    [Fact]
    public void DefaultHiddenAdoptionCannotBeOverridden() => Assert.Contains("XTSE3060", Assert.ThrowsAny<Exception>(() => Run(Principal("B", "'unused'"), true,
        libraryB: LibraryB.Replace("<xsl:accept component='function' names='p:f#3' visibility='abstract'/>", ""))).Message);

    private static string ExplicitPrincipal(string principal)
    {
        var doc = XDocument.Parse(principal);
        doc.Root!.Name = XName.Get("package", "http://www.w3.org/1999/XSL/Transform");
        doc.Root.SetAttributeValue("name", "urn:transitive:C");
        doc.Root.SetAttributeValue("package-version", "1.0");
        doc.Root.SetAttributeValue("declared-modes", "no");
        return doc.ToString();
    }

    /// <summary>Explicit principal packages preserve static and dynamic concrete bindings.</summary>
    /// <param name="call">The principal or library call path.</param>
    [Theory]
    [InlineData("p:f('A')")]
    [InlineData("p:from-a()")]
    [InlineData("p:from-b()")]
    [InlineData("function-lookup(QName('urn:transitive','f'),1)('A')")]
    public void ExplicitPrincipalPackageKeepsConcreteBinding(string call) => Assert.Equal("AXY", Run(ExplicitPrincipal(Principal("B", call))));

    /// <summary>Lookup in a declaring library still selects its own public declaration.</summary>
    [Fact]
    public void PublicOriginalLookupRemainsInDeclaringLibrary()
    {
        var original = LibraryA.Replace("visibility='abstract'", "visibility='public'")
            .Replace("select=\"'C'\"/></xsl:function>", "select=\"'C'\"/><xsl:sequence select=\"'BASE'\"/></xsl:function>")
            .Replace("p:f('A')", "function-lookup(QName('urn:transitive','f'),1)('A')");
        Assert.Equal("BASE", Run(ExplicitPrincipal(Principal("A", "p:from-a()")), libraryA: original));
    }

    private sealed class Resolver(string libraryA, string libraryB) : IXsltUriResolver
    {
        /// <inheritdoc/>
        public XDocument Resolve(string href,string? baseUri) => XDocument.Parse(href.EndsWith("a.xsl",StringComparison.Ordinal) ? libraryA : libraryB);
    }
}
