<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak engine, Schema and Braid ecosystem">
  <br><br><h1>The Bosak ecosystem</h1>
  <p>One XML engine, optional schema awareness and a visual-authoring application</p>
</div>

Bosak brings XPath, XSLT and XQuery into .NET applications. Bosak.Schema extends the engine with licensed complex-type schema awareness. Bosak.Braid uses the engine's source-preserving APIs to build a visual/source XSLT authoring workflow.

| Component | Role | Availability and next step |
|---|---|---|
| Bosak | Apache-2.0 XPath 3.1 / XSLT 3.0 / XQuery 3.1 engine; documented opt-in 4.0 draft features | Public .NET 10 packages. Start with the [README](../README.md#quick-start). |
| Bosak.Schema | Optional commercial XSD complex-type schema-awareness add-on | Public [integration/licensing guide](https://fytala-charles.github.io/Bosak.Docs/) and [trial request](https://licensing.fytala.com/api/trial); contact Fytala for package access and compatible engine versions. |
| Bosak.Braid | Separate visual/source XSLT application, with reviewed authoring and execution workflows | Active development in a private repository. [Contact Fytala](mailto:charles.korthout@fytala.com) for access and delivery information. |
| VS Code extension | Editing assistance and evaluate/run commands | Public [Marketplace extension](https://marketplace.visualstudio.com/items?itemName=fytala.vscode-bosak); its bundled engine follows a separate release cycle. |

## Why schema awareness helps

A schema can describe more than the presence of a field: complex types express attributes, child structure and content relationships. Using those types in a transformation helps detect invalid document structure at a validation boundary and makes type expectations explicit in the transformation rules.

The Schema add-on's documented implemented profile includes `xsl:import-schema`, named complex-type checks through `instance of` / `treat as`, and schema-validated construction through supported validation/type attributes. It uses the core's extension interfaces rather than a separate engine fork. The free core retains its own simple-type support.

The add-on is separately licensed and activated. Its documented schema profile, package distribution and compatibility lifecycle are independent of the core's version number; verify the supported combination before adoption. The customer guide explains activation and licensing. No Schema version is implied by a Bosak core release.

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

[Core quick start](../README.md#quick-start), [worked examples](xpath40-catalog/worked-examples/README.md), [architecture](ARCHITECTURE.md), [Schema customer guide](https://fytala-charles.github.io/Bosak.Docs/) and [Fytala contact](mailto:charles.korthout@fytala.com).
