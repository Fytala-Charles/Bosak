<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak type collection and pattern grammar reconciliation">
  <h1>Type, collection and pattern reconciliation — C05</h1>
  <p>26 October 2026 · published Bosak 1.26.3</p>
</div>

**C05 completes the bounded review of nine grammar rows and 49 selected production names. Eight closure workpackages remain open.** Sixteen retained pages and five linked type/pattern pages supply 17 structured entries plus four audits. [The report](TYPE-COLLECTION-PATTERN-RECONCILIATION.json) maps source, controls, known model limits and correction ownership. This closes review scope, not defects, full grammar/model coverage or AC-01–04.

## Evidence and results

All **2,457 focused existing tests pass**: 323 API/version/record/constructor, 89 JNode/array-member and 2,045 selected XSLT checks. New probes cover **111 XPath expressions /219 observations** and **28 stylesheets /56 host observations**. Of **275 asserted controls, 259 match and 16 remain unmet**: six choice-coercion checks, two record-destructuring checks and eight known generic-model boundaries. These unmet controls are retained with their independent expected results; they are not passing conformance examples.

The host probes use version-4 stylesheets under both Legacy and Xslt40Draft. Ordinary map/array/record/alias patterns, set grouping, union priority and EQName controls pass. The two explicit JNode-pattern probes refuse compilation. Exact source inputs, host/profile, compile/evaluation phase and structured `fn:error` codes are preserved. Initial authoring errors assigned three retained 3.1 refusals to 4.0 profiles; their original records and corrections remain separate. Error-code capture was improved without changing an engine expectation.

Engine/test/executable-example files remain identical to published source `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f`; documentation base is `2d8a7005`. C02's **21,798 full solution passes and 3,148 catalog passes**, and C00's frozen QT3 **31,142/0/679** and gated qt4 **8,436/0/1,894**, remain inherited evidence. No fresh full-suite or conformance run is claimed in C05. Catalog totals remain 76 entries and 3,148 feature +24 chapter checks.

## Production and contract reconciliation

| Row | Verified boundary and remaining scope |
|---|---|
| S4-iteration | Array members and map values remain whole sequences; positional counting continues across container sequences. Empty input, nested/member boundaries, containing focus and invalid-container errors are checked. Map iteration is not portable business ordering. |
| S6-types | Membership, explicit cast and coercion differ. Explicit casts try alternatives in order; coercion should first honor an alternative the input already matches. The latter has the K02 mismatch below. Aliases require their host/type context. |
| S6-record-conversion | C03 cast filtering versus coercion rejection, optional present-empty keys, recursive promotion, annotation and lookup boundaries remain supported in the tested forms. No universal nominal/composite/package conversion follows. |
| S6-update | Updates reconstruct under the existing annotation and retain the original value. Unknown fields and noncoercible replacements refuse; structural examples do not certify all static/default/nominal updates. |
| S8-patterns | Closed/open map keys, absent versus empty entry values, member-sequence cardinality, vacuous empty arrays, nested arrays, aliases and nominal/structural identity pass bounded host probes. Pattern matching does not perform constructor or binding coercion. |
| S8-compound | Grouped versus left-associative set matching passes. Delivered union-minimum priority, EQName URI-punctuation priority and bracket parsing pass; stale earlier limitation notices remain historical. Earlier-profile dispatch/next-match combinations retain separate named evidence. |
| REQ123-constructors | C03's 49-name/1,127-observation audit is retained. C05 spot checks direct/reference, missing/empty focus and old-profile refusal; constructor coverage does not establish all type grammar or generic casting. |
| REQ123-destructuring | Sequence final-variable remainder, short/empty input, duplicate names, per-variable/whole-pattern types, array groups/surplus/short errors, ordinary missing map keys and lexical shadowing are checked. Annotated-record missing fields expose K03. |
| REQ123-jnode | Navigation, parent/root/key/value tests and JNode member conversions work in the tested forms. Generic `node()`/`xnode()`/identity and explicit JNode patterns remain outside delivered model support. |

