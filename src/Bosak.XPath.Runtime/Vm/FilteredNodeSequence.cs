// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 November 2026
// PURPOSE              : Lazily filters node sequences using the existing XDM enumeration protocol
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 15-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>Lazily filters node sequences while preserving the existing XDM protocol.</summary>
internal class FilteredNodeSequence : IXdmSequence
{
    private readonly IXdmSequence _source;
    private readonly Func<IXdmNode, bool> _predicate;
    private FilteredNodeSequence(IXdmSequence source, Func<IXdmNode, bool> predicate)
    { _source = source; _predicate = predicate; }
    /// <inheritdoc/>
    public bool TryGetLength(out int length) { length = 0; return false; }
    /// <inheritdoc/>
    public virtual IXdmSequenceEnumerator GetEnumerator() => new FilterEnumerator(_source, _predicate);

    /// <summary>Creates a lazy filter retaining the source single-pass marker.</summary>
    internal static IXdmSequence Create(IXdmSequence source, Func<IXdmNode, bool> predicate)
        => source is ISinglePassSequence ? new MarkedFilter(source, predicate) : new FilteredNodeSequence(source, predicate);

    private sealed class MarkedFilter : FilteredNodeSequence, ISinglePassSequence
    {
        private int _taken;
        internal MarkedFilter(IXdmSequence source, Func<IXdmNode, bool> predicate) : base(source, predicate) { }
        /// <inheritdoc/>
        public override IXdmSequenceEnumerator GetEnumerator()
        {
            if (Interlocked.Exchange(ref _taken, 1) != 0)
                throw new StreamingException("Streaming: a streamed sequence is forward-only and has already been consumed. " +
                    "Restructure the expression so the streamed input is read in a single pass.");
            return base.GetEnumerator();
        }
    }

    private sealed class FilterEnumerator : IXdmSequenceEnumerator
    {
        private readonly IXdmSequence _source;
        private readonly Func<IXdmNode, bool> _predicate;
        private IXdmSequenceEnumerator? _inner;
        private XdmValue _current;
        private int _state;
        internal FilterEnumerator(IXdmSequence source, Func<IXdmNode, bool> predicate)
        { _source = source; _predicate = predicate; }
        /// <inheritdoc/>
        public XdmValue Current => _current;
        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (_state == 2) return false;
            try
            {
                if (_state == 0) { _state = 1; _inner = _source.GetEnumerator(); }
                while (_inner is not null && _inner.MoveNext())
                {
                    var item = _inner.Current;
                    if (item.IsNode && _predicate(item.NodeValue)) { _current = item; return true; }
                }
                _state = 2;
                return false;
            }
            catch { _state = 2; throw; }
        }
    }
}
