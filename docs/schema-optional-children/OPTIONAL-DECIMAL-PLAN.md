# Optional decimal children: implementation plan — 29 October 2026

This is a reviewable next slice, not implemented support. Read the [adoption record](OPTIONAL-DECIMAL-ADOPTION-2026-10-29.md) first. The existing 204 Schema tests and the earlier core/gate checkpoint remain the required-only baseline.

## Trigger and intended result

For declarations `amount` (required), `shipped` (optional), `tax` (required), `<order><amount>12.5</amount><tax>1</tax></order>` should structurally match. Its successful assessment contains root, amount and tax states; tax retains the original **third declaration**, not the skipped second declaration. Present optional children are validated and typed exactly like required decimals.

Assertions still run after complete structural and lexical/value assessment. `shipped le amount` on that instance has false effective Boolean value, producing Invalid with an assertion finding and no document. `shipped le amount or amount gt 0` can pass. Missing required children, duplicates, unexpected names and out-of-order children are structural Invalid results before assertions. Absent optional children do not make an otherwise false assertion pass.

## Required implementation changes

1. **Compilation:** replace the all-local-ExactlyOne check with bounded local-minimum recognition. Keep enclosing sequence/max-occurrence checks. Compile each local particle's minimum alongside its exact original declaration object. Preserve the public declaration list and reject repeated names. Select v2 only when an optional local particle exists; keep required-only v1 behavior and historical refusals.
2. **Matching:** scan schema particles and actual child elements with separate indices. Consume a matching name once. If it does not match, skip only an optional particle; a missing required particle is Invalid. After the scan, any unconsumed instance child is Invalid. Do not seek ahead in the instance or sort children by name. At most sixteen particles makes this bounded without backtracking.
3. **Present-state construction:** append an `AssessedElement` only when its instance element was consumed, using the consumed particle's declaration. Run lexical/exact-value checks for that element before making it available to assertions. Preserve cancellation checks while scanning skipped particles and present nodes.
4. **Assertion context:** build the existing parentless candidate using actual present children. Annotate only present decimals. Replace direct Boolean payload access with `GetEffectiveBooleanValue()` on the assertion result. Empty means a failed assertion, not Unsupported. Preserve the existing bounded error/refusal policy for unrelated engine failures; do not expand grammar or add `exists()`/`empty()` as part of this slice.
5. **Copies/exports:** audit every use of `ChildDeclarations[index]` and declaration-count iteration. Existing export indexing by present `Elements` may remain only where the state-to-XML correspondence is explicit and tested. Missing projections refuse; present projections after gaps return the correct decimal and state. Preserve owner/component references, independent snapshots and caller-owned XML.
6. **Empty-root binding:** the provider currently removes ignorable whitespace from shape checks only when a parent has child elements. An all-optional-absent element-only root has none. Normalize its ignorable root whitespace using the carrier's element-only assessment, without normalizing decimal leaf text. Keep insertion of a real child or non-whitespace text detectable. Add empty-root whitespace, real-text mutation and XSLT preserve/strip controls. This requires the bounded core patch and fresh core solution/conformance checks.
7. **Delivery:** advance the Schema feature minimum to 0.6, update changed C# histories/XML comments, profile guide and canonical docs, then capture original raw receipts and identities in a new dossier. Keep all earlier sealed evidence unchanged. Proposed core patch candidate is 1.34.1 for the empty-root binding correction; update only the unpublished Schema candidate references/source override contract. No product consumer pin, gate, fixture, package or tag change is part of the planned implementation.

## Acceptance matrix

| Area | Required cases |
|---|---|
| Occurrence parsing | Omitted/default minimum one; explicit one; canonical/zero-padded/XML-whitespace zero; signed zero Unsupported; repeated/unbounded/zero maximum Unsupported; optional enclosing sequence Unsupported. |
| Structural presence | Optional first/middle/last present and absent; consecutive optionals; all optional absent; required-only regressions; required child missing. |
| Invalid ordering/count | Duplicate optional; duplicate required; reordered present children; unknown child before/after; required element cannot be skipped to accept a later optional. |
| Assertions | Absent optional comparison using eq/ne/order operators; comparison wrapped by and/or; true constant-only assertion for all-absent structure; false assertion exposes no document; missing required wins before assertion evaluation. |
| Original identity | ChildDeclarations retains omitted declaration; Elements contains present instances only; later child retains its own original declaration; copies retain references and independent snapshots. |
| Engine exports | Typed later child after a gap; absent projection refuses; whole-document XPath decimal types; FOTY0012 root with all children absent; ignorable whitespace removal on that root; non-whitespace/root-child edits still refused; XQuery/XSLT preserve/strip on a gapped document; edit refusal and source immutability. |
| Safety/regressions | Cancellation, XML/schema bounds, decimal overflow/rounding refusal, nil/default/fixed refusal and full existing suite. |

Use data-driven tests for the combinations; the matrix is a requirement inventory, not a promised test count. Add at least one happy path and edge case for each changed responsibility. Capture the current refusal before implementing optional admission; never change the original assertions merely to obtain green results.

## Evidence and completion gate

Current source inspection identified four index/count assumptions: schema local compilation, structural equality check/state assignment, assertion-candidate annotation loop and state/XML projection mapping. Thirteen unique isolated baseline observations (the initial twelve are retained separately) confirm required-only success, current optional-schema refusal and correct engine empty-comparison Boolean behavior. The thirteenth observation exposes the empty element-only root whitespace binding gap; it is not a current required-profile conformance failure because required children cannot all be absent. These are diagnostic observations, not implemented feature tests or conformance admissions.

Before considering implementation complete, run the complete Schema solution against current core source, the whole-document example, strict owning docs check and meaningful absent/present copy controls. The planned empty-root correction changes core production code, so run the full core solution and normal QT3/qt4 gates with exact skip comparisons. The current source-identical checkpoint (24,209 passes; QT3 31,397/0/424; qt4 8,941/0/1,918, 230 sets) is inherited only for this planning step and is not sufficient to qualify that future correction. Any engine-touching change requires fresh full gates under AGENTS. No upstream fixture changes are planned.

The owning implementation remains in the persistent Schema feature worktree. The core `docs/schema-optional-children` dossier holds snapshots, baseline observations and hashes. No implementation, public API freeze, push, commit, tag or release is performed by this planning step.
