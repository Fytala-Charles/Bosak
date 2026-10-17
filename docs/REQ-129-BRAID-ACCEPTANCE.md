# REQ-129 - Bosak.Braid Consumer Acceptance

Date: 2026-10-17. Decision: **Implemented and accepted by Bosak.Braid**, within the delivered bounded readable value-of insertion profile. Requester: Bosak.Braid. Authority: product owner explicitly asked the Braid agent to confirm functionality to Bosak so the request can be marked implemented and accepted.

## Accepted Delivery

Published Bosak 1.17.0, exact annotated-tag/release revision `15eacde48b47cc8e48af29ddc95949801a2e3e5d`. Braid adopted that immutable revision in a clean dependency checkout and production Infrastructure/Application/Workbench composition. Earlier root-branch source and other working changes are preserved. This is consumer acceptance of the published delivery, not a new engine implementation, release or tag.

Public engine API keeps PreserveSource as its compatible default. Braid separately discovers readable capability, explicitly requests Readable in the capable desktop, adapts all SourceEdits, freezes full review evidence, checks exact unchanged-span replay and byte/Unicode/identity ownership, and requires explicit Apply. Ambiguous/unsupported formatting refuses without silent fallback. Bounded full-span review refuses oversized evidence rather than truncating it.

## Automated Consumer Evidence

- Full normal Braid Release: **952 passed, zero failed/skipped** (27 Core, 341 Application, 352 Infrastructure, 232 Workbench).
- Fresh public-nuget.org-only consumer restore: **184 passed, zero failed/skipped**, including unchanged public engine contracts and independent Braid probes/Application cases.
- All nine consumed public engine assemblies report `1.17.0+15eacde48b47cc8e48af29ddc95949801a2e3e5d`.
- Source edits replay exactly; unchanged spans, original bytes, BOM/declarations, node correspondence and inserted select coordinates are retained across UTF-8 BOM/no-BOM and UTF-16 LE/BE.
- Both formatting modes pass licensed validation, worker preview with unchanged ExactBytes golden output, new-file export/hash/provenance, independent ordinary-file reopen/run and exact one-step undo/redo. Stale/cancelled/denied/malformed evidence refuses under consumer authority gates.
- Headless desktop coverage verifies complete review, input/mode invalidation including during work, explicit formatting-refusal retry, review bounds and selection/graph integration.

Consumer receipts in the sibling Braid checkout: `.acceptance/bosak-1.17.0-public/verified-braid-results/` (four full-suite TRX files) and `.acceptance/bosak-1.17.0-public/results/public-release.trx`. [Consumer report](../../Bosak.Braid/docs/BOSAK_REQ129_CONSUMER_REVIEW.md), [production adoption](../../Bosak.Braid/docs/STRUCTURAL_AUTHORING.md#readable-insertion-release-adoption) and [native walkthrough/feedback](../../Bosak.Braid/docs/GRAPH_UI_ACCEPTANCE.md#preview-against-expected-output-export-and-reopen). Local sibling links refer to the preserved working checkout; Braid changes remain uncommitted at HEAD d863993.

## Native Product-owner Confirmation

The user confirms pretty-printed value-of insertion displays correctly in three tested encodings; encoding names/variants were not supplied. The user also confirms undo/redo and validation happy paths, followed by checking all steps of preview against expected output, export to a new file, reopening, revalidation and fresh preview/result confirmation without finding issues.

The user explicitly requests Bosak record REQ-129 functionality as implemented and accepted. This closes the consumer functional acceptance; no additional native observations are invented.

## Supplemental Evidence and Scope Limits

Step 7 manual SHA-256 values were missed. Native export-dialog/file-hash comparison and original-file manifest verification remain unrecorded; this acceptance does not assert those manual audit steps passed. Automated exact-byte/export-hash/provenance tests passed independently. This supplemental evidence gap does not block the product owner's explicit functional REQ-129 acceptance.

No complete XSLT conformance, broader formatter/insertion profiles, unobserved native failure/cancellation/policy-loss scenarios, additional encoding variants, DPI/screen-reader certification or production issuer/trust activation approval is granted. Existing conservative xml:space/mixed-content/layout/profile/resource limits remain part of the accepted contract. No engine files, tags, installed extensions, commits or pushes were changed by recording this decision.
