# Proposed K01 QT3 baseline update

**Pending owner approval — 27 October 2026. The current baseline remains unchanged.**

Approve removal of exactly these two entries from `KnownSkips` in `tests/Bosak.XPath.Conformance/ConformanceRunner.cs`:

| Case | Released direct executor | Candidate direct executor | Original assertion |
|---|---|---|---|
| CastAs-ListType-20 | Failed: XPTY0004 | Passed | XPST0051 |
| CastAs-ListType-28 | Passed | Passed | assert-true |

The first case has the new bounded binding-type correction; the second already worked in the released source. No dependency filter, XQuery routing, fixture, assertion, timeout or Experimental retry change is proposed. The two original skip reasons remain in the historical triage and execution receipts.

Current frozen QT3 policy is **31,142 passed/0 failed/679 skipped**. Removing these two now-passing skips would produce **31,144 passed/0 failed/677 skipped**, retaining 31,821 total cases. This is a proposed result, not a completed full run under revised admission. The qt4 gate remains **8,436/0/1,894**, with 224 gated sets and the existing 11 Experimental passes.

Approval authorizes updating the exact QT3 requirement in [AGENTS.md](../../../AGENTS.md), removing only the two named known-skip entries, updating their current triage status and rerunning the complete QT3 gate against the revised expectation. Historical receipts and the REQ-126 accepted catalog remain unchanged. Any unexpected failure prevents adoption; no assertion is relaxed to reach the proposed totals.

This decision is needed because AGENTS.md currently requires the exact frozen QT3 total. The runtime correction and its validation can be reviewed independently of the baseline change. No tag, merge, push or release is requested by this proposal.
