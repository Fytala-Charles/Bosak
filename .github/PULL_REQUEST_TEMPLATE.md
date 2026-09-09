<!--
Thanks for contributing to Bosak! Keep PRs small and focused — one logical change per PR.
See CONTRIBUTING.md for conventions (file headers, tests, warning-free build).
-->

## Summary

<!-- What does this change do, and why? Link the issue it addresses (e.g. "Closes #123"). -->

## Conformance impact

<!-- If the change touches the engine, report the relevant W3C sweep numbers before/after:
     QT3 (tests/Bosak.XPath.Conformance) and/or XSLT (tests/Bosak.Xslt.Conformance).
     Example: "QT3 strict sweep 31,142/0/679 → 31,142/0/679 (no change)". -->

## Checklist

- [ ] `dotnet build Bosak.sln` — 0 warnings, 0 errors
- [ ] `dotnet test Bosak.sln` — all tests pass
- [ ] New functionality has at least one happy-path and one edge-case test
- [ ] Modified `.cs` files carry an updated change-history row in the header
- [ ] Public API changes have triple-slash XML documentation
