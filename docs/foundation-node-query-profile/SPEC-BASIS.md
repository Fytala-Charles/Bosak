# Public original-node profile selection basis

Recorded 5 November 2026 at 6523c649 / source 1.61.1. This preparation records a bounded implementation selection, not activated semantics. Adopt the implementation subset again before source edits; no later drafts/errata are selected.

Normative review: [F&O 3.1 REC 21 March 2017, section 2.2](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/#func-nilled) specifies optional Boolean nilled results, including empty results for empty/non-element input. [XDM 3.1 REC 21 March 2017, section 5.8 and section 6.2.2](https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/#acc-nilled) provide the nilled accessor/element basis. The selected profile initially restricts syntax, capture and original-evidence capabilities; ordinary untyped-element inference and zero-argument focus semantics are excluded.

Retain TA-10's distinction between nilled, absent typed value and empty atomic results, and TA-11's ownership/lifetime guards. Future [F&O section 2.4](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/#func-data) atomization must preserve the absent-value error distinction. It is not activated by selecting a nilled profile. Metadata-only NotRequested is engine observation state, not a new XDM typed-value category.

Exact source ownership, issuer binding/tuple checks, malformed-provider policy, finite grammar, closed capture and refusal boundaries are Core host policy. Existing contracts and scoped R2 closure supply design entry evidence, not native execution acceptance. No new public signatures, complete XSD support, owner performance acceptance, API freeze or publication is implied.
