<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-133 literal text body replacement implementation">
<h1>REQ-133: literal text body replacement implementation</h1>
<p>Source 1.21.0 - 18 October 2026</p>
</div>

Source implementation and verification are in progress after [no-blocking Braid design review](REQ-133-CONSUMER-DESIGN-REVIEW.md) and explicit user authorization. Baseline main is `00b5e01e9749e6c11e3dfa8279dda76ff8a429df`. Public packages remain 1.20.0; no release, Braid adoption or native acceptance is claimed.

The [reviewed contract](REQ-133-TEXT-BODY-REPLACEMENT-CONTRACT.md) is implemented additively in `Bosak.Xslt.Authoring`. `SupportsTextBodyReplacement`, its 8,388,608-byte bound and immutable encoding list describe a separate operation. Snapshot discovery issues an opaque owner-specific paired ordinary text-body target; proposals contain exact logical Unicode and no formatting option.

`ProposeTextBodyReplacement` returns three exclusive outcomes. Changed supplies a fresh statically validated candidate, exactly one complete body edit, actual instruction/parent/payload descriptors and disjoint survivor/replaced/new partitions. NoChange supplies only original-owner evidence after eligibility, original validity, scalar and cancellation checks; it precedes hypothetical canonical encoding and size checks. Refused supplies only its operation-specific failure. There is no generic result IsSuccess property.

The pipeline independently checks replacement eligibility without borrowing append layout restrictions. It supports preserved and significant surrounding text, alias/default XSLT names, and effective false expansion overrides. Self-closing/body-markup/expanded-text/output-escaping and captured static/resource exclusions refuse without repair. Existing authoring pipelines and failure enum values are unchanged. Static validation and deny-by-default compilation reuse established primitives; no transform is executed during proposal.

One retained-byte splice replaces only the complete body. Emission escapes ampersand/less/greater and uses numeric CR/LF/TAB references in the original strict encoding. Original and actual final bounds include BOM; old body bytes are subtracted, so shrink-to-fit is legal. Equal intent preserves original references and physical source spelling. Survivor proof checks actual ordinary inspection, exact translated ranges, names, complete attribute metadata/ranges, outside logical content and child order, excluding the replaced payload even for nonempty-to-nonempty.

## Verification

Pending final full-solution, language-server, conformance, CI and independent package receipts. Targeted source checks currently pass 76 cases: 63 public API cases and 13 instrumented internal metadata/cancellation controls. These overlap future package-public cases and are not additive independent product acceptance. All 28 acceptance groups remain a matrix; consumer-only R23-R25/R27 and consumer malformed-evidence review remain Braid responsibilities.

The first compile failed because the new failure adapter referenced Code instead of the existing ErrorCode member; corrected before test execution. The original failing build log is retained. No engine/fixture failures occurred in the first 47, expanded 63 or source-control 71 and hardened 76 targeted test runs. Final evidence supersedes these earlier checkpoints.

## Consumer integration and remaining gates

Use a replacement-specific adapter after compatible package delivery. Discover capability explicitly; never reuse insertion/deletion authority. Distinguish NoChange from a candidate, and use ReplacementBodyRange even when empty. For Changed, require both complete disjoint graph partitions, keep old logical text separate from lexical bytes, freeze full review and restore selection to actual EditedNode plus payload/caret. Current license/session/revision, separate atomic Apply, exact-byte history, worker/export/reopen and native observations remain consumer gates.

Next is independent Braid implementation/package review through the [implementation handoff](REQ-133-CONSUMER-IMPLEMENTATION-HANDOFF.md). No public publication is authorized or performed by this implementation step.
