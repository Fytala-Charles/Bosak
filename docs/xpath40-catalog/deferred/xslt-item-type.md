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
