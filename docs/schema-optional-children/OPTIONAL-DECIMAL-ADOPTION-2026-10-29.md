# Optional decimal children: pre-implementation adoption — 29 October 2026

Status: specification selection and bounded implementation scope recorded; optional-child semantics are **not implemented** by this step. Core base `a163ff11895038ceb2b718ecfba77383384a3b35`; Schema base `766eff7a77ee08e9c38cf479994e94eaf232d242`. The user requested the next step described as scoping and recording specification adoption. This does not imply general API/session acceptance, package publication or an engine baseline change.

## Exact normative revisions

| Immutable edition | Affected sections | Selected behavior |
|---|---|---|
| [XSD 1.1 Structures, REC-xmlschema11-1-20120405](https://www.w3.org/TR/2012/REC-xmlschema11-1-20120405/) | §3.3.2, §3.8.4.1, §3.9.1–§3.9.4, §3.13.4.1 clause 3 and §3.13.4.2 | Local particle occurrence bounds; ordered matching; assertion-result Boolean conversion. |
| [XPath 2.0 Second Edition, REC-xpath20-20101214](https://www.w3.org/TR/2010/REC-xpath20-20101214/) | §2.4.2–§2.4.3, §3.5.1, §3.6 | Atomization, effective Boolean value, value comparisons and logical combinations. |
| [XDM 3.1, REC-xpath-datamodel-31-20170321](https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/) | §6.2, especially typed-value and construction properties | Preserve actual present nodes, original types and absent root typed value. |

These dated Recommendation identifiers are the adoption revisions, not moving latest-version URLs. Review date is 29 October 2026. Later status notices, errata links, fixture revisions and feature-origin commits do not replace the adoption revision; no later errata are separately adopted here. The earlier whole-document bridge's XQuery/XSLT copy adoption remains unchanged.

A zero-minimum particle allows omission while sequence order remains constrained. An absent operand makes a value comparison empty; Boolean conversion then yields false. An absent element is not a present nilled element, a zero decimal or a node whose typed value is missing. These rules are selected for the finite extension; the implementation design below is an engineering choice.

## Deliberately finite coverage

Only local decimal particles may gain `minOccurs="0"`; the enclosing sequence still occurs once and local `maxOccurs` remains absent or one. Retain the existing 1–16 unique declarations, 1–8 assertions, namespace-free structure, exact decimal representation and resource/input bounds. No repetitions, choices, defaults, fixed values, nil, attributes, imports, type alternatives, new functions or broader assertion grammar are added.

For occurrence lexical forms, retain the existing trimmed, unsigned, zero-padded spelling of one and add the corresponding zero spelling for local minimum only. Examples: `0`, `00`, XML-whitespace-surrounded `0`. Signed value-equivalent forms such as `+0` and `-0` remain explicitly Unsupported, not invalid XSD. This is a finite lexical coverage restriction, not a claim that those forms violate the standard. Unknown/unsupported occurrence constructs are never ignored or sent through a stripped XSD 1.0 fallback.

## Proposed API and version disposition

Occurrence belongs to a compiled particle, not to `AssessedElementDeclaration`. Keep public declaration/type identity unchanged and introduce an internal immutable particle binding if needed. `ChildDeclarations` keeps all declarations in schema order. `AssessedDocument.Elements` keeps the root plus **present** assessed children in instance order, retaining each governing declaration object. No placeholders or fabricated optional-node states.

Keep the existing non-null `CreateTypedElementCopy` API: an absent child has no element copy and raises its documented BSK-XSD-016 refusal. It must not synthesize XML or return another child's value. Whole-document exports and native copies preserve only actual nodes and original states. Root atomization remains FOTY0012 even when every local child is optional and absent.

Proposed optional-schema profile identifier: `decimal-sequence-assertions-v2`; required-only schemas retain `decimal-sequence-assertions-v1`. The next Schema implementation feature advances the unpublished development minimum from 0.5 to 0.6 via MinVer, without a tag. The all-optional-absent case exposes an empty element-only root whitespace-binding gap in core; the implementation therefore includes a bounded core fix and proposes patch candidate 1.34.1, with Schema candidate dependency references updated accordingly. This planning step changes neither version nor profile behavior.

See [implementation and verification plan](OPTIONAL-DECIMAL-PLAN.md). Full schema conformance, independent compatible universes, consumer adoption and general public API freeze remain separate.
