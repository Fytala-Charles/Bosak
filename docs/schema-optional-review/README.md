# Optional decimal implementation review checkpoint — 29 October 2026

The [bounded optional implementation](../schema-optional-implementation/README.md) has been reviewed before checkpoint delivery. Its original 24,214 core/260 Schema passing results and unchanged exact gates remain applicable: every changed implementation/test/project source hash matches that qualified source. No production correction was required by this review. Earlier uncommitted notices in the sealed implementation dossier describe its original capture, not the later delivery here.

## Review findings

- Local zero/one occurrence parsing retains the documented lexical restrictions and maximum-one/enclosing-sequence rules. Required-only v1 is preserved; optional schemas use v2.
- Separate particle/instance indices reject required omissions, repetitions and order changes before assertions. Present states refer to the actual matched original declaration. Private particle arrays are not exposed through mutable collections.
- Assertion effective Boolean conversion handles empty comparisons. No grammar expansion or error-policy change is introduced for the maximum-one profile.
- Snapshot exports, later-child projections, copy owner/state and missing-child refusal are consistent with present-instance order. Empty-root normalization excludes real content and decimal leaf lexical edits; the focused controls cover both permissive and refusing paths.
- Public interfaces/layouts and product consumer pins remain unchanged. Existing bounded shape-clone allocation is deferred pending measurement; no performance guarantee or full schema/session acceptance is inferred.

The existing suite covers 56 optional cases and five core binding cases, including required-middle/optional-neighbor combinations, original identity, copies and absent/invalid/unsupported boundaries. The original 45-case before receipt and all earlier sealed evidence are preserved. Five build warnings in unchanged Standard-test/XSLT-conformance sources remain recorded, with zero build errors.

## Delivery

The user requested continuation of the recommendations to review, commit and push both repositories, then scope bounded repeated children. Current candidates remain Schema 0.6/core 1.34.1. Paired implementation delivery identities and remote targets are in CHECKPOINT.json; core delivery resolver: `git log -1 --format=%H -- docs/schema-optional-review/README.md`. Core uses the existing `checkpoint/restart-2026-10-29` branch for draft PR #467; Schema uses `feature/bounded-document-assessment`. No merge, tag or package publication.

Next: a separate pre-implementation record for bounded repeated decimal children. That scope must resolve multi-item value comparison errors and ambiguous singular exports before implementation. It does not change this optional-child checkpoint.

Schema optional implementation delivery: `773fd69faa9ff7a5bb27f1c4f8469f3024efc043`, remote-verified on `origin/feature/bounded-document-assessment` with a clean feature tree. Core review delivery resolver: `git log -1 --format=%H -- docs/schema-optional-review/README.md`; remote target `origin/checkpoint/restart-2026-10-29`. No merge or release.
