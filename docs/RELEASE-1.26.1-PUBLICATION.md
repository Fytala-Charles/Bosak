<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.26.1 verified publication">
  <h1>Bosak 1.26.1 publication</h1>
  <p>26 October 2026</p>
</div>

**Published and independently verified.** [Bosak v1.26.1](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.26.1) uses owner-approved source `65417cdb3bff184ca9ff1f624582f55ca4681db9` and annotated tag object `de863097ac4b0caff8d32fe54859dd85b6b9779a`. Integration PR461 passed all three checks and 14,620 solution/LSP tests, then merged through the protected-main workflow. The tag deliberately points at the validated package source, excluding later candidate documentation. The owner authorized tagging and publication before these actions.

| Verification | Observed result |
|---|---|
| Single tag-triggered [Release run](https://github.com/Fytala-Charles/Bosak/actions/runs/40286511584) | 14,536 solution passes; zero failures/skips; ten upload acknowledgments |
| Public packages | Ten signed 1.26.1 archives with exact source, versions, internal dependencies, XML documentation and Git README/license/icon/brand assets |
| Signatures | All ten `dotnet nuget verify --all` checks pass |
| Fresh nuget.org-only broad consumer | 153 checks; ten assembly identities |
| Fresh nuget.org-only focused consumer | 3,913 tests; zero failures/skips; nine assembly identities |
| Provenance | All 19 identities are 1.26.1.0 with informational revision `65417cdb3bff184ca9ff1f624582f55ca4681db9`; signed archive/cache/output DLL bytes match |
| Restore integrity | Raw signed archive SHA-512 and signature-normalized NuGet content hashes match their respective restore records |
| Dependency audits | Neither fresh transitive audit reports vulnerable packages |

The patch corrects repeated unary conversion, recursively atomized singleton/empty operands, abs cardinality, and rounding numeric/precision conversion while preserving coerced function names. The [release notes](RELEASE-NOTES-1.26.1.md) retain scope and limits. The focused consumer retains 1,493 prior cases and adds all 2,420 public source regressions/controls from seven byte-identical test files. It also retains 47 public conditional cases and 17 independent Braid probes. Counts overlap source/package runs and are not added into conformance totals.

[Candidate validation](RELEASE-1.26.1-CANDIDATE.md) freshly passed QT3 **31,142/0/679** and frozen qt4 **8,436/0/1,894** across all 224 configured sets, with 11 existing Experimental fallback passes. Current-native Unix remained **22/2/0, exit 2**, retaining negative-domain cases 120/121. Release reran the solution; it did not rerun manual gates. No full current-draft conformance or REQ-126 closure is claimed.

No new public API or authoring lifecycle is introduced. XSLT authoring is unchanged since public 1.26.0. Its bounded Braid REQ-138 acceptance remains current; 1.26.1 Braid adoption/native acceptance is separate. Decimal/storage/precision/schema-list/general conversion and BC-dynamic limits remain as documented in the candidate.

Install `dotnet add package Bosak.XPath.Api --version 1.26.1` or `dotnet add package Bosak.Xslt --version 1.26.1`.

[Machine-readable publication proof](RELEASE-1.26.1-PUBLICATION.json) and [retained raw evidence](release-evidence/1.26.1-publication/) include the owner authorization, integration CI, tag, single Release run, propagation observations, signature attempts, public test receipts and complete provenance. Original public archives, fresh caches and complete attempts remain under `.guard-tmp/release1261-publication`. Initial NuGet 404 propagation observations remain distinct from successful availability and consumer verification. Original unsigned candidate evidence is unchanged. No retag or duplicate dispatch occurred.
