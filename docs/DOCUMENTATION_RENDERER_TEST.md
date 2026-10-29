29 October 2026: reader-facing README/ROADMAP cleanup, concise implementation status, historical snapshot relocation and related ecosystem/commercial/navigation/style/handover pages rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/reader-docs-cleanup/render`. New local links, archived text retention, preserved FYTALA statement, active Schema licensing wording and whitespace checked. HTML only; no screenshot/PDF acceptance or new runtime qualification.

29 October 2026: Apache schema publication/checkpoint and canonical pages rendered as HTML using unchanged Fytala CSS under `.guard-tmp/schema-apache-core-render`. Public/private repository identities, Apache metadata, public 114-test/CI receipts, links and whitespace checked. HTML only; no screenshot/PDF or fresh complete core/consumer ABI acceptance.

29 October 2026: C44-R2 compatible-schema prototype, API proposal, qualification/owner gate and eight canonical/checkpoint pages rendered as HTML with markdown-it and unchanged Fytala CSS under `.guard-tmp/c44r2-compatibility-render`. Prior 141-file preservation, seven product hashes, compiled API diff, 123 correct controls/twelve detected mutants, BCL witness, source/receipt hashes, JSON, new links and authored whitespace checked. HTML only; no screenshot/PDF, real consumer ABI/provider or fresh full-engine acceptance.

29 October 2026: XDM 4.0 cross-system compatibility review and eight canonical/checkpoint pages rendered as HTML with markdown-it and unchanged Fytala CSS under `.guard-tmp/xdm40-compatibility-render`. Immutable source/blob hashes, 58 selected source-file hashes, seven C42 binary matches, 62 diagnostic observations, local links and whitespace verified. HTML render check only; no screenshot/PDF acceptance or fresh complete engine/consumer test claim.

**REQ-126 C05 complete — 26 October 2026:** [Type/collection/pattern review](xpath40-catalog/TYPE-COLLECTION-PATTERN-RECONCILIATION.md) reconciles nine type/collection/pattern rows and 49 selected production names. All 2,457 focused tests pass. Of 275 independent controls, 259 match; 16 retain choice-coercion, record-destructuring and known generic-model gaps. Delivered union/EQName fixes remain closed. Eight closure workpackages remain; C06 is next. No engine or executable example changes, acceptance waiver, profile/gate promotion or release. Skips, Braid and extension work stay deferred; earlier checkpoints are historical.


**K02 guarded loader diagnostics — 27 October 2026:** [Implemented guarded mode](conformance/k02-schema-loader/README.md) delegates eligible schemas to actual query imports only when explicitly enabled for diagnostics; normal eager loading stays the default. All 66 actual-harness diagnostic runs match independent expectations, including wrong-assertion failures and preserved source/profile boundaries. All 21,825 solution tests and both normal gates pass at 31,144/0/677 and 8,436/0/1,894; all 677 QT3 skips/reasons are unchanged. Candidate 1.26.6 is unpublished. K02 profile/capability and wider applicability work remain open; no admission or baseline change. Earlier checkpoints are historical.


**K02 bounded diagnostic correction — 27 October 2026:** [Duration-schema investigation](conformance/k02-schema-duration/README.md) reproduces the unavailable schema duration types and corrects the public aggregated schema-import error to XQST0012. All 9 regressions pass (baseline 4 failed/5 passed). The original public query reaches its declared error alternative, but the QT3 environment still fails before query execution; validateexpr-28 remains skipped. No XSD 1.1 capability, skip, fixture or baseline change. Candidate 1.26.5 is unpublished; public 1.26.3 and REQ-126 acceptance are unchanged. Earlier checkpoints are historical.


**K01 complete; approved QT3 baseline — 27 October 2026:** [Exact two-case adoption](conformance/k01-schema-list/BASELINE-ADOPTION.md) removes only CastAs-ListType-20 and -28 from known skips. The fresh full QT3 gate passes **31,144/0/677**; qt4 remains 8,436/0/1,894. The owner approval and original receipts are preserved; no fixture/assertion/filter change. Source candidate 1.26.4 remains unpublished and REQ-126 remains Complete. Earlier totals and pending-approval notices are historical.


**K01 schema-list correction — 27 October 2026:** [Correction and evidence](conformance/k01-schema-list/README.md) fixes legacy typed bindings reporting XPTY0004 instead of XPST0051 for invalid list item types. All 18 regressions pass (baseline 6 failed/12 passed); direct original QT3 cases CastAs-ListType-20 and -28 now both pass. Both skips and frozen totals stay unchanged pending a separate baseline-policy approval. Source candidate 1.26.4 is unpublished; public 1.26.3 and REQ-126 bounded acceptance are unchanged. Earlier checkpoints are historical.


**REQ-126 complete for the accepted bounded catalog — 26 October 2026:** [Owner acceptance](xpath40-catalog/CATALOG-ACCEPTANCE.md) closes C13 and all 13 workpackages; zero remain open. AC-01 through AC-04 are explicitly accepted for 76 entries/235 bounded contract groups; AC-05 maintained. All 38 catalog dispositions are accepted while all 48 unmet batch observations and original technical exit criteria remain unchanged. No full current-draft compliance, engine fix, gate change or release follows. Earlier open-state notices are historical.


**REQ-126 C13 review prepared — 26 October 2026:** [Final closure review](xpath40-catalog/FINAL-CLOSURE-REVIEW.md) consolidates 76 entries, 235 bounded contract groups and 38 proposed disposition records. All 3,172 fresh documentation tests pass. Six Known Uses sections now state their evidence/gate boundary directly. The [acceptance proposal](xpath40-catalog/C13-ACCEPTANCE-PROPOSAL.md) would close the documentation catalog with explicit limits and open technical follow-ups; it is not approved. All 48 unmet batch observations remain unmet. C13 remains open, AC-01 through AC-04 unchecked and REQ-126 In Progress; no engine, fixture, gate, version or release change. Earlier checkpoints are historical.


**REQ-126 C12 complete — 26 October 2026:** [Final semantic/evidence batch](xpath40-catalog/SEMANTIC-REVIEW-BATCH-4.md) reviews 19 entries/57 contract groups with 369 exact catalog cases and six dedicated walkthroughs. All 5,595 fresh tests pass; independent diagnostics retain 167 matching and eight unmet controls. Choice coercion and switch comparison remain explicit blockers. All 76 entries now have C09-C12 bounded reviews. One workpackage remains: C13 blocker disposition and owner acceptance. REQ-126 stays In Progress; no engine, fixture, profile, gate or release change. Earlier checkpoints are historical.

**REQ-126 C11 complete — 26 October 2026:** [Third semantic/evidence batch](xpath40-catalog/SEMANTIC-REVIEW-BATCH-3.md) reviews 19 entries/56 contract groups and 874 exact linked cases. All 3,756 fresh tests pass; all 26 sample blocks have verification paths, including seven independently executed inline queries. Of 99 supplemental observations, 77 controls match, 20 retain known requirements and two record URI type behavior. Destructuring, spelling, focus and epoch-profile caveats remain explicit. Two workpackages remain; C12 is next. No final acceptance, engine, fixture, profile, gate or release change; earlier checkpoints are historical.

**REQ-126 C10 complete — 26 October 2026:** [Second semantic/evidence batch](xpath40-catalog/SEMANTIC-REVIEW-BATCH-2.md) reviews 19 entries/55 contract groups with 892 exact linked cases and 82 sample blocks. All 3,469 fresh tests pass. Supplemental observations retain 60 matching controls, 10 known draft mismatches, two disputed highest-example controls and two URI observations. The source/example reading issue stays separate from an engine-defect diagnosis. Three packages remain; C11 is next. No final acceptance, engine, fixture, profile, gate or release change; earlier checkpoints are historical.

**REQ-126 C09 complete — 26 October 2026:** [First semantic/evidence batch](xpath40-catalog/SEMANTIC-REVIEW-BATCH-1.md) reviews 19 entries/67 contract groups and 536 distinct linked cases. All 3,447 fresh existing tests pass; 116 sample +8 walkthrough blocks have explicit verification paths. Of 54 independent profile controls, 46 match and eight retain known model/extrema/partial-apply boundaries. Scan sample queries are separated and executed verbatim. Four closure workpackages remain; C10 is next. No final acceptance, engine, fixture, profile, gate or release change; earlier checkpoints are historical.

**REQ-126 C08 complete — 26 October 2026:** [Historical adoption research](xpath40-catalog/ADOPTION-HISTORY-RESEARCH.md) checks all 28 original records through 16 delivery trees, 16 retained PR heads, 58 matching historical commits and 16 PR records. No exact adoption pin was recovered. The owner explicitly approved 26 legacy provenance exceptions and two non-adoption dispositions, retaining all original nulls. C08 is complete; five packages remain open, with C09 next. Earlier receipts, defects and semantic acceptance remain open. No engine, profile, gate or release change; earlier checkpoints are historical.

**REQ-126 C07 complete — 26 October 2026:** [Lifecycle/disposition review](xpath40-catalog/LIFECYCLE-RECONCILIATION.md) reconciles all 76 classifications and 19 original/wider decision groups. All 3,519 fresh existing tests and 113 profile controls pass. Current compound-pattern wording, method-call boundaries and a malformed delivery link are corrected with original receipts retained. Six closure workpackages remain; C08 historical adoption evidence is next. All 28 missing adoption pins, earlier defects and semantic acceptance remain open. No feature/profile/gate promotion or release; skips, Braid and extension work stay deferred. Earlier checkpoints are historical.

**REQ-126 C06 complete — 26 October 2026:** [Host/profile review](xpath40-catalog/HOST-PROFILE-RECONCILIATION.md) reconciles the final three grammar rows; all 18 now have bounded reviews, not semantic acceptance. All 376 focused tests and 3,148 catalog examples pass. Of 154 probe observations, 133 controls match, two retain a switch comparison mismatch, and 19 record context boundaries. Seven closure workpackages remain; C07 is next. Static-mode and loader/adoption limits remain explicit. No engine, profile, gate or release change; skips, Braid and extension work stay deferred. Earlier checkpoints are historical.

26 October 2026: C05 report, 21 scoped/linked pages and twelve canonical/audit pages rendered as HTML with unchanged Fytala CSS. Scoped/new links, source/production hashes and independent unmet controls checked; no screenshot/PDF acceptance.

**REQ-126 C04 complete — 26 October 2026:** [Expression/call/focus review](xpath40-catalog/EXPRESSION-CALL-FOCUS-RECONCILIATION.md) reconciles six expression/call/focus rows and 33 selected production/token names. All 880 focused tests pass; 209 controls match, ten retain current-draft spelling/placeholder/mapping-focus gaps, and eight late-binding observations remain separate. Nine closure workpackages remain; C05 is next. No engine or executable example changes, gate/profile promotion, acceptance waiver or release. C02's atomic-annotation defect remains open. Skips, Braid and extension work stay deferred; earlier checkpoint counts are historical.

26 October 2026: C04 report, eleven scoped pages and twelve canonical/audit pages rendered as HTML using unchanged Fytala CSS. Scoped/new links, production mappings, source/evidence hashes and retained unmet controls checked. No screenshot/PDF acceptance.

**REQ-126 C03 complete — 26 October 2026:** [Constructor/type review](xpath40-catalog/CONSTRUCTOR-TYPE-RECONCILIATION.md) reconciles eight entries and all 49 focus constructors. All 1,333 asserted controls match across 1,357 observations; 245 focused schema/API/host tests pass. Bounded nominal defaults/static fields are now evidenced. Generic wide casts, decimal precision, schema/host context and broader conversion remain explicit boundaries; C02's atomic-annotation defect remains open. Ten closure workpackages remain; C04 is next. No engine/test/example/version/gate change or acceptance waiver; skips/Braid/extension work stays deferred. Earlier checkpoint counts are historical.

26 October 2026: the C03 report, eight entries and fifteen canonical/audit/example/declaration pages rendered as HTML with unchanged Fytala CSS. Local links, retained controls and exact source/evidence hashes checked; no screenshot/PDF acceptance.

**REQ-126 C02 complete — 26 October 2026:** [Function/options reconciliation](xpath40-catalog/FUNCTION-OPTIONS-RECONCILIATION.md) reconciles 30 entries/74 function surfaces with explicit supported boundaries and 14 follow-up groups. Release build and all 21,798 solution tests pass; all 3,148 catalog examples pass separately. The 195 added profile observations include 118 matching controls, two unmet controls for the new `atomic-type-annotation(())` defect, and 75 diagnostics. Eleven closure workpackages remain; C03 is next. C02 closes review scope, not AC-01–04 or the retained defects. Skip fixes, Braid and extension work remain deferred. Original receipts and frozen gates are preserved; no engine/version/release change.

26 October 2026: the C02 report, thirty function entries and twelve canonical/audit pages are rendered as HTML with unchanged Fytala CSS. This checks HTML production and local links; no screenshot/PDF acceptance is claimed.

**REQ-126 C01 complete — 26 October 2026:** [Current inventory and ownership record](xpath40-catalog/FUNCTION-INVENTORY-REFRESH.md) refreshes 316 names against published 1.26.3 and pinned current draft b4723ff5: 119 registered 4.0-only names (70 non-constructors, 49 focus constructors), plus 197 existing names. The retained inventory missed `array:members` and `array:of-members`. All 1,581 profile lookup observations completed without probe errors; four changed declarations and remaining semantic/default/empty/error reviews have explicit C02/C03/C06 owners. Twelve closure workpackages remain open; REQ-126 and AC-01–04 remain partial. Finish these packages before skip fixes, Braid or VS Code extension work. Engine, APIs, tests, version and frozen gates are unchanged; existing passing source/gate evidence is inherited. Earlier counts and checkpoint notices are historical.

26 October 2026: ten REQ-126 closure-plan/skip-triage/canonical/catalog pages rendered as HTML with markdown-it and unchanged Fytala CSS. Thirteen workpackage scopes, four exact 19-entry batches, 18 grammar rows, 28 historical-pin gaps, 2,573 skip cases, actual filters, 26 diagnostic receipts, raw/source/index hashes and links verified. Gate counts/policy unchanged; no fresh full-unit, semantic acceptance, screenshot/PDF, Braid or publication claim.

26 October 2026: eleven 1.26.3 publication/release-note/canonical/catalog pages rendered as HTML with markdown-it and unchanged Fytala CSS. Tag/workflow/public signatures/cache/output/consumer hashes and links verified; original candidate evidence retained. No screenshot/PDF or new Braid acceptance inferred.

26 October 2026: eleven 1.26.3 candidate/release-note/canonical/catalog/audit pages rendered as HTML with markdown-it and unchanged Fytala CSS. Pinned source/tree, package/cache/output/assembly identities, raw receipt/consumer-source hashes, counts, links and diff verified. Original Braid/earlier evidence retained; no screenshot/PDF, signed-public verification or 1.26.3 Braid adoption inferred.

26 October 2026: nine floor/ceiling contract-audit/canonical/catalog pages rendered as HTML with markdown-it and unchanged Fytala CSS. Unchanged-engine source identity, new test/input/receipt/index hashes, required-error controls, links and diff verified. No correction tally, screenshot/PDF, new Braid acceptance or 1.26.3 publication.

26 October 2026: nine floor/ceiling integer/canonical/catalog pages rendered as HTML with markdown-it and unchanged Fytala CSS. Baseline/source/test/receipt/index hashes, numeric identity/sign predicates, links and diff verified; original abs/Braid snapshot retained. No screenshot/PDF, new Braid acceptance or 1.26.3 publication.

26 October 2026: nine abs-integer/adoption/canonical/catalog pages rendered as HTML with markdown-it and unchanged Fytala CSS. Baseline/source/test/receipt/index identities, supplied Braid report snapshot, numeric type/sign/error predicates, links and diff verified. No screenshot/PDF, new Braid native acceptance or 1.26.3 publication claimed.

26 October 2026: eleven 1.26.2 publication/release-note/canonical/catalog pages rendered as HTML with markdown-it and unchanged Fytala CSS. Tag/workflow/public signatures/cache/output/consumer hashes and links verified; original candidate evidence retained. No screenshot/PDF or new Braid acceptance inferred.

26 October 2026: eleven 1.26.2 candidate/release-note/canonical/catalog/kernel pages rendered as HTML with markdown-it and unchanged Fytala CSS. Pinned-source tree, archive/cache/output/assembly provenance, raw receipt and consumer-source hashes, case counts, links and diff verified; no screenshot/PDF, signed public package or Braid outcome inferred.

26 October 2026: ten exact-kernel/canonical/catalog/corrected historical-guide pages rendered as HTML with markdown-it and unchanged Fytala CSS. Source/receipt/index hashes, independent oracle corrections, test counters, local links and diff verified; no screenshot/PDF or Braid outcome inferred.

26 October 2026: nine extreme-rounding precision audit/correction/canonical/catalog pages rendered as HTML with markdown-it and unchanged Fytala CSS. Corrected/draft receipt hashes, pinned baseline source, watchdog and retained-kernel findings, links and diff checked; no screenshot/PDF or Braid acceptance.

26 October 2026: eleven 1.26.1 publication/release-note/canonical/catalog pages rendered as HTML with markdown-it and unchanged Fytala CSS. Tag/workflow/signed-public/cache/output/consumer receipt hashes and links verified; original candidate evidence retained. No screenshot/PDF or Braid native acceptance.

26 October 2026: ten 1.26.1 candidate/release-note/canonical/catalog pages rendered as HTML with markdown-it and unchanged Fytala CSS. Immutable source, package/cache/output/assembly provenance, raw evidence hashes, consumer sources/receipts, local links and diff checked; no screenshot/PDF or signed/public/native acceptance.

26 October 2026: nine rounding-function correction/canonical/catalog pages rendered as HTML with markdown-it and unchanged Fytala CSS. Receipt/source hashes, local links and diff checked; no screenshot/PDF acceptance. Original abs/rounding evidence bytes are protected from line-ending conversion.

25 October 2026: atomic-function conversion audit/correction and canonical status pages rendered as HTML with markdown-it and unchanged Fytala CSS. Local links, source/receipt hashes and diff checked; no screenshot/PDF acceptance.

25 October 2026: five restart/upstream/handover checkpoint pages rendered as HTML with unchanged Fytala CSS. Receipt hashes, upstream heads and engine/test identity verified; no screenshot/PDF acceptance.

25 October 2026: twelve REQ-126 numeric binary/range and REQ-138 acceptance status pages rendered as HTML with markdown-it and unchanged Fytala CSS; source/evidence hashes, links and diff checked. No screenshot/PDF acceptance.

25 October 2026: nine REQ-126 singleton-array correction/canonical/catalog pages rendered as HTML with markdown-it and unchanged Fytala CSS. Source/receipt hashes, links and diff checked; no screenshot/PDF acceptance.

25 October 2026: nine REQ-126 recursive-array audit/canonical/catalog pages rendered as HTML with markdown-it and unchanged Fytala CSS. Receipt/source hashes, links and diff checked; no screenshot/PDF acceptance.

25 October 2026: nine REQ-126 empty unary atomization/canonical/catalog pages rendered as HTML with markdown-it and unchanged Fytala CSS. Receipt/source hashes, links and diff checked; no screenshot/PDF acceptance.

25 October 2026: REQ-126 unary conversion guide and updated canonical/catalog/history pages rendered as HTML with markdown-it and unchanged Fytala CSS. Receipt hashes, local links and diff checked; no screenshot/PDF acceptance.

25 October 2026: twelve 1.26.0 publication/release/API/canonical/catalog pages rendered as HTML using markdown-it and unchanged Fytala CSS; receipt hashes and local links checked. No screenshot/PDF acceptance.

25 October 2026: 1.26.0 candidate notes/proof and canonical/catalog status pages rendered as HTML with markdown-it and unchanged Fytala CSS. Exact merge/source tree, test/package/assembly/cache/audit hashes, new links/JSON and unchanged engine/test/kit/fixture boundaries verified. HTML only; no screenshot/PDF or signed/native acceptance claimed.

25 October 2026: REQ-138 consumer implementation review, documentation-only registry/punctuation corrections and canonical status pages rendered with unchanged Fytala CSS. Exact reviewed engine/test/version/design bytes and independent consumer TRX hashes/counters verified. HTML only; no signed-package or native acceptance claimed.

25 October 2026: REQ-138 implementation/API guide, retained consumer design review and canonical status pages rendered as HTML with markdown-it and unchanged Fytala CSS under `.guard-tmp/req138-implementation-render`. Exact design/request bytes, new relative links, JSON/receipt hashes, source headers/XML-doc build and unchanged kit/fixture boundaries checked. HTML only; no screenshot/PDF or consumer runtime acceptance.

25 October 2026: REQ-138 maintained design, original registration and canonical status pages rendered as HTML with markdown-it and unchanged Fytala CSS. Original Braid documents retained byte-exact; their relative citations retain Braid context. Links/JSON and unchanged engine/kit checked. HTML only; no screenshot/PDF acceptance.

25 October 2026: 1.25.2 release-candidate notes/proof and canonical/catalog status pages rendered as HTML with markdown-it and unchanged Fytala CSS. Pinned source, test/package proof, links/JSON and unchanged engine/assets checked. HTML only; no screenshot/PDF acceptance.

25 October 2026: exact idiv correction and canonical/catalog status pages rendered as HTML with markdown-it and unchanged Fytala CSS. New links/JSON, oracle comparisons and unchanged fixture/kit boundaries checked. HTML only; no screenshot/PDF acceptance.

25 October 2026: idiv precision/range investigation and canonical/catalog status pages rendered as HTML with markdown-it and unchanged Fytala CSS. New links/JSON, exact engine comparison and oracle receipts checked. HTML only; no screenshot/PDF acceptance.

25 October 2026: integer modulo correction, canonical status and catalog evidence pages rendered as HTML with markdown-it and unchanged Fytala CSS. New links/JSON and unchanged gate/fixture/kit boundaries verified. HTML only; no screenshot/PDF acceptance.

25 October 2026: division/modulo audit and canonical/catalog status pages rendered as HTML with markdown-it and unchanged Fytala CSS. New links/JSON and exact unchanged engine boundaries verified. HTML only; no screenshot/PDF acceptance.

25 October 2026: integer unary boundary correction, canonical status and catalog status pages rendered as HTML with markdown-it and unchanged Fytala CSS. New links/JSON and engine/gate/kit boundaries checked. HTML only; no screenshot/PDF acceptance.

24 October 2026: 1.25.1 publication, release notes, retained candidate checkpoint, README and canonical status pages rendered as HTML with markdown-it and unchanged Fytala CSS. JSON/public receipts and links checked; no screenshot/PDF acceptance.

24 October 2026: Unix timestamp contract, timestamp entry and synchronized canonical/catalog status pages rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-unix-contract-render`. New links/JSON, receipt hashes, 16-field/count audit and source header/XML docs checked. HTML only; no screenshot/PDF acceptance. Engine/kit/fixtures/version unchanged.

