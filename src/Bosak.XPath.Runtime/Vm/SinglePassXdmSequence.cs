// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Single-shot lazy sequence marking streamed (forward-only) VM results
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>
/// An <see cref="ISinglePassSequence"/> produced by the VM's lazy streaming paths (lazy
/// axis maps, filters, and path-step maps over a streamed input). Like the provider-side
/// single-pass sequences it composes with, it can be enumerated at most once; a second
/// enumeration throws so re-reads fail loudly instead of returning silently incomplete
/// data or re-pulling an exhausted stream.
/// </summary>
internal sealed class SinglePassXdmSequence : ISinglePassSequence
{
    private readonly IEnumerable<XdmValue> _pull;
    private int _taken;

    internal SinglePassXdmSequence(IEnumerable<XdmValue> pull)
        => _pull = pull;

    public bool TryGetLength(out int length)
    {
        length = 0;
        return false;
    }

    public IXdmSequenceEnumerator GetEnumerator()
    {
        if (Interlocked.Exchange(ref _taken, 1) != 0)
        {
            throw new InvalidOperationException(
                "Streaming: a streamed sequence is forward-only and has already been consumed. " +
                "Restructure the expression so the streamed input is read in a single pass.");
        }
        return new Adapter(_pull.GetEnumerator());
    }

    private sealed class Adapter : IXdmSequenceEnumerator
    {
        private readonly IEnumerator<XdmValue> _inner;

        internal Adapter(IEnumerator<XdmValue> inner)
            => _inner = inner;

        public XdmValue Current => _inner.Current;

        public bool MoveNext() => _inner.MoveNext();
    }
}
