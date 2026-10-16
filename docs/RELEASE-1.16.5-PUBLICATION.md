# Bosak 1.16.5 publication verification

**Published and verified on 16 October 2026.** Approved immutable tag `v1.16.5` points to `09940e90f702566107bb2eed521b7f8d789a6410`. The single tag-triggered [release workflow](https://github.com/Fytala-Charles/Bosak/actions/runs/38958389011) succeeded; [GitHub release](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.16.5). [Complete publication receipt](RELEASE-1.16.5-PUBLICATION.json); [release notes](RELEASE-NOTES-1.16.5.md).

| Verification | Result |
|---|---|
| Publication authorization | User explicitly approved the exact revision, ten NuGet packages and GitHub release |
| Workflow | Exact approved revision, push event, 9,577 solution tests pass; ten package upload receipts; no additional dispatch |
| Public packages | All ten 1.16.5 libraries indexed and downloaded from nuget.org; correct repository revision, dependencies, XML docs, README, license and icon |
| Signatures | All ten public archives pass `dotnet nuget verify --all`; NuGet repository signatures retained |
| Fresh broad consumer | 153 checks pass; all ten public assembly versions1.16.5.0 |
| Fresh public focused consumer | 338 passed,0 failed,0 skipped; uses the previously reviewed public-only fixture |
| Provenance | nuget.org-only source mapping and independent fresh caches; public signed archive/cache/output DLL hashes and restore hashes match; exact assembly informational revision verified |
| Dependency advisories | No vulnerable top-level or transitive packages reported by either public consumer graph |

Public signed archive hashes are recorded separately from local prepublication archives. NuGet signing and platform-specific builds are not assumed byte-identical to the earlier Windows candidates. The verified chain is the approved workflow revision, public package repository/assembly revision, then signed public archive/cache/output bytes. Preliminary and final preparation receipts are preserved without relabeling them as public-package executions.

The initial public license check compared Linux package bytes with a Windows checkout containing CRLF line endings. Investigation confirmed the public license matches the approved Git blob exactly. Public README/license/branding verification now compares against approved committed bytes; the original investigation is retained. This corrects the verifier's platform assumption without changing packages or expected content.

All ten libraries target `net10.0`. Legacy remains the host default; XSLT4 support remains experimental. Selected native draft tests remain219 passed /36 failed /22 skipped across 277 cases:33 catalog/draft mismatches and 3 unresolved contracts. Those failures were not hidden or promoted into passing gates. Frozen QT3 31,142/0/679 and qt4 gated 8,278/0/1894 are retained from exact-source verification.

Public VS Code 0.2.4 continues to bundle engine 1.8.0. Extension alignment, Braid production adoption and the catalog/specification backlog are separate work. No installed extension, Braid pin, upstream test or REQ acceptance status changed. Engine work and the approved library publication are complete; no automatic next feature or release is started.
