<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak REQ-132: literal element acceptance matrix">
<h1>REQ-132: literal element acceptance matrix</h1>
<p>Planned verification - 18 October 2026</p>
</div>

**Implementation update:** Observed source/package checks belong in the [implementation record](REQ-132-IMPLEMENTATION.md) and its delivery handoff. The matrix below remains the reviewed plan, including unexecuted Braid/native responsibilities.


**Reviewed acceptance plan.** Current observed engine/package results are recorded separately; consumer/native groups remain independent work. Review with the [contract](REQ-132-LITERAL-ELEMENT-CONTRACT.md) and [original consumer LE groups](REQ-132-literal-result-element-insertion.md). Public 1.19.1 has no literal-element insertion capability.

Each engine case needs positive and refusal/control fixtures where applicable. UI/session/license/native cases belong to Braid; engine tests cannot certify them. Verify expanded XML names/result semantics separately from exact stylesheet source bytes. Preserve failing receipts; record fixture corrections separately rather than replacing the original evidence.

| Case | Consumer group | Fixture | Required assertion |
|---|---|---|---|
| E01 | LE-01 | Happy path: explicit template and nested LRE | One actual paired no-namespace element appended; exact parent, name and empty body. |
| E02 | LE-01/11 | Unsupported target shapes | Self-closing parent, text/comment/PI descriptor, instruction ancestor, dependency and unrelated module refuse with the appropriate existing class. |
| E03 | LE-01/07 | Ownership and stale session | Foreign equal-byte inspection and foreign target refuse; old owner can propose to itself, while consumer stale Apply rejects. |
| E04 | LE-02 | Name grammar and Unicode | ASCII, non-ASCII and supported supplementary/combining NCNames retain exact spelling; invalid leading digit/space/colon/markup/EQName, illegal scalar and unpaired surrogate return InvalidElementName. Include non-normalized equivalent spellings without canonicalization. |
| E05 | LE-02 | Literal identity versus labels | value-of, text, template and legal xml/xmlns local spellings are classified by empty namespace, not as XSLT or declaration syntax. No invented reserved-name blacklist. |
| E06 | LE-02/03 | Default namespace reset | Nonempty ordinary default and default XSLT namespace each generate local xmlns="" only on the inserted element; already-empty default adds none. Existing bindings and surrounding result names remain unchanged. |
| E07 | LE-02/11 | Namespace and preprocessing hazards | Namespace aliases including #default, extension-element-prefixes and unsupported preprocessing refuse UnsupportedCoverage before evaluation; no resource requests or source rewrite. |
| E08 | LE-03 | Source structure and caret | Opening and closing names match exact accepted spelling; no self-close, body whitespace, synthetic text child or Expression slot; zero-byte BodyRange immediately between tags. |
| E09 | LE-04 | Four physical encoding variants | UTF-8 no-BOM/BOM and BOM-marked UTF-16 LE/BE retain source/declaration/BOM; all ranges use physical bytes and existing line/column rules. |
| E10 | LE-04/11 | Encoding refusal | Unsupported structural encoding, declaration mismatch and invalid source byte sequences refuse at the documented inspection/insertion boundary; no lossy fallback or conversion. |
| E11 | LE-04 | Source fidelity and replay | LF/CRLF/lone CR, comments/PIs, quotes, entity spellings, existing aliases/prefix spellings and non-ASCII before anchor; ordered complete edits replay exactly, every untouched gap matches bytes. |
| E12 | LE-05 | Total old-node correspondence | Map all old ancestors, elements, comments/PIs, text and layout nodes injectively; preserve parent/order and attribute metadata, allowing only declared layout text/range differences. |
| E13 | LE-05 | Complete new descriptor partition | Candidate = mapped old nodes disjoint-union NewNodes; exactly inserted LRE plus new layout text. Namespace reset appears as actual attribute metadata, not node; body caret has no node. |
| E14 | LE-05/11 | Alignment and malformed evidence | Missing/duplicate/out-of-snapshot mappings, omitted layout nodes, wrong parent/name/body/ranges and impossible text merge/split are refused by engine alignment or rejected by consumer review, according to authority boundary. |
| E15 | LE-06 | PreserveSource default compatibility | Default and explicit PreserveSource produce identical bytes; one append edit retains all original bytes and discloses reset within inserted range. |
| E16 | LE-06 | Readable supported fixtures | Compact two-space fallback and uniform existing indentation/newlines; all layout edits disclosed, no indentation within paired body or protected text/comments/PIs; execution matches same insertion in PreserveSource. |
| E17 | LE-06/11 | Readable/mixed-content refusal | Significant direct parent text, direct CDATA, effective preserve; ancestor preserve even under default reset for Readable, mixed newline/indent ambiguity; exact failure, no implicit fallback. |
| E18 | LE-07/11 | Limits and overflow | At/over 8,388,608-byte original/final boundaries; long Unicode names appear twice in emission; UTF-16 bytes, reset and layout counted, shrink-to-fit Readable case; overflow-safe early refusal. |
| E19 | LE-07 | Deep immutability and concurrency | Mutating exported bytes or input holders cannot change target/candidate/evidence; readonly collections cannot expose backing storage; concurrent proposals isolated. |
| E20 | LE-07/11 | Cancellation | Pre-cancelled discovery/proposal and observed traversal/validation/pre-success cancellation return Cancelled with no candidate. Consumer cancels before Apply atomically; cancellation after committed Apply does not reverse success. |
| E21 | LE-08 | Review, permissions and history | Compatible adapter and separate capabilities, bounded complete review, name/mode/source changes invalidate evidence, explicit licensed current-revision Apply; actual new element/body selection, graph reopen, exact one-step undo/redo with fresh inspection. |
| E22 | LE-09 | Compose both existing child operations | Insert LRE then independently discover fresh value-of and text targets on actual new node, including namespace-reset fixture. Populate with each operation, retain reset/paired markup and old identities; each Apply gets its own history step. |
| E23 | LE-09 | Repeated siblings and nesting | Repeat sibling element append and nested container creation; never reuse prior targets across snapshots; correspondence and undo/redo remain exact for every accepted step. |
| E24 | LE-09/11 | Existing capability regressions | Prior expression/value-of/text/deletion behavior, bytes and failures remain unchanged, including broader legacy value-of xsl:text controls; unsupported new-operation context does not narrow old APIs. |
| E25 | LE-10 | Real worker/output goldens | Use XML-output fixtures: original, empty-element and populated-element results differ as intended; independently assert expanded names, namespace bindings and content, not serializer empty-tag spelling. Compare modes and four encodings; validate/preview ExactBytes/export receipt/hash/reopen/run. |
| E26 | LE-11 | Validation and resource authority | Invalid original/candidate, unsupported static/component context and denied resource paths preserve bytes/authority; name is never evaluated as XPath/AVT. Instrument resolver/extension calls and assert none during proposal. |
| E27 | LE-12 | Native consumer acceptance | Protected kit/manifests, explicit walkthrough, immediate actual export receipt/file hashes, independent reopen/result observations; no fabricated variants or acceptance from upstream tests. |
| E28 | LE-01 through LE-12 | Delivery separation | Full solution and separate LS tests, exact-head CI, unchanged QT3 31,142/0/679 and qt4 gated 8,278/0/1,894, independent exact-merge package consumer; later signed public release and Braid/native acceptance remain separate. |

Counts above identify **28 planned groups**, not 28 tests or passed runs. Implementations may need multiple tests per group; package-public cases must avoid friend/internal access. Frozen conformance numbers are delivery requirements, not results from this documentation slice.
