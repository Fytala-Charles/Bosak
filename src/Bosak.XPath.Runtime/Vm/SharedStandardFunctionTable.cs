// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Shares read-only standard function mappings with private copy-on-write contexts.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-11-2026     | Creation; Isolated function sharing (1.44.1)                                                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Runtime.CompilerServices;
using Bosak.XPath.Runtime.Functions;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>Privately owned ordered mapping snapshot; function signatures retain legacy shallow sharing.</summary>
internal sealed class SharedStandardFunctionTable
{
    private static readonly ConditionalWeakTable<Dictionary<(string, string, int), FunctionSignature>, SharedStandardFunctionTable> Cache = new();
    internal static readonly SharedStandardFunctionTable Empty = new(null, new());
    private readonly Dictionary<(string, string, int), FunctionSignature> _functions;
    private readonly Dictionary<(string, string), int> _variadic = new();
    internal object? Source { get; }
    internal int Count => _functions.Count;
    internal Dictionary<(string, string, int), FunctionSignature>.Enumerator GetEnumerator() => _functions.GetEnumerator();
    static SharedStandardFunctionTable() { }
    private SharedStandardFunctionTable(object? source, Dictionary<(string, string, int), FunctionSignature> functions)
    {
        Source = source; _functions = new(functions);
        foreach (var ((ns, name, arity), signature) in _functions)
            if (signature.IsVariadic && (!_variadic.TryGetValue((ns, name), out int min) || arity < min)) _variadic[(ns, name)] = arity;
    }
    internal static SharedStandardFunctionTable Get(Dictionary<(string, string, int), FunctionSignature> source)
        => Cache.GetValue(source, static template => new SharedStandardFunctionTable(template, template));
    internal bool TryGetFunction((string, string, int) key, out FunctionSignature signature) => _functions.TryGetValue(key, out signature!);
    internal bool TryGetVariadic((string, string) key, out int arity) => _variadic.TryGetValue(key, out arity);
    internal Dictionary<(string, string, int), FunctionSignature> CopyFunctions() => new(_functions);
    internal void CopyVariadicTo(Dictionary<(string, string), int> destination)
    { destination.Clear(); foreach (var pair in _variadic) destination.Add(pair.Key, pair.Value); }
}
