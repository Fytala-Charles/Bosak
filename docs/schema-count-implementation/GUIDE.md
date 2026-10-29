# Bounded count assertion candidate — 29 October 2026

Schema 0.8 implements the [count assertion plan](../schema-count-assertions/COUNT-ASSERTION-PLAN.md), with the [recorded normative adoption](../schema-count-assertions/COUNT-ASSERTION-ADOPTION-2026-10-29.md) and [separate source locks](../schema-count-assertions/COUNT-ASSERTION-SOURCES.json). Core remains source-identical 1.34.1. Earlier scope-only records describe their capture time; current count admission is implemented for the finite boundary below. Changes remain uncommitted and unpublished.

## Grammar and profile

Unprefixed `count(DeclaredChildName)` is now a scalar operand alongside existing exact-decimal and bare-name operands. It participates in eq/ne/lt/le/gt/ge comparisons, and/or and grouping. The argument is one declared direct no-namespace child name. Count returns one integer including zero for absence; equal-valued occurrences remain distinct nodes and count separately. The standard default function namespace resolves the unprefixed function independently of element/type namespaces.

Bare count child names retain name selection. A function is recognized only when the count token is followed by an opening parenthesis, so `count eq 2` and `count(count) eq 1` are distinct cases. Full grammar admission returns immutable feature metadata; profile v4 is selected only after a complete parse admits a count call. Schema compilation still evaluates the validated original expression under XPath 2.0; no synthetic count predicate or text-based feature scan replaces it. Valid count syntax selects v4 even if the compiler can optimize away its runtime branch. Existing schemas without count calls retain v1/v2/v3.

The 512-character, 128-token and sixteen-parenthesis-depth limits remain. Function-call parentheses contribute to depth while legacy grouping keeps its previous limit. Unsupported grammar is rejected before XPath compilation even in unexecuted branches. Prefixed fn:count, aliases/rebindings, other functions, nested/empty/multiple arguments, unknown names, paths, predicates, wildcards, arbitrary sequences/variables, general comparisons and arithmetic remain Unsupported. This is finite admission, not a claim those forms are invalid XPath.

## Native assessment and copies

Structural matching and exact decimal validation still finish before assertions. A count that would be true cannot mask missing required children, wrong order, invalid lexical values or unrepresentable valid decimals. Count is scalar, but an uncounted multi-item operand on the other side still yields the established XPTY0004 assertion Invalid. Effective Boolean evaluation, short-circuit behavior, cancellation/resource bounds and the narrow error contract are unchanged.

Count does not alter original declaration/type identity, distinct occurrence states, ownership or copy semantics. Native/XQuery/XSLT preserve copies retain state and typing; strip removes annotations on private copies. Ambiguous singular projection still refuses, source edits still cannot reuse stale values, and element-only root atomization remains FOTY0012. No public overload, general schema-session contract or product pin change is introduced.

## Run and qualification

```powershell
dotnet build Bosak.Schema.slnx -c Release -p:BosakSourceRoot=D:/Development/Bosak
dotnet test Bosak.Schema.slnx -c Release -p:BosakSourceRoot=D:/Development/Bosak
dotnet run --project examples/Bosak.Schema.Assessment.Example -c Release -p:BosakSourceRoot=D:/Development/Bosak
```

The example uses actual native `count(amount) eq 2` and `count(tax) eq 0` assertions, retains shared declaration/distinct occurrence state and later declaration identity, and demonstrates total 15, ambiguous projection refusal and ordered typed preserve/strip copies. Its sum remains a host query, not admitted schema aggregation. The source override remains required for unpublished core 1.34.1.

The complete Schema suite passes 397 cases, including 63 new count tests. The original 57-case count cohort moved from 23 passes/34 failures to all passing; six later profile/optimization-boundary controls complete the new suite. One recorded count refusal is migrated to an explicit supported/v4 capability case; its source/receipt is preserved and the other 333 prior expectations remain unchanged. Source-override build has zero warnings/errors; example and strict docs checks pass.

Core production/tests/harness/fixture references and build configuration are unchanged, so the recorded 24,214 core passes and QT3 31,397/0/424 / qt4 8,941/0/1,918 (230 sets) are inherited after identity verification, not fresh full runs. Raw receipts, source copies and prior dossier preservation checks are in core `docs/schema-count-implementation`. Full XSD 1.1, general namespace/function grammar, API freeze and consumer adoption remain separate. No commit/push, merge, package or tag is included. Next: review/checkpoint the bounded candidate before wider grammar.

Final count-candidate qualification: **397 complete Schema cases pass**, including **63 new count/grammar/validation-order/copy cases**. The original 57-case cohort (23 passed/34 failed) now all passes; six further profile/optimization-boundary cases pass. The prior 334-case suite retains **333 unchanged passing expectations** plus the explicitly adopted count capability check. Source-override build has **zero warnings/errors**. The actual native count/absence example and strict documentation checks pass. Core source/tests/harness/fixture references and build configuration remain identical, so **24,214 core passes**, QT3 **31,397/0/424**, qt4 **8,941/0/1,918** (230 sets) are inherited, not fresh full runs. All earlier sealed evidence and original refusal source/receipts remain unchanged.
