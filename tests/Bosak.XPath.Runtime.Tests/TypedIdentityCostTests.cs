// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Measures standard-function setup allocation under fresh compiled evaluation contexts.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; typed identity allocation attribution (1.57.1)                                                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Xunit.Abstractions;
namespace Bosak.XPath.Runtime.Tests;
/// <summary>Compares the same compiled VM with mutable and isolated standard-function registries.</summary>
/// <param name="output">Records warm setup allocation observations.</param>
public sealed class TypedIdentityCostTests(ITestOutputHelper output)
{
    /// <summary>Existing isolated setup preserves compiled output while avoiding repeated registry copying.</summary>
    /// <param name="source">Compiled source.</param>
    /// <param name="expected">Expected scalar output.</param>
    [Theory][InlineData("$value","abc")][InlineData("upper-case($value)","ABC")]
    public void FreshContextComparison(string source,string expected)
    {
        var expression=XPath31Expression.Compile(source);
        using var profile=Bosak.XPath.Api.Schema.SchemaQuerySession.CompileTypedIdentityCandidate("$value",new Bosak.XPath.Api.Schema.SchemaQueryLimits(4,8,8,2048,2048)).Value;
        output.WriteLine($"Identity descriptor fingerprint: {profile.Environment.StaticContextFingerprint}");
        long Measure(bool isolated)
        {
            XdmValue Run(){var context=isolated?EvaluationContext.CreateIsolated():new EvaluationContext();context.WithVariable("value",XdmValue.FromString("abc"));return expression.Evaluate(context);}
            for(int i=0;i<100;i++)Assert.Equal(expected,Run().StringValue);
            long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)Run();long bytes=(GC.GetAllocatedBytesForCurrentThread()-before)/1000;Assert.Equal(expected,Run().StringValue);return bytes;
        }
        long mutable=Measure(false),shared=Measure(true);output.WriteLine($"Identity setup {source}: mutable={mutable}; isolated={shared} bytes/call");Assert.True(shared<mutable/2,$"mutable={mutable}; isolated={shared}");
    }

    /// <summary>Optimized identity calls retain fresh mutable state even when the same raw operation is reused.</summary>
    [Fact] public void FreshCallStateDoesNotLeak()
    {
        using var f=new QueryTypedContainerTests.Fixture();EvaluationContext? prior=null;int calls=0;
        using var runtime=new QuerySourceSession(f.Session.Environment,context=>{calls++;Assert.NotSame(prior,context);Assert.False(context.TryGetVariable("poison",out _));Assert.True(context.TryGetVariable("value",out var value));context.WithVariable("poison",XdmValue.FromString("must not leak"));prior=context;return value;},4,typedIdentity:true);
        using var snapshot=runtime.OpenSnapshot(runtime.Environment,f.Input,f.Input.Enter);var c=f.Value(2,true,false);
        runtime.Run(runtime.Environment,[snapshot],op=>{var view=op.ReadRegisteredTypedContainer(snapshot,f.Issuer,f.Input,f.Origin,c,8,8,2048,2048);for(int i=0;i<2;i++){var value=view.GetTransport(i);Assert.Same(XdmValue.SchemaTransportMarker(value),XdmValue.SchemaTransportMarker(op.EvaluateTypedIdentity(snapshot,value,8,2048,2048)));}return 0;});Assert.Equal(2,calls);
    }
}
