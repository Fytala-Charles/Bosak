# Current XSD 1.1 coverage audit

25 November 2026. This is the active feature-boundary audit for the qualified Schema development source and Core calendar work. The 19 November matrix remains a historical baseline. [Machine-readable audit](CURRENT-MATRIX.json) records exact source revisions, immutable evidence hashes, normative section references and remaining exclusions.

The review covers 24 areas. It does not enumerate every normative requirement or certify complete coverage of any whole area. There is no defensible completion percentage. Section references identify the rules being reviewed; linked feature dossiers retain their original adoption and case qualification. This review infers no new feature, corpus or retained-binary qualification. Fresh repository regression and documentation checks are reported separately in VALIDATION.json.

| Area | Qualified development scope | Still outside that scope |
|---|---|---|
| Expanded names and schema target namespaces | Namespace-aware expanded names, target namespaces and local qualified/unqualified forms. | Chameleon assembly and unrestricted namespace/schema composition. |
| Element declarations and ordered decimal content | Finite scalar/simple-content and structural element declarations with original identity. | General recursive graphs, arbitrary content and implicit anyType breadth. |
| Schemas without assertions | Assertions may be absent in admitted schema shapes. | This does not admit arbitrary schemas without assertions. |
| Attribute declarations and uses | Closed local/global attributes, optional/required uses and acyclic attribute groups. | Attribute wildcards, full group recursion semantics and automatic missing-attribute insertion. |
| Named types, component references and derivation | Named simple/complex types, selected derivation chains and shared component references. | General complex derivation, arbitrary extension/restriction combinations and recursive models. |
| Choice, all, nested models and occurrence breadth | Finite sequence/choice/all models, occurrence bounds, named sequence groups and sequence mixed content. | Full recursive model language; mixed all/choice/empty/derivation and mixed-plus-assertion combinations. |
| Built-in datatype families beyond decimal | String, Boolean, decimal, integer families and date in admitted placements. | Native dateTime/time/dateTimeStamp, duration, floating/binary/QName/Gregorian-fragment breadth. Core exact calendar prerequisites are merged but unpublished. |
| Exact decimal lexical/value mapping | Exact integer/decimal lexical/value handling under explicit representation capacities. | Unbounded precision or every legacy XPath numeric route. |
| Restriction facets, normalization and validation | Selected length/pattern/enumeration/range/digit/whitespace/timezone facets by datatype and placement. | Full Unicode/XSD regex grammar, all fixed facets and arbitrary facet/type combinations. |
| Lists and unions | No native list/union admission is qualified by these production receipts. | Lists/unions and member-selection semantics remain follow-on work. |
| Default, fixed, nil and abstract elements | Simple nillability/empty typed values and constraints on provided default/fixed values. | Automatic default augmentation, abstract/substitution breadth, complex nil and excluded assertion combinations. |
| Include/import and component assembly | Closed same-namespace includes and named-simple-type imports with shared identities. | Chameleon schemas, foreign global elements/complex owners/attributes and unrestricted assembly. |
| Override/redefine and version controls | No broad native override/redefine/version-control implementation is qualified. | Override/redefine and associated component/version rules. |
| Complex-type assertion semantics and context | Finite assertions on admitted scalar/structural owners with explicit namespace/context rules. | General assertion XPath, arbitrary traversal/functions and excluded mixed/nil/attribute combinations. |
| Simple-type assertion facets | Legacy assertion-facet helpers are not native admission evidence. | General native simple-type assertion facets. |
| Conditional type assignment | Legacy alternative/applicability helpers are not native structural validation. | General native conditional type assignment. |
| Open content and wildcard structural enforcement | Legacy descriptor metadata does not enforce native structural open content. | Native open content and wildcard structural enforcement. |
| Wildcard negative constraints and matching | Recorded wildcard descriptors do not prove native negative-constraint matching. | Native wildcard negative constraints and validation matching. |
| Default attribute application | Provided attribute constraints are supported elsewhere; automatic default attribute application is separate. | Default attribute groups/application and missing-attribute augmentation. |
| Identity constraints and substitution groups | No native identity-constraint/substitution-group coverage is qualified here. | key, keyref, unique and general substitution groups. |
| PSVI and typed atomization transport | Actual declaration/governing/atomic facts and original-node atomization for admitted shapes, with expiry guards. | Full PSVI, augmented trees, general public Core C45 acceptance and cross-universe compatibility. |
| Schema-aware XPath/XQuery/XSLT application use | Bounded native original-node consumers and explicit guarded typed query profiles. | General schema-aware XPath/XQuery/XSLT validation integration and arbitrary typed queries. |
| Conformance, defined limits and unsupported behavior | Explicit Invalid/Unsupported and finite selected corpus outcomes; no full normative denominator. | Exhaustive clause-to-case inventory and complete W3C conformance qualification. |
| Host acquisition, cancellation, cleanup and reproducibility | Owned acquisition, cancellation, source identity, invalid-fact refusal and deterministic release boundaries. | General performance/total-memory SLAs and unrestricted provider acceptance. |

## Evidence and availability

Schema source qualification records 10,248 cases and 8,778 public package cases, including refusal/lifecycle checks and repeated facade/context variations. These are not counts of specification clauses. The retained pilot of 21 cases pilot matches all expected outcomes; the expanded sample of 129 cases records 19 matches and 110 Unsupported with zero mismatches/harness errors. A match includes the expected rejection of invalid input. These selected cases are not a W3C-wide denominator.

Core 1.86.0 adds bounded exact calendar components, atomic transport, explicit comparisons/interoperability and guarded typed comparison queries. Its latest source qualification records 26,750 solution and 73 public package cases. These Core tests do not confer native Schema datatype admission. Both development snapshots are unpublished; Schema still pins signed Core 1.81.17. The last verified Schema beta remains the bounded decimal release.

## Next sequence

1. Prepare/review the exact Core release candidate and preserve its source/artifact identities.
2. Obtain explicit exact-artifact publication approval and verify the signed registry payloads; preparation does not authorize publication.
3. Separately adopt and qualify bounded native Schema dateTime/time/dateTimeStamp support against that actual signed pin, including facets, provenance, detached API, original-node facts and lifetime.
4. Continue broader datatype/regex, identity-constraint, conditional-type/wildcard, augmentation and assembly work with clause-specific evidence. General assertions and full PSVI/C45 remain separate.

All prior matrices, dossiers, adoption records and original receipts remain unchanged. No product API, grammar, runtime, signed pin, tag or registry publication changes in this audit.
