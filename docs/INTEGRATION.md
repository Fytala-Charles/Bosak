<!-- Bosak XPath / XSLT — General Integration Guide -->
<!-- Living document: updated with each significant Bosak change. -->

# Bosak XPath / XSLT / XQuery — Integration Guide

> **Purpose:** Quick-reference for any application consuming the Bosak XPath 3.1 + XSLT + XQuery stack.
> **Last updated:** 14 July 2026
> **Bosak baseline:** 940 unit tests passed / 0 failed / 0 skipped
> **XSLT baseline:** 5,737 passed / 7 failed / 8,856 skipped (only `fn:transform` tests fail)

---

## 0. Recent Changes

- **2026-07-14** — HOF unskip + snapshot cluster: higher-order functions fully enabled; snapshot set 19/0/24; seqtor/static/regex/system-property/current-output-uri sets green.
  - `fn:snapshot` now matches the spec-equivalent stylesheet implementation (`snapshot-equivalent.xsl`) node-for-node: non-node items pass through unchanged, ancestor grafting preserves parentage, namespace declarations are excluded from attribute comparisons in `fn:deep-equal`, and in-scope namespaces are not redeclared on copied descendants.
  - Typed templates (`xsl:template/@as`) now collect results through the placeholder sequence accumulator, so node identity and parentage survive template boundaries; `xsl:element` suspends the accumulator while constructing content (fixes `__xdm_seq__` placeholder leak, `namespace-0912`).
  - Function-body results no longer clone a single text node (`NormalizeSequenceConstructorItems`), preserving text-node parentage through `xsl:function` results (`snapshot-0102a`).
  - `namespace-node()` is now a valid match pattern (priority −0.5) matching namespace-axis nodes.
  - `fn:concat` / `fn:compare#2` register their `xs:anyAtomicType?` parameters as pass-through; dynamic-call argument conversion no longer stringifies arbitrary atomics to `xs:string` (`higher-order-functions-064` raises XPTY0004 again).
  - Other fixes: `fn:min`/`fn:max` return `xs:integer` for all-integer input; `system-property()` expands `Q{uri}local` and reports `xsl:supports-higher-order-functions`; `xsl:function` accepts `cache`; user functions in map/math/array reserved namespaces raise XTSE0080; TVT/§4.3 whitespace handling; missing F&O registrations (`element-with-id#2`, `idref`, `uri-collection`, `xs:error`).
  - Full W3C suite: **5,737 passed / 7 failed / 8,856 skipped** — remaining failures are the `fn:transform` set (transform-002..009), which awaits `fn:transform` implementation.

- **2026-07-13** — Skip-pool audit: unskipped `position-0103` (xsl:merge) and `position-2201` (xsl:result-document); both pass now that the features they gate on are implemented. Full W3C suite: **5,607 passed / 0 failed / 8,993 skipped**.

- **2026-07-13** — Phase 5n: all remaining singleton failures cleared; **zero failing runnable tests** in the W3C XSLT 3.0 suite.
  - `attribute-0701`: HTML serialization now minimizes recognized boolean attributes (`checked`, `selected`, `disabled`, ...) whose value equals their name, restricted to the HTML boolean allowlist so attributes such as `ffi="ffi"` keep the explicit form.
  - `backwards-019b`: `escape-uri-attributes` now defaults to true for the XHTML method as well as HTML. The XSLT 3.0 backwards-compatibility rule is implemented via new `OutputProperties.EffectiveVersion` / `ImplicitResultTree` flags: in version-1.0 mode an implicitly generated result tree infers the `xml` (not `xhtml`) output method, while an explicit `xsl:result-document` still infers `xhtml` (`backwards-019` vs `backwards-019b`).
  - `maps-017`: the conformance harness unwraps JSON-string-serialized results (json/adaptive output of a node) before reparsing for tree assertions.
  - `merge-021`: `XTDE2210` is now also raised when a merge-key attribute (`lang`, `order`, `collation`, `case-order`, `data-type`) is present on one of two corresponding `xsl:merge-key` elements and absent on the other, per the XSLT 3.0 spec as written.
  - `include-0101`: two fixes. (1) `xsl:include`/`xsl:import` hrefs now resolve against the *element's* base URI, so modules pulled in through DTD external entities resolve nested imports relative to the entity location. (2) The unnamed `xsl:output` declarations are now merged across the module tree by import precedence via `Stylesheet.EffectiveOutputProperties` (previously only the principal module's declarations were used), so the included module's `method="html"` correctly overrides the imported module's `method="xml"`.
  - Full W3C suite: **5,605 passed / 0 failed / 8,995 skipped** (100% of runnable tests).

- **2026-07-13** — Phase 5m `select` conformance cluster cleared.
  - The conformance harness now honors the `encoding` attribute on assertion elements (`assert-serialization`, `assert-xml`) when reading expected-result files, so ISO-8859-1 expected outputs decode correctly (`select-6101`).
  - W3C `select` conformance set: **157 passed / 0 failed / 1 skipped**.
  - Full W3C suite: **5,600 passed / 5 failed / 8,995 skipped** (99.9%).
  - Remaining failures: `attribute-0701`, `backwards-019b`, `include-0101`, `maps-017`, `merge-021`.

- **2026-07-13** — Phase 5l `bug` conformance cluster cleared.
  - `ResultTreeSerializer`'s text output method no longer emits comment/PI markup — comment and processing-instruction nodes contribute nothing to `method="text"` output (`bug-1405`).
  - The conformance harness now self-closes HTML void elements (`meta`, `br`, `img`, ...) when reparsing HTML output for tree assertions, so the HTML5-mandated unclosed `<meta ...>` no longer breaks XPath assertions (`bug-1301`).
  - `assert-xml` comparisons now strip the serialization-injected `meta http-equiv="Content-Type"` element: assert-xml compares result trees, and the meta is a serialization artifact (`bug-1901`). The serializer still injects it whenever `include-content-type` is in effect, as required by `output-0123` and `backwards-018`.
  - Also clears `select-6201` (HTML table serialization).
  - W3C `bug` conformance set: **75 passed / 0 failed / 11 skipped**.
  - Full W3C suite: **5,599 passed / 6 failed / 8,995 skipped** (99.9%).
  - Remaining failures: `attribute-0701`, `backwards-019b`, `include-0101`, `maps-017`, `merge-021`, `select-6101` (since cleared).

- **2026-07-13** — Phase 5k `for-each-group` conformance cluster cleared.
  - `FunctionSignature` gains an optional `DynamicImplementation`; `VmEngine.InvokeFunctionItem` uses it for dynamic calls through function items (named references and partial application) while static calls keep using `Implementation`.
  - `TransformEngine.RegisterGroupingFunctions` now supplies dynamic implementations that raise `XTDE1061` (`current-group`), `XTDE1071` (`current-grouping-key`), `XTDE3480` (`current-merge-group`), and `XTDE3510` (`current-merge-key`), per XSLT 3.0: these context components are not retained in the closure of a function item.
  - W3C `for-each-group` conformance set: **78 passed / 0 failed / 7 skipped** (plus 114 streaming tests skipped).
  - Full W3C suite: **5,595 passed / 10 failed / 8,995 skipped** (99.8%).
  - Remaining failures: `bug` (3), `select` (2), `attribute-0701`, `include-0101`, `maps-017`, `merge-021`, `backwards-019b`.

- **2026-07-13** — Phase 5j `output` conformance cluster cleared.
  - `ResultTreeSerializer.SerializeAsXhtml` now normalizes text/attribute values before CDATA wrapping, so `cdata-section-elements` combined with `normalization-form` split unrepresentable normalized characters correctly (`output-0115d`).
  - `TransformEngine.IsRawCollectionTopLevel` is now scoped to the actual principal/secondary result document, so literal elements inside `xsl:variable/@as` bodies are no longer swallowed by raw-item collection (`output-0716`, `output-0717`).
  - `XdmJsonSerializer.SerializeValue` now handles sequence-valued array/map members, so nested HTML/XML nodes serialize as JSON strings instead of `"(sequence)"` (`output-0702`).
  - Also clears `arrays-304`.
  - W3C `output` conformance set: **232 passed / 0 failed / 29 skipped**.
  - Full W3C suite: **5,593 passed / 12 failed / 8,995 skipped** (99.8%).
  - Remaining failures: `bug` (3), `select` (2), `for-each-group` (2), `attribute` (1), and singleton regressions in `include`, `maps`, `merge`, and `backwards`.

- **2026-07-13** — Phase 5i `normalize-unicode` conformance cluster cleared.
  - `ResultTreeSerializer` now escapes tab, line-feed, and carriage-return characters inside XML attribute values as decimal numeric character references (`&#9;`, `&#10;`, `&#13;`) after `XmlWriter` emits them literally. This aligns the `xml` output method with XSLT/XQuery Serialization 3.1.
  - Also clears `copy-3801` and `attribute-1101` (attribute-value whitespace regressions).
  - W3C `normalize-unicode` conformance set: **18 passed / 0 failed / 0 skipped**.
  - Full W3C suite: **5,588 passed / 17 failed / 8,995 skipped** (99.7%).
  - Remaining clusters: `output` (4), `bug` (3), `select` (2), `for-each-group` (2), `attribute` (1), and singleton regressions in `arrays`, `backwards`, `include`, `maps`, and `merge`.

