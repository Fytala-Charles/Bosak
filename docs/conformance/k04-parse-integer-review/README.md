# K04-C10: parse-integer contract review
**K04-C11 parse-integer implementation - 27 October 2026:** [Bounded parser and promotion](../k04-parse-integer-fix/README.md) adds4.0-only arities1/2,typed optional/default arguments,keywords and exact Int64 parsing/errors. All47 parse originals,the formatter round trip and46 profile controls pass;new87 regression rows and all22,198 solution tests pass. Fresh complete sets47/0/0 and101/0/1 qualify for promotion:qt4 now **226sets,8,586/0/1,893**. All1,892 prior qt4 skips remain;only the existing format-integer-042 skip is newly covered. QT3 stays **31,161/0/660**,with identical reasons. Candidate **1.28.0** unpublished/public1.26.3. No fixture/assertion,filter,schema or QT3 baseline change. Bosak.Schema owns staged XSD1.1;REQ-126 remains Complete. Next:review the negative numbering-sequence dependency on format-integer-042 before any exact-baseline proposal. Earlier K04 notices are historical checkpoints.


27 October 2026. Source base **a856dced**; candidate **1.27.0** unpublished,public **1.26.3**.

**Review complete;implementation remains open.** All47 original parse-integer cases and the repaired formatter's round-trip case fail with missing-function XPST0017. A fresh normal parse-set run reports **0passed/47failed/0skipped**. This is a registration/implementation gap,not a dependency skip or an already-working parser needing one extra overload. The nongated runner's exit0 does not mean the cases pass.

## Pinned contract

[SPECIFICATION-REVIEW.json](evidence/SPECIFICATION-REVIEW.json) records a review-only pin of fn:parse-integer at immutable qt4cg revision[e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml),SHA256409221fb91752e0a174a00f3ef6aecd7bf69807bf203e15f91e4d420145e39e5. It is distinct from future semantic adoption and historical REQ-126 acceptance.

| Contract | Reviewed requirement |
| --- | --- |
| Arity/signature | value as xs:string?,radix as xs:integer?;result xs:integer?;effective arities1/2 |
| Defaults | Radix10 when omitted or empty;empty value returns empty |
| Lexical preprocessing | Remove whitespace and underscores,including internal positions;the pinned equivalent explicitly removes codepoints9/10/13/32/95 |
| Digits/sign | Optional leading +/-,at least one ASCII generalized digit;case-insensitive alphabet0123456789abcdefghijklmnopqrstuvwxyz restricted to the base |
| Radix error | FORG0011 outside2..36 |
| Lexical error | FORG0012 for empty processed text or invalid digit/sign structure |
| Size error | FOCA0003 beyond the implementation-dependent integer limit |
| Call surface | Positional calls,named references,lookup and value/radix keywords must agree;lower profiles remain unavailable |

The future implementation must record its exact stripping/range/error-order policies before semantic changes. In particular,broad .NET whitespace classification should not silently replace the pinned equivalent's narrow set. The proposed Int64 overflow control is a future implementation choice,not a claim that parsing already exists. Ambiguous combinations of empty input and invalid arguments need an explicit reviewed policy;the original fixtures do not settle every precedence combination.

## Original fixtures and controls

The [fixture inventory](evidence/fixture-contract-inventory.json) covers all47 cases:20assert-eq,sevenassert-true,sevenany-of,threeassert-empty,eighterror and twostring-value assertions. Seven originals allow FOCA0003 for size limits. The current XDM Integer factory stores long,so a bounded Int64 parser can satisfy those alternatives while preserving the within-range exact-value cases,including values above2^53. No arbitrary-precision result claim is justified.

The48 original diagnostics preserve every expression/environment/assertion. **46 accepted controls** cover23 expressions in each profile:arity1/2/unsupported0/3,defaults/empty values,whitespace,typed arguments,nodes,sign/digit/radix/size errors,keywords,named references and lookup. QT3 has23passes through its refusal/absence assertions;qt4 has three unsupported-arity/lookup passes and20missing-capability failures. Passing negative assertions while the function is absent does not prove future argument validation or correct supported arities.

One initial3.1 keyword control expected XPST0017,but the parser correctly rejects4.0 keyword syntax earlier with **XPST0003**. The corrected control passes;the initial expected-value mistake and receipt are retained in [initial-keyword-expectation.json](evidence/initial-keyword-expectation.json). There are **96 actual bounded process executions**:48originals,46initial controls,one corrected control and one normal47-case set;95receipts form the accepted current diagnosis. Zero timeouts. No failed outcome is relabeled as implementation success.

The existing registry lacks both arities and the keyword-signature entry. The future function needs typed optional-argument metadata,real default-on-empty handling,node atomization and strict integer radix validation;simply adding a permissive string-to-long helper would not complete the contract. [Next implementation scope](NEXT-SLICE.md) defines the bounded work and verification.

No source,harness,version,fixture/assertion,admission or baseline changes. Source-identical accepted **22,111 solution passes**,normal QT3 **31,161/0/660**,qt4 gated **8,438/0/1,892** and11Experimental passes are inherited,not fresh full-suite/gate runs. The [manifest](evidence/manifest.json) protects this review and **2,077 prior artifacts**. Bosak.Schema retains staged XSD1.1 ownership;REQ-126 remains Complete for its accepted bounded catalog. No release,push,tag,Braid change or upstream submission occurs.

```powershell
python docs/conformance/k04-parse-integer-review/build-review.py
python docs/conformance/k04-parse-integer-review/evidence/validate-evidence.py
```
