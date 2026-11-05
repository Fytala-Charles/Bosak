<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak constructor type and coercion reconciliation">
  <h1>Constructor, type and coercion reconciliation — C03</h1>
  <p>26 October 2026 · published Bosak 1.26.3</p>
</div>

**REQ-126 C05 complete — 26 October 2026:** [Type/collection/pattern review](TYPE-COLLECTION-PATTERN-RECONCILIATION.md) reconciles nine type/collection/pattern rows and 49 selected production names. All 2,457 focused tests pass. Of 275 independent controls, 259 match; 16 retain choice-coercion, record-destructuring and known generic-model gaps. Delivered union/EQName fixes remain closed. Eight closure workpackages remain; C06 is next. No engine or executable example changes, acceptance waiver, profile/gate promotion or release. Skips, Braid and extension work stay deferred; earlier checkpoints are historical.

**REQ-126 C04 complete — 26 October 2026:** [Expression/call/focus review](EXPRESSION-CALL-FOCUS-RECONCILIATION.md) reconciles six expression/call/focus rows and 33 selected production/token names. All 880 focused tests pass; 209 controls match, ten retain current-draft spelling/placeholder/mapping-focus gaps, and eight late-binding observations remain separate. Nine closure workpackages remain; C05 is next. No engine or executable example changes, gate/profile promotion, acceptance waiver or release. C02's atomic-annotation defect remains open. Skips, Braid and extension work stay deferred; earlier checkpoint counts are historical.

**C03 completes the bounded review of eight entries and all 49 registered focus-constructor names. Ten closure workpackages remain open.** This is reconciliation of supported forms, evidence and remaining boundaries; no acceptance criterion closes. [The machine-readable report](CONSTRUCTOR-TYPE-RECONCILIATION.json) and [explicit decisions](C03-REVIEW-DECISIONS.json) preserve that distinction. C02's `atomic-type-annotation(())` defect remains open with its independent failing expectations.

## Fresh evidence

The constructor matrix supplies **1,127 observations**, 23 per name: direct calls, named references, lookup calls, node atomization, explicit one-argument comparison, empty argument, empty iteration, missing focus, old-profile refusal, representative invalid input and type checks. Applicable paths run under XPath40/Experimental; one-argument controls also run under XPath31. All 49 zero-arity names refuse XPath31. Missing focus is `XPDY0002`; an empty iteration invokes nothing, while an explicit empty argument to the one-argument constructor returns empty. These are different contracts. `xs:error` has no nonempty successful value: empty/type checks do not establish a constructed value of that type.

The broader run covers **631 XPath expressions /1,337 observations** and **10 stylesheets /20 host observations**. All **1,333 asserted controls match**; **24 additional observations** retain representation, focus-capture, BC and static-field refusal boundaries without counting them as conformance passes. The focused existing suites pass **245 tests**: 70 schema/list/union/typed-value, 119 record/literal/API version-gate, and 56 host/nominal/report checks. No engine or test source changes were needed.

The unchanged engine and test files are identical to published source `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f`; documentation base is `cc6495c3`. C02's **21,798 full solution passes and 3,148 catalog passes** remain inherited evidence, not a claimed fresh full-suite run in C03. Frozen QT3 **31,142/0/679** and gated qt4 **8,436/0/1,894** remain inherited C00 evidence. The catalog still has 76 entries and 3,148 feature +24 chapter checks.

## Contract reconciliation

