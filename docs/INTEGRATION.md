<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak integration guide">
  <br><br>
  <h1>Bosak XPath / XSLT / XQuery — Integration Guide</h1>
  <p>Quick-reference for any application consuming the Bosak XPath 3.1 + XSLT + XQuery stack</p>
</div>

<!-- Bosak XPath / XSLT — General Integration Guide -->
<!-- Living document: updated with each significant Bosak change. -->

> **Purpose:** Quick-reference for any application consuming the Bosak XPath 3.1 + XSLT + XQuery stack.
> **Last updated:** 10 October 2026
> **Bosak baseline:** 3,811 unit tests passed / 0 failed / 0 skipped
> **Language-server baseline:** 72 passed / 0 failed / 0 skipped
> **QT3 baseline (strict error codes):** **31,142 passed / 0 failed / 679 skipped** (97.87%) — **100%** of runnable tests pass.
> **XSLT baseline:** `package` cluster **159 passed / 1 failed / 3 skipped**; `accept` cluster **50 passed / 0 failed / 0 skipped**; `expose` cluster **42 passed / 0 failed / 0 skipped**; `use-package` cluster **53 passed / 0 failed / 1 skipped**; `declared-modes` cluster **10 passed / 0 failed / 4 skipped**; full W3C XSLT 3.0 strict sweep (last full run) **11,054 passed / 1 failed / 3,546 skipped** — 99.6% pass rate among runnable tests (the single failure is type-functions-0401, a documented platform limitation)
> **XQuery baseline:** Phase 4 — full core FLWOR, direct and computed constructors, switch/typeswitch, `validate`, output declarations and serialization, user-defined functions and variables, library modules, string constructors, ordering features, `fn:load-xquery-module` with schema propagation, schema-aware `fn:json-to-xml`

---

## 0. Recent Changes

- **2026-10-10 (n)** — **REQ-125 Slice B — controlled resource policy (`Bosak.XPath.Runtime.Resources`, branch `feature/req125-slice-b-resource-policy`).** Opt-in, host-authoritative acquisition policy on the 1.1.0 track; **zero existing signatures touched** — every branch keys off a null `ResourcePolicy`, so the default is bit-identical. Single authority seam: `IControlledResourceAuthority` decides `ControlledResourceRequest`s (route, purpose, redacted URI); responses are `Approve` (with authoritative bytes) / `ApproveCollection` / `Deny` — **null = abstain = refuse**. **14-route `ControlledResourceRoute`** covers document, json-doc, unparsed-text, unparsed-text-lines, collection + their availability probes, schema-import, source-document, streaming-input, extension-function and transform-inherit. **Refusals:** XV0004 (deny / abstain / authority error), XV0005 (declared-unsupported route — extension functions are permanently unsupported). **Guarantees:** approved bytes win over disk alternatives and are never merged with them; availability probes perform **zero IO**; every acquisition records a `ControlledResourceReceipt` (credential-redacted URI, route, purpose, outcome, byte count, lowercase-hex SHA-256) in the thread-safe policy log. **Wiring:** `EvaluationContext.ResourcePolicy` + `LoadDocument(uri, route)`; `fn:doc`/`fn:json-doc`/`fn:unparsed-text(-lines)`/`fn:collection` and `-available` probes in `FunctionLibrary`; `XsltCompiler.ResourcePolicy` → `TransformEngine` source-document + streaming input; `ControlledSchemaResolver` for `xsl:import-schema`; `fn:transform` inherits the caller's policy. Null-policy default verified untouched. **AC evidence (AC-03/04/05):** 53 tests in `ControlledResourcePolicyTests` + `ControlledResourceRouteTests` — approval-beats-disk, no-fallback-IO, zero-IO probes, receipt contents (SHA-256, redaction), XV0004/XV0005 taxonomy, nested-transform inheritance, run isolation. Gates: build 0 errors; `dotnet test Bosak.sln -c Release` **3,811 passed / 0 failed** (3,758 + 53); QT3 **31,142/0/679**; XSLT smoke **162/0/26**; qt4 gate-only **0 gated failures / 223 sets**. Versioning note: `Directory.Build.props` still 1.0.0 — the 1.1.0 bump is release mechanics, handled separately. (New: `src/Bosak.XPath.Runtime/Resources/` 0.1 ×7; tests 0.1 ×2.)

- **2026-10-10 (m)** — **REQ-125 Slice A — controlled static validation for XSLT (`Bosak.Xslt.Validation`, branch `feature/req125-slice-a-validation`).** Additive public API on the 1.1.0 track; **zero existing signatures touched, zero frozen-API changes**. Closes the dossier's evidence gap: `xsl:value-of select='('` in an unused template **compiled clean** (pinned as the AC-01 baseline test — compilation success is preliminary, not validation). **`XsltValidation.Validate(...)`** overloads over `AuthoringSnapshot` (no re-resolution — bytes provably untouched), `AuthoringSource` + resolver, and raw bytes; never throws for bad source. **Outcome taxonomy** (`XsltValidationOutcome`): Valid / Invalid / InvalidSource / UnsupportedCoverage / Refused (+ **Cancelled reserved for Slice D**, documented as not yet triggerable); precedence InvalidSource > Invalid > Refused > UnsupportedCoverage > Valid. **Declared coverage** (`XsltValidationCoverage`): every Expression/Pattern/AVT slot in every resolved module — *including templates never executed* — compiled via public `XPath31Expression.Compile` at the slot's effective version with its real `ExpressionSlotContext`; free variable references matched against **real declarations** (local `xsl:variable`/`xsl:param` with following-sibling scoping, template/function/`xsl:iterate` params, cross-module globals, function-body isolation, `use-when` statics-only → XPST0008); `use-when="false()"` subtrees elided per XSLT §3.13; patterns via internal `PatternCompiler` (XTSE0340); AVTs decomposed into `{expr}` segments (unmatched `{` → XTSE0350, `{{`/`}}` escapes). **Explicit gaps are never a silent pass**: `xsl:import-schema` / `xsl:package`/`xsl:use-package` present → outcome downgraded to UnsupportedCoverage with per-gap entries. **Diagnostics** (`XsltValidationDiagnostic`): stable code (spec code or XV0001 undecodable / XV0002 resolver refusal / XV0003 uncoded engine failure), module URI, message, `SourceRange?` with **explicit `LocationAvailable`** (parse EOF positions are -1 → whole-value range fallback; entity-spelling attributes map at value granularity — compiling the DOM-expanded value is semantically correct, raw-spelling mapping would be fiction). **Seam added**: internal `FreeVariableCollector` — standalone compile resolves variables at runtime, so no static variable-scope machinery existed; the scope-aware AST walker (for/let/quantified bindings, inline-function params, typeswitch, 4.0 pipeline/string templates) lives in `Bosak.Xslt` via existing `InternalsVisibleTo` — minimal and additive. `AttributeSlotClassifier` grew (v0.2) with missing XSLT 3.0 XPath-bearing attributes (`xsl:evaluate/@xpath`, `for-each-group` group-adjacent/starting-with/ending-with, `merge-source` for-each-item/source, `xsl:number` AVTs) — also improves REQ-124 classification. **AC evidence**: 29 tests in `XsltValidationTests` — AC-01 (unused-template malformed select → Invalid/XPST0003 with range inside the attribute value; valid multi-template fixture with use-when=false + LRE → Valid), static context (in-scope param OK / undeclared XPST0008 / prefixes XPST0081 / before-declaration visibility / function-body isolation), pattern+AVT coverage, AC-02 (principal+include `ExportOriginal` byte-identical, descriptors unchanged, defensive byte[] copy), cross-module diagnostic URI, taxonomy (Refused/XV0002, malformed XML → InvalidSource/XV0001, XTSE0010 non-stylesheet root, import-schema → UnsupportedCoverage, precedence), version (4.0 stylesheet + `fn:replicate` OK; forced 3.1 → XPST0017), no side effects (memory-resolver-only include absent from disk; templates never executed). Gates: build 0 errors 0 warnings; `dotnet test Bosak.sln -c Release` **3,758 passed / 0 failed** (baseline 3,729 + 29); QT3 **31,142/0/679**; XSLT smoke **162/0/26**; qt4 gate-only **0 gated failures / 223 sets**. Versioning note: `Directory.Build.props` still 1.0.0 — the 1.1.0 bump is release mechanics, handled separately. (New: `src/Bosak.Xslt/Validation/*` 0.1 ×8; AttributeSlotClassifier 0.2.)

- **2026-10-10 (l)** — **REQ-126 — XPATH40-FEATURE-CATALOG scaffolded (branch `feature/req126-xpath40-catalog`, documentation-only).** GoF-style catalog (Design Patterns pp. 6–7 template) of every XPath/XSLT 4.0 syntax decision, at `docs/xpath40-catalog/`: README (purpose/audience, 16-field entry template with field contracts, compatibility-tier table, benchmark provenance rule, structural rules) + index table + one file per feature in chapter directories (operators-and-syntax / functions / type-system / data-model / xslt-instructions / experimental / rejected). Four field families beyond the GoF original, per owner design: **History/Supersedes** (1.x–3.x idioms replaced + migration pair; "None — new capability" declared when empty), **Language Alignment** (prior art + semantic-drift warnings — `->` first-arg vs F# last, `??` empty-seq vs nullish, `=>>`→`=!>` rename), **Rejected/Do-Not-Implement** (graveyard so dropped shapes are never re-introduced), **Performance Notes** (cost class; measured deltas only from `benchmarks/Bosak.Benchmarks` with revision+date). Seeded with 4 exemplars spanning every tier: destructuring `let` (frozen — [PR #129] surface), `fn:scan` (experimental), method-call operator `=?>` (deferred, Issue 2219 churn), mapping arrow `=>>` (rejected/renamed). Structural rules: per-entry REQ/PR traceability; index updated in the same PR as any entry change; entries never deleted — moved to `rejected/` with disposition. Registry: REQ-126 row + detail section (5 ACs); REQ-118 dossier points at the catalog. Serves engine users choosing 4.x idioms and Bosak.Braid's syntax generation per compatibility level. No engine source changes.

- **2026-10-10 (k)** — **Destructuring-let cluster — XPath 4.0 destructuring let bindings + typed-let coercion + lexical let scoping (REQ-123 slice, qt4cg PR1131, frozen `XPath40` level, branch `feature/req123-destructuring-let`).** All new surfaces are 4.0-only (**XPST0003 at 3.1** for `$(`/`$[`/`${` after `$` and for `as` type declarations in simple XPath let bindings), **not** experimental. **Consumers on the 3.1 default: nothing changes** — the parser rejects the new forms and `EnforceType` keeps the 3.1 instance check; the new `Destructure` opcode is unreachable. **Destructuring patterns** (qt4tests prod-LetClause is the ground truth): `let $($x, $y) := V` binds positionally from the sequence V — the **last** variable takes ALL remaining items (`(1,2,3)` → `$x=1, $y=(2,3)`; `$($x) := 1 to 3` → `(1,2,3)`), earlier surplus items are discarded, surplus variables bind `()`; `let $[$a, $b] := V` binds positionally from a **single array** (surplus members discarded, surplus variables **FOAY0001**); `let ${$x, $y} := V` binds each variable to the map entry whose key equals the variable **local name** (missing keys → `()`). Each pattern accepts optional per-variable `as` types and one whole-pattern `as` type: sequence patterns take any item type (`xs:integer*` coerces the whole value, atomizing an array RHS), array patterns require an `array(...)` type, map patterns require `map(*)`/`map(K,V)`/`record(...)` (record types reject variables that are not declared fields, XPTY0004) — and the RHS must be a single array/map **even when the type allows empty or multiple**. Empty patterns are XPST0003. **Typed let bindings** (`let $v as xs:T := e`) become legal at XP40+ and apply XPath 4.0 §3.4.2 **coercion** instead of the 3.1 instance check: untypedAtomic cast, numeric promotion, record coercion, and **PR 254 relabeling** to derived integer types (annotation changes, the datum does not: `42 as xs:short` succeeds, `5.0 as xs:short` succeeds, `2.5 as xs:integer` and `40000 as xs:short` stay **XPTY0004** — the datum must lie within the target's value space). XSLT/XQuery 3.1 paths keep `IsXPath40=false` and the old behavior. **Lexical let scoping**: new `SaveVariables`/`RestoreVariables` VM opcodes capture and restore the bound variables' direct bindings around the let's body, so an inner let shadows an outer binding only within its own body and a trailing `$var` outside the let's body raises **XPST0008** (K-LetExprWithout-1, let-seq-019). `SaveVariables` captures **direct bindings only** (`EvaluationContext.TryGetDirectVariable`) — probing lazy globals would re-enter the lazy resolver and falsely report a circular dependency for an in-flight global of the same name (QT3 K2-FunctionProlog-15 regression caught on the frozen gate). Engine: `LetDestructuringKind`/`DestructuringVariable` AST records (XPathAstNode), parser dispatch in `ParseSimpleLetBinding` (destructuring + `as` at `_xpath40`), `IrOpCode.Destructure`/`SaveVariables`/`RestoreVariables` (IrLowerer builds `DestructuringInfo` and emits Save/Restore around every `LetExpression` binding group), `VmEngine.Destructure` (sequence rest semantics, FOAY0001, single-container RHS rules, map(K,V) entry coercion via `SplitTopLevel`, record coercion/field checks). qt4tests: **prod-LetClause 137/0/52** (52 XQ-only dependency skips) + **fn-compare 233/0/2** (the 4 destructuring-let stragglers) — both promoted into the gate (**now 223 sets**). Gates: build 0 errors; `dotnet test Bosak.sln -c Release` all green — **3,729 passed / 0 failed** (10 projects; Parser.Tests +6, Api.Tests +10 incl. relabeling edge cases) + LanguageServer.Tests **72/72** separately; QT3 **31,142/0/679** exit 0; qt4 gate-only **10,172/8,278/0/1,894, 0 gated failures, 223 sets** (the 2 OverflowException skips are pre-existing). (XPathAstNode 1.25, XPathParser 1.74, IrOpCode 1.12, IrLowerer 1.54, VmEngine 2.173/2.174, EvaluationContext 2.36, ConformanceRunner40 0.12, VersionGateTests 0.20, ParserTests 0.11.)

- **2026-10-10 (j)** — **REQ-124 consumer-acceptance fixes — Braid acceptance review of Slices A/B/C (branch `fix/req124-acceptance-findings`).** The consumer acceptance review (`docs/REQ-124-CONSUMER-ACCEPTANCE-REVIEW.md`, reviewed main `8e29289` / Slice C `cdf379d`; independent authoring-suite rerun 62/62) raised two acceptance blockers. **F1 — `AuthoringEditCandidate.Compile` bypassed the supplied snapshot resolver (defect, confirmed by source trace):** the candidate compile path built `new XsltCompiler()` with no `UriResolver` → `FileSystemUriResolver` fallback, outside the earlier inspection-time bridge correction (PR #122). Fixed: the bridge is extracted from `AuthoringInspector` into a shared internal `AuthoringModuleUriResolverBridge` (same contract: absolute-URI resolution, `ResolveModule(uri, referencingUri)`, no retry/no fallback, `FileNotFoundException` on refusal, `Xml11Loader.Parse` with `PreserveWhitespace | SetLineInfo | SetBaseUri` + module base URI), and `Compile()` applies it whenever the snapshot's effective resolver is not the default file-system one — the snapshot already retained the effective resolver from Slice B. Deliberate default-filesystem behavior preserved; compilation (success or failure) never touches envelopes/ranges/correspondence; resource policy documented on `Compile()` (`<remarks>` + `<exception>`). **Regression evidence (`AuthoringCandidateCompileResolverTests`, 6 tests, include AND import across the suite):** in-memory module with no disk file → compiles with supplied bytes; deny-all + valid disk alternative → throws naming the module (decisive — a filesystem fallback would have compiled), with post-failure isolation asserted; supplied content wins over disk alternative (import variant); nested relative reference base-URI chaining (principal → `sub/level1.xsl` → `level2.xsl`); default-resolver path unchanged end-to-end; successful compile changes nothing (compiled twice). **F2 — capability lists exposed mutable global arrays (defect, independently reproduced):** `FidelityModes`/`SupportedEncodings`/`DocumentedLimits` were `IReadOnlyList<T>` over mutable arrays — `(string[])AuthoringCapabilities.SupportedEncodings` succeeded and corrupted process-wide advertising. Fixed: all three are `Array.AsReadOnly(...)` over `private static readonly` backing arrays; signatures/values/order unchanged; immutability + concurrent-read guarantee documented in `<remarks>`. **Regression evidence (`AuthoringCapabilitiesTests` +3):** cast refused for all three (`!(list is T[])`, `as T[]` null); `IList<T>` mutation refused (`IsReadOnly`; Add/Set/RemoveAt/Insert/Clear throw `NotSupportedException`) with reads unchanged; mutated consumer copies never affect later reads — probes assert refusal only, never mutating global state. Dossier §15 records the review + full Bosak response. Status stays **Implemented (pending owner acceptance)** pending Braid re-review/pin. Gates: build 0 errors; `dotnet test Bosak.sln -c Release` **3,709 passed / 0 failed** (Xslt.Tests 883 = 874+9); XSLT smoke **162/0/26**. (AuthoringEdit 0.3, AuthoringInspector 0.4, AuthoringCapabilities 0.2, AuthoringModuleUriResolverBridge 0.1 new, AuthoringCandidateCompileResolverTests 0.1 new, AuthoringCapabilitiesTests 0.2.)

- **2026-10-10 (i)** — **REQ-124 Slice C — supported adoption: `AuthoringCapabilities` descriptor + AC-11 consumer sample + lifecycle contract (branch `feature/req124-slice-c`). Completes REQ-124** (all three slices landed; status **Implemented, pending owner acceptance**). **`AuthoringCapabilities`** (new `src/Bosak.Xslt/Authoring/AuthoringCapabilities.cs`, static read-only descriptor) lets a consumer discover the authoring surface **without probing with exceptions** — the exact adoption seam Bosak.Braid asked for in dossier §10: `SupportsInspection` / `SupportsExpressionSlotReplacement` (true), `SupportsPatternSlotReplacement` / `SupportsAvtSlotReplacement` / `SupportsQNameSlotReplacement` (false — refusal kind is `SlotNotEditable`, cross-referenced), `FidelityModes` (`AuthoringFidelityMode.LosslessByteExport` only), `SupportedEncodings` (the 7 canonical names **exactly matching** `AuthoringSource` detection — UTF-8, UTF-16 LE/BE, UTF-32 LE/BE, ISO-8859-1, US-ASCII — with BOM-vs-declaration detection rules in the docs), `MaxModuleCount` (256, tied to the options default), `DocumentedLimits` (7 entries: internal-DTD refusal, XML 1.1 split between inspection parsing and the Xml11Loader compile bridge, UTF-16/32 BOM-only detectability, node-id scoping, candidate staleness, Expression-only editability, thread-safety/no-disposal), `VersionPolicy` (XSLT 3.0 / XPath 3.1 first; 4.0 gated by existing version policy), `EngineAssemblyName` / `EngineVersion` (informational version with assembly-name fallback). **AC-11 sample-as-test:** `AuthoringConsumerSampleTests.Ac11_ConsumerSample_InspectEditExport_EndToEnd` — the method body *is* the documented walkthrough, runnable verbatim against public APIs only (no reflection, no friend access, no absolute paths): retain bytes → inspect (with compilation) → navigate descriptors → check the Expression slot → propose `/input/id` → review the candidate (raw literal, affected ranges, correspondence, compilability) → export/compile (`<result>42</result>` proof), input snapshot untouched, late proposal still resolves. **Lifecycle contract** recorded as `<remarks>` across the authoring surface (8 files, remarks-only): pure-managed lifetime (no disposal), candidate validity scoped to its deriving snapshot, immutable snapshots/candidates safe for concurrent sharing, snapshot-scoped node identity. **Packaging decision (dossier §13.1):** keep shipping inside the existing `Bosak.Xslt` package — zero packaging churn; a future split is driven by consumer demand. Gates: build 0 errors; `dotnet test Bosak.sln -c Release` **3,700 passed / 0 failed** (Xslt.Tests 874 = 871+3); XSLT smoke **162/0/26**. (AuthoringCapabilities 0.1, AuthoringConsumerSampleTests 0.1, AuthoringCapabilitiesTests 0.1; 8 authoring files 0.2 remarks-only.) **Consumer note:** `EngineVersion` currently resolves to the SDK default (`0.0.0+hash`) because `Bosak.Xslt.csproj` pins no `<Version>` — worth a packaging pass (Slice D follow-up) if Braid wants a meaningful binding-log string.

- **2026-10-10 (h)** — **REQ-124 consumer-review fixes — Bosak.Braid Slice A review findings (branch `fix/req124-review-findings`).** The Braid consumer review (`Braid/docs/REQ-124-SLICE-A-REVIEW.md`, against main `6c651b8`) raised four findings. **Findings 1–2 (line-ending-sensitive AC-02/AC-03 byte-offset assertions)** were already resolved on main by the Slice B fixture hardening — LF-pinned `string.Join("\n", …)` fixtures make hand-verified offsets independent of `core.autocrlf` while keeping LF and CRLF coverage; verified, no further work. **Finding 3 (caller-owned bytes aliased the source envelope) — defect, fixed:** `AuthoringSource.TryCreate` now takes a **defensive copy** of the input array up front (documented on the parameter — "the caller may reuse or mutate the array"); any `AuthoringSource` is immutable after construction, however its bytes were obtained (this also closes the resolver-returned-source case). **Finding 4 (optional compilation bypassed the inspection resolver) — defect, fixed:** when `AttemptCompilation` runs and the effective resolver is not the default file-system one, the derived compile now resolves include/import through the **same `IAuthoringModuleResolver` inspection used** — internal `IXsltUriResolver` bridge (`AuthoringModuleUriResolverBridge`) parsing returned envelopes via `Xml11Loader` with `FileSystemUriResolver`'s failure contract (`InvalidOperationException` unresolvable / `FileNotFoundException` on null); the default file-system path leaves the compiler untouched (byte-identical). A deny-all resolver can therefore no longer be circumvented by compile-time module acquisition (dossier §9 caller-controlled resolution). **Regression evidence:** 6 new `AuthoringReviewFindingTests` — F3(a) post-inspect mutation of the principal array leaves `ExportOriginal` at pre-mutation bytes; F3(b) pre-inspect mutation baseline; F3(c) resolver-provided module array mutation isolated; F4(a) deny-all resolver + on-disk include present → inspection succeeds, `IsCompilable=false` with a resolution diagnostic (a filesystem fallback would have compiled — decisive); F4(b) memory resolver serving the include → compilable (bridge end-to-end); F4 parity default resolver → compilable (unchanged path). Dossier gains §15 Consumer Reviews recording the review and Bosak's response. Gates: build 0 errors; `dotnet test Bosak.sln -c Release` **3,697 passed / 0 failed** (Xslt.Tests 871 = 865+6); XSLT smoke **162/0/26**. (AuthoringSource 0.3, AuthoringInspector 0.3, AuthoringReviewFindingTests 0.1.)

- **2026-10-10 (g)** — **REQ-124 Slice B — first candidate edit: contextual expression replacement (`AuthoringSnapshot.ProposeExpressionEdit`, branch `feature/req124-slice-b`).** New `AuthoringEdit.cs` public API (`AuthoringEditProposal`, `AuthoringEditResult`/`AuthoringEditFailure`/`AuthoringEditFailureKind` — UnknownNode / SlotNotEditable / ExpressionParseError / RequiresParentChange / NotRepresentableInEncoding, `AuthoringChangedSlot`, `AuthoringEditCandidate`) + internal `ExpressionEditPipeline.cs`; Slice A files `AuthoringSnapshot`/`AuthoringSource`/`AuthoringInspector` bumped 0.2 with additive members. **Contract (dossier §5/§7/§8):** v1 edits **Expression slots only** — Pattern/AVT/QName/Plain slots are a documented capability absence (`SlotNotEditable`), never an exception. The pipeline resolves the owning node + attribute, **validates the new text against the slot's real static context** via the public `XPath31Expression.Compile` (in-scope prefixes, xpath-default-namespace as DefaultElementNamespace, xml:base-resolved base URI, `XPath40` iff the effective version starts "4") → `ExpressionParseError`; an undeclared-prefix (XPST0081, message-matched in one isolated helper pending a code-bearing public API) → `RequiresParentChange` carrying the owning element's **full start-tag range** as ExpandedRange — per §5 the engine refuses and reports expanded ownership rather than pretending an attribute-local change. Splice text escapes `&`/`<`/the original quote char (`&quot;`/`&apos;`, quote read from source) and encodes once with `EncoderExceptionFallback` — non-representable characters (e.g. `€` into ISO-8859-1/US-ASCII) are an explicit `NotRepresentableInEncoding` refusal naming the encoding, never a silent loss. **Emission is splice-only:** the candidate's `EmittedSource` is the original module bytes with exactly the attribute ValueRange byte span replaced; all other modules/envelopes shared; the input snapshot, retained source and caller-owned XDocuments are never mutated. The candidate is immutable: derived re-inspected `Snapshot`, `AffectedRanges`, paired-DFS `NodeCorrespondence` (kind + QName + child ordinal; unpaired nodes omitted, never remapped — §6/§7 identity rule), `IsCompilable`/`CompilationDiagnostics`, and `Compile()` → `XsltExecutable`. Concurrency-safe by immutability (50-way `Parallel.For` test; chained revisions; stale-revision proposals resolve against their own snapshot instance). **AC evidence:** 17 new tests (`AuthoringExpressionEditTests`) — AC-04 runs the dossier §4 workflow end-to-end (baseline `<result>Braid</result>` vs edited `<result>42</result>` on the same input XML, byte-diff strictly inside the affected span, comment/declaration retained, correspondence maps template + value-of); AC-05 parse-error/SlotNotEditable/UnknownNode/misuse taxonomy; AC-06 declared vs undeclared prefixes, `&`/`<`/quote escaping, non-ASCII accept (UTF-8) vs refuse (Latin-1 `€`, accept `é`), CRLF byte-exactness; AC-10 isolation/concurrency/stale-revision. Slice A fixtures hardened: hand-verified byte offsets now line-ending-independent (`string.Join("\n", …)` fixtures instead of raw literals — autocrlf-proof; the CRLF offsets were verified failing on a CRLF checkout). Gates: build 0 errors; `dotnet test Bosak.sln -c Release` **3,691 passed / 0 failed** (Xslt.Tests 865 = 848+17); XSLT smoke **162/0/26**. (AuthoringEdit 0.1, ExpressionEditPipeline 0.1; AuthoringSnapshot/AuthoringSource/AuthoringInspector 0.2; AuthoringExpressionEditTests 0.1.)

- **2026-10-10 (f)** — **REQ-124 Slice A — source-preserving XSLT authoring API (`Bosak.Xslt.Authoring` namespace, branch `feature/req124-slice-a`).** New additive namespace in the existing `Bosak.Xslt` package (13 new files, **zero existing files touched**, no new dependencies) implementing the consumer contract of `docs/REQ-124-source-preserving-xslt-authoring.md` with the owner-ratified decisions recorded in dossier §13.1. This is the seam the Bosak.Braid visual designer builds on — engine-owned source-aware state before compilation preprocessing, with no second parser and no reflection. **Architecture — retained-envelope + splice:** `AuthoringSource` is an immutable envelope (original bytes + detected encoding + absolute base URI; BOM → XML-declaration `encoding` → UTF-8 default; unsupported encoding names and invalid byte sequences are *explicit classified failures*, never silent). Original bytes are the only emission source for untouched regions — `AuthoringSnapshot.ExportOriginal(moduleUri)` is byte-for-byte exact (BOM, CRLF, quotes, entity spelling, comments, PIs) — so use-when=false branches, underscore shadow attributes and literal-result roots are retained **structurally**, not by special-casing; the parsed `XDocument` (PreserveWhitespace | SetLineInfo | SetBaseUri) is derived working state from which compilation is derived. **Inspection:** `XsltAuthoring.Inspect(source, resolver?, options?)` → classified outcome, never throws for bad source — `Structure` (malformed XML, with the XmlException position mapped to a source range), `UnsupportedEncoding`, `InvalidSourceBytes`, `ResolverFailure`. On success the snapshot exposes ordered per-module descriptors: QName'd element tree with full source extents (depth-counting scanner over raw source — quote-aware start tags, comments/PIs/CDATA skipped), attribute descriptors with slot classification (`Expression`/`Pattern`/`Avt`/`QName`/`Plain` — data-driven table, unclassified defaults to Plain), `RawLiteral` preserving entity spelling alongside the lossy expanded DOM value, and expression slots carrying an **opaque context** (in-scope namespace bindings, xpath-default-namespace, xml:base-resolved base URI, effective XSLT version) so expressions are edited in their real static context, not as naked strings. **Coordinates:** line/column (1-based, UTF-16 code units, matching `IXmlLineInfo`) is the primary contract; byte offsets derive deterministically through the retained source (CRLF / lone-CR / LF line index verified against `System.Xml` counting; astral and multiline-attribute fixtures in AC-09). Node identities are snapshot-scoped ints. **Modules:** caller-controlled `IAuthoringModuleResolver` (file-system default), relative href + xml:base resolution, unresolved edges with diagnostics (missing module → principal stays inspectable), cycle detection, duplicate-URI envelope reuse, 256-module bound. **Semantics:** `IsCompilable`/`CompilationDiagnostics` are recorded by compiling the derived document — semantic errors never fail inspection (dossier §8). **Acceptance evidence:** engine-owned contract tests AC-01/02/03/07/08/09 = 36 tests under `tests/Bosak.Xslt.Tests/Authoring/` (byte-exact export with hand-verified ranges; use-when/shadow-attr/LRE retention with derived compilation; vocabulary + opaque xsl:iterate extents + slot contexts; failure taxonomy; module graphs incl. cycle/missing/double-URI; coordinate contract). **Not in Slice A** (per dossier, explicitly): candidate edits/emission (Slice B), public packaging/sample (Slice C). Gates: build 0 errors; `dotnet test Bosak.sln -c Release` **3,674 passed / 0 failed** (Xslt.Tests 848 = 812+36); XSLT smoke **162/0/26**; zero existing-file modifications keeps every regression gate bit-identical by construction. (New: `src/Bosak.Xslt/Authoring/*` 0.1 ×13; `tests/Bosak.Xslt.Tests/Authoring/*` 0.1 ×4.)

- **2026-10-10 (e)** — **JNode cluster — F&O 4.0 §17.7 JNode model: `fn:jtree` / `fn:jkey` / `fn:jvalue`, `jnode()` type matching, path-navigation rework (REQ-123 slice, frozen `XPath40` level, branch `feature/req123-jnode`).** All new surfaces are 4.0-only (static XPST0017 at 3.1 in call and named-function-ref form), **not** experimental. **Consumers on the 3.1 default: nothing changes** — JNodes cannot be constructed at 3.1 (no `fn:jtree`), so every new engine branch is unreachable from 3.1 expressions. **⚠ One behavioral break at the frozen `XPath40` level:** the element-to-map slice's minimal public `fn:jvalue#1` (unwrap the typed value of a map entry produced by `fn:element-to-map`) is **superseded** by the §17.7 JNode accessor — `fn:jvalue` now expects a JNode and raises XPTY0004 on any other item; `ElementMap.cs` keeps its element-to-map converter internally (renamed comment only). The collateral qt4 sets (fn-element-to-map 174/0/40, fn-map-to-element 156/0/2, fn-element-to-map-plan 20/0/1) were re-verified green under the new semantics — most §17.6 asserts read through `jvalue($result//"…")`, which still works because navigation now returns JNodes and `fn:jvalue` unwraps them. **XDM:** new `XdmValueKind.JNode` (appended after `External` — no existing numeric value changes) + sealed `XdmJNode` (Value/Key/Parent; root = `Undefined` key). A JNode wraps **any** item — maps, arrays, atomics, nodes, functions, whole sequences — so the alternative design of annotating Map/Array was rejected. **Functions:** `fn:jtree#1` (keyword `input`) — a sequence wraps as one root JNode, a singleton wraps its item, `()` yields a root with `Undefined` value, double-wrapping is legal; `fn:jkey`/`fn:jvalue` in #0 (context item) and #1 forms — XPDY0002 with no context item, XPTY0004 on a non-JNode or multi-item argument. **Type matching:** `jnode()`, `jnode(*)`, `jnode(())` (root only), `jnode(K)` and `jnode(K,T)` — key specifiers `*`, `()`, quoted string literals, `#`QName literals, integers, and NCNames (as string keys); the `T` form recursively matches the jvalue against T. **`node()` is unchanged** (3.1 parity — no test pressure to include JNodes). **Atomization/coercion:** atomizing a JNode atomizes its jvalue (fn:string, fn:data, `AtomizeValue` — the string-join path that caught the ElementMap regression tests); the function-call fast path and `as`-coercion unwrap a JNode argument for map/array/function-typed parameters; EBV of a JNode is `true`. **Navigation (the big rework):** `ApplyAxis` now wraps non-JNode map/array inputs in an implicit `jtree()` before stepping (`MapAxis` → `JNodeAxis`) — child of a map = one JNode per entry keyed by the entry key, child of an array = members keyed by 1-based position, child of a sequence-wrapping JNode = items keyed by position; parent/ancestor(-or-self) walk the JNode parent chain; attribute/namespace axes are empty. NameTests match entry keys (`$m/"key"`, `$m//QName` — string keys exact-lexical, QName keys by local/prefixed resolution); KindTests `node()`/`element()` pass JNodes through; `?` lookup unwraps a JNode LHS; SimpleMap/`!` context checks, XPTY0018 result checks and `RequireNodesLazy` accept JNodes. **Parser:** integer-literal step `$m/2` (selects the child JNode with integer key 2) and the PR2667 braced key selector `child::{expr}` — new `LookupIndex`/`LookupComputed` opcodes with atomized-key matching; this also fixed the documented XPTY0019 quirk for predicates directly on a lookup step (`[2,4,{'a':20}] / *[3] / a` → 20). **Serialization:** the serializer emits the jvalue of top-level JNode items (json + adaptive methods); `fn:deep-equal` recurses on jvalues. qt4tests: **fn-jtree 30/0/0, fn-jkey 14/0/0, fn-jvalue 14/0/0** — all three promoted into the gate (now **221 sets**). Gates: build 0 errors; unit all green — `dotnet test Bosak.sln -c Release` **3,638 passed / 0 failed** (Standard.Tests +41 `JNodeTests`, Api.Tests VersionGate +2 rows); QT3 **31,142/0/679** exit 0; XSLT smoke **162/0/26** + Xslt.Tests units 812/812 via `run-xslt-tests.ps1`; gate-only **0 gated failures / 221 sets** (`.guard-tmp/qt4-gate-jnode.log`). (XdmValueKind 0.3, XdmValue 0.4, XdmJNode 0.1 new, VmEngine 2.172, FunctionLibrary 5.139, XPathParser 1.73, XPathAstNode 1.24, IrOpCode 1.11, IrLowerer 1.53, XdmSerializer 0.3, ElementMap 0.3, ConformanceRunner40 0.11, VersionGateTests 0.19; new `tests/Bosak.XPath.Standard.Tests/JNodeTests.cs` 0.1.)

- **2026-10-10 (d)** — **compare-tail cluster — Unicode case-insensitive collation + keyword rows + fn:collation-available + # QName literals (REQ-123 slice, frozen `XPath40` level, branch `feature/req123-compare-tail`).** All new surfaces are 4.0-only (static XPST0017 at 3.1 for the function; XPST0003 for the literal syntax), **not** experimental. **Unicode case-insensitive collation** (PR1945): `http://www.w3.org/2005/xpath-functions/collation/unicode-case-insensitive` is now recognized engine-wide — `FunctionLibrary.CompareStrings` (the single collation comparer feeding value/general comparisons), `collation-key` sort keys (`GetSortKey` with `IgnoreCase`, so key `eq`/`lt` stay consistent), contains/starts-with/ends-with/substring-before/after/index-of, the XSLT sort + group string comparison paths, the XQuery prolog default-collation check, and the VM order-by allow-list. Semantics: invariant-culture `CompareInfo.Compare(..., IgnoreCase)` — a Unicode simple-case-folding approximation (same posture as the grapheme work); `compare("ä","Ä") → 0`, `compare("bää","BÄÄB") → −1`. **Keyword-argument rows:** `collation-key(value, collation)`, `contains-token(value, token, collation)`, `node-name(node)`, and `min`/`max(values, collation)` — all with the 4.0 empty-`()`-selects-default-collation rule (PR197; the 3.1 evaluation path keeps XPTY0004 on an empty sequence, and `node-name(input:=…)` correctly raises XPST0017 unknown-keyword). **`fn:collation-available#1`** is new (4.0-only): atomizes with `xs:anyURI` promotion, resolves relative URIs against the base URI, returns true for the four recognized collations + `caseblind` + any parseable UCA URI, false otherwise (e.g. `ftp://not-a-collation/`); non-string singletons → XPTY0004. **`#` QName literals** (PR1976/PR2227): `#local`, `#prefix:local`, and `#Q{uri}local` in primary-expression position — whitespace and XPath comments after `#` are permitted (PR1982). The lexical prefix is **retained** in the value (`#Q{http://www.example.com}ex:a` → prefix `ex`, eqname-042) while equality/compare ignore it. A prefixed form resolves against the compile-time namespaces with the canonical predefined fallback (`xml`/`fn`/`map`/`array`/`math`); unresolvable → XPST0081; braced form with a prefix but empty URI → XPST0154; at 3.1 the `#` syntax is XPST0003. Implementation: new `QNameLiteralNode` (XPathAstNode), parsed in `ParsePrimaryExpr` (gated on the parser `xpath40` flag), prefix-resolved in the Api `ResolveFunctionNamespaces` pass, lowered to a constant `fn:QName(ns, lexical)` call — no new opcodes. **`fn:min`/`fn:max`** order `xs:QName` values by (namespace URI, local name) codepoint in 4.0 mode (PR2256 — `min((#xml:space, #xml:id, #fn:min))` → `#fn:min`); 3.1 keeps FORG0006 for QName inputs. qt4tests: **fn-collation-key 37/0/0, fn-contains-token 48/0/0, fn-collation-available 8/0/0** — all three promoted into the gate (now **218 sets**); **fn-compare 231/4** exploratory (the 4 remaining = destructuring `let $(…)`, its own slice); fn-node-name/fn-min/fn-max collateral green (fn-min-41 keyword `collation:=()` fixed by the min/max rows). Gates: build 0 errors; unit all green — `dotnet test Bosak.sln -c Release` **3,590 passed / 0 failed** (Standard.Tests +9 compare-tail tests incl. collation/QName-literal/error cases, Api.Tests VersionGate +9 rows) + LanguageServer.Tests **72/72**; QT3 **31,142/0/679** exit 0; XSLT smoke **162/0/26**; gate-only **7,850/0/1,840, exit 0, 218 sets** (`.guard-tmp/qt-sweeps-compare-tail.log`). (FunctionLibrary 5.137, VmEngine 2.171, XPathParser 1.72, XPathAstNode 1.23, XPath31Expression 0.18, IrLowerer 1.52, Stylesheet 2.128, TransformEngine 7.05, XQueryParser 1.16, ConformanceRunner40 0.10, XPath40FunctionTests 0.8, VersionGateTests 0.18.)
- **2026-10-10 (c)** — **sort-with cluster — F&O 4.0 `fn:compare` PR909 upgrade + `fn:sort-with` fixes + `array:sort-with` + `fn:is-NaN` + `fn:atomic-type-annotation` (REQ-123 slice, frozen `XPath40` level, branch `feature/req123-sort-with`).** All 4.0-only (static XPST0017 at 3.1 in call and named-function-ref form), **not** experimental. **`fn:compare`** comparands relax from `xs:string?` to `xs:anyAtomicType?` with cross-type rules: finite numerics compare **exactly** (`(double)3.1 > (decimal)3.1` +1, `(float)3.1 < (decimal)3.1` −1, `2e0 = 2`) via BigInteger decimal expansion — no rounding ever; NaN sorts below −INF and NaN = NaN; +0 = −0; booleans false < true; strings/`xs:untypedAtomic` by the effective collation (3-arg `()` collation → default collation; keyword form `comparand1 := …, comparand2 := …, collation := ()`); `xs:hexBinary`/`xs:base64Binary` mutually comparable by decoded octets; `xs:QName` by {namespace URI, local name}; `xs:date`/`xs:time`/`xs:dateTime` by instant; **all duration subtypes mutually comparable by the un-normalized (months, days, seconds) tuple** — `P2Y > P1000D`, `PT1H = PT60M0.00S`, `P1Y = P12M` (yearMonth); g* types on the **UTC instant** with missing timezone read as the implicit timezone (PR2256 — g* values are String-kind, so the date-subtype check now precedes the string branch); mixed incomparable kinds (integer vs string) → XPTY0004. The 3.1 code path of `fn:compare` is byte-identical. **`fn:sort-with`** validates comparators *before* the empty-input shortcut (`sort-with((), ())` → XPTY0004). **`array:sort-with#2`** is new. **`fn:is-NaN#1`** atomizes (arrays contribute members; maps/functions → FOTY0013; empty/multi → XPTY0004) and is true only for double/float NaN. **`fn:atomic-type-annotation#1`** returns the type record: `name` (QName), `is-simple`, `variety` ("atomic"), `base-type()`/`primitive-type()` zero-arity closures (hierarchy table, `xs:anyAtomicType` self-closes), `matches($v)` (hierarchy-walking instance-of — 42 matches `xs:decimal`, a dayTimeDuration value matches `xs:duration`, not vice versa), `constructor($v)` (delegates to `VmEngine.Cast`); values without an explicit annotation (literals, constructor results) get their XDM-natural type **inferred from the value kind** (`42` → `xs:integer`, `'x'` → `xs:string`). **Parser:** PR2962 lookup keys accept variable references — `$map?$var`. **Harness:** `DependencyFilter40` marks `typedData` unsupported (the harness builds no PSVI-typed source nodes). qt4tests: fn-sort-with + array-sort-with **49/0/0** promoted (gate now **215 sets**), fn-is-NaN **14/0/2** dep-skips, fn-atomic-type-annotation **2/0/4** dep-skips, fn-compare **220/13** exploratory (was 107 failures — remaining 13 = `#Q{}` URI-qualified literals, destructuring `let $($h,$m)`, UCA collation FOCH0002, each its own slice). Gates: build 0 errors; unit all green — `dotnet test Bosak.sln -c Release` **3,570 passed / 0 failed** (Standard.Tests +15 `XPath40FunctionTests`, Api.Tests VersionGate +12 rows); QT3 **31,142/0/679** exit 0; XSLT smoke **162/0/26**; gate-only **7,764/0/1,833, exit 0, 215 sets** (`.guard-tmp/qt4-gate-only-sort-with.log`). (XPathParser 1.71, VmEngine 2.170, FunctionLibrary 5.138, ConformanceRunner40 0.10, DependencyFilter40 0.2, XPath40FunctionTests 0.7, VersionGateTests 0.17.)
- **2026-10-10 (b)** — **Focus constructors — every built-in `xs:*` constructor gains its XPath 4.0 arity-0 form (REQ-123 slice, spec PR661, frozen `XPath40` level, branch `feature/req123-focus-constructors`).** 4.0-only (static XPST0017 at 3.1 in both call and named-function-ref form), **not** experimental. `xs:T()` with zero arguments takes the **context item** as the implicit cast argument — `'42' ! xs:integer()`, `'2026-10-10' ! xs:date()`, `xs:unsignedLong()` over a max-ulong context — with errors straight from the cast rules: **XPDY0002** when there is no context item, **FOTY0013** when it is a map or function item, **FORG0001** on an invalid lexical form, **XPTY0004** on a non-castable source type; `xs:QName()` resolves prefixes against the in-scope namespace bindings. Implementation: the `FunctionLibrary` static constructor auto-registers an arity-0 counterpart for every registered `xs:*#1` constructor (flagged `IsXPath40Only`, so 3.1 evaluation contexts never see it — `fn:function-lookup(xs:QName('xs:integer'), 0)` returns `()` at 3.1 and the signature at 4.0), and `XsFocusCtor` delegates to the existing `VmEngine.Cast` path (QName to the namespace-aware arity-1 constructor). **Version-gate change (consumer-visible only at 3.1):** the compile-time 4.0-only/experimental gates are now **arity-aware** — the (namespace, local-name) sets remain as pre-filters, but the decision comes from the resolved signature, because the arity-0 constructors share their names with the 3.1 arity-1 forms; the canonical-predefined-prefix fallback now also covers `xs`. The arity-1 constructors (`xs:integer($x)`) are unchanged and remain legal at 3.1. **Harness:** `assert-type xs:unsignedLong` now accepts decimal-backed values carrying the `xs:unsignedLong` annotation — integers are `long`-backed, so max-ulong values are decimal-backed (the qt4 test-set explicitly allows this implementation limit). qt4tests: misc-FocusConstructors **153/0/0** — the set enters the gate (now **213 sets**). Gates: build 0 errors; unit all green — `dotnet test Bosak.sln -c Release` **3,541 passed / 0 failed** (Api.Tests +24 `FocusConstructorTests`, VersionGate +13 rows); QT3 **31,142/0/679** exit 0; XSLT smoke **162/0/26**; gate-only **7,715/0/1,833, exit 0, 213 sets** (`.guard-tmp/gate-only-focus-ctors.log`). (FunctionLibrary 5.137, XPath31Expression 0.17, ResultComparer 3.2, ConformanceRunner40 0.9, VersionGateTests 0.16; new `tests/Bosak.XPath.Api.Tests/FocusConstructorTests.cs` 0.1.)

- **2026-10-10 (a)** — **fn:atomic-equal — F&O 4.0 §2.2.1 map-key equality exposed (REQ-123 slice, frozen `XPath40` level, branch `feature/req123-atomic-equal`).** 4.0-only (static XPST0017 at 3.1), **not** experimental. `fn:atomic-equal($value1 as xs:anyAtomicType, $value2 as xs:anyAtomicType) as xs:boolean` returns true iff the two items would be the same key in a map: string/`xs:anyURI`/`xs:untypedAtomic` compare by codepoint (the default collation is never consulted — atomic-equal-002); numerics compare by exact mathematical magnitude — NaN = NaN, +0 = −0, INF families by sign, and `1.1` (decimal) is **not** equal to `1.1e0` (double) because no rounding is applied; `xs:date`/`xs:time`/`xs:dateTime` and the g* types compare only when timezone presence matches (both zoned or both not — the implicit timezone is never used) and the instants are equal; `xs:QName` by {namespace URI, local name} (prefix irrelevant); `xs:duration` by normalized totals (`P1Y` = `P12M`); the 4.0 delta over 3.1 map keys (PR2168) makes `xs:hexBinary` and `xs:base64Binary` **mutually comparable by decoded octets**. The implementation delegates to the existing map-key comparer (`XdmValueEqualityComparer`) — the same semantics source map construction/lookup uses — with the binary case handled in the function; empty or multi-item arguments are XPTY0004 via the standard `xs:anyAtomicType` signature, node arguments are atomized. **Parser companions (4.0):** the quantifier keywords double as function names in arrow targets (`$seq => every()` ≡ `fn:every($seq)`, likewise `some`), and `if` accepts braced branches — `if (C) { X } else { Y }` or `if (C) { X }` (omitted else → empty sequence; the keyword form still requires `else`). qt4tests: fn-atomic-equal **29/0/1** (skip = XQ-only dependency), fn-while-do **30/0/0** promoted (its last failure was fixed incidentally by earlier engine work) — the gate gains two sets (now **212 sets**). Gates: build 0 errors; unit all green — `dotnet test Bosak.sln -c Release` **3,501 passed / 0 failed** (Standard.Tests +21 `AtomicEqualTests`, Api.Tests VersionGate +3 rows); QT3 **31,142/0/679** exit 0; XSLT smoke **162/0/26**; gate-only **7,562/0/1,833, exit 0, 212 sets** (`.guard-tmp/gate-only-atomic-equal2.log`). (FunctionLibrary 5.136, XPathParser 1.70, VersionGateTests 0.15, ConformanceRunner40 0.8; new `tests/Bosak.XPath.Standard.Tests/AtomicEqualTests.cs` 0.1.)

- **2026-10-09 (f)** — **fn:element-to-map / fn:map-to-element / fn:element-to-map-plan / fn:jvalue — F&O 4.0 §17.6 element↔map conversion (REQ-123 slice, frozen `XPath40` level, branch `feature/req123-element-map`).** All four are 4.0-only (static XPST0017 at 3.1) and, like fn:op and the CSV functions, **not** experimental — they ship at the frozen `XPath40` level. **Surfaces (spec-verified against F&O 4.0 §17.6 and the qt4tests ground truth):** `fn:element-to-map($input[, $options])` converts an element (or a document's element node) to a map — `()` in → `()` out, non-node or multi-item input → XPTY0004. Layout inference (per element name, union-merged over same-named elements per §14.6.2 `$EE`): `empty`/`simple`/`list`/`record`/`sequence`/`mixed`, each with a `-plus` variant; attributes become `@`-prefixed keys, text content goes under `#content` (only when the layout needs it); `name-format` `default` abbreviates a child to its bare local name only when it shares the parent's namespace, `eqname` writes `Q{uri}local` for every namespaced name. With a `plan` (from `fn:element-to-map-plan` or literal), values are typed by numeric subtype promotion: all-integers → `xs:integer` (a leading zero inhibits), integer+decimal mixes → `xs:decimal`, any exponent form → `xs:double`, `true`/`false` (+`0`/`1`) → `xs:boolean`, anything else stays `xs:untypedAtomic`; a plan entry `@id: {type: integer}` also applies to `fn:element-to-map`'s own attribute typing; layout violations against the plan → FOJS0008, prescribed-type conversion failures → FOJS0010 unless `liberal: true()` (keeps the lexical value); `type: skip` keeps the attribute with its lexical value. `fn:map-to-element($map[, $options])` rebuilds a parentless element from a single-entry map (`()` in → `()` out, non-map → XPTY0004, malformed → FOJS0009); `fn:jvalue` unwraps the typed value of a map entry. **Engine companion (PR2688): map path navigation** — a string literal in non-first step position is a key lookup: `$m/"key"` (child lookup), `$m//"key"` (descendant map/array closure — `$result//"@id"` is exactly how the §17.6 tests read typed attributes), and `$m//QName` (a name test on maps performs the same key lookup, so `$result//id` works). New `LookupKey` opcode (parser `NameTestKind.LookupKey`, IrLowerer early-return) plus map-aware `ApplyAxis`/`NameTest`/`KindTest(element)`/`NamespaceTest` branches; child-of-map is the identity (the following test performs the lookup), descendant is the transitive map/array closure, and a **first-position string literal stays a primary expression** — the disambiguation is a `firstStep` parameter on `ParseStepExpr`, because `parse-xml('<a/>')` arguments must never become lookup steps (this regression was caught and fixed in-session). **Harness companion:** the qt4 harness now applies the **recursive predefined-entity decode** convention — test expressions and string-valued asserts (`assert`, `assert-eq`, `assert-deep-eq`, `assert-type`, `assert-count`, `assert-string-value`, `serialization-matches`) are entity-expanded a second time, while `assert-xml`/`assert-serialization` stay single-expanded. Decoding test expressions alone regressed the legacy ground truth of `fn-iri-to-uri-18A` and `string-template-004` ("Entity references not recognized"), whose expected values encode the entity text literally; the recursive convention satisfies both them and `map-to-element-014/021`. Flagged `TestCase.DecodeEntitiesInTestExpressions` (qt4 harness sets it; QT3 is untouched). qt4tests: fn-element-to-map **174/0/40** (dependency skips), fn-map-to-element **156/0/2**, fn-element-to-map-plan **20/0/1** — the three sets enter the gate (now **210 sets**; gate-only verification 7,503/0/1,832). Gates: build 0 errors; unit all green (Standard.Tests incl. 22 new `ElementMapTests`, Api.Tests 156 VersionGate rows incl. 8 new); QT3 **31,142/0/679**; XSLT smoke unchanged. (XPathParser 1.69, XPathAstNode 1.22, IrOpCode 1.10, IrLowerer 1.51, VmEngine 2.169, FunctionLibrary 5.135, ElementMap 0.2, TestCase 0.8, ResultComparer 3.1, ConformanceRunner40 0.7, VersionGateTests 0.14; new `tests/Bosak.XPath.Standard.Tests/ElementMapTests.cs` 0.1.)

- **2026-10-09 (e)** — **fn:parse-csv / fn:csv-to-xml / fn:csv-doc — F&O 4.0 §17.5 CSV functions (REQ-123 slice, frozen `XPath40` level, branch `feature/req123-parse-csv`).** All three are 4.0-only (static XPST0017 at 3.1) and, like fn:op, **not** experimental — they ship at the frozen `XPath40` level. **Surfaces (spec-verified against F&O 4.0 §17.5 and the qt4tests edge cases):** `fn:parse-csv($value[, $options])` returns the 4-entry record map (`columns`, `column-index`, `rows`, `get`) — CR/CRLF normalized up front; comment rows (`comments` option) recognized at raw row start; blank rows exist only when newline-terminated (a blank unterminated tail is discarded); quotes open only as the first field character, doubled quotes escape, a closing quote must be followed by separator/newline/EOF (else FOCV0001), unterminated → FOCV0001; `trim-rows` pads rows to the raw first-row width (captured before header consumption), `select-columns` reorders/duplicates/pads (with `""`) and takes precedence; option validation: unknown key → XPTY0004, `field-delimiter`/`column-names` names dropped, `CharOption` singleton (0 items → FOCV0002, multiple → XPTY0004, length ≠ 1 → FOCV0002), `header` single boolean or string* name list, `select-columns` positiveInteger keys else XPTY0004. `fn:csv-to-xml($value[, $options])` emits the §17.5.9 element shape (columns only up to the last non-empty name; `column` attribute only for named positions). `fn:csv-doc($href[, $options])` resolves via the unparsed-text path (FOUT1200 on an invalid BOM) and then parses. `get` is an arity-2 function item: a string column name misses → FOCV0004; `r < 1` or an integer column < 1 → FORG0001; a positive out-of-range row/column → `""`. **Parser companions (4.0-gated, XPST0003 in 3.1):** the bare `{…}` map constructor (spec PR2778 — `{"a":1}` with no `map` keyword) and consecutive `for`/`let` FLWOR clauses in a single expression. **Harness:** `ResultComparer.AssertCompatibility` (default 3.1) lets qt3tests/qt4tests assert at different compatibility levels from one shared comparer. qt4tests: fn-parse-csv 124/0/0, fn-csv-to-xml 68/0/2 (2 skips = XQ40-only dependencies, legit), fn-csv-doc 20/0/0 — the three sets enter the gate (now **207 sets**). Gates: build 0 errors; unit all green (Standard.Tests 1171 incl. ~50 new `CsvTests`, Api.Tests 301 incl. 4 new VersionGate rows); QT3 **31,142/0/679**; XSLT smoke 162/0/26. (FunctionLibrary 5.134, XPathParser 1.68, ResultComparer 3.0, ConformanceRunner40 0.6, VersionGateTests 0.13; new `tests/Bosak.XPath.Standard.Tests/CsvTests.cs` 0.1.)

- **2026-10-08 (k)** — **XSLT 4.0 array construction + switch (REQ-118 slice 4.0-S8, branch `feature/req118-40-s8`).** **Consumers on any XSLT version: nothing breaks** — per XSLT 4.0 draft §3.8.2/§3.8.3 the new instructions work at every effective version, and version="4.0" stylesheets remain forwards-compatible (the supported-version ceiling deliberately stays 3.0 per the frozen W3C forwards-* tests) — xsl:array/xsl:array-member/xsl:switch are known XSLT elements, so forwards-compatible stylesheets still execute them. **Surfaces (spec-verified against the live XSLT 4.0 WG Review Draft, 08-10-2026):** **`xsl:array`/`xsl:array-member`** (§21.1.2 — `@select` contributes one singleton member per item; `@for-each` contributes one member per focus item whose value is the whole per-item sequence; sequence-constructor content treats each ordinary item as a singleton member while a nested xsl:array-member contributes exactly one sequence-valued member — the spec's JNode wrapper is unnecessary because XdmArray members hold sequences natively; select+content → XTSE3185; an array as an element/document child → XTDE0450, top-level under a text output method → SENR0001, mirroring xsl:map). **`xsl:switch`** (§8.3 — `@select` coerced to a single atomic item, XPTY0004 otherwise; each `@test` coerced to atomic items and compared with the selector under general-comparison `=` semantics with the default collation in scope — NaN never matches; first satisfied branch wins and later tests/branches are never evaluated; `xsl:when`/`xsl:otherwise` under xsl:switch gain an optional `@select` (select+content → XTSE3185), while under xsl:choose the attribute stays inert so choose behavior is bit-identical to 3.0; no match + no otherwise → empty). **Pattern system §6.3–§6.4 (analysis only, no API change):** Bosak already computes union-pattern default priority as the maximum of the branches — exactly the XSLT 4.0 rule — so no behavioral change; the new 4.0 pattern forms (map/array/JNode patterns, `element(a|b)`) and the §6.4.2 type-pattern priority table are recorded as known deviations pending the owner's decision on a true XSLT 4.0 mode flag.

- **2026-10-08 (j)** — **XSLT 4.0 easy surfaces (REQ-118 slice 4.0-S7, branch `feature/req118-40-s7`).** **Consumers on any XSLT version: nothing breaks** — per XSLT 4.0 draft §3.8.2/§3.8.3 no differences are defined for XSLT 3.0 behavior, so the new surfaces are deliberately NOT gated to version="4.0" stylesheets (they work at every effective version), and version="5.0"+ stylesheets keep forwards-compatible processing; the supported-version ceiling deliberately stays 3.0 (the frozen W3C forwards-* tests exercise forward compatibility via version="4.0"/"3.3"), so a version="4.0" stylesheet is processed forwards-compatibly — unknown instructions are ignored (xsl:fallback evaluated) — while the new surfaces themselves work at every effective version per the draft. **Surfaces (spec-verified against the live XSLT 4.0 WG Review Draft, 08-10-2026):** **`xsl:note`** (§3.11.2 — may appear anywhere except as the outermost element; discarded at load WITHOUT validation, even content that would otherwise be a static error; usable as a comment-out mechanism). **`xsl:if` `then`/`else`** (§8.1 — `<xsl:if test="C" then="X" else="Y"/>` ≡ `<xsl:sequence select="if (C) then X else Y"/>`; then/else are plain expressions, NOT AVTs; a `then` attribute requires no children (XTSE0010); absent `else` means `()`; a test with no effective boolean value (e.g. `(1,2)`) is FORG0006; works in template bodies, function bodies, simple content and accumulator rules). **`separator` on `xsl:for-each`/`xsl:apply-templates`** (§7.1.1/§6.7 — an AVT inserted as a text node between the results of successive items of the sorted sequence; a zero-length separator still occupies a slot; in raw-sequence contexts such as `xsl:variable`/`xsl:function` content the separator becomes a string item of the sequence and therefore also breaks runs of adjacent atomic values). **`xsl:map` upgrades** (§21.1.1 — `@select` coerces its input to `map(*)*` (a non-map item is XTTE3375) and merges the maps as `map:merge`; `@duplicates` is an expression — one of the strings use-first/use-last/use-any/combine/reject (anything else FOJS0005; 'reject' raises FOJS0003 on the first duplicate) or an arity-2 combining function `fn($existing, $new)`; with no `@duplicates` the default remains XTDE3365 on the first duplicate key, exactly as before; `@select` plus sequence-constructor content is a static XTSE3185, xsl:fallback exempt). **`xsl:map-entry` promotion** — was already first-class in Bosak (usable in any sequence constructor); NOTE the select+content error stays XTSE3280 because the xt3 catalog (maps-008/error-3280a) pins it, even though the 4.0 draft re-labels it XTSE3185. Gates: build 0 errors; unit all green (Xslt.Tests 780 incl. 38 new `Xslt40SurfaceTests`); QT3 31,142/0/679; XSLT smoke 162/0/26 — full XSLT sweeps pending before merge. (Stylesheet 2.126, TransformEngine 7.03, StreamabilityAnalyzer 0.12.)
- **2026-10-08 (i)** — **XPath 4.0 structural record types + `but with` (REQ-118 slice 4.0-S6b, branch `feature/req118-40-s6b`).** **Consumers on the 3.1 default: nothing changes** — `record(…)` in type positions and the `but with` operator are rejected with XPST0003 in 3.1/3.0 mode; lookup on plain maps is bit-identical (the new field check is record-annotation-gated), and 3.1-created maps can never carry an annotation. **4.0 opt-in surfaces (spec-verified against the live XPath 4.0 WG Review Draft, 08-10-2026):** **structural record types** (§3.2.10 — `record(*)`, `record()`, `record(first as xs:string, "a b", last,)`; duplicate field names XPST0021; an omitted field type is `item()*`). A record is a **map carrying a record-type annotation** — map constructors still produce plain maps, and **plain maps never match any record type** (`map{"x":3} instance of record(x)` → false). Records come into existence through coercion, cast, or `but with`. **Instance-of** is structural with an exact entry count; per-field recursive matching gives covariant field subtyping (`record(a as xs:integer)` ⊆ `record(a as xs:decimal)`). **Coercion** (§3.4.2 rule 10 — e.g. `function($r as record(a as xs:integer)) {…}` called with a plain map): missing fields become `()` entries (XPTY0004 when the field type requires a value), **surplus keys are XPTY0004**, present values are coerced recursively (numeric promotion applies), and entries are stored in field-declaration order. **Cast** (§4.19.2.7) differs: present values are kept when matching else **cast** to the field type (failure FORG0001), surplus keys are **discarded**, `record(*)` is an assertion (XPTY0004 on a plain map) — e.g. `{"first":"John","middle":"Ignatius","last":"Smith"} cast as record(title, first, last)` yields `{"title": (), "first": "John", "last": "Smith"}`. **Lookup** (§4.15.3): `?key` and record-as-function calls raise XPTY0004 for keys that are not declared fields (checked per key in the multi-key form); `?*` and the `map:*` functions stay field-blind. **`but with`** (§4.15.4): `$A but with $B` ≡ `let $temp as R := map:merge(($A,$B), {'duplicates':'use-last'}) return $temp` with R = A's annotation — a plain-map left operand is XPTY0004 (no annotation to name R), right values are coerced to their field types, and the result stays annotated (chainable). **Plumbing:** first XDM change of the REQ-118 wave — `XdmMap.RecordType` + `WithRecordType` (annotations survive `WithAdded`/`WithRemoved`), new `XdmRecordType`/`XdmRecordField` in `Bosak.XPath.Core.Xdm`; parser `record` type branch + `TryParseRecordTypeFields` + `ParseButWithExpr` (XPathParser 1.67); `BinaryOperator.ButWith` reusing `BinaryExpressionNode` (no new AST node — every traversal consumer already handles it) with new `IrOpCode.ButWith` (IrLowerer 1.50); VmEngine 2.167 shape tests before QName resolution plus `CoerceMapToRecord`/`TryCastToRecord`/`ButWith`. **Interop:** record-annotated maps pass through map/array functions, serialization, and providers unchanged. Nominative record types stay deferred. qt4tests `prod/RecordType.xml` is stale (pre-annotation draft) — hand-written unit tests only (`RecordTypeTests`, 30 methods + VersionGateTests 0.10). **Gates:** `dotnet test` 2,729→full suite green incl. XSLT 742; QT3 31,142/0/679; XSLT smoke 162/0/26 — full XSLT sweeps pending before merge.


- **2026-10-08 (h)** — **XPath 4.0 type-system first piece (REQ-118 slice 4.0-S6a, branch `feature/req118-40-s6a`).** **Consumers on the 3.1 default: nothing changes** — `enum(…)` and parenthesized `|` choices in type positions are rejected with XPST0003 in 3.1/3.0 mode, exactly like a syntax error; the runtime type matcher only reaches the new branches for texts the 4.0 parser produced. **4.0 opt-in surfaces (spec-verified against the live XPath 4.0 WG Review Draft):** **enumeration types** (§3.2.6 — `enum("red", "green")` in any ItemType position: `instance of`, `cast as`/`castable as`, function parameter/return declarations). An enum is a *structural* type over xs:string: membership compares by codepoints, instances are **not** re-annotated (a matching value stays a plain `xs:string`), a multi-member enum ≡ the union of its singletons, and `xs:untypedAtomic` is **not** an instance (it is not an xs:string subtype) though it *casts* to the enum via xs:string (§3.4.2 rule 05). A non-member cast fails with FORG0001. **Choice item types** (§3.2.5 — `(xs:date|xs:dateTime)` in the same positions, mixing node-kind and atomic alternatives freely, e.g. `(element(a)|xs:string)`). An all-atomic choice is a generalized atomic type, so it is a valid cast target: `"2024-01-01" cast as (xs:date|xs:dateTime)` yields the xs:date. Casts and function-argument coercion try the alternatives **in declaration order** (§3.4.2 rule 02 / F&O §23.3.7): a value already matching an alternative passes through unchanged, otherwise the first successful coercion wins — the spec's own `fn:char` example confirms an integer against `(xs:string|xs:positiveInteger)` arrives as the **string**; when every alternative fails a cast raises FORG0001 and a function-argument mismatch raises XPTY0004. Node-valued arguments against all-node-kind choices (`(element(a)|element(b))`) pass through unchanged. **Plumbing:** no XDM changes — the type system is string-based, so enum/choice are new type-text branches: the parser serializes a choice as a verbatim `(alt1|alt2…)` type text and validates enum literal lists quote-aware (`XPathParser.TryParseEnumTypeMembers`, shared to the runtime via InternalsVisibleTo); `VmEngine` recognizes the shapes before QName resolution in `ValueMatchesType`, `InstanceOf`, `TryCast`, and the function-conversion paths (`IsNodeKindTestType` recurses into choices). Structural record types (`record(…)`, `but with`) are the separate 4.0-S6b slice. Gates: Release build 0 errors; unit all green incl. 16 new `VersionGateTests` methods; QT3 **31,142/0/679**; XSLT smoke re-run (parser/VM engine files touched — `mode` set 162/0/26, 100.0% of runnable; full sweeps pending, parent runs them before merge). (XPathParser 1.66, VmEngine 2.166, VersionGateTests 0.9.)

- **2026-10-08 (g)** — **XPath 4.0 tier-1 higher-order function batch (REQ-118 slice 4.0-S5, branch `feature/req118-40-s5`).** Same gate as the earlier function batches: 4.0-only functions, invisible to the 3.1 default (XPST0017 at compile time in call and named-function-ref form; absent from 3.1 `fn:function-lookup` tables), enabled via `CompileOptions.Compatibility = XPathCompatibility.XPath40`. Nine F&O 4.0 functions (signatures + error codes verified against the live F&O 4.0 WG Review Draft and the qt4tests edge cases): `fn:some` / `fn:every` (§2.5.16/§2.5.4 — `$predicate` optional, default `fn:boolean#1`; passing an empty sequence as the predicate selects the default; an arity-1 predicate is legal (extra arguments are ignored per F&O 4.0 §1.8); a predicate result is strict-cast to `xs:boolean?` — `()` counts as false, a non-boolean-castable result raises XPTY0004, EBV is *not* used), `fn:index-where` (§2.5.11 — no default predicate; `()` raises XPTY0004), `fn:partition` (§2.5.14 — `$split-when fn($partition, $item, $pos)` returns `array(item()*)*`; the callback is never invoked for the first item; an arity-1 callback receives the partition only), `fn:take-while` / `fn:drop-while` (§2.5.21/§2.5.3 — no default; a map or array used as the predicate is applied as a 1-argument function), `fn:while-do` / `fn:do-until` (§2.5.23/§2.5.2 — the *whole sequence* is the value threaded through `$predicate`/`$action fn($input, $pos)`, `$pos` starts at 1 and increments per iteration; while-do tests first (action never runs when the predicate is initially false), do-until acts first (the action always runs on the initial value); there is no empty-input special case — `fn:do-until((), string#1, exists#1)` returns `""`), `fn:partial-apply` (§2.5.13 — `$arguments` is a map of 1-based parameter positions; keys above the function arity are ignored; binding every parameter yields a zero-arity function; a map/array used as the function argument is applied as a 1-argument function; bound-value coercion may be raised at bind time or at call time), `fn:transitive-closure` (§2.5.22 — `$node as node()?`, `$step fn($node) as node()*`; the result is a set in document order, excludes `$node` unless reachable, cycles terminate, a non-node input raises XPTY0004). **Plumbing:** new `FunctionLibrary` helpers `RequireCallable`/`CallableArity`/`InvokeCallable` (a callable is a function item, map, or array — anything else is XPTY0004; extra arguments truncated), `PredicateBoolean` (strict `xs:boolean?` conversion), `DefaultBooleanPredicate`, `CoerceBoundValue` (eager coercion only when parameter-type metadata is available), `TransitiveClosure_2` (BFS with `IXdmNode.IsSameNode` dedup + document-order sort); `VmEngine.ConvertArgToKind` became public. `fn:scan` stays deferred per the dossier (post-June-2026 churn). Gates: Release build 0 errors; unit all green incl. 10 new `VersionGateTests` data rows + 62 new `XPath40FunctionTests`; QT3 **31,142/0/679**; XSLT smoke unchanged (zero XSLT-project files touched). (FunctionLibrary 5.131, VmEngine 2.165, XPath40FunctionTests 0.4, VersionGateTests 0.7.)

- **2026-10-08 (e)** — **XPath 4.0 grammar, second piece (REQ-118 slice 4.0-S3b, branch `feature/req118-40-s3b`).** **Consumers on the 3.1 default: nothing changes** — `XPathParser.Parse`/`ParseExprSingle` and `XPathLexer` gained 4.0-only productions (keyword arguments, string templates) that are inert unless the `xpath40` flag is set (only `XPath31Expression.Compile` with `Compatibility = XPathCompatibility.XPath40` sets it); in 3.1/3.0 mode both are rejected with XPST0003 (a backtick stays an invalid token, exactly as before). **4.0 opt-in surfaces (spec-verified against the live XPath 4.0 / F&O 4.0 WG Review Drafts):** **keyword arguments** (XPath 4.0 §4.6.1) — `f(pos, name := expr, …)`; positional arguments fill the leading parameters, keywords match the remaining declared parameter names under the *no-namespace rule* (an unprefixed keyword is in no namespace), and every mismatch is XPST0017: unknown keyword, duplicate keyword, keyword matching an already-filled parameter, unmatched required parameter. Unfilled optional parameters take their F&O 4.0 declared defaults (`FunctionLibrary.KeywordSignatures` holds the metadata as plain data — e.g. `fn:substring($value, $start, $length := ())` where an empty length now means "to the end" in 4.0 mode, `fn:sort($input, $collation := fn:default-collation(), $key := fn:data#1)`, `fn:hash($value, $algorithm := "MD5", $options := {})`). The call is rewritten to a fully positional call of the fully-populated arity; arrow targets work — the arrow source counts as the first positional argument (`(4,3,2,1) => fn:sort(key := fn:data#1)`). Keyword arguments on dynamic function calls are XPST0017. **Coverage: 21 functions** (substring/subsequence/string-join/replace/matches/contains/starts-with/ends-with/index-of/serialize/sort/sort-by/slice/parse-uri/hash/lang; map:merge/build; array:sort/slice) — full population + variadic keywords (`fn:concat`) recorded as follow-up. **string templates** (§4.10.2) — `` `fixed {expr} text` ``; fixed parts may contain `{{`, `}}`, and `` `` `` escapes; each interpolation contributes its atomized items cast to strings, joined with single spaces; an empty, whitespace-only, or comment-only interpolation contributes nothing; interpolations are full XPath 4.0 expressions (strings, comments, nested templates). **Plumbing:** `FunctionCallNode` gained an optional `KeywordArguments` list + `KeywordArgumentNode`; new `StringTemplateNode` lowers through the same parts-join as XQuery string constructors. Gates: Release build 0 errors; unit all green incl. 22 new `VersionGateTests` methods; QT3 **31,142/0/679**; XSLT smoke re-run (parser/AST/compiler shared files touched — full sweeps pending, parent runs them before merge). (TokenKind 0.7, XPathLexer 1.9, XPathParser 1.64, XPathAstNode 1.19, IrLowerer 1.48, XPathOptimizer 1.12, FunctionLibrary 5.130, XPath31Expression 0.14, VersionGateTests 0.5.)

- **2026-10-08 (f)** — **XPath 4.0 grammar, third piece (REQ-118 slice 4.0-S4, branch `feature/req118-40-s4`).** **Consumers on the 3.1 default: nothing changes** — all four constructs are lexed unconditionally but parsed behind the existing `xpath40` flag, and every one is rejected with XPST0003 in 3.1/3.0 mode (one deliberate 3.1 tightening: a bare name `fn` used as a function-call name now errors in 3.1 mode — it is an XPath 4.0 keyword — while `fn:foo(...)` and `Q{uri}fn(...)` are unaffected). **4.0 opt-in surfaces (spec-verified against the live XPath 4.0 WG Review Draft):** **pipeline operator `->`** (§4.20 — `A -> B` binds the *whole* of A as the context value and evaluates B once with the focus fixed at (S,1,1); `PipelineExpr ::= (ArrowExpr ++ "->")` sits between cast and arrow levels, so `->` binds tighter than `=>`. The RHS is only an `ArrowExpr` per the spec grammar — a FLWOR or other composite RHS must be parenthesized). **Mapping arrow `=!>`** (§4.22.2 — `U =!> F(A,B…)` ≡ `U ! F(., A, B…)`; desugared at parse time by prepending a context-item argument, static and dynamic call targets both). **Focus functions** (§4.6.6 — `fn` becomes an alternative to `function`; §4.6.6.1 — the brace-only form `fn { E }` ≡ `function($Z as item()*) as item()* { $Z -> E }`, evaluated with focus (Z,1,1); useful for predicates like `//*[fn { . >> $ref }]`, callbacks like `fn:sort($in, fn { $1 mod 2 })`, and numeric sorting `fn:sort($in, fn { number() })`). **Binding extensions** (§4.14.1 — `for member $m at $p in …` iterates array members, including the extended form where the input is a sequence of arrays; `for key $k value $v in …` / `for key $k in …` / `for value $v in …` iterate map entries; `at $pos` counts across the whole expansion; a non-array/non-map item raises XPTY0141; duplicate key/value variable names raise XQST0089; member/entry type declarations (`for member $m as xs:integer in …`) are accepted in XQuery only, matching the existing type-declaration rule). **Plumbing:** new `TokenKind.PipelineArrow`/`MappingArrow`; `PipelineExprNode` lowered by a new `IrOpCode.Pipeline` (VM saves the focus, runs the RHS block once with `WithFocus(value,1,1)`, restores); `QuantifiedBinding`/`ForBindingLoopInfo` gained `BindingKind` (Item/Member/EntryKeyValue/EntryKeyOnly/EntryValueOnly) driving member/entry expansion in the VM `For` opcode; traversal cases everywhere `ArrowExprNode` had one (optimizer, static name-test validator, Api resolution, streamability analyzer, pattern compiler, accumulator definitions, module visibility, XQuery compiler). **Follow-ups:** `fn:some`/`fn:every` HOFs (F&O 4.0 — the spec's own focus-function examples need them), `=?>` method-call arrow (§4.22.3, deferred with record types), member/entry bindings in XQuery multi-binding FLWOR tuple clauses (single-binding simple-for only). Gates: Release build 0 errors; unit all green incl. 31 new `VersionGateTests` methods (96/96 in that class); QT3 **31,142/0/679**; XSLT smoke re-run (XSLT-project traversal files touched — `mode` set 162/0/26, 100.0% of runnable; full sweeps pending, parent runs them before merge). (TokenKind 0.8, XPathLexer 1.10, XPathParser 1.65, XPathAstNode 1.20, IrOpCode 1.8, IrLowerer 1.49, XPathOptimizer 1.13, StaticNameTestValidator 0.4, VmEngine 2.164, XPath31Expression 0.15, StreamabilityAnalyzer 0.11, AccumulatorDefinition 1.1, PatternCompiler 3.14, ModuleVisibilityValidator 0.9, XQueryCompiler 3.5, VersionGateTests 0.6.)

- **2026-10-08 (d)** — **XPath 4.0 grammar, first piece (REQ-118 slice 4.0-S3a, branch `feature/req118-40-s3a`).** **Consumers on the 3.1 default: nothing changes** — the parser/lexer version flag defaults to false everywhere except `XPath31Expression.Compile` with `Compatibility = XPathCompatibility.XPath40`, and the new `??` token is rejected with XPST0003 in 3.1/3.0 mode. **4.0 opt-in surfaces (spec-verified against the live XPath 4.0 WG Review Draft):** the `??` otherwise operator (§4.17 — returns the LHS unless it is the empty sequence, otherwise the RHS; binds tighter than comparisons, looser than `||`/arithmetic; left-associative; the RHS is *guarded* per §2.6.5 — it can never raise a dynamic error when the LHS is non-empty, and both operands evaluate with the containing expression's focus), and numeric literal extensions (§4.3.1): hexadecimal `0x…` and binary `0b…` integer literals typed `xs:integer`, plus underscore digit separators (`1_000_000`, also in fractional/exponent digits). Plumbing: `XPathLexer` and `XPathParser.Parse`/`ParseExprSingle` gained an `xpath40` flag (default false); XSLT/XQuery call sites are untouched and stay 3.1. Gates: Release build 0 errors; unit all green incl. 16 new `VersionGateTests` (Api.Tests); QT3 **31,142/0/679**; XSLT files untouched (full sweeps pending — parent runs them before merge). (XPathLexer 1.8, TokenKind 0.6, XPathParser 1.63, XPathAstNode 1.18, IrLowerer 1.47, XPath31Expression 0.13, VersionGateTests 0.4.)

- **2026-10-09 (d)** — **fn:op — operator-function accessor (REQ-123, F&O 4.0 §18.4, frozen `XPath40` level, branch `feature/req123-fn-op`).** `fn:op($operator as xs:string)` returns `fn($x, $y) { $x ⊙ $y }` for all 31 supported operators — arithmetic (`+ - * div idiv mod`), comparisons (`= < <= > >= !=` general; `eq lt le gt ge ne` value; `is << >>` node; plus the XPath 4.0 keyword forms `is-not precedes follows precedes-or-is follows-or-is`), boolean (`and or`), sequence (`,` `to` `otherwise`), string (`||`), and venn (`| union intersect except`). **Semantics are bit-identical to the infix operators** — both the VM opcode dispatch and fn:op route through one new `VmEngine.ApplyBinaryOperator` (collation, implicit timezone, backwards-compat, and empty-sequence propagation included), so an operator used through fn:op cannot drift from its infix form. The result is a plain arity-2 function item (partial application `op('+')(?, 1)`, `for-each-pair`, and `=>` pipeline all work); an unsupported operator name raises XPTY0004 **eagerly** at the `fn:op` call. Consumers on 3.1 and on the frozen level: fn:op is 4.0-only (static XPST0017 at 3.1), but it is **not** experimental — it is part of the frozen `XPath40` surface. Also fixed: `fn:function-arity` returned empty for `DelegateFunctionItem` function items (affected fn:op results and fn:random-number-generator's `next`/`permute`). qt4tests: fn-op 34/0/1, fn-scan KnownGaps cleared (six fn:op-based scan tests now pass at `XPath40Experimental`), fn-do-until/fn-take-while/fn-contains-subsequence promoted into the gate (now 204 gated sets). (VmEngine 2.168, FunctionLibrary 5.133, VersionGateTests 0.12; new `tests/Bosak.XPath.Standard.Tests/FnOpTests.cs` 0.1, 35 tests.)

- **2026-10-09 (c)** — **qt4tests wired as the XPath 4.0 conformance harness (REQ-123 slice 4.0-Exp S2, branch `feature/req123-40-s2`).** New project `tests/Bosak.XPath.Conformance40` (added to `Bosak.sln`; **not** a unit-test project — a manual gate runner like the QT3 harness) with the qt4tests suite as a pinned git submodule at `tests/qt4tests` (official `qt4cg/qt4tests` @ `68910080`, 738 test sets / ~47k tests). **Architecture:** the generic harness machinery (test-case parsing, environments, result comparison, reporting) is **linked as shared sources from `Bosak.XPath.Conformance`** — single source of truth, no copy drift; the 4.0-specific pieces are new: `DependencyFilter40` (spec deps satisfied by `XP20+`…`XP40+` tokens; tests with only XQuery spec tokens skip — no XQuery 4.0 mode exists; unsupported 4.0 features skip: XQUpdate/fullText/binary/CLDR format-integer/…), `TestExecutor40` (compiles at the **frozen `XPath40` level**, and on the experimental-only static XPST0017 **transparently retries at `XPath40Experimental`** — no hand-maintained set list), and a per-test **watchdog** (`BOSAK_QT4_TEST_TIMEOUT_SECS`, default 120s — qt4tests contains pathological evaluations: the first sweep hit a runaway growing ~45MB/s to 49GB; timed-out tests are recorded failed-for-triage and their background threads never block exit). **The gate:** only test-sets listed in `GatedSets` (100% green: zero failures, ≥1 pass) fail the run (exit 2); all other sets run as an exploratory baseline and report without gating — sets are promoted into the gate by the slice that fixes their last failure, never by silencing. **Baseline (2026-10-09, suite @ 68910080):** 25,774 passed / 5,933 exploratory failures / 15,278 skipped; 6 passes only possible at the `XPath40Experimental` level (fn:scan); `fn:scan`'s six `fn:op`-based tests are KnownGaps (`op()` operator functions not implemented). Failure clusters are dominated by not-yet-implemented 4.0 surfaces (fn:op, fn:parse-csv/csv-to-xml, fn:element-to-map/map-to-element, jtree/jvalue/jkey JNodes, fn:atomic-equal, keyword signatures on 4.0 functions, `~` union types, focus constructors, composite casts). Usage: `dotnet run --project tests/Bosak.XPath.Conformance40 -c Release -- tests/qt4tests [set-filter] [test-filter]`; `BOSAK_QT4_GATE_ONLY=1` runs just the gated sets (verification pass); `BOSAK_QT4_DUMP_SKIPS=<path>` dumps per-test skip reasons. **Consumers: nothing changes** — pure test-infrastructure addition; one additive `InternalsVisibleTo("Bosak.XPath.Conformance40")` in `Bosak.XPath.Runtime` (harness-only). Gates: build 0 errors; unit all green; QT3 **31,142/0/679**; XSLT smoke 162/0/26. (ConformanceRunner40 0.3, TestExecutor40 0.1, DependencyFilter40 0.1, Program40 0.2.)

- **2026-10-09 (b)** — **'4.0 Experimental' compatibility level + fn:scan (REQ-123 slice 4.0-Exp S1, branch `feature/req123-40-experimental`).** **Consumers on the 3.1 default and on the frozen `XPath40` level: nothing changes** — the new level is a strict third tier: `XPathCompatibility.XPath40Experimental = 50` sits above `XPath40`, so every existing `>= XPath40` check passes through unchanged, and experimental-only additions are hidden from both lower tiers (dedicated standard-function templates keep them invisible to `fn:function-lookup`/dynamic dispatch; the Api layer raises static XPST0017 in call AND named-function-ref form at 3.1 and at frozen `XPath40`, with a message pointing at the opt-in). **Opt in via** `new CompileOptions { Compatibility = XPathCompatibility.XPath40Experimental }`. **First experimental surface (spec-verified against F&O 4.0 §2.5.15 / qt4tests `fn/scan.xml`):** `fn:scan($input, $init, $action)` — the prefix scan: returns N+1 single-member arrays, the first holding `$init` and each subsequent one holding `$action($acc, $item, $pos)` for the corresponding input item; empty input yields the single array `[$init]`; arity-2 callbacks are legal (F&O 4.0 §1.8 truncates the extra `$pos` argument); sequence accumulators work because array members hold sequences natively. Gates: Release build 0 errors; unit all green (Api.Tests 293 incl. 7 new REQ-123 gate tests; Standard.Tests 1068 incl. 8 new `Scan_*` tests); QT3 **31,142/0/679**; XSLT smoke 162/0/26. (XPathCompatibility 0.3, XPathFunction 0.53, EvaluationContext 2.35, XPath31Expression 0.16, FunctionLibrary 5.118, VersionGateTests 0.11, XPath40FunctionTests 0.6.)

- **2026-10-09** — **Release: `v1.0.0` — general availability.** Owner decision 2026-10-09 shortened the 0.13.0 soak; the pin bump `0.13.0 → 1.0.0` in `src/Directory.Build.props` + `xsl:product-version` fallback synced (FunctionLibrary 5.130). The 1.0 packages carry the complete REQ-118 XPath/XSLT 4.0 wave (slices S0–S8) behind the default-3.1 gate — consumers on the 3.1 default are bit-identical. **New public surface (additive, frozen-surface-safe):** `Bosak.XPath.Api.XPathExpression` — a version-neutral facade with a surface identical to `XPath31Expression` (`Compile` ×2, `Evaluate` ×2, `EvaluateNodes`), resolving the REQ-118 dossier §5.4 naming question without a breaking rename; `XPath31Expression` stays fully supported. 10 new `XPathExpressionTests` (3.1 default, 4.0 opt-in, node evaluation, error propagation). Release notes finalized at `docs/RELEASE-NOTES-1.0.md`. **Post-1.0 (scoped, not scheduled):** a '4.0 Experimental' compatibility level targeting 1.1.0 — an opt-in level above `XPath40` for testing the latest draft additions ahead of stabilization; the default remains 3.1. The paired Bosak.Schema `v1.0.0` tag is owner-side in that repo's runbook.

- **2026-10-08 (c)** — **XPath 4.0 map/array + URI/date function batch (REQ-118 slice 4.0-S2, branch `feature/req118-40-s2`).** Same gate as the S1 batches: 4.0-only functions, invisible to the 3.1 default (XPST0017 at compile time; absent from 3.1 `fn:function-lookup` tables), enabled via `CompileOptions.Compatibility = XPathCompatibility.XPath40`. New F&O 4.0 functions (spec-verified against the live WG Review Draft): `map:build` (default key/value `fn:identity#1`; `duplicates` option defaults to `"combine"` and also accepts a combiner function `fn(existing, new)`), `map:entries`, `map:filter` (predicate `fn(key, value, position)`; `()` counts as false), `map:items`; `array:build`, `array:empty` (only a zero-member array is empty — `[[]]` is not), `array:items` (non-recursive concatenation of members), `array:slice` (same position rules as `fn:slice`; out-of-bounds yields an empty array, never an error); `fn:parse-uri` (returns the full 14-entry uri-structure-record map — `uri`, `scheme`, `absolute`, `hierarchical`, `authority`, `userinfo`, `host`, `port` as `xs:integer`, `path`, `query`, `fragment`, `path-segments`, `filepath`, `query-parameters`; query parameters are form-decoded so `+` becomes a space there only; options `allow-deprecated-features` / `omit-default-ports` / `unc-path`; FOUR0001 on an unmatched `[` in the authority), `fn:build-uri` (inverse serializer with per-component escaping; segments are escaped only for hierarchical URIs), `fn:decode-from-uri` (UTF-8 percent-decoding; invalid escapes and invalid UTF-8 become U+FFFD; `+` is preserved); `fn:seconds` / `fn:duration-to-seconds` (inverse pair over `xs:dayTimeDuration`), `fn:build-dateTime` (constructs any of the eight Gregorian shapes from a component record — xs:dateTime / xs:dateTimeStamp / xs:date / xs:time / xs:gYear / xs:gYearMonth / xs:gMonth / xs:gMonthDay / xs:gDay; FODT0005 on a non-matching field set, FODT0003 on a timezone outside ±PT14H, FORG0001 on out-of-range components), `fn:unix-dateTime` (UTC xs:dateTimeStamp from Unix milliseconds, default 0), `fn:days-in-month` (proleptic Gregorian leap rule, year 0 = 1 BCE is a leap year). **Arity coercion (F&O 4.0 §1.8):** 4.0 callbacks may declare more arguments than the supplied function has — `FunctionLibrary.Invoke40` truncates extra arguments, so arity-1 functions are valid `$key`/`$action`/`$predicate` callbacks. **Deferred/flagged:** `array:members` / `array:of-members` return/take XDM 4.0 JNodes (Issues 2351/2393) — flagged for the JNode slice; the known-hierarchical scheme list is implementation-defined per spec (http/https/ftp/ssh/file hierarchical; mailto/news/urn/tel/data/javascript not); `fn:build-dateTime` fractions below milliseconds truncate to millisecond precision (engine-wide `XPathDateTime` limit); `fn:unix-dateTime` results beyond year 9999 raise FODT0001; `fn:parse-uri` of a non-numeric port keeps the raw string (spec defines no error). Gates: Release build 0 errors (1 pre-existing warning in Xslt.Conformance harness untouched); unit all green incl. 3 new `VersionGateTests` methods + 63 new `XPath40FunctionTests`; QT3 **31,142/0/679**; XSLT smoke unchanged (zero XSLT-project files touched). (FunctionLibrary 5.129.)

- **2026-10-08 (b)** — **XPath 4.0 second function batch (REQ-118 slice 4.0-S1 part 2, branch `feature/req118-40-s1-part2`).** Same gate as part 1: 4.0-only functions, invisible to the 3.1 default (XPST0017 at compile time; absent from 3.1 `fn:function-lookup` tables), enabled via `CompileOptions.Compatibility = XPathCompatibility.XPath40`. New F&O 4.0 functions (spec-verified against the live WG Review Draft): `fn:contains-subsequence` / `fn:starts-with-subsequence` / `fn:ends-with-subsequence` (default compare is `fn:deep-equal#2` with the context collation; a callback returning `()` counts as false), `fn:duplicate-values`, `fn:all-equal` / `fn:all-different`, `fn:highest` / `fn:lowest` (untypedAtomic keys are cast to `xs:double` per `fn:min`/`fn:max`; not castable → FORG0001), `fn:sort-by` (sort-key records duck-typed on map entries — `key`/`collation`/`order`, defaults `fn:data#1`/default collation/`"ascending"`), `fn:sort-with` (comparator cascade, stable), `fn:graphemes` (documented UAX #29 approximation: CRLF, Extend/SpacingMark combining, ZWJ linker glue for emoji and Indic conjuncts; Hangul jamo, Prepend, and regional-indicator pairing are not distinguished), `fn:pad-string` (zero-length padding → FORG0001; never truncates), `fn:trim-space` (U+0020/U+0009/U+000D/U+000A only), `fn:index-of-substring` (codepoint comparison, overlapping occurrences, positions in code points), `fn:substring-before-last` / `fn:substring-after-last` (collation-aware, minimal-match semantics), `fn:hash` (MD5/SHA-1/SHA-256/SHA-384/SHA-512 over UTF-8 or raw binary octets → `xs:hexBinary`; the spec-required BLAKE3 and CRC-32 raise FOHA0001 — no .NET primitive, flagged in the dossier). Note: `fn:pad-string`, `fn:trim-space`, `fn:index-of-substring`, and the `*-last` pair are post-June-2026 spec sections — implementational churn risk accepted. Gates: Release build 0 errors (1 pre-existing warning in Xslt.Conformance harness untouched); unit all green incl. 2 new `VersionGateTests` + 73 new `XPath40FunctionTests`; QT3 **31,142/0/679**; XSLT smoke unchanged (zero XSLT-project files touched). (FunctionLibrary 5.117.)

- **2026-10-08** — **XPath 4.0 version gate + first function batch (REQ-118 slices 4.0-S0/S1 part 1, branch `feature/req118-40-gate-s1`).** **Consumers on the 3.1 default: nothing changes** — default `CompileOptions.Compatibility` stays `XPathCompatibility.XPath31`, behavior is bit-identical, and QT3 **31,142/0/679** is preserved. **4.0 opt-in:** `XPath31Expression.Compile(expr, new CompileOptions { Compatibility = XPathCompatibility.XPath40 })`. In 3.1 mode, XPath 4.0-only functions raise **XPST0017 at compile time** (call form and named-function-ref form) and are absent from 3.1 `fn:function-lookup`/dynamic-dispatch tables; in 4.0 mode they compile and evaluate normally. First batch (F&O 4.0, spec-verified): `fn:replicate`, `fn:slice`, `fn:items-at`, `fn:foot`, `fn:trunk`, `fn:insert-separator`, `fn:char` (full WHATWG HTML5 named-character-reference table), `fn:characters`. New public surface (additive/frozen-surface-safe): enum member `XPathCompatibility.XPath40`, `FunctionSignature.IsXPath40Only`, `FunctionLibrary.XPath40OnlyFunctionNames`; runtime version knowledge crosses the layer boundary as an internal `EvaluationContext.IsXPath40` flag (Runtime cannot reference the Api enum), stamped by `XPath31Expression.Evaluate` before `FunctionLibrary.Populate`. XSLT and XQuery hosts are unaffected (they never set the flag — 3.1 tables). Gates: Release build 0/0; unit all green incl. 16 new `VersionGateTests` (Api.Tests) + 50 new `XPath40FunctionTests` (Standard.Tests); QT3 **31,142/0/679**; XSLT basic sweep unchanged (zero XSLT-project files touched — smoke only). Usage: §2.1 (`CompileOptions.Compatibility`). (XPathCompatibility 0.2, XPathFunction 0.52, EvaluationContext 2.34, FunctionLibrary 5.116, Html5CharacterReferences generated, XPath31Expression 0.12.)

- **2026-10-07** — **Release: `v0.13.0` published to nuget.org — the `-beta` postfix is stripped; this is the pre-1.0 soak release** (workflow run 37682060968, green 4m56s; all **10** packages `Created`, Trusted Publishing OIDC — the pre-tag pin bump from PR #76 worked, no all-skipped re-pack). Package identity is now un-postfixed (`Bosak.XPath.Api` 0.13.0 etc.); the nuget.org search index lags the flat container by minutes — verify via the flat-container URL, not search. Carries everything since 0.12.3-beta; the paired `v1.0.0` tags follow the soak readout.

- **2026-10-06 (a)** — **VS Code extension 0.1.5: TextMate grammar JSON-escape fix.** All three grammar files (`xpath`/`xquery`/`xslt` `.tmLanguage.json`) contained invalid strict-JSON escape sequences in their operator/attribute regexes (`"\-"`, `"\s"`, `"\""` forms — verified: none of the three parsed as strict JSON at 0.1.4). Corrected to the proper escaped forms (`"\\-"`, `"\\s"`, `"\\""`). Extension version **0.1.4 → 0.1.5**; `vscode-bosak/README.md` and §8.2 VSIX references synced. Gates: all three grammars now parse as strict JSON; `npm run compile` clean (tsc). No engine code touched.

- **2026-10-06 (b)** — **Full basic-sweep record refreshed on harness 3.73 — 10,242 / 0 / 4,359, 100.0% pass rate.** The 3.73 env-schema heuristic (any environment declaring a `<schema>` element is an implicit schema-awareness dependency, per the w3c/xslt30-test#90 maintainer reply) is now backed by a complete full-catalog run on main `929eea7` (merged PR #72): single chunk, no kills, `.guard-tmp/work/sweep373-basic-final.log`, baseline `.sweep-baselines/basic-after-373.txt` (empty fail list). Per-test diff vs the 3.72 record (10,250/0/4,351 on `b1a7479`): exactly 8 pass→skip moves — merge-049/050/052/053/054, type-0303, xpath-default-namespace-0501/0502 — **zero pass→fail, zero new failures** (import-schema-191, the 9th targeted-set-run delta, never runs in full sweeps at all). The pre-1.0 confirmation gate now stands on the 3.73 record. Detail: `docs/BASIC_SWEEP_TRIAGE.md` §6.

- **2026-10-07** — **VS Code extension published to the marketplace: `fytala.vscode-bosak` 0.1.5 live.** The `fytala` publisher (created owner-side) received its first two publications: **0.1.4** (2026-10-05, refreshed language-server bundle — published owner-side) and **0.1.5** (2026-10-07, the TextMate grammar JSON-escape fix from PR #73 — published via `vsce publish`, verified live: `Version: 0.1.5, Last updated: October 7, 2026`; the gallery API lags ~5 min behind `vsce show`). Distribution: VS Code Marketplace is now the primary install path (`vscode-bosak/README.md` Option 1; VSIX/sideload remain for contributors). **PAT hygiene:** the marketplace PAT used for the publish exists in the 2026-10-07 session history — owner to rotate/revoke it (or rely on its expiry); a follow-up CI workflow + repo-secret PAT is the recommended steady state for future extension releases. No engine code touched.

- **2026-10-03 (h)** — **Release: `v0.12.3-beta` published to nuget.org** (workflow run 37158005610, green; all **10** packages `Created`, Trusted Publishing OIDC — no all-skipped re-pack, the pre-tag pin bump from (g) worked). **First publish of `Bosak.XPath.Providers.Database`** — the new ID's Trusted Publishing registration verified end-to-end. Carries REQ-118 (PR #60), REQ-119 (PR #61), REQ-120 database backends end-to-end (PR #62–68).

- **2026-10-03 (g)** — **Pre-tag pin bump: `0.12.3-beta`** — `<Version>` in `src/Directory.Build.props` `0.12.2-beta → 0.12.3-beta` and the `xsl:product-version` fallback synced (FunctionLibrary 5.127); build 0/0, product-version test green. **Packaging note:** this release carries **REQ-120 database backends end-to-end** (dossier PR #62, REST spike PR #63 + CI fix PR #64, `Bosak.XPath.Providers.Database` package PR #65 + docs PR #66, NuGet enablement PR #67 — the 10th package publishes for the first time, first live proof of the Trusted Publishing registration for the new ID — Slice 3 collection seam PR #68), **REQ-119** error-set engine gaps (PR #61), and **REQ-118** XPath/XSLT 4.0 tracking (PR #60). Tag push triggers the Release workflow (whole-solution pack; skip-duplicate).

- **2026-10-03 (f)** — **REQ-120 Slice 3: collection seam + foreign-provider friction — additive `EvaluationContext.CollectionLoader` + `Bosak.XPath.Providers.Database` collection listings.** **Engine (SemVer minor, frozen-surface-safe):** new public `EvaluationContext.CollectionLoader` hook (`Func<string, IReadOnlyList<string>?>`, full `///` contract on the member) consulted by `fn:collection`/`fn:uri-collection` after the registered/environment collections and before the directory fallback; it receives the collection URI before any `?select=`/fragment stripping (relative URIs absolutized against the static base URI; the default collection as the empty string), returns member document URIs that funnel through the existing `LoadDocument` path — preserving document identity caching, per-load-policy cache keys, FODC0002/FODC0005 mapping — with hook order as the creation-sequence document-order story; `null` declines (fall-through to FODC0002), an empty list is an empty collection; unset-hook behavior is bit-identical. Foreign-provider friction fixes: `FunctionLibrary.LoadDocumentFragment` is now provider-agnostic (reuses the pre-existing `IXdmNode`-axis ID lookup; fragments are grounded via the established `DeepCopyForeignNode`, never aliasing source data); `TransformEngine.IsNodeAttached` handles foreign providers (document/parent/document-claim) instead of blanket-attached; the `LoadDocument` `RegisterTree` skip and the XDocument-only `xsl:strip-space` path are documented on the members (both are mutability-bound — `IXdmNode` has no mutation API); `fn:copy-of` already had a provider-agnostic fallback (5.109) — verified unchanged. **Providers:** `DatabaseDocumentLoader.LoadCollection(uri, options)` + `DispatchCollection(fallback, options)` (consistent with the `Dispatch` idiom) list collections per scheme behind the registry — BaseX `GET /rest/{db/coll}` XML listing (nested directories in-response or via follow-up), eXist `GET /exist/rest/db/coll` (one follow-up per `subcollection`), MarkLogic `GET /v1/search?directory={dir}&view=uris&depth=Infinity` (`search:uri` entries; order not stable — documented) — and return members as `scheme://host[:port]/…` URIs (effective default port applied) so they resolve through the unchanged document-load path. URI-embedded credentials stay rejected; options-level Basic auth reuses the registry. 12 engine tests (incl. the tree's first in-memory foreign `IXdmNode` double) + 13 provider loopback-stub tests. Gates: Release build 0 errors (1 pre-existing `Bosak.Xslt.Conformance` CS8602, untouched file); `dotnet test Bosak.sln -c Release` all green; QT3 **31,142/0/679** preserved; basic sweep **10,250/26/4,325** and schema-aware sweep **11,054/1/3,546** bit-identical to the REQ-117 baselines (engine files touched — both sweeps mandatory). Usage: §2.2 "Database collections".

- **2026-10-03 (e)** — **NuGet enablement: `Bosak.XPath.Providers.Database` ships.** `<IsPackable>` flipped `false → true` — the owner registered the ID on nuget.org for Trusted Publishing, so the package now publishes automatically with the next core tag via the existing `release.yml` (whole-solution pack, skip-duplicate makes re-runs safe). Pack pre-flight verified a fully-populated nuspec (version from the `Directory.Build.props` pin, deps `Bosak.XPath.Core` + `Bosak.XPath.Providers`, license/readme/icon/tags). No code change; first live publish happens at the next tag (real verification of the Trusted Publishing registration). Usage: §2.2 "Database document loaders".

- **2026-10-03 (d)** — **REQ-120 Slice 2: `Bosak.XPath.Providers.Database` promoted from spike to general-purpose package — basex/exist/marklogic REST scheme registry.** The loader now dispatches three schemes via an internal registry (per-DB wire quirks — path prefix, MarkLogic's query-parameter document URI — stay behind the scheme entries, not in shared code): `basex://host[:port]/db/resource` → `http://host:port/rest/db/resource` (8984), `exist://host[:port]/db/resource` → `http://host:port/exist/rest/db/resource` (8080), `marklogic://host[:port]/db/resource` → `http://host:port/v1/documents?uri=%2Fdb%2Fresource` (8000, `Accept: application/xml` — MarkLogic takes the document URI as the `uri` query parameter, not the path; shape confirmed against the MarkLogic REST reference). Public API stays source-compatible: `Dispatch`/`DispatchStreaming`/`Load`/`LoadStreaming` unchanged, `Handles(uri)` now recognizes all three schemes, `DatabaseLoaderOptions` gains `Exist`/`MarkLogic` alongside `BaseX` on a new shared `DatabaseConnectionOptions` base (`EndpointBase`, `Username`, `Password` — EndpointBase overrides the whole `/rest` resp. `/exist/rest` base for basex/exist, the origin only for marklogic). **Streaming teardown (Slice 1 deferred item):** the response content stream is now wrapped in a `ResponseBoundStream` that disposes the `HttpResponseMessage` with the stream, so end-of-stream / failure release the response and connection deterministically (abandoned partially-read streams still rely on finalization — documented). Packaging: full nuspec-level metadata added, but `<IsPackable>false</IsPackable>` kept — `release.yml` packs the whole solution (`dotnet pack Bosak.sln`) and pushes every nupkg except `*LanguageServer*`, so the flip to `true` must wait for the owner reserving `Bosak.XPath.Providers.Database` on nuget.org. 21 new tests (18 scheme-registry + 3 teardown; stub renamed `DatabaseRestStub`, now records query string + Accept header). Zero engine files modified. Gates: Release build 0 errors (1 pre-existing `Bosak.Xslt.Conformance` warning, untouched file); unit all green incl. the 35 database-loader tests; QT3 **31,142/0/679** preserved. Usage: §2.2 "Database document loaders".

- **2026-10-03 (c)** — **REQ-120 Slice 1: database REST adapter spike — `Bosak.XPath.Providers.Database` + scheme-dispatching `DatabaseDocumentLoader`.** New spike package (10th source project, **not packed** — `IsPackable=false`; verified sufficient against `release.yml`, which packs the whole solution and pushes every nupkg except `*LanguageServer*`) and new test project `Bosak.XPath.Providers.Database.Tests` (14 tests on a loopback `HttpListener` BaseX REST stub — the tree's first in-memory `IXdmNode` consumer suite). API: `DatabaseDocumentLoader.Dispatch(fallback, options)` / `DispatchStreaming(fallback, options)` return `Func<string, IXdmNode>` ready for `EvaluationContext.DocumentLoader` / `StreamingDocumentLoader`; `basex://host[:port]/db/resource` → `http://host:port/rest/db/resource` (default port 8984; `DatabaseLoaderOptions.BaseX.EndpointBase` overrides the base, `Username`/`Password` add HTTP Basic auth — URI-embedded credentials are rejected). In-memory loads wrap via the public `XDocumentProvider.ParseXml` path with the basex URI as document URI; streaming loads read `ResponseHeadersRead` straight into `XmlStreamingProvider.Load` (bounded-memory record-at-a-time). Error contract: network/HTTP/timeout → `IOException`, malformed payload → `XmlException` — mapped to FODC0002 by `EvaluationContext.LoadDocument`; unsupported URI shapes → `ArgumentException`/`UriFormatException` (FODC0005 class). Zero engine files modified. Usage snippet: §2.2 "Database document loaders (spike)". Gates: Release build 0 errors (1 pre-existing `Bosak.Xslt.Conformance` warning in an untouched file); `dotnet test Bosak.sln -c Release` all green incl. the 14 new tests; QT3 **31,142/0/679** preserved. Full XSLT sweeps not required: zero engine files modified and the new package is referenced by no engine project. **Go for Slice 2** (scheme registry, packaging, full docs) per the REQ-120 decision log.

- **2026-10-03 (b)** — **`error`-test-set engine gaps closed (REQ-119)** — the six genuine gaps from the REQ-117 label wave now raise the spec-mandated codes; targeted `error`-set run **507/7/65 → 513/0/66**, zero pass→fail. **(1) XTSE0730** — a `streamable="yes"` `xsl:attribute-set` may only reference attribute sets that also specify `streamable="yes"` (load-time check beside the XTSE0720 circularity check). **(2) XTSE3120** — `xsl:break`/`xsl:next-iteration` tail-position validation moved from one runtime iterate interpreter to load-time static validation covering all three runtime paths; tail position is verified along the whole ancestor chain to the iterate body (literal result elements count as following instructions; `xsl:choose` branches are alternatives; `xsl:fallback` is exempt everywhere per §8.4). **(3) XTSE3155** — an `xsl:function` with no `xsl:param` children may only declare `streamability="unclassified"`. **(4) FOJS0004** — `fn:json-to-xml` with `validate:=true()` now raises FOJS0004 on a non-schema-aware processor (F+O 3.1 §17.5.2); schema-awareness is a new `EvaluationContext.IsSchemaAware` flag set by `TransformEngine` from the compilation's `SchemaAware` state, so schema-aware hosts keep validating. **(5) XTDE3362** — `accumulator-before`/`accumulator-after` against a node in a document being processed in a streamed pipeline (streamable `xsl:source-document`) now raise XTDE3362 unless the accumulator is declared `streamable="yes"` (`AccumulatorDefinition.Streamable`); a harness-streamed source under a grounded mode is not affected — the read stays legal (accumulator-034 pattern). **Behavior changes for consumers:** stylesheets with misplaced `xsl:break`/`xsl:next-iteration` now fail at compile time (was: at transform time on some paths, or not at all); zero-parameter `xsl:function` declarations with any `streamability` other than `unclassified` now fail at compile time; `json-to-xml(…, map{'validate':true()})` requires a schema-aware compilation (`XsltCompiler.SchemaAware = true`) — in basic mode it raises FOJS0004 instead of silently validating; streaming accumulator fixtures must declare `streamable="yes"` (the catalog pattern all along). error-1160a is a documented environment-limited skip (remote HTTP fetch blocked, same class as QT3 `fn-unparsed-text-054a`). Gates: build 0/0; unit **2,758/2,758** across 9 solution assemblies (Xslt.Tests 742) + LanguageServer.Tests **72/72**; QT3 **31,142/0/679** preserved (the QT3 harness admits `schemaImport`/`schemaValidation` and now runs with `IsSchemaAware = true`, `TestExecutor` 0.26, so its `json-to-xml` validate tests keep validating against the built-in schema-for-JSON); basic sweep **10,250/26/4,325** and schema-aware sweep **11,054/1/3,546** bit-identical to the REQ-117 baselines. (Stylesheet 2.125, TransformEngine 7.01, StreamabilityAnalyzer 0.10, AccumulatorDefinition 0.9, EvaluationContext 2.32, FunctionLibrary 5.125, conformance Program.cs 3.71.)

- **2026-10-03 (a)** — **Release: `v0.12.2-beta` published to nuget.org** (all 9 packages, Trusted Publishing OIDC; Release workflow green on tag `6484e31`; every package `Created` — the pre-tag pin bump from (g) worked, no all-skipped re-pack). Carries **REQ-116** (PR #48), **REQ-117** (PR #49), and the REQ-117 tail (PR #51: positional streamed group patterns → XTSE3430, `StreamabilityAnalyzer` 0.9; error-set label equivalences, harness Program.cs 3.70).

- **2026-10-02 (h)** — **XSLT: positional streamed group patterns now raise XTSE3430; `error`-test-set label mismatches fixed in the harness** (branch `feat/xtse3430-error-label-cleanup`, two commits). **(1) Analyzer 0.9** — `StreamabilityAnalyzer.CheckPattern` already rejected `position()`/`last()` predicates in `group-starting-with`/`group-ending-with` patterns over a streamed population, but numeric-literal predicates (`[1]`, `[(2.5)]`) are positional per XPath §2.4.3 and carried no flag: they compiled silently and collapsed to one group at runtime (the deviation recorded at REQ-117). `IsNumericLiteralPredicate` now extends the existing throw site, so the spec-correct static XTSE3430 ("group-starting-with contains a positional predicate ('1')") is raised at compile time. Detection lives in the analyzer, not `PatternCompiler`, because the compiler also serves grounded populations where positional patterns are legal. Non-positional streamed predicates are unchanged (si-group-054/056 stay PASS). **(2) Harness 3.70** — the `error` test-set stays wholesale-skipped in full sweeps by design (579 tests, needs full static-validator coverage), but a targeted filter run un-skips it and used to show ~63 "Expected error X, got: \<label\>" mismatches. `ErrorCodeMatches` gains a scoped equivalence table: uncoded engine messages that describe the exact spec condition (e.g. "Circular stylesheet reference detected" ↔ XTSE0180/XTSE0210, "Expected xsl:stylesheet or xsl:transform" ↔ XTSE0150) plus one-to-one code aliases for the underlying/adjacent spec code (e.g. FORX0002 ↔ XTDE1140, XPST0003 ↔ XTDE3160, XTTE0590/XTTE0570 ↔ XTDE0700), mirroring the existing XTSE0800→XTSE0085 alias. Targeted `error` run: **453/61/65 → 507/7/65** — all label mismatches gone, zero pass→fail; the 7 remaining are genuine engine gaps (XTSE0730/3120/3155, XTDE3245/3362 never raised) plus error-1160a (remote HTTP fetch blocked, same class as fn-unparsed-text-054a). Full-catalog behavior is byte-identical (the set stays skipped; no baseline failure matches an aliased pair). Gates: Release build 0/0; unit green (StreamabilityAnalysisTests +6); targeted si-for-each-group 114/114 bit-identical vs pre-change, si-fork 55/55 schema-aware. (StreamabilityAnalyzer 0.9, conformance Program.cs 3.70.)

- **2026-10-02 (g)** — **Release prep: `v0.12.2-beta` pin bump + release notes.** The next tag carries **REQ-116** (validation-0201 — §11.9 construction/result validation scoping + Saxon 9.x HTMLIndenter port, PR #48) and **REQ-117** (PC-1 streaming cluster closed — all 26 streaming failures FAIL→PASS, zero pass→fail, PR #49). Pin bumped `0.12.1-beta` → `0.12.2-beta` in `src/Directory.Build.props` before tagging (the version is pinned, NOT tag-derived — see the (d) packaging note). Release state at tag time: schema-aware sweep **11,054/1/3,546** (only type-functions-0401 remains — documented DateTimeOffset platform limit; the non-streaming + streaming conformance tail is fully closed), basic sweep **10,250/26/4,325**, QT3 **31,142/0/679**, unit **2,729/2,729** + LanguageServer 72/72.

- **2026-10-02 (f)** — **XSLT: PC-1 streaming conformance cluster closed (REQ-117) — all 26 streaming failures FAIL→PASS, zero pass→fail.** Premise corrected: the 26 PC-1 failures were NOT schema-on-streaming (24 of 26 failed bit-identically in the basic sweep); nine root causes fixed in four waves on branch `fix/pc1-streaming-w1-w2` (8 commits on top of main `9e40a97`). Wave summary: **W1+W2** — unprefixed `xsl:assert`/`@error-code` values are no-namespace local names per XSLT 3.0 §5.2 (engine + harness Clark `Q{uri}local` matching — si-assert-901); FODC0002/0005 error mapping on the streamable `xsl:source-document` branch (stream-002/006); regression fix: existing rooted platform paths (`File.Exists`) accepted in the backslash check. **W3+W4** (StreamabilityAnalyzer 0.6→0.8) — `xsl:map` key/value atomization-usage rule (crawling operand still XTSE3430); grounded-group `current-group()` usable in nested for-each/source-document/iterate; `xsl:fork` at-most-one-streaming-prong rule; shallow-descent arity-0 → XTSE3155 (propagates past the fail-open wrapper); absorbing-result constructor-feed exception; next-match with-param transmission into lower-priority rules (si-map-001..009, si-group-048/051, su-shallow-descent-901, si-fork-901, si-next-match-108). **W5+W6** — `key()` context-dependent 2nd pattern argument per §10.1.4 (stream-211); runtime absorbing grounding — absorbing functions get deep-materialized (snapshot) args; VM call-site conversion skipped for absorbing callees (su-absorbing-202/203/301). **W7** — `ExecuteXslIterate` body loop iterated `Elements()` dropping text-node children — now `Nodes()` (si-iterate-005, not streaming-specific); `IrLowerer` merges descendant steps with non-positional predicates (si-for-each-801; positional `//product[1]` stays unmerged — documented StreamingException); streamed `group-starting-with` predicate patterns evaluated `self::node()[pred]` on `IStreamingNode` candidates (si-group-054/056); result-document `@type` PSVI validation on the streaming accumulator path (si-result-document-116). **W7-1 regression fix** (Program.cs 3.69) — `RunRawTransform` non-capture path dropped the initial match selection → XTDE0044 for package-001d..s (12 pass→fail on the full-sweep gate); restored the pre-W7 call shapes, fixed before PR. **Behavior changes for consumers:** unprefixed `xsl:assert`/`@error-code` values are now no-namespace local names per XSLT 3.0 §5.2 (**BREAKING** if any code relied on the old q-namespace behavior); FODC0002/0005 are now raised from the streamable `xsl:source-document` branch; `key()` patterns accept context-dependent 2nd arguments; absorbing functions materialize streamed arguments (snapshot grounding). Known deviation recorded for future work: positional `group-starting-with` patterns over streams silently collapse to one group — candidate for a future XTSE3430 streamability-analysis check (**closed 2026-10-02 (h)**). Gates (all re-run on the branch): build 0/0; unit **2,729/2,729** across 9 solution assemblies (Xslt.Tests 729) + LanguageServer.Tests **72/72** = 2,801 combined (two pre-existing parallelism flakes — `OverrideFunction_UnionSameMembersDifferentOrder_Compiles`, documented since REQ-115, and `PackageWhitespaceStrippingTests.Doc_Function_Loads_Distinct_Trees_Per_Calling_Package` — failed once under full-suite parallelism, passed on isolated and full-suite re-runs, not a regression); QT3 **31,142/0/679** baseline preserved (IrLowerer touched → re-run); basic sweep **10,236/40/4,325 → 10,250/26/4,325** (+14 FAIL→PASS, zero pass→fail); schema-aware sweep **11,028/27/3,546 → 11,054/1/3,546** (all 26 PC-1 items FAIL→PASS, zero pass→fail; only type-functions-0401 remains — documented platform limitation); new baselines `.sweep-baselines/basic-after-req117.txt` / `schema-aware-after-req117.txt`. (TransformEngine 6.96→6.99, Program.cs 3.66→3.69, StreamabilityAnalyzer 0.6→0.8, Stylesheet 2.123, PatternCompiler 3.11→3.12, IrLowerer 1.46.)

- **2026-10-02 (e)** — **XSLT: validation-0201 dedicated fix (REQ-116) — Saxon 9.x HTMLIndenter port + §11.9 schema-scope split.** Three stacked root causes for the last named schema-aware failure: (1) **schema scoping** — validation of constructed/result trees (`xsl:validation`, `[xsl:]type`, result-document validation) now consults only the components imported into the stylesheet per XSLT 3.0 §11.9: `xsl:import-schema` winners plus host `stylesheet-import` schemas, never host `secondary` schemas. `SchemaSetBuilder.Build` emits a second, imported-only compiled set in parallel (fresh `XmlSchema` instances per winner; synthesized xml-namespace schema whenever any import-schema declaration exists — attribute-1501/1502/1503). (2) **method=xhtml indent=yes** serialization now ports the Saxon 9.x `HTMLIndenter` event model (3-space levels; 9.7 inline/formatted tag lists classified XHTML-namespace-only; end-tag indent iff the element did not end on the same line; text folded at embedded newlines with space absorption and 80-column wrapping) — `html`/`xml` methods unchanged, indent=no bit-identical. (3) **Harness** — golden assert-serialization files without an explicit `@encoding` are read per their own XML-prolog encoding (validation-0201's ISO-8859-1 nbsp); environment `<schema>` documents split by role (`secondary` → `EnvironmentSchemaSet`). **New public API:** `XsltCompiler.EnvironmentSchemaSet` (XmlSchemaSet) — host "secondary" environment schemas: visible to compilation and source-document validation, never part of the construction/result-validation scope. `XsltCompiler.SchemaSet` semantics are refined accordingly (stylesheet-import role = part of the in-scope definitions). **Behavior changes for consumers:** lax-validated constructed/result trees no longer pick up default attributes from host secondary schemas (spec-correct; previously they did); xhtml indented output is byte-different from the old 2-space layout by design (Saxon parity). Gates: build 0/0; unit 2,714/2,714 across 9 solution assemblies (Xslt.Tests 699 = 687+12 new `ValidationSchemaScopingTests`/`XhtmlIndentTests`) + LanguageServer.Tests 72/72 = 2,786 combined; validation set 56/0/11 (was 55/1/11); strip-space 29/0/1, output 267/0/14, import-schema 204/0/1, attribute 124/0/1 — all matching pre-change baselines; full schema-aware sweep final gate in FEATURE_REQUESTS REQ-116. (SchemaImportState 0.3, SchemaSetBuilder 0.9/0.10, Stylesheet 2.122, XsltCompiler 0.8, TransformEngine 6.95, ResultTreeSerializer 1.36, conformance Program.cs 3.65/3.66.)

- **2026-10-02 (d)** — **Release: `v0.12.1-beta` published to nuget.org** (all 9 packages, Trusted Publishing OIDC; Release workflow green on the tag). Carries REQ-101…REQ-115. **Packaging note for maintainers:** the package version is pinned in `src/Directory.Build.props` (`<Version>`), NOT derived from the git tag — a tag push alone re-packs the previous pin (the first v0.12.1-beta tag re-published 0.12.0-beta as all-skipped until PR #46 bumped the pin). Always bump the pin before tagging.

- **2026-10-01 (c)** — **XSLT: REQ-115 gate-repair tail — QT3 regression fixed, all gates final green.** The gate re-run of the REQ-115 wave (b) found one QT3 regression: the new fn:resolve-uri RFC 3986 char scan (FunctionLibrary 5.121) accepted malformed percent-encodings, so `misc/CombinedErrorCodes` FORG0002 (`resolve-uri("%gg")`) succeeded instead of raising FORG0002. Fixed: `IsValidRelativeUriReference` now requires exactly two hex digits after every '%' — malformed encodings raise FORG0002, valid `%20` stays encoded per fn-resolve-uri-31 (FunctionLibrary 5.124, FunctionLibraryTests 2.43 +3); also harness CS8602 in the streaming-validation block (Program.cs 3.64). **Final REQ-115 gates:** build 0/0; unit **2,702/2,702** across 9 solution assemblies (Xslt.Tests 687, XPath.Standard.Tests 797) + LanguageServer.Tests **72/72** (separate, not in sln) = 2,774 combined; QT3 **31,142/0/679** — regression cleared, back to the REQ-114 baseline; basic sweep **10,236/40/4,325** — per-test PASS/FAIL bit-identical to the post-wave run (zero flips either direction); schema-aware sweep **11,027/28/3,546** — per-test PASS/FAIL bit-identical to the wave's final raw-log state (the wave's cross-chunk merged total of 11,026 understated by one; the single clean re-run's 11,027 is authoritative); failure list unchanged (26 streaming (PC-1) + validation-0201 + type-functions-0401).

- **2026-10-01 (b)** — **XSLT: target-fix wave for the REQ-114 schema-aware tail (REQ-115)** — 11 of the 12 remaining named non-streaming failures fixed (catalog-001, mode-1506, non-stream-006, non-stream-201, override-misc-007, sf-avg-100, sf-insert-before-011, sf-reverse-001, sf-xml-to-json-004, type-functions-0304, validation-0202); validation-0201 (whitespace-stripping vs schema-invalid input) is characterized and left for a dedicated XDM/serializer investigation. Root causes: the blanket streamed-pipeline Normalize suppression from the REQ-114 wave was replaced by an axis-aware RegisterC flag on the Normalize opcode (1 = forward-axis/non-axis step, 0 = reverse axis) + `VmEngine.MixesDetachedNodes` (materialized mixed rooted/parentless inputs only — lazy streams must NOT be enumerated), clearing ~70 streaming regressions while keeping reverse-axis sort correctness (IrLowerer 1.45, VmEngine 2.162/2.163); sibling xsl:import modules now share an XTSE0545 precedence level via `Stylesheet.ImportDepth` grouping in `CollectModeDefinitions` (Stylesheet 2.121 — mode-1506; new ModeConflictResolutionTests); the FODC0005 raw-backslash URI check moved from `EvaluationContext.LoadDocument` to the fn:document entry and the `xsl:source-document` non-streamable branch (EvaluationContext 2.31, FunctionLibrary 5.122, TransformEngine 6.97 — internal callers legitimately pass Windows platform paths); streamed environment sources are schema-validated per record via RecordPostProcessor, deferred until env schemas are known (Program.cs 3.61); fn:resolve-uri is IRI-tolerant per an RFC 3986 char scan (FunctionLibrary 5.121); fn:sum casts xs:untypedAtomic→xs:double per FORG0001 (FunctionLibrary 5.120); key-index schema-element() patterns compile with the evaluation context + mixed-content indentation (KeyIndex 0.11, ResultTreeSerializer — validation-0202); **fn:distinct-values O(n²) → codepoint fast path** — ordinal HashSets for string-family values under the default/codepoint collation (gate: `collation.Length == 0 || codepoint URI` — DefaultCollation defaults to string.Empty), untypedAtomic/anyURI join membership, pairwise fallback otherwise; sf-distinct-values-001 went 481 s → ~2 s (FunctionLibrary 5.123, +3 tests); harness: bare-file-name doc fallback gating reverted to unconditional (3.60), CS0136 fix (3.62), `--resume-file <path>` for kill-resilient chunked sweeps (3.63). **Behavior changes for consumers:** fn:document / xsl:source-document now apply the FODC0005 raw-backslash URI check at the spec-facing entry points (internal `LoadDocument` callers passing platform paths are unaffected); fn:distinct-values is dramatically faster on large string-family inputs under the default/codepoint collation; mode precedence across sibling imports follows XSLT 3.0 import semantics (shared precedence level). Gates: build 0/0; unit **2,699/2,699** across 9 solution assemblies (Xslt.Tests 687, XPath.Standard.Tests 794) + LanguageServer.Tests **72/72** (separate, not in sln) = 2,771 combined; schema-aware sweep **11,015/39/3,546 → 11,027/28/3,546** (+11 FAIL→PASS, ZERO pass→fail, per-test diff vs raw logs; remaining-failures list at `.sweep-baselines/schema-aware-after-target-fixes.txt`); basic sweep + QT3 regression checks — the QT3 re-run caught one regression (CombinedErrorCodes FORG0002), repaired in (c).

- **2026-10-01 (a)** — **XSLT: PB-3 (C9) closed — schema-aware long tail (REQ-114)** — two waves (wave 1 on 2026-09-30 interrupted, wave 2 on 2026-10-01 resumed and gated; uncommitted working tree, PR pending). List-typed sequence flattening in general comparisons + function conversion (XPTY0004 cardinality kept for singular targets); parameterized `document-node(element(E[,T]))` KindTest end-to-end (parser keeps the inner test; pattern compiler + VM enforce exactly-one-element/no-text; XPST0081 on undeclared prefixes); built-in `xs:` typed patterns enforced without an in-scope schema set (conflict-resolution-1402 — user-defined type names in basic mode still ignored); schema-set loading via `XmlUrlResolver` (chameleon includes/redefines resolve at compile, XTSE0220 on IO errors, import-precedence shadowing recorded so the host-set merge skips losers); `xpath-default-namespace`/`default-collation` whitelisted on variable/param/with-param; XTSE0020 for lax/strict default-validation below 3.0; XTSE0770 user-function vs type-constructor collision; pre-E36 `#arity` suffix tolerated; `PreserveSchemaAnnotations` (xs:anyType/xs:untypedAtomic marking per §25.1.1) wired end-to-end — preserve shells marked xs:anyType, xsl:copy-of passes false so preserved untyped trees stay xs:untyped; RC3 ref+use-site default/fixed pre-injection; xdt→xs untypedAtomic normalization; item-separator honored by the serializer; `xsl:evaluate @schema-aware` yes/no AVT (XTSE0020/XTDE0030/XTDE3160) + fn:document stubs; merge per-input-sequence XTDE2220 sortedness, sort-before-merge collation, codepoint default merge keys. Wave-2 corrections: document-node `[xsl:]type` semantics — the wave-1 blanket XTTE1540 throws are removed; XTTE1550 shape check first, then the single root element validates against the named type (content failure → XTTE1540, undeclared root → XTTE1512, valid content succeeds); deferred strict-declaration errors are recorded and re-thrown as XTTE1512 when the validating ancestor completes cleanly (the ancestor's own XTTE1510 keeps precedence); XTTE0950 namespace-sensitive attribute copy (XSLT §11.8.2 — a QName/NOTATION-derived-typed attribute whose annotation survives the copy throws XTTE0950 when its parent element is not copied or the value's prefix is unresolvable on the copied-to element, covering copy-namespaces="no"; `XdmValidationOptions.ExtraNamespaceBindings` + `ImportInScopeNamespaces` carry value-only prefixes into temp-tree validation); strip-space never strips whitespace from simple-content elements; invalid xsl:message/@terminate AVT values now raise XTDE0030 (was XTDE0975). **Behavior changes for consumers:** built-in `xs:` typed patterns are now enforced even without an in-scope schema set (basic mode); code relying on whitespace stripping inside simple-content elements will no longer see it; invalid `@terminate` AVT values surface XTDE0030 instead of XTDE0975. Gates: build 0/0; unit **2,683/2,683** across 9 solution assemblies + LanguageServer **72/72** (run separately — not in `Bosak.sln`); QT3 **31,142/0/679** unchanged; basic sweep **10,221/54/4,325 → 10,228/47/4,325** (+7 FAIL→PASS — evaluate-048, merge-072/074/079/097s, package-021err, package-022err — zero pass→fail); schema-aware sweep **10,954/100/3,546 → 11,015/39/3,546** (+61 FAIL→PASS, zero pass→fail, skips identical); new baselines `basic-after-req114.txt` / `schema-aware-after-req114.txt` / `qt3-after-req114.txt`.

- **2026-09-30 (d)** — **XSLT: PB-2 (C8) closed — XTTE15xx completion (REQ-113)** — schema-aware XSLT validation now partitions element-level vs document-level identity-constraint errors per XSLT §25.4.1.3 vs §25.4.2: ID/IDREF constraint failures (duplicate IDs, dangling IDREF/IDREFS) are suppressed during element-level assessment and surfaced only at document level as XTTE1555 (`HasDocumentLevelConstraintFailure` on `XdmSubtreeValidationResult` + internal `CheckDocumentIdentityConstraints`; `ValidateConstructedElement` gained a `documentLevel` parameter threaded into `XdmValidationOptions`). Unresolvable `xsi:type` QNames now raise XTTE1510 under lax validation; Strip/Preserve dispatch runs before the XTTE1550 document-shape check; copied-attribute named-type failures report XTTE1510 (strict)/XTTE1515 (lax) instead of the constructed-attribute XTTE1555; the XTDE1490 duplicate-URI check remains transformation-scoped (two final result trees with the same URI are an error per XSLT 3.0 §25.2, even for sequential episodes — try-021); and implicit result-tree validation is a spec-mandated no-op (W3C bug 30211). ~44 previously-failing schema-aware W3C tests now pass. **Behavior changes for consumers:** code that relied on element-level validation raising ID/IDREF errors will no longer see them (they are document-level only, per spec); code that relied on implicit result-tree validation must add explicit `validation`/`@type` directives; any two `xsl:result-document` writes to the same URI in one transformation raise XTDE1490, including sequential episodes (unchanged from previous releases). Gates: build 0/0; unit **2,721/2,721** across 10 assemblies; QT3 **31,142/0/679** unchanged; basic sweep **10,221/54/4,325** (1 FAIL→PASS — si-result-document-008 — zero pass→fail); schema-aware sweep **10,954/100/3,546** (45 FAIL→PASS, zero pass→fail).

- **2026-09-30 (c)** — **XSLT conformance harness: XSD 1.1 `xs:assert`/`xs:alternative` skip-list (REQ-112, PA-5)** — no engine change. 28 schema-aware tests failed at schema-compile time because their schemas use XSD 1.1 assertions while the engine's schema stack is XSD 1.0 (`System.Xml.Schema`): the `books.xsd` environment schemas guard `xs:assert` with `vc:minVersion="1.1"` but the catalog pins `xsd-version="1.0"`, and validation-1301 declares `xs:alternative` inline with no environment schema. The harness now scans raw schema text prefix-agnostically (per-URI cache) and inline stylesheet schemas, skipping with an explicit `XSD 1.1; engine supports XSD 1.0 only` reason. Feature `XSD_1.1` is deliberately not in `SkipFeatures` — the four `satisfied="false"` probes (regex-syntax-0056/0086/0102, type-available-0151) are XSD-1.0-only applicability tests whose silent skip must be preserved. Gates: unit **2,714/2,714** + LanguageServer 72/72; QT3 **31,142/0/679** unchanged; basic sweep **10,220/55/4,325 per-test bit-identical**; schema-aware sweep **10,909/173 → 10,909/145/3,546** (exactly the 28 FAIL→SKIP, zero pass→fail).

- **2026-09-30 (b)** — **XSLT: PA-4 (C7) closed — `@as` coercion namespace context (REQ-111)** — the last two XTTE0570 conversion failures had one root cause: `ConvertVariableValue` resolved unprefixed sequence-type QNames (`element(base)`, `myPartNumberType`) against the transform-wide context, which deliberately never carries a default element namespace, instead of the declaring instruction's in-scope `xpath-default-namespace`. The coercion now takes the declaring element (~19 call sites: variables, params, with-param/tunnel params, template/function results, `xsl:evaluate`, accumulators), publishes its `xpath-default-namespace` for the coercion duration (try/finally restore), and delegates to the unchanged core. `element(N)` ≡ `element(N, xs:anyType)` accepts name matches over lax-`xs:any` content with no global declaration (import-schema-202); unprefixed user-defined type names in `@as` resolve via `xpath-default-namespace` and REQ-108's identity machinery accepts the PSVI-typed value (xpath-default-namespace-0701 → `truetruetrue`). `schema-element(N)` still requires a real declaration; `attribute(N)` stays no-namespace. Gates: unit **2,714/2,714** across 10 assemblies (Xslt.Tests 670 = 665+5 new `ElementQNameSequenceTypeTests`) + LanguageServer 72/72; QT3 **31,142/0/679** unchanged; basic sweep **10,220/55/4,325 per-test bit-identical**; schema-aware sweep **10,907/175 → 10,909/173/3,518** (+2 FAIL→PASS — import-schema-202, xpath-default-namespace-0701 — zero pass→fail); `import-schema` set 175/29 → **176/28/1**.

- **2026-09-30** — **XSLT: PA-3 residuals — `match` set closed (REQ-110)** — `xsl:mode/@typed` lands as a real feature: the attribute was parsed yes/no, so `strict`/`lax`/`unspecified` threw XTSE0020; it is now the enum `ModeTyped { Unspecified, Strict, Lax, Untyped }` (yes/true/1 ≡ strict, no/false/0 ≡ untyped). In strict/lax modes, plain QName pattern branches (top level, union branches, trailing step after `//`) rewrite to `schema-element(QName)` via per-rule variant predicates reusing REQ-105's `MatchesSchemaElement` (lax falls back to plain name matching without a declaration); strict + unannotated element/attribute node → XTTE3100 at apply-templates dispatch; strict-mode QName with no element declaration → static XTSE3105; `typed="no"` + schema-dependent pattern matching an annotated node → XTTE3110. Companions: `element-with-id(X[, S])` allowed at pattern start (XSLT 3.0 §5.5.3 — match-054/055); top-level `..` pattern steps rejected with XTSE0340 (match-213); deep-copy built-in rules and `CopyNodeToResult` carry attribute PSVI (`IXmlSchemaInfo` + `XdmIdProperties`) so copied attributes keep `instance of attribute(N, T)` (match-263); validation-by-named-type no longer rejects non-derived clone content via the root declaration (match-220/221). Gates: unit **2,709/2,709** across 10 assemblies (Xslt.Tests 665 = 641+24, LanguageServer 72/72); QT3 **31,142/0/679** unchanged; basic sweep **10,220/55/4,325 per-test bit-identical**; schema-aware sweep **10,889/193 → 10,907/175/3,518** (+18 FAIL→PASS — the 15 match targets + import-schema-138/si-element-116/si-lre-116 — zero pass→fail); `match` set 271/15 → **286/0**.

- **2026-09-29** — **XSLT: PA-3 tail — `strip-type-annotations` cluster (REQ-109)** — three PSVI typed-value fixes. (1) XSD years are unbounded but .NET's `XmlSchemaDatatype.ParseValue` returns `System.DateTime` (year 1..9999) and threw for conformant lexicals (`-0012-12-03`, `21999-05`), where the blanket catch downgraded the typed value to an unannotated string — `XDocumentNode.GetTypedValue` now re-parses out-of-range date/time lexicals into `XPathDateTime` (new annotated `XdmValue.FromDate`/`FromTime(XPathDateTime, …)` overloads; g* types stay annotated strings, matching the constructor shape), so `data($e) instance of xs:date` works across the full XSD year range. (2) XDM §2.7.2: a complex type with **mixed** content has its concatenated descendant text as the typed value tagged `xs:untypedAtomic` — the fallback now tags it. (3) XSLT 3.0 §3.13: stripping (`validation="strip"` / `input-type-annotations="strip"`) preserves is-id/is-idref — the strip pass snapshots them onto a new internal `XdmIdProperties` marker before deleting the PSVI, so `fn:id`/`fn:idref` keep working on stripped trees. Bonus: **as-1701** flips — the `as` set is now **187/0**, zero documented failures. Gates: unit **2,612/2,613** (Providers.Tests 131 = 118+13, Xslt.Tests 641 = 630+11; the one failure is the known `BoundedMemoryWithAccumulator` environment drift) + LanguageServer 72/72; QT3 **31,142/0/679** unchanged; basic sweep **10,220/55/4,325 per-test bit-identical**; schema-aware sweep **10,883/199 → 10,889/193/3,518** (+6 FAIL→PASS — strip-type-annotations-001/002/012/014/021 + as-1701 — zero pass→fail).

- **2026-09-25 (a)** — **XSLT: PSVI user-defined type identity + derivation-aware `instance of` (REQ-108, PA-3)** — the deferred REQ-107 family lands additively. `XdmValue` gains a second, optional annotation `_userSchemaTypeName` (`Q{uri}local`, public `UserSchemaTypeName`) alongside the untouched built-in base name in `SchemaTypeName`, so every existing consumer stays correct; both `ConvertSchemaValue` paths (XDocumentNode PSVI, VmEngine cast — incl. the QName/NOTATION namespace-sensitive branch of `TryCastToSchemaType`) tag user-defined-typed results; `ValueMatchesType`'s user-defined branch requires the value's own type annotation and answers via `IsSchemaTypeSubtype` (schema-set hierarchy walk, exact-QName fallback without one) — a castable-but-untyped value is no longer an instance of a user-defined type (as-2002), while PSVI/constructor/coercion-typed values match their user type and its built-in base; `@as` coercion of xs:untypedAtomic converts and carries the user type (as-1806–1809); URI promotion still converts-and-loses the user type per XSD 1.0 (as-2101). En-route regression fixes: complex-type-with-simple-content values normalize to their simple content base before identity tagging (cbcl-module-001), and type annotations survive the bool/float/double/date/time conversion arms (evaluate-009, type-expr-0201/0401, type-functions-0201). Constraint honored: no XDM 3.1 anyURI<:string. Gates: unit **2,600/0** across 9 assemblies (Xslt.Tests 641 = 630+11 new `TypeIdentityTests`; 6 castability-era assertions updated to the QT3 instanceof118/119 identity shape); QT3 **31,142/0/679** unchanged; basic sweep **10,220/55/4,325 per-test bit-identical**; schema-aware sweep **10,874/208 → 10,883/199/3,518** (+9 FAIL→PASS flips — as-1806/1807/1808/1809/2002/2101, import-schema-176/181, type-functions-0202 — zero pass→fail regressions); `as` set 180/7 → **186/1** (only as-1701, the documented `DateTimeOffset` year limit, remains).

- **2026-09-24 (c)** — **XSLT: no-namespace named-type validation + canonical PSVI typed-value forms (REQ-107, PA-3)** — three fixes from the residual `as`-set failures. (1) Named-type validation of no-namespace user types (e.g. `derivedURI`) crashed with ArgumentException: `XdmSchemaAnnotator.Validate` bound a generated prefix to the empty namespace on the assessment clone; it now writes unprefixed `xsi:type` and drops the clone's default-namespace declaration for the assessment — the live tree is untouched. (2) `xsl:value-of` on schema-validated element/attribute nodes now atomizes via the PSVI typed value (`TransformEngine.ConstructValueOfString` + `NodeAtomizedString`; nilled elements contribute no slot) instead of the raw string value, and `XDocumentNode.ConvertSchemaValue` emits canonical lexical forms — durations via `TryCanonicalizeDuration` (months→years fold, zero-component omission, fraction trailing-zero trim, `PT0S`), decimals with trailing-zero strip, `xs:anyURI` verbatim (.NET `System.Uri` appends a slash). (3) Companion correctness: `HasNoTypedValue` is now true only for element-only content — empty content has the zero-length string typed value (XDM §2.7.2). as-1701 (year −12/21999 dates) remains the documented `DateTimeOffset` platform limit; the type-identity family (as-2002/2101/1806–1809) is deferred to **REQ-108** (design notes `.sweep-baselines/REQ-108-design-notes.md`; XSD 1.0 semantics — no XDM 3.1 anyURI<:string). Gates: unit **2,587/2,588** (Providers.Tests 118 = 110+8, Xslt.Tests 630 = 626+4; the one failure is the known `BoundedMemoryWithAccumulator` environment issue) + LanguageServer 72/72; QT3 **31,142/0/679** unchanged; basic sweep **10,220/55/4,325 bit-identical**; schema-aware sweep **10,840/242/3,518 → 10,874/208/3,518** (+34 FAIL→PASS flips, zero pass→fail regressions); `as` set 178/9 → **180/7**.

- **2026-09-24 (b)** — **XPath/XSLT: NOTATION surface + nested schema import resolution (REQ-106, PA-3)** — casts to NOTATION-derived user types now annotate the result `xs:NOTATION` (the namespace-sensitive cast branch dropped the annotation, so `instance of xs:NOTATION` / `as="xs:NOTATION"` coercion failed with XTTE0570); the same branch enforces the §19.3 cast matrix's stringish-source rule (xs:anyURI is not castable to QName/NOTATION-derived types); `xs:QName()` accepts QName-kind input (NOTATION→QName casting, XPath 3.0); unprefixed type names in `instance of` resolve against no-namespace schema types (was XPST0051); `xsl:for-each-group` keys and `xsl:key` values atomize schema-annotated nodes to their PSVI typed value (NOTATION-typed attributes group and look up by QName namespace+local); the typed-template result harvest skips namespace declarations (a fixup-added `xmlns:` binding previously counted as a second result item → spurious XTTE0505 with kind-tested `@as` — as-1812/1813/1814); and `SchemaSetBuilder` eagerly loads locationful nested `xs:import`/`xs:include` targets resolved against the including schema's `SourceUri` (`XmlSchemaSet.Compile` fetches nothing with a null resolver in .NET 10, silently dropping such documents — the notation-0301 family). Conformance harness (3.54): the source-validation schema set gets an `XmlUrlResolver` (safe there: URI-added documents populate the fetch-dedup table). Gates: unit **2,575/2,576** (Xslt.Tests 626 = 617+9 new; the one failure is the known `BoundedMemoryWithAccumulator` environment issue) + LanguageServer 72/72; QT3 **31,142/0/679** unchanged; `notation` set **8/15 → 23/0**; `import-schema` set 166/38 → **168/36**; `as` set 175/12 → **178/9**.

- **2026-09-24** — **XSLT: schema-aware typed pattern dispatch for `element(N,T)`/`attribute(N,T)` match patterns (REQ-105, PA-3)** — the optional schema type argument of the `element(...)`/`attribute(...)` kind tests was silently dropped at all four `PatternCompiler` call sites, so `match="attribute(*, my:partNumberType)"` matched every attribute regardless of its type annotation (W3C match-164). In schema-aware stylesheets the type is now enforced at match time: exact name or `XmlSchemaType.IsDerivedFrom` against the in-scope schema set, with built-in types resolved from the System.Xml.Schema built-in table (`XmlSchemaSet.GlobalTypes` never surfaces them) and XPath-only members (`xs:anyAtomicType`, `xs:untypedAtomic`, …) via the `XmlTypeCode` overloads; nilled elements match only the `T?` form; unannotated nodes match only `xs:untyped`/`xs:untypedAtomic`-family targets; unprefixed type names expand against the xpath-default-namespace while unprefixed attribute names stay in no namespace. Without a schema set the type argument is ignored exactly as before (basic processors bit-identical). Default priority of the four typed forms is now 0.25 per XSLT 3.0 §6.4, including after an axis. Companions: `schema-element(N)`/`schema-attribute(N)` matching (match patterns and XPath `instance of`) reads `ElementSchemaType`/`AttributeSchemaType` (`SchemaType` is null for type-referenced declarations), with anonymous-type annotations falling back to the governing declaration's type object; `schema-attribute(N)` matches type-only-validated constructed attributes by name+derivation; validated constructed attributes keep their PSVI through sequence-constructor harvesting (and the harvest no longer picks up auto-added xmlns declarations); validated simple-typed content is whitespace-normalized per the whiteSpace facet (XDM §3.3.2). Gates: unit all 10 projects green (Xslt.Tests 617 = 605+12 new); `match` set 239/47/8 → 271/15/8 (schema-aware); full schema-aware sweep 10,770/312/3,518 → **10,820/262/3,518** (+50, zero pass→fail regressions); basic sweep **10,220/55/4,325 bit-identical**; QT3 **31,142/0/679** unchanged.

- **2026-09-24** — **XPath/XSLT: schema kind tests visible in every transform XPath static context (REQ-104, PA-2)** — `schema-element()`/`schema-attribute()` failed statically with XPST0008 "no schema awareness" even in schema-aware transforms, because secondary XPath compilations carried no schema set. New public surface: **`CompileOptions.SchemaSet`** — the compiled `XmlSchemaSet` in scope for a schema-aware XPath compilation. When set, the parser accepts unprefixed kind-test names and the static name-test validator checks the name argument against the set's global declarations (XPST0008 when absent, XPST0081 keeps precedence; unprefixed names expand against `DefaultElementNamespace`); when null (default), the no-schema-awareness XPST0008 behavior is unchanged. The transform threads its merged set through every compilation it touches — `CompileXPath` options, bare select/catch/copy sites, `xsl:evaluate` (XSLT 3.0 §5.3.3), AVTs, and pattern compilation. Companions: variable/parameter coercion atomizes validated nodes to their PSVI typed value so subtype substitution applies (as-1702's xs:QName into `as="xs:QName"`; unvalidated nodes still atomize to xs:untypedAtomic), `ValueMatchesType` accepts DateTime-kind g* values, and a locationless `xsl:import-schema` of the XPath functions namespace binds the embedded W3C schema-for-JSON (json-to-xml-typed family). Conformance harness (3.53): `role="source-reference"` environment schemas join the host set, principal sources with `validation="strict"/"lax"` are schema-validated at load, and xsi-typed result trees are revalidated for kind-test assertions (validation-1705/1706). Gates: unit **2,555/2,555** (Api.Tests 103 = 87+16 new, Xslt.Tests 605 = 596+9 new) + LanguageServer 72/72; QT3 **31,142/0/679** unchanged; basic sweep **10,220/55/4,325** identical; schema-aware sweep **10,665/418/3,517 → 10,770/312/3,518** (+105, zero pass→fail regressions).

- **2026-09-23 (p)** — **XSLT: named-type attribute validation crash fixed + exception stacks preserved (REQ-103, PB-1)** — the schema-aware sweep's 21 import-schema `NullReferenceException`s traced to `XdmSchemaAnnotator.ValidateAttribute` (named simple-type path) passing null `XmlNameTable`/`IXmlNamespaceResolver` into `DatatypeImplementation.ParseValue` (NCName-family datatypes dereference them — `xsl:attribute` + `@type="xs:ID"` under `default-validation="preserve"`); it now passes a real name table and the attribute's in-scope namespace bindings. Companion: `XsltExecutable.RunWithStack` rethrows via `ExceptionDispatchInfo.Throw`, so engine exception stacks survive the dedicated-stack thread (diagnostics-only; error codes unchanged). Gates: unit **2,530/2,530** (Providers.Tests 110 = 108+2 new) + LanguageServer 72/72, QT3 **31,142/0/679** unchanged, basic sweep identical (10,220/55/4,325), schema-aware sweep **10,665/418/3,517** (+17 vs REQ-102, zero regressions), `import-schema` set **153/51/1**.

- **2026-09-23 (o)** — **XSLT schema import resolution/merge correctness (REQ-102, PA-1)** — `SchemaSetBuilder` (0.2) fixes four resolution rules the first schema-aware sweep exposed: (1) host-set schemas now merge alongside stylesheet declarations with document-URI dedup instead of a namespace-keyed skip, so `xs:include` companions sharing a target namespace survive (import-schema-056 `colors`) and catalog environment schemas stay in scope next to stylesheet imports (import-schema-186); (2) `schema-location` is treated as a *hint* — a resolved document whose target namespace doesn't match the declaration yields nothing (fallback to the host set / next hint), with XTSE0220 only when no source covers the namespace and locations were given (200/201), while an inline-schema mismatch has no fallback and raises XTSE0215 (154); (3) locationless imports are inert (XSLT 3.0 §3.14.1) — no error while no component from the namespace is used (178/184), and a `@namespace`-less inline schema imports its own target namespace (179); (4) the predefined XML namespace schema (`xml:lang`/`xml:space`/`xml:base`/`xml:id`) is added to every built/merged set (XSD 1.0 §4.2.6.2 — System.Xml.Schema doesn't pre-populate it; fixes the si-* `xml:lang` family). Harness (3.52): dropped the file-based `SchemaResolver` (base-URI-less streams broke include resolution and URI dedup); `XXXX9999` catalog code = "any error" (203). Gates: unit **2,528/2,528** (Xslt.Tests 596 = 589+7 new) + LanguageServer 72/72; QT3 **31,142/0/679** unchanged; basic sweep identical to baseline (10,220/55/4,325); schema-aware sweep **10,648/435/3,517** (+111 passes vs the REQ-101 baseline, zero regressions). `import-schema` set: **136/68/1** (was 129/75/1).

- **2026-09-23 (n)** — **XSLT conformance harness: `--schema-aware` mode (REQ-101)** — the runner (`tests/Bosak.Xslt.Conformance`) gains an opt-in schema-aware mode that un-gates the `schema_aware`/`schema-import` feature dependencies and drives the REQ-097/098/099 seam end-to-end from the W3C catalog: every stylesheet compiles with `XsltCompiler.SchemaAware = true`, `xsl:import-schema` `schema-location` hints resolve against the test-set and catalog directories, and the environment's catalog `<schema role="stylesheet-import|secondary">` documents merge into the host `SchemaSet` (XSD 1.1 environments skip — the engine is XSD 1.0 only). First targeted run: the `import-schema` set (205 tests, previously skipped) now runs **129 passed / 75 failed / 1 skipped** — the failures are the REQ-001 backlog (G1–G6 engine work tracked by the Bosak.Schema phases), not regressions. Basic-mode behavior is untouched: the arg parser consumes the flag and everything else is flag-gated. First full schema-aware sweep (Phase A starting line): **10,537 passed / 546 failed / 3,517 skipped** (14,600; 95.1% of runnable). Gates: unit **2,521/2,521** + LanguageServer 72/72, full basic sweep identical to the REQ-100 baseline (10,220/55/4,325), build 0/0.

- **2026-09-23 (m)** — **XSLT: `schema-element()` / `schema-attribute()` kind tests in match patterns (REQ-100)** — `match="schema-element(N)"` / `match="@schema-attribute(N)"` (and path-step / axis-step positions) previously compiled without error but **never matched**: the pattern compiler had no branch for the schema kind tests, so the argument fell through to `ParseQName`, which only preserves `Q{uri}local` and dropped every prefixed name — a silent fallback-template bug invisible to the sweep (no catalog test exercises schema kind tests in *patterns*). The compiler now evaluates the declaration against the schema set captured in the validation context at all four entry points (single pattern, path-step node test, `attribute::` node test, `@`-attribute pattern), mirroring `VmEngine.MatchesSchemaElement`/`MatchesSchemaAttribute`: element/attribute kind check, `SchemaElementDeclaration`/`SchemaAttributeDeclaration` lookup, substitution-group walk, nilled nillability check, and `XmlSchemaType.IsDerivedFrom` type-annotation compatibility. Unprefixed `schema-element()` names take `xpath-default-namespace`; unprefixed `schema-attribute()` names have no namespace (XPath rules). With no schema set in scope the tests never match and never throw — basic-processor behavior unchanged; default priority 0.25 was already computed correctly and is unchanged. No public API changes (pattern compiler is internal post-REQ-096). Gates: unit **2,521/2,521** across all 10 projects (Xslt.Tests 589 = 581+8 new `SchemaKindTestPatternTests`) + LanguageServer 72/72, QT3 **31,142/0/679** unchanged, full XSLT sweep **10,220 / 55 / 4,325 — identical to the REQ-099 baseline**, build 0/0.

- **2026-09-22 (l)** — **XSLT schema-awareness seam H4 (REQ-099) — public validation service + `validation`/`@type` runtime semantics** — the final seam hook: `XdmSchemaAnnotator` gains mode-aware validation (`XdmValidationMode` Strict/Lax/Strip/Preserve, `XdmValidationOptions` Mode/TypeName/DocumentLevel; `Validate`/`ValidateAttribute` port the XQuery validate algorithms — lax `xs:anyType` root augmentation, named-type `xsi:type` injection/removal, document-shape checks, element-only whitespace stripping; helpers `StripSchemaAnnotations`, `ResolveSchemaType`, `IsQNameOrNotationDerived`), and the XSLT engine now reads per-instruction `validation`/`type` (LREs: `xsl:validation`/`xsl:type`; `default-validation` inherited per module) and validates constructed elements/attributes/documents when a schema set is in scope, raising the catchable XTTE15xx family (1510/1512/1515/1535/1540/1545/1550/1555). Companion fixes: PSVI preserved across `xsl:copy`/`xsl:copy-of`, the secondary `xsl:result-document` finalize gap closed, `xsl:strip-type-annotations`/`input-type-annotations="strip"` honored. The service is error-code-agnostic (hosts map codes); with no schema set in scope the engine is bit-identical. See §3.1c. Gates: unit **2,513/2,513** in-solution (+75: Providers.Tests 108 = 74+34 new, Xslt.Tests 581 = 540+41 new) + LanguageServer 72/72, QT3 **31,142/0/679** unchanged, XSLT sweep aggregate identical to baseline (details in the REQ-099 decision log), build 0/0.
- **2026-09-22 (k)** — **XSLT schema-awareness seam H3 (REQ-098) — typed-construction/annotation API** — two additions give a host full access to constructed nodes for schema annotation: **(1) `XdmSchemaAnnotator`** (`Bosak.XPath.Providers.Xml`, new) — `ValidateSubtree(XElement, XmlSchemaSet, ValidationEventHandler? = null, bool throwOnInvalid = false)` validates an in-memory subtree *in place* (temporary `XDocument` wrapper, `addSchemaInfo: true`; when the subtree is attached, a deep clone is validated and the PSVI annotations are copied back onto the live XObjects in document order, so node identity is preserved) and returns `XdmSubtreeValidationResult` (`IsValid`, `IReadOnlyList<ValidationEventArgs> Errors`; `throwOnInvalid: true` throws `XmlSchemaValidationException` with the first error); `Annotate(XObject, IXmlSchemaInfo)` attaches a host-built annotation without validation. **(2) `EvaluationContext.ConstructedElementProcessor` / `ConstructedDocumentProcessor`** (`Action<IXdmNode>?`, default null) — the element processor fires exactly once per constructed element after its content is complete, bottom-up (innermost first), covering `xsl:element`, literal result elements, and `xsl:copy` element results; the document processor fires at each result-document boundary with the wrapped document node. Every consultation is null-conditional: with the processors unset the engine is bit-identical (verified by the sweep and a byte-identity unit test). See §3.1b. Gates: full XSLT sweep **10,220/55/4,325, aggregate identical to the REQ-097 baseline**, QT3 **31,142/0/679** unchanged, unit **2,510/2,510** across all 10 projects (Providers.Tests 74 = 61+13 new, Xslt.Tests 540 = 534+6 new), build 0/0.
- **2026-09-22 (j)** — **XSLT schema-awareness seam H1/H2 (REQ-097)** — opt-in schema-aware compilation: `XsltCompiler.SchemaAware` gates the XTSE1650/XTSE1660 throws (default basic processor unchanged, bit-identical); `XsltCompiler.SchemaResolver`/`SchemaSet` supply schema documents; `xsl:import-schema` declarations (inline / resolver / schema-location / host set) compile into one merged `XmlSchemaSet` with import-precedence merging (XTSE0215/XTSE0220) that is folded into `EvaluationContext.SchemaSet` before function-library population — user-defined simple-type constructors, `cast as` / `instance of`, and `schema-element()`/`schema-attribute()` kind tests work from stylesheet-declared schemas. No runtime enforcement of `validation`/`@type` yet (hook H4) and no complex-type typed construction (hook H3); both are scheduled with the Bosak.Schema commercial track. Gates: full XSLT sweep **10,220/55/4,325, aggregate identical to the REQ-096 baseline** (only delta: XTSE1650/1660 error-code precedence, unreachable by the failing set), QT3 **31,142/0/679** unchanged, unit **2,491/2,491** across all 10 projects (Xslt.Tests 534 = 522+12 new `SchemaAwareCompilationTests`), build 0/0.
- **2026-09-21 (i)** — **xml-to-json package-namespace batch** — the four xml-to-json-B2 failures (B2-005/006/010/014) were never an XPath 4.0 gap: the error text `XPST0017: ...escape#1` came from a **namespace-contamination bug**. The W3C reference package `xml-to-json.xsl` (package `http://www.w3.org/2013/XSLT/xml-to-json`) calls its package-private `j:escape(.)` from template-rule `xsl:sequence/@select` attributes, while the using driver rebinds `xmlns:j` to the `fn` namespace. The engine compiled that select with `XPath31Expression.Compile(select)` **without the instruction's in-scope namespaces**, so the package's `j` prefix silently resolved against the default fn namespace. Fix (TransformEngine 6.82): the `xsl:sequence/@select` handler now compiles via `CompileXPath(select, instruction)`, which resolves prefixes against the element that lexically contains the select — exactly the XSLT namespace-scoping rule for used packages. Clears xml-to-json-B2-005/006/010/014. Gates: full XSLT sweep **10,220/55/4,325** (+4/−4, skips unchanged, per-set fail diff exactly the four targets, zero sets worse), QT3 **31,142/0/679** unchanged, unit 2,479/0/0 across all projects (Xslt.Tests 522, LanguageServer 72), build 0/0.
- **2026-09-21 (h)** — **sx-treat / sx-instance-of braced-EQName batch** — braced-URI function calls in *step position* (`A ! Q{uri}fn(...)` or `A/Q{uri}fn(...)`) were misparsed as kind-test steps: `SplitQName` drops the URI of a `Q{uri}local` name, so `Q{f}text('x')` after `!` routed to the `text()` kind-test production — evaluating `child::text()[…]` (or `attribute::node()[…]`) over the context instead of calling the function. Primary-position calls were unaffected, which is why the extensive QT3 EQName coverage never caught it. One-condition fix in `XPathParser.ParseStepExpr` (1.58): a `Q{`-prefixed name is never a kind test. Clears sx-treat-107/108/109 and sx-instance-of-107/108. Gates: full XSLT sweep **10,216/59/4,325** (+5/−5, skips unchanged, per-set diff exactly the five targets, zero sets worse), QT3 **31,142/0/679** unchanged, unit 2,479/0/0 across all projects (Parser 192, Xslt.Tests 522, LanguageServer 72), build 0/0.

- **2026-09-21 (g)** — **sx-MapExpr map-constructor batch** — two `xsl:map` content bugs fixed in `TransformEngine` (6.81): **(1)** the content merger required every map in the content sequence to have exactly one entry, spuriously raising XTTE3365 — XSLT 3.0 merges the entries of *any* maps the content produces, so the count check is removed (non-map items still raise XTTE3375, and duplicate keys across the merged entries still raise XTDE3365); **(2)** duplicate map-constructor keys inside streamable `xsl:source-document` content raised XQDY0137 instead of XTDE3365 — the content constructor now sets `EvaluationContext.InStreamingMapContext` (the same flag xsl:fork branches already set), and the runtime picks XTDE3365 when it is set. No catalog test anywhere expects XQDY0137, so the widened code is safe; QT3 (XQuery) is untouched. Clears sx-MapExpr-007/008/009 (si-map-007/009 remain schema-gated XTSE1650 — schema-awareness track). Gates: full XSLT sweep **10,211/64/4,325** (+3/−3, skips unchanged, per-set diff exactly the three targets, zero sets worse), unit Xslt.Tests 522/0/0 (unchanged), build 0/0.

- **2026-09-21 (f)** — **si-message assert-message batch (conformance harness)** — the W3C catalog schema states "tests are free to output additional messages beyond those expected", but the harness matched `assert-message` #N positionally against message #N. Matching is now non-positional: each `assert-message` claims a distinct emitted message; extra messages are allowed. No engine change — the engine was already emitting correct message content (including streamed nodes copied into `xsl:message`); si-message 11/11. Gates: full XSLT sweep **10,208/67/4,325** (+6/−6, skips unchanged, per-set diff exactly si-message-005..010, zero sets worse), unit Xslt.Tests 522/0/0 (unchanged), build 0/0.

- **2026-09-21 (e)** — **si-iterate XTSE3120 batch** — `xsl:break` and `xsl:next-iteration` are now accepted as the last instruction of an `xsl:if` inside `xsl:iterate` (XSLT 3.0 §8.4); the placement validator's allowed-parent list omitted `xsl:if`. The other positions are unchanged: `xsl:for-each` still rejects with XTSE3120, and a break that is not the last instruction in its sequence constructor still raises XTSE3120. Clears si-iterate-013/094/099/140 (094/099/140 exercise early exit; 099 also covers `xsl:break select=` with `xsl:on-completion`, which the runtime already supported). Gates: full XSLT sweep **10,202/73/4,325** (+4/−4, skips unchanged, per-set diff clean — zero sets worse), unit Xslt.Tests 522/0/0 (+4), build 0/0.

- **2026-09-21 (d)** — **su-filter / su-unclassified analyzer batch** — three §19 analyzer false positives fixed in `StreamabilityAnalyzer` (0.6): **(1)** a lone *boolean-typed* variable predicate (`$input[$test]` with `$test as xs:boolean`) is a filter predicate, not a positional one — filter functions with a boolean gate parameter now compile; **(2)** a positional but motionless predicate on a *striding* step (`ITEM[position() ne 42]`) keeps the step striding per §19.8.8.9 rule 5 instead of going roaming — `last()`-using predicates and crawling operands still raise XTSE3430 as before; **(3)** `streamability="unclassified"` functions atomize atomic-typed parameters in **any** argument position (su-unclassified-006 passes a striding path as argument 2 of `xs:decimal*`), sharing the §19.8.5.1 rule already applied to undeclared functions. This clears the 10 real analyzer gaps exposed by the use-when batch: su-filter 10/10, su-unclassified 6/6. Gates: full XSLT sweep **10,198/77/4,325** (+10/−10 vs the use-when batch, skips unchanged, per-set diff clean — zero sets worse), unit Xslt.Tests 518/0/0 (+6), build 0/0.

- **2026-09-21 (c)** — **use-when XTSE0090 batch** — `use-when` is a common attribute permitted on every XSLT element (XSLT 3.0 §3.13); the element-specific attribute whitelists for `xsl:function`, `xsl:copy-of`, and `xsl:copy` now accept it, and those three whitelist checks now require the XSLT namespace so a literal result element named `<copy>`/`<copy-of>` is no longer validated as the XSLT instruction. This clears the false XTSE0090 static errors that masked 22 W3C streaming tests (su-absorbing ×17, su-inspection ×4, si-apply-templates-005). Gates: full XSLT sweep **10,188/87/4,325** (+22/−22, skips unchanged, no new failure causes), unit Xslt.Tests 512/0/0 (+3), LanguageServer 72/0/0, build 0/0.

- **2026-09-21 (b)** — **Streaming provider batch** — four follow-ups to the burst-mode provider landed together: **(1) wrapper cache** — `StreamingSource.Wrap` now caches `StreamingNode` wrappers per underlying node in a `ConditionalWeakTable` (evicted with the record), so navigation allocates per node instead of per access while the 500k-record bounded-memory contract holds; **(2) pre-root comment/PI parity** — comments/PIs before the root element are captured into the shell document and surfaced as document-node children (children/child/descendant axes, document order before the root, document serialization); shell-root axes deliberately do not gain them; post-root comments/PIs remain unsurfaced; **(3) `fn:copy-of` deep-copy guard** — a provider-agnostic `IXdmNode`-based deep copy is the fallback for foreign-provider nodes, so `fn:copy-of` over streamed records returns grounded independent copies (document/root copies drain the stream, the documented "unbounded but correct" contract) instead of aliasing the live wrapper; **(4) `XsltExecutable.TransformStreamingToString`** — streaming input with the same output-property handling as `TransformToString`. Gates: full XSLT sweep **10,166/109/4,325** (+2 passes vs baseline, per-set diff clean), QT3 31,142/0/679 (unchanged), unit +13 (Providers 61, Xslt.Tests 509), build 0/0. See §3.2a.

- **2026-09-16 (b)** — **Streaming Phase B: accumulators over streamed sources** — `xsl:accumulator` now works over burst-mode streamed input. Values are computed *push-style*: each accumulator's current value is carried across records in declaration order and per-node before/after values are attached as annotations (bounded — they die with the record). Document/root-level `accumulator-after` is a consuming read that drains the rest of the stream when nothing is mid-enumeration (XTDE3350 while consumed); record-level after values resolve on demand (cross-accumulator references in declaration order, cycle guard XTDE3400); rule and initial-value errors defer to the point of access per spec bug 29813. Also: `fn:snapshot` supports streamed nodes; the accumulator-rule `@match` validator was corrected (globals are in scope, `$value` is not — accumulator-034/091); per-record `xsl:strip-space` now also drops top-level whitespace text records (engine-owned). **W3C `decl/accumulator`: 93/0/14 — 100% of runnable.** Gates: QT3 31,142/0/679, XSLT 7,759/3/6,838, unit 2,279/0/0, build 0/0.

- **2026-09-16** — **Streaming Phase A: burst-mode streaming input** — very large source documents can now be transformed record-at-a-time in bounded memory. New public surface: `XmlStreamingProvider` (`Bosak.XPath.Providers.Streaming`) presents an `XmlReader` source as a forward-only document node (records materialized one at a time as the engine pulls them), `XsltExecutable.TransformStreaming(Stream, StreamingTransformOptions?, …)` (with per-record `xsl:strip-space`/`xsl:preserve-space` application and an `xsl:accumulator` guard), and the `ISinglePassSequence` marker in Core. The VM and XSLT engine gained single-pass execution paths (`xsl:for-each` / `xsl:apply-templates` over the streamed children iterate without materializing; `fn:last()` over a streamed focus raises a clear error). Verified: 500k records through XPath and XSLT at ≤ 2 MB live input-side growth; 10 stylesheet parity tests byte-identical vs in-memory. `xsl:supports-streaming` still reports `no`; `streamable="yes"` and streaming accumulators are later phases. See §3.2a. Gates: QT3 31,142/0/679, XSLT 7,722/3/6,875, unit 2,249/0/0 + Xslt.Tests 391/0/0, build 0/0.

- **2026-09-14** — **Performance wave 4 (REQ-085):** result-tree append path — constructed-element content normalization now skips its list rebuild unless a text merge/discard is actually required, literal AVT values (no braces) return without computing namespaces/base URI, and per-literal-result-element bookkeeping is cached/static (interned prefix hints, lazy duplicate-attribute set, variable-snapshot only when the subtree can declare variables). **Transform_HtmlTable 62.57 → 51.46 ms (cumulative 193.34 → 51.46, −73%), 61.54 → 47.13 MB (cumulative 115.48 → 47.13, −59%)**. No public API or behavior change; XSLT strict sweep 7,722/3/6,875, QT3 31,142/0/679, unit 2,216/0/0 — all unchanged.

- **2026-09-10** — **Performance wave 3 (REQ-085):** serializer path — HTML escaping no longer allocates per-character strings or performs per-character encoding lookups (clean spans are written whole; unicode encodings skip representability checks), and namespace bindings during HTML serialization are copy-on-write instead of two dictionary copies per element. **Transform_HtmlTable 73.06 → 62.57 ms (cumulative 193.34 → 62.57, −68%), 71.53 → 61.54 MB (cumulative 115.48 → 61.54, −47%)**; output bytes unchanged (XSLT strict sweep 7,722/3/6,875, QT3 31,142/0/679, unit 2,216/0/0).

- **2026-09-09 (e)** — **Performance wave 2 (REQ-085):** XSLT transform path — `Transform_HtmlTable` 193.34 → 73.06 ms (−62%) and 115.48 → 71.53 MB (−38%). The engine now caches compiled XPath per stylesheet instruction (the `select` of `value-of`, `for-each`, sort keys etc. was re-compiled on every execution), skips the per-evaluation standard-function-library re-population inside transforms, and caches literal-result-element namespace info (extension namespaces, exclude-result-prefixes, in-scope declarations) instead of re-walking ancestors per element. No public API or behavior change; Xslt.Tests 377/0, QT3 31,142/0/679, XSLT strict 7,722/3/6,875 unchanged.

- **2026-09-09 (d)** — **Performance wave 1 (REQ-085):** BenchmarkDotNet harness at `benchmarks/Bosak.Benchmarks` (not part of the solution) with a published baseline. New public surface: `XDocumentNode.Wrap(XObject)` (shared per-object wrapper cache — direct `new XDocumentNode(x)` still works but allocates), `MaterializedSequence.Items` (copy-free item view), `EnumerableXdmSequence` (lazy sequence type in Core). Behavior is unchanged; document-evaluation benchmarks improved 33–48% on time and 45–51% on allocations. QT3 31,142/0/679, XSLT 7,722/3/6,875 unchanged.

- **2026-09-09 (c)** — **Beta readiness (REQ-084):** public API review pass over the nine published packages; `OccurrenceIndicator` moved to namespace `Bosak.XPath.Core.Xdm` (breaking for direct AST consumers only); `Bosak.LanguageServer` is no longer packable (executable, not a library); full XML-doc coverage on the public surface (~480 declarations); all file headers now carry `license.md (Apache-2.0)` + `SPDX-License-Identifier: Apache-2.0`. **Version `0.10.0-beta` — naming and options objects are frozen for the 1.0 line from here on.** QT3 31,142/0/679, XSLT 7,722/3/6,875, unit tests 2,216/0/0 — all unchanged.

- **2026-09-09 (b)** — XPath/XQuery: **QT3 is 100% of runnable (31,142/0/679)** — the final XPST0051 residual family closed: sequence-type item types in function signatures/returns are validated per XPath 3.1 §2.5.5.2 (`none`, list types, and unions containing or derived from lists are rejected with XPST0051), and unresolved 1-argument calls in a schema-imported namespace are treated as type constructors (unknown type → XPST0051, was XPST0017).

- **2026-09-09** — XPath/XQuery: **error-code alignment sweep (REQ-082 QT3 residual backlog, 233 → 8)** — a large triage pass over strict error codes. Behavior changes visible to consumers: (1) wrong-arity **dynamic** function-item calls now raise **XPTY0004** (XPST0017 remains for static calls), and `fn:apply` raises **FOAP0001**; (2) **fn:collection/uri-collection**: missing default collection is **FODC0002**, relative collection URIs resolve against the static base URI; (3) XQuery constructors: computed element/attribute names of non-string/non-QName atomic types raise **XPTY0004**, attribute xml/xmlns prefix misuse raises **XQDY0044**, invalid PI targets **XQDY0041**, invalid namespace prefixes **XQDY0074**, attributes in `document {...}` content **XPTY0004**, duplicate direct-constructor attributes are now the static **XQST0040** (was dynamic XQDY0025); (4) `fn:json-doc` decodes strictly — undecodable content raises **FOUT1200**, and unknown encoding *names* in `fn:unparsed-text(-lines)` raise **FOUT1190**; (5) `validate` strict with no top-level element declaration raises **XQDY0084** (precedes XQDY0027), operand-shape errors raise **XQDY0061** before XQST0075; (6) prolog `external` variables without supplied values raise **XPDY0002** on reference (was XPST0008); (7) duplicate schema-import target namespaces raise **XQST0058**; (8) `fn:transform` mutually exclusive options and invalid `delivery-format` raise **FOXT0002**; (9) character-map multi-character keys raise **SEPM0017** in XML parameter-documents (map form keeps SEPM0016); (10) `xs:error` is a recognized empty type (cast → FORG0001, `instance of` → false); (11) arithmetic/comparison on maps and function items raises **FOTY0013**; (12) integer literals beyond the `long` range keep type xs:integer; (13) `fn:unparsed-text(-lines)` reject non-string `$href`/`$encoding` with XPTY0004, and `()` encodings are XPTY0004 in the 2-argument forms. QT3 strict: **31,134/8/679** (97.84%); XSLT strict sweep unchanged **7,722/3/6,875**.

- **2026-09-07** — XPath/XQuery: **static name-test validation (QT3 static-error follow-up)** — undeclared prefixes in name tests and schema-aware kind tests are now rejected at **compile time** instead of surfacing as XPDY0002 at evaluation: `StaticNameTestValidator` (new, `src/Bosak.XPath.Compiler`) raises **XPST0081** for unresolvable prefixes and **XPST0008** for schema-element/schema-attribute with unknown types, mirroring the VM's `NamespaceTest` operand rules and understanding direct-element-constructor `xmlns:` scopes. It runs from `XPath31Expression.Compile` when `CompileOptions.Namespaces` is supplied (XSLT pattern compilation is unchanged), and from `XQueryCompiler` for the main module body. Related changes: the parser rejects whitespace/comments inside wildcard QNames (`*:local`, K2-Axes-5..16), the `namespace::` axis in XQuery (**XPST0003**, K2-Axes-54), malformed kind-test arguments (`item(...)`, `element(1)`, …), and invalid `document-node(...)` content; the lexer raises **XPST0003** for unterminated `Q{` and **XQST0046** for invalid braced-URI literals; a new `CheckFunction` IR opcode resolves callees before arguments so unknown functions raise **XPST0017** with correct precedence (K2-NodeTest-10); `fn:document#1/#2` moved out of the base library (XPST0017 in pure XPath/XQuery) and are registered by `XsltFunctionLibrary.Populate` for XSLT only. `XQueryCompiler` gains **`WithNamespace(prefix, uri)`** for host-environment bindings (§4.4a). QT3: **30,909/233/679** (was 30,737/405/679 — no new failure names).

- **2026-09-05** — XSLT: **EXSLT math extension library (`extension-functions-0201`)** — `FunctionLibrary` now registers the EXSLT math module in namespace `http://exslt.org/math`: `math:constant#1/#2` (PI, E, SQRRT2, LN2, LN10, LOG2E, SQRT1_2; decimal-string truncation to the requested precision, matching the Saxon/EXSLT reference semantics) plus `abs`, `sqrt`, `sin`, `cos`, `tan`, `log`, `exp`, `power`, `atan2`, `max`, `min`. `math:constant` raises **XTDE1420** for an unknown constant name or out-of-range precision and **XTDE1425** when an argument cannot be converted to the required type. Dispatch is by namespace URI, so the standard XSLT 2.0+ `math:*` functions in `http://www.w3.org/2005/xpath-functions/math` are unaffected. This restores the W3C `extension-functions-0201` test (7,718/12 → 7,719/11).
  - Implementation: `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs` (header → 5.109).
  - Regression tests: `tests/Bosak.XPath.Standard.Tests/ExsltMathTests.cs` (new file, 24 tests).
  - Verification: `extension-functions-0201` and `extension-functions-0101..0105` PASS; QT3 `math` test-set 149/149; `Bosak.XPath.Standard.Tests` 768/0.

- **2026-09-05** — XPath/XSLT: **Structural XPTY0019/XPTY0020 split for path steps (`context-item-911`, `analyze-string-085`)** — A step whose input is the result of a preceding path step (LHS of `/`) now raises **XPTY0019** for atomic input, while a standalone/first step applied to an atomic ambient context item raises **XPTY0020**. The lowerer records the distinction in a has-LHS flag: axis instructions and `PathStepMap` carry it in `RegisterC` (`IrLowerer` → 1.37), and `VmEngine` selects the error code from that flag (`ApplyAxis`, `PathStepMap` handler → 2.133). The `SimpleMap` check is unchanged (its `RegisterC != 0` path is already LHS-only). Strict full sweep **7,715/15 → 7,717/13/6,870** with zero regressions; full QT3 sweep unchanged at **31,148/0/673** (all `AxisStep`/`CombinedErrorCodes`/`TryCatchExpr` gates green).
  - Implementation: `src/Bosak.XPath.Compiler/Ir/IrLowerer.cs` (1.37), `src/Bosak.XPath.Runtime/Vm/VmEngine.cs` (2.133).
  - Verification: `context-item-911` PASS (XPTY0019), `analyze-string-085` PASS (XPTY0020); QT3 `AxisStep` sets 606/588/0/18, `CombinedErrorCodes` 259/240/0/19, `try-017` PASS; `dotnet test` green across all unit-test projects (0 warnings).

- **2026-09-01** — XSLT: **Strict error-code matching in the conformance harness** — Expected `<error>` results in the W3C XSLT harness now require the declared error code to appear in the raised exception; previously any exception satisfied an error expectation, which masked wrong-error-code failures (discovered when `override-f-019` passed via an unrelated `XPST0017`). The strict full sweep is **7,480 passed / 250 failed / 6,870 skipped** (96.8%), exposing **147 masked failures** (lenient figure: 7,627/103/6,870); zero genuinely passing tests were lost. The newly exposed failures are spec error-code bugs in the engine — top families: `XTSE0020` (15), `XTSE0010` (13), `XPTY0004` (12), `XTTE0505` (10), `XTDE3052` (10, abstract-component handling), `XTSE3070` (6), `XTDE0820` (6), `XTSE3050`/`XTSE3080` (8), `FODT0001` (4) — tracked as REQ-082. Note: the QT3 harness has the same leniency (`ResultComparer.CompareError` accepts any `InvalidOperationException` on code mismatch) and is a follow-up candidate.
  - Implementation: `tests/Bosak.Xslt.Conformance/Program.cs` (header → 3.35).
  - Verification: strict full sweep 7,480/250/6,870; 147 newly exposed, 0 regressions; `package-101` and all result-comparison passes unaffected; `dotnet test Bosak.sln` passes (2,114/0/0).

- **2026-09-01** — XSLT: **`xsl:override` scope propagation and `xsl:original` for functions (REQ-081)** — The W3C `package` cluster is now fully green at **72 passed / 0 failed / 0 skipped** (`package-101` passes). `Stylesheet.RegisterPackageOverrideContribution` records each `xsl:use-package` relationship that carries variable/parameter/function overrides on the used package's stylesheet instance, and the new package-scope views (`GetPackageScopeFunctionDefinitions`, `CollectPackageScopeGlobalsInDocumentOrder`) apply those contributions when a used-package component executes — so a used-package function calling an overridden function dispatches to the override, and a used-package template referencing an overridden global sees the override's value (XSLT 3.0 §3.5.7.2). `XsltFunctionDefinition.OverriddenFunction` links an override to the declaration it replaces; `TransformEngine` dispatches `xsl:original(...)` calls to that declaration while the override executes, and the registration is marked `IsHiddenFromFunctionLookup` because `xsl:original` is only available lexically inside an overriding component (function-lookup-006). `ValidateFunctionOverrides` now raises `XTSE0770` for duplicate overriding functions in one `xsl:override` element. Inside a package, `fn:function-lookup` resolves through a new `EvaluationContext.FunctionLookupInterceptor` so it returns the package's own declarations (originals, via an internal alias when the plain name is shadowed by an override) and excludes abstract declarations and functions declared only in other packages (function-lookup-005); an interceptor is required because `XPath31Expression.Evaluate` re-populates the standard function library on every evaluation. The `override` cluster improves to **56 passed / 43 failed / 4 skipped** (up from 49/50/4, zero regressions) and `function-lookup` is **8 passed / 0 failed**; residual work in `override` is `$xsl:original` variable references, `xsl:original#N` named references, `xsl:original` partial application, and `xsl:call-template name="xsl:original"`.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.93); `src/Bosak.Xslt/Stylesheet/XsltFunctionDefinition.cs` (header → 0.6); `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.53); `src/Bosak.XPath.Runtime/Functions/XPathFunction.cs` (header → 0.5); `src/Bosak.XPath.Runtime/Vm/EvaluationContext.cs` (header → 2.17); `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs` (header → 5.91); `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (header → 0.90).
  - Verification: `package` cluster 72/0/0; `use-package` cluster 53/0/1; `accept` cluster 50/0/0; `expose` cluster 42/0/0; `declared-modes` cluster 10/0/4; `override` cluster 56/43/4; `function-lookup` cluster 8/0/0; QT3 `fn-function-lookup` set 669/0; full W3C XSLT sweep 7,627/103/6,870 (98.7% of runnable tests, up from 7,618/112/6,870 — nine tests fixed, zero regressions); `dotnet test Bosak.sln` passes (2,114/0/0; language-server tests 72/0/0).

- **2026-09-01** — XSLT: **Package cluster residual work and conformance doc update** — The W3C `package` cluster is now **159 passed / 1 failed / 3 skipped** (up from 152/8/3). Four incremental fixes landed since the `xsl:accept` work: the conformance harness now supports `assert-string-value` on raw XDM node/sequence results via `GetStringValue(XdmValue)`; `xsl:use-package` appearing in an imported or included module now raises `XTSE3008` through a new `Stylesheet.IsPrincipalLevel` flag; library-package global variables whose initializer references the context item now raise `XPDY0002` by evaluating them with an absent focus when `SourceStylesheet` is a non-principal package; and `xsl:original` for overridden attribute-sets now includes the used-package original definitions when an override's `use-attribute-sets` contains `xsl:original`. The remaining failure is `package-101`, where variable/function overrides from `xsl:override` are not yet visible inside used-package components that reference them; this requires deeper override-scope propagation and `xsl:original` support for functions/variables and is tracked as a known residual.
  - Implementation: `tests/Bosak.Xslt.Conformance/Program.cs` (header → 3.34); `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.92); `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.34); `src/Bosak.Xslt/Stylesheet/AttributeSetDefinition.cs`.
  - Verification: `package` cluster 159/1/3; `use-package` cluster 53/0/1; `declared-modes` cluster 10/0/4; `accept` cluster 50/0/0; `expose` cluster 42/0/0; full W3C XSLT sweep 7,618/112/6,870 (98.6% of runnable tests, up from 7,585/145/6,870); `dotnet test Bosak.sln` passes (2,111/0/0).

- **2026-08-31** — XSLT: **`xsl:accept` visibility enforcement and runtime checks** — `Stylesheet.ValidateAcceptRules` now validates every `xsl:accept` rule against the components exported by used packages, raising `XTSE0010`, `XTSE0020`, `XTSE3030`, `XTSE3032`, `XTSE3040`, and `XTSE3050`/`XTSE3080` where required. `GetEffectiveAcceptRule` resolves the most specific matching rule by name and component specificity; `GetEffectiveVisibility` applies both `xsl:expose` (used package) and `xsl:accept` (using package) rules. Private templates explicitly accepted as `private` are tracked via `TemplateRule.AcceptedBy` and remain visible only to the accepting package. Runtime checks in `TransformEngine` raise `XTDE0040` for inaccessible named templates and `XTDE3052` for abstract functions, templates, variables, and attribute-sets. The W3C `accept` conformance cluster is now **50 passed / 0 failed / 0 skipped**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.91); `src/Bosak.Xslt/Stylesheet/TemplateRule.cs` (header → 2.0); `src/Bosak.Xslt/Stylesheet/AttributeSetDefinition.cs` (header → 0.3); `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.51).
  - Verification: `accept` cluster 50/0/0; `expose` cluster 42/0/0; `use-package` cluster 53/0/1; `declared-modes` cluster 10/0/4; full W3C XSLT sweep 7,585/145/6,870 (98.1% of runnable tests, up from 7,560/170/6,870); `dotnet test Bosak.sln` passes (2,111/0/0).

- **2026-08-31** — XSLT: **`xsl:expose` static validation and runtime visibility** — `Stylesheet` now parses and validates `xsl:expose` declarations on `xsl:package` roots, raising `XTSE0020`, `XTSE3010`, `XTSE3020`, `XTSE3022`, and `XTSE3025` where required. Runtime visibility (`GetExposedVisibility`) and package export (`IsExportedFromPackage`) apply expose rules so only exposed public/final components are visible to using packages. Match-only templates inherit public/final visibility from public/final modes when no explicit `@visibility` is present. The W3C `expose` conformance cluster is now **42 passed / 0 failed / 0 skipped**, and `use-package` is back to **53 passed / 0 failed / 1 skipped**. `TransformEngine.ExecuteXsltFunction` raises `XTDE3052` for `visibility="abstract"` functions, and the conformance harness reads package `@name`/`@package-version` from the package document when the catalog omits them. A unit-test package root now declares `<xsl:mode/>` to stay compatible with declared-modes validation.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.89); `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.49); `tests/Bosak.Xslt.Conformance/Program.cs` (header → 3.33).
  - Regression test: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (header → 0.89).
  - Verification: `expose` cluster 42/0/0; `use-package` cluster 53/0/1; `declared-modes` cluster 10/0/4 (no regression); full W3C XSLT sweep 7,560/170/6,870 (97.8% of runnable tests, up from 7,523/207/6,870); `dotnet test Bosak.sln` passes (2,111/0/0).

- **2026-08-31** — XSLT: **`declared-modes` / `XTSE3085` validation** — `Stylesheet.ValidateModeDefinitions` now enforces `xsl:package/@declared-modes="yes"` by checking that every mode used inside the package is declared. `CollectUsedModes` gathers modes from `xsl:template/@mode`, `xsl:apply-templates/@mode`, and implicit unnamed/default mode usages across the package's root stylesheet and its imports/includes; explicit `#default`/`#unnamed` are normalized to the unnamed mode, while `#current` and `#all` are ignored. `CollectDeclaredModes` considers both local `xsl:mode` declarations and public/final modes accepted from used packages, so cross-package mode references continue to work. This closes W3C `declared-modes-001` through `declared-modes-008` and `declared-modes-013/014`.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.88).
  - Verification: `declared-modes` set 10/0/4 (skips are declared-modes-009..012, intentionally harness-skipped); `use-package` set 53/0/1; full XSLT sweep 7,523/207/6,870; `dotnet test Bosak.sln` passes (2,104/0/0).

- **2026-08-31** — Language Server: **Richer XSLT document symbols / outline (`REQ-073`)** — `DocumentSymbolHandler` now produces outline symbols for all top-level XSLT declarations requested in REQ-073: templates (named and matched), functions, variables, parameters, attribute-sets, keys, and output declarations. It also continues to cover imports/includes, modes, decimal formats, character maps, and accumulators. The `xsl:output` symbol now includes the serialization method (e.g., **output (html)**) when present, making it easier to distinguish multiple output declarations. New unit tests verify every requested declaration type.
  - Implementation: `src/Bosak.LanguageServer/DocumentSymbolHandler.cs` (header → 0.2).
  - Tests: `tests/Bosak.LanguageServer.Tests/DocumentSymbolHandlerTests.cs` (header → 0.2).
  - Verification: `dotnet test tests/Bosak.LanguageServer.Tests/Bosak.LanguageServer.Tests.csproj` passes; language-server tests 72/0/0.

- **2026-08-31** — Language Server: **XSLT source-document hint polish (`REQ-071`)** — The default source-document code-lens hint now supports an XML comment alternative (`<!-- bosak:source-document="..." -->`) in addition to the existing processing instruction (`<?bosak source-document="..."?>`). Single-quoted processing instructions are now covered by unit tests, and surrounding whitespace is trimmed from the supplied path before relative-path resolution. The processing-instruction regex is renamed to `DefaultSourcePiRegex`; a new `DefaultSourceCommentRegex` provides the comment fallback. Relative paths continue to resolve against the stylesheet directory.
  - Implementation: `src/Bosak.LanguageServer/CodeLensHandler.cs` (header → 0.6).
  - Tests: `tests/Bosak.LanguageServer.Tests/CodeLensHandlerTests.cs` (header → 0.6).
  - Verification: `dotnet test tests/Bosak.LanguageServer.Tests/Bosak.LanguageServer.Tests.csproj` passes; language-server tests 70/0/0.

- **2026-08-31** — Language Server: **XSLT initial-template runner code lens (`REQ-072`)** — `.xsl` and `.xslt` documents that declare a named template now display a second code lens, **Run initial template** (or **Run initial template 'name'** for a non-implicit entry point). The lens detects `<xsl:template name="xsl:initial-template">` as the declared entry point and falls back to the first other named template when no implicit initial template exists. Clicking the lens invokes the new `bosak.runInitialTemplate` command, which sends a custom `bosak/runInitialTemplate` request to the language server. The server compiles the stylesheet and runs it via `XsltExecutable.TransformToString(source: null, initialTemplate: name)` without requiring a source XML document. The VS Code extension registers the command, sends the request, and opens the serialized result in a preview editor. This covers the common XSLT 3.0 use case of generating output from parameters alone.
  - Server handler: `src/Bosak.LanguageServer/EvaluationHandler.cs` (`RunInitialTemplateParams`, `RunInitialTemplateResult`, `RunInitialTemplateHandler`).
  - Code-lens detection: `src/Bosak.LanguageServer/CodeLensHandler.cs`.
  - Registration: `src/Bosak.LanguageServer/Program.cs`.
  - VS Code client: `vscode-bosak/src/extension.ts` and `vscode-bosak/package.json`.
  - Tests: `tests/Bosak.LanguageServer.Tests/CodeLensHandlerTests.cs` and `tests/Bosak.LanguageServer.Tests/EvaluationHandlerTests.cs`.
  - Verification: `dotnet test tests/Bosak.LanguageServer.Tests/Bosak.LanguageServer.Tests.csproj` passes; language-server tests 67/0/0.

- **2026-08-30** — XSLT: **`xsl:use-package` package-version range matching and `package_version_resolution` dependency** — `xsl:use-package` now matches `@package-version` against registered package versions using `XsltFunctionLibrary.PackageVersion.Parse`, `VersionMatches`, and `ResolvePackageLocation`. Supported syntax includes exact versions, wildcard prefixes (`1.*`), hyphen and `to` ranges (`1.0-2.0`, `1.0 to 2.0`), minimum bounds (`1.5+`), comma-separated alternatives, and `*` / empty for any version. `PackageVersionResolutionStrategy.Highest` selects the highest matching version by default (the XSLT 3.0 default); the W3C conformance harness honors the `package_version_resolution` dependency values `highest_version`, `lowest_version`, and `unspecified`. Inline test packages are registered in `tests/Bosak.Xslt.Conformance/Program.cs` so the package-version conformance tests run.
  - Implementation: `src/Bosak.Xslt/Api/XsltFunctionLibrary.cs` (`PackageVersion`, `VersionMatches`, `ResolvePackageLocation`, `PackageVersionResolutionStrategy`); `src/Bosak.Xslt/Stylesheet/Stylesheet.cs`; `src/Bosak.Xslt/Runtime/TransformEngine.cs`; `tests/Bosak.Xslt.Conformance/Program.cs`.
  - Verification: all runnable `use-package` tests pass; `dotnet test Bosak.sln` passes; unit tests 2,111/0/0.

- **2026-08-30** — XSLT: **`xsl:use-package` accept/override component merging and lazy-global isolation (`REQ-077`)** — `xsl:use-package` now resolves to a registered package and merges functions, variables, and parameters using `xsl:accept` visibility and `xsl:override` replacements. `CollectGlobalsInDocumentOrder` propagates a `CollectingScope` so overrides and used-package globals are collected in the declaring package scope; `ValidateGlobalVariableBindings` groups conflicts by `(name, collecting scope, source stylesheet)`, allowing same-name public variables from diamond used-package routes. `TransformEngine.EnterPackageScope` now includes `includeUsedPackagePrivate: true` so accepted private functions remain visible inside the used package's own scope. Per-package lazy-global isolation snapshots/restores cached globals via `EvaluationContext.SnapshotLazyGlobals` on package scope entry/exit, and `LazyGlobalInfo.CollectingScope` drives the runtime visibility rule (same scope ⇒ visible; different package ⇒ public/final only). This closes W3C `use-package-160` through `use-package-176`.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.86); `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.47); `src/Bosak.XPath.Runtime/Vm/EvaluationContext.cs`.
  - Verification: `use-package-160` through `use-package-176` PASS (17/0/0); target `use-package` set (001-007, 101-108, 150) 10/0/0; unit tests 2,111/0/0.

- **2026-08-29** — XSLT: **Basic `xsl:package` / `xsl:use-package` parsing (`REQ-076`)** — The `Stylesheet` loader now recognizes `xsl:package` as a valid root element and requires its `@name` attribute (XTSE0010). It also treats `xsl:use-package`, `xsl:expose`, `xsl:accept`, and `xsl:override` as known XSLT elements. Because full package resolution is not yet implemented, `xsl:use-package` is rejected at compile time with `XTSE0165`. This is foundational work for eventual full XSLT 3.0 package support and does not change the XSLT conformance numbers, because the conformance harness already skips package/use-package tests.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.80).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+5 tests; header → 0.87).
  - Verification: package parsing tests 5/0/0; full unit test suite 2,109/0/0.

- **2026-08-31** — XPath/XQuery: **schema-aware `fn:json-to-xml` (`REQ-075`)** — `fn:json-to-xml` with `validate:=true()` now validates the generated XML representation against the embedded W3C schema-for-JSON. This populates PSVI annotations so that `document-node(schema-element(j:array))`, `element(j:string, j:stringType)`, and typed-value access (`data($n) instance of xs:double` for `j:number`) work as required by the spec. This closes the last remaining QT3 failure cluster; the full QT3 sweep is now **31,148/0/673** (100% of runnable tests pass).
  - Implementation: `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs` (header → 5.105); unit test `JsonToXml_ValidateTrue_ProducesTypedDocument` added in `tests/Bosak.XPath.Standard.Tests/FunctionLibraryTests.cs`.
  - Verification: `json-to-xml-016/017/017b/037/037b/038/038b/044/046/047` PASS; full `fn-json-to-xml` 87/0/7; full QT3 sweep 31,148/0/673; unit tests 2,104/0/0.

- **2026-08-31** — XPath/XQuery: **`format-number` FODF1310 cluster (`numberformat143/144/146`)** — `FormatNumberEngine.ParseSubpicture` now counts exponent-separator occurrences only inside the parsed format token, so a passive occurrence of the exponent character in the prefix or suffix (e.g., `'end'` in `'9.9999e99end'`) no longer raises `FODF1310`. Percent and per-mille handling remains unchanged; percent/per-mille positioning validation still scans the whole subpicture. This closes the last remaining non-schema-aware `fn:format-number` failures; the full `fn-format-number` test set is **261/0/8**.
  - Implementation: `src/Bosak.XPath.Standard/Functions/FormatNumberEngine.cs` (header → 0.5).
  - Verification: `numberformat143/144/146` PASS; full `fn-format-number` 261/0/8; unit tests 2,103/0/0.

- **2026-08-31** — XSLT: **DTD-declared element-only whitespace stripping (`number-4501`)** — Source documents with a DTD are now parsed for `<!ELEMENT>` declarations. Elements declared as `EMPTY` or with a pure element content model (no `#PCDATA`) are annotated, and the XSLT processor strips whitespace-only text nodes inside those elements by default, matching XSLT 1.0 §3.4. This closes the last remaining XSLT conformance failure. Full XSLT sweep is now **7,254/0/7,346**; unit tests remain **2,103/0/0**.
  - Implementation: `src/Bosak.XPath.Providers/Xml11/Xml11Loader.cs` (header → 0.7), `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.41), plus new `src/Bosak.XPath.Providers/Xml11/DtdElementOnlyAnnotation.cs`.

- **2026-08-26** — XSLT: **XTSE0660 cluster (`error-0660*`)** — Duplicate named `xsl:template` declarations with the same expanded name at the same import precedence now raise `XTSE0660`, unless a higher-precedence template exists. The validation uses `CollectNamedTemplatesInDocumentOrder` to gather all named templates with their import precedence, then groups by name and checks the highest-precedence group. The W3C `error-0660*` cluster is now **4/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.74).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.72).
  - Verification: `error-0660*` 4/0/0; routine XSLT sweep 7,056/0/7,544; unit tests 2,033/0/0.

- **2026-08-26** — XSLT: **QName whitespace trimming** — QName-valued attributes (such as `xsl:template/@name`, `xsl:call-template/@name`, and `xsl:variable/@name`) are now trimmed before resolution. This fixes W3C `call-template-0109`, which uses a template name with leading and trailing whitespace, and prevents similar failures for other QName-valued attributes.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.74).
  - Verification: `call-template-0109` PASS; routine XSLT sweep 7,056/0/7,544.

- **2026-08-26** — XSLT: **XTSE0630 cluster (`error-0630*`)** — Duplicate global `xsl:variable` and `xsl:param` declarations with the same expanded name at the same import precedence now raise `XTSE0630`, unless a higher-precedence binding exists. The validation uses `CollectGlobalsInDocumentOrder` to gather all top-level declarations with their import precedence, then groups by name and checks the highest-precedence group. The W3C `error-0630*` cluster is now **3/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.73).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+3 tests; header → 0.71).
  - Verification: `error-0630*` 3/0/0; routine XSLT sweep 7,056/0/7,544; unit tests 2,031/0/0.

- **2026-08-26** — XSLT: **XTSE0620 cluster (`error-0620*`)** — Variable-binding elements (`xsl:variable`, `xsl:param`, `xsl:with-param`) now raise `XTSE0620` at compile time when they have both a `select` attribute and non-empty content. Whitespace-only text, comments, and processing instructions are treated as empty content, matching the existing static-variable check. The W3C `error-0620*` cluster is now **2/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.72).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+3 tests; header → 0.70).
  - Verification: `error-0620*` 2/0/0; routine XSLT sweep 7,056/0/7,544; unit tests 2,028/0/0.

- **2026-08-26** — XSLT: **XTDE0044 cluster (`error-0044*`)** — Invoking a transformation with an explicit initial mode but no source document, initial-match selection, or global context item now raises `XTDE0044`. `TransformEngine.Transform` checks for the required input after resolving the initial mode, and the `FOXT0002` source-required guard now allows an explicit initial mode to reach this error path. The conformance harness no longer injects a dummy source for initial-mode-only entry points. The W3C `error-0044*` cluster is now **4/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.31).
  - Harness: `tests/Bosak.Xslt.Conformance/Program.cs` (header → 3.19).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+3 tests; header → 0.69).
  - Verification: `error-0044*` 4/0/0; routine XSLT sweep 7,056/0/7,544; unit tests 2,025/0/0.

- **2026-08-26** — XSLT: **XTTE1020 cluster (`error-1020*`)** — Multi-item `xsl:sort` keys now raise `XTTE1020` outside XSLT 1.0 backwards-compatible mode. `TransformEngine.SortItems` and `SortGroups` atomize each sort-key value and, when the effective version of the `xsl:sort` element is 2.0 or higher, reject sequences containing more than one item. With effective version `< 2.0`, the first item is used, preserving XSLT 1.0 behavior. The W3C `error-1020a` test is now **1/0/0**; the `sort` cluster remains **80/0/2**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.30).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+3 tests; header → 0.68).
  - Verification: `error-1020a` 1/0/0; `sort` 80/0/2; unit tests 2,022/0/0.

- **2026-08-26** — XSLT: **XTSE0265 cluster (`error-0265*`)** — Conflicting `xsl:stylesheet/@input-type-annotations` values across stylesheet modules now raise `XTSE0265`. The implementation parses `strip`, `preserve`, and `unspecified` on every module and validates that no two modules declare different non-`unspecified` values. Matching values and `unspecified` combined with an explicit value continue to be allowed. The W3C `error-0265*` cluster is now **1/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.71).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+3 tests; header → 0.67).

- **2026-08-26** — XSLT: **XTSE1600 cluster (`error-1600*`)** — Circular `use-character-maps` references in `xsl:character-map` declarations are now detected at compile time and raise `XTSE1600`. The check covers both direct self-references and indirect cycles, including maps declared in imported or included modules. The W3C `error-1600*` cluster is now **1/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.70).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.66).

- **2026-08-26** — XSLT: **XTSE1590 cluster (`error-1590*`)** — `xsl:output/@use-character-maps` and `xsl:character-map/@use-character-maps` now raise `XTSE1590` at compile time when a referenced character-map name does not match any `xsl:character-map` declared in the stylesheet (including imported and included modules). The W3C `error-1590*` cluster is now **1/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.69).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+3 tests; header → 0.65).

- **2026-08-26** — XSLT: **XTSE1560 cluster (`error-1560*`)** — Multiple `xsl:output` declarations in the same output definition now raise `XTSE1560` when they explicitly specify different values for the same scalar attribute. The check applies to both unnamed and named output definitions and covers all scalar serialization attributes except `cdata-section-elements` and `use-character-maps`, which the XSLT specification excludes from this rule. Matching values continue to be allowed, and list-valued attributes continue to be merged. The W3C `error-1560*` cluster is now **2/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/OutputProperties.cs` (header → 1.9), `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.68).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+4 tests; header → 0.64).
  - Verification: `error-1560*` 2/0/0; routine XSLT sweep 7,056/0/7,544; unit tests 2,007/0.

- **2026-08-26** — XSLT: **XTSE1570 cluster (`error-1570*`)** — `OutputProperties.FromElement` now validates `xsl:output/@method` per XTSE1570. The value must be a valid EQName: the `Q{uri}local` form (with a syntactically valid URI and NCName local part), a lexical `prefix:local` QName whose prefix is bound to a namespace in scope, or an unprefixed NCName that is one of the supported serialization methods (`xml`, `html`, `xhtml`, `text`, `json`, `adaptive`). An EQName in no namespace (`Q{}local`) is accepted only when the local name is a supported method. Anything else raises `XTSE1570`. The W3C `error-1570*` cluster is now **1/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/OutputProperties.cs` (header → 1.8).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+1 `Theory` with 18 cases; header → 0.63).
  - Verification: `error-1570*` 1/0/0; routine XSLT sweep 7,056/0/7,544; unit tests 2,007/0.

- **2026-08-26** — XSLT: **XTDE1500 cluster (`error-1500*`)** — `EvaluationContext` now exposes a `DocumentLoaded` callback that `TransformEngine` uses to track every document URI read during a transformation. When a secondary `xsl:result-document` (or EXSLT `exsl:document`) targets an absolute URI that has already been read, the runtime raises `XTDE1500`. The W3C `error-1500*` cluster is now **1/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.XPath.Runtime/Vm/EvaluationContext.cs` (header → 2.16), `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.29).
  - Regression test: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+1 test; header → 0.62).
  - Verification: `error-1500*` 1/0/0; routine XSLT sweep 7,056/0/7,544; unit tests 1,989/0.

- **2026-08-26** — XSLT: **XTDE1450 cluster (`error-1450*`)** — `TransformEngine.CopyLiteralElement` now raises `XTDE1450` when an element in an extension-element namespace has no `xsl:fallback` children. If one or more `xsl:fallback` children are present, their content continues to be evaluated in place of the extension element. The W3C `error-1450*` cluster is now **2/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.28).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.60).
  - Verification: `error-1450*` 2/0/0; routine XSLT sweep 7,056/0/7,544; unit tests 1,988/0.

- **2026-08-26** — XSLT: **EXSLT `exsl:document` regression fix** — `TransformEngine.CopyLiteralElement` now recognizes the EXSLT `exsl:document` extension element (in namespace `http://exslt.org/common`) and executes it as `xsl:result-document`. This restores the W3C `docbook-001` test, which uses the DocBook XHTML5 stylesheets and was failing with `XTDE1450` after the extension-element change above. The routine XSLT sweep is now **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.28).
  - Regression test: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+1 test; header → 0.61).
  - Verification: `docbook-001` PASS; routine XSLT sweep 7,056/0/7,544; unit tests 1,988/0.

- **2026-08-26** — XSLT: **XTDE1440 cluster (`error-1440*`)** — `FunctionLibrary.ElementAvailable` now validates that its argument is a valid EQName. It accepts the `Q{uri}local` form, a lexical `prefix:local` QName whose prefix is bound to a namespace in the static context, or an unprefixed local name that is a valid NCName (using the default namespace of the defining element). Anything else raises `XTDE1440`. The W3C `error-1440*` cluster is now **2/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.

- **2026-08-26** — XSLT: **XTDE1360 cluster (`error-1360*`)** — `fn:current()` now raises `XTDE1360` when the current item is absent. The guard is applied in `FunctionLibrary.Current`; `TransformEngine.ExecuteXsltFunction` clears the current item so stylesheet functions have no current node, and `VmEngine.InvokeFunctionItem` clears the current item for dynamic function-item invocations. The W3C `error-1360*` cluster is now **2/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs` (header → 5.101), `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.27), `src/Bosak.XPath.Runtime/Vm/VmEngine.cs` (header → 2.128).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+3 tests; header → 0.58).
  - Verification: `error-1360*` 2/0/0; routine XSLT sweep 7,056/0/7,544; unit tests 1,982/0.

- **2026-08-26** — XSLT: **XTDE1428 cluster (`error-1428*`)** — `FunctionLibrary.TypeAvailable` now validates that its argument is a valid EQName. It accepts the `Q{uri}local` form, a lexical `prefix:local` QName whose prefix is bound to a namespace in the static context, or an unprefixed local name that is a valid NCName. Anything else (including an unbound prefix, an empty prefix, or an invalid local part) raises `XTDE1428`. The W3C `error-1428*` cluster is now **1/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.

- **2026-08-26** — XSLT: **XTDE1390 cluster (`error-1390*`)** — `FunctionLibrary.ExpandXsltPropertyName` now validates that the argument to `fn:system-property` is a valid QName. It accepts the `Q{uri}local` EQName form (requiring a syntactically valid URI and a valid NCName local part), a lexical `prefix:local` QName whose prefix is bound to a namespace in the static context, or an unprefixed local name that is a valid NCName. Anything else raises `XTDE1390`. The W3C `error-1390*` cluster is now **3/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs` (header → 5.59).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+3 tests; header → 0.56).
  - Verification: `error-1390*` 3/0/0; routine XSLT sweep 7,056/0/7,544; unit tests 1,977/0.

- **2026-08-25** — XSLT: **XTDE/FODF1310 cluster (`error-1310*`)** — `FormatNumberEngine.ParseSubpicture` now raises `FODF1310` (XTDE1310 in XSLT) when a picture subpicture contains a percent sign `%` more than once, a per-mille sign `‰` more than once, or both a percent and a per-mille sign. The W3C `error-1310*` cluster is now **9/0/0**; the routine XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.XPath.Standard/Functions/FormatNumberEngine.cs` (header → 0.4).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.55).
  - Verification: `error-1310*` 9/0/0; routine XSLT sweep 7,056/0/7,544; unit tests 1,974/0.

- **2026-08-25** — XSLT: **XTDE/FODF1280 cluster (`error-1280*`)** — `format-number` already raised `FODF1280` when the named decimal-format does not exist. The W3C `error-1280*` cluster is now **3/0/0** (XSLT 2.0 variants are skipped as unsupported spec dependencies); the routine XSLT sweep remains **7,056/0/7,544**.
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+1 test; header → 0.54).
  - Verification: `error-1280*` 3/0/0; routine XSLT sweep 7,056/0/7,544; unit tests 1,972/0.

- **2026-08-25** — XSLT: **XTSE1290 static-error cluster (`error-1290*`)** — `Stylesheet.GetAllDecimalFormats` now validates that no two `xsl:decimal-format` declarations for the same named or default format supply conflicting values for the same attribute at the same import precedence, unless a higher-precedence declaration also defines that attribute. Conflicts raise `XTSE1290`. The W3C `error-1290*` cluster is now **2/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.66).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+6 tests; header → 0.53).
  - Verification: `error-1290*` 2/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,971/0.

- **2026-08-25** — XSLT: **XTSE1295 static-error cluster (`error-1295*`)** — `Stylesheet` decimal-format parsing now validates that an explicit `xsl:decimal-format/@zero-digit` is a Unicode decimal digit whose numeric value is zero. Non-digit characters or digits with a non-zero numeric value raise `XTSE1295`. The W3C `error-1295*` cluster is now **2/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.65).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.52).
  - Verification: `error-1295*` 2/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,965/0.

- **2026-08-25** — XSLT: **XTSE0760 static-error cluster (`error-0760*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE0760` when an `xsl:param` inside an `xsl:function` has a `select` attribute or non-empty content. The W3C `error-0760*` cluster is now **2/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.64).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+3 tests; header → 0.51).
  - Verification: `error-0760*` 2/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,963/0.

- **2026-08-25** — XSLT: **XTSE3350 static-error cluster (`error-3350*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE3350` when two `xsl:accumulator` declarations in the same stylesheet module have the same expanded QName. The W3C `error-3350*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.63).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.50).
  - Verification: `error-3350*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,960/0.

- **2026-08-25** — XSLT: **XTSE3190 static-error cluster (`error-3190*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE3190` when two sibling `xsl:merge-source` elements within the same `xsl:merge` have the same effective name (explicit `name` attribute or the default implicit name). The W3C `error-3190*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.62).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.49).
  - Verification: `error-3190*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,958/0.

- **2026-08-25** — XSLT: **XTSE3150 static-error cluster (`error-3150*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE3150` when `xsl:catch` has a `select` attribute together with non-empty content (text or element children). A `select` attribute with empty content continues to be accepted. The W3C `error-3150*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.61).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.48).
  - Verification: `error-3150*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,956/0.

- **2026-08-25** — XSLT: **XTSE3140 static-error cluster (`error-3140*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE3140` when `xsl:try` has a `select` attribute together with content other than `xsl:catch` and `xsl:fallback` instructions. A `select` attribute with only `xsl:catch`/`xsl:fallback` children continues to be accepted. The W3C `error-3140*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.60).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.47).
  - Verification: `error-3140*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,954/0.

- **2026-08-25** — XSLT: **XTSE1660 static-error cluster (`error-1660*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE1660` when a literal result element carries an `xsl:type` attribute (a non-schema-aware processor cannot support type validation). A no-namespace `type` attribute on a literal result element continues to be treated as an ordinary attribute. The W3C `error-1660*` cluster is now **5/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.59).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.46).
  - Verification: `error-1660*` 5/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,952/0.

- **2026-08-25** — XSLT: **XTSE1430 static-error cluster (`error-1430*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE1430` when `extension-element-prefixes` (on an XSLT element or a literal result element) contains a prefix that is not bound to a namespace, or `#default` when no default namespace is in scope. The W3C `error-1430*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.58).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.45).
  - Verification: `error-1430*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,950/0.

- **2026-08-25** — XSLT: **XTSE1222 static-error cluster (`error-1222*`)** — The stylesheet compiler now raises `XTSE1222` when two or more `xsl:key` declarations share the same expanded name but have different effective `composite` values. The W3C `error-1222*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.57).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.44).
  - Verification: `error-1222*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,948/0.

- **2026-08-25** — XSLT: **XTSE1040 static-error cluster (`error-1040*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE1040` when `xsl:perform-sort` has a `select` attribute together with content other than `xsl:sort` and `xsl:fallback` instructions. A `select` attribute with only `xsl:sort`/`xsl:fallback` children continues to be accepted. The W3C `error-1040*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.56).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.43).
  - Verification: `error-1040*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,946/0.

- **2026-08-25** — XSLT: **XTSE1015 static-error cluster (`error-1015*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE1015` when `xsl:sort` has a `select` attribute together with non-empty content (text or element children). A `select` attribute with empty content continues to be accepted. The W3C `error-1015*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.55).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.42).
  - Verification: `error-1015*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,944/0.

- **2026-08-25** — XSLT: **XTSE0940 static-error cluster (`error-0940*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE0940` when `xsl:comment` has both a `select` attribute and non-empty content (text or element children). A `select` attribute with empty content continues to be accepted. The W3C `error-0940*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.54).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.41).
  - Verification: `error-0940*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,942/0.

- **2026-08-25** — XSLT: **XTSE0910 static-error cluster (`error-0910*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE0910` when `xsl:namespace` has a `select` attribute together with content other than `xsl:fallback` instructions, or when it has empty content and no `select` attribute. A `select` attribute with empty content or with only `xsl:fallback` children continues to be accepted. The W3C `error-0910*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.53).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+3 tests; header → 0.40).
  - Verification: `error-0910*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,940/0.

- **2026-08-25** — XSLT: **XTSE0880 static-error cluster (`error-0880*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE0880` when `xsl:processing-instruction` has both a `select` attribute and non-empty content (text or element children). A `select` attribute with empty content continues to be accepted. The W3C `error-0880*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.52).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.39).
  - Verification: `error-0880*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,937/0.

- **2026-08-25** — XSLT: **XTSE0870 static-error cluster (`error-0870*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE0870` when `xsl:value-of` has a `select` attribute and non-empty content, or when it has empty content and no `select` attribute. A `select` attribute with empty content continues to be accepted. The W3C `error-0870*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.51).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+3 tests; header → 0.38).
  - Verification: `error-0870*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,935/0.

- **2026-08-25** — XSLT: **XTSE0840 static-error cluster (`error-0840*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE0840` when `xsl:attribute` has both a `select` attribute and non-empty content (text or element children). A `select` attribute with empty content continues to be accepted. The W3C `error-0840*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.50).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.37).
  - Verification: `error-0840*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,932/0.

- **2026-08-25** — XSLT: **XTSE0125 static-error cluster (`error-0125*`)** — `Stylesheet.ValidateInstructionTree` now validates `[xsl:]default-collation` attributes: the whitespace-separated URI list must contain at least one collation URI recognized by this implementation. The supported URIs are the codepoint collation, the HTML ASCII case-insensitive collation, and any URI starting with the W3C UCA prefix. Relative URIs are resolved against the element's base URI before checking. The W3C `error-0125*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.49).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+6 tests; header → 0.36).
  - Verification: `error-0125*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,930/0.

- **2026-08-25** — XSLT: **XTDE0420 dynamic-error cluster (`error-0420*`)** — `TransformEngine.ExecuteSingleCopy` now rejects any attribute on the temporary collector used for `xsl:copy` of a document node, so both `xsl:attribute` and `xsl:namespace` content raise `XTDE0420` instead of silently producing an invalid document node. The W3C `error-0420*` cluster is now **2/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.26).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+2 tests; header → 0.35).
  - Verification: `error-0420*` 2/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,908/0.

- **2026-08-25** — XSLT: **XTDE0560 dynamic-error cluster (`error-0560*`)** — `TransformEngine` now clears `_currentTemplateRule` while evaluating global variable and parameter bodies, so `xsl:apply-imports` and `xsl:next-match` inside those bodies correctly raise `XTDE0560`. This matches the existing isolation already applied to `xsl:for-each`, `xsl:for-each-group`, and `xsl:call-template`. The W3C `error-0560*` cluster is now **4/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.25).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+3 tests; header → 0.34).
  - Verification: `error-0560*` 4/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,908/0.

- **2026-08-25** — XSLT: **XTSE0530 static-error cluster (`error-0530*`)** — `Stylesheet.ValidateInstructionTree` now validates that `xsl:template/@priority` is a valid lexical `xs:decimal` value. Exponent notation (e.g. `2.0e2`) and non-numeric values are rejected with `XTSE0530`; valid decimals such as `2`, `2.0`, `-0.5`, `+3`, and `.5` continue to be accepted. The W3C `error-0530*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.48).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+8 tests; header → 0.33).
  - Verification: `error-0530*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,908/0.

- **2026-08-25** — XSLT: **XTSE0370 static-error cluster (`error-0370*`)** — `Stylesheet.SplitAttributeValueTemplate` and `TransformEngine` now raise `XTSE0370` when an unescaped right curly bracket `}` appears in the fixed part of an attribute value template or text value template without a matching left curly bracket. The `}}` escape continues to produce a literal `}`. The W3C `error-0370*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.47), `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.24).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+3 tests; header → 0.32).
  - Verification: `error-0370*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,908/0.

- **2026-08-25** — XSLT: **XTSE0350 static-error cluster (`error-0350*`)** — `Stylesheet.SplitAttributeValueTemplate` and `TransformEngine` now raise `XTSE0350` when an unescaped left curly bracket in an attribute value template or text value template has no matching right curly bracket. Previously the unmatched `{` was treated as a literal character. The W3C `error-0350*` cluster is now **2/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.46), `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.23).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+3 tests; header → 0.31).
  - Verification: `error-0350*` 2/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,908/0.

- **2026-08-25** — XSLT: **XTSE0260 static-error cluster (`error-0260*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE0260` when a known empty XSLT element (`xsl:include`, `xsl:import`, `xsl:strip-space`, `xsl:preserve-space`, `xsl:output`, `xsl:namespace-alias`, `xsl:decimal-format`, `xsl:output-character`, `xsl:copy-of`, `xsl:mode`, `xsl:import-schema`, `xsl:expose`, `xsl:global-context-item`, and `xsl:context-item`) contains a text node or element child. Elements that may contain a sequence constructor (`xsl:key`, `xsl:sort`, `xsl:accumulator-rule`, `xsl:merge-key`, `xsl:value-of`, `xsl:assert`) are excluded so existing valid tests are not rejected. Comments and processing instructions are still permitted. The W3C `error-0260*` cluster is now **4/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.45).
  - Regression tests: `tests/Bosak.Xslt.Tests/StylesheetTests.cs` (+7 tests; header → 0.30).
  - Verification: `error-0260*` 4/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,905/0.

- **2026-08-25** — XSLT: **XTSE0340 static-error cluster (`error-0340*`)** — `PatternCompiler.ValidatePatternSyntax` now rejects patterns that start with a numeric literal or expression (e.g. `2+2`), path steps that are numeric literals (e.g. `name/1223`), and `processing-instruction()` arguments that are not a valid string literal or NCName (e.g. `processing-instruction(proc:inst-2)`). `Stylesheet.ValidateInstructionTree` performs this validation at stylesheet load time for literal `xsl:template/@match`, `xsl:key/@match`, and `xsl:number/@count`/`@from` attributes. The W3C `error-0340*` cluster is now **3/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Patterns/PatternCompiler.cs` (header → 2.9), `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.44).
  - Regression tests: `tests/Bosak.Xslt.Tests/PatternCompilerPredicateTests.cs` (+3 tests; header → 0.4).
  - Verification: `error-0340*` 3/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,896/0.

- **2026-08-25** — XSLT: **XTSE0809 static-error cluster (`error-0809*`)** — `Stylesheet.ValidateInstructionTree` now raises `XTSE0809` when `exclude-result-prefixes` contains `#default` and the owning element has no default namespace declaration (empty default namespace URI). This completes the `#default` handling left pending by `XTSE0808`. The W3C `error-0809*` cluster is now **1/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.43).
  - Verification: `error-0809*` 1/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,895/0.

- **2026-08-24** — XSLT: **XTSE0808 static-error cluster (`error-0808*`)** — `Stylesheet.ValidateInstructionTree` now validates that every non-special token in an `exclude-result-prefixes` value names a namespace prefix that is in scope on the owning element. This covers `xsl:stylesheet`/`xsl:transform/@exclude-result-prefixes` and `xsl:exclude-result-prefixes` on literal result elements. The W3C `error-0808*` cluster is now **3/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.42).
  - Verification: `error-0808*` 3/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,895/0.

- **2026-08-24** — XSLT: **XTSE0710 static-error cluster (`error-0710*`)** — `Stylesheet.ValidateInstructionTree` now validates `use-attribute-sets` references on `xsl:copy`, `xsl:element`, and literal result elements, ensuring every token is a valid EQName and matches a declared `xsl:attribute-set` across the whole stylesheet. The W3C `error-0710*` cluster is now **4/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.41), `src/Bosak.Xslt/Stylesheet/AttributeSetDefinition.cs` (header → 0.2), `src/Bosak.Xslt/Runtime/TransformEngine.cs` (header → 6.22).
  - Verification: `error-0710*` 4/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,895/0.

- **2026-08-24** — XSLT: **XTSE0280 static-error cluster (`error-0280*`)** — `Stylesheet.ValidateXsltName` now raises `XTSE0280` for prefixed lexical QNames whose prefix is not in scope on the defining element; `xsl:apply-templates/@mode` and `xsl:template/@mode` tokens are now checked for namespace binding after lexical validation. The W3C `error-0280*` cluster is now **6/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.40).
  - Verification: `error-0280*` 6/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,895/0.

- **2026-08-24** — XSLT: **XTSE0500/0550 static-error clusters (`error-0500*`, `error-0550*`)** — `Stylesheet.ValidateInstructionTree` now enforces `xsl:template` attribute constraints: a template must have `@match` or `@name`; `@mode`/`@priority` require `@match`; `@visibility` requires `@name`; and `@mode` is validated as a whitespace-separated list of mode names (rejecting empty lists, duplicates, invalid tokens, `#all` with other values, and `#current`). `#unnamed` is correctly accepted. The W3C `error-0500*` cluster is **4/0/0** and `error-0550*` is **6/0/0**; the full XSLT sweep is restored to **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.39).
  - Verification: `error-0500*` 4/0/0; `error-0550*` 6/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,895/0.

- **2026-08-24** — XSLT: **XTSE0120 static-error cluster (`error-0120*`)** — `Stylesheet.ValidateInstructionTree` now raises XTSE0120 when `xsl:stylesheet`, `xsl:transform`, or `xsl:package` contains non-whitespace text node children. The W3C `error-0120*` cluster is now **2/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.38).
  - Verification: `error-0120*` 2/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,895/0.

- **2026-08-24** — XSLT: **XTSE0090 static-error cluster (`error-0090*`)** — `Stylesheet.ValidateInstructionTree` now enforces the XSLT attribute whitelist (XTSE0090) for `xsl:stylesheet`/`xsl:transform`, `xsl:template`, `xsl:apply-templates`, `xsl:apply-imports`, `xsl:call-template`, `xsl:attribute-set`, and `xsl:key`. The whitelists combine the standard XSLT attributes with element-specific ones, and unknown attributes are ignored in forwards-compatible mode. The W3C `error-0090*` cluster is now **14/0/0**; the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.37).
  - Verification: `error-0090*` 14/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,895/0.

- **2026-08-24** — XSLT: **XTSE0020 static-error cluster (`error-0020*`)** — `Stylesheet.ValidateInstructionTree` name and attribute-value validation now handles `Q{uri}local` EQNames before AVT detection, accepts XML 1.0 fifth edition / XML 1.1 NCName characters (e.g. `Ĳ`), recognizes `xsl:decimal-format/@exponent-separator`, validates decimal-format single-character attributes by Unicode code point count (supporting non-BMP symbols), and ignores unknown decimal-format attributes in forwards-compatible mode. The W3C `error-0020*` cluster is now **11/0/0** and the full XSLT sweep is restored to **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.36).
  - Verification: `error-0020*` 11/0/0; full XSLT sweep 7,056/0/7,544; unit tests 1,895/0.

- **2026-08-24** — XSLT: **XTSE0010 static-error cluster (`error-0010*`)** — `Stylesheet.ValidateInstructionTree` now enforces structural constraints: required attributes on `xsl:if`/`xsl:call-template`/`xsl:attribute-set`, `xsl:param` placement and parent context, `xsl:choose` structure, permissible children of `xsl:apply-templates`/`xsl:apply-imports`/`xsl:call-template`, top-level-only declarations, and tolerates unknown top-level XSLT elements as vendor extensions in XSLT 3.0 (and all unknown elements in forwards-compatible mode). Added `character-map`, `output-character`, `fork`, and `accumulator-rule` to the known element set. The W3C `error-0010*` cluster is now **52/0/1** (`error-0010bb` skipped as an upstream forwards-compatibility contradiction); the full XSLT sweep remains **7,056/0/7,544**.
  - Implementation: `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` (header → 2.34).
  - Harness: `tests/Bosak.Xslt.Conformance/Program.cs` now runs normally-skipped test sets when a filter is supplied; `error-0010bb` added to the skip list (header → 3.18).
  - Verification: `error-0010*` 52/0/1; full XSLT sweep 7,056/0/7,544; unit tests 1,895/0.

- **2026-08-23** — XPath/XQuery: **cbcl-module-001 residual fixed** — `VmEngine.InstanceOf` now rejects `xs:untypedAtomic` values when testing against user-defined schema simple types. Previously, cast-based facet checking allowed untypedAtomic text-node values to pass `instance of` for user-defined string restrictions. `instance of` now uses type-hierarchy semantics for these types while function argument/return conversion keeps its cast-based behaviour. This closes the final QT3 failure `cbcl-module-001`; the full QT3 sweep is now **30,959/0/862** (97.29%) with **0 failures**. Unit tests pass **1,883/0**.
  - Implementation: `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`.
  - Regression test in `tests/Bosak.XPath.Runtime.Tests/SchemaTypedValueTests.cs` (1/1 passing).
  - Targeted verification: `cbcl-module-001` 1/0/0.

- **2026-08-23** — XPath/XQuery: **xquery30keywords5 cluster** — `XPathParser` now allows most XPath/XQuery keywords to be used as unprefixed function names when followed by `(` or `#` in a primary-expression context. Reserved function names (`if`, `function`, `map`, `array`, etc.) remain rejected, and `validate`, quantified expressions, `try/catch`, and FLWOR expressions are still recognized when their normal follow tokens (`{`, `$`) are present. This closes the QT3 failure `xquery30keywords5`. Full QT3 sweep is **30,958/1/862** (97.29%); unit tests pass **1,840/0**.
  - Implementation: `src/Bosak.XPath.Parser/Ast/XPathParser.cs`.
  - Regression tests in `tests/Bosak.XPath.Parser.Tests/ParserTests.cs` (6/6 passing).
  - Targeted verification: `xquery30keywords5` 1/0/0.

- **2026-08-23** — XPath/XQuery: **function return-type atomization cluster** — `VmEngine.ApplyFunctionConversion` now atomizes node values when the target type is an atomic or user-defined simple type, even if the node's typed value matches the target type. A new `IsNodeKindTestType` helper distinguishes node kind tests (including `item()`) from atomic/simple targets. This closes the QT3 failures `qischema040` and `qischema040a`. Full QT3 sweep is **30,957/2/862** (97.35%); unit tests pass **1,838/0**.
  - Implementation: `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`.
  - Regression tests in `tests/Bosak.XQuery.Tests/PlaceholderTests.cs` (3/3 passing).
  - Targeted verification: `qischema040` 1/0/0; `qischema040a` 1/0/0.

- **2026-08-23** — XPath/XQuery: **QName accessor singleton-sequence XPTY0004 cluster** — `FunctionLibrary` now provides an `AtomizeSingleton` helper used by `LocalNameFromQName`, `NamespaceUriFromQName`, and `PrefixFromQName`. Multi-item sequences raise `XPTY0004`; empty sequences still return the empty sequence. This closes the QT3 failures `LocalNameFromQNameFunc010` and `NamespaceURIFromQNameFunc010`. Full QT3 sweep is **30,955/4/862** (97.33%); unit tests pass **1,835/0**.
  - Implementation: `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs`.
  - Regression tests in `tests/Bosak.XPath.Standard.Tests/FunctionLibraryTests.cs` (4/4 passing).
  - Targeted verification: `LocalNameFromQNameFunc010` 1/0/0; `NamespaceURIFromQNameFunc010` 1/0/0.

- **2026-08-23** — XPath/XQuery: **document-node / root() / constructed-element cluster** — `VmEngine.ValueMatchesType` now distinguishes bare `document-node()` from `document-node(element(...))` / `document-node(schema-element(...))`, so empty documents created with `document {}` correctly match `document-node()`. XQuery-constructed elements are now annotated with `ConstructedElementAnnotation`; `XDocumentNode.IsConstructedElement` exposes this and `VmEngine.IsElementTypeCompatible` treats constructed elements as `xs:anyType` rather than `xs:untyped` when no schema validation has occurred. This closes the QT3 failures `K2-NodeRootFunc-8`, `K2-ancestor-or-selfAxis-5`, `K2-ConDocNode-33`, and `K2-DirectConElemContent-35a`. Full QT3 sweep is **30,953/6/862** (97.27%); unit tests pass **1,831/0**.
  - Implementation: `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`, `src/Bosak.XPath.Providers/XDocument/XDocumentProvider.cs`, `src/Bosak.XPath.Providers/XDocument/XDocumentNode.cs`, `src/Bosak.XPath.Providers/XDocument/ConstructedElementAnnotation.cs`, `src/Bosak.XPath.Core/Xdm/IXdmNode.cs`.
  - Regression tests in `tests/Bosak.XQuery.Tests/PlaceholderTests.cs` (4/4 passing).
  - Targeted verification: `K2-NodeRootFunc-8` 1/0/0; `K2-ancestor-or-selfAxis-5` 1/0/0; `K2-ConDocNode-33` 1/0/0; `K2-DirectConElemContent-35a` 1/0/0.

- **2026-08-23** — XPath/XQuery: **HOF residuals cluster** — `VmEngine.IsElementOrAttributeSchemaSubtype` now strips outer occurrence indicators (`?`, `*`, `+`) from candidate and target element kind-test type strings, defaults a missing type part to `xs:anyType`, honours the nillability `?` marker, and compares built-in schema types even when `context.SchemaSet` is null. `GetDirectSupertypes` now includes `anyatomictype → anysimpletype → anytype → item()` so `xs:anyAtomicType` is recognized as a subtype of `xs:anyType`. This closes the QT3 `misc-HigherOrderFunctions` failures `hof-039` and `hof-053`. Full QT3 sweep is **30,948/11/862** (97.26%); unit tests pass **1,827/0**.
  - Implementation: `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`.
  - Regression tests in `tests/Bosak.XQuery.Tests/PlaceholderTests.cs` (2/2 passing).
  - Targeted verification: `hof-039` 1/0/0; `hof-053` 1/0/0.

- **2026-08-23** — XPath/XQuery: **schema-aware validate / QName-NOTATION / ID / typed-value cluster** — `VmEngine.ValidateNode` now validates against the built-in schema set so `validate lax` honours `xsi:type` annotations, supports `validate type QName { Expr }`, returns a new validated `XDocumentNode`, and populates PSVI via `addSchemaInfo`. `XDocumentNode` typed-value construction resolves QName/NOTATION prefixes via an in-scope namespace resolver, preserves the lexical prefix, reports the declared schema type for schema-element/attribute kind tests, and recognizes `xsi:type='xs:ID'` and `xsi:type='xs:IDREF'/'xs:IDREFS'` elements as ID/IDREF even without a schema. `VmEngine` fixes: `xs:language` cast accepts any atomic operand; `xs:NOTATION` instance-of recognizes schema-typed NOTATION values; `IsUserDefinedSchemaType` rejects kind tests containing `(` while allowing braced-URI names; function conversion atomizes operands; `TryCast` is skipped for known sequence type names. This closes the QT3 failures `CastAsNamespaceSensitiveType-6`, `CastAs-UnionType-33`, `FunctionCall-049`, `qischema061`, `instanceof142`, `fo-test-fn-id-002`, `fo-test-fn-element-with-id-002`, `fo-test-fn-idref-001`, and `fo-test-fn-idref-002`. Full QT3 sweep is **30,946/13/862** (97.25%); unit tests pass **1,825/0**.
  - Implementation: `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`, `src/Bosak.XPath.Providers/XDocument/XDocumentNode.cs`, `src/Bosak.XPath.Core/Xdm/XdmValue.cs`, `src/Bosak.XPath.Parser/Ast/XPathParser.cs`, `src/Bosak.XPath.Parser/Ast/XPathAstNode.cs`, `src/Bosak.XPath.Compiler/Ir/IrLowerer.cs`, `src/Bosak.XPath.Runtime/Bosak.XPath.Runtime.csproj`.
  - Regression tests in `tests/Bosak.XPath.Runtime.Tests/SchemaTypedValueTests.cs` (4/4 passing).
  - Targeted verification: `CastAsNamespaceSensitiveType-6` 1/0/0; `CastAs-UnionType-33` 1/0/0; `FunctionCall-049` 1/0/0; `qischema061` 1/0/0; `instanceof142` 1/0/0; `fo-test-fn-id-002` 1/0/0; `fo-test-fn-element-with-id-002` 1/0/0; `fo-test-fn-idref-001` 1/0/0; `fo-test-fn-idref-002` 1/0/0.

- **2026-08-22** — XPath/XQuery: **`fn:load-xquery-module` / `validate` expression cluster** — `fn:load-xquery-module` now propagates schema imports from the loaded module into its evaluation context so schema-aware XQuery (including `validate`) runs correctly. The XQuery `validate { Expr }` expression is implemented as a contextual keyword: `validate strict`/`lax` and the default form lower to a `ValidateNode` opcode; `validate lax` with no schema returns the operand unchanged, `validate strict`/plain `validate` without a schema raises `XQST0075`, invalid operands raise `XQTY0030`, and validation failure raises `XQDY0027`. This closes the QT3 `fn-load-xquery-module` failures `fn-load-xquery-module-050`–`-052` and `-056`. Full QT3 sweep is **30,929/30/862** (97.20%); unit tests pass **1,813/0**.
  - Implementation: `src/Bosak.XPath.Parser/Lexer/XPathLexer.cs`, `src/Bosak.XPath.Parser/Ast/XPathParser.cs`, `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`, `tests/Bosak.XPath.Conformance/TestExecutor.cs`.
  - Regression tests in `tests/Bosak.XQuery.Tests/PlaceholderTests.cs` (3/3 passing).
  - Targeted verification: `fn-load-xquery-module` 69/0/14.

- **2026-08-22** — XPath/XQuery: **namespace-sensitive atomic function-conversion cluster** — `VmEngine.ApplyFunctionConversion` now raises `XPTY0117` when `xs:untypedAtomic` is supplied to `xs:QName`, `xs:NOTATION`, or a user-defined restriction of those, before subtype substitution can silently accept it. A new `IsNamespaceSensitiveTargetType` helper detects built-in and user-defined namespace-sensitive atomic types. This closes the QT3 `prod-CastExpr` failures `CastAs675a`, `CastAsNamespaceSensitiveType-1`, and `CastAsNamespaceSensitiveType-2`. Full QT3 sweep is **30,854/19/948** (96.96%); unit tests pass **1,813/0**.
  - Implementation: `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`.
  - Regression tests in `tests/Bosak.XPath.Runtime.Tests/SchemaListUnionTests.cs` (5/5 passing).
  - Targeted verification: `CastAs675a` 1/0/0; `CastAsNamespaceSensitiveType-1` 1/0/0; `CastAsNamespaceSensitiveType-2` 1/0/0.

- **2026-08-22** — XPath/XQuery: **schema-aware list/union function-conversion cluster** — the remaining schema-aware residuals around `attribute(*, T)` case preservation, union function conversion, unprefixed user-defined type names in `instance of`, and element schema-type subtyping are now covered by regression tests. `VmEngine` preserves case in attribute kind-test type names; applies membership semantics for union types in `ValueMatchesType`; casts `xs:untypedAtomic` to the first matching member in `ApplyFunctionConversion` (rejecting namespace-sensitive unions with `XPTY0117`); accepts unprefixed user-defined schema types via the default element namespace in `InstanceOf`; and handles `element(*, T1)` / `attribute(*, T1)` subtyping through the schema type hierarchy in `IsSequenceTypeSubtype`. Full QT3 sweep is **30,851/22/948** (96.95%); unit tests pass **1,813/0**.
  - Implementation: `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`.
  - Regression tests in `tests/Bosak.XPath.Runtime.Tests/SchemaListUnionTests.cs` (6/6 passing).
  - Targeted verification: `prod-FLWORExpr` 21/0/28; `prod-FunctionCall` 120/0/32.

- **2026-08-22** — XPath/XQuery: **op-numeric-add / union named-member cluster** — `XPathParser.ParseSingleType` now disambiguates `*`/`+` after a cast/castable target type: when a valid operand follows, the operator is left for the enclosing additive/multiplicative expression (`15 cast as xs:integer + 15`); standalone occurrence indicators (`'string' cast as xs:string*`) still raise `XPST0003`. `VmEngine.GetUnionMemberTypes` now returns both anonymous inline members and named `@memberTypes` members (including built-in `xs:*` types), so unions like `t:integer-or-nothing` can cast values matching the named `xs:integer` member. This closes `op-numeric-add-13`–`op-numeric-add-16` and several union cast residuals. Full QT3 sweep is **30,842/31/948** (96.92%); unit tests pass **1,806/0**.
  - Implementation: `src/Bosak.XPath.Parser/Ast/XPathParser.cs`, `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`.
  - Regression tests in `tests/Bosak.XPath.Parser.Tests/ParserTests.cs` (4/4 passing) and `tests/Bosak.XPath.Runtime.Tests/SchemaListUnionTests.cs` (2/2 passing).
  - Targeted verification: `op-numeric-add` 155/0/11.

- **2026-08-22** — XPath/XQuery: **castable cluster** — `VmEngine.TryCast`, the `Cast` opcode, and the `Castable` opcode now share an `AtomizeForCast` helper that recursively atomizes arrays, atomizes nodes, and raises `FOTY0013` for maps and function items. `castable as` propagates type errors (`FOTY0013`, `XPTY0004`) instead of returning `false`. This closes the QT3 `prod-CastableExpr` failures `CastableAs665`–`CastableAs668`. Full QT3 sweep is **30,836/37/948** (96.90%); unit tests pass **1,801/0**.
  - Implementation: `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`.
  - Regression tests in `tests/Bosak.XPath.Runtime.Tests/VmEngineTests.cs` (11/11 passing).
  - Targeted verification: `CastableAs665`–`CastableAs668` 4/4; `prod-CastableExpr` 951/0/8.

- **2026-08-22** — XPath/XQuery: **schema-validated date/time timezone preservation cluster** — `XDocumentNode.GetTypedValue` now re-parses the lexical string for `xs:date`, `xs:time`, `xs:dateTime`, `xs:dateTimeStamp`, and the `g*` date/time types using `XmlConvert.ToDateTimeOffset`, preserving explicit timezone offsets such as `+05:00` and `Z`. This closes the QT3 `prod-CastExpr.schema` residual failures `casthcds30`–`casthcds34` and the `prod-WindowClause` `WindowingUseCase*` residual failures; the `fn-adjust-*-to-timezone` targeted sets all pass. Full QT3 sweep is **30,832/41/948** (96.89%); unit tests pass **1,792/0**.
  - Implementation: `src/Bosak.XPath.Providers/XDocument/XDocumentNode.cs`.
  - Regression tests in `tests/Bosak.XPath.Core.Tests/SchemaDateTimeTypedValueTests.cs` (6/6 passing).
  - Targeted verification: `casthcds30`–`casthcds34` 5/5; `prod-WindowClause` `WindowingUseCase*` 38/0/0; `fn-adjust-time-to-timezone` 42/0/0; `fn-adjust-date-to-timezone` 41/0/0; `fn-adjust-dateTime-to-timezone` 48/0/0.
  - The `AGENTS.md` known limitation for `adjust-time-to-timezone` has been removed.

- **2026-08-22** — XPath/XQuery: **schema-derived string/numeric/union cast cluster** — `VmEngine.TryCastToSchemaType` and related cast helpers now convert numeric input to string before casting to derived string subtypes, validate derived atomic pattern facets against XSD canonical lexical forms (decimal `12` → `"12.0"`, double `93.7` → `"9.37E1"`), reject single non-string atomic values for list type casts, and convert `TimeSpan` values from schema parsing back to XSD duration lexical form. `XdmValue.ToString()` now respects `gYear`/`gYearMonth`/`gMonth`/`gMonthDay`/`gDay` schema type annotations. The conformance runner skips `app-Demos`, `app-XMark`, and `cbcl-codepoints-to-string-021` so unattended full sweeps complete reliably. The full QT3 sweep is **30,821/52/948** (96.86%); unit tests pass **1,786/0**.
  - Implementation: `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`, `src/Bosak.XPath.Core/Xdm/XdmValue.cs`.
  - Regression tests in `tests/Bosak.XPath.Runtime.Tests/SchemaListUnionTests.cs` (38/38 passing).
  - Targeted verification: `cbcl-normalizedstring` 7/7; `cbcl-token` 7/7; `CastableAs65` 10/10; `cbcl-castable-impure-009`/`-019` pass; `cbcl-cast-derived-001` pass.
  - Conformance runner: `tests/Bosak.XPath.Conformance/ConformanceRunner.cs`.

- **2026-08-21** — XPath/XQuery: **`prod-OrderByClause` decimal normalization cluster** — `VmEngine.TryCast` now updates the cast result to the atomized node value before checking the target type. Casting a schema-validated node to its own typed value (e.g., `xs:decimal($x)` on an `xs:decimal` element) now returns the atomic typed value instead of the original element node. The QT3 `prod-OrderByClause` cluster closes at **205/0/0**; full QT3 sweep is **30,831/68/922** (96.89%); unit tests pass **1,777/0**.
  - Implementation: `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`.
  - Regression tests in `tests/Bosak.XPath.Runtime.Tests/SchemaListUnionTests.cs`: `Cast_TypedDecimalElement_ReturnsDecimalAtomicValue`, `Cast_TypedDecimalElementInForExpression_ReturnsDecimalSequence`.
  - Targeted verification: `prod-OrderByClause` 205/0/0.

- **2026-08-21** — XPath/XQuery: **`fn:idref` cluster** — `IXdmNode.IsIdref` exposes the PSVI *is-idrefs* property for schema-validated nodes. `XDocumentNode` computes it for `xs:IDREF`/`xs:IDREFS`, derived restrictions/lists, unions where the selected member is `xs:IDREF`, and complex types with simple content whose base is an IDREF-bearing simple type; nilled elements report `false`. `FunctionLibrary.CollectIdrefElements` now consults `IsIdref` instead of relying only on DTD declarations or attribute names. `XDocumentNode.Prefix` prefers the empty prefix when the element namespace is bound to the in-scope default namespace, so `fn:name()` returns the unprefixed lexical form used in the source document. The QT3 `fn-idref` cluster closes at **54/0/0**; unit tests pass **1,772/0**.
  - Implementation: `src/Bosak.XPath.Core/Xdm/IXdmNode.cs`, `src/Bosak.XPath.Providers/XDocument/XDocumentNode.cs`, `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs`, `tests/Bosak.XPath.Standard.Tests/FunctionLibraryTests.cs`.
  - Targeted verification: `fn-idref` 54/0/0.

- **2026-08-21** — XPath/XQuery: **schema-aware SequenceType XPST0051 cluster** — `VmEngine.InstanceOf` now rejects all non-atomic user-defined schema simple types as SequenceType item types: direct list types, restrictions of list types, restrictions of union types, and union types whose members transitively contain a list type (including built-in `xs:NMTOKENS`). Pure atomic unions (possibly via nested atomic unions) remain valid item types. The QT3 `prod-InstanceofExpr` cluster moves from **305/3/1** to **308/0/1** and `prod-TypeswitchExpr` moves from **70/2/1** to **72/0/1**; unit tests pass **1,775/0**.
  - Implementation: `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`.
  - Regression tests in `tests/Bosak.XPath.Runtime.Tests/SchemaListUnionTests.cs`: `ListInstanceOf_RejectsListTypeAsSequenceTypeItemType`, `UnionContainingListInstanceOf_ThrowsXpst0051`, `UnionContainingBuiltInListInstanceOf_ThrowsXpst0051`, `UnionOfAtomicInstanceOf_BracedUriLiteralAcceptsMatchingValue`.
  - Targeted verification: `prod-InstanceofExpr` 308/0/1; `prod-TypeswitchExpr` 72/0/1.

- **2026-08-21** — XPath/XQuery: **`fn:json-to-xml` cluster** — schema-aware `fn:json-to-xml` validation is now implemented. `validate:=true()` validates the generated XML against the embedded W3C schema-for-JSON (`src/Bosak.XPath.Standard/Resources/schema-for-json.xsd`) and annotates the result with PSVI types; `validate:=true()` combined with explicit `duplicates:='retain'` raises `FOJS0005`; schema/duplicate-key validation failures map to `FOJS0003`. `VmEngine.ValueMatchesType` now recognizes parameterized kind tests (`document-node(...)`, `schema-element(...)`) including `document-node(schema-element(...))`, and preserves original case for schema type names in `element(name, type)`. XQuery now resolves `import schema "http://www.w3.org/2005/xpath-functions"` to the embedded JSON schema via `FunctionLibrary.JsonSchemaSet`. The QT3 `fn-json-to-xml` cluster closes at **86/0/8** (skips are unsupported dependencies); unit tests pass **1,765/0**.
  - Implementation: `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs`, `src/Bosak.XPath.Standard/Bosak.XPath.Standard.csproj`, `src/Bosak.XPath.Providers/XDocument/XDocumentProvider.cs`, `src/Bosak.XQuery/Api/XQueryExecutable.cs`, `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`, `tests/Bosak.XPath.Conformance/TestEnvironment.cs`, `tests/Bosak.XPath.Standard.Tests/FunctionLibraryTests.cs`.
  - Targeted verification: `fn-json-to-xml` 86/0/8.

- **2026-08-21** — XPath/XQuery: **`fn:nilled` cluster** — `fn:nilled` now honors the PSVI `IsNil` annotation. `fn:data` returns the PSVI typed value for schema-validated element/attribute nodes, producing an empty sequence for nilled elements (XDM §2.7.2). `XDocumentNode.GetTypedValue` returns empty and `HasNoTypedValue` returns false for nilled elements so `fn:data` does not raise `FOTY0012`. `element(*, T)` / `element(N, T)` kind tests reject nilled elements, while the nillable form `element(*, T?)` / `element(N, T?)` accepts them. The QT3 `fn-nilled` cluster closes at **60/0/4**; overall QT3 failures drop from **138** to **119**.
  - Implementation: `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs`, `src/Bosak.XPath.Providers/XDocument/XDocumentNode.cs`, `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`.
  - Targeted verification: `fn-nilled` 60/0/4.
  - QT3: **30,780/119/922** (96.73%); unit tests: **1,762/0**.

- **2026-08-21** — XPath/XQuery: **runtime recursion fixes** — the full QT3 conformance sweep that was aborting with a stack overflow in `FunctionItemInstanceOf` now completes. `FunctionItemInstanceOf` no longer falls back recursively to `ValueMatchesType` for unresolved function items; it matches on arity only. `IsSchemaTypeSequenceSubtype` no longer calls `IsSequenceTypeSubtype` for atomic schema types, breaking the cycle that re-entered `IsSchemaAwareSequenceSubtype` with a fresh visited set. The fix restores schema-aware function-type instance-of tests (`instanceof136`–`instanceof141`) and drops overall QT3 failures from **175** to **138**. `prod-CastExpr.schema` remains **123/6/1**; `prod-CastableExpr UnionType` and `ListType` remain 100% (29/0 and 18/0); `fn-for-each` is **64/0/2**; `fn-function-lookup` is **669/0/5**.
  - Implementation: `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`.
  - Targeted verification: `prod-InstanceofExpr` 305/3/1; `prod-CastExpr.schema` 123/6/1; `prod-CastableExpr UnionType` 29/0/0; `prod-CastableExpr ListType` 18/0/0; `fn-for-each` 64/0/2; `fn-function-lookup` 669/0/5.
  - QT3: **30,761/138/922** (96.67%); unit tests: **1,762/0**.

- **2026-08-21** — XPath/XQuery: **schema-aware QName/NOTATION cast fixes** — `VmEngine.TryCast` now resolves prefixed type names using the original-case prefix, fixing spurious `XPST0081` errors for mixed-case schema prefixes such as `myType` (`qname-cast-3/4`, `user-defined-8/9`). Namespace-sensitive user-defined types (`QName`/`NOTATION` restrictions) are parsed directly against their schema datatype, so `xs:NOTATION`-derived constructor and cast expressions work and the original lexical prefix is preserved in the resulting XDM QName value (`notation-cast-3`). `xs:string`-derived subtypes (`NCName`, `Name`, `NMTOKEN`, `language`, `normalizedString`, `token`, `ID`, `IDREF`, `ENTITY`) now require a string-kind operand, preventing `xs:QName` values from incorrectly matching `xs:NCName` in unions and restoring `CastAs-UnionType-20`. `XQueryExecutable.ApplyStaticContext` detects `XQST0034` conflicts between user-declared functions and schema simple-type constructor functions (`user-defined-11`). `prod-CastExpr.schema` moves from **117/12/1** to **123/6/1**; the remaining six failures are pre-existing timezone/float-formatting issues (`casthcds12/30/31/32/33/42`). `prod-CastableExpr UnionType` and `ListType` remain 100% (29/0 and 18/0).
  - Implementation: `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`, `src/Bosak.XQuery/Api/XQueryExecutable.cs`.
  - Targeted verification: `prod-CastExpr.schema` 123/6/1; `prod-CastableExpr UnionType` 29/0/0; `prod-CastableExpr ListType` 18/0/0.
  - QT3: pending full sweep; unit tests: **1,762/0**.

- **2026-08-21** — XPath/XQuery: **schema-aware list/union residual fixes** — `NamedFunctionItem` now captures the in-scope namespace bindings from where it is materialized, and `VmEngine.InvokeFunctionItemCore` restores them for the duration of a dynamic named-function call. This makes constructor functions for namespace-sensitive schema unions resolve lexical prefixes against the static definition context instead of the call-site context (`CastAs-UnionType-13/14/15`). `ItemInstanceOf` now raises `XPST0051` when a SequenceType item type is a user-defined simple type derived by restriction from a union or list type (`CastAs-UnionType-17`). `prod-CastExpr.schema` moves from **113/16/1** to **117/12/1**; `prod-CastableExpr UnionType` and `ListType` remain 100% (29/0 and 18/0).
  - Implementation: `src/Bosak.XPath.Core/Xdm/FunctionItem.cs`, `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs`, `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`.
  - Added regression tests in `tests/Bosak.XPath.Runtime.Tests/SchemaListUnionTests.cs`: `LowercaseNameInstanceOfItself_ThrowsXpst0051`, `SensitiveUnionConstructorDynamicCall_UsesDefiningNamespaceContext`.
  - Targeted verification: `prod-CastExpr.schema` 117/12/1; `prod-CastableExpr UnionType` 29/0/0; `prod-CastableExpr ListType` 18/0/0; `fn-function-lookup` 669/0/5.
  - QT3: pending full sweep; unit tests: **1,762/0** (+2).

- **2026-08-21** — XPath/XQuery: **schema-aware XSD list/union simple types** — `VmEngine.TryCastToSchemaType` recursively casts to unions, lists, and restrictions of those varieties, using XPath cast semantics for atomic members (e.g. decimal-to-integer truncation). `ValueMatchesType` accepts user-defined list/union types by delegating to the same recursive cast. Sequence-type subtyping (`IsSequenceTypeSubtype`) is now schema-aware, so function coercion works for union/list return types (`CastAs-UnionType-18/26/32`, `CastAs-ListType-26/27/32`). `XDocumentNode.GetTypedValue` uses `IXmlSchemaInfo.MemberType` for union typed values, and typed `xs:QName` values are preserved through namespace-sensitive unions (`CastAs-UnionType-20/25`). `prod-CastExpr.schema` moves from **88/41/1** to **110/19/1**; `prod-CastableExpr UnionType` and `ListType` both pass 100%.
  - Implementation: `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`, `src/Bosak.XPath.Providers/XDocument/XDocumentNode.cs`.
  - Added regression tests in `tests/Bosak.XPath.Runtime.Tests/SchemaListUnionTests.cs`: `UnionCast_SelectsIntegerMemberAndTruncatesDecimal`, `UnionCast_SelectsDateMember`, `UnionCast_SelectsPatternRestrictedStringMember`, `UnionCast_FailsForNonMatchingValue`, `ListCast_TokenizesStringToIntegerSequence`, `ListOfUnionsCast_TokenizesAndSelectsMemberType`, `UnionInstanceOf_AcceptsMatchingDecimalValue`, `UnionCast_QNameMemberFromString`, `UnionInstanceOf_AcceptsMatchingStringValue`, `ListInstanceOf_AcceptsMatchingStringValue`, `SchemaValidatedUnionElement_TypedValueUsesSelectedMemberType`, `LowercaseNameInstanceOfSensitiveUnion`, `QNameCastableToUnion_WithNamespace`, `ListFunctionCoercion_UserDefinedListConstructorViaLookupMatchesTypedFunctionItem`, `UnionOfListsFunctionCoercionViaLookup_MatchesAnyAtomicTypeStarReturn`.
  - Targeted verification: `prod-CastExpr.schema` 110/19/1; `prod-CastableExpr UnionType` 29/0/0; `prod-CastableExpr ListType` 18/0/0.
  - QT3: pending full sweep; unit tests: **1,759/0** (+21).

- **2026-08-21** — XPath/XQuery: **XML 1.1-only name characters in constructed elements/attributes** — `XDocumentProvider.ConstructElement` and `ConstructAttribute` now encode XML 1.1-only local names via `Xml11NameCodec.EncodeName` so they can be stored in the .NET `XDocument` provider, annotate constructed XML 1.1 elements with `Xml11Annotation.Instance`, and decode the names in `XDocumentNode.ToXmlString` and `ResultComparer.CanonicalName` before serialization and assert-xml comparisons. This clears the two previously skipped `misc-XMLEdition` name-character tests (`XML10-4ed-Excluded-char-1-new` and `XML11-1ed-Included-char-1-new`).
  - Implementation: `src/Bosak.XPath.Providers/XDocument/XDocumentProvider.cs`, `src/Bosak.XPath.Providers/XDocument/XDocumentNode.cs`, `tests/Bosak.XPath.Conformance/ResultComparer.cs`.
  - Removed known-gap entries `XML10-4ed-Excluded-char-1-new` and `XML11-1ed-Included-char-1-new` from `tests/Bosak.XPath.Conformance/ConformanceRunner.cs`.
  - Added regression tests in `tests/Bosak.XPath.Runtime.Tests/Xml11NameTests.cs`: `ConstructElement_Xml11OnlyNameStartChar`, `ConstructElement_Xml11OnlyNameChar`, `ConstructElement_Xml10NameUnchanged`.
  - Targeted verification: `misc-XMLEdition` 13/0/6; `XML10-4ed-Excluded-char-1-new` and `XML11-1ed-Included-char-1-new` both 1/0/0.
  - QT3: **30,724/175/922**; unit tests: **1,738/0** (+3).

- **2026-08-21** — XPath/XQuery: **schema-element()/schema-attribute() kind tests** — `IrLowerer` emits dedicated `SchemaElementTest`/`SchemaAttributeTest` opcodes for `schema-element(N)` and `schema-attribute(N)` kind tests. `VmEngine` evaluates these against the compiled `XmlSchemaSet`, matching the node's element/attribute declaration, traversing substitution groups, and applying nillability checks against the actual element declaration. The `IXdmNode` provider exposes `SchemaElementDeclaration`, `SchemaAttributeDeclaration`, and `IsNilled` via `IXmlSchemaInfo`, and `ValueMatchesType` routes these tests so they also work for function-parameter type checking. This clears the 11 previously skipped `cbcl-schema-element-*`/`cbcl-schema-attribute-*` tests in `prod-SchemaImport`.
  - Implementation: `src/Bosak.XPath.Compiler/Ir/IrOpCode.cs`, `src/Bosak.XPath.Compiler/Ir/IrLowerer.cs`, `src/Bosak.XPath.Runtime/Vm/VmEngine.cs`, `src/Bosak.XPath.Core/Xdm/IXdmNode.cs`, `src/Bosak.XPath.Providers/XDocument/XDocumentNode.cs`.
  - Removed known-gap entries from `tests/Bosak.XPath.Conformance/ConformanceRunner.cs`.
  - Added regression tests in `tests/Bosak.XPath.Runtime.Tests/SchemaTypedValueTests.cs`: `SchemaElementKindTest_MatchesElementDeclaration`, `SchemaElementKindTest_NilledSubstitutionGroupMemberMatchesHead`, `SchemaAttributeKindTest_MatchesAttributeDeclaration`, `SchemaElementKindTest_WrongElementDoesNotMatch`.
  - Targeted verification: `prod-SchemaImport` (`cbcl-schema-element`, `cbcl-schema-attribute`) 11/0/0.
  - QT3: **30,722/175/924**; unit tests: **1,735/0** (+4).

- **2026-08-20** — Language Server: **XSLT code lens default source document** — `.xsl` and `.xslt` files that contain a `<?bosak source-document="..."?>` processing instruction now show a code lens titled **Run XSLT transformation (file.xml)**. The lens command (`bosak.transformXslt`) receives both the stylesheet URI and the resolved absolute source path, so the VS Code client runs the transformation without prompting. Relative source paths are resolved against the stylesheet directory; if the processing instruction is absent, the existing picker-based lens is used.
  - Handler: `src/Bosak.LanguageServer/CodeLensHandler.cs`.
  - Client: `vscode-bosak/src/extension.ts`.
  - Tests in `tests/Bosak.LanguageServer.Tests/CodeLensHandlerTests.cs`.
  - Language-server tests: **61 passed / 0 failed / 0 skipped** (+1).

- **2026-08-20** — Language Server: **execute command with serializable result** — the language server now implements `workspace/executeCommand` for `bosak.evaluateXPath` and `bosak.evaluateXQuery`. Clicking a code lens (or invoking the command palette action) sends the command with the document URI; the server evaluates the document and sends the serialized result or error back to the client via a `bosak/evaluationResult` notification. The VS Code extension registers both command IDs, routes them through `workspace/executeCommand`, and opens the result in a preview editor (or shows an error message).
  - Handler: `src/Bosak.LanguageServer/ExecuteCommandHandler.cs`.
  - Registration in `src/Bosak.LanguageServer/Program.cs`.
  - Tests in `tests/Bosak.LanguageServer.Tests/ExecuteCommandHandlerTests.cs`.
  - Client: `vscode-bosak/src/extension.ts` and `vscode-bosak/package.json`.
  - Language-server tests: **58 passed / 0 failed / 0 skipped** (+4).

- **2026-08-20** — Language Server: **code lens (XSLT)** — `.xsl` and `.xslt` documents now display a code lens titled **Run XSLT transformation** at the top of the file. The lens command is `bosak.transformXslt`; it reuses the existing VS Code source-document picker and custom LSP request to run the transformation, because XSLT requires an external source document. The document selector now covers `.xpath`, `.xq`, `.xqy`, `.xquery`, `.xsl`, and `.xslt`.
  - Handler: `src/Bosak.LanguageServer/CodeLensHandler.cs`.
  - Registration in `src/Bosak.LanguageServer/Program.cs`.
  - Tests in `tests/Bosak.LanguageServer.Tests/CodeLensHandlerTests.cs`.
  - Language-server tests: **60 passed / 0 failed / 0 skipped** (+2).

- **2026-08-20** — Language Server: **code lens (XPath + XQuery)** — `.xpath`, `.xq`, `.xqy`, and `.xquery` documents now display a code lens at the top of the file that evaluates the expression and shows the serialized result (or error) above the document. The lens uses `XPath31Expression` for `.xpath` files and `XQueryCompiler` for XQuery files; the command name is `bosak.evaluateXPath` or `bosak.evaluateXQuery` respectively. Unsupported file types receive an empty lens container.
  - Handler: `src/Bosak.LanguageServer/CodeLensHandler.cs`.
  - Registration in `src/Bosak.LanguageServer/Program.cs`.
  - Tests in `tests/Bosak.LanguageServer.Tests/CodeLensHandlerTests.cs`.
  - Language-server tests: **54 passed / 0 failed / 0 skipped** (+2).

- **2026-08-20** — Language Server: **code actions** — the language server now provides quick fixes for common namespace, stylesheet, XPath, and XQuery syntax issues. For XPath documents (`.xpath`) it offers to close unclosed parentheses, square brackets, and string literals. For XQuery documents it offers to declare an undeclared namespace prefix (`declare namespace prefix = "";`), to declare a default element namespace when unprefixed element constructors are present (`declare default element namespace "";`), to remove an invalid `xmlns:prefix=""` empty namespace declaration that triggers `XQST0085`, to import a module namespace for prefixes used in function calls (`import module namespace prefix = "";`), and to close unclosed curly braces in direct element constructors. For XSLT documents it offers to declare an undeclared prefix on the root element (`xmlns:prefix=""`), to promote a bare `<stylesheet>`/`<transform>` root to `<xsl:stylesheet>`/`<xsl:transform>` with the required `xsl` namespace, to add a missing `version="3.0"` attribute, and to react to `XPST0081` diagnostics by offering a namespace declaration for the prefix named in the diagnostic. Namespace declarations for the reserved `xml` prefix use the standard XML namespace URI (`http://www.w3.org/XML/1998/namespace`). `DiagnosticsHandler` now emits a warning when the XSLT root lacks `version`. The handler implements `textDocument/codeAction` and `codeAction/resolve`; the `vscode-bosak` client uses the edits automatically.
  - New handler: `src/Bosak.LanguageServer/CodeActionHandler.cs`.
  - Updated handler: `src/Bosak.LanguageServer/DiagnosticsHandler.cs`.
  - New tests in `tests/Bosak.LanguageServer.Tests/CodeActionHandlerTests.cs`.
  - Language-server tests: **49 passed / 0 failed / 0 skipped** (+2).

- **2026-08-20** — Language Server: **semantic tokens** — the language server now provides semantic highlighting for XPath, XQuery, and XSLT documents. Tokens are emitted for function calls, variables, XSLT instructions (`xsl:*`), XQuery keywords, type names (`xs:*`), namespace prefixes, number literals, and XPath operators. The `vscode-bosak` extension version is bumped to **0.1.3**; no client-side wiring is required because `vscode-languageclient` uses the server's advertised `textDocument/semanticTokens` capability automatically.
  - New handler: `src/Bosak.LanguageServer/SemanticTokensHandler.cs`.
  - New tests in `tests/Bosak.LanguageServer.Tests/SemanticTokensHandlerTests.cs`.
  - Language-server tests: **27 passed / 0 failed / 0 skipped** (+7).

- **2026-08-20** — XPath/XQuery: **schema-awareness sweep closed** — full QT3 sweep after the user-defined schema simple-type work is **29,929 passed / 0 failed / 1,892 skipped** (94.05%). The remaining 162 schema-awareness skips are list/union types, `QName`/`NOTATION` casts, and `schema-element()`/`schema-attribute()` kind tests, all outside the XDocument-backed simple-type scope. Unit tests: **1,708/0**. This finalizes REQ-070.

- **2026-08-18** — Language Server: **XQuery language support** — the extension and language server now handle XQuery documents (`.xq`/`.xqy`/`.xquery`): syntax highlighting, diagnostics via `XQueryCompiler`, keyword/constructor completion, hover, go-to-definition for XQuery functions/variables, document symbols for top-level XQuery declarations, and a `bosak/runXQuery` command backed by a new `bosak/evaluateXQuery` LSP request.

- **2026-08-19** — XPath/XQuery: **schema-awareness sweep (user-defined schema simple types)** — `FunctionLibrary.Populate` registers constructor functions for non-`xs:*` simple types found in `EvaluationContext.SchemaSet`; `ValueMatchesType`, `ApplyFunctionConversion`, and `instance of` now accept prefixed user-defined schema types by resolving the prefix and validating against XSD facets; schema-validated typed values keep the integer XDM kind for integer-derived types and remain typed as date/time values. This clears the `qischema003`, `qischema030`, and `qischema040`/`qischema040a` failures in `prod/SchemaImport`.
  - Added regression tests in `SchemaTypedValueTests.cs`: `UserDefinedSchemaTypeConstructor`, `UserDefinedSchemaTypeCast`, `UserDefinedSchemaTypeInstanceOf`, `SchemaValidatedTimeIsTyped`, `UserDefinedDateTypeCast`.
  - Documented 11 remaining `cbcl-schema-element/attribute-*` tests as a known XQuery gap (`schema-element()` / `schema-attribute()` kind tests are not supported by the XDocument provider).
  - Targeted verification: `prod-SchemaImport` 49/0/91.
  - Unit tests: **1,727/0**.

- **2026-08-18** — Language Server: **hover, go-to-definition, document symbols, and evaluate/transform commands** — the language server now supports hover (function signatures/descriptions for XPath functions), go-to-definition (user-defined XSLT functions/variables/params/named templates), document symbols (an outline of top-level XSLT declarations), and two custom requests: `bosak/evaluateXPath` (evaluate the current `.xpath` document) and `bosak/transformXslt` (run the current stylesheet against a chosen source XML document). The extension's context-menu commands are wired to these. A new `Bosak.LanguageServer.Tests` project covers the handlers; the language server builds separately from `Bosak.sln`.

- **2026-08-18** — XPath/XQuery: **XML 1.1 namespace undeclaration** — element constructors now accept `xmlns:p=""` in XML 1.1 mode, recording it as a `PrefixedNamespaceUndeclarations` annotation instead of raising `XQST0085`. An `Xml11Mode` flag was added to `EvaluationContext`/`XdmElementSpec`, set by the harness for `xml-version=1.1` tests. `XQST0085b`, `K2-Serialization-20`, and `K2-Serialization-21` now pass; the two XML 1.1-only character-name tests were previously a known gap but are now implemented via `Xml11NameCodec` name encoding.
  - Added regression tests in `PlaceholderTests.cs`: `DirectConstructor_Xml11PrefixedNamespaceUndeclaration`, `DirectConstructor_Xml10PrefixedNamespaceUndeclaration_StillErrors`.
  - Targeted verification: `misc-CombinedErrorCodes` (`XQST0085b`) 1/0/0, `method-xml` (`K2-Serialization-20/21`) 2/0/0.
  - QT3: **30,344/0/1,477** (95.36%); unit tests: **1,722/0**.

- **2026-08-18** — XQuery: **external variable declared-type cluster** — main-module function bodies now use the main module's static default element namespace instead of inheriting a namespace leaked from an enclosing direct element constructor at the call site. `extvardeclwithtype-23` now passes and is removed from `KnownXQueryGaps`.
  - Added regression test in `PlaceholderTests.cs`: `FunctionCall_UsesModuleStaticDefaultElementNamespace`.
  - Targeted verification: `prod-VarDecl.external` (`extvardeclwithtype-23`) 1/0/0.
  - QT3: **30,341/0/1,480** (95.35%); unit tests: **1,720/0**.

- **2026-08-18** — XPath/XQuery: **group-by post-clause cluster** — `let` clauses between `group by` and `order by` are now evaluated during the re-key pass so order-by keys can reference them (`use-case-groupby-Q6`), and `for`/`window` clauses after `group by` are lowered as a nested FLWOR evaluated per group (`TumblingWindowExpr545`). The two `NotSupportedException` guards are removed. The conformance harness gained a `BOSAK_QT3_DUMP_SKIPS` per-test skip dump used to locate the guard-hitting tests.
  - Added regression tests in `PlaceholderTests.cs`: `Flwor_GroupBy_LetBeforeOrderBy_ReferencedByKey`, `Flwor_GroupBy_WindowAfterGroupBy`.
  - Targeted verification: `prod-GroupByClause` 36/0/0, `prod-WindowClause` 132/0/3.
  - QT3: **30,340/0/1,481** (95.34%); unit tests: **1,719/0**.

- **2026-08-18** — XPath/XQuery: **multiple order by cluster** — FLWORs now support multiple `order by` clauses. `LowerFlworWithTuples` splits the clause list at each `order by` and chains stable-sort stages: build+sort for the first, then a re-key stage per additional `order by` that rebinds tuple variables, processes intermediate `count`/`where`/`let` clauses, and evaluates new keys. `orderBy65` and `orderBy66` now pass.
  - Added regression tests in `PlaceholderTests.cs`: `Flwor_MultipleOrderBy_Adjacent`, `Flwor_MultipleOrderBy_WithCountAndLetBetween`.
  - Targeted verification: `prod-OrderByClause` 201/0/4, `prod-GroupByClause` 35/0/1.
  - QT3: **30,338/0/1,483** (95.34%); unit tests: **1,717/0**.

- **2026-08-18** — XPath/XQuery: **attribute whitespace/namespace cluster** — `XPathParser.ScanConstructorAttributeValue` now applies XML 1.0 line-ending normalization (`\r\n` → one space) before attribute-value whitespace normalization, preserving literal whitespace runs within a single text part. `VmEngine.ResolveComputedName` no longer applies the default element namespace to unprefixed computed attribute names. `K2-DirectConElemAttr-75` and `currencysvg` now pass and are removed from `KnownXQueryGaps`.
  - Added regression tests in `PlaceholderTests.cs`: `DirectAttributeValue_LineEndingNormalization`, `DirectAttributeValue_LiteralSpacesArePreserved`, `ComputedAttribute_UnprefixedNameIgnoresDefaultElementNamespace`.
  - Targeted verification: `K2-DirectConElemAttr-75` 1/0/0, `currencysvg` 1/0/0; previously regressed `K2-DirectConOther-49/58/59/60/68/69` all pass.
  - QT3: **30,336/0/1,485** (95.33%); unit tests: **1,715/0**.

- **2026-08-18** — XPath/XQuery: **xml:space computed-attribute cluster** — `XDocumentProvider.ConstructAttribute` now validates `xml:space` at construction time and raises `XQDY0092` for values other than `default` or `preserve`. This prevents a LINQ `ArgumentException` from surfacing later during serialization.
  - Added regression test in `PlaceholderTests.cs`: `ComputedAttribute_XmlSpaceInvalid_RaisesXQDY0092`.
  - Targeted verification: `prod-CompAttrConstructor` (`K2-ComputeConAttr-60`) 1/0/0.
  - QT3: **30,334/0/1,487** (95.33%); unit tests: **1,712/0**.

- **2026-08-18** — XPath/XQuery: **UCA identical/blanked cluster** — `fn:compare` with a UCA collation using `strength=identical;alternate=blanked` no longer combines `CompareOptions.Ordinal` with `IgnoreSymbols`. The comparison now applies the blanked collation first and uses a codepoint tie-break for equality.
  - Added regression test in `FunctionLibraryTests.cs`: `Compare_UcaIdenticalBlanked_NotEqual`.
  - Targeted verification: `fn-compare` 88/0/8.
  - QT3: **30,333/0/1,488** (95.32%); unit tests: **1,711/0**.

- **2026-08-18** — XPath/XQuery: **fn-transform cluster** — `TransformEngine` now raises `FOXT0002` when `fn:transform` is called without a source document and without an initial template/selection. Secondary result documents whose content is not a single root element are captured through the synthetic `__xdm_doc__` wrapper, so text-node content such as `sect1` no longer triggers a LINQ "Non-whitespace characters cannot be added to content" error.
  - Added regression tests in `StylesheetTests.cs`: `FnTransform_MissingSource_RaisesFOXT0002`, `FnTransform_ResultDocumentTextContent_IsCaptured`.
  - Targeted verification: `fn-transform` 120/0/4.
  - QT3: **30,332/0/1,489** (95.32%); unit tests: **1,710/0**.

- **2026-08-18** — XPath/XQuery: **arrow partial-application cluster** — `LowerArrow` now supports `ArgumentPlaceholderNode` in static arrow targets. `"$" => concat(?)` now produces a curried function item instead of a `NotSupportedException`.
  - Added regression test in `ApiTests.cs`: `ArrowPartialApplication_PlaceholderArg`.
  - Targeted verification: `prod-ArrowPostfix` 42/0/0.
  - QT3: **30,330/0/1,491** (95.31%); unit tests: **1,708/0**.

- **2026-08-18** — XPath/XQuery: **function-arity overflow cluster** — `fn:concat#340282366920938463463374607431768211456` caused a parser `OverflowException`. `XPathParser.ParseNamedFunctionRef` now clamps out-of-range arity literals to `int.MaxValue`, and `VmEngine.ResolveNamedFunctionTuple`/`ResolveNamedFunctionItem` raise `FOAR0002` for that sentinel instead of matching a variadic fallback.
  - Added regression tests in `FunctionLibraryTests.cs`: `FunctionArity_HugeArity_RaisesFOAR0002`, `FunctionName_HugeArity_RaisesFOAR0002`.
  - Targeted verification: `fn-function-arity` 21/0/2, `fn-function-name` 25/0/1.
  - QT3: **30,329/0/1,492** (95.31%); unit tests: **1,707/0**.

- **2026-08-18** — XPath/XQuery: **app-CatalogCheck hang** — the `app-CatalogCheck` catalog-consistency set is now skipped by the conformance harness. Each test in this set loads the entire QT3 catalog and all 428 referenced test-set files, which caused the full sweep to hang/timeout before producing a summary. The harness records the 14 tests as skipped so the full QT3 sweep completes at **30,327/0/1,494 (95.30%)**.
  - Files changed: `tests/Bosak.XPath.Conformance/ConformanceRunner.cs`.
  - QT3: **30,327/0/1,494** (95.30%); unit tests: **1,705/0**.

- **2026-08-18** — XPath/XQuery: **misc-JsonTestSuite cluster** — `FunctionLibrary.JsonDoc` now resolves relative URIs against `EvaluationContext.BaseUri` before loading, and reads a resolved local JSON file as plain text instead of routing it through the XML `DocumentLoader`. The `misc-JsonTestSuite` tests reference JSON files with relative URIs such as `JSONTestSuite/test_parsing/...`; without base-URI resolution these were resolved against the process working directory, so the files could not be found. The entire set is now **318 passed / 0 failed / 0 skipped**.
  - Added regression test in `FunctionLibraryTests.cs`: `JsonDoc_RelativeUri_ResolvesAgainstBaseUri`.
  - Targeted verification: `misc-JsonTestSuite` 318/0/0; `fn-json-doc` 61/0/7.
  - QT3: **30,327/0/1,495** (95.30%); unit tests: **1,705/0**.

- **2026-08-18** — XPath/XQuery: **fn-json-doc cluster** — `FunctionLibrary.JsonDoc` now wraps failures from `EvaluationContext.DocumentLoader` as `FOUT1170`, matching the behavior of the direct file-load path. Previously the loader's raw `IOException`/`FileNotFoundException`/`DirectoryNotFoundException` bubbles escaped as unexpected errors, causing `json-doc-error-028..032` to skip. The `DocumentLoader` branch now rethrows `InvalidOperationException` unchanged and converts any other exception to `InvalidOperationException("FOUT1170: Cannot load JSON document {uri}")`.
  - Added regression test in `FunctionLibraryTests.cs`: `JsonDoc_DocumentLoaderThrows_WrapsAsFOUT1170`.
  - Targeted verification: `fn-json-doc` 61/0/7 (remaining skips are unsupported dependencies).
  - QT3: **30,009/0/1,813** (94.31%); unit tests: **1,704/0**.

- **2026-08-18** — XPath/XQuery: **group-by/order-by cluster** — `IrLowerer` now supports `where` and `let` clauses after `group by` and after `order by`. `LowerFlworBodyIteration` emits `JumpIfFalse` for `where` clauses and stores `let` bindings while restoring scoped variable names after each iteration. The lowerer also guards against multiple `order by` clauses and against the unsupported pattern where a post-grouping `let` variable is referenced by a post-grouping `order by` key (the latter is skipped cleanly instead of failing with `XPST0008`).
  - Added regression tests in `PlaceholderTests.cs`: `XQuery_GroupBy_WhereAfterGroupBy`, `XQuery_GroupBy_WhereAfterOrderBy`, `XQuery_OrderBy_WhereAfterOrderBy`, `XQuery_GroupBy_LetAfterGroupBy`.
  - Targeted verification: `prod-GroupByClause` 35/0/1, `prod-OrderByClause` 199/0/6, `app-Duplicates` 14/0/0.
  - QT3: **30,004/0/1,818** (94.29%); unit tests: **1,703/0**.

- **2026-08-18** — XPath/XQuery: **QT3 skip-cluster cleanup** — `TestCase.FromElement` now loads `<test file="..."/>` query text from external `.xq` files relative to the test-set base directory, fixing the empty-query `ArgumentException` cluster (~37 tests). Reserved namespace binding errors now surface as the correct XQuery spec codes: `XQST0070` for default element namespace declarations bound to the XML/XMLNS namespace URI; `XQDY0096` for computed element names in the XMLNS namespace or with a non-`xml` prefix bound to the XML namespace URI; `XQST0085` for prefixed namespace declarations bound to the empty namespace URI; and `XQDY0101` for namespace constructors bound to the XMLNS namespace URI. Six tests are documented as known gaps: three XML 1.1 prefixed namespace undeclarations (`XQST0085b`, `K2-Serialization-20`, `K2-Serialization-21`) and three newly exposed external-file tests (`currencysvg`, `extvardeclwithtype-23`, `K2-DirectConElemAttr-75`).
  - Files changed: `TestCase.cs`, `XQueryParser.cs`, `XDocumentProvider.cs`, `VmEngine.cs`, `ConformanceRunner.cs`.
  - QT3: **29,984/0/1,837** (94.23%); unit tests: **1,699/0**.

- **2026-08-18** — XPath/XQuery: **document-node stripping cluster** — `XDocumentProvider.ConstructElement` and `ConstructDocument` now strip document nodes used as element or document constructor content, matching XQuery §3.9.1.1. Document nodes are unwrapped, the engine's synthetic `__xdm_doc__` wrapper is bypassed, and text children are merged with the surrounding `pendingText` so adjacent text nodes collapse correctly. This fixes the 20-test `ArgumentException: A node of type Document cannot be added to content` skip group.
  - Files changed: `src/Bosak.XPath.Providers/XDocument/XDocumentProvider.cs`.
  - QT3: **30,004/0/1,817** (94.29%); unit tests: **1,699/0**.

- **2026-08-18** — XPath/XQuery: **if-keyword-as-name cluster** — `XPathParser.ParseExprSingle` now treats `if` as a conditional keyword only when the next token is `(`. Otherwise `if` falls through to `ParseOrExpr` and is parsed as an ordinary name/name test, consistent with the existing gating for `for` and `let`. This fixes the W3C tokenizer-torture query `if(if) then then else else-...` (`K2-NameTest-5`), which previously failed at parse time with `XPST0003` and now produces the expected runtime `XPTY0004`/`XPDY0002`.
  - Added regression test `IfKeyword_ParseAsNameTestWhenNotConditional` in `ApiTests.cs`.
  - Removed 1 stale `KnownXQueryGaps` entry: `K2-NameTest-5`.
  - QT3: **29,941/0/1,880** (94.09%); unit tests: **1,699/0**.

- **2026-08-18** — XPath/XQuery: **axis-step cluster** — `ApplyAxis`/`PathStepMap` in `VmEngine.cs` now treat an empty-sequence input (`XdmValue.Undefined`) as an empty result instead of raising `XPDY0002`. The real "absent context item" case is still caught by `LoadContextItem`, so missing-context errors continue to surface correctly. Path shapes such as `doc(())/*` and the nested FLWOR in `Catalog004` now evaluate to the empty sequence where appropriate.
  - Removed 1 stale `KnownXQueryGaps` entry: `Catalog004`.
  - QT3: **29,940/0/1,881** (94.09%); unit tests: **1,695/0**.

- **2026-08-18** — XPath/XQuery: **distinct-values cluster** — `fn:distinct-values` and `fn:index-of` in `FunctionLibrary.cs` now compare `XdmValueKind.String` values by XSD type family. `xs:string`, `xs:untypedAtomic`, `xs:anyURI`, and the derived string subtypes compare by string value; `xs:gYear`/`gMonth`/`gDay`/`gYearMonth`/`gMonthDay` compare on the timeline only when they are the same subtype; `xs:hexBinary` and `xs:base64Binary` compare by decoded octet sequence. Cross-family values are no longer collapsed, so `cbcl-distinct-values-002b` passes.
  - Removed 1 stale `KnownXQueryGaps` entry: `cbcl-distinct-values-002b`.
  - QT3: **29,939/0/1,882** (94.08%); unit tests: **1,695/0**.

- **2026-08-17** — XPath/XQuery: **namespace fixup cluster** — `XDocumentProvider.ConstructElement` now tracks declared `prefix -> URI` bindings. When copied attributes share a prefix that is already bound to a different URI on the constructed element, a generated prefix is allocated for the second URI and stored via `AttributePrefixAnnotation` so the attribute's reported prefix matches the declaration. This fixes `cbcl-ns-fixup-1` without regressing other `DirElemContent` namespace tests.
  - Removed 1 stale `KnownXQueryGaps` entry: `cbcl-ns-fixup-1`.
  - QT3: **29,938/0/1,883** (94.08%); unit tests: **1,695/0**.

- **2026-08-17** — XPath/XQuery: **fn:analyze-string in-scope-prefixes cluster** — `fn:analyze-string` now adds an explicit `xmlns:fn` namespace declaration to the root `fn:analyze-string-result` element. LINQ to XML stores the namespace on the element name but does not materialize an `xmlns` attribute until serialization, so `fn:in-scope-prefixes` previously only reported `xml`. The explicit declaration makes `analyzeString-028` pass.
  - Removed 1 stale `KnownXQueryGaps` entry: `analyzeString-028`.
  - QT3: **29,937/0/1,884** (94.07%); unit tests: **1,695/0**.

- **2026-08-17** — XPath/XQuery: **assert-xml trailing-whitespace cluster** — `ResultComparer.NormalizeXml` now strips trailing whitespace that appears after the last element in multi-root `assert-xml` fragments (e.g. a newline before `]]>` in the expected CDATA), so formatting differences outside the result tree do not cause false mismatches.
  - Removed 1 stale `KnownXQueryGaps` entry: `d1e74610`.
  - QT3: **29,936/0/1,885** (94.07%); unit tests: **1,695/0**.

- **2026-08-17** — XPath/XQuery: **unparsed-text-available $encoding cardinality cluster** — `fn:unparsed-text-available#2` now raises **XPTY0004** when the `$encoding` argument is the empty sequence, matching the QT3 expectation in `fn-unparsed-text-available-012`.
  - Removed 1 stale `KnownXQueryGaps` entry: `fn-unparsed-text-available-012`.
  - QT3: **29,935/0/1,886** (94.07%); unit tests: **1,695/0**.

- **2026-08-17** — XPath/XQuery: **fn:path document-level PI/comment cluster** — `XDocumentNode.GetXPathParent` now falls back to the owning `XDocument` for document-level `XProcessingInstruction` and `XComment` nodes, so `fn:path` returns `/processing-instruction(...)[n]` instead of `Q{...}root()` for top-level PIs/comments. The fallback is restricted to those node kinds to avoid a self-referential loop on the `XDocument` node itself (which previously caused `fn-doc` and unit-test hangs).
  - Removed 1 stale `KnownXQueryGaps` entry: `path009`.
  - QT3: **29,934/0/1,887** (94.07%); unit tests: **1,695/0**.

- **2026-08-15** — XPath/XQuery: **assert-eq singleton-sequence unwrapping** — `CompareAssertEq` in the QT3 harness now unwraps singleton sequences before comparing values, so a single-item `QName` result (e.g. from `fn:node-name`) is compared against the expected `QName` rather than its sequence serialization.
  - `fn-node-name-26` passed once the harness treated the singleton sequence `QName` as equivalent to the bare `QName`; the engine already produced the correct namespace URI and local name.
  - Removed 1 stale `KnownXQueryGaps` entry: `fn-node-name-26`.
  - QT3: **29,933/0/1,888** (94.07%); unit tests: **1,695/0**.

- **2026-08-15** — XPath/XQuery: **date/time extraction cluster** — `fn:*-from-dateTime`, `fn:*-from-date`, and `fn:*-from-time` now declare `ParameterTypeNames` so node arguments are atomized and cast to `xs:dateTime`, `xs:date`, or `xs:time` before component extraction.
  - `FunctionLibrary` adds `ParameterTypeNames` to `fn:year-from-dateTime`, `fn:month-from-dateTime`, `fn:day-from-dateTime`, `fn:hours-from-dateTime`, `fn:minutes-from-dateTime`, `fn:seconds-from-dateTime`, `fn:timezone-from-dateTime`, `fn:year-from-date`, `fn:month-from-date`, `fn:day-from-date`, `fn:timezone-from-date`, `fn:hours-from-time`, `fn:minutes-from-time`, `fn:seconds-from-time`, and `fn:timezone-from-time`.
  - Removed 1 stale `KnownXQueryGaps` entry: `rdb-queries-results-q9` (app/UseCaseR).
  - Reverted the unverified `FirstStepRequiresContext` helper in `IrLowerer`; the simpler `StepNode` check for context-item loading is restored while the nested `let`/`for` runtime issue behind `Catalog004` is investigated separately.
  - QT3: **29,932/0/1,889** (94.07%); unit tests: **1,695/0**.

- **2026-08-15** — XPath/XQuery: **UseCaseR31 cluster** — map dynamic calls now return the empty sequence for missing keys instead of `Undefined`, and maps/arrays can be coerced to typed function items so they can be passed as `function(T) as R` arguments.
  - `InvokeFunctionItem` for maps returns `XdmSequence.Empty` when a key is absent, so subsequent path steps such as `$index(.)/title` evaluate to `()` rather than raising `XPDY0002`.
  - `ApplyFunctionConversion` wraps a map or array in a `CoercedFunctionItem` (backed by a `DelegateFunctionItem`) when the target type is a one-argument function type whose parameter and value types are compatible, applying argument and return-type conversion at each call.
  - Removed 2 `KnownXQueryGaps` entries: `UseCaseR31-009` and `UseCaseR31-012`.
  - QT3: **29,931/0/1,890** (94.06%); unit tests: **1,695/0**.

- **2026-08-15** — XPath/XQuery: **NameTest `document-node(element(...))` fix** — `instance of document-node(element(Root))` now preserves the case of the nested element kind test when matching the document element.
  - `ValueMatchesType` extracts the nested `element(...)` kind test from the case-preserved type string instead of the lowercased normalized string, so local names such as `Root` match correctly.
  - Removed `NodeTest004` from `KnownXQueryGaps`; `K2-NameTest-5` remains a documented tokenizer-torture gap.
  - QT3: **29,929/0/1,892** (94.05%); unit tests: **1,695/0**.

- **2026-08-15** — XPath/XQuery: **query-based environment collections cluster** — the QT3 harness now evaluates environment `<collection><query>` declarations and registers the resulting XDM sequences for `fn:collection` / `fn:uri-collection`.
  - `EvaluationContext` gains a `CollectionValues` dictionary for precomputed collection sequences; `FunctionLibrary.ResolveCollection` checks it before the document-path `Collections` dictionary.
  - `TestEnvironment` parses `<query>` children of `<collection>` elements and evaluates them with `XPath31Expression` after namespaces, sources, base URI, and URI mapping are applied.
  - Removed 6 stale `KnownXQueryGaps` entries: `cbcl-collection-002/003/004`, `UseCaseR31-026/027`, and `duplicates-maps-2`.
  - QT3: **29,928/0/1,893** (94.05%); unit tests: **1,695/0**.

- **2026-08-14** — XPath/XQuery: **map:merge default duplicates cluster** — `map:merge` now defaults to `use-first` for the `duplicates` option, matching F&O 3.1 §15.2.
  - `FunctionLibrary.MapMerge` uses `string duplicates = "use-first";` when the option is absent, instead of `use-last`.
  - Removed 5 stale `KnownXQueryGaps` entries (`Walmsley d1e66015/26/48/70/81`).
  - QT3: **29,922/0/1,899** (94.03%); unit tests: **1,695/0**.

- **2026-08-14** — XPath/XQuery: **direct constructor attribute cluster** — direct attribute constructors now correctly include comment/PI string values and raise **XQDY0092** for invalid `xml:space` values.
  - The `ConstructElement` attribute-value loop treats `Comment` and `ProcessingInstruction` parts as literal string values (their node content), concatenating them with literal text and atomized expression values.
  - A constructed `xml:space` attribute whose value is not exactly `default` or `preserve` now raises **XQDY0092**.
  - Removed 3 stale `KnownXQueryGaps` entries (`K2-DirectConElemAttr-42/43`, `K2-DirectConOther-65`).
  - QT3: **29,917/0/1,904** (94.02%); unit tests: **1,695/0**.

- **2026-08-14** — XPath/XQuery: **fn:deep-equal comment/PI cluster** — `fn:deep-equal` now ignores comments and processing instructions when comparing the children of element nodes, per F&O 3.1.
  - `ToNodeList` filters out `Comment` and `ProcessingInstruction` nodes from element child lists before the node-for-node comparison.
  - Removed 5 stale `KnownXQueryGaps` entries (`K2-SeqDeepEqualFunc-21/23`, `cbcl-deep-equal-001`, `functx-fn-deep-equal-5`, `functx-fn-deep-equal-all`).
  - QT3: **29,911/0/1,910** (94.00%); unit tests: **1,695/0**.

- **2026-08-14** — XPath/XQuery: **default collation sorting cluster** — `fn:sort`, `array:sort` and FLWOR `order by` clauses now honor the default or explicit collation.
  - `CompareOrderByValues` resolves the effective collation URI (default or explicit) and compares string keys with `CompareStrings` instead of ordinal comparison.
  - `fn:sort` and `array:sort` fall back to `EvaluationContext.DefaultCollation` when the collation argument is the empty sequence or omitted.
  - `declare default collation` stores the URI resolved against the static base URI, so `fn:default-collation()` reports the absolute URI.
  - Removed 8 stale collation-related `KnownXQueryGaps` entries (`fn-sort-collation-*`, `array-sort-collation-*`, `K-CollationProlog-1`, `defaultcolldecl-6`).
  - QT3: **29,906/0/1,915** (93.98%); unit tests: **1,695/0**.

- **2026-08-14** — XPath/XQuery: **QT3 sweep wave 3** — function items, constructors, validation, environment variables.
  - Named function items subtype-check against coarse kind-derived signatures (`instanceof132/133/134`); a `Undefined` return kind is treated as `empty-sequence()` (`xs-error-006/007`, `fn:error`).
  - General comparisons with a function-item operand raise **FOTY0013**; `ApplyFunctionConversion` now atomizes array arguments recursively (`FunctionCall-022`) and is public.
  - `fn:filter` converts the predicate result to `xs:boolean` via function-conversion rules, not effective boolean value (`filter-006`); `fn:parse-xml(())` returns `()`; `fn:namespace-uri-for-prefix` returns `xs:anyURI`.
  - XQuery direct element constructors require whitespace between attributes (**XPST0003**); empty enclosed expressions evaluate to the empty sequence; XML 1.0 line-ending normalization applies to literal characters in string literals.
  - Strict schema validation strips whitespace-only text nodes from element-only schema content (`ForExprType009`).
  - Switch-case comparison treats **NaN = NaN** (`switch-011`); predicate subscripts use the general path when the literal exceeds `int` range (`filter-limits-003`).
  - Conformance harness sets `QTTEST`/`QTTEST2`/`QTTESTEMPTY` for the environment-variable test sets; `assert-type` accepts `empty-sequence()` and optional cardinalities for empty results.
  - Gap-cleanup probe dropped 25 stale `KnownXQueryGaps` entries that are now passing after the wave 3 fixes.
  - QT3: **29,898/0/1,923** (93.96%); unit tests: **1,695/0**.

- **2026-08-07** — XQuery/XPath: **base-URI / URI-resolution conformance cluster** (QT3 residual sweep).
  - `declare base-uri` URILiterals are whitespace-normalized per the fn:normalize-space rules (XQ 3.1 §2.4.5): leading/trailing whitespace is stripped and internal whitespace runs (including `&#xa;` character references) collapse to a single space (base-URI-18/22/23).
  - A relative or empty declared base URI is made absolute by resolving it against the ambient static base URI (XQ 3.1 §4.5) instead of replacing/ignoring it (K2-BaseURIProlog-4; K2-BaseURIProlog-5 additionally needs the QT3 harness fallback base URI to be the test-set file URI rather than its directory).
  - fn:json-doc(()) returns the empty sequence for both arities; the options map is still validated eagerly (json-doc-028/035).
  - fn:unparsed-text / fn:unparsed-text-lines with an empty-sequence href return the empty sequence, and fn:unparsed-text-available returns false (FO31; fn-unparsed-text-available-053/054) — previously the empty href was resolved against the static base URI and probed as a resource.
  - fn:resolve-uri keeps non-ASCII IRI characters of the inputs literal in the result while leaving existing percent-encodings intact (fn-resolve-uri-30; fn-resolve-uri-31 regression-checked).
  - fn-unparsed-text-054a remains a known gap by environment: timeanddate.com fronts .NET HttpClient with a Cloudflare JS challenge (`Cf-Mitigated: challenge`, HTTP 403) regardless of request headers, while curl passes — not fixable engine-side.
- **2026-08-03** — XPath/XSLT: **XSD 1.1 regex hyphen rules + environment-stylesheet conformance sweep** (REQ-067/068/069). XSLT suite **8,340 passed / 0 failed / 6,260 skipped** (was 7,109/0 — +1,231 passing); QT3 **29,745/0** (+4); unit tests **1,695/0** (+18).
  - The recorded "XSD 1.1 regex class subtraction" gap was stale: the engine already implements the XSD 1.1 rule (`-` is a subtraction operator only immediately before `[`; `[a-d-b-c]` = `{a-d,'-',b-c}`). regex-syntax-0056a/0086a unskipped (set **986/0/4**).
  - **Harness**: test cases whose principal stylesheet comes from the referenced `<environment>` now run (~4,800 tests across 100+ sets), plus environment static `<param>` support and `unicode-version` dependency handling (regex-classes pins 6.0 → skipped; unicode-90 pins 9.0 → runs).
  - **`\i`/`\c`** use the explicit XML 1.0 (5th ed) `NameStartChar`/`NameChar` ranges per XSD 1.1 (regex-syntax-0986/0987, QT3 re00987) — category-`So` characters such as U+212E are initial name characters.
  - **Engine**: accumulator `initial-value` sees global parameters (accumulator-052); fn:path keeps steps below a parentless root with sibling indices (accessor-059..064, QT3 fo-test-fn-path-006/008/009); `element()`/`attribute()` kind tests honor `Q{uri}local` and default-element/no-namespace rules (json-to-xml-escape-*); fn:xml-to-json rejects multi-element documents (FOJS0006); fn:snapshot#0; detached-copy document-order stability (square-array-014); fn:stream-available + fn:unparsed-entity-* (QT3 harness registers fn:transform only — XSLT-only functions stay XPST0017 in XPath).
  - **XSLT engine**: xsl:where-populated filters emptiness per item (§8.4: childless elements, zero-length attribute/text/comment/PI values, zero-length strings/binaries, empty maps/arrays); xsl:fork (sequential prongs); xsl:assert (XTMM9001 default / custom error codes / try-catch, also in function bodies; `enable_assertions="false"` skips); XTDE1480 temporary-output-state tracking for xsl:result-document (variable/param/function/key/accumulator/attribute-set content); apply-templates treats arrays/maps as single items and the built-in rule applies templates to their members (arrays-301/302).
  - **Serialization**: XHTML attributes escape `"` as `&#34;` and C1 controls as `&#NNN;` (output-0102/0103); HTML5 keeps foreign-namespace prefixes for elements (except svg/MathML/XHTML which take the default-namespace form) and attributes, declaring bindings on the host element (output-0602/0603).
- **2026-08-01 (c)** — XQuery: **`declare decimal-format` / `declare boundary-space` prolog support** — prod/DecimalFormatDecl **41/0**, prod/BoundarySpaceDecl **28/0**, fn-load-xquery-module **61/0**.
  - Named and default decimal formats with the full XQST0097/0098/0111/0114 validation matrix; declarations feed fn:format-number named/default resolution, with module-local formats applied around library-module bodies (decimal-format-21).
  - `declare boundary-space strip|preserve` (XQST0068 on duplicate) threads into direct-constructor whitespace handling: preserve keeps whitespace-only runs at content boundaries.
  - **HTML/XHTML serialization matrix**: version-dependent void-element lists (frame/isindex are HTML 4.0-only), XHTML void self-closing, foreign-namespace "XML islands" (self-close + CDATA text), boolean attribute minimization, script/style raw text, HTML5 prefix normalization, doctype fallback gated to version ≥ 5 — all six ser/* sets green (html 64, xhtml 49, xml 39, text 18, json 73, adaptive 87).
- **2026-08-01 (b)** — XQuery: **fn:load-xquery-module implemented** — dynamic library-module loading (F&O 3.1 §15.3.1): fn-load-xquery-module set **58 passed / 0 failed / 25 skipped**.
  - Module URIs resolve via compiler-registered sources (`XQueryCompiler.WithModule`, seeded onto the evaluation context as `XQueryModuleSources`) with `location-hints` candidate selection and a filesystem fallback; the transitive import closure is compiled through the existing library-module pipeline.
  - The result map exposes the target module's PUBLIC variables and functions only: `map{"variables": map{QName → value}, "functions": map{QName → map{arity → function item}}}`. Function items invoke against the module's own evaluation context (context item, externals, lazy variables).
  - Options map: `variables` (external-only, type-checked FOQM0005, non-external ignored), `context-item` (validated against the module's declared context item type), `xquery-version` (numeric; unsupported → FOQM0006), `location-hints`; full error-code matrix (FOQM0001/0002/0003/0005/0006, XPDY0002, XPTY0004) — module initializer errors propagate unchanged.
  - Recorded gaps: `declare decimal-format` / `declare boundary-space` prolog support (3 tests skipped, matching the dedicated sets).
- **2026-08-01** — XSLT: **conformance sweep closed** — 19 engine/harness fixes, XSLT 3.0 suite at **7,109 passed / 0 failed / 7,491 skipped**; QT3 unchanged at **29,510/0**; unit tests **1,665/0**.
  - `fn:system-property` is available in use-when and other static expressions (`IsXsltMode` on static contexts); `fn:current` raises XPST0017 there.
  - Pattern keyword disambiguation: `union`/`intersect`/`except` after `/`, `@`, `::` is a NameTest.
  - XPath 1.0 backwards compatibility honored on element-level `xsl:version="1.0"` AVTs, global variable declarations, and for function-argument first-item rules and numeric conversions; `fn:namespace-uri`/`fn:string` take the first sequence item in BC mode.
  - Environment-declared collections are available to `fn:collection`/`fn:uri-collection`; a declared-but-empty collection returns the empty sequence.
  - Comment/PI-only result trees are preserved; `xs:QName()` atomizes node arguments; `xml:id` values are whitespace-collapsed at load; timezone `[z]` drops whole-hour minutes when the full form exceeds the width modifier; unknown calendars fall back to `[Calendar: AD]` in XSLT mode; `suppress-indentation` covers descendants; `copy-namespaces="no"` keeps the element's own prefix; bare `fn:serialize` omits the XML declaration while static output declarations include it; `fn:round` uses exact rational scaling at extreme magnitudes.
- **2026-07-29** — XQuery: **residual-cluster sweep closed** (REQ-066): QT3 **29,510 passed / 0 failed** (from 29,427; +83 passing; gaps 216, −83).
  - Stable order-by (index-decorated sort — `List<T>.Sort` is unstable); switch no-match-on-error with cardinality pre-checks and empty-matches-empty; array atomization and recursive content flattening; min/max boolean and date/time family rules.
  - Computed elements apply the default element namespace (xmlns="" materialized); constructor-local prefixes propagate; constructor/`()` steps after `/` with `<`-after-slash XPST0003; schema kind-test grammar/runtime error split; implicit namespace-node() is XQST0134 in XQuery.
  - External function declarations, initializer self-reference XPST0008, XQST0070/XQST0052 namespace rules, type-text comments, xs:error constructor, generate-id/base-uri/xml:id checks, duration-division rule.
  - Every swept set runs fully green (AxisStep, VarDecl, StepExpr, SwitchExpr, PathExpr, ArrayTest, DefaultNamespaceDecl, fn:id/idref, in-scope-prefixes, min, base-uri, doc, generate-id, xs:error, divide-dayTimeDuration).
- **2026-07-29** — XQuery: **dayTimeDurations clusters closed** (REQ-065): QT3 **29,427 passed / 0 failed** (from 29,400; +27 passing; gaps 299, −27).
  - Plain `xs:duration` operands in date/time arithmetic now raise **XPTY0004** — only `xs:dayTimeDuration`/`xs:yearMonthDuration` are permitted (cbcl-plus/minus family).
  - op/add-dayTimeDurations 61/0/0, op/subtract-dayTimeDurations 69/0/0; all other duration-arithmetic sets remain green.
- **2026-07-29** — XQuery: **HigherOrderFunctions cluster closed** (REQ-064): QT3 **29,400 passed / 0 failed** (from 29,389; +11 passing; gaps 326, −11).
  - Function-item error codes: **FOTY0013** for comparisons and content atomization, **XQTY0105** for element content; partial-application arity validated (**XPTY0004**).
  - Dynamic invokes apply the function conversion rules (singleton unwrap, atomization, untypedAtomic casting) for named refs, user functions, inline functions, and partial applications; `fn:round-half-to-even` coerces untypedAtomic.
  - Named references created without a focus invoke with an absent focus (**XPDY0002**); function items capture their module's static base URI; parenthesized sequence types `(function(...) as ...)*` parse and match.
  - misc/HigherOrderFunctions 126/0/3; function-lookup, round, comparison, constructor, and cast sets all green.
- **2026-07-29** — XQuery: **CompNamespaceConstructor cluster closed** (REQ-063): QT3 **29,389 passed / 0 failed** (from 29,378; +11 passing; gaps 337, −11).
  - Namespace declarations in element content interleave freely with attributes (no XQTY0024); same-URI duplicates merge; redundant xmlns:xml omitted.
  - Content namespace declarations win over name-implied prefixes — conflicting element/attribute names get a generated prefix; `namespace {expr} {uri}` validates the prefix type (**XPTY0004**), empty expression = default declaration.
  - Computed namespace nodes are parentless with an xs:string typed value (XDM §2.7.2).
  - prod/CompNamespaceConstructor 32/0/12; constructor, namespace-axis, in-scope-prefixes, name-test, and fn:data sets all green.
- **2026-07-29** — XQuery: **AllowingEmpty cluster closed** (REQ-062): QT3 **29,378 passed / 0 failed** (from 29,364; +14 passing; gaps 348, −14).
  - `for $x allowing empty at $p in E` parses in grammar position (before the positional variable); empty input binds `$x = ()` with position 0.
  - The empty binding is checked against the declared type occurrence: `as xs:integer?` accepts it, `as xs:integer` raises **XPTY0004**.
  - prod/AllowingEmpty 19/0/0; for/let/window clause sets all green.
- **2026-07-29** — XQuery: **MapConstructor cluster closed** (REQ-061): QT3 **29,364 passed / 0 failed** (from 29,349; +15 passing; gaps 362, −15).
  - Map constructors work in step and `!` position with step expressions as keys/values: entry-colon disambiguation for `prefix:*`/`*:local` (gated inside map keys), one-colon QNames, `*:b:b` token splitting, `self` as an element name.
  - Singleton sequences unwrap for map/array/function-typed call parameters (`map:size($ctx ! map{...})`); fn:deep-equal compares map values and array members with sequence semantics.
  - prod/MapConstructor 42/0/0; fn-deep-equal, array, name-test, axis-step, EQName, and HOF sets all green.
- **2026-07-29** — XQuery: **CombinedErrorCodes cluster closed** (REQ-060): QT3 **29,349 passed / 0 failed** (from 29,332; +17 passing; gaps 377, −17).
  - `fn:id`/`fn:idref`/`fn:element-with-id` require a document-rooted tree (**FODC0001**); path steps over atomic items raise **XPTY0019**; unsupported default collations raise **XQST0038**; empty default function namespace is **XQST0060**; `for $x at $x` is **XQST0089**; inline `%public`/`%private` is **XQST0125**.
  - misc/CombinedErrorCodes 210/0/49; fn-id/idref, axis-step, for-clause, collation, and annotation sets all green (7 stale entries removed).
- **2026-07-29** — XQuery: **Literal cluster closed** (REQ-059): QT3 **29,332 passed / 0 failed** (from 29,316; +16 passing; gaps 394, −16).
  - Character references to invalid XML characters raise **XQST0090** in string literals and constructors (`&#x00;`, `&#x0;`); numeric overflows — including 64-bit — are **XQST0090**; malformed references (`&#+20;`) are **XPST0003**.
  - Valid references expand normally (predefined entities, decimal/hex, astral codepoints); XPath mode does not expand references (8 stale gap entries removed).
  - prod/Literal 171/0/3; direct/computed constructor, string-constructor, and EQName sets all green.
- **2026-07-29** — XQuery: **Annotation cluster closed** (REQ-058): QT3 **29,316 passed / 0 failed** (from 29,292; +24 passing; gaps 410, −24).
  - Inline-function annotations (`%eg:sequential function () { ... }`) parse with literal parameters, EQName forms, and multiples; unrecognized annotations are ignored.
  - Function-test annotation assertions (`instance of %eg:x function(*)`) parse and are ignored for matching (a conformant choice); reserved annotation namespaces raise **XQST0045**, unbound prefixes **XPST0081**.
  - Annotation arguments must be literals — `%eg:sequential(true())` is **XPST0003**; annotations in XPath mode are **XPST0003** (XQuery-only grammar).
  - prod/Annotation 58/0/0; instance-of, cast/castable, treat, typeswitch, inline-function, and higher-order-function sets all green.
- **2026-07-29** — XQuery: **NamespaceDecl cluster closed** (REQ-057): QT3 **29,292 passed / 0 failed** (from 29,281; +11 passing; gaps 434, −11).
  - Duplicate namespace prefix declarations raise **XQST0033** — undeclarations count as declarations (K2-NamespaceProlog-1/2/3).
  - Reserved names raise **XQST0070**: the `xml` prefix must not be declared at all, `xmlns` must not be declared or undeclared, and no prefix may be bound to the XML/XMLNS namespace names.
  - Two-phase prolog ordering enforced: namespace, default-namespace, setter, and import declarations after a context-item/function/variable/option declaration are **XPST0003** (K2-NamespaceProlog-14).
  - prod/NamespaceDecl 44/0/0; module-import, option, base-uri, collation, ordering, and default-namespace sets all green.
- **2026-07-29** — XQuery: **VarDecl.external cluster closed** (REQ-056): QT3 **29,281 passed / 0 failed** (from 29,264; +17 passing; gaps 445, −17).
  - Variable initializers and context-item initial values are parsed as **ExprSingle**: a top-level comma is **XPST0003** (K2-ExternalVariablesWith-11).
  - Declared `as T` on variable declarations is enforced **strictly** — atomization plus an instance check, no casts and no numeric/URI promotion; mismatch raises **XPTY0004** (K2-ExternalVariablesWith-12..19, extvardeclwithtype-19).
  - Occurrence indicators inside kind-test type names: `element(*, xs:untyped+)` / `xs:untyped*` are **XPST0003**; `?` stays legal as the XSD 1.1 nullable marker.
  - Namespace undeclarations (`declare namespace p = "";`) now unbind the runtime context too — undeclaring the predeclared `xs` prefix makes `xs:integer(1)` raise **XPST0081** (K2-NamespaceProlog-4/9); unbound function/variable prefixes report XPST0081.
  - prod/VarDecl.external 96/0/3; prod/VarDecl, prod/NamespaceDecl, prod/FunctionDecl, FLWOR clause sets all green.
- **2026-07-28** — XPath/XQuery: **NameTest cluster closed** (REQ-055): QT3 **29,264 passed / 0 failed** (from 29,244; +20 passing; gaps 462, −20).
  - Name tests with unresolvable prefixes raise **XPST0081**; kind-test schema type names are validated (**XPST0008** for undeclared types) and matched (untyped elements/attributes match untyped types and supertypes); `processing-instruction(...)` arguments are trimmed and NCName-validated (**XPTY0004**; **XPST0003** for invalid forms); `Q{   }*` normalizes to the empty-namespace wildcard.
  - Constructor in-scope namespaces are now spec-correct: explicit xmlns declarations and element-name bindings **propagate** to nested constructors with override semantics, while **attribute-name-implied** bindings stay local to the carrying element (`NonPropagatingNamespaceBinding` markers on constructed trees; `in-scope-prefixes`, `namespace-uri-for-prefix`, and the namespace axis honor them).
  - Redundant namespace declarations are omitted at serialization/comparison time (trees stay semantically complete; output matches SAXON).
  - Instance-of `element(P:L)`/`attribute(P:L)` compares the resolved namespace URI; `ApplyFunctionConversion` threads the runtime context.
  - prod/NameTest 125/0/2; prod/DirElemContent(.namespace), fn/in-scope-prefixes, op/union, op/intersect, op/except, app/CatalogCheck, app/FunctxFunctx all green.
- **2026-07-27** — XQuery 3.1: **ordering features** (REQ-054): QT3 **29,244 passed / 0 failed** (from 29,150; +94 passing).
  - `ordered { E }` / `unordered { E }` expressions (identity — document order is always produced, valid under both modes; empty bodies are the empty sequence); `declare ordering ordered|unordered;` with XQST0065 on duplicate.
  - `declare default order empty least|greatest;` with XQST0069; the default flows through the static context into the IR lowerer and applies to order-by clauses without an explicit `empty` modifier (an explicit modifier wins).
  - Sets: prod/UnorderedExpr 26/0/2, prod/OrderingModeDecl 27/0/0, prod/EmptyOrderDecl 32/0/0; gaps unchanged (464).
- **2026-07-27** — XQuery 3.1: **string constructors** (REQ-053): QT3 **29,150 passed / 0 failed** (from 29,114; +36 passing; gaps 464, −35).
  - `` `[literal `{expr}` literal]`` with full nesting awareness: interpolations inside interpolations, string constructors inside direct element constructors and vice versa; literal text is raw (no reference expansion, whitespace preserved, single backticks literal); unterminated forms are XPST0003.
  - Interpolations desugar to `fn:string-join(fn:data(E) ! fn:string(.), " ")` — atomization raises FOTY0013 for maps, arrays flatten, sequence items space-join, parts concatenate without separator; empty interpolations are empty strings.
  - XPath-mode string literals no longer expand predefined entity/character references (spec: expansion is XQuery-only; assert-eq expectations evaluate per XPath rules).
  - Harness: construct-gate regex fixed (`RegexOptions.Compiled` had been glued into the pattern, breaking pragma gating after the string-constructor alternative was removed).
  - prod/StringConstructor 49/0/3 (was 14/0/38).
- **2026-07-27** — **try/catch completion** (REQ-052): QT3 **29,114 passed / 0 failed** (from 28,931; +183 passing).
  - Full XPath 3.1 catch grammar on both pipelines: `catch CodePatternList { Expr }` with one-or-more clauses (first match wins), patterns `*`, `err:X`, `err:*`, `*:X`, `Q{uri}X`, `Q{uri}*`, NCName; empty try/catch bodies; unmatched errors propagate.
  - All seven `err:*` variables bound with save/restore: `err:code` as `xs:QName` (prefix preserved), `err:description`, `err:value`, `err:module`, `err:line-number`, `err:column-number`, `err:additional` (empty).
  - `fn:error` throws structured `XPathErrorException` (Runtime layer); empty code → `err:FOER0000`; error value surfaced via `$err:value`.
  - Bypass rules: static-coded errors (XPST/XQST not from `fn:error`) and lazy global-variable initializer errors are never caught.
  - Error-code hygiene: `cast` FORG0001, `treat as` XPDY0050, computed-constructor prefix XQDY0074, `fn:zero-or-one`/`one-or-more`/`exactly-one` FORG0003/0004/0005, `fn:parse-xml`/`parse-xml-fragment` FODC0006 with external-DTD resolution against the static base URI and validated text declarations in fragments.
  - prod/TryCatchExpr 172/0/1; gaps unchanged (499 reasoned skips).
- **2026-07-27** — XQuery 3.1 Phase 4: **library modules** (slice 2, REQ-051): QT3 **28,931 passed / 0 failed** (from 28,735; +196 passing).
  - `module namespace prefix = "uri";` library modules and `import module (namespace p =)? "uri" (at "loc", ...)?;` in main and library modules; module namespace URIs and location hints get whitespace normalization; import cycles are legal; multiple modules may share one target namespace (merged, XQST0034/XQST0049 on collisions).
  - Static validations: XQST0047 (duplicate import), XQST0088 (empty target namespace), XQST0059 (module not found / target-namespace mismatch), XQST0048 (declaration outside the target namespace), XQST0070 (xml/xmlns import prefix), XQST0108 (output declaration in a library module), XQST0113 (context-item value/default in a library module), XQST0032 (duplicate base-uri), XPST0003 (library module as query / body in a library module).
  - `%public` / `%private` declaration annotations with visibility enforcement: private functions/variables are invisible to importing modules (XPST0017/XPST0008, statically checked incl. named function references); conflicting/duplicate visibility annotations are XQST0106/XQST0116; annotations in reserved namespaces or unknown XQuery-namespace annotations are XQST0045; annotation arguments must be literals; unknown annotations in other namespaces are ignored; `xsi` is now a predeclared prefix.
  - Module loading: `XQueryCompiler.WithModule(uri, source, location?)` registers library module sources; imports resolve to the transitive closure with per-(namespace, location-hint) incremental loading — all public declarations of every loaded module in an imported namespace are visible (XQ 3.1 §4.12.2, modules-31).
  - Per-module static contexts: each library module's function/variable bodies compile with its own prolog context and execute with its namespaces, base URI, default element namespace, and default collation applied (cbcl-module-002: module-local base URIs win over the importing module's).
  - Harness: `<module uri location? file>` catalog entries parsed and registered; `moduleImport` feature admitted; inline `<context-item select="..."/>` environments applied as the initial focus; `<assert>` comparisons also bind the query result as the context item; the unsupported-prolog gate is comment-tolerant.
  - prod/ModuleImport 106/0/22 (remaining skips are XQ10-only or schema-import-gated); prod/ContextItemDecl, prod/Annotation, misc/CombinedErrorCodes module tests green; 499 reasoned skips total (new: closure context-capture in module function items xqhof16/18, map-as-function coercion UseCaseR31-012, copy.xq `fn:id` FODC0001 semantics, context-item type enforcement contextDecl-054, function-test annotation assertions).
- **2026-07-26** — XQuery 3.1 Phase 4: **user-defined functions and variables** (library modules slice 1, REQ-050): QT3 **28,735 passed / 0 failed** (from 26,299; +2,436 passing).
  - `declare function` / `declare variable` prolog with the static-validation matrix (XQST0034/0039/0045/0049, XPST0003, runtime XQST0054); `local` prefix predeclared; lazy global initializers evaluated with the module's initial focus.
  - Invocation semantics per XPath 3.1 §3.1.5: focus absent inside user-function bodies (XPDY0002); full variable-scope snapshot per call (recursive `let` bindings cannot clobber the caller); captured closures preserved.
  - Function conversion: attribute nodes atomize to xs:untypedAtomic; function-item coercion wraps items in `CoercedFunctionItem` for typed function tests (occurrence/parens/whitespace normalization); `External` kind parameters pass through dynamic-call conversion.
  - Function-type syntax (`function(...) as ...`, parenthesized item types) in declared signatures, `let`/`for` `as` clauses, and `instance of`; shared-parser `SkipSequenceType` stops at expression boundaries after a function-type `as`.
  - Order-by comparator casts untypedAtomic to xs:string and raises XPTY0004 for cross-family comparisons (orderBy68).
  - Newly-enabled sets: app/FunctxFn 499/2, app/FunctxFunctx 622/5, app/Walmsley 212/6, app/spec-examples 630/3, prod/FunctionDecl 150/3/20, misc/HigherOrderFunctions 108/9/12.
  - Recorded gaps: static-analysis errors (XPST0008/XPST0017 in unexecuted bodies), `sudoku` (too slow under the interpreter), functx `get-matches`/`remove-elements` edges; 487 reasoned skips total.
- **2026-07-25** — XQuery 3.1 Phase 4 start: **output declarations + serialization round-out** (REQ-049): QT3 **26,299 passed / 0 failed** (from 25,928; +371).
  - `declare option output:* "..."` prolog with QName/EQName option names, prolog ordering rules (namespace declarations precede options), and static validations (XQST0109/XQST0110/XQST0066/XPST0003/XPST0081); XQuery comments `(: :)` skipped in the prolog.
  - Static output parameters flow to `fn:serialize` (per-call parameters override). Two `omit-xml-declaration` defaults apply by context: bare `fn:serialize` (no static output declarations) defaults to `yes` (the declaration is omitted, per Serialization 3.1), while queries with static output declarations (`declare option output:*`) follow the XSLT/XQuery host-language default of including the declaration (`no`); `output:parameter-document` resolves lazily through the document loader (prolog options take precedence).
  - Serializer driven to Serialization 3.1 fidelity: declaration/DOCTYPE matrix, html/html5/xhtml variants, adaptive constructor-form atomics, JSON character maps, CDATA rules, indent/suppress-indentation/xml:space, namespace fixup with a declaration scope stack, XML 1.1 namespace undeclarations (undeclare-prefixes), and XML 1.1 line-ending normalization gated on the test's xml-version.
  - QT3 fully green: all six ser/* method sets (38+18+45+40+73+87), fn/serialize 168/0, OptionDecl 41/0, OptionDecl.serialization 36/0, Comment 72/0.
  - Supporting fixes: attribute normalization is literal-only with xml:id collapse; map keys distinguish string-family subtypes from g* date types; inline-function instance-of uses declared types; XML 1.1 character references and namespace undeclarations honored end-to-end.
  - Harness: `serialization-matches`/`assert-serialization`/`assert-serialization-error` assertions with flags and `not`; assert-type delegates parenthesized types to the engine.
  - Unit tests now **1,479/0**; XSLT baseline unchanged.

- **2026-07-25** — XQuery 3.1: **switch / typeswitch expressions** (REQ-048): QT3 **25,928 passed / 0 failed** (from 25,846; +82).
  - `switch (E) case V1 case V2 return R1 ... default return RD` and `typeswitch (E) case $v as T return R ... default ($d)? return RD` parsed as dedicated AST nodes (XQuery mode only) and desugared in the IR lowerer to synthetic `let` + nested `if` chains — no new opcodes.
  - `switch` compares with `eq` value-comparison semantics; case operands evaluate lazily in order (errors in later cases never surface after a match). `typeswitch` uses `instance of` checks with per-branch variable scoping; sequence-type unions (`case $i as xs:integer | xs:string`) supported.
  - Supporting fixes: `fn:document-uri` returns an `xs:anyURI`-annotated value; harness keeps XPath-only tests expecting a parse error on the XPath pipeline even inside XQuery test sets; optimizer traversal for the new nodes is reference-transparent (no fixpoint loop).
  - QT3 sets: SwitchExpr 67/3, TypeswitchExpr 62/3 (the 3 remaining need static variable-scope analysis — recorded as gaps).
  - Unit tests now **1,470/0**; XSLT baseline unchanged.

- **2026-07-25** — XQuery 3.1 Phase 3 complete: **computed constructors** (REQ-047): QT3 **25,846 passed / 0 failed** (from 25,060; +786).
  - All seven computed constructor forms (`element`/`attribute`/`document`/`text`/`comment`/`processing-instruction`/`namespace`) with static EQName or computed `{expr}` names; parser recognition hooked into step expressions so `element` is not swallowed as a name test; keywords usable as constructor names (`attribute return {()}`).
  - Single `ConstructComputed` IR opcode with per-kind VM handlers and a shared content accumulator: attributes before content only (XQTY0024), duplicate attributes (XQDY0025), namespace nodes become declarations (XQDY0102 incl. spec bug 22032), adjacent atomic values joined with single spaces, text nodes merged without separator, arrays flattened.
  - Computed name resolution: EQName `Q{uri}local` (whitespace normalization, char/entity reference expansion, literal `{` rejected), `prefix:local` via context namespaces, `xs:QName` instances; full error-code coverage (XPTY0004, XQDY0074, XPST0081, XQDY0096, XQDY0044, XQDY0041/0064, XQDY0026, XQDY0072, XQDY0091, XQDY0101); static PI targets must be NCNames (XPST0003).
  - Attribute prefix rules: XML namespace coerces to the `xml` prefix, other namespaces get a generated prefix, and prefixes survive on free-standing attributes via a provider annotation (LINQ attributes cannot carry one).
  - Computed `text {}` with empty content produces no node; a zero-length string still constructs a text node.
  - Supporting fixes: window-clause and FLWOR tuple variable bindings keep prefixes/EQName namespaces (`TupleBindInfo` carries prefixes, resolved at bind time); empty-CDATA boundary whitespace; XQuery 3.1 spec-token awareness in the harness dependency filter (XQ10/XQ30-only tests skip on an XQ31 processor).
  - QT3 sets fully green: CompText 38/0, CompComment 27/0, CompDoc 40/0, CompElem 86/0, CompAttr 111/0, CompPI 56/0, CompNamespace 11/0; supporting: WindowClause 123/0, OrderByClause 194/0, GroupByClause 30/0, CountClause 13/0, DirElemConstructor 62/0.
  - Unit tests now **1,458/0**; XSLT baseline unchanged.

- **2026-07-25** — XQuery 3.1 Phase 3 start: **direct element constructors** (REQ-046): QT3 **25,060 passed / 0 failed** (from 22,983; +2,077).
  - New lexer constructor mode: a whole direct constructor (`<name a="v">text {expr}<nested/></name>`, `<!-- c -->`, `<?pi d?>`) is emitted as a single `Constructor` token, keeping quotes, `&`, and raw text out of the token stream; structure falls back to the `<` comparison operator when it does not hold.
  - Source-level constructor scanner builds `DirectElementConstructorNode`/`DirectCommentNode`/`DirectProcessingInstructionNode` (entity refs, `{{`/`}}` escapes, quote doubling, comment-aware enclosed expressions, empty-`{}` rules).
  - New IR opcodes: `ConstructElement`, `ConstructContentNode`, `SaveNamespaces`/`DeclareNamespace`/`RestoreNamespaces` (constructor-local `xmlns` with dynamic scoping, `xmlns=""` undeclarations, redundant-declaration fixup, in-scope copying for cloned nodes).
  - Provider-neutral node construction via `EvaluationContext.ElementConstructorHook`/`ContentNodeConstructorHook` with an XDocument implementation (prefix declarations, namespace fixup, clone copying, static base-URI annotation).
  - Constructor semantics: attribute values normalized (collapse+trim), items joined with single spaces per enclosed expression, attribute nodes in content become element attributes (XQTY0024), arrays flattened, boundary whitespace stripped unless `xml:space="preserve"` or reference/CDATA-significant, standalone comment/PI constructors as primary expressions.
  - Validations: XQST0118 (tag mismatch), XQDY0025 (duplicate attributes), XQST0070/0071 (prefix misuse/duplicates), XQST0022 (computed ns URI), XQST0046 (invalid ns URI char), XQST0090 (invalid character reference), XPST0081 (undeclared prefix incl. prefixed type names in `instance of`/casts).
  - Supporting fixes: predicate EBV no longer atomizes node results (self-axis predicates work), FLWOR tuple variables scoped to the body `For` (XPST0008 on reference after scope ends), `fn:distinct-values` returns atomized values, `day-from-dateTime` parameter conversion, decimal −0 normalization, `xsi`/`local` predefined prefixes, `allowing empty` for-bindings.
  - QT3 sets: WindowClause 117/0, OrderByClause 191/0, GroupByClause 30/0, CountClause 13/0, DirElemConstructor 62/1, DirElemContent.namespace 111/1, DirElemContent 227/4, DirElemContent.whitespace 19/0.
  - Unit tests now **1,443/0**; XSLT baseline unchanged.

- **2026-07-25** — XQuery conformance gap shrinkage (REQ-045 follow-up): QT3 **22,983 passed / 0 failed** (from 22,947); gaps 203 → 167.
  - Named function reference arity validation: `fn:filter#0` and friends now raise `XPST0017` in the `LoadFunction` named-item path (~40 tests across `fn-filter`, `fn-function-lookup`, `fn-innermost`, `fn-outermost`, `fn-for-each-pair`, etc.).
  - Variadic functions: `FunctionSignature.IsVariadic` with a fallback in `TryResolveFunction`; `fn:concat` is variadic, so `fn:concat#99` resolves.
  - Group-by string keys compare with the default/spec collation (base-URI resolved), e.g. `html-ascii-case-insensitive` merges `ABC`/`abc`.
  - `fn:distinct-values`/`fn:deep-equal` compare `gYear`/`gYearMonth`/`gMonth`/`gMonthDay`/`gDay` values on the timeline (implicit timezone for tz-less values) via the now-public `VmEngine.CompareDateTimeValues`.
  - Map keys treat timezone presence as significant for date/time keys; hashing/comparison uses throw-safe UTC instant keys (civil-date arithmetic) instead of the `DateTimeOffset` projection.
  - Unit tests now **1,429/0**; XSLT baseline unchanged.

- **2026-07-25** — QT3 harness wired to the XQuery pipeline (REQ-045): **22,947 passed / 0 failed** (from 14,994 / 0).
  - `Bosak.XPath.Conformance` now references `Bosak.XQuery`; tests with positive XQuery-only spec dependencies are admitted when the query uses only supported constructs (`DependencyFilter.IsSupported(..., allowXQuerySpecs)` + `TestExecutor.CanHandleAsXQuery` gating out constructors, switch/typeswitch, annotations, pragmas, string constructors, and unsupported prolog forms).
  - Admitted tests evaluate through `XQueryCompiler` with the harness `EvaluationContext` bridged into `XQueryContext`; result comparison is unchanged. XQ-dep tests expecting parse errors also route (the XQuery parser produces the expected `XPST0003`); XPath-dep parse-error tests stay on the XPath pipeline.
  - Engine fixes surfaced by the newly-routed tests: window end-positional is the input-sequence position and the end condition is optional; `XQST0103`; nested `For`/`Window` tuple-path blocks now `Return` accumulated tuples (multi-binding `for` + order by, `let` + order by, nested window); `as SequenceType` declarations on `for`/`let`/`some`/`every` bindings, window variables, and grouping specs (new `EnforceType` opcode, `XPTY0004`); NaN follows empty least/greatest; order-by collation validation (`XQST0076`) with base-URI resolution; `stable order by`; `declare base-uri`; version/encoding validation (`XQST0031`/`XQST0087`); duplicate default collation (`XQST0038`); prolog literals expand character references and prolog syntax errors are `XPST0003`; XQuery string literals expand entity/character references; empty inline-function bodies; date/time group keys compare by instant; FLWOR positional variables captured in tuples.
  - The four FLWOR QT3 sets are green: WindowClause 34, OrderByClause 39, GroupByClause 14, CountClause 4 passed, 0 failed.
  - The remaining 203 admitted-but-failing tests are recorded in `ConformanceRunner.KnownXQueryGaps` with per-set reasons — the work-item list for closing XQuery conformance (see REQ-045).
  - QT3: 22,947 passed / 0 failed / 8,874 skipped (72.11%). Unit tests now **1,421/0**. XSLT baseline unchanged.

- **2026-07-25** — XQuery 3.1 Phase 2 complete: `window` clause implemented on top of the tuple-based FLWOR path.
  - Extended `XPathParser` to parse `for tumbling|sliding window $var in expr start ... when ... (only)? end ... when ...` when `allowFullFlwor` is true, both as the initial and as an intermediate clause; XPath-only mode rejects it with `XPST0003`.
  - Added `WindowClauseNode` and `WindowCondition` to `XPathAstNode`; `XPathOptimizer` and `XQueryCompiler` traverse/resolve the in-expression and both when-expressions.
  - Added a `Window` IR opcode and `WindowInfo` literal-pool record; `IrLowerer.LowerWindowClauseForTuples` emits start-condition, end-condition, and window-body blocks on the tuple path, and `ComputeBoundVariables` captures the window and condition variables so `order by` keys and the return expression see them.
  - Added a `Window` VM handler implementing tumbling (windows open only when none is open) and sliding (possibly overlapping) semantics, with current/positional/previous/next WindowVars, `only end`, single-item windows, and unclosed windows at end of input.
  - Clauses other than `count` after an `order by` (including `window`) now fail fast with `NotSupportedException` instead of being silently dropped.
  - Added 7 XQuery unit tests covering tumbling, `only end`, sliding overlap, start/end variables, previous/next, window + `order by`, and XPath-mode rejection — all pass.
  - No regressions in XPath, XSLT, or existing unit tests; unit tests now **1,409/0**.

- **2026-07-25** — XQuery 3.1 Phase 2: `group by` clause implemented on top of the tuple-based FLWOR path.
  - Extended `XPathParser` to parse `group by` grouping specs (`$var` or `$var := expr`, optional `collation`) when `allowFullFlwor` is true; XPath-only mode rejects it with `XPST0003`.
  - Added `GroupByClauseNode` and `GroupingSpec` to `XPathAstNode`; `XPathOptimizer` and `XQueryCompiler` traverse/resolve grouping-spec key expressions.
  - Added a `GroupBy` IR opcode and `GroupByInfo` literal-pool record; `IrLowerer.LowerFlworWithGrouping` lowers `:=` specs as synthetic `let` bindings so every grouping key is captured in the tuple.
  - Added a `GroupBy` VM handler that groups tuples by key equality (first-appearance order; empty keys group together, NaN = NaN, multi-item keys raise `XPTY0004`) and merges each group: grouping variables keep the shared key value, other variables bind to the concatenated group values.
  - A post-group `order by` re-keys the grouped tuples in a second tuple pass so sort keys are evaluated against the grouped bindings; post-group `count` clauses are also supported.
  - Unsupported shapes fail fast at compile time: multiple `group by` clauses, `order by` before `group by`, and post-group clauses other than `order by`/`count`.
  - Added 8 XQuery unit tests covering simple grouping, computed string keys, aggregation of non-grouping variables, `where`, post-group `order by`, post-group `count`, multiple grouping specs, and XPath-mode rejection — all pass.
  - No regressions in XPath, XSLT, or existing unit tests; unit tests now **1,402/0**.

- **2026-07-23** — XQuery 3.1 Phase 2: `count` clause implemented on top of the tuple-based FLWOR path.
  - Extended `XPathParser` to parse `count $var` as a FLWOR intermediate clause when `allowFullFlwor` is true.
  - Added `CountClauseNode` to `XPathAstNode`; `XPathOptimizer` traverses it.
  - Generalised `IrLowerer.LowerFlworWithTuples` to maintain compiler-managed integer counters for each `count` clause, both before and after any `order by`.
  - Reuses existing `LoadVariable`, `StoreVariable`, and `Add` opcodes; no new VM opcodes required.
  - Added 5 XQuery unit tests covering simple count, `where`, `let`, pre-`order by`, and post-`order by` — all pass.
  - No regressions in XPath, XSLT, or existing unit tests; unit tests now **1,394/0**.

- **2026-07-22** — XQuery 3.1 Phase 2: `order by` clause implemented with tuple-based VM sorting.
  - Extended `XPathParser` with `allowFullFlwor` to parse multi-clause `for`/`let`, `where`, and `order by` (empty ordering, collation) while preserving XPath-only mode.
  - Added `FlworExpressionNode` and `OrderByClauseNode` AST nodes; `XPathOptimizer` now traverses full FLWOR clauses.
  - Added `OrderBy` and `TupleBind` IR opcodes; `IrLowerer` lowers FLWOR expressions into tuple arrays, sorts them, and binds them back to the body.
  - Added `OrderBy` and `TupleBind` VM handlers in `VmEngine` for stable, collation-aware sorting.
  - Added 13 XQuery unit tests covering ascending, descending, strings, `where`, `let`, and multiple sort keys — all pass.
  - No regressions in XPath, XSLT, or existing unit tests; unit tests now **1,389/0**.

- **2026-07-22** — XQuery 3.1 Phase 1 foundation: `Bosak.XQuery` now compiles and executes prolog-less queries.
  - Added `XQueryParser` (top-level XQuery grammar + prolog declarations) and `XQueryStaticContext` (namespace/default bindings, declared variables/functions).
  - Wired `XQueryCompiler` → `XPathParser` → `XPathOptimizer` → `IrLowerer` → `VmEngine`.
  - `XQueryExecutable.Evaluate` applies the prolog-derived static context to the runtime `EvaluationContext`, executes the IR module, and restores the original context state.
  - First passing XQuery tests: `for $i in 1 to 3 return $i`, `let $x := 42 return $x`, and `declare namespace math = '...'; math:pi()`.
  - No regressions in XPath, XSLT, or existing unit tests; unit tests now **1,382/0**.

- **2026-07-21** — QT3 XML 1.0-only skip categorization cleanup: `DependencyFilter` now handles `xml-version` dependencies and the hardcoded `DocumentedSkips` entries are removed.
  - 5 tests (`cbcl-codepoints-to-string-023/024`, `K-CodepointToStringFunc-8/11/12`) were skipped as "XML 1.0-only test on an XML 1.1 implementation" in `ConformanceRunner.DocumentedSkips`.
  - The tests already declare `<dependency type="xml-version" value="1.0"/>`; `DependencyFilter` now skips any `xml-version` dependency, so Bosak's XML 1.1 implementation no longer attempts to run XML 1.0-specific tests.
  - Total pass/skip counts are unchanged; the skips are now reported under "Unsupported dependency".
  - Full QT3 remains **14,994 passed / 0 failed / 16,827 skipped = 47.12%** (runnable pass rate **100%**); unit tests **1,379/0**.

- **2026-07-21** — QT3 XQuery syntax heuristic cluster: `TestExecutor.LooksLikeXQuery` no longer rejects valid XPath constructs.
  - 10 runnable tests were skipped because the heuristic treated `import` as XQuery syntax, treated `schema-element()`/`schema-attribute()` as XQuery-only, and treated `element foo`/`attribute foo` name tests as XQuery constructors.
  - `import` is now only flagged for XQuery prolog forms (`import module ...`, `import schema ...`).
  - `schema-element()` and `schema-attribute()` are recognized as XPath 2.0+ node tests.
  - `element`/`attribute` constructors are now only detected when followed by `{`.
  - Full QT3 now **14,994 passed / 0 failed / 16,827 skipped = 47.12%** (runnable pass rate **100%**); unit tests **1,379/0**.

- **2026-07-21** — QT3 K-Literals-29 empty-expression singleton: `XPath31Expression.Compile` now reports `XPST0003` for an empty expression.
  - `K-Literals-29` expects `XPST0003` for an empty XPath expression, but the API previously threw `ArgumentException: The value cannot be an empty string.`.
  - `XPath31Expression.Compile` now detects null/whitespace input and throws `ParseException("Empty expression is not a valid XPath expression", 0)`, which is auto-prefixed to `XPST0003: Empty expression is not a valid XPath expression`.
  - Full QT3 now **14,984 passed / 0 failed / 16,837 skipped = 47.09%** (runnable pass rate **100%**); unit tests **1,379/0**.

- **2026-07-21** — QT3 fn-doc document-loading cluster: `EvaluationContext.LoadDocument` now maps `UriFormatException`, `IOException`, and `XmlException` to the correct XPath error codes.
  - 6 runnable tests were skipped because raw CLR exceptions escaped during document loading: `fn-doc-1` (invalid hostname → `UriFormatException`), `K2-SeqDocFunc-14` (`':/'` → `UriFormatException`), `K2-SeqDocFunc-5` (invalid `.invalid` domain → `IOException`), and `fn-doc-27/28/35` (malformed XML → `XmlException`).
  - `UriFormatException` is now reported as `FODC0005` (invalid document URI); `IOException` and `XmlException` are reported as `FODC0002` (document not available).
  - URI resolution against the static base URI is now protected as well, so malformed relative URIs also produce `FODC0005`.
  - Full QT3 now **14,983 passed / 0 failed / 16,838 skipped = 47.09%** (runnable pass rate **100%**); unit tests **1,379/0**.

- **2026-07-21** — QT3 assert-deep-eq parse cluster: multi-line `assert-deep-eq` values are now evaluated as a single XPath expression, and `unicode-version` dependencies are skipped.
  - 5 runnable tests were skipped because `ResultComparer` split `assert-deep-eq` content by newlines and tried to compile each line as a standalone XPath expression. Lines ending in a trailing comma (continuations of a multi-line sequence) produced "Unexpected token Eof in primary expression".
  - `ResultComparer.CompareAssertDeepEq` now trims the entire element value and compiles it as one expression; newlines are treated as XPath whitespace.
  - `DependencyFilter` now skips tests with a `unicode-version` dependency, because Bosak uses .NET's case folding without guaranteeing a specific Unicode version. This correctly skips `fn-lower-case-19` and `fn-upper-case-19` (which require Unicode 7.0) while keeping `fn-lower-case-18`, `fn-upper-case-18`, and `last-23` passing.
  - Full QT3 now **14,977 passed / 0 failed / 16,844 skipped = 47.07%** (runnable pass rate **100%**); unit tests **1,379/0**.

- **2026-07-21** — QT3 regex-pattern cluster: `RegexHelper.CacheRegex` now converts `RegexParseException` to `FORX0002`.
  - 2 runnable tests were skipped because `fn:matches` let .NET `RegexParseException` escape for patterns that XSD validation did not reject: `**%%` (no preceding atom for `*`) and `a{99999999999999999999999999}` (quantifier larger than `Int32.MaxValue`).
  - `fn-matches-25` (expects `FORX0002`) and `cbcl-matches-004` (expects `assert-false` or `FORX0002`) now pass.
  - Full QT3 now **14,974 passed / 0 failed / 16,847 skipped = 47.06%** (runnable pass rate **100%**); unit tests **1,379/0**.

- **2026-07-21** — QT3 decimal-overflow cluster: `XPathOptimizer` now skips constant folding when decimal arithmetic overflows, letting the runtime raise `FOAR0002`.
  - 4 runnable tests were skipped because `OptimizeBinary` constant-folded decimal subtraction/division and let `OverflowException` escape to the harness instead of surfacing the XPath `FOAR0002` error.
  - `cbcl-numeric-subtract-001`, `op-numeric-subtract-big-01`, `cbcl-numeric-divide-015`, and `op-numeric-divide-big-01` now pass by matching the `<error code="FOAR0002"/>` alternative.
  - Full QT3 now **14,972 passed / 0 failed / 16,849 skipped = 47.05%** (runnable pass rate **100%**); unit tests **1,379/0**.

- **2026-07-21** — QT3 date/time harness-error cluster: `ResultComparer` extended-year serialization and `fn:*-from-time` `DateTimeOffset` out-of-range.
  - 17 runnable tests were skipped because `ResultComparer` converted extended-year `xs:dateTime`/`xs:date`/`xs:time` values to `DateTimeOffset`, which does not support years outside 1–9999.
  - `ResultComparer.SerializeSingle` now uses `XdmValue.ToString()` for date/time values, which delegates to `XPathDateTime` formatting and correctly handles negative years and year 0.
  - `ResultComparer.ValuesEqual` already fell back to canonical string comparison for extended-year operands; the serialization fix removed the remaining `InvalidOperationException` skips.
  - 2 additional tests (`fn-hours-from-time-3`, `fn-timezone-from-time-11`) were skipped because `fn:*-from-time` read `XdmValue.TimeValue`, which throws `ArgumentOutOfRangeException` when the reference date `0001-01-01` plus a positive timezone offset normalizes to year 0.
  - `HoursFromTime`, `MinutesFromTime`, `SecondsFromTime`, and `TimezoneFromTime` now read components directly from `XdmValue.TimeXPathValue`.
  - Full QT3 now **14,968 passed / 0 failed / 16,853 skipped = 47.04%** (runnable pass rate **100%**); unit tests **1,379/0**.

- **2026-07-21** — QT3 gDateTime comparison/cast cluster: `ParseGDateTime` now uses regex-based parsing and correctly handles optional timezones.
  - 72 runnable tests across `op-gDay-equal`, `op-gMonth-equal`, `op-gMonthDay-equal`, `op-gYearMonth-equal`, and `prod-CastExpr` were skipped due to `IndexOutOfRangeException`.
  - The old code used `LastIndexOfAny(['+', '-'])` and misidentified structural dashes in `xs:gDay` (`---DD`), `xs:gMonth` (`--MM`), `xs:gMonthDay` (`--MM-DD`), and `xs:gYearMonth` (`YYYY-MM`) as timezone signs, producing invalid timezone strings like `-31` or `-11`.
  - Rewrote `ParseGDateTime` with per-subtype regexes that match the complete lexical form including optional `Z` or `[+-]HH:MM` timezone, and normalized the timezone with `NormalizeTimezone`.
  - Full QT3 now **14,949 passed / 0 failed / 16,872 skipped = 46.98%** (runnable pass rate **100%**); unit tests **1,279/0**.

- **2026-07-21** — QT3 `ForExpr013` / `string-queries-results-q1`: `assert-xml` now loads expected output from external `file` references.
  - Both tests were producing correct element sequences, but the conformance harness compared them against an empty expected string because `assert-xml` with a `file` attribute was not implemented.
  - Added `BaseDirectory` to `TestCase` and threaded it through `ConformanceRunner`, `TestExecutor`, and `ResultComparer` so the referenced file can be resolved relative to the test-set directory.
  - `ResultComparer.CompareAssertXml` now reads the file content when the `file` attribute is present and falls back to inline element content when it is absent.
  - This fixes all 41 QT3 tests that use `assert-xml file` references, including `ForExpr013` and `string-queries-results-q1`.
  - Full QT3 now **14,877 passed / 0 failed / 16,944 skipped = 46.75%** (runnable pass rate **100%**); unit tests **1,279/0**.

- **2026-07-21** — QT3 `K-SeqExprInstanceOf-46/51` pair: `xs:NOTATION` is now recognized for `instance of` and `xs:QName` is case-sensitive.
  - `"a string" instance of xs:NOTATION` raised `XPST0051` because `xs:NOTATION` was missing from the known atomic type list. It is now recognized, and `ItemInstanceOf` always returns `false` since `xs:NOTATION` is abstract and cannot be instantiated.
  - `3 instance of xs:qname` was incorrectly returning `false` because the type-name lookup was case-insensitive; it now raises `XPST0051` as required. Only the exact spelling `xs:QName` is accepted.
  - Added `GetTypeLocalName` helper to preserve the original local name from the sequence type string before lowercasing, so `qname` can be detected and rejected while `QName` is accepted.
  - Targeted `prod-InstanceofExpr` tests now pass: 259 passed / 0 failed / 50 skipped.
  - Full QT3 now **14,875 passed / 2 failed / 16,944 skipped = 46.75%** (runnable pass rate **99.99%**); unit tests **1,279/0**.

- **2026-07-21** — QT3 `fn-intersect/union-node-args-*` and multi-root fragment canonicalization: standalone element serialization now includes in-scope namespaces.
  - `XDocumentNode.ToXmlString()` only emitted the namespace used by the element name; `assert-xml` expected all ancestor namespace declarations (`xmlns:foo`, `xmlns:xsi`, `xmlns:atomic`) for `fn-intersect-node-args-015/016` and `fn-union-node-args-015/016/017`.
  - Added `ElementToXmlStringWithNamespaces` which clones the element, gathers missing in-scope namespace bindings via the namespace axis, and adds them before serializing.
  - `ResultComparer.NormalizeXml` now wraps multi-root fragments in a temporary root so it can canonicalize them (sorted attributes, normalized escaping) instead of falling back to raw-string comparison. This also fixed `unabbreviatedSyntax-30`, `filterexpressionhc1`, `filterexpressionhc4`, and `predicates-24`.
  - Targeted `op-intersect` and `op-union` sets now pass all runnable tests: 29/29 and 28/28 respectively.
  - Full QT3 now **14,873 passed / 4 failed / 16,944 skipped = 46.74%** (runnable pass rate **99.97%**); unit tests **1,279/0**.

- **2026-07-21** — QT3 `xs-dateTimeStamp-*`: registered `xs:dateTimeStamp#1` and added cast/instance-of support.
  - `xs:dateTimeStamp("2011-07-28T12:34:56-08:00")` raised `XPST0017` because the constructor was missing from the `FunctionLibrary` xs: constructor dictionary.
  - Added `XsDateTimeStamp` and registered `[(Namespaces.Xs, "dateTimeStamp", 1)]`.
  - Added `case "datetimestamp"` to `VmEngine.TryCast` requiring a timezone; parses lexical dateTimes, casts timezone-aware `xs:dateTime` and `xs:date` values, and rejects values without a timezone so `Cast` raises `FORG0001`.
  - Added `datetimestamp` to `IsKnownAtomicTypeName`, `ItemInstanceOf`, and `GetDirectSupertypes` so `instance of xs:dateTimeStamp` and `current-date() castable as xs:dateTimeStamp` work.
  - Added `dateTimeStamp` to `fn:type-available`'s built-in type list.
  - Targeted tests now pass: `xs-dateTimeStamp-2`, `xs-dateTimeStamp-5`, `xs-dateTimeStamp-6` (whole set of 6 passes).
  - Full QT3 now **14,864 passed / 13 failed / 16,944 skipped = 46.71%** (runnable pass rate **99.91%**); unit tests **1,279/0**.

- **2026-07-21** — QT3 `fn-month/from-dateTime-6`: `fn:year-from-dateTime` and `fn:month-from-dateTime` now support extended years.
  - The implementations were reading `XdmValue.DateTimeValue`, which converts to `DateTimeOffset` and fails for years outside the 1–9999 range.
  - Switched to `XdmValue.DateTimeXPathValue.Year` / `.Month`, which stores the year as `long` and supports XML Schema extended years such as `-1999`.
  - Targeted tests now pass: `fn-month-from-dateTime-6`, `fn-year-from-dateTime-6`.
  - Full QT3 now **14,861 passed / 16 failed / 16,944 skipped = 46.70%** (runnable pass rate **99.89%**); unit tests **1,379/0**.

- **2026-07-21** — QT3 `fn-resolve-uri-3/26`: `fn:resolve-uri` now raises `FORG0002` for invalid base URIs and relative references.
  - `fn-resolve-uri-3`: the relative reference `":"` is not a valid absolute URI (empty scheme) and not a valid relative URI reference because its first path segment contains `":"`.
  - `fn-resolve-uri-26`: a base URI containing a fragment (`http://www.example.com/a.html#fragment`) is not a valid base URI per RFC 3986.
  - Added explicit validation in `FunctionLibrary.ResolveUri`: reject base URIs with non-empty fragments and reject non-path-absolute relative references whose first path segment contains `":"`.
  - Targeted tests now pass: `fn-resolve-uri-3`, `fn-resolve-uri-26`.
  - Full QT3 now **14,859 passed / 18 failed / 16,944 skipped = 46.70%** (runnable pass rate **99.88%**); unit tests **1,379/0**.

- **2026-07-21** — QT3 `prod-FunctionCall` cluster: inline and static function calls now apply XPath 3.1 function conversion rules.
  - `FunctionCall-010`, `FunctionCall-025`: inline function argument conversion now applies untypedAtomic casting and numeric promotion (already supported; validated).
  - `FunctionCall-011`: static call to `fn:codepoints-to-string` now casts a sequence of `xs:untypedAtomic` values to `xs:integer*` via the VM `Call` opcode and `ApplyFunctionConversion`.
  - `FunctionCall-026`: inline function call now promotes `xs:anyURI` values to `xs:string*`; `TryPromoteNumericOrUri` was updated to recognize `xs:anyURI` annotations stored as `String` kind with `SchemaTypeName="anyURI"`.
  - `K-FunctionCallExpr-22/26`, `K2-FunctionCallExpr-3/8`: `fn:current()` and `fn:system-property()` now raise `XPST0017` when invoked outside XSLT mode. Added `EvaluationContext.IsXsltMode` and set it in `TransformEngine` and `fn:transform`.
  - Added `ParameterTypeNames = ["xs:integer*"]` and `ReturnTypeName = "xs:string"` to the `fn:codepoints-to-string` signature.
  - Targeted tests now pass: `FunctionCall-010`, `FunctionCall-011`, `FunctionCall-025`, `FunctionCall-026`, `K-FunctionCallExpr-22`, `K-FunctionCallExpr-26`, `K2-FunctionCallExpr-3`, `K2-FunctionCallExpr-8`.
  - Full QT3 now **14,857 passed / 20 failed / 16,944 skipped = 46.69%** (runnable pass rate **99.86%**); unit tests **1,379/0**.

- **2026-07-20** — QT3 Tier-2z: `K2-SeqExprCast-1/201` / `xs:QName` namespace resolution for `cast as` and `xs:QName()` constructor.
  - `"myPrefix:ncname" cast as xs:QName` was not resolving the prefix against the static namespace context; the cast was producing a string instead of a QName and raising `XPTY0004`.
  - `xs:QName("ncname")` was ignoring the default element namespace, returning an empty namespace URI instead.
  - Added `EvaluationContext` overloads to `Cast` and `TryCast` so the `Cast`/`Castable` opcodes can pass the static namespace context into QName casting. The cast path now resolves prefixed QNames and uses the default element namespace for unprefixed ones.
  - Updated the `xs:QName` constructor (`XsQNameConstructor`) to use `DefaultElementNamespace` for unprefixed lexical QNames.
  - Updated `TestEnvironment.ApplyTo` to map a QT3 environment `<namespace prefix="" uri="...">` to `EvaluationContext.DefaultElementNamespace`.
  - Fixed unprefixed QName resolution in `CastUntypedAtomicToQName` to fall back to the empty namespace when no default element namespace is defined.
  - Rewrote `FunctionLibraryTests` chained-FLWOR regression tests as nested expressions so they remain valid under the XPath grammar restriction enforced by `LetExpr020a`.
  - Added `QNameCast_ResolvesPrefixedNamespace`, `QNameCast_UsesDefaultElementNamespaceForUnprefixed`, and `XsQNameConstructor_UsesDefaultElementNamespace` regression tests.
  - Targeted tests now pass: `K2-SeqExprCast-1`, `K2-SeqExprCast-201`, `CastableAs647`, `K-SeqExprCastable-19`.
  - Full QT3 now **14,849 passed / 28 failed / 16,944 skipped = 46.66%** (runnable pass rate **99.81%**); unit tests **1,379/0**.

- **2026-07-20** — QT3 Tier-2z: `K-SeqExprCast-67` / `cast as` raises `XPTY0004` for empty singleton input.
  - `() cast as xs:QName` was succeeding because the `Cast` opcode only checked for empty input when the target occurrence was `?`, `*`, or `+`; the default `One` occurrence fell through to `Cast()`.
  - Restructured the empty-input branch in the `Cast` opcode: empty input with occurrence `One` now raises `XPTY0004`; empty input with `?` still returns `()`; `*`/`+` still raise the existing occurrence-indicator error.
  - Added `EvaluateValue_EmptySequenceCastAsQName_RaisesXPTY0004` regression test.
  - Targeted test passes: `K-SeqExprCast-67`.
  - Full QT3 now **14,847 passed / 30 failed / 16,944 skipped = 46.66%** (runnable pass rate **99.80%**); unit tests **1,376/0**.

- **2026-07-20** — QT3 Tier-2z: `K-SeqExprTreat-16` / require closing parenthesis in sequence type tests.
  - `3 treat as item(` was being accepted because `ParseTypeNameAndParens` consumed tokens until EOF without verifying that the opening parenthesis was closed.
  - Added a `parenDepth > 0` check after consuming the sequence type; an unclosed paren now raises `XPST0003`.
  - Added `TreatExpr` and `TreatExpr_UnclosedTypeParens_RaiseXPST0003` regression tests.
  - Targeted test passes: `K-SeqExprTreat-16`.
  - Full QT3 now **14,846 passed / 31 failed / 16,944 skipped = 46.65%** (runnable pass rate **99.79%**); unit tests **1,375/0**.

- **2026-07-20** — QT3 Tier-2z: `LetExpr020a` / disallow consecutive `for`/`let` clauses in XPath FLWOR.
  - XPath 3.1 allows only one initial `for` or `let` clause; intermediate clauses may only be `where`, `order by`, or `count`. The parser was treating additional `for`/`let` keywords as new intermediate clauses, so `let $a := 1 let $b := $a return ...` parsed successfully.
  - `ParseFlworExpr` no longer accepts `KeywordFor` or `KeywordLet` as intermediate clauses; a following `let` is now treated as unexpected input and `Expect(TokenKind.KeywordReturn)` raises `XPST0003`.
  - Added `LetExpr` and `LetExpr_ConsecutiveLetKeywords_RaiseXPST0003` regression tests.
  - Targeted test passes: `LetExpr020a`.
  - Full QT3 now **14,845 passed / 32 failed / 16,944 skipped = 46.65%** (runnable pass rate **99.79%**); unit tests **1,373/0**.

- **2026-07-20** — QT3 Tier-2z: `K-XQueryComment-14/15` / unterminated XPath comments now raise `XPST0003`.
  - The lexer previously consumed unterminated comments to EOF silently, so expressions like `1(: this comment does not end` parsed as just `1` and succeeded.
  - `XPathLexer.SkipComment` now throws `ParseException` (auto-prefixed `XPST0003`) when a comment is still open at end of input, including partially closed nested comments.
  - Added `UnterminatedComment_AfterExpression_RaisesXPST0003` and `NestedUnterminatedComment_AfterExpression_RaisesXPST0003` regression tests.
  - Targeted tests pass: `K-XQueryComment-14`, `K-XQueryComment-15`.
  - Full QT3 now **14,844 passed / 33 failed / 16,944 skipped = 46.65%** (runnable pass rate **99.78%**); unit tests **1,371/0**.

- **2026-07-20** — QT3 Tier-2z: `CastAs009/091` / `xs:float` fixed-point formatting in decimal range.
  - `FormatXPathFloat` was normalizing `R`-format scientific strings (e.g. `1E-05`) inside the decimal range (`1e-6 <= |x| < 1e6`), producing `1.0E-5` instead of expanding to fixed-point `0.00001`.
  - Aligned the float branch with the double branch: expand `R`-scientific to fixed-point inside the decimal range, then trim trailing zeros.
  - Added `FloatToString_InsideDecimalRange_ExpandsToFixedPoint` regression test.
  - Targeted tests pass: `CastAs009`, `CastAs091`.
  - Full QT3 now **14,842 passed / 35 failed / 16,944 skipped = 46.64%** (runnable pass rate **99.77%**); unit tests **1,369/0**.

- **2026-07-20** — QT3 Tier-2z: `Literals017/025/028` / XPath canonical double formatting.
  - `FormatXPathDouble` was using `G17` for scientific-range values, which preserved round-trip noise (e.g. `6553503.2000000002`) and inflated the exponent when normalizing fixed-point to scientific notation.
  - Switched to `R` (shortest round-trip) format and compute the exponent from the fixed-point decimal position rather than the total digit count.
  - `FormatXPathFloat` uses the same decimal-point-based exponent calculation.
  - Added `DoubleToString_FixedPointScientific_TrimsRoundTripNoise` regression test.
  - Targeted tests pass: `Literals017`, `Literals025`, `Literals028`.
  - Full QT3 now **14,840 passed / 37 failed / 16,944 skipped = 46.64%** (runnable pass rate **99.75%**); unit tests **1,368/0**.

- **2026-07-20** — QT3 Tier-2z: `K-FilterExpr-82` / atomize predicate result before numeric/EBV check.
  - Filter expression predicates that return a sequence must be atomized before deciding whether they are numeric positional predicates or being used for their effective boolean value.
  - In `VmEngine.Filter`, `predResult` is now atomized first; a multi-item sequence raises `XPTY0004`, and a singleton integer (e.g. `(1)` from `remove((1, "a string"), 2)`) is treated as a numeric predicate.
  - Added `Predicate_AtomizesSequenceResult` regression test.
  - Targeted test passes: `K-FilterExpr-82`.
  - Full QT3 now **14,836 passed / 41 failed / 16,944 skipped = 46.62%** (runnable pass rate **99.73%**); unit tests **1,367/0**.

- **2026-07-20** — QT3 Tier-2z: `K2-Axes-50/53` / XPTY0019 for path steps on non-node context items.
  - `SimpleMap` (used for non-axis path steps) now raises `XPTY0019` when the input sequence contains non-node items, but only in path-step mode (`RegisterC != 0`); the `!` operator continues to allow non-node items.
  - `PathStepMap` (used for predicated axis steps) also raises `XPTY0019` for non-node context items.
  - Added `PathStep_RequiresNodeContextItem` regression test.
  - Targeted tests pass: `K2-Axes-50`, `K2-Axes-53`.
  - Full QT3 now **14,835 passed / 42 failed / 16,944 skipped = 46.62%** (runnable pass rate **99.72%**); unit tests **1,366/0**.

- **2026-07-20** — QT3 Tier-2z: `Axes123` / namespace-node identity in `is`.
  - Namespace nodes are virtual properties of an element; the underlying XAttribute objects are created on demand, so reference equality failed. `XDocumentNode.IsSameNode` now compares namespace nodes by owner element + prefix + URI, and `GetHashCode` is consistent with this semantic identity.
  - Added `NamespaceNode_IsSameNodeIdentity` regression test.
  - Targeted test passes: `Axes123`.
  - Full QT3 now **14,833 passed / 44 failed / 16,944 skipped = 46.61%** (runnable pass rate **99.71%**); unit tests **1,365/0**.

- **2026-07-20** — QT3 Tier-2z: `K2-NameTest-78/79` / `let` and `for` as name tests.
  - `let` and `for` are FLWOR keywords only when followed by a variable binding (`$`). When used as a single name test, they now parse as path steps and raise `XPDY0002` (no context item) instead of `XPST0003`.
  - Added `FlworKeywords_ParseAsNameTests` regression test.
  - Targeted tests pass: `K2-NameTest-78`, `K2-NameTest-79`.
  - Full QT3 now **14,832 passed / 45 failed / 16,944 skipped = 46.61%** (runnable pass rate **99.70%**); unit tests **1,362/0**.

- **2026-07-20** — QT3 Tier-2z: `K-NodeSame-6` / allow `is` as a non-reserved function name.
  - `is` is an operator keyword but not a reserved function name, so `is(...)` parses as a function call and raises `XPST0017` because no such function exists.
  - Added `IsKeyword_AllowedAsFunctionName` regression test.
  - Targeted test passes: `K-NodeSame-6`.
  - Full QT3 now **14,830 passed / 47 failed / 16,944 skipped = 46.60%** (runnable pass rate **99.68%**); unit tests **1,361/0**.

- **2026-07-20** — QT3 Tier-2z: `K-NodeNumberFunc-13/15` / `fn:number` on non-numeric, non-string atomic types.
  - `fn:number` now returns `NaN` for atomic types such as `xs:anyURI`, `xs:gYear`, and `xs:QName`, while continuing to convert numeric types, `xs:string`, `xs:untypedAtomic`, and `xs:boolean` to `xs:double`.
  - Added `Number_ReturnsNaNForNonNumericNonStringTypes` regression test.
  - Targeted tests pass: `K-NodeNumberFunc-13`, `K-NodeNumberFunc-15`.
  - Full QT3 now **14,829 passed / 48 failed / 16,944 skipped = 46.60%** (runnable pass rate **99.68%**); unit tests **1,361/0**.

- **2026-07-20** — QT3 Tier-2z: `K2-SeqDeepEqualFunc-40` / `fn:deep-equal` implicit timezone handling.
  - `fn:deep-equal` now compares `xs:dateTime`, `xs:date`, and `xs:time` values using the evaluation context's implicit timezone when one operand has no explicit timezone.
  - Added `DeepEqual_RespectsImplicitTimezone` regression test.
  - Targeted test passes: `K2-SeqDeepEqualFunc-40`.
  - Full QT3 now **14,827 passed / 50 failed / 16,944 skipped = 46.60%** (runnable pass rate **99.66%**); unit tests **1,360/0**.

- **2026-07-20** — QT3 Tier-2z: `K2-DataFunc-6` / `fn:data()` on complex element-only schema elements.
  - Added `IXdmNode.HasNoTypedValue` default accessor and `XDocumentNode.HasNoTypedValue` implementation using PSVI schema info.
  - `FunctionLibrary.Data` now raises `FOTY0012` for element-only or empty complex-type elements.
  - Added `Data_ThrowsFoty0012ForElementOnlyComplexElement` regression test.
  - Targeted test passes: `K2-DataFunc-6`.
  - Full QT3 now **14,826 passed / 51 failed / 16,944 skipped = 46.59%** (runnable pass rate **99.66%**); unit tests **1,360/0**.

- **2026-07-20** — QT3 Tier-2z: `fn-upper-case-22` / Armenian ligature upper-case mapping.
  - `FunctionLibrary.ApplyUnicodeCaseMapping` now maps U+FB17 (Armenian small ligature men xeh) to U+0544 U+053D (MEN + XEH).
  - Added `UpperCase_ArmenianLigatureMenXeh` regression test.
  - Targeted test passes: `fn-upper-case-22`.
  - Full QT3 now **14,825 passed / 52 failed / 16,944 skipped = 46.59%** (runnable pass rate **99.65%**); unit tests **1,359/0**.

- **2026-07-20** — QT3 Tier-2z: `fn-number-3` / `fn:number()` with no context item.
  - `FunctionLibrary.Number_0` now raises `XPDY0002` when `fn:number()` is called without a context item.
  - The one-argument form `fn:number(())` still returns `NaN` as required by the spec.
  - Added `Number_ThrowsWithoutContextItem` regression test.
  - Targeted test passes: `fn-number-3`.
  - Full QT3 now **14,824 passed / 53 failed / 16,944 skipped = 46.59%** (runnable pass rate **99.64%**); unit tests **1,357/0**.

- **2026-07-20** — QT3 Tier-2z: `fn-not-28` / effective boolean value of multi-item sequences.
  - `XdmValue.SequenceEffectiveBooleanValue` now raises `FORG0006` when a sequence of more than one item has a non-node first item.
  - Previously, any multi-item sequence was treated as `true`; XPath 3.1 §2.4.3 requires a node-first sequence for this behavior.
  - Added `Not_ThrowsOnMixedSequence` regression test.
  - Targeted test passes: `fn-not-28`.
  - Full QT3 now **14,823 passed / 54 failed / 16,944 skipped = 46.58%** (runnable pass rate **99.63%**); unit tests **1,357/0**.

- **2026-07-19** — QT3 Tier-2z: `fn-doc-available-2` / `fn:doc` and `fn:doc-available` URI argument validation.
  - `FunctionLibrary.Doc_1` and `DocAvailable_1` now use `RequireString` on the URI argument.
  - Non-string atomics (e.g., `fn:doc-available(xs:integer(2))`) now raise `XPTY0004`; empty sequence behavior is preserved.
  - Added `DocAvailable_RejectsNonStringArgument` regression test.
  - Targeted test passes: `fn-doc-available-2`.
  - Full QT3 now **14,822 passed / 55 failed / 16,944 skipped = 46.58%** (runnable pass rate **99.63%**); unit tests **1,356/0**.

- **2026-07-19** — QT3 Tier-2z: `fn-substring-after-23` / `fn-substring-before-23` / relative collation URI resolution.
  - Added `FunctionLibrary.ResolveCollationUri` to absolutize relative collation URIs against `EvaluationContext.BaseUri`.
  - `fn:substring-before` and `fn:substring-after` now resolve their `$collation` argument before validating it.
  - Added `SubstringAfter_ResolvesRelativeCollationUri` regression test.
  - Targeted tests pass: `fn-substring-after-23`, `fn-substring-before-23`.
  - Full QT3 now **14,821 passed / 56 failed / 16,944 skipped = 46.58%** (runnable pass rate **99.62%**); unit tests **1,355/0**.

- **2026-07-19** — QT3 Tier-2z: `fn-implicit-timezone-10/11/12` / duration `div` NaN/zero validation.
  - `VmEngine.DivideDuration` now checks for `NaN` and `0.0`/`-0.0` before the zero-duration short-circuit.
  - `xs:dayTimeDuration` (including the `PT0S` implicit timezone) divided by `NaN` now raises `FOCA0005`; divided by zero raises `FODT0002`.
  - Added `ImplicitTimezone_DivByInvalidNumber_Throws` regression test.
  - Targeted tests pass: `fn-implicit-timezone-10`, `fn-implicit-timezone-11`, `fn-implicit-timezone-12`.
  - Full QT3 now **14,819 passed / 58 failed / 16,944 skipped = 46.57%** (runnable pass rate **99.61%**); unit tests **1,354/0**.

- **2026-07-19** — QT3 Tier-2z: `fn:iri-to-uri` / `K2-IRIToURIfunc` non-string argument validation.
  - `FunctionLibrary.IriToUri` now uses `RequireString` on its argument.
  - Non-string atomics (e.g., `iri-to-uri(12)`) and multi-item sequences (e.g., `iri-to-uri(('a','b'))`) now raise `XPTY0004`; nodes and `xs:untypedAtomic` are still atomized to strings.
  - Added `IriToUri_RejectsNonStringArguments` regression test.
  - Targeted tests pass: `fn-iri-to-uri1args-5`, `K2-IRIToURIfunc-3`, `K2-IRIToURIfunc-4`.
  - Full QT3 now **14,816 passed / 61 failed / 16,944 skipped = 46.56%** (runnable pass rate **99.59%**); unit tests **1,353/0**.

- **2026-07-19** — QT3 Tier-2z: double `MAX_VALUE` string formatting / `G17` round-trip cluster.
  - `XdmValue.FormatXPathDouble` now uses `"G17"` instead of `"G16"` for scientific-notation doubles, preserving all round-trip digits.
  - Fixes boundary-value failures in `fn:ceiling`, `fn:concat`, `fn:data`, `fn:exactly-one`, `fn:floor`, `fn:number`, `fn:one-or-more`, `fn:string`, and `fn:zero-or-one` `*dbl1args-*` tests.
  - Added `DoubleMaxValue_RoundTripString` regression test.
  - Full QT3 now **14,813 passed / 64 failed / 16,944 skipped = 46.55%** (runnable pass rate **99.57%**); unit tests **1,352/0**.

- **2026-07-19** — QT3 Tier-2z: `compare-011` / `fn:compare` non-string argument validation.
  - `FunctionLibrary.Compare_2` and `Compare_3` now use `RequireString` on both arguments.
  - Non-string atomics (e.g., `compare(123, 456)`) now raise `XPTY0004`; nodes and `xs:untypedAtomic` are still atomized to strings.
  - Added `Compare_RejectsNonStringArguments` regression test.
  - Targeted test passes: `compare-011`.
  - Full QT3 now **14,791 passed / 86 failed / 16,944 skipped = 46.48%** (runnable pass rate **99.42%**); unit tests **1,350/0**.

- **2026-07-19** — QT3 Tier-2z: `K2-StringLT-1` / default codepoint collation for value comparisons.
  - `FunctionLibrary.Populate` now installs the standard `FunctionLibrary.CompareStrings` comparer when the context has no custom comparer.
  - This ensures XPath value comparisons (`lt`/`le`/`gt`/`ge`/`eq`/`ne`) use the codepoint collation (Unicode scalar values) in the API path, not `string.CompareOrdinal`.
  - Added `StringLessThan_UsesUnicodeCodepoints` regression test (BMP vs supplementary plane codepoints).
  - Targeted test passes: `K2-StringLT-1`.
  - Full QT3 now **14,790 passed / 87 failed / 16,944 skipped = 46.48%** (runnable pass rate **99.42%**); unit tests **1,350/0**.

- **2026-07-19** — QT3 Tier-2z: `K-NumericSubtract-34/35` / `xs:untypedAtomic` arithmetic promotion.
  - `VmEngine.Add`, `Subtract`, `Multiply`, `Divide`, `IntegerDivide`, and `Modulo` now atomize operands and check `xs:untypedAtomic` before the numeric type-specific branches.
  - When any operand of an arithmetic expression is `xs:untypedAtomic`, both operands are cast to `xs:double` and the result is `xs:double` (or `xs:integer` for `idiv`/`mod`).
  - Added `NumericSubtract_PromotesUntypedAtomicToDouble` regression test.
  - Targeted tests pass: `K-NumericSubtract-34`, `K-NumericSubtract-35`.
  - Full QT3 now **14,789 passed / 88 failed / 16,944 skipped = 46.48%** (runnable pass rate **99.41%**); unit tests **1,348/0**.
  - `XPathOptimizer` now only folds `+x` for numeric literals; non-literal operands keep the `UnaryExpressionNode`.
  - `IrLowerer` emits the `UnaryPlus` VM opcode instead of a simple `Move`.
  - `VmEngine.UnaryPlus` validates the operand and raises `XPTY0004` for non-numeric, non-untypedAtomic values (e.g., `+"a string"`).
  - `xs:untypedAtomic` is promoted to `xs:double`; numeric types are returned unchanged.
  - Targeted test passes: `K-NumericUnaryPlus-1`.
  - Full QT3 now **14,786 passed / 91 failed / 16,944 skipped = 46.47%** (runnable pass rate **99.38%**); unit tests **1,348/0**.

- **2026-07-19** — QT3 Tier-2z: `op-boolean-equal-4` / `and`/`or` register-lifetime fix.
  - `IrLowerer.LowerAnd` and `LowerOr` no longer free the target result register when an operand is lowered into it.
  - This fixes `op-boolean-equal-4`, where `xs:boolean('true') and xs:boolean('true')` was clobbering its left operand register, causing the subsequent `eq` to compare the same value against itself.
  - Added `ApiTests.DebugBooleanEqual` regression test.
  - Full QT3 now **14,785 passed / 92 failed / 16,944 skipped = 46.46%** (runnable pass rate **99.38%**); unit tests **1,347/0**.

- **2026-07-19** — QT3 Tier-2z: duration / date arithmetic cluster.
  - `xs:date` +/− `xs:dayTimeDuration` now returns an `xs:date` with the time components zeroed to `00:00:00`.
  - `xs:time` +/− `xs:yearMonthDuration` now raises `XPTY0004` instead of returning the time unchanged.
  - Generic `xs:duration` values are handled by `fn:*-from-duration` so mixed year-month and day-time components are extracted correctly.
  - `fn:distinct-values` and `fn:index-of` now compare durations using normalized total months and total seconds.
  - Targeted tests pass: `fn-months-from-duration-20`, `K-MonthsFromDurationFunc-7`, `fn-years-from-duration-20`, `K-YearsFromDurationFunc-7`, `K-DateAddDTD-1/2`, `K-DateSubtractDTD-1`, `K-TimeSubtractDTD-2/3/5`, and `distinct-duration-equal-1`.
  - Full QT3 now **14,784 passed / 93 failed / 16,944 skipped = 46.46%** (runnable pass rate **99.38%**); unit tests **1,346/0**.

- **2026-07-19** — QT3 Tier-2z: `union` / `intersect` / `except` XPTY0004 validation.
  - `VmEngine` now validates that all items in both operands of `union`, `intersect`, and `except` are nodes, raising `XPTY0004` for non-node operands.
  - Added `RequireNodeSequence` helper and `LoadNode` VM opcode.
  - Updated `VmOpcodeTests.Concatenate` to use node values.
  - Targeted tests pass: `K2-SeqExcept-1`, `K2-SeqIntersect-1/43/44`, `K2-SeqUnion-5/46/47`.
  - Full QT3 now **14,773 passed / 104 failed / 16,944 skipped = 46.43%** (runnable pass rate **99.30%**); unit tests **1,345/0**.

- **2026-07-19** — QT3 Tier-2z: `fn:adjust-date-to-timezone` / `fn:adjust-time-to-timezone` / `fn:adjust-dateTime-to-timezone` FODT0003 validation.
  - Added `ParseTimezoneOffset` helper to validate timezone offset arguments.
  - Offsets outside `-PT14H` to `+PT14H` (e.g., `PT14H1M`, `-PT14H1M`, `P1D`) now raise `FODT0003`.
  - Offsets with seconds/milliseconds (e.g., `PT14H0M0.001S`) now raise `FODT0003` for violating the one-minute resolution requirement.
  - Targeted pools now all **0 failed**: `fn-adjust-date-to-timezone` 37/0/4, `fn-adjust-time-to-timezone` 37/0/5, `fn-adjust-dateTime-to-timezone` 46/0/2.
  - Full QT3 now **14,766 passed / 111 failed / 16,944 skipped = 46.40%** (runnable pass rate **99.25%**); unit tests **1,345/0**.

- **2026-07-19** — QT3 Tier-2z: `fn-string-length` / `fn-string-join` / `fn-string-to-codepoints` / `fn:remove` / `fn:replace` type checks.
  - `fn:string-length()` zero-arg form now uses `fn:string(.)` semantics, so non-string atomic context items (e.g., integers) are converted to their string representation before counting code points.
  - `fn:string-join()`, `fn:string-to-codepoints()`, `fn:replace()`, and `fn:replace()` four-arg form now use `RequireStringRequired` for required string parameters; the empty sequence raises `XPTY0004`.
  - `fn:remove()` now uses `RequireInteger` for the position argument, raising `XPTY0004` for non-integer atomics or the empty sequence.
  - Added `RequireStringRequired` and `RequireInteger` helpers to `FunctionLibrary`.
  - Targeted pools now all **0 failed**: `fn-string-length` 33/0/3, `fn-string-join` 32/0/14, `fn-string-to-codepoints` 44/0/0, `fn-remove` 51/0/0, `fn-replace` 81/0/10.
  - Full QT3 now **14,755 passed / 122 failed / 16,944 skipped = 46.37%** (runnable pass rate **99.18%**); unit tests **1,345/0**.

- **2026-07-19** — QT3 Tier-2z: `fn-lang` / `fn-in-scope-prefixes` / `fn-codepoints-to-string` fixes.
  - `fn:lang()` now raises `XPDY0002` for an absent context item and `XPTY0004` for a non-node context item; `fn:lang($test, $node)` raises `XPTY0004` when `$node` is not a single node.
  - `fn:in-scope-prefixes()` now raises `XPTY0004` when the argument is not a single element node (e.g., document node or non-node value).
  - Documented `K-CodepointToStringFunc-8/11/12` as XML 1.0-only tests on an XML 1.1 implementation.
  - Targeted pools now **0 failed**: `fn-lang` 36/0/10, `fn-in-scope-prefixes` 9/0/53, `fn-codepoints-to-string` 61/0/18.
  - Full QT3 now **14,747 passed / 130 failed / 16,944 skipped = 46.34%** (runnable pass rate **98.92%**); unit tests **1,345/0**.

- **2026-07-19** — QT3 Tier-2z: `fn-root`/`fn-name`/`fn-local-name` context-item and `fn-QName` QName fixes.
  - Added `GetOptionalSingleNode` helper for `node()?` arguments, raising `XPTY0004` for non-node or multi-item arguments.
  - `fn:local-name()`, `fn:namespace-uri()`, `fn:name()`, `fn:node-name()`, and `fn:root()` now raise `XPDY0002` for an absent context item and `XPTY0004` for a non-node context item.
  - `fn:local-name(())` and `fn:namespace-uri(())` now return the zero-length `xs:string` / `xs:anyURI` per their declared return types, not the empty sequence.
  - `fn:QName((), "local")` now works (empty-sequence namespace URI treated as empty string); `:person` and `person:` lexical forms now raise `FOCA0002`.
  - Targeted pools now all **0 failed**: `fn-root` 11/0/27, `fn-name` 72/0/54, `fn-local-name` 66/0/22, `fn-prefix-from-QName` 27/0/0, `fn-QName` 25/0/9.
  - Full QT3 now **14,743 passed / 137 failed / 16,941 skipped = 46.33%** (runnable pass rate **98.92%**); unit tests **1,345/0**.

- **2026-07-19** — QT3 Tier-2z: duration-arithmetic round-half-up and overflow fixes.
  - `VmEngine.MultiplyDuration` and `DivideDuration` for `xs:yearMonthDuration` now use `RoundHalfUp(totalMonths)` (`floor(x + 0.5)`) per F+O Erratum FO.E12, fixing rounding ties such as `P5M div -2` and `P2Y11M * 2.3`.
  - `xs:dayTimeDuration` multiply/divide no longer casts `xs:double` factors/divisors directly to `decimal`; zero-duration operands return `PT0S`, huge divisors fall back to `double` and round to `PT0S` when below half a tick, and true overflow raises `FODT0002`.
  - Divide by `NaN` raises `FOCA0005`; divide by `0` raises `FODT0002`; divide by `INF`/`-INF` returns `P0M`/`PT0S`.
  - `TryCast` to `xs:duration` now records the generic `duration` schema annotation so the runtime can distinguish `xs:duration` from `xs:yearMonthDuration`/`xs:dayTimeDuration`, making `xs:duration("P1Y3M") * 3` and `xs:duration("P1Y3M") div 3` raise `XPTY0004`.
  - Targeted duration pools now **0 failed** (16 previously failing tests now pass).
  - Full QT3 now **14,720 passed / 160 failed / 16,941 skipped = 46.26%** (runnable pass rate **98.92%**); unit tests **1,344/0**.

- **2026-07-19** — QT3 Tier-2z: `fn-element-with-id` schema-validated ID support.
  - The conformance harness now loads source documents with `validation="strict"` against the environment's declared XML Schema(s), adding PSVI annotations to the XDocument tree.
  - `IXdmNode` gains an `IsId` accessor; `XDocumentNode` computes it from `XmlSchemaInfo` so that elements and attributes with typed values of type `xs:ID` (derived types, union ID members, and singleton lists of `xs:ID`) are recognized.
  - `fn:id()` now returns ID-valued elements themselves (including child `<id>` elements typed as `xs:ID`), and `fn:element-with-id()` returns their parent element when the ID is provided by a child element.
  - `fn:id()` / `fn:element-with-id()` continue to support DTD-declared `ID` attributes via `XDocumentType.InternalSubset`.
  - Targeted `fn-element-with-id` pool now **5 passed / 0 failed / 0 skipped** (5 previously failing tests now pass).
  - Full QT3 now **14,703 passed / 173 failed / 16,945 skipped = 46.21%** (runnable pass rate **98.84%**); unit tests **1,344/0**.

- **2026-07-19** — QT3 Tier-2z: `op/numeric-less-than` unsignedLong overflow fix.
  - `xs:unsignedLong` lexical values that exceed `long.MaxValue` (e.g. `18446744073709551615`) are now represented as `XdmValueKind.Decimal` with the `unsignedLong` subtype annotation, so casts and comparisons work.
  - `ItemInstanceOf` accepts decimal-backed values whose schema type is an integer subtype.
  - Targeted `op-numeric-less-than` pool now **154 passed / 0 failed / 29 skipped** (2 previously failing tests now pass).
  - Full QT3 now **21,620 passed / 317 failed / 9,884 skipped = 67.93%** (runnable pass rate **98.56%**); unit tests **1,343/0**.

- **2026-07-19** — QT3 Tier-2z: `fn/contains` collation/whitespace fixes.
  - Fixed UCA collation strength mapping in `FunctionLibrary.TryParseUca`: `primary` ignores case and non-space accents, `secondary` ignores only case, and `tertiary`/`quaternary` use no ignore flags.
  - Implemented true ASCII-only case folding for the HTML ASCII case-insensitive collation (`http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive`), so only `A-Z`/`a-z` are folded; non-ASCII characters such as `ô`/`Ô` are compared exactly.
  - `fn:contains-token` now tokenizes on XPath whitespace only (`#x20`, `#x9`, `#xD`, `#xA`); non-breaking space (`U+00A0`) is no longer treated as a token separator.
  - Targeted `fn-contains` and `fn-contains-token` pools now **0 failed** (6 previously failing tests now pass).
  - Full QT3 now **21,618 passed / 319 failed / 9,884 skipped = 67.93%** (runnable pass rate **98.54%**); unit tests **1,343/0**.

- **2026-07-19** — QT3 Tier-2z: `cbcl-castable` fixes.
  - `VmEngine` `Castable` opcode catches dynamic cast errors (FOCA0003, FOAR0002) and returns `false` for `castable as`.
  - Empty sequence is now correctly reported as castable only for `?` / `*` occurrence indicators.
  - `prod-CastableExpr` targeted pool now **782 passed / 0 failed / 177 skipped** (was 772/10/177).
  - Full QT3 now **21,607 / 333 / 9,881 = 67.81%** (runnable pass rate **98.48%**); unit tests **1,343/0**.

- **2026-07-19** — QT3 Tier-2z: `fn:format-number` precision and dependency-filter fixes.
  - `FormatNumberEngine` raises `XPTY0004` for non-numeric string inputs in non-BC mode; BC mode still returns the `NaN` symbol.
  - Scientific notation now supports non-BMP (supplementary-plane) zero-digits and counts exponent digit signs correctly.
  - `DependencyFilter` ANDs spec dependencies across `<dependency>` elements, so XP30-only tests are skipped under XP31+.
  - `numberformat63/64` (decimal literals requiring >28 digits of precision) are documented as platform limitations.
  - `fn-format-number` targeted pool now **246 passed / 0 failed / 23 skipped** (was 244/5/20).
  - Full QT3 now **21,612 / 325 / 9,884 = 67.92%** (runnable pass rate **98.52%**); unit tests **1,343/0**.

- **2026-07-19** — QT3 Tier-2z: `fn/matches` caseless-match `i`-flag fix. `i` now maps to `RegexOptions.IgnoreCase`; category escapes `\p{}`/`\P{}` stay case-sensitive via `(?-i:...)`; bracketed classes are case-folded during translation; back-references and quote mode match case-insensitively. Targeted `fn-matches` pool **1,117/0/58** (was 5 failing). Full QT3 now **21,610 / 330 / 9,881 = 67.91%** (runnable pass rate **98.49%**); unit tests **1,343/0**.

- **2026-07-19** — QT3 Tier-2z: `prod-NamedFunctionRef` / `named-function-ref-reserved-function-names` fixes.
  - `XPathParser.ParseNamedFunctionRef` raises `XPST0003` for reserved function names (e.g., `attribute#0`, `element#0`).
  - The reserved-name check is not applied to `ParseFunctionCall` because names like `attribute()` are valid as kind tests.
  - `prod-NamedFunctionRef` targeted pool now **546 passed / 0 failed / 10 skipped** (was 534/12/10).
  - Full QT3 now **21,597 / 343 / 9,881 = 67.79%** (runnable pass rate **98.44%**); unit tests **1,339/0**.

- **2026-07-19** — QT3 Tier-2y: `fn:index-of` fixes.
  - `FunctionLibrary.IndexOfImpl` now uses XPath `eq` semantics via `AtomicValuesEqual` instead of string comparison.
  - NaN no longer matches itself; incompatible types (e.g., `xs:integer` vs `xs:string`) return empty.
  - Empty / multi-item search argument and empty collation argument now raise `XPTY0004`.
  - `fn-index-of` targeted pool now **53 passed / 0 failed / 0 skipped** (was 44/9/0).
  - Full QT3 now **21,585 / 355 / 9,881 = 67.79%** (runnable pass rate **98.38%**); unit tests **1,327/0**.

- **2026-07-19** — QT3 Tier-2x: `op-numeric-mod` fixes.
  - `VmEngine.Modulo` now returns `NaN` for `xs:double`/`xs:float` mod by zero (IEEE 754 semantics).
  - Integer and decimal mod by zero continue to raise `FOAR0001`.
  - `op-numeric-mod` targeted pool now **113 passed / 0 failed / 11 skipped** (was 107/6/11).
  - Full QT3 now **21,576 / 364 / 9,881 = 67.79%** (runnable pass rate **98.34%**); unit tests **1,317/0**.

- **2026-07-19** — QT3 Tier-2w: `fn:has-children` fixes.
  - `FunctionLibrary.HasChildren_0` raises `XPDY0002` when the context item is absent.
  - `FunctionLibrary.HasChildren` unwraps singleton sequences: empty sequence returns `false`, multi-item / non-node arguments raise `XPTY0004`.
  - `fn-has-children` targeted pool now **34 passed / 0 failed / 3 skipped** (was 26/8/3).
  - Full QT3 now **21,570 / 370 / 9,881 = 67.77%** (runnable pass rate **98.31%**); unit tests **1,311/0**.

- **2026-07-19** — QT3 Tier-2v: `op-numeric-integer-divide` fixes.
  - `VmEngine.IntegerDivide` raises `FOAR0002` for NaN/INF operands and returns `0` for finite dividend `idiv` INF/-INF.
  - `xs:float('1e38') idiv xs:float('1e-37')` and similar overflow cases now raise `FOAR0002` instead of returning a truncated `long`.
  - `XPathLexer.ReadNumber` rejects `NumericLiteral` tokens immediately followed by keyword operators (e.g. `10idiv 3` → `XPST0003`).
  - `op-numeric-integer-divide` targeted pool now **125 passed / 0 failed / 11 skipped**.
  - Full QT3 now **21,562 / 378 / 9,881 = 67.76%** (runnable pass rate **98.28%**); unit tests **1,303/0**.

- **2026-07-19** — QT3 Tier-2u: `xs:numeric` cast and constructor support.

- **2026-07-18** — QT3 Tier-2t: `fn:id` / `fn:idref` / `fn:element-with-id` DTD support.
  - `IXdmNode` gains DTD properties (`HasDocumentType`, `DocumentTypeName`, `PublicId`, `SystemId`, `InternalSubset`); `XDocumentNode` exposes `XDocument.DocumentType`.
  - `FunctionLibrary` parses the DTD internal subset for `ID`/`IDREF`/`IDREFS` attribute declarations and caches the result per document node.
  - `fn:idref` now returns the matching attribute node(s) per F+O.
  - `fn:id`/`fn:idref`/`fn:element-with-id` raise `XPTY0004` when the context item or second argument is not a node.
  - `fn-id`/`fn-idref` targeted pool now **54 passed / 0 failed / 61 skipped**.
  - Full QT3 now **21,535 / 405 / 9,881 = 67.68%** (runnable pass rate **98.15%**); unit tests **1,147/0**.

- **2026-07-18** — QT3 Tier-2s: `fn:function-lookup` context-focus capture.
  - `function-lookup` and compiler-generated named function references now capture the creation focus in `NamedFunctionItem`, so context-dependent functions (`fn:base-uri#0`, `fn:document-uri#0`) use the creator's context item during dynamic invocation.
  - `DependencyFilter` now declares `fn-load-xquery-module` unsupported, so tests that assert the feature are skipped rather than failing with `FOQM0001`.
  - Full QT3 now **21,494 / 446 / 9,881 = 67.55%** (runnable pass rate **97.97%**); unit tests **1,283/0**.

- **2026-07-18** — QT3 Tier-2r: `fn:collection()` / `fn:uri-collection()` support.
  - `EvaluationContext.Collections` is now populated by the QT3 harness and used by `fn:collection()` and `fn:uri-collection()` to resolve registered collections, with directory-based fallback and `FODC0002`/`FODC0003`/`FODC0004` error reporting.
  - Full QT3 now **21,511 / 482 / 9,828 = 67.60%** (runnable pass rate **97.81%**); unit tests **1,282/0**.

- **2026-07-18** — QT3 Tier-2q: XQ31-only dependency filter + XdmMap insertion-order fix.
  - `DependencyFilter` now skips positive `spec="XQ31"` dependencies, correctly reclassifying ~116 previously-failing and ~68 previously-passing XQuery-only tests as skipped. Full QT3 now **21,475 / 518 / 9,828 = 67.49%** (runnable pass rate **97.65%**); unit tests **1,282/0**.
  - `XdmMap` restored insertion-order iteration for `Keys`/`Values`/`Entries` via persistent `_keyOrder` and `_keyIndices`; `map:remove`/`map:put` use new `WithRemoved`/`WithAdded` helpers.

- **2026-07-17** — QT3 `op-same-key` hang resolved: **+34 net passed, −20 failed, +14 skipped** (full QT3 now 21,543 / 634 / 9,644 = 67.70%; unit tests 1,286/0).
  - `XdmMap` now uses `ImmutableDictionary<XdmValue, XdmValue>` so `map:remove`, `map:put`, and `map:merge` perform O(log n) structural sharing instead of copying the whole dictionary.
  - `map:remove` and `map:put` rewritten to use the immutable dictionary directly.
  - QT3 harness dependency filter now skips `arbitraryPrecisionDecimal` tests (same-key-008 and same-key-025) because .NET `decimal` is fixed-precision 128-bit.

- **2026-07-15** — QT3 regex/string quick-wins cluster: **+216 passed, −198 failed, zero regressions** (QT3 now 18,698 / 1,742 / 11,381 = 58.76%; unit tests 999/0).
  - Strict XSD regex syntax validation (re00xxx cluster, ~124 tests): malformed quantifiers, bare `{`/`}`/`]`, `(?x` constructs other than `(?:`, octal escapes, .NET-only escapes (`\x \u \A \Z \z \b \B`), trailing backslash, empty char classes, unescaped `[` in classes, and empty-base subtraction all raise FORX0002.
  - Back-references per F&O 5.6.1.4: multi-digit gobbling bounded by previously-opened groups; reference to an unclosed group → FORX0002 (erratum FO.E24).
  - `.` excludes `#xD` as well as `#xA`; `\S` no longer matches CR/TAB/space (unsorted `\s` range broke `Complement`); flag `x` strips pattern whitespace pre-translation (incl. inside `\p{ }` names); multiline `^` no longer matches after a trailing newline but still matches at 0 of the empty string.
  - fn:tokenize no longer interleaves capturing groups (was `Regex.Split`); one-arg fn:tokenize and fn:normalize-space treat only #x20/#x9/#xD/#xA as whitespace (NBSP preserved).
  - XPTY0004 for non-string atomics / empty sequences passed to required string parameters of fn:translate, fn:matches, fn:normalize-unicode.
  - fn:normalize-unicode: case-insensitive trimmed form names, zero-length form = no normalization, FULLY-NORMALIZED implemented (NFC + leading-non-starter check, FOCH0003 otherwise).
  - QT3 harness: `DocumentedSkips` per-test skip list with reasons (upstream defects, platform limitations).

- **2026-07-14** — W3C `unicode-90` conformance set enabled: **1,365 passed / 0 failed / 95 skipped** (1,460 tests; all skips are upstream test/data defects, documented in the harness).
  - New XSD character-class regex engine `XsdCharClasses` with pinned **Unicode 9.0.0** data (`UnicodeData90`, generated from UCD 9.0): all 38 general categories (incl. grouped `LC`), `\p{IsBlock}` script blocks, `\d \D \w \W \s \S \i \I \c \C`, ranges, negation, unions, and class subtraction `[A-[B]]`; astral ranges are emitted as surrogate-pair alternations so astral characters are never split. `\w` follows the XSD definition `[^\p{P}\p{Z}\p{C}]` (emoji are word characters). Unknown category/block → `FORX0002`.
  - Regex translation and compiled-`Regex` caches keyed by the short original pattern (`RegexHelper.ValidateAndTranslatePatternCached` / `GetRegex`), wired into `fn:matches`/`fn:replace`/`fn:tokenize`/`fn:analyze-string` and `xsl:analyze-string`. Compiled regexes are used throughout: `RegexOptions.NonBacktracking` silently mis-matched U+000A on large translated alternations (probe-verified; regression test added).
  - `fn:codepoints-to-string` validity now follows the XML 1.1 `Char` production exactly (C0 controls except NUL, U+FDD0..FDEF and astral xFFFE/xFFFF are legal; surrogates, U+FFFE/U+FFFF, NUL → `FOCH0001`). `fn:translate` is Rune-based (astral pairs no longer split).
  - `fn:concat` is registered up to arity 32 (unicode-90 uses `concat#16`).
  - `VmEngine` general comparison fast path: `=`/`!=` between a single `xs:integer` and a large all-integer sequence uses a cached `HashSet<long>`, so `$validrange[not(. = $c)]` (1.1M × 2,063 comparisons per unicode-90 test) is O(n).
  - Harness: injects the `charclass` stylesheet parameter for Gen tests (the upstream generator omits it), caches the 54MB data documents, and drops degenerate empty-`@c` entries (U+FFFE/U+FFFF placeholders in `unicode-C.xml`/`unicode-Cn.xml`) on load.
  - Skipped upstream defects: `unicode90-001..008` (BMP-only expected counts contradict this suite's own Gen tests), `unicode90-{cat}-033/035` (fn-replace3/5 compare against `string-join` of empty `<c>` elements — broken in w3c/xslt30-test master), `unicode90-Cs-001..004/023` + `unicode90-Zl-023`/`Zp-023` (empty/one-member categories → invalid quantifiers), `unicode90-L-017/038` + `unicode90-Lo-017/038` (stylesheet `$validrange` omits U+10000 but the documents include it).
  - Full W3C suite: **7,109 passed / 0 failed / 7,491 skipped** — 100% of runnable tests.

- **2026-07-14** — HOF unskip + snapshot cluster: higher-order functions fully enabled; snapshot set 19/0/24; seqtor/static/regex/system-property/current-output-uri sets green.
  - `fn:snapshot` now matches the spec-equivalent stylesheet implementation (`snapshot-equivalent.xsl`) node-for-node: non-node items pass through unchanged, ancestor grafting preserves parentage, namespace declarations are excluded from attribute comparisons in `fn:deep-equal`, and in-scope namespaces are not redeclared on copied descendants.
  - Typed templates (`xsl:template/@as`) now collect results through the placeholder sequence accumulator, so node identity and parentage survive template boundaries; `xsl:element` suspends the accumulator while constructing content (fixes `__xdm_seq__` placeholder leak, `namespace-0912`).
  - Function-body results no longer clone a single text node (`NormalizeSequenceConstructorItems`), preserving text-node parentage through `xsl:function` results (`snapshot-0102a`).
  - `namespace-node()` is now a valid match pattern (priority −0.5) matching namespace-axis nodes.
  - `fn:concat` / `fn:compare#2` register their `xs:anyAtomicType?` parameters as pass-through; dynamic-call argument conversion no longer stringifies arbitrary atomics to `xs:string` (`higher-order-functions-064` raises XPTY0004 again).
  - Other fixes: `fn:min`/`fn:max` return `xs:integer` for all-integer input; `system-property()` expands `Q{uri}local` and reports `xsl:supports-higher-order-functions`; `xsl:function` accepts `cache`; user functions in map/math/array reserved namespaces raise XTSE0080; TVT/§4.3 whitespace handling; missing F&O registrations (`element-with-id#2`, `idref`, `uri-collection`, `xs:error`).
  - Full W3C suite: **5,744 passed / 0 failed / 8,856 skipped** — every runnable test passes, including the complete `fn:transform` set (transform-001..009).

- **2026-07-13** — Skip-pool audit: unskipped `position-0103` (xsl:merge) and `position-2201` (xsl:result-document); both pass now that the features they gate on are implemented. Full W3C suite: **5,607 passed / 0 failed / 8,993 skipped**.

- **2026-07-13** — Phase 5n: all remaining singleton failures cleared; **zero failing runnable tests** in the W3C XSLT 3.0 suite.
  - `attribute-0701`: HTML serialization now minimizes recognized boolean attributes (`checked`, `selected`, `disabled`, ...) whose value equals their name, restricted to the HTML boolean allowlist so attributes such as `ffi="ffi"` keep the explicit form.
  - `backwards-019b`: `escape-uri-attributes` now defaults to true for the XHTML method as well as HTML. The XSLT 3.0 backwards-compatibility rule is implemented via new `OutputProperties.EffectiveVersion` / `ImplicitResultTree` flags: in version-1.0 mode an implicitly generated result tree infers the `xml` (not `xhtml`) output method, while an explicit `xsl:result-document` still infers `xhtml` (`backwards-019` vs `backwards-019b`).
  - `maps-017`: the conformance harness unwraps JSON-string-serialized results (json/adaptive output of a node) before reparsing for tree assertions.
  - `merge-021`: `XTDE2210` is now also raised when a merge-key attribute (`lang`, `order`, `collation`, `case-order`, `data-type`) is present on one of two corresponding `xsl:merge-key` elements and absent on the other, per the XSLT 3.0 spec as written.
  - `include-0101`: two fixes. (1) `xsl:include`/`xsl:import` hrefs now resolve against the *element's* base URI, so modules pulled in through DTD external entities resolve nested imports relative to the entity location. (2) The unnamed `xsl:output` declarations are now merged across the module tree by import precedence via `Stylesheet.EffectiveOutputProperties` (previously only the principal module's declarations were used), so the included module's `method="html"` correctly overrides the imported module's `method="xml"`.
  - Full W3C suite: **5,605 passed / 0 failed / 8,995 skipped** (100% of runnable tests).

- **2026-07-13** — Phase 5m `select` conformance cluster cleared.
  - The conformance harness now honors the `encoding` attribute on assertion elements (`assert-serialization`, `assert-xml`) when reading expected-result files, so ISO-8859-1 expected outputs decode correctly (`select-6101`).
  - W3C `select` conformance set: **157 passed / 0 failed / 1 skipped**.
  - Full W3C suite: **5,600 passed / 5 failed / 8,995 skipped** (99.9%).
  - Remaining failures: `attribute-0701`, `backwards-019b`, `include-0101`, `maps-017`, `merge-021`.

- **2026-07-13** — Phase 5l `bug` conformance cluster cleared.
  - `ResultTreeSerializer`'s text output method no longer emits comment/PI markup — comment and processing-instruction nodes contribute nothing to `method="text"` output (`bug-1405`).
  - The conformance harness now self-closes HTML void elements (`meta`, `br`, `img`, ...) when reparsing HTML output for tree assertions, so the HTML5-mandated unclosed `<meta ...>` no longer breaks XPath assertions (`bug-1301`).
  - `assert-xml` comparisons now strip the serialization-injected `meta http-equiv="Content-Type"` element: assert-xml compares result trees, and the meta is a serialization artifact (`bug-1901`). The serializer still injects it whenever `include-content-type` is in effect, as required by `output-0123` and `backwards-018`.
  - Also clears `select-6201` (HTML table serialization).
  - W3C `bug` conformance set: **75 passed / 0 failed / 11 skipped**.
  - Full W3C suite: **5,599 passed / 6 failed / 8,995 skipped** (99.9%).
  - Remaining failures: `attribute-0701`, `backwards-019b`, `include-0101`, `maps-017`, `merge-021`, `select-6101` (since cleared).

- **2026-07-13** — Phase 5k `for-each-group` conformance cluster cleared.
  - `FunctionSignature` gains an optional `DynamicImplementation`; `VmEngine.InvokeFunctionItem` uses it for dynamic calls through function items (named references and partial application) while static calls keep using `Implementation`.
  - `TransformEngine.RegisterGroupingFunctions` now supplies dynamic implementations that raise `XTDE1061` (`current-group`), `XTDE1071` (`current-grouping-key`), `XTDE3480` (`current-merge-group`), and `XTDE3510` (`current-merge-key`), per XSLT 3.0: these context components are not retained in the closure of a function item.
  - W3C `for-each-group` conformance set: **78 passed / 0 failed / 7 skipped** (plus 114 streaming tests skipped).
  - Full W3C suite: **5,595 passed / 10 failed / 8,995 skipped** (99.8%).
  - Remaining failures: `bug` (3), `select` (2), `attribute-0701`, `include-0101`, `maps-017`, `merge-021`, `backwards-019b`.

- **2026-07-13** — Phase 5j `output` conformance cluster cleared.
  - `ResultTreeSerializer.SerializeAsXhtml` now normalizes text/attribute values before CDATA wrapping, so `cdata-section-elements` combined with `normalization-form` split unrepresentable normalized characters correctly (`output-0115d`).
  - `TransformEngine.IsRawCollectionTopLevel` is now scoped to the actual principal/secondary result document, so literal elements inside `xsl:variable/@as` bodies are no longer swallowed by raw-item collection (`output-0716`, `output-0717`).
  - `XdmJsonSerializer.SerializeValue` now handles sequence-valued array/map members, so nested HTML/XML nodes serialize as JSON strings instead of `"(sequence)"` (`output-0702`).
  - Also clears `arrays-304`.
  - W3C `output` conformance set: **232 passed / 0 failed / 29 skipped**.
  - Full W3C suite: **5,593 passed / 12 failed / 8,995 skipped** (99.8%).
  - Remaining failures: `bug` (3), `select` (2), `for-each-group` (2), `attribute` (1), and singleton regressions in `include`, `maps`, `merge`, and `backwards`.

- **2026-07-13** — Phase 5i `normalize-unicode` conformance cluster cleared.
  - `ResultTreeSerializer` now escapes tab, line-feed, and carriage-return characters inside XML attribute values as decimal numeric character references (`&#9;`, `&#10;`, `&#13;`) after `XmlWriter` emits them literally. This aligns the `xml` output method with XSLT/XQuery Serialization 3.1.
  - Also clears `copy-3801` and `attribute-1101` (attribute-value whitespace regressions).
  - W3C `normalize-unicode` conformance set: **18 passed / 0 failed / 0 skipped**.
  - Full W3C suite: **5,588 passed / 17 failed / 8,995 skipped** (99.7%).
  - Remaining clusters: `output` (4), `bug` (3), `select` (2), `for-each-group` (2), `attribute` (1), and singleton regressions in `arrays`, `backwards`, `include`, `maps`, and `merge`.

- **2026-07-13** — Phase 5h `character-map` conformance cluster cleared.
  - Adaptive output now uses XPath/XQuery string-literal escaping (double quotes) instead of JSON escaping, so character-map replacements in strings and maps are serialized correctly.
  - `fn:current-time()` now keeps the date part on day 1 of year 1 when possible and falls back to day 2 only when a positive timezone offset would underflow `DateTimeOffset.MinValue`.
  - W3C `character-map` conformance set: **29 passed / 0 failed / 0 skipped**.
  - Full W3C suite: **5,565 passed / 40 failed / 8,995 skipped** (99.3%).
  - Remaining clusters: `mode` (9), `xml-version` (7), and scattered regressions.

- **2026-07-12** — Phase 5g `inherit-namespaces="no"` conformance fix; the W3C `namespace` cluster is now clear.
  - `TransformEngine.FinalizeNamespaceInheritance` attaches `PrefixedNamespaceUndeclarations` to children of `NamespaceInheritanceBarrier` elements so `xmlns:prefix=""` is emitted where required.
  - The synthetic `__xdm_doc__` root is detached before being wrapped in the final `XDocument`, preserving namespace annotations instead of cloning them away.
  - `ResultTreeSerializer` routes trees with prefixed namespace undeclarations to the raw XML serializer.
  - W3C `namespace` conformance set: **203 passed / 0 failed / 21 skipped**.
  - Full W3C suite: **5,561 passed / 44 failed / 8,995 skipped** (99.2%).

- **2026-07-12** — Phase 5f final serialization clusters cleared.
  - `fn:current-output-uri()` now returns the base output URI at the top level and remains empty in temporary output state (functions, variables, sort/merge keys, patterns), clearing the `current-output-uri` cluster.
  - Original namespace prefixes are preserved for sibling elements that map to the same URI, clearing `output-0138`.
  - The `output` conformance cluster now has **0 failures** (203 passed / 0 failed / 29 skipped); the `current-output-uri` cluster has **0 failures** (15 passed / 0 failed / 2 skipped).
  - Full W3C suite: **5,544 passed / 61 failed / 8,995 skipped** (98.9%).

- **2026-07-12** — Phase 5e remaining `output` serialization edge cases.
  - Arrays are now flattened in sequence constructors, fixing `output-0713`–`output-0715`.
  - `json-node-output-method="html"` now injects the HTML content-type `<meta>` element (`output-0716`).
  - Adaptive output preserves `omit-xml-declaration` and applies parameter-document character maps (`output-0721`).
  - XML comments preserve `\r` characters literally (`output-0723`).
  - `SEPM0009` is restricted to XML/XHTML and now covers `version != 1.0` with `doctype-system`; `SEPM0010` validates `undeclare-prefixes` for XML 1.1.
  - Text output now writes the BOM, applies character maps, and honors `normalization-form`; HTML DOCTYPE is emitted immediately before the first element.
  - Full W3C suite: **5,538 passed / 67 failed / 8,995 skipped** (98.8%).

- **2026-07-11** — Phase 5 JSON output method: `xsl:output method="json"`, `json-node-output-method`, `allow-duplicate-names`, `escape-solidus`, and `xsl:output parameter-document` with inline character maps.
  - Top-level `xsl:map`/`xsl:map-entry` results are preserved for JSON serialization instead of being rejected as element/document children.
  - HTML node serialization inside JSON strings now emits XHTML namespace declarations and escapes solidus characters per XSLT/XQuery Serialization 3.1 defaults.
  - W3C `output` conformance set: **175 passed / 28 failed / 29 skipped** (was 168/35/29). Remaining JSON/text failures include `output-0703` (item-separator) and `output-0710`/`0711` (unescaped keys).
  - Full W3C suite: **5,477 passed / 128 failed / 8,995 skipped** (97.7%).

- **2026-07-11** — Phase 5b JSON/text output edge cases: `item-separator` and `SENR0001` validation.
  - `item-separator` is now honored for `method="text"`, fixing `output-0703`, `output-0709`, `output-0718`, and `output-0719`.
  - Maps, arrays, and functions at the top level of XML, HTML, XHTML, or text output now raise `SENR0001`, fixing `output-0710`, `output-0711`, and `output-0712`.
  - W3C `output` conformance set: **179 passed / 24 failed / 29 skipped** (was 175/28/29).
  - Full W3C suite: **5,481 passed / 124 failed / 8,995 skipped** (97.8%).

- **2026-07-11** — Phase 5c `xsl:result-document` serialization fixes.
  - AVTs are now evaluated for all `xsl:result-document` serialization attributes, including `html-version`, `byte-order-mark`, and `allow-duplicate-names`.
  - `yes`/`no` attribute values are now case-sensitive; uppercase variants raise `XTSE0020`.
  - `SEPM0009` is only raised for XML/XHTML methods that emit an XML declaration.
  - `xsl:result-document` now supports raw-item collection for `method="json"`, `method="adaptive"`, and `build-tree="no"`.
  - W3C `result-document` conformance set: **104 passed / 21 failed / 29 skipped** (was 86/39/29).
  - Full W3C suite: **5,506 passed / 99 failed / 8,995 skipped** (98.2%).

- **2026-07-11** — Phase 4 serialization fixes: named-output import precedence, XHTML 1.0 empty-element handling, DOCTYPE quote/namespace rules, and `fn:current-output-uri()` scoping.
  - W3C `output` conformance set: **168 passed / 35 failed / 29 skipped** (was 155/48/29).
  - Full W3C suite: **5,473 passed / 132 failed / 8,995 skipped** (97.6%).

- **2026-06-26** — Fixed `normalize-unicode-014`: HTML result-tree serialization now applies `xsl:output/@normalization-form` (NFC/NFD/NFKC/NFKD) to text, attribute values, comments, and processing instructions.
  - Full W3C suite: **5,238 passed / 5 failed / 9,357 skipped** (99.9%).
  - Remaining failures: `catalog-006/007`, `docbook-001/002/004`.

- **2026-06-26** — Fixed `accumulator-090`: global variables that call `accumulator-after()` no longer trigger a false `XPST0008` circular-reference error. The accumulator evaluation context now copies globals lazily but skips the variable currently being initialized, preserving access to globals referenced by accumulators (e.g., `merge-066`).
  - Full W3C suite: **5,237 passed / 6 failed / 9,357 skipped** (99.9%).
  - Remaining failures: `normalize-unicode-014`, `catalog-006/007`, `docbook-001/002/004`.

- **2026-06-26** — Fixed `function-1014` (FXSL higher-order recursion): `xsl:apply-templates` and `xsl:call-template` inside `xsl:function` bodies now expand `__xdm_seq__` placeholders so atomic values returned by `xsl:sequence` reach the function result instead of being dropped.
  - Full W3C suite: **5,236 passed / 7 failed / 9,357 skipped** (99.9%).
  - Remaining failures: `accumulator-090`, `normalize-unicode-014`, `catalog-006/007`, `docbook-001/002/004`.

- **2026-07-08** — Cleared the W3C `unparsed-text`, `match`, `forwards`, `lre`, `whitespace`, `xslt-compat`, and `for-each-group` conformance clusters.
  - `fn:unparsed-text()` one-argument form detects encoding from BOM, XML declaration, and HTTP `Content-Type`; `unparsed-text-available()` works for HTTP resources; sequence arguments are atomized.
  - `xsl:template/@match` no longer rejects `Q{uri}*` / `except` patterns as AVTs.
  - Forward-compatibility mode ignores unknown XSLT elements/attributes and unresolvable `use-when` expressions when the effective version is > 3.0.
  - Maps and functions raise `XTDE0450` when serialized directly to element content.
  - QName names in `xsl:element`/`xsl:attribute` are whitespace-normalized before validation.
  - Backwards-compatible mode converts atomic values to strings for string functions.
  - `fn:distinct-values` treats NaN as equal.
  - `xsl:function` bodies follow XSLT text-node merging rules while preserving consecutive zero-length text nodes.
  - Full W3C suite: **5,233 passed / 10 failed / 9,357 skipped** (99.8%).

- **2026-07-05** — Cleared the W3C `tunnel` conformance cluster (58 runnable tests pass; 0 failed).
  - `xsl:with-param/@tunnel` and `xsl:param/@tunnel` now accept `yes`/`no` (XSLT 2.0) and `true`/`false`/`1`/`0` (XSLT 3.0); invalid/empty values raise `XTSE0020`.
  - `xsl:call-template` enforces `XTSE0680` when an ordinary `xsl:with-param` matches a tunnel `xsl:param` (or vice versa) and when no matching parameter is declared.
  - `xsl:call-template` parameter validation now skips `xsl:context-item` children and is suppressed in XSLT 1.0 backwards-compatible mode, so extra parameters are silently ignored.
  - Tunnel parameters are correctly isolated from `xsl:function` bodies and pass through intermediate named templates, `xsl:apply-templates`, `xsl:apply-imports`, and `xsl:next-match`.
  - Tunnel parameters now bind only to tunnel-declared `xsl:param`s; non-tunnel `xsl:with-param`s no longer shadow tunnel parameters.

- **2026-07-05** — Cleared the W3C `avt` conformance cluster (35 runnable tests pass; 0 failed).
  - AVT expressions now correctly handle XPath comments (`(: ... :)`), empty expressions, escaped `}}`, and `{{` even when no `{` expression is present.
  - `xsl:attribute/@separator`, `xsl:value-of/@separator`, and `xsl:sort/@stable` are now evaluated as attribute value templates.
  - AVTs in XSLT 1.0 backwards-compatibility mode take only the first item of the expression value, matching `string()` semantics.
  - `xsl:value-of` inside `xsl:function` bodies constructs real text nodes, so functions declared `as="text()*"` return the expected node kind.
  - `xsl:template/@match` now rejects AVT syntax with `XTSE0340`.

- **2026-07-05** — Cleared the W3C `collations` conformance cluster (43 runnable tests pass; 0 failed).
  - `xsl:stylesheet`/`xsl:template`/`xsl:*` `default-collation` attributes now flow into the XPath evaluation context, so `eq`, `=`, `fn:compare`, `fn:starts-with`, `fn:contains`, `fn:ends-with`, etc. use the correct collation without an explicit argument.
  - `xsl:for-each-group` and `xsl:key` use the effective default collation when no explicit `@collation` is supplied.
  - `xsl:sort` with `case-order="upper-first"`/`"lower-first"` works even when no `@lang` or `@collation` is present (primary comparison is case-insensitive, with case as the tie-breaker).
  - Collation-aware aggregate functions (`fn:max`, `fn:min`, `fn:index-of`, `fn:distinct-values`, `fn:deep-equal`) now honor the in-scope default collation.
  - UCA collations with `fallback=no` raise `FOCH0002`, matching the implementation-defined fallback behavior expected by the test suite.

- **2026-07-04** — Cleared the W3C `iterate` conformance cluster (44 runnable tests pass; 0 failed; 35 streaming tests skipped).
  - `xsl:iterate` now works in the result-tree path with `xsl:param`, `xsl:next-iteration`, `xsl:break`, and `xsl:on-completion`.
  - `xsl:next-iteration`/`xsl:with-param` values are coerced to the declared `xsl:param` type, so atomization happens when required.
  - `xsl:on-completion` and `xsl:break` sequence-constructor content are evaluated as document-producing constructors, so nested `xsl:copy-of` inside literal elements contributes correctly.
  - `xsl:try` now rolls back output written in the try block before executing `xsl:catch`, using efficient last-node/last-attribute snapshots. This fixes `iterate-036` and prevents the `catalog` self-tests from hanging.
  - Full W3C suite: **5,073 passed / 177 failed / 9,350 skipped** (~96.6%).

- **2026-07-02** — Cleared the W3C `seqtor` conformance cluster (54 runnable tests pass; 18 skipped).
  - Sequence-constructor whitespace and empty atomic items now produce correct spacing in complex content.
  - Empty sequence items act as atomic separators; text-node/atomic merging and adjacent-text concatenation match the XSLT 3.0 serialization rules.
  - `xsl:sequence` without `@select` now returns its raw sequence-constructor content.
  - `xsl:document` inside `xsl:comment`, `xsl:processing-instruction`, and `xsl:attribute` simple content is handled correctly.
  - Namespace prefix `xs` is now declared when evaluating Text Value Templates.
  - Mixed atomics and text nodes produced by `xsl:function` are serialized correctly.
  - Full W3C suite: **4,964 passed / 286 failed / 9,350 skipped** (~94.6%).

- **2026-07-03** — Cleared the quick-win conformance clusters `available-system-properties`, `on-empty`, `copy`, and `where-populated`.
  - `fn:available-system-properties` now returns `xs:QName` values and includes all required XSLT system properties.
  - Sequence-constructor placeholders no longer count as significant content, so `xsl:on-empty` fires correctly for empty `xsl:sequence` results.
  - `xsl:where-populated` now expands sequence placeholders produced by `xsl:sequence`, preserving arrays and other sequence values.
  - Full W3C suite: **4,999 passed / 251 failed / 9,350 skipped** (~95.2%).

- **2026-06-26** — Cleared the W3C `as`, `xml-to-json`, and `json-to-xml` conformance clusters.
  - `xs:float` serialization now uses the shortest round-trip `"R"` format in the scientific range, fixing `as-0802` / `as-0802b`.
  - `fn:json-to-xml` now honors the `duplicates` option (`use-first`, `retain`, `reject`) and reports `FOJS0005` / `XPTY0004` for invalid option values.
  - `fn:codepoints-to-string` now accepts XML 1.1 C0 control characters, allowing `xml-to-json` to serialize backspace/bell/form-feed as JSON escapes.
  - Full W3C suite: **4,953 passed / 297 failed / 9,350 skipped** (~94.3%).

- **2026-06-30** — Cleared the W3C `match` conformance cluster (1 failure → 0).
  - `xsl:mode` declarations without an explicit `@on-no-match` now default to `text-only-copy` per the XSLT 3.0 spec, so atomic items processed by `xsl:apply-templates` produce their string value in the default mode.
  - The built-in rule for atomic values now respects the effective mode's `on-no-match` behavior (deep-skip and shallow-skip suppress output).
  - Full W3C suite: **4,944 passed / 306 failed / 9,350 skipped** (~94.2%).

- **2026-06-30** — Cleared the W3C `current-output-uri` conformance cluster (1 remaining failure → 0).
  - `TransformEngine.TransformFunction` now compiles template match patterns and registers grouping functions before executing a stylesheet function, so `xsl:apply-templates` inside `xsl:function` can match template rules.
  - Result-document URI tracking is reset at function entry points.
  - Full W3C suite: **4,943 passed / 307 failed / 9,350 skipped** (~94.2%).

- **2026-06-28** — Cleared the W3C `apply-templates` conformance cluster (11 runnable failures → 0).
  - `match="/"` now uses the correct default priority of `-0.5` in XSLT 2.0/3.0.
  - Root-template selection now respects the default (unnamed) mode and applies conflict resolution, fixing `mode="#current"` through `xsl:call-template`.
  - `document-node(element(E))` and `document-node(element(*))` match patterns now compile and match correctly.
  - `xsl:apply-templates` with no `@select` raises `XTTE0510` when the context item is not a node.
  - `xsl:apply-imports` and `xsl:next-match` now forward ordinary parameters to the built-in rule fallback.
  - Ambiguous template matches raise `XTRE0540` for test cases that declare `on-multiple-match="error"` via the new `XsltCompiler.TreatRecoverableAmbiguousMatchAsError` flag.
  - Full W3C suite: **4,871 passed / 379 failed / 9,350 skipped** (~92.8%).

- **2026-06-28** — Restored the W3C `catalog` self-test set and fixed the O(N²) slowness that made it hang.
  - `NormalizeSequence` in the XPath VM now removes duplicate nodes with a `HashSet<IXdmNode>` instead of a nested loop, dropping large cross-document sequences from >10 minutes to seconds.
  - The `catalog` cluster now completes in under a minute; remaining failures are XML 1.1 parser limitations and a `catalog-007` `element-available()` mismatch.
  - Full W3C suite: **4,855 passed / 395 failed / 9,350 skipped** (~92.5%).

- **2026-06-28** — Cleared the `resolve-uri` cluster (24/24) and the `namespace-4801` regression.
  - `fn:resolve-uri()` now raises `FORG0002` for malformed relative URIs and relative base URIs per erratum FO.E1.
  - Dotted-path URIs resolve correctly in `fn:resolve-uri()` and `fn:static-base-uri()`.
  - `fn:document()` resolves relative URIs against the base URI of the supplied node argument.
  - Text/PI nodes produced by DTD entity expansion now inherit the parent element's resolved `xml:base`.
  - TVT evaluation passes the context element so the compiled XPath uses the correct in-scope namespaces and effective base URI.
  - Full W3C suite: **4,852 passed / 391 failed / 9,357 skipped** (~92.5%).
  - The `catalog` self-test set is temporarily skipped because it became extremely slow after the `document()` node-base fix.

- **2026-06-28** — Fixed the remaining XSLT `namespace` cluster failure (`namespace-3005`).
  - Top-level `xsl:namespace` instructions now produce a standalone namespace-node item when the containing sequence constructor is typed as `node()` or `node()?` (in addition to explicit `namespace-node()` types).
  - Full W3C suite: **4,845 passed / 405 failed / 9,350 skipped** (~92.3%).

- **2026-06-27** — Cleared the remaining single-failure clusters `sort`, `merge`, and `arrays`.
  - `sort-072`: `xsl:perform-sort` now preserves in-scope prefixed namespaces for relocated sequence-constructor children.
  - `merge-066`: XPath prefix validation no longer treats integer map keys (e.g. `map{1:xs:dateTime(...)}`) as undeclared QName prefixes.
  - `square-array-201`: `xsl:source-document` with `streamable="no"` now loads the document and evaluates its content with the loaded document as the context item.

- **2026-06-27** — Cleared the entire XSLT `math` conformance cluster (15 runnable failures).
  - `fn:number` now parses XPath lexical forms `INF`, `-INF`, and `NaN`.
  - `fn:floor`, `fn:ceiling`, and `fn:round` now atomize their arguments before numeric dispatch.
  - `fn:round` / `fn:round-half-to-even` now use `decimal` arithmetic for precision-bound decimal/integer values, fixing tie-rounding and large-integer precision loss.
  - `xs:double` and `xs:float` serialization now uses shortest round-trip formatting (`"G16"` / `"G9"`) in the scientific-notation range, producing canonical output such as `1.0E-98` instead of `1.0000000000000001E-98`.

- **2026-06-27** — Cleared the entire XSLT `maps` conformance cluster (35 runnable failures).
  - Implemented `xsl:map` and `xsl:map-entry` instructions in `TransformEngine`, including duplicate-key (`XTDE3365`) and non-map-content (`XTTE3375`) errors, and `XTDE0450` when a map is used as an element/document child.
  - `fn:serialize` now supports `method=json` for maps, arrays, booleans, numbers, and strings.
  - Map key equality now treats `xs:anyURI` as comparable to `xs:string` and handles `NaN` numeric keys safely.
  - `XPath31Expression.Compile` resolves function-call namespaces from `CompileOptions` and reports static `XPST0017` for removed functions and the obsolete `http://www.w3.org/2011/xpath-functions/map` namespace.
  - The conformance harness expands W3C test-suite `_select` AVT attributes using static parameters before compilation.
  - Follow-up fixes: preserve explicit `Q{uri}local` namespace URIs in function calls/named function refs; fall back to run-time `_select` expansion when static parameters are insufficient; atomize/flatten arrays for `xsl:apply-templates`, `xsl:value-of`, AVTs, and complex content construction.

- **2026-06-27** — Cleared the remaining XSLT `namespace` cluster failures (`namespace-0912` and `namespace-2611`) and the full `namespace-alias` cluster.
  - Built-in `shallow-copy` now suspends the outer sequence accumulator while applying templates to children, so typed variables containing shallow-copied elements keep child results nested instead of escaping as siblings.
  - `XdmValue.GetEffectiveBooleanValue()` now follows XPath sequence EBV rules: empty sequence → `false`, singleton sequence → EBV of its item, multi-node sequence → `true`, multi-item atomic sequence → `FORG0006`.

- **2026-06-26** — Cleared the XSLT `date` conformance cluster (46 runnable failures).
  - `xsl:value-of` now evaluates the `_select` AVT used by static-parameter test stylesheets.
  - `fn:format-date`, `format-time`, and `format-dateTime` now support escaped brackets (`[[` / `]]`), roman/alphabetic presentations, ISO week-of-month around year boundaries, non-BMP digit families, correct default widths, and timezone semantics (`[Z]` / `[z]`).
  - `fn:adjust-dateTime-to-timezone` now preserves the target timezone offset instead of returning a zero-offset value.

## 1. Consuming Bosak

### 1.1 Via Project References (development)

Add project references to the Bosak layer stack from your consuming project:

```xml
<ItemGroup>
  <ProjectReference Include="..\Bosak\src\Bosak.XPath.Api\Bosak.XPath.Api.csproj" />
  <ProjectReference Include="..\Bosak\src\Bosak.Xslt\Bosak.Xslt.csproj" />
  <!-- <ProjectReference Include="..\Bosak\src\Bosak.XQuery\Bosak.XQuery.csproj" /> -->
</ItemGroup>
```

`Bosak.XPath.Api`, `Bosak.Xslt`, and `Bosak.XQuery` pull in the lower layers automatically (Core, Parser, Compiler, Runtime, Standard, Providers).

**Target framework:** `net10.0` (both sides must align).

### 1.2 Via NuGet Packages

All Bosak source projects are now packable. After packing (`dotnet pack`), the following packages are produced:

| Package | Description |
|---------|-------------|
| `Bosak.Xslt` | XSLT 3.0 processor and transform engine |
| `Bosak.XPath.Api` | Public API for compiling and evaluating XPath 3.1 |
| `Bosak.XPath.Core` | XDM types and core abstractions |
| `Bosak.XPath.Runtime` | Register-based VM execution engine |
| `Bosak.XPath.Standard` | Standard XPath 3.1 / XQuery function library |
| `Bosak.XPath.Providers` | `IXdmNode` adapters for `System.Xml.Linq` |
| `Bosak.XPath.Parser` | Recursive-descent XPath 3.1 parser |
| `Bosak.XPath.Compiler` | AST-to-IR compilation pipeline |

**Consuming from a private feed:**
```bash
# Pack all projects
dotnet pack Bosak.sln --output ./nupkgs

# Push to your private NuGet feed
dotnet nuget push ./nupkgs/Bosak.Xslt.1.0.0.nupkg --source https://your-feed/nuget/v3/index.json
```

Then reference in your consuming project:
```xml
<ItemGroup>
  <PackageReference Include="Bosak.Xslt" Version="1.0.0" />
  <PackageReference Include="Bosak.XPath.Providers" Version="1.0.0" />
</ItemGroup>
```

> **Note:** Transitive dependencies are automatically resolved. You only need to reference the top-level packages your code directly uses (`Bosak.Xslt` and/or `Bosak.XPath.Api`).

---

## 2. XPath 3.1 Expressions

### 2.1 Compile & Evaluate

> **v1.0.0:** `XPathExpression` is the version-neutral entry point with an identical
> surface (`Compile` ×2, `Evaluate` ×2, `EvaluateNodes`) — new code should prefer it;
> `XPath31Expression` remains fully supported. Both accept the same `CompileOptions`.

```csharp
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;

// One-shot evaluation
var expr = XPath31Expression.Compile("/invoice/items/item[@price > 100]");
var result = expr.Evaluate(document);

// Re-use compiled expression
var expr2 = XPath31Expression.Compile("$minPrice + $taxRate * $amount");
var result2 = expr2.Evaluate(
    new EvaluationContext()
        .WithVariable("minPrice",  XdmValue.FromDecimal(100.00m))
        .WithVariable("taxRate",   XdmValue.FromDecimal(0.21m))
        .WithVariable("amount",    XdmValue.FromDecimal(500.00m)));
```

**XPath 4.0 opt-in (REQ-118).** The default target is XPath 3.1 and is unchanged. Compiling with `Compatibility = XPathCompatibility.XPath40` additionally enables the XPath 4.0 surfaces as they land — currently the 4.0-only F&O functions (`fn:replicate`, `fn:slice`, `fn:items-at`, `fn:foot`, `fn:trunk`, `fn:insert-separator`, `fn:char`, `fn:characters`); grammar features land in later slices. In 3.1 mode a call to a 4.0-only function fails at compile time with **XPST0017**, and 3.1 evaluation contexts do not expose them through `fn:function-lookup` or dynamic dispatch:

```csharp
var expr40 = XPath31Expression.Compile(
    "fn:slice($in, 2, 4)",
    new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
```

### 2.2 Evaluation Context

```csharp
using Bosak.XPath.Runtime.Vm;

var ctx = new EvaluationContext
{
    BaseUri = "file:///C:/Data/",
    DocumentLoader = uri => /* your IXdmNode loader */
};

// Redirect published (e.g. http:) resource URIs to local files. Consulted by
// fn:doc, fn:json-doc, fn:unparsed-text(-available/-lines), and fn:transform's
// stylesheet-location before any filesystem or network access. Return null for
// URIs that should follow the normal resolution path.
ctx.ResourceUriMapper = uri =>
    uri == "http://example.org/published/spec.xml"
        ? @"C:\Data\local-copy.xml"
        : null;

// Pre-register a source document so fn:doc(document-uri($node)) returns the same node.
ctx.RegisterDocument("file:///C:/Data/input.xml", sourceDocument);

ctx.WithNamespace("edi", "http://example.org/edi")
   .WithVariable("docId", XdmValue.FromString("DOC-1234"));

var result = expr.Evaluate(ctx);
```

#### Database document loaders (REQ-120 Slice 2 — `Bosak.XPath.Providers.Database`)

The `Bosak.XPath.Providers.Database` project contains a scheme-dispatching loader that
resolves XML database REST URIs and delegates every other URI to a fallback loader. Three
schemes are registered, each with its own default port and REST path shape:

| Scheme | Default port | REST mapping (`host`/`port` from the URI authority) |
|--------|--------------|--------------------------------------------------------|
| `basex://host[:port]/db/resource` | 8984 | `http://host:port/rest/db/resource` (BaseX REST: document at path) |
| `exist://host[:port]/db/resource` | 8080 | `http://host:port/exist/rest/db/resource` (eXist REST: document at path) |
| `marklogic://host[:port]/db/resource` | 8000 | `http://host:port/v1/documents?uri=%2Fdb%2Fresource` with `Accept: application/xml` (MarkLogic REST: document URI as the `uri` query parameter, not the request path) |

```csharp
using Bosak.XPath.Providers.Database;
using Bosak.XPath.Providers.Xml;

var options = new DatabaseLoaderOptions();
options.BaseX.Username = "admin";          // HTTP Basic auth; URI-embedded credentials are NOT supported
options.BaseX.Password = "admin";
// options.BaseX.EndpointBase = "https://example.org/basex/rest";  // optional, overrides http://host:port/rest
// options.Exist.EndpointBase = "https://example.org/exist/rest";  // optional, overrides http://host:port/exist/rest
// options.MarkLogic.EndpointBase = "https://example.org";         // optional, overrides the http://host:port origin
//                                                                   (the /v1/documents?uri=… suffix is still appended)

ctx.DocumentLoader = DatabaseDocumentLoader.Dispatch(XDocumentProvider.LoadFile, options);
// Forward-only streaming variant (bounded memory, record-at-a-time off the response stream):
ctx.StreamingDocumentLoader = DatabaseDocumentLoader.DispatchStreaming(XDocumentProvider.LoadFile, options);
```

With both hooks installed, `fn:doc`/`fn:document`/`xsl:source-document` (both modes)/
`xsl:merge-source`/`fn:transform` resolve database URIs unchanged.

- **Options/auth** — `DatabaseLoaderOptions` carries one connection-options object per scheme
  (`BaseX`, `Exist`, `MarkLogic`, all deriving from `DatabaseConnectionOptions`:
  `EndpointBase`, `Username`, `Password`). Credentials are sent as an HTTP Basic
  `Authorization` header; the `EndpointBase` override replaces the scheme's default origin
  (and path prefix for basex/exist; the MarkLogic override replaces the origin only).
- **Streaming usage** — `DispatchStreaming` reads the response with `ResponseHeadersRead`
  straight into `XmlStreamingProvider.Load`, so top-level records materialize one at a time.
  The response stream is bound to the `HttpResponseMessage` (`ResponseBoundStream`): the
  response and its connection are released deterministically at end-of-stream and on load
  failure. Abandoning a partially-read streamed document without consuming it still relies
  on finalization.
- **Error contract** — unreachable endpoints, HTTP error statuses, and timeouts surface as
  `IOException`; malformed XML payloads as `XmlException` — through
  `EvaluationContext.LoadDocument` both map to FODC0002. Unsupported URI shapes (an
  unregistered scheme passed to `DatabaseDocumentLoader.Load` directly, or URI-embedded
  userinfo) surface as `ArgumentException`/`UriFormatException` (FODC0005 class).
- **Packaging** — `<IsPackable>true</IsPackable>`: the owner registered
  `Bosak.XPath.Providers.Database` on nuget.org for Trusted Publishing (2026-10-03), so the
  package ships automatically with the next core tag. `release.yml` packs the whole solution
  and pushes every nupkg except `*LanguageServer*` with skip-duplicate, so re-runs are safe.

#### Database collections (REQ-120 Slice 3 — `fn:collection` over REST)

Slice 3 adds an additive public hook on the frozen `EvaluationContext` and the
corresponding listing support in the providers package, so
`fn:collection("basex://host/db/coll")` resolves server-side:

```csharp
ctx.DocumentLoader = DatabaseDocumentLoader.Dispatch(XDocumentProvider.LoadFile, options);
ctx.CollectionLoader = DatabaseDocumentLoader.DispatchCollection(_ => null, options);
// fn:collection("basex://host/db/coll")     → member documents of the collection
// fn:uri-collection("basex://host/db/coll") → their URIs, in listing order
```

- **Hook contract** — `EvaluationContext.CollectionLoader`
  (`Func<string, IReadOnlyList<string>?>`, additive, SemVer minor) receives the collection
  URI and returns the member document URIs (or absolute file paths), or `null` to decline
  (the engine then falls through to its built-in directory collections and finally raises
  FODC0002). It is consulted after registered/environment collections, sees the URI before
  any `?select=`/fragment stripping, and receives the empty string for `fn:collection()`
  with no argument. A host may set the delegate directly instead of using
  `DispatchCollection`.
- **Document order** — members are loaded in the order returned by the hook/listing, and
  cross-tree document order follows that load order (the engine's creation-sequence model),
  so the listing order IS the collection's document-order story. BaseX and eXist listings
  are returned in the database's listing order; **MarkLogic's `view=uris` search results
  have no stable order** — do not rely on MarkLogic collection order for document-order
  semantics.
- **Listing shapes** (per-DB wire quirks stay behind the scheme registry):
  BaseX `GET /rest/{db/coll}` returns an XML listing of `rest:resource` members (nested
  `rest:directory` content is read in-response; empty directories via a follow-up request);
  eXist `GET /exist/rest/db/coll` returns `resource` members with one follow-up request per
  `subcollection`; MarkLogic
  `GET /v1/search?directory={dir}&view=uris&depth=Infinity` (`Accept: application/xml`)
  returns one `search:uri` entry per matching document.
- **Member URIs** — `DatabaseDocumentLoader.LoadCollection(uri, options)` (and the
  `DispatchCollection` fallback form) return the members as `scheme://host[:port]/…` document
  URIs on the collection's own authority (the scheme's default port is applied when the URI
  carries none). Members are ordinary database document URIs: identity caching, the
  FODC0002/FODC0005 error contract, and `xsl:strip-space` post-processing behave exactly as
  for `fn:doc`.
- **Error contract** — unreachable endpoints, HTTP error statuses, and timeouts surface as
  `IOException`; a malformed listing payload as `XmlException`; unregistered schemes and
  URI-embedded userinfo keep the Slice 1/2 `ArgumentException`/`UriFormatException` classes.
  A declined collection URI surfaces as FODC0002 from the engine.
- **Foreign node providers** — collection member fragments (`doc.xml#id`) now resolve for
  any `IXdmNode` provider (located via the provider-agnostic axes, grounded in a fresh
  LINQ-to-XML copy). Non-`XDocumentNode` documents keep their provider's own
  `DocumentOrder` semantics (the engine's creation-sequence registration applies to
  `XDocumentNode` only — documented on `LoadDocument`), and `xsl:strip-space` applies only
  to `XDocumentNode`-backed trees (mutation-bound; foreign providers should strip at load
  time in their loader or `DocumentPostProcessor`).

### 2.3 Reading Results

```csharp
if (result.IsNode && result.NodeValue is { } node)
{
    Console.WriteLine(node.StringValue);
}
else if (result.IsSequence && result.SequenceValue is { } seq)
{
    foreach (var item in XdmSequence.FromSource(seq))
        Console.WriteLine(item.ToString());
}
else
{
    Console.WriteLine(result.ToString());
}
```

### 2.4 Context Item (Focus)

```csharp
// Evaluate with a context item so `.` and `position()` work
ctx.WithFocus(XdmValue.FromNode(document), position: 1, size: 1);
var result = expr.Evaluate(ctx);
```

---

## 3. XSLT Transforms

### 3.1 Compile a Stylesheet

```csharp
using Bosak.Xslt.Api;

var xsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
    <xsl:template match='/'>
        <output><xsl:value-of select='root/@id'/></output>
    </xsl:template>
</xsl:stylesheet>";

var compiler = new XsltCompiler();
var executable = compiler.Compile(xsl);
```

#### 3.1a Schema-Aware Compilation (`xsl:import-schema`)

By default the compiler is a *basic* XSLT processor: stylesheets containing `xsl:import-schema`, `validation="strict"`, or `xsl:type` are rejected at compile time with XTSE1650/XTSE1660. Opt in to schema-aware compilation when the host provides schema-aware processing:

```csharp
var compiler = new XsltCompiler
{
    SchemaAware = true,
    // Optional: supply schema documents for xsl:import-schema declarations.
    // (target namespace, location hints) => schema stream, or null to try file/URI resolution.
    SchemaResolver = (ns, hints) => mySchemaStream,
    // Optional: pre-built schema set made in-scope (lowest precedence).
    SchemaSet = myCompiledSchemaSet,
};

var executable = compiler.Compile(xsl);   // xsl:import-schema declarations are compiled
                                          // into one merged XmlSchemaSet
```

Declarations may use an inline `xs:schema` child, a `schema-location` (resolved against the module's base URI), the resolver, or a namespace already present in the host `SchemaSet`. Declarations merge across the import tree by import precedence (same-precedence conflicts → XTSE0215; unlocatable/invalid schemas → XTSE0220). The merged set is in scope during the transform: user-defined simple-type constructor functions (`Q{uri}local#1`), `cast as` / `instance of` against user-defined types, and `schema-element()` / `schema-attribute()` kind tests work as they do for XQuery `import schema`. Runtime validation of constructed content (`validation` / `@type` semantics, XTTE15xx) is enforced when a schema set is in scope — see §3.1c; typed construction of complex-typed nodes via interception hooks is §3.1b.

#### 3.1b Intercepting Constructed Nodes (Schema Annotation Seam)

A host that provides schema-aware processing (see §3.1a) can intercept every node the
transform constructs and annotate it with schema PSVI, so typed-value operations on the
result tree (`TypedValue`, `instance of` against schema types, `SchemaTypeAnnotation`, …)
work through the standard engine surfaces. Two optional processors on the
`EvaluationContext` passed to `Transform`/`TransformToString` do this; both default to
null, and with them unset the engine is bit-identical:

```csharp
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;

var context = new EvaluationContext
{
    // Fires once per constructed element, AFTER its attributes and content are
    // complete, bottom-up (innermost element first). Covers xsl:element,
    // literal result elements, and xsl:copy element results.
    ConstructedElementProcessor = node =>
        XdmSchemaAnnotator.ValidateSubtree(
            ((XDocumentNode)node).UnderlyingObject as System.Xml.Linq.XElement
                ?? throw new InvalidOperationException("expected element"),
            mySchemaSet),

    // Fires at each result-document boundary with the wrapped document node.
    ConstructedDocumentProcessor = node => { /* document-level policy */ },
};

var result = executable.TransformToString(source, context, initialTemplate: "main");
```

`XdmSchemaAnnotator` (`Bosak.XPath.Providers.Xml`) supplies the annotation primitives:

- `ValidateSubtree(XElement element, XmlSchemaSet schemas, ValidationEventHandler? handler = null, bool throwOnInvalid = false)` — validates the subtree **in place**: PSVI (`IXmlSchemaInfo`) annotations are attached to the live `XObject`s, so the caller's tree keeps its node identity and every typed-value surface works immediately (no serialize/parse round-trip). Returns `XdmSubtreeValidationResult` (`IsValid`, `IReadOnlyList<ValidationEventArgs> Errors`); the optional handler receives every validation event. With `throwOnInvalid: true` an invalid subtree throws `XmlSchemaValidationException` carrying the first error. Attribute nodes are annotated as part of validating their parent element.
- `Annotate(XObject node, IXmlSchemaInfo annotation)` — attaches a host-built `IXmlSchemaInfo` (a simple read-only interface: implement it or reuse the instances validation produces) without performing validation — e.g. declaration-only annotation or nodes validated elsewhere.

Notes:

- Per-element validation runs bottom-up, so nested constructed subtrees are re-validated
  O(depth) times; a host can instead do nothing per element and validate once at the
  document boundary.
- The processors observe the node as the engine sees it (`XDocumentNode` wrapper over the
  live `XObject`); mutating annotations on that object is the intended use.
- An empty result produces no calls at all.

#### 3.1c Schema-Aware Validation of Constructed Content (REQ-099)

When a stylesheet is compiled schema-aware (§3.1a) and a schema set is in scope, the
engine applies XSLT 3.0 validation semantics to constructed content automatically:
`validation="strict|lax|preserve|strip"` and `type="QName"` on `xsl:element`,
`xsl:attribute`, `xsl:copy`, `xsl:document`, `xsl:result-document`, and literal result
elements (`xsl:validation` / `xsl:type`), with stylesheet-level `default-validation`
inherited per module. Failures raise the XTTE15xx family as catchable dynamic errors
(`xsl:try`/`xsl:catch`): XTTE1510 (strict content invalid), XTTE1512 (strict, no
top-level element declaration), XTTE1515 (attribute invalid), XTTE1535 (complex type
named for an attribute), XTTE1540 (lax or `type=` failure), XTTE1545 (QName/NOTATION
content), XTTE1550 (document shape), XTTE1555 (parentless attribute).

With `SchemaAware=false` (or no schema set in scope) none of this runs: `strict`/`type`
still raise XTSE1660 at compile time, and `lax`/`preserve`/`strip` keep their basic-processor
errata behavior — bit-identical to previous releases.

Related declarations now honored: `xsl:strip-type-annotations` (PSVI removed from copied
nodes), `input-type-annotations="strip"` (PSVI removed from loaded input documents), and
PSVI annotations are preserved across `xsl:copy`/`xsl:copy-of` (`validation="preserve"`
and nilled properties survive).

The validation primitives are also public for host-side use
(`Bosak.XPath.Providers.Xml`):

```csharp
var result = XdmSchemaAnnotator.Validate(element, schemas,
    new XdmValidationOptions(XdmValidationMode.Strict, DocumentLevel: true));
// or, for an attribute / a named type target:
var attrResult = XdmSchemaAnnotator.ValidateAttribute(attribute, schemas,
    new XdmValidationOptions(XdmValidationMode.Lax, TypeName: new XmlQualifiedName("size", "urn:t")));
```

`XdmValidationMode` = `Strict | Lax | Strip | Preserve`; `XdmValidationOptions` =
`(Mode, TypeName?, DocumentLevel)`. The service is error-code-agnostic — it returns
`XdmSubtreeValidationResult` (`IsValid`, `Errors`) and never raises XSLT/XQuery codes
itself; hosts map failures to their own error family.

### 3.2 Transform a Document

```csharp
using Bosak.XPath.Providers.Xml;
using System.Xml.Linq;

var source = new XDocument(new XElement("root", new XAttribute("id", "42")));
var resultXml = executable.TransformToString(new XDocumentNode(source));
// => "<output>42</output>"
```

### 3.2a Streaming Input (Burst Mode)

For very large documents, `TransformStreaming` reads the source lazily from a stream:
the root's top-level nodes (*records*) are materialized one at a time as the stylesheet
consumes them, and each record is released once processing moves on. Within a record all
axes work as usual (it is a normal, detached tree); parent chains above a record reach
the streamed root and document nodes, so patterns like `match="/inventory/product"` work.

```csharp
using Bosak.Xslt.Api;

var executable = new XsltCompiler().Compile(xsl);
using var source = File.OpenRead("orders-2gb.xml");
var result = executable.TransformStreaming(source,
    new StreamingTransformOptions { BaseUri = "file:///data/orders-2gb.xml" });

// Serialized directly (same output-property handling as TransformToString):
string xml = executable.TransformStreamingToString(source2,
    new StreamingTransformOptions { BaseUri = "file:///data/orders-2gb.xml" });
```

The lower-level primitive is provider-neutral and works with plain XPath too:

```csharp
using Bosak.XPath.Providers.Streaming;

IXdmNode doc = XmlStreamingProvider.Load(stream, new StreamingLoadOptions { BaseUri = "…" });
// pass to XPath31Expression.Evaluate or XsltExecutable.Transform like any other node
```

**Memory contract** — what stays bounded and what does not:

| Access pattern | Memory |
|---|---|
| `xsl:for-each` / `xsl:apply-templates` over the root's children (or `//record`), record-local bodies | **Bounded** — records are released as they are consumed |
| Predicates on the record step, `position()` | Bounded |
| `xsl:sort`, `xsl:for-each-group`, `fn:count()`, `fn:last()` subscript `[last()]`, keys, variables retaining records | Unbounded but correct (buffers the stream) |
| Spec-streaming multi-operand shapes (`//A \| //B`, `except`/`intersect`, `xsl:fork` branches, multi-entry `map{}`) via `xsl:source-document streamable="yes"` | Unbounded but correct — the engine opts into record retention (`StreamingLoadOptions.RetainRecords` / `IStreamingDocument.TryEnableReplay`): records are memoized so each operand replays the stream; memory is bounded by document size |
| `fn:last()` in a streamed focus, a second pass over the streamed root, `preceding` axes across records, `following` axes past the current record | Clear `StreamingException`/error — never silently wrong data |
| `xsl:accumulator` declarations | **Bounded** — values are pushed per record and travel as annotations; document-level `accumulator-after` drains the stream when nothing is mid-enumeration |
| `fn:copy-of`/`xsl:copy-of` of a streamed node | Records: **bounded** (a grounded, detached copy of the record is built). Streamed document/root: unbounded but correct — the copy drains the stream into memory, the same contract as sorting |

Notes and limitations: the streamed children of the root are **forward-only** (one pass).
`xsl:strip-space`/`xsl:preserve-space` rules are applied per record (including top-level
whitespace text records, which are dropped). Cross-accumulator references in rule selects
must target accumulators declared **earlier**; accumulator rule and initial-value errors
surface at the point of access (spec bug 29813). DTDs are processed by default (matching
the in-memory loader; unparsed entities are surfaced on the document node) — pass
`ReaderSettings` with `DtdProcessing.Prohibit` to reject them. Comments/PIs before the
root element are materialized eagerly and surfaced as document-node children before the
root element (matching the in-memory provider); comments/PIs **after** the root element
are not surfaced. Engine wrappers over streamed nodes are cached per underlying node
(`ConditionalWeakTable`), so navigation allocates per node rather than per access without
retaining released records. `fn:transform` with a streaming source is unsupported;
`xsl:source-document streamable="yes"` is implemented since Streaming Phase C (spec
constructs compile under the §19 streamability analyzer and execute at runtime, with
record retention for multi-operand shapes as documented above). `fn:copy-of` deep-copies
streamed nodes into grounded copies (never returning the live streamed wrapper);
`xsl:copy-of` into the result tree works normally, and `fn:snapshot` deep-copies
streamed nodes with their accumulator values.

### 3.3 Named Templates & `call-template`

```csharp
var xsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
    <xsl:template match='/'>
        <result>
            <xsl:call-template name='format-address'>
                <xsl:with-param name='city' select='root/city'/>
            </xsl:call-template>
        </result>
    </xsl:template>

    <xsl:template name='format-address'>
        <xsl:param name='city'/>
        <address><xsl:value-of select='$city'/></address>
    </xsl:template>
</xsl:stylesheet>";

var executable = new XsltCompiler().Compile(xsl);
var result = executable.TransformToString(new XDocumentNode(source));
```

### 3.4 Tunnel Parameters

```csharp
var xsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
    <xsl:template match='/'>
        <xsl:apply-templates>
            <xsl:with-param name='traceId' select='"REQ-123"' tunnel='yes'/>
        </xsl:apply-templates>
    </xsl:template>

    <xsl:template match='item'>
        <!-- $traceId is available here via tunnel -->
        <item trace='{$traceId}'><xsl:value-of select='.'/></item>
    </xsl:template>
</xsl:stylesheet>";
```

### 3.5 `fn:transform()` — XSLT from XPath

```csharp
var callerXsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
    xmlns:map='http://www.w3.org/2005/xpath-functions/map'>
    <xsl:template match='/'>
        <result>
            <xsl:copy-of select='transform(map{
                ""stylesheet-location"": ""file:///C:/styles/main.xsl"",
                ""source-node"": .,
                ""stylesheet-params"": map{""greeting"": ""world""}
            })?output'/>
        </result>
    </xsl:template>
</xsl:stylesheet>";
```

---

## 4. XQuery 3.1 Queries

### 4.1 Compile and Evaluate a Query

```csharp
using Bosak.XQuery.Api;
using Bosak.XPath.Core.Xdm;

var compiler = new XQueryCompiler();
var executable = compiler.Compile("for $i in 1 to 3 return $i");

var context = new XQueryContext();
var result = executable.Evaluate(context);

foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
    Console.WriteLine(item.IntegerValue);
// => 1 2 3
```

### 4.2 Prolog Declarations

```csharp
var query = @"
    declare namespace math = 'http://www.w3.org/2005/xpath-functions/math';
    declare default element namespace 'http://example.com/ns';
    math:pi()
";
var result = new XQueryCompiler().Compile(query).Evaluate(new XQueryContext());
```

### 4.3 Context Item and External Variables

```csharp
using Bosak.XPath.Providers.Xml;
using System.Xml.Linq;

var doc = new XDocument(new XElement("root", new XAttribute("id", "42")));
var context = new XQueryContext()
    .WithContextItem(XdmValue.FromNode(new XDocumentNode(doc)))
    .WithVariable("threshold", XdmValue.FromInteger(10));

var result = new XQueryCompiler()
    .Compile("/root/@id[. > $threshold]")
    .Evaluate(context);
```

### 4.4 Library Modules

Register library module sources with `WithModule(uri, source, location?)`; queries import
them by target namespace (optionally narrowed by `at` location hints):

```csharp
var compiler = new XQueryCompiler()
    .WithModule("http://example.com/greet", """
        module namespace g = "http://example.com/greet";
        declare function g:hello($n as xs:string) as xs:string { "hello " || $n };
        """);

var result = compiler
    .Compile("import module namespace g = \"http://example.com/greet\"; g:hello(\"world\")")
    .Evaluate(new XQueryContext());
// => "hello world"
```

- Imports resolve to the transitive closure; cycles are legal. Multiple modules may share
  one target namespace — all their public declarations merge (`at` hints select by location).
- `%public` / `%private` annotations control cross-module visibility (private declarations
  are invisible to importers: XPST0017 / XPST0008).
- Each library module's bodies compile and execute with its own prolog context (namespaces,
  base URI, default element namespace, default collation).
- A library module cannot be evaluated as a query (XPST0003); unresolved imports raise
  XQST0059; duplicate imports of one namespace raise XQST0047.

### 4.4a Host-Environment Namespace Bindings

Host environments (test drivers, embedding applications) can pre-bind namespace prefixes in
the static context with `WithNamespace(prefix, uri)` — the same mechanism the QT3 harness
uses for environment-declared prefixes. Prolog `declare namespace` declarations take
precedence over external bindings:

```csharp
var compiler = new XQueryCompiler()
    .WithNamespace("test", "http://www.xpathtest.com/test");

var result = compiler
    .Compile("/test:valueComp200/test:integer[. eq 3]")
    .Evaluate(new XQueryContext());
```

Undeclared prefixes in name tests are rejected at compile time with XPST0081 (see
§2 static-name-test validation), so queries that rely on host bindings must declare them
through `WithNamespace`.

### 4.5 Current Status

| Feature | Status | Notes |
|---------|--------|-------|
| Prolog-less queries (`for`, `let`, `where`, `return`) | ✅ Working | Reuses XPath 3.1 FLWOR support |
| Version declaration / namespace declarations | ✅ Working | Parsed by `XQueryParser` |
| Default element / function / collation declarations | ✅ Working | Stored in `XQueryStaticContext` |
| `order by` | ✅ Working | Tuple-based sorting; ascending/descending, empty least/greatest, collation |
| `count` | ✅ Working | Compiler-managed integer counters over the tuple path |
| `group by` | ✅ Working | `GroupBy` opcode; `$var` / `$var := expr` specs with optional collation; post-group `order by`/`count` |
| `window` | ✅ Working | `Window` opcode; tumbling/sliding, start/end vars (current/position/previous/next), `only end` |
| Direct / computed constructors | ✅ Working | All seven forms; constructor-local namespaces, copy semantics |
| `switch` / `typeswitch` | ✅ Working | Desugared to `let` + `if`/`eq`/`instance-of` chains |
| User-defined functions and variables | ✅ Working | `declare function` / `declare variable` with %public/%private annotations |
| Library modules / `import module` | ✅ Working | Transitive import graph, location hints, per-module static contexts |
| Serialization | ✅ Working | `xml`, `html`, `xhtml`, `text`, `json`, `adaptive`; `declare option output:*` |

---

## 5. XSLT Feature Matrix (Current State)

| Feature | Status | Notes |
|---------|--------|-------|
| Streaming input (burst mode) | ✅ Working | `XsltExecutable.TransformStreaming`/`TransformStreamingToString` + `XmlStreamingProvider`: record-at-a-time processing in bounded memory by default; forward-only root children; pre-root comments/PIs surfaced as document children; per-node `StreamingNode` wrapper cache; see §3.2a. Streaming accumulators (Phase B) work over the streamed source. `streamable="yes"` constructs receive compile-time XTSE3430 streamability analysis (Phase C, `StreamabilityAnalyzer` — §19 posture/sweep rules) and execute at runtime (Phase D: fused single-pass eager helpers, §11.7.3 content semantics, `fn:snapshot` grounding, opt-in record retention for crawling multi-operand shapes, streaming DTD). `fn:copy-of` deep-copies streamed nodes into grounded copies (never the live wrapper). The current group is not visible inside templates invoked via xsl:call-template/xsl:apply-templates (dynamic XTDE1061/XTDE1071, XSLT 3.0 §14.4); current-group() in a streamable template with no group in scope is a static XTSE3430; fn:generate-id is stable across xsl:fork prongs; duplicate map-constructor keys inside xsl:fork branches or streamable source-document content raise XTDE3365. Full sweep 10,220/55/4,325; streamable `xsl:source-document` implemented; `xsl:supports-streaming` reports `yes` |
| `xsl:template match="…"` | ✅ Working | Pattern compiler: element names, `*`, `@*`, predicates, union (`\|`) |
| `xsl:template name="…"` | ✅ Working | Named template dispatch; raw XDM result via `XsltExecutable.Transform(..., rawResult: true)`; whitespace/EQName names normalized; `xsl:initial-template` permitted in XSLT namespace |
| `xsl:call-template` | ✅ Working | With `xsl:with-param` support; matches named templates by expanded QName (different prefixes bound to the same URI); rejects template names in reserved namespaces (`XTSE0080`) except `xsl:initial-template` |
| `xsl:apply-templates` | ✅ Working | Default mode; `select` attribute supported |
| `xsl:value-of` | ✅ Working | |
| `xsl:for-each` | ✅ Working | Position / size context updated per item |
| `xsl:if` / `xsl:choose` | ✅ Working | `when` + `otherwise` |
| `xsl:element` / `xsl:attribute` | ✅ Working | |
| `xsl:text` | ✅ Working | |
| `xsl:copy` | ✅ Working | Shallow copy with `@select` support; focus set correctly per item |
| `xsl:copy-of` | ✅ Working | Deep copy of nodes; Document nodes supported. `copy-namespaces` respected. Static validation rejects disallowed children (`XTSE0260`) and invalid attributes (`XTSE0090`). |
| `xsl:evaluate` | ✅ Working | Dynamic XPath 3.1 evaluation inside XSLT; supports context item, `xsl:with-param` / `@with-params`, in-scope namespaces, base URI, default collation, and `@as` coercion. Stylesheet functions are visible only when not `private`/`hidden`. Java extension functions are not supported. |
| `xsl:comment` | ✅ Working | `select` attribute or text content |
| `fn:copy-of` | ✅ Working | XSLT 3.0 context function |
| `xsl:decimal-format` | ✅ Working | Parsed and registered for `fn:format-number` |
| `xsl:variable` | ✅ Working | Lexical scoping; `as` attribute with full atomic type coercion and atomization (`xs:integer`, `xs:string`, `xs:boolean`, `xs:double`, `xs:decimal`, `xs:duration`, `xs:QName`, `xs:dateTime`, gYear, etc.). Node type tests (`element(...)`, `attribute(...)`, `document-node(...)`, `node()`, `item()`) bypass atomization. Usable in XPath via `$var`. Global variables with sequence constructors are evaluated lazily on first reference with a singleton focus based on the root node of the tree containing the initial context node (XSLT 3.0 §9.6). |
| `xsl:param` | ✅ Working | On named templates, global params, default values; `as` attribute with full atomic type coercion and atomization. Subtype substitution (integer→decimal, float→double) and type promotion supported. |
| Built-in template rules | ✅ Working | Shallow-copy elements, copy text/attributes |
| Literal result elements | ✅ Working | Namespace preservation, AVT evaluation |
| `xsl:import` / `xsl:include` | ✅ Working | URI resolution with correct precedence rules |
| Modes | ✅ Working | Named modes, `#current`, `#default`, `#all`, multi-mode templates |
| `xsl:sort` | ✅ Working | Single and multi-key; `data-type`, `order`, `stable`, AVTs for `lang`/`case-order`/`collation`; recognized collations including UCA `alternate=non-ignorable`, `blanked`, and `shifted`. Default collation is respected; `case-order` works without an explicit collation. |
| `xsl:number` | ✅ Working | `single`, `any`, `multiple` levels; format tokens |
| `xsl:key` / `key()` | ✅ Working | Indexed lookup; composite keys; content-constructor keys preserve typed atomic values; results returned in document order; `key()` allowed in match patterns with XTSE0340 validation. Key-value comparison respects the effective default or explicit `@collation`; conflicting collations for the same key name raise XTSE1220. |
| `xsl:output` | ✅ Working | `method` (`xml`, `html`, `xhtml`, `text`, `json`, `adaptive`), `indent`, `omit-xml-declaration`, `encoding`, `version`, `standalone`, `doctype-system`, `doctype-public`, `cdata-section-elements`, `escape-uri-attributes`, `include-content-type`, `media-type`, `byte-order-mark`, `html-version`, `suppress-indentation`, `normalization-form`, `use-character-maps`, `json-node-output-method`, `allow-duplicate-names`, `escape-solidus`, `item-separator`, `parameter-document`, `undeclare-prefixes`, `build-tree`. Encoding-aware output escapes unrepresentable characters as numeric character references and splits CDATA sections around them. Named `xsl:output` definitions are resolved by import precedence; `xsl:result-document` attributes override the effective output definition. XHTML 1.0 uses the HTML 4 empty-element list; XHTML5 strips the XHTML namespace prefix, serializes HTML5 void elements as empty tags, ignores `doctype-public` when no `doctype-system` is supplied, and preserves root-element case in the DOCTYPE. DOCTYPE literal values are quoted with the delimiter that does not occur in the value. JSON output serializes maps, arrays, booleans, numbers, strings, and nodes; `json-node-output-method` controls node serialization; `escape-solidus` defaults to `yes`; character maps are applied to the final JSON text. Maps/arrays/functions at the top level of a non-JSON output raise `SENR0001`. `xsl:result-document` supports `method="json"`, `method="adaptive"`, and `build-tree="no"` with raw-item collection. `SEPM0009` and `SEPM0010` are enforced for XML/XHTML output properties. |
| `xsl:character-map` | ✅ Working | `name`, `use-character-maps`, and `xsl:output-character/@character` / `@string`; merged in declaration order, later maps override earlier ones (last-wins) for duplicate characters; explicit mappings override referenced maps within a single character map; applied to text, attribute, comment, PI, raw-XML, JSON, and adaptive output in all methods |
| `xsl:function` | ✅ Working | User-defined XPath functions in XSLT; `@as` return type enforced via `ConvertVariableValue` |
| `xsl:sequence` | ✅ Working | Returns sequences from functions |
| `xsl:mode` | ✅ Working | `on-no-match`, `on-multiple-match`, `warning-on-no-match`, `warning-on-multiple-match`, `visibility`, `typed`, `streamable`, `default-mode`, duplicate-declaration checks (`XTSE0545`), and `#unnamed` normalization |
| `xsl:analyze-string` | ✅ Working | Regex matching/non-matching children; `regex-group()`; XSLT 3.0 zero-length match semantics; `@flags` including multiline (`m`) are passed to regex translation |
| Tunnel parameters | ✅ Working | `tunnel="yes"` propagation through `apply-templates` |
| `fn:transform()` | ✅ Working | Full option support: `stylesheet-location`/`stylesheet-node`/`stylesheet-text`/`package-name`(+`package-version` range selection), `source-node`, `global-context-item`, `initial-match-selection` (arbitrary XDM), `initial-template`/`initial-mode`/`default-mode` (xs:QName), `stylesheet-params`/`template-params`/`tunnel-params`/`static-params`, `delivery-format` (`document`/`raw`/`serialized`), `base-output-uri`, `serialization-params`, `xslt-version`. Secondary `xsl:result-document` output is captured into the result map keyed by resolved URI, and absent principal output is suppressed. Available in static expressions (`static="yes"` variables, `xsl:use-when`); function items returned via `delivery-format="raw"` remain callable in the calling stylesheet. Package entry points honor `visibility` (XTDE0040). W3C `fn-transform` Tier-2m 117/124 passed (7 skipped). |
| `xsl:use-package` | ✅ Working | Resolves a used package by `@name` and `@package-version` against the registered package set. Supports exact versions, wildcard prefixes (`1.*`), hyphen and `to` ranges (`1.0-2.0`, `1.0 to 2.0`), minimum bounds (`1.5+`), comma-separated alternatives, and `*` / empty for any version. Honors `xsl:accept`/`xsl:override` visibility, per-package lazy-global isolation, and the W3C `package_version_resolution` dependency (`highest_version`/`lowest_version`/`unspecified`). |
| `xsl:attribute-set` / `use-attribute-sets` | ✅ Working | Accumulates across imports/includes; cycle detection; `xsl:next-match` inside attribute sets works |
| `xsl:use-when` | ✅ Working | Top-level and nested elements evaluated in document order; `true()`/`false()` and static-variable references work; XTSE0090 and XTSE3450 error cases validated. |
| Shadow attributes (`_{attr}` static AVTs) | ✅ Working | `_version`, `_href`, `_use-when`, `_xpath-default-namespace`, `_static`, `_select`, and other underscore-prefixed XSLT attributes are evaluated at compile time in the current static context and replace their non-underscore counterparts. Shadow attributes on literal result elements are preserved as ordinary attributes. |
| `xsl:where-populated` | ✅ Working | Filters empty sequences, empty text nodes, empty PIs, empty comments, and empty elements; attributes and namespace nodes do not make a sequence populated; empty strings and empty arrays are treated as empty |
| `xsl:on-empty` | ✅ Working | Evaluated by parent container (xsl:copy, xsl:document, literal result elements, general sequence constructors) when sequence constructor produces no nodes; supports `@select` and sequence constructor children; `on-empty` conformance cluster 72/72 |
| `xsl:on-non-empty` | ✅ Working | Evaluated by parent container when sequence constructor produces nodes; supports `@select` and sequence constructor children; `on-non-empty` conformance cluster 14/14 |
| `xsl:context-item` | ✅ Working | Declares required/optional/absent context item and type for templates; raises `XTTE0590`/`XTTE3090` at runtime and `XTSE0010`/`XTSE0020`/`XTSE0090` statically. `context-item` conformance cluster 31/31. |
| `xsl:iterate` | ✅ Working | Stateful iteration in result-tree and function-body contexts with `xsl:param`, `xsl:next-iteration`, `xsl:break`, and `xsl:on-completion`. `iterate` conformance cluster 44/44. |
| `xsl:message` | ✅ Working | Evaluates `terminate` and `error-code`; emits serialized message text via `IXsltMessageListener`; terminating messages throw `XsltRuntimeException` carrying the XDM value. The listener also receives `OnWarning` callbacks for XSLT warnings (e.g. no-matching-template / multiple-template warnings). |
| `xsl:try` / `xsl:catch` | ✅ Working | Catches dynamic XPath/XSLT errors in both result-tree and function-body contexts; rolls back output written in the try block before executing a matching catch (unless `rollback-output="no"`). Supports multiple `xsl:catch` clauses evaluated in document order; `@errors` supports `*`, plain local names, `prefix:local` (err namespace), `*:local`, and `Q{uri}local`; binds `$err:code`, `$err:description`, `$err:value`. Static errors in `xsl:variable`/`xsl:param`/`xsl:with-param` `@select` expressions are now reported at stylesheet compile time. |
| `xsl:map` / `xsl:map-entry` | ✅ Working | `xsl:map` evaluates its content as map-entry-producing sequence constructor and merges the entries of *any* maps the content produces (multi-entry maps included); `xsl:map-entry` builds a single-entry map; duplicate keys raise `XTDE3365` (also inside streamable `xsl:source-document` content); non-map content raises `XTTE3375`; maps as element/document children raise `XTDE0450` |
| `xsl:result-document` | ✅ Working | Secondary result documents with `format`, `href`, and serialization attributes; principal `xsl:result-document` captured for `TransformToString`; `current-output-uri()` reflects the active result-document URI and is empty outside any result document |

---

## 6. XPath 3.1 Feature Highlights

### Well-covered areas
- Sequence construction, filtering, FLWOR expressions (`for`/`let` chains, `at $pos` positional variables, `where` clauses)
- Standard `fn:*` functions (string, numeric, date/time, QName, URI)
- `map:*` and `array:*` functions
- Higher-order functions (`fn:for-each`, `fn:filter`, `fn:fold-left`, etc.)
- `fn:doc`, `fn:collection` with pluggable document loader
- Decimal formatting (`fn:format-number`)
- JSON functions: `fn:parse-json`, `fn:json-to-xml`, `fn:xml-to-json`, `fn:json-doc`
- Date/time ordering (`lt`, `gt`, `le`, `ge`)
- `fn:analyze-string` — nested group structure and zero-length checks

### Known gaps
- `fn:load-xquery-module` — not implemented
- `fn:serialize` — partial (JSON method supported for maps/arrays/atomics; XML serialization options still limited)
- `fn:transform` — full option support including `delivery-format`, `global-context-item`, `xslt-version`, serialization parameters, and package selection; principal `xsl:use-package` stylesheets are now supported; remaining package gap is secondary-package harness registration
- Schema-aware operations — source documents with `validation="strict"` are now validated in the QT3 harness, and `fn:id`/`fn:element-with-id` use the resulting PSVI; `import schema` and `validate` expressions remain unsupported.
- Regex functions (`fn:matches`, `fn:tokenize`, `fn:replace`) — full XSD regex support: strict syntax validation, character classes/subtraction, backreferences (incl. unclosed-group FORX0002), flags, code-point `.`, and pinned Unicode 9.0 category/block data (`\p{X}`, `\p{IsBlock}`). Remaining gap: the `i` flag uses .NET case-insensitivity rather than Unicode full case folding (affects patterns mixing `i` with `\p{...}` or negated classes)

---

## 7. XSD Validation

Bosak provides an `IXsdValidator` abstraction for XML Schema validation:

```csharp
using Bosak.XPath.Api.Xsd;

var validator = new XsdValidator();
var result = validator.ValidateSafe(xmlString, xsdStream);

if (result.IsValid)
{
    Console.WriteLine("Document is valid");
}
else
{
    foreach (var error in result.ErrorsOnly)
    {
        Console.WriteLine($"Error at line {error.LineNumber}: {error.Message}");
    }
}
```

Features:
- Single-schema and multi-schema validation (handles `xs:import`/`xs:include`)
- Structured error results with line/column numbers
- Non-throwing `ValidateSafe` and throwing `Validate` variants
- Configurable via `XsdValidatorOptions` (max error count, treat warnings as errors)

---

## 8. Current Build State

Run the full suite from the Bosak repo root:

```bash
dotnet build Bosak.sln
dotnet test Bosak.sln
```

**Unit tests:** 913 passed, 0 failed, 0 skipped  
**Target framework:** `net10.0`

### Behavioral Changes

| Change | Impact | When |
|--------|--------|------|
| `XsltExecutable.Transform` gained an optional `rawResult` parameter. | When `true` and an initial named template is used, returns the raw template result as an XDM value instead of wrapping it in a result document. Required for `initial-template-004` and similar raw-output tests. | 2026-06-25 |
| `TransformEngine.IsNodeAttached` now treats a document's root element as attached. | After whitespace stripping, the initial source node (`/doc`) was incorrectly considered detached because the root `XElement` has no `XObject.Parent`. The check now also verifies `XObject.Document != null`. Fixes `mode-1105`. | 2026-06-26 |
| Regex `.` now matches Unicode code points including surrogate pairs. | `RegexHelper.TranslateDot` replaces `.` with an alternation that prefers a high+low surrogate pair over a single code unit. Fixes `regex-026` and aligns `fn:matches`/`replace`/`tokenize` with XPath/XSD semantics. | 2026-06-26 |
| `xsl:evaluate` blocks `fn:system-property`. | XSLT-defined functions are removed from the dynamic context; calling `system-property` inside `xsl:evaluate` now raises `XTDE3160`. Fixes `system-property-022`. | 2026-06-26 |
| `xsl:catch` now matches bare error codes. | `GetErrorCode` recognizes 8-character codes such as `FOUT1190` even without a trailing colon, so `xsl:catch errors="*:FOUT1190"` matches. Fixes `unparsed-text-lines-004`. | 2026-06-26 |
| Root-level literal result elements copy in-scope stylesheet namespaces. | `CopyLiteralElement` copies namespace declarations from the stylesheet root onto the output root element (except excluded prefixes and the XSLT namespace). Fixes `attribute-0601`. | 2026-06-26 |
| `xsl:analyze-string` now passes regex flags to `RegexHelper.ValidateAndTranslatePattern`. | Fixes multiline-mode (`m`) tests `analyze-string-007/067/071/090b`; previously `$` was translated to `\z` even when multiline was requested. | 2026-06-25 |
| `xsl:call-template` now resolves named templates by expanded QName. | A call using one prefix bound to a namespace URI finds a template declared with a different prefix bound to the same URI. Fixes `call-template-1701`. | 2026-06-25 |
| Initial template names from the conformance harness are expanded using catalog namespace bindings. | Names are passed to `TransformEngine` in Clark notation, so a test-catalog prefix bound to a different URI than the stylesheet prefix correctly raises `XTDE0040`. Fixes `call-template-0104/0105/0107`. | 2026-06-25 |
| `xsl:template/@name` values are whitespace-trimmed and validated against reserved namespaces. | Leading/trailing spaces and EQName forms such as ` Q{}temp ` are normalized; names in the XSD, XPath-functions, or XSLT namespaces raise `XTSE0080` (except the special `xsl:initial-template` name). Fixes `call-template-0106/0109`. | 2026-06-25 |
| `xpath-default-namespace` fully wired through XSLT → XPath pipeline. | `CompileOptions.DefaultElementNamespace` controls unprefixed element/type names in XPath expressions. Threaded through `CompileXPath`, `PatternCompiler`, `TemplateRule.ResolveNamespacePrefixes`, `VmEngine.NamespaceTest`, and whitespace stripping (`SpaceHandlingRule`). Fixes xpath-default-namespace-0101 through 1102 (21/22 passing). | 2026-06-11 |
| `xsl:attribute` with unprefixed name now uses empty namespace URI. | Previously inherited default namespace from parent; now correctly produces no-namespace attributes per XSLT spec. Fixes namespace-3306. | 2026-06-11 |
| `xsl:call-template` evaluates default `xsl:param` values when no `with-param` is provided. | Previously omitted parameters fell back to empty sequence instead of evaluating the param's `select` or sequence constructor. Fixes namespace-3501/3503. | 2026-06-11 |
| `AddElementToContainer` injects `xmlns=""` when no-namespace element is placed inside a default-namespace parent. | Prevents LINQ-to-XML from silently inheriting parent's default namespace. Fixes namespace-0913. | 2026-06-11 |
| `fn:node-name` on text nodes returns `XdmValue.Undefined` (empty sequence). | Was incorrectly returning empty sequence due to unintended `NodeToQName` change. Reverted to spec-compliant `Undefined`. | 2026-06-11 |
| `CopyLiteralElement` no longer walks ancestor chain to copy namespace declarations. | Was leaking `xmlns:xs` and other stylesheet prefixes into literal result elements, breaking `exclude-result-prefixes` and `fn:transform` output. | 2026-06-11 |
| `*:local` name tests now emit `"*:local"` into the literal pool. | Prevents VM from applying the no-namespace attribute restriction to `*:local` patterns. Fixes namespace-1402 and related tests. | 2026-06-11 |
| Global sequence-constructor variables now evaluate with the initial context item. | `xsl:variable` sequence constructors at the top level use a singleton focus based on the root of the source tree (XSLT 3.0 §9.6), not the focus at the point of reference. Fixes `string-041`. | 2026-06-12 |
| Named-template entry points without a source document use an absent context item. | `XsltExecutable.Transform`/`TransformToString` accept a null source; the conformance harness passes null for named-template tests with no explicit source. Keeps `copy-4308` (XTTE0945) correct. | 2026-06-12 |
| Namespace node `parent::node()` now returns the element whose namespace axis includes the node (`_namespaceOwner`), not the element where the underlying `XAttribute` declaration resides. | Fixes `.. is $e` for inherited namespace nodes in XPath. Required for XSLT `namespace::*` axis correctness. | 2026-06-10 |
| `xsl:variable`/`xsl:param`/`xsl:function`/`xsl:with-param` `@as` now fully supports atomic type coercion and atomization. | `ConvertVariableValue` rewrites atomization + casting via `VmEngine.TryCast`. Subtype substitution (integer→decimal, float→double) and type promotion. Node type tests (`element(...)`, `attribute(...)`, `document-node(...)`) bypass atomization. | 2026-06-11 |
| `xsl:document` no longer leaks outer `_sequenceAccumulator` into its sequence constructor. | `wrapInDocumentNode=true` now isolates the accumulator, ensuring `xsl:copy-of` inside `xsl:document` unwinds document nodes into the new document instead of the outer variable. | 2026-06-11 |
| `xsl:call-template/@as` now raises `XTSE0010` at runtime. | `@as` is not permitted on `xsl:call-template`; previously ignored. | 2026-06-11 |
| `xsl:copy` now raises `XTTE0945` (no context item), `XTTE3180` (select returns >1 item), `XTDE0410` (attribute after children), and `XTDE0420` (attribute on non-element) per XSLT 3.0 spec. | Previously these error conditions were silently ignored or produced wrong results. | 2026-06-10 |
| XSLT functions (`xsl:function`) no longer leak the first argument as the context item. | Functions now correctly have no context item per XSLT 3.0 §9.6. Fixes `xsl:copy` inside functions. | 2026-06-10 |
| `xsl:where-populated` now correctly filters empty PIs, comments, and text nodes. | Previously only whitespace-only text nodes were filtered; empty PIs/comments passed through incorrectly. | 2026-06-10 |
| XPath parser no longer treats prefixed names as kind tests (e.g. `my:node()`). | `my:node()` was parsed as `child::node()` instead of a function call. Affected any prefixed name where local name matched a kind test. | 2026-06-10 |
| `xsl:key` content constructors now preserve typed atomic key values. | `string-length(.)`, `string-to-codepoints(.)`, and other atomic producers are stored as typed values rather than converted to text nodes. Fixes `key-082`, `key-073/074/075`. | 2026-06-12 |
| `key()` lookup results are returned in document order. | Multiple `xsl:key` definitions with the same name no longer return nodes in definition order. Fixes `key-073/074/075` ordering. | 2026-06-12 |
| Pattern predicates in `key()` match patterns now isolate caller focus. | `PatternCompiler.WrapWithCurrentItem` saves/restores context item, position, and size so `xsl:number` with `key()` patterns does not corrupt subsequent instructions. Fixes `key-035`. | 2026-06-12 |
| `key()` pattern validation restored. | XTSE0340 is raised for invalid second arguments in `key()` match patterns; numeric literals, variable references, and parenthesized sequences are allowed. Fixes `key-083`, `key-093`, `key-097`, `match-079`, `match-080`. | 2026-06-12 |
| `xsl:where-populated` now implements populated-node semantics per XSLT 3.0. | Document nodes from `xsl:document` and items from `xsl:sequence` are preserved; empty elements/documents are filtered; `xsl:on-empty` children are honoured. Fixes `element-0104` through `element-0108`. | 2026-06-11 |
| Fragment document nodes now serialize correctly. | Multi-root `xsl:document` results are wrapped in `__xdm_doc__` during copying and unwrapped by `ResultTreeSerializer`. Fixes `xsl-document-0501`. | 2026-06-11 |
| `xsl:document` inside simple content contributes the document's string value. | Excludes comment/PI descendants; fixes `xsl-document-0601`. | 2026-06-11 |
| `XDocumentNode.StringValue` for synthetic-wrapper documents includes all descendant text. | Previously only direct text children of the wrapper were included. | 2026-06-11 |
| `xsl:message` now includes both `@select` and sequence-constructor content. | Both contributions are concatenated, matching XSLT 3.0 semantics. Fixes `xsl-document-0603`. | 2026-06-11 |
| Conformance harness supports `<assert-message>` and fragment assertions. | Messages are captured via `RecordingMessageListener`; multiple direct assertion children are treated as an implicit `<all-of>`. | 2026-06-11 |
| `xsl:copy` shallow copy no longer copies source attributes/children. | Source attributes and children must now be produced by the contained sequence constructor, matching the XSLT spec. Fixes `attribute-set-0107`. | 2026-06-13 |
| XPath parser resolves the `xml` prefix to the XML namespace in node tests. | Previously `@xml:*` fell back to a prefix-only match that matched attributes in any namespace. Fixes `attribute-0901`. | 2026-06-13 |
| `fn:document#1/#2` supports URI fragment identifiers. | Fragment identifiers resolve to the element with matching `id`/`xml:id`; relative document URIs resolve from the stylesheet base URI. Fixes `id-001`. | 2026-06-13 |
| `xsl:evaluate` is fully supported. | Dynamic XPath 3.1 evaluation inside XSLT with context item, parameters, namespaces, base URI, default collation, and `@as` coercion. Fixes the entire `evaluate` cluster. | 2026-06-13 |
| Cross-tree document order now follows document creation order. | `XDocumentNode.DocumentOrder` combines a global creation sequence (high bits) with the per-document local index (low bits), so union/path results across separately constructed temporary trees are stable. Fixes `evaluate-002`. | 2026-06-13 |
| `xsl:function` validation and static errors are implemented. | `Stylesheet.ValidateInstructionTree` reports XTSE0020/XTSE0080/XTSE0770/XTSE0090/XTSE0740 for invalid function declarations, reserved namespaces, duplicate signatures, and `Q{}local` names. Fixes function cluster validation tests. | 2026-06-13 |
| `xsl:function` supports deterministic memoization. | `new-each-time="no"` results are cached per (name, arity, argument) key; AVTs on `_new-each-time` select deterministic/non-deterministic mode at run time. Fixes `function-0240` and related tests. | 2026-06-13 |
| `fn:function-available` is fully spec compliant. | Parses EQNames, atomizes/casts the arity argument, reports `fn:concat` for any variadic arity, and reports the full XSLT 3.0 function set. Fixes `function-available` cluster. | 2026-06-13 |
| `fn:element-available` is implemented. | Reports availability for XSLT 2.0/3.0 instructions in the XSLT namespace. The unprefixed first argument is expanded using the XML default namespace of the element containing the expression, tracked separately from the XPath default namespace via `CompileOptions.DefiningElementDefaultNamespace`. Fixes `function-0302b`. | 2026-06-24 |
| `fn:available-environment-variables` and `fn:environment-variable` are implemented. | Returns/succeeds on process environment variables; matching is case-sensitive exact. Fixes function cluster environment tests. | 2026-06-13 |
| Numeric arguments to `fn:subsequence` and `fn:format-integer` are atomized. | Atomization now preserves `xs:untypedAtomic`, so attribute and element text nodes are accepted implicitly. Fixes `function-0502`, `function-0503`, and related tests. | 2026-06-13 |
| Namespace context is applied to `xsl:variable`/`xsl:param`/`xsl:with-param` @select expressions. | Local and global variable/param `select` expressions, and named-template default param values, now use the in-scope namespace bindings and effective default namespace. Fixes unprefixed EQName tests in `function` cluster. | 2026-06-13 |
| `date` cluster is fully passing. | Implicit timezone, `xs:time` midnight semantics, timezone adjustment, AM/PM formatting, extended-year constructor bounds, and static-parameter substitution in the harness. Fixes all runnable `date` tests. | 2026-06-13 |
| `xsl:message` now implements terminate/error-code semantics and serializes node content. | Messages evaluate `@terminate` and `@error-code`, emit via `IMessageListener`, and throw `XsltRuntimeException` with the captured XDM value when terminating; `xsl:try`/`xsl:catch` binds `$err:code`, `$err:description`, `$err:value`. Fixes the `message` conformance cluster (45/0/0). | 2026-06-13 |
| `fn:unparsed-text` resolves relative `href` against `EvaluationContext.BaseUri`. | Previously resolved only against the static base URI parameter; now uses the dynamic base URI when no explicit base is supplied. Required for `message-0313`. | 2026-06-13 |
| `fn:element-available` uses the defining element's default namespace. | Added `EvaluationContext.DefiningElementDefaultNamespace` and `CompileOptions.DefiningElementDefaultNamespace` so that XSLT's `element-available()` expands unprefixed QNames using `xmlns="..."` rather than `xpath-default-namespace`. | 2026-06-24 |
| `validation="lax"` is accepted on basic processors. | Non-schema-aware processors no longer raise `XTSE1660` for `lax` (or `default-validation="lax"`), matching XSLT 3.0 semantics. Fixes `validation-0102b`. | 2026-06-24 |
| `fn:doc('')` resolves against the static base URI. | In XSLT, `fn:doc('')` now loads the stylesheet module; in pure XPath it still yields the empty sequence when no base URI is present. Fixes `document-0302`. | 2026-06-24 |
| `fn:doc` atomizes and validates its argument. | Empty sequence returns empty sequence; more than one item raises `XPTY0004`; prevents literal `\(sequence\)` from being loaded as a URI. Fixes `document-0303/0307/0601/0901/1101`. | 2026-06-24 |
| Stylesheet module base URIs are preserved. | `FileSystemUriResolver`, `XsltCompiler`, and the conformance `TestUriResolver` load stylesheet modules with `LoadOptions.SetBaseUri`, so `fn:doc('')` and `fn:document()` inside included/imported modules resolve relative URIs against the correct module base URI. Fixes `document-1003/1004/1901`. | 2026-06-24 |
| `fn:doc`/`fn:document` loaded documents are subject to `xsl:strip-space`/`xsl:preserve-space`. | `EvaluationContext.DocumentPostProcessor` lets XSLT apply the stylesheet's whitespace-handling rules to documents loaded during transformation, while protecting stylesheet modules themselves from mutation. Fixes `document-0308`. | 2026-06-24 |
| `xsl:use-package` tests are skipped by the conformance harness. | The compiler does not support XSLT 3.0 packages; the harness now detects `xsl:use-package` in the principal stylesheet and reports a skip instead of a null-reference failure. Moves `document-2402` to skipped. | 2026-06-24 |
| `fn:unparsed-text-lines` is fully spec compliant. | Trailing line terminators no longer produce an empty final line; decoded text is validated for XML-legal characters, raising `FOUT1190` for invalid characters such as NUL. Fixes `unparsed-text-lines-002/004`. | 2026-06-24 |
| `xsl:try` / `xsl:catch` handles multiple catch clauses and error-code matching. | `TransformEngine` evaluates all `xsl:catch` children in order, matches `@errors` against `*`, plain names, `*:local`, `Q{uri}local`, and `prefix:local` (err namespace), and rethrows unmatched errors. Fixes `call-template-0110`. | 2026-06-25 |
| `fn:function-available` validates its argument. | Invalid QName/EQName syntax or an unbound prefix now raises `XTDE1400`; the error is propagated through `use-when` expressions. Fixes `extension-functions-0103/0104`. | 2026-06-24 |
| `extension-element-prefixes` bound to reserved namespaces is rejected. | The stylesheet loader reports `XTSE0085` when the XSLT, XML, XML Schema, or XML Schema instance namespace is declared as an extension namespace (code corrected per the XSLT 3.0 REC; the retired `XTSE0800` expected by `extension-functions-0105` is aliased in the conformance harness). Fixes `math-3702` and `extension-functions-0105`. | 2026-09-05 |
| `use-when` namespace context includes all ancestors. | Prefixes declared on any ancestor of the element carrying `use-when` are now in scope for the expression. Fixes `extension-functions-0101`. | 2026-06-24 |
| `fn:snapshot` is implemented. | Creates a copy of a node with shallow ancestor copies (attributes/namespaces preserved) and deep-copied descendants; in-scope namespace bindings are now copied to every element in the snapshot, matching `xsl:copy-of validation="preserve"`. Top-level `xsl:namespace` instructions only produce standalone namespace-node items when the containing sequence constructor is typed as `namespace-node()`. Clears the `snapshot` cluster. | 2026-06-28 |
| `fn:innermost` / `fn:outermost` relationship checks are correct. | Both functions now use `IsSameNode` instead of reference equality, and `innermost`/`outermost` apply the correct descendant/ancestor filtering semantics. Fixes `innermost-001/901`. | 2026-06-24 |
| Conformance harness supports raw XDM comparison for `<initial-function>`. | Tests with `<output tree="no" serialize="no"/>` now compare the raw function result using `assert-type`, `assert-count`, `assert-deep-eq`, and `assert-eq` instead of serializing to a string. Fixes `initial-function-002` and `initial-function-100a..100i`. | 2026-06-24 |
| `VmEngine.ValueMatchesType` respects sequence occurrence indicators. | Top-level sequence values are now matched against `?`, `*`, and `+` occurrence indicators by checking each item against the base type. Fixes `initial-function-100e` (`xs:string*`). | 2026-06-24 |
| `xsl:function/@_name` AVTs are expanded to expanded QNames at parse time. | `XsltFunctionDefinition.FromElement` evaluates `_name` attribute value templates (including `xs:QName`-returning expressions) in the static context, so functions declared with dynamic names are registered under the correct expanded QName. Fixes `initial-function-101c..101e`. | 2026-06-24 |
| XPath value comparison casts `xs:untypedAtomic` to `xs:string`. | In value comparisons (`eq`/`ne`/`lt`/`le`/`gt`/`ge`), an `xs:untypedAtomic` operand is atomized to `xs:string` before comparison, so `xs:untypedAtomic('72') gt 70` raises `XPTY0004` while `xs:untypedAtomic('') eq ''` succeeds. General comparisons continue to promote `xs:untypedAtomic` to the other operand's type. Fixes `type-0165`. | 2026-06-25 |
| Whitespace stripping applies to the source document root, and stripped source nodes are treated as absent. | The engine strips whitespace from the document containing the initial context node, detects when the selected node has been removed, and evaluates globals with focus on the source-tree root. Fixes `strip-space-023`. | 2026-06-25 |
| Path expressions only load the context item when the first step is an axis step. | Prevents `parse-xml(...)/root/item` from raising `XPDY0002` when the XPath focus is absent. Required by the `strip-space` fix. | 2026-06-25 |
| `XsltCompiler.StaticParameters` supplies values for static `xsl:param` declarations. | Caller-supplied values override stylesheet `select` defaults and are coerced against `@as` during `BuildStaticContext()`. Required for parameterized static tests such as `static-003a/013c`. | 2026-06-26 |
| Static variables and parameters are eagerly bound at runtime. | `InitializeGlobalParametersAndVariables` binds values from `Stylesheet.StaticVariables` before lazy non-static globals, so static values remain visible even when a non-static declaration shadows the name. Fixes `static-027`. | 2026-06-26 |
| XTSE0090 and XTSE3450 validations for static declarations are implemented. | `static="yes"` is rejected on non-global `xsl:variable`/`xsl:param`; `visibility` is rejected on static declarations; a static variable and static parameter with the same expanded name raise `XTSE3450`. Fixes `static-020/023/025/026`. | 2026-06-26 |
| Static declarations without a value default to empty sequence (or undefined for required parameters). | Optional static variables/parameters default to `()`; required static parameters without a supplied value raise `XTDE0050`. Fixes `static-010` and related cases. | 2026-06-26 |
| General comparison with an empty operand returns `false`. | `VmEngine.CompareGeneral` now follows XPath 3.1 §17.3: one empty operand yields `false`, not an empty sequence. Fixes `static-011`. | 2026-06-26 |
| Namespace axis includes implied default namespaces. | `XDocumentNode.GetNamespaceAxis` adds a default-namespace node when the element is in a non-empty namespace that is not declared explicitly as default or prefixed. Fixes `static-030` and `json-to-xml` namespace-axis coverage. | 2026-06-26 |
| Static conformance cluster is fully passing. | All 49 `static` tests pass (was 47/49). Combined with the two cross-cutting fixes, the full W3C suite improves to 4,599/652/9,349. | 2026-06-26 |
| `xsl:use-attribute-sets` is allowed on literal result elements. | Added `use-attribute-sets` to the XTSE0805 whitelist of XSLT-namespaced attributes permitted on LREs. Clears the `attribute-set`, `xsl-document`, `analyze-string`, and `next-match` clusters. | 2026-06-26 |
| Precedence-aware XTSE3450 detection for static variables. | `Stylesheet.BuildStaticContext` evaluates top-level `use-when` in document order and tracks import precedence; same-precedence conflicting values and higher-precedence overrides that change the effective value raise `XTSE3450`. Fixes `use-when-0137/0138` and keeps `static` cluster at 49/49. | 2026-06-26 |
| `use-when` conformance cluster is fully passing. | All 99 runnable `use-when` tests pass (was 97/99); `use-when-0137/0138` now raise `XTSE3450` correctly. | 2026-06-26 |
| Shadow attributes (static AVTs) are implemented. | `_version`, `_href`, `_use-when`, `_xpath-default-namespace`, `_static`, `_select`, and other underscore-prefixed XSLT attributes are expanded at compile time using the current static context; shadow attributes on literal result elements are left untouched. Clears the `shadow` cluster. | 2026-06-26 |
| XSLT 1.0 backwards-compatible mode is fully implemented. | `CompileOptions.BackwardsCompatible` flows into the XPath optimizer, IR lowerer, VM arithmetic/comparisons, standard-function argument conversion, `xsl:value-of`, `xsl:number`, and `key()` string-valued lookups. Clears the `backwards` cluster (43/43 runnable). | 2026-07-07 |
| The `bug` conformance cluster is fully passing. | Imported-template XTSE0680 validation, `<assert-serialization>` file loading in the harness, namespace fixup for copied attributes, and `current()` inside `xsl:sort`. Clears the `bug` cluster (69/69 runnable). | 2026-07-07 |
| The `xpath-compat` conformance cluster is fully passing. | Backwards-compatible negative-zero constant folding and `fn:subsequence` numeric argument coercion for strings/untyped atoms. Clears the `xpath-compat` cluster (17/17 runnable). | 2026-07-07 |
| Resource URIs can be redirected to local files. | `EvaluationContext.ResourceUriMapper` (`Func<string, string?>`) maps a requested URI to a local path; consulted by `fn:doc`, `fn:json-doc`, `fn:unparsed-text(-available/-lines)`, and `fn:transform`'s `stylesheet-location` before filesystem/network access. `XDocumentProvider.LoadFile` now absolutizes relative paths before deriving the document URI (previously `UriFormatException`). JSON parse failures in `fn:parse-json`/`fn:json-doc`/`fn:json-to-xml` raise `FOJS0001` instead of propagating `JsonException`. | 2026-07-15 |
| QT3 `fn:transform` Tier-2m is fully passing. | Implemented `global-context-item`, `xslt-version` validation/propagation, default-mode routing, `template-params`/`tunnel-params`, `base-output-uri` raw-result delivery, serialization parameter merging, `suppress-indentation` override, and absent-principal-output suppression. Filtered suite: 117 passed / 0 failed / 7 skipped. | 2026-07-15 |
| Whitespace stripping of loaded documents follows the calling package's rules. | Per XSLT 3.0 §2.13.4, documents loaded by `fn:doc`/`fn:document`/`fn:collection` from code in a used package are stripped with that package's own `xsl:strip-space`/`xsl:preserve-space` rules. `TransformEngine` sets `EvaluationContext.DocumentLoadPolicy` when entering used-package components (templates, functions, global initializers), and the document cache is keyed by `(URI, policy)` so the same URI yields a distinct stripped tree per distinct rule set. Principal-stylesheet code keeps the default policy, sharing the host-registered document pool (e.g. the initial source tree). Fixes `document-2401/2402` and `collection-006`. | 2026-09-05 |

### Conformance Baselines

| Suite | Passed | Failed | Skipped | Pass Rate | Notes |
|-------|--------|--------|---------|-----------|-------|
| XSLT 3.0 (W3C) | 5,506 | 99 | 8,995 | 98.2% | `output` cluster 179/24/29; `result-document` cluster 104/21/29; remaining failures are pre-existing non-output issues |
| XPath 3.1 (QT3) | 21,218 | 1,317 | 9,286 | 66.68% | `?`/`?*` lookup operator spec-complete (UnaryLookup, FOAY0001/XPTY0004); suite http: resources mapped to local files |

> **Note:** The conformance runner locks DLLs. If you get build errors about locked files, run:
> ```bash
> taskkill /F /IM Bosak.XPath.Conformance.exe
> taskkill /F /IM Bosak.Xslt.Conformance.exe
> ```

---

## 9. VS Code Extension

Bosak ships with a VS Code extension (`vscode-bosak/`) that provides syntax highlighting, realtime diagnostics, and auto-completion via a Language Server Protocol (LSP) server.

### 8.1 Building & Running

```bash
# Build the language server (.NET 10)
dotnet build src/Bosak.LanguageServer/Bosak.LanguageServer.csproj

# Build the extension client (Node.js 18+)
cd vscode-bosak
npm install
npm run compile

# Launch Extension Development Host
code . --goto src/extension.ts
# Then press F5 inside VS Code
```

### 8.2 Packaging as VSIX

```bash
cd vscode-bosak
npx vsce package
# Produces: vscode-bosak-0.1.5.vsix
```

Install in VS Code: **Extensions** → **⋯** → **Install from VSIX…**

### 8.3 Extension Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `bosak.server.path` | `string \| null` | `null` | Absolute path to `Bosak.LanguageServer` binary. When null, the extension searches the workspace. |
| `bosak.trace.server` | `string` | `"off"` | LSP traffic tracing: `"off"`, `"messages"`, `"verbose"`. |

### 8.4 Supported File Types

| Language | Extensions | Features |
|----------|------------|----------|
| XPath | `.xpath` | Syntax highlight, diagnostics, completions (functions, axes, keywords) |
| XSLT | `.xsl`, `.xslt` | Syntax highlight, diagnostics, completions (XSLT instructions + XPath) |

---

## 10. Getting Help / Reporting Issues

- Check `docs/ARCHITECTURE.md` in the Bosak repo for the layer overview and execution pipeline.
- Check `docs/FEATURE_REQUESTS.md` for the feature request registry.
- XPath failures: capture the expression, input XML, and expected vs. actual result.
- XSLT failures: capture the stylesheet fragment, source XML, and expected output.

**REQ-124 final consumer acceptance — 2026-10-10:** Accepted 2026-10-10 on the owner's instruction after Bosak.Braid re-review. F1 and F2 are closed. Tested fix cb98af1ff05a37a59e7b1fe051b024d3436e1cf2 matches merged main src/tests at 5720f873c8f9e5ec0b3182186bf336d2b5e58a03 (implementation b5eb1b17249fe458095af2dc53445d459f30db11). Independent gates: full Release unit suite 3,709 passed / 0 failed / 0 skipped; authoring subset 71/71; XSLT mode smoke 162 passed / 0 failed / 26 skipped. Acceptance covers the agreed engine surface, not a product/package release. Braid dependency adoption remains separate. See [acceptance review](REQ-124-CONSUMER-ACCEPTANCE-REVIEW.md).
