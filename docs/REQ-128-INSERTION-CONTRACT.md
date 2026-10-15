<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-128 source-preserving insertion design">
  <br><br><h1>REQ-128: value-of insertion contract</h1>
  <p>Design for review, 15 October 2026</p>
</div>

## Status and decision boundary

This resolves the design questions identified in the [consumer dossier](REQ-128-source-preserving-value-of-insertion.md). **No insertion API, capability, runtime port or UI is implemented.** REQ-128 remains Pending, target TBD, owner Unassigned; acceptance and implementation scheduling remain separate. API names below are proposed names, not available members. An additive API feature requires a minor release after acceptance; no version is reserved here.

Audit base: published engine 1.7.1 at `35e531c101bcbba13a2533f33a7decca5f18eddd`; source inspected at `d310b864aa5248d6c6c8fbd78aef51dd422a4dff`, whose source/test trees match that release. Existing `AuthoringSnapshot.ProposeExpressionEdit` replaces an existing attribute. `AuthoringEditCandidate` and its changed-slot marker cannot truthfully represent a newly inserted instruction. Integer descriptor IDs and old expression proposals remain unchanged by this design.

## First-slice eligibility

The engine decides eligibility and returns an opaque target; Braid never enables insertion by matching labels, QNames or child counts. The capability is global, but eligibility belongs to the particular snapshot and parent. The first slice deliberately supports the following subset of the consumer request.

| Case | Decision |
|---|---|
| Principal root | Explicit `xsl:stylesheet` or `xsl:transform`, effective supported XSLT 3.0 or 4.0 profile |
| Modules | Exactly one fully inspected principal module, with no include/import/use-package or package root; no dependency loading or edits |
| Parent | A principal `xsl:template`, or an ordinary literal-result element beneath it through only literal-result-element ancestors |
| Other instruction ancestors | Refuse initially, including LREs nested under `xsl:if`, `xsl:for-each`, `xsl:try`, functions or other constructors; this avoids claiming unproved prospective context capture |
| Parent syntax | Explicit closing tag required; refuse a self-closing parent rather than expanding its spelling |
| Source support | Strict XML 1.0 and the insertion encodings below; unsupported versions, DTD/schema/package contexts, excluded or opaque target subtrees refuse |
| Parent content | Existing elements/comments/PIs and XML-whitespace-only raw text allowed; direct non-whitespace text or CDATA refuses as mixed content; effective `xml:space="preserve"` refuses |
| Existing instructions | Preserve existing children, including leading `xsl:param` declarations and preceding local variables; complete prospective static context and grammar must be provable |
| Validation | Require original and candidate to pass complete static validation within the declared first-slice coverage; invalid originals, unsupported coverage or unprovable contexts refuse |

Children of a template can contain arbitrary constructs only when supported static validation covers them; eligibility is not a promise that all such constructs are supported. Inspect-only/unsupported regions elsewhere remain byte-preserved but cannot be silently treated as validated. Extending these boundaries needs separate fixtures and an explicit capability/coverage update.

## Snapshot ownership and proposed API

Use an additive structural contract instead of extending `AuthoringEditProposal` or changing expression-edit behavior.

| Proposed member/type | Contract |
|---|---|
| `AuthoringCapabilities.SupportsValueOfInsertion` | Advertise true only after the entire declared engine slice is implemented and verified; no placeholder true flag |
| Insertion encoding/size limits | Advertise independently of inspection's wider encoding list; the first-slice limits below are normative for this design |
| `snapshot.GetValueOfInsertionTarget(parentDescriptor, cancellationToken)` | Return an eligible opaque target or a classified refusal for that exact descriptor/snapshot; discovery performs no new external IO |
| `AuthoringInsertionTarget` | Immutable, engine-created, non-public constructor; internally retains its owning snapshot, module identity, parent identity and sole position `End` |
| `AuthoringValueOfInsertionProposal(target, selectExpressionText)` | Immutable target plus expanded XPath text; accepts no XML fragment, offset, QName, prefix, source range or caller-selected sibling position |
| `snapshot.ProposeValueOfInsertion(proposal, cancellationToken)` | Return exactly one isolated structural candidate or one refusal, never a partially usable candidate |
| `AuthoringStructuralEditCandidate` | Immutable input-snapshot identity, principal URI, candidate snapshot/source, affected ranges, complete surviving-node correspondence and explicit inserted instruction/select-slot identity |

