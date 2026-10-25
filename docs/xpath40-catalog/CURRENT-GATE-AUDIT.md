<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala frozen and current qt4tests gate comparison">
  <h1>Current upstream gate audit</h1>
  <p>23 October 2026</p>
</div>

**Integer division precision audit — 25 October 2026:** [Evidence and proposed scope](IDIV-PRECISION-AUDIT.md) add 48 result-range controls and 96 exact-rational comparisons, exposing 48 in-range one-unit discrepancies while documenting precision qualifications. Recommend exact truncation for existing decimal-backed operands with the current Int64 result bounds preserved. Engine/version unchanged; source 1.25.2 unpublished/public 1.25.1; REQ-126 partial.


**Integer modulo correction — 25 October 2026:** [Behavior and evidence](INTEGER-MODULO.md) return integer zero for Int64 minimum modulo -1 and preserve integer identity for decimal-backed remainders. Adds 84 direct/runtime regression cases (45 failed before, all pass after). Genuine decimal/floating policies and `idiv` remain unchanged. Source 1.25.2 unpublished/public 1.25.1; REQ-126 stays partial.


**Division/modulo audit — 25 October 2026:** [Evidence and next correction](DIVISION-MODULO-AUDIT.md) add 72 supported controls and 84 observed diagnostics. Minimum/-1 modulo overflow and wide remainder type loss are the recommended next bounded correction; quotient narrowing and precision remain separate. Engine/version unchanged: source 1.25.2 unpublished, public 1.25.1. REQ-126 remains partial.


**Integer unary correction — 25 October 2026:** [Behavior and evidence](INTEGER-UNARY.md) fix Int64-minimum negation wraparound and retain decimal-backed integer identity in folded/runtime paths. Adds 48 cross-profile regressions (24 failed before, all pass after). Source 1.25.2 is unpublished; public 1.25.1 and Braid smoke/adoption remain separate. Division/modulo and wider storage work remain open; REQ-126 stays partial.


**Unsigned floating truncation — 24 October 2026:** [Correction and limits](UNSIGNED-FLOATING-TRUNCATION.md) truncate finite float/double operands before unsigned range checks, allowing(-1,0) to become unsigned zero. Adds60 constructor/cast/reference/castable cases; exact overflow thresholds and nonfinite errors remain intact. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 partial.



**Unsigned Decimal truncation — 24 October 2026:** [Correction and limits](UNSIGNED-DECIMAL-TRUNCATION.md) discard numeric fractions before unsignedLong range facets/backing selection, with48 constructor/cast/reference/castable cases. Fractional lexical forms still refuse; floating near-zero consistency remains separate. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 partial.



**Exact floating integer boundaries — 24 October 2026:** [Correction and limits](FLOATING-INTEGER-BOUNDARIES.md) reject the signed2⁶³/unsigned2⁶⁴ overflow thresholds and preserve accepted large unsigned magnitudes, with72 constructor/cast/reference/castable cases. Unsigned Decimal truncation and generic storage remain separate. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 partial.



**Unix-time wide integer calendar correction — 24 October 2026:** [Behavior and limits](UNIX-DATETIME-WIDE-RANGE.md) report FODT0001 for valid decimal-backed integer epochs beyond the supported calendar, preserving frozen negative refusal and Experimental signed behavior. Adds34 source cases; current native Unix improves to22/2/0. Generic constructors/casts remain separate. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 partial.



**Arithmetic correction gate follow-up — 24 October 2026:** [Fresh receipts](INTEGER-PROMOTION.json) record frozen qt4 **8,436/0/1,894**, replacing two prior compile-overflow skips with passes accepted through runtime FOAR0002. QT3 remains31,142/0/679;224 gates, fixture and retry policy unchanged. Current Unix20/4/0 remains; earlier comparison counts below are historical.



**Experimental signed Unix epochs — 24 October 2026:** [Implementation and limits](UNIX-DATETIME-EXPERIMENTAL.md) record the opt-in correction, 27 new source tests, 11,407 passing tests and unchanged frozen gates. Source 1.25.1 is unpublished; public/Braid 1.25.0 remains current. REQ-126 stays partial.



