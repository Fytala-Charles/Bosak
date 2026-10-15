<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.6.0 Braid compatibility verification">
  <br><br><h1>Braid compatibility with Bosak 1.6.0</h1>
  <p>Isolated verification of the current consumer working tree, 15 October 2026</p>
</div>

## Result

An isolated copy of the local Braid working tree builds against public Bosak 1.6.0 with **zero warnings and errors**. All **836 tests pass**, with no failures or skips: Core 27, Application 291, Infrastructure 314 and Workbench 204. This exercises the consumer's existing source inspection/editing, controlled preview, worker-process, export and headless workbench coverage. It adds no new acceptance criteria.

Nine transitive engine dependencies restore exclusively from nuget.org into a fresh cache. Archives match the [1.6.0 publication receipt](RELEASE-1.6.0-PUBLICATION.json); engine DLL bytes in both Infrastructure and Workbench test output match their public package assets. Nine dependency assemblies have version **1.6.0.0** and informational version **1.6.0+3715b2f98bbf80883358d31234224cbb9cd94cad**. The engine dependency graph uses packages, with no engine source project references.

[Machine-readable evidence](BRAID-1.6.0-COMPATIBILITY.json) records package provenance, DLL metadata, logs and TRX hashes. The original 279-file manifest is retained at `.guard-tmp/braid160-snapshot.json` with its SHA-256 in the receipt. Copied source, cache and logs remain under `.guard-tmp/braid160-*`.

## Reproduction boundary

The snapshot includes existing uncommitted work at Braid HEAD `61bfd62f142470bac84278ade7690c7be603d997`. That commit alone does not identify the tested tree; the preserved manifest records the exact original bytes. In the copy only:

1. Replace Infrastructure's engine source reference with `Bosak.Xslt` package version 1.6.0.
2. Remove engine projects from the solution and replace the source revision-check target with approved release revision metadata.
3. Disable MinVer so the enclosing engine repository's tags do not affect the copy's version.
4. Restore using nuget.org alone and a fresh cache, build Release and run all copied solution tests.

The original Braid files and Git status still match the snapshot after verification. Its production source pin remains `5bab4799783569d538cec9b03d37a57454f0eebc`. This work did not write, commit or push changes in Braid.

The [earlier 1.5.0 compatibility run](BRAID-1.5.0-COMPATIBILITY.md) also passed 836 tests; its receipt is preserved separately. This new result establishes automated compatibility of the isolated package adaptation. It does not establish production pin adoption, native desktop acceptance, independent Braid approval or broader package-dependent editor validation.

## Follow-up

Align the VS Code extension with engine 1.6.0 and verify its diagnostics, launchers, packaged dependencies and editor behavior before a separate extension release. Scope abstract optional originals as a later bounded engine slice, keeping overlapping arity families separate. No engine behavior, public API, REQ status or conformance gate changes are made by this verification.
