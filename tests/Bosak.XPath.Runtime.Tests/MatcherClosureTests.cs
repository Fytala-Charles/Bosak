// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 November 2026
// PURPOSE              : Verifies type-matcher allocation and ordered node-name alternatives
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 14-11-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 14-11-2026     | Bound conversion allocation with fixed noise allowance; retain zero matcher check.        |
//                      | Charles Korthout | 0.3   | 14-11-2026     | Replace allowance with standalone allocation measurements.                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Checks closure-free scalar matching and ordered node alternative behavior.</summary>
public class MatcherClosureTests
{
    /// <summary>Checks warmed matching and conversion allocations for five primitive kinds.</summary>
    [Fact]
    public void PrimitiveMatching_AvoidsClosureAndRetainsValidationCost()
    {
        var context = new EvaluationContext();
        var cases = new[] { (XdmValue.FromInteger(1), "xs:integer"),
            (XdmValue.FromString("a"), "xs:string"), (XdmValue.FromBoolean(true), "xs:boolean"),
            (XdmValue.FromDecimal(1m), "xs:decimal"), (XdmValue.FromDouble(1), "xs:double") };
        foreach (var (value, target) in cases)
        {
            Assert.True(VmEngine.ValueMatchesType(value, target, context));
            Assert.Equal(value.Kind, VmEngine.ApplyFunctionConversion(value, target, context).Kind);

        }
        var rows = AllocationProbeRunner.Run("matching");
        foreach (var row in rows)
            if (row.Key.StartsWith("match/", StringComparison.Ordinal)) Assert.Equal(0, row.Value);
            else Assert.InRange(row.Value, 0L, 40L * 100000);
        Assert.True(VmEngine.ValueMatchesType(XdmValue.FromInteger(1), "xs:integer", null));
    }

    /// <summary>Checks element alternative order, early success and undeclared-prefix errors.</summary>
    [Fact]
    public void ElementAlternatives_PreserveShortCircuitAndErrorOrder()
    {
        var value = XdmValue.FromNode(new XDocumentNode(new XElement("a")));
        var context = new EvaluationContext();
        Assert.True(VmEngine.ValueMatchesType(value, "element(a|p:a)", context));
        Assert.False(VmEngine.ValueMatchesType(value, "element(b|c)", context));
        Assert.Contains("XPST0081", Assert.Throws<InvalidOperationException>(() =>
            VmEngine.ValueMatchesType(value, "element(p:a|a)", context)).Message);
        Assert.Contains("XPST0081", Assert.Throws<InvalidOperationException>(() =>
            VmEngine.ValueMatchesType(value, "element(b|p:a)", context)).Message);
    }

    /// <summary>Checks attribute alternatives preserve order and namespace context.</summary>
    [Fact]
    public void AttributeAlternatives_PreserveOrderAndContext()
    {
        var element = new XElement("e", new XAttribute("a", "1"));
        var value = XdmValue.FromNode(new XDocumentNode(element.Attribute("a")!));
        var context = new EvaluationContext();
        Assert.True(VmEngine.ValueMatchesType(value, "attribute(a|p:a)", context));
        Assert.False(VmEngine.ValueMatchesType(value, "attribute(b|c)", context));
        Assert.Contains("XPST0081", Assert.Throws<InvalidOperationException>(() =>
            VmEngine.ValueMatchesType(value, "attribute(p:a|a)", context)).Message);
        var qualified = new XElement("e", new XAttribute(XName.Get("a", "urn:one"), "1"));
        var qualifiedValue = XdmValue.FromNode(new XDocumentNode(qualified.Attribute(XName.Get("a", "urn:one"))!));
        context.WithNamespace("p", "urn:one");
        Assert.True(VmEngine.ValueMatchesType(qualifiedValue, "attribute(p:b|p:a)", context));
        context.WithNamespace("p", "urn:two");
        Assert.False(VmEngine.ValueMatchesType(qualifiedValue, "attribute(p:b|p:a)", context));
    }

    /// <summary>Checks element namespace changes and schema type suffixes remain observed.</summary>
    [Fact]
    public void ElementAlternatives_PreserveNamespaceAndSchemaSuffix()
    {
        var context = new EvaluationContext().WithNamespace("p", "urn:one");
        var qualified = XdmValue.FromNode(new XDocumentNode(new XElement(XName.Get("a", "urn:one"))));
        Assert.True(VmEngine.ValueMatchesType(qualified, "element(p:b|p:a)", context));
        context.WithNamespace("p", "urn:two");
        Assert.False(VmEngine.ValueMatchesType(qualified, "element(p:b|p:a)", context));
        const string schema = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:element name='a' type='xs:int'/></xs:schema>";
        var schemas = new XmlSchemaSet { XmlResolver = null };
        using var reader = XmlReader.Create(new StringReader(schema)); schemas.Add(null, reader); schemas.Compile();
        var document = XDocument.Parse("<a>42</a>"); document.Validate(schemas, null, true);
        var value = XdmValue.FromNode(new XDocumentNode(document.Root!));
        context.SchemaSet = schemas;
        Assert.True(VmEngine.ValueMatchesType(value, "element(b|a,xs:int)", context));
        Assert.False(VmEngine.ValueMatchesType(value, "element(b|a,xs:string)", context));
        Assert.Equal(42, VmEngine.ApplyFunctionConversion(value, "xs:integer", context).IntegerValue);
    }

    /// <summary>Checks choice matching, empty cardinality and conversion error behavior.</summary>
    [Fact]
    public void MatchingAndConversion_KeepChoiceAndEmptySemantics()
    {
        var value = XdmValue.FromInteger(1); var context = new EvaluationContext();
        Assert.True(VmEngine.ValueMatchesType(value, "xs:string|xs:integer", context));
        Assert.False(VmEngine.ValueMatchesType(value, "xs:string|xs:boolean", context));
        Assert.True(VmEngine.ValueMatchesType(XdmValue.Undefined, "xs:integer?", context));
        Assert.True(VmEngine.ApplyFunctionConversion(XdmValue.Undefined, "xs:integer?", context).IsUndefined);
        Assert.Contains("XPTY0004", Assert.Throws<InvalidOperationException>(() =>
            VmEngine.ApplyFunctionConversion(XdmValue.Undefined, "xs:integer", context)).Message);
        Assert.Contains("XPTY0117", Assert.Throws<InvalidOperationException>(() =>
            VmEngine.ApplyFunctionConversion(XdmValue.FromString("p:a", "untypedAtomic"), "xs:QName", context)).Message);
    }
}
