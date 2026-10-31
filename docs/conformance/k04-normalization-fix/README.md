# K04-C21 ? FULLY-NORMALIZED conversion

Completed 2026-10-28 against source base `4ff820672daa9cd74472b7ddc28105232cedeb1f`. Candidate **1.29.1** remains unpublished; public version is **1.26.3**.

`fn:normalize-unicode(..., "FULLY-NORMALIZED")` now converts its input. It classifies the first original Unicode scalar, prepends U+0020 when that scalar is composing, and performs NFC. Classification includes nonzero canonical combining classes, canonical composition seconds and algorithmic Hangul V/T. It correctly handles non-composing marks and supplementary scalars.

The FULLY path uses a coherent, pinned Unicode 17.0.0 backend: 968 nonzero combining-class entries, 2,081 canonical decompositions and 961 composition pairs, plus algorithmic Hangul. The generated resource is 39,305 bytes. Stable canonical ordering preserves equal-class order. NFC/NFD/NFKC/NFKD continue using the existing platform backend; this is no claim that all forms share or certify Unicode 17. Serializer behavior is unchanged. Representative FULLY idempotence is tested; universal FULLY idempotence is not claimed.

Unicode License V3 attribution is in NOTICE. The exact license is embedded in the assembly and included under `licenses/Unicode17-LICENSE.txt` in the locally checked development package. No package was published. Source pins, generator, raw receipts, source identities and hashes are preserved in [evidence](evidence/manifest.json).

## Verification

- New public regressions: **135 passed**, versus 90 failures and 45 passes before implementation. These cover three language profiles and both XSLT execution modes, exact scalar results, empty strings, composing and non-composing marks, Hangul, ordering, supplementary characters and errors.
- All ten solution projects are green, totaling **23,500 tests**. The initial full solution run passed nine projects (including 8,196 XSLT tests) and exposed one revised Standard assertion that rendered a lazy scalar sequence as `(sequence)`. Only its assertion formatting changed. The complete Standard project then passed **13,551 tests**. Both runs and the correction are retained; this total combines those runs rather than claiming a final single solution invocation succeeded.
- Independent Unicode 17 corpus: **20,034 cases**, **200,340 equations**, **1,112,064 scalar classifications**, **1,094,978 identity checks** and four malformed UTF-16 checks; **zero mismatches**. Expected classifications are derived independently from the official text data. Assembly resource hashes match generated data and license bytes.
- C20 review probes: two positive originals and 24 controls change from failure to pass. Originals now yield 4 passes/4 failures; controls yield 57 passes/5 failures. The four negative originals assert unsupported optional normalization behavior and remain inapplicable to this supported form; they are not admitted or rewritten.
- Normalization sets: QT3 **42/0/6**; qt4 **42/2/6**. The two remaining qt4 failures concern the separate 4.0 empty-form contract.
- Complete normal gates: QT3 **31,164/0/657** and qt4 **8,895/0/1,914**, with **229 gated sets** and 11 experimental passes. All retained skip names and reasons match prior evidence exactly. No fixtures, dependency filters, KnownSkips, GatedSets or AGENTS baseline changed.

## Remaining work

Five separate controls remain open: two form-name NBSP cases, two 4.0 empty-form defaults and one keyword-metadata case. Address these in a separate bounded slice before considering normalization-set promotion. The corrected positive QT3 `cbcl-fn-normalize-unicode-006` still has its historical, incorrectly described KnownSkip entry; removing it requires a separate exact-baseline adoption proposal and owner approval. No upstream test correction is warranted for that assertion.

REQ-126 remains Complete under its bounded owner acceptance; all 48 historical unmet batch observations and previous dispositions are preserved. Bosak.Schema retains ownership of staged XSD 1.1 work. This slice approves no release or baseline adoption.
