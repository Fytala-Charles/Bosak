<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak REQ-126 catalog completeness audit">
  <br><br><h1>REQ-126 Catalog Completeness Audit</h1>
  <p>Evidence, missing coverage, and the next documentation deliveries</p>
</div>

## Finding and scope

**REQ-126 is not ready to close.** Four worked chapters are supplied, but
registered names and reviewed section-1.2/wider dispositions now have records.
Constructor/signature/grammar coverage, feature-specific drift and acceptance
evidence remain partial; see the [close-out review](CLOSEOUT-REVIEW.md). The original audit was dated 2026-10-11 against source revision
`09e678f3` (1.1.1 publication documentation); published engine 1.1.1 is revision
`d35cf1d4bd1d1b7143ccb9ab304812f781dc0d50`.

Current review: **2026-10-12**, source baseline `9f656df3edf0a162226c996fbeb939da09c175a6`, public engine **1.1.2** (`3aebf76a`). All **117 registered namespace/local-name pairs** now have entries. This is a name-inventory milestone, not proof of exhaustive signatures, grammar, dispositions or conformance.

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
| AC-02: deferred/rejected decisions | Partial | Method-call and renamed mapping-arrow entries exist; dedicated records now cover the missing section 1.2 decisions, including shipped collection-sequence iteration and the partial JNode boundary. The wider disposition records are also supplied; source-to-disposition and lifecycle coverage still need systematic validation. |
| AC-03: verified examples and gate evidence | Partial | 2,602 executable documentation checks pass. Baseline entries now carry named implementation evidence and explicit assigned-set or unassigned status. The original audit corrected fn:scan gate status. AC-03 literally asks for qt4tests sets for every entry; XSLT-only and refused/out-of-scope entries require applicable harness evidence and explicit absent-gate status. This does not waive the criterion. |
| AC-04: structure, retained entries, drift warnings | Partial | 67 entries have all numbered fields and index links. No entry is removed by this audit. Generic prior-art disclaimers do not replace feature-specific drift explanations; supplementary forms and historical lifecycle need review. |
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
| Frozen zero-argument `xs:*` constructors | 49 | All 49 names now have representative lexical/type/context/error and 3.1/experimental checks, plus a typed delivery report. This is not exhaustive casting conformance. |
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
| External-input functions | [Five-function scope record](deferred/external-input-functions.md) | Absence verified across compatibility modes; alternatives do not certify missing format/acquisition/schema factory contracts. |
| Serialization 4.0 additions | [Parameter disposition](deferred/serialization-parameters.md) | Nine additions reviewed; XSLT solidus control is distinct from deferred JSON Lines/canonical/layout/CSV behavior. |
| fn:apply-templates | [Deferred function record](deferred/fn-apply-templates.md) | Missing function registration; verified instruction/wrapper alternatives do not certify dynamic draft options or public-mode validation. |
| Capturing accumulators | [Deferred capture record](deferred/capturing-accumulators.md) | Source-node identity exposes ignored capture; in-memory snapshot/projection alternatives are separate from streaming capture semantics. |
| Enclosing modes | [Deferred enclosing-mode record](deferred/enclosing-modes.md) | Generic empty-element refusal; named-mode export/audit alternatives and lexical/dynamic dispatch differences verified. |
| XSLT named item/record declarations | [Item-type record](deferred/xslt-item-type.md), [record-type record](deferred/xslt-record-type.md) | Unused declaration tolerance is separate from named type/constructor support; portable validation and structural cast alternatives supplied. |
| Computed XPath node constructors | [Deferred constructor record](deferred/computed-node-constructors.md) | Braced-name draft syntax refused in all three XPath modes; existing XQuery evidence and complete XSLT alternative are separate. |
| Method-call `=?>` | Dedicated Deferred entry | Preserve the churn rationale and exact refused shape. |
| Nominative record types | [Deferred record](type-system/nominative-records.md) | Kept distinct from shipped structural record contracts. |
| Iteration over sequences of arrays/maps | [Shipped S4 extension](operators-and-syntax/sequence-collection-iteration.md) | Named tests and three published examples pin collection boundaries and invalid input. |
| fn:scan | Experimental entry | Historical deferral is superseded by experimental adoption; gate status corrected here. |
| JNode built-in template rules and full GNode/JNode model | [Partial-model boundary](data-model/jnode-template-boundary.md) | Navigation shipped; eighteen added checks confirm invoice reports and node()/xnode()/identity gaps. Full model and JNode-specific XSLT built-in behavior are not certified. |
| `fn:all` / `fn:some-of` | [Rejected names](rejected/obsolete-quantifier-functions.md) | Exact XPST0017 refusals and accepted predicate alternative supplied. |
| `scan-left` / `scan-right` | [Superseded names](rejected/directional-scan.md) | Refused names and left-fold/array-state semantics recorded. |
| Old mapping arrow `=>>` | Dedicated rejected entry | Retain renamed disposition and link to `=!>`. |
| XPath switch expression | [XPath rejection](rejected/xpath-switch-expression.md) | Exact grammar refusal plus a 3.1 conditional alternative; XQuery and XSLT remain separate. |
| Extensible map types | [Proposal disposition](rejected/extensible-map-types.md) | Supplied cast/coercion examples pin surplus-field behavior; maps themselves remain supported. |

The section-1.2 and wider disposition records listed above are supplied. Their recorded deferred/out-of-scope implementations remain separate from documentary completion. Source-to-record coverage and historical lifecycle evidence still require validation.

## Prioritized deliveries

The [close-out review](CLOSEOUT-REVIEW.md) defines the current queue and completion evidence:

| Priority | Concrete delivery | Completion evidence |
|---|---|---|
| D1 — supplied | [All 49 focus constructors](operators-and-syntax/focus-functions.md) | 508 new checks and detailed delivery-import README; representative values/types, invalid sources, missing context, empty/multiple input and explicit modes. |
| D2 | Complete signatures and changed existing functions | Reconcile 68 non-constructor registrations, keywords/defaults/options and changed 3.1 contracts against pinned sources; verify uncovered forms. |
| D3 | Build grammar/type/pattern inventory | Map delivered productions and version gates to entries, parser/compiler tests and realistic fixtures. |
| D4 | Reconcile provenance, drift and acceptance | Validate decision lifecycle, retain 28 unavailable historical adopted pins, and resolve the literal AC-03 gate requirement without invented evidence. |

Registered-name assignment and disposition authoring are delivered milestones. REQ-126 remains In Progress. Engine gaps are tracked separately in the review; this queue does not authorize deferred feature implementation.

## Historical audit method and current review

The original method below describes the initial audit. The 2026-10-12 close-out review reconciles the current 67 entries, 778 feature checks, 24 chapter checks and supplied disposition records; inventory scopes remain unchanged.

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

[Ordered queue prefix](functions/ordered-queue-prefix.md) supplies take-while/drop-while with complete input/output, an XPath 3.1 comparison and 18 boundary/callback cases. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public version remains 1.1.2. Bounded loop processing is now supplied by the reference below; dependency traversal and partial application are now supplied below.

[Bounded retry state](functions/bounded-retries.md) documents while-do/do-until using invoice-submission response fixtures, complete input/output, a bounded XPath 3.1 fold, and 25 executable normal/edge/error cases. It explains precondition versus postcondition evaluation, explicit attempt limits, zero-budget guarding, and whole-state callback values. Current coverage is 67 entries and 802 documentation checks (778 feature checks plus 24 chapter checks); 117 of 117 registered names have entries, leaving 0. REQ-126 remains In Progress; source/public version stays 1.1.2. Dependency traversal and partial application are supplied by this delivery.

[Product dependency closure](functions/product-dependency-closure.md) and [configured label callbacks](functions/configured-label-callbacks.md) add two complete references for transitive-closure, partial-apply and identity, with full input/output, XPath 3.1 comparisons and 37 executable cases. Shared dependencies, cycles, node identity/document order, unresolved references, argument order and callback binding limits are explicit. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public version stays 1.1.2.

