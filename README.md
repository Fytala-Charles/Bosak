<div align="center">
  <img src="assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak XPath, XSLT and XQuery for .NET">
  <br><br><h1>Bosak</h1>
  <p>Query XML. Transform documents. Build visual XSLT workflows.</p>
</div>

---

<details>
<summary>About FYTALA: the people and purpose behind Bosak</summary>

## About FYTALA

**FYTALA — Feeling Young, Thriving, Active, Learning Always** — is a personal initiative founded after retirement, driven by the belief that curiosity, enthusiasm, and learning have no age limit.

It is about staying engaged, exploring new ideas, and sharing the excitement of technology and innovation with others. A special ambition of FYTALA is to spark that enthusiasm in young people and encourage them to discover how fascinating technology can be—not just by talking about technology, but by making it visible, tangible, surprising, and fun.

My dream captures that ambition perfectly: **to walk into a classroom one day, side by side with a humanoid robot, and make my enthusiasm for technology and innovation contagious.**

If that experience inspires even a few young minds to start asking questions, experimenting, building, programming, or imagining what might be possible, FYTALA has achieved something worthwhile.

Technology, after all, is not just about machines, electronics, or software. It is about curiosity, creativity, and turning ideas into reality. FYTALA therefore takes a deliberately broad perspective, embracing software, electronics, engineering, science, artificial intelligence, robotics, and whatever comes next.

> Experimenting matters.
>
> Making mistakes matters.
>
> Understanding *why* something works matters even more.

Every project is an opportunity to learn something new and, hopefully, to help someone else learn as well.

The cable-stayed bridge in the FYTALA logo represents that philosophy. A bridge connects places, but it can also connect people, ideas, generations, and fields of knowledge. Its strength comes from many individual elements working together—much like technology itself.

FYTALA wants to help build those bridges: between experience and youthful curiosity, theory and practice, and imagination and real-world creation. It encourages looking beyond the obvious, asking questions, taking things apart, and building them again in new ways.

Above all, FYTALA is about keeping the desire to discover alive—and passing that desire on to the next generation.

> Because we never have to stop being curious.
>
> We never have to stop creating.
>
> And we are never too old—or too young—to learn something new.

</details>

---

**XPath 3.1, XSLT 3.0 and XQuery 3.1 for .NET 10**, with a shared data model and APIs for embedding queries and transformations in your applications.

