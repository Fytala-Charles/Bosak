<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.25.2 consolidated candidate evidence">
  <h1>Bosak 1.25.2 validated release candidate</h1>
  <p>25 October 2026 — unsigned local candidate; unpublished</p>
</div>

**Pinned source: `3785672d695cbc8467e1c027d5ce4304e9f92152`**, PR449 merge, complete tree identical to reviewed `0489b5cd60dcf91f0ed92dd88a32ba1a3479273d`. [Reviewed-head CI](https://github.com/Fytala-Charles/Bosak/actions/runs/40169031220) passes all three checks and 12,143 solution/LSP tests. This document records fresh consolidated verification of that immutable source. Documentation commits are separate and must not be used as the package source.

[Candidate release notes](RELEASE-NOTES-1.25.2.md) summarize unary negation, integer remainder and exact truncated decimal-backed idiv. Implementation PRs445/447/449 and investigation PRs446/448 are merged. Public installation remains 1.25.1. No new API, layer, version bump, gate membership, fixture, retry policy, upstream adoption or publication accompanies this preparation. REQ-126 stays partial; REQ-138 is a separate design track.

| Check | Fresh result |
|---|---|
| Release build | Zero errors; existing CS8604 warning |
| Solution tests | 12,059 passed /0 failed /0 skipped |
| Separate LSP tests | 84 passed /0 failed /0 skipped |
| QT3 | 31,142/0/679 |
| Frozen qt4 | 8,436/0/1,894; 224 configured/executed sets |
| Current native Unix | 22/2/0, expected exit2 under frozen runner |
| Local-package broad consumer | 153 checks; all ten libraries |
| Local-package focused consumer | 1,429 passed /0 failed /0 skipped; nine libraries |
| Fresh transitive audits | Neither consumer reports vulnerable packages; no metadata availability errors |

The 384 added cases are counted independently by class in the focused package consumer, joining 1,045 retained checks. The existing 25-case UnixDateTimeExperimentalTests file is refreshed from current source; its obsolete minimum-literal limitation expectation was corrected in PR445. Expected case count is unchanged. Source and package counts overlap; skips and the 11 existing Experimental fallback passes do not certify full conformance. Native negative-domain failures remain explicit.

All ten local 1.25.2 archives have the exact repository commit, version and internal dependency pins. PackageReference-only consumers use separate fresh caches with Bosak.* mapped exclusively to the local feed. Nineteen consumed assembly versions/informational revisions and archive/cache/output DLL bytes agree; unsigned archive SHA-512, NuGet restore metadata/contentHash and source metadata also agree. XML documentation and README/license/icon/brand assets match source bytes. Packages are unsigned local artifacts; no public repository-signature or signed public-consumer verification is claimed.

The official vulnerability index was reachable with HTTP200, and the public Bosak.Xslt index did not list 1.25.2 at preflight. [Machine-readable proof](RELEASE-1.25.2-CANDIDATE.json) records exact test counters/hashes, all package/dependency/assembly identities, cache/source verification and native gate receipts. [Retained evidence](release-evidence/1.25.2/) includes normalized UTF8 audit copies; original raw TRX/log/audit files and fresh caches remain under `.guard-tmp/release1252-candidate`, archives under `.guard-tmp/release1252-packages`.

The XSLT authoring source is byte-unchanged since published 1.25.1. Generic wide constructors/storage/errors, bounded decimal precision, double-negation/coercion, the Int64 idiv result range, years 1–9999 Unix calendar and 28 unavailable historical adopted pins remain separate. REQ-126/D2/D3/D4/AC-03 are not closed.

## Publication boundary

The owner authorized consolidation, not tagging or publication. A later approved release must use the pinned source above, then independently verify signed public packages and fresh public consumers. Braid acceptance of a prior release is not reused as acceptance of this candidate. Pending local REQ-138/upstream notes, retained stashes and protected PR132 remain preserved. No tag, GitHub release or NuGet upload has been created by this step.
