# REQ-128: Source-Preserving Value-of Insertion for Bosak.Braid

**Parent:** REQ-122. **Date:** 2026-10-15.
**Status:** Specification and capability audit; runtime insertion is unavailable. Registered as Bosak REQ-128: Pending, target TBD, owner Unassigned. No maintainer acceptance or implementation is implied. Target version and implementation owner remain TBD.

**Design follow-up ? 2026-10-15:** The [proposed engine contract](REQ-128-INSERTION-CONTRACT.md) now resolves the first-slice eligibility, snapshot ownership, byte/encoding/namespace fidelity, explicit candidate identity, cancellation/resource boundary and planned fixtures. Public 1.7.1 still has no insertion API. This is design for review, not implementation acceptance; status/owner/target remain Pending/Unassigned/TBD.

## Capability Audit

The immutable Braid pin is `5bab4799783569d538cec9b03d37a57454f0eebc`. Its public `Bosak.Xslt.Authoring.AuthoringSnapshot` offers `ProposeExpressionEdit(AuthoringEditProposal)` for an existing attribute. `AuthoringCapabilities` advertises inspection and Expression replacement; Pattern/Avt/QName replacement are false, and no insertion capability or proposal exists. Braid's `SourceAuthoringCapabilities` and `IExpressionEditor` expose the corresponding existing-slot workflow only.

The same public authoring surface was inspected in local Bosak HEAD `e16851e1297758f9043a845134ad59ef293637fc`. No supported structural-insertion entry point was found in that namespace. This is a source audit, not a runtime/package adoption review; upstream has unrelated working changes. REQ-124's dossier explicitly leaves insert/delete/reorder/move capabilities for separate delivery. Its accepted status does not authorize implementing those operations in Braid.

## Bounded User Workflow

1. Inspect the current principal module and select a parent in the source graph.
2. Show **Add value-of** only when the engine advertises insertion and provides an eligible target for that exact snapshot. The first position is **End of selected sequence constructor**. It appends one instruction; sibling positioning is deferred.
3. Enter the expanded XPath expression as text. The engine owns contextual parsing, XML escaping, namespaces and the precise insertion boundary. No raw XML editor, arbitrary byte offset or Braid-generated fragment supplies the command.
4. Request an isolated candidate and review the affected source range, proposed text and any declared limitations. Candidate review does not change source, history or validation/preview/export evidence.
5. Apply explicitly. Application independently rechecks source session/revision, capability, target identity, AuthorTransformation permission and cancellation. Accept one candidate atomically as one draft-history entry; discard a cancelled/stale/denied proposal.
6. Refresh source, inspection and graph from accepted candidate evidence. Select the engine-identified new value-of node and its select Expression slot. Missing or ambiguous mapping clears selection; no old node ID or label/offset heuristic supplies identity.
7. Undo/redo follows the accepted-draft history rules, invalidating old evidence. Validation, preview and restricted export remain explicit separate commands. Original files are preserved.

## Engine Boundary and Initial Limits

Bosak must identify legal sequence-constructor targets; namespace/name matching in Braid is presentation only. Request support for principal xsl:template sequence constructors and ordinary literal-result element sequence constructors inside templates, subject to an explicit engine eligibility/refusal contract. The UI must never enable insertion merely because a descriptor has children or its label resembles an instruction.

The engine chooses a prefix already correctly bound to the XSLT namespace or declares a fresh local binding on the new instruction, without changing existing bindings. Inherited namespaces, default XPath namespace, version gates, base URI and variable scope are used for the expression. Comments, processing instructions, text and opaque surrounding constructs retain their original bytes. Engine-defined trivia ownership must make End unambiguous; refusal is preferable to relocating existing trivia or changing mixed-content semantics invisibly.

First slice: one principal module, one inserted value-of with one select expression, declared engine-supported encodings and existing eight-MiB Braid bound. Require UTF-8 and BOM UTF-16 LE/BE consumer evidence; additional encodings require declared support and tests. Self-closing parent expansion may be explicitly refused initially. No dependency edits, arbitrary XML fragments, move/delete/reorder, alternate insertion positions, AVT/Pattern authoring or unrestricted source editing. No compiler internal access, reflection, Braid parser, reserialization or engine-source patch is permitted.

The addition is proposed, not implemented: no insertion button, fake capability flag or new runtime port is shipped before supported upstream delivery and adoption. Bosak owns API shape, diagnostics, implementation, engine tests and release. Braid owns session/revision/entitlement orchestration, candidate review, UI, layout, history and consumer fidelity tests. Structural candidates may need a separate Application port/history description; exact contracts are designed after Bosak's accepted response rather than misusing expression-edit metadata.

## Acceptance Matrix

