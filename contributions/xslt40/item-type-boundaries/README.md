# Native item-type boundary batch

Submitted as [xslt40-test PR #25](https://github.com/qt4cg/xslt40-test/pull/25). Two native cases, 0 pass / 2 fail with Bosak. Acceptance and independent processor confirmation are pending.

## Source and existing coverage

Reviewed type-alias case descriptions and representative declarations: array/record/union aliases, forward/unresolved/circular references, duplicate names, precedence and package visibility exist. New cases use enumeration membership boundaries and locally rebound QName prefixes with the same local type name. This is a focused audit, not exhaustive absence-of-duplicates certification.

Expected results derive from [pinned source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml) and the [current section](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types). This is a focused review, not exhaustive absence-of-duplicates certification or complete family conformance. No existing expectations changed.

## Input and expected output

Each case starts at xsl:initial-template and requires no source document. Literal values inside the stylesheet supply the input. Status routing, order-export projections and reusable type declarations represent small boundaries arising in real transformations; these are not full production schemas or applications.

### type-alias-023

An enumeration alias accepts status strings and rejects unknown strings and integers.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:order-types"
 xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="#all">
<!-- An enumeration alias accepts status strings and rejects unknown strings and integers. -->
<xsl:item-type name="t:status" as="enum('ready','queued')"/>
<xsl:template name="xsl:initial-template"><out ready="{'ready' instance of t:status}" unknown="{'lost' instance of t:status}" numeric="{1 instance of t:status}"/></xsl:template>
</xsl:stylesheet>
```

Expected:

```xml
<out ready="true" unknown="false" numeric="false"/>
```

Bosak outcome: **fail** — FAIL type-alias-023: XPST0051. See [exact result](type-alias-023-result.txt) and [receipt](type-alias-023-receipt.json).

### type-alias-024

Enumeration aliases with the same local name but distinct expanded names remain distinct.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:order-types"
 xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="#all">
<!-- Enumeration aliases with the same local name but distinct expanded names remain distinct. -->
<xsl:item-type name="t:status" as="enum('ready')"/>
<xsl:item-type xmlns:t="urn:other-status" name="t:status" as="enum('queued')"/>
<xsl:template xmlns:u="urn:other-status" name="xsl:initial-template"><out primary="{'ready' instance of t:status}" other="{'queued' instance of u:status}" crossed="{'ready' instance of u:status}"/></xsl:template>
</xsl:stylesheet>
```

Expected:

```xml
<out primary="true" other="true" crossed="false"/>
```

Bosak outcome: **fail** — FAIL type-alias-024: XPST0051. See [exact result](type-alias-024-result.txt) and [receipt](type-alias-024-receipt.json).

## Reproduction and evidence

The [portable mail patch](0001-native-tests.patch), [evidence manifest](evidence.json) and [submitted PR body](UPSTREAM_PR_BODY.md) preserve the native commit 4ddcad8cfc0604f3a0a2cbec7d00032c7e8e0c5a. It is based on 6205faf9f32ace6338ac245c41f8eb9a1447926f and changes only the existing test-set and two stylesheets. Each patch reproduces its native commit tree; SHA-256 is recorded.

In the configured checkout, select the correct independent branch before running:

```powershell
git -C .upstream/xslt40-test switch bosak/item-type-boundaries
pwsh -File ./validate-xslt40-catalog.ps1 -Files tests/type/type-alias/_type-alias-test-set.xml
pwsh -File ./run-xslt40-tests.ps1 -TestSet type-alias -TestCase type-alias-023 -NoBuild
pwsh -File ./run-xslt40-tests.ps1 -TestSet type-alias -TestCase type-alias-024 -NoBuild
```

Build Bosak first if the local binary is absent. Failed cases exit 1; retain the catalog output instead of changing it to fit the engine. Runner totals can include substring-selected inherited cases; use exact case IDs for the two-case tally. For a fresh suite clone at the recorded base, create a new branch and apply the mail patch with git am; do not apply it again to the existing branch.

## Follow-up

Monitor the upstream review and requested independent cross-check. Record Bosak engine gaps separately; this contribution does not fix them or claim they are upstream defects. Source 1.1.4 remains unpublished, public release 1.1.3.
