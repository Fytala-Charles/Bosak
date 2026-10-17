# REQ-129: Readable Value-of Insertion - Consumer Request

Published 17 October 2026. [Release verification](RELEASE-1.17.0-PUBLICATION.md). Original source/design checkpoint text below is historical.


Date: 2026-10-16. Requester: Braid. Upstream ID: REQ-129. Current status: Implemented and published. Target: 1.17.0. Owner: Charles Korthout. The user authorized implementation after contract preparation; public signatures and fresh consumers verify; user reports Braid found no blockers. Additional native UI results and production adoption are independent.

## Source implementation - 17 October 2026

The additive Readable mode, capability and complete source-edit evidence are implemented in source. See [implementation record](REQ-129-IMPLEMENTATION.md). PreserveSource remains the default; no release or Braid adoption is implied. The original design/request below is historical.

## Design checkpoint - 17 October 2026

A [proposed formatting contract](REQ-129-READABLE-INSERTION-CONTRACT.md) defines the additive mode/capability, direct-child whitespace footprint, compact two-space fallback, strict refusal cases, candidate edit/provenance evidence and planned tests. Design only: Pending/TBD/Unassigned remains accurate; implementation and consumer acceptance are not claimed.

## Consumer Need

Native review shows repeated insertions appended on one line, for example `<result><xsl:value-of select="/input/name"/><xsl:value-of select="/input/name2"/></result>`. The user requests readable indentation when adding instructions. Bosak 1.8.0 has no public insertion formatting option; Braid cannot rewrite engine candidates while retaining their acceptance evidence.

## Acceptance Criteria

- Expose supported opt-in readable insertion formatting, with the current source-preserving behavior remaining compatible by default. Bosak owns API design and supported profiles.
- Indent eligible element-only constructor content consistently with surrounding indentation/newlines; define compact-source fallback (Braid authored convention: two spaces). State the exact formatting footprint, including whether existing sibling indentation changes.
- Preserve significant text, CDATA, effective xml:space and unsupported/ambiguous contexts by retaining source or issuing a classified refusal. Do not alter XPath strings, namespaces, comments or processing instructions as a side effect.
- Return exact formatted candidate bytes, current inspected snapshot, inserted-node/select extents and complete injective surviving-node correspondence through supported public APIs. Braid reviews these exact bytes before Apply.
- Preserve UTF-8 BOM/no-BOM and UTF-16 LE/BE declarations/BOM, Unicode, line endings and source URI. Enforce existing resource, validation and cancellation bounds.
- Cover compact and indented parents, repeat insertion, newline/encoding variants, mixed/preserved content and unsupported profiles. Verify equivalent transformation output with exact golden bytes where applicable.
- One accepted formatted insertion remains one draft-history step; undo restores exact original bytes and redo exact reviewed formatted bytes. Neither operation restores old validation authority.

## Ownership and Compatibility

Bosak maintainers own acceptance, API, engine implementation/tests and release. Braid owns capability discovery, review/Apply/history, UI and consumer tests after a reviewed delivery. Do not patch pinned engine sources or splice XML in Braid. Existing applications must keep their current insertion results unless they explicitly request formatting.

References: [structural authoring](../../Bosak.Braid/docs/STRUCTURAL_AUTHORING.md), [ADR-015](../../Bosak.Braid/docs/ADR-015-explicit-reviewed-insertion-acceptance.md), [draft history](../../Bosak.Braid/docs/DRAFT_HISTORY.md).
