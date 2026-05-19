// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : An XDM array value: a ordered collection of XDM values with 1-based indexing.
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
/// An XDM array value: an ordered collection of XDM values with 1-based indexing.
/// </summary>
public sealed class XdmArray
{
    private readonly List<XdmValue> _items;

    /// <summary>Creates an empty array.</summary>
    public XdmArray() => _items = new List<XdmValue>();

    /// <summary>Creates an array from an existing sequence of values.</summary>
    public XdmArray(IEnumerable<XdmValue> items) => _items = new List<XdmValue>(items);

    /// <summary>Appends a value to the end of the array.</summary>
    public void Add(XdmValue value) => _items.Add(value);

    /// <summary>Retrieves the item at the given 1-based index, or Undefined if out of range.</summary>
    public XdmValue Get(int index)
    {
        if (index >= 1 && index <= _items.Count)
            return _items[index - 1];
        return XdmValue.Undefined;
    }

    /// <summary>Returns whether the array contains a value equal to the given value (by string comparison).</summary>
    public bool Contains(XdmValue value)
    {
        string target = AtomizedString(value);
        foreach (var item in _items)
        {
            if (AtomizedString(item) == target)
                return true;
        }
        return false;
    }

    /// <summary>Returns the number of items in the array.</summary>
    public int Count => _items.Count;

    /// <summary>Returns all items in the array.</summary>
    public IEnumerable<XdmValue> Values => _items;

    private static string AtomizedString(XdmValue value)
    {
        if (value.IsUndefined) return string.Empty;
        if (value.IsNode) return value.NodeValue.StringValue;
        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                return AtomizedString(item);
            return string.Empty;
        }
        return value.ToString();
    }
}
