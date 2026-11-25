# Bosak 1.86.0

Bosak provides XPath 3.1, XQuery 3.1 and XSLT 3.0 execution for .NET 10 under Apache-2.0. The libraries are shipped as one version-matched cohort; install the entry package for your application.

```sh
dotnet add package Bosak.XPath.Api --version 1.86.0
```

```csharp
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;

var expression = XPathExpression.Compile("sum((10, 20, 30))");
Console.WriteLine(expression.Evaluate(new EvaluationContext()).IntegerValue); // 60
```

Use Bosak.XQuery for XQuery, Bosak.Xslt for transformations, Bosak.XPath.Providers for XML adapters/streaming, and Bosak.XPath.Providers.Database for REST document loading. Keep all directly referenced Bosak packages at 1.86.0. The database provider package is distinct from the separate Bosak.XmlDb storage product.

## Exact calendar values and explicit comparisons

The opt-in calendar APIs preserve bounded exact decimal seconds, timezone presence and extended Gregorian years. Plain legacy values can participate through explicit interoperability APIs:

```csharp
using Bosak.XPath.Core.Xdm;

var legacy = XdmValue.FromDateTime(new DateTimeOffset(2000, 1, 2, 3, 4, 5, TimeSpan.Zero));
var exact = XdmValue.FromExactCalendar(XdmExactCalendarValue.Parse(
    "2000-01-02T03:04:05.000000001Z", XdmExactCalendarKind.DateTime, 256, 256));
Console.WriteLine(XdmCalendarInterop.CompareXPath(legacy, exact, 0)); // -1
```

Explicit APIs distinguish XPath comparison with a supplied implicit timezone, XSD partial ordering and same-key equivalence. Legacy storage cannot recover previously discarded DTO ticks. Existing default expression/map semantics and legacy accessors are unchanged. Plain interoperability refuses authority-bearing carriers.

Applications with an immutable/version-guarded schema issuer can opt into SchemaQuerySession.CompileCalendarComparison for two admitted calendar scalars, a fixed implicit timezone and one of eq/ne/lt/le/gt/ge. Actual type ownership, receiving capacities, cancellation and lifetime are checked. Results are plain booleans. See the [guarded query guide](https://github.com/Fytala-Charles/Bosak/blob/0649d9f44395aa59eef8b04091c68f0ee8ea3435/docs/guarded-calendar-query/README.md) and [calendar interoperability guide](https://github.com/Fytala-Charles/Bosak/blob/0649d9f44395aa59eef8b04091c68f0ee8ea3435/docs/calendar-interop/README.md).

## Scope and ecosystem

These bounded APIs do not provide complete XSD 1.1 validation or native Schema dateTime/time/dateTimeStamp admission. Bosak.Schema is a separate Apache-2.0 package with its own supported profiles and release lifecycle. Its native calendar admission requires a separately qualified signed Core pin. General PSVI/C45 acceptance and cross-universe compatibility remain separate.

Legacy numeric routes retain .NET decimal precision and operations that narrow dates to DateTimeOffset retain years 1-9999. Explicit exact profiles have their own capacities and refusals; they do not imply unrestricted exact arithmetic, calendar arithmetic or default-map migration. No performance or total-memory guarantee is made.

Read the [integration guide](https://github.com/Fytala-Charles/Bosak/blob/main/docs/INTEGRATION.md), [current XSD coverage audit](https://github.com/Fytala-Charles/Bosak/blob/main/docs/specification-coverage-current/README.md) and [project overview](https://github.com/Fytala-Charles/Bosak). NOTICE and license.md are included in every package; retain their required notices when redistributing.
