# REQ-136: Complete literal result element lifecycle for Bosak.Braid

23 October 2026. Requester: Bosak.Braid. Status: **Pending maintainer scope/API review. Owner: Unassigned. Target version: TBD.** Successful isolated 1.23.8 smoke triggered this owner-authorized documentation-only registration. No implementation or release is approved by this document.

## Outcome and capability gap

Complete Create, Rename and Delete as one coordinated feature, with one upstream implementation PR and one later Braid integration PR. Existing REQ-132 element creation is reused, not redesigned. A user should create an output element, populate its attributes/text/value-of children, rename the element, then explicitly delete the element and its contents if desired.

Published 1.23.8 at `6d12bed35dac56a635acc7702645ef9085103c83` exposes `GetLiteralElementInsertionTarget` and `ProposeLiteralElementInsertion`. Its public AuthoringSnapshot/AuthoringCapabilities have no literal-element rename or deletion operation. Expression, text and attribute operations do not grant that authority. Braid remains adopted on 1.23.7; a successful isolated smoke is not dependency adoption or native acceptance.

Bosak owns syntax, name legality, literal-result classification, exact source changes and descriptor correspondence. Braid owns session/revision/license gates, complete review, atomic Apply, history and UI navigation. No Braid XML splice, engine-internal access or reflection bypass is proposed.

## Requested bounded profile

- One engine-approved, ordinary no-namespace literal result element in a dependency-free principal module, within explicitly captured authoring/static/resource coverage. Support previously imported as well as engine-created elements, paired and self-closing syntax. Declare supported XSLT versions/hosts, encodings and refusal boundaries.
- Rename changes only the local name to a valid unqualified XML NCName, with exact case and Unicode spelling. Preserve the existing empty namespace, namespace declarations/context, attributes, children, comments/PIs, whitespace, quote choices and paired/self-closing form. No namespace/prefix changes, wrapping, unwrapping, relocation, instruction renaming or attribute renaming. Bosak owns name validity and expanded-name classification; Braid does not infer literal-result identity from a prefix.
- Delete means **remove the entire selected element and its owned subtree**, including its attributes and all contained instructions, literal text, comments/PIs and descendants. It does not unwrap or keep children. Empty-only deletion would not complete the requested populated-element lifecycle; if unsafe cases must be deferred, disclose them and obtain consumer agreement rather than silently advertising complete deletion.
- Nonempty deletion requires an engine-proven supported subtree and complete review evidence. Unknown/opaque or unsupported instruction/static/resource contexts must refuse rather than rely on UI confirmation as proof. Existing whole-module restricted coverage remains necessary; removal must not be used to repair an invalid original module or bypass resource denial.
- No adjacent indentation cleanup or unrelated pretty printing in rename/delete. Preserve every byte outside declared source edits. The existing creation operation retains its own accepted formatting contract.

## Operation and source contracts

Use additive, independently discoverable rename/delete capabilities and opaque snapshot-owned targets. Caller node IDs, ranges, names or equal-byte foreign snapshots cannot authorize mutation. Exact API names and error precedence are maintainer design decisions.

Rename must update both actual name tokens for paired syntax, or the one opening name for self-closing syntax. Return every exact original/replacement edit with original and candidate ownership, strict encoding coordinates and an unambiguous replay convention. Multiple name edits belong to one candidate and one eventual history step. No unreviewed whole-element normalization is permitted. Renaming to the same exact logical NCName returns explicit NoChange, without a changed candidate, Apply authority or history/redo invalidation.

Delete returns the exact full element/subtree removal extent and candidate surviving-parent identity. Outside bytes, including siblings and parent delimiters, remain untouched. Explicitly disclose every removed node/attribute and the complete removed source. If deleting the boundary creates an adjacent text-descriptor merge, either refuse with a documented reason or provide a bounded, exact source-backed merge partition; never hide it in incomplete correspondence.

For both operations, preserve the original snapshot, produce a fresh candidate inspection/ports only on Changed, and expose complete source/engine/module provenance. Physically bound original bytes, candidate bytes, traversal, evidence and encoded names independently with checked arithmetic. Reject invalid Unicode, unsupported encodings and malformed original/candidate source. Preserve UTF-8 with/without BOM and UTF-16 LE/BE within declared support.

## Actual identities and immutable evidence

Rename requires complete injective correspondence of all actual graph descriptors and per-owner attributes. Only the addressed element's lexical/local name and associated exact ranges may change; all descendant/attribute identities, logical values, kinds, order, parent relationships and namespace identities survive. New selection identifies the actual renamed candidate element.

Delete partitions original descriptors into the selected removed subtree and ordinary survivors, plus any separately declared bounded adjacent-text merge. Every actual original descriptor belongs to exactly one partition; every actual candidate descriptor has exactly the permitted surviving or merged provenance. Parent, siblings and all unaffected attributes require complete correspondence with strict translated extents. No synthetic IDs, unexplained extra/missing nodes, foreign descriptors, duplicated mappings or reuse of attribute/text-payload partitions as element authority. Selection identifies the actual surviving parent and clears removed slots/targets.

Collections, targets, edits, partitions and candidate ports must be immutable or defensively copied. Concurrent proposals must not mutate their input or each other. Discovery/proposal performs no transformation or ambient file/network/extension acquisition. Observe cancellation through bounded work and immediately before publishing success. Return typed refusals for unsupported capability/target/profile/resource/encoding/name, stale/foreign authority, bounds, original/candidate validation or correspondence failure; unexpected engine faults remain distinguishable from ordinary user refusals.

