# K04-C23 - Normalization admission review

28 October 2026. **Proposal complete; adoption awaits explicit owner approval.** Source base `25d6d817`; candidate **1.29.2** remains unpublished/public **1.26.3**.

C21 repaired FULLY-NORMALIZED conversion and C22 repaired form-name/default/keyword behavior. This review qualifies the unchanged original tests for a precise admission change. It preserves every C22 artifact and inherits its complete 23,746-test solution validation; no engine or unit source changed and no fresh solution test run is claimed.

The [exact proposal](BASELINE-UPDATE-PROPOSAL.md) removes one repaired QT3 KnownSkip and its incorrect comment, promotes only the green qt4 normalization set, and updates only AGENTS.md's required QT3 total. Normal source and policy are untouched. [Reproducible preparation](evidence/prepare-rehearsal.py) records the literal three-file patch and two isolated complete harnesses. Every other copied source file matches normal bytes, and copied engine DLLs match current normal binaries.

## Fresh isolated receipts

| Scope | Result (passed/failed/skipped) | Verified delta |
|---|---|---|
| Complete QT3 gate | **31,165/0/656** | Only positive `cbcl-fn-normalize-unicode-006` leaves skips |
| Complete qt4 gate | **8,939/0/1,920** | Add normalization's 44 passes and six existing dependency skips |
| QT3 normalization set | **43/0/5** | One repaired positive becomes a pass |
| qt4 normalization set | **44/0/6** | All admitted original cases remain green |
| Positive-case substring scope, each suite | **1/0/1** | Positive passes; related unsupported negative remains skipped |

All six processes finish within their watchdogs. The qt4 gate has **230 proposed sets**, with 11 Experimental passes unchanged. All 656 retained QT3 skip names/reasons and all 1,914 pre-existing qt4 skip names/reasons are identical to C22. The six newly gated qt4 skips match the previously exploratory normalization set exactly. The [98-case inventory](evidence/normalization-inventory.json) preserves expressions, assertions and dependencies; fixture revisions and raw source hashes are pinned.

The positive Bengali fixture expects exactly codepoints **32,2494**. The leading space is required by FULLY-NORMALIZED conversion; its former upstream-defect comment is incorrect. Both proposed normal runners pass the unchanged assertion. Negative assertions conditioned on lack of optional-form support remain excluded. This is no claim that every dependency is supported or that skipped assertions pass directly. No upstream correction is justified.

[Evidence validation](evidence/validate-evidence.py) checks proposed patch reconstruction/application, unchanged normal source/policy, copied sources/binaries, original fixture hashes, gate membership, all totals, exact skip maps, preserved C22 evidence and [manifest identities](evidence/manifest.json). Render receipts cover ten Markdown pages using unchanged Fytala CSS; HTML generation is not screenshot/PDF acceptance.

## Next action

Owner approval of [the exact baseline proposal](BASELINE-UPDATE-PROPOSAL.md) is required before applying it because AGENTS.md mandates the current QT3 total. Then apply that patch and run fresh actual normal gates and affected sets, requiring exact agreement with these rehearsals. No release is included.

REQ-126 remains Complete under its existing bounded acceptance, with every original disposition and all 48 historical unmet batch observations preserved. Bosak.Schema retains staged XSD 1.1 ownership. No engine, filter, fixture/assertion, version, schema, publication or Braid change is part of this review.
