<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak expression call and focus grammar reconciliation">
  <h1>Expression, call and focus reconciliation — C04</h1>
  <p>26 October 2026 · published Bosak 1.26.3</p>
</div>

**C04 completes the bounded review of six grammar rows, 33 selected production/token names and nine structured entries. Nine closure workpackages remain open.** This closes review scope, not the discovered gaps, full grammar coverage or AC-01–04. [The machine-readable report](EXPRESSION-CALL-FOCUS-RECONCILIATION.json) preserves expected outcomes and remaining ownership. C05's nine grammar rows and C06's three host/profile rows remain separate; C09–C12 and C13 retain semantic/applicable-evidence review and owner acceptance.

## Evidence and outcomes

All **880 focused existing tests pass**: 289 API/version/literal, 202 parser, and 389 selected host/catalog checks. The added **108 expressions produce 227 profile observations**. Of **219 asserted controls, 209 match and ten retain unmet draft expectations**. Eight further observations describe public-API late binding and are not counted as conformance passes. Recorded phases distinguish compilation, evaluation and success without imposing a universal phase on dynamic/type errors.

Engine and test files remain identical to published 1.26.3 source `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f`; documentation base is `bdd303d0`. C02's 21,798 full solution passes and 3,148 full catalog passes remain inherited evidence, as do C00's frozen QT3 **31,142/0/679** and gated qt4 **8,436/0/1,894**. No fresh full-suite or conformance run is claimed in C04, and no gate/profile promotion follows from these checks.

## Reviewed productions and boundaries

| Row | Current disposition |
|---|---|
| S1–S2–S5 | Named/ref/dynamic function-call and HOF argument/profile forms are mapped. Function-specific defaults, metadata, options and callback conversion retain C01–C03 evidence and limitations; registrations do not prove grammar completeness. |
| S3-otherwise | Released `??` passes empty/present-value, full-sequence, precedence, containing-focus and guarded dynamic-error controls. Pinned `OtherwiseExpr` uses the word `otherwise`, which Bosak rejects. |
| S3-keywords | Reordered/mixed arguments, default/empty forms, `Q{}` keyword names and static rejection controls pass. A bare `?` in a keyword argument fails parsing. Dynamic calls retain positional-argument grammar; their keyword refusal is distinct from missing metadata in a named call. |
| S3-lexical | Radix/separator boundaries and template nesting/escaping/comments/atomization/error/profile forms are mapped. C03's representation limits remain; these probes do not certify every adjacency, Unicode digit, XQuery string constructor or host/AVT combination. |
| S4-focus | Pipeline whole-value/fixed focus, implicit/explicit focus functions, lexical capture and restoration pass. Named mapping-arrow extra arguments incorrectly observe mapped-item focus. Zero-arity constructors retain C03's separate 49-name evidence. |
| REQ123-scan | Direct calls and lookup remain Experimental-only. Normal and empty state-array results pass. Existing `fn-scan` gate membership/retry does not promote it into the frozen API. |

The [explicit decisions](C04-REVIEW-DECISIONS.json) map each row to retained source/tests/examples, current grammar fragments, expression sections, new controls and residual dispositions. Only these six rows receive a current review annotation in the 18-row [grammar matrix](GRAMMAR-COVERAGE-MATRIX.json); its original baseline fields and the other twelve rows remain historical/unreviewed by C04. Existing examples are retained rather than restarted or relabeled as exhaustive coverage.

## Unmet contracts and alternatives

**G01 — otherwise spelling.** The pinned grammar and expression section use `A otherwise B`; released Bosak uses `A ?? B`. Both `() otherwise 4` and the guarded-word form refuse `XPST0003` in both 4.0 profiles, accounting for four unmet controls. The historical adopted revision is still unavailable, so this review does not invent provenance or silently adopt a new spelling. Use the documented released form or an explicit `if (exists(...))` alternative. A correction needs an explicit compatibility decision and must preserve contextual element names, precedence, guards and lower-profile behavior.

**G02 — mapping-arrow containing focus.** The pinned static-call definition is `for $u in U return F($u,A,...)`, which retains the containing focus for extra arguments. Bosak lowers to `U ! F(.,A,...)`, which changes it. With containing focus `100`:

```xpath
string-join((100 ! string-join((1,2) =!> concat(':', string(.)), '|')), '|')
```

The required result is `1:100|2:100`; the released result is `1:1|2:2`. Position/size similarly produce `1:1/2|2:2/2` instead of `1:1/1|2:1/1`. These are four unmet controls across the two profiles. The independently tested `for $u in ... return concat($u,...)` alternative retains containing focus. Use it when extra arguments depend on dot, position or size. Context-independent mapping examples remain valid; this is not a claim that focus-function bodies should stop receiving their argument as focus.

**G03 — keyword placeholders.** The current `KeywordArgument` production uses `Argument`, which includes `ArgumentPlaceholder`. `substring(value := ?, start := 2, length := 2)` should create the illustrated partial function, but raises `XPST0003` in both profiles, adding two unmet controls. Positional `substring(?,2,2)` works in all four tested profiles. The bounded correction must cover placeholder parsing, residual arity, defaults, collisions and binding rather than merely accepting a token.

All ten independent expected outcomes remain unchanged. G02 and G03 have separate correction records; G01 retains an adoption/compatibility decision. Engine-touching fixes require the mandatory frozen gates before merge. C04 does not implement or accept these gaps, and C02's atomic-annotation empty-input defect remains open.

## Guarding and host binding

Unselected dynamic errors in the tested fallback/conditional/and/or/empty-map/filter forms are suppressed; selected dynamic errors propagate. Known keyword-binding errors and malformed syntax are still reported during compilation even in an unselected fallback. Guarding does not remove the specification's obligation to report static errors in a closed static context.

The public API is different from a proved closed host: an expression can compile before its runtime variable/function context is supplied. The new controls supply `$missing` or the tested runtime function after compilation and obtain `7`. Without a supplied context, standalone references fail at evaluation, while an unselected guarded reference is omitted. These observations do not certify closed XSLT/XQuery binding or establish a new static-error defect. **G04** leaves that host question to C06. The initial probe draft assumed closed binding incorrectly; its requests/results and separate corrections remain preserved. Singleton-sequence display normalization is likewise recorded separately from engine semantics.

## Sources, preservation and next work

Review uses qtspecs **b4723ff52fd9d4fabf2b4cd33bd44f21939c512c**, with the matching expression and grammar bytes retained by C03. The report preserves the actual selected XML production/token fragments and conditions; C01's F&O metadata and C02/C03 semantic evidence remain distinct inputs. No current review pin recovers C08's 28 missing historical adoption revisions. **G05** retains unaccepted lexical/type/focus/host combinations for C05/C06 and final entry review; there is no promise to implement every draft addition.

Run `python docs/xpath40-catalog/build-c04-review.py`, `python docs/xpath40-catalog/build-closure-plan.py` and `python docs/xpath40-catalog/evidence/c04-expression-review/validate-evidence.py`. Exact requests, results, source identities, focused logs, HTML renders and isolated historical plan reproduction are retained. A C03 prose encoding error is corrected without changing findings; its original Git identity and rendered evidence remain preserved.

No engine, API, test, executable example, version, fixture/filter/gate, kit, upstream or Braid change occurs. Next is **C05: type, collection and pattern grammar deltas**. Skip fixes, Braid and extension work remain deferred until REQ-126 closure work finishes; no release follows from this review.
