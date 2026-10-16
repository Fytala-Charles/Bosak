# Current record field contract audit

Milestone1 of the three-PR plan closes the audit without reintroducing obsolete syntax. Source remains unpublished1.16.1; no engine implementation or package version changes.

The pinned [nominative record section](https://qt4cg.org/specifications/xslt-40/Overview.html#named-record-types) records issue1979/PR2566 (31March2026): fields cannot be optional, and absent values use an empty sequence. Its editorial note explicitly calls for removing xsl:field/@required from the schema and adding static. The current field contract covers name, as, initializer and static. The engine's draft XTSE0090 rejection of required is therefore correct. Upstream record-023 still uses required='no' with recursive fields and an earlier pattern spelling; its expected successful result is not a reason to restore retired syntax.

Thirteen permanent independent controls verify six retired boolean spellings are rejected, current empty-sequence values keep the map field present, static initializers work, malformed/missing static initialization errors remain, foreign metadata is ignored, and future-version suppression is retained. All71 focused tests pass (13 new plus existing record controls), without fixture corrections. Original source is retained under .artifacts/original-tests.txt.

Fresh unchanged-upstream record-023 selections fail in both profiles: Legacy result mismatch, Draft XTSE0090 on required; each selection0pass/1fail/64skip, exit1. Full-family native receipts from PR346 remain authoritative (not fresh audit runs): Legacy array155/5/20, record29/35/1, type-alias23/8/1; Draft array158/2/20, record29/35/1, type-alias23/8/1. No skip, expected-error alias, fixture rewrite or gate promotion.

PR346 exact-head CI38944375488 passed at38a5ee6eb90fedc955eeb20014a440d793e0e522 and merged at5be7d95b6f264c63ab234dd7c115a5133f92e5fc. No upstream submission is authorized or prepared; an eventual contribution must refresh its upstream default branch and keep original receipts.

Remaining milestone: implement earlier-module fixed binding activation under the draft profile, then unknown-future/no-fallback handling. Stop after those two PRs and reassess. Record construction, field semantics beyond this audit, schema-role, nested transform/authoring profiles and publication remain outside this milestone.


All processes completed: Release build0errors/2 existing warnings; solution **9439/0/0** (XSLT6505), language-server **84/0/0**, ten CLI controls pass. No engine change: prior PR346 QT3 31,142/0/679 and qt4 gated8,278/0/1,894 receipts retained; not rerun in this audit. Verified fixture links removed; canonical documents synchronized and rendered.

Refreshed suite91328fbf0fed4a5d2813eb3661808ee53cb961ba; qtspecs72f222929768b411d8e88d96a1a8740483290078; xslt.xml blob59b9bdc79dca23b0c18fdd3e94130bc59037db3e. No upstream edits, aliases, skips or gate promotions. Source remains unpublished; public versions and independent acceptance unchanged.
