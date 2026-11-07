// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 November 2026
// PURPOSE              : Retains a bounded immutable operation lease set and preserves rollback failures.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
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
//                      | Charles Korthout | 0.2   | 04-11-2026     | Expose engine-only active-state check for callback-free registration (1.54.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 05-11-2026     | Preserve exact cleanup failures for deferred provider completion (1.65.0). |
//                      | Charles Korthout | 0.4   | 05-11-2026     | Separate admission and original ordered rollback errors (1.73.0). |
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Runtime.ExceptionServices;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>Private engine-owned operation leases; provider resources remain host-owned.</summary>
internal sealed class SchemaOperationLeaseSet
{
    /// <summary>Trusted engine adapter over one admission; callbacks never acquire provider resources.</summary>
    internal sealed class Member(object session, Action check, Func<IDisposable> enter)
    {
        internal object Session { get; } = session ?? throw new ArgumentNullException(nameof(session));
        internal Action Check { get; } = check ?? throw new ArgumentNullException(nameof(check));
        internal Func<IDisposable> Enter { get; } = enter ?? throw new ArgumentNullException(nameof(enter));
    }

    private readonly Member[] _members;
    private readonly Action _checkSession;
    private readonly CancellationToken _cancellation;
    private IDisposable[]? _leases;

    private SchemaOperationLeaseSet(Member[] members, IDisposable[] leases, Action checkSession, CancellationToken cancellation)
    { _members = members; _leases = leases; _checkSession = checkSession; _cancellation = cancellation; }

    // A report is engine-owned data; provider aggregate boundaries remain intact.
    internal sealed class Acquisition(SchemaOperationLeaseSet? leases, Exception? admissionError,
        ImmutableArray<Exception> cleanupErrors)
    {
        internal SchemaOperationLeaseSet? Leases { get; } = leases;
        internal Exception? AdmissionError { get; } = admissionError;
        internal ImmutableArray<Exception> CleanupErrors { get; } = cleanupErrors;
    }

    internal static SchemaOperationLeaseSet Acquire(object session, ImmutableArray<Member> members,
        int maxMembers, Action checkSession, CancellationToken cancellation = default)
    {
        var outcome = AcquireOutcome(session, members, maxMembers, checkSession, cancellation);
        if (outcome.AdmissionError is null) return outcome.Leases!;
        if (!outcome.CleanupErrors.IsEmpty)
            throw new AggregateException("Operation entry/work and lease cleanup failures.",
                new[] { outcome.AdmissionError }.Concat(outcome.CleanupErrors));
        ExceptionDispatchInfo.Capture(outcome.AdmissionError).Throw();
        throw new InvalidOperationException("Unreachable admission outcome.");
    }

    internal static Acquisition AcquireOutcome(object session, ImmutableArray<Member> members,
        int maxMembers, Action checkSession, CancellationToken cancellation = default)
    {
        IDisposable[] leases = [];
        int acquired = 0;
        try
        {
            ArgumentNullException.ThrowIfNull(session);
            ArgumentNullException.ThrowIfNull(checkSession);
            if (maxMembers <= 0) throw new ArgumentOutOfRangeException(nameof(maxMembers));
            if (members.IsDefault) throw new ArgumentException("Explicit membership is required.", nameof(members));
            if (members.Length > maxMembers) throw new ArgumentException("Operation membership limit exceeded.", nameof(members));
            var copied = members.ToArray();
            var identities = new HashSet<Member>(ReferenceEqualityComparer.Instance);
            foreach (var member in copied)
                if (member is null || !ReferenceEquals(member.Session, session) || !identities.Add(member))
                    throw new ArgumentException("Members must be unique admissions from this session.", nameof(members));
            cancellation.ThrowIfCancellationRequested();
            checkSession();
            leases = new IDisposable[copied.Length];
            foreach (var member in copied)
            {
                cancellation.ThrowIfCancellationRequested();
                checkSession();
                member.Check();
                leases[acquired] = member.Enter() ?? throw new InvalidOperationException("Admission returned no operation lease.");
                acquired++;
            }
            var result = new SchemaOperationLeaseSet(copied, leases, checkSession, cancellation);
            result.Check();
            return new Acquisition(result, null, []);
        }
        catch (Exception primary)
        {
            return new Acquisition(null, primary, [.. CollectFailures(leases, acquired)]);
        }
    }

    // Deliberately no provider/session delegates: safe under an internal registration lock.
    internal void CheckActive()
    { ObjectDisposedException.ThrowIf(Volatile.Read(ref _leases) is null,this);_cancellation.ThrowIfCancellationRequested(); }

    internal void Check()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _leases) is null, this);
        _cancellation.ThrowIfCancellationRequested();
        _checkSession();
        foreach (var member in _members) member.Check();
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _leases) is null, this);
    }

    // Completion belongs to the raw callback/task, never to a canceled wait.
    internal void Complete(Exception? primary = null)
    {
        var leases = Interlocked.Exchange(ref _leases, null);
        if (leases is null) return;
        Release(leases, leases.Length, primary);
    }

    internal List<Exception> CompleteFailures()
    {
        var leases = Interlocked.Exchange(ref _leases, null);
        return leases is null ? [] : CollectFailures(leases, leases.Length);
    }

    private static List<Exception> CollectFailures(IDisposable[] leases, int count)
    {
        List<Exception> failures = [];
        for (int i = count - 1; i >= 0; i--)
            try { leases[i].Dispose(); }
            catch (Exception error) { failures.Add(error); }
        return failures;
    }

    private static void Release(IDisposable[] leases, int count, Exception? primary)
    {
        var failures = CollectFailures(leases, count);
        if (failures.Count > 0)
        {
            if (primary is not null) failures.Insert(0, primary);
            if (primary is null && failures.Count == 1) ExceptionDispatchInfo.Capture(failures[0]).Throw();
            throw new AggregateException("Operation entry/work and lease cleanup failures.", failures);
        }
        if (primary is not null) ExceptionDispatchInfo.Capture(primary).Throw();
    }
}
