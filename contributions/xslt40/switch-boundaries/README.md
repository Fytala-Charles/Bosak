# Native switch boundary batch

**Current status — 2026-10-13:** [Upstream PR #24](https://github.com/qt4cg/xslt40-test/pull/24) was merged by Michael Kay (`michaelhkay`) at `2026-10-13T20:23:59Z`; 2 new cases are accepted. Independent processor execution confirmation remains pending. [Merge record](../UPSTREAM_ACCEPTANCE.json). Earlier submission and verification entries below are historical; their original receipts are preserved.

## Original submission evidence

Submitted as [xslt40-test PR #24](https://github.com/qt4cg/xslt40-test/pull/24). Two native cases, 2 pass / 0 fail with Bosak. Acceptance and independent processor confirmation are pending.

## Source and existing coverage

Reviewed switch-001–007: basic expressions/content, multiple alternatives, fallback and no-match behavior exist. New cases add first-match termination with a later failing test, and empty-result termination after an empty candidate test. These are distinct from a switch with no matching branch.

Expected results derive from [pinned source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml) and the [current section](https://qt4cg.org/specifications/xslt-40/Overview.html#xsl-switch). This is a focused review, not exhaustive absence-of-duplicates certification or complete family conformance. No existing expectations changed.

## Input and expected output

Each case starts at xsl:initial-template and requires no source document. Literal values inside the stylesheet supply the input. Status routing, order-export projections and reusable type declarations represent small boundaries arising in real transformations; these are not full production schemas or applications.

### switch-008

Overlapping status alternatives select the first match and do not evaluate later tests.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:order-types"
 xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="#all">
<!-- Overlapping status alternatives select the first match and do not evaluate later tests. -->
<xsl:template name="xsl:initial-template"><out><xsl:switch select="'ready'">
<xsl:when test="('ready','queued')" select="'first'"/>
<xsl:when test="error()" select="'later'"/>
<xsl:otherwise select="error()"/>
</xsl:switch></out></xsl:template>
</xsl:stylesheet>
```

Expected:

```xml
<out>first</out>
```

Bosak outcome: **pass** — PASS switch-008. See [exact result](switch-008-result.txt) and [receipt](switch-008-receipt.json).

### switch-009

A matching empty-result branch ends selection instead of falling through.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:order-types"
 xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="#all">
<!-- A matching empty-result branch ends selection instead of falling through. -->
<xsl:template name="xsl:initial-template">
<xsl:variable name="v" as="item()*"><xsl:switch select="'omit'">
<xsl:when test="()" select="error()"/><xsl:when test="'omit'" select="()"/>
<xsl:when test="'omit'" select="error()"/><xsl:otherwise select="error()"/>
</xsl:switch></xsl:variable><out count="{count($v)}"/>
</xsl:template>
</xsl:stylesheet>
```

Expected:

```xml
<out count="0"/>
```

Bosak outcome: **pass** — PASS switch-009. See [exact result](switch-009-result.txt) and [receipt](switch-009-receipt.json).

## Reproduction and evidence

The [portable mail patch](0001-native-tests.patch), [evidence manifest](evidence.json) and [submitted PR body](UPSTREAM_PR_BODY.md) preserve the native commit b91f75a45dccd6cf6653bfe00f95ae2e2aa49603. It is based on 6205faf9f32ace6338ac245c41f8eb9a1447926f and changes only the existing test-set and two stylesheets. Each patch reproduces its native commit tree; SHA-256 is recorded.

In the configured checkout, select the correct independent branch before running:

```powershell
git -C .upstream/xslt40-test switch bosak/switch-boundaries
pwsh -File ./validate-xslt40-catalog.ps1 -Files tests/insn/switch/_switch-test-set.xml
pwsh -File ./run-xslt40-tests.ps1 -TestSet switch -TestCase switch-008 -NoBuild
pwsh -File ./run-xslt40-tests.ps1 -TestSet switch -TestCase switch-009 -NoBuild
```

Build Bosak first if the local binary is absent. Failed cases exit 1; retain the catalog output instead of changing it to fit the engine. Runner totals can include substring-selected inherited cases; use exact case IDs for the two-case tally. For a fresh suite clone at the recorded base, create a new branch and apply the mail patch with git am; do not apply it again to the existing branch.

## Follow-up

Monitor the upstream review and requested independent cross-check. Record Bosak engine gaps separately; this contribution does not fix them or claim they are upstream defects. Source 1.1.4 remains unpublished, public release 1.1.3.