Bosak and [Bosak.Schema](https://github.com/Fytala-Charles/Bosak.Schema) are **Apache-2.0**. Schema compilation and validation require no activation or subscription. Schema's XSD 1.1 implementation is staged; complete structural validation and full engine integration remain in development.

[Quick start](#quick-start) | [Integration](docs/INTEGRATION.md) | [Worked examples](docs/xpath40-catalog/worked-examples/README.md) | [Roadmap](ROADMAP.md)

## Why Bosak

- Query XML, transform business documents, and produce XML, HTML, JSON or text.
- Compile expressions and stylesheets once, then reuse them with different inputs.
- Use explicit resource policies and supported source-preserving authoring APIs.
- Explore documented 4.0 draft features through opt-in language profiles.

Try the [invoice-to-ERP](docs/xpath40-catalog/worked-examples/invoice-to-erp/README.md), [manual publishing](docs/xpath40-catalog/worked-examples/manual-to-html/README.md) or [order reconciliation](docs/xpath40-catalog/worked-examples/order-reconciliation/README.md) walkthroughs.

## Quick start

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10), then create a console application:

```bash
dotnet new console -n BosakDemo
cd BosakDemo
dotnet add package Bosak.XPath.Api --version 1.26.3
```

Replace `Program.cs` with:

```csharp
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;

var expression = XPathExpression.Compile("sum((10, 20, 30))");
var result = expression.Evaluate(new EvaluationContext());
Console.WriteLine(result.IntegerValue); // 60
```

Run `dotnet run`. For other languages, use the matching package:

| Package | Purpose |
|---|---|
| [Bosak.XPath.Api](https://www.nuget.org/packages/Bosak.XPath.Api/1.26.3) | Compile and evaluate XPath. |
| [Bosak.Xslt](https://www.nuget.org/packages/Bosak.Xslt/1.26.3) | Compile XSLT and transform documents. |
| [Bosak.XQuery](https://www.nuget.org/packages/Bosak.XQuery/1.26.3) | Compile and evaluate XQuery. |

Version 1.26.3 is the [verified published baseline](docs/RELEASE-1.26.3-PUBLICATION.md). Source candidates and technical qualification are tracked separately in [implementation status](docs/IMPLEMENTATION_STATUS.md).

<a id="bosak-ecosystem"></a>

## Ecosystem and licensing

| Component | Role | License |
|---|---|---|
| Bosak | XPath, XSLT and XQuery engine | [Apache-2.0](license.md) |
| [Bosak.Schema](https://github.com/Fytala-Charles/Bosak.Schema) | Optional schema integration and staged XSD 1.1 services | Apache-2.0; no activation |
| Bosak.Braid | Visual/source XSLT authoring application in development | Commercial; separate product terms |
| Bosak.Folio | XSL-FO-to-PDF formatter in development | Proprietary; separate product terms |

Schema's public source has its own version and compatibility lifecycle; the Apache transition did not publish a new Schema NuGet package. See its [integration guide](https://github.com/Fytala-Charles/Bosak.Schema/blob/main/docs/INTEGRATION.md) and the [ecosystem overview](docs/ECOSYSTEM.md).

Try the [schema-aware price example](integrations/Schema/README.md) to validate XML and transform its typed values using the public Schema source.

An unpublished [native XSD 1.1 assessment candidate](docs/schema-assessment/README.md) adds a bounded structurally checked document result and typed decimal exports.

## Work in VS Code

Install [Bosak XPath / XSLT](https://marketplace.visualstudio.com/items?itemName=fytala.vscode-bosak) for diagnostics, completion, navigation and evaluate/run commands. See the [extension guide](vscode-bosak/README.md) for its bundled-engine release cycle.

## Documentation and development

| Guide | Contents |
|---|---|
| [Integration](docs/INTEGRATION.md) | Public APIs and supported usage profiles. |
| [Architecture](docs/ARCHITECTURE.md) | Layers, execution pipeline and extension points. |
| [Roadmap](ROADMAP.md) | Current direction and upcoming work. |
| [Implementation status](docs/IMPLEMENTATION_STATUS.md) | Source candidates, qualification and open technical boundaries. |
| [Foundation API review](docs/foundation-api-baseline/README.md) | Proposed shared API scope and compatibility questions for Schema and XmlDb. |
| [Foundation contract draft](docs/foundation-api-contract-draft/README.md) | Proposed member scope and planned consumer compatibility cases. |
| [Result ownership review](docs/foundation-result-ownership/README.md) | Lazy and escaped result lifetimes, ownership proposal and observations. |
| [Development history](docs/DEVELOPMENT_HISTORY.md) | Archived checkpoints and earlier roadmap snapshots. |

Build and test a source checkout:

```bash
git submodule update --init --recursive
dotnet build Bosak.sln
dotnet test Bosak.sln
```

See [AGENTS.md](AGENTS.md) for conformance runners and the documented Windows XSLT test workaround.

## Support and contribute

Use [Discussions](https://github.com/Fytala-Charles/Bosak/discussions) for questions and [Issues](https://github.com/Fytala-Charles/Bosak/issues) for bugs. [Commercial support](COMMERCIAL.md) and [GitHub Sponsors](https://github.com/sponsors/Fytala-Charles) support development; schema access and conformance fixes are not paywalled.

The unpublished [native whole-document assessment candidate](docs/schema-document-bridge/README.md) documents the optional Schema/provider integration and local source-build commands.

The [bounded bridge API review checkpoint](docs/schema-document-bridge/api-review/README.md) records current local qualification and the next structural-coverage boundary.

The unpublished [optional decimal assessment candidate](docs/schema-optional-implementation/README.md) extends the finite Schema profile while preserving original assessed state through engine copies.

The [optional assessment review checkpoint](docs/schema-optional-review/README.md) records the bounded implementation review and paired delivery identity.

The unpublished [bounded repeated-decimal candidate](docs/schema-repeated-implementation/README.md) supports finite repetitions with original occurrence state and explicit cardinality/projection behavior.

The [bounded repetition review checkpoint](docs/schema-repetition-review/README.md) records current implementation review and paired delivery identity.

The unpublished [count assertion candidate](docs/schema-count-implementation/README.md) adds finite cardinality assertions while preserving native validation and existing child-name semantics.

[Bounded Schema assessment review and next scope](docs/schema-count-review/README.md).

The [bounded presence assertion candidate](docs/schema-presence-implementation/README.md) adds exists/empty predicates while preserving count and typed-copy behavior.

[Presence assertion implementation review](docs/schema-presence-review/README.md).

[Consumer compatibility and native Schema priorities](docs/schema-consumer-priorities/README.md).

[Isolated persistent-provider compatibility qualification](docs/xmldb-compatibility/README.md).

[Isolated authoring/preview source compatibility qualification](docs/braid-compatibility/README.md).

[Consumer qualification review](docs/consumer-qualification-review/README.md).

[Unchanged-binary compatibility qualification plan](docs/binary-compatibility-scope/README.md).

[Unchanged-binary qualification and runtime identity limits](docs/binary-compatibility-execution/README.md).

[Unchanged-binary qualification review](docs/binary-compatibility-review/README.md).

[Consumer engine identity and reviewed adoption policy](docs/braid-engine-identity-policy/README.md).

[Isolated consumer identity-policy prototype](docs/braid-identity-prototype/README.md).

[Identity-policy prototype review](docs/braid-identity-review/README.md).

[Isolated actual-runtime identity admission experiment](docs/braid-runtime-admission/README.md).

[Actual-runtime admission experiment review](docs/braid-runtime-review/README.md).

[Consumer engine-session integration boundary design](docs/braid-integration-design/README.md).

[Real consumer inspector typed-boundary proof](docs/braid-typed-inspector/README.md).

[Typed-inspector proof and integration review](docs/braid-typed-inspector-review/README.md).

[Isolated guarded authoring-port facade proof](docs/braid-guarded-ports/README.md).

[Guarded authoring-port facade review](docs/braid-guarded-ports-review/README.md).

[Consumer facade and lifecycle contract scope](docs/braid-facade-lifecycle-contract/README.md).

[Deterministic consumer facade lifecycle proof](docs/braid-lifecycle-proof/README.md).

[Facade lifecycle contract and proof review](docs/braid-lifecycle-review/README.md).

[Finite consumer read-only session adoption proposal](docs/braid-readonly-adoption/README.md).

[Read-only consumer adoption proposal review](docs/braid-readonly-adoption-review/README.md).
