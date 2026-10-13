// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verify fixed namespace resolution for named types in authoring validation.
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
//                      | Charles Korthout | 0.2   | 13-10-2026     | Link captured named types using include and import precedence.                           |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Tests named-type registration without ordinary stylesheet compilation.</summary>
public class Xslt40AuthoringNamedTypeTests
{
    private static readonly Uri MainUri = new("file:///types/main.xsl");
    private static string Sheet(string declarations, string body) => "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' fixed-namespaces='#standard t=urn:types'>" + declarations + body + "</xsl:stylesheet>";
    private static XsltValidationResult Validate(string text)
    {
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var inspection = XsltAuthoring.Inspect(source!, options: new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(inspection.IsSuccess);
        Assert.Equal(Encoding.UTF8.GetBytes(text), inspection.Snapshot!.ExportOriginal(MainUri));
        return XsltValidation.Validate(inspection.Snapshot);
    }

    /// <summary>Expression and AVT slots receive expanded module-local named aliases.</summary>
    /// <param name="definition">The alias definition.</param>
    /// <param name="value">An instance of the alias.</param>
    [Theory]
    [InlineData("xs:integer", "7")]
    [InlineData("enum('red','blue')", "'red'")]
    [InlineData("array(xs:integer)", "[7]")]
    [InlineData("map(xs:string,xs:integer)", "map{ 'a':7 }")]
    [InlineData("element(root)", "/*")]
    [InlineData("(xs:integer|xs:string)", "7")]
    public void NamedAliasesAgreeWithExecution(string definition, string value)
    {
        var text = Sheet("<xsl:item-type name='t:value' as=\"" + definition + "\"/>", "<xsl:template match='/'><out result=\"{" + value + " instance of t:value}\"><xsl:value-of select=\"" + value + " instance of t:value\"/></out></xsl:template>");
        var result = Validate(text);
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
        var output = XElement.Parse(new Api.XsltCompiler().Compile(text).TransformToString(new XDocumentNode(XDocument.Parse("<root/>"))));
        Assert.Equal("true", output.Value);
        Assert.Equal("true", output.Attribute("result")!.Value);
    }

    /// <summary>Forward aliases expand independently of declaration order.</summary>
    [Fact]
    public void ForwardAliasWorks()
    {
        var text = Sheet("<xsl:item-type name='t:a' as='array(t:b)'/><xsl:item-type name='t:b' as='xs:integer'/>", "<xsl:template match='/'><out><xsl:value-of select='[7] instance of t:a'/></out></xsl:template>");
        Assert.Equal(XsltValidationOutcome.Valid, Validate(text).Outcome);
    }

    /// <summary>Known record names can appear in type checks without constructor registration.</summary>
    [Fact]
    public void RecordNameWorks()
    {
        var text = Sheet("<xsl:record-type name='t:r'><xsl:field name='value' as='xs:integer'/></xsl:record-type><xsl:item-type name='t:a' as='t:r'/>", "<xsl:template match='/'><out><xsl:value-of select='map{} instance of t:a'/></out></xsl:template>");
        Assert.Equal(XsltValidationOutcome.Valid, Validate(text).Outcome);
        _ = new Api.XsltCompiler().Compile(text);
    }

    /// <summary>Definition errors remain visible instead of registering incomplete aliases.</summary>
    /// <param name="declarations">The invalid declarations.</param>
    /// <param name="code">The expected diagnostic.</param>
    [Theory]
    [InlineData("<xsl:item-type name='t:a' as='t:b'/><xsl:item-type name='t:b' as='t:a'/>", "XTSE4035")]
    [InlineData("<xsl:item-type name='t:a' as='t:missing'/>", "XPST0051")]
    [InlineData("<xsl:item-type name='t:a' as='xs:integer'/><xsl:item-type name='t:a' as='xs:string'/>", "XTSE4030")]
    [InlineData("<xsl:item-type name='t:a' as='xs:integer?'/>", "XTSE0020")]
    [InlineData("<xsl:item-type name='t:a'/>", "XTSE0010")]
    [InlineData("<xsl:item-type name='xs:a' as='xs:integer'/>", "XTSE4030")]
    [InlineData("<xsl:item-type name='t:a' as='xs:integer' visibility='final'/>", "XTSE0020")]
    public void InvalidDefinitionsAreReported(string declarations, string code)
    {
        var text = Sheet(declarations, "<xsl:template match='/'><out/></xsl:template>");
        Assert.Contains(Validate(text).Diagnostics, d => d.Code == code);
        Assert.Contains(code, Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(text)).ToString());
    }

