# FG-01/02/07 — Proposed stable and legacy boundary

30 October 2026. **Proposal; all three owner dispositions Pending.** This uses exact compiled signature evidence, not a new normative semantic adoption. [REQ-140](../FOUNDATION-API-FREEZE-PLAN.md), [REQ-139 owner work](../REQ-139-OWNER-WORK-PACKAGES.md) and the [pending C45 decision](../conformance/k04-schema-compatibility-prototype/OWNER-DECISION.md) remain authoritative for scope. No interface is implemented or accepted here.

## Proposed disposition

Keep two explicit review ledgers before freeze: the required shared foundation contract and existing legacy compatibility obligations. They must be reviewed together; placing an existing member on the legacy ledger does not make its compatibility optional. The exact named freeze scope and supported platform/profile still need joint owner disposition.

| Boundary | Proposed treatment | Required evidence before freeze |
|---|---|---|
| .NET platform types used by Core/query | Retain supported primitives, collections, immutable collections, IO and callback/modifier contracts. A neutral schema contract need not be free of every BCL type. | Exact public signature closure, supported target/runtime/assembly binding and package-only qualification. Do not pin internal implementation assembly names as new public dependencies. |
| Existing XML adapter contracts | Keep existing XmlReader/stream/XObject/XElement paths and caller ownership. Their concrete adapter coupling need not become a requirement on every persistent provider. | Source/binary/error/default/resource behavior, including the separately qualified 1.34.2 completion fix. Public-signature reachability does not replace actual runtime/package dependency review. |
| Existing System.Xml.Schema paths | Preserve unchanged public members, defaults and documented behavior on an explicit legacy compatibility ledger. No removal, automatic obsolete attribute or silent provider substitution. | Existing compiled callers, legacy/no-provider routes, exact component/error/default semantics and mutable environment/caching rules. All nine declarations inside the proposed graph need explicit member-level disposition. |
| Existing XSD validation facade | Keep IXsdValidator/XsdValidator result/stream APIs under legacy facade review even where no System.Xml.Schema type appears in the signature. | Actual validation/error behavior and supported profile; no inference of complete XSD 1.1/component/provenance support from a stream/error-result interface. |
| Bounded native assessment bridge | Preserve original Owner/State retention and anonymous/decimal supported facts; keep current copy/strip/refusal rules. | Actual provider coverage, provenance and lifetime. Object-shaped metadata is not a complete neutral session/component universe or compatible-view protocol. |
| Future shared neutral schema seam | Optional core-owned contracts may be proposed after reconciling real Schema and XmlDb needs; concrete Schema/XmlDb/licensing implementations must not become Core dependencies. | Full known component/derivation/affiliation/typed list-union/provenance/copy/ownership requirements, finite signatures and explicit C139-01/C45 scope. No new signature accepted here. |
| LanguageServer/LSP application types | Outside the proposed foundation scope; retain their existing application obligations separately. | Verify the core/API package graph does not gain these application dependencies. Observed third-party assembly versions do not establish their NuGet identities or a foundation package requirement. |

## Concrete legacy signature witnesses

| Owner/type | Logical member | Declarations in proposed type graph |
|---|---|---:|
| Bosak.XPath.Api.CompileOptions | SchemaSet (init option) | 3: getter, setter and property |
| Bosak.XPath.Runtime.Vm.EvaluationContext | SchemaSet (mutable context) | 3: getter, setter and property |
| Bosak.XPath.Runtime.Vm.EvaluationContext | GetSchemaElement | 1 |
| Bosak.XPath.Runtime.Vm.EvaluationContext | GetSchemaAttribute | 1 |
| Bosak.XPath.Runtime.Vm.EvaluationContext | GetSchemaType | 1 |

The remaining 17 schema-exposing declarations are in in-memory provider validation/annotation/result APIs and XsltCompiler.SchemaSet/EnvironmentSchemaSet. The full 26 declarations are individually identified in [the inventory](evidence/signature-dependencies.json). Core's current public signatures expose no System.Xml.Schema definition; that observation alone does not complete its future neutral requirements.

The eight observed schema definitions are IXmlSchemaInfo, ValidationEventArgs, ValidationEventHandler, XmlSchemaAttribute, XmlSchemaContentType, XmlSchemaElement, XmlSchemaSet and XmlSchemaType. Their use spans callback shapes, validation results, declarations and mutable schema sets. Changing a name/default, interface requirement, return shape or dispatch behavior can affect existing clients even if a future optional provider is otherwise additive.

## Decisions still open

- **FG-01:** decide exact member-level inclusion and dependency closure. Whole-type exclusion of legacy members cannot silently waive existing obligations.
- **FG-02/03:** select the minimal neutral contract from actual full-XSD-1.1 requirements. Preserve original governing component/universe/PSVI; names and BCL projections are not authority or complete transport.
- **FG-04/06:** define immutable semantic capture versus mutable legacy inputs and ownership. Init-only options do not make a referenced XmlSchemaSet immutable; a retained object does not extend a disposed snapshot's validity.
- **FG-07/08:** run actual source/package/unchanged-binary consumers. Signature equality and this metadata tool do not qualify a real provider or replace either owner's sign-off.

C45 stays Pending. No legacy member is removed/retyped, no provider routing/default/error behavior is changed, and no package is published. Full XSD 1.1 remains Schema's implementation goal; ordinary XmlDb XML stays independent of optional schema storage. API freeze remains pending until required contract gaps and actual dual-consumer qualification are resolved.

## Next concrete core work

Build an isolated compatibility harness that uses the existing schema/no-provider APIs, retains exact old caller binaries and checks baseline versus current outputs, defaults/errors and source signatures. Start with the five logical members above plus legacy validation/annotation paths; do not infer that old pins can be upgraded from compilation alone. Record exact artifacts and existing contract scope before executing or extending those cases. External consumer execution remains delegated to owners.
