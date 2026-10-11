# REQ-125 End-to-End Consumer Review

> Initial review and correction re-review: 2026-10-11 · Consumer: Bosak.Braid · Current decision: technically accepted within declared coverage/restricted profiles; C-F1/D-F1 closed (section 7)

## 1. Delivery and Verification

Reviewed clean Bosak tracked sources at `697c01e19db2bef099a78f2e727c9d869c512373`. Relevant implementations: corrected Slice A `5b0cf05`, corrected Slice B `facbf37`, B-F3 `aac4387`, Slice C `012efa5`, Slice D `1d85767`.

Independently ran the XSLT test filter covering Validation, XsltCaptureTests and SliceDExecutionControlTests: **261 passed / 0 failed / 0 skipped**. This is a targeted suite, not a rerun of the full engine/conformance gates reported by Bosak. Public-API probes additionally checked changed-content cache receipts, cancellation at the end of a short transformation and a 1-byte output bound against a 1,000,000-byte text result. No engine source modifications or Braid pin changes were made.

## 2. Acceptance by Capability (Initial Assessment; Superseded by Section 7)

| Area | Review outcome |
|------|----------------|
| Slice A | Previously accepted within declared coverage; unsupported gaps must remain explicit |
| Slice B | B-F1/B-F2 previously verified closed; B-F3 now verified closed; accepted for its documented controlled acquisition contract |
| Slice C | Public capture, identities, encoding/BOM, collisions and partial outcomes have passing regression evidence; byte-production bound gap C-F1 remains |
| Slice D | Cooperative checkpoints, nested budgets and isolation have passing regression evidence; short-run completion gap D-F1 remains |
| REQ-125 overall | Feature delivery exists, but whole-request consumer acceptance remains pending the two findings below |

B-F3 probe now returns a fresh node with `NEW` after approval changes from `<approved>OLD</approved>` to `<changed>NEW</changed>`. Receipts carry the corresponding hashes and `ServedFromCache=false`. The old content is no longer returned under the new receipt. The new digest/URI-validated cache behavior is documented and covered by upstream tests.

## 3. C-F1: Output Bounds Are Checked After Full Serialization

Public probe: a text-output template emits `string-join(for $i in 1 to 1000000 return 'x', '')`; capture uses `MaxBytesPerOutput=1`.

**Observed:** capture returns Partial/XV0007 with the message that the output serialized to 1,000,000 bytes and was dropped. Source inspection confirms `ResultTreeSerializer.SerializeToBytes` first calls the complete string serializer and then `encoding.GetBytes(serialized)`. `XsltCaptureSession` checks per-output and aggregate bounds only after that complete byte array exists.

This correctly bounds retained successful outputs, but it does not implement REQ-125 section 3.3's requirement to enforce byte bounds during production rather than after constructing the entire output. Principal output checks happen after the transformation; secondary checks happen after each entire secondary has been serialized. A tiny limit cannot prevent allocation of a huge serialized string and byte array. Process termination supplies a separate hard control, not an implementation of the advertised byte-production limit.

**Required correction:** a bounded encoding/serialization sink enforcing per-output and remaining aggregate byte budgets while bytes are produced, or an explicit restricted/unsupported profile that refuses cases without the guarantee. Preserve encoding and BOM accounting, output identities, classified partial outcomes and prior outputs. State result-tree construction/memory limits separately; bounded serialization alone is not an OS memory sandbox.

**Regression evidence:** principal and secondary outputs stop at the configured production bound without allocating the entire encoded result; aggregate budget applies to the next output's production; UTF-8/UTF-16/BOM boundaries retain fidelity; zero/small limits and serialization failures classify correctly. Use a meaningful instrumented sink or production counter to prove early stopping, rather than only asserting that the output was eventually dropped.

## 4. D-F1: Short Runs Can Report Success After Cancellation

Public probe: a compiler message listener cancels the token on `<xsl:message>cancel</xsl:message>`. The template then emits `<ok/>`. `CaptureTransform` receives that token through `XsltCaptureOptions.CancellationToken`.

**Observed:** the token is cancelled before the transformation finishes, but the result is `Success`. `ExecutionGate` amortizes token/deadline checks over 64 checkpoints. A short run can finish before the next batch check, and capture has no unconditional completion check.

Cooperative batching is an acceptable latency tradeoff during work; it must not turn a known cancelled run into a successful published result. This probe does not demand preemption inside a single XPath operation. The same completion boundary needs to verify an expired deadline after the final long operation or serialization.

**Required correction:** unconditional control observation before/after terminal serialization and before constructing a successful capture result, while preserving exact operation-budget semantics. Classify cancellation as Cancelled/XV0013 and deadline expiry as a non-success limit outcome. Retained earlier outputs remain partial evidence. Keep ordinary hot-loop batching if useful, but document terminal checks distinctly.

**Regression evidence:** cancellation from a message listener in a run shorter than 64 checkpoints cannot succeed; cancellation/deadline expiry during final serialization cannot succeed; generous controls remain byte-identical; sequential/nested/concurrent isolation remains green. Distinguish short-run terminal cancellation from mid-loop interruption.