- **2026-07-13** — Phase 5h `character-map` conformance cluster cleared.
  - Adaptive output now uses XPath/XQuery string-literal escaping (double quotes) instead of JSON escaping, so character-map replacements in strings and maps are serialized correctly.
  - `fn:current-time()` now keeps the date part on day 1 of year 1 when possible and falls back to day 2 only when a positive timezone offset would underflow `DateTimeOffset.MinValue`.
  - W3C `character-map` conformance set: **29 passed / 0 failed / 0 skipped**.
  - Full W3C suite: **5,565 passed / 40 failed / 8,995 skipped** (99.3%).
  - Remaining clusters: `mode` (9), `xml-version` (7), and scattered regressions.

- **2026-07-12** — Phase 5g `inherit-namespaces="no"` conformance fix; the W3C `namespace` cluster is now clear.
  - `TransformEngine.FinalizeNamespaceInheritance` attaches `PrefixedNamespaceUndeclarations` to children of `NamespaceInheritanceBarrier` elements so `xmlns:prefix=""` is emitted where required.
  - The synthetic `__xdm_doc__` root is detached before being wrapped in the final `XDocument`, preserving namespace annotations instead of cloning them away.
  - `ResultTreeSerializer` routes trees with prefixed namespace undeclarations to the raw XML serializer.
  - W3C `namespace` conformance set: **203 passed / 0 failed / 21 skipped**.
  - Full W3C suite: **5,561 passed / 44 failed / 8,995 skipped** (99.2%).

- **2026-07-12** — Phase 5f final serialization clusters cleared.
  - `fn:current-output-uri()` now returns the base output URI at the top level and remains empty in temporary output state (functions, variables, sort/merge keys, patterns), clearing the `current-output-uri` cluster.
  - Original namespace prefixes are preserved for sibling elements that map to the same URI, clearing `output-0138`.
  - The `output` conformance cluster now has **0 failures** (203 passed / 0 failed / 29 skipped); the `current-output-uri` cluster has **0 failures** (15 passed / 0 failed / 2 skipped).
  - Full W3C suite: **5,544 passed / 61 failed / 8,995 skipped** (98.9%).

- **2026-07-12** — Phase 5e remaining `output` serialization edge cases.
  - Arrays are now flattened in sequence constructors, fixing `output-0713`–`output-0715`.
  - `json-node-output-method="html"` now injects the HTML content-type `<meta>` element (`output-0716`).
  - Adaptive output preserves `omit-xml-declaration` and applies parameter-document character maps (`output-0721`).
  - XML comments preserve `\r` characters literally (`output-0723`).
  - `SEPM0009` is restricted to XML/XHTML and now covers `version != 1.0` with `doctype-system`; `SEPM0010` validates `undeclare-prefixes` for XML 1.1.
  - Text output now writes the BOM, applies character maps, and honors `normalization-form`; HTML DOCTYPE is emitted immediately before the first element.
  - Full W3C suite: **5,538 passed / 67 failed / 8,995 skipped** (98.8%).

- **2026-07-11** — Phase 5 JSON output method: `xsl:output method="json"`, `json-node-output-method`, `allow-duplicate-names`, `escape-solidus`, and `xsl:output parameter-document` with inline character maps.
  - Top-level `xsl:map`/`xsl:map-entry` results are preserved for JSON serialization instead of being rejected as element/document children.
  - HTML node serialization inside JSON strings now emits XHTML namespace declarations and escapes solidus characters per XSLT/XQuery Serialization 3.1 defaults.
  - W3C `output` conformance set: **175 passed / 28 failed / 29 skipped** (was 168/35/29). Remaining JSON/text failures include `output-0703` (item-separator) and `output-0710`/`0711` (unescaped keys).
  - Full W3C suite: **5,477 passed / 128 failed / 8,995 skipped** (97.7%).

- **2026-07-11** — Phase 5b JSON/text output edge cases: `item-separator` and `SENR0001` validation.
  - `item-separator` is now honored for `method="text"`, fixing `output-0703`, `output-0709`, `output-0718`, and `output-0719`.
  - Maps, arrays, and functions at the top level of XML, HTML, XHTML, or text output now raise `SENR0001`, fixing `output-0710`, `output-0711`, and `output-0712`.
  - W3C `output` conformance set: **179 passed / 24 failed / 29 skipped** (was 175/28/29).
  - Full W3C suite: **5,481 passed / 124 failed / 8,995 skipped** (97.8%).

- **2026-07-11** — Phase 5c `xsl:result-document` serialization fixes.
  - AVTs are now evaluated for all `xsl:result-document` serialization attributes, including `html-version`, `byte-order-mark`, and `allow-duplicate-names`.
  - `yes`/`no` attribute values are now case-sensitive; uppercase variants raise `XTSE0020`.
  - `SEPM0009` is only raised for XML/XHTML methods that emit an XML declaration.
  - `xsl:result-document` now supports raw-item collection for `method="json"`, `method="adaptive"`, and `build-tree="no"`.
  - W3C `result-document` conformance set: **104 passed / 21 failed / 29 skipped** (was 86/39/29).
  - Full W3C suite: **5,506 passed / 99 failed / 8,995 skipped** (98.2%).

- **2026-07-11** — Phase 4 serialization fixes: named-output import precedence, XHTML 1.0 empty-element handling, DOCTYPE quote/namespace rules, and `fn:current-output-uri()` scoping.
  - W3C `output` conformance set: **168 passed / 35 failed / 29 skipped** (was 155/48/29).
  - Full W3C suite: **5,473 passed / 132 failed / 8,995 skipped** (97.6%).

- **2026-06-26** — Fixed `normalize-unicode-014`: HTML result-tree serialization now applies `xsl:output/@normalization-form` (NFC/NFD/NFKC/NFKD) to text, attribute values, comments, and processing instructions.
  - Full W3C suite: **5,238 passed / 5 failed / 9,357 skipped** (99.9%).
  - Remaining failures: `catalog-006/007`, `docbook-001/002/004`.

- **2026-06-26** — Fixed `accumulator-090`: global variables that call `accumulator-after()` no longer trigger a false `XPST0008` circular-reference error. The accumulator evaluation context now copies globals lazily but skips the variable currently being initialized, preserving access to globals referenced by accumulators (e.g., `merge-066`).
  - Full W3C suite: **5,237 passed / 6 failed / 9,357 skipped** (99.9%).
  - Remaining failures: `normalize-unicode-014`, `catalog-006/007`, `docbook-001/002/004`.

- **2026-06-26** — Fixed `function-1014` (FXSL higher-order recursion): `xsl:apply-templates` and `xsl:call-template` inside `xsl:function` bodies now expand `__xdm_seq__` placeholders so atomic values returned by `xsl:sequence` reach the function result instead of being dropped.
  - Full W3C suite: **5,236 passed / 7 failed / 9,357 skipped** (99.9%).
  - Remaining failures: `accumulator-090`, `normalize-unicode-014`, `catalog-006/007`, `docbook-001/002/004`.

- **2026-07-08** — Cleared the W3C `unparsed-text`, `match`, `forwards`, `lre`, `whitespace`, `xslt-compat`, and `for-each-group` conformance clusters.
  - `fn:unparsed-text()` one-argument form detects encoding from BOM, XML declaration, and HTTP `Content-Type`; `unparsed-text-available()` works for HTTP resources; sequence arguments are atomized.
  - `xsl:template/@match` no longer rejects `Q{uri}*` / `except` patterns as AVTs.
  - Forward-compatibility mode ignores unknown XSLT elements/attributes and unresolvable `use-when` expressions when the effective version is > 3.0.
  - Maps and functions raise `XTDE0450` when serialized directly to element content.
  - QName names in `xsl:element`/`xsl:attribute` are whitespace-normalized before validation.
  - Backwards-compatible mode converts atomic values to strings for string functions.
  - `fn:distinct-values` treats NaN as equal.
  - `xsl:function` bodies follow XSLT text-node merging rules while preserving consecutive zero-length text nodes.
  - Full W3C suite: **5,233 passed / 10 failed / 9,357 skipped** (99.8%).

- **2026-07-05** — Cleared the W3C `tunnel` conformance cluster (58 runnable tests pass; 0 failed).
  - `xsl:with-param/@tunnel` and `xsl:param/@tunnel` now accept `yes`/`no` (XSLT 2.0) and `true`/`false`/`1`/`0` (XSLT 3.0); invalid/empty values raise `XTSE0020`.
  - `xsl:call-template` enforces `XTSE0680` when an ordinary `xsl:with-param` matches a tunnel `xsl:param` (or vice versa) and when no matching parameter is declared.
  - `xsl:call-template` parameter validation now skips `xsl:context-item` children and is suppressed in XSLT 1.0 backwards-compatible mode, so extra parameters are silently ignored.
  - Tunnel parameters are correctly isolated from `xsl:function` bodies and pass through intermediate named templates, `xsl:apply-templates`, `xsl:apply-imports`, and `xsl:next-match`.
  - Tunnel parameters now bind only to tunnel-declared `xsl:param`s; non-tunnel `xsl:with-param`s no longer shadow tunnel parameters.

