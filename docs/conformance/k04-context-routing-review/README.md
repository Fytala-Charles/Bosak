# K04-C29 - Complete context-routing admission review

28 October 2026. Review complete against `68ae2bc3`. **The exact 43-case adoption is qualified and awaiting owner approval.** Production source, frozen baselines and package versions remain unchanged. Candidate **1.30.1** remains unpublished; public **1.26.3**.

## Complete inventory and rehearsal

The inventory parses every catalog case with the real harness `TestCase.FromElement` reader, including file-backed expressions, inherited/own dependencies and module mappings. Both normal and copied-prototype assemblies are independently loaded. The real lexical normalizers and pragma predicate are reused; the only proposed predicate change removes `context` from the unsupported-prolog alternatives. Each assembly's actual routing decision is checked against its corresponding independently computed predicate for all **31,821 cases in 428 sets**. Expression hashes and dependency decisions agree across the two inventories.

Exactly **43 predicates** change: **40** in `prod-ContextItemDecl`, and **three** in `prod-VarDefaultValue` (`extvardef-015`, `extvardef-016a`, `extvardef-016b`). All have XQuery-admissible dependencies and currently have **Unsupported dependency** skips in the full normal runner. No other catalog predicate changes.

| Fresh check | Passed / failed / skipped |
|---|---|
| Full normal QT3 gate | **31,165 / 0 / 656** |
| Full copied-prototype QT3 gate | **31,208 / 0 / 613** |
| Full normal qt4 gate | **8,939 / 0 / 1,920** |
| All 43 affected originals, direct normal executor | **1 / 0 / 42** |
| Same originals, direct prototype executor | **43 / 0 / 0** |

The direct normal executor bypasses the runner's dependency admission step. Its lone pass is `contextDecl-023`, whose original assertion expects XPST0003; the XPath parser supplies that error. This does **not** mean the full normal gate already admits it: the full runner excludes it before execution through the dependency check. All 43 full-run skip removals are reconciled individually.

The exact [skip delta](evidence/skip-delta.json) contains no added skips and preserves all **613** retained names and reasons. The normal QT3 and qt4 skip maps match the approved C23 adoption maps exactly. qt4 retains **230** gated sets and **11** experimental passes. Thirty lexical controls pass against both harnesses, preserving unrelated construction/copy-namespaces and pragma guards, quoted strings/doubled quotes, nested comments, whitespace/case and keyword boundaries. The existing conservative regex boundary also matches malformed `context-item`; grammar validation remains the parser's responsibility.

All **134 final receipt-listed diagnostic processes** finish successfully without timeout: 129 original/resource probes, three full gates and two full-catalog inventories. Three build logs are retained separately. The complete prototype copies every harness source file, changing only the context alternative and its diagnostic history row. All production dependency filters, KnownSkips, GatedSets, fixtures, assertions and schema initialization policy are unchanged. Engine DLL identities match the normal binaries. The 487 pinned fixture/resource files include the catalog, all set files, external query files and prior resource witnesses. Prior C28 evidence is preserved byte-for-byte.

C28's **23,934** solution passes and **112** focused passes are inherited evidence, not fresh unit-test runs in this read-only review. Fresh full conformance runs qualify the proposed harness change. No engine implementation is included. Bosak.Schema retains XSD 1.1 ownership; REQ-126 remains Complete under bounded acceptance and retains its 48 historical unmet observations.

## Adoption boundary

The [baseline proposal](BASELINE-UPDATE-PROPOSAL.md) supplies the exact two-file patch and proposed source snapshots. Owner approval is required because AGENTS.md mandates the current exact QT3 totals. After approval, adoption must run fresh normal gates and the full solution before completion. No version or release change is included.

## Reproduction

From the repository root, run `python docs/conformance/k04-context-routing-review/evidence/prepare-review.py` and `python docs/conformance/k04-context-routing-review/evidence/prepare-inventory.py`. Build the three generated `Probe.csproj` projects under `.guard-tmp/k04-context-routing-harness`, `k04-context-routing-driver` and `k04-context-routing-inventory` in Release. Then run `run-inventory.py`, `run-originals.py` and `run-gates.py` from the evidence folder. These scripts run with the repository root as their working directory. The frozen fixture revisions and source identities must match [review pin](evidence/REVIEW-PIN.json). Run `validate-evidence.py` to reconcile the preserved receipt set and manifest; regenerating receipts requires regenerating their raw identities.
