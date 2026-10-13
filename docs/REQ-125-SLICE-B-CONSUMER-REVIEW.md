# REQ-125 Slice B Consumer Review

> Reviewed: 2026-10-10 · Consumer: Bosak.Braid · Decision: corrections required before acceptance

## 1. Delivery and Independent Evidence

Slice B implementation: `e72e4a1bec88f0b202951d744cacc732b2572648`. Tested a source archive of committed Bosak `ba5a30f`, which includes Slice A corrections and Slice B but excludes the active checkout's uncommitted Slice C edits. No Bosak engine files or active checkout state were changed.

Independently ran `ControlledResourcePolicyTests` and `ControlledResourceRouteTests`: **53 passed / 0 failed / 0 skipped**. The archive initially inherited Braid-only warning/analyzer enforcement; upstream tests were then built with that enforcement disabled for this isolated review only. No production build settings were weakened. Bosak's broader conformance results were not rerun.

Source review confirms authoritative resource bytes, denial/abstention/error handling, no-fallback branches on the tested routes, DTD-prohibited document loading, nested transform policy inheritance, and receipt generation. Independent public-API probes nevertheless found two acceptance blockers.

## 2. Findings

### B-F1: Document cache bypasses route-specific refusal

Construct `ControlledResourcePolicy` with an authority approving `<approved/>`, and mark `ControlledResourceRoute.SourceDocument` unsupported. Attach it to one new `EvaluationContext`. Call:

```csharp
context.LoadDocument("urn:review:input", ControlledResourceRoute.Document);
context.LoadDocument("urn:review:input", ControlledResourceRoute.SourceDocument);
```

**Actual:** both calls return a document. The second call does not consult the policy or record a refusal, despite SourceDocument being explicitly unsupported.

**Expected:** cached content must not bypass the requested route's authority or unsupported-route check. This probe does not demonstrate new filesystem access; it demonstrates access through a prohibited route and missing acquisition-policy evidence. A control that runs only on cache misses cannot enforce the public per-route contract.

`EvaluationContext.LoadDocumentControlled` checks `_documentCache` before `policy.AcquireBytes`. Its cache key includes URI and load policy, but not resource authority or route. Correctness needs both route checks and a defined policy/authority lifetime for cached documents.

**Required regressions:** approved Document followed by unsupported SourceDocument must refuse XV0005; route-specific authority denial must refuse XV0004 even when another route populated the cache; changing the context policy must not expose earlier unapproved cached content. Positive same-policy/same-route cache behavior and document identity must remain supported as documented. Include an actual stylesheet using doc() and source-document for the same URI.

### B-F2: Static-evaluation restrictions are not applied

Construct an approving authority and mark `ControlledResourceRoute.StaticEvaluation` unsupported. Compile using `XsltCompiler.ResourcePolicy`:

```xml
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
  <xsl:template match="/" use-when="doc-available('urn:review:static')">
    <ok/>
  </xsl:template>
</xsl:stylesheet>
```

**Actual:** compilation completes and the resource is approved. Receipts are tagged `Document`; there is no StaticEvaluation refusal. The probe used no resource files or transformation execution.

**Expected:** a host's declared static-evaluation restriction must govern acquisition during static evaluation, even where the concrete acquisition is a document/text/JSON operation. For availability functions, spec-appropriate false may be appropriate, but there must be no approval through the forbidden phase and no fallback IO. The request/receipt contract must make both purpose and concrete operation enforceable or clearly define an equivalent combined policy.

`Stylesheet` attaches the policy and marks the context as static evaluation, but the document acquisition path still uses the ordinary Document route. The StaticEvaluation enum declaration alone does not enforce the host restriction.

**Required regressions:** denied/unsupported static acquisition cannot approve a resource, while permitted runtime Document access still works. Cover doc/document, availability functions and text/JSON static paths where supported, including nested compile contexts. Preserve useful concrete-route evidence while identifying static purpose. If a static path cannot be controlled, refuse it explicitly rather than documenting unsupported controls as supported.

