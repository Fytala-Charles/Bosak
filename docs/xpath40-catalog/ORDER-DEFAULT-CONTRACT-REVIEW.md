<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak ordering and duplicate contract review">
  <br><br><h1>Ordering and Merge Default Contracts</h1>
  <p>Existing function defaults, comparison and duplicate policies</p>
</div>

## Scope and sources

Reviewed 2026-10-12 against Bosak `28144810a56370d3901ceca5ef35bb1759053b47`, published version 1.1.3. This is a targeted review of fn:sort, array:sort, fn:min, fn:max and map:merge following the [197-name inventory](CHANGED-31-CONTRACT-INVENTORY.md). The [raw evidence](audit-order-default-contracts.json) retains baseline and pinned declarations, literal registrations and all case outcomes. Sources: [dated W3C F&O 3.1 catalog](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/function-catalog.xml) and [pinned QT4CG F&O e7ee4ab8 catalog](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml). The immutable pin describes this review, not recovered historical adoption.

132 new checks: 126 focused checks including keyword paths/modes, plus six complete normal/empty/reordered supplier reports. Catalog: 67 entries, 2,578 feature + 24 chapter checks = 2,602; four worked chapters, 117 assigned names. REQ-126 remains In Progress, D2 partial, version 1.1.3. No engine/API/harness changes, release or gate promotion.

## Exact declaration boundaries

| Family | Pinned contract | Observed default boundary |
|---|---|---|
| fn:sort | input item()*, optional collation and optional unary key returning atomic values; key defaults to data#1, including empty argument | Omitted key works; explicit empty key fails XPTY0004 on nonempty input, succeeds on empty input. |
| array:sort | array(*), optional collation and unary member key; key defaults to data#1, including empty argument | Same empty-key boundary; callbacks receive whole members, and empty/grouped members stay grouped. |
| fn:min / fn:max | values xs:anyAtomicType*, optional collation; no key callback, arities 1–2 | Empty values return empty; empty collation works. values keyword works in 4.0/experimental; input/key labels refuse XPST0017. #3 lookup is zero, as the draft also requires. |
| map:merge | maps map(*)*, optional options map(*)? default empty map, including empty argument | Omitted options and explicit map{} work; explicit empty options fail XPTY0004 even with no input. |

Pinned fn:sort is retained for compatibility and defined through fn:sort-by; this review does not re-review sort-by or sort-with. Baseline sort keys and merge options are required when their third/second argument is supplied; pinned optional/default-on-empty declarations must not be inferred from legacy overload availability. Correct keyword forms work at 4.0 and experimental, and refuse XPST0003 at 3.1 grammar. Direct positional and named references are checked separately. min/max have no key parameter in either source; their #3 refusal is not missing implementation.

## Comparison, conversion and validation findings

Numeric sort and negative-key sort, lexicographic multi-key results, stable ties and node identity are verified. A unary callback returning no keys preserves input order in the tested sequence. Invalid nonfunction callbacks and zero/two-argument callback arities fail XPTY0004 when invoked; invalid callbacks can be bypassed by empty input. This is invocation-boundary evidence, not certification of complete function conversion. fn:sort receives individual items; array:sort receives whole members.

Explicit codepoint and HTML ASCII case-insensitive collations distinguish ('a','B') as B,a and a,B. Fresh contexts use the normal default collation; this review does not inject a host-custom default. Tested unknown collation URIs are accepted by fn:sort and array:sort, including a two-string comparison, while nonempty string min/max refuse FOCH0002. Empty min/max bypass the tested unknown collation. Keep availability/validation behavior as a follow-up; no general claim about every unused collation or host context follows.

NaN min/max return NaN. Sorting (1,NaN,2) produces 2,NaN,1, while (NaN,2,1) produces 1,2,NaN in both tested profiles. Retain a dedicated NaN ordering follow-up and validate finite numeric business inputs. Mixed number/string sort refuses XPTY0004; mixed min/max refuses FORG0006. Untyped numeric min/max input is converted to double; malformed untyped input refuses FORG0001. Decimal-only results retain a decimal type in both observed profiles; this is not a complete mixed numeric promotion audit. Numeric XML nodes atomize successfully for min/max. min/max atomize [3,1,2], while passing that array as one fn:sort item refuses FOTY0014. Array sorting itself preserves member boundaries.

## Duplicate and options policy

map:merge defaults to use-first in both sources. Explicit use-first/use-last/combine return 1/2/1,2 for duplicate A entries; reject raises FOJS0003 only when a duplicate exists. combine concatenates values in input order. An existing empty-valued key remains present; combining two empty values gives a present key with zero values. Integer and decimal equal keys combine in the tested fixture. Map enumeration/insertion order and the full map:build option space are not certified here.

Bad duplicates values raise FOJS0005; an unknown option is ignored in the tested form. A multi-item duplicates value ('use-first','use-last') selects the tested first policy instead of rejecting its cardinality. A mixed maps input containing numeric 42 silently skips that item. These conversion/cardinality boundaries remain engine follow-ups. The [current MapMerge implementation](../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs) explicitly skips non-map items; this observation does not justify production use of malformed inputs. Pinned map:merge imports its option definitions from map:build, with its own use-first default; do not describe all imported options as verified.

## Complete consumer example

The [supplier-offer README](examples/order-contracts/README.md) supplies complete XML, paired 3.1/4.0 expressions and exact output. Validate unique IDs, product codes and nonnegative integer prices in a single unit. Rank using (numeric price, ID), so ties are deterministic under reordering. Keep duplicate products with explicit combine; min/max provide a range, not all tied winners. The example uses explicit valid callbacks and a real options map rather than unsupported empty defaults.

Use simple mapping to extract IDs from the sorted node sequence: `$rank ! string(@id)`. A path `$rank/@id` restores document order under XPath path semantics and would discard the ranking during projection. This is a consumer semantic distinction, not a sorting engine defect.

## Next work

Continue existing-function default/conversion contracts with fn:distinct-values, fn:index-of, fn:deep-equal, fn:compare and fn:contains-token. Review exact equality, collation and cardinality boundaries against the complete inventory. Retain the empty-key/options, unknown-collation, NaN sorting and malformed merge input/option follow-ups for later bounded engine remediation. D2 semantic/prose coverage, all 37 absent keyword metadata families, D3 grammar/pattern, D4 acceptance/provenance, historical pins and AC-03 applicability remain open.


## Delivery validation

Release solution build passed (zero warnings/errors); **6,806 tests / 0 failures / 0 skips**, ten assemblies including all 2,578 feature checks. Renderer verified 103 catalog pages plus six implementation/canonical pages. Five pinned declarations, 132 exact fixtures with explicit version expectations, six independently computed supplier reports, local links/code fences, 67 sixteen-field entries, 241 cached primary links/anchors, 28 delivery/test records and 117 assigned names verified. All 37 reviewed keyword families still lack metadata. The first full run exposed five experimental fixtures missing frozenExpected assertions; the corrected final full run passed. No engine/API/harness changes; manual engine gates were not rerun.
