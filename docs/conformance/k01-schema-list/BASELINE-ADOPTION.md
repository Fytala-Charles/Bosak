# K01: approved QT3 baseline adoption

**27 October 2026 — owner-approved exact two-case update.**

The owner approved [the proposal as presented at 3f0aed6b](baseline-adoption/proposal-as-presented.md). [The exact question and answer](baseline-adoption/owner-approval.json) authorize removing only `CastAs-ListType-20` and `CastAs-ListType-28` from the QT3 known-skip dictionary, updating AGENTS.md, and rerunning the complete gate.

The full QT3 run now reports **31,144 passed/0 failed/677 skipped**, retaining all 31,821 cases. The complete skipped-name/reason inventory matches the prior 679 cases after removing exactly the two approved names. No other skip admission, dependency filter, XQuery routing, fixture, assertion, timeout or retry policy changes. Both previously skipped cases now participate through the normal runner.

The qt4 gate is unchanged at **8,436/0/1,894**, 224 sets and 11 existing Experimental passes, inherited from the unchanged K01 correction checkpoint. Engine/library sources and the 18 regressions are unchanged from 3f0aed6b; its 21,816 full-solution passes remain valid inherited evidence. This follow-up rebuilds the QT3 harness and freshly runs that complete manual gate; it does not claim another unit-suite or qt4 run.

K01 is complete for this bounded list-binding correction and two-case admission. Broader schema/list capabilities remain unclaimed. Original frozen 31,142/0/679 logs, failed/passing regressions, proposal and pending-decision checkpoint remain immutable history. The [initial triage](../../xpath40-catalog/CONFORMANCE-SKIP-TRIAGE.json) remains its 26 October snapshot; it is not overwritten to erase the original skips. [Current machine-readable adoption](BASELINE-ADOPTION.json) records the new baseline and exact skip delta.

Source candidate **1.26.4** remains unpublished; public 1.26.3 and REQ-126 accepted catalog remain unchanged. No tag, push, publication, Braid or upstream action occurs. Next recommendation: K02's separately bounded schema duration resolution review, before the larger multi-affiliation substitution work.

Validate: `python docs/conformance/k01-schema-list/baseline-adoption/validate-evidence.py`. The adoption manifest protects the approval, fresh gate/dump, source identities and original K01 evidence. Rendering is HTML with unchanged Fytala CSS, not screenshot/PDF acceptance.
