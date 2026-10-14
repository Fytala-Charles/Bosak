# Optional default fixture corrections

**Submitted 2026-10-14:** [Upstream PR #32](https://github.com/qt4cg/xslt40-test/pull/32). Review, acceptance and independent processor confirmation are pending. This correction is separate from new-case PR #31; original contribution receipts remain unchanged.

| Case | Correction | Reason |
|---|---|---|
| function-40-012 | Default selects `$data/a` instead of `$data/a/@code`; existing `blue` assertion retained | The function body selects `@code` from its parameter. An attribute has no attribute children. |
| function-40-903 | Default is `QName('urn:invalid', 'invalid')` instead of `1e7`; expected error is `XPTY0004` | Current XPath 4.0 allows numeric coercion. A QName remains incompatible with `xs:integer*`. Function argument coercion applies to defaults too. |

The [XSLT function parameter rules](https://qt4cg.org/specifications/xslt-40/Overview.html#function-arguments) delegate conversion and errors to XPath. [XPath static call evaluation](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-eval-static-function-call) applies coercion to supplied and defaulted arguments. The [coercion rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-coercion-rules) permit numeric conversions and reject incompatible items/cardinalities. The change to 903 corrects both its obsolete numeric premise and its error expectation; changing only the error code would leave that premise unresolved.

Both corrected cases pass in Bosak source candidate 1.3.1. Unchanged `function-40-904` also passes after the [Bosak default-conversion fix](../../../docs/XSLT40-DEFAULT-ARGUMENT-ERRORS.md). The corrected current-upstream `function-40-` sample has **28 passes / 8 failures / 363 unselected or unsupported skips**. The eight failures are existing unsupported no-namespace cases 014–021. This sample excludes the three new cases in pending PR #31, which is based independently on master.

Negative controls: restoring only 012's original parameter default reports `XTTE0780` from an empty function result. Replacing 903's QName default with valid integer `7` fails its expected-error assertion. Original corrected fixtures were restored byte-for-byte. A preliminary 012 mutation also changed `apply-templates/@select`, bypassing the function through built-in attribute processing; that run is excluded from assertion evidence. Saved final controls target only the default. Catalog schema validation passes.

The upstream base was fetched before branching and again immediately before submission; it remained `91328fbf0fed4a5d2813eb3661808ee53cb961ba`. The three-file diff has six additions/four deletions and passed whitespace checks. `UPSTREAM_SUBMISSION.json` records the exact base/head. Apply the portable patch to that base and reproduce from Bosak:

```powershell
powershell -ExecutionPolicy Bypass -File ./validate-xslt40-catalog.ps1 -Files tests/decl/function/_function-test-set.xml
powershell -ExecutionPolicy Bypass -File ./run-xslt40-tests.ps1 -TestSet function -TestCase function-40-012 -NoBuild
powershell -ExecutionPolicy Bypass -File ./run-xslt40-tests.ps1 -TestSet function -TestCase function-40-903 -NoBuild
powershell -ExecutionPolicy Bypass -File ./run-xslt40-tests.ps1 -TestSet function -TestCase function-40-904 -NoBuild
```

Build the Release conformance project first if needed. Receipts preserve original source/suite revisions and dirty-state metadata; the final engine revision and source/binary hashes are pinned in the separate Bosak fix evidence. Existing acceptance records, assertions outside these two cases and gate membership remain unchanged. Source candidate 1.3.1 is unpublished; libraries 1.2.1 and Marketplace extension 0.1.6 remain published.
