<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: deferred xsl:item-type declaration">
  <br><br><h1>xsl:item-type</h1>
  <p>Deferred — named XSLT declarations and verified alternatives</p>
</div>

## 1. Name and Classification

**xsl:item-type — Deferred.** [REQ-118 tier-4 decision](../../REQ-118-xpath-xslt-40.md) records the named-declaration deferral. This is a disposition reference, not a type implementation or tier promotion. Bosak baseline reviewed 2026-10-12: [revision 0d199878](https://github.com/Fytala-Charles/Bosak/commit/0d199878a9cdc1fe689c63664d8aafb7632b83aa), engine version 1.1.2.
External reference: [XSLT 4.0 named item types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types). Immutable review: [QT4CG xslt.xml at e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml), IDs `named-types` and `named-item-types`. This current pin does not recover historical adopted specification provenance.

## 2. Intent

Give a permitted currency set a reusable name for stylesheet interfaces.

## 3. Also Known As

Named item-type aliases and reusable enumeration/choice/function type abbreviations. Use the exact declaration name; similar C# types do not establish this XSLT contract.

## 4. History / Supersedes

The reviewed draft lets `xsl:item-type` name an existing item type. The name acts as an abbreviation within the package, with visibility/import-precedence rules; it does not establish a generated record constructor. Recursive aliases are not permitted. Bosak's adoption dossier defers this declaration. The released baseline does not register named stylesheet types. Source candidate 1.2.0 adds enumeration aliases for XPath instance-of and treat expressions; the broader declaration contract remains deferred.

## 5. Motivation

An ERP invoice interface needs a stable contract for identifiers and currencies. Reusing a type name would centralize the definition, but the actual execution host must resolve it. These examples distinguish accepted unused XML from a working type interface.

## 6. Language Alignment

| Context | Verified outcome |
|---|---|
| Version 4.0, unused enumeration declaration | Validated and registered in source 1.2.0; `ready` alone does not test membership |
| Version 3.0, top-level declaration | `XTSE0010` |
| Named `as="erp:currency"`, EUR or XYZ input | `XTTE0570`; neither uses a registered alias |
| `instance of erp:currency` | `true` for EUR in source 1.2.0 |
| Duplicate unused declarations | `XTSE4030` in source 1.2.0 |
| Inline `enum('EUR','USD')` in XSLT 4.0 `@as` | EUR succeeds; XYZ produces `XTTE0570` |
| XPath31 enum syntax | `XPST0003` |

## 7. Applicability

Use this record when deciding how to express invoice field contracts. Keep named declarations out of a supported production interface until their semantics are implemented and verified. The alternatives here use existing inline types or explicit predicates; they do not provide named package APIs.

## 8. Structure

Invoice XML → project id/currency map → explicit field validation or inline anonymous type → text report. Named-declaration use is a separate failing probe, not a step required by the alternative.

## 9. Participants

[Stylesheet](../../../src/Bosak.Xslt/Stylesheet/Stylesheet.cs) supplies known-element/top-level checks and forwards-compatible handling. [TransformEngine](../../../src/Bosak.Xslt/Runtime/TransformEngine.cs) converts local variable `@as` values. [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) evaluates complete stylesheets and reports compilation or capture errors. XPath inline type behavior is verified separately through the public XPath API.

## 10. Collaborations

`map(*)`, `map:size`, map lookup, string-length and membership comparison support the portable map contract. XSLT 4.0 selects use XPath40; XSLT 3.0 alternatives use portable maps and explicit tests. Output is a text report with no secondary outputs or external resource access.

## 11. Consequences

For a small currency set, use an inline enumeration only in a verified 4.0 binding, or an explicit membership check in portable 3.0. The portable report rejects XYZ and empty IDs. It projects selected input attributes; it does not validate every source attribute or the monetary amount. An unused declaration cannot enforce currency membership, and named-type conversion failure is not successful alias validation.

## 12. Implementation

The historical released baseline registered neither declaration. Source 1.2.0 recognizes xsl:item-type and supplies enumeration aliases to XPath instance-of/treat compilation; Source 1.2.0 also supplies the separate [named-record constructor slice](xslt-record-type.md), with broader record coercion and package semantics still unfinished. `IsForwardsCompatibleElement` uses effective version greater than 3.0, so the tested unused 4.0 declarations are tolerated without implementation. Version 3.0 probes refuse top-level placement. The validation path has its own tolerance rules; these fixtures test compile/capture, not authoring-validation completeness. The enumeration follow-up changes the engine; no conformance gate promotion is claimed.

## 13. Sample Code

Complete synthetic input:

```xml
<invoice id="INV-2048" currency="EUR" amount="125.50"/>
```

Complete [named alias-use fixture](../examples/xslt-type-declarations/item-used-valid.xsl):

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
<xsl:item-type name="erp:currency" as="enum('EUR', 'USD')"/>
  <!-- Context: document node; result: text or error. Local values are evaluated when used. -->
  <xsl:template match="/">
<xsl:variable name="c" as="erp:currency" select="string(invoice/@currency)"/><xsl:value-of select="$c"/>
  </xsl:template>
</xsl:stylesheet>
```

Observed outcome: `XTTE0570`; even the EUR input does not bind through the named alias. The XYZ input has the same error. The unused-declaration fixture returns `ready`; the missing alias is exposed only by actual use.

### Inline enumeration alternative

Complete [inline enum fixture](../examples/xslt-type-declarations/inline-enum40.xsl):

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

  <!-- Context: document node; result: text or error. Local values are evaluated when used. -->
  <xsl:template match="/">
<xsl:variable name="c" as="enum('EUR', 'USD')" select="string(invoice/@currency)"/><xsl:value-of select="$c"/>
  </xsl:template>
</xsl:stylesheet>
```

Output:

```text
EUR
```

Using the invalid input below yields `XTTE0570`, which verifies the anonymous enumeration boundary.

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
| [item-unused40](../examples/xslt-type-declarations/item-unused40.xsl) | xslt40 | `ready` |
| [item-unused30](../examples/xslt-type-declarations/item-unused30.xsl) | xslt31 | `XTSE0010` |
| [item-used-valid](../examples/xslt-type-declarations/item-used-valid.xsl) | xslt40 | `XTTE0570` |
| [item-used-invalid-observed](../examples/xslt-type-declarations/item-used-invalid-observed.xsl) | xslt40 | `XTTE0570` |
| [item-duplicate-unused-observed](../examples/xslt-type-declarations/item-duplicate-unused-observed.xsl) | xslt40 | `XTSE4030` |
| [item-instance](../examples/xslt-type-declarations/item-instance.xsl) | xslt40 | `true` |
| [portable-valid](../examples/xslt-type-declarations/portable-valid.xsl) | xslt31 | `INV-2048\|EUR` |
| [portable-invalid](../examples/xslt-type-declarations/portable-invalid.xsl) | xslt31 | `reject` |
| [portable-empty-id](../examples/xslt-type-declarations/portable-empty-id.xsl) | xslt31 | `reject` |
| [inline-enum40](../examples/xslt-type-declarations/inline-enum40.xsl) | xslt40 | `EUR` |
| [inline-enum-invalid40](../examples/xslt-type-declarations/inline-enum-invalid40.xsl) | xslt40 | `XTTE0570` |
| [enum-valid](../examples/xslt-type-declarations/enum-valid.xpath) | 40 | `true` |
| [enum-invalid](../examples/xslt-type-declarations/enum-invalid.xpath) | 40 | `false` |
| [enum-default-refusal](../examples/xslt-type-declarations/enum-default-refusal.xpath) | 31 | `XPST0003` |

The shared [manifest](../examples/cases.json) runs 28 cases for these two references. `error` asserts the marker from either compiler failure or failed capture; it does not promise every error is detected before transformation. Observed unused/duplicate declarations freeze tolerance, not draft conformance.

## 14. Performance

The portable example projects two strings and checks the resulting map. Inline casts allocate annotated records. No performance claim is made for deferred named-type declarations, and no benchmark is included.

## 15. Known Uses

The invoice interface is synthetic integration evidence. [VersionGateTests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) includes `Compile_EnumType_31Mode_ThrowsXpst0003`, `Evaluate_Enum_InstanceOf_MembershipIsCodepointSensitive`, `Evaluate_Enum_Cast_MemberSucceedsAsPlainString` and `Evaluate_Enum_Cast_NonMemberFailsForg0001`. These are anonymous XPath enumeration evidence, not named XSLT alias evidence.
No named XSLT type declaration gate promotion or full draft conformance is claimed. These published manifest cases are separate evidence from the engine tests and [4.0 gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).

## 16. Related Features

[Named record declarations](xslt-record-type.md), [structural records](../type-system/structural-records.md), [nominative record disposition](../type-system/nominative-records.md), [completeness audit](../COMPLETENESS-AUDIT.md) and [supplementary audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md).


**Current source follow-up — 2026-10-12:** Bosak source candidate 1.2.0 (unpublished) now resolves xsl:item-type enumeration aliases in XPath instance-of and treat expressions, including AVTs, namespace rebinding, EQNames and occurrence indicators. Both unchanged native cases type-alias-023/024 pass; broader type-alias-filter run is 3 pass/29 fail/0 skip (includes inherited related cases). Other alias types, named XSLT @as conversion, package exposure and complete recursive/schema-collision validation remain unsupported. Duplicate winning declarations raise XTSE4030; lower-precedence definitions remain overridable. Original failure receipts are preserved; upstream PR #25 review and independent confirmation are pending. N04 and REQ-126 remain In Progress; public release remains 1.1.3. Next: generated nominative record constructors, with wider alias support and select namespace declarations tracked separately.
