**REQ-126 closure baseline and conformance skip triage — 26 October 2026:** [Closure plan](CLOSURE-PLAN.md) defines 13 open catalog-review/closure workpackages, with all 76 entries assigned to four 19-entry evidence batches. [Skip triage](CONFORMANCE-SKIP-TRIAGE.md) separately tracks six investigations. Fresh QT3 31,142/0/679 and gated qt4 8,436/0/1,894 remain unchanged; all 2,573 skips are inventoried and actual filters checked for 2,512 dependency cases. Twenty-six bounded executor probes yielded 6 passes/8 assertion failures/7 schema exceptions/5 skips outside gate accounting; applicability still governs admission. AC-01–04 remain partial, AC-05 maintained. Public 1.26.3, source, version and gate/filter policy are unchanged. Earlier checkpoints below are historical.

**1.26.3 published and verified — 26 October 2026:** [Publication proof](../RELEASE-1.26.3-PUBLICATION.md) pins source `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f`, one Release run with 21,798 solution passes/ten uploads, ten verified signed public packages, and fresh nuget.org-only 153 broad/11,175 focused checks. All 19 assembly identities and signed archive/cache/output provenance match; both dependency audits are clear. All 3,061 added integer-preservation/audit cases are available. Original evidence and the bounded Braid local 1.26.2 adoption snapshot remain preserved; REQ-126 stays partial and no 1.26.3 adoption/native acceptance is inferred. Earlier candidate/unpublished notices below are historical.

**1.26.3 candidate validated — 26 October 2026:** [Pinned source/package proof](../RELEASE-1.26.3-CANDIDATE.md) consolidate abs and floor/ceiling integer identity fixes with the completed argument audit at `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f`. Fresh isolated checks pass 21,882 source/LSP tests, unchanged frozen gates, 153 broad and 11,175 focused package checks, including all 3,061 added source cases unchanged. Ten unsigned packages and 19 consumed identities match source/archive/cache/output provenance; both fresh dependency audits are clear. The 802 contract-audit cases required no engine correction. Public 1.26.2 and its previously captured bounded Braid adoption evidence remain current; no 1.26.3 adoption/CI/native acceptance or publication is inferred. REQ-126 stays partial. Earlier notices below are historical.

**Floor/ceiling argument audit — 26 October 2026:** [Contract tests and evidence](FLOOR-CEILING-ARGUMENT-CONTRACT.md) add 802 passing controls for cardinality, recursive arrays, untyped/node conversion, required errors, numeric types, call forms and bounded static backwards-compatible behavior. All pass on the unchanged engine; no implementation correction is claimed. The 798-observation public probe has zero discrepancies. Fresh 21,882 solution/LSP tests pass with unchanged frozen gates. The unpublished 1.26.3 patch now adds 3,061 cases across abs, integer identity and contract audit; public 1.26.2 and its bounded Braid adoption snapshot remain current. REQ-126 remains partial; no new adoption/native acceptance or publication is inferred. Earlier notices below are historical.

**Floor/ceiling integer preservation — 26 October 2026:** [Bounded correction and evidence](FLOOR-CEILING-INTEGER-PRESERVATION.md) retain supported decimal-backed integer values and annotations unchanged in both functions. All 936 new cases pass (baseline: 168 failed); all 21,080 solution/LSP tests pass with unchanged frozen gates. This consolidates with abs into unpublished 1.26.3; public 1.26.2 remains current. Numeric calculations, atomization/conversion and metadata are unchanged; broader floor/ceiling argument contracts remain a separate audit. Braid's prior bounded local 1.26.2 adoption snapshot is retained, with no new adoption/CI/native acceptance inferred or Braid writes. REQ-126 remains partial. Earlier notices below are historical.

**Abs integer preservation and Braid adoption — 26 October 2026:** [Correction and evidence](ABS-INTEGER-PRESERVATION.md) widen Int64.MinValue before absolute-value negation and preserve existing decimal-backed integer identity, retaining plain decimal/floating/conversion behavior. All 1,323 new cases pass (baseline: 288 failed); 20,144 solution/LSP tests pass with unchanged frozen gates. Source 1.26.3 is unpublished; public 1.26.2 stays current. Braid's supplied local 1.26.2 adoption report records 1,876 Release passes, 246 XPath +72 preview cases and 280 overlapping signed-package checks. It explicitly remains uncommitted, with no current-adoption CI or added native UI/accessibility acceptance. No Braid edits. REQ-126 remains partial; general constructors/casts and idiv remain separate. Earlier notices below are historical.

**1.26.2 published and verified — 26 October 2026:** [Publication proof](../RELEASE-1.26.2-PUBLICATION.md) pins source `6a3372fc3b91e733b71a426cf40f26feaaeb3bcb`, one Release run with 18,737 solution passes/ten uploads, ten verified signed public packages, and fresh nuget.org-only 153 broad/8,114 focused checks. All 19 assembly identities and signed archive/cache/output provenance match; both dependency audits are clear. All 4,201 new rounding cases are available. Original evidence and the separate 12-expectation correction remain preserved; REQ-126 stays partial. Braid smoke completion is owner-reported, outcome/new adoption/native acceptance not supplied. Earlier candidate/unpublished notices below are historical.

**1.26.2 candidate validated — 26 October 2026:** [Pinned source and package proof](../RELEASE-1.26.2-CANDIDATE.md) consolidate extreme precision and exact rounding kernels at `6a3372fc3b91e733b71a426cf40f26feaaeb3bcb`. Fresh isolated checks pass 18,821 source/LSP tests, unchanged frozen gates, 153 broad and 8,114 focused package checks, including all 4,201 added source cases unchanged. Ten unsigned packages and 19 consumed assembly identities match source/archive/cache/output provenance; both fresh dependency audits report no vulnerable packages. The 600 actual historical rounding discrepancies are corrected; 12 mistaken expectations remain separately corrected with original receipts preserved. Public 1.26.1 remains current; no tag/release/upload or signed public verification. Braid smoke completion is owner-reported, with no supplied outcome/acceptance. REQ-126 remains partial. Earlier notices below are historical.

**Exact rounding kernels — 26 October 2026:** [Implementation and corrected evidence](ROUNDING-KERNELS.md) fix decimal scaling, exact floating midpoint decisions, signed zero and wide-integer result identity. All 2,880 new cases pass (same-test baseline: 1,110 failed), with 18,821 solution/LSP tests and unchanged frozen gates. The previous 612 diagnostic discrepancies comprise 600 corrected rounding observations and 12 wrong expectations, now corrected separately with original receipts retained. Both current diagnostic matrices are fully green. Source 1.26.2 remains unpublished; public 1.26.1 stays current. The owner reports Braid's smoke test finished; its outcome/acceptance has not been supplied. REQ-126 remains partial. Earlier notices below are historical.

**Extreme rounding precision — 26 October 2026:** [Audit, bounded correction and remaining findings](ROUNDING-PRECISION-BOUNDARIES.md) admit existing decimal-backed integer precision payloads and handle precision outside +/-1000 before narrowing/power calculations, preserving numeric types, signed zero and special values. Adds 1,321 passing cases (corrected baseline: 1,027 failed); all 15,941 solution/LSP tests pass with unchanged frozen gates. All 1,452 extreme observations match; 612 retained-kernel discrepancies stay explicit and unchanged. Source 1.26.2 is unpublished; public 1.26.1 remains current. The owner reports Braid's 1.26.1 smoke test is underway; no result/acceptance is inferred. REQ-126 remains partial.

**1.26.1 published and verified — 26 October 2026:** [Publication proof](../RELEASE-1.26.1-PUBLICATION.md) pins approved source `65417cdb3bff184ca9ff1f624582f55ca4681db9`, one Release run with 14,536 passing solution tests/ten uploads, ten verified signed public packages and fresh nuget.org-only 153 broad/3,913 focused checks with all 19 assembly identities and signed archive/cache/output provenance matching. Neither fresh dependency audit reports vulnerable packages. Atomic/rounding fixes and 2,420 new regressions/controls are available; REQ-126 remains partial and existing Braid 1.26.0 acceptance is unchanged. Earlier unpublished/candidate notices below are historical; no new Braid adoption/native acceptance is inferred.

**1.26.1 candidate validated — 26 October 2026:** [Pinned source and package proof](../RELEASE-1.26.1-CANDIDATE.md) records source `65417cdb3bff184ca9ff1f624582f55ca4681db9`, 14,620 source/LSP passes, unchanged frozen gates, ten unsigned packages and 153 broad/3,913 focused PackageReference-only checks. All 19 consumed assembly identities and archive/cache/output bytes agree; fresh dependency audits report no vulnerable packages. The 2,420 added source cases are copied byte-identically into the package consumer. Public 1.26.0 and bounded Braid acceptance remain current; REQ-126 remains partial. No tag, publication or new Braid adoption/native acceptance.

**Rounding argument conversion — 26 October 2026:** [Audit, correction and evidence](ROUNDING-FUNCTION-CONVERSION.md) enforce optional numeric cardinality and integer precision conversion for `round`/`round-half-to-even`, preserving existing coerced function names and supported static backwards-compatible behavior. Adds 1,514 passing cases; the unchanged baseline had 654 failures. All 14,620 solution/LSP tests pass; QT3 and frozen qt4 remain unchanged. The 1,494-observation comparison has 654 corrections and no sampled regressions. Source 1.26.1 remains unpublished, public 1.26.0/Braid acceptance is unchanged, and REQ-126 remains partial. Extreme precision/numerical kernels and broader coercion remain separate.

**Atomic function conversion/cardinality — 25 October 2026:** [Audit, correction and evidence](ATOMIC-FUNCTION-CONVERSION.md) enforce `abs`'s `xs:numeric?` argument contract at its implementation boundary, preserve named-function identity, and retain FOTY0013 for maps/function items in numeric-union conversion. Adds 366 passing cases; all 13,106 solution/LSP tests pass and frozen gates are unchanged. The 576-pair matrix now has zero sampled array/flat-reference discrepancies (36/12 corrected, no regressions). Remaining rounding contracts, metadata/coercion identity and broader conversion remain follow-up scope. Source 1.26.1 stays unpublished; public 1.26.0/Braid acceptance is unchanged; REQ-126 remains partial.

**Publication update - 25 October 2026:** [Bosak 1.26.0 is published and verified](../RELEASE-1.26.0-PUBLICATION.md), including the carried-forward 384 numeric cases. REQ-126 remains partial; D2/D3/D4 and applicable-evidence AC-03 remain open. Earlier candidate/public-version notices below are historical. No fixture/gate promotion or full current-draft conformance is claimed.

<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak XPath 4.0 feature catalog">
  <br><br>
  <h1>The Bosak XPath/XSLT 4.0 Feature Catalog</h1>
  <p>Structured reference for every XPath/XSLT 4.0 syntax decision in Bosak — where each feature comes from, why it exists, what it replaces, and what it costs</p>
</div>

**REQ-126 C03 complete — 26 October 2026:** [Constructor/type review](CONSTRUCTOR-TYPE-RECONCILIATION.md) reconciles eight entries and all 49 focus constructors. All 1,333 asserted controls match across 1,357 observations; 245 focused schema/API/host tests pass. Bounded nominal defaults/static fields are now evidenced. Generic wide casts, decimal precision, schema/host context and broader conversion remain explicit boundaries; C02's atomic-annotation defect remains open. Ten closure workpackages remain; C04 is next. No engine/test/example/version/gate change or acceptance waiver; skips/Braid/extension work stays deferred. Earlier checkpoint counts are historical.

**REQ-126 C02 complete — 26 October 2026:** [Function/options reconciliation](FUNCTION-OPTIONS-RECONCILIATION.md) reconciles 30 entries/74 function surfaces with explicit supported boundaries and 14 follow-up groups. Release build and all 21,798 solution tests pass; all 3,148 catalog examples pass separately. The 195 added profile observations include 118 matching controls, two unmet controls for the new `atomic-type-annotation(())` defect, and 75 diagnostics. Eleven closure workpackages remain; C03 is next. C02 closes review scope, not AC-01–04 or the retained defects. Skip fixes, Braid and extension work remain deferred. Original receipts and frozen gates are preserved; no engine/version/release change.

**REQ-126 C01 complete — 26 October 2026:** [Current inventory and ownership record](FUNCTION-INVENTORY-REFRESH.md) refreshes 316 names against published 1.26.3 and pinned current draft b4723ff5: 119 registered 4.0-only names (70 non-constructors, 49 focus constructors), plus 197 existing names. The retained inventory missed `array:members` and `array:of-members`. All 1,581 profile lookup observations completed without probe errors; four changed declarations and remaining semantic/default/empty/error reviews have explicit C02/C03/C06 owners. Twelve closure workpackages remain open; REQ-126 and AC-01–04 remain partial. Finish these packages before skip fixes, Braid or VS Code extension work. Engine, APIs, tests, version and frozen gates are unchanged; existing passing source/gate evidence is inherited. Earlier counts and checkpoint notices are historical.

**Numeric binary/range empty correction - 25 October 2026:** [Behavior and evidence](BINARY-EMPTY-ATOMIZATION.md) propagate empty atomized operands in six numeric paths and modern ranges. Adds 214 cases (168 failed before; all pass after), 12,740 solution/LSP passes and unchanged fresh frozen gates. Matrix discrepancies fall 84 to 36 (48 corrected, no regressions); function conversion remains separate. Existing source patch 1.26.1 is unpublished; public 1.26.0 has bounded Braid acceptance. REQ-126 stays partial.


**Recursive array singleton correction - 25 October 2026:** [Behavior and evidence](ARRAY-SINGLETON-ATOMIZATION.md) count atomic values after recursively processing array members. Adds 111 regressions (92 failed before; all pass after), 12,526 solution/LSP passes and unchanged fresh frozen gates. Independent matrix discrepancies fall 207 to 84 (123 corrected, no regressions); caller empty handling and function conversion remain separate. Existing source patch 1.26.1 is unpublished; public/Braid-validation 1.26.0 remains current. REQ-126 stays partial.


**Recursive array atomization audit - 25 October 2026:** [Findings and proposed scope](ARRAY-ATOMIZATION-AUDIT.md) add 72 passing controls and 576 diagnostic pairs (1,152 raw observations). Independent expectations identify 207 array-case discrepancies and 12 faulty processor flat references. All three profiles agree; recursive singleton cardinality, caller empty handling and function conversion are separate findings. All 12,415 solution/LSP tests pass. Engine/version unchanged; source 1.26.1 unpublished/public 1.26.0; REQ-126 remains partial.


