# Preserve XML namespace undeclarations through literal-result construction

Depends on #474; this review is based on its `docs/xslt30-qualification` branch and contains only the namespace follow-up.

Literal-result namespace collection ignored XML 1.1 undeclaration metadata and resurrected ancestor bindings. Attribute-name hints could do the same, while provisional inheritance-barrier markers could hide a child's own binding from raw XDM queries. Honor prefix shadowing in both collection routes and reconcile markers with final namespace bindings. Deeper rebinding remains effective without leaking into siblings.

The XML writer also omits redundant identical declarations while preserving actual rebinding and default namespace resets. This is a serialization policy choice, not a claim that redundant XML 1.0 declarations are universally forbidden. Public API signatures stay unchanged; Core/server development metadata advances to 1.81.4 without publication or an extension-version change.

Validation:

- Five new cases pass; the identical sources against the sealed 1.81.3 baseline yield four regression failures and one pass.
- The complete XML-version selection passes 42/42, resolving all eight failures.
- All 26,354 solution cases, including 8,329 XSLT cases, and 90 separate language-server cases pass.
- Frozen QT3 remains 31,397/0/424 and selected qt4 remains 8,941/0/1,918, with exact skip names/reasons preserved and 11 experimental passes included.
- Eleven isolation controls and sixteen existing profile/error controls retain their expected outcomes.
- Full pinned W3C XSLT 3.0 catalog: 10,219 passed / 2 failed / 4,380 skipped; all 14,601 cases accounted for, no corpus writes, and every skip identity/explanation unchanged.

Standalone qualification remains unsuccessful because `output-0707` and `try-022` still fail. The legacy rounded 100.0% line and exit zero are independently rejected as success. No fixture edits, new skips, schema expansion or standalone gate promotion. Original and PR #474 evidence remain unchanged; immutable adoption, source/runtime hashes, regression probes and final receipts are retained separately.
