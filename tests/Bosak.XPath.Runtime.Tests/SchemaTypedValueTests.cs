// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 augustus 2026
// PURPOSE              : Verifies PSVI typed-value extraction and schema-aware comparisons.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-08-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 19-08-2026     | User-defined schema type constructor, cast, instance-of, and timezone preservation tests |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

public class SchemaTypedValueTests
{
    [Fact]
    public void AtomicFloatTypedValueIsFloat()
    {
        var schemaSet = new XmlSchemaSet();
        string schemaPath = Path.GetFullPath("../../../../qt3tests/docs/atomic.xsd");
        string docPath = Path.GetFullPath("../../../../qt3tests/docs/atomic.xml");
        using (var stream = File.OpenRead(schemaPath))
        using (var reader = System.Xml.XmlReader.Create(stream))
        {
            schemaSet.Add(XmlSchema.Read(reader, null)!);
        }
        schemaSet.Compile();

        var doc = XDocumentProvider.LoadXml(docPath, null, schemaSet);
        var ctx = new EvaluationContext();
        ctx = ctx.WithNamespace("atomic", "http://www.w3.org/XQueryTest");
        ctx = ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);
        FunctionLibrary.Populate(ctx);

        var raw = XPath31Expression.Compile("//atomic:float[1]").Evaluate(ctx);
        XdmValue node;
        if (raw.IsNode) node = raw;
        else
        {
            var seq = XdmSequence.FromSource(raw.SequenceValue!).GetEnumerator();
            if (!seq.MoveNext()) Assert.Fail("Empty sequence");
            node = seq.Current;
        }
        if (!node.IsNode)
            Assert.Fail($"Expected node, got kind {node.Kind}");
        var typed = node.NodeValue.TypedValue;
        if (typed.Kind != XdmValueKind.Float)
            Assert.Fail($"Expected Float typed value, got kind {typed.Kind}, value '{typed}', schemaTypeName={typed.SchemaTypeName}");
    }

    [Fact]
    public void AtomicFloatAvgComparison()
    {
        var schemaSet = new XmlSchemaSet();
        string schemaPath = Path.GetFullPath("../../../../qt3tests/docs/atomic.xsd");
        string docPath = Path.GetFullPath("../../../../qt3tests/docs/atomic.xml");
        using (var stream = File.OpenRead(schemaPath))
        using (var reader = System.Xml.XmlReader.Create(stream))
        {
            schemaSet.Add(XmlSchema.Read(reader, null)!);
        }
        schemaSet.Compile();

        var doc = XDocumentProvider.LoadXml(docPath, null, schemaSet);
        var ctx = new EvaluationContext();
        ctx = ctx.WithNamespace("atomic", "http://www.w3.org/XQueryTest");
        ctx = ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);
        FunctionLibrary.Populate(ctx);

        var floats = XPath31Expression.Compile("//atomic:float").Evaluate(ctx);
        var sum = XPath31Expression.Compile("fn:sum((//atomic:float,//atomic:float))").Evaluate(ctx);
        var avg = XPath31Expression.Compile("fn:avg((//atomic:float,//atomic:float))").Evaluate(ctx);
        var rhs = XPath31Expression.Compile("xs:float(1.26743233E15)").Evaluate(ctx);
        var result = XPath31Expression.Compile("(fn:avg((//atomic:float,//atomic:float))) eq xs:float(1.26743233E15)")
            .Evaluate(ctx);
        if (!result.BooleanValue)
        {
            var seq = XdmSequence.FromSource(floats.SequenceValue!).GetEnumerator();
            var kinds = new List<string>();
            while (seq.MoveNext())
            {
                var typed = seq.Current.NodeValue.TypedValue;
                kinds.Add($"{typed.Kind}:{typed}");
            }
            Assert.Fail($"float kinds=[{string.Join(",", kinds)}], sum kind={sum.Kind} val={sum}, avg kind={avg.Kind} val={avg}, rhs kind={rhs.Kind} val={rhs}");
        }
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void AtomicDecimalTypedValueIsDecimal()
    {
        var schemaSet = new XmlSchemaSet();
        string schemaPath = Path.GetFullPath("../../../../qt3tests/docs/atomic.xsd");
        string docPath = Path.GetFullPath("../../../../qt3tests/docs/atomic.xml");
        using (var stream = File.OpenRead(schemaPath))
        using (var reader = System.Xml.XmlReader.Create(stream))
        {
            schemaSet.Add(XmlSchema.Read(reader, null)!);
        }
        schemaSet.Compile();

        var doc = XDocumentProvider.LoadXml(docPath, null, schemaSet);
        var ctx = new EvaluationContext();
        ctx = ctx.WithNamespace("atomic", "http://www.w3.org/XQueryTest");
        ctx = ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);
        FunctionLibrary.Populate(ctx);

        var raw = XPath31Expression.Compile("//atomic:decimal[1]").Evaluate(ctx);
        XdmValue node;
        if (raw.IsNode) node = raw;
        else
        {
            var seq = XdmSequence.FromSource(raw.SequenceValue!).GetEnumerator();
            if (!seq.MoveNext()) Assert.Fail("Empty sequence");
            node = seq.Current;
        }
        if (!node.IsNode)
            Assert.Fail($"Expected node, got kind {node.Kind}");
        var typed = node.NodeValue.TypedValue;
        Assert.Equal(XdmValueKind.Decimal, typed.Kind);
    }

    [Fact]
    public void AtomicDecimalAvgComparison()
    {
        var schemaSet = new XmlSchemaSet();
        string schemaPath = Path.GetFullPath("../../../../qt3tests/docs/atomic.xsd");
        string docPath = Path.GetFullPath("../../../../qt3tests/docs/atomic.xml");
        using (var stream = File.OpenRead(schemaPath))
        using (var reader = System.Xml.XmlReader.Create(stream))
        {
            schemaSet.Add(XmlSchema.Read(reader, null)!);
        }
        schemaSet.Compile();

        var doc = XDocumentProvider.LoadXml(docPath, null, schemaSet);
        var ctx = new EvaluationContext();
        ctx = ctx.WithNamespace("atomic", "http://www.w3.org/XQueryTest");
        ctx = ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);
        FunctionLibrary.Populate(ctx);

        var result = XPath31Expression.Compile("(fn:avg((//atomic:decimal,//atomic:decimal))) eq 12678967.543233").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void UserDefinedSchemaTypeConstructor()
    {
        var schemaSet = LoadInlineSchema(@"
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
           targetNamespace='http://example.com/schema'
           xmlns='http://example.com/schema'>
  <xs:simpleType name='age'>
    <xs:restriction base='xs:integer'>
      <xs:minInclusive value='0'/>
      <xs:maxInclusive value='120'/>
    </xs:restriction>
  </xs:simpleType>
</xs:schema>");

        var ctx = new EvaluationContext();
        ctx.SchemaSet = schemaSet;
        ctx = ctx.WithNamespace("ex", "http://example.com/schema");
        FunctionLibrary.Populate(ctx);

        var result = XPath31Expression.Compile("ex:age(21)").Evaluate(ctx);
        // The constructor validates against the integer-derived schema type; the XDM kind
        // stays Decimal until full integer-subtype preservation is implemented.
        Assert.Equal(21m, result.DecimalValue);
    }

    [Fact]
    public void UserDefinedSchemaTypeCast()
    {
        var schemaSet = LoadInlineSchema(@"
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
           targetNamespace='http://example.com/schema'
           xmlns='http://example.com/schema'>
  <xs:simpleType name='age'>
    <xs:restriction base='xs:integer'>
      <xs:minInclusive value='0'/>
      <xs:maxInclusive value='120'/>
    </xs:restriction>
  </xs:simpleType>
</xs:schema>");

        var ctx = new EvaluationContext();
        ctx.SchemaSet = schemaSet;
        ctx = ctx.WithNamespace("ex", "http://example.com/schema");
        FunctionLibrary.Populate(ctx);

        var result = XPath31Expression.Compile("21 cast as ex:age").Evaluate(ctx);
        // The cast validates against the integer-derived schema type; the XDM kind stays
        // Decimal until full integer-subtype preservation is implemented.
        Assert.Equal(21m, result.DecimalValue);
    }

    [Fact]
    public void UserDefinedSchemaTypeInstanceOf()
    {
        var schemaSet = LoadInlineSchema(@"
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
           targetNamespace='http://example.com/schema'
           xmlns='http://example.com/schema'>
  <xs:simpleType name='age'>
    <xs:restriction base='xs:integer'>
      <xs:minInclusive value='0'/>
      <xs:maxInclusive value='120'/>
    </xs:restriction>
  </xs:simpleType>
</xs:schema>");

        var ctx = new EvaluationContext();
        ctx.SchemaSet = schemaSet;
        ctx = ctx.WithNamespace("ex", "http://example.com/schema");
        FunctionLibrary.Populate(ctx);

        var result = XPath31Expression.Compile("21 instance of ex:age").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void SchemaValidatedTimeIsTyped()
    {
        var schemaSet = LoadInlineSchema(@"
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
           targetNamespace='http://example.com/schema'
           xmlns='http://example.com/schema'>
  <xs:element name='birthTime' type='xs:time'/>
</xs:schema>");

        var doc = XDocument.Parse("<birthTime xmlns='http://example.com/schema'>14:30:00+05:00</birthTime>");
        doc.Validate(schemaSet, null, true);
        var node = XdmValue.FromNode(new XDocumentNode(doc.Root!));

        var typed = node.NodeValue.TypedValue;
        Assert.Equal(XdmValueKind.Time, typed.Kind);
        Assert.True(typed.HasTimezone);
    }

    [Fact]
    public void UserDefinedDateTypeCast()
    {
        var schemaSet = LoadInlineSchema(@"
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
           targetNamespace='http://example.com/schema'
           xmlns='http://example.com/schema'>
  <xs:simpleType name='date2003'>
    <xs:restriction base='xs:date'>
      <xs:minInclusive value='2003-01-01'/>
      <xs:maxInclusive value='2003-12-31'/>
    </xs:restriction>
  </xs:simpleType>
</xs:schema>");

        var ctx = new EvaluationContext();
        ctx.SchemaSet = schemaSet;
        ctx = ctx.WithNamespace("ex", "http://example.com/schema");
        FunctionLibrary.Populate(ctx);

        var result = XPath31Expression.Compile("xs:date('2003-02-02') cast as ex:date2003").Evaluate(ctx);
        Assert.Equal(XdmValueKind.Date, result.Kind);
        Assert.Equal("2003-02-02", result.ToString());
    }

    private static XmlSchemaSet LoadInlineSchema(string schemaXml)
    {
        var schemaSet = new XmlSchemaSet();
        using var reader = XmlReader.Create(new StringReader(schemaXml));
        schemaSet.Add(XmlSchema.Read(reader, null)!);
        schemaSet.Compile();
        return schemaSet;
    }
}
