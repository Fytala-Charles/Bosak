# K04-C29 - Owner-approved context-routing adoption

28 October 2026. **The exact 43-case context-routing proposal is adopted.** The owner selected `Approve exact 43-case adoption (Recommended)` for proposal commit **1daa687c**. The [approval receipt](approval.json) records that decision; the original proposal, patch, review receipts and identities remain byte-for-byte preserved separately from adoption evidence.

The normal QT3 runner now removes only `context` from its unsupported-prolog alternatives and appends history row 0.29. Its construction, copy-namespaces and pragma guards remain intact. AGENTS.md now requires **31,208/0/613** and records this adoption's provenance. Both production files match the owner-approved proposed snapshots exactly. No other production source changes.

## Fresh normal verification

| Check | Outcome |
|---|---|
| Complete solution build | Passed, zero errors |
| Complete solution tests | **23,934 passed**, ten projects |
| Complete normal QT3 gate | **31,208 / 0 / 613** |
| Complete normal qt4 gate | **8,939 / 0 / 1,920** |
| Rebuilt normal routing inventory | All **31,821 cases / 428 sets** match the isolated rehearsal |
| Lexical guard controls | All **30** match the isolated rehearsal |

Exactly **43** former Unsupported dependency skips leave QT3: 40 context-declaration cases and `extvardef-015`, `extvardef-016a`, `extvardef-016b`. Every retained QT3 skip name and reason is unchanged; the entire normal skip map equals the qualified prototype. All qt4 skip names/reasons remain unchanged, with **230** gated sets and **11** experimental passes. The adopted inventory also preserves all 43 original query/assertion snapshots. The earlier direct-executor parse-error pass for contextDecl-023 remains distinct from the normal runner's dependency admission, as explained in the original review.

Solution totals include **535 XQuery**, **13,797 Standard** and **8,196 XSLT** tests. All solution projects finish successfully; no Application Control workaround is needed. The adoption changes harness policy only: frozen fixture revisions/resources remain unchanged. Rebuilding from the newer proposal commit changes DLL hashes through the assembly informational-version source stamp (`93c564da` to `1daa687c`). Read-only PE comparison confirms that all nine engine assemblies retain their method bodies, type/field/method signatures and assembly versions. Fresh binary hashes and the comparison are recorded separately; binary equality is not claimed. [Validation](validate-adoption.py), [source identities](adopted-source-identities.json), [skip reconciliation](skip-verification.json) and the manifest support the exact adopted scope.

Candidate **1.30.1** remains unpublished; public **1.26.3**. No engine, dependency filter, KnownSkips, GatedSets, schema/backend, fixture/assertion, version, release, publication, push, tag, upstream or Braid change is included. Bosak.Schema retains XSD 1.1 ownership. REQ-126 remains Complete under bounded owner acceptance; its original dispositions and 48 historical unmet observations remain recorded. Earlier pending-proposal notices are historical and are superseded by this adoption record.

Next: a separate read-only review of the remaining unsupported-prolog skip cohorts, including construction and copy-namespaces. Establish their actual parser/runtime boundaries and unchanged original assertions before proposing any semantic implementation or additional baseline change.