The nine rows receive new annotations in the [grammar matrix](GRAMMAR-COVERAGE-MATRIX.json). C04's six annotations remain unchanged, giving **15 bounded reviewed rows out of 18**. C06 owns the other three host/declaration/profile rows. Neither this count nor 49 selected names means the whole XPath/XSLT grammar is accepted.

## Choice coercion and record destructuring findings

**K02 — an already-matching choice alternative.** The pinned item-coercion rule first selects an alternative the item already matches. For example:

```xpath
let $v as (xs:integer|xs:string) := '12'
return $v instance of xs:string
```

The required result is `true`; Bosak returns `false` after converting the string to integer. Typed function arguments and return annotations reproduce the same mismatch in both 4.0 profiles, giving six unmet controls. This is distinct from explicit `'12' cast as (xs:integer|xs:string)`, whose first successful cast is integer, and from C03's untyped-input coercion controls. K02 requires separate cast/coercion handling and preservation of matched alternatives before ordered fallback. No change to the engine or its earlier regression expectations is made here.

**K03 — destructuring an annotated record.** Ordinary missing map keys bind empty, but current record lookup rules reject undeclared fields:

```xpath
let ${$missing} := (map{'a':1} cast as record(a))
return $missing
```

The required result is `XPTY0004`; Bosak returns empty in both 4.0 profiles. Existing direct unknown-field lookup controls still raise the required error. The bounded correction must use annotation-aware lookup while retaining ordinary-map missing-key behavior, single RHS evaluation, typing and lexical scope. The successful whole-pattern typed-record refusal is not evidence that this untyped destructuring path enforces the annotation.

Both findings keep independent expectations and correction exit criteria in [the decisions](C05-REVIEW-DECISIONS.json). Engine-touching fixes require the mandatory frozen gates before merge. C02's atomic-annotation defect and C04's syntax/placeholder/mapping-focus gaps remain open; C05 grants no acceptance waiver.

## JNode scope and the two previously omitted names

`array:members` and `array:of-members`, discovered by C01's inventory refresh, now have explicit catalog assignments and bounded evidence in [JNode navigation](data-model/jnode-navigation.md). The first returns one ordered member JNode per array member, with one-based keys and parent value; the second builds one member per supplied wrapper. Empty members, multi-item values, nested arrays/maps, empty inputs, invalid inputs and roundtrip shape are checked. They are distinct from `array:items` concatenation and do not establish generic node identity.

**K01** retains eight known unsupported-model controls: a JNode does not match `node()`, `xnode()` on XML raises `XPST0051`, JNode `is` raises `XPTY0004`, and a current-draft `jnode('a',atomic(xs:integer))` pattern raises `XTSE0340` under both hosts. This is partial delivered navigation/member support, not a new full-model adoption or proof of default JNode template behavior. No skip, fixture, gate or model boundary is silently changed.

**K04** retains host/type/declaration/package ownership for C06 and final entry review. **K05** retains remaining composite, cardinality, callback, error-order, scope and identity combinations for C09–C12/C13. Bounded supported examples and explicit limitations remain distinct from whole-entry acceptance or a promise to implement every current draft addition.

## Sources, reproduction and next work

Review uses pinned qtspecs **b4723ff52fd9d4fabf2b4cd33bd44f21939c512c** expression, grammar, XSLT and F&O source retained by C01/C03. The new evidence stores exact production fragments, conditions and relevant normative sections. It retains previous examples and delivered correction receipts instead of reopening fixed union/EQName issues. Current review does not reconstruct C08's 28 missing historical adoption revisions or promote a language/host profile.

Run `python docs/xpath40-catalog/build-c05-review.py`, `python docs/xpath40-catalog/build-closure-plan.py` and `python docs/xpath40-catalog/evidence/c05-collection-pattern-review/validate-evidence.py`. The validator checks source/binary identity, independent expectations, focused receipts, exact scoped annotations, deterministic outputs, retained prior evidence and isolated historical plans. Render checks use unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.

No engine, API, test, executable example, version, fixture/filter/gate, kit, upstream or Braid change occurs. Next is **C06: host, declaration, package and profile grammar boundaries**. Skip fixes, Braid and extension work remain deferred until REQ-126 closure work finishes; no release follows from this review.
