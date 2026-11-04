# Bounded exact arithmetic sessions

Source 1.48.0 adds `bosak-exact-arithmetic-v1` through the existing `SchemaQuerySession.CompileXPath` API. It supports a finite source-free XPath 3.1 subset: integer/decimal literals, unprefixed primitive numeric variables, parentheses, unary signs, addition, subtraction and multiplication. Publication and API freeze remain pending.

```csharp
using System.Globalization;
using System.Numerics;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Session = Bosak.XPath.Api.Schema.SchemaQuerySession;

var limits = new SchemaQueryLimits(4, 8, 8, maxNumericDigits: 64, maxDecimalScale: 32);
var compiled = Session.CompileXPath("$amount * 1.20 + 0.03", limits);
if (compiled.Kind != SchemaResultKind.Supported)
    throw new NotSupportedException(compiled.DiagnosticCode);
using var session = compiled.Value;
var context = new EvaluationContext().WithVariable("amount", XdmValue.FromInteger(
    BigInteger.Parse("123456789012345678901234567890", CultureInfo.InvariantCulture)));
var result = session.Run([], operation => operation.Evaluate(context));
Console.WriteLine(result.ExactDecimalValue.ToInvariantString(maxCharacters: 100));
```

Queries already supported by the ordinary string/boolean profile keep that profile. Numeric syntax selects the arithmetic profile; inspect `Environment.BehaviorProfile`. A bare `$value` still selects the previous ordinary profile and refuses numeric bindings. Use `+$value` or `$value + 0` to request numeric identity. No public signature or caller option is added.

## Numeric capacity

Select explicit positive `MaxNumericDigits` and nonnegative `MaxDecimalScale`. Every supplied literal digit, including leading/trailing zeros, and fractional place counts before coefficient construction/normalization. For bindings, all supplied variables are checked, including unused ones. Raw CLR decimal coefficient/scale count before normalization; an already-owned exact holder supplies its current normalized coefficient/scale. The engine cannot reconstruct or bound the earlier creation work or lexical scale of that holder.

Inputs must be Integer/Decimal carriers with no private marker or user-defined type annotation. Only absent/base integer/decimal schema names are supported; derived schema numeric semantics are pending. Floats, doubles, strings, booleans, node variables and structured values refuse. Borrowed focus, when supplied, still needs an admitted node owner; numeric evaluation does not atomize or traverse it.

Operands, aligned coefficients and every intermediate result must fit the selected coefficient capacity. Multiplication's supplied scale sum must fit before normalization. Thus `9.9 + 0.1` at two digits refuses because the intermediate coefficient is 100, even though normalized 10 would fit; `2.5 * 4` has the same policy. `9.9 - 9.9` fits because cancellation itself stays within the operand width. Alignment can refuse a nonzero operand before a large power of ten is allocated. Subsequent cancellation cannot erase earlier intermediate work requirements.

Capacity thresholds are bounded metadata derived from the selected digit limit and can use one extra threshold digit. These are coefficient/scale limits, not a hard bound on all temporary bytes, expression length, total evaluation time or stack-plan memory. The compiler/evaluator are iterative, and the evaluation stack is pooled and cleared on return. Numeric input allocation performed earlier by a caller is outside operation admission.

## Profile and errors

No legacy numeric parser/VM fallback occurs. Literals are checked lexically before arbitrary-precision construction; the arithmetic path computes exact integer/decimal values and preserves decimal promotion. Numeric static descriptors identify the profile and captured limits. Namespace options are copied as the actual compiler's explicit static context; this profile supports only unprefixed variable references. It refuses JIT/strict-static-typing modes, schema sets/callback services and the other unsupported capture options.

Compile-time literal capacity, unsupported syntax and unsupported options return Unsupported. Dynamic input/intermediate capacity throws `NotSupportedException`. A missing binding uses the existing deferred XPST0008 convention. Invalid source may still raise the existing syntax errors. Division/idiv/mod, exponent/floating-point literals, comparisons, functions/constructors, mixed string/boolean arithmetic, sequences, maps/arrays, prefixed/EQName variables, node atomization and schema operations remain unavailable. This is not a full numeric or XSD validation profile.

Raw operation availability/cancellation is checked between instructions and around arithmetic. Existing admission, rollback, close and drain contracts remain: canceling a wait does not end raw work, and owner leases survive until raw completion. A BigInteger library operation or lexical scan is not forcibly preemptible. The public arithmetic tests exercise cancellation and close during actual dispatch without touching borrowed root getters.

## Read the result

Results are plain Integer/Decimal carriers. Use `BigIntegerValue` for the full integer, or `ExactDecimalValue.ToInvariantString` with an output bound. Legacy `IntegerValue`, `DecimalValue` and `XdmValue.ToString` still enforce their earlier CLR capacity and may throw on a valid wide result. Numeric map keys/comparisons/serialization in other legacy APIs are not automatically migrated. The earlier `FromDecimal(default)` typed-default source migration remains applicable.

Exact comparisons/map keys and bounded serialization, then wider operators/schema/all-host/native qualification remain pending. No workload budget, C45/freeze or publication is accepted by this slice.
