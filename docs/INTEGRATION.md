<!-- Bosak XPath / XSLT — General Integration Guide -->
<!-- Living document: updated with each significant Bosak change. -->

# Bosak XPath / XSLT / XQuery — Integration Guide

> **Purpose:** Quick-reference for any application consuming the Bosak XPath 3.1 + XSLT + XQuery stack.
> **Last updated:** 20 July 2026
> **Bosak baseline:** 1,379 unit tests passed / 0 failed / 0 skipped
> **QT3 baseline:** 14,849 passed / 28 failed / 16,944 skipped (46.66% / 99.81% of runnable tests)
> **XSLT baseline:** 7,109 passed / 0 failed / 7,491 skipped — 100% of runnable W3C XSLT 3.0 tests pass

---

## 0. Recent Changes

- **2026-07-20** — QT3 Tier-2z: `K2-SeqExprCast-1/201` / `xs:QName` namespace resolution for `cast as` and `xs:QName()` constructor.
  - `"myPrefix:ncname" cast as xs:QName` was not resolving the prefix against the static namespace context; the cast was producing a string instead of a QName and raising `XPTY0004`.
  - `xs:QName("ncname")` was ignoring the default element namespace, returning an empty namespace URI instead.
  - Added `EvaluationContext` overloads to `Cast` and `TryCast` so the `Cast`/`Castable` opcodes can pass the static namespace context into QName casting. The cast path now resolves prefixed QNames and uses the default element namespace for unprefixed ones.
  - Updated the `xs:QName` constructor (`XsQNameConstructor`) to use `DefaultElementNamespace` for unprefixed lexical QNames.
  - Updated `TestEnvironment.ApplyTo` to map a QT3 environment `<namespace prefix="" uri="...">` to `EvaluationContext.DefaultElementNamespace`.
  - Fixed unprefixed QName resolution in `CastUntypedAtomicToQName` to fall back to the empty namespace when no default element namespace is defined.
  - Rewrote `FunctionLibraryTests` chained-FLWOR regression tests as nested expressions so they remain valid under the XPath grammar restriction enforced by `LetExpr020a`.
  - Added `QNameCast_ResolvesPrefixedNamespace`, `QNameCast_UsesDefaultElementNamespaceForUnprefixed`, and `XsQNameConstructor_UsesDefaultElementNamespace` regression tests.
  - Targeted tests now pass: `K2-SeqExprCast-1`, `K2-SeqExprCast-201`, `CastableAs647`, `K-SeqExprCastable-19`.
  - Full QT3 now **14,849 passed / 28 failed / 16,944 skipped = 46.66%** (runnable pass rate **99.81%**); unit tests **1,379/0**.

- **2026-07-20** — QT3 Tier-2z: `K-SeqExprCast-67` / `cast as` raises `XPTY0004` for empty singleton input.
  - `() cast as xs:QName` was succeeding because the `Cast` opcode only checked for empty input when the target occurrence was `?`, `*`, or `+`; the default `One` occurrence fell through to `Cast()`.
  - Restructured the empty-input branch in the `Cast` opcode: empty input with occurrence `One` now raises `XPTY0004`; empty input with `?` still returns `()`; `*`/`+` still raise the existing occurrence-indicator error.
  - Added `EvaluateValue_EmptySequenceCastAsQName_RaisesXPTY0004` regression test.
  - Targeted test passes: `K-SeqExprCast-67`.
  - Full QT3 now **14,847 passed / 30 failed / 16,944 skipped = 46.66%** (runnable pass rate **99.80%**); unit tests **1,376/0**.

- **2026-07-20** — QT3 Tier-2z: `K-SeqExprTreat-16` / require closing parenthesis in sequence type tests.
  - `3 treat as item(` was being accepted because `ParseTypeNameAndParens` consumed tokens until EOF without verifying that the opening parenthesis was closed.
  - Added a `parenDepth > 0` check after consuming the sequence type; an unclosed paren now raises `XPST0003`.
  - Added `TreatExpr` and `TreatExpr_UnclosedTypeParens_RaiseXPST0003` regression tests.
  - Targeted test passes: `K-SeqExprTreat-16`.
  - Full QT3 now **14,846 passed / 31 failed / 16,944 skipped = 46.65%** (runnable pass rate **99.79%**); unit tests **1,375/0**.

- **2026-07-20** — QT3 Tier-2z: `LetExpr020a` / disallow consecutive `for`/`let` clauses in XPath FLWOR.
  - XPath 3.1 allows only one initial `for` or `let` clause; intermediate clauses may only be `where`, `order by`, or `count`. The parser was treating additional `for`/`let` keywords as new intermediate clauses, so `let $a := 1 let $b := $a return ...` parsed successfully.
  - `ParseFlworExpr` no longer accepts `KeywordFor` or `KeywordLet` as intermediate clauses; a following `let` is now treated as unexpected input and `Expect(TokenKind.KeywordReturn)` raises `XPST0003`.
  - Added `LetExpr` and `LetExpr_ConsecutiveLetKeywords_RaiseXPST0003` regression tests.
  - Targeted test passes: `LetExpr020a`.
  - Full QT3 now **14,845 passed / 32 failed / 16,944 skipped = 46.65%** (runnable pass rate **99.79%**); unit tests **1,373/0**.

- **2026-07-20** — QT3 Tier-2z: `K-XQueryComment-14/15` / unterminated XPath comments now raise `XPST0003`.
  - The lexer previously consumed unterminated comments to EOF silently, so expressions like `1(: this comment does not end` parsed as just `1` and succeeded.
  - `XPathLexer.SkipComment` now throws `ParseException` (auto-prefixed `XPST0003`) when a comment is still open at end of input, including partially closed nested comments.
  - Added `UnterminatedComment_AfterExpression_RaisesXPST0003` and `NestedUnterminatedComment_AfterExpression_RaisesXPST0003` regression tests.
  - Targeted tests pass: `K-XQueryComment-14`, `K-XQueryComment-15`.
  - Full QT3 now **14,844 passed / 33 failed / 16,944 skipped = 46.65%** (runnable pass rate **99.78%**); unit tests **1,371/0**.

- **2026-07-20** — QT3 Tier-2z: `CastAs009/091` / `xs:float` fixed-point formatting in decimal range.
  - `FormatXPathFloat` was normalizing `R`-format scientific strings (e.g. `1E-05`) inside the decimal range (`1e-6 <= |x| < 1e6`), producing `1.0E-5` instead of expanding to fixed-point `0.00001`.
  - Aligned the float branch with the double branch: expand `R`-scientific to fixed-point inside the decimal range, then trim trailing zeros.
  - Added `FloatToString_InsideDecimalRange_ExpandsToFixedPoint` regression test.
  - Targeted tests pass: `CastAs009`, `CastAs091`.
  - Full QT3 now **14,842 passed / 35 failed / 16,944 skipped = 46.64%** (runnable pass rate **99.77%**); unit tests **1,369/0**.

- **2026-07-20** — QT3 Tier-2z: `Literals017/025/028` / XPath canonical double formatting.
  - `FormatXPathDouble` was using `G17` for scientific-range values, which preserved round-trip noise (e.g. `6553503.2000000002`) and inflated the exponent when normalizing fixed-point to scientific notation.
  - Switched to `R` (shortest round-trip) format and compute the exponent from the fixed-point decimal position rather than the total digit count.
  - `FormatXPathFloat` uses the same decimal-point-based exponent calculation.
  - Added `DoubleToString_FixedPointScientific_TrimsRoundTripNoise` regression test.
  - Targeted tests pass: `Literals017`, `Literals025`, `Literals028`.
  - Full QT3 now **14,840 passed / 37 failed / 16,944 skipped = 46.64%** (runnable pass rate **99.75%**); unit tests **1,368/0**.

- **2026-07-20** — QT3 Tier-2z: `K-FilterExpr-82` / atomize predicate result before numeric/EBV check.
  - Filter expression predicates that return a sequence must be atomized before deciding whether they are numeric positional predicates or being used for their effective boolean value.
  - In `VmEngine.Filter`, `predResult` is now atomized first; a multi-item sequence raises `XPTY0004`, and a singleton integer (e.g. `(1)` from `remove((1, "a string"), 2)`) is treated as a numeric predicate.
  - Added `Predicate_AtomizesSequenceResult` regression test.
  - Targeted test passes: `K-FilterExpr-82`.
  - Full QT3 now **14,836 passed / 41 failed / 16,944 skipped = 46.62%** (runnable pass rate **99.73%**); unit tests **1,367/0**.

- **2026-07-20** — QT3 Tier-2z: `K2-Axes-50/53` / XPTY0019 for path steps on non-node context items.
  - `SimpleMap` (used for non-axis path steps) now raises `XPTY0019` when the input sequence contains non-node items, but only in path-step mode (`RegisterC != 0`); the `!` operator continues to allow non-node items.
  - `PathStepMap` (used for predicated axis steps) also raises `XPTY0019` for non-node context items.
  - Added `PathStep_RequiresNodeContextItem` regression test.
  - Targeted tests pass: `K2-Axes-50`, `K2-Axes-53`.
  - Full QT3 now **14,835 passed / 42 failed / 16,944 skipped = 46.62%** (runnable pass rate **99.72%**); unit tests **1,366/0**.

- **2026-07-20** — QT3 Tier-2z: `Axes123` / namespace-node identity in `is`.
  - Namespace nodes are virtual properties of an element; the underlying XAttribute objects are created on demand, so reference equality failed. `XDocumentNode.IsSameNode` now compares namespace nodes by owner element + prefix + URI, and `GetHashCode` is consistent with this semantic identity.
  - Added `NamespaceNode_IsSameNodeIdentity` regression test.
  - Targeted test passes: `Axes123`.
  - Full QT3 now **14,833 passed / 44 failed / 16,944 skipped = 46.61%** (runnable pass rate **99.71%**); unit tests **1,365/0**.

- **2026-07-20** — QT3 Tier-2z: `K2-NameTest-78/79` / `let` and `for` as name tests.
  - `let` and `for` are FLWOR keywords only when followed by a variable binding (`$`). When used as a single name test, they now parse as path steps and raise `XPDY0002` (no context item) instead of `XPST0003`.
  - Added `FlworKeywords_ParseAsNameTests` regression test.
  - Targeted tests pass: `K2-NameTest-78`, `K2-NameTest-79`.
  - Full QT3 now **14,832 passed / 45 failed / 16,944 skipped = 46.61%** (runnable pass rate **99.70%**); unit tests **1,362/0**.

- **2026-07-20** — QT3 Tier-2z: `K-NodeSame-6` / allow `is` as a non-reserved function name.
  - `is` is an operator keyword but not a reserved function name, so `is(...)` parses as a function call and raises `XPST0017` because no such function exists.
  - Added `IsKeyword_AllowedAsFunctionName` regression test.
  - Targeted test passes: `K-NodeSame-6`.
  - Full QT3 now **14,830 passed / 47 failed / 16,944 skipped = 46.60%** (runnable pass rate **99.68%**); unit tests **1,361/0**.

- **2026-07-20** — QT3 Tier-2z: `K-NodeNumberFunc-13/15` / `fn:number` on non-numeric, non-string atomic types.
  - `fn:number` now returns `NaN` for atomic types such as `xs:anyURI`, `xs:gYear`, and `xs:QName`, while continuing to convert numeric types, `xs:string`, `xs:untypedAtomic`, and `xs:boolean` to `xs:double`.
  - Added `Number_ReturnsNaNForNonNumericNonStringTypes` regression test.
  - Targeted tests pass: `K-NodeNumberFunc-13`, `K-NodeNumberFunc-15`.
  - Full QT3 now **14,829 passed / 48 failed / 16,944 skipped = 46.60%** (runnable pass rate **99.68%**); unit tests **1,361/0**.

- **2026-07-20** — QT3 Tier-2z: `K2-SeqDeepEqualFunc-40` / `fn:deep-equal` implicit timezone handling.
  - `fn:deep-equal` now compares `xs:dateTime`, `xs:date`, and `xs:time` values using the evaluation context's implicit timezone when one operand has no explicit timezone.
  - Added `DeepEqual_RespectsImplicitTimezone` regression test.
  - Targeted test passes: `K2-SeqDeepEqualFunc-40`.
  - Full QT3 now **14,827 passed / 50 failed / 16,944 skipped = 46.60%** (runnable pass rate **99.66%**); unit tests **1,360/0**.

- **2026-07-20** — QT3 Tier-2z: `K2-DataFunc-6` / `fn:data()` on complex element-only schema elements.
  - Added `IXdmNode.HasNoTypedValue` default accessor and `XDocumentNode.HasNoTypedValue` implementation using PSVI schema info.
  - `FunctionLibrary.Data` now raises `FOTY0012` for element-only or empty complex-type elements.
  - Added `Data_ThrowsFoty0012ForElementOnlyComplexElement` regression test.
  - Targeted test passes: `K2-DataFunc-6`.
  - Full QT3 now **14,826 passed / 51 failed / 16,944 skipped = 46.59%** (runnable pass rate **99.66%**); unit tests **1,360/0**.

- **2026-07-20** — QT3 Tier-2z: `fn-upper-case-22` / Armenian ligature upper-case mapping.
  - `FunctionLibrary.ApplyUnicodeCaseMapping` now maps U+FB17 (Armenian small ligature men xeh) to U+0544 U+053D (MEN + XEH).
  - Added `UpperCase_ArmenianLigatureMenXeh` regression test.
  - Targeted test passes: `fn-upper-case-22`.
  - Full QT3 now **14,825 passed / 52 failed / 16,944 skipped = 46.59%** (runnable pass rate **99.65%**); unit tests **1,359/0**.

- **2026-07-20** — QT3 Tier-2z: `fn-number-3` / `fn:number()` with no context item.
  - `FunctionLibrary.Number_0` now raises `XPDY0002` when `fn:number()` is called without a context item.
  - The one-argument form `fn:number(())` still returns `NaN` as required by the spec.
  - Added `Number_ThrowsWithoutContextItem` regression test.
  - Targeted test passes: `fn-number-3`.
  - Full QT3 now **14,824 passed / 53 failed / 16,944 skipped = 46.59%** (runnable pass rate **99.64%**); unit tests **1,357/0**.

- **2026-07-20** — QT3 Tier-2z: `fn-not-28` / effective boolean value of multi-item sequences.
  - `XdmValue.SequenceEffectiveBooleanValue` now raises `FORG0006` when a sequence of more than one item has a non-node first item.
  - Previously, any multi-item sequence was treated as `true`; XPath 3.1 §2.4.3 requires a node-first sequence for this behavior.
  - Added `Not_ThrowsOnMixedSequence` regression test.
  - Targeted test passes: `fn-not-28`.
  - Full QT3 now **14,823 passed / 54 failed / 16,944 skipped = 46.58%** (runnable pass rate **99.63%**); unit tests **1,357/0**.

- **2026-07-19** — QT3 Tier-2z: `fn-doc-available-2` / `fn:doc` and `fn:doc-available` URI argument validation.
  - `FunctionLibrary.Doc_1` and `DocAvailable_1` now use `RequireString` on the URI argument.
  - Non-string atomics (e.g., `fn:doc-available(xs:integer(2))`) now raise `XPTY0004`; empty sequence behavior is preserved.
  - Added `DocAvailable_RejectsNonStringArgument` regression test.
  - Targeted test passes: `fn-doc-available-2`.
  - Full QT3 now **14,822 passed / 55 failed / 16,944 skipped = 46.58%** (runnable pass rate **99.63%**); unit tests **1,356/0**.

