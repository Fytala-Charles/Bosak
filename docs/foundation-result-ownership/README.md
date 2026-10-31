# F140-01 — Lazy and escaped result ownership

30 October 2026. [Proposed disposition and source trace](DECISION.md) complete; **joint ownership contract/API freeze still pending**. No production implementation, package pin or external owner work changed.

The review recommends retaining existing borrowed-result contracts and explicit provider/host resource ownership, while reserving an optional scope design until actual consumer requirements justify it. Ten isolated in-memory observations confirm that generic sequence enumeration supplies no automatic early-release protocol. A throwing streaming completion callback also reproduces a core-owned reader cleanup gap for a focused follow-up fix.

## Evidence and reproduce

- [Exact source pin and inspected file hashes](evidence/source-pins.json)
- [Ten bounded observations](evidence/observations.json)
- [Validation receipt](evidence/validation.json)
- [Previous member/case draft](../foundation-api-contract-draft/README.md)

From the repository root:

```powershell
dotnet run --project docs/foundation-result-ownership/tools/Ownership.csproj -c Release -- .guard-tmp/f140-ownership-current.json
python docs/foundation-api-baseline/tools/verify.py
```

Write new observations to a separate path; retain the original receipt. The probe uses public APIs and owned memory-only XML/streams with external resources prohibited. Its optional-disposable custom sequence is a protocol observation, not a real consumer. Caller cleanup closes fixtures after the observation boundary. No full engine/consumer suites or conformance gates were rerun in this step; a future engine fix requires them.

Next core task: fix exception-safe owned-reader completion with focused tests and explicit failure precedence, independently of external owner work. Broader lazy/escaped-result ownership, cancellation and compatible scope design remain under FG-04/06 and required consumer qualification.
