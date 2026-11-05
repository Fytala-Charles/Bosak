# Isolated Braid identity-policy prototype — 29 October 2026

An independent compiled C# prototype passes **47 contract controls**, detects **six deliberately weakened enforcement variants**, and matches assembly metadata/SHA-256 for **18 real engine files** across the qualified baseline/current closures. Build has zero warnings/errors; the final compiled run exits zero under an external 90-second bound. No Braid/core/Schema production source, pin, worker protocol or stored result is changed.

## What is qualified

| Area | Prototype evidence |
|---|---|
| Manifest identity | Deterministic ordered digest over reviewed revision and nine member names/assembly identities/hashes; member order and relocation do not change the digest, while content/revision changes do. Prototype format only, no production string adoption. |
| Admission/refusal | Baseline closure admits under its explicit approval; current closure refuses under old approval and admits only under separately supplied new approval in the model. Same-version byte changes, partial swaps, missing/duplicate/unreadable/unexpected members, invalid hashes and unsupported/unstable modeled bindings refuse. |
| Worker controls | Equal verified-looking model identities/profiles/request IDs agree; legacy/malformed identity, empty request/profile and mismatches refuse. Entitlement, validation, resource admission and freshness remain independent gates. These are simulated envelopes, not a new real worker implementation. |
| Retained results | Legacy/different identity is not current; model checks do not rewrite retained history. Real report-store read/reopen/migration and product UI behavior are not exercised. |
| Mutation controls | Omitting content hashes, checking the facade only, allowing incomplete observation, or ignoring worker identity/profile/entitlement each admits a targeted forbidden case and is detected. |
| Real file observation | Reads nine baseline and nine candidate engine PE identities/hashes; all match the prequalified artifact inputs. Disk-only observation still refuses runtime admission. |
| Recorded runtime evidence | Consumes assembly-name evidence from the earlier sealed Braid baseline load traces: eight engine members were observed and XQuery was not. The nine-member model refuses that incomplete observation. No new engine load or consumer execution occurs here. |

Initial receipt: 41 controls/six detected variants/18 file observations. Final receipt adds malformed/legacy protocol and recorded-trace controls to reach 47, without a product fix. Both source/result generations are preserved; the final tested source/project/assembly identities are recorded separately.

## Boundary exposed by the prototype

Actual Braid loads eight of the nine engine members in the exercised earlier scenario. A whole-closure policy that simply demands all nine have already loaded would refuse that valid lazy-loading workflow. The model correctly refuses to claim complete evidence, but this is a design issue to resolve before integration, not proof that the product is invalid or that XQuery must be forcibly loaded.

`StableBinding`, supported layout and loaded-member lists are explicit modeled inputs, not authoritative runtime capabilities. Parent/worker envelopes, operation rechecks, restart and stale-result handling are modeled. This prototype does not establish actual dispatch-to-loaded-assembly binding, loader race control, worker handshake enforcement or entitlement enforcement in Braid. It never loads arbitrary engine/private types to compute identity; real file observations use PE assembly metadata and hashing only. Prior actual consumer/load/worker findings remain inherited evidence.

## Next bounded step

Resolve and demonstrate a consumer-owned runtime observation/admission design in isolation before product integration. Keep the reviewed pin plus mismatch refusal policy. Compare (a) explicit operation-specific closures with admission checked again before a lazy dependency is used, and (b) an approved complete resolver/closure binding that verifies approved unloaded members when they load. Do not silently relax to facade-only checking, accept caller booleans as proof, or force engine loading merely for identity. The next proof must cover actual loaded paths/hashes, lazy XQuery behavior, process restart, post-initial-check replacement and parent/worker disagreement, with model and real runtime results separated.

Review/checkpoint this model and its policy first. An eventual Braid slice needs its consumer-owned design/registry/ADR, full suites, existing binary expectations, stored-result/protocol compatibility and applicable native acceptance. No core API need or new request ID is established. A normal product upgrade still requires a reviewed rebuild/pin; successful modeled new approval does not authorize one.

## State and evidence

The pre-source policy scope is retained as POLICY-SCOPE-SNAPSHOT.md; raw original/final output, source text, inputs, mutations and identities are under evidence/. All earlier sealed dossiers are unchanged. Core 1.34.1 and Schema 0.9 production source and qualifications remain inherited: 24,214 core passes, 451 Schema passes, QT3 31,397/0/424 and qt4 8,941/0/1,918 (230 sets). No full consumer/engine suite is rerun for this isolated model. Current policy/prototype documentation remains uncommitted and unpublished.