[Month-end cutoffs](functions/month-end-cutoffs.md) and [timestamp deadlines](functions/timestamp-deadlines.md) add full scheduling references for build-dateTime, days-in-month, unix-dateTime, seconds and duration-to-seconds. Complete inputs/reports, XPath 3.1 comparisons and 36 executable cases explain Gregorian leap rules, numeric timezone offsets, stamp types, epoch milliseconds, signed/fractional durations and explicit budget boundaries. Coverage is 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public stays 1.1.2. URI parsing/construction and encoding are supplied by the reference below.

[Invoice API URIs](functions/invoice-api-uris.md) supplies parse-uri, build-uri and decode-from-uri with full request input/output, a scoped XPath 3.1 construction comparison and 28 executable cases. It explains segment boundaries, query form decoding versus literal plus, repeated keys, permissive malformed-input decoding, component precedence and textual roundtrip limits. Coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned names. REQ-126 remains In Progress; source/public stays 1.1.2. Unicode/text cleanup and substring references are supplied below.

[Customer text cleanup](functions/customer-text-cleanup.md) and [import file names](functions/import-file-names.md) add eight function names with full input/output, scoped XPath 3.1 comparisons and 35 executable cases. They distinguish codepoints from grapheme clusters and display width, preserve interior whitespace, and explain final-delimiter/overlap rules. Bosak grapheme segmentation is approximate; complete Unicode segmentation remains outside the verified coverage. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public stays 1.1.2. Ranking, numeric diagnostics and utility references are supplied below.

Text definitions were reviewed on 2026-10-12 against immutable QT4CG `e7ee4ab8`; all eight are unchanged from the earlier `a014e1b4` pin. The inventory-wide reviewed specification pin retains its original scope. The source grapheme algorithm does not distinguish all Hangul, Prepend, regional-indicator and Indic/extended-pictographic classes; the documented combining/ZWJ tests do not certify those omitted rules. No engine correction is included in this reference delivery.

[Supplier offer ranking](functions/supplier-offer-ranking.md), [numeric import diagnostics](functions/numeric-import-diagnostics.md) and [payload fingerprints](functions/payload-fingerprints.md) add the final eight registered function names with 41 executable cases. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total); all 117 registered names have family entries. This completes name assignment, not REQ-126 acceptance: signatures/options, grammar/disposition provenance and conformance limits remain. Computed-key boundary ties require explicit filtering in the business example and follow-up specification/engine reconciliation. Source/public stays 1.1.2; REQ-126 remains In Progress.

## Ranking and utility review: 2026-10-12

All 117 registered names now map to entries. Supplier ranking records observed full-item boundary matching: lowest returns only B for distinct B/C nodes sharing price 8. The business report explicitly filters all equal-price offers. Pinned draft contextual-equality rules and computed-key examples require reconciliation before broader tie semantics are certified. The observed fixture is implementation evidence, not a conformance promotion. NaN screening is not financial/range validation, type reflection is not schema validation, and hash options/algorithms remain partially covered. The independent SHA-256 reference uses the same declared UTF-8 payload. No engine/package/gate change is included.

## Supplementary review: 2026-10-12

The [supplementary coverage audit](SUPPLEMENTARY-COVERAGE-AUDIT.md) records 68 non-constructor name registration arities, identifies changed `fn:compare` contracts outside the name inventory, and enumerates wider deferred/out-of-scope dossier decisions. Registration metadata is not full signature evidence. All acceptance statuses remain unchanged. The 1.1.2 map result-shape correction is delivered; key-tie semantics and grapheme segmentation remain separate follow-ups. Typed measurement comparison is supplied below; min/max references are now supplied; sorting references are now supplied; URI option contracts and component precedence are now supplied; next: catalog close-out and prioritized follow-up review.

[Typed measurement comparison](functions/measurement-comparison.md) adds 27 executable cases for changed `fn:compare` behavior, with complete input/output and an explicit 3.1 double-normalization policy that differs at a decimal boundary. The runner verifies declared default-mode results or evaluation errors for existing functions. Keyword drift (`comparand1`/`comparand2` versus reviewed `value1`/`value2`) and legacy binary/empty-collation behavior are explicit. Current coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); the 117-name assignment is unchanged. REQ-126 remains In Progress; source/public stays 1.1.2. Min/max references are now supplied; sorting references are now supplied; URI option contracts and component precedence are now supplied; next: catalog close-out and prioritized follow-up review.

**Measurement reference delivery verification (2026-10-12):** Validation: Release build passed (one existing warning, zero errors); all 4,440 tests across ten assemblies passed without failures/skips, including 418 catalog cases. Renderer verified 69 pages. Entry fields/index, local/primary anchors, inventory/provenance, named tests, complete XML inputs and inline report/results verified; numeric boundary values independently checked with Python Fraction/float32. No engine conformance rerun for this documentation/test-harness change.

[Batch extrema and component codes](functions/batch-extrema.md) adds 29 executable cases for `fn:min`/`fn:max`: complete batch input/output, explicit decimal normalization, namespace/local-name QName boundaries and a 3.1 fold comparison. Mixed numeric promotion/result-type and plain-duration limits remain explicit observed gaps against the pinned draft; these fixtures do not claim conformance. Current coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 registered names remain assigned. REQ-126 remains In Progress; source/public stays 1.1.2. Sorting references are now supplied; URI option contracts and component precedence are now supplied; next: catalog close-out and prioritized follow-up review.


Min/max follow-up findings: QName ordering is supplied, but shared numeric promotion/result-type conversion and plain-duration refusal differ from the reviewed draft. Empty-collation acceptance in 3.1 remains an observed legacy boundary. No exact fn-min/fn-max gate assignment or engine correction is claimed.

**Batch extrema delivery verification (2026-10-12):** Validation: Release build passed (one existing warning, zero errors); all 4,469 tests across ten assemblies passed without failures/skips, including 447 catalog cases. Renderer verified 70 pages. Entry fields/index, primary/local links, inventory/provenance, named tests, full XML and inline reports/results verified; batch decimal range and QName endpoints independently computed with Python. No engine conformance rerun for this documentation-only delivery.

[Warehouse pick sorting](functions/warehouse-pick-sorting.md) adds 28 cases for retained `fn:sort`/`array:sort`: complete input/output, a portable 3.1 comparison, stable composite location keys and preserved multi-item array members. Mixed numeric promotion, input-dependent NaN order and missing empty-key defaults and unknown-collation acceptance are explicit observed boundaries. Exact fn-sort/array-sort gate membership is recorded without claiming full conformance. Coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. REQ-126 stays In Progress; source/public remains 1.1.2. URI option policy is now supplied; next: catalog close-out and prioritized follow-up review.


Sorting review: retained functions still use the legacy shared comparer rather than the pinned exact numeric key rules. Mixed decimal/double keys retain input ties, and two-item NaN ordering is input-dependent. These documentation fixtures preserve observed limits; no engine correction or gate promotion is claimed.

**Warehouse sorting delivery verification (2026-10-12):** Validation: Release build passed (one existing warning, zero errors); all 4,497 tests across ten assemblies passed without failures/skips, including 475 catalog cases. Renderer verified 71 pages. Entry fields/index, primary/local links, inventory/provenance, named tests, full XML and inline reports/results verified; pick order/payloads and numeric boundary independently computed with Python. No engine conformance rerun for this documentation-only delivery.

**Historical session close (2026-10-12, before URI review):** PR #173 merged after CI passed. Current reference coverage is 57 entries and 499 documentation checks (475 feature plus 24 chapter); all 117 registered names are assigned. Latest implementation validation: 4,497 tests passed across ten assemblies. REQ-126 remains In Progress; released/source version remains 1.1.2. Resume with URI options and component precedence; see [the handover](../AGENT_HANDOVER.md).


**URI option review — 2026-10-12:** [Archive and partner URI policy](functions/uri-option-policy.md) adds complete input/output, a scoped 3.1 comparison and 28 executable cases. Boolean options, UNC spelling, raw/structured authority and path/query precedence are explicit. Non-Boolean option truthiness and empty-query-map fallback remain observed draft gaps. Coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. REQ-126 stays In Progress; source/public version remains 1.1.2. Next: catalog close-out and prioritized follow-up review.

