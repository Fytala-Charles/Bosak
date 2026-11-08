# Reviewed whole-document bridge checkpoint — 29 October 2026

The [bounded API review](../API-REVIEW.md) is complete. Attachment now computes its replacement before removing existing state, so a bounds failure preserves the original binding. Two new provider regressions cover replacement and initial-attachment failure. The host contract explicitly requires immutable state and the correct assessed value; attachment itself performs no validation.

The earlier [bridge](../README.md), [native assessment](../../schema-assessment/README.md) and [integration](../../schema-integration/README.md) dossiers remain historical, sealed evidence. Their original results and source identities are not rewritten. Fresh post-review tests and source identities are in this directory's RESULTS.json and evidence folder.

## Local checkpoint scope

User instruction: continue the recommended API review and local commits. The core checkpoint includes the previously uncommitted assessment documentation and bridge source/tests; the separate Schema checkpoint includes the native assessment, whole-document exports, tests, example and owning documentation. Core candidate remains 1.34.0; Schema remains on its 0.5 development line. Dependencies are unpublished, so the explicit local source override remains required. No push, tag, package publication or merge is included.

Delivery identities are recorded in CHECKPOINT.json. Core delivery can also be resolved with `git log -1 --format=%H -- docs/schema-document-bridge/api-review/README.md`; this avoids embedding a commit's own hash in its content.

## Next slice

Prepare a separate adoption/scope record for optional decimal children (`minOccurs="0"`) before changing native structural semantics. Preserve sequence order, assertion behavior, original component identity and explicit unsupported coverage. General sessions, compatible-universe admission and broader consumer adoption remain separate. This review does not implement optional children or freeze the general public API.

Post-review verification: **24,209 core tests pass in one complete solution run**, **204 Schema tests pass**, and all six focused provider attachment cases pass. Release build: zero warnings/errors. QT3 **31,397/0/424**, qt4 **8,941/0/1,918** (230 gated sets, 11 experimental passes), with exact skip mappings unchanged. The full-document example retains original root state and passes typed-value and preserve/strip checks. Prior sealed assessment, bridge and integration receipts remain unchanged.
