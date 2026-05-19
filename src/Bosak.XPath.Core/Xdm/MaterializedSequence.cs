// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : A materialized (eager) XDM sequence backed by a list of values
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// A materialized (eager) XDM sequence backed by a list of values.
/// </summary>
public sealed class MaterializedSequence : IXdmSequence
{
    private readonly IReadOnlyList<XdmValue> _items;

    private MaterializedSequence(IReadOnlyList<XdmValue> items) => _items = items;

    /// <summary>Creates a materialized sequence from a list of values.</summary>
    public static XdmSequence FromList(IReadOnlyList<XdmValue> items)
        => XdmSequence.FromSource(new MaterializedSequence(items));

    /// <summary>Creates a materialized sequence from an array of values.</summary>
    public static XdmSequence FromArray(params XdmValue[] items)
        => XdmSequence.FromSource(new MaterializedSequence(items));

    /// <summary>Creates a materialized sequence from an enumerable.</summary>
    public static XdmSequence FromEnumerable(IEnumerable<XdmValue> items)
        => XdmSequence.FromSource(new MaterializedSequence(items.ToList()));

    public bool TryGetLength(out int length)
    {
        length = _items.Count;
        return true;
    }

    public IXdmSequenceEnumerator GetEnumerator() => new Enumerator(_items);

    private sealed class Enumerator : IXdmSequenceEnumerator
    {
        private readonly IReadOnlyList<XdmValue> _items;
        private int _index = -1;
        public Enumerator(IReadOnlyList<XdmValue> items) => _items = items;
        public XdmValue Current => _items[_index];
        public bool MoveNext() => ++_index < _items.Count;
    }
}
