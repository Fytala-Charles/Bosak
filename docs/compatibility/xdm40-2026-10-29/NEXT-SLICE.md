# Revised candidate and next compatibility slice

29 October 2026. Supersedes the next-action wording in the restart checkpoint; preserves C44's sealed proposal/evidence. C45 production implementation, permanent API acceptance, ADR-006 and REQ-139 remain pending.

## Candidate to qualify

Continue with an additive optional schema-session route, preserving `IXdmNode`, `IXdmSequence`, existing `XmlSchemaSet`/validator/resolver members and existing `XdmValue` fields. Leave generalized XML/JNode operations and record/item types in core. Full XSD 1.1 assessment remains an add-on responsibility.

Revise the isolated C44 contract before requesting acceptance:

| Concern | Concrete proposed revision |
|---|---|
| Explicit semantic inputs | Replace the ambiguous `Language` meaning with separately specified host language/version and XDM profile/review-or-adoption identity; carry XSD version independently. Define canonical encoding and defaults before settling CLR signatures. |
| Component authority | Keep fresh session/issuer tokens for provenance. Add a documented complete component-universe fingerprint, including transitive resources and interpretation context. Fingerprint equality alone must not forge issuer authority or establish compatibility. |
| Compatible-schema interchange | Specify a provider-attested compatibility bridge or canonical shared authority over compatible universes. Retain the source session/type/declaration annotation; map relations into the captured target universe only after proof. Test a compatible superset whose assessment differs. Do not silently revalidate/strip or reject every different token while claiming general compatibility. |
| Separate identities | A source/snapshot identity may bind an assessment to its input, but must never become schema type identity. Record labels/item types remain distinct from XSD component identities. |
| Ownership/coverage | Preserve explicit host-opened session capture at compilation, immutable closed resources, complete supported outcomes, refusal of unsupported owned coverage, provider exception causes and separate cancellation. No mutable ambient/runtime authority selector. |
| Carrier/API compatibility | Retain the existing-slot envelope as the candidate. Recheck every real payload/accessor path and actual consumers; keep old built-in/user type-name accessors coherent. Do not accept a 48-byte size observation as sufficient ABI proof. |
| Execution environment | Document whether sessions are safely shareable and how compiled expressions/values retain them. XML snapshots and caller trees remain under their current owner/lifetime/thread rules. |

No precise new public method name is frozen here. The compatibility bridge must be demonstrated first; the shape could reside in provider/host admission rather than adding a new method to every node or value.

## Next bounded workpackage: revise the isolated compatibility prototype

1. Record a **prototype-only** immutable semantic scope against the [review pin](evidence/spec-pin.json), especially XDM §§4.1.4–5 and PSVI/typed-value rules. Preserve all C44 receipts and create new artifacts for the revised experiment.
2. Demonstrate four distinct identities: query/XDM profile, schema version, component authority/universe and source/snapshot binding. Equal QNames and root-only digests must not pass compatibility checks.
3. Add compatible distinct-universe handoff cases: same closure/fresh sessions; compatible superset/subset; original PSVI retained even when target revalidation would differ; incompatible same-name definitions; anonymous derivation; multi-head membership and stripping/copy/atomic flow. Include negative variants that deliberately confuse tokens, fingerprints, versions and storage IDs.
4. Produce revised compile-checked neutral declarations, explicit coverage/lifetime/error rules and a public-surface diff. Keep the experiment outside product assemblies. Reconcile the complete-provider requirements with Schema's actual model; an adapter stub is not provider qualification.
5. Present that concrete revised API and phased migration proposal for the existing owner acceptance gate. Only then start the guarded C45 product candidate.

This is the recommended next action, rather than requesting approval for the unmodified C44 surface. The current user request authorized this review, not production acceptance.

## Following delivery and acceptance sequence

| Stage | Deliverable and completion gate |
|---|---|
| Revised isolated prototype | Compiled contract and compatibility/mutation controls above; no product/package/consumer mutation. |
| Owner disposition | Accept exact revised API, capability limits, migration/version plan and guarded production scope. Preserve the decision separately from earlier pending records. |
| Guarded core candidate | Adoption evidence before semantic changes; opt-in route; no-provider regression/performance checks; exact public API diff and payload audit. New functionality gets happy-path/edge regressions and required header/version/docs updates. |
| Real consumer qualification | In isolated copies, build actual XmlDb/Schema/Braid source against candidate; run their appropriate complete suites. Run unchanged previously compiled binaries against replacement assemblies, including real annotated/lazy values. Record source/package/binary identities and failures separately. Execute licensing only through separately authorized existing owner arrangements. |
| Engine gates | Fresh complete solution (including XSLT workaround if applicable), exact QT3 31,397/0/424 and qt4 8,941/0/1,918 with all skip names/reasons and 230 set names unchanged, unless a separate baseline decision explicitly changes them. |
| Consumer adoption/release | Consumer-specific dependency changes, native/host acceptance where relevant and publication are separately reviewed actions. No automatic storage migration or schema-aware Braid profile follows from the core release. |

The [current-draft behavior gaps](SPEC-REVIEW.md) form a separate repair queue: generalized node identity/order, simple/labeled records, map order/key equality and comparison-operator consistency. Triage them against the currently adopted profile before choosing a repair; no gate admission or frozen expectation is changed by this review. Do not combine that queue with the schema-session slice merely because the affected values share the XDM carrier.
