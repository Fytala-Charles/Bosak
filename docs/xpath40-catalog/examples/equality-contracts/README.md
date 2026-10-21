<div align="center">
<img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak Customer-reference Equality and Collation">
<h1>Customer-reference Equality and Collation</h1>
<p>21 October 2026</p>
</div>

## Customer-reference matching policy

A synthetic customer import needs to match the requested reference `acme`, report duplicate reference classes, retain every matching input position and identify exact `vip` tokens. This example deliberately uses HTML ASCII case-insensitive collation for these business references. Real identifiers can require case-sensitive comparison; the receiving system must choose that policy. It does not perform account linking, identity verification, address matching or external IO.

Complete input, also embedded verbatim in both expressions:

```xml
<customers><customer id="C1" reference="ACME" tags="priority vip"/><customer id="C2" reference="acme" tags="VIP"/><customer id="C3" reference="CAFÉ" tags="standard"/><customer id="C4" reference="CAFÉ" tags="vip-plus"/></customers>
```

`id` is the report identifier; `reference` is compared with `acme`; `tags` is XML-whitespace-separated text. Inputs are synthetic and assume those fields exist; the example does not validate missing/duplicate customer IDs or establish a production schema. C3 uses precomposed U+00C9; C4 uses U+0045 followed by U+0301. ASCII case folding does not remove accents or normalize Unicode.

## Runnable 4.0 expression

The expression requires explicit `XPathCompatibility.XPath40` or the experimental mode. It parses the literal XML itself, so no implicit context item, host collation or network access is required. [The file](report-normal-40.xpath) uses supported keyword calls for `index-of` and `contains-token`; other calls are positional.

```xquery
let $doc := parse-xml('<customers><customer id="C1" reference="ACME" tags="priority vip"/><customer id="C2" reference="acme" tags="VIP"/><customer id="C3" reference="CAFÉ" tags="standard"/><customer id="C4" reference="CAFÉ" tags="vip-plus"/></customers>')/customers
return let $coll := 'http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive'
return let $refs := $doc/customer/@reference ! string(.)
return let $positions := index-of(input := $refs,target := 'acme',collation := $coll)
return let $matches := $doc/customer[compare(string(@reference),'acme',$coll) eq 0]
return let $ids := $matches ! string(@id)
return concat('references=',count(distinct-values($refs,$coll)),';positions=',string-join($positions,','),';ids=',string-join($ids,','),';vip=',count($matches[contains-token(value := string(@tags),token := 'vip',collation := $coll)]),';expected-order=',deep-equal($ids,('C1','C2')))
```

Exact result:

```text
references=3;positions=1,2;ids=C1,C2;vip=2;expected-order=true
```

`distinct-values` counts three reference equivalence classes under the explicit collation. No representative spelling or distinct-value ordering is assumed. `index-of` returns all matching one-based positions, including duplicates. `compare(...)=0` selects C1/C2 while preserving input order. `contains-token` accepts both `vip` and `VIP`, but not `vip-plus`. `deep-equal` checks that the selected ID sequence is exactly the stated two-ID sequence in that order; it is a report check, not a complete customer validation rule.

## Complete XPath 3.1 baseline

[The baseline](report-normal-31.xpath) supplies the same XML, policy and output. All five function names already exist in 3.1; the illustrated4.0 convenience is keyword argument syntax. Positional calls and nested `return let` bindings keep this baseline in 3.1 grammar. The broader 4.0 `compare` atomic contract and `deep-equal` options-map declaration are separate boundaries, not prerequisites for this report.

```xquery
let $doc := parse-xml('<customers><customer id="C1" reference="ACME" tags="priority vip"/><customer id="C2" reference="acme" tags="VIP"/><customer id="C3" reference="CAFÉ" tags="standard"/><customer id="C4" reference="CAFÉ" tags="vip-plus"/></customers>')/customers
return let $coll := 'http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive'
return let $refs := $doc/customer/@reference ! string(.)
return let $positions := index-of($refs,'acme',$coll)
return let $matches := $doc/customer[compare(string(@reference),'acme',$coll) eq 0]
return let $ids := $matches ! string(@id)
return concat('references=',count(distinct-values($refs,$coll)),';positions=',string-join($positions,','),';ids=',string-join($ids,','),';vip=',count($matches[contains-token(string(@tags),'vip',$coll)]),';expected-order=',deep-equal($ids,('C1','C2')))
```

## Empty and reordered inputs

