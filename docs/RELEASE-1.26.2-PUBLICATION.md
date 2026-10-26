<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.26.2 verified publication">
  <h1>Bosak 1.26.2 publication</h1>
  <p>26 October 2026</p>
</div>

**Published and independently verified.** [Bosak v1.26.2](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.26.2) uses owner-approved source `6a3372fc3b91e733b71a426cf40f26feaaeb3bcb` and annotated tag object `fb01b31412fd1f689228bd2f32285b955642b3f8`. Integration [PR463](https://github.com/Fytala-Charles/Bosak/pull/463) passed all three checks and 18,821 solution/LSP tests, then merged through the protected-main workflow. The tag points at the verified package source, excluding later candidate documentation. The owner explicitly approved merging PR463 and tagging/publishing this source after automatic review required confirmation; the rejected earlier command did not execute.

| Verification | Observed result |
|---|---|
| Single tag-triggered [Release run](https://github.com/Fytala-Charles/Bosak/actions/runs/40308530558) | 18,737 solution passes; zero failures/skips; ten upload acknowledgments |
| Public packages | Ten signed 1.26.2 archives; exact source/dependencies/XML documentation/Git README/license/icon/brand assets |
| Signatures | All ten `dotnet nuget verify --all` checks pass |
| Fresh nuget.org-only broad consumer | 153 checks; ten assembly identities |
| Fresh nuget.org-only focused consumer | 8,114 tests; zero failures/skips; nine assembly identities |
| Provenance | All 19 identities are 1.26.2.0 with informational revision `6a3372fc3b91e733b71a426cf40f26feaaeb3bcb`; signed archive/cache/output DLL bytes match |
| Restore integrity | Raw signed archive SHA-512 and signature-normalized NuGet content hashes match their respective restore records |
| Dependency audits | Neither fresh transitive audit reports vulnerable packages |

The patch corrects extreme precision handling, decimal scaling, exact IEEE midpoint decisions, signed zero and wide-integer result identity. It adds no public API or arbitrary-precision storage. [Release notes](RELEASE-NOTES-1.26.2.md) retain scope and limits. The focused consumer retains 3,913 prior cases and adds all 4,201 rounding source regressions/controls from two byte-identical test files. It retains 47 public conditional cases and 17 independent Braid probes. Counts overlap and are not added into conformance totals.

[Candidate validation](RELEASE-1.26.2-CANDIDATE.md) freshly passed QT3 **31,142/0/679** and frozen qt4 **8,436/0/1,894** across 224 configured sets, with 11 existing Experimental fallback passes. Current-native Unix remained **22/2/0, exit 2**, retaining negative-domain cases 120/121. Release reran the solution; manual gates are inherited from that exact-source candidate. No full current-draft conformance or REQ-126 closure is claimed.

Original diagnostic evidence is unchanged: 12 wrong expectations are corrected separately from 600 measured rounding corrections. Fixed numeric storage, general wide constructors/casts, wide abs, Int64 idiv, schema-list/general coercion and DateTimeOffset Unix limits remain explicit. XSLT authoring is unchanged since public 1.26.1. The owner reports Braid smoke completion; no supplied outcome, new adoption or native acceptance is inferred.

Install `dotnet add package Bosak.XPath.Api --version 1.26.2` or `dotnet add package Bosak.Xslt --version 1.26.2`.

[Machine-readable proof](RELEASE-1.26.2-PUBLICATION.json) and [raw evidence](release-evidence/1.26.2-publication/) retain authorization, integration CI, tag, single Release run, propagation observations, signatures, public test receipts and complete provenance. Original archives, fresh caches and attempts remain under `.guard-tmp/release1262-publication`. Propagation observations remain distinct from successful verification. Original unsigned candidate evidence is unchanged. No retag or duplicate dispatch occurred.
