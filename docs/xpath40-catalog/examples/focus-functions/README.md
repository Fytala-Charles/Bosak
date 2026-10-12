<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak supplier delivery constructor examples">
  <br><br><h1>Typed Supplier Delivery Import</h1>
  <p>Complete reports and the 49-name focus-constructor audit</p>
</div>

## Business case

A warehouse receives delivery lines as XML attributes. Before downstream processing, convert quantity to integer, unit price to decimal, promised date to date and active status to Boolean. The report preserves the supplier line ID and document order. `2.50` becomes canonical `2.5`; this is numeric conversion, not currency display formatting. No rounding, totals or missing-field acceptance policy is implied.

The [catalog entry](../../operators-and-syntax/focus-functions.md#13-sample-code) shows complete input, runnable 4.0 expression and output. Each XPath file embeds its XML using parse-xml: no file resolver, external variables or host context item is required. This deliberately tests node atomization as well as lexical conversion. The `!` operator sets each selected attribute as context for its constructor. In 3.1 the equivalent expression passes that attribute explicitly as `(.)`.

| Scenario | Frozen 4.0 file | Portable 3.1 file | Result |
|---|---|---|---|
| Two delivery lines | [import](audit/import-40.xpath) | [import](audit/import-31.xpath) | `A\|12\|2.5\|2026-10-12\|true;B\|5\|4\|2026-10-13\|false` |
| Empty delivery | [empty](audit/import-empty-40.xpath) | [empty](audit/import-empty-31.xpath) | Empty string |
| Reversed line order | [reordered](audit/import-reordered-40.xpath) | [reordered](audit/import-reordered-31.xpath) | B then A, unchanged field values |
| Quantity is twelve | [invalid](audit/import-invalid-40.xpath) | [invalid](audit/import-invalid-31.xpath) | FORG0001 |

## All registered constructor names

The [machine-readable audit](../../audit-focus-constructors.json) matches all 49 xs names in the scoped registered-name inventory. The table links each exact representative value expression. Separate files beside it use `<name>-invalid`, `-missing`, `-empty`, `-multiple`, `-before`, `-type`, and `-argument-empty` suffixes. The [manifest](../cases.json) declares every ID, mode, output/error and extra experimental/3.1 gate checks.

| Constructor | Input | Current output/error | Type check |
|---|---|---|---|
| [`xs:ENTITIES`](audit/ENTITIES-valid.xpath) | `A B` | `A\|B` | Pass |
| [`xs:ENTITY`](audit/ENTITY-valid.xpath) | `PART_A` | `PART_A` | Pass |
| [`xs:ID`](audit/ID-valid.xpath) | `PART_A` | `PART_A` | Pass |
| [`xs:IDREF`](audit/IDREF-valid.xpath) | `PART_A` | `PART_A` | Pass |
| [`xs:IDREFS`](audit/IDREFS-valid.xpath) | `A B` | `A\|B` | Pass |
| [`xs:NCName`](audit/NCName-valid.xpath) | `PART_A` | `PART_A` | Pass |
| [`xs:NMTOKEN`](audit/NMTOKEN-valid.xpath) | `PART_A` | `PART_A` | Pass |
| [`xs:NMTOKENS`](audit/NMTOKENS-valid.xpath) | `A B` | `A\|B` | Pass |
| [`xs:Name`](audit/Name-valid.xpath) | `PART_A` | `PART_A` | Pass |
| [`xs:QName`](audit/QName-valid.xpath) | [`xs:integer`](audit/integer-valid.xpath) | [`xs:integer`](audit/integer-valid.xpath) | Pass |
| [`xs:anyURI`](audit/anyURI-valid.xpath) | `https://supplier.example/invoice/42` | `https://supplier.example/invoice/42` | Pass |
| [`xs:base64Binary`](audit/base64Binary-valid.xpath) | `QUI=` | `QUI=` | Pass |
| [`xs:boolean`](audit/boolean-valid.xpath) | `true` | `true` | Pass |
| [`xs:byte`](audit/byte-valid.xpath) | `12` | `12` | Pass |
| [`xs:date`](audit/date-valid.xpath) | `2026-10-12` | `2026-10-12` | Pass |
| [`xs:dateTime`](audit/dateTime-valid.xpath) | `2026-10-12T09:30:00Z` | `2026-10-12T09:30:00Z` | Pass |
| [`xs:dateTimeStamp`](audit/dateTimeStamp-valid.xpath) | `2026-10-12T09:30:00Z` | `2026-10-12T09:30:00Z` | Pass |
| [`xs:dayTimeDuration`](audit/dayTimeDuration-valid.xpath) | `P1DT2H` | `P1DT2H` | Pass |
| [`xs:decimal`](audit/decimal-valid.xpath) | `12.50` | `12.5` | Pass |
| [`xs:double`](audit/double-valid.xpath) | `12.5` | `12.5` | Pass |
| [`xs:duration`](audit/duration-valid.xpath) | `P1DT2H` | `P1DT2H` | Pass |
| [`xs:error`](audit/error-valid.xpath) | `PART_A` | `FORG0001` | No non-empty value |
| [`xs:float`](audit/float-valid.xpath) | `12.5` | `12.5` | Pass |
| [`xs:gDay`](audit/gDay-valid.xpath) | `---12` | `---12` | Pass |
| [`xs:gMonth`](audit/gMonth-valid.xpath) | `--10` | `--10` | Pass |
| [`xs:gMonthDay`](audit/gMonthDay-valid.xpath) | `--10-12` | `--10-12` | Pass |
| [`xs:gYear`](audit/gYear-valid.xpath) | `2026` | `2026` | Pass |
| [`xs:gYearMonth`](audit/gYearMonth-valid.xpath) | `2026-10` | `2026-10` | Pass |
| [`xs:hexBinary`](audit/hexBinary-valid.xpath) | `4142` | `4142` | Pass |
| [`xs:int`](audit/int-valid.xpath) | `12` | `12` | Pass |
| [`xs:integer`](audit/integer-valid.xpath) | `12` | `12` | Pass |
| [`xs:language`](audit/language-valid.xpath) | `en-GB` | `en-GB` | Pass |
| [`xs:long`](audit/long-valid.xpath) | `12` | `12` | Pass |
| [`xs:negativeInteger`](audit/negativeInteger-valid.xpath) | `-12` | `-12` | Pass |
| [`xs:nonNegativeInteger`](audit/nonNegativeInteger-valid.xpath) | `12` | `12` | Pass |
| [`xs:nonPositiveInteger`](audit/nonPositiveInteger-valid.xpath) | `-12` | `-12` | Pass |
| [`xs:normalizedString`](audit/normalizedString-valid.xpath) | `supplier A` | `supplier A` | Pass |
| [`xs:numeric`](audit/numeric-valid.xpath) | `12.5` | `12.5` | Pass |
| [`xs:positiveInteger`](audit/positiveInteger-valid.xpath) | `12` | `12` | Pass |
| [`xs:short`](audit/short-valid.xpath) | `12` | `12` | Pass |
| [`xs:string`](audit/string-valid.xpath) | `supplier A` | `supplier A` | Pass |
| [`xs:time`](audit/time-valid.xpath) | `09:30:00Z` | `09:30:00Z` | Pass |
| [`xs:token`](audit/token-valid.xpath) | `supplier A` | `supplier A` | Pass |
| [`xs:unsignedByte`](audit/unsignedByte-valid.xpath) | `12` | `12` | Pass |
| [`xs:unsignedInt`](audit/unsignedInt-valid.xpath) | `12` | `12` | Pass |
| [`xs:unsignedLong`](audit/unsignedLong-valid.xpath) | `12` | `12` | Pass |
| [`xs:unsignedShort`](audit/unsignedShort-valid.xpath) | `12` | `12` | Pass |
| [`xs:untypedAtomic`](audit/untypedAtomic-valid.xpath) | `supplier A` | `supplier A` | Pass |
| [`xs:yearMonthDuration`](audit/yearMonthDuration-valid.xpath) | `P2M` | `P2M` | Pass |

The three list constructors return two items for `A B`; `|` separates displayed values. For type checks, list members are tested as ENTITY, IDREF or NMTOKEN; every other inhabited type is tested using its own name. Numeric union membership does not establish which primitive type every lexical form selects. QName uses the predefined xs namespace binding; it is not an arbitrary unbound prefix.

## Error and context boundaries

For all names, direct zero-argument invocation without context reports XPDY0002. Empty mapping produces count zero without invoking a constructor; explicit one-argument empty input also produces count zero, including xs:error. Two mapped inputs create separate calls rather than a multi-item context item. Successful zero-argument calls reject compilation in default 3.1 with XPST0017; representative values also run under experimental mode and agree with frozen 4.0. Negative 4.0 calls are separate from those compilation gates.

| Boundary | Published expression | Expected |
|---|---|---|
| byte 128 / unsignedInt -1 / positiveInteger 0 | [byte](audit/byte-overflow.xpath), [unsigned](audit/unsigned-negative.xpath), [positive](audit/positive-zero.xpath) | FORG0001 |
| dateTimeStamp without timezone | [stamp](audit/stamp-no-timezone.xpath) | FORG0001 |
| QName missing prefix binding | [QName](audit/qname-unbound.xpath) | FONS0004 |
| Array with two members | [array](audit/array-many.xpath) | XPTY0004 |
| Singleton array | [singleton](audit/array-single.xpath) | 12 |
| Explicit integer argument with two values | [argument](audit/argument-many.xpath) | XPTY0004 |
| token whitespace normalization | [token](audit/token-whitespace.xpath) | supplier A |
| date as Boolean source | [wrong type](audit/boolean-wrong-type.xpath) | XPTY0004 |

String/untypedAtomic/anyURI/normalizedString/token invalid-input examples use map{}: there is no universally invalid lexical string for these types. They report FOTY0013. Other representative invalid strings report FORG0001. Numeric limits and lexical constraints are type-specific; these samples do not exhaust them.

## Run and interpret the checks

Run `dotnet test tests/Bosak.Xslt.Tests -c Release --filter FullyQualifiedName~CatalogExampleTests`. The existing published-example harness executes the exact files via XPath31Expression with fresh EvaluationContext. It compares output text or declared error and verifies applicable 3.1/frozen compatibility boundaries. No harness or engine change is introduced here.

508 new checks plus the three introductory focus checks establish representative all-name coverage. Existing qt4tests assignments remain unchanged: no set is promoted or full conformance certified. Missing historical adopted revision, arbitrary-precision decimal and pre-year-one date limitations remain recorded in the catalog/AGENTS.md. Follow-up signature and grammar review is still required for REQ-126 closure.