| Surface | Verified boundary and remaining scope |
|---|---|
| Focus constructors | All 49 representative families pass the named paths. Direct/ref/lookup focus is tested; the extra capture probes return the creation focus `12` when invoked with focus `99`. No complete namespace, schema, facet or context-capture proof is inferred. |
| Numeric representation | Wide literals retain integer identity, but generic wide `xs:integer` constructor/cast paths still narrow and report `FORG0001`; castable is false. A valid wide literal therefore does not imply a matching constructor range. Decimal precision remains bounded. |
| Delivered numeric corrections | Floating signed/unsigned overflow guards, unsigned truncation, arithmetic/unary/modulo fixes and integer-preserving numeric functions remain delivered. No old wrong threshold/fractional-unsigned result is reopened as a current defect. Generic representation and error policy remain separate. |
| Schema lists/unions | `xs:NMTOKENS` constructs item sequences and its callable matches the tested sequence-returning function signature. Using a list name as an ordinary item-test target refuses `XPST0051`. Custom schemas, unions and QName members rely on supplied schema/type/namespace context; passing schema tests do not imply globally available imported types or a complete XSD loader. |
| Choice and enum types | Membership is distinct from coercion. Ordered integer/string alternatives produce the tested different coercions for untyped input; enum membership is case-sensitive and typed binding rejects invalid values. Complete derived/schema/function/record alternatives remain outside these examples. |
| Structural records | Cast discards surplus keys and performs field casts; coercion rejects surplus keys and recursively coerces declared fields. Optional absent fields become present empty entries. A plain map is not an annotated record. `record(*)`, declaration order and lookup versus `map:get` retain their distinct checks. |
| Record updates | Reconstruction retains the annotation and leaves the original record available. Unknown fields, noncoercible values and invalid operands refuse. A string import cast is not the same as update coercion. All nominal/static/package update combinations are not certified. |
| Nominal records | Host-defined identity differs from field shape. Both Legacy and Xslt40Draft hosts, with version-4 stylesheets, support the tested constructor/coerced-map identity, select/body/trailing defaults, and static constructor/coercion values. An external static-field override refuses `XTTE0570` in the bounded observation. Package visibility/identity, recursion and global-initializer/error timing still need separate evidence. |
| Function signatures and BC | Typed parameter/result promotion, invalid result refusal and selected function wrappers pass. Modern unary string refusal differs from BC conversion. The runner explicitly sets both compile-time and runtime BC flags; this is not a claim about every XSLT host, arbitrary function variance or runtime-only propagation path. |

The nominal default/static probes improve the documentation's earlier unclaimed scope: these specific supported forms are now evidenced. They do not promote the entire historical deferred `xsl:record-type` proposal or claim that every default/static-field combination works. The source examples specify complete stylesheets and host profiles; a bare XPath expression does not register a host-defined type.

## Source policy and remaining ownership

Review uses pinned qtspecs **b4723ff52fd9d4fabf2b4cd33bd44f21939c512c**: [expression/type/coercion rules](https://github.com/qt4cg/qtspecs/blob/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xquery-40/src/expressions.xml), [grammar](evidence/c03-type-review/current-xpath-grammar.xml), [XSLT named records](https://github.com/qt4cg/qtspecs/blob/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xslt-40/src/xslt.xml), and C01's matching F&O catalog. The expression/grammar bytes match the retained record-review source hashes; XSLT source is captured for this review. Current review does not supply any of C08's 28 missing historical adoption pins, change profiles, or adopt every current draft feature.

| Record | Owner and disposition |
|---|---|
| T01 — generic integer range/error policy | Core/runtime maintainer and C13 owner disposition; distinguish storage, lexical, overflow and facet boundaries rather than replacing errors globally. |
| T02 — decimal precision | Documented numeric-storage boundary; arbitrary precision is not delivered. |
| T03 — schema/type/namespace context | Named evidence retained; C06 owns host/context integration and C09–C12 final applicable evidence. No skip is removed. |
| T04 — composite/nominal record context | Bounded defaults/static forms are supported; C05/C06 and final entry review retain package, recursion and remaining combination coverage. |
| T05 — generic signature/focus/BC propagation | C04/C06 and final entry review; selected passing paths do not certify all contexts or conversions. |
| C02 F08-atomic-annotation-empty | Still an unfixed, unaccepted correction ticket. No C03 result changes its required-input failure expectations. |

These records are limits and review ownership, not a promise to implement every proposal or an automatic waiver. C09–C12 retain per-entry semantic/applicable-evidence review and C13 retains explicit owner acceptance. No new engine defect was reproduced by the C03 asserted controls.

## Reproduction and preservation

Run `python docs/xpath40-catalog/build-c03-review.py`, `python docs/xpath40-catalog/build-closure-plan.py` and `python docs/xpath40-catalog/evidence/c03-type-review/validate-evidence.py`. Preserved requests, exact outcomes, named tests, focused logs, source/binary hashes and HTML receipts make the review reproducible. The initial focus-capture sequence-display observations and the explicit display normalization are retained separately; no engine expectation was altered.

Original C00/C01/C02 evidence bytes remain intact, and their closure plans reproduce in isolation. Engine, APIs, tests, examples, version, fixture/filter/gate policy, kit, upstream and Braid remain unchanged. Next is **C04: expression, call and focus grammar deltas**. Skip fixes, Braid and extension work remain deferred until REQ-126 closure work finishes; no release follows from this review.
