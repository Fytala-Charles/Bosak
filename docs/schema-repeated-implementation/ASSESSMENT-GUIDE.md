# Native document assessment — 0.7 development candidate

The activation-free `SchemaServices.CreateDocumentAssessor` API provides a **bounded native XSD 1.1 assessment**, separately from the existing assertion-only descriptor service. The source is an unpublished feature candidate; this is not full XSD 1.1 conformance or a general core schema-session API.

## Run the example

```sh
dotnet run --project examples/Bosak.Schema.Assessment.Example -c Release -p:BosakSourceRoot=D:/Development/Bosak
dotnet test Bosak.Schema.slnx -c Release -p:BosakSourceRoot=D:/Development/Bosak
```

The example compiles a schema with repeated `amount`, required `shipped` and optional `tax` decimal children and an assertion, assesses the XML structure and values, then queries/transforms a whole assessed document with its original root state. The output reports a valid assessment, decimal typing, total 15, and successful annotation-preserve/strip checks. The consumer supplies an empty BCL schema set solely as the legacy annotation-copy context; **no XSD 1.1 schema is compiled or validated through it**.

## Use the result

```csharp
var assessor = SchemaServices.CreateDocumentAssessor(schemaDocument);
var result = assessor.Assess(instanceDocument, cancellationToken);
if (result.Outcome == SchemaAssessmentOutcome.Valid)
{
    var assessed = result.Document!;
    var rootState = assessed.Elements[0];
    var originalComponents = rootState.Declaration;
    var preserved = assessed.Copy();
    var untypedXml = assessed.CreateDocumentCopy(preserveTypeAnnotations: false);
    var decimalLeaf = assessed.CreateTypedElementCopy("shipped"); // exactly one actual match
}
```

`IsSupported` describes compilation of this finite profile. It does not certify that an arbitrary schema is valid or fully implemented. `Invalid` means a supported structure, lexical constraint or assertion failed. `Unsupported` means coverage/representation was refused. Neither outcome exposes an `AssessedDocument`. Cancellation throws; it does not produce successful partial output.

## Exact profile

| Area | Supported coverage |
|---|---|
| Schema | One XML 1.0, DTD-free, no-target-namespace schema; one named global element with one anonymous element-only complex type. |
| Structure | One sequence of 1–16 unique local xs:decimal declarations, minimum zero/one and finite maximum 1–16, with sum of maxima at most sixteen, followed by 1–8 assertions. |
| Attributes | Namespace declarations; schema `version` and `xpathDefaultNamespace="##local"`; documented declaration name/type, local minimum zero/one and bounded maximum attributes; assertion test/local default namespace. Everything else is refused. |
| Assertions | Declared child names or exact decimal constants compared with eq/ne/lt/le/gt/ge, combined with and/or and parentheses under XPath 2.0. No functions, variables, axes, arithmetic or external access. |
| Values | XML-whitespace collapse, exact decimal lexical/value assessment. Non-decimal syntax is Invalid; valid values requiring rounding/overflow/underflow are Unsupported. |
| Instance | Exact root, child count/order/names, element-only root text and decimal leaf content. No application attributes or xsi requests. Root comments/PIs and decimal CDATA are supported; comments/PIs inside decimal leaves are Unsupported. |
| Bounds | 32 KiB serialized schema, 64 KiB serialized instance, 256 descendant nodes, 256 decimal lexical characters, 512 assertion characters, 128 assertion tokens and sixteen parenthesis levels. |

Imports/includes, named types, attributes, facets, alternatives, nil/default/fixed, identity constraints, wildcards, open content and other constructs are explicitly outside the profile. Unsupported constructs are never dropped and sent to a 1.0 validator. The native implementation uses no `XmlSchemaSet` structural-validation path.

## Ownership and original state

The assessor captures immutable schema text, original declaration/type objects and assertion plans. Each successful assessment owns a separate normalized XML snapshot and immutable root/present-element states. Omitted optional declarations remain in `ChildDeclarations`, without placeholder instance states. The root retains its anonymous complex type and **has no typed value**; decimal children retain their actual built-in type, governing local declaration and exact typed value. This profile never admits nilled elements.

Native `Copy()` preserves original PSVI, component references and `AssessmentIdentity` without revalidation; it creates an independent snapshot and `SnapshotIdentity`. These are process-local instance labels, not persistent source identities or compatibility proofs. Independent compilation creates independent anonymous component authority even when schema digests match.

