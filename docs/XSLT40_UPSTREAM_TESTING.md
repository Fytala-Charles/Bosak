<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak upstream XSLT 4.0 test contribution workflow">
  <br><br><h1>Contributing to the XSLT 4.0 Test Suite</h1>
  <p>Upstream-native tests, Bosak adapter and reproducible evidence</p>
</div>

## 2026-10-16 - Record-to-map and generic function subtyping

Record-subtyping milestone 2/2: unpublished patch1.16.5 compares structural record field types with map values and accepts generic function supertypes, including parenthesized host aliases. Five further native cases recover in both profiles. Stop engine work after integration; reassess and validate exact-revision packages with fresh consumers. [Contract and evidence](XSLT40-RECORD-MAP-SUBTYPING.md). Record-subtyping milestone 2/2; stop engine work after at most two slices, then validate release packages. Public libraries1.8.0, extension0.2.4/engine1.8.0, REQ statuses and Braid acceptance unchanged.



## 2026-10-16 - Structural record subtyping in function signatures

Record-subtyping milestone 1/2: unpublished patch1.16.4 compares structural field sets and covariant field types, preserving case-sensitive names through nested signatures. Typed zero-argument function tests now check arity and return types. Native record100/101/104/105 recover in both profiles; all other outcomes/details are unchanged. [Contract and evidence](XSLT40-STRUCTURAL-RECORD-SUBTYPING.md). Record-subtyping milestone 1/2; stop engine work after at most two slices, then validate release packages. Public libraries1.8.0, extension0.2.4/engine1.8.0, REQ statuses and Braid acceptance unchanged.



## 2026-10-16 - Release readiness and draft failure review

Review of integrated source1.16.3:33 catalog/draft mismatches,11 record-subtyping engine candidates and1 unresolved function contract account for all45 selected failures. Seven reduced probes confirm a record-signature subtype gap. Engineering checks support candidate preparation; package/consumer/provenance and release checks remain pending. Recommend at most two focused engine PRs, then reassess; recommendation only, no new implementation or publication. [Review and case inventory](XSLT40-RELEASE-READINESS-REVIEW.md). Public versions, REQ statuses and Braid acceptance unchanged.



## 2026-10-16 - Future instruction fallback validation and execution

Milestone 3/3: unpublished patch 1.16.3 requires fallback statically for unknown future instructions in the draft processor, and evaluates valid function-body fallbacks with raw item preservation and isolated variable scope. Legacy behavior is preserved. The approved three-PR milestone ends here. [Contract and evidence](XSLT40-FUTURE-FALLBACK.md). Milestone 3/3; stop and reassess after slice3. Public libraries1.8.0, extension0.2.4/engine1.8.0, REQ statuses and Braid acceptance unchanged.



## 2026-10-16 - Earlier-version fixed namespace contexts

Milestone2/3: unpublished patch1.16.2 activates fixed bindings from the captured draft processor capability for module versions1.0-4.0. Draft modules own their mutable compilation trees and caches; reader base URI/line information and provider provenance are retained. Legacy behavior remains unchanged. [Contract and evidence](XSLT40-EARLIER-FIXED-NAMESPACES.md). Milestone 2/3; stop and reassess after slice3. Public libraries1.8.0, extension0.2.4/engine1.8.0, REQ statuses and Braid acceptance unchanged.



## 2026-10-16 - Current record field contract audit

Milestone1/3: pinned draft removed xsl:field/@required in March2026. Preserve the correct draft XTSE0090 rejection; add13 current-contract controls. Engine/source version1.16.1 unchanged. Original record-023 remains an unchanged upstream failure. [Contract and evidence](XSLT40-FIELD-REQUIRED-AUDIT.md). Milestone 1/3; stop and reassess after slice3. Public libraries1.8.0, extension0.2.4/engine1.8.0, REQ statuses and Braid acceptance unchanged.



## 2026-10-16 - Fixed namespace module validation

Unpublished source **1.16.1** admits fixed-namespaces on version4.0-or-later stylesheet/transform/package roots, matching the existing binding handler. All seven lost draft type-alias passes (016..022) are restored: draft23/8/1, with no other native status or diagnostic changes in either profile. The handler activation threshold and shared instruction whitelist are unchanged. [Contract and evidence](XSLT40-FIXED-NAMESPACE-VALIDATION.md). Earlier module-version activation remains a separate profile gap. Published versions, REQ statuses, gates and Braid acceptance are unchanged.



## 2026-10-16 - Conformance runner processor profiles

Unpublished source **1.16.0** adds explicit `--processor-profile legacy|xslt40-draft` and wrapper `-ProcessorProfile Legacy|Xslt40Draft`, with separate receipt fields for catalog and processor profiles. Defaults remain legacy. Ten permanent CLI controls run in CI. Unchanged upstream fixtures reveal draft array158/2/20, record29/35/1 and type-alias16/15/1; legacy counts and passing identities are retained. [Comparison and evidence](XSLT40-RUNNER-PROFILE.md). Next: fix module fixed-namespaces validation causing seven lost draft type-alias passes. No engine implementation or upstream fixture edits; published versions, REQ statuses and Braid acceptance are unchanged.

### Explicit native-runner profile

```powershell
# Existing invocation: legacy compiler, XSLT40 catalog dependencies.
./run-xslt40-tests.ps1 -SuiteRoot .upstream/xslt40-test -TestSet array -NoBuild
# Opt-in draft compiler; retains the upstream stylesheet versions and exact errors.
./run-xslt40-tests.ps1 -SuiteRoot .upstream/xslt40-test -TestSet array -ProcessorProfile Xslt40Draft -NoBuild
# Direct runner: compiler and catalog profile are independent choices.
dotnet run --project tests/Bosak.Xslt.Conformance -c Release -- --xslt40 --processor-profile xslt40-draft .upstream/xslt40-test/catalog.xml array
# Permanent local controls (after the Release build).
powershell -NoProfile -ExecutionPolicy Bypass -File tests/Bosak.Xslt.Conformance/verify-runner-profile.ps1
```

`--xslt40` selects catalog dependencies and exact-error accounting. `--processor-profile` selects `XsltCompiler.ProcessorProfile`; it does not change catalog dependencies. Invalid, missing or repeated direct profile arguments exit2 before catalog loading. Draft runs exit1 for any failure or no executed tests, even without `--xslt40`; historical legacy exit behavior outside `--xslt40` is unchanged. Wrapper ValidateSet rejects unknown profile names. Receipts retain the historical profile field and add catalogProfile, processorProfile and runnerArguments. All-skipped selections remain unsuccessful.



## 2026-10-16 - Explicit XSLT processor profile

Unpublished source **1.15.0** adds `XsltCompiler.ProcessorProfile`: `Legacy` preserves existing behavior; opt-in `Xslt40Draft` selects a4.0 forward-compatibility ceiling and frozen XPath4.0 independently of stylesheet version. Copy/sequence unknown attributes follow the draft ceiling in the opt-in lane; fallback versions and imported/included/used-package modules share the captured profile. [Contract and evidence](XSLT40-PROCESSOR-PROFILE.md). Published versions, default runners, REQ statuses and independent Braid acceptance are unchanged. Earlier audit results remain historical baseline evidence.



## 2026-10-16 - Copy forward-compatibility audit

The [forward-compatibility audit](XSLT40-COPY-FORWARD-COMPATIBILITY.md) reproduces five draft mismatches in a twelve-case compilation matrix. Copy rejects unknown attributes at effective versions 5/17; apply-templates accepts them at 4; sequence accepts them at 3/4. The shared supported-version ceiling deliberately remains 3.0 for frozen-catalog compatibility. A coordinated processor-capability design and separate sequence attribute validation are the next implementation work. This evidence-only step changes no engine behavior, version, REQ status or gate membership; source remains unpublished1.14.17.



**Copy standard attributes - 2026-10-16:** Unpublished source candidate **1.14.17** validates `xsl:copy` against the shared supported-standard attribute set plus copy-specific options. Local expansion, XPath namespace, collation, mode and version attributes are accepted; module-only input-type-annotations and unknown attributes remain rejected. Native array155/5/20, record29/35/1 and type-alias23/8/1 retain identical passing cases. [Contract and evidence](XSLT40-COPY-STANDARD-ATTRIBUTES.md). Published versions, profile selection, REQ statuses and independent Braid acceptance remain unchanged. Earlier dated entries below are historical.

**Copy namespace validation - 2026-10-16:** Unpublished source candidate **1.14.16** classifies namespace declarations and qualified metadata before `xsl:copy` option validation. Legal default namespaces, undeclarations, option-like namespace prefixes and foreign attributes no longer trigger XTSE0090/0020; real unqualified invalid attributes/options still fail. Native array155/5/20, record29/35/1 and type-alias23/8/1 retain identical passing cases. [Contract and evidence](XSLT40-COPY-NAMESPACE-VALIDATION.md). Published versions, profile selection, REQ statuses and independent Braid acceptance remain unchanged. Earlier dated entries below are historical.

**Non-node copy values - 2026-10-16:** Unpublished source candidate **1.14.15** preserves non-node `xsl:copy` values in raw sequence collectors and uses existing complex-content delivery otherwise. Atomics retain their kinds, maps/arrays retain payloads and member boundaries, functions remain callable, and illegal XML function content raises the existing errors. Native array155/5/20, record29/35/1 and type-alias23/8/1 retain identical passing cases. [Contract and evidence](XSLT40-COPY-NON-NODE-VALUES.md). Published versions, profile selection, REQ statuses and independent Braid acceptance remain unchanged. Earlier dated entries below are historical.

