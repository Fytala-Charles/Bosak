# Local Schema dependency checkpoint

9 November 2026. **Schema commit9d1881ef7ccc52e9c756e864e984a6211ae514cc** is local on `feature/core-1.81.2-adoption`; the worktree is clean. It contains the five dependency edits and candidate documentation, with no C# product/test adaptation. [Exact commit patch](SCHEMA-COMMIT.patch).

A fresh build of this immutable commit has zero warnings/errors. All124 existing Schema solution cases and11 additional integration/ownership cases pass with unchanged case catalogs. Eighteen fresh output/DLL comparisons match the approved published1.81.2 packages. [Results](RESULTS.json), [raw logs/TRX and retained binaries](EXECUTION-RECEIPTS.zip).

The earlier124-case unchanged-binary lane is reused only because its retained Schema/test/configuration and nine replacement Core DLL bytes are unchanged. It is not counted as a new rerun. The original pre-commit receipts remain intact; their README and manifest snapshots are preserved here for provenance.

No Schema push, merge, tag or publication occurred. The original checkout, its uncommitted review documents and native assessment worktree are untouched. Remote delivery requires explicit direction under the Schema repository's Git rules. Full XSD1.1 structure, native provider/session/lease adoption and accepted workload budgets remain separate.
