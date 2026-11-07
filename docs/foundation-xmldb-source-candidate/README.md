# XmlDb isolated source candidate — bounded qualification

7 November 2026. An immutable export of clean XmlDb revision
`fbb829052f606f683aaf8620fa9401b4ba46c8e9` builds successfully against the exact
**1.81.1-preview.c0b926de** dependencies: **zero warnings and errors**, including
all three test projects and the sample. No source migration edit was required.
Full source qualification remains incomplete.

[Plan](PLAN.json) pins 308 exported source/configuration/fixture files, 14 cached
external package archives, a fresh private cache, offline restore sources and the
candidate-only dependency overlay. Owner source/project bodies stay unchanged;
only restored dependency lock files change inside the isolated workspace. MinVer
receives the observed integration-test identity `0.1.0-alpha.0.10` through its
supported command property, avoiding accidental parent Core Git version inference.
That is diagnostic version provenance, not an owner product release/tag change.
The proprietary source, binaries and frozen fixtures remain in the ignored task
workspace; this packet retains hashes and test receipts.

Completed evidence:

- Rebuilt XDM suite: **47/0**, matching the retained case catalog.
- Focused native Storage cleanup suite: **139/0**, matching the retained focused
  catalog. These are existing cases, not 139 new tests.
- Actual engine loads in XDM match preview package hashes in the Default,
  noncollectible context. Storage itself has no Core engine dependency; its
  passing focused cases do not establish package API coverage.
- Owner HEAD/status and source bodies remain unchanged. Product pins, storage
  formats, histories and frozen fixture bytes are preserved.

[Results](evidence/RESULTS.json), [TRX archive](evidence/TRX-RESULTS.zip) and
[execution receipts](evidence/EXECUTION-RECEIPTS.zip) preserve the runs.

The first XDM run passed 46 and failed one with an OS access denial during a
temporary staging-directory rename. The unchanged binaries passed all 47 in the
narrower authorized native-filesystem retry. The original failure remains;
the exact OS cause is not established and no engine fix is inferred.

The full 3,572-case Storage sweep was cancelled after its retained timing showed
about one hour, exceeding the launched 15-minute watchdog. Only the verified
owned test process tree was stopped. No completed full-sweep result is claimed;
3,433 cases lack completed results beyond the 139 focused controls.

The 863-case Integration suite was **not launched**. Automatic approval review
rejected the elevated full-suite retry because it includes intentional child
process kill/fault probes and general qualification authorization was insufficient
for that method. [Process review](PROCESS-REVIEW.json) records the restriction.
The approved safer alternative ran only XDM and focused Storage tests, after a
source scan found no process-start/kill calls in those test projects. The full
launcher now refuses while explicit authorization is absent. No machine-wide
Windows Error Reporting setting was changed.

Next requires a properly timed full Storage run and explicit authorization to
revisit the isolated Integration process probes under scoped launch protection.
Until those gates complete, do not label this a full 4,482-case qualification.
Production ordinary-input/lifetime adapter work remains separate, as do owner
workload/baseline/budget decisions, C45/freeze and publication approval.

Run `python docs/foundation-xmldb-source-candidate/tools/verify.py` for the
completed scope, source/layout/case/binding checks and explicit incomplete gates.
No owner repository write, commit, push, tag or publication occurred. No rendered
document approval is claimed.
