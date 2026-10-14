// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 October 2026
// PURPOSE              : Tests private namespace-free XSLT 4.0 function resolution.
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
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Verifies namespace-free stylesheet functions and explicit namespace identity.</summary>
[Collection("XSLT package registry")]
public class Xslt40NoNamespaceFunctionTests
{
    private static IEnumerable<AuthoringNodeDescriptor> Flatten(AuthoringNodeDescriptor node)
    {
        yield return node;
        foreach (var child in node.Children)
            foreach (var descendant in Flatten(child)) yield return descendant;
    }
    private const string Divide = "<xsl:function name='divide' visibility='private'><xsl:param name='dividend'/><xsl:param name='divisor'/><xsl:sequence select='$dividend div $divisor'/></xsl:function>";
    private static string Sheet(string declaration, string call, string version = "4.0") =>
        "<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:fn='http://www.w3.org/2005/xpath-functions' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>" + declaration + "<xsl:template match='/'><out><xsl:value-of select=\"" + call + "\"/></out></xsl:template></xsl:stylesheet>";
    private static string Run(string sheet) => XElement.Parse(new XsltCompiler().Compile(sheet).TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value;
    private static XsltValidationResult Validate(string sheet) => XsltValidation.Validate(Encoding.UTF8.GetBytes(sheet), new Uri("file:///no-namespace/main.xsl"));

    /// <summary>All supported call forms identify the private no-namespace declaration.</summary>
    /// <param name="call">The expression invoking the function.</param>
    [Theory]
    [InlineData("divide(9,3)")]
    [InlineData("9 => divide(3)")]
    [InlineData("divide(divisor := 3, dividend := 9)")]
    [InlineData("9 => divide(divisor := 3)")]
    [InlineData("Q{}divide(9,3)")]
    [InlineData("Q{}divide(divisor := 3, dividend := 9)")]
    [InlineData("(divide#2)(9,3)")]
    [InlineData("(Q{}divide#2)(9,3)")]
    [InlineData("function-lookup(QName('', 'divide'),2)(9,3)")]
    [InlineData("let $f := divide(?,3) return $f(9)")]
    [InlineData("let $f := divide(?, divisor := 3) return $f(9)")]
    public void PrivateFunctionsExecute(string call)
    {
        var sheet = Sheet(Divide, call);
        Assert.Equal(XsltValidationOutcome.Valid, Validate(sheet).Outcome);
        Assert.Equal("3", Run(sheet));
    }

    /// <summary>The declared arity chooses the private function before fn fallback.</summary>
    [Fact]
    public void AritySelectsPrivateOrStandardFunction()
    {
        var declaration = "<xsl:function name='sum'><xsl:param name='a'/><xsl:param name='b'/><xsl:param name='c'/><xsl:sequence select='$a+$b+$c'/></xsl:function>";
        Assert.Equal("33", Run(Sheet(declaration,"sum(sum(10,11,12),0)")));
        Assert.Equal("33", Run(Sheet(declaration,"(sum#2)((sum#3)(10,11,12),0)")));
    }

    /// <summary>Explicit fn names bypass the private declaration, while Q{} retains no namespace.</summary>
    /// <param name="call">The collision expression.</param>
    /// <param name="expected">The expected result.</param>
    [Theory]
    [InlineData("count((1,2))", "7")]
    [InlineData("Q{}count((1,2))", "7")]
    [InlineData("fn:count((1,2))", "2")]
    [InlineData("Q{http://www.w3.org/2005/xpath-functions}count((1,2))", "2")]
    [InlineData("(count#1)((1,2))", "7")]
    [InlineData("(fn:count#1)((1,2))", "2")]
    public void ExplicitNamespacesBypassFallback(string call, string expected)
        => Assert.Equal(expected,Run(Sheet("<xsl:function name='Q{}count'><xsl:param name='items'/><xsl:sequence select='7'/></xsl:function>",call)));

    /// <summary>Defaults and optional arity references work without a namespace prefix.</summary>
    /// <param name="call">The reduced-arity call.</param>
    [Theory]
    [InlineData("greet('A')")]
    [InlineData("greet(a := 'A')")]
    [InlineData("(greet#1)('A')")]
    [InlineData("function-lookup(QName('', 'greet'),1)('A')")]
    [InlineData("let $f := greet(?, b := 'B') return $f('A')")]
    public void OptionalFunctionsExecute(string call)
        => Assert.Equal("AB",Run(Sheet("<xsl:function name='greet'><xsl:param name='a'/><xsl:param name='b' required='no' select=\"'B'\"/><xsl:sequence select='concat($a,$b)'/></xsl:function>",call)));

    /// <summary>Only the effective 4.0 private profile permits no-namespace declarations.</summary>
    /// <param name="version">The stylesheet version.</param>
    /// <param name="attributes">The declaration attributes.</param>
    [Theory]
    [InlineData("3.0", "")]
    [InlineData("2.0", "")]
    [InlineData("4.0", "version='3.0'")]
    [InlineData("4.0", "visibility='public'")]
    [InlineData("4.0", "visibility='final'")]
    [InlineData("4.0", "visibility='abstract'")]
    public void InvalidProfilesAreRejected(string version, string attributes)
    {
        var sheet=Sheet("<xsl:function name='divide' "+attributes+"/>","1",version);
        Assert.Contains(Validate(sheet).Diagnostics,d=>d.Code=="XTSE0740");
        Assert.Contains("XTSE0740",Assert.ThrowsAny<Exception>(()=>new XsltCompiler().Compile(sheet)).Message);
    }

    /// <summary>A missing explicit no-namespace function never falls back to fn.</summary>
    /// <param name="call">The explicit name expression.</param>
    [Theory]
    [InlineData("Q{}count((1,2))")]
    [InlineData("(Q{}count#1)((1,2))")]
    public void ExplicitEmptyNamespaceDoesNotFallBack(string call)
        => Assert.Contains("XPST0017",Assert.ThrowsAny<Exception>(()=>Run(Sheet("",call))).Message);

    /// <summary>Availability strings retain their standard namespace rule.</summary>
    [Fact]
    public void AvailabilityUsesExplicitEqName()
        => Assert.Equal("false true true",Run(Sheet(Divide,"(function-available('divide',2),function-available('Q{}divide',2),exists(function-lookup(QName('', 'divide'),2)))")));

    /// <summary>Supported expression edits use captured private signatures and preserve original bytes.</summary>
    [Fact]
    public void AuthoringEditRetainsSignaturesAndSource()
    {
        var uri=new Uri("file:///no-namespace/edit.xsl");
        var bytes=Encoding.UTF8.GetBytes(Sheet(Divide,"divide(9,3)"));
        Assert.True(AuthoringSource.TryCreate(bytes,uri,out var source,out _));
        var snapshot=XsltAuthoring.Inspect(source!,options:new AuthoringInspectionOptions { AttemptCompilation=false }).Snapshot!;
        var node=Flatten(snapshot.PrincipalModule.Root).Single(n=>n.ElementName?.LocalName=="value-of");
        var edit=snapshot.ProposeExpressionEdit(new AuthoringEditProposal(node.Id,"select","divide(divisor := 3, dividend := 12)"));
        Assert.True(edit.IsSuccess);
        Assert.Equal("4",Run(Encoding.UTF8.GetString(edit.Candidate!.EmittedSource.Span)));
        Assert.Equal(bytes,snapshot.ExportOriginal(uri));
    }

    private sealed class TextResolver(string text) : IXsltUriResolver
    {
        /// <inheritdoc />
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse(text);
    }

    /// <summary>Included declarations participate in the same private function scope.</summary>
    [Fact]
    public void IncludedFunctionExecutes()
    {
        var included=Sheet(Divide,"1").Replace("<xsl:template match='/'><out><xsl:value-of select=\"1\"/></out></xsl:template>","");
        var principal=Sheet("<xsl:include href='library.xsl'/>","divide(9,3)");
        var executable=new XsltCompiler { UriResolver=new TextResolver(included) }.Compile(principal,"file:///no-namespace/main.xsl");
        Assert.Equal("3",XElement.Parse(executable.TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value);
    }

    private static string Package() => "<xsl:package xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:exports' name='urn:no-namespace-package' package-version='1.0' version='4.0'>"+Divide+"<xsl:function name='p:answer' visibility='public'><xsl:sequence select='divide(9,3)'/></xsl:function></xsl:package>";
    private static string PackageCaller(string declarations,string call) => Sheet("<xsl:use-package name='urn:no-namespace-package' package-version='1.0'/>"+declarations,call).Replace("xmlns:fn=","xmlns:p='urn:exports' xmlns:fn=");

    private static string RunPackage(string caller)
    {
        XsltFunctionLibrary.RegisterPackage("urn:no-namespace-package","1.0","file:///no-namespace/library.xsl");
        try
        {
            var executable=new XsltCompiler { UriResolver=new TextResolver(Package()) }.Compile(caller);
            return XElement.Parse(executable.TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value;
        }
        finally { XsltFunctionLibrary.ClearPackages(); }
    }

    /// <summary>A used package's private function retains its own implementation.</summary>
    [Fact]
    public void UsedPackagePrivateFunctionKeepsScope()
    {
        var caller=PackageCaller(Divide.Replace("$dividend div $divisor","42"),"(p:answer(),divide(9,3))");
        Assert.Equal("3 42",RunPackage(caller));
    }

    /// <summary>Private no-namespace functions do not become visible in a using package.</summary>
    [Fact]
    public void UsedPackagePrivateFunctionIsNotExported()
    {
        var caller=PackageCaller("","(p:answer(),empty(function-lookup(QName('', 'divide'),2)))");
        Assert.Equal("3 true",RunPackage(caller));
    }

    /// <summary>Match predicates carry the same private function signatures as select expressions.</summary>
    [Fact]
    public void PatternPredicateExecutes()
    {
        var declaration="<xsl:function name='accept'><xsl:param name='node'/><xsl:sequence select='true()'/></xsl:function>";
        var sheet=Sheet(declaration,"1").Replace("match='/'","match='root[accept(.)]'");
        Assert.Equal("1",Run(sheet));
    }

    /// <summary>Attribute value templates retain the resolved no-namespace call.</summary>
    [Fact]
    public void AvtExecutes()
    {
        var sheet=Sheet(Divide,"1").Replace("<out>","<out value='{divide(9,3)}'>");
        var result=XElement.Parse(new XsltCompiler().Compile(sheet).TransformToString(new XDocumentNode(XDocument.Parse("<root/>"))));
        Assert.Equal("3",result.Attribute("value")!.Value);
    }

    /// <summary>Conditional private signatures report a coverage gap instead of selecting fn.</summary>
    [Fact]
    public void ConditionalAuthoringSignatureIsDeferred()
    {
        var sheet=Sheet("<xsl:function name='count' use-when=\"system-property('xsl:version') = '4.0'\"><xsl:param name='items'/><xsl:sequence select='7'/></xsl:function>","count((1,2))");
        Assert.Equal(XsltValidationOutcome.UnsupportedCoverage,Validate(sheet).Outcome);
    }
}
