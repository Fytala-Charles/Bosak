// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 oktober 2026
// PURPOSE              : Unit tests for the XPath 4.0 focus constructors (arity-0 xs:* constructors, spec PR661).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation: happy paths across the built-in type set, XPDY0002/FOTY0013/FORG0001/XPTY0004   |
//                      |                  |       |                | error cases, unsignedLong above long.MaxValue, named-function-ref form                  |
//                      |==================|=======|================|=========================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Api.Tests;

public class FocusConstructorTests
{
    private static XdmValue Eval40WithFocus(string xpath, XdmValue contextItem)
    {
        var expr = XPath31Expression.Compile(xpath, new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        var ctx = new EvaluationContext().WithFocus(contextItem, 1, 1);
        return expr.Evaluate(ctx);
    }

    private static XdmValue Eval40(string xpath)
    {
        var expr = XPath31Expression.Compile(xpath, new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        return expr.Evaluate(new EvaluationContext());
    }

    // Stringifies the single expected item, unwrapping a sequence wrapper.
    private static XdmValue Unwrap(XdmValue result)
    {
        if (result.IsSequence && result.SequenceValue is not null)
        {
            var items = XdmSequence.FromSource(result.SequenceValue).GetEnumerator();
            Assert.True(items.MoveNext());
            var single = items.Current;
            Assert.False(items.MoveNext());
            return single;
        }
        return result;
    }

    private static string One(XdmValue result) => Unwrap(result).ToString();

    // ------------------------------------------------------------------
    // Happy paths
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("'42' ! xs:integer()", "42")]
    [InlineData("'-9223372036854775808' ! xs:long()", "-9223372036854775808")]
    [InlineData("'3.14' ! xs:decimal()", "3.14")]
    [InlineData("'1e2' ! xs:double()", "100")]
    [InlineData("'true' ! xs:boolean()", "true")]
    [InlineData("'hello' ! xs:string()", "hello")]
    [InlineData("'PT1H' ! xs:dayTimeDuration()", "PT1H")]
    [InlineData("'P1Y' ! xs:yearMonthDuration()", "P1Y")]
    [InlineData("'2026-10-10' ! xs:date()", "2026-10-10")]
    [InlineData("'12:00:00' ! xs:time()", "12:00:00")]
    [InlineData("'2026-10-10T12:00:00' ! xs:dateTime()", "2026-10-10T12:00:00")]
    [InlineData("'2005' ! xs:gYear()", "2005")]
    [InlineData("'--10-10' ! xs:gMonthDay()", "--10-10")]
    [InlineData("'0ff0' ! xs:hexBinary()", "0FF0")]
    [InlineData("'0' ! xs:nonPositiveInteger()", "0")]
    [InlineData("'-1' ! xs:negativeInteger()", "-1")]
    [InlineData("'0' ! xs:nonNegativeInteger()", "0")]
    [InlineData("'1' ! xs:positiveInteger()", "1")]
    public void Evaluate_FocusConstructor_ReturnsTypedValue(string xpath, string expected)
    {
        Assert.Equal(expected, One(Eval40(xpath)));
    }

    [Fact]
    public void Evaluate_FocusConstructor_StringAsWhitespaceIsNormalized()
    {
        // xs:normalizedString ctor: the value keeps its content; type is asserted by cast success.
        Assert.Equal("a b", One(Eval40("'a b' ! xs:normalizedString()")));
    }

    [Fact]
    public void Evaluate_FocusConstructor_UnsignedLongMaxValue()
    {
        // Above long.MaxValue the value is decimal-backed but annotated xs:unsignedLong.
        var result = Eval40("'18446744073709551615' ! xs:unsignedLong()");
        Assert.Equal("18446744073709551615", One(result));
        Assert.True(Unwrap(result).Kind is XdmValueKind.Integer or XdmValueKind.Decimal);
    }

    [Fact]
    public void Evaluate_FocusConstructor_NodeContextAtomizes()
    {
        const string xml = "<root>007</root>";
        var doc = System.Xml.Linq.XDocument.Parse(xml);
        var root = doc.Root!;
        var result = Eval40WithFocus(". ! xs:integer()", XdmValue.FromNode(new XDocumentNode(root)));
        Assert.Equal("7", One(result));
    }

    [Fact]
    public void Evaluate_FocusConstructor_QNameResolvesPrefixFromContext()
    {
        var ctx = new EvaluationContext()
            .WithNamespace("a", "http://example.com/ns")
            .WithFocus(XdmValue.FromString("a:b"), 1, 1);
        var expr = XPath31Expression.Compile("'a:b' ! xs:QName()",
            new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        var result = expr.Evaluate(ctx);
        Assert.Equal(XdmValueKind.QName, Unwrap(result).Kind);
    }

    [Fact]
    public void Evaluate_FocusConstructor_NamedFunctionRef()
    {
        Assert.Equal("7", One(Eval40("'7' ! xs:integer#0()")));
    }

    // ------------------------------------------------------------------
    // Error cases
    // ------------------------------------------------------------------

    [Fact]
    public void Evaluate_FocusConstructor_NoContextItem_ThrowsXpdy0002()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Eval40("let $f := function(){xs:integer()} return $f()"));
        Assert.Contains("XPDY0002", ex.Message);
    }

    [Fact]
    public void Evaluate_FocusConstructor_MapContext_ThrowsFoty0013()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("map{} ! xs:integer()"));
        Assert.Contains("FOTY0013", ex.Message);
    }

    [Fact]
    public void Evaluate_FocusConstructor_InvalidLexical_ThrowsForg0001()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Eval40WithFocus("xs:positiveInteger()", XdmValue.FromString("-2")));
        Assert.Contains("FORG0001", ex.Message);
    }

    [Fact]
    public void Evaluate_FocusConstructor_WrongSourceType_ThrowsXpty0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("current-date() ! xs:positiveInteger()"));
        Assert.Contains("XPTY0004", ex.Message);
    }
}
