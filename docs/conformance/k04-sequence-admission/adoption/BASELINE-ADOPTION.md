# K04-C12 approved unsupported-sequence adoption

27 October 2026. Owner disposition: **Approve exact one-case adoption (Recommended)** for proposal commit **e924a708**,decision **K04-sequence-adoption-v1**.

The exact approved three-file patch is applied:both dependency filters recognize the reviewed unsupported U+FBF4 sequence with positive refusal and negative admission,and AGENTS.md now mandates **31,162/0/659** for QT3. Filter histories0.17/0.7 record the adoption. No other source change is included.

Fresh complete normal gates pass **QT3 31,162/0/659** and **qt4 8,587/0/1,892**. Each skip set removes exactly format-integer-042;all659/1,892 retained names and reasons match the approved rehearsal and prior baseline. Both original assertions pass through normal single-case runs. The288 production filter controls exactly match approved prototype outcomes. No case-name bypass,fixture,assertion,unknown-capability policy or unrelated guard changes.

The complete solution build succeeds. All **22,198 solution tests pass** across10projects;see [validation.json](validation.json). The226GatedSets and11Experimental passes remain. The normal gate totals,original-case receipts and [skip verification](skip-verification.json) are fresh adoption evidence,distinct from the isolated rehearsal.

The original [proposal](../BASELINE-UPDATE-PROPOSAL.md),RESULTS.json and evidence remain preserved as historical checkpoint data,including pending-approval fields. This separate adoption record supersedes their status;the old validator is a pre-adoption checkpoint validator and is not the current-state gate. Current adoption validation checks preserved prior bytes and reconstructs the exact approved source patch against e924a708.

Candidate **1.28.0** remains unpublished;public **1.26.3**. Engine semantics,version,fixtures/assertions,gate membership and schema ownership are unchanged. Bosak.Schema retains staged XSD1.1 ownership;REQ-126 remains Complete for its accepted bounded catalog. No release,push,tag,Braid change or upstream submission occurs.

Next recommended work:review the remaining negative capability dependency kinds before broadening their policy;retain unsupported-prolog/XQuery and schema guards unless independently demonstrated.
