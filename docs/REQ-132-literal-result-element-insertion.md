# REQ-132 - Bounded Literal Result Element Insertion for Bosak.Braid

Submitted: 2026-10-18. Requester: Bosak.Braid. Status: Pending. Owner: Unassigned. Target Version: TBD. Documentation-only consumer request; no engine implementation or release is authorized. Current upstream main a9e954d7 was checked for identifier collisions before registration.

## User Outcome and First Profile

Append one empty literal result element to an engine-approved principal template or literal-result sequence constructor. Example: inside an existing result element, add an `item` child, then populate that child through the existing value-of/text commands. This adds output structure to the currently accepted output-value and literal-text authoring operations.

The first requested profile accepts one engine-validated unprefixed local name and creates an element in **no namespace**, with explicit paired tags and an empty body. Attributes, namespace-qualified output names, arbitrary XML snippets, wrapping/moving existing content, insertion at arbitrary positions, replacement, deletion and automatic namespace cleanup are excluded. Bosak may conservatively refuse additional contexts; unsupported profiles must have a classified refusal.

No-namespace means expanded name `{}` plus the requested local name, even beneath an inherited default namespace. Bosak must either generate a local namespace reset without altering surrounding meaning or explicitly refuse that context. It must not silently inherit the default namespace. Instruction classification must use expanded identity: an otherwise legal no-namespace local name such as `value-of` remains literal output. Raw markup, colon-bearing names, invalid XML names and reserved namespace spellings require engine validation/refusal, never a Braid naming parser.

## Capability and Authority Audit

Pinned published Bosak 1.19.0 at `27691fdb3708bb4ffd7c984abbf3196b7cde5667` exposes expression replacement, value-of insertion/deletion and text insertion only. Previously audited upstream main `f82495dd6ff7d9ef95f72b3e0c179d1205b376cb` exposes the same authoring capability set; no public literal-result-element target/proposal was found. Pattern/Avt/QName replacement remains unavailable. Runtime support for existing literal elements does not imply creation authority.

Bosak owns target eligibility, XML name legality, namespace/static context, literal-result semantics, syntax generation, layout and actual inspection descriptors. Braid owns typed consumer adapters, current-session/license/revision gates, bounded complete review, explicit source-owner Apply, selection, exact history and consumer fidelity tests. No engine edits, raw source splices or consumer XML parser are authorized by this design.

## Requested Public Contract

- Additive, separately discoverable literal-element insertion capability and snapshot-owned opaque target; optional readable formatting advertised independently. Exact API names and delivery version are Bosak decisions.
- Target discovery accepts the existing module/node identity and asks the engine for legal append placement. Principal-only, bounded single-module/resource profile should align with accepted structural operations where possible. Include/import/module refusal remains explicit.
- Proposal accepts the target, local-name intent and explicit PreserveSource or Readable mode, cancellation and declared limits. Engine output must establish the actual inserted literal element, exact expanded identity, paired markup, empty body/caret and the exact accepted name. No attribute/XPath/AVT editing authority is implied.
- Return immutable original/candidate bytes and inspection, complete ordered nonoverlapping source edits with exact old/new ranges, inserted instruction range, body caret, exact parent and total injective old-descriptor correspondence. Account for every new descriptor: inserted element and any explicitly edited layout whitespace or namespace metadata. No synthetic child or cross-snapshot numeric-ID guessing.
- Preserve every byte outside disclosed spans, including comments, PIs, namespace declarations, XML declaration, BOM and newline spelling. PreserveSource performs only the disclosed append edit; Readable permits separately disclosed layout-whitespace changes under a declared eligibility policy, never hidden significant-text normalization or cleanup.
- Candidate inspection must expose the fresh actual target/ports where supported, so value-of/text can populate the inserted paired element. Correspondence must preserve all pre-existing logical text and parent/order/name/attribute identities outside the declared footprint.
- Classify invalid name, unsupported context/mode/encoding, stale or foreign target, namespace hazard, resource limit and cancellation. Refusal never changes the original snapshot. Existing expression/value-of/text APIs retain behavior and independent capabilities.

