# Selected source-free construction policy

## Profile and exact scope

Reserve `bosak-constructed-atomic-v1` and `bosak-constructed-atomic-descriptor-v1`, **inactive**. Under existing CompileXPath with an explicit supported assessor, only the trimmed complete source `$constructed` selects this profile. This is an exact guarded identity consumer for results of the existing programmatic ConstructSimpleValue method, not a new constructor function, cast grammar or general variable rule. `$value`, numeric, ordinary, nilled and data profiles/descriptors keep their meanings.

Freeze supported CompileOptions using actual compiler/static-context capture; fingerprint all five existing limits plus lexical input, accepted output families, fresh/null-origin policy, optional provider admission, per-operation identity/lifetime and no-cache rules. Descriptive fingerprints do not authorize receipt/endpoint identity. Unsupported syntax/options refuse before Bind. A supported bound advisor must also implement existing IQueryOperationLifetime on that exact advisor object; otherwise this profile returns Unsupported. Do not fall back to an assessor-wide lifetime, a synthetic no-op lease, a different advisor or a new required member on every provider.

The profile uses Run/RunAsync with explicit empty snapshot membership. OpenSnapshot remains unavailable for this profile before provider input/lifetime getters. Do not synthesize ISchemaAssessmentInput, source/snapshot tokens, a source node or a null-input OwnsTypedValue call. Existing invalid/foreign membership precedence still applies to other operation methods. No general ReadTypedValue or compatible-view/source admission is gained.

## Actual ownership and raw work

The engine/session is the local receipt owner; actual provider resources remain externally owned. Lazily obtain one successful borrowed admission from the exact bound advisor's IQueryOperationLifetime.Enter per raw operation. Acquisition runs under current raw/binding/cancellation checks. Retain that IQueryOperationLease through all actual provider calls and the raw callback/task, then dispose only the admission, never advisor/assessor/container resources. A provider unable to cover its constructed immutable values/type evidence for this lease scope must refuse the profile.

Compile/bind does not acquire this runtime admission. Unsupported input screens do not call the provider constructor or acquire a result owner. Successful first acquisition is reused within that raw operation; failed acquisition does not create a permanent receipt or phantom lease. Enter throw/null/close/cancel/registration failure needs exact rollback/cleanup. No callbacks while holding an internal publication/state lock. A pending acquisition is actual raw work even before it returns a lease.

A private provider-call scope is an immediate implementation prerequisite. It must track in-flight provider acquisition/construction/evidence calls independently from whether the user callback has returned. Closing a user callback window revokes further calls/result delivery immediately, but cannot release borrowed provider admission while a call is still on the stack. Run/RunAsync completion and session DrainAsync must account for this tail work and its cleanup. Canceling only a drain/wait does not cancel or release actual raw work; raw cancellation stays cooperative. No detached result/payload object holds an active admission or extends raw lifetime.

First-profile provider-touching calls within one operation are serialized; overlap or reentrancy refuses before a second provider acquisition/call. Independent raw operations each need their own supported provider admission; this does not promise native parallelism. Even rejected out-of-window/escaped-operation use must not create a late lease leak or prematurely close an in-flight provider call. Such escaped use cannot deliver authorized results and is not supported application behavior.

| Internal state boundary | Required action |
|---|---|
| Fresh, first eligible call | Admit an in-flight call ticket before external Enter; acquire outside the gate. |
| Acquiring when raw callback completes/cancels/closes | Revoke new work/delivery; retain raw/session drain accounting until Enter returns and any returned admission is cleaned exactly once. |
| Admitted, eligible call | Guard raw/binding/lease/currentness before and after each provider callback; no new Enter. |
| Callback window closed with active call | Further callbacks stop at guard boundaries; retain resources until the active call/mandatory cleanup finishes. |
| No active calls and callback/task done | Release provider admission once, then finish existing engine raw lease cleanup/drain. |
| Closed | Receipt reuse and provider calls refuse; copied payload remains ordinary data. |

Preserve original work exception then cleanup failures in established order; a sole cleanup error propagates unchanged. No double Complete/Dispose, hidden background cleanup failure or machine-wide error setting change. Qualification needs actual handles and deterministic callback barriers, not only a standalone lifecycle model.

## Inputs, episode and provider call

ConstructSimpleValue screens an unmarked direct String carrier as lexical input; reject source/constructed markers, Undefined, other atomics, nodes and structured values with Unsupported before constructor/target/episode callbacks. Descriptive string schema names do not authorize input type. Require no user-schema type annotation; pass a plain lexical string to the provider and never infer source provenance. Null target/episode retain argument checks; the actual target must be owned by the selected authority before construction.

Capture CurrentDateTime and ImplicitTimezoneOffsetMinutes from the supplied episode once per call under the admitted guards, then pass an engine-owned immutable copy. The first eligible call fixes the raw operation's episode; subsequent calls require exact same local ticks/offset and timezone constants, not instant-only equality or object identity. Use a host-selected supported implicit-timezone range of -840 through +840 minutes. Do not read ambient clock/timezone, reread caller getters inside provider construction, normalize the supplied date/time offset, or retain the caller episode object as authority. The existing DateTimeOffset episode representation is bounded; no extended-year episode claim follows.

Call only the exact bound advisor's ConstructSimpleValue with plain lexical input, the verified target and frozen episode. Supported means the provider produced the value under that target and agreed profile; it is not a schema validator implemented by Core. Provider precision/coverage inability is Unsupported, not silently rounded numeric success. General facets/native conformance remain actual-provider qualification.

