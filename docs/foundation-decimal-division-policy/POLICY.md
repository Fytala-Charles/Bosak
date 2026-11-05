# Selected finite division policy

The next opt-in profile will return a Decimal for every successful primitive integer/decimal `div`, including two Integer operands and mathematically integral results. It preserves exact mathematical value, canonical carrier normalization and existing supplied-value admission. It accepts neither floating query inputs nor typed/schema numeric operands. Earlier arithmetic/scalar/quotient profiles retain their descriptors and selection.

| Situation | Selected behavior |
|---|---|
| `1 div 8` | Exact Decimal coefficient 125, scale 3. |
| `10 div 4` | Exact Decimal coefficient 25, scale 1. |
| `6 div 3` | Exact Decimal coefficient 2, scale 0. |
| Nonterminating `1 div 3` | Dynamic `NotSupportedException` for the finite profile; no rounded value. |
| Zero divisor, including zero divided by zero | After supplied-value admission, `DivideByZeroException` carrying FOAR0001, as the quotient profile does. |
| Supplied coefficient/scale or required result exceeds capacity | Explicit unsupported capacity; no CLR narrowing or rounding. |
| Huge scale difference | Integer exponent arithmetic and early bounds; no scale-expanded string or power of ten. |

These are Core's selected implementation rules for a partial exact lane. In particular, nontermination refusal is an intentional unsupported subset, not the standard's required decimal-division answer. General XPath decimal precision/rounding and standardized error-graph integration remain open before wider host qualification; the scoped R2 design closure is not expanded by this decision. Existing legacy division is unchanged. No owner sign-off or rounding-mode signature need is inferred.

For admitted inputs `a * 10^-sa` and `b * 10^-sb`, first check zero and cancel the coefficient GCD. The reduced denominator must contain only factors 2 and 5. If it is `2^p * 5^q`, set `k=max(p,q)`. Removing numerator factors 2 and 5 gives `n * 2^u * 5^v`. The tentative coefficient is `n * 2^(u+k-p) * 5^(v+k-q)` and scale is `k+sa-sb`. Cancel paired coefficient factors against a positive scale before constructing the coefficient; a negative scale becomes equal additional factors 2 and 5. This prevents a huge scale shift from triggering an oversized intermediate before cancellation.

Use wide checked exponent arithmetic; preflight result scale and coefficient growth before multiplying. A guard threshold may use at most the admitted digit width plus one; construct it only near actual operand width, never merely because the caller supplied a huge ceiling. No refused result coefficient is materialized. Powers must not come from an unchecked scale. Conservative implementation refusal is permitted if a necessary temporary cannot be bounded. Existing quota meanings do not become rounding precision.

Factor work depends on already-admitted coefficient width. Production should remove factors in bounded chunks with availability checks rather than expand decimal strings or perform one division for every trailing zero. The reference uses simple factor loops to check mathematics; it is not a performance design or benchmark. GCD and individual BigInteger primitives cannot promise immediate cancellation or a hard time bound. Check raw availability before/after those primitives and between bounded factor/growth steps; preserve release, rollback and drain obligations.

A separately selected future `bosak-exact-division-v4` / `bosak-exact-division-static-v4` identifies plans containing `div`. These names are a proposed implementation identity, not an existing supported profile. Its descriptor must explicitly bind the exact-terminating policy, algorithm revision and existing digit/scale/output limits. No public signature change is planned. Source version changes only when production implementation lands.

Alternatives remain explicit: silently rounding nonterminating results conflicts with the exact-lane requirement; a rational carrier would add an unreviewed value/API domain; full rounded decimal division needs a separate documented policy and any necessary owner disposition before freeze.