## Braid review, Apply and history

Show operation intent, original/candidate complete element evidence, old/new logical name for Rename, and full removed subtree plus surviving parent for Delete. Nonempty deletion requires explicit disclosure that children and instructions are removed; no implicit unwrap mode. Oversized complete review refuses instead of truncating applicable evidence.

Changing operation, target, name, source or policy invalidates held review. Separate Propose and Apply; no source/history change during proposal. Apply rechecks exact owner/source/inspection/port/session/revision/license, complete frozen evidence, cancellation and competing operations at the atomic source-owner boundary. Each Changed operation creates exactly one byte-exact history step, even if output bytes happen to remain equal. NoChange creates none. Undo/Redo re-inspects and restores actual element/parent navigation; old validation/preview/export evidence remains historical. Precommit refusal/cancellation retains source; successful committed late cancellation reports success.

## Planned acceptance groups

These are requirements, not executed rename/delete tests.

| Group | Required evidence |
|---|---|
| LE01 | Additive capability/opaque target discovery; unavailable, foreign and equal-byte other-owner authority refuses; legacy operations unchanged |
| LE02 | Imported and engine-created paired/self-closing no-namespace elements, namespace aliases/rebinding and default-namespace resets; unsupported namespaced/instruction/dependency targets refuse |
| LE03 | Unicode/case-sensitive NCNames; empty, colon, invalid Unicode and illegal names refuse; namespace and prefix changes excluded |
| LE04 | Rename paired opening/closing names coherently and self-closing names with exact bounded edit replay; untouched bytes preserved |
| LE05 | Equal-name NoChange preserves source, history, redo and current evidence |
| LE06 | Rename preserves attributes, literal values/AVTs as supported, children, namespace metadata, comments/PIs, lexical form and complete identities |
| LE07 | Delete empty paired/self-closing elements with exact survivor navigation |
| LE08 | Delete supported populated/nested elements as an explicit whole-subtree operation; all removed content is disclosed; siblings remain intact |
| LE09 | Adjacent text merge boundaries proven by complete partitions or explicitly refused; no unexplained descriptor loss |
| LE10 | Complete graph/attribute correspondence, strict original/candidate ranges and owned diagnostics; missing/duplicate/foreign/oversized evidence refuses |
| LE11 | Supported UTF-8/BOM/UTF-16 variants, Unicode/newlines/entities and physical boundary/overflow limits |
| LE12 | Restricted static/resource coverage, invalid original/candidate, unsupported subtree and no ambient acquisition |
| LE13 | Cancellation, concurrent proposal isolation and immutable results/ports |
| LE14 | Explicit review/Apply, stale source/policy/operation/input invalidation and complete-review size refusal |
| LE15 | One-step Changed history, NoChange behavior, Undo/Redo, surviving selection and historical-evidence staleness |
| LE16 | Full Create -> populate -> Rename -> Delete -> Create lifecycle, reusing existing creation and text/value-of/attribute operations |
| LE17 | Every changed stage: real validation, worker preview against ExactBytes goldens, restricted export receipt/file hash and independent reopen/inspect/validate/preview; include unchanged-output source changes |
| LE18 | Signed public-package compatibility/provenance and focused native review of new rename/nonempty-delete interactions; automate encoding/export/hash matrices |

## Delivery and ownership

Request one coordinated upstream design review and one implementation PR for Rename/Delete with existing Create compatibility, not staggered feature acceptance. Bosak maintainers choose API details, implementation, engine tests, scheduling and release. Braid reviews the design and exact implementation delivery, adopts a separately published release, then integrates both new operations together. No target version or acceptance is invented.

Related delivered Braid decisions: [ADR-018 creation](https://github.com/Fytala-Charles/Bosak.Braid/blob/391c16a5e8243e9b3817e5ac5a02b73f42752a11/docs/ADR-018-bounded-literal-element-insertion.md), [literal attribute lifecycle](https://github.com/Fytala-Charles/Bosak.Braid/blob/391c16a5e8243e9b3817e5ac5a02b73f42752a11/docs/LITERAL_ATTRIBUTES.md). Prepared local consumer requirement: `D:/Development/Bosak.Braid/docs/LITERAL_ELEMENT_LIFECYCLE_REQUEST.md`; proposed ADR: `D:/Development/Bosak.Braid/docs/ADR-023-literal-result-element-lifecycle.md`. These new local documents are not yet committed/published; this upstream request is self-contained.

## Smoke prerequisite and ownership

Braid base 391c16a on adopted Bosak 1.23.7 was tested in an isolated checkout against published 1.23.8/6d12bed3: 1,405 Release tests, 710 fresh public consumer checks (700 prior authoring checks plus 10 retained release probes), and 36 function-mode public-package regressions pass. Nine consumed signatures/assembly/archive/cache/source identities and publish licensing hygiene verify. The 36 regressions are imported from the exact release; coverage overlaps source tests and is not independent conformance certification. Braid has not adopted 1.23.8 or claimed new native acceptance. Evidence remains in its `.acceptance/bosak-1.23.8-smoke` directory. No engine code, public API, version, release or implementation approval is changed by this registration.
