<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.7.1 release publication">
  <br><br><h1>Bosak 1.7.1</h1>
  <p>Lexical original binding in escaping inline functions</p>
</div>

## Release status

**Published 15 October 2026.** Immutable `v1.7.1` points to user-approved revision `35e531c101bcbba13a2533f33a7decca5f18eddd`. The successful [release workflow](https://github.com/Fytala-Charles/Bosak/actions/runs/38768757675) passes all 8,483 solution tests and publishes ten .NET 10 libraries. Public libraries are now 1.7.1; VS Code 0.2.2 continues to bundle engine 1.7.0. [Publication evidence](RELEASE-1.7.1-PUBLICATION.json) preserves public packages, fresh consumer verification and original preparation provenance. [Preparation evidence](RELEASE-1.7.1-PREPARATION.json) remains unchanged.

## Correction

An inline function returned by a concrete override retains its lexical `xsl:original` after that override returns and when called inside another override. For example, `function($v) { xsl:original($v) }` calls the declaring original instead of failing to resolve it or calling another override's original.

The fix covers nested closures, reduced/full optional arities, keyword/arrow calls and named/partial originals created inside an escaped body. Original defaults retain declaring globals/private helpers. Caller aliases are restored after return and errors. Focus remains absent in inline bodies; dynamic lookup continues to hide originals and abstract originals remain static XTSE3075 errors. Only hidden bound aliases are captured. No public API is added. [Bounded implementation contract](XSLT40-ESCAPING-INLINE-ORIGINALS.md).

## Verification

All ten direct nuget.org downloads verify versions, coordinated dependencies, approved repository revision, net10 DLL/XML assets and tagged README/license/icon. Fresh nuget.org-only caches pass **153 broad checks** and **185 focused tests** (33 overlap). Archive/cache/output DLL bytes and assembly **1.7.1.0** with the exact approved informational revision match; signed archive SHA-512 and restore content hashes are verified separately. The preparation results below remain historical.

Refreshed-head CI passes the complete solution (8,483 tests, including 5,597 XSLT tests), 84 language-server tests and extension checks. Source/test trees match the reviewed implementation. Frozen gates rerun at the refreshed head remain exactly 31,142/0/679 and 8,278/0/1,894.

Ten exact refreshed-head packages pass metadata/dependency/asset checks. Fresh mapped caches pass 153 broad checks and 185 focused tests (33 overlap); archive/cache/output DLL bytes and assembly 1.7.1.0/exact full revision match. An isolated Braid snapshot passes 843 tests with nine-package provenance; original files, Git status and production pin are unchanged. Final merged-revision repacking and fresh consumer verification passed before approval; the public-package checks above passed after publication.

## Boundaries

General package-graph capture, overlapping optional families and editor package signatures remain separate. REQ-126 remains In Progress. No conformance gate membership, upstream submission or production Braid adoption changes. Extension alignment is separate from this library release. Existing precision, date-range and remote HTTP limitations remain documented.
