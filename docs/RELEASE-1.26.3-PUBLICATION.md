<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.26.3 verified publication">
  <h1>Bosak 1.26.3 publication</h1>
  <p>26 October 2026</p>
</div>

**Published and independently verified.** [Bosak v1.26.3](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.26.3) uses owner-approved source `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f` and annotated tag object `f8b89011f26192644dae2d41143716eeb0149d5f`. Integration [PR465](https://github.com/Fytala-Charles/Bosak/pull/465) passed all three checks and 21,882 solution/LSP tests, then merged through the protected-main workflow. The tag points at the verified package source, excluding later candidate documentation. The owner explicitly approved merge, tag and publication of this pinned candidate.

| Verification | Observed result |
|---|---|
| Single tag-triggered [Release run](https://github.com/Fytala-Charles/Bosak/actions/runs/40322323357) | 21,798 solution passes; zero failures/skips; ten upload acknowledgments |
| Public packages | Ten signed 1.26.3 archives; exact source/dependencies/XML documentation/Git README/license/icon/brand assets |
| Signatures | All ten `dotnet nuget verify --all` checks pass |
| Fresh nuget.org-only broad consumer | 153 checks; ten assembly identities |
| Fresh nuget.org-only focused consumer | 11,175 tests; zero failures/skips; nine assembly identities |
| Provenance | All 19 identities are 1.26.3.0 with informational revision `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f`; signed archive/cache/output DLL bytes match |
| Restore integrity | Raw signed archive SHA-512 and signature-normalized NuGet content hashes match their respective restore records |
| Dependency audits | Neither fresh transitive audit reports vulnerable packages |

The patch corrects abs Int64.MinValue handling and supported integer result identity in abs, floor and ceiling. The argument audit passed without an engine change. It adds no public API or numeric storage expansion. [Release notes](RELEASE-NOTES-1.26.3.md) retain scope and limits. The focused consumer retains 8,114 prior cases and adds all 3,061 integer-preservation/audit source cases from three byte-identical test files. It retains 47 public conditional cases and 17 independent Braid probes. Counts overlap and are not added into conformance totals.

[Candidate validation](RELEASE-1.26.3-CANDIDATE.md) freshly passed QT3 **31,142/0/679** and frozen qt4 **8,436/0/1,894** across 224 configured sets, with 11 existing Experimental fallback passes. Current-native Unix remained **22/2/0, exit 2**, retaining negative-domain cases 120/121. Release reran the solution; manual gates are inherited from that exact-source candidate. No full current-draft conformance or REQ-126 closure is claimed.

Original diagnostic and Braid adoption evidence is unchanged. Fixed numeric storage, general wide constructors/casts, Int64 idiv, schema-list/general coercion and DateTimeOffset Unix limits remain explicit. XSLT authoring is unchanged since public 1.26.2. Braid's earlier captured local automated 1.26.2 adoption report remains bounded by its uncommitted/no current-adoption CI or new native UI acceptance status at capture; no 1.26.3 adoption is inferred.

Install `dotnet add package Bosak.XPath.Api --version 1.26.3` or `dotnet add package Bosak.Xslt --version 1.26.3`.

[Machine-readable proof](RELEASE-1.26.3-PUBLICATION.json) and [raw evidence](release-evidence/1.26.3-publication/) retain authorization, integration CI, tag, single Release run, propagation observations, signatures, public test receipts and complete provenance. Original archives, fresh caches and attempts remain under `.guard-tmp/release1263-publication`. Propagation observations remain distinct from successful verification. Original unsigned candidate evidence is unchanged. No retag or duplicate dispatch occurred.
