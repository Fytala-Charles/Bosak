# REQ-132 implementation review handoff

**Publication update:** [1.20.0 is published and verified](RELEASE-1.20.0-PUBLICATION.md) after [Braid implementation review](REQ-132-CONSUMER-IMPLEMENTATION-REVIEW.md) reported no blockers. Earlier unpublished/review-pending statements below preserve their source-delivery checkpoint; consumer adoption/native acceptance remains separate.


18 October 2026. Source **1.20.0 unpublished**; source verification complete; independent implementation review pending. User authorized this slice following [Braid no-blocking design review](REQ-132-CONSUMER-DESIGN-REVIEW.md) of e6f21839. That review is unchanged; SHA-256 `49cdb0aae35036835942bd788a8033c6d4578acbee2457cc388070de1c31c30c`. It reported no new-operation probes and is not implementation acceptance.

Read [implementation details](REQ-132-IMPLEMENTATION.md), the [reviewed contract](REQ-132-LITERAL-ELEMENT-CONTRACT.md) and [planned acceptance matrix](REQ-132-LITERAL-ELEMENT-ACCEPTANCE.md). New APIs are operation-specific: GetLiteralElementInsertionTarget, AuthoringLiteralElementInsertionProposal and ProposeLiteralElementInsertion; candidates expose complete source edits, actual parent/element, empty-body caret and total old mapping/NewNodes partition.

Observed source results: **91/0/0** new-operation tests (85 public cases plus six instrumented source-only controls), final full solution **9,824/0/0**, separate language-server **84/0/0**, zero-warning/error solution build, QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**. Independent local-package verification at committed source `0677f060a89e515fe6ef03733c69f2f3df23334a` passes **374** public-API tests and **153** broad checks; all ten archives/nine focused/ten broad assemblies, cache/output bytes, metadata, SHA-512, restore sources and both audits verify. Six source-only controls are excluded from the package consumer. Read [machine-readable proof](REQ-132-SOURCE-VERIFICATION.json); counts overlap.

Exact-head CI is required before merge. The completed local delivery receipt `D:/Development/Bosak/.guard-tmp/req132-implementation-delivery.json` supplies final reviewed head, CI, merge revision and exact-merge feed at `D:/Development/Bosak/.guard-tmp/req132-merged-packages`. Verify that receipt/feed before consuming; original source-head candidate packages remain separately retained in `req132-head-packages`. These are unsigned local candidates, not public NuGet packages.

Review should independently check runtime default-namespace identity, no-prefix default-XSLT composition with text/value-of, Readable shrink-to-fit and complete edit replay/descriptor partition, immutable public evidence, exact naming and classified refusals, and unchanged legacy behavior. New consumer UI/license/history/worker/export/reopen/native cases remain Braid responsibilities. Source/package counts overlap; do not add them or infer native acceptance.

No tag, NuGet publication, Braid pin/UI change or extension publication is authorized by this implementation. Publication follows independent implementation review and separate approval. Preserve original failures/fixture corrections; do not replace execution receipts.
