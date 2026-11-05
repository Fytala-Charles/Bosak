# XDM 4.0 compatibility review before the C45 production candidate

**Schema licensing/path update — 29 October 2026:** [Apache publication](SCHEMA-APACHE-PUBLICATION-2026-10-29.md) is complete. The active open schema checkout is `D:/Development/Bosak.Schema.Public`; the old `Bosak.Schema` directory is a locked private licensing checkout. Reassess the minimal API before C45; the older proprietary-provider boundary/path assumptions below are historical.

**Revised prototype complete — 29 October 2026:** [C44-R2 results and pending decision](conformance/k04-schema-compatibility-prototype/README.md). The compatibility review and isolated revision are complete. Next is owner disposition of the exact revised API/guarded C45 scope; production implementation remains unapproved. Preserve the historical plans/receipts below.

**Review completed — 29 October 2026:** [Results and revised next slice](compatibility/xdm40-2026-10-29/README.md). The cross-system source review and immutable draft pin are complete; real consumer runtime/ABI acceptance remains unqualified. Next revise the isolated compatibility prototype, especially compatible-schema handoff and explicit profile/version identities, before owner acceptance of C45. The checkpoint/plan below is preserved history; it no longer describes the review as future work.

29 October 2026. Latest completed slice: **C44**, commit **affc228a**. The owner responded to the C45 approval request by asking for an XDM 4.0 / dependent-system compatibility review. **That response was not approval. C45 production implementation and permanent API acceptance remain pending.** The current task is documentation/checkpoint/push for a host restart, not implementation authorization.

## Next authorized direction

Prepare the compatibility review before resuming production API work. Consult the [current XDM 4.0 draft](https://qt4cg.org/specifications/xpath-datamodel-40/Overview.html), whose displayed edition at the discussion was 22 October 2026. The URL is mutable: before implementation or semantic adoption, obtain an exact immutable source revision and record affected sections/deviations. This discussion and document are review inputs, not a new specification adoption or a full compliance claim.

1. Inventory Bosak's actual support and gaps for generic XML/JSON nodes, identity/order, records and map entry order, schema versus item types, atomic comparisons, XML characters and serialization. Existing JNode/record classes do not establish complete current-draft support.
2. Review the actual XmlDb, Bosak.Schema and Braid contracts, pinned package versions and source identities. Use sibling repositories read-only unless separately authorized. Include source/binary compatibility, public-interface implementers, value layout, persistence, lazy evaluation, disposal, concurrency and observable results.
3. Separate XDM/language profile, schema language version, component authority and source/storage identity explicitly. Review C44's generic Language/static-context inputs and annotation design against those distinctions before accepting permanent signatures.
4. Produce a consumer acceptance matrix and concrete candidate migration/prototype plan. Any API break requires a documented need, migration/versioning plan and actual dependent-system builds/tests. Synthetic carrier ABI evidence alone does not qualify real consumers.
5. Present the revised API/implementation proposal for owner acceptance after the review. Keep C45, ADR-006 and REQ-139 pending until the corresponding decisions and actual acceptance work occur. No approval is inferred from a continuation or restart request.

## Known consumer concerns from initial read-only inspection

| System | Review boundary |
|---|---|
| XmlDb (`D:/Development/Bosal.XmlDb`) | PersistentXdmNode implements IXdmNode. M1 identity combines snapshot and node ID; document order uses immutable preorder IDs; lazy results require snapshot lifetime handling. Review interface additions and future persisted JSON/type annotations without assuming a storage migration is already necessary. Process-local session objects cannot simply be written to storage; persistent type provenance would need a versioned representation and rebinding policy. |
| Bosak.Schema (`D:/Development/Bosak.Schema`) | Owns full XSD 1.1 implementation and licensing. Review schema identities through validation/atomization/copies, complete component relations and the distinction between schema types and broader item/record types. The draft's XSD 1.1 atomic semantics still allow an XSD 1.0 schema processor (§4.1.1); XDM 4.0 support does not itself require full XSD 1.1 validation in core. |
| Braid (`D:/Development/Bosak.Braid`) | Preserve inspection-scoped source identities, original byte extents and edit authority independently of runtime node identity. Review language/profile/schema fingerprints in run freshness and caches, preview results, numeric comparisons, ordered maps/records and serialization. |

Initial inspected references: XmlDb `src/Bosak.XmlDb.Xdm/PersistentXdmNode.cs`, `docs/ADR-002-immutable-format-and-snapshot.md`, `docs/ADR-003-pinned-bosak-dependency.md`; Braid `docs/SOURCE_GRAPH.md`, `docs/RUN_PROVENANCE.md`, historical `docs/BOSAK-1.26.2-ADOPTION.md`; Schema ADR-006 and C43's source/document audit. These reads are not fresh consumer build/test or adoption certification. Refresh current pins in the next review.

## Preserved qualification and limits

C44's [prototype dossier](conformance/k04-schema-session-prototype/README.md) remains immutable: **110 contract controls**, four detected bad variants, synthetic consumer **25/55** checks, 24 compiled proposed types, and 376 evidence checks. Its API/owner-decision files record the proposal as it stood at affc228a; this later checkpoint supersedes their next-action wording without rewriting their sealed evidence.

Current core candidate **1.33.1** remains unpublished; public release **1.26.3**. Product source/binaries are unchanged. Inherit C42's **24,203 solution passes /39 regressions** and C43's complete QT3 **31,397/0/424**, qt4 **8,941/0/1,918** gates/maps (230 sets, 11 experimental passes). No engine tests or gates are rerun for this documentation-only checkpoint. REQ-126 remains Complete under bounded acceptance with all 48 historical observations preserved. REQ-139 remains Pending. No baseline, filter, dependency, sibling, release, tag or upstream action is approved by this checkpoint.