**Copy-select state restoration - 2026-10-16:** Unpublished source candidate **1.14.14** restores copy-select caller focus (item/position/size), current template rule and next-match exclusions in `finally` in both instruction dispatchers. Selected copy content retains singleton focus; caught errors no longer lose the caller rule. No-select, empty-selection and ignored-content controls stay green. Native array155/5/20, record29/35/1 and type-alias23/8/1 retain identical passing cases. [Contract and evidence](XSLT40-COPY-SELECT-STATE.md). Published versions, profile selection, REQ statuses and independent Braid acceptance remain unchanged. Earlier dated entries below are historical.

**Function-body copy capture - 2026-10-16:** Unpublished source candidate **1.14.13** routes function-body element copies through the existing copy constructor and shares copied-document construction with normal execution. Literal/conditional text, attribute sets, namespace options, document representation and capture isolation now use the same paths. The helper restores caller atomic adjacency in `finally`. Native array155/5/20, record29/35/1 and type-alias23/8/1 retain identical passing cases. [Contract and evidence](XSLT40-FUNCTION-COPY-CAPTURE.md). Published versions, profile selection, REQ statuses and independent Braid acceptance remain unchanged. Earlier dated entries below are historical.

**Copied-document capture - 2026-10-16:** Unpublished source candidate **1.14.12** constructs `xsl:copy` document content through the shared document pipeline, isolating raw/typed capture and supporting text-only, empty and multiple-root documents. Source document metadata and validation retain explicit handling; copied documents still reject attributes/namespaces with XTDE0420. Native array155/5/20, record29/35/1 and type-alias23/8/1 retain identical passing cases. [Contract and evidence](XSLT40-COPY-DOCUMENT-CAPTURE.md). Published versions, profile selection, REQ statuses and independent Braid acceptance remain unchanged. Earlier dated entries below are historical.

**Copied-element capture - 2026-10-16:** Unpublished source candidate **1.14.11** isolates element content constructed by `xsl:copy` from raw/typed template capture, protects attribute-set/content/validation cleanup with `finally`, and ends atomic adjacency at the copied-element boundary. Atomic spaces, nested-document flattening, illegal map/function errors and caught-error output are covered. Native array155/5/20, record29/35/1 and type-alias23/8/1 retain identical passing cases. [Contract and evidence](XSLT40-COPY-ELEMENT-CAPTURE.md). Published versions, profile selection, REQ statuses and independent Braid acceptance remain unchanged. Earlier dated entries below are historical.

**Literal-result-element capture - 2026-10-16:** Unpublished source candidate **1.14.10** isolates literal-result-element children from raw/typed document preservation and function-item capture, restores caller state, and ends atomic adjacency at the element boundary. Nested documents flatten and illegal maps/functions raise XTDE0450. Native array155/5/20, record29/35/1 and type-alias23/8/1 remain unchanged, with identical pass identities. [Contract and evidence](XSLT40-LITERAL-ELEMENT-CAPTURE.md). Published versions, profile selection, REQ statuses and independent Braid acceptance remain unchanged. Earlier dated entries below are historical.

**Computed-element capture - 2026-10-16:** Unpublished source candidate 1.14.9 isolates computed-element content from raw/typed result capture, restores state even after attribute-set errors, and ends atomic adjacency at the element boundary. Spaces, nested-document flattening and illegal map/function errors now follow complex-content rules. Native array155/5/20, record29/35/1 and type-alias23/8/1 remain unchanged, with no lost passes. [Contract and evidence](XSLT40-COMPUTED-ELEMENT-CAPTURE.md). Published versions, profile selection, REQ statuses and independent Braid acceptance remain unchanged. Earlier dated entries below are historical.

**Document construction capture - 2026-10-16:** Unpublished source candidate 1.14.8 isolates document construction from outer raw/typed template capture. Atomic values regain spaces, nested documents flatten, illegal maps/functions raise XTDE0450, and caller capture state is restored. The fix is shared construction state, not xsl:select syntax. Native array155/5/20, record29/35/1 and type-alias23/8/1 remain unchanged, with no lost passes. [Contract and evidence](XSLT40-DOCUMENT-CONSTRUCTION-CAPTURE.md). Published versions, profile selection, REQ statuses and independent Braid acceptance remain unchanged. Earlier dated entries below are historical.

**JNode text atomization - 2026-10-16:** Unpublished source candidate 1.14.7 atomizes complete JNode payloads for AVTs, expanded text, value-of and attribute construction, including nested/empty values and typed nodes; wrapped maps/functions raise atomization errors. The isolated root4 array-022 diagnostic now yields RU TJ. Original native array155/5/20, record29/35/1 and type-alias23/8/1 remain unchanged; diagnostic copies are not native passes. [Contract and evidence](XSLT40-JNODE-TEXT-ATOMIZATION.md). Processor-profile selection, published versions, REQ statuses and independent Braid acceptance remain unchanged. Earlier dated entries below are historical.

**Remaining array profile audit - 2026-10-16:** Documentation-only review; source1.14.6 unchanged. The current draft requires equivalent effective-version3.0/4.0 behavior for a4.0 processor, so Bosak's version-selected XPath profile is a processor-capability gap, not proof of invalid upstream3.0 metadata. Isolated root-version4 probes expose JNode text-construction failure in array-022 and absent current-catalog jposition in array-025; array-021/024 reach their expected outcomes. Native array155/5/20 remains unchanged; probes are not native passes. [Audit and evidence](XSLT40-ARRAY-PROFILE-AUDIT.md). Published versions, REQ statuses and independent Braid acceptance remain unchanged. Earlier dated entries below are historical.

**Array apply-imports fallback - 2026-10-16:** Unpublished source candidate1.14.6 applies member templates when apply-imports finds no imported array rule in text-only-copy or shallow-skip. New member applications temporarily suspend the enclosing array import restriction and restore it on normal/error exits. Imported-rule selection and priorities are unchanged. Native array155/5/20, record29/35/1 and type-alias23/8/1 are unchanged, with no lost passes. [Contract and evidence](XSLT40-ARRAY-IMPORT-FALLBACK.md). Published versions, REQ statuses and independent Braid acceptance remain unchanged. Earlier dated entries below are historical.

**Array template fallback - 2026-10-16:** Unpublished source candidate1.14.5 applies member templates when next-match exhausts array rules in text-only-copy or shallow-skip modes. Ordinary unmatched arrays share the same member application, preserving flattened focus, node dispatch, parameters and fresh chains. Native array-023 newly passes: array155/5/20; record29/35/1 and type-alias23/8/1 unchanged. Template priorities were not changed. [Contract and evidence](XSLT40-ARRAY-TEMPLATE-FALLBACK.md). Published versions, REQ statuses and independent Braid acceptance remain unchanged. Earlier dated entries below are historical.

**Implicit variable-binding documents - 2026-10-16:** Unpublished source candidate 1.14.4 retains raw sequences for untyped effective-XSLT-4.0 variable/parameter bindings with a direct map, map-entry, array, array-member or select child. Ordinary temporary trees, older effective versions and explicit types/documents retain their rules. Native array-018 newly passes: array154/6/20; record29/35/1 and type-alias23/8/1 unchanged. Array-017 still reports the draft-correct XTTE4060 against an upstream XTDE4060 expectation. [Contract and evidence](XSLT40-IMPLICIT-BINDING-DOCUMENTS.md). Published versions, REQ statuses and independent Braid acceptance are unchanged. Earlier dated entries below are historical.

**Untyped raw initial-template results - 2026-10-16:** Unpublished source candidate 1.14.3 makes the existing `rawResult: true` entry point retain untyped initial-template results, including arrays, maps, atomics and nodes. Nested typed calls contribute to their caller. Native array-010 through015 and type-alias-050 newly pass: array153/7/20; type-alias23/8/1. [Contract and evidence](XSLT40-RAW-INITIAL-RESULTS.md). Public versions, REQ statuses, frozen gates and independent Braid acceptance are unchanged. Earlier dated entries below are historical.

**Static for-each validation - 2026-10-16:** Unpublished source candidate 1.14.2 validates `xsl:for-each` attributes and required `select` during stylesheet compilation, including unreachable bodies. Existing separators, shadow attributes and local forward-compatibility policy are preserved. Native arrays-021 newly passes: arrays75/0/12, array aggregate147/13/20. [Review and evidence](XSLT40-FOR-EACH-VALIDATION.md). Public versions, REQ statuses, frozen gates and independent Braid acceptance are unchanged. Earlier dated entries below are historical.

**Complete sum atomization - 2026-10-16:** Unpublished source candidate 1.14.1 fixes both `fn:sum` arities to atomize complete input before empty handling. A JNode wrapping `(5,6)` now contributes both values; empty wrapped input selects the correct zero value. Native array-020/arrays-020 newly pass: array aggregate146/14/20, previously144/16/20. [Contract and evidence](XSLT40-JNODE-SUM.md). Other aggregates remain separate. Public versions, REQ statuses, frozen gates and independent Braid acceptance are unchanged. Earlier dated entries below are historical.

**Array member functions - 2026-10-16:** Unpublished source candidate 1.14.0 adds 4.0-only `array:members` and `array:of-members` using the current JNode signatures. Member boundaries, empty values, parent/key metadata and reconstruction order are retained without flattening or coercion. Unchanged native arrays-006 newly passes: array aggregate144/16/20, previously143/17/20. [Contract and evidence](XSLT40-ARRAY-MEMBER-FUNCTIONS.md). Public versions, REQ statuses, frozen gates and independent Braid acceptance are unchanged. Earlier dated entries below are historical.

