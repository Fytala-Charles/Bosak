# C44-R2 — Revised neutral API and interoperability proposal

**Proposed; owner acceptance pending.** All compiled declarations live in `C44R2.Proposed`, not product assemblies. The candidate production home remains `Bosak.XPath.Core.Schema`. [Compiled declarations](evidence/Contracts.cs.txt), [exact reflected surface](evidence/api-surface.json) and [namespace-normalized C44 diff](evidence/api-diff.json) are authoritative for this proposal.

## Explicit inputs and identities

The old `SchemaCompilationRequest.Language` is replaced by `QueryProfile` and `SchemaLanguage`. The original schema static-context digest remains separately named `StaticContextFingerprint`; `QuerySemanticProfile` carries the query static-context digest. The proposal has no implicit schema/profile defaults.

| Proposed contract | Meaning |
|---|---|
| `QueryHostLanguage` | XPath, XQuery or Xslt; independent of the XSD version. |
| `QuerySemanticProfile(HostLanguage, HostVersion, XdmRevision, BehaviorProfile, QueryStaticContextFingerprint)` | Exact consuming query environment. A production host must derive/validate it against actual compile options and adopted semantics; caller text cannot silently switch an already compiled executable. |
| `SchemaLanguageVersion` | Xsd10 or Xsd11; identifies schema interpretation, not query grammar. |
| `SchemaUniverseIdentity(ProviderModelVersion, SchemaLanguage, Fingerprint)` | Describes the complete interpreted component closure. It is reproducible data, not an authority token or compatibility certificate. |
| `SchemaSessionIdentity.Universe` | Associates a fresh reference-bound issuer/session token with its universe description. `Fingerprint` remains a descriptive projection. Equal fingerprints do not make tokens equal. |
| `SchemaSourceIdentity.Create(Provider, SnapshotKey, ContentFingerprint)` | Fresh host-owned provenance token. Equal descriptions/digests do not make tokens identical. It grants no XML node identity, storage lease, edit authority, writer rights or disposal ownership. |
| `SchemaValueAnnotation.SourceIdentity` | Optional retained original host source token, alongside the original session and actual type. The constructor still requires a type issued by that session. |

`ISchemaSession` gains `QueryProfile` in the **new proposed interface only**. No member is added to production `IXdmNode`, `IXdmSequence` or `XdmValue`. No XML enum value changes. `SchemaIdentityIssuer.Create` now takes the universe descriptor; it validates descriptor shape, interns IDs, rejects redefinitions and seals before publication in the prototype. The provider must retain control of the issuer.

The eight added exported types are the two enums, query profile, universe descriptor, source token, compatibility request, compatibility authority and compatibility view. Five existing proposed types change: `SchemaCompilationRequest`, `SchemaSessionIdentity`, `SchemaIdentityIssuer`, `SchemaValueAnnotation`, `ISchemaSession`. The public-surface diff concerns unaccepted neutral proposals; it is **not** a binary-compatibility claim about replacing product assemblies.

## Provider-attested directional view

```csharp
// Proposed neutral declarations, not shipped product APIs.
SchemaOutcome<ISchemaCompatibilityView> ISchemaCompatibilityAuthority.Establish(
    SchemaCompatibilityRequest request, CancellationToken cancellationToken);

// Request: Source session, Target session, exact target ExecutionProfile.
// View: Source, Target, ExecutionProfile, plus:
SchemaOutcome<bool> IsDerivedFrom(
    SchemaTypeIdentity actual, SchemaTypeIdentity expected,
    CancellationToken cancellationToken);
SchemaOutcome<bool> MatchesElement(
    SchemaNodeAnnotation actual, SchemaDeclarationIdentity expected,
    CancellationToken cancellationToken);
```

The authority verifies its ownership of both exact session objects, checks its supported schema/profile coverage, compares overlapping definitions transitively and qualifies the complete combined component model. The returned immutable view is bound to these endpoints and consuming profile. Source and target can have different host/query profiles when the component interpretation is compatible; they are not required to share one session token. The **target** consuming profile must match the captured view.

