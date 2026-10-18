# REQ-132 consumer design review handoff

18 October 2026. **Design proposed; Braid review pending.** No engine implementation or new capability is delivered. Baseline: Bosak public 1.19.1 at a9e954d75b3518340ce6da84c9ae06c50ab1f5a4; documentation cd5507afa2e2bfc98f20669b2941acb5b938a121.

Review the [proposed contract](REQ-132-LITERAL-ELEMENT-CONTRACT.md), [28 planned acceptance groups](REQ-132-LITERAL-ELEMENT-ACCEPTANCE.md), and [unchanged consumer request](REQ-132-literal-result-element-insertion.md). Its SHA-256 is `632df632b35a162ff288f74dad8e8a494eedcff1e4081194b8823bf01597f600`.

Please assess these concrete decisions:

1. One paired empty no-namespace element, local-name intent only. Common inherited default namespaces get a local xmlns="" reset; namespace-alias contexts refuse in the first profile.
2. Separate capability/typed target/proposal; PreserveSource default and independently advertised Readable, with no automatic downgrade.
3. Actual InsertedNode/InsertedParent and zero-length BodyRange; no synthetic child or fabricated Expression slot. Total injective old mapping plus explicit NewNodes partition; namespace declarations remain attribute metadata.
4. Fresh text/value-of targets on the new element, including namespace-reset fixtures, support separate later Apply/history steps. Existing operations retain their original behavior.
5. Scope/resource/cancellation/name refusals and 8 MiB physical-source bounds remain engine-owned. Stale current-session authority, licenses, frozen review, Apply/history and native acceptance remain consumer-owned.
6. API member names and proposed evidence are sufficient for bounded review without a Braid parser, markup splice, guessed ID or inferred source range.

Return blocking findings and any required contract corrections, with the reviewed immutable commit, inspection/probe counts if actually run, and explicit scope limits. A no-blocking design review does not constitute implementation review, public-package verification or native acceptance. Any future source/package probes need distinct observed receipts. No external message has been sent by preparing this handoff.
