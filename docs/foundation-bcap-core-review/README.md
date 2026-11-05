# Core revised O2/O4 reviews — 2 November 2026

Core has completed both scoped design reviews against the exact 47-type/651-row BCAP candidate:

| Return | Disposition | Scope |
|---|---|---|
| [BCAP-O2-Core](returns/BCAP-O2-Core.response.json) | AcceptWithScope | Explicit carry-forward of exact numeric/typed semantics, required caller-supplied finite limits and no implicit defaults. Runtime implementation and workload budgets remain separate. |
| [BCAP-O4-Core](returns/BCAP-O4-Core.response.json) | AcceptWithScope | Capture/drain outcomes, original local exceptions, ordered cleanup, raw lifetime and qualified sequential gate/cancellation design. No additional required Core signature demonstrated. |

These are Codex-authored Core engineering dispositions requested by the repository owner, not independent human signatures or acceptance on another project's behalf. Every cited evidence file is hash-bound; original prior responses and the prepared proposal remain unchanged. Matching copies are delivered to the unsealed incoming area without overwriting another return.

All **7/7 revised forms are now submitted: four scoped accepts and three Revise**. Braid O3 and XmlDb O1 remain AcceptWithScope; Schema O1/O2 and XmlDb O4 remain Revise. R2 stays InReview. No public SDK body, native consumer support, numeric runtime implementation, accepted performance budget, C45/freeze or publication is implied by these Core design decisions.

The [focused Schema follow-up](SCHEMA-FOLLOWUP.md) points to the available exact native assessment worktree and asks for an explicit classification of remaining API/policy blockers, source feasibility, R3 implementation and R4 qualification. It clarifies the already-proposed finite/no-default policy rather than inventing workload limits. Registering this follow-up does not replace or upgrade Schema's decisions. XmlDb's native masking/fault-latch/release revision remains independent.

Next is reconciliation of those three revisions. Schema should assess the pinned native lane and precisely disposition the finite policy; XmlDb continues its native cleanup/failure/profile work. Accepted public implementation scope can open only after the required contract/source feasibility dispositions are resolved. Full XSD 1.1 backend completion is not automatically an R2 prerequisite, and existing bounded evidence is not a waiver of the full foundation contract.

Validation: `python docs/foundation-bcap-core-review/tools/verify.py`. No engine source or product pin changed, no previous receipts were rewritten, no owner message was sent and no engine tests were rerun for this document/design-only step.