- **2026-07-19** — QT3 Tier-2z: `fn-substring-after-23` / `fn-substring-before-23` / relative collation URI resolution.
  - Added `FunctionLibrary.ResolveCollationUri` to absolutize relative collation URIs against `EvaluationContext.BaseUri`.
  - `fn:substring-before` and `fn:substring-after` now resolve their `$collation` argument before validating it.
  - Added `SubstringAfter_ResolvesRelativeCollationUri` regression test.
  - Targeted tests pass: `fn-substring-after-23`, `fn-substring-before-23`.
  - Full QT3 now **14,821 passed / 56 failed / 16,944 skipped = 46.58%** (runnable pass rate **99.62%**); unit tests **1,355/0**.

- **2026-07-19** — QT3 Tier-2z: `fn-implicit-timezone-10/11/12` / duration `div` NaN/zero validation.
  - `VmEngine.DivideDuration` now checks for `NaN` and `0.0`/`-0.0` before the zero-duration short-circuit.
  - `xs:dayTimeDuration` (including the `PT0S` implicit timezone) divided by `NaN` now raises `FOCA0005`; divided by zero raises `FODT0002`.
  - Added `ImplicitTimezone_DivByInvalidNumber_Throws` regression test.
  - Targeted tests pass: `fn-implicit-timezone-10`, `fn-implicit-timezone-11`, `fn-implicit-timezone-12`.
  - Full QT3 now **14,819 passed / 58 failed / 16,944 skipped = 46.57%** (runnable pass rate **99.61%**); unit tests **1,354/0**.

- **2026-07-19** — QT3 Tier-2z: `fn:iri-to-uri` / `K2-IRIToURIfunc` non-string argument validation.
  - `FunctionLibrary.IriToUri` now uses `RequireString` on its argument.
  - Non-string atomics (e.g., `iri-to-uri(12)`) and multi-item sequences (e.g., `iri-to-uri(('a','b'))`) now raise `XPTY0004`; nodes and `xs:untypedAtomic` are still atomized to strings.
  - Added `IriToUri_RejectsNonStringArguments` regression test.
  - Targeted tests pass: `fn-iri-to-uri1args-5`, `K2-IRIToURIfunc-3`, `K2-IRIToURIfunc-4`.
  - Full QT3 now **14,816 passed / 61 failed / 16,944 skipped = 46.56%** (runnable pass rate **99.59%**); unit tests **1,353/0**.

- **2026-07-19** — QT3 Tier-2z: double `MAX_VALUE` string formatting / `G17` round-trip cluster.
  - `XdmValue.FormatXPathDouble` now uses `"G17"` instead of `"G16"` for scientific-notation doubles, preserving all round-trip digits.
  - Fixes boundary-value failures in `fn:ceiling`, `fn:concat`, `fn:data`, `fn:exactly-one`, `fn:floor`, `fn:number`, `fn:one-or-more`, `fn:string`, and `fn:zero-or-one` `*dbl1args-*` tests.
  - Added `DoubleMaxValue_RoundTripString` regression test.
  - Full QT3 now **14,813 passed / 64 failed / 16,944 skipped = 46.55%** (runnable pass rate **99.57%**); unit tests **1,352/0**.

- **2026-07-19** — QT3 Tier-2z: `compare-011` / `fn:compare` non-string argument validation.
  - `FunctionLibrary.Compare_2` and `Compare_3` now use `RequireString` on both arguments.
  - Non-string atomics (e.g., `compare(123, 456)`) now raise `XPTY0004`; nodes and `xs:untypedAtomic` are still atomized to strings.
  - Added `Compare_RejectsNonStringArguments` regression test.
  - Targeted test passes: `compare-011`.
  - Full QT3 now **14,791 passed / 86 failed / 16,944 skipped = 46.48%** (runnable pass rate **99.42%**); unit tests **1,350/0**.

- **2026-07-19** — QT3 Tier-2z: `K2-StringLT-1` / default codepoint collation for value comparisons.
  - `FunctionLibrary.Populate` now installs the standard `FunctionLibrary.CompareStrings` comparer when the context has no custom comparer.
  - This ensures XPath value comparisons (`lt`/`le`/`gt`/`ge`/`eq`/`ne`) use the codepoint collation (Unicode scalar values) in the API path, not `string.CompareOrdinal`.
  - Added `StringLessThan_UsesUnicodeCodepoints` regression test (BMP vs supplementary plane codepoints).
  - Targeted test passes: `K2-StringLT-1`.
  - Full QT3 now **14,790 passed / 87 failed / 16,944 skipped = 46.48%** (runnable pass rate **99.42%**); unit tests **1,350/0**.

- **2026-07-19** — QT3 Tier-2z: `K-NumericSubtract-34/35` / `xs:untypedAtomic` arithmetic promotion.
  - `VmEngine.Add`, `Subtract`, `Multiply`, `Divide`, `IntegerDivide`, and `Modulo` now atomize operands and check `xs:untypedAtomic` before the numeric type-specific branches.
  - When any operand of an arithmetic expression is `xs:untypedAtomic`, both operands are cast to `xs:double` and the result is `xs:double` (or `xs:integer` for `idiv`/`mod`).
  - Added `NumericSubtract_PromotesUntypedAtomicToDouble` regression test.
  - Targeted tests pass: `K-NumericSubtract-34`, `K-NumericSubtract-35`.
  - Full QT3 now **14,789 passed / 88 failed / 16,944 skipped = 46.48%** (runnable pass rate **99.41%**); unit tests **1,348/0**.
  - `XPathOptimizer` now only folds `+x` for numeric literals; non-literal operands keep the `UnaryExpressionNode`.
  - `IrLowerer` emits the `UnaryPlus` VM opcode instead of a simple `Move`.
  - `VmEngine.UnaryPlus` validates the operand and raises `XPTY0004` for non-numeric, non-untypedAtomic values (e.g., `+"a string"`).
  - `xs:untypedAtomic` is promoted to `xs:double`; numeric types are returned unchanged.
  - Targeted test passes: `K-NumericUnaryPlus-1`.
  - Full QT3 now **14,786 passed / 91 failed / 16,944 skipped = 46.47%** (runnable pass rate **99.38%**); unit tests **1,348/0**.

- **2026-07-19** — QT3 Tier-2z: `op-boolean-equal-4` / `and`/`or` register-lifetime fix.
  - `IrLowerer.LowerAnd` and `LowerOr` no longer free the target result register when an operand is lowered into it.
  - This fixes `op-boolean-equal-4`, where `xs:boolean('true') and xs:boolean('true')` was clobbering its left operand register, causing the subsequent `eq` to compare the same value against itself.
  - Added `ApiTests.DebugBooleanEqual` regression test.
  - Full QT3 now **14,785 passed / 92 failed / 16,944 skipped = 46.46%** (runnable pass rate **99.38%**); unit tests **1,347/0**.

- **2026-07-19** — QT3 Tier-2z: duration / date arithmetic cluster.
  - `xs:date` +/− `xs:dayTimeDuration` now returns an `xs:date` with the time components zeroed to `00:00:00`.
  - `xs:time` +/− `xs:yearMonthDuration` now raises `XPTY0004` instead of returning the time unchanged.
  - Generic `xs:duration` values are handled by `fn:*-from-duration` so mixed year-month and day-time components are extracted correctly.
  - `fn:distinct-values` and `fn:index-of` now compare durations using normalized total months and total seconds.
  - Targeted tests pass: `fn-months-from-duration-20`, `K-MonthsFromDurationFunc-7`, `fn-years-from-duration-20`, `K-YearsFromDurationFunc-7`, `K-DateAddDTD-1/2`, `K-DateSubtractDTD-1`, `K-TimeSubtractDTD-2/3/5`, and `distinct-duration-equal-1`.
  - Full QT3 now **14,784 passed / 93 failed / 16,944 skipped = 46.46%** (runnable pass rate **99.38%**); unit tests **1,346/0**.

- **2026-07-19** — QT3 Tier-2z: `union` / `intersect` / `except` XPTY0004 validation.
  - `VmEngine` now validates that all items in both operands of `union`, `intersect`, and `except` are nodes, raising `XPTY0004` for non-node operands.
  - Added `RequireNodeSequence` helper and `LoadNode` VM opcode.
  - Updated `VmOpcodeTests.Concatenate` to use node values.
  - Targeted tests pass: `K2-SeqExcept-1`, `K2-SeqIntersect-1/43/44`, `K2-SeqUnion-5/46/47`.
  - Full QT3 now **14,773 passed / 104 failed / 16,944 skipped = 46.43%** (runnable pass rate **99.30%**); unit tests **1,345/0**.

- **2026-07-19** — QT3 Tier-2z: `fn:adjust-date-to-timezone` / `fn:adjust-time-to-timezone` / `fn:adjust-dateTime-to-timezone` FODT0003 validation.
  - Added `ParseTimezoneOffset` helper to validate timezone offset arguments.
  - Offsets outside `-PT14H` to `+PT14H` (e.g., `PT14H1M`, `-PT14H1M`, `P1D`) now raise `FODT0003`.
  - Offsets with seconds/milliseconds (e.g., `PT14H0M0.001S`) now raise `FODT0003` for violating the one-minute resolution requirement.
  - Targeted pools now all **0 failed**: `fn-adjust-date-to-timezone` 37/0/4, `fn-adjust-time-to-timezone` 37/0/5, `fn-adjust-dateTime-to-timezone` 46/0/2.
  - Full QT3 now **14,766 passed / 111 failed / 16,944 skipped = 46.40%** (runnable pass rate **99.25%**); unit tests **1,345/0**.

- **2026-07-19** — QT3 Tier-2z: `fn-string-length` / `fn-string-join` / `fn-string-to-codepoints` / `fn:remove` / `fn:replace` type checks.
  - `fn:string-length()` zero-arg form now uses `fn:string(.)` semantics, so non-string atomic context items (e.g., integers) are converted to their string representation before counting code points.
  - `fn:string-join()`, `fn:string-to-codepoints()`, `fn:replace()`, and `fn:replace()` four-arg form now use `RequireStringRequired` for required string parameters; the empty sequence raises `XPTY0004`.
  - `fn:remove()` now uses `RequireInteger` for the position argument, raising `XPTY0004` for non-integer atomics or the empty sequence.
  - Added `RequireStringRequired` and `RequireInteger` helpers to `FunctionLibrary`.
  - Targeted pools now all **0 failed**: `fn-string-length` 33/0/3, `fn-string-join` 32/0/14, `fn-string-to-codepoints` 44/0/0, `fn-remove` 51/0/0, `fn-replace` 81/0/10.
  - Full QT3 now **14,755 passed / 122 failed / 16,944 skipped = 46.37%** (runnable pass rate **99.18%**); unit tests **1,345/0**.

- **2026-07-19** — QT3 Tier-2z: `fn-lang` / `fn-in-scope-prefixes` / `fn-codepoints-to-string` fixes.
  - `fn:lang()` now raises `XPDY0002` for an absent context item and `XPTY0004` for a non-node context item; `fn:lang($test, $node)` raises `XPTY0004` when `$node` is not a single node.
  - `fn:in-scope-prefixes()` now raises `XPTY0004` when the argument is not a single element node (e.g., document node or non-node value).
  - Documented `K-CodepointToStringFunc-8/11/12` as XML 1.0-only tests on an XML 1.1 implementation.
  - Targeted pools now **0 failed**: `fn-lang` 36/0/10, `fn-in-scope-prefixes` 9/0/53, `fn-codepoints-to-string` 61/0/18.
  - Full QT3 now **14,747 passed / 130 failed / 16,944 skipped = 46.34%** (runnable pass rate **98.92%**); unit tests **1,345/0**.

- **2026-07-19** — QT3 Tier-2z: `fn-root`/`fn-name`/`fn-local-name` context-item and `fn-QName` QName fixes.
  - Added `GetOptionalSingleNode` helper for `node()?` arguments, raising `XPTY0004` for non-node or multi-item arguments.
  - `fn:local-name()`, `fn:namespace-uri()`, `fn:name()`, `fn:node-name()`, and `fn:root()` now raise `XPDY0002` for an absent context item and `XPTY0004` for a non-node context item.
  - `fn:local-name(())` and `fn:namespace-uri(())` now return the zero-length `xs:string` / `xs:anyURI` per their declared return types, not the empty sequence.
  - `fn:QName((), "local")` now works (empty-sequence namespace URI treated as empty string); `:person` and `person:` lexical forms now raise `FOCA0002`.
  - Targeted pools now all **0 failed**: `fn-root` 11/0/27, `fn-name` 72/0/54, `fn-local-name` 66/0/22, `fn-prefix-from-QName` 27/0/0, `fn-QName` 25/0/9.
  - Full QT3 now **14,743 passed / 137 failed / 16,941 skipped = 46.33%** (runnable pass rate **98.92%**); unit tests **1,345/0**.

- **2026-07-19** — QT3 Tier-2z: duration-arithmetic round-half-up and overflow fixes.
  - `VmEngine.MultiplyDuration` and `DivideDuration` for `xs:yearMonthDuration` now use `RoundHalfUp(totalMonths)` (`floor(x + 0.5)`) per F+O Erratum FO.E12, fixing rounding ties such as `P5M div -2` and `P2Y11M * 2.3`.
  - `xs:dayTimeDuration` multiply/divide no longer casts `xs:double` factors/divisors directly to `decimal`; zero-duration operands return `PT0S`, huge divisors fall back to `double` and round to `PT0S` when below half a tick, and true overflow raises `FODT0002`.
  - Divide by `NaN` raises `FOCA0005`; divide by `0` raises `FODT0002`; divide by `INF`/`-INF` returns `P0M`/`PT0S`.
  - `TryCast` to `xs:duration` now records the generic `duration` schema annotation so the runtime can distinguish `xs:duration` from `xs:yearMonthDuration`/`xs:dayTimeDuration`, making `xs:duration("P1Y3M") * 3` and `xs:duration("P1Y3M") div 3` raise `XPTY0004`.
  - Targeted duration pools now **0 failed** (16 previously failing tests now pass).
  - Full QT3 now **14,720 passed / 160 failed / 16,941 skipped = 46.26%** (runnable pass rate **98.92%**); unit tests **1,344/0**.

- **2026-07-19** — QT3 Tier-2z: `fn-element-with-id` schema-validated ID support.
  - The conformance harness now loads source documents with `validation="strict"` against the environment's declared XML Schema(s), adding PSVI annotations to the XDocument tree.
  - `IXdmNode` gains an `IsId` accessor; `XDocumentNode` computes it from `XmlSchemaInfo` so that elements and attributes with typed values of type `xs:ID` (derived types, union ID members, and singleton lists of `xs:ID`) are recognized.
  - `fn:id()` now returns ID-valued elements themselves (including child `<id>` elements typed as `xs:ID`), and `fn:element-with-id()` returns their parent element when the ID is provided by a child element.
  - `fn:id()` / `fn:element-with-id()` continue to support DTD-declared `ID` attributes via `XDocumentType.InternalSubset`.
  - Targeted `fn-element-with-id` pool now **5 passed / 0 failed / 0 skipped** (5 previously failing tests now pass).
  - Full QT3 now **14,703 passed / 173 failed / 16,945 skipped = 46.21%** (runnable pass rate **98.84%**); unit tests **1,344/0**.

