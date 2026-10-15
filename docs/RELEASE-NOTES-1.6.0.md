<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.6.0 release candidate">
  <br><br><h1>Bosak 1.6.0</h1>
  <p>Optional function overrides in used packages</p>
</div>

## Release status

**Prepared on 2026-10-15; unpublished.** This coordinated .NET 10 minor release adds bounded optional function overrides, integrated by [PR #293](https://github.com/Fytala-Charles/Bosak/pull/293) at `6dcc9671a7b28cd1b55ec80650e54249f3e44339`. Published NuGet libraries remain 1.5.0. VS Code 0.2.0 bundles engine 1.4.0.

## Changes

- Public used-package functions with trailing optional parameters can be overridden when required and total parameter counts and declaration metadata are compatible. Direct, arrow, keyword, named-reference, lookup and positional partial calls dispatch across the callable range.
- Replacement defaults and original defaults retain their own declaring scope. Keyword calls preserve the original public parameter names while binding the replacement's local names.
- `xsl:original` supports the original optional arity range inside the overriding function. Named original references and positional partials remain bound after returning or while another override executes. Dynamic lookup and `function-available` hide the pseudo-name.
- Original and replacement memoization use distinct declaration identities. Overrides called from a library re-enter their declaring globals, private helpers and decimal formats.

For an original whose second parameter defaults to `'B'` and a replacement whose second parameter defaults to `'X'`, a one-argument public call uses `'X'`. A one-argument `xsl:original` call inside the replacement uses `'B'`. See the [implementation contract](XSLT40-OPTIONAL-OVERRIDES.md) for focus and function-item behavior.

## Verification

[Merged-revision CI](https://github.com/Fytala-Charles/Bosak/actions/runs/38685404777) passes **8,372 solution tests**, including **5,486 XSLT tests**, plus **84 language-server tests** and extension checks. The Release build has zero errors and 82 existing warnings. QT3 remains exactly **31,142/0/679**; qt4 gate-only remains **8,278/0/1,894**.

Ten local 1.6.0 archives from that exact merge revision pass version, dependency, repository revision, net10 assets, XML documentation, README/license/icon and hash checks. A fresh package-only consumer passes **74 tests**: 41 override regressions and 33 existing used-package cases. Nine engine dependency assemblies have version **1.6.0.0**, exact merge-revision informational metadata and bytes matching the candidate archives. Source mapping, fresh-cache provenance and archive hashes are verified. Source and test Git trees match the reviewed implementation commit.

[Preparation evidence](RELEASE-1.6.0-PREPARATION.json) preserves those results. The 41-case published 1.5.0 baseline passes one existing error case and fails 40. A separate fresh-cache broad consumer now passes **153 checks** against all ten 1.6.0 libraries. Its ten DLLs match the candidate archive bytes and exact engine-revision metadata. This rerun retains the earlier 120 broad checks and executes the 33 used-package checks from the current committed fixture, including the two updated direct/transitive override cases. The 74-test consumer overlaps those 33 cases; these are separate runs, not 227 distinct tests. The broad archives include the preparation README from the working tree; final merged-revision repacking remains required by the [publication checklist](RELEASE-1.6.0-CHECKLIST.md). Local candidate verification does not establish public-feed availability.

## Boundaries

Optional abstract originals and overlapping adopted/local optional families remain explicit unsupported boundaries. Controlled editor validation still reports package-dependent signatures as unsupported coverage; module acquisition is unchanged. Escaping inline closures that defer a direct original call remain outside the tested contract; capture a named original item first.

No public API or conformance gate membership changes. REQ-126 remains In Progress. Existing decimal precision, extended-year conversion and remote HTTP limitations remain documented. Braid's isolated 836-test result applies to public 1.5.0; no 1.6.0 consumer adoption or independent acceptance is claimed.
