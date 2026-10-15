<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-128 consumer finding F1 correction">
  <br><br><h1>REQ-128 consumer review: F1</h1>
  <p>Engine correction — 15 October 2026</p>
</div>

The Braid review of [PR #312](https://github.com/Fytala-Charles/Bosak/pull/312), head `404d3096ad200052c28148087c4e004bcad877a3`, correctly identifies a P1 static-coverage defect. The review is retained at `D:/Development/Bosak.Braid/docs/BOSAK_REQ128_CONSUMER_REVIEW.md`; this engine response does not edit it or claim consumer acceptance.

## Finding and correction

The step visitor inspected lookup expressions and predicates but skipped `NodeTest.KindTestTypeName`. The parser stores the type of `document-node(element(..., T))` in that same field. Consequently typed element/attribute tests, including nested document tests, could return an accepted candidate even though the declared insertion contract excludes typed tests and general compilation defers some type/namespace errors until execution.

The correction enforces the existing exclusion. A step with any type annotation, a schema-element/schema-attribute test or a nested schema test returns `UnsupportedCoverage`. The visitor checks parsed typed metadata before its supplementary prefix validation, so an excluded type does not become an implicitly accepted namespace binding. The whole-snapshot visitor is shared by target discovery and candidate validation: both original slots and the proposed select are covered. No static type support or public API is added, and the unpublished source version remains 1.8.0.

Untyped `element(*)`, `attribute(*)` and `document-node(element(*))` tests retain support. Refusals return no target/candidate and leave the original bytes unchanged. Braid's production pin, ports, UI and Apply/history are untouched.

## Regression evidence

Fifty typed/pattern regression cases cover XSLT 3.0 and 4.0, element/attribute tests, nested document tests, nested predicates, optional inner types, missing declarations, bound built-ins, bound unknown types and unprefixed unknown types. Ten positive controls cover untyped element, attribute and nested document tests and ordinary names resembling schema tests in both versions. Each refusal asserts the result classification, absence of a usable target/candidate and exact original-byte retention.

Against the reviewed implementation, the initial 56 cases produce **6 pass / 50 fail**. After the correction, all **141 insertion cases pass**, comprising the original 81 and the new 60. The original consumer probes and upstream working copies remain unchanged. Baseline and corrected TRX/log receipts are retained separately.

Full solution, language-server, frozen-gate and fresh exact-revision package evidence will be recorded in [the verification receipt](REQ-128-F1-VERIFICATION.json) when complete. Previous candidate receipts remain historical and are not evidence of F1 closure.

## Adoption boundary

F1 is corrected in engine source, pending the revised exact-commit consumer review. REQ-128 remains In Progress for unpublished 1.8.0. Keep the Braid pin at `5bab4799783569d538cec9b03d37a57454f0eebc`; acceptance, merge, final integrated-revision verification, publication and consumer adoption remain separate.
