# K04-C36 — Remaining skips and pragma boundary review

28 October 2026. Read-only review base **1b5554b6**. **Pragma admission is not qualified.** Candidate **1.32.0** remains unpublished/public **1.26.3**. Normal source, guards, baseline, fixtures and version are unchanged.

## All 481 remaining skips accounted for

The complete normal and isolated inventories load the actual harness, expression reader, lexical normalizers and dependency filter for **31,821 cases / 428 sets**. Each dependency is also evaluated independently under the XPath and XQuery profiles. The [named ledger](evidence/remaining-skips.json) records every remaining skip, its actual routing predicate, complete dependencies, rejected singleton dependencies and existing skip reason.

| Mutually exclusive classification | Count |
|---|---:|
| Unsupported dependency: pragma routing alone blocks admission | **55** |
| Unsupported dependency: pragma routing plus independent exclusions | **2** |
| Unsupported dependency: independent exclusions with routing allowed | **368** |
| Unattended catalog/demo/XMark exclusions | **40** |
| Platform, implementation range and external-resource limits | **10** |
| Existing schema-initialization harness errors | **6** |
| Total | **481** |

The first three rows reconcile all **425 Unsupported dependency** skips. They describe current catalog/profile admission, not 425 engine defects. Independent exclusions include exact older spec tokens, static typing, XML/XSD version policies, negative dependencies on admitted features, and other explicit profile rules. [Dependency occurrence counts](evidence/classification-summary.json) overlap when a case has multiple blockers. The six existing substitution-020–025 harness errors concern the current XmlSchemaSet loader rejecting a multiple-name substitutionGroup value; they remain separate from ordinary dependency exclusions. Bosak.Schema retains XSD 1.1 ownership; ADR-006/REQ-139 remains separately pending and unaccepted.

## Pragma boundary and original assertions

Removing only the pragma regex refusal in a copied harness changes **57** actual predicates: 54 in prod-ExtensionExpr, and one each in prod-EQName, prod-ValidateExpr and misc-CombinedErrorCodes. The complete prod-ExtensionExpr set contains 57 cases; with the three external cases, the direct original cohort is **60**. Expressions, assertions, modules, environments and dependencies are unchanged. Two affected cases retain exact-XQ10 exclusions: **K-ExtensionExpression-4** and **XQST0079**.

| Fresh bounded check | Passed / failed / skipped |
|---|---|
| Complete normal QT3 | **31,340 / 0 / 481** |
| Complete isolated pragma-enabled QT3 | **31,348 / 47 / 426** |
| Complete normal qt4 gate | **8,939 / 0 / 1,920** |
| All 60 originals, direct normal executor | **11 / 9 / 40** |
| Same originals, direct isolated executor | **11 / 49 / 0** |
| Independent public API controls | **8 / 16 / 0** |

The isolated gate exits **2**, with its 47 assertion failures retained. Exactly **55** previous skips execute: eight pass and 47 fail; every **426** retained skip name/reason is unchanged. All 47 logged gate failure names exactly equal the direct failed cohort after removing the two independent exclusions. Both normal skip maps match the approved C35 adoption exactly. qt4 retains 230 gated sets and 11 experimental passes.

The direct executor bypasses runner dependency admission. This explains the nine normal direct failures despite a green normal gate: those cases remain excluded by normal routing/dependency admission. All eleven direct isolated passes accept XPST0003 for invalid syntax; they do not establish pragma fallback support. The 16 semantic/control failures cover prefixed and URI-qualified names, no-namespace names, required fallback and namespace errors, raw quote/comment contents, parenthesized path/predicate use and associated-expression errors. Six syntax-error controls and two ordinary/literal controls pass.

Sixteen lexical controls pass against each actual harness. An initial expected routing outcome for comment text between `(#` and the name was wrong: the existing normalizer removes that comment, making the guard match. An initial literal-length control expected eight characters instead of seven. Both failed diagnostic phases, original inputs and corrections are preserved separately; neither changed production or a catalog assertion. The final **209** diagnostic processes are externally bounded, with no timeout; only the deliberately failing isolated gate exits nonzero.

## Required implementation before admission

The pinned [XQuery 3.1 Recommendation §3.22](https://www.w3.org/TR/2017/REC-xquery-31-20170321/#id-extension-expressions) requires EQName resolution and ignored-pragma fallback. Pragma contents end at `#)` and need raw lexical handling rather than ordinary string/comment processing. Missing associated expressions require XQST0079 when all pragmas are ignored; unbound prefixes require XPST0081. Unprefixed/no-namespace names are ignored under 3.1. ExtensionExpr belongs at ValueExpr precedence. The review paraphrases these requirements in a specification record; it does not adopt implementation semantics.

The current lexer emits the ordinary hash token and the parser treats it as a 4.0 QName-literal form. Valid pragma queries therefore raise XPST0003; raw unmatched comment contents may be consumed as ordinary comments before parsing. Removing the routing guard cannot repair this behavior.

Next: [C37 implement bounded ignored-pragma fallback](NEXT-SLICE.md), retaining normal admission guards until a later exact qualification. Full solution evidence is explicitly inherited from adopted C35: **24,082 passing tests**, including 73 construction regressions. No source or normal DLL changed in this review. REQ-126 remains Complete under bounded acceptance with all 48 historical unmet observations preserved. No baseline proposal, release, upstream or Braid change is included.

Run `python docs/conformance/k04-pragma-review/evidence/validate-evidence.py` to verify the ledger, complete catalog, failures and immutable inputs. The manifest seals raw artifacts and normalized documentation identities. Ten HTML renders use unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.
