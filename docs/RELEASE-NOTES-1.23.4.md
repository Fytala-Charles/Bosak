<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.4 release notes">
  <h1>Bosak 1.23.4 release notes</h1>
  <p>22 October 2026</p>
</div>

**Published and verified.** Immutable `v1.23.4` source: `2b5ffc5076af0411dee606b010368a36d637863a`. [Public delivery proof](RELEASE-1.23.4-PUBLICATION.md). No separate1.23.3 release was made.

Bosak 1.23.4 corrects two XSLT 4.0 pattern-priority behaviors:

- **Node-union default priority:** effective 4.0 templates use the minimum default priority of their alternatives, including alternatives that do not match the current item. For `<root/>`, `root | missing/child` now has priority 0 rather than 0.5, so a rule with explicit priority 0.25 wins. Both `|` and `union` are covered. This includes the unpublished 1.23.3 correction from [PR #405](https://github.com/Fytala-Charles/Bosak/pull/405).
- **EQName URI punctuation:** name tests such as `Q{urn:n|part}root` and `Q{https://example.test/ns}root` get the expanded-name priority 0. Punctuation within the namespace URI no longer raises them to fallback priority 0.5 in effective 4.0. Forward-axis and attribute paths are covered.

These changes can select a different template when competing rules rely on implicit priorities. Explicit priorities remain authoritative. Earlier-profile behavior, local effective-version selection, 4.0 single-rule union identity and `next-match` deduplication are preserved. Real predicates and paths keep their priorities. Unrelated type-choice and kind-test-name priority calculations are unchanged.

The correction aligns with the explicitly pinned current XSLT draft; historical specification adoption has not been recovered. Opening brackets inside EQName namespace URIs still encounter independently reproduced pattern-parser errors. This release does not claim full XSLT 4.0 conformance or resolve all pattern grammar limitations. See the [union audit](xpath40-catalog/UNION-MINIMUM-PRIORITY.md) and [EQName audit](xpath40-catalog/EQNAME-PATTERN-PRIORITY.md).

REQ-126 documentation now contains 71 catalog entries, 2,743 feature checks plus 24 chapter checks =2,767, with four worked chapters and 117 assigned function names. REQ-126 remains In Progress; D2/D3/D4 and AC-03 remain partial. The catalog's two union observations retain their old 1.23.2 outcomes separately from corrected source expectations.

Verification: 10,470 solution tests and 84 separate language-server tests; 94 focused EQName/union/set-priority checks overlap the solution total. Frozen QT3 remains 31,142/0/679 and qt4 gated remains 8,278/0/1,894. Ten exact-merge unsigned packages pass 153 broad +270 focused package-only checks with matching package provenance and clear dependency audits. Signed public-package proof is now complete: ten signatures, fresh153/270 public consumer checks, matching identities and clear dependency audits.

REQ-135's completed Braid acceptance on public 1.23.0 remains unchanged. A 1.23.4 consumer upgrade smoke check is separate; no Braid package pins change during preparation. The VS Code extension has its own release cycle. See [release readiness](RELEASE-1.23.4-READINESS.md).
