<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.7 release notes">
  <h1>Bosak 1.23.7 release notes</h1>
  <p>23 October 2026</p>
</div>

**Published and verified.** Immutable v1.23.7 source: `9dd5096104b9d08bfad2dd88130f04ed0c3807f8`. [Public delivery proof](RELEASE-1.23.7-PUBLICATION.md). No separate1.23.5/1.23.6 release was made.

Bosak 1.23.7 contains three bounded corrections since 1.23.4:

- **EQName brackets in XSLT patterns:** `Q{urn:n[part]}root` treats the square bracket inside the namespace literal as URI content. Actual predicates after the closing brace retain their existing behavior. Element/attribute/axis/path/kind/wildcard and predicate cases are covered in effective 3.0/4.0. Effective4.0 expanded-name default priority also ignores URI brackets, preserving explicit priorities. Earlier-profile default-priority classification is unchanged. [PR #409](https://github.com/Fytala-Charles/Bosak/pull/409), [finding and46 regressions](xpath40-catalog/EQNAME-BRACKET-PARSING.md).
- **Inline record-signature version gate:** expressions such as `function($r as record(a)) { $r }` now refuse with XPST0003 in XPath 3.0/3.1, including nested map/array/function parameter and return signatures. Frozen/experimental 4.0 support and ordinary element/attribute/PI names `record` remain intact. Programs relying on this accidental acceptance in old profiles must select 4.0 or use a compatible signature. This does not certify every inline type gate. [PR #411](https://github.com/Fytala-Charles/Bosak/pull/411), [44 controls and finding](xpath40-catalog/INLINE-RECORD-TYPE-GATE.md).
- **Numeric digit-group boundaries:** XPath 4.0 rejects leading/trailing underscores in fractional/exponent digit groups, including `1._2`, `1.2_` and `1e_2`, with XPST0003. Repeated internal separators such as `1__2.3__4` remain valid. Earlier profiles are unchanged. Numeric string constructors retain XML Schema lexical rules: source literals do not make `xs:integer('1_000')` valid. [PR #412](https://github.com/Fytala-Charles/Bosak/pull/412), [50 catalog examples and34 API checks](xpath40-catalog/NUMERIC-LITERALS.md).

No public API is added. The corrections retain Bosak's permissive namespace-literal policy and bounded .NET numeric representations; they do not provide arbitrary precision, exhaustive pattern grammar or full XPath/XSLT4.0 conformance. General numeric adjacency/range/Unicode, nominal/static/default fields, other inline gates and broader host/declaration/package contexts remain separate.

The catalog also gains focus-function syntax, record contracts and numeric literal references with complete paired business reports. Current totals: **74 entries;2,869 feature +24 chapter checks =2,893**, four worked chapters and 117 assigned function names. The owner-approved applicable-evidence policy governs AC-03; REQ-126, D2/D3/D4 and AC-03 remain partial, with 28 historical adoption pins missing. Current draft comparison pins do not recover historical adoption.

Verification: exact-merge build,10,720 solution tests,84 additional language-server CI tests, and fresh unsigned-package consumers with 153 broad +394 focused checks. Focused checks overlap source coverage. QT3 stays31,142/0/679 and qt4 gated stays8,278/0/1,894; no fixtures or gate membership change. Ten candidate packages have matching version/source/dependencies, assembly/archive/cache/output identities, XML docs and README/license/icon/branding. Both consumer transitive audits report no vulnerable packages. [Readiness and publication plan](RELEASE-1.23.7-READINESS.md).

The local unsigned candidate proofs are supplemented by ten verified signed public packages and fresh nuget.org-only153 broad/394 focused checks with matching archive/cache/output/contentHash provenance and clear audits. Braid's latest recorded native acceptance is 1.23.0; its 1.23.4 upgrade smoke is separate. This preparation neither upgrades Braid nor claims new native acceptance. The VS Code extension has its own release cycle.
