<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak engine, Schema and Braid ecosystem">
  <br><br><h1>The Bosak ecosystem</h1>
  <p>One XML engine, optional schema awareness and a visual-authoring application</p>
</div>

Bosak brings XPath, XSLT and XQuery into .NET applications. Bosak.Schema provides optional Apache-2.0 schema integration and staged XSD 1.1 services without activation. Bosak.Braid uses the engine's source-preserving APIs to build a visual/source XSLT authoring workflow.

| Component | Role | Availability and next step |
|---|---|---|
| Bosak | Apache-2.0 XPath 3.1 / XSLT 3.0 / XQuery 3.1 engine; documented opt-in 4.0 draft features | Public .NET 10 packages. Start with the [README](../README.md#quick-start). |
| Bosak.Schema | Optional Apache-2.0 schema integration and staged XSD 1.1 services | Public [source](https://github.com/Fytala-Charles/Bosak.Schema) and [integration guide](https://github.com/Fytala-Charles/Bosak.Schema/blob/main/docs/INTEGRATION.md); no activation, trial or subscription. |
| Bosak.Braid | Separate visual/source XSLT application, with reviewed authoring and execution workflows | Active development in a private repository. [Contact Fytala](mailto:charles.korthout@fytala.com) for access and delivery information. |
| Bosak.Folio | XSL-FO-to-PDF formatter in development | Remains proprietary with separate product terms. |
| VS Code extension | Editing assistance and evaluate/run commands | Public [Marketplace extension](https://marketplace.visualstudio.com/items?itemName=fytala.vscode-bosak); its bundled engine follows a separate release cycle. |

## Why schema awareness helps

A schema can describe more than the presence of a field: complex types express attributes, child structure and content relationships. Using those types in a transformation helps detect invalid document structure at a validation boundary and makes type expectations explicit in the transformation rules.

Schema's documented implemented profile includes `xsl:import-schema`, named complex-type checks through `instance of` / `treat as`, and schema-validated construction through supported validation/type attributes. Its staged XSD 1.1 services cover selected compilation/assertion capabilities; complete structural validation and full component/PSVI integration remain incomplete. The core retains its own simple-type support.

Both core and Schema are Apache-2.0. Schema remains a separate module with its own supported profile and compatibility lifecycle; no Schema version is implied by a core release. The Apache transition published source without a new Schema NuGet release. See the [verified publication record](SCHEMA-APACHE-PUBLICATION-2026-10-29.md). Billing/key-generation infrastructure and private history remain outside the public schema repository.

## Braid's bidirectional builder direction

The goal is to work on an XSLT transformation through its source and a visual model, with coordinated selection and trustworthy source changes. Development of that builder is underway. The engine remains responsible for syntax, supported editing positions, XML semantics and candidate generation; Braid handles presentation, frozen review, current-revision checks and explicit Apply/history.

| Delivered foundation | Builder work still ahead |
|---|---|
| Source-to-outline and outline-to-source navigation; highlighted source review | Free-form editable drafts coordinated with inspection and accepted source |
| Reviewed principal expression edits | Broader visual composition and operation-specific authoring contracts |
| Bounded value-of insertion/deletion and literal text insertion, with explicit formatting choices | Full live bidirectional graph/source synchronization |
| Exact source-change review, explicit Apply and one-step Undo/Redo | Wider structural operations and their separate acceptance gates |
| Validation, preview, restricted new-file export and independent reopen workflows | Delivery beyond the exercised supported profiles |

The [REQ-131 consumer acceptance receipt](REQ-131-BRAID-ACCEPTANCE.md) reports 1,079 normal Braid tests, 298 fresh public-package checks and product-owner completion of the native text-insertion workflow with matching hashes. These are attributed consumer results, not new tests run for this overview; the suites overlap. The receipt limits acceptance to the delivered bounded profile and records text integration as uncommitted at its reporting checkpoint. It is not a public Braid application-release announcement or acceptance of full live synchronization.

See the [source-preserving authoring roadmap](STRUCTURAL-AUTHORING-ROADMAP.md) for engine-side operation contracts and the [integration guide](INTEGRATION.md) for public APIs. Source/visual navigation and reviewed operations are the current foundation; full live two-way editing remains in development.

## Learn more

[Core quick start](../README.md#quick-start), [worked examples](xpath40-catalog/worked-examples/README.md), [architecture](ARCHITECTURE.md), [implementation status](IMPLEMENTATION_STATUS.md), [Schema integration](https://github.com/Fytala-Charles/Bosak.Schema/blob/main/docs/INTEGRATION.md) and [Fytala contact](mailto:charles.korthout@fytala.com).
