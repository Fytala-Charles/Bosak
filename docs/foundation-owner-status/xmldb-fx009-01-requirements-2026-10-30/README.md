# FX009-01: XmlDb Foundation Requirements Matrix

Date: 2026-10-30. **Requirements audit delivered for XmlDb/Core review.** FX009-02 candidate
qualification and FX009-03 joint disposition remain pending. No dependency, source/runtime,
format, storage or normative semantic change is made by this audit. The Core notification is
local and uncommitted; no remote delivery or owner acceptance is implied.

## Exact Scope and Inputs

The audited provider is PersistentXdmNode and its actual Storage snapshot/lease and Xdm test
call sites at XmlDb commit 2f9a36bc3ff22c8215d2f12ff38541770dd82585. Each selected file matches
that committed blob through Git's text clean filter; raw working/committed SHA-256 and CRLF
counts remain explicit, including mixed line endings. Separate local SR3-B1 M2 work is preserved
and is not silently relabeled as foundation qualification. The product Core pin remains 1.26.2,
whose verified source revision is 6a3372fc3b91e733b71a426cf40f26feaaeb3bcb.

Core's named candidate is source c88eea4308c0e49a81e7bfe2f481319f2e4d208c / 1.34.2. The packet
feed SHA-256 is 03bb793d3cd69d7e819e1568ab11eff98bd2cec356a963098b3d5f738b15dc39; this audit
checks that ZIP hash without consuming/repacking/upgrading it. The proposed 5,766-entry manifest
is based on the separate 1.34.1 public baseline at f758a29f368201abdb98af003274451c84a9b7c4.
A proposed member ID is not a new candidate execution or accepted freeze slot. The 176 unique
entries include 6 evidence-only rows; repeated IDs across requirements count once,
and these entries are not 176 new APIs or automatically frozen members.

[Machine matrix](fx009-01-matrix.json) pins every source, baseline/manifest and feed identity,
exact member IDs, baseline row/declaration, compatibility-case IDs and source/test line anchors.
All nine selected Core XDM files and XPath31Expression.cs have identical committed source bytes
between the pinned source and candidate. EvaluationContext.cs differs by a change-history row
and internal initial-focus resolver/lazy-global cache methods; it is not byte-identical. Public
source similarity is not actual assembly binding, ABI, runtime behavior or context-capture proof.

## Requirements and Existing Use

| ID | Boundary / actual source or fixture | Requirement / finding | Core disposition needed |
|---|---|---|---|
| FXR-01 | PersistentXdmNode.NodeKind; StorageNodeKind | Direct cast requires XML flag values 1/2/4/8/16/32/64, None=0, All=127 and preserved compiled defaults/bytes | Existing constants; no renumbering or persistence migration |
| FXR-02 | Children, Attributes, AxisIds; Axes_AllNodeContexts_MatchInMemoryProvider | All thirteen axes, optional name filters, reverse ordering and lazy results; known xmlns/Attributes baseline difference is recorded in the fixture | Existing surface; qualify defaults and provider differences separately |
| FXR-03 | Name/URI/IsId/ToXmlString; XPath_NamespacesAndMixedContent_MatchesProvider | Snapshot-guarded XML metadata, inherited encoded-name defaults, exact URI and serialization behavior | Preserve required and optional/default members |
| FXR-04 | TypedValue and XdmValue.FromString | Current adapter returns a String carrier; determine named-profile atomization/type/error behavior before claiming conformance or fixing semantics | FXG-01; no new factory/signature demonstrated |
| FXR-05 | IsSameNode, DocumentOrder; transactional old/new snapshot fixture | Same snapshot object + node ID; independent snapshots/generations distinct; checked node-ID order is not global document ordering | Preserve snapshot-relative behavior; FXG-04 |
| FXR-06 | NodeSequence/NodeEnumerator; XdmSequence.Enumerator | Provider fetch checks and public cached Current are different boundaries | Preserve implementer slots; characterize FXG-02/03 |
| FXR-07 | BindResult | Outer wrapper guards fetches, not nested graph ownership; materialization is not detachment | Explicit borrowed snapshot ownership; FXG-02/03 |
| FXR-08 | NodeEnumerator.MoveNext; ImmutableXmlSnapshot.Dispose | Inner Dispose on normal exhaustion only; no universal early-break/fault/cancel transfer; snapshot owns the actual generation lease | Caller cleanup; optional scope only if demonstrated necessary |
| FXR-09 | XdmValue/Map/Array carriers; persistent leaf metadata access | Containers/cache may return wrappers after disposal, but dereferencing the persistent node requires its live original snapshot | Preserve carriers; qualify nested/copied/deferred access |
| FXR-10 | BeginRead/OpenPinnedSnapshot, AttachReaderLease; GenerationLeaseTests | Actual snapshot-owned admission survives coordinator disposal and releases on explicit snapshot disposal | No Core storage dependency or reclamation capability |
| FXR-11 | IXdmNode defaults; exact Xdm project reference | Untyped XML inherits optional schema/DTD defaults; no mandatory Schema/PSVI implementation | Do not adopt the full C45 prototype by inference |
| FXR-12 | ResolveDocument; XPath31Expression and EvaluationContext call sites | Same-snapshot exact URI loader; API/Runtime caller needs are separate from the production Core-only package boundary | Qualify context capture/profile, FXG-03/04/05 |
| FXR-13 | Project/PackageReference boundary tests | Source/default dispatch, enum constants, carrier layout and unchanged-binary replacement need exact candidate evidence | FX009-02; no product pin adoption |
| FXR-14 | Unknown axis, missing URI, disposed snapshot; mutable snapshot caches | Explicit errors; sequential evidence does not promise cancellation isolation, reentry or concurrent disposal | Finite supported scope first, FXG-05 |
| FXR-15 | Existing value-kind constants and XML-only persisted kinds | JNode already exists in the pinned carrier; ordinary XML does not require persisting generalized JSON/schema graphs | Preserve legacy obligations; no waiver of other owners' needs |

