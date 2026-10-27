# K04-C02: consistent bounded dependency polarity

27 October 2026. Source base `8a5eba4a`; candidate **1.26.11** unpublished, public **1.26.3**.

The QT3 and qt4 filters now evaluate negative `spec` and `default-language` dependencies by inverting the same positive predicate for the selected profile. This corrects contradictory results without broadening the positive profile or changing routing. The correction uses the pinned local catalog-schema.xsd dependency polarity and space-separated spec alternative definitions; it changes harness metadata evaluation, not engine language semantics.

For example, QT3 admitted-XQuery mode previously accepted both positive and negative `XQ31+`; the negative now rejects that profile. The qt4 XPath profile previously admitted a negative mixed `XP31+ XQ31+` dependency despite supporting its XPath alternative; it now rejects it. Both filters previously rejected positive and negative `default-language="fr-CA"`; the negative now admits it. A conjunction requiring both a predicate and its negation now rejects all reviewed profiles.

## Scope and verification

The inventory scans **31,821 QT3 cases and all 46,985 qt4 cases**, including inherited test-set declarations. Neither catalog contains a negative `spec` or `default-language` dependency. Therefore existing catalog admissions should not change; fresh normal gates and exact skip comparisons verify this. qt4 metadata inventory includes exploratory sets; query execution remains restricted to **10,330 cases in 224 gated sets**.

**74 synthetic controls** run before and after through the actual production filters in four externally bounded 30-second processes. QT3 XPath, QT3 admitted-XQuery and qt4 XPath give 111 profile outcomes per phase. Controls cover positive/negative exact/range/mixed spec predicates, supported/unsupported default languages, contradictions, and unchanged feature/schema/XML/Unicode boundaries. [Results](RESULTS.json) records every changed outcome. Positive predicates and all other negative dependency kinds remain unchanged.

The schema comments now identify `xsd-version=1.1` as the frozen admission profile, not proof of an implemented XSD 1.1 backend. Bosak.Schema retains staged XSD 1.1 ownership; the core seam currently uses XmlSchemaSet. No schema capability or admission predicate is added or removed. [Schema owner pin](evidence/schema-owner-pin.json) records the reviewed external document revision/hash without modifying that repository.

Fresh validation: solution build succeeds; **22,007 solution tests pass**, normal QT3 **31,161/0/660**, qt4 gated **8,437/0/1,893**, with 11 Experimental passes. All **660 QT3 and 1,893 qt4 skip names/reasons** match the previous accepted runs. Baselines, AGENTS.md, fixtures/assertions, GatedSets, engine sources and package version are unchanged. All 1,376 pinned previous artifacts remain intact. No additional skips are removed in this slice.

## Remaining work

K04-F03 is partially resolved. The positive QT3 mixed-language routing policy is retained: the filter still treats mixed XP/XQ declarations conservatively in XPath-only mode. This slice makes negative dependencies the logical complement of that existing predicate; it does not redefine the positive policy. Unknown positive features still default to supported, and other negative dependency kinds retain their prior capability policy. These require an explicit capability inventory and independent evidence before changing admissions.

K04-F04 wording is corrected, but schema admission remains a frozen harness policy pending Bosak.Schema capability integration. Unsupported prolog/pragma guards (F02) and qt4 XQuery routing exclusions (F05) remain. REQ-126 stays Complete for the accepted bounded catalog. No release, push, tag, Braid change or upstream contribution occurs.

Next recommended slice: inventory every feature/capability declaration against explicit harness and engine evidence, starting with unknown-positive feature fallback. Preserve current admissions until a concrete bounded correction is verified.

```powershell
python docs/conformance/k04-profile-review/evidence/build-inventory.py
python docs/conformance/k04-profile-review/build-review.py
python docs/conformance/k04-profile-review/evidence/validate-evidence.py
```
