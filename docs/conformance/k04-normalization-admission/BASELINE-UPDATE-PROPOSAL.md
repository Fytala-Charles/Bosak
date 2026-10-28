# K04-C23 normalization admission proposal

28 October 2026. **Pending owner approval. Normal runners and AGENTS.md remain unchanged.** Decision ID: `K04-normalization-adoption-v1`. Source base `25d6d817`; candidate **1.29.2** unpublished/public **1.26.3**.

Approve the [exact three-file patch](PROPOSED-ADOPTION.patch):

1. Remove only QT3 KnownSkip `cbcl-fn-normalize-unicode-006` and its incorrect upstream-defect comment. The unchanged original requires U+0020 followed by U+09BE, and passes after C21's FULLY-NORMALIZED correction. Append runner history 1.46.
2. Promote only `fn-normalize-unicode` into qt4 `GatedSets`, from **229 to 230 sets**, with runner history 0.16. All 44 admitted original cases pass; its six existing dependency skips remain excluded with their original reasons.
3. Update AGENTS.md's exact QT3 expectation from **31,164/0/657** to **31,165/0/656**.

## Reviewable result

- Complete isolated QT3 gate: **31,165/0/656**. Only the repaired positive skip disappears; all 656 retained names/reasons are identical to C22.
- Complete isolated qt4 gate: **8,939/0/1,920**, versus 8,895/0/1,914. Exactly 44 passes and six existing exclusions enter the gated scope. All 1,914 previous skip names/reasons remain identical; 11 Experimental passes remain unchanged.
- Complete normalization sets: QT3 **43/0/5**, qt4 **44/0/6**. The proposed normal path passes the unchanged positive assertion in both suites. The harness substring filter also selects its negative `...006a` companion, which stays skipped: each focused receipt is **1/0/1**.
- The 98-case normalization inventory preserves every expression, assertion and dependency. Conditional unsupported-form negatives remain excluded; no assertion is changed or weakened. This is zero failures among admitted original cases, not a claim that excluded cases execute successfully.
- Six bounded execution processes finish without timeout. Copied complete harness sources preserve parsing, environments, comparers, executors and filters. Only the proposed runner edits differ; copied engine DLLs match the normal binaries exactly.
- C22's **23,746 solution passes**, including 246 new option regressions and 8,196 XSLT tests, are inherited source-identical validation. This review makes no fresh full-solution-run claim. The prior 62 control passes and C21 Unicode corpus qualification remain preserved.

[Review](README.md), [results](RESULTS.json), [exact skip deltas](evidence/skip-delta.json), [positive scalar witnesses](evidence/positive-witnesses.json), [source pin](evidence/REVIEW-PIN.json) and [manifest](evidence/manifest.json) contain the receipts.

## Approval and completion boundary

Approval authorizes applying only this exact patch, synchronizing current documentation, building both actual normal harnesses, and rerunning fresh complete normal QT3 and qt4 gates plus both complete normalization sets. Adoption completes only with QT3 **31,165/0/656**, qt4 **8,939/0/1,920**, 230 gated sets, normalization sets **43/0/5** and **44/0/6**, and every retained skip name/reason matching rehearsal. Unexpected differences prevent closure; investigate them without weakening tests or guards.

Explicit owner approval is required because [AGENTS.md](../../../AGENTS.md) currently mandates exactly **31,164/0/657** for QT3. The isolated rehearsal does not change that requirement. This proposal requests that exact disposition and the paired qt4 set promotion; no release is approved.

No engine, dependency-filter, fixture/assertion, serializer, schema/backend, package version, publication, push, tag, upstream or Braid change is included. Public version remains **1.26.3**. Bosak.Schema owns staged XSD 1.1 work. REQ-126 remains Complete under bounded owner acceptance; all historical dispositions and 48 unmet batch observations are preserved.