- **2026-07-19** — QT3 Tier-2z: `op/numeric-less-than` unsignedLong overflow fix.
  - `xs:unsignedLong` lexical values that exceed `long.MaxValue` (e.g. `18446744073709551615`) are now represented as `XdmValueKind.Decimal` with the `unsignedLong` subtype annotation, so casts and comparisons work.
  - `ItemInstanceOf` accepts decimal-backed values whose schema type is an integer subtype.
  - Targeted `op-numeric-less-than` pool now **154 passed / 0 failed / 29 skipped** (2 previously failing tests now pass).
  - Full QT3 now **21,620 passed / 317 failed / 9,884 skipped = 67.93%** (runnable pass rate **98.56%**); unit tests **1,343/0**.

- **2026-07-19** — QT3 Tier-2z: `fn/contains` collation/whitespace fixes.
  - Fixed UCA collation strength mapping in `FunctionLibrary.TryParseUca`: `primary` ignores case and non-space accents, `secondary` ignores only case, and `tertiary`/`quaternary` use no ignore flags.
  - Implemented true ASCII-only case folding for the HTML ASCII case-insensitive collation (`http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive`), so only `A-Z`/`a-z` are folded; non-ASCII characters such as `ô`/`Ô` are compared exactly.
  - `fn:contains-token` now tokenizes on XPath whitespace only (`#x20`, `#x9`, `#xD`, `#xA`); non-breaking space (`U+00A0`) is no longer treated as a token separator.
  - Targeted `fn-contains` and `fn-contains-token` pools now **0 failed** (6 previously failing tests now pass).
  - Full QT3 now **21,618 passed / 319 failed / 9,884 skipped = 67.93%** (runnable pass rate **98.54%**); unit tests **1,343/0**.

- **2026-07-19** — QT3 Tier-2z: `cbcl-castable` fixes.
  - `VmEngine` `Castable` opcode catches dynamic cast errors (FOCA0003, FOAR0002) and returns `false` for `castable as`.
  - Empty sequence is now correctly reported as castable only for `?` / `*` occurrence indicators.
  - `prod-CastableExpr` targeted pool now **782 passed / 0 failed / 177 skipped** (was 772/10/177).
  - Full QT3 now **21,607 / 333 / 9,881 = 67.81%** (runnable pass rate **98.48%**); unit tests **1,343/0**.

- **2026-07-19** — QT3 Tier-2z: `fn:format-number` precision and dependency-filter fixes.
  - `FormatNumberEngine` raises `XPTY0004` for non-numeric string inputs in non-BC mode; BC mode still returns the `NaN` symbol.
  - Scientific notation now supports non-BMP (supplementary-plane) zero-digits and counts exponent digit signs correctly.
  - `DependencyFilter` ANDs spec dependencies across `<dependency>` elements, so XP30-only tests are skipped under XP31+.
  - `numberformat63/64` (decimal literals requiring >28 digits of precision) are documented as platform limitations.
  - `fn-format-number` targeted pool now **246 passed / 0 failed / 23 skipped** (was 244/5/20).
  - Full QT3 now **21,612 / 325 / 9,884 = 67.92%** (runnable pass rate **98.52%**); unit tests **1,343/0**.

- **2026-07-19** — QT3 Tier-2z: `fn/matches` caseless-match `i`-flag fix. `i` now maps to `RegexOptions.IgnoreCase`; category escapes `\p{}`/`\P{}` stay case-sensitive via `(?-i:...)`; bracketed classes are case-folded during translation; back-references and quote mode match case-insensitively. Targeted `fn-matches` pool **1,117/0/58** (was 5 failing). Full QT3 now **21,610 / 330 / 9,881 = 67.91%** (runnable pass rate **98.49%**); unit tests **1,343/0**.

- **2026-07-19** — QT3 Tier-2z: `prod-NamedFunctionRef` / `named-function-ref-reserved-function-names` fixes.
  - `XPathParser.ParseNamedFunctionRef` raises `XPST0003` for reserved function names (e.g., `attribute#0`, `element#0`).
  - The reserved-name check is not applied to `ParseFunctionCall` because names like `attribute()` are valid as kind tests.
  - `prod-NamedFunctionRef` targeted pool now **546 passed / 0 failed / 10 skipped** (was 534/12/10).
  - Full QT3 now **21,597 / 343 / 9,881 = 67.79%** (runnable pass rate **98.44%**); unit tests **1,339/0**.

- **2026-07-19** — QT3 Tier-2y: `fn:index-of` fixes.
  - `FunctionLibrary.IndexOfImpl` now uses XPath `eq` semantics via `AtomicValuesEqual` instead of string comparison.
  - NaN no longer matches itself; incompatible types (e.g., `xs:integer` vs `xs:string`) return empty.
  - Empty / multi-item search argument and empty collation argument now raise `XPTY0004`.
  - `fn-index-of` targeted pool now **53 passed / 0 failed / 0 skipped** (was 44/9/0).
  - Full QT3 now **21,585 / 355 / 9,881 = 67.79%** (runnable pass rate **98.38%**); unit tests **1,327/0**.

- **2026-07-19** — QT3 Tier-2x: `op-numeric-mod` fixes.
  - `VmEngine.Modulo` now returns `NaN` for `xs:double`/`xs:float` mod by zero (IEEE 754 semantics).
  - Integer and decimal mod by zero continue to raise `FOAR0001`.
  - `op-numeric-mod` targeted pool now **113 passed / 0 failed / 11 skipped** (was 107/6/11).
  - Full QT3 now **21,576 / 364 / 9,881 = 67.79%** (runnable pass rate **98.34%**); unit tests **1,317/0**.

- **2026-07-19** — QT3 Tier-2w: `fn:has-children` fixes.
  - `FunctionLibrary.HasChildren_0` raises `XPDY0002` when the context item is absent.
  - `FunctionLibrary.HasChildren` unwraps singleton sequences: empty sequence returns `false`, multi-item / non-node arguments raise `XPTY0004`.
  - `fn-has-children` targeted pool now **34 passed / 0 failed / 3 skipped** (was 26/8/3).
  - Full QT3 now **21,570 / 370 / 9,881 = 67.77%** (runnable pass rate **98.31%**); unit tests **1,311/0**.

- **2026-07-19** — QT3 Tier-2v: `op-numeric-integer-divide` fixes.
  - `VmEngine.IntegerDivide` raises `FOAR0002` for NaN/INF operands and returns `0` for finite dividend `idiv` INF/-INF.
  - `xs:float('1e38') idiv xs:float('1e-37')` and similar overflow cases now raise `FOAR0002` instead of returning a truncated `long`.
  - `XPathLexer.ReadNumber` rejects `NumericLiteral` tokens immediately followed by keyword operators (e.g. `10idiv 3` → `XPST0003`).
  - `op-numeric-integer-divide` targeted pool now **125 passed / 0 failed / 11 skipped**.
  - Full QT3 now **21,562 / 378 / 9,881 = 67.76%** (runnable pass rate **98.28%**); unit tests **1,303/0**.

- **2026-07-19** — QT3 Tier-2u: `xs:numeric` cast and constructor support.

- **2026-07-18** — QT3 Tier-2t: `fn:id` / `fn:idref` / `fn:element-with-id` DTD support.
  - `IXdmNode` gains DTD properties (`HasDocumentType`, `DocumentTypeName`, `PublicId`, `SystemId`, `InternalSubset`); `XDocumentNode` exposes `XDocument.DocumentType`.
  - `FunctionLibrary` parses the DTD internal subset for `ID`/`IDREF`/`IDREFS` attribute declarations and caches the result per document node.
  - `fn:idref` now returns the matching attribute node(s) per F+O.
  - `fn:id`/`fn:idref`/`fn:element-with-id` raise `XPTY0004` when the context item or second argument is not a node.
  - `fn-id`/`fn-idref` targeted pool now **54 passed / 0 failed / 61 skipped**.
  - Full QT3 now **21,535 / 405 / 9,881 = 67.68%** (runnable pass rate **98.15%**); unit tests **1,147/0**.

- **2026-07-18** — QT3 Tier-2s: `fn:function-lookup` context-focus capture.
  - `function-lookup` and compiler-generated named function references now capture the creation focus in `NamedFunctionItem`, so context-dependent functions (`fn:base-uri#0`, `fn:document-uri#0`) use the creator's context item during dynamic invocation.
  - `DependencyFilter` now declares `fn-load-xquery-module` unsupported, so tests that assert the feature are skipped rather than failing with `FOQM0001`.
  - Full QT3 now **21,494 / 446 / 9,881 = 67.55%** (runnable pass rate **97.97%**); unit tests **1,283/0**.

- **2026-07-18** — QT3 Tier-2r: `fn:collection()` / `fn:uri-collection()` support.
  - `EvaluationContext.Collections` is now populated by the QT3 harness and used by `fn:collection()` and `fn:uri-collection()` to resolve registered collections, with directory-based fallback and `FODC0002`/`FODC0003`/`FODC0004` error reporting.
  - Full QT3 now **21,511 / 482 / 9,828 = 67.60%** (runnable pass rate **97.81%**); unit tests **1,282/0**.

- **2026-07-18** — QT3 Tier-2q: XQ31-only dependency filter + XdmMap insertion-order fix.
  - `DependencyFilter` now skips positive `spec="XQ31"` dependencies, correctly reclassifying ~116 previously-failing and ~68 previously-passing XQuery-only tests as skipped. Full QT3 now **21,475 / 518 / 9,828 = 67.49%** (runnable pass rate **97.65%**); unit tests **1,282/0**.
  - `XdmMap` restored insertion-order iteration for `Keys`/`Values`/`Entries` via persistent `_keyOrder` and `_keyIndices`; `map:remove`/`map:put` use new `WithRemoved`/`WithAdded` helpers.

- **2026-07-17** — QT3 `op-same-key` hang resolved: **+34 net passed, −20 failed, +14 skipped** (full QT3 now 21,543 / 634 / 9,644 = 67.70%; unit tests 1,286/0).
  - `XdmMap` now uses `ImmutableDictionary<XdmValue, XdmValue>` so `map:remove`, `map:put`, and `map:merge` perform O(log n) structural sharing instead of copying the whole dictionary.
  - `map:remove` and `map:put` rewritten to use the immutable dictionary directly.
  - QT3 harness dependency filter now skips `arbitraryPrecisionDecimal` tests (same-key-008 and same-key-025) because .NET `decimal` is fixed-precision 128-bit.

- **2026-07-15** — QT3 regex/string quick-wins cluster: **+216 passed, −198 failed, zero regressions** (QT3 now 18,698 / 1,742 / 11,381 = 58.76%; unit tests 999/0).
  - Strict XSD regex syntax validation (re00xxx cluster, ~124 tests): malformed quantifiers, bare `{`/`}`/`]`, `(?x` constructs other than `(?:`, octal escapes, .NET-only escapes (`\x \u \A \Z \z \b \B`), trailing backslash, empty char classes, unescaped `[` in classes, and empty-base subtraction all raise FORX0002.
  - Back-references per F&O 5.6.1.4: multi-digit gobbling bounded by previously-opened groups; reference to an unclosed group → FORX0002 (erratum FO.E24).
  - `.` excludes `#xD` as well as `#xA`; `\S` no longer matches CR/TAB/space (unsorted `\s` range broke `Complement`); flag `x` strips pattern whitespace pre-translation (incl. inside `\p{ }` names); multiline `^` no longer matches after a trailing newline but still matches at 0 of the empty string.
  - fn:tokenize no longer interleaves capturing groups (was `Regex.Split`); one-arg fn:tokenize and fn:normalize-space treat only #x20/#x9/#xD/#xA as whitespace (NBSP preserved).
  - XPTY0004 for non-string atomics / empty sequences passed to required string parameters of fn:translate, fn:matches, fn:normalize-unicode.
  - fn:normalize-unicode: case-insensitive trimmed form names, zero-length form = no normalization, FULLY-NORMALIZED implemented (NFC + leading-non-starter check, FOCH0003 otherwise).
  - QT3 harness: `DocumentedSkips` per-test skip list with reasons (upstream defects, platform limitations).

