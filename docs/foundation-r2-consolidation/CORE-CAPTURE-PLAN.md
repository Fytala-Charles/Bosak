# BCAP-01 — next Core execution plan

Accountable role: Core; consumer review: Braid. Current state: open required capability/route decision, not an accepted implementation or additional passing test.

## Route decision and smallest candidate

Use the same compiled stylesheet, captured semantic environment and operation membership as the schema executable. First demonstrate a supported existing composition if one exists. The [source audit](../foundation-r2-response-review/README.md) has not established such a public route: existing CaptureTransform supplies capture semantics, the private explicit-context bridge supplies separate source roles, and the proposed schema Transform returns a raw graph.

If composition is insufficient, compile a minimal operation-bound capture entry on the schema executable. Reuse the existing XsltCaptureResult, capture engine/session and supported XsltCaptureOptions controls where valid. Decide how to supply the actual immutable per-run resource receipts: the existing capture result does not contain them. A small result envelope or another supported receipt handoff is a candidate to review, not a selected new public type in this plan. Do not add a second unrelated capture framework.

The candidate must specify:

- Operation/executable identity and live membership for match, global and dynamic input nodes, including shared and empty sets.
- Exactly one dynamic-context source and explicit precedence/refusal rules for duplicate template/mode/base-output settings. No silent fallback to an unqualified legacy executable.
- Copied/refused mutable options and context services; the captured resource policy cannot be replaced with a different authority by caller options.
- Explicit production output/message limits, cancellation and time/operation-gate composition. Raw-result exclusions must be honest; capture defaults are not assumed bounded.
- Detached managed principal/secondary bytes, effective output properties, ordered messages and classified Success/Partial/Failed/Cancelled outcomes, including completed outputs before an abort.
- Actual per-run receipt ownership/currentness, isolation between runs and the component responsible for credential redaction before publication/IPC.
- Raw lifetime through evaluation **and serialization**; cancellation of a wait is not completion. Capture faults and all cleanup failures must remain visible without converting a partial/cancelled run to success.

## Evidence-producing sequence

| Step | Deliverable | Gate |
|---|---|---|
| P02 | Supported route proof or isolated revised declarations, exact delta and option/result semantics | R2 design work; no production public adoption |
| P03 | Targeted executable feasibility cohort C01–C10 with immutable engine/source pins | Must exercise real capture machinery, not only throwing declarations or synthetic result objects |
| P04 | Braid disposition of the actual route/candidate and bounded preview profile | Prior Revise stays open until explicitly resolved |
| P08 | Reconcile every impacted owner return and accepted implementation scope | Do not silently reuse an old proposal-manifest hash |

A runtime candidate that changes engine behavior records immutable specification adoption evidence first and receives appropriate full engine/conformance qualification before promotion. Declaration compilation alone proves neither capture semantics nor native resource ownership. Use the existing guarded child runners; no deliberate unhandled crash or machine-wide error setting is needed.

## Completion criteria

BCAP-01 closes only when Braid's required capture data/limits/receipts are represented by a supported operation-bound route, targeted controls pass on that route, and the exact review disposition is recorded. No full typed/persistent worker integration, R4 performance acceptance, C45/freeze or publication is implied by closing this route gap.
