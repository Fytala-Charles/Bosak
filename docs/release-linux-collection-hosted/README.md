# Corrected-source hosted release preparation

8 November 2026. **Both preparation workflows pass. Nothing is published.** Source commit is `7249d0e01628c47f92b89a3380bc69cf58d70045`; the shipping `src` tree equals the qualified local 1d69d059 source. The [fixture transport correction](../release-linux-collection-fix/README.md) changes harness/test code only. Earlier failed and successful hosted runs remain historical in their original dossier.

| Preparation | Result | Retained artifacts |
|---|---|---|
| [NuGet run42479094503](https://github.com/Fytala-Charles/Bosak/actions/runs/42479094503) | Success: ten packages1.81.2,26345 solution passes in one invocation, exact frozen gates | [Packages, manifest, TRX and gates](nuget-42479094503.zip), [workflow log](nuget-workflow-log.zip) |
| [Extension run42479096917](https://github.com/Fytala-Charles/Bosak/actions/runs/42479096917) | Success: extension0.2.6, server/engine1.81.2,90 server tests | [VSIX, manifest and TRX](vscode-42479096917.zip), [workflow log](vscode-workflow-log.zip) |

QT3 is **31,397/0/424**; gated qt4 **8,941/0/1,918** plus11 experimental passes. Both exact skip maps pass the committed gate checker. All three new cross-platform regression cases pass on Linux. Both downloaded artifact sets independently pass the committed exact-byte verifier against a clean LF checkout of the producer commit, including source/version/dependencies/legal assets. [Results and manifest hashes](RESULTS.json), [VSIX verification](VSIX-VERIFICATION.json), producer run/job/artifact API metadata, and raw logs are retained.

Reviewed manifest SHA-256 values:

- NuGet: `3bfc5cd7116f6a97b1d438084ed9932b274ac26de79e677e92ed46b1644d92cf`.
- VSIX: `82b19dc721b1209c3308d0c510fa0072d0ebc78055ef20607f0a010f08b3fc2d`.

These hashes identify **hosted** manifests and differ from prior local Windows manifests. Do not substitute artifacts across cohorts. Both publish jobs were skipped; no tag, authentication or publication step ran. Structural verification and source/run metadata are not a cryptographic signing trust assessment or owner acceptance.

## Remaining release decisions

Owner workload/baseline/budget acceptance, deferred-provider requirement dispositions, C45/API acceptance, signing/provenance disposition and explicit publication approval remain open in the [release checklist](../RELEASE-READINESS-1.81.1.md). The older [local review packet](../release-1.81.2-review/README.md) retains its original inputs; final approval must cite these hosted run/source/manifest identities. Hosted preparation does not transfer old XmlDb/performance or installed-editor receipts to new binaries. Existing local editor checks exercise the unchanged product source; no fresh installed-editor run of this hosted VSIX is claimed.

Run `python docs/release-linux-collection-hosted/verify.py` to verify retained packet hashes, artifact payloads, producer identities and skipped publication. This is an evidence consistency check, not approval.
