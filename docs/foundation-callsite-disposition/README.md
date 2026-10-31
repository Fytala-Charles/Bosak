# Foundation call-site disposition ? 31 October 2026

The next implementation slice should repair the provider-to-Core typed-value boundary and capture semantic context privately. V4 is a reviewed experimental shape, not an implementable final contract: its typed-value producer path is incomplete, and its compiler-context mirror exposes too much while capturing too little. No production API or acceptance decision changes in this document.

## Actual-source findings

Exact committed revisions and selected file hashes are in [SOURCE-PINS.json](SOURCE-PINS.json). This is source inspection, not adapter execution or consumer qualification.

| Consumer / source | Actual operation | Disposition |
| --- | --- | --- |
| Native Schema assessment c362499, SchemaDocumentAssessment / AssessedDocument | Plain nullable XdmValue plus separate original declaration, type and assessment objects; copies retain component identity and change snapshot identity. | Adapt those original objects into issued handles and immutable value/evidence cells. V4 currently asks for already annotated atomics without a production attachment operation. |
| Native Schema AssessmentProfileParser | Anonymous root, shared decimal type and local declarations; assertions compiled in XPath20 mode. | Qualifies a bounded decimal profile only. General list/union construction, nil, derivation, affiliation and cross-universe compatibility remain requirements, not supplied capabilities. |
| Public Schema main 2009018 | Separate public SchemaServices line. | Keep its evidence separate from the native assessment candidate; do not combine their capabilities into an assumed full XSD 1.1 provider. |
| Core CompileOptions / XPath31Expression | Public settings plus internal item-type aliases, record names and function parameter/minimum-arity callbacks; compile merges namespaces. | Privately capture effective compiler and host inputs. Copying a callback reference does not freeze its mutable captured state. |
| XmlDb f9f74fa, PersistentXdmNode | TypedValue returns a string; identity and lazy outer result access depend on the snapshot. Core package remains 1.26.2. | Preserve existing behavior; typed schema input needs an explicit adapter. Outer BindResult guards do not prove nested lazy-value detachment or lifetime safety. |
| Braid 0f1d949, BosakPreviewExecutor / PreviewWorkerProtocol | Byte-input worker protocol, reparsed untyped input, fresh compiler/evaluation context. | Native typed/persistent inputs need an explicit route and worker ownership design. No native Schema/XmlDb route is demonstrated. |
| Braid PreviewRunConfiguration | Existing profile/engine/input/fixture fingerprints. | Add a versioned semantic descriptor to future currentness keys; a digest never grants authority or preserves an in-process identity across a worker boundary. |

XmlDb's inspected HEAD has advanced from historical owner-input 2f9a36b to f9f74fa. Existing owner receipts remain tied to their recorded revisions. This source inspection neither reruns nor broadens that qualification.

## Finite next slice: V5 boundary experiment

Prepare an isolated V5 candidate derived from [V4](../foundation-granularity-experiment/CORE-CANDIDATE-V4.cs.txt), with the following proposed changes. These are recommendations awaiting implementation and diagnostics, not new adopted signatures.

1. Replace ISchemaTypedValue.Atomics with an ordered boundary enumeration of readonly SchemaAtomicItem cells: plain XdmValue plus ISchemaAtomicProvenance. Add an optional scalar fast path that returns one cell without enumeration. Keep IsList so singleton and empty lists retain their boundary semantics.
2. Add advisor ownership checking for the issued immutable typed-value container. Reject a foreign or unissued container before reading its getters/enumerating it. Copying legitimate cells into a forged container must not confer authority. Validate default cells, absent evidence and non-atomic values; default structs can bypass constructors. Existing issuer/authority admission still applies to component evidence.
3. Keep the public static-context namespace snapshot needed for QName construction. Move the remaining compiler-option mirror into private capture. Capture immutable declaration tables or a versioned immutable host registry rather than serializing delegates or retaining mutable callbacks.
4. Give the semantic environment a descriptor format version and deterministic context fingerprint for diagnostics/cache keys. Specify canonical encoding and all effective inputs before using it. Exact admitted environment identity remains required; equal fingerprints do not admit a forged environment.
5. Have Core consume the provider cells and attach the selected internal carrier privately. Do not introduce an arbitrary public annotation factory or prescribe the carrier based on this source audit. SchemaAtomics.TryGetProvenance remains a proposed read helper until a production transport exists.

The additional cell type supplies a missing producer operation; removing public context getters reduces continuing coupling. Public type count alone is not a performance metric. Enumeration is a provider boundary, not a proposed replacement for VM hot loops. Check ownership once per issued immutable container where safe; measure scalar dispatch/allocation before adding further public fast paths.

## Required diagnostics and stop conditions

The experiment must compile the candidate against the exact retained Core DLL, replay the existing 54 protocol controls with explicitly recorded fixture adaptations, and add producer/consumer controls for:

- Plain scalar transport without provider access to Core internals; scalar fast-path use without enumeration.
- Empty and singleton lists, ordered distinct union-member evidence, and computed values without original assessment identity.
- Rejection of forged/unissued containers before any getter or enumeration, plus missing/default evidence and non-atomic payloads.
- Immutable namespace capture and deterministic descriptor encoding; a changed private host-declaration revision changes the descriptor without expanding the public interface.
- Rejection of descriptor-equal forged environments and unavailable source snapshots; no ownership transfer or lease inferred from metadata.

Record allocation and timing only for the exact diagnostic path. Do not infer actual Schema adapter execution, full XSD 1.1 coverage, VM provenance correctness, nested XmlDb lifetime correctness, worker transport correctness or final performance budgets from these controls.

After those diagnostics, implement the smallest real native-assessment adapter for the existing decimal profile, with original anonymous/local declaration identity preserved. Then qualify actual API/VM transport and real lazy consumer lifetime before extending the provider capability set. External owner implementation remains delegated to the respective owners; this plan sends no external messages.

## Specification and acceptance boundary

This step records source findings and proposed work only. It adopts no new XPath/XDM/XSD semantics. Before implementation, record immutable specification sources, exact affected sections and deviations; the existing [provenance specification basis](../foundation-provenance-lifetime/README.md) and [advisor proposal](../foundation-advisor-contract/README.md) are historical proposal evidence, not blanket adoption for future changes.

Production remains Core 1.34.2, source c88eea43. C45, the named stable manifest, full consumer qualification, performance budgets and Core/Schema/XmlDb/Braid sign-offs remain pending. The final-breaking-foundation-change intent remains; API freeze requires those dispositions. No production source, package pin, external repository, store or sealed earlier receipt was changed.
