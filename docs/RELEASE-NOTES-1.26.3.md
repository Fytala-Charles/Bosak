<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.26.3 release notes">
  <h1>Bosak 1.26.3 release notes</h1>
  <p>26 October 2026 — published and independently verified</p>
</div>

Bosak 1.26.3 preserves supported integer results in numeric standard functions. [Verified candidate](RELEASE-1.26.3-CANDIDATE.md) pins source `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f`; public installation is **1.26.3**.

- `fn:abs` handles Int64.MinValue using existing decimal-backed integer storage and retains integer identity for supported wide integer arguments.
- `fn:floor` and `fn:ceiling` preserve existing decimal-backed integer values and annotations unchanged.
- A separate floor/ceiling argument-contract audit verifies cardinality, recursive arrays, untyped/node conversion and required errors across public call forms. It passed without an engine change.

For example, `abs(xs:long('-9223372036854775808'))` returns integer 9223372036854775808, and `floor(9223372036854775808) instance of xs:integer` returns true. Plain decimals retain decimal behavior; floating calculations and special-value/zero handling remain unchanged. No public API, general constructor/cast support or numeric storage expansion is added.

All **21,882 source/LSP tests**, **153 broad package checks** and **11,175 focused package tests** pass. The focused consumer includes all **3,061 added source cases** unchanged. Both frozen gates remain unchanged. Ten signed public packages and 19 consumed assembly identities have matching pinned-source/archive/cache/output provenance. Both fresh dependency audits report no vulnerable packages at verification time.

REQ-126 remains partial. Fixed numeric storage, general wide constructors/casts, Int64 idiv, schema-list/general coercion and bounded Unix-calendar limitations remain. Current-native Unix retains known negative-domain failures (22/2/0); no full current-draft conformance is claimed. Braid's captured local 1.26.2 automated adoption evidence establishes no 1.26.3 adoption or new native UI acceptance.

[Verified publication](RELEASE-1.26.3-PUBLICATION.md) records the approved tag, single Release run, ten verified public signatures and fresh public consumers. Publication completed from the owner-approved source.
