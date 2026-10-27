# K04-C11: bounded parse-integer and integer-set gate promotion
**K04-C12 unsupported-sequence proposal - 27 October 2026:** [Exact baseline review](../k04-sequence-admission/BASELINE-UPDATE-PROPOSAL.md) verifies original format-integer-042 assertions in both profiles. Actual formatter mechanism and16expression controls prove decimal fallback for unsupported U+FBF4;288filter controls preserve other policy. Complete isolated rehearsals pass **31,162/0/659** and **8,587/0/1,892**,with only042removed and all retained skip reasons identical. Normal source and AGENTS stay **31,161/0/660** / **8,586/0/1,893** pending explicit owner approval of the exact three-file patch. Source-identical22,198solution passes are inherited,not a fresh unit run. Candidate1.28.0 unpublished/public1.26.3;no engine,fixture/assertion,schema or GatedSets change. Bosak.Schema owns staged XSD1.1;REQ-126 remains Complete. Earlier K04 notices are historical checkpoints.


27 October 2026. Source base **4efb46a5**;new function functionality advances the unpublished candidate to **1.28.0**. Public version remains **1.26.3**.

The standard library now supplies4.0-only parse-integer arities1/2,typed optional arguments,default radix10,keywords,named references and lookup. All **47 original parse assertions and the formatter round trip pass**. The complete normal sets report **47/0/0** for parsing and **101/0/1** for formatting. After those zero-failure receipts,both sets are promoted into the qt4 gate: **226sets,8,586passed/0failed/1,893skipped**.

## Adopted contract and exact behavior

[SPECIFICATION-ADOPTION.json](evidence/SPECIFICATION-ADOPTION.json) records pre-implementation semantic adoption against immutable qt4cg [revision e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml),SHA256409221fb91752e0a174a00f3ef6aecd7bf69807bf203e15f91e4d420145e39e5. It is separate from K04-C10 review provenance and historical REQ-126 acceptance.

Function signatures use xs:string? and xs:integer? argument metadata,with optional integer output. Keyword labels are value/radix,with default10;an empty supplied radix also uses10. Normal function conversion precedes the body,including node atomization and strict type/cardinality checking. Empty value returns empty before radix-domain validation. The3.1 call/reference/lookup boundaries remain;3.1 keyword syntax still fails at grammar stage with XPST0003.

The parser strips only XML whitespace codepoints9/10/13/32 and underscore95,then accepts one optional sign and at least one case-insensitive ASCII generalized digit. Radices outside2..36 raise **FORG0011**;invalid text/digits/sign structure raises **FORG0012**. The adopted result bound is Int64. Safe unsigned magnitude accumulation preserves both endpoints without floating-point or decimal parsing;oversized valid results raise **FOCA0003**. Lexical validation continues after detected overflow so invalid trailing digits keep their lexical error. The seven original any-of size alternatives remain unchanged and do not establish arbitrary precision.

## Validation and promotion

The new **87 regression rows** cover both4.0 profiles,all bases,exact values above2^53,defaults,empty inputs,whitespace/signs/digits,typed arguments,nodes,keywords,references,lookup,Int64 endpoints/overflow,error ordering and3.1 refusal. Before registration:81failed/6passed. An initial post-fix run had85passed/two incorrect empty-display expectations;the tests now assert semantic emptiness rather than comparing its display string. That source/log is retained separately. Final87rows pass.

All48 original diagnostics and46 cross-profile controls pass under separate20-second watchdogs. Complete normal parse and format sets run under separate60-second watchdogs before promotion. There are **96 bounded diagnostic processes**,zero timeouts.

The repository's gate criterion is P>0 andF=0;existing dependency skips remain visible. [GATE-PROMOTION.json](evidence/GATE-PROMOTION.json) pins the zero-failure prerequisite receipts and exact added sets. No old set is removed. The148new gated passes are47parse+101format. The only additional gated skip is the existing **format-integer-042**;all1,892old qt4 skip names/reasons remain identical. This is expanded gate coverage,not an introduced test failure. The total becomes10,479cases;11Experimental passes remain.

Fresh solution build succeeds and **22,198 solution tests pass**,including87new rows. Normal QT3 remains exactly **31,161/0/660**,with every skip reason unchanged. [RESULTS.json](RESULTS.json),[skip verification](evidence/skip-verification.json) and the [manifest](evidence/manifest.json) protect all current receipts and **2,292previous artifacts**.

No fixture/assertion,dependency filter,public C# API,schema backend,QT3 baseline or REQ-126 disposition change. FunctionLibrary history5.156 and ConformanceRunner40 history0.14 record the implementation/promotion. Bosak.Schema retains staged XSD1.1 ownership;REQ-126 remains Complete for its accepted bounded catalog. No release,push,tag,Braid change or upstream submission occurs.

Next recommended slice:review format-integer-042's negative numbering-sequence dependency in both profiles. Any resulting QT3 total change needs its own concrete exact-baseline proposal and owner approval;this slice does not authorize that change.

```powershell
python docs/conformance/k04-parse-integer-fix/build-review.py
python docs/conformance/k04-parse-integer-fix/evidence/validate-evidence.py
```
