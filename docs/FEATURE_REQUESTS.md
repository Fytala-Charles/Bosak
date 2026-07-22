# Bosak Cross-Application Feature Requests

> **Living Registry** — Last updated: 2026-07-22 (XQuery 3.1 Phase 1 foundation complete: prolog-less queries compile and execute via `XQueryCompiler`; unit tests now **1,382/0**; QT3 and XSLT baselines unchanged)
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
| REQ-028 | Bosak / Fytala Stack | VS Code Language Server Extension | IDE support for XPath 3.1 and XSLT 3.0 development: syntax highlighting, realtime diagnostics, auto-completion | **Implemented** | 0.1.2 | Charles Korthout | 2026-06-08 |
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
   - `TextDocumentSyncHandler`: full-document sync for `.xpath`, `.xsl`, `.xslt`
   - `DiagnosticsHandler`: XPath parse errors; XSLT XML well-formedness + XPath-in-attribute validation (`select`, `test`, `match`, `use-when`)
   - `CompletionHandler`: XPath functions, axes, keywords; XSLT instructions
   - `DocumentManager`: in-memory store of open document contents
2. **`vscode-bosak`** — TypeScript VS Code extension client:
   - Syntax highlighting (TextMate grammars for XPath and XSLT)
   - LSP client connecting via stdio
   - Bundled server support: server binary shipped inside the VSIX
   - Context-menu commands (Evaluate XPath, Run XSLT — placeholders for future wiring)

#### Acceptance Criteria

- [x] `Bosak.LanguageServer` compiles with 0 errors, 0 warnings
- [x] VSIX packages extension + bundled server (2.71 MB)
- [x] Installable via `code --install-extension vscode-bosak-0.1.2.vsix`
- [x] Diagnostics appear for invalid XPath expressions
- [x] Diagnostics appear for malformed XSLT and invalid XPath in attributes
- [x] Completions trigger for XPath functions and XSLT instructions
- [x] All 873 unit tests still pass

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
| 2026-06-08 | Charles Korthout / Kimi | Implemented | Developer experience improvement; no production blocking impact |

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

---

## 9. Roadmap (post-QT3 sweep)

After clearing all runnable QT3 and XSLT 3.0 failures, the following capabilities are queued for future work. They are ranked by **strategic value / effort** and are expected to be tracked as individual requests when work begins.

| Priority | REQ | Capability | Status | Notes |
|----------|-----|------------|--------|-------|
| 1 | REQ-040 | **XQuery 3.1 full implementation** | In Progress | Phase 1 complete: prolog-less queries run via `XQueryCompiler`. Phase 2 (full FLWOR clauses), Phase 3 (constructors), and Phase 4 (modules/serialization) remain. |
| 2 | TBD | **XSLT 3.0 packages** (`xsl:package`, `xsl:use-package`) | Pending | Completes the XSLT 3.0 spec surface. |
| 3 | TBD | **Schema awareness / XSD validation** | Pending | Cross-cutting for XPath + XSLT; clears schema-dependent test skips. |
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
