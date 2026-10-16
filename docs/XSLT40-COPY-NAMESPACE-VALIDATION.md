# Namespace classification in copy validation

Unpublished source **1.14.16** excludes namespace declarations and qualified metadata from the xsl:copy-specific attribute/options loop. LINQ-to-XML represents a default xmlns declaration with an empty NamespaceName, so the old whitelist incorrectly rejected it as an ordinary attribute (XTSE0090). The boolean check also used local names alone, causing xmlns:copy-namespaces and foreign e:copy-namespaces metadata to trigger XTSE0020.

The loop now explicitly skips IsNamespaceDeclaration and nonempty attribute namespaces before inspecting local names. Real unqualified attributes retain the existing whitelist, shadow-attribute handling and option-value checks. General XSLT-namespaced attribute validation remains in its existing earlier pass. No runtime copying or public API behavior is otherwise changed.

```xml
<xsl:copy xmlns="urn:result" select="$seed">
  <child>ok</child>
</xsl:copy>
<!-- Valid instruction; the literal child remains in urn:result. -->
```

## Specification and integration base

PR340 passed exact-head CI run38933454715 at `aea095eb853b8f6c1772a594e5c21c0bacf2b46a` and merged at `a73345fa6258d4255e171f2792230695ad570e51`, the final base. Tests began on that exact precursor while CI ran. Entire precursor/merge trees were verified identical (`4c85a2b2e36899e3efb3557d150b7b751daa11ec`) before alignment; no tested source changed during alignment.

Refreshed xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. The draft's [standard attribute rules](https://qt4cg.org/specifications/xslt-40/Overview.html#standard-attributes) distinguish instruction options from namespace declarations and foreign metadata. Namespace declarations are not XDM attribute nodes; option names are unqualified.

## Regression controls

Original21-case baseline: **5 passed / 16 failed**. Expanded23-case baseline: **6 passed / 17 failed**. Final focused selection: **257/0/0** (23 new +234 previous construction/copy controls). The final fixture is byte-identical to the expanded pre-fix source; original sources/logs are retained. No compile errors, fixture corrections, expectation changes or weakened assertions were needed.

The controls cover default XSLT namespaces, default namespace undeclarations, unrelated result namespaces, namespace prefixes named copy-namespaces, foreign metadata sharing option names, both ordinary/function copying, exact integer delivery, literal-child namespace preservation, real boolean options and XSLT3.0. Negative controls retain XTSE0090 for invalid unqualified attributes and XTSE0020 for invalid real boolean values, including beside a default namespace. Unused function bodies are checked statically too.

This fixes the legal default-namespace form recorded separately during PR340. Its original submission/execution artifacts remain untouched; the current independent tests establish the correction rather than rewriting those receipts.

## Native accounting

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 155/5/20 | 155/5/20 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 23/8/1 | 23/8/1 |

All three runs exit1 because failures remain. Exact passing identities are unchanged. PR340 receipts/logs are copied byte-identically under `.artifacts/native-before`; fresh receipts are under `.artifacts/xslt40`. The companion JSON retains source/log/receipt SHA-256 hashes. No upstream expectations, aliases, skips or gate membership changed.

## Verification and next review

All local jobs completed: Release build zero errors/2 warnings; solution **9,304/0/0** (XSLT6,370), focused combined **257/0/0**, language server **84/0/0**, frozen QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, both exit0. All test processes completed; fixture junctions were verified and removed.

Canonical documents are synchronized and rendered using markdown-it and canonical Fytala CSS; renderer receipt is in DOCUMENTATION_RENDERER_TEST.md. Source1.14.16 is unpublished. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance remain unchanged.

Next: exact-head CI/review, then audit xsl:copy standard XSLT attributes against the shared whitelist. JNode construction, broader constructor namespace semantics, nonraw text-only API delivery kind, processor capability and obsolete patterns/functions remain separate.
