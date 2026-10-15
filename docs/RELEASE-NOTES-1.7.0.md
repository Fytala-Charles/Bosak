<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.7.0 release publication">
  <br><br><h1>Bosak 1.7.0</h1>
  <p>Abstract optional function overrides and transitive adoption</p>
</div>

## Release status

**Published 15 October 2026.** Immutable `v1.7.0` points to user-approved revision `33efd8e5a2dda2e16e4cdc69df87dbc01a8a1961`. The successful [release workflow](https://github.com/Fytala-Charles/Bosak/actions/runs/38737338912) passes all 8,465 solution tests and publishes ten .NET 10 libraries. Public NuGet libraries are now 1.7.0; VS Code 0.2.1 still bundles engine 1.6.0. [Publication evidence](RELEASE-1.7.0-PUBLICATION.json) preserves workflow, public downloads and fresh consumer verification. [Preparation evidence](RELEASE-1.7.0-PREPARATION.json) remains unchanged.

## Changes

- A concrete override can implement one abstract function family with trailing optional parameters. Calls at each supported arity use the replacement, retaining original public keyword names and replacement defaults and declaring scope.
- In an A-to-B-to-C chain, B can explicitly accept A's function as abstract and C can implement it. Calls from A, B and C, including named and partial function items returned from a library, keep the concrete binding.
- Explicit principal packages support the same dispatch and dynamic lookup as implicit stylesheet packages. Library-local lookup continues to hide abstract declarations and preserve public originals.
- References to an abstract `xsl:original` raise XTSE3075 at compile time, including uncalled bodies, defaults and dead branches. Expression/AVT checking ignores foreign annotations and respects normalized `expand-text` inheritance.
- Hidden/private adoption boundaries, signature compatibility and unresolved abstract diagnostics remain enforced.

For an abstract `p:f($a, $b := 'B', $c := 'C')` adopted by B, C can implement it using local parameter names and defaults `'X'` and `'Y'`. A call `p:f('A')` from any of the three packages uses C's defaults; public keyword arguments still use `a`, `b` and `c`. See the [direct contract](XSLT40-ABSTRACT-OPTIONAL-OVERRIDES.md) and [transitive contract](XSLT40-TRANSITIVE-ABSTRACT-ADOPTION.md).

## Verification

All ten packages downloaded directly from nuget.org verify version, internal dependencies, repository revision, net10 DLL/XML assets, tagged README, license/icon and hashes. Fresh nuget.org-only consumers pass **153 broad checks** and **167 focused tests** (33 overlapping cases). All ten broad-consumer and nine focused-consumer DLLs match public archive bytes and carry assembly **1.7.0.0** with exact approved informational revision. Signed archive SHA-512 and restore content hashes are recorded separately. The preparation results below remain historical.

The merge revision passes **8,465 solution tests** (passing full-run projects plus the corrected Runtime project rerun after supplying the pinned QT3 schema fixtures), including **5,579 XSLT tests**, and **84 language-server tests**. Post-merge CI succeeds. Frozen QT3 is exactly **31,142/0/679**; qt4 gate-only is **8,278/0/1,894**. The incremental solution build has zero errors and 12 existing warnings; initial packaging warnings are preserved separately.

All ten local 1.7.0 packages verify dependency versions, repository revision, .NET 10 DLL/XML assets, README/license/icon and hashes. Fresh mapped caches pass **153 broad checks** across all ten libraries and **167 focused tests** across nine engine dependencies. These selections overlap 33 used-package cases. Cache/output DLL bytes, assembly versions **1.7.0.0** and exact merge informational revision match the archives. Four additional review boundaries pass alongside the 45 transitive cases (49 cases in that separate run).

A fresh isolated snapshot of the current Braid working tree passes **843 tests** with these candidate packages. [Braid compatibility](BRAID-1.7.0-COMPATIBILITY.md) distinguishes this automated result from production adoption. Original source files, Git status and dependency pin are preserved. Final documentation integration, exact approved-revision repacking and both fresh consumers passed before explicit publication approval; the public-feed checks above then passed after publication.

## Boundaries

Escaping inline closures, overlapping adopted/local optional families, intermediate concrete override chains and general re-export graphs remain separate work. Named/partial escaping items are covered; inline-body rejection of abstract original references does not establish escaping-closure support. Package acquisition and editor package signatures remain unchanged.

No public API, conformance gate membership or REQ status changes. REQ-126 remains In Progress. Existing decimal precision, extended-year conversion and remote HTTP limitations remain documented. The library release is complete. Production Braid adoption and extension alignment remain separate subsequent steps.
