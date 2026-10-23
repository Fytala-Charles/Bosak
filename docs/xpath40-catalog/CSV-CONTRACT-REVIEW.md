<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala CSV options, defaults and local-resource contracts">
  <h1>CSV options, defaults and local-resource contracts</h1>
  <p>23 October 2026</p>
</div>

**CSV index revision audit - 23 October 2026:** [Contract resolution](CSV-INDEX-CONTRACT.md) confirms upstream issue2849 already changed seven CSV assertions. Unchanged Bosak 1.24.0 passes124 frozen cases; a separate current-upstream diagnostic passes117 and fails7 (exit2), including selection and `get` indexes. No frozen fixture/assertion/gate or engine change. Next: compare all218 gated sets against a complete current snapshot before adopting the new CSV contract. REQ-126 remains partial; REQ-136 remains consumer-accepted. Earlier requests for upstream clarification are historical.

**PR420 refresh - 23 October 2026:** Reconciled against published, consumer-accepted Bosak 1.24.0 at main `06a76549bfed8200c7bb2580275b46bb46975d31`. The CSV fixtures, assertions and original evidence below are unchanged. The 1.23.8 preparation receipts remain historical; refreshed full-suite results are recorded separately in the machine audit. No new specification comparison or independent processor confirmation is claimed. REQ-126 remains partial.


Refreshed verification: **11,133 solution +84 language-server tests pass**, with zero failures/skips, including all 99 new catalog and 25 resource cases. Release build succeeds with the existing CS8604 warning at conformance Program.cs:1868. Temporary fixture junctions are removed. Original fixtures/assertions, prior TRX receipts and the original preparation audit remain unchanged; `refreshedValidation` records new TRX hashes. No engine, runner, gate, kit, README install or release change.

## Scope and sources