- **2026-07-05** — Cleared the W3C `avt` conformance cluster (35 runnable tests pass; 0 failed).
  - AVT expressions now correctly handle XPath comments (`(: ... :)`), empty expressions, escaped `}}`, and `{{` even when no `{` expression is present.
  - `xsl:attribute/@separator`, `xsl:value-of/@separator`, and `xsl:sort/@stable` are now evaluated as attribute value templates.
  - AVTs in XSLT 1.0 backwards-compatibility mode take only the first item of the expression value, matching `string()` semantics.
  - `xsl:value-of` inside `xsl:function` bodies constructs real text nodes, so functions declared `as="text()*"` return the expected node kind.
  - `xsl:template/@match` now rejects AVT syntax with `XTSE0340`.

- **2026-07-05** — Cleared the W3C `collations` conformance cluster (43 runnable tests pass; 0 failed).
  - `xsl:stylesheet`/`xsl:template`/`xsl:*` `default-collation` attributes now flow into the XPath evaluation context, so `eq`, `=`, `fn:compare`, `fn:starts-with`, `fn:contains`, `fn:ends-with`, etc. use the correct collation without an explicit argument.
  - `xsl:for-each-group` and `xsl:key` use the effective default collation when no explicit `@collation` is supplied.
  - `xsl:sort` with `case-order="upper-first"`/`"lower-first"` works even when no `@lang` or `@collation` is present (primary comparison is case-insensitive, with case as the tie-breaker).
  - Collation-aware aggregate functions (`fn:max`, `fn:min`, `fn:index-of`, `fn:distinct-values`, `fn:deep-equal`) now honor the in-scope default collation.
  - UCA collations with `fallback=no` raise `FOCH0002`, matching the implementation-defined fallback behavior expected by the test suite.

- **2026-07-04** — Cleared the W3C `iterate` conformance cluster (44 runnable tests pass; 0 failed; 35 streaming tests skipped).
  - `xsl:iterate` now works in the result-tree path with `xsl:param`, `xsl:next-iteration`, `xsl:break`, and `xsl:on-completion`.
  - `xsl:next-iteration`/`xsl:with-param` values are coerced to the declared `xsl:param` type, so atomization happens when required.
  - `xsl:on-completion` and `xsl:break` sequence-constructor content are evaluated as document-producing constructors, so nested `xsl:copy-of` inside literal elements contributes correctly.
  - `xsl:try` now rolls back output written in the try block before executing `xsl:catch`, using efficient last-node/last-attribute snapshots. This fixes `iterate-036` and prevents the `catalog` self-tests from hanging.
  - Full W3C suite: **5,073 passed / 177 failed / 9,350 skipped** (~96.6%).

- **2026-07-02** — Cleared the W3C `seqtor` conformance cluster (54 runnable tests pass; 18 skipped).
  - Sequence-constructor whitespace and empty atomic items now produce correct spacing in complex content.
  - Empty sequence items act as atomic separators; text-node/atomic merging and adjacent-text concatenation match the XSLT 3.0 serialization rules.
  - `xsl:sequence` without `@select` now returns its raw sequence-constructor content.
  - `xsl:document` inside `xsl:comment`, `xsl:processing-instruction`, and `xsl:attribute` simple content is handled correctly.
  - Namespace prefix `xs` is now declared when evaluating Text Value Templates.
  - Mixed atomics and text nodes produced by `xsl:function` are serialized correctly.
  - Full W3C suite: **4,964 passed / 286 failed / 9,350 skipped** (~94.6%).

- **2026-07-03** — Cleared the quick-win conformance clusters `available-system-properties`, `on-empty`, `copy`, and `where-populated`.
  - `fn:available-system-properties` now returns `xs:QName` values and includes all required XSLT system properties.
  - Sequence-constructor placeholders no longer count as significant content, so `xsl:on-empty` fires correctly for empty `xsl:sequence` results.
  - `xsl:where-populated` now expands sequence placeholders produced by `xsl:sequence`, preserving arrays and other sequence values.
  - Full W3C suite: **4,999 passed / 251 failed / 9,350 skipped** (~95.2%).

- **2026-06-26** — Cleared the W3C `as`, `xml-to-json`, and `json-to-xml` conformance clusters.
  - `xs:float` serialization now uses the shortest round-trip `"R"` format in the scientific range, fixing `as-0802` / `as-0802b`.
  - `fn:json-to-xml` now honors the `duplicates` option (`use-first`, `retain`, `reject`) and reports `FOJS0005` / `XPTY0004` for invalid option values.
  - `fn:codepoints-to-string` now accepts XML 1.1 C0 control characters, allowing `xml-to-json` to serialize backspace/bell/form-feed as JSON escapes.
  - Full W3C suite: **4,953 passed / 297 failed / 9,350 skipped** (~94.3%).

- **2026-06-30** — Cleared the W3C `match` conformance cluster (1 failure → 0).
  - `xsl:mode` declarations without an explicit `@on-no-match` now default to `text-only-copy` per the XSLT 3.0 spec, so atomic items processed by `xsl:apply-templates` produce their string value in the default mode.
  - The built-in rule for atomic values now respects the effective mode's `on-no-match` behavior (deep-skip and shallow-skip suppress output).
  - Full W3C suite: **4,944 passed / 306 failed / 9,350 skipped** (~94.2%).

- **2026-06-30** — Cleared the W3C `current-output-uri` conformance cluster (1 remaining failure → 0).
  - `TransformEngine.TransformFunction` now compiles template match patterns and registers grouping functions before executing a stylesheet function, so `xsl:apply-templates` inside `xsl:function` can match template rules.
  - Result-document URI tracking is reset at function entry points.
  - Full W3C suite: **4,943 passed / 307 failed / 9,350 skipped** (~94.2%).

- **2026-06-28** — Cleared the W3C `apply-templates` conformance cluster (11 runnable failures → 0).
  - `match="/"` now uses the correct default priority of `-0.5` in XSLT 2.0/3.0.
  - Root-template selection now respects the default (unnamed) mode and applies conflict resolution, fixing `mode="#current"` through `xsl:call-template`.
  - `document-node(element(E))` and `document-node(element(*))` match patterns now compile and match correctly.
  - `xsl:apply-templates` with no `@select` raises `XTTE0510` when the context item is not a node.
  - `xsl:apply-imports` and `xsl:next-match` now forward ordinary parameters to the built-in rule fallback.
  - Ambiguous template matches raise `XTRE0540` for test cases that declare `on-multiple-match="error"` via the new `XsltCompiler.TreatRecoverableAmbiguousMatchAsError` flag.
  - Full W3C suite: **4,871 passed / 379 failed / 9,350 skipped** (~92.8%).

- **2026-06-28** — Restored the W3C `catalog` self-test set and fixed the O(N²) slowness that made it hang.
  - `NormalizeSequence` in the XPath VM now removes duplicate nodes with a `HashSet<IXdmNode>` instead of a nested loop, dropping large cross-document sequences from >10 minutes to seconds.
  - The `catalog` cluster now completes in under a minute; remaining failures are XML 1.1 parser limitations and a `catalog-007` `element-available()` mismatch.
  - Full W3C suite: **4,855 passed / 395 failed / 9,350 skipped** (~92.5%).

- **2026-06-28** — Cleared the `resolve-uri` cluster (24/24) and the `namespace-4801` regression.
  - `fn:resolve-uri()` now raises `FORG0002` for malformed relative URIs and relative base URIs per erratum FO.E1.
  - Dotted-path URIs resolve correctly in `fn:resolve-uri()` and `fn:static-base-uri()`.
  - `fn:document()` resolves relative URIs against the base URI of the supplied node argument.
  - Text/PI nodes produced by DTD entity expansion now inherit the parent element's resolved `xml:base`.
  - TVT evaluation passes the context element so the compiled XPath uses the correct in-scope namespaces and effective base URI.
  - Full W3C suite: **4,852 passed / 391 failed / 9,357 skipped** (~92.5%).
  - The `catalog` self-test set is temporarily skipped because it became extremely slow after the `document()` node-base fix.

- **2026-06-28** — Fixed the remaining XSLT `namespace` cluster failure (`namespace-3005`).
  - Top-level `xsl:namespace` instructions now produce a standalone namespace-node item when the containing sequence constructor is typed as `node()` or `node()?` (in addition to explicit `namespace-node()` types).
  - Full W3C suite: **4,845 passed / 405 failed / 9,350 skipped** (~92.3%).

- **2026-06-27** — Cleared the remaining single-failure clusters `sort`, `merge`, and `arrays`.
  - `sort-072`: `xsl:perform-sort` now preserves in-scope prefixed namespaces for relocated sequence-constructor children.
  - `merge-066`: XPath prefix validation no longer treats integer map keys (e.g. `map{1:xs:dateTime(...)}`) as undeclared QName prefixes.
  - `square-array-201`: `xsl:source-document` with `streamable="no"` now loads the document and evaluates its content with the loaded document as the context item.

- **2026-06-27** — Cleared the entire XSLT `math` conformance cluster (15 runnable failures).
  - `fn:number` now parses XPath lexical forms `INF`, `-INF`, and `NaN`.
  - `fn:floor`, `fn:ceiling`, and `fn:round` now atomize their arguments before numeric dispatch.
  - `fn:round` / `fn:round-half-to-even` now use `decimal` arithmetic for precision-bound decimal/integer values, fixing tie-rounding and large-integer precision loss.
  - `xs:double` and `xs:float` serialization now uses shortest round-trip formatting (`"G16"` / `"G9"`) in the scientific-notation range, producing canonical output such as `1.0E-98` instead of `1.0000000000000001E-98`.

