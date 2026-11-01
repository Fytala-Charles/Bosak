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

    internal static SchemaOperationLeaseSet Acquire(object session, ImmutableArray<Member> members,
        int maxMembers, Action checkSession, CancellationToken cancellation = default)
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
        var leases = new IDisposable[copied.Length];
        int acquired = 0;
        try
        {
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
            return result;
        }
        catch (Exception primary)
        {
            Release(leases, acquired, primary);
            throw;
        }
    }

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

    private static void Release(IDisposable[] leases, int count, Exception? primary)
    {
        List<Exception>? failures = null;
        for (int i = count - 1; i >= 0; i--)
            try { leases[i].Dispose(); }
            catch (Exception error) { (failures ??= []).Add(error); }
        if (failures is not null)
        {
            if (primary is not null) failures.Insert(0, primary);
            if (primary is null && failures.Count == 1) ExceptionDispatchInfo.Capture(failures[0]).Throw();
            throw new AggregateException("Operation entry/work and lease cleanup failures.", failures);
        }
        if (primary is not null) ExceptionDispatchInfo.Capture(primary).Throw();
    }
}
