# Contributing to Bosak

Thanks for your interest in Bosak — a pure-managed .NET XPath 3.1 / XSLT 3.0 / XQuery 3.1 engine.

## Getting started

```bash
git clone --recurse-submodules https://github.com/Fytala-Charles/Bosak.git
dotnet build Bosak.sln
dotnet test Bosak.sln
```

The `tests/qt3tests` submodule is required — unit tests read schema fixtures from it. The W3C `xslt30-test` suite is cloned on demand by `tests/Bosak.Xslt.Conformance` and is gitignored.

> **Windows note:** Application Control on some development machines blocks the rebuilt `Bosak.Xslt.Tests` assembly in its normal output directory. If `dotnet test` fails with `0x800711C7` for that project, use `powershell -ExecutionPolicy Bypass -File .\run-xslt-tests.ps1 -Configuration Release`.

## How to contribute

1. **Issues first.** Open an issue (bug report or feature request) before substantial work, so we can align on approach.
2. **Small PRs.** One logical change per pull request; a PR that fixes one bug is much easier to review than one that fixes five.
3. **Tests.** New functionality needs at least one happy-path and one edge-case test, following the existing `tests/Bosak.XPath.*.Tests` layout. All tests must pass before merge.
4. **Warning-free.** The build must stay at 0 warnings; treat new compiler warnings as errors.
5. **Conformance.** If your change touches the engine, run the relevant W3C sweep (`tests/Bosak.Xslt.Conformance`, `tests/Bosak.XPath.Conformance`) and report the before/after pass counts in the PR description.

## Conventions

- Every `.cs` file carries the standard header with a change-history row; bump the header version when you modify a file (see `AGENTS.md`).
- File-scoped namespaces; `readonly struct` for small value types; no `IEnumerable<T>` boxing on hot paths.
- Public APIs in `Bosak.XPath.Api` stay thin — delegate to the runtime layer — and carry triple-slash XML comments.
- Commit messages use conventional prefixes (`feat:`, `fix:`, `docs:`, `ci:`, `chore:`).

## Ground rules

- **Spec-driven behavior.** Engine behavior is defined by the W3C recommendations (XPath 3.1, XSLT 3.0, XQuery 3.1, Functions & Operators 3.1). When fixing a bug, cite the spec section or a W3C test that pins the expected behavior.
- **No customer/proprietary material.** Do not commit code, names, documents, or test data that identify customer projects.
- **Licensing.** By submitting a pull request you agree that your contribution is licensed under the same terms as the project (Apache-2.0, see `license.md`).

## Commercial support

Paid support, priority triage, and consulting are available — see `COMMERCIAL.md`. Commercial offerings never gate community issue reports or conformance fixes.

## Code of conduct

Be kind, be constructive. The project follows the [Contributor Covenant v2.1](CODE_OF_CONDUCT.md).
