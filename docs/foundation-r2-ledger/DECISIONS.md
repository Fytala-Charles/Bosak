# Core decision rules

These rules select the next review draft. They are not owner acceptance or normative semantic adoption. Record immutable specifications/sections/deviations before subsequent semantic implementation.

## D01 Ordinary input admission

Optional assessment means ordinary XML is admitted without an assessor/origin. Retain ISchemaAssessmentInput as a neutral borrowed input; add bool OwnsNode(IXdmNode) before node getters. Never invent assessment evidence for XmlDb.

## D02 Node-fact association

Retain OwnsNodeFacts and additionally verify the exact node/facts/input/origin association using the new advisor OwnsNode overload. Issued facts alone cannot authenticate another node.

## D03 Source-specific typed values

ReadTypedValue requires a SchemaQuerySnapshot argument. Advisor OwnsTypedValue additionally receives admitted input and optional original assessment; issuance is checked before container getters. Fresh construction uses ConstructSimpleValue and does not mint original assessment.

## D04 Bounded dynamic context

Retain Evaluate(EvaluationContext) and XSLT Transform context parameters, but capture supported data internally under the operation. Refuse unsupported services before evaluation. Do not publish a new arbitrary input-capture factory or silently use raw context.

## D05 Host ownership and operation lifetime

Retain IQueryOperationLifetime/Lease and same-session immutable Run membership. Close revokes admissions; Drain waits for raw work. No provider/store disposal transfer or required Dispose on IXdmNode/sequence/managed assessed document. Host/provider must retain actual resources through raw work.

## D06 All-host invocation

Retain separate match/global snapshot roles, explicit empty membership for source-free named templates, and one lease per unique admission. Initial document roots are the first bounded profile; modes, resource policies, arbitrary initial sequences and streaming refuse when not captured/supported. Broader required Braid signatures must be resolved before freeze.

## D07 Authority and identity

Retain opaque source/snapshot/assessment/component identities and owner membership checks. Complete closure and supported false/empty remain distinct from Unsupported. Descriptors, QNames and GUID equality never admit authority.

## D08 Exact numeric representation

Retain XdmExactDecimal and all six XdmValue numeric member intents. Integrate arbitrary-precision parsing/arithmetic/casts/map keys/serialization before claiming support. Retain legacy signatures; wrong-kind checks remain, narrowing refuses overflow/inexact values. No annotation string issues authority.

## D09 Atomic/list/union and original PSVI

Retain governing list/union shape and per-atomic selected type/provenance, including empty/singleton lists. Preserve original facts or explicitly strip. Nil/no-typed-value differ from empty typed value. Unsupported backend coverage is explicit and cannot remove required contract representation.

## D10 Assessment and compatible views

Retain captured environment, advisor/episode, complete valid-only validated roots, diagnostics and directional compatible-view contracts. An unimplemented backend route returns Unsupported; compatibility is never inferred or fabricated. Positive supported profiles and intentional refusals need actual owner acceptance.

## D11 Failure preservation

Retain original work/admission/cancellation failure first, then every cleanup failure in reverse release order, without flattening provider exception objects. XmlDb cleanup masking is an owner implementation/profile issue, not justification for a new Core disposal API.

## D12 Finite limits and resource policy

Retain required positive finite query counts/numeric digits and nonnegative decimal scale; no workload defaults selected. Diagnostic limits and public record inputs require constructor/runtime validation. Captured resource policies must freeze or refuse mutable capabilities; no implicit source acquisition.

## D13 Existing compatibility

Retain all existing public declarations/constants/defaults, optional node defaults and persistent format behavior unless a separately adopted semantic change explicitly says otherwise. No mandatory schema channel on legacy IXdmNode, no new required enumerator disposal member, and no XmlDb format migration.

## D14 Performance and scope restraint

Require actual owner PF-01–09 workloads, budgets and default-path checks before freeze. Keep internal batching/caches private; no speculative public batch/cursor/group/acquisition SDK or proprietary licensing seam. Record any demonstrated remaining signature need before acceptance.
