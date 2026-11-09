# Refreshed optional Schema integration

9 November 2026. The Core development wrapper and its optional CI now pin merged public Schema **c953ebe0057bea1616d3e4f891f7b0a5fc4bd99b**, replacing044d2cb2. The [current manifest](SOURCE-PIN.json) and `Schema.Source.Files.props` bind20 library sources,13 upstream test sources, two fixtures and Apache LICENSE/NOTICE. Exact LF and CRLF forms remain accepted; other content remains refused. The original [integration dossier](../schema-integration/README.md) and every file beneath it remain byte-for-byte unchanged.

## Change and qualification

- The optional workflow checkout, contributor instructions, source-error pointer and MSBuild hash inventory agree on the new pin.
- The upstream test inventory grows from114 to124 cases, including its existing wildcard negative-constraint representation tests. No new Core engine semantics are introduced.
- Three previously qualified stream/reader ownership controls are adopted into the optional test project, alongside eight existing price/staged-assertion cases.
- **135/0/0** passes for both LF and CRLF copies with identical catalogs. These are135 distinct cases, not270.
- The typed-price example returns `<result declared-type="true" atomic-decimal="true" amount="12.5" total="15"/>`.
- An intentionally altered disposable source copy is refused by the existing MSBuild pin guard before compilation. This is a handled build refusal, not an unhandled negative .NET child probe.

[Results](RESULTS.json), [case catalog](TEST-CATALOG.json), [raw logs/TRX](EXECUTION-RECEIPTS.zip), [immutable adoption boundary](ADOPTION.json), [historical preservation hashes](HISTORICAL-PRESERVATION.json).

## Scope

The wrapper still compiles actual public Schema source against Core projects and remains nonpackable, outside the normal Bosak solution. It does not republish Schema, add a Core dependency on it, replace the standalone package-backed adoption qualification or advance native assessor/advisor/session/lifetime integration. The extra-child limitation remains an explicit regression: assertion success is not full XSD1.1 structural validation. No performance guarantee or new numerical budget is claimed.

No original Schema checkout, retained9d1881e worktree or separate native assessment worktree was modified. The tests use a new clean c953ebe worktree under `.worktrees/` and disposable line-ending/mutation copies under `.guard-tmp/`.

## Run and review

Use the [integration guide](../../integrations/Schema/README.md) and pass `-p:SchemaSourceRoot=<clean checkout at c953ebe>` when the normal sibling checkout has uncommitted work. Run `python docs/schema-integration-1812/tools/verify.py` from the Core root to check the current pin/props/workflow closure and preserved historical dossier. Local execution is not a hosted CI result; PR validation is recorded separately by GitHub.
