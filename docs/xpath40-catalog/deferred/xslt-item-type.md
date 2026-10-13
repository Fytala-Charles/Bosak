<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: deferred xsl:item-type declaration">
  <br><br><h1>xsl:item-type</h1>
  <p>Partial source support — enumeration and array bindings</p>
</div>

## 1. Name and Classification

**xsl:item-type — Partial in source 1.2.0; the broader declaration contract is deferred.** [REQ-118 tier-4 decision](../../REQ-118-xpath-xslt-40.md) records the named-declaration deferral. This reference records the historical disposition and the current enumeration and atomic/array binding implementation; no broad type-system tier promotion is claimed. Bosak baseline reviewed 2026-10-12: [revision 0d199878](https://github.com/Fytala-Charles/Bosak/commit/0d199878a9cdc1fe689c63664d8aafb7632b83aa), engine version 1.1.2.
External reference: [XSLT 4.0 named item types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types). Immutable review: [QT4CG xslt.xml at e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml), IDs `named-types` and `named-item-types`. This current pin does not recover historical adopted specification provenance.

## 2. Intent

Give a permitted currency set a reusable name for stylesheet interfaces.

## 3. Also Known As

Named item-type aliases and reusable enumeration/choice/function type abbreviations. Use the exact declaration name; similar C# types do not establish this XSLT contract.

## 4. History / Supersedes

The reviewed draft lets `xsl:item-type` name an existing item type. The name acts as an abbreviation within the package, with visibility/import-precedence rules; it does not establish a generated record constructor. Recursive aliases are not permitted. Bosak's adoption dossier defers this declaration. The released baseline does not register named stylesheet types. Source candidate 1.2.0 adds enumeration aliases for XPath instance-of/treat and XSLT variable/parameter/result conversion; the broader declaration contract remains deferred.

## 5. Motivation

An ERP invoice interface needs a stable contract for identifiers and currencies. Reusing a type name would centralize the definition, but the actual execution host must resolve it. These examples distinguish accepted unused XML from a working type interface.

## 6. Language Alignment

| Context | Verified outcome |
|---|---|
| Version 4.0, unused enumeration declaration | Validated and registered in source 1.2.0; `ready` alone does not test membership |
| Version 3.0, top-level declaration | `XTSE0010` |
| Named `as="erp:currency"`, EUR input | `EUR` in source 1.2.0 |
| Named `as="erp:currency"`, XYZ input | `XTTE0570`; alias membership rejects XYZ |
| `instance of erp:currency` | `true` for EUR in source 1.2.0 |
| Duplicate unused declarations | `XTSE4030` in source 1.2.0 |
| Inline `enum('EUR','USD')` in XSLT 4.0 `@as` | EUR succeeds; XYZ produces `XTTE0570` |
| XPath31 enum syntax | `XPST0003` |

## 7. Applicability

Use this record when deciding how to express invoice field contracts. Named enumeration bindings are verified in the unpublished source candidate; wider alias definitions and package contracts still require implementation and verification. The alternatives here use existing inline types or explicit predicates; they do not provide named package APIs.

## 8. Structure

Invoice XML → project id/currency map → explicit field validation or inline anonymous type → text report. The named-enumeration fixture now validates the currency binding; portable alternatives remain available for XSLT 3.0.

## 9. Participants

[Stylesheet](../../../src/Bosak.Xslt/Stylesheet/Stylesheet.cs) supplies known-element/top-level checks and forwards-compatible handling. [TransformEngine](../../../src/Bosak.Xslt/Runtime/TransformEngine.cs) converts local variable `@as` values. [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) evaluates complete stylesheets and reports compilation or capture errors. XPath inline type behavior is verified separately through the public XPath API.

## 10. Collaborations

`map(*)`, `map:size`, map lookup, string-length and membership comparison support the portable map contract. XSLT 4.0 selects use XPath40; XSLT 3.0 alternatives use portable maps and explicit tests. Output is a text report with no secondary outputs or external resource access.

## 11. Consequences

For a small currency set, use an inline enumeration only in a verified 4.0 binding, or an explicit membership check in portable 3.0. The portable report rejects XYZ and empty IDs. It projects selected input attributes; it does not validate every source attribute or the monetary amount. An unused declaration cannot enforce currency membership, and broader named-type conversion failures are not successful alias validation.

## 12. Implementation

The historical released baseline registered neither declaration. Source 1.2.0 recognizes xsl:item-type and supplies enumeration aliases to XPath instance-of/treat compilation and declaration conversion; Source 1.2.0 also supplies the separate [named-record constructor slice](xslt-record-type.md), with broader record coercion and package semantics still unfinished. The earlier unused-declaration outputs alone were tolerance evidence. Current enumeration declarations are validated and registered; binding and membership regressions establish the supported source behavior. Version 3.0 probes refuse top-level placement. The validation path has its own tolerance rules; these fixtures test compile/capture, not authoring-validation completeness. The enumeration follow-up changes the engine; no conformance gate promotion is claimed.

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
PURPOSE: Validate an invoice currency against a named enumeration.
SPECIAL NOTES: Named enumeration binding is verified in source 1.2.0; broader aliases remain partial.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
                Charles Korthout | 0.2 | 13-10-2026 | Named currency binding verified; EUR succeeds, invalid members refuse
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

Current source 1.2.0 outcome: `EUR` for the supplied invoice. An otherwise identical invoice with currency `XYZ` raises `XTTE0570`. The unchanged output `ready` from an unused declaration is not membership evidence.

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
| [item-used-valid](../examples/xslt-type-declarations/item-used-valid.xsl) | xslt40 | `EUR` |
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

The shared [manifest](../examples/cases.json) runs 28 cases for these two references. `error` asserts the marker from either compiler failure or failed capture; it does not promise every error is detected before transformation. Historical unused outputs alone were tolerance evidence; duplicate declarations and named bindings now have separate current regressions.

## 14. Performance

The portable example projects two strings and checks the resulting map. Inline casts allocate annotated records. No performance claim is made for deferred named-type declarations, and no benchmark is included.

## 15. Known Uses

The invoice interface is synthetic integration evidence. [VersionGateTests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) includes `Compile_EnumType_31Mode_ThrowsXpst0003`, `Evaluate_Enum_InstanceOf_MembershipIsCodepointSensitive`, `Evaluate_Enum_Cast_MemberSucceedsAsPlainString` and `Evaluate_Enum_Cast_NonMemberFailsForg0001`. The anonymous XPath evidence is complemented by [named binding regressions](../../../tests/Bosak.Xslt.Tests/Xslt40EnumerationBindingTests.cs).
No named XSLT type declaration gate promotion or full draft conformance is claimed. These published manifest cases are separate evidence from the engine tests and [4.0 gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).

## 16. Related Features

[Named record declarations](xslt-record-type.md), [structural records](../type-system/structural-records.md), [nominative record disposition](../type-system/nominative-records.md), [completeness audit](../COMPLETENESS-AUDIT.md) and [supplementary audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md).


**Current source follow-up — 2026-10-12:** Bosak source candidate 1.2.0 (unpublished) now resolves xsl:item-type enumeration aliases in XPath instance-of and treat expressions, including AVTs, namespace rebinding, EQNames and occurrence indicators. Both unchanged native cases type-alias-023/024 pass; broader type-alias-filter run is 3 pass/29 fail/0 skip (includes inherited related cases). Other alias types, named XSLT @as conversion, package exposure and complete recursive/schema-collision validation remain unsupported. Duplicate winning declarations raise XTSE4030; lower-precedence definitions remain overridable. Original failure receipts are preserved; upstream PR #25 review and independent confirmation are pending. N04 and REQ-126 remain In Progress; public release remains 1.1.3. Next: generated nominative record constructors, with wider alias support and select namespace declarations tracked separately.


### Verified declaration boundaries in source 1.2.0

Named enumerations abbreviate the same literal member set throughout the package. They produce ordinary string values, with no new record-like nominal identity. Optional/multiple occurrence indicators still constrain the result sequence. Source attributes are atomized and untyped atomic values converted before checking membership; an arbitrary typed integer is not accepted merely because it can be printed as a member string.

| Declaration boundary | Membership/cardinality error |
|---|---|
| Variable | `XTTE0570` |
| Template parameter | `XTTE0590` |
| Function parameter | `XPTY0004` |
| Function result | `XTTE0780` |
| xsl:select result | `XPTY0004` |

The definition remains package-local. A public function can use its declaring package's private enumeration internally; the caller's same-named enumeration is restored afterwards. This is isolation evidence, not implementation of export/expose/imported type visibility. Structural aliases, alias recursion checks, schema collisions and complete package type semantics remain deferred.


**Binding follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) now converts named enumeration aliases at XSLT variable, parameter and result boundaries. Namespace/EQName aliases and occurrence indicators resolve from the declaring instruction; atomization and membership use XPath function conversion rules. Direct/dynamic function metadata uses the same definitions; private package aliases are isolated and restored. Sixteen regressions cover valid values, membership/cardinality errors, source attributes, record fields, template/select/function boundaries and mixed-version refusal. The invoice currency fixture now returns EUR; invalid XYZ still raises XTTE0570. Original native type-alias-023/024 still pass; broader filtered family remains 3/29/0, with structural/recursive/schema/package gaps outstanding. N04 and REQ-126 stay In Progress; no complete alias or package-conformance claim. Public release remains 1.1.3. See [separate evidence](../../../contributions/xslt40/item-type-boundaries/BINDING_RESULTS.json).


