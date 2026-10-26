<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak function and option contract reconciliation">
  <h1>Function and option reconciliation — C02</h1>
  <p>26 October 2026 · published Bosak 1.26.3</p>
</div>

**C02 completes the bounded reconciliation of 30 function entries and 74 function surfaces. Eleven closure workpackages remain open.** This closes a review workpackage, not all function defects or AC-01–04. The [machine-readable report](FUNCTION-OPTIONS-RECONCILIATION.json) separates verified examples, explicit unsupported boundaries, current-draft adoption choices and correction work. C13 still requires acceptance or resolution of outstanding limitations; C02 grants no waiver. Skip fixes, Braid and the VS Code extension remain deferred.

## Verification and its limits

The unchanged 1.26.3 engine at `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f` passes a fresh Release solution build and **21,798 tests across all ten solution test assemblies**, with no failures or skips. A separate retained-binary run passes all **3,148 published catalog examples**. The catalog manifest remains unchanged: its passing observed-gap cases preserve implementation observations and do not certify draft conformance.

The additional **94 bounded expressions produce 195 profile observations**. Of **120 independent expected-outcome controls, 118 match and two expose the same new defect** in the two 4.0 profiles. The remaining 75 observations investigate existing limitations and policy boundaries; they are not counted as passing conformance tests. Six normal/empty/error controls explicitly cover `array:sort-with` and `element-to-map-plan`, which previously had no normal-result manifest candidate in this review's function index. Other explicit boundary controls supplement the filename-based case index.

The new defect is `atomic-type-annotation(())`: the required `xs:anyAtomicType` argument is empty, but the released implementation returns a type record instead of `XPTY0004`. Both independent failure expectations remain intact. Bounded ticket **F08-atomic-annotation-empty** specifies normal records, cardinality, direct/reference/lookup/partial calls, lower-profile refusal and engine gates for a correction. The catalog now says this empty-input refusal is unsupported. C13 must resolve the defect or record an owner-accepted limitation; these diagnostics do not accept it.

The first probe-authoring draft had errors in serialization assumptions, Gregorian field spelling, CSV row access and the frozen char error boundary. Original requests/results and separate corrections are retained. No expected outcome was silently changed to make an engine defect pass. An initial full-suite attempt in the retained release checkout lacked QT3 schema fixtures and was stopped after reporting 51 missing-fixture failures; its log remains separate. The successful complete suite ran in the main workspace with those fixtures available.

## Normative and profile decisions

