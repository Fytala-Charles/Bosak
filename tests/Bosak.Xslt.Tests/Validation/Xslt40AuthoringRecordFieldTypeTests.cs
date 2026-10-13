// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verify record-field sequence types against captured authoring namespace and type bindings.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Tests record-field type checks without compiling or evaluating the inspected stylesheet.</summary>
public class Xslt40AuthoringRecordFieldTypeTests
{
    private static readonly Uri MainUri = new("file:///record-fields/main.xsl");
    private static string Sheet(string body) => "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' fixed-namespaces='#standard t=urn:types'>" + body + "</xsl:stylesheet>";
    private static string Record(string type, string extra = "") => "<xsl:record-type name='t:r'><xsl:field name='value' as='" + type + "' " + extra + "/></xsl:record-type>";
    private static AuthoringSnapshot Inspect(string text, Resolver? resolver = null)
    {
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!, resolver, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(result.IsSuccess);
        return result.Snapshot!;
    }

    /// <summary>Unknown field types and excluded prefixes receive source-local type diagnostics.</summary>
    /// <param name="type">The field type.</param>
    /// <param name="code">The expected error code.</param>
    [Theory]
    [InlineData("t:missing", "XPST0051")]
    [InlineData("array(t:missing)", "XPST0051")]
    [InlineData("xs:missing", "XPST0051")]
    [InlineData("q:missing", "XPST0081")]
    public void InvalidFieldTypeIsReported(string type, string code)
    {
        var text = Sheet(Record(type));
        var result = XsltValidation.Validate(Inspect(text));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics, d => d.Code == code);
        Assert.Equal(MainUri, diagnostic.ModuleUri);
        Assert.NotNull(diagnostic.Range);
        // The compiler's declaration-name expander uses XTSE0280 for an unbound prefix;
        // authoring sequence-type compilation uses the XPath type diagnostic XPST0081.
        var compilerCode = code == "XPST0081" ? "XTSE0280" : code;
        Assert.Contains(compilerCode, Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(text)).ToString());
    }

    /// <summary>Malformed and expression-shaped field type values fail syntax checking.</summary>
    /// <param name="type">The malformed field type.</param>
    [Theory]
    [InlineData("")]
    [InlineData("xs:integer??")]
    [InlineData("xs:integer or true()")]
    public void MalformedFieldTypeIsReported(string type)
    {
        var result = XsltValidation.Validate(Inspect(Sheet(Record(type))));
        Assert.Contains(result.Diagnostics, d => d.Code == "XPST0003" && d.Range is not null);
    }

    /// <summary>Fields accept built-in, structural, alias and recursive record types.</summary>
    /// <param name="type">The valid field type.</param>
    [Theory]
    [InlineData("xs:integer?")]
    [InlineData("t:a*")]
    [InlineData("array(t:a)?")]
    [InlineData("map(xs:string,t:a)*")]
    [InlineData("function(t:a) as xs:string*")]
    [InlineData("t:r?")]
    public void SupportedFieldTypeIsAccepted(string type)
    {
        var text = Sheet("<xsl:item-type name='t:a' as='xs:integer'/>" + Record(type));
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(Inspect(text)).Outcome);
        _ = new Api.XsltCompiler().Compile(text);
    }

    /// <summary>Literal field and record exclusions suppress field-type checks.</summary>
    /// <param name="excludeRecord">Whether the record rather than its field is excluded.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExcludedFieldTypeIsIgnored(bool excludeRecord)
    {
        var body = excludeRecord
            ? Record("t:missing").Replace("name='t:r'", "name='t:r' use-when='false()'")
            : Record("t:missing", "use-when='false()'");
        var text = Sheet(body);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(Inspect(text)).Outcome);
        _ = new Api.XsltCompiler().Compile(text);
    }

    /// <summary>Field descriptors retain plain metadata and validation preserves original bytes.</summary>
    [Fact]
    public void FieldMetadataAndSourceArePreserved()
    {
        var text = Sheet(Record("xs:integer?"));
        var snapshot = Inspect(text);
        var field = snapshot.PrincipalModule.Root.FindDescendant(n => n.ElementName?.LocalName == "field")!;
        var attribute = Assert.Single(field.Attributes, a => a.LocalName == "as");
        Assert.Equal(AuthoringAttributeSlotKind.Plain, attribute.SlotKind);
        Assert.Null(attribute.SlotContext);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(MainUri));
    }

    /// <summary>Fields inherit the XPath default type namespace.</summary>
    [Fact]
    public void DefaultTypeNamespaceIsUsed()
    {
        var text = Sheet("<xsl:item-type name='t:a' as='xs:integer'/>" + Record("a?")).Replace("version='4.0'", "version='4.0' xpath-default-namespace='urn:types'");
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(Inspect(text)).Outcome);
        _ = new Api.XsltCompiler().Compile(text);
    }

    /// <summary>Linked aliases reach included field declarations without additional resolution.</summary>
    [Fact]
    public void IncludedFieldUsesPrincipalAlias()
    {
        var main = Sheet("<xsl:include href='fields.xsl'/><xsl:item-type name='t:a' as='xs:integer'/>");
        var module = Sheet(Record("t:a?"));
        var resolver = new Resolver(new() { ["fields.xsl"] = module });
        var snapshot = Inspect(main, resolver);
        int requests = resolver.Requests;
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(requests, resolver.Requests);
        Assert.Equal(Encoding.UTF8.GetBytes(module), snapshot.ExportOriginal(new Uri(MainUri, "fields.xsl")));
        _ = new Api.XsltCompiler { UriResolver = new AuthoringModuleUriResolverBridge(new Resolver(new() { ["fields.xsl"] = module })) }.Compile(main, MainUri.AbsoluteUri);
    }

    /// <summary>Errors in linked field types report the included module's range.</summary>
    [Fact]
    public void IncludedFieldErrorIsSourceLocal()
    {
        var main = Sheet("<xsl:include href='fields.xsl'/>");
        var resolver = new Resolver(new() { ["fields.xsl"] = Sheet(Record("t:missing")) });
        var diagnostic = Assert.Single(XsltValidation.Validate(Inspect(main, resolver)).Diagnostics, d => d.Code == "XPST0051");
        Assert.Equal(new Uri(MainUri, "fields.xsl"), diagnostic.ModuleUri);
        Assert.NotNull(diagnostic.Range);
    }

    /// <summary>Fixed namespace documents are captured once and are not resolved during validation.</summary>
    [Fact]
    public void NamespaceDocumentIsNotResolvedDuringValidation()
    {
        var text = Sheet("<xsl:item-type name='t:a' as='xs:integer'/>" + Record("t:a?")).Replace("t=urn:types", "bindings.xml");
        var resolver = new Resolver(new() { ["bindings.xml"] = "<bindings xmlns:t='urn:types'/>" });
        var snapshot = Inspect(text, resolver);
        int requests = resolver.Requests;
        Assert.True(requests > 0);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(snapshot).Outcome);
        Assert.Equal(requests, resolver.Requests);
        Assert.Equal(Encoding.UTF8.GetBytes(text), snapshot.ExportOriginal(MainUri));
    }

    /// <summary>Module fixed bindings override a conflicting native XML prefix on a field.</summary>
    [Fact]
    public void FixedBindingOverridesNativeFieldPrefix()
    {
        var text = Sheet("<xsl:item-type name='t:a' as='xs:integer'/>" + Record("t:a?", "xmlns:t='urn:wrong'"));
        var validation = XsltValidation.Validate(Inspect(text));
        Assert.True(validation.Outcome == XsltValidationOutcome.Valid, string.Join("; ", validation.Diagnostics.Select(d => d.Message)));
        _ = new Api.XsltCompiler().Compile(text);
    }

    private sealed class Resolver(Dictionary<string, string> modules) : IAuthoringModuleResolver
    {
        /// <summary>Gets the number of caller-controlled resource requests.</summary>
        public int Requests { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Requests++;
            if (!modules.TryGetValue(absoluteUri.Segments.Last(), out var text)) return null;
            Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), absoluteUri, out var source, out _));
            return source;
        }
    }
}
