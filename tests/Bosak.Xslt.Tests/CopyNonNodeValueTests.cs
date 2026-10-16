// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies non-node copy result kinds, payloads and complex-content behavior.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies non-node copy result kinds, payloads and complex-content behavior.</summary>
public class CopyNonNodeValueTests
{
    private static string Copy(string expression, bool context = false)
    {
        XNamespace xsl = "http://www.w3.org/1999/XSL/Transform";
        var copy = new XElement(xsl + "copy", new XAttribute(XNamespace.Xmlns + "xsl", xsl.NamespaceName),
            new XElement(xsl + "sequence", new XAttribute("select", "error()")));
        if (!context)
        {
            copy.SetAttributeValue("select", expression);
            return copy.ToString();
        }
        return new XElement(xsl + "for-each", new XAttribute(XNamespace.Xmlns + "xsl", xsl.NamespaceName),
            new XAttribute("select", expression), copy).ToString();
    }
    private static XdmValue Run(string body, bool function = false, bool typed = false, bool raw = true) => new Api.XsltCompiler().Compile(
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='urn:functions' xmlns:xs='http://www.w3.org/2001/XMLSchema' version='4.0'>" +
        (function ? "<xsl:function name='f:test' as='item()*'>" + body + "</xsl:function>" : "") +
        "<xsl:template name='start'" + (typed ? " as='item()*'" : "") + ">" + (function ? "<xsl:sequence select='f:test()'/>" : body) + "</xsl:template></xsl:stylesheet>")
        .Transform(null, initialTemplate: "start", rawResult: raw);
    private static List<XdmValue> Items(XdmValue value)
    {
        if (value.IsUndefined) return [];
        if (!value.IsSequence) return [value];
        var result = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(value.SequenceValue!)) result.Add(item);
        return result;
    }

    /// <summary>Preserves non-node kinds and skips content in either instruction dispatcher.</summary>
    /// <param name="expression">The selected value.</param>
    /// <param name="kind">Its required kind.</param>
    /// <param name="function">Whether the function dispatcher is used.</param>
    /// <param name="context">Whether the value is the context item.</param>
    [Theory]
    [InlineData("7", XdmValueKind.Integer, false, false)]
    [InlineData("7", XdmValueKind.Integer, false, true)]
    [InlineData("7", XdmValueKind.Integer, true, false)]
    [InlineData("7", XdmValueKind.Integer, true, true)]
    [InlineData("true()", XdmValueKind.Boolean, false, false)]
    [InlineData("true()", XdmValueKind.Boolean, true, true)]
    [InlineData("1.25", XdmValueKind.Decimal, false, false)]
    [InlineData("1.25", XdmValueKind.Decimal, true, true)]
    [InlineData("'text'", XdmValueKind.String, false, true)]
    [InlineData("'text'", XdmValueKind.String, true, false)]
    [InlineData("map{'x':1}", XdmValueKind.Map, false, false)]
    [InlineData("map{'x':1}", XdmValueKind.Map, false, true)]
    [InlineData("map{'x':1}", XdmValueKind.Map, true, false)]
    [InlineData("map{'x':1}", XdmValueKind.Map, true, true)]
    [InlineData("[(),(1,2)]", XdmValueKind.Array, false, false)]
    [InlineData("[(),(1,2)]", XdmValueKind.Array, false, true)]
    [InlineData("[(),(1,2)]", XdmValueKind.Array, true, false)]
    [InlineData("[(),(1,2)]", XdmValueKind.Array, true, true)]
    [InlineData("function(){3}", XdmValueKind.Function, false, false)]
    [InlineData("function(){3}", XdmValueKind.Function, false, true)]
    [InlineData("function(){3}", XdmValueKind.Function, true, false)]
    [InlineData("function(){3}", XdmValueKind.Function, true, true)]
    public void ValueKinds(string expression, XdmValueKind kind, bool function, bool context)
    {
        var value = Run(Copy(expression, context), function);
        Assert.Equal(kind, value.Kind);
        if (value.Kind == XdmValueKind.Integer) Assert.Equal(7, value.IntegerValue);
        if (value.Kind == XdmValueKind.String) Assert.Equal("text", value.StringValue);
        if (value.Kind == XdmValueKind.Boolean) Assert.True(value.BooleanValue);
        if (value.Kind == XdmValueKind.Decimal) Assert.Equal(1.25m, value.DecimalValue);
        if (value.IsMap)
        {
            Assert.True(value.MapValue.TryGetValue(XdmValue.FromString("x"), out var entry));
            Assert.Equal(1, entry.IntegerValue);
        }
        if (value.IsArray)
        {
            Assert.Equal(2, value.ArrayValue.Count);
            Assert.Empty(Items(value.ArrayValue.Get(1)));
            Assert.Equal(new long[] {1,2}, Items(value.ArrayValue.Get(2)).Select(v => v.IntegerValue));
        }
    }

    /// <summary>Preserves complete value kinds and order in a typed entry.</summary>
    [Fact]
    public void TypedMixedSequence()
    {
        var items = Items(Run(Copy("1") + Copy("map{'x':1}") + Copy("[(),(1,2)]") + Copy("function(){3}") + Copy("2"), typed: true));
        Assert.Equal(new[] { XdmValueKind.Integer, XdmValueKind.Map, XdmValueKind.Array, XdmValueKind.Function, XdmValueKind.Integer }, items.Select(v => v.Kind));
        Assert.Equal(1, items[0].IntegerValue);
        Assert.Equal(2, items[4].IntegerValue);
    }

    /// <summary>Uses normal spacing and array flattening when values construct an element.</summary>
    [Fact]
    public void ComplexContent() => Assert.Equal("1 2 3 4", Run("<out>" + Copy("1") + Copy("[2,(),(3,4)]") + "</out>").NodeValue.StringValue);

    /// <summary>Rejects function items inside element content.</summary>
    /// <param name="expression">The illegal copied value.</param>
    [Theory]
    [InlineData("map{}")]
    [InlineData("function(){1}")]
    public void IllegalComplexContent(string expression) => Assert.Contains("XTDE0450", Assert.ThrowsAny<Exception>(() => Run("<out>" + Copy(expression) + "</out>")).ToString());

    /// <summary>Enforces selected cardinality without flattening an array into selected items.</summary>
    [Fact]
    public void Cardinality() => Assert.Contains("XTTE3180", Assert.ThrowsAny<Exception>(() => Run(Copy("1,2"))).ToString());

    /// <summary>Retains copied function callability through a typed variable.</summary>
    [Fact]
    public void CallableFunction() => Assert.Equal(3, Run("<xsl:variable name='v' as='function(*)'>" + Copy("function(){3}") + "</xsl:variable><xsl:sequence select='$v()'/>").IntegerValue);

    /// <summary>Retains array member boundaries through a typed variable.</summary>
    [Fact]
    public void TypedArrayVariable()
    {
        var value = Run("<xsl:variable name='v' as='array(*)'>" + Copy("[(),(1,2)]") + "</xsl:variable><xsl:sequence select='$v'/>");
        Assert.True(value.IsArray);
        Assert.Equal(2, value.ArrayValue.Count);
        Assert.Empty(Items(value.ArrayValue.Get(1)));
        Assert.Equal(new long[] {1,2}, Items(value.ArrayValue.Get(2)).Select(v => v.IntegerValue));
    }

    /// <summary>Returns no items and skips content for an empty selection.</summary>
    /// <param name="function">Whether the function dispatcher is used.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EmptySelection(bool function) => Assert.Empty(Items(Run(Copy("()"), function)));

    /// <summary>Rejects maps/functions for ordinary XML result construction.</summary>
    /// <param name="expression">The copied function item.</param>
    [Theory]
    [InlineData("map{}")]
    [InlineData("function(){1}")]
    public void OrdinaryXmlError(string expression) => Assert.Contains("SENR0001", Assert.ThrowsAny<Exception>(() => Run(Copy(expression), raw: false)).ToString());
}
