# Exact decimal representation

Source 1.46.0 adds the reviewed `Bosak.XPath.Core.Xdm.XdmExactDecimal` public value type. It represents an exact integer coefficient divided by a nonnegative power of ten. Package publication and API freeze remain pending.

```csharp
using System.Globalization;
using System.Numerics;
using Bosak.XPath.Core.Xdm;

// This coefficient already belongs to the caller; its earlier allocation is outside these limits.
var coefficient = BigInteger.Parse(
    "123456789012345678901234567890123456789", CultureInfo.InvariantCulture);
var value = new XdmExactDecimal(coefficient, scale: 2,
    maxCoefficientDigits: 64, maxScale: 64);
Console.WriteLine(value.ToInvariantString(maxCharacters: 80));
// 1234567890123456789012345678901234567.89
```

Choose positive coefficient/output limits and a nonnegative scale limit for your workload. There are no defaults. Negative scales and invalid bounds throw `ArgumentOutOfRangeException`; exceeding supplied capacity throws `NotSupportedException`. Check these outcomes separately from schema validity. Limits supplied to this constructor cannot prevent an allocation performed earlier by `BigInteger.Parse` in the example.

## Value semantics

Trailing coefficient zeros normalize only while scale remains positive. Zero normalizes to coefficient/scale zero, including `default(XdmExactDecimal)`. Equivalent normalized decimals compare and hash equally. Leading sign is not a digit; supplied coefficient capacity is checked before normalization, so coefficient 1000 with scale 3 still requires four supplied digits even though the normalized value is 1. Supplied scale is also checked before zero or trailing-zero normalization.

`ToInvariantString` is culture independent and explicitly bounds sign, decimal point, coefficient digits and fractional padding before formatting the coefficient. It returns normalized numeric text; its contract is not a general XSD canonical-lexical serializer. A huge scale can be valid as representation while being refused by an output capacity. Output is allocated only within the selected character limit; this is not a hard total-memory or latency budget.

## Lexical ingress and current boundaries

Core also has an internal parser for subsequent numeric integration. It accepts the XSD decimal lexical forms: optional sign, ASCII digits, one optional decimal point and at least one digit, with XML boundary whitespace. Exponent notation, grouping, Unicode digits and non-XML whitespace refuse. The parser checks every supplied digit, including leading/trailing zeros, and fractional scale before building a coefficient. This lexical work quota is stricter than normalized coefficient size and is separate from XSD facets. If a quota is reached before a malformed tail is inspected, the outcome is capacity refusal, not proved invalidity.

The parser is not a new public parsing API and is not connected to host expression parsing yet. `XdmValue` exact carriers/accessors, arithmetic, comparisons, map keys, serialization, legacy narrowing and schema evidence transport require dependent integration. The ordinary-source session profile continues to refuse numeric syntax and inputs. Existing XPath decimal precision limitations therefore remain. Do not infer full XSD validation, actual owner adoption or native ABI acceptance from this value type.
