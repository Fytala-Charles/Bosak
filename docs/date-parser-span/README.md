# Date parser span candidate

Development 1.81.13, unpublished. Date regex-group numeric parsing and date-only timezone helpers use spans, preserving validation order, parsed values and error construction. Shared dateTime/time helpers remain unchanged. No new semantic behavior or intentional deviation is proposed.

[Adoption evidence](ADOPTION.json) records immutable XSD 1.1 and F&O 3.1 revisions before implementation. Three new tests cover calendar/timezone fields, invalid forms and distinct bounded-year/numeric overflow diagnostics.

The rebuilt engine matched the copied original implementation in 3,164 differential cases, comparing Boolean acceptance, every parsed field, timezone flags, and caught exception type/message. The same guarded worker measured 10,000 warmup and 100,000 calls per row/variant:

| Input | Original bytes/call | Candidate bytes/call |
|---|---:|---:|
| Plain date | 800 | 704 |
| UTC date | 856 | 736 |
| Numeric timezone | 1,072 | 736 |
| Obvious non-date / dateTime probe | 0 | 0 |

[Allocation results](ALLOCATION.json), [assembly identity](ALLOCATION-IDENTITY.json) and [bounded launch](ALLOCATION-RUN.json) retain the observations. The watchdog was 60 seconds with an 8 GiB aggregate job; engine hashes were checked before and after. Differential compatibility is not independent normative conformance; allocation measurements are calling-thread observations, not an end-to-end speed claim. Regex allocations remain.

Frozen QT3 is 31,397/0/424 and gated qt4 is 8,941/0/1,918, including 11 experimental passes. All skip names/reasons remain exact. See [frozen receipts](FROZEN-CHECKS.json).

Build passed. The initial sandboxed solution run hit 45 local HTTP fixture setup failures; its receipt is preserved in `.guard-tmp/date-span-engine-20261114/UNIT.log`. Those 47 database tests pass outside the sandbox. The unrestricted solution run and pinned standalone qualification are pending at this source checkpoint. Language server: 90 passed.

No release, public export or PR publication is included. Original diagnostics and validation logs remain in `.guard-tmp/date-span-engine-20261114`.
