# Host acquisition and admission rollback qualification

1 November 2026. The unchanged **1.42.0** production cohort from `b2ca55fc` passes **57 new controls twice**: 19 scenarios each for actual compiled XPath, XQuery and XSLT. [Basis](SPEC-BASIS.md) records the ownership policy before fixture implementation. This is a test-host qualification, not a new production acquisition API or native owner adapter.

## Qualified ownership transitions

The test host acquires each resource before calling OpenSnapshot and records it immediately in its pending resource slot. A successful admission takes responsibility for the supplied release callback. If admission throws, the host releases the untransferred resource itself. Failure while acquiring source B leaves already-admitted A in the host's rollback scope. The host attempts every cleanup in reverse source order, even if an earlier cleanup fails.

The host awaits the actual raw operation task through evaluation and deferred callback work before closing its admissions. Canceling a caller's wait does not run that cleanup early. Session closure during raw work revokes further guarded use and defers admitted resource release until the real leases end. No provider/store disposal is transferred to the session.

| Scenarios repeated for each host | Qualified result |
|---|---|
| Two-source success | Compiled evaluation returns `A|B`; B then A release through admission callbacks |
| Source B acquisition failure | A releases; B has no returned resource for the host to release |
| Forged assessment, duplicate identity, capacity refusal | B remains caller-owned; A remains admission-owned; work never starts |
| Provider revoked before B admission | Both cleanup paths remain usable despite revocation |
| Cancellation before acquisition, after A admission, after B acquisition, after B admission | Only acquired resources release; transfer state selects the correct cleanup path |
| Work error, work plus both cleanup errors | Original exception object retained first; every cleanup attempted in reverse order |
| Rejected admission plus both cleanup errors | Admission error retained first; nested cleanup AggregateException is not flattened |
| Successful work with one/two cleanup failures | One error rethrows directly; multiple errors aggregate in attempted order |
| Acquirer-local failure plus cleanup failure | The acquirer releases its own partial resource before throwing; host preserves that nested failure and cleans A |
| Canceled wait during raw work | Both resources remain held until raw callback completion |
| Session close during raw work | Guarded access refuses; resource cleanup occurs once after raw work ends |
| Cancellation plus both cleanup failures | Cancellation remains visible alongside all cleanup failures |

Release attempts are counted separately from claimed idempotent releases, so a repeated callback cannot be hidden by the fixture's idempotent wrapper. Rejected-admission callbacks must remain uncalled. Identity, assessment and capacity failures exercise real kernel admissions; successful work and post-evaluation failures use real compiled hosts and operation-stamped inputs.

## Evidence and limits

[First run](evidence/probe-0.json), [repeat](evidence/probe-1.json), [fixture build](evidence/build.txt) and [input hashes](evidence/INPUTS.json) retain exact evidence. The standalone fixture compiles with zero warnings/errors. Its loaded engine hashes match the sealed production cohort. Reflection binds private upper-host factory delegates once at setup; it is absent from acquisition, admission and per-evaluation dispatch. No friend grant or project dependency changed.

The resources, issuer and acquisition faults are synthetic. XML nodes are prepared before acquisition; these cases do not demonstrate real database reader/generation reclamation, native authenticated PSVI, asynchronous I/O acquisition, thread-abort recovery or worker-kill cleanup. A throwing acquirer must reclaim any resource it never returned, and every returned handle must have a clear release contract. The orchestration helper lives only in the fixture; consumers cannot bind to it as a public SDK. No source graph is copied to solve ownership, and no throughput or native resource budget is claimed.

Production source and binaries remain byte-identical to the [preceding full qualification](../foundation-xslt-source-membership/README.md): **24,390 unit passes**, QT3 **31,397/0/424**, gated qt4 **8,941/0/1,918** with 11 experimental passes, and **270 public types / 5,770 declarations**. These are inherited receipts, not new full-suite executions in this fixture-only step. The verifier checks every sealed production source hash and the actual loaded engine hashes. The original composite acceptance list remains sealed; these tests provide bounded evidence for its host-acquisition obligation without declaring full contract or owner acceptance.

Next map these qualified transitions to actual Schema and XmlDb owner capabilities, including acquisition failure atomicity, assessment/snapshot identity and reader/generation release. Use that mapping to identify the smallest remaining public contract gaps before C45 review. Native consumer qualification, broader invocation/typed-result profiles, exact numeric execution and API freeze remain pending. No external owner checkout or request was changed or sent.

Read-only verification: `python -B docs/foundation-host-acquisition-rollback/tools/verify.py`. Negative controls use caught exceptions, scoped child error mode and bounded waits; no machine-wide error setting change or intentional unhandled crash test is used.
