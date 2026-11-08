# K04-C42 — Actual schema-element membership and subtyping

29 October 2026. Existing-contract correction based on **2e364d66**, adopted before implementation in [IMPLEMENTATION-PIN.json](evidence/IMPLEMENTATION-PIN.json). Candidate **1.33.1** is unpublished; public release remains **1.26.3**.

`schema-element(N)` now uses the global declaration for the candidate node's name, rejects abstract candidates, and checks actual substitution membership with element substitution/derivation blocking and head/intermediate complex-type blocking. The candidate annotation must derive from the candidate declaration's type, including when nilled; nillability belongs to that candidate declaration. Concrete members still match abstract heads, and an intermediate element's substitution block applies to its own group rather than every ancestor group.

Schema-element sequence subtyping uses containment of actual member sets through the same membership helper. Existing function parameter contravariance and occurrence rules consume this relation. A head's affiliation alone is insufficient: a parent can block extension while an affiliated head admits an extended descendant. The implementation enumerates the current compiled global components without caching mutable schema sets. No public API is added.

The normative pins are [XPath 3.1 REC-20170321 §§2.5.5.4/2.5.6](https://www.w3.org/TR/2017/REC-xpath-31-20170321/#id-schema-element-test) and [XSD Structures REC-20041028 §3.3.6](https://www.w3.org/TR/2004/REC-xmlschema-1-20041028/#cos-equiv-derived-ok-rec), applied to the existing compiled single-affiliation XSD 1.0 component path. This is a correction of that path, not acceptance of the proposed XSD 1.1 advisor. The existing anonymous-type fallback is retained; no new provider annotation contract or multi-affiliation representation is introduced.

## Verification

| Evidence | Before | After |
|---|---|---|
| Final 39 public API regression tests | 22 pass / 17 fail on preserved 1.33.0 binaries | **39 pass** |
| C41's unchanged ten validated-node controls | Six pass / four mismatches | **All ten pass** |
| C41's unchanged independent function control | One mismatch | **Pass** |
| 37 unchanged older-profile originals | 15 pass / 22 fail directly | **19 pass / 18 fail directly** |
| Nine unchanged exact-query current-profile companions | Nine pass | **Nine pass** |
| Six unchanged multi-affiliation preparation cases | Six XmlSchemaException outcomes | **Same six preparation exceptions** |
| Complete solution | 24,164 prior tests | **24,203 pass / ten projects** |
| Complete normal QT3 | 31,397 / 0 / 424 | **31,397 / 0 / 424** |
| Complete normal qt4 gated run | 8,941 / 0 / 1,918 | **8,941 / 0 / 1,918** |

Only **substitution-003, -010, -011 and -030** change their direct original result, all from failed to passed. The other 48 original outcomes remain unchanged. All expressions, assertions, dependencies, environments, modules and resources are replayed without rewriting fixtures. Both complete normal skip maps, all 230 qt4 gated sets and 11 experimental passes are unchanged. The 37 legacy-profile exclusions and six distinct schema-preparation errors remain excluded for their original reasons; passing a counter-profile diagnostic does not authorize admission.

The 39 self-contained unit rows cover abstract candidates versus abstract heads, transitive membership, element substitution/extension/restriction blocks, intermediate type blocks, local/global declaration collisions, candidate typing including nilled values, positive restrictions, nilled concrete members, untyped nodes, namespace aliases, named and inline function signatures, both subtype directions and occurrence boundaries. Source XML and schema metadata identity are preserved. The initial 35-row run is retained separately; four additional edge controls expand the final cohort to 39. The copied unchanged QT3 XSD is embedded in the unit test so normal unit execution has no catalog checkout dependency.

All 109 final diagnostic/gate processes are externally bounded and complete without timeout. Assertion/preparation outcomes are checked separately from process exits. Build and full solution logs are fresh; build succeeds with six existing warnings, including unavailable NuGet audit metadata. No new warnings are introduced by the repair. HTML render receipts cover ten pages with unchanged Fytala CSS; no screenshot/PDF acceptance is claimed. Run `python docs/conformance/k04-schema-element-fix/evidence/validate-evidence.py` for scope, identities, outcome deltas and baseline checks.

## Boundaries and next work

**Bosak.Schema owns XSD 1.1**. The framework still rejects the unchanged multi-affiliation resource during XmlSchemaSet.Add; all six original assertions remain unexecuted past that preparation boundary. ADR-006 stays Proposed, REQ-139 stays Pending with its acceptance items unchanged, and the separately documented XSLT cohort is not certified here. C41's original receipts and current-schema documentation snapshots remain immutable. REQ-126 stays Complete under its bounded acceptance; all 48 historical observations remain preserved.

Next: [C43 concrete schema-advisor compatibility proposal](NEXT-SLICE.md), refreshing the actual seam inventory and no-advisor baseline before any owner decision. No filter, GatedSets, baseline, fixture/assertion, sibling, package dependency, release, upstream or Braid action is included in C42.
