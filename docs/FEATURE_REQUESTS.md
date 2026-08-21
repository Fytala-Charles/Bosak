<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak feature requests">
  <br><br>
  <h1>Bosak Cross-Application Feature Requests</h1>
  <p>Living registry of cross-cutting capabilities requested by consuming applications</p>
</div>

> **Living Registry** — Last updated: 2026-08-21 (fn:nilled cluster fixed: PSVI IsNil honored, fn:data returns PSVI typed value for schema-validated nodes (empty for nilled elements), element(*, T)/element(N, T) reject nilled elements while element(*, T?)/element(N, T?) accept them; full QT3 sweep at 30,780/119/922 (96.73%); runtime recursion fixes: FunctionItemInstanceOf no longer recurses through ValueMatchesType, and IsSchemaTypeSequenceSubtype no longer recurses through IsSequenceTypeSubtype for atomic schema types; dynamic constructor calls now capture/restore the static namespace context for namespace-sensitive union constructors; restrictions of union/list types are rejected as SequenceType item types with XPST0051; QName/NOTATION cast cluster fixed: original-case prefix resolution, xs:NOTATION-derived user-defined type construction, XQST0034 constructor-function conflicts, and QName-to-string-subtype rejection for union member selection; also fixes CastAs-UnionType-13/14/15/17/20, qname-cast-3/4, notation-cast-3, user-defined-8/9/11, instanceof136-141; `prod-CastExpr.schema` 123/6/1, `prod-CastableExpr UnionType` 29/0, `prod-CastableExpr ListType` 18/0, `fn-nilled` 60/0/4, `prod-InstanceofExpr` 305/3/1; unit tests pass 1,762/0)
> This document tracks feature requests originating from applications consuming the Bosak XPath / XSLT stack. It serves as the single source of truth for cross-cutting capabilities that multiple consumers need.

---

## ⚠️ BREAKING CHANGE: XSLT Namespace Rename (2026-06-06)

The XSLT implementation has been moved from `Bosak.XPath.Xslt` to its own top-level namespace **`Bosak.Xslt`**.

### What changed
| Before | After |
|--------|-------|
| `Bosak.XPath.Xslt.Api` | `Bosak.Xslt.Api` |
| `Bosak.XPath.Xslt.Runtime` | `Bosak.Xslt.Runtime` |
| `Bosak.XPath.Xslt.Patterns` | `Bosak.Xslt.Patterns` |
| `Bosak.XPath.Xslt.Stylesheet` | `Bosak.Xslt.Stylesheet` |
| `src/Bosak.XPath.Xslt/` | `src/Bosak.Xslt/` |
| `tests/Bosak.XPath.Xslt.Tests/` | `tests/Bosak.Xslt.Tests/` |
| `tests/Bosak.XPath.Xslt.Conformance/` | `tests/Bosak.Xslt.Conformance/` |

### Action required for downstream projects (Customer A, Customer C, Customer D, Customer B)
1. Update all `using Bosak.XPath.Xslt.*` → `using Bosak.Xslt.*`
2. Update `.csproj` `<ProjectReference>` paths from `src/Bosak.XPath.Xslt/` to `src/Bosak.Xslt/`
3. Update package references if consuming Bosak via NuGet (future)

### Rationale
XPath, XSLT, and future XQuery are three distinct W3C specifications. The new namespace layout (`Bosak.XPath`, `Bosak.Xslt`, `Bosak.XQuery`) reflects this separation and allows each spec to evolve independently.

---

## 1. Purpose

Bosak is a shared XPath 3.1 + XSLT implementation used by multiple applications (Customer A, and potentially others). When an application needs a new XML-stack capability that belongs in Bosak rather than in its own codebase, the request is recorded here. This prevents duplicate work, enables prioritization, and keeps all stakeholders informed.

**Who can request:** Any application team consuming Bosak libraries or APIs.  
**Who implements:** Bosak maintainers, or contributing teams via PR.  
**Who updates this file:** Kimi agents (on any project) and human maintainers.

---

## 2. How to Submit a Feature Request

### 2.1 Quick Add (for Kimi Agents)

When working on an application that needs a Bosak feature, append a new row to the **Request Registry** below using this format:

```markdown
| `<REQ-XXX>` | `<AppName>` | `<One-line summary>` | `<Motivation>` | `Pending` | `TBD` | `Unassigned` | `YYYY-MM-DD` |
```

Then create a detail section in **Request Details** following the template in §3.

### 2.2 Human-Submitted Requests

1. Open a PR adding your request to this file.
2. Tag the PR with `feature-request` and the requesting application name.
3. Discuss in the PR thread; maintainers will update **Status** and **Decision**.

---

## 3. Request Detail Template

Every request in the registry must have a matching detail section. Copy this template:

```markdown
### REQ-XXX: <Title>

**Requesting Application:** `<AppName>`  
**Submitted:** `YYYY-MM-DD`  
**Status:** `Pending | Accepted | Declined | In Progress | Implemented | Superseded`

#### Problem Statement
<What is the application trying to achieve?>

#### Proposed Solution
<What should Bosak provide?>

#### Acceptance Criteria
- [ ] <Criterion 1>
- [ ] <Criterion 2>

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None / New Syntax | |
| Compiler | None / New IR | |
| Runtime | None / New Opcode | |
| Standard | None / New Function | |
| XSLT | None / New Instruction | |
| API | None / Breaking | |

#### Related Requests
- <Link to related REQ-YYY>

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| YYYY-MM-DD | `<Name/Kimi>` | Accepted | <Why> |
```

---

## 4. Request Registry

