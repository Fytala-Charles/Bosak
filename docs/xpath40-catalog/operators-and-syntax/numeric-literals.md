<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala binary hexadecimal and separated numeric literals reference">
  <h1>Numeric literals and digit separators</h1>
  <p>Frozen XPath 4.0 - bounded executable reference</p>
</div>

**C11 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-3.md) records 4 contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Verified bounded literal grammar**. Delivered malformed-separator fix remains closed; literals do not create arbitrary-precision decimal arithmetic or unrestricted generic casts. Review completion is not final owner acceptance; earlier checkpoints remain historical.

**Public1.23.7 - 23 October2026:** EQName bracket parsing, inline-record signature gates and numeric separator boundaries are [published and verified](../../RELEASE-1.23.7-PUBLICATION.md). Current catalog74 entries/2,893 documentation checks; REQ-126/AC-03 remain partial. Braid dependency1.23.4 unchanged. Earlier source/unpublished checkpoints below are historical.

## 1. Name and Classification

**Hexadecimal/binary integer literals and underscore separators** - **Frozen 4.0**, REQ-118 S3. Lowercase `0x` and `0b` introduce integer literals. Separators are permitted inside decimal, hexadecimal and binary digit groups, including the fraction and exponent groups of decimal/double literals. Ordinary XPath 3.0/3.1 syntax remains available; these extensions require 4.0.

