# Native select boundary batch

Submitted as [xslt40-test PR #23](https://github.com/qt4cg/xslt40-test/pull/23). Two native cases, 0 pass / 2 fail with Bosak. Acceptance and independent processor confirmation are pending.

## Source and existing coverage

Reviewed seqtor select cases 113/114 and 124–144: mixed text, empty/error results, variables, child/fallback rules, type checking/coercion and namespace declarations are already covered. New cases isolate literal line-feed preservation plus both quote characters/CDATA less-than under expand-text, and sequence-valued item/position/size focus projected per output row. Current-item coverage exists; the second case complements it with explicit position/size/cardinality observations.

Expected results derive from [pinned source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml) and the [current section](https://qt4cg.org/specifications/xslt-40/Overview.html#select-instruction). This is a focused review, not exhaustive absence-of-duplicates certification or complete family conformance. No existing expectations changed.

## Input and expected output

Each case starts at xsl:initial-template and requires no source document. Literal values inside the stylesheet supply the input. Status routing, order-export projections and reusable type declarations represent small boundaries arising in real transformations; these are not full production schemas or applications.

### seqtor-145

CDATA map expression preserves a literal newline and both quote characters under expand-text.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:order-types"
 xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="#all">
<!-- CDATA map expression preserves a literal newline and both quote characters under expand-text. -->
<xsl:template name="xsl:initial-template">
<xsl:variable name="m" as="map(*)"><xsl:select as="map(*)" expand-text="yes"><![CDATA[
map{'small': 2 < 3, 'note': 'A
B', 'quotes': concat("'", '"')}
]]></xsl:select></xsl:variable>
<out small="{$m?small}" length="{string-length($m?note)}" newline="{string-to-codepoints($m?note)[2]}" quotes="{string-length($m?quotes)}"/>
</xsl:template>
</xsl:stylesheet>
```

Expected:

```xml
<out small="true" length="3" newline="10" quotes="2"/>
```

Bosak outcome: **fail** — FAIL seqtor-145: XTTE0570: Empty sequence not allowed for type map(*). See [exact result](seqtor-145-result.txt) and [receipt](seqtor-145-receipt.json).

### seqtor-146

Sequence-valued select body evaluates with the current item, position and size.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:order-types"
 xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="#all">
<!-- Sequence-valued select body evaluates with the current item, position and size. -->
<xsl:template name="xsl:initial-template">
<out><xsl:for-each select="('A','B','C')"><xsl:variable name="row" as="item()*"><xsl:select>(., position(), last())</xsl:select></xsl:variable><row value="{string-join($row ! string(.),'|')}"/></xsl:for-each></out>
</xsl:template>
</xsl:stylesheet>
```

Expected:

```xml
<out><row value="A|1|3"/><row value="B|2|3"/><row value="C|3|3"/></out>
```

Bosak outcome: **fail** — FAIL seqtor-146: Result mismatch. See [exact result](seqtor-146-result.txt) and [receipt](seqtor-146-receipt.json).

## Reproduction and evidence

The [portable mail patch](0001-native-tests.patch), [evidence manifest](evidence.json) and [submitted PR body](UPSTREAM_PR_BODY.md) preserve the native commit 5f3296d12cf7aa5ad76554ea95f95f990629e04f. It is based on 6205faf9f32ace6338ac245c41f8eb9a1447926f and changes only the existing test-set and two stylesheets. Each patch reproduces its native commit tree; SHA-256 is recorded.

In the configured checkout, select the correct independent branch before running:

```powershell
git -C .upstream/xslt40-test switch bosak/select-boundaries
pwsh -File ./validate-xslt40-catalog.ps1 -Files tests/misc/seqtor/_seqtor-test-set.xml
pwsh -File ./run-xslt40-tests.ps1 -TestSet seqtor -TestCase seqtor-145 -NoBuild
pwsh -File ./run-xslt40-tests.ps1 -TestSet seqtor -TestCase seqtor-146 -NoBuild
```

Build Bosak first if the local binary is absent. Failed cases exit 1; retain the catalog output instead of changing it to fit the engine. Runner totals can include substring-selected inherited cases; use exact case IDs for the two-case tally. For a fresh suite clone at the recorded base, create a new branch and apply the mail patch with git am; do not apply it again to the existing branch.

## Follow-up

Monitor the upstream review and requested independent cross-check. Record Bosak engine gaps separately; this contribution does not fix them or claim they are upstream defects. Source 1.1.4 remains unpublished, public release 1.1.3.
