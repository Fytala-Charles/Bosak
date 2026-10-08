# REQ-118 — XPath / XSLT 4.0 Adoption Plan

**Status:** Accepted — planning activated 2026-10-07 (implementation remains gated on core 1.0)
**Owner:** Charles Korthout
**Anchor REQ:** [`FEATURE_REQUESTS.md`](./FEATURE_REQUESTS.md) REQ-118

> Position recorded 2026-10-03: 4.0 is not a standard; nobody is "at parity." Bosak
> adopts *stabilized* 4.0 features early as a differentiator, but does not chase the
> moving pre-standard spec. This dossier is the concrete plan that REQ-118's registry
> entry points at; each scheduled slice gets its own sub-REQ.

---

## 1. Spec reality (audit 2026-10-07)

The QT4CG drafts have crossed a process threshold: the XPath 4.0 document is now a
**"WG Review Draft" (W3C Editor's Draft, 15 Sept 2026)** — handed from the Community
Group to a W3C Working Group for review (see Kay, *The XSLT/XPath/XQuery 4.0
Standards: a High-Level Perspective*, Balisage 2026). Still **no Candidate
Recommendation and no formal at-risk markers** — the commitment recorded in REQ-118
(no parity target, stabilized-features-only) stays in force.

### 1.1 Verified sources

| Spec | URL |
|------|-----|
| XPath 4.0 (WG Review Draft) | https://qt4cg.org/specifications/xquery-40/xpath-40.html |
| F&O 4.0 | https://qt4cg.org/specifications/xpath-functions-40/Overview.html |
| XSLT 4.0 (+ Appendix I "Changes since 3.0", Appendix J incompatibilities) | https://qt4cg.org/specifications/xslt-40/Overview.html |
| Spec index (XDM 4.0, Serialization 4.0, Streaming 4.0) | https://qt4cg.org/specifications/ |
| Test suite (~40,000 tests, 738 test-sets, catalog `version="4.0"`) | https://github.com/qt4cg/qt4tests |
| XSLT 4.0 tests (separate repo) | https://github.com/qt4cg/xslt40-test |

### 1.2 Stability tiers (from the change-history blocks)

**Looks stable** (unchanged 1.5–3 years, each with a dedicated qt4tests set):
- Operators: pipeline `->` (§4.20), mapping arrow `=!>` (§4.22.2 — *renamed from `=>>`*),
  otherwise `??` (§4.17), guarded-expression semantics (§2.6.5).
- String templates — backtick interpolation (§4.10.2); there is **no** separate
  `"""…"""` syntax.
- Keyword arguments `name := value` (§4.6.1); focus functions `fn { … }` and the `fn`
  abbreviation for `function` (§4.6.6).
- `for member` / `for key … value` bindings over arrays/maps (§4.14.1).
- Binary/hex literals with underscore separators (§4.3.1).
- Enum types (§3.2.6), choice item types (§3.2.5), record types *structural* form
  (§3.2.10).
- The bulk of the new F&O functions (dated 2023–2024): `fn:replicate`, `fn:slice`,
  `fn:items-at`, `fn:foot`/`fn:trunk`, `fn:char`/`fn:characters`, `fn:highest`/`fn:lowest`,
  `fn:sort-by`/`fn:sort-with`, `map:build`/`map:put`/`map:entries`, the `array:*`
  additions, and most of ch. 5–9 string/URI/date functions.
- XSLT: `xsl:array`/`xsl:array-member`, `xsl:map` `select`/`duplicates`,
  `xsl:map-entry` first-class, `xsl:switch`, `xsl:note`, `xsl:if then/else`,
  `separator` on `for-each`/`apply-templates`, `xsl:evaluate` mandatory.

**Still moving — do NOT build yet:**
- *Nominative* record types — re-specified **27 Sept 2026** (PR 2934).
- Method-call operator `=?>` — semantics changed 25 June 2026 (Issue 2219).
- `fn:scan` — added 31 July 2026 (PR 2824).
- `for member`/`for key` extended to *sequences* of arrays/maps — 25 June 2026.
- JNode built-in template rules — April 2026.
- XDM 4.0 GNode/JNode model throughout (§2.1.3 splits `node()` into `xnode()`/`jnode()`).

**Deliberately rejected / renamed (do not implement old shapes):**
- `fn:all` / `fn:some-of` never existed — the accepted names are `fn:all-equal`,
  `fn:all-different`, `fn:some`, `fn:every`.
- `scan-left`/`scan-right` consolidated into single `fn:scan` (left fold only).
- `=>>` → `=!>`. No `switch` *expression* in XPath (`xsl:switch` is XSLT-only).
- Extensible map types dropped (coercion discards undefined entries instead).

**Backwards-incompatibilities** (XSLT Appendix J) that matter to a 3.0-conformant
engine: union-pattern priority changes, `trusted=no` default on `xsl:evaluate`,
stylesheet-parameter visibility now always private. Bosak's 100.0% XSLT 3.0 basic
sweep must not regress when 4.0 surfaces land — every 4.0 feature needs a version
gate (§3).

## 2. Ranked candidates (value vs. Bosak fit)

Ranking axes: user value, implementation cost against Bosak's layered architecture
(Parser → Compiler → `VmEngine` → `FunctionLibrary`), testability via qt4tests, and
spec stability from §1.2.

### Tier 1 — F&O 4.0 function wave (pure functions, no grammar changes)

All live in `FunctionLibrary` behind the existing function-dispatch seam; each has a
ready-made qt4tests set. This is the same shape as the REQ-121 EXSLT wave (5.109) and
the lowest-risk 4.0 surface.

- Sequences: `fn:replicate` (2.1.10), `fn:slice` (2.1.12), `fn:items-at` (2.1.8),
  `fn:foot`/`fn:trunk` (2.1.3/2.1.15), `fn:insert-separator` (2.1.7),
  `fn:contains-subsequence` family (2.2.3/2.2.7/2.2.9), `fn:duplicate-values` (2.2.6),
  `fn:all-equal`/`fn:all-different` (2.4.2/2.4.3), `fn:highest`/`fn:lowest` (2.5.10/12),
  `fn:sort-by`/`fn:sort-with` (2.5.18/20; `fn:sort` stays as the 3.1 compatibility shim).
- Strings: `fn:char`/`fn:characters`/`fn:graphemes` (5.4.1–5.4.3),
  `fn:pad-string` (5.4.6), `fn:trim-space` (5.4.11), `fn:index-of-substring` (5.4.8),
  `fn:substring-before-last`/`fn:substring-after-last` (5.5.6/7), `fn:hash` (5.4.16).
- URIs: `fn:parse-uri` (7.6.2), `fn:build-uri` (7.6.3), `fn:decode-from-uri` (7.1).
- Dates/times: `fn:seconds`/`fn:duration-to-seconds` (8.4.1/2), `fn:build-dateTime`
  (9.4.2), `fn:unix-dateTime` (9.4.3), `fn:days-in-month` (9.6.11).
- Maps/arrays: `map:build`/`map:entries`/`map:entry`/`map:filter`/`map:items`/`map:put`
  (14.4), `array:build`, `array:empty`, `array:filter`, `array:items`, `array:join`,
  `array:members`/`array:of-members`, `array:slice`, `array:sort-by` family (15.2).
- Higher-order (after HOF seam review — Bosak already dispatches HOFs):
  `fn:some`/`fn:every` (2.5.16/4), `fn:index-where` (2.5.11), `fn:partition` (2.5.14),
  `fn:take-while`/`fn:drop-while` (2.5.21/3), `fn:while-do`/`fn:do-until` (2.5.23/2),
  `fn:partial-apply` (2.5.13), `fn:transitive-closure` (2.5.22). **`fn:scan` deferred**
  to Tier 4 (July 2026 — too new).

### Tier 2 — Stable XPath 4.0 grammar (parser/compiler work)

Each is version-gated syntax; the recursive-descent parser takes these in small,
independent commits.

- `??` otherwise operator (§4.17) + guarded-expression semantics (§2.6.5) — smallest.
- Keyword arguments (§4.6.1) — touches the call-argument grammar + static arity checks.
- String templates (§4.10.2) — new lexer tokens + atomization rules.
- `for member` / `for key value` (§4.14.1, basic forms only).
- Pipeline `->` (§4.20) and mapping arrow `=!>` (§4.22.2) — arrow-operators already
  exist for `=>`; these extend the same precedence table.
- Focus functions / `fn` abbreviation (§4.6.6); binary literals (§4.3.1).
- **Deferred within tier**: method-call `=?>` (June 2026 churn), computed node
  constructors in XPath (§4.13 — Feb 2026, overlaps XQuery-only territory).

### Tier 3 — Item-type system: enums, choices, (structural) records

`Xdm` type representation + `ItemType` parsing + function-signature coercion. Records
are the biggest single engine touch (field typing, coercion, `but with`); take the
**structural** form only and defer **nominative** records (re-specified 27 Sept 2026).
Enum and choice types are small and stable — can precede records.

### Tier 4 — XSLT 4.0 surfaces

- **First**: `xsl:note` (free — ignored element), `xsl:if then/else`, `separator` on
  `for-each`/`apply-templates`, `xsl:map select/duplicates`, `xsl:map-entry`
  first-class promotion — small `Stylesheet` loader changes.
- **Then**: `xsl:array`/`xsl:array-member` construction; `xsl:switch`.
- **Pattern system** (§6.3–6.4: map/array patterns, type patterns, new default
  priorities) — the largest XSLT 4.0 area; touches the pattern compiler and the
  template-rule priority machinery. Schedule only after Tiers 1–3 settle, and gate the
  union-pattern priority change behind the 4.0 version switch (Appendix J
  incompatibility).
- **Deferred**: `xsl:item-type`/`xsl:record-type` declarations, enclosing modes,
  capturing accumulators, `fn:apply-templates`, Serialization 4.0 parameters.

### Out of scope (recorded, not scheduled)

- **XDM 4.0 GNode/JNode model** (`xnode()`/`jnode()`, `fn:jtree`/`fn:jkey`/`fn:jvalue`,
  JSONPath comparisons §4.7.9) — a parallel node model touching every layer; revisit
  only if the drafts freeze and a customer need appears.
- **`fn:parse-html` / `fn:html-doc`** (17.3) — requires an HTML5 parser dependency;
  the known Cloudflare-challenge environment lesson applies. Owner decision needed.
- **`fn:invisible-xml`** (17.6), **`fn:unparsed-binary`**, **`fn:xsd-validator`** —
  external dependency or schema-validator machinery.
- **XQuery 4.0-only deltas** (direct constructors already exist; full FLWOR `where`/
  `order by`/`group by` in XPath is *not* in the grammar — XQuery-only, tracked under
  the XQuery project's own REQ if ever requested).
- **XSLT Streaming 4.0** deltas until the streaming conformance tail (REQ-087
  residuals) is closed.

## 3. Version gating (design decision, needs owner sign-off)

Bosak's public surface is `XPath31Expression` and a 3.1 static context. Shipping 4.0
features needs a gate so 3.1 behavior (including Bosak's 100.0% XSLT 3.0 sweep) is
byte-identical:

- **Option A (recommended):** an `XPathVersion`/`XsltVersion` switch on the compile
  surface (default 3.1; 4.0 opts in). 4.0-only functions raise XPST0017 in 3.1 mode;
  4.0-only syntax is a parse error in 3.1 mode. Appendix-J incompatibilities apply
  only in 4.0 mode.
- **Option B:** preview flag on `EvaluationContext` (runtime), functions always
  visible. Cheaper, but blurs the static/dynamic error distinction the specs demand.
- qt4tests conformance runs then execute in 4.0 mode alongside the frozen 3.x gates —
  the REQ-118 acceptance criterion "3.x conformance stays green" is checked on every
  slice.

## 4. Recommended slices (each = its own sub-REQ when scheduled)

| Slice | Content | Depends on |
|-------|---------|------------|
| **4.0-S1** | Tier-1 F&O function wave part 1: sequence + string functions (pure, non-HOF) | §3 gate decision |
| **4.0-S2** | Tier-1 part 2: map/array additions + URI/date functions | S1 |
| **4.0-S3** | `??`, keyword args, string templates, binary literals | §3 gate decision |
| **4.0-S4** | `->` / `=!>`, focus functions, `for member`/`for key` | S3 |
| **4.0-S5** | Tier-1 HOF functions (`fn:some`/`every`/`partition`/…) — landed 2026-10-08 on `feature/req118-40-s5`: all nine functions of the §2 higher-order list except `fn:scan`, which stays deferred (post-June-2026 churn). Signatures + error codes verified against the live F&O 4.0 WG Review Draft and qt4tests edge cases; 3.1 mode raises XPST0017 | S1 |
| **4.0-S6** | Enums + choice item types; structural records + `but with` — S6a (enums §3.2.6 + choice item types §3.2.5, in-order alternative coercion §3.4.2 rule 02) landed 2026-10-08 on `feature/req118-40-s6a`: semantics verified against the live XPath 4.0 WG Review Draft; no XDM changes (string-based type-text branches in the parser + VmEngine); 3.1 mode rejects both with XPST0003. S6b (structural records §3.2.10 + `but with` §4.15.4) landed 2026-10-08 on `feature/req118-40-s6b`: records are maps with a record-type annotation (new `XdmMap.RecordType`/`XdmRecordType`/`XdmRecordField` — the first XDM change of the wave); instance-of structural (plain maps never match, exact entry count, covariant fields); coercion §3.4.2 rule 10 (missing → () entries, surplus XPTY0004, recursive value coercion, declaration-order entries); cast §4.19.2.7 (keep-or-cast values FORG0001, surplus discarded, `record(*)` assertion); lookup field checks XPTY0004 with map:* field-blind; `but with` = use-last merge + re-coercion to the LHS annotation; qt4tests `prod/RecordType.xml` is stale (pre-annotation draft) so hand-written tests only; 3.1 mode rejects `record(…)` and `but with` with XPST0003 | S3–S4 |
| **4.0-S7** | XSLT 4.0 easy surfaces (`xsl:note`, `xsl:if then/else`, `separator`, `xsl:map`/`map-entry` upgrades) | §3 gate decision |
| **4.0-S8** | `xsl:array` construction; `xsl:switch`; pattern system | S6, S7 |

**Sequencing rule (REQ-118 decision log):** no slice starts before the paired
`v1.0.0` core tag. The 0.13.0 soak release (2026-10-07) is the issues/discussions
gathering window; 4.0-S1 scheduling is an owner call after that soak reads out.

## 5. Open owner decisions

1. **Version gate shape** — §3 Option A vs B (recommend A).
2. **qt4tests adoption** — wire the 4.0 catalog as the S1+ conformance gate (new
   harness project `Bosak.XPath.Conformance40`?) or hand-port selected sets. The
   catalog is `version="4.0"` with per-test `spec="XP40 XQ40"` dependency flags.
3. **`fn:parse-html`** — adopt an HTML5 parser dependency (e.g. AngleSharp) or skip
   permanently?
4. **Naming** — does the public API keep `XPath31Expression` with a version parameter,
   or grow a neutral `XPathExpression` facade? (API-freeze interplay: the freeze lifts
   the *surface*, not naming evolution in a new major.)
5. **Pattern-priority Appendix-J change** — confirm it stays gated to 4.0 mode and
   never affects the 3.1 conformance gates.

## 6. Risks

- **Spec churn** — the tier system mitigates; anything dated after ~June 2026 is
  Tier-4/deferred by rule. Re-audit the change blocks per release (REQ-118 acceptance).
- **Conformance gate skew** — qt4tests requires new tests to pass on ≥1
  implementation; early slices may hit tests for features we deliberately defer —
  the harness needs a skip-dependency mechanism keyed on `spec` flags.
- **Function-name collisions** — 4.0 reuses the `fn:`/`map:`/`array:` namespaces with
  URIs explicitly unchanged (F&O §1.3); no collision risk, but the 3.1 static context
  must not see 4.0-only entries (gate, §3).
- **Record types** are the single largest engine touch and the least frozen — the
  structural/nominative split may move again; S6 is where schedule slip would land.
- **3.x regression risk** — every slice re-runs the frozen gates: QT3 31,142/0/679;
  XSLT basic 10,242/0/4,359 (100.0%); schema-aware 11,054/1/3,546.

## 7. References

- QT4CG spec index: https://qt4cg.org/specifications/
- XPath 4.0 WG Review Draft (15 Sept 2026): https://qt4cg.org/specifications/xquery-40/xpath-40.html
- F&O 4.0: https://qt4cg.org/specifications/xpath-functions-40/Overview.html
- XSLT 4.0 (Appendix I changes, Appendix J incompatibilities): https://qt4cg.org/specifications/xslt-40/Overview.html
- qt4tests: https://github.com/qt4cg/qt4tests
- Kay, Balisage 2026 (process status): https://balisage.net/Proceedings/vol31/print/Kay01/BalisageVol31-Kay01.html

---

*Dossier created 2026-10-07 — REQ-118 planning activation. Feature inventory verified
against the live QT4CG drafts (section numbers as of the 15 Sept 2026 WG Review
Draft); stability tiers from the per-section change-history blocks, not memory.
Next action: owner decisions §5.1–§5.2, then sub-REQ 4.0-S1 after the v1.0.0 tag.*
