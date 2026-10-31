# Count assertion pre-implementation adoption — 29 October 2026

Status: specification and finite grammar scope only. Count assertions are **not implemented**. Baseline is reviewed Schema 0.7 `a462022a852575f84f0cc504f86771deaa118e0e` and paired core `26ae9b06be97a8860d9f4b5089d41d70a5bd831f`. The user requested repetition review/checkpoint followed by explicit count adoption/planning. This does not authorize general schema/session acceptance, consumer adoption or release.

## Exact normative selection

| Dated edition/revision | Affected sections | Selection |
|---|---|---|
| [Functions and Operators Second Edition, REC-xpath-functions-20101214, revised 13 April 2015](https://www.w3.org/TR/2010/REC-xpath-functions-20101214/#func-count) | §15.4.1 fn:count | Cardinality returns one xs:integer, including zero for an empty argument. |
| [XPath 2.0 Second Edition, REC-xpath20-20101214](https://www.w3.org/TR/2010/REC-xpath20-20101214/) | §2.1.1; §2.4.2–2.4.3; §3.1.5; §3.5.1; §3.6 | Function QName/default namespace, call semantics, comparisons and Boolean/logical evaluation. |
| [XSD Structures, REC-xmlschema11-1-20120405](https://www.w3.org/TR/2012/REC-xmlschema11-1-20120405/) | §3.13.2; §3.13.4.1–3.13.4.2; §3.13.6.2 | Assertion compilation/context and Boolean result after native validation. |

The F&O selection intentionally includes the stated 2015 revision of the dated Second Edition; it is not represented as pristine 2010 text. XPath's restored-link/status notices do not change the selected language edition. [Source locks](COUNT-ASSERTION-SOURCES.json) separately record exact retrieved representation SHA-256 values and retrieval date. Normative edition/revision, representation witness, publication date, current review date and later fixture pins are distinct fields; no later unrecorded errata are adopted.

Count observes sequence length without requiring atomization of the selected nodes. In this finite domain the result lies between zero and sixteen. It differs from a bare absent-child value comparison, which is empty/false under Boolean conversion. Native structure and exact decimal assessment still precede every assertion; count must not make an invalid or unrepresentable child acceptable.

## Finite exposure and intentional deviations

Expose the standard function QName `{http://www.w3.org/2005/xpath-functions}count` only through unprefixed `count(DeclaredChildName)` in this first slice. Its default function namespace is the standard function namespace; `xpathDefaultNamespace="##local"` continues to govern element selection separately. Explicit fn:count, alternate prefixes/aliases and rebound function namespaces remain Unsupported pending assertion-namespace-binding coverage. This is a lexical coverage restriction, not a claim those calls are invalid XPath.

The sole argument is one declared no-namespace child name, selecting actual direct child nodes. Empty/multiple arguments, nested calls, wildcards, arbitrary sequences/literals, predicates, paths, variables and other functions remain Unsupported. Existing scalar decimal/name operands, eq/ne/lt/le/gt/ge, and/or and grouping remain. Keep the 512-character, 128-token and sixteen-parenthesis-level bounds; count-call parentheses contribute to depth. Other aggregates such as sum/avg and general comparisons/arithmetic are separate future adoptions.

Bare child names such as `count` retain name-selection semantics. Recognize a function only from its parsed call syntax; `count(count)` counts a declared child named count. Do not use substring scanning for admission or profile selection. Native occurrence/resource/value bounds, original state/copy/ownership, narrow XPTY0004 handling and ambiguous singular projection refusal remain unchanged.

## Proposed compatibility/version

Propose Schema 0.8 via MinVer and `decimal-sequence-assertions-v4` only for schemas containing an actually admitted count call. Existing schemas without that call retain v1/v2/v3, including a bare child named count. Core stays source-identical 1.34.1 if no correction is required. No new public overload or consumer pin change is planned.

One current repeated-suite refusal is deliberately scheduled for reclassification: `WiderAssertionGrammar_RemainsUnsupported("count(a) eq 2")` becomes a supported v4 case. Preserve its source/receipt and adopt that exact capability expectation before implementation; all other refusal expectations stay intact. No test is changed during this scope step.

See [implementation and acceptance plan](COUNT-ASSERTION-PLAN.md). Count admission, Schema 0.8 and profile v4 remain unimplemented by this record; full XSD 1.1 and general API/session acceptance remain separate.