    /// <summary>Literal exclusion removes invalid declarations from the captured registry.</summary>
    [Fact]
    public void ExcludedTypeIsIgnored()
    {
        var text = Sheet("<xsl:item-type name='t:bad' as='t:missing' use-when='false()'/><xsl:item-type name='t:a' as='xs:integer'/>", "<xsl:template match='/'><out><xsl:value-of select='7 instance of t:a'/></out></xsl:template>");
        Assert.Equal(XsltValidationOutcome.Valid, Validate(text).Outcome);
    }
    /// <summary>Registered contexts reject undeclared named-type references.</summary>
    /// <param name="expression">The invalid type reference.</param>
    /// <param name="code">The expected XPath error.</param>
    [Theory]
    [InlineData("7 instance of t:missing", "XPST0051")]
    public void TypeReferencesAreChecked(string expression, string code)
    {
        var text = Sheet("<xsl:item-type name='t:a' as='array(xs:integer)'/>", "<xsl:template match='/'><out><xsl:value-of select='" + expression + "'/></out></xsl:template>");
        Assert.Contains(Validate(text).Diagnostics, d => d.Code == code);
    }

    /// <summary>Pattern predicates receive the same named-type registry.</summary>
    [Fact]
    public void PatternPredicateRejectsUnknownType()
    {
        var text = Sheet("<xsl:item-type name='t:a' as='xs:integer'/>", "<xsl:template match='root[. instance of t:missing]'/>");
        Assert.Contains(Validate(text).Diagnostics, d => d.Code == "XPST0051");
    }

    /// <summary>Non-literal exclusion creates a coverage gap without evaluating static expressions.</summary>
    [Fact]
    public void NonLiteralExclusionIsDeferred()
    {
        var text = Sheet("<xsl:item-type name='t:a' as='xs:integer' use-when='1 eq 1'/>", "<xsl:template match='/'><out/></xsl:template>");
        Assert.Equal(XsltValidationOutcome.UnsupportedCoverage, Validate(text).Outcome);
    }

    /// <summary>Namespace documents are captured once and validation performs no further resolution.</summary>
    [Fact]
    public void NamespaceDocumentIsCapturedOnce()
    {
        var text = Sheet("<xsl:item-type name='t:a' as='xs:integer'/>", "<xsl:template match='/'><out><xsl:value-of select='7 instance of t:a'/></out></xsl:template>").Replace("t=urn:types", "bindings.xml");
        var resolver = new NamespaceResolver();
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!, resolver, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(result.IsSuccess);
        Assert.Equal(1, resolver.Requests);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(result.Snapshot!).Outcome);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(result.Snapshot!).Outcome);
        Assert.Equal(1, resolver.Requests);
        Assert.Equal(Encoding.UTF8.GetBytes(text), result.Snapshot!.ExportOriginal(MainUri));
    }


    /// <summary>Refused namespace documents are diagnosed without retrying the resolver.</summary>
    [Fact]
    public void RefusedNamespaceDocumentIsNotRetried()
    {
        var text = Sheet("<xsl:item-type name='t:a' as='xs:integer'/>", "<xsl:template match='/'><out><xsl:value-of select='7 instance of t:a'/></out></xsl:template>").Replace("t=urn:types", "bindings.xml");
        var resolver = new NamespaceResolver { Refuse = true };
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!, resolver, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(result.IsSuccess);
        Assert.Equal(1, resolver.Requests);
        Assert.Contains(XsltValidation.Validate(result.Snapshot!).Diagnostics, d => d.Code == "XTSE0122");
        Assert.Equal(1, resolver.Requests);
    }


    /// <summary>Include-linked named types share the captured registry.</summary>
    [Fact]
    public void IncludedTypesAreRegistered()
    {
        var text = Sheet("<xsl:include href='types.xsl'/>", "<xsl:template match='/'><out/></xsl:template>");
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!, new IncludeResolver(), new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(result.IsSuccess);
        var validation = XsltValidation.Validate(result.Snapshot!);
        Assert.Equal(XsltValidationOutcome.Valid, validation.Outcome);
        Assert.DoesNotContain(validation.UnsupportedCoverage, g => g.Construct == "named-types");
    }

    private sealed class IncludeResolver : IAuthoringModuleResolver
    {
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Assert.Equal(new Uri("file:///types/types.xsl"), absoluteUri);
            Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(Sheet("<xsl:item-type name='t:a' as='xs:integer'/>", "")), absoluteUri, out var source, out _));
            return source;
        }
    }

    private sealed class NamespaceResolver : IAuthoringModuleResolver
    {
        /// <summary>Gets the number of caller-approved resolution requests.</summary>
        public int Requests { get; private set; }
        /// <summary>Gets or sets whether caller policy refuses the namespace document.</summary>
        public bool Refuse { get; init; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Requests++;
            if (Refuse) return null;
            Assert.Equal(new Uri("file:///types/bindings.xml"), absoluteUri);
            Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes("<bindings xmlns:t='urn:types'/>"), absoluteUri, out var source, out _));
            return source;
        }
    }

}
