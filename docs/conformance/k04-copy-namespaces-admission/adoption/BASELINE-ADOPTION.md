# K04-C32 - Owner-approved copy-namespaces admission

28 October 2026. **The exact 49-case copy-namespaces admission is adopted.** The owner selected `Approve exact 49-case adoption (Recommended)` for proposal commit **451f596a**. The [approval receipt](approval.json) records that decision. The original proposal, patch, snapshots, inventory and qualification receipts are preserved byte-for-byte separately from the adoption record.

The normal QT3 unsupported-prolog predicate now refuses only construction declarations. TestExecutor history row 0.30 records the qualified copy-namespaces admission, and AGENTS.md now requires **31,257/0/564**, with adoption provenance. Both production files match their proposed snapshots exactly. Construction, pragma and all independent dependency guards remain intact.

## Fresh actual normal verification

| Check | Outcome |
|---|---|
| Complete solution build | Passed, zero errors |
| Complete solution tests | **24,010 passed**, ten projects |
| Complete normal QT3 gate | **31,257 / 0 / 564** |
| Complete normal qt4 gate | **8,939 / 0 / 1,920** |
| Rebuilt normal routing inventory | All **31,821 cases / 428 sets** match the qualified rehearsal |
| Independent lexical controls | All **37** match the qualified rehearsal |

Exactly **49** Unsupported dependency skips leave QT3 across five sets: prod-CopyNamespacesDecl (33), prod-CompNamespaceConstructor (9), prod-DirElemContent (4), prod-DirElemConstructor (2) and misc-CombinedErrorCodes (1). All **564** retained skip names/reasons are unchanged, and the adopted complete map equals the isolated rehearsal. Every qt4 skip name/reason is unchanged, retaining **230** gated sets and **11** experimental passes. All 87 construction/mixed predicates retain their refusal; the 43 admitted context predicates remain admitted. The adopted original query/assertion snapshots match the review exactly.

The two former direct-executor parse-error passes were still excluded by the old full runner's dependency admission; this adoption reconciles all 49 full-run skip removals individually. There is no weakened assertion or expanded dependency capability claim. [Skip verification](skip-verification.json), [source identities](adopted-source-identities.json) and [validation](validate-adoption.py) support the exact approved scope.

The full solution includes **611 XQuery**, **13,797 Standard** and **8,196 XSLT** tests. All projects pass without an Application Control workaround. The engine source is unchanged. Rebuilding under the newer source revision changes DLL informational-version stamps and raw hashes; read-only PE comparison confirms identical method bodies, type/field/method signatures and assembly versions across all **nine** engine assemblies. [Binary comparison](binary-comparison.json) and [provenance disposition](binary-provenance-disposition.json) record that distinction explicitly. Normal qt4 source and all frozen fixtures remain unchanged.

Candidate **1.31.0** remains unpublished; public **1.26.3**. No engine, dependency filter, KnownSkips, GatedSets, fixture/assertion, schema/backend, package version, release, publication, push, tag, upstream or Braid change is included. Bosak.Schema retains XSD 1.1 ownership. REQ-126 remains Complete under bounded acceptance; original dispositions and 48 historical unmet observations remain recorded. Earlier pending-proposal notices are historical and superseded by this adoption record.

Next: separately review the construction-mode implementation boundary across the 75 construction-only and 12 mixed-mode originals. Namespace-sensitive typed-value/type-annotation rules require their own immutable specification adoption and schema-capability review; do not remove the retained construction refusal based on copy-namespaces success.
