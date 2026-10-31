# C43 — Proposed optional schema session contract

**Proposed, 29 October 2026; no production API freeze or ADR acceptance.** Core base `ebfb7c2f`, unpublished candidate 1.33.1, published core 1.26.3. This counterproposal narrows ADR-006's callback/fallback assumptions using current source evidence. It preserves Bosak.Schema's ownership of XSD 1.1. Names below identify proposed responsibilities, not implemented interfaces or reserved public signatures.

## Decision proposed

Keep existing public XmlSchemaSet, stream-validation and QName annotation members intact. Add a future opt-in, version-neutral **schema session** spanning compilation, assessment, identity and component relations. A typing-only callback cannot represent multi-affiliation membership, complete derivation or identity-preserving atomization. Prefer an opaque identity channel over fabricated XmlSchemaType components. Exact public signatures, value-layout compatibility and performance require the isolated C44 prototype before owner acceptance of the permanent API.

Amend ADR-006 in three respects: an owned unsupported operation must stop explicitly rather than downgrade to 1.0; coverage includes component/identity relations beyond conditional typing; commercial integration runs are separate opt-in evidence rather than a dependency of normal core gates. Preserve the dated original ADR and record any later owner disposition separately. Its blanket description of all seams as XmlSchemaSet-typed is inaccurate for IXsdValidator and IXdmNode. Earlier SEAM_DESIGN hard-boundary prose is historical; H1–H4 already exist.

## Registration, authority and failure behavior

A host explicitly supplies an optional provider through compiler options or an equivalent factory. There is no discovery, global registration, implicit assembly load or core license check. The compiler obtains one immutable session for a resolved schema closure, captures it in the compiled executable, and propagates it to evaluation and node assessment. Runtime contexts cannot swap the authority of an already compiled expression or stylesheet. Existing compiled/evaluation contexts with no provider retain the existing XmlSchemaSet path and results, including C42's repaired membership relation.

Ownership is resolved at compilation for declarations/types and the import closure, **not by namespace alone**. A namespace may contain multiple revisions, local declarations and conflicting sources. Resolve ownership conflicts before executing a query. Cross-namespace imported bases, substitution affiliations and related declarations must form one linked component universe; reject unsupported mixed-provider relations explicitly rather than compare raw handles or fall back to a different schema revision.

| Proposed verdict | Meaning and required host action |
|---|---|
| NotApplicable | Provider has not claimed this request/closure. The established legacy path may be selected before ownership is fixed. |
| Supported(value) | The session completely answers this operation for its declared coverage; false membership is a supported boolean result. |
| Invalid(diagnostics) | Schema or instance violates supported rules; retain diagnostics and apply the host language's existing error mapping. |
| Unsupported(coverage) | The claimed closure requires a missing construct or relation. Stop assessment/compilation explicitly; do not report Valid, ordinary false or legacy acceptance. |
| Cancelled / provider failure | Preserve cancellation separately; surface a provider failure without retrying through another schema authority. |

After the session claims a declaration or closure, **NotApplicable is not a fallback escape**. Do not combine a successful assertion-only response with an unperformed structural check into Valid. Legacy-only branches of a mixed document can retain legacy behavior when their separate component ownership was resolved in advance. A registered provider may report an unsupported schema without changing unrelated unowned legacy documents.

## Proposed operations and dependency placement

Neutral identity/result contracts belong in the lowest shared core layer that can be consumed by the runtime and node providers, provisionally `Bosak.XPath.Core`. Registration stays on existing high-level compiler/host contexts; implementation belongs to the provider. Core product projects must not reference Bosak.Schema, its model, commercial diagnostics or license code. Do not introduce an API-to-runtime dependency cycle or return XmlSchemaElement/XmlSchemaType for provider-owned components.

| Proposed responsibility | Inputs / complete output | Core consumers |
|---|---|---|
| OpenSession | Explicit language/coverage, host-resolved schema closure, static context and resource policy → immutable authority/session | XPath compile options, XQuery schema import, XSLT compiler/import state |
| ResolveElement / ResolveAttribute / ResolveType | Expanded name **and declaration scope** → session-bound component identity or explicit verdict | Static kind-test visibility, patterns, constructors, type lookup |
| DescribeType / IsDerivedFrom | Session-bound identities → type category/base/member information and complete derivation result | Type tests, casts/constructors, conversions and function signatures |
| DescribeElement / GetActualMembers | Declaration identity → abstract/nillable/block facts, affiliation **list**, and complete actual member set | schema-element matching and member-set subtype containment; no head-chain shortcut |
| Assess | Declaration/type request, strict/lax/type mode and complete node content → immutable assessed view with annotation/typed-value information or explicit verdict | Source loading, validation-on-construction, XQuery validate, XSLT validation/type paths |
| DescribeAnnotation / atomization | Assessed node/value identity → governing declaration, type, nilled/ID/IDREF/list/union/typed-value information | IXdmNode properties, data(), comparisons and binding conversion |