**Array patterns - 2026-10-16:** Unpublished source candidate 1.13.0 adds `array(SequencePattern)`, recursive member matching and priorities, plus outer predicates. Shared sequence checks reject repeated occurrence indicators without changing valid wildcard patterns. Unchanged native arrays-024 newly passes: array aggregate 143/17/20, previously 142/18/20. [Contract and evidence](XSLT40-ARRAY-PATTERNS.md). Public versions, REQ statuses, frozen gates and independent Braid acceptance are unchanged. Earlier dated entries below are historical.

**Named record patterns - 2026-10-16:** Unpublished source candidate 1.12.0 adds `record(TypeName)` for nominative record types and aliases resolving to record types, standalone and within map entry sequences. Bare priority is 0; predicates use 0.5. Existing instance checks preserve nominal identity and structural record matching without map coercion. XSLT 4.0 context item declarations can bind previously unsupported named item types. [Contract and evidence](XSLT40-RECORD-PATTERNS.md). Public versions, REQ statuses, frozen gates and independent Braid acceptance are unchanged. Earlier dated entries below are historical.

**Atomic patterns - 2026-10-16:** Unpublished source candidate 1.11.0 adds `atomic(TypeName)` for built-in atomic types, atomic-only aliases, alternatives and predicates, standalone and in map entry sequences. Default priority follows derivation depth and the minimum alternative; matching performs no coercion or node atomization. Schema-defined types and `record(TypeName)` remain separate. [Contract and evidence](XSLT40-ATOMIC-PATTERNS.md). Public libraries 1.8.0, extension 0.2.4/engine1.8.0, REQ statuses, frozen gates and independent Braid acceptance are unchanged. Earlier dated entries below are historical.

**Map entry sequence patterns - 2026-10-16:** Unpublished source candidate 1.10.0 adds string-key map entry constraints using existing node/predicate matchers, empty-sequence/cardinality checks, nested maps and outer predicates. Constrained maps have priority 0.25; filtered maps 0.5. Native record-011/012 newly pass: record filter 29/35/1 (record itself 29/18/0), type-alias unchanged 22/9/1. [Contract and evidence](XSLT40-MAP-ENTRY-PATTERNS.md). Separate atomic(TypeName)/record(TypeName) syntax and other pattern families remain follow-ups. Public versions, REQ statuses, gate membership and independent Braid acceptance are unchanged.

**String-key map patterns - 2026-10-16:** Unpublished source candidate 1.9.0 supports standalone XSLT 4.0 string-key map patterns: empty/any maps, exact key sets and a trailing wildcard, with correct default priorities. Unchanged native record-010 newly passes; record filter 27/37/1 (record itself 27/20/0), type-alias unchanged 22/9/1. Entry-value patterns, non-string constants and suffixes remain outside this bounded slice. [Contract and evidence](XSLT40-STRING-KEY-MAP-PATTERNS.md). Public libraries remain 1.8.0 and VS Code 0.2.4 bundles 1.8.0; REQ statuses, gates and independent Braid acceptance are unchanged.

**Record draft review and reliability - 2026-10-16:** Pinned current draft `72f22292` requires the errors observed in record-003/005/016; these are upstream/draft alignment discrepancies, not confirmed engine defects. A deterministic descriptor-traversal cancellation test replaces the one-millisecond timer race and adds a normal-completion control, using existing internal test construction only. [Review and evidence](XSLT40-RECORD-DRAFT-RELIABILITY.md). Native expectations, engine behavior, public versions, REQ statuses and gates remain unchanged; Braid acceptance stays independent.

**N04/N05 baseline - 2026-10-16:** Fresh unchanged upstream `91328fbf` yields type-alias 22/9/1 after correcting whitespace-separated specification dependency selection (initial 22/10/0). The record filter remains 26/38/1: record itself 26/21/0, xsl-record 0/17/1. Eight adapter controls pass; no engine change or conformance pass gain. Next engine review: extra-field matching/conversion in record-003/005/016, after current-draft review. [Baseline, triage and evidence](XSLT40-N04-N05-BASELINE.md). Published versions, REQ statuses, gates and independent Braid acceptance are unchanged.

## Restart plan - 16 October 2026

