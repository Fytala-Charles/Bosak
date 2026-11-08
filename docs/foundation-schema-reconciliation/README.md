# Schema source/policy reconciliation — 2 November 2026

Core has reconciled the eight Schema questions against the actual native source and existing proposal. The [decision map](RECONCILIATION.md) identifies what exists, what the proposed contract already represents, and what still needs an owner API/policy, adapter-feasibility, implementation or qualification decision. Schema's existing O1/O2 **Revise** returns remain unchanged; XmlDb's native lifetime revision remains separate.

## Concrete findings

- The exact native assessment source is available and clean at c3624998. A fresh isolated build leaves that worktree untouched and compiles with zero warnings/errors.
- **14 existing-source checks pass twice**: root/decimal state, original copy/component references, snapshot/assessment label separation, preserved/stripped exports, stale refusal, caller-edit isolation, independent components despite equal digest, pre-cancellation and invalid outcomes. They also show the copy-export owner reference differs from the original and that a carrier is not proof of node membership.
- There is still no implemented BCAP OwnsNode or typed issuance container in this source. Core provides a concrete private-owned-projection/container feasibility direction; it is not an implemented or qualified adapter, and no new mandatory Core identity member is demonstrated.
- Required caller-supplied finite limits and an invalid default limits struct are already the selected Core policy. No implicit workload defaults or unlimited fallback are proposed. Exact enforcement is R3; actual owner budgets are R4. Schema must confirm this design or identify the exact required change.

## Actual execution lane

The native worktree guide names Core **1.34.1**, but the observed DLLs in its output directory—and the actual probe bindings—are Core **1.34.2.0**. They are pinned by hash and were not rebuilt. Do not conflate a guide/package pin with loaded binary identity. The probe compiles exact native source into a separate harness assembly (default harness assembly version 1.0.0.0); that is not a new Schema release or product pin.

This is deliberately **not** the fully qualified Core 1.42 BCAP prototype or an implemented 47-type public SDK. The legacy bridge facts cannot qualify admission/lifetime/issuer behavior against that candidate. Initial 12-fact observations and the pre-observation basis are retained separately; the final 14-fact cohort adds discriminating copy/carrier tests. Original historical receipts and owner binaries are unchanged.

[Source/dependency inputs](evidence/INPUTS.json), [first run](evidence/native-facts-0.json), [second run](evidence/native-facts-1.json), [basis](SPEC-BASIS.md). No arbitrary private source/history or licensing material is copied; the native source used here is the active Apache public assessment worktree. Original exception tests catch errors under the scoped child error-mode wrapper.

## What is ready for the owner

The source-access ambiguity is resolved and the finite/no-default design is explicit. The remaining decision is whether Schema accepts the represented contract within supported/refused profiles, with a feasible native adapter plan, or identifies a precise unexpressible requirement. Full XSD 1.1 backend implementation is not required merely to disposition the R2 shape, but all known foundation needs must remain represented before freeze. Neither an empty signature-gap list nor a successful decimal probe is acceptance.

The concrete follow-up is registered locally in Schema's feature registry and integration guide; owner edits remain uncommitted. No owner message, Schema implementation/commit/push, Core engine change, product upgrade, native BCAP qualification, R2 closure or API freeze was performed. All seven review forms remain submitted: four scoped accepts, three revisions.

Reproduction uses a fresh output path: `tools/run.py` refuses an existing cohort. Run it with the full `docs/foundation-schema-reconciliation/` prefix, then `tools/verify.py`. Verify-only checks retain the recorded source/binary identities and do not rerun or overwrite historical evidence.
