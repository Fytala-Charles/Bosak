# Whole-snapshot navigation trial ? 31 October 2026

The native adapter now preserves original facts through a complete private assessed snapshot. **162 controls pass in each of four fresh processes**: baseline and holder-prototype cohorts, repeated in reversed order. Twenty-three actual API/VM path expressions cover navigation, copy identity and constructor behavior. The public V5 draft is unchanged. Production code, API acceptance and freeze status remain unchanged.

## Constructor provenance disposition

The candidate [design disposition](DISPOSITION.json) permits retaining existing evidence for a **verified exact no-op**: the value and its authoritative declared and actual type identities all remain unchanged. It grants no new validation, node/document assessment or issuer authority. Type-changing conversions require fresh target evidence without inherited OriginalAssessment. This refines earlier blanket draft cast wording; sealed historical proposals remain untouched.

The dated [F&O 3.1 Recommendation, sections 18.1 and 19](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/#constructor-functions) defines constructor/cast behavior through values and types. Bosak's capability metadata is a separate design choice. The [pre-execution basis](SPEC-BASIS.md) records exact dated XPath/XDM/F&O sources and the diagnostic-only scope; no normative deviation or production semantic adoption is selected.

Six guard controls permit the actual native decimal no-op and reject changed values, equal numeric values of a changed kind, foreign target handles, governing-type mismatch and actual-type mismatch. The latter two use injected negative evidence, not genuine native union/facet types. QName, digest, primitive kind or selected union member alone cannot prove exact type identity. The diagnostic guard is **not wired into VM constructors**. Native same-type preservation is observed; a general authoritative constructor fast-path implementation and actual derived/union type qualification remain pending.

## Whole snapshot implementation

[tools/Probe.cs.txt](tools/Probe.cs.txt) extends the [direct-node adapter trial](../foundation-native-adapter/README.md). One private native-exported XDocument is created per issued view; original AssessedElement state is mapped to each element by ordered count/name checks. Original type and declaration handles still wrap the actual retained native component objects. The caller receives wrappers, not mutable XML.

All navigation re-enters that snapshot owner: Children, Attributes, Parent, Document and all thirteen Axis routes. Regular wrappers are interned by underlying object identity; virtual namespace wrappers use owner/prefix/URI identity. Element wrappers carry V5 node facts and issued plain-value/evidence cells. Document, text, comment, processing-instruction and namespace wrappers carry no fabricated assessment facts. Node identity is scoped to the tree and underlying provider identity. A copied native assessment retains original type/assessment evidence but receives a distinct tree and node identities.

Native decimal values enter the same Core-side diagnostic holder as before, storing the complete evidence object. No new arbitrary public attachment factory or public node interface is introduced. Legacy schema annotation routing is still used by the unchanged VM.

## Verification performed

| Controls | Scope and result |
| --- | --- |
| Prior 61 controls | Actual bounded native assessment, original component/copy identities, Invalid/Unsupported/no document, cancellation, exact decimals and eleven direct API/VM expressions still pass with whole-tree wrapping. |
| Six rule controls | Exact no-op identity guard and negative type/value/authority cases pass as a separate diagnostic. |
| Thirteen wrapper controls | Cache identity, parent/document ownership, copied-node distinction, occurrence order, reachable original facts, namespace identity/ownership, mixed node kinds, foreign snapshot rejection and navigated-root FOTY0012 pass. |
| Thirteen axis controls | All axes return wrappers in the same tree; every returned element retains facts, parent links stay wrapped and document links return the same owner. |
| Sixty-nine path controls | Twenty-three compiled expressions each verify values, original assessment and actual type evidence. |

The fixture contains a document PI, root comment, internal PI, decimal text/CDATA and an inherited namespace. All thirteen axes are exercised across mixed seeds. Application attributes remain outside the native profile: attribute navigation is tested as empty, not as a positive typed-attribute capability.

Actual paths cover child/descendant, parent/ancestor, both sibling directions, following/preceding, self, ancestor-or-self, descendant-or-self, document identity, union deduplication, namespaces, text/comments/PIs, copied-document data and node distinction. In the holder cohort, original evidence survives typed-value paths and same-type decimal construction; cross-type integer construction and arithmetic have no inherited evidence. Baseline numeric/string/Boolean observations match. Root atomization through navigation raises FOTY0012.

For this finite fixture, the original snapshot ends with three element facts, nine regular wrappers and six namespace wrappers. Its copy has three facts, seven regular wrappers and zero namespace wrappers after the tested paths. These counts describe what was visited, not eager coverage of every possible namespace, total retained bytes or an accepted cache budget.

## Exact inputs and limits

The unchanged native six-file source compilation and license remain pinned in the parent [source record](../foundation-native-adapter/evidence/SOURCE.json) and archive; no owner checkout is modified or built. The runner compiles those exact native files inside the diagnostic assembly and invokes the real native factory path. This is not a production Schema package/assembly qualification or a fresh run of its complete suite.

Baseline loads the seven exact retained Core/API 1.34.2 package DLLs. Holder changes Core only using the unchanged prior overlay. [INPUTS.json](evidence/INPUTS.json), the four raw round files, build logs and retained holder source record exact generated inputs and actual assembly hashes. All seven assemblies load in Default, noncollectible contexts. [tools/verify.py](tools/verify.py) checks the sealed packet, source/input hashes, loaded layouts, expected evidence paths, repeated observations and counts.

Projection eagerly materializes delegated navigation sequences and caches wrappers per view. The adapter holds its opened views/trees until discarded; no production eviction, retained-memory limit, persistent resource lease, cancellation during lazy traversal or concurrent invalidation is implemented. Native instance limits bound this fixture, not arbitrary host sessions. No timing/allocation result, ABI acceptance, final carrier selection or final performance budget follows. The holder's known allocation/equality limitations remain.

Bound ISchemaAssessor/ISchemaAssessmentAdvisor composition, actual compiler-context capture, general constructor guards, user-defined types/facets/unions, XQuery/XSLT copies, XmlDb and Braid integration remain unqualified. No new public type/member was needed for this navigation slice, but that is not proof that the full foundation API can freeze. Core remains 1.34.2/c88eea43; C45 and four-owner freeze remain pending. No new production semantics or fresh full-product conformance results are claimed.

Next implementation should build the private trusted-issuer/evidence-read kernel behind V5 and wire authoritative constructor guards, with explicit snapshot ownership/cache bounds. Keep carrier selection private. Then qualify the real full binding and run the required unit/conformance gates before any production acceptance.

Reproduce with `python docs/foundation-snapshot-navigation/tools/run.py`; verify retained evidence with `python docs/foundation-snapshot-navigation/tools/verify.py`. Child exceptions are caught and build/process time is bounded. Earlier dossiers remain sealed and unchanged.