- **2026-06-27** — Cleared the entire XSLT `maps` conformance cluster (35 runnable failures).
  - Implemented `xsl:map` and `xsl:map-entry` instructions in `TransformEngine`, including duplicate-key (`XTDE3365`) and non-entry-content (`XTTE3365`) errors, and `XTDE0450` when a map is used as an element/document child.
  - `fn:serialize` now supports `method=json` for maps, arrays, booleans, numbers, and strings.
  - Map key equality now treats `xs:anyURI` as comparable to `xs:string` and handles `NaN` numeric keys safely.
  - `XPath31Expression.Compile` resolves function-call namespaces from `CompileOptions` and reports static `XPST0017` for removed functions and the obsolete `http://www.w3.org/2011/xpath-functions/map` namespace.
  - The conformance harness expands W3C test-suite `_select` AVT attributes using static parameters before compilation.
  - Follow-up fixes: preserve explicit `Q{uri}local` namespace URIs in function calls/named function refs; fall back to run-time `_select` expansion when static parameters are insufficient; atomize/flatten arrays for `xsl:apply-templates`, `xsl:value-of`, AVTs, and complex content construction.

- **2026-06-27** — Cleared the remaining XSLT `namespace` cluster failures (`namespace-0912` and `namespace-2611`) and the full `namespace-alias` cluster.
  - Built-in `shallow-copy` now suspends the outer sequence accumulator while applying templates to children, so typed variables containing shallow-copied elements keep child results nested instead of escaping as siblings.
  - `XdmValue.EffectiveBooleanValue()` now follows XPath sequence EBV rules: empty sequence → `false`, singleton sequence → EBV of its item, multi-node sequence → `true`, multi-item atomic sequence → `FORG0006`.

- **2026-06-26** — Cleared the XSLT `date` conformance cluster (46 runnable failures).
  - `xsl:value-of` now evaluates the `_select` AVT used by static-parameter test stylesheets.
  - `fn:format-date`, `format-time`, and `format-dateTime` now support escaped brackets (`[[` / `]]`), roman/alphabetic presentations, ISO week-of-month around year boundaries, non-BMP digit families, correct default widths, and timezone semantics (`[Z]` / `[z]`).
  - `fn:adjust-dateTime-to-timezone` now preserves the target timezone offset instead of returning a zero-offset value.

## 1. Consuming Bosak

### 1.1 Via Project References (development)

Add project references to the Bosak layer stack from your consuming project:

```xml
<ItemGroup>
  <ProjectReference Include="..\Bosak\src\Bosak.XPath.Api\Bosak.XPath.Api.csproj" />
  <ProjectReference Include="..\Bosak\src\Bosak.Xslt\Bosak.Xslt.csproj" />
  <!-- <ProjectReference Include="..\Bosak\src\Bosak.XQuery\Bosak.XQuery.csproj" /> -->
</ItemGroup>
```

`Bosak.XPath.Api`, `Bosak.Xslt`, and `Bosak.XQuery` pull in the lower layers automatically (Core, Parser, Compiler, Runtime, Standard, Providers).

**Target framework:** `net10.0` (both sides must align).

### 1.2 Via NuGet Packages

All Bosak source projects are now packable. After packing (`dotnet pack`), the following packages are produced:

| Package | Description |
|---------|-------------|
| `Bosak.Xslt` | XSLT 3.0 processor and transform engine |
| `Bosak.XPath.Api` | Public API for compiling and evaluating XPath 3.1 |
| `Bosak.XPath.Core` | XDM types and core abstractions |
| `Bosak.XPath.Runtime` | Register-based VM execution engine |
| `Bosak.XPath.Standard` | Standard XPath 3.1 / XQuery function library |
| `Bosak.XPath.Providers` | `IXdmNode` adapters for `System.Xml.Linq` |
| `Bosak.XPath.Parser` | Recursive-descent XPath 3.1 parser |
| `Bosak.XPath.Compiler` | AST-to-IR compilation pipeline |

**Consuming from a private feed:**
```bash
# Pack all projects
dotnet pack Bosak.sln --output ./nupkgs

# Push to your private NuGet feed
dotnet nuget push ./nupkgs/Bosak.Xslt.1.0.0.nupkg --source https://your-feed/nuget/v3/index.json
```

Then reference in your consuming project:
```xml
<ItemGroup>
  <PackageReference Include="Bosak.Xslt" Version="1.0.0" />
  <PackageReference Include="Bosak.XPath.Providers" Version="1.0.0" />
</ItemGroup>
```

> **Note:** Transitive dependencies are automatically resolved. You only need to reference the top-level packages your code directly uses (`Bosak.Xslt` and/or `Bosak.XPath.Api`).

---

## 2. XPath 3.1 Expressions

### 2.1 Compile & Evaluate

```csharp
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;

// One-shot evaluation
var expr = XPath31Expression.Compile("/invoice/items/item[@price > 100]");
var result = expr.Evaluate(document);

// Re-use compiled expression
var expr2 = XPath31Expression.Compile("$minPrice + $taxRate * $amount");
var result2 = expr2.Evaluate(
    new EvaluationContext()
        .WithVariable("minPrice",  XdmValue.FromDecimal(100.00m))
        .WithVariable("taxRate",   XdmValue.FromDecimal(0.21m))
        .WithVariable("amount",    XdmValue.FromDecimal(500.00m)));
```

### 2.2 Evaluation Context

```csharp
using Bosak.XPath.Runtime.Vm;

var ctx = new EvaluationContext
{
    BaseUri = "file:///C:/Data/",
    DocumentLoader = uri => /* your IXdmNode loader */
};

// Pre-register a source document so fn:doc(document-uri($node)) returns the same node.
ctx.RegisterDocument("file:///C:/Data/input.xml", sourceDocument);

ctx.WithNamespace("edi", "http://example.org/edi")
   .WithVariable("docId", XdmValue.FromString("DOC-1234"));

var result = expr.Evaluate(ctx);
```

### 2.3 Reading Results

```csharp
if (result.IsNode && result.NodeValue is { } node)
{
    Console.WriteLine(node.StringValue);
}
else if (result.IsSequence && result.SequenceValue is { } seq)
{
    foreach (var item in XdmSequence.FromSource(seq))
        Console.WriteLine(item.ToString());
}
else
{
    Console.WriteLine(result.ToString());
}
```

### 2.4 Context Item (Focus)

```csharp
// Evaluate with a context item so `.` and `position()` work
ctx.WithFocus(XdmValue.FromNode(document), position: 1, size: 1);
var result = expr.Evaluate(ctx);
```

---

## 3. XSLT Transforms

### 3.1 Compile a Stylesheet

```csharp
using Bosak.Xslt.Api;

var xsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
    <xsl:template match='/'>
        <output><xsl:value-of select='root/@id'/></output>
    </xsl:template>
</xsl:stylesheet>";

var compiler = new XsltCompiler();
var executable = compiler.Compile(xsl);
```

### 3.2 Transform a Document

```csharp
using Bosak.XPath.Providers.Xml;
using System.Xml.Linq;

var source = new XDocument(new XElement("root", new XAttribute("id", "42")));
var resultXml = executable.TransformToString(new XDocumentNode(source));
// => "<output>42</output>"
```

### 3.3 Named Templates & `call-template`

```csharp
var xsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
    <xsl:template match='/'>
        <result>
            <xsl:call-template name='format-address'>
                <xsl:with-param name='city' select='root/city'/>
            </xsl:call-template>
        </result>
    </xsl:template>

    <xsl:template name='format-address'>
        <xsl:param name='city'/>
        <address><xsl:value-of select='$city'/></address>
    </xsl:template>
</xsl:stylesheet>";

var executable = new XsltCompiler().Compile(xsl);
var result = executable.TransformToString(new XDocumentNode(source));
```

### 3.4 Tunnel Parameters

```csharp
var xsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
    <xsl:template match='/'>
        <xsl:apply-templates>
            <xsl:with-param name='traceId' select='"REQ-123"' tunnel='yes'/>
        </xsl:apply-templates>
    </xsl:template>

    <xsl:template match='item'>
        <!-- $traceId is available here via tunnel -->
        <item trace='{$traceId}'><xsl:value-of select='.'/></item>
    </xsl:template>
</xsl:stylesheet>";
```

### 3.5 `fn:transform()` — XSLT from XPath

```csharp
var callerXsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
    xmlns:map='http://www.w3.org/2005/xpath-functions/map'>
    <xsl:template match='/'>
        <result>
            <xsl:copy-of select='transform(map{
                ""stylesheet-location"": ""file:///C:/styles/main.xsl"",
                ""source-node"": .,
                ""stylesheet-params"": map{""greeting"": ""world""}
            })?output'/>
        </result>
    </xsl:template>
</xsl:stylesheet>";
```

---

## 4. XSLT Feature Matrix (Current State)