## Atomic and array binding follow-up — 2026-10-13

Source candidate 1.2.0 (unpublished) supports atomic and array item-type aliases at XSLT declaration and function boundaries. Array member types can use forward aliases, nested arrays, optional member sequences and enumeration aliases. Definition namespace bindings are preserved; outer occurrence indicators apply to arrays rather than their members. Supported direct/indirect recursive aliases raise XTSE4035. XPath 4.0 array coercion converts member sequences without atomizing the outer array; the XPath 3.1 path remains unchanged. Native type-alias-001/002 now pass; the filtered family is 6 pass/26 fail/0 skip, including unchanged contribution cases 023/024. Non-enumeration aliases in XPath instance-of/treat, general map/function/choice aliases, complete recursive validation, schema collisions and package exposure remain follow-ups. N04/REQ-126 remain In Progress. Public release remains 1.1.3.

The alias abbreviates a structural type; it does not constrain the array length. For a coordinate pipeline, input `<root x="3" y="4"/>` becomes `<distance>5</distance>` using this complete stylesheet:

```xml
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:t="urn:types" xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math" exclude-result-prefixes="#all">
  <xsl:item-type name="t:position" as="array(xs:double)"/>
  <xsl:template match="/">
    <xsl:variable name="point" as="t:position" select="[root/@x,root/@y]"/>
    <distance><xsl:value-of select="math:sqrt($point(1)*$point(1)+$point(2)*$point(2))"/></distance>
  </xsl:template>
</xsl:stylesheet>
```

The variable conversion atomizes the attribute members and converts their untyped values to doubles. One-based array access supplies the coordinates to the distance calculation. This example assumes both attributes exist; missing coordinates fail the required member type. `array(xs:double?)` would admit empty member sequences, while `t:position?` permits an absent array. These declarations have different meanings. The example is covered by `ArrayAlias_CoordinateReferenceExample`; it adds no catalogue manifest case or broad conformance claim.

See [array binding evidence](../../../contributions/xslt40/item-type-boundaries/ARRAY_BINDING_RESULTS.json), [named item-type specification](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types), and [XPath 4.0 function conversion](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-function-conversion-rules). The pinned specification source is recorded in the evidence. Native type-alias-004 still expects the processor-specific `SXTA0001`; Bosak reports the draft's `XTSE4035` for its circular definition. That remains a native failure, with the original expected error preserved.
