# K04-C03: explicit feature admission and capability inventory
**K04-C04 bounded collation admission - 27 October 2026:** [Collation review](../k04-collation-admission/README.md) admits existing non-codepoint collation routes in qt4. The original compare-010 assertion passes; the normal qt4 gate improves to **8,438/0/1,892**, with only that gated skip removed and all retained reasons unchanged. Four exploratory original assertions also pass; eight XQuery-only cases remain dependency-skipped, with their direct diagnostic failures preserved. Seven expression controls and248 before/after filter controls pass. Fresh solution validation passes **22,007 tests**. Normal QT3 remains **31,161/0/660**. No engine, fixture/assertion, GatedSets, schema or QT3 baseline change. Candidate1.26.11 unpublished/public1.26.3; Bosak.Schema retains XSD1.1 ownership; REQ-126 remains Complete. Next: bounded review of four qt4 CLDR format-integer declarations. Earlier K04 notices are historical checkpoints.


27 October 2026. Source base `8ea5e4ae`; candidate **1.26.11** unpublished, public **1.26.3**.

Both conformance filters now reject unrecognized feature names for either dependency polarity. Previously an unknown positive feature silently passed, while its negative failed. Unknown means unreviewed; neither support nor non-support is inferred. Existing unsupported declarations remain unchanged. Existing admissions are now explicit in `FrozenAdmittedFeatures`: **19 QT3 names and 17 qt4 names**, preserving their previous behavior, including case-insensitive comparisons.

These sets document the frozen admission policy. They do not certify complete implementation of each named feature. The [55-row capability ledger](evidence/capability-ledger.json) separates actual positive/negative filter decisions, source mechanisms/limitations, and original accepted-gate observations. A passing expected-error assertion is not proof of complete support. Adding a future name now requires an explicit profile review rather than silently inheriting admission.

## Inventory and evidence

The reproducible inventory covers **31,821 QT3 cases and all 46,985 qt4 cases**, including inherited declarations. It records every dependency kind/value/polarity and counts overlapping mentions separately from distinct feature-associated cases. There are **24 observed QT3 feature names and 31 qt4 names**. All names are either explicitly admitted or already declared unsupported; no existing feature decision changes. Only the **10,330 qt4 cases in 224 gated sets** execute in the normal verification run; exploratory cases are metadata-only in this slice.

**222 controls** run before/after through actual production filters in four externally bounded 30-second processes. They include every observed name, existing unsupported aliases, known-name casing, empty/misspelled/new names, conjunctions, contradictions, and the prior spec/language/schema/XML/Unicode controls. Exactly **23 profile outcomes across 15 control rows** change: formerly admitted unrecognized positive requirements now reject. Unknown negatives and recognized-feature decisions stay unchanged. The [results](RESULTS.json) enumerate every change.

Fresh validation: build succeeds; **22,007 solution tests pass**. Normal QT3 stays **31,161/0/660**, qt4 gated stays **8,437/0/1,893**, with 11 Experimental passes. All **660 QT3 and 1,893 qt4 skip names/reasons** match the preceding accepted runs. No fixture/assertion, baseline, GatedSets, engine source, public API or package version changes. All **1,410 prior pinned artifacts** are preserved.

## Capability findings retained for follow-up

| Feature | Current profile observation | Bounded implication |
| --- | --- | --- |
| `non_unicode_codepoint_collation` | QT3 admits 13 original cases; qt4 rejects it, including one gated case | Next candidate: verify the original qt4 assertion and collation route before a scoped admission change |
| `typedData` | QT3 admits it; qt4 explicitly rejects it | Harness PSVI support remains profile-specific; no general typed-source capability inferred |
| `fn-format-integer-CLDR` | QT3 has two original assertion passes; qt4 rejects it | Existing word-formatting paths do not establish complete CLDR support |
| `expath-file` | qt4 previously admitted 270 exploratory cases; none are gated | Explicitly retain the historical policy and flag unverified implementation; this is not a support claim |
| `schemaImport` / `schemaValidation` | Both profiles admit the names, while many original cases remain skipped by other guards | Bosak.Schema owns staged XSD 1.1; current XmlSchemaSet seam and bounded assertions do not certify XSD 1.1 |
| `collection-stability` | Existing positive admission makes the two negative cases skip in each catalog | Independent stability review required before changing that policy |

K04-F03's unknown feature-name fallback is resolved for this bounded profile; other dependency kinds and their fallback/negative policies remain open. Positive mixed-language routing and prolog/pragma guards remain unchanged. K04-F04 schema integration and F05 qt4 XQuery routing remain separate. REQ-126 remains Complete for its accepted bounded catalog. No release, push, tag, Braid change or upstream submission occurs.

Next recommended slice: investigate the single gated qt4 non-codepoint-collation skip, using its original assertion and bounded collation controls before considering admission.

```powershell
python docs/conformance/k04-feature-inventory/evidence/build-inventory.py
python docs/conformance/k04-feature-inventory/evidence/build-ledger.py
python docs/conformance/k04-feature-inventory/build-review.py
python docs/conformance/k04-feature-inventory/evidence/validate-evidence.py
```
