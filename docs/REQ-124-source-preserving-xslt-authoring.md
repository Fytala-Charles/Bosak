<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak engine">
  <br><br>
  <h1>REQ-124 — Source-Preserving XSLT Authoring Boundary</h1>
  <p>Bosak.Braid consumer requirements and observable delivery gates</p>
</div>

> **Status:** Slices A+B+C landed 2026-10-10 (source-aware inspection + contextual expression replacement + supported adoption: `AuthoringCapabilities` descriptor, AC-11 consumer sample-as-test, lifecycle remarks, regression evidence). **REQ-124 is Implemented, pending owner acceptance.** **Submitted:** 2026-10-10. **Requester:** Bosak.Braid. **Implementation owner:** Bosak maintainers.

## 1. Purpose and Implementation Readiness

Braid needs the engine's interpretation of a stylesheet before compilation preprocessing loses original source. One engine-owned authoring snapshot feeds text/graph projections; executable state is derived. The initial registry entry defines the direction but does not settle fidelity, edit, or lifecycle contracts. This dossier supplies testable consumer requirements so Bosak can scope/design a first delivery. Exact types, assembly boundaries and algorithms remain Bosak decisions.

Braid currently has source review, unchanged-copy persistence/recovery and 48 tests, but no semantic importer/graph editor. Those tests do not prove the requested engine capability. REQ-122 accepts the product concept; REQ-124 remains Pending until Bosak accepts its scope. P1 below is Braid's consumer phase, not a promise of a Bosak release version.

## 2. Existing Surface and Gap

Source audit on 2026-10-10, Bosak checkout HEAD eed05e7 (working source inspected; not a release guarantee):

| Existing surface | Gap for authoring |
|------------------|-------------------|
| Public XsltCompiler.Compile(string, baseUri) / Compile(XDocument, baseUri) | Returns executable state, not a supported source-preserving editor handle |
| PreserveWhitespace, SetLineInfo and SetBaseUri | Useful information, but not original quote/entity spelling, byte encoding, BOM or complete lexical ownership |
| Internal Stylesheet, PatternCompiler, XPathParser and XPathAstNode | Consumer cannot depend on internals/reflection or add its own parallel interpretation |
| Stylesheet preprocessing of use-when, shadow attributes and literal-result roots | Source not executed or represented in the compiled result must still be retained for authoring |
| IXsltUriResolver supplies XDocument | Parsed module data alone cannot prove original module-byte preservation |

Expose supported capabilities rather than simply making all compiler internals public. Existing executable APIs may remain unchanged; preserve source in an authoring path and derive compilation from it.

## 3. Ownership and Non-Goals

| Responsibility | Owner |
|----------------|-------|
| Source-aware language parsing, syntax/semantic context, node/source mappings, candidate edits, supported authoring/emission API | Bosak |
| File IO, source acquisition, editor sessions/revisions, graph layout, selection, history UX, recovery, licensing and capability presentation | Braid |
| API shape, dependencies, package licensing/versioning, engine tests and supported release | Bosak |
| Consumer adoption, pinned dependency and graph/edit/export fidelity tests | Braid |

Not requested here: Avalonia UI, live synchronization orchestration, billing/licensing, cloud service, WebAssembly port, optimization passes, mandatory incremental parsing, collaboration, or unrestricted visual coverage. Braid must not introduce a second XSLT/XPath AST or parser. Source bytes may be retained by an engine handle or an associated immutable source envelope; no ownership of disk writes is transferred to Bosak.

## 4. First Consumer Workflow

1. Braid supplies original module bytes, absolute module/base URI and explicit language/resource options.
2. Bosak returns a source-aware inspection result with an opaque authoring snapshot or a clearly classified parse failure. Inspection alone performs no transformation and no implicit external resource acquisition.
3. Braid derives descriptors for one template, a literal-result constructor and an xsl:value-of expression slot. Unclassified instructions such as xsl:iterate remain opaque source-backed ranges.
4. Unchanged export returns exact original bytes. Braid writes those bytes under its own persistence policy.
5. Braid proposes changing only the expression slot from /input/name to /input/id, identifying its owning node/context and expected snapshot.
6. Bosak produces a separate candidate with diagnostics, changed-source ranges and emitted source. The old snapshot remains unchanged.
7. Braid validates expected session revision and chooses whether to install the candidate; compilation/execution use derived state and explicit inputs.
8. The real-engine test runs baseline and edited exports against sample XML, while checking retained comments, encoding and opaque source spans.

