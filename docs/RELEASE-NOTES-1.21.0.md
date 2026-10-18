<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.21.0 release notes">
<h1>Bosak 1.21.0 release notes</h1>
<p>18 October 2026</p>
</div>

**Published and verified:** immutable `v1.21.0` at `8929a1301d1846bb24b49bcc505877f05fd3395a`.

Bosak 1.21.0 adds reviewed, source-preserving replacement of one existing paired ordinary literal `xsl:text` body (REQ-133).

- Separate snapshot-owned target/proposal and capability, with exclusive Changed, NoChange and Refused outcomes.
- Exact logical Unicode, including empty text, with one complete body edit and unchanged tags, attributes and outside bytes. Equal logical intent preserves original lexical spelling without a candidate or history action.
- Actual edited instruction/parent/payload evidence and complete disjoint survivor/replaced/new node partitions, including empty transitions.
- Preserved whitespace and significant surrounding text within the bounded static profile; strict UTF-8 with/without BOM and BOM-marked UTF-16 LE/BE, with independent original/final 8,388,608-byte bounds and replacement shrinkage.

Self-closing expansion, CDATA/comment/PI/mixed bodies, dynamic text-value templates, output-escaping/control edits and generic subtree editing remain outside this operation. Existing authoring operations are unchanged. Consumers need a compatible replacement-specific adapter; insertion validators cannot assume positive replacement length or a total old-node map.

Source verification: 9,900 solution tests, 84 language-server tests, 76 new source cases (63 public / 13 internal), 437 isolated package-public checks and 153 broad checks. Frozen QT3 31,142/0/679 and qt4 gated 8,278/0/1,894. Counts overlap. Braid independently reports 76 focused source cases and 460 local-package checks (437 imported plus 23 probes), with no blocking implementation finding.

This release publishes the ten NuGet libraries at the reviewed source commit `8929a1301d1846bb24b49bcc505877f05fd3395a`. All ten signed public packages are independently verified: 153 broad checks and 437 focused tests pass from fresh nuget.org-only caches, with exact source/assembly/archive provenance and clear dependency audits. Braid adapter/UI/history/worker/export/reopen and native acceptance remain separate gates; the VS Code extension retains its separate release cycle.

[Public delivery proof](RELEASE-1.21.0-PUBLICATION.md) records signatures, source/package identities, fresh consumers and clear dependency audits.
