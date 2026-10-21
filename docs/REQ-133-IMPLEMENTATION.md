<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-133 literal text body replacement implementation">
<h1>REQ-133: literal text body replacement implementation</h1>
<p>Source 1.21.0 - 18 October 2026</p>
</div>

**Publication update:** [1.21.0 is published and verified](RELEASE-1.21.0-PUBLICATION.md) after [Braid implementation review](REQ-133-CONSUMER-IMPLEMENTATION-REVIEW.md) reported no blockers. Earlier source-only, local-feed and review-pending statements below preserve their historical delivery checkpoint. Consumer adoption/native acceptance remains separate and pending.


Source implementation and local verification are complete after [no-blocking Braid design review](REQ-133-CONSUMER-DESIGN-REVIEW.md) and explicit user authorization. Baseline main is `00b5e01e9749e6c11e3dfa8279dda76ff8a429df`. Public packages remain 1.20.0; no release, Braid adoption or native acceptance is claimed.

The [reviewed contract](REQ-133-TEXT-BODY-REPLACEMENT-CONTRACT.md) is implemented additively in `Bosak.Xslt.Authoring`. `SupportsTextBodyReplacement`, its 8,388,608-byte bound and immutable encoding list describe a separate operation. Snapshot discovery issues an opaque owner-specific paired ordinary text-body target; proposals contain exact logical Unicode and no formatting option.

`ProposeTextBodyReplacement` returns three exclusive outcomes. Changed supplies a fresh statically validated candidate, exactly one complete body edit, actual instruction/parent/payload descriptors and disjoint survivor/replaced/new partitions. NoChange supplies only original-owner evidence after eligibility, original validity, scalar and cancellation checks; it precedes hypothetical canonical encoding and size checks. Refused supplies only its operation-specific failure. There is no generic result IsSuccess property.

The pipeline independently checks replacement eligibility without borrowing append layout restrictions. It supports preserved and significant surrounding text, alias/default XSLT names, and effective false expansion overrides. Self-closing/body-markup/expanded-text/output-escaping and captured static/resource exclusions refuse without repair. Existing authoring pipelines and failure enum values are unchanged. Static validation and deny-by-default compilation reuse established primitives; no transform is executed during proposal.

One retained-byte splice replaces only the complete body. Emission escapes ampersand/less/greater and uses numeric CR/LF/TAB references in the original strict encoding. Original and actual final bounds include BOM; old body bytes are subtracted, so shrink-to-fit is legal. Equal intent preserves original references and physical source spelling. Survivor proof checks actual ordinary inspection, exact translated ranges, names, complete attribute metadata/ranges, outside logical content and child order, excluding the replaced payload even for nonempty-to-nonempty.

## Verification

Final local Release build passed with zero warnings/errors. Full solution: **9,900 passed, zero failed/skipped**; separate language server: **84 passed**. New source cases: **76 passed**, comprising **63 public API cases** and **13 instrumented internal authority/cancellation controls**. These cover payload transitions/partitions, names/versions/context, four encodings/coordinates/outside bytes, Unicode/escaping/equality, null/immutability/concurrency, physical limits/shrinkage/NoChange budgets, static/resource/profile refusals, deterministic cancellation and malformed metadata/identity/order/payload evidence.

Frozen gates on the final implementation: QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, zero gated failures. No engine/parser/conformance fixture changes were needed.

Independent local package consumer at immutable source `d09737f1a37507e51d257d6268892eb8910cc2c2`: **437 passed**, comprising 63 new public cases and 374 prior public authoring/identity cases. A distinct non-friend assembly uses PackageReferences only; 13 internal controls are excluded. Fresh-cache source mapping pins Bosak packages to the local feed. Ten archive source revisions, nine consumed archives/cache/output assemblies, SHA-256/SHA-512 and restore-source receipts agree. Transitive vulnerability audit reports none. A separate all-ten-package consumer passed **153 broad checks**, verifying exact informational versions and archive/cache/output assembly equality including the database provider.

Counts overlap and must not be added as independent product acceptance. The 28 acceptance groups remain a matrix; consumer-only R23-R25/R27 and consumer malformed-evidence review remain Braid responsibilities. Seven Braid design probes cover existing operations only. Exact-head CI and final immutable package revision accompany delivery through the implementation PR/receipt; independent implementation review remains pending. Logs, original failures, TRXs, hashes and source/audit receipts are retained under `.guard-tmp/req133-*`. README and legacy operation implementations remain unchanged. Documentation HTML was rendered with unchanged canonical CSS; no native preview/PDF walkthrough is claimed. The unchanged consumer review retains its original Braid-relative references.

Two initial sandboxed solution runs each encountered 45 database-provider fixture failures because HttpListener could not start; the final full rerun with required local permissions passed all 9,900. Logs/TRXs are preserved, and the log-tail-only interim success assessment was corrected. Test exit codes and all ten final TRX counters are checked, rather than inferring success from the last project.


The first compile failed because the new failure adapter referenced Code instead of the existing ErrorCode member; corrected before test execution. The original failing build log is retained. No engine/fixture failures occurred in the first 47, expanded 63 or source-control 71 and hardened 76 targeted test runs. Final evidence supersedes these earlier checkpoints.

## Consumer integration and remaining gates

Use a replacement-specific adapter after compatible package delivery. Discover capability explicitly; never reuse insertion/deletion authority. Distinguish NoChange from a candidate, and use ReplacementBodyRange even when empty. For Changed, require both complete disjoint graph partitions, keep old logical text separate from lexical bytes, freeze full review and restore selection to actual EditedNode plus payload/caret. Current license/session/revision, separate atomic Apply, exact-byte history, worker/export/reopen and native observations remain consumer gates.

Next is independent Braid implementation/package review through the [implementation handoff](REQ-133-CONSUMER-IMPLEMENTATION-HANDOFF.md). No public publication is authorized or performed by this implementation step.