This bounded REQ-126 D2 review compares published Bosak **1.23.8** (package source `6d12bed35dac56a635acc7702645ef9085103c83`), reviewed repository base `2aa77d0c906edb790d686aeb989771f6031152da`, with the immutable [F&O function catalog](https://github.com/qt4cg/qtspecs/blob/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xpath-functions-40/src/function-catalog.xml) and [shared option conventions](https://github.com/qt4cg/qtspecs/blob/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xpath-functions-40/src/xpath-functions.xml). The current comparison pin `b4723ff52fd9d4fabf2b4cd33bd44f21939c512c` does not recover the missing historical adoption revision. The engine delivery remains PR104 at `e0024f5d`; the catalog's historical adoption gap stays explicit.

The slice adds **99 catalog checks** (76 focused and23 supplier-report runs) and **25 local-resource API tests**. Existing3,049 manifest rows and their serialized text remain intact; current totals:76 entries, **3,148 feature +24 chapter =3,172 documentation checks**, four chapters and117 assigned names. REQ-126, D2/D3/D4 and AC-03 remain partial. No engine/API/version/harness/gate/release change; Braid's published1.23.8 smoke remains independent. [Machine evidence](CSV-CONTRACT-REVIEW.json), [complete fixtures and reports](examples/csv-contracts/README.md).

## Signatures and defaults

`parse-csv($value as xs:string?, $options as map(*)? := {})` and `csv-to-xml` use `value`/`options` keyword names; `csv-doc` uses `source`/`options`. Omitted options, `()` and `map {}` yield the same tested defaults. An absent first argument yields `()`; a present zero-length string or empty file yields a present record/document with no rows. A newline is one blank row, not an empty field. Known keyword forms are executable in4.0; the catalog runner verifies the existing3.1 refusal for successful4.0 expressions. Broader coercion and every invalid keyword spelling are outside this slice.

| Option | Pinned default | Bounded evidence |
|---|---|---|
| separator | comma | Explicit semicolon; multi-character/colliding delimiters refuse FOCV0002 |
| quote-character | double quote | Explicit single quote, escaped quotes and quoted newline; malformed quotes refuse FOCV0001 |
| comment-marker | absent | Raw initial marker drops comment rows; omitted marker remains data |
| trim-whitespace | false | Unquoted padding retained by default; true trims unquoted fields while preserving quoted padding |
| header | false | First row retained by default; true consumes/cleans headers; supplied names describe output columns; empty name sequence consumes no row |
| select-columns | absent/empty: all columns | Reorder, repeat and positive out-of-range padding; explicit empty selection retains columns |
| trim-rows | false | Ragged widths retained by default; true uses first-row width; nonempty selection takes precedence |

Returned fields remain strings. Duplicate header labels map to the first occurrence in the tested `column-index` and `get` paths; duplicate data rows are preserved. Blank-row/empty-field width is explicit in focused summaries. Current `csv-to-xml` rows are compared by namespace-qualified structure converted to arrays, avoiding assumptions about serialization prefixes or document identity. This is not a schema-aware, node-identity or complete result-record audit.

## Validation and local resources

Unknown unqualified options and tested nonboolean values/nonmap options raise XPTY0004 on present input. Present empty text still inspects options; absent input bypasses the tested unknown option. No general error-precedence rule for all simultaneous failures follows.

[`CsvResourceContractTests`](../../tests/Bosak.XPath.Standard.Tests/CsvResourceContractTests.cs) checks absolute file URIs and relative URIs with explicit compile/runtime base URIs, omitted/empty options, header/selection, known keywords, UTF-8 BOM, empty/blank files, malformed input, invalid options and missing-file FOUT1170. Every fixture has a unique temporary directory and explicit file/directory cleanup. No remote HTTP resource or application global registry is required. The supplier `csv-doc` examples resolve local CSV relative to the stylesheet's static base URI and run unchanged through the existing catalog harness.

## Observed current-draft differences and follow-up

| Boundary | Published1.23.8 / executable observation | Current pinned draft | Disposition |
|---|---|---|---|
| Non-positive select-columns | `observed-nonpositive-select` and `observed-negative-select` raise XPTY0004 | Integers below one insert empty fields | Frozen `parse-csv-912`/`913` predate the issue2849 correction. Current upstream agrees with the draft; the [revision audit](CSV-INDEX-CONTRACT.md) also identifies five `get` changes. Engine/baseline adoption remains separate. |
| Unknown namespaced QName extension option | `observed-qname-extension` raises XPTY0004 | Shared conventions exempt unknown QName keys with a non-absent namespace from the unknown-key error | Record a separate option-key policy/coercion review; this slice claims no extension-key support. |

These three `observed-*` cases are versioned behavior records, **not normative conformance assertions**. No expectation was weakened to hide an engine failure and no frozen test was edited. Complete coercion (numeric/untyped/node values), supplementary Unicode delimiters, every `get`/record function type, transport/media-type encoding, network policy, resource cancellation and every header/selection interaction remain separate. Any engine correction needs its own bounded review and PR; this documentation/test PR changes no engine code. No upstream patch is submitted here; any later contribution must start from a refreshed upstream default branch.

## Business report and verification

The [supplier-import task](examples/csv-contracts/README.md) supplies normal, empty, reordered, duplicate-SKU and invalid-quantity fixtures. Four complete transformations consume the same logical values: portable3.0 host-parsed XML,4.0 `parse-csv`,4.0 `csv-to-xml`, and4.0 `csv-doc`. The3.0 fixture is an already-parsed boundary, not a replacement CSV parser. The three CSV paths also refuse an unterminated field. Business integer validation is separate from CSV parsing.

Initial focused execution:97/2. Both failures came from Windows newline translation doubling literal CR before LF in the generated CRLF test expressions. Original artifacts/TRX retained; the expressions now use explicit `char(13)`/`char(10)` and the original expectations pass99/0. Resource tests pass25/0. No engine correction was needed. Final validation: **11,060 solution tests pass**, including3,148 catalog +24 chapter cases and all25 resource tests;84 additional language-server tests pass. Final incremental Release build has zero errors/warnings; earlier existing conformance CS8604 warning is retained. Final test source includes explicit empty-file versus blank-row counts. The linked machine audit retains every full TRX hash. Original manifests, fixtures, upstream notes, receipts, stashes/worktrees and protected PR132 are preserved.

Manual gates are not rerun for this test/documentation-only change. Latest corrected-engine receipts in [the function default-mode audit](../XSLT-FUNCTION-DEFAULT-MODE.json) remain QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**; gate membership is unchanged. Passing CSV gates is bounded evidence and does not resolve the current-draft/test discrepancy above.
