# Specialized lazy node filter candidate

Development 1.81.14, unpublished. Direct XDM sequences/enumerators replace the intermediate generated filter IEnumerable and adapter layers. Scalar/empty paths, deferred source acquisition and predicate/errors, Current/end behavior, source marker/refusal, identity/mutation visibility and existing non-disposal protocol are retained. No new public API or language behavior is proposed.

[Adoption evidence](ADOPTION.json) predates implementation and pins immutable dated XPath/XDM references and qualified protocol sources. Ten focused semantic/allocation regressions passed. Allocation budgets were fixed before implementation at 48/56/192/160/160 bytes per call for create/marked-create/once/replay/cross-first, above prototype observations and below the legacy baseline. No repeated minimum-selection or post-failure widening.

An initial test-file generation path mistake caused one invocation to match no new tests; its zero-test receipt is retained separately and not counted. The corrected focused invocation passed ten tests. Full solution: 26,453 passed; language server: 90 passed. Frozen QT3 is exactly 31,397/0/424 and gated qt4 8,941/0/1,918, retaining every skip name and reason. Same compatible allocation worker rejects the legacy 1.81.13 rows against all five pre-recorded budgets and accepts every candidate row. A separate identical semantic worker on baseline and candidate passes 139 differential scenarios plus 14 fixture checks in each run against the immutable proven prototype. Standalone qualification remains pending at this source checkpoint.

Actual same-worker allocations (bytes/call): create 200 -> 32; marked create 208 -> 40; construction plus first enumeration 256 -> 160; replay/cross-thread first 232 -> 128. No end-to-end speed claim. ADOPTION.json retains the qualified 1.81.13 input closure; its engineVersion field records the planned 1.81.14 candidate. Actual candidate hashes are recorded separately in controls and subsequent qualification.

[Build/test receipts](CHECKS.json) · [Allocation controls](CALIBRATION.json) · [Control review](CALIBRATION-REVIEW.json) · [Semantic controls](SEMANTIC-CONTROLS.json) · [Frozen receipts](FROZEN-CHECKS.json) · [Preparation correction](PREPARATION.json)

No push, publication or package release. Previous publication approvals do not cover this new candidate or evidence dossier.