The next bounded baseline is the unchanged native `type-alias` and `record` families (N04/N05), followed by one evidence-selected implementation/reliability cluster. See the [planned slice and acceptance checkpoints](XSLT40_CONTRIBUTION_QUEUE.md#next-planned-slice-n04n05-conformance-and-reliability---16-october-2026) and [machine-readable queue](XSLT40_CONTRIBUTION_QUEUE.json). No fresh baseline has run yet. The initial arrangement and counts below remain historical: refresh upstream's default branch and record the exact new base before preparing any new contribution; fetch/rebase/recheck again immediately before submission. Preserve the existing checkout, branches and receipts. This checkpoint authorizes documentation/planning only, not upstream submission.



## Local arrangement

The upstream suite is an independent Git checkout at `.upstream/xslt40-test`, ignored by Bosak. Its baseline is `6205faf9f32ace6338ac245c41f8eb9a1447926f` from [qt4cg/xslt40-test](https://github.com/qt4cg/xslt40-test). The remote is named `upstream`; local contribution branch: `bosak/schema-awareness-review`. No upstream test modifications, fork, upstream PR or external message have been made in this setup step.

Bosak owns the adapter, launch/validation scripts, local adapter controls and review notes. The upstream checkout owns its unchanged `catalog.xml`, `admin/catalog-schema.xsd`, `tests/` structure and future contribution files. It is not a Bosak submodule, vendored copy or parallel Bosak-format test collection. [Adapter configuration](../tests/xslt40-adapter.json) records the reviewed source pin and paths.

| Location | Responsibility |
|---|---|
| .upstream/xslt40-test/catalog.xml | Upstream list of test sets. |
| .upstream/xslt40-test/tests/.../_name-test-set.xml | Upstream environments, dependencies, scenarios and assertions. |
| .upstream/xslt40-test/tests/.../*.xsl / *.xml | Upstream stylesheets, inputs and expected outputs. |
| tests/Bosak.Xslt.Conformance | Bosak catalog adapter; existing 3.0 behavior retained by default. |
| tests/Bosak.Xslt.Conformance/Fixtures/Xslt40Adapter | Local adapter controls, not contribution tests. |
| .artifacts/xslt40 | Ignored logs and JSON run receipts. |

Recreate a checkout on another machine:

```powershell
git clone https://github.com/qt4cg/xslt40-test.git .upstream/xslt40-test
git -C .upstream/xslt40-test remote rename origin upstream
git -C .upstream/xslt40-test switch -c bosak/schema-awareness-review 6205faf9f32ace6338ac245c41f8eb9a1447926f
```

Fetch/review upstream changes explicitly before advancing the baseline. Record the new pin and rerun relevant tests. Do not automatically update a contribution branch or rewrite upstream expected results to match Bosak.

## Running with Bosak

From the Bosak root:

```powershell
pwsh -File ./run-xslt40-tests.ps1 -TestSet select
pwsh -File ./run-xslt40-tests.ps1 -TestSet xml-to-json -TestCase xml-to-json-D202-40 -NoBuild
pwsh -File ./run-xslt40-tests.ps1 -TestSet si-fork -SchemaAware -NoBuild
```

Test-set and case filters are the existing adapter's substring filters. Thus xml-to-json also selects sf-xml-to-json. Unselected cases can appear as skipped in summaries; inspect the log before treating skip counts as unsupported coverage. The wrapper requires a nonempty test-set filter to bound runs.

The wrapper selects `--xslt40`, which recognizes XSLT40/XSLT40+ catalog dependencies, uses exact declared error-code matching without Bosak's legacy aliases, and returns failure for failed tests or zero executed tests. It retains actual stylesheet versions; the flag does not rewrite 3.0 stylesheets into 4.0. Engine compilation derives its XPath profile from the stylesheet's effective version. Explicit XSLT30-only catalog dependencies are skipped in the 4.0 profile; XSLT30+ remains eligible.

This is a partial implementation profile, not an assertion that Bosak supports every XSLT 4.0 feature or every catalog assertion. Existing environment/assertion machinery and unsupported-feature policies remain inherited from the adapter. Review the specific environment, dependencies, assertion implementation and error expectations before claiming upstream evidence. Independent-processor confirmation is desirable where available.

Each wrapper run writes a timestamped log and JSON receipt with suite revision/status, Bosak revision/status, selected filters, schema-aware mode, exit code and counts. A dirty tree is recorded, not represented as a clean revision. Preserve the exact patch or contribution commit alongside dirty-run receipts; file names alone do not reconstruct edits. Receipts are local working artifacts and contain absolute paths.

## Catalog validation and adapter controls

Validate touched catalogs against the upstream schema, as its [contribution instructions](https://github.com/qt4cg/xslt40-test/blob/6205faf9f32ace6338ac245c41f8eb9a1447926f/readme.md) request:

```powershell
pwsh -File ./validate-xslt40-catalog.ps1
pwsh -File ./validate-xslt40-catalog.ps1 -Files tests/attr/select/_select-test-set.xml
pwsh -File ./test-xslt40-adapter.ps1 -NoBuild
```

Files are relative to the upstream suite root, or absolute. In an interactive PowerShell invocation, Files also accepts an array. The validator validates XML catalog structure; it does not certify stylesheet semantics or assertion correctness.

Four controls verify a version-4.0 keyword stylesheet and exact unknown-function error, the unchanged legacy profile's 4.0 dependency skips, failure for an empty selection, and failure for a deliberately mismatched error code that the legacy alias table could accept. The negative catalog is generated under ignored artifacts; it is not submitted upstream.

## Initial baseline — 2026-10-12

| Probe | Result | Meaning |
|---|---|---|
| Upstream select set | 157 passed / 0 failed / 1 skipped; wrapper exit 0 | Existing suite can run through the Bosak adapter. Does not prove new 4.0 feature coverage. |
| Upstream xml-to-json-D202-40 | 0 passed / 1 failed / 161 skipped; wrapper exit 1 | XSLT40+ case actually executes. Bosak returns -0 where the assertion expects -0e0. Other skip counts include filtered cases. |
| Nonexistent set selection | Zero executed; wrapper exit 1 | An empty run is not reported as successful coverage. |
| Local adapter controls | Four controls passed | Version gating, keyword profile, exact errors and negative exit statuses verified. |
| Upstream catalog and select test-set schema validation | Passed | Native catalog structure verified. |

The JSON case is existing upstream evidence, not a new contribution or permission to change its expected result. Triage whether a difference is in Bosak, its adapter, the test or draft alignment, using the exact source revision. No full-suite sweep or conformance percentage is claimed.

## Initial schema-awareness track (now secondary)

Review schema-awareness dependencies/environments in the current 4.0 suite, starting with the source-document and si-fork/si-map scenarios behind [issue #90](https://github.com/w3c/xslt30-test/issues/90). Michael confirmed that an environment schema element implies schema-awareness and [recommended focusing contributions on 4.0](https://github.com/w3c/xslt30-test/issues/90#issuecomment-5993508731). Check what has already changed before proposing metadata or new tests.

For a contribution:

1. Find the closest native test set and avoid duplicate coverage.
2. Record the exact draft clause/pin and required outcome; Bosak's current failures are evidence, not normative expectations.
3. Add stylesheets, input/output files and test cases inside the upstream tests directory, using its naming, created-by metadata, environments, dependencies and assertions. Add catalog entries only for a genuinely new group.
4. Validate touched test-set XML and run the selected cases through Bosak in basic/schema-aware modes as applicable. Keep refusals, unsupported features and failures visible.
5. Prepare a small upstream commit and contribution description. The upstream guidance requires consensus before changing established expected results believed incorrect; metadata corrections have a lighter process.

Keep the upstream PR focused on portable tests. A short implementation note may link [Bosak's README](../README.md) and the relevant [contract review](xpath40-catalog/CHANGED-31-CONTRACT-INVENTORY.md), accurately distinguishing supported behavior and recorded gaps. A fork/push/PR or maintainer message is a later delivery step; this setup sends none.

The REQ-126 equality/collation catalog queue remains recorded, but upstream contribution setup/review is now the user's priority. REQ-126 stays In Progress; version remains 1.1.3.


## Setup validation

Release solution build passed (12 existing warnings, zero errors); **6,806 tests / 0 failures / 0 skips** across ten assemblies. Four adapter controls passed; upstream select 157/0/1, genuine XSLT40+ JSON case 0/1/161 with exit 1, zero selection exit 1, upstream catalog/select and local control schema validation passed. Setup guide and six canonical/implementation documents rendered; local links/code fences and JSON run receipt counts/status verified. Manual engine gates were not rerun for adapter-only changes; engine/API unchanged.


## Current contribution priority

The [contribution priority queue](XSLT40_CONTRIBUTION_QUEUE.md) now starts with new XSLT 4.0 elements: array/member, select, switch, named item types and named records. Likely use is estimated, not measured; structural stylesheet counts are not semantic coverage. Next: N01 native array-case audit and a small nonduplicate order-export/member batch. Schema-awareness and inherited JSON findings remain secondary triage. Version 1.1.3; REQ-126 In Progress; no engine change or upstream submission. The current clean upstream branch is bosak/array-member-review; the earlier schema-awareness-review branch remains available. Earlier setup evidence stays historical.


**First upstream test batch — 2026-10-12:** N01 now has a [six-case native array/member batch](../contributions/xslt40/array-member-boundaries/README.md), committed locally on bosak/array-member-review at e72f1b0e8511f78478e66c112201317cfe7b7ad1. Five exact new cases pass; array-032 exposes missing XTTE4060 enforcement and returns A|B. Normative expectations are retained. A portable mail patch, source/expected-outcome mapping, clean-commit receipts and an upstream PR draft preserve the work; no upstream submission or independent processor confirmation yet. Version 1.1.3, REQ-126 In Progress, no Bosak engine/API/adapter change. Next: independent assertion review and separate error triage before submission.


**Array/member correction — 2026-10-12:** The [array/member assertion review](../contributions/xslt40/array-member-boundaries/ASSERTION_REVIEW.md) confirms the six native expectations. Bosak now preserves existing core JNode wrappers in both XSLT evaluation paths, unwraps member values at array construction, and raises XTTE4060 for multi-item per-focus results containing a JNode. All six unchanged native cases pass; the original five-pass/one-failure receipts remain historical. Five additional regression checks cover direct, empty, conditional and variable-carried errors plus valid grouped/nested wrappers. Source candidate 1.1.4 is unpublished; 1.1.3 remains the published version. N01 and REQ-126 stay In Progress; independent processor confirmation and upstream submission remain outstanding.


**Upstream submission — 2026-10-12:** The first six-case native array/member batch is [submitted as xslt40-test PR #22](https://github.com/qt4cg/xslt40-test/pull/22). All six exact cases pass with Bosak; specification review is recorded. No second processor was run: independent cross-check was requested in the PR. [Submission details](../contributions/xslt40/array-member-boundaries/SUBMISSION.md) explain the Saxon PE/EE and partial-for-each limitation and the decision to seek upstream review without making local independent execution a prerequisite. Review/acceptance remain pending; N01 and REQ-126 stay In Progress. Bosak fix PR #202 merged after CI 38295568498 passed (6811 unit tests); source 1.1.4 remains unpublished, public release 1.1.3 unchanged.


**Remaining priority element batches — 2026-10-12:** Four additional native batches are submitted: [select PR #23](https://github.com/qt4cg/xslt40-test/pull/23), [switch PR #24](https://github.com/qt4cg/xslt40-test/pull/24), [item-type PR #25](https://github.com/qt4cg/xslt40-test/pull/25), [record-type PR #26](https://github.com/qt4cg/xslt40-test/pull/26). Eight new cases validate against native catalog schemas: switch 2 pass; select 2 fail (empty select results), item-type 2 fail (XPST0051), record-type/field 2 fail (constructor XPST0017). Normative outputs are retained, with [portable patches and detailed example/evidence pages](../contributions/xslt40/README.md). No independent processor was run; reviewer cross-check requested in each PR. These are initial priority-family contributions, not complete XSLT 4.0 coverage. N02–N05 and REQ-126 stay In Progress while upstream review and engine triage are pending. Source 1.1.4 remains unpublished; public release 1.1.3.


**Select body evaluation — 2026-10-12:** Bosak now recognizes and evaluates xsl:select body XPath in template, function, simple-content and accumulator-rule paths, preserving literal newlines/CDATA and current focus without TVT expansion. Declared result coercion, collation restoration, fallback/content validation and streamed-body analysis are covered by 14 focused regressions. Both unchanged native contribution cases pass; [post-fix evidence](../contributions/xslt40/select-boundaries/POST_FIX_RESULTS.json) retains the broader seqtor result 78/5/9. The five remaining failures concern expression-body namespace declarations, so complete select conformance is not claimed. Original failed receipts and upstream PR #23 remain unchanged; independent confirmation/acceptance are pending. Source candidate is 1.2.0 under the Minor-for-new-features convention, unpublished; public release remains 1.1.3. N02 and REQ-126 stay In Progress.


**Enumeration alias slice — 2026-10-12:** Bosak source candidate 1.2.0 (unpublished) now resolves xsl:item-type enumeration aliases in XPath instance-of and treat expressions, including AVTs, namespace rebinding, EQNames and occurrence indicators. Both unchanged native cases type-alias-023/024 pass; broader type-alias-filter run is 3 pass/29 fail/0 skip (includes inherited related cases). Other alias types, named XSLT @as conversion, package exposure and complete recursive/schema-collision validation remain unsupported. Duplicate winning declarations raise XTSE4030; lower-precedence definitions remain overridable. Original failure receipts are preserved; upstream PR #25 review and independent confirmation are pending. N04 and REQ-126 remain In Progress; public release remains 1.1.3. Next: generated nominative record constructors, with wider alias support and select namespace declarations tracked separately.


**Named record constructor slice — 2026-10-13:** Bosak source candidate 1.2.0 (unpublished) now recognizes xsl:record-type/xsl:field and registers constructor=yes functions. Constructors retain field order and empty entries, convert field values, supply trailing defaults and exclude static fields from arguments. Records carry declaration identity: constructed values satisfy named @as and instance-of tests; same-shaped other types and plain maps do not. Both unchanged native cases record-026/027 pass. The wider record substring sweep is 12 pass/52 fail/1 skip, including inherited structural/subtype cases. Plain-map coercion/casts, recursive record definitions, complete package exposure/function signatures and streaming conformance remain follow-ups. N05 and REQ-126 remain In Progress; original failed receipts and upstream PR #26 remain unchanged, with review/independent confirmation pending. Public release remains 1.1.3; no publication. Next: prioritize the wider named-type/coercion gaps and select body namespace declarations after checking this slice's CI.


**Select namespace follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) now supports expression-local XPath 4.0 namespace declarations in the XPath API and xsl:select bodies. Prefix bindings override host bindings; an optional default element namespace must come first. Default ##any matches unprefixed element axis name tests across namespaces while preserving attribute/kind-test boundaries and XML Schema item type resolution. Namespace contexts restore after success or failure. All unchanged native seqtor-140–146 cases pass; the broader seqtor-filter run is 83 pass/0 fail/9 skip. The nine known harness skips, independent confirmation and upstream PR #23 acceptance remain outstanding, so this does not establish complete XSLT 4.0 conformance. N02 and REQ-126 remain In Progress; public release remains 1.1.3. See [current namespace evidence](../contributions/xslt40/select-boundaries/NAMESPACE_RESULTS.json) and the [pinned XPath namespace declaration source](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xquery-40/src/expressions.xml). Twenty-three regressions were added; catalogue entry/check counts are unchanged. Next: named item-type conversion and remaining record coercion gaps, after this slice passes CI. Earlier select counts are historical.


**Enumeration binding follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) now converts named enumeration aliases at XSLT variable, parameter and result boundaries. Namespace/EQName aliases and occurrence indicators resolve from the declaring instruction; atomization and membership use XPath function conversion rules. Direct/dynamic function metadata uses the same definitions; private package aliases are isolated and restored. Sixteen regressions cover valid values, membership/cardinality errors, source attributes, record fields, template/select/function boundaries and mixed-version refusal. The invoice currency fixture now returns EUR; invalid XYZ still raises XTTE0570. Original native type-alias-023/024 still pass; broader filtered family remains 3/29/0, with structural/recursive/schema/package gaps outstanding. N04 and REQ-126 stay In Progress; no complete alias or package-conformance claim. Public release remains 1.1.3. See [binding evidence](../contributions/xslt40/item-type-boundaries/BINDING_RESULTS.json) and [XSLT 4.0 named item types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types). Catalogue counts remain 67 entries / 2,578 feature + 24 chapter checks. Next: structural aliases and remaining named-record coercion, after CI. Earlier named-enumeration binding refusals are historical.


**Atomic and array binding follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) supports atomic and array item-type aliases at XSLT declaration and function boundaries. Array member types can use forward aliases, nested arrays, optional member sequences and enumeration aliases. Definition namespace bindings are preserved; outer occurrence indicators apply to arrays rather than their members. Supported direct/indirect recursive aliases raise XTSE4035. XPath 4.0 array coercion converts member sequences without atomizing the outer array; the XPath 3.1 path remains unchanged. Native type-alias-001/002 now pass; the filtered family is 6 pass/26 fail/0 skip, including unchanged contribution cases 023/024. Non-enumeration aliases in XPath instance-of/treat, general map/function/choice aliases, complete recursive validation, schema collisions and package exposure remain follow-ups. N04/REQ-126 remain In Progress. Public release remains 1.1.3. See [separate evidence](../contributions/xslt40/item-type-boundaries/ARRAY_BINDING_RESULTS.json). Existing catalogue counts and expected outcomes are unchanged. Earlier slice results are historical. Next: structural aliases in XPath type expressions, other structural alias forms, then remaining named-record coercion.


**Atomic and array type-expression follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) resolves the supported atomic and array xsl:item-type aliases in XPath instance-of and treat expressions, including AVTs and expression-local namespace declarations. Tests preserve alias occurrences and definition namespaces, distinguish type checking from conversion, and isolate private package definitions from same-named caller aliases. Mixed XPath 3.1 instructions still refuse these aliases. Fourteen new regressions pass; the public EnumerationItemTypes option retains its enum-only contract. General map/function/choice aliases, aliases nested in arbitrary XPath structural type syntax, complete schema/package semantics and remaining named-record coercion remain follow-ups. N04/REQ-126 remain In Progress. Public release remains 1.1.3. The native filtered family remains 6 pass/26 fail/0 skip; original 023/024 still pass. See [separate evidence](../contributions/xslt40/item-type-boundaries/EXPRESSION_RESULTS.json). Catalogue counts and expected outcomes remain unchanged. Earlier slice results are historical. Next: other structural alias forms and nested references, then remaining named-record coercion.


**Typed map aliases and nested references — 2026-10-13:** Source candidate 1.2.0 (unpublished) supports typed map xsl:item-type aliases and nested alias references inside array/map types at XSLT binding/function boundaries and XPath instance-of/treat operands. Shared expansion respects declaration/use namespaces, occurrence indicators, quoted enumeration members, EQName URI delimiters, whitespace and nested comments. XPath 4.0 typed-map coercion recursively converts keys and values, rejects converted key collisions, preserves entry order and retains already-matching maps/record identity. The new coercion path excludes wildcard map types; their existing behavior is preserved. Twenty new regressions cover these boundaries and the documented sensor example. Function/choice/structural-record aliases, arbitrary type syntax, complete schema/package semantics and remaining named-record coercion remain follow-ups. N04/REQ-126 remain In Progress; public release remains 1.1.3. Broader native type-alias results remain 6/26/0; original 023/024 pass. See [separate evidence](../contributions/xslt40/item-type-boundaries/MAP_RESULTS.json). Catalogue counts/outcomes and original native assertions remain unchanged. Earlier slice results are historical. Next: function and choice alias forms, then remaining named-record coercion.


**Function and choice aliases — 2026-10-13:** Source candidate 1.2.0 (unpublished) now expands function signatures and ordered choice xsl:item-type aliases, including nested references, parameter/result occurrences and outer function-item occurrences. Existing binding conversion wraps function results and tries choice alternatives in declaration order; instance-of/treat checks do not coerce. XPath 4.0 function instance-of now uses cardinality-aware matching, safely rejecting non-functions and handling empty sequences. Nineteen new regressions cover these boundaries, cycles, mixed-version refusal and a calibration example. Structural-record aliases, remaining named-record coercion and complete schema/package semantics remain follow-ups. N04/REQ-126 remain In Progress; public release remains 1.1.3. Native type-alias improves to 7/25/0: case 011 now passes; original 023/024 remain green. See [separate evidence](../contributions/xslt40/item-type-boundaries/FUNCTION_CHOICE_RESULTS.json). Catalogue counts and original native expectations are unchanged; earlier slice notes are historical. Next: structural-record alias boundaries and remaining named-record coercion.


**Structural-record aliases — 2026-10-13:** Source candidate 1.2.0 (unpublished) now expands structural-record aliases and aliases nested in record fields, arrays, choices and function signatures. Quoted field names, default item()* fields, trailing commas, nested comments and declaration namespaces are preserved. Existing structural record coercion converts field values and annotates records; type checks require record values and do not coerce plain maps. Eleven new regressions cover the documented sensor example, nested contracts, function boundaries, invalid fields and cycles. Nominative record coercion, recursive named records and complete schema/package semantics remain follow-ups. N04/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](../contributions/xslt40/item-type-boundaries/RECORD_ALIAS_RESULTS.json). Catalogue counts and original native assertions remain unchanged; earlier slice notes are historical. Next: nominative record coercion boundaries.

Current native type-alias follow-up: 8 passed, 24 failed, 0 skipped; case 005 newly passes. Original contribution cases 023/024 remain green.


**Nominative record coercion — 2026-10-13:** Source candidate 1.2.0 (unpublished) now coerces maps to package-local nominative xsl:record-type declarations at variable and function boundaries. Conversion applies field types, missing-field defaults and static initializers, rejects surplus keys and supplied static fields, preserves declaration order and annotates the result with the target identity. Existing matching records are retained. Type checks still reject plain maps and distinguish same-shaped declarations. Nine new regressions cover sensor defaults, function references, cardinality, invalid bindings and identity; the previous valid-map refusal test now rejects surplus fields. Named casts, recursive named records, nested named-type/static namespace resolution and complete package exposure remain follow-ups. N04/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](../contributions/xslt40/record-type-boundaries/COERCION_RESULTS.json). Catalogue counts remain unchanged; the named-record plain-map fixture now expects INV-2048. Original native assertions remain unchanged; earlier slice notes are historical. Next: named record cast and nested type resolution boundaries.


**Named record casts and nested references — 2026-10-13:** Source candidate 1.2.0 (unpublished) now supports package-local nominative record cast/castable operations. Casts retain map operands, cast each present field sequence, apply defaults to missing fields, discard surplus keys, protect static fields and attach the target declaration identity. Nested named references in binding/function signatures resolve in their declaration namespace, and nested named record fields reuse casting recursively. Fourteen new regressions cover lexical/type failures, empty/optional inputs, field cardinalities and namespace boundaries. General array/map casts, item-type alias cast expansion, recursive definitions, full package exposure and complete static diagnostics remain follow-ups. N05/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](../contributions/xslt40/record-type-boundaries/CAST_RESULTS.json). Catalogue counts/outcomes and original native assertions remain unchanged. Earlier slice notes are historical. Next: remaining recursive/static type-resolution and cast-container boundaries.


**Finite recursive records and container casts — 2026-10-13:** Source candidate 1.2.0 (unpublished) now casts XPath 4.0 arrays/maps by casting member/key/value sequences, retaining container shape and checking cardinality. Map-key collisions raise XQDY0137; wildcard casts retain the supplied container and empty container types reject nonempty inputs. Named record field casts reuse the shared sequence caster. Record field metadata resolves aliases and finite self/mutual named references in the declaration namespace without expanding nominative identities. Sixteen new regressions cover finite linked lists, recursive array casts, nested containers, invalid members and collisions. Arbitrary-depth resource handling, full recursive/static diagnostics, item-type alias cast expansion and package exposure remain follow-ups. N05/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](../contributions/xslt40/record-type-boundaries/CONTAINER_RESULTS.json). Catalogue counts/outcomes and original native assertions remain unchanged; earlier slice notes are historical. Next: item-type alias cast expansion and static named-type validation.


## Item-type alias casts and declaration validation — 2026-10-13

Source candidate 1.2.0 (unpublished) expands named item-type aliases in XPath 4.0 cast and castable targets, including nested container targets and namespace forms. Atomic, enumeration, array, map and structural/nominative record aliases reuse existing casting. Optional targets preserve empty sequences. Function and generic item/node aliases raise XPST0080 as casting targets. Unknown named references in supported alias definitions raise XPST0051 during stylesheet registration; declared record references preserve nominative identity. Eighteen new regressions pass. Full schema validation, unsupported alias grammar, unknown names outside declarations, choice-target eligibility and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress; published version stays 1.1.3.

See [cast and validation evidence](../contributions/xslt40/item-type-boundaries/CAST_VALIDATION_RESULTS.json). Native type-alias remains 8/24/0; unchanged contributed cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Choice casting target validation — 2026-10-13

Source candidate 1.2.0 (unpublished) statically rejects XPath 4.0 cast/castable choices containing syntactically non-atomic alternatives, including arrays, maps, records, functions and node tests. Validation covers direct targets and expanded aliases, follows nested parentheses and preserves quoted separators and EQName text. Atomic/enumeration choices, optional inputs, ordinary container-valued instance-of choices and standalone container casts remain supported. Twenty-one new regressions pass. Schema-defined and nominative QName classification, unknown expression type names, broader alias grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress; published version stays 1.1.3.

See [choice casting evidence](../contributions/xslt40/item-type-boundaries/CHOICE_CAST_RESULTS.json). Native type-alias remains 8/24/0; unchanged contributed cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Host static named-type validation — 2026-10-13

Source candidate 1.2.0 (unpublished) validates package-local named types in supported XPath 4.0 instance-of, treat, cast and castable targets when the XSLT host compiles the expression. Unknown direct/nested names raise XPST0051, including AVT expressions; undeclared prefixes retain XPST0081. Declared nominative record references are canonicalized to their expanded names, and record alternatives in choice cast targets raise XPST0080. Ordinary record-valued type-test choices remain valid. The internal host registry survives inline namespace declarations, recognizes built-in/XDM types and consults in-scope simple schema types. Twenty-one new regressions pass. Binding/field declaration validation, unsupported type grammar, complete schema variety/choice semantics and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress; published version stays 1.1.3.

See [static named-type evidence](../contributions/xslt40/item-type-boundaries/STATIC_TYPE_RESULTS.json). Native type-alias remains 8/24/0; unchanged contributed cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Declaration static named-type validation — 2026-10-13

Candidate 1.2.0 now validates supported XPath 4.0 sequence-type names on XSLT @as declarations at stylesheet registration, including unused variables, parameters, function results and record fields. The check runs after package-local aliases and records are registered and traverses import/include modules with each declaration's namespace context. Unknown direct/nested names raise XPST0051; undeclared binding prefixes raise XPST0081; record registration retains XTSE0280 for unbound field prefixes. Forward aliases, finite recursive record references, default type namespaces and quoted enumeration/field names remain valid. Fourteen new regressions cover these boundaries. XPath 3.1 declarations keep their existing path. Unsupported shared-walker grammar, complete schema variety semantics and package type exposure remain follow-ups; N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; no release or gate promotion.

[Verification evidence](../contributions/xslt40/item-type-boundaries/DECLARATION_TYPE_RESULTS.json). Native type-alias: 9 pass/23 fail/0 skip; type-alias-019 newly passes and unchanged contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Node-kind alias grammar — 2026-10-13

Candidate 1.2.0 supports node-kind aliases whose tests have empty parentheses: element(), attribute(), text(), comment(), processing-instruction(), namespace-node() and document-node(). The shared type walker also accepts whitespace and nested comments around empty tests. These types can appear within supported array/map/record/function/choice types; adjacent unknown names now receive static validation instead of being hidden by an unsupported node-kind branch. Sixteen regressions verify node identity, occurrences, structural nesting, unknown names, binding failures and non-atomic choice cast rejection. Named/wildcard/schema-annotated node tests, fn/named-parameter function type grammar, full schema semantics and package type exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version remains 1.1.3; candidate 1.2.0 stays unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/NODE_KIND_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Named node-type alias grammar — 2026-10-13

Candidate 1.2.0 supports named element/attribute node tests, the complete * wildcard, and document-node(element(...)) inside package-local aliases and supported structural sequence types. Node names resolve in their declaration namespace and remain stable under use-site prefix/default namespace rebinding; unprefixed attributes retain the empty namespace. The host statically rejects unbound node prefixes and validates unknown type names beside named node tests. Explicit Q{}Name matching now checks both namespace and case-sensitive local name. Seventeen new regressions cover matching/mismatches, node identity, nested types, record fields, namespace defaults and static errors. Schema-annotated node tests, partial wildcards, processing-instruction targets, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/NAMED_NODE_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Processing-instruction target aliases — 2026-10-13

Candidate 1.2.0 supports processing-instruction targets in item-type aliases and supported structural sequence types. NCName and quoted-string targets retain case, normalize surrounding whitespace and are never expanded as type QNames. The parser validates targeted sequence-type syntax using a shared target reader; runtime matching compares the actual PI target. Malformed targets raise XPST0003 and quoted targets outside the NCName lexical space raise XPTY0004. Twelve new regressions verify identity, target/case mismatch, nesting, optional occurrences and static diagnostics. Partial wildcards, schema-annotated node tests, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/PI_TARGET_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Partial node wildcard aliases — 2026-10-13

Candidate 1.2.0 supports partial element/attribute name wildcards in aliases and supported structural types: *:Name matches an exact local name in any namespace; prefix:* and Q{uri}* match any local name in the specified namespace. Prefix wildcards retain declaration bindings under use-site rebinding, including document-node(element(...)) nesting. Unbound wildcard prefixes raise XPST0081; adjacent unknown type names retain XPST0051. The ##any declaration default now expands unprefixed element tests to *:Name while unprefixed attributes stay unqualified. Twenty new regressions verify identity, case/namespace mismatches, empty namespaces, nested arrays/documents and diagnostics. Node-name unions, schema-annotated tests, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/NODE_WILDCARD_RESULTS.json). Native type-alias improves to 11 pass/21 fail/0 skip: wildcard-type-001/002 newly pass and original contribution cases 023/024 still pass. Broader failures and independent processor confirmation remain outstanding.


## Node-name union aliases — 2026-10-13

Candidate 1.2.0 supports node-name unions in element/attribute item-type aliases and supported structural sequence types: element(doctor|nurse) and attribute(price|cost) match any listed name. Each alternative retains declaration namespaces and may use a partial wildcard, including nested document-node(element(...)), arrays and record fields. EQName namespace URIs containing pipe or comma characters remain intact. Empty alternatives raise XPST0003; an undeclared prefix in any alternative raises XPST0081; adjacent unknown type names retain XPST0051. Twenty-two new regressions cover positive/negative matching, identity, declaration prefix rebinding, wildcard combinations, nested types and ##any defaults. Schema-annotated alias traversal, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/NODE_UNION_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Schema-annotated node aliases — 2026-10-13

Candidate 1.2.0 supports schema annotations in element/attribute item-type aliases and supported structural sequence types, including node-name unions, nested documents, arrays and record fields. The annotation resolves independently against in-scope schema types and retains declaration namespaces under prefix rebinding; item-type aliases are not substituted into the schema annotation. Built-in and imported simple/complex schema types are accepted. Unknown annotations raise XPST0008; undeclared annotation prefixes raise XPST0081; malformed or empty annotations raise XPST0003. Only element annotations permit the nillable ? marker. The ##any default maps unprefixed schema annotations to the XML Schema namespace. Twenty-eight new regressions cover matching untyped and validated nodes, non-coercion, imported simple/complex types, nillability, namespace identity, nested contracts and unused declarations. fn/named-parameter function grammar, schema-element/schema-attribute alias traversal, document shorthand and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/SCHEMA_NODE_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Function-type grammar aliases — 2026-10-13

Candidate 1.2.0 accepts fn(...) as a synonym for function(...) and documentary parameter names in supported function-type aliases, direct XPath type operands and XSLT binding signatures. Names do not affect matching or coercion; duplicate expanded names raise XQST0039, including different prefixes bound to the same namespace and unprefixed names paired with Q{} names. Undeclared documentary prefixes raise XPST0081; malformed named parameters raise XPST0003; unknown parameter/result item types retain XPST0051. Named and unnamed parameters may be mixed, with independent name scopes for nested function types. Nested arrays, records and higher-order signatures retain parameter/result sequence occurrences. XPath 3.1 rejects fn type synonyms and named type parameters. Twenty-six new regressions cover callback conversion, wildcard maps/arrays, direct bindings without aliases, direct host evaluation, nesting, namespace identity, diagnostics and version boundaries. Annotated function types, schema-element/schema-attribute alias traversal, document shorthand and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/FUNCTION_GRAMMAR_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Schema-declaration and document shorthand aliases — 2026-10-13

Candidate 1.2.0 supports schema-element(Name), schema-attribute(Name) and document-node(schema-element(Name)) in item-type aliases and supported structural sequence types. Schema declaration names retain their declaration namespace under prefix rebinding and must resolve to imported global elements/attributes; unknown or unavailable declarations raise XPST0008 and undeclared prefixes raise XPST0081. Wildcards, unions and empty declaration names raise XPST0003. Schema-element under ##any uses no namespace for an unprefixed declaration; unprefixed schema attributes always use no namespace. Document shorthand such as document-node(root|other) normalizes to document-node(element(root|other)), including wildcard and EQName alternatives, and reuses single-root matching. Direct host type operands and alias-free XSLT document/schema bindings normalize the new forms. Twenty-nine new regressions cover node identity, namespace mismatches, imported declarations, substitution members, typed attribute matching, rejection of untyped nodes, nested arrays, ##any defaults, empty documents and static validation of unused bindings. Annotated function types, package exposure and broader native type-alias gaps remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/SCHEMA_DECLARATION_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Package named type exposure — 2026-10-13

Candidate 1.2.0 accepts item-type and record-type components in xsl:expose and xsl:accept. Public or exposed named types enter using-package type registries unless accepted as hidden; private dependencies retain the defining package's resolved alias definitions and record metadata. Accept names resolve namespaces on the accept element. Missing component names, incompatible acceptance, invalid declaration visibility and collisions with local types produce static errors. Seventeen regressions cover public declarations, explicit and wildcard exposure, private and hidden names, prefix scope, record constructors, private alias dependencies rejected collisions and public/private visibility through an intermediate package. Native type-alias improves from 11/21/0 to 15/17/0; newly passing cases are 018, 020, 021 and 022. Original contribution cases 023/024 remain green. Broader native failures and independent processor confirmation remain outstanding; N04/N05 and REQ-126 stay In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/PACKAGE_EXPOSURE_RESULTS.json). The [XSLT 4.0 named type and package rules](https://qt4cg.org/specifications/xslt-40/Overview.html) define these visibility boundaries.


## Fixed namespace type contexts — 2026-10-13

Candidate 1.2.0 resolves fixed-namespaces for named item/record type names, supported nested type references, static declaration validation and the main XPath compilation paths. Supported tokens include #standard, selected root-native or conventional prefixes, explicit prefix=URI bindings and namespace-document references through the existing stylesheet resolver. Bindings are module-local, cached at compilation, unaffected by descendant rebinding and applied in token order. Invalid bindings or unavailable namespace documents raise XTSE0122. Unprefixed type/node names retain their existing default-namespace rules; fixed bindings do not create native result namespaces. Sixteen regressions cover standard prefixes, nested node contracts, explicit/repeated bindings, namespace documents, include isolation and invalid/unavailable bindings. Native type-alias remains 15/17/0: case 016 now passes namespace resolution but still fails its independent inline-function record conversion contract with XPTY0004. No upstream assertion changes. Fixed namespace coverage of other QName-bearing XSLT attributes, pattern compilation and shadow/static processing remains a follow-up; this is not complete fixed-namespaces support. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/FIXED_NAMESPACE_RESULTS.json). The [XSLT 4.0 fixed-namespaces rules](https://qt4cg.org/specifications/xslt-40/Overview.html#fixed-namespaces) define the namespace token and scope rules.


## Inline function named type contracts — 2026-10-13

Candidate 1.2.0 expands host named types in inline function parameter and return contracts before IR lowering. Both function(...) and fn(...) retain resolved atomic, array, structural record and nominative record types, including occurrence indicators and nested member aliases. Signature resolution uses the expression static namespace context, including defaults, EQNames, inline namespace declarations and fixed bindings; the existing VM conversion rules apply to arguments and results. Unknown supported signature types are rejected statically even when the function is unused. Public EnumerationItemTypes registrations also resolve in inline signatures and remain restricted to single enum item types. Omitted annotations and XPath 3.1 instructions keep their existing behavior. Twenty-five regressions cover record/date updates, argument and return coercion, cardinality errors, namespaces, array members, record identity, AVTs, public enumeration boundaries and mixed-version refusal. Native type-alias improves from 15/17/0 to 16/16/0: case 016 now passes without changing its assertion; original contribution cases 023/024 remain green. Broader record matching, annotated function types, type patterns and remaining native failures stay outstanding. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/INLINE_FUNCTION_RESULTS.json). The [XPath 4.0 inline function rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-inline-func) define parameter and result contracts.


## Direct structural record bindings — 2026-10-13

Candidate 1.2.0 coerces direct structural record(...) and record(*) XSLT binding contracts through the existing XPath 4.0 converter. Supported direct record fields resolve aliases and node/type namespaces in their declaration context. Local/global variables and template parameters preserve record annotations and numeric field conversion; invalid values retain XTTE0570/XTTE0590 binding diagnostics. Required single-record and record-plus conversion now reject empty sequences in both empty-value representations, while optional/star records retain empty-sequence support. Plain maps still do not match annotated record instance tests; record(*) requires an existing annotation. Seventeen regressions cover local/global/default-parameter bindings, occurrence indicators, field aliases and node contracts, empty records, invalid values/extra fields, parameter diagnostics and required inline function returns. The local inline-record40 invoice example now verifies INV-2048|EUR instead of the previously observed XTTE0570. Native type-alias stays 16/16/0: case 050 now gets past XTTE0570 record coercion but still fails its independent result assertion. Upstream assertions remain unchanged. Type patterns, broader native record outcomes and annotated function types remain follow-ups. N04/N05 and REQ-126 stay In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/record-type-boundaries/STRUCTURAL_BINDING_RESULTS.json). The [XPath 4.0 structural record rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-record-types) distinguish coercion from annotated membership.


## Template type patterns — 2026-10-13

Candidate 1.2.0 supports standalone XSLT 4.0 template type patterns ~T with zero or more predicates, including parenthesized forms, named item types, structural records and nominative records. The matcher delegates membership to the existing XPath type tests and preserves numeric predicate, singleton focus and dynamic-error-as-nonmatch behavior. Template patterns compile with the declaring package type registry and applicable static namespaces, including fixed-namespaces. Default priorities cover item/node/function/map/array families, built-in atomic derivation depth, enumerations and supported choice types; predicate patterns have priority 0.5 and explicit priorities are retained. xs:anyAtomicType is now recognized in host-supplied static type contexts. Twenty-nine regressions cover these behaviors, invalid syntax, previous-profile rejection, quoted field names, EQName delimiters and package-private alias isolation. Native type-alias advances from 16/16/0 to 18/14/0: cases 008 and 009 now pass with their upstream assertions unchanged. Ordinary wildcard node patterns, full XSLT 4.0 union-pattern priority semantics, schema-defined atomic priority derivation and named-type/fixed-namespace authoring-validation contexts remain follow-ups. Non-template pattern slots have not gained the template declaring-type registry in this slice. Plain maps still do not acquire record annotations through pattern matching; cases 006/010/013 and case 050 result handling remain independent boundaries. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/TYPE_PATTERN_RESULTS.json). The [XSLT 4.0 type-pattern rules](https://qt4cg.org/specifications/xslt-40/Overview.html) and [default priorities](https://qt4cg.org/specifications/xslt-40/Overview.html) define matching and conflict resolution.


## Wildcard node patterns — 2026-10-13

Candidate 1.2.0 matches XSLT 4.0 element(...) and attribute(...) kind-test patterns using namespace wildcards (p:* and Q{uri}*), local-name wildcards (*:local), and supported name unions. Matching applies in standalone, explicit-axis and path steps while retaining schema type checks and distinct element/attribute default namespaces. The 4.0 priority table gives untyped partial wildcards -0.25, typed all-name wildcards 0, typed partial wildcards 0.125 and typed exact names 0.25; name unions use their highest member priority. Supported node type patterns and document-node wrappers reuse those priorities. Explicit priorities are retained, and predicates raise node-pattern priority to 0.5, including explicit-axis kind tests. Earlier profiles retain their existing matching and priority behavior. Node-type schema annotations now tolerate surrounding whitespace; 4.0 pattern EQName URI rewriting and kind-test argument splitting preserve embedded delimiters. Forty-one regressions cover matching/nonmatching namespaces and types, axes/paths, unions, priorities, malformed names, prior-profile compatibility and EQName delimiters. Native type-alias advances from 18/14/0 to 22/10/0: wildcard-type-003–006 newly pass with upstream assertions unchanged. Full XSLT 4.0 union-pattern priority/next-match semantics, other node-pattern priority forms, schema-defined atomic priorities, authoring-validation named-type/fixed-namespace contexts and non-template declaring-type registries remain follow-ups. Remaining record/result cases and annotated function types remain independent boundaries; plain maps do not gain record annotations through pattern matching. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/WILDCARD_PATTERN_RESULTS.json). The [XSLT 4.0 default priority table](https://qt4cg.org/specifications/xslt-40/Overview.html) in section 6.4.3 defines the supported kind-test priorities.


## Node union patterns — 2026-10-13

Candidate 1.2.0 keeps supported XSLT 4.0 node union patterns as one template rule, using the highest default priority of their alternatives even when a lower-priority alternative is the one that matches. xsl:next-match visits that rule once, and overlapping alternatives do not create competing rule identities. Earlier effective profiles retain separate rules and their original per-alternative next-match behavior. Explicit priorities remain unchanged. Priority calculation recognizes pipe and union keyword operators, recursively computes nested alternatives, and preserves strings, predicates and EQName URIs; parenthesized 4.0 matching retains the declaring static namespace/type context. Empty top-level alternatives are rejected with XTSE0340. Twenty-eight regressions verify maximum priority, single visits, duplicate alternatives, failing ambiguity modes, nested/default namespaces, explicit priorities, version overrides, delimiters and invalid alternatives. Native next-match (including si-next-match) remains 46/8/6, and type-alias remains 22/10/0 with all prior wildcard/type-pattern gains retained. Four next-match 4.0 assertions (021a/023a/025a/039a) reference version-2.0 stylesheets and therefore still select the earlier effective profile; their fixtures and assertions remain unchanged. Processor-level selection for those lower-version native fixtures is a separate harness/API boundary. Other node-pattern priority forms, broader pattern grammar, schema-defined atomic priorities, named-type/fixed-namespace authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/UNION_PATTERN_RESULTS.json). The [XSLT 4.0 node-pattern priorities](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.4.3, define the single-rule union behavior.


## Attribute and forward-axis priorities — 2026-10-13

Candidate 1.2.0 computes supported XSLT 4.0 attribute shorthand and forward-axis pattern priorities from the underlying node test. Named attributes (@A and attribute::A) and named element axis tests have priority 0; namespace/local wildcards retain -0.25 and untargeted kind tests retain -0.5. Named processing-instruction axis tests have priority 0 and now match their target in 4.0 child/self/descendant-style node-test compilation. Existing 4.0 typed kind-test priorities, explicit priorities, predicate priority 0.5 and single-rule union behavior are retained. Earlier effective profiles keep their original behavior. Axis recognition for priority and 4.0 matching ignores :: inside EQName URIs. Thirty-three regressions compare default priorities against explicit competitors, verify earlier-profile differences, target nonmatches, attribute-union next-match chains and positive/negative EQName URI cases. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior wildcard/type-pattern gains remain green and upstream assertions/fixture versions are unchanged. Intersect/except and bare predicate-pattern priorities, broader pattern grammar, lower-version native fixture processor selection, schema-defined atomic priorities, named-type/fixed-namespace authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/FORWARD_AXIS_PRIORITY_RESULTS.json). The [XSLT 4.0 node-pattern priorities](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.4.3, assign priorities independently of supported forward-axis prefixes.


## Set and predicate-pattern priorities — 2026-10-13

Candidate 1.2.0 computes supported XSLT 4.0 intersect/except pattern default priorities from the first operand, including mixed operators, parenthesized operands and union precedence. Bare predicate pattern `.` has priority -1; predicate patterns with one or more predicates have priority +1 even when their expressions contain path slashes or slash string literals. Earlier effective profiles retain their existing behavior. Thirty-four regressions compare explicit competitors, check excluded-node and false-predicate nonmatches, atomic bare-dot matching, comments, adjacent punctuation, operator-name elements and EQName URI operator text. This is a priority calculation slice; broader pattern grammar and set-matching semantics remain separate follow-ups. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior wildcard/type-pattern gains remain green and upstream assertions/fixture versions are unchanged. Parenthesized item-pattern static validation, broader pattern grammar, lower-version native fixture processor selection, schema-defined atomic priorities, named-type/fixed-namespace authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/SET_PREDICATE_PRIORITY_RESULTS.json). The [XSLT 4.0 node-pattern priorities](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.4.3, use first-operand priorities for intersect/except; section 6.4.1 assigns bare-dot and filtered predicate-pattern priorities.


## Parenthesized item patterns — 2026-10-13

Candidate 1.2.0 admits supported parenthesized predicate and type patterns in the effective XSLT 4.0 profile and restricts those outer XNode patterns to nodes. Atomic values, maps and records therefore do not match a parenthesized item pattern; standalone predicate/type patterns retain their item matching. A prior local quoted-record-field regression incorrectly used a parenthesized type pattern for a map; it now uses the standalone type pattern, and new negative regressions verify the node boundary. Nested compilation retains declaring-package aliases, nominative type registries and applicable namespaces. Contained type priorities are recomputed after alias expansion, including wrapped union operands. Earlier profiles retain parenthesized-predicate rejection. Twenty-seven new regressions cover nesting, predicates, node-only membership, atomic/map negatives, aliases, default namespaces, contained priority and invalid direct item unions. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior wildcard/type-pattern gains remain green and upstream assertions/fixture versions are unchanged. Parenthesized outer-filter path classification, broader pattern grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, named-type/fixed-namespace authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/PARENTHESIZED_ITEM_PATTERN_RESULTS.json). The [XSLT 4.0 node-pattern priorities](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.4.3, preserve contained-pattern priorities; section 6.3.4 permits parenthesized item patterns within XNode patterns and restricts XNode patterns to nodes.


## Parenthesized outer-filter paths — 2026-10-13

Candidate 1.2.0 classifies supported XSLT 4.0 parenthesized outer filters using path slashes outside predicates, string literals and EQName namespace URIs. Patterns such as `(.[child/..])[true()]` keep candidate-node focus instead of incorrectly evaluating from the document root. Actual path patterns retain sequence-based positional filtering, including nested parentheses, descendant paths and paths whose EQName URIs contain slashes. Earlier effective profiles retain their existing classification. Twenty-five regressions cover candidate predicates, nested predicates, escaped quotes, URI slashes, aliases, real path positions and nonmatches. The initial twenty-one-case baseline had six failures before the fix; all final twenty-five cases pass. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Pattern delimiter scanning around quoted parentheses/brackets, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, fixed namespace/authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/PARENTHESIZED_FILTER_PATH_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.3.4.2.


## Pattern delimiter scanning — 2026-10-13

Candidate 1.2.0 matches pattern parentheses and brackets outside string literals and EQName namespace URIs, including doubled quote escapes. Predicate extraction, parenthesized compilation and template normalization/priority share the same lexical delimiter matcher. Static function-call validation ignores parentheses inside EQName URIs. Nineteen regressions cover quoted opening/closing delimiters, nested predicates, escaped single/double quotes, URI parentheses, contained priority, false outer predicates and ordinary earlier-profile node patterns. The baseline was 6/13/0 before the fix; all nineteen now pass. Lexical corrections apply to supported patterns across profiles; existing profile-specific grammar and priority rules remain intact. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Top-level operator splitting around quoted delimiters, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, fixed namespace/authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/PATTERN_DELIMITER_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 6.3.1 and 6.3.4.


## Pattern operator scanning — 2026-10-13

Candidate 1.2.0 splits supported top-level pattern operators outside string literals, doubled quote escapes, EQName namespace URIs and nested expressions. The shared scanner handles pipe unions and union/intersect/except keywords without splitting complete node-name tokens such as pre-except-post, pre_intersect_post or pre.union.post. Earlier-profile union rule expansion and type-choice priority splitting reuse the lexical scanner; existing profile-specific priority and grammar rules remain intact. Twenty-three regressions cover quoted delimiters/operator text, nested predicates, escaped quotes, URI punctuation, complete operator-like names, nonmatches and earlier-profile unions. The baseline was 17/6/0 before the fix; all twenty-three now pass. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Mixed intersect/except associativity, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, fixed namespace/authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/PATTERN_OPERATOR_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 6.3.1 and 6.3.4.


## Set-pattern associativity — 2026-10-13

Candidate 1.2.0 evaluates supported mixed intersect/except pattern chains from left to right in the effective XSLT 4.0 profile. Both operators have equal precedence: `root except missing intersect missing` does not match root, while `root except (missing intersect missing)` does. The matcher preserves parenthesized grouping, union precedence, quoted/URI operator text and existing all-path/all-axis expression evaluation. Missing operands in mixed chains raise XTSE0340. Earlier profiles retain their existing dispatch behavior. Eighteen new regressions cover membership, nonmatches, grouping, union precedence, URI/quoted punctuation, path/axis negatives and earlier-profile behavior. The baseline was 10/8/0 before the fix; focused new plus prior operator checks pass 41/0/0. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Forward-axis operand focus in 4.0 set patterns, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, fixed namespace/authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/SET_PATTERN_ASSOCIATIVITY_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 6.3.1 and 6.3.4.


## Set-pattern axis focus — 2026-10-13

Candidate 1.2.0 matches supported forward-axis operands in XSLT 4.0 intersect/except patterns against the candidate node instead of forcing axis-only chains into document-root XPath evaluation. Mixed axis/path operands use their respective pattern matchers; all-actual-path chains retain sequence evaluation and positional selection. The 4.0 single-pattern classifier reuses lexical path detection so slashes inside predicates or string literals do not turn an axis pattern into a path. Earlier profiles retain existing dispatch. Twenty-two regressions cover self/child/descendant-style axes, attributes, predicates, mixed actual paths, all-path positions, EQName URIs, first-operand priority and earlier-profile behavior. The baseline was 9/13/0 before the fix; all twenty-two now pass. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Fixed namespace resolution in 4.0 template pattern names, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/SET_PATTERN_AXIS_FOCUS_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 6.3.1 and 6.3.4.


## Pattern fixed namespaces — 2026-10-13

Candidate 1.2.0 resolves prefixes in effective XSLT 4.0 template patterns using the declaring applicable static namespace bindings. Fixed bindings now survive native rebinding in node names, attributes, axes, unions, set operands, nested type tests and predicates. Existing EQNames and quoted text are preserved. Without fixed bindings, native declarations remain applicable; earlier profiles retain their native lookup. Twenty-one regressions cover these boundaries; the initial nineteen-test baseline was 3/16/0. Native next-match remains 46/8/6 and type-alias remains 22/10/0; upstream assertions and fixtures are unchanged. Broader pattern grammar, omitted-prefix diagnostics, non-template authoring/static contexts, schema-defined atomic priorities, lower-version native fixture processor selection, record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/PATTERN_FIXED_NAMESPACE_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 3.6.1 and 6.3.1.


## Pattern prefix diagnostics — 2026-10-13

Candidate 1.2.0 reports XPST0081 during compilation of effective XSLT 4.0 template patterns when a lexical QName or prefixed wildcard uses an unavailable prefix. This includes native prefixes excluded by fixed-namespaces and undeclared prefixes without fixed bindings. A shared XPath-lexer check covers node names, attributes, axes, paths, union/set operands, function/variable references and nested type patterns; string literals, nested comments and EQName URIs remain protected. Earlier-profile loading retains its existing behavior. Thirty-three regressions cover omitted bindings, Unicode and dotted prefixes, protected text, native bindings across 2.0/3.0/4.0 and a direct lexical check distinguishing spaced map-entry delimiters from prefix wildcards; the initial twenty-four-test baseline was 4/20/0. Native next-match remains 46/8/6 and type-alias remains 22/10/0; upstream assertions and fixtures are unchanged. This bounded check does not perform full eager pattern compilation. String-template interpolation checks, non-template authoring/static contexts, broader pattern grammar, schema-defined atomic priorities, lower-version native fixture processor selection, record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../contributions/xslt40/item-type-boundaries/PATTERN_PREFIX_DIAGNOSTIC_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 3.6.1 and 6.3.1.
