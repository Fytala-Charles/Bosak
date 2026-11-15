# Direct in-record provider wrapper candidate

Development 1.81.15, unpublished. Materialized-record children, attributes and direct child/descendant/attribute/namespace axes use an owner-based XDM sequence/enumerator. Nonnegative captured record indices delimit the change; document/shell-root/negative shell wrappers and the existing InnerAxis IEnumerable contract remain on their existing routes. The existing Source.Wrap cache and captured record index are reused without delegate allocation or a new cache.

[Adoption evidence](ADOPTION.json) precedes implementation and fixes source/specification pins and coarse allocation budgets. Thirteen focused cases pass, covering real provider identity/namespace/mutation/typed values, held record/navigation, null/source/cast failures, reader ownership, discarded synthetic-record cache roots and isolated allocation.

The first focused run had two new test expectation failures. The tests incorrectly expected Children/Attributes method inputs to be live; qualified XDocumentNode source materializes those inputs, while axis iterators are lazy. Original failures and source are retained; corrected tests explicitly preserve method snapshots and axis mutation visibility. Engine source and pre-recorded budgets were unchanged during correction. Those failures are not represented as successful validation.

Full solution: 26,466 passed; language server: 90 passed. Frozen QT3 remains 31,397/0/424 and gated qt4 8,941/0/1,918, retaining every skip name/reason. An identical compatible allocation worker compiled against lowest qualified 1.81.14 references measured create 192 ->32 B, once 248 ->160 B, replay/cross-first 224 ->128 B; four pre-recorded budgets reject baseline/accept candidate. An identical semantic worker passes all 52 differential cases per version, using the actual candidate helper when present and the immutable prototype otherwise. Pinned standalone qualification completed in 800.631s: 10,221 passed / 0 failed / 4,380 skipped. All 14,601 identities and exclusions reconcile against pinned corpus fddf1cf920087e791f13315d68dfbe874d97dc56 under unchanged 1,200-second / 8-GiB limits and the Legacy/Xslt30 basic non-schema-aware profile. Skips are not passes. Qualified source: `5f553e02086f2ba7be6dec95f120c293f7031c75`.

[Build/tests](CHECKS.json) · [Frozen gates](FROZEN-CHECKS.json) · [Compatible allocation controls](CALIBRATION.json) · [Control review](CALIBRATION-REVIEW.json) · [Semantic controls](SEMANTIC-CONTROLS.json) · [Retained expectation correction](TEST-EXPECTATION-CORRECTION.json) · [Source scope audit](SOURCE-AUDIT.json) No source-level public API, disposal contract or package release change; no push or publication. Original receipts in .guard-tmp/provider-wrapper-engine-20261115.


The baseline closure in ADOPTION.json is qualified 1.81.14 input; the planned candidate is 1.81.15. Actual candidate hashes are separate in [results](RESULTS.json). Namespace nodes retain XDM identity even where fresh reference wrappers are returned. Children/Attributes method membership snapshots and lazy axis mutation visibility are deliberately distinct.

| Operation | Baseline B/call | Candidate B/call | Fixed ceiling |
|---|---:|---:|---:|
| Construction | 192 | 32 | 48 |
| Construction plus first use | 248 | 160 | 192 |
| Replay | 224 | 128 | 160 |
| Cross-thread first use | 224 | 128 | 160 |

Each row uses 10,000 warmup and 100,000 measured calls with setup outside measurement. The identical worker was built against the lowest qualified references, run once per version under a 60-second watchdog, 256-MiB managed heap cap and 512-MiB job cap. Four budgets were recorded before implementation; all reject baseline and accept candidate. No retries, minimum selection or budget widening occurred. These calling-thread allocation observations do not establish end-to-end speed. The prototype's greater first-enumeration-only cost remains historical evidence.

[Sealed evidence](EVIDENCE.zip) ? [Offline verifier](verify.py) ? [Manifest](MANIFEST.json) ? [Documentation checks](DOCUMENTATION-CHECKS.json)

Run `python docs/in-record-wrapper-sequence/verify.py` to verify all outcomes, exact exclusions, test receipts, controls and source hashes without launching .NET. Original two test expectation failures and corrected successful runs are sealed separately. Runtime binaries and raw traces are excluded.

Local qualification is complete. Next: review the concrete publication payload, then hosted checks before any merge. No push, public export or package release has occurred for this candidate.
