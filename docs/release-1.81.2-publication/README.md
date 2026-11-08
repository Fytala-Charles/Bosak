# Approved 1.81.2 / extension 0.2.6 publication

8 November 2026. **Published and service-verified: ten NuGet1.81.2 packages and extension0.2.6.** [User approval](APPROVAL.json) authorizes release scope/API and signing/provenance for all four owner roles, tags and publication over the exact verified hosted artifacts from source `7249d0e01628c47f92b89a3380bc69cf58d70045`. Numerical workload budgets are explicitly deferred for this release, with **no performance guarantees claimed**. This does not deliver production Schema/XmlDb/Braid provider adoption or invent workload results.

NuGet preparation run42479094503 and extension preparation run42479096917 supply the approved manifests retained in the [hosted dossier](../release-linux-collection-hosted/README.md). Publication must reuse these artifacts, without rebuilding. Older unfilled review forms and open-gate entries remain historical and are superseded for this scoped release by this dated user disposition, not rewritten as independently signed owner returns.

[Registry preflight](REGISTRY-BEFORE.json) found none of the ten1.81.2 package versions or extension0.2.6 listed immediately before publication. Execution and service-side verification receipts follow separately; approval alone is not a claim that publication has occurred.

## Execution

Both annotated remote tags resolve to the approved producer source, retained in [tag receipts](REMOTE-TAGS.txt). [NuGet publication run42510683197](https://github.com/Fytala-Charles/Bosak/actions/runs/42510683197) and [Marketplace publication run42510685414](https://github.com/Fytala-Charles/Bosak/actions/runs/42510685414) succeed. Their preparation jobs are skipped; each publication job validates its prior successful preparation, downloads the approved artifacts, checks exact manifests/bytes and publishes without rebuilding. NuGet OIDC authentication succeeds and all ten pushes report success; Marketplace confirms version0.2.6.

[Marketplace service verification](MARKETPLACE-SERVICE-VERIFICATION.json) finds0.2.6 listed and its public VSIX download **byte-identical** to the approved hosted artifact. [NuGet service verification](NUGET-SERVICE-VERIFICATION.json) records public availability separately; initial404 responses remain in the first attempt receipt. Upload success alone is not package download verification. Raw run/job metadata and zipped publisher logs are retained here.

## Delivery verified

**All ten NuGet1.81.2 packages are publicly listed and downloadable.** Every original ZIP payload entry matches the approved hosted package; the only added entry is NuGet's `.signature.p7s`. [Service comparison](NUGET-SERVICE-VERIFICATION.json), [delivered signed packages](NUGET-DELIVERED.zip). This entry comparison does not claim an independent certificate trust assessment. Marketplace0.2.6 is listed and its VSIX is byte-identical: [comparison](MARKETPLACE-SERVICE-VERIFICATION.json), [delivered VSIX](MARKETPLACE-DELIVERED.zip).

A new nuget.org-only restore into a fresh private cache resolves exactly the ten1.81.2 libraries, builds and passes seven ordinary XPath/XQuery/XSLT/Database-presence smoke controls. All ten output assembly hashes match the approved hosted package DLLs. [Consumer results](PUBLIC-CONSUMER-RESULTS.json), [provenance](PUBLIC-CONSUMER-PROVENANCE.json), [raw restore/build/run receipts](PUBLIC-CONSUMER-RECEIPTS.zip). The unchanged diagnostic caller's false approval fields do not grant or revoke the separate release-owner approval. No vulnerability audit or performance acceptance is claimed.

[Final results](RESULTS.json) supersede the initial propagation404 observations. No artifacts were republished or rebuilt to address propagation. The approved workload budget deferral remains in force with no performance guarantees. Run `python docs/release-1.81.2-publication/verify.py` for retained evidence consistency; the network service verifier is a separate read-only recheck requiring network access.

The network recheck writes new observations under `.guard-tmp/published-1812/service-recheck`, preserving the sealed publication receipts. Run it from the repository root.
