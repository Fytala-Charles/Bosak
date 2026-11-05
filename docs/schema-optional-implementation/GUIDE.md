# Optional decimal assessment — 29 October 2026

The unpublished Schema 0.6 candidate implements the bounded [optional-decimal plan](../schema-optional-children/OPTIONAL-DECIMAL-PLAN.md) using the [recorded normative adoption](../schema-optional-children/OPTIONAL-DECIMAL-ADOPTION-2026-10-29.md). Core candidate is 1.34.1. Earlier documents describing optional support as planned are historical scope records; the current behavior is described here and in [the assessment guide](ASSESSMENT-GUIDE.md).

## Behavior

Local decimal particles may have minimum zero or one; maximum remains one, and the enclosing sequence still occurs once. Required-only schemas retain `decimal-sequence-assertions-v1`; optional schemas report `decimal-sequence-assertions-v2`. Existing input/resource bounds, exact decimal representation and bounded assertion grammar remain in force. Unsigned zero/one spellings with leading zeros/XML whitespace are covered; signed occurrence spellings remain explicitly Unsupported.

The matcher walks schema particles and actual children separately. Only optional particles may be skipped. Missing required, repeated, unknown or out-of-order children are Invalid before assertions. Present decimals receive exact lexical/value validation. Assertions use effective Boolean conversion, so an absent-child value comparison is false and yields an assertion violation, not Unsupported. No new assertion functions are admitted.

Occurrence is stored on an internal immutable particle binding, separate from the original public declaration/type objects. `ChildDeclarations` includes every original declaration in schema order. `AssessedDocument.Elements` contains the root and present child states in instance order. For a required `amount`, optional `tax`, required `shipped`, omitting tax retains shipped's third original declaration; no placeholder, nilled node or default value is created.

`CreateTypedElementCopy` refuses an absent child with BSK-XSD-016. Later present children retain their correct value/state. Native copies, whole-document exports and XQuery/XSLT preserve copies retain original state; strip removes it. The core bridge now normalizes ignorable whitespace on an empty element-only root using its assessment kind, while decimal leaf text, real root text and child mutations still trigger stale-assessment refusal. Root atomization remains FOTY0012 even when every optional child is absent.

## Run

```powershell
dotnet test Bosak.Schema.slnx -c Release -p:BosakSourceRoot=D:/Development/Bosak
dotnet run --project examples/Bosak.Schema.Assessment.Example -c Release -p:BosakSourceRoot=D:/Development/Bosak
```

The source override is required while core 1.34.1 is unpublished. The example omits a middle tax particle, reports v2 and retained later declaration, typed decimal, total 15, original root state and successful XSLT preserve/strip checks. The original public checkout and product consumer pins remain unchanged.

## Qualification and limits

The initial 45-case regression cohort moved from 9 passes/36 failures to all passing; eleven additional boundary/copy controls bring optional coverage to 56 new cases. All 260 Schema cases pass. Five new provider cases check empty-root whitespace/lexical forms, root content mutation and decimal whitespace refusal. Initial fixture compile and sandbox NuGet-audit access failures are retained separately from passing receipts. Core/gate and documentation receipts are in core `docs/schema-optional-implementation`.

One core Release build reports five existing warnings in unchanged Standard tests/XSLT conformance code and zero errors; they are not changes to this feature. Fresh full core/gate results are recorded in the owning core dossier before handover. Earlier sealed scope/bridge evidence is preserved. No full XSD 1.1 certification, general public API freeze, compatible-universe admission, consumer adoption, commit/push, tag or package publication is inferred. Next: review this bounded slice before selecting a further structural extension.

Fresh final qualification: **24,214 complete core tests pass** (including 8,196 XSLT and 212 provider cases), **260 complete Schema tests pass** (56 new optional cases), normal QT3 **31,397/0/424**, qt4 **8,941/0/1,918**, 230 gated sets and 11 experimental passes. Exact skip names/reasons match the sealed integration checkpoint. Core build has zero errors and five warnings in unchanged Standard test/XSLT conformance sources; no new warning source is introduced. The optional-gap whole-document example and strict Schema documentation check pass. Original 45-case before evidence is 9 passed/36 failed; all 45 now pass alongside eleven added boundary cases. Five new core cases cover the root binding correction.
