# REQ-127: Source-Preserving Inspection of Compact Sibling Elements

> Requesting application: Bosak.Braid. Submitted 2026-10-12. Status: Implemented; Braid verification reported by user, publication authorized. Owner: Charles Korthout. Target: 1.1.3.

## Problem and Ownership

Braid's source containment graph depends on the supported REQ-124 authoring inspection API. Valid, well-formed XSLT with adjacent compact nested sibling groups is refused with `Derived document and retained source are misaligned.` No usable graph can be projected. Adding line breaks inside nested groups changes the inspection outcome. Braid must preserve original bytes, so it cannot normalize or reparse source to bypass this refusal.

Bosak owns parsing, lexical scanning, descriptors, source maps, failure taxonomy and engine fixes/tests. Braid owns consumer source fidelity, graph projection/UI and adoption tests. This request asks for a compatible correction to supported inspection, not a new graph/AST API or an extension of validation/execution conformance. Bosak maintainers decide acceptance, diagnosis, implementation and release; Braid remains pinned until a delivered revision is reviewed.

## Verified Reproduction

Consumer runtime: .NET 10, Braid's pinned Bosak `0ff33163ba985bf8562722424db7e66c37e3566d`. Current Bosak HEAD inspected: `7ec3cb15f8490678de9bfadfa9e5ffa1ff60f524`. Git comparison shows AuthoringInspector.cs and SourceTagScanner.cs unchanged between those commits. Runtime cases below were exercised against the pinned source; current-HEAD runtime was not separately exercised. This is evidence of unchanged relevant source, not a full current-version acceptance claim.

Use UTF-8 source bytes, an absolute file URI, AuthoringSource.TryCreate and XsltAuthoring.Inspect without attempted compilation or external module acquisition. Braid reproduces through SourceReviewSession, SourceInspectionSession and BosakSourceInspector. The following uses CRLF between lines; inline nested tags have no internal line breaks:

```xml
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
<xsl:template match="/">
<result>
<group id="0"><nested><xsl:value-of select="/input/name"/></nested></group>
<group id="1"><nested><xsl:value-of select="/input/name"/></nested></group>
</result>
</xsl:template>
</xsl:stylesheet>
```

Observed: source envelope creation succeeds; inspection fails with the alignment diagnostic. Braid keeps the exact source snapshot/bytes and refuses graph projection. This is not an invalid-XML or invalid-XSLT finding. One compact group succeeds. Two groups with line breaks between group/nested/value-of/closing tags succeed. Two compact groups also fail with UTF-16 LE BOM and matching XML declaration. An eighty-group compact fixture fails; its indented equivalent succeeds. These are consumer observations, not a claimed complete root cause.

Braid evidence:

- `tests/Bosak.Braid.Infrastructure.Tests/GraphAcceptanceFixtureTests.cs`: four CompactSiblingBoundary_PreservesSource cases plus prepared-fixture inspection/refusal tests.
- `tests/TestData/Graph/native-review/minimal-compact-alignment-refusal.xsl`: two-group reproduction with Fytala fixture header.
- `tests/TestData/Graph/native-review/compact-alignment-refusal.xsl`: eighty-group refusal.
- `tests/TestData/Graph/native-review/many-groups.xsl`: indented successful layout fixture.
- `docs/GRAPH_UI_ACCEPTANCE.md`: review workflow and refusal disclosure.

Run in D:/Development/Bosak.Braid:

```powershell
dotnet test tests/Bosak.Braid.Infrastructure.Tests/Bosak.Braid.Infrastructure.Tests.csproj -c Release --no-restore --filter FullyQualifiedName~GraphAcceptanceFixtureTests
```

Current consumer tests intentionally expect the pinned failure. On adoption, convert valid compact cases to success/range-fidelity assertions; keep malformed-source refusal coverage.

## Expected Supported Behavior

Well-formed compact and indented inputs should both inspect successfully within the existing supported principal-only profile and budgets. Preserve each input's original bytes and URI independently; the descriptors need equivalent containment/expanded identities, not equal IDs, whitespace nodes or byte offsets. Do not rewrite source to make derived structure align.

## Acceptance Criteria

