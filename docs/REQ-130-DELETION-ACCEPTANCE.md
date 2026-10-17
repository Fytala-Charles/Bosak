<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala value-of deletion acceptance plan">
<h1>REQ-130: deletion acceptance plan</h1>
<p>Planned evidence — 17 October 2026</p>
</div>

**Pending design: none of these groups is claimed as an implemented or passed test.** Read the [proposed contract](REQ-130-DELETION-CONTRACT.md) and [unchanged consumer request](REQ-130-source-preserving-value-of-deletion.md). Each group requires positive and refusal/edge evidence where applicable. Preserve initial failures and corrections separately.

| Group | Required evidence |
|---|---|
| D01 | **Target authority:** Exact owned imported/previous-candidate target; foreign descriptor, equal IDs/bytes, wrong module and snapshot refuse. |
| D02 | **Placement:** First/middle/last/sole compact, indented and readable-inserted instructions succeed within the bounded map/merge profile; root/top-level/non-template chain refuse. |
| D03 | **Spellings:** Alias/default/rebound XSLT namespace and self-closing/explicit empty body; literal same-name element refuses. |
| D04 | **Body boundary:** Empty body succeeds; whitespace/comment/PI/CDATA/text/element body refuses without candidate. |
| D05 | **Exact edit:** One full removal/empty replacement; byte replay and all unchanged spans, no mutation or cleanup. |
| D06 | **Empty parent:** Sole deletion retains explicit tags and maps parent; no self-close rewrite or invented no-parent target. |
| D07 | **Text coalescence:** Two neighboring whitespace descriptors produce one explicit two-fragment merge group, including readable-inserted and indented sole/first/middle/last cases; preserve all whitespace bytes. |
| D08 | **Permitted trivia:** One-sided text or separating comment/PI/element retains exact distinct nodes and bytes; full graph still proved. |
| D09 | **Identity partition:** Original graph partitions into removed, one-to-one and merge participants; candidate graph partitions into map image and merge destinations, with no overlap/gaps; injectivity outside merges. |
| D10 | **Identical siblings:** Repeated equal value-of siblings map by source/structure, not ordinal/text alone; removed numeric ID reuse is not identity reuse. |
| D11 | **Coordinates:** Byte/UTF-16/line/column maps, astral Unicode, entities and mixed physical newline offsets before/at/after deletion. |
| D12 | **Encodings:** UTF-8 BOM/no-BOM, UTF-16 LE/BE BOM/declarations; strict mismatch/invalid byte/unsupported encoding refusals. |
| D13 | **Source spelling:** CR/LF/CRLF, tabs, blank lines, quoted markup, prefix/namespace/declaration/URI and unused bindings retained. |
| D14 | **Profile coverage:** Captured explicit 3.0/4.0 profiles and supported attributes/expressions; unsupported slots/versions/types/components refuse. |
| D15 | **Mixed/preserved contexts:** Significant text/CDATA and ancestor preserve, including nearer default reset, refuse; no whitespace normalization. |
| D16 | **Resource denial:** Dependencies, DTD/entities/static preprocessing/use-when/shadow/extension/fixed namespace exclusions fail before external execution/access. |
| D17 | **Original/candidate validity:** Invalid original never repaired by deletion; invalid/unsupported candidate returns classified no-candidate failure. |
| D18 | **Size/cancellation:** Original and candidate boundary including BOM, oversized original, pre-cancel and cancellation in scans/validation/map; no partial evidence. |
| D19 | **Immutability/concurrency:** Defensive bytes/lists, concurrent proposals on immutable snapshot, no shared current-revision illusion. |
| D20 | **Changed output:** Hand-written expected changed ExactBytes output, static validation, worker execution, new-file export/hash and independent reopen. |
| D21 | **Compatibility/delivery:** Existing insertion/expression/default behavior, capability adapter on old packages, full tests/frozen gates and exact-head public-only consumer. |
| D22 | **Braid review/history:** Complete bounded review, current authority, mandatory map-plus-merge adapter verification, explicit Apply, stale/denied/cancelled/overlap refusals, parent selection and exact one-step undo/redo. |
| D23 | **Native acceptance:** Separate native review/keyboard/theme/cancel/retry/selection/history/export/reopen observations; preserve original receipts and unknowns. |

| D24 | **Merge fragment proof:** Two full original text ranges map to ordered contiguous candidate fragments covering the complete actual candidate text range; exact bytes, encoding and own-snapshot coordinates verified. Missing/foreign/duplicate/overlapping/gapped/reordered/partial fragments refuse. |
| D25 | **Normalization at join:** CR + LF becomes CRLF across deletion, lone CR/LF, tabs, XML whitespace references and UTF-16 boundaries; prove parsed candidate whitespace from joined lexical bytes, not concatenation of original parsed values; source bytes remain exact. |
| D26 | **Merge boundaries:** Zero/one bounded merge group; reject unsupported significant text/CDATA, arbitrary many-to-one elements, wrong parent and nonadjacent or larger groups. Comments/PIs/elements keep unaffected nodes separate. |
| D27 | **Consumer merge authority:** Old/plain-dictionary adapter cannot Apply; compatible consumer verifies complete partitions/fragment replay and immutable evidence, parent selection, revision checks and history without resurrecting source identities. |

## Acceptance decisions

- [x] Product owner selected explicit merged-text provenance before implementation (17 October 2026).
- [ ] Review the concrete map-plus-merge schema and normalization/consumer verification rules before engine implementation.
- [ ] Maintainers review API names, supported empty-body forms, validation boundaries and exact partition/parent evidence.
- [ ] Implement automated fixture groups with observed results, exact revision/toolchain and retained original receipts.
- [ ] Full solution and language-server suites, frozen QT3/qt4 gates and independent package-only consumer verification pass on the exact implementation head and integrated revision.
- [ ] Braid independently reviews/adopts a compatible delivered package, including changed-output and history/selection coverage.
- [ ] Native owner accepts actual supported workflows and records remaining limits without inferring unobserved cases.

D07 is planned success coverage for bounded whitespace merging; D24–D27 prove its byte, graph, normalization and consumer boundaries. The original injective-only request is explicitly revised for text merges; no implementation or Braid acceptance is inferred from the direction decision. Do not mark REQ-130 implemented or consumer-accepted merely because a narrow automated subset passes. No release/pin/UI changes are made by this plan.
