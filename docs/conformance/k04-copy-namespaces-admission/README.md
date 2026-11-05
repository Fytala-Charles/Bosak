# K04-C32 - Exact copy-namespaces admission review

28 October 2026. Review complete against `cbd0d698`. **The exact 49-case admission is qualified and awaiting owner approval.** Production source, frozen baselines and version remain unchanged. Candidate **1.31.0** remains unpublished; public **1.26.3**.

## Complete inventory

Both normal and copied-prototype harnesses independently parse all **31,821 QT3 cases / 428 sets** using the real expression reader, including external query files, inherited/own dependencies and module mappings. The real comment/literal normalizers and pragma predicate are reused. Each actual assembly predicate is checked against its corresponding computed predicate; all expression identities and dependency decisions agree across inventories.

Removing only copy-namespaces from the prolog refusal changes **49 predicates** across five sets:

| Set | Changed predicates |
|---|---|
| prod-CopyNamespacesDecl | 33 |
| prod-CompNamespaceConstructor | 9 |
| prod-DirElemContent | 4 |
| prod-DirElemConstructor | 2 |
| misc-CombinedErrorCodes | 1 |

These are exactly C31's 49 qualified originals. Every affected case has XQuery-admissible dependencies. All **87 construction/mixed-mode predicates** retain their refusals, the **43 admitted context predicates** remain admitted, and the independent exact-XQ10 exclusions remain unchanged. Construction, pragma, environment, schema, comparison and every other policy retain their current source.

## Fresh bounded verification

| Check | Passed / failed / skipped |
|---|---|
| Complete normal QT3 gate | **31,208 / 0 / 613** |
| Complete isolated copy-only QT3 gate | **31,257 / 0 / 564** |
| Complete normal qt4 gate | **8,939 / 0 / 1,920** |
| All 49 originals, direct normal executor | **2 / 0 / 47** |
| Same originals, direct isolated executor | **49 / 0 / 0** |

Every retained QT3 skip name/reason is unchanged: exactly 49 removals, no additions and **564** retained entries. Both normal maps match C29's approved adoption exactly. qt4 retains **230** gated sets and **11** experimental passes. The copied complete harness changes only the copy-namespaces regex alternative and a diagnostic history row; all engine DLLs match normal binaries byte-for-byte.

The two normal direct passes are K-CopyNamespacesProlog-4 and K-CopyNamespacesProlog-5, whose original assertions accept XPST0003. Direct execution bypasses the runner's dependency-admission step; the full normal gate still excludes all 49 cases with Unsupported dependency. The isolated full gate admits and passes all of them without changing an assertion.

All **37 lexical controls** pass against both independently loaded harnesses: the four copy-mode spellings, nested comments/whitespace, strings/doubled quotes, keyword boundaries, context declarations, retained construction and pragma refusals. The existing case-insensitive/conservative routing heuristic also recognizes uppercase or malformed keyword boundaries; grammar validity remains the parser's decision. This review does not introduce uppercase XQuery grammar.

All **152 final bounded diagnostic processes** finish with exit zero and no timeout: 147 original/resource probes, three full gates and two catalog inventories. Three generated-project build logs are separate. Prior C31 evidence and fixture identities are preserved unchanged. C31's **24,010 solution tests** and **76 focused tests** are inherited evidence, not fresh unit-test runs in this read-only review. Normal source and compiled engine identities still match the delivered implementation.

## Adoption boundary

The [exact two-file proposal](BASELINE-UPDATE-PROPOSAL.md) supplies the proposed TestExecutor/AGENTS snapshots, patch and identities. Approval is required because AGENTS.md currently mandates QT3 31,208/0/613. Adoption must rebuild and rerun complete normal gates and solution tests after approval. No release or version change is included.

Construction type-annotation modes and their namespace-sensitive typed-value interactions remain separate follow-ups. Bosak.Schema retains XSD 1.1 ownership. REQ-126 remains Complete under bounded owner acceptance; original dispositions and all 48 historical unmet observations remain recorded. No upstream submission, publication, push, tag or Braid change is included.

## Reproduction

With the pinned source/fixtures, run `prepare-review.py` and `prepare-inventory.py`; build their generated Release projects under `.guard-tmp/k04-copy-admission-harness`, `k04-copy-admission-driver` and `k04-copy-admission-inventory`. Run `run-inventory.py`, `run-originals.py` and `run-gates.py`. The scripts locate the repository root themselves. Use `validate-evidence.py` for the preserved receipt set, exact source isolation and manifest reconciliation; regenerating receipts requires regenerating their raw identities.
