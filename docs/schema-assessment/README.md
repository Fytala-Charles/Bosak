# Native Schema assessment candidate — 29 October 2026

The first genuine native XSD 1.1 assessment profile is implemented in the public Schema feature worktree, separately from the existing assertion-only descriptor service. The result owns a snapshot and original declaration/type/value state. It checks complete admitted structure and exact decimal values **before** assertions; invalid or unsupported requests expose no assessed document.

## Run and review

Feature branch: `feature/bounded-document-assessment`, public Schema base `044d2cb2e67decd46e03e0f5ee7cb03029f34bd9`. Local implementation: `D:/Development/Bosak/.worktrees/Bosak.Schema.Assessment`. The original public checkout and private Licensing repository are untouched. The candidate is on the 0.5 MinVer development line, with existing published Bosak 1.26.3 pins unchanged.

From the feature worktree:

```sh
dotnet run --project examples/Bosak.Schema.Assessment.Example -c Release
dotnet test Bosak.Schema.slnx -c Release
```

The example reports valid assessment, decimal typing, total 15 and successful annotation-preserve/strip checks. A standalone schema-authored `xs:assert` is evaluated after native structural validation; no 1.1 schema is stripped or routed through `XmlSchemaSet`.

See the [API/profile guide snapshot](API-GUIDE.md), [normative adoption snapshot](ADOPTION.md), [results](RESULTS.json) and candidate patch/source identities in `evidence`. The owning guide and source remain in the Schema worktree; the snapshots here are review evidence, not a second implementation.

## Delivered boundary

| Request | Result |
|---|---|
| One no-namespace root with an anonymous element-only type, an ordered single-occurrence decimal sequence and bounded assertions | Native structural, lexical, exact-value and assertion assessment. |
| Invalid child names/order/count, attributes, nested decimal content or lexical values | Invalid; no assessed document. |
| Unimplemented schema/instance/expression features or valid decimals needing rounding/overflow/underflow | Unsupported; no partial success or 1.0 fallback. |
| Native assessment copy | Original component/PSVI state and assessment identity retained; independent snapshot identity. |
| Explicit stripped XML copy | Caller-owned XML with no type annotations. |
| Decimal child projection | Actual built-in decimal annotation usable by real XPath and XSLT; local-declaration authority is not projected. |
| Fully typed anonymous-root document export | Explicitly unsupported until a qualified engine bridge exists. |

The legacy XSLT copy check uses a document input and an explicit empty schema set as its annotation-copy context. That empty set never receives the XSD 1.1 schema. The initial detached-context and missing-annotation-context fixture failures are retained separately from the final passing fixture.

## Qualification and next step

- **191 Schema cases pass** against published core 1.26.3: the original 114 plus 77 new cases.
- The same **191 cases pass against current core 1.33.1 source** in an isolated source wrapper.
- **114 unchanged legacy test binaries pass** with only the Schema library replaced.
- Exported API grows from **32 to 39 types**, with no removed existing public members.

These scopes overlap. Complete core solution/conformance qualification is inherited from the source-identical earlier checkpoint; no core production file, public interface, package version, gate/filter, consumer pin or prior sealed dossier is changed. No new hosted CI or full XSD 1.1 certification is inferred.

The next step is the smallest whole-document/anonymous-root bridge that retains actual components, no-typed-value errors, copy semantics and ownership. The earlier general 32-type session proposal is not adopted by implication. Broader structural coverage, independent compatible universes, product adoption and API freeze remain separate.

Implementation changes remain uncommitted in the feature worktree under its explicit Git rule. No tag, push or package publication is included.