24 October 2026: 1.25.0 release notes, verified publication, exact consumer review, README and canonical status pages rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/release1250-render`. New links/JSON, byte-exact review, package/receipt proofs and unchanged engine/kit/fixtures checked. HTML only; no screenshot/PDF acceptance.

24 October 2026: REQ-137 implementation/API guide, immutable historical request/design/review and canonical status pages rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req137-implementation-render`. New links/JSON, raw receipt hashes, source headers/XML documentation and unchanged kit/fixtures/gates checked. HTML only; no screenshot/PDF acceptance.

24 October 2026: REQ-137 consumer design review and synchronized status pages rendered with unchanged Fytala CSS. Original review retained byte-exactly; reviewed proposal/request Git-blob hashes verified. HTML only; owner implementation approval pending.

24 October 2026: REQ-137 request, proposed scope/API and canonical planning counterparts rendered with markdown-it and unchanged Fytala CSS under .guard-tmp/req137-design-render. HTML only; no screenshot/PDF acceptance or runtime change.

<p align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="160" alt="Fytala documentation renderer test">
</p>

# Documentation Renderer Test

**Restart / next-action correction (2026-10-29):** [XDM 4.0 compatibility review precedes C45](XDM40-COMPATIBILITY-REVIEW-PLAN.md). The owner answered the pending approval request with a request to assess XmlDb, Bosak.Schema and Braid; **C45 production/API acceptance is still pending**. Review real consumer source/binary/persistence/behavior compatibility and pin the draft before semantic adoption. C44 at `affc228a` and all original evidence remain unchanged; 24,203 solution passes and exact 31,397/0/424 /8,941/0/1,918 gates are inherited. Candidate 1.33.1 remains unpublished/public 1.26.3; no implementation, baseline or release approval is inferred.

**K04-C44 isolated schema prototype (2026-10-29):** [110 contract checks and compiled API/ABI proposal](conformance/k04-schema-session-prototype/README.md) qualify fake-session ownership, multi-head member sets, node/atomic provenance, copies, closure keys, cancellation and lifetime. Four bad variants fail targeted controls; the unchanged synthetic old consumer passes 25/55 checks across library replacement. The existing-slot carrier stays 48 bytes versus 56 with an added reference; 24 neutral proposed types compile in isolation. Explicit host-opened sessions captured at compile time and a guarded C45 production candidate are proposed; **owner acceptance pending**. Production ABI, provider coverage and API freeze are not certified. Product source/binaries remain unchanged; C42's **24,203 solution /39 regression passes** and C43's **31,397/0/424** / **8,941/0/1,918** exact gates/maps (230 sets) are inherited. Bosak.Schema XSD 1.1 ownership, pending REQ-139, REQ-126 bounded closure/48 observations remain. No API/filter/baseline/version/release change; candidate **1.33.1** unpublished/public **1.26.3**.

**K04-C43 schema compatibility proposal (2026-10-29):** [Optional schema-session counterproposal and 14 acceptance groups](conformance/k04-schema-advisor-proposal/README.md) distinguish component assessment and node/atomic identity from a typing-only callback. Current source does not establish multi-affiliation/complete complex derivation; owned unsupported coverage must refuse rather than downgrade to 1.0. Six QT3 preparation exceptions and 30 documented XSLT guards are reproduced; validation-1301 additionally reaches its inline CTA guard, correcting historical unreachable prose. catalog-005b remains a separate applicability skip. Fresh gates/maps stay **31,397/0/424** and **8,941/0/1,918** (230 sets); C42's **24,203 solution /39 regression passes** are inherited with source/binaries unchanged. Next C44 isolated identity/session prototype before permanent API acceptance. Bosak.Schema ownership, proposed ADR-006/pending REQ-139, REQ-126 bounded closure and 48 historical observations remain. No product/API/filter/baseline/version/release change; candidate **1.33.1** unpublished/public **1.26.3**.