Null required arguments throw `ArgumentNullException`, consistently with the existing surface. Empty/incomplete expressions are classified proposal failures. Targets cannot be serialized, persisted across reopen or reconstructed from an integer ID. Out-of-process target transport is outside this slice.

Target discovery checks descriptor membership by engine-owned identity, not integer equality alone. A descriptor from another inspection with an equal integer ID is foreign. Proposal submission checks the target's owning snapshot before any node lookup. Repeating a proposal against its original immutable snapshot can still produce valid attached data after a newer candidate exists; the engine does not know Braid's accepted revision. Braid independently rejects stale sessions/revisions during review and Apply.

## Exact insertion and byte fidelity

`End` means **immediately before the opening `<` of the selected parent's existing closing tag, after every original child and trailing trivia byte**. Do not indent, add a newline, move comments/PIs, relocate existing whitespace, normalize a quote or reserialize any existing node. An indented source can therefore place the inserted instruction on the closing-tag line. Candidate review shows that literal source change.

For original bytes `S`, byte boundary `p` and encoded insertion `I`, require `candidate = S[0:p] + I + S[p:]`. Report one old half-open range with zero byte length at `p` and one new range covering exactly `I`. Use existing `SourceRange` byte offsets/lengths and independently correct 1-based UTF-16 line/column coordinates. A valid `p` is discovered from the engine's retained lexical source evidence, not a search for the first matching closing-tag string.

The generated instruction is self-closing and contains one unqualified `select` attribute, plus a local namespace declaration only when necessary. Existing bytes, BOM, XML declaration, line endings, entity spellings, comments, PIs and opaque surrounding text remain exact. The parent/ancestor ranges necessarily grow in the candidate; those are identity-correspondence effects, not additional edited byte ranges.

### Namespaces and expression text

Choose an existing nonempty in-scope prefix bound to the XSLT namespace, preferring `xsl`, then ordinal prefix order. If none exists, declare `xsl`, `xsl1`, `xsl2`, etc. on the **new instruction only**, choosing the first unused in-scope prefix. A rebound prefix is never overwritten; the default namespace never supplies an unprefixed instruction name. Preserve all existing bindings.

Parse and validate the expanded XPath in the prospective inserted `select` slot's actual context: effective grammar/version, applicable static namespace rules, default XPath namespace, base URI, preceding local variables/parameters, globals and available function signatures. Validate the ordinary emitted stylesheet as well; parsing an isolated XPath alone does not establish variable or function availability. Refuse any static context that cannot be captured completely within this slice, including unsupported fixed-namespace/type contexts.

Use a double-quoted attribute; XML-escape `&`, `<` and `"`, and preserve tab/LF/CR through numeric character references so XML attribute normalization does not alter the supplied expanded expression. Refuse invalid XML scalars or malformed surrogate sequences rather than replacing them. Re-inspection must recover the exact input expression as the new attribute's expanded value.

`select` is an Expression slot, not an AVT: **do not double XPath braces**. For example, map constructors and braces inside XPath strings retain their original expression spelling after XML decoding. Inherited `expand-text` is preserved; the new instruction has no text children and no additional formatting text is inserted. Do not add an `expand-text` override as an escaping workaround.

### Encodings and bounds

Insertion initially supports UTF-8 with or without BOM and BOM UTF-16 LE/BE, with a matching or absent encoding declaration. Preserve the original encoding/BOM/declaration exactly and use strict encoding; encode the inserted bytes without an additional BOM. UTF-16 without BOM, XML 1.1, declaration/BOM mismatch and other encodings refuse even if a wider inspection capability can read them. Additional encodings require a separate advertised support change and fidelity fixtures.

The insertion source and resulting principal module must each be at most **8,388,608 bytes**, including BOM. This is an insertion-specific engine limit aligned with Braid's current eight-MiB bound; it does not redefine inspection limits. Check the resulting byte count before allocating/emitting a candidate. A source within the bound whose insertion exceeds it refuses. Count bytes rather than decoded characters, including for non-BMP text and UTF-16.

## Candidate inspection and identity

