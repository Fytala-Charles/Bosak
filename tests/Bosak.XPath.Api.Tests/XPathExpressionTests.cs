// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 09 oktober 2026
// PURPOSE              : Unit tests for the version-neutral XPathExpression facade
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 09-10-2026     | Creation: v1.0.0 API naming facade — 3.1 default, 4.0 opt-in, node evaluation, error      |
//                      |                  |       |                | propagation                                                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Api.Tests;

public class XPathExpressionTests
{
    private static XdmValue Eval(string xpath)
        => XPathExpression.Compile(xpath).Evaluate(new EvaluationContext());

    [Fact]
    public void Compile_DefaultsToXPath31()
    {
        Assert.Equal("3", Eval("1 + 2").ToString());
    }

    [Fact]
    public void Compile_OptionsOverload_MatchesDefault()
    {
        var expr = XPathExpression.Compile("count(('a', 'b'))", CompileOptions.Default);
        Assert.Equal("2", expr.Evaluate(new EvaluationContext()).ToString());
    }

    [Fact]
    public void Evaluate_ContextItem_PathSelection()
    {
        var doc = XDocument.Parse("<root><item id='a'/><item id='b'/></root>");
        var expr = XPathExpression.Compile("/root/item/@id");
        var result = expr.Evaluate(new XDocumentNode(doc.Root!));
        Assert.True(result.IsSequence);
    }

    [Fact]
    public void EvaluateNodes_ReturnsSelectedNodes()
    {
        var doc = XDocument.Parse("<root><item id='a'/><item id='b'/></root>");
        var expr = XPathExpression.Compile("/root/item");
        var nodes = expr.EvaluateNodes(new XDocumentNode(doc.Root!));
        Assert.True(nodes.TryGetLength(out var length));
        Assert.Equal(2, length);
    }

    [Fact]
    public void EvaluateNodes_NonNodeResult_IsEmpty()
    {
        var expr = XPathExpression.Compile("1 + 1");
        var nodes = expr.EvaluateNodes(new XDocumentNode(XDocument.Parse("<root/>").Root!));
        Assert.True(nodes.TryGetLength(out var length));
        Assert.Equal(0, length);
    }

    [Fact]
    public void Compile_XPath40Compatibility_OptsInto40Surfaces()
    {
        // fn:replicate is a 4.0-only function: behind the gate it compiles and runs.
        var expr = XPathExpression.Compile(
            "fn:string-join(fn:replicate('x', 3), ' ')",
            new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        Assert.Equal("x x x", expr.Evaluate(new EvaluationContext()).ToString());
    }

    [Fact]
    public void Compile_XPath40FunctionIn31Mode_ThrowsXpst0017()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => XPathExpression.Compile("fn:replicate('x', 3)"));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Compile_EmptyExpression_ThrowsParseException()
    {
        Assert.Throws<XPathParseException>(() => XPathExpression.Compile("   "));
    }

    [Fact]
    public void Compile_NamespacesOption_IsHonored()
    {
        var expr = XPathExpression.Compile(
            "x:count((1, 2, 3))",
            new CompileOptions
            {
                Namespaces = new Dictionary<string, string> { ["x"] = "http://www.w3.org/2005/xpath-functions" }
            });
        Assert.Equal("3", expr.Evaluate(new EvaluationContext()).ToString());
    }

    [Fact]
    public void Evaluate_EvaluationContext_IsPropagated()
    {
        var expr = XPathExpression.Compile("$v * 2");
        var ctx = new EvaluationContext().WithVariable("v", XdmValue.FromInteger(21));
        Assert.Equal("42", expr.Evaluate(ctx).ToString());
    }
}
