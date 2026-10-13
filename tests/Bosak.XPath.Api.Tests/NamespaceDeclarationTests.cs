// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies expression-local XPath 4.0 namespace declarations and wildcard defaults.
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

using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Api.Tests;

/// <summary>Verifies expression-local XPath 4.0 namespace declarations and wildcard defaults.</summary>
public class NamespaceDeclarationTests
{
    private static XPath31Expression Compile(string expression, IReadOnlyDictionary<string, string>? namespaces = null)
        => XPath31Expression.Compile(expression, new CompileOptions { Compatibility = XPathCompatibility.XPath40, Namespaces = namespaces });

    /// <summary>Resolves declared function aliases through comments and escaped URI quotes.</summary>
    [Fact]
    public void FunctionAlias_WithCommentsAndQuotes()
    {
        var result = Compile("(: first (: nested :) :) declare (: binding :) namespace f = 'http://www.w3.org/2005/xpath-functions'; f:count((1,2))")
            .Evaluate(new EvaluationContext());
        Assert.Equal(2, result.IntegerValue);
        var context = new EvaluationContext().WithVariable("v", XdmValue.FromInteger(7), "urn:O'Reilly;data");
        Assert.Equal(7, Compile("declare namespace p = 'urn:O''Reilly;data'; $p:v").Evaluate(context).IntegerValue);
    }

    /// <summary>Restores overridden bindings after success and dynamic failure.</summary>
    [Fact]
    public void NamespaceOverride_IsScopedAndDoesNotMutateOptions()
    {
        var namespaces = new Dictionary<string, string> { ["p"] = "urn:old" };
        var context = new EvaluationContext().WithNamespace("p", "urn:old")
            .WithVariable("v", XdmValue.FromInteger(1), "urn:old")
            .WithVariable("v", XdmValue.FromInteger(2), "urn:new");
        var expression = Compile("declare namespace p='urn:new'; $p:v", namespaces);
        Assert.Equal(2, expression.Evaluate(context).IntegerValue);
        Assert.True(context.TryResolveNamespace("p", out var restored));
        Assert.Equal("urn:old", restored);
        Assert.Equal("urn:old", namespaces["p"]);
        Assert.ThrowsAny<Exception>(() => Compile("declare namespace p='urn:new'; error()", namespaces).Evaluate(context));
        Assert.True(context.TryResolveNamespace("p", out restored));
        Assert.Equal("urn:old", restored);
    }

    /// <summary>Sets and clears default namespaces for one evaluation without leaking context.</summary>
    [Fact]
    public void DefaultNamespace_OverridesAndRestores()
    {
        var context = new EvaluationContext { DefaultElementNamespace = "urn:host" }
            .WithFocus(XdmValue.FromNode(new XDocumentNode(XDocument.Parse("<doc><chap xmlns='urn:chap'/><chap/></doc>"))), 1, 1);
        Assert.Equal(1, Compile("declare default element namespace 'urn:chap'; count(//chap)").Evaluate(context).IntegerValue);
        Assert.Equal(1, Compile("declare default element namespace ''; count(//chap)").Evaluate(context).IntegerValue);
        Assert.Equal("urn:host", context.DefaultElementNamespace);
    }

    /// <summary>Limits wildcard defaults to element name tests and applies schema defaults to item types.</summary>
    [Fact]
    public void AnyNamespace_PreservesAttributeAndKindTestSemantics()
    {
        var context = new EvaluationContext().WithFocus(XdmValue.FromNode(new XDocumentNode(XDocument.Parse("<doc><chap xmlns='urn:a' id='1' xmlns:p='urn:p' p:id='2'/><chap xmlns='urn:b'/><chap/></doc>"))), 1, 1);
        Assert.Equal(3, Compile("declare default element namespace '##any'; count(//chap)").Evaluate(context).IntegerValue);
        Assert.Equal(1, Compile("declare default element namespace '##any'; count(//chap/@id)").Evaluate(context).IntegerValue);
        Assert.Equal(1, Compile("declare default element namespace '##any'; count(//element(chap))").Evaluate(context).IntegerValue);
        Assert.Equal(1, Compile("declare default element namespace '##any'; count(//Q{}chap)").Evaluate(context).IntegerValue);
        Assert.True(Compile("declare default element namespace '##any'; 42 instance of integer").Evaluate(context).BooleanValue);
        Assert.Equal(42, Compile("declare default element namespace '##any'; '42' cast as integer").Evaluate(context).IntegerValue);
        Assert.True(Compile("declare default element namespace '##any'; '42' castable as integer").Evaluate(context).BooleanValue);
        Assert.Equal(42, Compile("declare default element namespace '##any'; 42 treat as integer").Evaluate(context).IntegerValue);
        Assert.Equal(42, Compile("declare default element namespace '##any'; declare namespace xs='urn:other'; '42' cast as integer").Evaluate(context).IntegerValue);
    }

    /// <summary>Rejects duplicate, reserved and out-of-order declarations.</summary>
    /// <param name="expression">The declaration or reference under test.</param>
    /// <param name="code">The required static error code.</param>
    [Theory]
    [InlineData("declare namespace p='urn:a'; declare namespace p='urn:b'; 1", "XQST0033")]
    [InlineData("declare namespace xml='urn:a'; 1", "XQST0070")]
    [InlineData("declare namespace p='http://www.w3.org/XML/1998/namespace'; 1", "XQST0070")]
    [InlineData("declare default element namespace 'http://www.w3.org/2000/xmlns/'; 1", "XQST0070")]
    [InlineData("declare namespace p='urn:a'; declare default element namespace 'urn:b'; 1", "XPST0003")]
    [InlineData("declare default element namespace ''; declare default element namespace ''; 1", "XPST0003")]
    [InlineData("declare namespace p='urn:a' 1", "XPST0003")]
    [InlineData("declare namespace p:q='urn:a'; 1", "XPST0003")]
    [InlineData("declare namespace p=''; $p:v", "XPST0008")]
    [InlineData("declare namespace fn=''; fn:count(())", "XPST0081")]
    [InlineData("declare namespace p=''; p:node", "XPST0081")]
    [InlineData("declare default element namespace '##any'; 42 cast as anyAtomicType", "XPST0080")]
    [InlineData("declare default element namespace '##any'; 42 castable as anyType", "XQST0052")]
    public void InvalidDeclaration_HasExactCode(string expression, string code)
        => Assert.Contains(code, Assert.ThrowsAny<Exception>(() => Compile(expression)).Message);

    /// <summary>Keeps declarations gated to 4.0 while preserving the ordinary declare name test.</summary>
    [Fact]
    public void XPath31_RefusesPrologButAllowsDeclareName()
    {
        Assert.ThrowsAny<Exception>(() => XPath31Expression.Compile("declare namespace p='urn:a'; 1"));
        Assert.Equal(0, XPath31Expression.Compile("count(declare)")
            .Evaluate(new EvaluationContext().WithFocus(XdmValue.FromNode(new XDocumentNode(XDocument.Parse("<root/>"))), 1, 1)).IntegerValue);
    }

    /// <summary>Preserves literal default namespace handling in the frozen profile.</summary>
    [Fact]
    public void XPath31_DefaultAnyString_RemainsLiteralNamespace()
    {
        var expression = XPath31Expression.Compile("count(//chap)", new CompileOptions { DefaultElementNamespace = "##any" });
        var context = new EvaluationContext().WithFocus(XdmValue.FromNode(new XDocumentNode(XDocument.Parse("<doc><chap xmlns='##any'/><chap/></doc>"))), 1, 1);
        Assert.Equal(1, expression.Evaluate(context).IntegerValue);
    }
}
