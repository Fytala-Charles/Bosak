# Bosak Cross-Application Feature Requests

> **Living Registry** — Last updated: 2026-05-27  
> This document tracks feature requests originating from applications consuming the Bosak XPath / XSLT stack. It serves as the single source of truth for cross-cutting capabilities that multiple consumers need.

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
| REQ-008 | *(internal)* | `fn:function-lookup` double-to-string precision | Precision mismatches in numeric serialization | **Pending** | TBD | Unassigned | 2026-05-24 |
| REQ-009 | *(internal)* | Date/time ordering (`lt`, `gt`, `le`, `ge`) | 9 remaining QT3 failures; only equality works today | **Pending** | TBD | Unassigned | 2026-05-24 |
| REQ-010 | *(internal)* | `json-to-xml`, `parse-json`, `xml-to-json` | Standard XPath 3.1 JSON functions missing | **Pending** | TBD | Unassigned | 2026-05-24 |
| REQ-011 | *(internal)* | `fn:transform()` function | XPath-level XSLT invocation per spec | **Pending** | Phase 3 | Unassigned | 2026-05-24 |
| REQ-012 | Customer A | `xsl:call-template` tunnel parameters | Customer A passes context metadata through deep call chains | **Implemented** | TBD | Charles Korthout | 2026-05-24 |
| REQ-014 | Customer B | XML Schema (XSD) validation API | Customer B needs to validate Infor OAGIS BODs against XSDs before dispatching to handlers | **Accepted** | Phase 2 | Unassigned | 2026-05-25 |
| REQ-015 | Customer A | `xsl:function` support | Customer A defines 22+ helper functions (date, week, mapping) in shared fragments; cannot execute without this | **Implemented** | Phase 2 | Charles Korthout | 2026-05-26 |
| REQ-016 | Customer A | Multi-key `xsl:sort` (primary + secondary) | Customer A D99A JAMA basesheet sorts by item ID then ship-to; current implementation only handles first key | **Pending** | Phase 2 | Unassigned | 2026-05-26 |

> **Legend:**
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
**Status:** Pending

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
- [ ] `<xsl:key name="products" match="product" use="@code"/>` is parsed and indexed
- [ ] `key('products', $code)` returns the matching node(s)
- [ ] Index is rebuilt per source document (not shared across transforms)
- [ ] Works inside `xsl:for-each`, `xsl:if`, and `xsl:value-of select`

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
**Status:** Pending

#### Problem Statement
Customer A produces XML that is consumed by external systems (Infor, EDI gateways, customer APIs). These systems often have strict formatting requirements: UTF-8 encoding, no XML declaration, indented for debugging, or compact for size. Today Bosak always serializes with default `XDocument` settings.

#### Proposed Solution
1. Parse `xsl:output` attributes (`method`, `encoding`, `indent`, `omit-xml-declaration`, `standalone`, `version`).
2. Pass output properties to `ResultTreeSerializer`.
3. Support `method="xml"` first; `method="text"` and `method="html"` as stretch goals.

#### Acceptance Criteria
- [ ] `<xsl:output method="xml" encoding="UTF-8" indent="yes"/>` produces indented XML
- [ ] `<xsl:output omit-xml-declaration="yes"/>` suppresses `<?xml …?>`
- [ ] `method="text"` serializes only text nodes (no markup)
- [ ] Invalid combinations are ignored gracefully (not fatal)

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
**Status:** Pending

#### Problem Statement
`fn:function-lookup` returns function items that, when applied to doubles, produce strings with precision mismatches vs. the W3C expected output. This is likely a serialization issue in how `XdmValue.ToString()` handles `xs:double`.

#### Proposed Solution
Audit `XdmValue.ToString()` for double values and align with XPath 3.1 serialization rules (IEEE 754 shortest representation, or `xs:string()` cast semantics).

#### Acceptance Criteria
- [ ] All `function-lookup` QT3 tests pass
- [ ] `xs:string(1.0e0)` → `"1"` (not `"1.0"` or `"1E0"`)
- [ ] Edge cases (`NaN`, `INF`, `-INF`, very small/large exponents) match spec

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
**Status:** Pending

