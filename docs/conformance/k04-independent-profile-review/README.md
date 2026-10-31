# K04-C39 — Independent dependency-profile review

28 October 2026. Read-only review base **2806ddac**. All **370** remaining QT3 Unsupported dependency cases have named dispositions. Normal source, filters, assertions, fixtures, baseline and version are unchanged. Candidate **1.33.0** remains unpublished/public **1.26.3**.

## Complete review and overlapping blockers

Fresh actual harness inventories cover **78,806 cases / 1,166 sets**: QT3 **31,821 / 428**, qt4 **46,985 / 738**. They use the existing expression reader and dependency filters, with qt4's expression-entity decoding and gated-set membership. Every singleton dependency is evaluated under its applicable profile; qt4 is XPath-only. The complete QT3 inventory reproduces the adopted C38 expression identities and filter decisions. All 370 original expressions, assertions, modules, environments and dependency records are preserved.

| Mutually exclusive case disposition | Cases |
|---|---:|
| Earlier exact specification applicability | **165** |
| Declared unsupported optional features | **93** |
| Negative dependencies on frozen-admitted features | **26** |
| XML-version profile exclusions | **39** |
| Legacy XSD-version policy requiring separate capability review | **37** |
| Unreported specific Unicode versions | **5** |
| Negative dependencies on implemented FULLY-NORMALIZED | **2** |
| Different default-language requirement | **1** |
| Known unsupported `xib` language polarity candidate | **2** |
| Total | **370** |

There are **371 rejected singleton occurrences**. `fn-transform-902` has two negative transform-feature blockers in the same disposition category; counts of dependency occurrences must not be mistaken for case counts. [The named ledger](evidence/skip-ledger.json) accounts for all **426** skips, keeping the other **56** separate: unattended-sweep, platform/range, external-resource and six existing schema-initialization errors.

These are admission/profile dispositions, not 370 engine defects or certifications of complete optional-feature support. Earlier exact-version semantics must not be admitted simply because the current parser accepts their syntax. The frozen-admitted feature list explicitly preserves legacy policy, so its negative tests cannot be enabled merely by flipping polarity. XML and Unicode profile claims remain bounded; the private Unicode17 backend for FULLY-NORMALIZED does not establish a version guarantee for every .NET-backed normalization form.

The **37 XSD 1.0** exclusions reflect frozen catalog policy, not proof that the core supplies an XSD 1.1 validator. They require their own capability/ownership review. They are distinct from the six current QT3 schema-loader errors and from the Schema documentation's separate XSLT conformance mapping.

## Bounded original and capability observations

| Fresh check | Outcome |
|---|---|
| Complete normal QT3 | **31,395 / 0 / 426** |
| Complete normal qt4 gate | **8,939 / 0 / 1,920** |
| Original `format-date-en151` and `format-dateTime-en151`, each suite | **All four assertions pass directly** |
| Two unsupported-form normalization negatives, each suite | **All four fail directly as expected for implemented support** |
| QT3 `default-language-006` under the current default | **Fails directly: expected fr-CA, actual en** |
| Independent formatting controls | **12 pass** |
| Actual dependency-polarity observations | **32 per suite** |

Direct diagnostics bypass dependency admission; the five preserved assertion failures are intentional counter-profile witnesses, not new gate regressions. Both normal skip maps exactly match C38. qt4 retains 230 gated sets and 11 experimental passes. All **34 diagnostic processes** finish within external watchdogs, with exit zero and no timeout; assertion outcomes are examined independently of process exits. Full solution evidence is explicitly inherited from adopted C38: **24,164 passing tests**, with normal binaries unchanged.

The four unchanged language originals require fallback for unsupported Iberian `xib`. Actual `FormatDateTimeEngine.IsLanguageSupported` source rejects that tag, and both date functions produce the original assertions' required English marker and month name. Twelve public controls independently cover date/dateTime/time and `xib`, `XIB`, English and German behavior. These witnesses do not certify arbitrary language support. The fixed [Functions and Operators 3.1 §9.8.4.8](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/#formatting-dates-and-times) makes language support implementation-defined and requires the actual fallback language to be identified when it differs.

Both filters currently admit positive `language=xib` metadata by their generic default and reject its negative form. This disagrees with the bounded existing formatter capability. Unlike the normalization negatives, these four original negative-language assertions already pass. Both qt4 originals belong to existing gated date-formatting sets.

**Next recommendation: C40 exact `xib` metadata qualification in both filters.** Review only the canonical `language=xib` token as known unsupported: reject its positive dependency and admit its negative dependency, retaining all other tokens and rules. Complete isolated rehearsals must establish the exact two-case changes per suite before any baseline proposal. Conditional target totals are QT3 **31,397/0/424** and qt4 **8,941/0/1,918**, with existing gate membership unchanged. C39 itself qualifies no adoption and supplies no production patch.

## Bosak.Schema ownership and readiness

Five current working-tree documents were read and pinned byte-for-byte, together with sibling HEAD **228c9bea101760bce480b1f011998f887217097c** and its existing untracked-file status. The gap matrix contains documented S4b/S5a/S5b add-on progress; its S0 core-seam baseline header must not be read as a claim that the add-on is doing no XSD 1.1 work. ADR-006 remains **Proposed**; S4 preparation still conditions S4c on core owner acceptance. The core REQ-139 section remains **Pending**, with its ADR acceptance item unchecked. No sibling file or git configuration was changed, and no callback seam, test-only dependency, backend or license boundary is accepted here.

Bosak.Schema retains XSD 1.1 ownership. Current core XmlSchemaSet limits neither establish complete add-on capability nor authorize bypassing its staged integration contract. The 37 profile exclusions, six QT3 loader errors and the Schema documents' separately mapped XSLT cohort must remain separate ledgers.

REQ-126 remains Complete under bounded catalog acceptance, preserving all 48 historical unmet observations. No source, filter, baseline, fixture/assertion, GatedSets, package version, release, publication, upstream or Braid change is included. Run `python docs/conformance/k04-independent-profile-review/evidence/validate-evidence.py` to verify the complete ledger, original diagnostics, immutable inputs and scope. Ten HTML renders use unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.