| Feature | Status | Notes |
|---------|--------|-------|
| `xsl:template match="…"` | ✅ Working | Pattern compiler: element names, `*`, `@*`, predicates, union (`\|`) |
| `xsl:template name="…"` | ✅ Working | Named template dispatch; raw XDM result via `XsltExecutable.Transform(..., rawResult: true)`; whitespace/EQName names normalized; `xsl:initial-template` permitted in XSLT namespace |
| `xsl:call-template` | ✅ Working | With `xsl:with-param` support; matches named templates by expanded QName (different prefixes bound to the same URI); rejects template names in reserved namespaces (`XTSE0080`) except `xsl:initial-template` |
| `xsl:apply-templates` | ✅ Working | Default mode; `select` attribute supported |
| `xsl:value-of` | ✅ Working | |
| `xsl:for-each` | ✅ Working | Position / size context updated per item |
| `xsl:if` / `xsl:choose` | ✅ Working | `when` + `otherwise` |
| `xsl:element` / `xsl:attribute` | ✅ Working | |
| `xsl:text` | ✅ Working | |
| `xsl:copy` | ✅ Working | Shallow copy with `@select` support; focus set correctly per item |
| `xsl:copy-of` | ✅ Working | Deep copy of nodes; Document nodes supported. `copy-namespaces` respected. Static validation rejects disallowed children (`XTSE0260`) and invalid attributes (`XTSE0090`). |
| `xsl:evaluate` | ✅ Working | Dynamic XPath 3.1 evaluation inside XSLT; supports context item, `xsl:with-param` / `@with-params`, in-scope namespaces, base URI, default collation, and `@as` coercion. Stylesheet functions are visible only when not `private`/`hidden`. Java extension functions are not supported. |
| `xsl:comment` | ✅ Working | `select` attribute or text content |
| `fn:copy-of` | ✅ Working | XSLT 3.0 context function |
| `xsl:decimal-format` | ✅ Working | Parsed and registered for `fn:format-number` |
| `xsl:variable` | ✅ Working | Lexical scoping; `as` attribute with full atomic type coercion and atomization (`xs:integer`, `xs:string`, `xs:boolean`, `xs:double`, `xs:decimal`, `xs:duration`, `xs:QName`, `xs:dateTime`, gYear, etc.). Node type tests (`element(...)`, `attribute(...)`, `document-node(...)`, `node()`, `item()`) bypass atomization. Usable in XPath via `$var`. Global variables with sequence constructors are evaluated lazily on first reference with a singleton focus based on the root node of the tree containing the initial context node (XSLT 3.0 §9.6). |
| `xsl:param` | ✅ Working | On named templates, global params, default values; `as` attribute with full atomic type coercion and atomization. Subtype substitution (integer→decimal, float→double) and type promotion supported. |
| Built-in template rules | ✅ Working | Shallow-copy elements, copy text/attributes |
| Literal result elements | ✅ Working | Namespace preservation, AVT evaluation |
| `xsl:import` / `xsl:include` | ✅ Working | URI resolution with correct precedence rules |
| Modes | ✅ Working | Named modes, `#current`, `#default`, `#all`, multi-mode templates |
| `xsl:sort` | ✅ Working | Single and multi-key; `data-type`, `order`, `stable`, AVTs for `lang`/`case-order`/`collation`; recognized collations including UCA `alternate=non-ignorable`, `blanked`, and `shifted`. Default collation is respected; `case-order` works without an explicit collation. |
| `xsl:number` | ✅ Working | `single`, `any`, `multiple` levels; format tokens |
| `xsl:key` / `key()` | ✅ Working | Indexed lookup; composite keys; content-constructor keys preserve typed atomic values; results returned in document order; `key()` allowed in match patterns with XTSE0340 validation. Key-value comparison respects the effective default or explicit `@collation`; conflicting collations for the same key name raise XTSE1220. |
| `xsl:output` | ✅ Working | `method` (`xml`, `html`, `xhtml`, `text`, `json`, `adaptive`), `indent`, `omit-xml-declaration`, `encoding`, `version`, `standalone`, `doctype-system`, `doctype-public`, `cdata-section-elements`, `escape-uri-attributes`, `include-content-type`, `media-type`, `byte-order-mark`, `html-version`, `suppress-indentation`, `normalization-form`, `use-character-maps`, `json-node-output-method`, `allow-duplicate-names`, `escape-solidus`, `item-separator`, `parameter-document`, `undeclare-prefixes`, `build-tree`. Encoding-aware output escapes unrepresentable characters as numeric character references and splits CDATA sections around them. Named `xsl:output` definitions are resolved by import precedence; `xsl:result-document` attributes override the effective output definition. XHTML 1.0 uses the HTML 4 empty-element list; XHTML5 strips the XHTML namespace prefix, serializes HTML5 void elements as empty tags, ignores `doctype-public` when no `doctype-system` is supplied, and preserves root-element case in the DOCTYPE. DOCTYPE literal values are quoted with the delimiter that does not occur in the value. JSON output serializes maps, arrays, booleans, numbers, strings, and nodes; `json-node-output-method` controls node serialization; `escape-solidus` defaults to `yes`; character maps are applied to the final JSON text. Maps/arrays/functions at the top level of a non-JSON output raise `SENR0001`. `xsl:result-document` supports `method="json"`, `method="adaptive"`, and `build-tree="no"` with raw-item collection. `SEPM0009` and `SEPM0010` are enforced for XML/XHTML output properties. |
| `xsl:character-map` | ✅ Working | `name`, `use-character-maps`, and `xsl:output-character/@character` / `@string`; merged in declaration order, later maps override earlier ones (last-wins) for duplicate characters; explicit mappings override referenced maps within a single character map; applied to text, attribute, comment, PI, raw-XML, JSON, and adaptive output in all methods |
| `xsl:function` | ✅ Working | User-defined XPath functions in XSLT; `@as` return type enforced via `ConvertVariableValue` |
| `xsl:sequence` | ✅ Working | Returns sequences from functions |
| `xsl:mode` | ✅ Working | `on-no-match`, `on-multiple-match`, `warning-on-no-match`, `warning-on-multiple-match`, `visibility`, `typed`, `streamable`, `default-mode`, duplicate-declaration checks (`XTSE0545`), and `#unnamed` normalization |
| `xsl:analyze-string` | ✅ Working | Regex matching/non-matching children; `regex-group()`; XSLT 3.0 zero-length match semantics; `@flags` including multiline (`m`) are passed to regex translation |
| Tunnel parameters | ✅ Working | `tunnel="yes"` propagation through `apply-templates` |
| `fn:transform()` | ⚠️ Partial | Basic XPath-level XSLT invocation works; `transform#1` unregistered, `stylesheet-location` edge cases and several result mismatches remain (W3C transform-002..009) |
| `xsl:attribute-set` / `use-attribute-sets` | ✅ Working | Accumulates across imports/includes; cycle detection; `xsl:next-match` inside attribute sets works |
| `xsl:use-when` | ✅ Working | Top-level and nested elements evaluated in document order; `true()`/`false()` and static-variable references work; XTSE0090 and XTSE3450 error cases validated. |
| Shadow attributes (`_{attr}` static AVTs) | ✅ Working | `_version`, `_href`, `_use-when`, `_xpath-default-namespace`, `_static`, `_select`, and other underscore-prefixed XSLT attributes are evaluated at compile time in the current static context and replace their non-underscore counterparts. Shadow attributes on literal result elements are preserved as ordinary attributes. |
| `xsl:where-populated` | ✅ Working | Filters empty sequences, empty text nodes, empty PIs, empty comments, and empty elements; attributes and namespace nodes do not make a sequence populated; empty strings and empty arrays are treated as empty |
| `xsl:on-empty` | ✅ Working | Evaluated by parent container (xsl:copy, xsl:document, literal result elements, general sequence constructors) when sequence constructor produces no nodes; supports `@select` and sequence constructor children; `on-empty` conformance cluster 72/72 |
| `xsl:on-non-empty` | ✅ Working | Evaluated by parent container when sequence constructor produces nodes; supports `@select` and sequence constructor children; `on-non-empty` conformance cluster 14/14 |
| `xsl:context-item` | ✅ Working | Declares required/optional/absent context item and type for templates; raises `XTTE0590`/`XTTE3090` at runtime and `XTSE0010`/`XTSE0020`/`XTSE0090` statically. `context-item` conformance cluster 31/31. |
| `xsl:iterate` | ✅ Working | Stateful iteration in result-tree and function-body contexts with `xsl:param`, `xsl:next-iteration`, `xsl:break`, and `xsl:on-completion`. `iterate` conformance cluster 44/44. |
| `xsl:message` | ✅ Working | Evaluates `terminate` and `error-code`; emits serialized message text via `IXsltMessageListener`; terminating messages throw `XsltRuntimeException` carrying the XDM value. The listener also receives `OnWarning` callbacks for XSLT warnings (e.g. no-matching-template / multiple-template warnings). |
| `xsl:try` / `xsl:catch` | ✅ Working | Catches dynamic XPath/XSLT errors in both result-tree and function-body contexts; rolls back output written in the try block before executing a matching catch (unless `rollback-output="no"`). Supports multiple `xsl:catch` clauses evaluated in document order; `@errors` supports `*`, plain local names, `prefix:local` (err namespace), `*:local`, and `Q{uri}local`; binds `$err:code`, `$err:description`, `$err:value`. Static errors in `xsl:variable`/`xsl:param`/`xsl:with-param` `@select` expressions are now reported at stylesheet compile time. |
| `xsl:map` / `xsl:map-entry` | ✅ Working | `xsl:map` evaluates its content as map-entry-producing sequence constructor and merges entries; `xsl:map-entry` builds a single-entry map; duplicate keys raise `XTDE3365`; maps as element/document children raise `XTDE0450` |
| `xsl:result-document` | ✅ Working | Secondary result documents with `format`, `href`, and serialization attributes; principal `xsl:result-document` captured for `TransformToString`; `current-output-uri()` reflects the active result-document URI and is empty outside any result document |

---

## 5. XPath 3.1 Feature Highlights