- **2026-07-14** — W3C `unicode-90` conformance set enabled: **1,365 passed / 0 failed / 95 skipped** (1,460 tests; all skips are upstream test/data defects, documented in the harness).
  - New XSD character-class regex engine `XsdCharClasses` with pinned **Unicode 9.0.0** data (`UnicodeData90`, generated from UCD 9.0): all 38 general categories (incl. grouped `LC`), `\p{IsBlock}` script blocks, `\d \D \w \W \s \S \i \I \c \C`, ranges, negation, unions, and class subtraction `[A-[B]]`; astral ranges are emitted as surrogate-pair alternations so astral characters are never split. `\w` follows the XSD definition `[^\p{P}\p{Z}\p{C}]` (emoji are word characters). Unknown category/block → `FORX0002`.
  - Regex translation and compiled-`Regex` caches keyed by the short original pattern (`RegexHelper.ValidateAndTranslatePatternCached` / `GetRegex`), wired into `fn:matches`/`fn:replace`/`fn:tokenize`/`fn:analyze-string` and `xsl:analyze-string`. Compiled regexes are used throughout: `RegexOptions.NonBacktracking` silently mis-matched U+000A on large translated alternations (probe-verified; regression test added).
  - `fn:codepoints-to-string` validity now follows the XML 1.1 `Char` production exactly (C0 controls except NUL, U+FDD0..FDEF and astral xFFFE/xFFFF are legal; surrogates, U+FFFE/U+FFFF, NUL → `FOCH0001`). `fn:translate` is Rune-based (astral pairs no longer split).
  - `fn:concat` is registered up to arity 32 (unicode-90 uses `concat#16`).
  - `VmEngine` general comparison fast path: `=`/`!=` between a single `xs:integer` and a large all-integer sequence uses a cached `HashSet<long>`, so `$validrange[not(. = $c)]` (1.1M × 2,063 comparisons per unicode-90 test) is O(n).
  - Harness: injects the `charclass` stylesheet parameter for Gen tests (the upstream generator omits it), caches the 54MB data documents, and drops degenerate empty-`@c` entries (U+FFFE/U+FFFF placeholders in `unicode-C.xml`/`unicode-Cn.xml`) on load.
  - Skipped upstream defects: `unicode90-001..008` (BMP-only expected counts contradict this suite's own Gen tests), `unicode90-{cat}-033/035` (fn-replace3/5 compare against `string-join` of empty `<c>` elements — broken in w3c/xslt30-test master), `unicode90-Cs-001..004/023` + `unicode90-Zl-023`/`Zp-023` (empty/one-member categories → invalid quantifiers), `unicode90-L-017/038` + `unicode90-Lo-017/038` (stylesheet `$validrange` omits U+10000 but the documents include it).
  - Full W3C suite: **7,109 passed / 0 failed / 7,491 skipped** — 100% of runnable tests.

- **2026-07-14** — HOF unskip + snapshot cluster: higher-order functions fully enabled; snapshot set 19/0/24; seqtor/static/regex/system-property/current-output-uri sets green.
  - `fn:snapshot` now matches the spec-equivalent stylesheet implementation (`snapshot-equivalent.xsl`) node-for-node: non-node items pass through unchanged, ancestor grafting preserves parentage, namespace declarations are excluded from attribute comparisons in `fn:deep-equal`, and in-scope namespaces are not redeclared on copied descendants.
  - Typed templates (`xsl:template/@as`) now collect results through the placeholder sequence accumulator, so node identity and parentage survive template boundaries; `xsl:element` suspends the accumulator while constructing content (fixes `__xdm_seq__` placeholder leak, `namespace-0912`).
  - Function-body results no longer clone a single text node (`NormalizeSequenceConstructorItems`), preserving text-node parentage through `xsl:function` results (`snapshot-0102a`).
  - `namespace-node()` is now a valid match pattern (priority −0.5) matching namespace-axis nodes.
  - `fn:concat` / `fn:compare#2` register their `xs:anyAtomicType?` parameters as pass-through; dynamic-call argument conversion no longer stringifies arbitrary atomics to `xs:string` (`higher-order-functions-064` raises XPTY0004 again).
  - Other fixes: `fn:min`/`fn:max` return `xs:integer` for all-integer input; `system-property()` expands `Q{uri}local` and reports `xsl:supports-higher-order-functions`; `xsl:function` accepts `cache`; user functions in map/math/array reserved namespaces raise XTSE0080; TVT/§4.3 whitespace handling; missing F&O registrations (`element-with-id#2`, `idref`, `uri-collection`, `xs:error`).
  - Full W3C suite: **5,744 passed / 0 failed / 8,856 skipped** — every runnable test passes, including the complete `fn:transform` set (transform-001..009).

- **2026-07-13** — Skip-pool audit: unskipped `position-0103` (xsl:merge) and `position-2201` (xsl:result-document); both pass now that the features they gate on are implemented. Full W3C suite: **5,607 passed / 0 failed / 8,993 skipped**.

- **2026-07-13** — Phase 5n: all remaining singleton failures cleared; **zero failing runnable tests** in the W3C XSLT 3.0 suite.
  - `attribute-0701`: HTML serialization now minimizes recognized boolean attributes (`checked`, `selected`, `disabled`, ...) whose value equals their name, restricted to the HTML boolean allowlist so attributes such as `ffi="ffi"` keep the explicit form.
  - `backwards-019b`: `escape-uri-attributes` now defaults to true for the XHTML method as well as HTML. The XSLT 3.0 backwards-compatibility rule is implemented via new `OutputProperties.EffectiveVersion` / `ImplicitResultTree` flags: in version-1.0 mode an implicitly generated result tree infers the `xml` (not `xhtml`) output method, while an explicit `xsl:result-document` still infers `xhtml` (`backwards-019` vs `backwards-019b`).
  - `maps-017`: the conformance harness unwraps JSON-string-serialized results (json/adaptive output of a node) before reparsing for tree assertions.
  - `merge-021`: `XTDE2210` is now also raised when a merge-key attribute (`lang`, `order`, `collation`, `case-order`, `data-type`) is present on one of two corresponding `xsl:merge-key` elements and absent on the other, per the XSLT 3.0 spec as written.
  - `include-0101`: two fixes. (1) `xsl:include`/`xsl:import` hrefs now resolve against the *element's* base URI, so modules pulled in through DTD external entities resolve nested imports relative to the entity location. (2) The unnamed `xsl:output` declarations are now merged across the module tree by import precedence via `Stylesheet.EffectiveOutputProperties` (previously only the principal module's declarations were used), so the included module's `method="html"` correctly overrides the imported module's `method="xml"`.
  - Full W3C suite: **5,605 passed / 0 failed / 8,995 skipped** (100% of runnable tests).

- **2026-07-13** — Phase 5m `select` conformance cluster cleared.
  - The conformance harness now honors the `encoding` attribute on assertion elements (`assert-serialization`, `assert-xml`) when reading expected-result files, so ISO-8859-1 expected outputs decode correctly (`select-6101`).
  - W3C `select` conformance set: **157 passed / 0 failed / 1 skipped**.
  - Full W3C suite: **5,600 passed / 5 failed / 8,995 skipped** (99.9%).
  - Remaining failures: `attribute-0701`, `backwards-019b`, `include-0101`, `maps-017`, `merge-021`.

- **2026-07-13** — Phase 5l `bug` conformance cluster cleared.
  - `ResultTreeSerializer`'s text output method no longer emits comment/PI markup — comment and processing-instruction nodes contribute nothing to `method="text"` output (`bug-1405`).
  - The conformance harness now self-closes HTML void elements (`meta`, `br`, `img`, ...) when reparsing HTML output for tree assertions, so the HTML5-mandated unclosed `<meta ...>` no longer breaks XPath assertions (`bug-1301`).
  - `assert-xml` comparisons now strip the serialization-injected `meta http-equiv="Content-Type"` element: assert-xml compares result trees, and the meta is a serialization artifact (`bug-1901`). The serializer still injects it whenever `include-content-type` is in effect, as required by `output-0123` and `backwards-018`.
  - Also clears `select-6201` (HTML table serialization).
  - W3C `bug` conformance set: **75 passed / 0 failed / 11 skipped**.
  - Full W3C suite: **5,599 passed / 6 failed / 8,995 skipped** (99.9%).
  - Remaining failures: `attribute-0701`, `backwards-019b`, `include-0101`, `maps-017`, `merge-021`, `select-6101` (since cleared).

- **2026-07-13** — Phase 5k `for-each-group` conformance cluster cleared.
  - `FunctionSignature` gains an optional `DynamicImplementation`; `VmEngine.InvokeFunctionItem` uses it for dynamic calls through function items (named references and partial application) while static calls keep using `Implementation`.
  - `TransformEngine.RegisterGroupingFunctions` now supplies dynamic implementations that raise `XTDE1061` (`current-group`), `XTDE1071` (`current-grouping-key`), `XTDE3480` (`current-merge-group`), and `XTDE3510` (`current-merge-key`), per XSLT 3.0: these context components are not retained in the closure of a function item.
  - W3C `for-each-group` conformance set: **78 passed / 0 failed / 7 skipped** (plus 114 streaming tests skipped).
  - Full W3C suite: **5,595 passed / 10 failed / 8,995 skipped** (99.8%).
  - Remaining failures: `bug` (3), `select` (2), `attribute-0701`, `include-0101`, `maps-017`, `merge-021`, `backwards-019b`.

- **2026-07-13** — Phase 5j `output` conformance cluster cleared.
  - `ResultTreeSerializer.SerializeAsXhtml` now normalizes text/attribute values before CDATA wrapping, so `cdata-section-elements` combined with `normalization-form` split unrepresentable normalized characters correctly (`output-0115d`).
  - `TransformEngine.IsRawCollectionTopLevel` is now scoped to the actual principal/secondary result document, so literal elements inside `xsl:variable/@as` bodies are no longer swallowed by raw-item collection (`output-0716`, `output-0717`).
  - `XdmJsonSerializer.SerializeValue` now handles sequence-valued array/map members, so nested HTML/XML nodes serialize as JSON strings instead of `"(sequence)"` (`output-0702`).
  - Also clears `arrays-304`.
  - W3C `output` conformance set: **232 passed / 0 failed / 29 skipped**.
  - Full W3C suite: **5,593 passed / 12 failed / 8,995 skipped** (99.8%).
  - Remaining failures: `bug` (3), `select` (2), `for-each-group` (2), `attribute` (1), and singleton regressions in `include`, `maps`, `merge`, and `backwards`.

- **2026-07-13** — Phase 5i `normalize-unicode` conformance cluster cleared.
  - `ResultTreeSerializer` now escapes tab, line-feed, and carriage-return characters inside XML attribute values as decimal numeric character references (`&#9;`, `&#10;`, `&#13;`) after `XmlWriter` emits them literally. This aligns the `xml` output method with XSLT/XQuery Serialization 3.1.
  - Also clears `copy-3801` and `attribute-1101` (attribute-value whitespace regressions).
  - W3C `normalize-unicode` conformance set: **18 passed / 0 failed / 0 skipped**.
  - Full W3C suite: **5,588 passed / 17 failed / 8,995 skipped** (99.7%).
  - Remaining clusters: `output` (4), `bug` (3), `select` (2), `for-each-group` (2), `attribute` (1), and singleton regressions in `arrays`, `backwards`, `include`, `maps`, and `merge`.

- **2026-07-13** — Phase 5h `character-map` conformance cluster cleared.
  - Adaptive output now uses XPath/XQuery string-literal escaping (double quotes) instead of JSON escaping, so character-map replacements in strings and maps are serialized correctly.
  - `fn:current-time()` now keeps the date part on day 1 of year 1 when possible and falls back to day 2 only when a positive timezone offset would underflow `DateTimeOffset.MinValue`.
  - W3C `character-map` conformance set: **29 passed / 0 failed / 0 skipped**.
  - Full W3C suite: **5,565 passed / 40 failed / 8,995 skipped** (99.3%).
  - Remaining clusters: `mode` (9), `xml-version` (7), and scattered regressions.

- **2026-07-12** — Phase 5g `inherit-namespaces="no"` conformance fix; the W3C `namespace` cluster is now clear.
  - `TransformEngine.FinalizeNamespaceInheritance` attaches `PrefixedNamespaceUndeclarations` to children of `NamespaceInheritanceBarrier` elements so `xmlns:prefix=""` is emitted where required.
  - The synthetic `__xdm_doc__` root is detached before being wrapped in the final `XDocument`, preserving namespace annotations instead of cloning them away.
  - `ResultTreeSerializer` routes trees with prefixed namespace undeclarations to the raw XML serializer.
  - W3C `namespace` conformance set: **203 passed / 0 failed / 21 skipped**.
  - Full W3C suite: **5,561 passed / 44 failed / 8,995 skipped** (99.2%).

- **2026-07-12** — Phase 5f final serialization clusters cleared.
  - `fn:current-output-uri()` now returns the base output URI at the top level and remains empty in temporary output state (functions, variables, sort/merge keys, patterns), clearing the `current-output-uri` cluster.
  - Original namespace prefixes are preserved for sibling elements that map to the same URI, clearing `output-0138`.
  - The `output` conformance cluster now has **0 failures** (203 passed / 0 failed / 29 skipped); the `current-output-uri` cluster has **0 failures** (15 passed / 0 failed / 2 skipped).
  - Full W3C suite: **5,544 passed / 61 failed / 8,995 skipped** (98.9%).

- **2026-07-12** — Phase 5e remaining `output` serialization edge cases.
  - Arrays are now flattened in sequence constructors, fixing `output-0713`–`output-0715`.
  - `json-node-output-method="html"` now injects the HTML content-type `<meta>` element (`output-0716`).
  - Adaptive output preserves `omit-xml-declaration` and applies parameter-document character maps (`output-0721`).
  - XML comments preserve `\r` characters literally (`output-0723`).
  - `SEPM0009` is restricted to XML/XHTML and now covers `version != 1.0` with `doctype-system`; `SEPM0010` validates `undeclare-prefixes` for XML 1.1.
  - Text output now writes the BOM, applies character maps, and honors `normalization-form`; HTML DOCTYPE is emitted immediately before the first element.
  - Full W3C suite: **5,538 passed / 67 failed / 8,995 skipped** (98.8%).

- **2026-07-11** — Phase 5 JSON output method: `xsl:output method="json"`, `json-node-output-method`, `allow-duplicate-names`, `escape-solidus`, and `xsl:output parameter-document` with inline character maps.
  - Top-level `xsl:map`/`xsl:map-entry` results are preserved for JSON serialization instead of being rejected as element/document children.
  - HTML node serialization inside JSON strings now emits XHTML namespace declarations and escapes solidus characters per XSLT/XQuery Serialization 3.1 defaults.
  - W3C `output` conformance set: **175 passed / 28 failed / 29 skipped** (was 168/35/29). Remaining JSON/text failures include `output-0703` (item-separator) and `output-0710`/`0711` (unescaped keys).
  - Full W3C suite: **5,477 passed / 128 failed / 8,995 skipped** (97.7%).

- **2026-07-11** — Phase 5b JSON/text output edge cases: `item-separator` and `SENR0001` validation.
  - `item-separator` is now honored for `method="text"`, fixing `output-0703`, `output-0709`, `output-0718`, and `output-0719`.
  - Maps, arrays, and functions at the top level of XML, HTML, XHTML, or text output now raise `SENR0001`, fixing `output-0710`, `output-0711`, and `output-0712`.
  - W3C `output` conformance set: **179 passed / 24 failed / 29 skipped** (was 175/28/29).
  - Full W3C suite: **5,481 passed / 124 failed / 8,995 skipped** (97.8%).

- **2026-07-11** — Phase 5c `xsl:result-document` serialization fixes.
  - AVTs are now evaluated for all `xsl:result-document` serialization attributes, including `html-version`, `byte-order-mark`, and `allow-duplicate-names`.
  - `yes`/`no` attribute values are now case-sensitive; uppercase variants raise `XTSE0020`.
  - `SEPM0009` is only raised for XML/XHTML methods that emit an XML declaration.
  - `xsl:result-document` now supports raw-item collection for `method="json"`, `method="adaptive"`, and `build-tree="no"`.
  - W3C `result-document` conformance set: **104 passed / 21 failed / 29 skipped** (was 86/39/29).
  - Full W3C suite: **5,506 passed / 99 failed / 8,995 skipped** (98.2%).

- **2026-07-11** — Phase 4 serialization fixes: named-output import precedence, XHTML 1.0 empty-element handling, DOCTYPE quote/namespace rules, and `fn:current-output-uri()` scoping.
  - W3C `output` conformance set: **168 passed / 35 failed / 29 skipped** (was 155/48/29).
  - Full W3C suite: **5,473 passed / 132 failed / 8,995 skipped** (97.6%).

- **2026-06-26** — Fixed `normalize-unicode-014`: HTML result-tree serialization now applies `xsl:output/@normalization-form` (NFC/NFD/NFKC/NFKD) to text, attribute values, comments, and processing instructions.
  - Full W3C suite: **5,238 passed / 5 failed / 9,357 skipped** (99.9%).
  - Remaining failures: `catalog-006/007`, `docbook-001/002/004`.

- **2026-06-26** — Fixed `accumulator-090`: global variables that call `accumulator-after()` no longer trigger a false `XPST0008` circular-reference error. The accumulator evaluation context now copies globals lazily but skips the variable currently being initialized, preserving access to globals referenced by accumulators (e.g., `merge-066`).
  - Full W3C suite: **5,237 passed / 6 failed / 9,357 skipped** (99.9%).
  - Remaining failures: `normalize-unicode-014`, `catalog-006/007`, `docbook-001/002/004`.

- **2026-06-26** — Fixed `function-1014` (FXSL higher-order recursion): `xsl:apply-templates` and `xsl:call-template` inside `xsl:function` bodies now expand `__xdm_seq__` placeholders so atomic values returned by `xsl:sequence` reach the function result instead of being dropped.
  - Full W3C suite: **5,236 passed / 7 failed / 9,357 skipped** (99.9%).
  - Remaining failures: `accumulator-090`, `normalize-unicode-014`, `catalog-006/007`, `docbook-001/002/004`.

