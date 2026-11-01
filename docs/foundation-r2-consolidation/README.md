# Consolidated R2 dispositions and execution plan

Updated 1 November 2026. This is the current consolidated view of all supplied role returns and Schema's alignment guidance. [STATUS.json](STATUS.json) and [PLAN.json](PLAN.json) are the machine-readable status and dependency queue. Earlier dossiers remain historical receipts; this page supersedes their “still waiting on XmlDb” and five-/four-missing-return counts.

**Five of seven role returns are submitted: three scoped design accepts and two revisions. Only Schema's two formal returns are missing.** R2 remains InReview; production remains unchanged 1.42.0. No runtime/C45/freeze or publication acceptance is inferred.


Current Core work: [P03 gate/serializer controls](../foundation-bcap-controls/README.md) exercise the isolated capture path. Next resolve capture plus cleanup outcome orchestration and the original serializer exception requirement; native/full qualification and owner review remain. Production and release acceptance are unchanged.

## Consolidated answers

| Return | Decision/state | What it establishes | What remains |
|---|---|---|---|
| O1-Schema | Missing; detailed checklist guidance received | The [Schema comment](https://github.com/Fytala-Charles/Bosak/issues/468#issuecomment-6563123351) specifies the required association/issuance/copy evidence | Actual source-specific answers, supported/refused profile, decision, gap list and evidence |
| [O1-XmlDb](inputs/O1-XmlDb.json) | **AcceptWithScope** | Neutral ordinary-source design fits; no additional mandatory Core IXdmNode member demonstrated | Implement/test a provider-local exact snapshot membership bridge; no runtime adapter is claimed |
| O2-Schema | Missing; detailed checklist guidance received | Checklist covers numeric/value-shape/N1–N8/compatibility disposition | Formal answers and any precise required signature/context gaps, beyond the decimal backend |
| [O2-Core](inputs/O2-Core.json) | **AcceptWithScope** | Numeric/typed/authority/compatibility design direction selected by Core | Exact numeric execution and actual provider semantics remain unimplemented/unqualified |
| [O3-Braid](inputs/O3-Braid.json) | **Revise** | Existing session/lifetime concepts appear sufficient; current invocation needs do not demonstrate arbitrary initial sequences or streaming | **BCAP-01:** demonstrate or add the complete operation-bound bounded capture route |
| [O4-XmlDb](inputs/O4-XmlDb.json) | **Revise** | No additional required Core lifetime signature demonstrated; finite sequential target identified | **XDB-LIFE-01:** actual cleanup failure preservation, release uncertainty and supported concurrency profile |
| [O4-Core](inputs/O4-Core.json) | **AcceptWithScope** | Host acquisition and raw operation ownership; complete failure preservation; no universal node/sequence disposal API | Actual owner behavior and corresponding implementation scope must be qualified |

All five JSON returns match the exact R2 proposal manifest and pass its format checker. These are explicitly agent-authored source/design reviews, not authenticated human signatures or actual consumer integration. Schema's comment explicitly says it is a proposed work/acceptance checklist, not completed returns. It cannot be substituted for O1-Schema/O2-Schema.

The newer XmlDb review is pinned to **`b09c7341e7961f90f2c320fd9a180367b35e0035`**, not the older `08085954` audit snapshot. Its evidence directory is **`D:/Development/Bosal.XmlDb/docs/foundation-requirements/r2-owner-review`**. Seven selected source/test/project files match both the owner's SHA-256 records and that commit's content; both delivered response hashes also match. The checkout contains unrelated owner work, which was neither modified nor qualified by this audit. [Input receipts](INPUTS.json) preserve this distinction.

## Recommended next steps

1. **Core now: resolve BCAP-01.** Prepare the smallest capture route/declaration candidate, then execute targeted capture feasibility controls. This is named R2 gap work, not blanket R3 production promotion. [Core capture plan](CORE-CAPTURE-PLAN.md).
2. **XmlDb in parallel: resolve XDB-LIFE-01 and prepare membership.** Preserve actual primary/cleanup errors and fault state, qualify release uncertainty and the finite profile, and implement the provider-local membership bridge. This does not require inventing a Core disposal API. [Owner actions](OWNER-ACTIONS.md).
3. **Schema in parallel: complete O1/O2 formal returns.** Use the eight specific work items distilled from the new checklist, naming actual methods/proofs or explicit unsupported/signature gaps. Full XSD 1.1 backend completion is not required to represent all known foundation needs. [Owner actions](OWNER-ACTIONS.md).
4. **Reconcile the revised draft and close R2.** Obtain Braid's review of the capture solution, XmlDb's updated O4 disposition and Schema's two formal returns. Any changed proposal manifest requires explicit confirmation/rebinding of affected owner dispositions; old hash-bound acceptance is not silently carried over.
5. **Open accepted R3 slices, then R4–R7.** Implement the finite W1–W6 queue under accepted scope, qualify actual providers/source/unchanged binaries and PF-01–09 budgets on one immutable candidate, obtain four-role C45/freeze, prepare release packages and publish only through the release gates.

Core does **not** need to wait idle for XmlDb before doing the capture review work. Conversely, the three scoped design accepts do not waive Braid's capture gap, XmlDb's unresolved failure contract or Schema's required answers. Avoid another generic lifecycle experiment or planning-only checkpoint; the next Core deliverable should be the concrete capture candidate and its evidence.

## Complemented plan and acceptance

The [ten-task dependency queue](PLAN.json) separates immediate Core work, parallel owner work and gated production/publication. [Eighteen targeted follow-up controls](FOLLOWUP-CASES.json) supplement the existing W1 matrix: C01–C10 cover complete capture and X01–X08 cover actual XmlDb resource behavior. All are **NotRun** and overlap existing concerns; do not add them to historical pass totals.

Important additions include preserving partial/secondary outputs and classified failures, per-run receipt ownership, option/context precedence, production byte/message bounds, actual failure injection and the sequential handoff to XSLT's dedicated worker thread. “One snapshot per evaluation thread” must be clarified so a claimed profile neither permits concurrent cache access nor accidentally rules out the engine's supported sequential thread handoff.

The current capture defaults can be unbounded; selected limits must be explicit. Braid's proposed experiment ceilings are not Core defaults, total-memory bounds, agreed performance budgets or proof of compilation/worker time limits. Native resource retention, cancellation latency and serialized data movement require actual owner workloads.

## Evidence and limits

[INPUTS.json](INPUTS.json) pins byte-identical copies of all five returns, the exact Schema comment and paginated issue comments, source evidence and XmlDb's owner receipt. No original incoming response, external request, source checkout, product pin or store was changed. No outgoing message was sent.

This step changes documentation and review tooling only. Production source and engine bytes remain unchanged; the preceding 24,390 unit passes and exact conformance gates remain inherited receipts, not fresh executions here. Target release version/date remain unselected. Read-only verification: `python -B docs/foundation-r2-consolidation/tools/verify.py`.
