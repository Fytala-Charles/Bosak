// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : A lazy, struct-backed XDM sequence with zero-allocation enumeration
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
/// A lazy, struct-backed XDM sequence with zero-allocation enumeration.
/// </summary>
public readonly struct XdmSequence
{
    /// <summary>Sentinel for empty sequences.</summary>
    private static readonly IXdmSequence EmptySequenceInstance = new EmptyXdmSequence();

    private readonly IXdmSequence? _source;

    private XdmSequence(IXdmSequence? source)
    {
        _source = source;
    }

    /// <summary>Returns the underlying sequence source, if any.</summary>
    internal IXdmSequence? Source => _source;

    /// <summary>Creates a sequence from an internal sequence source.</summary>
    public static XdmSequence FromSource(IXdmSequence source) => new(source);

    /// <summary>An empty sequence (singleton, no allocation).</summary>
    public static XdmSequence Empty => new(EmptySequenceInstance);

    /// <summary>Creates a singleton sequence from a single value.</summary>
    public static XdmSequence Singleton(XdmValue value) => new(new SingletonXdmSequence(value));

    /// <summary>Attempts to get the length without materializing the sequence.</summary>
    public bool TryGetLength(out int length)
    {
        if (_source is null)
        {
            length = 0;
            return true;
        }
        return _source.TryGetLength(out length);
    }

    /// <summary>
    /// Gets a struct enumerator for zero-allocation foreach.
    /// </summary>
    public Enumerator GetEnumerator() => new(_source);

    /// <summary>Struct enumerator avoiding boxing.</summary>
    public struct Enumerator
    {
        private readonly IXdmSequence? _source;
        private IXdmSequenceEnumerator? _enumerator;
        private XdmValue _current;
        private bool _started;

        internal Enumerator(IXdmSequence? source)
        {
            _source = source;
            _enumerator = null;
            _current = default;
            _started = false;
        }

        public XdmValue Current => _current;

        public bool MoveNext()
        {
            if (!_started)
            {
                _started = true;
                if (_source is null)
                    return false;
                _enumerator = _source.GetEnumerator();
            }

            if (_enumerator is null)
                return false;

            if (_enumerator.MoveNext())
            {
                _current = _enumerator.Current;
                return true;
            }

            return false;
        }
    }

    // ------------------------------------------------------------------
    // Internal sequence implementations
    // ------------------------------------------------------------------

    private sealed class EmptyXdmSequence : IXdmSequence
    {
        public bool TryGetLength(out int length)
        {
            length = 0;
            return true;
        }

        public IXdmSequenceEnumerator GetEnumerator() => EmptyEnumerator.Instance;
    }

    private sealed class EmptyEnumerator : IXdmSequenceEnumerator
    {
        public static readonly EmptyEnumerator Instance = new();
        public XdmValue Current => default;
        public bool MoveNext() => false;
    }

    private sealed class SingletonXdmSequence : IXdmSequence
    {
        private readonly XdmValue _value;
        public SingletonXdmSequence(XdmValue value) => _value = value;
        public bool TryGetLength(out int length) { length = 1; return true; }
        public IXdmSequenceEnumerator GetEnumerator() => new SingletonEnumerator(_value);
    }

    private sealed class SingletonEnumerator : IXdmSequenceEnumerator
    {
        private readonly XdmValue _value;
        private bool _done;
        public SingletonEnumerator(XdmValue value) { _value = value; }
        public XdmValue Current => _value;
        public bool MoveNext()
        {
            if (_done) return false;
            _done = true;
            return true;
        }
    }
}
