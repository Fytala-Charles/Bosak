// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Retains provider admissions through actual in-flight calls and raw completion.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
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
//                      | Charles Korthout | 0.2 | 05-11-2026     | Guard source-free construction and operation-local capture (1.66.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3 | 05-11-2026     | Add immutable paired membership with guarded rollback (1.67.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4 | 05-11-2026     | Serialize exact host calls and retain real raw tails without fake admissions (1.71.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5 | 06-11-2026     | Preserve separate work and ordered cleanup reports through real call tails (1.74.0). |
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Runtime.ExceptionServices;
using Bosak.XPath.Core.Schema;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>Serializes optional provider work and retains its real admission through raw completion.</summary>
internal sealed class QueryProviderOperationScope(Action checkRaw, CancellationToken cancellation)
{
    private readonly object _gate = new();
    private bool _closed;
    private bool _busy;
    private IQueryOperationLifetime? _provider;
    private IQueryOperationLease? _admission;
    private IQueryOperationLifetime? _target;
    private IQueryOperationLease? _targetAdmission;
    private object? _host;
    private bool _paired;
    private bool _admissionFailed;
    private TaskCompletionSource? _idle;
    private TaskCompletionSource<Completion>? _completion;
    private TaskCompletionSource? _throwingCompletion;

    internal void CheckActive()
    {
        lock (_gate) ObjectDisposedException.ThrowIf(_closed, this);
        cancellation.ThrowIfCancellationRequested();
    }

    // Constructor callers keep immutable single membership; only the explicit engine pair path selects two.
    internal T Call<T>(IQueryOperationLifetime provider, Func<Action, T> work, Action<Action>? beforeEnter = null)
        => CallCore(provider, null, false, work, beforeEnter);

    internal T CallPair<T>(IQueryOperationLifetime source, IQueryOperationLifetime target,
        Func<Action, T> work, Action<Action>? beforeEnter = null)
    {
        ArgumentNullException.ThrowIfNull(target);
        return CallCore(source, ReferenceEquals(source, target) ? null : target, true, work, beforeEnter);
    }

