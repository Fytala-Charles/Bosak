<div align="center">
  <img src="assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak release roadmap">
  <br><br><h1>Bosak roadmap</h1>
  <p>From the qualified engine foundation to an accepted public API and publication</p>
</div>

Updated 5 November 2026. Source is **1.61.0**; target publication version/date are **not selected**. R2 is ClosedWithScope and R3 is InProgress. The ordinary public facade, bounded primitive numeric profiles and internal typed-container reader are qualified within their recorded scope. Broader schema/all-host activation, actual consumer acceptance and API freeze remain pending.

The [release plan and checklist](docs/release-planning/README.md) is the planning entry point. Its [owner capability map](docs/release-planning/OWNER-CAPABILITIES.md) and [machine-readable gates](docs/release-planning/GATES.json) separate implementation evidence from release acceptance. Engineering checkpoints are retained in the development history and sealed dossiers rather than repeated here.

## Publication path

| Gate | Status | Required outcome |
|---|---|---|
| R0 Internal engine foundation | Qualified within bounded private profile | Preserve the existing implementation and regression/conformance baseline. |
| R1 Owner capability mapping | Audited, not owner accepted | Exact source pins and explicit Schema/XmlDb/Braid contract gaps. |
| R2 Final public contract disposition | ClosedWithScope | Seven exact scoped role returns validated; design-stage closure is separate from runtime acceptance. |
| R3 Public implementation | InProgress | Ordinary facade and finite numeric profiles qualified; TA-01 containers, TA-02 registry, TA-03 evidence, TA-04 adapter and TA-05 internal compiled identity qualified. Single-atomic public typed identity is qualified; broader schema/all-host integration remains. |
| R4 Actual consumer/performance qualification | Pending candidate; preparation parallel | Same immutable candidate for Schema, XmlDb and Braid; source/unchanged binaries, owner resources and agreed workload budgets. |
| R5 C45 acceptance/API freeze | Pending R4 | Explicit Core/Schema/XmlDb/Braid acceptance of the exact manifest/profile. |
| R6 Release candidate | Pending freeze; preparation parallel | Select version; package/provenance, migration/docs and fresh-cache package-only verification. |
| R7 Publication | Pending reviewed artifacts/approval | Publish and verify the actual public artifacts; consumer pin changes remain separate. |

## Immediate Core task

Use the [current dependency plan](docs/foundation-r2-consolidation/PLAN.json). [TA-01 internal container admission](docs/foundation-typed-container/README.md) is qualified. [TA-02 cache/projection policies](docs/foundation-typed-cache-policy/README.md) are selected and reference-checked. [TA-02 transactional registry](docs/foundation-typed-scalar-cache/README.md) is qualified. [TA-03 private attachment](docs/foundation-typed-evidence/README.md) is qualified. [TA-04 public-contract adapter](docs/foundation-advisor-adapter/README.md) is internally qualified. [TA-05 internal compiled typed identity](docs/foundation-typed-identity/README.md) is qualified. [TA-06 allocation qualification](docs/foundation-typed-identity-cost/README.md) reduces identity setup from 23,360 to 4,104 bytes/call. [TA-07 bounded public typed identity](docs/foundation-public-typed-identity/README.md) is qualified through existing signatures. [TA-08 internal numeric projection](docs/foundation-typed-numeric-projection/README.md) is qualified. [TA-09 public typed numeric execution](docs/foundation-public-typed-numeric/README.md) is qualified. [TA-10 internal original-facts reader](docs/foundation-node-facts-reader/README.md) is qualified. Next attribute/reduce repeated ownership callbacks before selecting the bounded public node-query profile. Original facts, nil/no-value, provider construction, compatibility and all-host routes remain separate work. Do not bypass public Unsupported boundaries or infer schema authority from annotation strings.

The [owner capability audit](docs/release-planning/OWNER-CAPABILITIES.md) retains actual source requirements. External implementation stays delegated. Actual candidate source/binary/resource and representative performance qualification precede four-owner C45 acceptance. Full XSD 1.1 backend completion is separate from representing and testing the foundation's required capabilities.

## Parallel owner work

- **Schema:** advance supported XSD 1.1 assessment and authentic provider facts; qualify represented supported/refused features on the selected candidate.
- **XmlDb:** preserve ordinary persistent XML compatibility; qualify actual reader/generation ownership, acquisition faults and supported concurrency.
- **Braid:** settle typed/persistent invocation, worker ownership/protocol and currentness; qualify the chosen process route and representative workloads.
- **Core:** prepare packaging/provenance, migration guidance and API/ABI/performance harnesses while closing the public contract.

Full XSD 1.1 backend completion is a separate lane. The foundation must represent all known required API needs and explicitly refuse unsupported backend coverage before the intended final breaking API freeze. General Braid product validation does not block the Core/Schema contract queue.

## Other continuing directions

Preserve default XPath/XQuery/XSLT behavior and frozen gates. Continue 4.0 adoption only against immutable specification revisions and separately gated test sets. Advance [source-preserving authoring](docs/STRUCTURAL-AUTHORING-ROADMAP.md) and streaming with explicit lifetime/resource limits. Performance changes require representative measurements and owner budgets.

Bosak and Bosak.Schema are Apache-2.0; schema services require no activation. Schema remains a separate package with preserved dependency direction and original component identity. Bosak.Folio remains proprietary.

## Evidence and history

Use [implementation status](docs/IMPLEMENTATION_STATUS.md), [integration guidance](docs/INTEGRATION.md) and the [feature registry](docs/FEATURE_REQUESTS.md) for engineering detail. The [release checklist](docs/release-planning/README.md) links current qualification. Earlier checkpoints remain in [development history](docs/DEVELOPMENT_HISTORY.md), their sealed dossiers and the [archived previous roadmap](docs/release-planning/inputs/ROADMAP-before-consolidation.txt).

Passing internal tests does not constitute consumer acceptance, C45 sign-off, package publication or API freeze. No publication date, completion percentage or unreviewed version commitment is inferred from source checkpoints.
