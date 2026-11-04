// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Compiles a bounded, explicitly identified typed scalar variable identity query.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; internal compiled typed identity profile (1.57.0).                              |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text.Json;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Api.Schema;

/// <summary>Compiles a bounded, explicitly identified typed scalar variable identity query.</summary>
internal static class TypedIdentityPlan
{
    internal const string Profile="bosak-internal-typed-identity-v1";
    internal const string Descriptor="bosak-typed-identity-descriptor-v1";
    internal static bool TryCompile(string expression,SchemaQueryLimits limits,CompileOptions options,CancellationToken token,out QuerySourceSession? runtime)
    {
        runtime=null;token.ThrowIfCancellationRequested();
        if(expression.Trim()!="$value"||!SchemaSessionCompilation.TrySnapshotOptions(options,out var frozen,out _))return false;
        if(!SchemaSessionCompilation.TryCaptureCompilation("$value",frozen!,out var captured,out var evaluate,out _))return false;
        token.ThrowIfCancellationRequested();
        var descriptor=JsonSerializer.Serialize(new{format=Descriptor,profile=Profile,staticContext=captured!.Fingerprint,variable="value",limits.MaxOpenSnapshots,limits.MaxCachedValuesPerSnapshot,limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale});
        var environment=new SchemaQueryEnvironment(captured.Namespaces,descriptor);
        runtime=new QuerySourceSession(environment,evaluate!,limits.MaxOpenSnapshots,typedIdentity:true);return true;
    }
}
