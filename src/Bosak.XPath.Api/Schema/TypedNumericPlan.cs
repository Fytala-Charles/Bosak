// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Compiles an explicitly bound exact numeric profile over authenticated scalar operands.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation; bounded public typed exact numeric profile (1.60.0).                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text.Json;
using Bosak.XPath.Runtime.Vm;
namespace Bosak.XPath.Api.Schema;
/// <summary>Compiles an explicitly bound exact numeric profile over authenticated scalar operands.</summary>
internal static class TypedNumericPlan
{
    internal const string Profile="bosak-typed-exact-numeric-v1";
    internal const string Descriptor="bosak-typed-exact-numeric-descriptor-v1";
    internal static bool TryCompile(string expression,SchemaQueryLimits limits,CompileOptions options,CancellationToken token,out QuerySourceSession? runtime)
    {
        runtime=null;token.ThrowIfCancellationRequested();
        if(!SchemaSessionCompilation.TrySnapshotOptions(options,out var frozen,out _)
            ||!ExactArithmeticPlan.TryCompile(expression,frozen!,limits.MaxNumericDigits,limits.MaxDecimalScale,out var plan,token)||!plan!.IsTypedNumericCandidate)return false;
        var captured=SchemaSessionCompilation.Capture(frozen!,frozen!.Namespaces,frozen.DefaultElementNamespace);
        var descriptor=JsonSerializer.Serialize(new{format=Descriptor,profile=Profile,staticContext=captured.Fingerprint,arithmeticLane=plan.ProfileName,variable="value",projection="owned-actual-integer-before-decimal",result="plain-computed",division="exact-terminating",limits.MaxOpenSnapshots,limits.MaxCachedValuesPerSnapshot,limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale});
        runtime=new QuerySourceSession(new SchemaQueryEnvironment(captured.Namespaces,descriptor),plan.Evaluate,limits.MaxOpenSnapshots,typedNumeric:true);return true;
    }
}
