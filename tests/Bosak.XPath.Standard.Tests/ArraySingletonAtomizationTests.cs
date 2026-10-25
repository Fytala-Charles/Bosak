// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 October 2026
// PURPOSE              : Verifies singleton cardinality after recursive array-member atomization.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies singleton cardinality after recursive array-member atomization.</summary>
public sealed class ArraySingletonAtomizationTests
{
    /// <summary>Provides direct/runtime recursive singleton, empty and error cases.</summary>
    /// <returns>Profile, direct/runtime expressions, expected output/error and error flag.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Direct, string Runtime, string Expected, bool Error)[]
        {
            ("-([[],2])", "(function($x){-$x})([[],2])", "-2", false),
            ("-([2,[]])", "(function($x){-$x})([2,[]])", "-2", false),
            ("-([[],[2],[]])", "(function($x){-$x})([[],[2],[]])", "-2", false),
            ("-([[],[],[2]])", "(function($x){-$x})([[],[],[2]])", "-2", false),
            ("-([[[]],2])", "(function($x){-$x})([[[]],2])", "-2", false),
            ("-([([],2)])", "(function($x){-$x})([([],2)])", "-2", false),
            ("-([([],2,[])])", "(function($x){-$x})([([],2,[])])", "-2", false),
            ("-([[],[(),[2]],[]])", "(function($x){-$x})([[],[(),[2]],[]])", "-2", false),
            ("+([[],2])", "(function($x){+($x)})([[],2])", "2", false),
            ("([[],2]) + 1", "(function($x){($x) + 1})([[],2])", "3", false),
            ("([[],2]) - 1", "(function($x){($x) - 1})([[],2])", "1", false),
            ("([[],2]) * 3", "(function($x){($x) * 3})([[],2])", "6", false),
            ("([[],2]) div 2", "(function($x){($x) div 2})([[],2])", "1", false),
            ("([[],2]) idiv 1", "(function($x){($x) idiv 1})([[],2])", "2", false),
            ("([[],2]) mod 3", "(function($x){($x) mod 3})([[],2])", "2", false),
            ("string-join((([[],2]) to 3) ! string(.), ',')", "(function($x){string-join((($x) to 3) ! string(.), ',')})([[],2])", "2,3", false),
            ("([[],2]) eq 2", "(function($x){($x) eq 2})([[],2])", "true", false),
            ("empty(-([[],[]]))", "(function($x){empty(-$x)})([[],[]])", "true", false),
            ("empty(-([[],[[]],[]]))", "(function($x){empty(-$x)})([[],[[]],[]])", "true", false),
            ("empty(-([([],[])]))", "(function($x){empty(-$x)})([([],[])])", "true", false),
            ("-([[],1,2])", "(function($x){-$x})([[],1,2])", "XPTY0004", true),
            ("-([[1,2]])", "(function($x){-$x})([[1,2]])", "XPTY0004", true),
            ("-([[],[1],[2],[]])", "(function($x){-$x})([[],[1],[2],[]])", "XPTY0004", true),
            ("-([[],map{}])", "(function($x){-$x})([[],map{}])", "FOTY0013", true),
            ("-([map{},[]])", "(function($x){-$x})([map{},[]])", "FOTY0013", true),
            ("-([[],function(){1}])", "(function($x){-$x})([[],function(){1}])", "FOTY0013", true),
            ("-([[],xs:untypedAtomic('bad')])", "(function($x){-$x})([[],xs:untypedAtomic('bad')])", "FORG0001", true),
            ("-([[],'2'])", "(function($x){-$x})([[],'2'])", "XPTY0004", true),
            ("-([[],true()])", "(function($x){-$x})([[],true()])", "XPTY0004", true),
            ("-([[],xs:untypedAtomic('2.5'),[]])", "(function($x){-$x})([[],xs:untypedAtomic('2.5'),[]])", "-2.5", false),
            ("-([[],parse-xml('<n>3</n>')/n,[]])", "(function($x){-$x})([[],parse-xml('<n>3</n>')/n,[]])", "-3", false),
            ("-([[],9223372036854775808,[]])", "(function($x){-$x})([[],9223372036854775808,[]])", "-9223372036854775808", false),
            ("-([[],xs:decimal('2.5'),[]])", "(function($x){-$x})([[],xs:decimal('2.5'),[]])", "-2.5", false),
            ("-([[],xs:dayTimeDuration('P1D'),[]])", "(function($x){-$x})([[],xs:dayTimeDuration('P1D'),[]])", "-P1D", false),
            ("-([2])", "(function($x){-$x})([2])", "-2", false),
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                yield return new object[] { profile, row.Direct, row.Runtime, row.Expected, row.Error };
    }

    /// <summary>Counts atomic values rather than immediate array members.</summary>
    /// <param name="profile">The language profile.</param>
    /// <param name="direct">The direct expression.</param>
    /// <param name="runtime">The expression using runtime parameters.</param>
    /// <param name="expected">The expected value or error code.</param>
    /// <param name="error">Whether an error is required.</param>
    [Theory, MemberData(nameof(Cases))]
    public void RecursiveSingletonCardinality(XPathCompatibility profile, string direct, string runtime, string expected, bool error)
    {
        foreach (var expression in new[] { direct, runtime })
        {
            string Evaluate() => XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile })
                .Evaluate(new EvaluationContext()).ToString();
            if (error) Assert.Contains(expected, Assert.ThrowsAny<Exception>(() => Evaluate()).Message);
            else Assert.Equal(expected, Evaluate());
        }
    }

    /// <summary>Preserves the required no-typed-value error for schema-validated nodes.</summary>
    /// <param name="profile">The language profile.</param>
    [Theory]
    [InlineData(XPathCompatibility.XPath31)]
    [InlineData(XPathCompatibility.XPath40)]
    [InlineData(XPathCompatibility.XPath40Experimental)]
    public void NodeWithoutTypedValueRemainsFoty0012(XPathCompatibility profile)
    {
        var schemas = new XmlSchemaSet();
        using var reader = XmlReader.Create(new StringReader("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:element name='root'><xs:complexType><xs:sequence><xs:element name='child' type='xs:string'/></xs:sequence></xs:complexType></xs:element></xs:schema>"));
        schemas.Add("", reader); schemas.Compile();
        var doc = XDocument.Parse("<root><child>x</child></root>");
        doc.Validate(schemas, (_, args) => throw new InvalidOperationException(args.Message), addSchemaInfo: true);
        var context = new EvaluationContext().WithFocus(XdmValue.FromNode(new XDocumentNode(doc.Root!)), 1, 1);
        foreach (var source in new[] { "-[.]", "-[[],.,[]]" })
            Assert.Contains("FOTY0012", Assert.ThrowsAny<Exception>(() => XPath31Expression.Compile(source, new CompileOptions { Compatibility = profile }).Evaluate(context)).Message);
    }

    /// <summary>Backward-compatible numeric conversion still uses the atomized singleton.</summary>
    /// <param name="operand">A singleton array shape.</param>
    [Theory]
    [InlineData("[2]")]
    [InlineData("[[],2]")]
    [InlineData("[2,[]]")]
    public void BackwardsCompatibleSingleton(string operand)
    {
        var expression = XPath31Expression.Compile("-(" + operand + ")", new CompileOptions { BackwardsCompatible = true });
        var result = expression.Evaluate(new EvaluationContext { BackwardsCompatible = true });
        Assert.Equal("-2", result.ToString());
        Assert.Equal(XdmValueKind.Double, result.Kind);
    }
}
