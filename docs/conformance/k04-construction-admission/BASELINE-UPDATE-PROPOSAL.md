# K04-C35 — Exact construction baseline proposal

28 October 2026. **Owner approval pending. Adoption is not applied.**

Approve the [exact two-file patch](PROPOSED-ADOPTION.patch): remove the QT3 construction-routing refusal and its now-empty regex field, append TestExecutor history 0.31, and update AGENTS.md's mandated expectation from **31,257/0/564** to **31,340/0/481**, with adoption provenance. [Proposal identities](evidence/proposal-identities.json) pin the exact patch and proposed snapshots against C34 commit `994b4e33`.

All **31,821 cases / 428 sets** were inventoried. Exactly **87** predicates change; four retain independent exact-XQ10 exclusions. All 87 original assertions pass directly. The complete isolated gate passes **31,340/0/481**, with exactly the [83 named removals](evidence/skip-delta.json), no added skips and every retained reason unchanged. The 49 copy and 43 context assertions pass under both executors; 19 lexical controls pass under each.

Fresh normal gates remain **31,257/0/564** and **8,939/0/1,920** (230 qt4 sets, 11 experimental passes). The proposed executor differs from the qualified prototype only in its history wording. The pragma guard and all independent dependency guards remain intact. The unused comment-normalization helper is retained to avoid unrelated cleanup.

This proposal includes no engine, fixture, assertion, dependency filter, KnownSkips, GatedSets, qt4 runner, schema backend, version or release change. Candidate **1.32.0** remains unpublished/public **1.26.3**. Bosak.Schema owns XSD 1.1; ADR-006/REQ-139 remains separately pending. REQ-126 bounded closure and historical observations remain intact.

**Why approval is required:** [AGENTS.md](../../../AGENTS.md) mandates exactly **31,257/0/564**. Explicit owner disposition is required to replace that frozen baseline.

After approval, apply only these two proposed production files, synchronize the adoption notices, rebuild, run the full solution tests and fresh complete normal QT3/qt4 gates, and require exactly these 83 removals with all retained names/reasons unchanged. Preserve any failed receipt and stop adoption if the evidence differs. Approval does not authorize a release.
