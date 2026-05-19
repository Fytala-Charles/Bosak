// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : An XDM map value: a collection of key-value pairs where keys are atomic strings.
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
/// An XDM map value: a collection of key-value pairs where keys are atomic strings.
/// </summary>
public sealed class XdmMap
{
    private readonly Dictionary<string, XdmValue> _entries;

    /// <summary>Creates an empty map.</summary>
    public XdmMap() => _entries = new Dictionary<string, XdmValue>();

    /// <summary>Creates a map from an existing sequence of key-value pairs.</summary>
    public XdmMap(IEnumerable<KeyValuePair<string, XdmValue>> entries)
        => _entries = new Dictionary<string, XdmValue>(entries);

    /// <summary>Adds or replaces a key-value pair.</summary>
    public void Add(string key, XdmValue value) => _entries[key] = value;

    /// <summary>Attempts to retrieve the value for the given key.</summary>
    public bool TryGetValue(string key, out XdmValue value) => _entries.TryGetValue(key, out value!);

    /// <summary>Returns whether the map contains the given key.</summary>
    public bool ContainsKey(string key) => _entries.ContainsKey(key);

    /// <summary>Returns the number of entries in the map.</summary>
    public int Count => _entries.Count;

    /// <summary>Returns all keys in the map.</summary>
    public IEnumerable<string> Keys => _entries.Keys;

    /// <summary>Returns all values in the map.</summary>
    public IEnumerable<XdmValue> Values => _entries.Values;
}
