<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.1.1 release notes">
  <br><br><h1>Bosak 1.1.1</h1>
  <p>Effective-version XPath compatibility and executable reference material</p>
</div>

## Release scope

Published **2026-10-11** as [v1.1.1](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.1.1) from
commit `d35cf1d4bd1d1b7143ccb9ab304812f781dc0d50`. The [Trusted Publishing workflow](https://github.com/Fytala-Charles/Bosak/actions/runs/38139040259)
passed. All ten public NuGet libraries target .NET 10; downloaded artifacts
verify versioned internal dependencies, README and license. A fresh-cache
consumer verified four runtime compatibility checks against public packages.

Version 1.1.0 accepted version 4.0 stylesheets but compiled their runtime
expressions as XPath 3.1. Calls such as `fn:partition` consequently failed with
XPST0017. Version 1.1.1 selects frozen XPath40 when the owning effective XSLT
version is at least 4.0, including local overrides and imported/included modules.
Earlier versions and the default public XPath API retain XPath31.

The correction covers runtime/static expressions, AVTs and text templates,
dynamic evaluation and pattern predicates. Nested evaluations restore the
caller’s profile. A future stylesheet version does not enable experimental
functions, and serialization `xsl:output/@version` does not change expression
compatibility. Existing pattern grammar restrictions remain, including inline
function bodies with curly braces in match attributes. No public API is added.

See the [compatibility dossier](REQ-118-XSLT-XPATH40-COMPATIBILITY.md) for the
immutable reviewed specification revision, named regressions and limitations.

## Reference guide

The catalog now has 38 entries, 104 feature checks and four worked chapters
with 24 checks. The [order-routing chapter](xpath40-catalog/worked-examples/order-routing/README.md)
shows complete normal and edge-case inputs/outputs and both a self-contained
4.0 stylesheet and a portable two-stage integration suitable for 1.1.0.
The [provenance ledger](xpath40-catalog/PROVENANCE.md) records 28 baseline
delivery revisions and named tests; missing historical specification snapshots
remain explicit. REQ-126 stays In Progress with 49 unassigned registered names.

## Verification

- Release solution build: zero errors; one existing nullable warning.
- Full isolated solution tests: **4,103 passed / 0 failed / 0 skipped**.
- Focused runtime and routing checks: **41 passed**.
- Frozen QT3 gate: **31,142 passed / 0 failed / 679 skipped**.
- qt4 promoted sets: **8,278 passed / 0 failed / 1,894 skipped**, zero gated failures.
- XSLT version/evaluate sets: **110/0/4** and **41/0/16**.

A complete XSLT conformance sweep was not rerun for this patch. Braid release
adoption remains separate work.