**URI review verification (2026-10-12):** Release build passed (one existing warning, zero errors); 4,525 tests passed across ten assemblies without failures/skips; 503 focused catalog cases passed. Renderer verified 72 pages. Entry/index, source/local links, historical provenance scope, fixture code/results and eight named tests verified. Manual engine conformance gates were not rerun; gate membership is unchanged.

**Computed XPath constructor disposition — 2026-10-12:** [Deferred node construction](deferred/computed-node-constructors.md) separates the draft XPath feature from Bosak XQuery 3.1 support. Complete invoice input, refused XPath, 3.1 report and XSLT receipt output accompany 30 executable checks: 25 refusals across compatibility modes, three reports and two XML outputs. Current coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. Version stays 1.1.2; REQ-126 remains In Progress. Named XSLT declaration dispositions are now supplied; enclosing modes are now supplied; capturing accumulator disposition is now supplied; fn:apply-templates disposition is now supplied; next: catalog close-out and prioritized follow-up review.

**Constructor disposition verification (2026-10-12):** Release build passed (one existing warning, zero errors); all 4,555 tests across ten assemblies passed without failures/skips, including 533 catalog cases. Renderer verified 73 pages; entry/index, source/local links, scoped provenance/inventories, all inline inputs/code/results, ten XQuery named tests and independently calculated receipt projections verified. Manual engine gates were not rerun; no membership changed.

**Named XSLT declaration review — 2026-10-12:** [xsl:item-type](deferred/xslt-item-type.md) and [xsl:record-type](deferred/xslt-record-type.md) add full invoice cases, pinned disposition evidence and 28 executable boundary/alternative checks. Unused 4.0 declarations are ignored, not implemented; named bindings and constructors fail. Portable map validation and inline enums are verified. Structural-record XSLT `@as` raises XTTE0570; explicit XPath cast in `select` plus `as="map(*)"` is the verified workaround. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. REQ-126 remains In Progress, version 1.1.2. Next: catalog close-out and prioritized follow-up review.

**Named declaration verification (2026-10-12):** Release build passed (one existing warning, zero errors); all 4,583 tests across ten assemblies passed without failures/skips, including 561 catalog checks. Renderer verified 75 pages. Entry/index, source/local links, scoped inventories/provenance, all inline inputs/code/results, nine named tests, XML validity and independent portable reports verified. Manual engine gates were not rerun for the documentation/test-harness slice; membership is unchanged.

**Enclosing-mode review — 2026-10-12:** [Deferred enclosing modes](deferred/enclosing-modes.md) supplies a full invoice export/audit case, pinned draft/source evidence and 20 executable checks. Nested mode templates refuse with XTSE0260; top-level named rules provide supported dispatch, without enclosing-rule isolation. Explicit mode, #current and lexical defaults, empty/reordered input and namespace identity are verified. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. Version remains 1.1.2; REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**Enclosing-mode verification (2026-10-12):** Release build passed (one existing warning, zero errors); 4,603 tests passed across ten assemblies with no failures/skips. Final formatted fixtures passed all 581 catalog checks. Renderer verified 76 pages; entry/index, source/local links, scoped inventories/provenance, full input/code/output, six named tests, XML validity and eleven independently calculated reports verified. Manual engine gates were not rerun; membership is unchanged.

**Capturing-accumulator review — 2026-10-12:** [Deferred capturing accumulators](deferred/capturing-accumulators.md) adds full invoice input/output and 16 executable cases. Accepted capture=yes retains source identity rather than a draft-required snapshot; invalid/start-phase capture is tolerated in tested 4.0 forms. Ordinary accumulators, explicit snapshots and scalar projection are verified separately in memory, without streaming or special annotation-copy claims. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. REQ-126 remains In Progress, version 1.1.2. Next: catalog close-out and prioritized follow-up review.

**Capture disposition verification (2026-10-12):** Release build passed (one existing warning, zero errors); 4,619 tests passed across ten assemblies without failures/skips. Final formatted fixtures passed all 597 catalog checks; 77 pages rendered. Entry/index, source/local links, scoped inventories/provenance, input/code/output, six named tests, XML validity and fourteen report calculations verified; node-identity evidence comes from the executable fixtures. Manual engine gates were not rerun; membership is unchanged.

**fn:apply-templates review — 2026-10-12:** [Deferred function and invoice alternatives](deferred/fn-apply-templates.md) adds 28 executable checks. Calls/references refuse with XPST0017; instruction availability is separate from function availability. Full invoice input/output verifies named-mode dispatch and a stylesheet-function wrapper with ordinary/tunnel parameters, empty/reordered input and explicit joining. Draft dynamic options/public-mode validation remain deferred. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**Serialization parameter review — 2026-10-12:** [Nine draft additions and invoice export](deferred/serialization-parameters.md) adds 52 executable cases. XSLT JSON solidus control works; fn:serialize ignores the new keys. JSON Lines, canonicalization, layout controls and CSV serialization remain deferred; fn CSV fallback and XSLT sequence-as-array are explicit boundaries. Complete portable JSON Lines input/output includes empty/reordered cases. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**External-input function review — 2026-10-12:** [Five function boundaries and partner invoices](deferred/external-input-functions.md) adds 55 checks. parse-html, html-doc, invisible-xml, unparsed-binary and xsd-validator remain absent/out of the original implementation scope. Pinned contracts and per-function dependency/ownership rationale accompany complete normalized XML, fixed-record, binary-value and business-guard alternatives. These do not implement HTML recovery, arbitrary grammar parsing, binary acquisition or XSD factory validation. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 registered names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**GNode/JNode scope review — 2026-10-12:** [Partial model and invoice navigation](data-model/jnode-template-boundary.md) replaces blanket exclusion with shipped JNode support and a deferred remainder. Eighteen added checks verify full invoice navigation/portable reports and broader node()/xnode()/identity gaps. Full generic-model and stylesheet built-in behavior remain uncertified. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 registered names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**Language/streaming scope review — 2026-10-12:** [Existing APIs and separate 4.0 deltas](deferred/language-streaming-scope.md) adds 28 cases through XQuery, XPath, ordinary XSLT and actual streaming APIs. Corrects stale XPath where exclusion and distinguishes REQ-087 Done from historical residual reports. Full XQuery/Streaming 4.0 remains unclaimed. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.


**Focus-constructor audit — 2026-10-12:** [Typed supplier delivery import and all 49 names](operators-and-syntax/focus-functions.md) adds 508 executable checks, representative result types, lexical/source errors, missing context, empty/multiple input, default 3.1 refusal, experimental parity and portable 3.1 reports. List types and xs:error are explicit. Current coverage: 67 entries, 1,286 feature + 24 chapter checks = 1,310; four worked chapters; all 117 names assigned. Version remains 1.1.2, REQ-126 In Progress. This current snapshot supersedes earlier delivery counts. Next: D2, supplementary signatures/keywords/defaults/options.


**Signature contract review — 2026-10-12:** [Metadata matrix and consumer boundaries](SIGNATURE-CONTRACT-AUDIT.md) supplies 68 new-name families plus 20 selected existing functions, 148 exact registration checks and 39 behavior cases (187 new checks). 37 families lack keyword metadata; four keyword-name differences and three default-expression differences are recorded. Replace arity/default and two-let hash keyword failures have reproducible cases. Hash extension options are implementation-defined; missing mandatory algorithms remain distinct. Catalog: 67 entries, 1,473 feature + 24 chapter checks = 1,497; version 1.1.2, REQ-126 In Progress. D2 remains partial. This snapshot supersedes earlier delivery counts; next review missing metadata/defaults/options family by family.


**Text/Unicode contract review — 2026-10-12:** [Eight-family review and full label/filename reports](TEXT-CONTRACT-REVIEW.md) adds 83 checks: 24 keyword/mode refusals, 55 positional/default/empty/invalid/option cases and four normal/empty 3.1/4.0 reports. Metadata remains absent; eight of 37 such families are reviewed and 29 remain. Empty-source/empty-needle boundary wording corrected; coercion/option-validation follow-ups remain explicit. Current catalog: 67 entries, 1,556 feature + 24 chapter checks = 1,580; version 1.1.2, REQ-126 In Progress. This snapshot supersedes earlier delivery counts. Next: sequence selection/repetition contracts, keeping D2 partial.


