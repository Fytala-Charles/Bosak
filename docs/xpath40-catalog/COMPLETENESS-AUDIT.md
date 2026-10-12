<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak REQ-126 catalog completeness audit">
  <br><br><h1>REQ-126 Catalog Completeness Audit</h1>
  <p>Evidence, missing coverage, and the next documentation deliveries</p>
</div>

## Finding and scope

**REQ-126 is not ready to close.** Four worked chapters are supplied, but
the catalog does not yet cover every landed feature or record every deferred and
rejected decision. Existing entries also need more precise provenance and broader
examples. The original audit was dated 2026-10-11 against source revision
`09e678f3` (1.1.1 publication documentation); published engine 1.1.1 is revision
`d35cf1d4bd1d1b7143ccb9ab304812f781dc0d50`.

Current review: **2026-10-12**, source baseline `bb91dea2c31b7cfacb346b1689680517eae56d87`, public engine **1.1.2** (`3aebf76a`). All **117 registered namespace/local-name pairs** now have entries. This is a name-inventory milestone, not proof of exhaustive signatures, grammar, dispositions or conformance.

The original audit baseline was 28 entries, 81 catalog cases, three worked chapters, and 13
chapter checks. All entries have the 16 numbered fields. Field presence does not
establish complete or accurate content. Chapter completeness is separate from
whole-feature inventory completeness.

