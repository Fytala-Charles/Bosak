# C43 — Compatibility acceptance matrix

**Proposed requirements, not accepted implementation.** No-provider evidence is current; provider acceptance remains pending.

| ID | Area | Current disposition | Required evidence |
|---|---|---|---|
| AC01 | No-provider compatibility | Current baseline captured; future seam parity required | Same build/test/gate outputs and exact skip maps without registration; C42 membership/copy/context regressions preserved. |
| AC02 | Authority and verdicts | Planned prototype | Unowned fallback only; owned Unsupported/Invalid/Cancelled/provider exceptions never downgrade or silently succeed; conflicting owners refuse. |
| AC03 | Closure and static visibility | Planned prototype | Resolve schema imports/includes under host resource policy, retain scoped local/global component identities, cancellation and complete closure key. |
| AC04 | Node and atomic identity | Planned prototype / ABI review | Provenance survives data(), bindings, conversion, copies and escaped values; strip removes it; cross-session equal-QName mismatch refuses. |
| AC05 | Actual membership and subtype | Provider model missing / unqualified | Affiliation list, abstract/nillability/derivation blocking and actual member-set containment; original substitution-020..025 assertions pass unchanged. |
| AC06 | Assessment and assertions | Staged add-side only / unqualified | Full claimed structural validity, invalid-instance error mapping, applicable/inherited assertions, typed list/union/ID values and private-node ownership. |
| AC07 | Conditional types | Staged selection only / unqualified | Alternative order/default/static context and xsi:type derivation; current validation-1301 original becomes a separate integration witness. |
| AC08 | Documented XSLT cohort | 30 fresh retained guards | All 30 original results remain intact; independently qualify source-document/merge/accumulator/streaming and catalog scenarios before any skip removal. |
| AC09 | Independent applicability | Fresh retained earlier skip | catalog-005b retains its negative XML_1.1 requirement; do not fold it into XSD 1.1 closure counts. |
| AC10 | Lifetime and concurrency | Planned prototype | Immutable sessions, concurrent evaluations, disposal/escaped-node lifetime, complete cache key and cancellation; no caller-tree mutation. |
| AC11 | Host error mapping | Planned prototype | Stable structured provider diagnostics mapped per XPath/XQuery/XSLT operation; ordinary false membership remains distinct from unsupported assessment. |
| AC12 | Layering and licensing | Current source boundary captured | No core product or default gate dependency on Bosak.Schema, no auto-loading/activation; separate explicit licensed integration receipts. |
| AC13 | Public compatibility/performance | Required before API freeze | Exact additive signatures, XdmValue identity/layout review, reference consumers and measured no-provider overhead; preserve concrete legacy members. |
| AC14 | Admission and release | Separate owner actions required | Owner acceptance before S4c/API freeze, future semantic adoption pins, provider qualification, separate exact admission proposal and explicit release approval. |

The current six QT3 queries stop during schema preparation; all 30 documented XSLT cases and the additional CTA witness remain guarded. See [per-case XSLT ledger](evidence/xslt-ledger.json), [six original QT3 records](evidence/qt3-six-originals.json), [source findings](evidence/source-findings.json) and [compatibility proposal](COMPATIBILITY-PROPOSAL.md). Source observations do not certify an activated provider.
