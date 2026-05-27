using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

public class InstanceOfUndefinedTest
{
    [Theory]
    [InlineData("$p instance of xs:int*", true)]
    [InlineData("$p instance of xs:int", false)]
    [InlineData("$p instance of xs:int?", true)]
    [InlineData("$p instance of xs:string*", true)]
    [InlineData("$p instance of xs:boolean*", true)]
    [InlineData("$p instance of xs:decimal*", true)]
    [InlineData("$p instance of xs:double*", true)]
    [InlineData("$p instance of xs:float*", true)]
    [InlineData("$p instance of xs:dateTime*", true)]
    [InlineData("$p instance of xs:date*", true)]
    [InlineData("$p instance of xs:time*", true)]
    [InlineData("$p instance of xs:duration*", true)]
    [InlineData("$p instance of xs:QName*", true)]
    [InlineData("$p instance of xs:gYear*", true)]
    [InlineData("$p instance of xs:gYearMonth*", true)]
    [InlineData("$p instance of xs:gMonthDay*", true)]
    [InlineData("$p instance of xs:gDay*", true)]
    [InlineData("$p instance of xs:gMonth*", true)]
    [InlineData("$p instance of xs:hexBinary*", true)]
    [InlineData("$p instance of xs:base64Binary*", true)]
    [InlineData("$p instance of xs:anyURI*", true)]
    [InlineData("$p instance of xs:untypedAtomic*", true)]
    [InlineData("$p instance of xs:normalizedString*", true)]
    [InlineData("$p instance of xs:token*", true)]
    [InlineData("$p instance of xs:language*", true)]
    [InlineData("$p instance of xs:NMTOKEN*", true)]
    [InlineData("$p instance of xs:Name*", true)]
    [InlineData("$p instance of xs:NCName*", true)]
    [InlineData("$p instance of xs:ID*", true)]
    [InlineData("$p instance of xs:IDREF*", true)]
    [InlineData("$p instance of xs:ENTITY*", true)]
    [InlineData("$p instance of node()*", true)]
    [InlineData("$p instance of item()*", true)]
    [InlineData("$p instance of empty-sequence()", true)]
    [InlineData("$p instance of xs:anyAtomicType*", true)]
    [InlineData("$p instance of function(*)*", true)]
    public void Undefined_InstanceOf(string expr, bool expected)
    {
        var compiled = XPath31Expression.Compile(expr);
        var ctx = new EvaluationContext().WithVariable("p", XdmValue.Undefined);
        var result = compiled.Evaluate(ctx);
        Assert.Equal(expected, result.BooleanValue);
    }
}
