# TA-18 source-free construction selection basis

5 November 2026. Source remains **1.64.0**, ec6daa0e plus the exact uncommitted TA-17 manifest pinned in INPUTS.json. This finite Core selection precedes implementation. Record actual implementation revision/dirty source hashes and this subset again before source edits.

| Immutable normative edition | Sections | Selected relationship |
|---|---|---|
| [F&O 3.1 Recommendation 21 March 2017](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/) | 18.1 built-in atomic constructors, 18.5 user-defined constructors, 19.2 string/untypedAtomic conversion | Provider-owned lexical construction produces a value of the requested target type; this host entry is not general constructor-function syntax. |
| [XPath 3.1 Recommendation 21 March 2017](https://www.w3.org/TR/2017/REC-xpath-31-20170321/#id-cast) | 3.14.2 cast | Distinguish target-typed construction from preserving an input assessment claim; do not activate general cast/implicit atomization. |
| [XDM 3.1 Recommendation 21 March 2017](https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/) | 2.7.1 type representation, 2.7.4 type system, 2.7.5 atomic values | Current atomic type evidence and payload are distinct from original document/assessment provenance. |

Only these dated editions are selected; no latest draft/errata or full native XSD backend adoption. Retrieval on 5 November is a later review, not a specification revision. Provider XSD language/coverage remains separate from XPath/XDM version.

First-profile restrictions: programmatic ConstructSimpleValue accepts plain unmarked lexical String input; Supported output must be exactly one non-list plain String/Boolean/Integer/Decimal atomic cell with fresh type evidence and null original assessment. Provider handles actual lexical/facet semantics and exact numeric construction; unsupported coverage/precision refuses. Date/QName/namespace-sensitive, other atomic families, list/empty/multi-result construction, source-marked inputs, arbitrary function syntax, arithmetic, persistent/cross-operation/cross-session constructed evidence and all-host integration remain excluded. Known wider list/union/retained-value requirements are not closed by this first profile.

Core host policy, not normative W3C resource machinery: exact reserved $constructed consumer/profile, a real borrowed-advisor IQueryOperationLifetime admission, raw-owned in-flight call/drain tracking, immutable episode capture, engine-issued operation-local construction receipts, identity checks and quota/error boundaries in POLICY.md. Do not pass null/fictional ISchemaAssessmentInput to OwnsTypedValue, change its non-null contract, invent a source snapshot, reinterpret snapshot scalar quotas, or grant authority through descriptive names/fingerprints. No new public signature is selected for this bounded optional-capability profile; broader retained construction/authentication needs remain open before freeze.
