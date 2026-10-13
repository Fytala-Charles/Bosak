<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: deferred xsl:record-type declaration">
  <br><br><h1>xsl:record-type</h1>
  <p>Partial source implementation — constructors and verified alternatives</p>
</div>

## 1. Name and Classification

**xsl:record-type — Partially implemented in source 1.2.0; broader contract deferred.** [REQ-118 tier-4 decision](../../REQ-118-xpath-xslt-40.md) records the historical named-declaration deferral. The unreleased source now implements constructors and constructed-value nominal checks; this does not promote the complete feature. Historical baseline reviewed 2026-10-12: [revision 0d199878](https://github.com/Fytala-Charles/Bosak/commit/0d199878a9cdc1fe689c63664d8aafb7632b83aa), engine version 1.1.2.
External reference: [XSLT 4.0 named record types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-record-types). Immutable review: [QT4CG xslt.xml at e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml), IDs `named-types` and `named-record-types`. This current pin does not recover historical adopted specification provenance.

## 2. Intent

Give invoice records a reusable named identity and optional constructor.

## 3. Also Known As

Named/nominative record declarations and generated record constructors. Use the exact declaration name; similar C# types do not establish this XSLT contract.

## 4. History / Supersedes

The reviewed draft declares a nominative record type with `xsl:field` children and can generate a same-name constructor when `constructor="yes"`. It supports recursive named records and field defaults; these contracts exceed an anonymous structural map shape. Bosak's adoption dossier defers this declaration. Shipped XPath structural records, enums and choices do not supersede that decision or register named stylesheet types.

## 5. Motivation

An ERP invoice interface needs a stable contract for identifiers and currencies. Reusing a type name would centralize the definition, but the actual execution host must resolve it. These examples distinguish accepted unused XML from a working type interface.

## 6. Language Alignment

| Context | Verified outcome |
|---|---|
| Version 4.0, unused declaration | Validated in source 1.2.0; `ready` alone does not test identity |
| Version 3.0, top-level declaration | `XTSE0010` |
| Constructor call, `constructor="yes"` | Source 1.2.0 returns a named record; invoice fixture returns `INV-2048` |
| Constructor call, `constructor="no"` | `XPST0017`; no generated function |
| Named `as="erp:invoice"` binding | Constructed same-type record and plain-map coercion succeed in source candidate 1.2.0 |
| Duplicate unused fields | `XTSE4050` in source 1.2.0 |
| Structural `record(...)` in XPath40 | Explicit casts succeed; plain maps do not match annotated records |
| Inline structural record in XSLT `@as` | Observed `XTTE0570`; explicit cast in `select` plus `as="map(*)"` works |

## 7. Applicability

Use this record when deciding how to express invoice field contracts. Keep named declarations out of a supported production interface until their semantics are implemented and verified. The alternatives here use existing inline types or explicit predicates; they do not provide named package APIs.

## 8. Structure

Invoice XML → project id/currency map → explicit field validation or inline anonymous type → text report. Named-declaration use is a separate failing probe, not a step required by the alternative.

## 9. Participants

[Stylesheet](../../../src/Bosak.Xslt/Stylesheet/Stylesheet.cs) supplies known-element/top-level checks and forwards-compatible handling. [TransformEngine](../../../src/Bosak.Xslt/Runtime/TransformEngine.cs) converts local variable `@as` values. [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) evaluates complete stylesheets and reports compilation or capture errors. XPath inline type behavior is verified separately through the public XPath API.

## 10. Collaborations

`map(*)`, `map:size`, map lookup, string-length and membership comparison support the portable map contract. XSLT 4.0 selects use XPath40; XSLT 3.0 alternatives use portable maps and explicit tests. Output is a text report with no secondary outputs or external resource access.

## 11. Consequences

Anonymous structural records remain a separate shipped XPath feature. A cast drops surplus keys in the tested case; do not mistake it for complete input validation. A plain map never matches Bosak's annotated record test. The portable example validates its two projected fields explicitly; source attributes not projected into the map are outside that check. No named identity, recursive type, field default, generated constructor, package export or draft diagnostic completeness is supplied by this alternative.

## 12. Implementation

The historical released baseline registered neither declaration. Source 1.2.0 recognizes record/field declarations, registers generated constructors and installs package-local nominal identities for type tests and constructed-value bindings. `IsForwardsCompatibleElement` uses effective version greater than 3.0, so the tested unused 4.0 declarations are tolerated without implementation. Version 3.0 probes refuse top-level placement. The validation path has its own tolerance rules; these fixtures test compile/capture, not authoring-validation completeness. The constructor follow-up changes the engine; no conformance gate promotion is claimed.

## 13. Sample Code

Complete synthetic input:

```xml
<invoice id="INV-2048" currency="EUR" amount="125.50"/>
```

Complete [constructor-use fixture](../examples/xslt-type-declarations/record-constructor.xsl):

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify deferred XSLT named type declarations or portable invoice validation.
SPECIAL NOTES: Synthetic invoice projection; observed ignored declarations do not establish support.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: invoice attributes id, currency and amount. Output: text report or declared error.
These fixtures do not publish an actual named type API or perform external I/O.
-->
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:erp="urn:example:erp" xmlns:map="http://www.w3.org/2005/xpath-functions/map">
  <xsl:output method="text"/>
<xsl:record-type name="erp:invoice" constructor="yes"><xsl:field name="id" as="xs:string"/><xsl:field name="currency" as="xs:string"/></xsl:record-type>
  <!-- Context: document node; result: text or error. Local values are evaluated when used. -->
  <xsl:template match="/">
<xsl:value-of select="erp:invoice('INV-2048', 'EUR')?id"/>
  </xsl:template>
</xsl:stylesheet>
```

Current source 1.2.0 output: `INV-2048`. Changing the constructor option to `no` still produces `XPST0017`. An unused declaration produces `ready`, while constructor and nominal-identity regressions establish the supported behavior.

### XPath40 structural alternative

```xpath
let $i := parse-xml('<invoice id="INV-2048" currency="EUR" amount="125.50"/>')/invoice return let $r := map {'id': string($i/@id), 'currency': string($i/@currency)} cast as record(id as xs:string, currency as enum('EUR', 'USD')) return concat($r?id, '|', $r?currency)
```

Output:

```text
INV-2048|EUR
```

### XSLT structural binding workaround

Complete [cast-in-select fixture](../examples/xslt-type-declarations/inline-record-cast40.xsl):

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify deferred XSLT named type declarations or portable invoice validation.
SPECIAL NOTES: Synthetic invoice projection; observed ignored declarations do not establish support.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: invoice attributes id, currency and amount. Output: text report or declared error.
This workaround casts within XPath40 select, then binds as map(*); it avoids the
observed XSLT structural-record @as conversion boundary. No named type is declared.
-->
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:erp="urn:example:erp" xmlns:map="http://www.w3.org/2005/xpath-functions/map">
  <xsl:output method="text"/>

  <!-- Context: document node; result: text or error. Local values are evaluated when used. -->
  <xsl:template match="/">
<xsl:variable name="r" as="map(*)" select="map {'id': string(invoice/@id), 'currency': string(invoice/@currency)} cast as record(id as xs:string, currency as xs:string)"/><xsl:value-of select="concat($r?id, '|', $r?currency)"/>
  </xsl:template>
</xsl:stylesheet>
```

Output:

```text
INV-2048|EUR
```

The companion [inline-record40](../examples/xslt-type-declarations/inline-record40.xsl) binds the plain map directly with structural `@as` and observes `XTTE0570`. This is an XSLT conversion boundary, separate from the named-declaration deferral.

### Portable XSLT 3.0 alternative

Complete [portable validation stylesheet](../examples/xslt-type-declarations/portable-valid.xsl):

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify deferred XSLT named type declarations or portable invoice validation.
SPECIAL NOTES: Synthetic invoice projection; observed ignored declarations do not establish support.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: invoice attributes id, currency and amount. Output: text report or declared error.
These fixtures do not publish an actual named type API or perform external I/O.
-->
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:erp="urn:example:erp" xmlns:map="http://www.w3.org/2005/xpath-functions/map">
  <xsl:output method="text"/>

  <!-- Context: document node; result: text or error. Local values are evaluated when used. -->
  <xsl:template match="/">
<xsl:variable name="r" as="map(*)" select="map {'id': string(invoice/@id), 'currency': string(invoice/@currency)}"/>
    <xsl:variable name="valid" as="xs:boolean" select="map:size($r) = 2 and $r?id instance of xs:string and string-length($r?id) gt 0 and $r?currency = ('EUR', 'USD')"/>
    <xsl:value-of select="if ($valid) then concat($r?id, '|', $r?currency) else 'reject'"/>
  </xsl:template>
</xsl:stylesheet>
```

Output for the complete input above:

```text
INV-2048|EUR
```

Complete invalid-currency input:

```xml
<invoice id="INV-2048" currency="XYZ" amount="125.50"/>
```

Output:

```text
reject
```

Complete empty-ID input:

```xml
<invoice id="" currency="EUR" amount="125.50"/>
```

Output:

```text
reject
```

### Fixture inventory

| Fixture | Mode | Exact output/error marker |
|---|---|---|
| [record-unused40](../examples/xslt-type-declarations/record-unused40.xsl) | xslt40 | `ready` |
| [record-unused30](../examples/xslt-type-declarations/record-unused30.xsl) | xslt31 | `XTSE0010` |
| [record-constructor](../examples/xslt-type-declarations/record-constructor.xsl) | xslt40 | `INV-2048` |
| [record-no-constructor](../examples/xslt-type-declarations/record-no-constructor.xsl) | xslt40 | `XPST0017` |
| [record-duplicate-fields-unused-observed](../examples/xslt-type-declarations/record-duplicate-fields-unused-observed.xsl) | xslt40 | `XTSE4050` |
| [record-named-as-observed](../examples/xslt-type-declarations/record-named-as-observed.xsl) | xslt40 | `INV-2048` |
| [inline-record40](../examples/xslt-type-declarations/inline-record40.xsl) | xslt40 | `XTTE0570` |
| [record-main](../examples/xslt-type-declarations/record-main.xpath) | 40 | `INV-2048\|EUR` |
| [plain-map-not-record](../examples/xslt-type-declarations/plain-map-not-record.xpath) | 40 | `false` |
| [cast-record](../examples/xslt-type-declarations/cast-record.xpath) | 40 | `true` |
| [invalid-record-currency](../examples/xslt-type-declarations/invalid-record-currency.xpath) | 40 | `FORG0001` |
| [surplus-cast](../examples/xslt-type-declarations/surplus-cast.xpath) | 40 | `2` |
| [record-default-refusal](../examples/xslt-type-declarations/record-default-refusal.xpath) | 31 | `XPST0003` |
| [inline-record-cast40](../examples/xslt-type-declarations/inline-record-cast40.xsl) | xslt40 | `INV-2048\|EUR` |

The shared [manifest](../examples/cases.json) runs 28 cases for these two references. `error` asserts the marker from either compiler failure or failed capture; it does not promise every error is detected before transformation. Observed unused/duplicate declarations freeze tolerance, not draft conformance.

## 14. Performance

The portable example projects two strings and checks the resulting map. Inline casts allocate annotated records. No performance claim is made for deferred named-type declarations, and no benchmark is included.

## 15. Known Uses

The invoice interface is synthetic integration evidence. [RecordTypeTests](../../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs) includes `InstanceOf_PlainMap_NeverMatchesRecordType`, `InstanceOf_AnnotatedRecord_MatchesOwnType`, `Cast_DiscardsSurplusKeys`, `Cast_UncastableFieldValue_FORG0001` and `Coercion_BuildsAnnotatedRecord`. These are XPath structural-type evidence, not named XSLT declaration evidence.
No named XSLT type declaration gate promotion or full draft conformance is claimed. These published manifest cases are separate evidence from the engine tests and [4.0 gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).

## 16. Related Features

[Named item-type declarations](xslt-item-type.md), [structural records](../type-system/structural-records.md), [nominative record disposition](../type-system/nominative-records.md), [completeness audit](../COMPLETENESS-AUDIT.md) and [supplementary audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md).


**Current source follow-up — 2026-10-13:** Bosak source candidate 1.2.0 (unpublished) now recognizes xsl:record-type/xsl:field and registers constructor=yes functions. Constructors retain field order and empty entries, convert field values, supply trailing defaults and exclude static fields from arguments. Records carry declaration identity: constructed values satisfy named @as and instance-of tests; same-shaped other types and plain maps do not. Both unchanged native cases record-026/027 pass. The wider record substring sweep is 12 pass/52 fail/1 skip, including inherited structural/subtype cases. Plain-map coercion/casts, recursive record definitions, complete package exposure/function signatures and streaming conformance remain follow-ups. N05 and REQ-126 remain In Progress; original failed receipts and upstream PR #26 remain unchanged, with review/independent confirmation pending. Public release remains 1.1.3; no publication. Next: prioritize the wider named-type/coercion gaps and select body namespace declarations after checking this slice's CI.


**Nominative coercion follow-up — 2026-10-13:** Direct named bindings and function boundaries now convert maps, apply defaults/static initializers, reject surplus keys/static-field injection and retain declaration identity. The unchanged `record-named-as-observed` fixture now outputs `INV-2048`; its manifest expectation is updated to reflect implemented behavior. Earlier source notes are historical. See [separate evidence](../../../contributions/xslt40/record-type-boundaries/COERCION_RESULTS.json). Named casts, nested/recursive resolution and full package exposure remain follow-ups.


**Named record casts and nested references — 2026-10-13:** Source candidate 1.2.0 (unpublished) now supports package-local nominative record cast/castable operations. Casts retain map operands, cast each present field sequence, apply defaults to missing fields, discard surplus keys, protect static fields and attach the target declaration identity. Nested named references in binding/function signatures resolve in their declaration namespace, and nested named record fields reuse casting recursively. Fourteen new regressions cover lexical/type failures, empty/optional inputs, field cardinalities and namespace boundaries. General array/map casts, item-type alias cast expansion, recursive definitions, full package exposure and complete static diagnostics remain follow-ups. N05/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](../../../contributions/xslt40/record-type-boundaries/CAST_RESULTS.json). Catalogue counts/outcomes and original native assertions remain unchanged. Earlier slice notes are historical. Next: remaining recursive/static type-resolution and cast-container boundaries.


**Finite recursive records and container casts — 2026-10-13:** Source candidate 1.2.0 (unpublished) now casts XPath 4.0 arrays/maps by casting member/key/value sequences, retaining container shape and checking cardinality. Map-key collisions raise XQDY0137; wildcard casts retain the supplied container and empty container types reject nonempty inputs. Named record field casts reuse the shared sequence caster. Record field metadata resolves aliases and finite self/mutual named references in the declaration namespace without expanding nominative identities. Sixteen new regressions cover finite linked lists, recursive array casts, nested containers, invalid members and collisions. Arbitrary-depth resource handling, full recursive/static diagnostics, item-type alias cast expansion and package exposure remain follow-ups. N05/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](../../../contributions/xslt40/record-type-boundaries/CONTAINER_RESULTS.json). Catalogue counts/outcomes and original native assertions remain unchanged; earlier slice notes are historical. Next: item-type alias cast expansion and static named-type validation.


## Host static named-type validation — 2026-10-13

Source candidate 1.2.0 (unpublished) validates package-local named types in supported XPath 4.0 instance-of, treat, cast and castable targets when the XSLT host compiles the expression. Unknown direct/nested names raise XPST0051, including AVT expressions; undeclared prefixes retain XPST0081. Declared nominative record references are canonicalized to their expanded names, and record alternatives in choice cast targets raise XPST0080. Ordinary record-valued type-test choices remain valid. The internal host registry survives inline namespace declarations, recognizes built-in/XDM types and consults in-scope simple schema types. Twenty-one new regressions pass. Binding/field declaration validation, unsupported type grammar, complete schema variety/choice semantics and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress; published version stays 1.1.3.

See [static named-type evidence](../../../contributions/xslt40/item-type-boundaries/STATIC_TYPE_RESULTS.json). Native type-alias remains 8/24/0; unchanged contributed cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Declaration static named-type validation — 2026-10-13

Candidate 1.2.0 now validates supported XPath 4.0 sequence-type names on XSLT @as declarations at stylesheet registration, including unused variables, parameters, function results and record fields. The check runs after package-local aliases and records are registered and traverses import/include modules with each declaration's namespace context. Unknown direct/nested names raise XPST0051; undeclared binding prefixes raise XPST0081; record registration retains XTSE0280 for unbound field prefixes. Forward aliases, finite recursive record references, default type namespaces and quoted enumeration/field names remain valid. Fourteen new regressions cover these boundaries. XPath 3.1 declarations keep their existing path. Unsupported shared-walker grammar, complete schema variety semantics and package type exposure remain follow-ups; N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; no release or gate promotion.

[Verification evidence](../../../contributions/xslt40/item-type-boundaries/DECLARATION_TYPE_RESULTS.json). Native type-alias: 9 pass/23 fail/0 skip; type-alias-019 newly passes and unchanged contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Node-kind alias grammar — 2026-10-13

Candidate 1.2.0 supports node-kind aliases whose tests have empty parentheses: element(), attribute(), text(), comment(), processing-instruction(), namespace-node() and document-node(). The shared type walker also accepts whitespace and nested comments around empty tests. These types can appear within supported array/map/record/function/choice types; adjacent unknown names now receive static validation instead of being hidden by an unsupported node-kind branch. Sixteen regressions verify node identity, occurrences, structural nesting, unknown names, binding failures and non-atomic choice cast rejection. Named/wildcard/schema-annotated node tests, fn/named-parameter function type grammar, full schema semantics and package type exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version remains 1.1.3; candidate 1.2.0 stays unpublished.

[Verification evidence](../../../contributions/xslt40/item-type-boundaries/NODE_KIND_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.