Complete empty input:

```xml
<customers/>
```

[3.1](report-empty-31.xpath) and [4.0](report-empty-40.xpath) both return:

```text
references=0;positions=;ids=;vip=0;expected-order=false
```

The empty result is valid for this report; `false` means it does not equal the illustrative C1/C2 sequence. A receiving application that requires at least one match must enforce that rule explicitly.

Complete reordered input:

```xml
<customers><customer id="C2" reference="acme" tags="VIP"/><customer id="C1" reference="ACME" tags="priority vip"/><customer id="C4" reference="CAFÉ" tags="vip-plus"/><customer id="C3" reference="CAFÉ" tags="standard"/></customers>
```

[3.1](report-reordered-31.xpath) and [4.0](report-reordered-40.xpath) both return:

```text
references=3;positions=1,2;ids=C2,C1;vip=2;expected-order=false
```

Matching membership is retained, while positions and ID order refer to the supplied input. `deep-equal` is order-sensitive and does not assert set equality. No implicit sorting is performed.

## Compatibility and implementation boundaries

All 58 manifest cases run through the unchanged public catalog runner. Successful common4.0 cases declare their3.1 baseline explicitly; keyword successes in experimental mode also declare their frozen 4.0 outcome. The 3.1 keyword fixtures refuse with XPST0003. Positional comparison of numeric2 and10 returns-1 in 4.0, while the 3.1 call refuses XPTY0004. This numeric comparison is a4.0 extension of an existing function, not a new name.

`index-of(input,target,collation)` and `contains-token(value,token,collation)` have keyword metadata. `compare` currently accepts `comparand1`/`comparand2`, while the pinned draft declares `value1`/`value2`; the draft spellings refuse XPST0017. `distinct-values` and `deep-equal` have no keyword metadata and refuse tested keyword forms at both 4.0 levels. These observations are not recommended alternatives to the normative spellings.

The pinned draft defaults an explicitly empty collation for `index-of`; Bosak positional#3 instead raises XPTY0004. The pinned `deep-equal` third argument accepts an options map, but Bosak atomizes it as a legacy collation argument and raises FOTY0013. The two fixtures explicitly carry `reviewStatus=observed-implementation-gap` and separate `normative40Expected` metadata. They preserve current behavior for diagnosis; their passing tests do not certify draft conformance. Use omitted or valid string collation arguments in supported consumer examples. No engine correction is included.

NaN illustrates distinct equality contracts: two NaNs yield one distinct value and are deep-equal, but `index-of` uses value equality and finds neither NaN equal to itself. Case-insensitive deep comparison of map values does not make differently spelled map keys equivalent. ASCII collation does not equate Ä/ä or normalize CAFÉ/CAFÉ. NFC normalization is an explicit, separate policy in its own fixture.

Tests use fresh default contexts; host-custom default collation, all UCA configurations, complete coercion/options coverage and every draft production are not certified. Unknown nonempty collation fixtures raise FOCH0002 on the stated inputs. General unused-argument error eagerness is not claimed.

## Exact fixture inventory

