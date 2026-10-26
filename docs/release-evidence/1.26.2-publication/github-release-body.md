Bosak 1.26.2 corrects extreme rounding precision and numeric rounding kernels without changing public APIs.

- round and round-half-to-even handle supported decimal-backed integer precisions without narrowing/power hazards.
- Exact decimal coefficients and IEEE binary rational values prevent intermediate scaling overflow and spurious NaN/infinity, preserving midpoint rules and signed zero.
- Integer result identity is preserved, including supported decimal-backed results beyond Int64. Numeric storage and general constructor/cast limits remain bounded.

The single [Release workflow](https://github.com/Fytala-Charles/Bosak/actions/runs/40308530558) passes **18,737 solution tests** and acknowledges **ten uploads**. All ten signed public NuGet packages pass signature and exact-source verification. Fresh nuget.org-only consumers pass **153 broad and 8,114 focused checks**, including all **4,201 added rounding cases**, with 19 matching assembly identities and signed archive/cache/output provenance. Neither dependency audit reports vulnerable packages. Counts overlap.

Candidate/integration validation also passes 84 LSP tests. Frozen QT3 remains 31,142/0/679 and qt4 8,436/0/1,894 across 224 unchanged sets. Current-native Unix retains its two known negative-domain failures (22/2/0); REQ-126 remains partial. Twelve mistaken historical diagnostic expectations are corrected separately with original receipts preserved. The owner reports Braid smoke completion; no outcome/new acceptance is inferred. [Candidate scope and limitations](https://github.com/Fytala-Charles/Bosak/blob/38be251f8c6a00fc8f3295e138f99e413443557a/docs/RELEASE-1.26.2-CANDIDATE.md).

Install: `dotnet add package Bosak.XPath.Api --version 1.26.2` or `dotnet add package Bosak.Xslt --version 1.26.2`.

Approved immutable source: `6a3372fc3b91e733b71a426cf40f26feaaeb3bcb`. One annotated tag push and one tag-triggered Release run, with no retag or duplicate dispatch.
