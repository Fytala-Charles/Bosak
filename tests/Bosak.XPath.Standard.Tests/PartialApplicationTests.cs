using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

public class PartialApplicationTests
{
    [Fact]
    public void PartialApplication_Concat_WithPrefix()
    {
        var expr = XPath31Expression.Compile("let $f := fn:concat(?, '.', ?) return $f('a', 'b')");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.Equal("a.b", result.ToString());
    }

    [Fact]
    public void FunctionLookup_Concat()
    {
        var expr = XPath31Expression.Compile("let $f := function-lookup(xs:QName('fn:concat'), 3) return $f('a', '.', 'b')");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.Equal("a.b", result.ToString());
    }

    [Fact]
    public void Apply_Concat_ViaLookup()
    {
        var expr = XPath31Expression.Compile(
            "for $a in 2 to 3 return let $f := function-lookup(xs:QName('fn:concat'), $a) return apply($f, array { 1 to $a })");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.True(result.IsSequence);
        var items = new List<XdmValue>();
        foreach (var item in result.SequenceValue!)
            items.Add(item);
        Assert.Equal(2, items.Count);
        Assert.Equal("12", items[0].ToString());
        Assert.Equal("123", items[1].ToString());
    }

    [Fact]
    public void InlineFunction_ReturnsSequence()
    {
        var expr = XPath31Expression.Compile("let $f := function($a, $b) {($b, $a)} return $f(1, 2)");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.True(result.IsSequence);
        var items = new List<XdmValue>();
        foreach (var item in result.SequenceValue!)
            items.Add(item);
        Assert.Equal(2, items.Count);
        Assert.Equal("2", items[0].ToString());
        Assert.Equal("1", items[1].ToString());
    }

    [Fact]
    public void FoldLeft_ReverseSequence()
    {
        var expr = XPath31Expression.Compile("fold-left(1 to 5, (), function($a, $b) {($b, $a)})");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.True(result.IsSequence);
        var items = new List<XdmValue>();
        foreach (var item in result.SequenceValue!)
            items.Add(item);
        Assert.Equal(5, items.Count);
        Assert.Equal("5", items[0].ToString());
        Assert.Equal("4", items[1].ToString());
        Assert.Equal("3", items[2].ToString());
        Assert.Equal("2", items[3].ToString());
        Assert.Equal("1", items[4].ToString());
    }
}
