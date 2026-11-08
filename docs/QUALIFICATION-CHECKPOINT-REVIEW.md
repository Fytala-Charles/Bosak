# Qualification checkpoint and execution-review handoff

Current supplement, 8 November 2026: the execution restriction described below is historical and has been resolved by fresh automatic review. [The follow-up](foundation-xmldb-manual-approval/README.md) records Integration862 plus one layout-corrected focused pass, giving 4,482 distinct XmlDb passes across runs. [The current release checklist](RELEASE-READINESS-1.81.1.md) lists remaining owner, scope and delivery gates. The original checkpoint narrative below is preserved.

The owner authorized committing and pushing the completed qualification evidence
on `checkpoint/foundation-1.81.1`. The code/package candidate remains
`c0b926de34824ceb2010d5f814a4450b28b31dcc` and local preview
**1.81.1-preview.c0b926de**. This subsequent evidence commit does not relabel the
package artifacts with a new source revision or select a public release version.

Concrete completed work:

- [Preview artifacts](foundation-preview-packages/README.md): nine packages,
  fresh-cache package-only consumer6 and unchanged retained caller20; assets,
  dependency closure and package/cache/output/loaded hashes verified.
- [Package regression qualification](foundation-preview-qualification/README.md):
  solution26,342, language-server84, native451, adapter59, consumer10; frozen
  QT3/qt4 totals and exact skip maps; API318 types/6,465 rows.
- [Exact-package candidate workload](foundation-package-workload/README.md):
  six provider sessions/30 measured cycles/135 reads;14 method controls and
  candidate-as-baseline refusal. No accepted baseline or owner budget.
- [Owner repository inspection](foundation-owner-repo-review/README.md) and
  [draft workload proposals](foundation-owner-workload-draft/README.md): historical
  scoped design returns remain distinct from current package acceptance.
- [XmlDb source and full-run follow-up](foundation-xmldb-full-qualification/README.md):
  source builds without migration edits; full Storage3,572 plus reused XDM47 and
  in-process Integration69 give **3,688 distinct verified cases**. Original setup
  failures and cancelled runs remain historical evidence, not passing totals.

Focused accidental-disclosure screening of the 277 new files found no strong
credential patterns, sensitive archive paths, proprietary XmlDb DLL/PDB/EXE files
inside archives, or files at least50 MiB. This is targeted screening, not a full
security audit. Proprietary owner source/binaries remain in the ignored workspace.
Exact-path Git attributes preserve receipt bytes. Existing large sealed archives
retain their original Git LFS treatment.
An exact ignore exception includes the manifested synthetic checker log. The
two new frozen skip-map files retain their terminal blank lines verbatim; their
exact attributes suppress only that expected end-of-file whitespace diagnostic.

## Execution restriction requiring environment review

The owner explicitly approved the Storage sweep and Integration intentional
child-process kill/fault probes. [Authorization](foundation-xmldb-full-qualification/AUTHORIZATION.json)
records that instruction. It must not be confused with missing user consent,
owner performance acceptance or release approval.

Automatic approval review still rejected the full Integration launch after that
approval. Its stated reason was that intentional child-process termination/fault
behavior conflicted with the automation discipline and the caught-failure entry
did not remove that behavior. The [restriction receipt](foundation-xmldb-full-qualification/INTEGRATION-RESTRICTION.json)
records the rejection; the method was not launched again or routed around.

The applicable repository instruction is in [AGENTS.md](../AGENTS.md), Automation
crash discipline: "Do not intentionally launch unhandled .NET negative cases;
exercise refusal with a mocked launcher or catch failures inside the diagnostic."
It also forbids machine-wide Windows Error Reporting changes. The isolated
diagnostic dispatcher has a caught-exception entry guard; expected child
termination at test-owned persistence checkpoints remains intentional. No
machine-wide setting was changed.

The concrete question for an authorized environment/policy reviewer is whether
that guarded, test-owned termination/fault method is permitted under the applicable
execution policy, or whether the policy requires a different approved execution
environment. This document does not answer that question, amend AGENTS.md, grant
an exception or provide a bypass. No external review request or owner message
has been sent.

Until resolved, **794 Integration cases remain unqualified**. Passing in-process
or mocked checks cannot substitute for actual process/OS resource behavior. Do
not retry the rejected method merely on another general continuation instruction.

## Remaining delivery gates

The [XmlDb diagnostic identity note](foundation-xmldb-full-qualification/evidence/OWNER-IDENTITY-NOTE.json)
records parent-Core informational stamps. Explicit source-export/binary hashes
identify the observed XmlDb cohort; correctly stamped owner delivery artifacts
need a separate build and qualification. The current receipts cannot be relabeled.

Actual production-provider adoption, representative owner workloads/comparable
baselines/numerical budgets, four-owner C45/API freeze, final release SemVer and
signing/provenance review remain open. No release tag or publication is authorized
by this checkpoint. Next: resolve the execution restriction, qualify any resulting
new cohort, and obtain the explicit owner decisions before release preparation
can be accepted as complete.
