// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Marker interface for a forward-only (single-pass, streamed) XDM sequence
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
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
namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// Marker interface for a sequence that pulls items from a forward-only stream
/// (for example, an <see cref="System.Xml.XmlReader"/>-backed streaming source).
/// </summary>
/// <remarks>
/// A single-pass sequence can be enumerated at most once; a second
/// <see cref="IXdmSequence.GetEnumerator"/> call throws. The engine treats marked
/// sequences specially: sequence normalization (duplicate removal and document-order
/// sorting) is skipped because the producer guarantees a duplicate-free sequence in
/// document order, and path-step / mapping opcodes avoid materializing the sequence
/// so that streamed items can be released as they are consumed.
/// </remarks>
public interface ISinglePassSequence : IXdmSequence
{
}