### Well-covered areas
- Sequence construction, filtering, FLWOR expressions
- Standard `fn:*` functions (string, numeric, date/time, QName, URI)
- `map:*` and `array:*` functions
- Higher-order functions (`fn:for-each`, `fn:filter`, `fn:fold-left`, etc.)
- `fn:doc`, `fn:collection` with pluggable document loader
- Decimal formatting (`fn:format-number`)
- JSON functions: `fn:parse-json`, `fn:json-to-xml`, `fn:xml-to-json`, `fn:json-doc`
- Date/time ordering (`lt`, `gt`, `le`, `ge`)
- `fn:analyze-string` — nested group structure and zero-length checks

### Known gaps
- `fn:load-xquery-module` — not implemented
- `fn:serialize` — partial (JSON method supported for maps/arrays/atomics; XML serialization options still limited)
- `fn:transform` options (`delivery-format`, etc.) — partial
- Schema-aware operations — not supported
- Regex functions (`fn:matches`, `fn:tokenize`, `fn:replace`) — XSD regex validation, backreferences, flags, and `$` end-anchor semantics are now spec-compliant; surrogate-pair handling in `.` is the remaining gap

---

## 6. XSD Validation

Bosak provides an `IXsdValidator` abstraction for XML Schema validation:

```csharp
using Bosak.XPath.Api.Xsd;

var validator = new XsdValidator();
var result = validator.TryValidate(xmlString, xsdStream);

if (result.IsValid)
{
    Console.WriteLine("Document is valid");
}
else
{
    foreach (var error in result.OnlyErrors)
    {
        Console.WriteLine($"Error at line {error.LineNumber}: {error.Message}");
    }
}
```

Features:
- Single-schema and multi-schema validation (handles `xs:import`/`xs:include`)
- Structured error results with line/column numbers
- Non-throwing `TryValidate` and throwing `Validate` variants
- Configurable via `XsdValidatorOptions` (max error count, treat warnings as errors)

---

## 7. Current Build State

Run the full suite from the Bosak repo root:

```bash
dotnet build Bosak.sln
dotnet test Bosak.sln
```

**Unit tests:** 913 passed, 0 failed, 0 skipped  
**Target framework:** `net10.0`

### Behavioral Changes

