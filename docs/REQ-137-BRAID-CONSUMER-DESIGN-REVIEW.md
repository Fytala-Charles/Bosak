# Bosak REQ-137 consumer design review

Date: 2026-10-24. Disposition: **No blocking consumer-design findings within the proposed bounded profile.** Design review only; implementation authorization and runtime acceptance remain separate.

## Reviewed revision and evidence

Reviewed [Bosak PR #430](https://github.com/Fytala-Charles/Bosak/pull/430) at exact head `cf66f2be3a8d5dce0b694fdd5c410d2f19adaca4`, base `4de6968cfb6248b2aa553d930bcf3a63bace9ef4`. All nine changed files were inspected; changes are documentation, intake receipt and a request-specific Git byte-preservation rule. No engine, test, fixture, version or kit changes are included.

The exact-head maintainer scope document SHA-256 is `b91ff7e629d48766739e39aca07b93b37b175e29d3658663349741577abd2207`. The original upstream request SHA-256 is `adec8514200070994275b8267570b66096892b5622ce4befd90224b8e2e823f3`, independently matching Braid's registration receipt and the intake JSON. Request bytes are unchanged. Downloaded evidence is retained under `.dependencies/req137-review`.

Compared with the [consumer request and CR01-CR14](CHILD_ELEMENT_REORDER_REQUEST.md) and [proposed ADR-024](ADR-024-same-parent-child-element-reordering.md).

## Consumer decisions

| Area | Review result |
|---|---|
| Coordinated scope | Both directions together, one implementation PR followed by one consumer integration PR; no arbitrary destination or Delete/Create emulation. |
| Initial profile | Accept the explicit no-namespace literal parent/children, creation-compatible ancestor chain, REQ-136 vocabulary, dependency-free Legacy host and four strict encoding variants. Imported and authored supported subtrees remain covered. Broader contexts require separate review. |
| Adjacency and separator | Immediate neighbor only, absent or one proven ordinary whitespace-only text descriptor; significant/intervening content, preserved-space, CDATA and unsupported contexts refuse. Interior comments/PIs/instructions travel with their subtree. |
| Exact edit | One contiguous replacement implements P+A+W+B+S to P+B+W+A+S. Payload ownership, whole-source replay, unchanged outside bytes and no serialization/formatting satisfy the request. |
| Authority and selection | Independently advertised capability, opaque snapshot-owned direction-bound target, no proposal override, identity/eligibility rechecks and fresh candidate ports. Up selects moved B; Down selects moved A. |
| Complete proof | Total node/per-owner-attribute bijection, exhaustive ownership and metadata checks, preserved internal subtree order, explicit sibling-order exception and actual separator correspondence. A shifts by LB+LW, W by LB-LA, B by -(LA+LW); ancestors/outside extents stay fixed. These translations are consistent for unequal lengths. |
| NoChange | Exact whole-source equality only after eligibility/authority/bounds/cancellation gates; retain original selection without candidate, fresh ports or Apply authority. Equal output with changed bytes remains Changed. |
| Bounds | Accept independent 8 MiB source limits, graph limits of 65,536 nodes/attributes and depth 256, and cumulative 16 MiB copied-evidence limit. Complete evidence may refuse below the source maximum; no truncation. These are work/copy limits, not a total heap guarantee. |
| Safety and ownership | Bounded traversal/copy/proof, cancellation before both success outcomes, defensive exports, concurrent isolation and denied ambient acquisition are explicit. CR01-CR14 divide engine proof from Braid atomic Apply/history/selection and worker/export/reopen acceptance. |

## Verification and remaining gates

All three PR checks pass at the reviewed head: Build & test (net10.0), XSLT wrapper controls (Windows), and VS Code highlighting; [CI run 40062952894](https://github.com/Fytala-Charles/Bosak/actions/runs/40062952894). This is a documentation proposal: no reorder implementation or move tests exist in this PR. Previous Braid 1,444 Release /839 signed public-package passes remain historical evidence for accepted 1.24.0, not reorder verification.

No requested design correction is required. Recommend owner approval of implementation against this exact reviewed contract, then coordinated engine implementation, exact-head implementation review and separately verified signed publication before Braid integration. Registration and this review do not grant engine implementation, merge or release approval. REQ-137 remains Pending / target TBD / owner Unassigned; ADR-024 remains Proposed. No upstream edits/messages, dependency changes, commit or push were performed by this review.
