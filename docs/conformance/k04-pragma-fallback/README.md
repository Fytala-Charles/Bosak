# K04-C37 — Ignored-pragma fallback implementation

28 October 2026. Implementation base **5ba5528c**. Candidate **1.33.0**, unpublished; public **1.26.3**. The normal pragma guard and exact QT3 baseline remain unchanged.

## Semantic policy and implementation

The immutable pre-implementation pin adopts [XQuery 3.1 REC-20170321 §3.22](https://www.w3.org/TR/2017/REC-xquery-31-20170321/#id-extension-expressions) and the related lexical/ValueExpr grammar. The implementation-defined recognized pragma namespace set is **empty**: every well-formed pragma is ignored, and its associated expression provides fallback. No registration/handler API or recognized extension namespace is added. Missing associated expressions raise **XQST0079**; unresolved prefixed pragma names raise **XPST0081**. Unprefixed and empty-URI names are ignored under 3.1.

The XQuery lexer emits a complete pragma token by scanning raw text to the first `#)`. The parser validates EQNames, XML whitespace boundaries and content characters, then parses one or more pragmas and their fallback at ValueExpr precedence. Quote/comment/entity-like text and braces inside pragma contents do not affect expression scanning. The shared raw span scanner also protects function/global/context initializers, enclosed constructor expressions and XQuery string interpolation. XML namespace scopes in direct constructors and declaring-module scopes remain intact.

An internal extension AST node retains the names until static validation. Function-name resolution and module visibility walk the associated expression; pragma prefix validation runs before optimization, including unused/dead branches and module declarations. A pragma-only validation mode in function/global bodies reuses lexical constructor scopes without broadening unrelated existing name-test checks. After validation, the optimizer replaces the ignored extension with its fallback. Static and dynamic associated-expression errors remain observable. No runtime dispatch or public API change is needed.

XPath 3.1 still rejects extension grammar. XPath 4.0 and Experimental profiles retain QName-literal behavior and reject XQuery pragma syntax. Arrow application to an extension value is legal at the surrounding grammar level; bare path/postfix/simple-map use still requires the appropriate parentheses. An empty prolog namespace declaration removes its prefix, so later use in a pragma raises XPST0081.

## Fresh verification

| Check | Outcome |
|---|---|
| Full Release solution build | Passed, zero errors |
| Solution tests, ten projects | **24,164 passed** |
| New public API regressions | **82 passed** |
| Identical 82-test cohort on preserved 1.32.0 binaries | **55 failed / 27 passed** |
| Complete normal QT3 | **31,340 / 0 / 481** |
| Complete isolated pragma-enabled QT3 | **31,395 / 0 / 426** |
| Complete normal qt4 gate | **8,939 / 0 / 1,920** |
| Independent original public controls | **24 passed** |

All **31,821 cases / 428 sets** retain exactly the C36 normal/prototype routing predicates and dependency observations. All sixteen lexical controls under each harness are unchanged. Exactly **55** eligible original skips execute successfully in the isolated gate; all **426** retained names/reasons match C36. Both normal skip maps remain unchanged. qt4 retains 230 gated sets and 11 experimental passes.

The original direct cohort contains all 57 prod-ExtensionExpr cases plus three cases from other sets. Its isolated results are **59 passes / one failure**: `K-ExtensionExpression-4` expects the pre-3.1 no-namespace error, while 3.1 ignores that pragma and returns its associated expression. This exact-XQ10 case and `XQST0079` retain their independent dependency exclusions. All **55 eligible originals** pass; no assertion or fixture was modified. Direct normal results remain 11 passes / nine failures / 40 skips because that diagnostic bypasses runner dependency admission while preserving normal routing. The complete normal gate stays green.

All **209 final diagnostic processes** complete within external watchdogs with exit zero and no timeout; assertions are checked independently of process exits. The full solution includes 765 XQuery, 13,797 Standard and 8,196 XSLT tests. It runs with approved loopback-listener permissions and requires no Application Control workaround.

Initial failures are preserved: a missing helper caused the first compilation error; the first focused run found four missing declaration-scope checks and two mistaken test expectations. The latter were corrected after specification verification, with the initial generator and logs retained. The final qualified source is identical for the old/new 82-test cohorts. No original catalog expectation was changed.

Bosak.Schema retains XSD 1.1 ownership; the six existing schema harness skips are unchanged, and ADR-006/REQ-139 remains separately pending. REQ-126 remains Complete under bounded acceptance with all 48 historical unmet observations preserved. No normal harness, dependency filter, KnownSkips, GatedSets, baseline, fixture, assertion, release, publication, upstream or Braid change is included.

Next: [C38 separate pragma routing-admission qualification](NEXT-SLICE.md). Run `python docs/conformance/k04-pragma-fallback/evidence/validate-evidence.py` to verify the sealed evidence and scope. Ten HTML renders use unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.