Return a fresh snapshot from the emitted bytes, with the same applicable inspection options and only the controlled, single-module source. Return an explicit candidate node ID for the new `xsl:value-of`, and the actual `select` Expression-slot descriptor/range owned by that node. Consumers do not guess IDs from labels, offset proximity, child order or the largest ID.

Require a total, injective old-node-to-surviving-candidate-node map for the original principal descriptor graph, including document/parent/ancestor nodes. Compute it from retained source provenance and the exact insertion, with engine checks of node kinds and names. The existing equal-shape expression-edit pairing is not sufficient when a child is added. Old IDs are never used in the candidate identity space without this map. Duplicate/ambiguous/missing engine correspondence or an unidentifiable inserted slot refuses the proposal as an engine alignment failure; it does not return a successful candidate.

Candidate source export returns defensive copies; descriptor/mapping collections must not expose mutable backing storage. Keep original and candidate snapshots immutable and safely shareable. A successful proposal means **candidate produced**, not accepted, committed, validated by Braid or authorized for export.

## Cancellation, resources and failures

Cancellation is cooperative. Check before discovery/proposal work, before contextual parsing, before emission, before and after validation/re-inspection, and immediately before returning a candidate. Cancellation discards all work and returns `Cancelled` with no candidate. There is no hard real-time abort guarantee; Braid rechecks cancellation after return and before acceptance.

Neither discovery nor proposal generation transforms input, writes files or performs network access. Any compilation/static evaluation must use the supported deny-by-default controlled resource route with no fallback IO or extension-function execution, including `use-when`/static globals. If an existing compiler/validation path cannot enforce this boundary, that context refuses and the slice cannot ship broader support. Original inspection may already carry evidence, but a proposal cannot reacquire dependencies or mistake a cache hit for permission.

Proposed refusal kinds are distinct from XPath error codes. Include a stable reason, safe diagnostic text and an optional existing-source range; diagnostics for the not-yet-existing expression use proposed-expression coordinates rather than pretending to be an old source location.

| Refusal | Examples |
|---|---|
| `CapabilityUnavailable` | Engine build does not implement this operation |
| `ForeignSnapshot` / `UnknownTarget` | Descriptor/target belongs to another snapshot or cannot be resolved |
| `UnsupportedModule` / `UnsupportedProfile` / `UnsupportedCoverage` | Dependency edits, package/composite stylesheet, unsupported version/static context/validation coverage |
| `UnsupportedEncoding` / `SizeLimit` | BOM/declaration problems, excluded encoding, source/candidate exceeds byte bound |
| `IneligibleParent` / `SelfClosingParent` / `MixedContent` | Illegal parent/ancestor, missing closing tag, significant raw text/CDATA or preserved whitespace |
| `InvalidOriginal` / `InvalidExpression` / `InvalidCandidate` | Baseline or prospective static validation fails; preserve underlying XPath/XSLT codes as diagnostic data |
| `ResourceDenied` | Static processing would require disallowed IO or an extension function |
| `SourceAlignmentFailure` | Insertion boundary, exact splice, node mapping or inserted-slot evidence cannot be proved |
| `Cancelled` | A checked cancellation point observes cancellation |

After null argument checks, cancellation has priority; ownership is checked before resolving node/context; module/profile/encoding/size/eligibility precede expression processing. Candidate validation and final cancellation checks happen before success. Internal programming faults remain exceptions and are never disguised as a usable candidate. Braid entitlement/session failures are consumer outcomes, not engine error codes.

## Braid review and Apply

Review is read-only with respect to accepted source, history and validation/preview/export evidence. Retain candidate/input identities, source-session revision and the permission context. Apply independently rechecks AuthorTransformation entitlement, cancellation, the accepted session/revision and candidate/input ownership, then uses a serialized transition or equivalent compare-and-swap to accept the candidate as exactly one draft-history entry. A race/denial/cancellation leaves accepted source and history unchanged.

After acceptance, refresh source/inspection/graph from the candidate and select the explicit new instruction/select slot. If consumer-side selection evidence is missing or ambiguous, clear selection rather than using a heuristic. Undo/redo restores exact accepted bytes and invalidates old inspection/validation/preview/export evidence. Validation, controlled preview, restricted export and reopen remain explicit separate operations; proposal production performs none of them implicitly. Original files and production package pins remain unchanged by this design.

