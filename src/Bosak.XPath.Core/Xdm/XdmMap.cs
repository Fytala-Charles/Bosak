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
//                      | Charles Korthout | 0.4   | 15-07-2026     | Add replaces existing key object so the newest key (and its type annotation) survives  |
//                      | Charles Korthout | 0.5   | 17-07-2026     | Persistent ImmutableDictionary backing for O(log n) map updates (op-same-key)         |
//                      | Charles Korthout | 0.6   | 18-07-2026     | Preserve insertion order for Keys/Values/Entries; add WithAdded/WithRemoved helpers       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Collections.Immutable;
using System.Linq;

namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// An XDM map value: a collection of key-value pairs where keys are atomic XDM values.
/// </summary>
public sealed class XdmMap
{
    private ImmutableDictionary<XdmValue, XdmValue> _entries;
    private ImmutableList<XdmValue> _keyOrder;
    private ImmutableDictionary<XdmValue, int> _keyIndices;

    /// <summary>Creates an empty map.</summary>
    public XdmMap()
    {
        _entries = ImmutableDictionary.Create<XdmValue, XdmValue>(XdmValueEqualityComparer.Instance);
        _keyOrder = ImmutableList<XdmValue>.Empty;
        _keyIndices = ImmutableDictionary.Create<XdmValue, int>(XdmValueEqualityComparer.Instance);
    }

    /// <summary>Creates a map from an existing sequence of key-value pairs, preserving insertion order.</summary>
    public XdmMap(IEnumerable<KeyValuePair<XdmValue, XdmValue>> entries)
    {
        _entries = ImmutableDictionary.Create<XdmValue, XdmValue>(XdmValueEqualityComparer.Instance);
        _keyOrder = ImmutableList<XdmValue>.Empty;
        _keyIndices = ImmutableDictionary.Create<XdmValue, int>(XdmValueEqualityComparer.Instance);
        foreach (var kvp in entries)
            Add(kvp.Key, kvp.Value);
    }

    /// <summary>Creates a map that wraps an existing immutable dictionary in hash order.</summary>
    public XdmMap(ImmutableDictionary<XdmValue, XdmValue> entries)
    {
        _entries = entries;
        _keyOrder = ImmutableList<XdmValue>.Empty;
        _keyIndices = ImmutableDictionary.Create<XdmValue, int>(XdmValueEqualityComparer.Instance);
        int i = 0;
        foreach (var key in entries.Keys)
        {
            if (_keyIndices.ContainsKey(key))
                continue;
            _keyOrder = _keyOrder.Add(key);
            _keyIndices = _keyIndices.Add(key, i++);
        }
    }

    private XdmMap(ImmutableDictionary<XdmValue, XdmValue> entries, ImmutableList<XdmValue> keyOrder, ImmutableDictionary<XdmValue, int> keyIndices)
    {
        _entries = entries;
        _keyOrder = keyOrder;
        _keyIndices = keyIndices;
    }

    /// <summary>
    /// Adds or replaces a key-value pair. The stored key object is the supplied key, so
    /// the newest key (and its type annotation) survives, as required by op:same-key
    /// and map:merge use-last semantics.
    /// </summary>
    public void Add(XdmValue key, XdmValue value)
    {
        // ImmutableDictionary.SetItem with a custom equality comparer does not always
        // replace the stored key object, so remove first then re-add.
        if (_entries.ContainsKey(key))
        {
            int index = _keyIndices[key];
            _entries = _entries.Remove(key).Add(key, value);
            _keyOrder = _keyOrder.SetItem(index, key);
            _keyIndices = _keyIndices.Remove(key).Add(key, index);
        }
        else if (_keyIndices.ContainsKey(key))
        {
            // Key was previously removed; reuse its original slot so order is preserved.
            int index = _keyIndices[key];
            _entries = _entries.Add(key, value);
            _keyOrder = _keyOrder.SetItem(index, key);
            _keyIndices = _keyIndices.Remove(key).Add(key, index);
        }
        else
        {
            int index = _keyOrder.Count;
            _entries = _entries.Add(key, value);
            _keyOrder = _keyOrder.Add(key);
            _keyIndices = _keyIndices.Add(key, index);
        }
    }

    /// <summary>Removes the entry with the given key, if present.</summary>
    public bool Remove(XdmValue key)
    {
        bool contained = _entries.ContainsKey(key);
        _entries = _entries.Remove(key);
        // Leave _keyOrder and _keyIndices as tombstones so a later re-add keeps the original slot.
        return contained;
    }

    /// <summary>Returns a new map with the key added or replaced, preserving insertion order.</summary>
    public XdmMap WithAdded(XdmValue key, XdmValue value)
    {
        if (_entries.ContainsKey(key))
        {
            int index = _keyIndices[key];
            var newEntries = _entries.Remove(key).Add(key, value);
            var newKeyOrder = _keyOrder.SetItem(index, key);
            var newKeyIndices = _keyIndices.Remove(key).Add(key, index);
            return new XdmMap(newEntries, newKeyOrder, newKeyIndices);
        }
        else if (_keyIndices.ContainsKey(key))
        {
            int index = _keyIndices[key];
            var newEntries = _entries.Add(key, value);
            var newKeyOrder = _keyOrder.SetItem(index, key);
            var newKeyIndices = _keyIndices.Remove(key).Add(key, index);
            return new XdmMap(newEntries, newKeyOrder, newKeyIndices);
        }
        else
        {
            int index = _keyOrder.Count;
            var newEntries = _entries.Add(key, value);
            var newKeyOrder = _keyOrder.Add(key);
            var newKeyIndices = _keyIndices.Add(key, index);
            return new XdmMap(newEntries, newKeyOrder, newKeyIndices);
        }
    }

    /// <summary>Returns a new map with the key removed, preserving insertion order.</summary>
    public XdmMap WithRemoved(XdmValue key)
    {
        if (!_entries.ContainsKey(key))
            return this;
        return new XdmMap(_entries.Remove(key), _keyOrder, _keyIndices);
    }

    /// <summary>Attempts to retrieve the value for the given key.</summary>
    public bool TryGetValue(XdmValue key, out XdmValue value) => _entries.TryGetValue(key, out value!);

    /// <summary>Returns whether the map contains the given key.</summary>
    public bool ContainsKey(XdmValue key) => _entries.ContainsKey(key);

    /// <summary>Returns the number of entries in the map.</summary>
    public int Count => _entries.Count;

    /// <summary>Returns all keys in insertion order.</summary>
    public IEnumerable<XdmValue> Keys => _keyOrder.Where(k => _entries.ContainsKey(k));

    /// <summary>Returns all values in insertion order.</summary>
    public IEnumerable<XdmValue> Values => _keyOrder.Where(k => _entries.ContainsKey(k)).Select(k => _entries[k]);

    /// <summary>Returns all key-value pairs in insertion order.</summary>
    public IEnumerable<KeyValuePair<XdmValue, XdmValue>> Entries => _keyOrder
        .Where(k => _entries.ContainsKey(k))
        .Select(k => new KeyValuePair<XdmValue, XdmValue>(k, _entries[k]));
}
