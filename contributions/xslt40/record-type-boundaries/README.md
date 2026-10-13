# Native record-type boundary batch

Submitted as [xslt40-test PR #26](https://github.com/qt4cg/xslt40-test/pull/26). Both native cases now pass with Bosak source 1.2.0; the original 0 pass / 2 fail receipts below remain historical. Acceptance and independent processor confirmation are pending.

## Source and existing coverage

Existing record-001–025 and subtype cases primarily use structural record syntax; the only scanned xsl:record-type occurrence is an older atomic-set package using extensible forms. New cases target current nominative declarations with constructor=yes, explicit empty fields, and static initialization excluded from constructor arguments. They do not change old structural/optional-field expectations or certify all nominative semantics.

Expected results derive from [pinned source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml) and the [current section](https://qt4cg.org/specifications/xslt-40/Overview.html#named-record-types). This is a focused review, not exhaustive absence-of-duplicates certification or complete family conformance. No existing expectations changed.

## Input and expected output

Each case starts at xsl:initial-template and requires no source document. Literal values inside the stylesheet supply the input. Status routing, order-export projections and reusable type declarations represent small boundaries arising in real transformations; these are not full production schemas or applications.

### record-026

Named record constructor uses declared field order and retains an explicit empty field.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:order-types"
 xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="#all">
<!-- Named record constructor uses declared field order and retains an explicit empty field. -->
<xsl:record-type name="t:line" constructor="yes"><xsl:field name="sku" as="xs:string"/><xsl:field name="quantity" as="xs:integer"/><xsl:field name="note" as="xs:string?"/></xsl:record-type>
<xsl:template name="xsl:initial-template"><xsl:variable name="line" as="t:line" select="t:line('A',2,())"/><out sku="{$line?sku}" quantity="{$line?quantity}" note="{map:contains($line,'note')}" empty="{empty($line?note)}" typed="{$line instance of t:line}"/></xsl:template>
</xsl:stylesheet>
```

Expected:

```xml
<out sku="A" quantity="2" note="true" empty="true" typed="true"/>
```

Bosak outcome: **fail** — FAIL record-026: XPST0017: Function {urn:order-types}line#3 not found.. See [exact result](record-026-result.txt) and [receipt](record-026-receipt.json).

### record-027

Static field initialization is excluded from constructor parameters and is retained in the record.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:order-types"
 xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="#all">
<!-- Static field initialization is excluded from constructor parameters and is retained in the record. -->
<xsl:record-type name="t:line" constructor="yes"><xsl:field name="sku" as="xs:string"/><xsl:field name="quantity" as="xs:integer"/><xsl:field name="kind" static="yes" as="xs:string" select="'line'"/></xsl:record-type>
<xsl:template name="xsl:initial-template"><xsl:variable name="line" as="t:line" select="t:line('B',3)"/><out sku="{$line?sku}" kind="{$line?kind}" quantity="{$line?quantity}" fields="{map:size($line)}"/></xsl:template>
</xsl:stylesheet>
```

Expected:

```xml
<out sku="B" kind="line" quantity="3" fields="3"/>
```

Bosak outcome: **fail** — FAIL record-027: XPST0017: Function {urn:order-types}line#2 not found.. See [exact result](record-027-result.txt) and [receipt](record-027-receipt.json).

## Reproduction and evidence

The [portable mail patch](0001-native-tests.patch), [evidence manifest](evidence.json) and [submitted PR body](UPSTREAM_PR_BODY.md) preserve the native commit 024e11f3525c1753ccda92561adc220bf584f1f0. It is based on 6205faf9f32ace6338ac245c41f8eb9a1447926f and changes only the existing test-set and two stylesheets. Each patch reproduces its native commit tree; SHA-256 is recorded.

In the configured checkout, select the correct independent branch before running:

```powershell
git -C .upstream/xslt40-test switch bosak/record-type-boundaries
pwsh -File ./validate-xslt40-catalog.ps1 -Files tests/type/record/_record-test-set.xml
pwsh -File ./run-xslt40-tests.ps1 -TestSet record -TestCase record-026 -NoBuild
pwsh -File ./run-xslt40-tests.ps1 -TestSet record -TestCase record-027 -NoBuild
```

Build Bosak first if the local binary is absent. Failed cases exit 1; retain the catalog output instead of changing it to fit the engine. Runner totals can include substring-selected inherited cases; use exact case IDs for the two-case tally. For a fresh suite clone at the recorded base, create a new branch and apply the mail patch with git am; do not apply it again to the existing branch.

## Follow-up

Monitor the upstream review and requested independent cross-check. Record Bosak engine gaps separately; this contribution does not fix them or claim they are upstream defects. Source 1.1.4 remains unpublished, public release 1.1.3.


**Post-implementation — 2026-10-13:** Bosak source candidate 1.2.0 (unpublished) now recognizes xsl:record-type/xsl:field and registers constructor=yes functions. Constructors retain field order and empty entries, convert field values, supply trailing defaults and exclude static fields from arguments. Records carry declaration identity: constructed values satisfy named @as and instance-of tests; same-shaped other types and plain maps do not. Both unchanged native cases record-026/027 pass. The wider record substring sweep is 12 pass/52 fail/1 skip, including inherited structural/subtype cases. Plain-map coercion/casts, recursive record definitions, complete package exposure/function signatures and streaming conformance remain follow-ups. N05 and REQ-126 remain In Progress; original failed receipts and upstream PR #26 remain unchanged, with review/independent confirmation pending. Public release remains 1.1.3; no publication. Next: prioritize the wider named-type/coercion gaps and select body namespace declarations after checking this slice's CI. See [post-fix results](POST_FIX_RESULTS.json).


**Nominative record coercion — 2026-10-13:** Source candidate 1.2.0 (unpublished) now coerces maps to package-local nominative xsl:record-type declarations at variable and function boundaries. Conversion applies field types, missing-field defaults and static initializers, rejects surplus keys and supplied static fields, preserves declaration order and annotates the result with the target identity. Existing matching records are retained. Type checks still reject plain maps and distinguish same-shaped declarations. Nine new regressions cover sensor defaults, function references, cardinality, invalid bindings and identity; the previous valid-map refusal test now rejects surplus fields. Named casts, recursive named records, nested named-type/static namespace resolution and complete package exposure remain follow-ups. N04/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](COERCION_RESULTS.json). Catalogue counts remain unchanged; the named-record plain-map fixture now expects INV-2048. Original native assertions remain unchanged; earlier slice notes are historical. Next: named record cast and nested type resolution boundaries.


**Named record casts and nested references — 2026-10-13:** Source candidate 1.2.0 (unpublished) now supports package-local nominative record cast/castable operations. Casts retain map operands, cast each present field sequence, apply defaults to missing fields, discard surplus keys, protect static fields and attach the target declaration identity. Nested named references in binding/function signatures resolve in their declaration namespace, and nested named record fields reuse casting recursively. Fourteen new regressions cover lexical/type failures, empty/optional inputs, field cardinalities and namespace boundaries. General array/map casts, item-type alias cast expansion, recursive definitions, full package exposure and complete static diagnostics remain follow-ups. N05/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](CAST_RESULTS.json). Catalogue counts/outcomes and original native assertions remain unchanged. Earlier slice notes are historical. Next: remaining recursive/static type-resolution and cast-container boundaries.