**K04-C42 schema-element repair (2026-10-29):** [Actual substitution membership and member-set subtyping repaired](conformance/k04-schema-element-fix/README.md). Abstract candidate, blocking, candidate typing/nillability and named/inline function regressions pass: **39 tests**, previously 17 failing on preserved binaries. All 11 independent controls pass; only four direct originals change, leaving the older-profile cohort at 19 passes/18 intentional counter-profile failures, nine companions passing and six separate preparation exceptions. Fresh **24,203 solution tests** and unchanged normal gates/maps **31,397/0/424** and **8,941/0/1,918** pass (230 sets, 11 experimental passes). Filters, fixtures, assertions and baselines remain unchanged. Candidate **1.33.1** unpublished/public **1.26.3**; next C43 concrete schema-advisor compatibility proposal. Bosak.Schema XSD 1.1 ownership, proposed ADR-006/pending REQ-139, REQ-126 bounded closure and 48 historical observations remain. No release is included.

**K04-C41 schema-profile review (2026-10-29):** [37 legacy exclusions and six preparation errors reconciled](conformance/k04-schema-profile-review/README.md). Direct legacy results are 15 passes/22 failures; all nine exact-query XSD 1.1 companions pass. Six unchanged multi-affiliation schemas fail during preparation. Independent XPath 3.1 controls over valid XSD 1.0 inputs reproduce four abstract/blocked membership mismatches plus one function-subtype mismatch; six positive/edge controls pass. Next: C42 existing schema-element contract repair, retaining normal profile guards. Fresh gates/maps remain **31,397/0/424** and **8,941/0/1,918** (230 sets); C40's 24,164 solution passes are inherited. Current Schema documents, actual core API surfaces and stale draft baseline references are pinned; Bosak.Schema ownership and proposed ADR-006/pending REQ-139 remain unchanged. No source/filter/baseline/version/release change; candidate **1.33.0** unpublished/public **1.26.3**. REQ-126 bounded closure and 48 historical observations remain.

**K04-C40 xib admission adopted (2026-10-28):** [Exact owner-approved adoption in both suites verified](conformance/k04-xib-admission/adoption/BASELINE-ADOPTION.md). Fresh normal gates pass **31,397/0/424** and **8,941/0/1,918**: only format-date-en151 and format-dateTime-en151 leave each skip map, with every retained name/reason, all 230 qt4 gated sets and 11 experimental passes unchanged. All **24,164 solution tests** pass. Both rebuilt catalogs (78,806 cases) and all 854 actual filter requests/1,281 profile outcomes match qualification; four original assertions and 12 public controls pass. Only the two approved canonical xib rules/history rows and AGENTS baseline change; engine source, other dependency policies, fixtures and assertions remain intact. Candidate **1.33.0** unpublished/public **1.26.3**. Next: C41 read-only reconciliation of 37 legacy XSD-profile exclusions and six separate schema-loader errors, retaining Bosak.Schema ownership and pending ADR-006/REQ-139. REQ-126 bounded closure and 48 historical observations remain. No release is included.

**K04-C40 xib admission qualified (2026-10-28):** [Exact three-file baseline proposal](conformance/k04-xib-admission/BASELINE-UPDATE-PROPOSAL.md) is ready; **owner approval pending**. Both complete catalogs (78,806 cases) show only two changed admissions per suite: format-date-en151 and format-dateTime-en151. All four original assertions pass under both executors. Isolated gates pass **31,397/0/424** and **8,941/0/1,918**, with every retained name/reason, 230 qt4 gated sets and 11 experimental passes unchanged. The actual-filter matrix verifies canonical xib polarity and retained conjunction policy (854 requests/1,281 profile outcomes per phase); 12 public controls pass and five counter-profile failures remain excluded. Fresh normal gates stay **31,395/0/426** and **8,939/0/1,920**; 24,164 solution passes are inherited. No production/filter/baseline/version/release change. Candidate **1.33.0** unpublished/public **1.26.3**; Bosak.Schema ownership, pending ADR-006/REQ-139, REQ-126 bounded closure and 48 historical observations remain.

**K04-C39 independent-profile review (2026-10-28):** [All 370 dependency exclusions dispositioned](conformance/k04-independent-profile-review/README.md) across complete QT3/qt4 inventories (78,806 cases). The ledger distinguishes 165 older-spec, 93 unsupported optional-feature, 26 negative frozen-feature, 39 XML-version, 37 legacy XSD-profile, five Unicode-version, two implemented-normalization negative, one default-language and two xib-language candidate cases; 371 blocker occurrences include one overlapping case. All four original xib fallback assertions and 12 public controls pass; five counter-profile assertion failures remain preserved. Fresh normal gates/maps stay **31,395/0/426** and **8,939/0/1,920** (230 sets); C38's 24,164 solution passes are inherited. Next: C40 exact xib metadata qualification in both filters. Current Bosak.Schema documents are pinned; XSD 1.1 ownership, proposed ADR-006/pending REQ-139 and six separate schema harness errors remain unchanged. No source/filter/baseline/version/release change; candidate **1.33.0** unpublished/public **1.26.3**. REQ-126 bounded closure and 48 historical observations remain.

**K04-C38 pragma admission adopted (2026-10-28):** [Exact owner-approved 55-case adoption verified](conformance/k04-pragma-admission/adoption/BASELINE-ADOPTION.md). Fresh normal QT3 is **31,395/0/426**, with exactly 55 removals and every retained name/reason matching qualification, including both exact-XQ10 pragma exclusions. Fresh qt4 remains **8,939/0/1,920** (230 sets, 11 experimental passes). All **24,164 solution tests** pass; rebuilt normal routing matches all 31,821 cases and 16 lexical controls. Only the approved TestExecutor refusal/history/summary and AGENTS baseline change; independent dependency policy, engine source, fixtures and assertions remain intact. The bounded grammar refusal set is empty; all 370 remaining Unsupported dependency skips are independently excluded under both profiles. Candidate **1.33.0** unpublished/public **1.26.3**. Next: C39 read-only dependency-profile review. Bosak.Schema owns XSD 1.1, ADR-006/REQ-139 remains pending, and REQ-126 bounded closure/48 historical observations remain. No release is included.

**K04-C38 pragma admission qualified (2026-10-28):** [Exact two-file baseline proposal](conformance/k04-pragma-admission/BASELINE-UPDATE-PROPOSAL.md) is ready; **owner approval pending**. Complete inventory covers 31,821 cases and 57 changed predicates; all 55 eligible original assertions pass in the isolated **31,395/0/426** gate, with all 426 retained names/reasons and two exact-XQ10 exclusions unchanged. The older no-namespace direct expectation remains explicitly excluded. All 24 public controls and copy/context/construction originals (49/43/87) pass under the required executors; 16 lexical controls match each. Fresh normal gates remain **31,340/0/481** and **8,939/0/1,920** (230 sets, 11 experimental passes). All 567 processes are bounded; C37's 24,164 solution passes are inherited. Production, baseline, fixtures and version are unchanged. Candidate **1.33.0** unpublished/public **1.26.3**; Bosak.Schema ownership, pending ADR-006/REQ-139, REQ-126 bounded closure and 48 historical observations remain. No release is included.

**K04-C37 pragma fallback implemented (2026-10-28):** [Raw pragma lexing and statically validated fallback](conformance/k04-pragma-fallback/README.md) recognize no pragma namespaces; well-formed pragmas are ignored, with XQST0079 for missing fallback and XPST0081 for unbound prefixes. Raw contents, declaring-module/constructor scopes and XPath 3.1/4.0 compatibility are covered. Fresh **24,164 solution tests**, **82 regressions** and **24 public controls** pass; all 55 eligible originals pass in the isolated **31,395/0/426** gate. One old exact-XQ10 no-namespace expectation remains an explicitly excluded direct failure. Normal gates/maps remain **31,340/0/481** and **8,939/0/1,920** (230 sets, 11 experimental passes). Normal guards, fixtures, assertions and baseline are unchanged. Candidate **1.33.0** unpublished/public **1.26.3**. Next: C38 separate routing admission qualification. Bosak.Schema owns XSD 1.1, ADR-006/REQ-139 remains pending, and REQ-126 bounded closure/48 historical observations remain.

**K04-C36 remaining-skip/pragma review (2026-10-28):** [All 481 retained skips classified](conformance/k04-pragma-review/README.md): 425 dependency skips partition into 55 routing-only, two routing-plus-independent and 368 independent exclusions; 40 unattended exclusions, ten range/platform/external limits and six existing schema harness errors remain separate. Complete inventory covers 31,821 cases; 57 pragma predicates change, with 60 originals across four sets. Isolated gate **31,348/47/426** (exit 2) does not qualify admission; direct originals give 11 passes/49 failures and public controls eight passes/16 failures. Fresh normal gates and maps remain **31,340/0/481** and **8,939/0/1,920** (230 sets, 11 experimental passes). All 209 final processes are bounded; adopted C35's 24,082 solution passes are inherited. Next: C37 raw pragma lexing and ignored-pragma fallback, retaining normal guards. Source, fixtures, baseline and version remain unchanged; candidate **1.32.0** unpublished/public **1.26.3**. Bosak.Schema owns XSD 1.1, ADR-006/REQ-139 remains separately pending, and REQ-126 bounded closure/48 historical observations remain.

**K04-C35 construction admission adopted (2026-10-28):** [Exact owner-approved 83-case adoption verified](conformance/k04-construction-admission/adoption/BASELINE-ADOPTION.md). Fresh normal QT3 is **31,340/0/481**; the 83 named removals and every retained skip name/reason match qualification, including four independent exact-XQ10 exclusions. Fresh qt4 remains **8,939/0/1,920** (230 sets, 11 experimental passes). All **24,082 solution tests** pass; rebuilt normal routing matches all 31,821 cases and 19 lexical controls. Only the approved TestExecutor refusal/history and AGENTS baseline change; pragma/dependency guards, engine source, fixtures and assertions remain intact. Candidate **1.32.0** unpublished/public **1.26.3**. REQ-126 bounded closure and 48 historical observations remain; Bosak.Schema owns XSD 1.1 and ADR-006/REQ-139 remains separately pending. Next: C36 refresh remaining skips and review the pragma boundary. No release is included.

**K04-C35 construction admission qualified (2026-10-28):** [Exact two-file baseline proposal](conformance/k04-construction-admission/BASELINE-UPDATE-PROPOSAL.md) is ready; **owner approval pending**. All 31,821 cases were inventoried; 87 original assertions pass and exactly 83 eligible skips leave the isolated gate (**31,340/0/481**), with all 481 retained names/reasons and four exact-XQ10 exclusions unchanged. Copy/context assertions (49/43) pass under both executors; 19 lexical controls pass each. Fresh normal gates remain **31,257/0/564** and **8,939/0/1,920** (230 sets, 11 experimental passes). All 450 processes are bounded and successful; C34's 24,082 solution passes are inherited. No production, baseline, fixture, version or release change is applied. Candidate **1.32.0** unpublished/public **1.26.3**; Bosak.Schema XSD 1.1 ownership, separately pending ADR-006/REQ-139, REQ-126 bounded closure and 48 historical observations remain.

**K04-C34 construction implementation (2026-10-28):** [Construction modes and consistent typed copies](conformance/k04-construction-modes/README.md) implement explicit strip/preserve with default preserve, module-scoped IR policy, private copied schema metadata, nil/list/ID behavior and QName/NOTATION safeguards. Core adds nullable construction-policy inputs and copied-attribute source metadata; null direct hooks retain legacy behavior. Required anyType matching, validate-expression precedence and xml:id error mapping are repaired. Fresh **24,082 solution tests**, **73 regressions** and **87 original assertions** pass. Normal gates remain **31,257/0/564** and **8,939/0/1,920** (230 sets); isolated construction routing gives **31,340/0/481**, exactly 83 eligible removals with retained reasons unchanged. Candidate **1.32.0** unpublished/public **1.26.3**. Next: C35 separate admission qualification and owner-approved baseline proposal. REQ-126 bounded closure and 48 historical observations remain; Bosak.Schema owns XSD 1.1 and ADR-006/REQ-139 remains separately pending.

**K04-C33 construction review (2026-10-28):** [All 87 construction/mixed originals reviewed](conformance/k04-construction-mode-review/README.md) across ten sets. The isolated gate gives **31,261/79/481** (exit 2); admission is unqualified. A public typed-copy probe retains the root schema type but loses descendant/attribute types, IDREFS cardinality and nilled state across direct/computed/document copies. Source nodes are unchanged. Default construction is implementation-defined and needs an explicit compatibility policy. Fresh normal gates remain **31,257/0/564** and **8,939/0/1,920** (230 sets); production source, guards, fixtures, baselines and version are unchanged. C32 unit tests are inherited. Next: C34 construction-mode/default/metadata implementation. Bosak.Schema owns XSD 1.1; ADR-006/REQ-139 remains separately proposed/pending. Candidate **1.31.0** unpublished/public **1.26.3**; REQ-126 bounded closure and 48 historical observations remain.

**K04-C32 copy-admission adopted (2026-10-28):** [Exact owner-approved 49-case adoption verified](conformance/k04-copy-namespaces-admission/adoption/BASELINE-ADOPTION.md). Fresh normal QT3 is **31,257/0/564**; every retained skip name/reason is unchanged. Fresh qt4 remains **8,939/0/1,920** (230 sets, 11 experimental passes). All **24,010 solution tests** pass, and rebuilt normal routing matches all 31,821 catalog cases plus 37 lexical controls. Construction, pragma and independent dependency guards remain intact; engine source, fixtures, assertions and version are unchanged. AGENTS.md records the approved baseline. Candidate **1.31.0** unpublished/public **1.26.3**. REQ-126 bounded closure and 48 historical observations remain; Bosak.Schema owns XSD 1.1. Next: separately review construction-mode boundaries.

