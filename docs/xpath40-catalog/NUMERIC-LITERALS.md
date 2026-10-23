<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-126 numeric literal delivery audit">
  <h1>Numeric literal boundaries</h1>
  <p>23 October2026 - source1.23.7 unpublished; public1.23.4</p>
</div>

**Public1.23.7 - 23 October2026:** EQName bracket parsing, inline-record signature gates and numeric separator boundaries are [published and verified](../RELEASE-1.23.7-PUBLICATION.md). Current catalog74 entries/2,893 documentation checks; REQ-126/AC-03 remain partial. Braid dependency1.23.4 unchanged. Earlier source/unpublished checkpoints below are historical.

## Contract and correction

[Dedicated16-field entry](operators-and-syntax/numeric-literals.md) and [complete telemetry task](examples/numeric-literals/README.md) cover lowercase0x/0b prefixes, numeric types and keys, internal repeated separators, decimal/exponent groups, old-profile refusals and string-cast differences. The [machine audit](NUMERIC-LITERALS.json) lists all50 new cases and their exact outcomes. Existing2,819 manifest rows and their serialized text are preserved.

Base: merged PR411 `df27eeb01d6489edd2a22c937eda280135ef5725`. [Immutable grammar](https://raw.githubusercontent.com/qt4cg/qtspecs/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/grammar-40/xpath-grammar.xml), refreshed against upstream master on23 October2026. This is a current comparison revision, not a recovered historical adoption pin.

The first38 catalog probes were30pass/8fail. The lexer accepted `1._2`, `1.2_`, `1e_2`, `1e2_`, `1e+_2`, `.1_`, `1.2_e3` and `1.2e3_`. The grammar's Digits group begins and ends with a digit, allowing repeated internal underscores. A bounded XPathLexer correction rejects those fractional/exponent edge forms as XPST0003 in4.0; 3.0/3.1 remain unchanged. Valid `1__2.3__4`, `1_2.5e+0__2`, `1_2.` and `.1__2` remain accepted. Thirty-four named [API regressions](../../tests/Bosak.XPath.Api.Tests/NumericLiteralBoundaryTests.cs) cover eight malformed forms in all four profiles plus valid-group controls in both4.0 profiles. Source patch version advances1.23.6 to1.23.7. No expected result was weakened.

The report compares equivalent decimal-constant3.0 and based/separated-constant4.0 stylesheets. Eight exact report outcomes cover normal, empty, reordered and malformed data. Source literal spelling does not change XML Schema input lexical conversions or supply bitwise operators. Numeric overflow/arbitrary precision, other adjacency/Unicode forms, broader XQuery/package/host contexts and feature-specific qt4 assignment remain outside scope.

## Verification

All10,720 solution tests pass with no failures/skips, including7,674 XSLT,2,869 feature and24 chapter checks. The34 new API and50 catalog cases are included in that total. An additional84 language-server tests pass. Both engine gates rerun against unchanged source fixtures: QT3 **31,142/0/679**, qt4 gated **8,278/0/1,894**. No set membership, fixture, expectation or harness change.

Release builds have zero errors. Initial/rebuilt logs retain the existing CS8604 conformance-runner warning; final incremental build has zero warnings. Initial full-run fixture-path and restricted loopback-listener failures are retained separately; corrected fixture/listener runs pass. XSLT also passes from a unique temporary copy using the documented workaround. External detailed TRX/hash/log receipts remain under `.guard-tmp/req126-numeric-literals-evidence`; no existing evidence is overwritten.

Catalog **74 entries;2,869 feature +24 chapter =2,893** checks; four worked chapters and117 assigned function names unchanged. The18-row matrix and current inventory are regenerated. All16-field/index, JSON/XML/local-link and diff checks are verified separately with the renderer receipt. HTML rendering uses unchanged Fytala CSS; no screenshot/PDF approval is claimed.

REQ-126 and D2/D3/D4/AC-03 remain partial under the owner-approved applicable-evidence policy.28 historical adoption pins remain missing. Public/Braid dependency1.23.4, README install/build instructions and protected PR132 remain unchanged. No release, package publication, tier change or Braid acceptance claim. Next bounded scope: declaration/package/profile evidence, separate from open D2 defaults/options and D4 historical pins.