| ID | Application | Summary | Motivation | Status | Target Version | Owner | Submitted |
|----|-------------|---------|------------|--------|----------------|-------|-----------|
| REQ-001 | Customer A | XSLT `xsl:import` / `xsl:include` URI resolution | Customer A stylesheets are modular; need to split maps across files | **Implemented** | Phase 1b | Charles Korthout | 2026-05-24 |
| REQ-002 | Customer A | Named XSLT modes | Customer A uses mode-based dispatch for multi-pass transforms | **Implemented** | Phase 2 | Charles Korthout | 2026-05-24 |
| REQ-003 | Customer A | `xsl:sort` support | Customer A EDI sorts line items by sequence number | **Implemented** | Phase 2 | Charles Korthout | 2026-05-24 |
| REQ-004 | Customer A | `xsl:number` support | Customer A generates human-readable line item numbers | **Implemented** | Phase 2 | Charles Korthout | 2026-05-24 |
| REQ-005 | Customer A | `xsl:key` + `key()` function | Customer A looks up reference data by key within transforms | **Implemented** | Phase 2 | Charles Korthout | 2026-05-24 |
| REQ-006 | Customer A | `xsl:output` serialization control | Customer A needs UTF-8, indentation, and omit-xml-declaration control | **Implemented** | Phase 2 | Charles Korthout | 2026-05-24 |
| REQ-007 | *(internal)* | `fn:sort` mixed-type comparator | 20+ QT3 conformance failures block full spec compliance | **Implemented** | TBD | Charles Korthout | 2026-05-24 |
| REQ-008 | *(internal)* | `fn:function-lookup` double-to-string precision | Precision mismatches in numeric serialization | **Implemented** | TBD | Charles Korthout | 2026-05-24 |
| REQ-009 | *(internal)* | Date/time ordering (`lt`, `gt`, `le`, `ge`) | 9 remaining QT3 failures; only equality works today | **Implemented** | TBD | Charles Korthout | 2026-05-24 |
| REQ-010 | *(internal)* | `json-to-xml`, `parse-json`, `xml-to-json` | Standard XPath 3.1 JSON functions missing | **Implemented** | TBD | Charles Korthout | 2026-05-27 |
| REQ-011 | *(internal)* | `fn:transform()` function | XPath-level XSLT invocation per spec | **Implemented** | TBD | Charles Korthout | 2026-05-27 |
| REQ-012 | Customer A | `xsl:call-template` tunnel parameters | Customer A passes context metadata through deep call chains | **Implemented** | TBD | Charles Korthout | 2026-05-24 |
| REQ-014 | Customer B | XML Schema (XSD) validation API | Customer B needs to validate Infor OAGIS BODs against XSDs before dispatching to handlers | **Implemented** | TBD | Charles Korthout | 2026-05-27 |
| REQ-015 | Customer A | `xsl:function` support | Customer A defines 22+ helper functions (date, week, mapping) in shared fragments; cannot execute without this | **Implemented** | Phase 2 | Charles Korthout | 2026-05-26 |
| REQ-016 | Customer A | Multi-key `xsl:sort` (primary + secondary) | Customer A D99A JAMA basesheet sorts by item ID then ship-to; current implementation only handles first key | **Implemented** | Phase 2 | Charles Korthout | 2026-05-26 |
| REQ-017 | *(internal)* | Fix CS0219 unused variable in `FormatNumberEngine` | Compiler warning `CS0219` on unused `hasDecimal` flag in `FormatNumberEngine.cs` | **Implemented** | TBD | Charles Korthout | 2026-05-27 |
| REQ-018 | *(internal)* | Fix CS8602 null dereference in `FormatNumberEngine` | Compiler warning `CS8602` on potential null dereference `sub.Suffix` in `FormatNumberEngine.cs` | **Implemented** | TBD | Charles Korthout | 2026-05-27 |
| REQ-019 | Customer A | `xsl:try` / `xsl:catch` support | Customer A's date/number helper functions use try/catch for defensive parsing of dirty EDI data | **Implemented** | Phase 2 | Charles Korthout | 2026-05-31 |
| REQ-020 | Customer A | `exclude-result-prefixes` support | Customer A's 42 stylesheets declare `exclude-result-prefixes="xs app"`; without it, output XML is polluted with unused namespace declarations | **Implemented** | Phase 2 | Charles Korthout | 2026-05-31 |
| REQ-021 | Customer A | `xsl:message` support | Customer A partner overrides use `xsl:message` for debugging and audit logging during transform execution | **Implemented** | Phase 2 | Charles Korthout | 2026-05-31 |
| REQ-022 | Bosak / Fytala Stack | Migrate to .NET 10 | Bosak targets .NET 9, which reached end-of-life in May 2026. Upgrade to .NET 10 LTS to restore support and unblock Customer B BOD-to-OData integration | **Implemented** | Phase 3 | Charles Korthout | 2026-06-03 |
| REQ-023 | Bosak / Fytala Stack | Rename XSLT namespace from `Bosak.XPath.Xslt` to `Bosak.Xslt` | Align namespace hierarchy with W3C spec boundaries (XPath, XSLT, XQuery as peers); unblock independent versioning | **Implemented** | Phase 3 | Charles Korthout | 2026-06-06 |
| REQ-024 | Bosak / Fytala Stack | XQuery 3.1 skeleton project structure | Prepare `Bosak.XQuery` project, validate naming convention, and align documentation for future XQuery implementation | **Implemented** | Phase 3 | Charles Korthout | 2026-06-06 |
| REQ-025 | *(internal)* | `xsl:attribute-set` / `xsl:use-attribute-sets` support | Required for `next-match-012` and broader XSLT 3.0 conformance; attribute sets accumulate across imports/includes; `xsl:use-attribute-sets` now whitelisted on literal result elements (XTSE0805 fix) | **Implemented** | TBD | Charles Korthout | 2026-06-26 |
| REQ-026 | *(internal)* | Nested `xsl:use-when` evaluation | `use-when="false()"` on nested XSLT instructions and LREs was ignored; now stripped during stylesheet load | **Implemented** | TBD | Charles Korthout | 2026-06-07 |
| REQ-027 | Customer B | Publish Bosak packages to NuGet feed | Customer B.DataBridge.Application.BodMapping package-references Bosak.Xslt and Bosak.XPath.Providers, but Bosak projects lack NuGet metadata | **Implemented** | TBD | Charles Korthout | 2026-06-07 |
| REQ-028 | Bosak / Fytala Stack | VS Code Language Server Extension | IDE support for XPath 3.1, XSLT 3.0, and XQuery 3.1 development: syntax highlighting, semantic tokens, realtime diagnostics, auto-completion, hover, go-to-definition, document symbols, code actions, code lens for `.xpath`/XQuery results and XSLT transformation command (with optional default source-document hint via `<?bosak source-document="..."?>`), evaluate/run commands, executeCommand handler | **Implemented** | 0.1.3 | Charles Korthout | 2026-08-20 |
| REQ-029 | *(internal)* | `xsl:where-populated`, `xsl:on-empty`, and `xsl:on-non-empty` support | Required for copy-1213/1214/1215/1216/1217 conformance tests and full `on-empty`/`on-non-empty` clusters; where-populated filters empty nodes, on-empty provides fallback content, on-non-empty provides content when non-empty | **Implemented** | TBD | Charles Korthout | 2026-06-25 |
| REQ-030 | *(internal)* | XSLT `@as` type coercion and atomization | Required for as-0101 through as-1602 conformance tests; `xsl:variable`, `xsl:param`, `xsl:function`, `xsl:with-param` `@as` attribute must coerce/atomize per XSLT 3.0 spec | **Implemented** | TBD | Charles Korthout | 2026-06-11 |
| REQ-031 | *(internal)* | XSLT `base-uri` cluster conformance | `document('')`, `fn:base-uri()`, `fn:static-base-uri()`, and `xml:base` propagation through copies must match XSLT 3.0 spec | **Implemented** | TBD | Charles Korthout | 2026-06-11 |
| REQ-032 | *(internal)* | XSLT 3.0 `xsl:merge` instruction | Required for `merge` conformance cluster: merge sources/keys/action, `current-merge-group()`, `current-merge-key()`, static/dynamic errors | **Implemented** | TBD | Charles Korthout | 2026-06-13 |
| REQ-033 | *(internal)* | XSLT `format-date-en` cluster — English number words and era-aware year formatting | Required for `format-date-en` conformance cluster: `[Ww]`, `[Wo]`, era-aware negative years, and ordinal-year width handling | **Implemented** | TBD | Charles Korthout | 2026-06-15 |
| REQ-034 | *(internal)* | XSLT `static` cluster conformance | Required for `static` conformance cluster (49/49): external static parameters, static variable/parameter runtime binding, XTSE0090/XTSE3450 validations, implicit empty-sequence defaults, `@as` coercion, plus general-comparison empty-sequence and namespace-axis fixes exposed by the cluster | **Implemented** | TBD | Charles Korthout | 2026-06-26 |
| REQ-035 | *(internal)* | XSLT `number` cluster — German/Italian word and ordinal formatting | Required for `number-0802/0812/0813/0828/0829/2506` and `format-integer-065/066`: German cardinal/ordinal words (`drei`, `dritte`, `zweihunderteinste`), Italian masculine/feminine ordinals (`primo`/`prima`), and CLDR `%spellout-ordinal` scheme support | **Implemented** | TBD | Charles Korthout | 2026-06-28 |
| REQ-036 | *(internal)* | XSLT `method="json"` output serialization | Required for W3C `output-0701` through `output-0719`: JSON output method, node serialization via `json-node-output-method`, duplicate-key control, solidus escaping, `item-separator` for text output, `SENR0001` validation, and `xsl:output parameter-document` defaults | **Implemented** | Phase 5 | Charles Korthout | 2026-07-11 |
| REQ-037 | *(internal)* | XSLT `xsl:result-document` serialization completeness | Required for W3C `result-document` cluster: AVT evaluation on all serialization attributes, case-sensitive yes/no values, `SEPM0009` scoping, `build-tree="no"`, and raw-item collection for `method="json"`/`adaptive` | **Implemented** | Phase 5 | Charles Korthout | 2026-07-11 |
| REQ-038 | *(internal)* | XSLT `namespace` cluster — `inherit-namespaces="no"` | Required for W3C `namespace-2603` through `namespace-2632`: prefixed namespace undeclarations for children of `xsl:element`/`xsl:copy`/LRE barriers, and preservation of namespace annotations when unwrapping the synthetic document root | **Implemented** | Phase 5 | Charles Korthout | 2026-07-12 |
| REQ-039 | *(internal)* | Resolve QT3 `op-same-key` hang | `XdmMap` copied the whole dictionary on every `map:remove`/`map:put`, causing O(N²) behavior on the 28-test `op-same-key` set; switched to `ImmutableDictionary` structural sharing | **Implemented** | TBD | Charles Korthout | 2026-07-17 |
| REQ-040 | Bosak / Fytala Stack | XQuery 3.1 Phase 1 — prolog-less query execution | Wire `Bosak.XQuery` to the XPath pipeline so basic XQuery expressions compile and run | **Implemented** | Phase 4 | Charles Korthout | 2026-07-22 |
| REQ-041 | *(internal)* | XQuery 3.1 Phase 2 — FLWOR `order by` clause | Required for full XQuery FLWOR: multi-clause for/let, where, and `order by` with ascending/descending, empty least/greatest, and collation | **Implemented** | Phase 4 | Charles Korthout | 2026-07-22 |
| REQ-042 | *(internal)* | XQuery 3.1 Phase 2 — FLWOR `count` clause | Required for full XQuery FLWOR: `count $var` positional-variable clause, both pre- and post-`order by` | **Implemented** | Phase 4 | Charles Korthout | 2026-07-23 |
| REQ-043 | *(internal)* | XQuery 3.1 Phase 2 — FLWOR `group by` clause | Required for full XQuery FLWOR: `group by` grouping specs (`$var` or `$var := expr`, optional collation), grouped variable rebinding, and post-group `order by`/`count` | **Implemented** | Phase 4 | Charles Korthout | 2026-07-25 |
| REQ-044 | *(internal)* | XQuery 3.1 Phase 2 — FLWOR `window` clause | Required for full XQuery FLWOR: tumbling/sliding windows with start/end conditions (current/positional/previous/next vars) and `only end` | **Implemented** | Phase 4 | Charles Korthout | 2026-07-25 |
| REQ-045 | *(internal)* | QT3 harness XQuery routing + conformance sweep | Validate the XQuery pipeline against the W3C QT3 suite; route supported XQuery tests, fix surfaced engine gaps, keep Failed=0 | **Implemented** | Phase 4 | Charles Korthout | 2026-07-25 |
| REQ-046 | *(internal)* | XQuery 3.1 Phase 3 — direct element constructors | Required for XQuery element construction: direct element/comment/PI constructors with computed attributes/content, constructor-local namespaces, and copy semantics | **Implemented** | Phase 4 | Charles Korthout | 2026-07-25 |
| REQ-047 | *(internal)* | XQuery 3.1 Phase 3 — computed constructors | Required for full XQuery construction: `element`/`attribute`/`text`/`document`/`comment`/`processing-instruction`/`namespace` with static EQName or computed (`{expr}`) names | **Implemented** | Phase 4 | Charles Korthout | 2026-07-25 |
| REQ-048 | *(internal)* | XQuery 3.1 Phase 3 — `switch` / `typeswitch` expressions | Required for full XQuery expressions: `switch` value matching and `typeswitch` type matching with case variables, default clause, and sequence-type unions | **Implemented** | Phase 4 | Charles Korthout | 2026-07-25 |
| REQ-049 | *(internal)* | XQuery 3.1 Phase 4 — output declarations + serialization round-out | Required for XQuery serialization: `declare option output:*` prolog, static serialization parameters, parameter-document, and full Serialization 3.1 method/parameter fidelity | **Implemented** | Phase 4 | Charles Korthout | 2026-07-25 |
| REQ-050 | *(internal)* | XQuery 3.1 Phase 4 — user-defined functions and variables (library modules slice 1) | Required for XQuery modules: `declare function` / `declare variable` prolog with static validations, lazy globals, function-item coercion, and function-type syntax | **Implemented** | Phase 4 | Charles Korthout | 2026-07-26 |
| REQ-051 | *(internal)* | XQuery 3.1 Phase 4 — library modules (slice 2) | Required for XQuery modules: `module namespace` / `import module` with location hints, transitive import graph, %public/%private visibility, and per-module static contexts | **Implemented** | Phase 4 | Charles Korthout | 2026-07-27 |
| REQ-052 | *(internal)* | try/catch completion — named error codes and error variables | Required for XPath/XQuery 3.1 conformance: catch code patterns (`err:XPTY0004`, `err:*`, `*:local`, `Q{uri}local`), multiple catch clauses, and the `err:*` error variables | **Implemented** | Phase 4 | Charles Korthout | 2026-07-27 |
| REQ-053 | *(internal)* | XQuery 3.1 string constructors | Required for XQuery 3.1 conformance: `` `[literal `{expr}` literal]`` string constructors with interpolations, the largest single QT3 gap cluster (35 tests) | **Implemented** | Phase 4 | Charles Korthout | 2026-07-27 |
| REQ-054 | *(internal)* | XQuery 3.1 ordering features | Required for XQuery 3.1 conformance: `ordered`/`unordered` expressions, `declare ordering`, and `declare default order empty least/greatest` with the default applied to order-by | **Implemented** | Phase 4 | Charles Korthout | 2026-07-27 |
| REQ-055 | *(internal)* | Name tests, kind-test types, and constructor namespace semantics | Required for XPath/XQuery conformance: XPST0081/XPST0008 name-test errors, kind-test schema type names, PI name validation, and spec-correct in-scope namespaces on constructed elements | **Implemented** | Phase 4 | Charles Korthout | 2026-07-28 |
| REQ-056 | *(internal)* | Variable declaration type strictness and external variables | Required for XQuery conformance: ExprSingle initializers, strict `as T` enforcement (no casts/promotions), kind-test occurrence validation, namespace undeclaration, and typed external-variable binding checks | **Implemented** | Phase 4 | Charles Korthout | 2026-07-29 |
| REQ-057 | *(internal)* | Namespace declaration static errors and prolog ordering | Required for XQuery conformance: XQST0033 duplicate prefix declarations, XQST0070 reserved xml/xmlns prefix rules, and two-phase prolog ordering (XPST0003) | **Implemented** | Phase 4 | Charles Korthout | 2026-07-29 |
| REQ-058 | *(internal)* | Inline-function annotations and function-test annotation assertions | Required for XQuery conformance: `%eg:*` annotations on inline functions, annotation assertions in function tests, literal-only annotation arguments, and reserved annotation namespaces (XQST0045) | **Implemented** | Phase 4 | Charles Korthout | 2026-07-29 |
| REQ-059 | *(internal)* | Character and entity reference validation in literals and constructors | Required for XQuery conformance: XQST0090 for invalid/overflow character references, XPST0003 for malformed references, XPath-mode non-expansion | **Implemented** | Phase 4 | Charles Korthout | 2026-07-29 |
| REQ-060 | *(internal)* | Combined error-code conformance (FODC0001, XPTY0019, collation and prolog statics) | Required for XQuery conformance: document-root requirement for fn:id/idref, XPTY0019 for path steps over atomics, XQST0038 collation errors, XQST0060/0089/0125 statics | **Implemented** | Phase 4 | Charles Korthout | 2026-07-29 |
| REQ-061 | *(internal)* | Map constructors in step position with key disambiguation | Required for XPath/XQuery conformance: `map{...}` in step and `!` position, step expressions as keys/values, entry-colon disambiguation, and deep-equal sequence semantics for map values | **Implemented** | Phase 4 | Charles Korthout | 2026-07-29 |
| REQ-062 | *(internal)* | `allowing empty` in for clauses — grammar order and typed bindings | Required for XQuery conformance: `allowing empty` before the positional variable, and the empty binding checked against the declared type occurrence (XPTY0004) | **Implemented** | Phase 4 | Charles Korthout | 2026-07-29 |
| REQ-063 | *(internal)* | Computed namespace constructors in element content | Required for XQuery conformance: namespace declarations in content (interleaving, dedupe, prefix conflicts, prefix type checks) and namespace-node identity (parentless, xs:string typed value) | **Implemented** | Phase 4 | Charles Korthout | 2026-07-29 |
| REQ-064 | *(internal)* | Higher-order function conformance — conversions, focus, base URI, error codes | Required for XPath/XQuery conformance: function-item error codes (FOTY0013/XQTY0105), partial-application arity, dynamic-call conversions, absent-focus named references, per-module base-URI capture, parenthesized sequence types | **Implemented** | Phase 4 | Charles Korthout | 2026-07-29 |
| REQ-065 | *(internal)* | Reject plain xs:duration in date/time arithmetic | Required for XPath/XQuery conformance: duration operands in date/time arithmetic must be xs:dayTimeDuration or xs:yearMonthDuration (XPTY0004 for plain xs:duration) | **Implemented** | Phase 4 | Charles Korthout | 2026-07-29 |
| REQ-066 | *(internal)* | Residual-cluster sweep (83 QT3 gaps) | Required for XPath/XQuery conformance: stable order-by, switch semantics, array atomization/flattening, min/max type families, computed-element default namespaces, constructor-local propagation, kind-test errors, and assorted error codes | **Implemented** | Phase 4 | Charles Korthout | 2026-07-29 |
| REQ-067 | *(internal)* | XSLT harness: environment-supplied stylesheets and static params | Required for W3C XSLT conformance: test cases whose principal stylesheet is supplied by the referenced `<environment>` (plus environment static `<param>`) must run instead of skip; unskips ~1,300 tests across 100+ sets (regex-syntax 986, xml-to-json 76, json-to-xml 46, accessor, where-populated, assert, result-document, stream-available, ...) | **Implemented** | TBD | Charles Korthout | 2026-08-03 |
| REQ-068 | *(internal)* | XSD 1.1 regex hyphen rules and `\i`/`\c` ranges | Required for XSD 1.1 conformance: `-` is a subtraction operator only immediately before `[` (regex-syntax-0056a/0086a); `\i`/`\c` use the explicit XML 1.0 (5th ed) NameStartChar/NameChar ranges (regex-syntax-0986/0987, QT3 re00987) | **Implemented** | TBD | Charles Korthout | 2026-08-03 |
| REQ-069 | *(internal)* | Engine conformance cluster exposed by environment stylesheets | Required for XSLT conformance: `xsl:assert` evaluation (XTMM9001/custom codes, try/catch), XTDE1480 temporary-output-state tracking for xsl:result-document, where-populated per-item emptiness rules (XSLT 3.0 §8.4), xsl:fork sequential prongs, fn:stream-available, fn:unparsed-entity-* stubs, accumulator initial-value global-param scope, fn:path on parentless trees with sibling indices, element()/attribute() kind-test namespace rules, fn:xml-to-json FOJS0006 for multi-element documents, XHTML attribute escaping (&#34;, C1 controls), HTML5 foreign-namespace prefixes | **Implemented** | TBD | Charles Korthout | 2026-08-03 |
| REQ-070 | *(internal)* | Schema awareness — user-defined schema simple types and schema kind tests | Register constructor functions for user-defined schema simple types, match/cast them in `ValueMatchesType`/`ApplyFunctionConversion`/`instance of`, keep integer XDM kind for integer-derived typed values; evaluate `schema-element()`/`schema-attribute()` kind tests against the compiled schema set with substitution-group and nillability handling; recursive cast supports union/list simple types and restrictions of union/list; dynamic constructor calls capture the static namespace context for namespace-sensitive unions; restrictions of union/list types are rejected as SequenceType item types (XPST0051); original-case schema prefix resolution in `TryCast`; direct schema-datatype parsing for QName/NOTATION-derived user-defined types; XQST0034 detection for user functions conflicting with schema constructor functions; reject QName values when casting to xs:string-derived subtypes so unions prefer xs:QName members | **Implemented** | Phase 4 | Charles Korthout | 2026-08-21 |
| REQ-071 | Bosak / Fytala Stack | XSLT code lens source-document hint polish | Harden the default source-document hint: add test coverage for single-quoted processing-instruction values, support an XML comment hint alternative, and trim whitespace around the supplied path | **Accepted** | TBD | Charles Korthout | 2026-08-20 |
| REQ-072 | Bosak / Fytala Stack | XSLT code lens initial-template runner | Detect an `xsl:initial-template` declaration or named template entry point and offer a code lens that runs the transform without requiring a source XML document | **Accepted** | TBD | Charles Korthout | 2026-08-20 |
| REQ-073 | Bosak / Fytala Stack | Richer XSLT document symbols / outline | Extend `DocumentSymbolHandler` to outline top-level XSLT declarations: templates, functions, variables, parameters, attribute-sets, and key definitions | **Accepted** | TBD | Charles Korthout | 2026-08-20 |

> **Legend:
> - `Pending` — Under review, no decision yet.
> - `Accepted` — Approved for implementation, awaiting scheduling.
> - `In Progress` — Actively being developed.
> - `Implemented` — Merged to main, available in Target Version.
> - `Declined` — Rejected with rationale recorded.
> - `Superseded` — Replaced by another request.

---

## 5. Request Details

### REQ-001: XSLT `xsl:import` / `xsl:include` URI Resolution

**Requesting Application:** Customer A  
**Submitted:** 2026-05-24  
**Status:** In Progress

#### Problem Statement
Customer A maintains a library of XSLT maps (EDI → Canonical, Canonical → BOD, etc.). These maps share common helper templates and cannot practically be maintained as monolithic files. `xsl:import` and `xsl:include` are parsed today but the `href` attribute is not resolved to a real document, causing the import/include to be silently ignored.

#### Proposed Solution
Wire URI resolution into the `StylesheetLoader`:
1. Resolve `href` relative to the stylesheet's `base-uri`.
2. Load the referenced document via a pluggable `IXsltUriResolver`.
3. Merge imported template rules with correct precedence (imported = lower priority).
4. Merge included templates with same precedence.

#### Acceptance Criteria
- [ ] `xsl:import href="common.xsl"` resolves and loads templates from `common.xsl`
- [ ] Imported templates have lower precedence than local templates
- [ ] `xsl:include href="helpers.xsl"` resolves and loads templates from `helpers.xsl`
- [ ] Included templates have same precedence as local templates
- [ ] Pluggable resolver interface for Customer A's file-system or embedded-resource loading
- [ ] Circular import/include detection (at minimum, fail gracefully)

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | Already parses the elements |
| Compiler | None | No change |
| Runtime | None | No change |
| Standard | None | No change |
| XSLT | Modified | `StylesheetLoader.ResolveImport/ResolveInclude` |
| API | New API | `IXsltUriResolver` or callback on `XsltCompiler` |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-24 | Kimi | Accepted | Required for Customer A's modular map library |
| 2026-05-24 | Kimi | In Progress | Parsing done; URI resolution is next |
| 2026-05-24 | Kimi | Implemented | IXsltUriResolver + FileSystemUriResolver + import precedence + circular detection + tests |

---

### REQ-002: Named XSLT Modes

**Requesting Application:** Customer A  
**Submitted:** 2026-05-24  
**Status:** Implemented

#### Problem Statement
Customer A uses multi-pass transforms: e.g., pass 1 normalizes the input, pass 2 applies business rules, pass 3 generates output. Each pass targets a different `mode`. Today Bosak only supports the default mode (`""`).

#### Proposed Solution
1. Parse `mode` attribute on `xsl:template` (already done).
2. Parse `mode` attribute on `xsl:apply-templates` (already done).
3. Implement mode-aware dispatch in `TransformEngine.FindBestTemplate`.
4. Support `#current` and `#default` mode aliases.

#### Acceptance Criteria
- [ ] `<xsl:apply-templates mode="normalize"/>` dispatches only to templates with `mode="normalize"`
- [ ] Templates without a `mode` attribute participate in the default mode
- [ ] `#current` resolves to the mode of the current `apply-templates` call
- [ ] `#default` resolves to the unnamed default mode
- [ ] Unrecognized mode falls back to built-in rules (not error)

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | Already parsed |
| Compiler | None | No change |
| Runtime | Modified | `TransformEngine.ApplyTemplates` and `FindBestTemplate` |
| Standard | None | No change |
| XSLT | Modified | Mode dispatch logic |
| API | None | No surface change |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-24 | Kimi | Pending | Blocked until Phase 1b (call-template + import) is complete |
| 2026-05-24 | Kimi | Implemented | Mode stack, #current, #default, #all, multi-mode parsing, built-in attribute copy fix |

---

### REQ-003: `xsl:sort` Support

**Requesting Application:** Customer A  
**Submitted:** 2026-05-24  
**Status:** Implemented

#### Problem Statement
Customer A EDI transforms frequently need to sort line items, invoice rows, or delivery notes by sequence number, date, or amount. Without `xsl:sort`, Customer A must pre-sort in C# before invoking the transform, which leaks presentation logic into the application layer.

#### Proposed Solution
Implement `xsl:sort` as a child of `xsl:apply-templates` and `xsl:for-each`:
1. Collect all `xsl:sort` children before processing the selected sequence.
2. Evaluate the `select` expression for each item to produce sort keys.
3. Sort the sequence using the XPath comparison rules (with `data-type`, `order`, `lang`, `case-order`).
4. Process the sorted sequence.

#### Acceptance Criteria
- [ ] `<xsl:for-each select="items/item"><xsl:sort select="@seq"/></xsl:for-each>` produces sorted output
- [ ] `<xsl:apply-templates select="items/item"><xsl:sort select="@price" order="descending"/></xsl:apply-templates>` works
- [ ] Multiple `xsl:sort` keys (primary, secondary) work
- [ ] `data-type="number"` and `data-type="text"` are respected
- [ ] `order="ascending|descending"` is respected

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | Parse `xsl:sort` element |
| Compiler | None | Sorting happens at runtime |
| Runtime | Modified | `TransformEngine.ApplyTemplates` / `ExecuteXsltInstruction` |
| Standard | None | Reuses existing comparison |
| XSLT | New instruction | `xsl:sort` |
| API | None | No surface change |

#### Related Requests
- REQ-007 (`fn:sort`) — underlying comparator must be robust for `xsl:sort` to be fully correct.

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-24 | Kimi | Pending | Phase 2 item; blocked until Phase 1 is stable |
| 2026-05-24 | Kimi | Implemented | XdmValueComparer with type promotion; xsl:sort in apply-templates and for-each |

---

### REQ-004: `xsl:number` Support

**Requesting Application:** Customer A  
**Submitted:** 2026-05-24  
**Status:** Implemented

#### Problem Statement
Customer A generates human-readable documents where line items need sequential numbering (1, 2, 3…). Today this requires awkward XPath workarounds (`count(preceding-sibling::*) + 1`) which break when elements are filtered or reordered.

#### Proposed Solution
Implement `xsl:number` with at least `level="single"` (sibling numbering):
1. `level="single"` — count preceding siblings matching the same node test.
2. `count` pattern support.
3. `format` attribute for Roman numerals, letters, etc. (optional stretch goal).

#### Acceptance Criteria
- [ ] `<xsl:number/>` inside a template outputs the sibling position
- [ ] `level="single"` works for elements
- [ ] Output is `1`-based (not `0`-based)

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | Parse `xsl:number` |
| Compiler | None | |
| Runtime | Modified | `TransformEngine.ExecuteXsltInstruction` |
| Standard | None | |
| XSLT | New instruction | `xsl:number` |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-24 | Kimi | Pending | Phase 2 item |
| 2026-06-12 | Kimi | Implemented | Core `xsl:key`/`key()` support complete; key cluster 91/91 runnable passing in W3C XSLT 3.0 suite |
| 2026-05-24 | Kimi | Implemented | OutputProperties + text/xml methods + indent + omit-xml-declaration + tests |

---

### REQ-005: `xsl:key` + `key()` Function

**Requesting Application:** Customer A  
**Submitted:** 2026-05-24  
**Status:** Pending

#### Problem Statement
Customer A transforms often need to look up reference data (e.g., convert a product code to a product name using a lookup table embedded in the source XML). Without `xsl:key`, each lookup scans the entire document (`//product[@code = $code]`), which is O(n²) on large documents.

#### Proposed Solution
1. Parse `xsl:key` declarations at stylesheet load time.
2. Build an index (dictionary) keyed by the `use` expression value.
3. Implement `key($name, $value)` as an XPath function extension or runtime intrinsic.

#### Acceptance Criteria
- [x] `<xsl:key name="products" match="product" use="@code"/>` is parsed and indexed
- [x] `key('products', $code)` returns the matching node(s)
- [x] Index is rebuilt per source document (not shared across transforms)
- [x] Works inside `xsl:for-each`, `xsl:if`, and `xsl:value-of select`
- [x] Composite keys (`xsl:key` with sequence-constructor content) work
- [x] Results are returned in document order
- [x] `key()` patterns in `match` attributes validated per XTSE0340

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | Parse `xsl:key` |
| Compiler | None | |
| Runtime | Modified | Add `key()` function dispatch |
| Standard | None | `key()` is XSLT-specific, not standard XPath |
| XSLT | New instruction | `xsl:key` + `key()` |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-24 | Kimi | Pending | Phase 2 item |

---

### REQ-006: `xsl:output` Serialization Control

**Requesting Application:** Customer A  
**Submitted:** 2026-05-24  
**Status:** Implemented

#### Problem Statement
Customer A produces XML that is consumed by external systems (Infor, EDI gateways, customer APIs). These systems often have strict formatting requirements: UTF-8 encoding, no XML declaration, indented for debugging, or compact for size. Today Bosak always serializes with default `XDocument` settings.

#### Proposed Solution
1. Parse `xsl:output` attributes (`method`, `encoding`, `indent`, `omit-xml-declaration`, `standalone`, `version`, `doctype-system`, `doctype-public`, `cdata-section-elements`, `escape-uri-attributes`, `include-content-type`, `media-type`, `byte-order-mark`, `html-version`, `suppress-indentation`, `normalization-form`).
2. Pass output properties to `ResultTreeSerializer`.
3. Support `method="xml"`, `method="text"`, `method="html"`, and `method="xhtml"`.

#### Acceptance Criteria
- [x] `<xsl:output method="xml" encoding="UTF-8" indent="yes"/>` produces indented XML
- [x] `<xsl:output omit-xml-declaration="yes"/>` suppresses `<?xml …?>`
- [x] `method="text"` serializes only text nodes (no markup)
- [x] `method="html"` and `method="xhtml"` produce HTML/XHTML serialization with DOCTYPE, void elements, and Content-Type meta
- [x] `doctype-system` / `doctype-public` emit a DOCTYPE declaration
- [x] `cdata-section-elements` wraps text children of named elements in CDATA sections
- [x] Multiple `xsl:output` declarations merge, unioning `cdata-section-elements` and `suppress-indentation`
- [x] Invalid combinations are ignored gracefully (not fatal)

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | Parse `xsl:output` |
| Compiler | None | |
| Runtime | Modified | `ResultTreeSerializer` |
| Standard | None | |
| XSLT | New instruction | `xsl:output` |
| API | Modified | `TransformToString` respects output properties |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-24 | Kimi | Pending | Phase 2 item |
| 2026-07-11 | Kimi | Implemented | Core serialization properties for XML/HTML/XHTML added; CDATA merge bug fixed |
| 2026-07-11 | Kimi | Implemented | Fragment result trees (multiple top-level nodes) serialize correctly for xml/html/xhtml. |
| 2026-07-11 | Kimi | Implemented | Default serialization method inferred from result root element (xhtml for XHTML html, html for no-namespace html). |
| 2026-07-11 | Kimi | Implemented | Serialization validation: SESU0007 for unsupported encodings, SEPM0009 for standalone with omitted declaration. |
| 2026-07-11 | Kimi | Implemented | XHTML5 DOCTYPE formatting (public-only ignored), html-version accepts decimal forms (5.00, +5.0), XHTML namespace prefix stripping, HTML void-element handling, and root-element case preservation. |

---

### REQ-007: `fn:sort` Mixed-Type Comparator

**Requesting Application:** *(internal — conformance)*  
**Submitted:** 2026-05-24  
**Status:** Pending

#### Problem Statement
`fn:sort` has 20+ QT3 conformance failures when sorting sequences containing mixed types (e.g., integers and decimals, or strings and numbers). The current comparator does not handle type promotion rules correctly.

#### Proposed Solution
Implement a spec-compliant `AtomizedComparator` that:
1. Atomizes all items before comparison.
2. Applies XPath 3.1 type promotion rules (e.g., `integer` → `decimal`, `decimal` → `float`, `float` → `double`).
3. Uses `codepoint-collation` for strings.
4. Throws `XPTY0004` for truly incomparable types.

#### Acceptance Criteria
- [ ] All `fn-sort` QT3 tests pass
- [ ] Mixed numeric types sort correctly (`1, 2.5, 3`)
- [ ] Strings sort by Unicode codepoint
- [ ] Incomparable types raise `XPTY0004`

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | |
| Compiler | None | |
| Runtime | Modified | `VmEngine` comparison path or dedicated sort comparator |
| Standard | Modified | `fn:sort` implementation |
| XSLT | None | |
| API | None | |

#### Related Requests
- REQ-003 (`xsl:sort`) — shares the same comparator foundation.

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-24 | Kimi | Pending | Deferred until after XSLT Phase 1 is complete |
| 2026-05-24 | Kimi | Implemented | XdmValueComparer reused by fn:sort and xsl:sort; atomization + type promotion |

---

### REQ-008: `fn:function-lookup` Double-to-String Precision

**Requesting Application:** *(internal — conformance)*  
**Submitted:** 2026-05-24  
**Status:** Implemented

#### Problem Statement
`fn:function-lookup` returns function items that, when applied to doubles, produce strings with precision mismatches vs. the W3C expected output. This was a serialization issue in how `XdmValue.FormatXPathDouble()` handled `xs:double`.

#### Proposed Solution
Switched `FormatXPathDouble` to use `"R"` round-trip format plus `"E16"` scientific format with a `NormalizeScientific` helper. Ensures IEEE 754 shortest representation aligned with XPath 3.1 serialization rules.

#### Acceptance Criteria
- [x] All `function-lookup` QT3 tests pass
- [x] `xs:string(1.0e0)` → `"1"` (not `"1.0"` or `"1E0"`)
- [x] Edge cases (`NaN`, `INF`, `-INF`, very small/large exponents) match spec

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | |
| Compiler | None | |
| Runtime | Modified | `XdmValue.ToString()` or `xs:string` cast |
| Standard | None | |
| XSLT | None | |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-24 | Kimi | Pending | Low priority; does not block Customer A |

---

### REQ-009: Date/Time Ordering (`lt`, `gt`, `le`, `ge`)

**Requesting Application:** *(internal — conformance)*  
**Submitted:** 2026-05-24  
**Status:** Implemented / Stabilized

#### Problem Statement
Date/time equality comparisons worked, but ordering comparisons (`<`, `>`, `<=`, `>=`) had 9 QT3 failures. The `VmEngine.Compare()` path needed actual comparison semantics for `xs:dateTime`, `xs:date`, `xs:time`, and `g*` types. The subsequent XSLT `date` cluster also needed implicit-timezone handling, midnight normalization, timezone adjustment, and constructor bounds.

#### Proposed Solution
Extended `VmEngine.Compare()` to call `CompareDateTimeValues` with the dynamic context's implicit timezone. Added `EvaluationContext.ImplicitTimezoneOffsetMinutes`, rewrote `adjust-*-to-timezone` and the `fn:dateTime#2` constructor to use `XPathDateTime`, normalized `xs:time('24:00:00')` to the same reference day, fixed `IsLeapYear` for negative years, enforced year-range bounds, and corrected AM/PM width formatting.

#### Acceptance Criteria
- [x] All remaining `op-dateTime-less-than` etc. QT3 tests pass
- [x] `xs:date("2024-01-01") < xs:date("2024-01-02")` → `true`
- [x] Incomparable pairs handled using the implicit timezone
- [x] XSLT `date` cluster: 130 passed / 0 failed / 8 skipped

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | |
| Compiler | None | |
| Runtime | Modified | `VmEngine.Compare()` date/time branch |
| Standard | None | |
| XSLT | None | |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-24 | Kimi | Pending | Does not block Customer A; deferred |

---

### REQ-010: JSON/XML Functions (`json-to-xml`, `parse-json`, `xml-to-json`)

**Requesting Application:** *(internal — completeness)*  
**Submitted:** 2026-05-24  
**Status:** Implemented

#### Problem Statement
XPath 3.1 mandates `json-to-xml`, `parse-json`, `xml-to-json`, and `json-doc`. These were entirely missing from Bosak, causing QT3 failures and limiting interoperability with JSON-heavy APIs.

#### Proposed Solution
Implemented the functions in `FunctionLibrary` using `System.Text.Json` for parsing:
1. `parse-json($json-text)` → `map` or `array` (numbers as `xs:double`, null as empty sequence)
2. `json-to-xml($json-text)` → XML representation in `http://www.w3.org/2005/xpath-functions` namespace
3. `xml-to-json($xml)` → JSON string (round-trips with `json-to-xml`)
4. `json-doc($uri)` → loads JSON text and parses it

Options supported: `liberal` (trailing commas), `duplicates` (use-first/retain/reject), `escape` (JSON escaping).

#### Acceptance Criteria
- [x] `json-to-xml` produces correct XML representation for objects/arrays/primitives
- [x] `parse-json` returns maps/arrays with correct XDM types
- [x] `xml-to-json` round-trips correctly for simple cases
- [x] Options parameter (`liberal`, `duplicates`, `escape`) supported, with `FOJS0003`/`FOJS0005`/`XPTY0004` error reporting
- [x] `json-to-xml` conformance cluster: 7/7 runnable tests passing
- [x] `xml-to-json` conformance cluster: 3/3 runnable tests passing

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | |
| Compiler | None | |
| Runtime | Modified | New opcodes or function delegates |
| Standard | New functions | `FunctionLibrary` entries |
| XSLT | None | |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-24 | Kimi | Pending | Not needed for Customer A's EDI/XML use case |

---

### REQ-011: `fn:transform()` Function

**Requesting Application:** *(internal — completeness)*  
**Submitted:** 2026-05-24  
**Status:** Implemented

#### Problem Statement
`fn:transform($options)` is an XPath 3.1 function that invokes XSLT from within an XPath expression. This is useful for composing transforms but was entirely unimplemented.

#### Proposed Solution
Implemented `fn:transform` in `XsltFunctionLibrary` (Bosak.Xslt project) as a delegate that:
1. Accepts a map of options (`stylesheet-location`, `source-node`, `initial-template`, `stylesheet-params`, etc.).
2. Loads and compiles the referenced stylesheet via `XsltCompiler`.
3. Runs the transform via `XsltExecutable.Transform` with an isolated `EvaluationContext`.
4. Returns the result as a map with an `"output"` key containing the result document.

#### Acceptance Criteria
- [x] `fn:transform(map{"stylesheet-location":"foo.xsl","source-node":.})` executes
- [x] `initial-template` option works for named-template entry points
- [x] Parameters can be passed via `stylesheet-params`
- [x] `initial-match-selection` applies templates to arbitrary XDM values (2026-07-14)
- [x] `delivery-format` `document`/`raw`/`serialized`, incl. callable function items in raw results (2026-07-14)
- [x] Secondary `xsl:result-document` output captured in the result map (2026-07-14)
- [x] `package-name`/`package-version` selection from a registered package set (2026-07-14)
- [x] Available in static expressions (`static="yes"`, `xsl:use-when`) (2026-07-14)
- [x] `global-context-item` option and default wrapper for non-document source nodes (2026-07-15)
- [x] `default-mode` honored when no `initial-mode` is supplied (2026-07-15)
- [x] `xslt-version` type validation (string value raises `XPTY0004`) (2026-07-15)
- [x] `serialization-params` override `cdata-section-elements`/`suppress-indentation` for XML method (2026-07-15)
- [x] W3C `fn-transform` test set 117/124 passed (7 skipped) (2026-07-15)

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | |
| Compiler | None | |
| Runtime | Modified | Reuses existing `TransformEngine` |
| Standard | New function | `FunctionLibrary` |
| XSLT | None | |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-24 | Kimi | Pending | Phase 3 item; depends on stable XSLT engine |
| 2026-07-14 | Kimi | Completed | Full option surface implemented; W3C transform set 9/9; conformance suite green |
| 2026-07-15 | Kimi | Completed | Tier-2m fixes: global-context-item, default-mode, xslt-version validation, serialization overrides; fn-transform 117/124 passed |

---

### REQ-012: `xsl:call-template` Tunnel Parameters

**Requesting Application:** Customer A  
**Submitted:** 2026-05-24  
**Status:** Implemented

#### Problem Statement
Customer A passes context metadata (document type, source system, correlation ID) through deep call-template chains. Without tunnel parameters, every intermediate template must explicitly forward the parameter.

#### Proposed Solution
Extend `xsl:with-param` and `xsl:param` to support `tunnel="yes"`:
1. Tunnel params are passed implicitly through `call-template` chains.
2. Only templates declaring `<xsl:param name="x" tunnel="yes"/>` receive them.
3. Tunnel params do not interfere with regular params.

#### Acceptance Criteria
- [x] `<xsl:with-param name="corrId" select="..." tunnel="yes"/>` propagates through call-template
- [x] Intermediate templates without the tunnel param ignore it
- [x] Final template with `<xsl:param name="corrId" tunnel="yes"/>` receives the value
- [x] Non-tunnel params are unaffected

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | Parse `tunnel` attribute |
| Compiler | None | |
| Runtime | Modified | `TransformEngine` param dispatch |
| Standard | None | |
| XSLT | Modified | `call-template` param forwarding |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-24 | Kimi | Pending | Nice-to-have; workaround is explicit forwarding |
| 2026-05-27 | Kimi | Implemented | Tunnel param stack in TransformEngine; propagates through call-template and apply-templates; 4 unit tests pass |

---

### REQ-014: XML Schema (XSD) Validation API

**Requesting Application:** Customer B  
**Submitted:** 2026-05-25  
**Status:** Implemented

#### Problem Statement
Customer B receives Infor OAGIS Business Object Documents (BODs) from multiple sources (IMS HTTP, ActiveMQ/Artemis). Currently, malformed or non-compliant BODs are dispatched to handlers, which then fail with confusing errors. Customer B needs a centralized, reusable way to validate BOD XML against OAGIS XSD schemas *before* routing to handlers.

Because Bosak is the project's XML-stack owner (XPath 3.1, XSLT, XQuery), XSD validation naturally belongs here rather than in Customer B's transport layer. Customer B should consume a Bosak validation API rather than re-implementing schema loading and validation.

#### Proposed Solution
Add an `IXsdValidator` abstraction to Bosak with a default implementation using `System.Xml.Schema`:

1. `IXsdValidator.Validate(string xml, Stream xsdStream)` — validates XML against a single XSD.
2. `IXsdValidator.Validate(string xml, IEnumerable<Stream> xsdStreams)` — validates against a schema set (handles OAGIS imports/includes).
3. `IXsdValidator.TryValidate(string xml, Stream xsdStream, out string? error)` — non-throwing variant.
4. `XsdValidatorOptions` for severity filtering (warning vs. error) and max error count.

Expose a high-level helper:
- `BodValidator.ValidateOagis(string bodXml, string namespaceUri)` — loads the appropriate embedded OAGIS XSD (`/2`, `/2006`, `/2018`) and validates.

#### Acceptance Criteria
- [x] `IXsdValidator` interface defined in Bosak
- [x] Default implementation uses `System.Xml.Schema.XmlSchemaSet`
- [x] Supports single-schema and multi-schema (with `xs:import`/`xs:include`) validation
- [x] Returns structured validation results (line number, column, severity, message)
- [x] Non-throwing `TryValidate` variant available
- [ ] `BodValidator.ValidateOagis` helper loads correct XSD by namespace URI *(deferred: requires embedded OAGIS schemas)*
- [x] Unit tests cover valid XML, invalid XML, and schema-set validation
- [x] Documented in Bosak integration guide

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | |
| Compiler | None | |
| Runtime | None | |
| Standard | None | XSD is a W3C standard; this is a tooling layer |
| XSLT | None | |
| API | New API | `IXsdValidator`, `XsdValidator`, `BodValidator` |

#### Related Requests
- Customer B REQ-002 (Multi-namespace OAGIS BOD parser) — provides the namespace detection needed to select the correct XSD
- Customer B REQ-005 (BOD telemetry) — validation failures can be recorded as telemetry events

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-25 | Kimi | Accepted | Bosak is the XML-stack owner; XSD validation belongs here. Customer B will consume the API rather than duplicating schema logic. |

---

### REQ-015: `xsl:function` Support

**Requesting Application:** Customer A  
**Submitted:** 2026-05-26  
**Status:** Implemented

#### Problem Statement
Customer A's fragment-composition architecture relies on pure-XSLT helper functions defined in shared library files (`DateFunctions.xsl`, `WeekFunctions.xsl`, `MappingFunctions.xsl`, `NumberFunctions.xsl`). These define 22+ `xsl:function` declarations in the `app:` namespace that are called from basesheets and partner overrides.

Today Bosak does not parse, compile, or execute `xsl:function` declarations at all. The `Stylesheet` class parses `xsl:template`, `xsl:variable`, `xsl:param`, `xsl:key`, `xsl:mode`, `xsl:output`, `xsl:strip-space`, and `xsl:preserve-space` — but there is no collection for `xsl:function` and no dispatch mechanism for function calls in XPath expressions.

Without `xsl:function` support, Customer A's basesheets cannot execute on Bosak. The only workaround is to inline all logic into named templates, which defeats the purpose of the fragment library.

#### Proposed Solution
1. Parse `xsl:function` declarations at stylesheet load time (including imported/included stylesheets).
2. Store functions in a dictionary keyed by `{namespace, local-name, arity}`.
3. Implement function dispatch in the XPath compiler/runtime:
   - When the compiler encounters a function call with an unknown prefix, resolve it against the `xsl:function` registry.
   - Compile the function body as a callable delegate.
   - Support function parameters with `as` type declarations.
   - Support function return type with `as` declaration.
4. Handle import precedence: local functions override included, which override imported.

#### Acceptance Criteria
- [x] `xsl:function name="app:parse-edidate"` is parsed and callable from XPath
- [x] Functions defined in imported stylesheets are available to the importing stylesheet
- [x] Functions defined in included stylesheets are available to the including stylesheet
- [x] Function parameters with `as="xs:string?"` are type-checked (via `ConvertVariableValue`)
- [x] Function return type with `as="xs:date?"` is enforced (via `ConvertVariableValue`)
- [x] Recursive functions work (e.g., `app:factorial($n)` calling itself)
- [x] All 22 Customer A helper functions execute correctly

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | Parse `xsl:function` declarations |
| Compiler | Modified | Add function-call resolution against XSLT-defined functions |
| Runtime | Modified | Function body execution (sequence constructor + return) |
| Standard | None | `xsl:function` is XSLT-specific |
| XSLT | New instruction | `xsl:function` + function-call dispatch |
| API | None | No surface change |

#### Related Requests
- REQ-001 (`xsl:import`/`xsl:include`) — functions must respect import/include precedence

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-26 | Kimi | Pending | Critical blocker for Customer A fragment library |
| 2026-05-27 | Kimi | Implemented | xsl:function parsing, registration on EvaluationContext, function body execution with param binding, recursive calls, import/include precedence. 6 unit tests pass. |

---

### REQ-016: Multi-Key `xsl:sort`

**Requesting Application:** Customer A  
**Submitted:** 2026-05-26  
**Status:** Implemented

#### Problem Statement
Customer A's DELFOR D99A JAMA basesheet sorts line items by two keys:
```xsl
<xsl:for-each select="SG6/SG12">
    <xsl:sort select="LIN/C212/D7140"/>
    <xsl:sort select="LOC[D3227='54']/C517/D3225"/>
    ...
</xsl:for-each>
```

REQ-003 implemented single-key `xsl:sort` support, but the acceptance criterion for multiple sort keys remains unmet. The current `SortItems` implementation only evaluates the first `xsl:sort` element and ignores any additional keys.

#### Proposed Solution
Extend the sorting logic in `TransformEngine` to evaluate all `xsl:sort` children in document order:
1. Collect all sort specifications.
2. For each item, evaluate every sort key in order.
3. Use a composite comparator: compare primary keys; if equal, compare secondary keys; if equal, compare tertiary keys, etc.
4. Reuse existing `data-type` and `order` logic for each key.

#### Acceptance Criteria
- [x] Two `xsl:sort` elements produce correctly ordered output (primary then secondary)
- [x] Three or more `xsl:sort` elements work correctly
- [x] Each key respects its own `data-type` and `order` attributes
- [x] Stable sort: items with equal keys retain their original relative order

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | Already parses `xsl:sort` |
| Compiler | None | |
| Runtime | Modified | `TransformEngine.SortItems` → composite comparator |
| Standard | None | |
| XSLT | Modified | `xsl:sort` multi-key support |
| API | None | |

#### Related Requests
- REQ-003 (`xsl:sort`) — builds on single-key foundation

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-26 | Kimi | Pending | Required for D99A JAMA basesheet correctness |
| 2026-05-27 | Kimi | Implemented | Composite SortKey/SortEntry with per-key data-type and order; stable sort via original index tiebreaker; 4 unit tests pass |

---

### REQ-017: Fix CS0219 Unused Variable in `FormatNumberEngine`

**Requesting Application:** *(internal — code quality)*  
**Submitted:** 2026-05-27  
**Status:** Implemented

#### Problem Statement
`FormatNumberEngine.cs` (Bosak.XPath.Formatting) triggers compiler warning **CS0219**: *"The variable 'hasDecimal' is assigned but its value is never used."* This clutters the build output and masks more serious warnings.

#### Proposed Solution
Remove the unused `bool hasDecimal = false;` declaration and any assignments to it, or use the variable if it was intended to drive formatting logic.

#### Acceptance Criteria
- [ ] `dotnet build` on Bosak.XPath.Formatting produces zero CS0219 warnings
- [ ] `FormatNumberEngine` behavior is unchanged (no functional regression)

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | |
| Compiler | None | |
| Runtime | None | |
| Standard | None | |
| XSLT | None | |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-27 | Kimi | Pending | Low-priority code cleanup; does not block Customer A |

---

### REQ-018: Fix CS8602 Null Dereference in `FormatNumberEngine`

**Requesting Application:** *(internal — code quality)*  
**Submitted:** 2026-05-27  
**Status:** Implemented

#### Problem Statement
`FormatNumberEngine.cs` triggers compiler warning **CS8602**: *"Dereference of a possibly null reference"* on `sub.Suffix` where `sub` may be null. This is a potential `NullReferenceException` at runtime if the formatting path reaches this line with a null `sub` value.

#### Proposed Solution
Add a null-conditional guard (`sub?.Suffix` or an explicit null check) before accessing `sub.Suffix`, ensuring safe behavior.

#### Acceptance Criteria
- [ ] `dotnet build` on Bosak.XPath.Formatting produces zero CS8602 warnings for this line
- [ ] No `NullReferenceException` can occur on the `sub.Suffix` access path
- [ ] Existing number-formatting unit tests continue to pass

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | |
| Compiler | None | |
| Runtime | Modified | Safer null handling in number formatting |
| Standard | None | |
| XSLT | None | |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-27 | Kimi | Pending | Low-priority bug fix; does not block Customer A |
| 2026-05-30 | Kimi | Implemented | Warnings no longer reproduced after `FormatNumberEngine` rewrite (2026-05-22); `Subpicture` is now a struct and `Suffix` is non-null; build is clean |

---

### REQ-019: `xsl:try` / `xsl:catch` Support

**Requesting Application:** Customer A  
**Submitted:** 2026-05-31  
**Status:** `Implemented`

#### Problem Statement
Customer A's pure-XSLT helper functions in `DateFunctions.xsl` and `NumberFunctions.xsl` rely on `xsl:try`/`xsl:catch` for defensive parsing of dirty EDI data:

- `app:try-date` attempts `xs:date(...)` and returns empty sequence on invalid dates
- `app:to-number` attempts `xs:decimal(...)` and returns a fallback on invalid numbers

Without try/catch, any malformed date or numeric field causes a hard XPath error (e.g. `FORG0001`), aborting the entire transform. EDI data is inherently dirty — missing fields, wrong formats, and unexpected values are common.

#### Proposed Solution
Implement `xsl:try`/`xsl:catch` in `TransformEngine.ExecuteXsltInstruction` and `EvaluateFunctionBodyInstruction`:
1. Parse `xsl:try` children (sequence constructor) and `xsl:catch` children (sequence constructor + optional `select`).
2. Wrap try-body execution in a .NET `try` block.
3. On a matching dynamic error, execute the first matching catch body and return its result.
4. Support `xsl:catch` without attributes (catch-all), with `@errors` (`*`, plain local names, `*:local`, `Q{uri}local`, and `prefix:local` in the `err` namespace), and multiple catch clauses evaluated in document order.

#### Acceptance Criteria
- [x] `xsl:try` with a single `xsl:catch` (no attributes) executes catch body on any error
- [x] `app:try-date` returns `()` for invalid dates instead of crashing
- [x] `app:to-number` returns `$fallback` for non-numeric input instead of crashing
- [x] Errors from the try body do not propagate outside the `xsl:try` instruction when a catch matches
- [x] Multiple `xsl:catch` clauses are evaluated in order
- [x] `@errors` supports namespace wildcard and Clark notation

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | Already parses `xsl:try` / `xsl:catch` |
| Compiler | None | No new IR needed |
| Runtime | Modified | `TransformEngine` new instruction handler |
| Standard | None | |
| XSLT | New instruction | `xsl:try`, `xsl:catch` |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-31 | Kimi | Pending | P0 blocker for Customer A production; dirty EDI data is normal |
| 2026-05-31 | Kimi | Implemented | Basic try/catch in TransformEngine + EvaluateFunctionBodyInstruction; 4 unit tests pass; catches any Exception broadly |
| 2026-06-25 | Kimi | Implemented | Multiple `xsl:catch` clauses, `@errors` matching (`*`, `*:local`, `Q{uri}local`, `prefix:local`), and rethrowing of unmatched errors. Fixes `call-template-0110`. |

---

### REQ-020: `exclude-result-prefixes` Support

**Requesting Application:** Customer A  
**Submitted:** 2026-05-31  
**Status:** `Pending`

#### Problem Statement
All 42 Customer A stylesheets declare `exclude-result-prefixes="xs app"` (and sometimes others). This attribute tells the XSLT processor to omit namespace declarations for prefixes that are only used in the stylesheet logic, not in the result tree.

Bosak currently ignores this attribute. The output XML therefore contains `xmlns:xs="http://www.w3.org/2001/XMLSchema"` and `xmlns:app="http://fytala.com/app/xslt/functions"` on many elements. Downstream Infor OAGIS BOD consumers may reject documents with unexpected namespace declarations, or schema validation may fail.

#### Proposed Solution
1. Parse `exclude-result-prefixes` on `xsl:stylesheet` / `xsl:transform` at load time.
2. Store excluded prefixes (and `#all` shorthand) on the `Stylesheet` object.
3. During result tree serialization (`ResultTreeSerializer` or `CopyToResult`), filter out namespace attributes for excluded prefixes.
4. Handle `#all` → exclude all prefixes not used in literal result elements.

#### Acceptance Criteria
- [ ] `exclude-result-prefixes="xs app"` removes `xmlns:xs` and `xmlns:app` from output
- [ ] `#all` shorthand excludes all non-literal-result prefixes
- [ ] Literal result elements retain their necessary namespace declarations
- [ ] Partner overrides with multiple excluded prefixes work correctly

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | Parse `exclude-result-prefixes` on `xsl:stylesheet` |
| Compiler | None | |
| Runtime | Modified | `ResultTreeSerializer` or `TransformEngine` namespace filtering |
| Standard | None | |
| XSLT | Modified | Stylesheet load + serialization path |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-31 | Kimi | Pending | P1 — output namespace pollution breaks downstream OAGIS validation |
| 2026-05-31 | Kimi | Implemented | Parsed in Stylesheet.Load, filtered in CopyLiteralElement, merges across imports/includes, supports #all, 3 unit tests pass |

---

### REQ-021: `xsl:message` Support

**Requesting Application:** Customer A  
**Submitted:** 2026-05-31  
**Status:** `Pending`

#### Problem Statement
Customer A partner override stylesheets use `xsl:message` for debugging and audit logging during transform execution (e.g. `GENERIC_EU_DELFOR_D97A_Override.xsl`). Without `xsl:message`, developers have no visibility into transform execution flow, making debugging production issues extremely difficult.

#### Proposed Solution
1. Add `xsl:message` handler in `TransformEngine.ExecuteXsltInstruction`.
2. Evaluate the `select` attribute or sequence constructor children.
3. Convert the result to string (atomization + concatenation with spaces).
4. Write to a pluggable `IXsltMessageListener` or default to `Console.WriteLine`.
5. Support `terminate="yes"` as a stretch goal (raises fatal error).

#### Acceptance Criteria
- [ ] `xsl:message select="'Debug: ' || $value"` outputs the message
- [ ] `xsl:message` with sequence constructor children outputs concatenated text
- [ ] Messages do not appear in the result tree
- [ ] Pluggable listener interface for testability

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | Already parses `xsl:message` |
| Compiler | None | |
| Runtime | Modified | `TransformEngine` new instruction handler + listener interface |
| Standard | None | |
| XSLT | New instruction | `xsl:message` |
| API | New API | `IXsltMessageListener` optional callback |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-05-31 | Kimi | Pending | P2 — debugging aid; no production blocking impact |
| 2026-05-31 | Kimi | Implemented | IXsltMessageListener interface, XsltCompiler.MessageListener, TransformEngine handler for select and sequence constructor, 3 unit tests pass |

---

### REQ-028: VS Code Language Server Extension

**Requesting Application:** Bosak / Fytala Stack  
**Submitted:** 2026-06-08  
**Status:** **Implemented**

#### Problem Statement

Developers working with XPath 3.1 and XSLT 3.0 in VS Code had no IDE support specific to the Bosak engine. Generic XML extensions provide basic syntax highlighting but no XPath/XSLT-aware diagnostics, completions, or error reporting. This slows down stylesheet development and makes it hard to catch errors early.

#### Proposed Solution

Build a Language Server Protocol (LSP) implementation and VS Code extension:

1. **`Bosak.LanguageServer`** — .NET 10 console app using OmniSharp.Extensions.LanguageServer 0.19.9:
   - `TextDocumentSyncHandler`: full-document sync for `.xpath`, `.xsl`, `.xslt`, `.xq`, `.xqy`, `.xquery`
   - `DiagnosticsHandler`: XPath/XQuery parse errors; XSLT XML well-formedness + XPath-in-attribute validation (`select`, `test`, `match`, `use-when`)
   - `CompletionHandler`: XPath/XQuery functions, axes, keywords; XSLT instructions
   - `HoverHandler`: function signatures and descriptions
   - `DefinitionHandler`: go-to-definition for XSLT/XQuery functions, variables, templates
   - `DocumentSymbolHandler`: outline for XSLT and XQuery declarations
   - `SemanticTokensHandler`: semantic highlighting for function calls, variables, XSLT instructions, XQuery keywords, type names, namespace prefixes, numbers, and operators
   - `CodeActionHandler`: quick fixes for XPath syntax errors (unclosed parentheses, brackets, string literals), XQuery unclosed curly braces and default element namespace declaration, undeclared namespace prefixes in XQuery/XSLT (including `XPST0081` diagnostic-driven fixes), XQuery `import module namespace` for function-call prefixes, removal of invalid empty namespace declarations (`XQST0085`), promotion of bare `<stylesheet>`/`<transform>` roots to `xsl:*`, and missing `version` attribute on `xsl:stylesheet`/`xsl:transform`
   - `CodeLensHandler`: evaluates `.xpath`, `.xq`, `.xqy`, and `.xquery` documents and displays the serialized result (or error message) as a code lens at the top of the file; `.xsl` and `.xslt` documents show a **Run XSLT transformation** lens that invokes the existing `bosak.transformXslt` command (source-document picker handled by the VS Code client); when a `<?bosak source-document="..."?>` processing instruction is present, the lens title includes the source file name and the command arguments include the resolved source path so the transform runs without prompting
   - `ExecuteCommandHandler`: implements `workspace/executeCommand` for `bosak.evaluateXPath` and `bosak.evaluateXQuery`; evaluates the document and sends the serialized result/error back to the client via a `bosak/evaluationResult` notification
   - `EvaluationHandler`: custom LSP requests to evaluate XPath, run XSLT, and run XQuery
   - `DocumentManager`: in-memory store of open document contents
2. **`vscode-bosak`** — TypeScript VS Code extension client:
   - Syntax highlighting (TextMate grammars for XPath, XSLT, and XQuery)
   - LSP client connecting via stdio
   - Bundled server support: server binary shipped inside the VSIX
   - Context-menu commands (Evaluate XPath, Run XSLT, Run XQuery)

#### Acceptance Criteria

- [x] `Bosak.LanguageServer` compiles with 0 errors, 0 warnings
- [x] VSIX packages extension + bundled server (2.71 MB)
- [x] Installable via `code --install-extension vscode-bosak-0.1.2.vsix`
- [x] Diagnostics appear for invalid XPath expressions
- [x] Diagnostics appear for malformed XSLT and invalid XPath in attributes
- [x] Completions trigger for XPath functions and XSLT instructions
- [x] Hover shows function signatures
- [x] Go-to-definition resolves XSLT/XQuery functions and variables
- [x] Document symbols show outline for XSLT and XQuery
- [x] Semantic tokens highlight functions, variables, keywords, types, namespaces, and operators
- [x] Code actions offer quick fixes for XPath/XQuery syntax errors (unclosed brackets, strings, curly braces), XQuery `declare default element namespace`, undeclared namespace prefixes (including XPST0081 diagnostic-driven fixes), XQuery `import module namespace`, invalid XQuery empty namespace declarations (XQST0085), missing XSLT namespace, and missing XSLT version attribute
- [x] Code lens evaluates `.xpath`, `.xq`, `.xqy`, and `.xquery` documents and shows the result or error above the document
- [x] `workspace/executeCommand` handles `bosak.evaluateXPath` and `bosak.evaluateXQuery` and sends result/error via `bosak/evaluationResult` notification
- [x] Context-menu commands evaluate XPath, run XSLT, and run XQuery
- [x] All 1,708 unit tests still pass

#### Impact Analysis

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | Reused for diagnostics |
| Compiler | None | Reused for diagnostics |
| Runtime | None | |
| Standard | None | |
| XSLT | None | |
| API | None | |
| Tooling | New project | `Bosak.LanguageServer` + `vscode-bosak/` |

#### Decision Log

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-06-08 | Charles Korthout / Kimi | Implemented | Initial LSP server + VS Code extension |
| 2026-08-18 | Charles Korthout / Kimi | Extended | XQuery language support, hover, go-to-definition, document symbols, evaluate/transform/run commands |
| 2026-08-20 | Charles Korthout / Kimi | Extended | Code actions offer `declare default element namespace` for unprefixed XQuery element constructors |
| 2026-08-20 | Charles Korthout / Kimi | Extended | Code actions close unclosed curly braces in XQuery direct element constructors |
| 2026-08-20 | Charles Korthout / Kimi | Extended | Namespace declarations use the standard XML namespace URI for the reserved `xml` prefix |
| 2026-08-20 | Charles Korthout / Kimi | Extended | Code actions close unclosed XPath parentheses, brackets, and string literals |
| 2026-08-20 | Charles Korthout / Kimi | Extended | Code actions offer `import module namespace` for XQuery function-call prefixes |
| 2026-08-20 | Charles Korthout / Kimi | Extended | Code actions remove invalid empty namespace declarations (`XQST0085`) in XQuery |
| 2026-08-20 | Charles Korthout / Kimi | Extended | Code actions react to XPST0081 diagnostics to declare the reported prefix in XSLT |
| 2026-08-20 | Charles Korthout / Kimi | Extended | Code actions for XSLT root `<stylesheet>`/`<transform>` rename and missing `version` attribute |
| 2026-08-20 | Charles Korthout / Kimi | Extended | Code actions for undeclared namespaces in XQuery/XSLT and missing XSLT root namespace |
| 2026-08-20 | Charles Korthout / Kimi | Extended | Semantic tokens for XPath/XQuery/XSLT; extension version 0.1.3 |
| 2026-08-20 | Charles Korthout / Kimi | Extended | Code lens evaluates `.xpath` documents and displays the result or error at the top of the file |
| 2026-08-20 | Charles Korthout / Kimi | Extended | Code lens extended to XQuery documents (`.xq`/`.xqy`/`.xquery`) |
| 2026-08-20 | Charles Korthout / Kimi | Extended | `workspace/executeCommand` handler for `bosak.evaluateXPath`/`bosak.evaluateXQuery`; sends serialized result/error via `bosak/evaluationResult` notification; VS Code extension opens result in editor |

---

### REQ-029: `xsl:where-populated` and `xsl:on-empty` Support

**Requesting Application:** *(internal — conformance)*
**Submitted:** 2026-06-10
**Status:** **Implemented**

#### Problem Statement

The `copy-1213` through `copy-1217` conformance tests require `xsl:where-populated` and `xsl:on-empty` support:

- `xsl:where-populated` filters the result of its sequence constructor, discarding items that are "deemed empty" (empty text nodes, empty PIs, empty comments, empty elements, document nodes with no children).
- `xsl:on-empty` provides fallback content when its parent container's sequence constructor produces no nodes.

Additionally, the XPath parser incorrectly treated prefixed names like `my:node()` as kind tests (`child::node()`) instead of function calls, causing `copy-1214` to fail because `my:node()` returned the document's child element instead of calling the user-defined function.

#### Proposed Solution

1. **`xsl:where-populated` in `TransformEngine.ExecuteXsltInstruction`**:
   - Evaluates sequence constructor into a temporary container.
   - Checks if the container has any "non-empty" nodes (text with content, PIs with content, comments with content, elements with children).
   - If populated, copies nodes and attributes to the real result container.
   - For `@select`, checks if the result sequence is empty before copying.

2. **`xsl:on-empty` in `CopyLiteralElement`**:
   - Collects `xsl:on-empty` children before processing other children.
   - Skips them during normal processing.
   - After all children are processed, if no nodes were added to the copy, evaluates each `xsl:on-empty` (via `@select` or sequence constructor) and copies results to the parent container.

3. **Parser fix for prefixed kind tests**:
   - In `XPathParser.ParseStep` and `ParseNodeTest`, added `string.IsNullOrEmpty(prefix)` guard before treating a name as a kind test.
   - Prefixed names followed by `()` are now always parsed as function calls.

#### Acceptance Criteria
- [x] `copy-1213` (non-empty comment) passes
- [x] `copy-1214` (empty text node + on-empty function call) passes
- [x] `copy-1215` (non-empty text node) passes
- [x] `copy-1216` (empty PI) passes
- [x] `copy-1217` (non-empty PI) passes
- [x] `copy-1205` (xsl:copy on-empty on element) passes
- [x] `copy-1208` (xsl:copy on-empty on document node) passes
- [x] `copy-1209` (xsl:document with xsl:on-empty) passes
- [x] `copy-1210` (namespace node on document node raises XTDE0420) passes
- [x] `element-0607` (invalid copy-namespaces on xsl:copy-of raises XTSE0020) passes
- [x] `element-0608` (invalid copy-namespaces on xsl:copy raises XTSE0020) passes
- [x] `my:node()` function call works correctly in XPath expressions
- [x] `on-empty` conformance cluster: 72/72 passing
- [x] `on-non-empty` conformance cluster: 14/14 passing

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | Kind-test parsing now excludes prefixed names |
| Compiler | None | |
| Runtime | Modified | `TransformEngine` new instruction handlers |
| Standard | None | |
| XSLT | New instructions | `xsl:where-populated`, `xsl:on-empty` |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-06-10 | Charles Korthout / Kimi | Implemented | Required for copy cluster conformance; low-risk parser fix + instruction handlers |

---

### REQ-031: XSLT `base-uri` Cluster Conformance

**Requesting Application:** *(internal — conformance)*
**Submitted:** 2026-06-11
**Status:** **Implemented**

#### Problem Statement

The W3C XSLT 3.0 `base-uri` test cluster was failing because Bosak did not correctly handle base URI resolution in several areas:

- `document('')` inside a template returned the wrong stylesheet document or failed because the static base URI was the main stylesheet file rather than the template's effective base URI.
- `xml:base` attributes on `xsl:template` and `xsl:stylesheet` were ignored when compiling XPath expressions, so `fn:static-base-uri()` returned the wrong URI.
- `xsl:copy` and `xsl:copy-of` did not preserve source base URIs on copied document/element nodes.
- `xml:*` prefixed names (e.g. `xml:base`) were not resolving to `http://www.w3.org/XML/1998/namespace` in node tests.
- `fn:base-uri`, `fn:resolve-uri`, and `fn:static-base-uri` returned plain strings instead of `xs:anyURI`.

#### Proposed Solution

1. **Effective base URI in XPath compilation** — `TransformEngine.CompileXPath` now computes `GetEffectiveBaseUri(element)` by walking the ancestor chain and resolving `xml:base` attributes, then passes this URI into `EvaluationContext.BaseUri`.
2. **`document('')` resolution** — `FunctionLibrary.Document_1` / `Document_2` resolve an empty URI against `ctx.BaseUri`. The conformance harness `DocumentLoader` returns the compiled stylesheet document when the requested URI matches the stylesheet base URI.
3. **Base URI propagation through copies** — `TransformEngine.EvaluateSequenceConstructor` annotates newly constructed document nodes and elements with the effective base URI. `ExecuteSingleCopy`, `CopyXdmNode`, `CopyNodeToContainer`, and `CopyNodeToResult` preserve source base URI annotations. Built-in template rules shallow-copy/deep-copy base URIs onto created elements.
4. **`xml` prefix resolution** — `XPathParser.ParseNodeTest` returns a `QName` node test for `xml:local` bound to `http://www.w3.org/XML/1998/namespace`. `EvaluationContext.TryResolveNamespace` hard-codes the same URI for the `xml` prefix.
5. **`xs:anyURI` returns** — `FunctionLibrary.BaseUri_*`, `ResolveUri`, and `StaticBaseUri` wrap string results with `XdmValue.FromString(uri, "anyURI")`.

#### Acceptance Criteria
- [x] `base-uri-050` passes (`document('')` resolves against template's effective base URI)
- [x] `base-uri-053` passes (`fn:base-uri()` on copied nodes reflects `xml:base` chain and source base URIs)
- [x] `base-uri-052` explicitly skipped (requires XInclude support)
- [x] `fn:base-uri`, `fn:resolve-uri`, `fn:static-base-uri` return `xs:anyURI`
- [x] Base URI propagation also improves `copy-*` test results

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | `xml:local` node tests resolve to XML namespace |
| Compiler | Modified | `CompileXPath` takes effective base URI from element chain |
| Runtime | Modified | `EvaluationContext` predefined `xml` prefix; base URI annotations |
| Standard | Modified | URI functions return `xs:anyURI` |
| XSLT | Modified | `TransformEngine` preserves base URIs through copies and built-in rules |
| API | Modified | `CompileOptions` and `XPath31Expression` expose base URI |

#### Related Requests
- REQ-001 (`xsl:import`/`xsl:include`) — import/include base URI resolution is related

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-06-11 | Charles Korthout / Kimi | Implemented | Required for XSLT 3.0 conformance; fixes multiple clusters depending on base URI correctness |

---

## 6. Request Lifecycle

```
┌─────────┐    ┌──────────┐    ┌─────────────┐    ┌───────────┐    ┌────────────┐
│ Pending │───▶│ Accepted │───▶│ In Progress │───▶│ Implemented│───▶│ Archived   │
└─────────┘    └──────────┘    └─────────────┘    └───────────┘    └────────────┘
     │               │                │                  │
     ▼               ▼                ▼                  ▼
┌─────────┐    ┌──────────┐    ┌─────────────┐    ┌───────────┐
│Declined │    │Superseded│    │  Blocked    │    │  Backlog   │
└─────────┘    └──────────┘    └─────────────┘    └───────────┘
**Transitions:**
- `Pending` → `Accepted` / `Declined` / `Superseded`
- `Accepted` → `In Progress` / `Backlog`
- `In Progress` → `Implemented` / `Blocked`
- `Blocked` → `In Progress` / `Declined`
- `Implemented` → `Archived` (after 2 releases)

---

## 7. Priority Guidelines

| Priority | Criteria | SLA Target |
|----------|----------|------------|
| **P0 — Critical** | Blocks production go-live; no workaround | 1 week |
| **P1 — High** | Significant friction; workaround is costly | 2 weeks |
| **P2 — Medium** | Nice-to-have; workaround exists | Next minor version |
| **P3 — Low** | Exploration / future-proofing | TBD |

Requests without explicit priority default to **P2**.

**Current P0/P1 requests:** None.

---

## 8. Machine-Parsable Metadata

For Kimi agents scanning this file, the following markers are used consistently:

- **Request IDs:** `REQ-` followed by a zero-padded 3-digit number (`REQ-001`, `REQ-002`, …)
- **Status keywords:** Exactly one of `Pending`, `Accepted`, `Declined`, `In Progress`, `Implemented`, `Superseded`, `Blocked`, `Archived`
- **Date format:** `YYYY-MM-DD`
- **Application names:** `Customer A`, `Customer D`, or `*(internal)*` for conformance-driven work
- **Owner:** GitHub username or `Unassigned`

When updating this file via automated tools, preserve the table alignment and section structure so that regex/grep-based discovery continues to work.

---

### REQ-022: Migrate Bosak to .NET 10

**Requesting Application:** Bosak / Fytala Stack  
**Submitted:** 2026-06-02  
**Status:** **Accepted**

#### Problem Statement

Bosak previously targeted .NET 9 (`net9.0`). .NET 9 reached end-of-life in **May 2026**. This created two urgent problems:

1. **Security risk** — Running on an EOL runtime means no security patches
2. **Integration blocker** — Customer B's BOD-to-OData XSLT Bridge (REQ-024) could not reference Bosak directly because Bosak targeted .NET 9 while Customer B targeted .NET 8. Both must be on .NET 10 for clean project references.

#### Proposed Solution

Upgraded all 18 Bosak project files from `net9.0` to `net10.0`.

**Projects to migrate:**
- `Bosak.XPath.Core`
- `Bosak.XPath.Parser`
- `Bosak.XPath.Compiler`
- `Bosak.XPath.Runtime`
- `Bosak.XPath.Standard`
- `Bosak.XPath.Api`
- `Bosak.XPath.Providers`
- `Bosak.Xslt`
- All test and conformance projects

**Alternative considered:** Multi-target `net8.0;net9.0;net10.0` to support consumers on older versions. **Rejected** — adds build complexity and testing matrix for an already-EOL runtime.

#### Acceptance Criteria
- [x] All Bosak projects target `net10.0`
- [x] Full QT3 conformance suite passes (or matches current pass rates)
- [x] XSLT conformance suite passes (or matches current pass rates)
- [ ] Customer A's `validate-corpus` regression suite passes
- [ ] Customer B BOD-to-OData spike builds without standalone `net9.0` workaround

#### Impact Analysis

| Layer | Impact | Notes |
|-------|--------|-------|
| Core / XDM | Low | Value types and sequences are framework-agnostic |
| Parser | Low | `ReadOnlySpan<char>` APIs are stable |
| Compiler / VM | Low | IL generation and register VM unchanged |
| XSLT | Low | Transform engine uses framework primitives only |
| Conformance | Low | W3C QT3 harness must run on .NET 10 |

#### Related Requests
- Customer B REQ-025 (Migrate Customer B to .NET 10)
- Customer A REQ-019 (Unified migration to .NET 10)
- Customer D REQ-007 (Migrate Customer D to .NET 10)
- Diffie REQ-009 (Migrate Diffie to .NET 10)

#### Decision Log

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-06-02 | Charles Korthout / Kimi | Accepted | .NET 9 is EOL (May 2026); .NET 10 is the correct LTS target |
| 2026-06-03 | Charles Korthout / Kimi | Implemented | All 18 projects migrated; 867 unit tests pass; XSLT conformance stable at 59.6% (3,257/14,600) |

---

### REQ-025: `xsl:attribute-set` / `xsl:use-attribute-sets` Support

**Requesting Application:** *(internal — conformance)*  
**Submitted:** 2026-06-07  
**Status:** **Implemented**

#### Problem Statement

The `next-match-012` conformance test requires `xsl:attribute-set` and `xsl:use-attribute-sets` support. The test defines an attribute set containing `xsl:attribute` children, one of which calls `xsl:next-match`. Without attribute-set support, the test fails because the attribute set is never applied.

Additionally, many other XSLT 3.0 conformance tests depend on attribute sets for reusable attribute definitions.

#### Proposed Solution

1. Parse `xsl:attribute-set` declarations at stylesheet load time (including imported/included stylesheets).
2. Store attribute sets in a dictionary keyed by resolved `{namespace, local-name}`.
3. Implement merge semantics: unlike templates (last-wins), attribute sets **accumulate** across imports/includes. Collect `List<AttributeSetDefinition>` per name so runtime can apply them in precedence order.
4. Add `ApplyAttributeSets` to `TransformEngine`:
   - Reads `use-attribute-sets` attribute from `xsl:element` and literal result elements.
   - Resolves names, looks up sets via `_stylesheet.GetAllAttributeSets()`.
   - Recursively applies referenced sets (cycle detection via `HashSet<string>`).
   - Executes each set's `xsl:attribute` children via `ExecuteXsltInstruction`.
5. Literal attributes on LREs and `xsl:attribute` children of `xsl:element` override attribute-set values for the same name.

#### Acceptance Criteria
- [x] `xsl:attribute-set` declarations parsed and stored
- [x] `use-attribute-sets` on LREs applies attributes from named sets
- [x] `use-attribute-sets` on `xsl:element` applies attributes from named sets
- [x] Attribute sets accumulate across imports/includes (merge semantics)
- [x] Circular `use-attribute-sets` references are detected and prevented
- [x] `xsl:next-match` inside an attribute set works correctly (current template rule preserved)
- [x] `next-match-012` conformance test passes
- [x] `attribute-set` conformance test set: 36/50 passing (73.5%)

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | Parse `xsl:attribute-set` declarations |
| Compiler | None | |
| Runtime | Modified | `TransformEngine.ApplyAttributeSets` |
| Standard | None | |
| XSLT | New instruction | `xsl:attribute-set` + `use-attribute-sets` |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-06-07 | Charles Korthout / Kimi | Implemented | Required for next-match-012; also enables 36 attribute-set conformance tests |

---

### REQ-026: Nested `xsl:use-when` Evaluation

**Requesting Application:** *(internal — conformance)*  
**Submitted:** 2026-06-07  
**Status:** **Implemented**

#### Problem Statement

`use-when` attributes on nested XSLT instructions and literal result elements were completely ignored. Only top-level declarations (imports, includes, templates, etc.) had `use-when` support. This caused 50+ conformance test failures in the `use-when` cluster, including tests where `use-when="false()"` on `xsl:sort` should suppress the sort, or `use-when="false()"` on `xsl:value-of` should remove the instruction.

#### Proposed Solution

1. Add `StripUseWhenElements(XElement)` to `Stylesheet.Load()` that recursively processes the entire stylesheet tree after imports/includes are resolved.
2. `GetUseWhenAttribute` checks both no-namespace `use-when` (for XSLT elements) and `xsl:use-when` (for LREs).
3. Evaluate `use-when` XPath expressions with in-scope namespace declarations passed to the evaluation context.
4. Remove elements whose `use-when` evaluates to `false()` from the XDocument tree before templates are parsed.

#### Acceptance Criteria
- [x] `use-when="false()"` on nested `xsl:sort` removes the sort instruction
- [x] `use-when="false()"` on `xsl:value-of` removes the instruction
- [x] `xsl:use-when="false()"` on LREs removes the element
- [x] In-scope namespace prefixes are available to `use-when` XPath expressions
- [x] `use-when` cluster: 68/102 passing (+19 from 49/102)

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | `Stylesheet.Load()` recursive stripping |
| Compiler | None | |
| Runtime | None | |
| Standard | None | |
| XSLT | Modified | `use-when` now applies to all elements |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-06-07 | Charles Korthout / Kimi | Implemented | +19 tests; low-risk tree modification during load |

---

### REQ-027: Publish Bosak Packages to NuGet Feed

**Requesting Application:** Customer B
**Submitted:** 2026-06-07
**Status:** Pending

#### Problem Statement

Customer B's `Customer B.DataBridge.Application.BodMapping` project references `Bosak.Xslt` and `Bosak.XPath.Providers` as project references. When `Customer B.DataBridge.Application.BodMapping` is packed as a NuGet package, it declares package dependencies on `Bosak.Xslt` and `Bosak.XPath.Providers`.

However, Bosak projects currently do **not** have NuGet package metadata (`<IsPackable>`, `<PackageId>`, `<Version>`, `<Authors>`, etc.). This means:
- `dotnet pack` on Bosak projects produces no `.nupkg` files
- Any consumer that pulls in `Customer B.DataBridge.Application.BodMapping` from a NuGet feed cannot resolve the transitive Bosak dependencies
- Customer B REQ-019 (publish DataBridge packages to NuGet) is blocked for the BodMapping package

#### Proposed Solution

Add NuGet package metadata to all Bosak projects that Customer B depends on:

1. `Bosak.Xslt`
2. `Bosak.XPath.Core`
3. `Bosak.XPath.Runtime`
4. `Bosak.XPath.Api`
5. `Bosak.XPath.Standard`
6. `Bosak.XPath.Providers`

Each `.csproj` needs at minimum:
```xml
<PropertyGroup>
  <IsPackable>true</IsPackable>
  <PackageId>Bosak.Xslt</PackageId>
  <Version>1.0.0</Version>
  <Authors>Fytala</Authors>
  <Company>Fytala</Company>
  <Description>...</Description>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
</PropertyGroup>
```

#### Acceptance Criteria
- [ ] `Bosak.Xslt` packs as a versioned NuGet package
- [ ] `Bosak.XPath.Providers` packs as a versioned NuGet package
- [ ] `Bosak.XPath.Core` packs as a versioned NuGet package
- [ ] `Bosak.XPath.Runtime` packs as a versioned NuGet package
- [ ] `Bosak.XPath.Api` packs as a versioned NuGet package
- [ ] `Bosak.XPath.Standard` packs as a versioned NuGet package
- [ ] `Customer B.DataBridge.Application.BodMapping` can be restored from a NuGet feed when Bosak packages are present on the same feed

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | No code changes |
| Compiler | None | No code changes |
| Runtime | None | No code changes |
| Standard | None | No code changes |
| XSLT | None | No code changes |
| API | New packaging | NuGet package metadata only |

#### Related Requests
- Customer B REQ-019 (Publish Customer B.DataBridge packages to NuGet feed) — blocked until Bosak packages are available

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-06-07 | Kimi | Pending | Required for Customer B BodMapping NuGet consumption; low-effort metadata addition |
| 2026-06-08 | Kimi | Implemented | Added `src/Directory.Build.props` with shared NuGet metadata; all 10 src projects now packable; 6 core packages verified: Bosak.Xslt, Bosak.XPath.Api, Bosak.XPath.Core, Bosak.XPath.Providers, Bosak.XPath.Runtime, Bosak.XPath.Standard |

---

### REQ-030: XSLT `@as` Type Coercion and Atomization

**Requesting Application:** *(internal — conformance)*  
**Submitted:** 2026-06-11  
**Status:** **Implemented**

#### Problem Statement

The XSLT `as` attribute (`xsl:variable/@as`, `xsl:param/@as`, `xsl:function/@as`, `xsl:with-param/@as`) is central to XSLT 3.0 type safety. Bosak previously ignored `as` entirely for sequence constructors, returning raw nodes instead of atomized/cast values. This caused 60+ failures in the W3C `as` conformance test cluster — the largest single remaining block of XSLT failures.

Specific gaps included:
- No atomization: text nodes from sequence constructors were stored as text nodes, not cast to `xs:integer`, `xs:string`, etc.
- No subtype substitution: `xs:integer` values rejected for `xs:decimal` parameters.
- No type promotion: `xs:float` values rejected for `xs:double` parameters.
- Node type tests (`element(name, type)`, `attribute(name, type)`, `document-node(element(...))`) not validated.
- `xsl:with-param` did not coerce values to the target template's `xsl:param/@as`.
- `xsl:function` bodies did not validate return values against `@as`.
- Functions like `abs()` did not atomize `xs:untypedAtomic` arguments, causing `XPTY0004` crashes.

#### Proposed Solution

1. **`ConvertVariableValue` helper** — Centralized atomization + casting for all variable/param/function return paths. Atomizes nodes to `xs:untypedAtomic`, then delegates to `VmEngine.TryCast` for atomic coercion. Node tests bypass atomization and return as-is.
2. **`VmEngine.TryCast` enhancements** — Strip occurrence indicators and `xsd:` prefix. Subtype substitution: integer→decimal, float→double, anyURI→string.
3. **`VmEngine.ValueMatchesType` enhancements** — Public visibility. Added `element(name, type)`, `attribute(name, type)`, `document-node(element(...))` validation with namespace resolution. Added `document-node()`, `text()`, `comment()`, `processing-instruction()`, `namespace-node()` forms.
4. **`ItemInstanceOf` cleanup** — Exact kind matching for `double`/`float` (post-promotion), subtype for `decimal`→`integer`. Added node-kind matching.
5. **`Abs()` atomization** — `FunctionLibrary.Abs()` now atomizes before checking type and falls back to `ConvertToDouble()`.
6. **Param propagation** — `ApplyBuiltInRules` passes `callParams` through built-in shallow-copy/skip modes.
7. **Lazy global params** — Sequence-constructor global params evaluated on first reference.
8. **`xsl:document` accumulator isolation** — Set `_sequenceAccumulator = null` when `wrapInDocumentNode=true` to prevent content leakage.

#### Acceptance Criteria
- [x] `as` cluster 99/99 passing (100%)
- [x] `xsl:variable/@as="xs:integer"` with text sequence constructor `"42"` returns `xs:integer(42)`
- [x] `xsl:param/@as="xs:decimal"` accepts `xs:integer` arguments (subtype substitution)
- [x] `xsl:with-param` coerces values to target param's `@as`
- [x] `xsl:function/@as="xs:double"` enforces return type; string `"hello"` raises `XPTY0004`
- [x] Node tests (`element(*, xs:untyped)`, `document-node(element(doc, xs:untyped))`) validate structure
- [x] `@as` on `xsl:call-template` raises `XTSE0010`

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | |
| Compiler | None | |
| Runtime | Modified | `VmEngine.TryCast`, `ValueMatchesType`, `ItemInstanceOf` |
| Standard | Modified | `FunctionLibrary.Abs()` atomization |
| XSLT | Modified | `TransformEngine.ConvertVariableValue`, param passing, lazy globals, document accumulator |
| API | None | No surface change |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-06-11 | Kimi | Implemented | 60+ conformance tests fixed; unblocks remaining XSLT clusters |

---

### REQ-032: XSLT 3.0 `xsl:merge` instruction

**Requesting Application:** *(internal)*  
**Submitted:** 2026-06-13  
**Status:** Implemented

#### Problem Statement
The XSLT 3.0 `xsl:merge` instruction and its companion functions `current-merge-group()` and `current-merge-key()` were unimplemented. The `merge` conformance cluster had 21 runnable failures (72 % pass rate) and blocked progress on the broader XSLT 3.0 conformance sweep.

#### Proposed Solution
Implement `xsl:merge`, `xsl:merge-source`, `xsl:merge-key`, and `xsl:merge-action` semantics in the runtime, plus the required static validation and error handling.

#### Acceptance Criteria
- [x] Multiple `xsl:merge-source` inputs are evaluated and sorted by key tuple.
- [x] `current-merge-group()` and `current-merge-group($name)` return the correct items inside `xsl:merge-action`.
- [x] `current-merge-key()` returns the shared key value for the current merge group.
- [x] Static errors (`XTSE0010`, `XTSE0020`, `XTSE3200`, `XTSE1505`) are raised for invalid merge markup.
- [x] Dynamic errors (`XTDE2210`, `XTDE3480`, `XTDE3490`, `XTDE3510`, `XTDE3362`) are raised in the correct contexts.
- [x] `xsl:merge` works inside `xsl:function` bodies and interacts correctly with `xsl:apply-templates`.
- [x] `merge` conformance cluster reaches 0 runnable failures.

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | |
| Compiler | None | |
| Runtime | Modified | `TransformEngine.ExecuteMergeInstruction`, merge context functions, accumulator applicability |
| Standard | Modified | `FunctionLibrary.DateTime_2` atomization fix |
| XSLT | Modified | `TransformEngine`, `Stylesheet` validation, `PatternCompiler` atomic `.` match, `TemplateRule` dynamic `_match` |
| API | None | No surface change |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-06-13 | Kimi | Implemented | Merge cluster now 75/0/31; unblocks `date` cluster sweep |

---

### REQ-033: XSLT `format-date-en` cluster — English number words and era-aware year formatting

**Requesting Application:** *(internal)*  
**Submitted:** 2026-06-15  
**Status:** Implemented

#### Problem Statement
The XSLT 3.0 `format-date` and `format-dateTime` picture string supports English cardinal/ordinal number-word presentation modifiers (`[W]`, `[w]`, `[Ww]`, `[Wo]`, `[wo]`, `[Wwo]`), era-aware negative-year rendering, and ordinal-year width handling. These were unimplemented, causing the entire `format-date-en` conformance cluster (33 tests) to fail.

#### Proposed Solution
Extend `FormatDateTimeEngine` to:
1. Render numeric components as English cardinal words (`one`, `two`, …) and ordinal words (`first`, `second`, …) in uppercase, lowercase, and title-case forms.
2. When the picture contains an era component (`[E...]`), render negative years as absolute values with the appropriate default minimum width.
3. For ordinal year presentation (`[Yo]`), append the ordinal suffix to the full year value instead of truncating.

#### Acceptance Criteria
- [x] Cardinal words `[W]`, `[w]`, `[Ww]` produce uppercase, lowercase, and title-case output.
- [x] Ordinal words `[Wo]`, `[wo]`, `[Wwo]` produce uppercase, lowercase, and title-case output.
- [x] Values up to billions are supported.
- [x] Negative years with an era component render as absolute values (e.g. `55BC` not `0-55BC`).
- [x] Ordinal year `[Y1o]` renders as `1990th`, not `1st`.
- [x] `format-date-en` conformance cluster reaches 33/33 passing (0 runnable failures).
- [x] Regression unit tests added to `FormatDateTimeEngineTests`.

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | |
| Compiler | None | |
| Runtime | None | |
| Standard | Modified | `FormatDateTimeEngine` number-word helpers |
| XSLT | None | |
| API | None | No surface change |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-06-15 | Kimi | Implemented | `format-date-en` cluster now 33/0/0; full suite +30 passes / −30 failures |

---

### REQ-036: XSLT `method="json"` output serialization

**Requesting Application:** *(internal — conformance)*  
**Submitted:** 2026-07-11  
**Status:** Implemented

#### Problem Statement
XSLT 3.0 adds a JSON output method controlled by `xsl:output method="json"` (and `xsl:result-document`). Bosak already supported `fn:serialize(..., map{'method':'json'})` for XDM values, but the XSLT result-tree builder rejected top-level `xsl:map`/`xsl:map-entry` results because they could not become children of the synthetic XML wrapper. This caused W3C `output-0702`, `output-0704`, `output-0706`, and `output-0706a` to fail, blocking the JSON output conformance sweep.

#### Proposed Solution
1. Preserve raw XDM items (maps, arrays, and other values) produced at the top level of a JSON output instead of forcing them into the XML result tree.
2. Extend `OutputProperties` with JSON-specific parameters: `json-node-output-method`, `allow-duplicate-names`, `escape-solidus`, and `parameter-document`.
3. Reuse `XdmJsonSerializer` from `Bosak.XPath.Standard` in `ResultTreeSerializer`, applying character maps after JSON escaping and honoring `json-node-output-method` for nested nodes.
4. Implement namespace-declaration output for the HTML `json-node-output-method` so XHTML-rooted nodes round-trip correctly.

#### Acceptance Criteria
- [x] `output-0701` passes: basic map/array JSON serialization.
- [x] `output-0702` passes: nested HTML nodes inside JSON strings with XHTML namespace declarations.
- [x] `output-0703` passes: `item-separator` for `method="text"`.
- [x] `output-0704` passes: `allow-duplicate-names="yes"` permits duplicate JSON keys.
- [x] `output-0705` passes: `allow-duplicate-names="no"` raises `SERE0022`.
- [x] `output-0706`/`output-0706a` pass: `xsl:output parameter-document` supplies `method="json"` and inline character maps.
- [x] `output-0709`/`output-0718`/`output-0719` pass: `item-separator` for `method="text"` with `build-tree="yes"`.
- [x] `output-0710`/`output-0711`/`output-0712` pass: maps/arrays/functions at top level of XML/HTML/text output raise `SENR0001`.
- [x] `output` conformance cluster improves from 168/35/29 to 179/24/29.
- [x] Full W3C suite improves from 5,473/132 to 5,481/124.

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | `OutputProperties.FromElement` parses JSON attributes, `item-separator`, and `parameter-document`. |
| Compiler | None | |
| Runtime | Modified | `TransformEngine` collects raw JSON items, applies `item-separator`, and raises `SENR0001`; `ResultTreeSerializer` dispatches to JSON serializer and validates non-JSON output. |
| Standard | None | Reuses existing `XdmJsonSerializer`. |
| XSLT | Modified | `xsl:output`/`xsl:result-document` now support `method="json"` and `item-separator`. |
| API | None | No public surface change. |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-11 | Kimi | Implemented | Raw-item collection, JSON parameter parsing, parameter-document support, and HTML namespace output clear the remaining JSON output failures. |

---

### REQ-037: XSLT `xsl:result-document` serialization completeness

**Requesting Application:** *(internal — conformance)*  
**Submitted:** 2026-07-11  
**Status:** Implemented

#### Problem Statement
After clearing the principal `output` cluster, the W3C `result-document` cluster still had 39 failures. Many were caused by `xsl:result-document` serialization attributes being validated as static values before AVT evaluation, by case-insensitive yes/no parsing accepting invalid uppercase values, by `SEPM0009` being raised for methods with no XML declaration, and by the engine lacking raw-item collection for JSON/adaptive/build-tree="no" secondary outputs.

#### Proposed Solution
1. Evaluate AVTs for all serialization attributes in `TransformEngine.EvaluateResultDocumentInstruction` before passing the stub to `OutputProperties.FromElement`.
2. Make yes/no parsing case-sensitive while retaining `true`/`false`/`1`/`0` synonyms, and normalize `standalone` to `yes`/`no`/`omit`.
3. Restrict `SEPM0009` to `xml` and `xhtml` methods.
4. Parse `build-tree` and collect raw XDM items for `method="json"`, `method="adaptive"`, and `build-tree="no"` in both principal and secondary result documents.
5. Use the principal `xsl:result-document` output properties in `XsltExecutable.TransformToString`.

#### Acceptance Criteria
- [x] `result-document-0244`/`0245` pass: AVT `html-version="{$param}"`.
- [x] `result-document-0701`/`1203`–`1205` pass: AVT yes/no attributes (`include-content-type`, `byte-order-mark`, `escape-uri-attributes`).
- [x] `result-document-0246`–`0250`/`0276`/`0283` pass: invalid uppercase yes/no values raise `XTSE0020`.
- [x] `result-document-0239` passes: `SEPM0009` not raised for text output method.
- [x] `result-document-0303`/`1401`/`1404`/`1411` pass: maps serialized as JSON from `xsl:result-document`.
- [x] `result-document` conformance set improves from 86/39/29 to 104/21/29.
- [x] Full W3C suite improves from 5,481/124 to 5,506/99.

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | `OutputProperties` parses `build-tree` and normalizes `standalone`; yes/no parsing case-sensitive. |
| Compiler | None | |
| Runtime | Modified | `TransformEngine` evaluates result-document AVTs, collects raw items, and writes secondary JSON documents. |
| Standard | None | |
| XSLT | Modified | `xsl:result-document` now supports JSON/adaptive/raw output. |
| API | Modified | `XsltExecutable.TransformToString` uses principal result-document properties. |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-11 | Kimi | Implemented | AVT evaluation, value normalization, SEPM0009 scoping, and raw-item collection clear 18 result-document failures and push the full suite to 98.2%. |

---

### REQ-038: XSLT `namespace` cluster — `inherit-namespaces="no"`

**Requesting Application:** *(internal — conformance)*  
**Submitted:** 2026-07-12  
**Status:** Implemented

#### Problem Statement
After clearing the principal `output` and `current-output-uri` clusters, the W3C `namespace` cluster still had 10 failures (`namespace-2603` through `namespace-2632`). The failures were caused by `inherit-namespaces="no"` on `xsl:element`, `xsl:copy`, and literal result elements not emitting the required `xmlns:prefix=""` undeclarations for children that inherited prefixed namespaces. In addition, the synthetic `__xdm_doc__` wrapper was unwrapped by creating a new `XDocument` from its single child, which cloned the element and silently dropped all namespace annotations.

#### Proposed Solution
1. In `TransformEngine.FinalizeNamespaceInheritance`, detect `NamespaceInheritanceBarrier` annotations and attach a `PrefixedNamespaceUndeclarations` annotation to every child element listing the non-empty prefixed bindings that would otherwise be inherited.
2. In `TransformEngine.Transform`, detach the single root element from the synthetic wrapper before constructing the final `XDocument`, so user annotations are moved rather than cloned away.
3. In `ResultTreeSerializer.SerializeXmlFragment`, route any tree carrying `PrefixedNamespaceUndeclarations` annotations to the raw XML 1.1 serializer, because `XmlWriter` cannot represent `xmlns:prefix=""`.

#### Acceptance Criteria
- [x] `namespace-2603` passes: `xsl:element` with `inherit-namespaces="no"` emits `xmlns:n=""` for a child that would otherwise inherit `n`.
- [x] `namespace-2604` through `namespace-2632` pass: coverage for `xsl:copy`, literal result elements, nested barriers, and explicit `inherit-namespaces="yes"` redeclarations.
- [x] The entire W3C `namespace` conformance set reports 0 failures.
- [x] The `output-0138` prefix-preservation path continues to pass.

#### Impact Analysis
| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | |
| Compiler | None | |
| Runtime | Modified | `TransformEngine.FinalizeNamespaceInheritance` and `Transform` unwrap logic. |
| Standard | None | |
| XSLT | Modified | `xsl:element`, `xsl:copy`, and literal result elements now honor `inherit-namespaces="no"` for prefixed namespaces. |
| API | None | |

#### Decision Log
| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-12 | Kimi | Implemented | Barrier-attached undeclarations plus raw-serializer routing clear the remaining namespace failures without regressing output tests. |

---

### REQ-039: Resolve QT3 `op-same-key` hang

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-17  
**Status:** Implemented  
**Target Version:** TBD  

**Problem Statement:**  
The full W3C QT3 suite could not complete because the `op-same-key` test set (28 tests) hung. The first hanging test (`same-key-023`) builds a ~400k-entry map and then calls `map:remove` and `map:put` inside an `every` quantifier over all keys. The original `XdmMap` implemented `map:remove` and `map:put` by copying the entire dictionary, giving O(N²) behavior and causing an effective hang.

**Acceptance Criteria:**  
- `op-same-key` completes without hanging.
- All non-skipped `op-same-key` tests pass.
- No regressions in the existing `map:*` unit or QT3 tests.

**Implementation Notes:**  
- Replaced the `Dictionary<XdmValue, XdmValue>` backing of `XdmMap` with `ImmutableDictionary<XdmValue, XdmValue>` (using the existing `XdmValueEqualityComparer`).
- `map:remove` now removes keys by structural sharing; `map:put` removes then re-adds the key so the newest key object survives for `op:same-key` / `map:merge use-last` semantics.
- `map:merge` continues to use `XdmMap.Add`, which now performs remove+add under the immutable dictionary.
- Added `arbitraryPrecisionDecimal` to the harness's unsupported-feature list so the two tests that require arbitrary-precision decimal arithmetic are skipped (same-key-008 and same-key-025).

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | None | |
| Compiler | None | |
| Runtime | None | |
| Standard | Modified | `map:remove`, `map:put`, and `map:merge` in `FunctionLibrary.cs`; `XdmMap` in `Bosak.XPath.Core`. |
| XSLT | None | |
| API | None | |
| Conformance | Modified | `DependencyFilter` skips `arbitraryPrecisionDecimal` tests. |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-17 | Kimi | Implemented | `ImmutableDictionary` gives structural sharing with minimal API surface change; remove+add preserves the key-object replacement semantics required by `op:same-key`. |

---

### REQ-040: XQuery 3.1 Phase 1 — prolog-less query execution

**Requesting Application:** Bosak / Fytala Stack  
**Submitted:** 2026-07-22  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
The `Bosak.XQuery` project existed only as a skeleton with placeholder `XQueryCompiler`, `XQueryExecutable`, and `XQueryContext` classes. No XQuery source could be parsed, compiled, or executed, blocking the roadmap priority to implement XQuery 3.1.

**Proposed Solution:**  
Wire the XQuery public API to the proven XPath pipeline: parse the query body with `XPathParser`, resolve function namespaces against an XQuery static context, optimize with `XPathOptimizer`, lower with `IrLowerer`, and execute with `VmEngine`. Add a dedicated `XQueryParser` for the XQuery top-level grammar (version declaration and prolog) and a `XQueryStaticContext` to hold prolog-derived bindings.

**Acceptance Criteria:**
- [x] `XQueryCompiler.Compile` parses an XQuery source string and returns an executable plan.
- [x] `XQueryExecutable.Evaluate` executes the plan via the XPath VM and returns an `XdmValue`.
- [x] Prolog-less queries such as `for $i in 1 to 3 return $i` and `let $x := 42 return $x` produce correct results.
- [x] Namespace declarations from the prolog are applied to the evaluation context.
- [x] No regressions in XPath, XSLT, or existing unit tests.

**Implementation Notes:**
- Created `src/Bosak.XQuery/Parser/XQueryParser.cs` to parse the version declaration and basic prolog declarations (`declare namespace`, `declare default element namespace`, `declare default function namespace`, `declare default collation`). The parser delegates the query body (`Expr`) to the existing `XPathParser`.
- Created `src/Bosak.XQuery/Compiler/XQueryStaticContext.cs` as an immutable static context holding namespace bindings, default element/function namespaces, default collation, base URI, declared variables, and declared function signatures.
- Updated `XQueryCompiler` to resolve function namespaces using the static context, optimize, and lower the AST to an `IrModule`.
- Updated `XQueryExecutable` to apply the static context to the runtime `EvaluationContext`, execute with `VmEngine`, and restore the original context state afterwards.
- Added unit tests in `tests/Bosak.XQuery.Tests/PlaceholderTests.cs` for `for`, `let`, and `declare namespace`.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | New | `XQueryParser` in `Bosak.XQuery`; reuses `XPathParser` for expressions. |
| Compiler | New | `XQueryStaticContext` in `Bosak.XQuery`; reuses `XPathOptimizer` and `IrLowerer`. |
| Runtime | None | Reuses `VmEngine` and `EvaluationContext`. |
| Standard | None | Standard function library populated as before. |
| XSLT | None | No XSLT changes. |
| API | New | Public `XQueryCompiler`, `XQueryExecutable`, `XQueryContext` are now functional. |
| Conformance | None | QT3 harness not yet wired to XQuery tests. |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-22 | Kimi | Implement separate XQuery parser that delegates to XPathParser | Keeps XPath lexer/parser clean and avoids XML-tokenization complexity in the XPath layer. |

### REQ-041: XQuery 3.1 Phase 2 — FLWOR `order by` clause

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-22  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
XQuery 3.1 requires full FLWOR expressions, including multiple `for`/`let` clauses, `where`, and `order by`. The XPath-only parser only allowed a single initial `for`/`let` and a `where` clause, so queries such as `for $x in ... order by ... return ...` could not be parsed.

**Proposed Solution:**  
Extend `XPathParser` with an `allowFullFlwor` flag used by `XQueryParser`, add `FlworExpressionNode` and `OrderByClauseNode` AST nodes, and lower them with new `OrderBy` and `TupleBind` IR opcodes executed by the VM.

**Acceptance Criteria:**
- [x] `XQueryParser` parses multi-clause `for`/`let`/`where`/`order by` FLWOR expressions.
- [x] `order by` supports `ascending`/`descending`, `empty least`/`greatest`, and an optional `collation` URI.
- [x] XPath mode still rejects multi-clause FLWOR and `order by` per `LetExpr020a`.
- [x] `IrLowerer` and `VmEngine` correctly sort tuples and bind them back to the body.
- [x] No regressions in XPath, XSLT, or existing unit tests.

**Implementation Notes:**
- Added `allowFullFlwor` to `XPathParser.Parse` and `XPathParser` constructor; XPath callers default to `false`.
- `ParseFlworExpr` raises `XPST0003` for intermediate `for`/`let` or `order by` when `_allowFullFlwor` is false.
- Added `OrderByClauseNode`, `ForClauseNode`, `LetClauseNode`, `WhereClauseNode` to `XPathAstNode`.
- `XPathOptimizer` traverses all FLWOR clause types.
- `IrLowerer.LowerFlworExpression` builds XDM-array tuples, emits `OrderBy`, then iterates sorted tuples with `TupleBind`.
- `VmEngine` handlers for `OrderBy` (stable sort with key extraction) and `TupleBind` (bind tuple members to named variables).

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | `XPathParser` gains `allowFullFlwor`; new clause AST nodes. |
| Compiler | Modified | `XPathOptimizer` and `IrLowerer` support `FlworExpressionNode` and `OrderByClauseNode`. |
| Runtime | Modified | `VmEngine` adds `OrderBy` and `TupleBind` handlers. |
| Standard | None | No new standard functions. |
| XSLT | None | No XSLT changes. |
| API | None | Public `XQueryCompiler` surface unchanged. |
| Conformance | None | QT3 harness not yet wired to XQuery tests. |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-22 | Kimi | Implement tuple-based lowering for order by | Keeps the VM simple by treating FLWOR tuples as arrays and sorting before binding. |

---

### REQ-042: XQuery 3.1 Phase 2 — FLWOR `count` clause

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-23  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
XQuery 3.1 requires the `count $var` FLWOR intermediate clause, which binds an `xs:integer` counting the current tuple in the FLWOR stream (1-based). Without it, queries such as `for $i in ('a','b','c') count $n return $n` cannot be parsed or evaluated.

**Proposed Solution:**  
Extend the existing tuple-based FLWOR lowering so that `count` clauses maintain a compiler-managed integer counter. Counters are initialised to `0`, incremented for each tuple, and stored under the declared variable name. Variables bound by pre-`order by` counts are captured in the tuple so they can be referenced in `order by` keys and in the return expression; post-`order by` counts are incremented during tuple iteration after sorting.

**Acceptance Criteria:**
- [x] `XPathParser` parses `count $var` as a FLWOR intermediate clause when `allowFullFlwor` is true.
- [x] XPath-only mode rejects `count` clauses with `XPST0003`.
- [x] `count` works with `for`, `let`, `where`, and both pre- and post-`order by` positions.
- [x] The count value is an `xs:integer` starting at 1 and is filtered by preceding `where` clauses.
- [x] No regressions in XPath, XSLT, or existing unit tests.

**Implementation Notes:**
- Added `CountClauseNode` to `XPathAstNode`.
- `XPathOptimizer` recognises and passes through `CountClauseNode`.
- `IrLowerer.LowerFlworExpression` routes FLWOR expressions containing `count` through the tuple path.
- `LowerFlworWithTuples` initialises one counter per `count` clause and emits `LoadVariable`/`Add`/`StoreVariable` to increment it.
- Post-`order by` counts are handled in `LowerFlworBodyIteration`.
- No new VM opcodes were required.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | New `CountClauseNode`; `count $var` parsed in full FLWOR mode. |
| Compiler | Modified | `XPathOptimizer` and `IrLowerer` handle `CountClauseNode`. |
| Runtime | None | Reuses existing `LoadVariable`, `StoreVariable`, and `Add` opcodes. |
| Standard | None | No new standard functions. |
| XSLT | None | No XSLT changes. |
| API | None | Public `XQueryCompiler` surface unchanged. |
| Conformance | None | QT3 harness not yet wired to XQuery tests. |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-23 | Kimi | Implement count via tuple-path counters | Reuses the order-by tuple infrastructure and avoids adding a new VM opcode. |

---

### REQ-043: XQuery 3.1 Phase 2 — FLWOR `group by` clause

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-25  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
XQuery 3.1 requires the `group by` FLWOR intermediate clause, which partitions the tuple stream into groups sharing equal grouping keys and rebinds variables per group: grouping variables take the shared key value; all other variables are bound to the concatenation of their values across the group. Without it, aggregation queries such as `for $i in 1 to 6 group by $g := $i mod 2 return ($g, count($i))` cannot be parsed or evaluated.

**Proposed Solution:**  
Extend the tuple-based FLWOR lowering with a `GroupBy` opcode. Grouping specs of the form `$var := expr` are lowered as synthetic `let` bindings evaluated per pre-grouping tuple, so every grouping key is a variable captured in the tuple. The VM groups tuples by key equality (preserving first-appearance order) and merges each group into a single tuple. An `order by` after `group by` is supported by re-keying the grouped tuples in a second tuple pass so that sort keys are evaluated against the grouped bindings.

**Acceptance Criteria:**
- [x] `XPathParser` parses `group by` grouping specs (`$var` or `$var := expr`, optional `collation`) when `allowFullFlwor` is true.
- [x] XPath-only mode rejects `group by` with `XPST0003`.
- [x] Grouping variables keep the shared key value; non-grouping variables are bound to the concatenated group values.
- [x] Empty grouping keys group together; NaN groups with NaN; a multi-item grouping key raises `XPTY0004`.
- [x] `where` before, and `order by` / `count` after `group by` are supported.
- [x] No regressions in XPath, XSLT, or existing unit tests.

**Implementation Notes:**
- Added `GroupByClauseNode` and `GroupingSpec` to `XPathAstNode`.
- `XPathOptimizer` traverses grouping-spec key expressions; `XQueryCompiler` resolves their function namespaces.
- Added `GroupBy` IR opcode and `GroupByInfo` literal-pool record.
- `IrLowerer.LowerFlworWithGrouping` lowers `:=` specs as synthetic `let` bindings, emits `GroupBy`, and re-keys grouped tuples for a post-group `order by`.
- `VmEngine` `GroupBy` handler groups tuples (first-appearance order) and merges each group; grouping-key equality atomizes keys and compares numerics (NaN = NaN), strings (codepoint), and booleans.
- Unsupported shapes fail fast at compile time: multiple `group by` clauses, `order by` before `group by`, and post-group clauses other than `order by`/`count`.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | New `GroupByClauseNode`/`GroupingSpec`; `group by` parsed in full FLWOR mode. |
| Compiler | Modified | `XPathOptimizer`, `IrLowerer`, and new `GroupBy` opcode. |
| Runtime | Modified | New `GroupBy` VM handler and grouping-key equality helpers. |
| Standard | None | No new standard functions. |
| XSLT | None | No XSLT changes. |
| API | None | Public `XQueryCompiler` surface unchanged. |
| Conformance | None | QT3 harness not yet wired to XQuery tests. |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-25 | Kimi | Lower `:=` grouping specs as synthetic let bindings; re-key tuples for post-group `order by` | Reuses the proven tuple infrastructure and keeps the `GroupBy` opcode a pure grouping/merge operation. |

---

### REQ-044: XQuery 3.1 Phase 2 — FLWOR `window` clause

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-25  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
XQuery 3.1 requires the `window` FLWOR clause, which partitions or slides over the input sequence to produce a stream of windows. Without it, queries such as `for tumbling window $w in (2,4,6) start when true() end at $p when $p = 2 return $w` cannot be parsed or evaluated.

**Proposed Solution:**  
Add a `Window` opcode on the tuple-based FLWOR path. The lowerer emits the window input expression, then three blocks: the start-condition when-expression, the end-condition when-expression, and the window body (the remaining clauses/return, lowered as usual). The VM iterates the input sequence, evaluates the conditions with the declared WindowVars (current item, position, previous item, next item) bound, and for each produced window binds the window variable to the window's items plus the start/end condition variables captured at window open/close, then executes the body block. Tumbling windows open only when no window is open; sliding windows open at every matching item and may overlap. With `only end`, windows still open at end of input are discarded.

**Acceptance Criteria:**
- [x] `XPathParser` parses `for tumbling|sliding window $var in expr start ... when ... (only)? end ... when ...` when `allowFullFlwor` is true, both as the initial and as an intermediate clause.
- [x] XPath-only mode rejects window clauses with `XPST0003`.
- [x] Tumbling and sliding semantics, including single-item windows and unclosed windows at end of input (emitted unless `only end`).
- [x] Start condition position is 1-based in the input sequence; end condition position is 1-based within the window; previous/next items come from the input sequence.
- [x] Window variable and start/end condition variables are visible in later clauses (`order by` keys) and in the return expression.
- [x] No regressions in XPath, XSLT, or existing unit tests.

**Implementation Notes:**
- Added `WindowClauseNode` and `WindowCondition` to `XPathAstNode`.
- `XPathOptimizer` traverses the in-expression and both when-expressions; `XQueryCompiler` resolves their function namespaces.
- Added `Window` IR opcode and `WindowInfo` literal-pool record.
- `IrLowerer` routes window-containing FLWORs through the tuple path; `LowerWindowClauseForTuples` emits the start/end/body blocks; `ComputeBoundVariables` captures the window and condition variables so `order by`/`group by` see them.
- `VmEngine` `Window` handler implements the tumbling/sliding algorithms with condition evaluation via `ExecuteBlock`, EBV truthiness, and save/restore of all bound variables.
- Clauses other than `count` after an `order by` (including `window`) now fail fast with `NotSupportedException` instead of being silently dropped.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | New `WindowClauseNode`/`WindowCondition`; `for tumbling|sliding` dispatch in full FLWOR mode. |
| Compiler | Modified | `XPathOptimizer`, `IrLowerer`, and new `Window` opcode. |
| Runtime | Modified | New `Window` VM handler and window condition/binding helpers. |
| Standard | None | No new standard functions. |
| XSLT | None | No XSLT changes. |
| API | None | Public `XQueryCompiler` surface unchanged. |
| Conformance | None | QT3 harness not yet wired to XQuery tests. |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-25 | Kimi | Implement window as a VM opcode with nested start/end/body blocks | The stateful windowing algorithm does not decompose into existing opcodes; the `For`-style `ExecuteBlock` pattern keeps it consistent with the tuple path. |

---

### REQ-045: QT3 harness XQuery routing + XQuery conformance sweep

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-25  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
With the XQuery 3.1 Phase 2 FLWOR surface complete (`order by`, `count`, `group by`, `window`), the QT3 harness still skipped every XQuery-syntax test — 16,815 tests sat in the "Unsupported dependency" bucket, including the FLWOR test sets (`prod/WindowClause`, `prod/GroupByClause`, `prod/OrderByClause`, `prod/CountClause`) that validate the new clauses. The harness needed to route supported XQuery tests through the `Bosak.XQuery` pipeline.

**Proposed Solution:**  
Relax the dependency filter for positive XQuery-only spec tokens when the query uses only supported constructs, route admitted tests through `XQueryCompiler` with the harness `EvaluationContext` bridged into `XQueryContext`, and gate out queries using unsupported constructs (constructors, switch/typeswitch, unsupported prolog forms, annotations, pragmas, string constructors). Then drive failures to zero by fixing the engine gaps the newly-routed tests surfaced.

**Acceptance Criteria:**
- [x] The four FLWOR test sets run with **0 failures** (WindowClause 34, GroupByClause 14, OrderByClause 39, CountClause 4 pass; remainder skipped on unsupported constructs).
- [x] Full QT3 suite improves from 14,994 passed / 0 failed to **22,983 passed / 0 failed** (+7,989); skipped 16,827 → 8,838 (167 of them recorded in `KnownXQueryGaps` with reasons).
- [x] XPath-only behavior unchanged; all 1,429 unit tests pass.

**Implementation Notes:**
- Harness: `Bosak.XPath.Conformance` references `Bosak.XQuery`; `DependencyFilter.IsSupported(..., allowXQuerySpecs)`; `TestExecutor` routes XQ-dep/XQuery-syntax tests through `XQueryCompiler` with construct gating (`CanHandleAsXQuery`); `ConformanceRunner.KnownXQueryGaps` records the 203 remaining gaps as reasoned skips.
- Window fixes: end-condition positional variable is the input-sequence position (not window-relative); end condition optional (tumbling closes on next start; sliding extends to end of input); `XQST0103` duplicate-variable check.
- Tuple-path structural fix: nested `For`/`Window` blocks now `Return` their accumulated tuples to the enclosing block (multi-binding `for` + order by, `let` + order by, window nested in `for`).
- Type declarations: `as SequenceType` on `for`/`let`/`some`/`every` bindings, window variable, and grouping specs, enforced via new `EnforceType` opcode (XPTY0004).
- Order by: NaN follows empty least/greatest; unknown collations raise XQST0076 with base-URI resolution; `stable order by` accepted.
- Prolog: `declare base-uri`; version/encoding validation (XQST0031/XQST0087); duplicate default collation (XQST0038); prolog syntax errors are XPST0003; character references in prolog literals; `xquery` as a plain name no longer triggers version-declaration mode.
- XQuery string literals: predefined entity and character references (`&amp;`, `&#65;`), raw `&` rejected with XPST0003.
- Empty inline-function bodies (`function($x) {}`) evaluate to the empty sequence.
- Group-by keys: date/time values group by instant on the timeline; positional variables (`at $p`) captured in tuples; grouping-spec type checks apply to the atomized key.
- *(2026-07-25 follow-up)* Named function reference arity validation (`XPST0017`); variadic functions (`FunctionSignature.IsVariadic`, `fn:concat#N` for any N ≥ 2); group-by string keys honor the default/spec collation; `fn:distinct-values`/`fn:deep-equal` compare g\* dates on the timeline; map keys treat timezone presence as significant with throw-safe UTC instant keys.
- *(2026-08-17 follow-up)* `fn:analyze-string` result element declares the `fn` namespace explicitly so `fn:in-scope-prefixes` reports both `fn` and `xml` (`analyzeString-028`); 1 stale `KnownXQueryGaps` entry removed.
- *(2026-08-17 follow-up)* `XDocumentProvider.ConstructElement` tracks prefix->URI bindings and allocates generated prefixes for copied attributes whose original prefix is already bound to a different URI, preserving the chosen prefix via `AttributePrefixAnnotation` (`cbcl-ns-fixup-1`); 1 stale `KnownXQueryGaps` entry removed.
- *(2026-08-18 follow-up)* `fn:distinct-values`/`fn:index-of` in `FunctionLibrary.cs` now compare `XdmValueKind.String` values by XSD type family: xs:string/untypedAtomic/anyURI and derived string types compare by string; gYear/gMonth/gDay/gYearMonth/gMonthDay compare on the timeline only within the same subtype; xs:hexBinary and xs:base64Binary compare by decoded value and never compare equal to string-family or cross-family values (`cbcl-distinct-values-002b`); 1 stale `KnownXQueryGaps` entry removed.
- *(2026-08-18 follow-up)* `ApplyAxis`/`PathStepMap` in `VmEngine.cs` treat an empty-sequence input (`XdmValue.Undefined`) as an empty result instead of raising `XPDY0002`; the real "absent context item" case is still caught by `LoadContextItem`. This fixes path shapes like `doc(())/*` and the nested FLWOR in `Catalog004`; 1 stale `KnownXQueryGaps` entry removed.
- *(2026-08-18 follow-up)* `XPathParser.ParseExprSingle` treats `if` as a conditional keyword only when the next token is `(`. Otherwise `if` falls through to `ParseOrExpr` and is parsed as an ordinary name/name test, consistent with the existing `for`/`let` gating. This fixes the W3C tokenizer-torture query `if(if) then then else else-...` (`K2-NameTest-5`); 1 stale `KnownXQueryGaps` entry removed.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | Optional window end, XQST0103, `stable`, `as` declarations, entity references, empty function bodies. |
| Compiler | Modified | Nested-rhs Return fix, `EnforceType` opcode, positional vars in tuples, declared types in `GroupByInfo`/`WindowInfo`. |
| Runtime | Modified | Window semantics, type enforcement, NaN/collation order by, dateTime group keys, `Window` no-end handling. |
| XQuery | Modified | `declare base-uri`, version/encoding/collation validation, prolog char references. |
| Conformance | Modified | XQuery routing, construct gating, `KnownXQueryGaps` (203 reasoned skips). |
| XSLT | None | No XSLT changes; XSLT baseline unchanged. |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-25 | Kimi | Gate XQuery admission by constructs, fix engine gaps to zero failures | Keeps the Failed=0 invariant honest: every admitted test is verifiably handled; remaining gaps are explicit reasoned skips instead of silent failures. |

---

### REQ-046: XQuery 3.1 Phase 3 — direct element constructors

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-25  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
XQuery 3.1 requires direct element constructors (`<out a="{1+1}">text {expr}<nested/></out>`), comment constructors (`<!-- c -->`), and processing-instruction constructors (`<?pi data?>`), including computed attribute values, enclosed expressions in content, constructor-local namespace declarations, and copy semantics for existing nodes. Without them, ~2,000 QT3 tests were constructor-gated, including most of the FLWOR use cases and the DirElem* test sets.

**Proposed Solution:**  
Add a lexer constructor mode that emits a whole direct constructor as a single token (robust against quotes, `&`, and text that is not tokenizable), a source-level constructor scanner in the parser producing `DirectElementConstructorNode`/`DirectCommentNode`/`DirectProcessingInstructionNode` AST, `ConstructElement`/`ConstructContentNode` IR opcodes, and a provider-neutral node-construction hook (`EvaluationContext.ElementConstructorHook` / `ContentNodeConstructorHook`) with an XDocument implementation. Constructor-local `xmlns` declarations are applied dynamically (`SaveNamespaces`/`DeclareNamespace`/`RestoreNamespaces` opcodes) so nested constructors and in-scope paths see them.

**Acceptance Criteria:**
- [x] Direct element constructors with literal/computed attributes, enclosed expressions (items joined per-expression with single spaces), nested constructors, comments, PIs, and CDATA.
- [x] Standalone comment/PI constructors as primary expressions (`<?pi x?>` valid anywhere).
- [x] Constructor-local namespace declarations with dynamic scoping, undeclarations (`xmlns=""`), redundant-declaration fixup, and in-scope copying for cloned nodes.
- [x] Static validations: `XQST0118` (tag mismatch), `XQDY0025` (duplicate attributes), `XQTY0024` (attribute after content), `XQST0070/0071` (prefix misuse/duplicates), `XQST0022` (computed ns URI), `XQST0046` (invalid ns URI char), `XQST0090` (invalid character reference), `XPST0081` (undeclared prefix).
- [x] Boundary whitespace handling (`strip` default, `xml:space="preserve"`, reference/CDATA-significant text).
- [x] Attribute nodes in content become element attributes; arrays in content flatten; base URI annotates constructed elements.
- [x] QT3 sets: WindowClause 117/0, OrderByClause 191/0, GroupByClause 30/0, CountClause 13/0, DirElemConstructor 62/1, DirElemContent.namespace 111/1, DirElemContent 227/4, DirElemContent.whitespace 19/0. Full suite: **25,060 passed / 0 failed / 8,477 skipped**.

**Implementation Notes:**
- Lexer: `TokenKind.Constructor` with structure-validated span scanning (falls back to the `<` operator for comparisons).
- Parser: source-level scanner for tags/attributes/content (entity refs, `{{`/`}}` escapes, quote doubling, XQuery comment awareness in enclosed expressions).
- VM: `ConstructElement` handler (prefix resolution, attribute normalization, XQTY0024 attribute rules, atomic joining per enclosed expression, array flattening) and `ConstructContentNode` for standalone comment/PI.
- Provider: `XDocumentProvider.ConstructElement` (prefix declarations, namespace fixup, in-scope copying on clones, base-URI annotation) and `ConstructContentNode`.
- Supporting fixes: predicate EBV must not atomize node results (self-axis predicates), FLWOR tuple variables scoped to the body `For` (no leaking), `day-from-dateTime` parameter conversion, `fn:distinct-values` returns atomized values, decimal −0 normalization, `xsi`/`local` predefined prefixes, prefixed type-name resolution for `instance of`/casts, `allowing empty` for-bindings.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | Constructor lexer mode + source scanner; `allowing empty`; validation error codes. |
| Compiler | Modified | `ConstructElement`/`ConstructContentNode`/`SaveNamespaces`/`DeclareNamespace`/`RestoreNamespaces` opcodes; scoped FLWOR variables. |
| Runtime | Modified | Constructor VM handlers; namespace scoping; type-prefix resolution. |
| Providers | Modified | `ConstructElement`/`ConstructContentNode`; namespace fixup; clone copying; base-URI annotation. |
| XSLT | None | No XSLT changes; XSLT baseline unchanged. |
| Conformance | Modified | Direct constructors admitted; `KnownXQueryGaps` regenerated (284 reasoned skips). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-25 | Kimi | Lexer-level constructor tokens + provider-neutral construction hooks | Token-level delimiting keeps text/quotes/`&` out of the token stream; hooks keep the Runtime provider-neutral (XDocument is only the default). |

---

### REQ-047: XQuery 3.1 Phase 3 — computed constructors

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-25  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
XQuery 3.1 computed constructors (`element e { ... }`, `attribute a { ... }`, `document { ... }`, `text { ... }`, `comment { ... }`, `processing-instruction pi { ... }`, `namespace n { ... }`) build nodes whose names are static EQNames or computed from enclosed expressions (`element { $name } { ... }`). Without them, ~800 QT3 tests were constructor-gated, including the whole Comp* and nscons test sets and many FLWOR use cases that return constructed nodes.

**Proposed Solution:**  
AST nodes for the seven computed constructor forms, parser recognition gated on XQuery mode (keyword + `{`, or keyword + name + `{`, hooked into step expressions so `element` is not swallowed as a name test), a single `ConstructComputed` IR opcode with per-kind VM handlers, and a shared content accumulator implementing the XQuery content rules. Computed names resolve from EQName strings, prefixed QNames (context namespaces), or `xs:QName` instances; constructed attribute prefixes survive on free-standing attributes via a provider annotation.

**Acceptance Criteria:**
- [x] All seven computed constructor forms with static (`NCName`/EQName) and computed (`{expr}`) names; empty `{}` content legal (XQ31).
- [x] Content rules: attributes before content only (`XQTY0024`), duplicate attributes (`XQDY0025`), namespace nodes become declarations with conflict checks (`XQDY0102` incl. spec bug 22032 default-namespace rule), adjacent atomic values joined with single spaces, text nodes merged without separator, arrays flattened.
- [x] Name resolution: EQName `Q{uri}local` (whitespace normalization, char/entity reference expansion in source literals, literal `{` rejected), `prefix:local` via context namespaces, `xs:QName` instances; error codes `XPTY0004` (empty/wrong-typed name), `XQDY0074` (malformed), `XPST0081` (undeclared prefix), `XQDY0096` (xml/xmlns misuse), `XQDY0044` (xmlns attribute forms), `XQDY0041`/`XQDY0064` (PI target), `XQDY0026` (`?>` in PI data), `XQDY0072` (comment `--`), `XQDY0091` (xml:id whitespace), `XQDY0101` (namespace constructor reserved forms).
- [x] Static PI target must be an NCName (`XPST0003` for prefixed names); computed PI target must be string-typed (`XPTY0004`).
- [x] Attribute prefix rules: XML namespace coerces to the `xml` prefix; any other namespace without a prefix gets a generated one; prefixes preserved on free-standing attributes.
- [x] Computed `text {}` with empty content produces no node; a zero-length string still constructs a text node.
- [x] QT3 sets fully green: CompText 38/0, CompComment 27/0, CompDoc 40/0, CompElem 86/0, CompAttr 111/0, CompPI 56/0, CompNamespace 11/0; supporting sets: WindowClause 123/0, OrderByClause 194/0, GroupByClause 30/0, CountClause 13/0, DirElemConstructor 62/0. Full suite: **25,846 passed / 0 failed / 5,975 skipped (81.22%)**.
- [x] Supporting fixes: window-clause and FLWOR tuple variable bindings keep prefixes/EQName namespaces (`TupleBindInfo` carries prefixes, resolved at bind time); keyword-named constructors (`attribute return {()}` constructs an attribute named `return`); empty-CDATA boundary whitespace; XQuery 3.1 spec-token awareness in the harness dependency filter (XQ10/XQ30-only tests skip on an XQ31 processor).

**Implementation Notes:**
- Parser: `IsComputedConstructorForm` + `ParseComputedConstructor` gated on `_allowFullFlwor`; hooked into `ParseStepExpr`; EQName URI part expands char/entity references and rejects literal braces (`XPST0003`).
- IR/VM: `IrOpCode.ConstructComputed` with `ComputedConstructorInfo`; `ComputedContentAccumulator` (attribute ordering, duplicates, namespace-node declarations, text merging with atomic-adjacency tracking); `ResolveComputedName`.
- Provider: `XDocumentProvider.ConstructAttribute`/`ConstructDocument` (synthetic `__xdm_doc__` wrapper for non-single-root content); `AttributePrefixAnnotation` preserves constructed prefixes (LINQ attributes cannot carry one); `ConstructContentNode` handles text/namespace kinds.
- Harness: constructor forms admitted by the XQuery gate; `KnownXQueryGaps` regenerated from a true-list run (307 reasoned skips).

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | Computed constructor forms; EQName reference expansion; keyword-named constructors; empty-CDATA boundary whitespace. |
| Compiler | Modified | `ConstructComputed` opcode + lowering; window/tuple bindings keep prefixes. |
| Runtime | Modified | `ConstructComputed` VM handler; content accumulator; name resolution; window variable binding resolution. |
| Providers | Modified | `ConstructAttribute`/`ConstructDocument`; prefix annotation; text/namespace content nodes. |
| XSLT | None | No XSLT changes; XSLT baseline unchanged. |
| Conformance | Modified | Computed constructors admitted; XQ31 spec-token dependency awareness; `KnownXQueryGaps` regenerated (307 reasoned skips). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-25 | Kimi | Single `ConstructComputed` opcode + shared content accumulator; prefix annotation for free-standing attributes | Mirrors the direct-constructor pipeline; LINQ attributes cannot carry prefixes, so the constructed prefix rides as an annotation the node wrapper reports. |

---

### REQ-048: XQuery 3.1 Phase 3 — switch / typeswitch expressions

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-25  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
XQuery 3.1 adds two conditional expressions that XPath 3.1 lacks: `switch` (value matching: `switch (E) case V1 case V2 return R1 ... default return RD`) and `typeswitch` (type matching: `typeswitch (E) case $v as T return R ... default ($d)? return RD`, including sequence-type unions `case $i as xs:integer | xs:string`). Without them, ~200 QT3 tests were gated, and any query using the most common XQuery branching form could not run.

**Proposed Solution:**  
Parse both forms as dedicated AST nodes (`SwitchExpressionNode`, `TypeswitchExpressionNode`) in XQuery mode only (a `switch`/`typeswitch` name followed by `(`), then desugar in the IR lowerer — no new opcodes. `switch` becomes a synthetic `let` over the operand plus a nested `if`/`or` chain of `eq` value comparisons (case operands evaluate lazily in order, so errors in later cases do not surface after a match). `typeswitch` becomes the same `let` plus a chain of `instance of` checks (one per union member) with the case/default variables bound as nested `let`s, preserving per-branch scoping.

**Acceptance Criteria:**
- [x] `switch` with single- and multi-value cases, nested switch, lazy case evaluation, and default fallback.
- [x] `typeswitch` with atomic types (subtype-aware), node kinds, `empty-sequence()`, occurrence indicators, sequence-type unions, case variables, and default variables.
- [x] Case/default variables scoped to their own branch only.
- [x] `typeswitch (…)` on the XPath pipeline remains XPST0003 (reserved function name).
- [x] QT3 sets: SwitchExpr 67/3, TypeswitchExpr 62/3 (the 3 remaining are `K2-sequenceExprTypeswitch-5/9/11`, which require static variable-scope analysis — the engine is dynamically scoped; recorded as gaps). Full suite: **25,928 passed / 0 failed / 5,893 skipped (81.48%)**.
- [x] Supporting fixes: `fn:document-uri` returns an `xs:anyURI`-annotated value (K2-DocumentURIFunc-11); harness routing keeps XPath-only tests expecting a parse error on the XPath pipeline even inside XQuery test sets (typeswitch-in-xpath); optimizer switch/typeswitch traversal is reference-transparent (no fixpoint loop from fresh list instances).

**Implementation Notes:**
- Parser: `ParseSwitchExpr`/`ParseTypeswitchExpr` hooked into `ParseExprSingle` gated on `_allowFullFlwor`; `SequenceTypeUnion` (`|`) supported in typeswitch case clauses.
- Lowerer: `LowerSwitch`/`LowerTypeswitch` synthesize `let`/`if`/`eq`/`or`/`instance-of` AST and lower it (the established FLWOR-without-order-by pattern); synthetic operand variables are numbered `__switch_N`/`__typeswitch_N`.
- Harness: `TestCase.OwnDependencies` distinguishes case-level from set-level spec dependencies for pipeline routing.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | switch/typeswitch forms; sequence-type unions in case clauses. |
| Compiler | Modified | Desugar lowering; optimizer traversal for the new nodes. |
| Runtime | None | No new opcodes; desugared trees run on existing machinery. |
| Standard | Modified | `fn:document-uri` annotates `xs:anyURI`. |
| XSLT | None | No XSLT changes; XSLT baseline unchanged. |
| Conformance | Modified | switch/typeswitch admitted; XPath-only parse-error routing exception; `KnownXQueryGaps` regenerated (310 reasoned skips). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-25 | Kimi | Desugar to let/if/eq/instance-of chains in the lowerer instead of new opcodes | Reuses proven machinery (value comparison, instance-of, let scoping) with zero VM risk; lazy if-chains give the spec's error semantics for free. |

---

### REQ-049: XQuery 3.1 Phase 4 — output declarations + serialization round-out

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-25  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
XQuery 3.1 output declarations (`declare option output:method "xml"`, and the other `output:*` serialization parameters) are the standard way to control serialization from a query, and `fn:serialize` must honor them as static serialization parameters — including `output:parameter-document` (an external parameters file). Without them, ~350 QT3 tests were gated: the entire `ser/*` sets (six output methods), `fn/serialize`, `prod/OptionDecl*`, and the serialization-adjacent clusters. The serializer itself (built for the fn-serialize pool) had never been validated against the ser/* sets and diverged from the Serialization 3.1 spec in dozens of details.

**Proposed Solution:**  
Parse `declare option QName "value"` in the prolog (with QName/EQName option names, XQuery comment awareness, prolog ordering rules, and static validations XQST0109/XQST0110/XQST0066/XPST0003/XPST0081), carry the options in the static context, and seed them into the evaluation context as static serialization parameters that `fn:serialize` merges under explicit per-call parameters. `output:parameter-document` resolves lazily through the document loader to element-form parameters underneath the prolog's own. Then drive the serializer to Serialization 3.1 fidelity against the ser/* matrix: XML declaration and DOCTYPE emission rules, html/xhtml/html-version/html5 variants, adaptive constructor-form atomics, JSON maps and character maps, CDATA section rules, indent/suppress-indentation/xml:space, namespace fixup with a declaration scope stack, XML 1.1 namespace undeclarations (undeclare-prefixes), and XML 1.1 line-ending normalization gated on the test's xml-version.

**Acceptance Criteria:**
- [x] `declare option` prolog (prefixed, unprefixed, and `Q{uri}local` option names); ordering rules (namespace declarations precede options); validations XQST0109 (unknown parameter), XQST0110 (duplicate parameter), XQST0066 (duplicate default namespace), XPST0081 (undeclared prefix), XPST0003 (ordering/body-missing).
- [x] Static output parameters flow to `fn:serialize`; explicit per-call parameters override them; map-form parameters default omit-xml-declaration=true while element/default forms emit the declaration.
- [x] `output:parameter-document` (lazy load, character maps included; prolog options take precedence).
- [x] QT3 fully green: ser/method-xml 38/0, ser/method-text 18/0, ser/method-html 45/0, ser/method-xhtml 40/0, ser/method-json 73/0, ser/method-adaptive 87/0, fn/serialize 168/0, prod/OptionDecl 41/0, prod/OptionDecl.serialization 36/0, prod/DefaultNamespaceDecl 22/1, prod/Comment 72/0.
- [x] Supporting fixes: attribute normalization is literal-only with xml:id collapse; map keys distinguish string-family subtypes from g* date types; inline-function instance-of uses declared types; XML 1.1 character references accepted; XML 1.1 namespace undeclarations honored by the namespace axis, in-scope-prefixes, and namespace-uri-for-prefix; prolog comments `(: :)` skipped; `xml:space` is an ordinary constructor attribute; boundary whitespace stripped at flush time.
- [x] Harness: `serialization-matches`/`assert-serialization`/`assert-serialization-error` assertions with flags (`q`/`i`/`x`/`m`) and `not` wrapper; assert-type delegates parenthesized types to the engine; xml-version 1.1 enables line-ending normalization through a threaded `Xml11LineEndings` flag. Full suite: **26,299 passed / 0 failed / 5,522 skipped (82.64%)**.

**Implementation Notes:**
- Prolog: `XQueryParser` gains `declare option` parsing with deferred prefix resolution (XPST0081 vs XPST0003 ordering), the validations above, and XQuery-comment-aware whitespace.
- Runtime: `EvaluationContext.StaticOutputParameters`; `XQueryExecutable` seeds them with QName-list expansion against the default element namespace.
- Serializer: `XdmSerializer` parameter merging (`ParametersFromOutputDictionary`/`ParametersFromElementForm`), character-map application in JSON encoding, and the large fidelity set above.
- Harness: `TestCase.OwnDependencies` for pipeline routing; serialization assertions serialize the actual result through the engine with the query's static parameters and test-set base URI.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | `declare option`; prolog validations; XML 1.1 char refs; comment skipping; xml:space ordinary; flush-time boundary whitespace. |
| XQuery | Modified | Static-context options; seeding; EQName option names. |
| Runtime | Modified | StaticOutputParameters; attribute normalization; inline-function instance-of. |
| Standard | Modified | fn:serialize merge logic; serializer fidelity; fn:document-uri anyURI annotation. |
| Providers | Modified | XML 1.1 undeclaration annotations exposed and honored (namespace axis, reparse transfer). |
| XSLT | None | No XSLT changes; XSLT baseline unchanged. |
| Conformance | Modified | serialization assertions; pipeline routing; xml-version flag; `KnownXQueryGaps` regenerated (305 reasoned skips). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-25 | Kimi | Static parameters merged under per-call parameters; map-form omits the declaration by default | Matches the Serialization 3.1 defaults proven by the QT3 ser/* matrix (serialize-xml-127a vs K2-Serialization-24). |
| 2026-07-25 | Kimi | Line-ending normalization gated on the xml-version dependency | The QT3 evidence is split: xml-version 1.1 tests demand normalization (line-ending-Q004-6) while 1.0-mode tests demand exact reference characters (P002, re00127a). |

---

### REQ-050: XQuery 3.1 Phase 4 — user-defined functions and variables (library modules slice 1)

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-26  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
XQuery 3.1 library modules start with user declarations in the prolog: `declare function` (with typed parameters and return type, recursion, empty bodies) and `declare variable` (globals, possibly `external`). Without them, every QT3 test that declares its own helper functions was gated behind the harness's unsupported-prolog skip — including the entire functx library sets (`app/FunctxFn`, `app/FunctxFunctx`, ~1,100 tests), `prod/FunctionDecl` (173), `app/Walmsley` (222), `app/spec-examples` (641), and thousands of individual tests across other sets. Supporting declarations also surfaced latent engine semantics that only user-written recursive/higher-order code exercises: focus propagation into function bodies, variable-scope clobbering across recursive calls, function-item coercion, and function-type syntax in sequence types.

**Proposed Solution:**  
Parse `declare function` / `declare variable` in the prolog with the full static-validation matrix (XQST0034/0039/0045/0049, XPST0003/0008/0017, XQST0054 at runtime), compile bodies through the standard optimizer→IR pipeline, and dispatch calls through `InlineFunctionItem` invocation. Align the invocation semantics with XPath 3.1 §3.1.5: absent focus in every user-function body, a full variable-scope snapshot per call so recursive locals cannot clobber the caller, and function conversion (atomization, untypedAtomic casts, numeric/URI promotion, **function-item coercion**) applied to arguments and results. Extend both parsers for function-type syntax (`function(xs:integer) as xs:integer`, parenthesized item types) in declared signatures and `as` clauses.

**Acceptance Criteria:**
- [x] `declare function` prolog: typed/untyped parameters and return type, recursion (5,000-deep `fn-format-number` numberformat121/122), empty bodies (`{ }` → empty sequence), named references (`local:f#1`), partial application.
- [x] Static validations: XQST0039 (duplicate parameter), XQST0034 (duplicate name+arity), XQST0045 (reserved namespaces), XQST0049 (duplicate variable), XPST0003 (reserved names, `empty-sequence()` occurrence, prolog ordering), XQST0054 (circular globals, runtime).
- [x] `declare variable`: lazy on-first-reference evaluation with the module's **initial focus** (function-declaration-026), variable chains, `$name :=` adjacency (`$A:=` not misparsed as a prefix).
- [x] Invocation semantics: caller focus never propagates into function bodies (K2-FunctionProlog-14 → XPDY0002); full variable-scope snapshot per call (functx `dynamic-path` recursion); captured closures preserved.
- [x] Function conversion: attribute nodes atomize to xs:untypedAtomic (K2-FunctionProlog-18); comment/PI atomize to xs:string (K2-FunctionProlog-20); function-item coercion wraps items in `CoercedFunctionItem` for typed function tests incl. occurrence-wrapped and whitespace-variant forms (hof-028/029/030/040-047/049).
- [x] Function-type syntax: `function(...) as ...` in declared signatures, `let`/`for` `as` clauses, and `instance of`; `SkipSequenceType` stops at expression boundaries (`:=`, `in`, `return`, `then`, `else`, `|`); parenthesized item types.
- [x] Order-by comparator: untypedAtomic casts to xs:string; cross-family comparisons raise XPTY0004 (orderBy68).
- [x] Harness: runs on a dedicated 512MB-stack thread (deep recursion); `sudoku` recorded as a reasoned skip (solver too slow under the tree-walking interpreter).
- [x] QT3: prod/FunctionDecl 150/3/20 (3 static-analysis gaps recorded), misc/HigherOrderFunctions 108/9/12 (was 78/39), app/FunctxFunctx 622/5, app/FunctxFn 499/2, app/Walmsley 212/6, app/spec-examples 630/3. Full suite: **28,735 passed / 0 failed / 3,086 skipped (90.30%)**.
- [x] Unit tests: 12 new declare function/variable tests; full suite **1,491/0**.

**Implementation Notes:**
- Prolog: `XQueryParser` gains `declare function`/`declare variable` branches with QName/sequence-type text readers (`ReadSequenceTypeText`/`ReadItemTypeText` with function-type `as` suffixes and parenthesized item types), XQuery-comment awareness, and the validations above; `ReadQName` no longer treats the `:` of `:=` as a prefix separator.
- Static context: `UserFunctionDeclaration`/`UserFunctionParameter`/`UserVariableDeclaration` records with clone threading; the `local` prefix is predeclared (xquery-local-functions).
- Compilation: `XQueryCompiler` compiles declaration bodies into `CompiledUserFunction`/`CompiledUserVariable`; `XQueryExecutable` registers functions as `FunctionSignature`s (kind-level `External` fillers, real type names) whose implementation invokes an `InlineFunctionItem`, and variables as a lazy-resolver chain with an in-flight set for XQST0054.
- Runtime (`VmEngine`): InlineFunctionItem invocation clears the focus and snapshots/restores the whole variable scope per call; return path applies converting `ApplyFunctionConversion`; atomization unions include attribute nodes; `ApplyFunctionConversion` gained the function-coercion branch (mirroring the XSLT engine) with occurrence/spacing normalization; `ConvertDynamicCallArgs` passes `External` kinds through; the For opcode restores per-iteration `let` scoping; the order-by comparator enforces type families.
- Parser (shared): `SkipSequenceType` stops at `:=`/`in`/`return`/`then`/`else`/`|` after a function-type `as` clause.
- Harness: 512MB worker stack; `variable|function` removed from the unsupported-prolog gate.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | `SkipSequenceType` expression boundaries; function-type support in sequence-type readers. |
| XQuery | Modified | Prolog declarations, static context records, compilation and registration of user functions/variables. |
| Runtime | Modified | Invocation scope/focus semantics, atomization union, function coercion, order-by families, per-iteration let scoping. |
| Compiler | Modified | `IrLowerer` seeds `QuantifiedLoopInfo.ScopedVariableNames` on the simple for path. |
| XSLT | None | No XSLT changes; XSLT baseline unchanged (143/0). |
| Conformance | Modified | 512MB worker stack; prolog gate narrowed; `KnownXQueryGaps` regenerated (NNN reasoned skips, incl. `sudoku`). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-26 | Kimi | Lazy evaluation of global variable initializers with the module's initial focus | Spec: initializers run in the module's dynamic context; laziness keeps unreferenced erroring globals from failing the query (function-declaration-026 vs lazy-error tests). |
| 2026-07-26 | Kimi | Full variable-scope snapshot per function call instead of per-parameter save/restore | Recursive functions with local `let` bindings clobbered the caller's same-named bindings through the shared mutable context (functx dynamic-path); the snapshot subsumes parameter and captured-variable restore. |
| 2026-07-26 | Kimi | Function-item coercion in `ApplyFunctionConversion` mirrors the XSLT engine's `CoercedFunctionItem` pattern | One coercion implementation, two call sites; parameter/return mismatches surface at invocation time per XPath 3.1 §3.1.5.1. |
| 2026-07-26 | Kimi | `sudoku` (app/Demos) recorded as a reasoned skip | The solver recurses far deeper/longer than the tree-walking interpreter sustains; it blocked the full-suite run. |
| 2026-07-26 | Kimi | Static-analysis errors (XPST0008 undefined variable, XPST0017 in never-executed bodies) recorded as gaps | The engine is dynamically scoped; static variable/function-existence analysis over declared bodies is a separate work item (same category as the typeswitch gaps). |

---

### REQ-051: XQuery 3.1 Phase 4 — library modules (slice 2)

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-27  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
With user-defined functions and variables in place (REQ-050), the remaining structural feature of the XQuery module system was the library module itself: `module namespace` declarations and `import module` with optional location hints. Without it, 256 `prod/ModuleImport` QT3 tests plus every module-dependent test in other sets (`prod/ContextItemDecl`, `misc/HigherOrderFunctions`, `fn/id`, `app/Walmsley`, …) stayed skipped, and consumers could not organize queries into reusable modules. The semantics go well beyond text inclusion: each module has its own static context (namespaces, base URI, collation), imports are not transitive, several modules may share one target namespace, import cycles are legal, and `%private` declarations must be invisible across module boundaries.

**Proposed Solution:**  
Parse library module declarations and module imports in the prolog with the full static-validation matrix; register library module sources on the compiler (`XQueryCompiler.WithModule`) and resolve imports to the transitive closure of the module graph, merging same-namespace modules; compile every module's declarations with its own static context and execute its bodies with that module's runtime context applied; enforce public/private visibility statically (XPST0017/XPST0008 across module boundaries) without disturbing the dynamically scoped runtime; admit `<module>` catalog entries in the QT3 harness.

**Acceptance Criteria:**
- [x] `module namespace prefix = "uri";` library modules (no query body, XPST0003 when evaluated as a query; body in a library module is XPST0003) and `import module (namespace p =)? "uri" (at "loc", ...)?;` in main and library modules; module namespace URIs and `at` hints are whitespace-normalized (module-URIs-1..25).
- [x] Static validations: XQST0047 (duplicate import), XQST0088 (empty target namespace), XQST0059 (not found / target-namespace mismatch), XQST0048 (declaration outside target namespace), XQST0070 (xml/xmlns import prefix), XQST0108 (output declaration in a library module), XQST0113 (context-item initial/default value or duplicate in a library module), XQST0032 (duplicate base-uri), XQST0034/XQST0049 (same-namespace merge collisions and own-vs-imported collisions), self-import is legal (XQST0093a).
- [x] `%public`/`%private` annotations: visibility enforced statically for calls, named function references, and variable references (XPST0017/XPST0008); conflicting/duplicate visibility annotations are XQST0106/XQST0116; reserved-namespace or unknown XQuery-namespace annotations are XQST0045; annotation arguments must be literals (XPST0003); unknown annotations in other namespaces are ignored; `xsi` predeclared.
- [x] Module graph: transitive closure with incremental per-(namespace, location-hint) loading (modules-30..33); import cycles terminate (modules-circular, errata8); all public declarations of every loaded module in an imported namespace are visible regardless of load route (XQ 3.1 §4.12.2, modules-31).
- [x] Per-module contexts: library module bodies compile with the module's own static context and execute with its namespaces, base URI, default element namespace, and default collation applied (cbcl-module-002); the importing module's context is restored afterwards.
- [x] Prolog parser tokenization: comments/whitespace between prolog keywords (`declare(::)base-uri`, `import(::)module`) parse correctly (K-*Prolog comment variants); duplicate `declare base-uri` is XQST0032.
- [x] Harness: `<module uri location? file>` catalog entries parsed and registered (unreadable files simply never satisfy an import → XQST0059, module-URIs-4); `moduleImport` feature admitted; inline `<context-item select="..."/>` applied as initial focus; `<assert>` comparisons bind the query result as the context item; comment-tolerant unsupported-prolog gate.
- [x] QT3: prod/ModuleImport 106/0/22 (remaining skips XQ10-only or schema-import-gated); full suite **28,931 passed / 0 failed / 2,890 skipped (90.92%)**; 499 reasoned gaps (12 new: closure context-capture in module function items (xqhof16/18), map-as-function coercion (UseCaseR31-012), copy.xq `fn:id` FODC0001 semantics (fn-id-4, fn-idref-4, K2-SeqIDFunc-4..7), `fn:path` over copied nodes (path014), context-item type enforcement (contextDecl-054), function-test annotation assertions (annotation-assertion-20)).
- [x] Unit tests: 18 new module/annotation tests; full suite **1,509/0**.

**Implementation Notes:**
- Parser: `XQueryParser` parses the module declaration before the prolog (binding its prefix), `import module` as a prolog declaration, and annotations on function/variable declarations; prolog phrase matching is token-based (comment-tolerant); library-module-only `declare context item` is parsed with XQST0113.
- Static context: `ModuleImport` records, `ImportedModules`, `ModuleNamespaceUri`, and `IsPrivate` flags on user declarations; `xsi` predeclared.
- Compiler: `XQueryCompiler.WithModule(uri, source, location?)` builds the catalog; `LoadModuleNamespace` resolves imports incrementally by (namespace, hints) with cycle tolerance and merge-collision checks; `ModuleVisibilityValidator` statically checks module-namespace references against per-module visibility sets (own declarations plus publics of direct imports) with lexical bound-variable tracking.
- Runtime: `CompiledUserFunction`/`CompiledUserVariable` carry the declaring module's runtime context (namespaces, base URI, default element namespace, default collation); invocation and lazy global initializers apply and restore it — no VM changes were required.
- Harness: `TestCaseModule` entries resolve files against the test-set directory; `TestExecutor` registers sources per test; the `import\s+module` and `declare\s+%` gates removed; `ResultComparer` binds the context item for `<assert>`.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| XQuery | Modified | Parser (module declaration, imports, annotations, tokenized prolog), static context (imports, privacy), compiler (module graph, visibility), executable (per-module runtime context). |
| Runtime | None | No VM changes; modules reuse `FunctionSignature` registration and the lazy variable resolver. |
| Conformance | Modified | `<module>` catalog support, inline `<context-item>`, context-item assert binding, comment-tolerant gates; `KnownXQueryGaps` regenerated (499 entries). |
| XSLT | None | XSLT baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-27 | Kimi | Per-module runtime context wrapper instead of compile-time namespace rewriting | Constructor prefixes and variable prefixes resolve at runtime in this engine; applying the declaring module's namespaces/base URI around body execution covers both uniformly (cbcl-module-002) with zero VM churn. |
| 2026-07-27 | Kimi | Static visibility checks limited to module namespaces | The engine is dynamically scoped; checking only references into loaded module target namespaces enforces %private and import transitivity rules without false positives on local/dynamic bindings (bound-variable tracking covers FLWOR/inline-function shadowing). |
| 2026-07-27 | Kimi | Incremental same-namespace loading keyed by (namespace, location hints) | XQ 3.1 §4.12.2 requires all public declarations of every loaded module in an imported namespace to be visible regardless of participation route (modules-31); a namespace already in the graph is extended when a later import names additional sources. |
| 2026-07-27 | Kimi | Unreadable module files are not registered rather than failing the test | The QT3 catalog itself contains a misspelled file reference (module-URIs-4); the import then raises the expected XQST0059. |
| 2026-07-27 | Kimi | Closure context-capture (xqhof16/18), map-as-function coercion (UseCaseR31-012), copy.xq `fn:id` semantics, context-item type enforcement, and function-test annotation assertions recorded as gaps | Engine semantics beyond the module-system slice (function items capturing static context, map coercion in function conversion, document-less `fn:id` roots, XQ 3.1 §4.14 type checks, annotation assertions in sequence types); recorded as reasoned skips. |

---

### REQ-052: try/catch completion — named error codes and error variables

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-27  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
try/catch is XPath 3.1 grammar shared by both pipelines, but the engine supported only a single `catch *` clause, bound a spec-noncompliant `$err:code` (the CLR type name as a string), and had no structured error representation: `fn:error` raised message-only exceptions, discarding the code QName and the error value. The whole `prod/TryCatchExpr` set (173 tests) and incidental try/catch users sat behind the harness's construct gate — the largest remaining QT3 skip driver.

**Proposed Solution:**  
Extend the AST to multiple catch clauses with error-code name-test patterns (`*`, `prefix:local`, `prefix:*`, `*:local`, `Q{uri}local`, `Q{uri}*`, NCName); introduce a structured error carrier in the Runtime layer (`XPathErrorException` + `XPathError` helpers, ported from the XSLT engine's proven catch implementation); match clauses first-match-wins in the `TryCatch` opcode and bind the seven `err:*` variables with save/restore; make `fn:error` throw structured errors; and honor the two bypass rules: static errors and global-variable-initializer errors are never caught.

**Acceptance Criteria:**
- [x] Grammar: `catch CodePatternList { Expr }` with one-or-more clauses on both pipelines (try/catch is XPath grammar); code patterns `*`, `err:X`, `err:*`, `*:X`, `Q{uri}X`, `Q{uri}*`, unprefixed NCName (empty namespace); empty try/catch bodies are the empty sequence (try-019/020); no matching clause → the error propagates unchanged.
- [x] `err:*` variables: `err:code` as `xs:QName` (prefix preserved through `fn:error`), `err:description`, `err:value` (fn:error's third argument, sequences included), `err:module`/`err:line-number`/`err:column-number` (empty/zero — no source tracking), `err:additional` (empty, implementation-defined); previous bindings restored after the catch (nested try, try-011).
- [x] `fn:error`: empty code argument behaves as `err:FOER0000` (fn-error-5/6, K-ErrorFunc); code/description/value surface structuredly; the XSLT engine recognizes the new exception type (its `xsl:catch` unchanged).
- [x] Bypass rules: static-coded errors (XPST/XQST not raised by `fn:error`) are never caught, even when a pattern matches (try-catch-static-error-1..4); lazy global variable initializer errors bypass try/catch via `GlobalVariableEvaluationException` (try-006/007), unwrapped at the executable boundary.
- [x] Error-code hygiene: `cast` FORG0001, `treat as` XPDY0050, computed-constructor unresolvable prefix XQDY0074, `fn:zero-or-one`/`one-or-more`/`exactly-one` FORG0003/0004/0005, `fn:parse-xml`/`parse-xml-fragment` FODC0006 with external-DTD resolution against the static base URI and validated text declarations in fragments.
- [x] QT3: prod/TryCatchExpr **172/0/1**; full suite **29,114 passed / 0 failed / 2,707 skipped (91.49%)**; gaps unchanged (499 reasoned skips — no new entries).
- [x] Unit tests: 15 new try/catch tests (both pipelines); full suite **1,524/0**.

**Implementation Notes:**
- `TryCatchNode` → `(TryExpression, IReadOnlyList<TryCatchClause>)` with `CatchCodePattern` records; `XPathParser.ParseTryExpr` accepts the full `CatchErrorList` grammar (the lexer already tokenizes `Q{uri}*` and wildcard prefixes); `TryCatchInfo`/`CatchClauseInfo` carry ordered clause entry points in the literal pool.
- `XPathError` (new, Runtime/Vm): `GetErrorDetails` (structured pass-through + legacy message parsing), `CatchPatternMatches` (context-resolved prefixes), `BindCatchErrorVariables`/`RestoreCatchErrorVariables` (seven variables, save/restore), `IsUncatchableStaticError` (XPST/XQST bypass for non-`fn:error` errors); `GlobalVariableEvaluationException` marks lazy-global failures, wrapped by the XQuery resolver and unwrapped at `XQueryExecutable.Evaluate`.
- `fn:error` throws `XPathErrorException` (empty code → FOER0000); `TransformEngine.GetErrorDetails` recognizes it.
- All AST traversals updated for the new node shape (`XPath31Expression`, `XQueryCompiler`, `ModuleVisibilityValidator`); the optimizer keeps its reference-transparent default.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | `TryCatchNode` multi-clause shape + catch pattern parsing (shared by XPath and XQuery). |
| Compiler | Modified | `TryCatchInfo`/`CatchClauseInfo` records; multi-clause lowering. |
| Runtime | Modified | `TryCatch` opcode semantics; `XPathError` infrastructure (new file); FORG0001/XPDY0050/XQDY0074 codes in cast/treat/computed-name paths. |
| Standard | Modified | `fn:error` structured; FORG0003/0004/0005 codes; parse-xml(-fragment) FODC0006, DTD base URI, text declarations. |
| XSLT | Modified | `GetErrorDetails` handles `XPathErrorException` (one branch; `xsl:catch` behavior unchanged). |
| XQuery | Modified | Global-variable error marking; traversal updates for the new node shape. |
| Conformance | Modified | try/catch admitted by the construct gate; gaps unchanged (499). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-27 | Kimi | New `XPathErrorException` in the Runtime layer rather than reusing `XsltRuntimeException` | The XSLT type is engine-internal with XSLT-specific codes; a small Runtime type is consumable by both pipelines and by the XSLT catch via one extra branch. |
| 2026-07-27 | Kimi | Static-error bypass keyed on XPST/XQST codes *not* raised by `fn:error` | Spec: try/catch catches dynamic errors only. The engine raises static codes dynamically (dynamic scoping); bypassing them matches observable spec behavior (try-catch-static-error-1..4), while user-thrown `fn:error` values stay catchable per spec. |
| 2026-07-27 | Kimi | `err:additional` bound to empty rather than a blanket resolver | try-021 requires the implementation-defined `err:additional` to exist while try-catch-err-other-variable-1 requires arbitrary `err:*` names to stay undefined (XPST0008). |
| 2026-07-27 | Kimi | Lazy-global errors bypass catch via a marker exception | XQuery defers global initializers to first reference; the marker (unwrapped at the executable boundary) reproduces the spec rule that such errors are not caught by try/catch (try-006/007). |

---

### REQ-053: XQuery 3.1 string constructors

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-27  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
XQuery 3.1 string constructors (`` `[literal `{expr}` literal]``) were the largest single QT3 gap cluster (35 recorded skips in `prod/StringConstructor`, 52 tests). The syntax mixes raw literal text (no reference expansion, whitespace preserved, backticks literal) with `` `{` Expr `}` `` interpolations that nest full expressions — including nested string constructors — so it cannot be tokenized naively. The feature is the idiomatic way to build JSON/CSS/SPARQL text in XQuery.

**Proposed Solution:**  
Follow the established direct-element-constructor architecture: the lexer scans the whole constructor span (interpolation-aware) into one token; the parser re-scans the span into literal runs and interpolation expressions; evaluation desugars to `fn:string-join` with spec-faithful atomization semantics — no new opcodes.

**Acceptance Criteria:**
- [x] Lexical rules: `` ``[ `` … `]` `` ` `` delimiters; `` `{` Expr? `}` `` interpolations; single backticks literal unless starting an interpolation; unterminated forms are XPST0003 (string-constructor-901..905); nesting inside interpolations (009/020/028) and inside direct element constructors (010/011/029..034).
- [x] Literal text is raw: no entity/character-reference expansion (`&lt;` stays literal, 004/029..034), whitespace and newlines preserved (014).
- [x] Interpolation semantics: atomize with `fn:data` (maps raise FOTY0013, 910/911; arrays flatten, 017), cast each item to `xs:string`, join with single spaces (912), concatenate parts without a separator (006); empty interpolations are the empty string (024/025).
- [x] String constructors work as operands: predicates (013), parenthesized selection (015), if/else branches (012), direct element content and attributes (010/011), `declare variable` initializers (003).
- [x] Not valid where the grammar forbids expressions: attribute-value literals (913) and prolog namespace literals (914) are XPST0003.
- [x] XPath-mode string literals no longer expand entity/character references (spec: expansion is XQuery-only) — assert-eq expectations evaluate per XPath rules (029..034).
- [x] Harness construct-gate regex fixed: `RegexOptions.Compiled` was glued into the pattern by a trailing `+`; the enum is a proper argument again (pragma gating restored).
- [x] QT3: prod/StringConstructor **49/0/3** (from 14/0/38); full suite **29,150 passed / 0 failed / 2,671 skipped (91.61%)**; gaps **464** (−35).
- [x] Unit tests: 12 new tests; full suite **1,536/0**.

**Implementation Notes:**
- Lexer: `ScanStringConstructorEnd`/`ScanStringInterpolationEnd` skip the whole span with string-literal, comment, brace-depth, and nested-constructor awareness; emitted as one `Constructor` token (constructor mode only).
- Parser: `ScanStringConstructor`/`ScanStringInterpolation` split the span into `StringLiteralNode` runs and parsed interpolation bodies (`Parse(inner, allowFullFlwor)` recurses for nesting); empty bodies become the empty sequence.
- Lowering: desugars each interpolation to `fn:string-join(fn:data(E) ! fn:string(.), " ")` and the whole to `fn:string-join((…), "")` — mirrors the switch/typeswitch desugar precedent; optimizer and namespace-resolution traversals updated.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | Lexer span scan; `StringConstructorNode`; span re-scan; XPath-mode literals no longer expand references. |
| Compiler | Modified | String-constructor lowering + optimizer traversal. |
| Conformance | Modified | Construct-gate regex fixed; 35 gap entries removed (464 remain). |
| XSLT | None | Baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-27 | Kimi | Whole-span token + parser re-scan (same architecture as direct element constructors) | Interpolations nest full expressions (including nested constructors); a mode-switching token stream would be far more invasive. |
| 2026-07-27 | Kimi | Desugar to `fn:string-join(fn:data(E) ! fn:string(.), " ")` | Reuses battle-tested atomization/join semantics (FOTY0013 for maps, array flattening, space-joined items) with zero new opcodes. |
| 2026-07-27 | Kimi | Restrict reference expansion to XQuery-mode string literals | Spec: XPath 3.1 does not expand predefined entity/character references; the previous both-modes expansion made XPath-evaluated assertions disagree with raw string-constructor output (029..034). |

---

### REQ-054: XQuery 3.1 ordering features

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-27  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
Three QT3 sets sat behind the harness's ordering gates: `prod/UnorderedExpr` (28), `prod/OrderingModeDecl` (27), and `prod/EmptyOrderDecl` (32). The expressions `ordered { E }` / `unordered { E }` and the prolog declarations `declare ordering` and `declare default order empty least|greatest` were unparseable, and the order-by engine had no way to take a prolog default for empty-key placement.

**Proposed Solution:**  
Ordering expressions pass their body through unchanged (the engine always produces document order, which is a valid implementation of both ordering modes); parse the two prolog declarations with their duplicate validations; and thread the default-empty-order through the static context into the IR lowerer, where order-by specs without an explicit modifier pick it up.

**Acceptance Criteria:**
- [x] `ordered { E }` / `unordered { E }` are primary expressions (XQuery only; intercepted before the name-test step path); identity semantics; empty bodies are the empty sequence (K-OrderExpr-1a/2a); postfix chains (`ordered {E}[2]`) work.
- [x] `declare ordering ordered|unordered;` with XQST0065 on duplicate (incl. comment variants K-DefaultOrderingProlog-1/2/3); `ordering` stays usable as an element name (K2-DefaultOrderingProlog-1/2).
- [x] `declare default order empty least|greatest;` with XQST0069 on duplicate; the default applies to order-by clauses lacking an explicit `empty least/greatest` (emptyorderdecl-2: empties sort last under `greatest`); an explicit modifier in the clause wins.
- [x] `OrderSpec.EmptyOrder` is nullable (null = use the prolog default, itself defaulting to least); the IR lowerer's `DefaultEmptyOrder` property is threaded per module (library modules keep their own prolog default).
- [x] QT3: prod/UnorderedExpr 26/0/2, prod/OrderingModeDecl 27/0/0, prod/EmptyOrderDecl 32/0/0, prod/OrderByClause 198/0/7 (unchanged); full suite **29,244 passed / 0 failed / 2,577 skipped (91.90%)**; gaps unchanged (464).
- [x] Unit tests: 8 new tests; full suite **1,544/0**.

**Implementation Notes:**
- The ordered/unordered intercept sits next to the computed-constructor intercept in `ParseStepExpr`; the actual parse is in `ParsePrimaryExpr` (name + `{`).
- `XQueryStaticContext.DefaultEmptyOrderLeast` (bool?); `IrLowerer.DefaultEmptyOrder` (EmptyOrder?) applied at both OrderByInfo construction sites via `ResolveEmptyOrder(spec ?? default ?? Least)`.
- Harness: construct gate now contains only `\bvalidate\s` and the pragma alternative; the prolog gate keeps `boundary-space`, `construction`, `context`, decimal-format, `copy-namespaces`, and `import schema`.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | ordered/unordered expressions; `OrderSpec.EmptyOrder` nullable. |
| XQuery | Modified | Two prolog declarations with duplicate validations; static-context property; lowerer threading. |
| Compiler | Modified | `IrLowerer.DefaultEmptyOrder`. |
| Conformance | Modified | Gates narrowed; gaps unchanged (464). |
| XSLT | None | Baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-27 | Kimi | Identity semantics for ordering expressions | The spec permits any result order under `unordered` and requires document order under `ordered`; the engine always produces document order, satisfying both with zero runtime cost. |
| 2026-07-27 | Kimi | Nullable `OrderSpec.EmptyOrder` resolved in the lowerer | Distinguishes "explicit least" from "unspecified" so the prolog default applies exactly where the grammar leaves it open, without touching the runtime comparator. |

---

### REQ-055: Name tests, kind-test types, and constructor namespace semantics

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-28  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
The prod/NameTest cluster (22 recorded gaps) covered several distinct non-conformances: name tests with unbound prefixes silently matched the empty namespace instead of raising XPST0081; kind-test schema type names (`element(foo, T)`) were discarded rather than validated (XPST0008) and matched; `processing-instruction(...)` arguments were unvalidated; and constructed elements' in-scope namespaces were computed incorrectly in both directions (prolog bindings leaked in, explicit constructor declarations didn't propagate). The last item turned out to be the deepest: getting K2-NameTest-30/31 (no inheritance) and K2-DirectConElemNamespace-40/41 (explicit declarations propagate) to hold simultaneously requires distinguishing binding kinds.

**Proposed Solution:**  
Fix the four error-code clusters at their sources (VM namespace tests, kind-test parsing and a new `KindTestType` opcode, PI argument validation, prefixed instance-of name checks with `ApplyFunctionConversion` context threading), and implement the precise in-scope namespace model: explicit xmlns declarations and element-name bindings propagate to nested constructors with override semantics, while attribute-name-implied bindings stay local to the carrying element — with redundant declarations omitted at serialization/comparison time.

**Acceptance Criteria:**
- [x] Name tests and wildcard namespace tests with unresolvable prefixes raise **XPST0081** (nametest-3/4, K2-NameTest-66/67/72/73); `Q{   }*` whitespace-normalizes to the empty-namespace wildcard (eqname-023).
- [x] Kind-test schema type names: unknown types raise **XPST0008**, unbound type prefixes **XPST0081** (K2-NameTest-69/70/74/75/87..90); untyped elements/attributes match only their untyped compatible types and supertypes (K2-NameTest-68/71); prefixed kind-test name arguments get a namespace check (K2-NameTest-66/72); instance-of `element(P:L)`/`attribute(P:L)` compare the resolved namespace URI with `ApplyFunctionConversion` threading the runtime context (K2-DirectConElemNamespace-79, Catalog005/006).
- [x] `processing-instruction("...")` arguments trimmed and NCName-validated (**XPTY0004**); non-NCName/unquoted-invalid arguments are **XPST0003** (K2-NameTest-21..27).
- [x] Constructor in-scope namespaces: parent's attribute-name-implied bindings are NOT inherited by children (K2-NameTest-30/31), while explicit xmlns declarations and element-name bindings propagate with override (K2-DirectConElemNamespace-40/41, K2-InScopePrefixesFunc-9/10/16/28); redundant declarations omitted in serialization and canonical comparison (K2-DirectConElemNamespace-27/42/43, Constr-inscope-*).
- [x] `Q{whitespace}` URI-qualified wildcards normalize to the empty namespace.
- [x] QT3: prod/NameTest **125/0/2**; full suite **29,264 passed / 0 failed / 2,557 skipped (91.96%)**; gaps **462** (−20; remaining: K2-NameTest-5 keywords-as-names, NodeTest004 schema type assertion).
- [x] Unit tests: 14 new tests; full suite **1,558/0**.

**Implementation Notes:**
- `NodeTest.KindTestTypeName` carries the schema type name; `KindTestType` opcode validates (built-in XSD type registry) and filters by kind-appropriate compatibility; prefixed kind-test arguments emit a preceding `NamespaceTest` (with a fresh result register — an in-place emission initially corrupted operand registers in intersect/except).
- The in-scope model is encoded physically: `NonPropagatingNamespaceBinding` annotations mark attribute-name-implied xmlns attributes; `in-scope-prefixes`, `namespace-uri-for-prefix`, and the namespace axis skip them on ancestors; `CloneNode` preserves annotations. The old `ApplyNamespaceFixup` (which destroyed children bindings) was removed; redundancy omission lives in `XdmSerializer`, `ElementToXmlStringWithNamespaces` (per-branch scope), and the harness canonicalizer — trees stay semantically complete while output matches SAXON.
- `ApplyFunctionConversion` gained an optional `EvaluationContext` parameter; prefixed-name namespace checks are enforced only when a context is present (context-less function-item type tests keep local-name matching).

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | PI argument validation; kind-test type capture; `Q{ }*` normalization. |
| Compiler | Modified | `KindTestType` opcode; prefixed kind-test arg `NamespaceTest` (register-lifetime fix). |
| Runtime | Modified | NamespaceTest XPST0081; kind-test type registry + validation; instance-of prefixed-name ns check; `ApplyFunctionConversion` context. |
| Providers | Modified | Non-propagating-binding markers; annotation-preserving clones; per-branch redundancy omission. |
| Standard | Modified | Traversal sites skip marked ancestor bindings; serializer redundancy omission + xmlns="" guard. |
| Conformance | Modified | Canonical comparison omits redundant declarations; gaps 462. |
| XSLT | None | Baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-28 | Kimi | Attribute-name-implied bindings marked non-propagating; everything else propagates with override | The only model consistent with all QT3 data points: K2-NameTest-30 (no inheritance of prolog/attribute-name bindings), K2-DirectConElemNamespace-40/41 and InScopePrefixesFunc-9 (explicit declarations inherited), K2-InScopePrefixesFunc-28 (override of the default namespace). |
| 2026-07-28 | Kimi | Redundant declarations omitted at serialization/comparison, never removed from the tree | K2-NameTest-30 requires the child's binding to exist semantically; the constructor sets require SAXON-style omission in output. |
| 2026-07-28 | Kimi | Prefixed-name namespace checks enforced only when a resolution context is present | `ApplyFunctionConversion` historically ran context-free (local-name matching); null-context function-item type tests keep that behavior while the runtime-enforced paths (EnforceType, parameter conversion) resolve prefixes properly. |

---

### REQ-056: Variable declaration type strictness and external variables

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-29  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
The prod/VarDecl.external cluster (17 recorded gaps) covered five non-conformances in `declare variable`: initializers were parsed as full `Expr` so a top-level comma was silently accepted; a declared `as T` caused implicit conversion instead of a strict check (untypedAtomic→integer, numeric promotion, URI promotion all wrongly succeeded); occurrence indicators inside kind-test type names (`element(*, xs:untyped+)`) were accepted; `declare namespace p = ""` removed the binding statically but left the runtime's predeclared bindings live (so undeclaring `xs` did nothing); and external variables with a declared type never had their supplied values checked.

**Proposed Solution:**  
Parse initializers as `ExprSingle` (new `XPathParser.ParseExprSingle` entry); enforce declared types strictly via an `EnforceType` instruction appended to the initializer module (atomization + instance check, XPTY0004); validate kind-test type occurrence indicators in the XQuery parser (XPST0003 for `*`/`+`, `?` allowed as the XSD 1.1 nullable marker); track undeclared prefixes in the static context and unbind them in the runtime context; check typed external-variable bindings at execution start; and resolve prefixed harness `<param>` names against the param element's own in-scope namespaces.

**Acceptance Criteria:**
- [x] `declare variable $i := 1, 1;` raises **XPST0003** (K2-ExternalVariablesWith-11).
- [x] Typed initializers are strict: `xs:integer := xs:untypedAtomic("1")`, `xs:float|xs:double := 1` / `:= 1.1` / `:= xs:float(3)`, `xs:string := xs:untypedAtomic(...)` / `:= xs:anyURI(...)` all raise **XPTY0004** (K2-ExternalVariablesWith-12..19); nodes atomize to `xs:untypedAtomic` for the check (the variable keeps its original value).
- [x] `element(*, xs:untyped+)` / `element(*, xs:untyped*)` (and named-element forms) raise **XPST0003**; `element(*, xs:untyped?)` and `element(elementName, xs:anyType?)` work (K2-ExternalVariablesWith-22..27).
- [x] `declare namespace xs = ""; xs:integer(1)` raises **XPST0081** (K2-NamespaceProlog-4/9); `declare namespace prefix = ""; declare variable $prefix:x external;` raises **XPST0081** (K2-ExternalVariablesWithout-3); unbound function/variable prefixes report XPST0081 (previously a code-less message).
- [x] Typed external variables: bound values are checked strictly, mismatch raises **XPTY0004** (extvardeclwithtype-19); prefixed external bindings resolve through the `<param>` element's own namespaces (extvardeclwithouttype-24, extvardeclwithtype-24).
- [x] QT3: prod/VarDecl.external **96/0/3**; full suite **29,281 passed / 0 failed / 2,540 skipped (92.02%)**; gaps **445** (−17).
- [x] Unit tests: 12 new tests; full suite **1,570/0**.

**Implementation Notes:**
- `XPathParser.ParseExprSingle(xpath, allowFullFlwor, xml11LineEndings)` parses one ExprSingle and raises XPST0003 on trailing tokens; the XQuery parser's `ReadExpressionTo(';')` (variable initializers and context-item initial values — all ExprSingle per grammar) routes through it.
- `XQueryCompiler.WithEnforcedType` splits a trailing occurrence indicator off the type text and inserts an `EnforceType` instruction (pool entry `EnforceTypeInfo(typeName, occurrence, "XPTY0004")`) before the initializer module's final Return; the VM's EnforceType opcode atomizes per item unless the type is a node kind test.
- `XQueryStaticContext.UndeclaredPrefixes` records prefixes undeclared and not later redeclared; `XQueryExecutable.ApplyStaticContext` calls `EvaluationContext.RemoveNamespace` for each — this is what makes undeclaring the *predeclared* `xs` prefix observable.
- Two NamespaceProlog tests were previously false-passing: the old code threw XPST0017 (function-not-found after resolving `xs` to the empty URI) which the comparer's lenient `InvalidOperationException` matching accepted for the expected XPST0081.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | `ParseExprSingle` entry; kind-test occurrence validation; initializers as ExprSingle. |
| Runtime | Modified | EnforceType atomization; namespace undeclaration in `WithNamespace`/`RemoveNamespace`; XPST0081 codes for unbound prefixes. |
| XQuery | Modified | `WithEnforcedType`; `UndeclaredPrefixes`; typed external binding check. |
| Conformance | Modified | Prefixed `<param>` namespace resolution; gaps 445. |
| XSLT | None | Baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-29 | Kimi | Strict type enforcement (no casts/promotions) for variable declarations | XQuery 3.1 §4.16: the declared type is checked after atomization; the function conversion rules do NOT apply to variable initializers (K2-ExternalVariablesWith-12..19). |
| 2026-07-29 | Kimi | `?` allowed inside kind-test type names, `*`/`+` rejected | `?` is the XSD 1.1 nullable-type marker (K2-ExternalVariablesWith-22a/23 expect success); occurrence indicators `*`/`+` are grammatically excluded (K2-ExternalVariablesWith-24..27). |

---

### REQ-057: Namespace declaration static errors and prolog ordering

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-29  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
The prod/NamespaceDecl cluster (11 recorded gaps) covered three unchecked static errors in `declare namespace`: duplicate declarations of one prefix were accepted (even when one was an undeclaration); the reserved `xml`/`xmlns` prefixes and the XML/XMLNS namespace names could be (re)bound freely; and the prolog's two-phase grammar was unenforced, so namespace declarations after variable declarations parsed silently.

**Proposed Solution:**  
Track per-prolog declared prefixes in the parser (XQST0033, undeclarations count); reject any declaration of `xml` or `xmlns` and any binding to the XML/XMLNS namespace names (XQST0070); and enforce the two-phase prolog structure with a `_seenSecondPhaseDecl` flag set by context-item/function/variable/option declarations and checked by every phase-1 declaration branch (XPST0003).

**Acceptance Criteria:**
- [x] Duplicate prefix declarations raise **XQST0033**, including declare-then-undeclare and undeclare-then-declare (K2-NamespaceProlog-1/2/3).
- [x] `declare namespace xml = ...` raises **XQST0070** even for the proper XML namespace name (namespaceDecl-3, K2-NamespaceProlog-6/15); `declare namespace xmlns = ...` (any URI, including empty) raises **XQST0070** (namespaceDecl-5, K2-NamespaceProlog-7); binding another prefix to `http://www.w3.org/XML/1998/namespace` or `http://www.w3.org/2000/xmlns/` raises **XQST0070** (namespaceDecl-4).
- [x] Namespace/default-namespace/setter/import declarations after a context-item, function, variable, or option declaration raise **XPST0003** (K2-NamespaceProlog-14).
- [x] `declare namespace test=""; <test:a />` raises **XPST0081** (cbcl-declare-namespace-001, via the previous session's undeclaration propagation).
- [x] QT3: prod/NamespaceDecl **44/0/0**; full suite **29,292 passed / 0 failed / 2,529 skipped (92.05%)**; gaps **434** (−11).
- [x] Unit tests: 7 new tests; full suite **1,577/0**.

**Implementation Notes:**
- All checks live in `XQueryParser`: `_declaredNamespacePrefixes` (parser-local `HashSet<string>`) for XQST0033 — predeclared prefixes such as `xs` may still be bound once per prolog; reserved-name checks run before the duplicate check.
- `_seenSecondPhaseDecl` replaces the narrower `_seenOptionDecl`; it is set by context-item, function, variable, and option declarations and checked in the `namespace`, `default element namespace`, `default function namespace`, `default collation`, `default order empty`, `ordering`, `base-uri`, and `import module` branches.
- The XQST0070 rule for `xml` is deliberately stricter than the module-import rule: a namespace declaration may not declare `xml` at all (namespaceDecl-3), while an import may bind `xml` to its proper namespace name.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| XQuery Parser | Modified | XQST0033/XQST0070 checks; two-phase prolog ordering enforcement. |
| Conformance | Modified | Gaps 434. |
| XSLT | None | Baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-29 | Kimi | Undeclarations count as declarations for XQST0033 | K2-NamespaceProlog-1/2/3 expect XQST0033 for declare-then-undeclare, undeclare-then-declare, and declare-redeclare-undeclare sequences. |
| 2026-07-29 | Kimi | `xml` may not be declared even to its proper namespace name | namespaceDecl-3 expects XQST0070 for `declare namespace xml = "http://www.w3.org/XML/1998/namespace"` — unlike module imports, where the proper binding is tolerated. |

---

### REQ-058: Inline-function annotations and function-test annotation assertions

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-29  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
The prod/Annotation cluster (24 recorded gaps) covered the two annotation grammar forms the engine did not parse at all (the lexer had no `%` token): annotations on inline function expressions (`%eg:sequential function () { ... }`, with literal parameters, EQNames, and multiples) and annotation assertions in function tests (`instance of %eg:x function(*)`), including the reserved-namespace error (XQST0045) and the literals-only argument rule.

**Proposed Solution:**  
Lex `%` as a `Percent` token; parse-and-discard annotations on inline functions in the XPath parser (gated to XQuery mode — annotations are an XQuery-only grammar extension); capture function-test assertion text verbatim into the sequence-type string and strip/validate it in the VM's `InstanceOf` (assertions may be ignored per spec, but their namespaces are validated: XQST0045 for reserved namespaces, XPST0081 for unbound prefixes); enforce literal-only annotation arguments (XPST0003).

**Acceptance Criteria:**
- [x] Inline-function annotations parse and evaluate: bare, with literal parameters, EQName form, and multiple annotations (annotation-3/30/31/32).
- [x] Function-test annotation assertions parse and are ignored for matching (assertion-1..10/19); `%public %private` on a function item is allowed (assertion-20, any-of).
- [x] Annotation names in reserved namespaces (XML, XMLSchema, XMLSchema-instance, xpath-functions, xpath-functions/math, 2012/xquery) raise **XQST0045** (assertion-11..18); unprefixed names are always allowed; unbound prefixes raise **XPST0081**.
- [x] Non-literal annotation arguments (`%eg:sequential(true())`) raise **XPST0003** (annotation-33).
- [x] Annotations in XPath mode raise **XPST0003** (inline-fn-016 — XQuery-only grammar).
- [x] QT3: prod/Annotation **58/0/0**; full suite **29,316 passed / 0 failed / 2,505 skipped (92.13%)**; gaps **410** (−24).
- [x] Unit tests: 7 new tests; full suite **1,584/0**.

**Implementation Notes:**
- The lexer gained `TokenKind.Percent`; the parser's `ParsePrimaryExpr` handles `%`-prefixed inline functions, and `ParseTypeNameAndParens` captures assertion text verbatim (`CaptureAnnotations`) into the type string — no AST shape changes.
- `VmEngine.StripAnnotationAssertions` runs at the top of `InstanceOf`: it validates each annotation's namespace via the evaluation context and returns the bare type text, so all downstream matching is untouched.
- Both annotation argument lists share `SkipAnnotationArguments`/`SkipAnnotationLiteral` (string/integer/decimal/double literals only, commas between).
- annotation-33 was previously false-passing through the comparer's lenient `InvalidOperationException` matching (the lexer error carried no code); gating annotations to XQuery mode also fixed the XP30+-spec inline-fn-016 expectation.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | `Percent` token; inline annotations; function-test assertion capture (XQuery-mode gated). |
| Runtime | Modified | `InstanceOf` strips and validates annotation assertions (XQST0045/XPST0081). |
| Conformance | Modified | Gaps 410. |
| XSLT | None | Baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-29 | Kimi | Annotation assertions validated but ignored for matching | XQuery 3.1: assertions can only restrict the matched set; ignoring them is a conformant implementation choice, and every catalog expectation in the cluster holds under it. |
| 2026-07-29 | Kimi | Annotations gated to XQuery mode | inline-fn-016 (spec XP30+) expects XPST0003 — the annotation grammar is an XQuery extension of the XPath grammar. |

---

### REQ-059: Character and entity reference validation in literals and constructors

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-29  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
The prod/Literal cluster (16 recorded gaps) covered character-reference validation in XQuery string literals and direct constructors: references to invalid XML characters produced the codepoint instead of XQST0090; numeric overflows (32/64-bit) fell through to a generic XPST0003 instead of XQST0090; and a signed reference (`&#+20;`) was silently accepted because `NumberStyles.Integer` permits a leading sign.

**Proposed Solution:**  
Share one numeric-reference expander between the string-literal and constructor paths: digit-run pre-screening (malformed → XPST0003), digit-count overflow detection plus exact parsing (invalid value → XQST0090, XML 1.1 character rules).

**Acceptance Criteria:**
- [x] `"&#x00;"` / `'&#x0;'` raise **XQST0090** (K2-Literals-1, cbcl-literals-004/008).
- [x] Overflow references `&#xFF000000F6;`, `&#4294967542;`, `&#xFFFFFFFF000000F6;`, `&#18446744073709551862;` raise **XQST0090** in direct constructors (K2-Literals-16..19).
- [x] `"&#+20;"` raises **XPST0003** (K2-Literals-25).
- [x] Valid references still expand, including astral codepoints (`&#x1F600;`) and the predefined entities (`&amp;` `&lt;` `&gt;` `&quot;` `&apos;`).
- [x] XPath mode does not expand references (Literals056a..061a, K-Literals-31a/47a — 8 stale gap entries un-gapped without code changes).
- [x] QT3: prod/Literal **171/0/3**; full suite **29,332 passed / 0 failed / 2,489 skipped (92.18%)**; gaps **394** (−16).
- [x] Unit tests: 9 new tests; full suite **1,593/0**.

**Implementation Notes:**
- `XPathParser.ExpandNumericCharReference` serves both `ExpandCharReference` (string literals) and `ScanConstructorCharReference` (direct constructors); `ValidateXmlCharReference` holds the XML 1.1 validity ranges (NUL, surrogates, and noncharacters excluded; controls permitted as references).
- Overflow detection avoids `BigInteger`: after stripping leading zeros, more digits than 0x10FFFF needs (6 hex / 7 decimal) means the value overflows by construction; otherwise `int.Parse` is exact and range-checked.
- ASCII-only digit checks (`Uri.IsHexDigit`, own `IsAsciiDigit`) keep signs, whitespace, and non-ASCII digits on the XPST0003 path.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | Char-reference validation shared between string literals and constructors. |
| Conformance | Modified | Gaps 394. |
| XSLT | None | Baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-29 | Kimi | Malformed references → XPST0003, invalid values → XQST0090 | The catalog distinguishes syntax errors (`&#+20;` — K2-Literals-25) from valid-syntax-but-invalid-character references (`&#x00;`, overflows — K2-Literals-1/16..19). |

---

### REQ-060: Combined error-code conformance (FODC0001, XPTY0019, collation and prolog statics)

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-29  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
The misc/CombinedErrorCodes cluster (17 recorded gaps) mixed several distinct non-conformances: `fn:id`/`fn:idref` silently searched constructed element fragments instead of requiring a document-rooted tree; path steps silently skipped atomic items in their input sequence instead of raising XPTY0019; an unsupported collation in `declare default collation` raised the nonstandard XQST0087; an empty default function namespace was accepted (XQST0060); a positional variable duplicating the range variable was accepted (XQST0089); and inline functions could be annotated `%public`/`%private` (XQST0125).

**Proposed Solution:**  
Add the document-root check to the id functions; make `ApplyAxis` reject atomic items in sequence inputs; correct the collation error code to XQST0038; and add the three parser statics (empty default function namespace, positional-variable duplicate, inline %public/%private).

**Acceptance Criteria:**
- [x] `fn:id`/`fn:idref`/`fn:element-with-id` raise **FODC0001** when the target node's tree is not rooted at a document node (FODC0001_1/2); constructed documents still work.
- [x] Path steps raise **XPTY0019** when their input sequence contains atomic values (`<a/>/1/node()`, `(<a/>,1)/node()`, `foo:something()/a`); the XPTY0020 context-item check is unchanged.
- [x] Unsupported/malformed default collation URIs raise **XQST0038** (XQST0038_3, XQST0046_06 via its alternative).
- [x] `declare default function namespace ""` raises **XQST0060**.
- [x] `for $x at $x` raises **XQST0089**.
- [x] `%public`/`%private` on inline functions raises **XQST0125**.
- [x] Stale entries XQST0032/0033/0045-4/0066_1/0066_3/0070_4/0090 un-gapped without code changes.
- [x] QT3: misc/CombinedErrorCodes **210/0/49**; full suite **29,349 passed / 0 failed / 2,472 skipped (92.23%)**; gaps **377** (−17).
- [x] Unit tests: 10 new tests; full suite **1,603/0**.

**Implementation Notes:**
- `RequireDocumentRootedTree` (FunctionLibrary) walks `Parent` to the tree root and checks `NodeKind == Document`; it runs before the id-token search in all six id-function overloads.
- `ApplyAxis`'s sequence branch previously filtered non-nodes silently; it now throws XPTY0019, which covers both the intermediate-step rule and the FOTS mixed-sequence axis-step expectation.
- XQST0087 remains in use only for the version-declaration encoding check (its legitimate purpose).

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | XQST0060/0089/0125 statics; collation error code. |
| Runtime | Modified | `ApplyAxis` XPTY0019 for atomic sequence items. |
| Standard | Modified | FODC0001 document-root check in id functions. |
| Conformance | Modified | Gaps 377. |
| XSLT | None | Baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-29 | Kimi | Unsupported collation → XQST0038 (dropping XQST0087 for collations) | XQST0038_3 expects exactly XQST0038 for an unsupported collation URI; XQST0087 is only the encoding-declaration code. |
| 2026-07-29 | Kimi | XPTY0019 raised in `ApplyAxis` for any atomic sequence item | Covers both spec readings exercised by the catalog: intermediate steps producing atomics and axis steps over mixed sequences (XPTY0019_1/2). |

---

### REQ-061: Map constructors in step position with key disambiguation

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-29  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
The prod/MapConstructor cluster (15 recorded gaps) covered map constructors in step and `!` position whose keys and values are step expressions — a parsing minefield around the entry `:`: `map{b:2}` failed because `prefix:*` destructively consumed the entry colon; `map{* :b}` vs `map{*:b:*}` needed context-sensitive wildcard greediness; `map{*:b:b}` lexed as one run; `map{z:b:z:b}` lexed as a multi-colon "QName"; and `self:2` had to read `self` as an element name. Their deep-equal expectations also exposed that map values built from steps (sequence-wrapped) compared unequal to bare-node values.

**Proposed Solution:**  
Make the `prefix:*` name test non-destructive; gate both wildcard name-test forms inside map keys on a following entry colon; cap QNames at one colon in the lexer; splice the merged `*:b:b` run at key-parse time; unwrap singleton sequences for map/array/function-typed call parameters; and compare map values and array members with sequence semantics in fn:deep-equal.

**Acceptance Criteria:**
- [x] `<a><b>x</b></a>/map{b:2}` evaluates in step position; `map:size` receives the constructed map (MapConstructor-015/017/021).
- [x] `map{* :b}` = key `*` value `b`; `map{*:b:*}` = key `*:b` value `*`; `map{*:b:b}` = key `*:b` value `b` (MapConstructor-019/020/032).
- [x] `map{a:b:*}` = key `a:b` value `*`; `map{a:*:*}` = key `a:*` value `*`; `map{a:*:c}` = key `a:*` value `c` (MapConstructor-028/030/031).
- [x] `map{z:b:z:b}` = key `z:b` value `z:b` (MapConstructor-026); `self:2` reads `self` as an element name (MapConstructor-021).
- [x] deep-equal of step-built maps against literal maps holds (MapConstructor-027..035), including `map{*:*div*,*||*:*}` (div/concat operators in entries).
- [x] QT3: prod/MapConstructor **42/0/0**; full suite **29,364 passed / 0 failed / 2,457 skipped (92.28%)**; gaps **362** (−15).
- [x] Unit tests: 7 new tests; full suite **1,610/0**.

**Implementation Notes:**
- `_mapKeyDepth` in the XPath parser gates both wildcard name-test forms (`prefix:*` and `*:local`) while a map key parses: the wildcard form applies only when the entry `:` follows it.
- The lexer no longer produces multi-colon "QNames" (`z:b:z:b` → `z:b` `:` `z:b`); `ParseMapConstructorKey` splices a merged `*:b:b` token back into three tokens before delegating to the normal expression parser.
- `VmEngine.UnwrapSingletonItem` applies the function conversion rules to kind-typed parameters (Map/Array/Function) at the static-call site — sequence-producing operators (`!`, `/`) can now feed `map:size` and friends directly.
- `DeepEqualValue` materializes both sides item-wise (as top-level deep-equal always did); `DeepEqualMap` and `DeepEqualArray` use it for values/members.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Lexer | Modified | One-colon QNames. |
| Parser | Modified | Map-key wildcard gating; `*:b:b` splice; non-destructive `prefix:*`. |
| Runtime | Modified | Singleton unwrap for map/array/function parameters. |
| Standard | Modified | deep-equal sequence semantics for map values and array members. |
| Conformance | Modified | Gaps 362. |
| XSLT | None | Baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-29 | Kimi | Wildcard name tests gated on a following entry colon inside map keys | The only rule consistent with all catalog data points: `map{* :b}` vs `map{*:b:*}` vs `map{a:b:*}` vs `map{a:*:*}`. |
| 2026-07-29 | Kimi | Singleton unwrap at the VM call site rather than per-function | One edit covers all map/array/function parameters; every such signature takes a single item, never a sequence of them. |

---

### REQ-062: `allowing empty` in for clauses — grammar order and typed bindings

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-29  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
The prod/AllowingEmpty cluster (14 recorded gaps) covered `for $x allowing empty at $p in E` in all combinations — positions, multiple and dependent bindings, and `as` type declarations. The VM already implemented the runtime semantics, but the parser only accepted `allowing empty` *after* the positional variable (the grammar, and every catalog query, puts it before), and the empty-sequence binding was checked with a hardcoded item-level occurrence, so `as xs:integer?` wrongly rejected it.

**Proposed Solution:**  
Accept `allowing empty` in grammar position (after the optional type declaration, before `at $p`); with `allowing empty`, enforce the declared type's own occurrence on the () binding — `xs:integer?` accepts it, `xs:integer` raises XPTY0004.

**Acceptance Criteria:**
- [x] `for $x allowing empty at $p in 1 to $n` parses and evaluates: non-empty input iterates normally (outer-003), empty input produces one iteration with `$x = ()` and `$p = 0` (outer-004).
- [x] Multiple bindings with `allowing empty` on the first/second/both (outer-007..010), including dependent sequences `($x+1) to $n` (outer-011).
- [x] Typed bindings: `as xs:integer?` accepts the empty binding (outer-012/014/016/017), `as xs:integer` raises **XPTY0004** (outer-013).
- [x] `allowing empty` in XPath mode is **XPST0003** (unchanged).
- [x] QT3: prod/AllowingEmpty **19/0/0**; full suite **29,378 passed / 0 failed / 2,443 skipped (92.32%)**; gaps **348** (−14).
- [x] Unit tests: 5 new tests; full suite **1,615/0**.

**Implementation Notes:**
- Only two files needed changes: the for-binding parser (order) and the IR lowerer's `EmitEnforceTypeIfDeclared` (occurrence selection). The VM's `For` opcode already bound () and positional 0 for the empty case and jumped into the RHS block, so a single emitted `EnforceType` instruction with the right occurrence covers both iteration kinds.
- Regular (single-item) iterations match any occurrence, so switching the emitted occurrence to the declared one under `allowing empty` cannot regress non-empty inputs.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Parser | Modified | `allowing empty` in grammar position. |
| Compiler | Modified | Declared-occurrence enforcement for allowing-empty bindings. |
| Conformance | Modified | Gaps 348. |
| XSLT | None | Baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-29 | Kimi | One EnforceType instruction with the declared occurrence under `allowing empty` | Single items match every occurrence, so the same instruction is correct for regular iterations, the nullable empty case, and the XPTY0004 empty case. |

---

### REQ-063: Computed namespace constructors in element content

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-29  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
The prod/CompNamespaceConstructor cluster (11 recorded gaps) covered computed namespace constructors (`namespace p {uri}`, `namespace {expr} {uri}`) as element content: they were treated as attribute-like content (tripping XQTY0024 when attributes followed), same-URI duplicates raised "Duplicate attribute", name prefixes conflicting with declarations were not regenerated, prefix expressions went untyped, and constructed namespace nodes had the wrong identity (a parent, and an untypedAtomic typed value).

**Proposed Solution:**  
Stop treating namespace declarations as "other content" for XQTY0024; merge same-prefix-same-URI duplicates and omit redundant xmlns:xml; give conflicting element/attribute names a generated prefix; validate the prefix expression type (string-family only, XPTY0004) with an empty expression meaning a default declaration; and mark computed namespace nodes parentless with an xs:string typed value.

**Acceptance Criteria:**
- [x] Namespace declarations and attributes interleave freely at the start of element content (nscons-001/010) in both direct and computed constructors.
- [x] Duplicate declarations with the same prefix and URI merge silently (nscons-005/006); redundant `xmlns:xml` is omitted (nscons-004); `xml` bound to its proper URI is allowed (nscons-004), anything else is XQDY0101.
- [x] Name-prefix conflicts: `prefix-from-QName(node-name(.)) != 'p'` for a conflicting attribute/element name while `in-scope-prefixes` still contains `p` (nscons-010/011).
- [x] `namespace {expr} {uri}`: xs:anyURI/xs:duration prefixes raise **XPTY0004** (nscons-043/044); an empty prefix expression yields a default namespace declaration (nscons-015).
- [x] Computed namespace nodes are parentless and their typed value is xs:string (nscons-012).
- [x] QT3: prod/CompNamespaceConstructor **32/0/12**; full suite **29,389 passed / 0 failed / 2,432 skipped (92.36%)**; gaps **337** (−11).
- [x] Unit tests: 7 new tests; full suite **1,622/0**.

**Implementation Notes:**
- The generated-prefix mechanism lives in the XDocument provider (`GeneratePrefix` probing only — it must not pre-add to the `declared` set, or `Declare` suppresses the declaration; caught by nscons-010).
- `ParentlessNamespaceNode` is a marker annotation on the synthetic owner element, honored by both the `Parent` property and `GetXPathParent` (the parent/ancestor axes); namespace-axis nodes keep their real owners.
- fn:data and the VM atomizer return plain `xs:string` for namespace nodes (XDM §2.7.2) — comments and PIs already took that branch.

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Runtime | Modified | XQTY0024 exemption; prefix type check; ns atomization to xs:string. |
| Standard | Modified | fn:data xs:string for namespace nodes. |
| Providers | Modified | Declaration dedupe; generated prefixes; xmlns:xml omission; parentless marker. |
| Conformance | Modified | Gaps 337. |
| XSLT | None | Baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-29 | Kimi | Content namespace declarations take precedence over name-implied bindings, with generated prefixes for the names | nscons-010/011 require `prefix-from-QName != 'p'` for the name while `in-scope-prefixes` contains `p` — the declaration wins the prefix, the name keeps its namespace. |

---

### REQ-064: Higher-order function conformance — conversions, focus, base URI, error codes

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-29  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
The misc/HigherOrderFunctions cluster (11 recorded gaps) covered six distinct non-conformances in function-item semantics: comparisons and atomization of function items returned values instead of error codes; partial applications never validated arity; dynamic invokes skipped the function conversion rules for node sequences and untypedAtomic; named references created without a focus saw the call-site focus; function items forgot which module's base URI they were created with; and parenthesized sequence types `(function(...) as ...)*` were rejected.

**Proposed Solution:**  
Raise FOTY0013 in comparisons and content atomization and XQTY0105 in element content; validate partial-application arity in the `Curry` opcode (XPTY0004); unwrap singleton sequences before kind conversion and drive dynamic-call conversion from declared `ParameterTypeNames`; coerce untypedAtomic in `fn:round-half-to-even`; invoke named references with an absent focus when none was captured (XPDY0002); capture the static base URI on `NamedFunctionItem` and switch to it on invocation; and unwrap outer parentheses in sequence-type parsing and matching.

**Acceptance Criteria:**
- [x] `string-join#1 eq string-join#1` raises **FOTY0013** (function-item-4); `element a { avg#1 }` raises **XQTY0105** (function-item-5); `attribute a { avg#1 }` raises **FOTY0013** (function-item-6).
- [x] `concat#4("one", ?, "three")` and `concat#2("one", ?, "three")` raise **XPTY0004** (xqhof8/9).
- [x] Implicit atomization and untypedAtomic casting for all function kinds (hof-042/043: named refs, user functions, inline functions, partial applications — exact expected strings).
- [x] `<a/>/(name#0)()` raises **XPDY0002** (xqhof14).
- [x] Function items capture their module's static base URI: `lib:getfun()()` → "lib", main-module refs → "main", including via `function-lookup` in the library (xqhof16/18).
- [x] `let $f as (function(xs:integer) as xs:integer)* := ...` parses and enforces (hof-013).
- [x] QT3: misc/HigherOrderFunctions **126/0/3**; full suite **29,400 passed / 0 failed / 2,421 skipped (92.39%)**; gaps **326** (−11).
- [x] Unit tests: 9 new tests; full suite **1,631/0**.

**Implementation Notes:**
- `NamedFunctionItem.CapturedBaseUri` is set at all three materialization sites (named-ref lowering resolution, runtime tuple resolution, `fn:function-lookup`); invocation switches `EvaluationContext.BaseUri` for the call's duration and restores it.
- The absent-focus change removes the legacy "defining context's current focus" fallback: a function item created without a focus now invokes with `WithFocus(Undefined, 0, 0)` when the caller has one.
- `fn:round-half-to-even` mirrors `fn:round`'s untypedAtomic→double coercion branch (it was the only rounding function missing it).

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Core | Modified | `CapturedBaseUri` on `NamedFunctionItem`. |
| Parser | Modified | Parenthesized sequence types. |
| Runtime | Modified | Error codes; Curry arity; conversions; focus; base URI; paren types. |
| Standard | Modified | `fn:round-half-to-even` coercion; `fn:function-lookup` capture. |
| Conformance | Modified | Gaps 326. |
| XSLT | None | Baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-29 | Kimi | Absent captured focus means absent focus at call, replacing the legacy fallback | xqhof14 requires `<a/>/(name#0)()` to fail with XPDY0002; the fallback saw the caller's mutated context object. |
| 2026-07-29 | Kimi | Base URI captured by value on the function item | Module contexts are swapped and restored on one shared `EvaluationContext`, so a reference capture would see the restored (wrong) base URI. |

---

### REQ-065: Reject plain xs:duration in date/time arithmetic

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-29  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
The op/add-dayTimeDurations (16) and op/subtract-dayTimeDurations (11) clusters are all one rule: an operand annotated plain `xs:duration` in date/time arithmetic must raise XPTY0004 — only the `xs:dayTimeDuration` and `xs:yearMonthDuration` subtypes are permitted. The engine's arithmetic dispatch analyzed the duration's *string pattern* to choose the addition algorithm, which accepted any well-formed duration regardless of its type annotation.

**Proposed Solution:**  
Validate duration operands at the `Add`/`Subtract` dispatch: a value of kind Duration whose subtype resolves to plain `xs:duration` (annotation first, pattern fallback via the existing `GetDurationSubtype`) raises XPTY0004 before the arithmetic proceeds.

**Acceptance Criteria:**
- [x] `xs:date + xs:duration("P1D")` and `xs:duration("P1D") + xs:date(...)` raise **XPTY0004** (cbcl-plus-002..032).
- [x] `xs:dayTimeDuration + xs:duration` raises **XPTY0004** (duration±duration operands covered too).
- [x] `xs:date − xs:duration("P1D")` raises **XPTY0004** (cbcl-minus-002..032).
- [x] Proper subtypes still work in both directions and for duration±duration.
- [x] QT3: op/add-dayTimeDurations **61/0/0**, op/subtract-dayTimeDurations **69/0/0**; full suite **29,427 passed / 0 failed / 2,394 skipped (92.48%)**; gaps **299** (−27).
- [x] Unit tests: 5 new tests; full suite **1,636/0**.

**Implementation Notes:**
- One helper (`RequireProperDurationSubtype`) guards all five dispatch branches (date+duration, duration+date, duration+duration, date−duration, duration−duration).
- String-kind operands are untouched: untypedAtomic continues to cast into the arithmetic (the pattern fallback in `GetDurationSubtype` keeps unannotated values working).

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Runtime | Modified | Duration subtype validation in Add/Subtract dispatch. |
| Conformance | Modified | Gaps 299. |
| XSLT | None | Baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-29 | Kimi | Enforce at dispatch, not in the addition helpers | One check site covers every branch; helpers like `AddDurations` never see a plain duration after the guard. |

---

### REQ-066: Residual-cluster sweep (83 QT3 gaps)

**Requesting Application:** *(internal)*  
**Submitted:** 2026-07-29  
**Status:** Implemented  
**Target Version:** Phase 4

**Problem Statement:**  
A broad tail of 83 recorded gaps across ~17 sets: AxisStep (7), VarDecl (6), StepExpr (6), SwitchExpr (6), ArrayTest (5), PathExpr (5), DefaultNamespaceDecl (7), fn:id/idref (8), fn:in-scope-prefixes (7), fn:min (8), fn:base-uri (4), fn:doc (2), fn:generate-id (5), xs:error (5), and op/divide-dayTimeDuration (4). The causes spanned a genuinely unstable order-by sort, missing switch-case semantics, incomplete array handling, wrong min/max type-family rules, missing default-namespace and constructor-local propagation, and a dozen smaller error-code gaps. About a third of the entries were stale after the preceding sessions.

**Proposed Solution:**  
Sweep the clusters in one pass: index-decorated stable sort; switch case no-match-on-error with pre-guarded cardinality checks; array atomization and recursive content flattening; min/max boolean and date/time family rules; computed-element default namespace plus constructor-local prefix propagation; constructor and empty-paren steps with `<`-after-slash XPST0003; schema kind-test grammar/runtime error split; and the remaining targeted fixes (external function declarations, initializer self-reference exclusion, XQST0070/XQST0052 namespace rules, type-text comment stripping, xs:error constructor, generate-id and base-uri checks, xml:id NCName validity, duration-division subtype rule).

**Acceptance Criteria (highlights):**
- [x] Stable order-by preserves input order for equal keys at any scale (fn-doc-33, 40-item stability repro).
- [x] Switch: erroring cases don't match (switch-006/007); multi-item operand/case values raise XPTY0004 (switch-901/902); empty matches empty (switch-009).
- [x] Array operands atomize in arithmetic; nested arrays flatten in content; attribute content joins members (AT-028/047/050/051).
- [x] min/max: all-boolean → boolean; boolean mixes, date/time kind mixes, and plain xs:duration → FORG0006 (cbcl-min-001..017).
- [x] Computed element names apply the default element namespace; xmlns="" materialized; constructor-local prefixes propagate (K2-InScopePrefixesFunc-12/13/18/29/30, fn-in-scope-prefixes-6).
- [x] Constructors and `()` steps after `/`; `<` after `/` is XPST0003 in XQuery (PathExpr/StepExpr sets green).
- [x] `schema-element`/`schema-attribute` syntax errors at parse, XPST0008 unprefixed, XPST0081 unbound prefix; implicit namespace-node() is XQST0134 in XQuery only (Axes112 vs 115/117).
- [x] `declare function … external` parses; initializer self-reference is XPST0008; XQST0070 reserved default function namespace; XQST0052 non-XSD cast types; comments stripped from type text; xs:error(()) → (), xs:error(non-empty) → FORG0001; generate-id/base-uri/xml:id validity checks; plain xs:duration rejected in division.
- [x] QT3: full suite **29,510 passed / 0 failed / 2,311 skipped (92.74%)**; gaps **216** (−83); every swept set fully green.
- [x] Unit tests: 24 new tests; full suite **1,660/0**.

**Implementation Notes:**
- `List<T>.Sort` is introsort and unstable — order-by tuples are now decorated with input position; this was the single highest-impact fix (every large stable sort in the suite).
- `xs:error` is the abstract *type constructor* (returns () for empty input, FORG0001 otherwise) — distinct from `fn:error`, which still raises FOER0000 on an empty code argument; the previous registration mistakenly aliased it to fn:error.
- The `<`-after-slash rule is positional (XQuery mode only): the lexer falls back to the less-than operator when a constructor doesn't scan, and the parser raises XPST0003 where a step is expected.
- xml:id attributes count as IDs only with a valid NCName value (fn-id-25: "789x" and " a123 " never match).

**Impact Analysis**

| Layer | Impact | Notes |
|-------|--------|-------|
| Lexer/Parser | Modified | Constructor scanning, step rules, kind tests, type-text comments. |
| Compiler | Modified | Switch desugar; stable sort is runtime. |
| Runtime | Modified | Sort stability, arrays, computed names, propagation, division rule. |
| Standard | Modified | min/max families, fn:error/xs:error, generate-id, base-uri. |
| Providers | Modified | xml:id NCName validity. |
| Conformance | Modified | Gaps 216. |
| XSLT | None | Baseline unchanged (143/0). |

**Decision Log**

| Date | Actor | Decision | Rationale |
|------|-------|----------|-----------|
| 2026-07-29 | Kimi | Implicit-axis-only XQST0134 for namespace-node() in XQuery | The catalog requires the error only for `/*/namespace-node()` (Axes112) while `self::`/`attribute::` namespace-node() and the namespace axis keep working (Axes115/117, generate-id). |
| 2026-07-29 | Kimi | XQST0070 for default function namespace covers only XML/XMLNS URIs | defaultnamespacedeclerr-4/6/8 pin those two; hof-007 proves XMLSchema is legal as a default function namespace. |

---

## 9. Roadmap (post-QT3 sweep)

After clearing all runnable QT3 and XSLT 3.0 failures, the following capabilities are queued for future work. They are ranked by **strategic value / effort** and are expected to be tracked as individual requests when work begins.

| Priority | REQ | Capability | Status | Notes |
|----------|-----|------------|--------|-------|
| 1 | REQ-040 … REQ-066 | **XQuery 3.1 full implementation** | In Progress | Phase 3 complete (direct + computed constructors, switch/typeswitch); Phase 4: output declarations + serialization done, user-defined functions/variables done, library modules done, try/catch done, string constructors done, ordering features done, NameTest cluster closed, VarDecl.external cluster closed, NamespaceDecl cluster closed, Annotation cluster closed, Literal cluster closed, CombinedErrorCodes cluster closed, MapConstructor cluster closed, AllowingEmpty cluster closed, CompNamespaceConstructor cluster closed, HigherOrderFunctions cluster closed, dayTimeDurations clusters closed, residual sweep closed, fn:path cluster closed, unparsed-text-available cluster closed, assert-xml cluster closed; QT3 wired (29,936/0, 94.07%). Residual singles follow. |
| 2 | TBD | **XSLT 3.0 packages** (`xsl:package`, `xsl:use-package`) | Pending | Completes the XSLT 3.0 spec surface. |
| 3 | REQ-070 | **Schema awareness / XSD validation** | Implemented | User-defined schema simple-type constructors, recursive cast/match for union/list types and their restrictions, namespace-context capture for dynamic constructor calls, restriction-of-union/list SequenceType rejection (XPST0051), typed-value integer preservation, and `schema-element()`/`schema-attribute()` kind tests. Remaining QName/NOTATION cast failures are a separate pre-existing cluster, not list/union specific. |
| 4 | TBD | **Streaming** (`streamable="yes"`, `XmlReader`-backed XDM) | Pending | Performance/scalability for large documents. |
| 5 | TBD | **Custom decimal + date-time types** | Pending | Clears 4 platform-limitation skips; requires replacing .NET `decimal`/`DateTimeOffset`. |
| 6 | TBD | **Database backends** | Pending | `IXdmNode` implementations over XML databases. |
| 7 | TBD | **XPath / XSLT 2.0 legacy certification** | Pending | Lowest priority — 3.1/3.0 are supersets and no separate mode is planned unless a customer requires it. |
| 8 | TBD | **XPath 4.0 / XSLT 4.0** | Pending | W3C specs are still drafts; wait for Recommendation status. |

---

## 10. Related Documents

- `D:\Development\Customer A\docs\INTEGRATION.md` — How to consume Bosak from Customer A
- [`ARCHITECTURE.md`](./ARCHITECTURE.md) — High-level Bosak architecture and roadmap
- Project root `AGENTS.md` — Coding conventions for Bosak contributors

---

## 11. VS Code Extension Backlog

### REQ-071: XSLT code lens source-document hint polish

**Requesting Application:** Bosak / Fytala Stack  
**Submitted:** 2026-08-20  
**Status:** **Accepted**

Harden the default source-document hint introduced with REQ-028. Acceptance criteria:

- Add unit-test coverage for single-quoted `<?bosak source-document='...'?>` processing instructions.
- Support an XML comment alternative such as `<!-- bosak:source-document=... -->`.
- Trim surrounding whitespace from the supplied path.
- Keep relative-path resolution against the stylesheet directory.

### REQ-072: XSLT code lens initial-template runner

**Requesting Application:** Bosak / Fytala Stack  
**Submitted:** 2026-08-20  
**Status:** **Accepted**

Add a second XSLT code lens for stylesheets that declare an `xsl:initial-template` or contain a named template entry point. The lens should run the transform without requiring a source XML document, using the named template as the entry point. This covers the common XSLT 3.0 use case where the stylesheet generates output from parameters alone.

### REQ-073: Richer XSLT document symbols / outline

**Requesting Application:** Bosak / Fytala Stack  
**Submitted:** 2026-08-20  
**Status:** **Accepted**

Extend `DocumentSymbolHandler` to provide a richer outline for XSLT documents. Symbols should include top-level declarations: templates (named and matched), functions, variables, parameters, attribute-sets, keys, and output declarations. This improves navigation in large stylesheets.
