# FX009-02: Bounded XmlDb Candidate Characterization

Date: 2026-10-30. **Existing-API engineering characterization passes; joint contract/freeze
acceptance remains pending.** This is the bounded continuation of
the FX009-01 requirements return (15 areas / 176 manifest entries), not a product upgrade or a
claim that all FX009-02/foundation gates are closed.

## Scope and exact identities

Committed XmlDb source 2f9a36bc3ff22c8215d2f12ff38541770dd82585 is archived into independent
baseline/candidate workspaces. Current uncommitted M2 and requirements/status edits are
excluded from this source selection and preserved. Production source and the exact Core
1.26.2 pin remain untouched. Only a private Directory.Build.targets updates engine PackageReference
versions for the candidate builds; original project declarations and test assertions are intact.

Candidate is the existing seven-package 1.34.2 cohort from source
c88eea4308c0e49a81e7bfe2f481319f2e4d208c, feed SHA-256
03bb793d3cd69d7e819e1568ab11eff98bd2cec356a963098b3d5f738b15dc39.
All seven archive/DLL hashes match the Core packet. Offline closure contains 28 exact packages.
Raw archive hashes and NuGet assets contentHash are verified separately against their archive
sidecars and cache metadata; they are not assumed interchangeable. Isolated offline restores
explicitly disable NuGetAudit; no vulnerability-audit or publication claim follows, and product
settings/warning enforcement are unchanged. Both source builds have zero compiler errors.

Supported experimental scope is ordinary untyped persistent XML/default XPath 3.1, sequential
use and explicit caller ownership of snapshots/results. Existing Core proposals support
qualification of retained XML interfaces without new required members; they do not record
joint acceptance. No C45/schema/JSON persistence, general cancellation/reentry/concurrent-disposal,
all-XDM-4.0, full Storage/Integration or physical durability qualification is inferred.

## Primary results

| Mode, on each of Windows and isolated native Linux | Existing Xdm tests | Diagnostic observations |
|---|---:|---:|
| Baseline 1.26.2 source build | 27 pass | 37 |
| Exact 1.34.2 package-backed source build | 27 pass | Same 37 |
| Frozen baseline binary control | 27 pass | Same 37 |
| Unchanged binaries with seven engine DLLs replaced | 27 pass | Same 37 |

All eight primary platform/mode runs have the same 27 test IDs and the same 37 observation
records. Counts overlap: this is 27 existing cases per run, not 216 independent tests or 296
independent diagnostic behaviors. The 37 observations comprise 26 actual in-memory/persistent
query comparisons, seven numeric XML-kind mappings, direct typed value, cached/nested disposal,
actual generation leases and early/normal/cancellation ownership. Existing all-axis/namespace,
Unicode scalar update, URI, identity and disposal fixtures run unchanged in the Xdm suite.

The two project-boundary tests still inspect the original copied project XML, including its
1.26.2 declaration. They do not establish the effective candidate package version. Private
assets/cache checks and actual loaded assembly hashes independently establish that candidate
binding. No test expectation or source assertion is changed to obtain these results.

## Behavior returned to Core

- Direct TypedValue is a String carrier for the sampled untyped element. The 26 expressions
  compare atomization/type tests, numeric comparisons/errors, cardinality, axes, identity/order
  and duplicate removal. Matching the baseline does not establish full normative conformance.
- Public cached Current, constant inherited node defaults and Equals/GetHashCode can remain
  readable after disposal. Persistent leaf/fetch access and nested/map/array node dereference
  throw ObjectDisposedException; a copied scalar stays usable. BindResult is an outer guard,
  not deep detachment or ownership transfer.
- Two independently owned snapshots hold the actual generation-1 reader lease through coordinator
  disposal and a later generation-2 commit. Closing one snapshot leaves the other pinned; closing
  the second permits exclusive access to that owned fixture lease with unchanged bytes. This
  proves that fixture's known ownership, not global reader absence or permission to reclaim.
