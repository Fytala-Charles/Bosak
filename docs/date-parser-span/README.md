# Date parser span qualification

Development 1.81.13, unpublished. The date parser reads regex groups and timezone slices as spans, avoiding temporary strings. The existing regex, validation order, parsed values and error construction remain unchanged. Shared dateTime/time helpers are byte-for-byte unchanged. No semantic deviation, new API or language coverage is proposed.

[Adoption evidence](ADOPTION.json) records immutable XSD 1.1 and F&O 3.1 references before implementation. Three added tests cover calendar/timezone fields, invalid forms, and bounded-year versus numeric overflow diagnostics.

| Verification | Result |
|---|---|
| Solution tests | 26,443 passed |
| Language server tests | 90 passed |
| QT3 | 31,397 passed / 0 failed / 424 skipped |
| Gated qt4 | 8,941 passed / 0 failed / 1,918 skipped; 11 experimental passes |
| Pinned standalone XSLT 3.0 | 10,221 passed / 0 failed / 4,380 skipped |
| Differential date checks | 3,164 matched |

All frozen skip names/reasons are exact. Standalone reconciles all 14,601 identities against corpus `fddf1cf920087e791f13315d68dfbe874d97dc56` in 780.187 seconds, under unchanged 1,200-second/8-GiB limits. The basic non-schema-aware Legacy/Xslt30 profile remains unchanged; exclusions are not passes. Qualified source checkpoint: `c477f1424157de76148c09563db50107c769566b`.

The rebuilt engine matched a copied original implementation across acceptance, every date/time field, timezone flags, and caught exception type/message. Each allocation row used 10,000 warmup and one 100,000-call measurement per variant in one guarded worker:

| Input | Original bytes/call | Candidate bytes/call |
|---|---:|---:|
| Plain date | 800 | 704 |
| UTC date | 856 | 736 |
| Numeric timezone | 1,072 | 736 |
| Obvious non-date / dateTime probe | 0 | 0 |

[Allocation results](ALLOCATION.json), [assembly identity](ALLOCATION-IDENTITY.json), and [bounded launch](ALLOCATION-RUN.json) retain the observations. Its 60-second/8-GiB controls and before/after engine hashes are recorded. These are calling-thread allocation observations, not end-to-end speed measurements. Regex allocations remain. Differential compatibility does not independently establish normative correctness.

The original sandboxed solution run failed 45 local HTTP fixture setups; all 26,443 tests passed outside the sandbox. The first standalone attempt failed required W3C HTTP cases because outbound sockets were forbidden, and its owned worker was terminated after those failures. It remains a failed, incomplete attempt. [Environment correction](ENVIRONMENT-CORRECTION.json) predates the fresh full run outside the sandbox, retaining the same corpus, exclusions and limits. No failed attempt was overwritten, filtered or represented as a pass. Two diagnostic preparation refusals occurred before launching the allocation worker and are recorded separately.

[Results](RESULTS.json) · [Sealed evidence](EVIDENCE.zip) · [Offline verifier](verify.py) · [Manifest](MANIFEST.json) · [Frozen receipts](FROZEN-CHECKS.json) · [Documentation checks](DOCUMENTATION-CHECKS.json)

Run `python docs/date-parser-span/verify.py` to verify all case identities, frozen exclusions, solution results, source hashes, allocation rows and retained sandbox failure without launching .NET. The evidence archive contains original successful and failed receipts, transcripts, test reports, diagnostic source and engine source snapshots. Binaries remain local in the ignored qualification packets.

No release, push, PR or public export has been performed. The earlier publication approvals do not authorize this new dossier. Next: review this bounded candidate and its sealed evidence for publication.
