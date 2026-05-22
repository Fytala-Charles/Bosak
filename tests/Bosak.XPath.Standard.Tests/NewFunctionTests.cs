using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

public class NewFunctionTests
{
    [Fact]
    public void FunctionName_NamedFunction()
    {
        var expr = XPath31Expression.Compile("function-name(xs:int#1)");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.Equal("int", result.QNameValue.LocalName);
        Assert.Equal("http://www.w3.org/2001/XMLSchema", result.QNameValue.NamespaceUri);
    }

    [Fact]
    public void FunctionName_InlineFunction_ReturnsEmpty()
    {
        var expr = XPath31Expression.Compile("function-name(function($x) { $x })");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.True(result.IsUndefined);
    }

    [Fact]
    public void NormalizeUnicode_NFC()
    {
        // é can be represented as U+00E9 (NFC) or U+0065 U+0301 (NFD)
        var expr = XPath31Expression.Compile("normalize-unicode(\"\u0065\u0301\", \"NFC\")");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.Equal("\u00E9", result.ToString());
    }

    [Fact]
    public void NormalizeUnicode_DefaultIsNFC()
    {
        var expr = XPath31Expression.Compile("normalize-unicode(\"\u0065\u0301\")");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.Equal("\u00E9", result.ToString());
    }

    [Fact]
    public void AnalyzeString_StringValue()
    {
        var expr = XPath31Expression.Compile("let $result := analyze-string(\"banana\", \"(b)(anana)\") return string($result)");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.Equal("banana", result.ToString());
    }
}