- **2026-07-08** — Cleared the W3C `unparsed-text`, `match`, `forwards`, `lre`, `whitespace`, `xslt-compat`, and `for-each-group` conformance clusters.
  - `fn:unparsed-text()` one-argument form detects encoding from BOM, XML declaration, and HTTP `Content-Type`; `unparsed-text-available()` works for HTTP resources; sequence arguments are atomized.
  - `xsl:template/@match` no longer rejects `Q{uri}*` / `except` patterns as AVTs.
  - Forward-compatibility mode ignores unknown XSLT elements/attributes and unresolvable `use-when` expressions when the effective version is > 3.0.
  - Maps and functions raise `XTDE0450` when serialized directly to element content.
  - QName names in `xsl:element`/`xsl:attribute` are whitespace-normalized before validation.
  - Backwards-compatible mode converts atomic values to strings for string functions.
  - `fn:distinct-values` treats NaN as equal.
  - `xsl:function` bodies follow XSLT text-node merging rules while preserving consecutive zero-length text nodes.
  - Full W3C suite: **5,233 passed / 10 failed / 9,357 skipped** (99.8%).

- **2026-07-05** — Cleared the W3C `tunnel` conformance cluster (58 runnable tests pass; 0 failed).
  - `xsl:with-param/@tunnel` and `xsl:param/@tunnel` now accept `yes`/`no` (XSLT 2.0) and `true`/`false`/`1`/`0` (XSLT 3.0); invalid/empty values raise `XTSE0020`.
  - `xsl:call-template` enforces `XTSE0680` when an ordinary `xsl:with-param` matches a tunnel `xsl:param` (or vice versa) and when no matching parameter is declared.
  - `xsl:call-template` parameter validation now skips `xsl:context-item` children and is suppressed in XSLT 1.0 backwards-compatible mode, so extra parameters are silently ignored.
  - Tunnel parameters are correctly isolated from `xsl:function` bodies and pass through intermediate named templates, `xsl:apply-templates`, `xsl:apply-imports`, and `xsl:next-match`.
  - Tunnel parameters now bind only to tunnel-declared `xsl:param`s; non-tunnel `xsl:with-param`s no longer shadow tunnel parameters.

- **2026-07-05** — Cleared the W3C `avt` conformance cluster (35 runnable tests pass; 0 failed).
  - AVT expressions now correctly handle XPath comments (`(: ... :)`), empty expressions, escaped `}}`, and `{{` even when no `{` expression is present.
  - `xsl:attribute/@separator`, `xsl:value-of/@separator`, and `xsl:sort/@stable` are now evaluated as attribute value templates.
  - AVTs in XSLT 1.0 backwards-compatibility mode take only the first item of the expression value, matching `string()` semantics.
  - `xsl:value-of` inside `xsl:function` bodies constructs real text nodes, so functions declared `as="text()*"` return the expected node kind.
  - `xsl:template/@match` now rejects AVT syntax with `XTSE0340`.

- **2026-07-05** — Cleared the W3C `collations` conformance cluster (43 runnable tests pass; 0 failed).
  - `xsl:stylesheet`/`xsl:template`/`xsl:*` `default-collation` attributes now flow into the XPath evaluation context, so `eq`, `=`, `fn:compare`, `fn:starts-with`, `fn:contains`, `fn:ends-with`, etc. use the correct collation without an explicit argument.
  - `xsl:for-each-group` and `xsl:key` use the effective default collation when no explicit `@collation` is supplied.
  - `xsl:sort` with `case-order="upper-first"`/`"lower-first"` works even when no `@lang` or `@collation` is present (primary comparison is case-insensitive, with case as the tie-breaker).
  - Collation-aware aggregate functions (`fn:max`, `fn:min`, `fn:index-of`, `fn:distinct-values`, `fn:deep-equal`) now honor the in-scope default collation.
  - UCA collations with `fallback=no` raise `FOCH0002`, matching the implementation-defined fallback behavior expected by the test suite.

- **2026-07-04** — Cleared the W3C `iterate` conformance cluster (44 runnable tests pass; 0 failed; 35 streaming tests skipped).
  - `xsl:iterate` now works in the result-tree path with `xsl:param`, `xsl:next-iteration`, `xsl:break`, and `xsl:on-completion`.
  - `xsl:next-iteration`/`xsl:with-param` values are coerced to the declared `xsl:param` type, so atomization happens when required.
  - `xsl:on-completion` and `xsl:break` sequence-constructor content are evaluated as document-producing constructors, so nested `xsl:copy-of` inside literal elements contributes correctly.
  - `xsl:try` now rolls back output written in the try block before executing `xsl:catch`, using efficient last-node/last-attribute snapshots. This fixes `iterate-036` and prevents the `catalog` self-tests from hanging.
  - Full W3C suite: **5,073 passed / 177 failed / 9,350 skipped** (~96.6%).

- **2026-07-02** — Cleared the W3C `seqtor` conformance cluster (54 runnable tests pass; 18 skipped).
  - Sequence-constructor whitespace and empty atomic items now produce correct spacing in complex content.
  - Empty sequence items act as atomic separators; text-node/atomic merging and adjacent-text concatenation match the XSLT 3.0 serialization rules.
  - `xsl:sequence` without `@select` now returns its raw sequence-constructor content.
  - `xsl:document` inside `xsl:comment`, `xsl:processing-instruction`, and `xsl:attribute` simple content is handled correctly.
  - Namespace prefix `xs` is now declared when evaluating Text Value Templates.
  - Mixed atomics and text nodes produced by `xsl:function` are serialized correctly.
  - Full W3C suite: **4,964 passed / 286 failed / 9,350 skipped** (~94.6%).

- **2026-07-03** — Cleared the quick-win conformance clusters `available-system-properties`, `on-empty`, `copy`, and `where-populated`.
  - `fn:available-system-properties` now returns `xs:QName` values and includes all required XSLT system properties.
  - Sequence-constructor placeholders no longer count as significant content, so `xsl:on-empty` fires correctly for empty `xsl:sequence` results.
  - `xsl:where-populated` now expands sequence placeholders produced by `xsl:sequence`, preserving arrays and other sequence values.
  - Full W3C suite: **4,999 passed / 251 failed / 9,350 skipped** (~95.2%).

- **2026-06-26** — Cleared the W3C `as`, `xml-to-json`, and `json-to-xml` conformance clusters.
  - `xs:float` serialization now uses the shortest round-trip `"R"` format in the scientific range, fixing `as-0802` / `as-0802b`.
  - `fn:json-to-xml` now honors the `duplicates` option (`use-first`, `retain`, `reject`) and reports `FOJS0005` / `XPTY0004` for invalid option values.
  - `fn:codepoints-to-string` now accepts XML 1.1 C0 control characters, allowing `xml-to-json` to serialize backspace/bell/form-feed as JSON escapes.
  - Full W3C suite: **4,953 passed / 297 failed / 9,350 skipped** (~94.3%).

- **2026-06-30** — Cleared the W3C `match` conformance cluster (1 failure → 0).
  - `xsl:mode` declarations without an explicit `@on-no-match` now default to `text-only-copy` per the XSLT 3.0 spec, so atomic items processed by `xsl:apply-templates` produce their string value in the default mode.
  - The built-in rule for atomic values now respects the effective mode's `on-no-match` behavior (deep-skip and shallow-skip suppress output).
  - Full W3C suite: **4,944 passed / 306 failed / 9,350 skipped** (~94.2%).

- **2026-06-30** — Cleared the W3C `current-output-uri` conformance cluster (1 remaining failure → 0).
  - `TransformEngine.TransformFunction` now compiles template match patterns and registers grouping functions before executing a stylesheet function, so `xsl:apply-templates` inside `xsl:function` can match template rules.
  - Result-document URI tracking is reset at function entry points.
  - Full W3C suite: **4,943 passed / 307 failed / 9,350 skipped** (~94.2%).

- **2026-06-28** — Cleared the W3C `apply-templates` conformance cluster (11 runnable failures → 0).
  - `match="/"` now uses the correct default priority of `-0.5` in XSLT 2.0/3.0.
  - Root-template selection now respects the default (unnamed) mode and applies conflict resolution, fixing `mode="#current"` through `xsl:call-template`.
  - `document-node(element(E))` and `document-node(element(*))` match patterns now compile and match correctly.
  - `xsl:apply-templates` with no `@select` raises `XTTE0510` when the context item is not a node.
  - `xsl:apply-imports` and `xsl:next-match` now forward ordinary parameters to the built-in rule fallback.
  - Ambiguous template matches raise `XTRE0540` for test cases that declare `on-multiple-match="error"` via the new `XsltCompiler.TreatRecoverableAmbiguousMatchAsError` flag.
  - Full W3C suite: **4,871 passed / 379 failed / 9,350 skipped** (~92.8%).

- **2026-06-28** — Restored the W3C `catalog` self-test set and fixed the O(N²) slowness that made it hang.
  - `NormalizeSequence` in the XPath VM now removes duplicate nodes with a `HashSet<IXdmNode>` instead of a nested loop, dropping large cross-document sequences from >10 minutes to seconds.
  - The `catalog` cluster now completes in under a minute; remaining failures are XML 1.1 parser limitations and a `catalog-007` `element-available()` mismatch.
  - Full W3C suite: **4,855 passed / 395 failed / 9,350 skipped** (~92.5%).

- **2026-06-28** — Cleared the `resolve-uri` cluster (24/24) and the `namespace-4801` regression.
  - `fn:resolve-uri()` now raises `FORG0002` for malformed relative URIs and relative base URIs per erratum FO.E1.
  - Dotted-path URIs resolve correctly in `fn:resolve-uri()` and `fn:static-base-uri()`.
  - `fn:document()` resolves relative URIs against the base URI of the supplied node argument.
  - Text/PI nodes produced by DTD entity expansion now inherit the parent element's resolved `xml:base`.
  - TVT evaluation passes the context element so the compiled XPath uses the correct in-scope namespaces and effective base URI.
  - Full W3C suite: **4,852 passed / 391 failed / 9,357 skipped** (~92.5%).
  - The `catalog` self-test set is temporarily skipped because it became extremely slow after the `document()` node-base fix.

- **2026-06-28** — Fixed the remaining XSLT `namespace` cluster failure (`namespace-3005`).
  - Top-level `xsl:namespace` instructions now produce a standalone namespace-node item when the containing sequence constructor is typed as `node()` or `node()?` (in addition to explicit `namespace-node()` types).
  - Full W3C suite: **4,845 passed / 405 failed / 9,350 skipped** (~92.3%).

- **2026-06-27** — Cleared the remaining single-failure clusters `sort`, `merge`, and `arrays`.
  - `sort-072`: `xsl:perform-sort` now preserves in-scope prefixed namespaces for relocated sequence-constructor children.
  - `merge-066`: XPath prefix validation no longer treats integer map keys (e.g. `map{1:xs:dateTime(...)}`) as undeclared QName prefixes.
  - `square-array-201`: `xsl:source-document` with `streamable="no"` now loads the document and evaluates its content with the loaded document as the context item.

- **2026-06-27** — Cleared the entire XSLT `math` conformance cluster (15 runnable failures).
  - `fn:number` now parses XPath lexical forms `INF`, `-INF`, and `NaN`.
  - `fn:floor`, `fn:ceiling`, and `fn:round` now atomize their arguments before numeric dispatch.
  - `fn:round` / `fn:round-half-to-even` now use `decimal` arithmetic for precision-bound decimal/integer values, fixing tie-rounding and large-integer precision loss.
  - `xs:double` and `xs:float` serialization now uses shortest round-trip formatting (`"G16"` / `"G9"`) in the scientific-notation range, producing canonical output such as `1.0E-98` instead of `1.0000000000000001E-98`.

- **2026-06-27** — Cleared the entire XSLT `maps` conformance cluster (35 runnable failures).
  - Implemented `xsl:map` and `xsl:map-entry` instructions in `TransformEngine`, including duplicate-key (`XTDE3365`) and non-entry-content (`XTTE3365`) errors, and `XTDE0450` when a map is used as an element/document child.
  - `fn:serialize` now supports `method=json` for maps, arrays, booleans, numbers, and strings.
  - Map key equality now treats `xs:anyURI` as comparable to `xs:string` and handles `NaN` numeric keys safely.
  - `XPath31Expression.Compile` resolves function-call namespaces from `CompileOptions` and reports static `XPST0017` for removed functions and the obsolete `http://www.w3.org/2011/xpath-functions/map` namespace.
  - The conformance harness expands W3C test-suite `_select` AVT attributes using static parameters before compilation.
  - Follow-up fixes: preserve explicit `Q{uri}local` namespace URIs in function calls/named function refs; fall back to run-time `_select` expansion when static parameters are insufficient; atomize/flatten arrays for `xsl:apply-templates`, `xsl:value-of`, AVTs, and complex content construction.

- **2026-06-27** — Cleared the remaining XSLT `namespace` cluster failures (`namespace-0912` and `namespace-2611`) and the full `namespace-alias` cluster.
  - Built-in `shallow-copy` now suspends the outer sequence accumulator while applying templates to children, so typed variables containing shallow-copied elements keep child results nested instead of escaping as siblings.
  - `XdmValue.EffectiveBooleanValue()` now follows XPath sequence EBV rules: empty sequence → `false`, singleton sequence → EBV of its item, multi-node sequence → `true`, multi-item atomic sequence → `FORG0006`.

- **2026-06-26** — Cleared the XSLT `date` conformance cluster (46 runnable failures).
  - `xsl:value-of` now evaluates the `_select` AVT used by static-parameter test stylesheets.
  - `fn:format-date`, `format-time`, and `format-dateTime` now support escaped brackets (`[[` / `]]`), roman/alphabetic presentations, ISO week-of-month around year boundaries, non-BMP digit families, correct default widths, and timezone semantics (`[Z]` / `[z]`).
  - `fn:adjust-dateTime-to-timezone` now preserves the target timezone offset instead of returning a zero-offset value.

## 1. Consuming Bosak

### 1.1 Via Project References (development)

Add project references to the Bosak layer stack from your consuming project:

```xml
<ItemGroup>
  <ProjectReference Include="..\Bosak\src\Bosak.XPath.Api\Bosak.XPath.Api.csproj" />
  <ProjectReference Include="..\Bosak\src\Bosak.Xslt\Bosak.Xslt.csproj" />
  <!-- <ProjectReference Include="..\Bosak\src\Bosak.XQuery\Bosak.XQuery.csproj" /> -->
</ItemGroup>
```

`Bosak.XPath.Api`, `Bosak.Xslt`, and `Bosak.XQuery` pull in the lower layers automatically (Core, Parser, Compiler, Runtime, Standard, Providers).

**Target framework:** `net10.0` (both sides must align).

### 1.2 Via NuGet Packages

All Bosak source projects are now packable. After packing (`dotnet pack`), the following packages are produced:

| Package | Description |
|---------|-------------|
| `Bosak.Xslt` | XSLT 3.0 processor and transform engine |
| `Bosak.XPath.Api` | Public API for compiling and evaluating XPath 3.1 |
| `Bosak.XPath.Core` | XDM types and core abstractions |
| `Bosak.XPath.Runtime` | Register-based VM execution engine |
| `Bosak.XPath.Standard` | Standard XPath 3.1 / XQuery function library |
| `Bosak.XPath.Providers` | `IXdmNode` adapters for `System.Xml.Linq` |
| `Bosak.XPath.Parser` | Recursive-descent XPath 3.1 parser |
| `Bosak.XPath.Compiler` | AST-to-IR compilation pipeline |