**K04-C32 copy-admission review (2026-10-28):** [All 31,821 QT3 cases audited](conformance/k04-copy-namespaces-admission/README.md); exactly **49** copy-namespaces admissions qualify across five sets. The full isolated gate passes **31,257/0/564**, with every retained skip name/reason unchanged; all 49 original assertions and 37 lexical controls pass. All 87 construction/mixed predicates and 43 admitted context predicates retain policy. Fresh normal gates remain **31,208/0/613** and **8,939/0/1,920** (230 sets). The exact two-file baseline proposal awaits owner approval; normal source, fixtures, baselines and version are unchanged. C31 solution/focused tests are inherited. Candidate **1.31.0** unpublished/public **1.26.3**; REQ-126 bounded closure and 48 historical observations remain. Bosak.Schema owns XSD 1.1.

**K04-C31 namespace-copy implementation (2026-10-28):** [All four XQuery copy-namespaces modes implemented](conformance/k04-copy-namespaces/README.md), with module-scoped constructor IR, literal/enclosed-content distinction, source isolation and XQST0055 diagnostics. All **76 focused tests**, **24,010 solution tests** and **49 original copy-only cases** pass; the 43 admitted context results are unchanged. Normal gates remain exactly **31,208/0/613** and **8,939/0/1,920** (230 sets). The isolated copy-only gate is **31,257/0/564**, with every retained skip reason unchanged. Construction, normal guards, fixtures and baselines remain unchanged. Candidate **1.31.0** unpublished/public **1.26.3**. Next: separate C32 exact routing-admission review. REQ-126 bounded closure and 48 historical observations remain; Bosak.Schema owns XSD 1.1.

**K04-C30 prolog-mode review (2026-10-28):** [All 136 construction/copy-namespaces queries reviewed](conformance/k04-prolog-mode-review/README.md) across 13 sets: 75 construction-only, 49 copy-only, 12 mixed. Removing both refusals in isolation yields **31,214/126/481** (exit 2), so admission remains unqualified. Public controls expose missing valid-mode parsing and XQST0055/XQST0067 diagnostics; two declaration-free default namespace witnesses pass. Fresh normal gates remain **31,208/0/613** and **8,939/0/1,920** (230 sets). Source, guards, fixtures, baselines and version are unchanged; C29 solution tests are inherited. Next: C31 copy-namespaces implementation against the 49 copy-only originals, with separate later admission review. Candidate **1.30.1** unpublished/public **1.26.3**. REQ-126 bounded closure and 48 historical observations remain; Bosak.Schema owns XSD 1.1.

**K04-C29 context-routing adoption (2026-10-28):** [Exact owner-approved 43-case admission adopted](conformance/k04-context-routing-review/adoption/BASELINE-ADOPTION.md). Fresh normal QT3 is **31,208/0/613**; all 613 retained skip names/reasons are unchanged. Fresh qt4 remains **8,939/0/1,920** (230 sets, 11 experimental passes). All **23,934 solution tests** pass, and the rebuilt normal runner matches all 31,821 catalog routing decisions plus 30 lexical controls. Construction, copy-namespaces and pragma guards remain intact; fixtures, assertions, engine and version are unchanged. AGENTS.md records the approved baseline. Candidate **1.30.1** unpublished/public **1.26.3**. REQ-126 bounded closure and its 48 historical observations remain; Bosak.Schema owns XSD 1.1. Next: separately audit remaining unsupported-prolog cohorts.

**K04-C29 complete routing review (2026-10-28):** [All 31,821 QT3 cases audited](conformance/k04-context-routing-review/README.md); exactly **43** context-routing admissions are qualified (40 context cases and three variable-default cases). The complete isolated gate passes **31,208/0/613**, with all 613 retained skip names/reasons unchanged. Fresh normal gates remain **31,165/0/656** and **8,939/0/1,920** (230 sets). Thirty lexical controls and all 43 original assertions pass under the prototype. The exact two-file baseline proposal awaits owner approval; normal source, guards, fixtures, baselines, version and release remain unchanged. C28 unit tests are inherited. Candidate **1.30.1** unpublished/public **1.26.3**; REQ-126 bounded closure and 48 historical observations remain. Bosak.Schema owns XSD 1.1.

**K04-C28 library context correction (2026-10-28):** [Declaring-module namespace and type identity repaired](conformance/k04-library-context-fix/README.md); all imported constraints are retained, and dynamic target validation uses its prepared schema context. **112 focused tests** and **23,934 solution tests** pass. All 13 resource/module originals and 12 controls pass; isolated QT3 context set **44/0/0**. Normal gates remain **31,165/0/656** and **8,939/0/1,920** (230 sets). Guards, fixtures and baselines are unchanged. Candidate **1.30.1** unpublished/public **1.26.3**. Next: C29 complete routing/admission review before any baseline proposal. REQ-126 bounded closure and 48 historical unmet observations remain; Bosak.Schema retains XSD 1.1 ownership.

**K04-C27 resource/module audit (2026-10-28):** [All 13 remaining originals reviewed](conformance/k04-context-resource-review/README.md) through actual resource/module machinery: normal direct execution gives 4 passes/9 skips; isolated context routing gives 11 passes/2 failures. The complete isolated set is **42/2/0**, so admission is unqualified. Both failures lose the library prefix `u`; independent controls expose declaring-context/type-identity and built-in-alias issues (**8 passed/4 failed**). Normal context sets remain **4/0/40** and **0/0/53**. Source, guards, fixtures, baselines and version remain unchanged. C26's 23,822 solution passes and gates **31,165/0/656**, **8,939/0/1,920** (230 sets) are inherited, not fresh full runs. Candidate 1.30.0 unpublished/public 1.26.3. Next: C28 constraint-scope/identity correction before any guard proposal; Bosak.Schema retains XSD 1.1 ownership.

**K04-C26 main context initialization (2026-10-28):** [Bounded main-module implementation](conformance/k04-main-context/README.md) adds typed context values, external/default selection, singleton focus, global dependencies/cycles and host-focus restoration. All **52 new regressions**, **31 bounded original assertions**, 18 controls and five library controls pass. Fresh solution: **23,822 tests**. Normal admission is unchanged: context sets **4/0/40** and **0/0/53**, full gates **31,165/0/656** and **8,939/0/1,920**, 230 gated sets, identical skip mappings. Explicit-context sequence results are stabilized before focus restoration; queries without a declaration retain their prior result path. Candidate **1.30.0** unpublished/public **1.26.3**. Next: audit the 13 resource/module-bearing original boundaries before any guard or baseline proposal; Bosak.Schema owns staged XSD 1.1.

**K04-C25 duplicate context declaration corrected (2026-10-28):** [Narrow library parser fix](conformance/k04-context-duplicate-fix/README.md) reports XQST0099 for duplicate context declarations while retaining XQST0113 for library initializers/defaults and the dynamic-loader wrapper. All 24 new public regressions and **23,770 solution tests** pass. All five library probes pass; the 80 C24 original/control results and 97 filter assessments are unchanged, so main-module declaration support remains open. Fresh gates stay **31,165/0/656**, **8,939/0/1,920**, with 230 gated sets and identical skip names/reasons. Candidate **1.29.3** unpublished/public **1.26.3**. No routing, fixture, baseline, schema or release change. Next: main-module context declaration implementation.

**K04-C24 context declarations reviewed (2026-10-28):** [Bounded XQuery diagnosis](conformance/k04-context-declaration-review/README.md) inventories 97 originals and executes 31 resource-free QT3 cases: public XQuery gives 1 syntax-error pass/30 failures; the actual executor gives 1 pass/30 skips. Three baseline controls pass, while 15 declaration controls fail at parsing. Four of five library parser controls pass; duplicate declarations incorrectly report XQST0113 instead of XQST0099. Fresh normal sets remain **4/0/40** (QT3) and **0/0/53** (qt4). No guard/source/admission/baseline/version change. Next: narrow duplicate error correction before main-module implementation. C23's 23,746 solution passes and gates **31,165/0/656**, **8,939/0/1,920** (230 sets) are inherited, not fresh full-suite runs. Candidate 1.29.2 unpublished/public 1.26.3.

**K04-C23 normalization adopted (2026-10-28):** The owner-approved exact patch at ee19ebbf is applied. [Fresh actual normal receipts](conformance/k04-normalization-admission/adoption/BASELINE-ADOPTION.md) verify QT3 **31,165/0/656** (only repaired positive `cbcl-fn-normalize-unicode-006` leaves skips), qt4 **8,939/0/1,920** and **230 gated sets** (normalization adds 44 passes and six existing dependency skips). Complete normalization sets are **43/0/5** and **44/0/6**; all skip mappings match approved rehearsal exactly, including retained conditional negatives. AGENTS requires the new QT3 baseline. Fresh full solution: **23,746 passing tests**, including 8,196 XSLT tests. Candidate 1.29.2 unpublished/public 1.26.3; no engine, filter, fixture, schema or release change. Earlier proposal notices are historical.

**K04-C23 normalization admission review (2026-10-28):** [Exact three-file proposal](conformance/k04-normalization-admission/BASELINE-UPDATE-PROPOSAL.md) awaits owner approval; normal runners and AGENTS remain unchanged. Complete isolated gates pass QT3 **31,165/0/656** (only repaired positive `cbcl-fn-normalize-unicode-006` leaves skips) and qt4 **8,939/0/1,920**, promoting only the 44-pass normalization set with its six existing dependency exclusions (230 proposed gated sets). Every retained skip reason is identical; conditional negatives remain excluded. Normalization sets are **43/0/5** and **44/0/6**; no assertion is changed. C22's 23,746 solution passes are inherited source-identical evidence, not a fresh unit run. Candidate 1.29.2 unpublished/public 1.26.3; no engine, filter, fixture, schema or release change.

**K04-C22 normalization options (2026-10-28):** Candidate **1.29.2** applies XML-whitespace form-name normalization, adds NFC defaults for an explicit empty-sequence form in 4.0/Experimental and supplies `value`/`form` keyword metadata. XPath 3.1 retains its explicit-empty type error and keyword refusal; an empty-string form still disables normalization. All 246 new regressions and 62 review controls pass; original qt4 normalization is now **44/0/6**. Complete solution: **23,746 passing tests**. Frozen gates remain QT3 **31,164/0/657**, qt4 **8,895/0/1,914**, 229 gated sets, with identical skip names/reasons. No admission or release; public **1.26.3**. See [scope and receipts](conformance/k04-normalization-options-fix/README.md).

**K04-C21 ? FULLY-NORMALIZED conversion (2026-10-28):** Candidate **1.29.1** implements conversion with a pinned Unicode 17 NFC backend and first-scalar composing-character classification. The 135 new regression rows pass; all ten solution projects are green in aggregate (**23,500 tests**, with the Standard project rerun after an assertion-rendering correction). Unicode corpus and scalar audits report zero mismatches. Frozen QT3 **31,164/0/657** and qt4 **8,895/0/1,914** retain every skip name/reason and 229 gated sets. Other normalization forms retain the platform backend; form-name whitespace, 4.0 empty-form defaults and keyword metadata remain open. No skip adoption or release; public version remains **1.26.3**. See [implementation and evidence](conformance/k04-normalization-fix/README.md).

**K04-C20 normalization review ? 28 October 2026:** [Contract/capability diagnosis](conformance/k04-normalization-review/README.md) finds a FULLY-NORMALIZED conversion/classifier defect and confirms that the Bengali fixture's leading space is required; QT3 currently mislabels it as an upstream defect. Conditional unsupported-form negatives remain excluded. Eight original probes yield 2 passes/6 diagnostic failures; 62 controls give 33 passes/29 failures. Fresh normal sets are **42/0/6** (QT3) and **41/3/6** (qt4), separating conversion from 4.0 empty-form/keyword gaps. Next: adopted scalar-aware conversion before admission or known-skip removal. No engine, filter, fixture/assertion, version or baseline change. Inherited source-identical validation: 23,365 solution passes, QT3 **31,164/0/657**, qt4 gate **8,895/0/1,914**; no fresh full-suite claim. Candidate 1.29.0 unpublished/public 1.26.3; Bosak.Schema owns staged XSD 1.1 and REQ-126 remains Complete.

**K04-C19 optional date-formatting calls ? 28 October 2026:** [Typed 4.0 arities/keywords and qualified promotion](conformance/k04-date-optional-fix/README.md) implements arities 3/4 and keyword defaults for date/dateTime/time, preserving 3.1 arities 2/5 and refusal of 3/4. All 1,167 focused checks, eight originals and 96 review controls pass. Complete sets **135/0/3**, **79/0/15**, **94/0/4** qualify for promotion: qt4 now **229 sets, 8,895/0/1,914**, retaining all 1,892 previous skip reasons and adding only 22 existing exclusions from the promoted sets. QT3 stays **31,164/0/657**, with identical skip reasons. Twelve live catalog expectations now verify availability/arity while retaining 3.1 checks; historical REQ-126 records and all 48 unmet batch observations remain unchanged. All **23,365 solution tests** pass, including **8,196 XSLT tests**. Candidate **1.29.0** unpublished/public **1.26.3**; no frozen fixture/assertion, dependency filter, schema or release change. Bosak.Schema owns staged XSD 1.1; REQ-126 remains Complete. Next: retained FULLY-NORMALIZED contract review.

