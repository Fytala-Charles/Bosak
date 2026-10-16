# Conformance runner processor profiles

Unpublished tooling feature version **1.16.0** exposes the processor profile added in1.15.0 through the native conformance runner and PowerShell wrapper. Engine implementation files and upstream fixtures are unchanged. This minor version records the new CLI capability; no public package or extension is published.

## Invocation and receipts

Direct CLI: `--processor-profile legacy|xslt40-draft`, accepted once and independently of `--xslt40`. Default is Legacy. The runner prints the actual processor profile and passes it to every test's XsltCompiler. Invalid values, missing values and duplicates exit2 before catalog loading. Draft-profile failures and zero execution exit1 even when the catalog flag is absent; legacy historical exit behavior outside --xslt40 is unchanged.

Wrapper: `-ProcessorProfile Legacy|Xslt40Draft`, validated by ValidateSet, default Legacy. It always passes the canonical CLI token. Existing --xslt40, source versions, exact errors and skips remain unchanged. Receipts preserve the previous profile field and add catalogProfile, processorProfile and runnerArguments. Counts/success remain independent of the selected profile; failures and all-skipped runs do not become passes.

## Permanent controls and baseline

Fixtures/ProcessorProfile contains five local catalog cases, not upstream contributions: a shared positive, version3 XPath4 syntax, version17 unknown copy attribute, version4 unknown sequence attribute and a catalog4 dependency. The permanent verify-runner-profile.ps1 script exercises default/explicit legacy, draft, independent catalog selection, empty execution, invalid/missing/duplicate options and both profiles against existing adapter fixtures. CI runs these **ten controls** after building and preserves their logs on failure.

The initial smoke attempt failed because its summary regex lost whitespace escapes. The corrected pre-implementation script passed the default legacy control (2pass/3fail local matrix), then failed explicit legacy: the old runner treated --processor-profile as a catalog path. First implementation passed four controls, then exposed a fixture expectation error: filtered-out cases are counted as skips. The empty selection expectation was corrected from0/0/0 to0/0/5; required exit1 and zero executed tests were retained. All original scripts/logs and corrections are preserved separately. Final ten controls pass, including draft local matrix5/0/0 and draft without catalog4 flag4/0/1.

Wrapper checks also exercised explicit Legacy and Xslt40Draft on the array-021 substring selection (three matching cases across selected sets): Legacy2/1/177, draft3/0/177. Receipt processor fields and argument mapping are retained. Invalid wrapper profile exits1 without running tests. Full-family Legacy invocations omitted the new wrapper option to verify backward-compatible defaults.

## Original upstream comparison

| Family | Legacy pass/fail/skip | Draft pass/fail/skip | Pass changes |
|---|---|---|---|
| array aggregate |155/5/20 |158/2/20 |Gains array-021, array-022, array-024; no lost passes |
| record aggregate |29/35/1 |29/35/1 |No pass changes; nine failure diagnostics change |
| type-alias |23/8/1 |16/15/1 |Loses type-alias-016 through type-alias-022 |

All six full-family invocations exit1 because failures remain. Legacy passing identities match PR344's original receipts. All gained/lost cases and every changed failure detail are recorded in companion JSON; raw logs, receipts and normalized full-case maps remain under .artifacts. Initial CRLF parsing produced carriage-return-suffixed identities in an exploratory map; original maps are retained, and final comparison uses splitlines before matching exact identities. No expectations, stylesheet versions, aliases, skips or gate membership changed.

Array-021 now reaches the expected XPTY0004 instead of XPath31 function unavailability. Array-022 now executes natively and passes. Array-024 now accepts the upstream fixture's existing type-pattern syntax; this does not establish every current draft type-pattern spelling. Array-025 still fails, but the error advances from unavailable jkey to missing jposition#1. Array-017 retains its XTDE4060 versus XTTE4060 code mismatch; no alias was added.

Record failure details change for record-023 and xsl-record-001 through005,006,008,009. Record-023 now reports XTSE0090 on field/@required; several others reject unsupported xsl:record instead of returning mismatching/empty output. These remain failures and require individual draft review.

Every lost type-alias pass reports XTSE0090 for fixed-namespaces on xsl:stylesheet or xsl:package (type-alias-019 expects XPST0051 but is preempted). Existing module namespace processing recognizes fixed-namespaces; the stricter shared whitelist does not include it. The [draft fixed-namespaces section](https://qt4cg.org/specifications/xslt-40/Overview.html#the-fixed-namespaces-attribute) provides the next review target. This slice exposes that defect without editing engine behavior or suppressing assertions.

## Verification and revisions

All verification processes completed: Release build **0errors/81 existing warnings**; solution **9,384/0/0** (XSLT6,450), separate language-server **84/0/0**, ten CLI controls pass, frozen QT3 **31,142/0/679**, qt4 gated **8,278/0/1,894**, both gates exit0. Fixture links were verified and removed. Canonical documentation synchronized and ten pages rendered with markdown-it and canonical Fytala CSS. Ubuntu/pwsh CI validation of the new smoke step remains pending on this PR.

PR344 exact-head CI run38939354663 passed atcc07d50f8a55bdca907d398d4916d95cbed36734; merged atbcbffa69f3040c9d72657f7221554e34ff9d5cf3, this branch's base. Refreshed xslt40-test upstream/master remains91328fbf0fed4a5d2813eb3661808ee53cb961ba; qtspecs master remains72f222929768b411d8e88d96a1a8740483290078; pinned xslt.xml blob59b9bdc79dca23b0c18fdd3e94130bc59037db3e. Native runs used command-scoped exact-path Git ownership trust, with no global configuration change. No upstream contribution was prepared.

## Next slice

Fix module-level fixed-namespaces admission under the selected processor profile with independent positive/negative controls, then rerun both profile lanes and retain the original seven regression receipts. Review field/@required and remaining direct whitelists separately. Unknown-future/no-fallback handling, schema-role, authoring/validation and nested fn:transform profile options remain separate. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance remain unchanged.