- A foreign iterator's actual private file handle remains owned by its caller after early break
  and normal enumeration. Explicit caller cleanup releases it without closing the snapshot.
  A caller-supplied DocumentLoader cancellation callback throws OperationCanceledException in
  these fixtures; the snapshot stays usable/pinned until explicit disposal. This is not general
  engine cancellation, race, exception-safe foreign cleanup or leak qualification.
- Unsupported axis and unregistered URI fail explicitly. No missing member, numeric remapping,
  candidate differential failure or original-store format migration is observed in this scope.

[RESULTS.json](RESULTS.json) returns each FXG-01 through FXG-06 with an engineering disposition;
all finalAccepted flags remain false. Core/Schema/XmlDb still need the finite profile, ownership,
legacy/experimental manifest and final candidate dispositions before shared freeze. FXG-05's
broader context/cancellation/concurrency questions are deliberately not filled with inferred
acceptance, and Schema's independent full-foundation needs are not waived.

## Binary, binding and store proof

Every frozen layout is inventoried before/after execution. Baseline and replacement contain
the same files; exactly the seven allowlisted engine DLLs differ. Consumer/test/helper DLLs,
executables, deps/runtimeconfig files, third-party files and fixtures are byte-identical within
each frozen pair and remain unchanged during execution. This is local Windows/Linux .NET 10
replacement evidence for these cases, not a universal ABI or archived-release certificate.

The unchanged non-retaining Core metadata startup observer records actual loads in every test
host/probe, with path, context, identity and SHA-256. All seven engine assemblies load in Default,
noncollectible context from the intended role layout. Candidate rows match the exact package
DLL hashes; baseline rows match the pinned baseline. No consumer binary or runtime manifest is
rewritten for replacement, and no unexpected fallback or alternate-context binding is accepted.

All real data stores are fresh owned fixtures. An existing H=1 format-1 store is seeded once
per platform and reopened by every mode; full bytes and file/directory inventories stay equal.
Legacy Open may republish byte-identical CURRENT: equality is not a claim of zero write calls.
Separate scratch stores perform the explicit later-commit/lease checks. Original stores, sealed
backups and historical fixture families are never opened as candidate inputs or migrated.

Windows frozen archives and Linux frozen archives retain complete replay layouts. The separate
Linux archival replay again passes the same four modes and reproduces the exact original frozen
bytes/inventories. It adds no distinct coverage. Sources/scripts, commands/logs, raw TRX, observations,
load reports, pre/post inventories and replay archives are retained under [evidence](evidence/selection.json).
The original primary Linux runner and the later archive-producing runner remain separate.

Linux uses SDK 10.0.301 in the existing pinned local image with network disabled and read-only
source/feed/observer mounts; execution/cache/fixtures are private container files. It runs on
overlayfs, not a physical power-loss/ENOSPC harness. Windows also uses SDK 10.0.301. Source-only
Git-free MinVer warnings in Linux are retained; no compiler warning policy is weakened.

## Verification and next step

The [data-only verifier](tools/verify-packet.py) checks artifact hashes, all test IDs/counts,
37-record equality, actual cohort/layout binding and frozen/store inventories. Seven detached
invariant sanity controls reject altered claims; these are not end-to-end malicious-tamper or
publisher-authentication qualification. An initial sanity-control setup shared the same mutable
expected/observed dictionary; the control caught that alias and the expected map was separated
before final verification. No product or result assertion was changed.

Next: Core and both consumer owners reconcile the concrete return into an accepted finite
profile/manifest and remaining semantic/cancellation boundaries, then refresh qualification on
any changed final cohort. FX009-03 sign-off, package adoption, release, all current M2 public/host/
archive gates, source replacement and retirement remain separate. CanReclaim=false; retain all
source/candidate histories. This packet and local Core handover are uncommitted; no remote send,
package publication, tag or product dependency change is claimed.
