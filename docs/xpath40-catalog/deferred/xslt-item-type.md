<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: deferred xsl:item-type declaration">
  <br><br><h1>xsl:item-type</h1>
  <p>Partial source support — enumeration, array and typed map aliases</p>
</div>

## 1. Name and Classification

**xsl:item-type — Partial in source 1.2.0; the broader declaration contract is deferred.** [REQ-118 tier-4 decision](../../REQ-118-xpath-xslt-40.md) records the named-declaration deferral. This reference records the historical disposition and the current enumeration, atomic, array and typed map implementation; no broad type-system tier promotion is claimed. Bosak baseline reviewed 2026-10-12: [revision 0d199878](https://github.com/Fytala-Charles/Bosak/commit/0d199878a9cdc1fe689c63664d8aafb7632b83aa), engine version 1.1.2.
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


## Atomic and array type-expression follow-up — 2026-10-13

Source candidate 1.2.0 (unpublished) resolves the supported atomic and array xsl:item-type aliases in XPath instance-of and treat expressions, including AVTs and expression-local namespace declarations. Tests preserve alias occurrences and definition namespaces, distinguish type checking from conversion, and isolate private package definitions from same-named caller aliases. Mixed XPath 3.1 instructions still refuse these aliases. Fourteen new regressions pass; the public EnumerationItemTypes option retains its enum-only contract. General map/function/choice aliases, aliases nested in arbitrary XPath structural type syntax, complete schema/package semantics and remaining named-record coercion remain follow-ups. N04/REQ-126 remain In Progress. Public release remains 1.1.3.

After the coordinate example's `as="t:position"` conversion, `$point instance of t:position` returns `true`. `($point treat as t:position)(1)` returns the existing first coordinate. In contrast, `[3,4] instance of t:position` returns `false`: its integer members have not been converted to doubles. `[3,4] treat as t:position` raises `XPDY0050`, rather than converting those members. The alias's structural definition does not impose a two-member length requirement.

`Xslt40AliasExpressionTests` verifies these distinctions, optional/repeated alias occurrences, nested array definitions, namespace/EQName/default namespace references, inline namespace declarations, AVTs, package isolation and mixed-version refusal. The standalone public enumeration option remains enum-only. Package-local lookup does not establish public type export/expose support. No catalogue manifest case or expected outcome changed.

See [expression evidence](../../../contributions/xslt40/item-type-boundaries/EXPRESSION_RESULTS.json) and [XSLT named item types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types). Original native cases and historical receipts are preserved; the broader filtered family remains 6/26/0.


## Typed map aliases and nested references — 2026-10-13

Source candidate 1.2.0 (unpublished) supports typed map xsl:item-type aliases and nested alias references inside array/map types at XSLT binding/function boundaries and XPath instance-of/treat operands. Shared expansion respects declaration/use namespaces, occurrence indicators, quoted enumeration members, EQName URI delimiters, whitespace and nested comments. XPath 4.0 typed-map coercion recursively converts keys and values, rejects converted key collisions, preserves entry order and retains already-matching maps/record identity. The new coercion path excludes wildcard map types; their existing behavior is preserved. Twenty new regressions cover these boundaries and the documented sensor example. Function/choice/structural-record aliases, arbitrary type syntax, complete schema/package semantics and remaining named-record coercion remain follow-ups. N04/REQ-126 remain In Progress; public release remains 1.1.3.

A sensor ingestion pipeline can normalize XML attribute values to doubles while restricting known reading names. Input `<root temperature="21" humidity="55"/>` produces `<reading temperature="21" humidity="55" typed="true"/>` with this stylesheet:

```xml
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:t="urn:types" xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="#all">
  <xsl:item-type name="t:readings" as="map(t:sensor,t:value)"/>
  <xsl:item-type name="t:sensor" as="enum('temperature','humidity')"/>
  <xsl:item-type name="t:value" as="xs:double"/>
  <xsl:template match="/">
    <xsl:variable name="readings" as="t:readings"
      select="map{'temperature':root/@temperature,'humidity':root/@humidity}"/>
    <reading temperature="{$readings?temperature}" humidity="{$readings?humidity}"
      typed="{$readings instance of t:readings}"/>
  </xsl:template>
</xsl:stylesheet>
```

The forward references resolve within the declaring package. Each map key must belong to the sensor enumeration; attribute value sequences are atomized and converted to doubles. Both source attributes are assumed present because the required value type is a singleton. This map type limits keys and value types, but does not require exactly those two keys; a required-field record contract remains a separate concern. The example is covered by `MapAlias_SensorReferenceExample` and adds no manifest case.

Inline `map(t:sensor,array(t:value?))` is also expanded, preserving optional array members. `instance of`/`treat as` keep existing member types and never perform binding conversion. Converting numeric keys to a narrower type can collapse distinct keys; that fails rather than discarding an entry. A matching map is returned unchanged, retaining a nominative record's identity when no conversion is required. Wildcard-map and XPath 3.1 behavior are preserved.

See [map evidence](../../../contributions/xslt40/item-type-boundaries/MAP_RESULTS.json), [XSLT named item types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types), and [XPath item coercion rule 9](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-coercion-rules). The evidence records the immutable source review. Native expectations and historical results are preserved; broader family remains 6/26/0.


## Function and choice aliases — 2026-10-13

Source candidate 1.2.0 (unpublished) now expands function signatures and ordered choice xsl:item-type aliases, including nested references, parameter/result occurrences and outer function-item occurrences. Existing binding conversion wraps function results and tries choice alternatives in declaration order; instance-of/treat checks do not coerce. XPath 4.0 function instance-of now uses cardinality-aware matching, safely rejecting non-functions and handling empty sequences. Nineteen new regressions cover these boundaries, cycles, mixed-version refusal and a calibration example. Structural-record aliases, remaining named-record coercion and complete schema/package semantics remain follow-ups. N04/REQ-126 remain In Progress; public release remains 1.1.3.

A sensor pipeline can pass a typed calibration callback to reusable transformation code. Input `<root value="21"/>` produces `<reading>23.1</reading>`:

```xml
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:t="urn:types" xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="#all">
  <xsl:item-type name="t:calibration" as="function(t:value) as t:value"/>
  <xsl:item-type name="t:value" as="xs:double"/>
  <xsl:template match="/">
    <xsl:variable name="calibrate" as="t:calibration"
      select="fn($v as xs:double) as xs:double {$v*1.1}"/>
    <reading><xsl:value-of select="$calibrate(xs:double(root/@value))"/></reading>
  </xsl:template>
</xsl:stylesheet>
```

The alias names the callback contract: one double argument and one double result. Forward references resolve in the declaring package. The example explicitly converts the XML attribute before invoking the callback and assumes that attribute is present and numeric. It is verified by `FunctionAlias_CalibrationReferenceExample`; it adds no catalogue manifest case.

Function binding conversion can wrap a compatible function to convert its results. Return cardinality and function-item cardinality are distinct: `function() as xs:double?` permits an empty result, while an optional alias use permits an absent callback. Wrong arity or non-function bindings fail with the corresponding XSLT type error. Cyclic aliases remain static errors.

An ordered choice such as `(xs:double|xs:string)` is useful for incoming values that can be either numeric readings or textual status labels. Binding conversion tries alternatives in order, so an untyped attribute containing `12` becomes a double with this ordering and a string with the reverse ordering. `instance of` and `treat as` never perform that conversion. Choice aliases also work inside array types and function signatures.

See [function/choice evidence](../../../contributions/xslt40/item-type-boundaries/FUNCTION_CHOICE_RESULTS.json), [XSLT named item types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types), [XPath choice types](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-choice-item-types), and [item coercion](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-coercion-rules). Immutable specification sources are recorded in the evidence. Native type-alias now reports 7/25/0; broader structural/package gaps and independent processor confirmation remain outstanding.


## Structural-record aliases — 2026-10-13

Source candidate 1.2.0 (unpublished) now expands structural-record aliases and aliases nested in record fields, arrays, choices and function signatures. Quoted field names, default item()* fields, trailing commas, nested comments and declaration namespaces are preserved. Existing structural record coercion converts field values and annotates records; type checks require record values and do not coerce plain maps. Eleven new regressions cover the documented sensor example, nested contracts, function boundaries, invalid fields and cycles. Nominative record coercion, recursive named records and complete schema/package semantics remain follow-ups. N04/REQ-126 remain In Progress; published version remains 1.1.3.

A sensor ingestion contract can require a numeric reading while accepting an optional text label. Input `<root value="21"/>` produces `<reading value="21" typed="true"/>`:

```xml
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:t="urn:types" xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="#all">
  <xsl:item-type name="t:reading" as="record(value as t:number,label as xs:string?)"/>
  <xsl:item-type name="t:number" as="xs:double"/>
  <xsl:template match="/">
    <xsl:variable name="r" as="t:reading" select="map{'value':root/@value}"/>
    <reading value="{$r?value}" typed="{$r instance of t:reading}"/>
  </xsl:template>
</xsl:stylesheet>
```

Binding conversion atomizes the XML attribute, converts it to a double and creates a structural record annotation. The source attribute is assumed present and numeric. Missing required fields, extra keys and incompatible field values fail; a missing optional label is accepted. `RecordAlias_SensorExample` verifies the value and type check. This example adds no catalogue manifest case.

Aliases also work in inline `record(value as t:number)`, nested record fields and callback signatures. Type tests do not convert maps: the original plain map fails a record type test. Structural records describe field contracts; a nominative `xsl:record-type` additionally has named identity and constructors, whose remaining coercion work is separate.

See [structural-record alias evidence](../../../contributions/xslt40/item-type-boundaries/RECORD_ALIAS_RESULTS.json), [XSLT named item types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types), [XPath record types](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-record-types), and [item coercion](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-coercion-rules). Pinned specification sources and native results are recorded in the evidence; independent processor confirmation remains outstanding.


## Nominative record coercion — 2026-10-13

Source candidate 1.2.0 (unpublished) now coerces maps to package-local nominative xsl:record-type declarations at variable and function boundaries. Conversion applies field types, missing-field defaults and static initializers, rejects surplus keys and supplied static fields, preserves declaration order and annotates the result with the target identity. Existing matching records are retained. Type checks still reject plain maps and distinguish same-shaped declarations. Nine new regressions cover sensor defaults, function references, cardinality, invalid bindings and identity; the previous valid-map refusal test now rejects surplus fields. Named casts, recursive named records, nested named-type/static namespace resolution and complete package exposure remain follow-ups. N04/REQ-126 remain In Progress; published version remains 1.1.3.

A sensor record can add consistent metadata while normalizing an XML reading. Input `<root value="21"/>` produces `<reading value="21" label="sensor" kind="reading" typed="true"/>`:

```xml
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:t="urn:types" xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="#all">
  <xsl:record-type name="t:reading">
    <xsl:field name="value" as="xs:double"/>
    <xsl:field name="label" as="xs:string" select="'sensor'"/>
    <xsl:field name="kind" static="yes" as="xs:string" select="'reading'"/>
  </xsl:record-type>
  <xsl:template match="/">
    <xsl:variable name="r" as="t:reading" select="map{'value':root/@value}"/>
    <reading value="{$r?value}" label="{$r?label}" kind="{$r?kind}" typed="{$r instance of t:reading}"/>
  </xsl:template>
</xsl:stylesheet>
```

Conversion atomizes the required XML attribute and promotes it to a double. Missing label uses its initializer; kind is initialized by the declaration and cannot be supplied by the incoming map. The example assumes a present numeric attribute and is verified by `NominalCoercion_SensorExample`. It adds no catalogue manifest case.

The resulting record has the declaration's identity. An original plain map does not match the named type, and a record created for another same-shaped declaration does not match it without explicit conversion. Surplus fields, non-maps, missing required values and invalid cardinalities fail binding conversion. Function parameters/results and named function references use the same path. Defaults do not imply that arbitrary string values are cast to numbers.

See [coercion evidence](../../../contributions/xslt40/record-type-boundaries/COERCION_RESULTS.json), [XSLT named item types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types), [XPath nominative record types](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-nominative-record-types), and [item coercion rule 11](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-coercion-rules). Independent processor confirmation remains outstanding.


## Named record casts and nested references — 2026-10-13

Source candidate 1.2.0 (unpublished) now supports package-local nominative record cast/castable operations. Casts retain map operands, cast each present field sequence, apply defaults to missing fields, discard surplus keys, protect static fields and attach the target declaration identity. Nested named references in binding/function signatures resolve in their declaration namespace, and nested named record fields reuse casting recursively. Fourteen new regressions cover lexical/type failures, empty/optional inputs, field cardinalities and namespace boundaries. General array/map casts, item-type alias cast expansion, recursive definitions, full package exposure and complete static diagnostics remain follow-ups. N05/REQ-126 remain In Progress; published version remains 1.1.3.

Explicit projection is useful when a sensor message carries extra metadata or typed textual values. Input `<root value="21"/>` produces `<reading value="21" label="sensor" kind="reading" typed="true"/>`:

```xml
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:t="urn:types" xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="#all">
  <xsl:record-type name="t:reading">
    <xsl:field name="value" as="xs:double"/>
    <xsl:field name="label" as="xs:string" select="'sensor'"/>
    <xsl:field name="kind" static="yes" as="xs:string" select="'reading'"/>
  </xsl:record-type>
  <xsl:template match="/">
    <xsl:variable name="r" select="map{'value':string(root/@value),'extra':99} cast as t:reading"/>
    <reading value="{$r?value}" label="{$r?label}" kind="{$r?kind}" typed="{$r instance of t:reading}"/>
  </xsl:template>
</xsl:stylesheet>
```

The cast turns the textual value into a double, discards extra and initializes missing metadata. Binding conversion instead rejects surplus keys and does not generally cast typed strings to numbers. The example assumes a present numeric attribute; `NamedCast_SensorProjection` verifies the same map-to-record projection and defaults. It adds no catalogue manifest case.

Static fields cannot be injected by a plain map, even when the supplied value equals the initializer. Existing instances of the target declaration can retain their static fields when cast. Missing required fields and invalid field cardinalities raise type errors; invalid numeric lexical forms raise casting errors. `castable` reports false for these failures. Optional casts accept empty input. Each member of a declared field sequence is checked/cast individually before its cardinality is validated.

Nested `array(n:reading)` binding and function signatures resolve n in the declaration's namespace; nested named record fields retain declaration identity and use the cast path recursively. This does not establish general array/map casting or full recursive/static type validation.

See [cast evidence](../../../contributions/xslt40/record-type-boundaries/CAST_RESULTS.json), [XPath record casts](https://qt4cg.org/specifications/xquery-40/xpath-40.html#casting-to-record), and [XPath castable](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-castable). Immutable source review and remaining native failures are recorded separately; independent processor confirmation remains outstanding.


## Finite recursive records and container casts — 2026-10-13

Source candidate 1.2.0 (unpublished) now casts XPath 4.0 arrays/maps by casting member/key/value sequences, retaining container shape and checking cardinality. Map-key collisions raise XQDY0137; wildcard casts retain the supplied container and empty container types reject nonempty inputs. Named record field casts reuse the shared sequence caster. Record field metadata resolves aliases and finite self/mutual named references in the declaration namespace without expanding nominative identities. Sixteen new regressions cover finite linked lists, recursive array casts, nested containers, invalid members and collisions. Arbitrary-depth resource handling, full recursive/static diagnostics, item-type alias cast expansion and package exposure remain follow-ups. N05/REQ-126 remain In Progress; published version remains 1.1.3.

A route-processing stage can normalize a finite linked sequence of stops received as textual values. With source `<root/>`, this stylesheet produces `<route>1|2|true</route>`:

```xml
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:t="urn:types" xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="#all">
  <xsl:record-type name="t:list">
    <xsl:field name="value" as="xs:integer"/>
    <xsl:field name="next" as="t:list?"/>
  </xsl:record-type>
  <xsl:template match="/">
    <xsl:variable name="routes"
      select="[map{'value':'1','next':map{'value':'2'}}] cast as array(t:list)"/>
    <route><xsl:value-of separator="|"
      select="($routes?1?value,$routes?1?next?value,$routes?1 instance of t:list)"/></route>
  </xsl:template>
</xsl:stylesheet>
```

The map represents a small synthetic route payload: value holds the stop order and next links to the following stop. The array models a batch of routes. Casting converts each link's textual order to an integer and assigns the same declaration identity. A missing next field becomes an empty sequence because it is optional; the required value field must be present. `RecursiveRecord_ArrayCast` verifies this contract and output values. This example adds no catalogue manifest case and is not evidence for unlimited nesting depth or cyclic runtime values.

For a grouped batch, `[('1','2'),()] cast as array(xs:integer*)` retains two members rather than flattening the outer array. For counts indexed by textual IDs, `map{'1':('1','2')} cast as map(xs:integer,xs:integer+)` converts both the key and value sequence. Atomic casts of array-valued members still atomize those members where required. Matching function/node members can remain unchanged; general static validation of invalid cast targets is still a follow-up.

Explicit map casting preserves entry order and fails with XQDY0137 if key conversion introduces duplicates. For example, keys `1` and `01` both cast to integer 1. Failed lexical values and required member cardinalities also fail; castable reports false. Wildcard array/map casts retain their supplied containers. Empty container types accept only empty containers.

See [recursive/container evidence](../../../contributions/xslt40/record-type-boundaries/CONTAINER_RESULTS.json), [XPath array casts](https://qt4cg.org/specifications/xquery-40/xpath-40.html#casting-to-array), [map casts](https://qt4cg.org/specifications/xquery-40/xpath-40.html#casting-to-map), and [recursive record types](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-recursive-record-tests). The pinned specification and remaining native failures are recorded separately; independent processor confirmation remains outstanding.


## Item-type alias casts and declaration validation — 2026-10-13

Source candidate 1.2.0 (unpublished) expands named item-type aliases in XPath 4.0 cast and castable targets, including nested container targets and namespace forms. Atomic, enumeration, array, map and structural/nominative record aliases reuse existing casting. Optional targets preserve empty sequences. Function and generic item/node aliases raise XPST0080 as casting targets. Unknown named references in supported alias definitions raise XPST0051 during stylesheet registration; declared record references preserve nominative identity. Eighteen new regressions pass. Full schema validation, unsupported alias grammar, unknown names outside declarations, choice-target eligibility and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress; published version stays 1.1.3.

See [cast and validation evidence](../../../contributions/xslt40/item-type-boundaries/CAST_VALIDATION_RESULTS.json). Native type-alias remains 8/24/0; unchanged contributed cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.

A telemetry feed can deliver numeric readings as strings. With `<xsl:item-type name="t:reading" as="xs:integer"/>` and `<xsl:item-type name="t:batch" as="array(t:reading)"/>`, `(['42','43'] cast as t:batch)?*` yields the integers `42 43`. `['bad'] castable as t:batch` yields `false`; `() castable as t:batch?` yields `true`. This example assumes an XSLT 4.0 stylesheet with `t` bound to the declaration namespace and `xs` to XML Schema; casting reports invalid data while castable provides a validation decision.

The [XPath 4.0 casting and castable specification](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-cast) defines eligible alias targets and static diagnostics. See the pinned source and hashes in the evidence file for the implementation reference.


## Choice casting target validation — 2026-10-13

Source candidate 1.2.0 (unpublished) statically rejects XPath 4.0 cast/castable choices containing syntactically non-atomic alternatives, including arrays, maps, records, functions and node tests. Validation covers direct targets and expanded aliases, follows nested parentheses and preserves quoted separators and EQName text. Atomic/enumeration choices, optional inputs, ordinary container-valued instance-of choices and standalone container casts remain supported. Twenty-one new regressions pass. Schema-defined and nominative QName classification, unknown expression type names, broader alias grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress; published version stays 1.1.3.

See [choice casting evidence](../../../contributions/xslt40/item-type-boundaries/CHOICE_CAST_RESULTS.json). Native type-alias remains 8/24/0; unchanged contributed cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.

A logistics event feed may supply either a calendar date or a precise timestamp. Declare `<xsl:item-type name="t:event-time" as="(xs:date | xs:dateTime)"/>`, then cast the supplied string with `$raw cast as t:event-time`. Input `2026-10-13` yields an `xs:date`; input `2026-10-13T09:30:00` yields an `xs:dateTime`. The declaration order is retained. `'invalid' castable as t:event-time` yields `false`, and `() castable as t:event-time?` yields `true` when an absent time is allowed. This assumes XSLT 4.0 and the usual XML Schema/type namespaces. A mixed payload choice such as `(map(*) | array(*))` remains useful for an `instance of` test, but it raises static XPST0080 if used as a cast/castable target.

The [choice item type specification](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-choice-item-types) and [casting specification](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-cast) define the generalized atomic requirement. Pinned source revision and hashes are recorded in the separate evidence file.


## Host static named-type validation — 2026-10-13

Source candidate 1.2.0 (unpublished) validates package-local named types in supported XPath 4.0 instance-of, treat, cast and castable targets when the XSLT host compiles the expression. Unknown direct/nested names raise XPST0051, including AVT expressions; undeclared prefixes retain XPST0081. Declared nominative record references are canonicalized to their expanded names, and record alternatives in choice cast targets raise XPST0080. Ordinary record-valued type-test choices remain valid. The internal host registry survives inline namespace declarations, recognizes built-in/XDM types and consults in-scope simple schema types. Twenty-one new regressions pass. Binding/field declaration validation, unsupported type grammar, complete schema variety/choice semantics and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress; published version stays 1.1.3.

See [static named-type evidence](../../../contributions/xslt40/item-type-boundaries/STATIC_TYPE_RESULTS.json). Native type-alias remains 8/24/0; unchanged contributed cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.

For a telemetry contract, a misspelled target such as `$reading instance of t:senosr` now raises XPST0051 when that XPath expression is compiled by the host. After declaring `t:sensor` with an integer `reading` field, `(map{'reading':'42'} cast as t:sensor)?reading` yields `42`. The resulting record satisfies `instance of t:sensor`; a test against `(xs:integer | t:sensor)` can accept either payload shape. That same mixed choice is not an atomic choice and raises XPST0080 when used as a cast/castable target. These examples assume XSLT 4.0 with `t` bound to the declaration namespace.

The XSLT engine may compile an expression on first execution; a static XPath diagnostic does not imply that every unused stylesheet instruction is eagerly compiled. Validation here covers type syntax traversed by the shared helper. Binding/field declarations, schema-annotated node types and complete schema variety checks require further work.

The [named item type rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-sequencetype-syntax) and [castable static errors](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-castable) define XPST0051/XPST0081/XPST0080. Pinned source revision and hashes remain in the evidence file.


## Declaration static named-type validation — 2026-10-13

Candidate 1.2.0 now validates supported XPath 4.0 sequence-type names on XSLT @as declarations at stylesheet registration, including unused variables, parameters, function results and record fields. The check runs after package-local aliases and records are registered and traverses import/include modules with each declaration's namespace context. Unknown direct/nested names raise XPST0051; undeclared binding prefixes raise XPST0081; record registration retains XTSE0280 for unbound field prefixes. Forward aliases, finite recursive record references, default type namespaces and quoted enumeration/field names remain valid. Fourteen new regressions cover these boundaries. XPath 3.1 declarations keep their existing path. Unsupported shared-walker grammar, complete schema variety semantics and package type exposure remain follow-ups; N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; no release or gate promotion.

[Verification evidence](../../../contributions/xslt40/item-type-boundaries/DECLARATION_TYPE_RESULTS.json). Native type-alias: 9 pass/23 fail/0 skip; type-alias-019 newly passes and unchanged contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.

An unused `<xsl:variable name="reading" as="t:senosr?" select="()"/>` now raises XPST0051 during stylesheet compilation. Declaring `t:sensor` later in the same package and correcting the spelling accepts the declaration, including nested `array(t:sensor)?` references. The same name check applies to record-field @as declarations. This covers supported sequence-type syntax rather than all expressions in unused instructions. [Named item-type rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-sequencetype-syntax) define the static name boundary.


## Node-kind alias grammar — 2026-10-13

Candidate 1.2.0 supports node-kind aliases whose tests have empty parentheses: element(), attribute(), text(), comment(), processing-instruction(), namespace-node() and document-node(). The shared type walker also accepts whitespace and nested comments around empty tests. These types can appear within supported array/map/record/function/choice types; adjacent unknown names now receive static validation instead of being hidden by an unsupported node-kind branch. Sixteen regressions verify node identity, occurrences, structural nesting, unknown names, binding failures and non-atomic choice cast rejection. Named/wildcard/schema-annotated node tests, fn/named-parameter function type grammar, full schema semantics and package type exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version remains 1.1.3; candidate 1.2.0 stays unpublished.

[Verification evidence](../../../contributions/xslt40/item-type-boundaries/NODE_KIND_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.

For a document payload contract, `<xsl:item-type name="t:payload" as="array(element())"/>` allows `[/*] instance of t:payload` and preserves element identity through an @as binding. A record payload must be constructed or cast before testing its record type: `(map{'body':/*/text()} cast as record(body as text())) instance of record(body as text())`. A choice such as `(element() | t:missing)` raises XPST0051 for the unknown alternative during host compilation. [XPath node types](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-kind-tests) define these node tests. This slice covers tests with empty parentheses; QName resolution and schema annotations in node tests remain outside its scope.


## Named node-type alias grammar — 2026-10-13

Candidate 1.2.0 supports named element/attribute node tests, the complete * wildcard, and document-node(element(...)) inside package-local aliases and supported structural sequence types. Node names resolve in their declaration namespace and remain stable under use-site prefix/default namespace rebinding; unprefixed attributes retain the empty namespace. The host statically rejects unbound node prefixes and validates unknown type names beside named node tests. Explicit Q{}Name matching now checks both namespace and case-sensitive local name. Seventeen new regressions cover matching/mismatches, node identity, nested types, record fields, namespace defaults and static errors. Schema-annotated node tests, partial wildcards, processing-instruction targets, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../../contributions/xslt40/item-type-boundaries/NAMED_NODE_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.

For a namespace-qualified payload, `<xsl:item-type xmlns:p="urn:payload" name="t:payload" as="array(element(p:Root))"/>` retains `urn:payload` even if the consuming template rebinds `p`. The declaration default element namespace applies to `element(Root)`; it does not apply to `attribute(Code)`. An explicit `element(Q{}Root)` matches only the no-namespace element named `Root`. [XPath element types](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-element-test) and [attribute types](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-attribute-test) define these distinctions. Schema annotations and partial wildcards remain outside this slice.


## Processing-instruction target aliases — 2026-10-13

Candidate 1.2.0 supports processing-instruction targets in item-type aliases and supported structural sequence types. NCName and quoted-string targets retain case, normalize surrounding whitespace and are never expanded as type QNames. The parser validates targeted sequence-type syntax using a shared target reader; runtime matching compares the actual PI target. Malformed targets raise XPST0003 and quoted targets outside the NCName lexical space raise XPTY0004. Twelve new regressions verify identity, target/case mismatch, nesting, optional occurrences and static diagnostics. Partial wildcards, schema-annotated node tests, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../../contributions/xslt40/item-type-boundaries/PI_TARGET_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.

A document pipeline can declare `<xsl:item-type name="t:stylesheet-hint" as="processing-instruction('xml-stylesheet')"/>`. A binding using this alias retains the processing-instruction node and accepts only the exact target `xml-stylesheet`; `XML-stylesheet` does not match. `array(processing-instruction('xml-stylesheet'))` can express a collection of those hints. A target is a name/string rather than a type QName, so changing the default type namespace does not change its meaning. [XPath simple node types](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-kind-tests) specify PI target matching and invalid NCName diagnostics.


## Partial node wildcard aliases — 2026-10-13

Candidate 1.2.0 supports partial element/attribute name wildcards in aliases and supported structural types: *:Name matches an exact local name in any namespace; prefix:* and Q{uri}* match any local name in the specified namespace. Prefix wildcards retain declaration bindings under use-site rebinding, including document-node(element(...)) nesting. Unbound wildcard prefixes raise XPST0081; adjacent unknown type names retain XPST0051. The ##any declaration default now expands unprefixed element tests to *:Name while unprefixed attributes stay unqualified. Twenty new regressions verify identity, case/namespace mismatches, empty namespaces, nested arrays/documents and diagnostics. Node-name unions, schema-annotated tests, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../../contributions/xslt40/item-type-boundaries/NODE_WILDCARD_RESULTS.json). Native type-alias improves to 11 pass/21 fail/0 skip: wildcard-type-001/002 newly pass and original contribution cases 023/024 still pass. Broader failures and independent processor confirmation remain outstanding.

A namespace-tolerant payload contract can declare `<xsl:item-type name="t:payload" as="array(element(*:Root))"/>`: the local name must be exactly `Root`, while the namespace may vary. A contract declared as `element(p:*)` instead accepts any element name in the namespace bound to `p` at the declaration. `element(Q{}*)` accepts only elements with no namespace. The [XPath element and attribute wildcard rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-element-test) define these distinctions and the ##any element-name default. Name unions and schema annotations remain outside this slice.


## Node-name union aliases — 2026-10-13

Candidate 1.2.0 supports node-name unions in element/attribute item-type aliases and supported structural sequence types: element(doctor|nurse) and attribute(price|cost) match any listed name. Each alternative retains declaration namespaces and may use a partial wildcard, including nested document-node(element(...)), arrays and record fields. EQName namespace URIs containing pipe or comma characters remain intact. Empty alternatives raise XPST0003; an undeclared prefix in any alternative raises XPST0081; adjacent unknown type names retain XPST0051. Twenty-two new regressions cover positive/negative matching, identity, declaration prefix rebinding, wildcard combinations, nested types and ##any defaults. Schema-annotated alias traversal, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../../contributions/xslt40/item-type-boundaries/NODE_UNION_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.

A staffing contract can declare `<xsl:item-type name="t:staff" as="element(doctor|nurse)"/>`; either name is accepted, and `patient` is rejected. `element(p:*|*:nurse)` accepts any element in the namespace declared for `p`, or an element whose local name is exactly `nurse` in any namespace. The [XPath element type rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-element-test) define name unions. Schema-annotated alias traversal remains outside this slice.


## Schema-annotated node aliases — 2026-10-13

Candidate 1.2.0 supports schema annotations in element/attribute item-type aliases and supported structural sequence types, including node-name unions, nested documents, arrays and record fields. The annotation resolves independently against in-scope schema types and retains declaration namespaces under prefix rebinding; item-type aliases are not substituted into the schema annotation. Built-in and imported simple/complex schema types are accepted. Unknown annotations raise XPST0008; undeclared annotation prefixes raise XPST0081; malformed or empty annotations raise XPST0003. Only element annotations permit the nillable ? marker. The ##any default maps unprefixed schema annotations to the XML Schema namespace. Twenty-eight new regressions cover matching untyped and validated nodes, non-coercion, imported simple/complex types, nillability, namespace identity, nested contracts and unused declarations. fn/named-parameter function grammar, schema-element/schema-attribute alias traversal, document shorthand and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../../contributions/xslt40/item-type-boundaries/SCHEMA_NODE_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.

An untyped XML contract can declare `<xsl:item-type name="t:raw" as="element(root,xs:untyped)"/>`. A validated contract can use an imported schema type, for example `element(*:root,s:Label?)`; the `?` accepts a nilled element without making the sequence optional. An attribute annotation does not allow this marker. An item-type alias cannot replace the schema type after the comma. The [XPath element type rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-element-test) define these annotation and namespace requirements.


## Function-type grammar aliases — 2026-10-13

Candidate 1.2.0 accepts fn(...) as a synonym for function(...) and documentary parameter names in supported function-type aliases, direct XPath type operands and XSLT binding signatures. Names do not affect matching or coercion; duplicate expanded names raise XQST0039, including different prefixes bound to the same namespace and unprefixed names paired with Q{} names. Undeclared documentary prefixes raise XPST0081; malformed named parameters raise XPST0003; unknown parameter/result item types retain XPST0051. Named and unnamed parameters may be mixed, with independent name scopes for nested function types. Nested arrays, records and higher-order signatures retain parameter/result sequence occurrences. XPath 3.1 rejects fn type synonyms and named type parameters. Twenty-six new regressions cover callback conversion, wildcard maps/arrays, direct bindings without aliases, direct host evaluation, nesting, namespace identity, diagnostics and version boundaries. Annotated function types, schema-element/schema-attribute alias traversal, document shorthand and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../../contributions/xslt40/item-type-boundaries/FUNCTION_GRAMMAR_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.

A callback contract can declare `<xsl:item-type name="t:callback" as="fn($value as xs:integer) as xs:double"/>`. A supplied callback may use a different parameter name; the documented name does not become a keyword in calls. A binding without aliases can use the same signature directly. The [XPath function type rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-function-test) define fn/function equivalence, documentary names and duplicate-name diagnostics.


## Schema-declaration and document shorthand aliases — 2026-10-13

Candidate 1.2.0 supports schema-element(Name), schema-attribute(Name) and document-node(schema-element(Name)) in item-type aliases and supported structural sequence types. Schema declaration names retain their declaration namespace under prefix rebinding and must resolve to imported global elements/attributes; unknown or unavailable declarations raise XPST0008 and undeclared prefixes raise XPST0081. Wildcards, unions and empty declaration names raise XPST0003. Schema-element under ##any uses no namespace for an unprefixed declaration; unprefixed schema attributes always use no namespace. Document shorthand such as document-node(root|other) normalizes to document-node(element(root|other)), including wildcard and EQName alternatives, and reuses single-root matching. Direct host type operands and alias-free XSLT document/schema bindings normalize the new forms. Twenty-nine new regressions cover node identity, namespace mismatches, imported declarations, substitution members, typed attribute matching, rejection of untyped nodes, nested arrays, ##any defaults, empty documents and static validation of unused bindings. Annotated function types, package exposure and broader native type-alias gaps remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../../contributions/xslt40/item-type-boundaries/SCHEMA_DECLARATION_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.

An imported document contract can declare `<xsl:item-type name="t:document" as="document-node(schema-element(s:root))"/>`; its root must match the imported declaration, including permitted substitution members. A schema-free contract can use `document-node(root|other)` or `document-node(*:root)`. The ##any default applies differently: an unprefixed ordinary root name becomes a local-name wildcard, while an unprefixed schema-element name identifies a declaration in no namespace. The [XPath node type rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html) define these distinctions.


## Package named type exposure — 2026-10-13

Candidate 1.2.0 accepts item-type and record-type components in xsl:expose and xsl:accept. Public or exposed named types enter using-package type registries unless accepted as hidden; private dependencies retain the defining package's resolved alias definitions and record metadata. Accept names resolve namespaces on the accept element. Missing component names, incompatible acceptance, invalid declaration visibility and collisions with local types produce static errors. Seventeen regressions cover public declarations, explicit and wildcard exposure, private and hidden names, prefix scope, record constructors, private alias dependencies rejected collisions and public/private visibility through an intermediate package. Native type-alias improves from 11/21/0 to 15/17/0; newly passing cases are 018, 020, 021 and 022. Original contribution cases 023/024 remain green. Broader native failures and independent processor confirmation remain outstanding; N04/N05 and REQ-126 stay In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../../contributions/xslt40/item-type-boundaries/PACKAGE_EXPOSURE_RESULTS.json). The [XSLT 4.0 named type and package rules](https://qt4cg.org/specifications/xslt-40/Overview.html) define these visibility boundaries.


## Fixed namespace type contexts — 2026-10-13

Candidate 1.2.0 resolves fixed-namespaces for named item/record type names, supported nested type references, static declaration validation and the main XPath compilation paths. Supported tokens include #standard, selected root-native or conventional prefixes, explicit prefix=URI bindings and namespace-document references through the existing stylesheet resolver. Bindings are module-local, cached at compilation, unaffected by descendant rebinding and applied in token order. Invalid bindings or unavailable namespace documents raise XTSE0122. Unprefixed type/node names retain their existing default-namespace rules; fixed bindings do not create native result namespaces. Sixteen regressions cover standard prefixes, nested node contracts, explicit/repeated bindings, namespace documents, include isolation and invalid/unavailable bindings. Native type-alias remains 15/17/0: case 016 now passes namespace resolution but still fails its independent inline-function record conversion contract with XPTY0004. No upstream assertion changes. Fixed namespace coverage of other QName-bearing XSLT attributes, pattern compilation and shadow/static processing remains a follow-up; this is not complete fixed-namespaces support. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../../contributions/xslt40/item-type-boundaries/FIXED_NAMESPACE_RESULTS.json). The [XSLT 4.0 fixed-namespaces rules](https://qt4cg.org/specifications/xslt-40/Overview.html#fixed-namespaces) define the namespace token and scope rules.