**Consuming from a private feed:**
```bash
# Pack all projects
dotnet pack Bosak.sln --output ./nupkgs

# Push to your private NuGet feed
dotnet nuget push ./nupkgs/Bosak.Xslt.1.0.0.nupkg --source https://your-feed/nuget/v3/index.json
```

Then reference in your consuming project:
```xml
<ItemGroup>
  <PackageReference Include="Bosak.Xslt" Version="1.0.0" />
  <PackageReference Include="Bosak.XPath.Providers" Version="1.0.0" />
</ItemGroup>
```

> **Note:** Transitive dependencies are automatically resolved. You only need to reference the top-level packages your code directly uses (`Bosak.Xslt` and/or `Bosak.XPath.Api`).

---

## 2. XPath 3.1 Expressions

### 2.1 Compile & Evaluate

```csharp
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;

// One-shot evaluation
var expr = XPath31Expression.Compile("/invoice/items/item[@price > 100]");
var result = expr.Evaluate(document);

// Re-use compiled expression
var expr2 = XPath31Expression.Compile("$minPrice + $taxRate * $amount");
var result2 = expr2.Evaluate(
    new EvaluationContext()
        .WithVariable("minPrice",  XdmValue.FromDecimal(100.00m))
        .WithVariable("taxRate",   XdmValue.FromDecimal(0.21m))
        .WithVariable("amount",    XdmValue.FromDecimal(500.00m)));
```

### 2.2 Evaluation Context

```csharp
using Bosak.XPath.Runtime.Vm;

var ctx = new EvaluationContext
{
    BaseUri = "file:///C:/Data/",
    DocumentLoader = uri => /* your IXdmNode loader */
};

// Redirect published (e.g. http:) resource URIs to local files. Consulted by
// fn:doc, fn:json-doc, fn:unparsed-text(-available/-lines), and fn:transform's
// stylesheet-location before any filesystem or network access. Return null for
// URIs that should follow the normal resolution path.
ctx.ResourceUriMapper = uri =>
    uri == "http://example.org/published/spec.xml"
        ? @"C:\Data\local-copy.xml"
        : null;

// Pre-register a source document so fn:doc(document-uri($node)) returns the same node.
ctx.RegisterDocument("file:///C:/Data/input.xml", sourceDocument);

ctx.WithNamespace("edi", "http://example.org/edi")
   .WithVariable("docId", XdmValue.FromString("DOC-1234"));

var result = expr.Evaluate(ctx);
```

### 2.3 Reading Results

```csharp
if (result.IsNode && result.NodeValue is { } node)
{
    Console.WriteLine(node.StringValue);
}
else if (result.IsSequence && result.SequenceValue is { } seq)
{
    foreach (var item in XdmSequence.FromSource(seq))
        Console.WriteLine(item.ToString());
}
else
{
    Console.WriteLine(result.ToString());
}
```

### 2.4 Context Item (Focus)

```csharp
// Evaluate with a context item so `.` and `position()` work
ctx.WithFocus(XdmValue.FromNode(document), position: 1, size: 1);
var result = expr.Evaluate(ctx);
```

---

## 3. XSLT Transforms

### 3.1 Compile a Stylesheet

```csharp
using Bosak.Xslt.Api;

var xsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
    <xsl:template match='/'>
        <output><xsl:value-of select='root/@id'/></output>
    </xsl:template>
</xsl:stylesheet>";

var compiler = new XsltCompiler();
var executable = compiler.Compile(xsl);
```

### 3.2 Transform a Document

```csharp
using Bosak.XPath.Providers.Xml;
using System.Xml.Linq;

var source = new XDocument(new XElement("root", new XAttribute("id", "42")));
var resultXml = executable.TransformToString(new XDocumentNode(source));
// => "<output>42</output>"
```

### 3.3 Named Templates & `call-template`

```csharp
var xsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
    <xsl:template match='/'>
        <result>
            <xsl:call-template name='format-address'>
                <xsl:with-param name='city' select='root/city'/>
            </xsl:call-template>
        </result>
    </xsl:template>

    <xsl:template name='format-address'>
        <xsl:param name='city'/>
        <address><xsl:value-of select='$city'/></address>
    </xsl:template>
</xsl:stylesheet>";

var executable = new XsltCompiler().Compile(xsl);
var result = executable.TransformToString(new XDocumentNode(source));
```

### 3.4 Tunnel Parameters

```csharp
var xsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
    <xsl:template match='/'>
        <xsl:apply-templates>
            <xsl:with-param name='traceId' select='"REQ-123"' tunnel='yes'/>
        </xsl:apply-templates>
    </xsl:template>

    <xsl:template match='item'>
        <!-- $traceId is available here via tunnel -->
        <item trace='{$traceId}'><xsl:value-of select='.'/></item>
    </xsl:template>
</xsl:stylesheet>";
```

### 3.5 `fn:transform()` — XSLT from XPath

```csharp
var callerXsl = @"<xsl:stylesheet version='3.0'
    xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
    xmlns:map='http://www.w3.org/2005/xpath-functions/map'>
    <xsl:template match='/'>
        <result>
            <xsl:copy-of select='transform(map{
                ""stylesheet-location"": ""file:///C:/styles/main.xsl"",
                ""source-node"": .,
                ""stylesheet-params"": map{""greeting"": ""world""}
            })?output'/>
        </result>
    </xsl:template>
</xsl:stylesheet>";
```

---

## 4. XSLT Feature Matrix (Current State)

| Feature | Status | Notes |
|---------|--------|-------|
| `xsl:template match="…"` | ✅ Working | Pattern compiler: element names, `*`, `@*`, predicates, union (`\|`) |
| `xsl:template name="…"` | ✅ Working | Named template dispatch; raw XDM result via `XsltExecutable.Transform(..., rawResult: true)`; whitespace/EQName names normalized; `xsl:initial-template` permitted in XSLT namespace |
| `xsl:call-template` | ✅ Working | With `xsl:with-param` support; matches named templates by expanded QName (different prefixes bound to the same URI); rejects template names in reserved namespaces (`XTSE0080`) except `xsl:initial-template` |
| `xsl:apply-templates` | ✅ Working | Default mode; `select` attribute supported |
| `xsl:value-of` | ✅ Working | |
| `xsl:for-each` | ✅ Working | Position / size context updated per item |
| `xsl:if` / `xsl:choose` | ✅ Working | `when` + `otherwise` |
| `xsl:element` / `xsl:attribute` | ✅ Working | |
| `xsl:text` | ✅ Working | |
| `xsl:copy` | ✅ Working | Shallow copy with `@select` support; focus set correctly per item |
| `xsl:copy-of` | ✅ Working | Deep copy of nodes; Document nodes supported. `copy-namespaces` respected. Static validation rejects disallowed children (`XTSE0260`) and invalid attributes (`XTSE0090`). |
| `xsl:evaluate` | ✅ Working | Dynamic XPath 3.1 evaluation inside XSLT; supports context item, `xsl:with-param` / `@with-params`, in-scope namespaces, base URI, default collation, and `@as` coercion. Stylesheet functions are visible only when not `private`/`hidden`. Java extension functions are not supported. |
| `xsl:comment` | ✅ Working | `select` attribute or text content |
| `fn:copy-of` | ✅ Working | XSLT 3.0 context function |
| `xsl:decimal-format` | ✅ Working | Parsed and registered for `fn:format-number` |
| `xsl:variable` | ✅ Working | Lexical scoping; `as` attribute with full atomic type coercion and atomization (`xs:integer`, `xs:string`, `xs:boolean`, `xs:double`, `xs:decimal`, `xs:duration`, `xs:QName`, `xs:dateTime`, gYear, etc.). Node type tests (`element(...)`, `attribute(...)`, `document-node(...)`, `node()`, `item()`) bypass atomization. Usable in XPath via `$var`. Global variables with sequence constructors are evaluated lazily on first reference with a singleton focus based on the root node of the tree containing the initial context node (XSLT 3.0 §9.6). |
| `xsl:param` | ✅ Working | On named templates, global params, default values; `as` attribute with full atomic type coercion and atomization. Subtype substitution (integer→decimal, float→double) and type promotion supported. |
| Built-in template rules | ✅ Working | Shallow-copy elements, copy text/attributes |
| Literal result elements | ✅ Working | Namespace preservation, AVT evaluation |
| `xsl:import` / `xsl:include` | ✅ Working | URI resolution with correct precedence rules |
| Modes | ✅ Working | Named modes, `#current`, `#default`, `#all`, multi-mode templates |
| `xsl:sort` | ✅ Working | Single and multi-key; `data-type`, `order`, `stable`, AVTs for `lang`/`case-order`/`collation`; recognized collations including UCA `alternate=non-ignorable`, `blanked`, and `shifted`. Default collation is respected; `case-order` works without an explicit collation. |
| `xsl:number` | ✅ Working | `single`, `any`, `multiple` levels; format tokens |
| `xsl:key` / `key()` | ✅ Working | Indexed lookup; composite keys; content-constructor keys preserve typed atomic values; results returned in document order; `key()` allowed in match patterns with XTSE0340 validation. Key-value comparison respects the effective default or explicit `@collation`; conflicting collations for the same key name raise XTSE1220. |
| `xsl:output` | ✅ Working | `method` (`xml`, `html`, `xhtml`, `text`, `json`, `adaptive`), `indent`, `omit-xml-declaration`, `encoding`, `version`, `standalone`, `doctype-system`, `doctype-public`, `cdata-section-elements`, `escape-uri-attributes`, `include-content-type`, `media-type`, `byte-order-mark`, `html-version`, `suppress-indentation`, `normalization-form`, `use-character-maps`, `json-node-output-method`, `allow-duplicate-names`, `escape-solidus`, `item-separator`, `parameter-document`, `undeclare-prefixes`, `build-tree`. Encoding-aware output escapes unrepresentable characters as numeric character references and splits CDATA sections around them. Named `xsl:output` definitions are resolved by import precedence; `xsl:result-document` attributes override the effective output definition. XHTML 1.0 uses the HTML 4 empty-element list; XHTML5 strips the XHTML namespace prefix, serializes HTML5 void elements as empty tags, ignores `doctype-public` when no `doctype-system` is supplied, and preserves root-element case in the DOCTYPE. DOCTYPE literal values are quoted with the delimiter that does not occur in the value. JSON output serializes maps, arrays, booleans, numbers, strings, and nodes; `json-node-output-method` controls node serialization; `escape-solidus` defaults to `yes`; character maps are applied to the final JSON text. Maps/arrays/functions at the top level of a non-JSON output raise `SENR0001`. `xsl:result-document` supports `method="json"`, `method="adaptive"`, and `build-tree="no"` with raw-item collection. `SEPM0009` and `SEPM0010` are enforced for XML/XHTML output properties. |
| `xsl:character-map` | ✅ Working | `name`, `use-character-maps`, and `xsl:output-character/@character` / `@string`; merged in declaration order, later maps override earlier ones (last-wins) for duplicate characters; explicit mappings override referenced maps within a single character map; applied to text, attribute, comment, PI, raw-XML, JSON, and adaptive output in all methods |
| `xsl:function` | ✅ Working | User-defined XPath functions in XSLT; `@as` return type enforced via `ConvertVariableValue` |
| `xsl:sequence` | ✅ Working | Returns sequences from functions |
| `xsl:mode` | ✅ Working | `on-no-match`, `on-multiple-match`, `warning-on-no-match`, `warning-on-multiple-match`, `visibility`, `typed`, `streamable`, `default-mode`, duplicate-declaration checks (`XTSE0545`), and `#unnamed` normalization |
| `xsl:analyze-string` | ✅ Working | Regex matching/non-matching children; `regex-group()`; XSLT 3.0 zero-length match semantics; `@flags` including multiline (`m`) are passed to regex translation |
| Tunnel parameters | ✅ Working | `tunnel="yes"` propagation through `apply-templates` |
| `fn:transform()` | ✅ Working | Full option support: `stylesheet-location`/`stylesheet-node`/`stylesheet-text`/`package-name`(+`package-version` range selection), `source-node`, `global-context-item`, `initial-match-selection` (arbitrary XDM), `initial-template`/`initial-mode`/`default-mode` (xs:QName), `stylesheet-params`/`template-params`/`tunnel-params`/`static-params`, `delivery-format` (`document`/`raw`/`serialized`), `base-output-uri`, `serialization-params`, `xslt-version`. Secondary `xsl:result-document` output is captured into the result map keyed by resolved URI, and absent principal output is suppressed. Available in static expressions (`static="yes"` variables, `xsl:use-when`); function items returned via `delivery-format="raw"` remain callable in the calling stylesheet. Package entry points honor `visibility` (XTDE0040). W3C `fn-transform` Tier-2m 117/124 passed (7 skipped). |
| `xsl:attribute-set` / `use-attribute-sets` | ✅ Working | Accumulates across imports/includes; cycle detection; `xsl:next-match` inside attribute sets works |
| `xsl:use-when` | ✅ Working | Top-level and nested elements evaluated in document order; `true()`/`false()` and static-variable references work; XTSE0090 and XTSE3450 error cases validated. |
| Shadow attributes (`_{attr}` static AVTs) | ✅ Working | `_version`, `_href`, `_use-when`, `_xpath-default-namespace`, `_static`, `_select`, and other underscore-prefixed XSLT attributes are evaluated at compile time in the current static context and replace their non-underscore counterparts. Shadow attributes on literal result elements are preserved as ordinary attributes. |
| `xsl:where-populated` | ✅ Working | Filters empty sequences, empty text nodes, empty PIs, empty comments, and empty elements; attributes and namespace nodes do not make a sequence populated; empty strings and empty arrays are treated as empty |
| `xsl:on-empty` | ✅ Working | Evaluated by parent container (xsl:copy, xsl:document, literal result elements, general sequence constructors) when sequence constructor produces no nodes; supports `@select` and sequence constructor children; `on-empty` conformance cluster 72/72 |
| `xsl:on-non-empty` | ✅ Working | Evaluated by parent container when sequence constructor produces nodes; supports `@select` and sequence constructor children; `on-non-empty` conformance cluster 14/14 |
| `xsl:context-item` | ✅ Working | Declares required/optional/absent context item and type for templates; raises `XTTE0590`/`XTTE3090` at runtime and `XTSE0010`/`XTSE0020`/`XTSE0090` statically. `context-item` conformance cluster 31/31. |
| `xsl:iterate` | ✅ Working | Stateful iteration in result-tree and function-body contexts with `xsl:param`, `xsl:next-iteration`, `xsl:break`, and `xsl:on-completion`. `iterate` conformance cluster 44/44. |
| `xsl:message` | ✅ Working | Evaluates `terminate` and `error-code`; emits serialized message text via `IXsltMessageListener`; terminating messages throw `XsltRuntimeException` carrying the XDM value. The listener also receives `OnWarning` callbacks for XSLT warnings (e.g. no-matching-template / multiple-template warnings). |
| `xsl:try` / `xsl:catch` | ✅ Working | Catches dynamic XPath/XSLT errors in both result-tree and function-body contexts; rolls back output written in the try block before executing a matching catch (unless `rollback-output="no"`). Supports multiple `xsl:catch` clauses evaluated in document order; `@errors` supports `*`, plain local names, `prefix:local` (err namespace), `*:local`, and `Q{uri}local`; binds `$err:code`, `$err:description`, `$err:value`. Static errors in `xsl:variable`/`xsl:param`/`xsl:with-param` `@select` expressions are now reported at stylesheet compile time. |
| `xsl:map` / `xsl:map-entry` | ✅ Working | `xsl:map` evaluates its content as map-entry-producing sequence constructor and merges entries; `xsl:map-entry` builds a single-entry map; duplicate keys raise `XTDE3365`; maps as element/document children raise `XTDE0450` |
| `xsl:result-document` | ✅ Working | Secondary result documents with `format`, `href`, and serialization attributes; principal `xsl:result-document` captured for `TransformToString`; `current-output-uri()` reflects the active result-document URI and is empty outside any result document |

