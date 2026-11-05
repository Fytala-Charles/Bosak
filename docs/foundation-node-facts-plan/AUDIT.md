# Node-fact route audit

The original input remains the neutral owner. `ISchemaAssessmentInput.OwnsNode` checks membership without reading a purported node's contents; the raw operation must establish exactly one admitted owner and that it is the selected snapshot. Only then may the engine inspect an annotated-node channel. A caller-supplied facts object requires selected-advisor issuance and the exact node/facts/original-input/origin tuple before its getters.

`SchemaAdvisorBinding.Issuer` implements `IQueryTypedContainerIssuer`, whose contract covers environment, input, origin, authority and container issuance. It has no node-facts capability. `QuerySchemaAssociation.ValidateNodeFacts` is an earlier private opaque-object proof route; public `OpenSnapshot` does not connect that association to the selected public advisor. Reusing its name does not establish a public implementation.

`SchemaQueryOperation.ReadTypedValue` authenticates an issued container, not a node association. It supports a single scalar for public identity/numeric evaluation; empty and multi-cell shapes remain Unsupported. That restriction must not erase list shape or establish nilled/no-value facts. The complete internal container reader preserves these distinctions and is the appropriate subordinate reader for TA-10.

The proposed new internal facts issuer must bind exact environment, original input/current projection, selected authority/advisor and original origin. Separate fact issuance and exact tuple checks remain necessary; matching handles alone prove neither. Ordinary source ownership does not depend on a Schema issuer. Public API shape and old descriptors stay unchanged in the first slice.

Use an uncached fact view, with no dictionary keyed by node or facts and no new quota interpretation. Existing scalar-cache limits count atomic values, not arbitrary fact objects. Measure repeated ownership/authority callbacks, allocation and latency before deciding whether any facts cache is justified. Retain original facts/authority; compatible-view remapping and source-free construction remain separate.
