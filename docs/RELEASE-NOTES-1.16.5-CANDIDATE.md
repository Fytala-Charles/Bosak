<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.16.5 candidate release notes">
  <br><br><h 1>Bosak 1.16.5 candidate</h 1>
  <p>Experimental XSLT 4 profiles and record subtype corrections</p>
</div>

**Unpublished candidate**, prepared 16 October 2026. Revision`c95b56c0b18e4493c31f2285bbe98621c1027655`. [Verification and limitations](RELEASE-1.16.5-CANDIDATE-VALIDATION.md). These notes summarize source changes since published 1.8.0; no publication is implied.

## New capability

The host can explicitly select an experimental XSLT 4 processor profile, independently of stylesheet version:

```csharp
var compiler = new XsltCompiler
{
    ProcessorProfile = XsltProcessorProfile.Xslt40Draft
};
```

The default remains Legacy. The draft profile selects the supported 4.0 capability boundary for validation/forward compatibility, XPath 4 handling and module-local fixed namespaces. The conformance runner exposes explicit legacy/draft profile selection. The new opt-in API and pattern/JNode capabilities make this a minor-line candidate relative to 1.8.0;1.16.5 is the current unpublished source version.

## Corrections and experimental additions

- Structural record subtyping respects equal, case-sensitive field sets, field-type/cardinality variance, nested types, aliases and record/map/generic-function relationships. Typed zero-argument function tests check return types and arity.
- Fixed namespace contexts work for earlier-version modules under the selected draft capability, with owned compilation trees and retained provider provenance.
- Unknown future draft instructions require fallback statically; valid function-body fallbacks execute with raw items and isolated variable scope.
- XPath/XSLT array members use JNodes across decomposition/reconstruction, text atomization and summation; experimental atomic, map, named-record and array patterns extend dispatch.
- Raw sequence, document, literal/computed element and copy construction preserve the appropriate item/result shape, focus and template-rule state. Copy and for-each validation follow their supported attribute contracts.

## Scope and compatibility

All ten libraries target net10.0. Existing source-preserving insertion APIs remain covered by the fresh public consumer checks. Legacy remains the host default, but shared VM correctness fixes also apply to XPath and existing hosts. This candidate is not a full XSLT 4 conformance release: selected draft tests remain 219/36/22, with 33 catalog/draft mismatches and 3 unresolved contracts. Typed callable-record details, ambiguous literal-prefix serialization and previously documented numeric/date/resource limits remain outside this milestone. No obsolete xsl:record instruction, retired field/@required behavior or expected-error alias was added to make historical tests pass.

Source 9577/0/0, language-server 84/0/0, ten CLI controls and both frozen gates pass. Fresh candidate consumers pass 153 broad checks and 338 public focused tests; archive/cache/output/assembly provenance and dependency advisories verify. See the linked receipt for exact revision, historical evidence and coverage boundaries. Published libraries 1.8.0 and extension 0.2.4/engine 1.8.0 are unchanged.
