<div align="center">
  <img src="assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak roadmap">
  <br><br><h1>Bosak roadmap</h1>
  <p>Current direction for the open XML engine and schema foundation</p>
</div>

Last updated: **29 October 2026**. This roadmap replaces the old beta/paired-commercial-launch checklist. Detailed measurements and historical milestones are in [implementation status](docs/IMPLEMENTATION_STATUS.md) and [development history](docs/DEVELOPMENT_HISTORY.md#roadmap-snapshot-2026-10-29).

## Where we are

| Area | Current position |
|---|---|
| Core engine | Published XPath 3.1, XSLT 3.0 and XQuery 3.1 packages for .NET 10; [1.26.3 is the verified published baseline](docs/RELEASE-1.26.3-PUBLICATION.md). |
| Source development | Candidate 1.33.1 is unpublished; source qualification does not imply a package release. |
| Bosak.Schema | [Public and Apache-2.0](https://github.com/Fytala-Charles/Bosak.Schema); activation-free schema services. Full XSD 1.1 structural validation and engine integration remain incomplete. |
| 4.0 language support | Explicit opt-in support for documented draft features; complete current-draft conformance is not claimed. |
| Authoring and tooling | VS Code integration and bounded source-preserving APIs support Braid's developing visual/source workflow. |

## Next priorities

1. **Integrate the open schema foundation.** The first [runnable integration](integrations/Schema/README.md) uses existing APIs for XSD 1.0 validation and typed transformation. Next, establish an assessed-document result for staged XSD 1.1 without overstating structural validation. Preserve modular dependencies, original PSVI, component authority and resource/lifetime boundaries. Production session-API acceptance remains pending.
2. **Qualify real providers and consumers.** Build and test the actual Schema, XmlDb and Braid integrations, including compatible-schema handoff, lazy results and unchanged-consumer ABI checks, before freezing or releasing the new integration.
3. **Complete staged XSD 1.1 coverage.** Implement or explicitly refuse remaining structural/assessment features; publish precise support profiles rather than broad conformance claims.
4. **Reconcile 4.0 draft behavior.** Triage the recorded JNode, record, map and comparison gaps against immutable adopted specification revisions. Keep each semantic change and conformance admission separately reviewed.

The [feature registry](docs/FEATURE_REQUESTS.md) owns individual request status and acceptance criteria. Implementation work does not automatically approve an API, alter a frozen gate or authorize publication.

## Continuing work

- Improve source-preserving authoring through bounded, reviewed operation contracts; see the [authoring roadmap](docs/STRUCTURAL-AUTHORING-ROADMAP.md).
- Extend streaming support within documented streamability and resource limits.
- Measure runtime and transformation performance before selecting optimizations.
- Preserve default-language behavior, source compatibility and clear consumer migration paths as newer profiles develop.

## Open-source and commercial boundaries

Bosak and **Bosak.Schema are Apache-2.0**. Schema compilation and validation need no activation, trial or subscription. The schema implementation remains a separate package; using one license does not require introducing a core dependency cycle.

Braid and Folio retain their separate commercial/proprietary terms. Billing and key-generation infrastructure remains in the private Bosak.Licensing repository. The Schema transition did not publish a new NuGet package, change live billing or alter another product's license.

Commercial support and consulting can fund development without restricting access to the open engine or schema services. See [commercial offerings](COMMERCIAL.md) and the [ecosystem overview](docs/ECOSYSTEM.md).

## Compatibility, limitations and evidence

Changes follow [Semantic Versioning](https://semver.org/) and documented adoption/migration decisions. Extended-year construction does not imply every date operation supports that range; .NET decimal precision and external HTTP behavior also impose documented limits. See [implementation status](docs/IMPLEMENTATION_STATUS.md) and [integration](docs/INTEGRATION.md) for technical details.

Historical launch, release, test and pricing statements remain preserved as dated evidence in [development history](docs/DEVELOPMENT_HISTORY.md), not as current roadmap gates.
