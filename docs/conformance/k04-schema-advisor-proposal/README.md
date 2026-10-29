# K04-C43 — Schema advisor compatibility review

29 October 2026. Read-only source base **ebfb7c2f**. [The concrete counterproposal](COMPATIBILITY-PROPOSAL.md) defines optional schema sessions, explicit coverage verdicts, declaration/type relations, node **and atomic** identity, source ownership, lifetime and separate licensed integration. **ADR-006 is not accepted and no public API is frozen.** Next: [C44 isolated contract prototype](NEXT-SLICE.md), resolving identity/layout and host mapping before the owner decision.

Current Bosak.Schema HEAD **228c9bea101760bce480b1f011998f887217097c** and six working-tree design documents are pinned with reviewed product-source hashes. Staged assertion/alternative selection is present. The public ElementDeclaration model records explicit name/type pairs, omits typeless/ref declarations from that list, and exposes no abstract/nillability/block/affiliation-list component fields. Complete complex derivation and inherited assertion applicability are not established. Root-document digesting alone is insufficient for the proposed closure identity. Commercial source is not copied, built or executed; no activation or sibling mutation occurs. Package references remain core 1.26.3; this is not activated-provider compatibility evidence for unpublished candidate 1.33.1.

| Bounded current evidence | Outcome |
|---|---|
| Six unchanged QT3 multi-affiliation originals | Six schema-preparation XmlSchemaException outcomes reproduced |
| 30 documented XSLT cases | All 30 retain explicit XSD 1.1 guards |
| Additional CTA witness validation-1301 | Reaches inline xs:alternative guard; historical “unreachable” claim is stale |
| Independent catalog-005b control | Selected and skipped before an emitted XSD reason; negative XML_1.1 applicability remains separate |
| Complete normal QT3 | **31,397 / 0 / 424**, exact skip map unchanged |
| Complete normal qt4 gated run | **8,941 / 0 / 1,918**, exact skip map unchanged, 230 sets / 11 experimental passes |
| Full solution / C42 regressions | **24,203 / 39 passes inherited**, with product source/binaries unchanged |

There are **40 fresh bounded processes**: 32 XSLT requests, six QT3 requests and two gates. XSLT's test-name selector uses substring matching, so stream-101 requests also select non-stream-101 and catalog-005 also selects catalog-005b. Receipts retain all observations and selected/unselected totals; targets are reconciled by exact names, not by process success or global skipped totals. The documented cohort remains 30; the additional CTA witness and independent applicability control are explicitly separate. This is not a full XSLT skip census or a provider-registered conformance run.

The XSLT checkout is not globally clean: five unrelated tracked paths and generated artifacts are present. All 34 reviewed resources retain their pre-replay byte hashes and match the pinned HEAD content after CRLF normalization. Full-checkout status was captured after replay; no initial clean-state claim or unrelated cleanup is made.

The proposal corrects two unsafe simplifications: a typing callback alone cannot supply 1.1 component relations/atomic provenance, and an advisor-owned unsupported schema must not silently fall back to a 1.0 success. It also distinguishes existing XmlSchemaSet members, the stream-based IXsdValidator facade and QName-based node/value annotations. [Fourteen acceptance groups](ACCEPTANCE-MATRIX.md) separate current baseline receipts from planned provider requirements. Names and exact layout/signatures remain subject to C44 qualification and later owner acceptance.

REQ-139 stays Pending with original unchecked acceptance items and historical baseline text preserved; Bosak.Schema owns XSD 1.1. REQ-126 remains Complete under bounded acceptance with all 48 historical unmet observations. C42's original evidence is immutable. No product source, public API, dependency filter, GatedSets, baseline, fixture/assertion, package version/dependency, sibling, release, upstream or Braid action changes. Candidate **1.33.1** remains unpublished/public **1.26.3**.

Run `python docs/conformance/k04-schema-advisor-proposal/evidence/validate-evidence.py` to verify scope, source/document identities, cohort mappings, receipts and exact gates. Twelve HTML renders use unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.
