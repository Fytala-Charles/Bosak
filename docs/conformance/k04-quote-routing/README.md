# K04-C01: quote-aware routing review — 27 October 2026

**Correction verified in isolation; normal adoption awaits owner approval.** All 16 previously identified quote-routing cases pass their original assertions in a copied harness with only the raw quote refusal removed. The complete rehearsal reports **31,161/0/660**. Normal QT3 remains **31,145/0/676** until approval and a fresh adopted normal run.

[The concrete proposal](BASELINE-UPDATE-PROPOSAL.md) and [dry patch](PROPOSED-ADOPTION.patch) cover the exact routing change and AGENTS baseline. The complete skip delta removes only those 16 cases and preserves all 660 retained names and reasons. The 11 unsupported-prolog/pragma boundary cases remain unchanged. Thirteen lexical controls cover doubled quotes, direct constructors, comments/nesting, CDATA, PI payloads and preserved exclusions.

Fresh diagnostics: 32 candidate and 22 boundary process runs plus two lexical-control processes, each externally bounded to 20 seconds; one complete 31,821-case rehearsal under a 300-second watchdog. Zero timeouts. The real assertion comparer and static output-option handling are retained; no assertion or fixture is rewritten. The XQuery parser still rejects invalid grammar.

[RESULTS.json](RESULTS.json) consolidates all results. [Evidence manifest](evidence/manifest.json) pins copied-source, fixture/binary identities, receipts and prior evidence. The isolated generated-file marker affects comments only. Current tracked engine/harness/AGENTS source is unchanged.

```powershell
python docs/conformance/k04-quote-routing/build-review.py
python docs/conformance/k04-quote-routing/evidence/validate-evidence.py
git apply --cached --check docs/conformance/k04-quote-routing/PROPOSED-ADOPTION.patch
```

After owner approval, apply the reviewed routing/baseline change and verify the actual normal entry point at the new exact totals and skip delta. Other K04 prolog, polarity, unknown-capability and schema-profile work remains open. Bosak.Schema owns XSD 1.1; REQ-126 remains Complete for its accepted bounded catalog.

Source base `88960f3b`; candidate **1.26.11 unpublished**, public **1.26.3**. The unchanged engine's 22,007 solution passes and qt4 **8,437/0/1,893**, 224 gated sets/11 existing Experimental passes are inherited. No new unit/qt4 run, release, tag, push, external merge, upstream or Braid action is claimed.
