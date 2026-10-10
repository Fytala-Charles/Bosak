// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Contract tests for expression edits: dossier transform (AC-04), failure taxonomy (AC-05), context/escaping/encoding (AC-06), revisions (AC-10).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

public class AuthoringExpressionEditTests
{
    private static readonly Uri BaseUri = new("file:///C:/project/main.xsl");
    private const string InputXml = "<input><name>Braid</name><id>42</id></input>";

    private static readonly string[] DossierLines =
    {
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
        "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">",
        "  <!-- dossier: single field lookup -->",
        "  <xsl:output method=\"xml\" encoding=\"utf-8\"/>",
        "  <xsl:template match=\"/\">",
        "    <result><xsl:value-of select=\"/input/name\"/></result>",
        "  </xsl:template>",
        "</xsl:stylesheet>",
    };

    private static string DossierText(string newline) => string.Join(newline, DossierLines);

    private static AuthoringSnapshot Inspect(string text, out byte[] bytes, Encoding? encoding = null)
    {
        encoding ??= new UTF8Encoding(false, true);
        bytes = encoding.GetBytes(text);
        Assert.True(AuthoringSource.TryCreate(bytes, BaseUri, out var source, out var failure), failure?.ToString());
        var result = XsltAuthoring.Inspect(source!);
        Assert.True(result.IsSuccess, result.Failure?.ToString());
        return result.Snapshot!;
    }

    private static AuthoringSnapshot Inspect(byte[] bytes)
    {
        Assert.True(AuthoringSource.TryCreate(bytes, BaseUri, out var source, out var failure), failure?.ToString());
        var result = XsltAuthoring.Inspect(source!);
        Assert.True(result.IsSuccess, result.Failure?.ToString());
        return result.Snapshot!;
    }

    private static AuthoringNodeDescriptor FindValueOf(AuthoringSnapshot snapshot, int ordinal = 0) =>
        snapshot.PrincipalModule.Root
            .FindDescendant(n => n.Kind == AuthoringNodeKind.Element && n.ElementName!.LocalName == "value-of")
        ?? throw new InvalidOperationException("xsl:value-of not found.");

    private static AuthoringNodeDescriptor FindNodeById(AuthoringSnapshot snapshot, int id) =>
        snapshot.PrincipalModule.Root.FindDescendant(n => n.Id == id)
        ?? throw new InvalidOperationException($"Node {id} not found.");

    private static XDocumentNode Input() => new(XDocument.Parse(InputXml));

    private static void AssertBytesOutsideRangeUnchanged(byte[] original, ReadOnlyMemory<byte> emitted, SourceRange range)
    {
        var start = (int)range.StartByteOffset;
        var length = (int)range.ByteLength;
        var actual = emitted.Span;
        Assert.True(
            original.AsSpan(0, start).SequenceEqual(actual.Slice(0, start)),
            "Bytes before the affected range must be untouched.");
        Assert.True(
            original.AsSpan(start + length).SequenceEqual(actual.Slice(actual.Length - (original.Length - start - length))),
            "Bytes after the affected range must be untouched.");
    }

    // ---------------------------------------------------------------------------------------------
    // AC-04: the dossier example — baseline transform vs edited candidate, byte-exact splice.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Ac04_Baseline_TransformsName()
    {
        var baseline = new XsltCompiler().Compile(DossierText("\n"), BaseUri.AbsoluteUri);
        Assert.Contains("<result>Braid</result>", baseline.TransformToString(Input()), StringComparison.Ordinal);
    }

    [Fact]
    public void Ac04_Candidate_TransformsEditedExpression_AndOnlyAffectedBytesDiffer()
    {
        var snapshot = Inspect(DossierText("\n"), out var original);
        var valueOf = FindValueOf(snapshot);

        var result = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(valueOf.Id, "select", "/input/id"));

