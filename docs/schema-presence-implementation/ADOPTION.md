# Presence assertion adoption and implementation plan — 29 October 2026

Status: adopted bounded semantic scope for the next implementation; implementation pending. Current supported Schema remains 0.8 / decimal-sequence-assertions-v4. Proposed feature version is 0.9 with profile v5; core stays 1.34.1 unless separately justified and qualified.

## Exact specification selection

Select [F&O 2.0 Second Edition](https://www.w3.org/TR/2010/REC-xpath-functions-20101214/), explicitly revised 13 April 2015, sections 15.1.4 (empty), 15.1.5 (exists), and 15.1.1 (boolean). `empty` returns a Boolean indicating an empty argument sequence; `exists` indicates a nonempty sequence. Child numeric value zero is still a present node. Both accept item sequences without atomization.

Also retain XPath 2.0 Second Edition (14 December 2010, link corrections 3 January 2011), sections 2.1.1, 2.4.3, 3.1.5, 3.5.1 and 3.6; XSD 1.1 Structures REC 5 April 2012, sections 3.13.2, 3.13.4.1–2 and 3.13.6.2. [Source locks](PRESENCE-SOURCES.json) distinguish normative revision from exact retrieved representation and retrieval date. This adoption precedes future production edits.

## Deliberately finite scope

Admit only unprefixed lowercase `exists(DeclaredChildName)` and `empty(DeclaredChildName)` as Boolean predicates, including standalone assertions and existing and/or/grouping. Retain existing decimal/name/count value comparisons. Presence predicates are not scalar comparison operands: Boolean comparisons, true/false/not calls, function aliases, prefixed calls, paths, predicates, nested calls, undeclared names and arbitrary expressions remain Unsupported.

Parse the entire expression before compiling the original XPath 2.0 expression. Record admitted count/presence features structurally; select v5 only for actual presence calls, v4 for count-only calls, and preserve v1–v3 otherwise. A call in an optimized-away branch still counts. Bare children named exists/empty remain ordinary operands; exists(exists) and empty(empty) work when declared, while count(exists) is count-only.

Keep 512 characters, 128 tokens and depth 16, counting function parentheses consistently with count. Keep the total occurrence budget 16. Native structure, lexical validity and exact decimal representability must complete before assertions, even when presence alone could return true. Preserve Invalid versus Unsupported and the narrow XPTY0004 treatment. Keep owner/state/declaration identities, typed export, source mutation guards and singular projection refusal.

## Implementation and acceptance order

1. Capture baseline failures and original test expectations before production edits. Audit the exists(a) refusal in OptionalDecimalAssessmentTests and CountAssertionAssessmentTests; migrate only their presence expectations, retaining the optional enclosing-group refusal.
2. Extend Boolean-primary admission with explicit feature metadata. Compile and evaluate original expressions through the existing engine; do not implement a second predicate evaluator.
3. Cover absent, one, two and sixteen instances; zero-valued children; mixed presence/count/value predicates; declaration-name collisions; complete admission across short-circuited branches; all syntax budgets and refusals. Verify native malformed/unsupported values still fail before presence, and preserve all earlier profile choices.
4. Exercise real XSLT/XQuery preserved/stripped copies, original owner/declarations, independent occurrence states and edits. Run the full Schema build/tests and example; reconcile exact expectation deltas. Inherit core gates only after proving core source identity; any core semantic change requires fresh solution/gates and its own adoption evidence.
5. Update canonical documentation and evidence, review, then checkpoint. No broader XSD 1.1 conformance or public API acceptance is implied.
