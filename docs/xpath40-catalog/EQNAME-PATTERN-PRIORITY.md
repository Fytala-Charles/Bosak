<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala EQName pattern priority correction">
  <h1>EQName pattern priority</h1>
  <p>22 October 2026 — source 1.23.4, unpublished</p>
</div>

**22 October 2026 - EQName bracket parsing, source 1.23.5 (unpublished):** [Parser correction and evidence](EQNAME-BRACKET-PARSING.md) distinguish brackets inside namespace literals from real predicates in effective 3.0 and 4.0 patterns. Public release remains 1.23.4; REQ-126 remains In Progress, with D2/D3/D4 and AC-03 partial. Earlier limitation statements below are historical.

**Public1.23.4 — 22 October 2026:** Union-minimum and EQName URI-punctuation priority corrections are [published and verified](../RELEASE-1.23.4-PUBLICATION.md). Opening-bracket URI parsing remains separately limited. Earlier unpublished delivery records below are historical.

Source 1.23.4 ignores namespace URI punctuation when classifying an effective 4.0 name test's default priority. For `<root xmlns="urn:n|part"/>`, the pattern `Q{urn:n|part}root` now has priority 0, so an explicit priority 0.25 competitor wins. Source 1.23.3 and public 1.23.2 classified that name as a fallback pattern with priority 0.5. Stylesheets relying on this implicit priority can select a different template after upgrading; explicit priorities remain authoritative.

## Contract and implementation

The [pinned current XSLT draft](https://raw.githubusercontent.com/qt4cg/qtspecs/3230573afe246f1b502ed2e22d187ce9928155b1/specifications/xslt-40/src/xslt.xml), section `default-priority-for-xnode-patterns`, assigns priority 0 to an expanded-name node test, optionally preceded by a forward axis. Punctuation within the URI is part of the name, not a path, wildcard, predicate or union operator. Upstream `master` was refreshed on 22 October 2026 and remained at `3230573afe246f1b502ed2e22d187ce9928155b1`; freshly downloaded XML SHA-256 is `6d3038e21291abf761d883dd371cb9885972e0dd1c85b41844df70ca1ede45b8`. This is a current review pin, not recovered historical adoption.

`ComputeSinglePatternPriority` already builds a URI-masked string for effective 4.0 axis classification. The correction passes that same string to `IsQNameNodeTest`, rather than scanning the original URI punctuation. Only this classification line changes. Earlier profiles still receive the original text, preserving their existing behavior. Real predicates and paths, wildcard name tests, attribute normalization, explicit priorities and union/set aggregation are unchanged.

## Regression evidence

Twenty-six new cases in `Xslt40EqNamePriorityTests` cover URI pipes, slashes, parentheses, wildcards, axis-like colons and closing delimiters; root/local 3.0 and 4.0 combinations; attributes; actual predicates outside the URI; paths, kind tests and wildcards; and both union spellings plus grouped set composition. The existing `OperatorDelimiters` regression deliberately changes from `union` to `fallback`: its QName branch now has priority 0, giving the 4.0 union minimum priority 0 against a 0.25 competitor. This is the separate limitation recorded during the [union-minimum correction](UNION-MINIMUM-PRIORITY.md).

With the final test inputs and the old classification line, 17 of the 26 new cases fail and nine pass. With the fix, all 94 EQName/union/set-priority checks pass. Original source, the initial exploratory test file, all failed/intermediate receipts and the final scoped red/green TRXs are preserved under `.guard-tmp/req126-eqname-priority-evidence`. The previous union-priority receipts remain untouched.

Initial probes using `[` inside a namespace URI failed in pattern parsing, before useful priority dispatch: XPST0081 in effective 4.0, and XPST0003 in the earlier-profile attribute probe. Those parser failures are retained in the original receipts. Final dispatch regressions use supported URI punctuation and retain true/false predicates outside the URI. No support for opening brackets inside EQName URIs is claimed; parser support is a separate follow-up. Closing-bracket punctuation is covered independently.

Final validation: **10,470 solution tests pass**, including 7,502 XSLT tests, with no failures or skips. The 94 focused checks pass. QT3 remains **31,142/0/679** and qt4 gated remains **8,278/0/1,894**, without gate promotion or fixture changes. Receipt hashes are recorded in [the machine-readable audit](EQNAME-PATTERN-PRIORITY.json). The Release build succeeds with the pre-existing nullable warning in the conformance runner.

## Delivery boundaries

PR #405 merged as `9964e2399976dc4f831d550f94f233c75f9a64d1`; this slice starts from that revision. Source 1.23.4 is unpublished; public release remains 1.23.2. REQ-126 stays In Progress, with D2/D3/D4 and AC-03 partial. The manifest and worked reports are unchanged: 71 catalog entries, 2,743 feature checks plus 24 chapter checks = 2,767; four worked chapters and 117 assigned function names. README install/CLI instructions remain current. No public API, harness, upstream test, Braid pin or consumer acceptance change is included.

Next: review and merge the correction, then decide whether to publish the accumulated 1.23.3/1.23.4 fixes or first address the separately reproduced opening-bracket URI parser limitation. Any parser correction needs its own scope, regressions and both engine gates.