**Sequence contract review — 2026-10-12:** [Six-family review and complete dispatch batch reports](SEQUENCE-CONTRACT-REVIEW.md) adds 65 checks: 18 keyword/mode cases, 41 focused defaults/types/identity/keyword-validation cases and six normal/empty/reordered 3.1/4.0 reports. Five more absent-keyword families reviewed; 13 of 37 total, 24 awaiting targeted review. slice keywords/defaults work; items-at accepts the tested integral decimal, while replicate/slice reject it. Untyped-node position/coercion limits remain explicit. Catalog: 67 entries, 1,621 feature + 24 chapter checks = 1,645; version 1.1.2, REQ-126 In Progress, D2 partial. This snapshot supersedes earlier delivery counts. Next: equality/distinctness/subsequence predicate contracts.


**Predicate contract review — 2026-10-12:** [Equality/distinctness/subsequence contracts and complete shipment reports](PREDICATE-CONTRACT-REVIEW.md) add 102 checks: 15 keyword/mode refusals, 79 positional/default/type/callback checks and eight normal/empty/reordered/duplicate-event reports in 3.1/4.0. Targeted absent-keyword review now covers 18 of 37 families, with 19 remaining; metadata remains absent. Map acceptance in atomic-signature equality functions and numeric/empty-string collation processing remain engine follow-ups. Current catalog: 67 entries, 1,723 feature + 24 chapter checks = 1,747. Version 1.1.2, REQ-126 In Progress, D2 partial. This snapshot supersedes earlier counts and next-step queues. Next: duplicate-values/highest/lowest/index-where comparison and selection contracts.


**Comparison/selection contract review — 2026-10-12:** [Four-family review and detailed warehouse reports](SELECTION-CONTRACT-REVIEW.md) add 68 checks: 12 keyword/mode cases, 50 focused default/type/callback/keyword-validation checks and six complete 3.1/4.0 warehouse reports. Three more absent-keyword families reviewed: 21 of 37 supplied, 16 remaining, metadata unchanged. index-where keywords succeed. Map atomization and extrema NaN ordering remain explicit follow-ups. Current catalog: 67 entries, 1,791 feature + 24 chapter checks = 1,815; version 1.1.2, REQ-126 In Progress, D2 partial. This snapshot supersedes earlier counts and queues. Next: map:entries/map:items/map:filter and array:build/array:items/array:empty container contracts.


**1.1.3 authoring patch published — 2026-10-12:** [REQ-127](../REQ-127-compact-source-inspection.md) is published with verified public artifacts. Catalog semantics/gate membership are unchanged: 67 entries, 1,791 feature + 24 chapter checks = 1,815. REQ-126 remains In Progress, D2 partial (21/37 absent-keyword families reviewed; 16 remain). Next: map/array container contracts.


**Container contract review — 2026-10-12:** [Six-family review and detailed stock-feed reports](CONTAINER-CONTRACT-REVIEW.md) 89 new checks: 18 keyword/mode refusals, 65 focused member/item/default/type/callback checks and six complete XPath 3.1/4.0 stock-feed reports. Targeted absent-keyword review: 27 of 37 families supplied, 10 remaining; metadata unchanged. map:filter accepts tested string/numeric predicate results despite its boolean result signature; dedicated conversion/enforcement review remains. Current catalog: 67 entries, 1,880 feature + 24 chapter checks = 1,904; four worked chapters, 117 names assigned. Version 1.1.3, REQ-126 In Progress, D2 partial; no engine/API/harness change or release. This snapshot supersedes earlier counts and queues. Next: build-dateTime, days-in-month, unix-dateTime, seconds and duration-to-seconds contracts.


**Date/time contract review — 2026-10-12:** [Five-family review and detailed receipt-retention reports](DATE-TIME-CONTRACT-REVIEW.md) 108 new checks: 15 keyword/mode refusals, 87 focused Gregorian/timezone/unit/type/cardinality checks and six full XPath 3.1/4.0 receipt-retention reports. Targeted absent-keyword review: 32 of 37 supplied, five remaining; metadata unchanged. Malformed string/multi-item tolerance, record conversion and fractional-component precision remain explicit follow-ups. Current catalog: 67 entries, 1,988 feature + 24 chapter checks = 2,012; four worked chapters, 117 names assigned. Version 1.1.3, REQ-126 In Progress, D2 partial; no engine/API/harness change or release. This snapshot supersedes earlier counts and queues. Next: build-uri, decode-from-uri, collation-available, identity and is-NaN.


**URI/import utility contract review — 2026-10-12:** [Five-family review and detailed import/link reports](URI-UTILITY-CONTRACT-REVIEW.md) 105 new checks: 15 keyword/mode refusals, 82 focused URI/option/decoding/collation/identity/NaN checks and eight complete XPath 3.1/4.0 import/link reports. All 37 originally absent-keyword families now have targeted review; zero await that first review, but all 37 still lack metadata. Boolean-option and record-field tolerance, empty query-map precedence and context-dependent collation boundaries remain follow-ups. Current catalog: 67 entries, 2,093 feature + 24 chapter checks = 2,117; four worked chapters, 117 names assigned. Version 1.1.3, REQ-126 In Progress, D2 partial; no engine/API/harness change or release. This snapshot supersedes earlier counts and queues. Next: exhaustively inventory changed existing 3.1 function contracts; the current matrix samples only twenty.


**Complete baseline-name inventory — 2026-10-12:** [Existing 3.1 contract inventory](CHANGED-31-CONTRACT-INVENTORY.md) The complete 197-name fn/math/map/array baseline inventory is now supplied, with dated W3C 3.1 declarations, pinned F&O e7ee4ab8 declarations and fresh 3.1/4.0 evidence. Added 353 checks: 320 exact-arity lookups, 17 named-reference refusals, ten focused defaults/behavior checks and six complete stock reports. Thirteen families have positional arity declaration differences; seventeen draft arities are unavailable in the fresh standard context. Type/prose differences remain candidates for semantic review, not certified behavioral changes. Empty substring/subsequence lengths have distinct observed 3.1 and 4.0 results. Empty sort keys and map:merge options refuse XPTY0004. Catalog: 67 entries, 2,446 feature + 24 chapter checks = 2,470; four chapters, 117 assigned names. All 37 reviewed keyword families still lack metadata. REQ-126 stays In Progress, D2 partial; version 1.1.3, no engine/API/harness change or release. Next: fn:sort, array:sort, fn:min, fn:max and map:merge defaults/conversion contracts. Prose classification, D3/D4, historical adoption pins and AC-03 remain open.


**Ordering/default review — 2026-10-12:** [Five-family review and supplier reports](ORDER-DEFAULT-CONTRACT-REVIEW.md) Targeted review of fn:sort, array:sort, fn:min, fn:max and map:merge adds 132 checks (126 focused, six complete supplier reports), preserving source declarations and exact 3.1/4.0/experimental keyword boundaries. min/max have no key callback parameter. Explicit empty sort keys and merge options, unknown sort collations, NaN ordering and malformed merge input/option cardinality remain engine follow-ups. Supplier rankings use numeric price plus stable ID and explicit duplicate combine; normal/reordered results match. Current catalog: 67 entries, 2,578 feature + 24 chapter checks = 2,602; four worked chapters, 117 assigned names. Version 1.1.3, REQ-126 In Progress, D2 partial. All 37 reviewed keyword families still lack metadata; no engine/API/harness change or release. Next: distinct-values, index-of, deep-equal, compare and contains-token equality/collation contracts. Earlier counts and queues are historical; normative prose impact, D3/D4, historical pins and AC-03 remain open.


**Upstream contribution adapter — 2026-10-12:** [Local setup and contribution workflow](../XSLT40_UPSTREAM_TESTING.md) Upstream XSLT 4.0 contribution setup now takes priority over the next equality/collation catalog slice. Independent ignored xslt40-test checkout at pin 6205faf9f32ace6338ac245c41f8eb9a1447926f retains native catalogs/schema/tests; Bosak supplies an explicit --xslt40 adapter mode, bounded runner with revision/status receipts and failure/zero-execution exit status, catalog validator and four adapter controls. Initial upstream select evidence: 157/0/1. Existing XSLT40+ JSON case executes and fails (-0 versus expected -0e0); it remains a visible finding. No upstream tests or expected results changed, no upstream PR/message and no release. Version 1.1.3; REQ-126 In Progress, prior D2/D3/D4 and provenance/metadata limits retained.


