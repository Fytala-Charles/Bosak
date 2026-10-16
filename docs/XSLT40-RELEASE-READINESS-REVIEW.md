# Release readiness and remaining draft failures

Reviewed 16 October 2026 at `48d636c2c2cc66b76bc3e6a5da038bafccdb18e4`, after PRs347-349. Source1.16.3 is unpublished; public libraries1.8.0 and VS Code0.2.4/engine1.8.0 remain unchanged. [Machine-readable case assessment and evidence hashes](XSLT40-RELEASE-READINESS-REVIEW.json).

## Decision

Engineering checks support preparing a release candidate, but publication readiness is not established. Keep the draft profile experimental. Do not require all historical catalog expectations to pass before preparing an explicitly limited candidate, and do not claim full XSLT4 conformance. No implementation, upstream submission, package publication or consumer adoption is part of this review.

| Release criterion | Evidence and remaining work |
|---|---|
| Integrated source | All three milestone PRs merged; exact-head CI38950028063 passed for77c14d8d. Merge and head have identical whole trees0df962d19f58e7accfb63caa7530991724fde039. |
| Regression checks | Existing exact-source receipts: solution9509/0/0, XSLT6575, language-server84/0/0, ten CLI controls, QT331142/0/679, qt4 gated8278/0/1894. Not rerun for this documentation review. |
| Draft scope | Selected native array/record/type-alias:210pass/45fail/22skip across277 cases. This is a selected sample, not a full XSLT suite result. All six profile/family outcomes and details were unchanged by the last slice. |
| Public API/version | Diff from public v1.8.0 adds XsltProcessorProfile and XsltCompiler.ProcessorProfile. Retain legacy default. Release notes and consumer checks must cover the opt-in API; a patch-only release description would be misleading. Existing source minor version1.16 is unreleased. |
| Archives and consumers | Ten-library exact-candidate pack/restore, fresh-cache broad and focused consumers, dependency/assembly/revision/hash verification have not been performed for1.16.3. Old1.8.0 publication evidence cannot establish these. |
| Dependency and editor evidence | Fresh dependency/security review and any future VSIX alignment remain pending. Current public extension bundles1.8.0; source manifest0.2.4 is not evidence of a newly delivered engine. |
| Publication decision | Choose and validate an exact candidate/version, check tag/package availability and obtain publication authorization only after preparation. No tag, release dispatch, upload or Braid pin change performed. |

## Failure classification

**33 catalog/draft drift;11 engine candidates;1 unresolved contract.** Every one of the45 failing identities appears below and in the JSON. These are review dispositions, not changed test outcomes. Original suite91328fbf0fed4a5d2813eb3661808ee53cb961ba and qtspecs72f222929768b411d8e88d96a1a8740483290078 were checked against refreshed upstream heads and remain current. The fork's origin/master is not used as upstream. No upstream correction is submitted.

| Group | Cases | Disposition |
|---|---:|---|
| Removed xsl:record instruction and duplicates variants |17| Reconcile with current instruction definitions; do not implement an obsolete instruction. |
| Surplus entries, undeclared key, removed grammar, retired field metadata |8| Preserve current-contract rejection; migrate fixtures independently. |
| Obsolete alias codes, map instance expectation, pattern/type wrappers and multiple-error fixture |7| Reconcile catalog; mixed-error cases need isolated replacements, not an error alias. |
| Array error code |1| Current XTTE4060 differs from historical XTDE4060. |
| Record subtyping through function signatures |11| Investigate engine behavior; record-100 has an exploratory not-yet-agreed dependency. |
| jposition function contract |1| Missing from pinned FO catalog; no replacement inferred and no function alias added. |

