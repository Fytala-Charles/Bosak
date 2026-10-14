<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak primitive numeric coercion">
  <br><br><h1>Primitive numeric coercion</h1>
  <p>Bounded XPath and XSLT 4.0 correction - 14 October 2026</p>
</div>

Source candidate **1.3.2** supports the primitive numeric pairs in the [XPath 4.0 implicit-casting table](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-coercion-rules). XPath 4.0 function calls use the shared VM conversion path; ordinary XSLT 4.0 functions use the same cast operation when binding supplied or defaulted parameters. Numeric casts retain their cast error codes, including FOCA0002 for NaN/infinity to decimal. Existing decimal representation limits still apply.

| Numeric source | Primitive target |
|---|---|
| xs:decimal, including its integer subtypes | xs:float or xs:double |
| xs:float | xs:decimal or xs:double |
| xs:double | xs:decimal or xs:float |

The target must be the primitive type itself. The specification explicitly excludes double-to-integer coercion and does not combine promotion with derived-type relabeling. This step leaves derived relabeling, general XSLT variable conversion and return-value host conversion outside its scope. XPath 3.1 calls keep their earlier promotion rules.

For example, an XSLT 4.0 function parameter declared `as="xs:decimal"` accepts a supplied `1.25e0` and receives a decimal value `1.25`. Select and sequence-constructor defaults follow the same path. Keyword/arrow calls, named references, lookup, positional partial applications, plural numeric arguments and XML Schema EQName targets are covered. The executable XPath catalog example `seconds(1.25e0)` now returns `PT1.25S`, so its previous observed-error expectation has been updated.

## Correction to the previous specification interpretation

The earlier review overstated a numeric changelog summary and replaced the `1e7` default in upstream `function-40-903` with a QName. The normative table and its explicit exclusion show that double-to-integer remains invalid. [Upstream PR #32](https://github.com/qt4cg/xslt40-test/pull/32) now retains the original fixture and changes only its expected error to XPTY0004; the independent element-default correction for 012 remains. The final upstream diff contains two files, four additions and two deletions against refreshed master `91328fbf0fed4a5d2813eb3661808ee53cb961ba`.

[Revision 2 evidence](../contributions/xslt40/optional-default-corrections/revision-2/README.md) supersedes the original proposal's numeric rationale. Original submission and execution receipts remain historical and unchanged. Acceptance and independent processor confirmation are pending.

## Verification and release state

The initial 27 numeric checks against retained 1.3.1 binaries report **11 passes / 16 failures**. The expanded focused set has **29 passes / 0 failures**, covering all six directions, defaults, function items, earlier-profile refusals, excluded derived targets, non-finite casts, sequences and cardinality. Frozen QT3 remains **31,142/0/679**; qt4 gate-only remains **8,278/0/1,894**. Final solution, related catalog checks and language-server totals are pinned in [the verification receipt](XSLT40-PRIMITIVE-NUMERIC-COERCION.json).

No new public API or gate membership change. Version 1.3.2 is an unpublished source candidate; published libraries remain 1.2.1 and Marketplace extension 0.1.6. Publication requires a separately reviewed release scope and package/consumer verification.
