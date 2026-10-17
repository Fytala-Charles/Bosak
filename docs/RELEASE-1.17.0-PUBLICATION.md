<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.17.0 publication">
<h1>Bosak 1.17.0 publication verification</h1>
<p>Readable insertion release — 17 October 2026</p>
</div>

Published from approved immutable `v1.17.0` at `15eacde48b47cc8e48af29ddc95949801a2e3e5d`. The single [release workflow](https://github.com/Fytala-Charles/Bosak/actions/runs/39010589845) succeeds; [GitHub release](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.17.0). [Complete receipt](RELEASE-1.17.0-PUBLICATION.json); [release notes](RELEASE-NOTES-1.17.0.md).

| Verification | Result |
|---|---|
| Authorization and consumer feedback | User requested publication and reported Braid found no blocking issues |
| Workflow | Exact approved revision; push event; 9,612 solution tests pass; ten upload acknowledgments; no duplicate dispatch |
| Public packages | All ten 1.17.0 libraries indexed and downloaded from nuget.org; correct repository revision, dependencies, XML docs and committed branding/license assets |
| Public signatures | All ten archives pass dotnet nuget verify --all |
| Fresh broad consumer | 153 checks pass against all ten public libraries |
| Fresh focused consumer | 372 pass, 0 fail, 0 skip: 140 current public insertion controls, 35 readable cases, 197 profile/namespace/fallback/record regressions |
| Provenance | nuget.org-only source mapping; fresh package/HTTP caches; signed public archive/cache/output DLL and restore hashes match; exact 1.17.0.0 and informational revision |
| Dependency advisories | Neither public consumer reports vulnerable top-level or transitive packages |

Public signed Linux-built archives are verified against the approved Git/assembly revision and their own restored/cache/output payloads, rather than assumed identical to unsigned Windows preparation archives. Text assets compare byte-for-byte with committed Git blobs. All prepublication and original implementation failure receipts remain preserved separately.

REQ-129 is implemented and published; the user reports no Braid blockers. Braid source/pin changes and additional native appearance/focus/accessibility acceptance are not inferred or performed by this release. Public VS Code 0.2.5 continues to bundle engine1.16.5. Experimental 4.0 coverage and existing editor limitations remain unchanged.