Example source intent (illustrative; not a frozen public API or engine fixture):

~~~xml
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" omit-xml-declaration="yes"/>
  <!-- preserve this comment and its surrounding source -->
  <xsl:template match="/">
    <result><xsl:value-of select="/input/name"/></result>
  </xsl:template>
</xsl:stylesheet>
~~~

For input `<input><name>Braid</name><id>42</id></input>`, baseline produces `<result>Braid</result>`; the expression edit produces `<result>42</result>`. Only edited/generated source regions may change under the agreed fidelity mode. A separate unsupported-subtree fixture proves opaque xsl:iterate retention.

## 5. Source Fidelity Contract

| Situation | Required observable behavior |
|-----------|------------------------------|
| No semantic/text edits | Every module exported as exactly its original bytes, including declaration, encoding/BOM, CRLF/LF, quotes, entity references, comments, PIs and whitespace |
| Supported localized edit | Preserve source outside the declared affected lexical regions; emitted edited text is well-formed and behaves as intended |
| Edit requires parent namespace/start-tag change | Report expanded affected ownership/ranges before emission; do not pretend an attribute-only change preserved the entire parent |
| Raw/unsupported region outside affected ownership | Preserve its bytes and inherited namespace/static/base context |
| Explicit formatting mode | Caller opts in; distinguish regenerated formatting from lossless/source-preserving emission |
| Fidelity cannot be established | Diagnostic and refused capability/edit/export; no silent normalization or loss of source |

Original bytes are an input requirement for byte-level guarantees. XDocument-only input may support inspection, but its fidelity level must explicitly say lexical/byte provenance is unavailable. Never advertise reconstructed XML as original bytes. Define supported encodings and behavior for unsupported/invalid byte sequences; non-ASCII edits must be representable in the declared encoding or explicitly rejected/migrated by caller choice.

For every regenerated region, account for XML attribute/text escaping and namespace binding. Canonical XML or structural comparison alone cannot establish exact source preservation. When regenerated output is compared semantically, record the XML comparison policy separately from byte assertions.

## 6. Inspection, Context and Source Maps

At minimum expose ordered module/template/instruction relationships, qualified element names, attribute/expression roles, source ownership and capability/opaque markers. Braid chooses which descriptors have a visual idiom. Unsupported by the visual vocabulary must not be reported as invalid XSLT merely for that reason.

Expressions/patterns must retain their owning context: namespace bindings, xpath-default-namespace, variable/static-parameter scope, base URI, language/version gates and relevant XSLT context rules. Editing a naked XPath string in a default context is insufficient. Context may be provided as opaque engine-owned data rather than a public AST.

Define range coordinates before implementation: module identity, start/end convention, offset unit (byte, Unicode scalar or UTF-16), line/column origin and newline handling. If more than one coordinate space is exposed, provide deterministic conversion through the retained source. Ranges are snapshot-scoped and cannot be reused against unrelated revisions. Contract fixtures must cover CRLF, astral characters/surrogate pairs, non-ASCII text, entities and multiline expressions.

Node identities need only be reliable within a snapshot for the first delivery. Supported candidate edits must provide correspondence for surviving nodes/source ownership so Braid can reattach selection/history/layout conservatively. Stable cross-session or arbitrary full-reparse identity is not a P1 prerequisite. Never reuse an old ID for a different node without marking the identity change.

## 7. Candidate Edits and Snapshot Lifecycle

The first edit is expression replacement in an existing owned attribute slot. Additional insert/delete/reorder/move capabilities can be delivered independently and advertised explicitly; absence of a capability is a supported outcome. Do not expose unrestricted mutation merely to satisfy graph editing.

