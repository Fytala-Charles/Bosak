<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.2 release notes">
  <h1>Bosak 1.23.2 release notes</h1>
  <p>21 October 2026</p>
</div>

**Prepared; not published.** Proposed `v1.23.2` source: `7832b785925d2491e0ceac0dd9faa60261619bc4` ([PR #399](https://github.com/Fytala-Charles/Bosak/pull/399)). Public packages remain 1.23.0 until separately approved publication completes.

Bosak 1.23.2 corrects two focused XPath/XSLT behaviors:

- **XSLT switch node identity:** nodes returned through `xsl:when/@select` and `xsl:otherwise/@select` retain their identity inside raw sequence constructors. Template and function paths are covered, with existing tree serialization behavior preserved. This includes the previously unpublished source 1.23.1 fix from [PR #395](https://github.com/Fytala-Charles/Bosak/pull/395); no separate 1.23.1 release is planned.
- **XPath 4.0 empty collation:** `index-of(input, target, ())` uses the evaluation context's default collation in both supported 4.0 modes. Positional, named function, dynamic lookup, partial application and keyword calls agree. XPath 3.1 retains its existing XPTY0004 behavior; explicit collation strings and invalid nonempty arguments retain their existing handling.

The REQ-126 catalog also adds a customer-reference matching example and executable equality/collation coverage: 68 entries and 2,660 checks. The index-of observation is corrected separately from the original evidence. REQ-126 remains In Progress; other recorded equality boundaries are outside this release. No full XSLT 4.0 conformance or upstream test acceptance is claimed.

Source verification covers 10,331 solution tests and 84 separate language-server tests. New regression coverage includes 32 public switch checks, 27 index-of checks and 58 catalog checks; these counts overlap the solution total. Frozen QT3 remains 31,142/0/679 and qt4 gated remains 8,278/0/1,894, as verified for the unchanged reviewed implementation tree.

Exact-merge unsigned packages pass 153 broad public checks and 176 focused checks (117 existing attribute-lifecycle +32 switch +27 index-of). All ten package revisions and archive/cache/output identities match; dependency audits report no vulnerabilities. These local candidates do not certify signed public packages. The existing conformance CS8604 warning remains documented and is not fixed by this release.

REQ-135's completed Braid acceptance applies to public 1.23.0 and remains valid. A future 1.23.2 consumer upgrade smoke check is distinct from that completed acceptance. The VS Code extension follows its own release cycle. See [release readiness](RELEASE-1.23.2-READINESS.md).
