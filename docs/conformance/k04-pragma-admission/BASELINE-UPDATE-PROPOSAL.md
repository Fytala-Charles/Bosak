# K04-C38 — Exact pragma baseline proposal

28 October 2026. **Owner approval pending. Adoption is not applied.**

Approve the [exact two-file patch](PROPOSED-ADOPTION.patch): remove the final pragma-only QT3 routing refusal and its regex field, update the routing helper summary, append TestExecutor history **0.32**, and update AGENTS.md's mandatory baseline from **31,340/0/481** to **31,395/0/426**, with adoption provenance. [Proposal identities](evidence/proposal-identities.json) pin the patch and exact proposed snapshots against C37 commit **254d07d7**.

All **31,821 cases / 428 sets** were inventoried. Exactly **57** predicates change; two cases retain independent exact-XQ10 exclusions. All **55 eligible original assertions** pass. The complete isolated gate passes **31,395/0/426**, with exactly the [55 named skip removals](evidence/skip-delta.json), no added skips and every retained name/reason unchanged. All 24 public controls pass. Previously admitted copy/context/construction originals (**49/43/87**) pass under both executors; 16 lexical controls match under each.

The 60-case direct pragma cohort gives **59 passes / one preserved out-of-profile failure**: K-ExtensionExpression-4 expects older no-namespace semantics and remains independently excluded. XQST0079 also retains its exact-XQ10 exclusion. The patch changes no independent dependency rule or assertion to obtain green results.

Fresh normal gates remain **31,340/0/481** and **8,939/0/1,920** (230 qt4 gated sets, 11 experimental passes). Proposed and qualified routing are identical except for history wording. The bounded heuristic refusal set becomes empty; the parser continues to report invalid syntax and other semantic errors. This is a bounded catalog admission, not a claim of unrestricted XQuery grammar or feature completeness. Existing lexical helpers and all independent dependency guards remain intact.

No engine, fixture, assertion, dependency filter, KnownSkips, GatedSets, qt4 runner, schema backend, package version or release change is included. Candidate **1.33.0** remains unpublished/public **1.26.3**. Bosak.Schema owns XSD 1.1; ADR-006/REQ-139 and the six existing schema harness skips remain separately tracked. REQ-126 bounded closure and its historical observations remain intact.

**Why approval is required:** [AGENTS.md](../../../AGENTS.md) mandates exactly **31,340/0/481**. Explicit owner disposition is required before replacing that frozen baseline.

After approval, apply only these two proposed production files, synchronize adoption notices, rebuild, run fresh full solution tests and complete normal QT3/qt4 gates, and require exactly these 55 removals with every retained name/reason unchanged. Preserve failed receipts and stop adoption if results differ. Approval does not authorize a release.