The result cannot be mutated through the original schema/instance, its read-only collections or exported XML. Exported XML is caller-owned; later edits invalidate any assumptions about that export's assessment and do not modify the retained result. Inputs must not be concurrently edited while being captured. Immutable compiled assessors can process independent documents concurrently; no session/resource is disposed on behalf of the caller.

## Engine projection boundary

`CreateTypedElementCopy` exports a **detached built-in decimal value view**, not local-declaration authority or a complete PSVI document. Use it with `XDocumentNode` for real typed XPath and wrap it in an `XDocument` for the example's XSLT initial-template invocation. Native and engine preserve/strip copies are tested separately.

`CreateDocumentCopy(preserveTypeAnnotations: true)` now exports the whole document through the unpublished core 1.34.1 `XdmElementAssessment` provider bridge. Original owner and element/component state are retained; the anonymous element-only root has no typed value (FOTY0012 on atomization), while decimal children retain exact typed values. The false branch exports stripped XML. XQuery/XSLT preserve copies retain state and strip copies remove it. Edited content refuses stale values; namespace fixup and ignorable element-only whitespace preserve state.

Legacy reassessment and declaration projection refuse explicitly. No anonymous name is synthesized and QName/digest equality grants no cross-schema admission. Full schema-element matching, compatible universes, general PSVI serialization and product adoption remain pending. The candidate requires `BosakSourceRoot` because core 1.34.1 is unpublished; original public checkout/package pins remain unchanged.

## Historical assessment-only qualification

The complete candidate Schema suite passes **191 cases**: the original 114 plus 77 new assessment cases. It passes against the unchanged published 1.26.3 package pins and against current core source in an isolated source wrapper. Existing 114-case test binaries also pass with only the Schema library replaced by the candidate. These scopes overlap and must not be added together.

The general core production code, frozen conformance gates and consumer dependency pins are unchanged. No new hosted CI result, package/tag publication or public API freeze is claimed. See the [adoption record](../schema-assessment/ADOPTION.md) and [ADR-008](../schema-assessment/ADR.md).

## Current whole-document qualification

Fresh bridge qualification: Release build has zero warnings/errors; complete core aggregate **24,207 passed**, native Schema **204 passed** (13 whole-document cases), QT3 **31,397/0/424**, qt4 gated **8,941/0/1,918**, 230 sets and 11 experimental passes. Exact skip mappings match the sealed integration checkpoint. The initial sandbox database failures are retained separately; the local-HTTP-authorized rerun passes all 47 cases. Whole-document example and strict Schema documentation checks pass. Core 1.34.0 source override is required for this candidate; historical published-1.26.3 runs above do not qualify the new bridge.

## Reviewed whole-document bridge — 29 October 2026

The optional provider attachment API now checks a replacement completely before removing the previous binding. Failed replacement retains original state and stale-content refusal; failed initial attachment leaves the node unregistered. The trusted host must supply immutable state and the correct assessed value. This API carries that state without performing schema validation. The finite native profile is unchanged; optional children and general API freeze remain pending. Current core candidate is unpublished 1.34.0; use the BosakSourceRoot override. New review qualification lives in the core `docs/schema-document-bridge/api-review` dossier, separately from historical assessment receipts.

Post-review verification: **24,209 core tests pass in one complete solution run**, **204 Schema tests pass**, and all six focused provider attachment cases pass. Release build: zero warnings/errors. QT3 **31,397/0/424**, qt4 **8,941/0/1,918** (230 gated sets, 11 experimental passes), with exact skip mappings unchanged. The full-document example retains original root state and passes typed-value and preserve/strip checks. Prior sealed assessment, bridge and integration receipts remain unchanged.

## Historical optional decimal child scope — 29 October 2026

The [new adoption record](../schema-optional-children/OPTIONAL-DECIMAL-ADOPTION-2026-10-29.md) and [implementation plan](../schema-optional-children/OPTIONAL-DECIMAL-PLAN.md) define the next finite extension. It is not implemented. Keep occurrence bounds on particles, actual element states only for present nodes, and use effective Boolean conversion for empty assertion results. Maximum occurrence stays one; no broader grammar/session admission. Proposed implementation version is Schema 0.6; this documentation step keeps 0.5 and core 1.34.0 unchanged. Fresh baseline: 204 Schema tests pass. Core solution/gates inherit the source-identical `a163ff11` checkpoint. No REQ completion or publication.

Scope refinement: a thirteenth isolated observation exposes ignorable-whitespace removal being refused for an empty element-only assessed root. Supporting all-optional-absent documents therefore needs a bounded provider binding correction. The future implementation proposes core patch 1.34.1 plus fresh complete core tests and exact conformance gates; current versions and production source stay unchanged. Both the initial twelve and extended thirteen-observation receipts are preserved in the optional-child dossier; they overlap.

