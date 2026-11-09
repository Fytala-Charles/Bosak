<div align="center">
  <img src="assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak XPath, XSLT and XQuery for .NET">
  <br><br><h1>Bosak</h1>
  <p>Query XML. Transform documents. Build visual XSLT workflows.</p>
</div>

> Named after **Jon Bosak**, the "father of XML" who led the W3C working group that created
> XML 1.0 — making structured business documents interoperable across every system.
> Full story: [Fytala Hall of Fame](../Fytala/docs/HALL_OF_FAME.md#jon-bosak--bosak)

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

An opt-in [typed identity integration profile](docs/foundation-public-typed-identity/GUIDE.md) binds a supported assessor to a query session and preserves an issued scalar through `$value`. An opt-in
[typed numeric profile](docs/foundation-public-typed-numeric/GUIDE.md) computes bounded exact arithmetic and
comparisons from authenticated scalar operands. Broader schema operations remain staged.

The published packages also provide [captured XQuery sessions](docs/foundation-public-xquery/GUIDE.md) and [captured XSLT transforms](docs/foundation-public-xslt/GUIDE.md) with explicit source and operation lifetimes. Opt-in [atomic provenance](docs/foundation-public-provenance/GUIDE.md), [compatible provenance](docs/foundation-leased-provenance/GUIDE.md) and [typed-container reads](docs/foundation-public-primitives/GUIDE.md) retain their documented supported profiles and limits.

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
dotnet add package Bosak.XPath.Api --version 1.81.2
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
| [Bosak.XPath.Api](https://www.nuget.org/packages/Bosak.XPath.Api/1.81.2) | Compile and evaluate XPath. |
| [Bosak.Xslt](https://www.nuget.org/packages/Bosak.Xslt/1.81.2) | Compile XSLT and transform documents. |
| [Bosak.XQuery](https://www.nuget.org/packages/Bosak.XQuery/1.81.2) | Compile and evaluate XQuery. |

Version 1.81.2 is the [verified published baseline](docs/release-1.81.2-publication/README.md), accompanied by VS Code extension 0.2.6. Numerical workload budgets are deferred for this release; no performance guarantees are claimed. Source candidates and technical qualification are tracked separately in [implementation status](docs/IMPLEMENTATION_STATUS.md).

<a id="bosak-ecosystem"></a>

## Ecosystem and licensing

| Component | Role | License |
|---|---|---|
| Bosak | XPath, XSLT and XQuery engine | [Apache-2.0](license.md) |
| [Bosak.Schema](https://github.com/Fytala-Charles/Bosak.Schema) | Optional schema integration and staged XSD 1.1 services | Apache-2.0; no activation |
| Bosak.XmlDb | Native XML storage and persistent XDM navigation in development | Proprietary; separate product terms |
| Bosak.Braid | Visual/source XSLT authoring application in development | Commercial; separate product terms |
| Bosak.Folio | XSL-FO-to-PDF formatter in development | Proprietary; separate product terms |

Schema's public source has its own version and compatibility lifecycle; the Apache transition did not publish a new Schema NuGet package. See its [integration guide](https://github.com/Fytala-Charles/Bosak.Schema/blob/main/docs/INTEGRATION.md) and the [ecosystem overview](docs/ECOSYSTEM.md).

Bosak.XmlDb is a separate storage product in development, starting with embedded use. It is distinct from the published [Bosak.XPath.Providers.Database](https://www.nuget.org/packages/Bosak.XPath.Providers.Database/1.81.2) REST-loader package; the native database is not included in the Core packages.

Try the [schema-aware price example](integrations/Schema/README.md) to validate XML and transform its typed values using the public Schema source.

## Work in VS Code

Install [Bosak XPath / XSLT](https://marketplace.visualstudio.com/items?itemName=fytala.vscode-bosak) for diagnostics, completion, navigation and evaluate/run commands. See the [extension guide](vscode-bosak/README.md) for its bundled-engine release cycle.

## Documentation and development

| Guide | Contents |
|---|---|
| [Integration](docs/INTEGRATION.md) | Public APIs and supported usage profiles. |
| [Ordinary-source sessions](docs/foundation-public-ordinary/GUIDE.md) | Opt-in string/boolean queries with explicit source ownership and lifetime. |
| [Exact decimals](docs/foundation-exact-decimal/GUIDE.md) | Exact coefficient/scale values with explicit capacity and output bounds. |
| [Exact XDM numeric carriers](docs/foundation-exact-value-transport/GUIDE.md) | Full numeric access with checked legacy views and source migration guidance. |
| [Bounded exact arithmetic](docs/foundation-bounded-numeric/GUIDE.md) | Opt-in integer/decimal arithmetic with explicit coefficient and scale limits. |
| [Exact comparisons, keys and strings](docs/foundation-exact-scalars/GUIDE.md) | Exact numeric comparisons and map keys with bounded scalar output. |
| [Exact quotient and remainder](docs/foundation-exact-quotient/GUIDE.md) | Bounded integer division and remainder for primitive integer/decimal values. |
| [Exact terminating division](docs/foundation-exact-division/GUIDE.md) | Decimal division with explicit quotas and refusal instead of rounding. |
| [Primitive numeric constructors](docs/foundation-exact-constructors/GUIDE.md) | Bounded integer/decimal conversion over the exact numeric grammar. |
| [Architecture](docs/ARCHITECTURE.md) | Layers, execution pipeline and extension points. |
| [Roadmap](ROADMAP.md) | Current direction and upcoming work. |
| [Provider qualification](docs/foundation-provider-qualification/GUIDE.md) | Candidate-bound owner workload receipts and evidence checks. |
| [Native Schema compatibility](docs/foundation-native-schema-lane/GUIDE.md) | Source and unchanged-binary qualification scope. |
| [Implementation status](docs/IMPLEMENTATION_STATUS.md) | Source candidates, qualification and open technical boundaries. |
| [Foundation integration readiness](docs/foundation-readiness/README.md) | Qualified candidate identities, integration evidence and pending consumer acceptance. |
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

Maintainers can find artifact preparation and publication instructions in the [release guide](docs/RELEASING.md).
