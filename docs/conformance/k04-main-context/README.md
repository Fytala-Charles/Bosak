# K04-C26 - Main-module context-item declarations

28 October 2026. Implemented against `e2034067`. Candidate **1.30.0** remains unpublished; public version is **1.26.3**. The [pre-implementation semantic adoption](evidence/IMPLEMENTATION-PIN.json) pins XQuery 3.1 REC-20170321 and the initialization, typing and dependency rules.

Main modules now accept `declare context item`, including typed values, external values and external defaults. An internal declaration record survives static-context cloning; initializer expressions compile with the complete prolog's namespaces, functions and globals. The runtime enforces a single item and its declared type without atomization/promotion, gives it singleton focus, and initializes dependent globals under that focus. Context/global cycles raise XQDY0054. ItemType grammar rejects top-level occurrence indicators and empty-sequence(); duplicates raise XQST0099.

A supplied host item takes precedence for an external declaration. An explicit non-external value ignores the host item, an implementation choice recorded before coding. External declarations without a supplied value/default leave focus absent; a later required focus read raises XPDY0002. Initializer self-focus dependencies use the accepted XQDY0054 alternative. Library initializers/defaults still raise XQST0113, and C25's duplicate correction is preserved.

## Execution boundary

An internal callback resolves semantic focus reads while initialization is pending. VM context loads and implicit-focus standard functions use it; bookkeeping focus snapshots remain inert. Global variable initializers retain forward references and cycle detection. Cached declared globals are refreshed when an explicit-context query is evaluated again. Host focus is restored on success and error.

For queries with an explicit main context declaration, global sequence values and the query-body sequence result are materialized before their focus scope ends. This stabilizes deferred results when host focus is restored and is an explicit execution policy, with memory proportional to those materialized sequences. Queries without such declarations keep their prior result path. Existing global-resolver lifetime behavior is retained, including globals referenced by a returned function through its host context. No new public API is introduced.

Type matching uses the current XDM implementation. General custom-schema/imported-module qualification is not inferred from these bounded controls; Bosak.Schema retains staged XSD 1.1 ownership.

## Verification

- **52 new public API regressions pass**. The initial 36-case cohort had 30 failures/six passes before implementation and then passed completely. Twelve supplemental controls cover implicit focus, composite items, body-error restoration and imported-library types. Four further grammar controls cover whitespace, comments, parenthesized empty-sequence and union alternatives. All final 52 pass.
- The complete final solution build and test run pass: **23,822 tests**, including **423 XQuery**, **13,797 Standard** and **8,196 XSLT** tests.
- All **31 bounded original QT3 assertions** pass through public XQuery. Thirty prior failures become passes; the invalid ItemType original `contextDecl-023` retains its XPST0003 pass with a grammar-specific diagnostic. Every original assertion and accepted error alternative is preserved.
- All **18 independent public controls** pass, repairing the 15 prior declaration failures. Together this repairs exactly **45 assertion outcomes**. All five library-parser controls are unchanged passes.
- Actual dependency/routing assessments for all **97 inventoried originals** remain identical. The direct actual executor still reports one pass/30 skips for the selected cohort, because the normal routing guard is retained.
- Complete normal context-declaration sets remain **4/0/40** (QT3) and **0/0/53** (qt4). Every skip name/reason is unchanged.
- Fresh complete gates remain QT3 **31,165/0/656**, qt4 **8,939/0/1,920**, **230 gated sets** and 11 Experimental passes. Every retained skip name/reason matches the approved baseline exactly.

All 90 final bounded gate/diagnostic processes exit successfully without timeout. The first implementation's 36-case verification and complete 23,806-test run and the later 48-case, 23,818-test verification are archived separately from final verification. A supplemental error assertion initially looked for FOER0000 in the message of `XPathErrorException`; that exception exposes it as `CodeLocalName`. The test now checks the structured code, retaining FOER0000 as the expectation. Its initial 47-pass/one-failure log and source are preserved. No production error code changed for that assertion correction.

Two existing C25 unit checks that expected unsupported main declarations are updated to assert their newly adopted successful behavior. Their old source and all C25 receipts remain byte-preserved. [Manifest](evidence/manifest.json), [review deltas](evidence/review-deltas.json), [skip verification](evidence/skip-verification.json), [results](RESULTS.json) and [validator](evidence/validate-evidence.py) record the complete scope. Nine Markdown pages render with unchanged Fytala CSS; HTML generation is not screenshot/PDF acceptance.

## Next slice: wider qualification

Audit C24's **13 remaining QT3 originals**: ten with module mappings and three with resource-bearing environments. The two independent imported-library type regressions here do not replace that original-case audit. Verify source/document/schema environment handling, imported type constraints and shared initial focus using unchanged assertions and bounded processes. Only after qualification should a separate review consider narrowing the context routing guard and proposing exact baseline changes. Qt4 XQuery-only declarations remain inapplicable to its XPath-only runner.

No routing/dependency guard, KnownSkips, GatedSets, AGENTS baseline, fixture/assertion, schema backend, release, publication, push, tag, upstream or Braid change is included. REQ-126 stays Complete under bounded owner acceptance; all historical dispositions and 48 unmet batch observations are preserved.