## Consumer Review and Selection

Future outline/graph commands **Check element target** and **Add element** remain absent until reviewed public delivery. A proposal dialog will accept a local name, display the exact resulting expanded name, whole inserted element, all changed spans and source/revision/engine provenance. It will require a separate Apply and invalidate review on any input/mode/current-source change. Readable refusal never silently falls back.

After atomic licensed Apply, rebuild inspection, select the actual new element and highlight its exact empty-body caret. Graph reopen must restore the same current identity. One-step undo/redo restores exact bytes and fresh outline under new revisions; old validation/preview/export evidence stays historical. A separate subsequent child insertion forms its own history step. No Expression slot is fabricated for the new container.

## Acceptance Matrix

| Gate | Required evidence |
|------|-------------------|
| LE-01 Target | Engine-approved principal template/LRE append; unsupported, dependency, stale and foreign targets refuse |
| LE-02 Name/context | Legal and invalid Unicode XML names, colon/raw markup/reserved names, default namespace reset-or-refusal, XSLT namespace/default alias hazards |
| LE-03 Empty structure | One actual no-namespace paired empty element, exact expanded name/range/caret and no invented child |
| LE-04 Fidelity | UTF-8 BOM/no-BOM, UTF-16 LE/BE, LF/CRLF/lone CR, comments/PIs, aliases and unchanged gaps; complete ordered edit replay |
| LE-05 Mapping | Total injective old correspondence, complete candidate descriptor partition, preserved logical text/parent/order; malformed evidence rejected |
| LE-06 Modes | PreserveSource and independently supported Readable; mixed/preserved content refusal, complete layout disclosure and explicit retry choice |
| LE-07 Session | Deep freeze, exact owner/revision, independent permissions, overlap, cancellation before acceptance and success after committed late cancellation |
| LE-08 UI/history | Separate proposal/review/Apply, bounded complete evidence, cancel/deferred close, exact new-node/body selection, graph handoff/reopen, one-step undo/redo |
| LE-09 Composition | Add element then text/value-of to that current element; repeat siblings; individual undo/redo; fresh engine targets rather than inferred identity |
| LE-10 Golden | Original baseline differs from inserted-element output; populated child output also differs; real worker validation/preview ExactBytes, export receipt/hash and independent reopen/validate/run across four encodings |
| LE-11 Refusals | Invalid names, resource/capability/policy loss, stale inspection, invalid evidence and cancelled operations preserve original bytes/history |
| LE-12 Native | Protected kit/manifests, explicit user walkthrough, immediate export receipt/file hash recording, independent reopened result and reported observations |

## Delivery Sequence

1. Consumer request registered with explicit authorization as REQ-132; maintainer acceptance and public API/profile decisions remain pending.
2. Independently review implementation and source/public-package evidence; adopt only a reviewed published release at its exact immutable revision.
3. Implement separate typed adapter and frozen review/Apply/history checks with happy-path and malformed/refusal tests.
4. Add bounded outline/graph UI and current element/body selection, then real-engine worker/export/reopen goldens and protected native kit.
5. Record only observed native functional acceptance, then formally notify Bosak when requested.

## Consumer References

- D:/Development/Bosak.Braid/docs/LITERAL_ELEMENT_INSERTION.md: consumer scope and twelve acceptance groups.
- D:/Development/Bosak.Braid/docs/ADR-018-bounded-literal-element-insertion.md: proposed architectural decision.
- D:/Development/Bosak.Braid/docs/FEATURE_REQUESTS.md: Braid REQ-122.

Braid checkpoint4b41e77 is pushed. The consumer design is an uncommitted follow-up. The upstream API, scope acceptance, owner assignment, tests, delivery version and release decisions remain Bosak-maintainer responsibilities.
