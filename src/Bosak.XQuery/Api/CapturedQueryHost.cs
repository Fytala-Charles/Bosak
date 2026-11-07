// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Executes captured XQuery through the exact public session raw operation.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2 | 05-11-2026     | Bind the public XQuery entry without exposing a delegate factory (1.72.0). |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
namespace Bosak.XQuery.Api;

/// <summary>Private production bridge; the public XQuery facade is a separate slice.</summary>
internal sealed class CapturedQueryHost
{
    private readonly XQueryExecutable _executable;
    private readonly CapturedHostBinding _binding;
    internal Bosak.XPath.Api.Schema.SchemaQuerySession Session=>_binding.Session;
    private CapturedQueryHost(XQueryExecutable executable,SchemaQueryEnvironment environment,SchemaQueryLimits limits,int bindings,int namespaces)
    {_executable=executable;_binding=new(executable,QueryHostLanguage.XQuery,environment,limits,bindings,namespaces);}
    internal static CapturedQueryHost? Compile(string query,XQueryCompiler compiler,SchemaQueryLimits limits,int bindings,int namespaces,bool xml11=false)
    {
        ArgumentNullException.ThrowIfNull(query);ArgumentNullException.ThrowIfNull(compiler);limits.Validate();ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bindings);ArgumentOutOfRangeException.ThrowIfNegativeOrZero(namespaces);
        return compiler.TryCaptureSchemaCompilation(query,xml11,out var executable,out var environment,out _)?new(executable!,environment!,limits,bindings,namespaces):null;
    }
    internal static Bosak.XPath.Api.Schema.SchemaQuerySession? CompilePublic(string query,XQueryCompiler compiler,SchemaQueryLimits limits,bool xml11,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var supported=compiler.TryCaptureSchemaCompilation(query,xml11,out var executable,out var environment,out _);
        token.ThrowIfCancellationRequested();if(!supported)return null;
        var binding=CapturedHostBinding.CreatePublicQuery(executable!,environment!,limits,executable!.EvaluateOwnedContext);
        try{token.ThrowIfCancellationRequested();return binding.Session;}
        catch(Exception primary){SchemaEvidenceKernel.PreserveFailure(primary,binding.Session.Dispose);throw;}
    }
    internal T Use<T>(SchemaQueryOperation operation,EvaluationContext context,Func<XdmValue,Action,T> consume)
    {
        ArgumentNullException.ThrowIfNull(consume);
        return _binding.Call(operation,_executable,context,null,null,frame=>
        {frame.Check();var value=_executable.EvaluateIsolatedContext(frame.Context);frame.Check();var result=consume(value,frame.Check);frame.Check();return result;});
    }
}