## Required fixture inventory

These are planned acceptance fixtures, **not executed tests or passing evidence**. Every engine/API addition also needs the repository's full solution/LS checks, frozen QT3 total 31,142/0/679 and qt4 gated verification, exact package/API provenance and release evidence before Braid adoption. Consumer native keyboard/focus/theme acceptance is recorded separately from automated tests.

| Fixtures | Owner | Expected evidence | Acceptance |
|---|---|---|---|
| T01 template; T02 direct LRE; T03 nested LRE chain | Bosak | One contextual select; exactly one added instruction; explicit identity | SA-01/02/06 |
| T04 illegal parent; T05 instruction ancestor; T06 self-close; T07 dependency/package/profile; T08 unsupported coverage | Bosak | Classified refusal, no output candidate or source mutation | SA-01/04 |
| T09 equal integer IDs from different snapshots; T10 foreign target; T11 repeated proposal on original snapshot | Bosak + Braid | Ownership rejection; immutable original remains usable; consumer stale revision rejected | SA-01/04 |
| T12 compact; T13 LF/CRLF/CR indentation; T14 trailing comment/PI/whitespace; T15 opaque preceding source | Bosak | Exact unchanged byte prefix/suffix and deterministic End boundary | SA-03/08 |
| T16 significant raw text/CDATA; T17 inherited xml:space preserve | Bosak | Explicit mixed-content refusal without moving text/trivia | SA-01/03 |
| T18 UTF-8 optional BOM; T19 UTF-16 LE BOM; T20 UTF-16 BE BOM; T21 rejected encodings/mismatch | Bosak + Braid | Byte/BOM/declaration fidelity; correct UTF-16 and byte coordinates | SA-03/08 |
| T22 eight-MiB source/candidate boundary and one byte over; T23 non-BMP before insertion | Bosak | Byte-aware bounds and correct coordinate shifts | SA-01/03 |
| T24 existing alias/default/rebound namespaces; T25 new local prefix collision sequence | Bosak | No changed bindings; emitted name is XSLT; actual prospective context | SA-02/08 |
| T26 ampersand/less-than/quotes/tab/LF/CR; T27 map/string braces with expand-text true/false | Bosak | Decoded select equals input; no AVT brace doubling or added text | SA-02/03 |
| T28 preceding local variables/params/globals/base URI/default XPath namespace; T29 undeclared names/incomplete XPath | Bosak | Correct scope; invalid expressions refuse with contextual diagnostics | SA-02/04/08 |
| T30 pre-cancel; T31 cancellation during validation and before return; T32 denied static IO/extension function | Bosak | No candidate, IO or mutation after refusal/cancellation | SA-04 |
| T33 source race/entitlement loss/review without Apply; T34 serialized Apply + undo/redo | Braid | Zero changes on refusal; one accepted history entry; exact byte restoration | SA-04/05 |
| T35 inserted-node/slot/range mapping; T36 unrelated graph selection | Bosak + Braid | Complete engine mapping; explicit new selection or cleared consumer selection | SA-03/06 |
| T37 controlled validate/preview/export/reopen; T38 native focus/keyboard/theme | Braid | Real accepted ordinary XSLT, exact-output golden, separate native receipt | SA-07/08 |

Golden G1: an eligible explicit `<out></out>` under a template, with XML output, UTF-8, `omit-xml-declaration="yes"` and `indent="no"`, receives select expression `'A'`; accepted transform output must be exactly the UTF-8 bytes `<out>A</out>`. Golden G2: append `select="$v"` to a template after a local `$v := 'A'`, with text output; expected output is exactly `A`. Run the source-encoding variants independently of output encoding. Assert baseline and candidate source bytes, re-inspection, candidate diagnostics and output separately.

## Delivery sequence and remaining acceptance

The design decisions above are resolved for review. Next obtain maintainer acceptance of this bounded scope, API naming and ownership/scheduling; do not mark REQ-128 Accepted or reserve a release merely because this document exists. Implement the engine in a separate slice with real fixtures and explicit capability/coverage declarations, then deliver and verify a minor library release. Only after supported public delivery may Braid adapt its ports, acceptance/history and UI, with independent real-engine fidelity tests and native acceptance. No insertion button or fake capability ships in this design step.
