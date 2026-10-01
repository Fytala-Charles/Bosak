// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 30 september 2026
// PURPOSE              : Verifies that general comparisons and function conversion flatten multi-item typed values of list-typed schema nodes.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 30-09-2026     | Creation (REQ-114/PB-3 C9)                                                               |
//                      |==================|=======|================|=========================================================================================
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>
/// Regression tests for REQ-114/PB-3 C9: atomizing a schema-validated node whose type is an
/// XSD list type yields a multi-item sequence. General comparisons (XPath 3.1 §3.5.2) must
/// compare the Cartesian product of both atomized sequences (existential semantics), and
/// function conversion must convert each member against plural targets such as xs:decimal*
/// (import-schema-020/029/030, validation-0301/0401).
/// </summary>
public sealed class ListTypedSequenceFlatteningTests
{
    private const string ListSchemaXml =
        "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' " +
        "xmlns:t='urn:listtest' targetNamespace='urn:listtest' elementFormDefault='qualified'>" +
        "<xs:simpleType name='decimalList'>" +
        "<xs:list itemType='xs:decimal'/>" +
        "</xs:simpleType>" +
        "<xs:element name='root'>" +
        "<xs:complexType>" +
        "<xs:sequence>" +
        "<xs:element name='e' type='t:decimalList'/>" +
        "</xs:sequence>" +
        "<xs:attribute name='a' type='xs:NMTOKENS'/>" +
        "</xs:complexType>" +
        "</xs:element>" +
        "</xs:schema>";

    private static (XmlSchemaSet SchemaSet, XElement Root) LoadValidatedListDocument()
    {
        var schemaSet = new XmlSchemaSet();
        using (var reader = XmlReader.Create(new StringReader(ListSchemaXml)))
            schemaSet.Add(XmlSchema.Read(reader, null)!);
        schemaSet.Compile();

        var doc = XDocument.Parse("<root a='red green blue' xmlns='urn:listtest'><e>1.5 2.5 3.5</e></root>");
        doc.Validate(schemaSet, null, true);
        return (schemaSet, doc.Root!);
    }

    [Fact]
    public void GeneralComparison_ListTypedAttribute_MatchingMemberIsTrue()
    {
        // import-schema-020: $t[1] = 'green' where $t is an xs:NMTOKENS attribute.
        var (schemaSet, root) = LoadValidatedListDocument();
        var attr = XdmValue.FromNode(new XDocumentNode(root.Attribute("a")!));
        var ctx = new EvaluationContext { SchemaSet = schemaSet };

        var result = XPath31Expression.Compile(". = 'green'").Evaluate(ctx.WithFocus(attr, 1, 1));
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void GeneralComparison_ListTypedAttribute_NoMatchingMemberIsFalse()
    {
        var (schemaSet, root) = LoadValidatedListDocument();
        var attr = XdmValue.FromNode(new XDocumentNode(root.Attribute("a")!));
        var ctx = new EvaluationContext { SchemaSet = schemaSet };

        var result = XPath31Expression.Compile(". = 'purple'").Evaluate(ctx.WithFocus(attr, 1, 1));
        Assert.False(result.BooleanValue);
    }

    [Fact]
    public void GeneralComparison_ListTypedElement_NumericMemberIsTrue()
    {
        // validation-0401: a list-typed element compared against a numeric literal.
        var (schemaSet, root) = LoadValidatedListDocument();
        var element = XdmValue.FromNode(new XDocumentNode(root.Elements().First()));
        var ctx = new EvaluationContext { SchemaSet = schemaSet };

        var result = XPath31Expression.Compile(". = 2.5").Evaluate(ctx.WithFocus(element, 1, 1));
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void ValueComparison_ListTypedAttribute_StillRequiresSingleton()
    {
        // Value comparisons keep strict singleton semantics: atomizing the multi-item
        // typed value of a list-typed node is XPTY0004 (unchanged by REQ-114).
        var (schemaSet, root) = LoadValidatedListDocument();
        var attr = XdmValue.FromNode(new XDocumentNode(root.Attribute("a")!));
        var ctx = new EvaluationContext { SchemaSet = schemaSet };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            XPath31Expression.Compile(". eq 'green'").Evaluate(ctx.WithFocus(attr, 1, 1)));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void FunctionConversion_ListTypedElement_ToPluralTargetConvertsEachMember()
    {
        // validation-0301: a single list-typed element whose atomized value is a 3-item
        // xs:decimal sequence converts member-by-member to xs:decimal*.
        var (schemaSet, root) = LoadValidatedListDocument();
        var ctx = new EvaluationContext { SchemaSet = schemaSet };
        var element = XdmValue.FromNode(new XDocumentNode(root.Elements().First()));

        var result = XdmConversions.ApplyFunctionConversion(element, "xs:decimal*", ctx);
        Assert.True(result.IsSequence);
        var items = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item);
        Assert.Equal(3, items.Count);
        Assert.All(items, item => Assert.Equal(XdmValueKind.Decimal, item.Kind));
        Assert.Equal(new[] { "1.5", "2.5", "3.5" }, items.Select(i => i.ToString()).ToArray());
    }

    [Fact]
    public void FunctionConversion_ListTypedElement_ToSingularTargetRaisesCardinalityError()
    {
        // A plural atomized value converted to a singular xs:decimal target keeps the
        // XPTY0004 cardinality error.
        var (schemaSet, root) = LoadValidatedListDocument();
        var ctx = new EvaluationContext { SchemaSet = schemaSet };
        var element = XdmValue.FromNode(new XDocumentNode(root.Elements().First()));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            XdmConversions.ApplyFunctionConversion(element, "xs:decimal", ctx));
        Assert.Contains("XPTY0004", ex.Message);
    }
}
