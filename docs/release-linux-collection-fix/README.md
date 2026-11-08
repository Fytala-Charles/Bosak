# Linux QT3 collection fixture transport correction

8 November 2026. The [first hosted NuGet preparation](../release-1.81.2-hosted/README.md) passed all 26,342 solution tests but failed six QT3 collection cases. Their local fixture path was resolved against the catalog HTTP base as `http://www.w3.org/home/runner/.../bib.xml`. Packing and publication did not run.

## Change and boundary

`TestEnvironment.FromElement` now converts an existing collection `source file=` fixture to an explicit escaped absolute file URI. A catalog `source uri=` member remains unchanged, including HTTP-rooted URI references. Missing files remain absent. No runtime, public API, policy, shipping source, tool pin or release target changes. Core/server remain unpublished 1.81.2 and extension 0.2.6.

[Immutable adoption evidence](SPECIFICATION-ADOPTION.json) records the pinned QT3 catalog and original harness hash before implementation. This adopts no new XPath semantics. The Standard test project links the real environment parser, so its three new regression cases exercise the same code used by the manual QT3 gate. They cover default/named collections under an HTTP base, percent/hash/space/Unicode filename escaping, URI member preservation and missing fixtures.

The valid before-fix regression run fails both file-URI expectations and passes the URI/missing-file control; all three pass after the fix. An earlier test-authoring run failed because the shell transport replaced a Unicode filename character with `?`; the test now uses a C# Unicode escape. Both original failed TRX sets are retained separately, rather than presenting the authoring error as the regression proof.

## Qualification

Local full build passes. All **26,345 distinct solution cases** pass across the full invocation and a focused 47-case Database retry. The initial restricted invocation retains 45 HTTP-listener setup failures; the retry passes all 47 with reviewed process permission. A single wholly green local invocation is not claimed. QT3 remains **31,397/0/424**; gated qt4 **8,941/0/1,918**, experimental11, with exact frozen skip maps. [Summary](LOCAL-RESULTS.json) and [raw logs/TRX/gates](LOCAL-RECEIPTS.zip) retain all attempts. Hosted retry source/run identities and artifacts must be retained separately from the older failed run and earlier local Windows artifacts. No prior package hashes are relabeled; publication and owner/API/signing decisions remain separate.
