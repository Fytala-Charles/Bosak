// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Freezes a closed typed calendar comparison and its implicit timezone.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text.Json;
using System.Text.RegularExpressions;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Api.Schema;

/// <summary>Freezes a closed typed calendar comparison and its implicit timezone.</summary>
internal sealed record CalendarComparisonPlan(string Operator, int ImplicitTimezoneMinutes)
{
    internal const string Profile = "bosak-typed-calendar-comparison-v1";
    internal const string Descriptor = "bosak-typed-calendar-comparison-descriptor-v1";
    private static readonly Regex Grammar = new(@"\A[ \t\r\n]*\$left[ \t\r\n]+(eq|ne|lt|le|gt|ge)[ \t\r\n]+\$right[ \t\r\n]*\z", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    internal static bool TryCompile(string expression, SchemaQueryLimits limits, int timezone, CompileOptions options,
        CancellationToken token, out CalendarComparisonPlan? plan, out QuerySourceSession? runtime)
    {
        plan = null; runtime = null; token.ThrowIfCancellationRequested();
        if (expression.Length > 128 || options.Compatibility != XPathCompatibility.XPath31 || options.BackwardsCompatible
            || options.StrictStaticTyping || options.Xml11LineEndings || options.EnableJit || options.StaticVariableTypes is not null
            || options.EnumerationItemTypes is not null || options.ItemTypeAliases is not null || options.NamedRecordTypeNames is not null) return false;
        var match = Grammar.Match(expression);
        if (!match.Success || !SchemaSessionCompilation.TrySnapshotOptions(options, out var frozen, out _)) return false;
        if (!SchemaSessionCompilation.TryCaptureCompilation("$value", frozen!, out var captured, out _, out _)) return false;
        token.ThrowIfCancellationRequested();
        plan = new(match.Groups[1].Value, timezone);
        var descriptor = JsonSerializer.Serialize(new { format = Descriptor, profile = Profile, staticContext = captured!.Fingerprint,
            operation = plan.Operator, implicitTimezoneMinutes = timezone, projection = "two-owned-calendar-singletons",
            limits.MaxOpenSnapshots, limits.MaxCachedValuesPerSnapshot, limits.MaxTypedValueAtoms, limits.MaxNumericDigits, limits.MaxDecimalScale });
        var environment = new SchemaQueryEnvironment(captured.Namespaces, descriptor);
        runtime = new QuerySourceSession(environment, static _ => throw new NotSupportedException("Calendar comparison requires guarded operands."),
            limits.MaxOpenSnapshots, typedIdentity: true, typedCalendar: true);
        return true;
    }
}
