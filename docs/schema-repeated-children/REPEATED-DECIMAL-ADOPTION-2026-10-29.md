# Bounded repeated decimals: pre-implementation adoption — 29 October 2026

Status: future implementation scope only. Repetitions are **not implemented**. Baseline is the reviewed optional-child checkpoint: Schema `773fd69faa9ff7a5bb27f1c4f8469f3024efc043`, core `5e277c6a8233dc63af350c4485cc87609c132643`. The user requested continuation of the review/checkpoint recommendations and preparation of the next repeated-child scope. This record does not freeze a general schema/session API or authorize release/admission changes.

## Exact normative revisions

| Dated immutable edition | Affected sections | Adoption selection |
|---|---|---|
| [XSD Structures REC-xmlschema11-1-20120405](https://www.w3.org/TR/2012/REC-xmlschema11-1-20120405/) | §3.3.2; §3.8.4.1; §3.9.1–3.9.4.3, especially §3.9.4.1.1; §3.9.6.1; §3.13.4.1–3.13.4.2 | Finite local occurrence bounds, ordered repetitions and assertion validity/error behavior. |
| [XPath 2.0 Second Edition REC-xpath20-20101214](https://www.w3.org/TR/2010/REC-xpath20-20101214/) | §2.4.2–2.4.3; §3.5.1; §3.6 | Empty/single/multiple atomized operands, value comparison type errors, Boolean/logical evaluation. |
| [XDM REC-xpath-datamodel-31-20170321](https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/) | §6.2 and its node construction/type/typed-value properties | Distinct actual element nodes/states may share one original declaration/type. |

The adoption revisions are these dated editions, not moving latest URLs, later status notices, publication dates, fixture pins or feature-origin commits. Review date is 29 October 2026; no later errata are separately adopted. Original optional-child and copy adoption records remain separate and unchanged.

Repeated occurrences remain ordered instances attributed to a particle's declaration. A value comparison atomizing more than one value raises XPTY0004; it must not silently take the first value or become an existential comparison. Assertion type/dynamic failure does not establish validity. The bounded plan below selects a narrow schema-side error policy for the existing closed grammar.

## Intentional finite restrictions

Admit local decimal minimum zero or one, maximum 1–16, with the sum of local maxima at most sixteen. The enclosing sequence still occurs once; declaration names remain unique. Keep unsigned, zero-padded, XML-whitespace-trimmed occurrence spellings. Signed spellings, minimum greater than one, zero/unbounded/greater-than-sixteen maximum and excessive declared occurrence budget remain Unsupported even where valid XSD can express them. These are coverage restrictions, not invalid-XSD claims.

No choice/nesting/reused names, imports, defaults/fixed/nil, attribute model, new assertion functions/operators or external resources. Existing schema/instance size and exact-decimal bounds remain. Assertion expressions keep the same decimal/name value-comparison and and/or grammar; count, sum, exists, predicates and general comparison operators are deferred.

## Proposed representation and compatibility

Use internal particle minimum/maximum values; preserve public original declaration/type identity. All repeated instance states are distinct, ordered and retain the same original governing declaration object. Do not deduplicate states by name or declaration. ChildDeclarations remains the schema declaration list; Elements remains root plus every present instance state.

Keep singular `CreateTypedElementCopy(name)` for exactly one present match. Zero or multiple matches must refuse explicitly with BSK-XSD-016; never select the first repetition. Whole-document exports and host XPath queries provide occurrence-specific access without adding a new public overload in this slice.

Propose `decimal-sequence-assertions-v3` only when some local maximum exceeds one. Preserve v1/v2 for their existing schemas. Future Schema feature minimum: 0.7 via MinVer, without a tag; current 0.6 is unchanged by this record. Keep core 1.34.1 if no core production change is required. Any discovered core change requires its own pre-edit disposition/version and full core/gate qualification.

See the [implementation/verification plan](REPEATED-DECIMAL-PLAN.md). Full XSD 1.1, general session interoperability, public API freeze and consumer adoption remain separate.
