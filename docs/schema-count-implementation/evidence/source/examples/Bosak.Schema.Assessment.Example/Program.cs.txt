// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Runs native XSD 1.1 assessment and consumes a genuinely assessed decimal in Bosak.
// SPECIAL NOTES        : Bounded native XSD 1.1 assessment; decimal-sequence-assertions-v1 profile.
//
// COPYRIGHT            : Fytala
// LICENSE              : LICENSE.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-10-2026     | Creation (REQ-009 bounded document assessment, 0.5 development line)                      |
//                      | Charles Korthout | 0.2   | 29-10-2026     | Demonstrate original whole-document annotations through XPath and XSLT                    |
//                      | Charles Korthout | 0.3   | 29-10-2026     | Demonstrate an omitted optional particle without shifting later state (0.6)               |
//                      | Charles Korthout | 0.4   | 29-10-2026     | Demonstrate ordered repetitions, shared authority and ambiguous projection refusal (0.7)  |
//                      | Charles Korthout | 0.5   | 29-10-2026     | Validate repeated count and optional absence through actual native assertions (0.8)       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.Schema;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;

var schema = XDocument.Parse("""
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" version="1.1">
      <xs:element name="order"><xs:complexType>
        <xs:sequence>
          <xs:element name="amount" type="xs:decimal" maxOccurs="2"/>
          <xs:element name="tax" type="xs:decimal" minOccurs="0"/>
          <xs:element name="shipped" type="xs:decimal"/>
        </xs:sequence>
        <xs:assert test="count(amount) eq 2 and count(tax) eq 0 and shipped gt 0"/>
      </xs:complexType></xs:element>
    </xs:schema>
    """);
var assessor = SchemaServices.CreateDocumentAssessor(schema);
var assessment = assessor.Assess(XDocument.Parse("<order><amount>12.50</amount><amount>2.50</amount><shipped>2</shipped></order>"));
if (assessment.Outcome != SchemaAssessmentOutcome.Valid)
{
    foreach (var diagnostic in assessment.Diagnostics) Console.Error.WriteLine(diagnostic);
    return 1;
}
var document = assessment.Document!;
var input = new XDocumentNode(document.CreateDocumentCopy(preserveTypeAnnotations: true));
var context = new EvaluationContext().WithFocus(XdmValue.FromNode(input), 1, 1);
Console.WriteLine("Assessment: " + assessment.Outcome);
Console.WriteLine("Profile: " + assessor.Profile);
Console.WriteLine("Native count assertions: passed");
Console.WriteLine("Later declaration retained: " + ReferenceEquals(assessor.ChildDeclarations[2], document.Elements[3].Declaration));
Console.WriteLine("Shared declaration: " + ReferenceEquals(document.Elements[1].Declaration, document.Elements[2].Declaration));
Console.WriteLine("Distinct occurrence states: " + !ReferenceEquals(document.Elements[1], document.Elements[2]));
Console.WriteLine("Typed decimals: " + XPath31Expression.Compile("every $n in /order/amount satisfies data($n) instance of xs:decimal").Evaluate(context).BooleanValue);
Console.WriteLine("Total: " + XPath31Expression.Compile("sum(/order/amount)").Evaluate(context).DecimalValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
// Aggregation is a host XPath query, not a newly admitted schema assertion function.
try
{
    document.CreateTypedElementCopy("amount");
    Console.Error.WriteLine("Ambiguous singular projection unexpectedly succeeded.");
    return 1;
}
catch (NotSupportedException error) when (error.Message.StartsWith("BSK-XSD-016:", StringComparison.Ordinal))
{
    Console.WriteLine("Ambiguous singular projection: refused");
}
Console.WriteLine("Original root state: " + ReferenceEquals(document.Elements[0], XdmElementAssessment.Get(((XDocument)input.UnderlyingObject).Root!)!.State));

const string stylesheet = """
    <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                    xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs">
      <xsl:template name="main">
        <xsl:variable name="preserved" as="element()"><xsl:copy-of select="/*" validation="preserve"/></xsl:variable>
        <xsl:variable name="stripped" as="element()"><xsl:copy-of select="/*" validation="strip"/></xsl:variable>
        <result total="{sum(/order/amount)}" values="{string-join($preserved/amount ! string(data(.)), ',')}" preserved="{every $n in $preserved/amount satisfies data($n) instance of xs:decimal}" stripped="{every $n in $stripped/amount satisfies data($n) instance of xs:untypedAtomic}"/>
      </xsl:template>
    </xsl:stylesheet>
    """;
var compiler = SchemaAwareXslt.CreateCompiler();
// Legacy annotation-copy context only: no 1.1 schema is routed through the BCL validator.
compiler.SchemaSet = new XmlSchemaSet();
Console.WriteLine(compiler.Compile(stylesheet).TransformToString(
    input, initialTemplate: "main"));
return 0;
