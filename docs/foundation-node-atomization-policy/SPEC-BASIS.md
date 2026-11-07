# TA-16 bounded original-node data basis

5 November 2026. Core selection before production edits; candidate source remains **1.63.1** at ec6daa0e plus the exact uncommitted TA-15 manifest pinned in INPUTS.json. This is a finite semantic selection for implementation, not activated support or owner acceptance. Re-record implementation source revision/dirty hashes and the selected subset before code changes.

| Immutable normative edition | Affected sections | Selected use |
|---|---|---|
| [F&O 3.1 Recommendation, 21 March 2017](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/#func-data) | 2.4 fn:data | A node contributes its ordered typed atomic values; an absent typed value raises FOTY0012; empty input/result is empty. |
| [XDM 3.1 Recommendation, 21 March 2017](https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/) | 3.3.1.2 typed-value determination, 5.8 nilled, 5.14 typed-value, 6.2.2 element and 6.3.2 attribute accessor behavior | Preserve atomic/list/actual union member values, nilled versus absent/empty distinctions and original type/assessment facts. |
| [XPath 3.1 Recommendation, 21 March 2017](https://www.w3.org/TR/2017/REC-xpath-31-20170321/#id-atomization) | 2.4.2 atomization | Connect explicit fn:data to atomization and its absent-value error without enabling implicit/general VM atomization. |

These dated editions, rather than latest drafts, publication-origin PRs, test-suite revisions or retrieval time, are the selected revisions. No later errata/draft is selected. Review retrieval on 5 November is separate evidence, not a new specification edition.

Intentional first-profile subset: only exact trimmed data($node) / fn:data($node), an explicit supported assessor and one direct original element/attribute or explicitly bound Undefined. Other node kinds, atomics, sequence wrappers, arrays/maps/functions, zero-argument focus, aliases/EQNames, paths/operators and recursive array atomization refuse under the profile. Generic fn:data FOTY0013/array/focus semantics and ordinary untyped inference are not activated. Old no-assessor/ordinary/identity/numeric/nilled routes keep their meaning.

Core host policy (not W3C provenance machinery): original source/issuer tuple proofs, materialized output representation, private resident scalar markers, transactional quotas, copied-payload versus evidence lifetime, immutable provider contracts, error categories and cache keys follow POLICY.md. The original PL-01-10 proposal remains historical, not blanket owner acceptance. No new API signatures are selected by this finite scope, and none are asserted frozen.
