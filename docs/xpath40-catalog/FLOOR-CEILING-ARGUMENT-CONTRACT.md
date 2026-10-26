<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala floor and ceiling argument contract audit">
  <h1>Floor and ceiling argument contract audit</h1>
  <p>26 October 2026 — source 1.26.3 unpublished; public 1.26.2 unchanged</p>
</div>

**Consolidated checkpoint:** [Verified unsigned 1.26.3 candidate](../RELEASE-1.26.3-CANDIDATE.md) pins both integer-preservation corrections and this audit. Public 1.26.2 stays current; original audit proof/evidence remains unchanged.

This REQ-126 audit starts from `1c20de059f268fe6fd76eb355f01b8e6896dc217`, after [integer identity preservation](FLOOR-CEILING-INTEGER-PRESERVATION.md). **No engine correction is required by the sampled contract tests.** It adds tests and retained evidence to the existing unpublished 1.26.3 patch; public 1.26.2 remains current.

## Contract and result

[fn:floor](https://www.w3.org/TR/xpath-functions-31/#func-floor) and [fn:ceiling](https://www.w3.org/TR/xpath-functions-31/#func-ceiling) declare an optional numeric argument. Public function dispatch already applies their `xs:numeric?` metadata through `VmEngine.ApplyFunctionConversion` before invoking the implementation. This audit therefore leaves the implementation, metadata, atomization, numerical calculations and conversion pipeline unchanged. It does not add redundant boundary conversion based only on the implementation's internal `AtomizeValue` call.

The **802 new cases all pass on the unchanged engine**: 798 modern-profile cases and four supported static backwards-compatible controls. There is no before/after defect count and no passing assertion that encodes a discovered defect. Direct calls, named references, lookup calls and inline wrappers run under XPath31, XPath40 and XPath40Experimental.

| Sampled behavior | Expected and observed |
|---|---|
| Empty sequence, empty arrays and recursively empty arrays | Empty result in modern evaluation |
| Singleton recursively atomized arrays with empty members | One rounded numeric result |
| Multiple atomic results, including nested arrays | XPTY0004 |
| Valid untyped atomic input and untyped attributes | Correct rounded values |
| Invalid untyped input or attribute | FORG0001 |
| String and boolean arguments, including array members | XPTY0004 |
| Maps and function items, including array members | FOTY0013 |
| Multiple untyped element nodes | XPTY0004 |
| Explicit integer/decimal/float/double array inputs | Corresponding numeric type retained |
| Coercion to the matching `function(xs:numeric?) as xs:numeric?` type | Named-function identity retained |
| Supported static backwards-compatible sequences/empty input | First-item result /NaN |

Node controls use a fresh public XDocument adapter and focus containing valid/invalid attributes and two numeric elements. Expected values and errors are declared before execution. A separate public API probe repeats **798 modern observations**, with **zero discrepancies**, all rows executed and no 30-second external-batch watchdog timeout. Probe and test counts overlap, and expected-error controls are successful tests rather than failed conformance observations. No benchmark or per-expression timeout guarantee is claimed.

The audit is bounded. It does not certify generalized metadata/coercion behavior, dynamic backwards-compatible conversion, schema-list atomization, error precedence with multiple independently invalid conditions, or every numeric/host boundary. Existing numerical and integer-identity checks remain in their separate suites.

## Validation and state

Fresh Release build has zero errors and the existing CS8604 conformance warning. All **21,798 solution +84 language-server tests pass (21,882 total)**. Fresh frozen gates remain **QT3 31,142/0/679** and **qt4 8,436/0/1,894**, with 11 existing Experimental fallback passes. No current-native Unix rerun is claimed. Nine changed pages render as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance.

[Machine-readable proof](FLOOR-CEILING-ARGUMENT-CONTRACT.json), [diagnostic checker](evidence/floor-ceiling-contract/analyze.py) and [portable verifier](evidence/floor-ceiling-contract/validate-evidence.py) retain exact source/test fingerprints, predeclared inputs, raw unit-test/probe receipts, commands and hashes. The engine source fingerprint matches the audit base; this is one unchanged-engine audit, not a correction or a separately claimed isolated baseline.

The unpublished patch now contains **3,061 added cases**: 1,323 abs cases, 936 floor/ceiling integer cases and 802 contract-audit controls. Original abs, rounding, publication/candidate and Braid adoption snapshots remain untouched. Braid's previously supplied report establishes bounded local automated public 1.26.2 adoption at capture, with uncommitted status and no current-adoption CI/new native UI acceptance; it establishes no 1.26.3 adoption. This task makes no Braid changes.

REQ-126/D2/D3/D4/applicable-evidence AC-03 remain partial; inventory remains 76 entries /3,148 feature +24 chapter checks. Numeric storage, general constructors/casts, Int64 idiv, schema-list/general coercion and DateTimeOffset Unix limits remain explicit. No API, engine, version, fixture/gate/retry, workflow, kit, upstream, Braid or publication change occurs.

Next: prepare a consolidated 1.26.3 package candidate and run PackageReference-only consumer checks for all three added suites before a later publication decision.
