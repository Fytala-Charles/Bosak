# Live owner repository review — 7 November 2026

The owner identified the development repositories as the source of workload,
baseline, budget and adoption decisions. Read-only inspection resolved them to:

| Owner | Actual directory | Current evidence |
|---|---|---|
| Schema | `D:/Development/Bosak.Schema.Public` | November 3 scoped R2 design replies; product packages remain 1.26.3; candidate runtime, budgets and C45/freeze explicitly excluded |
| XmlDb | `D:/Development/Bosal.XmlDb` | Native cleanup/lifetime fix and November 2 scoped design return; Core remains 1.26.2; exact new-Core/XSLT, workloads/budgets and joint acceptance remain separate |
| Braid | `D:/Development/Bosak.Braid` | November 2 scoped capture-design return; product remains 1.26.2; typed/persistent worker routing, exact candidate qualification and budgets remain open |

`Bosak.XmlDb` and `Bosay.Braid` were not present under those exact spellings.
Existing Schema/Braid documentation changes and the clean XmlDb checkout were
preserved. The live XmlDb HEAD differs from the earlier isolated foundation
worktree, so that worktree is not substituted for this new inspection.

[Repository receipt](REPOSITORIES.json) pins current HEADs/statuses, reviewed
documentation hashes and design-return metadata. All Markdown/JSON files under
each current `docs/` tree were searched for `1.81.1` and the exact preview identity;
none mentioned this cohort. This is a bounded documentation review, not proof
that no decision exists elsewhere. No private licensing checkout or credential
material was read, and no external files were modified.

The existing replies provide useful constraints for proposed owner workloads:
typed decimal/list/union/nil/no-value and preserve/strip/foreign controls; persistent
readers across versions; same/distinct roots and source-free invocation; partial
consumption, serialization cancellation/close and timeout/kill/reopen. Braid's
recorded 8 MiB document, 64 MiB frame and 30-second post-launch controls are current
operational limits, **not** accepted PF latency/allocation budgets. Its earlier
snapshot/cache/atom/digit/scale ceilings are explicitly experiments. XmlDb's
native cleanup evidence does not select a new Core package or transport route.

Consequently, the [exact-package owner review](../foundation-package-workload/OWNER-REVIEW.md)
still has nine unfilled workload/baseline/budget slots and outstanding production
adoption decisions. Existing AcceptWithScope replies can be retained as historical
design input; they cannot be relabeled as acceptance of the preview packages.

Next: prepare proposed workload mappings and budget decision fields from these
actual owner constraints, clearly labeled for review, and scope candidate-specific
provider integration work. Numerical budgets and final approvals remain explicit
owner decisions; repeated candidate measurements cannot create them. This review
does not send requests to owners or authorize publication.

Run `python docs/foundation-owner-repo-review/tools/review.py` to re-check the
recorded repository/document state. This is documentation-only inspection; no
owner test or fresh engine qualification is claimed.
