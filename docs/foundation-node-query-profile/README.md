# Public original-node nilled profile: TA-12 selection

5 November 2026. Base **6523c649**, source remains **1.61.1**. The selected first public original-node slice is `nilled($node)` / `fn:nilled($node)` under a supported explicit assessor binding. Reserved behavior profile: `bosak-original-node-nilled-v1`; descriptor: `bosak-original-node-nilled-descriptor-v1`. These names are selected for implementation, not activated or owner-accepted.

[Selected profile](PROFILE.md) · [Machine selection](PROFILE.json) · [Implementation sequence](IMPLEMENTATION.md) · [24 new cases, all NotRun](ACCEPTANCE-CASES.json) · [Specification basis](SPEC-BASIS.md)

A metadata-only reader is the immediate prerequisite: TA-10's existing complete reader eagerly reads typed values, which a nilled-only query must avoid. The metadata-only state will explicitly say NotRequested rather than masquerading as absent, Unsupported or Supported. No new public signature gap is demonstrated; the existing session operation and annotated-node/advisor contracts appear sufficient.

Fresh unchanged-binary controls pass **349/0** (Runtime 311, API 38): existing facts, containers, association, public identity/numeric, advisor and ordinary/refusal controls. [Receipts](evidence/CONTROLS.json) verify exact existing 1.61.1 engine loads. Production source/version/API metadata are unchanged. Prior **25,193 unit tests** and frozen gates remain historical same-binary qualification; no fresh full run or new nilled support is claimed. Verify with `python docs/foundation-node-query-profile/tools/verify.py`.

Next implement/qualify the internal metadata-only facts mode, then wire the public advisor and exact new profile with NP01-NP24. Atomization follows as a separate source-provenance/sequence-output policy and implementation slice. R3 remains InProgress; public node execution, native/owner budgets, all-host/migration, construction/compatibility, C45/freeze and publication remain pending.
