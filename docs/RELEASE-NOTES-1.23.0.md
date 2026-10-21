<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.0 release notes">
<h1>Bosak 1.23.0 release notes</h1>
<p>21 October 2026</p>
</div>

**Published and verified.** Immutable `v1.23.0` source: `9218a2b6f2339f923fd6d018ba988c8325b2c44d`. [Public delivery proof](RELEASE-1.23.0-PUBLICATION.md).

Bosak 1.23.0 adds the complete source-preserving literal result attribute lifecycle (REQ-135): **Create, Update and Delete together**.

- Separate capability and snapshot-owned operation targets/proposals/results. Create requires an absent unqualified name; Update requires an existing proven constant attribute with ordinal logical NoChange; Delete removes the selected token without whitespace cleanup.
- Exact literal Unicode, braces, quotes, markup and tab/CR/LF semantics through engine-owned XML/AVT interpretation and escaping. Equal logical Update retains every original quote/reference/brace byte and returns no candidate/history authority.
- One exact edit, total actual graph mapping and complete per-owner ordinary/addressed attribute relations; no invented attribute child nodes. Actual new/updated attribute/value or mapped-owner selection is explicit.
- UTF-8 BOM/no-BOM and BOM-marked UTF-16 LE/BE, independent 8,388,608-byte source/final bounds, denied unsupported static/resource contexts, cooperative typed cancellation and immutable evidence. Existing authoring contracts remain unchanged.

The first attribute profile is conservative: supported principal no-namespace owners, unqualified constant attributes and complete module coverage. Qualified owner/attribute edits, dynamic AVTs, rename, namespace/control changes, generic upsert and arbitrary source editing are excluded. Ordinary mixed/preserved children can remain where proven unchanged. See the [contract](REQ-135-ATTRIBUTE-LIFECYCLE-CONTRACT.md) and [implementation record](REQ-135-IMPLEMENTATION.md).

This release also includes the previously unpublished source 1.22.1 switch correction: `xsl:when`/`xsl:otherwise` after a switch fallback tail now raise XTSE0010; legal trailing fallbacks remain accepted. Twenty regression cases cover both processor profiles and stylesheet versions 3.0/4.0. No separate 1.22.1 publication is planned.

Conformance tooling adds bounded native-run receipts/timeouts, selected/unselected accounting and missing selected-set failures, exact structured error identity matching,16 CLI controls and four Windows wrapper controls. Twenty-four native array/xsl:select boundary cases and48 public-API checks expand coverage; no upstream acceptance, independent processor confirmation or gate promotion is implied by their inclusion.

Verified source/merge CI: **10,214 solution +84 language-server tests**;136 new lifecycle source cases comprise117 public/19 internal controls. Exact-merge local packages pass 117 checks, and Braid independently passes700 (117 imported lifecycle +551 prior authoring/identity +32 probes), with no blockers and matching merge provenance. Counts overlap. Frozen QT3 remains31,142/0/679 and qt4 gated8,278/0/1,894 for the unchanged verified runtime source.

Signed public delivery is verified: ten signatures and exact package identities, fresh nuget.org-only153 broad/117 lifecycle checks and clear dependency audits. Earlier source/consumer review evidence used unsigned local candidates. Braid stays on1.22.0 until its adoption/integration, actual worker/export/reopen/native lifecycle acceptance; the VS Code extension has a separate release cycle. [Release readiness](RELEASE-1.23.0-READINESS.md).
