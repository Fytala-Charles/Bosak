# REQ-130 - Source-Preserving Value-of Deletion for Bosak.Braid

Submitted: 2026-10-17. Requester: Bosak.Braid. Status: Pending. Owner: Unassigned. Target: TBD. Maintainer API/scope acceptance, implementation and release remain open.

## Audited Gap and Motivation

Braid adopts published 1.17.0 at 15eacde48b47cc8e48af29ddc95949801a2e3e5d. Current main was audited at 24841b7ca33b6b9ad31313ef19cda12e7dea4e82; AuthoringSnapshot/AuthoringCapabilities match the pinned API after checkout newline differences. The snapshot is sealed/non-partial and exposes expression replacement, insertion target/proposal, source export/lookup; there is no public deletion target/proposal or deletion capability. Incidental collection Remove methods are not source-authoring APIs.

Users can insert and edit value-of but need to remove an existing instruction. Braid is a consumer: it must not generate a source splice, add a parser, reserialize XML, use reflection/friend internals or patch its pinned engine checkout.

## Requested First Slice

- Separately advertise supported deletion capability, bounds/profile and an opaque snapshot-owned target for one existing principal-module xsl:value-of. Target eligibility comes from the engine, not Braid's lexical label/visual role. Accept originally imported and previously inserted instructions where eligible.
- Initially use the existing bounded single-module/template/literal-result authoring profile where supported. Exact attribute/body/version/namespace/static-coverage and mixed/preserved-context rules remain Bosak-owned. No arbitrary node/subtree, dependency, bulk delete, reorder/move or other instruction is requested.
- Remove only the selected instruction's exact source extent. Preserve all other bytes: indentation/blank lines, comments, PI payloads, namespaces, ancestor/child/sibling markup, explicit parent tags, source URI, BOM/declaration, Unicode and newline convention. An empty parent must not be silently shortened to a self-closing tag. Whitespace/namespace cleanup is deferred; a harmless blank indentation line may remain.
- Produce a defensive isolated ordinary XSLT candidate, fresh inspection, exact original removal and empty replacement ranges, immutable ordered complete SourceEdits-like evidence, explicit removed descriptor identities and total injective survivor correspondence. Every original descriptor is either removed or mapped exactly once; include represented text/trivia/document/ancestor descriptors, not just elements. Removed identities cannot be reused as selected candidate nodes.
- Provide exact surviving-parent correspondence/selection evidence where provable. If selection evidence is absent or ambiguous, consumers clear it rather than infer a nearest sibling. Define the no-parent/empty-parent cases explicitly.
- Validate original/candidate within the declared restricted profile and preserve existing resource/encoding/cancellation/no-side-effect boundaries. Classified refusals return no candidate and retain input. Do not require output equivalence: deleting value-of intentionally changes the transformation result.
- Add APIs without breaking existing insertion/expression/default behavior. Keep engine implementations/tests/gates and release decisions within the Bosak workflow.

## Required Evidence

1. Happy paths: delete first, middle, last and sole eligible value-of from imported source and a prior inserted candidate. Repeated identical siblings must not confuse identity; self-closing/explicitly closed supported forms and ineligible body forms must be documented.
2. Exact fidelity: all surviving source spans match original bytes, source edits replay exactly, no undeclared trivia/namespace cleanup, candidate exported bytes are defensive and original bytes immutable.
3. Provenance: removed/surviving partition is complete/disjoint, survivors are injective, fresh candidate descriptors/parent mapping are exact and missing/duplicate/ambiguous/range evidence refuses.
4. Formats: compact/indented and previously readable source, LF/CRLF/lone CR, UTF-8 BOM/no-BOM and UTF-16 LE/BE with declarations/Unicode, alias/default/rebound namespaces, comments/PI and mixed/preserved-context policy.
5. Failures: stale/foreign targets, non-value-of/dependency/unsupported-profile input, invalid original/candidate, resource limits, cooperative cancellation and denied external access. Retain original snapshot and existing supported operations.
6. Consumer: Braid independently checks current identity/permission/cancellation/overlap, freezes complete review and accepts only an explicit session-issued deletion review. One Apply adds one history step; undo/redo restores exact bytes under new revisions and does not restore old inspection/validation/preview/export authority.
7. End-to-end: a hand-written changed-output ExactBytes golden is validated/executed, exported to a new file, independently reopened and consumed. Native candidate review/Apply, survivor selection, keyboard/theme/cancel/retry/history acceptance remains separate from automated proof.

Exact public type/member names, deletion scope, engine architecture and test/release decisions are intentionally not prescribed. An upstream implementation must be separately accepted, delivered and reviewed before Braid updates its pin or exposes Delete.

## Consumer References

Local sibling-checkout documents: [Braid deletion plan](../../Bosak.Braid/docs/VALUE_OF_DELETION.md), [proposed ADR-016](../../Bosak.Braid/docs/ADR-016-reviewed-deletion-and-survivor-selection.md), [REQ-122](../../Bosak.Braid/docs/FEATURE_REQUESTS.md#req-122-visual-xslt-transformation-designer), [existing reviewed acceptance](../../Bosak.Braid/docs/ADR-015-explicit-reviewed-insertion-acceptance.md). Braid source checkpoint d863993 has preserved uncommitted insertion/readable changes and 952 passing normal Release tests; that baseline is not deletion implementation evidence.
