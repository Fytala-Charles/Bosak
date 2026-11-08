// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Executes captured XSLT through the exact public session raw operation.
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
// ===========================================================================================================================================================
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
namespace Bosak.Xslt.Api;

/// <summary>Private exact-executable transform bridge; no public BCAP capture is activated.</summary>
internal sealed class CapturedTransformHost
{
    private readonly XsltExecutable _executable;
    private readonly CapturedHostBinding _binding;
    private readonly KeyValuePair<(string LocalName,string NamespaceUri),XdmValue>[] _statics;
    internal Bosak.XPath.Api.Schema.SchemaQuerySession Session=>_binding.Session;
    private CapturedTransformHost(XsltExecutable executable,SchemaQueryEnvironment environment,SchemaQueryLimits limits,int bindings,int namespaces)
    {_executable=executable;_statics=executable.CaptureSchemaStaticValues().ToArray();_binding=new(executable,QueryHostLanguage.Xslt,environment,limits,bindings,namespaces);}
    internal static CapturedTransformHost? Compile(string source,XsltCompiler compiler,SchemaQueryLimits limits,int bindings,int namespaces,string? baseUri=null)
    {
        ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(compiler);limits.Validate();ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bindings);ArgumentOutOfRangeException.ThrowIfNegativeOrZero(namespaces);
        return SchemaHostSessionCompilation.TryCaptureXslt(source,compiler,baseUri,out var executable,out var environment,out _)?new(executable!,environment!,limits,bindings,namespaces):null;
    }
    internal T Use<T>(SchemaQueryOperation operation,SchemaQuerySnapshot? match,SchemaQuerySnapshot? global,string? template,
        EvaluationContext? context,Func<XdmValue,Action,T> consume)
    {
        ArgumentNullException.ThrowIfNull(consume);
        return _binding.Call(operation,_executable,context,match,global,frame=>
        {frame.Check();var child=frame.Context;foreach(var pair in _statics)child.WithVariable(pair.Key.LocalName,pair.Value,pair.Key.NamespaceUri);frame.Check();var value=_executable.TransformWithExplicitContext(frame.Match,frame.Global,template,child);frame.Check();var result=consume(value,frame.Check);frame.Check();return result;});
    }
    // Upper-host composition also exercises the lower XQuery assembly's private bridge without reflection.
    internal sealed class Query
    {
        private readonly Bosak.XQuery.Api.CapturedQueryHost _query;
        internal Bosak.XPath.Api.Schema.SchemaQuerySession Session=>_query.Session;
        private Query(Bosak.XQuery.Api.CapturedQueryHost query)=>_query=query;
        internal static Query? Compile(string source,Bosak.XQuery.Api.XQueryCompiler compiler,SchemaQueryLimits limits,int bindings,int namespaces)
        {var query=Bosak.XQuery.Api.CapturedQueryHost.Compile(source,compiler,limits,bindings,namespaces);return query is null?null:new(query);}
        internal T Use<T>(SchemaQueryOperation operation,EvaluationContext context,Func<XdmValue,Action,T> consume)=>_query.Use(operation,context,consume);
    }
}