**New-element contribution priority — 2026-10-12:** The [contribution priority queue](../XSLT40_CONTRIBUTION_QUEUE.md) now starts with new XSLT 4.0 elements: array/member, select, switch, named item types and named records. Likely use is estimated, not measured; structural stylesheet counts are not semantic coverage. Next: N01 native array-case audit and a small nonduplicate order-export/member batch. Schema-awareness and inherited JSON findings remain secondary triage. Version 1.1.3; REQ-126 In Progress; no engine change or upstream submission.


**First upstream test batch — 2026-10-12:** N01 now has a [six-case native array/member batch](../../contributions/xslt40/array-member-boundaries/README.md), committed locally on bosak/array-member-review at e72f1b0e8511f78478e66c112201317cfe7b7ad1. Five exact new cases pass; array-032 exposes missing XTTE4060 enforcement and returns A|B. Normative expectations are retained. A portable mail patch, source/expected-outcome mapping, clean-commit receipts and an upstream PR draft preserve the work; no upstream submission or independent processor confirmation yet. Version 1.1.3, REQ-126 In Progress, no Bosak engine/API/adapter change. Next: independent assertion review and separate error triage before submission.


**Array/member correction — 2026-10-12:** The [array/member assertion review](../../contributions/xslt40/array-member-boundaries/ASSERTION_REVIEW.md) confirms the six native expectations. Bosak now preserves existing core JNode wrappers in both XSLT evaluation paths, unwraps member values at array construction, and raises XTTE4060 for multi-item per-focus results containing a JNode. All six unchanged native cases pass; the original five-pass/one-failure receipts remain historical. Five additional regression checks cover direct, empty, conditional and variable-carried errors plus valid grouped/nested wrappers. Source candidate 1.1.4 is unpublished; 1.1.3 remains the published version. N01 and REQ-126 stay In Progress; independent processor confirmation and upstream submission remain outstanding.


**Upstream submission — 2026-10-12:** The first six-case native array/member batch is [submitted as xslt40-test PR #22](https://github.com/qt4cg/xslt40-test/pull/22). All six exact cases pass with Bosak; specification review is recorded. No second processor was run: independent cross-check was requested in the PR. [Submission details](../../contributions/xslt40/array-member-boundaries/SUBMISSION.md) explain the Saxon PE/EE and partial-for-each limitation and the decision to seek upstream review without making local independent execution a prerequisite. Review/acceptance remain pending; N01 and REQ-126 stay In Progress. Bosak fix PR #202 merged after CI 38295568498 passed (6811 unit tests); source 1.1.4 remains unpublished, public release 1.1.3 unchanged.


**Remaining priority element batches — 2026-10-12:** Four additional native batches are submitted: [select PR #23](https://github.com/qt4cg/xslt40-test/pull/23), [switch PR #24](https://github.com/qt4cg/xslt40-test/pull/24), [item-type PR #25](https://github.com/qt4cg/xslt40-test/pull/25), [record-type PR #26](https://github.com/qt4cg/xslt40-test/pull/26). Eight new cases validate against native catalog schemas: switch 2 pass; select 2 fail (empty select results), item-type 2 fail (XPST0051), record-type/field 2 fail (constructor XPST0017). Normative outputs are retained, with [portable patches and detailed example/evidence pages](../../contributions/xslt40/README.md). No independent processor was run; reviewer cross-check requested in each PR. These are initial priority-family contributions, not complete XSLT 4.0 coverage. N02–N05 and REQ-126 stay In Progress while upstream review and engine triage are pending. Source 1.1.4 remains unpublished; public release 1.1.3.


**Select body evaluation — 2026-10-12:** Bosak now recognizes and evaluates xsl:select body XPath in template, function, simple-content and accumulator-rule paths, preserving literal newlines/CDATA and current focus without TVT expansion. Declared result coercion, collation restoration, fallback/content validation and streamed-body analysis are covered by 14 focused regressions. Both unchanged native contribution cases pass; [post-fix evidence](../../contributions/xslt40/select-boundaries/POST_FIX_RESULTS.json) retains the broader seqtor result 78/5/9. The five remaining failures concern expression-body namespace declarations, so complete select conformance is not claimed. Original failed receipts and upstream PR #23 remain unchanged; independent confirmation/acceptance are pending. Source candidate is 1.2.0 under the Minor-for-new-features convention, unpublished; public release remains 1.1.3. N02 and REQ-126 stay In Progress.


**Enumeration alias slice — 2026-10-12:** Bosak source candidate 1.2.0 (unpublished) now resolves xsl:item-type enumeration aliases in XPath instance-of and treat expressions, including AVTs, namespace rebinding, EQNames and occurrence indicators. Both unchanged native cases type-alias-023/024 pass; broader type-alias-filter run is 3 pass/29 fail/0 skip (includes inherited related cases). Other alias types, named XSLT @as conversion, package exposure and complete recursive/schema-collision validation remain unsupported. Duplicate winning declarations raise XTSE4030; lower-precedence definitions remain overridable. Original failure receipts are preserved; upstream PR #25 review and independent confirmation are pending. N04 and REQ-126 remain In Progress; public release remains 1.1.3. Next: generated nominative record constructors, with wider alias support and select namespace declarations tracked separately.


**Named record constructor slice — 2026-10-13:** Bosak source candidate 1.2.0 (unpublished) now recognizes xsl:record-type/xsl:field and registers constructor=yes functions. Constructors retain field order and empty entries, convert field values, supply trailing defaults and exclude static fields from arguments. Records carry declaration identity: constructed values satisfy named @as and instance-of tests; same-shaped other types and plain maps do not. Both unchanged native cases record-026/027 pass. The wider record substring sweep is 12 pass/52 fail/1 skip, including inherited structural/subtype cases. Plain-map coercion/casts, recursive record definitions, complete package exposure/function signatures and streaming conformance remain follow-ups. N05 and REQ-126 remain In Progress; original failed receipts and upstream PR #26 remain unchanged, with review/independent confirmation pending. Public release remains 1.1.3; no publication. Next: prioritize the wider named-type/coercion gaps and select body namespace declarations after checking this slice's CI.


**Select namespace follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) now supports expression-local XPath 4.0 namespace declarations in the XPath API and xsl:select bodies. Prefix bindings override host bindings; an optional default element namespace must come first. Default ##any matches unprefixed element axis name tests across namespaces while preserving attribute/kind-test boundaries and XML Schema item type resolution. Namespace contexts restore after success or failure. All unchanged native seqtor-140–146 cases pass; the broader seqtor-filter run is 83 pass/0 fail/9 skip. The nine known harness skips, independent confirmation and upstream PR #23 acceptance remain outstanding, so this does not establish complete XSLT 4.0 conformance. N02 and REQ-126 remain In Progress; public release remains 1.1.3. See [current namespace evidence](../../contributions/xslt40/select-boundaries/NAMESPACE_RESULTS.json) and the [pinned XPath namespace declaration source](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xquery-40/src/expressions.xml). Twenty-three regressions were added; catalogue entry/check counts are unchanged. Next: named item-type conversion and remaining record coercion gaps, after this slice passes CI. Earlier select counts are historical.


**Enumeration binding follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) now converts named enumeration aliases at XSLT variable, parameter and result boundaries. Namespace/EQName aliases and occurrence indicators resolve from the declaring instruction; atomization and membership use XPath function conversion rules. Direct/dynamic function metadata uses the same definitions; private package aliases are isolated and restored. Sixteen regressions cover valid values, membership/cardinality errors, source attributes, record fields, template/select/function boundaries and mixed-version refusal. The invoice currency fixture now returns EUR; invalid XYZ still raises XTTE0570. Original native type-alias-023/024 still pass; broader filtered family remains 3/29/0, with structural/recursive/schema/package gaps outstanding. N04 and REQ-126 stay In Progress; no complete alias or package-conformance claim. Public release remains 1.1.3. See [binding evidence](../../contributions/xslt40/item-type-boundaries/BINDING_RESULTS.json) and [XSLT 4.0 named item types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types). Catalogue counts remain 67 entries / 2,578 feature + 24 chapter checks. Next: structural aliases and remaining named-record coercion, after CI. Earlier named-enumeration binding refusals are historical.


