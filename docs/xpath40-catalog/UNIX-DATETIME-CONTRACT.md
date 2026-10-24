<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Unix timestamp boundary contract review">
  <h1>Unix timestamp boundary contract review</h1>
  <p>24 October 2026 — source/public 1.25.0; test and evidence slice</p>
</div>

**Experimental signed Unix epochs — 24 October 2026:** [Implementation and limits](UNIX-DATETIME-EXPERIMENTAL.md) record the opt-in correction, 27 new source tests, 11,407 passing tests and unchanged frozen gates. Source 1.25.1 is unpublished; public/Braid 1.25.0 remains current. REQ-126 stays partial.



This slice adds **52 source regression cases**, three complete deadline reports with XPath 3.1 comparisons, and a fresh frozen/current native comparison for `fn:unix-dateTime`. Engine/API, version, frozen gitlink, gate membership and retry policy are unchanged. Braid's signed 1.25.0 smoke/integration proceeds independently. REQ-126/D2/D3/D4/AC-03 remain partial.

## Contract and observed native boundary

The refreshed primary pins are qtspecs [`b4723ff5`](https://github.com/qt4cg/qtspecs/blob/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xpath-functions-40/src/function-catalog.xml) and qt4tests [`3a2640cf`](https://github.com/qt4cg/qt4tests/blob/3a2640cf649a9bff6c7a5b257a1fe7e50450b2c2/fn/unix-dateTime.xml). They remain the latest upstream defaults at this review. Downloaded blobs are decoded from GitHub's base64 Git-blob API and checked by Git object ID, byte count and SHA-256; native checkout CRLF conversion is recorded separately. Review pins do not recover any unavailable historical adopted specification snapshot.

Current F&O takes optional `xs:integer`, defaults omission/empty input to zero, interprets milliseconds in UTC and permits negative epochs. [PR2885](https://github.com/qt4cg/qtspecs/pull/2885) merged on 20 October 2026; its [discussion in the September meeting](https://qt4cg.org/meeting/minutes/2026/09-15) predates that merge. The broader restricted-integer change also underlies the prior [CSV contract review](CSV-INDEX-CONTRACT.md). Bosak's frozen behavior still uses a nonnegative domain; selecting Experimental for the substring correction does not adopt every newer function contract.

| Complete native set | Passed | Failed | Skipped | Exit |
|---|---:|---:|---:|---:|
| Frozen `68910080` | 16 | 0 | 0 | 0 |
| Current `3a2640cf` | 20 | 4 | 0 | 2 |

All 16 frozen execution expressions/assertions remain unchanged in current; eight cases were added. This comparison does not infer equivalence of every metadata or inherited-context field. No fixture/assertion is rewritten and no individual failure is excluded. The function is already gated, so the current four failures produce exit2. [Frozen receipt](evidence/unix-datetime-contract/frozen-native.txt), [current receipt](evidence/unix-datetime-contract/current-native.txt), [machine review and all native cases](UNIX-DATETIME-CONTRACT.json).

| Current case | Required current outcome | Bosak 1.25.0 observed outcome |
|---|---|---|
| `unix-DateTime-114` /`115` | Exact very large date, FODT0001, or FOAR0002 for `9223372036854775808` /`5809` milliseconds | FORG0001 while converting the out-of-Int64 representation to nonNegativeInteger |
| `unix-DateTime-120` | `-86400000` gives 1969-12-31 UTC | FOCA0002 under the retained nonnegative contract |
| `unix-DateTime-121` | `-1` gives 1969-12-31T23:59:59.999Z | FOCA0002 under the retained nonnegative contract |

The two large-input tests originate in numeric-limit commit [`16645335`](https://github.com/qt4cg/qt4tests/commit/166453359bd7770fb113b6d9c9015df7f7fa5d02). The negative cases originate in [`1d149118`](https://github.com/qt4cg/qt4tests/commit/1d149118d85df69aa7bee1b2b3ee67cdb3624533). These are separate boundaries: signed-domain adoption does not resolve integer representation/error classification, and changing an error expectation would not resolve negative-epoch semantics.

## Added regression coverage

[`UnixDateTimeContractTests`](../../tests/Bosak.XPath.Standard.Tests/UnixDateTimeContractTests.cs) exercises both `XPath40` and `XPath40Experimental` without changing either profile:

- Eleven exact nonnegative values per profile: epoch, millisecond/second/day transitions, 32-bit millisecond boundaries, the 2038 second transition expressed in milliseconds, and the last supported millisecond of year9999.
- Eight omitted/empty paths per profile: direct calls, arity-zero/one references, dynamic lookup, partial application and variables. All return epoch zero, never current time.
- Negative-domain refusal through direct/indirect calls; first out-of-calendar positive value and Int64.MaxValue yield FODT0001; UTC stamp subtype and timezone remain stable across four implicit-timezone settings.
- Three complete supplied-input reports cross a 2038 millisecond boundary, with early/on-time/late completion. Exact expected reports are checked in XPath 3.1, frozen 4.0 and Experimental. Older profiles retain static refusal and absent dynamic lookup for both arities.

The same data-driven test can contain multiple assertions; 52 is the executed source test count, not the number of assertions or native tests. These cases are outside `examples/cases.json`: catalog counts stay **76 entries /3,148 feature +24 chapter =3,172 documentation checks**. This is a strengthened existing timestamp entry, not a new catalog family or worked chapter. The complete expressions and exact reports are in the named tests; no input file, wall clock or ambient resource is needed.

## Additional diagnostic: lexical minimum integer

The [22 observed API diagnostics](UNIX-DATETIME-CONTRACT.json) are separate from passing normative assertions. In both profiles:

```xpath
(-9223372036854775808) instance of xs:integer       (: observed false :)
(-9223372036854775807 - 1) instance of xs:integer   (: observed true :)
```

The direct minimum-value literal reaches FORG0001 in `unix-dateTime`; computing the same value with integer arithmetic, or constructing it with `xs:integer` from a string, reaches the existing FOCA0002 domain refusal. This identifies a lexical/representation boundary before the function's final calendar calculation. It does not establish a permissible standard error or a complete diagnosis of arbitrary-precision integer support.

The first new test run retained five failures: three came from using nonportable chained-let syntax in the 3.1 baseline; two came from expecting the computed-integer refusal for the direct minimum literal. The baseline now uses nested `let ... return`, and the passing negative-domain control constructs Int64.MinValue by integer arithmetic. The literal observation remains explicit evidence rather than a skipped, hidden or renamed native pass. No engine correction or broad integer policy is smuggled into this test slice. Original failed TRX/logs and native receipts remain retained.

## Supported limits and remaining decision

The tested nonnegative epoch path covers 1970 through `9999-12-31T23:59:59.999Z`; the next millisecond raises FODT0001. `XPathDateTime` can construct some extended-year values, but that does not expand this function's DateTimeOffset-based conversion. Milliseconds are not seconds; UTC does not depend on the implicit timezone; Unix time does not model leap seconds. The existing [date/time construction review](DATE-TIME-CONTRACT-REVIEW.md) separately records permissive type/cardinality conversion and keyword gaps. This slice does not certify those conversions or every extended-year operation.

Next decide whether newer negative epochs should be an explicit **Experimental-only opt-in**, preserving frozen negative refusal, or part of a separately approved baseline migration. Do not infer this decision from the earlier substring approval. Review large-integer error classification and the minimum-literal representation independently before promising wider ranges. No upstream fixture change, issue or contribution is needed to explain an already documented upstream contract revision.

Full local verification and TRX/log hashes are recorded in the machine review. Full QT3/qt4 gates are not rerun for this tests/docs-only slice; the source/public1.25.0 receipts remain QT3 31,142/0/679 and frozen qt4 8,434/0/1,896 across224 sets. The targeted native runs above are fresh and distinct. No release, Braid dependency change or consumer acceptance is implied.

**Executed full checks:** Release build passes (existing CS8604 warning only); 11,296 solution +84 language-server tests pass, zero failures/skips. All52 new cases execute. Temporary fixture junctions are removed. Native current exit2 and all four failures remain recorded separately from the green unit run.
