// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 31 October 2026
// PURPOSE              : Checks immutable private context capture and raw evaluation operation lifetime.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 31-10-2026     | Creation: private foundation admission (1.35.0)                                          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Xml.Schema;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Checks private compiler-context admission and whole-operation ownership with the actual VM.</summary>
public class SchemaQuerySessionTests
{
    private sealed class Issuer : ISchemaEvidenceIssuer
    {
        internal readonly object Origin = new();
        internal int AssessmentCalls;
        bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar value) => false;
        bool ISchemaEvidenceIssuer.OwnsType(object type) => false;
        bool ISchemaEvidenceIssuer.IsSameType(object first, object second) => ReferenceEquals(first, second);
        bool ISchemaEvidenceIssuer.OwnsAssessment(object assessment) { AssessmentCalls++; return ReferenceEquals(assessment, Origin); }
    }
    private static SchemaQuerySession Compile(string source, CompileOptions options, Issuer issuer)
    {
        Assert.True(SchemaSessionCompilation.TryCompile(source, options, issuer, 2, 16, out var session, out var reason), reason);
        return session!;
    }
    private static XdmValue One(XdmValue value)
    {
        if (value.IsSequence) foreach (var item in XdmSequence.FromSource(value.SequenceValue!)) return item;
        return value;
    }
    private sealed class DeceptiveComparer : IEqualityComparer<string>
    {
        bool IEqualityComparer<string>.Equals(string? x, string? y) => StringComparer.OrdinalIgnoreCase.Equals(x, y);
        int IEqualityComparer<string>.GetHashCode(string value) => StringComparer.OrdinalIgnoreCase.GetHashCode(value);
        /// <summary>Deliberately lies about comparer identity to test private capture admission.</summary>
        /// <param name="obj">An object to claim as equivalent.</param>
        /// <returns>True for every object.</returns>
        public override bool Equals(object? obj) => true;
        /// <summary>Provides the adversarial object's hash.</summary>
        /// <returns>A fixed hash.</returns>
        public override int GetHashCode() => 0;
    }

    /// <summary>Only the private path freezes caller namespace maps; public compilation remains unchanged.</summary>
    [Fact]
    public void PrivateCompilationCapturesNamespaceBindings()
    {
        var namespaces = new Dictionary<string, string> { ["p"] = "urn:before" };
        var options = new CompileOptions { Namespaces = namespaces };
        const string expression = "namespace-uri-from-QName(xs:QName($name))";
        var issuer = new Issuer(); using var session = Compile(expression, options, issuer);
        var ordinary = XPath31Expression.Compile(expression, options);
        namespaces["p"] = "urn:after";
        using var snapshot = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => { });
        var result = session.Run(session.Environment, snapshot, op => One(op.Evaluate(new EvaluationContext().WithVariable("name", XdmValue.FromString("p:item")))));
        Assert.Equal("urn:before", result.ToString());
        Assert.Equal("urn:before", session.Environment.Namespaces["p"]);
        Assert.Equal("urn:after", One(ordinary.Evaluate(new EvaluationContext().WithVariable("name", XdmValue.FromString("p:item")))).ToString());
    }

    /// <summary>Inline namespaces are captured after actual compiler resolution.</summary>
    [Fact]
    public void CapturesEffectiveInlineNamespaceContext()
    {
        var issuer = new Issuer();
        using var session = Compile("declare namespace p = 'urn:inline'; namespace-uri-from-QName(xs:QName($name))",
            new CompileOptions { Compatibility = XPathCompatibility.XPath40, Namespaces = new Dictionary<string, string> { ["p"] = "urn:outer" } }, issuer);
        Assert.Equal("urn:inline", session.Environment.Namespaces["p"]);
        using var snapshot = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => { });
        Assert.Equal("urn:inline", session.Run(session.Environment, snapshot,
            op => One(op.Evaluate(new EvaluationContext().WithVariable("name", XdmValue.FromString("p:item"))))).ToString());
    }

    /// <summary>Descriptive equality does not authenticate another expression's environment.</summary>
    [Fact]
    public void EqualFingerprintsCannotForgeEnvironmentAdmission()
    {
        var issuer = new Issuer();
        using var first = Compile("1", new CompileOptions(), issuer);
        using var second = Compile("2", new CompileOptions(), issuer);
        Assert.Equal(first.Environment.Fingerprint, second.Environment.Fingerprint);
        Assert.NotSame(first.Environment, second.Environment);
        Assert.Throws<ArgumentException>(() => first.OpenSnapshot(second.Environment, new object(), issuer.Origin, () => { }));
        Assert.Equal(0, issuer.AssessmentCalls);
    }

    /// <summary>All supported compiler inputs contribute deterministically, independently of insertion order.</summary>
    [Fact]
    public void DescriptorTracksPrivateContextInputsAndNullBoundaries()
    {
        var issuer = new Issuer();
        using var a = Compile("1", new CompileOptions { Namespaces = new Dictionary<string, string> { ["b"] = "urn:b", ["a"] = "urn:a" } }, issuer);
        using var b = Compile("1", new CompileOptions { Namespaces = new Dictionary<string, string> { ["a"] = "urn:a", ["b"] = "urn:b" } }, issuer);
        Assert.Equal(a.Environment.Fingerprint, b.Environment.Fingerprint);
        var inputs = new[] {
            new CompileOptions(),
            new CompileOptions { Namespaces = ImmutableDictionary<string, string>.Empty },
            new CompileOptions { BaseUri = "urn:base" },
            new CompileOptions { BackwardsCompatible = true },
            new CompileOptions { StrictStaticTyping = true },
            new CompileOptions { Xml11LineEndings = true },
            new CompileOptions { EnableJit = true },
            new CompileOptions { DefaultElementNamespace = "urn:elements" },
            new CompileOptions { DefiningElementDefaultNamespace = "urn:defining" },
            new CompileOptions { Compatibility = XPathCompatibility.XPath40 },
            new CompileOptions { StaticVariableTypes = new Dictionary<string, string> { ["v"] = "xs:integer" } },
            new CompileOptions { EnumerationItemTypes = new Dictionary<string, string> { ["Q{urn:t}A"] = "enum('a')" } },
            new CompileOptions { ItemTypeAliases = new Dictionary<string, string> { ["Q{urn:t}A"] = "xs:string" } },
            new CompileOptions { NamedRecordTypeNames = new HashSet<string>() }
        };
        var fingerprints = new HashSet<string>(StringComparer.Ordinal);
        foreach (var options in inputs)
        {
            using var session = Compile("1", options, issuer);
            Assert.True(fingerprints.Add(session.Environment.Fingerprint));
        }
    }

    /// <summary>Unversioned callbacks, mutable schemas and altered comparer semantics fail closed privately.</summary>
    [Fact]
    public void RefusesUncapturableHostStateWithoutChangingOrdinaryCompile()
    {
        var issuer = new Issuer(); int callbackCalls = 0;
        var inputs = new[] {
            new CompileOptions { FunctionParameterNames = (_, _, _) => { callbackCalls++; return null; } },
            new CompileOptions { FunctionMinimumArity = (_, _, _) => { callbackCalls++; return null; } },
            new CompileOptions { SchemaSet = new XmlSchemaSet() },
            new CompileOptions { Namespaces = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) },
            new CompileOptions { Namespaces = new Dictionary<string, string>(new DeceptiveComparer()) },
            new CompileOptions { NamedRecordTypeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) },
            new CompileOptions { Namespaces = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(new Dictionary<string, string>()) }
        };
        foreach (var options in inputs)
        {
            Assert.False(SchemaSessionCompilation.TryCompile("1+1", options, issuer, 2, 16, out var session, out var reason));
            Assert.Null(session); Assert.NotNull(reason);
            Assert.Equal(2L, One(XPath31Expression.Compile("1+1", options).Evaluate(new EvaluationContext())).IntegerValue);
        }
        Assert.Equal(0, callbackCalls); Assert.Equal(0, issuer.AssessmentCalls);
    }

    /// <summary>The raw task keeps its admission through evaluation and draining despite a canceled waiter.</summary>
    /// <returns>The bounded asynchronous ownership regression.</returns>
    [Fact]
    public async Task CanceledWaitDoesNotDisposeRawEvaluationOperation()
    {
        var issuer = new Issuer(); using var session = Compile("($v,$v+1)", new CompileOptions(), issuer);
        int releases = 0;
        var snapshot = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => Interlocked.Increment(ref releases));
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        SchemaQuerySession.Operation? captured = null;
        var raw = session.RunAsync(session.Environment, snapshot, async op =>
        {
            captured = op;
            var value = op.Evaluate(new EvaluationContext().WithVariable("v", XdmValue.FromInteger(20)));
            var detached = new List<long>();
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!)) detached.Add(item.IntegerValue);
            entered.SetResult();
            await resume.Task;
            return detached.ToArray();
        });
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            using var cancel = new CancellationTokenSource(); cancel.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => raw.WaitAsync(cancel.Token));
            snapshot.Dispose();
            Assert.Equal(0, Volatile.Read(ref releases));
            Assert.Throws<ObjectDisposedException>(() => captured!.Evaluate(new EvaluationContext()));
        }
        finally { resume.TrySetResult(); }
        Assert.Equal(new long[] { 20, 21 }, await raw.WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.Equal(1, Volatile.Read(ref releases));
        Assert.Throws<ObjectDisposedException>(() => captured!.Evaluate(new EvaluationContext()));
    }

    /// <summary>Callback failure and cancellation release raw operation ownership.</summary>
    /// <returns>The completed exceptional operation checks.</returns>
    [Fact]
    public async Task RawCallbackFailureAndCancellationReleaseAdmissions()
    {
        var issuer = new Issuer(); using var session = Compile("1", new CompileOptions(), issuer);
        int releases = 0;
        var snapshot = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => releases++);
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.RunAsync<int>(session.Environment, snapshot,
            _ => Task.FromException<int>(new InvalidOperationException("raw failure"))));
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => session.RunAsync(session.Environment, snapshot, _ => Task.FromResult(1), cancel.Token));
        snapshot.Dispose(); Assert.Equal(1, releases);
    }

    /// <summary>Foreign snapshots and completed operation objects cannot invoke an evaluator.</summary>
    [Fact]
    public void RejectsForeignSnapshotsAndEscapedOperationObjects()
    {
        var issuer = new Issuer(); using var a = Compile("40+2", new CompileOptions(), issuer);
        using var b = Compile("1", new CompileOptions(), issuer);
        using var foreign = b.OpenSnapshot(b.Environment, new object(), issuer.Origin, () => { });
        Assert.Throws<ArgumentException>(() => a.Run(a.Environment, foreign, _ => 0));
        using var local = a.OpenSnapshot(a.Environment, new object(), issuer.Origin, () => { });
        SchemaQuerySession.Operation? escaped = null;
        Assert.Equal(42L, a.Run(a.Environment, local, op => { escaped = op; return One(op.Evaluate(new EvaluationContext())).IntegerValue; }));
        Assert.Throws<ObjectDisposedException>(() => escaped!.Evaluate(new EvaluationContext()));
    }

    /// <summary>Raw callback and owned cleanup errors are both reported in synchronous and asynchronous paths.</summary>
    /// <returns>The exceptional lifetime regression.</returns>
    [Fact]
    public async Task CallbackAndCleanupFailuresAreBothRetained()
    {
        var issuer = new Issuer(); using var session = Compile("1", new CompileOptions(), issuer);
        var primary = new InvalidOperationException("callback"); var cleanup = new ApplicationException("cleanup");
        var sync = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => throw cleanup);
        Assert.Equal(new Exception[] { primary, cleanup }, Assert.Throws<AggregateException>(() => session.Run<int>(session.Environment, sync,
            _ => { sync.Dispose(); throw primary; })).InnerExceptions);
        var asyncSnapshot = session.OpenSnapshot(session.Environment, new object(), issuer.Origin, () => throw cleanup);
        var failure = await Assert.ThrowsAsync<AggregateException>(() => session.RunAsync<int>(session.Environment, asyncSnapshot,
            _ => { asyncSnapshot.Dispose(); return Task.FromException<int>(primary); }));
        Assert.Equal(new Exception[] { primary, cleanup }, failure.InnerExceptions);
    }
}
