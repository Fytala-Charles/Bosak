# K04-C23 - Owner-approved normalization adoption

28 October 2026. **The exact owner-approved normalization admission change is adopted.** The owner selected `Approve exact normalization adoption (Recommended)` for the proposal at **ee19ebbf**. The [approval receipt](approval.json) records decision `K04-normalization-adoption-v1`. [Proposal identities](proposal-identities.json) and presented document snapshots preserve the original proposal, exact patch, inventories and isolated execution receipts separately from this adoption record.

The normal QT3 runner now removes only repaired KnownSkip `cbcl-fn-normalize-unicode-006` and its incorrect upstream-defect comment (history 1.46). The normal qt4 runner promotes only `fn-normalize-unicode` (history 0.16), bringing the gate to **230 sets**. AGENTS.md now requires QT3 **31,165/0/656**. Both original positive Bengali assertions are unchanged and pass with their required leading space, codepoints **32,2494**.

## Fresh actual normal verification

| Scope | Passed / failed / skipped | Verification |
|---|---|---|
| Complete QT3 gate | **31,165 / 0 / 656** | Only the repaired positive leaves skips; all 656 retained names/reasons match |
| Complete qt4 gate | **8,939 / 0 / 1,920** | 44 normalization passes and six existing dependency skips added; all prior 1,914 skip names/reasons match |
| Complete QT3 normalization set | **43 / 0 / 5** | All five remaining dependency skips preserved |
| Complete qt4 normalization set | **44 / 0 / 6** | All six dependency skips preserved |
| Positive-case substring scope, each suite | **1 / 0 / 1** | Positive passes; related conditional negative stays skipped |

All six bounded processes exit successfully without timeout. All totals and complete skip mappings match the approved isolated rehearsals exactly. The qt4 gate retains 11 Experimental passes. Conditional unsupported-form negatives remain excluded; no assertion or dependency guard is weakened.

The full solution build and a fresh complete solution test run pass: **23,746 tests**, including **13,797 Standard** and **8,196 XSLT** tests. The normal engine binaries and all frozen fixture revisions/hashes are recorded. [Validation](validate-adoption.py) reconstructs the exact approved patch from its source base, checks that no other engine/test source changed, compares all fresh normal receipts against rehearsal, and verifies [manifest identities](manifest.json).

Candidate **1.29.2** stays unpublished; public version is **1.26.3**. No engine, package version, fixture/assertion, dependency filter, serializer, schema/backend, release, publication, push, tag, upstream or Braid change is included. Bosak.Schema retains staged XSD 1.1 ownership. REQ-126 remains Complete under bounded owner acceptance; all historical dispositions and 48 unmet batch observations are preserved. Earlier pending-proposal notices and results are historical.

This completes the normalization conversion, options and exact-admission work. Next, resume the remaining conformance-skip review from the canonical triage record; further baseline changes require their own concrete evidence and disposition.
