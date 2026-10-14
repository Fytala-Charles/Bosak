// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verify source-preserving expression edits use captured XSLT static contexts.
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
//                      | Charles Korthout | 0.2   | 13-10-2026     | Cover malformed contexts, overloads, local profiles and chained revisions.                 |
//                      | Charles Korthout | 0.3   | 13-10-2026     | Resolve the local profile boundary using module fixed bindings.                            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 14-10-2026     | Optional keyword edits now preserve captured defaults and source ownership.                 |
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

/// <summary>Tests applicable namespaces and host keyword signatures in expression edits.</summary>
public class Xslt40ExpressionEditContextTests
{
    private static readonly Uri MainUri = new("file:///edit-context/main.xsl");
    private static string Sheet(string body, string? fixedBindings = "#standard p=urn:fixed q=urn:fixed")
        => "<?xml version='1.0' encoding='UTF-8'?>\r\n<!--keep &amp; spelling-->\r\n"
            + "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:native' exclude-result-prefixes='#all'"
            + (fixedBindings is null ? "" : " fixed-namespaces='" + fixedBindings + "'") + ">"
            + body + "</xsl:stylesheet>";
    private static string Function(string first = "p:a", string second = "p:b")
        => "<xsl:function name='p:f'><xsl:param name='" + first + "'/><xsl:param name='" + second
            + "'/><xsl:sequence select=\"$" + first + " || '|' || $" + second + "\"/></xsl:function>";
    private static string Output(string extra = "")
        => "<xsl:template match='/'><out><xsl:value-of " + extra + " select=\"'old'\"/></out></xsl:template>";
    private static AuthoringSnapshot Inspect(string text, Resolver? resolver = null)
    {
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), MainUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!, resolver, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(result.IsSuccess);
        return result.Snapshot!;
    }
    private static AuthoringNodeDescriptor Slot(AuthoringSnapshot snapshot)
        => snapshot.PrincipalModule.Root.FindDescendant(n => n.ElementName?.LocalName == "value-of")!;
    private static AuthoringEditResult Edit(AuthoringSnapshot snapshot, string expression)
        => snapshot.ProposeExpressionEdit(new AuthoringEditProposal(Slot(snapshot).Id, "select", expression));
    private static AuthoringEditCandidate AssertAccepted(string text, string expression, string expected, Resolver? resolver = null, string input = "<root/>")
    {
        var original = Encoding.UTF8.GetBytes(text);
        var snapshot = Inspect(text, resolver);
        var node = Slot(snapshot);
        var originalBindings = node.Attributes.Single(a => a.Name == "select").SlotContext!.InScopeNamespaces.ToArray();
        var result = Edit(snapshot, expression);
        Assert.True(result.IsSuccess, result.Failure?.ToString());
        var candidate = result.Candidate!;
        var range = Assert.Single(candidate.AffectedRanges);
        var start = checked((int)range.StartByteOffset);
        var end = checked((int)(range.StartByteOffset + range.ByteLength));
        Assert.True(original.AsSpan(0, start).SequenceEqual(candidate.EmittedSource.Span[..start]));
        Assert.True(original.AsSpan(end).SequenceEqual(candidate.EmittedSource.Span[^ (original.Length - end)..]));
        Assert.Equal(original, snapshot.ExportOriginal(MainUri));
        Assert.Equal(originalBindings, Slot(candidate.Snapshot).Attributes.Single(a => a.Name == "select").SlotContext!.InScopeNamespaces);
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(candidate.Snapshot).Outcome);
        Assert.Equal(candidate.ChangedSlot.NewValueRange, Slot(candidate.Snapshot).Attributes.Single(a => a.Name == "select").ValueRange);
        Assert.Equal(Slot(candidate.Snapshot).Id, candidate.NodeCorrespondence[node.Id]);
        var output = candidate.Compile().TransformToString(new XDocumentNode(XDocument.Parse(input)));
        Assert.Equal(expected, XElement.Parse(output).Value);
        return candidate;
    }
    private static AuthoringEditFailure AssertRefused(AuthoringSnapshot snapshot, string expression, AuthoringEditFailureKind kind, string message)
    {
        var original = snapshot.ExportOriginal(MainUri);
        var node = Slot(snapshot);
        var result = Edit(snapshot, expression);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Candidate);
        var failure = result.Failure!;
        Assert.Equal(kind, failure.Kind);
        Assert.Contains(message, failure.Message, StringComparison.Ordinal);
        Assert.Equal(node.Attributes.Single(a => a.Name == "select").ValueRange, failure.Range);
        Assert.Equal(original, snapshot.ExportOriginal(MainUri));
        return failure;
    }

    /// <summary>Fixed-only prefixes are already available without changing the start tag.</summary>
    [Fact]
    public void FixedOnlyNodePrefixIsAccepted()
        => AssertAccepted(Sheet(Output()), "string(/root/q:value)", "fixed", input: "<root xmlns:q='urn:fixed'><q:value>fixed</q:value></root>");

    /// <summary>Fixed bindings override local native rebinding during edit compilation.</summary>
    [Fact]
    public void FixedReboundNodePrefixIsAccepted()
        => AssertAccepted(Sheet(Output("xmlns:p='urn:local'")), "string(/root/p:value)", "fixed", input: "<root xmlns:p='urn:fixed'><p:value>fixed</p:value></root>");

    /// <summary>Native prefixes omitted from the fixed set cannot leak into edit compilation.</summary>
    [Fact]
    public void OmittedNativePrefixRequiresParentChange()
    {
        var snapshot = Inspect(Sheet(Output(), "#standard"));
        var failure = AssertRefused(snapshot, "p:value", AuthoringEditFailureKind.RequiresParentChange, "XPST0081");
        Assert.NotNull(failure.ExpandedRange);
        Assert.Equal(Slot(snapshot).Range.StartByteOffset, failure.ExpandedRange!.StartByteOffset);
    }

    /// <summary>Captured host signatures reorder valid keyword arguments in edited expressions.</summary>
    /// <param name="expression">The replacement function call.</param>
    [Theory]
    [InlineData("p:f(p:b := 'B', p:a := 'A')")]
    [InlineData("q:f(q:b := 'B', q:a := 'A')")]
    [InlineData("Q{urn:fixed}f(Q{urn:fixed}b := 'B', Q{urn:fixed}a := 'A')")]
    [InlineData("p:f('A', q:b := 'B')")]
    [InlineData("'A' => p:f(q:b := 'B')")]
    [InlineData("declare namespace r = 'urn:fixed'; r:f(r:b := 'B', r:a := 'A')")]
    [InlineData("p:f(p:a := substring(value := 'XA', start := 2), p:b := 'B')")]
    public void HostKeywordsAreAccepted(string expression)
        => AssertAccepted(Sheet(Function() + Output()), expression, "A|B");

    /// <summary>Unprefixed keywords retain no-namespace identity with an XPath default namespace.</summary>
    [Fact]
    public void UnprefixedKeywordsIgnoreElementDefault()
        => AssertAccepted(Sheet(Function("a", "b") + Output()).Replace("version='4.0'", "version='4.0' xpath-default-namespace='urn:elements'"), "p:f(b := 'B', a := 'A')", "A|B");

    /// <summary>Invalid argument identities refuse only the owned expression value.</summary>
    /// <param name="expression">The invalid call.</param>
    [Theory]
    [InlineData("p:f(a := 'A', b := 'B')")]
    [InlineData("p:f(p:a := 'A', q:a := 'B')")]
    [InlineData("p:f('A', p:a := 'B')")]
    [InlineData("p:f(p:a := 'A')")]
    [InlineData("p:f(p:a := 'A', p:b := 'B', p:c := 'C')")]
    public void InvalidKeywordsAreExpressionErrors(string expression)
    {
        var failure = AssertRefused(Inspect(Sheet(Function() + Output())), expression, AuthoringEditFailureKind.ExpressionParseError, "XPST0017");
        Assert.Null(failure.ExpandedRange);
    }

    /// <summary>Missing keyword prefixes report start-tag ownership rather than borrowing no namespace.</summary>
    [Fact]
    public void MissingKeywordPrefixRequiresParentChange()
        => Assert.NotNull(AssertRefused(Inspect(Sheet(Function("a", "b") + Output())), "p:f(missing:a := 'A', b := 'B')", AuthoringEditFailureKind.RequiresParentChange, "XPST0081").ExpandedRange);

    /// <summary>Linked declarations supply signatures from their own module contexts.</summary>
    /// <param name="kind">The linkage instruction.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void LinkedFunctionContextsAreRetained(string kind)
    {
        var resolver = new Resolver(new() { ["functions.xsl"] = Sheet(Function(), "#standard p=urn:module") });
        AssertAccepted(Sheet("<xsl:" + kind + " href='functions.xsl'/>" + Output(), "#standard q=urn:module"), "q:f(q:b := 'B', q:a := 'A')", "A|B", resolver);
    }

    /// <summary>Principal declarations win over imported parameter names.</summary>
    [Fact]
    public void PrincipalFunctionWins()
        => AssertAccepted(Sheet("<xsl:import href='functions.xsl'/>" + Function() + Output()), "p:f(p:b := 'B', p:a := 'A')", "A|B", new Resolver(new() { ["functions.xsl"] = Sheet(Function("p:x", "p:y")) }));

    /// <summary>Namespace documents are retained for preflight; successful candidate inspection follows existing resolver policy.</summary>
    [Fact]
    public void NamespaceDocumentBindingsAreAccepted()
    {
        var resolver = new Resolver(new() { ["bindings.xml"] = "<bindings xmlns:p='urn:fixed' xmlns:q='urn:fixed'/>" });
        AssertAccepted(Sheet(Function() + Output(), "#standard bindings.xml"), "q:f(q:b := 'B', q:a := 'A')", "A|B", resolver);
    }

    /// <summary>Captured namespace refusal stops edits without retrying or expanding ownership.</summary>
    [Fact]
    public void RefusedNamespaceDocumentIsNotRetried()
    {
        var resolver = new Resolver(new());
        var snapshot = Inspect(Sheet(Output(), "#standard missing.xml"), resolver);
        Assert.Equal(1, resolver.Requests);
        var failure = AssertRefused(snapshot, "'new'", AuthoringEditFailureKind.ExpressionParseError, "XTSE0122");
        Assert.Null(failure.ExpandedRange);
        AssertRefused(snapshot, "p:value", AuthoringEditFailureKind.ExpressionParseError, "XTSE0122");
        Assert.Equal(1, resolver.Requests);
    }

    /// <summary>Deferred signatures produce capability refusals rather than claiming invalid syntax.</summary>
    /// <param name="declaration">The declaration outside captured coverage.</param>
    /// <param name="expression">The dependent keyword call.</param>
    [Theory]
    [InlineData("<xsl:function name='p:f' use-when='1 eq 1'><xsl:param name='p:a'/></xsl:function>", "p:f(p:a := 'A')")]
    [InlineData("<xsl:function name='p:f'><xsl:param name='p:a' _name=\"{'p:x'}\"/></xsl:function>", "p:f(p:a := 'A')")]
    public void DeferredSignaturesAreNotEditable(string declaration, string expression)
    {
        var failure = AssertRefused(Inspect(Sheet(declaration + Output())), expression, AuthoringEditFailureKind.SlotNotEditable, "coverage");
        Assert.DoesNotContain("XPST0017", failure.Message, StringComparison.Ordinal);
        Assert.Null(failure.ExpandedRange);
    }

    /// <summary>Refused module graphs do not retry during edit preflight.</summary>
    [Fact]
    public void RefusedModuleGraphIsNotRetried()
    {
        var resolver = new Resolver(new());
        var snapshot = Inspect(Sheet("<xsl:include href='missing.xsl'/>" + Output()), resolver);
        Assert.Equal(1, resolver.Requests);
        AssertRefused(snapshot, "p:f(p:a := 'A')", AuthoringEditFailureKind.SlotNotEditable, "coverage");
        Assert.Equal(1, resolver.Requests);
    }

    /// <summary>Earlier profiles keep native namespaces and reject keyword syntax.</summary>
    /// <param name="version">The earlier profile.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void EarlierProfilesRetainNativeBindings(string version)
    {
        var text = Sheet(Function() + Output(), null).Replace("version='4.0'", "version='" + version + "'");
        AssertAccepted(text, "p:f('A', 'B')", "A|B");
        AssertRefused(Inspect(text), "p:f(p:b := 'B', p:a := 'A')", AuthoringEditFailureKind.ExpressionParseError, "XPST0003");
    }

    /// <summary>Built-in keyword defaults do not depend on host declaration coverage.</summary>
    [Fact]
    public void BuiltInDefaultsRemainAvailable()
        => AssertAccepted(Sheet(Output()), "substring(value := 'XA', start := 2)", "A");

    /// <summary>Static use-when editing must not use ordinary stylesheet function signatures.</summary>
    [Fact]
    public void UseWhenDoesNotBorrowRuntimeSignatures()
    {
        var snapshot = Inspect(Sheet(Function() + Output("use-when='true()'")));
        var node = Slot(snapshot);
        var result = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(node.Id, "use-when", "p:f(p:a := 'A', p:b := 'B')"));
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthoringEditFailureKind.ExpressionParseError, result.Failure!.Kind);
        Assert.Contains("XPST0017", result.Failure.Message, StringComparison.Ordinal);
    }

    /// <summary>Malformed captured fixed bindings refuse even a prefix-free expression.</summary>
    /// <param name="bindings">The malformed fixed bindings.</param>
    [Theory]
    [InlineData("p=")]
    [InlineData("xmlns=urn:wrong")]
    [InlineData("xml=urn:wrong")]
    public void MalformedFixedBindingsRefuseBeforeSplicing(string bindings)
        => AssertRefused(Inspect(Sheet(Output(), bindings)), "'new'", AuthoringEditFailureKind.ExpressionParseError, "XTSE0122");

    /// <summary>Edit preflight retains module fixed bindings independently of local grammar.</summary>
    [Fact]
    public void LocalEarlierProfileRetainsModuleFixedBindings()
    {
        var text = Sheet(Output("version='3.0'"), "#standard");
        var snapshot = Inspect(text);
        AssertRefused(snapshot, "string(/root/p:value)", AuthoringEditFailureKind.RequiresParentChange, "XPST0081");
        AssertAccepted(Sheet(Output("version='3.0'")), "string(/root/q:value)", "fixed", input: "<root xmlns:q='urn:fixed'><q:value>fixed</q:value></root>");
        AssertRefused(snapshot, "substring(value := 'XA', start := 2)", AuthoringEditFailureKind.ExpressionParseError, "XPST0003");
    }

    /// <summary>Arity selects each overload's own keyword identities.</summary>
    [Fact]
    public void OverloadsUseTheirOwnParameterNames()
    {
        var overload = "<xsl:function name='p:f'><xsl:param name='p:single'/><xsl:sequence select='$p:single'/></xsl:function>";
        AssertAccepted(Sheet(Function() + overload + Output()), "p:f(p:b := 'B', p:a := p:f(p:single := 'A'))", "A|B");
    }

    /// <summary>Parameter-local native bindings are captured independently of the function element.</summary>
    [Fact]
    public void LocalParameterBindingsAreRetained()
    {
        var text = Sheet(Function() + Output(), null)
            .Replace("xmlns:p='urn:native'", "xmlns:p='urn:native' xmlns:q='urn:fixed'")
            .Replace("name='p:a'/>", "name='p:a' xmlns:p='urn:fixed'/>")
            .Replace("name='p:b'/>", "name='p:b' xmlns:p='urn:fixed'/>")
            .Replace("$p:a", "$q:a").Replace("$p:b", "$q:b");
        AssertAccepted(text, "p:f(q:b := 'B', q:a := 'A')", "A|B");
    }

    /// <summary>Candidate re-inspection preserves the registry for subsequent expression edits.</summary>
    [Fact]
    public void ChainedEditsRetainCapturedContext()
    {
        var first = AssertAccepted(Sheet(Function() + Output()), "p:f(p:b := 'B', p:a := 'A')", "A|B");
        var originalFirst = first.EmittedSource.ToArray();
        var second = Edit(first.Snapshot, "q:f(q:b := 'D', q:a := 'C')");
        Assert.True(second.IsSuccess, second.Failure?.ToString());
        Assert.Equal(originalFirst, first.EmittedSource.ToArray());
        Assert.Equal(originalFirst, first.Snapshot.ExportOriginal(MainUri));
        Assert.Equal(XsltValidationOutcome.Valid, XsltValidation.Validate(second.Candidate!.Snapshot).Outcome);
        var output = second.Candidate.Compile().TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));
        Assert.Equal("C|D", XElement.Parse(output).Value);
    }

    /// <summary>Invalid edits using captured resource-backed signatures never begin re-inspection.</summary>
    [Fact]
    public void InvalidKeywordDoesNotResolveAgain()
    {
        var resolver = new Resolver(new() { ["bindings.xml"] = "<bindings xmlns:p='urn:fixed' xmlns:q='urn:fixed'/>" });
        var snapshot = Inspect(Sheet(Function() + Output(), "#standard bindings.xml"), resolver);
        Assert.Equal(1, resolver.Requests);
        AssertRefused(snapshot, "q:f(q:a := 'A', q:a := 'B')", AuthoringEditFailureKind.ExpressionParseError, "XPST0017");
        Assert.Equal(1, resolver.Requests);
    }

    /// <summary>Later imports supply the winning signature rather than the discovery-first declaration.</summary>
    [Fact]
    public void LaterImportWins()
    {
        var resolver = new Resolver(new() { ["first.xsl"] = Sheet(Function("p:x", "p:y")), ["second.xsl"] = Sheet(Function()) });
        AssertAccepted(Sheet("<xsl:import href='first.xsl'/><xsl:import href='second.xsl'/>" + Output()), "p:f(p:b := 'B', p:a := 'A')", "A|B", resolver);
    }

    private sealed class Resolver(Dictionary<string, string> resources) : IAuthoringModuleResolver
    {
        /// <summary>Gets resource requests including candidate re-inspection and compilation.</summary>
        public int Requests { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Requests++;
            if (!resources.TryGetValue(absoluteUri.Segments.Last(), out var text)) return null;
            Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text), absoluteUri, out var source, out _));
            return source;
        }
    }    /// <summary>Optional keyword edits retain fixed parameter identities and source fidelity.</summary>
    [Fact]
    public void OptionalKeywordSignatureIsEditable()
    {
        var declaration = "<xsl:function name='p:f'><xsl:param name='p:a' required='no' select=\"'A'\"/><xsl:param name='p:b' required='no' select=\"'B'\"/><xsl:sequence select='$p:a || $p:b'/></xsl:function>";
        AssertAccepted(Sheet(declaration + Output()), "p:f(p:b := 'B')", "AB");
    }


}
