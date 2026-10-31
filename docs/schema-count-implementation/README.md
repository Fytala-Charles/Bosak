# Count assertion implementation candidate — 29 October 2026

Schema 0.8 implements the [bounded count scope](../schema-count-assertions/README.md). Read the [current guide snapshot](GUIDE.md) and [implementation adoption](ADOPTION.md). Owning source/guide remain in the persistent Schema feature worktree; earlier sealed review and normative scope evidence are unchanged.

Unprefixed count(DeclaredChildName) is admitted as a scalar comparison operand; bare count child names keep selection semantics. Immutable metadata is returned only after a complete parse, selecting v4 only for admitted calls. Legacy v1/v2/v3 remain for no-count schemas. Native structure/decimal checks, original state/copies, error policy, bounds and product pins are unchanged. Prefix/alias/function/argument expansion remains explicitly Unsupported.

Fresh complete Schema qualification: 397 passes, 63 new count tests, zero-warning/error source-override build and successful native-count example. The original 57-case before cohort has 23 passes/34 failures, now all passing. The one count refusal is deliberately adopted with source/receipt preservation; 333 other prior expectations remain unchanged. RESULTS.json records source identities and inherited core/gate qualification; no fresh full-core/gate run or full XSD 1.1 certification is claimed.

## Candidate execution and state

Source bases: core `da6c08141c03af34b537599b3ff5eadb77240ac9`; Schema `2f4bf417131ba70e1653eda3d9148d83a6ef6d8a`, branch `feature/bounded-document-assessment`. Current Schema source/docs are uncommitted; core production version remains 1.34.1.

```powershell
dotnet test .worktrees/Bosak.Schema.Assessment/Bosak.Schema.slnx -c Release -p:BosakSourceRoot=D:/Development/Bosak
dotnet run --project .worktrees/Bosak.Schema.Assessment/examples/Bosak.Schema.Assessment.Example -c Release -p:BosakSourceRoot=D:/Development/Bosak
```

The example validates repeated count and optional absence using real native assertions, retaining typed ordered copies and original state. The source override is required while core dependencies are unpublished. No commit/push, merge, tag, package or consumer adoption is included. Next: bounded review/checkpoint.

Final count-candidate qualification: **397 complete Schema cases pass**, including **63 new count/grammar/validation-order/copy cases**. The original 57-case cohort (23 passed/34 failed) now all passes; six further profile/optimization-boundary cases pass. The prior 334-case suite retains **333 unchanged passing expectations** plus the explicitly adopted count capability check. Source-override build has **zero warnings/errors**. The actual native count/absence example and strict documentation checks pass. Core source/tests/harness/fixture references and build configuration remain identical, so **24,214 core passes**, QT3 **31,397/0/424**, qt4 **8,941/0/1,918** (230 sets) are inherited, not fresh full runs. All earlier sealed evidence and original refusal source/receipts remain unchanged.