1. Inspect one/two/eighty compact siblings and equivalent indented forms successfully without compilation or external acquisition. Include variants with no inter-sibling whitespace, spaces, LF and CRLF.
2. Preserve principal source bytes, BOM/encoding, base URI, comments and original entity spelling. Cover UTF-8, UTF-8 BOM and supported UTF-16 LE/BE; extend existing encoding coverage as maintainers judge appropriate.
3. Return module-scoped unique descriptor IDs, correct containment order and bounded original-byte extents. Each group, nested element and value-of extent must select its own lexical element, never the next sibling or parent closing tag. Check language-slot value ranges/raw/expanded values and accurate line/column metadata.
4. Verify mixed siblings and repeated element names, empty/self-closing nodes, comments, processing instructions, text/entities and namespace aliases. No descriptor omission or arbitrary cursor advancement may silently conceal mismatch.
5. Preserve contextual principal Expression-slot replacement: editing a selected sibling's select attribute creates an isolated candidate, changes only its value extent and retains all other original bytes. Reinspect accepted candidate with correct ranges. Other siblings and original source remain unchanged.
6. Keep malformed XML and unsupported encodings clearly refused through the established failure taxonomy, with source retention. Genuine alignment failures must not publish fabricated descriptors or partial success.
7. Keep public API/capability compatibility and existing module/source/revision/resource guarantees. No new authority to edit Pattern/AVT/dependency slots or broaden validation/runtime profiles is requested.
8. Add Bosak regression tests for the minimal and larger fixtures; run relevant engine/authoring tests and required conformance gates per Bosak AGENTS.md. Document the fix, delivered commit/version, supported scope and any remaining restrictions.

## Delivery and Consumer Acceptance

Submit implementation through Bosak's normal workflow. Tell Braid the delivered revision and regression evidence. Braid then reviews the supported contract, updates its exact source pin in a separate adoption step, converts compact success expectations, checks range/source fidelity and licensed Expression editing, and reruns the full consumer suite. Keep the current safe refusal until adoption; no Braid parser workaround or engine-internal patch is authorized by this dossier.

Priority requested: P1 consumer defect; scheduling/target remains the maintainer's decision. Blocks graph inspection for affected compact documents, while indented documents, other graph work and native review can proceed independently.


## Bosak Triage Review — 2026-10-12

Reviewed on current checkout `2e5fe2f0b847573a152a577c49a8af622aa5b432`. A temporary diagnostic program called AuthoringSource.TryCreate and XsltAuthoring.Inspect with AttemptCompilation=false, an absolute file URI and no include/import references. This independently extends the consumer's pinned-runtime evidence to the current checkout; no Braid tests or adoption were performed.

24 inspection probes crossed 1/2/80 groups, compact/indented layout and UTF-8 without BOM, UTF-8 BOM, UTF-16 LE BOM and UTF-16 BE BOM (matching UTF-16 declaration). All envelopes retained exact original bytes. Eight compact two/eighty-group probes failed with Structure / `Derived document and retained source are misaligned.` and no snapshot. The other sixteen inspected successfully; success alone does not establish their range fidelity. Full descriptor and edit acceptance still requires regression tests.

**Diagnosis:** [SourceTagScanner.FindElementEnd](../src/Bosak.Xslt/Authoring/SourceTagScanner.cs) calls ScanTagInterior, which returns the exclusive offset after `>`, then increments the offset again before scanning children. When the first child starts immediately, this skips its opening `<`. A non-self-closing child's close tag then prematurely reaches depth zero. Direct scanner evidence: `<group><nested><value/></nested></group>` selects only through `</nested>` (end offset 32, expected 40); placing LF after `<group>` gives the correct extent. [DescriptorBuilder.BuildElement](../src/Bosak.Xslt/Authoring/AuthoringInspector.cs) advances the sibling cursor to that incorrect end, accounting for subsequent alignment refusal. A successful single-group inspection therefore cannot be treated as proof of correct extents.

Recommended Bosak implementation sequence:

1. Add failing compact-range regressions before correcting the scanner's exclusive-offset handling. Cover immediate nested non-self-closing and self-closing children; do not hide errors by arbitrary cursor advancement or source rewriting.
2. Add the requested 1/2/80-group whitespace/encoding matrix and assert exact original bytes/URI, unique IDs, containment, each element's own byte extent, slot raw/expanded values and line/column positions. Include mixed content, comments/PIs/CDATA, entities, repeated names and namespace aliases. Existing encoding/fidelity tests supply a baseline, not compact-sibling acceptance.
3. Exercise an Expression edit in a selected middle sibling. Assert that only its original value bytes change, other siblings/source remain unchanged, and the candidate reinspects with correct ranges. Keep Pattern/AVT/dependency restrictions intact.
4. Rerun malformed/unsupported-source, module-resolution, revision, bounds and capability tests. Preserve classified failure and absence of fabricated/partial snapshots on genuine alignment errors.
5. Run the Release solution build/full tests and required engine gates (QT3 exactly 31,142/0/679 and qt4 gated sets). Update canonical docs and publish the delivered revision/regression evidence for Braid's separate adoption. A patch release, proposed 1.1.3, is appropriate after validation; no release or target approval is claimed here.

