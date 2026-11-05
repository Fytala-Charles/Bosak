# K04-C15: bounded calendar fallback correction ? 27 October 2026

The shared formatter now distinguishes valid but unsupported calendar designators from invalid names. Requests for listed unsupported calendars such as CB and AH use Gregorian AD formatting with one `[Calendar: AD]` indication per complete output. Namespaced calendar fallback now carries the same indication. Invalid unqualified names still raise FOFD1340 in XPath mode; existing XSLT fallback for unknown bare names is retained.

## Semantic scope

[Pre-implementation adoption](evidence/IMPLEMENTATION-PIN.json) pins the immutable F&O 3.1 Recommendation and qt4cg source revision e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e, with hashes and affected sections. Calendar validity and support are separate. The implementation recognizes the 28 listed designators; the existing AD/Gregorian and ISO/ISO8601 aliases remain implemented paths. Other listed designators select AD fallback rather than alternate-calendar arithmetic. Namespaced identifiers select the documented AD fallback even when their local name matches a supported bare alias.

Validation and calendar indication now happen once, before rendering the picture. Empty and literal-only pictures therefore validate the calendar too; multiple markers produce one calendar prefix. In fallback output, component C identifies AD. Existing non-fallback C/E behavior is unchanged. Language fallback remains separate.

This is deliberately bounded. Existing case-insensitive supported aliases and the default for an empty calendar string remain. Listed unsupported designators use the spelling in the pinned table. Static prefix resolution and the legacy NCName/EQName parser approximation remain open. No complete alternate-calendar, ISO era/week or XML Schema capability claim is made.

## Verification

- **381 final regression rows pass** across XPath 3.1, XPath 4.0 and Experimental profiles, including shared XSLT mode. They cover every unsupported listed calendar, supported aliases, malformed/unknown names, namespaced fallback, literal/empty/multiple-component pictures, component errors and retained empty-value behavior.
- The initial regression run had 249 failures/132 passes. Of these, 240 demonstrate calendar behavior corrected here. Nine expose the independent pre-existing empty-sequence wrapper defect: XPTY0004 is raised before calendar parsing. Those nine are retained as no-change controls; this does not claim correct empty-value semantics. [Disposition](evidence/empty-value-disposition.json) preserves the expected correct behavior and open defect. The initial post-change run and test source remain historical receipts.
- All **58 original/control diagnostics pass**, including all four original CB assertions with unchanged fixtures, environments and assertions. The earlier review's 28 assertion failures become passes; its original 30 passes remain passes. These are direct executor diagnostics, not admission changes.
- The complete solution build succeeds with zero errors; all **22,579 solution tests** pass, including **8,196 XSLT tests**. Normal gate totals and exact skip identities are recorded in [validation](evidence/validation.json). Both normal gates retain **QT3 31,162/0/659** and **qt4 8,587/0/1,892**, with every retained skip name/reason identical to the approved K04-C12 baseline. GatedSets and dependency filters are unchanged.

The first diagnostic attempt reused a driver linked to 1.28.0 after harness assemblies were rebuilt as 1.28.1; it produced a version-load error and watchdog timeout. Its receipts are preserved separately. A freshly built diagnostic driver linked to the current assemblies completes all 58 checks under the unchanged 20-second watchdog. No runner retry or relaxed assertion ships.

## Status and next slice

Candidate **1.28.1** is unpublished; public version remains **1.26.3**. The calendar engine correction is complete for this bounded scope. The four original CB cases remain dependency-skipped in normal runs. Next: review exact CB capability admission with positive/negative and unknown-name controls, then rehearse complete normal runners before proposing any exact QT3 baseline change. Language admission, FULLY-NORMALIZED and empty-value wrapper findings remain separate. Bosak.Schema retains staged XSD 1.1 ownership; REQ-126 remains Complete under its original bounded acceptance.

No fixture/assertion, dependency filter, GatedSets, exact baseline, schema backend, public API, release, upstream or Braid change. Historical C13/C14 evidence is preserved separately from this semantic adoption. Source base: 548d18a9; delivery revision: `git log -1 --format=%H -- docs/conformance/k04-calendar-fix/README.md`.

Run `python docs/conformance/k04-calendar-fix/evidence/validate-evidence.py` to verify the recorded results and identities. HTML renderer receipts cover the changed documentation using unchanged Fytala CSS; they do not claim screenshot/PDF acceptance.
