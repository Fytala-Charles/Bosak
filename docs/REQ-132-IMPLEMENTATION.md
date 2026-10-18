<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak REQ-132: literal element source implementation">
<h1>REQ-132: literal element source implementation</h1>
<p>Source 1.20.0; unpublished - 18 October 2026</p>
</div>

## Delivered source profile

User authorized implementation after [Braid design review](REQ-132-CONSUMER-DESIGN-REVIEW.md) found no blockers at e6f21839. The report is imported byte-for-byte; its four-document/28-group assessment includes zero new-operation probes. The reported Braid 1,079-test baseline is not an implementation rerun, public-package verification or native acceptance. Source baseline is `e6f2183905739e2a303acc9ee1827004e1307041`; public packages remain 1.19.1.

Implements the [reviewed contract](REQ-132-LITERAL-ELEMENT-CONTRACT.md): append one paired empty no-namespace literal element by exact supported NCName. Inherited nonempty/default-XSLT namespaces receive a local xmlns="" reset, with no ancestor edits. Existing bindings, attributes and text remain protected. Namespace-alias contexts and unsupported static/module/mixed-content profiles refuse. No attributes, namespaced intent, arbitrary placement, wrapping, moves or self-closing-parent expansion are added.

The operation has independent capabilities and snapshot-owned target/proposal/result types. PreserveSource is default; Readable uses the existing bounded layout planner with its own 8,388,608-byte original/final physical bound. The pipeline separately validates captured scope and ordinary text profile, creates exact source bytes, reinspects and validates under denied resources, then proves total injective correspondence including child order, old text and attribute metadata/ranges. Old value-of/text/deletion implementations and layouts are not tightened.

The candidate returns actual InsertedNode/InsertedParent, exact LocalName/ElementName, paired InsertedRange, zero-byte BodyRange, complete ordered SourceEdits, snapshot-scoped NodeCorrespondence and immutable NewNodes partition. Namespace reset is an actual attribute descriptor; there is no fabricated namespace child, empty text node or Expression slot. Fresh actual text and value-of targets can populate the new element, including default-XSLT contexts without an existing nonempty XSLT prefix.

## Public use

Requires source/package 1.20.0 when delivered; these members do not exist in public 1.19.1. Given an existing immutable snapshot and its actual eligible parent descriptor:

```csharp
var target = snapshot.GetLiteralElementInsertionTarget(parent);
if (target.IsSuccess)
{
    var result = snapshot.ProposeLiteralElementInsertion(
        new AuthoringLiteralElementInsertionProposal(target.Target!, "item"));
    if (result.IsSuccess)
    {
        var candidate = result.Candidate!;
        byte[] proposedBytes = candidate.ExportSource();
        var nextTextTarget = candidate.Snapshot.GetTextInsertionTarget(candidate.InsertedNode);
        // Review the complete edits/evidence, then explicitly Apply under current consumer authority.
    }
}
```

Null arguments and undefined mode enum values are argument exceptions. Invalid name intent is the appended InvalidElementName failure (older enum values retained); unsupported namespace hazards are UnsupportedCoverage. Cancellation is cooperative/classified and returns no candidate before success. Consumers separately enforce latest revision/license/permissions, frozen review, atomic Apply, exact-byte history and late-cancellation semantics.

## Verification and evidence boundary

New source/public contract tests cover expanded runtime names and changed output, four physical encoding variants in both modes, subsequent text/value-of composition, Unicode and xml/xmlns local names, exact replay/partition/old metadata, refusals, limits including Readable shrinkage, immutability and concurrent isolation. Separate instrumented source controls test mid-traversal cancellation and inconsistent old comment/attribute metadata. The original 76-case run had nine fixture failures: eight assumed a single document result for valid template-level multi-root output and one assumed serializer spacing in empty tags. Corrected tests wrap fragments and assert XML semantics; original logs/TRX remain retained. No engine fix was needed for those fixture errors.

Intermediate build receipts also remain: initial sandbox output-directory denial, incorrect capability insertion inside an enum (fixed before successful build), online vulnerability metadata refreshed after a restricted restore, and a test analyzer predicate correction. No suppressions or analyzer reductions were introduced. Only final observed results are authoritative. Detailed final verification and package identity receipts are recorded in the delivery handoff and `.guard-tmp/req132-*`.

All 28 design acceptance groups remain mapped to engine or consumer responsibility. Braid UI/history/license/native cases are still consumer work; upstream tests do not certify them. No release tag, public NuGet upload, Braid pin/adapter/UI change or extension publication is part of this implementation step.

## Observed final source verification

| Check | Result |
|---|---|
| Solution build | Zero warnings and zero errors, no suppressions. |
| Final full solution | **9,824/0/0**, ten projects, including all 91 new cases. |
| Separate language server | **84/0/0**. |
| New operation tests | **91/0/0**: 85 public cases plus six instrumented cancellation/alignment controls. |
| Frozen QT3 / qt4 gated | **31,142/0/679** / **8,278/0/1,894**, zero gated failures. |
| Independent public-only package consumer | **374/0/0**: existing 289 plus 85 new cases; distinct non-friend assembly, no project references, fresh mapped local-package cache. |
| Broad package consumer | **153** checks pass across all ten packages in a separate fresh cache. |
| Exact committed candidate | All ten archives and nine focused/ten broad consumed assemblies identify source0677f060 and version1.20.0; archive/cache/output bytes, SHA-512, restore source and aligned dependencies verified. Both audits clear. |

[Machine-readable source proof](REQ-132-SOURCE-VERIFICATION.json) preserves exact hashes, identities and the unchanged design-review hash. Counts overlap; this is local unsigned candidate evidence, not published/signed-package verification. Exact-head CI must pass before merge. The completed delivery receipt `.guard-tmp/req132-implementation-delivery.json` records the observed final CI, merge and exact-merge package verification.
