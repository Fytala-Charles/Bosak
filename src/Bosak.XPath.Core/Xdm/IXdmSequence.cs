// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Internal interface for a lazily-evaluated XDM sequence
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
/// Internal interface for a lazily-evaluated XDM sequence.
/// </summary>
public interface IXdmSequence
{
    /// <summary>Attempts to get the length without materializing, if known.</summary>
    bool TryGetLength(out int length);

    /// <summary>Returns an enumerator over the sequence.</summary>
    IXdmSequenceEnumerator GetEnumerator();
}

/// <summary>
/// Enumerator for XDM sequences. Implementations should be structs where possible
/// to avoid boxing during foreach.
/// </summary>
public interface IXdmSequenceEnumerator
{
    XdmValue Current { get; }
    bool MoveNext();
}