Type relations take a source actual type and target expected type. Element relations take the original source annotation and target head declaration, preserving original declaration/type/nilled state. Foreign source or target IDs fail even when their names, fingerprints, component keys or source snapshot labels match. `Supported(false)` is data; it is never confused with lack of coverage.

A view uses the qualified union for derivation and actual membership; it does not project an unknown source-only actual type into a fictional target identity. A successful atomic binding returns the original annotation. Compatible subset/superset cases, anonymous source derivation, rekeyed named components and multi-head membership are exercised. Full XML Schema substitution/derivation/content-model semantics still belong to the real provider.

The experiment admits sessions opened by **one fake provider instance**. Equal-fingerprint sessions from another provider instance are explicitly unsupported. A real implementation may add a trusted interoperability broker or shared component authority after qualification; arbitrary cross-provider compatibility is not established here. These are integrity rules for registered conforming hosts/providers, not a security sandbox against dishonest implementations of public interfaces.

## Canonical identity and execution-key encoding

The executable fake provider consumes an explicit closed bundle of immutable **JSON component descriptors**, not XSD documents. It refuses missing imports, cycles, unconsumed supplied resources, duplicate canonical origins/prefixes, malformed digests and incomplete claimed coverage. No resource fetch path exists.

Its universe key is SHA-256 over a versioned binary encoding of provider/model version, explicit XSD version, root base URI, resource-policy digest, schema static-context digest, every consumed resource's canonical origin and SHA-256 bytes, and schema namespace bindings. Resources sort by ordinal canonical URI; bindings sort by ordinal prefix. URI normalization is pinned to .NET 10 `Uri.AbsoluteUri`; credential-bearing or fragment-bearing resource identities refuse. Filesystem alias equivalence is not inferred. Digest hexadecimal spelling normalizes to uppercase.

The encoder uses little-endian 32-bit enum/count fields, and `BinaryWriter.Write(string)` with strict UTF-8/no BOM and its 7-bit encoded byte-length prefix. The format marker is `c44r2-universe-v1`. The executable key uses `c44r2-executable-v1`, the universe digest and all query-profile fields, with query-context digest casing normalized. Source/storage tokens are absent from both component and executable keys. Changes in query context affect the executable key without automatically changing an otherwise identical XSD universe; schema interpretation changes belong in the schema context/provider identity.

These exact formats qualify the experiment. A production canonicalization format, supported profile identifiers and provider model/version policy require explicit acceptance and interoperability tests. No `c44r2-fake` behavior identifier should ship as a production semantic profile.

## Retention, errors and boundaries

Explicit host-opened sessions remain captured at compilation. Runtime context setters do not switch authority. An owned Unsupported/Invalid result, provider fault or lost ownership stops explicitly; cancellation remains cancellation; provider errors retain their cause. Structured supported/failure result factories reject null success payloads and malformed findings. The existing detailed host error families/precedence remain adapter requirements; no new W3C error QName is invented.

Annotations and escaped atoms retain their original session. Host registration disposal prevents new captures but does not invalidate captured values/views. Source snapshots retain their existing disposal/thread rules; a schema annotation does not revive a disposed source. Tree preserve/strip and atomization controls cover root/child/attribute annotations. Record labels remain core item-type metadata, separate from XSD component identities.

The fake provider intentionally retains its opened sessions for the finite test run; it does not qualify production registry reclamation. The GC control uses an independent minimal session to show annotation retention. Attribute assessment, full list/union/complex PSVI, actual XSD validation in the fake provider, CTA and real consumer carrier integration are unqualified. The fake node is a minimal witness, not a complete node provider. Integer built-in conversion in the alternate carrier is a policy demonstration, not an all-payload accessor implementation.

Keep the existing-slot envelope as the production carrier candidate, retaining C44's size/ABI evidence as bounded historical input. This revision neither replaces the real carrier nor adds a field; it does not repeat or broaden the 48-byte or synthetic ABI certification. Actual payload, memory, disposal/concurrency, performance, provider and unchanged-consumer tests remain mandatory before production API freeze.