**Atomic and array binding follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) supports atomic and array item-type aliases at XSLT declaration and function boundaries. Array member types can use forward aliases, nested arrays, optional member sequences and enumeration aliases. Definition namespace bindings are preserved; outer occurrence indicators apply to arrays rather than their members. Supported direct/indirect recursive aliases raise XTSE4035. XPath 4.0 array coercion converts member sequences without atomizing the outer array; the XPath 3.1 path remains unchanged. Native type-alias-001/002 now pass; the filtered family is 6 pass/26 fail/0 skip, including unchanged contribution cases 023/024. Non-enumeration aliases in XPath instance-of/treat, general map/function/choice aliases, complete recursive validation, schema collisions and package exposure remain follow-ups. N04/REQ-126 remain In Progress. Public release remains 1.1.3. See [separate evidence](../../contributions/xslt40/item-type-boundaries/ARRAY_BINDING_RESULTS.json). Existing catalogue counts and expected outcomes are unchanged. Earlier slice results are historical. Next: structural aliases in XPath type expressions, other structural alias forms, then remaining named-record coercion.


**Atomic and array type-expression follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) resolves the supported atomic and array xsl:item-type aliases in XPath instance-of and treat expressions, including AVTs and expression-local namespace declarations. Tests preserve alias occurrences and definition namespaces, distinguish type checking from conversion, and isolate private package definitions from same-named caller aliases. Mixed XPath 3.1 instructions still refuse these aliases. Fourteen new regressions pass; the public EnumerationItemTypes option retains its enum-only contract. General map/function/choice aliases, aliases nested in arbitrary XPath structural type syntax, complete schema/package semantics and remaining named-record coercion remain follow-ups. N04/REQ-126 remain In Progress. Public release remains 1.1.3. The native filtered family remains 6 pass/26 fail/0 skip; original 023/024 still pass. See [separate evidence](../../contributions/xslt40/item-type-boundaries/EXPRESSION_RESULTS.json). Catalogue counts and expected outcomes remain unchanged. Earlier slice results are historical. Next: other structural alias forms and nested references, then remaining named-record coercion.


**Typed map aliases and nested references — 2026-10-13:** Source candidate 1.2.0 (unpublished) supports typed map xsl:item-type aliases and nested alias references inside array/map types at XSLT binding/function boundaries and XPath instance-of/treat operands. Shared expansion respects declaration/use namespaces, occurrence indicators, quoted enumeration members, EQName URI delimiters, whitespace and nested comments. XPath 4.0 typed-map coercion recursively converts keys and values, rejects converted key collisions, preserves entry order and retains already-matching maps/record identity. The new coercion path excludes wildcard map types; their existing behavior is preserved. Twenty new regressions cover these boundaries and the documented sensor example. Function/choice/structural-record aliases, arbitrary type syntax, complete schema/package semantics and remaining named-record coercion remain follow-ups. N04/REQ-126 remain In Progress; public release remains 1.1.3. Broader native type-alias results remain 6/26/0; original 023/024 pass. See [separate evidence](../../contributions/xslt40/item-type-boundaries/MAP_RESULTS.json). Catalogue counts/outcomes and original native assertions remain unchanged. Earlier slice results are historical. Next: function and choice alias forms, then remaining named-record coercion.


**Function and choice aliases — 2026-10-13:** Source candidate 1.2.0 (unpublished) now expands function signatures and ordered choice xsl:item-type aliases, including nested references, parameter/result occurrences and outer function-item occurrences. Existing binding conversion wraps function results and tries choice alternatives in declaration order; instance-of/treat checks do not coerce. XPath 4.0 function instance-of now uses cardinality-aware matching, safely rejecting non-functions and handling empty sequences. Nineteen new regressions cover these boundaries, cycles, mixed-version refusal and a calibration example. Structural-record aliases, remaining named-record coercion and complete schema/package semantics remain follow-ups. N04/REQ-126 remain In Progress; public release remains 1.1.3. Native type-alias improves to 7/25/0: case 011 now passes; original 023/024 remain green. See [separate evidence](../../contributions/xslt40/item-type-boundaries/FUNCTION_CHOICE_RESULTS.json). Catalogue counts and original native expectations are unchanged; earlier slice notes are historical. Next: structural-record alias boundaries and remaining named-record coercion.


**Structural-record aliases — 2026-10-13:** Source candidate 1.2.0 (unpublished) now expands structural-record aliases and aliases nested in record fields, arrays, choices and function signatures. Quoted field names, default item()* fields, trailing commas, nested comments and declaration namespaces are preserved. Existing structural record coercion converts field values and annotates records; type checks require record values and do not coerce plain maps. Eleven new regressions cover the documented sensor example, nested contracts, function boundaries, invalid fields and cycles. Nominative record coercion, recursive named records and complete schema/package semantics remain follow-ups. N04/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](../../contributions/xslt40/item-type-boundaries/RECORD_ALIAS_RESULTS.json). Catalogue counts and original native assertions remain unchanged; earlier slice notes are historical. Next: nominative record coercion boundaries.


**Nominative record coercion — 2026-10-13:** Source candidate 1.2.0 (unpublished) now coerces maps to package-local nominative xsl:record-type declarations at variable and function boundaries. Conversion applies field types, missing-field defaults and static initializers, rejects surplus keys and supplied static fields, preserves declaration order and annotates the result with the target identity. Existing matching records are retained. Type checks still reject plain maps and distinguish same-shaped declarations. Nine new regressions cover sensor defaults, function references, cardinality, invalid bindings and identity; the previous valid-map refusal test now rejects surplus fields. Named casts, recursive named records, nested named-type/static namespace resolution and complete package exposure remain follow-ups. N04/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](../../contributions/xslt40/record-type-boundaries/COERCION_RESULTS.json). Catalogue counts remain unchanged; the named-record plain-map fixture now expects INV-2048. Original native assertions remain unchanged; earlier slice notes are historical. Next: named record cast and nested type resolution boundaries.


**Named record casts and nested references — 2026-10-13:** Source candidate 1.2.0 (unpublished) now supports package-local nominative record cast/castable operations. Casts retain map operands, cast each present field sequence, apply defaults to missing fields, discard surplus keys, protect static fields and attach the target declaration identity. Nested named references in binding/function signatures resolve in their declaration namespace, and nested named record fields reuse casting recursively. Fourteen new regressions cover lexical/type failures, empty/optional inputs, field cardinalities and namespace boundaries. General array/map casts, item-type alias cast expansion, recursive definitions, full package exposure and complete static diagnostics remain follow-ups. N05/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](../../contributions/xslt40/record-type-boundaries/CAST_RESULTS.json). Catalogue counts/outcomes and original native assertions remain unchanged. Earlier slice notes are historical. Next: remaining recursive/static type-resolution and cast-container boundaries.


**Finite recursive records and container casts — 2026-10-13:** Source candidate 1.2.0 (unpublished) now casts XPath 4.0 arrays/maps by casting member/key/value sequences, retaining container shape and checking cardinality. Map-key collisions raise XQDY0137; wildcard casts retain the supplied container and empty container types reject nonempty inputs. Named record field casts reuse the shared sequence caster. Record field metadata resolves aliases and finite self/mutual named references in the declaration namespace without expanding nominative identities. Sixteen new regressions cover finite linked lists, recursive array casts, nested containers, invalid members and collisions. Arbitrary-depth resource handling, full recursive/static diagnostics, item-type alias cast expansion and package exposure remain follow-ups. N05/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](../../contributions/xslt40/record-type-boundaries/CONTAINER_RESULTS.json). Catalogue counts/outcomes and original native assertions remain unchanged; earlier slice notes are historical. Next: item-type alias cast expansion and static named-type validation.


