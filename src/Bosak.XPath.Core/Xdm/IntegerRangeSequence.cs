// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 mei 2026
// PURPOSE              : A lazy IXdmSequence representing an inclusive range of xs:integer values.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 24-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// A lazy sequence representing an inclusive integer range (<c>from to to</c>).
/// Avoids materialising the entire range into memory.
/// </summary>
public sealed class IntegerRangeSequence : IXdmSequence
{
    private readonly long _from;
    private readonly long _to;

    /// <summary>Creates a lazy inclusive range from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public IntegerRangeSequence(long from, long to)
    {
        _from = from;
        _to = to;
    }

    /// <summary>The inclusive start value of the range.</summary>
    public long From => _from;

    /// <summary>The inclusive end value of the range.</summary>
    public long To => _to;

    /// <summary>Returns the item count without materializing; false when the range exceeds <see cref="int.MaxValue"/>.</summary>
    public bool TryGetLength(out int length)
    {
        long count = _to - _from + 1;
        if (count > int.MaxValue)
        {
            length = 0;
            return false;
        }
        length = (int)count;
        return true;
    }

    /// <summary>Returns an enumerator that produces the range values lazily.</summary>
    public IXdmSequenceEnumerator GetEnumerator() => new RangeEnumerator(_from, _to);

    private sealed class RangeEnumerator : IXdmSequenceEnumerator
    {
        private readonly long _to;
        private long _current;
        private bool _started;

        public RangeEnumerator(long from, long to)
        {
            _current = from;
            _to = to;
            _started = false;
        }

        public XdmValue Current => XdmValue.FromInteger(_current);

        public bool MoveNext()
        {
            if (!_started)
            {
                _started = true;
                return _current <= _to;
            }

            if (_current >= _to)
                return false;

            _current++;
            return true;
        }
    }
}
