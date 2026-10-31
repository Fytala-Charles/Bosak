# Open Schema integration qualification

29 October 2026. [Runnable example and commands](../../integrations/Schema/README.md). [Pre-implementation scope and immutable specification sources](ADOPTION.md). [Public Schema source pin](SOURCE-PIN.json).

## What this delivers

The actual public Schema source compiles against current Bosak projects in an optional development harness. Its activation-free factory imports an in-memory XSD 1.0 schema, strictly validates a copied price element, exposes its named type and decimal typed value, and calculates a total. The caller's source tree remains unchanged and unannotated. The schema resolver serves only the bundled schema and throws for other requests rather than invoking URI fallback.

The integration tests also exercise staged XSD 1.1 assertion success/failure, refused external-resource functions and explicit rejection of 1.1 constructs under the 1.0 compiler. A retained extra-child example proves why assertion success must not be described as full structural validity.

The harness verifies SHA-256 identities for the twenty public library sources, twelve existing test sources, two schema fixtures and Apache license/notice. It accepts the exact LF or CRLF representation of the same pinned content. The normal Bosak solution has no new dependency on Schema, and the public Schema checkout/package pins are untouched. A dedicated optional CI job checks the integration; local execution does not establish a hosted CI result.

## API decision

The first useful XSD 1.0 path needs **no new core public interface or value-layout change**. The earlier 32-type session proposal is therefore not adopted wholesale. Existing compiler/resolver, validation-copy and typed-value APIs suffice for this example.

The next XSD 1.1 step belongs in the schema engine: define a precisely bounded assessment result that retains the assessed tree, governing components, actual types and diagnostics, and explicitly identifies unsupported structural coverage. Its existing `SchemaValidationResult` is an assertion result, not that contract. Do not expose its private typed clone as a fully assessed document or invent full component authority from a root-document digest.

Only then qualify how Bosak consumes that result, including atomization, copies, compatible independent schema universes, anonymous types, cancellation and caller ownership. C44/C44-R2 remain valuable acceptance requirements; their sealed proposal and pending decision are historical evidence. No production session API, REQ-139 completion, ADR-006 acceptance, profile change or API freeze follows from this example.

The [next slice](NEXT-SLICE.md) defines the concrete backend-to-assessed-document work and acceptance cases.

## Qualification boundaries

See [RESULTS.json](RESULTS.json) and the retained evidence for exact commands, counts, source identities and environment corrections. Source compilation, unchanged Schema binary replacement and consumer suites are separate observations. The legacy core solution and frozen gates remain separate from the additional optional Schema tests.

| Scope | Passing evidence |
|---|---|
| Complete core solution | 24,203 tests; fresh build and run. |
| Actual Schema source integration | 122 cases: 114 existing and eight new. |
| Unchanged Schema library/test binaries with candidate core | 114 cases. |
| Actual XmlDb adapter source with verified candidate references | 27 cases; all resolved core assemblies match current Release outputs. |
| Unchanged XmlDb integration binaries with candidate core | All 593 original cases have passing evidence: 590 initial passes, one layout-corrected reopen rerun and two timeout cases covered by a fresh six-case restore run. Assertions and deadlines are unchanged. |
| Actual Braid source | 1,872 unchanged passes plus four separately passing cases after an isolated exact expected-version adjustment. |
| Frozen QT3 | 31,397 passed / 0 failed / 424 skipped; identical skip names/reasons. |
| Gated qt4 | 8,941 passed / 0 failed / 1,918 skipped; 230 gated sets, eleven experimental passes, identical skip names/reasons. |

The independent XmlDb storage baseline was stopped after 1,258 observed passes to end a competing disk-fault sweep. Its run-level cancellation remains recorded; this is not a complete storage-suite pass. The completed published-dependency baselines and corrected candidate source/binary routes are labeled separately. Braid's four original version failures and XmlDb's three original binary-run failures remain visible rather than being overwritten by reruns.

XmlDb and Braid are tested from isolated archives of their committed revisions. Uncommitted sibling changes are excluded. Candidate routing changes only build metadata; the original repositories and adopted dependency pins remain untouched. Braid's four exact-version assertions initially reject the new identity; their separate rerun changes only that expected identity in the isolated test copy. This is compatibility evidence, not consumer adoption or new native UI acceptance.

The public source remains candidate 1.33.1 / verified published 1.26.3; this harness changes neither package version nor NuGet publication. The development Schema wrapper is not packable. Publication requires a separate reviewed release.

## Local preview packages

A coordinated **unsigned local preview** is prepared under `.guard-tmp/schema-preview-packages`: nine Bosak packages at the existing candidate 1.33.1 and Schema at the local-only label `0.4.0-alpha.1`. An isolated copy of the original public Schema project changes only its four core dependency pins to 1.33.1; the wrapper project is not used for packaging. The public Schema checkout and its 1.26.3 pins remain unchanged. The local version label is not an allocated/public release.

The [package inventory](evidence/local-preview-packages.json) records hashes, exact dependency versions, notices and source/build provenance. A package-only application restores from the local feed, resolves all ten exact candidate versions and runs the typed-price example successfully. All ten output assemblies match their package payloads. Core and Schema packages contain their Apache notices; no Schema dependency is introduced into the core packages.

No packages were signed, uploaded or adopted by consumers, and no release tag was created. This is a reviewable preview for the bounded existing API path, not full XSD 1.1 or session-API acceptance.
