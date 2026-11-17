# Native public Schema consumer qualification

17 November 2026. Core base `cfcc06a9a4f3060f73ea889fce6a9f86cc82e1c6`, Schema merged source `9fd4e6acd20b34446d736ab0ec3fa2d439f9e337`. [Pre-implementation scope](ADOPTION.json) and [exact58-input pin](SOURCE-PIN.json). The real consumer is Core's existing optional Apache Schema source-wrapper. This step closes its stale-source/native-entry qualification gap, without inventing structural demand or changing either engine's grammar.

## Behavior and dependency direction

The wrapper compiles actual pinned Schema library source against Core1.81.17 projects and remains nonpackable/outside the normal Bosak solution. No Core source project depends on Schema. Its existing260-case test assembly contains249 upstream implementation cases unchanged plus11 original price/assertion/caller-ownership cases. NativeDecimalExample uses only public session/options/context/document/results; the default CLI retains the original price output and `--native` selects the explicit finite decimal sample.

The separate Bosak.Schema.CoreConsumer.Tests assembly has no InternalsVisibleTo grant and runs nine actual public controls: exact scalar/lexical export, Invalid/Unsupported/malformed/DTD refusal, document/session disposal, quota reuse, detached values, cancellation and numerical refusal, unsupported schema, and explicit currentCore1.81.17 identity. The upstream standalone public-consumer source asserts Core1.81.2 exactly; that test is not rewritten, suppressed or relabeled as current-Core evidence. Its23-case qualification and Schema's272-case standalone main receipt remain separate historical package-backed lanes.

## Executed qualification

| Lane | Result | Scope |
|---|---|---|
| Refreshed upstream/legacy integration | 260 passed, zero failed/skipped | 249 unchanged Schema implementation +11 original integration/ownership identities |
| Separate public native consumer | Nine passed, zero failed/skipped | Public surface only, explicit current Core identity |
| LF/CRLF source forms | Same269 identities pass per form | Repeated case catalogs, not additive coverage |
| Guard refusals | Three handled build refusals | Altered library source, fixture and NOTICE rejected before compilation |
| Default typed-price CLI | Exact original output | Named type/decimal/arithmetic and original caller ownership unchanged |
| Native CLI | Three exact decimals and detached XML | 12.5/2/1 values; exported lexical12.50 retained |
| Normal Core solution | Final count and warnings in RESULTS.json | No engine source changes; exact submodule fixtures initialized |
| Hosted check | Not run | Local candidate not pushed |

Original docs/schema-integration and docs/schema-integration-1812 dossiers are hash-preserved. Current source props, workflow pin and contributor commands point here; prior execution receipts retain their original identities. [Evidence/index](RESULTS.json), [offline verifier](verify.py).

## Run

Use the [contributor commands](../../integrations/Schema/README.md), passing `-p:SchemaSourceRoot=<clean checkout at9fd4e6a>` if the normal sibling checkout is older/dirty. Both test projects verify the same exact source/test/fixture/legal inventory. Run `python docs/schema-native-consumer/verify.py` from the Core root for offline source/receipt integrity.

Schema's package references stay1.81.2 and its public native API belongs to its0.5 development line. This is a current-Core source-consumer qualification, not a published Schema0.5 package or unchanged-binary adoption. It neither broadens the native grammar nor upgrades Braid/XmlDb. The legacy staged service's extra-child limitation remains explicitly tested. Full XSD1.1, component-universe interchange, broader concurrent disposal, actual performance budgets, C45/API freeze, new native human/product acceptance, tags and releases remain separate gates. No commit/push/publication is included in this local step.