**Unix timestamp boundary review — 24 October 2026:** [Contract and evidence](UNIX-DATETIME-CONTRACT.md) add52 source regression cases, UTC/default/call-path/calendar/2038 deadline checks and a fresh complete native-set comparison: frozen16/0/0; current20/4/0, exit2. Current negative-epoch semantics and wide-integer errors remain explicit adoption boundaries; a minimum-literal representation diagnostic is retained. All11,296 solution +84 language-server tests pass. Catalog remains76 entries /3,172 documentation checks; no engine/API/version/gate/fixture/release change. Source/public1.25.0 and Braid integration remain separate; REQ-126/D2/D3/D4/AC-03 stay partial.

**Frozen map-to-element gate restored - 24 October 2026:** [Repair and process controls](MAP-TO-ELEMENT-GATE-REPAIR.md) correct the dormant name to `fn-map-to-element`. All224 configured sets execute; expanded frozen gate8,434/0/1,896 includes156 additional passes/two skips. QT3 remains31,142/0/679. A synthetic negative assertion now exits2 instead of exploratory exit0. No engine/API/version/fixture-pin/release change; current upstream remains unadopted. REQ-126 stays partial; REQ-136 remains accepted. Earlier gate counts and dormant-name observations are historical.

## Result and boundary

The current upstream suite is not ready to replace Bosak's frozen qt4tests pin. The unchanged engine passes its frozen gate; the same configured gate names against a complete current upstream checkout produce **45 failures across eight sets**. This is a diagnostic comparison, not adoption or promotion of the current suite. [Machine inventory and failures](CURRENT-GATE-AUDIT.json), [original execution receipts](evidence/current-gate-audit/frozen-gate.txt).