Assessment includes structural validity and every required assertion/alternative rule in the claimed coverage. An assertion service alone is not full assessment. CTA must account for ordered alternatives, defaults, permitted test context, xsi:type derivation and applicable inherited assertions. Source inspection shows staged selection exists but complete derivation and inherited applicability are not yet qualified. No claimed operation may be implemented by reducing a 1.1 affiliation list to one head.

The standalone stream-based IXsdValidator remains a distinct document-validation facade; do not silently reinterpret its signatures as schema component injection. A future adapter for it needs separate explicit scope. Existing GetSchemaElement/GetSchemaType methods retain their concrete return types for legacy callers; new internal semantic dispatch consults the optional session without pretending provider-owned components are XmlSchemaSet entries.

## Identity, lifetime, copies and concurrency

Each opaque type/declaration identity is bound to an immutable session and component universe. Expanded QNames remain useful for display and existing public annotation views but cannot alone prove equivalence across providers or schema revisions. The session key must cover the complete resolved import/include closure, content identities, base/resource policy, language and relevant static context. SchemaCompiler currently computes its published digest from the root document; that digest alone must not become the new component-universe key.

Both **node annotations and atomized user-defined values** need provenance. IXdmNode currently exposes QName tuples, and XdmValue has lexical SchemaTypeName/UserSchemaTypeName fields. A node-only callback would lose authority through data(), variable binding or function conversion. C44 must prototype a neutral optional identity channel for both without freezing a new XdmValue layout prematurely. Foreign-session or stale identities must fail explicitly; equal display names are not interchangeable. No synthetic 1.0 complex-type facade is accepted as proof of 1.1 identity or derivation.

Assessment operates on provider-owned immutable views or private clones, preserving caller trees and existing schema metadata. Preserve copies retain annotation identity and a live reference to their session; strip copies remove it consistently from descendants and attributes. Construction mode and copy-namespaces policies remain independent and unchanged. A compiled executable owns the session lifetime; nodes/values that escape evaluation must keep their referenced universe alive. Disposing a host must not leave dangling handles. Concurrency requires immutable compiled components, per-evaluation mutable state and cancellation-aware assessment; caches must include the complete session/static-context key. No unproven single-null-check or zero-allocation promise is made.

## Integration, licensing and acceptance boundary

Bosak.Schema supplies its implementation and remains responsible for activation and its existing license gates. Core accepts any conforming host provider and never activates, validates or bypasses a commercial license. This review does not build or execute the commercial package, mint keys or qualify an activated provider. The inspected add-on references published core 1.26.3; a future provider must be qualified against the eventual released core seam, not merely this unpublished 1.33.1 source candidate.

Use a separate opt-in licensed integration runner, preferably add-on-owned, against published core packages and explicitly reviewed harness extension points. Keep default core builds/tests and normal QT3/qt4 runners independent of the commercial package. Schema-aware XSLT mode alone does not register a provider. Preserve all normal skip policies until original assertions pass under an explicitly identified integration profile and a separately reviewed admission change is approved.

[Acceptance matrix](ACCEPTANCE-MATRIX.md) separates current no-provider receipts from planned provider obligations. The six QT3 multi-affiliation originals, 30 documented XSLT skips, additional CTA witness and independent catalog applicability skip are different cohorts. No number of staged add-side tests or a registered callback alone closes them.

## Owner gate and recommendation

ADR-006 says **“pending owner acceptance in the Bosak core repo before the S4 design freeze”**; S4 preparation says S4c is blocked on that acceptance plus a core release. Those gates remain pending. C43 does not request premature permanent-interface acceptance: the atomic identity/layout channel, complete host error mapping, session lifetime and fake-provider proof still need C44 qualification. REQ-139 remains Pending with its original unchecked acceptance items and historical 31,145/0/676 draft reference preserved. Current compatibility targets are 31,397/0/424 and 8,941/0/1,918 (230 sets, 11 experimental passes), with C42's 24,203 passing solution tests and 39 regressions as the source-identical baseline.

Proceed with [C44 isolated contract prototype](NEXT-SLICE.md), then present the qualified amended contract for explicit core-owner acceptance. No public hook, production backend, filter/baseline, package dependency, sibling file or release is changed here.
