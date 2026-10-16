<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak string-key map pattern support">
  <br><br><h1>XSLT 4.0 string-key map patterns</h1>
  <p>Bounded current-draft matching and unchanged native-suite evidence</p>
</div>

## Contract — 16 October 2026

Unpublished source candidate **1.9.0** adds template map patterns for string-key presence. Public libraries remain **1.8.0** and VS Code **0.2.4** continues to bundle 1.8.0. No public API, publication, release tag, consumer adoption, gate membership or REQ status changed.

Supported forms are `{}`, `{*}`, `{'first','last'}` and `{'first','last',*}`, with an optional `map` keyword. Empty patterns require an empty map; a closed key list requires exactly those distinct keys; a trailing wildcard allows other entries. Values may be empty or contain multiple items because this slice tests presence only. Duplicate key requirements are permitted. String matching uses map key equality, independent of default collation. Maps and annotated records both qualify; other items do not. No record annotation or input map is modified.

Default priorities are +0.5 for `{}`, -0.25 for `{*}`, and 0 for nonempty string-key lists, including lists ending in a wildcard. Existing explicit priority and next-match dispatch remain applicable. Parentheses introduce a node pattern under the current 4.0 rules; a parenthesized map pattern therefore cannot match a map. Bare map patterns are rejected as union/intersect/except operands; enclosing them in a node pattern retains the node-only constraint.

String literals support doubled quote escapes, Unicode and delimiter characters. XPath comments and whitespace are handled before parsing. Earlier effective profiles, including a locally 3.0 template in a 4.0 stylesheet, reject map syntax with XTSE0340.

The bounded parser intentionally refuses entry-value/sequence constraints, outer predicates and path suffixes, non-string key constants and trailing commas. These refusals are coverage limits, not claims that every refused form is invalid in the draft. Complete map, record, array or atomic pattern support is not claimed. In particular record-011/012 and the new `record(TypeName)` pattern form remain follow-ups. Existing legacy `~type` support is unchanged.

## Consumer example

```xml
<xsl:template match="{'id','label',*}">
  <row id="{?id}"><xsl:value-of select="?label"/></row>
</xsl:template>
```

In a version 4.0 stylesheet this rule matches any map or record containing both named entries. Extra entries are allowed because of the trailing wildcard; entry values are unconstrained. Omit the wildcard to require exactly two distinct keys.

## Draft and implementation review

Review pins `qt4cg/qtspecs` revision `72f222929768b411d8e88d96a1a8740483290078`, `specifications/xslt-40/src/xslt.xml` (blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`). The active [map pattern rules](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13075) and [priorities](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13836) define the reviewed behavior. The former type-pattern section is XML-commented out at line 12275; [record patterns](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13240) now use record(TypeName). Older ~type native cases are not proof that ordinary maps should match current structural/nominative types.

Previously TemplateRule treated map-pattern braces as AVT delimiters. In the 4.0 profile, pattern compilation now owns brace syntax validation. Earlier-profile AVT rejection is unchanged. StringKeyMapPattern parses a complete standalone supported pattern, deduplicates requirements and builds a matcher using direct map key lookups. PatternCompiler validates profile and node-set operand boundaries; TemplateRule uses map-family priorities. Invalid or unsupported forms fail at compile time rather than silently never matching.

## Evidence

Upstream xslt40-test default was refreshed and remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`. The suite worktree stays unchanged, with no contribution branch, catalog edit, changed expectation, skip, error alias or upstream submission. Original [N04/N05 baseline](XSLT40-N04-N05-BASELINE.json) and [draft discrepancy review](XSLT40-RECORD-DRAFT-RELIABILITY.json) remain unchanged.

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| record substring, aggregate | 26 / 38 / 1 | 27 / 37 / 1 |
| record set, within aggregate | 26 / 21 / 0 | 27 / 20 / 0 |
| xsl-record set, within aggregate | 0 / 17 / 1 | 0 / 17 / 1 |
| type-alias | 22 / 9 / 1 | 22 / 9 / 1 |

Only unchanged native record-010 newly passes. Both filtered family runs correctly retain exit 1 because failures remain. The xsl-record-007 schema-awareness skip and inapplicable type-alias-001b version skip remain visible. No family-wide conformance or new gate is claimed.

The initial 34 new regressions on unchanged engine source produced 10 pass / 24 fail. The final fixture adds five negative/profile controls: all 39 pass on the candidate. Coverage includes valid/empty/missing/surplus/repeated keys, non-string extra entries, string-like key equality, quotes and delimiters, structural records, priorities/next-match, non-map input, earlier profiles and static refusal boundaries. No independent processor execution is claimed.

Verification, source/patch/log hashes and original native run receipts are recorded in [machine-readable evidence](XSLT40-STRING-KEY-MAP-PATTERNS.json). Next bounded slice: map entry-value/sequence constraints and their priorities, after reviewing the current sequence-pattern rules. Keep old ~type expectations, recursive record conversion, draft error-code discrepancies and broader instruction families separate.

## Verification

Release build succeeds with zero errors/22 warnings. Solution 8,664/0/0 (XSLT 5,778); language server 84/0/0; focused fixture 39/0/0 and repeated 39/0/0. Frozen gates are exactly QT3 31,142/0/679 and qt4 gated 8,278/0/1,894; both exit 0. Both native-family counts repeat identically. Canonical docs rendered with markdown-it and Fytala CSS; AGENTS.md conventions remain applicable. Original logs, receipts and source hashes remain separate. No local test job needs to survive restart.
