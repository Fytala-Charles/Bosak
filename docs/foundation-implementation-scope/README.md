# Proposed C139-01 implementation scope — 30 October 2026

The smallest justified next production change is not yet a signature list. The registered requirements support two independent tracks: qualify ordinary XmlDb XML behavior through existing APIs, and design the missing optional Schema authority/session route. This proposal separates them so untyped XML is not forced through a Schema integration.

[REQUIREMENTS.json](REQUIREMENTS.json) maps twelve concrete needs to registered work packages, current source boundaries and the [six-group contract matrix](../foundation-contract-decision/README.md). It pins the inspected Core source at `6ab944d9`; owner registration evidence remains in the preceding packet. Registered work is a requirement input, not an owner acceptance response or proof of a defect.

## Proposed implementation order

1. **Retain the existing XML provider contract.** No new required `IXdmNode` or `IXdmSequence` member is justified by the registered XmlDb requirements. Qualify axes, kind values, atomization, snapshot identity/order and supported binary replacement with the actual provider. Nested/deferred values and cancellation need explicit lifetime cases. A discovered failure gets its own reproduction and bounded fix; this review does not invent a provider defect.
2. **Settle authority and annotation shape before routing.** For Schema, identify the provider-owned universe/component/source identities and original PSVI facts actually needed by queries. Prefer an optional neutral annotation channel. The current Providers `XdmElementAssessment` carries opaque Owner/State for anonymous element-only and decimal assessments; it is neither a general session nor an atomic/list/union provenance solution. Its trusted attachment does not validate XML.
3. **Select the smallest session operations from actual use sites.** Request exact lookup, derivation, affiliation and descriptor operations from the Schema provider mapping. Choose error/refusal outcomes, cancellation and immutable semantic capture together. Preserve legacy BCL defaults and use an explicit native route. Keep complete schema models, assertion execution and structural validation in Schema.
4. **Bind compatibility and value transport end to end.** Qualify directional compatible-session authority without replacing original PSVI. Audit atomic extraction, casts, equality/hash, map/array transport, copy/strip and serialization before choosing a carrier representation. Existing `XdmValue` layout and required provider members remain compatibility constraints. A reference field or synthetic carrier example is not an ABI/lifetime proof.
5. **Implement and qualify only a recorded scope.** Record immutable normative revisions, sections and deviations before semantics change. Then add happy/error/boundary tests, complete solution tests, exact frozen gates and actual consumer source/package/binary evidence on the resulting candidate. Shared freeze still requires all known foundation gaps and owner dispositions, even if backend implementation continues afterward.

These are dependency stages, not five independently accepted API releases. Do not freeze an early bounded stage as sufficient for the full XSD 1.1/XmlDb foundation goal.

## Finite decisions needed from the existing owner work

| Owner work | Concrete information needed to select signatures |
|---|---|
| Schema S009-02 / S009-04 | Actual operation call sites; provider component/universe identity model; original PSVI and list/union values; derivation/affiliation facts; compatible-session authority and lifetime. Identify requirements for the full intended validator, beyond the current decimal bridge. |
| XmlDb FX009-01 / FX009-02 | Exact existing node/sequence use sites and any missing operation; nested/deferred result lease rules; snapshot identity/order boundaries; supported source/binary replacement modes. Schema-aware persistence is separate. |
| Core C139-01 | Choose exact retained/rejected signatures from those needs, immutable semantic adoption, supported error/profile/ownership contract and the bounded implementation scope. |

Public identity-issuer factories, generated record convenience members and atomic attachment helpers remain prototype-only unless a concrete consumer requirement justifies them. No new mandatory disposal interface, storage format, concrete Schema dependency or Core validator is proposed.

## Current disposition and verification

All accepted-signature lists are empty. This document proposes scope and ordering; it does not record C45 acceptance, authorize the general implementation, freeze an API or publish packages. Braid D02 validation is not a prerequisite for this design work. Missing owner requirements cannot be inferred from passing Core tests.

Core source is unchanged. The source hashes and requirement IDs were checked, the preceding matrix/readiness verifiers passed, and canonical documentation links were rendered and checked. Existing test counts remain historical qualified evidence, not fresh runs for this documentation step. The next useful action is an exact owner requirement return or a concrete reproduced Core defect; another inventory alone will not close C139-01.
