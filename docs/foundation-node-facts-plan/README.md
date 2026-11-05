# Original node facts: TA-10 implementation preparation

5 November 2026. Base **2b7fa1ab**, source remains **1.60.0**. The next Core slice is an internal, uncached reader for original node facts and their guarded typed value. [Audit](AUDIT.md), [implementation checklist](IMPLEMENTATION.md), [specification basis](SPEC-BASIS.md), [member matrix](MATRIX.json) and [22 acceptance cases](ACCEPTANCE-CASES.json) define the work.

The public contracts expose fact issuance, exact node/facts/input/origin association, governing type/declaration, nilled/no-typed-value flags and typed-value reads. The selected public adapter currently implements container issuance only. The older private association authenticates opaque node tuples but does not consume these fact fields. Neither path yet supplies a complete original-node fact reader. No new public signature requirement is demonstrated by this audit; this is not a freeze disposition.

Fresh existing controls pass **235/0** on exact unchanged candidate binaries: association 33, containers 94, adapter 35, public numeric 35, ordinary/refusal 38. These reaffirm current behavior, not the unimplemented node-fact cases. [Receipts](evidence/CONTROLS.json) and [immutable inputs](INPUTS.json) preserve the evidence. Verify with `python docs/foundation-node-facts-plan/tools/verify.py`.

The prior [TA-09 qualification](../foundation-public-typed-numeric/README.md) remains the 25,114-unit/frozen-gate baseline; full suites were not rerun for this source-unchanged preparation. R3 remains InProgress. Public node-query activation, native/owner performance qualification, construction/compatibility/all-host/migration, C45/freeze and publication remain pending.
