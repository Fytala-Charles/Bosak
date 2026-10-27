# Proposed K04-C16 CB calendar adoption

**K04-C16 adopted ? 27 October 2026:** Owner approved the exact proposal at 6df9a58b. [Fresh normal adoption receipt](adoption/BASELINE-ADOPTION.md) records QT3 **31,164/0/657**, with only the two original CB skips removed and all 657 retained reasons identical. The qt4 gate stays **8,587/0/1,892**; combined exploratory date/dateTime is **208/6/18**, preserving all six failures and 18 other skips, with no promotion. All four originals, 364 production filter controls and 40 projected controls match approval. AGENTS now requires **31,164/0/657**. All **22,579 solution tests** pass, including **8,196 XSLT tests**. Engine, fixtures/assertions, 226 GatedSets, schema and version remain unchanged. Candidate 1.28.1 unpublished/public 1.26.3; Bosak.Schema owns staged XSD 1.1 and REQ-126 remains Complete. Next: missing date-formatting arities 3/4 contract review. Earlier pending-proposal notices are historical.

27 October 2026. **Pending owner approval. Normal source and AGENTS.md remain unchanged.**

Decision ID: **K04-CB-calendar-adoption-v1**. Source base **870dd275**; candidate **1.28.1** unpublished/public **1.26.3**.

Approve the [exact three-file patch](PROPOSED-ADOPTION.patch): recognize the reviewed unsupported `calendar=CB` predicate in both filters, refuse positive declarations and admit negative declarations while retaining all other guards; append source history 0.18/0.8; update AGENTS.md's exact QT3 expectation from **31,162/0/659** to **31,164/0/657**.

The rule removes only format-date-en152 and format-dateTime-en152 from the QT3 skip set. Both corresponding qt4 cases pass through the proposed runner too, but they belong to exploratory sets: **the qt4 gated baseline stays 8,587/0/1,892**. No GatedSets promotion is proposed. Other calendar names and dependency kinds retain their current admission policy.

## Reviewable evidence

- The complete 78,806-case catalog inventory finds exactly four calendar declarations, all negative CB originals. There is no positive CB fixture acquiring a new skip.
- All four direct original assertions and all four isolated normal-path originals pass. The unchanged actual normal runners still skip them.
- Actual private classifier observations distinguish CB fallback from implemented AD/ISO paths. Existing AD fallback is not CB conversion capability.
- **364 before/after filter controls**, 538 profile outcomes per phase, preserve unrelated policy. All 28 changed outcomes concern exact CB-bearing controls; the actual-normal-filter projection oracle verifies all 40 such controls.
- Fresh complete isolated QT3 rehearsal passes **31,164/0/657**, with only the two approved names removed and all 657 retained skip names/reasons identical to K04-C15.
- Fresh complete isolated qt4 gate passes **8,587/0/1,892**, preserving every gated skip name/reason, all 226 gated sets and 11 Experimental passes.
- Fresh combined exploratory date/dateTime runs improve **206/6/20 ? 208/6/18**. All six failure names/messages and 18 retained skip names/reasons are unchanged. Those failures remain open and the sets remain ungated.

All 25 diagnostic/rehearsal processes complete without timeout. Copied complete harness sources preserve parsing, environments and comparisons; only the filter rule/history differs. [Review](README.md), [results](RESULTS.json), [skip deltas](evidence/skip-delta.json) and [manifest](evidence/manifest.json) provide the receipts. K04-C15's **22,579 solution passes** and 381 regression rows are inherited source-identical validation; no fresh unit run is claimed.

## Approval and completion boundary

Approval authorizes applying only this exact patch, synchronizing current documentation, building actual normal harnesses and running fresh complete normal QT3 and qt4 gates plus the affected exploratory sets. Adoption completes only when normal QT3 is **31,164/0/657**, qt4 gate remains **8,587/0/1,892**, all four originals pass normally, the two exploratory sets report **208/6/18** with the same six failures, and every retained skip name/reason matches rehearsal. Unexpected differences prevent completion; fixtures, assertions and unrelated guards must not be weakened.

Explicit approval is required because [AGENTS.md](../../../AGENTS.md) mandates exactly **31,162/0/659** before this change. A copied-harness rehearsal does not update that policy. This proposal requests that exact owner disposition; it does not approve a release.

No engine behavior, public API, fixture/assertion, known-skip dictionary, GatedSets, schema/backend, package version, publication, push, tag, upstream or Braid change. Candidate 1.28.1 stays unpublished/public 1.26.3. Bosak.Schema owns staged XSD 1.1; REQ-126 remains Complete under its existing bounded acceptance. The separate empty-value wrapper defect and other capability findings remain open.
