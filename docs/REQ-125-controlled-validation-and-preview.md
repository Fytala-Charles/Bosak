# Controlled Validation and Preview Requirements

> Requester: Bosak.Braid · Submitted: 2026-10-10 · Upstream: Bosak REQ-125, Pending

## 1. Problem and Evidence

Braid needs to validate the accepted stylesheet and preview it against explicit input and expected-output fixtures. A compile-only check cannot authorize validated export. At pinned Bosak revision `b5eb1b17249fe458095af2dc53445d459f30db11`, the Braid integration test `CheckCompilationAsync_MalformedXPath_RecordsPinnedEngineLimitation` demonstrates that `xsl:value-of select='('` compiles without diagnostics. Bosak must define which static checks have completed and which remain deferred.

`XsltExecutable.TransformCaptured` is internal. Existing public transforms do not expose the same captured secondary-result contract. `EvaluationContext.DocumentLoader`, `StreamingDocumentLoader`, `ResourceUriMapper` and `CollectionLoader` provide useful hooks, but their documented defaults include filesystem fallbacks; a URI mapper is not a comprehensive acquisition policy. This review does not establish that every existing path bypasses supplied hooks. It establishes that Braid cannot yet rely on a documented, tested policy covering all relevant paths.

REQ-124 remains Implemented and accepted for source-preserving authoring. These additional validation/runtime needs belong to a separate upstream request.

## 2. Ownership and Consumer Workflow

Bosak owns static analysis, diagnostic codes and source mapping, execution, resource-hook propagation, output capture, cancellation checkpoints, and declarations of supported behavior. Braid owns licensing, accepted draft revision, approved resource envelopes, fixture storage, orchestration, comparison, provenance, UI, and any isolated worker process.

Braid captures a source snapshot and effective configuration, checks authoring entitlement, and requests complete static validation using approved immutable module bytes. A preview separately requires `RunTransformation`, explicit input, typed parameters and bounded output capture. Before publication Braid checks cancellation, entitlement, source identity and dependency/configuration evidence. Stale or rejected results preserve the draft and earlier evidence. A passing comparison is useful only for a successful, current run with complete declared output coverage.

The host must opt into controlled behavior. Existing Bosak callers keep current defaults unless maintainers decide otherwise through the normal compatibility process. No Braid commercial-license checks belong in Bosak.

## 3. Required Engine Guarantees

### 3.1 Static Validation

Provide a public validation contract that reports success, invalid source, unsupported validation coverage, policy refusal and cancellation distinctly. Success means all applicable static checks for the declared supported language profile completed, including expressions in templates that are not executed. Unsupported or deferred analysis must be explicit and must not be presented as a complete pass.

Diagnostics should include stable engine/spec codes, module identity and source-backed locations where available; missing locations must be explicit. Validation must leave principal and dependency envelopes unchanged and perform no transformation or result-document writes. The engine owns parsing expressions, patterns and AVTs in their actual static context. Braid will not scan these independently or invoke edit proposals as a substitute for stylesheet validation.

### 3.2 Resource Policy

Define one controlled policy covering include/import, package acquisition, schema acquisition where supported, static evaluation, document/document-available, unparsed-text and variants, json-doc, collections, source-document including streaming, dynamic transform/evaluate and extension mechanisms capable of IO. Document unsupported routes and refuse them under the controlled profile.

All acquisition must pass through an explicit host authority with resolved identity and resource purpose. Denial, missing resources and callback errors must not cause alternate disk/network resolution. Nested transforms, evaluation contexts and streaming paths must inherit the policy. Availability functions may return the spec-appropriate false result for a denied resource, but must still perform no fallback IO. In-memory resources may be approved without a disk counterpart; approved bytes must win over conflicting disk content. URI remapping alone is insufficient.

Requested and effective URIs, purpose and successful acquisition evidence must be observable so Braid can attach retained bytes and hashes. Diagnostics must not expose credentials embedded in URIs. XML DTD/entity policy and limits must apply to all XML acquisition paths, rather than only the principal stylesheet.

### 3.3 Output Capture