A proposal identifies its input snapshot, owning node/slot and new expression text. Candidate creation must not mutate the input snapshot, retained source or a caller-owned XDocument. Return either a separate usable candidate plus diagnostics/change correspondence or a clear failure. Compilation is also derived and must not preprocess away source in any authoring snapshot.

Braid owns the session commit queue, application revision check, undo/redo UX and last-good draft policy. Bosak owns the validity of handles, candidate isolation and mapping. Define handle lifetime/disposal, data needed after candidate creation, and what happens if inspection, compilation or cancellation runs concurrently. Full background reparse is acceptable initially; do not promise incremental performance before measurement.

Cancellation must not return a partially mutated accepted snapshot. A late result remains attached to its input handle; Braid can discard it. Exceptions for programmer misuse/corrupt handles should be documented separately from ordinary source diagnostics.

## 8. Syntax, Semantics and Failure Outcomes

| Input/result | Consumer requirement |
|--------------|----------------------|
| Malformed/incomplete XML or unsupported encoding | Explicit parse failure; preserve original envelope/diagnostics; no fabricated complete semantic model |
| Well-formed source with semantic errors | Inspection/source ownership may remain available with diagnostics; explicitly identify compilability rather than requiring successful compilation for all editing |
| Unknown language construct or unsupported version | Preserve its source as opaque where possible; report relevant engine/version capability without discarding it |
| Missing include/import | Principal module remains inspectable with an unresolved edge/diagnostic when source retention permits it |
| Unsafe context-changing raw move | Refuse the edit or require engine-established safe rewriting; never blind copy/move |
| Failed candidate or cancelled analysis | Input snapshot unchanged; diagnostics/failure carries originating module/range where available |

Braid decides when a structurally valid candidate with semantic diagnostics may become accepted authoring state. Bosak must distinguish source structure, semantic validity, executable availability and fidelity capability so the consumer can make that decision. No universal guarantee that arbitrary invalid source is compilable is requested.

## 9. Modules, Resolution and Preprocessing

Module identities are absolute URIs; relative include/import references resolve against their own retained base URI, including xml:base where applicable. Preserve principal and module source envelopes separately and retain import/include order/precedence information. Do not flatten modules into one regenerated stylesheet without explicit caller choice.

Resolution is caller-controlled. A source-aware resolver or module-source mapping must supply raw bytes/provenance where byte fidelity is claimed. Define unresolved references, cycles, duplicate URI identity and resolver failure/cancellation. Inspection may record unresolved edges rather than fetching them implicitly. Existing XDocument-based compilation resolution remains supported at its existing fidelity level.

Retain use-when-inactive source, underscore shadow attributes and original literal-result roots. Analysis/compilation may evaluate static conditions or generate effective attributes only on derived state under explicit caller options/resource policy. Scope static parameters and cached analysis to the snapshot/options; changing them cannot overwrite original syntax. Inspection defaults must not execute an arbitrary transformation, expand external DTD/entities or acquire network content implicitly.

## 10. Capability and Compatibility Contract

Expose discoverable fidelity/inspection/edit/emission/version capabilities. Braid can start with a small vocabulary and disable unsupported graph commands rather than guessing from engine exceptions. Compatibility covers XSLT 3.0/XPath 3.1 first; preserve input version declarations and explicitly gate 4.0/frozen/experimental features through Bosak's existing version policy.

Publish supported net10.0 packaging, license/dependency metadata and a minimal consumer sample using public APIs without reflection, friend access or absolute sibling paths. Bosak decides whether this is an additive namespace, adapter package or another supported boundary. Braid's private product/license must not become an engine dependency. Existing compile APIs and existing engine/conformance gates remain regression requirements under the Bosak workflow.

## 11. Acceptance Matrix

All cases below require engine-owned contract tests. Braid independently verifies consumer adoption; its source-copy tests are not substitutes.

