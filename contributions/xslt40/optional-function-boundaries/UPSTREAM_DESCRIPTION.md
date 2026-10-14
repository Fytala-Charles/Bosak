Adds three XSLT 4.0 cases to the function family:

- function-40-024 distinguishes an omitted keyword argument from explicit (), with reordered keywords and an arrow call.
- function-40-025 checks that both select and sequence-constructor defaults resolve the global binding when an earlier function parameter and a caller local have the same name.
- function-40-026 checks item, position and size from function-item creation focus for named references, function-lookup and partial application; invocation occurs under a different focus. Both default forms are covered.

The expected results follow XSLT 4.0 sections 10.3.1-10.3.3, especially the default-evaluation context rule in 10.3.2:
https://qt4cg.org/specifications/xslt-40/Overview.html#function-arguments

Existing cases 011-013, 022-023 and higher-order-functions-081 already cover basic defaults and reduced-arity access. These additions cover combinations not exercised there. Existing fixtures and assertions are unchanged.

Validation: catalog schema validation passes; all three new cases pass in Bosak source candidate 1.3.0. Each result assertion fails after a deliberate fixture mutation. All three fail with XTSE0020 in retained pre-feature 1.2.1 binaries and a compatible retained harness. Independent processor confirmation is pending.

A broader function-40 sample reports 29 passes and 10 existing failures, versus 26 passes and the same 10 failures without these additions. Eight failures concern unsupported no-namespace functions; function-40-012 returns an empty sequence and function-40-904 expects XPTY0004 where Bosak reports XTTE0600. These observations are separate from this contribution.

Prepared from freshly fetched upstream master 91328fbf0fed4a5d2813eb3661808ee53cb961ba, refreshed again immediately before submission to avoid the stale-base conflicts noted in issue #28.