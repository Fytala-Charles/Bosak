<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.2.0 release preparation notes">
  <br><br><h1>Bosak 1.2.0</h1>
  <p>XSLT 4.0 construction, named types and template-pattern improvements</p>
</div>

## Release scope

**Prepared candidate; unpublished.** Scope frozen by the user on 2026-10-13 through source revision `8a5164b4123c04836b1c06da9ab3963dbaa39c57` (PR #246). Version 1.1.3 remains the public release. This is a coordinated minor release of the ten public .NET 10 libraries. Existing compatibility profiles continue to govern 4.0 features; the release does not imply complete draft conformance.

The release preparation also enables XML API documentation generation for the coordinated packages, matching the repository convention. It stabilizes an existing validation-cancellation regression by triggering cancellation between walked elements instead of relying on a 1 ms timer; production cancellation behavior is unchanged.

The completed slices provide:

- XSLT 4.0 array/member construction and error enforcement, select-body XPath evaluation with its declaring namespaces, and switch support.
- Named item types covering supported atomic, enumeration, array, map, function, choice, record and node forms, including nested aliases, binding conversion and instance-of/treat checks.
- Named record constructors, supported field defaults/conversion, nominative identity and map-to-record coercion. Plain-map membership is not treated as named-record membership.
- Supported package type exposure/acceptance, schema-related aliases, fixed namespace bindings and inline-function type contracts.
- Standalone template type patterns, supported wildcard/node-name tests, 4.0 union/set/forward-axis priorities, parenthesized node-pattern semantics, lexical delimiter/operator handling and candidate focus for axis set operands.
- Fixed namespace resolution in template patterns and compile-time XPST0081 diagnostics for unavailable lexical prefixes in effective 4.0 template patterns.

[Preparation PR #247](https://github.com/Fytala-Charles/Bosak/pull/247) merged the linear PR #223–246 stack at `ed785f7979fd04552df7549a0d502d21462e8010`. [Final candidate CI](https://github.com/Fytala-Charles/Bosak/actions/runs/38412051528) passed. The merged tree exactly equals the tested candidate tree; all 24 slice heads are ancestors of main and their superseded PRs are closed. Earlier slices already on main also form part of the 1.2.0 release. Original contribution assertions and upstream fixture versions remain unchanged.

## Verification

The initial Release build and packing succeed with zero errors and 71 warnings, including existing XML comment issues exposed by documentation generation. All ten local packages pass metadata/content/hash verification; 45 fresh-cache package-only consumer checks pass and all ten assembly versions are 1.2.0.0. All 7,676 unit tests pass with zero failures/skips, including 4,790 XSLT tests. Final candidate CI passed on `d6316237`; the merged tree is identical. The documentation integration receipt triggers the final main-based CI check before tagging. The source baseline has 7,676 passing unit tests across ten assemblies, frozen QT3 31,142/0/679 and qt4 gate-only 8,278/0/1,894 with zero gated failures. No test sets are promoted for this release.

Native next-match, including si-next-match, remains 46/8/6; type-alias remains 22/10/0. These filtered runners retain known exploratory failures and exit 1. The original five contribution batches' sample cases pass in recorded evidence; upstream review and independent-processor confirmation are separate from Bosak release readiness.

Preparation checks the versions, internal dependencies, .NET 10 assemblies, XML documentation, README/license/icon and SHA-256 hashes of all ten packages, followed by a consumer restored from a fresh cache using package references only. Local-package checks do not establish successful nuget.org publication. [Preparation evidence](RELEASE-1.2.0-PREPARATION.json) records the actual results and immutable candidate revisions.

## Known boundaries and follow-up work

Authoring/static-context alignment, string-template interpolation prefix checks, full eager pattern compilation, wider pattern grammar and schema-defined atomic priorities remain follow-ups. Lower-version native fixtures requiring processor-level 4.0 selection and record/result assertion-adapter cases remain visible. Annotated function types and remaining schema/record cases need separate slices. Existing decimal precision, bounded date/time operations and remote HTTP test limitations remain documented in the integration guide.

REQ-126 remains In Progress. Upstream contribution review or a second processor's agreement is not claimed. Release preparation freezes the delivered scope rather than closing the catalog or all exploratory failures.

After publication, resume focused implementation slices and select a new batch of XSLT 4.0 test areas based on specification gaps and practical consumer benefit. Retain independent assertion review, unchanged normative expectations, source-pinned evidence and the frozen 3.1 gate.

## Publication procedure

After the preparation PR is integrated and main CI is green, the final release revision is ready for approval. Creating the `v1.2.0` tag triggers the existing Trusted Publishing workflow. Publication is followed by downloading all ten public packages, checking their metadata and hashes, and repeating the consumer against nuget.org with a new cache. Publication has not been authorized by the scope-freeze decision alone. The VS Code Marketplace release is a separate workflow and is outside this package release.