| Runnable fixture | Mode | Result/error |
|---|---|---|
| [distinct-codepoint](distinct-codepoint.xpath) | 40 | `2` |
| [distinct-ascii](distinct-ascii.xpath) | 40 | `1` |
| [distinct-empty](distinct-empty.xpath) | 40 | `0` |
| [distinct-nan](distinct-nan.xpath) | 40 | `1` |
| [distinct-unknown-collation](distinct-unknown-collation.xpath) | 40 | `FOCH0002` |
| [distinct-empty-collation](distinct-empty-collation.xpath) | 40 | `2` |
| [index-codepoint](index-codepoint.xpath) | 40 | `1,3` |
| [index-ascii](index-ascii.xpath) | 40 | `1,2,3` |
| [index-empty](index-empty.xpath) | 40 | `0` |
| [index-empty-target](index-empty-target.xpath) | 40 | `XPTY0004` |
| [index-nan](index-nan.xpath) | 40 | `0` |
| [index-unknown-collation](index-unknown-collation.xpath) | 40 | `FOCH0002` |
| [index-empty-collation](index-empty-collation.xpath) | 40 | `XPTY0004` (observed gap; draft expects `1`) |
| [deep-same-order](deep-same-order.xpath) | 40 | `true` |
| [deep-reordered](deep-reordered.xpath) | 40 | `false` |
| [deep-nan](deep-nan.xpath) | 40 | `true` |
| [deep-map-keys](deep-map-keys.xpath) | 40 | `false` |
| [deep-map-values](deep-map-values.xpath) | 40 | `true` |
| [deep-empty-collation](deep-empty-collation.xpath) | 40 | `false` |
| [deep-options-map](deep-options-map.xpath) | 40 | `FOTY0013` (observed gap; draft expects `true`) |
| [compare-ascii](compare-ascii.xpath) | 40 | `0` |
| [compare-codepoint](compare-codepoint.xpath) | 40 | `-1` |
| [compare-empty](compare-empty.xpath) | 40 | `true` |
| [compare-numeric](compare-numeric.xpath) | 40 | `-1` |
| [compare-unknown-collation](compare-unknown-collation.xpath) | 40 | `FOCH0002` |
| [token-whole](token-whole.xpath) | 40 | `true` |
| [token-substring](token-substring.xpath) | 40 | `false` |
| [token-ascii-boundary](token-ascii-boundary.xpath) | 40 | `false` |
| [token-empty](token-empty.xpath) | 40 | `false` |
| [token-two-words](token-two-words.xpath) | 40 | `false` |
| [token-unknown-collation](token-unknown-collation.xpath) | 40 | `FOCH0002` |
| [token-empty-collation](token-empty-collation.xpath) | 40 | `false` |
| [unicode-codepoint](unicode-codepoint.xpath) | 40 | `false` |
| [unicode-normalized](unicode-normalized.xpath) | 40 | `true` |
| [distinct-keyword-31](distinct-keyword-31.xpath) | 31 | `XPST0003` |
| [distinct-keyword-40](distinct-keyword-40.xpath) | 40 | `XPST0017` |
| [distinct-keyword-experimental](distinct-keyword-experimental.xpath) | experimental | `XPST0017` |
| [deep-keyword-31](deep-keyword-31.xpath) | 31 | `XPST0003` |
| [deep-keyword-40](deep-keyword-40.xpath) | 40 | `XPST0017` |
| [deep-keyword-experimental](deep-keyword-experimental.xpath) | experimental | `XPST0017` |
| [index-keyword-31](index-keyword-31.xpath) | 31 | `XPST0003` |
| [index-keyword-40](index-keyword-40.xpath) | 40 | `1,2` |
| [index-keyword-experimental](index-keyword-experimental.xpath) | experimental | `1,2` |
| [token-keyword-31](token-keyword-31.xpath) | 31 | `XPST0003` |
| [token-keyword-40](token-keyword-40.xpath) | 40 | `true` |
| [token-keyword-experimental](token-keyword-experimental.xpath) | experimental | `true` |
| [compare-implemented-keyword-31](compare-implemented-keyword-31.xpath) | 31 | `XPST0003` |
| [compare-implemented-keyword-40](compare-implemented-keyword-40.xpath) | 40 | `0` |
| [compare-implemented-keyword-experimental](compare-implemented-keyword-experimental.xpath) | experimental | `0` |
| [compare-draft-keyword-31](compare-draft-keyword-31.xpath) | 31 | `XPST0003` |
| [compare-draft-keyword-40](compare-draft-keyword-40.xpath) | 40 | `XPST0017` |
| [compare-draft-keyword-experimental](compare-draft-keyword-experimental.xpath) | experimental | `XPST0017` |
| [report-normal-31](report-normal-31.xpath) | 31 | `references=3;positions=1,2;ids=C1,C2;vip=2;expected-order=true` |
| [report-normal-40](report-normal-40.xpath) | 40 | `references=3;positions=1,2;ids=C1,C2;vip=2;expected-order=true` |
| [report-empty-31](report-empty-31.xpath) | 31 | `references=0;positions=;ids=;vip=0;expected-order=false` |
| [report-empty-40](report-empty-40.xpath) | 40 | `references=0;positions=;ids=;vip=0;expected-order=false` |
| [report-reordered-31](report-reordered-31.xpath) | 31 | `references=3;positions=1,2;ids=C2,C1;vip=2;expected-order=false` |
| [report-reordered-40](report-reordered-40.xpath) | 40 | `references=3;positions=1,2;ids=C2,C1;vip=2;expected-order=false` |

The [review](../../EQUALITY-COLLATION-CONTRACT-REVIEW.md), [feature entry](../../functions/customer-reference-matching.md) and [machine-readable audit](../../audit-equality-collation-contracts.json) separate normative declarations, current observations, original failures and remaining work.
