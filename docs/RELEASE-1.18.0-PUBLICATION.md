<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala verified Bosak 1.18.0 publication">
<h1>Bosak 1.18.0 publication</h1>
<p>Published and independently verified — 17 October 2026</p>
</div>

User approved publication after [release preparation](RELEASE-1.18.0-READINESS.md) and Braid's [no-blocking implementation review](REQ-130-CONSUMER-IMPLEMENTATION-REVIEW.md). Immutable annotated `v1.18.0` points exactly to `3264fcfaf739e492770c882d2cbfc1f93f0172d2`. [Public GitHub release](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.18.0). No tag movement or duplicate workflow dispatch occurred.

| Gate | Verified result |
|---|---|
| Single tag-triggered Release workflow | Run 39032243994 succeeds, **9,661 solution tests**, ten library upload acknowledgments. |
| Public signed packages | All ten 1.18.0 NuGet archives downloaded, repository signatures cryptographically verified, version/dependencies/repository revision/XML docs/branding/source Git asset bytes checked. |
| Fresh public-only consumers | **153 broad checks +221 focused public tests** pass; separate fresh caches and only nuget.org configured for restore/tests/audit. |
| Provenance | Ten broad/nine focused assemblies identify 1.18.0+3264fcfa; downloaded signed archive/cache/output bytes, source receipts and restored hashes agree. |
| Dependency audit | Zero vulnerable top-level/transitive dependencies reported. |
| Earlier source gates | Source focused 226 and CI 9,661 solution+84 LS pass; QT3 31,142/0/679 and qt4 gated8,278/0/1,894 unchanged. No engine changes in this documentation step. |

[Structured publication proof](RELEASE-1.18.0-PUBLICATION.json) retains exact public URLs/hashes/metadata and consumer provenance. Original workflow/download/restore/test/signature/audit receipts are retained in `.guard-tmp/release1180-publication`; any propagation/setup corrections remain separately disclosed. Public signed archive bytes are verified against the actual downloaded package, not assumed equal to local unsigned builds. Package assets match the approved source Git blobs, preserving platform line-ending differences correctly.

## Preserved propagation correction

The initial focused public restore reported NU1102 from a stale NuGet version index (nearest listed 1.16.5), after upload acknowledgments and signed direct downloads. Its original log is retained as `focused-public-restore-initial.log`; a subsequent forced/no-HTTP-cache public-only restore succeeded, followed by 221 passing tests. No local feed, package re-upload, duplicate workflow or engine patch was used. Broad consumer signed-cache archives independently matched all ten public downloads.

## Consumer boundary

REQ-130 is implemented and published within the bounded instruction-only value-of deletion profile. Braid's independent implementation review reported no blocking findings and overlapping 226/221/241 local-artifact runs; these are not fresh production adoption or native acceptance. Its production pin remains accepted 1.17.0 until it independently adopts this release and implements deletion-specific port/review/Apply/history/selection, licensed worker/restricted export/reopen and native acceptance. Do not close consumer acceptance from engine/public package success alone.

VS Code 0.2.5 continues to bundle 1.16.5; extension alignment is separate. This publication changes no Braid source/pin/UI or installed extension. See [release notes](RELEASE-NOTES-1.18.0.md) and [implementation/API details](REQ-130-IMPLEMENTATION.md).