## Item-type alias casts and declaration validation — 2026-10-13

Source candidate 1.2.0 (unpublished) expands named item-type aliases in XPath 4.0 cast and castable targets, including nested container targets and namespace forms. Atomic, enumeration, array, map and structural/nominative record aliases reuse existing casting. Optional targets preserve empty sequences. Function and generic item/node aliases raise XPST0080 as casting targets. Unknown named references in supported alias definitions raise XPST0051 during stylesheet registration; declared record references preserve nominative identity. Eighteen new regressions pass. Full schema validation, unsupported alias grammar, unknown names outside declarations, choice-target eligibility and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress; published version stays 1.1.3.

See [cast and validation evidence](../../contributions/xslt40/item-type-boundaries/CAST_VALIDATION_RESULTS.json). Native type-alias remains 8/24/0; unchanged contributed cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Choice casting target validation — 2026-10-13

Source candidate 1.2.0 (unpublished) statically rejects XPath 4.0 cast/castable choices containing syntactically non-atomic alternatives, including arrays, maps, records, functions and node tests. Validation covers direct targets and expanded aliases, follows nested parentheses and preserves quoted separators and EQName text. Atomic/enumeration choices, optional inputs, ordinary container-valued instance-of choices and standalone container casts remain supported. Twenty-one new regressions pass. Schema-defined and nominative QName classification, unknown expression type names, broader alias grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress; published version stays 1.1.3.

See [choice casting evidence](../../contributions/xslt40/item-type-boundaries/CHOICE_CAST_RESULTS.json). Native type-alias remains 8/24/0; unchanged contributed cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Host static named-type validation — 2026-10-13

Source candidate 1.2.0 (unpublished) validates package-local named types in supported XPath 4.0 instance-of, treat, cast and castable targets when the XSLT host compiles the expression. Unknown direct/nested names raise XPST0051, including AVT expressions; undeclared prefixes retain XPST0081. Declared nominative record references are canonicalized to their expanded names, and record alternatives in choice cast targets raise XPST0080. Ordinary record-valued type-test choices remain valid. The internal host registry survives inline namespace declarations, recognizes built-in/XDM types and consults in-scope simple schema types. Twenty-one new regressions pass. Binding/field declaration validation, unsupported type grammar, complete schema variety/choice semantics and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress; published version stays 1.1.3.

See [static named-type evidence](../../contributions/xslt40/item-type-boundaries/STATIC_TYPE_RESULTS.json). Native type-alias remains 8/24/0; unchanged contributed cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Declaration static named-type validation — 2026-10-13

Candidate 1.2.0 now validates supported XPath 4.0 sequence-type names on XSLT @as declarations at stylesheet registration, including unused variables, parameters, function results and record fields. The check runs after package-local aliases and records are registered and traverses import/include modules with each declaration's namespace context. Unknown direct/nested names raise XPST0051; undeclared binding prefixes raise XPST0081; record registration retains XTSE0280 for unbound field prefixes. Forward aliases, finite recursive record references, default type namespaces and quoted enumeration/field names remain valid. Fourteen new regressions cover these boundaries. XPath 3.1 declarations keep their existing path. Unsupported shared-walker grammar, complete schema variety semantics and package type exposure remain follow-ups; N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; no release or gate promotion.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/DECLARATION_TYPE_RESULTS.json). Native type-alias: 9 pass/23 fail/0 skip; type-alias-019 newly passes and unchanged contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Node-kind alias grammar — 2026-10-13

Candidate 1.2.0 supports node-kind aliases whose tests have empty parentheses: element(), attribute(), text(), comment(), processing-instruction(), namespace-node() and document-node(). The shared type walker also accepts whitespace and nested comments around empty tests. These types can appear within supported array/map/record/function/choice types; adjacent unknown names now receive static validation instead of being hidden by an unsupported node-kind branch. Sixteen regressions verify node identity, occurrences, structural nesting, unknown names, binding failures and non-atomic choice cast rejection. Named/wildcard/schema-annotated node tests, fn/named-parameter function type grammar, full schema semantics and package type exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version remains 1.1.3; candidate 1.2.0 stays unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/NODE_KIND_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Named node-type alias grammar — 2026-10-13

Candidate 1.2.0 supports named element/attribute node tests, the complete * wildcard, and document-node(element(...)) inside package-local aliases and supported structural sequence types. Node names resolve in their declaration namespace and remain stable under use-site prefix/default namespace rebinding; unprefixed attributes retain the empty namespace. The host statically rejects unbound node prefixes and validates unknown type names beside named node tests. Explicit Q{}Name matching now checks both namespace and case-sensitive local name. Seventeen new regressions cover matching/mismatches, node identity, nested types, record fields, namespace defaults and static errors. Schema-annotated node tests, partial wildcards, processing-instruction targets, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/NAMED_NODE_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Processing-instruction target aliases — 2026-10-13

Candidate 1.2.0 supports processing-instruction targets in item-type aliases and supported structural sequence types. NCName and quoted-string targets retain case, normalize surrounding whitespace and are never expanded as type QNames. The parser validates targeted sequence-type syntax using a shared target reader; runtime matching compares the actual PI target. Malformed targets raise XPST0003 and quoted targets outside the NCName lexical space raise XPTY0004. Twelve new regressions verify identity, target/case mismatch, nesting, optional occurrences and static diagnostics. Partial wildcards, schema-annotated node tests, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/PI_TARGET_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Partial node wildcard aliases — 2026-10-13

Candidate 1.2.0 supports partial element/attribute name wildcards in aliases and supported structural types: *:Name matches an exact local name in any namespace; prefix:* and Q{uri}* match any local name in the specified namespace. Prefix wildcards retain declaration bindings under use-site rebinding, including document-node(element(...)) nesting. Unbound wildcard prefixes raise XPST0081; adjacent unknown type names retain XPST0051. The ##any declaration default now expands unprefixed element tests to *:Name while unprefixed attributes stay unqualified. Twenty new regressions verify identity, case/namespace mismatches, empty namespaces, nested arrays/documents and diagnostics. Node-name unions, schema-annotated tests, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/NODE_WILDCARD_RESULTS.json). Native type-alias improves to 11 pass/21 fail/0 skip: wildcard-type-001/002 newly pass and original contribution cases 023/024 still pass. Broader failures and independent processor confirmation remain outstanding.


## Node-name union aliases — 2026-10-13

Candidate 1.2.0 supports node-name unions in element/attribute item-type aliases and supported structural sequence types: element(doctor|nurse) and attribute(price|cost) match any listed name. Each alternative retains declaration namespaces and may use a partial wildcard, including nested document-node(element(...)), arrays and record fields. EQName namespace URIs containing pipe or comma characters remain intact. Empty alternatives raise XPST0003; an undeclared prefix in any alternative raises XPST0081; adjacent unknown type names retain XPST0051. Twenty-two new regressions cover positive/negative matching, identity, declaration prefix rebinding, wildcard combinations, nested types and ##any defaults. Schema-annotated alias traversal, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/NODE_UNION_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Schema-annotated node aliases — 2026-10-13

Candidate 1.2.0 supports schema annotations in element/attribute item-type aliases and supported structural sequence types, including node-name unions, nested documents, arrays and record fields. The annotation resolves independently against in-scope schema types and retains declaration namespaces under prefix rebinding; item-type aliases are not substituted into the schema annotation. Built-in and imported simple/complex schema types are accepted. Unknown annotations raise XPST0008; undeclared annotation prefixes raise XPST0081; malformed or empty annotations raise XPST0003. Only element annotations permit the nillable ? marker. The ##any default maps unprefixed schema annotations to the XML Schema namespace. Twenty-eight new regressions cover matching untyped and validated nodes, non-coercion, imported simple/complex types, nillability, namespace identity, nested contracts and unused declarations. fn/named-parameter function grammar, schema-element/schema-attribute alias traversal, document shorthand and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/SCHEMA_NODE_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Function-type grammar aliases — 2026-10-13

