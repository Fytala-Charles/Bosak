# K04-C40 — Exact xib dependency baseline proposal

28 October 2026. **Owner approval pending. Adoption is not applied.**

Approve the [exact three-file patch](PROPOSED-ADOPTION.patch): recognize canonical **`language=xib`** as known unsupported in the QT3 and qt4 filters (positive refuses, negative admits), append histories **0.19 / 0.9**, and update AGENTS.md's mandatory QT3 expectation from **31,395/0/426** to **31,397/0/424**, with adoption provenance. [Proposal identities](evidence/proposal-identities.json) pin the base revision, exact patch and proposed snapshots.

All **78,806 cases** were inventoried under normal and isolated filters. Exactly **format-date-en151** and **format-dateTime-en151** leave each suite's skip map; all four original assertions pass in both actual executors. The isolated complete gates pass **QT3 31,397/0/424** and **qt4 8,941/0/1,918**, with every retained name/reason unchanged. Both qt4 originals are already gated; all 230 GatedSets members and 11 experimental passes remain unchanged.

The actual-filter matrix checks **854 requests / 1,281 applicable profile outcomes per phase**, including normal-filter projection oracles for conjunction, polarity, order, repeats and unrelated rules. Twelve public formatting controls pass. Normalization/default-language counter-profile failures remain preserved and excluded. Proposed filter predicates are identical to the isolated rehearsal, differing only in history wording.

This is a narrow catalog-capability policy for the canonical xib token. Every other language spelling/value, dependency kind, feature polarity and exact-version policy retains current behavior. Positive xib metadata is refused even though no current positive fixture exists. No global unsupported-language rule, engine change, fixture/assertion correction, KnownSkips change, GatedSets promotion, runner change, schema backend, package version or release is included. Candidate **1.33.0** remains unpublished/public **1.26.3**. Bosak.Schema owns XSD 1.1, ADR-006/REQ-139 remains separately pending, and REQ-126 bounded closure/48 historical observations remain intact.

**Why approval is required:** [AGENTS.md](../../../AGENTS.md) mandates exactly **31,395/0/426**. Explicit owner disposition is required before replacing that frozen QT3 baseline and applying the reviewed filter policy.

After approval, apply only these three proposed production files, synchronize adoption notices, rebuild, run fresh full solution tests and both complete normal gates, replay the actual-filter matrix, and require exactly these two removals per suite with every retained name/reason unchanged. Preserve failed receipts and stop adoption if results differ. Approval does not authorize a release.
