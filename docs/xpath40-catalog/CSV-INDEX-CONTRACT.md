<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala CSV index contract and upstream revision audit">
  <h1>CSV index contract: upstream resolution and adoption boundary</h1>
  <p>23 October 2026</p>
</div>

**Frozen map-to-element gate restored - 24 October 2026:** [Repair and process controls](MAP-TO-ELEMENT-GATE-REPAIR.md) correct the dormant name to `fn-map-to-element`. All224 configured sets execute; expanded frozen gate8,434/0/1,896 includes156 additional passes/two skips. QT3 remains31,142/0/679. A synthetic negative assertion now exits2 instead of exploratory exit0. No engine/API/version/fixture-pin/release change; current upstream remains unadopted. REQ-126 stays partial; REQ-136 remains accepted. Earlier gate counts and dormant-name observations are historical.

**Current upstream gate audit - 23 October 2026:** [Audit](CURRENT-GATE-AUDIT.md) corrects the documentation inventory:224 configured names,223 executed sets,one dormant `map-to-element` name. Frozen8,278/0/1,894 remains unchanged; current8,297/45/1,894 fails across8 sets. Supplemental `fn-map-to-element`: frozen156/0/2,current173/53/7; current exit0 is exploratory, not green. Documentation extraction now includes six dotted names; no engine, runtime gate, fixture pin or release change. REQ-126 remains partial; REQ-136 remains accepted. Next: repair the dormant gate against frozen fixtures before upstream adoption. Earlier218-count statements are historical.

## Finding

The CSV index discrepancy identified in [the original contract review](CSV-CONTRACT-REVIEW.md) is resolved upstream. It is a difference between Bosak's frozen qt4tests revision and the current upstream contract, rather than an outstanding request for upstream clarification. No duplicate upstream issue or test correction is needed.

[Specification issue 2849](https://github.com/qt4cg/qtspecs/issues/2849) discusses replacing restricted integer subtypes with `xs:integer`. It closed on 20 October 2026. [Upstream test commit `1d149118`](https://github.com/qt4cg/qt4tests/commit/1d149118d85df69aa7bee1b2b3ee67cdb3624533), committed on 19 October, updates seven CSV expectations. Its per-test modification dates say 2 September; these are distinct from the Git commit and issue closure dates.

The freshly checked defaults are qtspecs [`b4723ff5`](https://github.com/qt4cg/qtspecs/tree/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c) and qt4tests [`b3a26509`](https://github.com/qt4cg/qt4tests/tree/b3a26509c9d9a7fe535ec3d9b8b78d2560de5009). Bosak still pins qt4tests `68910080d2d4d4092a1c6a908998ceec64ffc6d4`. The specification comparison revision is unchanged from PR420; its historical adoption provenance remains unassigned.

## Complete affected boundary

The pinned [function catalog](https://github.com/qt4cg/qtspecs/blob/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xpath-functions-40/src/function-catalog.xml) defines `select-columns` as `xs:integer*`. A non-positive index contributes an empty field at its selected position. The returned `parsed-csv-structure-record` has `get` signature `(xs:integer, (xs:integer | xs:string)) => xs:string`; a numeric row or column position outside the available range returns a zero-length string. Unknown string column names retain their separate FOCV0004 rule.

| Tests | Frozen expectations / current Bosak | Current upstream expectations |
|---|---|---|
| `parse-csv-912`, `parse-csv-913` | Negative /zero column selection raises XPTY0004 | The joined result is `["d","c","","s","r",""]` |
| `parse-csv-914`, `parse-csv-915` | Non-positive `get` row raises FORG0001 (the frozen assertion also accepts XPTY0004) | Zero-length string |
| `parse-csv-916`, `parse-csv-917` | Non-positive numeric `get` column raises FORG0001 (the frozen assertion also accepts XPTY0004) | Zero-length string |
| `parse-csv-919` | Another negative `get` row raises FORG0001 (the frozen assertion also accepts XPTY0004) | Zero-length string |

[`Csv.SelectColumnsOption`](../../src/Bosak.XPath.Standard/Functions/Csv.cs) enforces positive indexes; the same file's returned `get` function explicitly rejects non-positive rows and columns. The selection pipeline is shared by `parse-csv`, `csv-to-xml` and `csv-doc`. The native reproduction below executes `parse-csv`; it does not establish complete three-function coverage, coercion support, or conformance for every record field.

## Reproduction and retained evidence

The unchanged runner built from PR420 head `75ca7542506a92265b13c456a96891423555ae75` executes both native test-set revisions. That head and merged main `3641002312ef28299913ed8efb66f14d7c75834a` have identical trees. The runner and loaded Bosak assembly hashes are recorded in [the machine audit](CSV-INDEX-CONTRACT.json).

| Native `fn-parse-csv` revision | Passed | Failed | Skipped | Exit |
|---|---:|---:|---:|---:|
| Frozen `68910080` | 124 | 0 | 0 | 0 |
| Current `b3a26509` | 117 | 7 | 0 | 2 |

Both runs retain their original [frozen output](evidence/csv-index-contract/frozen-native.txt) and [current output](evidence/csv-index-contract/current-native.txt). All seven current failures are the tests listed above; the other 117 pass. Exit 2 is the runner's actual gated-failure result for this separate diagnostic snapshot. It has not been reclassified as a passing gate or hidden by skips. Native catalog and test-set inputs are copied unchanged; only the requested test-set is executed. Other catalog files are not present in these diagnostic directories.

To reproduce, obtain `catalog.xml` and `fn/parse-csv.xml` at each linked immutable revision in separate directories, retaining their relative paths, then run:

```powershell
dotnet run --project tests/Bosak.XPath.Conformance40 -c Release -- PATH_TO_SNAPSHOT fn-parse-csv
```

The audit records SHA-256 and Git blob IDs of the current source files. Base64 Git blob bytes were verified against Git SHA-1 and byte counts. An initial `gh api` raw-output specification snapshot sanitized escape sequences elsewhere in the catalog; that diagnostic snapshot remains retained separately and was superseded by verified blob bytes before deriving the contract. The verified specification bytes match PR420's original snapshot exactly. Original execution and submission receipts remain intact.

## Recommended adoption sequence

1. Audit the existing 218 gated sets against a complete, separately pinned current upstream checkout, and inventory changes from `68910080`. Preserve the frozen gate and its receipts during the comparison. The seven CSV changes are not evidence that other upstream changes are already compatible.
2. Prepare a bounded CSV alignment slice covering selection and `get` together. Add direct happy-path and boundary checks for negative/zero positions, empty/ragged input, positive positions, headers and all three shared CSV entry points. Keep unknown option-key policy and broader coercion changes separate.
3. Make any new baseline adoption explicit and review its complete gate delta. Do not silently replace frozen assertions, remove gated sets, add skips, or report the original frozen receipt as green after a contract change. If maintaining the old frozen contract alongside the current draft requires a new compatibility policy, decide that policy before implementing it; no new profile is introduced here.

This investigation changes no engine, public API, version, fixture gitlink, assertion, gate membership or release. Bosak remains published 1.24.0; REQ-136 remains consumer-accepted. REQ-126/D2/D3/D4/AC-03 remain partial. The QName extension-option difference in the original review is independent and remains open. No independent processor confirmation is claimed.
