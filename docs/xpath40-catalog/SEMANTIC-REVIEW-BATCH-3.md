<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak third nineteen-entry semantic and applicable-evidence review">
  <h1>Semantic and applicable-evidence review — C11</h1>
  <p>26 October 2026 · batch 3 of 4 · published Bosak 1.26.3</p>
</div>

**C11 completes the prescribed 19-entry review with 56 contract-group receipts. Two closure workpackages remain open.** [The machine report](SEMANTIC-REVIEW-BATCH-3.json) records exact examples, profile/context requirements, source/test mappings, language drift and remaining blockers. Completion of the review does not unconditionally accept every contract or close REQ-126.

## Evidence and results

All **3,756 fresh existing tests pass**: 3,148 catalog cases, 323 selected API tests and 285 library tests. The batch maps **874 distinct exact cases**; their **764 distinct fixture/input files** and the complete manifest match compiled test assets byte-for-byte. Per-function C02 normal/empty/error evidence and C04/C05 grammar receipts remain explicit, alongside current execution. Representative coverage of all 49 zero-arity constructors is not a claim about every lexical value, facet, cast or extended-year operation.

All **26 Sample Code blocks** have explicit verification paths. The destructuring and old-arrow pages combined independent queries in single blocks; they now use separate fences and clear compatibility contexts. **All seven inline queries execute verbatim**, including deliberate errors. The destructuring comment now displays the flat `(1,2,3)` result while explaining that the last bound variable holds `(2,3)`. Ordinary-map missing-key behavior is distinguished from the annotated-record lookup defect. Epoch prose now distinguishes frozen nonnegative inputs from delivered Experimental signed epochs. Mapping-arrow prose retains the containing-focus defect and explicit-for alternative. [Five before/after corrections](evidence/c11-semantic-batch3/documentation-corrections.json) preserve the originals.

The **48 supplemental expressions produce 99 observations: 77 matching controls, 20 unmet current-draft controls and two URI type-coercion observations**. No required expectation is weakened to match the released result:

| Required boundary | Released observation under both 4.0 API profiles | Existing ownership |
|---|---|---|
| Empty structured query map takes precedence over raw query | Raw query retained | C02 F04 |
| Empty sequence and array sort key use defaults | `XPTY0004` | C02 F05 |
| Unknown sort collation refuses `FOCH0002` | Accepted | C02 F05 |
| Exact mixed-number sort order | Promoted tie retains double before decimal | C02 F06 |
| NaN-first input remains in the required NaN-first order | Returns number before NaN; opposite input order passes | C02 F06 |
| Annotated-record undeclared-field destructuring refuses | Empty result | C05 K03 |
| Keyword placeholder partial application | `XPST0003`; positional placeholder alternative passes | C04 G03 |
| Mapping extra arguments retain containing dot | Mapped-item dot observed | C04 G02 |
| Current `otherwise` spelling | `XPST0003`; released `??` behavior remains tested | C04 G01 |

The two URI diagnostics retain unsupported string-valued Boolean behavior without inventing a required coercion/error classification. Current grammar spelling, frozen API behavior and historical adoption evidence remain separate. Passing a frozen gate or one NaN order does not establish the unmet contracts.

## Per-entry receipts

