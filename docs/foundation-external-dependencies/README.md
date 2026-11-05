# F140-01 — External signature dependency review

30 October 2026. **Core inventory/proposal complete; owner disposition and API freeze pending.** Inspected candidate 1.34.2 assemblies match the exact hashes qualified by [the streaming cleanup slice](../streaming-completion-cleanup/README.md). Production source is unchanged at documentation base `c88eea43`; external owner projects were neither modified nor executed.

## Findings

The [typed metadata inventory](evidence/signature-dependencies.json) covers **all 3,506 public type/member declarations** from the [previous 5,766-entry baseline](../foundation-api-baseline/README.md). The remaining baseline entries are attribute/field-shape evidence, not additional public declarations. Each exact declaration retains the same assembly-plus-signature SHA-256 ID used in the [member draft](../foundation-api-contract-draft/README.md).

Recursive inspection includes generic arguments/definitions, array/byref/pointer elements, delegate Invoke parameter/return shapes, generic constraints and required/optional modifiers. It found **176 external named type definitions across eight resolved assemblies**. Within the previous conservative 40-type Bosak signature graph, **47 external definitions** occur, resolved to System.Private.CoreLib, System.Collections.Immutable and System.Private.Xml on this runtime.

| Resolved implementation assembly | Distinct external definitions |
|---|---:|
| `MediatR` | 5 |
| `OmniSharp.Extensions.JsonRpc` | 4 |
| `OmniSharp.Extensions.LanguageProtocol` | 92 |
| `System.Collections.Immutable` | 2 |
| `System.Private.CoreLib` | 54 |
| `System.Private.Uri` | 1 |
| `System.Private.Xml` | 12 |
| `System.Private.Xml.Linq` | 6 |

These are **observed runtime implementation identities**, not automatically NuGet dependencies, reference-pack identities or stable platform contract boundaries. Named definitions are deduplicated separately from constructed generic uses; counts include framework types such as Void and generic wrappers. The tool does not recursively inventory every external class member, inspect private method bodies, certify package closure, or execute provider logic.

There are **26 declarations exposing eight System.Xml.Schema definitions**, including accessors. Nine declarations inside the proposed type graph represent five logical members: CompileOptions.SchemaSet and EvaluationContext.SchemaSet/GetSchemaElement/GetSchemaAttribute/GetSchemaType. A whole-type freeze of the query options/context would therefore include legacy BCL schema contracts. See [the proposed boundary](BOUNDARY.md).

The three observed third-party assemblies (MediatR and two OmniSharp assemblies) are exposed only by LanguageServer declarations and not by the proposed type graph. The separately recorded csproj reference graph also excludes LanguageServer from the Core/API project closure. This is declared project-reference evidence, not an evaluated MSBuild/published-package dependency graph.

## Review artifacts

- [Boundary proposal and declared project graph](evidence/boundary-proposal.json): every declaration is classified; no owner is marked accepted.
- [Typed dependency witnesses](evidence/signature-dependencies.json): complete signatures, exact IDs, external type constructions and resolved assembly identities.
- [Boundary rationale](BOUNDARY.md): platform, existing XML adapters, legacy schema compatibility, bounded native bridge and future neutral contracts remain distinct.
- [Validation receipt](evidence/validation.json): source/artifact pins, deterministic repeat and tool checks.

| Proposed review group | Public declarations, including accessors |
|---|---:|
| ApplicationOutsideProposedFoundation | 211 |
| BoundedAssessmentBridgeReview | 13 |
| DraftMemberReview | 40 |
| ExistingCompatibilityWatch | 2189 |
| FoundationCandidateWithPlatformContracts | 787 |
| LegacySchemaCompatibilityReview | 26 |
| LegacyValidatorFacadeReview | 52 |
| XmlProviderAdapterReview | 188 |

These are proposed review groups, not new API categories in the product. In particular, Legacy does not mean removed, deprecated or waived from compatibility. Full validator/provenance capability cannot be inferred from the absence of a BCL schema type in one facade signature.

## Reproduce

From the repository root, write new inspection output to a separate path:

```powershell
dotnet run --project docs/foundation-external-dependencies/tools/Dependencies.csproj -c Release -- --self-test
dotnet run --project docs/foundation-external-dependencies/tools/Dependencies.csproj -c Release -- . docs/foundation-api-baseline/evidence/public-api.json .guard-tmp/foundation-external-current.json
python docs/foundation-external-dependencies/tools/review.py --check
python docs/foundation-external-dependencies/tools/verify.py
python -m unittest discover -s docs/foundation-external-dependencies/tools -p test_review.py -v
```

Six typed extraction checks and eight verifier/mutation tests pass; two complete inspections are byte-identical. Candidate assembly hashes and exact public declaration coverage match previous qualification. No production source/version/API changed, so the earlier 24,225 core tests and frozen gates are historical qualification, not rerun in this metadata-only step. The previous source/API/lifetime dossiers remain untouched.

Next core task: prepare and execute a small compatibility harness for the existing legacy schema and no-provider paths, including source/binary/default/error checks. It can qualify core-owned behavior while the delegated owners provide the required general Schema/XmlDb contract and consumer evidence. Do not select or implement new neutral C45 signatures through this review.
