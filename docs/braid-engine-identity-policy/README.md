# Braid engine identity and adoption policy scope — 29 October 2026

Recommendation: **reviewed rebuild with an explicit engine pin, plus runtime mismatch refusal**. Do not automatically adopt replacement engine DLLs merely because existing binaries execute. This is a proposed consumer-owned policy and bounded prototype scope; no product implementation, dependency update, protocol migration or new core API is included.

## Verified problem and affected paths

The [reviewed binary qualification](../binary-compatibility-review/README.md) preserves the exact reproduction: unchanged Braid inspector/preview identities remain `bosak:6a3372fc...` after engine-only replacement, while capabilities and actual loaded assembly hashes report 1.34.1. XmlDb's exercised replacement path passes; Braid has 1,872 passing cases/four unchanged pinned-version assertion failures. No cache corruption was demonstrated.

Read-only inspection at clean Braid 0f1d949e758645f7944f617f2b4613e49161a195 confirms the identity is stamped from BosakDependencyRevision at build time. It is compared by preview configuration, parent/worker request-response checks and restricted-export freshness/approved-engine checks; regression settings/reports retain identity strings. [Selected source hashes](evidence/consumer-source-identities.json) identify the exact inspection inputs. No proprietary source or licensing fixture is copied into this dossier.

## Options and recommendation

| Option | Result and tradeoff | Disposition |
|---|---|---|
| Rebuild at a reviewed pin only | Fixes normal source-upgrade metadata, but does not detect later mixed/replaced runtime assemblies. | Necessary adoption path, insufficient by itself for runtime drift. |
| Reviewed pin plus verified runtime artifact manifest | Refuses drift before engine-backed work and binds retained result identity to the exact approved engine closure. Requires consumer packaging, worker and stale-result tests. | Recommended bounded direction. |
| Automatically accept arbitrary runtime closure | Could describe observed bytes, but bypasses explicit dependency review and would need a broader compatibility/admission design. | Defer; no automatic sidegrade policy. |

Actual current product pin remains 1.26.2/source 6a3372fc3b91e733b71a426cf40f26feaaeb3bcb. Core 1.34.1 is a qualification candidate, not an adopted Braid dependency. First qualify the guard using the current pin and deliberate 1.34.1 substitution; upgrading the actual pin is a separate consumer adoption step.

## Proposed contract for an isolated prototype

Build-time approval and runtime observation are separate facts. An artifact-specific approved manifest records the chosen source revision plus every supported engine assembly's simple name, assembly version/token and SHA-256. The manifest is produced from the actual reviewed deployment artifact, with deterministic ordering/encoding and duplicate-name refusal. It covers the nine Braid engine assemblies, not merely the XSLT facade. Paths and timestamps are excluded from the identity digest. Different Debug/Release/platform artifacts may have different approved manifests; do not silently treat them as identical.

A manifest is an exact compatibility/provenance label, not a substitute for trusted package distribution or a new security boundary against an attacker who can replace the manifest and application together. Signing/provenance remains the existing release pipeline's responsibility. Do not infer NuGet signing from assembly public-key tokens.

Before admitting engine-backed inspection, validation, preview or export, establish agreement between the approved artifact and the actual engine closure used in that process. Missing, duplicate, mixed, unreadable or mismatched assemblies must refuse engine-backed work with a clear identity-mismatch diagnostic. Do not load arbitrary assemblies or private types merely to compute identity. An isolated prototype must explicitly test loaded assemblies and lazy-load paths; checking filenames on disk alone is insufficient. Unsupported/dynamic/single-file layouts should refuse until separately qualified rather than pretend to verify.

The prototype must define the interval between verification and dispatch. A one-time disk hash is insufficient for long-lived worker replacements. Prefer observing a fixed loaded closure and rechecking admission at operation boundaries; a newly spawned worker independently verifies its own closure. If a stable supported binding cannot be established, refuse. This is not a claim of protection against hostile in-process code or an arbitrary loader race.

Keep source pin, runtime artifact identity and execution/resource profile distinct. A proposed versioned runtime identity may combine the reviewed source revision and manifest digest, but no final string/protocol/storage format is adopted here. Parent configuration, worker request/response, inspection and export approval must compare the same verified identity. Keep existing process deadlines, cancellation, resource policy, source/freshness and entitlement checks; manifest agreement grants none of those authorities.

Old regression/history/export evidence retains its original recorded identity and bytes. Treat a legacy pin-only identity as historical provenance, not proof of the current verified closure. Show retained history read-only where already supported; require fresh engine-backed validation/execution before reusing it as current. Do not silently rewrite identity fields or erase results. Protocol/file-version compatibility and explicit migration/refusal behavior must be demonstrated before product changes.

## Bounded next slice and acceptance

Create an ignored, independent contract prototype under this core workspace; do not edit Braid/core production or real pins. Use already qualified baseline/current artifact hashes and public-contract observations. The prototype is evidence for policy design, not a substitute for actual Braid integration or runtime admission.

| Control | Required result |
|---|---|
| Current approved artifact | Coherent 1.26.2 closure is admitted; source pin/runtime digest remain separate and deterministic. |
| Deliberate replacement | 1.34.1 replacement under old approval refuses; observing its bytes does not approve it. |
| Partial/mixed closure | Facade-only change, unchanged version with changed bytes, missing/duplicate/unreadable members and incorrect metadata refuse. |
| Identity stability | Relocation, enumeration order and timestamp changes preserve identity; approved byte changes produce a distinct identity. |
| Runtime binding | Verify the exercised loaded paths/hashes; test lazy loading, worker restart and files changed after initial validation. Do not claim those runtime paths from a pure data-model prototype. |
| Parent/worker agreement | Separately observed equal closures agree; wrong worker identity/profile/request ID refuses before accepting output. |
| Retained results | Legacy/different runtime identity remains readable history, fails currentness checks and needs fresh validation; no mutation of original evidence. |
| Independence | Passing identity checks never bypass source freshness, validation, resource limits or license/capability policy. |

Preserve original passing/failing qualification receipts. Add explicit bad variants that demonstrate controls fail when enforcement is removed. Report model controls separately from real runtime loading/worker execution. The following production slice, if selected, belongs to Braid: design/registry/ADR, public integration, complete source suites, unchanged-binary expectations, worker protocol/retained-result compatibility and applicable native acceptance. No request ID is assigned here. If an actual core API need emerges, register it through Braid's documented upstream request workflow before implementation; the current evidence establishes none.

## State and next action

No new specification semantics are adopted. This policy concerns consumer artifact identity and operation admission, not XPath/XSLT interpretation. Core production 1.34.1, Schema 0.9, original Braid/XmlDb checkouts and all pins remain unchanged. Earlier tests, gates and binary findings are inherited; no consumer or prototype execution occurred in this scope step. All sealed dossiers remain unmodified. Documentation checks are recorded separately.

Next: build the isolated policy prototype with positive/mismatch controls and an explicit boundary between modeled checks and actual runtime verification. Scope and synchronized documentation remain uncommitted.