## Optional decimal scope delivery — 29 October 2026

See the [synchronized checkpoint](../schema-optional-children/delivery/README.md) for current delivery identity and remote target. Documentation is prepared for the user-authorized commit/push; optional-child semantics are not implemented. Current versions/pins and production source remain unchanged. Thirteen observations and 204 passing Schema baseline cases support the scope; the core suite/gates are inherited. Next: implement the bounded plan and its core binding correction with fresh required qualification.

## Optional decimal implementation candidate — 29 October 2026

Current behavior: [Schema 0.6 optional assessment](../schema-optional-implementation/GUIDE.md), with core 1.34.1 and required local source override. Local zero-or-one particles, ordered matching, assertion EBV and present-only original state are implemented; required-only v1 remains and optional schemas use v2. Empty element-only root whitespace normalization is corrected while real/decimal mutation refusal remains. The complete Schema suite passes 260 cases, including 56 new optional cases; the full-document example retains later declaration identity. Earlier planned-scope/version notices are historical. Broader REQ-009/general session/API freeze and consumer acceptance remain pending. Original public checkout and product pins are unchanged; implementation is uncommitted and unpublished.

Fresh final qualification: **24,214 complete core tests pass** (including 8,196 XSLT and 212 provider cases), **260 complete Schema tests pass** (56 new optional cases), normal QT3 **31,397/0/424**, qt4 **8,941/0/1,918**, 230 gated sets and 11 experimental passes. Exact skip names/reasons match the sealed integration checkpoint. Core build has zero errors and five warnings in unchanged Standard test/XSLT conformance sources; no new warning source is introduced. The optional-gap whole-document example and strict Schema documentation check pass. Original 45-case before evidence is 9 passed/36 failed; all 45 now pass alongside eleven added boundary cases. Five new core cases cover the root binding correction.

## Optional decimal review checkpoint — 29 October 2026

Current delivery: [reviewed optional implementation](../schema-optional-review/README.md), Schema 0.6/core 1.34.1. No further production fix was needed; source matches tested qualification. The user authorized checkpoint commit/push; original uncommitted notices are capture-time history. Next: scope repetitions separately before implementation. Full schema/session/API freeze and product adoption remain pending.

## Bounded repeated decimal scope — 29 October 2026

The [repetition adoption](../schema-repeated-children/REPEATED-DECIMAL-ADOPTION-2026-10-29.md) and [implementation plan](../schema-repeated-children/REPEATED-DECIMAL-PLAN.md) define the next slice. Repetitions are not implemented. The reviewed optional source is checkpointed at Schema `773fd69` / core `5e277c6a`; current Schema 0.6/core 1.34.1 remain unchanged. Sixteen diagnostic observations cover current refusal and real-engine comparison cardinality, not native repetition acceptance. Proposed Schema 0.7/v3 keeps grammar and public overloads unchanged, bounds total occurrences, preserves original state, handles known cardinality errors and refuses ambiguous singular projection. Broader API/session acceptance remains pending.

## Bounded repeated decimal candidate — 29 October 2026

Current behavior: [Schema 0.7/v3 repetition implementation](GUIDE.md). Local minimum zero/one, maximum and total budget up to sixteen, contiguous ordered states, shared original declarations, narrow XPTY0004 assertion failure and ambiguous singular projection refusal are implemented. Required-only v1 and optional v2 remain; broader grammar is refused. Fresh complete Schema tests: 334 passes (74 new). Build/example pass; unchanged core 1.34.1 full/gate evidence is inherited after source checks. Earlier planned-scope/version notices describe earlier capture. General schema/session acceptance and product adoption remain pending. Source changes are uncommitted and unpublished.

Final repeated-candidate qualification: **334 complete Schema cases pass**, including **74 new repetition/error-boundary cases**. The original 57-case before cohort (17 passed/40 failed) now all passes; seventeen further boundary/safety cases also pass. The prior 260-case suite retains 259 unchanged passing expectations and one explicitly adopted maxOccurs-two coverage/profile update. Schema source-override Release build: **zero warnings/errors**. The repeated-document XPath/XSLT example and strict Schema docs check pass. Core source/tests/harness/fixture references and build configuration are unchanged from the reviewed source, so **24,214 core passes**, QT3 **31,397/0/424** and qt4 **8,941/0/1,918** (230 sets) are inherited, not fresh full runs. Exact raw source/receipt identities and all earlier sealed dossiers are preserved.
