<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala XSLT union minimum-priority correction">
  <h1>XSLT 4.0 union minimum priority</h1>
  <p>22 October 2026 — source 1.23.3, unpublished</p>
</div>

**22 October 2026 — EQName priority correction, source 1.23.4:** Effective 4.0 name-test priority ignores URI punctuation; earlier profiles retain their existing behavior. [Scope, regressions and validation](EQNAME-PATTERN-PRIORITY.md). Opening-bracket URI parsing remains a separate limitation. Public release remains 1.23.2; REQ-126 stays In Progress. Prior delivery records below retain their historical versions and outcomes.

Source 1.23.3 assigns a 4.0 node-union template the **minimum** default priority of its alternatives, including alternatives that do not match the current item. Public 1.23.2 uses the maximum. This can change template selection when competing rules rely on implicit priorities. Explicit template priorities remain authoritative.

For `<root/>`, `match="root | missing/child"` now has priority 0 rather than 0.5. A competing rule with explicit priority 0.25 therefore produces `competitor`. Both symbolic and keyword union catalog examples now declare this result; their previous `pattern` outcomes remain labelled as observations of 1.23.2. Existing example filenames are retained for stable links.

## Specification and scope

The [immutable XSLT draft source](https://raw.githubusercontent.com/qt4cg/qtspecs/3230573afe246f1b502ed2e22d187ce9928155b1/specifications/xslt-40/src/xslt.xml), section `default-priority-for-xnode-patterns`, requires the minimum alternative default priority. Upstream `master` was refreshed on 22 October 2026 and remained at `3230573afe246f1b502ed2e22d187ce9928155b1`; independently downloaded XML SHA-256 is `6d3038e21291abf761d883dd371cb9885972e0dd1c85b41844df70ca1ede45b8`. This is a current review pin; historical specification adoption has not been recovered.

The change is confined to `TemplateRule.ComputeDefaultPriority`'s 4.0 node-union aggregation. It retains 3.0 branch-specific dispatch, effective local template versions, explicit priorities, 4.0 single-rule identity, ambiguity handling and `next-match` deduplication. Type-choice and kind-test-name maximum calculations are unchanged. No harness assertions, gate membership, APIs, Braid pins or upstream fixtures change.

## Regression evidence

The original union test file and catalog manifest are preserved under `.guard-tmp/req126-union-minimum-evidence`, alongside the previous `.guard-tmp/req126-compound-evidence` execution receipts. The nine maximum-priority expectations are deliberately replaced with pinned minimum-priority expectations. Six new cases cover unmatched lower-priority alternatives under four root/local version combinations and equal priorities under both union spellings. Existing explicit-priority, nested-union, single-rule `next-match`, ambiguity, namespace and malformed-pattern controls remain.

The initial red run reported 12 failures / 22 passes. Eleven failures demonstrate the scoped priority discrepancy (nine existing expectation corrections and two effective-4.0 controls). The twelfth was an exploratory expectation change in `OperatorDelimiters`, which uses `Q{urn:n|part}root`. Inspection found a separate limitation: `IsQNameNodeTest` treats the pipe inside the EQName URI as an operator, giving that branch fallback priority 0.5. Minimum aggregation then still yields 0.5. That test's original assertion is restored unchanged; correcting EQName classification needs a separate focused slice. The original red log and the intermediate 33-pass/1-failure run are retained. This delivery does not claim full EQName-priority conformance.

The first full run additionally identified two maximum-based expectations in `Xslt40SetPatternPriorityTests.FirstOperandPriority`: `(* except missing)|root` has minimum -0.5, and `root intersect * | missing/child` has minimum 0. Their explicit competitors (-0.1 and 0.25 respectively) now win. Both assertions are corrected to `explicit`; the set operand calculation itself is unchanged. Original test source and the failed full-run TRXs remain preserved. An attempted concurrent rebuild encountered a test assembly lock; its log is retained and a clean final rebuild follows the completed run.

Final validation is recorded in [the machine-readable receipt](UNION-MINIMUM-PRIORITY.json): 68 focused union/set-priority checks, 10,444 full solution tests, QT3 31,142/0/679 and qt4 gated 8,278/0/1,894. Build and test logs, TRXs, both gate outputs and specification content are retained in the evidence directory. A sandboxed restore reported vulnerability-feed access warnings; the existing conformance nullable warning is unrelated to this correction.

The final full run encountered one unrelated database HTTP listener registration collision on `127.0.0.1:54156`. The isolated database project retry passed all 47 tests. The 10,444 passing total uses that successful retry in place of the affected project result; the original failed receipt is preserved separately.

## Delivery boundaries

PR #404 merged as `beac211f`; the correction starts from that revision. Catalog totals remain 71 entries, 2,743 feature checks plus 24 chapter checks = 2,767, four worked chapters and 117 assigned function names. REQ-126 remains In Progress; D2/D3/D4 and AC-03 remain partial. README install/CLI instructions are unchanged. Source is 1.23.3; public release remains 1.23.2. Publication and downstream adoption require their own delivery evidence.

Next suggested slice: correct EQName priority classification with URI punctuation regressions, while preserving older-profile behavior and running both mandatory gates again. The [original finding](UNION-PRIORITY-ENGINE-FINDING.md) and [compound-pattern audit](COMPOUND-PATTERN-GRAMMAR.json) retain the earlier observation history.