    // Host work uses the real raw/source leases; never fabricate an optional provider admission.
    internal T CallHost<T>(object host,Func<Action,T> work)
    {
        ArgumentNullException.ThrowIfNull(host);ArgumentNullException.ThrowIfNull(work);
        lock(_gate)
        {
            ObjectDisposedException.ThrowIf(_closed,this);cancellation.ThrowIfCancellationRequested();
            if(_busy)throw new InvalidOperationException("Overlapping host work is not supported within one operation.");
            if(_provider is not null||_host is not null&&!ReferenceEquals(_host,host))throw new ArgumentException("Host executable membership changed.");
            _host=host;_busy=true;_idle=new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        try{Check();var result=work(Check);Check();return result;}
        finally{TaskCompletionSource idle;lock(_gate){_busy=false;idle=_idle!;}idle.TrySetResult();}
    }

    // No provider delegate, admission check or release runs under _gate.
    private T CallCore<T>(IQueryOperationLifetime provider, IQueryOperationLifetime? target, bool paired,
        Func<Action, T> work, Action<Action>? beforeEnter)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(work);
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_closed, this);
            cancellation.ThrowIfCancellationRequested();
            if (_busy) throw new InvalidOperationException("Overlapping provider work is not supported within one operation.");
            if (_provider is not null && (_paired != paired || !ReferenceEquals(_provider, provider) || !ReferenceEquals(_target, target)))
                throw new ArgumentException("Provider membership belongs to another operation profile or advisor pair.", nameof(provider));
            if (_host is not null)throw new ArgumentException("A host operation cannot change into provider execution.");
            if (_admissionFailed) throw new InvalidOperationException("The failed provider pair cannot be reopened within this raw operation.");
            _provider = provider; _target = target; _paired = paired;
            _busy = true;
            _idle = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        bool acquiring = paired && _admission is null;
        bool acquired = false;
        try
        {
            Check();
            beforeEnter?.Invoke(Check);
            Check();
            _admission ??= provider.Enter(cancellation)
                ?? throw new InvalidOperationException("Provider returned no operation admission.");
            Check();
            if (target is not null)
            {
                _targetAdmission ??= target.Enter(cancellation)
                    ?? throw new InvalidOperationException("Target provider returned no operation admission.");
                Check();
            }
            acquired = true;
            var result = work(Check);
            Check();
            return result;
        }
        catch (Exception primary) when (acquiring && !acquired)
        {
            lock (_gate) _admissionFailed = true;
            var failures = new List<Exception> { primary };
            ReleaseAdmissions(failures);
            if (failures.Count > 1) throw new AggregateException("Provider pair entry and rollback failures.", failures);
            throw;
        }
        finally
        {
            TaskCompletionSource idle;
            lock (_gate) { _busy = false; idle = _idle!; }
            idle.TrySetResult();
        }
    }

    private void Boundary() { CheckActive(); checkRaw(); CheckActive(); }
    private void Check()
    {
        Boundary();
        _admission?.ThrowIfUnavailable(cancellation);
        Boundary();
        _targetAdmission?.ThrowIfUnavailable(cancellation);
        Boundary();
    }

    private void ReleaseAdmissions(List<Exception> errors)
    {
        // Null before callbacks: every successful obligation has one cleanup owner even if cleanup throws.
        var target = _targetAdmission; _targetAdmission = null;
        var source = _admission; _admission = null;
        try { target?.Dispose(); } catch (Exception error) { errors.Add(error); }
        try { source?.Dispose(); } catch (Exception error) { errors.Add(error); }
    }

    internal sealed class Completion(Exception? primary, ImmutableArray<Exception> cleanupErrors)
    {
        internal Exception? Primary { get; } = primary;
        internal ImmutableArray<Exception> CleanupErrors { get; } = cleanupErrors;

        internal void ThrowIfFailed()
        {
            if (Primary is null && CleanupErrors.IsEmpty) return;
            if (Primary is not null && CleanupErrors.IsEmpty) ExceptionDispatchInfo.Capture(Primary).Throw();
            if (Primary is null && CleanupErrors.Length == 1) ExceptionDispatchInfo.Capture(CleanupErrors[0]).Throw();
            throw new AggregateException("Operation work and admission cleanup failures.",
                Primary is null ? CleanupErrors : new[] { Primary }.Concat(CleanupErrors));
        }
    }

    internal Task CompleteAsync(Func<List<Exception>> releaseRaw, Exception? primary = null)
    {
        var report = CompleteOutcomeAsync(releaseRaw, primary);
        TaskCompletionSource completion;
        lock (_gate)
        {
            if (_throwingCompletion is not null) return _throwingCompletion.Task;
            completion = _throwingCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        _ = SetThrowingCompletionAsync(report, completion);
        return completion.Task;
    }

    private static async Task SetThrowingCompletionAsync(Task<Completion> report, TaskCompletionSource completion)
    {
        try
        {
            var outcome = await report.ConfigureAwait(false);
            outcome.ThrowIfFailed();
            completion.TrySetResult();
        }
        catch (Exception error) { completion.TrySetException(error); }
    }

    internal Task<Completion> CompleteOutcomeAsync(Func<List<Exception>> releaseRaw, Exception? primary = null)
    {
        ArgumentNullException.ThrowIfNull(releaseRaw);
        Task wait;
        TaskCompletionSource<Completion> completion;
        lock (_gate)
        {
            if (_completion is not null) return _completion.Task;
            _closed = true;
            completion = _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            wait = _busy ? _idle!.Task : Task.CompletedTask;
        }
        // The shared task returns data, so observing a report never leaves an unobserved faulted task.
        _ = ReleaseAsync(wait, releaseRaw, primary, completion);
        return completion.Task;
    }

    private async Task ReleaseAsync(Task wait, Func<List<Exception>> releaseRaw, Exception? primary,
        TaskCompletionSource<Completion> completion)
    {
        await wait.ConfigureAwait(false);
        List<Exception> errors = [];
        ReleaseAdmissions(errors);
        try { errors.AddRange(releaseRaw()); }
        catch (Exception error) { errors.Add(error); }
        completion.TrySetResult(new Completion(primary, [.. errors]));
    }
}
