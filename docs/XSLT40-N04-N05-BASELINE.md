<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak named type and record baseline">
  <br><br><h1>XSLT 4.0 N04/N05 baseline</h1>
  <p>Unchanged upstream tests, adapter correction and bounded next slice</p>
</div>

## Scope — 16 October 2026

PR316 merged at `cdcd517d627a9e8f975ec5b378963692f9ab8f91`. Upstream default `master` was freshly fetched before creating the detached suite worktree: `91328fbf0fed4a5d2813eb3661808ee53cb961ba`. Both initial source trees were clean. No contribution branch, upstream changes, submission, engine changes or publication were made. Bosak runs use the basic XSLT40 adapter profile, retained stylesheet versions and exact error codes.

## Results

| Selection | Initial pass/fail/skip | Corrected adapter pass/fail/skip |
|---|---|---|
| type-alias | 22 / 10 / 0 | 22 / 9 / 1 |
| record substring, aggregate | 26 / 38 / 1 | 26 / 38 / 1 |
| record set only, within aggregate | 26 / 21 / 0 | 26 / 21 / 0 |
| xsl-record set, within aggregate | 0 / 17 / 1 | 0 / 17 / 1 |

Every family run returns exit 1 because real failures remain. No timeout occurred. Other catalog sets are excluded from these totals. The skipped `type-alias-001b` is restricted to XSLT20/XSLT30; it is inapplicable to this profile, not an unsupported feature. `xsl-record-007` is skipped by its dependency. Native logs, counts, actual errors, hashes and original run receipts are preserved in [machine-readable evidence](XSLT40-N04-N05-BASELINE.json). The adapter does not provide a per-case watchdog; bounded filters completed normally, and no timeout-protection improvement is claimed.

## First reliability correction

The adapter previously interpreted `XSLT20 XSLT30` as one unrecognized specification string and executed it in the 4.0 profile. The upstream catalog schema permits whitespace-separated specification tokens. The adapter now evaluates each token as an alternative, retaining exact-version and minimum-version semantics. Eight controls pass: the original four plus excluded/included version lists, legacy-profile inclusion and XML whitespace/minimum-version boundaries. This corrects selection only; no failed result was converted into a pass and no assertion or engine error alias was changed.

The original baseline remains separate from corrected dirty-tree runs. Exact patch/source hashes accompany those receipts; a dirty run is not attributed to a clean committed revision.

## Failure triage

These are observed clusters and review candidates, not blanket normative defect findings:

| Cases | Observation | Classification / next review |
|---|---|---|
| type-alias-003/004 | Expected SXTA0001; actual XPST0051 / XTSE4035 | Upstream/draft error-code alignment question; retain expectations and review current draft before correction. |
| type-alias-007/014; record-020 | Different error codes | Error-code or unsupported syntax boundary; exact matching retained. |
| type-alias-006 | All map instance-of answers false | Structural record/map matching candidate. |
| type-alias-012; record-004/009 | Optional-field record syntax rejected | Parser coverage gap candidate. |
| type-alias-010/013; record-010/011/012/023 | Pattern rejection or built-in traversal instead of expected templates | Record/named-type pattern coverage candidate. |
| record-003/005/016 | Additional map field rejected | Small conversion/matching cluster selected for the next engine review. |
| record-100/101/104/105/110/111/112/113/120/121/123 | Function subtype checks return false | Wider structural subtyping candidate; keep separate from first conversion slice. |
| type-alias-050 | JSON assertion mismatch | Inspect assertion evaluation and actual XDM result before attributing to engine. |
| xsl-record family | Empty construction/cardinality errors or XTSE0090 | Separate instruction/attribute coverage; not a named-record conformance count. |

## Next engine slice

Review current pinned XPath/XSLT draft clauses and the existing coercion/matching code for the extra-field behavior demonstrated by `record-003/005/016`. Distinguish structural map matching, coercion and nominative identity. Derive happy-path and edge/error regressions from the reviewed clauses before implementing. Repeat the unchanged cases and both families; require solution/LS checks and frozen QT3 31,142/0/679 plus qt4 gated 8,278/0/1,894 after engine changes. This baseline does not authorize weakening tests, gate promotion or upstream submission.

The one-millisecond authoring cancellation test is a separate follow-up: `Task.Run` plus `CancelAfter(1)` does not establish cancellation before completion. Preserve its prior failed/successful CI receipts; deterministic in-flight coverage needs a synchronization seam rather than a longer delay or looser assertion. It is unchanged in this adapter slice.

REQ-126 and N04/N05 remain In Progress. REQ-128 engine delivery remains Implemented; libraries 1.8.0, VS Code 0.2.4/engine1.8.0 and independent Braid acceptance are unchanged.

## Verification

Release build: zero errors, 32 warnings. Solution: 8,624/0/0; language server: 84/0/0; all eight adapter controls pass. Repeated corrected native-family counts are identical. Engine gates were not rerun for this adapter-only change. Documentation rendered with markdown-it and canonical Fytala CSS. Initial environment-failed test log and successful rerun remain separate. AGENTS.md build/test/convention guidance remains applicable.