Current review uses the immutable [F&O catalog b4723ff5](https://github.com/qt4cg/qtspecs/blob/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xpath-functions-40/src/function-catalog.xml) retained by C01 and the [matching F&O framework](https://github.com/qt4cg/qtspecs/blob/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xpath-functions-40/src/xpath-functions.xml) captured for this review. The framework's option conventions apply only where a function explicitly adopts them, include conversion of option values, and exempt unrecognized namespaced QName keys. They do not establish blanket eager validation or the same rules for every map-shaped parameter. Unknown JSON options are ignored in 3.1 by design; the current 4.0 rule is different. A single shared validation change across all profiles would be incorrect.

| C01 draft delta | C02 disposition |
|---|---|
| `char`: positiveInteger to integer | Frozen nonpositive values still fail type conversion; current integer-domain invalid-character error is `FOCH0005`. F02 owns explicit adoption/error policy. |
| `partial-apply`: integer map keys | Frozen zero/negative keys still refuse; current draft ignores out-of-range positions. String keys remain invalid. F02 owns adoption. |
| `trim-space`: omitted value uses focus | Zero-arity form is unavailable in both 4.0 profiles. Explicit input works; F02 owns adoption. |
| `unix-dateTime`: signed integer | Negative epochs work only in Experimental, as already delivered. Frozen refusal and bounded calendar remain explicit. |
| `element-to-map`: `escape-names` | New decoding option is unsupported and raises `XPTY0004` in both 4.0 profiles; F14 owns adoption/capability work. |
| `map-to-element`: inverse/error prose and examples | New encoding option is likewise unsupported. Revised error prose/example corrections do not by themselves prove an engine defect; F14 retains plan/name/layout review. |
| `parse-csv`: nonpositive selection | Frozen positive-index refusal remains; upstream already corrected selection/get expectations. F12 owns adoption, with no duplicate upstream issue. |
| `function-lookup`, `load-xquery-module`, `unparsed-text` | Context-error timing, module variable coercion and fragment handling belong to C04/C06/C07. No new host/resource/profile support is inferred. |

Neither current review pin supplies any of C08's 28 missing historical adoption revisions. Current sources are not automatically adopted into frozen or Experimental profiles.

## Delivered corrections and remaining boundaries

Map entry/item result shapes, 4.0 empty index-of collation, Experimental absent substring patterns, Experimental signed epochs, wide Unix calendar errors, extreme rounding precision, and 1.26.3 integer-preserving abs/floor/ceiling are delivered corrections. Fresh examples and focused controls support the applicable bounded claims. Older unresolved notices remain historical; these items are not reopened as defects. Exact `compare` behavior does not imply exact ordering in the other sort/extrema comparers, and numeric function fixes do not establish generic constructor or callback conversion.

The [explicit review decisions](C02-REVIEW-DECISIONS.json) enumerate each entry's contracts and limitations. The report supplies current signatures/options, profile/keyword metadata, normal/empty/error evidence candidates, preserved source sections and follow-up ownership. Lexical case indexing aids review and never accepts an untested contract. Unrepresented combinations of types, options, callback signatures and error precedence remain outside the documented supported reference; C09–C12 still perform final contract-by-contract semantic/evidence review.

| Follow-up | Bounded subject and current disposition |
|---|---|
| F01 | Missing keyword metadata and four label mismatches; use documented positional calls. Family changes need individual binding/default/type tests. |
| F02 | Explicit current-draft adoption decisions for char, partial binding and implicit trimming; retain frozen policy. |
| F03 | Function/profile-specific option keys, QName extensions, coercion and unused-option validation; retain the 3.1 JSON distinction. |
| F04 | URI Boolean interpretation and empty-query-map precedence; use Boolean values and remove stale raw query. Raw authority bypass is draft-defined. |
| F05 | Empty sort keys, unknown collations and sort-key record validation; explicit `data#1` is the verified key workaround. |
| F06 | Mixed-number exact ordering and direction-sensitive NaN sorting/extrema; restrict business keys to validated homogeneous values. |
| F07 | Merge/build input/default/duplicate-policy boundaries; their default duplicate policies differ. |
| F08 | Atomic/cardinality/callback conversion, including the new **F08-atomic-annotation-empty** defect. Do not use truthy nonboolean map-filter predicates. |
| F09 | `deep-equal` map options remain unsupported; legacy string-collation calls are distinct. |
| F10 | Gregorian fractional precision and permissive input/cardinality boundaries; extended construction is distinct from bounded .NET interop. |
| F11 | Mandatory BLAKE3/CRC-32 capability remains unsupported with `FOHA0001`; hash extension options are implementation-defined. |
| F12 | Current CSV selection/get-index adoption; frozen fixtures and upstream receipts remain unchanged. |
| F13 | Full Unicode grapheme segmentation remains unsupported by the approximate implementation. |
| F14 | XML/map conversion `escape-names`, plans, layouts and names; only verified options/forms are claimed. |

These are 14 follow-up groups, **not 14 mandatory feature implementations or an estimate of remaining PRs**. Separate unrelated corrections before implementing them. The newly reproduced atomic-annotation defect has its own bounded correction record; accurately stated limitations do not become supported contracts through documentation. Any unresolved acceptance issue remains for C13.

## Reproduction and next work

Run `python docs/xpath40-catalog/build-c02-review.py`, `python docs/xpath40-catalog/build-closure-plan.py` and `python docs/xpath40-catalog/evidence/c02-function-review/validate-evidence.py`. Retained manifests protect the original draft/corrected probes, final observations, complete build/test receipts, source identities and HTML render records. Original C00/C01 evidence bytes remain unchanged; their historical plan outputs are reproduced in isolation rather than resetting live progress.

No engine, API, test, example, package version, fixture, filter, gate, upstream, Braid or kit change occurs. Frozen QT3 **31,142/0/679** and gated qt4 **8,436/0/1,894** are inherited from C00, not rerun here. Next is **C03: constructor, type and coercion boundaries**, followed by the remaining closure plan. No release follows from this review.
