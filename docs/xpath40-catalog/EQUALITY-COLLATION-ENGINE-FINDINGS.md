<div align="center">
<img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak equality and collation engine follow-ups">
<h1>Equality and Collation Engine Follow-ups</h1>
<p>21 October 2026 — separate from the documentation delivery</p>
</div>

**Index-of correction - 21 October 2026:** Source1.23.2 [defaults an empty collation only in4.0](INDEX-OF-EMPTY-COLLATION.md), retaining XPath3.1 behavior. The manifest reproducer now verifies result1 plus the3.1 XPTY0004 baseline. Original source5de2cfc5 findings/hashes below remain historical; deep-equal options and unrelated keyword findings remain open. No new release.


These bounded findings are recorded against source 5de2cfc5 (unpublished1.23.1). No engine fix or release is included in the REQ-126 documentation slice. The [contract review](EQUALITY-COLLATION-CONTRACT-REVIEW.md) and [raw audit](audit-equality-collation-contracts.json) pin the draft declarations and original receipts. A future engine correction must retain the real customer report, add specification-derived regressions, assess3.1 compatibility and rerun both mandatory engine gates.

| Reproduction in explicit XPath40 | Pinned draft outcome in this context | Current observation | Scope of next investigation |
|---|---|---|---|
| `string-join(index-of(('A','a'),'A',()),',')` | `1` using the default codepoint collation | XPTY0004 | IndexOf_3 requires a nonempty string instead of applying the draft's default-on-empty. Test positional/reference/keyword calls and3.1 differences separately. |
| `deep-equal('A','a',map{'collation':'http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive'})` | `true` | FOTY0013 | DeepEqual_3 treats the third argument as a legacy collation and atomizes the map. Full options grammar/defaults/validation is larger than this minimal reproducer and is not certified. |

The existing compare keyword drift (`comparand1`/`comparand2` versus draft `value1`/`value2`) and absent distinct-values/deep-equal keyword metadata are refreshed observations, not newly implemented contracts. No broad options or error-eagerness conclusion follows from these two examples. The manifest records current observations with explicit `normative40Expected` and `reviewStatus` fields; a passing observed-gap case does not mean the draft behavior is supported.
