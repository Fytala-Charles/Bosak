# Bosak 1.26.2 adoption and rounding smoke

26 October 2026. Published [Bosak 1.26.2](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.26.2) is adopted at **6a3372fc3b91e733b71a426cf40f26feaaeb3bcb**. Annotated tag object `fb01b31412fd1f689228bd2f32285b955642b3f8` independently peels to that immutable revision. This follows the committed 1.26.1 adoption at Braid `1b4e97b`; the current adoption remains uncommitted.

## Delivery and provenance

All ten public NuGet packages at version 1.26.2 pass `dotnet nuget verify --all`. Each nuspec identifies the expected package ID/version and released repository commit. A separate initially empty package cache restores a PackageReference-only consumer from nuget.org. All ten loaded assemblies have version `1.26.2.0` and informational version `1.26.2+6a3372fc3b91e733b71a426cf40f26feaaeb3bcb`.

Fresh official archive and restored archive SHA-256 identities match. Archive/cache/output DLL hashes match for all ten libraries. Raw archive SHA-512 matches the cache sidecar; NuGet contentHash matches restore assets and metadata identifies nuget.org. The public consumer dependency audit reports no vulnerable packages. Downloads, signature logs, receipts, standalone consumer sources and TRXs remain ignored under `.acceptance/bosak-1.26.2-review`; its initially empty cache is preserved at `.dependencies/bosak-1.26.2-public-cache`.

The manifest, nine solution engine paths and exact informational-version assertion adopt the release. The separate revision-specific dependency checkout has no tracked modifications. Bosak engine sources and upstream records were not edited.

## Consumer smoke results

- **246 public XPath API cases** exercise XPath 3.1, XPath 4.0 and XPath 4.0 Experimental, direct calls and named function references for `round` and `round-half-to-even`.
- **72 Braid preview cases** validate ordinary XSLT 3.0 and compare exact text-output bytes for the same successful predicates.
- **280 signed-public-package checks** pass separately: the 246 rounding cases, 33 retained 1.26.1 numeric cases and one ten-assembly identity check. These overlap the integrated tests and are not additive conformance coverage.

Predicates verify decimal precision 29, double precision 309, large finite values without spurious NaN/infinity, subnormals, extreme precision beyond Int64, retained integer identity and widened rounding results, midpoint differences, exact binary input rounding, negative zero, infinities, NaN and empty input. Required-error controls cover argument type/cardinality, invalid untyped conversion and final decimal-storage overflow (`XPTY0004`, `FORG0001`, `FOAR0002`). The historical `1e300` at precision -29 expectation correctly preserves the same double.

Full Braid Release verification: **1,876 passed, zero failures/skips** (27 Core /514 Application /965 Infrastructure /370 Workbench). The 318 new integrated cases retain the prior 40 numeric regressions. Fresh desktop publish passes licensing-package hygiene for 113 files. Strict documentation and whitespace checks pass.

## Scope and restart

No public API or Braid authoring operation is added. Supported decimal/integer storage remains bounded; general wide constructors/casts and broader numerical conformance remain separate. Bosak REQ-126 remains partial. The existing native functional acceptance covers the 1.26.0 conditional workflow; this automated smoke does not add native UI or accessibility acceptance. No upstream acceptance notification is due and no current-adoption CI result is claimed.

Preserve the existing local credential, accepted native host and earlier evidence. See [restart checkpoint](RESTART_CHECKPOINT.md). This task does not commit or push the new adoption.
