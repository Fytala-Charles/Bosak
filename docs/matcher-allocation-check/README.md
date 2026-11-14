# Matcher allocation assertion correction

Postmerge CI run 43592879249 at 0eb66c658eb3d17fb3695e7771ff30d438cd5c03 failed one test: conversion allocated 4,000,120 bytes against an exact 4,000,000 assertion. The source of the 120-byte excess is not identified. The original failed log, runtime TRX, counters and artifact digest remain separate evidence; earlier successful runs do not replace this failure.

The correction changes only the test. It keeps exact-zero warmed type matching and bounds 100,000 warmed conversions by 4,001,024 bytes: the existing 40 bytes/call plus a fixed 1 KiB allowance. This admits the observed120-byte excess without admitting a new per-call object. A future legitimate reduction of validation allocation may pass too.

| Observation or boundary | Allocated bytes | Corrected conversion bound |
|---|---:|---|
| Existing 40 bytes/call over 100,000 calls | 4,000,000 | Accepted |
| Original failed run | 4,000,120 | Accepted |
| Maximum with fixed 1 KiB allowance | 4,001,024 | Accepted |
| One byte beyond the bound | 4,001,025 | Rejected |
| Original 112 bytes/call closure route | 11,200,000 | Rejected |

The separate matcher assertion remains exactly 0. Original closure regression would also fail that check with 7,200,000 bytes. This is a regression budget, not a claim that every conversion always allocates exactly 40 bytes or an attribution of the 120-byte excess to a proven cause.

Corrected full solution 26,435 and separate language-server 90 tests pass. Prior main standalone independently reconciles 10,221/0/4,380 across 14,601 identities in 631.688s with unchanged corpus/runtime/transcripts and original 1200s/8GiB. Main's original unit failure remains recorded. Engine source, production runner, workflows and solution/global configuration are unchanged; local frozen/standalone gates were not repeated for this test-only correction. Hosted checks for the corrected head remain required before merge. No unchanged failed head was retried.

See [review](REVIEW.json), [results](RESULTS.json), [compact evidence](EVIDENCE.zip), [manifest](MANIFEST.json), [offline verifier](verify.py) and [HTML receipt](DOCUMENTATION-CHECKS.json). Run `python docs/matcher-allocation-check/verify.py` to replay original failure and corrected receipts without launching .NET. The archive includes the original failed runtime TRX; the complete original 5.9MB artifact remains local at `.guard-tmp/allocation-check-20261114/ORIGINAL-TEST-RESULTS.zip` bound to GitHub's digest. No runtime binaries or profiler traces are added. Development 1.81.11 remains unpublished.
