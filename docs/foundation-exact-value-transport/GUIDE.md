# Exact numeric XDM carriers

Source 1.47.0 adds the six reviewed exact-value factory/accessor signatures on `Bosak.XPath.Core.Xdm.XdmValue`. Existing numeric factory/getter signatures remain. This is a carrier and private-evidence integration slice; numeric session execution, package publication and API freeze remain pending.

```csharp
using System.Globalization;
using System.Numerics;
using Bosak.XPath.Core.Xdm;

// The BigInteger is already owned; this factory does not impose a query admission limit.
var integer = BigInteger.Parse("123456789012345678901234567890", CultureInfo.InvariantCulture);
var integerValue = XdmValue.FromInteger(integer);
Console.WriteLine(integerValue.BigIntegerValue.ToString(CultureInfo.InvariantCulture));

var exact = new XdmExactDecimal(integer, scale: 12, maxCoefficientDigits: 64, maxScale: 64);
var decimalValue = XdmValue.FromDecimal(exact);
Console.WriteLine(decimalValue.ExactDecimalValue.ToInvariantString(maxCharacters: 80));
```

Annotated overloads accept `schemaTypeName` and optional `userSchemaTypeName`. These names remain descriptive data and do not establish issuer authority or schema validation. Factories do not fabricate assessment evidence. Small BigInteger values retain the original inline Int64 storage; wide integers and exact decimals have private typed payload holders. The existing reference slot and instance fields are retained. Private markers remain separate from numeric payloads, including marker replacement and removal.

## Checked legacy access

`BigIntegerValue` reads the complete Integer payload. `ExactDecimalValue` reads the complete Decimal payload, including an exact normalized view of an existing CLR decimal. Wrong-kind access keeps the existing type-error convention. Default exact decimal is numeric zero; default `XdmValue` remains Undefined.

`IntegerValue` returns Int64 only when the integer fits; otherwise it throws `OverflowException`. `DecimalValue` returns CLR decimal only when the normalized coefficient fits 96 bits and scale is at most 28. It constructs bits directly, without formatted parsing or rounding. A mathematically in-range value that needs excess precision also refuses.

`XdmValue.ToString` remains a checked legacy-format route and throws when these numeric narrowing limits are exceeded. For extended integers, format `BigIntegerValue` explicitly. For exact decimals, call `ExactDecimalValue.ToInvariantString` with an output character bound. This avoids implicitly expanding a tiny representation with an extreme scale into an enormous padded string.

Direct effective Boolean value checks actual zero without narrowing. CLR equality/hash remain evidence-neutral representation equality. Fresh equivalent exact holders compare alike; they are not automatically the same representation as a boxed legacy decimal. Numeric map-key equivalence is a separate operation. Legacy map keys/comparisons still use checked getters, so representable exact values work and wider values refuse. Host arithmetic, casts, indexes, aggregates, number formatting and serialization still require wider-domain integration; the ordinary public string/boolean session continues to refuse numeric inputs and syntax.

## Source migration

The reviewed new `FromDecimal(XdmExactDecimal)` overload makes an untyped `XdmValue.FromDecimal(default)` call ambiguous. Replace an old call with `XdmValue.FromDecimal(default(decimal))`. Use `default(XdmExactDecimal)` when intentionally constructing the exact carrier. `FromInteger(default)` remains resolved in the retained compiler probe, and explicit `default(long)` / `default(BigInteger)` calls compile.

[Three compile probes](evidence/SOURCE-COMPATIBILITY.json) preserve the old successful source build against exact 1.46.0 Core bytes, the expected CS0121 candidate refusal and the successful typed-default correction. Retaining binary signatures is not an unconditional source-compatibility claim. Actual owner source/unchanged-binary qualification and the final release SemVer must account for this migration before freeze/publication.

## Costs and remaining integration

The qualified 64-bit process retains the 48-byte XdmValue carrier. Warm small-integer factories, inline marker attachment, cached holder reads and checked CLR decimal reads from the selected exact holder allocate zero bytes per operation. Selected wide-integer and exact-decimal holder creation costs 32 and 40 bytes; marking an already-built wide numeric value costs a further 32 bytes. Input allocation is excluded. Exact views converted from legacy values may allocate; the cached-holder zero-allocation result does not promise otherwise. These are observations, not owner workload, throughput or retained-memory budgets.

[Source audit](AUDIT.md) records the 192 direct legacy numeric getter references and pending routes. Explicit operation limits, exact parser/arithmetic/comparison/map/serialization routing and full schema/all-host qualification precede numeric profile enablement. Public SchemaAtomics/provenance, native adoption, C45/freeze and publication remain pending.
