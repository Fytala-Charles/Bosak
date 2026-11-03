# Open-session drain prerequisite

3 November 2026. Source **1.44.2** lets the internal ordinary-source session observe active work without closing first. [Adoption basis](SPEC-BASIS.md), [status](STATUS.json), [acceptance controls](ACCEPTANCE.json).

## Behavior

An idle open session completes drain immediately. A busy session returns a shared task for the current busy epoch, including source-free callbacks and pending snapshot admission or rollback. Completion leaves admitted snapshots and their owner leases usable. Later work begins a separate epoch; drain is an observation, not an exclusive admission barrier. Do not await an epoch from a callback whose completion that epoch requires.

Canceling a caller's wait leaves the underlying work, shared observation and owner resources intact. Close still revokes admissions immediately and retains the stronger drain contract: all deferred snapshot releases complete, with cleanup errors preserved. An earlier open observation waits for raw completion but does not replace that closed-session cleanup receipt. Lazy completion sources are allocated only when a busy drain is requested.

## Qualification

Release solution build and all **24,475 tests** pass, including eight new controls (32 ordinary admission controls total; Runtime 451). QT3 remains **31,397/0/424** and gated qt4 **8,941/0/1,918**. Both skip maps are byte-identical. All eleven public inventories, 270 exported types, 5,770 metadata rows and friend grants remain unchanged.

[Source and binary pins](evidence/SOURCE-AND-BINARY-PINS.json), [unit counters](evidence/UNIT-RESULTS.json), [loaded bindings](evidence/LOADED-BINDINGS.json), [public API audit](evidence/API-COMPARISON.json), [changed source](evidence/CHANGED-SOURCE.zip), [engines](evidence/ENGINE-BINARIES.zip) and [execution receipts](evidence/EXECUTION-RECEIPTS.zip) retain this cohort. The separate nonshipping 83-control capture prototype was not rerun for this slice; its earlier receipts remain historical. Run `python docs/foundation-open-session-drain/tools/verify.py` to check retained qualification.

## Next work

Promote the accepted thin ordinary-source public facade after completing its bounded profile and cancellation propagation. All-host integration, numeric quotas and broader typed behavior, native owner qualification, C45, API freeze and publication remain pending. This prerequisite changes no owner checkout or pin.
