<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.26.1 atomic conversion patch notes">
  <h1>Bosak 1.26.1 release notes</h1>
  <p>26 October 2026 — published and independently verified</p>
</div>

Bosak 1.26.1 is a patch release for atomic conversion and cardinality corrections. Public packages are available as 1.26.1. [Pinned source and complete candidate evidence](RELEASE-1.26.1-CANDIDATE.md).

- Repeated unary operators retain atomization, untyped conversion, numeric normalization and required errors instead of cancelling blindly.
- Recursive array singleton consumers count atomic results after processing empty and nested members.
- Empty atomized numeric binary operands and modern range bounds propagate empty before numeric conversion.
- abs now enforces its optional numeric argument contract while preserving coerced function names.
- round and round-half-to-even enforce numeric cardinality and integer precision conversion across direct and dynamic calls, including recursive arrays and untyped nodes. Existing coarse function types and supported static backwards-compatible behavior are retained.

The patch adds 2,420 source regressions and controls. Fresh pinned-source validation passes 14,536 solution and 84 LSP tests. QT3 remains 31,142/0/679 and frozen qt4 8,436/0/1,894 across all 224 configured sets. The separate current-native Unix diagnostic remains 22/2/0 with its known negative-domain cases; no full current-draft conformance is claimed.

Ten signed public packages pass 153 broad and 3,913 focused PackageReference-only consumer checks with separate fresh caches. All 19 consumed assembly identities and archive/cache/output bytes match the exact source. Neither fresh dependency audit reports vulnerable packages at the recorded check.

No new public API or authoring lifecycle is introduced. REQ-126 remains partial. Numerical kernels, extreme precision, generic wide-integer policy, schema-list/general conversion limits and the DateTimeOffset-based Unix calendar remain explicit boundaries. Public 1.26.0/Braid acceptance is unchanged; candidate package checks do not establish signed public artifacts, Braid adoption or new native acceptance.

Published from the owner-approved pinned source. All ten signatures and fresh public consumers are verified. Install `dotnet add package Bosak.XPath.Api --version 1.26.1` or `dotnet add package Bosak.Xslt --version 1.26.1`. [Verified publication](RELEASE-1.26.1-PUBLICATION.md) retains the tag, single Release run and independent public checks.
