<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-126 grammar coverage and acceptance reconciliation">
  <h1>REQ-126 coverage and acceptance reconciliation</h1>
  <p>23 October 2026 - source 1.23.7; public 1.23.4</p>
</div>

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

**Public1.23.7 - 23 October2026:** [Signed public packages and consumers are verified](../RELEASE-1.23.7-PUBLICATION.md). Current inventory records source/public1.23.7; Braid dependency1.23.4 is unchanged. Earlier checkpoint versions/counts below remain historical. REQ-126/AC-03 and D2/D3/D4 remain partial.


## Decision and evidence boundary

REQ-126 remains **In Progress**. The initial reconciliation against main `47b4f3a9c1ec624f53407101f261d0c2ae55ca65` changed only documentation and the owner-approved AC-03 evidence policy. The subsequent [record-contract slice](RECORD-CONTRACTS.md) includes a bounded inline-record signature gate correction in unpublished source1.23.6. Public and Braid dependency remain1.23.4; no tier/gate promotion or release is implied. Initial delivery/hash receipts remain historical evidence.

At the initial reconciliation checkpoint the catalog had **71 entries /2,767 documentation checks**. The [focus-function syntax delivery](FOCUS-FUNCTION-SYNTAX.md) adds one entry and 30 checks: the subsequent [record-contract delivery](RECORD-CONTRACTS.md) adds another entry and46 checks; the numeric literal delivery adds one entry and50 checks, so current **76 entries, 3,148 feature +24 chapter checks =3,172**, four worked chapters and 117 assigned function names. All 76 entries have the 16 numbered fields and an index link. Those structural checks do not establish complete grammar, signatures, semantic drift or conformance.

The reproducible [inventory script](audit-reconciliation.py) reads all 76 entries, the fixture manifest, existing provenance ledger and current `GatedSets`; it validates the named methods and files in the [18-row grammar matrix](GRAMMAR-COVERAGE-MATRIX.json). Run `python docs/xpath40-catalog/audit-reconciliation.py` from any directory. [Inventory output](COVERAGE-RECONCILIATION.json) records literal citations, counts and input hashes. It does not infer a test-set promotion or semantic pass from a file link.

## D3 - source, tests and catalog coverage

The REQ-118 planning table explicitly lists S1-S8. The older completeness audit labels compatibility setup S0; the row below retains that bookkeeping label rather than inventing a separate landed grammar slice. Function waves are mapped to D2 instead of being counted as syntax. Source files and additional named tests are in the JSON matrix; each row below identifies existing evidence and the remaining boundary.