**Empty unary atomization correction - 25 October 2026:** [Behavior and evidence](UNARY-EMPTY-ATOMIZATION.md) return empty after unary-minus atomization instead of converting an empty atomic result to integer. Adds 64 cases (21 failed before; all pass after), with 12,343 solution/LSP passes and unchanged frozen gates. Source remains unpublished 1.26.1; public/Braid-validation 1.26.0 stays separate. Shared recursive-array cardinality remains an explicit follow-up; REQ-126 stays partial.


**Repeated unary conversion correction - 25 October 2026:** [Behavior and evidence](UNARY-COERCION.md) remove unsafe double-negation elimination, preserving atomization, untyped-to-double conversion, numeric normalization and required errors. Adds 79 regressions (46 failed before; all pass after). All 12,279 solution/LSP tests and unchanged frozen gates pass. Source 1.26.1 is unpublished; public/Braid-validation 1.26.0 remains separate. REQ-126 stays partial.


**1.26.0 candidate validated - 25 October 2026:** [Candidate proof](../RELEASE-1.26.0-CANDIDATE.md) pins merged source `1f06efb4d938d3d8e12eb273e632a8fe15110041`, 12,200 solution/LSP passes, unchanged frozen gates and ten unsigned packages with 153 broad/1,493 focused package-only passes, 19 matching assembly identities and clear audits. Conditional Wrap/Edit/Unwrap is source-reviewed; REQ-138 remains In progress pending publication and consumer integration/native acceptance. Public 1.25.1 remains current; numeric 1.25.2 receipts remain separately pinned. No tag/publication.

Historical preparation/review checkpoints follow.

**1.25.2 candidate validated — 25 October 2026:** [Consolidated proof](../RELEASE-1.25.2-CANDIDATE.md) pins source `3785672d695cbc8467e1c027d5ce4304e9f92152`, 12,143 solution/LSP passes, unchanged frozen gates, ten unsigned local packages and 153 broad/1,429 focused package checks with verified provenance and clear audits. Public 1.25.1 remains current; no tag/publication. REQ-126 remains partial; REQ-138 design is separate.

**Exact idiv truncation — 25 October 2026:** [Correction and evidence](EXACT-IDIV.md) compute exact decimal-backed quotients before applying the existing Int64 bounds. Adds 132 direct/runtime regressions (48 failed before, all pass after); all 96 oracle observations now match the exact reference/range policy. FOAR0001/FOAR0002 codes, other dispatch and storage bounds remain intact. Source 1.25.2 unpublished/public 1.25.1; REQ-126 partial.


**Integer division precision audit — 25 October 2026:** [Evidence and proposed scope](IDIV-PRECISION-AUDIT.md) add 48 result-range controls and 96 exact-rational comparisons, exposing 48 in-range one-unit discrepancies while documenting precision qualifications. Recommend exact truncation for existing decimal-backed operands with the current Int64 result bounds preserved. Engine/version unchanged; source 1.25.2 unpublished/public 1.25.1; REQ-126 partial.


**Integer modulo correction — 25 October 2026:** [Behavior and evidence](INTEGER-MODULO.md) return integer zero for Int64 minimum modulo -1 and preserve integer identity for decimal-backed remainders. Adds 84 direct/runtime regression cases (45 failed before, all pass after). Genuine decimal/floating policies and `idiv` remain unchanged. Source 1.25.2 unpublished/public 1.25.1; REQ-126 stays partial.


**Division/modulo audit — 25 October 2026:** [Evidence and next correction](DIVISION-MODULO-AUDIT.md) add 72 supported controls and 84 observed diagnostics. Minimum/-1 modulo overflow and wide remainder type loss are the recommended next bounded correction; quotient narrowing and precision remain separate. Engine/version unchanged: source 1.25.2 unpublished, public 1.25.1. REQ-126 remains partial.


**Integer unary correction — 25 October 2026:** [Behavior and evidence](INTEGER-UNARY.md) fix Int64-minimum negation wraparound and retain decimal-backed integer identity in folded/runtime paths. Adds 48 cross-profile regressions (24 failed before, all pass after). Source 1.25.2 is unpublished; public 1.25.1 and Braid smoke/adoption remain separate. Division/modulo and wider storage work remain open; REQ-126 stays partial.


**1.25.1 release candidate validated — 24 October 2026:** [Consolidated proof](../RELEASE-1.25.1-CANDIDATE.md) pins exact source6081e70d,11,759 passing source/language-server tests, unchanged frozen gates, ten verified unsigned local packages and153 broad/1,045 focused package checks. Public1.25.0 remains current; no tag/publication. REQ-126 stays partial; generic wide storage/error and remaining audit limits remain explicit.



**Unsigned floating truncation — 24 October 2026:** [Correction and limits](UNSIGNED-FLOATING-TRUNCATION.md) truncate finite float/double operands before unsigned range checks, allowing(-1,0) to become unsigned zero. Adds60 constructor/cast/reference/castable cases; exact overflow thresholds and nonfinite errors remain intact. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 partial.



**Unsigned Decimal truncation — 24 October 2026:** [Correction and limits](UNSIGNED-DECIMAL-TRUNCATION.md) discard numeric fractions before unsignedLong range facets/backing selection, with48 constructor/cast/reference/castable cases. Fractional lexical forms still refuse; floating near-zero consistency remains separate. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 partial.



**Exact floating integer boundaries — 24 October 2026:** [Correction and limits](FLOATING-INTEGER-BOUNDARIES.md) reject the signed2⁶³/unsigned2⁶⁴ overflow thresholds and preserve accepted large unsigned magnitudes, with72 constructor/cast/reference/castable cases. Unsigned Decimal truncation and generic storage remain separate. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 partial.



**Integer constructor/cast audit — 24 October 2026:** [Evidence and next correction](INTEGER-CAST-AUDIT.md) add72 supported controls and150 observed diagnostics. Floating-point signed/unsigned boundaries silently admit out-of-range values; unsigned decimal truncation and generic storage/error policy are separate findings. No engine/version/release change. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 partial.



**Unix-time wide integer calendar correction — 24 October 2026:** [Behavior and limits](UNIX-DATETIME-WIDE-RANGE.md) report FODT0001 for valid decimal-backed integer epochs beyond the supported calendar, preserving frozen negative refusal and Experimental signed behavior. Adds34 source cases; current native Unix improves to22/2/0. Generic constructors/casts remain separate. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 partial.



**Checked integer arithmetic — 24 October 2026:** [Correction and limits](INTEGER-PROMOTION.md) prevent folded binary arithmetic wraparound and preserve integer identity through decimal-backed addition/subtraction/multiplication, with42 profile/call-path regression cases. General casts, unary/division and Unix-time wide conversion remain separate. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 stays partial.



**Wide-integer Unix-time investigation — 24 October 2026:** [Evidence and next fix](UNIX-DATETIME-WIDE-INTEGER.md) add24 boundary tests and74 observed diagnostics. Valid wide literals fail Int64 conversion before date arithmetic; constant folding also wraps while runtime promotion loses integer typing. Defects remain explicit; no engine/version/release change. Source1.25.1 unpublished, public1.25.0 accepted; REQ-126 stays partial.



**Experimental signed Unix epochs — 24 October 2026:** [Implementation and limits](UNIX-DATETIME-EXPERIMENTAL.md) record the opt-in correction, 27 new source tests, 11,407 passing tests and unchanged frozen gates. Source 1.25.1 is unpublished; public/Braid 1.25.0 remains current. REQ-126 stays partial.



**Unix timestamp boundary review — 24 October 2026:** [Contract and evidence](UNIX-DATETIME-CONTRACT.md) add52 source regression cases, UTC/default/call-path/calendar/2038 deadline checks and a fresh complete native-set comparison: frozen16/0/0; current20/4/0, exit2. Current negative-epoch semantics and wide-integer errors remain explicit adoption boundaries; a minimum-literal representation diagnostic is retained. All11,296 solution +84 language-server tests pass. Catalog remains76 entries /3,172 documentation checks; no engine/API/version/gate/fixture/release change. Source/public1.25.0 and Braid integration remain separate; REQ-126/D2/D3/D4/AC-03 stay partial.

**Experimental substring correction - 24 October 2026:** Owner-approved source **1.24.1 (unpublished)** normalizes absent `index-of-substring` patterns only in `XPath40Experimental`; frozen `XPath40` retains XPTY0004. [Contract and proof](INDEX-OF-SUBSTRING-CONTRACT.md) records38 API/two XSLT host controls, fresh QT3 31,142/0/679 and frozen qt4 8,434/0/1,896. Current native case021 remains an explicit frozen-profile failure; no retry/gate/fixture-policy change. Public1.24.0 and REQ-136 acceptance stay current; REQ-126 remains partial. No release or Braid change. Earlier decision-pending checkpoints are historical.

**Frozen map-to-element gate restored - 24 October 2026:** [Repair and process controls](MAP-TO-ELEMENT-GATE-REPAIR.md) correct the dormant name to `fn-map-to-element`. All224 configured sets execute; expanded frozen gate8,434/0/1,896 includes156 additional passes/two skips. QT3 remains31,142/0/679. A synthetic negative assertion now exits2 instead of exploratory exit0. No engine/API/version/fixture-pin/release change; current upstream remains unadopted. REQ-126 stays partial; REQ-136 remains accepted. Earlier gate counts and dormant-name observations are historical.

**Current upstream gate audit - 23 October 2026:** [Audit](CURRENT-GATE-AUDIT.md) corrects the documentation inventory:224 configured names,223 executed sets,one dormant `map-to-element` name. Frozen8,278/0/1,894 remains unchanged; current8,297/45/1,894 fails across8 sets. Supplemental `fn-map-to-element`: frozen156/0/2,current173/53/7; current exit0 is exploratory, not green. Documentation extraction now includes six dotted names; no engine, runtime gate, fixture pin or release change. REQ-126 remains partial; REQ-136 remains accepted. Next: repair the dormant gate against frozen fixtures before upstream adoption. Earlier218-count statements are historical.

**CSV index revision audit - 23 October 2026:** [Contract resolution](CSV-INDEX-CONTRACT.md) confirms upstream issue2849 already changed seven CSV assertions. Unchanged Bosak 1.24.0 passes124 frozen cases; a separate current-upstream diagnostic passes117 and fails7 (exit2), including selection and `get` indexes. No frozen fixture/assertion/gate or engine change. Next: compare all218 gated sets against a complete current snapshot before adopting the new CSV contract. REQ-126 remains partial; REQ-136 remains consumer-accepted. Earlier requests for upstream clarification are historical.

**CSV options/defaults review - 23 October 2026:** [Contract review](CSV-CONTRACT-REVIEW.md) supplies99 catalog checks and25 local-resource API tests, including complete supplier reports. Current76 entries,3,148 feature +24 chapter =3,172 checks. Source/public1.23.8 and Braid smoke remain unchanged; no engine/version/release/gate change. D2/D3/D4 and AC-03 stay partial. Earlier checkpoints below are historical.



**Public 1.23.8 - 23 October 2026:** The function default-mode fix and host/module profile coverage are [published and verified](../RELEASE-1.23.8-PUBLICATION.md). Current catalog:76 entries/3,073 documentation checks. REQ-126/AC-03 remain partial; Braid smoke/acceptance is separate. Earlier source/unpublished/PR checkpoints below are historical.

**Function default-mode correction - 23 October2026:** [Isolated source1.23.8 fix](../XSLT-FUNCTION-DEFAULT-MODE.md) makes omitted apply-templates modes in functions use the default mode.36 regressions pass; public1.23.7 unchanged. Module/profile examples are a separate follow-up. Catalog75 entries/2,959 documentation checks unchanged; REQ-126/AC03 remain partial. Earlier delivery checkpoints below are historical.


**Public1.23.7 - 23 October2026:** EQName bracket parsing, inline-record signature gates and numeric separator boundaries are [published and verified](../RELEASE-1.23.7-PUBLICATION.md). Current catalog74 entries/2,893 documentation checks; REQ-126/AC-03 remain partial. Braid dependency1.23.4 unchanged. Earlier source/unpublished checkpoints below are historical.

**22 October 2026 - EQName bracket parsing, source 1.23.5 (unpublished):** [Parser correction and evidence](EQNAME-BRACKET-PARSING.md) distinguish brackets inside namespace literals from real predicates in effective 3.0 and 4.0 patterns. Public release remains 1.23.4; REQ-126 remains In Progress, with D2/D3/D4 and AC-03 partial. Earlier limitation statements below are historical.

**Public1.23.4 — 22 October 2026:** Union-minimum and EQName URI-punctuation priority corrections are [published and verified](../RELEASE-1.23.4-PUBLICATION.md). Opening-bracket URI parsing remains separately limited. Earlier unpublished delivery records below are historical.

**22 October 2026 — EQName priority correction, source 1.23.4:** Effective 4.0 name-test priority ignores URI punctuation; earlier profiles retain their existing behavior. [Scope, regressions and validation](EQNAME-PATTERN-PRIORITY.md). Opening-bracket URI parsing remains a separate limitation. Public release remains 1.23.2; REQ-126 stays In Progress. Prior delivery records below retain their historical versions and outcomes.

**22 October 2026 — source 1.23.3:** 4.0 node unions now use the minimum alternative default priority, aligned with the pinned current draft. The two catalog reproducers now expect `competitor`; their prior 1.23.2 observations remain recorded. 3.0 dispatch, explicit priorities and single-rule 4.0 `next-match` are preserved. [Correction and validation](UNION-MINIMUM-PRIORITY.md). Public release remains 1.23.2; REQ-126 stays In Progress. Earlier delivery snapshots below are historical.

**REQ-126 compound-pattern review — 21 October 2026:** [Grammar mapping](COMPOUND-PATTERN-GRAMMAR.md) adds30 manifest checks, including six complete3.0/4.0 manual reports. Two rows are explicit implementation observations: source1.23.2 uses maximum union alternative priority, while the pinned draft requires minimum; [separate finding](UNION-PRIORITY-ENGINE-FINDING.md) records the normative outcome and correction scope. Catalog71 entries /2,767 checks. REQ-126 remains In Progress; D2/D3/D4 and AC-03 partial. No engine/API/harness/version/gate/release change; prior count snapshots are historical.