## Ownership Audit: Exact Limits

The constructor borrows a live ImmutableXmlSnapshot. It neither acquires nor transfers snapshot
ownership. Children, attributes, axes and wrapped results keep references to that same snapshot.
The actual lease is acquired by EmbeddedXmlDatabase.BeginRead/OpenPinnedSnapshot and attached
to the snapshot; the internal epoch path has separate child admission/generation ownership.
Standalone immutable Open is a distinct path; transactional roots must use BeginRead for
actual snapshot-owned generation admission. No result enumerator independently acquires a
reader lease. Disposing a coordinator is not disposing its independently owned snapshots.

NodeSequence.TryGetLength/GetEnumerator and NodeEnumerator.Current/MoveNext validate the
snapshot. However, public XdmSequence.GetEnumerator merely creates a struct and starts the
provider on first MoveNext; its Current returns a cached XdmValue. After a successful fetch,
reading that cached Current after snapshot disposal need not throw. Accessing the cached
persistent node's StringValue or advancing the bound sequence does check disposal. The provider's
Equals/GetHashCode also compare identities without a disposal check; IsSameNode checks both
snapshots when both nodes are persistent; a foreign provider returns false after the own-snapshot
check. Constant inherited defaults such as IsNilled=false and SchemaTypeAnnotation=null also
do not consult snapshot lifetime. Do not describe these operations as an all-access guard.

BindResult does not recursively wrap maps, arrays, functions, external objects or nested
sequences, nor does it detach/copy persistent resources. A materialized container can still
hold an unusable persistent node. A previously copied scalar string can remain usable. The
outer bound wrapper can itself reject another fetch after snapshot disposal even when that
fetch would have returned a scalar. Retention of a CLR object is not a resource-use guarantee.

NodeEnumerator disposes its inner iterator on normal exhaustion only. Its two Core interfaces
and the public struct enumerator expose no universal Dispose/cancellation member. Early break,
abandonment and exceptions therefore do not establish recursive or foreign-iterator cleanup.
This does not prove a native leak: the audited generation lease is snapshot-owned. The caller
must explicitly dispose its snapshot when all supported graph use ends. Snapshot clears its stored lease before calling its Dispose;
this audit does not certify close-failure recovery or concurrent disposal. No GC/handle/cancellation
or simultaneous-disposal experiment was run here; stronger guarantees remain unqualified.

## Open Return Items

| ID | Owner / item | Evidence or decision still required |
|---|---|---|
| FXG-01 | XmlDb + Core: untyped typed-value policy | Actual candidate data()/type/numeric/error comparison; immutable normative adoption before any semantic fix |
| FXG-02 | XmlDb + Core: cached/nested result boundary | Explicit borrowed-result disposition and cached Current, nested/copied/materialized fixture observations |
| FXG-03 | XmlDb + Core / FG-04: lifecycle | Actual snapshot/generation lease retention/release for exhaustion, break, abandonment, faults and cancellation; optional scope only on demonstrated need |
| FXG-04 | XmlDb / FX009-02: candidate qualification | Isolated real source/package/unchanged-binary runs, loaded hashes, constants/default dispatch, old stores/identity/order/namespace/type/error fixtures |
| FXG-05 | XmlDb + Core / FG-06: supported concurrency/capture | Finite sequential/cancellation/reentry/disposal scope and any claimed deferred semantic environment behavior |
| FXG-06 | Core + Schema + XmlDb / FX009-03: joint scope | Exact stable/legacy/experimental member scope and dated dispositions; no inference from source or test counts |

No new required Core signature is demonstrated by this audit. That finding is not an empty
accepted gap list: all six items above remain open, with decision/signers null. The existing
borrowed-owner direction needs a finite joint disposition; this report does not add required
Dispose members or accept C45. Ordinary XML exclusions do not waive Schema's full foundation
requirements or existing Core compatibility obligations.

## Validation and Next Step

Validation checks all 15 rows against exact manifest entries and full baseline declarations,
source/fixture anchors, snapshot ownership paths, and immutable candidate/pinned source bytes.
The selected enum/default/member slots remain explicitly versioned. The retained latest M2
record qualifies 2,789 Windows IDs/full Linux cases, including 27 Xdm cases per platform; that
is historical current-pin evidence, not 1.34.2 execution. Earlier isolated 1.34.1 provider and
binary receipts are also historical, with their original limitations. The existing Xdm suite is rerun on Windows against the unchanged declared 1.26.2 pin:
[27 pass, zero fail/skip](evidence/current-pin.trx.xml). Its output Core DLL matches the cached pinned package bytes; no
load-time binding observer was used. No 1.34.2 candidate tests or packages are consumed.

Next Core/XmlDb action: review the six return items and finite borrowed-result/profile scope,
then execute FX009-02 against the exact agreed candidate/manifest. FX009-03 disposition,
publication, product pin adoption and any storage change remain separate. Existing M2 work,
all persisted formats/frozen fixtures/histories, and CanReclaim=false remain unchanged.
