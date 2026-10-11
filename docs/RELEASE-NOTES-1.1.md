<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.1.0 release notes">
  <br><br><h1>Bosak 1.1.0</h1>
  <p>Supported stylesheet authoring, controlled preview, and executable reference chapters</p>
</div>

## Release scope

This minor release follows 1.0.0 and retains the default XPath 3.1 compatibility
level. All ten public library packages use the coordinated version 1.1.0.
The target framework remains .NET 10. The package version comes from
`src/Directory.Build.props`, not from the Git tag.

## Stylesheet authoring and validation

REQ-124 provides public source envelopes, diagnostic spans, analysis/editing
contracts, and capability/version information in the existing Bosak.Xslt package.
REQ-125 adds static validation with explicit coverage: unsupported constructs
remain visible and cannot silently count as a complete validation pass.

## Controlled transformation preview

REQ-125 adds controlled resource acquisition with policy decisions and provenance
receipts, in-memory principal/secondary output capture, and cooperative
cancellation, time limits, and operation budgets. Cache hits are authorized on
every access and validated against approved content digests and effective URIs.
Terminal observations prevent a short cancelled or expired run from publishing
Success. Nested transformations inherit the same controls.

Byte-bounded capture enforces per-output and aggregate budgets while producing
encoded bytes for the documented text/XML/HTML/XHTML profiles, including BOM
accounting. Unsupported bounded profiles explicitly return XV0014: JSON/adaptive,
text Unicode normalization, non-text character maps, and unsupported markup
provider fallbacks. Unbounded capture retains the existing serializers.

These are cooperative engine controls. They do not preempt a single long XPath
evaluation or bound result-tree/XPath working memory. Hosts needing hard memory
and wall-clock limits must use worker-process controls. Transformation success
also does not imply business acceptance of partial documents.

Bosak.Braid accepted the declared contract and adopted correction revision
`0ff33163ba985bf8562722424db7e66c37e3566d` in its immutable dependency manifest.
Verification against the published 1.1.0 packages is the next consumer check;
production preview adapter/worker integration remains separate product work.

## Executable reference guide

Three worked chapters explain invoice mapping, XML-manual publishing, and
order/shipment reconciliation. Each includes complete input/output documents,
field mappings, annotated XSLT 3.0 stylesheets, used-element explanations,
and executable normal/edge cases. The feature catalog has 28 entries and
81 feature checks; chapter coverage adds 13 checks. REQ-126 remains In Progress
for the wider shipped/deferred inventory and traceability work.

See the [chapter reading path](xpath40-catalog/worked-examples/README.md),
[integration guide](INTEGRATION.md), and
[consumer acceptance record](REQ-125-END-TO-END-CONSUMER-REVIEW.md).

## XPath 4.0 adoption since 1.0

The frozen opt-in level gains operator access through `fn:op`, CSV parsing and
XML conversion, element/map conversion and plans, atomic equality, arity-zero
focus constructors, comparator sorting, atomic type annotation, JNode navigation,
and destructuring `let`. Related parser, QName ordering, and collation changes
are documented with compatibility notes in the integration guide and catalog.

The separate `XPath40Experimental` level introduces `fn:scan`; it is not part
of the default or frozen level. The qt4tests harness gates only promoted sets;
unpromoted exploratory failures remain outside the release gate.

## Compatibility and verification

XPath/XSLT 4.0 features retain explicit compatibility opt-in; experimental
features require their separate experimental level. This release also includes
the post-1.0 draft-feature adoption and fixes recorded in the integration guide.
Known platform limitations and coverage restrictions remain documented.

Release-candidate verification results and publication identities are recorded
in [the handover](AGENT_HANDOVER.md). Earlier 1.0 conformance sweep results are
historical evidence, not a newly executed complete sweep for 1.1.0.

| Release-candidate check | Result |
|---|---|
| Release solution build | 0 errors; 12 existing warnings |
| Full solution tests | 4,039 passed / 0 failed / 0 skipped |
| Frozen QT3 gate | 31,142 passed / 0 failed / 679 skipped |
| qt4 promoted sets | 8,278 passed / 0 failed / 1,894 skipped |
| Package metadata | All ten 1.1.0 packages; internal dependency versions, README, and license verified |
| Isolated Braid package consumer | 192 passed / 0 failed / 0 skipped against candidate packages |

The consumer-loaded XSLT assembly reports `1.1.0+<revision>` as its informational
version. Candidate-package testing does not change Braid's production source pin;
an additional check against the published artifacts follows publication.
