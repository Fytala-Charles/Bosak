# K04-C16: bounded CB calendar admission review ? 27 October 2026

**K04-C16 adopted ? 27 October 2026:** Owner approved the exact proposal at 6df9a58b. [Fresh normal adoption receipt](adoption/BASELINE-ADOPTION.md) records QT3 **31,164/0/657**, with only the two original CB skips removed and all 657 retained reasons identical. The qt4 gate stays **8,587/0/1,892**; combined exploratory date/dateTime is **208/6/18**, preserving all six failures and 18 other skips, with no promotion. All four originals, 364 production filter controls and 40 projected controls match approval. AGENTS now requires **31,164/0/657**. All **22,579 solution tests** pass, including **8,196 XSLT tests**. Engine, fixtures/assertions, 226 GatedSets, schema and version remain unchanged. Candidate 1.28.1 unpublished/public 1.26.3; Bosak.Schema owns staged XSD 1.1 and REQ-126 remains Complete. Next: missing date-formatting arities 3/4 contract review. Earlier pending-proposal notices are historical.

**Review complete; owner approval pending.** The four original CB assertions pass directly and through an isolated proposed runner, while the unchanged normal runners dependency-skip them. The proposed admission rule recognizes only the reviewed unsupported `calendar=CB` capability: positive dependencies are refused, negative dependencies are admitted subject to all other guards. It is keyed by dependency type/value, not test names.

## Catalog and capability evidence

All 78,806 catalog cases were scanned with inherited and local declarations. Exactly four calendar declarations exist: negative CB dependencies on format-date-en152 and format-dateTime-en152 in each profile. There is no positive CB declaration acquiring a new skip. [Inventory](evidence/calendar-inventory.json) records every scanned test-set hash and each declaration.

The actual private formatter classifier reports CB fallback in both shared modes, with AD and ISO supported-path contrasts. Sixteen observations preserve unsupported namespaced/listed and malformed/unlisted distinctions. K04-C15 already implements Gregorian AD fallback; it does not implement CB calendar conversion. Its pinned semantic adoption and 381 passing regressions are inherited source-identical evidence, separate from this admission review.

The actual filters execute **364 controls / 538 profile outcomes per phase**. Twenty-eight outcomes change across 21 rows, all containing exact CB dependencies. A further actual-normal-filter oracle checks all 40 CB-bearing controls after removing only CB predicates, proving the candidate applies the intended positive refusal/negative admission while retaining the other conjunctions. Unknown calendar names, AH/AD/ISO, case variants, schema/profile guards, unknown features, language predicates and the adopted numbering-sequence rule keep their existing policy. Legacy permissive positive admission for other dependency kinds is not a support certification.

## Complete runner observations

| Scope | Current evidence | Proposed isolated runner | Exact change |
| --- | --- | --- | --- |
| Complete QT3 | 31,162 / 0 / 659 | **31,164 / 0 / 657** | Only the two CB skips removed; 657 retained names/reasons identical |
| Complete qt4 gated sets | 8,587 / 0 / 1,892 | **8,587 / 0 / 1,892** | No gated case or skip change |
| Combined exploratory qt4 date/dateTime sets | 206 / 6 / 20 | **208 / 6 / 18** | Only the two CB skips removed; 18 retained reasons and all six failures identical |

Values are passed / failed / skipped. The existing runner's `fn-format-date` substring selects both date and dateTime sets (232 cases); the additional narrower dateTime run independently reports 76/2/16 ? 77/2/15. These are overlapping scopes, not independent case totals. The six exploratory failures concern unavailable format-date/dateTime arities 3/4. They remain failures, and the sets stay ungated. Process exit zero for these exploratory runs is not an assertion-pass claim. All 226 GatedSets and 11 Experimental passes remain unchanged.

Four direct-original, four normal single-case and four proposed single-case checks independently confirm the skip-to-pass transitions. The copied executables preserve complete actual runner/parser/environment/comparison sources; only the proposed filter source differs. Twenty-five diagnostic/rehearsal processes complete under external watchdogs with zero timeouts. Complete rehearsal gates are fresh; current full gates and **22,579 solution passes** are inherited from unchanged K04-C15 source, not newly repeated. Both actual current and candidate exploratory sets are fresh runs.

## Disposition

The exact [baseline proposal](BASELINE-UPDATE-PROPOSAL.md) is ready for owner disposition. Normal filters and AGENTS.md remain unchanged at **31,162/0/659** pending approval of the dry three-file patch. No engine, fixture/assertion, GatedSets, schema, version or release change. Candidate **1.28.1** remains unpublished; public **1.26.3**. REQ-126 remains Complete and Bosak.Schema retains staged XSD 1.1 ownership. Empty-value wrapper, language admission and FULLY-NORMALIZED findings remain separate; missing date-formatting arities need a later contract review.

Source base: 870dd275. Delivery revision: `git log -1 --format=%H -- docs/conformance/k04-calendar-admission/BASELINE-UPDATE-PROPOSAL.md`. [RESULTS.json](RESULTS.json), [skip deltas](evidence/skip-delta.json), raw receipts, binary/source identities and the manifest preserve the review. Earlier C13?C15 raw evidence is protected without rewriting its outcomes.

Run `python docs/conformance/k04-calendar-admission/evidence/validate-evidence.py` to verify the inventory, exact candidate patch, original outcomes, controls, complete rehearsals and inherited identities. HTML renderer receipts use unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.
