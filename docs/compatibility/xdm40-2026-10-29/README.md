# XDM 4.0 cross-system compatibility review

29 October 2026. Review base: `4e523082dea931d9377f34672ff63c57eb5dbd7b`. C44 remains complete at `affc228a`; C45, REQ-139 and the permanent schema-session API remain pending.

The recommended direction is an **additive schema integration with explicit compatibility between component universes**, followed by real consumer qualification. This review establishes no need to replace `IXdmNode`, renumber XML node kinds, expand `XdmValue`, migrate XmlDb storage or merge record types into the schema provider. C44's session token remains useful for provenance, but token equality alone cannot implement the draft's compatible-schema interchange requirement.

## Findings that change the proposal

1. **Separate provenance from schema compatibility.** XDM §4.1.4 requires nodes assessed against one schema to be usable with another compatible schema. C44's shared-session reuse covers one route, but its rejection of every foreign session leaves compatible independently compiled universes uncovered. A provider/host must establish compatibility and retain original assessment provenance; QName equality or revalidation is insufficient.
2. **Split four identities explicitly.** Query/XDM profile, schema language version, schema-component authority and source/storage identity have different lifetimes and meanings. C44's free-form `Language` and static-context digest should become explicitly specified inputs before signatures are accepted. Schema already has an explicit `XsdLanguageVersion`.
3. **Treat current 4.0 support as partial.** Fresh probes show working JNode navigation and generalized `compare`, alongside gaps in JNode comparison, simple record matching, map reinsertion order, binary map-key equality and numeric/duration operators. Existing classes and green frozen gates do not establish support for the new review pin.
4. **Preserve consumer ownership.** XmlDb owns snapshot leases and persistent identities; Braid owns inspection identities, byte extents, freshness and edit authority; Schema owns XSD 1.1 processing and licensing. A schema session must not take over those responsibilities.

See the [specification and capability review](SPEC-REVIEW.md), [consumer compatibility/acceptance matrix](CONSUMER-MATRIX.md) and [revised candidate and next slice](NEXT-SLICE.md).

## Exact inputs

| Repository | Inspected HEAD | Effective Bosak dependency |
|---|---|---|
| Bosak | `4e523082dea931d9377f34672ff63c57eb5dbd7b` | Candidate 1.33.1 source; public 1.26.3 |
| XmlDb (`Bosal.XmlDb` directory) | `4598346e324a4385381aa31a68d565bcdee9c479` | Exact `[1.26.2]` NuGet reference in Xdm; independent Storage/Core |
| Bosak.Schema | `fa0da3e2d9972706a5a86616ef285af4daf5037b` | 1.26.3 package references in Schema; no package upgrade performed |
| Braid | `0f1d949e758645f7944f617f2b4613e49161a195` | Project references to clean immutable checkout `6a3372fc3b91e733b71a426cf40f26feaaeb3bcb` (1.26.2); separate signed-public-package adoption receipts |

The [repository manifest](evidence/repositories.json) records five revision/status observations, including Braid's actual dependency checkout, and hashes 58 selected source/contract files. All three sibling working trees and Braid's pinned engine checkout were clean at capture. During final validation, independent XmlDb documentation edits appeared, including its handover and a post-EP6 readiness review; its HEAD and inspected production source remained unchanged. The [final checks](evidence/documentation-checks.json) retain both the original hash and later handover hash/status rather than overwriting the initial evidence. No sibling source was copied into core, written, built, activated or executed by this review. Git ownership exceptions were scoped to individual read commands; global configuration was unchanged.

The specification **review pin** is qt4cg/qtspecs [`3e9f3bddd38f39f8953750306f0c574a2fbd0387`](https://github.com/qt4cg/qtspecs/commit/3e9f3bddd38f39f8953750306f0c574a2fbd0387), committed 28 October 2026. The mutable rendered page displayed 22 October 2026. These are distinct identities; equivalence of that generated HTML and this later source tree is not claimed. No semantic adoption has occurred. [Pin/hash receipt](evidence/spec-pin.json), [original commit metadata](evidence/spec-commit.json), [XDM source](evidence/xpath-datamodel.xml) and [upstream license](evidence/SPEC-LICENSE.md) are retained. Supporting XPath/F&O files have immutable retrieval URLs and byte/blob hashes.

## Fresh diagnostics and limits

The review-only helper evaluated **31 expressions in two profiles: 62 observations** against existing Release assemblies. All seven loaded Bosak assemblies match C42's preserved SHA-256 binary receipt. It built only an isolated helper, with assembly references, and did not rebuild product projects. The [observations](evidence/observations.json) include both returned values and exceptions; **62 is not a passing-test count**. Three successive diagnostic runs contain 50, 56 and 62 observations. Initial sequence display was made explicit for the control-character probe, and construction/cross-document/function controls were added; earlier receipts are retained separately.

The [verification receipt](evidence/verification.json) distinguishes fresh observations from inherited **24,203 solution passes**, QT3 **31,397/0/424**, qt4 **8,941/0/1,918**, 230 gated sets and 11 experimental passes. Product/test source, package pins, versions, fixtures, filters, gate membership and skip reasons are unchanged. Complete engine suites were not rerun for this review. Actual consumer source rebuilds, unchanged-binary replacement tests, provider qualification and native Braid acceptance remain future gates, not inferred from source inspection or C44's synthetic ABI experiment.

## Delivery boundary

This is a source-backed review and revised proposal. Preserve C41–C44 evidence, existing conformance exclusions, REQ-126's bounded completion and all 48 historical observations. Bosak.Schema retains XSD 1.1 ownership. Candidate 1.33.1 remains unpublished and public 1.26.3 remains current. No production API acceptance, consumer adoption, release, upstream contribution or merge is included.
