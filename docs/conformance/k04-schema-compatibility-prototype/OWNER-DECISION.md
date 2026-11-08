# Owner decision — revised schema compatibility candidate

**Pending.** Decision identifier: `K04-schema-compatibility-v2`. This record supersedes the proposed next action, not C44's sealed decision/evidence. The [restart checkpoint](../../RESTART-2026-10-29.md) explicitly withheld C45/public-API acceptance pending compatibility analysis; a generic continuation is not production acceptance.

The concrete review package is the [compiled 32-type surface](evidence/api-surface.json), [exact normalized proposal diff](evidence/api-diff.json), [API policy](API-PROPOSAL.md), [123-control/mutation qualification](QUALIFICATION.md) and [consumer acceptance matrix](../../compatibility/xdm40-2026-10-29/CONSUMER-MATRIX.md). The full evidence manifest pins the reviewed bytes.

## Proposed approval scope

Accept this revised **design direction and a guarded C45 production candidate**, subject to all of these constraints:

1. Keep existing production `IXdmNode`, `IXdmSequence`, XML enum values, `XmlSchemaSet`/validator/resolver paths and `XdmValue` fields intact. Put optional neutral schema contracts in Core; Apache core references no commercial assembly or license service.
2. Capture an explicit immutable semantic environment at compilation. Separate host/XDM profile, XSD version, component authority/universe and source identity; validate supported combinations against real compiler options. Preserve no-provider behavior and resource-policy boundaries.
3. Support compatible independently compiled sessions through a trusted endpoint-bound view/admission mechanism, retaining original PSVI. Do not admit by QName, root/full digest or source ID alone, silently rewrite annotations, revalidate as a handoff substitute, or downgrade owned unsupported coverage.
4. Use the existing-slot annotation envelope as a candidate only after the full production payload/accessor/equality/hash/serialization audit. Preserve source/snapshot ownership and cancellation. Keep detailed host error precedence; no invented W3C error QName.
5. Bosak.Schema remains the XSD 1.1 implementation owner. Its current model is not accepted as a complete backend. Implement or explicitly refuse each claimed component/assessment capability; qualify the real provider separately, without copying commercial implementation into core.
6. Build/test actual XmlDb, Schema and Braid source in isolated copies against the candidate; execute unchanged-binary replacement checks, including real lazy/annotated/copy values. Keep the original consumer pins/evidence intact. Use only separately authorized license arrangements; no activation or key generation is implicit in this decision.
7. Before API freeze or release, pass the appropriate complete solution, provider/consumer tests and fresh exact gates: QT3 **31,397/0/424**, qt4 **8,941/0/1,918**, 230 gated sets, with unchanged skip identities/reasons unless separately approved. Document any newly demonstrated limitation rather than claiming inherited or synthetic tests certify it.

This decision would authorize preparing/reviewing the guarded candidate, **not** unconditional API freeze, release/publication, consumer adoption, storage migration, full XSD 1.1/current-XDM certification, new Braid schema profiles or unrelated JNode/record/map/comparison repairs. Any production semantic change first records its immutable adoption scope and gets the required version/header/documentation/tests. Candidate 1.33.1/public 1.26.3 remain unchanged until that work is actually undertaken.

## Recorded disposition

- Owner response: **Pending**.
- Accepted API revision/source hashes: **Pending**.
- C45 production implementation: **Not started**.
- REQ-139/ADR-006: **Pending**; no status change inferred.
- Permanent API/ABI/provider acceptance: **Not certified**.

After an explicit decision, record the owner's exact scope and the reviewed manifest/source identities in a new disposition receipt. Preserve this pending proposal and historical C44 decision separately rather than rewriting original evidence.