The request is a reproducible Bosak correctness defect and its eight criteria are coherent. Recommended priority: handle this consumer blocker before the next documentation-only catalog slice. Status remains Pending until maintainer acceptance; implementation, commit/push, release and Braid adoption have not been performed by this review.

## Bosak Implementation — 2026-10-12

Source version: **1.1.3 (unpublished)**. The scanner now begins child scanning at ScanTagInterior's exclusive end without incrementing it again. Immediate nested opening markup is counted, so each group/nested/value-of extent terminates at its own closing markup and the next sibling starts at the correct source position. Source decoding, XML parsing, original-byte retention and public APIs are unchanged.

Regression file: [CompactSourceInspectionTests](../tests/Bosak.Xslt.Tests/Authoring/CompactSourceInspectionTests.cs). Before the correction, 155 of its 206 checks failed; after it, all 206 passed. This also covers successful inspections with formerly incorrect extents, not just the reported alignment refusal.

| Criteria | Implemented evidence |
|---|---|
| AC-01/02/03 | 192 combinations: 1/2/80 groups, four inter-sibling separators and four interior layouts (none, space, LF, CRLF), UTF-8/no-BOM, UTF-8 BOM and UTF-16 LE/BE BOM with declaration. Exact source bytes/URI, unique IDs, sibling order/containment, every group/nested/value-of byte extent, slot raw/expanded values and independent line/column coordinates checked. |
| AC-04 | Four encoding cases with immediate comments, PIs, CDATA, non-ASCII/entity text, alias-qualified XSLT, repeated group names and self-closing/explicitly closed empty elements; node count/order, exact ranges and raw/expanded values asserted. Five direct scanner cases cover immediate child markup, comments/PIs/CDATA and quoted greater-than entity spelling. |
| AC-05 | Four encoding cases edit only a middle sibling's contextual Expression slot to a longer value. Expected bytes are constructed by replacing that single old extent; original source and other sibling expressions are unchanged. Candidate snapshot/ranges/containment/correspondence verified. Pattern edits remain refused. |
| AC-06 | Malformed compact XML remains Structure failure with retained bytes and no snapshot; unterminated raw markup still throws from the internal scanner. Existing unsupported-encoding/invalid-byte inspection and failure-taxonomy tests remain applicable. |
| AC-07 | No public API, edit capability, profile, resolver, revision or resource-limit changes. Existing module/capability/edit/controlled validation-execution tests remain required in the full solution run. |
| AC-08 | Release solution build/full tests and required QT3/qt4 gates pass; exact outcomes recorded below. Delivery revision follows the implementation commit. |

Release and consumer adoption remain separate. No NuGet publication, release tag or Braid pin update is claimed. Braid should adopt the delivered immutable revision after its own success/range-fidelity/edit review; no source-normalizing workaround is needed.


### Final Bosak verification

Release solution build passed (12 warnings, zero errors); **6,019 tests / 0 failures / 0 skips** across ten assemblies. QT3 stays exactly **31,142/0/679**. qt4 gated-only run: **8,278/0/1,894**, zero gated failures; no gate promotion. All 206 REQ-127 regressions pass. All 24 original current-runtime reproduction probes now succeed, retaining exact bytes; all direct scanner extent probes match their expected ends. Local links/source headers/version checked; 93 catalog and six implementation/canonical pages rendered successfully. Current catalog coverage remains 67 entries, 1,791 feature + 24 chapter checks = 1,815; REQ-126 stays In Progress. Published packages remain 1.1.2.

Implementation revision: `5bab4799783569d538cec9b03d37a57454f0eebc`, [PR #192](https://github.com/Fytala-Charles/Bosak/pull/192), pushed on `fix/req127-compact-source-inspection`. CI pending; PR remains open. This subsequent documentation commit records delivery. On resume check CI and merge after success. Source 1.1.3 remains unpublished; Braid adoption is separate. Braid must separately adopt that revision and rerun its graph/source/edit acceptance suite.

Verification commands (Release):

```powershell
dotnet build Bosak.sln -c Release -m:1
dotnet test Bosak.sln -c Release --no-build -m:1
dotnet run --project tests/Bosak.XPath.Conformance -c Release --no-build
$env:BOSAK_QT4_GATE_ONLY = '1'
dotnet run --project tests/Bosak.XPath.Conformance40 -c Release --no-build -- tests/qt4tests
```


**1.1.3 release decision — 2026-10-12:** The user reports that Braid verified REQ-127 and explicitly authorized publication of 1.1.3. This records consumer verification without inventing its exact adopted pin or test totals. Existing fix CI passed; final documentation CI, merge/tag, Trusted Publishing and public-package verification are the remaining release steps. No additional features are included.
