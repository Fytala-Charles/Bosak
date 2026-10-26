<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-126 closure backlog and acceptance plan">
  <h1>REQ-126 closure plan</h1>
  <p>26 October 2026 — plan baseline v1; public Bosak 1.26.3</p>
</div>

**REQ-126 C07 complete — 26 October 2026:** [Lifecycle/disposition review](LIFECYCLE-RECONCILIATION.md) reconciles all 76 classifications and 19 original/wider decision groups. All 3,519 fresh existing tests and 113 profile controls pass. Current compound-pattern wording, method-call boundaries and a malformed delivery link are corrected with original receipts retained. Six closure workpackages remain; C08 historical adoption evidence is next. All 28 missing adoption pins, earlier defects and semantic acceptance remain open. No feature/profile/gate promotion or release; skips, Braid and extension work stay deferred. Earlier checkpoints are historical.

**REQ-126 C06 complete — 26 October 2026:** [Host/profile review](HOST-PROFILE-RECONCILIATION.md) reconciles the final three grammar rows; all 18 now have bounded reviews, not semantic acceptance. All 376 focused tests and 3,148 catalog examples pass. Of 154 probe observations, 133 controls match, two retain a switch comparison mismatch, and 19 record context boundaries. Seven closure workpackages remain; C07 is next. Static-mode and loader/adoption limits remain explicit. No engine, profile, gate or release change; skips, Braid and extension work stay deferred. Earlier checkpoints are historical.

**REQ-126 C05 complete — 26 October 2026:** [Type/collection/pattern review](TYPE-COLLECTION-PATTERN-RECONCILIATION.md) reconciles nine type/collection/pattern rows and 49 selected production names. All 2,457 focused tests pass. Of 275 independent controls, 259 match; 16 retain choice-coercion, record-destructuring and known generic-model gaps. Delivered union/EQName fixes remain closed. Eight closure workpackages remain; C06 is next. No engine or executable example changes, acceptance waiver, profile/gate promotion or release. Skips, Braid and extension work stay deferred; earlier checkpoints are historical.

**REQ-126 C04 complete — 26 October 2026:** [Expression/call/focus review](EXPRESSION-CALL-FOCUS-RECONCILIATION.md) reconciles six expression/call/focus rows and 33 selected production/token names. All 880 focused tests pass; 209 controls match, ten retain current-draft spelling/placeholder/mapping-focus gaps, and eight late-binding observations remain separate. Nine closure workpackages remain; C06 is next. No engine or executable example changes, gate/profile promotion, acceptance waiver or release. C02's atomic-annotation defect remains open. Skips, Braid and extension work stay deferred; earlier checkpoint counts are historical.

**REQ-126 C03 complete — 26 October 2026:** [Constructor/type review](CONSTRUCTOR-TYPE-RECONCILIATION.md) reconciles eight entries and all 49 focus constructors. All 1,333 asserted controls match across 1,357 observations; 245 focused schema/API/host tests pass. Bounded nominal defaults/static fields are now evidenced. Generic wide casts, decimal precision, schema/host context and broader conversion remain explicit boundaries; C02's atomic-annotation defect remains open. Ten closure workpackages remain; C06 is next. No engine/test/example/version/gate change or acceptance waiver; skips/Braid/extension work stays deferred. Earlier checkpoint counts are historical.

**REQ-126 C02 complete — 26 October 2026:** [Function/options reconciliation](FUNCTION-OPTIONS-RECONCILIATION.md) reconciles 30 entries/74 function surfaces with explicit supported boundaries and 14 follow-up groups. Release build and all 21,798 solution tests pass; all 3,148 catalog examples pass separately. The 195 added profile observations include 118 matching controls, two unmet controls for the new `atomic-type-annotation(())` defect, and 75 diagnostics. Eleven closure workpackages remain; C06 is next. C02 closes review scope, not AC-01–04 or the retained defects. Skip fixes, Braid and extension work remain deferred. Original receipts and frozen gates are preserved; no engine/version/release change.

