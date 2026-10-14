<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.2.1 release preparation notes">
  <br><br><h1>Bosak 1.2.1</h1>
  <p>Namespace identity, authoring static context and key indexing corrections</p>
</div>

## Release scope

**Published on 2026-10-14.** Tag `v1.2.1` points to approved revision `70f5c5b2c1b18a238ad8b90b6cad204b840e460c`. The successful [release workflow](https://github.com/Fytala-Charles/Bosak/actions/runs/38525706208) passed 8,170 unit tests and published all ten public .NET 10 libraries. This coordinated patch introduces no new public API or conformance gate membership; existing compatibility profiles continue to govern draft 4.0 syntax.

The release includes the post-1.2.0 fixes integrated through PR #251-273:

- Align fixed namespace identities for variables/parameters, named templates, functions, modes and keys across compilation, execution and authoring diagnostics. Module bindings remain independent of local earlier grammar.
- Capture applicable authoring contexts for expressions, AVTs, patterns, named item/record types, sequence types and record fields, with include/import type linkage and retained QName metadata.
- Correct all-supplied, exact-arity XSLT function keyword binding and authoring signature validation. Expression edits use retained namespaces and signatures while preserving source bytes and the existing resource/refusal contract.
- Resolve exclude-result-prefixes="#all" to namespace URIs at the attribute owner, preserving descendant native rebindings on literal result elements. Fixed namespaces do not become native result namespace nodes.
- Compile key match/use expressions in their declaring static context, retaining namespace bindings, local grammar, defaults, base URI and existing schema/type/function metadata.

For example, a locally 3.0 key inside a fixed-binding 4.0 module now registers the same QName as its lookup, and a p:item key pattern selects the fixed-namespace node rather than a native-prefix counterpart. A descendant namespace rebound after a root #all exclusion is retained where required.

## Verification

Published-package verification is complete: all ten 1.2.1 packages downloaded from nuget.org pass version, dependency, repository revision, assets, .NET 10 assembly, XML documentation and SHA-256 checks. A fresh-cache consumer restored exclusively from nuget.org passes 60 checks and verifies all ten assembly versions as 1.2.1.0. Exact hashes and feed provenance are recorded in [publication evidence](RELEASE-1.2.1-PUBLICATION.json).

Final preparation candidate ed5080b5 passes 8,170 unit tests, including 5,284 XSLT tests, with no failures or skips. Frozen QT3 is 31,142/0/679 and qt4 gate-only is 8,278/0/1,894. Release build and pack succeed with zero errors and 81 existing warnings. All ten 1.2.1 packages pass version, dependency, .NET 10 assembly, XML documentation, README/license/icon and SHA-256 checks. A fresh-cache consumer with package references only passes 60 checks and verifies all ten assembly versions as 1.2.1.0; every Bosak dependency comes from the local candidate feed. Local package verification does not establish successful publication. Exact revisions, hashes and provenance are recorded in [preparation evidence](RELEASE-1.2.1-PREPARATION.json).

The submitted native key-indexing cases pass 2/0, and their expanded fixed-namespaces family passes 18/0/0. Both cases fail against retained pre-index-fix binaries, and namespace negative controls fail exactly those two cases. These selected results are not complete draft conformance.

## Upstream contribution status

Seventeen originally contributed cases remain accepted upstream in PR #22-27. Assertion correction PR #29 and two additional key-indexing cases in PR #30 remain under review; independent processor execution confirmation is pending. Original assertion, submission, acceptance and execution receipts are preserved. New submissions use current upstream bases fetched before branching and before submission, with rebasing required if the default branch advances.

## Boundaries retained

Broader package linkage, omitted/default host function execution, named-type expression-edit compilation, general pattern/AVT replacement, string-template interpolation and broader draft conformance remain separate work. REQ-125 acceptance and REQ-126 status are unchanged. Existing fixed-precision decimal, bounded date/time conversion and remote HTTP test limitations remain documented.

The VS Code Marketplace release is separate from this library release. The user explicitly approved tagging and publication after reviewing the exact verified revision. The tag and publication are complete.
