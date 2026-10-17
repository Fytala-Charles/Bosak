<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.18.0 public release">
<h1>Bosak 1.18.0 release notes</h1>
<p>Published value-of deletion release — 17 October 2026</p>
</div>

Published from immutable tag `v1.18.0` at `3264fcfaf739e492770c882d2cbfc1f93f0172d2`. All ten libraries target .NET 10. [Verified public delivery](RELEASE-1.18.0-PUBLICATION.md) and [GitHub release](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.18.0).

## Bounded source-preserving deletion

REQ-130 adds engine-issued snapshot-owned targets and proposals for removing one existing empty-body xsl:value-of in the supported single principal-module, template/literal-result constructor profile. Imported and previously inserted instructions are supported, including ordinary indented/readable source. Only the instruction's exact source bytes are removed; indentation/blank lines, comments, PIs, namespace spelling, encoding/BOM and explicit now-empty parent tags remain unchanged. No whitespace cleanup or general subtree deletion is included.

The isolated ordinary candidate supplies exact removal/empty replacement ranges, immutable edits, removed descriptor evidence, a proven surviving parent and complete survivor provenance. One-to-one correspondence uses actual descriptor references. When the two neighboring ordinary whitespace nodes merge, a separate bounded two-fragment relation proves their retained source bytes and the actual candidate text node. The engine owns XML character-reference/newline parsing; consumers verify ownership, graph partitions and exact byte replay without implementing a parallel parser.

## Compatibility and limits

Existing insertion/expression APIs, formatting defaults and failure enums remain unchanged. Deletion has separate capability/target/proposal/result/failure types. Null programmer arguments throw; known exclusions return classified refusal; observed deletion cancellation throws OperationCanceledException (the older insertion API retains its classified cancellation behavior). Older immutable snapshots remain usable independently; the consumer enforces its current revision/licensing/Apply authority.

Original and candidate source are bounded to 8,388,608 bytes including BOM. Supported encodings retain strict UTF-8 BOM/no-BOM and BOM-marked UTF-16 LE/BE declaration rules. Empty-body self-closing/explicit instruction spelling is supported; body trivia/content refuses. Dependencies, unsupported static/resource/type/expression profiles, significant mixed/CDATA contexts and any xml:space=preserve ancestor (including a nearer default reset) remain outside this slice. Deleting value-of intentionally changes output; output equivalence is not required.

## Verification and consumer state

Upstream full build and **9,661 solution +84 language-server tests** pass. Focused source **226** passes (49 deletion +177 existing insertion/readable). Frozen QT3 remains **31,142/0/679**, qt4 gated **8,278/0/1,894**. Independent local-package consumers on exact final head and merge each pass **221** (45 public deletion +175 existing public insertion +one metadata case), with ten archive/nine assembly/revision/cache/output/restore provenance checks and no reported vulnerable dependencies.

[Braid's independent implementation review](REQ-130-CONSUMER-IMPLEMENTATION-REVIEW.md) reports **no blocking finding** within this bounded profile. It independently reports 226 source-focused, 221 unchanged package and 241 combined package/probe passes; these overlap and must not be added. The 241 includes 20 new Braid probes for repeated deletion, four encodings/newline seams, insertion/deletion round trips and ordinary-file export/hash/reopen/run. These use unpublished local artifacts, not public release packages; this session does not claim to rerun Braid tests.

Braid's production pin remains accepted public 1.17.0. Its deletion-specific adapter, frozen review/Apply/history/selection, licensed worker/restricted export/reopen workflow and native acceptance remain open. Its implementation review is not release authorization or functional acceptance. VS Code 0.2.5 still bundles 1.16.5. No Braid or extension update is part of this release preparation.

[Implementation and API details](REQ-130-IMPLEMENTATION.md), [implementation PR #361](https://github.com/Fytala-Charles/Bosak/pull/361), [release readiness and execution boundary](RELEASE-1.18.0-READINESS.md).

## Public artifact verification

All ten signed NuGet packages verify; fresh nuget.org-only consumers pass 153 broad and 221 focused public checks with exact archive/cache/output/assembly provenance and no reported vulnerable dependencies. This verifies publication, not Braid production/native acceptance.

[Bosak.XPath.Api](https://www.nuget.org/packages/Bosak.XPath.Api/1.18.0), [Bosak.XPath.Compiler](https://www.nuget.org/packages/Bosak.XPath.Compiler/1.18.0), [Bosak.XPath.Core](https://www.nuget.org/packages/Bosak.XPath.Core/1.18.0), [Bosak.XPath.Parser](https://www.nuget.org/packages/Bosak.XPath.Parser/1.18.0), [Bosak.XPath.Providers](https://www.nuget.org/packages/Bosak.XPath.Providers/1.18.0), [Bosak.XPath.Providers.Database](https://www.nuget.org/packages/Bosak.XPath.Providers.Database/1.18.0), [Bosak.XPath.Runtime](https://www.nuget.org/packages/Bosak.XPath.Runtime/1.18.0), [Bosak.XPath.Standard](https://www.nuget.org/packages/Bosak.XPath.Standard/1.18.0), [Bosak.XQuery](https://www.nuget.org/packages/Bosak.XQuery/1.18.0), [Bosak.Xslt](https://www.nuget.org/packages/Bosak.Xslt/1.18.0)
