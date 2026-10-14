<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak optional function default conversion errors">
  <br><br><h1>Optional function default conversion errors</h1>
  <p>Verified source correction - 14 October 2026</p>
</div>

When an omitted XSLT 4.0 function argument has a default that does not satisfy its declared type, Bosak now reports **XPTY0004** through its existing function-argument conversion path. The initial optional-function implementation reported XTTE0600. The correction applies consistently to direct, keyword and arrow calls, named references, lookup and partial application. It also preserves errors raised while evaluating the default and skips invalid defaults when a valid explicit argument is supplied.

The [XSLT 4.0 parameter rules](https://qt4cg.org/specifications/xslt-40/Overview.html#function-arguments) delegate function parameter conversion and errors to XPath. [XPath 4.0 static call evaluation](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-eval-static-function-call) applies argument coercion to both supplied values and defaults; [cardinality coercion](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-coercion-rules) reports XPTY0004. Upstream implicit-default case `function-40-904` therefore required a Bosak fix.

Source version advances from unpublished 1.3.0 to **1.3.1**, following the repository patch-version rule. All ten libraries share that candidate version. Published libraries remain 1.2.1 and Marketplace extension 0.1.6; no release publication is authorized in this step. There is no new public API or gate membership change. This fix reuses the existing XSLT host conversion behavior; it does not extend every newer XPath 4.0 numeric coercion into that host.

## Verification

- Ten conversion regressions against retained pre-fix 1.3.0 binaries: **0 pass / 10 fail**. The final optional-function family has **61 pass / 0 fail / 0 skip**, including eleven added cases and the corrected existing implicit-empty assertion.
- Solution tests: **8,230 pass / 0 fail / 0 skip**, including **5,344 XSLT tests**. Separate language-server tests: **77/0/0**. Initial sandboxed solution execution could not initialize the database tests' local HTTP listener; the project passes **47/0/0** with the required access, followed by full-solution verification with that access.
- Frozen QT3 gate: **31,142/0/679**. qt4 gate-only: **8,278/0/1,894**, zero gated failures.
- Unchanged upstream `function-40-904` passes with its existing XPTY0004 assertion. The [separate upstream correction PR #32](https://github.com/qt4cg/xslt40-test/pull/32) corrects 012's node default and 903's obsolete numeric premise/error expectation. Exact 012/903/904 checks pass; final negative controls fail as intended. Corrected upstream sample: **28/8/363**, with the eight unsupported no-namespace cases left unchanged. Pending PR #31's new fixtures are not included in that master-based sample.

Source revision, specification pins, baseline/final assembly hashes and log hashes are recorded in [the fix receipt](XSLT40-DEFAULT-ARGUMENT-ERRORS.json). [Portable upstream correction evidence](../contributions/xslt40/optional-default-corrections/README.md) retains submission metadata and final controls separately from previous receipts. Upstream acceptance and independent processor confirmation remain pending.
