# PV-Q08 — local preview packages and package-only consumption

7 November 2026. Nine local preview packages have been built as
**1.81.1-preview.c0b926de** from immutable checkpoint
`c0b926de34824ceb2010d5f814a4450b28b31dcc`. This is an explicitly selected local
preview identity, not an approved public release version. No package was uploaded,
tag created, owner acceptance granted or API freeze declared.

The [plan](PLAN.json) records the source archive, recipe overlay, package closure
and checkout materializations. All **669 qualification source hashes** and
qualified package asset hashes match before building. Git stores some unchanged
files with LF while the sealed Windows qualification used CRLF. The recipe
records 191 newline-only materializations with both hashes; any other difference
refuses. The first two extraction attempts are preserved in the ignored task
area: one stopped on newline differences, the other on missing pinned test-source
paths. Neither built packages. The successful third extraction includes the exact
pinned paths and retains a [source bundle](evidence/SOURCE-BUNDLE.zip).

The new preview assemblies were freshly compiled with a consistent preview
Version/PackageVersion and exact SourceRevisionId/RepositoryCommit. They are
distinct binaries from the earlier production build; earlier correctness receipts
are not automatically qualification of these package bytes. Both the nine-library
build and consumer build have **zero warnings and errors**.

[Download the nine local preview packages](evidence/PREVIEW-PACKAGES.zip).
[Package inventory](evidence/PACKAGES.json) records SHA-256, NuGet SHA-512,
dependencies and checked contents. The selected Bosak.Xslt closure contains API,
Compiler, Core, Parser, Providers, Runtime, Standard, XQuery and XSLT. Language
server and database packages are outside this preview.

Checks completed:

- Each package ID/version and repository commit match the plan; every dependency
  matches the selected preview version and actual project-reference graph.
- License, NOTICE, README, icon, canonical brand assets, XML documentation and
  Standard's Unicode license match their archived source bytes. The existing
  explicit NOTICE overlay is retained; production pack metadata is unchanged.
- A consumer using only one PackageReference to Bosak.Xslt restores solely from
  this local feed into a newly empty private cache, with fallback folders cleared.
  Its assets file contains exactly the nine preview packages, without project
  references, HintPath references or global-cache engine dependencies.
- Six behavior controls pass: XPath arithmetic, empty sequence and map lookup;
  XQuery FLWOR and empty iteration; source-free XSLT named-template execution.
- Package/cache/output/actual loaded DLL hashes agree for all nine assemblies in
  the default noncollectible context. Metadata loads ensure inventory coverage;
  they do not imply behavioral coverage of every package.
- The unchanged retained binary caller passes **20/0** through the mandatory
  scoped launcher with the exact seven-assembly package candidate manifest and
  original caller/configuration hashes.

[Consumer result](evidence/CONSUMER-RESULTS.json),
[retained caller result](evidence/RETAINED-FOUNDATION.json),
[execution layouts](evidence/EXECUTION-LAYOUTS.zip) and
[14 successful execution receipts](evidence/EXECUTION-RECEIPTS.zip) preserve these
checks. Inspection initially assumed consumer output included transitive XML
documentation; the check was corrected to require XML in packages/cache and DLLs
in runtime output. No artifact changed for that verifier correction.

This completes local artifact preparation and bounded consumer screening. It does
**not** complete full package-backed solution/conformance/API qualification,
fresh native/owner workload qualification against these newly compiled bytes,
signing/provenance review, final release SemVer or four-owner C45/API freeze.
The [owner gaps](../foundation-owner-gap-review/README.md),
[production adoption proposal](../foundation-provider-adoption/README.md) and
R4–R7 gates remain open. Nothing is waived by the preview.

Next **PV-Q09**: qualify the exact package cohort against the solution regressions,
frozen QT3/gated qt4 skip maps and API inventory; record non-packaged products and
native/owner binding lanes separately. Preserve this preview if changes require
a new cohort. Final package readiness and publication approval follow those
results and the outstanding owner decisions.

Run `python docs/foundation-preview-packages/tools/verify.py` for source, package,
archive, cache, loaded-byte, receipt and local-link checks. The preparation and
consumer scripts require fresh task paths; do not overwrite original evidence.
Source archive checkout and package contents are checked statically; no new
rendered-document approval or reproducible-build guarantee is claimed.
