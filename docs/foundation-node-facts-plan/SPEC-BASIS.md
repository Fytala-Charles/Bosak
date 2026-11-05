# Original node facts: specification and policy basis

Preparation recorded 5 November 2026 against Core 2b7fa1ab / 1.60.0. This is an audit and implementation plan, not a new production semantic adoption. Before source edits, record the exact implementation base and adopted subset again.

Immutable normative references reviewed:

- [XDM 3.1, REC 21 March 2017](https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/), sections 3.3.1.2 (Typed Value Determination), 5.8 (nilled), 5.14 (typed-value), 6.2.2 (element accessors), 6.3.2 (attribute accessors). Original nilled status, absent typed values and empty atomic sequences are distinct; list/union member typing remains represented. The document does not define host issuer trust or ownership protocols.
- [Functions and Operators 3.1, REC 21 March 2017](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/), sections 2.2 (fn:nilled) and 2.4 (fn:data). Later public atomization must distinguish a node lacking a typed value (FOTY0012) from an empty typed result. Nilled queries require node-kind-sensitive optional Boolean behavior; a raw provider Boolean alone is insufficient for general fn:nilled.

No later draft/errata, rounded arithmetic, validation/assessment implementation or general public atomization is adopted here. Exact source membership, original tuple proof, guarded reads, lifetime checks, malformed-provider refusal and quotas are Core contract policy, not normative W3C trust mechanisms. Internal transient reading precedes any public profile; it must preserve original authority and cannot infer facts from cardinality or lexical annotations.