**REQ-126 C01 complete — 26 October 2026:** [Current inventory and ownership record](FUNCTION-INVENTORY-REFRESH.md) refreshes 316 names against published 1.26.3 and pinned current draft b4723ff5: 119 registered 4.0-only names (70 non-constructors, 49 focus constructors), plus 197 existing names. The retained inventory missed `array:members` and `array:of-members`. All 1,581 profile lookup observations completed without probe errors; four changed declarations and remaining semantic/default/empty/error reviews have explicit C02/C03/C06 owners. Twelve closure workpackages remain open; REQ-126 and AC-01–04 remain partial. Finish these packages before skip fixes, Braid or VS Code extension work. Engine, APIs, tests, version and frozen gates are unchanged; existing passing source/gate evidence is inherited. Earlier counts and checkpoint notices are historical.

**C01-C07 are complete; 6 of the 13 scoped closure workpackages remain open.** This is a new planning baseline, not a claim that exactly 13 implementation PRs or releases will finish every engine feature. C00 decomposition, C01 live inventory, C02 function/options, C03 constructor/type, C04 expression/call/focus, C05 type/collection/pattern, C06 host/profile and C07 lifecycle/disposition reconciliation are complete. Defects and acceptance remain distinct. AC-01 through AC-04 remain open/partial; AC-05 remains met for current discoverability. No acceptance criterion or catalog entry is marked accepted by this inventory.

The baseline is main `927d7a458dd90a8b506840c045808f4f27e3c8d1`, following [verified 1.26.3 publication](../RELEASE-1.26.3-PUBLICATION.md). Engine source, tests, version, fixtures, filters and gates are unchanged. [Machine-readable plan](CLOSURE-PLAN.json) contains exact entry/grammar/pin membership, dependencies and completion conditions. [Conformance skip triage](CONFORMANCE-SKIP-TRIAGE.md) is a separate investigation queue.

## Measured inventory

| Item | Current observation | Interpretation |
|---|---:|---|
| Catalog entries | 76 | All have the 16 numbered fields and an index link; structural evidence only |
| Feature/chapter checks | 3,148 +24 | 3,172 retained documentation checks; source regressions and this triage are separate |
| Worked chapters | 4 | Retained chapter delivery scope |
| Current registered-name inventory | 119 | C01 complete: 70 non-constructors and 49 focus constructors; two omissions corrected from retained 117-name inventory |
| Retained existing-function inventory | 197 names | Declaration differences are not automatically semantic changes |
| Bounded grammar rows | 18 | All assigned exactly once to C04–C06 |
| Entries literally mentioning a current gate set | 26 | Not a semantic coverage certificate |
| Entries without a literal current gate name | 50 | Not automatically untested; XSLT/refusal/named-unit evidence can apply |
| Entries naming exact manifest IDs in Known Uses | 41 | Literal linkage, not acceptance |
| Missing historical adopted-specification pins | 28 | A current review pin cannot reconstruct adoption history |

The fresh [catalog snapshot](evidence/closure-triage/catalog-inventory.json) records entry hashes, classifications, Sample Code, Known Uses, local links, manifest IDs, gate mentions and exact missing-pin records. Earlier `COVERAGE-RECONCILIATION.json` and its dated source/version metadata remain historical evidence and are not overwritten.

## Finite closure backlog

