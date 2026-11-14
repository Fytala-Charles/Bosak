# Span-based validator candidate

ValidateFunctionConversionTarget previously copied the xs: bare name before built-in classification. It now uses a `ReadOnlySpan<char>` over the normalized string. Existing string callers forward into that same classifier, with the original 47 names defined once. Normalization, list/none refusal order, original-case schema lookup and dynamic context remain unchanged. There is no validity cache or new type vocabulary.

The [adoption record](ADOPTION.json) predates engine edits and pins [XPath 3.1 REC-xpath-31-20170321](https://www.w3.org/TR/2017/REC-xpath-31-20170321/) sections 2.5.5.2, 2.5.2, 3.1.5.2 and 2.3.4, with no intentional deviations or new language adoption. Development Core/server 1.81.12 remains unpublished.

| Check | Result |
|---|---|
| New validator regressions | 5 passed |
| Focused validator/matcher/typed-list suite | 16 passed |
| Full solution | 26,440 passed |
| Separate language-server suite | 90 passed |
| Canonical primitive function conversion | Baseline 40 bytes/call; candidate 0 |
| Canonical primitive validator | Baseline 40 bytes/call; candidate 0 |
| Short xs:int/xs:long validator | Baseline 32 bytes/call; candidate 0 |
| Existing uppercase/trim/occurrence normalization | Candidate 48 bytes/call retained |
| xs:anyAtomicType normalization | Candidate 56 bytes/call retained |
| element(a) normalization | Candidate 40 bytes/call retained |
| Frozen QT3 | 31,397 / 0 / 424; all skip names/reasons unchanged |
| Gated qt4 | 8,941 / 0 / 1,918; all skip names/reasons unchanged, 11 experimental passes |
| Complete pinned standalone | 10,221 / 0 / 4,380; all 14,601 identities/exclusions, 784.991s |

Five new facts check the frozen original vocabulary against both string/span entry points, raw nonmatches/null, normalization and built-in refusals, namespace rebinding and SchemaSet replacement, user lists/list-containing unions/restricted unions, namespace-sensitive conversion errors and isolated validator allocation. The five primitive conversions now require exact zero in the established isolated worker. No fixed allowance, retries or minimum selection. Schema/type-context checks still run on each relevant call.

Compatible controls compile the identical production worker source against exact qualified 1.81.11 references, then run that same compatible binary against both old and candidate DLL sets. References bind equal or upward; no incompatible downward-loading experiment. Each mode runs once under a 60-second/512-MiB Windows job bound and scoped 256-MiB managed-heap cap, .NET 10.0.11 with roll-forward disabled. DLL hashes are checked before/after; setup/reflection/output is outside 100,000 measured calls after 10,000 warm-ups. Canonical conversion decreases 40 -> 0 and short-name validation 32 -> 0; mixed-case/trim/suffix costs decrease only by the removed bare-name slice. These calling-thread observations establish neither suite-wide speed nor a hosted timeout cause.

Qualified source checkpoint: deae35494cf2f575a074d5dd1d02269cb2eb4eed. Candidate nine DLL hashes match every common engine DLL in tested output. The corpus remains fddf1cf920087e791f13315d68dfbe874d97dc56, Legacy/Xslt30 basic non-schema-aware, original 1200-second/8-GiB limits. Every fixture/runtime hash and exclusion is unchanged. The QT3 fixture junction was verified and removed before clean-source standalone preflight.

See [results](RESULTS.json), [compact evidence](EVIDENCE.zip), [manifest](MANIFEST.json), [offline verifier](verify.py) and [HTML receipt](DOCUMENTATION-CHECKS.json). Run `python docs/validator-span/verify.py` to replay source/vocabulary binding, old/new rows, all unit counts, every frozen skip and all 14,601 standalone identities without launching .NET. Evidence contains text/JSON/CSV/source snapshots only; no new runtime binaries or profiler traces. Full runtime/control packets remain ignored under `.guard-tmp/validator-span-20261114` and `.guard-tmp/validator-span-qualified-20261114`.

Predecessor main independently qualifies in 873.266s with all six checks passed; its receipt does not qualify this change. Hosted candidate checks and independent artifact verification remain required before merge. Earlier test-host allocation failures and isolated-worker corrections remain historical and unchanged. No release is performed.
