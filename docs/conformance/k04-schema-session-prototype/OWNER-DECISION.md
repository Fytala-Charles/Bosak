# C44 — Core-owner decision requested

**Pending.** Review [API proposal](API-PROPOSAL.md), [bounded implementation plan](IMPLEMENTATION-PLAN.md), [compiled proposed signatures](evidence/api-surface.json) and [prototype results](evidence/prototype-none-results.json).

Approve the **amended schema-session contract direction and C45 guarded core implementation candidate**, with these binding limits:

- Explicit host-opened immutable sessions captured at compile time; no mutable runtime authority setter, namespace-only ownership or auto-discovery.
- Five explicit verdicts, no legacy downgrade after ownership, complete member sets and session-bound node/atomic identity. Keep the six-field XdmValue shape and use a private payload envelope with new static annotation helpers, subject to real production ABI/accessor/performance qualification.
- New neutral contracts remain core-owned and usable by any provider. Bosak.Schema retains XSD 1.1 implementation/license ownership; core product/default gates gain no commercial dependency.
- Any owned operation not yet wired in the candidate must refuse explicitly. Existing legacy APIs, profile policies, exact gates, fixture assertions and historical evidence stay intact.
- Approval permits implementation and review of C45; permanent API freeze requires the completed production candidate checks. Component/assessment integration, licensed provider qualification, skip admission and release remain separate work.

This amends ADR-006's typing-only callback, namespace selection, owned-unsupported fallback and default conformance dependency assumptions. Preserve the original proposed ADR and record any owner disposition separately; do not rewrite sibling documents or mark REQ-139 complete.

The acceptance gate comes from the [exact reviewed ADR-006 snapshot](../k04-schema-advisor-proposal/evidence/schema-context/ADR-006-xsd11-engine-seam-compatibility.md.txt): **“pending owner acceptance in the Bosak core repo before the S4 design freeze”**. Its [S4 preparation snapshot](../k04-schema-advisor-proposal/evidence/schema-context/XSD11_S4_DESIGN_PREP.md.txt) also keeps S4c blocked on that acceptance plus a core release. C44 has completed the authorized isolated prototype and reviewable proposal; it does not itself grant this production/public-API permission.
