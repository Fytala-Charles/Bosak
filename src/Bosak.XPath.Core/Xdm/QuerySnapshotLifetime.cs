// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Retains raw-operation leases and observes deferred snapshot cleanup.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-11-2026     | Creation; W1-A neutral admission (1.43.0)                                                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 03-11-2026     | Observe open-session idle epochs without revoking admissions (1.44.2).                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Core.Xdm;

/// <summary>Neutral revocation and exact-once deferred release, independent of schema authority.</summary>
internal sealed class QuerySnapshotLifetime(Action release) : IDisposable
{
    internal object SyncRoot { get; } = new();
    private Action? _release = release ?? throw new ArgumentNullException(nameof(release));
    private int _operations;
    private bool _closing, _finished;
    private Exception? _failure;
    private TaskCompletionSource<bool>? _drained, _idle;

    internal void Check() { lock (SyncRoot) ObjectDisposedException.ThrowIf(_closing, this); }
    internal IDisposable Enter()
    {
        lock (SyncRoot) { Check(); _operations++; return new Lease(this); }
    }
    internal ReadLease EnterRead()
    {
        lock (SyncRoot) { Check(); _operations++; return new ReadLease(this); }
    }
    internal ref struct ReadLease
    {
        private QuerySnapshotLifetime? _owner;
        internal ReadLease(QuerySnapshotLifetime owner) => _owner = owner;
        internal void Dispose() { var owner = _owner; _owner = null; owner?.Leave(); }
    }
    internal Task DrainAsync()
    {
        lock (SyncRoot)
        {
            if (_finished) return _failure is null ? Task.CompletedTask : Task.FromException(_failure);
            return (_drained ??= new(TaskCreationOptions.RunContinuationsAsynchronously)).Task;
        }
    }
    /// <summary>Observes the current busy epoch without revoking future admissions.</summary>
    internal Task WaitForIdleAsync()
    {
        lock (SyncRoot)
            return _operations == 0 ? Task.CompletedTask
                : (_idle ??= new(TaskCreationOptions.RunContinuationsAsynchronously)).Task;
    }
    private Action? TakeRelease()
    {
        if (!_closing || _operations != 0 || _release is null) return null;
        var action = _release; _release = null; return action;
    }
    private void Release(Action? action)
    {
        if (action is null) return;
        Exception? failure = null;
        try { action(); }
        catch (Exception error) { failure = error; throw; }
        finally
        {
            TaskCompletionSource<bool>? drained;
            lock (SyncRoot) { _failure = failure; _finished = true; drained = _drained; }
            if (failure is null) drained?.TrySetResult(true); else drained?.TrySetException(failure);
        }
    }
    /// <summary>Revokes access and releases after the last actual operation completes.</summary>
    public void Dispose()
    {
        Action? action;
        lock (SyncRoot) { _closing = true; action = TakeRelease(); }
        Release(action);
    }
    private void Leave()
    {
        Action? action;
        TaskCompletionSource<bool>? idle = null;
        lock (SyncRoot)
        {
            _operations--;
            if (_operations == 0) { idle = _idle; _idle = null; }
            action = TakeRelease();
        }
        try { Release(action); idle?.TrySetResult(true); }
        catch (Exception error) { idle?.TrySetException(error); throw; }
    }
    private sealed class Lease(QuerySnapshotLifetime owner) : IDisposable
    {
        private QuerySnapshotLifetime? _owner = owner;
        /// <summary>Releases this raw lease once.</summary>
        public void Dispose() => Interlocked.Exchange(ref _owner, null)?.Leave();
    }
}
