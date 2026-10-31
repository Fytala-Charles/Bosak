# C44 — Proposed schema session and identity API

**Proposed; owner acceptance pending.** The [compiled declarations](evidence/proposed-contracts.cs.txt) export 24 types under the deliberately isolated namespace `C44.Proposed`. The proposed production home is `Bosak.XPath.Core.Schema`; no type is added to a product assembly in C44. The declaration assembly is an API-shape check: issuer and atomic-attachment bodies explicitly throw, and it is not a usable provider. Strict DTO/diagnostic validation and all product routing remain C45 implementation requirements.

## Exact proposed neutral surface

[api-surface.json](evidence/api-surface.json) records the compiler-produced signatures, including DTO constructors, properties and methods. The assembly references only Bosak.XPath.Core and BCL assemblies; it has no API/runtime/XSLT/commercial dependency. Existing IXsdValidator, XmlSchemaSet members and IXdmNode remain intact.

| Contract | Proposed signature / data |
|---|---|
| Provider | `SchemaOutcome<ISchemaSession> OpenSession(SchemaCompilationRequest request, CancellationToken cancellationToken)` |
| Session identity | `SchemaSessionIdentity Identity { get; }` |
| Element/attribute lookup | `SchemaOutcome<SchemaDeclarationIdentity> ResolveElement/ResolveAttribute(SchemaName name, string scope, CancellationToken cancellationToken)` |
| Named type lookup | `SchemaOutcome<SchemaTypeIdentity> ResolveType(SchemaName name, CancellationToken cancellationToken)` |
| Component descriptions | `DescribeElement(SchemaDeclarationIdentity, CancellationToken)` and `DescribeType(SchemaTypeIdentity, CancellationToken)` return typed SchemaOutcome descriptions |
| Derivation | `SchemaOutcome<bool> IsDerivedFrom(SchemaTypeIdentity actual, SchemaTypeIdentity expected, CancellationToken cancellationToken)` |
| Complete membership | `SchemaOutcome<ImmutableArray<SchemaDeclarationIdentity>> GetActualMembers(SchemaDeclarationIdentity head, CancellationToken cancellationToken)` |
| Full claimed assessment | `SchemaOutcome<SchemaAssessment> Assess(SchemaAssessmentRequest request, CancellationToken cancellationToken)` |
| Optional node channel | `ISchemaAnnotatedNode : IXdmNode` adds `SchemaNodeAnnotation? SchemaAnnotation { get; }` |
| Atomic channel | `SchemaAtomicValues.Attach(XdmValue value, SchemaValueAnnotation annotation)` and `TryGetAnnotation(XdmValue value, out SchemaValueAnnotation? annotation)` |

SchemaCompilationRequest contains immutable resource bytes with origin URIs, base URI, language, resource-policy fingerprint, namespace bindings and static-context fingerprint. The host supplies the resolved permitted bundle; the provider must verify the complete consumed import/include closure. URI spelling is assumed canonical in the fake prototype; production must validate/canonicalize origin identities explicitly. No provider may fetch undeclared resources or reuse a root-only digest as the component-universe key.

SchemaName is an expanded namespace/local pair. Declaration identities additionally carry a provider-defined declaration key and scope; type display names can be null for anonymous types. SchemaIdentityIssuer is privately held by the trusted provider, interns identities within one fresh session token, and is sealed before publication. Clients see the token, not the issuer. Equal fingerprints/display names do not imply reference identity; sharing the same immutable session is the supported cross-executable reuse path. This is an integrity/lifetime boundary for conforming hosts, not a security sandbox against an intentionally dishonest registered provider.

Element descriptions include type, an affiliation **list**, abstract/nillable flags and blocked derivations. Type descriptions distinguish atomic/list/union and complex content kinds, bases, members, list item type and built-in base name. GetActualMembers must return a complete immutable result, including supported false/empty cases; a partial list must be Unsupported, never a successful subset. The provider owns XSD 1.1 component semantics. Core uses the returned set for matching/containment without reducing affiliations to one head.

SchemaValueAnnotation holds the actual ISchemaSession strongly plus its type identity. SchemaNodeAnnotation adds governing declaration, nilled/complex/no-typed-value and ID/IDREF properties. Production validates every returned identity against the captured session, including declaration identities in assessment results. Caller trees remain untouched, preserve copies retain annotations, and strip copies remove descendant/attribute annotations consistently. Reattaching an annotation may preserve the existing same-session/type identity, but must not replace its owner or actual type without explicit reassessment. A value whose provider provenance was explicitly removed by a valid built-in conversion is distinct from an owned value silently entering an unregistered legacy path.

