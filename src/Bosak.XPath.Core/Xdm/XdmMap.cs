// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : An XDM map value: a collection of key-value pairs where keys are atomic XDM values.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 21-05-2026     | Keys changed from string to XdmValue with numeric promotion equality                   |
//                      | Charles Korthout | 0.3   | 15-07-2026     | Added Remove(key) for fn:parse-json duplicates='use-last' entry replacement            |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// An XDM map value: a collection of key-value pairs where keys are atomic XDM values.
/// </summary>
public sealed class XdmMap
{
    private readonly Dictionary<XdmValue, XdmValue> _entries;

    /// <summary>Creates an empty map.</summary>
    public XdmMap() => _entries = new Dictionary<XdmValue, XdmValue>(XdmValueEqualityComparer.Instance);

    /// <summary>Creates a map from an existing sequence of key-value pairs.</summary>
    public XdmMap(IEnumerable<KeyValuePair<XdmValue, XdmValue>> entries)
        => _entries = new Dictionary<XdmValue, XdmValue>(entries, XdmValueEqualityComparer.Instance);

    /// <summary>Adds or replaces a key-value pair.</summary>
    public void Add(XdmValue key, XdmValue value) => _entries[key] = value;

    /// <summary>Removes the entry with the given key, if present.</summary>
    public bool Remove(XdmValue key) => _entries.Remove(key);

    /// <summary>Attempts to retrieve the value for the given key.</summary>
    public bool TryGetValue(XdmValue key, out XdmValue value) => _entries.TryGetValue(key, out value!);

    /// <summary>Returns whether the map contains the given key.</summary>
    public bool ContainsKey(XdmValue key) => _entries.ContainsKey(key);

    /// <summary>Returns the number of entries in the map.</summary>
    public int Count => _entries.Count;

    /// <summary>Returns all keys in the map.</summary>
    public IEnumerable<XdmValue> Keys => _entries.Keys;

    /// <summary>Returns all values in the map.</summary>
    public IEnumerable<XdmValue> Values => _entries.Values;

    /// <summary>Returns all key-value pairs in the map.</summary>
    public IEnumerable<KeyValuePair<XdmValue, XdmValue>> Entries => _entries;
}