The pinned [XPath record rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-record-test) require annotations, reject surplus entries during coercion, and raise an error for undeclared keys. Old extensible/optional syntax and several success assertions conflict with that contract. Duplicate fields use XPST0021. The [XSLT named types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-record-types), [pattern rules](https://qt4cg.org/specifications/xslt-40/Overview.html#patterns) and [array error](https://qt4cg.org/specifications/xslt-40/Overview.html#ERRXTTE4060) account for other catalog drift. The draft contains editorial inconsistencies; normative productions/rules, not historical examples or change-log summaries, control this assessment. Mixed-error fixtures are not treated as clean proofs of the observed early error's correctness.

## Reduced diagnostic evidence

Seven fresh source probes ran against the unchanged integrated engine. Atomic subtyping passes both directly and through aliases; identical record signatures pass. A covariant record-field relation returns false both directly and through aliases, where true is expected from structural subtyping. A plain map fails a record instance test, while a coerced annotated record passes. This isolates a record-signature subtyping gap without blaming generic alias resolution or weakening annotation checks. These diagnostic probes do not change unit-test totals or gate membership.

The existing subtype comparer in VmEngine compares function/map/array families and atomic/schema types; record family comparison needs investigation. Do not generalize the reduced covariance result to every record subtype rule: the11 native candidates span record/record, record/map and record/function relations. Record-100's exploratory dependency must remain explicit.

## Recommended bounded next milestone

1. One engine PR for structural record subtyping in function signatures: same-field matching, field-type/cardinality variance, aliases, case-sensitive names and negative controls. Establish a failing baseline first.
2. At most one engine PR for record-to-map/function subtype relations, conditional on independent reduced reproducers and current normative rules. Do not force an expected native gain where the fixture contract is stale.

After each engine slice, run solution tests, language-server checks, CLI controls and both frozen gates; compare unchanged native case identities and details. Stop after at most two engine PRs and reassess. Upstream catalog corrections form a separate reviewed contribution against freshly fetched upstream; they are not mixed into these engine changes. Package preparation can instead be selected before further engine work if release delivery is the priority; this review does not begin either path.

## Complete failing-case inventory

The JSON includes observed diagnostics, per-case rationale, confidence, source paths/hashes and specification references. A candidate label is not a promise that its current native assertion will become green.

| Case | Classification | Reason group | Confidence |
|---|---|---|---|
| `array-017` | catalog/draft drift | array-error-code | high |
| `array-025` | unresolved contract | jposition-contract | unresolved |
| `xsl-record-001` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-002` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-003` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-004` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-005` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-006` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-008` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-009` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-401` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-402` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-403` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-404` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-405` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-407` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-408` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-409` | catalog/draft drift | removed-record-instruction | high |
| `xsl-record-410` | catalog/draft drift | removed-record-instruction | high |
| `record-003` | catalog/draft drift | surplus-record-entries | high |
| `record-004` | catalog/draft drift | removed-record-grammar | high |
| `record-005` | catalog/draft drift | surplus-record-entries | high |
| `record-009` | catalog/draft drift | removed-record-grammar | high |
| `record-016` | catalog/draft drift | undeclared-record-key | high |
| `record-020` | catalog/draft drift | duplicate-field-error | high |
| `record-023` | catalog/draft drift | retired-field-required | high |
| `record-100` | engine candidate | record-subtyping | candidate |
| `record-101` | engine candidate | record-subtyping | candidate |
| `record-104` | engine candidate | record-subtyping | candidate |
| `record-105` | engine candidate | record-subtyping | candidate |
| `record-110` | engine candidate | record-subtyping | candidate |
| `record-111` | engine candidate | record-subtyping | candidate |
| `record-112` | engine candidate | record-subtyping | candidate |
| `record-113` | engine candidate | record-subtyping | candidate |
| `record-120` | engine candidate | record-subtyping | candidate |
| `record-121` | engine candidate | record-subtyping | candidate |
| `record-123` | engine candidate | record-subtyping | candidate |
| `type-alias-003` | catalog/draft drift | nonstandard-alias-errors | high |
| `type-alias-004` | catalog/draft drift | nonstandard-alias-errors | high |
| `type-alias-006` | catalog/draft drift | plain-map-instance-test | high |
| `type-alias-007` | catalog/draft drift | obsolete-type-wrapper | mixed |
| `type-alias-010` | catalog/draft drift | obsolete-pattern-syntax | high |
| `type-alias-012` | catalog/draft drift | removed-record-grammar | high |
| `type-alias-013` | catalog/draft drift | obsolete-pattern-syntax | high |
| `type-alias-014` | catalog/draft drift | multiple-static-errors | mixed |
