// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 November 2026
// PURPOSE              : Verifies span validator classification, refusals and dynamic schema context
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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Preserves the qualified validator contract while avoiding bare-name strings.</summary>
public class ValidatorSpanTests
{
    private delegate bool SpanPredicate(ReadOnlySpan<char> name);
    private static readonly Action<string, EvaluationContext> Validate = typeof(VmEngine)
        .GetMethod("ValidateFunctionConversionTarget", BindingFlags.NonPublic | BindingFlags.Static)!
        .CreateDelegate<Action<string, EvaluationContext>>();
    private static readonly Func<string, bool> Known = typeof(VmEngine)
        .GetMethod("IsKnownAtomicTypeName", BindingFlags.NonPublic | BindingFlags.Static)!
        .CreateDelegate<Func<string, bool>>();
    private static readonly SpanPredicate KnownSpan = typeof(VmEngine)
        .GetMethod("IsKnownAtomicTypeNameSpan", BindingFlags.NonPublic | BindingFlags.Static)!
        .CreateDelegate<SpanPredicate>();
    // Frozen from qualified main before the refactor; not discovered from the candidate.
    private static readonly string[] ExistingNames = new[] { "string", "normalizedstring", "token", "language", "nmtoken", "name", "ncname", "id", "idref", "entity", "boolean", "integer", "int", "long", "short", "byte", "unsignedshort", "unsignedint", "unsignedlong", "unsignedbyte", "positiveinteger", "negativeinteger", "nonpositiveinteger", "nonnegativeinteger", "decimal", "double", "float", "numeric", "datetime", "datetimestamp", "date", "time", "duration", "daytimeduration", "yearmonthduration", "qname", "anyuri", "notation", "gyear", "gyearmonth", "gmonthday", "gday", "gmonth", "hexbinary", "untypedatomic", "anyatomictype", "error" };

    /// <summary>Checks every original classification name and raw nonmatching inputs.</summary>
    [Fact]
    public void Classifier_PreservesFrozenVocabularyAndRawMatching()
    {
        var context = new EvaluationContext();
        Assert.Equal(47, ExistingNames.Length);
        foreach (var name in ExistingNames)
        {
            Assert.True(Known(name)); Assert.True(KnownSpan(name.AsSpan()));
            Validate("xs:" + name, context);
        }
        foreach (var name in new[] { "", "Integer", "xs:integer", "not-a-type", "nmtokens", "idrefs", "entities", "none" })
        {
            Assert.False(Known(name)); Assert.False(KnownSpan(name.AsSpan()));
        }
        Assert.False(Known(null!)); Assert.False(KnownSpan(default));
    }

    /// <summary>Checks original normalization and built-in list/none refusal behavior.</summary>
    [Fact]
    public void Validator_PreservesNormalizationAndBuiltInRefusals()
    {
        var context = new EvaluationContext();
        foreach (var target in new[] { "xs:integer", "xs:INTEGER", "  xs:integer  ", "xs:integer?", "integer", "xsd:integer", "element(a)" })
            Validate(target, context);
        foreach (var target in new[] { "none", "xs:NMTOKENS", "xs:IDREFS", "xs:ENTITIES", "  xs:NMTOKENS?  " })
            Assert.Contains("XPST0051", Assert.Throws<InvalidOperationException>(() => Validate(target, context)).Message);
        Assert.Contains("XPTY0117", Assert.Throws<InvalidOperationException>(() =>
            VmEngine.ApplyFunctionConversion(XdmValue.FromString("p:a", "untypedAtomic"), "xs:QName", context)).Message);
    }

    /// <summary>Checks namespace rebinding and schema replacement without stale validity caching.</summary>
    [Fact]
    public void Validator_ObservesMutableSchemaAndNamespaceContext()
    {
        var schemas = new XmlSchemaSet { XmlResolver = null };
        AddSchema(schemas, "urn:list", "<xs:list itemType='xs:integer'/>");
        AddSchema(schemas, "urn:atomic", "<xs:restriction base='xs:string'/>"); schemas.Compile();
        var context = new EvaluationContext { SchemaSet = schemas }.WithNamespace("p", "urn:list");
        Assert.Contains("XPST0051", Assert.Throws<InvalidOperationException>(() => Validate("p:T", context)).Message);
        context.WithNamespace("p", "urn:atomic"); Validate("p:T", context);
        var replacement = new XmlSchemaSet { XmlResolver = null };
        AddSchema(replacement, "urn:atomic", "<xs:list itemType='xs:integer'/>"); replacement.Compile();
        context.SchemaSet = replacement;
        Assert.Contains("XPST0051", Assert.Throws<InvalidOperationException>(() => Validate("p:T", context)).Message);
    }

    /// <summary>Checks user-defined lists, list-containing unions and atomic type acceptance.</summary>
    [Fact]
    public void Validator_PreservesUserDefinedListAndUnionRefusals()
    {
        const string text = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:t='urn:types' targetNamespace='urn:types'>" +
            "<xs:simpleType name='L'><xs:list itemType='xs:integer'/></xs:simpleType>" +
            "<xs:simpleType name='U'><xs:union memberTypes='t:L xs:string'/></xs:simpleType>" +
            "<xs:simpleType name='R'><xs:restriction base='t:U'><xs:pattern value='.*'/></xs:restriction></xs:simpleType>" +
            "<xs:simpleType name='A'><xs:restriction base='xs:integer'/></xs:simpleType></xs:schema>";
        var schemas = new XmlSchemaSet { XmlResolver = null };
        using var reader = XmlReader.Create(new StringReader(text)); schemas.Add("urn:types", reader); schemas.Compile();
        var context = new EvaluationContext { SchemaSet = schemas }.WithNamespace("p", "urn:types");
        foreach (var target in new[] { "p:L", "p:U", "p:R" })
            Assert.Contains("XPST0051", Assert.Throws<InvalidOperationException>(() => Validate(target, context)).Message);
        Validate("p:A", context);
    }

    /// <summary>Checks zero bare-name allocations separately from unchanged normalization costs.</summary>
    [Fact]
    public void Validator_Allocations_AvoidBareNameStringsInIsolatedWorker()
    {
        var rows = AllocationProbeRunner.Run("validation");
        foreach (var row in rows)
        {
            var cap = row.Key switch {
                "validate/xs:INTEGER" or "validate/  xs:integer  " or "validate/xs:integer?" => 4800000L,
                "validate/xs:anyAtomicType" => 5600000L,
                "validate/element(a)" => 4000000L,
                _ => 0L };
            Assert.InRange(row.Value, 0L, cap);
        }
    }

    private static void AddSchema(XmlSchemaSet schemas, string ns, string body)
    {
        var text = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='" + ns + "'><xs:simpleType name='T'>" + body + "</xs:simpleType></xs:schema>";
        using var reader = XmlReader.Create(new StringReader(text)); schemas.Add(ns, reader);
    }
}