## Registration refinement

Prefer **explicit host-opened immutable sessions**, captured at compilation, over a mutable runtime advisor slot. Proposed additive registration members are:

```csharp
// Proposed members only; not compiled into product assemblies by C44.
CompileOptions: public ISchemaSession? SchemaSession { get; init; }
XsltCompiler: public ISchemaSession? SchemaSession { get; set; }
XQueryCompiler: public XQueryCompiler WithSchemaSession(ISchemaSession session);
EvaluationContext: public ISchemaSession? SchemaSession { get; } // internal capture/clone propagation
```

A host calls OpenSession first. NotApplicable permits choosing the established legacy path before registration. Once a session is captured, NotApplicable is a protocol failure, Unsupported/Invalid stop explicitly, cancellation remains cancellation and provider exceptions retain their cause. No mutable EvaluationContext setter, namespace-only authority selector, implicit provider discovery or commercial activation is added.

This matters for XQuery: current schema imports are resolved during Evaluate using XQueryContext's legacy resolver. Registering a session on that mutable runtime context could switch the authority of an already compiled query. The proposed WithSchemaSession captures the immutable environment at Compile; imports outside its declared owned closure refuse explicitly. Existing context-resolver behavior remains unchanged for unowned legacy imports. A per-execution context overlay carries the captured session without mutating a caller's context or accepting a foreign session from it.

## Carrier decision and compatibility limits

Recommend a private payload envelope in XdmValue's **existing `_reference` slot** for provider-owned atomic values, with the inline discriminator/integer/double fields and both legacy type-name fields retained. Expose the new channel through SchemaAtomicValues; no new XdmValue field or public XdmValue member is required. All existing reference-payload access paths must unwrap consistently, including string/decimal/QName/date-time/duration/URI accessors, EBV, serialization and equality/hash consumers. No unwrapping through reflection, ambient dictionary or lexical-value key is permitted.

On this .NET 10 Windows x64 machine, real XdmValue and the alternate six-field carrier measure **48 bytes**; adding an identity reference measures **56 bytes**. A synthetic same-identity library replacement passes an unchanged v1-compiled consumer, including annotated payloads, copies, arrays, boxing, generics and in-parameter calls. This is evidence for the design, **not production ABI certification**. The seven supported alternate-carrier kinds and simplified model do not cover all production date/time/binary/URI/function/sequence paths. Reflection in C44 only inventories public API/private field categories for review; no product private member is modified or used by a provider.

A lexical-key sidecar collides for equal values/type names from different sessions. An ambient dictionary would also fail copied value/lifetime guarantees. The payload envelope keeps provenance with the copied value and allocates only for annotated carrier creation in the experiment. Its observed integer envelope cost is 32 bytes per creation after subtracting the read-loop baseline; timing includes delegate/timer effects and is not a product no-provider benchmark.

## Host result and error policy

Use five verdicts: NotApplicable, Supported, Invalid, Unsupported, Cancelled. Supported(false) is data. Production result factories must reject malformed/default findings and null supported reference payloads; adapters must validate all provider responses. Escaping provider faults are wrapped with their cause and never retried through legacy validation. No ownership check is skipped because the textual fingerprint or QName matches.

The prototype projects these established categories: invalid XQuery schema compilation → XQST0012; unresolved import → XQST0059; invalid XQuery instance → XQDY0027; invalid XSLT instance → XTTE1510; strict missing XSLT declaration → XTTE1512; no typed value → FOTY0012; binding mismatch → XPTY0004. Unsupported coverage and provider failures are structured host-level failures, **not new W3C error QNames and not successful false results**. Cancellation remains separate. Existing detailed error precedence, including document/type/QName validation distinctions, must stay with existing host adapters and be tested during implementation; the ten-row projection test is not full host error-family certification. Do not reuse the static authoring XsltValidationResult API as an instance-schema backend result.

All permanent API signatures and the guarded production candidate remain subject to the [owner decision](OWNER-DECISION.md). Bosak.Schema supplies and licenses its own XSD 1.1 implementation; C44 does not certify one.
