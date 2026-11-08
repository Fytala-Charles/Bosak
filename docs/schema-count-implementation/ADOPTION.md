# Count assertion implementation adoption — 29 October 2026

Recorded before production edits. Source bases: core `da6c08141c03af34b537599b3ff5eadb77240ac9`, Schema `2f4bf417131ba70e1653eda3d9148d83a6ef6d8a`. The user requested implementation of the recommended count slice after review/checkpoint and scope delivery.

Adopt the exact normative edition/revision and bounded deviations in the [count adoption](../schema-count-assertions/COUNT-ASSERTION-ADOPTION-2026-10-29.md) and its [separate source locks](../schema-count-assertions/COUNT-ASSERTION-SOURCES.json): F&O Second Edition `REC-xpath-functions-20101214`, expressly as revised 13 April 2015, §15.4.1; XPath Second Edition `REC-xpath20-20101214` §2.1.1, §2.4.2–2.4.3, §3.1.5, §3.5.1/§3.6; XSD Structures `REC-xmlschema11-1-20120405` §3.13.2, §3.13.4.1–3.13.4.2 and §3.13.6.2. Retrieved representation hashes, dates and historical adoption editions remain separate records.

Expose only unprefixed count(DeclaredChildName) as a scalar operand in the existing comparison/logical grammar. Preserve bare count child names; no prefixed/aliased calls, arbitrary arguments, paths, nested functions, aggregates, general comparisons or arithmetic. Enforce character/token/depth bounds, including call parentheses. Compile/evaluate the validated original expression through the real XPath 2.0 engine after complete native structure and exact-value assessment. Preserve original state, copying, no-typed-value root and error boundaries.

Select Schema 0.8 development via MinVer and profile v4 only after a successful full grammar parse consumes an actual count call. Existing no-count schemas retain v1/v2/v3. Core remains source-identical 1.34.1; no public overload or product consumer pin change.

One historical refusal intentionally changes: RepeatedDecimalAssessmentTests.WiderAssertionGrammar_RemainsUnsupported("count(a) eq 2") becomes a dedicated supported/v4 capability check. Preserve its original test source/receipt and explicitly map the adopted case, keeping the other 333 expectations unchanged. This is the scoped capability adoption, not removal of a failing assertion. Run new count regressions against the original source before production edits.

Qualify complete Schema tests, source-override build, actual count-validated example, copy/typing controls and strict docs. Inherit prior core/gates only after source/configuration identity checks per the plan. Keep all prior sealed evidence unchanged. No commit/push, merge, package or tag is included in this implementation step.
