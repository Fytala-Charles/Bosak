# Exact comparisons, keys and strings

The opt-in session compiler selects `bosak-exact-scalars-v2` for singleton integer/decimal comparisons and top-level scalar string output. Arithmetic-only plans retain `bosak-exact-arithmetic-v1`; ordinary queries retain their existing profile.

Use the existing `SchemaQuerySession.CompileXPath` overload with explicit `SchemaQueryLimits`. Expressions include `$x lt 1.0`, `1 + 2 eq 3`, `string(+$x)` and `string(1 eq 2)`. Bind primitive integer/decimal values through the existing variable mechanism. Results are Boolean or String for these examples. Exact integer/decimal carriers retain their full coefficients.

Supported comparisons are `eq`, `ne`, `lt`, `le`, `gt`, `ge` and singleton `=`, `!=`, `<`, `<=`, `>` and `>=`. Arithmetic binds first. Chained comparisons, Boolean operands, Boolean composition, sequences, floating inputs, division and arbitrary functions remain unsupported. Ordinary `string($x)` still selects the ordinary profile: use explicit numeric arithmetic such as `string(+$x)` for exact numeric conversion. Only unprefixed `string` and correctly bound `fn:string` select this wrapper; an explicit nonstandard `fn` binding is refused.

Coefficient and scale limits apply to inputs and intermediate numbers. Comparison first uses signs and decimal orders, aligning coefficients only when necessary within the admitted width. Output capacity is derived as `min(int.MaxValue, max(5, MaxNumericDigits + 2, MaxDecimalScale + 3))`; canonical formatting refuses excess output before padding. Availability checks run during scalar dispatch and formatting; session close/cancellation retains acquired-resource cleanup obligations. These limits do not establish a total CPU or memory budget.

`XdmMap` now compares and hashes full numeric keys through normalized coefficient/scale pairs, including exact finite IEEE float/double values, infinities, NaNs and signed zeros. Decimal `1.1` and binary double `1.1` are different exact keys; their genuinely equal cross-type values share a hash. Wide coefficients and extreme scale gaps do not require padded decimal strings. Existing key replacement/annotation behavior is retained. Global maps have no query quota: work depends on owned coefficient size and intrinsic IEEE range. Map construction syntax is outside this session profile. Never persist hash codes.

Legacy general XPath/XQuery/XSLT numeric routes and serializers remain separately bounded CLR routes. The earlier `FromDecimal(default)` overload ambiguity still requires explicit `default(decimal)`; source migration and final release SemVer remain open. This slice establishes neither full XSD support nor native adapter acceptance or API freeze.

See [qualification](README.md) and [semantic adoption](SPEC-BASIS.md).
