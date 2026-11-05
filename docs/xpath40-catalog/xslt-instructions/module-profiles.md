<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala module and used-package host profile reference">
  <h1>Module and used-package profile propagation</h1>
  <p>Bosak host policy - bounded executable reference</p>
</div>

**C12 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-4.md) records three contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Verified bounded direct module policy**. Direct nonconflicting examples do not establish arbitrary transitive graphs, import precedence, package versions, override/accept/expose or full lexical defaults. C06 complete static-detection mode remains unproved. Final owner acceptance remains C13; earlier checkpoints are historical.

**C06 host/profile review — 26 October 2026:** [Bounded reconciliation](../HOST-PROFILE-RECONCILIATION.md) maps this entry to current pinned instruction/host rules and named tests. Seven workpackages remain; instruction comparison, complete static-mode evidence and host/adoption limits remain explicit. No acceptance or profile promotion. Earlier notices remain historical.

**Public 1.23.8 - 23 October 2026:** The function default-mode fix and host/module profile coverage are [published and verified](../../RELEASE-1.23.8-PUBLICATION.md). Current catalog:76 entries/3,073 documentation checks. REQ-126/AC-03 remain partial; Braid smoke/acceptance is separate. Earlier source/unpublished/PR checkpoints below are historical.

## 1. Name and Classification

**Module/profile propagation** - implemented Bosak host composition, with an opt-in experimental XSLT draft profile and frozen embedded XPath 4.0. The same compilation host applies to a directly included, imported or used-package module, while expression selection and forwards-compatible validation retain the dependency's own effective version. This is not a new grammar feature or an experimental XPath promotion.