Current comparison: [immutable grammar](https://raw.githubusercontent.com/qt4cg/qtspecs/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/grammar-40/xpath-grammar.xml), productions NumericLiteral, HexIntegerLiteral, BinaryIntegerLiteral, DecimalLiteral, DoubleLiteral, Digits, HexDigits and BinaryDigits. Upstream master was refreshed on 23 October 2026 at this revision. It is a current comparison pin, not a recovered historical adoption pin. [Delivery audit](../NUMERIC-LITERALS.md) separates the existing syntax from the source1.23.7 boundary correction; public release remains1.23.4.

## 2. Intent

Make a bit mask or a large threshold readable in source code while retaining the numeric value and ordinary arithmetic semantics.

## 3. Also Known As

Based integer literals; digit separators. `0xFF` is an integer, not an xs:hexBinary byte value. Binary literals are numbers, not xs:base64Binary values. Uppercase hex digits are accepted; uppercase prefixes `0X` and `0B` are refused.

## 4. History / Supersedes

In 3.1 use decimal `255` instead of `0xFF`, `4` instead of `0b0100`, and `1000` instead of `1_000`. The paired [3.0 stylesheet](../examples/numeric-literals/report-3.xsl) and [4.0 stylesheet](../examples/numeric-literals/report-4.xsl) keep the same telemetry report algorithm; only the constants change. Literals do not introduce a new runtime numeric type or relax input casting.

## 5. Motivation

A telemetry feed stores flags and counts as decimal attributes. A report selects readings with bit 4 set and count at least 1000. The mask is easier to inspect in binary and the threshold easier to read with grouping. The [complete task](../examples/numeric-literals/README.md) includes normal, empty, reordered and invalid inputs and exact output.

## 6. Language Alignment

The prefixes resemble C-family source literals; the exact XPath grammar governs the boundaries. Repeated internal underscores such as `1__000` and `0xFF__FF` are valid under the pinned draft. Each nonempty digit group starts and ends in a digit: `1._2`, `1.2_` and `1e_2` are invalid. A sign is a unary operator, not part of the integer token. Source separators do not extend the XML Schema lexical forms accepted by xs:integer: `xs:integer('1_000')` and `xs:integer('0xFF')` still fail FORG0001.

## 7. Applicability

Use these constants in authored XPath/XSLT 4.0 expressions. Keep external decimal attributes in their declared lexical format, and select a 3.0/3.1-compatible spelling when generating an older stylesheet. This entry verifies ordinary Legacy-host effective3.0/4.0 stylesheets, not every package, XQuery or host override context.

## 8. Structure

```xpath
0xFF + 0b1_000 + 1_000
```

The sum is1263. `1_2.3__4` is decimal12.34; `1_2.5e+0_2` is double1250. A decimal point may have no following digits (`1_2.`), and a decimal may omit its integer part (`.1_2`). If a fraction/exponent group is present, its separators cannot be at an edge.

## 9. Participants

[XPathLexer](../../../src/Bosak.XPath.Parser/Lexer/XPathLexer.cs) scans version-sensitive numeric tokens. [XPathParser](../../../src/Bosak.XPath.Parser/Ast/XPathParser.cs) removes separators and computes based integer values. The public XPath31Expression/CompileOptions surface selects the compatibility level. No new public API is added.

## 10. Collaborations

In the report the binary constant and hexadecimal zero form the mask; idiv/mod test the flag arithmetically. A separated threshold filters counts. xsl:for-each preserves input order, and sum builds the total from parsed decimal attribute values. The literals do not implement bitwise operators or alter data conversion.

## 11. Consequences

The supplied checks cover integer/decimal/double identity, a value above Int64 range, negative unary application, numeric map/array lookup keys, valid internal repeated separators, malformed groups, wrong radix digits and version refusals. Bosak uses bounded .NET numeric representations; one above-Int64 example does not certify arbitrary-precision integer/decimal support. Other numeric adjacency, Unicode digit and overflow behavior is outside this bounded correction. No performance improvement is claimed.

## 12. Implementation

**C04 grammar review — 26 October 2026, published 1.26.3:** [Expression/call/focus reconciliation](../EXPRESSION-CALL-FOCUS-RECONCILIATION.md) covers S3-lexical. Radix/separator token boundaries, template escaping/nesting/comment/atomization and old-profile/static-error cases are mapped. C03 retains numeric representation limits. Do not infer arbitrary precision, every adjacency/Unicode digit case, XML/XQuery string constructors or AVT/host behavior from these lexical probes. This is bounded production/profile evidence, not full grammar or AC-04 acceptance.


**C03 review — 26 October 2026, published 1.26.3:** [Constructor/type reconciliation](../CONSTRUCTOR-TYPE-RECONCILIATION.md) verifies 4.0 radix/separator syntax and lower-profile refusal; Above-Int64 integer literals versus generic constructors/casts; Delivered floating thresholds, unsigned truncation and preserved integer identity. **Boundary:** A decimal-backed wide literal does not establish arbitrary precision or a matching generic constructor range. Generic wide xs:integer casts still narrow; .NET decimal precision remains bounded. Ownership: T01, T02. Per-path controls and named schema/API/host evidence are retained in the [review matrix](../CONSTRUCTOR-TYPE-RECONCILIATION.json); no whole-type or AC-04 acceptance follows.

Fifty new catalog cases use the existing public CatalogExampleTests execution path. Initial38 probes produced30 passes and8 failures: malformed fractional/exponent underscore groups were accepted. Source1.23.7 rejects those groups only in the 4.0 lexer path. Thirty-four dedicated API cases cover those eight shapes in all four compatibility levels and valid-group controls in both4.0 profiles. Expected outcomes were not weakened. [Audit and retained evidence](../NUMERIC-LITERALS.md).

## 13. Sample Code

The complete [4.0 report](../examples/numeric-literals/report-4.xsl) reads [normal.xml](../examples/numeric-literals/normal.xml): readings A(flags5,count1500), B(flags2,count900). It emits `A:ready|B:hold|ready=1;total=2400`, matching the3.0 baseline. Empty input emits `ready=0;total=0`; reordering changes row order; invalid count `oops` raises FORG0001. [Task and all expectations](../examples/numeric-literals/README.md) describe every instruction and input assumption.

## 14. Performance Notes

Digit separators are removed during compilation; based integers are accumulated in the parser. These facts are implementation descriptions, not allocation or timing measurements. Report arithmetic and numeric conversion still run per reading. No benchmark or asymptotic improvement over decimal constants is claimed.

## 15. Known Uses

Applicable AC-03 evidence: `numeric-literals-hex`, `numeric-literals-binary`, `numeric-literals-fraction`, `numeric-literals-exponent`, `numeric-literals-beyond-long`, `numeric-literals-invalid-13` and `numeric-literals-report-4-normal`, enumerated with all fifty cases in [the audit](../NUMERIC-LITERALS.json) and [manifest](../examples/cases.json). Named [NumericLiteralBoundaryTests](../../../tests/Bosak.XPath.Api.Tests/NumericLiteralBoundaryTests.cs) methods Compile_InvalidDigitGroup_ThrowsXpst0003 and Evaluate_ValidDigitGroups_ReturnExpectedValues independently cover the correction. Existing [VersionGateTests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) methods Evaluate_HexBinaryLiterals_XPath40Mode_Work, Evaluate_UnderscoreSeparators_XPath40Mode_Work and Evaluate_HexBinaryLiterals_31Mode_ThrowXpst0003 remain relevant. Feature-specific qt4 assignment is explicitly unassigned in this slice; running the unchanged global gate does not establish individual production coverage or promote a set.

## 16. Related Features

[String templates](string-templates.md), [keyword arguments](keyword-arguments.md), [coverage reconciliation](../COVERAGE-RECONCILIATION.md). The [grammar matrix](../GRAMMAR-COVERAGE-MATRIX.json) retains remaining host/declaration coverage. REQ-126 and D2/D3/D4/AC-03 remain partial.