Read alongside [REQ-126 acceptance criteria](../FEATURE_REQUESTS.md#req-126-xpath40-feature-catalog--gof-style-catalog-of-xpathxslt-40-syntax-decisions),
the [REQ-118 adoption dossier](../REQ-118-xpath-xslt-40.md),
and the [catalog index](README.md).

## External specifications and source documentation

These primary sources provide language definitions, migration baselines, draft
history, and upstream tests. Links and section anchors were checked on
2026-10-11. The 4.0 documents are evolving QT4CG drafts, not final W3C
Recommendations. Current links are navigation references; they do not prove that
Bosak adopted every feature or the latest semantics. Record the adopted draft
revision/date and Bosak delivery revision alongside feature-specific references.
When wording or numbering changes, retain historical disposition evidence and
compare the current draft explicitly.

| Area | Primary source | Use |
|---|---|---|
| XPath syntax and types | [XPath 4.0 draft](https://qt4cg.org/specifications/xquery-40/xpath-40.html) | Operators, bindings, grammar, coercion, and compatibility. |
| Functions and operators | [F&O 4.0 draft](https://qt4cg.org/specifications/xpath-functions-40/Overview.html) | Signatures, arguments, results, errors, and function examples. |
| Stylesheet language | [XSLT 4.0 draft](https://qt4cg.org/specifications/xslt-40/Overview.html), [changes since 3.0](https://qt4cg.org/specifications/xslt-40/Overview.html#changes-since-3.0), [incompatibilities](https://qt4cg.org/specifications/xslt-40/Overview.html#incompatibilities) | Instruction behavior and migration differences. |
| Data model | [XDM 4.0 draft](https://qt4cg.org/specifications/xpath-datamodel-40/Overview.html) | Atomic items, nodes, maps, arrays, and JNodes. |
| Serialization | [Serialization 4.0 draft](https://qt4cg.org/specifications/xslt-xquery-serialization-40/Overview.html) | Output methods, parameters, and serialization errors. |
| Stable expression baseline | [XPath 3.1](https://www.w3.org/TR/xpath-31/), [F&O 3.1](https://www.w3.org/TR/xpath-functions-31/) | Earlier-version syntax and migration examples. |
| Stable stylesheet/model/output baseline | [XSLT 3.0](https://www.w3.org/TR/xslt-30/), [XDM 3.1](https://www.w3.org/TR/xpath-datamodel-31/), [Serialization 3.1](https://www.w3.org/TR/xslt-xquery-serialization-31/) | Portable worked chapters and baseline contracts. |
| Draft source and discussions | [QT4CG specification repository](https://github.com/qt4cg/qtspecs) | Source history, issues, pull requests, and adoption decisions. |
| Upstream 4.0 tests | [qt4tests](https://github.com/qt4cg/qt4tests), [xslt40-test](https://github.com/qt4cg/xslt40-test) | XPath/XQuery versus XSLT evidence; distinguish upstream tests from Bosak gate membership. |
| Upstream 3.x tests | [QT3](https://github.com/w3c/qt3tests), [xslt30-test](https://github.com/w3c/xslt30-test) | Regression baselines and XSLT 3.0 conformance evidence. |

Specification links establish language meaning; Bosak source, named tests, and
conformance runners establish implemented coverage. Deferred/rejected entries
link current alternatives, while their historical disposition must cite the
relevant issue/PR or archived draft when that evidence is supplied.

### Draft-evolution sources used by this audit

| Decision area | Primary history reference | Audit use |
|---|---|---|
| Destructuring and extended bindings | [QT4CG PR #1131](https://github.com/qt4cg/qtspecs/pull/1131) | Proposal context for the delivered binding forms. |
| Method-call scope | [QT4CG issue #2219](https://github.com/qt4cg/qtspecs/issues/2219) | Sequence generalization discussion behind the recorded deferral. |
| Experimental scan | [QT4CG PR #2824](https://github.com/qt4cg/qtspecs/pull/2824) | Proposal history; Bosak tier/gate evidence remains separate. |
| Nominative versus structural records | [QT4CG PR #2934](https://github.com/qt4cg/qtspecs/pull/2934) | Context for keeping the two type decisions distinct. |

These links support the history review. They do not replace an immutable adopted
specification revision or establish that Bosak implements the entire proposal.

## Acceptance assessment

| Criterion | Assessment | Evidence and remaining work |
|---|---|---|
| AC-01: landed slices, tiers, spec and delivery links | Partial | All registered names now have entries; supplementary signatures/options and changed existing functions remain partial. The 28 baseline entries now link exact Bosak delivery revisions and named tests. Immutable adopted specification revisions were not retained historically and remain explicit gaps. |
| AC-02: deferred/rejected decisions | Partial | Method-call and renamed mapping-arrow entries exist; dedicated records now cover the missing section 1.2 decisions, including shipped collection-sequence iteration and the partial JNode boundary. Further deferred grammar needs review. |
| AC-03: verified examples and gate evidence | Partial | 585 executable documentation checks pass. Baseline entries now carry named implementation evidence and explicit assigned-set or unassigned status. One concrete stale claim, fn:scan gate status, is corrected with this audit. |
| AC-04: structure, retained entries, drift warnings | Partial | 61 entries have all numbered fields and index links. No entry is removed by this audit. Generic prior-art disclaimers do not replace feature-specific drift explanations; supplementary forms and historical lifecycle need review. |
| AC-05: discoverability and canonical links | Met for current scope | README roadmap/catalog links, integration, and the REQ-118 reference companion exist; registry and index remain In Progress. This does not close other criteria. |

## Runtime function inventory

The [machine-readable snapshot](audit-function-inventory.json) contains the
published engine's public `FunctionLibrary.XPath40OnlyFunctionNames` and
`XPath40ExperimentalOnlyFunctionNames` sets. These are namespace/local-name
pairs, **not function signatures or arities**.

| Registered category | Names | Catalog assessment |
|---|---:|---|
| Frozen `fn:*` | 58 | 58 frozen fn names have family entries; signature and semantic coverage remain partial. |
| Frozen `map:*` | 4 | All four names map to the product-index family; the entries/items result-shape correction shipped in 1.1.2. Supplementary contracts still need review. Map iteration syntax is a different feature. |
| Frozen `array:*` | 5 | All five names have family entries: four batch functions plus array:sort-with in the sorting family. |
| Frozen zero-argument `xs:*` constructors | 49 | One constructor-family entry exists, with an integer example; this does not verify all constructor contracts. |
| Experimental `fn:scan` | 1 | Entry and verified examples exist; gate status was stale. |
| Total | 117 | 117 names map to an existing family entry; 0 have no dedicated family entry. A mapped name is not a completeness pass. |

Mapped non-constructor families now include scalar equality, CSV, element/map
conversion, comparator sorting, JNode navigation, higher-order selection,
partitioning, distinctness, product-map indexing, array batches, positional
sequence selection and experimental scan. The snapshot records an exact matching
gate identifier only when it occurs in GatedSets; null means no exact-name set
is assigned, not proof that no broader set exercises the function.

The snapshot does **not** inventory changed behavior/overloads of existing 3.1
functions, parser grammar, type syntax, XSLT instructions, or resource routes.
For example, upgraded `fn:compare`, QName `min`/`max`, changed map/array overloads,
and collations require an additional semantic/signature audit. The missing-name
count is not a count of missing Markdown files: related functions can share a
well-explained family entry.

### Family coverage and remaining work

| Family | Registered names without a dedicated family entry | Suggested realistic case |
|---|---|---|
| Sequence selection and repetition — supplied in this delivery | [`fn:replicate`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-replicate), [`fn:slice`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-slice), [`fn:items-at`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-items-at), [`fn:foot`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-foot), [`fn:trunk`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-trunk), [`fn:insert-separator`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-insert-separator) | Paginate an order export while retaining boundary records. |
| Equality, duplicates, subsequences — supplied in this delivery | [`fn:all-equal`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-all-equal), [`fn:contains-subsequence`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-contains-subsequence), [`fn:starts-with-subsequence`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-starts-with-subsequence), [`fn:ends-with-subsequence`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-ends-with-subsequence) | Check shipment-event identifiers and required processing steps. |
| Higher-order processing — supplied in this delivery sequence | [`fn:take-while`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-take-while), [`fn:drop-while`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-drop-while), [`fn:while-do`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-while-do), [`fn:do-until`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-do-until), [`fn:partial-apply`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-partial-apply), [`fn:transitive-closure`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-transitive-closure), [`fn:identity`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-identity) | Route invoice lines, bound retries, and follow product dependencies with cycles. |
| Ranking, operators, type inspection — supplied | [`fn:highest`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-highest), [`fn:lowest`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-lowest), [`fn:sort-by`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-sort-by), [`fn:op`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-op), [`fn:is-NaN`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-is-NaN), [`fn:atomic-type-annotation`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-atomic-type-annotation) | Prioritize work orders and describe imported measurements. |
| Strings and Unicode — text and hash families supplied; segmentation limits remain | [`fn:char`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-char), [`fn:characters`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-characters), [`fn:graphemes`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-graphemes), [`fn:pad-string`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-pad-string), [`fn:trim-space`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-trim-space), [`fn:index-of-substring`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-index-of-substring), [`fn:substring-before-last`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-substring-before-last), [`fn:substring-after-last`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-substring-after-last), [`fn:hash`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-hash) | Clean imported text with explicit segmentation limits; fingerprint supplied payloads. |
| URI and collation — URI family and collation availability supplied | [`fn:parse-uri`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-parse-uri), [`fn:build-uri`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-build-uri), [`fn:decode-from-uri`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-decode-from-uri), [`fn:collation-available`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-collation-available) | Construct and validate document-resource identifiers. |
| Dates and durations — supplied in this delivery | [`fn:seconds`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-seconds), [`fn:duration-to-seconds`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-duration-to-seconds), [`fn:build-dateTime`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-build-dateTime), [`fn:unix-dateTime`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-unix-dateTime), [`fn:days-in-month`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-days-in-month) | Convert timestamps and build a monthly dispatch calendar. |
| Maps — supplied in this delivery | [`map:build`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-build), [`map:entries`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-entries), [`map:filter`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-filter), [`map:items`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-items) | Index a product feed with an explicit duplicate-key policy. |
| Arrays — supplied in this delivery | [`array:build`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-array-build), [`array:empty`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-array-empty), [`array:items`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-array-items), [`array:slice`](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-array-slice) | Preserve record boundaries when splitting import batches. |

## Landed syntax and cluster coverage

| Delivery area | Current coverage | Gap to resolve |
|---|---|---|
| REQ-118 S0 | Tier table and manifest modes | Explain arity-sensitive rejection and experimental boundaries in each affected family. |
| S1 / S2 / S5 | Function registry/tests are linked indirectly | Add the missing sequence/string/URI/date/map/array/HOF families above. |
| S3 | Otherwise, keyword arguments, string templates | Add binary/hex/underscore literals and guarded-expression behavior; distinguish valid forms and refusals. |
| S4 | Pipeline, mapping arrow, array/map iteration | Add focus-function syntax and `fn` abbreviation. The existing focus-functions filename currently explains zero-arity constructors, a separate later cluster. |
| S6a / S6b | Enum, choice, record, typed-let entries | Expand coercion/cast/lookup rules and `but with`; current record example does not cover the update construct. |
| S7 | Four instruction families | Expand actual supported forms and errors, particularly xsl:map duplicate functions and map-entry usage. |
| S8 | Arrays and switch | Audit delivered pattern/type-pattern and priority behavior before claiming coverage; no pattern-system entry exists. |
| REQ-123 operator/CSV/conversion clusters | CSV/conversion entries exist | fn:op supplied; expand plan-driven conversion and supplementary function forms. |
| REQ-123 comparison/sorting cluster | Sorting and atomic equality | Add comparison semantics, array comparator form, NaN/type annotation, QName ordering and collation behavior. |
| REQ-123 JNode and destructuring clusters | Entries exist | Audit JNode type/path forms and every destructuring shape; a single successful example does not cover all forms. |
| REQ-123 experimental scan | Entry, examples, named gate | Keep its experimental tier distinct despite its promoted test set. |

The delivery history in the repository identifies concrete PRs: operator #102,
CSV #104, conversion #106, atomic equality #108, focus constructors #110,
sorting #112, comparison tail #114, JNode #116, and destructuring #129.
These are starting points for per-entry provenance, not substitutes for checking
the exact source and tests. Specification section numbers must be checked against
the adopted draft revision when entries are authored; this audit does not certify
that historical dossier numbers still match the live draft.

## Deferred and rejected decisions

| Dossier decision | Current record | Needed disposition work |
|---|---|---|
| XSLT named item/record declarations | [Item-type record](deferred/xslt-item-type.md), [record-type record](deferred/xslt-record-type.md) | Unused declaration tolerance is separate from named type/constructor support; portable validation and structural cast alternatives supplied. |
| Computed XPath node constructors | [Deferred constructor record](deferred/computed-node-constructors.md) | Braced-name draft syntax refused in all three XPath modes; existing XQuery evidence and complete XSLT alternative are separate. |
| Method-call `=?>` | Dedicated Deferred entry | Preserve the churn rationale and exact refused shape. |
| Nominative record types | [Deferred record](type-system/nominative-records.md) | Kept distinct from shipped structural record contracts. |
| Iteration over sequences of arrays/maps | [Shipped S4 extension](operators-and-syntax/sequence-collection-iteration.md) | Named tests and three published examples pin collection boundaries and invalid input. |
| fn:scan | Experimental entry | Historical deferral is superseded by experimental adoption; gate status corrected here. |
| JNode built-in template rules and full GNode/JNode model | [Partial-model boundary](data-model/jnode-template-boundary.md) | Navigation shipped; full model and JNode-specific XSLT built-in behavior are not certified. |
| `fn:all` / `fn:some-of` | [Rejected names](rejected/obsolete-quantifier-functions.md) | Exact XPST0017 refusals and accepted predicate alternative supplied. |
| `scan-left` / `scan-right` | [Superseded names](rejected/directional-scan.md) | Refused names and left-fold/array-state semantics recorded. |
| Old mapping arrow `=>>` | Dedicated rejected entry | Retain renamed disposition and link to `=!>`. |
| XPath switch expression | [XPath rejection](rejected/xpath-switch-expression.md) | Exact grammar refusal plus a 3.1 conditional alternative; XQuery and XSLT remain separate. |
| Extensible map types | [Proposal disposition](rejected/extensible-map-types.md) | Supplied cast/coercion examples pin surplus-field behavior; maps themselves remain supported. |

Computed XPath constructors now have the dedicated record above. XSLT type declarations now also have dedicated records above. The wider dossier still defers enclosing modes, capturing accumulators, fn:apply-templates,
Serialization 4.0 changes, external-parser/validator functions, and XQuery/
streaming deltas. Include them in the broader decision inventory after the
section-1.2 acceptance scope is covered. Do not implement a deferred shape merely
because its documentation is missing.

## Prioritized deliveries

| Priority | Concrete delivery | Completion evidence |
|---|---|---|
| P1 | Repair evidence for the 28 existing entries | Exact adopted spec revision/section, delivery PR/revision, named tests, named conformance sets and actual gate status; retain explicit gaps. |
| P1 | Add the missing decision records | Cover the moving/rejected matrix, with alternatives and refusal evidence; resolve historical deferrals superseded by shipped features. |
| P2 | Add sequence and higher-order function chapters/entries | Real document cases, normal/edge inputs and results, complete code, earlier-version comparison where meaningful, exact compatibility refusals. |
| P2 | Add map/array, Unicode, URI/date, operator/type families | All 117 names are assigned. Audit supplementary arities, options and changed signatures; name assignment alone is not acceptance. |
| P2 | Expand syntax and changed-3.1 semantics | Literals, focus functions, guarded expressions, record updates, comparisons/collations, patterns, and supplementary forms. |
| P3 | Reconcile acceptance and publish the complete reference | Zero unassigned adopted features/decisions, accurate index/tier links, verified examples, and explicit exceptions to conformance evidence rules. |

**Recommended next step:** audit supplementary signatures/options and grammar/
parser/type/pattern forms against acceptance criteria, then reconcile disposition
provenance. Track computed-key boundary ties and approximate Unicode grapheme
segmentation as explicit engine/specification follow-ups. Registered-name
assignment is complete; REQ-126 remains In Progress.
The baseline provenance repair, section-1.2 decision records and first sequence/HOF
routing chapter are supplied below. Immutable historical specification snapshots
remain an explicit gap. No engine work or conformance-set promotion is required
for the next documentation slice.

For XPath entries, distinguish named qt4 sets from unit/example checks. XSLT-only
instructions need XSLT test evidence; do not invent qt4 expression sets for them.
Record the applicable harness and any unavailable 4.0 conformance evidence
explicitly when reconciling AC-03.

## Audit method and verification

1. Read the five REQ-126 criteria, REQ-118 section-1.2 decisions and S0-S8 plan,
   post-1.0 delivery history, 28 entries, and published example manifest.
2. Reflect the two public function-name sets from the published 1.1.0 assembly;
   classify namespace/local-name pairs into existing catalog families.
3. Compare named gate identifiers with the actual `GatedSets` initializer in
   [ConformanceRunner40](../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).
4. Check entry field counts, index membership, snapshot totals, manifest files,
   local links, and rendered Markdown. Retain executable fixtures unchanged.

Audit findings are documentation coverage assessments, not new claims of engine
conformance. The last published full solution/gate results remain in the
[patch release notes](../RELEASE-NOTES-1.1.1.md); this audit does not rerun those engines.

## Historical progress before the map/array/selection delivery: 2026-10-11

PR #155 is merged at `17f9cc7c`. The follow-up supplies 38 entries, 104 manifest
cases and four worked chapters with 24 chapter checks: 128 documentation checks.
The [provenance ledger](PROVENANCE.md) records exact engine/dossier commits and
named tests for all 28 baseline entries, while preserving the historical adopted
specification snapshot gap. Three new function families cover six additional
registered names, reducing unassigned names from 55 to 49. This remains a family
mapping, not complete signature coverage or a new conformance promotion.

The [XPath switch](rejected/xpath-switch-expression.md) and
[extensible-map proposal](rejected/extensible-map-types.md) now have explicit
disposition records. Record casts discard surplus entries; coercion rejects them.
The REQ-118 historical shorthand must not override delivered record tests.

The [routing chapter](worked-examples/order-routing/README.md) also exposes a
runtime integration gap: `version="4.0"` alone does not select XPath40 for
transformation-time expression compilation. It uses the public explicitly opted-in
XPath API for planning and passes its map to portable XSLT. Automatic runtime
compatibility propagation requires a separately tested engine slice; it is not
implemented or certified by this documentation work.

Next priorities are the remaining sequence-selection families, supplementary
signatures and parser/type/pattern coverage. Future engine slices must record an
immutable adopted specification revision at delivery time. REQ-126 stays In Progress.

## Runtime compatibility correction (source 1.1.1, 2026-10-11)

The [separately tested engine correction](../REQ-118-XSLT-XPATH40-COMPATIBILITY.md)
resolves the runtime propagation gap discovered by routing. Effective-version
4.0 expressions select frozen XPath40; older elements retain XPath31 and nested
evaluations restore their caller's profile. Static compilation, AVTs, dynamic
evaluation and module boundaries are covered. The routing chapter now also has a
self-contained stylesheet with five additional checks; the original two-stage
workflow remains the published-1.1.0 alternative. The correction is published in 1.1.1. At that delivery the missing-name count
remained 49; the later map/array/selection delivery below reduces it to 35.
REQ-126 remains In Progress.

The correction records an immutable QT4CG specification source revision at
delivery. This establishes its own reviewed source and does not fill historical
snapshot gaps in the 28 baseline ledger rows.

## Map/array and sequence-selection delivery: 2026-10-11

Three new entries supply 40 exact manifest checks: [product-map indexing](functions/product-map-index.md)
(15), [array import batches](functions/import-array-batches.md) (11), and
[order-export selection](functions/order-sequence-selection.md) (14).
The catalog now has **41 entries, 144 feature checks and 24 chapter checks
(168 total)**. Fourteen names gain family assignments: **82 assigned / 35 unassigned
of 117**. All 16 fields, named unit tests and actual exact-name gate membership
are supplied. Historical adopted-specification snapshots remain unavailable;
these entries record a new review pin instead:
[QT4CG source a014e1b4](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xpath-functions-40/src/function-catalog.xml).

### Confirmed engine follow-up: map result shapes

In public/source 1.1.1, `map:entries` returns one array item containing single-entry
maps; the reviewed draft specifies a sequence of single-entry maps. Its named
existing unit test already expects array indexing. The new main example uses an
explicit Bosak adapter (`?*`); `entries-boundary` and `entries-baseline` distinguish
the shapes. `map:items` also preserves a grouped multi-item value: the new
`items-sequence-boundary` counts two values while the draft-equivalent 3.1
`map:for-each` baseline counts three items. Existing named unit evidence likewise
expects the grouped result. See [the entry’s consequences](functions/product-map-index.md#11-consequences).

Correct both contracts in a separate REQ-118 bug-fix slice, updating the existing
tests and these probes deliberately. Include ordinary/dynamic calls, empty maps,
multi-item and nested map/array values, entry order, default-3.1 refusal and
caller-profile restoration; run full solution, frozen QT3 and qt4 gates before
release. This catalog assignment is not a completeness/conformance pass.

The current delivery changes documentation and fixtures only. No engine code,
package version or gate membership changes. REQ-126 remains In Progress.

**Delivery verification (2026-10-11):** Release build 0 warnings / 0 errors; full isolated solution **4,143/0/0**, targeted catalog **144/0/0**. All 41 entries/index links, 166 primary links/anchors, 28 baseline provenance records and 144 manifest files verified; 54 documentation pages rendered. Inline expressions/results and named tests match. Engine gates were not rerun for this documentation-only change.

## Map contract correction (source 1.1.2): 2026-10-11

The [REQ-118 engine correction](../REQ-118-MAP-RESULT-SHAPES.md) addresses both
result-shape findings above. `map:entries` returns a sequence of single-entry
maps; `map:items` concatenates value sequences while preserving nested containers.
The product-index main expression no longer uses the Bosak-specific `?*` adapter;
boundary probes now agree with the portable baselines. Twenty-one new standard
checks and two nested-stylesheet profile checks cover direct/dynamic calls,
empty and nested values, order and default-mode isolation. The dossier records
full regression and frozen gate evidence. Map upstream sets remain exploratory;
no sets are promoted. Version 1.1.2 is published and its public artifacts verified. Counts remain
41 entries, 168 documentation checks and 35 unassigned names; REQ-126 stays In Progress.

**Map correction final verification (2026-10-11):** 1.1.2 published and verified; full solution 4,166/0/0, QT3 31,142/0/679, qt4 promoted sets 8,278/0/1,894 with zero gated failures. Targeted map-entries 16/0/1 and map-items 15/0/0 remain exploratory. See [the correction dossier](../REQ-118-MAP-RESULT-SHAPES.md) and [patch release notes](../RELEASE-NOTES-1.1.2.md).

**1.1.2 publication (2026-10-11):** [v1.1.2](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.1.2) at `3aebf76a24b28af7d40aa42e6eaf714b3a596838`; all ten NuGet packages downloaded and verified, eight fresh-cache consumer checks passed. REQ-126 remains In Progress with 35 unassigned names.

## Shipment-event sequence delivery: 2026-10-11

[The new family](functions/shipment-event-sequences.md) assigns fn:all-equal,
fn:contains-subsequence, fn:starts-with-subsequence and fn:ends-with-subsequence.
Coverage is now **42 entries, 171 feature checks and 24 worked-chapter checks
(195 total)**; **86 assigned / 31 unassigned** of 117 registered names.
The 27 added cases include full XML histories and reports, a XPath31 comparison,
missing/mixed identities, missing/unknown codes, unfinished histories,
noncontiguous/repeated events, empty input/requirements, callback forms/errors,
collation and numeric/NaN equality contracts. All-equal alone is explicitly
insufficient to validate an empty history or omitted identity fields.

Delivery PR #83 and its immutable engine revision, named existing unit tests,
the current exact gate assignment and a pinned review specification source are
recorded. Only fn-contains-subsequence has an exact assigned GatedSets identifier;
the other three functions retain separate unit/example evidence. Historical
adopted snapshots remain unavailable. No new gate promotion or engine/package
change is included. REQ-126 stays In Progress; higher-order processing remains
the next family delivery.

**Shipment delivery verification (2026-10-11):** full solution **4,193/0/0**, targeted catalog **171/0/0**; Release build 0 errors / 12 existing warnings. Verified 42 entry structures/index links, 170 primary links/anchors, 28 baseline provenance rows, 117 inventory names, 171 manifest files and all new inline sample/fixture results. Rendered 55 documentation pages; whitespace checks pass. No new engine gate run or promotion is claimed.

[Ordered queue prefix](functions/ordered-queue-prefix.md) supplies take-while/drop-while with complete input/output, an XPath 3.1 comparison and 18 boundary/callback cases. Current coverage: 61 entries, 561 feature checks plus 24 chapter checks (585 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public version remains 1.1.2. Bounded loop processing is now supplied by the reference below; dependency traversal and partial application are now supplied below.

[Bounded retry state](functions/bounded-retries.md) documents while-do/do-until using invoice-submission response fixtures, complete input/output, a bounded XPath 3.1 fold, and 25 executable normal/edge/error cases. It explains precondition versus postcondition evaluation, explicit attempt limits, zero-budget guarding, and whole-state callback values. Current coverage is 61 entries and 585 documentation checks (561 feature checks plus 24 chapter checks); 117 of 117 registered names have entries, leaving 0. REQ-126 remains In Progress; source/public version stays 1.1.2. Dependency traversal and partial application are supplied by this delivery.

[Product dependency closure](functions/product-dependency-closure.md) and [configured label callbacks](functions/configured-label-callbacks.md) add two complete references for transitive-closure, partial-apply and identity, with full input/output, XPath 3.1 comparisons and 37 executable cases. Shared dependencies, cycles, node identity/document order, unresolved references, argument order and callback binding limits are explicit. Current coverage: 61 entries, 561 feature checks plus 24 chapter checks (585 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public version stays 1.1.2.

[Month-end cutoffs](functions/month-end-cutoffs.md) and [timestamp deadlines](functions/timestamp-deadlines.md) add full scheduling references for build-dateTime, days-in-month, unix-dateTime, seconds and duration-to-seconds. Complete inputs/reports, XPath 3.1 comparisons and 36 executable cases explain Gregorian leap rules, numeric timezone offsets, stamp types, epoch milliseconds, signed/fractional durations and explicit budget boundaries. Coverage is 61 entries, 561 feature checks plus 24 chapter checks (585 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public stays 1.1.2. URI parsing/construction and encoding are supplied by the reference below.

[Invoice API URIs](functions/invoice-api-uris.md) supplies parse-uri, build-uri and decode-from-uri with full request input/output, a scoped XPath 3.1 construction comparison and 28 executable cases. It explains segment boundaries, query form decoding versus literal plus, repeated keys, permissive malformed-input decoding, component precedence and textual roundtrip limits. Coverage: 61 entries, 561 feature checks plus 24 chapter checks (585 total), and 117 assigned / 0 unassigned names. REQ-126 remains In Progress; source/public stays 1.1.2. Unicode/text cleanup and substring references are supplied below.

[Customer text cleanup](functions/customer-text-cleanup.md) and [import file names](functions/import-file-names.md) add eight function names with full input/output, scoped XPath 3.1 comparisons and 35 executable cases. They distinguish codepoints from grapheme clusters and display width, preserve interior whitespace, and explain final-delimiter/overlap rules. Bosak grapheme segmentation is approximate; complete Unicode segmentation remains outside the verified coverage. Current coverage: 61 entries, 561 feature checks plus 24 chapter checks (585 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public stays 1.1.2. Ranking, numeric diagnostics and utility references are supplied below.

Text definitions were reviewed on 2026-10-12 against immutable QT4CG `e7ee4ab8`; all eight are unchanged from the earlier `a014e1b4` pin. The inventory-wide reviewed specification pin retains its original scope. The source grapheme algorithm does not distinguish all Hangul, Prepend, regional-indicator and Indic/extended-pictographic classes; the documented combining/ZWJ tests do not certify those omitted rules. No engine correction is included in this reference delivery.

[Supplier offer ranking](functions/supplier-offer-ranking.md), [numeric import diagnostics](functions/numeric-import-diagnostics.md) and [payload fingerprints](functions/payload-fingerprints.md) add the final eight registered function names with 41 executable cases. Current coverage: 61 entries, 561 feature checks plus 24 chapter checks (585 total); all 117 registered names have family entries. This completes name assignment, not REQ-126 acceptance: signatures/options, grammar/disposition provenance and conformance limits remain. Computed-key boundary ties require explicit filtering in the business example and follow-up specification/engine reconciliation. Source/public stays 1.1.2; REQ-126 remains In Progress.

## Ranking and utility review: 2026-10-12

All 117 registered names now map to entries. Supplier ranking records observed full-item boundary matching: lowest returns only B for distinct B/C nodes sharing price 8. The business report explicitly filters all equal-price offers. Pinned draft contextual-equality rules and computed-key examples require reconciliation before broader tie semantics are certified. The observed fixture is implementation evidence, not a conformance promotion. NaN screening is not financial/range validation, type reflection is not schema validation, and hash options/algorithms remain partially covered. The independent SHA-256 reference uses the same declared UTF-8 payload. No engine/package/gate change is included.

## Supplementary review: 2026-10-12

The [supplementary coverage audit](SUPPLEMENTARY-COVERAGE-AUDIT.md) records 68 non-constructor name registration arities, identifies changed `fn:compare` contracts outside the name inventory, and enumerates wider deferred/out-of-scope dossier decisions. Registration metadata is not full signature evidence. All acceptance statuses remain unchanged. The 1.1.2 map result-shape correction is delivered; key-tie semantics and grapheme segmentation remain separate follow-ups. Typed measurement comparison is supplied below; min/max references are now supplied; sorting references are now supplied; URI option contracts and component precedence are now supplied; next: enclosing-mode disposition.

[Typed measurement comparison](functions/measurement-comparison.md) adds 27 executable cases for changed `fn:compare` behavior, with complete input/output and an explicit 3.1 double-normalization policy that differs at a decimal boundary. The runner verifies declared default-mode results or evaluation errors for existing functions. Keyword drift (`comparand1`/`comparand2` versus reviewed `value1`/`value2`) and legacy binary/empty-collation behavior are explicit. Current coverage: 61 entries, 561 feature plus 24 chapter checks (585 total); the 117-name assignment is unchanged. REQ-126 remains In Progress; source/public stays 1.1.2. Min/max references are now supplied; sorting references are now supplied; URI option contracts and component precedence are now supplied; next: enclosing-mode disposition.

**Measurement reference delivery verification (2026-10-12):** Validation: Release build passed (one existing warning, zero errors); all 4,440 tests across ten assemblies passed without failures/skips, including 418 catalog cases. Renderer verified 69 pages. Entry fields/index, local/primary anchors, inventory/provenance, named tests, complete XML inputs and inline report/results verified; numeric boundary values independently checked with Python Fraction/float32. No engine conformance rerun for this documentation/test-harness change.

[Batch extrema and component codes](functions/batch-extrema.md) adds 29 executable cases for `fn:min`/`fn:max`: complete batch input/output, explicit decimal normalization, namespace/local-name QName boundaries and a 3.1 fold comparison. Mixed numeric promotion/result-type and plain-duration limits remain explicit observed gaps against the pinned draft; these fixtures do not claim conformance. Current coverage: 61 entries, 561 feature plus 24 chapter checks (585 total); all 117 registered names remain assigned. REQ-126 remains In Progress; source/public stays 1.1.2. Sorting references are now supplied; URI option contracts and component precedence are now supplied; next: enclosing-mode disposition.


Min/max follow-up findings: QName ordering is supplied, but shared numeric promotion/result-type conversion and plain-duration refusal differ from the reviewed draft. Empty-collation acceptance in 3.1 remains an observed legacy boundary. No exact fn-min/fn-max gate assignment or engine correction is claimed.

**Batch extrema delivery verification (2026-10-12):** Validation: Release build passed (one existing warning, zero errors); all 4,469 tests across ten assemblies passed without failures/skips, including 447 catalog cases. Renderer verified 70 pages. Entry fields/index, primary/local links, inventory/provenance, named tests, full XML and inline reports/results verified; batch decimal range and QName endpoints independently computed with Python. No engine conformance rerun for this documentation-only delivery.

[Warehouse pick sorting](functions/warehouse-pick-sorting.md) adds 28 cases for retained `fn:sort`/`array:sort`: complete input/output, a portable 3.1 comparison, stable composite location keys and preserved multi-item array members. Mixed numeric promotion, input-dependent NaN order and missing empty-key defaults and unknown-collation acceptance are explicit observed boundaries. Exact fn-sort/array-sort gate membership is recorded without claiming full conformance. Coverage: 61 entries, 561 feature plus 24 chapter checks (585 total); all 117 names remain assigned. REQ-126 stays In Progress; source/public remains 1.1.2. URI option policy is now supplied; next: enclosing-mode disposition.


Sorting review: retained functions still use the legacy shared comparer rather than the pinned exact numeric key rules. Mixed decimal/double keys retain input ties, and two-item NaN ordering is input-dependent. These documentation fixtures preserve observed limits; no engine correction or gate promotion is claimed.

**Warehouse sorting delivery verification (2026-10-12):** Validation: Release build passed (one existing warning, zero errors); all 4,497 tests across ten assemblies passed without failures/skips, including 475 catalog cases. Renderer verified 71 pages. Entry fields/index, primary/local links, inventory/provenance, named tests, full XML and inline reports/results verified; pick order/payloads and numeric boundary independently computed with Python. No engine conformance rerun for this documentation-only delivery.

**Historical session close (2026-10-12, before URI review):** PR #173 merged after CI passed. Current reference coverage is 57 entries and 499 documentation checks (475 feature plus 24 chapter); all 117 registered names are assigned. Latest implementation validation: 4,497 tests passed across ten assemblies. REQ-126 remains In Progress; released/source version remains 1.1.2. Resume with URI options and component precedence; see [the handover](../AGENT_HANDOVER.md).


**URI option review — 2026-10-12:** [Archive and partner URI policy](functions/uri-option-policy.md) adds complete input/output, a scoped 3.1 comparison and 28 executable cases. Boolean options, UNC spelling, raw/structured authority and path/query precedence are explicit. Non-Boolean option truthiness and empty-query-map fallback remain observed draft gaps. Coverage: 61 entries, 561 feature plus 24 chapter checks (585 total); all 117 names remain assigned. REQ-126 stays In Progress; source/public version remains 1.1.2. Next: enclosing-mode disposition.

**URI review verification (2026-10-12):** Release build passed (one existing warning, zero errors); 4,525 tests passed across ten assemblies without failures/skips; 503 focused catalog cases passed. Renderer verified 72 pages. Entry/index, source/local links, historical provenance scope, fixture code/results and eight named tests verified. Manual engine conformance gates were not rerun; gate membership is unchanged.

**Computed XPath constructor disposition — 2026-10-12:** [Deferred node construction](deferred/computed-node-constructors.md) separates the draft XPath feature from Bosak XQuery 3.1 support. Complete invoice input, refused XPath, 3.1 report and XSLT receipt output accompany 30 executable checks: 25 refusals across compatibility modes, three reports and two XML outputs. Current coverage: 61 entries, 561 feature plus 24 chapter checks (585 total); all 117 names remain assigned. Version stays 1.1.2; REQ-126 remains In Progress. Named XSLT declaration dispositions are now supplied; next: enclosing modes.

**Constructor disposition verification (2026-10-12):** Release build passed (one existing warning, zero errors); all 4,555 tests across ten assemblies passed without failures/skips, including 533 catalog cases. Renderer verified 73 pages; entry/index, source/local links, scoped provenance/inventories, all inline inputs/code/results, ten XQuery named tests and independently calculated receipt projections verified. Manual engine gates were not rerun; no membership changed.

**Named XSLT declaration review — 2026-10-12:** [xsl:item-type](deferred/xslt-item-type.md) and [xsl:record-type](deferred/xslt-record-type.md) add full invoice cases, pinned disposition evidence and 28 executable boundary/alternative checks. Unused 4.0 declarations are ignored, not implemented; named bindings and constructors fail. Portable map validation and inline enums are verified. Structural-record XSLT `@as` raises XTTE0570; explicit XPath cast in `select` plus `as="map(*)"` is the verified workaround. Catalog: 61 entries, 561 feature plus 24 chapter checks (585 total); all 117 names assigned. REQ-126 remains In Progress, version 1.1.2. Next: enclosing-mode disposition.

**Named declaration verification (2026-10-12):** Release build passed (one existing warning, zero errors); all 4,583 tests across ten assemblies passed without failures/skips, including 561 catalog checks. Renderer verified 75 pages. Entry/index, source/local links, scoped inventories/provenance, all inline inputs/code/results, nine named tests, XML validity and independent portable reports verified. Manual engine gates were not rerun for the documentation/test-harness slice; membership is unchanged.