| Family | Existing evidence | Remaining D3 boundary |
|---|---|---|
| S0: Compatibility and arity-sensitive visibility | [README](README.md), [fn-scan](experimental/fn-scan.md); [Compile_ExperimentalFunctions_FrozenXPath40_ThrowXpst0017](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [FunctionLookup_ScanInXPath40Experimental_ReturnsFunction](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) | Reconcile every family and host/local-profile boundary; promotion of a test set does not freeze fn:scan. |
| S1-S2-S5: Function waves and HOF contracts | [SIGNATURE-CONTRACT-AUDIT](SIGNATURE-CONTRACT-AUDIT.md), [CHANGED-31-CONTRACT-INVENTORY](CHANGED-31-CONTRACT-INVENTORY.md); [Compile_HigherOrderFunctions_DefaultOptions_ThrowXpst0017](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) | D2 signature/default/options and callback conversion remain open; registered names are not grammar completeness. |
| S3-otherwise: Otherwise and guarded focus | [otherwise-operator](operators-and-syntax/otherwise-operator.md); [Evaluate_Otherwise_RightOperandGetsContainingFocus](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [Evaluate_Otherwise_RightOperandIsGuarded](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) | Complete guarded-expression/type/error timing matrix; distinguish empty sequence from truthiness. |
| S3-keywords: Keyword arguments | [keyword-arguments](operators-and-syntax/keyword-arguments.md); [Compile_PositionalAfterKeyword_XPath40Mode_ThrowsXpst0003](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [Compile_UnknownKeyword_XPath40Mode_ThrowsXpst0017](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) | D2 per-function keyword metadata/defaults and known spelling drift remain separate. |
| S3-lexical: Binary literals, separators and string templates | [string-templates](operators-and-syntax/string-templates.md); [Evaluate_HexBinaryLiterals_XPath40Mode_Work](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [Compile_StringTemplate_31Mode_ThrowsXpst0003](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) | [Dedicated numeric entry](operators-and-syntax/numeric-literals.md) and50 checks supplied; broader numeric/host boundaries remain open. |
| S4-focus: Pipeline, mapping arrow and focus-function syntax | [pipeline-operator](operators-and-syntax/pipeline-operator.md), [mapping-arrow](operators-and-syntax/mapping-arrow.md), [focus-functions](operators-and-syntax/focus-functions.md), [focus-function-syntax](operators-and-syntax/focus-function-syntax.md); [Evaluate_Pipeline_SetsFixedFocusPositionOneSizeOne](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [Evaluate_Pipeline_FocusRestoredAfterEvaluation](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [Evaluate_FocusFunction_BindsArgumentAsWholeContextValue](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [Compile_FocusFunction_31Mode_ThrowsXpst0003](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) | Dedicated fn { ... } / fn abbreviation entry, 30 public examples and paired business reports supplied. Constructor page remains separate; exhaustive focus/coercion/callback/host combinations are not certified. |
| S4-iteration: Member/key iteration including sequences of containers | [array-member-iteration](operators-and-syntax/array-member-iteration.md), [map-entry-iteration](operators-and-syntax/map-entry-iteration.md), [sequence-collection-iteration](operators-and-syntax/sequence-collection-iteration.md); [Compile_ForMember_31Mode_ThrowsXpst0003](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [Evaluate_ForMember_SequenceOfArraysCountsPositionsAcrossArrays](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) | Complete position/cardinality/coercion and per-production mapping; older moving-draft deferral is superseded only within delivered scope. |
| S6-types: Enums and choice item types | [enum-types](type-system/enum-types.md), [choice-types](type-system/choice-types.md), [TYPE-PATTERN-GRAMMAR](TYPE-PATTERN-GRAMMAR.md); [Evaluate_Enum_Castable_ReportsMembership](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [Evaluate_Choice_Cast_FirstCoercibleAlternativeWins](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) | Casting versus in-order coercion, cardinality, nested values and namespace/type aliases need systematic contract coverage. |
| S6-record-conversion: Record casts, coercion and lookup | [structural-records](type-system/structural-records.md), [typed-let-coercion](type-system/typed-let-coercion.md); [Cast_DiscardsSurplusKeys](../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs), [Coercion_SurplusKeys_XPTY0004](../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs), [Coercion_PromotesFieldValuesRecursively](../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs), [Lookup_UnknownFieldOnRecord_XPTY0004](../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs), [RecordSignatures](../../tests/Bosak.XPath.Api.Tests/InlineRecordTypeVersionGateTests.cs) | Bounded cast/coercion/lookup comparisons, 46 public cases and paired stock reports supplied. Full composite/nominal/static/default-field and host coverage remain unclaimed. |
| S6-update: Record but-with updates | [structural-records](type-system/structural-records.md), [record-updates](type-system/record-updates.md); [ButWith_ResultKeepsRecordAnnotation](../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs), [ButWith_UnknownField_XPTY0004](../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs), [ButWith_CoercesRhsValuesToFieldTypes](../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs) | Dedicated entry and complete stock task supplied; recursive/optional/order/annotation cases are bounded, not exhaustive named-record or host contracts. |
| S7: XSLT note/if/map and instruction separators | [xsl-note](xslt-instructions/xsl-note.md), [xsl-if](xslt-instructions/xsl-if.md), [xsl-map](xslt-instructions/xsl-map.md), [instruction-separators](xslt-instructions/instruction-separators.md); [ProcessorProfileTests](../../tests/Bosak.Xslt.Tests/ProcessorProfileTests.cs) | Complete instruction forms, static errors, module/package/authoring contexts and version/host combinations. |
| S8-dispatch: XSLT array, switch and select | [SWITCH-ARRAY-SELECT-GRAMMAR](SWITCH-ARRAY-SELECT-GRAMMAR.md); [Xslt40ArraySwitchTests](../../tests/Bosak.Xslt.Tests/Xslt40ArraySwitchTests.cs), [Xslt40SelectTests](../../tests/Bosak.Xslt.Tests/Xslt40SelectTests.cs) | Bounded mapping is supplied; independent upstream review, full host/profile and all native/fallback cases remain separate. |
| S8-patterns: Aliases, records and map/array patterns | [TYPE-PATTERN-GRAMMAR](TYPE-PATTERN-GRAMMAR.md); [Xslt40ItemTypeTests](../../tests/Bosak.Xslt.Tests/Xslt40ItemTypeTests.cs), [Xslt40RecordPatternTests](../../tests/Bosak.Xslt.Tests/Xslt40RecordPatternTests.cs) | Do not infer full named-type declaration/module/package or Draft-host coverage. |
| S8-compound: Compound patterns, priorities and EQNames | [COMPOUND-PATTERN-GRAMMAR](COMPOUND-PATTERN-GRAMMAR.md), [UNION-MINIMUM-PRIORITY](UNION-MINIMUM-PRIORITY.md), [EQNAME-BRACKET-PARSING](EQNAME-BRACKET-PARSING.md); [NamespaceBrackets](../../tests/Bosak.Xslt.Tests/EqNameBracketPatternTests.cs), [NamedElementPriority](../../tests/Bosak.Xslt.Tests/Xslt40EqNamePriorityTests.cs) | PR409 merged parsing correction and 4.0 priority checks; exhaustive earlier-profile URI-punctuation priorities remain unclaimed. |
| REQ123-constructors: Zero-argument focus constructors | [focus-functions](operators-and-syntax/focus-functions.md); [Evaluate_FocusConstructor_NoContextItem_ThrowsXpdy0002](../../tests/Bosak.XPath.Api.Tests/FocusConstructorTests.cs) | All 49 names have representative coverage; not all casting/focus-function grammar. |
| REQ123-destructuring: Typed let and sequence/array/map destructuring | [destructuring-let](operators-and-syntax/destructuring-let.md), [typed-let-coercion](type-system/typed-let-coercion.md); [Evaluate_TypedLet_FrozenXPath40_Coerces](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [Evaluate_DestructuringSequence_CoercionFailure_ThrowsXpty0004](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) | Per-shape cardinality, scope and conversion/error matrix remains partial. |
| REQ123-jnode: JNode navigation and deferred model remainder | [jnode-navigation](data-model/jnode-navigation.md), [jnode-template-boundary](data-model/jnode-template-boundary.md); [Evaluate_JNodeFunctions_FrozenXPath40_Work](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) | Navigation delivery does not establish complete GNode/JNode model/template rules. |
| REQ123-scan: Experimental scan visibility and set membership | [fn-scan](experimental/fn-scan.md); [Compile_ExperimentalFunctions_FrozenXPath40_ThrowXpst0017](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [Evaluate_Scan_XPath40Experimental_Works](../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) | fn-scan is gated with experimental retry; gate membership is not frozen API promotion. |

These are bounded evidence mappings, not certificates that every production or every valid/error form is covered. In particular, do not interpret the 508 focus-constructor checks as coverage of focus-function grammar. The existing `focus-functions.md` page describes zero-argument `xs:*` constructors; `fn { ... }` and the `fn` abbreviation are distinct parser surfaces with existing engine tests and a newly supplied [dedicated catalog entry](operators-and-syntax/focus-function-syntax.md).

PR #409 closes the previously reproduced opening-bracket URI parsing defect, with explicit matching tests in 3.0/4.0 and the remaining 4.0 priority correction. The old close-out queue item to review the union correction is obsolete: union-minimum and URI-priority corrections are published in 1.23.4, while bracket parsing is merged in unpublished source 1.23.5. This does not close all pattern or earlier-profile URI-priority behavior.

## D4 - decision lifecycle reconciliation

Classification comes from the entry's current Name and Classification section, not its directory name. In particular, two historical `deferred/` URLs now document bounded shipped declarations. No entry is deleted or moved in this reconciliation. This table checks the original dossier's moving/rejected groups and the subsequently supplied wider records; it does not certify every historical draft revision.

| Dossier shape / record | Current recorded disposition | Acceptance boundary |
|---|---|---|
| [Nominative records](type-system/nominative-records.md) | Bounded XSLT-defined identity implemented | Standalone XPath declaration/full historical proposal is not certified. |
| [Method-call operator](operators-and-syntax/method-call-operator.md) | Deferred | Do not confuse with pipeline or mapping-arrow syntax. |
| [fn:scan](experimental/fn-scan.md) | Experimental implementation | `fn-scan` gate membership and experimental retry do not promote it to frozen XPath40. |
| [Sequences of arrays/maps](operators-and-syntax/sequence-collection-iteration.md) | Delivered iteration supersedes bounded original deferral | Complete iteration/type/focus mapping remains separate. |
| [JNode built-in/model boundary](data-model/jnode-template-boundary.md) | Partial frozen delivery; deferred remainder | Navigation does not imply full model/template support. |
| [Never-adopted quantifier names](rejected/obsolete-quantifier-functions.md) | Rejected | Record accepted names and refusal/alternative evidence; no implementation of guessed names. |
| [Directional scan](rejected/directional-scan.md) | Rejected/superseded | Preserve experimental fn:scan's distinct contract. |
| [Old mapping arrow](rejected/mapping-arrow-gtgt.md) | Rejected/superseded spelling | Delivered `=!>` is not evidence that `=>>` shipped. |
| [XPath switch expression](rejected/xpath-switch-expression.md) | Rejected in XPath | XSLT xsl:switch is a separate instruction. |
| [Extensible map types](rejected/extensible-map-types.md) | Rejected proposal; alternatives shipped | Record cast/coercion behavior must stay distinct. |
| [xsl:item-type](deferred/xslt-item-type.md) | Bounded implemented declaration; historical URL retained | Do not label all present behavior deferred, or infer a complete declaration contract. |
| [xsl:record-type](deferred/xslt-record-type.md) | Bounded implemented named-record behavior; historical URL retained | Structural annotation, named identity, construction and noncoercing tests differ. |
| [Computed node constructors](deferred/computed-node-constructors.md) | XPath addition deferred | Existing XQuery construction is not adoption of this XPath proposal. |
| [Capturing accumulators](deferred/capturing-accumulators.md) | Deferred | Do not infer support from ordinary accumulators. |
| [Enclosing modes](deferred/enclosing-modes.md) | Deferred | Existing mode support does not enable nested declarations. |
| [fn:apply-templates](deferred/fn-apply-templates.md) | Deferred | Instruction/runtime template application is not this function. |
| [Serialization parameters](deferred/serialization-parameters.md) | 4.0 additions deferred; supported solidus control recorded | Do not describe all serialization as absent. |
| [External input proposals](deferred/external-input-functions.md) | Original out-of-scope proposals; reviewed-path absence | Do not generalize absence into an implementation commitment. |
| [XQuery/streaming language scope](deferred/language-streaming-scope.md) | Existing support; 4.0 deltas outside original schedule | Existing language/streaming implementation is not adoption of every 4.0 delta. |

The baseline ledger still has **28 missing historically adopted specification revisions**. A current immutable review pin cannot reconstruct adoption history. The existing type, compound-pattern and EQName delivery audits retain their own review pins and revisions; this reconciliation performs no new live-draft normative review. Their links are evidence of those scoped reviews, not historical adoption proof.

## AC-03 - evidence inventory and approved decision

The original criterion required every entry's Known Uses to cite qt4tests gate sets. That original wording is retained here as history; the owner approved the applicable-evidence amendment below on 22 October 2026. Of 73 Known Uses sections, **25 literally mention at least one name in the current 218-member GatedSets** and **48 do not**. The scanner checks names, not whether every cited set covers each contract. A passing set may cover only part of an entry; an entry without a literal set name may still have named unit, XSLT or refusal tests. The JSON makes these distinctions inspectable. Missing literal names therefore do not mean 48 untested features.

**Approved acceptance wording - owner decision, 22 October 2026:**

> Each entry's Known Uses identifies its applicable executable evidence. Implemented XPath contracts name the relevant current qt4tests gate sets where available, or explicitly record an unassigned set together with named engine tests. XSLT-only contracts name XSLT runner/unit tests and runnable public examples, explicitly stating that a XPath gate is not applicable. Deferred, rejected and out-of-scope decisions cite disposition evidence plus a versioned refusal test or a supported alternative where executable. Sample Code remains verified verbatim or pinned to a named test; worked examples retain input, context, expected output, compatibility and a meaningful edge case. A missing gate is never presented as conformance, and unsupported syntax is not implemented solely to create gate evidence.

The owner selected the applicable-evidence policy. The alternative of retaining a blanket qt4tests-only requirement was not selected. The REQ-126 criterion in FEATURE_REQUESTS.md is amended accordingly. This approval changes the evidence policy; it does not certify any entry, close AC-03, implement deferred syntax or promote a gate. All entries still require an evidence pass under the approved rule.

## Acceptance state and ordered next work

| Criterion | Reconciled state | What still prevents closure |
|---|---|---|
| AC-01 | Partial: 18 bounded grammar/cluster rows, existing signature inventories and exact delivery records | Dedicated missing grammar entries, complete per-contract mapping and historical pins. |
| AC-02 | Partial: current lifecycle table covers original moving/rejected groups plus wider records | Exhaustive source/history validation, including superseded and partially implemented contracts. |
| AC-03 | Partial: applicable-evidence policy owner-approved; reproducible citation inventory supplied | Validate applicable evidence for every entry; absence of a gate stays explicit. |
| AC-04 | Structure verified for 76 entries; semantic acceptance partial | Feature-specific drift and complete lifecycle/history review; generic headings do not suffice. |
| AC-05 | Met for current scope | Keep actual registry counts, canonical links and index date synchronized. |

1. **Supplied: focus-function syntax and pipeline focus.** [The focused delivery](FOCUS-FUNCTION-SYNTAX.md) adds a dedicated 16-field entry and 30 checks with a complete paired order-line report. The constructor-only page retains its stable URL and separate scope. Whole/per-item and caller/callback focus, empty input and 3.1 refusals have bounded examples; exhaustive callbacks/coercion are not claimed.
2. **Supplied: bounded record casts/coercion/updates.** [Record delivery](RECORD-CONTRACTS.md) supplies46 cases and a complete stock report plus44 API regressions for the signature gate it exposed. General nominal/static/default-field and broader host contracts remain unclaimed.
3. **Next: lexical and host boundaries.** The [numeric literal slice](NUMERIC-LITERALS.md) supplies the missing lexical entry; continue with bounded declaration/package/profile evidence rather than another blanket support claim.
4. **Continue D2 and D4 independently.** Defaults/options/metadata and historical pins remain open. Apply the approved AC-03 policy through a reviewable entry-by-entry evidence pass; policy approval alone does not close the criterion.

No number of future PRs is fixed by this queue; each slice should remain independently reviewable and validated. README installation/build instructions remain current; no structural/CLI/public API change requires an overview rewrite.

## Validation

At the initial reconciliation checkpoint, **10,516 solution tests passed**, including 2,743 catalog and 24 chapter cases. The [focus-syntax audit](FOCUS-FUNCTION-SYNTAX.json) records the focus checkpoint; [the record audit](RECORD-CONTRACTS.json) records the later current engine validation. Release build succeeds with zero errors and the pre-existing CS8604 conformance-runner warning. Final execution and hash receipts are recorded in [RECONCILIATION-VALIDATION.json](RECONCILIATION-VALIDATION.json). The script and matrix validate the current fields/index links, named test methods and referenced source files. Existing engine/example expectations and original receipts remain unchanged. The initial documentation/inventory slice did not rerun manual gates. The later record parser fix reruns both gates, recorded in its own audit; older PR409 receipts remain historical evidence. HTML rendering is checked with unchanged Fytala CSS, without claiming screenshot/PDF acceptance.


23 October2026: [Numeric literal delivery](NUMERIC-LITERALS.md) adds the74th entry and50 checks, correcting eight malformed fraction/exponent separator cases in source1.23.7. Current totals2,869 feature +24 chapter =2,893. Broader D2/D3/D4 and AC-03 remain partial;28 historical adoption pins remain missing. Public1.23.4 and gate membership unchanged.


## Ordinary stylesheet host/version follow-up - 23 October2026

[Host-profile delivery](HOST-PROFILE-BOUNDARIES.md) supplies the75th16-field entry and66 cases (50 focused,16 report runs). Current2,935 feature +24 chapter =2,959 documentation checks. Legacy uses effective-version-selected XPath; Draft supplies frozen4.0 independently. Root/local/ancestor declarations, AVTs, select/static paths and bounded forwards validation are covered. Package/import propagation and broader D2/D3/D4/AC03 remain partial;28 historical adoption pins unchanged. Public/source1.23.7 unchanged; no engine/version/release/gate change. Earlier counts/statuses remain historical.


## Direct module/profile follow-up - 23 October2026

[Module delivery](MODULE-PROFILE-BOUNDARIES.md) supplies the76th entry and114 cases, stacked on separate runtime-fix PR416. Current3,049 feature +24 chapter =3,073 checks. Host inheritance and dependency-owned effective versions are covered for one include/import/used-package edge. Transitive graphs, package lifecycle/override/default-mode/static/authoring/resource contexts remain separate. Source1.23.8 unpublished/public1.23.7; no further engine change or release in the module diff. D2/D3/D4/AC03 and28 missing historical pins remain partial. Earlier checkpoint counts/versions are historical.


## CSV options/defaults follow-up - 23 October2026

**CSV options/defaults review - 23 October 2026:** [Contract review](CSV-CONTRACT-REVIEW.md) supplies99 catalog checks and25 local-resource API tests, including complete supplier reports. Current76 entries,3,148 feature +24 chapter =3,172 checks. Source/public1.23.8 and Braid smoke remain unchanged; no engine/version/release/gate change. D2/D3/D4 and AC-03 stay partial. Earlier checkpoints below are historical.
