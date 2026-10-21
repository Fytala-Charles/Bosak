<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.21.0 publication">
<h1>Bosak 1.21.0 publication</h1>
<p>18 October 2026</p>
</div>

**Published and independently verified.** [Release v1.21.0](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.21.0) targets reviewed source `8929a1301d1846bb24b49bcc505877f05fd3395a`. Annotated tag `d54eac16228186d3d88107a15203c60b9c1b09e5` remains immutable.

REQ-133 adds bounded replacement of one existing paired ordinary literal `xsl:text` body. Separate opaque authority and exclusive Changed / NoChange / Refused outcomes preserve exact logical intent, one complete body edit, unchanged outside bytes and complete survivor/replaced/new payload partitions. Empty transitions retain paired tags and an actual empty caret. Equal intent preserves original lexical spelling without a candidate. Read the [release notes](RELEASE-NOTES-1.21.0.md), [contract](REQ-133-TEXT-BODY-REPLACEMENT-CONTRACT.md) and [implementation](REQ-133-IMPLEMENTATION.md).

| Gate | Observed result |
|---|---|
| Single tag-triggered [Release workflow](https://github.com/Fytala-Charles/Bosak/actions/runs/39139755889) | Success at `8929a1301d1846bb24b49bcc505877f05fd3395a`; **9,900** solution tests pass, zero failed/skipped; ten upload acknowledgments. |
| Public archives | Ten signed 1.21.0 NuGet archives downloaded; exact repository revisions, aligned Bosak dependencies, README/license/icon/brand asset bytes and XML documentation verified against approved Git content. |
| Signatures | All ten `dotnet nuget verify --all` checks pass. |
| Fresh public broad consumer | **153** checks pass; ten exact assembly identities and archive/cache/output DLL bytes verify. |
| Fresh public focused consumer | **437/0/0**, including 63 public replacement cases and 374 prior public authoring/identity cases; nine consumed assemblies verify. Thirteen source-only controls excluded. |
| Signed content and dependencies | Raw signed archive/cache hashes match. NuGet PackageArchiveReader content hashes match restored assets and metadata. Both transitive vulnerability audits are clear. |

The consumers use separate fresh caches, nuget.org only, no project references and distinct non-friend assembly identity. Counts overlap. Signed public archives are separate evidence from unsigned local candidates. Earlier source verification includes 76 new cases, 9,900 solution / 84 language-server tests, exact-head CI **9,984**, QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**. Release reruns the solution, not the separate language-server/manual gates.

[Braid implementation review](REQ-133-CONSUMER-IMPLEMENTATION-REVIEW.md) reports no blocking finding at the reviewed merge, with 76 focused source cases and 460 local-package checks (437 imported plus 23 probes). Its harness compile correction, outer source-test settings and limits are preserved. It did not verify signed public packages or certify Braid adoption, adapter/validator, license/session/UI/history, restricted export/worker/destination relocation or native acceptance. Review SHA-256: `4ea7d262f4971767a9ecec44d158ed91353add6a7c360f8acd18c9a2ddfe7d19`.

Original propagation snapshots, workflow logs, signatures, TRXs, restore-source receipts and hashes are retained under `.guard-tmp/release1210-publication/`. The [machine-readable proof](RELEASE-1.21.0-PUBLICATION.json) records literal identities. No retag, duplicate publication workflow or reupload was used.

Braid remains on its own accepted production pin until independent public-package adoption/integration. No consumer runtime/UI/native or VS Code extension release is claimed. The package README is the immutable reviewed source snapshot; main-branch quick-start links are updated separately after publication.