        Assert.True(result.IsSuccess, result.Failure?.ToString());
        var candidate = result.Candidate!;
        Assert.True(candidate.IsCompilable);
        Assert.Empty(candidate.CompilationDiagnostics);
        Assert.Equal(new UTF8Encoding(false, true).GetByteCount("/input/id"), candidate.ChangedSlot.NewValueRange.ByteLength);
        Assert.Single(candidate.AffectedRanges);
        Assert.Equal(candidate.ChangedSlot.OldValueRange, candidate.AffectedRanges[0]);
        Assert.Equal("/input/name", candidate.ChangedSlot.OldRawLiteral);
        Assert.Equal("/input/id", candidate.ChangedSlot.NewRawLiteral);

        var output = candidate.Compile().TransformToString(Input());
        Assert.Contains("<result>42</result>", output, StringComparison.Ordinal);

        AssertBytesOutsideRangeUnchanged(original, candidate.EmittedSource, candidate.ChangedSlot.OldValueRange);
    }

    [Fact]
    public void Ac04_Correspondence_MapsEditedNodeToCandidateCounterpart()
    {
        var snapshot = Inspect(DossierText("\n"), out _);
        var valueOf = FindValueOf(snapshot);

        var candidate = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(valueOf.Id, "select", "/input/id")).Candidate!;

        Assert.True(candidate.NodeCorrespondence.TryGetValue(valueOf.Id, out var newId));
        var newNode = FindNodeById(candidate.Snapshot, newId);
        var newAttribute = newNode.Attributes.Single(a => a.Name == "select");
        Assert.Equal("/input/id", newAttribute.ExpandedValue);
        Assert.Equal(candidate.ChangedSlot.NewValueRange, newAttribute.ValueRange);
    }

    // ---------------------------------------------------------------------------------------------
    // AC-05: failure taxonomy — parse errors, non-expression slots, unknown nodes, misuse.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Ac05_ParseError_IsClassified()
    {
        var snapshot = Inspect(DossierText("\n"), out _);
        var valueOf = FindValueOf(snapshot);

        var result = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(valueOf.Id, "select", "1 +"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthoringEditFailureKind.ExpressionParseError, result.Failure!.Kind);
        Assert.Equal(valueOf.Attributes.Single(a => a.Name == "select").ValueRange, result.Failure.Range);
    }

    [Fact]
    public void Ac05_NonExpressionSlots_AreSlotNotEditable()
    {
        const string text = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template name="main" match="/">
                <r/>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var snapshot = Inspect(text, out _);
        var template = snapshot.PrincipalModule.Root.FindDescendant(
            n => n.Kind == AuthoringNodeKind.Element && n.ElementName!.LocalName == "template")!;

        var matchResult = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(template.Id, "match", "/"));
        Assert.False(matchResult.IsSuccess);
        Assert.Equal(AuthoringEditFailureKind.SlotNotEditable, matchResult.Failure!.Kind);

        var nameResult = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(template.Id, "name", "other"));
        Assert.False(nameResult.IsSuccess);
        Assert.Equal(AuthoringEditFailureKind.SlotNotEditable, nameResult.Failure!.Kind);
    }

    [Fact]
    public void Ac05_MissingAttribute_IsSlotNotEditable()
    {
        var snapshot = Inspect(DossierText("\n"), out _);
        var output = snapshot.PrincipalModule.Root.FindDescendant(
            n => n.Kind == AuthoringNodeKind.Element && n.ElementName!.LocalName == "output")!;

        var result = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(output.Id, "select", "1"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthoringEditFailureKind.SlotNotEditable, result.Failure!.Kind);
    }

    [Fact]
    public void Ac05_UnknownNode_IsClassified()
    {
        var snapshot = Inspect(DossierText("\n"), out _);

        var result = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(9999, "select", "1"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthoringEditFailureKind.UnknownNode, result.Failure!.Kind);
    }

    [Fact]
    public void Ac05_Proposal_Misuse_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new AuthoringEditProposal(1, "", "1"));
        Assert.Throws<ArgumentException>(() => new AuthoringEditProposal(1, "select", ""));
        Assert.Throws<ArgumentNullException>(() => new AuthoringEditProposal(1, null!, "1"));
        Assert.Throws<ArgumentNullException>(() => new AuthoringEditProposal(1, "select", null!));

        var snapshot = Inspect(DossierText("\n"), out _);
        Assert.Throws<ArgumentNullException>(() => snapshot.ProposeExpressionEdit(null!));
    }

    // ---------------------------------------------------------------------------------------------
    // AC-06: static context, escaping, encodings, quote styles, CRLF fidelity.
    // ---------------------------------------------------------------------------------------------

    private const string NamespacedText = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:m="urn:m">
          <xsl:template match="/">
            <r><xsl:value-of select="m:foo"/></r>
          </xsl:template>
        </xsl:stylesheet>
        """;

    [Fact]
    public void Ac06_DeclaredPrefix_Complies()
    {
        var snapshot = Inspect(NamespacedText, out _);
        var valueOf = FindValueOf(snapshot);

        var result = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(valueOf.Id, "select", "m:bar"));

        Assert.True(result.IsSuccess, result.Failure?.ToString());
        Assert.Equal("m:bar", result.Candidate!.ChangedSlot.NewRawLiteral);
    }

    [Fact]
    public void Ac06_UndeclaredPrefix_RequiresParentChange_WithStartTagOwnership()
    {
        var snapshot = Inspect(NamespacedText, out _);
        var valueOf = FindValueOf(snapshot);

        var result = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(valueOf.Id, "select", "zzz:bar"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthoringEditFailureKind.RequiresParentChange, result.Failure!.Kind);
        Assert.NotNull(result.Failure.ExpandedRange);
        // Expanded ownership is the owning element's full start tag: it starts where the element
        // starts and covers the whole attribute, through the closing '>'.
        Assert.Equal(valueOf.Range.StartLine, result.Failure.ExpandedRange!.StartLine);
        Assert.Equal(valueOf.Range.StartColumn, result.Failure.ExpandedRange.StartColumn);
        var select = valueOf.Attributes.Single(a => a.Name == "select");
        var expandedEnd = result.Failure.ExpandedRange.StartByteOffset + result.Failure.ExpandedRange.ByteLength;
        var selectEnd = select.FullRange.StartByteOffset + select.FullRange.ByteLength;
        Assert.True(expandedEnd >= selectEnd);
    }

    [Fact]
    public void Ac06_Ampersand_And_LessThan_AreEscaped()
    {
        var snapshot = Inspect(DossierText("\n"), out _);
        var valueOf = FindValueOf(snapshot);

        var amp = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(valueOf.Id, "select", "'a&b'"));
        Assert.True(amp.IsSuccess, amp.Failure?.ToString());
        Assert.Equal("'a&amp;b'", amp.Candidate!.ChangedSlot.NewRawLiteral);
        var decoded = new UTF8Encoding(false, true).GetString(amp.Candidate.EmittedSource.Span);
        Assert.Contains("select=\"'a&amp;b'\"", decoded, StringComparison.Ordinal);

        var lt = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(valueOf.Id, "select", "'a<b'"));
        Assert.True(lt.IsSuccess, lt.Failure?.ToString());
        Assert.Equal("'a&lt;b'", lt.Candidate!.ChangedSlot.NewRawLiteral);
        Assert.Contains("select=\"'a&lt;b'\"", new UTF8Encoding(false, true).GetString(lt.Candidate.EmittedSource.Span), StringComparison.Ordinal);
    }

    [Fact]
    public void Ac06_OriginalQuoteStyle_IsEscaped()
    {
        const string doubleQuoted = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <r><xsl:value-of select="/input/name"/></r>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var doubleSnapshot = Inspect(doubleQuoted, out _);
        var doubleResult = doubleSnapshot.ProposeExpressionEdit(
            new AuthoringEditProposal(FindValueOf(doubleSnapshot).Id, "select", "'\"x\"'"));
        Assert.True(doubleResult.IsSuccess, doubleResult.Failure?.ToString());
        Assert.Equal("'&quot;x&quot;'", doubleResult.Candidate!.ChangedSlot.NewRawLiteral);

        const string singleQuoted = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <r><xsl:value-of select='/input/name'/></r>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var singleSnapshot = Inspect(singleQuoted, out _);
        var singleResult = singleSnapshot.ProposeExpressionEdit(
            new AuthoringEditProposal(FindValueOf(singleSnapshot).Id, "select", "\"'\""));
        Assert.True(singleResult.IsSuccess, singleResult.Failure?.ToString());
        Assert.Equal("\"&apos;\"", singleResult.Candidate!.ChangedSlot.NewRawLiteral);
    }

    [Fact]
    public void Ac06_NonAscii_Utf8Complies_Latin1RefusesEuroButAcceptsEAcute()
    {
        var lines = new[]
        {
            "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>",
            "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">",
            "  <xsl:template match=\"/\">",
            "    <r><xsl:value-of select=\"'old'\"/></r>",
            "  </xsl:template>",
            "</xsl:stylesheet>",
        };
        var snapshot = Inspect(Encoding.Latin1.GetBytes(string.Join("\r\n", lines)));

        var euro = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(FindValueOf(snapshot).Id, "select", "'€'"));
        Assert.False(euro.IsSuccess);
        Assert.Equal(AuthoringEditFailureKind.NotRepresentableInEncoding, euro.Failure!.Kind);
        Assert.Contains("ISO-8859-1", euro.Failure.Message, StringComparison.OrdinalIgnoreCase);

        var eAcute = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(FindValueOf(snapshot).Id, "select", "'é'"));
        Assert.True(eAcute.IsSuccess, eAcute.Failure?.ToString());
        var emitted = eAcute.Candidate!.EmittedSource.Span;
        var newRange = eAcute.Candidate.ChangedSlot.NewValueRange;
        var expectedBytes = Encoding.Latin1.GetBytes("'é'");
        Assert.Equal((int)newRange.ByteLength, expectedBytes.Length);
        Assert.True(expectedBytes.AsSpan().SequenceEqual(
            emitted.Slice((int)newRange.StartByteOffset, (int)newRange.ByteLength)));
        var decoded = Encoding.Latin1.GetString(emitted);
        Assert.Contains("select=\"'é'\"", decoded, StringComparison.Ordinal);
    }

    [Fact]
    public void Ac06_CrlfModule_StaysByteExactOutsideTheEdit()
    {
        var snapshot = Inspect(DossierText("\r\n"), out var original);
        var valueOf = FindValueOf(snapshot);

        var result = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(valueOf.Id, "select", "/input/id"));

        Assert.True(result.IsSuccess, result.Failure?.ToString());
        var decoded = new UTF8Encoding(false, true).GetString(result.Candidate!.EmittedSource.Span);
        Assert.Contains("\r\n", decoded, StringComparison.Ordinal);
        AssertBytesOutsideRangeUnchanged(original, result.Candidate.EmittedSource, result.Candidate.ChangedSlot.OldValueRange);
    }

    // ---------------------------------------------------------------------------------------------
    // AC-10: chained edits, concurrent proposals, old-revision proposals, snapshot-scoped ids.
    // ---------------------------------------------------------------------------------------------

    private const string TwoSlotText = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template match="/">
            <a><xsl:value-of select="/input/name"/></a>
            <b><xsl:value-of select="/input/id"/></b>
          </xsl:template>
        </xsl:stylesheet>
        """;

    [Fact]
    public void Ac10_ChainedEdits_ProduceIndependentRevisions()
    {
        var snapshot = Inspect(TwoSlotText, out var original);
        var first = FindValueOf(snapshot);

        var firstResult = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(first.Id, "select", "'N'"));
        Assert.True(firstResult.IsSuccess, firstResult.Failure?.ToString());
        var revision1 = firstResult.Candidate!;

        // The input snapshot is untouched by its candidate.
        Assert.True(original.SequenceEqual(snapshot.ExportOriginal(BaseUri)));

        // Second edit chains off the candidate's own snapshot, targeting the untouched slot.
        var second = revision1.Snapshot.PrincipalModule.Root.FindDescendant(
            n => n.Kind == AuthoringNodeKind.Element &&
                 n.ElementName!.LocalName == "value-of" &&
                 n.Attributes.Any(a => a.Name == "select" && a.RawLiteral == "/input/id"))!;
        var secondResult = revision1.Snapshot.ProposeExpressionEdit(new AuthoringEditProposal(second.Id, "select", "'I'"));
        Assert.True(secondResult.IsSuccess, secondResult.Failure?.ToString());
        var revision2 = secondResult.Candidate!;

        var decoded = new UTF8Encoding(false, true).GetString(revision2.EmittedSource.Span);
        Assert.Contains("select=\"'N'\"", decoded, StringComparison.Ordinal);
        Assert.Contains("select=\"'I'\"", decoded, StringComparison.Ordinal);

        // Revision 1 still reflects only its own edit.
        var revision1Text = new UTF8Encoding(false, true).GetString(revision1.EmittedSource.Span);
        Assert.Contains("select=\"'N'\"", revision1Text, StringComparison.Ordinal);
        Assert.Contains("select=\"/input/id\"", revision1Text, StringComparison.Ordinal);

        // The chained correspondence maps revision-1 ids into revision-2 ids.
        Assert.True(revision2.NodeCorrespondence.TryGetValue(second.Id, out var chainedId));
        var chainedNode = FindNodeById(revision2.Snapshot, chainedId);
        Assert.Equal("'I'", chainedNode.Attributes.Single(a => a.Name == "select").ExpandedValue);
    }

    [Fact]
    public void Ac10_ConcurrentProposals_OnOneSnapshot_AreAllIndependent()
    {
        var snapshot = Inspect(DossierText("\n"), out _);
        var valueOf = FindValueOf(snapshot);
        var results = new AuthoringEditResult[50];

        Parallel.For(0, results.Length, i =>
            results[i] = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(valueOf.Id, "select", "/input/id")));

        Assert.All(results, r => Assert.True(r.IsSuccess, r.Failure?.ToString()));
        Assert.All(results, r => Assert.Equal("/input/id", r.Candidate!.ChangedSlot.NewRawLiteral));
    }

    [Fact]
    public void Ac10_OldRevisionProposal_StillResolvesAgainstItsOwnSnapshot()
    {
        var snapshot = Inspect(DossierText("\n"), out _);
        var valueOf = FindValueOf(snapshot);
        var proposal = new AuthoringEditProposal(valueOf.Id, "select", "/input/id");

        var candidate = snapshot.ProposeExpressionEdit(proposal).Candidate!;

        // The same proposal object, submitted late to its own (superseded) snapshot, still resolves —
        // proposals are validated against the snapshot they are submitted to, never a candidate.
        var late = snapshot.ProposeExpressionEdit(proposal);
        Assert.True(late.IsSuccess, late.Failure?.ToString());
        Assert.Equal(candidate.ChangedSlot.NewValueRange, late.Candidate!.ChangedSlot.NewValueRange);

        // Node ids are snapshot-scoped: the candidate's tree is a different identity space.
        var candidateValueOf = candidate.Snapshot.PrincipalModule.Root.FindDescendant(
            n => n.Kind == AuthoringNodeKind.Element && n.ElementName!.LocalName == "value-of")!;
        Assert.True(candidate.NodeCorrespondence.TryGetValue(valueOf.Id, out var mappedId));
        Assert.Equal(candidateValueOf.Id, mappedId);
        Assert.Equal("/input/id", candidateValueOf.Attributes.Single(a => a.Name == "select").ExpandedValue);
    }
}
