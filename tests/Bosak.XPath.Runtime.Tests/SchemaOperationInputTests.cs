// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 November 2026
// PURPOSE              : Checks typed input isolation, explicit profile refusal and real operation result lifetime.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 01-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Reflection;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Protects bounded capture and atomic visitation inside actual private session leases.</summary>
public sealed class SchemaOperationInputTests
{
    private sealed class Issuer : ISchemaEvidenceIssuer
    {
        internal readonly object Origin = new();
        bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar value) => false;
        bool ISchemaEvidenceIssuer.OwnsType(object type) => false;
        bool ISchemaEvidenceIssuer.OwnsAssessment(object origin) => ReferenceEquals(origin, Origin);
        bool ISchemaEvidenceIssuer.IsSameType(object a, object b) => ReferenceEquals(a, b);
    }
    private static IXdmNode Node() => new XDocumentNode(XDocument.Parse("<n>value</n>"));
    private static SchemaQuerySession Compile(string source, Issuer issuer)
    {
        Assert.True(SchemaSessionCompilation.TryCompile(source, new CompileOptions(), issuer, 4, 16, out var session, out var reason), reason);
        return session!;
    }

    /// <summary>Copies supported data without sharing caller registries or observing later binding mutations.</summary>
    [Fact]
    public void CapturesIndependentSupportedBindings()
    {
        var node = Node();
        var caller = new EvaluationContext { BaseUri = "urn:base", DefaultElementNamespace = "urn:elements", DefiningElementDefaultNamespace = "urn:defining", DefaultCollation = "urn:collation" }
            .WithFocus(XdmValue.FromNode(node), 1, 1).WithVariable("v", XdmValue.FromInteger(42)).WithNamespace("p", "urn:before");
        var inputs = caller.CaptureSchemaInputs(n => ReferenceEquals(n, node), 16, 16);
        caller.WithVariable("v", XdmValue.FromInteger(99)).WithNamespace("p", "urn:after");
        var child = inputs.CreateContext();
        Assert.True(child.TryGetVariable("v", out var value)); Assert.Equal(42L, value.IntegerValue);
        Assert.True(child.TryResolveNamespace("p", out var uri)); Assert.Equal("urn:before", uri);
        Assert.Equal(caller.BaseUri, child.BaseUri); Assert.Equal(caller.DefaultElementNamespace, child.DefaultElementNamespace);
        Assert.Equal(caller.DefiningElementDefaultNamespace, child.DefiningElementDefaultNamespace); Assert.Equal(caller.DefaultCollation, child.DefaultCollation);
        Assert.Same(node, child.ContextItem.NodeValue); Assert.NotSame(child, inputs.CreateContext());
        child.WithVariable("extra", XdmValue.FromInteger(1)); Assert.False(caller.TryGetVariable("extra", out _));
    }

    /// <summary>Refuses nondefault execution state before consulting node membership.</summary>
    /// <param name="state">The configured state outside the bounded data profile.</param>
    [Theory]
    [InlineData("loader")][InlineData("decimal")][InlineData("functions")][InlineData("cache")][InlineData("date")]
    [InlineData("alias-comparer")][InlineData("lazy")][InlineData("collection")][InlineData("flag")]
    public void RefusesAdvancedState(string state)
    {
        var caller = new EvaluationContext().WithFocus(XdmValue.FromNode(Node()), 1, 1);
        switch (state)
        {
            case "loader": caller.DocumentLoader = _ => throw new Exception("Must not run"); break;
            case "decimal": caller.WithDecimalFormat(null, new DecimalFormat { DecimalSeparator = "," }); break;
            case "functions": FunctionLibrary.Populate(caller); break;
            case "cache": caller.RegisterDocument("urn:cached", Node()); break;
            case "date": _ = caller.CurrentDateTimeSnapshot; break;
            case "alias-comparer": caller.BindingItemTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); break;
            case "lazy": caller.LazyVariableResolver = (_, _) => throw new Exception("Must not run"); break;
            case "collection": caller.Collections.Add("urn:collection", new[] { "urn:item" }); break;
            case "flag": caller.IsXsltMode = true; break;
        }
        int calls = 0;
        Assert.Throws<NotSupportedException>(() => caller.CaptureSchemaInputs(_ => { calls++; return true; }, 16, 16));
        Assert.Equal(0, calls);
    }

    /// <summary>Refuses foreign node membership without changing caller bindings.</summary>
    [Fact]
    public void RefusesForeignNodes()
    {
        var node = Node(); var caller = new EvaluationContext().WithVariable("n", XdmValue.FromNode(node));
        Assert.Throws<NotSupportedException>(() => caller.CaptureSchemaInputs(_ => false, 16, 16));
        Assert.True(caller.TryGetVariable("n", out var value)); Assert.Same(node, value.NodeValue);
        Assert.Throws<NotSupportedException>(() => new EvaluationContext().WithVariable("v", XdmValue.FromSequence(XdmSequence.Empty)).CaptureSchemaInputs(_ => false, 16, 16));
    }

    /// <summary>Absent focus cannot carry a contradictory position or size.</summary>
    [Fact]
    public void RefusesInvalidFocus()
        => Assert.Throws<NotSupportedException>(() => new EvaluationContext().WithFocus(XdmValue.Undefined, 1, 1).CaptureSchemaInputs(_ => false, 16, 16));

    /// <summary>Validates explicit input count bounds.</summary>
    /// <param name="bindings">Requested binding capacity.</param>
    /// <param name="namespaces">Requested namespace capacity.</param>
    [Theory][InlineData(0, 16)][InlineData(16, 0)][InlineData(-1, 16)]
    public void RefusesInvalidLimits(int bindings, int namespaces)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new EvaluationContext().CaptureSchemaInputs(_ => true, bindings, namespaces));

    /// <summary>Refuses actual input counts above capacity.</summary>
    /// <param name="bindings">Whether the overflowing collection is the variable map.</param>
    [Theory][InlineData(true)][InlineData(false)]
    public void EnforcesInputCapacity(bool bindings)
    {
        var caller = new EvaluationContext().WithVariable("a", XdmValue.FromInteger(1)).WithVariable("b", XdmValue.FromInteger(2));
        Assert.Throws<NotSupportedException>(() => caller.CaptureSchemaInputs(_ => true, bindings ? 1 : 16, bindings ? 16 : 1));
    }

    /// <summary>Every new context field requires an explicit bounded-profile review.</summary>
    [Fact]
    public void ContextFieldPolicyHasNoUnreviewedFields()
    {
        string[] reviewed = { "<AccumulatorValueCopier>k__BackingField", "<AttributeConstructorHook>k__BackingField", "<BackwardsCompatible>k__BackingField", "<BaseUri>k__BackingField", "<BindingItemTypes>k__BackingField", "<CollationComparer>k__BackingField", "<CollectionLoader>k__BackingField", "<CollectionValues>k__BackingField", "<Collections>k__BackingField", "<ConstructedDocumentProcessor>k__BackingField", "<ConstructedElementProcessor>k__BackingField", "<ContentNodeConstructorHook>k__BackingField", "<CurrentOutputUri>k__BackingField", "<DefaultCollation>k__BackingField", "<DefaultElementNamespace>k__BackingField", "<DefiningElementDefaultNamespace>k__BackingField", "<DocumentConstructorHook>k__BackingField", "<DocumentLoadPolicy>k__BackingField", "<DocumentLoaded>k__BackingField", "<DocumentLoader>k__BackingField", "<DocumentPostProcessor>k__BackingField", "<ElementConstructorHook>k__BackingField", "<ExecutionGate>k__BackingField", "<FunctionLookupInterceptor>k__BackingField", "<InStreamedPipeline>k__BackingField", "<InStreamingMapContext>k__BackingField", "<InitialFocusResolver>k__BackingField", "<InitialTemplateCallParameters>k__BackingField", "<InitialTemplateTunnelParameters>k__BackingField", "<IsSchemaAware>k__BackingField", "<IsStaticEvaluation>k__BackingField", "<IsXPath40>k__BackingField", "<IsXPath40Experimental>k__BackingField", "<IsXsltMode>k__BackingField", "<LazyVariableResolver>k__BackingField", "<NamedRecordCoercion>k__BackingField", "<NamedRecordTypes>k__BackingField", "<RegexGroups>k__BackingField", "<ResourcePolicy>k__BackingField", "<ResourceUriMapper>k__BackingField", "<SchemaResolver>k__BackingField", "<SkipLazyGlobalCacheOnce>k__BackingField", "<SkipStandardFunctionPopulation>k__BackingField", "<StaticOutputParameters>k__BackingField", "<StreamingDocumentLoader>k__BackingField", "<SuppressLazyGlobalCaching>k__BackingField", "<SuppressLazySequenceConstructorGlobals>k__BackingField", "<UnsuppliedExternalVariables>k__BackingField", "<XQueryModuleLoader>k__BackingField", "<XQueryModuleSources>k__BackingField", "<Xml11Mode>k__BackingField", "<XsltVersion>k__BackingField", "_constructorLocalNamespaces", "_contextItem", "_contextPosition", "_contextSize", "_currentDateTimeSnapshot", "_currentItem", "_defaultDecimalFormat", "_documentCache", "_evaluatedLazyGlobals", "_functions", "_implicitTimezoneOffsetMinutes", "_implicitTimezoneOffsetSet", "_namedDecimalFormats", "_namespaces", "_schemaSet", "_variables", "_variadicMinArity" };
        Assert.Equal(reviewed.OrderBy(n => n, StringComparer.Ordinal), typeof(EvaluationContext).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Select(f => f.Name).OrderBy(n => n, StringComparer.Ordinal));
    }

    /// <summary>Retains the typed result through raw async consumption and rejects later escape.</summary>
    /// <returns>The completed lifetime checks.</returns>
    [Fact]
    public async Task TypedResultUsesActualRawCallbackLifetime()
    {
        var issuer = new Issuer(); using var session = Compile("$v", issuer); int releases = 0;
        using var snapshot = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => releases++);
        var caller = new EvaluationContext().WithVariable("v", XdmValue.FromInteger(42));
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        SchemaAtomicResult escaped = default;
        var task = session.RunAsync(session.Environment, snapshot, async op =>
        {
            var inputs = op.CaptureInputs(caller, _ => false, 16, 16); escaped = op.EvaluateAtomic(inputs, 16); entered.SetResult(); await resume.Task;
            var values = new List<long>(); escaped.ForEachAtomic(v => values.Add(v.IntegerValue)); return values;
        });
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task.WaitAsync(canceled.Token));
        Assert.False(task.IsCompleted); Assert.Equal(0, releases); resume.SetResult();
        Assert.Equal(new long[] { 42 }, await task.WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.Throws<ObjectDisposedException>(() => escaped.ForEachAtomic(_ => { }));
        snapshot.Dispose(); Assert.Equal(1, releases); Assert.True(caller.TryGetVariable("v", out var original)); Assert.Equal(42L, original.IntegerValue);
    }

    /// <summary>Handles empty output, structured refusal and bounded visitation.</summary>
    /// <param name="profile">Selected result behavior.</param>
    [Theory][InlineData("empty")][InlineData("node")][InlineData("limit")]
    public void EnforcesAtomicResultProfile(string profile)
    {
        var issuer = new Issuer(); using var session = Compile(profile == "empty" ? "()" : profile == "node" ? "$n" : "(1,2)", issuer);
        using var snapshot = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => { });
        session.Run(session.Environment, snapshot, op =>
        {
            var caller = new EvaluationContext().WithVariable("n", XdmValue.FromNode(Node()));
            var result = op.EvaluateAtomic(op.CaptureInputs(caller, _ => true, 16, 16), 1); int visits = 0;
            Assert.Throws<ArgumentOutOfRangeException>(() => op.EvaluateAtomic(op.CaptureInputs(caller, _ => true, 16, 16), 0));
            if (profile == "empty") result.ForEachAtomic(_ => visits++);
            else Assert.Throws<NotSupportedException>(() => result.ForEachAtomic(_ => visits++));
            Assert.Equal(profile == "limit" ? 1 : 0, visits); return 0;
        });
        Assert.Throws<InvalidOperationException>(() => default(SchemaAtomicResult).ForEachAtomic(_ => { }));
    }

    /// <summary>Closing admission refuses typed capture before ownership callbacks.</summary>
    [Fact]
    public void ClosedOperationRefusesTypedCapture()
    {
        var issuer = new Issuer(); using var session = Compile("1", issuer); using var snapshot = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => { });
        session.Run(session.Environment, snapshot, op => { snapshot.Dispose(); Assert.Throws<ObjectDisposedException>(() => op.CaptureInputs(new EvaluationContext(), _ => throw new Exception("Must not run"), 16, 16)); return 0; });
    }

    /// <summary>Captured inputs cannot be transferred to another snapshot or supplied unbound.</summary>
    [Fact]
    public void RefusesForeignOrUnboundCapturedInputs()
    {
        var issuer = new Issuer(); using var session = Compile("1", issuer);
        using var first = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => { });
        using var second = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => { });
        var caller = new EvaluationContext();
        var captured = session.Run(session.Environment, first, op => op.CaptureInputs(caller, _ => false, 16, 16));
        session.Run(session.Environment, second, op =>
        {
            Assert.Throws<ArgumentException>(() => op.EvaluateAtomic(captured, 16));
            Assert.Throws<ArgumentException>(() => op.EvaluateAtomic(caller.CaptureSchemaInputs(_ => false, 16, 16), 16));
            return 0;
        });
    }

    /// <summary>Retains both visitor and deferred owner-cleanup failures.</summary>
    [Fact]
    public void VisitorAndCleanupFailuresAreRetained()
    {
        var issuer = new Issuer(); using var session = Compile("1", issuer); var snapshot = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => throw new ApplicationException("cleanup"));
        var primary = new InvalidOperationException("visitor");
        var error = Assert.Throws<AggregateException>(() => session.Run<int>(session.Environment, snapshot, op => { var result = op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext(), _ => false, 16, 16), 16); result.ForEachAtomic(_ => { snapshot.Dispose(); throw primary; }); return 0; }));
        Assert.Same(primary, error.InnerExceptions[0]); Assert.Equal("cleanup", error.InnerExceptions[1].Message);
    }
}
