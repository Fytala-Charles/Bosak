# Isolated function-table allocation fix

3 November 2026. Source **1.44.1** reduces the measured captured-context allocation without changing the public API or pooling evaluation contexts. [Status](STATUS.json), [adoption basis](SPEC-BASIS.md), [20 new registry controls and retained schema/ordinary controls](ACCEPTANCE.json).

## What changed

A fresh evaluation context allocated 2,640 bytes; populating its 379-entry standard function table raised that to 21,896 bytes. Internally created isolated contexts now reuse a privately owned, ordered read-only mapping snapshot and its variadic index. Snapshots are weakly cached by the trusted immutable standard-template identity. Existing function-signature objects were already shallow-shared by legacy table copies; this does not promise deep immutability of signature metadata.

The existing private registry slot is tagged as either an owned mutable dictionary or a shared table, adding no per-context fields. Registration/removal materializes a context-local copy; clear and restore replace the shared view. Repeated population with the same untouched profile reuses it; profile changes materialize and merge, retaining historical overwrite, ordering, visibility and variadic behavior. Function snapshots remain detached writable copies and lexical aliases stay context-local. Struct enumeration avoids a new boxing cost on existing paths.

Publicly constructed contexts retain their mutable registry path. No focus, variables, namespaces, schema/evidence, time, documents, resource policy or execution gate is shared, and no context pool/reset protocol is introduced. W1-A/B ownership, revocation and before-getter checks remain intact.

## Measured result

[Three fresh-process comparisons](evidence/COST-OBSERVATIONS.json) use the retained exact 1.44.0 binaries and observer against 1.44.1, with 200 warmups and 4,000 iterations per row.

| Scenario | Before | After |
|---|---:|---:|
| Public fresh context | 2,640 bytes | 2,640 bytes |
| Public fresh context plus standard population | 21,896 bytes | 21,896 bytes |
| Isolated copied context plus population | 21,896 bytes | 2,640 bytes |
| Ordinary captured XPath operation | 24,680 bytes | **5,424 bytes** |
| Assessed captured identity evaluation | 25,280 bytes | **6,024 bytes** |
| Legacy warm typed read | 88 bytes | 88 bytes |
| Guarded assessed warm scalar read | 1,240 bytes | 1,240 bytes |

The captured-operation saving is **19,256 bytes each**: about **78.0% ordinary / 76.2% assessed identity** in these fixtures. The first isolated context plus population allocated 22,712 bytes after the public library was warm, reflecting one-time snapshot/cache setup for that profile; this cost is retained separately rather than hidden in warmed numbers. It is allocated memory, not a retained-heap or worst-case memory measurement. Mutating a shared registry pays for its local copy. Timing samples remain observations; no owner workloads, budgets or hard latency guarantees are accepted here.

## Qualification

- Complete Release build passes; retained full rebuild has five unchanged test/conformance warnings and zero errors.
- **24,467 tests pass across all ten projects**, including 20 new registry controls, 33 schema-association controls and 24 ordinary-source controls. Database uses its authorized local HTTP fixture; all 8,234 XSLT tests pass normal layout.
- QT3 remains **31,397/0/424**; gated qt4 remains **8,941/0/1,918**, with 11 experimental passes and zero gated/exploratory failures. Both frozen skip maps are byte-identical.
- All **11 shipping public inventories / 270 types / 5,770 rows** and original friend grants are unchanged.
- The distinct private capture-extension cohort passes **83 controls twice**, retaining nonshipping prototype methods; this is not public SDK or native unchanged-binary acceptance.

The controls compare all six standard profiles and profile transitions against public-context behavior, replacement and variadic removal, repeated population, clear/snapshot/restore, lexical aliases, caller/dynamic-state isolation and a stable allocation-ratio regression. The full Runtime project has 443 passes.

[Exact unit/TRX counters](evidence/UNIT-RESULTS.json), [source/binary pins](evidence/SOURCE-AND-BINARY-PINS.json), [loaded engine hashes](evidence/LOADED-BINDINGS.json), [public API comparison](evidence/API-COMPARISON.json), [changed source](evidence/CHANGED-SOURCE.zip), [eleven engines](evidence/ENGINE-BINARIES.zip) and [original execution receipts](evidence/EXECUTION-RECEIPTS.zip) preserve this cohort. The archive retains the development test build that attempted to set a read-only date/time property; the final timezone-isolation control and all qualification runs pass. [Private regression source](evidence/BCAP-REGRESSION-SOURCE.json) and [binary bindings](evidence/BCAP-REGRESSION-BINDINGS.json) distinguish its generated gate-capture variants from production.

## Next work

Continue the accepted thin ordinary-source public facade and all-host integration. Wider typed/list/union/numeric/fact/construction/compatibility implementation, native consumer qualification and representative performance budgets remain pending. This private source patch changes no owner checkout/pin, reviewed declaration manifest, API freeze, release version/date or publication authorization.