The comparison pins frozen [`68910080`](https://github.com/qt4cg/qt4tests/tree/68910080d2d4d4092a1c6a908998ceec64ffc6d4) and freshly fetched current [`3a2640cf`](https://github.com/qt4cg/qt4tests/tree/3a2640cf649a9bff6c7a5b257a1fe7e50450b2c2). Current advanced beyond the `b3a26509` snapshot in [the CSV index audit](CSV-INDEX-CONTRACT.md); the earlier receipt remains unchanged. Both complete checkouts retain their own catalog and fixtures. No native fixture was rewritten.

| Existing gate comparison | Passed | Failed | Skipped | Exit |
|---|---:|---:|---:|---:|
| Frozen | 8,278 | 0 | 1,894 | 0 |
| Current | 8,297 | 45 | 1,894 | 2 |

The two [skip receipts](evidence/current-gate-audit/frozen-skips.txt) are byte-identical: 1,892 unsupported-dependency skips and two existing overflow-error skips. Their original final blank separator is retained; the receipt-specific whitespace attributes preserve that output format. Both runs include 11 successes reached through the runner's experimental compatibility fallback. Neither skipping nor fallback is evidence of complete frozen-profile conformance. The [current log](evidence/current-gate-audit/current-gate.txt) preserves exit-2 failures. Test totals grow by 64, from 10,172 to 10,236; the higher pass count does not make the new baseline green.

## Configured names versus executed sets

The runner contains **224 configured gate names**, not the catalog inventory's previous 218. The documentation extractor omitted six dotted names: `prod-AxisStep.abbr`, `prod-AxisStep.unabbr`, and `prod-GeneralComp.ge`, `.le`, `.lt`, `.ne`. The extractor now includes dots; generated reconciliation counts are corrected without modifying runtime membership. Prior receipts are historical and are retained.

Only **223 configured names resolve and execute** in either catalog. The configured name `map-to-element` is absent; the actual catalog name is `fn-map-to-element`. Therefore the existing gate does not enforce that function's tests. The six dotted names do resolve and execute. All 223 resolvable configured sets were checked in both lanes: 215 have zero current failures and eight fail. Zero failures can coexist with dependency skips and is not a complete feature certification.

The correctly named set was run separately, without gate-only filtering:

| Supplemental `fn-map-to-element` | Passed | Failed | Skipped | Exit |
|---|---:|---:|---:|---:|
| Frozen | 156 | 0 | 2 | 0 |
| Current | 173 | 53 | 7 | 0 |

These [frozen](evidence/current-gate-audit/frozen-map-to-element.txt) and [current](evidence/current-gate-audit/current-map-to-element.txt) runs are exploratory: exit 0 is the existing runner's behavior for a non-gated set even when 53 cases fail. Frozen skips comprise one unsupported dependency and one XML-name error; current skips comprise five unsupported dependencies, the XML-name error and a namespace-binding error. The 53 failures are **additional to**, and excluded from, the 45 gated failures. No silent gate rename, addition, removal, or skip was applied.

## Current failure clusters

| Configured set | Failures | Observed boundary requiring review |
|---|---:|---|
| `fn-boolean` | 2 | Effective boolean value of JNode expressions |
| `fn-data` | 1 | A new assertion uses unregistered `type-of#1` |
| `fn-element-to-map` | 26 | New `escape-names` option and corresponding errors |
| `fn-index-of-substring` | 1 | Empty pattern now expects positions `1, 2, 3, 4` |
| `fn-jkey` | 3 | JNode axes/kind tests and type metadata |
| `fn-jvalue` | 1 | JNode `item` axis |
| `fn-parse-csv` | 7 | Non-positive selection and `get` positions; see the resolved upstream CSV contract |
| `fn-unix-dateTime` | 4 | Negative epochs and integer range/error behavior |

The machine audit identifies all 45 cases, their actual messages, current expressions/assertions and earlier versions where present. Of these failures, 37 are added cases and eight have changed execution nodes. Fourteen of the 223 test-set files differ in Git; there are 64 added cases, zero removed, and 15 cases with changed explicit test/result/dependency/environment nodes. The inventory excludes metadata-only edits from that last count and does not infer inherited-context equivalence. A failure identifies an observed implementation/harness boundary; each engine correction still requires its own pinned specification review. The supplemental map-to-element failures cover option support, key handling and layout/error behavior and need a separate review.

## Reproduce and next slices

The runner was built at PR420 head `75ca7542506a92265b13c456a96891423555ae75`; engine and test source are identical to audit base `d014365a9782811311fcca7f07130535b051d492`. Runner hash, gate names, per-set counts, native blob revisions, execution dates and watchdog configuration are recorded in the machine audit. Existing exact-head CI supplies 11,133 solution plus84 language-server passes; this documentation/inventory change has separate CI.

```powershell
$env:BOSAK_QT4_GATE_ONLY = '1'
dotnet run --project tests/Bosak.XPath.Conformance40 -c Release -- PATH_TO_PINNED_COMPLETE_SUITE
# Supplemental set: exit 0 can still contain exploratory failures.
$env:BOSAK_QT4_GATE_ONLY = '0'
dotnet run --project tests/Bosak.XPath.Conformance40 -c Release -- PATH_TO_PINNED_COMPLETE_SUITE fn-map-to-element
```

1. Repair the dormant gate name in a dedicated slice against the **existing frozen suite**. Frozen supplemental 156/0/2 is the promotion evidence; rerun the resulting complete gate and record its increased coverage explicitly. This audit leaves the membership unchanged.
2. Review bounded current-contract slices, including CSV selection and `get` together, then the independent option/JNode/string/time clusters. Keep the frozen/current evidence distinct. Decide baseline/compatibility adoption before implementing a change that contradicts frozen assertions.
3. Re-run the complete proposed gate before any upstream gitlink update. The current suite's other test sets were not exhaustively evaluated here; a comparison of existing gates is not whole-suite certification.

No engine, API, C# test, version, frozen gitlink, gate membership, release or Braid change. Published Bosak remains1.24.0; REQ-136 remains consumer-accepted; REQ-126/D2/D3/D4/AC-03 remain partial. No upstream issue, contribution or message was sent. Independent processor confirmation is not claimed.
