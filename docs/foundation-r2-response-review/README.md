# Schema registration and Braid O3 response review

1 November 2026. Reviewed live issue #468 and the Braid response supplied by the user. Production remains unchanged 1.42.0. [Raw inputs](INPUTS.json) pin the retrieved issue, exact response bytes and inspected source.

## Dispositions

| Role return | Current recorded state | Meaning |
|---|---|---|
| O1-Schema / O2-Schema | Schema informed; no submitted answers in issue #468 | Latest inspected comment registers the packets and explicitly keeps sign-off pending. Registration is not an O1/O2 response. |
| O1-XmlDb / O4-XmlDb | Awaiting returns | User confirms XmlDb is still pending; no acceptance inferred. |
| O2-Core / O4-Core | Two scoped Core design returns recorded | Existing design-only positions remain; not runtime/C45 acceptance. |
| O3-Braid | Submitted, format/draft binding valid, **Revise** | One open bounded-capture gap, BCAP-01; Braid is not accepted or counted as a missing response. |

The [latest inspected issue comment](https://github.com/Fytala-Charles/Bosak/issues/468#issuecomment-6562337494) informs Schema of the R2 packets; it does not answer O1/O2. The original issue body and older comments describe historical provider work. The [issue snapshot](inputs/issue-468.json) preserves what was actually read. No issue comment or external message was posted.

The [Braid response](inputs/O3-Braid.response.json) targets the exact proposal manifest `63ab58cf319ebb635e28b606c939e2edea2b18f682ea6564dc899fe2309ccc0f`. It is a Codex-authored consumer source/design review requested by the repository owner, explicitly not a human acceptance signature. Its revision and ordinary product pin remain separate from the 1.42.0 Core source. The original user-supplied response remains untouched and untracked; this dossier retains a byte-identical reviewed copy.

**Current count:** three submitted role returns (two Core design dispositions plus Braid Revise), four missing external role returns, and one open revision request. R2 remains InReview; R3 remains pending. Neither the Schema registration nor Braid's proposed experiment ceilings constitute runtime, performance-budget, C45 or freeze acceptance.

## BCAP-01: bounded operation-bound XSLT capture

Braid's existing preview uses XsltExecutable.CaptureTransform and projects retained output bytes/properties, messages and classified failures; it reads resource receipts from its actual ControlledResourcePolicy. It needs the same supported behavior through the new operation/session boundary. The existing draft SchemaXsltExecutable exposes Session and Transform returning SchemaResult<XdmValue>. A raw principal graph does not by itself preserve the complete capture contract.

| Required capability | Inspected existing route | Gap in current R2 route |
|---|---|---|
| Principal/secondary serialized bytes and effective output properties | CaptureTransform uses XsltCaptureSession, ResultDocumentCaptureObserver and principal serialization properties | Draft Transform returns XdmValue; the explicit-context bridge returns the raw principal result only |
| Ordered bounded messages and classified failure/outcome | CaptureTransform installs its capture message listener and builds XsltCaptureResult, including partial/cancelled output handling | No corresponding operation-bound capture result route is exposed by the draft |
| Output-count, per-output/total-byte and message limits | XsltCaptureOptions applies bounds while producing output | Atomic visitation bounds cannot substitute for serialized production bounds |
| Cooperative cancellation/time/operation limits | CaptureTransform creates/inherits an ExecutionGate | Composition with operation cancellation and raw resource ownership must be proven, not replaced by a canceled wait |
| Resource denial receipts | Braid projects the actual ControlledResourcePolicy.Receipts separately; XsltCaptureResult does not contain them | Private factory policy is captured/closed internally; no demonstrated operation-bound receipt handoff accompanies the proposed capture route |
| Distinct match/global roles and exact semantic environment | Private TransformWithExplicitContext supports separate roles; public CaptureTransform takes one source | No public operation-bound capture bridge is established for the compiled schema executable and separate roles |

This source audit confirms a **required capability/route gap**, not yet a decision to add a specific new public method. Calling the old capture API inside an arbitrary callback is not sufficient proof that it uses the schema executable's exact captured environment, admitted roles and advisor/resource policy. Serializing only the principal raw result after the operation would lose secondary/message/failure channels and could release resources too early.

## Concrete next Core action

Resolve BCAP-01 before considering O3 accepted. First establish whether a supported public composition over the same compiled executable/environment can preserve the complete capture behavior. If not, select the smallest operation-bound capture signature in a revised isolated declaration draft, reusing existing capture result/options where their semantics fit. Do not add an unrelated capture subsystem or silently route typed inputs through an unqualified legacy evaluator.

Either route must specify operation membership for match/global/dynamic input nodes; option/context precedence and refusal rules; finite production limits; per-run resource receipt ownership; raw completion through serialization; detached managed output behavior; and preservation of cancellation plus cleanup failures. Existing CaptureTransform defaults are unbounded, so Braid's selected bounded profile must explicitly supply its limits. Braid's suggested experiment ceilings are not Core defaults, total-memory bounds or agreed performance targets.

Required follow-up controls include distinct/shared/source-free roles; principal and secondary bytes/properties; ordered messages; each bound; denied resource receipts; partial/failure/cancel outcomes; raw lease retention through serialization; and cancellation/cleanup failure preservation. No new capture behavior or passing runtime control is claimed here. Braid must review the demonstrated route or exact revision; other owner dispositions remain scoped to the draft they reviewed.

## Verification and inbound responses

The response passes the existing format/draft-binding checker; that alone does not authenticate its author or accept its requested scope. Incoming responses under the owner packet's returns directory are now treated as unsealed submissions, separate from the sealed prepared packets/templates. This permits legitimate submitted files without implicitly adding them to the prepared-packet manifest or marking them accepted.

Run `python -B docs/foundation-r2-response-review/tools/verify.py`. It checks exact issue/response/source hashes, Braid's Revise/open-gap fields, recorded counts and unchanged production. Engine tests/gates are inherited from the existing cohort; no production implementation or full suite rerun occurs in this review-only step.
