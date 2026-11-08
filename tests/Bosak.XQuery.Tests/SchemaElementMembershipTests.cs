// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Verifies actual schema-element membership and function signature subtyping.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Bosak.XQuery.Api;
using Xunit;

namespace Bosak.XQuery.Tests;

/// <summary>Verifies actual schema-element membership and function signature subtyping.</summary>
public class SchemaElementMembershipTests
{
    private const string Ns = "http://www.w3.org/XQueryTest/substitution";
    // Self-contained copy of the unchanged QT3 XSD 1.0 schema, not a unit-test fixture dependency.
    private const string Schema = """
        <?xml version="1.0" ?>
        <!-- A schema designed to explore corner cases relating to schema-element() and its
             interaction with substitution groups -->
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   targetNamespace="http://www.w3.org/XQueryTest/substitution"
                   elementFormDefault="qualified"
                   xmlns:s="http://www.w3.org/XQueryTest/substitution">

          <xs:complexType name="Ht" block="extension">
          <xs:sequence>
            <xs:element name="string" type="xs:string" minOccurs="0"/>
            <xs:element name="decimal" type="xs:decimal" minOccurs="0"/>
          </xs:sequence>
        </xs:complexType>

        <xs:complexType name="HtR">
          <xs:complexContent>
            <xs:restriction base="s:Ht">
              <xs:sequence>
                <xs:element name="string" type="xs:string" minOccurs="1"/>
                <xs:element name="decimal" type="xs:decimal" minOccurs="1"/>
              </xs:sequence>
            </xs:restriction>
          </xs:complexContent>
        </xs:complexType>

          <xs:complexType name="HtRR">
            <xs:complexContent>
              <xs:restriction base="s:HtR">
                <xs:sequence>
                  <xs:element name="string" type="xs:NCName" minOccurs="1"/>
                  <xs:element name="decimal" type="xs:decimal" minOccurs="1"/>
                </xs:sequence>
              </xs:restriction>
            </xs:complexContent>
          </xs:complexType>

        <xs:complexType name="HtE">
          <xs:complexContent>
            <xs:extension base="s:Ht">
              <xs:sequence>
                <xs:element name="date" type="xs:date" minOccurs="0"/>
              </xs:sequence>
            </xs:extension>
          </xs:complexContent>
        </xs:complexType>

        <xs:element name="H" type="s:Ht" nillable="true"/>

        <xs:element name="A" abstract="true" substitutionGroup="s:H" nillable="true"/>

        <xs:element name="B" abstract="false" substitutionGroup="s:A" nillable="true"/>

        <xs:element name="C" abstract="false" substitutionGroup="s:A" nillable="false"/>

        <xs:element name="D" substitutionGroup="s:A" nillable="true" block="substitution"/>

        <xs:element name="E" substitutionGroup="s:D"/>

        <xs:element name="F" substitutionGroup="s:H" type="s:HtR"/>

        <xs:element name="G" substitutionGroup="s:H" type="s:HtE"/>

        <xs:element name="cuckoo">
          <xs:complexType>
            <xs:choice>
              <xs:element name="A" type="s:Ht"/>
              <xs:element name="B" type="s:Ht"/>
              <xs:element name="C" type="s:Ht" nillable="true"/>
              <xs:element name="D" type="s:Ht"/>
              <xs:element name="E" type="s:Ht"/>
              <xs:element name="F" type="s:HtRR"/>
              <xs:element name="G" type="s:HtE"/>
            </xs:choice>
          </xs:complexType>
        </xs:element>

        </xs:schema>
        """;

    /// <summary>Validated local declarations use the named global candidate's membership constraints.</summary>
    /// <param name="xml">A validated local child of cuckoo.</param>
    /// <param name="head">The requested global head name.</param>
    /// <param name="expected">Whether the schema-element test must match.</param>
    [Theory]
    [InlineData("<A/>", "H", false)]
    [InlineData("<A/>", "A", false)]
    [InlineData("<B/>", "H", true)]
    [InlineData("<B/>", "A", true)]
    [InlineData("<C/>", "H", true)]
    [InlineData("<D/>", "H", true)]
    [InlineData("<E/>", "D", false)]
    [InlineData("<E/>", "H", true)]
    [InlineData("<D/>", "D", true)]
    [InlineData("<F><string>s</string><decimal>12.7</decimal></F>", "H", true)]
    [InlineData("<G><date>2012-10-10</date></G>", "H", false)]
    [InlineData("<G><date>2012-10-10</date></G>", "G", true)]
    [InlineData("<C xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true'/>", "H", false)]
    public void LocalCandidates_RespectAbstractBlockingAndNillability(string xml, string head, bool expected)
    {
        var schemas = CreateSchemas(Schema);
        var document = XDocument.Parse($"<cuckoo xmlns='{Ns}'>{xml}</cuckoo>");
        document.Validate(schemas, (_, e) => throw new InvalidOperationException(e.Message), true);
        var child = document.Root!.Elements().Single();
        var before = document.ToString(SaveOptions.DisableFormatting);
        var info = child.GetSchemaInfo();
        var node = XDocumentNode.Wrap(child);
        var context = Context(schemas).WithVariable("n", XdmValue.FromNode(node));
        Assert.Equal(expected, Evaluate($"$n instance of schema-element(s:{head})", schemas, context));
        Assert.Equal(before, document.ToString(SaveOptions.DisableFormatting));
        Assert.Same(info, child.GetSchemaInfo());
    }