## Fresh evidence and result validation

Use the exact returned Supported container as a private call-bound issuance witness. No caller-supplied container or copied handles can create this receipt. Source-bound OwnsTypedValue requires a real input and is deliberately not used here; all node/facts/assessment/source-input getters are forbidden. This witness is valid only with the same engine operation, exact bound advisor/environment and actual live borrowed admission.

Validate the entire bounded provider result and dispose any owned iterator before publication. Require IsList=false and exactly one cell; list, empty or multi-cell Supported shapes return Unsupported, never successful empty/flattened output. Atom overflow refuses before excess Current, including malformed/unsupported shape staging. Returned governing and declared type must be owned and authoritatively the same component as the requested target using IsSameComponent; do not require handle interning, compare names/digests, or infer equality from reference shape. Unsupported identity capability yields Unsupported; Supported false target mismatch is malformed. Actual type must be owned and remains the actual member supplied by the constructor; do not replace it with the governing type or infer union membership using numeric derivation alone. The provider is responsible for valid actual member/facet semantics.

Only plain String/Boolean/Integer/Decimal result carriers enter this first profile. Preserve exact accepted representations and captured numeric digit/scale limits. Reject default/nonplain/already-marked/structured cells and non-null OriginalAssessment as malformed. A constructor has fresh target-type evidence even for the same lexical/value/type result; never copy an input document validation claim or rewrite original node evidence. This selection does not change historical no-op conversion/native constructor receipts globally.

Publish one direct atomic XdmValue carrying a new private operation-local construction receipt only after complete validation, identity checks and final currentness proof. Allocate unpublished result/receipt storage first; no provisional marker escapes. Repeated construction calls do not intern/deduplicate receipts, and later consumption validates saved immutable evidence without replaying result getters. Copied payload/representation may remain readable after raw completion; schema authorization does not. A provider must cover immutability and currentness of the returned constructed evidence for the admitted raw scope; a backend needing independent per-value revocation/owners cannot claim this narrower capability silently.

## Quotas and no fabricated cache

MaxTypedValueAtoms keeps per-result consumption units. MaxNumericDigits and MaxDecimalScale keep accepted carrier representation limits. MaxOpenSnapshots and MaxCachedValuesPerSnapshot remain source-admission/per-snapshot resident scalar limits; constructed results consume zero source slots and do not reinterpret either field as an operation/container quota. Do not store constructor receipts in QueryTypedScalarCache or a hidden session/global registry. Receipts are reachable from returned values only, plus bounded operation metadata (one admission/episode); retaining an arbitrary number of results is caller memory, not a promised bounded-total operation budget.

There is no new owner workload default, lexical byte/string-length cap, provider parsing allocation limit, retained-memory cap, total constructor-call budget or hard deadline. Input strings are already allocated, and output digit bounds do not bound provider lexical parsing work before return. Requirements for explicit parse budgets, retained constructed-value accounting or persistent per-value authentication must be settled with owners before general freeze; this scope does not prove those signatures unnecessary.

## Consumption and errors

Evaluate in this profile captures exactly one unprefixed constructed variable, absent focus, and the same closed services/runtime-state policy. It accepts only this operation's successfully issued receipt and matching representation, revalidates admission/binding/target/governing/declared/actual evidence, and returns the same marker/value through guarded identity dispatch. Do not feed the new marker into generic VM or source-cache TypedOwner fallbacks. Unmarked values, source markers, another operation/session's receipt, tampering, closed/canceled/revoked resources or changed authority refuse. Another raw operation in the same compiled session does not revive old construction receipts. Numeric computation, data atomization, persistent/cross-session import and all-host use remain unselected.

| Outcome | Selected behavior |
|---|---|
| Provider Supported, one valid non-list eligible cell | Supported direct fresh marked scalar; original assessment null. |
| Provider Invalid | Engine Invalid outcome, fixed construction-invalid diagnostic; never read provider Value. No receipt. |
| Provider Unsupported or missing optional profile capability | Engine Unsupported; never read provider Value. No fallback/receipt. |
| Unsupported input/result family/shape/identity capability | Unsupported under this finite profile. |
| Result exceeds atom/digit/scale capacity | Existing NotSupportedException capacity refusal, not lexical Invalid. |
| Foreign target, null/malformed outcome, NotFound/unknown status, Supported-null, wrong target/type/origin, forged/tampered evidence | Existing argument/malformed-evidence exception. |
| Cancellation/disposal/acquisition/provider/cleanup failures | Preserve their actual exceptions and existing ordered cleanup, not Invalid. |

Invalid and Unsupported wrappers must not read provider diagnostic getters in this first profile; use engine diagnostics and preserve the caller's obligation to inspect Kind before Value. Rich provider diagnostic transport is later work. The existing private two-kind Outcome helper needs a precise Invalid branch for this new operation without changing old outcome meanings.

## Deliberate remaining boundary

This is a Core implementation selection, not a completed provider-call scope, construction receipt, native backend or owner approval. The current mandatory-input OwnsTypedValue contract cannot authenticate arbitrary retained source-free values. The proposed call witness works only for the agreed leased raw operation; it is not an alternative proof for persistent/cross-operation/list/general construction needs. Those remain explicitly open alongside compatible views, all-host behavior, actual owner performance/migration and C45/API freeze. No new public signature is selected for this optional finite profile, and none are declared permanently sufficient.
