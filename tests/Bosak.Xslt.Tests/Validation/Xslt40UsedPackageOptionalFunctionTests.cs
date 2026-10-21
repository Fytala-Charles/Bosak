// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 October 2026
// PURPOSE              : Tests optional function arguments across used-package boundaries.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 14-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2    | 15-10-2026     | Verify direct and transitive optional override execution.                                |
//                      | Charles Korthout | 0.3   | 18-10-2026     | Correct build-warning documentation and nullable-flow diagnostics.                     |
//                      |==================|=======|================|=========================================================================================

// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Verifies optional arities, declaring defaults and package visibility.</summary>
[Collection("XSLT package registry")]
public class Xslt40UsedPackageOptionalFunctionTests
{
    private const string Params = "<xsl:param name='a'/><xsl:param name='b' required='no' select=\"'B'\"/><xsl:param name='c' required='no' select=\"'C'\"/>";
    private static string Function(string parameters = Params, string body = "$a || '|' || $b || '|' || $c", string visibility = "public") =>
        "<xsl:function name='p:f'" + (visibility.Length == 0 ? "" : " visibility='" + visibility + "'") + ">" + parameters + "<xsl:sequence select=\"" + body + "\"/></xsl:function>";
    private static string Package(string declarations, string name = "urn:optional-library") =>
        "<xsl:package xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:functions' xmlns:xs='http://www.w3.org/2001/XMLSchema' name='" + name + "' package-version='1.0' version='4.0'>" + declarations + "</xsl:package>";
    private static string PrincipalStylesheet(string call, string accepts = "", string extra = "", string version = "4.0") =>
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:functions' xmlns:xs='http://www.w3.org/2001/XMLSchema' version='" + version + "' exclude-result-prefixes='#all'><xsl:use-package name='urn:optional-library' package-version='1.0'>" + accepts + "</xsl:use-package>" + extra + "<xsl:template match='/'><out><xsl:value-of select=\"" + call + "\"/></out></xsl:template></xsl:stylesheet>";
    private static string Run(string main, string library, string input = "<root/>", string? inner = null)
    {
        XsltFunctionLibrary.RegisterPackage("urn:optional-library", "1.0", "file:///optional/library.xsl");
        if (inner is not null) XsltFunctionLibrary.RegisterPackage("urn:optional-inner", "1.0", "file:///optional/inner.xsl");
        try
        {
            var resolver = new Resolver(library, inner);
            return XElement.Parse(new XsltCompiler { UriResolver = resolver }.Compile(main,"file:///optional/main.xsl").TransformToString(new XDocumentNode(XDocument.Parse(input)))).Value;
        }
        finally { XsltFunctionLibrary.ClearPackages(); }
    }

    /// <summary>All ordinary call paths bind reduced arities from the used package.</summary>
    /// <param name="call">The function invocation.</param>
    /// <param name="expected">The expected parameter values.</param>
    [Theory]
    [InlineData("p:f('A')","A|B|C")]
    [InlineData("p:f('A','X')","A|X|C")]
    [InlineData("p:f('A','X','Y')","A|X|Y")]
    [InlineData("p:f(a := 'A', c := 'Y')","A|B|Y")]
    [InlineData("'A' => p:f(c := 'Y')","A|B|Y")]
    [InlineData("(p:f#1)('A')","A|B|C")]
    [InlineData("function-lookup(QName('urn:functions','f'),1)('A')","A|B|C")]
    [InlineData("let $f := p:f(?, 'X') return $f('A')","A|X|C")]
    [InlineData("let $f := p:f#2 return $f('A','X')","A|X|C")]
    public void PublicFunctionCallPaths(string call, string expected) => Assert.Equal(expected,Run(PrincipalStylesheet(call),Package(Function())));