Candidate 1.2.0 accepts fn(...) as a synonym for function(...) and documentary parameter names in supported function-type aliases, direct XPath type operands and XSLT binding signatures. Names do not affect matching or coercion; duplicate expanded names raise XQST0039, including different prefixes bound to the same namespace and unprefixed names paired with Q{} names. Undeclared documentary prefixes raise XPST0081; malformed named parameters raise XPST0003; unknown parameter/result item types retain XPST0051. Named and unnamed parameters may be mixed, with independent name scopes for nested function types. Nested arrays, records and higher-order signatures retain parameter/result sequence occurrences. XPath 3.1 rejects fn type synonyms and named type parameters. Twenty-six new regressions cover callback conversion, wildcard maps/arrays, direct bindings without aliases, direct host evaluation, nesting, namespace identity, diagnostics and version boundaries. Annotated function types, schema-element/schema-attribute alias traversal, document shorthand and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/FUNCTION_GRAMMAR_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Schema-declaration and document shorthand aliases — 2026-10-13

Candidate 1.2.0 supports schema-element(Name), schema-attribute(Name) and document-node(schema-element(Name)) in item-type aliases and supported structural sequence types. Schema declaration names retain their declaration namespace under prefix rebinding and must resolve to imported global elements/attributes; unknown or unavailable declarations raise XPST0008 and undeclared prefixes raise XPST0081. Wildcards, unions and empty declaration names raise XPST0003. Schema-element under ##any uses no namespace for an unprefixed declaration; unprefixed schema attributes always use no namespace. Document shorthand such as document-node(root|other) normalizes to document-node(element(root|other)), including wildcard and EQName alternatives, and reuses single-root matching. Direct host type operands and alias-free XSLT document/schema bindings normalize the new forms. Twenty-nine new regressions cover node identity, namespace mismatches, imported declarations, substitution members, typed attribute matching, rejection of untyped nodes, nested arrays, ##any defaults, empty documents and static validation of unused bindings. Annotated function types, package exposure and broader native type-alias gaps remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/SCHEMA_DECLARATION_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Package named type exposure — 2026-10-13

Candidate 1.2.0 accepts item-type and record-type components in xsl:expose and xsl:accept. Public or exposed named types enter using-package type registries unless accepted as hidden; private dependencies retain the defining package's resolved alias definitions and record metadata. Accept names resolve namespaces on the accept element. Missing component names, incompatible acceptance, invalid declaration visibility and collisions with local types produce static errors. Seventeen regressions cover public declarations, explicit and wildcard exposure, private and hidden names, prefix scope, record constructors, private alias dependencies rejected collisions and public/private visibility through an intermediate package. Native type-alias improves from 11/21/0 to 15/17/0; newly passing cases are 018, 020, 021 and 022. Original contribution cases 023/024 remain green. Broader native failures and independent processor confirmation remain outstanding; N04/N05 and REQ-126 stay In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/PACKAGE_EXPOSURE_RESULTS.json). The [XSLT 4.0 named type and package rules](https://qt4cg.org/specifications/xslt-40/Overview.html) define these visibility boundaries.


## Fixed namespace type contexts — 2026-10-13

Candidate 1.2.0 resolves fixed-namespaces for named item/record type names, supported nested type references, static declaration validation and the main XPath compilation paths. Supported tokens include #standard, selected root-native or conventional prefixes, explicit prefix=URI bindings and namespace-document references through the existing stylesheet resolver. Bindings are module-local, cached at compilation, unaffected by descendant rebinding and applied in token order. Invalid bindings or unavailable namespace documents raise XTSE0122. Unprefixed type/node names retain their existing default-namespace rules; fixed bindings do not create native result namespaces. Sixteen regressions cover standard prefixes, nested node contracts, explicit/repeated bindings, namespace documents, include isolation and invalid/unavailable bindings. Native type-alias remains 15/17/0: case 016 now passes namespace resolution but still fails its independent inline-function record conversion contract with XPTY0004. No upstream assertion changes. Fixed namespace coverage of other QName-bearing XSLT attributes, pattern compilation and shadow/static processing remains a follow-up; this is not complete fixed-namespaces support. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/FIXED_NAMESPACE_RESULTS.json). The [XSLT 4.0 fixed-namespaces rules](https://qt4cg.org/specifications/xslt-40/Overview.html#fixed-namespaces) define the namespace token and scope rules.


## Inline function named type contracts — 2026-10-13

Candidate 1.2.0 expands host named types in inline function parameter and return contracts before IR lowering. Both function(...) and fn(...) retain resolved atomic, array, structural record and nominative record types, including occurrence indicators and nested member aliases. Signature resolution uses the expression static namespace context, including defaults, EQNames, inline namespace declarations and fixed bindings; the existing VM conversion rules apply to arguments and results. Unknown supported signature types are rejected statically even when the function is unused. Public EnumerationItemTypes registrations also resolve in inline signatures and remain restricted to single enum item types. Omitted annotations and XPath 3.1 instructions keep their existing behavior. Twenty-five regressions cover record/date updates, argument and return coercion, cardinality errors, namespaces, array members, record identity, AVTs, public enumeration boundaries and mixed-version refusal. Native type-alias improves from 15/17/0 to 16/16/0: case 016 now passes without changing its assertion; original contribution cases 023/024 remain green. Broader record matching, annotated function types, type patterns and remaining native failures stay outstanding. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/INLINE_FUNCTION_RESULTS.json). The [XPath 4.0 inline function rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-inline-func) define parameter and result contracts.


## Direct structural record bindings — 2026-10-13

Candidate 1.2.0 coerces direct structural record(...) and record(*) XSLT binding contracts through the existing XPath 4.0 converter. Supported direct record fields resolve aliases and node/type namespaces in their declaration context. Local/global variables and template parameters preserve record annotations and numeric field conversion; invalid values retain XTTE0570/XTTE0590 binding diagnostics. Required single-record and record-plus conversion now reject empty sequences in both empty-value representations, while optional/star records retain empty-sequence support. Plain maps still do not match annotated record instance tests; record(*) requires an existing annotation. Seventeen regressions cover local/global/default-parameter bindings, occurrence indicators, field aliases and node contracts, empty records, invalid values/extra fields, parameter diagnostics and required inline function returns. The local inline-record40 invoice example now verifies INV-2048|EUR instead of the previously observed XTTE0570. Native type-alias stays 16/16/0: case 050 now gets past XTTE0570 record coercion but still fails its independent result assertion. Upstream assertions remain unchanged. Type patterns, broader native record outcomes and annotated function types remain follow-ups. N04/N05 and REQ-126 stay In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/record-type-boundaries/STRUCTURAL_BINDING_RESULTS.json). The [XPath 4.0 structural record rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-record-types) distinguish coercion from annotated membership.


## Template type patterns — 2026-10-13

Candidate 1.2.0 supports standalone XSLT 4.0 template type patterns ~T with zero or more predicates, including parenthesized forms, named item types, structural records and nominative records. The matcher delegates membership to the existing XPath type tests and preserves numeric predicate, singleton focus and dynamic-error-as-nonmatch behavior. Template patterns compile with the declaring package type registry and applicable static namespaces, including fixed-namespaces. Default priorities cover item/node/function/map/array families, built-in atomic derivation depth, enumerations and supported choice types; predicate patterns have priority 0.5 and explicit priorities are retained. xs:anyAtomicType is now recognized in host-supplied static type contexts. Twenty-nine regressions cover these behaviors, invalid syntax, previous-profile rejection, quoted field names, EQName delimiters and package-private alias isolation. Native type-alias advances from 16/16/0 to 18/14/0: cases 008 and 009 now pass with their upstream assertions unchanged. Ordinary wildcard node patterns, full XSLT 4.0 union-pattern priority semantics, schema-defined atomic priority derivation and named-type/fixed-namespace authoring-validation contexts remain follow-ups. Non-template pattern slots have not gained the template declaring-type registry in this slice. Plain maps still do not acquire record annotations through pattern matching; cases 006/010/013 and case 050 result handling remain independent boundaries. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/TYPE_PATTERN_RESULTS.json). The [XSLT 4.0 type-pattern rules](https://qt4cg.org/specifications/xslt-40/Overview.html) and [default priorities](https://qt4cg.org/specifications/xslt-40/Overview.html) define matching and conflict resolution.
