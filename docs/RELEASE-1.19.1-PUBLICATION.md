<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.19.1 public release verification">
<h1>Bosak 1.19.1 publication</h1>
<p>18 October 2026</p>
</div>

**Published and verified.** [Release v1.19.1](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.19.1) targets approved source `a9e954d75b3518340ce6da84c9ae06c50ab1f5a4`. Annotated tag object `aeaf3342d5916e0e371c832f4f00bd6f42db8d2d` is immutable. The product owner explicitly approved publication after reviewing the prepared release.

The patch returns FOJS0008 when a failed explicit element-to-map layout has no actual wildcard fallback layout, and clears build warnings without suppressions. No new public API or structural authoring operation is introduced. See [release notes](RELEASE-NOTES-1.19.1.md) and [maintenance details](MAINTENANCE-1.19.1.md).

| Verification | Result |
|---|---|
| Single tag-triggered [Release run](https://github.com/Fytala-Charles/Bosak/actions/runs/39113986765) | Success at the approved revision; 9,733 solution tests passed, zero failed/skipped, ten upload acknowledgments. |
| Public archives | All ten NuGet 1.19.1 archives downloaded; repository revisions, dependencies, README/license/icon/assets and XML documentation verified against approved Git bytes. |
| Signatures | `dotnet nuget verify --all`: all ten pass. |
| Broad fresh public consumer | 153 checks pass; all ten assembly identities and archive/cache/output bytes verified. |
| Focused fresh public consumer | 289 tests pass: 282 existing authoring/identity tests plus seven maintenance regression/control cases; nine exact assembly identities verified. |
| Dependencies | Both fresh consumers report zero vulnerable dependencies. |
| Signed-package content | NuGet PackageArchiveReader content hashes match assets and restore metadata; raw signed archive hashes also match cached archives. |

Both consumers restore exclusively from nuget.org into separate fresh caches, with no project references. Counts overlap existing source coverage and are not additive. Public signed archives are distinct evidence from the earlier local unsigned candidates. Frozen gates and separate language-server tests retain their recorded source-maintenance results; they were not rerun by the Release workflow.

NuGet propagation was observed separately after successful uploads; original availability snapshots remain preserved. No duplicate workflow, reupload or retag was used. [Machine-readable proof](RELEASE-1.19.1-PUBLICATION.json) records literal hashes and identities. Raw receipts are retained under `.guard-tmp/release1191-publication/`.

This publishes ten NuGet libraries only. The VS Code extension has its own release cycle. Existing Braid acceptance remains scoped to its recorded package versions; no new Braid pin, integration or native acceptance is claimed.
