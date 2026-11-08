# Finite foundation contract decision — 30 October 2026

This is a review proposal for C139-01/C45, not acceptance of a production API. The [matrix](DECISION-MATRIX.json) partitions the sealed prototype into six groups: 32 types and 377 reflected member entries. These entries include generated record members and accessors; their count is not a proposed minimum API size. The [core readiness packet](../foundation-readiness/README.md) retains the qualified artifact and remaining owner requirements.

| Group | Types | Recommended decision boundary |
|---|---:|---|
| CD-01 Semantic profile | 3 | Capture actual host, XDM, schema-language and static-context semantics immutably. |
| CD-02 Provider/session requests and outcomes | 11 | Select operations required by actual consumers; preserve explicit refusal, cancellation and resource ownership. |
| CD-03 Authority and provenance identities | 9 | Preserve original universe, session, source and component identity. Public issuer factories require separate justification. |
| CD-04 Descriptor shape | 2 | Expose query-needed facts; keep the complete validator model in Schema. |
| CD-05 Node/atomic annotations and carrier | 4 | Preserve original PSVI, list/union values, copies and lifetime. Prototype atomic helpers do not prove a production carrier or ABI. |
| CD-06 Compatible independent sessions | 3 | Require directional endpoint/profile-bound authority without replacing original annotations. |

The exact prototype signatures are copied for review, not approved wholesale. Before implementation, reduce this inventory to an explicitly named member manifest, record rejected/deferred helpers and justify every retained public signature against a consumer requirement. In particular, record convenience methods, issuer helpers and carrier attachment methods must not become permanent merely because the prototype exposes them.

## Committed owner observations

[Owner inputs](OWNER-INPUTS.json) pin selected committed files and their hashes. Inspection was read-only, with no fetch, owner builds or owner messages. Concurrent XmlDb working changes were excluded. The selected records contain no returned acceptance of this candidate; this is not an exhaustive claim about all external communication.

The active public Schema checkout at `2009018` adds negative wildcard descriptor constraints and transferred historical design evidence. Its public 0.4 line reports 124 tests in its own handover; Core did not run those tests. Core session integration remains Pending and full XSD 1.1 structural validation remains incomplete. Historical seam documents retain their historical framing and do not establish current adoption. This checkout is separate from the older isolated 0.9/v5 assessment candidate at `243cc60`; their versions, capabilities and qualifications must not be combined.

The active XmlDb checkout at `2f9a36b` advances stopped-handoff storage work. Its optional Schema integration and XDM 4.0 provider qualification still await the Core foundation decision. Its dirty working tree was neither used as committed evidence nor modified. The isolated foundation registration remains pinned separately at `0808595`.

## Required invariants and disposition

Preserve required `IXdmNode`/sequence members, XML enum values, `XdmValue` layout and existing BCL schema defaults. A native route must be explicit and captured per executable. Core must not depend on concrete Schema, XmlDb or licensing implementations. Callers retain snapshot/resource ownership; nested or deferred values must not imply ownership transfer. Names or digests alone must not substitute for compatibility authority.

Required foundation gaps for full XSD 1.1 and XmlDb block the shared API freeze even when bounded legacy tests pass. Backend conformance remains a separate qualification. Existing package tests do not qualify actual external consumers.

Use the blank [disposition template](DISPOSITION.template.json) to record exact signatures, owner requirements, immutable specification adoption and a bounded implementation scope when those decisions exist. Every decision is currently Pending; implementation, freeze and release flags remain false. Generic continuation does not supply those decisions.

The next core action is to reconcile returned concrete owner requirements against these six groups and produce the smallest named C139-01 implementation scope. No generic session API should be implemented from the full prototype inventory before that scope and its semantic adoption evidence are recorded.

## Verification

Run `python docs/foundation-contract-decision/tools/verify.py` from the repository root. It checks exact prototype coverage, committed owner input hashes, pending disposition and dossier hashes. This documentation step changes no product source or package version; the existing readiness packet owns prior runtime and gate results.