**K04-C18 empty date-formatting correction ? 28 October 2026:** [Shared helper and metadata fix](conformance/k04-date-empty-fix/README.md) returns a true empty sequence for empty values in existing arities 2/5 across all profiles/shared XSLT mode. All 729 focused checks pass (348 new regressions plus 381 calendar checks); exactly 30 C17 empty-value failures become passes, leaving 21 call-surface failures and all eight original missing-arity failures open. Fresh exploratory scopes remain **208/6/18** and **92/2/4**, with every failure and skip reason unchanged. Complete normal gates stay **31,164/0/657** and **8,587/0/1,892**. All **22,927 solution tests** pass, including **8,196 XSLT tests**. Candidate **1.28.2** unpublished/public **1.26.3**; no catalog fixture/assertion, harness/admission, GatedSets or schema change. Next: separate 4.0 optional arities/keyword implementation. Bosak.Schema owns staged XSD 1.1; REQ-126 remains Complete. Earlier checkpoints are historical.

**K04-C17 optional date-arity review ? 28 October 2026:** [Pinned contract and diagnosis](conformance/k04-date-arity-review/README.md) covers format-date, format-dateTime and format-time. Eight originals fail with missing arities 3/4; 96 controls give 45 passes/51 failures, separating 30 existing empty-value failures from 21 missing 4.0 call-surface outcomes. Fresh normal exploratory scopes remain **208/6/18** (date/dateTime) and **92/2/4** (time); all eight failures are retained. Next: correct shared empty-result semantics before separate 4.0 registrations/keywords. No engine, admission, version or GatedSets change. Inherited source-identical validation: 22,579 solution passes, QT3 **31,164/0/657**, qt4 gate **8,587/0/1,892**; no fresh full-suite claim. Candidate 1.28.1 unpublished/public 1.26.3; Bosak.Schema owns staged XSD 1.1 and REQ-126 remains Complete.

**K04-C16 adopted ? 27 October 2026:** Owner approved the exact proposal at 6df9a58b. [Fresh normal adoption receipt](conformance/k04-calendar-admission/adoption/BASELINE-ADOPTION.md) records QT3 **31,164/0/657**, with only the two original CB skips removed and all 657 retained reasons identical. The qt4 gate stays **8,587/0/1,892**; combined exploratory date/dateTime is **208/6/18**, preserving all six failures and 18 other skips, with no promotion. All four originals, 364 production filter controls and 40 projected controls match approval. AGENTS now requires **31,164/0/657**. All **22,579 solution tests** pass, including **8,196 XSLT tests**. Engine, fixtures/assertions, 226 GatedSets, schema and version remain unchanged. Candidate 1.28.1 unpublished/public 1.26.3; Bosak.Schema owns staged XSD 1.1 and REQ-126 remains Complete. Next: missing date-formatting arities 3/4 contract review. Earlier pending-proposal notices are historical.

**K04-C16 CB admission proposal ? 27 October 2026:** [Exact three-file proposal](conformance/k04-calendar-admission/BASELINE-UPDATE-PROPOSAL.md) is ready for owner disposition. The complete isolated QT3 rehearsal passes **31,164/0/657**, removing only the two original CB skips with 657 retained reasons identical. The qt4 gate stays **8,587/0/1,892**; combined exploratory date/dateTime improves **206/6/20 ? 208/6/18**, retaining all six failures and 18 other skips, with no promotion. All four originals and 364 filter controls verify the bounded rule; all 25 diagnostic/rehearsal processes complete without timeout. Normal filters and AGENTS remain **31,162/0/659** pending explicit approval. K04-C15 source-identical 22,579 solution passes are inherited, not a fresh unit run. Candidate 1.28.1 unpublished/public 1.26.3; no engine, fixture/assertion, GatedSets, schema or release change. Bosak.Schema owns staged XSD 1.1; REQ-126 remains Complete. Earlier checkpoints are historical.

**K04-C15 calendar fallback correction ? 27 October 2026:** [Bounded implementation](conformance/k04-calendar-fix/README.md) separates listed unsupported calendars from invalid names, adds one AD fallback indication per output and retains shared XSLT policy. All 381 final regressions and 58 original/control diagnostics pass, including four original CB assertions. The pre-existing empty-value XPTY0004 defect remains separately documented. Normal gates remain **31,162/0/659** and **8,587/0/1,892**, with identical skip names/reasons. All **22,579 solution tests** pass, including **8,196 XSLT tests**. Candidate **1.28.1** unpublished/public **1.26.3**; no admission, fixture/assertion, GatedSets or schema change. Next: bounded CB capability admission review before any exact-baseline proposal. Bosak.Schema owns staged XSD 1.1; REQ-126 remains Complete. Earlier checkpoints remain historical.

**K04-C12 adopted - 27 October 2026:** Owner approved the exact proposal at e924a708. [Normal adoption receipt](conformance/k04-sequence-admission/adoption/BASELINE-ADOPTION.md) records fresh gates **31,162/0/659** and **8,587/0/1,892**;only format-integer-042 leaves each skip set,with all retained659/1,892 names/reasons identical. All22,198solution tests pass. Both originals pass normally and288production filter controls match the approved prototype. AGENTS.md now requires31,162/0/659. Engine,fixtures/assertions,schema,version and226GatedSets are unchanged. Candidate1.28.0 unpublished/public1.26.3;Bosak.Schema owns staged XSD1.1;REQ-126 remains Complete. Earlier pending-proposal notices are historical checkpoints.

**K04-C12 unsupported-sequence proposal - 27 October 2026:** [Exact baseline review](conformance/k04-sequence-admission/BASELINE-UPDATE-PROPOSAL.md) verifies original format-integer-042 assertions in both profiles. Actual formatter mechanism and16expression controls prove decimal fallback for unsupported U+FBF4;288filter controls preserve other policy. Complete isolated rehearsals pass **31,162/0/659** and **8,587/0/1,892**,with only042removed and all retained skip reasons identical. Normal source and AGENTS stay **31,161/0/660** / **8,586/0/1,893** pending explicit owner approval of the exact three-file patch. Source-identical22,198solution passes are inherited,not a fresh unit run. Candidate1.28.0 unpublished/public1.26.3;no engine,fixture/assertion,schema or GatedSets change. Bosak.Schema owns staged XSD1.1;REQ-126 remains Complete. Earlier K04 notices are historical checkpoints.

**K04-C11 parse-integer implementation - 27 October 2026:** [Bounded parser and promotion](conformance/k04-parse-integer-fix/README.md) adds4.0-only arities1/2,typed optional/default arguments,keywords and exact Int64 parsing/errors. All47 parse originals,the formatter round trip and46 profile controls pass;new87 regression rows and all22,198 solution tests pass. Fresh complete sets47/0/0 and101/0/1 qualify for promotion:qt4 now **226sets,8,586/0/1,893**. All1,892 prior qt4 skips remain;only the existing format-integer-042 skip is newly covered. QT3 stays **31,161/0/660**,with identical reasons. Candidate **1.28.0** unpublished/public1.26.3. No fixture/assertion,filter,schema or QT3 baseline change. Bosak.Schema owns staged XSD1.1;REQ-126 remains Complete. Next:review the negative numbering-sequence dependency on format-integer-042 before any exact-baseline proposal. Earlier K04 notices are historical checkpoints.

**K04-C10 parse-integer review - 27 October 2026:** [Parser-function contract review](conformance/k04-parse-integer-review/README.md) reproduces47 original parse failures plus the formatter round trip,all caused by missing registration. The frozen catalog requires4.0-only arities1/2,typed optional arguments/default10,keywords and distinct FORG0011/FORG0012/FOCA0003 handling. Seven originals allow implementation-size errors;an explicit Int64 result bound is feasible. Accepted46 controls isolate profiles,defaults,typing,lexical/range behavior and keyword metadata;one corrected3.1 keyword expectation is preserved. No source/admission/version change. Inherited22,111 solution passes and gates **31,161/0/660** / **8,438/0/1,892** are not fresh full-suite claims. Candidate1.27.0 unpublished/public1.26.3;Bosak.Schema owns staged XSD1.1;REQ-126 remains Complete. Next: explicit semantic adoption and bounded parse-integer implementation. Earlier K04 notices are historical checkpoints.

**K04-C09 radix formatter - 27 October 2026:** [Bounded radix implementation](conformance/k04-radix-fix/README.md) fixes14 formatter-only originals and preserves the two caret-grouping cases. The exploratory set improves to **100/1/1**;case012 now exposes missing parse-integer#2 and stays failed/ungated. New68 regression rows pass (53failed before);all22,111 solution tests pass. Frozen gates/skips remain **31,161/0/660** and **8,438/0/1,892**. The4.0-only path preserves3.1 behavior and existing public Int64 input bounds. Explicit pre-implementation adoption uses the frozen qt4cg revision;candidate **1.27.0** unpublished/public1.26.3. No harness,fixture/assertion,GatedSets,schema or QT3 baseline change. Bosak.Schema owns staged XSD1.1;REQ-126 remains Complete. Next: separate parse-integer contract review. Earlier K04 notices are historical checkpoints.

**K04-C08 radix review - 27 October 2026:** [Radix contract review](conformance/k04-radix-review/README.md) reproduces17 original radix cases (two existing passes/15 failures),34 profile controls and the normal102-case exploratory result **86/15/1**. The failures separate into14 formatter-only cases and one round trip that also needs the unavailable parse-integer#2. The exact frozen qt4cg specification revision is pinned for review; no semantic adoption or source change. Caret-grouping compatibility, original-value ordinal suffixes and the existing Int64 input boundary are explicit. Next: adopt and implement a4.0-only radix formatter;keep parse-integer/case012 separate. Source-identical22,043 solution passes and gates **31,161/0/660** / **8,438/0/1,892** are inherited,not fresh full-suite claims. Candidate1.26.12 unpublished/public1.26.3; Bosak.Schema owns staged XSD1.1; REQ-126 remains Complete. Earlier K04 notices are historical checkpoints.

**K04-C07 bounded CLDR admission - 27 October 2026:** [CLDR admission](conformance/k04-cldr-admission/README.md) enables the four repaired original qt4 cases through normal admission. The full102-case exploratory set improves from82/15/5 to **86/15/1**; all15 failure names/messages and the remaining dependency skip are retained. The set stays ungated. All242 before/after filter controls and six normal original checks pass. Fresh22,043 solution tests pass; exact gates/skips remain **31,161/0/660** and **8,438/0/1,892**. No engine/version, fixture/assertion, GatedSets, schema or QT3 baseline change. Candidate1.26.12 unpublished/public1.26.3; Bosak.Schema owns staged XSD1.1; REQ-126 remains Complete. Next: immutable-spec review of the15 remaining XPath4 radix-picture errors. Earlier K04 notices are historical checkpoints.

**K04-C06 formatter correction - 27 October 2026:** [Bounded integer-variation fix](conformance/k04-cldr-fix/README.md) accepts cardinal variations and preserves known German named ordinal endings using pinned F&O/CLDR semantics. All six original assertions and22 diagnostic controls pass. The new36 regression rows pass in both compatibility paths (21failed before). Fresh solution validation passes **22,043 tests**; gates remain **31,161/0/660** and **8,438/0/1,892**, with every skip name/reason unchanged. Candidate **1.26.12** remains unpublished/public1.26.3. No qt4 CLDR admission, fixture/assertion, GatedSets, schema or QT3 baseline change. Bosak.Schema owns staged XSD1.1; REQ-126 remains Complete. Next: separate bounded qt4 CLDR admission review. Earlier K04 notices are historical checkpoints.

**K04-C05 CLDR review - 27 October 2026:** [CLDR diagnosis](conformance/k04-cldr-review/README.md) preserves the original two QT3 passes, two qt4 direct passes and two qt4 FODF1310 failures. Normal qt4 still dependency-skips all four cases. Focused controls isolate rejected named-cardinal modifiers and lost German named-ordinal endings; initial control-expectation mistakes are preserved separately. No source/admission/baseline change. Inherited source-identical results:22,007 solution passes, QT3 **31,161/0/660**, qt4 gated **8,438/0/1,892**; no fresh full-suite claim. Next: pin immutable specification/CLDR semantics and implement a bounded formatter correction before reviewing admission. Candidate1.26.11 unpublished/public1.26.3; Bosak.Schema owns staged XSD1.1; REQ-126 remains Complete. Earlier K04 notices are historical checkpoints.

**K04-C04 bounded collation admission - 27 October 2026:** [Collation review](conformance/k04-collation-admission/README.md) admits existing non-codepoint collation routes in qt4. The original compare-010 assertion passes; the normal qt4 gate improves to **8,438/0/1,892**, with only that gated skip removed and all retained reasons unchanged. Four exploratory original assertions also pass; eight XQuery-only cases remain dependency-skipped, with their direct diagnostic failures preserved. Seven expression controls and248 before/after filter controls pass. Fresh solution validation passes **22,007 tests**. Normal QT3 remains **31,161/0/660**. No engine, fixture/assertion, GatedSets, schema or QT3 baseline change. Candidate1.26.11 unpublished/public1.26.3; Bosak.Schema retains XSD1.1 ownership; REQ-126 remains Complete. Next: bounded review of four qt4 CLDR format-integer declarations. Earlier K04 notices are historical checkpoints.

**K04-C03 explicit feature admission - 27 October 2026:** [Capability inventory](conformance/k04-feature-inventory/README.md) records all 78,806 catalog cases and 55 feature/profile rows. Both filters now reject unrecognized feature names for either polarity and preserve explicit legacy admissions (19 QT3/17 qt4), without certifying full support. All 222 before/after controls pass; current feature decisions and all skip names/reasons remain unchanged. Fresh solution validation passes **22,007 tests**. Fresh gates remain **31,161/0/660** and **8,437/0/1,893**. Bosak.Schema retains staged XSD 1.1 ownership. Candidate1.26.11 unpublished/public1.26.3; REQ-126 remains Complete. Next: original-assertion review of the single gated qt4 non-codepoint-collation skip. Earlier K04 notices are historical checkpoints.

