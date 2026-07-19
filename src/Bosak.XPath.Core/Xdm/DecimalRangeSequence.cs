// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 juli 2026
// PURPOSE              : A lazy IXdmSequence representing an inclusive range of xs:integer values too large to fit in long.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-07-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// A lazy sequence representing an inclusive integer range (<c>from to to</c>) whose
/// endpoints do not fit in <see cref="long"/>. The items are decimal-backed values
/// annotated with the <c>xs:integer</c> schema type.
/// </summary>
public sealed class DecimalRangeSequence : IXdmSequence
{
    private readonly decimal _from;
    private readonly decimal _to;

    public DecimalRangeSequence(decimal from, decimal to)
    {
        _from = from;
        _to = to;
    }

    /// <summary>The inclusive start value of the range.</summary>
    public decimal From => _from;

    /// <summary>The inclusive end value of the range.</summary>
    public decimal To => _to;

    public bool TryGetLength(out int length)
    {
        decimal count = _to - _from + 1;
        if (count > int.MaxValue)
        {
            length = 0;
            return false;
        }
        length = (int)count;
        return true;
    }

    public IXdmSequenceEnumerator GetEnumerator() => new RangeEnumerator(_from, _to);

    private sealed class RangeEnumerator : IXdmSequenceEnumerator
    {
        private readonly decimal _to;
        private decimal _current;
        private bool _started;

        public RangeEnumerator(decimal from, decimal to)
        {
            _current = from;
            _to = to;
            _started = false;
        }

        public XdmValue Current => XdmValue.FromDecimal(_current, "integer");

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
