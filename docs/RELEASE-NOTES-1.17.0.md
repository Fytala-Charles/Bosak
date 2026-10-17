<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.17.0 publication">
<h1>Bosak 1.17.0 release notes</h1>
<p>Readable insertion release — 17 October 2026</p>
</div>

Published from immutable tag `v1.17.0` at `15eacde48b47cc8e48af29ddc95949801a2e3e5d`. All ten libraries target .NET 10.

Adds opt-in readable value-of insertion for REQ-129. The existing two-argument constructor preserves its byte-for-byte insertion behavior. The new Readable mode formats only supported direct-child whitespace gaps and returns exact source edits, a fresh candidate snapshot and complete surviving-node correspondence.

Compact layouts use a two-space fallback; supported existing indentation and uniform newlines are retained. Ambiguous layouts, mixed content and unproven contexts refuse without a silent fallback. Readable also refuses beneath any ancestor xml:space=preserve, including a nearer default reset, because execution testing found observable output whitespace there. Existing resource, encoding, cancellation and size limits remain.

The user reports that Braid found no blocking issues. This publication does not change Braid's sources or dependency pin. VS Code 0.2.5 continues to bundle engine 1.16.5; extension alignment is separate.

Validation: 9,612 solution tests and 84 language-server tests pass; frozen QT3 remains31,142/0/679 and qt4 gated8,278/0/1,894. Fresh nuget.org-only public package consumers pass 153 broad checks and 372 public focused cases with exact archive/cache/output/assembly provenance and no reported vulnerable dependencies. All ten public packages pass signature verification; signed archive/cache/output payloads and exact assembly revisions verify.

[Implementation and API example](https://github.com/Fytala-Charles/Bosak/blob/15eacde48b47cc8e48af29ddc95949801a2e3e5d/docs/REQ-129-IMPLEMENTATION.md). [Change PR](https://github.com/Fytala-Charles/Bosak/pull/357).


## NuGet packages

[Bosak.XPath.Api](https://www.nuget.org/packages/Bosak.XPath.Api/1.17.0), [Bosak.XPath.Compiler](https://www.nuget.org/packages/Bosak.XPath.Compiler/1.17.0), [Bosak.XPath.Core](https://www.nuget.org/packages/Bosak.XPath.Core/1.17.0), [Bosak.XPath.Parser](https://www.nuget.org/packages/Bosak.XPath.Parser/1.17.0), [Bosak.XPath.Providers](https://www.nuget.org/packages/Bosak.XPath.Providers/1.17.0), [Bosak.XPath.Providers.Database](https://www.nuget.org/packages/Bosak.XPath.Providers.Database/1.17.0), [Bosak.XPath.Runtime](https://www.nuget.org/packages/Bosak.XPath.Runtime/1.17.0), [Bosak.XPath.Standard](https://www.nuget.org/packages/Bosak.XPath.Standard/1.17.0), [Bosak.XQuery](https://www.nuget.org/packages/Bosak.XQuery/1.17.0), [Bosak.Xslt](https://www.nuget.org/packages/Bosak.Xslt/1.17.0)
