# Independent source ownership qualification

1 November 2026. The unchanged production **1.39.0** factories from `f72db224` pass **151 checks twice**, including **48 new independent-owner controls** across XPath, XQuery and XSLT. [Qualification basis](SPEC-BASIS.md) records scope before the fixture change. No production source, API or version changes occur in this step.

Each host uses two distinct providers, sessions, assessments, node bundles and snapshots. Both evaluate successfully, and the same identity object can be admitted independently in their separate sessions. Foreign assessments, snapshots, input captures and node bundles refuse. Rejected duplicate and canceled acquisitions do not invoke the supplied release callback: until admission succeeds, resource cleanup remains the caller's responsibility.

Revoking and closing the first provider/session prevents result consumption while its raw asynchronous callback still holds the lease. The second source continues evaluating. The first release runs exactly once after the raw callback completes. A failing cleanup callback on an additional snapshot does not invalidate the second source's original snapshot; its final release also runs exactly once.

## Evidence and limits

[Both runs](evidence/probe-0.json), [repeat](evidence/probe-1.json), [build](evidence/build.txt) and [exact inputs](evidence/INPUTS.json) retain the results. The generated fixture builds with zero warnings/errors and checks every loaded engine hash against the sealed factory cohort. Its existing 103 factory/capture/host controls also pass. Setup reflection and diagnostic field inspection remain fixture mechanisms; execution uses production typed capture and direct host delegates.

This qualifies independent sources running side by side. It does not admit nodes or evidence from multiple owners into a single operation, perform native schema assessment, or implement acquisition rollback for a combined source group. Test issuers still provide authority; the ownership predicates are trusted host contracts. No throughput claim follows from the inherited component allocation measurements.

The [preceding full qualification](../foundation-private-host-factories/README.md) remains the source/build/test baseline: 24,341 unit passes, QT3 31,397/0/424, gated qt4 8,941/0/1,918, and unchanged 270 public types/5,770 declarations. These totals are inherited receipts, not fresh executions in this fixture-only step. Source and engine hashes remain identical; no engine change requires a new conformance run here.

Read-only verification: `python -B docs/foundation-independent-owners/tools/verify.py`.

## Next decision

Specify a private composite ownership contract before implementing a joint operation: immutable owner membership, per-source availability checks, acquisition order, rollback ownership before/after admission, deterministic cleanup and preservation of all failures. Compare a host-owned source group against a session-owned group and select the smallest contract that supports Schema, XmlDb and Braid. Keep the existing single-snapshot admission boundary until this is concrete. Native consumer qualification, exact numeric execution, C45 acceptance and API freeze remain pending.
