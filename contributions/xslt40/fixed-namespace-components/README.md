# Fixed namespace component boundaries

Three specification-derived native tests complement the original five contribution areas. They target existing XSLT 4.0 attributes/components. All three pass in Bosak source `32916981`; published version is 1.2.0 and candidate 1.2.1 remains unpublished. [Submitted as upstream PR #27](https://github.com/qt4cg/xslt40-test/pull/27); upstream acceptance and independent processor confirmation are pending.

**Keyword follow-up - 2026-10-13:** All 16 unchanged native cases now pass at Bosak implementation `96f24e0a`. The initial wider 11/5 results below remain historical. See [post-fix evidence](POST_KEYWORD_RESULTS.json), [exact family log](post-keyword-result.txt) and [receipt](post-keyword-receipt.json). Published version remains 1.2.0; candidate 1.2.1 is unpublished.

## Coverage review

Reviewed fixed-namespaces 001-012/901 and key 101/102. Case 010 covers implicit fixed-only modes; the new cases add explicit declarations with local native rebinding, root/scoped defaults with EQName membership, and fixed-bound key names alongside QName values whose lexical prefix is absent from the static context. Existing key QName tests do not combine these fixed/native binding boundaries.

Specification source: [72f22292](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml); live sections: [fixed-namespaces](https://qt4cg.org/specifications/xslt-40/Overview.html#fixed-namespaces), [default-mode](https://qt4cg.org/specifications/xslt-40/Overview.html#default-mode), [fn:key](https://qt4cg.org/specifications/xslt-40/Overview.html#func-key). Native base: `6205faf9f32ace6338ac245c41f8eb9a1447926f`; commit: `2c6664329b4951f0122bd4c82e6de78460cc9411`.

## Cases and examples

### fixed-namespaces-013

Explicit mode declaration and template membership use fixed bindings despite native prefix rebinding.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:p="urn:native" exclude-result-prefixes="#all"
    fixed-namespaces="#standard p=urn:fixed">
  <xsl:mode name="p:m" on-no-match="fail"/>
  <xsl:template name="main">
    <xsl:variable name="node"><item/></xsl:variable>
    <out><xsl:apply-templates xmlns:p="urn:local" select="$node/item" mode="p:m"/></out>
  </xsl:template>
  <xsl:template xmlns:p="urn:other" match="item" mode="p:m"><hit>fixed</hit></xsl:template>
</xsl:stylesheet>
```

Expected output:

```xml
<out><hit>fixed</hit></out>
```

Bosak: **pass**. [Exact log](fixed-namespaces-013-result.txt), [receipt](fixed-namespaces-013-receipt.json).

### fixed-namespaces-014

Root and scoped default-mode names resolve against fixed bindings and agree with EQName template modes.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:p="urn:native" exclude-result-prefixes="#all"
    fixed-namespaces="#standard p=urn:fixed" default-mode="p:root">
  <xsl:mode name="p:root" on-no-match="fail"/>
  <xsl:mode name="p:scoped" on-no-match="fail"/>
  <xsl:template name="main">
    <xsl:variable name="node"><item/></xsl:variable>
    <out>
      <xsl:apply-templates select="$node/item"/>
      <xsl:apply-templates xmlns:p="urn:local" default-mode="p:scoped" select="$node/item"/>
    </out>
  </xsl:template>
  <xsl:template match="item" mode="Q{urn:fixed}root"><root/></xsl:template>
  <xsl:template match="item" mode="Q{urn:fixed}scoped"><scoped/></xsl:template>
</xsl:stylesheet>
```

Expected output:

```xml
<out><root/><scoped/></out>
```

Bosak: **pass**. [Exact log](fixed-namespaces-014-result.txt), [receipt](fixed-namespaces-014-receipt.json).

### fixed-namespaces-015

Key declaration and string lookup use fixed bindings; QName lookup retains its own namespace identity.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:p="urn:native" exclude-result-prefixes="#all"
    fixed-namespaces="#standard p=urn:fixed">
  <xsl:key name="p:k" match="item" use="@id"/>
  <xsl:template name="main">
    <xsl:variable name="doc"><item id="a"/></xsl:variable>
    <out xmlns:p="urn:local"
      string="{count(key('p:k', 'a', $doc))}"
      qname="{count(key(QName('urn:fixed', 'unbound:k'), 'a', $doc))}"/>
  </xsl:template>
</xsl:stylesheet>
```

Expected output:

```xml
<out string="1" qname="1"/>
```

Bosak: **pass**. [Exact log](fixed-namespaces-015-result.txt), [receipt](fixed-namespaces-015-receipt.json).

## Verification and limits

Exact new cases: **3 pass / 0 fail**. Each filtered receipt reports one pass and fifteen unselected cases as skips. The [wider family run](family-result.txt) is **11 pass / 5 fail / 0 skip**; cases 006-009 and 011 fail on keyword parameter metadata (XPST0017). Those existing assertions are preserved. Neither this batch nor the first fourteen cases establish complete XSLT 4.0 conformance. Independent processor confirmation remains pending.

## Reproduction

Apply [the portable patch](0001-native-tests.patch) to a clean native checkout at the base above using `git am`, or use branch `bosak/fixed-namespace-components`. From Bosak:

```powershell
pwsh -File ./validate-xslt40-catalog.ps1 -Files tests/attr/fixed-namespaces/_fixed-namespaces-test-set.xml
pwsh -File ./run-xslt40-tests.ps1 -TestSet fixed-namespaces -TestCase fixed-namespaces-013 -NoBuild
pwsh -File ./run-xslt40-tests.ps1 -TestSet fixed-namespaces -TestCase fixed-namespaces-014 -NoBuild
pwsh -File ./run-xslt40-tests.ps1 -TestSet fixed-namespaces -TestCase fixed-namespaces-015 -NoBuild
```

Build the Release adapter first if needed. Initial family evidence retains the five failures; the current keyword-fixed source passes all sixteen unchanged cases. [Manifest](evidence.json) records stylesheet, patch and exact receipt SHA-256 values. Receipts retain original checkout paths and statuses for provenance. [Upstream PR text](UPSTREAM_PR_BODY.md) records the requested independent cross-check.

Verification: Release solution build succeeds (0 errors; 88 existing warnings); all 7,908 unit tests pass, including 5,022 XSLT tests, with zero failures/skips. No engine source changed; conformance gate results are retained from merged PR #260. Contribution and canonical Markdown render checks pass.


## Authoring keyword validation follow-up - 2026-10-13

The unchanged native family remains 16/0/0 at implementation `7caa9f4d`. A separate 38-case authoring slice aligns expression/AVT validation with all-supplied host keyword calls; [verification and limits](../../../docs/XSLT40-AUTHORING-FUNCTION-KEYWORD-RESULTS.json), [native log](post-authoring-keyword-result.txt) and [receipt](post-authoring-keyword-receipt.json) retain precise scope. These regressions add no native contribution cases and leave original assertions/receipts unchanged. Candidate 1.2.1 remains unpublished.


## Expression edit context follow-up - 2026-10-13

The unchanged native family remains 16/0/0 at implementation `a34b0672`. The separate 39-case expression-edit slice adds no contribution cases. See [verification and boundaries](../../../docs/XSLT40-EXPRESSION-EDIT-CONTEXT-RESULTS.json), [native log](post-edit-context-result.txt) and [receipt](post-edit-context-receipt.json). Original submissions, failure receipts and assertions are retained; candidate 1.2.1 remains unpublished.
