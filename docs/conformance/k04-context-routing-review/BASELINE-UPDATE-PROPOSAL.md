# K04-C29 - Exact context-routing baseline proposal

28 October 2026. **Owner approval pending.** This proposal qualifies admission; it does not apply it.

Approve the [exact two-file patch](PROPOSED-ADOPTION.patch): remove only `context` from the QT3 unsupported-prolog alternatives, append its source history row, and update AGENTS.md's frozen QT3 expectation from **31,165/0/656** to **31,208/0/613**, with the adoption provenance sentence. The copied proposed file text and base/patch identities are preserved in [proposal identities](evidence/proposal-identities.json).

The independent full-catalog inventory covers **31,821 cases in 428 sets**. Exactly 43 predicates change: 40 cases in `prod-ContextItemDecl` and `extvardef-015`, `extvardef-016a`, `extvardef-016b` in `prod-VarDefaultValue`. All three additional variable-default assertions pass without modification. All 43 cases currently have **Unsupported dependency** skips in the normal gate and pass in the isolated complete gate. Their exact names are in [skip delta](evidence/skip-delta.json).

The complete isolated gate is **31,208/0/613**; every retained skip name and reason is identical. Fresh normal gates remain **31,165/0/656** and **8,939/0/1,920** (230 qt4 gated sets, 11 experimental passes). All 43 original assertions pass directly under the prototype. Thirty lexical controls pass under both independently loaded harnesses: quoted/commented text, doubled quotes, nested comments, keyword spacing/case, keyword boundaries, retained construction/copy-namespaces refusals and pragma refusals. The existing conservative regex boundary also matches malformed `context-item`; the parser still decides grammar validity.

The final source patch has the same predicate as the complete isolated rehearsal; only its history row describes adoption instead of diagnostics. It changes no engine, schema backend, fixture, assertion, dependency filter, KnownSkips, GatedSets, qt4 runner, version, publication or release. Candidate 1.30.1 remains unpublished; public 1.26.3. Bosak.Schema retains XSD 1.1 ownership.

**Why approval is required:** [AGENTS.md](../../../AGENTS.md) currently says the frozen gate ?must stay exactly **31,165/0/656**?. The proposed baseline changes that explicit requirement; a review slice cannot adopt it automatically.

After explicit approval, apply only these two files, synchronize the canonical adoption notices, rebuild, run all solution tests and fresh complete normal QT3/qt4 gates, and verify exactly these 43 skip removals with every retained reason unchanged. If any expected result differs, stop adoption and preserve the failure evidence. No release is authorized.