    /// <summary>The whole declared range is available and out-of-range lookup stays empty.</summary>
    [Fact]
    public void LookupExposesExactlyTheRange() => Assert.Equal("true true true true true true",Run(PrincipalStylesheet("(function-available('p:f',1),function-available('p:f',2),function-available('p:f',3),empty(function-lookup(QName('urn:functions','f'),0)),empty(function-lookup(QName('urn:functions','f'),4)),function-arity(p:f#1)=1)"),Package(Function())));

    /// <summary>Default evaluation sees declaring package globals, not caller globals or locals.</summary>
    [Fact]
    public void DefaultsUseDeclaringGlobals()
    {
        var library = Package("<xsl:variable name='suffix' select=\"'LIB'\"/>"+Function("<xsl:param name='a'/><xsl:param name='b' required='no' select='$suffix'/>","$a || $b"));
        var main=PrincipalStylesheet("let $suffix := 'LOCAL' return p:f('A')",extra:"<xsl:variable name='suffix' select=\"'CALLER'\"/>");
        Assert.Equal("ALIB",Run(main,library));
    }

    /// <summary>Direct defaults retain the caller focus across the package boundary.</summary>
    [Fact]
    public void DirectDefaultsUseCallerFocus()
    {
        var library=Package(Function("<xsl:param name='a' required='no' select='name(.)'/>","$a"));
        Assert.Equal("root",Run(PrincipalStylesheet("/root ! p:f()"),library));
    }

    /// <summary>Function items retain the focus where the reference was created.</summary>
    [Fact]
    public void NamedReferenceCapturesCreationFocus()
    {
        var library=Package(Function("<xsl:param name='a' required='no' select='name(.)'/>","$a"));
        Assert.Equal("root",Run(PrincipalStylesheet("let $f := /root ! p:f#0 return /root/child ! $f()"),library,"<root><child/></root>"));
    }

    /// <summary>Default constructors also observe caller focus.</summary>
    [Fact]
    public void ConstructorDefaultUsesCallerFocus()
    {
        var library=Package(Function("<xsl:param name='a' required='no'><xsl:value-of select='name(.)'/></xsl:param>","string($a)"));
        Assert.Equal("root",Run(PrincipalStylesheet("/root ! p:f()"),library));
    }

    /// <summary>Default type errors keep the XPath conversion code.</summary>
    [Fact]
    public void DefaultTypeErrorIsPreserved()
    {
        var library=Package(Function("<xsl:param name='a' required='no' as='xs:integer' select=\"'bad'\"/>","$a"));
        Assert.Contains("XPTY0004",Assert.ThrowsAny<Exception>(()=>Run(PrincipalStylesheet("p:f()"),library)).Message);
    }

    /// <summary>An optional private helper executes only in its declaring package.</summary>
    [Fact]
    public void PrivateOptionalHelperIsIsolated()
    {
        var helper="<xsl:function name='helper'><xsl:param name='a' required='no' select=\"'LIB'\"/><xsl:sequence select='$a'/></xsl:function>";
        var library=Package(helper+Function("<xsl:param name='a' required='no' select='helper()'/>","$a"));
        var caller=helper.Replace("'LIB'","'CALLER'");
        Assert.Equal("LIB CALLER",Run(PrincipalStylesheet("(p:f(),helper())",extra:caller),library));
    }

    /// <summary>Explicit accept uses the upper bound as the component identity for every arity.</summary>
    [Fact]
    public void AcceptUpperBoundAsPrivateKeepsRange() => Assert.Equal("A|B|C",Run(PrincipalStylesheet("p:f('A')","<xsl:accept component='function' names='p:f#3' visibility='private'/>"),Package(Function())));

    /// <summary>Hiding the component hides all its reduced arities.</summary>
    [Fact]
    public void HiddenComponentHidesRange() => Assert.Equal("true true true",Run(PrincipalStylesheet("(empty(function-lookup(QName('urn:functions','f'),1)),empty(function-lookup(QName('urn:functions','f'),2)),empty(function-lookup(QName('urn:functions','f'),3)))","<xsl:accept component='function' names='p:f#3' visibility='hidden'/>"),Package(Function())));

