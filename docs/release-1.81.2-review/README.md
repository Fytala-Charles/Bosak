# Exact-artifact release review ? 8 November 2026

This packet brings the remaining review to the qualified **1.81.2 / extension 0.2.6** source `1d69d059c14888c1c43264efa8668909292af858`. It replaces no historical receipt and grants no acceptance. [Machine-readable decisions and immutable input hashes](REVIEW.json) deliberately leave all owner decisions unfilled.

## Concrete review inputs

- [Ten NuGet packages](../release-1.81.2-final/evidence/NUGet-1.81.2-reviewed.zip) and [exact package manifest](../release-1.81.2-final/evidence/nuget-release-manifest.json).
- [Extension 0.2.6](../release-1.81.2-final/evidence/vsix-out/vscode-bosak-0.2.6.vsix) and [exact VSIX manifest](../release-1.81.2-final/evidence/vsix-out/release-manifest.json).
- [Full qualification and limitations](../release-1.81.2-final/README.md), [API declarations](../release-1.81.2-final/evidence/public-api.json) and [API comparison](../release-1.81.2-final/evidence/API-COMPARISON.json).

The two manifest hashes are recorded directly from their bytes in REVIEW.json. These are local Windows artifacts; a hosted preparation run must receive its own review over its own manifest hashes.

## Decisions still required

| Decision | Required return |
|---|---|
| Scope and deferred providers | Explicitly accept the ten Core libraries and extension boundary, and dispose of deferred Schema/XmlDb/Braid production requirements. Compatibility probes alone do not do this. |
| Core, Schema, XmlDb, Braid acceptance | Dated reviewer identity, exact candidate pins, supported/refused profile and assigned/shared scenario dispositions. |
| PF-01 through PF-09 budgets | Immutable workloads and independently comparable baselines; numerical units, statistics, limits and tolerances; raw correctness/performance/lifetime evidence. No historical budget is transferred automatically. |
| C45/public API | Dated acceptance over this API inventory, supported profile and deferred requirements. |
| Signing/provenance | Decide required signing and provenance for each destination, validate the resulting artifacts and record trust evidence. ZIP signature-entry inspection alone establishes no trust. |
| Hosted preparation | Explicitly authorize pushing the reviewed checkpoint and running both workflows with publish=false. Record successful run IDs, source SHA and freshly reviewed manifest hashes. |
| Publication | Separate explicit approval over final hosted source/artifacts, destination and tags after applicable gates close. |

The [prior owner review](../foundation-package-workload/OWNER-REVIEW.md) supplies scenario assignments and measurement requirements only. Its templates pin an older preview and must not be submitted unchanged for this cohort. No owner message has been sent.

## Recommended execution

First preserve and review the evidence checkpoint. Then, with explicit instruction, push and run preparation only using the [release guide](../RELEASING.md). Owner/API and signing decisions can proceed alongside hosted preparation; publishing remains blocked until those decisions are recorded. Do not add signing after artifact approval without producing and reviewing new hashes.

Run `python docs/release-1.81.2-review/verify.py` to check this packet's pinned inputs and unfilled status. It is a consistency check, not an acceptance mechanism.