| Entry | Receipt | Applicable evidence and remaining limit |
|---|---|---|
| [functions/timestamp-deadlines.md](functions/timestamp-deadlines.md) | Verified bounded epoch/duration profiles; 3 groups/67 cases | fn-unix-dateTime; Signed epoch support is Experimental only; scalar/cardinality tolerance and bounded DateTimeOffset calendar do not certify arbitrary extended-year operations. |
| [functions/uri-option-policy.md](functions/uri-option-policy.md) | Limited URI options; query precedence gap; 3 groups/84 cases | Named tests/refusals/alternatives; no exact gate assignment; C02-F04: empty structured query map falls back to raw query; unsupported string Booleans remain a type-validation limit. |
| [functions/warehouse-pick-sorting.md](functions/warehouse-pick-sorting.md) | Blocked for full default-key/collation/order parity; 3 groups/100 cases | array-sort, fn-sort; C02-F05/F06: explicit empty-key defaults, unknown-collation refusal and numeric ordering remain incomplete; passing one NaN input order is not complete ordering support. |
| [operators-and-syntax/array-member-iteration.md](operators-and-syntax/array-member-iteration.md) | Verified bounded member binding; 3 groups/3 cases | Named tests/refusals/alternatives; no exact gate assignment; No complete iteration/coercion/type grammar certification; C05 retains the remaining combinations. |
| [operators-and-syntax/destructuring-let.md](operators-and-syntax/destructuring-let.md) | Limited bindings; annotated-record blocker; 4 groups/2 cases | fn-compare, prod-LetClause; C05-K03: annotated-record unknown-field destructuring incorrectly returns empty instead of XPTY0004; broader type/choice coercion limits remain. |
| [operators-and-syntax/focus-function-syntax.md](operators-and-syntax/focus-function-syntax.md) | Verified bounded focus-function syntax; 3 groups/30 cases | Named tests/refusals/alternatives; no exact gate assignment; No arbitrary callback/constructor/package context acceptance; C04 mapping-extra-focus and C06 host/static-mode limits remain distinct. |
| [operators-and-syntax/focus-functions.md](operators-and-syntax/focus-functions.md) | Verified representative49-constructor boundary; 3 groups/511 cases | misc-FocusConstructors, xs-normalizedString, xs-numeric, xs-token; C03 does not certify all facets or every generic cast/extended-year operation; fn focus syntax is a separate entry. |
| [operators-and-syntax/keyword-arguments.md](operators-and-syntax/keyword-arguments.md) | Limited keywords; placeholder blocker; 3 groups/3 cases | Named tests/refusals/alternatives; no exact gate assignment; C04-G03 and C02-F01: keyword placeholders and per-function label/metadata gaps remain; not a universal dynamic-call keyword facility. |
| [operators-and-syntax/map-entry-iteration.md](operators-and-syntax/map-entry-iteration.md) | Verified bounded key/value binding; 3 groups/3 cases | Named tests/refusals/alternatives; no exact gate assignment; Full typed/default/context iteration combinations remain C05 limits. |
| [operators-and-syntax/mapping-arrow.md](operators-and-syntax/mapping-arrow.md) | Limited mapping; containing-focus blocker; 3 groups/3 cases | Named tests/refusals/alternatives; no exact gate assignment; C04-G02: containing dot/position/size semantics remain unmet; unary examples cannot certify this boundary. |
| [operators-and-syntax/method-call-operator.md](operators-and-syntax/method-call-operator.md) | Verified deferred decision and alternative; 2 groups/2 cases | Named tests/refusals/alternatives; no exact gate assignment; No method dictionary or nominal/union dispatch implementation or future tier commitment follows. |
| [operators-and-syntax/numeric-literals.md](operators-and-syntax/numeric-literals.md) | Verified bounded literal grammar; 4 groups/50 cases | Named tests/refusals/alternatives; no exact gate assignment; Delivered malformed-separator fix remains closed; literals do not create arbitrary-precision decimal arithmetic or unrestricted generic casts. |
| [operators-and-syntax/otherwise-operator.md](operators-and-syntax/otherwise-operator.md) | Limited released spelling; current grammar mismatch; 3 groups/3 cases | Named tests/refusals/alternatives; no exact gate assignment; C04-G01: retain frozen spelling and historical adoption uncertainty separately from the current review; no automatic rename/adoption. |
| [operators-and-syntax/pipeline-operator.md](operators-and-syntax/pipeline-operator.md) | Verified bounded pipeline focus; 3 groups/3 cases | Named tests/refusals/alternatives; no exact gate assignment; No universal callback/type-coercion or host/profile propagation acceptance. |
| [operators-and-syntax/sequence-collection-iteration.md](operators-and-syntax/sequence-collection-iteration.md) | Verified bounded multi-container iteration; 3 groups/3 cases | Named tests/refusals/alternatives; no exact gate assignment; Not a full generic collection/type-model promotion. |
| [operators-and-syntax/string-templates.md](operators-and-syntax/string-templates.md) | Verified bounded template contract; 3 groups/3 cases | Named tests/refusals/alternatives; no exact gate assignment; No separate triple-quote language or arbitrary interpolation/coercion certification. |
| [rejected/directional-scan.md](rejected/directional-scan.md) | Verified rejected names; 2 groups/2 cases | Named tests/refusals/alternatives; no exact gate assignment; No right-scan equivalence is inferred from reversing sequences. |
| [rejected/extensible-map-types.md](rejected/extensible-map-types.md) | Verified rejected proposal and record alternatives; 2 groups/2 cases | Named tests/refusals/alternatives; no exact gate assignment; Keep structural membership, cast, coercion and annotation-aware lookup separate; C05 defects remain. |
| [rejected/mapping-arrow-gtgt.md](rejected/mapping-arrow-gtgt.md) | Verified rejected spelling and bounded replacement; 3 groups/2 cases | Named tests/refusals/alternatives; no exact gate assignment; This proves Bosak refusal/replacement scope, not an exact external rename/adoption history or corrected mapping-extra focus. |

Each contract links exact case IDs, supplemental controls, named tests or inherited grammar/function receipts. Missing gate assignments use appropriate refusal/alternative or engine evidence; no gate is fabricated or promoted. The current 224-set list is captured as source evidence, not a new conformance run. Deferred method dispatch and rejected spellings/names remain unimplemented; accepted alternatives do not promote those shapes.

## Preservation and reproduction

Current source review is immutable `b4723ff52fd9d4fabf2b4cd33bd44f21939c512c`, with [exact function and conditional grammar fragments](evidence/c11-semantic-batch3/review-sources.json). It is not a recovered adoption snapshot. C08's approved legacy policy remains restricted to its original 28 records; no semantic defect is waived here.

TRX arguments may be truncated. Candidate matches retain suffix information; full enumeration, unique result IDs, all-Passed outcomes and exact manifest/asset identity establish execution without fabricating unique identities for ambiguous display prefixes. Source snapshots and initial probe/serialization corrections remain separate from final requirements.

Engine, tests and executable fixtures remain identical to published source `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f`; documentation base is `00ee9bf`. C02's **21,798 full solution passes** and frozen QT3 **31,142/0/679** and gated qt4 **8,436/0/1,894** remain inherited evidence. No new full-suite or manual gate execution is claimed. No engine, fixture, profile, filter, version or release change follows. The catalog remains 76 entries and 3,148 feature +24 chapter checks.

```powershell
python docs/xpath40-catalog/build-c11-review.py
python docs/xpath40-catalog/evidence/c11-semantic-batch3/validate-evidence.py --local-artifacts
```

[The manifest](evidence/c11-semantic-batch3/manifest.json) protects original snapshots, corrections, case/TRX results, assets, source identities and exact controls. C00–C10 raw evidence and historical plans remain unchanged. HTML uses unchanged Fytala CSS; no screenshot/PDF acceptance is claimed. Next is C12's final 19-entry batch; C13 retains final owner acceptance and blocker resolution. Skip fixes, Braid and extension work remain deferred.
