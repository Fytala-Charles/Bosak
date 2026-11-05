# K04-C01 approved quote-routing adoption

Owner disposition: **Approve exact 16-case adoption**, received 27 October 2026 for proposal commit `d0ca0741`, decision `K04-quote-routing-adoption-v1`.

The exact two-file proposed patch is applied: TestExecutor history 0.28 removes only the raw triple-double-quote refusal, retaining lexical normalization, the parser and all broader guards. AGENTS.md now requires **31,161/0/660**. No engine, fixture, assertion, dependency predicate, skip dictionary or public API changed.

The complete normal QT3 runner passes **31,161/0/660**, total 31,821. [Skip verification](skip-verification.json) proves exactly the 16 approved removals, no additions and identical names/reasons for all 660 retained skips. This is fresh production-harness evidence, separate from the preserved isolated rehearsal. The fresh qt4 gated run passes **8,437/0/1,893**, zero gated failures, 11 Experimental passes.

Solution build succeeds with zero errors (existing nullable warning and restricted vulnerability-feed warnings recorded). The initial sandbox test attempt could not create local HTTP listeners; its receipt is retained separately from the unrestricted complete run. The unrestricted complete rerun passes **22,007 tests across all 10 projects**, including 8,196 XSLT tests, with zero failures. See [validation.json](validation.json).

The original [proposal](../BASELINE-UPDATE-PROPOSAL.md), RESULTS.json and evidence manifest remain historical proposal receipts; their pending-approval fields describe that checkpoint. The original validator is a pre-adoption validator, not a current-state gate. This separate adoption receipt supersedes its status without rewriting its evidence. The prior validator can be rerun at proposal commit d0ca0741 with its original source/binaries. The adoption validator checks preserved prior bytes and exact approved source changes against that commit.

Candidate **1.26.11** remains unpublished; public **1.26.3**. REQ-126 remains Complete for its accepted bounded catalog. Bosak.Schema owns XSD 1.1. Broader K04 profile/polarity findings remain open; next is their bounded review, retaining unsupported-prolog and schema capability guards. No release, push, tag or upstream submission is included.
