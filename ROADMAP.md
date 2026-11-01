<div align="center">
  <img src="assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak release roadmap">
  <br><br><h1>Bosak roadmap</h1>
  <p>From the qualified engine foundation to an accepted public API and publication</p>
</div>

Updated 1 November 2026. Source is **1.42.0**; the next publication version/date are **not selected**. The internal bounded foundation is qualified. The public foundation contract, actual consumer acceptance and API freeze remain pending.

The [release plan and checklist](docs/release-planning/README.md) is the single planning entry point. Its [owner capability map](docs/release-planning/OWNER-CAPABILITIES.md) and [machine-readable gates](docs/release-planning/GATES.json) separate implementation evidence from release acceptance.


Current execution update (1 November): [P02 isolated capture declaration candidate](docs/foundation-bcap-candidate/README.md) compiles; owner feature requests are registered locally. P03 real capture-producer feasibility and C01–C10 are next. BCAP-01 remains open pending runtime evidence and Braid review.


P03 progress (1 November): [real capture feasibility](docs/foundation-bcap-runtime/README.md) passes 26 bounded controls twice and 631 selected existing regressions on isolated binaries. P03 remains InProgress for controls/fault/native qualification; production and owner acceptance remain unchanged.

## Publication path

| Gate | Status | Required outcome |
|---|---|---|
| R0 Internal engine foundation | Qualified within bounded private profile | Preserve the existing implementation and regression/conformance baseline. |
| R1 Owner capability mapping | Audited, not owner accepted | Exact source pins and explicit Schema/XmlDb/Braid contract gaps. |
| R2 Final public contract disposition | In review | Five returns: three scoped accepts/two revisions; Schema O1/O2 missing. |
| R3 Public implementation | Pending R2 | Implement and document accepted contracts with API/ABI and semantic/lifetime tests. |
| R4 Actual consumer/performance qualification | Pending candidate; preparation parallel | Same immutable candidate for Schema, XmlDb and Braid; source/unchanged binaries, owner resources and agreed workload budgets. |
| R5 C45 acceptance/API freeze | Pending R4 | Explicit Core/Schema/XmlDb/Braid acceptance of the exact manifest/profile. |
| R6 Release candidate | Pending freeze; preparation parallel | Select version; package/provenance, migration/docs and fresh-cache package-only verification. |
| R7 Publication | Pending reviewed artifacts/approval | Publish and verify the actual public artifacts; consumer pin changes remain separate. |

## Immediate Core task

Use the [consolidated answers and execution plan](docs/foundation-r2-consolidation/README.md). **Core next: P02/P03, the operation-bound bounded XSLT capture candidate and targeted evidence for BCAP-01**, followed by Braid review. In parallel, XmlDb resolves XDB-LIFE-01 (actual cleanup/failure profile) and prepares provider-local membership; Schema submits formal O1/O2 answers using its checklist. Five role returns are submitted: Core O2/O4 and XmlDb O1 accept scoped design, while Braid O3 and XmlDb O4 request revision. Only Schema's two formal returns are missing. Resolve all known gaps and explicitly rebind any revised draft before R2 closes and accepted R3 work starts. Full XSD 1.1 backend completion remains separate from required foundation signature completeness.

The [capability audit](docs/release-planning/OWNER-CAPABILITIES.md) identifies actual source paths and remaining owner evidence, including XmlDb acquisition cleanup/failure preservation. Core owns contract selection and implementation; external implementation remains delegated. Do not add more synthetic lifecycle experiments unless they resolve a named decision.

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