#### Problem Statement
Date/time equality comparisons work after recent fixes, but ordering comparisons (`<`, `>`, `<=`, `>=`) still have 9 QT3 failures. The `VmEngine.Compare()` path needs actual comparison semantics (not just type-checking) for `xs:dateTime`, `xs:date`, `xs:time`, and `g*` types.

#### Proposed Solution
Extend `VmEngine.Compare()` to implement proper ordering for each date/time subtype:
1. Normalize to a common timeline (handle timezones, incomplete dates).
2. Apply spec rules for partial ordering (some pairs are incomparable → empty sequence in `order by`).
3. Reuse existing `DateTimeOffset` infrastructure where possible.

#### Acceptance Criteria
- [ ] All remaining `op-dateTime-less-than` etc. QT3 tests pass
- [ ] `xs:date("2024-01-01") < xs:date("2024-01-02")` → `true`
- [ ] Incomparable pairs (e.g., `xs:time` with different implicit timezones) handled per spec

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
**Status:** Pending

#### Problem Statement
XPath 3.1 mandates `json-to-xml`, `parse-json`, `xml-to-json`, and `json-doc`. These are entirely missing from Bosak, causing QT3 failures and limiting interoperability with JSON-heavy APIs.

#### Proposed Solution
Implement the functions per the W3C spec:
1. `parse-json($json-text)` → `map` or `array`
2. `json-to-xml($json-text)` → XML representation using `http://www.w3.org/2005/xpath-functions` namespace
3. `xml-to-json($xml)` → JSON string
4. `json-doc($uri)` → `parse-json(doc($uri))`

#### Acceptance Criteria
- [ ] All `json-to-xml` QT3 tests pass
- [ ] All `parse-json` QT3 tests pass
- [ ] `xml-to-json` round-trips correctly for simple cases
- [ ] Options parameter (`liberal`, `duplicates`, etc.) supported where feasible

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
**Status:** Pending

#### Problem Statement
`fn:transform($options)` is an XPath 3.1 function that invokes XSLT from within an XPath expression. This is useful for composing transforms but is entirely unimplemented.

#### Proposed Solution
Implement `fn:transform` as a delegate that:
1. Accepts a map of options (`stylesheet-location`, `source-node`, `initial-template`, etc.).
2. Compiles or reuses the referenced stylesheet.
3. Runs the transform via `TransformEngine`.
4. Returns the result as an XDM value.

#### Acceptance Criteria
- [ ] `fn:transform(map{"stylesheet-location":"foo.xsl","source-node":.})` executes
- [ ] `initial-template` option works for named-template entry points
- [ ] Parameters can be passed via `stylesheet-params`

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
**Status:** Accepted

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
- [ ] `IXsdValidator` interface defined in Bosak
- [ ] Default implementation uses `System.Xml.Schema.XmlSchemaSet`
- [ ] Supports single-schema and multi-schema (with `xs:import`/`xs:include`) validation
- [ ] Returns structured validation results (line number, column, severity, message)
- [ ] Non-throwing `TryValidate` variant available
- [ ] `BodValidator.ValidateOagis` helper loads correct XSD by namespace URI
- [ ] Unit tests cover valid BOD, invalid BOD, and schema-set validation
- [ ] Documented in Bosak integration guide

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
- [ ] Function parameters with `as="xs:string?"` are type-checked
- [ ] Function return type with `as="xs:date?"` is enforced
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
**Status:** Pending

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
- [ ] Two `xsl:sort` elements produce correctly ordered output (primary then secondary)
- [ ] Three or more `xsl:sort` elements work correctly
- [ ] Each key respects its own `data-type` and `order` attributes
- [ ] Stable sort: items with equal keys retain their original relative order

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
```

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

## 9. Related Documents

- [`BERLIN_INTEGRATION.md`](./BERLIN_INTEGRATION.md) — How to consume Bosak from Customer A
- [`ARCHITECTURE.md`](./ARCHITECTURE.md) — High-level Bosak architecture and roadmap
- Project root `AGENTS.md` — Coding conventions for Bosak contributors