| ID | Workpackage | Completion boundary | Criteria |
|---|---|---|---|
| C01 — complete | Live function/signature/default/keyword inventory | Refreshed 119-name, 70 non-constructor/49 focus-constructor scopes and 197 existing-function declarations; pinned review sources and classified residual contracts | AC-01 |
| C02 — complete | Function/options behavior and drift | Reconcile documented function/option contracts with published fixes, examples, errors and explicit unsupported boundaries | AC-01/04 |
| C03 — complete | Constructors, types and coercion | Reconcile lexical/value/context/error and schema-list/record/function-type boundaries without implying broader storage or casts | AC-01/04 |
| C04 — complete | Expression/call/focus grammar | Six named grammar rows: operators, calls, keyword/lexical/focus/scan boundaries | AC-01/04 |
| C05 — complete | Type/collection/pattern grammar | Nine named grammar rows: iteration, types/records/updates, patterns, constructors/destructuring and JNode scope | AC-01/04 |
| C06 — complete | Host/declaration/package/profile grammar | Three named rows: S0, S7, S8-dispatch; residual version/module/package/static contexts | AC-01/04 |
| C07 — complete | Lifecycle and disposition reconciliation | Reconcile all current classifications and original moving/rejected groups; retain historical URLs, versioned refusals and alternatives | AC-02/04 |
| C08 | Historical adoption evidence | Research all 28 exact missing records; recover immutable historical evidence or leave explicit owner-policy blockers | AC-01/02 |
| C09 | Applicable-evidence/semantic review batch 1 | First exact 19-entry manifest; per-contract evidence, sample and drift receipts | AC-03/04 |
| C10 | Applicable-evidence/semantic review batch 2 | Second exact 19-entry manifest | AC-03/04 |
| C11 | Applicable-evidence/semantic review batch 3 | Third exact 19-entry manifest | AC-03/04 |
| C12 | Applicable-evidence/semantic review batch 4 | Fourth exact 19-entry manifest | AC-03/04 |
| C13 | Final closure and owner acceptance | Resolve blocking receipts, verify examples/counts/links, and record explicit acceptance of AC-01–04; maintain AC-05 | AC-01–05 |

C09–C12 partition the current 76 entries exactly once, in stable path order. Their complete file lists are in the JSON; they are review manifests, not accepted-entry lists. Already supplied bounded examples remain evidence. Each package reviews the remaining delta instead of restarting completed work.

C01–C08 establish inventories, policy boundaries and mappings; C09–C12 apply the owner-approved applicable-evidence rule to every contract, Sample Code, Known Uses and Language Alignment. XPath contracts use applicable gates or explicit unassigned status plus named tests. XSLT-only and deferred/rejected contracts use appropriate executable or disposition/refusal/alternative evidence. Generic headings, a passing example or a link to a test set cannot substitute for semantic review.

C08 does **not** grant a waiver for missing history. If a historically adopted pin cannot be recovered, C08/C13 retain that blocker until the owner records a disposition against the acceptance policy. No missing pin is replaced silently with today's draft. New normative reviews must record an actual immutable review reference and distinguish frozen/experimental/draft policies. Any upstream test contribution must follow the current-default-branch refresh/base/receipt rules in AGENTS.md.

## Scope control and next order

The initial order is C01, then C02–C08 as their dependencies permit, C09–C12, and C13. The [skip queue](CONFORMANCE-SKIP-TRIAGE.md), Braid and the VS Code extension are deferred until all REQ-126 closure workpackages finish, as directed by the owner. C06 is next.

New engine defects receive their own bounded correction tickets. A catalog can truthfully document an unsupported/deferred boundary without implementing every proposal; a failed claimed contract must be corrected or accurately dispositioned. Changing this 13-package baseline requires an explicit scope/status update. The plan is finite for the current inventory and does not promise a fixed number of future PRs or waive acceptance conditions.

## Verification and preservation

Fresh read-only harness runs retain **QT3 31,142/0/679** and **gated qt4 8,436/0/1,894**. All 2,573 skip records map unambiguously to pinned fixture cases. The released dependency filters were invoked directly for all 2,512 dependency-filtered cases; no filter was rewritten. Twenty-six isolated, 20-second-watchdog executor probes completed without timeout, outside gate accounting. Their applicability and outcomes are retained separately; they establish no gate promotion.

The current engine source is unchanged from published 1.26.3. Its 21,798 release solution passes and 84 integration LSP passes are inherited evidence; this documentation task claims no fresh full unit-test run. Current validation rebuilds both plan JSON files, checks exact partitions/counts/receipt/source hashes and links, and renders ten changed pages as HTML with unchanged Fytala CSS. No screenshot/PDF acceptance is claimed.

Original publication/candidate/diagnostic/Braid/upstream receipts remain untouched. No engine, test, fixture, dependency policy, gate/retry, version, kit, upstream or Braid change or new publication occurs. REQ-126 remains In Progress. The [current C05 validator](evidence/c05-collection-pattern-review/validate-evidence.py) checks progress and verifies original C00-C04 evidence bytes, historical plans and grammar snapshots in isolation. Original validators remain unchanged for their original checkpoint checkouts; they expect historical closure counts.