    /// <summary>A reduced arity is not the symbolic component identifier in accept.</summary>
    [Fact]
    public void AcceptReducedArityIsRejected() => Assert.Contains("XTSE3030",Assert.ThrowsAny<Exception>(()=>Run(PrincipalStylesheet("'unused'","<xsl:accept component='function' names='p:f#1' visibility='private'/>"),Package(Function()))).Message);

    /// <summary>Expose of the upper-bound identifier exports the entire range.</summary>
    [Fact]
    public void ExposeUpperBoundExportsRange() => Assert.Equal("A|B|C",Run(PrincipalStylesheet("p:f('A')"),Package(Function(visibility:"")+"<xsl:expose component='function' names='p:f#3' visibility='public'/>")));

    /// <summary>Final optional functions can be called without an override.</summary>
    [Fact]
    public void FinalOptionalFunctionExecutes() => Assert.Equal("A|B|C",Run(PrincipalStylesheet("p:f('A')"),Package(Function(visibility:"final"))));

    /// <summary>A positional call in XSLT 3.0 can use a library declared in XSLT 4.0.</summary>
    [Fact]
    public void EarlierCallerUsesLibraryDefaults() => Assert.Equal("A|B|C",Run(PrincipalStylesheet("p:f('A')",version:"3.0"),Package(Function())));

    /// <summary>Transitive used-package calls preserve the inner declaring scope.</summary>
    [Fact]
    public void NestedUsedPackageDefaultsExecute()
    {
        var inner=Package(Function(),"urn:optional-inner");
        var library=Package("<xsl:use-package name='urn:optional-inner' package-version='1.0'/><xsl:function name='p:wrapper' visibility='public'><xsl:sequence select=\"p:f('A')\"/></xsl:function>");
        Assert.Equal("A|B|C",Run(PrincipalStylesheet("p:wrapper()"),library,inner:inner));
    }

    /// <summary>Optional override linkage replaces the complete callable range.</summary>
    [Fact]
    public void OptionalOverrideExecutes()
    {
        var overrides="<xsl:override>"+Function()+"</xsl:override>";
        Assert.Equal("A|B|C",Run(PrincipalStylesheet("p:f('A')",overrides),Package(Function())));
    }

    /// <summary>Private optional helpers do not become visible to callers.</summary>
    [Fact]
    public void PrivateHelperIsAbsentFromCallerLookup()
    {
        var helper="<xsl:function name='helper'><xsl:param name='a' required='no' select=\"'LIB'\"/><xsl:sequence select='$a'/></xsl:function>";
        var library=Package(helper+Function("<xsl:param name='a' required='no' select='helper()'/>","$a"));
        Assert.Equal("LIB true true",Run(PrincipalStylesheet("(p:f(),empty(function-lookup(QName('', 'helper'),0)),empty(function-lookup(QName('', 'helper'),1)))"),library));
    }

    /// <summary>Keyword identities come from the declaring package's parameter namespaces.</summary>
    [Fact]
    public void KeywordUsesDeclaringNamespace()
    {
        var library=Package(Function("<xsl:param xmlns:q='urn:param' name='q:a'/><xsl:param name='b' required='no' select=\"'B'\"/>","$Q{urn:param}a || $b"));
        Assert.Equal("AB",Run(PrincipalStylesheet("p:f(Q{urn:param}a := 'A')"),library));
        Assert.Contains("XPST0017",Assert.ThrowsAny<Exception>(()=>Run(PrincipalStylesheet("p:f(a := 'A')"),library)).Message);
    }

