# Native whole-document bridge — 29 October 2026

The bounded native Schema candidate now exports whole documents with original anonymous-root and decimal-child assessment state. This extends the earlier [assessment checkpoint](../schema-assessment/README.md); its sealed receipts remain historical and unchanged. The [pre-implementation adoption record](ADOPTION.md) defines the normative editions and deliberate scope.

## Behavior and boundary

`AssessedDocument.CreateDocumentCopy(true)` produces caller-owned XML carrying the original owner and element/component states through the optional `XdmElementAssessment` provider annotation. Decimal children atomize as decimal; the anonymous element-only root has no typed value and atomization raises FOTY0012. Its annotation is anonymous, not a fabricated xs:anyType. Universal type matching does not confer declaration authority.

XQuery construction preserve and XSLT preserve copies retain original state; strip removes it without changing input. Edited content refuses stale assessment, while namespace fixup and ignorable element-only whitespace retain state. Escaped trees retain their assessment owner. Legacy structural reassessment and declaration projection refuse explicitly with BSK-XSD-016 rather than substituting BCL components.

The finite decimal-sequence/assertion profile is unchanged. Full XSD 1.1, declaration matching, compatible-universe admission, general schema sessions, consumer adoption and public API freeze remain pending. IXdmNode and XdmValue layouts are unchanged. No package, tag, commit or push is included.

## Run the candidate

From the core repository root:

```powershell
dotnet test .worktrees/Bosak.Schema.Assessment/Bosak.Schema.slnx -c Release -p:BosakSourceRoot=D:/Development/Bosak
dotnet run --project .worktrees/Bosak.Schema.Assessment/examples/Bosak.Schema.Assessment.Example -c Release -p:BosakSourceRoot=D:/Development/Bosak
```

The unpublished core candidate is 1.34.0; Schema stays on its 0.5 development line. Its candidate references 1.34.0, so use the explicit source override until those dependencies exist as packages. Original public checkout and product consumer pins remain untouched. The example reports valid assessment, typed decimal, total 15, original root identity and successful XSLT preserve/strip checks.

See RESULTS.json and evidence for fresh qualification. Next: review this bounded candidate before extending structural coverage or adopting a general public schema-session contract.

Fresh bridge qualification: Release build has zero warnings/errors; complete core aggregate **24,207 passed**, native Schema **204 passed** (13 whole-document cases), QT3 **31,397/0/424**, qt4 gated **8,941/0/1,918**, 230 sets and 11 experimental passes. Exact skip mappings match the sealed integration checkpoint. The initial sandbox database failures are retained separately; the local-HTTP-authorized rerun passes all 47 cases. Whole-document example and strict Schema documentation checks pass.