Provide a supported public operation that captures principal and secondary outputs without filesystem or network writes. Preserve resolved output identities, effective serialization settings and output bytes; never label UTF-8 encoding of a serialized string as the engine's declared output encoding. For a text-only first slice, document the conversion and restrict supported output profiles explicitly.

Enforce output count, per-output and aggregate size bounds during production, not solely after constructing the entire result. Detect URI collisions and define partial-output behavior on execution failure, policy refusal, cancellation and limit exhaustion. Partial outputs may be retained as diagnostics but cannot become a successful run. Messages need a separate bounded channel; the host cannot assume they are part of result output.

### 3.4 Execution Control

Document cancellation support and enforceable limits for validation, execution, nested calls and serialization. Provide cooperative cancellation or clearly state where it is unavailable. Do not describe checking a token only before/after a synchronous transform as mid-run cancellation.

Braid can provide process termination for hard wall-clock limits; it cannot infer an OS sandbox from process isolation. Bosak must identify remaining uninterruptible operations and resource-control limitations. A delivery may explicitly defer advanced profiles, but must refuse them under a restricted profile rather than silently running without controls.

## 4. Acceptance Criteria and Delivery Slices

| Gate | Consumer acceptance evidence | Proposed slice |
|------|------------------------------|----------------|
| AC-01 | Malformed XPath in an unused template reports a static failure; ordinary valid fixtures pass; unsupported coverage is explicit | A: static validation |
| AC-02 | Principal/dependency bytes and authoring snapshots remain unchanged; diagnostics identify the failing module and available source range | A |
| AC-03 | Explicit include/import bytes compile without files and override conflicting disk content; denied valid disk alternatives remain unread | A/B: policy |
| AC-04 | Table-driven tests exercise every declared acquisition route, including nested calls, static evaluation and availability functions; denial has no disk/network fallback | B |
| AC-05 | Approved resource receipts describe exact effective inputs; inaccessible routes are explicitly unsupported and refused | B |
| AC-06 | Public preview captures principal plus multiple secondary outputs, correct resolved URIs and serialization bytes; no output files are created | C: capture |
| AC-07 | Duplicate output URIs, message floods, per-output/count/aggregate overflow and partial failures produce classified non-success outcomes | C |
| AC-08 | Cancellation and declared execution limits stop representative loops/recursion and serialization as documented; limitations are demonstrated and documented | D: limits |
| AC-09 | Existing default APIs remain compatible; controlled opt-in survives nested contexts; parallel requests cannot share mutable policy or result state | A–D |
| AC-10 | Engine-owned public-API consumer sample, capability/profile documentation and tests support a pinned Braid adoption without reflection or internal access | A–D |

Bosak maintainers may refine the slices and API shape. Braid may adopt A independently, while preview remains disabled until the applicable policy, capture and limit gates pass. Existing regression suites and a targeted conformance smoke must remain green; exact commands and results belong in the Bosak delivery evidence.

## 5. Braid Adoption and Remaining Work

The current preliminary API remains [compilation checking](../../Bosak.Braid/docs/COMPILATION_CHECKING.md). It is not upgraded to a validated state by this specification. No production preview command or engine patch is introduced.

After upstream delivery, Braid will pin an accepted revision, run independent consumer tests, map engine outcomes without erasing unsupported/refused states, build the licensed validation/run coordinators, capture complete resource/output provenance and then add desktop fixture/preview UI. Browser execution remains a separate deployment decision. Generated-XSLT optimization stays a later phase with semantic-equivalence evidence, rather than being part of this request.

## 6. Bosak Delivery Record

