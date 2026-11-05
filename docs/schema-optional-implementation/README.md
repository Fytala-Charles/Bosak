# Optional decimal implementation candidate — 29 October 2026

The scoped native Schema extension is implemented in the persistent public Schema feature worktree. Read the [implementation guide snapshot](GUIDE.md), [implementation adoption](ADOPTION.md) and original [pre-implementation scope](../schema-optional-children/README.md). The earlier scope/delivery records remain historical evidence; optional support is now implemented for the finite profile.

Schema candidate 0.6 supports local decimal minimum zero/one with maximum one, ordered matching, actual-present-state identity and assertion effective Boolean conversion. Required-only schemas retain v1; optional schemas use v2. Core candidate 1.34.1 fixes empty element-only root whitespace binding without weakening decimal/content mutation checks. General sessions, repeated particles, defaults/nil/fixed, broader assertion grammar, compatible-universe admission and consumer adoption remain separate.

The runnable example omits a middle optional tax child and retains the following declaration and exact decimal values through real XPath/XSLT use. The complete Schema suite passes 260 cases (56 new), with five additional core provider cases. The original 45-case regression cohort has a preserved 9-pass/36-failure before result. Final qualification is in RESULTS.json and evidence.

## Candidate location and execution

Core source base: `6b362a15f6fb62eebc36f82e803e35faf42cecd4`. Schema source base: `33a9039a9fc605f8f7192bfcec88ea559c5c0e19`, branch `feature/bounded-document-assessment`, path `D:/Development/Bosak/.worktrees/Bosak.Schema.Assessment`. Current implementation changes remain uncommitted.

```powershell
dotnet test .worktrees/Bosak.Schema.Assessment/Bosak.Schema.slnx -c Release -p:BosakSourceRoot=D:/Development/Bosak
dotnet run --project .worktrees/Bosak.Schema.Assessment/examples/Bosak.Schema.Assessment.Example -c Release -p:BosakSourceRoot=D:/Development/Bosak
```

Core candidate dependencies are unpublished; the local source override is required. Product consumer pins and the original public checkout remain unchanged. No commit, push, merge, tag or package release is included in this implementation step. Next: review this bounded slice before a further structural extension.

Fresh final qualification: **24,214 complete core tests pass** (including 8,196 XSLT and 212 provider cases), **260 complete Schema tests pass** (56 new optional cases), normal QT3 **31,397/0/424**, qt4 **8,941/0/1,918**, 230 gated sets and 11 experimental passes. Exact skip names/reasons match the sealed integration checkpoint. Core build has zero errors and five warnings in unchanged Standard test/XSLT conformance sources; no new warning source is introduced. The optional-gap whole-document example and strict Schema documentation check pass. Original 45-case before evidence is 9 passed/36 failed; all 45 now pass alongside eleven added boundary cases. Five new core cases cover the root binding correction.
