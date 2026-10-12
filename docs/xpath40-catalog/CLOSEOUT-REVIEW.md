<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog completion and follow-up review">
  <br><br><h1>Catalog Close-out Review</h1>
  <p>Remaining acceptance work and observed implementation boundaries</p>
</div>

## Decision and evidence

**REQ-126 remains In Progress; version remains 1.1.2.** Reviewed on 2026-10-12 against Bosak `3a078fa4cb8b17233837670d41e0ef951da86cd1` (merged PR #186). The catalog contains 67 entries, four worked chapters, 1,473 feature checks and 24 chapter checks: 1,497 executable documentation checks. The [name inventory](audit-function-inventory.json) assigns all 117 registered names; the [registration inventory](audit-registration-arities.json) scopes 68 non-constructor names. The remaining 49 names are focus constructors. These inventories do not prove all signatures, productions or conformance.

The [completeness audit](COMPLETENESS-AUDIT.md) and [supplementary audit](SUPPLEMENTARY-COVERAGE-AUDIT.md) retain historical deliveries. Reviewed section-1.2 and wider deferred/rejected decisions now have records. Name assignment, those records and the 1.1.2 map entries/items result-shape correction are completed milestones. Remaining work must not be described as missing records already supplied.

## Acceptance review

| Criterion | Current assessment | Remaining evidence |
|---|---|---|
| AC-01: landed slices and traceability | Partial | Full slice/grammar/signature mapping, exact tiers, per-contract source/tests. The 28 baseline provenance rows have delivery evidence but no historical adopted-specification pins. |
| AC-02: deferred/rejected decisions | Partial | Validate the dossier-to-record matrix and superseded decisions against source/history; supplied records alone do not certify exhaustive lifecycle coverage. |
| AC-03: examples and qt4tests gate references | Partial | Current fixtures establish their stated outcomes. The criterion literally requests qt4tests sets for every entry. XSLT-only, refusal and out-of-scope records need applicable harness evidence and explicit absent-gate status; acceptance treatment remains unresolved. Do not invent gates or silently waive this requirement. |
| AC-04: structure and drift | Partial | All 67 entries have numbered fields/index links. Complete feature-specific drift and historical lifecycle review; generic prior-art disclaimers are insufficient. |
| AC-05: discoverability | Met for current scope | Keep canonical links/status synchronized through the remaining deliveries. |

A current immutable review pin can describe today's comparison, but cannot reconstruct a historically adopted revision. Retain those 28 gaps unless actual historical evidence is recovered.

## Documentation delivery queue

| Order | Deliverable | Reviewable completion evidence |
|---|---|---|
| D1 — supplied | Expand [focus constructors](operators-and-syntax/focus-functions.md) across all 49 names | 508 new checks cover all 49 names, representative values/types, errors/context/cardinality, explicit modes and full typed delivery-import input/output. Exhaustive casting remains unclaimed; no engine behavior changed. |
| D2 — partial | Signature, keyword, default and options matrix | [First matrix](SIGNATURE-CONTRACT-AUDIT.md) supplies all 68 families and 20 selected existing functions, plus 187 availability/behavior checks. Complete missing keyword metadata, semantic defaults/types/options and an exhaustive changed-3.1 inventory; current samples do not close D2. |
| D3 | Grammar/type/pattern matrix | Map REQ-118 S0–S8 and REQ-123 productions to parser/compiler tests, catalog entries and version gates. Include coercion/casts, focus syntax, record updates and XSLT patterns. Name absent productions rather than infer completeness from function counts. |
| D4 | Acceptance reconciliation | Validate disposition lifecycle and feature-specific drift, retain unavailable historical pins, and record the agreed treatment of AC-03's blanket gate requirement. Close only when each criterion has reviewable evidence. |

Each delivery needs canonical documentation synchronization, executable examples where behavior is asserted, solution build/tests and rendered-document verification. Engine changes additionally require the manual gates in AGENTS.md. This review authorizes no new release or deferred feature implementation.

## Observed engine follow-ups

The linked entries and [fixture manifest](examples/cases.json) contain reproducible current outcomes. Passing fixtures sometimes preserve a known gap; they are not assertions of draft conformance. Before any engine slice, select the exact existing case, retain a regression for its current consumer scenario, and add a specification-derived expected-behavior test. Prioritize bounded existing-contract defects before larger deferred features.

| Priority | Existing evidence | Consumer impact and follow-up |
|---|---|---|
| E1 | [URI option policy](functions/uri-option-policy.md) | Non-Boolean option truthiness and empty-query-map fallback can change resource identifiers. Verify pinned option types and component precedence before correcting them. Raw-authority bypass is draft-defined and is not included as a defect. |
| E1 | [Signature contract audit](SIGNATURE-CONTRACT-AUDIT.md) | Three further keyword spelling differences, replace positional/reference #2 refusal versus keyword defaults, and tested two-let hash keyword failure need scoped engine review. Current examples reproduce behavior without diagnosing the cause. |
| E1 | [Measurement comparison](functions/measurement-comparison.md) | Keyword spelling differs from the pinned draft; legacy binary validation needs review. Verify draft value1/value2 versus implemented comparand1/comparand2, including compatibility effects. |
| E1 | [Batch extrema](functions/batch-extrema.md), [warehouse sorting](functions/warehouse-pick-sorting.md) | Mixed numeric promotion/result types, duration boundaries, NaN ordering, empty-key defaults and unknown collation handling can alter selected or ordered business records. Separate contract-specific fixes and retain typed/empty/invalid cases. |
| E2 | [Offer ranking](functions/supplier-offer-ranking.md) | Computed-key boundary ties require draft comparison; successful ranking examples do not prove tie completeness. |
| E2 | [Text cleanup](functions/customer-text-cleanup.md) | Approximate grapheme segmentation needs Unicode contract review. Do not infer complete segmentation from simple strings. |
| E2 | [Supplementary options audit](SUPPLEMENTARY-COVERAGE-AUDIT.md) | Hash options currently establish shape validation and tested opaque-key tolerance; additional parameters are implementation-defined in the pinned contract. Required BLAKE3/CRC-32 are unsupported FOHA0001. Separate extension policy from required algorithm implementation/dependency decisions. |
| E2 | [XSLT record declarations](deferred/xslt-record-type.md) | Structural-record @as conversion raises XTTE0570 in tested cases; explicit cast in select with map(*) is a documented workaround. Named declarations remain deferred. |
| E2 | [Capturing accumulators](deferred/capturing-accumulators.md) | Accepted capture retains source identity; invalid/start-phase forms are tolerated. Snapshot alternatives do not implement streaming capture or special annotation copying. |
| E2 | [JNode boundary](data-model/jnode-template-boundary.md) | Tested node() is false, xnode() refuses XPST0051 and same-JNode identity comparison raises XPTY0004. Scope the generic-model work separately from already shipped navigation. |
| E2 | [Serialization parameters](deferred/serialization-parameters.md) | Nine new fn:serialize keys are ignored. JSON Lines, canonical/layout/CSV boundaries remain documented; XSLT solidus control works. Prioritize explicit parameter validation/handling decisions before broad serialization additions. |

Missing fn:apply-templates, named XSLT types, enclosing modes, computed XPath constructors, external-input functions and full XQuery/Streaming 4.0 are documented deferred/out-of-scope features. They require individual scope and implementation decisions rather than automatic inclusion in a bug-fix release. Existing XQuery 3.1 and streaming APIs remain distinct from those full 4.0 claims.
