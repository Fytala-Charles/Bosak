# K04-C34 — Construction modes and consistent typed copies

28 October 2026. Implementation base `11bb1c1e`; candidate **1.32.0**, unpublished. Public version remains **1.26.3**.

Compiled constructors now support `declare construction strip|preserve;`. The implementation-defined default is **preserve**, chosen explicitly before implementation to retain the existing fresh-element `xs:anyType` policy while repairing incomplete metadata copying. Duplicate declarations raise XQST0067; malformed and misplaced declarations retain XPST0003.

The declaring module's mode travels through static context and IR, including module functions, globals, closures and dynamic modules. Direct/computed element and document copies preserve descendant and attribute schema annotations, typed list cardinality, nilled state and ID/IDREF properties. Metadata containers and node identity are private to the copy; source nodes remain unchanged. Strip removes schema/IDREF metadata, uses untyped element/attribute values and retains the xml:id rule. Namespace-sensitive QName/NOTATION copies enforce XQTY0086 when required.

Core adds `XdmConstructionMode`, nullable `ConstructionMode` properties on construction inputs and `SourceNode` on `XdmAttributeValue`. Existing constructor/deconstruction signatures remain compatible. Custom construction providers must honor supplied modes; direct provider hooks with a null mode retain their legacy behavior. The XDocument implementation uses the existing XmlSchemaSet/IXmlSchemaInfo boundary. Bosak.Schema retains XSD 1.1 ownership; this does not accept the separately proposed ADR-006/REQ-139 callback seam.

Three narrowly required corrections are independently pinned: xs:anyType accepts untyped elements, validate expressions parse at ValueExpr precedence (postfix/path use requires parentheses), and invalid fresh xml:id values—including empty/whitespace-only values—raise XQDY0091. The fixed reference is the [XQuery 3.1 Recommendation](https://www.w3.org/TR/2017/REC-xquery-31-20170321/); semantic adoption and prerequisite scope are recorded in evidence pins. No original fixture or assertion was edited.

| Final fresh verification | Result |
|---|---|
| Solution tests, ten projects | **24,082 passed** |
| Construction regressions | **73 passed**; old 1.31.0 binaries: **67 failed / 6 passed** |
| All 87 original construction/mixed assertions, isolated executor | **87 passed** |
| Normal QT3 | **31,257 / 0 / 564** |
| Isolated construction-enabled QT3 | **31,340 / 0 / 481** |
| Normal qt4 gate | **8,939 / 0 / 1,920**, 230 gated sets |
| Previously admitted copy/context originals | **49 / 43 passed** |
| Independent public controls | **8 passed** |

Exactly 83 dependency-admissible skips execute successfully in the isolated full gate; four exact-XQ10 policy exclusions remain. Every retained normal and isolated skip reason is unchanged. Normal construction routing remains guarded. The unchanged C33 typed-copy probe now finds source/copy metadata equal across all three paths. One obsolete copy-policy unit expectation (`declare construction strip; 1` → XPST0003) was removed explicitly; all other 75 copy-policy tests remain.

Earlier failed runs are preserved separately: initial original failures exposed anyType matching and validate precedence; an intermediate gate exposed empty xml:id error mapping. They are not final qualification receipts. Raw logs, immutable prior evidence, source identities, process receipts and an executable validator accompany this report. HTML rendering checks cover the updated documentation; no screenshot/PDF acceptance is claimed.

REQ-126 remains Complete under bounded catalog acceptance; all 48 historical unmet observations remain preserved. No normal harness, baseline, dependency guard, fixture, GatedSets, release or upstream submission changes are included. Next: [C35 separate construction admission qualification](NEXT-SLICE.md).
