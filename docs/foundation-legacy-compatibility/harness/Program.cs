// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 30 October 2026
// PURPOSE              : Exercises retained-caller compatibility of legacy schema and no-schema paths.
// SPECIAL NOTES        : Core-owned memory-only caller; not external consumer or API freeze acceptance.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 30-10-2026     | Creation; retained legacy caller compatibility cases                                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Api;
using Bosak.XPath.Api.Xsd;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;

if (args.Length != 1) throw new ArgumentException("Usage: LegacyCaller <output-json>");
const string Ns = "urn:legacy-harness";
const string Schema = """
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:legacy-harness" targetNamespace="urn:legacy-harness" elementFormDefault="qualified">
  <xs:simpleType name="Amount"><xs:restriction base="xs:decimal"/></xs:simpleType>
  <xs:element name="amount" type="t:Amount"/>
  <xs:attribute name="code" type="xs:integer"/>
</xs:schema>
""";
const string Valid = "<t:amount xmlns:t='urn:legacy-harness'>12.50</t:amount>";
const string Invalid = "<t:amount xmlns:t='urn:legacy-harness'>bad</t:amount>";
var cases = new List<object>();
var failures = 0;
void Check(bool condition) { if (!condition) throw new InvalidOperationException("Caller contract assertion failed"); }
void Case(string id, Func<object> action)
{
    try { cases.Add(new { id, passed = true, value = action(), error = (string?)null }); }
    catch (Exception error) { failures++; cases.Add(new { id, passed = false, value = (object?)null, error = error.ToString() }); }
}
MemoryStream Xsd() => new(Encoding.UTF8.GetBytes(Schema));
XmlSchemaSet Schemas()
{
    var set = new XmlSchemaSet { XmlResolver = null };
    using var source = new StringReader(Schema);
    using var reader = XmlReader.Create(source, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
    set.Add(XmlSchema.Read(reader, null)!); set.Compile(); return set;
}
CompileOptions Options(XmlSchemaSet? set) => new() { SchemaSet = set, Namespaces = new Dictionary<string, string> { ["t"] = Ns } };
object Expect<T>(Action action, string? expectedCode = null) where T : Exception
{
    try { action(); }
    catch (T error)
    {
        var match = Regex.Match(error.Message, @"\b[A-Z]{4}\d{4}\b");
        var code = match.Success ? match.Value : null;
        Check(expectedCode is null || code == expectedCode);
        return new { type = error.GetType().FullName, code };
    }
    throw new InvalidOperationException("Expected failure was not raised: " + typeof(T).FullName);
}

Case("LC-01", () => {
    var a = CompileOptions.Default; var b = CompileOptions.Default; var x = XsdValidatorOptions.Default;
    Check(!ReferenceEquals(a,b) && a.SchemaSet is null && a.Compatibility == XPathCompatibility.XPath31 && !a.StrictStaticTyping);
    Check(!x.TreatWarningsAsErrors && x.MaxErrorCount == 0 && x.BaseUri is null);
    return new { compatibility = a.Compatibility.ToString(), a.StrictStaticTyping, schemaAbsent = a.SchemaSet is null, x.TreatWarningsAsErrors, x.MaxErrorCount, x.BaseUri };
});
Case("LC-02", () => {
    var c = new EvaluationContext();
    Check(c.SchemaSet is null && !c.IsSchemaAware && c.GetSchemaElement(Ns,"amount") is null && c.GetSchemaAttribute(Ns,"code") is null && c.GetSchemaType(Ns,"Amount") is null);
    return new { schemaAbsent = true, elementAbsent = true, attributeAbsent = true, typeAbsent = true, c.IsSchemaAware };
});
Case("LC-03", () => {
    var s = Schemas(); var c = new EvaluationContext { SchemaSet = s };
    Check(ReferenceEquals(s,c.SchemaSet) && ReferenceEquals(s.GlobalElements[new XmlQualifiedName("amount",Ns)],c.GetSchemaElement(Ns,"amount")));
    Check(c.GetSchemaAttribute(Ns,"code") is not null && c.GetSchemaType(Ns,"Amount") is not null && !c.IsSchemaAware);
    return new { retainedSet = true, element = c.GetSchemaElement(Ns,"amount")!.QualifiedName.ToString(), attribute = c.GetSchemaAttribute(Ns,"code")!.QualifiedName.ToString(), type = c.GetSchemaType(Ns,"Amount")!.QualifiedName.ToString(), c.IsSchemaAware };
});
Case("LC-04", () => {
    var c = new EvaluationContext { SchemaSet = Schemas() };
    Check(c.GetSchemaElement(Ns,"missing") is null && c.GetSchemaAttribute(Ns,"missing") is null && c.GetSchemaType(Ns,"missing") is null);
    return new { elementAbsent = true, attributeAbsent = true, typeAbsent = true };
});
Case("LC-05", () => {
    var c = new EvaluationContext { SchemaSet = Schemas(), IsSchemaAware = true }; c.SchemaSet = null;
    Check(c.GetSchemaElement(Ns,"amount") is null && c.GetSchemaAttribute(Ns,"code") is null && c.GetSchemaType(Ns,"Amount") is null && c.IsSchemaAware);
    return new { schemaCleared = true, lookupsAbsent = true, c.IsSchemaAware };
});
Case("LC-06", () => { var s = Schemas(); var o = Options(s); Check(ReferenceEquals(o.SchemaSet,s)); var compiled = XPath31Expression.Compile("schema-element(t:amount)",o); Check(compiled is not null); return new { retainedSet = true, compiled = true }; });
Case("LC-07", () => Expect<Exception>(() => XPath31Expression.Compile("schema-element(t:amount)",Options(null)),"XPST0008"));
Case("LC-08", () => Expect<Exception>(() => XPath31Expression.Compile("schema-element(t:missing)",Options(Schemas())),"XPST0008"));
Case("LC-09", () => Expect<Exception>(() => XPath31Expression.Compile("schema-element(q:amount)",Options(Schemas())),"XPST0081"));
Case("LC-10", () => { Check(XPath31Expression.Compile("schema-attribute(t:code)",Options(Schemas())) is not null); return new { compiled = true }; });
Case("LC-11", () => {
    var node = XDocumentNode.Wrap(XDocument.Parse("<amount>12.50</amount>"));
    var text = XPath31Expression.Compile("string(/amount)").Evaluate(node).StringValue;
    var count = XPath31Expression.Compile("count(/amount)").Evaluate(node).IntegerValue;
    Check(text == "12.50" && count == 1); return new { text, count };
});
Case("LC-12", () => { using var s = Xsd(); IXsdValidator v = new XsdValidator(); var r = v.ValidateSafe(Valid,s); Check(r.IsValid && r.Errors.Count == 0 && s.CanRead); return new { r.IsValid, errors = r.Errors.Count, inputOpen = s.CanRead }; });
Case("LC-13", () => { using var s = Xsd(); var r = new XsdValidator().ValidateSafe(Invalid,s); Check(!r.IsValid && r.ErrorsOnly.Count() == 1); return new { r.IsValid, errors = r.ErrorsOnly.Count(), warnings = r.WarningsOnly.Count() }; });
Case("LC-14", () => { using var s = Xsd(); var r = new XsdValidator().ValidateSafe("<t:amount xmlns:t='urn:legacy-harness'>12.50",s); Check(!r.IsValid && r.ErrorsOnly.Any()); return new { r.IsValid, errors = r.ErrorsOnly.Count() }; });
Case("LC-15", () => { using var s = Xsd(); return Expect<XmlSchemaValidationException>(() => new XsdValidator().Validate(Invalid,s)); });
Case("LC-16", () => { using var a = Xsd(); using var b = Xsd(); IXsdValidator v = new XsdValidator(); var safe = v.ValidateSafe(Valid,new Stream[]{a}); var strict = v.Validate(Valid,new Stream[]{b}); Check(safe.IsValid && strict.IsValid); return new { safe = safe.IsValid, strict = strict.IsValid }; });
Case("LC-17", () => {
    var el = XElement.Parse(Valid); var r = XdmSchemaAnnotator.ValidateSubtree(el,Schemas()); var node = XDocumentNode.Wrap(el);
    var data = XPath31Expression.Compile("data(.)").Evaluate(node);
    Check(r.IsValid && node.SchemaTypeAnnotation == (Ns,"Amount") && node.TypedValue.DecimalValue == 12.50m && data.DecimalValue == 12.50m);
    return new { r.IsValid, schemaNamespace = node.SchemaTypeAnnotation!.Value.NamespaceUri, schemaName = node.SchemaTypeAnnotation.Value.LocalName, kind = data.Kind.ToString(), value = data.DecimalValue, node.HasNoTypedValue };
});
Case("LC-18", () => {
    var el = XElement.Parse(Invalid); var r = XdmSchemaAnnotator.ValidateSubtree(el,Schemas()); Check(!r.IsValid && r.Errors.Count > 0);
    var error = Expect<XmlSchemaValidationException>(() => XdmSchemaAnnotator.ValidateSubtree(XElement.Parse(Invalid),Schemas(),throwOnInvalid:true));
    return new { r.IsValid, errors = r.Errors.Count, error };
});
Case("LC-19", () => {
    var el = XElement.Parse(Valid); var s = Schemas(); Check(XdmSchemaAnnotator.ValidateSubtree(el,s).IsValid); var info = el.GetSchemaInfo();
    Check(XdmSchemaAnnotator.Validate(el,s,new XdmValidationOptions(XdmValidationMode.Preserve)).IsValid && ReferenceEquals(info,el.GetSchemaInfo()));
    Check(XdmSchemaAnnotator.Validate(el,s,new XdmValidationOptions(XdmValidationMode.Strip)).IsValid);
    var node = XDocumentNode.Wrap(el); Check(node.SchemaTypeAnnotation is null && el.GetSchemaInfo() is null);
    return new { preserveSameInfo = true, annotationStripped = true, kind = node.TypedValue.Kind.ToString(), text = node.StringValue };
});
Case("LC-20", () => {
    var el = XElement.Parse("<anything>12.50</anything>"); var r = XdmSchemaAnnotator.Validate(el,Schemas(),new XdmValidationOptions(XdmValidationMode.Strict,new XmlQualifiedName("Amount",Ns)));
    var node = XDocumentNode.Wrap(el); Check(r.IsValid && node.TypedValue.DecimalValue == 12.50m);
    return new { r.IsValid, kind = node.TypedValue.Kind.ToString(), value = node.TypedValue.DecimalValue };
});
var caller = Assembly.GetExecutingAssembly();
var loaded = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name!.StartsWith("Bosak.",StringComparison.Ordinal)).OrderBy(a => a.GetName().Name,StringComparer.Ordinal)
    .Select(a => new { name = a.GetName().Name, identity = a.FullName, informationalVersion = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(a.Location))).ToLowerInvariant() }).ToArray();
var output = new { status = "Core-owned retained caller only; no consumer or freeze acceptance", passed = cases.Count-failures, failed = failures, cases,
    callerSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(caller.Location))).ToLowerInvariant(), requestedBosakReferences = caller.GetReferencedAssemblies().Where(a => a.Name!.StartsWith("Bosak.",StringComparison.Ordinal)).Select(a => a.FullName).Order(StringComparer.Ordinal).ToArray(), loadedAssemblies = loaded };
File.WriteAllText(args[0],JsonSerializer.Serialize(output,new JsonSerializerOptions{WriteIndented=true})+"\n");
Console.WriteLine($"Legacy caller: {cases.Count-failures} passed; {failures} failed.");
return failures == 0 ? 0 : 1;
