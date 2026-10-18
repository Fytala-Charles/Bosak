<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak Bosak 1.19.1 release readiness">
<h1>Bosak 1.19.1 release readiness</h1>
<p>18 October 2026</p>
</div>

**Published and verified:** See [public delivery proof](RELEASE-1.19.1-PUBLICATION.md). The preparation-only evidence below is historical.


**Ready for publication approval; no publication performed or authorized.** Public 1.19.0 remains the delivered release.

Proposed immutable tag: `v1.19.1` at `a9e954d75b3518340ce6da84c9ae06c50ab1f5a4`. Later documentation commits must not move this target. [Prepared release notes](RELEASE-NOTES-1.19.1.md) describe the patch; [machine-readable evidence](RELEASE-1.19.1-READINESS.json) records exact archives and provenance.

| Check | Observed result |
|---|---|
| Source delivery | PR #370 merged; reviewed source CI [39107867726](https://github.com/Fytala-Charles/Bosak/actions/runs/39107867726) passes 9,817 tests. |
| Clean builds | Solution and separate language-server: zero warnings/errors. |
| Frozen gates | QT3 31,142/0/679; qt4 gated 8,278/0/1,894. Source maintenance receipts, not new preparation reruns. |
| Exact merged candidate | Ten local unsigned 1.19.1 archives built from a9e954d7, each with exact repository revision and aligned Bosak dependencies. |
| Independent regression consumer | Eight tests pass; nine assembly versions/informational revisions, archive/cache/output DLL bytes, SHA-512 assets and restore-source receipts verified. Fresh cache, distinct assembly, no project references; vulnerability audit clear. |
| Broad existing API consumer | 153 checks pass across all ten packages; all ten restored DLLs and package hash/source receipts verified against exact-merge archives. Separate fresh cache. |
| Publication preflight | Proposed remote tag, GitHub release and all ten public NuGet versions absent at the recorded check time. Recheck immediately before publishing. |

Source counts overlap package tests. Local unsigned candidates do not prove public package signing or consumer adoption. Original red regression and intermediate CI warning receipts remain preserved; the maintenance record explains their corrections.

After explicit publication approval: recheck target/version and remote absence, push one annotated tag, observe the single Release workflow and ten package uploads, verify signed public archives and fresh nuget.org-only consumers, then create the GitHub release and publication evidence. Never retag or duplicate publication to resolve propagation delays. Any already-present version requires inspecting its provenance first.

Preparation receipts remain in `.guard-tmp/maintenance-release-*`, `.guard-tmp/release1191-broad*` and `release1191-preflight.json`. No Braid integration, pin change, native acceptance, new request implementation or extension publication is part of this patch.