    /// <summary>Named and inline function parameters compare actual member sets contravariantly.</summary>
    /// <param name="parameter">The actual function's parameter sequence type.</param>
    /// <param name="testedParameter">The tested function type's parameter sequence type.</param>
    /// <param name="expected">The expected function subtype result.</param>
    [Theory]
    [InlineData("s:H", "s:A", true)]
    [InlineData("s:A", "s:H", false)]
    [InlineData("s:H", "s:B", true)]
    [InlineData("s:A", "s:D", true)]
    [InlineData("s:D", "s:E", false)]
    [InlineData("s:H", "s:G", false)]
    [InlineData("s:H", "s:F", true)]
    [InlineData("s:H", "t:H", true)]
    [InlineData("s:H?", "s:A", true)]
    [InlineData("s:H", "s:A?", false)]
    [InlineData("s:H*", "s:A+", true)]
    [InlineData("s:H+", "s:A*", false)]
    [InlineData("s:H?", "s:A*", false)]
    [InlineData("s:A", "s:D?", false)]
    public void FunctionParameters_UseMemberContainment(string parameter, string testedParameter, bool expected)
    {
        static string Type(string input)
        {
            var suffix = input[^1] is '?' or '+' or '*' ? input[^1].ToString() : "";
            return "schema-element(" + (suffix.Length == 0 ? input : input[..^1]) + ")" + suffix;
        }
        var p = Type(parameter);
        var test = Type(testedParameter);
        var schemas = CreateSchemas(Schema);
        Assert.Equal(expected, Evaluate($"(function($p as {p}) as xs:integer {{3}}) instance of function({test}) as xs:integer", schemas, Context(schemas)));
        var context = new XQueryContext().WithSchemaResolver((_, _) => new MemoryStream(Encoding.UTF8.GetBytes(Schema)));
        var query = $"import schema namespace s='{Ns}'; declare namespace t='{Ns}'; declare function local:f($p as {p}) as xs:integer {{3}}; local:f#1 instance of function({test}) as xs:integer";
        Assert.Equal(expected, new XQueryCompiler().Compile(query).Evaluate(context).BooleanValue);
    }

    /// <summary>Blocked derivation methods include head and intermediate type constraints.</summary>
    /// <param name="member">The global candidate name.</param>
    /// <param name="head">The requested head name.</param>
    /// <param name="expected">The expected actual membership result.</param>
    [Theory]
    [InlineData("R", "HR", false)]
    [InlineData("X", "HX", false)]
    [InlineData("Deep", "H", false)]
    [InlineData("Mid", "H", true)]
    [InlineData("R", "R", true)]
    [InlineData("V", "Q", false)]
    [InlineData("V", "P", true)]
    public void DerivationBlocks_RespectCompiledComponents(string member, string head, bool expected)
    {
        var schemas = CreateSchemas(BlockingSchema);
        var document = XDocument.Parse($"<cuckoo xmlns='{Ns}'><{member}/></cuckoo>");
        document.Validate(schemas, (_, e) => throw new InvalidOperationException(e.Message), true);
        var context = Context(schemas).WithVariable("n", XdmValue.FromNode(XDocumentNode.Wrap(document.Root!.Elements().Single())));
        Assert.Equal(expected, Evaluate($"$n instance of schema-element(s:{head})", schemas, context));
    }

    /// <summary>Candidate annotation must derive from declaration N even when nilled or head type is broader.</summary>
    /// <param name="nilled">Whether the local candidate is nilled.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CandidateType_IsCheckedAgainstMemberDeclaration(bool nilled)
    {
        var schemas = CreateSchemas(BlockingSchema);
        string attributes = nilled ? " xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true'" : "";
        var document = XDocument.Parse($"<cuckoo xmlns='{Ns}'><N{attributes}/></cuckoo>");
        document.Validate(schemas, (_, e) => throw new InvalidOperationException(e.Message), true);
        var context = Context(schemas).WithVariable("n", XdmValue.FromNode(XDocumentNode.Wrap(document.Root!.Elements().Single())));
        Assert.False(Evaluate("$n instance of schema-element(s:H)", schemas, context));
    }

