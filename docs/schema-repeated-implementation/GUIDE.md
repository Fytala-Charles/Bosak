# Bounded repeated decimal assessment — 29 October 2026

Schema 0.7 development candidate implements the [bounded repetition plan](../schema-repeated-children/REPEATED-DECIMAL-PLAN.md) and its [pre-implementation adoption](../schema-repeated-children/REPEATED-DECIMAL-ADOPTION-2026-10-29.md). Core stays 1.34.1 with unchanged production source. The candidate remains unpublished and uncommitted; earlier scope-only records describe their capture time, not current implemented coverage.

## Finite behavior

Local decimal minimum is zero or one, maximum is 1–16, and the sum of all maxima is at most sixteen. The enclosing sequence still occurs once and declaration names remain unique. Unsigned zero-padded/XML-whitespace occurrence spellings are supported. Signed, unbounded, minimum-greater-than-one and out-of-budget requests remain explicitly Unsupported. Existing schema/instance/node/lexical bounds and exact decimal representation remain unchanged.

Internal immutable particle bounds are separate from public declaration/type identity. Contiguous ordered matching checks all counts/order before lexical values and assertions. Each actual occurrence has a distinct AssessedElement state and node, even for equal decimal values; repetitions share the original governing declaration/type objects. ChildDeclarations retains schema order; Elements retains root plus every present occurrence in instance order. Required omissions, excess copies, unknown names and order changes are Invalid, not silently ignored.

The original assertion grammar is unchanged: name/decimal value comparisons and and/or. More than one atomized operand raises the existing engine XPTY0004 error. In this closed grammar, the exact standard cardinality error becomes Invalid/AssertionViolated with no assessed document. Structured errors require the standard namespace and exact code; plain errors require exact InvalidOperationException type and an anchored `XPTY0004:` prefix. Foreign namespaces, misleading substrings, other codes and CLR errors are not converted to assertion failure. Cancellation/out-of-memory propagate. Existing actual logical short-circuit behavior is preserved; no whole-expression preflight scans unexecuted branches.

Singular `CreateTypedElementCopy(name)` now requires exactly one actual matching child. Zero or multiple matches refuse BSK-XSD-016. Whole-document export supports host occurrence-specific queries such as `/order/amount[2]`, retaining ordered state. Native copies and XQuery/XSLT preserve/strip retain distinct occurrences, shared original declaration authority and source immutability. Root atomization remains FOTY0012, including an all-absent v3 document.

Profile selection preserves required-only v1 and optional maximum-one v2. Schemas with a local maximum greater than one use `decimal-sequence-assertions-v3`. No new public overload or core interface/layout change is introduced. The original public checkout and product consumer pins are untouched; no general session/API freeze or full XSD 1.1 certification is inferred.

## Run the candidate

```powershell
dotnet build Bosak.Schema.slnx -c Release -p:BosakSourceRoot=D:/Development/Bosak
dotnet test Bosak.Schema.slnx -c Release -p:BosakSourceRoot=D:/Development/Bosak
dotnet run --project examples/Bosak.Schema.Assessment.Example -c Release -p:BosakSourceRoot=D:/Development/Bosak
```

The source override is still required for unpublished core 1.34.1. The example assesses two amount values (12.5, 2.5), an omitted optional tax and later shipped value. It reports v3, shared declaration/distinct occurrence states, retained later declaration, typed decimals, host-query total 15, explicit ambiguous projection refusal and correct XSLT preserve/strip sequence order. Its `sum` call is a host query, not admitted schema assertion grammar.

## Qualification and boundary

The complete Schema suite passes 334 cases, including 74 new repetition/error-policy controls. The initial 57-case cohort changes from 17 passes/40 failures to all passing; seventeen additional safety/boundary controls complete the new suite. One historical maxOccurs-two refusal expectation is deliberately updated under the recorded adoption; its original source is preserved. All other prior expectations remain unchanged.

The Schema source-override Release build has zero warnings/errors. Core production/tests/harness/fixture references remain source-identical to the reviewed optional checkpoint, allowing inheritance of 24,214 core passes and exact QT3 31,397/0/424 / qt4 8,941/0/1,918, 230 gated sets under the approved plan. No fresh full-core/gate run is claimed for this Schema-only slice; identity/provenance checks and raw receipts are in core `docs/schema-repeated-implementation`.

The narrow error helper has crafted boundary tests and internal fault-injected compiled-plan tests; those deliberately bypass grammar only in tests and do not admit `error()` to user schemas. Defaults/nil/fixed, attributes, imports, choices, aggregation/general comparisons, independent compatible universes and consumer adoption remain separate. No commit/push, merge, tag or package publication is included. Next: bounded implementation review and checkpoint before another structural extension.

Final repeated-candidate qualification: **334 complete Schema cases pass**, including **74 new repetition/error-boundary cases**. The original 57-case before cohort (17 passed/40 failed) now all passes; seventeen further boundary/safety cases also pass. The prior 260-case suite retains 259 unchanged passing expectations and one explicitly adopted maxOccurs-two coverage/profile update. Schema source-override Release build: **zero warnings/errors**. The repeated-document XPath/XSLT example and strict Schema docs check pass. Core source/tests/harness/fixture references and build configuration are unchanged from the reviewed source, so **24,214 core passes**, QT3 **31,397/0/424** and qt4 **8,941/0/1,918** (230 sets) are inherited, not fresh full runs. Exact raw source/receipt identities and all earlier sealed dossiers are preserved.