---

## 5. XPath 3.1 Feature Highlights

### Well-covered areas
- Sequence construction, filtering, FLWOR expressions (`for`/`let` chains, `at $pos` positional variables, `where` clauses)
- Standard `fn:*` functions (string, numeric, date/time, QName, URI)
- `map:*` and `array:*` functions
- Higher-order functions (`fn:for-each`, `fn:filter`, `fn:fold-left`, etc.)
- `fn:doc`, `fn:collection` with pluggable document loader
- Decimal formatting (`fn:format-number`)
- JSON functions: `fn:parse-json`, `fn:json-to-xml`, `fn:xml-to-json`, `fn:json-doc`
- Date/time ordering (`lt`, `gt`, `le`, `ge`)
- `fn:analyze-string` — nested group structure and zero-length checks

### Known gaps
- `fn:load-xquery-module` — not implemented
- `fn:serialize` — partial (JSON method supported for maps/arrays/atomics; XML serialization options still limited)
- `fn:transform` — full option support including `delivery-format`, `global-context-item`, `xslt-version`, serialization parameters, and package selection; principal `xsl:use-package` stylesheets remain unsupported
- Schema-aware operations — source documents with `validation="strict"` are now validated in the QT3 harness, and `fn:id`/`fn:element-with-id` use the resulting PSVI; `import schema` and `validate` expressions remain unsupported.
- Regex functions (`fn:matches`, `fn:tokenize`, `fn:replace`) — full XSD regex support: strict syntax validation, character classes/subtraction, backreferences (incl. unclosed-group FORX0002), flags, code-point `.`, and pinned Unicode 9.0 category/block data (`\p{X}`, `\p{IsBlock}`). Remaining gap: the `i` flag uses .NET case-insensitivity rather than Unicode full case folding (affects patterns mixing `i` with `\p{...}` or negated classes)

---

## 6. XSD Validation

Bosak provides an `IXsdValidator` abstraction for XML Schema validation:

```csharp
using Bosak.XPath.Api.Xsd;

var validator = new XsdValidator();
var result = validator.TryValidate(xmlString, xsdStream);

if (result.IsValid)
{
    Console.WriteLine("Document is valid");
}
else
{
    foreach (var error in result.OnlyErrors)
    {
        Console.WriteLine($"Error at line {error.LineNumber}: {error.Message}");
    }
}
```

Features:
- Single-schema and multi-schema validation (handles `xs:import`/`xs:include`)
- Structured error results with line/column numbers
- Non-throwing `TryValidate` and throwing `Validate` variants
- Configurable via `XsdValidatorOptions` (max error count, treat warnings as errors)

---

## 7. Current Build State

Run the full suite from the Bosak repo root:

```bash
dotnet build Bosak.sln
dotnet test Bosak.sln
```

**Unit tests:** 913 passed, 0 failed, 0 skipped  
**Target framework:** `net10.0`

### Behavioral Changes