**REQ-126 type/pattern review — 21 October 2026:** [Current grammar mapping](TYPE-PATTERN-GRAMMAR.md) and [intake reports](examples/intake-types/README.md) add40 executable checks and one16-field structured-value pattern entry. Current catalog: **70 entries;2,713 feature +24 chapter checks =2,737**. Existing alias/record pages now describe bounded published1.23.2 XSLT support; older source/deferral logs are preserved separately. D2/D3/D4 and AC-03 remain partial; REQ-126 stays In Progress. No engine/API/harness/version/profile/gate/release change. Earlier count/status snapshots are historical.

**REQ-126 D3 grammar review — 21 October 2026:** [Switch/array/select mapping](SWITCH-ARRAY-SELECT-GRAMMAR.md) adds a16-field [xsl:select entry](xslt-instructions/xsl-select.md) and37 executable checks, including six full dispatch reports. Current catalog: **69 entries;2,673 feature +24 chapter checks =2,697**; four chapters and117 assigned function names unchanged. Public/source1.23.2 is verified. Instruction recognition, effective XPath version and host profile are distinct. REQ-126 remains In Progress; D2/D3/D4 and AC-03 remain partial. No engine/API/harness/version/gate change or release; earlier status/count snapshots are historical.

**Index-of empty-collation correction - 21 October 2026:** Unpublished source1.23.2 [uses the evaluation default collation for an empty third argument only in4.0](INDEX-OF-EMPTY-COLLATION.md).27 focused checks and10,331 full solution tests pass; XPath3.1 refusal, explicit strings/validation and all other equality behavior are retained. QT3 stays31,142/0/679; qt4 gated stays8,278/0/1,894. One catalog outcome is corrected; documentation totals remain68 entries/2,660 checks. Public1.23.0, REQ-126 In Progress and old PR132 remain unchanged. No release.


**REQ-126 equality/collation review - 21 October 2026:** [Five-family review](EQUALITY-COLLATION-CONTRACT-REVIEW.md) and [customer-reference matching](functions/customer-reference-matching.md) add58 executable checks, including six complete3.1/4.0 reports. Current catalog: **68 entries;2,636 feature +24 chapter checks =2,660**; four worked chapters and117 assigned names unchanged. Source1.23.1 remains unpublished; public1.23.0 unchanged. Two engine boundaries and compare keyword drift are explicitly separate observations, not supported draft contracts. REQ-126 remains In Progress; earlier counts/version statements below are historical.