Profile implementation: [cc07d50f8a55bdca907d398d4916d95cbed36734](https://github.com/Fytala-Charles/Bosak/commit/cc07d50f8a55bdca907d398d4916d95cbed36734). This slice depends on the separate [function default-mode fix](../../XSLT-FUNCTION-DEFAULT-MODE.md), [PR416](https://github.com/Fytala-Charles/Bosak/pull/416), source 1.23.8 unpublished; public 1.23.7 is unchanged. Current comparison: [immutable XSLT XML](https://raw.githubusercontent.com/qt4cg/qtspecs/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xslt-40/src/xslt.xml), include/import/use-package/backwards/forwards/modes sections, refreshed on 23 October 2026. This is not recovered historical adoption. [Audit](../MODULE-PROFILE-BOUNDARIES.md).

## 2. Intent

Move a transformation function into a dependency without losing the selected compiler host or silently replacing the dependency's declared version with the principal stylesheet's version.

## 3. Also Known As

Include/import module loading and used-package compilation. An XSLT package name/package-version here identifies an XML library package; it is distinct from the Bosak NuGet release version. Include, import and use-package are different composition mechanisms, not interchangeable aliases.

## 4. History / Supersedes

Each relation has a baseline library at version 3.0 using explicit if/then/else and a version 4.0 library using otherwise. The principal report remains version 3.0 and calls the same quantity function. The six complete report/library pairs in [the task](../examples/module-profiles/README.md) run under both hosts. This is Bosak compatibility policy, not a promise that an arbitrary XSLT 3.0 processor can run a 4.0 library.

## 5. Motivation

A shared order-quantity function defaults a missing quantity to1 while retaining explicit zero. It can be maintained in one dependency rather than duplicated in a principal template. The examples separate library migration from the principal version and verify namespace, URI and registration setup together with the actual report.

## 6. Language Alignment

The current XSLT draft gives included/imported modules and packages their own declaration/static-context rules. Bosak propagates the selected processor host through compilation; it does not overwrite the child version. Legacy's hybrid policy selects embedded XPath by effective version, whereas Xslt40Draft supplies frozen 4.0 even in a 3.0 library. The tests use nonconflicting function names to isolate that policy. They do not establish import precedence, override/accept/expose semantics, or general equivalence between composition mechanisms.

## 7. Applicability

Use this reference for one direct include/import edge or one registered used package loaded from local files, compiled with either public host profile. Multi-hop graphs, package version ranges, overrides, schema/static parameters, dynamic evaluate, authoring, streaming, remote/resource-policy behavior and complete default-mode propagation are outside this slice.

## 8. Structure

| Dependency effective version | Legacy host | Draft host |
|---|---|---|
|3.0 | The supplied otherwise expression refuses XPST0003, regardless of principal 3.0/4.0 | Frozen4.0 expression succeeds |
|4.0 | Frozen4.0 expression succeeds, even with a 3.0 principal | Frozen4.0 expression succeeds |
| Module3.0 with local function4.0 | Local override enables the supplied 4.0 expression | Remains frozen 4.0 |
| Module4.0 with local function3.0 | Local override refuses the supplied 4.0 expression | Remains frozen 4.0 |

The selected host's forwards-compatible ceiling still applies to the child's effective version:3.0 for Legacy and4.0 for Draft. A future-version principal does not make a known-version child forwards compatible in Draft. scan remains unavailable under both hosts in the supplied 4.0 libraries.

## 9. Participants

[XsltCompiler](../../../src/Bosak.Xslt/Api/XsltCompiler.cs) captures ProcessorProfile and the URI resolver. [Stylesheet](../../../src/Bosak.Xslt/Stylesheet/Stylesheet.cs) resolves includes, imports and used packages. [XsltFunctionLibrary.RegisterPackage](../../../src/Bosak.Xslt/Api/XsltFunctionLibrary.cs) maps a package name/version to an absolute module URI. The [catalog runner](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) supplies optional per-case package registrations and always cleans them in finally; its existing package-registry collection serializes these tests with other registry users.

## 10. Collaborations

An app/main.xsl principal resolves ../library/module.xsl relative to its own file URI for include/import. A package principal uses a synthetic URN and exact package-version 1.0, registered to the corresponding absolute XML file URI before compilation. The library declares p:quantity and its own version. The principal passes each line node, preserves order and sums the returned quantities. Registry setup is caller/test setup; the XML file does not embed a compiler profile.

## 11. Consequences

Changing the principal's version alone does not promote a 3.0 dependency in Legacy. A4.0 dependency does not promote unrelated principal expressions either; this entry tests calls into the library, not every caller expression. Unknown attributes on the supplied apply-templates instruction follow the child/local version and inherited host. Those validation probes exposed an independent function default-mode bug: legal success cases raised XTSE0020 after validation. The separate fix preserves their expected results rather than recording the bug as supported behavior.

## 12. Implementation

The 114 cases are 66 focused host/version/static/validation/refusal controls and 48 complete report runs. Before package setup the baseline was70pass/44fail:38 missing-registration failures and six default-mode failures. Scoped registrations changed this to105pass/9fail, revealing the same runtime issue in packages too. On the isolated fix all 114 pass with unchanged expected results. The runner adds optional packages metadata and cleanup on both success and failure; rows without it preserve their prior setup. [Machine audit](../MODULE-PROFILE-BOUNDARIES.json).

## 13. Sample Code

For each relation, [the complete task](../examples/module-profiles/README.md) links the baseline and mixed-version principal/library files. With [normal.xml](../examples/module-profiles/normal.xml), both variants under both hosts emit `A=2|B=1|C=0|lines=3;total=3`. Empty input emits `lines=0;total=0`; reordered input changes line order; invalid integer text raises FORG0001. All namespaces, fixtures, dependency locations, package registrations and expected results are supplied. The library expression `(xs:integer($line/@qty) ?? 1)` is actually executed in the4.0 variants.

## 14. Performance Notes

These examples verify compilation policy and module loading, not a performance optimization. Each run compiles real local files and may allocate a separate package/module context. No compile-cache, concurrency or timing benefit is claimed. Reusing a compiled executable or resolving larger graphs needs separate measurements and contracts.

## 15. Known Uses

Applicable AC-03 evidence: `module-profile-include-root4-module3-legacy`, `module-profile-import-root3-module4-legacy`, `module-profile-package-root4-module3-draft40`, `module-profile-package-root4-module4-local3-legacy`, `module-profile-import-validation-root5-module4-draft40`, `module-profile-package-static-root4-module3-draft40` and `module-profile-package-report-module40-normal-legacy`, plus all 114 cases listed in [the audit](../MODULE-PROFILE-BOUNDARIES.json) and [manifest](../examples/cases.json). Named [ProcessorProfileTests](../../../tests/Bosak.Xslt.Tests/ProcessorProfileTests.cs) methods ModulesInheritProfile, ModuleValidationInheritsProfile and UsedPackageInheritsProfile provide independent API evidence. This XSLT host/module contract has no applicable XPath-only gate assignment; no gate is invented or promoted.

## 16. Related Features

[Ordinary stylesheet profiles](processor-profiles.md), [otherwise](../operators-and-syntax/otherwise-operator.md), [function default-mode correction](../../XSLT-FUNCTION-DEFAULT-MODE.md) and [coverage reconciliation](../COVERAGE-RECONCILIATION.md). General package visibility/version resolution, transitive graphs and full lexical default-mode contexts remain follow-ups. REQ-126 and D2/D3/D4/AC03 remain partial.
