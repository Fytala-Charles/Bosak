// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 09 November 2026
// PURPOSE              : Tests caller-owned stream and reader behavior across the Core dependency upgrade.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 09-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml;
using System.Xml.Linq;
using Bosak.Schema;
using Xunit;

namespace Bosak.Schema.Adoption.Tests;

/// <summary>Checks existing compilation input ownership without claiming native provider lifetime support.</summary>
public sealed class SchemaOwnershipTests
{
    private const string Schema = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
          <xs:element name="order"><xs:complexType>
            <xs:sequence><xs:element name="amount" type="xs:decimal"/></xs:sequence>
            <xs:assert test="amount gt 0"/>
          </xs:complexType></xs:element>
        </xs:schema>
        """;

    /// <summary>The compiled model remains usable after the caller closes its input stream.</summary>
    [Fact]
    public void CompiledSchemaDoesNotRetainCallerStream()
    {
        var input = new MemoryStream(Encoding.UTF8.GetBytes(Schema));
        var compiled = SchemaServices.CreateSchemaCompiler().Compile(input, new Uri("urn:adoption:stream"), SchemaCompileOptions.Default(XsdLanguageVersion.Xsd11));
        Assert.True(input.CanRead);
        Assert.Empty(compiled.Diagnostics);
        input.Dispose();
        var document = XElement.Parse("<order><amount>2</amount></order>");
        string before = document.ToString(SaveOptions.DisableFormatting);
        Assert.True(SchemaServices.CreateValidationService().Validate(compiled, document).IsValid);
        Assert.Equal(before, document.ToString(SaveOptions.DisableFormatting));
    }

    /// <summary>A failed compile leaves the caller-owned stream open and reusable.</summary>
    [Fact]
    public void MalformedSchemaDoesNotDisposeCallerStream()
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes("<broken>"));
        Assert.Throws<XmlException>(() => SchemaServices.CreateSchemaCompiler().Compile(input, new Uri("urn:adoption:invalid"), SchemaCompileOptions.Default(XsdLanguageVersion.Xsd11)));
        Assert.True(input.CanRead);
        input.Position = 0;
        Assert.Equal((int)'<', input.ReadByte());
    }

    /// <summary>Compilation leaves a supplied reader under caller ownership.</summary>
    [Fact]
    public void CallerReaderIsNotClosedByCompilation()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Schema));
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { CloseInput = false, DtdProcessing = DtdProcessing.Prohibit });
        var compiled = SchemaServices.CreateSchemaCompiler().Compile(reader, new Uri("urn:adoption:reader"), SchemaCompileOptions.Default(XsdLanguageVersion.Xsd11));
        Assert.NotEqual(ReadState.Closed, reader.ReadState);
        Assert.True(stream.CanRead);
        reader.Dispose();
        Assert.True(SchemaServices.CreateValidationService().Validate(compiled, XElement.Parse("<order><amount>3</amount></order>")).IsValid);
    }
}
