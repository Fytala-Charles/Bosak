<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.5.0 Braid compatibility verification">
  <br><br><h1>Braid compatibility with Bosak 1.5.0</h1>
  <p>Isolated verification of the current consumer working tree, 15 October 2026</p>
</div>

## Result

An isolated copy of the local Bosak.Braid working tree builds against published Bosak 1.5.0 with **zero warnings and errors**. Its full automated suite passes **836 tests / 0 failures / 0 skips**: Core 27, Application 291, Infrastructure 314 and Workbench 204. This includes existing source inspection/editing, controlled preview, worker-process, export and headless workbench coverage; it adds no new acceptance criteria.

All nine transitive engine dependencies used by Braid restore exclusively from nuget.org into a fresh cache. Every package archive matches the 1.5.0 publication receipt. Engine DLL bytes in both Infrastructure and Workbench test output match the published package assets. No engine project reference remains in the isolated consumer dependency graph.

[Machine-readable evidence](BRAID-1.5.0-COMPATIBILITY.json) records package hashes and provenance, build/test logs and TRX hashes. The original 279-file snapshot manifest remains at `.guard-tmp/braid150-snapshot.json`; the receipt records its SHA-256. Logs, copied source and cache remain under `.guard-tmp/braid150-*`.

## Reproduction boundary

The snapshot includes the consumer's existing uncommitted changes at Braid HEAD `61bfd62f142470bac84278ade7690c7be603d997`. That commit alone does not identify the tested working tree; use the preserved file manifest and its hash. In the isolated copy only:

1. Replace Infrastructure's Bosak source project reference with `Bosak.Xslt` package version 1.5.0.
2. Remove engine source projects and the source-checkout revision guard, recording approved release revision `d5c3aa9ee39b95efd1e27c8cb990e1ce0da9689c` in Braid's existing dependency metadata.
3. Disable MinVer to prevent the enclosing Bosak repository's tags from affecting the copied consumer's version.
4. Restore from nuget.org only with a fresh package cache, build Release and run the complete copied solution tests.

The actual Braid checkout and its source pin `5bab4799783569d538cec9b03d37a57454f0eebc` were not changed by this verification. Source, test and project files still match the snapshot. Eight documentation files changed concurrently in the original checkout; their paths are recorded separately in the receipt. This verification did not write to that checkout or commit/push Braid changes.

These results establish automated compatibility of the isolated adaptation. They do not claim production dependency adoption, independent Braid acceptance, native desktop acceptance or newly supported package-dependent authoring validation. Braid adoption must respect its source-pin workflow and preserve its existing work.

## Next engine slice

The proposed next slice is [optional function overrides and `xsl:original`](XSLT40-OPTIONAL-OVERRIDE-SCOPE.md). The [bounded implementation](XSLT40-OPTIONAL-OVERRIDES.md) is now verified in unpublished source 1.6.0. This earlier Braid result remains specific to published 1.5.0 and does not establish adoption or acceptance of the new slice.
