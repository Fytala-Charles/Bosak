// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 augustus 2026
// PURPOSE              : Regression tests for preserving lexical timezone offsets in schema-validated date/time typed values.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-08-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Core.Tests;

public class SchemaDateTimeTypedValueTests
{
    private static IXdmNode CreateValidatedNode(string elementName, string xsdType, string lexicalValue)
    {
        string xsd = $$"""
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
           targetNamespace="http://test.example"
           xmlns:t="http://test.example"
           elementFormDefault="qualified">
  <xs:element name="{{elementName}}" type="{{xsdType}}"/>
</xs:schema>
""";
        var schemaSet = new XmlSchemaSet();
        using (var reader = XmlReader.Create(new StringReader(xsd)))
        {
            schemaSet.Add("http://test.example", reader);
        }
        schemaSet.Compile();

        string xml = $"""<t:{elementName} xmlns:t="http://test.example">{lexicalValue}</t:{elementName}>""";
        var doc = XDocument.Parse(xml);
        doc.Validate(schemaSet, null, true);

        return new XDocumentNode(doc.Root!);
    }

    [Fact]
    public void Date_PreservesNonLocalOffset()
    {
        var node = CreateValidatedNode("d", "xs:date", "2000-01-01+05:00");
        var value = node.TypedValue;

        Assert.Equal(XdmValueKind.Date, value.Kind);
        Assert.True(value.HasTimezone);
        Assert.Equal("2000-01-01+05:00", value.ToString());
    }

    [Fact]
    public void DateTime_PreservesUtcOffset()
    {
        var node = CreateValidatedNode("dt", "xs:dateTime", "2002-04-02T12:00:00Z");
        var value = node.TypedValue;

        Assert.Equal(XdmValueKind.DateTime, value.Kind);
        Assert.True(value.HasTimezone);
        Assert.Equal("2002-04-02T12:00:00Z", value.ToString());
    }

    [Fact]
    public void DateTime_PreservesNonLocalOffset()
    {
        var node = CreateValidatedNode("dt", "xs:dateTime", "2000-01-01T12:34:56+05:00");
        var value = node.TypedValue;

        Assert.Equal(XdmValueKind.DateTime, value.Kind);
        Assert.True(value.HasTimezone);
        Assert.Equal("2000-01-01T12:34:56+05:00", value.ToString());
    }

    [Fact]
    public void Time_PreservesUtcOffsetWithFractionalSeconds()
    {
        var node = CreateValidatedNode("t", "xs:time", "13:20:10.5Z");
        var value = node.TypedValue;

        Assert.Equal(XdmValueKind.Time, value.Kind);
        Assert.True(value.HasTimezone);
        Assert.Equal("13:20:10.5Z", value.ToString());
    }

    [Fact]
    public void Cast_DateTimeToTime_PreservesTimezone()
    {
        var node = CreateValidatedNode("dt", "xs:dateTime", "2002-04-02T12:00:00Z");
        var timeValue = VmEngine.Cast(node.TypedValue, "xs:time");

        Assert.Equal(XdmValueKind.Time, timeValue.Kind);
        Assert.Equal("12:00:00Z", timeValue.ToString());
    }

    [Fact]
    public void Cast_DateTimeToDate_PreservesTimezone()
    {
        var node = CreateValidatedNode("dt", "xs:dateTime", "2002-04-02T12:00:00Z");
        var dateValue = VmEngine.Cast(node.TypedValue, "xs:date");

        Assert.Equal(XdmValueKind.Date, dateValue.Kind);
        Assert.Equal("2002-04-02Z", dateValue.ToString());
    }
}
