# Isolated actual-runtime admission experiment — 29 October 2026

The independent runtime experiment passes **34 actual controls** with **nine independent worker-process exchanges**. Real public XSLT and XQuery operations execute from approved immutable assembly-byte snapshots through a guarded collectible AssemblyLoadContext. This resolves the model's eight/nine-member lazy-loading boundary for this loader: approve all nine snapshots up front, then bind each approved dependency only when an operation needs it. No Braid/core/Schema production implementation or actual dependency pin changes.

## Fresh evidence

| Boundary | Observed result |
|---|---|
| Lazy real operations | XSLT returns `<ok>2</ok>` and loads eight engine members; XQuery is not forced. A later real query returns 3 and binds the ninth member. |
| Binding provenance | Every engine load is fed the approved byte snapshot; loaded assembly full identity and MVID match that snapshot's PE metadata. Ledger records snapshot SHA-256, source-artifact path and actual Assembly.Location (empty for stream loads). No engine falls back to the default load context. |
| Explicit approval | Baseline 1.26.2 executes under baseline approval; 1.34.1 files refuse under old approval. Separately supplied current-artifact approval executes both operations; this does not adopt it into Braid. |
| Artifact refusal | Facade-only substitution, corrupt bytes with unchanged metadata, missing lazy member and duplicate approval members refuse admission. |
| Drift and revocation | Replacing XQuery before deferred use refuses without loading it. Restoring the file does not automatically readmit a revoked session; a fresh verified session succeeds. Changing an already-loaded deployed member refuses the next dispatch. |
| Controlled check/dispatch interval | A file is replaced after the disk precheck but before lazy query use. The operation still loads the earlier approved baseline snapshot, verified by hash/identity/MVID; the next dispatch detects disk drift and revokes. This is a controlled event-order test, not exhaustive adversarial race or concurrent-disposal qualification. |
| Actual workers | Independent child processes run transform/query, report nine loaded members and agree on identity. Restart uses a new PID and the same approved identity. Replaced files under old approval and different worker approval/profile refuse before engine execution. |
| Response rejection | Four deliberate worker response faults (request ID, profile, identity and protocol version) are rejected by the parent after real child execution. A transport process can exit zero with a refused response; transport success is not admission. |

Both runtime host and baseline-API operation plugin build with zero warnings/errors. Final execution exits zero within an external 120-second bound; each child has a 15-second watchdog with process-tree termination on timeout. No timeout was observed. The plugin is compiled once against baseline public engine APIs, returns strings across the context boundary, and is reused unchanged with both approved engine versions. Only the experiment's own public plugin methods are invoked reflectively; no engine/private API or Braid friend-access bypass is used.

## Admission design demonstrated

The experiment hashes and reads PE metadata from the same captured bytes, retains those private snapshots, and verifies them against the approved artifact manifest. It does not treat a caller-supplied stable-binding Boolean or observed version string as runtime proof. A snapshot-backed resolver admits only approved engine members and compatible reference identities. Engine DLLs may remain unloaded until needed, but their bytes are already fixed. Before each operation, deployed files are checked for drift; detection permanently revokes that session. Relocation does not change the approved runtime identity.

The snapshot itself is the binding boundary during the controlled precheck/dispatch interval. Disk drift after a check cannot substitute new engine bytes into that context. This does not freeze the filesystem, protect against hostile code inside the process, prove signing/manifest authenticity or certify arbitrary loaders. An approved deployment manifest and its distribution remain an external trust input.

## Harness corrections and scope limits

Initial plugin compilation failed because a copied scratch header carried a mojibake BOM prefix. Normalizing that new experiment header fixed compilation; the initial log is preserved. The first runtime attempt refused the baseline because its PE identity builder omitted an explicit null public-key-token representation. The harness now constructs that metadata identity consistently; the original failure is preserved separately. Missing/unreadable artifact errors are treated as refusal, not engine execution success. No product fix resolved these harness issues.

This is real runtime/worker evidence for an independent, single-threaded host and explicitly approved payloads, not actual Braid adapter/worker integration. The Braid product uses its existing loader and protocol; those remain unchanged. The experiment's versioned identity and JSON messages are diagnostic formats, not adopted product contracts. Actual entitlement, retained-history migration, UI/native/accessibility, complete API/type interchange, default-loader admission, single-file deployments, hostile path/loader races, parallel evaluations, unload/GC completion and performance are not qualified. Requesting collectible-context unload is not proof of collection. Existing full core/Schema/consumer results remain inherited, not rerun.

The plugin returns strings rather than engine-typed objects across load contexts. Applying this technique to Braid requires a consumer-owned boundary/type-identity design; it is not a drop-in fix for existing default-context adapters. In particular, snapshot stream loads have no file Location and must not be presented as path-loaded engine evidence. Source paths in the ledger identify captured artifacts; snapshot hashes/MVIDs identify the actual bound payloads.

## Next step and state

Review and checkpoint this experiment, then prepare a consumer-owned integration design comparing an isolated snapshot-backed execution boundary with reviewed deployment/default-loader mismatch refusal. Preserve Braid's original pin until that design, complete consumer suites and applicable native/protocol/history acceptance justify a real adoption. No new core API need or request ID is established.

Current runtime experiment/evidence and synchronized documentation are uncommitted. All earlier sealed dossiers, original consumer checkouts, core 1.34.1 and Schema 0.9 production source remain unchanged. Their earlier 24,214/451 unit passes and exact QT3 31,397/0/424 / qt4 8,941/0/1,918 (230 sets) remain inherited.
