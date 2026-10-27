# K02: approved guarded-runner adoption — 27 October 2026

**Complete. The normal QT3 baseline is now 31,145 passed / 0 failed / 676 skipped.**

The owner explicitly approved decision `K02-guarded-runner-adoption-v1`, presented at commit `628f169e`. [The exact question and answer](baseline-adoption/owner-approval.json), [proposal snapshot](baseline-adoption/proposal-as-presented.md), and [approved patch snapshot](baseline-adoption/patch-as-presented.patch) preserve that disposition. The approval covers normal-runner guarded initialization, the exact AGENTS baseline update and a fresh complete normal gate. It does not approve publication.

The normal `ConformanceRunner` now selects the existing `UseQueryOwnedSchemasForDiagnostics` switch when initializing its executor. Its historical internal name is unchanged; standalone `TestExecutor` callers remain default-off. Every structural and XSD-profile guard is unchanged. No known-skip entry was removed: `validateexpr-28` previously became a harness skip during eager environment preparation and now reaches the original assertion's allowed XQST0012 alternative.

The real normal Program entry point, without reflection or diagnostic flag injection, freshly completes all **31,821 cases** at **31,145/0/676** under an external 300-second watchdog. Build passes with zero warnings/errors. The [exact skip delta](baseline-adoption/skip-delta.json) removes only `validateexpr-28`, adds no skips, and preserves all 676 retained names/reasons. The result also matches the approved full-runner rehearsal exactly.

The accepted scope is bounded schema ownership and this single assertion admission. It does not establish successful duration-derived schema validation or XSD 1.1 capability. **Bosak.Schema REQ-009 owns staged XSD 1.1 implementation**, including separately governed future public seams. The explicit-profile exclusions, six substitution-group preparation exceptions, broader inventory exclusions and non-file/wrapped stream identity contract remain outside this adoption.

Engine sources and package candidate **1.26.8** are unchanged. K02-C02's **21,842 solution passes** and qt4 **8,436/0/1,894**, 224 gated sets and 11 existing Experimental passes are inherited; this follow-up claims only the fresh QT3 harness build/full normal gate. Public version remains **1.26.3**. REQ-126 remains Complete for its owner-accepted bounded catalog. No tag, release, push, upstream or Braid action occurs.

[BASELINE-ADOPTION.json](BASELINE-ADOPTION.json) is the current machine-readable adoption record. Earlier review `RESULTS.json`, pending-decision receipts and historical 31,144/0/677 logs remain immutable checkpoint evidence. The [adoption manifest](baseline-adoption/manifest.json) protects approval, source identities, full gate, exact skip delta and all prior artifacts.

```powershell
python docs/conformance/k02-admission-review/build-adoption.py
python docs/conformance/k02-admission-review/baseline-adoption/validate-evidence.py
```

K02's bounded core correction and admission work is complete. Next, review the remaining core-only skip clusters separately from Bosak.Schema's XSD 1.1 work; any new baseline change needs its own evidence and disposition.
