# F140-01 — Member-level contract draft

30 October 2026. **Proposal only; joint requirements, accepted scope and API freeze remain pending.** This is the next core deliverable under [REQ-140](../FOUNDATION-API-FREEZE-PLAN.md). External work remains delegated to the Schema and XmlDb owners; no external checkout was modified or executed.

## Scope and immutable inputs

The [member manifest](evidence/member-manifest.json) assigns a unique SHA-256 entry ID, declaring assembly/type, zero-based baseline row, review group and compatibility-case links to **all 5,766 entries** of the [previous compiled baseline](../foundation-api-baseline/README.md). That baseline pins production source `f758a29f368201abdb98af003274451c84a9b7c4`, candidate 1.34.1. This draft was prepared from core documentation checkpoint `bed37596c91428dc0517c79ff6309299106aea25`; it does not claim another compiled engine candidate.

The entries comprise 3,236 member declarations (including accessors), 270 types and 2,260 attribute/field-shape evidence rows. Evidence-only rows are marked explicitly and do not create callable APIs. Full signatures/defaults/attributes stay in the immutable baseline rather than being copied into a second signature format. An overload ID hashes the assembly plus the complete baseline row, not just its method name.

| Proposed review group | Entries, including attached evidence |
|---|---:|
| CompatibilityWatch | 3228 |
| DraftMemberReview | 65 |
| FoundationCandidate | 806 |
| OutsideProposedFoundation | 400 |
| ProviderContractReview | 312 |
| RuntimeDependencyReview | 725 |
| SchemaContractReview | 230 |

[Scope rules](scope-rules.json) route review work using type/member names and observed signature dependencies. These groups are conservative triage, not a claim of automatic stable/draft semantic classification. Current-draft JNode/record members and BCL/bounded schema surfaces remain explicit review queues; no existing member or compatibility obligation is removed. Core, Schema and XmlDb dispositions all remain Pending; no entry is frozen.

## Public signature closure finding

The [conservative type graph](evidence/signature-closure.json) has 469 directed edges across the observed surface. Starting with all 25 Core types and the four query entry types reaches **40 Bosak public types**, including these **11 Runtime dependencies** beyond the initial 29:

- `Bosak.XPath.Runtime.Functions.FunctionSignature`
- `Bosak.XPath.Runtime.Functions.XPathFunction`
- `Bosak.XPath.Runtime.Resources.ControlledResourcePolicy`
- `Bosak.XPath.Runtime.Resources.ControlledResourceReceipt`
- `Bosak.XPath.Runtime.Resources.ControlledResourceReceiptOutcome`
- `Bosak.XPath.Runtime.Resources.ControlledResourceRequest`
- `Bosak.XPath.Runtime.Resources.ControlledResourceResponse`
- `Bosak.XPath.Runtime.Resources.ControlledResourceRoute`
- `Bosak.XPath.Runtime.Resources.IControlledResourceAuthority`
- `Bosak.XPath.Runtime.Vm.DecimalFormat`
- `Bosak.XPath.Runtime.Vm.EvaluationContext`

The parameter of `XPath31Expression.Evaluate(EvaluationContext)` is one direct witness; following the context's resource-policy signatures reaches `IControlledResourceAuthority`. A scope limited to Core and API assembly names would miss these contracts. Review their actual member selection before accepting a self-contained foundation surface.

This graph deliberately includes every observed declaration of reached types, including draft/deferred members. It is an upper-bound review inventory, not the final member-filtered closure. It excludes attribute-only and private field-shape rows as public-signature edges; BCL/external package types, IL dependencies, unmanaged layout, exceptions thrown and behavioral ownership still require separate review. The 11 additional types are not newly accepted APIs.

## Compatibility matrix and disposition

[Twenty planned compatibility cases](COMPATIBILITY-MATRIX.md) map source/binary compatibility, constants/layout, identity, atomization, lazy/nested/single-pass values, semantic capture, resources, PSVI, ordinary persistent XML, errors, concurrency, packages, frozen gates and drift. Every case is **NotRun**, with no result receipt. Tool verification below is distinct from executing these engine/consumer cases.

The nine [open decisions](../foundation-api-baseline/GAPS.md) remain open. This draft advances FG-01/04/07 preparation; it does not settle the general C45 schema seam or the owners' requirements. No semantic adoption, API change, consumer dependency upgrade, storage mutation, release or freeze is authorized by this draft.

## Reproduce and validate

From the repository root:

```powershell
python docs/foundation-api-baseline/tools/verify.py
python docs/foundation-api-contract-draft/tools/compile_manifest.py --check
python docs/foundation-api-contract-draft/tools/verify_manifest.py
python -m unittest discover -s docs/foundation-api-contract-draft/tools -p test_manifest.py -v
```

`--check` regenerates in memory and refuses drift without rewriting the committed drafts. Omit it only when intentionally producing a new reviewed draft. The independent verifier checks one-to-one coverage, exact IDs, interface slots, cases and parameter/transitive dependencies. Ten tool tests include missing/duplicate entries, lost parameter/closure edges, fabricated case results and false freeze acceptance. These are audit-tool tests, not runtime conformance qualification.

No new build, unit suite or conformance gate was needed for unchanged engine code; the preceding baseline's Release build remains historical. Validation and artifact/tool hashes are recorded separately in [validation.json](evidence/validation.json).

## Next core task

Prepare a focused lifetime/ownership decision note for lazy and escaped results (FG-04), using actual engine fetch/early-exit paths and existing provider behavior. Identify whether current owner-bound results suffice or an optional compatible adapter is needed. Keep implementation pending the selected scope and owner requirements. This can proceed independently of Braid product validation and without changing either external project.
