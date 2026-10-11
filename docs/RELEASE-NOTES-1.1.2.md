<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.1.2 patch release notes">
  <br><br><h1>Bosak 1.1.2</h1>
  <p>Correct map entry and item sequence results</p>
</div>

## Release scope

Prepared **2026-10-11**, not yet published. Coordinated patch version 1.1.2
targets the existing ten public .NET 10 libraries. No new public API is added.

`map:entries` returns a sequence of single-entry maps instead of the array
returned by 1.1.1. Empty input returns an empty sequence. `map:items` concatenates
entry value sequences into ordinary items, omitting empty values and retaining
nested maps/arrays as items. Both preserve entry order and remain frozen XPath40
functions; direct calls and dynamic lookup stay unavailable at XPath31.

Replace `map:entries($map)?*` with a direct call and array lookup on the old result
with sequence predicates. Review counts and iteration that depended on grouped
`map:items` values. See the [correction dossier](REQ-118-MAP-RESULT-SHAPES.md)
and [product-index entry](xpath40-catalog/functions/product-map-index.md) for
complete examples, pinned specification source and migration details.

The catalog has 41 entries and 168 documentation checks, with 35 registered names
still unassigned. REQ-126 remains In Progress. No conformance sets are promoted;
Braid adoption remains separate.

## Verification

Release solution build: **0 warnings / 0 errors** (final incremental build;
the initial rebuild reported 12 existing warnings). Full isolated solution:
**4,166 passed / 0 failed / 0 skipped**, across ten assemblies (Standard 1,303;
XSLT 1,299). Focused new standard checks **21/0/0**; catalog plus nested map
profile checks **146/0/0**. Frozen QT3 remains exactly **31,142/0/679**.
qt4 promoted sets remain **8,278/0/1,894**, zero gated/exploratory failures and
11 experimental-level passes. Logs: `.guard-tmp/map-shapes-*`.
All 41 catalog structures/index links, 166 primary anchors/links, 28 baseline
provenance records, 117 inventory names and 144 manifest files are verified.
All three family sample blocks match their exact files/results, and 54 catalog
pages plus this dossier and patch notes render successfully. Whitespace checks pass.

Upstream `map-entries` is **16/0/1** (unsupported dependency); `map-items` is
**15/0/0**. Both remain exploratory. A complete XSLT sweep was not rerun.
