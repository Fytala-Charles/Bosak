<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak REQ-126 catalog completeness audit">
  <br><br><h1>REQ-126 Catalog Completeness Audit</h1>
  <p>Evidence, missing coverage, and the next documentation deliveries</p>
</div>

## Finding and scope

**REQ-126 is not ready to close.** The three worked chapters are supplied, but
the catalog does not yet cover every landed feature or record every deferred and
rejected decision. Existing entries also need more precise provenance and broader
examples. This audit is dated 2026-10-11 against source revision
`9d3950f8ebc8bb0793c9317a91d3d412095cf6e8`; published engine 1.1.0 is revision
`6c2a867cbc7587922cb3973a7505746116caa54c`.

The baseline is 28 entries, 81 catalog cases, three worked chapters, and 13
chapter checks. All entries have the 16 numbered fields. Field presence does not
establish complete or accurate content. Chapter completeness is separate from
whole-feature inventory completeness.

Read alongside [REQ-126 acceptance criteria](../FEATURE_REQUESTS.md#req-126-xpath40-feature-catalog--gof-style-catalog-of-xpathxslt-40-syntax-decisions),
the [REQ-118 adoption dossier](../REQ-118-xpath-xslt-40.md),
and the [catalog index](README.md).

## Acceptance assessment

| Criterion | Assessment | Evidence and remaining work |
|---|---|---|
| AC-01: landed slices, tiers, spec and delivery links | Partial | S1/S2/S5 function families are largely absent. Most newer entries cite families/tests instead of exact spec sections and delivery PRs. Only destructuring currently links a concrete Bosak delivery PR directly. |
| AC-02: deferred/rejected decisions | Partial | Method-call and renamed mapping-arrow entries exist; several moving and rejected families from dossier section 1.2 have no dedicated disposition record. |
| AC-03: verified examples and gate evidence | Partial | 94 existing executable documentation cases are retained. Most Known Uses fields cite unit/example tests without named qt4 set status. One concrete stale claim, fn:scan gate status, is corrected with this audit. |
| AC-04: structure, retained entries, drift warnings | Partial | 28 entries have all numbered fields and index links. No entry is removed by this audit. Generic prior-art disclaimers do not replace feature-specific drift explanations; supplementary forms and historical lifecycle need review. |
| AC-05: discoverability and canonical links | Met for current scope | README roadmap/catalog links, integration, and the REQ-118 reference companion exist; registry and index remain In Progress. This does not close other criteria. |

## Runtime function inventory

The [machine-readable snapshot](audit-function-inventory.json) contains the
published engine's public `FunctionLibrary.XPath40OnlyFunctionNames` and
`XPath40ExperimentalOnlyFunctionNames` sets. These are namespace/local-name
pairs, **not function signatures or arities**.

| Registered category | Names | Catalog assessment |
|---|---:|---|
| Frozen `fn:*` | 58 | Twelve frozen fn/array names together have an identifiable existing family entry; many functions remain without one. |
| Frozen `map:*` | 4 | No dedicated map-function family entry. Map iteration syntax is a different feature. |
| Frozen `array:*` | 5 | `array:sort-with` belongs to the existing sorting family; other registered array families need entries. |
| Frozen zero-argument `xs:*` constructors | 49 | One constructor-family entry exists, with an integer example; this does not verify all constructor contracts. |
| Experimental `fn:scan` | 1 | Entry and verified examples exist; gate status was stale. |
| Total | 117 | 62 names map to an existing family entry; 55 have no dedicated family entry. A mapped name is not a completeness pass. |

Names mapped to existing non-constructor entries are `fn:atomic-equal`, the three
CSV functions, `fn:element-to-map`/`fn:element-to-map-plan`, `fn:map-to-element`,
`fn:sort-with`/`array:sort-with`, `fn:jtree`/`fn:jkey`/`fn:jvalue`, and experimental
`fn:scan`. Some are only mentioned within a broader family, without their own
worked form. The snapshot records a matching gate identifier only when that
identifier actually occurs in the runner's GatedSets; null means no exact-name
match was assigned, not proof that no other test set covers the function.

The snapshot does **not** inventory changed behavior/overloads of existing 3.1
functions, parser grammar, type syntax, XSLT instructions, or resource routes.
For example, upgraded `fn:compare`, QName `min`/`max`, changed map/array overloads,
and collations require an additional semantic/signature audit. The missing-name
count is not a count of missing Markdown files: related functions can share a
well-explained family entry.

### Missing function families

| Family | Registered names without a dedicated family entry | Suggested realistic case |
|---|---|---|
| Sequence selection and repetition | `fn:replicate`, `fn:slice`, `fn:items-at`, `fn:foot`, `fn:trunk`, `fn:insert-separator` | Paginate an order export while retaining boundary records. |
| Equality, duplicates, subsequences | `fn:all-equal`, `fn:all-different`, `fn:duplicate-values`, `fn:contains-subsequence`, `fn:starts-with-subsequence`, `fn:ends-with-subsequence` | Check shipment-event identifiers and required processing steps. |
| Higher-order processing | `fn:some`, `fn:every`, `fn:index-where`, `fn:partition`, `fn:take-while`, `fn:drop-while`, `fn:while-do`, `fn:do-until`, `fn:partial-apply`, `fn:transitive-closure`, `fn:identity` | Route invoice lines, bound retries, and follow product dependencies with cycles. |
| Ranking, operators, type inspection | `fn:highest`, `fn:lowest`, `fn:sort-by`, `fn:op`, `fn:is-NaN`, `fn:atomic-type-annotation` | Prioritize work orders and describe imported measurements. |
| Strings and Unicode | `fn:char`, `fn:characters`, `fn:graphemes`, `fn:pad-string`, `fn:trim-space`, `fn:index-of-substring`, `fn:substring-before-last`, `fn:substring-after-last`, `fn:hash` | Normalize supplier references without splitting Unicode graphemes. |
| URI and collation | `fn:parse-uri`, `fn:build-uri`, `fn:decode-from-uri`, `fn:collation-available` | Construct and validate document-resource identifiers. |
| Dates and durations | `fn:seconds`, `fn:duration-to-seconds`, `fn:build-dateTime`, `fn:unix-dateTime`, `fn:days-in-month` | Convert timestamps and build a monthly dispatch calendar. |
| Maps | `map:build`, `map:entries`, `map:filter`, `map:items` | Index a product feed with an explicit duplicate-key policy. |
| Arrays | `array:build`, `array:empty`, `array:items`, `array:slice` | Preserve record boundaries when splitting import batches. |

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
| REQ-123 operator/CSV/conversion clusters | CSV/conversion entries exist | Add fn:op; expand plan-driven conversion and supplementary function forms. |
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
| Method-call `=?>` | Dedicated Deferred entry | Preserve the churn rationale and exact refused shape. |
| Nominative record types | Missing | Add a decision entry distinct from shipped structural records. |
| Iteration over sequences of arrays/maps | Missing | Establish the actual implemented boundary before assigning Deferred or Shipped. |
| fn:scan | Experimental entry | Historical deferral is superseded by experimental adoption; gate status corrected here. |
| JNode built-in template rules and full GNode/JNode model | Partial navigation entry | Reconcile historical deferral with delivered JNode support; distinguish the remaining model and template gaps. |
| `fn:all` / `fn:some-of` | Missing dedicated rejection family | Record never-existed names and accepted alternatives. |
| `scan-left` / `scan-right` | Missing dedicated rejection family | Record consolidation into experimental fn:scan, including direction semantics. |
| Old mapping arrow `=>>` | Dedicated rejected entry | Retain renamed disposition and link to `=!>`. |
| XPath switch expression | Missing dedicated rejection family | Distinguish XPath from XSLT xsl:switch and XQuery switch. |
| Extensible map types | Missing dedicated rejection family | Record dropped shape and the adopted record/map rules. |

The wider dossier also defers computed XPath node constructors, XSLT type
declarations, enclosing modes, capturing accumulators, fn:apply-templates,
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
| P2 | Add map/array, Unicode, URI/date, operator/type families | Cover all 55 unassigned registered names through coherent families, including signature-specific behavior. |
| P2 | Expand syntax and changed-3.1 semantics | Literals, focus functions, guarded expressions, record updates, comparisons/collations, patterns, and supplementary forms. |
| P3 | Reconcile acceptance and publish the complete reference | Zero unassigned adopted features/decisions, accurate index/tier links, verified examples, and explicit exceptions to conformance evidence rules. |

**Recommended next implementation step:** P1 evidence repair and decision records.
Then begin the sequence/HOF families with one complete order-routing case. This
avoids presenting a larger catalog as complete while its existing evidence is
still generic. No engine work or conformance-set promotion is required here.

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
[release notes](../RELEASE-NOTES-1.1.md); this audit does not rerun those engines.