**Primitive numeric coercion - 2026-10-14:** Source candidate 1.3.2 supports the permitted decimal/float/double casts in XPath and ordinary XSLT 4.0 function arguments, including defaults. Double-to-integer remains invalid. The previous broader changelog interpretation is corrected: [PR #32](https://github.com/qt4cg/xslt40-test/pull/32) retains its original 903 fixture and updates only the error expectation. [Contract and evidence](../XSLT40-PRIMITIVE-NUMERIC-COERCION.md). Published libraries remain 1.2.1 and extension 0.1.6; source 1.3.2 is unpublished. Earlier milestone entries below are historical.

<!-- The Bosak XPath/XSLT 4.0 Feature Catalog -->
<!-- Living document: one file per feature; index updated in the same PR as any entry. -->

> **Purpose:** A catalog, in the spirit of the Design Patterns template (Gamma, Helm, Johnson, Vlissides, pp. 6–7), of every XPath/XSLT 4.0 feature Bosak ships, defers, or rejects. **Audience:** engine users choosing which 4.x idioms to adopt, Bosak.Braid (which syntax it may generate per compatibility level), and contributors extending the engine. **Living document:** last updated 2026-10-24. **Governing REQ:** [REQ-126](../FEATURE_REQUESTS.md); adoption plan and stability tiers: [REQ-118 dossier](../REQ-118-xpath-xslt-40.md); experimental track: [REQ-123](../FEATURE_REQUESTS.md).

## Current coverage and acceptance

Current totals: **76 entries; 3,148 feature +24 chapter checks =3,172**, four worked chapters and117 assigned function names. Published engine1.23.8 includes the [function default-mode correction](../XSLT-FUNCTION-DEFAULT-MODE.md) and the [ordinary host/profile](HOST-PROFILE-BOUNDARIES.md) and [direct module/profile](MODULE-PROFILE-BOUNDARIES.md) coverage. Braid's1.23.8 smoke remains separate.

Reference entries: [numeric literals](operators-and-syntax/numeric-literals.md), [stylesheet versions and profiles](xslt-instructions/processor-profiles.md), and [module/profile propagation](xslt-instructions/module-profiles.md).

[The CSV options/defaults review](CSV-CONTRACT-REVIEW.md) adds99 runnable catalog checks and25 local-resource public API tests, including complete supplier-import reports. It enriches the existing CSV entry; it adds no engine/API/version/release change. The non-positive selection draft/test conflict and namespaced QName option policy are explicit follow-ups.

[The coverage reconciliation](COVERAGE-RECONCILIATION.md) maps18 bounded grammar/cluster families and inventories applicable evidence. [Focus syntax](FOCUS-FUNCTION-SYNTAX.md), [record contracts](RECORD-CONTRACTS.md) and [numeric literals](NUMERIC-LITERALS.md) have bounded deliveries; broader grammar, transitive graphs and option/coercion contracts remain open. D2/D3/D4 and AC-03 stay partial under the owner-approved applicable-evidence policy;28 historical adoption pins remain unavailable. Run `python docs/xpath40-catalog/audit-reconciliation.py` to regenerate the bounded inventory.

## Direction: a book and a working reference

This catalog is intended to develop into a book or reference guide. Readers should be able to follow chapters to learn how features solve document-processing problems, or look up a single entry while implementing a transformation. The catalog now contains 76 entries and four worked chapters. Further coverage of shipped functions and deferred draft decisions remains REQ-126 work.

Examples must reflect real work. Use synthetic but plausible fixtures: supplier invoices mapped into an ERP format, order lines grouped into a reconciliation report, XML manuals published as HTML, or transaction amounts accumulated into running balances. State the business problem before introducing the syntax, and explain why the feature helps. Publishing examples should preserve mixed content and cross-references; mapping examples should handle namespaces, repeated records, optional fields, and conversions.

A worked example must include representative input, a complete expression or stylesheet with its required context, expected output, the compatibility level, and at least one meaningful edge case. Declare simplifications, such as a reduced invoice vocabulary; a small fixture must not imply compliance with an industry standard. Pair new syntax with the earlier idiom where a migration comparison helps. Short syntax probes can supplement the worked example.

Worked-example stylesheets must be understandable independently of the chapter:
include a file contract and change history, helper purpose/parameter/return/edge
blocks, and template context/output/precondition blocks. Explain business choices
and non-obvious invariants rather than narrating each instruction. See the
[XSLT style-guide applicability note](../XSLT_STYLE_GUIDE.md) and the annotated
[invoice stylesheet](worked-examples/invoice-to-erp/invoice-to-erp.xsl).

Keep reader-facing explanations before engine implementation details. Chapter introductions and cross-links will connect the individual entries into a learning path; the feature index remains available for quick lookup. Runnable examples must meet REQ-126's engine-verification requirement before their results are presented as verified.

## Start with a worked chapter

[Supplier invoices to ERP XML](worked-examples/invoice-to-erp/README.md) is the first complete case: a 3.0 stylesheet, realistic inputs, expected principal/exception outputs, and seven executable checks. It includes a separate verified 3.1/4.0 map-destructuring comparison. Continue with [XML manual publishing](worked-examples/manual-to-html/README.md) and [order reconciliation](worked-examples/order-reconciliation/README.md). The [example runner](examples/README.md) verifies 2,743 focused catalog cases and migration baselines.

## External specifications and source documentation

These primary sources provide language definitions, migration baselines, draft
history, and upstream tests. Links and section anchors were checked on
2026-10-11. The 4.0 documents are evolving QT4CG drafts, not final W3C
Recommendations. Current links are navigation references; they do not prove that
Bosak adopted every feature or the latest semantics. Record the adopted draft
revision/date and Bosak delivery revision alongside feature-specific references.
When wording or numbering changes, retain historical disposition evidence and
compare the current draft explicitly.

| Area | Primary source | Use |
|---|---|---|
| XPath syntax and types | [XPath 4.0 draft](https://qt4cg.org/specifications/xquery-40/xpath-40.html) | Operators, bindings, grammar, coercion, and compatibility. |
| Functions and operators | [F&O 4.0 draft](https://qt4cg.org/specifications/xpath-functions-40/Overview.html) | Signatures, arguments, results, errors, and function examples. |
| Stylesheet language | [XSLT 4.0 draft](https://qt4cg.org/specifications/xslt-40/Overview.html), [changes since 3.0](https://qt4cg.org/specifications/xslt-40/Overview.html#changes-since-3.0), [incompatibilities](https://qt4cg.org/specifications/xslt-40/Overview.html#incompatibilities) | Instruction behavior and migration differences. |
| Data model | [XDM 4.0 draft](https://qt4cg.org/specifications/xpath-datamodel-40/Overview.html) | Atomic items, nodes, maps, arrays, and JNodes. |
| Serialization | [Serialization 4.0 draft](https://qt4cg.org/specifications/xslt-xquery-serialization-40/Overview.html) | Output methods, parameters, and serialization errors. |
| Stable expression baseline | [XPath 3.1](https://www.w3.org/TR/xpath-31/), [F&O 3.1](https://www.w3.org/TR/xpath-functions-31/) | Earlier-version syntax and migration examples. |
| Stable stylesheet/model/output baseline | [XSLT 3.0](https://www.w3.org/TR/xslt-30/), [XDM 3.1](https://www.w3.org/TR/xpath-datamodel-31/), [Serialization 3.1](https://www.w3.org/TR/xslt-xquery-serialization-31/) | Portable worked chapters and baseline contracts. |
| Draft source and discussions | [QT4CG specification repository](https://github.com/qt4cg/qtspecs) | Source history, issues, pull requests, and adoption decisions. |
| Upstream 4.0 tests | [qt4tests](https://github.com/qt4cg/qt4tests), [xslt40-test](https://github.com/qt4cg/xslt40-test) | XPath/XQuery versus XSLT evidence; distinguish upstream tests from Bosak gate membership. |
| Upstream 3.x tests | [QT3](https://github.com/w3c/qt3tests), [xslt30-test](https://github.com/w3c/xslt30-test) | Regression baselines and XSLT 3.0 conformance evidence. |

Specification links establish language meaning; Bosak source, named tests, and
conformance runners establish implemented coverage. Deferred/rejected entries
link current alternatives, while their historical disposition must cite the
relevant issue/PR or archived draft when that evidence is supplied.

## 1. How to read this catalog

Each feature is one file, written against the template below. Fields may not be omitted, but may be *empty by declaration* — see the field contracts.

### 1.1 Entry template

1. **Name and Classification** — feature name + tier (**Frozen 4.0** / **Experimental** / **Deferred** / **Rejected–Superseded** / **Rejected–Never Existed**), the spec section, and the REQ slice/PR that landed it (traceability — every entry links back to its delivery).
2. **Intent** — one paragraph: what the construct does.
3. **Also Known As** — spec PR/issue numbers and former names. The 4.x drafts rename things; this field is the antidote.
4. **History / Supersedes** — which 1.x–3.x idiom this replaces, with a migration pair. If the feature is genuinely new, declare: *"None — new capability; 3.1 workaround: …"* — emptiness must be informative, not accidental.
5. **Motivation** — the problem in 4.x terms: why the pre-existing idioms were not enough.
6. **Language Alignment** — prior art in other languages (F#, Haskell, JS, Python…), **including semantic-drift warnings** where XPath deliberately differs from the language it borrowed from (argument position, null-vs-empty, renames).
7. **Applicability** — when to use it; when *not* (library code targeting 3.1, hot paths, …).
8. **Structure** — the grammar fragment or syntax shape.
9. **Participants** — how Bosak models it: parser flag, AST node(s), opcodes, version-gate entry.
10. **Collaborations** — interactions with other features (e.g., destructuring × typed-let coercion × lexical scoping).
11. **Consequences** — benefits *and* liabilities, including any deliberate breaking change.
12. **Implementation** — files, versions, and where the version gate rejects (XPST0003 at 3.1, XPST0017, …).
13. **Sample Code** — a worked real-world case with input, complete code, required context, expected output, compatibility level, and a meaningful edge case; syntax probes supplement the case and cover each form.
14. **Performance Notes** — cost class (zero-cost desugar / small constant overhead / hot-path relevant), plus measured deltas **only** when they carry revision + date from `benchmarks/Bosak.Benchmarks` (BenchmarkDotNet). Ad-hoc timings do not belong here; the harness owns numbers.
15. **Known Uses** — the qt4tests sets that pin the behavior and their gate status.
16. **Related Features** — cross-links to other catalog entries.

For book-style reading, begin with Intent, Motivation, Applicability, and Sample Code, then use History, Consequences, and Related Features to evaluate alternatives. Participants, Collaborations, and Implementation provide a separate contributor reference. Authors should open with a practical scenario and link its runnable case before presenting engine details. The numbered fields remain stable for traceability; a chapter can introduce them in the order that best teaches the task.

### 1.2 Compatibility tiers

| Tier | `XPathCompatibility` | Meaning |
|------|----------------------|---------|
| **Frozen 4.0** | `XPath40` (40) | Stabilized draft features. Static XPST0003/XPST0017 at `XPath31`; legal at `XPath40`. |
| **Experimental** | `XPath40Experimental` (50) | Not-yet-stabilized additions (post-June-2026 churn-risk sections). XPST0017 at *both* 3.1 and frozen 4.0. Semantics may change as the draft moves. |
| **Deferred** | — | Spec sections still moving; deliberately not implemented. Entries exist so the decision is recorded. |
| **Rejected** | — | Superseded renames and shapes that never existed; implemented old shapes are a defect. |

The 3.1 default is untouched by every tier: consumers on `XPath31` see zero change.

### 1.3 Structural rules

1. One feature = one file, named short-kebab (no numbering — the index owns ordering).
2. Every entry links back to its REQ slice/PR (field 1).
3. The index table (§2) is updated **in the same PR** as any entry add, move, or tier change. No exceptions.
4. Entries are never deleted when a feature is rejected or superseded — the file moves to `rejected/` and the index records the disposition.

## 2. Index

The [completeness audit](COMPLETENESS-AUDIT.md) records acceptance gaps and the
prioritized completion plan, originally against the published 1.1.1 engine and
reviewed against 1.1.2 at that checkpoint. Current review evidence is linked above. Its
[function inventory](audit-function-inventory.json) distinguishes registered names
from fully documented signatures; the catalog remains In Progress.

68 entries now include practical cases or stable alternatives. The [2,636-case manifest](examples/cases.json) records exact files, modes, results, and errors. The catalog is not a claim of complete coverage of every shipped 4.0 function or every deferred draft shape.

### Operators and syntax

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [Destructuring let](operators-and-syntax/destructuring-let.md) | Frozen 4.0 | REQ-123 / PR #129 |
| [Method-call operator](operators-and-syntax/method-call-operator.md) | Deferred | REQ-118 deferred surface |
| [Otherwise operator](operators-and-syntax/otherwise-operator.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Pipeline operator](operators-and-syntax/pipeline-operator.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Mapping arrow](operators-and-syntax/mapping-arrow.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Keyword arguments](operators-and-syntax/keyword-arguments.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [String templates](operators-and-syntax/string-templates.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Array member iteration](operators-and-syntax/array-member-iteration.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Map entry iteration](operators-and-syntax/map-entry-iteration.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Focus-function syntax and pipeline focus](operators-and-syntax/focus-function-syntax.md) | Frozen 4.0 | REQ-118 S4; explicit applicable evidence, 30 new checks and paired order-line reports |
| [Focus-based constructors](operators-and-syntax/focus-functions.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |

### Functions

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [CSV parsing](functions/csv-parsing.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Element-to-map conversion](functions/element-to-map.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Map-to-element conversion](functions/map-to-element.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Atomic equality](functions/atomic-equal.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Comparator-based sorting](functions/sort-with.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Typed measurement comparison](functions/measurement-comparison.md) | Frozen 4.0 changed contract | Mixed numeric boundaries, 3.1 policy and keyword drift; 27 cases |
| [Batch extrema and component codes](functions/batch-extrema.md) | Frozen 4.0 changed contracts | Decimal batch range, QName endpoints, 3.1 folds and observed limits; 29 cases |
| [Warehouse pick sorting](functions/warehouse-pick-sorting.md) | Retained 3.1 / changed 4.0 contracts | Stable composite keys, array member payloads and observed limits; 28 cases |

### Map/array and sequence-selection families

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [Product indexing with maps](functions/product-map-index.md) | Frozen 4.0; source-1.1.2 result-shape correction | [#84](https://github.com/Fytala-Charles/Bosak/pull/84); 15 exact examples and named unit tests |
| [Import batches with arrays](functions/import-array-batches.md) | Frozen 4.0 | [#84](https://github.com/Fytala-Charles/Bosak/pull/84); 11 exact examples and named unit tests |
| [Order-export sequence selection](functions/order-sequence-selection.md) | Frozen 4.0 | [#82](https://github.com/Fytala-Charles/Bosak/pull/82); 14 exact examples and named unit tests |

These entries include complete synthetic inputs, exact output text, 3.1
comparisons and edge contracts. The map entry distinguishes published 1.1.1 behavior from the corrected
source-1.1.2 sequence contracts and the pinned draft. That historical delivery had **778 feature checks plus
24 worked-chapter checks (802 total)**; current coverage is2,636 feature +24 chapter checks. See the [audit](COMPLETENESS-AUDIT.md)
for remaining signature/grammar coverage and engine follow-up findings.

### Shipment history and sequence contracts

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [Shipment-event sequence checks](functions/shipment-event-sequences.md) | Frozen 4.0 | [#83](https://github.com/Fytala-Charles/Bosak/pull/83); 27 exact examples, named tests and pinned specification source |

This entry explains identity consistency, contiguous required steps and event
boundaries with complete carrier-history inputs and report outputs. The empty
history, missing identifiers/codes, unknown events, record comparators and
XPath 3.1 comparison make the acceptance contract explicit.

### Customer-reference equality and collation

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [Customer-reference equality and collation](functions/customer-reference-matching.md) | Existing3.1 functions; reviewed4.0 contracts | [REQ-126 review and executable evidence](EQUALITY-COLLATION-CONTRACT-REVIEW.md) |

### Type system

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [Enumeration types](type-system/enum-types.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Choice item types](type-system/choice-types.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Structural record types](type-system/structural-records.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Record updates with but with](type-system/record-updates.md) | Frozen 4.0 | REQ-118 S6b; cast/coercion distinctions, reconstruction and46 executable contract checks |
| [Typed let coercion](type-system/typed-let-coercion.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |

### Data model

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [JNode navigation](data-model/jnode-navigation.md) | Frozen 4.0 | REQ-118 / REQ-123; linked primary tests and executable cases |

### XSLT instructions

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [xsl:note](xslt-instructions/xsl-note.md) | Bosak XSLT 4.0 surface | REQ-118 / REQ-123; linked primary tests and executable cases |
| [xsl:if then/else](xslt-instructions/xsl-if.md) | Bosak XSLT 4.0 surface | REQ-118 / REQ-123; linked primary tests and executable cases |
| [Instruction separators](xslt-instructions/instruction-separators.md) | Bosak XSLT 4.0 surface | REQ-118 / REQ-123; linked primary tests and executable cases |
| [xsl:map selection and duplicate policy](xslt-instructions/xsl-map.md) | Bosak XSLT 4.0 surface | REQ-118 / REQ-123; linked primary tests and executable cases |
| [xsl:array and xsl:array-member](xslt-instructions/xsl-array.md) | Bosak XSLT 4.0 surface | REQ-118 / REQ-123; linked primary tests and executable cases |
| [xsl:select](xslt-instructions/xsl-select.md) | Bosak XSLT 4.0 surface | Text-body XPath; explicit host/version/type evidence |
| [Type/map/array patterns](xslt-instructions/structured-value-patterns.md) | Bounded XSLT4.0 surface | Key presence, member sequences, annotation and identity |
| [xsl:item-type](deferred/xslt-item-type.md) | Bounded published XSLT aliases | Enum/structural alias binding versus noncoercing tests |
| [xsl:record-type](deferred/xslt-record-type.md) | Bounded published XSLT support | Constructor/identity evidence; historical deferral retained |
| [Compound template patterns](xslt-instructions/compound-patterns.md) | Bounded XSLT4.0; observed priority gap | Grouping, rule identity and explicit application priority |
| [xsl:switch](xslt-instructions/xsl-switch.md) | Bosak XSLT 4.0 surface | REQ-118 / REQ-123; linked primary tests and executable cases |

### Experimental

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [fn:scan](experimental/fn-scan.md) | Experimental | REQ-123 S1 |

### Deferred

| Feature | Tier | Evidence |
|---|---|---|
| [XQuery and streaming scope](deferred/language-streaming-scope.md) | Separate 4.0 deltas | Existing query/streaming APIs; 28 scope and invoice checks |
| [External-input function boundaries](deferred/external-input-functions.md) | Out of scope; absent | Five contracts/ownership decisions; 55 refusal and narrower alternative checks |
| [Serialization 4.0 parameters](deferred/serialization-parameters.md) | Deferred, scoped XSLT solidus support | Nine parameter additions; 52 boundary and portable invoice checks |
| [fn:apply-templates](deferred/fn-apply-templates.md) | Deferred | REQ-118 tier 4; 28 refusal and invoice alternative checks |
| [Capturing accumulators](deferred/capturing-accumulators.md) | Deferred | REQ-118 tier 4; 16 capture identity and supported alternative checks |
| [Enclosing modes](deferred/enclosing-modes.md) | Deferred | REQ-118 tier 4; 20 refusal and named-mode dispatch checks |
| [Computed XPath node constructors](deferred/computed-node-constructors.md) | Deferred in XPath | REQ-118 tier 2; 30 refusal/alternative checks |

### Rejected / superseded

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [Mapping arrow =>>](rejected/mapping-arrow-gtgt.md) | Rejected / renamed | REQ-118 syntax audit |

### Added routing families and disposition records

| Feature | Tier | Delivery / evidence |
|---|---|---|
| [Higher-order selection](functions/higher-order-selection.md) | Frozen 4.0 | [#88](https://github.com/Fytala-Charles/Bosak/pull/88); named evidence and executable cases |
| [Consecutive partitioning](functions/consecutive-partition.md) | Frozen 4.0 | [#88](https://github.com/Fytala-Charles/Bosak/pull/88); named evidence and executable cases |
| [Ordered queue prefix](functions/ordered-queue-prefix.md) | Frozen 4.0 | [#88](https://github.com/Fytala-Charles/Bosak/pull/88); 18 exact queue and callback examples |
| [Bounded retry state](functions/bounded-retries.md) | Frozen 4.0 | [#88](https://github.com/Fytala-Charles/Bosak/pull/88); 25 exact state, budget and callback examples |
| [Product dependency closure](functions/product-dependency-closure.md) | Frozen 4.0 | [#88](https://github.com/Fytala-Charles/Bosak/pull/88); 15 exact graph and validation examples |
| [Configured label callbacks](functions/configured-label-callbacks.md) | Frozen 4.0 | [#88](https://github.com/Fytala-Charles/Bosak/pull/88); 22 exact binding and identity examples |
| [Month-end cutoffs](functions/month-end-cutoffs.md) | Frozen 4.0 | [#84](https://github.com/Fytala-Charles/Bosak/pull/84); 19 calendar, timezone and component examples |
| [Timestamp deadlines](functions/timestamp-deadlines.md) | Frozen 4.0 | [#84](https://github.com/Fytala-Charles/Bosak/pull/84); 17 unit, duration and budget examples |
| [Archive and partner URI policy](functions/uri-option-policy.md) | Frozen 4.0 | #84; 28 option and precedence cases |
| [Invoice API URIs](functions/invoice-api-uris.md) | Frozen 4.0 | [#84](https://github.com/Fytala-Charles/Bosak/pull/84); 28 component, encoding and integration examples |
| [Customer text cleanup](functions/customer-text-cleanup.md) | Frozen 4.0 | [#82](https://github.com/Fytala-Charles/Bosak/pull/82), [#83](https://github.com/Fytala-Charles/Bosak/pull/83); 22 cleanup/Unicode/padding cases and segmentation limits |
| [Import file names](functions/import-file-names.md) | Frozen 4.0 | [#83](https://github.com/Fytala-Charles/Bosak/pull/83); 13 delimiter/position cases |
| [Supplier offer ranking](functions/supplier-offer-ranking.md) | Frozen 4.0 | Boundary selection, stable multi-key sorting and collation preflight; 16 cases |
| [Numeric import diagnostics](functions/numeric-import-diagnostics.md) | Frozen 4.0 | NaN, atomic reflection and operator callbacks; 15 cases |
| [Payload fingerprints](functions/payload-fingerprints.md) | Frozen 4.0 | Explicit hash input and algorithm contracts; 10 cases |
| [Sequence identity checks](functions/sequence-distinctness.md) | Frozen 4.0 | [#83](https://github.com/Fytala-Charles/Bosak/pull/83); named evidence and executable cases |
| [Nominative record types](type-system/nominative-records.md) | Bounded XSLT host identity | Standalone declaration facility not implied; historical PR77 retained |
| [Iteration across collection sequences](operators-and-syntax/sequence-collection-iteration.md) | Frozen 4.0 | [#87](https://github.com/Fytala-Charles/Bosak/pull/87); named evidence and executable cases |
| [JNode model and template boundary](data-model/jnode-template-boundary.md) | Deferred remainder; partial Frozen 4.0 | [#116](https://github.com/Fytala-Charles/Bosak/pull/116); named evidence and executable cases |
| [Never-adopted quantifier names](rejected/obsolete-quantifier-functions.md) | Rejected | [#77](https://github.com/Fytala-Charles/Bosak/pull/77); named evidence and executable cases |
| [Directional scan names](rejected/directional-scan.md) | Rejected / superseded | [#77](https://github.com/Fytala-Charles/Bosak/pull/77); named evidence and executable cases |
| [XPath switch expression](rejected/xpath-switch-expression.md) | Rejected in XPath | [#77](https://github.com/Fytala-Charles/Bosak/pull/77); named evidence and executable cases |
| [Extensible map type proposal](rejected/extensible-map-types.md) | Rejected proposal; record alternatives shipped | [#77](https://github.com/Fytala-Charles/Bosak/pull/77); named evidence and executable cases |

The [provenance ledger](PROVENANCE.md) supplies exact Bosak delivery revisions and named tests for the 28 baseline entries; historical immutable adopted-specification snapshots remain unavailable.

## 3. Benchmark provenance

Measured performance claims in this catalog must cite `benchmarks/Bosak.Benchmarks` (BenchmarkDotNet) output and carry the git revision + measurement date. Qualitative cost classes (zero-cost desugar / constant overhead / hot-path relevant) need no citation but must be stated as expectations, not measurements. See field 14 contract in §1.1.

*Last updated: 2026-10-11: four complete worked chapters; 38 catalog entries; 104 catalog checks plus 24 chapter checks. Remaining feature inventory is still in progress.*

[Ordered queue prefix](functions/ordered-queue-prefix.md) supplies take-while/drop-while with complete input/output, an XPath 3.1 comparison and 18 boundary/callback cases. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public version remains 1.1.2. Bounded loop processing is now supplied by the reference below; dependency traversal and partial application are now supplied below.

[Bounded retry state](functions/bounded-retries.md) documents while-do/do-until using invoice-submission response fixtures, complete input/output, a bounded XPath 3.1 fold, and 25 executable normal/edge/error cases. It explains precondition versus postcondition evaluation, explicit attempt limits, zero-budget guarding, and whole-state callback values. Current coverage is 67 entries and 802 documentation checks (778 feature checks plus 24 chapter checks); 117 of 117 registered names have entries, leaving 0. REQ-126 remains In Progress; source/public version stays 1.1.2. Dependency traversal and partial application are supplied by this delivery.

[Product dependency closure](functions/product-dependency-closure.md) and [configured label callbacks](functions/configured-label-callbacks.md) add two complete references for transitive-closure, partial-apply and identity, with full input/output, XPath 3.1 comparisons and 37 executable cases. Shared dependencies, cycles, node identity/document order, unresolved references, argument order and callback binding limits are explicit. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public version stays 1.1.2.

[Month-end cutoffs](functions/month-end-cutoffs.md) and [timestamp deadlines](functions/timestamp-deadlines.md) add full scheduling references for build-dateTime, days-in-month, unix-dateTime, seconds and duration-to-seconds. Complete inputs/reports, XPath 3.1 comparisons and 36 executable cases explain Gregorian leap rules, numeric timezone offsets, stamp types, epoch milliseconds, signed/fractional durations and explicit budget boundaries. Coverage is 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public stays 1.1.2. URI parsing/construction and encoding are supplied by the reference below.

[Invoice API URIs](functions/invoice-api-uris.md) supplies parse-uri, build-uri and decode-from-uri with full request input/output, a scoped XPath 3.1 construction comparison and 28 executable cases. It explains segment boundaries, query form decoding versus literal plus, repeated keys, permissive malformed-input decoding, component precedence and textual roundtrip limits. Coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned names. REQ-126 remains In Progress; source/public stays 1.1.2. Unicode/text cleanup and substring references are supplied below.

[Customer text cleanup](functions/customer-text-cleanup.md) and [import file names](functions/import-file-names.md) add eight function names with full input/output, scoped XPath 3.1 comparisons and 35 executable cases. They distinguish codepoints from grapheme clusters and display width, preserve interior whitespace, and explain final-delimiter/overlap rules. Bosak grapheme segmentation is approximate; complete Unicode segmentation remains outside the verified coverage. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total), and 117 assigned / 0 unassigned registered names. REQ-126 remains In Progress; source/public stays 1.1.2. Ranking, numeric diagnostics and utility references are supplied below.

[Supplier offer ranking](functions/supplier-offer-ranking.md), [numeric import diagnostics](functions/numeric-import-diagnostics.md) and [payload fingerprints](functions/payload-fingerprints.md) add the final eight registered function names with 41 executable cases. Current coverage: 67 entries, 778 feature checks plus 24 chapter checks (802 total); all 117 registered names have family entries. This completes name assignment, not REQ-126 acceptance: signatures/options, grammar/disposition provenance and conformance limits remain. Computed-key boundary ties require explicit filtering in the business example and follow-up specification/engine reconciliation. Source/public stays 1.1.2; REQ-126 remains In Progress.

[Supplementary coverage audit](SUPPLEMENTARY-COVERAGE-AUDIT.md) (2026-10-12) records registered arities, changed existing-function contracts and wider grammar/disposition gaps. REQ-126 remains In Progress; current coverage is 67 entries and 802 checks. Typed measurement comparison with `fn:compare` is now supplied; min/max references are now supplied; sorting references are now supplied; URI option contracts and component precedence are now supplied; next: catalog close-out and prioritized follow-up review. Source/public version remains 1.1.2.

[Typed measurement comparison](functions/measurement-comparison.md) adds 27 executable cases for changed `fn:compare` behavior, with complete input/output and an explicit 3.1 double-normalization policy that differs at a decimal boundary. The runner verifies declared default-mode results or evaluation errors for existing functions. Keyword drift (`comparand1`/`comparand2` versus reviewed `value1`/`value2`) and legacy binary/empty-collation behavior are explicit. Current coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); the 117-name assignment is unchanged. REQ-126 remains In Progress; source/public stays 1.1.2. Min/max references are now supplied; sorting references are now supplied; URI option contracts and component precedence are now supplied; next: catalog close-out and prioritized follow-up review.

[Batch extrema and component codes](functions/batch-extrema.md) adds 29 executable cases for `fn:min`/`fn:max`: complete batch input/output, explicit decimal normalization, namespace/local-name QName boundaries and a 3.1 fold comparison. Mixed numeric promotion/result-type and plain-duration limits remain explicit observed gaps against the pinned draft; these fixtures do not claim conformance. Current coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 registered names remain assigned. REQ-126 remains In Progress; source/public stays 1.1.2. Sorting references are now supplied; URI option contracts and component precedence are now supplied; next: catalog close-out and prioritized follow-up review.

[Warehouse pick sorting](functions/warehouse-pick-sorting.md) adds 28 cases for retained `fn:sort`/`array:sort`: complete input/output, a portable 3.1 comparison, stable composite location keys and preserved multi-item array members. Mixed numeric promotion, input-dependent NaN order and missing empty-key defaults and unknown-collation acceptance are explicit observed boundaries. Exact fn-sort/array-sort gate membership is recorded without claiming full conformance. Coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. REQ-126 stays In Progress; source/public remains 1.1.2. URI option policy is now supplied; next: catalog close-out and prioritized follow-up review.

**Historical session close (2026-10-12, before URI review):** PR #173 merged after CI passed. Current reference coverage is 57 entries and 499 documentation checks (475 feature plus 24 chapter); all 117 registered names are assigned. Latest implementation validation: 4,497 tests passed across ten assemblies. REQ-126 remains In Progress; released/source version remains 1.1.2. Resume with URI options and component precedence; see [the handover](../AGENT_HANDOVER.md).


**URI option review — 2026-10-12:** [Archive and partner URI policy](functions/uri-option-policy.md) adds complete input/output, a scoped 3.1 comparison and 28 executable cases. Boolean options, UNC spelling, raw/structured authority and path/query precedence are explicit. Non-Boolean option truthiness and empty-query-map fallback remain observed draft gaps. Coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. REQ-126 stays In Progress; source/public version remains 1.1.2. Next: catalog close-out and prioritized follow-up review.

**Computed XPath constructor disposition — 2026-10-12:** [Deferred node construction](deferred/computed-node-constructors.md) separates the draft XPath feature from Bosak XQuery 3.1 support. Complete invoice input, refused XPath, 3.1 report and XSLT receipt output accompany 30 executable checks: 25 refusals across compatibility modes, three reports and two XML outputs. Current coverage: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. Version stays 1.1.2; REQ-126 remains In Progress. Named XSLT declaration dispositions are now supplied; enclosing modes are now supplied; capturing accumulator disposition is now supplied; fn:apply-templates disposition is now supplied; next: catalog close-out and prioritized follow-up review.

**Named XSLT declaration review — 2026-10-12:** [xsl:item-type](deferred/xslt-item-type.md) and [xsl:record-type](deferred/xslt-record-type.md) add full invoice cases, pinned disposition evidence and 28 executable boundary/alternative checks. Unused 4.0 declarations are ignored, not implemented; named bindings and constructors fail. Portable map validation and inline enums are verified. Structural-record XSLT `@as` raises XTTE0570; explicit XPath cast in `select` plus `as="map(*)"` is the verified workaround. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. REQ-126 remains In Progress, version 1.1.2. Next: catalog close-out and prioritized follow-up review.

**Enclosing-mode review — 2026-10-12:** [Deferred enclosing modes](deferred/enclosing-modes.md) supplies a full invoice export/audit case, pinned draft/source evidence and 20 executable checks. Nested mode templates refuse with XTSE0260; top-level named rules provide supported dispatch, without enclosing-rule isolation. Explicit mode, #current and lexical defaults, empty/reordered input and namespace identity are verified. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names remain assigned. Version remains 1.1.2; REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**Capturing-accumulator review — 2026-10-12:** [Deferred capturing accumulators](deferred/capturing-accumulators.md) adds full invoice input/output and 16 executable cases. Accepted capture=yes retains source identity rather than a draft-required snapshot; invalid/start-phase capture is tolerated in tested 4.0 forms. Ordinary accumulators, explicit snapshots and scalar projection are verified separately in memory, without streaming or special annotation-copy claims. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. REQ-126 remains In Progress, version 1.1.2. Next: catalog close-out and prioritized follow-up review.

**fn:apply-templates review — 2026-10-12:** [Deferred function and invoice alternatives](deferred/fn-apply-templates.md) adds 28 executable checks. Calls/references refuse with XPST0017; instruction availability is separate from function availability. Full invoice input/output verifies named-mode dispatch and a stylesheet-function wrapper with ordinary/tunnel parameters, empty/reordered input and explicit joining. Draft dynamic options/public-mode validation remain deferred. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**Serialization parameter review — 2026-10-12:** [Nine draft additions and invoice export](deferred/serialization-parameters.md) adds 52 executable cases. XSLT JSON solidus control works; fn:serialize ignores the new keys. JSON Lines, canonicalization, layout controls and CSV serialization remain deferred; fn CSV fallback and XSLT sequence-as-array are explicit boundaries. Complete portable JSON Lines input/output includes empty/reordered cases. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**External-input function review — 2026-10-12:** [Five function boundaries and partner invoices](deferred/external-input-functions.md) adds 55 checks. parse-html, html-doc, invisible-xml, unparsed-binary and xsd-validator remain absent/out of the original implementation scope. Pinned contracts and per-function dependency/ownership rationale accompany complete normalized XML, fixed-record, binary-value and business-guard alternatives. These do not implement HTML recovery, arbitrary grammar parsing, binary acquisition or XSD factory validation. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 registered names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**GNode/JNode scope review — 2026-10-12:** [Partial model and invoice navigation](data-model/jnode-template-boundary.md) replaces blanket exclusion with shipped JNode support and a deferred remainder. Eighteen added checks verify full invoice navigation/portable reports and broader node()/xnode()/identity gaps. Full generic-model and stylesheet built-in behavior remain uncertified. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 registered names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.

**Language/streaming scope review — 2026-10-12:** [Existing APIs and separate 4.0 deltas](deferred/language-streaming-scope.md) adds 28 cases through XQuery, XPath, ordinary XSLT and actual streaming APIs. Corrects stale XPath where exclusion and distinguishes REQ-087 Done from historical residual reports. Full XQuery/Streaming 4.0 remains unclaimed. Catalog: 67 entries, 778 feature plus 24 chapter checks (802 total); all 117 names assigned. Version 1.1.2, REQ-126 In Progress. Next: catalog close-out and prioritized follow-up review.


Catalog signature audit (2026-10-12): **67 entries, 1,791 feature + 24 chapter checks = 1,815**; four worked chapters and all 117 registered names assigned. REQ-126 remains In Progress, version 1.1.2. The [prioritized review](CLOSEOUT-REVIEW.md) identifies constructor/signature/grammar and acceptance work, with engine follow-ups tracked separately. All 49 focus constructors now have representative lexical/type/context/error checks and paired delivery-import examples. D2 now supplies a first metadata matrix and selected behavior checks; full contracts remain partial. Next: review missing keyword metadata and defaults/options family by family.


The [signature contract audit](SIGNATURE-CONTRACT-AUDIT.md) compares 68 new-name families and 20 selected existing functions, records 148 exact registrations and distinguishes availability from keyword/default/type/option behavior. Its invoice-fingerprint examples reproduce spelling, default and expression-shape boundaries. REQ-126 remains In Progress; no engine or release change.


The [text/Unicode contract review](TEXT-CONTRACT-REVIEW.md) adds 83 keyword/default/empty/invalid/option and full import-report checks for eight families. Their keyword metadata remains absent; 29 other absent-metadata families still need targeted review. D2 remains partial. Next: sequence selection/repetition contracts.


The [sequence selection/repetition review](SEQUENCE-CONTRACT-REVIEW.md) adds 65 checks and paired normal/empty/reordered batch exports. Five more absent-keyword families are reviewed; 13 of 37 now have targeted review and 24 remain, with metadata unchanged. slice keywords/defaults work in the sampled forms; typed-position/coercion boundaries remain explicit. Next: sequence equality/distinctness/subsequence predicates.


**Predicate contract review — 2026-10-12:** [Equality/distinctness/subsequence contracts and complete shipment reports](PREDICATE-CONTRACT-REVIEW.md) add 102 checks: 15 keyword/mode refusals, 79 positional/default/type/callback checks and eight normal/empty/reordered/duplicate-event reports in 3.1/4.0. Targeted absent-keyword review now covers 18 of 37 families, with 19 remaining; metadata remains absent. Map acceptance in atomic-signature equality functions and numeric/empty-string collation processing remain engine follow-ups. Current catalog: 67 entries, 1,723 feature + 24 chapter checks = 1,747. Version 1.1.2, REQ-126 In Progress, D2 partial. This snapshot supersedes earlier counts and next-step queues. Next: duplicate-values/highest/lowest/index-where comparison and selection contracts.


**Comparison/selection contract review — 2026-10-12:** [Four-family review and detailed warehouse reports](SELECTION-CONTRACT-REVIEW.md) add 68 checks: 12 keyword/mode cases, 50 focused default/type/callback/keyword-validation checks and six complete 3.1/4.0 warehouse reports. Three more absent-keyword families reviewed: 21 of 37 supplied, 16 remaining, metadata unchanged. index-where keywords succeed. Map atomization and extrema NaN ordering remain explicit follow-ups. Current catalog: 67 entries, 1,791 feature + 24 chapter checks = 1,815; version 1.1.2, REQ-126 In Progress, D2 partial. This snapshot supersedes earlier counts and queues. Next: map:entries/map:items/map:filter and array:build/array:items/array:empty container contracts.


**Container contract review — 2026-10-12:** [Six-family review and detailed stock-feed reports](CONTAINER-CONTRACT-REVIEW.md) 89 new checks: 18 keyword/mode refusals, 65 focused member/item/default/type/callback checks and six complete XPath 3.1/4.0 stock-feed reports. Targeted absent-keyword review: 27 of 37 families supplied, 10 remaining; metadata unchanged. map:filter accepts tested string/numeric predicate results despite its boolean result signature; dedicated conversion/enforcement review remains. Current catalog: 67 entries, 1,880 feature + 24 chapter checks = 1,904; four worked chapters, 117 names assigned. Version 1.1.3, REQ-126 In Progress, D2 partial; no engine/API/harness change or release. This snapshot supersedes earlier counts and queues. Next: build-dateTime, days-in-month, unix-dateTime, seconds and duration-to-seconds contracts.


**Date/time contract review — 2026-10-12:** [Five-family review and detailed receipt-retention reports](DATE-TIME-CONTRACT-REVIEW.md) 108 new checks: 15 keyword/mode refusals, 87 focused Gregorian/timezone/unit/type/cardinality checks and six full XPath 3.1/4.0 receipt-retention reports. Targeted absent-keyword review: 32 of 37 supplied, five remaining; metadata unchanged. Malformed string/multi-item tolerance, record conversion and fractional-component precision remain explicit follow-ups. Current catalog: 67 entries, 1,988 feature + 24 chapter checks = 2,012; four worked chapters, 117 names assigned. Version 1.1.3, REQ-126 In Progress, D2 partial; no engine/API/harness change or release. This snapshot supersedes earlier counts and queues. Next: build-uri, decode-from-uri, collation-available, identity and is-NaN.


**URI/import utility contract review — 2026-10-12:** [Five-family review and detailed import/link reports](URI-UTILITY-CONTRACT-REVIEW.md) 105 new checks: 15 keyword/mode refusals, 82 focused URI/option/decoding/collation/identity/NaN checks and eight complete XPath 3.1/4.0 import/link reports. All 37 originally absent-keyword families now have targeted review; zero await that first review, but all 37 still lack metadata. Boolean-option and record-field tolerance, empty query-map precedence and context-dependent collation boundaries remain follow-ups. Current catalog: 67 entries, 2,093 feature + 24 chapter checks = 2,117; four worked chapters, 117 names assigned. Version 1.1.3, REQ-126 In Progress, D2 partial; no engine/API/harness change or release. This snapshot supersedes earlier counts and queues. Next: exhaustively inventory changed existing 3.1 function contracts; the current matrix samples only twenty.


**Complete baseline-name inventory — 2026-10-12:** [Existing 3.1 contract inventory](CHANGED-31-CONTRACT-INVENTORY.md) The complete 197-name fn/math/map/array baseline inventory is now supplied, with dated W3C 3.1 declarations, pinned F&O e7ee4ab8 declarations and fresh 3.1/4.0 evidence. Added 353 checks: 320 exact-arity lookups, 17 named-reference refusals, ten focused defaults/behavior checks and six complete stock reports. Thirteen families have positional arity declaration differences; seventeen draft arities are unavailable in the fresh standard context. Type/prose differences remain candidates for semantic review, not certified behavioral changes. Empty substring/subsequence lengths have distinct observed 3.1 and 4.0 results. Empty sort keys and map:merge options refuse XPTY0004. Catalog: 67 entries, 2,446 feature + 24 chapter checks = 2,470; four chapters, 117 assigned names. All 37 reviewed keyword families still lack metadata. REQ-126 stays In Progress, D2 partial; version 1.1.3, no engine/API/harness change or release. Next: fn:sort, array:sort, fn:min, fn:max and map:merge defaults/conversion contracts. Prose classification, D3/D4, historical adoption pins and AC-03 remain open.


**Ordering/default review — 2026-10-12:** [Five-family review and supplier reports](ORDER-DEFAULT-CONTRACT-REVIEW.md) Targeted review of fn:sort, array:sort, fn:min, fn:max and map:merge adds 132 checks (126 focused, six complete supplier reports), preserving source declarations and exact 3.1/4.0/experimental keyword boundaries. min/max have no key callback parameter. Explicit empty sort keys and merge options, unknown sort collations, NaN ordering and malformed merge input/option cardinality remain engine follow-ups. Supplier rankings use numeric price plus stable ID and explicit duplicate combine; normal/reordered results match. Current catalog: 67 entries, 2,578 feature + 24 chapter checks = 2,602; four worked chapters, 117 assigned names. Version 1.1.3, REQ-126 In Progress, D2 partial. All 37 reviewed keyword families still lack metadata; no engine/API/harness change or release. Next: distinct-values, index-of, deep-equal, compare and contains-token equality/collation contracts. Earlier counts and queues are historical; normative prose impact, D3/D4, historical pins and AC-03 remain open.


**Select namespace follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) now supports expression-local XPath 4.0 namespace declarations in the XPath API and xsl:select bodies. Prefix bindings override host bindings; an optional default element namespace must come first. Default ##any matches unprefixed element axis name tests across namespaces while preserving attribute/kind-test boundaries and XML Schema item type resolution. Namespace contexts restore after success or failure. All unchanged native seqtor-140–146 cases pass; the broader seqtor-filter run is 83 pass/0 fail/9 skip. The nine known harness skips, independent confirmation and upstream PR #23 acceptance remain outstanding, so this does not establish complete XSLT 4.0 conformance. N02 and REQ-126 remain In Progress; public release remains 1.1.3. See [current namespace evidence](../../contributions/xslt40/select-boundaries/NAMESPACE_RESULTS.json) and the [pinned XPath namespace declaration source](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xquery-40/src/expressions.xml). Twenty-three regressions were added; catalogue entry/check counts are unchanged. Next: named item-type conversion and remaining record coercion gaps, after this slice passes CI. Earlier select counts are historical.


**Enumeration binding follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) now converts named enumeration aliases at XSLT variable, parameter and result boundaries. Namespace/EQName aliases and occurrence indicators resolve from the declaring instruction; atomization and membership use XPath function conversion rules. Direct/dynamic function metadata uses the same definitions; private package aliases are isolated and restored. Sixteen regressions cover valid values, membership/cardinality errors, source attributes, record fields, template/select/function boundaries and mixed-version refusal. The invoice currency fixture now returns EUR; invalid XYZ still raises XTTE0570. Original native type-alias-023/024 still pass; broader filtered family remains 3/29/0, with structural/recursive/schema/package gaps outstanding. N04 and REQ-126 stay In Progress; no complete alias or package-conformance claim. Public release remains 1.1.3. See [binding evidence](../../contributions/xslt40/item-type-boundaries/BINDING_RESULTS.json) and [XSLT 4.0 named item types](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types). Catalogue counts remain 67 entries / 2,578 feature + 24 chapter checks. Next: structural aliases and remaining named-record coercion, after CI. Earlier named-enumeration binding refusals are historical.


**Atomic and array binding follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) supports atomic and array item-type aliases at XSLT declaration and function boundaries. Array member types can use forward aliases, nested arrays, optional member sequences and enumeration aliases. Definition namespace bindings are preserved; outer occurrence indicators apply to arrays rather than their members. Supported direct/indirect recursive aliases raise XTSE4035. XPath 4.0 array coercion converts member sequences without atomizing the outer array; the XPath 3.1 path remains unchanged. Native type-alias-001/002 now pass; the filtered family is 6 pass/26 fail/0 skip, including unchanged contribution cases 023/024. Non-enumeration aliases in XPath instance-of/treat, general map/function/choice aliases, complete recursive validation, schema collisions and package exposure remain follow-ups. N04/REQ-126 remain In Progress. Public release remains 1.1.3. See [separate evidence](../../contributions/xslt40/item-type-boundaries/ARRAY_BINDING_RESULTS.json). Existing catalogue counts and expected outcomes are unchanged. Earlier slice results are historical. Next: structural aliases in XPath type expressions, other structural alias forms, then remaining named-record coercion.


**Atomic and array type-expression follow-up — 2026-10-13:** Source candidate 1.2.0 (unpublished) resolves the supported atomic and array xsl:item-type aliases in XPath instance-of and treat expressions, including AVTs and expression-local namespace declarations. Tests preserve alias occurrences and definition namespaces, distinguish type checking from conversion, and isolate private package definitions from same-named caller aliases. Mixed XPath 3.1 instructions still refuse these aliases. Fourteen new regressions pass; the public EnumerationItemTypes option retains its enum-only contract. General map/function/choice aliases, aliases nested in arbitrary XPath structural type syntax, complete schema/package semantics and remaining named-record coercion remain follow-ups. N04/REQ-126 remain In Progress. Public release remains 1.1.3. The native filtered family remains 6 pass/26 fail/0 skip; original 023/024 still pass. See [separate evidence](../../contributions/xslt40/item-type-boundaries/EXPRESSION_RESULTS.json). Catalogue counts and expected outcomes remain unchanged. Earlier slice results are historical. Next: other structural alias forms and nested references, then remaining named-record coercion.


**Typed map aliases and nested references — 2026-10-13:** Source candidate 1.2.0 (unpublished) supports typed map xsl:item-type aliases and nested alias references inside array/map types at XSLT binding/function boundaries and XPath instance-of/treat operands. Shared expansion respects declaration/use namespaces, occurrence indicators, quoted enumeration members, EQName URI delimiters, whitespace and nested comments. XPath 4.0 typed-map coercion recursively converts keys and values, rejects converted key collisions, preserves entry order and retains already-matching maps/record identity. The new coercion path excludes wildcard map types; their existing behavior is preserved. Twenty new regressions cover these boundaries and the documented sensor example. Function/choice/structural-record aliases, arbitrary type syntax, complete schema/package semantics and remaining named-record coercion remain follow-ups. N04/REQ-126 remain In Progress; public release remains 1.1.3. Broader native type-alias results remain 6/26/0; original 023/024 pass. See [separate evidence](../../contributions/xslt40/item-type-boundaries/MAP_RESULTS.json). Catalogue counts/outcomes and original native assertions remain unchanged. Earlier slice results are historical. Next: function and choice alias forms, then remaining named-record coercion.


**Function and choice aliases — 2026-10-13:** Source candidate 1.2.0 (unpublished) now expands function signatures and ordered choice xsl:item-type aliases, including nested references, parameter/result occurrences and outer function-item occurrences. Existing binding conversion wraps function results and tries choice alternatives in declaration order; instance-of/treat checks do not coerce. XPath 4.0 function instance-of now uses cardinality-aware matching, safely rejecting non-functions and handling empty sequences. Nineteen new regressions cover these boundaries, cycles, mixed-version refusal and a calibration example. Structural-record aliases, remaining named-record coercion and complete schema/package semantics remain follow-ups. N04/REQ-126 remain In Progress; public release remains 1.1.3. Native type-alias improves to 7/25/0: case 011 now passes; original 023/024 remain green. See [separate evidence](../../contributions/xslt40/item-type-boundaries/FUNCTION_CHOICE_RESULTS.json). Catalogue counts and original native expectations are unchanged; earlier slice notes are historical. Next: structural-record alias boundaries and remaining named-record coercion.


**Structural-record aliases — 2026-10-13:** Source candidate 1.2.0 (unpublished) now expands structural-record aliases and aliases nested in record fields, arrays, choices and function signatures. Quoted field names, default item()* fields, trailing commas, nested comments and declaration namespaces are preserved. Existing structural record coercion converts field values and annotates records; type checks require record values and do not coerce plain maps. Eleven new regressions cover the documented sensor example, nested contracts, function boundaries, invalid fields and cycles. Nominative record coercion, recursive named records and complete schema/package semantics remain follow-ups. N04/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](../../contributions/xslt40/item-type-boundaries/RECORD_ALIAS_RESULTS.json). Catalogue counts and original native assertions remain unchanged; earlier slice notes are historical. Next: nominative record coercion boundaries.


**Nominative record coercion — 2026-10-13:** Source candidate 1.2.0 (unpublished) now coerces maps to package-local nominative xsl:record-type declarations at variable and function boundaries. Conversion applies field types, missing-field defaults and static initializers, rejects surplus keys and supplied static fields, preserves declaration order and annotates the result with the target identity. Existing matching records are retained. Type checks still reject plain maps and distinguish same-shaped declarations. Nine new regressions cover sensor defaults, function references, cardinality, invalid bindings and identity; the previous valid-map refusal test now rejects surplus fields. Named casts, recursive named records, nested named-type/static namespace resolution and complete package exposure remain follow-ups. N04/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](../../contributions/xslt40/record-type-boundaries/COERCION_RESULTS.json). Catalogue counts remain unchanged; the named-record plain-map fixture now expects INV-2048. Original native assertions remain unchanged; earlier slice notes are historical. Next: named record cast and nested type resolution boundaries.


**Named record casts and nested references — 2026-10-13:** Source candidate 1.2.0 (unpublished) now supports package-local nominative record cast/castable operations. Casts retain map operands, cast each present field sequence, apply defaults to missing fields, discard surplus keys, protect static fields and attach the target declaration identity. Nested named references in binding/function signatures resolve in their declaration namespace, and nested named record fields reuse casting recursively. Fourteen new regressions cover lexical/type failures, empty/optional inputs, field cardinalities and namespace boundaries. General array/map casts, item-type alias cast expansion, recursive definitions, full package exposure and complete static diagnostics remain follow-ups. N05/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](../../contributions/xslt40/record-type-boundaries/CAST_RESULTS.json). Catalogue counts/outcomes and original native assertions remain unchanged. Earlier slice notes are historical. Next: remaining recursive/static type-resolution and cast-container boundaries.


**Finite recursive records and container casts — 2026-10-13:** Source candidate 1.2.0 (unpublished) now casts XPath 4.0 arrays/maps by casting member/key/value sequences, retaining container shape and checking cardinality. Map-key collisions raise XQDY0137; wildcard casts retain the supplied container and empty container types reject nonempty inputs. Named record field casts reuse the shared sequence caster. Record field metadata resolves aliases and finite self/mutual named references in the declaration namespace without expanding nominative identities. Sixteen new regressions cover finite linked lists, recursive array casts, nested containers, invalid members and collisions. Arbitrary-depth resource handling, full recursive/static diagnostics, item-type alias cast expansion and package exposure remain follow-ups. N05/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](../../contributions/xslt40/record-type-boundaries/CONTAINER_RESULTS.json). Catalogue counts/outcomes and original native assertions remain unchanged; earlier slice notes are historical. Next: item-type alias cast expansion and static named-type validation.


## Item-type alias casts and declaration validation — 2026-10-13

Source candidate 1.2.0 (unpublished) expands named item-type aliases in XPath 4.0 cast and castable targets, including nested container targets and namespace forms. Atomic, enumeration, array, map and structural/nominative record aliases reuse existing casting. Optional targets preserve empty sequences. Function and generic item/node aliases raise XPST0080 as casting targets. Unknown named references in supported alias definitions raise XPST0051 during stylesheet registration; declared record references preserve nominative identity. Eighteen new regressions pass. Full schema validation, unsupported alias grammar, unknown names outside declarations, choice-target eligibility and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress; published version stays 1.1.3.

See [cast and validation evidence](../../contributions/xslt40/item-type-boundaries/CAST_VALIDATION_RESULTS.json). Native type-alias remains 8/24/0; unchanged contributed cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Choice casting target validation — 2026-10-13

Source candidate 1.2.0 (unpublished) statically rejects XPath 4.0 cast/castable choices containing syntactically non-atomic alternatives, including arrays, maps, records, functions and node tests. Validation covers direct targets and expanded aliases, follows nested parentheses and preserves quoted separators and EQName text. Atomic/enumeration choices, optional inputs, ordinary container-valued instance-of choices and standalone container casts remain supported. Twenty-one new regressions pass. Schema-defined and nominative QName classification, unknown expression type names, broader alias grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress; published version stays 1.1.3.

See [choice casting evidence](../../contributions/xslt40/item-type-boundaries/CHOICE_CAST_RESULTS.json). Native type-alias remains 8/24/0; unchanged contributed cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Host static named-type validation — 2026-10-13

Source candidate 1.2.0 (unpublished) validates package-local named types in supported XPath 4.0 instance-of, treat, cast and castable targets when the XSLT host compiles the expression. Unknown direct/nested names raise XPST0051, including AVT expressions; undeclared prefixes retain XPST0081. Declared nominative record references are canonicalized to their expanded names, and record alternatives in choice cast targets raise XPST0080. Ordinary record-valued type-test choices remain valid. The internal host registry survives inline namespace declarations, recognizes built-in/XDM types and consults in-scope simple schema types. Twenty-one new regressions pass. Binding/field declaration validation, unsupported type grammar, complete schema variety/choice semantics and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress; published version stays 1.1.3.

See [static named-type evidence](../../contributions/xslt40/item-type-boundaries/STATIC_TYPE_RESULTS.json). Native type-alias remains 8/24/0; unchanged contributed cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Declaration static named-type validation — 2026-10-13

Candidate 1.2.0 now validates supported XPath 4.0 sequence-type names on XSLT @as declarations at stylesheet registration, including unused variables, parameters, function results and record fields. The check runs after package-local aliases and records are registered and traverses import/include modules with each declaration's namespace context. Unknown direct/nested names raise XPST0051; undeclared binding prefixes raise XPST0081; record registration retains XTSE0280 for unbound field prefixes. Forward aliases, finite recursive record references, default type namespaces and quoted enumeration/field names remain valid. Fourteen new regressions cover these boundaries. XPath 3.1 declarations keep their existing path. Unsupported shared-walker grammar, complete schema variety semantics and package type exposure remain follow-ups; N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; no release or gate promotion.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/DECLARATION_TYPE_RESULTS.json). Native type-alias: 9 pass/23 fail/0 skip; type-alias-019 newly passes and unchanged contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Node-kind alias grammar — 2026-10-13

Candidate 1.2.0 supports node-kind aliases whose tests have empty parentheses: element(), attribute(), text(), comment(), processing-instruction(), namespace-node() and document-node(). The shared type walker also accepts whitespace and nested comments around empty tests. These types can appear within supported array/map/record/function/choice types; adjacent unknown names now receive static validation instead of being hidden by an unsupported node-kind branch. Sixteen regressions verify node identity, occurrences, structural nesting, unknown names, binding failures and non-atomic choice cast rejection. Named/wildcard/schema-annotated node tests, fn/named-parameter function type grammar, full schema semantics and package type exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version remains 1.1.3; candidate 1.2.0 stays unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/NODE_KIND_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Named node-type alias grammar — 2026-10-13

Candidate 1.2.0 supports named element/attribute node tests, the complete * wildcard, and document-node(element(...)) inside package-local aliases and supported structural sequence types. Node names resolve in their declaration namespace and remain stable under use-site prefix/default namespace rebinding; unprefixed attributes retain the empty namespace. The host statically rejects unbound node prefixes and validates unknown type names beside named node tests. Explicit Q{}Name matching now checks both namespace and case-sensitive local name. Seventeen new regressions cover matching/mismatches, node identity, nested types, record fields, namespace defaults and static errors. Schema-annotated node tests, partial wildcards, processing-instruction targets, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/NAMED_NODE_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Processing-instruction target aliases — 2026-10-13

Candidate 1.2.0 supports processing-instruction targets in item-type aliases and supported structural sequence types. NCName and quoted-string targets retain case, normalize surrounding whitespace and are never expanded as type QNames. The parser validates targeted sequence-type syntax using a shared target reader; runtime matching compares the actual PI target. Malformed targets raise XPST0003 and quoted targets outside the NCName lexical space raise XPTY0004. Twelve new regressions verify identity, target/case mismatch, nesting, optional occurrences and static diagnostics. Partial wildcards, schema-annotated node tests, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/PI_TARGET_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Partial node wildcard aliases — 2026-10-13

Candidate 1.2.0 supports partial element/attribute name wildcards in aliases and supported structural types: *:Name matches an exact local name in any namespace; prefix:* and Q{uri}* match any local name in the specified namespace. Prefix wildcards retain declaration bindings under use-site rebinding, including document-node(element(...)) nesting. Unbound wildcard prefixes raise XPST0081; adjacent unknown type names retain XPST0051. The ##any declaration default now expands unprefixed element tests to *:Name while unprefixed attributes stay unqualified. Twenty new regressions verify identity, case/namespace mismatches, empty namespaces, nested arrays/documents and diagnostics. Node-name unions, schema-annotated tests, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/NODE_WILDCARD_RESULTS.json). Native type-alias improves to 11 pass/21 fail/0 skip: wildcard-type-001/002 newly pass and original contribution cases 023/024 still pass. Broader failures and independent processor confirmation remain outstanding.


## Node-name union aliases — 2026-10-13

Candidate 1.2.0 supports node-name unions in element/attribute item-type aliases and supported structural sequence types: element(doctor|nurse) and attribute(price|cost) match any listed name. Each alternative retains declaration namespaces and may use a partial wildcard, including nested document-node(element(...)), arrays and record fields. EQName namespace URIs containing pipe or comma characters remain intact. Empty alternatives raise XPST0003; an undeclared prefix in any alternative raises XPST0081; adjacent unknown type names retain XPST0051. Twenty-two new regressions cover positive/negative matching, identity, declaration prefix rebinding, wildcard combinations, nested types and ##any defaults. Schema-annotated alias traversal, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/NODE_UNION_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Schema-annotated node aliases — 2026-10-13

Candidate 1.2.0 supports schema annotations in element/attribute item-type aliases and supported structural sequence types, including node-name unions, nested documents, arrays and record fields. The annotation resolves independently against in-scope schema types and retains declaration namespaces under prefix rebinding; item-type aliases are not substituted into the schema annotation. Built-in and imported simple/complex schema types are accepted. Unknown annotations raise XPST0008; undeclared annotation prefixes raise XPST0081; malformed or empty annotations raise XPST0003. Only element annotations permit the nillable ? marker. The ##any default maps unprefixed schema annotations to the XML Schema namespace. Twenty-eight new regressions cover matching untyped and validated nodes, non-coercion, imported simple/complex types, nillability, namespace identity, nested contracts and unused declarations. fn/named-parameter function grammar, schema-element/schema-attribute alias traversal, document shorthand and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/SCHEMA_NODE_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Function-type grammar aliases — 2026-10-13

Candidate 1.2.0 accepts fn(...) as a synonym for function(...) and documentary parameter names in supported function-type aliases, direct XPath type operands and XSLT binding signatures. Names do not affect matching or coercion; duplicate expanded names raise XQST0039, including different prefixes bound to the same namespace and unprefixed names paired with Q{} names. Undeclared documentary prefixes raise XPST0081; malformed named parameters raise XPST0003; unknown parameter/result item types retain XPST0051. Named and unnamed parameters may be mixed, with independent name scopes for nested function types. Nested arrays, records and higher-order signatures retain parameter/result sequence occurrences. XPath 3.1 rejects fn type synonyms and named type parameters. Twenty-six new regressions cover callback conversion, wildcard maps/arrays, direct bindings without aliases, direct host evaluation, nesting, namespace identity, diagnostics and version boundaries. Annotated function types, schema-element/schema-attribute alias traversal, document shorthand and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/FUNCTION_GRAMMAR_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Schema-declaration and document shorthand aliases — 2026-10-13

Candidate 1.2.0 supports schema-element(Name), schema-attribute(Name) and document-node(schema-element(Name)) in item-type aliases and supported structural sequence types. Schema declaration names retain their declaration namespace under prefix rebinding and must resolve to imported global elements/attributes; unknown or unavailable declarations raise XPST0008 and undeclared prefixes raise XPST0081. Wildcards, unions and empty declaration names raise XPST0003. Schema-element under ##any uses no namespace for an unprefixed declaration; unprefixed schema attributes always use no namespace. Document shorthand such as document-node(root|other) normalizes to document-node(element(root|other)), including wildcard and EQName alternatives, and reuses single-root matching. Direct host type operands and alias-free XSLT document/schema bindings normalize the new forms. Twenty-nine new regressions cover node identity, namespace mismatches, imported declarations, substitution members, typed attribute matching, rejection of untyped nodes, nested arrays, ##any defaults, empty documents and static validation of unused bindings. Annotated function types, package exposure and broader native type-alias gaps remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/SCHEMA_DECLARATION_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Package named type exposure — 2026-10-13

Candidate 1.2.0 accepts item-type and record-type components in xsl:expose and xsl:accept. Public or exposed named types enter using-package type registries unless accepted as hidden; private dependencies retain the defining package's resolved alias definitions and record metadata. Accept names resolve namespaces on the accept element. Missing component names, incompatible acceptance, invalid declaration visibility and collisions with local types produce static errors. Seventeen regressions cover public declarations, explicit and wildcard exposure, private and hidden names, prefix scope, record constructors, private alias dependencies rejected collisions and public/private visibility through an intermediate package. Native type-alias improves from 11/21/0 to 15/17/0; newly passing cases are 018, 020, 021 and 022. Original contribution cases 023/024 remain green. Broader native failures and independent processor confirmation remain outstanding; N04/N05 and REQ-126 stay In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/PACKAGE_EXPOSURE_RESULTS.json). The [XSLT 4.0 named type and package rules](https://qt4cg.org/specifications/xslt-40/Overview.html) define these visibility boundaries.


## Fixed namespace type contexts — 2026-10-13

Candidate 1.2.0 resolves fixed-namespaces for named item/record type names, supported nested type references, static declaration validation and the main XPath compilation paths. Supported tokens include #standard, selected root-native or conventional prefixes, explicit prefix=URI bindings and namespace-document references through the existing stylesheet resolver. Bindings are module-local, cached at compilation, unaffected by descendant rebinding and applied in token order. Invalid bindings or unavailable namespace documents raise XTSE0122. Unprefixed type/node names retain their existing default-namespace rules; fixed bindings do not create native result namespaces. Sixteen regressions cover standard prefixes, nested node contracts, explicit/repeated bindings, namespace documents, include isolation and invalid/unavailable bindings. Native type-alias remains 15/17/0: case 016 now passes namespace resolution but still fails its independent inline-function record conversion contract with XPTY0004. No upstream assertion changes. Fixed namespace coverage of other QName-bearing XSLT attributes, pattern compilation and shadow/static processing remains a follow-up; this is not complete fixed-namespaces support. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/FIXED_NAMESPACE_RESULTS.json). The [XSLT 4.0 fixed-namespaces rules](https://qt4cg.org/specifications/xslt-40/Overview.html#fixed-namespaces) define the namespace token and scope rules.


## Inline function named type contracts — 2026-10-13

Candidate 1.2.0 expands host named types in inline function parameter and return contracts before IR lowering. Both function(...) and fn(...) retain resolved atomic, array, structural record and nominative record types, including occurrence indicators and nested member aliases. Signature resolution uses the expression static namespace context, including defaults, EQNames, inline namespace declarations and fixed bindings; the existing VM conversion rules apply to arguments and results. Unknown supported signature types are rejected statically even when the function is unused. Public EnumerationItemTypes registrations also resolve in inline signatures and remain restricted to single enum item types. Omitted annotations and XPath 3.1 instructions keep their existing behavior. Twenty-five regressions cover record/date updates, argument and return coercion, cardinality errors, namespaces, array members, record identity, AVTs, public enumeration boundaries and mixed-version refusal. Native type-alias improves from 15/17/0 to 16/16/0: case 016 now passes without changing its assertion; original contribution cases 023/024 remain green. Broader record matching, annotated function types, type patterns and remaining native failures stay outstanding. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/INLINE_FUNCTION_RESULTS.json). The [XPath 4.0 inline function rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-inline-func) define parameter and result contracts.


## Direct structural record bindings — 2026-10-13

Candidate 1.2.0 coerces direct structural record(...) and record(*) XSLT binding contracts through the existing XPath 4.0 converter. Supported direct record fields resolve aliases and node/type namespaces in their declaration context. Local/global variables and template parameters preserve record annotations and numeric field conversion; invalid values retain XTTE0570/XTTE0590 binding diagnostics. Required single-record and record-plus conversion now reject empty sequences in both empty-value representations, while optional/star records retain empty-sequence support. Plain maps still do not match annotated record instance tests; record(*) requires an existing annotation. Seventeen regressions cover local/global/default-parameter bindings, occurrence indicators, field aliases and node contracts, empty records, invalid values/extra fields, parameter diagnostics and required inline function returns. The local inline-record40 invoice example now verifies INV-2048|EUR instead of the previously observed XTTE0570. Native type-alias stays 16/16/0: case 050 now gets past XTTE0570 record coercion but still fails its independent result assertion. Upstream assertions remain unchanged. Type patterns, broader native record outcomes and annotated function types remain follow-ups. N04/N05 and REQ-126 stay In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/record-type-boundaries/STRUCTURAL_BINDING_RESULTS.json). The [XPath 4.0 structural record rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-record-types) distinguish coercion from annotated membership.


## Template type patterns — 2026-10-13

Candidate 1.2.0 supports standalone XSLT 4.0 template type patterns ~T with zero or more predicates, including parenthesized forms, named item types, structural records and nominative records. The matcher delegates membership to the existing XPath type tests and preserves numeric predicate, singleton focus and dynamic-error-as-nonmatch behavior. Template patterns compile with the declaring package type registry and applicable static namespaces, including fixed-namespaces. Default priorities cover item/node/function/map/array families, built-in atomic derivation depth, enumerations and supported choice types; predicate patterns have priority 0.5 and explicit priorities are retained. xs:anyAtomicType is now recognized in host-supplied static type contexts. Twenty-nine regressions cover these behaviors, invalid syntax, previous-profile rejection, quoted field names, EQName delimiters and package-private alias isolation. Native type-alias advances from 16/16/0 to 18/14/0: cases 008 and 009 now pass with their upstream assertions unchanged. Ordinary wildcard node patterns, full XSLT 4.0 union-pattern priority semantics, schema-defined atomic priority derivation and named-type/fixed-namespace authoring-validation contexts remain follow-ups. Non-template pattern slots have not gained the template declaring-type registry in this slice. Plain maps still do not acquire record annotations through pattern matching; cases 006/010/013 and case 050 result handling remain independent boundaries. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/TYPE_PATTERN_RESULTS.json). The [XSLT 4.0 type-pattern rules](https://qt4cg.org/specifications/xslt-40/Overview.html) and [default priorities](https://qt4cg.org/specifications/xslt-40/Overview.html) define matching and conflict resolution.


## Wildcard node patterns — 2026-10-13

Candidate 1.2.0 matches XSLT 4.0 element(...) and attribute(...) kind-test patterns using namespace wildcards (p:* and Q{uri}*), local-name wildcards (*:local), and supported name unions. Matching applies in standalone, explicit-axis and path steps while retaining schema type checks and distinct element/attribute default namespaces. The 4.0 priority table gives untyped partial wildcards -0.25, typed all-name wildcards 0, typed partial wildcards 0.125 and typed exact names 0.25; name unions use their highest member priority. Supported node type patterns and document-node wrappers reuse those priorities. Explicit priorities are retained, and predicates raise node-pattern priority to 0.5, including explicit-axis kind tests. Earlier profiles retain their existing matching and priority behavior. Node-type schema annotations now tolerate surrounding whitespace; 4.0 pattern EQName URI rewriting and kind-test argument splitting preserve embedded delimiters. Forty-one regressions cover matching/nonmatching namespaces and types, axes/paths, unions, priorities, malformed names, prior-profile compatibility and EQName delimiters. Native type-alias advances from 18/14/0 to 22/10/0: wildcard-type-003–006 newly pass with upstream assertions unchanged. Full XSLT 4.0 union-pattern priority/next-match semantics, other node-pattern priority forms, schema-defined atomic priorities, authoring-validation named-type/fixed-namespace contexts and non-template declaring-type registries remain follow-ups. Remaining record/result cases and annotated function types remain independent boundaries; plain maps do not gain record annotations through pattern matching. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/WILDCARD_PATTERN_RESULTS.json). The [XSLT 4.0 default priority table](https://qt4cg.org/specifications/xslt-40/Overview.html) in section 6.4.3 defines the supported kind-test priorities.


## Node union patterns — 2026-10-13

Candidate 1.2.0 keeps supported XSLT 4.0 node union patterns as one template rule, using the highest default priority of their alternatives even when a lower-priority alternative is the one that matches. xsl:next-match visits that rule once, and overlapping alternatives do not create competing rule identities. Earlier effective profiles retain separate rules and their original per-alternative next-match behavior. Explicit priorities remain unchanged. Priority calculation recognizes pipe and union keyword operators, recursively computes nested alternatives, and preserves strings, predicates and EQName URIs; parenthesized 4.0 matching retains the declaring static namespace/type context. Empty top-level alternatives are rejected with XTSE0340. Twenty-eight regressions verify maximum priority, single visits, duplicate alternatives, failing ambiguity modes, nested/default namespaces, explicit priorities, version overrides, delimiters and invalid alternatives. Native next-match (including si-next-match) remains 46/8/6, and type-alias remains 22/10/0 with all prior wildcard/type-pattern gains retained. Four next-match 4.0 assertions (021a/023a/025a/039a) reference version-2.0 stylesheets and therefore still select the earlier effective profile; their fixtures and assertions remain unchanged. Processor-level selection for those lower-version native fixtures is a separate harness/API boundary. Other node-pattern priority forms, broader pattern grammar, schema-defined atomic priorities, named-type/fixed-namespace authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/UNION_PATTERN_RESULTS.json). The [XSLT 4.0 node-pattern priorities](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.4.3, define the single-rule union behavior.


## Attribute and forward-axis priorities — 2026-10-13

Candidate 1.2.0 computes supported XSLT 4.0 attribute shorthand and forward-axis pattern priorities from the underlying node test. Named attributes (@A and attribute::A) and named element axis tests have priority 0; namespace/local wildcards retain -0.25 and untargeted kind tests retain -0.5. Named processing-instruction axis tests have priority 0 and now match their target in 4.0 child/self/descendant-style node-test compilation. Existing 4.0 typed kind-test priorities, explicit priorities, predicate priority 0.5 and single-rule union behavior are retained. Earlier effective profiles keep their original behavior. Axis recognition for priority and 4.0 matching ignores :: inside EQName URIs. Thirty-three regressions compare default priorities against explicit competitors, verify earlier-profile differences, target nonmatches, attribute-union next-match chains and positive/negative EQName URI cases. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior wildcard/type-pattern gains remain green and upstream assertions/fixture versions are unchanged. Intersect/except and bare predicate-pattern priorities, broader pattern grammar, lower-version native fixture processor selection, schema-defined atomic priorities, named-type/fixed-namespace authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/FORWARD_AXIS_PRIORITY_RESULTS.json). The [XSLT 4.0 node-pattern priorities](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.4.3, assign priorities independently of supported forward-axis prefixes.


## Set and predicate-pattern priorities — 2026-10-13

Candidate 1.2.0 computes supported XSLT 4.0 intersect/except pattern default priorities from the first operand, including mixed operators, parenthesized operands and union precedence. Bare predicate pattern `.` has priority -1; predicate patterns with one or more predicates have priority +1 even when their expressions contain path slashes or slash string literals. Earlier effective profiles retain their existing behavior. Thirty-four regressions compare explicit competitors, check excluded-node and false-predicate nonmatches, atomic bare-dot matching, comments, adjacent punctuation, operator-name elements and EQName URI operator text. This is a priority calculation slice; broader pattern grammar and set-matching semantics remain separate follow-ups. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior wildcard/type-pattern gains remain green and upstream assertions/fixture versions are unchanged. Parenthesized item-pattern static validation, broader pattern grammar, lower-version native fixture processor selection, schema-defined atomic priorities, named-type/fixed-namespace authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/SET_PREDICATE_PRIORITY_RESULTS.json). The [XSLT 4.0 node-pattern priorities](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.4.3, use first-operand priorities for intersect/except; section 6.4.1 assigns bare-dot and filtered predicate-pattern priorities.


## Parenthesized item patterns — 2026-10-13

Candidate 1.2.0 admits supported parenthesized predicate and type patterns in the effective XSLT 4.0 profile and restricts those outer XNode patterns to nodes. Atomic values, maps and records therefore do not match a parenthesized item pattern; standalone predicate/type patterns retain their item matching. A prior local quoted-record-field regression incorrectly used a parenthesized type pattern for a map; it now uses the standalone type pattern, and new negative regressions verify the node boundary. Nested compilation retains declaring-package aliases, nominative type registries and applicable namespaces. Contained type priorities are recomputed after alias expansion, including wrapped union operands. Earlier profiles retain parenthesized-predicate rejection. Twenty-seven new regressions cover nesting, predicates, node-only membership, atomic/map negatives, aliases, default namespaces, contained priority and invalid direct item unions. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior wildcard/type-pattern gains remain green and upstream assertions/fixture versions are unchanged. Parenthesized outer-filter path classification, broader pattern grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, named-type/fixed-namespace authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/PARENTHESIZED_ITEM_PATTERN_RESULTS.json). The [XSLT 4.0 node-pattern priorities](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.4.3, preserve contained-pattern priorities; section 6.3.4 permits parenthesized item patterns within XNode patterns and restricts XNode patterns to nodes.


## Parenthesized outer-filter paths — 2026-10-13

Candidate 1.2.0 classifies supported XSLT 4.0 parenthesized outer filters using path slashes outside predicates, string literals and EQName namespace URIs. Patterns such as `(.[child/..])[true()]` keep candidate-node focus instead of incorrectly evaluating from the document root. Actual path patterns retain sequence-based positional filtering, including nested parentheses, descendant paths and paths whose EQName URIs contain slashes. Earlier effective profiles retain their existing classification. Twenty-five regressions cover candidate predicates, nested predicates, escaped quotes, URI slashes, aliases, real path positions and nonmatches. The initial twenty-one-case baseline had six failures before the fix; all final twenty-five cases pass. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Pattern delimiter scanning around quoted parentheses/brackets, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, fixed namespace/authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/PARENTHESIZED_FILTER_PATH_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.3.4.2.


## Pattern delimiter scanning — 2026-10-13

Candidate 1.2.0 matches pattern parentheses and brackets outside string literals and EQName namespace URIs, including doubled quote escapes. Predicate extraction, parenthesized compilation and template normalization/priority share the same lexical delimiter matcher. Static function-call validation ignores parentheses inside EQName URIs. Nineteen regressions cover quoted opening/closing delimiters, nested predicates, escaped single/double quotes, URI parentheses, contained priority, false outer predicates and ordinary earlier-profile node patterns. The baseline was 6/13/0 before the fix; all nineteen now pass. Lexical corrections apply to supported patterns across profiles; existing profile-specific grammar and priority rules remain intact. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Top-level operator splitting around quoted delimiters, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, fixed namespace/authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/PATTERN_DELIMITER_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 6.3.1 and 6.3.4.


## Pattern operator scanning — 2026-10-13

Candidate 1.2.0 splits supported top-level pattern operators outside string literals, doubled quote escapes, EQName namespace URIs and nested expressions. The shared scanner handles pipe unions and union/intersect/except keywords without splitting complete node-name tokens such as pre-except-post, pre_intersect_post or pre.union.post. Earlier-profile union rule expansion and type-choice priority splitting reuse the lexical scanner; existing profile-specific priority and grammar rules remain intact. Twenty-three regressions cover quoted delimiters/operator text, nested predicates, escaped quotes, URI punctuation, complete operator-like names, nonmatches and earlier-profile unions. The baseline was 17/6/0 before the fix; all twenty-three now pass. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Mixed intersect/except associativity, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, fixed namespace/authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/PATTERN_OPERATOR_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 6.3.1 and 6.3.4.


## Set-pattern associativity — 2026-10-13

Candidate 1.2.0 evaluates supported mixed intersect/except pattern chains from left to right in the effective XSLT 4.0 profile. Both operators have equal precedence: `root except missing intersect missing` does not match root, while `root except (missing intersect missing)` does. The matcher preserves parenthesized grouping, union precedence, quoted/URI operator text and existing all-path/all-axis expression evaluation. Missing operands in mixed chains raise XTSE0340. Earlier profiles retain their existing dispatch behavior. Eighteen new regressions cover membership, nonmatches, grouping, union precedence, URI/quoted punctuation, path/axis negatives and earlier-profile behavior. The baseline was 10/8/0 before the fix; focused new plus prior operator checks pass 41/0/0. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Forward-axis operand focus in 4.0 set patterns, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, fixed namespace/authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/SET_PATTERN_ASSOCIATIVITY_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 6.3.1 and 6.3.4.


## Set-pattern axis focus — 2026-10-13

Candidate 1.2.0 matches supported forward-axis operands in XSLT 4.0 intersect/except patterns against the candidate node instead of forcing axis-only chains into document-root XPath evaluation. Mixed axis/path operands use their respective pattern matchers; all-actual-path chains retain sequence evaluation and positional selection. The 4.0 single-pattern classifier reuses lexical path detection so slashes inside predicates or string literals do not turn an axis pattern into a path. Earlier profiles retain existing dispatch. Twenty-two regressions cover self/child/descendant-style axes, attributes, predicates, mixed actual paths, all-path positions, EQName URIs, first-operand priority and earlier-profile behavior. The baseline was 9/13/0 before the fix; all twenty-two now pass. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Fixed namespace resolution in 4.0 template pattern names, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/SET_PATTERN_AXIS_FOCUS_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 6.3.1 and 6.3.4.


## Pattern fixed namespaces — 2026-10-13

Candidate 1.2.0 resolves prefixes in effective XSLT 4.0 template patterns using the declaring applicable static namespace bindings. Fixed bindings now survive native rebinding in node names, attributes, axes, unions, set operands, nested type tests and predicates. Existing EQNames and quoted text are preserved. Without fixed bindings, native declarations remain applicable; earlier profiles retain their native lookup. Twenty-one regressions cover these boundaries; the initial nineteen-test baseline was 3/16/0. Native next-match remains 46/8/6 and type-alias remains 22/10/0; upstream assertions and fixtures are unchanged. Broader pattern grammar, omitted-prefix diagnostics, non-template authoring/static contexts, schema-defined atomic priorities, lower-version native fixture processor selection, record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/PATTERN_FIXED_NAMESPACE_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 3.6.1 and 6.3.1.


## Pattern prefix diagnostics — 2026-10-13

Candidate 1.2.0 reports XPST0081 during compilation of effective XSLT 4.0 template patterns when a lexical QName or prefixed wildcard uses an unavailable prefix. This includes native prefixes excluded by fixed-namespaces and undeclared prefixes without fixed bindings. A shared XPath-lexer check covers node names, attributes, axes, paths, union/set operands, function/variable references and nested type patterns; string literals, nested comments and EQName URIs remain protected. Earlier-profile loading retains its existing behavior. Thirty-three regressions cover omitted bindings, Unicode and dotted prefixes, protected text, native bindings across 2.0/3.0/4.0 and a direct lexical check distinguishing spaced map-entry delimiters from prefix wildcards; the initial twenty-four-test baseline was 4/20/0. Native next-match remains 46/8/6 and type-alias remains 22/10/0; upstream assertions and fixtures are unchanged. This bounded check does not perform full eager pattern compilation. String-template interpolation checks, non-template authoring/static contexts, broader pattern grammar, schema-defined atomic priorities, lower-version native fixture processor selection, record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../../contributions/xslt40/item-type-boundaries/PATTERN_PREFIX_DIAGNOSTIC_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 3.6.1 and 6.3.1.