- **2026-10-10 — Slice A consumer-review corrections (F1–F4) delivered** in response to `docs/REQ-125-SLICE-A-CONSUMER-REVIEW.md` (PR #137). No public signatures changed. **F1:** IO-free structural pass (unknown XSLT-namespace instructions XTSE0010-class, top-level placement, must-be-empty XTSE0260, static var/param placement XTSE0090, xsl:if/@test, xsl:use-package/@name, misplaced xsl:on-completion, XSLT-namespaced attribute rules XTSE0090/XTSE0805, xsl:note discard; forwards-compatible + top-level vendor-extension tolerance preserved); checked set pinned as new declared coverage item `StructuralChecks`; exclusions documented on `XsltValidation` remarks degrade through UnsupportedCoverage — never a silent Valid. **F2:** `PatternCompiler.Compile` optional namespace-bindings parameter (3.15) — undeclared prefixes in match/grouping/count/from patterns and pattern predicates reject XPST0081 in module-local static context; runtime callers bit-identical. **F3:** use-when exclusion evaluated before slot checks; supported subset exactly the literals true/true()/false/false(); malformed exclusions still fail; excluded elements skip own attributes + descendants; non-literal exclusions record a use-when coverage gap → UnsupportedCoverage; literal-excluded globals skipped (references XPST0008). **F4:** template/iterate parameter defaults get declaration-order scope (later/self fail, earlier pass, body visibility retained); xsl:function unchanged. **Regression evidence (review §4):** 38 tests in `XsltValidationReviewFindingTests` + pinned coverage test; validation target 207/0. Gates: unit 3,849/0 (3,811 + 38), QT3 31,142/0/679, XSLT smoke 162/0/26, qt4 gate 0/223. Consumer note: Braid re-review for Slice A acceptance is unblocked; Slices C and D remain.

- **2026-10-10 — Slice B (controlled resource policy) delivered.** New opt-in `Bosak.XPath.Runtime.Resources` namespace: `ControlledResourcePolicy` holding a single host `IControlledResourceAuthority`; `ControlledResourceRequest`/`ControlledResourceResponse` (Approve with authoritative bytes / ApproveCollection / Deny; null = abstain = refuse); 14-route `ControlledResourceRoute` covering document, json-doc, unparsed-text, unparsed-text-lines, collection, the three availability probes, schema-import, source-document, streaming-input, extension-function and transform-inherit. Refusals XV0004 (deny/abstain/authority-error) and XV0005 (declared-unsupported route; extension functions permanently unsupported). Approved bytes are authoritative and never merged with disk alternatives; availability probes do zero IO; every acquisition records a `ControlledResourceReceipt` (credential-redacted URI, route, purpose, outcome, byte count, lowercase-hex SHA-256) in the policy's thread-safe log. Wired through `EvaluationContext.ResourcePolicy` + `LoadDocument(uri, route)`, `FunctionLibrary` (fn:doc/json-doc/unparsed-text/-lines/collection + -available probes), `XsltCompiler.ResourcePolicy` → `TransformEngine` (source-document + streaming input), `ControlledSchemaResolver` (xsl:import-schema), and `fn:transform` inheritance. Default (null policy) bit-identical — all branches key off null. **AC evidence (AC-03/04/05):** 53 tests in `ControlledResourcePolicyTests` + `ControlledResourceRouteTests` — approval wins over disk with no fallback IO, zero-IO availability probes, receipt contents and redaction, XV0004/XV0005 taxonomy, nested-transform inheritance, run-to-run isolation. Gates: unit 3,811/0 (3,758 + 53), QT3 31,142/0/679, XSLT smoke 162/0/26, qt4 gate 0/223. Consumer note: Braid may adopt Slices A+B together; output capture (C) and limits/cancellation (D) remain before full preview per §4. Ships on the 1.1.0 track.

- **2026-10-10 — Slice A (static validation) delivered.** Additive `Bosak.Xslt.Validation` namespace, `XsltValidation.Validate(...)` over REQ-124 authoring envelopes; outcome taxonomy Valid / Invalid / InvalidSource / UnsupportedCoverage / Refused (+Cancelled reserved for Slice D); declared coverage = all Expression/Pattern/AVT slots in all resolved modules incl. never-executed templates, with real static context (variable scoping, prefixes, xpath-default-namespace, xml:base, effective version) and use-when=false elision; explicit gaps (xsl:import-schema, packages) reported as UnsupportedCoverage — never a silent pass; diagnostics carry stable codes, module URI, message and source range with explicit `LocationAvailable`; byte-preserving (AC-02 verified by byte-equality tests). Gates: unit 3,758/0 (29 new AC-mapped tests), QT3 31,142/0/679, XSLT smoke 162/0/26, qt4 gate 0/223. Consumer note: Braid may adopt Slice A independently; preview remains disabled until slices B/C/D pass, per §4. Ships on the 1.1.0 track.

