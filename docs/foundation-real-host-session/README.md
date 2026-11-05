# Real private session: bounded all-host integration

1 November 2026. The adapter exercises actual production SchemaQuerySession and SchemaEvidenceKernel from the qualified **1.37.0** cohort across XPath, XQuery and XSLT. It replaces the earlier modeled lifetime with real private admission/lease code. No production source, version, binary, public API, friend grant or owner pin changes in this step. [Basis](SPEC-BASIS.md) preceded execution.

Two guarded processes produce identical [75-check receipts](evidence/run-0.json) and [repeat receipts](evidence/run-1.json). The scratch fixture builds with zero warnings/errors. [Source/binary pins](evidence/INPUTS.json) verify all ten canonical engine hashes remain unchanged and every actually loaded engine has the qualified hash. The test assembly uses an existing Runtime.Tests friend identity; XSLT's exact four-argument internal bridge is reflected. No grant is added. Earlier 60-check and pre-allocation receipts remain historical observations, not independently replayable source snapshots.

## What ran

Each host has 25 checks covering captured input bindings, real host evaluation/atomization, unchanged caller state on success/error/async completion, foreign environment/snapshot refusal, canceled admission and canceled waits, deferred consumption, escaped results, close/release ordering, repeated close, combined callback/cleanup errors, and unsupported contexts/results. XPath and XQuery return `42|10|A`; named-template XSLT returns `42|10|B|B`, using explicit global context B rather than match document A. The ordinary typed test node has lexical text 999 and decimal typed value 10.

| Boundary | Evidence and limit |
|---|---|
| Real private operation | Session.Run/RunAsync and actual kernel snapshot leases execute; cleanup is not an adapter counter model. Release actions and issuer are test doubles. |
| Static producer | XPath uses the existing private compilation factory. XQuery/XSLT use fixture producers with bounded descriptors; these are not production factories or complete effective-context capture. |
| Snapshot ownership | One fixture-owned composite snapshot contains the admitted roots. Foreign snapshots reject; independent multi-snapshot acquisition is not implemented. |
| Caller context | Nine selected private fields are copied into fresh contexts; all 60 other fields must match fresh-context defaults. Bindings/namespaces are captured before deliberate caller mutations, and all 69 observed fields remain unchanged by execution. |
| Unsupported state | Configured loader callbacks, nondefault mutable decimal formats, foreign node inputs and sequence inputs reject. This is not general EvaluationContext support or an untrusted-context security audit. |
| Result lifetime | A fixture result adapter allows bounded flat atomics and checks the actual operation before reading. Structured results and consumption after callback completion reject. Raw engine graphs are not automatically guarded or detached. |
| Close | New access rejects immediately, including through an already admitted operation; resource release waits for the raw callback to finish. Canceled waits do not cancel that task or release resources. |

The fixture's admission audit uses reflection and diagnostic value descriptions. It is not proposed production cloning code and must not become a security boundary for arbitrary caller-provided objects. Node graphs remain borrowed; binding copies do not freeze or authenticate their contents. Native assessment/PSVI issuance, exact numeric transport and actual XmlDb/Braid resources are not covered by the test issuer/node.

## Measured context costs

Warmed single-thread allocated bytes per operation, averaged over 1,000 iterations in each process; both captures agree. These exclude compilation, host evaluation, operation/result wrappers, worker-thread allocation and XSLT stack memory. They are fixture measurements, not consumer budgets.

| Operation | Bytes/op |
|---|---:|
| Fixture input capture including reflective default-state admission | 33,461 |
| Fresh EvaluationContext populated from admitted inputs | 2,920 |
| Additional XQueryContext wrapper/copy from a populated child | 3,744 |

The XQuery route currently creates both the fresh child and a wrapper copy. Production work should use an explicit typed input contract and a direct host bridge, avoiding reflective admission and duplicate context creation. Preserve caller isolation and the tested refusal behavior while measuring any replacement against these same fixtures. No throughput or workload budget is selected.

## Disposition and next step

The existing delegate/session boundary can execute all three hosts with the selected private lease semantics and fresh-context isolation. This proves a bounded integration route; it does not finish the proposed public facade, 43 owner requirements, general caller restoration, independent multi-source operations or authenticated provider binding. C45, consumer acceptance and API freeze remain pending.

Next implement a typed internal input/result boundary for this selected profile and direct XQuery/XSLT host bridges, with explicit refusals and measured costs. Production factories must capture effective static context and bind the actual owner/lifetime contract before public promotion. Preserve the current close semantics rather than adopting the earlier model's ability to continue new reads after close.

Replay: `python -B docs/foundation-real-host-session/tools/run.py`; read-only verification: `python -B docs/foundation-real-host-session/tools/verify.py`. Full solution/gates were not rerun for this fixture-only step; their [1.37.0 production receipts](../foundation-xslt-context-promotion/README.md) belong to the unchanged engine cohort and are not new runs here. All negative cases are caught with scoped child error mode and bounded waits; no deliberate unhandled crash or machine-wide error setting change occurred.
