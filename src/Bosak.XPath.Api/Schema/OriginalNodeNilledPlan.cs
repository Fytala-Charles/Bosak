// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Compiles and evaluates the bounded original-node nilled profile.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation; bounded public original-node nilled profile (1.63.0).                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Text.Json;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
namespace Bosak.XPath.Api.Schema;

/// <summary>Compiles and evaluates the bounded original-node nilled profile.</summary>
internal static class OriginalNodeNilledPlan
{
    internal const string Profile="bosak-original-node-nilled-v1";
    internal const string Descriptor="bosak-original-node-nilled-descriptor-v1";
    internal static bool TryCompile(string expression,SchemaQueryLimits limits,CompileOptions options,CancellationToken token,out QuerySourceSession? runtime)
    {
        runtime=null;token.ThrowIfCancellationRequested();
        if(expression.Trim() is not("nilled($node)" or "fn:nilled($node)")
            ||!SchemaSessionCompilation.TrySnapshotOptions(options,out var frozen,out _))return false;
        if(frozen!.Namespaces is not null&&frozen.Namespaces.TryGetValue("fn",out var fn)&&fn!="http://www.w3.org/2005/xpath-functions")return false;
        var captured=SchemaSessionCompilation.Capture(frozen,frozen.Namespaces,frozen.DefaultElementNamespace);
        var descriptor=JsonSerializer.Serialize(new{format=Descriptor,profile=Profile,staticContext=captured.Fingerprint,operation="fn:nilled",variable="node",ownership="exact-selected-source-and-original-tuple",metadata="typed-value-not-requested",output="plain-optional-boolean",cache="none",limits.MaxOpenSnapshots,limits.MaxCachedValuesPerSnapshot,limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale});
        token.ThrowIfCancellationRequested();
        runtime=new QuerySourceSession(new SchemaQueryEnvironment(captured.Namespaces,descriptor),static _=>throw new NotSupportedException("Original nilled requires explicit admitted dispatch."),limits.MaxOpenSnapshots);
        return true;
    }
    internal static XdmValue Evaluate(QuerySourceSession.Operation operation,SchemaAdvisorBinding binding,ImmutableArray<SchemaQuerySnapshot> snapshots,SchemaQueryLimits limits,EvaluationContext context)
    {
        var token=operation.Cancellation;
        void Check(){operation.Check();binding.CheckAvailable(token);operation.Check();}
        Check();var value=context.CaptureOriginalNode();Check();
        if(value.IsUndefined){Check();return XdmValue.Undefined;}
        var node=value.NodeValue??throw new ArgumentException("Missing node payload.");
        var owner=operation.FindNodeOwner(node);Check();
        void CheckNode(){Check();operation.RequireNodeOwner(owner,node);Check();}
        CheckNode();var kind=node.NodeKind;CheckNode();
        if(kind is not(XdmNodeKind.Document or XdmNodeKind.Element or XdmNodeKind.Attribute or XdmNodeKind.Text or XdmNodeKind.Comment or XdmNodeKind.ProcessingInstruction or XdmNodeKind.Namespace))
            throw new ArgumentException("Unknown original node kind.");
        if(kind!=XdmNodeKind.Element){CheckNode();return XdmValue.Undefined;}
        var snapshot=snapshots.Single(s=>ReferenceEquals(s.Runtime,owner));
        var typed=snapshot.TypedState;
        if(node is not ISchemaAnnotatedNode annotated||typed?.Origin is null)throw new NotSupportedException("Original element facts and selected original assessment are required.");
        if(typed.Issuer is not IQueryNodeFactsIssuer issuer)throw new NotSupportedException("Original node issuance is unavailable.");
        CheckNode();var facts=annotated.OriginalSchemaFacts;CheckNode();
        if(facts is null)throw new ArgumentException("Missing original facts.");
        var view=operation.ReadNodeFacts(owner,issuer,typed.Input,typed.Origin,node,facts,limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale,true);
        if(view.NodeKind!=XdmNodeKind.Element)throw new ArgumentException("Original node kind changed.");
        CheckNode();return XdmValue.FromBoolean(view.IsNilled);
    }
}