| ID | Test setup | Required evidence | Delivery |
|----|------------|-------------------|----------|
| AC-01 | Untouched module with BOM, CRLF, quote styles, comments/PIs/entities | Exact original-byte export and reliable ownership/ranges | Slice A |
| AC-02 | use-when false branch, shadow attributes, literal-result stylesheet | Original source survives inspection and derived compilation | Slice A |
| AC-03 | Template/constructor/expression plus opaque xsl:iterate | Ordered descriptors, context and complete opaque range retained | Slice A |
| AC-04 | /input/name -> /input/id expression edit | Separate candidate; old bytes/model unchanged; expected edited result | Slice B |
| AC-05 | Invalid expression and cancelled/failed candidate | Diagnostics classified; accepted snapshot untouched | Slice B |
| AC-06 | Namespaced expression, inherited base/default namespace, non-ASCII edit | Correct context/escaping or explicit refusal; documented affected regions | Slice B |
| AC-07 | Malformed XML, unsupported encoding/version, semantic error | Distinct structure/semantic/fidelity outcomes with no silent source loss | Slice A/B |
| AC-08 | Imported/included module, relative URI, missing module and cycle | Per-module provenance; controlled resolution; unresolved-edge/error policy | Slice A/B |
| AC-09 | Astral text, entity spelling, multiline attribute and CRLF | Tested range coordinate/conversion contract | Slice A |
| AC-10 | Retained snapshots, repeat edits, concurrent analysis and stale consumer result | Isolation/lifetime/correspondence behavior; no accidental mutation | Slice B |
| AC-11 | Public package consumed outside Bosak solution | Reproducible sample and documented capability/encoding/version limits | Slice C |
| AC-12 | Existing compile tests/conformance baselines | No regression from additive authoring work; run Bosak's current gates | Every engine slice |

Additional move/reorder tests are mandatory only before those capabilities are advertised. Test fixtures must name expected assertion strength: exact bytes, structural XML with explicit comparison policy, or semantic execution. Differential tests support a bounded declared edit scope; they are not proof for every possible XSLT program.

## 12. Suggested Delivery Slices

| Slice | Deliverable | Braid can then do |
|-------|-------------|-------------------|
| A: source-aware inspection | Supported source input, pre-preprocessing retention, read-only handle/descriptors, ranges, opaque blocks and failure taxonomy | Real import, graph projection and unchanged export |
| B: first candidate edit | Contextual expression replacement, candidate isolation/correspondence and source-preserving emission | First graph edit with edited semantic/fidelity fixtures |
| Slice C: supported adoption | Public packaging/sample/docs, capability/version/lifecycle agreement and regression evidence — **delivered 2026-10-10** (`AuthoringCapabilities`, AC-11 sample-as-test, `<remarks>` lifecycle contract, gates green) | Pin dependency and integrate P1 import/edit/export |
| Later expansion | Additional safe edits, full module editing, richer analysis and measured performance | Broader vocabulary and later phases |

Do not deliver a placeholder model that bypasses Bosak interpretation just to unblock the UI. A smaller supported read-only seam is useful; a misleading lossless claim is not. Slice order/scheduling is a proposal for Bosak acceptance, not an assigned implementation plan.

## 13. Decisions Required from Bosak

Before implementation, record the accepted slice scope, supported encodings, source-input/resolver provenance, coordinate conventions, snapshot/node lifetime, structural-versus-semantic outcome policy, first edit capability, fidelity modes, public package/license/version strategy and release/test owner. Document alternatives or refusals explicitly so Braid can adapt its scope.

No fixed latency/maximum-file-size engine promise is requested yet. Braid currently caps review at 8 MiB; choose a shared fixture corpus and measure parse/map/candidate memory and latency before setting engine targets. WebAssembly compatibility and export optimization are separate future requests if they need engine changes.

## 13.1 Bosak Decisions (recorded 2026-10-10, owner-ratified)

