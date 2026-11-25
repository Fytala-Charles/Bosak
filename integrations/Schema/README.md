## Core 1.83.0 current-source candidate - 25 November 2026

The current-source consumer asserts exact1.83.0.0 for the atomic calendar transport candidate. The public Schema source pin, its hash guards and standalone package assertions remain unchanged. Historical1.81.17/1.82.0 receipts are retained.

# Core 1.82.0 source candidate - 25 November 2026

The current-source native consumer asserts exact Core 1.82.0.0 for the additive calendar foundation candidate. The Schema source revision/hash guards and standalone package pins remain unchanged. Historical Core 1.81.17 qualification is retained separately in docs/schema-native-consumer.

# Open Schema integration example

This development harness compiles the actual **Apache-2.0 Bosak.Schema source** against the current Bosak projects. It validates a price with XSD 1.0, exposes its named schema type and decimal typed value inside an XSLT transformation, and calculates a total. No activation is required.

## Run

Requires .NET 10 and a checkout of the public schema engine beside this repository:

```sh
git clone https://github.com/Fytala-Charles/Bosak.Schema ../Bosak.Schema.Public
git -C ../Bosak.Schema.Public checkout 9fd4e6acd20b34446d736ab0ec3fa2d439f9e337
dotnet run --project integrations/Schema/Example/Bosak.Schema.Example.csproj -c Release
dotnet test integrations/Schema/Tests/Bosak.Schema.Integration.Tests.csproj -c Release
dotnet test integrations/Schema/PublicConsumer/Bosak.Schema.CoreConsumer.Tests.csproj -c Release
dotnet run --project integrations/Schema/Example/Bosak.Schema.Example.csproj -c Release -- --native
```

If a public checkout already exists, preserve any uncommitted work; use a separate worktree at the pinned revision rather than forcing a checkout. An alternate location can be supplied with `-p:SchemaSourceRoot=/absolute/path/to/Bosak.Schema`. The build verifies the [pinned source, tests, fixtures and notices](../../docs/schema-native-consumer/SOURCE-PIN.json); both LF and CRLF checkouts are supported. Use the public schema repository, not the old private licensing checkout.

Expected example output:

```xml
<result declared-type="true" atomic-decimal="true" amount="12.5" total="15"/>
```

Read [PriceExample.cs](Example/PriceExample.cs) for the complete flow: explicitly select schema-aware compilation, resolve only the bundled in-memory schema, strictly validate a copy, and use its assessed values. The original XML remains unannotated and unchanged. Invalid decimal content, a missing required attribute and unexpected child content fail with `XTTE1510`.

## Scope

The refreshed implementation suite runs 249 unchanged upstream cases plus eight price/assertion integration cases and three caller stream/reader ownership cases (260 total). A separate assembly without friend access runs nine native public-consumer cases on Core 1.81.17 (269 distinct current-Core cases across the two assemblies). Schema's original 23-case package consumer retains its exact Core 1.81.2 assertion and is not imported, weakened or counted in this lane; its successful standalone/hosted receipt remains separate. It separately exercises staged XSD 1.1 assertions and explicit diagnostics. **Assertion success does not certify structural XSD 1.1 validity**, and the staged service does not export a fully assessed document for a subsequent query. Its extra-child limitation is retained as an explicit test. This example never downgrades a 1.1 schema to 1.0.

The source wrapper produces an unpackageable development assembly; it does not update Schema's published package dependencies. Nothing adds Schema to the normal Bosak solution or makes a core project depend on it. The dedicated [CI workflow](../../.github/workflows/schema-integration.yml) checks this optional integration separately.

See the [current native consumer source pin and qualification](../../docs/schema-native-consumer/README.md). The [previous 135-case source-wrapper receipt](../../docs/schema-integration-1812/README.md) is historical and unchanged. The [original adoption record](../../docs/schema-integration/ADOPTION.md) and [historical qualification](../../docs/schema-integration/README.md) retain their original source/version identities. The added native mode compiles the finite decimal schema, completely assesses the sample, reads exact detached values and exports detached XML through NativeDecimalSession. It returns three decimals (12.5, 2, 1) and preserves 12.50 in exported XML. Explicit sample capacities and assessment date/timezone are supplied; they are not API defaults. Invalid/Unsupported input publishes no document. This native route is distinct from the legacy descriptor/assertion service and its retained extra-child limitation. Complete XSD 1.1, general query syntax, positive compatible-universe handoff, permanent provider/API acceptance, numerical workload budgets and a Schema package release remain separate work.