| Change | Impact | When |
|--------|--------|------|
| `XsltExecutable.Transform` gained an optional `rawResult` parameter. | When `true` and an initial named template is used, returns the raw template result as an XDM value instead of wrapping it in a result document. Required for `initial-template-004` and similar raw-output tests. | 2026-06-25 |
| `TransformEngine.IsNodeAttached` now treats a document's root element as attached. | After whitespace stripping, the initial source node (`/doc`) was incorrectly considered detached because the root `XElement` has no `XObject.Parent`. The check now also verifies `XObject.Document != null`. Fixes `mode-1105`. | 2026-06-26 |
| Regex `.` now matches Unicode code points including surrogate pairs. | `RegexHelper.TranslateDot` replaces `.` with an alternation that prefers a high+low surrogate pair over a single code unit. Fixes `regex-026` and aligns `fn:matches`/`replace`/`tokenize` with XPath/XSD semantics. | 2026-06-26 |
| `xsl:evaluate` blocks `fn:system-property`. | XSLT-defined functions are removed from the dynamic context; calling `system-property` inside `xsl:evaluate` now raises `XTDE3160`. Fixes `system-property-022`. | 2026-06-26 |
| `xsl:catch` now matches bare error codes. | `GetErrorCode` recognizes 8-character codes such as `FOUT1190` even without a trailing colon, so `xsl:catch errors="*:FOUT1190"` matches. Fixes `unparsed-text-lines-004`. | 2026-06-26 |
| Root-level literal result elements copy in-scope stylesheet namespaces. | `CopyLiteralElement` copies namespace declarations from the stylesheet root onto the output root element (except excluded prefixes and the XSLT namespace). Fixes `attribute-0601`. | 2026-06-26 |
| `xsl:analyze-string` now passes regex flags to `RegexHelper.ValidateAndTranslatePattern`. | Fixes multiline-mode (`m`) tests `analyze-string-007/067/071/090b`; previously `$` was translated to `\z` even when multiline was requested. | 2026-06-25 |
| `xsl:call-template` now resolves named templates by expanded QName. | A call using one prefix bound to a namespace URI finds a template declared with a different prefix bound to the same URI. Fixes `call-template-1701`. | 2026-06-25 |
| Initial template names from the conformance harness are expanded using catalog namespace bindings. | Names are passed to `TransformEngine` in Clark notation, so a test-catalog prefix bound to a different URI than the stylesheet prefix correctly raises `XTDE0040`. Fixes `call-template-0104/0105/0107`. | 2026-06-25 |
| `xsl:template/@name` values are whitespace-trimmed and validated against reserved namespaces. | Leading/trailing spaces and EQName forms such as ` Q{}temp ` are normalized; names in the XSD, XPath-functions, or XSLT namespaces raise `XTSE0080` (except the special `xsl:initial-template` name). Fixes `call-template-0106/0109`. | 2026-06-25 |
| `xpath-default-namespace` fully wired through XSLT → XPath pipeline. | `CompileOptions.DefaultElementNamespace` controls unprefixed element/type names in XPath expressions. Threaded through `CompileXPath`, `PatternCompiler`, `TemplateRule.ResolveNamespacePrefixes`, `VmEngine.NamespaceTest`, and whitespace stripping (`SpaceHandlingRule`). Fixes xpath-default-namespace-0101 through 1102 (21/22 passing). | 2026-06-11 |
| `xsl:attribute` with unprefixed name now uses empty namespace URI. | Previously inherited default namespace from parent; now correctly produces no-namespace attributes per XSLT spec. Fixes namespace-3306. | 2026-06-11 |
| `xsl:call-template` evaluates default `xsl:param` values when no `with-param` is provided. | Previously omitted parameters fell back to empty sequence instead of evaluating the param's `select` or sequence constructor. Fixes namespace-3501/3503. | 2026-06-11 |
| `AddElementToContainer` injects `xmlns=""` when no-namespace element is placed inside a default-namespace parent. | Prevents LINQ-to-XML from silently inheriting parent's default namespace. Fixes namespace-0913. | 2026-06-11 |
| `fn:node-name` on text nodes returns `XdmValue.Undefined` (empty sequence). | Was incorrectly returning empty sequence due to unintended `NodeToQName` change. Reverted to spec-compliant `Undefined`. | 2026-06-11 |
| `CopyLiteralElement` no longer walks ancestor chain to copy namespace declarations. | Was leaking `xmlns:xs` and other stylesheet prefixes into literal result elements, breaking `exclude-result-prefixes` and `fn:transform` output. | 2026-06-11 |
| `*:local` name tests now emit `"*:local"` into the literal pool. | Prevents VM from applying the no-namespace attribute restriction to `*:local` patterns. Fixes namespace-1402 and related tests. | 2026-06-11 |
| Global sequence-constructor variables now evaluate with the initial context item. | `xsl:variable` sequence constructors at the top level use a singleton focus based on the root of the source tree (XSLT 3.0 §9.6), not the focus at the point of reference. Fixes `string-041`. | 2026-06-12 |
| Named-template entry points without a source document use an absent context item. | `XsltExecutable.Transform`/`TransformToString` accept a null source; the conformance harness passes null for named-template tests with no explicit source. Keeps `copy-4308` (XTTE0945) correct. | 2026-06-12 |
| Namespace node `parent::node()` now returns the element whose namespace axis includes the node (`_namespaceOwner`), not the element where the underlying `XAttribute` declaration resides. | Fixes `.. is $e` for inherited namespace nodes in XPath. Required for XSLT `namespace::*` axis correctness. | 2026-06-10 |
| `xsl:variable`/`xsl:param`/`xsl:function`/`xsl:with-param` `@as` now fully supports atomic type coercion and atomization. | `ConvertVariableValue` rewrites atomization + casting via `VmEngine.TryCast`. Subtype substitution (integer→decimal, float→double) and type promotion. Node type tests (`element(...)`, `attribute(...)`, `document-node(...)`) bypass atomization. | 2026-06-11 |
| `xsl:document` no longer leaks outer `_sequenceAccumulator` into its sequence constructor. | `wrapInDocumentNode=true` now isolates the accumulator, ensuring `xsl:copy-of` inside `xsl:document` unwinds document nodes into the new document instead of the outer variable. | 2026-06-11 |
| `xsl:call-template/@as` now raises `XTSE0010` at runtime. | `@as` is not permitted on `xsl:call-template`; previously ignored. | 2026-06-11 |
| `xsl:copy` now raises `XTTE0945` (no context item), `XTTE3180` (select returns >1 item), `XTDE0410` (attribute after children), and `XTDE0420` (attribute on non-element) per XSLT 3.0 spec. | Previously these error conditions were silently ignored or produced wrong results. | 2026-06-10 |
| XSLT functions (`xsl:function`) no longer leak the first argument as the context item. | Functions now correctly have no context item per XSLT 3.0 §9.6. Fixes `xsl:copy` inside functions. | 2026-06-10 |
| `xsl:where-populated` now correctly filters empty PIs, comments, and text nodes. | Previously only whitespace-only text nodes were filtered; empty PIs/comments passed through incorrectly. | 2026-06-10 |
| XPath parser no longer treats prefixed names as kind tests (e.g. `my:node()`). | `my:node()` was parsed as `child::node()` instead of a function call. Affected any prefixed name where local name matched a kind test. | 2026-06-10 |
| `xsl:key` content constructors now preserve typed atomic key values. | `string-length(.)`, `string-to-codepoints(.)`, and other atomic producers are stored as typed values rather than converted to text nodes. Fixes `key-082`, `key-073/074/075`. | 2026-06-12 |
| `key()` lookup results are returned in document order. | Multiple `xsl:key` definitions with the same name no longer return nodes in definition order. Fixes `key-073/074/075` ordering. | 2026-06-12 |
| Pattern predicates in `key()` match patterns now isolate caller focus. | `PatternCompiler.WrapWithCurrentItem` saves/restores context item, position, and size so `xsl:number` with `key()` patterns does not corrupt subsequent instructions. Fixes `key-035`. | 2026-06-12 |
| `key()` pattern validation restored. | XTSE0340 is raised for invalid second arguments in `key()` match patterns; numeric literals, variable references, and parenthesized sequences are allowed. Fixes `key-083`, `key-093`, `key-097`, `match-079`, `match-080`. | 2026-06-12 |
| `xsl:where-populated` now implements populated-node semantics per XSLT 3.0. | Document nodes from `xsl:document` and items from `xsl:sequence` are preserved; empty elements/documents are filtered; `xsl:on-empty` children are honoured. Fixes `element-0104` through `element-0108`. | 2026-06-11 |
| Fragment document nodes now serialize correctly. | Multi-root `xsl:document` results are wrapped in `__xdm_doc__` during copying and unwrapped by `ResultTreeSerializer`. Fixes `xsl-document-0501`. | 2026-06-11 |
| `xsl:document` inside simple content contributes the document's string value. | Excludes comment/PI descendants; fixes `xsl-document-0601`. | 2026-06-11 |
| `XDocumentNode.StringValue` for synthetic-wrapper documents includes all descendant text. | Previously only direct text children of the wrapper were included. | 2026-06-11 |
| `xsl:message` now includes both `@select` and sequence-constructor content. | Both contributions are concatenated, matching XSLT 3.0 semantics. Fixes `xsl-document-0603`. | 2026-06-11 |
| Conformance harness supports `<assert-message>` and fragment assertions. | Messages are captured via `RecordingMessageListener`; multiple direct assertion children are treated as an implicit `<all-of>`. | 2026-06-11 |
| `xsl:copy` shallow copy no longer copies source attributes/children. | Source attributes and children must now be produced by the contained sequence constructor, matching the XSLT spec. Fixes `attribute-set-0107`. | 2026-06-13 |
| XPath parser resolves the `xml` prefix to the XML namespace in node tests. | Previously `@xml:*` fell back to a prefix-only match that matched attributes in any namespace. Fixes `attribute-0901`. | 2026-06-13 |
| `fn:document#1/#2` supports URI fragment identifiers. | Fragment identifiers resolve to the element with matching `id`/`xml:id`; relative document URIs resolve from the stylesheet base URI. Fixes `id-001`. | 2026-06-13 |
| `xsl:evaluate` is fully supported. | Dynamic XPath 3.1 evaluation inside XSLT with context item, parameters, namespaces, base URI, default collation, and `@as` coercion. Fixes the entire `evaluate` cluster. | 2026-06-13 |
| Cross-tree document order now follows document creation order. | `XDocumentNode.DocumentOrder` combines a global creation sequence (high bits) with the per-document local index (low bits), so union/path results across separately constructed temporary trees are stable. Fixes `evaluate-002`. | 2026-06-13 |
| `xsl:function` validation and static errors are implemented. | `Stylesheet.ValidateInstructionTree` reports XTSE0020/XTSE0080/XTSE0770/XTSE0090/XTSE0740 for invalid function declarations, reserved namespaces, duplicate signatures, and `Q{}local` names. Fixes function cluster validation tests. | 2026-06-13 |
| `xsl:function` supports deterministic memoization. | `new-each-time="no"` results are cached per (name, arity, argument) key; AVTs on `_new-each-time` select deterministic/non-deterministic mode at run time. Fixes `function-0240` and related tests. | 2026-06-13 |
| `fn:function-available` is fully spec compliant. | Parses EQNames, atomizes/casts the arity argument, reports `fn:concat` for any variadic arity, and reports the full XSLT 3.0 function set. Fixes `function-available` cluster. | 2026-06-13 |
| `fn:element-available` is implemented. | Reports availability for XSLT 2.0/3.0 instructions in the XSLT namespace. The unprefixed first argument is expanded using the XML default namespace of the element containing the expression, tracked separately from the XPath default namespace via `CompileOptions.DefiningElementDefaultNamespace`. Fixes `function-0302b`. | 2026-06-24 |
| `fn:available-environment-variables` and `fn:environment-variable` are implemented. | Returns/succeeds on process environment variables; matching is case-sensitive exact. Fixes function cluster environment tests. | 2026-06-13 |
| Numeric arguments to `fn:subsequence` and `fn:format-integer` are atomized. | Atomization now preserves `xs:untypedAtomic`, so attribute and element text nodes are accepted implicitly. Fixes `function-0502`, `function-0503`, and related tests. | 2026-06-13 |
| Namespace context is applied to `xsl:variable`/`xsl:param`/`xsl:with-param` @select expressions. | Local and global variable/param `select` expressions, and named-template default param values, now use the in-scope namespace bindings and effective default namespace. Fixes unprefixed EQName tests in `function` cluster. | 2026-06-13 |
| `date` cluster is fully passing. | Implicit timezone, `xs:time` midnight semantics, timezone adjustment, AM/PM formatting, extended-year constructor bounds, and static-parameter substitution in the harness. Fixes all runnable `date` tests. | 2026-06-13 |
| `xsl:message` now implements terminate/error-code semantics and serializes node content. | Messages evaluate `@terminate` and `@error-code`, emit via `IMessageListener`, and throw `XsltRuntimeException` with the captured XDM value when terminating; `xsl:try`/`xsl:catch` binds `$err:code`, `$err:description`, `$err:value`. Fixes the `message` conformance cluster (45/0/0). | 2026-06-13 |
| `fn:unparsed-text` resolves relative `href` against `EvaluationContext.BaseUri`. | Previously resolved only against the static base URI parameter; now uses the dynamic base URI when no explicit base is supplied. Required for `message-0313`. | 2026-06-13 |
| `fn:element-available` uses the defining element's default namespace. | Added `EvaluationContext.DefiningElementDefaultNamespace` and `CompileOptions.DefiningElementDefaultNamespace` so that XSLT's `element-available()` expands unprefixed QNames using `xmlns="..."` rather than `xpath-default-namespace`. | 2026-06-24 |
| `validation="lax"` is accepted on basic processors. | Non-schema-aware processors no longer raise `XTSE1660` for `lax` (or `default-validation="lax"`), matching XSLT 3.0 semantics. Fixes `validation-0102b`. | 2026-06-24 |
| `fn:doc('')` resolves against the static base URI. | In XSLT, `fn:doc('')` now loads the stylesheet module; in pure XPath it still yields the empty sequence when no base URI is present. Fixes `document-0302`. | 2026-06-24 |
| `fn:doc` atomizes and validates its argument. | Empty sequence returns empty sequence; more than one item raises `XPTY0004`; prevents literal `\(sequence\)` from being loaded as a URI. Fixes `document-0303/0307/0601/0901/1101`. | 2026-06-24 |
| Stylesheet module base URIs are preserved. | `FileSystemUriResolver`, `XsltCompiler`, and the conformance `TestUriResolver` load stylesheet modules with `LoadOptions.SetBaseUri`, so `fn:doc('')` and `fn:document()` inside included/imported modules resolve relative URIs against the correct module base URI. Fixes `document-1003/1004/1901`. | 2026-06-24 |
| `fn:doc`/`fn:document` loaded documents are subject to `xsl:strip-space`/`xsl:preserve-space`. | `EvaluationContext.DocumentPostProcessor` lets XSLT apply the stylesheet's whitespace-handling rules to documents loaded during transformation, while protecting stylesheet modules themselves from mutation. Fixes `document-0308`. | 2026-06-24 |
| `xsl:use-package` tests are skipped by the conformance harness. | The compiler does not support XSLT 3.0 packages; the harness now detects `xsl:use-package` in the principal stylesheet and reports a skip instead of a null-reference failure. Moves `document-2402` to skipped. | 2026-06-24 |
| `fn:unparsed-text-lines` is fully spec compliant. | Trailing line terminators no longer produce an empty final line; decoded text is validated for XML-legal characters, raising `FOUT1190` for invalid characters such as NUL. Fixes `unparsed-text-lines-002/004`. | 2026-06-24 |
| `xsl:try` / `xsl:catch` handles multiple catch clauses and error-code matching. | `TransformEngine` evaluates all `xsl:catch` children in order, matches `@errors` against `*`, plain names, `*:local`, `Q{uri}local`, and `prefix:local` (err namespace), and rethrows unmatched errors. Fixes `call-template-0110`. | 2026-06-25 |
| `fn:function-available` validates its argument. | Invalid QName/EQName syntax or an unbound prefix now raises `XTDE1400`; the error is propagated through `use-when` expressions. Fixes `extension-functions-0103/0104`. | 2026-06-24 |
| `extension-element-prefixes` bound to reserved namespaces is rejected. | The stylesheet loader reports `XTSE0800` when the XSLT, XML, XML Schema, or XML Schema instance namespace is declared as an extension namespace. Fixes `extension-functions-0105`. | 2026-06-24 |
| `use-when` namespace context includes all ancestors. | Prefixes declared on any ancestor of the element carrying `use-when` are now in scope for the expression. Fixes `extension-functions-0101`. | 2026-06-24 |
| `fn:snapshot` is implemented. | Creates a copy of a node with shallow ancestor copies (attributes/namespaces preserved) and deep-copied descendants; in-scope namespace bindings are now copied to every element in the snapshot, matching `xsl:copy-of validation="preserve"`. Top-level `xsl:namespace` instructions only produce standalone namespace-node items when the containing sequence constructor is typed as `namespace-node()`. Clears the `snapshot` cluster. | 2026-06-28 |
| `fn:innermost` / `fn:outermost` relationship checks are correct. | Both functions now use `IsSameNode` instead of reference equality, and `innermost`/`outermost` apply the correct descendant/ancestor filtering semantics. Fixes `innermost-001/901`. | 2026-06-24 |
| Conformance harness supports raw XDM comparison for `<initial-function>`. | Tests with `<output tree="no" serialize="no"/>` now compare the raw function result using `assert-type`, `assert-count`, `assert-deep-eq`, and `assert-eq` instead of serializing to a string. Fixes `initial-function-002` and `initial-function-100a..100i`. | 2026-06-24 |
| `VmEngine.ValueMatchesType` respects sequence occurrence indicators. | Top-level sequence values are now matched against `?`, `*`, and `+` occurrence indicators by checking each item against the base type. Fixes `initial-function-100e` (`xs:string*`). | 2026-06-24 |
| `xsl:function/@_name` AVTs are expanded to expanded QNames at parse time. | `XsltFunctionDefinition.FromElement` evaluates `_name` attribute value templates (including `xs:QName`-returning expressions) in the static context, so functions declared with dynamic names are registered under the correct expanded QName. Fixes `initial-function-101c..101e`. | 2026-06-24 |
| XPath value comparison casts `xs:untypedAtomic` to `xs:string`. | In value comparisons (`eq`/`ne`/`lt`/`le`/`gt`/`ge`), an `xs:untypedAtomic` operand is atomized to `xs:string` before comparison, so `xs:untypedAtomic('72') gt 70` raises `XPTY0004` while `xs:untypedAtomic('') eq ''` succeeds. General comparisons continue to promote `xs:untypedAtomic` to the other operand's type. Fixes `type-0165`. | 2026-06-25 |
| Whitespace stripping applies to the source document root, and stripped source nodes are treated as absent. | The engine strips whitespace from the document containing the initial context node, detects when the selected node has been removed, and evaluates globals with focus on the source-tree root. Fixes `strip-space-023`. | 2026-06-25 |
| Path expressions only load the context item when the first step is an axis step. | Prevents `parse-xml(...)/root/item` from raising `XPDY0002` when the XPath focus is absent. Required by the `strip-space` fix. | 2026-06-25 |
| `XsltCompiler.StaticParameters` supplies values for static `xsl:param` declarations. | Caller-supplied values override stylesheet `select` defaults and are coerced against `@as` during `BuildStaticContext()`. Required for parameterized static tests such as `static-003a/013c`. | 2026-06-26 |
| Static variables and parameters are eagerly bound at runtime. | `InitializeGlobalParametersAndVariables` binds values from `Stylesheet.StaticVariables` before lazy non-static globals, so static values remain visible even when a non-static declaration shadows the name. Fixes `static-027`. | 2026-06-26 |
| XTSE0090 and XTSE3450 validations for static declarations are implemented. | `static="yes"` is rejected on non-global `xsl:variable`/`xsl:param`; `visibility` is rejected on static declarations; a static variable and static parameter with the same expanded name raise `XTSE3450`. Fixes `static-020/023/025/026`. | 2026-06-26 |
| Static declarations without a value default to empty sequence (or undefined for required parameters). | Optional static variables/parameters default to `()`; required static parameters without a supplied value raise `XTDE0050`. Fixes `static-010` and related cases. | 2026-06-26 |
| General comparison with an empty operand returns `false`. | `VmEngine.CompareGeneral` now follows XPath 3.1 §17.3: one empty operand yields `false`, not an empty sequence. Fixes `static-011`. | 2026-06-26 |
| Namespace axis includes implied default namespaces. | `XDocumentNode.GetNamespaceAxis` adds a default-namespace node when the element is in a non-empty namespace that is not declared explicitly as default or prefixed. Fixes `static-030` and `json-to-xml` namespace-axis coverage. | 2026-06-26 |
| Static conformance cluster is fully passing. | All 49 `static` tests pass (was 47/49). Combined with the two cross-cutting fixes, the full W3C suite improves to 4,599/652/9,349. | 2026-06-26 |
| `xsl:use-attribute-sets` is allowed on literal result elements. | Added `use-attribute-sets` to the XTSE0805 whitelist of XSLT-namespaced attributes permitted on LREs. Clears the `attribute-set`, `xsl-document`, `analyze-string`, and `next-match` clusters. | 2026-06-26 |
| Precedence-aware XTSE3450 detection for static variables. | `Stylesheet.BuildStaticContext` evaluates top-level `use-when` in document order and tracks import precedence; same-precedence conflicting values and higher-precedence overrides that change the effective value raise `XTSE3450`. Fixes `use-when-0137/0138` and keeps `static` cluster at 49/49. | 2026-06-26 |
| `use-when` conformance cluster is fully passing. | All 99 runnable `use-when` tests pass (was 97/99); `use-when-0137/0138` now raise `XTSE3450` correctly. | 2026-06-26 |
| Shadow attributes (static AVTs) are implemented. | `_version`, `_href`, `_use-when`, `_xpath-default-namespace`, `_static`, `_select`, and other underscore-prefixed XSLT attributes are expanded at compile time using the current static context; shadow attributes on literal result elements are left untouched. Clears the `shadow` cluster. | 2026-06-26 |
| XSLT 1.0 backwards-compatible mode is fully implemented. | `CompileOptions.BackwardsCompatible` flows into the XPath optimizer, IR lowerer, VM arithmetic/comparisons, standard-function argument conversion, `xsl:value-of`, `xsl:number`, and `key()` string-valued lookups. Clears the `backwards` cluster (43/43 runnable). | 2026-07-07 |
| The `bug` conformance cluster is fully passing. | Imported-template XTSE0680 validation, `<assert-serialization>` file loading in the harness, namespace fixup for copied attributes, and `current()` inside `xsl:sort`. Clears the `bug` cluster (69/69 runnable). | 2026-07-07 |
| The `xpath-compat` conformance cluster is fully passing. | Backwards-compatible negative-zero constant folding and `fn:subsequence` numeric argument coercion for strings/untyped atoms. Clears the `xpath-compat` cluster (17/17 runnable). | 2026-07-07 |

