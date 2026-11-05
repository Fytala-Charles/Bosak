# Bounded repeated decimal implementation candidate — 29 October 2026

Schema 0.7 implements the [scoped repetition slice](../schema-repeated-children/README.md). Read the [current guide snapshot](GUIDE.md) and [implementation adoption](ADOPTION.md). Owning editable source/guide remain in the persistent public Schema feature worktree; earlier sealed optional/repetition-scope evidence is preserved.

Finite local maxima and an aggregate budget of sixteen, ordered occurrence matching, distinct states sharing original declarations, exact cardinality-error classification and ambiguous singular-export refusal are implemented. Existing v1/v2 remain; repeated schemas use v3. Assertion grammar and public overloads remain unchanged. Core stays source-identical 1.34.1; product consumer pins are untouched.

Fresh complete Schema suite: 334 passes, 74 new cases. Original 57-case before cohort: 17 passes/40 failures, now all passing. One prior optional maximum-two refusal is explicitly updated with adoption/source preservation. Schema build and repeated-document XPath/XSLT example pass. RESULTS.json records exact source preservation and inherited core/gate qualification; no fresh full-engine run or full XSD 1.1 certification is claimed.

## Run and source state

Source bases: core `9e4d4d82a27270ec0fa72c3f3f406513a0358d6f`; Schema `77c740ca5c86f9a98afbeacbe8949b0076da7abd`, branch `feature/bounded-document-assessment`. Current changes remain uncommitted.

```powershell
dotnet test .worktrees/Bosak.Schema.Assessment/Bosak.Schema.slnx -c Release -p:BosakSourceRoot=D:/Development/Bosak
dotnet run --project .worktrees/Bosak.Schema.Assessment/examples/Bosak.Schema.Assessment.Example -c Release -p:BosakSourceRoot=D:/Development/Bosak
```

The unpublished core dependency requires the explicit source override. Example output demonstrates ordered repeated values, shared declaration/distinct states, host total 15, retained later declaration, explicit ambiguous projection refusal and preserve/strip semantics. No commit/push, merge, tag, package or consumer adoption is included. Next: review/checkpoint this bounded candidate.

Final repeated-candidate qualification: **334 complete Schema cases pass**, including **74 new repetition/error-boundary cases**. The original 57-case before cohort (17 passed/40 failed) now all passes; seventeen further boundary/safety cases also pass. The prior 260-case suite retains 259 unchanged passing expectations and one explicitly adopted maxOccurs-two coverage/profile update. Schema source-override Release build: **zero warnings/errors**. The repeated-document XPath/XSLT example and strict Schema docs check pass. Core source/tests/harness/fixture references and build configuration are unchanged from the reviewed source, so **24,214 core passes**, QT3 **31,397/0/424** and qt4 **8,941/0/1,918** (230 sets) are inherited, not fresh full runs. Exact raw source/receipt identities and all earlier sealed dossiers are preserved.
