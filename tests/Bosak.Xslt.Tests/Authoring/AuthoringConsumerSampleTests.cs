// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : AC-11: the documented consumer walkthrough (inspect -> descriptors -> edit -> candidate -> export), verbatim as an engine-owned test.
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
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

/// <summary>
/// AC-11: the consumer sample. The body of <see cref="Ac11_ConsumerSample_InspectEditExport_EndToEnd"/>
/// IS the documented walkthrough: public authoring APIs only, strings and in-memory bytes only, no
/// reflection and no file-system dependencies — so the documentation cannot rot against the engine.
/// </summary>
public class AuthoringConsumerSampleTests
{
    private static readonly Uri BaseUri = new("file:///C:/project/main.xsl");

    private const string Stylesheet = """
        <?xml version="1.0" encoding="UTF-8"?>
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <!-- dossier: single field lookup -->
          <xsl:template match="/">
            <result><xsl:value-of select="/input/name"/></result>
          </xsl:template>
        </xsl:stylesheet>
        """;

    private const string InputXml = "<input><name>Braid</name><id>42</id></input>";

    [Fact]
    public void Ac11_ConsumerSample_InspectEditExport_EndToEnd()
    {
        // --- Step 1: retain the stylesheet. Bytes can come from anywhere (disk, editor buffer,
        // download); the envelope takes a defensive copy, so the caller's array stays caller-owned.
        var bytes = new UTF8Encoding(false, true).GetBytes(Stylesheet);
        if (!AuthoringSource.TryCreate(bytes, BaseUri, out var source, out var failure))
        {
            throw new InvalidOperationException(failure!.Message);
        }

        // --- Step 2: inspect. The snapshot is immutable, source-backed, and safe to keep alive as
        // long as the descriptors or the original bytes are needed; it needs no disposal.
        var options = new AuthoringInspectionOptions { AttemptCompilation = true };
        var inspection = XsltAuthoring.Inspect(source!, resolver: null, options);
        if (!inspection.IsSuccess)
        {
            throw new InvalidOperationException(inspection.Failure!.Message);
        }

        var snapshot = inspection.Snapshot!;
        Assert.True(snapshot.IsCompilable, string.Join(" | ", snapshot.CompilationDiagnostics));

        // --- Step 3: navigate the descriptors to the slot the user edited. Node ids are
        // snapshot-scoped: they identify this tree only and must not be reused against later
        // inspections or candidates.
        var valueOf = snapshot.PrincipalModule.Root.FindDescendant(
            n => n.Kind == AuthoringNodeKind.Element && n.ElementName!.LocalName == "value-of")
            ?? throw new InvalidOperationException("The expected xsl:value-of was not found.");
        var select = valueOf.Attributes.Single(a => a.Name == "select");
        Assert.Equal(AuthoringAttributeSlotKind.Expression, select.SlotKind);
        Assert.Equal("/input/name", select.ExpandedValue);

        // --- Step 4: propose the replacement expression. The proposal is validated in the slot's
        // real static context (namespaces, xpath-default-namespace, base URI, version); refusals
        // are classified data, never exceptions.
        var proposal = new AuthoringEditProposal(valueOf.Id, "select", "/input/id");
        var result = snapshot.ProposeExpressionEdit(proposal);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.Failure!.Message);
        }

        // --- Step 5: review the candidate before adopting it. Only the declared affected span (the
        // attribute value) differs from the original bytes; everything else — declaration, quotes,
        // comments, line endings — is preserved because it is spliced, never re-serialized. A
        // candidate is valid only against the snapshot it was derived from.
        var candidate = result.Candidate!;
        Assert.True(candidate.IsCompilable);
        Assert.Equal("/input/id", candidate.ChangedSlot.NewRawLiteral);
        Assert.Single(candidate.AffectedRanges);
        Assert.True(candidate.NodeCorrespondence.ContainsKey(valueOf.Id));

        // --- Step 6: adopt. Export the emitted bytes (or compile the candidate straight away); the
        // input snapshot stays untouched, so a consumer can diff, discard, or chain another edit.
        var original = snapshot.ExportOriginal(BaseUri);
        var emitted = candidate.EmittedSource.ToArray();
        Assert.Equal(bytes.Length, original.Length);
        Assert.NotEqual(original, emitted);

        var output = candidate.Compile().TransformToString(new XDocumentNode(XDocument.Parse(InputXml)));
        Assert.Contains("<result>42</result>", output, StringComparison.Ordinal);

        // The input snapshot still describes the original source; a late re-submission of the same
        // proposal still resolves against it.
        var late = snapshot.ProposeExpressionEdit(proposal);
        Assert.True(late.IsSuccess, late.Failure?.ToString());
    }
}
