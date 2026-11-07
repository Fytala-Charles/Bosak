// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Compiles and evaluates the bounded original-node data profile.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation; bounded public original-node data profile (1.64.0).                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Text.Json;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
namespace Bosak.XPath.Api.Schema;

/// <summary>Compiles and evaluates the bounded original-node data profile.</summary>
internal static class OriginalNodeDataPlan
{
    internal const string Profile="bosak-original-node-data-v1";
    internal const string Descriptor="bosak-original-node-data-descriptor-v1";
    internal static bool TryCompile(string expression,SchemaQueryLimits limits,CompileOptions options,CancellationToken token,out QuerySourceSession? runtime)
    {
        runtime=null;token.ThrowIfCancellationRequested();
        if(expression.Trim() is not("data($node)" or "fn:data($node)")
            ||!SchemaSessionCompilation.TrySnapshotOptions(options,out var frozen,out _))return false;
        if(frozen!.Namespaces is not null&&frozen.Namespaces.TryGetValue("fn",out var fn)&&fn!="http://www.w3.org/2005/xpath-functions")return false;
        var captured=SchemaSessionCompilation.Capture(frozen,frozen.Namespaces,frozen.DefaultElementNamespace);
        var descriptor=JsonSerializer.Serialize(new{format=Descriptor,profile=Profile,staticContext=captured.Fingerprint,operation="fn:data",variable="node",ownership="exact-selected-source-and-original-tuple",metadata="original-typed-container",output="ordered-materialized-zero-one-many",cache="scalar-container-origin-ordinal",transaction="whole-batch",provenance="original-cell-types-and-assessment",limits.MaxOpenSnapshots,limits.MaxCachedValuesPerSnapshot,limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale});
        token.ThrowIfCancellationRequested();
        runtime=new QuerySourceSession(new SchemaQueryEnvironment(captured.Namespaces,descriptor),static _=>throw new NotSupportedException("Original data requires explicit admitted dispatch."),limits.MaxOpenSnapshots);
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
        if(kind is not(XdmNodeKind.Element or XdmNodeKind.Attribute))throw new NotSupportedException("This profile atomizes original elements and attributes only.");
        var snapshot=snapshots.Single(s=>ReferenceEquals(s.Runtime,owner));
        var typed=snapshot.TypedState;
        if(node is not ISchemaAnnotatedNode annotated||typed?.Origin is null)throw new NotSupportedException("Original node facts and selected original assessment are required.");
        if(typed.Issuer is not IQueryNodeFactsIssuer issuer)throw new NotSupportedException("Original node issuance is unavailable.");
        CheckNode();var facts=annotated.OriginalSchemaFacts;CheckNode();
        if(facts is null)throw new ArgumentException("Missing original facts.");
        return operation.AtomizeOriginalNode(owner,issuer,typed.Input,typed.Origin,node,facts,kind,limits.MaxCachedValuesPerSnapshot,limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale);
    }
}