    /// <summary>Untyped elements do not gain schema membership merely by matching a name.</summary>
    [Fact]
    public void UntypedCandidate_RemainsUnmatched()
    {
        var schemas = CreateSchemas(Schema);
        var context = Context(schemas).WithVariable("n", XdmValue.FromNode(XDocumentNode.Wrap(XElement.Parse($"<B xmlns='{Ns}'/>"))));
        Assert.False(Evaluate("$n instance of schema-element(s:H)", schemas, context));
    }

    /// <summary>A member head need not have its entire group contained in its parent's group.</summary>
    [Fact]
    public void FunctionParameters_CheckAllMembersBeyondTheHeadChain()
    {
        var schemas = CreateSchemas(BlockingSchema);
        Assert.False(Evaluate("(function($p as schema-element(s:Q)) {3}) instance of function(schema-element(s:P)) as item()*", schemas, Context(schemas)));
        Assert.True(Evaluate("(function($p as schema-element(s:Q)) {3}) instance of function(schema-element(s:Q)) as item()*", schemas, Context(schemas)));
    }

    /// <summary>A nilled concrete global candidate remains valid under an abstract head.</summary>
    [Fact]
    public void NilledConcreteMember_CanMatchAbstractHead()
    {
        var schemas = CreateSchemas(Schema);
        var document = XDocument.Parse($"<B xmlns='{Ns}' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true'/>");
        document.Validate(schemas, (_, e) => throw new InvalidOperationException(e.Message), true);
        var node = XDocumentNode.Wrap(document.Root!);
        Assert.True(node.IsNilled);
        Assert.True(Evaluate("$n instance of schema-element(s:A)", schemas, Context(schemas).WithVariable("n", XdmValue.FromNode(node))));
    }

    private static XmlSchemaSet CreateSchemas(string text)
    {
        var schemas = new XmlSchemaSet();
        using var reader = XmlReader.Create(new StringReader(text));
        schemas.Add(Ns, reader);
        schemas.Compile();
        return schemas;
    }

    private static EvaluationContext Context(XmlSchemaSet schemas)
    {
        var context = new EvaluationContext { SchemaSet = schemas, IsSchemaAware = true }.WithNamespace("s", Ns).WithNamespace("t", Ns);
        FunctionLibrary.Populate(context);
        return context;
    }

    private static bool Evaluate(string expression, XmlSchemaSet schemas, EvaluationContext context)
        => XPath31Expression.Compile(expression, new CompileOptions { SchemaSet = schemas, Namespaces = new Dictionary<string, string> { ["s"] = Ns, ["t"] = Ns } }).Evaluate(context).BooleanValue;

    private const string BlockingSchema = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:s="http://www.w3.org/XQueryTest/substitution"
            targetNamespace="http://www.w3.org/XQueryTest/substitution" elementFormDefault="qualified">
          <xs:complexType name="Base"/>
          <xs:complexType name="Restr"><xs:complexContent><xs:restriction base="s:Base"/></xs:complexContent></xs:complexType>
          <xs:complexType name="Ext"><xs:complexContent><xs:extension base="s:Base"/></xs:complexContent></xs:complexType>
          <xs:complexType name="Middle" block="extension"><xs:complexContent><xs:restriction base="s:Base"/></xs:complexContent></xs:complexType>
          <xs:complexType name="DeepExt"><xs:complexContent><xs:extension base="s:Middle"/></xs:complexContent></xs:complexType>
          <xs:element name="H" type="s:Base"/>
          <xs:element name="Q" type="s:Base" block="extension"/>
          <xs:element name="P" type="s:Base" substitutionGroup="s:Q"/>
          <xs:element name="V" type="s:Ext" substitutionGroup="s:P"/>
          <xs:element name="HR" type="s:Base" block="restriction"/>
          <xs:element name="HX" type="s:Base" block="extension"/>
          <xs:element name="R" type="s:Restr" substitutionGroup="s:HR"/>
          <xs:element name="X" type="s:Ext" substitutionGroup="s:HX"/>
          <xs:element name="Mid" type="s:Middle" substitutionGroup="s:H"/>
          <xs:element name="Deep" type="s:DeepExt" substitutionGroup="s:Mid"/>
          <xs:element name="N" type="s:Restr" substitutionGroup="s:H" nillable="true"/>
          <xs:element name="cuckoo"><xs:complexType><xs:choice>
            <xs:element name="R" type="s:Restr"/><xs:element name="X" type="s:Ext"/>
            <xs:element name="Deep" type="s:DeepExt"/><xs:element name="Mid" type="s:Middle"/>
            <xs:element name="N" type="s:Base" nillable="true"/><xs:element name="V" type="s:Ext"/>
          </xs:choice></xs:complexType></xs:element>
        </xs:schema>
        """;
}
