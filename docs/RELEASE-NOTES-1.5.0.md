<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.5.0 release preparation">
  <br><br><h1>Bosak 1.5.0</h1>
  <p>Optional arguments in used-package functions</p>
</div>

## Release scope

**Candidate prepared on 2026-10-14; unpublished.** This coordinated .NET 10 minor release includes the bounded used-package optional/default function support integrated by PR #288. Published NuGet libraries remain 1.4.0. The separately published VS Code extension is 0.2.0 and bundles engine 1.4.0.

- Direct, arrow and keyword calls, named references, lookup and positional partial application expose the declared optional arity range of public/final functions in a used package.
- Defaults retain the declaring package's namespaces, globals and private helpers while using the caller focus. Function items retain their creation focus. Constructor defaults and XPath conversion errors preserve the ordinary function contract.
- Accept/expose names identify the component by its upper arity bound. Accepting it as private keeps all callable arities available locally; hiding it hides the whole range. Private helpers remain isolated from caller lookup.
- Nested used packages preserve declaring scope, and positional XSLT 3.0 callers can use defaults from an XSLT 4.0 library.

For a public `p:join` with one required parameter and a second parameter defaulting to `'B'`, `p:join('A')` returns `AB`, while `p:join('A','X')` returns `AX`. The accept/expose component name is `p:join#2`. See the [bounded feature contract](XSLT40-USED-PACKAGE-OPTIONAL-FUNCTIONS.md).

## Verification

The ten local 1.5.0 packages built from `ac891b16c8402c5614639898de5619434e45676d` pass version, internal dependency, repository revision, net10 assets, XML documentation, README/license/icon and SHA-256 checks. A fresh package-only consumer passes **153 checks**: the 120 retained 1.4.0 consumer checks plus all 33 used-package feature and boundary regressions executed against package references. All ten assembly versions are **1.5.0.0**. Package source mapping routes every Bosak package to the local candidate feed; cache bytes and SHA-512 records match the candidate archives. The initial sandbox restore reported NU1900; an unrestricted restore successfully retrieved metadata without that warning. Both logs are retained.

Release build passes with zero errors and 82 existing warnings; language-server tests pass 84/0/0. Frozen QT3 remains exactly **31,142/0/679** and qt4 gate-only remains **8,278/0/1,894**, with zero gated failures. Integrated engine CI passed 8,331 solution tests. The unrestricted local full suite passes **8,331/0/0**, including **5,445 XSLT tests**. The initial sandbox run failed to start the database tests' local HTTP server and was stopped; its log is retained separately. [CI for the package source revision](https://github.com/Fytala-Charles/Bosak/actions/runs/38625061782) also passes all solution and language-server tests plus extension checks.

[Preparation evidence](RELEASE-1.5.0-PREPARATION.json) records the exact candidate revision, package/content hashes, feed provenance, code trees and verification logs. The source and test trees match the integrated feature receipt. Preparation documentation must be integrated and the final release revision repacked and checked before publication approval.

## Boundaries

Optional `xsl:override` and its `xsl:original` interactions remain explicitly unsupported, including in transitive packages. Overlapping optional ranges between adopted components or local/adopted definitions remain an explicit `NotSupportedException` boundary; hiding a conflicting family resolves that boundary and disjoint families remain callable. Controlled authoring validation still reports package-dependent signatures as unsupported coverage. Broader package conflict/override semantics and module acquisition in editor validation remain separate work.

No public API or gate membership changes. REQ-126 remains In Progress. Existing decimal precision, extended-year conversion and remote HTTP limitations remain documented. No upstream submission, acceptance or new independent consumer verification is claimed. Historical receipts and scratch evidence are preserved; `.sweep-baselines/` remains pending a later version-control decision.