| Open question (§13) | Bosak decision |
|---------------------|----------------|
| Accepted slice scope | **Slice A first** (source-aware inspection), then Slice B (expression replacement); §12 order accepted as the implementation plan |
| Public package strategy (Slices A/B) | **Additive `Bosak.Xslt.Authoring` namespace inside the existing `Bosak.Xslt` package** — no new package/packaging work until Slice C |
| Packaging/release decision (Slice C) | **Keep shipping inside the existing `Bosak.Xslt` package** (it already publishes; zero packaging churn); a future package split is driven by consumer demand, not anticipated — **delivered 2026-10-10** |
| Source input/resolver provenance | Immutable engine-owned source envelope: original bytes + detected encoding (BOM, else XML-declaration charset, else UTF-8) + absolute base URI; caller-supplied module resolver for include/import provenance; unsupported encodings refused explicitly |
| Coordinate conventions | **Line/column (1-based, UTF-16 code units, matching `IXmlLineInfo`) as the primary contract; byte offsets derived deterministically through the retained source** (AC-09 fixtures: CRLF, astral/surrogate pairs, entity spelling, multiline attributes) |
| Structural-vs-semantic outcome policy | Distinct outcome kinds: structure failure, semantic diagnostics, fidelity capability, capability absence — never conflated; inspection does not require compilability |
| Fidelity modes | Lossless (original-byte envelope + splice-only emission) is the only mode in Slices A/B; any regenerated/formatting mode is a later explicit opt-in |
| First edit capability (Slice B) | Expression replacement in an owned attribute slot only; edits requiring parent namespace/start-tag changes are refused with expanded affected ranges per §5 — **delivered 2026-10-10** (`ProposeExpressionEdit`, `AuthoringEditCandidate`) |
| Snapshot/node lifetime | Snapshot-scoped node identities; engine owns handle validity and candidate isolation (§7); full reparse acceptable, no incremental promise |
| Release/test owner | Bosak maintainers; engine-owned contract tests for every AC row; existing compile/conformance gates are regression requirements per AC-12 |

Delivery model: retained-envelope + splice architecture — the original bytes are the only emission source for untouched regions; the parsed model is derived working state. This satisfies use-when/shadow-attribute/literal-result-root retention (AC-02) structurally rather than by special-casing.

## 14. Provenance and Related Records

- [Bosak registry](FEATURE_REQUESTS.md): authoritative REQ-124 status, acceptance and delivery tracking.
- [REQ-122](REQ-122-braid-visual-designer.md): accepted commercial product concept.
- Braid baseline 0b5c7fb contains docs/ARCHITECTURE.md, ADR-002, ADR-003, P1-FIRST-SLICE.md and hand-written basic-template/opaque-iterate fixtures. Braid's repository is private; local sibling checkout paths are review conveniences, not required package references. These baseline commits have not been pushed by this session, so GitHub commit links are not used as delivery evidence.
- No engine source/API changes, imported proprietary test fixtures or acceptance-status changes are made by this dossier. Example fragments above describe consumer behavior; Bosak should author/license its own engine fixtures or obtain explicit fixture reuse approval.

## 15. Consumer Reviews

- **2026-10-10 — Bosak.Braid Slice A review** (`Braid/docs/REQ-124-SLICE-A-REVIEW.md`, reviewed against main `6c651b8` / implementation `7f2338f`). Four findings; Bosak response:
  - *Line-ending-sensitive AC-02/AC-03 assertions* — already resolved on main: the Slice B fixture hardening (2026-10-10) converted hand-verified byte-offset fixtures to LF-pinned `string.Join("\n", …)` construction, making expectations independent of `core.autocrlf` checkout style while keeping LF and CRLF coverage. Verified: no further work.
  - *Caller-owned bytes alias the source envelope* — **defect, fixed same day**: `AuthoringSource.TryCreate` now takes a defensive copy of the input array (documented on the parameter); any `AuthoringSource` is immutable after construction regardless of how its bytes were obtained. Regression tests: `AuthoringReviewFindingTests` F3(a–c).
  - *Optional compilation bypassed the inspection resolver* — **defect, fixed same day**: derived compilation now resolves include/import through the same `IAuthoringModuleResolver` inspection used (internal `IXsltUriResolver` bridge, `Xml11Loader` parity, `FileSystemUriResolver` failure contract); the default file-system resolver path is byte-identical to before. Regression tests: F4(a–b) + default-path parity.
  - Adoption conditions (§3 of the review) are consumer-side precautions; the engine contract is unchanged by them. Braid may proceed with the read-only inspection adapter once these fixes are in its pinned revision.

*Last updated: 2026-10-10*
