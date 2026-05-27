// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 mei 2026
// PURPOSE              : Unit tests for XSD validation API.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 27-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text;
using Bosak.XPath.Api.Xsd;
using Xunit;

namespace Bosak.XPath.Api.Tests.Xsd;

public class XsdValidatorTests
{
    private static Stream ToStream(string text)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(text));
    }

    [Fact]
    public void Validate_ValidXml_ReturnsValid()
    {
        var xsd = @"<?xml version=""1.0""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""root"" type=""xs:string""/>
</xs:schema>";

        var xml = @"<?xml version=""1.0""?>
<root>hello</root>";

        var validator = new XsdValidator();
        var result = validator.TryValidate(xml, ToStream(xsd));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_InvalidXml_ReturnsErrors()
    {
        var xsd = @"<?xml version=""1.0""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""root"" type=""xs:int""/>
</xs:schema>";

        var xml = @"<?xml version=""1.0""?>
<root>not-an-int</root>";

        var validator = new XsdValidator();
        var result = validator.TryValidate(xml, ToStream(xsd));

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.OnlyErrors);
    }

    [Fact]
    public void Validate_MissingElement_ReturnsError()
    {
        var xsd = @"<?xml version=""1.0""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""root"">
    <xs:complexType>
      <xs:sequence>
        <xs:element name=""child"" type=""xs:string""/>
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>";

        var xml = @"<?xml version=""1.0""?>
<root></root>";

        var validator = new XsdValidator();
        var result = validator.TryValidate(xml, ToStream(xsd));

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.OnlyErrors);
    }

    [Fact]
    public void Validate_WithSchemaSet_MultipleSchemas()
    {
        var schemaA = @"<?xml version=""1.0""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema""
           targetNamespace=""http://example.org/a"">
  <xs:element name=""item"" type=""xs:string""/>
</xs:schema>";

        var xml = @"<?xml version=""1.0""?>
<a:item xmlns:a=""http://example.org/a"">hello</a:item>";

        var validator = new XsdValidator();
        var result = validator.TryValidate(xml, new[] { ToStream(schemaA) });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Throws_OnInvalid()
    {
        var xsd = @"<?xml version=""1.0""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""root"" type=""xs:int""/>
</xs:schema>";

        var xml = @"<?xml version=""1.0""?>
<root>bad</root>";

        var validator = new XsdValidator();
        Assert.Throws<System.Xml.Schema.XmlSchemaValidationException>(() =>
            validator.Validate(xml, ToStream(xsd)));
    }

    [Fact]
    public void Validate_MalformedXml_ReturnsParseError()
    {
        var xsd = @"<?xml version=""1.0""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""root"" type=""xs:string""/>
</xs:schema>";

        var xml = @"<root>unclosed";

        var validator = new XsdValidator();
        var result = validator.TryValidate(xml, ToStream(xsd));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("parse error", StringComparison.OrdinalIgnoreCase) || e.Message.Contains("unexpected end", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_Error_HasLineNumber()
    {
        var xsd = @"<?xml version=""1.0""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""root"" type=""xs:int""/>
</xs:schema>";

        var xml = @"<?xml version=""1.0""?>
<root>not-an-int</root>";

        var validator = new XsdValidator();
        var result = validator.TryValidate(xml, ToStream(xsd));

        Assert.False(result.IsValid);
        var error = result.OnlyErrors.First();
        Assert.True(error.LineNumber > 0, "Expected line number to be set");
    }
}
