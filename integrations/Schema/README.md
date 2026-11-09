# Open Schema integration example

This development harness compiles the actual **Apache-2.0 Bosak.Schema source** against the current Bosak projects. It validates a price with XSD 1.0, exposes its named schema type and decimal typed value inside an XSLT transformation, and calculates a total. No activation is required.

## Run

Requires .NET 10 and a checkout of the public schema engine beside this repository:

```sh
git clone https://github.com/Fytala-Charles/Bosak.Schema ../Bosak.Schema.Public
git -C ../Bosak.Schema.Public checkout c953ebe0057bea1616d3e4f891f7b0a5fc4bd99b
dotnet run --project integrations/Schema/Example/Bosak.Schema.Example.csproj -c Release
dotnet test integrations/Schema/Tests/Bosak.Schema.Integration.Tests.csproj -c Release
```

If a public checkout already exists, preserve any uncommitted work; use a separate worktree at the pinned revision rather than forcing a checkout. An alternate location can be supplied with `-p:SchemaSourceRoot=/absolute/path/to/Bosak.Schema`. The build verifies the [pinned source, tests, fixtures and notices](../../docs/schema-integration/SOURCE-PIN.json); both LF and CRLF checkouts are supported. Use the public schema repository, not the old private licensing checkout.

Expected example output:

```xml
<result declared-type="true" atomic-decimal="true" amount="12.5" total="15"/>
```

Read [PriceExample.cs](Example/PriceExample.cs) for the complete flow: explicitly select schema-aware compilation, resolve only the bundled in-memory schema, strictly validate a copy, and use its assessed values. The original XML remains unannotated and unchanged. Invalid decimal content, a missing required attribute and unexpected child content fail with `XTTE1510`.

## Scope

The suite runs the public engine's 124 tests plus eight price/assertion integration cases and three caller stream/reader ownership cases (135 total). It separately exercises staged XSD 1.1 assertions and explicit diagnostics. **Assertion success does not certify structural XSD 1.1 validity**, and the staged service does not export a fully assessed document for a subsequent query. Its extra-child limitation is retained as an explicit test. This example never downgrades a 1.1 schema to 1.0.

The source wrapper produces an unpackageable development assembly; it does not update Schema's published package dependencies. Nothing adds Schema to the normal Bosak solution or makes a core project depend on it. The dedicated [CI workflow](../../.github/workflows/schema-integration.yml) checks this optional integration separately.

See the [current source pin and qualification](../../docs/schema-integration-1812/README.md). The [original adoption record](../../docs/schema-integration/ADOPTION.md) and [historical qualification](../../docs/schema-integration/README.md) retain their original source/version identities. Native provider/session adoption, complete XSD 1.1 assessment, compatible component-universe handoff and a Schema package release remain separate work.