**K04-C02 bounded polarity correction - 27 October 2026:** [Profile review](conformance/k04-profile-review/README.md) makes negative spec/default-language predicates invert their existing positive predicates in both harnesses, with 74 before/after controls. An inventory of 31,821 QT3 and all 46,985 qt4 cases finds no current negative declaration of either kind. Fresh solution validation passes **22,007 tests**. Fresh gates remain **31,161/0/660** and **8,437/0/1,893**, retaining every skip name/reason. Schema comments now distinguish frozen admission from Bosak.Schema's staged XSD 1.1 implementation; admission is unchanged. F03 broader capabilities and F04 integration remain open. Candidate1.26.11 unpublished/public1.26.3; REQ-126 remains Complete. Next: explicit feature/capability inventory, beginning with unknown-positive fallback. Earlier K04 notices are historical checkpoints.

**K04-C01 adopted ? 27 October 2026:** Owner approved the exact 16-case quote-routing change proposed at `d0ca0741`. The complete normal QT3 gate passes **31,161/0/660**; exactly the 16 approved cases leave the skip set and all 660 retained names/reasons are unchanged. The qt4 gated run passes **8,437/0/1,893**, with 11 Experimental passes. All broader routing/schema/dependency guards remain unchanged. Candidate 1.26.11 remains unpublished; public version is 1.26.3. Bosak.Schema retains XSD 1.1 ownership; REQ-126 remains Complete. Next: the remaining K04 dependency-profile/polarity review. See [adoption receipt](conformance/k04-quote-routing/adoption/BASELINE-ADOPTION.md) for approval and fresh validation. Earlier proposal notices are historical.

**K04-C01 quote-routing proposal — 27 October 2026:** [Exact adoption review](conformance/k04-quote-routing/BASELINE-UPDATE-PROPOSAL.md) verifies all16 original candidate assertions in an isolated corrected harness. Full rehearsal passes31,161/0/660, with exactly those16 skips removed and all660 retained names/reasons unchanged. Eleven boundary cases and13 lexical controls preserve broader guards. Normal source/baseline stays31,145/0/676 pending owner approval; no tracked engine/harness/filter change. Candidate1.26.11 unpublished/public1.26.3;22,007 solution passes and qt4 8,437/0/1,893 are inherited unchanged-engine evidence. Bosak.Schema XSD1.1 ownership and REQ-126 Complete are unchanged.



**K04 applicability audit — 27 October 2026:** [Current dependency/routing review](conformance/k04-dependency-audit/README.md) reproduces all619 QT3 and1,893 gatedqt4 dependency skips through actual filters. Of619 QT3 cases,246 satisfy XQuery dependencies but fail syntax routing;16 are triple-quote candidates. Thirty-two bounded execution probes yield six public-XQuery assertion passes and ten retained failures;62 synthetic predicate assessments expose profile/polarity questions. No engine, filter or baseline change. Next K04-C01: quote-aware routing with complete candidate and rehearsal evidence before any QT3 admission proposal. Candidate1.26.11 unpublished/public1.26.3; inherited22,007 solution passes and gates31,145/0/676 and8,437/0/1,893. Bosak.Schema owns XSD1.1; REQ-126 remains Complete.



**K03 name-storage integration — 27 October 2026:** [Bounded core completion](conformance/k03-name-storage/README.md) preserves Unicode map/provider names, literal values and namespace/copy metadata. All47 new regressions and22,007 solution tests pass. The original map-to-element-053 assertion now passes; qt4 improves to8,437/0/1,893 with exactly that skip removed and all1,893 retained reasons unchanged. QT3 remains31,145/0/676 with identical676 skips. No fixture, assertion, harness filter or gated-set change. Candidate1.26.11 unpublished/public1.26.3. Parser-edition/current-draft limits remain explicit; Bosak.Schema retains XSD1.1 ownership and REQ-126 remains Complete. Next: K04 dependency/routing applicability audit. Earlier open Unicode findings are historical checkpoints.



**K03-C02 serialization — 27 October 2026:** [Content-preserving name decoding](conformance/k03-serialization/README.md) restores encoded element/attribute name tokens while preserving existing literal node content, namespace URIs and declarations. All40 focused regressions and21,960 solution tests pass; QT3 remains31,145/0/676 with identical676 skips, and qt4 remains8,436/0/1,894. The original supplementary-name skip remains. Candidate1.26.10 unpublished/public1.26.3. Next: reversible name-storage integration with loader/accessor/copy round-trip verification; no public API, baseline or XSD1.1 ownership change. REQ-126 remains Complete.



**K03-C01 XML-name validation — 27 October 2026:** [Scalar validation correction](conformance/k03-name-validation/README.md) replaces category/surrogate-pair approximation with explicit XML name ranges and rejects malformed UTF-16. All78 focused tests pass (baseline18 failures), all21,920 solution tests pass, and gates stay QT3 31,145/0/676 and qt4 8,436/0/1,894. All676 QT3 skip names/reasons are unchanged. Original supplementary construction and literal-content serialization findings remain open; next is K03-C02 serialization preservation before provider storage integration. Candidate1.26.9 unpublished/public1.26.3; no baseline or XSD1.1 capability change. REQ-126 remains Complete.



**K03 Unicode-name trace — 27 October 2026:** [Bounded investigation](conformance/k03-unicode-names/README.md) reproduces map-to-element-053 and separates three open corrections: scalar NCName validation, reversible provider name storage, and serialization that preserves literal sentinel text. All 30 isolated diagnostics complete without timeouts; no engine or baseline change. Next: K03-C01 name-validation regressions/correction, followed by serialization preservation before map storage integration. QT3 remains31,145/0/676 and qt4 remains8,436/0/1,894; solution21,842 and gates are inherited unchanged-source evidence. Candidate1.26.8 unpublished/public1.26.3; REQ-126 remains Complete. Bosak.Schema XSD1.1 ownership is unchanged.



**K02 adopted — 27 October 2026:** [Owner-approved normal-runner adoption](conformance/k02-admission-review/BASELINE-ADOPTION.md) is complete. The fresh normal QT3 gate passes **31,145/0/676**; only validateexpr-28 leaves the skip set through its original XQST0012 error alternative, and all 676 retained names/reasons match. AGENTS now requires this exact baseline. The normal runner selects the existing guard; standalone TestExecutor defaults and all profile/structural exclusions remain unchanged. Engine/candidate1.26.8 unchanged and unpublished; public1.26.3. Solution21,842 and qt4 8,436/0/1,894 are inherited source-identical validation. Bosak.Schema owns XSD1.1. REQ-126 remains Complete; earlier pending/default-off notices are historical checkpoints.



**K02 admission review — 27 October 2026:** [Guarded-runner proposal](conformance/k02-admission-review/BASELINE-UPDATE-PROPOSAL.md) is ready for owner approval. Two full actual-runner rehearsals verify current 31,144/0/677 and guarded 31,145/0/676, with only validateexpr-28 leaving the skip set through its existing XQST0012 alternative; all 676 retained names/reasons match. Normal runner, AGENTS policy and baseline remain unchanged pending approval. Candidate 1.26.8 is unpublished/public 1.26.3. Unit21,842 and qt4 8,436/0/1,894 receipts are inherited from unchanged source; no new XSD1.1 capability. Bosak.Schema REQ-009 owns that track; REQ-126 remains Complete.



**K02-C02 relative schema imports — 27 October 2026:** [File-backed source URI correction](conformance/k02-relative-schema-uri/README.md) fixes the four remaining guarded regressions. All 324 eligible original cases now pass in guarded diagnostics; 17 declined-boundary cases remain unchanged. All 21,842 solution tests and both required gates pass: QT3 31,144/0/677 (all 677 skip names/reasons unchanged), qt4 8,436/0/1,894. Guarded loading stays default-off; admission/baseline review is separate. Candidate 1.26.8 is unpublished/public 1.26.3. Bosak.Schema REQ-009 owns XSD 1.1; this slice adds no public API or backend. REQ-126 remains Complete. Earlier failure counts are historical evidence.



**K02-C01 constructor registration — 27 October 2026:** [First-evaluation correction](conformance/k02-constructor-registration/README.md) fixes all 68 constructor regressions in the wider audit. All 11 focused tests and 21,836 solution tests pass; QT3 remains 31,144/0/677 and the qt4 gate 8,436/0/1,894. The 324-case paired sweep leaves four relative-import failures for K02-C02, with no new failures; 17 declined boundaries are unchanged. Guarded loading stays default-off. Candidate 1.26.7 is unpublished/public 1.26.3. Bosak.Schema REQ-009 owns XSD 1.1; no new public schema seam or capability is introduced. REQ-126 remains Complete. Earlier audit counts are historical evidence.



**K02 wider audit — 27 October 2026:** [Schema/import inventory](conformance/k02-schema-inventory/README.md) blocks normal adoption: 324 eligible original cases yield 251 retained passes, 72 regressions and one environment-exception-to-pass transition. All 1,584 metadata/paired/boundary processes complete without timeouts. Two corrections remain open: schema constructor registration after import (68 observed regressions), then relative schema-import base URI (four). Guarded loading stays default-off; no source, skip, baseline or version change. Next: K02-C01 constructor registration. Candidate 1.26.6 unpublished/public 1.26.3; REQ-126 remains Complete. Earlier narrow-sample results remain historical evidence.



23 October2026: PR425 REQ-136 formal consumer acceptance and canonical status counterparts rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req136-acceptance-render`; original acceptance Git bytes, four1444-pass TRX hashes/counters, nine signed public assembly identities, reported native limits, registry/detail consistency and new links checked. Evidence was inspected, not rerun. HTML only; no screenshot/PDF acceptance; kit assets unchanged.


23 October2026: REQ-136 consumer implementation receipt, 1.24.0 release notes/readiness and canonical counterparts rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/release1240-render`; new links/JSON, exact retained review bytes and package verification checked. HTML only; no screenshot/PDF acceptance. Managed kit assets unchanged.


23 October 2026: REQ-136 implementation, exact supported vocabulary, verification receipts and canonical counterparts rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req136-implementation-render`; new links, JSON and source headers/XML documentation checked. HTML only; no screenshot/PDF acceptance. The reviewed scope proposal and original request remain byte-unchanged.


23 October 2026: release 1.23.8 notes/readiness and canonical counterparts rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/release1238-render`; new relative links and JSON checked. HTML only; no screenshot/PDF approval. Kit assets unchanged.

23 October2026: direct module/profile entry, complete library reports, prerequisite separation and canonical evidence counterparts rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-module-profile-render`; JSON/XML, new links,76-entry/16-field index, source headers and diff checked. HTML only; no screenshot/PDF acceptance. Kit assets unchanged.

23 October2026: function default-mode finding and canonical status counterparts rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-function-mode-render`; JSON, new links, source headers and diff checked. HTML only; no screenshot/PDF acceptance. Kit assets unchanged.

23 October2026: ordinary stylesheet host/version entry, complete order task and canonical evidence counterparts rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-host-profile-render`. JSON/XML, new links,75-entry/16-field index and source header checks pass. HTML only; no screenshot/PDF approval. Kit assets unchanged.

23 October2026: release1.23.7 notes/readiness and canonical counterparts rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/release1237-render`; new relative links checked. HTML only; no screenshot/PDF approval. Kit assets unchanged.

23 October2026: numeric-literal entry, complete telemetry task, lexical finding and canonical coverage/counterparts rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-numeric-literals-render`. JSON/XML, local-link,16-field/index and source header checks pass. HTML only; no screenshot/PDF acceptance. Kit assets unchanged.


22 October2026: record-contract reference, complete stock task, inline-record signature finding and canonical status/counterpart updates rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-record-contract-render`. Field/index, local-link, XML/JSON and source header checks pass. HTML check only; no screenshot/PDF acceptance. Kit assets unchanged.

22 October 2026: focus-function syntax entry, complete order-line examples, corrected pipeline prose and synchronized canonical coverage references rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-focus-syntax-render`. Local links, XML/JSON and 16-field/index checks pass. HTML check only; no screenshot/PDF acceptance. Kit assets unchanged.

22 October 2026: REQ-126 coverage/acceptance reconciliation, corrected primary registry counts and canonical references rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-reconciliation-render`. Local links, JSON and matrix references validated. HTML check only; no screenshot/PDF acceptance. Kit assets unchanged.

22 October 2026: EQName bracket-parser correction and canonical updates rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-eqname-parser-render`. HTML render check only; no screenshot or PDF acceptance. Kit assets unchanged.

22 October 2026: release1.23.4 notes/readiness and changed canonical documents rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/release1234-render`; new relative links checked. HTML render check only, not screenshot/PDF review. Kit assets unchanged.

22 October 2026: EQName-priority correction pages rendered with markdown-it and unchanged canonical Fytala CSS under `.guard-tmp/req126-eqname-priority-render`; added relative links checked. HTML render check only; no screenshot/PDF approval. Kit assets unchanged.

22 October 2026: union minimum-priority correction pages rendered with markdown-it and unchanged canonical Fytala CSS under `.guard-tmp/req126-union-minimum-render`. Relative links in added content were checked. This is an HTML render check; no screenshot or PDF approval is claimed. Kit-managed assets are unchanged.

Use this document to approve a FYTALA documentation-kit version in VS Code's built-in Markdown preview, Markdown Preview Enhanced, and Markdown PDF. It deliberately exercises the elements whose presentation is governed by the kit.

## Typography and links

