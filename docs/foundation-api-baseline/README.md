# F140-01 — Core API baseline and proposed freeze scope

30 October 2026. **Core inventory complete; joint requirements and freeze disposition pending.** This is the core portion of [REQ-140](../FOUNDATION-API-FREEZE-PLAN.md), prepared while the external owners execute their delegated work. No consumer checkout, dependency pin or production implementation is changed.

## Exact basis and results

Observed source: `f758a29f` (full revision and 245 source/build-file hashes in [baseline.json](evidence/baseline.json)), unpublished candidate version 1.34.1, Release/net10.0. A fresh full-solution build passed with zero errors and five existing nullable warnings in unchanged test/conformance files; [build log](evidence/build.txt) retains them. No unit suites, manual conformance gates, package publication or consumer execution were performed for this metadata audit.

[public-api.json](evidence/public-api.json) records **270 externally accessible types and 5,766 metadata entries across 11 production assemblies**. Entries include public/protected declarations, interface abstract/default method flags, generic constraints, parameter defaults and modifiers, nullable/custom attributes, enum constants and public value-type private field shapes. Counts include accessors/attribute/layout rows and are not a count of callable APIs. Compiler has no exported types. Reflection type filtering includes externally derivable protected nested types if present.

| Assembly | Exported types | Metadata entries |
|---|---:|---:|
| Bosak.LanguageServer | 23 | 400 |
| Bosak.XPath.Api | 10 | 189 |
| Bosak.XPath.Compiler | 0 | 0 |
| Bosak.XPath.Core | 25 | 807 |
| Bosak.XPath.Parser | 1 | 9 |
| Bosak.XPath.Providers.Database | 6 | 63 |
| Bosak.XPath.Providers | 15 | 370 |
| Bosak.XPath.Runtime | 23 | 747 |
| Bosak.XPath.Standard | 4 | 83 |
| Bosak.XQuery | 6 | 137 |
| Bosak.Xslt | 157 | 2961 |

## Proposed selection

The [type disposition inventory](evidence/proposed-type-scope.json) classifies every observed type; every owner disposition remains Pending and every frozen flag is false. Type grouping is a review queue, not a member-level freeze manifest.

- Core XDM and query entry APIs are foundation candidates. `XdmValue`, `CompileOptions` and other mixed surfaces need member-level selection because established XML behavior and current-draft additions coexist.
- Runtime context/function/resource types require dependency closure review: public signatures reference them even when the API project is thin. Namespace alone cannot establish assembly ownership (`XdmValueComparer` is in Runtime).
- Provider abstractions, in-memory adapters and the existing bounded assessment bridge require separate dispositions. Preserve current compatibility; do not turn bounded anonymous/decimal annotations into a full XSD 1.1 contract.
- Existing XSD facade and `System.Xml.Schema` projections remain compatibility inputs while C139-01/C45 defines the general neutral seam. No replacement signature is selected here.
- XSLT, XQuery, standard functions, parser error and database transport surfaces stay on the compatibility watch list. This proposal does not freeze every member in their assemblies. The LanguageServer application surface is outside the proposed shared foundation scope.

See [contract observations](CONTRACTS.md) and [gap/owner question register](GAPS.md). Required members cannot be added to existing consumer interfaces merely by calling them additive; default-member behavior, virtual slots, enums, layout, defaults and errors all need compatibility review. Existing compatibility commitments remain applicable outside the proposed scope.

## Reproduce and review

From the repository root, with .NET 10 and Python installed:

```powershell
dotnet build Bosak.sln -c Release --nologo
dotnet run --project docs/foundation-api-baseline/tools/Inventory.csproj -c Release -- . .guard-tmp/f140-api-current.json
python docs/foundation-api-baseline/tools/verify.py
```

Compare the newly generated inventory to the sealed [baseline](evidence/public-api.json); do not overwrite historical receipts when source changes. Identical source/toolchain should reproduce metadata and deterministic artifact hashes; toolchain identity is recorded in [SDK information](evidence/dotnet-info.txt). Another toolchain may change assembly hashes without a contract change and needs separate disposition. The verifier checks receipt integrity, source hashes, complete type classification and selected required/default/enum/layout/nullable boundaries. Two independent inventory executions were byte-identical at capture.

The audit tool is isolated from the solution and production packages. It loads local Release assemblies for reflection, without creating instances or executing consumer requests. Layout rows show metadata and field shape, not actual managed offsets, JIT size or ABI compatibility. It does not capture method behavior or prove all attribute semantics. Actual binary replacement, package identity, resource/lifetime behavior and both consumer sign-offs remain mandatory before freeze.

## Next core task

Prepare a member-level proposed contract manifest and compatibility-case matrix from this baseline, resolving the core-owned decisions in GAPS.md. Merge Schema and XmlDb owner requirements into that matrix when delivered. Do not implement new semantics before recording immutable specification adoption and the corresponding accepted scope. F140-01 as a whole remains in progress; F140-02 through F140-04 and the actual freeze remain pending.
