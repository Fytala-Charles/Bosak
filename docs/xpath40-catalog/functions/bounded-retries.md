<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: bounded invoice submission retries">
  <br><br><h1>Bounded Retry State</h1>
  <p>Frozen 4.0 — precondition and postcondition loops with an attempt budget</p>
</div>

**C09 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-1.md) records 4 contract groups with exact sample/context, applicable evidence and language-alignment limits. Status: **Verified finite callback contract**. No automatic delay, timeout, retry ceiling or unbounded termination guarantee. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**Bounded retry state — Frozen 4.0.** [fn:while-do](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-while-do) and [fn:do-until](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-do-until).
Initial engine delivery: [PR #88](https://github.com/Fytala-Charles/Bosak/pull/88),
[revision 0a928bb8](https://github.com/Fytala-Charles/Bosak/commit/0a928bb81b81d0631cc681f86bba83259c0ed790), REQ-118 4.0-S5.
Definitions reviewed **2026-10-11** against immutable
[QT4CG function source a014e1b4](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xpath-functions-40/src/function-catalog.xml).
This review pin does not recover the unavailable historical adopted specification snapshot.

## 2. Intent

Advance an invoice-submission state until it is accepted or its attempt budget
is consumed. Keep the status, attempt count and response history visible in the
result so acceptance can be distinguished from budget exhaustion.

## 3. Also Known As

Precondition loop (`while-do`) and postcondition loop (`do-until`). These are
explanatory terms, not alternative function names.

## 4. History / Supersedes

XPath 3.1 can carry state through a bounded `fold-left`, skipping updates after
acceptance. The complete comparison below returns the same report for the
illustrated responses. It still visits every position in the fixed range; it is
not a general early-terminating loop. Recursive helper functions are another
option in a host language that supports named function declarations.

## 5. Motivation

Electronic invoice gateways commonly acknowledge a submission before later
reporting acceptance. An application must stop checking once accepted and must
also stop when its configured attempt budget is used. Treating exhausted
attempts as success would hide invoices still awaiting acceptance.
The fixture uses recorded responses to make that state transition reproducible.

## 6. Language Alignment

`while-do($input, $predicate, $action)` tests the current value before updating
it. False or empty returns that value immediately; true calls the action and
repeats with the new value. Position starts at one and increments after an action.

`do-until($input, $action, $predicate)` always calls the action first, then tests
its new value using the same position. True returns it; false or empty repeats
at the next position. Note the opposite meanings of a true predicate and the
opposite callback argument order.

Both callbacks receive the **whole current `item()*` value**, not individual
members of a sequence. Actions may return any sequence, including empty.
Predicates return `xs:boolean?`; a status string is not a boolean and raises
XPTY0004. Callback arguments must be callable. The examples use two-argument
inline functions to show both state and position explicitly.

## 7. Applicability

Use for repeated pure state transformations with a clear stopping condition.
The sample assumes a nonnegative integer attempt limit, starts attempts at zero,
and recognizes only `accepted` as success. Missing response/status is normalized
to `pending`; other statuses remain unsuccessful and consume the budget.
A production integration should separately classify permanent rejection and
validate gateway responses according to its own protocol.

The XPath consumes an already supplied response fixture. The host application
owns actual requests, idempotency keys, persistence, timeouts, delays and any
backoff policy. Here an attempt means one recorded status check; it does not
mean resubmitting the invoice. No network request or sleep occurs in the expression.

## 8. Structure

A state map contains `attempts`, `status` and a sequence-valued `trace`.
The action reads the response at the current one-based position and constructs
a replacement map. The predicate combines status with the finite attempt limit.
The final report exposes acceptance or exhaustion independently.

## 9. Participants

The submission fixture provides ordered responses. The initial state supplies
an empty trace and zero attempts. The action advances state. The predicate owns
the stopping rule. The report presents the final state for the caller.

## 10. Collaborations

For `while-do`, continue while not accepted **and** below the limit. For
`do-until`, stop when accepted **or** at the limit. An outer zero-budget guard
is essential for `do-until`, because the function itself always runs its action.
The initially-accepted cases show why replacing one form with the other can
change behavior: `while-do` does nothing, whereas `do-until` performs a new check.

## 11. Consequences

A finite budget plus an action that increments attempts guarantees termination
for these fixtures, even if no response ever reports acceptance. The loops have
no implicit maximum iteration count or delay. An unchanged state with a predicate
that never stops could run indefinitely; every runnable case here has a bound
or a predicate that terminates on its first call. Exceptions propagate; the
functions do not automatically convert them into retryable outcomes.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles Before-versus-after predicate timing and callback argument order; Whole-sequence state, one-based iteration positions and empty results; Boolean/empty predicate result, callback errors and finite example termination. **Boundaries:** No implicit timeout, retry delay or iteration ceiling; only bounded terminating examples are supplied. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

Compile the new function calls with `CompileOptions.Compatibility40`. The default
3.1 profile rejects them; compile the `fold-left` comparison with `Compatibility31`.
The existing [catalog harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs)
verifies exact result strings, exception-message markers and default-profile
refusal for successful 4.0 cases. The custom `ATTEMPT` fixture includes that code
in its description: it verifies message propagation, not typed error-code metadata.
All inputs are inline; no variables, context item or external files are needed.

## 13. Sample Code

Complete synthetic input: a gateway status-check sequence for invoice INV-2048.
The third response remains unread because the second response is accepted.

```xml
<submission id="INV-2048">
  <response status="pending"/>
  <response status="accepted"/>
  <response status="pending"/>
</submission>
```

### Test before the action

[Runnable while-do expression](../examples/bounded-retries/while-main.xpath):

```xpath
let $responses := parse-xml('<submission id="INV-2048"><response status="pending"/><response status="accepted"/><response status="pending"/></submission>')/submission/response
let $limit := 3
let $initial := map {'attempts': 0, 'status': 'pending', 'trace': ()}
let $advance := function($state, $position) {
  let $status := string(($responses[$position]/@status, 'pending')[1])
  return map {'attempts': $state?attempts + 1,
              'status': $status,
              'trace': ($state?trace, $status)}
}
let $final := while-do($initial, function($state, $position) { $state?status ne 'accepted' and $state?attempts lt $limit }, $advance)
return concat('status=', $final?status, '; attempts=', $final?attempts, '; exhausted=', $final?status ne 'accepted' and $final?attempts ge $limit, '; trace=', string-join($final?trace, ','))
```

Exact output:

```text
status=accepted; attempts=2; exhausted=false; trace=pending,accepted
```

### Test after the action

[Runnable do-until expression](../examples/bounded-retries/until-main.xpath):

```xpath
let $responses := parse-xml('<submission id="INV-2048"><response status="pending"/><response status="accepted"/><response status="pending"/></submission>')/submission/response
let $limit := 3
let $initial := map {'attempts': 0, 'status': 'pending', 'trace': ()}
let $advance := function($state, $position) {
  let $status := string(($responses[$position]/@status, 'pending')[1])
  return map {'attempts': $state?attempts + 1,
              'status': $status,
              'trace': ($state?trace, $status)}
}
let $final := if ($limit le 0) then $initial else do-until($initial, $advance, function($state, $position) { $state?status eq 'accepted' or $state?attempts ge $limit })
return concat('status=', $final?status, '; attempts=', $final?attempts, '; exhausted=', $final?status ne 'accepted' and $final?attempts ge $limit, '; trace=', string-join($final?trace, ','))
```

Exact output:

```text
status=accepted; attempts=2; exhausted=false; trace=pending,accepted
```

### XPath 3.1 comparison

[Runnable bounded fold](../examples/bounded-retries/baseline.xpath):

```xpath
let $responses := parse-xml('<submission id="INV-2048"><response status="pending"/><response status="accepted"/><response status="pending"/></submission>')/submission/response
return let $limit := 3
return let $initial := map {'attempts': 0, 'status': 'pending', 'trace': ()}
return let $advance := function($state, $position) {
  let $status := string(($responses[$position]/@status, 'pending')[1])
  return map {'attempts': $state?attempts + 1,
              'status': $status,
              'trace': ($state?trace, $status)}
}
return let $final := fold-left(1 to $limit, $initial, function($state, $position) { if ($state?status eq 'accepted') then $state else $advance($state, $position) })
return concat('status=', $final?status, '; attempts=', $final?attempts, '; exhausted=', $final?status ne 'accepted' and $final?attempts ge $limit, '; trace=', string-join($final?trace, ','))
```

Exact output:

```text
status=accepted; attempts=2; exhausted=false; trace=pending,accepted
```

### Exhausted budget

Replace the accepted response with pending. Complete input:

```xml
<submission id="INV-2048">
  <response status="pending"/>
  <response status="pending"/>
  <response status="pending"/>
</submission>
```

Both [while-do](../examples/bounded-retries/while-exhausted.xpath) and
[do-until](../examples/bounded-retries/until-exhausted.xpath) report:

```text
status=pending; attempts=3; exhausted=true; trace=pending,pending,pending
```

### All boundary and callback fixtures

Every file embeds its full input and each report below is exact.

| Runnable case | Result or diagnostic marker |
|---|---|
| [while-main](../examples/bounded-retries/while-main.xpath) | `status=accepted; attempts=2; exhausted=false; trace=pending,accepted` |
| [until-main](../examples/bounded-retries/until-main.xpath) | `status=accepted; attempts=2; exhausted=false; trace=pending,accepted` |
| [baseline](../examples/bounded-retries/baseline.xpath) | `status=accepted; attempts=2; exhausted=false; trace=pending,accepted` |
| [while-exhausted](../examples/bounded-retries/while-exhausted.xpath) | `status=pending; attempts=3; exhausted=true; trace=pending,pending,pending` |
| [while-one-attempt](../examples/bounded-retries/while-one-attempt.xpath) | `status=pending; attempts=1; exhausted=true; trace=pending` |
| [while-zero-budget](../examples/bounded-retries/while-zero-budget.xpath) | `status=pending; attempts=0; exhausted=true; trace=` |
| [while-missing-status](../examples/bounded-retries/while-missing-status.xpath) | `status=accepted; attempts=2; exhausted=false; trace=pending,accepted` |
| [until-exhausted](../examples/bounded-retries/until-exhausted.xpath) | `status=pending; attempts=3; exhausted=true; trace=pending,pending,pending` |
| [until-one-attempt](../examples/bounded-retries/until-one-attempt.xpath) | `status=pending; attempts=1; exhausted=true; trace=pending` |
| [until-zero-budget](../examples/bounded-retries/until-zero-budget.xpath) | `status=pending; attempts=0; exhausted=true; trace=` |
| [until-missing-status](../examples/bounded-retries/until-missing-status.xpath) | `status=accepted; attempts=2; exhausted=false; trace=pending,accepted` |
| [while-initially-accepted](../examples/bounded-retries/while-initially-accepted.xpath) | `status=accepted; attempts=0; exhausted=false; trace=` |
| [until-initially-accepted](../examples/bounded-retries/until-initially-accepted.xpath) | `status=accepted; attempts=2; exhausted=false; trace=pending,accepted` |
| [while-empty-decision](../examples/bounded-retries/while-empty-decision.xpath) | `0` |
| [until-empty-decision](../examples/bounded-retries/until-empty-decision.xpath) | `2` |
| [until-empty-state](../examples/bounded-retries/until-empty-state.xpath) | `1,2` |
| [while-sequence-state](../examples/bounded-retries/while-sequence-state.xpath) | `1,2,3,4` |
| [while-nonboolean](../examples/bounded-retries/while-nonboolean.xpath) | `XPTY0004` |
| [while-noncallable-action](../examples/bounded-retries/while-noncallable-action.xpath) | `XPTY0004` |
| [while-noncallable-predicate](../examples/bounded-retries/while-noncallable-predicate.xpath) | `XPTY0004` |
| [while-action-error](../examples/bounded-retries/while-action-error.xpath) | `ATTEMPT` |
| [until-nonboolean](../examples/bounded-retries/until-nonboolean.xpath) | `XPTY0004` |
| [until-noncallable-action](../examples/bounded-retries/until-noncallable-action.xpath) | `XPTY0004` |
| [until-noncallable-predicate](../examples/bounded-retries/until-noncallable-predicate.xpath) | `XPTY0004` |
| [until-action-error](../examples/bounded-retries/until-action-error.xpath) | `ATTEMPT` |

A zero budget reports exhausted without performing a check. A budget of one
stops after pending. An initially accepted state stays unchanged with `while-do`,
but the `do-until` variant consumes pending then accepted responses. A missing
first status uses pending and still reaches acceptance on attempt two.
The sequence-state fixtures prove that callbacks receive the whole sequence;
the empty-state fixture proves that empty input does not skip a do-until action.
An empty decision stops while-do immediately but continues do-until until its
second, true decision. Nonboolean/noncallable fixtures raise XPTY0004; an action
exception propagates without being retried automatically.

## 14. Performance

The main expressions perform at most the configured number of actions and
stop early on acceptance. They are synchronous state transformations, with no
built-in scheduling or wait. The trace grows with completed attempts and keeps
those statuses in memory. The bounded 3.1 fold visits all budget positions even
after acceptance, while avoiding further action calls. No benchmark or general
streaming claim is implied.

## 15. Known Uses

This is a synthetic reference workflow, not a claim about a particular deployed
gateway. Existing unit evidence includes
[`WhileDo_PredicateInitiallyFalse_NeverCallsAction`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`WhileDo_WholeSequenceIsTheValue`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`WhileDo_PositionIncrementsFromOne`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`WhileDo_EmptyPredicateResultMeansFalse`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`WhileDo_ActionError_Propagates`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`DoUntil_ActionEvaluatedBeforePredicate`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`DoUntil_EmptyInput_StillRunsAction`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
and [`DoUntil_PositionIncrementsFromOne`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).
The [4.0 gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs)
assigns both exact sets `fn-while-do` and `fn-do-until` to GatedSets.
This documentation delivery changes no gate membership.

## 16. Related Features

[Ordered queue prefix](ordered-queue-prefix.md) stops sequence selection at a
boundary; these loops repeatedly replace an entire state value instead.
[Higher-order selection](higher-order-selection.md) describes callback-based
selection. [The completeness audit](../COMPLETENESS-AUDIT.md) records coverage
limits and the remaining dependency traversal and partial-application families.