## 3. Consumer Acceptance

**Slice B is not yet accepted.** B-F1 and B-F2 affect the core host-authoritative route/purpose contract in AC-04/05. The green upstream tests establish useful baseline coverage but do not cover these cross-route/cache and phase restrictions.

Bosak owns corrections, API semantics, engine tests and compatibility decisions through REQ-125. Braid will rerun these probes after delivery. Accepted Slice A and REQ-124 remain accepted; Braid's engine pin is unchanged. Production preview and Slice C/D acceptance remain separate.

## 4. Correction Re-review: B-F1/B-F2 Closed; B-F3 Outstanding

Reviewed fix **facbf373de0cd8c54b2a0ddbc9d730c21e83c78f** on 2026-10-10 using an isolated committed-source archive. Independently ran the three policy/route/review-finding suites: **67 passed / 0 failed / 0 skipped**. Source review confirms authorization before cache service and policy/purpose-aware cache keys; static requests now carry StaticEvaluation purpose plus a concrete operation, with both unsupported-route checks.

The original probes now return SourceDocument refusal XV0005 and a StaticEvaluation RefusedUnsupported receipt. The static doc-available case may complete compilation because the denied availability check returns false; no static resource approval occurs. **B-F1 and B-F2 are verified closed.**

### B-F3: Cache-hit receipts identify newly approved bytes, while the returned node contains old bytes

Use one new policy and EvaluationContext with an authority whose approved content can change. Load `urn:review:changing` with bytes `<approved>OLD</approved>`. Change the authority response to `<changed/>`, then load the same URI under the same policy/route.

**Observed:** the second call returns the identical cached node with string value `OLD`, but its Approved receipt SHA-256 identifies `<changed/>`, not the retained document. First digest: `7af55314edb57d23ea0ecb44ce645725c0434cc8d8ba6cec21dec39b788de943`. Second digest: `a6f97c6663c45f978a3d5ca749ec1a9aacaa11851902e08203ebe0cf36485f47`.

`LoadDocumentControlled` calls `AcquireBytes` (which records approval of fresh bytes) before checking the cache, then discards those bytes on a cache hit. This closes authorization bypass but does not preserve AC-05's exact effective-input evidence. Effective-URI changes have the same unresolved evidence/identity risk. The probe does not demonstrate new filesystem IO or failure of route denial.

**Required correction:** distinguish authorization evidence from the content actually served. Cache-hit consumption evidence must identify retained bytes and their effective URI. If new approved bytes/effective identity conflict with cached content, define an explicit deterministic policy: reject the conflict, start a fresh execution scope, or expose separate approved-versus-consumed receipts while preserving documented document identity. Do not silently present fresh approval bytes as the execution input. Braid can freeze authority envelopes per run, but that mitigation does not make the current general receipt guarantee correct.

**Required regression evidence:** same-content repeat loads preserve document identity and matching receipts; changed bytes and changed effective URIs cannot yield misleading consumed-input evidence; denial remains enforced on cache hits; policy/route changes and nested calls retain the corrected semantics. Record the chosen contract in public API documentation.

**Decision:** original blockers closed; Slice B acceptance remains pending B-F3 correction. Accepted Slice A and REQ-124 remain unchanged. No engine source edits or Braid dependency upgrade were made; broader unit/conformance gates were not independently rerun in this review.

## 5. B-F3 Closure and Slice B Acceptance (2026-10-11)

Reviewed aac4387 at Bosak HEAD 697c01e. The original changed-content probe now returns refreshed approved content with matching digest/effective-URI evidence. B-F1/B-F2 remain closed; B-F3 is verified closed. Slice B is accepted for its documented controlled acquisition contract. Whole-request acceptance remains pending C-F1/D-F1 in the end-to-end review. Independently ran 261 targeted validation/resource/capture/limit tests, all passing; no full conformance rerun or production dependency upgrade was performed.
