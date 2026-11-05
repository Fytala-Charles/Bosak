// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Verifies isolated standard-function sharing and copy-on-write behavior.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
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
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Functions;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Checks observable registry isolation, profile behavior and allocation reduction.</summary>
public sealed class IsolatedFunctionTableTests
{
    private const string Fn="http://www.w3.org/2005/xpath-functions";
    private static EvaluationContext Isolated() => new EvaluationContext().CaptureSchemaInputs(_=>false,16,16).CreateContext();
    private static FunctionSignature Signature(string ns,string name,int arity,string value,bool variadic=false,bool only40=false) => new()
    {
        NamespaceUri=ns,LocalName=name,Arity=arity,ParameterTypes=Enumerable.Repeat(XdmValueKind.String,arity).ToArray(),ReturnType=XdmValueKind.String,
        IsVariadic=variadic,IsXPath40Only=only40,Implementation=(_,_)=>XdmValue.FromString(value)
    };
    private static void Compare(EvaluationContext expected,EvaluationContext actual)
    {
        var a=expected.SnapshotFunctions();var b=actual.SnapshotFunctions();Assert.Equal(a.Keys,b.Keys);
        foreach(var (key,signature) in a)
        {
            Assert.Same(signature,b[key]);
            Assert.Equal(expected.TryResolveFunction(key.NamespaceUri,key.LocalName,key.Arity,out var x),actual.TryResolveFunction(key.NamespaceUri,key.LocalName,key.Arity,out var y));
            Assert.Same(x,y);
        }
        Assert.Equal(expected.TryResolveFunction(Fn,"concat",99,out var ca),actual.TryResolveFunction(Fn,"concat",99,out var cb));Assert.Same(ca,cb);
    }
    /// <summary>Standard lookup, order and variadic dispatch match public contexts in every populated profile.</summary>
    /// <param name="isStatic">Whether XSLT dynamic functions are excluded by the template.</param>
    /// <param name="level">Zero for 3.1, one for frozen 4.0, two for experimental 4.0.</param>
    [Theory][InlineData(false,0)][InlineData(false,1)][InlineData(false,2)][InlineData(true,0)][InlineData(true,1)][InlineData(true,2)]
    public void ProfilesMatchLegacy(bool isStatic,int level)
    {
        var legacy=new EvaluationContext{IsStaticEvaluation=isStatic,IsXPath40=level>0,IsXPath40Experimental=level>1};
        var isolated=Isolated();isolated.IsStaticEvaluation=isStatic;isolated.IsXPath40=level>0;isolated.IsXPath40Experimental=level>1;
        FunctionLibrary.Populate(legacy);FunctionLibrary.Populate(isolated);Compare(legacy,isolated);
    }
    /// <summary>Replacing a standard entry cannot mutate another context or the shared standard template.</summary>
    [Fact] public void ReplacementIsContextLocal()
    {
        var first=Isolated();var second=Isolated();FunctionLibrary.Populate(first);FunctionLibrary.Populate(second);
        Assert.True(second.TryResolveFunction(Fn,"string",1,out var original));var replacement=Signature(Fn,"string",1,"replacement");first.RegisterFunction(replacement);
        Assert.True(first.TryResolveFunction(Fn,"string",1,out var own));Assert.Same(replacement,own);Assert.True(second.TryResolveFunction(Fn,"string",1,out var peer));Assert.Same(original,peer);
        var third=Isolated();FunctionLibrary.Populate(third);Assert.True(third.TryResolveFunction(Fn,"string",1,out var fresh));Assert.Same(original,fresh);
    }
    /// <summary>Registration before first standard population preserves the caller's nonstandard functions.</summary>
    [Fact] public void CustomBeforePopulationSurvives()
    {
        var context=Isolated();var custom=Signature("urn:test","custom",1,"custom");context.RegisterFunction(custom);FunctionLibrary.Populate(context);
        Assert.True(context.TryResolveFunction("urn:test","custom",1,out var result));Assert.Same(custom,result);
        var peer=Isolated();FunctionLibrary.Populate(peer);Assert.False(peer.TryResolveFunction("urn:test","custom",1,out _));
    }
    /// <summary>Variadic removal is local and repopulation restores only standard bindings.</summary>
    [Fact] public void VariadicRemovalAndRepopulationAreLocal()
    {
        var first=Isolated();var peer=Isolated();FunctionLibrary.Populate(first);FunctionLibrary.Populate(peer);
        Assert.True(first.UnregisterFunction(Fn,"concat",2));Assert.False(first.TryResolveFunction(Fn,"concat",99,out _));Assert.True(peer.TryResolveFunction(Fn,"concat",99,out _));
        var extra=Signature("urn:test","custom",0,"x");first.RegisterFunction(extra);FunctionLibrary.Populate(first);Assert.True(first.TryResolveFunction(Fn,"concat",99,out _));Assert.True(first.TryResolveFunction("urn:test","custom",0,out var own));Assert.Same(extra,own);
    }
    /// <summary>Clearing one context does not clear another context's functions or variadic index.</summary>
    [Fact] public void ClearIsContextLocal()
    {
        var first=Isolated();var peer=Isolated();FunctionLibrary.Populate(first);FunctionLibrary.Populate(peer);first.ClearFunctions();
        Assert.Empty(first.SnapshotFunctions());Assert.False(first.TryResolveFunction(Fn,"concat",99,out _));Assert.True(peer.TryResolveFunction(Fn,"concat",99,out _));FunctionLibrary.Populate(first);Compare(peer,first);
    }
    /// <summary>A copied function snapshot is writable and restoring it never mutates its source or peers.</summary>
    [Fact] public void SnapshotRestoreIsDetached()
    {
        var first=Isolated();var peer=Isolated();FunctionLibrary.Populate(first);FunctionLibrary.Populate(peer);var snapshot=first.SnapshotFunctions();snapshot.Remove((Fn,"string",1));
        Assert.True(first.TryResolveFunction(Fn,"string",1,out _));first.RestoreFunctions(snapshot);snapshot.Clear();Assert.False(first.TryResolveFunction(Fn,"string",1,out _));Assert.True(peer.TryResolveFunction(Fn,"string",1,out _));Assert.True(first.TryResolveFunction(Fn,"concat",99,out _));
    }
    /// <summary>Successive profile changes preserve the same historical merge and visibility semantics.</summary>
    [Fact] public void ProfileTransitionsMatchLegacyMerge()
    {
        var legacy=new EvaluationContext();var isolated=Isolated();
        foreach(var (isStatic,level) in new[]{(false,2),(true,0),(false,1),(false,0),(true,2)})
        {
            legacy.IsStaticEvaluation=isolated.IsStaticEvaluation=isStatic;legacy.IsXPath40=isolated.IsXPath40=level>0;legacy.IsXPath40Experimental=isolated.IsXPath40Experimental=level>1;
            FunctionLibrary.Populate(legacy);FunctionLibrary.Populate(isolated);Compare(legacy,isolated);
        }
    }
    /// <summary>Repeated population still overwrites local standard replacements.</summary>
    [Fact] public void RepeatedPopulationRestoresStandardReplacement()
    {
        var context=Isolated();FunctionLibrary.Populate(context);FunctionLibrary.Populate(context);Assert.True(context.TryResolveFunction(Fn,"string",1,out var standard));
        context.RegisterFunction(Signature(Fn,"string",1,"x"));FunctionLibrary.Populate(context);Assert.True(context.TryResolveFunction(Fn,"string",1,out var restored));Assert.Same(standard,restored);
    }
    /// <summary>Multiple variadic overloads and version-gated minima retain legacy selection behavior.</summary>
    [Fact] public void CustomVariadicFallbackMatchesLegacy()
    {
        var legacy=new EvaluationContext();var isolated=Isolated();FunctionLibrary.Populate(legacy);FunctionLibrary.Populate(isolated);
        var gated=Signature("urn:test","many",2,"gated",true,true);var visible=Signature("urn:test","many",3,"visible",true);
        foreach(var c in new[]{legacy,isolated}){c.RegisterFunction(gated);c.RegisterFunction(visible);}
        Assert.True(legacy.TryResolveFunction("urn:test","many",8,out var a));Assert.True(isolated.TryResolveFunction("urn:test","many",8,out var b));Assert.Same(a,b);Assert.Same(visible,b);
        foreach(var c in new[]{legacy,isolated})c.UnregisterFunction("urn:test","many",2);
        Assert.True(legacy.TryResolveFunction("urn:test","many",8,out a));Assert.True(isolated.TryResolveFunction("urn:test","many",8,out b));Assert.Same(a,b);
    }
    /// <summary>Missing removal returns false and leaves all shared peers intact.</summary>
    [Fact] public void MissingRemovalDoesNotAffectPeers()
    {var first=Isolated();var peer=Isolated();FunctionLibrary.Populate(first);FunctionLibrary.Populate(peer);Assert.False(first.UnregisterFunction("urn:missing","x",0));Compare(peer,first);}
    /// <summary>Hidden lexical aliases can be captured/restored without appearing in sibling contexts.</summary>
    [Fact] public void LexicalAliasesStayLocal()
    {
        var first=Isolated();var peer=Isolated();FunctionLibrary.Populate(first);FunctionLibrary.Populate(peer);
        var target=new FunctionSignature{NamespaceUri="urn:lexical",LocalName="target",Arity=0,ParameterTypes=[],ReturnType=XdmValueKind.String,Implementation=(_,_)=>XdmValue.FromString("target"),IsBoundFunctionItem=true};
        var alias=new FunctionSignature{NamespaceUri="urn:lexical",LocalName="alias",Arity=0,ParameterTypes=[],ReturnType=XdmValueKind.String,Implementation=target.Implementation,IsHiddenFromFunctionLookup=true,FunctionItemTarget=target};
        first.RegisterFunction(alias);var snapshot=first.SnapshotBoundFunctionAliases();Assert.NotNull(snapshot);Assert.Single(snapshot);
        first.ClearFunctions();FunctionLibrary.Populate(first);first.RestoreBoundFunctionAliases(snapshot);Assert.True(first.TryResolveFunction("urn:lexical","alias",0,out var restored));Assert.Same(alias,restored);Assert.Null(peer.SnapshotBoundFunctionAliases());
    }
    /// <summary>Variables, focus, namespaces, timezone and resource policy remain independent of shared mappings.</summary>
    [Fact] public void DynamicStateIsNeverShared()
    {
        var caller=new EvaluationContext().WithVariable("v",XdmValue.FromInteger(7)).WithNamespace("p","urn:original");var inputs=caller.CaptureSchemaInputs(_=>false,16,16);
        var first=inputs.CreateContext();var peer=inputs.CreateContext();FunctionLibrary.Populate(first);FunctionLibrary.Populate(peer);
        first.WithVariable("v",XdmValue.FromInteger(99)).WithNamespace("p","urn:changed");first.ImplicitTimezoneOffsetMinutes=60;first.ResourcePolicy=SchemaProviderSessionBinding.ClosedResources();
        first.WithFocus(XdmValue.FromNode(new XDocumentNode(XDocument.Parse("<first/>"))),1,1);
        Assert.True(peer.TryGetVariable("v",out var value));Assert.Equal(7,value.IntegerValue);Assert.True(caller.TryGetVariable("v",out value));Assert.Equal(7,value.IntegerValue);
        Assert.True(peer.TryResolveNamespace("p",out var uri));Assert.Equal("urn:original",uri);Assert.Null(peer.ResourcePolicy);Assert.True(peer.ContextItem.IsUndefined);Assert.Equal(0,peer.ImplicitTimezoneOffsetMinutes);
    }
    /// <summary>Populated shared registries remain outside the bounded caller-input capture profile.</summary>
    [Fact] public void PopulatedContextCannotReenterCallerCapture()
    {var context=Isolated();FunctionLibrary.Populate(context);Assert.Throws<NotSupportedException>(()=>context.CaptureSchemaInputs(_=>false,16,16));}
    /// <summary>Private population saves most per-context allocations without a timing-dependent assertion.</summary>
    [Fact] public void SharedPopulationAllocationRegression()
    {
        for(int i=0;i<100;i++){var a=new EvaluationContext();FunctionLibrary.Populate(a);var b=Isolated();FunctionLibrary.Populate(b);}
        long legacy=Measure(()=>{var c=new EvaluationContext();FunctionLibrary.Populate(c);GC.KeepAlive(c);});
        long isolated=Measure(()=>{var c=Isolated();FunctionLibrary.Populate(c);GC.KeepAlive(c);});
        Assert.True(isolated<legacy/2,$"Isolated {isolated} vs public {legacy} bytes.");
        static long Measure(Action action){long start=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<64;i++)action();return GC.GetAllocatedBytesForCurrentThread()-start;}
    }
    /// <summary>Repeated isolated evaluations cannot observe mutation of an earlier caller context.</summary>
    [Fact] public void CapturedInputsAndRepeatedEvaluationStayIsolated()
    {
        Assert.True(SchemaSessionCompilation.TryCompileOrdinary("$v",new CompileOptions(),4,out var session,out var reason),reason);using var s=session!;
        var caller=new EvaluationContext().WithVariable("v",XdmValue.FromInteger(7));
        s.Run(s.Environment,[],op=>{var inputs=op.CaptureInputs(caller,16,16);caller.WithVariable("v",XdmValue.FromInteger(99));Assert.Equal(7,op.EvaluateAtomic(inputs).IntegerValue);Assert.Equal(7,op.EvaluateAtomic(inputs).IntegerValue);return 0;});
    }
}
