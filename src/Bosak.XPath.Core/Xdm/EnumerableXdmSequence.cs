// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 09 september 2026
// PURPOSE              : Lazy IEnumerable-backed XDM sequence (fresh enumerator per enumeration, no up-front materialization)
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 09-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// An <see cref="IXdmSequence"/> backed by a lazy <see cref="IEnumerable{T}"/>:
/// each enumeration creates a fresh iterator, so no intermediate list is materialized.
/// Used by axis implementations where the consumer typically enumerates once
/// (child/descendant/attribute axes on path-heavy evaluations).
/// </summary>
public sealed class EnumerableXdmSequence : IXdmSequence
{
    private readonly IEnumerable<XdmValue> _items;

    /// <summary>Wraps a lazily evaluated item source; each enumeration re-iterates it.</summary>
    /// <param name="items">The deferred item source.</param>
    public EnumerableXdmSequence(IEnumerable<XdmValue> items)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
    }

    /// <summary>Returns false: the length of a lazy sequence is not known without enumeration.</summary>
    /// <param name="length">Always 0 when returning false.</param>
    /// <returns>Always <c>false</c>.</returns>
    public bool TryGetLength(out int length)
    {
        length = 0;
        return false;
    }

    /// <summary>Returns a fresh enumerator over the deferred source.</summary>
    /// <returns>A new enumerator.</returns>
    public IXdmSequenceEnumerator GetEnumerator() => new Enumerator(_items.GetEnumerator());

    private sealed class Enumerator : IXdmSequenceEnumerator
    {
        private readonly IEnumerator<XdmValue> _inner;

        public Enumerator(IEnumerator<XdmValue> inner) => _inner = inner;

        /// <inheritdoc/>
        public XdmValue Current => _inner.Current;

        /// <inheritdoc/>
        public bool MoveNext() => _inner.MoveNext();
    }
}
