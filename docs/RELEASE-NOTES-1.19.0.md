<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.19.0 public release">
<h1>Bosak 1.19.0 release notes</h1>
<p>Published literal-text insertion release - 18 October 2026</p>
</div>

Published immutable tag `v1.19.0` at **`27691fdb3708bb4ffd7c984abbf3196b7cde5667`**. All ten coordinated libraries target .NET 10. [Readiness and approval boundary](RELEASE-1.19.0-READINESS.md). [Public artifact verification](RELEASE-1.19.0-PUBLICATION.md) is complete: ten signed packages, 153 broad/282 focused fresh public-only checks, exact provenance and clear audits. Braid adoption/native acceptance remains separate.

## Bounded literal-text insertion

REQ-131 adds separate snapshot-owned typed targets/proposals/results/candidates and text/readable capabilities for appending one literal xsl:text in a supported principal-module template/literal-result constructor parent. PreserveSource remains the default; explicit Readable supplies complete bounded layout edits without fallback. Literal Unicode is never XPath/AVT/TVT or markup: the engine validates XML scalars/surrogates, escapes markup and encodes TAB/CR/LF as numeric references, with local `expand-text="no"`. Whitespace and normalization distinctions are retained. Empty payload is valid and emits a paired real instruction with no invented payload descriptor or select slot.

Ordinary isolated candidates supply exact source edits, instruction and encoded payload ranges, the actual optional payload descriptor, complete new-subtree evidence and defensive exported bytes. NodeCorrespondence is a total injective old-ID to candidate-ID dictionary scoped to the owning snapshots, as for value-of insertion. Deletion's descriptor-reference map and separate whitespace-merge relation are a different schema. Consumers verify ownership, graph partitions and byte replay before granting current Apply authority.

## Compatibility and boundaries

Existing value-of/deletion defaults and numeric failure values are preserved; InvalidPayload is appended to insertion failures. Published1.18.0 already allowed existing xsl:text in value-of insertion, including several DOE/whitespace-expansion forms. Those successes remain unchanged. Only the new text operation independently restricts existing text to ordinary text-only bodies, no effective expansion and absent/no DOE.

Original and candidate source each have an inclusive 8,388,608-byte text budget including BOM and complete Readable footprints. UTF-8 BOM/no-BOM and BOM-marked UTF-16 LE/BE retain strict encoding/declaration rules. Null programmer arguments/undefined modes throw; invalid payloads and unsupported scope refuse. Cancellation returns insertion-style Cancelled; deletion retains its throwing contract. Immutable old engine snapshots remain independently usable; consumers enforce current revision/license/session authority.

Existing mixed/preserved/CDATA/dependency/version/static/resource exclusions remain. There is no arbitrary markup/subtree insertion, broader XSLT conformance, automatic Apply or resource authority. Text insertion intentionally changes output.

## Verification and consumer state

Upstream build and **9,726 solution +84 language-server tests** pass. Focused **291** =65 text +177 legacy insertion/readable +49 deletion. Frozen QT3 **31,142/0/679**, qt4 gated **8,278/0/1,894**. Exact-head and integrated local-package consumers each pass **282** public cases with ten archive/nine assembly revision/cache/output/restore provenance and clear audits. Ten internal controls remain source-only.

[Braid's unchanged independent review](REQ-131-CONSUMER-IMPLEMENTATION-REVIEW.md) reports no blockers and overlapping 291/282/298 passes;298 includes 16 new public-API probes. Those runs use local exact-merge archives, not published packages. Its accepted 1.18 production baseline revalidation passes1,015; no 1.19 pin/runtime integration or native text acceptance is claimed. Original fixture correction receipts and warning-override details remain in the review.

After verified publication, Braid independently adopts the immutable delivery and implements the text-specific frozen review/Apply/selection/history and worker/export/reopen workflow before native acceptance. REQ-130 acceptance cannot accept REQ-131. VS Code 0.2.5 continues bundling 1.16.5.

[Implementation/API details](REQ-131-IMPLEMENTATION.md), [contract](REQ-131-TEXT-INSERTION-CONTRACT.md), [acceptance matrix](REQ-131-TEXT-INSERTION-ACCEPTANCE.md), [PR #364](https://github.com/Fytala-Charles/Bosak/pull/364).
