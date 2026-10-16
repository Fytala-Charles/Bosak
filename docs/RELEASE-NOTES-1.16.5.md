<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.16.5 published release">
  <br><br><h1>Bosak 1.16.5</h1>
  <p>Experimental XSLT 4 profiles and record subtype corrections — published 16 October 2026</p>
</div>

**Published and verified.** Immutable `v1.16.5` identifies approved revision `09940e90f702566107bb2eed521b7f8d789a6410`. [Publication verification](RELEASE-1.16.5-PUBLICATION.md); [GitHub release](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.16.5).

## New capability

The host can explicitly select an experimental XSLT 4 processor profile, independently of stylesheet version:

```csharp
using Bosak.Xslt.Api;

var compiler = new XsltCompiler
{
    ProcessorProfile = XsltProcessorProfile.Xslt40Draft
};
```

The default remains `Legacy`. The draft profile selects the supported 4.0 capability boundary for validation and forward compatibility, XPath 4 handling, and module-local fixed namespaces. The conformance runner exposes explicit legacy/draft profile selection. The opt-in public API and experimental pattern/JNode additions make this a minor-line release relative to 1.8.0; 1.16.5 is the released version.

## Corrections and experimental additions

- Structural record subtyping respects equal, case-sensitive field sets, field-type and cardinality variance, nested types, aliases, and record/map/generic-function relationships. Typed zero-argument function tests check return types and arity.
- Fixed namespace contexts work for earlier-version modules under the selected draft capability, with owned compilation trees and retained provider provenance.
- Unknown future draft instructions require fallback statically. Valid function-body fallbacks execute with raw items and isolated variable scope.
- XPath/XSLT array members use JNodes across decomposition and reconstruction, text atomization and summation. Experimental atomic, map, named-record and array patterns extend dispatch.
- Raw sequence, document, literal/computed element and copy construction preserve the appropriate item/result shape, focus and template-rule state. Copy and for-each validation follow their supported attribute contracts.

## Compatibility and limits

All ten libraries target `net10.0`. Existing source-preserving insertion APIs remain covered by public consumer checks. `Legacy` remains the host default; shared VM correctness fixes also apply to XPath and existing hosts.

XSLT 4 support remains experimental. The selected draft array/record/type-alias tests report **219 passed, 36 failed, 22 skipped** across 277 cases. The remaining failures comprise 33 catalog/draft mismatches and three unresolved contracts. This selected sample does not establish full XSLT 4 conformance. Typed callable-record details, ambiguous literal-prefix serialization and previously documented numeric, date and resource limits remain outside this milestone. Obsolete `xsl:record`, retired `xsl:field/@required` behavior and expected-error aliases were not added to satisfy historical fixtures.

The engine milestone passes 9,577 solution tests, 84 language-server tests, ten CLI controls, QT3 31,142/0/679 and qt4 gated 8,278/0/1,894. Fresh public-package consumers pass 153 broad checks and 338 public focused tests, with archive/cache/output/assembly provenance and dependency advisory checks. Exact workflow revision, public archives, fresh-cache output DLLs and assembly metadata are verified in the publication receipts.

Public libraries are 1.16.5. VS Code 0.2.4 continues to bundle 1.8.0; extension alignment and Braid adoption are separate actions.
