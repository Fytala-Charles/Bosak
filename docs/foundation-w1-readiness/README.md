# W1 ordinary-source admission implementation readiness

1 November 2026. This is the finite pre-implementation plan for W1 from the [R2 Core ledger](../foundation-r2-ledger/README.md) and [Core O4 return](../foundation-r2-core-returns/returns/O4-Core.json). **No production source or public API changes occur here.** R2 is still InReview; W1 execution remains subject to the corresponding accepted scope and provider facts. [Acceptance cases](ACCEPTANCE.json) are all NotRun.

## The exact implementation gap

The current private SchemaQuerySession always constructs SchemaEvidenceKernel. That constructor requires an ISchemaEvidenceIssuer, and Open requires a non-null assessment accepted by that issuer. This is appropriate for authenticated schema evidence, but cannot serve as ordinary XmlDb source admission by substituting a dummy issuer/assessment. Existing ordinary public XPath/XQuery/XSLT and IXdmNode usage remains available outside this private path; this is a gap in the future opt-in public session implementation.

The reviewed public shape already permits absent assessor/origin and supplies input membership, actual lifetime admission and source/snapshot bindings. W1 implements that selected distinction. It does not add a new public resource manager, require Schema activation, add disposal to IXdmNode or copy a persistent document to solve ownership.

## Selected implementation approach

Use **neutral source admission composed with optional schema evidence**. Avoid deriving ordinary XML from a schema issuer or treating a source label as assessment authority.

| Component | Responsibility | Explicit exclusion |
|---|---|---|
| Host | Acquire an actual input resource; retain and release its owner according to transfer/rollback rules | Engine does not acquire stores, schema files or workers implicitly |
| Neutral admission state | Exact session/environment and source/snapshot input binding, availability, ownership predicate, admitted resource/operation leases and close state | No fabricated schema assessment, component identity or typed provenance |
| Optional schema association | Bound advisor, original assessment/compatibility proof and issuer-verified node/value facts when supplied | No schema callbacks on the no-assessor route; descriptive names are not authority |
| Operation | Immutable bounded same-session membership, raw callback ownership, guarded input/result use and complete failure preservation | No automatic recursive graph detachment or blanket provider thread safety |
| Thin public facade | Accepted public declarations and documented supported/refused profiles; delegate to Runtime | No concrete Schema/XmlDb dependency below their host adapters |

Private implementation type names and internal field layout remain implementation choices. W1 must use the accepted production contracts, not reference the isolated .C45Proposal assemblies. Reuse SchemaOperationLeaseSet's acquired-prefix rollback and raw completion rules. Any extraction of existing schema snapshot lifetime code must preserve its evidence/cache/type-origin behavior under the full regression suite; do not duplicate it silently into a second divergent lifetime implementation.

## Admission and getter ordering

1. Validate supplied arguments, bounds, cancellation and exact engine environment. Reject a schema origin without a usable selected advisor; never ignore the origin or mint a replacement. Final public error/result mapping must match the accepted profile.
2. Enter the already-acquired owner's lifetime and check availability before reading its root. Validate source/snapshot descriptors as bindings, not authority. A throwing Enter must leave no hidden unreturned lease.
3. Establish the neutral admission. On rejection, release only acquired admission obligations; the host still owns resources whose release responsibility never transferred. Preserve the original failure plus cleanup failures.
4. Bind optional original assessment only through the selected advisor/compatibility checks. With no assessor/origin, no schema issuance callback runs and no typed authority is attached.
5. Before reading a supplied node's contents, establish exactly one current operation source membership. Only then consult optional original schema facts and authenticate their node/input/origin association. Recheck availability after trusted owner callbacks and before dispatch.
6. Capture supported dynamic data and freeze membership for the raw callback/task. Keep the selected closed-resource profile; resources discovered during evaluation cannot silently join the operation.
7. Engine close revokes further admission/guarded use. Actual resource release follows raw ownership, including a canceled caller wait. Attempt every release once in the selected reverse order, preserving all failures.

These rules do not make arbitrary raw IXdmNode getters revocable. The public profile must distinguish guarded borrowed use from escaped raw graphs, copied scalar values and native owner guarantees.

## Finite implementation slices after gate closure

| Slice | Deliverable | Evidence before concluding |
|---|---|---|
| W1-A Contract boundary | Implement the accepted neutral input/lifetime pieces and optional schema association; no mandatory legacy node/sequence member | Updated public inventory and no-provider/schema-channel call-count controls |
| W1-B Admission lifecycle | Neutral admission/close/raw leases, actual owner transfer and all failure paths | Getter spies, partial admission, cancellation, reverse cleanup and nested/concurrent-close tests under the selected provider scope |
| W1-C Host wiring | Ordinary session compilation and evaluation for XPath/XQuery/XSLT, including separate match/global roles | Source-free/single/multiple ordinary sources, foreign/ambiguous nodes and actual captured-context refusals |
| W1-D Actual owner integration | XmlDb snapshots and representative supported Schema input without changing product pins/storage | Actual reader/generation/assessment facts, not synthetic issuer substitution; owner-approved immutable fixture/source pins |
| W1-E Candidate qualification | Full source build, appropriate new happy/edge tests, exact engine gates, ABI/member inventory and ordinary-path performance | All unit projects pass; QT3 31,397/0/424, gated qt4 8,941/0/1,918 and retained skip maps; representative budgets remain owner obligations |

Before any semantic implementation, record exact immutable adopted specification revisions, sections and intentional deviations. This planning packet adopts no new atomization, numeric or schema semantics. In particular, XmlDb's typed-value policy remains the separate FXG-01 disposition; W1 must not silently retag or reinterpret its existing values.

## Owner-dependent decisions that remain explicit

- O1-XmlDb/O1-Schema must validate actual input membership and authentic fact association without eager graph copies. No private reflection shortcut is selected.
- O2-Schema must identify any required observable advisor context and typed/compatibility contract gaps before freeze.
- O4-XmlDb must qualify source-level cleanup masking, reader/generation retention and the exact cancellation/reentry/concurrency profile. Core's host fixture does not certify those provider paths.
- O3-Braid must resolve any required broader invocation or worker/currentness signature. No process route is selected by W1.

The five external role returns remain pending. The latest bounded local documentation search found no revised-draft return; its [search scope and pins](INPUTS.json) do not cover remote issues/messages. No external file or message changed.

## Readiness and stopping condition

The Core plan, dependency ordering and 20 acceptance cases are prepared. Implementation has not started and no acceptance case is marked passed. **The next release-critical action is to obtain and disposition the five external returns, then open only the corresponding accepted W1 slices.** Further Core planning artifacts do not substitute for that action.

Source remains unchanged 1.42.0. Prior 24,390 unit passes and frozen conformance totals are inherited evidence; no engine suite was rerun for this planning-only step. Run `python -B docs/foundation-w1-readiness/tools/verify.py` to check source/plan pins and the unexecuted acceptance matrix.
