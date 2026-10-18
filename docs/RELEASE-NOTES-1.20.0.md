<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak Bosak 1.20.0 release notes">
<h1>Bosak 1.20.0 release notes</h1>
<p>18 October 2026</p>
</div>

**Published and verified:** immutable `v1.20.0` at `91f1af0d488bb2ed72b219118eeb1ab3570726bd`.

Bosak 1.20.0 adds bounded, source-preserving literal result element insertion for visual XSLT authoring.

- Create one paired empty no-namespace output element through an engine-owned target and exact local-name intent.
- PreserveSource remains the default; optional Readable layout discloses every changed span.
- A local namespace reset preserves the intended expanded name under inherited default namespaces.
- Complete source edits, old-node correspondence, new-node partition and an exact empty-body caret support review and selection.
- Populate the new container through fresh existing text/value-of targets in separate operations.

The bounded profile excludes attributes, namespaced-name intent, arbitrary placement, moves/wrapping and self-closing-parent expansion. Namespace-alias and unsupported static/module contexts refuse explicitly. Original/final source is limited to 8 MiB; supported structural encodings are UTF-8 and BOM-marked UTF-16 LE/BE.

Targets .NET 10. Braid implementation review found no blocking finding; Braid adoption/UI/history/native acceptance and the VS Code extension release remain separate.

[Public delivery proof](RELEASE-1.20.0-PUBLICATION.md) records signatures, source/package identity, fresh consumers and clear audits.