    /// <summary>AVTs and predicates see the reduced package arity.</summary>
    [Fact]
    public void AvtAndPredicateUseOptionalFunction()
    {
        var text=PrincipalStylesheet("'unused'").Replace("<out><xsl:value-of select=\"'unused'\"/></out>","<out value=\"{p:f('A')}\"><xsl:value-of select=\"/root[p:f('A')='A|B|C']/@value\"/></out>");
        Assert.Equal("OK",Run(text,Package(Function()),"<root value='OK'/>"));
    }

    /// <summary>Two optional families whose callable ranges overlap remain outside this slice.</summary>
    [Fact]
    public void ConflictingUsedPackageRangesAreRejected()
    {
        var inner=Package(Function("<xsl:param name='a'/><xsl:param name='b' required='no' select=\"'INNER'\"/>","$a || $b"),"urn:optional-inner");
        var main=PrincipalStylesheet("p:f('A')",extra:"<xsl:use-package name='urn:optional-inner' package-version='1.0'/>");
        Assert.Throws<NotSupportedException>(()=>Run(main,Package(Function()),inner:inner));
    }

    /// <summary>A local declaration cannot partially replace an adopted optional family.</summary>
    [Fact]
    public void LocalOverlapWithUsedPackageRangeIsRejected()
    {
        var main=PrincipalStylesheet("p:f('A')",extra:Function("<xsl:param name='a'/><xsl:param name='b'/>","'LOCAL'"));
        Assert.Throws<NotSupportedException>(()=>Run(main,Package(Function())));
    }

    /// <summary>Hiding one adopted family resolves its collision with another used package.</summary>
    [Fact]
    public void HiddenOverlappingFamilyDoesNotConflict()
    {
        var inner=Package(Function("<xsl:param name='a'/><xsl:param name='b' required='no' select=\"'INNER'\"/>","$a || $b"),"urn:optional-inner");
        var main=PrincipalStylesheet("p:f('A')",extra:"<xsl:use-package name='urn:optional-inner' package-version='1.0'><xsl:accept component='function' names='p:f#2' visibility='hidden'/></xsl:use-package>");
        Assert.Equal("A|B|C",Run(main,Package(Function()),inner:inner));
    }

    /// <summary>Disjoint adopted arity families remain independently callable.</summary>
    [Fact]
    public void NonoverlappingFamiliesExecute()
    {
        var inner=Package(Function("<xsl:param name='a'/><xsl:param name='b'/><xsl:param name='c'/><xsl:param name='d'/>","'INNER'"),"urn:optional-inner");
        var main=PrincipalStylesheet("(p:f('A'),p:f(1,2,3,4))",extra:"<xsl:use-package name='urn:optional-inner' package-version='1.0'/>");
        Assert.Equal("A|B|C INNER",Run(main,Package(Function()),inner:inner));
    }

    /// <summary>A default cannot see a caller-local variable in place of a declaration.</summary>
    [Fact]
    public void CallerLocalDoesNotSupplyDefaultVariable()
    {
        var library=Package(Function("<xsl:param name='a' required='no' select='$callerOnly'/>","$a"));
        Assert.Contains("XPST0008",Assert.ThrowsAny<Exception>(()=>Run(PrincipalStylesheet("let $callerOnly := 'CALLER' return p:f()"),library)).Message);
    }

    /// <summary>Optional override execution also applies inside a transitively used package.</summary>
    [Fact]
    public void NestedOptionalOverrideExecutes()
    {
        var inner=Package(Function(),"urn:optional-inner");
        var library=Package("<xsl:use-package name='urn:optional-inner' package-version='1.0'><xsl:override>"+Function()+"</xsl:override></xsl:use-package>");
        Assert.Equal("A|B|C",Run(PrincipalStylesheet("p:f('A')"),library,inner:inner));
    }

    private sealed class Resolver(string library,string? inner) : IXsltUriResolver
    {
        /// <inheritdoc />
        public XDocument Resolve(string href,string? baseUri) => XDocument.Parse(href.EndsWith("inner.xsl",StringComparison.Ordinal) ? inner! : library);
    }
}
