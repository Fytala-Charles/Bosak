<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.22.0 release notes">
<h1>Bosak 1.22.0 release notes</h1>
<p>19 October 2026</p>
</div>

**Published and verified:** immutable `v1.22.0` at `3b00938875490d1997b09f53b80f2ee2986a666b`.

Bosak 1.22.0 adds source-preserving deletion of one eligible literal `xsl:text` instruction and its actual payload (REQ-134).

- Separate capability, snapshot-owned opaque target/proposal, and exclusive Candidate/Failure outcomes. Every successful deletion changes source, including empty/self-closing instructions; no NoChange branch.
- Exact full-instruction removal with one source edit and every outside byte unchanged. Paired empty/nonempty ordinary text and self-closing syntax are supported; self-closing has no fabricated body range.
- Complete actual removed/surviving/merged descriptor partitions, mapped surviving parent, and bounded two-fragment adjacent whitespace provenance. Fresh engine inspection owns physical/reference newline semantics.
- UTF-8 BOM/no-BOM and BOM-marked UTF-16 LE/BE; 8,388,608-byte original/final bounds. Typed cancellation for this new API preserves legacy behavior.

CDATA/body markup, effective text expansion/output escaping, significant direct parent text and any surviving preserving ancestor remain outside this bounded deletion profile. Existing text replacement's broader mixed/preserved contexts remain unchanged. Consumers need a separate deletion adapter and complete provenance review before atomic Apply/history/navigation.

Source and merge CI pass: 10,010 solution +84 language-server tests; 110 new source cases (94 public /16 internal). Exact-merge local packages pass 531 focused checks and 153 broad compatibility checks, with complete identities and clear audits. Frozen QT3 remains31,142/0/679 and qt4 gated8,278/0/1,894. Braid independently passed551 checks (531 imported +20 probes) against both head and merge, with no blocking finding; repeated and upstream checks overlap.

All ten signed public NuGet packages are independently verified through fresh nuget.org-only consumers: 531 focused tests and 153 broad compatibility checks, exact source/assembly/archive provenance, signature verification and clear dependency audits. Braid adoption/adapter/UI/history/worker/export/native acceptance and the VS Code extension lifecycle remain separate.


[Public delivery proof](RELEASE-1.22.0-PUBLICATION.md) records signed public identities and fresh consumer verification.
