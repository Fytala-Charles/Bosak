<div align="center">
  <img src="assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak engine and ecosystem roadmap">
  <br><br><h1>Bosak roadmap</h1>
  <p>Next steps for the published engine and its XML ecosystem</p>
</div>

Updated 9 November 2026.

## Published baseline

**Bosak 1.81.2** and **VS Code extension 0.2.6** are published. The ten Core NuGet libraries, extension and public downloads are verified in the [publication record](docs/release-1.81.2-publication/README.md); [GitHub release notes](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.81.2) describe the shipping scope.

The release includes the documented ordinary-source, bounded numeric and typed foundation profiles. The release owner approved the scoped API and delivery. Numerical workload budgets were explicitly deferred for this release, with **no performance guarantees claimed**. Production Schema/XmlDb/Braid adoption and broader schema/all-host coverage remain separate work.

## Next priorities

| Area | Direction | Completion evidence |
|---|---|---|
| Core compatibility | Preserve the released profiles, default XPath/XQuery/XSLT behavior and frozen conformance gates. Resolve demonstrated consumer gaps through bounded changes. | Regression/conformance checks, API/ABI review and migration guidance for the resulting exact artifacts. |
| Schema integration | Advance native assessment, authenticated provider facts and the represented supported/refused XSD profiles. Broader list/union and full structural coverage remain staged. | Actual public Schema source/provider integration and scoped correctness, identity and lifetime qualification. |
| Bosak.XmlDb | Advance embedded native XML storage and persistent XDM navigation; qualify reader/generation ownership, acquisition/cleanup faults and supported concurrency against the released Core. | Exact source and unchanged-binary compatibility, deterministic lifetime evidence and an explicit database delivery decision. |
| Bosak.Braid | Adopt the released engine through explicit consumer pin changes; advance typed/persistent preview, worker ownership/currentness and source/visual authoring. | Qualified chosen process route, actual product workflows and scoped owner acceptance. |
| Workloads and budgets | Select representative Core/Schema/XmlDb/Braid workloads, comparable independent baselines, finite limits and numerical budgets. | Agreed metrics, units, statistics and tolerances with raw correctness, performance and resource-lifetime receipts. |
| Release maintenance | Keep package, server, extension, migration and documentation identities aligned. Reuse reviewed artifacts for publication. | Fresh consumer checks, artifact/provenance review and service-side delivery verification for each release. |

No next release version or date is committed here. The completed 1.81.2 approval applies to its recorded scope; broader provider adoption and future profiles need their own decisions and qualification. Historical design returns and compatibility probes do not automatically accept new workloads or production delivery.

## Continuing language and authoring work

Continue opt-in 4.0 adoption against immutable specification revisions and separately gated test sets. Advance [source-preserving authoring](docs/STRUCTURAL-AUTHORING-ROADMAP.md) and streaming with explicit resource and lifetime limits. Preserve explicit unsupported outcomes where backend coverage is incomplete. Future performance claims require representative measurements and agreed budgets.

Full XSD 1.1 backend completion is a separate Schema direction. Core must preserve dependency direction, original component identity and supported/refused behavior as Schema coverage grows. Further API changes follow semantic versioning and explicit compatibility/migration review.

## Ecosystem boundaries

Bosak and Bosak.Schema are Apache-2.0; Schema services require no activation. Bosak.XmlDb is a separate proprietary storage project in development, distinct from the published Apache-2.0 `Bosak.XPath.Providers.Database` REST loader. Bosak.Braid and Bosak.Folio retain separate commercial/proprietary product terms. A Core release does not announce a Schema, database, authoring application or formatter release. See the [ecosystem overview](docs/ECOSYSTEM.md).

## Engineering detail and history

Use [implementation status](docs/IMPLEMENTATION_STATUS.md), [integration guidance](docs/INTEGRATION.md) and the [feature registry](docs/FEATURE_REQUESTS.md) for exact profiles, engineering checkpoints and unresolved requirements. The [release guide](docs/RELEASING.md) describes the current preparation/publication process. Earlier R0-R7 planning snapshots remain in [release planning](docs/release-planning/README.md) and [development history](docs/DEVELOPMENT_HISTORY.md); they retain their original candidate identities and do not override the published release disposition.