| Change | Impact | When |
|--------|--------|------|
| `XsltExecutable.Transform` gained an optional `rawResult` parameter. | When `true` and an initial named template is used, returns the raw template result as an XDM value instead of wrapping it in a result document. Required for `initial-template-004` and similar raw-output tests. | 2026-06-25 |
| `TransformEngine.IsNodeAttached` now treats a document's root element as attached. | After whitespace stripping, the initial source node (`/doc`) was incorrectly considered detached because the root `XElement` has no `XObject.Parent`. The check now also verifies `XObject.Document != null`. Fixes `mode-1105`. | 2026-06-26 |
| Regex `.` now matches Unicode code points including surrogate pairs. | `RegexHelper.TranslateDot` replaces `.` with an alternation that prefers a high+low surrogate pair over a single code unit. Fixes `regex-026` and aligns `fn:matches`/`replace`/`tokenize` with XPath/XSD semantics. | 2026-06-26 |
| `xsl:evaluate` blocks `fn:system-property`. | XSLT-defined functions are removed from the dynamic context; calling `system-property` inside `xsl:evaluate` now raises `XTDE3160`. Fixes `system-property-022`. | 2026-06-26 |
| `xsl:catch` now matches bare error codes. | `GetErrorCode` recognizes 8-character codes such as `FOUT1190` even without a trailing colon, so `xsl:catch errors="*:FOUT1190"` matches. Fixes `unparsed-text-lines-004`. | 2026-06-26 |
| Root-level literal result elements copy in-scope stylesheet namespaces. | `CopyLiteralElement` copies namespace declarations from the stylesheet root onto the output root element (except excluded prefixes and the XSLT namespace). Fixes `attribute-0601`. | 2026-06-26 |
| `xsl:analyze-string` now passes regex flags to `RegexHelper.ValidateAndTranslatePattern`. | Fixes multiline-mode (`m`) tests `analyze-string-007/067/071/090b`; previously `$` was translated to `\z` even when multiline was requested. | 2026-06-25 |
| `xsl:call-template` now resolves named templates by expanded QName. | A call using one prefix bound to a namespace URI finds a template declared with a different prefix bound to the same URI. Fixes `call-template-1701`. | 2026-06-25 |
| Initial template names from the conformance harness are expanded using catalog namespace bindings. | Names are passed to `TransformEngine` in Clark notation, so a test-catalog prefix bound to a different URI than the stylesheet prefix correctly raises `XTDE0040`. Fixes `call-template-0104/0105/0107`. | 2026-06-25 |
| `xsl:template/@name` values are whitespace-trimmed and validated against reserved namespaces. | Leading/trailing spaces and EQName forms such as ` Q{}temp ` are normalized; names in the XSD, XPath-functions, or XSLT namespaces raise `XTSE0080` (except the special `xsl:initial-template` name). Fixes `call-template-0106/0109`. | 2026-06-25 |
| `xpath-default-namespace` fully wired through XSLT → XPath pipeline. | `CompileOptions.DefaultElementNamespace` controls unprefixed element/type names in XPath expressions. Threaded through `CompileXPath`, `PatternCompiler`, `TemplateRule.ResolveNamespacePrefixes`, `VmEngine.NamespaceTest`, and whitespace stripping (`SpaceHandlingRule`). Fixes xpath-default-namespace-0101 through 1102 (21/22 passing). | 2026-06-11 |
| `xsl:attribute` with unprefixed name now uses empty namespace URI. | Previously inherited default namespace from parent; now correctly produces no-namespace attributes per XSLT spec. Fixes namespace-3306. | 2026-06-11 |
| `xsl:call-template` evaluates default `xsl:param` values when no `with-param` is provided. | Previously omitted parameters fell back to empty sequence instead of evaluating the param's `select` or sequence constructor. Fixes namespace-3501/3503. | 2026-06-11 |
| `AddElementToContainer` injects `xmlns=""` when no-namespace element is placed inside a default-namespace parent. | Prevents LINQ-to-XML from silently inheriting parent's default namespace. Fixes namespace-0913. | 2026-06-11 |
| `fn:node-name` on text nodes returns `XdmValue.Undefined` (empty sequence). | Was incorrectly returning empty sequence due to unintended `NodeToQName` change. Reverted to spec-compliant `Undefined`. | 2026-06-11 |
| `CopyLiteralElement` no longer walks ancestor chain to copy namespace declarations. | Was leaking `xmlns:xs` and other stylesheet prefixes into literal result elements, breaking `exclude-result-prefixes` and `fn:transform` output. | 2026-06-11 |
| `*:local` name tests now emit `"*:local"` into the literal pool. | Prevents VM from applying the no-namespace attribute restriction to `*:local` patterns. Fixes namespace-1402 and related tests. | 2026-06-11 |
| Global sequence-constructor variables now evaluate with the initial context item. | `xsl:variable` sequence constructors at the top level use a singleton focus based on the root of the source tree (XSLT 3.0 §9.6), not the focus at the point of reference. Fixes `string-041`. | 2026-06-12 |
| Named-template entry points without a source document use an absent context item. | `XsltExecutable.Transform`/`TransformToString` accept a null source; the conformance harness passes null for named-template tests with no explicit source. Keeps `copy-4308` (XTTE0945) correct. | 2026-06-12 |
| Namespace node `parent::node()` now returns the element whose namespace axis includes the node (`_namespaceOwner`), not the element where the underlying `XAttribute` declaration resides. | Fixes `.. is $e` for inherited namespace nodes in XPath. Required for XSLT `namespace::*` axis correctness. | 2026-06-10 |
| `xsl:variable`/`xsl:param`/`xsl:function`/`xsl:with-param` `@as` now fully supports atomic type coercion and atomization. | `ConvertVariableValue` rewrites atomization + casting via `VmEngine.TryCast`. Subtype substitution (integer→decimal, float→double) and type promotion. Node type tests (`element(...)`, `attribute(...)`, `document-node(...)`) bypass atomization. | 2026-06-11 |
| `xsl:document` no longer leaks outer `_sequenceAccumulator` into its sequence constructor. | `wrapInDocumentNode=true` now isolates the accumulator, ensuring `xsl:copy-of` inside `xsl:document` unwinds document nodes into the new document instead of the outer variable. | 2026-06-11 |
| `xsl:call-template/@as` now raises `XTSE0010` at runtime. | `@as` is not permitted on `xsl:call-template`; previously ignored. | 2026-06-11 |
| `xsl:copy` now raises `XTTE0945` (no context item), `XTTE3180` (select returns >1 item), `XTDE0410` (attribute after children), and `XTDE0420` (attribute on non-element) per XSLT 3.0 spec. | Previously these error conditions were silently ignored or produced wrong results. | 2026-06-10 |
| XSLT functions (`xsl:function`) no longer leak the first argument as the context item. | Functions now correctly have no context item per XSLT 3.0 §9.6. Fixes `xsl:copy` inside functions. | 2026-06-10 |
| `xsl:where-populated` now correctly filters empty PIs, comments, and text nodes. | Previously only whitespace-only text nodes were filtered; empty PIs/comments passed through incorrectly. | 2026-06-10 |
| XPath parser no longer treats prefixed names as kind tests (e.g. `my:node()`). | `my:node()` was parsed as `child::node()` instead of a function call. Affected any prefixed name where local name matched a kind test. | 2026-06-10 |
| `xsl:key` content constructors now preserve typed atomic key values. | `string-length(.)`, `string-to-codepoints(.)`, and other atomic producers are stored as typed values rather than converted to text nodes. Fixes `key-082`, `key-073/074/075`. | 2026-06-12 |
| `key()` lookup results are returned in document order. | Multiple `xsl:key` definitions with the same name no longer return nodes in definition order. Fixes `key-073/074/075` ordering. | 2026-06-12 |
| Pattern predicates in `key()` match patterns now isolate caller focus. | `PatternCompiler.WrapWithCurrentItem` saves/restores context item, position, and size so `xsl:number` with `key()` patterns does not corrupt subsequent instructions. Fixes `key-035`. | 2026-06-12 |
| `key()` pattern validation restored. | XTSE0340 is raised for invalid second arguments in `key()` match patterns; numeric literals, variable references, and parenthesized sequences are allowed. Fixes `key-083`, `key-093`, `key-097`, `match-079`, `match-080`. | 2026-06-12 |
| `xsl:where-populated` now implements populated-node semantics per XSLT 3.0. | Document nodes from `xsl:document` and items from `xsl:sequence` are preserved; empty elements/documents are filtered; `xsl:on-empty` children are honoured. Fixes `element-0104` through `element-0108`. | 2026-06-11 |
| Fragment document nodes now serialize correctly. | Multi-root `xsl:document` results are wrapped in `__xdm_doc__` during copying and unwrapped by `ResultTreeSerializer`. Fixes `xsl-document-0501`. | 2026-06-11 |
| `xsl:document` inside simple content contributes the document's string value. | Excludes comment/PI descendants; fixes `xsl-document-0601`. | 2026-06-11 |
| `XDocumentNode.StringValue` for synthetic-wrapper documents includes all descendant text. | Previously only direct text children of the wrapper were included. | 2026-06-11 |
| `xsl:message` now includes both `@select` and sequence-constructor content. | Both contributions are concatenated, matching XSLT 3.0 semantics. Fixes `xsl-document-0603`. | 2026-06-11 |
| Conformance harness supports `<assert-message>` and fragment assertions. | Messages are captured via `RecordingMessageListener`; multiple direct assertion children are treated as an implicit `<all-of>`. | 2026-06-11 |
| `xsl:copy` shallow copy no longer copies source attributes/children. | Source attributes and children must now be produced by the contained sequence constructor, matching the XSLT spec. Fixes `attribute-set-0107`. | 2026-06-13 |
| XPath parser resolves the `xml` prefix to the XML namespace in node tests. | Previously `@xml:*` fell back to a prefix-only match that matched attributes in any namespace. Fixes `attribute-0901`. | 2026-06-13 |
| `fn:document#1/#2` supports URI fragment identifiers. | Fragment identifiers resolve to the element with matching `id`/`xml:id`; relative document URIs resolve from the stylesheet base URI. Fixes `id-001`. | 2026-06-13 |
| `xsl:evaluate` is fully supported. | Dynamic XPath 3.1 evaluation inside XSLT with context item, parameters, namespaces, base URI, default collation, and `@as` coercion. Fixes the entire `evaluate` cluster. | 2026-06-13 |
| Cross-tree document order now follows document creation order. | `XDocumentNode.DocumentOrder` combines a global creation sequence (high bits) with the per-document local index (low bits), so union/path results across separately constructed temporary trees are stable. Fixes `evaluate-002`. | 2026-06-13 |
| `xsl:function` validation and static errors are implemented. | `Stylesheet.ValidateInstructionTree` reports XTSE0020/XTSE0080/XTSE0770/XTSE0090/XTSE0740 for invalid function declarations, reserved namespaces, duplicate signatures, and `Q{}local` names. Fixes function cluster validation tests. | 2026-06-13 |
| `xsl:function` supports deterministic memoization. | `new-each-time="no"` results are cached per (name, arity, argument) key; AVTs on `_new-each-time` select deterministic/non-deterministic mode at run time. Fixes `function-0240` and related tests. | 2026-06-13 |
| `fn:function-available` is fully spec compliant. | Parses EQNames, atomizes/casts the arity argument, reports `fn:concat` for any variadic arity, and reports the full XSLT 3.0 function set. Fixes `function-available` cluster. | 2026-06-13 |
| `fn:element-available` is implemented. | Reports availability for XSLT 2.0/3.0 instructions in the XSLT namespace. The unprefixed first argument is expanded using the XML default namespace of the element containing the expression, tracked separately from the XPath default namespace via `CompileOptions.DefiningElementDefaultNamespace`. Fixes `function-0302b`. | 2026-06-24 |
| `fn:available-environment-variables` and `fn:environment-variable` are implemented. | Returns/succeeds on process environment variables; matching is case-sensitive exact. Fixes function cluster environment tests. | 2026-06-13 |
| Numeric arguments to `fn:subsequence` and `fn:format-integer` are atomized. | Atomization now preserves `xs:untypedAtomic`, so attribute and element text nodes are accepted implicitly. Fixes `function-0502`, `function-0503`, and related tests. | 2026-06-13 |
| Namespace context is applied to `xsl:variable`/`xsl:param`/`xsl:with-param` @select expressions. | Local and global variable/param `select` expressions, and named-template default param values, now use the in-scope namespace bindings and effective default namespace. Fixes unprefixed EQName tests in `function` cluster. | 2026-06-13 |
| `date` cluster is fully passing. | Implicit timezone, `xs:time` midnight semantics, timezone adjustment, AM/PM formatting, extended-year constructor bounds, and static-parameter substitution in the harness. Fixes all runnable `date` tests. | 2026-06-13 |
| `xsl:message` now implements terminate/error-code semantics and serializes node content. | Messages evaluate `@terminate` and `@error-code`, emit via `IMessageListener`, and throw `XsltRuntimeException` with the captured XDM value when terminating; `xsl:try`/`xsl:catch` binds `$err:code`, `$err:description`, `$err:value`. Fixes the `message` conformance cluster (45/0/0). | 2026-06-13 |
| `fn:unparsed-text` resolves relative `href` against `EvaluationContext.BaseUri`. | Previously resolved only against the static base URI parameter; now uses the dynamic base URI when no explicit base is supplied. Required for `message-0313`. | 2026-06-13 |
| `fn:element-available` uses the defining element's default namespace. | Added `EvaluationContext.DefiningElementDefaultNamespace` and `CompileOptions.DefiningElementDefaultNamespace` so that XSLT's `element-available()` expands unprefixed QNames using `xmlns="..."` rather than `xpath-default-namespace`. | 2026-06-24 |
| `validation="lax"` is accepted on basic processors. | Non-schema-aware processors no longer raise `XTSE1660` for `lax` (or `default-validation="lax"`), matching XSLT 3.0 semantics. Fixes `validation-0102b`. | 2026-06-24 |
| `fn:doc('')` resolves against the static base URI. | In XSLT, `fn:doc('')` now loads the stylesheet module; in pure XPath it still yields the empty sequence when no base URI is present. Fixes `document-0302`. | 2026-06-24 |
| `fn:doc` atomizes and validates its argument. | Empty sequence returns empty sequence; more than one item raises `XPTY0004`; prevents literal `\(sequence\)` from being loaded as a URI. Fixes `document-0303/0307/0601/0901/1101`. | 2026-06-24 |
| Stylesheet module base URIs are preserved. | `FileSystemUriResolver`, `XsltCompiler`, and the conformance `TestUriResolver` load stylesheet modules with `LoadOptions.SetBaseUri`, so `fn:doc('')` and `fn:document()` inside included/imported modules resolve relative URIs against the correct module base URI. Fixes `document-1003/1004/1901`. | 2026-06-24 |
| `fn:doc`/`fn:document` loaded documents are subject to `xsl:strip-space`/`xsl:preserve-space`. | `EvaluationContext.DocumentPostProcessor` lets XSLT apply the stylesheet's whitespace-handling rules to documents loaded during transformation, while protecting stylesheet modules themselves from mutation. Fixes `document-0308`. | 2026-06-24 |
| `xsl:use-package` tests are skipped by the conformance harness. | The compiler does not support XSLT 3.0 packages; the harness now detects `xsl:use-package` in the principal stylesheet and reports a skip instead of a null-reference failure. Moves `document-2402` to skipped. | 2026-06-24 |
| `fn:unparsed-text-lines` is fully spec compliant. | Trailing line terminators no longer produce an empty final line; decoded text is validated for XML-legal characters, raising `FOUT1190` for invalid characters such as NUL. Fixes `unparsed-text-lines-002/004`. | 2026-06-24 |
| `xsl:try` / `xsl:catch` handles multiple catch clauses and error-code matching. | `TransformEngine` evaluates all `xsl:catch` children in order, matches `@errors` against `*`, plain names, `*:local`, `Q{uri}local`, and `prefix:local` (err namespace), and rethrows unmatched errors. Fixes `call-template-0110`. | 2026-06-25 |
| `fn:function-available` validates its argument. | Invalid QName/EQName syntax or an unbound prefix now raises `XTDE1400`; the error is propagated through `use-when` expressions. Fixes `extension-functions-0103/0104`. | 2026-06-24 |
| `extension-element-prefixes` bound to reserved namespaces is rejected. | The stylesheet loader reports `XTSE0800` when the XSLT, XML, XML Schema, or XML Schema instance namespace is declared as an extension namespace. Fixes `extension-functions-0105`. | 2026-06-24 |
| `use-when` namespace context includes all ancestors. | Prefixes declared on any ancestor of the element carrying `use-when` are now in scope for the expression. Fixes `extension-functions-0101`. | 2026-06-24 |
| `fn:snapshot` is implemented. | Creates a copy of a node with shallow ancestor copies (attributes/namespaces preserved) and deep-copied descendants; in-scope namespace bindings are now copied to every element in the snapshot, matching `xsl:copy-of validation="preserve"`. Top-level `xsl:namespace` instructions only produce standalone namespace-node items when the containing sequence constructor is typed as `namespace-node()`. Clears the `snapshot` cluster. | 2026-06-28 |
| `fn:innermost` / `fn:outermost` relationship checks are correct. | Both functions now use `IsSameNode` instead of reference equality, and `innermost`/`outermost` apply the correct descendant/ancestor filtering semantics. Fixes `innermost-001/901`. | 2026-06-24 |
| Conformance harness supports raw XDM comparison for `<initial-function>`. | Tests with `<output tree="no" serialize="no"/>` now compare the raw function result using `assert-type`, `assert-count`, `assert-deep-eq`, and `assert-eq` instead of serializing to a string. Fixes `initial-function-002` and `initial-function-100a..100i`. | 2026-06-24 |
| `VmEngine.ValueMatchesType` respects sequence occurrence indicators. | Top-level sequence values are now matched against `?`, `*`, and `+` occurrence indicators by checking each item against the base type. Fixes `initial-function-100e` (`xs:string*`). | 2026-06-24 |
| `xsl:function/@_name` AVTs are expanded to expanded QNames at parse time. | `XsltFunctionDefinition.FromElement` evaluates `_name` attribute value templates (including `xs:QName`-returning expressions) in the static context, so functions declared with dynamic names are registered under the correct expanded QName. Fixes `initial-function-101c..101e`. | 2026-06-24 |
| XPath value comparison casts `xs:untypedAtomic` to `xs:string`. | In value comparisons (`eq`/`ne`/`lt`/`le`/`gt`/`ge`), an `xs:untypedAtomic` operand is atomized to `xs:string` before comparison, so `xs:untypedAtomic('72') gt 70` raises `XPTY0004` while `xs:untypedAtomic('') eq ''` succeeds. General comparisons continue to promote `xs:untypedAtomic` to the other operand's type. Fixes `type-0165`. | 2026-06-25 |
| Whitespace stripping applies to the source document root, and stripped source nodes are treated as absent. | The engine strips whitespace from the document containing the initial context node, detects when the selected node has been removed, and evaluates globals with focus on the source-tree root. Fixes `strip-space-023`. | 2026-06-25 |
| Path expressions only load the context item when the first step is an axis step. | Prevents `parse-xml(...)/root/item` from raising `XPDY0002` when the XPath focus is absent. Required by the `strip-space` fix. | 2026-06-25 |
| `XsltCompiler.StaticParameters` supplies values for static `xsl:param` declarations. | Caller-supplied values override stylesheet `select` defaults and are coerced against `@as` during `BuildStaticContext()`. Required for parameterized static tests such as `static-003a/013c`. | 2026-06-26 |
| Static variables and parameters are eagerly bound at runtime. | `InitializeGlobalParametersAndVariables` binds values from `Stylesheet.StaticVariables` before lazy non-static globals, so static values remain visible even when a non-static declaration shadows the name. Fixes `static-027`. | 2026-06-26 |
| XTSE0090 and XTSE3450 validations for static declarations are implemented. | `static="yes"` is rejected on non-global `xsl:variable`/`xsl:param`; `visibility` is rejected on static declarations; a static variable and static parameter with the same expanded name raise `XTSE3450`. Fixes `static-020/023/025/026`. | 2026-06-26 |
| Static declarations without a value default to empty sequence (or undefined for required parameters). | Optional static variables/parameters default to `()`; required static parameters without a supplied value raise `XTDE0050`. Fixes `static-010` and related cases. | 2026-06-26 |
| General comparison with an empty operand returns `false`. | `VmEngine.CompareGeneral` now follows XPath 3.1 §17.3: one empty operand yields `false`, not an empty sequence. Fixes `static-011`. | 2026-06-26 |
| Namespace axis includes implied default namespaces. | `XDocumentNode.GetNamespaceAxis` adds a default-namespace node when the element is in a non-empty namespace that is not declared explicitly as default or prefixed. Fixes `static-030` and `json-to-xml` namespace-axis coverage. | 2026-06-26 |
| Static conformance cluster is fully passing. | All 49 `static` tests pass (was 47/49). Combined with the two cross-cutting fixes, the full W3C suite improves to 4,599/652/9,349. | 2026-06-26 |
| `xsl:use-attribute-sets` is allowed on literal result elements. | Added `use-attribute-sets` to the XTSE0805 whitelist of XSLT-namespaced attributes permitted on LREs. Clears the `attribute-set`, `xsl-document`, `analyze-string`, and `next-match` clusters. | 2026-06-26 |
| Precedence-aware XTSE3450 detection for static variables. | `Stylesheet.BuildStaticContext` evaluates top-level `use-when` in document order and tracks import precedence; same-precedence conflicting values and higher-precedence overrides that change the effective value raise `XTSE3450`. Fixes `use-when-0137/0138` and keeps `static` cluster at 49/49. | 2026-06-26 |
| `use-when` conformance cluster is fully passing. | All 99 runnable `use-when` tests pass (was 97/99); `use-when-0137/0138` now raise `XTSE3450` correctly. | 2026-06-26 |
| Shadow attributes (static AVTs) are implemented. | `_version`, `_href`, `_use-when`, `_xpath-default-namespace`, `_static`, `_select`, and other underscore-prefixed XSLT attributes are expanded at compile time using the current static context; shadow attributes on literal result elements are left untouched. Clears the `shadow` cluster. | 2026-06-26 |
| XSLT 1.0 backwards-compatible mode is fully implemented. | `CompileOptions.BackwardsCompatible` flows into the XPath optimizer, IR lowerer, VM arithmetic/comparisons, standard-function argument conversion, `xsl:value-of`, `xsl:number`, and `key()` string-valued lookups. Clears the `backwards` cluster (43/43 runnable). | 2026-07-07 |
| The `bug` conformance cluster is fully passing. | Imported-template XTSE0680 validation, `<assert-serialization>` file loading in the harness, namespace fixup for copied attributes, and `current()` inside `xsl:sort`. Clears the `bug` cluster (69/69 runnable). | 2026-07-07 |
| The `xpath-compat` conformance cluster is fully passing. | Backwards-compatible negative-zero constant folding and `fn:subsequence` numeric argument coercion for strings/untyped atoms. Clears the `xpath-compat` cluster (17/17 runnable). | 2026-07-07 |
| Resource URIs can be redirected to local files. | `EvaluationContext.ResourceUriMapper` (`Func<string, string?>`) maps a requested URI to a local path; consulted by `fn:doc`, `fn:json-doc`, `fn:unparsed-text(-available/-lines)`, and `fn:transform`'s `stylesheet-location` before filesystem/network access. `XDocumentProvider.LoadXml` now absolutizes relative paths before deriving the document URI (previously `UriFormatException`). JSON parse failures in `fn:parse-json`/`fn:json-doc`/`fn:json-to-xml` raise `FOJS0001` instead of propagating `JsonException`. | 2026-07-15 |
| QT3 `fn:transform` Tier-2m is fully passing. | Implemented `global-context-item`, `xslt-version` validation/propagation, default-mode routing, `template-params`/`tunnel-params`, `base-output-uri` raw-result delivery, serialization parameter merging, `suppress-indentation` override, and absent-principal-output suppression. Filtered suite: 117 passed / 0 failed / 7 skipped. | 2026-07-15 |

### Conformance Baselines

| Suite | Passed | Failed | Skipped | Pass Rate | Notes |
|-------|--------|--------|---------|-----------|-------|
| XSLT 3.0 (W3C) | 5,506 | 99 | 8,995 | 98.2% | `output` cluster 179/24/29; `result-document` cluster 104/21/29; remaining failures are pre-existing non-output issues |
| XPath 3.1 (QT3) | 21,218 | 1,317 | 9,286 | 66.68% | `?`/`?*` lookup operator spec-complete (UnaryLookup, FOAY0001/XPTY0004); suite http: resources mapped to local files |

> **Note:** The conformance runner locks DLLs. If you get build errors about locked files, run:
> ```bash
> taskkill /F /IM Bosak.XPath.Conformance.exe
> taskkill /F /IM Bosak.Xslt.Conformance.exe
> ```

---

## 8. VS Code Extension

Bosak ships with a VS Code extension (`vscode-bosak/`) that provides syntax highlighting, realtime diagnostics, and auto-completion via a Language Server Protocol (LSP) server.

### 8.1 Building & Running

```bash
# Build the language server (.NET 10)
dotnet build src/Bosak.LanguageServer/Bosak.LanguageServer.csproj

# Build the extension client (Node.js 18+)
cd vscode-bosak
npm install
npm run compile

# Launch Extension Development Host
code . --goto src/extension.ts
# Then press F5 inside VS Code
```

### 8.2 Packaging as VSIX

```bash
cd vscode-bosak
npx vsce package
# Produces: vscode-bosak-0.1.0.vsix
```

Install in VS Code: **Extensions** → **⋯** → **Install from VSIX…**

### 8.3 Extension Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `bosak.server.path` | `string \| null` | `null` | Absolute path to `Bosak.LanguageServer` binary. When null, the extension searches the workspace. |
| `bosak.trace.server` | `string` | `"off"` | LSP traffic tracing: `"off"`, `"messages"`, `"verbose"`. |

### 8.4 Supported File Types

| Language | Extensions | Features |
|----------|------------|----------|
| XPath | `.xpath` | Syntax highlight, diagnostics, completions (functions, axes, keywords) |
| XSLT | `.xsl`, `.xslt` | Syntax highlight, diagnostics, completions (XSLT instructions + XPath) |

---

## 9. Getting Help / Reporting Issues

- Check `docs/ARCHITECTURE.md` in the Bosak repo for the layer overview and execution pipeline.
- Check `docs/FEATURE_REQUESTS.md` for the feature request registry.
- XPath failures: capture the expression, input XML, and expected vs. actual result.
- XSLT failures: capture the stylesheet fragment, source XML, and expected output.