Body text should use the configured type stack with dark-slate text. A [normal link](https://fytala.com) should remain visually distinct, and `inline code` should be readable without overpowering the paragraph.

> Blockquotes should have a restrained accent and remain readable in both preview and PDF output.

## Table

| Element | Expected result | Reference |
|---|---|---|
| Header | Dark slate with light mint text | `--table-header-bg` |
| Link in header | Inherits the header foreground | [Style guide](DOCUMENTATION_STYLE_GUIDE.md) |
| Alternating row | Subtle neutral contrast | Canonical stylesheet |

## Code

```csharp
public sealed record DocumentationKit(string Version, bool IsPortable);
```

## Mermaid diagram

```mermaid
flowchart LR
    Manifest[Versioned manifest] --> Preview[Markdown preview]
    Manifest --> Enhanced[Enhanced preview]
    Manifest --> Pdf[PDF export]

    classDef prime fill:#F0FFF0,stroke:#518D8F,color:#2F4F4F
    classDef platform fill:#E8F4EE,stroke:#556B2F,color:#2F4F4F
    class Manifest prime
    class Preview,Enhanced,Pdf platform
```

<p class="fytala-figure-caption"><strong>Renderer coverage.</strong> One versioned contract drives all supported local renderers.</p>

## Acceptance checklist

- The logo renders without a decorative frame.
- Table headers use dark slate and light mint, including links and inline code.
- The Mermaid canvas is white while semantic node colors remain visible.
- Code blocks, blockquotes, links, and captions remain legible.
- PDF output preserves local images, page margins, and diagram boundaries.


## Implicit binding documents verification - 2026-10-16

The canonical documents, queue, upstream guide and XSLT40-IMPLICIT-BINDING-DOCUMENTS.md were rendered using markdown-it and the canonical Fytala CSS. HTML artifacts are retained under D:/Development/Bosak/.guard-tmp/variable-binding-render. This is a render check, not a visual screenshot review. Kit-managed assets were unchanged.


## Array template fallback verification - 2026-10-16

Canonical documents, queue, upstream guide and XSLT40-ARRAY-TEMPLATE-FALLBACK.md were rendered using markdown-it and canonical Fytala CSS. HTML artifacts are retained under D:/Development/Bosak/.guard-tmp/array-fallback-render. This is a render check, not a screenshot review. Kit-managed assets are unchanged.


## Array apply-imports fallback verification - 2026-10-16

Canonical documents, queue, upstream guide and XSLT40-ARRAY-IMPORT-FALLBACK.md were rendered using markdown-it and canonical Fytala CSS. HTML artifacts are retained under D:/Development/Bosak/.guard-tmp/array-import-fallback-render. This is a render check, not a screenshot review. Kit-managed assets are unchanged.


## Array profile audit verification - 2026-10-16

Canonical documents, queue, upstream guide and XSLT40-ARRAY-PROFILE-AUDIT.md were rendered using markdown-it and canonical Fytala CSS. HTML artifacts are retained under D:/Development/Bosak/.guard-tmp/array-profile-audit-render. This is a render check, not a screenshot review. Kit-managed assets are unchanged.


## JNode text atomization verification - 2026-10-16

Canonical documents, queue, upstream guide and XSLT40-JNODE-TEXT-ATOMIZATION.md were rendered using markdown-it and canonical Fytala CSS. HTML artifacts are retained under D:/Development/Bosak/.guard-tmp/jnode-text-render. This is a render check, not a screenshot review. Kit-managed assets are unchanged.


## Document construction capture verification - 2026-10-16

Canonical documents, queue, upstream guide and XSLT40-DOCUMENT-CONSTRUCTION-CAPTURE.md were rendered using markdown-it and canonical Fytala CSS. HTML artifacts are retained under D:/Development/Bosak/.guard-tmp/document-capture-render. This is a render check, not a screenshot review. Kit-managed assets are unchanged.


## Computed-element capture verification - 2026-10-16

Canonical documents, queue, upstream guide and XSLT40-COMPUTED-ELEMENT-CAPTURE.md were rendered using markdown-it and canonical Fytala CSS. HTML artifacts are retained under D:/Development/Bosak/.guard-tmp/computed-element-render. This is a render check, not a screenshot review. Kit-managed assets are unchanged.


## Literal-result-element capture receipt - 2026-10-16

Ten current documentation pages, including XSLT40-LITERAL-ELEMENT-CAPTURE.md, rendered successfully using markdown-it and canonical Fytala CSS to `.guard-tmp/literal-element-render`. This verifies Markdown/HTML generation; it is not a visual approval or screenshot receipt. Kit-managed assets were untouched.


## Copied-element capture receipt - 2026-10-16

Ten current documentation pages, including XSLT40-COPY-ELEMENT-CAPTURE.md, rendered using markdown-it and canonical Fytala CSS to `.guard-tmp/copy-capture-render`. This verifies Markdown/HTML generation; it is not a visual approval or screenshot receipt. Kit-managed assets were untouched.


## Copied-document capture receipt - 2026-10-16

Ten current documentation pages, including XSLT40-COPY-DOCUMENT-CAPTURE.md, rendered using markdown-it and canonical Fytala CSS to `.guard-tmp/copy-document-render`. This verifies Markdown/HTML generation; it is not a visual approval or screenshot receipt. Kit-managed assets were untouched.


## Function-body copy capture receipt - 2026-10-16

Ten current documentation pages, including XSLT40-FUNCTION-COPY-CAPTURE.md, rendered using markdown-it and canonical Fytala CSS to `.guard-tmp/function-copy-render`. This verifies Markdown/HTML generation; it is not a visual approval or screenshot receipt. Kit-managed assets were untouched.


## Copy-select state receipt - 2026-10-16

Ten current documentation pages, including XSLT40-COPY-SELECT-STATE.md, rendered using markdown-it and canonical Fytala CSS to `.guard-tmp/copy-select-render`. This verifies Markdown/HTML generation; it is not a visual approval or screenshot receipt. Kit-managed assets were untouched.


## Non-node copy values receipt - 2026-10-16

Ten current documentation pages, including XSLT40-COPY-NON-NODE-VALUES.md, rendered using markdown-it and canonical Fytala CSS to `.guard-tmp/copy-values-render`. This verifies Markdown/HTML generation; it is not a visual approval or screenshot receipt. Kit-managed assets were untouched.


## Copy namespace validation receipt - 2026-10-16

Ten current documentation pages, including XSLT40-COPY-NAMESPACE-VALIDATION.md, rendered using markdown-it and canonical Fytala CSS to `.guard-tmp/copy-xmlns-render`. This verifies Markdown/HTML generation; it is not a visual approval or screenshot receipt. Kit-managed assets were untouched.


## Copy standard attributes receipt - 2026-10-16

Ten current documentation pages, including XSLT40-COPY-STANDARD-ATTRIBUTES.md, rendered using markdown-it and canonical Fytala CSS to `.guard-tmp/copy-standard-render`. This verifies Markdown/HTML generation; it is not a visual approval or screenshot receipt. Kit-managed assets were untouched.


## 2026-10-16 copy forward-compatibility audit receipt

Ten updated documentation pages rendered with markdown-it and canonical Fytala CSS into .guard-tmp/copy-forward-audit-render. Automated HTML generation only; no visual preview approval claimed.


## 2026-10-16 processor-profile receipt

Ten updated documentation pages rendered with markdown-it and canonical Fytala CSS into .guard-tmp/processor-profile-render. Automated HTML generation only; no visual approval claimed.


## 2026-10-16 runner-profile receipt

Ten updated pages rendered with markdown-it and canonical Fytala CSS into .guard-tmp/runner-profile-render. Automated HTML generation only; no visual approval claimed.


## 2026-10-16 fixed-namespace validation receipt

Ten updated pages rendered with markdown-it and canonical Fytala CSS into .guard-tmp/fixed-namespace-validation-render. Automated generation only; no visual approval claimed.


## 2026-10-16 XSLT40-FIELD-REQUIRED-AUDIT

Ten pages rendered with markdown-it and canonical Fytala CSS. Automated HTML generation only; no visual approval claimed.


## 2026-10-16 XSLT40-EARLIER-FIXED-NAMESPACES

Ten pages rendered with markdown-it and canonical Fytala CSS. Automated HTML generation only; no visual approval claimed.


## 2026-10-16 XSLT40-FUTURE-FALLBACK

Ten pages rendered with markdown-it and canonical Fytala CSS. Automated HTML generation only; no visual approval claimed.


## 2026-10-16 Release-readiness review

Ten pages rendered with markdown-it and canonical Fytala CSS. Automated HTML generation only; no visual approval claimed. Complete45-case inventory and seven reduced probe outcomes validated. Engine/tests unchanged; prior exact-source CI and gates retained.


## 2026-10-16 XSLT40-STRUCTURAL-RECORD-SUBTYPING

Ten pages rendered with markdown-it and canonical Fytala CSS. Automated HTML generation only; no visual approval claimed.


## 2026-10-16 XSLT40-RECORD-MAP-SUBTYPING

Ten pages rendered with markdown-it and canonical Fytala CSS. Automated HTML generation only; no visual approval claimed.


## 2026-10-16 Record-subtyping candidate checkpoint

Candidate validation, release notes and canonical pages rendered with markdown-it and canonical Fytala CSS. Automated HTML generation only; no visual approval claimed. Package-only provenance,338 public focused tests,153 broad controls and all36 remaining case dispositions validated. Engine/tests unchanged by this checkpoint.


## 2026-10-16 Release preparation

Eleven pages rendered with markdown-it and canonical Fytala CSS in a fresh output directory. The candidate banner contains a valid h1 element; malformed heading tags are rejected by the renderer check. Relative links and unchanged engine/test trees are checked. Automated rendering only; no visual approval claimed. Prior renderer output is preserved.


## 2026-10-16 Published1.16.5 record

Twelve canonical/publication/release-note/checklist pages rendered with markdown-it and canonical Fytala CSS in a fresh output directory. Valid heading tags and release-note h1 are checked. Public package signatures,153 broad controls,338 public focused tests and exact release provenance verify. Automated rendering only; no visual approval claimed. Engine/tests and immutable release tag unchanged.


## Extension 0.2.5 preparation

Render the new preparation report and canonical status pages with the canonical Fytala stylesheet. Verify branded banners, links and the pending publication wording. No kit assets change.


## Extension 0.2.5 publication

Publication report and canonical status pages use the unchanged Fytala stylesheet. Verify branded banners, publication links and preserved historical candidate notices.


## REQ-129 design checkpoint

Render the readable insertion contract and canonical status notices with the unchanged Fytala stylesheet. Verify tables, XML example, branded banner and explicit proposed/not-implemented language.


## REQ-129 implementation checkpoint

Render the implementation API example, source-only status and canonical notes with unchanged Fytala styles. Verify links and explicit public-release/consumer boundaries.


## Bosak1.17.0 publication

Render the publication report, release notes and canonical notices using unchanged Fytala CSS. Verify publication links, bounded API wording and Braid feedback attribution.


## Literal-text design and consumer acceptance

Render the new contract/matrix and accepted-status notices using unchanged Fytala CSS. Preserve the imported acceptance receipt byte-for-byte; its compact heading is an evidence-record exception. Verify XML examples, table escaping and explicit proposed/no-implementation wording.


## Bosak1.23.0 publication verification - 2026-10-21

Publication proof, release notes, canonical documents and updated delivery guides were rendered with markdown-it and the unchanged canonical Fytala CSS. HTML artifacts remain under `.guard-tmp/release1230-publication-render`. This is a render check, not a visual screenshot or PDF review. No kit-managed assets changed.


## XSLT4 switch boundary slice - 2026-10-21

The switch boundary evidence and canonical documentation were rendered using markdown-it and the unchanged Fytala CSS. HTML artifacts remain under .guard-tmp/switch-boundaries-render. This is a render check, not a visual screenshot or PDF review. Kit-managed assets are unchanged.


## Switch upstream submission record - 2026-10-21

The updated switch evidence, queue and canonical documents were rendered with markdown-it and the unchanged Fytala CSS under `.guard-tmp/switch-submission-render`. This is a render check, not a screenshot or PDF review. Kit assets remain unchanged.


## REQ-126 equality/collation reference - 2026-10-21

The changed catalog/reference pages and canonical documents were rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-equality-render`. The new entry has all16 required fields; the manifest keeps its2,578 original rows and adds58 checks. JSON/XML and new local links validate. This is a render check, not a screenshot or PDF review. Kit-managed assets are unchanged.


## Index-of empty-collation correction - 2026-10-21

Correction evidence, updated catalog entries and canonical documents were rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/index-of-collation-render`. New local links/JSON, the exact one-row catalog correction and bounded engine/test diff validate. This is a render check, not screenshot/PDF review. Kit-managed assets are unchanged.


## Release 1.23.2 preparation — 2026-10-21

Changed readiness, notes and canonical documents render with markdown-it and the unchanged Fytala stylesheet under `.guard-tmp/release1232-render`. New relative links and machine-readable package provenance validate. This is an HTML render check, not screenshot/PDF review. Kit-managed assets are unchanged.


## Release1.23.2 publication — 2026-10-21

README package links, canonical status, release notes/readiness and new publication proof render with markdown-it and unchanged Fytala CSS under `.guard-tmp/release1232-publication-render`. New local links, JSON identities and public package versions validate. This is an HTML render check, not screenshot/PDF review. Kit-managed assets are unchanged.


## REQ-126 grammar review — 2026-10-21

New xsl:select, grammar mapping and dispatch examples plus updated canonical/catalog pages render with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-grammar-render`. New local links, XML, JSON and all16-field entries validate. This is an HTML render check, not screenshot/PDF review. Kit-managed assets are unchanged.


## REQ-126 type/pattern slice — 2026-10-21

Current entries, intake reports, grammar mapping, canonical pages and relocated historical snapshots render with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-types-render`. New relative links, XML/JSON, all16-field entries and original-row preservation validate. Historical links are rebased to the immutable original revision; raw original Git-blob and transformed-archive hashes are distinct. This is an HTML render check, not screenshot/PDF review; kit assets are unchanged.


## REQ-126 compound-pattern slice — 2026-10-21

Grammar/priority finding, compound entry, manual examples and canonical/catalog updates render with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-compound-render`. New local links/XML/JSON/headers, all16-field entries and prior-row preservation validate. Observed gaps retain separate normative metadata. This is an HTML render check, not screenshot/PDF review; kit assets unchanged.


## Release1.23.4 publication — 2026-10-22

README package links, canonical status, release notes/readiness, catalog notices and publication proof rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/release1234-publication-render`; added relative links and public package receipts checked. HTML render check only; no screenshot/PDF review. Kit-managed assets unchanged.


## Release1.23.7 publication - 23 October2026

README install/package links, canonical status, release notes/readiness, catalog notices and public proof rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/release1237-publication-render`; new relative links and package receipts checked. HTML only; no screenshot/PDF approval. Kit assets unchanged.


## Release1.23.8 publication - 23 October2026

README install/package/release-note links, canonical status, release notes/readiness, catalog notices and public proof rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/release1238-publication-render`; new relative links and package receipts checked. HTML only; no screenshot/PDF approval. Kit assets unchanged.


## CSV options/defaults review - 23 October 2026

CSV contract review, supplier-import task, existing entry/index and canonical counterparts rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-csv-render`. New links/JSON, CSV/XML fixture parity,16 fields, preserved manifest rows and source header checked. HTML only; no screenshot/PDF approval. Kit assets unchanged.

## REQ-136 intake and scope proposal - 23 October2026

Original consumer request, maintainer scope/API proposal and canonical planning counterparts rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/req136-scope-render`; new links, intake JSON and source/API references checked. HTML only; no screenshot/PDF approval. Kit assets unchanged.


## Release1.24.0 publication — 23 October2026

README install/package/release-note links, canonical status, notes/readiness and signed-public proof rendered with markdown-it and unchanged Fytala CSS under `.guard-tmp/release1240-publication-render`; new links/JSON/README versions and package receipts checked. HTML only; no screenshot/PDF acceptance. Kit assets unchanged.

## CSV PR420 refreshed against accepted 1.24.0 - 23 October 2026

The twelve CSV review/canonical pages are re-rendered under `.guard-tmp/req126-csv-refresh-render` with unchanged Fytala CSS. Original renderer and execution receipts remain preserved. HTML only; no screenshot/PDF acceptance.

## CSV index revision audit - 23 October 2026

Contract/receipt links, seven native failure IDs, immutable revisions and canonical notices are checked. Ten Markdown pages render with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-csv-index-render`. HTML only; no screenshot/PDF acceptance. Imported native output is preserved byte-for-byte by `.gitattributes`. Kit assets unchanged.

## Current upstream gate audit - 23 October 2026

Ten audit/canonical Markdown pages render with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-current-gate-render`. Receipt hashes,223 executed names,224 configured names, six dotted-name extraction corrections, all45 native failures, JSON and new relative links are checked. Imported native output is retained byte-for-byte, including the skip dumps' final blank separators through receipt-specific whitespace attributes. HTML only; no screenshot/PDF acceptance. Kit assets unchanged.

## Frozen map-to-element gate repair - 24 October 2026

Eleven repair/canonical/catalog pages render with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-map-gate-render`. Exact224-name resolution/execution,8434/0/1896,QT3, original receipt hashes, synthetic control XML/exit policy, sixteen-field entry, JSON and new links checked. Native text and synthetic XML evidence retain original bytes and separators. HTML only; no screenshot/PDF acceptance. Kit assets unchanged.

## Experimental substring correction - 24 October 2026

Nine contract/canonical/catalog pages render with markdown-it and unchanged Fytala CSS under `.guard-tmp/req126-substring-contract-render`. Owner policy,38 API/two host controls, raw native receipt hashes, explicit current-frozen failure, JSON/links, header histories and16-field entry checked. Original native output and separators preserved by `.gitattributes`. HTML only; no screenshot/PDF acceptance. Kit assets unchanged.

## Open Schema integration documentation — 29 October 2026

The reader-document validator was extended to render six additional pages covering the optional integration guide, scope, qualification, reference correction, next slice and evidence guide alongside the existing reader pages. Local Markdown destinations and anchors are checked, complete historical README/roadmap snapshots and the FYTALA statement remain intact, and 141 sealed earlier review/prototype artifacts retain their identities. This is HTML/render-structure verification, not a screenshot or hosted-CI claim. Current receipts are in `docs/schema-integration/evidence` and ignored render output under `.guard-tmp/reader-docs-cleanup/render`.

## Native Schema assessment documentation — 29 October 2026

The native assessment check renders sixteen core reader/assessment pages and nine owning Schema canonical/profile pages, verifies local destinations and preserves all 39 earlier sealed integration files plus 141 preceding review/prototype artifacts. The owning Schema documentation checker passes in Strict mode. Results and rebased guide/adoption/ADR snapshots are in `docs/schema-assessment`; the primary editable implementation remains in the persistent Schema feature worktree. No screenshot, hosted CI or new full-core runtime qualification is claimed by this documentation check.

## Whole-document bridge documentation — 29 October 2026

Current bridge/canonical pages are checked separately from the sealed assessment dossier. HTML rendering and local-link validation use unchanged Fytala CSS; no screenshot or hosted-CI claim. See `docs/schema-document-bridge/evidence/documentation-checks.json`.

## Bounded bridge API review — 29 October 2026

Current canonical/guide/review pages receive a separate HTML/local-destination check with unchanged Fytala CSS. Original sealed documentation-check receipts are preserved. New render and strict Schema documentation receipts belong to `docs/schema-document-bridge/api-review/evidence`. No screenshot or hosted-CI claim.

## Optional decimal child scope — 29 October 2026

Current scope, adoption, plan and owning/core canonical pages receive HTML/local-destination checks with unchanged Fytala CSS. Receipts are separate under `docs/schema-optional-children/evidence`. This documents a proposed slice; it is not screenshot, runtime feature or hosted-CI qualification.

## Optional decimal scope remote checkpoint — 29 October 2026

Current core/Schema canonical pages and both delivery entry points receive a separate HTML/local-destination check; the strict Schema docs checker is rerun. Original scope and earlier sealed evidence remain separately verifiable. Receipts are under `docs/schema-optional-children/delivery/evidence`. No screenshot or new runtime qualification is claimed.

## Optional decimal implementation — 29 October 2026

Current core/Schema canonical pages and the new implementation guide/dossier receive separate HTML/local-destination checks with unchanged Fytala CSS. Strict owning Schema docs checks also run. New receipts are in `docs/schema-optional-implementation/evidence`; original scope/bridge documentation receipts remain unchanged. No screenshot or hosted CI claim.

## Optional implementation review delivery — 29 October 2026

Current canonical and review pages receive a separate HTML/local-destination check and strict Schema docs check. Tested source identities are verified unchanged; earlier sealed dossier bytes are preserved. Review receipts live under `docs/schema-optional-review/evidence`. No screenshot or new full-engine test claim.

## Bounded repeated decimal scope — 29 October 2026

Current core/Schema canonical and new adoption/plan pages receive separate HTML/local-destination checks and the strict Schema docs checker. Source-identical qualification and sealed receipt preservation are verified; no fresh full engine/gate, screenshot or hosted-CI result is claimed. New receipts belong to `docs/schema-repeated-children/evidence`.

## Bounded repetition implementation — 29 October 2026

Current canonical and owning repetition/assessment guide pages receive HTML/local-destination checks with unchanged Fytala CSS plus strict Schema documentation checks. Raw runtime, source-preservation and documentation receipts are kept in `docs/schema-repeated-implementation/evidence`. No fresh full-core/gate, screenshot or hosted-CI result is claimed for the Schema-only candidate.

## Repetition review checkpoint — 29 October 2026

Current core/Schema canonical and review pages receive separate HTML/local-link and strict Schema documentation checks. Tested-source and original sealed evidence hashes are verified. New receipts are in `docs/schema-repetition-review/evidence`; no screenshot or fresh full-core/gate claim.

## Count assertion scope — 29 October 2026

Current canonical, owning adoption/plan and core snapshots receive separate HTML/local-link and strict Schema docs checks. Normative source representation hashes, reviewed code identity and previous sealed dossier preservation are verified. New receipts live in `docs/schema-count-assertions/evidence`; no fresh full-engine/gate, screenshot or hosted-CI result is claimed.

## Count assertion implementation — 29 October 2026

Current canonical and owning count/assessment guides receive separate HTML/local-destination checks with unchanged Fytala CSS and strict Schema docs checks. Native count receipts, original expectation mapping, source/configuration invariance and old dossier preservation are recorded under `docs/schema-count-implementation/evidence`. No fresh full-core/gate, screenshot or hosted-CI claim.


**29 October 2026 review:** Schema 0.8 count assertions reviewed: fresh 397/397 Schema tests pass; core 1.34.1 source and inherited engine gates remain unchanged. Next is the separately scoped presence-predicate implementation; exists/empty remain unsupported in this checkpoint. [Review and next scope](schema-count-review/README.md).


Current presence implementation — 29 October 2026: Schema 0.9/v5 supports bounded unprefixed exists(child)/empty(child) Boolean predicates, retaining count-only v4 and no-call v1–v3. Complete Schema qualification: 451 passes (54 new); build zero warnings/errors; native example and real preserve/strip consumer tests pass. Two prior refusal expectations are explicitly adopted, 395 unchanged. Core 1.34.1 and its source-identical engine qualification remain inherited. Current implementation is uncommitted/unpublished; next is review and checkpoint. Earlier scope-only/presence-unsupported notices are historical capture state. [Implementation and evidence](schema-presence-implementation/README.md).


**29 October 2026 review:** Schema 0.9/v5 presence predicates reviewed: fresh 451/451 Schema tests pass and all seven tested implementation source identities match. Core 1.34.1 remains unchanged with inherited engine/gate qualification. No further production correction was needed. Earlier uncommitted notices are historical capture state; checkpoint delivery resolves through the review document Git history. [Review](schema-presence-review/README.md).


Consumer-priority assessment (29 October 2026): qualify the actual XmlDb persistent-node adapter in an isolated source workspace next, then Braid authoring/preview compatibility. No inspected dependency requires more native assertion functions now. Native structural expansion needs a concrete schema/instance use case. Schema 0.9/core 1.34.1 and all prior qualifications remain unchanged; no consumer execution or dependency adoption is claimed. [Assessment and acceptance plan](schema-consumer-priorities/README.md).


Isolated XmlDb qualification (29 October 2026): committed adapter source builds unchanged against current core; 27/27 existing Xdm tests pass on both 1.26.2 and 1.34.1. All 44 bounded diagnostic observations agree, including 26 query pairs, seven persisted kind mappings and nested snapshot disposal. Unrelated consumer dirty work is excluded. No dependency adoption, binary/native/API acceptance or production fix. Next: isolated Braid source compatibility. [Qualification and limits](xmldb-compatibility/README.md).


Isolated Braid qualification (29 October 2026): all actual consumer source builds against current core without a production fix. Original full suites on 1.26.2/current 1.34.1 each report 1,872 passes/four exact-version failures; separate corrected/adapted identity follow-ups pass all four. No single green unchanged full-suite claim, actual dependency adoption or native desktop acceptance. Next: review/checkpoint the consumer-priority and both compatibility dossiers. [Qualification and limits](braid-compatibility/README.md).


Consumer qualification reviewed (29 October 2026): exact sealed receipts and test identities verified; XmlDb 27 passes per version/44 matching observations, Braid 1,872 passes/four version failures plus separate four passing identity follow-ups per version. No further production fix, dependency adoption or binary/native acceptance. Earlier uncommitted notices are historical; delivery resolves through the review document Git history. Next: separately scope unchanged-binary qualification. [Review checkpoint](consumer-qualification-review/README.md).


Unchanged-binary qualification scoped (29 October 2026): exact retained baseline/candidate assembly hashes and identities are inventoried. Next: frozen baseline controls then engine-only replacement, preserving consumer DLLs, hosts, manifests and original version assertions. No replacement or new consumer execution yet; source qualification is not binary compatibility. Core 1.34.1/Schema 0.9 and actual product pins remain unchanged. [Execution plan](binary-compatibility-scope/README.md).


Unchanged-binary qualification (29 October 2026): baseline controls pass XmlDb 27/Braid 1,876; engine-only replacement passes XmlDb 27 with identical 44 observations and Braid 1,872/four original version failures. All consumer/host/manifest bytes stay frozen; actual candidate loads and preview workers are verified. Additional public-contract probe shows Braid inspector/preview identity stays old pin despite loaded 1.34.1; transparent product adoption is not qualified. No production fix or dependency adoption. Next: review/checkpoint and separately resolve consumer runtime-identity policy before upgrade. [Results and limits](binary-compatibility-execution/README.md).


Unchanged-binary qualification reviewed (29 October 2026): raw counts/identities, frozen layouts and actual replacement loads verified. XmlDb 27/27 plus identical 44 observations; Braid baseline 1,876 passes, replacement 1,872/four original version failures. Braid advertised runtime identity remains old pin; transparent adoption is not qualified. No production fix/pin/release change. Earlier uncommitted notices are historical; delivery resolves through review Git history. Next: separately scope consumer identity/adoption policy. [Review checkpoint](binary-compatibility-review/README.md).
