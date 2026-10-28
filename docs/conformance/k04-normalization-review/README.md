# K04-C20: FULLY-NORMALIZED contract and capability review ? 28 October 2026

**Review complete; implementation remains open.** The current FULLY-NORMALIZED branch validates NFC/category conditions instead of performing the specified conversion. The positive Bengali fixture exposes a real engine error; its leading space is required, despite the current QT3 ?upstream defect? label. The negative unsupported-form fixtures are conditional cases, not instructions to make an accepted form fail.

## Reviewed contract and source pins

[Review pin](evidence/SPECIFICATION-REVIEW.json) records the dated [F&O 3.1 normalization rules](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/#func-normalize-unicode), immutable qt4cg e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e and exact source hashes. F&O defines conversion: inspect the first scalar for composing status, add one U+0020 when required, then normalize to NFC. Composing status includes nonzero canonical combining class and eligible canonical-composition second characters; a Unicode general category alone is insufficient. Supported forms must normalize valid input, rather than reject input for needing normalization.

[Unicode 17.0 metadata](https://www.unicode.org/Public/17.0.0/ucd/UnicodeData.txt), composition-exclusion data and [UAX #15 revision 57](https://www.unicode.org/reports/tr15/tr15-57.html) provide selected character witnesses. They are review references, not a production data adoption or a declaration that this .NET runtime implements Unicode 17. The future classifier and NFC backend need an explicit, coherent version policy; four observed NFC composition checks do not certify all Unicode normalization data.

## Evidence and concrete findings

| Finding | Reproduced evidence | Disposition |
| --- | --- | --- |
| C20-F01 conversion | Decomposed Latin/Bengali/Hangul input raises FOCH0003 instead of being normalized; leading combining characters are rejected or returned without the required space | Correct conversion before capability/admission changes |
| C20-F02 classifier | U+09BE has CCC=0 but is the second component of U+09CB; Hangul V/T jamo and supplementary U+1D165 also need the prefix. U+034F and U+20DD are category-based false rejections | Use scalar/property/composition rules, including algorithmic Hangul; avoid category shortcuts |
| C20-F03 skip misclassification | Original cbcl-fn-normalize-unicode-006 expects space + U+09BE; actual result lacks space. QT3 labels that space artifactual | Keep the valid original assertion; document the incorrect label pending a separate approved baseline disposition |
| C20-F04 form spelling | A leading NBSP is wrongly stripped from an NFC form name | Separate XML-whitespace/effective-form correction |
| C20-F05 4.0 call contract | The frozen form parameter defaults to NFC on empty; two original cases raise XPTY0004. A keyword control also lacks metadata | Separate profile-aware default/keyword implementation, preserving 3.1 required-form behavior |

The unit NormalizeUnicode_FullyNormalized currently expects FOCH0003 for decomposed input and checks only count=1 for Bengali. The former encodes the wrong behavior; the latter cannot distinguish strings with and without the required space. Preserve its historical source/output and replace those checks with exact converted results when implementing F01/F02.

## Actual outcomes

All 78,806 catalog cases were scanned: **14 normalization-form declarations**, eight for FULLY-NORMALIZED (four positive/four negative across both profiles). Eight selected original probes give **two passes/six failures**: ASCII positive cases pass; the two positive Bengali cases fail; four negative expectations fail in direct bypass diagnostics. Their FOCH0003 expectation applies only when the form is unsupported. Acceptance of a few strings is not full capability proof. Leave their admission unchanged while repairing conversion.

**62 controls give 33 passes/29 failures**: 24 conversion/classification failures, two non-XML form-whitespace failures, two 4.0 empty-form failures and one 4.0 keyword failure. ASCII/empty values, non-composing contrasts, NFC witnesses, existing type checks and 3.1 boundaries remain passing controls. [Character witnesses](evidence/codepoint-witnesses.json), [control contract](evidence/control-contract.json), raw outputs and [receipts](evidence/probe-receipts.json) preserve exact inputs/results.

Fresh complete normal normalization sets:

| Profile | Passed / failed / skipped | Interpretation |
| --- | --- | --- |
| QT3 | **42 / 0 / 6** | Five dependency exclusions plus the misclassified positive known skip |
| qt4 | **41 / 3 / 6** | Bengali positive failure plus two 4.0 empty-form failures; set remains ungated |

Actual filter/parser metadata admits the positive FULLY dependencies and rejects the negative ones. QT3's known-skip mapping then separately suppresses the positive Bengali case. These layers must not be conflated. All **74 bounded diagnostic/filter/normal-set processes** finish without timeout. Exploratory process exit zero does not mean its assertions pass.

## Next slice and boundaries

[Implementation scope](NEXT-SLICE.md) recommends F01/F02 conversion and composing-character classification first, with immutable semantic/data adoption and exact result tests. Keep the current skip mapping/filters and frozen totals until a later review can propose the positive known-skip removal. Do not remove negative unsupported-form exclusions to manufacture passes. Address F04/F05 independently; qt4 promotion requires all remaining failures to be corrected and fresh complete zero-failure sets.

No engine, harness, filter, fixture/assertion, version or baseline change here. Candidate **1.29.0** unpublished/public **1.26.3**. Complete solution **23,365 passes**, QT3 **31,164/0/657** and qt4 gate **8,895/0/1,914** are inherited source-identical C19 evidence, not fresh full-suite/gate runs. Bosak.Schema retains staged XSD 1.1 ownership; REQ-126 remains Complete with historical dispositions and 48 unmet batch observations unchanged. No release, upstream submission or Braid action.

Source base: ab2a3d0d. Delivery revision: `git log -1 --format=%H -- docs/conformance/k04-normalization-review/README.md`. Run `python docs/conformance/k04-normalization-review/evidence/validate-evidence.py` to verify preserved identities, selected character facts, original/control outcomes and normal scopes. HTML renderer receipts use unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.