### Conformance Baselines

| Suite | Passed | Failed | Skipped | Pass Rate | Notes |
|-------|--------|--------|---------|-----------|-------|
| XSLT 3.0 (W3C) | 5,506 | 99 | 8,995 | 98.2% | `output` cluster 179/24/29; `result-document` cluster 104/21/29; remaining failures are pre-existing non-output issues |
| XPath 3.1 (QT3) | 18,785 | 3,085 | 9,951 | 59.04% | Stable |

> **Note:** The conformance runner locks DLLs. If you get build errors about locked files, run:
> ```bash
> taskkill /F /IM Bosak.XPath.Conformance.exe
> taskkill /F /IM Bosak.Xslt.Conformance.exe
> ```

---

## 8. VS Code Extension

Bosak ships with a VS Code extension (`vscode-bosak/`) that provides syntax highlighting, realtime diagnostics, and auto-completion via a Language Server Protocol (LSP) server.

### 8.1 Building & Running

```bash
# Build the language server (.NET 10)
dotnet build src/Bosak.LanguageServer/Bosak.LanguageServer.csproj

# Build the extension client (Node.js 18+)
cd vscode-bosak
npm install
npm run compile

# Launch Extension Development Host
code . --goto src/extension.ts
# Then press F5 inside VS Code
```

### 8.2 Packaging as VSIX

```bash
cd vscode-bosak
npx vsce package
# Produces: vscode-bosak-0.1.0.vsix
```

Install in VS Code: **Extensions** → **⋯** → **Install from VSIX…**

### 8.3 Extension Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `bosak.server.path` | `string \| null` | `null` | Absolute path to `Bosak.LanguageServer` binary. When null, the extension searches the workspace. |
| `bosak.trace.server` | `string` | `"off"` | LSP traffic tracing: `"off"`, `"messages"`, `"verbose"`. |

### 8.4 Supported File Types

| Language | Extensions | Features |
|----------|------------|----------|
| XPath | `.xpath` | Syntax highlight, diagnostics, completions (functions, axes, keywords) |
| XSLT | `.xsl`, `.xslt` | Syntax highlight, diagnostics, completions (XSLT instructions + XPath) |

---

## 9. Getting Help / Reporting Issues

- Check `docs/ARCHITECTURE.md` in the Bosak repo for the layer overview and execution pipeline.
- Check `docs/FEATURE_REQUESTS.md` for the feature request registry.
- XPath failures: capture the expression, input XML, and expected vs. actual result.
- XSLT failures: capture the stylesheet fragment, source XML, and expected output.
