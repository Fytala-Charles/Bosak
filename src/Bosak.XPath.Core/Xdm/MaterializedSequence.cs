// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : A materialized (eager) XDM sequence backed by a list of values
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 09-09-2026     | Added Items view (copy-free access for the VM's materialization fast path)             |
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

    /// <summary>Returns the materialized items without copying.</summary>
    public IReadOnlyList<XdmValue> Items => _items;

    /// <summary>Returns the item count; always known for a materialized sequence.</summary>
    public bool TryGetLength(out int length)
    {
        length = _items.Count;
        return true;
    }

    /// <summary>Returns an enumerator over the materialized items.</summary>
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
