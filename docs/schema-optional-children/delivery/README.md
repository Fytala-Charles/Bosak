# Optional decimal scope documentation delivery — 29 October 2026

The [adoption record](../OPTIONAL-DECIMAL-ADOPTION-2026-10-29.md), [implementation plan](../OPTIONAL-DECIMAL-PLAN.md), owning Schema guides and core canonical notices are synchronized. Optional-child support is not implemented. Current candidate versions remain Schema 0.5 and core 1.34.0; proposed implementation targets are Schema 0.6/core 1.34.1.

The scope's 13 diagnostic observations and fresh 204-case Schema baseline are preserved. Core production source remains identical to the reviewed 24,209-pass and exact-gate checkpoint; no fresh core/gate test run is claimed for documentation delivery. The future core binding change requires fresh full tests/gates. No registry completion, consumer adoption or general API freeze is inferred.

## Paired delivery

User authorization: synchronize documentation, commit and push to remote. Core remote target is `origin/checkpoint/restart-2026-10-29`, the existing draft PR #467 backup/review branch; local main is not merged into remote main. Schema remote target is `origin/feature/bounded-document-assessment`. No force push, merge, tag or package publication is included.

See [paired identities](CHECKPOINT.json). Core delivery revision can be resolved with `git log -1 --format=%H -- docs/schema-optional-children/delivery/README.md`; this avoids embedding a commit's own hash into its content. The Schema owning delivery guide uses the same resolver pattern.

## Evidence preservation

The original scope manifest and README capture are retained under `evidence`. The scope README's stale uncommitted notice is updated to link here; the refreshed scope manifest records that documentation correction separately. All original runtime receipts, observations, normative records and source identities remain unchanged. Earlier sealed integration/assessment/bridge dossiers are untouched. HTML/local-link validation and the strict Schema documentation check have separate delivery receipts.

Next: implement the bounded plan, preserving the pre-implementation scope and original receipts.

Schema scope delivery is committed and remote-verified at `33a9039a9fc605f8f7192bfcec88ea559c5c0e19` on `origin/feature/bounded-document-assessment`; its working tree is clean. Core delivery target remains `origin/checkpoint/restart-2026-10-29` (draft PR #467). Resolve the core delivery with `git log -1 --format=%H -- docs/schema-optional-children/delivery/README.md`. No merge or release.
