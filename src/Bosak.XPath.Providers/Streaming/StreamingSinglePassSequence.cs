// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Single-shot IXdmSequence that pulls XDM items from a forward-only stream
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Providers.Streaming;

/// <summary>
/// An <see cref="ISinglePassSequence"/> over a pull factory. The sequence can be
/// enumerated at most once; a second <see cref="GetEnumerator"/> throws a
/// <see cref="StreamingException"/> so re-reads fail loudly instead of returning
/// silently incomplete data.
/// </summary>
internal sealed class StreamingSinglePassSequence : ISinglePassSequence
{
    private readonly Func<IEnumerator<XdmValue>> _pull;
    private int _taken;

    internal StreamingSinglePassSequence(Func<IEnumerator<XdmValue>> pull)
        => _pull = pull;

    public bool TryGetLength(out int length)
    {
        length = 0;
        return false;
    }

    public IXdmSequenceEnumerator GetEnumerator()
    {
        if (Interlocked.Exchange(ref _taken, 1) != 0)
        {
            throw new StreamingException(
                "Streaming: a streamed sequence is forward-only and has already been consumed. " +
                "Restructure the expression so the streamed input is read in a single pass.");
        }
        return new Adapter(_pull());
    }

    private sealed class Adapter : IXdmSequenceEnumerator
    {
        private readonly IEnumerator<XdmValue> _inner;

        internal Adapter(IEnumerator<XdmValue> inner)
            => _inner = inner;

        public XdmValue Current => _inner.Current;

        public bool MoveNext() => _inner.MoveNext();
    }
}
