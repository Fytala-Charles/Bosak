# Proposed K04-C01 quote-routing adoption
**K04-C01 adopted ? 27 October 2026:** Owner approved the exact 16-case quote-routing change proposed at `d0ca0741`. The complete normal QT3 gate passes **31,161/0/660**; exactly the 16 approved cases leave the skip set and all 660 retained names/reasons are unchanged. The qt4 gated run passes **8,437/0/1,893**, with 11 Experimental passes. All broader routing/schema/dependency guards remain unchanged. Candidate 1.26.11 remains unpublished; public version is 1.26.3. Bosak.Schema retains XSD 1.1 ownership; REQ-126 remains Complete. Next: the remaining K04 dependency-profile/polarity review. See [adoption receipt](adoption/BASELINE-ADOPTION.md) for approval and fresh validation. Earlier proposal notices are historical.


**Pending owner approval — 27 October 2026. Normal routing and AGENTS.md remain unchanged.**

Decision ID: `K04-quote-routing-adoption-v1`. Source candidate **1.26.11**, public **1.26.3**.

Approve the [exact proposed patch](PROPOSED-ADOPTION.patch): remove the raw triple-double-quote refusal in `TestExecutor.CanHandleAsXQuery`, retain its existing lexical normalization/other guards and the actual XQuery parser, and update AGENTS.md's exact QT3 expectation from **31,145/0/676** to **31,161/0/660**. The patch also appends the dated source history row. It adds no public API or dependency capability.

## Why this correction is bounded

Triple double quotes can arise from legal doubled-quote escapes, direct-constructor attributes/content, comments, CDATA, PI content and serialization declarations. A raw substring test does not establish unsupported grammar. Existing normalization and parser validation handle these cases; original malformed-input assertions still require their original errors.

All **16 exact current dependency-skip candidates** pass their original assertions in the isolated corrected harness, including the serialization declarations with the actual static output-option/comparison path. These are the cases that leave the skip set:

- `base-URI-12`
- `Constr-attr-syntax-5`
- `DirectConElemAttr-1`
- `DirectConElemNamespace-3`
- `K2-DirectConElem-47`
- `K2-DirectConElemAttr-29`
- `K2-DirectConElemAttr-40`
- `K2-DirectConElemAttr-8`
- `K2-DirectConElemContent-28`
- `K2-DirectConElemWhitespace-9`
- `K2-DirectConOther-1`
- `Literals069`
- `Serialization-adaptive-32`
- `Serialization-adaptive-33`
- `Serialization-json-41`
- `string-constructor-021`

No skip dictionary entry is removed. Correct syntax routing allows their existing dependency predicates to admit them; those predicates are unchanged. The normal direct-executor diagnostic outcomes before correction are eight skips, seven failed assertions and one pass. All 16 are currently skipped by normal admission, so direct diagnostic passes do not authorize bypassing that policy.

## Completed evidence

| Check | Result |
| --- | --- |
| All candidate assertions in corrected copied harness | 16 passed |
| Candidate process comparisons | 32 isolated processes |
| Context/construction/copy-namespace/pragma boundaries | 11 cases / 22 processes, material outcomes unchanged |
| Lexical routing controls | 13 controls, current/corrected comparison |
| Small processes with external 20-second watchdogs | 56, zero timeouts |
| Complete corrected-harness rehearsal | **31,161 passed / 0 failed / 660 skipped** |
| Full-catalog total | 31,821, unchanged |
| Exact skip delta | Only the 16 named cases removed; 660 retained names/reasons identical |

The complete actual Program/runner source is copied into an isolated diagnostic assembly; only the quote predicate's executable behavior changes. Normal source, normal binaries and current policy remain untouched. A generated-file marker is metadata only. The full rehearsal runs under an external 300-second watchdog, using the entry point's existing stack/environment setup and real fixtures/assertions.

Unsupported context, construction, copy-namespace and pragma routing guards are retained. Lexical controls preserve those exclusions even when quotes occur in comments; accepting a malformed expression for routing does not make it valid—the parser/assertion still decides its outcome. Schema, version, feature, namespace-axis, retry and dependency policies remain unchanged. The broader K04 profile/polarity findings remain open.

Source-identical inherited validation remains **22,007 solution passes** and qt4 **8,437/0/1,893**, 224 gated sets and 11 existing Experimental passes. This harness-only review does not claim fresh unit or qt4 runs. [RESULTS.json](RESULTS.json) and the [manifest](evidence/manifest.json) protect full results, exact skip delta and prior evidence.

## Approval and acceptance

Approval authorizes applying this exact routing/baseline change, synchronizing current documentation, rebuilding the normal harness and rerunning the **complete normal QT3 gate**. Adoption completes only with **31,161/0/660**, all 16 named cases admitted, no added skips and identical reasons for all 660 retained skips. Unexpected failures prevent adoption; assertions and other guards must not be weakened.

Explicit owner approval is required because [AGENTS.md](../../../AGENTS.md) currently mandates exactly **31,145/0/676**. The successful isolated rehearsal does not change that rule.

Approval does not publish, tag, push or merge externally. Candidate1.26.11 remains unpublished. Bosak.Schema retains XSD 1.1 ownership; the schema profile/capability mismatch is not resolved by this routing correction. REQ-126 remains Complete for its accepted bounded catalog. No fixture, assertion, unknown-capability policy, broader prolog support, current-draft certification or upstream submission is included.
