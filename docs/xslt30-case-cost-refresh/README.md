# Case-cost PR delivery refresh

PR #486 incorporates merged main `2b292e81522d4246f3a546a1bf7528445133f146` after PR #485 passed all six checks and independent qualification. History is preserved by a merge, without rebase or force-push. The case-cost tool, its test, generated-helper template and original CI addition are byte-identical to original head `2ad594cc9f5d021eb2075698e0fb69d041d0c3b0`. Production source, production tests, runner and required gate match merged main.

| Retained attempt | Result | Interpretation |
|---|---|---|
| Original PR486 delivery, 43463788504 | Timed out at 1,200.250 seconds; last started square-array-141 | Failed and retained; earlier original failures remain in the sealed dossier |
| Qualified PR485, 43501163101 | 10,221 / 0 / 4,380 in 848.750 seconds | All 14,601 identities/exclusions replayed; all six checks green before merge |
| Prior main, 43499912773 | 10,221 / 0 / 4,380 in 1,057.812 seconds | Complete independent artifact replay |
| Refreshed PR486 | Required hosted qualification pending | Actual 1.81.9 engine fix inherited from main; original 1,200-second / 8 GiB limits |

The [original case-cost dossier](../xslt30-case-cost/README.md), retained 1.81.8 engine payload, copied caller, source/native pins, complete 14,601-case ledger, 11,224 measured cases, 3,377 pre-loop exclusions and original failed receipts remain unchanged. The tool still refuses arbitrary newer qualified payloads: refreshing the repository does not update that immutable diagnostic baseline or turn old measurements into current-engine results. No new copied sweep or capture is launched. The original API adoption evidence is separate from this delivery receipt.

Fifteen case-cost controls pass. Release workflow validation passes with CI-pinned PyYAML 6.0.3 installed only into the ignored test packet; global Python packages/settings are unchanged. The initial missing-dependency refusal and the scoped correction are recorded in [workflow validation](WORKFLOWS.json), with [output](WORKFLOWS.log) and [controls](CONTROLS.json). Local preparation also corrected UTF-8 decoding of GitHub metadata and selected a unique unchanged conformance insertion anchor before any affected merge action; no qualification attempt or diagnostic was retried.

No new engine behavior, public API, fixture, exclusion/admission, required workflow, gate-budget or ruleset change. Development Core/server 1.81.9 is unpublished. Separate historical/new hosted times establish neither a failure rate, suite-wide speedup nor timeout cause. The telemetry opening-only/manual trigger remains inherited unchanged, and this synchronization does not dispatch it.

[verify.py](verify.py) replays the retained artifacts and original case-cost/telemetry/timing/wrapper dossiers without executing .NET. Historical checkout-scoped delivery boundaries are checked at their recorded commits; the current four-file case-cost tool/CI boundary is checked separately. Sealed original verifiers remain unchanged. See the [hash manifest](MANIFEST.json), [merge receipt](MERGE485.json) and [HTML receipt](DOCUMENTATION-CHECKS.json).
