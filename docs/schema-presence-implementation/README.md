# Bounded presence assertion implementation — 29 October 2026

Schema 0.9 candidate now admits unprefixed `exists(DeclaredChildName)` and `empty(DeclaredChildName)` as Boolean predicates, standalone or within and/or/grouping. Existing name/decimal/count comparisons remain intact. Complete-parse metadata selects `decimal-sequence-assertions-v5` only for admitted presence calls; count-only v4 and structural v1–v3 remain unchanged. Child names exists/empty are disambiguated by call syntax. There is no public API or core production change.

Native structure and exact decimal validation still precede assertion evaluation. A zero-valued child is present; malformed lexical values are Invalid and unrepresentable decimals are Unsupported even when presence would succeed. Boolean comparisons, prefixed function calls/aliases, arbitrary arguments and wider grammar remain Unsupported. Limits remain 512 characters, 128 tokens, depth sixteen and total occurrence maximum sixteen.

## Qualification

The source-override Release build has **zero warnings/errors**. All **451 complete Schema cases pass**: 54 new presence/grammar/validation/copy cases plus the 397-case legacy suite. The original 49-case cohort moved from 25 pass/24 fail to all pass; five further boundary cases pass. Exactly two historical exists-refusal expectations are explicitly adopted: the count theory row is replaced with a dedicated presence capability fact, and the optional test retains enclosing-group refusal while adopting its separate presence check. The other **395 legacy expectations are unchanged**. Original source and raw before/initial/final TRX receipts are preserved.

The real native example passes with v5, present amount/absent tax, count two, decimal total 15, original later declarations, shared declaration/distinct occurrence states, ambiguous projection refusal and XSLT preserve/strip typing. Regression tests also exercise actual XQuery copies, root FOTY0012 behavior, original state/owner, stale source refusal and independent snapshots.

Core source/configuration/fixtures match qualified 1.34.1. **24,214 core passes**, QT3 **31,397/0/424**, qt4 **8,941/0/1,918** (230 gated sets) are inherited, not freshly run. This does not assert global cleanliness of historical nested fixture checkouts. No release, general XSD 1.1 conformance, schema-session API freeze or C45 acceptance is implied.

## Evidence and next step

The pre-source adoption and exact source locks are retained separately as `ADOPTION.md` and `PRESENCE-SOURCES.json`. Production sources and receipts have byte identities in `MANIFEST.json`. Earlier sealed dossiers remain historical and unmodified. Source base: core 066ee8cc10c1365fe3708c0ea6ab8dd660a8e401, Schema 119f766a9116fe2c5f78048892d467a24e7ce8d5. Current implementation is uncommitted and unpublished.

Next: review this bounded implementation, verify the tested source identities and checkpoint it. Broader function coverage needs its own adoption and plan.