## 5. Ownership and Next Step

Register corrections under existing Bosak REQ-125; Bosak owns serializer/gate changes and regression delivery. Braid will rerun these probes against the fixes. Slices A/B can be adopted independently with faithful coverage and resource evidence; production preview acceptance must retain these open C/D gates.

No independent XSLT parser, reflection access or engine patch is introduced by this review. Braid's production pin is unchanged. The existing Braid 186-test code evidence remains current because this step changes review documentation only.

## 6. Bosak Correction Delivery (2026-10-11; Consumer Re-review Pending)

The review above remains the consumer's original assessment. Bosak has now implemented corrections in the working tree based on main `697c01e`; Braid has not yet accepted these corrections or changed its production pin.

- **D-F1:** unconditional token/deadline observations before/after serialization and before result publication, without decrementing the operation budget or resetting hot-loop batching. Bounded encoding observes controls between chunks. Short message-triggered cancellation returns Cancelled/XV0013; terminal deadline expiry is non-success/XV0011. Earlier outputs remain evidence.
- **C-F1:** an incremental encoding sink enforces per-output and remaining aggregate budgets before retaining bytes. XML uses a fixed-size decoding bridge and bounded lexical filter; HTML/XHTML share writer-based serializer bodies; text traverses items/nodes and maps characters incrementally. No complete serialized string or encoded array is constructed for an overflowing supported profile.

Byte-bounded capture streams text (including character maps), XML, HTML and XHTML. It refuses JSON/adaptive, text Unicode normalization, non-text character maps and non-XDocument markup provider fallbacks before serialization as `UnsupportedSerializationProfile` / `XV0014`. Unbounded capture keeps the established serializers for those profiles. Encoding/BOM bytes count against per-output and remaining aggregate budgets; fixed-size encoding lookahead is discarded on overflow. Result-tree construction, normalization and XPath working memory are outside these byte budgets; the host worker process remains the hard memory/wall-clock control.

The restricted-profile option is the explicit alternative permitted by section 3 of this review. Please verify both the original probes and the declared refusals; unsupported profiles cannot report successful byte-bounded capture.

Regression command:

```powershell
dotnet test tests/Bosak.Xslt.Tests/Bosak.Xslt.Tests.csproj -c Release --filter "FullyQualifiedName~ConsumerReviewCorrectionTests|FullyQualifiedName~XsltCaptureTests|FullyQualifiedName~SliceDExecutionControlTests"
```

The new 42-case correction suite includes actual production counters (a 1-byte budget against a million-character result encodes only constant-size lookahead), principal/secondary and aggregate breaches, encoding/BOM fidelity, explicit refusals, serialization failure identities, public probes and terminal cancellation/deadline checks. **Bosak verification:** Release build 0 errors; full solution **3,945 passed / 0 failed / 0 skipped**; combined consumer filter **76/0**; QT3 **31,142/0/679**; qt4 gate-only **8,278/0/1,894**, zero gated failures. The database-provider HTTP test server required an unsandboxed test run; all 47 provider-database tests then passed. See the handover for the environment note and logs.

## 7. Braid Correction Re-review (2026-10-11)

**C-F1 and D-F1 verified closed for the documented restricted byte-bounded profile.** Reviewed Bosak's uncommitted correction working tree based on main `697c01e`; no immutable correction commit is available yet. No consumer engine edits or Braid pin changes were made.

Independently reran the original public probes: message-triggered short-run cancellation now returns Cancelled; the million-character text result under a 1-byte limit stops serialization during production and returns Partial/XV0007. B-F3 remains correct. Independently ran the documented Release filter (ConsumerReviewCorrectionTests, XsltCaptureTests, SliceDExecutionControlTests): **76 passed / 0 failed / 0 skipped**, including the 42-case correction suite, production counters, encoding/BOM fidelity, terminal checks and declared unsupported-profile refusals. The full engine/conformance gates in section 6 remain Bosak-reported and were not independently rerun here.

Source review confirms the incremental encoding sink, per-output/remaining-aggregate enforcement before retaining bytes, explicit XV0014 refusals, and unconditional terminal token/deadline observations without consuming additional operation budget. Restricted byte-bounded capture supports text/XML/HTML/XHTML as documented; JSON/adaptive and the other listed profiles remain explicitly unsupported under byte bounds. This is not universal serializer support or a hard bound on result-tree/XPath memory. Braid must preserve unsupported outcomes and provide separate hard worker-process controls where required.

**Consumer conclusion:** the corrected end-to-end contract is technically accepted within its declared coverage and restricted profiles. Final delivery traceability and Braid dependency adoption require Bosak to commit the reviewed corrections and provide the resulting revision. Existing Slices A/B remain accepted. Production preview is not enabled by this documentation review. The prior open C-F1/D-F1 assessments are superseded by this re-review; the immutable delivery/pinning step is still pending.
## 8. Immutable Delivery

The reviewed correction source and acceptance evidence are committed with this delivery. The resulting revision will be recorded in the traceability follow-up and used by Braid's immutable dependency pin. Section 7 records the consumer's actual assessment; its uncommitted-tree references describe the state at review time.
