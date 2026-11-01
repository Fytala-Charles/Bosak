# R2 owner review packets

Prepared 1 November 2026 against the compiled draft delivered in **`0890592e`**. Four packets and seven role-specific return forms make O1–O4 executable review tasks. The [packet index](PACKETS.json) binds every return to the exact R2 proposal manifest and decision ledger hashes. **Preparation is complete; delivery and owner acceptance are not claimed.** No external checkout/request was edited and no message was sent.

| Packet | Requested roles | Concrete review |
|---|---|---|
| [O1](O1.md) | Schema, XmlDb | Actual source-node membership, original node/facts association and snapshot-specific typed issuance; ordinary XmlDb without fake schema assessment |
| [O2](O2.md) | Schema, Core | Exact numeric/list/union contracts, N1–N8, advisor context and compatible-view supported/refused profiles |
| [O3](O3.md) | Braid | Required invocation completeness, worker ownership/protocol/currentness and representative workloads |
| [O4](O4.md) | XmlDb, Core | Actual acquisition failure preservation, reader/generation retention and finite cancellation/reentry/concurrent-close profile |

Each packet has four precise questions and links to the compiled proposal, exact metadata delta and actual capability audit. Each requested role supplies its own response; a shared packet is not a shared or inferred signature. No production product reference to .C45Proposal assemblies or product pin/store change is requested to answer R2.

## Existing-return check

[The local check](LOCAL-RETURN-CHECK.json) records five clean checkouts and the exact documentation search scope. No matching explicit response to the revised draft was found in those local documents. Remote issues/messages were not searched; this is not proof that no external answer exists. Older provider facts, source/package qualification and ordinary XML upgrades remain evidence within their original scope, not acceptance of these new signatures.

Braid's active checkout is `0f1d949e758645f7944f617f2b4613e49161a195` (1.26.2 upgrade), distinct from the earlier audited worktree `d81f195f63a534099391220536f9885c44fddb30`. Neither observed local record supplies an R2 disposition. This packet does not upgrade either checkout or extend D01 acceptance.

## Completing and reviewing a return

Incoming files under `returns/` are unsealed submissions, excluded from the prepared-packet manifest. They require separate format and substantive review; merely adding a file changes no acceptance field. Original packets/templates remain unchanged. Current return dispositions are tracked in the [consolidated answers and execution plan](../foundation-r2-consolidation/README.md).

Copy the role's JSON template to a new response file. Set responseStatus to Submitted; provide Accept, AcceptWithScope, Revise or Reject, exact source revision, named signer/date, explicit profile, an answer to every question, an explicit required-signature-gap list and evidence references. A justified not-applicable answer is allowed; a null answer or null gap assessment is not acceptance. Accept/AcceptWithScope cannot leave a required signature gap unresolved.

Check completeness with `python -B docs/foundation-r2-owner-review/tools/returns.py --response <response.json>`. This validates format and draft binding only. It does not authenticate the signer, evaluate evidence quality, accept a contract or update any release gate. Actual runtime qualification stays distinct from a design disposition; R2 acceptance does not imply R4 consumer acceptance, C45, API freeze, publication or a product pin upgrade.

The templates remain NotSubmitted, with decisions/signers/answers/gap assessments unset. No response is prefilled as accepted. [Ten synthetic validator controls](evidence/FORMAT-CHECKS.json) test rejection of unsubmitted, foreign-draft, incomplete, unresolved-signature and premature-freeze responses. Their one format-valid example is an in-memory synthetic fixture, not an owner return.

## Next gate action

Collect and review the seven role returns against this exact draft. Resolve revisions in the Core ledger, rebuild any changed declaration shape and refresh the applicable owner dispositions. Close R2 only when the accepted implementation scope and every known required signature need are explicit. W1–W7 planning can continue, but public implementation remains subject to C139-01/C139-02 start conditions; another synthetic lifecycle experiment does not replace owner disposition.

Production remains unchanged 1.42.0. Prior engine/gate receipts are inherited; this documentation/tooling step reruns no engine suite and claims no actual consumer integration. Read-only verification: `python -B docs/foundation-r2-owner-review/tools/verify.py`.
