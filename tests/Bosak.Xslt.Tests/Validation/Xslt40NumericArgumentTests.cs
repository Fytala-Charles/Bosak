// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 October 2026
// PURPOSE              : Tests primitive numeric coercion for XSLT 4.0 function arguments.
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

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Verifies permitted primitive numeric casts at XSLT function argument boundaries.</summary>
public class Xslt40NumericArgumentTests
{
    private static string Run(string type, string call, string parameter = "", string version = "4.0", bool constructorDefault = false)
    {
        var declaration = constructorDefault
            ? "<xsl:param name='x' as='" + type + "' required='no'><xsl:sequence select='" + parameter + "'/></xsl:param>"
            : "<xsl:param name='x' as='" + type + "' " + parameter + "/>";
        var sheet = "<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:f='urn:numeric' exclude-result-prefixes='#all'>"
            + "<xsl:function name='f:convert'>" + declaration + "<xsl:sequence select='($x instance of " + type + ", $x)'/></xsl:function>"
            + "<xsl:template match='/'><out><xsl:value-of select=\"" + call + "\"/></out></xsl:template></xsl:stylesheet>";
        return XElement.Parse(new XsltCompiler().Compile(sheet).TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value;
    }

    /// <summary>All six permitted primitive numeric directions preserve value and required type.</summary>
    /// <param name="type">The required primitive type.</param>
    /// <param name="value">The supplied numeric expression.</param>
    [Theory]
    [InlineData("xs:decimal", "1.25e0")]
    [InlineData("xs:decimal", "xs:float(1.25)")]
    [InlineData("xs:float", "1.25e0")]
    [InlineData("xs:float", "1.25")]
    [InlineData("xs:double", "1.25")]
    [InlineData("xs:double", "xs:float(1.25)")]
    public void PrimitiveArgumentsConvert(string type, string value)
        => Assert.Equal("true 1.25", Run(type, "f:convert(" + value + ")"));

    /// <summary>New reverse casts also apply to default arguments.</summary>
    /// <param name="type">The required primitive type.</param>
    /// <param name="value">The default numeric expression.</param>
    [Theory]
    [InlineData("xs:decimal", "1.25e0")]
    [InlineData("xs:decimal", "xs:float(1.25)")]
    [InlineData("xs:float", "1.25e0")]
    public void PrimitiveDefaultsConvert(string type, string value)
        => Assert.Equal("true 1.25", Run(type, "f:convert()", "required='no' select='" + value + "'"));

    /// <summary>Every function call form accepts the primitive double-to-decimal cast.</summary>
    /// <param name="call">The call expression.</param>
    [Theory]
    [InlineData("f:convert(x := 1.25e0)")]
    [InlineData("1.25e0 => f:convert()")]
    [InlineData("(f:convert#1)(1.25e0)")]
    [InlineData("function-lookup(QName('urn:numeric','convert'),1)(1.25e0)")]
    [InlineData("let $f := f:convert(?) return $f(1.25e0)")]
    public void FunctionItemsAndKeywordCallsConvert(string call)
        => Assert.Equal("true 1.25", Run("xs:decimal", call));

    /// <summary>The XPath 3.1 profile keeps rejecting the newer reverse casts.</summary>
    /// <param name="type">The required primitive type.</param>
    /// <param name="value">The supplied numeric expression.</param>
    [Theory]
    [InlineData("xs:decimal", "1.25e0")]
    [InlineData("xs:decimal", "xs:float(1.25)")]
    [InlineData("xs:float", "1.25e0")]
    public void EarlierProfileRejectsReverseCasts(string type, string value)
        => Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Run(type, "f:convert(" + value + ")", version: "3.0")).Message);

    /// <summary>Promotion is never combined with derived-type relabeling or fractional truncation.</summary>
    /// <param name="type">The required derived type.</param>
    /// <param name="value">The incompatible numeric expression.</param>
    [Theory]
    [InlineData("xs:integer", "1e7")]
    [InlineData("xs:unsignedByte", "1e0")]
    [InlineData("xs:integer", "1.25")]
    [InlineData("xs:short", "1.25")]
    public void DerivedTargetsRemainExcluded(string type, string value)
        => Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Run(type, "f:convert(" + value + ")")).Message);

    /// <summary>Numeric cast value errors retain their standard cast error identity.</summary>
    /// <param name="value">A non-finite double expression.</param>
    /// <param name="defaulted">Whether the argument uses its default.</param>
    [Theory]
    [InlineData("xs:double('INF')", false)]
    [InlineData("xs:double('NaN')", false)]
    [InlineData("xs:double('INF')", true)]
    [InlineData("xs:double('NaN')", true)]
    public void NonFiniteDecimalCastRetainsError(string value, bool defaulted)
    {
        var call = defaulted ? "f:convert()" : "f:convert(" + value + ")";
        var parameter = defaulted ? "required='no' select=\"" + value + "\"" : "";
        Assert.Contains("FOCA0002", Assert.ThrowsAny<Exception>(() => Run("xs:decimal", call, parameter)).Message);
    }

    /// <summary>Plural arguments convert each numeric member.</summary>
    [Fact]
    public void SequenceMembersConvert() => Assert.Equal("true 1.25 2.5", Run("xs:decimal*", "f:convert((1.25e0,xs:float(2.5)))"));

    /// <summary>A sequence-constructor default uses primitive numeric coercion.</summary>
    [Fact]
    public void ConstructorDefaultConverts()
        => Assert.Equal("true 1.25", Run("xs:decimal", "f:convert()", "1.25e0", constructorDefault: true));

    /// <summary>A schema EQName identifies the same primitive conversion target.</summary>
    [Fact]
    public void EqNameTargetConverts()
        => Assert.Equal("true 1.25", Run("Q{http://www.w3.org/2001/XMLSchema}decimal", "f:convert(1.25e0)"));

    /// <summary>Numeric conversion preserves singleton cardinality errors.</summary>
    [Fact]
    public void SingletonCardinalityRemainsChecked()
        => Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Run("xs:decimal", "f:convert((1e0,2e0))")).Message);
}
