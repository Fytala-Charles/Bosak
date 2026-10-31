// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 31 October 2026
// PURPOSE              : Captures immutable context for the opt-in private schema compilation path.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 31-10-2026     | Creation: private foundation admission (1.35.0)                                          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Text.Json;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Api;

/// <summary>Opt-in private compilation path; ordinary Compile and Evaluate remain unchanged.</summary>
internal static class SchemaSessionCompilation
{
    internal static bool TryCompile(string expression, CompileOptions options, ISchemaEvidenceIssuer issuer,
        int snapshotLimit, int valueLimit, out SchemaQuerySession? session, out string? unsupportedReason)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(issuer);
        session = null;
        unsupportedReason = null;
        // Delegates can capture mutable host state. A delegate reference or ToString is not a snapshot.
        if (options.FunctionParameterNames is not null || options.FunctionMinimumArity is not null || options.SchemaSet is not null)
        {
            unsupportedReason = "Host callbacks and mutable BCL schema sets need a versioned immutable capture contract.";
            return false;
        }
        if (!CanSnapshot(options.Namespaces) || !CanSnapshot(options.EnumerationItemTypes)
            || !CanSnapshot(options.ItemTypeAliases) || !CanSnapshot(options.StaticVariableTypes)
            || !CanSnapshot(options.NamedRecordTypeNames))
        {
            unsupportedReason = "A custom/non-ordinal collection cannot be captured without changing its lookup semantics.";
            return false;
        }
        var frozen = new CompileOptions
        {
            Compatibility = options.Compatibility,
            Namespaces = Copy(options.Namespaces),
            EnumerationItemTypes = Copy(options.EnumerationItemTypes),
            ItemTypeAliases = Copy(options.ItemTypeAliases),
            StaticVariableTypes = Copy(options.StaticVariableTypes),
            NamedRecordTypeNames = options.NamedRecordTypeNames?.ToImmutableHashSet(StringComparer.Ordinal),
            DefaultElementNamespace = options.DefaultElementNamespace,
            DefiningElementDefaultNamespace = options.DefiningElementDefaultNamespace,
            BaseUri = options.BaseUri,
            BackwardsCompatible = options.BackwardsCompatible,
            StrictStaticTyping = options.StrictStaticTyping,
            Xml11LineEndings = options.Xml11LineEndings,
            EnableJit = options.EnableJit
        };
        var compiled = XPath31Expression.Compile(expression, frozen);
        var environment = compiled.CaptureSchemaEnvironment(frozen);
        session = new SchemaQuerySession(environment, compiled.Evaluate, issuer, snapshotLimit, valueLimit);
        return true;
    }

    private static bool Ordinal(IEqualityComparer<string> comparer)
        => ReferenceEquals(comparer, StringComparer.Ordinal) || ReferenceEquals(comparer, EqualityComparer<string>.Default);

    private static bool CanSnapshot(IReadOnlyDictionary<string, string>? values)
        => values is null || (values switch
        {
            Dictionary<string, string> dictionary => Ordinal(dictionary.Comparer),
            ImmutableDictionary<string, string> dictionary => Ordinal(dictionary.KeyComparer),
            _ => false
        }) && values.All(pair => pair.Key is not null && pair.Value is not null);

    private static bool CanSnapshot(IReadOnlySet<string>? values)
        => values is null || (values switch
        {
            HashSet<string> set => Ordinal(set.Comparer),
            ImmutableHashSet<string> set => Ordinal(set.KeyComparer),
            _ => false
        }) && values.All(value => value is not null);

    private static ImmutableDictionary<string, string>? Copy(IReadOnlyDictionary<string, string>? values)
        => values?.ToImmutableDictionary(StringComparer.Ordinal);

    internal static SchemaQueryEnvironment Capture(CompileOptions options, IReadOnlyDictionary<string, string>? effectiveNamespaces,
        string? effectiveDefaultNamespace)
    {
        static string[][]? Ordered(IReadOnlyDictionary<string, string>? values)
            => values?.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => new[] { pair.Key, pair.Value }).ToArray();
        var descriptor = JsonSerializer.Serialize(new
        {
            format = SchemaQueryEnvironment.DescriptorFormat,
            host = "XPath",
            xdm = "https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/",
            compatibility = (int)options.Compatibility,
            namespaces = Ordered(effectiveNamespaces),
            enumerationTypes = Ordered(options.EnumerationItemTypes),
            aliases = Ordered(options.ItemTypeAliases),
            variables = Ordered(options.StaticVariableTypes),
            records = options.NamedRecordTypeNames?.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            options.DefaultElementNamespace,
            effectiveDefaultNamespace,
            options.DefiningElementDefaultNamespace,
            defaultFunctionNamespace = "http://www.w3.org/2005/xpath-functions",
            options.BaseUri,
            options.BackwardsCompatible,
            options.StrictStaticTyping,
            options.Xml11LineEndings,
            options.EnableJit
        });
        return new SchemaQueryEnvironment(effectiveNamespaces?.ToImmutableDictionary(StringComparer.Ordinal)
            ?? ImmutableDictionary<string, string>.Empty, descriptor);
    }
}