| ID | Required evidence |
|----|-------------------|
| SA-01 | Explicit capability and engine-owned target eligibility; unsupported/foreign/stale node, illegal parent, module/version/encoding and missing capability refuse without mutation |
| SA-02 | Append a value-of with contextual select into an eligible template/literal-result sequence; engine owns position, escaping, prefix and static context |
| SA-03 | Exact original bytes outside declared affected ranges; BOM/newlines/comments/PI/opaque/mixed content remain intact; candidate has fresh inspection, new-node identity and surviving-node correspondence |
| SA-04 | Invalid/incomplete expressions, cancellation, entitlement loss, source races and overlap retain accepted source/history; review alone never accepts a draft |
| SA-05 | Explicit Apply makes exactly one history entry; undo/redo restores exact bytes and invalidates historical inspection/validation/preview/export evidence |
| SA-06 | New node/slot/source highlight agree; missing/ambiguous identity evidence clears selection rather than guessing |
| SA-07 | Real engine validation/preview/export/reopen consumes accepted ordinary XSLT with a dedicated exact-output golden; invalid or excluded cases retain restricted-coverage disclosures |
| SA-08 | UTF-8/BOM UTF-16 LE/BE, compact/indented input, alias/default/rebound namespaces, static variable scope and illegal-parent/refusal cases; native focus/keyboard/theme review recorded separately |

## Delivery Gates

1. Register the consumer request in Bosak and await its acceptance/design/delivery.
2. Review the delivered public API, compatibility, source fidelity and engine test/release evidence before changing the Braid pin.
3. Implement Application capability/target/candidate/acceptance orchestration and failure/history tests.
4. Adapt the public API in Infrastructure and add real-engine consumer fidelity tests.
5. Add graph/UI candidate review and Apply controls, source/selection refresh and native acceptance procedure.

See source-owner architecture, graph editing context and restricted export.


Consumer specification: D:/Development/Bosak.Braid/docs/STRUCTURAL_AUTHORING.md (working document based on Braid d863993). Braid request: REQ-122. This dossier is self-contained for upstream review; the local consumer document is not yet committed. Compatibility: additive supported API/capability only; preserve REQ-124 expression editing and existing compiler/runtime APIs. Priority requested: P1. Bosak maintainers own acceptance, API design, engine implementation/tests, scheduling and release. No engine modification is authorized by this registration.

## Bosak Design Review — 2026-10-15

Review against local authoring source at a6867201 and refreshed main 5a9b3c4b (the subsequent changes on main are documentation/extension delivery changes). This is a design review only; REQ-128 remains Pending, target TBD and owner Unassigned.

- Use an additive structural proposal/result contract. Existing expression proposals require an existing owning attribute and cannot represent insertion, a new node or a new select slot truthfully.
- Issue eligible insertion targets from the snapshot, with an opaque snapshot binding. Existing integer node IDs alone cannot establish provenance: two snapshots may contain the same integer. Reject a target submitted to a different snapshot before resolving its node. Braid must additionally reject a candidate if the accepted session/revision changed while it was being prepared.
- Define End as a precise engine-owned byte boundary before the selected parent's closing tag, after its existing content, without moving or copying existing trailing whitespace/comments. Appending an instruction can change transformation output; candidate review and the dedicated output golden must show that intentional change. Refuse self-closing parents initially. Eligibility must account for instruction grammar, effective text expansion and static context, not just element names.
- Return candidate bytes, explicit old/new affected ranges (including a zero-length old insertion range), fresh inspection, surviving-node correspondence and an explicit new instruction/select-slot identity. Existing attribute changed-slot metadata is insufficient. Never infer correspondence from labels or an assumed traversal position in Braid.
- Establish the inserted expression's context at the actual append position, including preceding local variables, applicable namespace rules, effective version and base URI. Choose an existing nonempty XSLT prefix or a fresh local binding; test default and rebound namespace cases. Escape XML attribute text without changing XPath semantics, including attribute whitespace normalization. Handle effective expand-text explicitly so expression braces remain literal attribute content.
- Advertise insertion-specific encoding support; inspection's wider supported-encoding list does not prove insertion fidelity. Start with UTF-8 and BOM UTF-16 LE/BE and refuse unsupported encodings without mutation. Verify exact unchanged byte prefixes/suffixes rather than only comparing decoded XML.
- Keep proposal generation immutable and distinguish its successful candidate from Braid's explicit acceptance. Specify cancellation points and refusal categories before implementation; entitlement, accepted-session races and undo/redo remain consumer responsibilities.

Before accepting implementation, settle the eligibility/refusal table, snapshot-bound target and candidate API, cancellation contract, namespace/text-expansion handling and the SA-01–SA-08 fixture inventory. Preserve expression-edit compatibility. No public capability, source version, package pin or Braid UI is changed by this review.
