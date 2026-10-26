<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.26.2 candidate release notes">
  <h1>Bosak 1.26.2 candidate release notes</h1>
  <p>26 October 2026 — unpublished patch candidate</p>
</div>

Bosak 1.26.2 corrects numeric rounding while retaining existing public APIs. [Verified candidate](RELEASE-1.26.2-CANDIDATE.md) pins source `6a3372fc3b91e733b71a426cf40f26feaaeb3bcb`; public NuGet installation remains **1.26.1**.

- `fn:round` and `fn:round-half-to-even` accept existing supported decimal-backed integer precision values and handle extreme precision before narrowing or computing powers.
- Decimal rounding uses exact coefficients; finite floating inputs use their exact IEEE values and destination-format conversion. This prevents spurious scaling overflow, NaN and infinity, while retaining midpoint rules and signed zero.
- Integer rounding results retain integer identity, including supported decimal-backed values beyond Int64. Final results outside current decimal storage still raise FOAR0002.

For example, `round(2.5,29)` returns decimal 2.5; `round-half-to-even(2.5e0,309)` returns double 2.5; negative floating values rounded to zero retain negative zero. Supported argument conversion, empty behavior and function metadata remain unchanged.

All **18,821 source/LSP tests**, **153 broad package checks** and **8,114 focused package tests** pass. The focused consumer includes all **4,201 new source cases** unchanged. Both frozen conformance gates remain unchanged. Ten unsigned local packages and 19 consumed assembly identities have matching pinned-source provenance. Both fresh dependency audits report no vulnerable packages at verification time.

Fixed-precision numeric storage and general constructor/cast limits remain; this patch does not add arbitrary-precision decimals or integers. REQ-126 stays partial. Current-native Unix retains its two known negative-domain failures (22/2/0); no full current-draft conformance is claimed. Earlier rounding diagnostics include a separately documented correction of 12 mistaken expectations; original evidence is preserved.

No package upload, tag, release or public signature verification occurs in candidate preparation. The owner reports Braid's smoke test finished; no outcome or new acceptance has been supplied. Publication remains a separate owner decision.
