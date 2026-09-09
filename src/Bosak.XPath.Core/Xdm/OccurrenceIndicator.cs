// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 May 2026
// PURPOSE              : Sequence type occurrence indicator for XPath type expressions.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 09-09-2026     | Moved into Bosak.XPath.Core.Xdm (namespace consistency, Beta API review)                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Core.Xdm;

/// <summary>Occurrence indicator of a sequence type (XPath 3.1 §2.5).</summary>
public enum OccurrenceIndicator : byte
{
    /// <summary>Exactly one item (no indicator).</summary>
    One = 0,
    /// <summary>Zero or one item (<c>?</c>).</summary>
    ZeroOrOne = 1,
    /// <summary>Zero or more items (<c>*</c>).</summary>
    ZeroOrMore = 2,
    /// <summary>One or more items (<c>+</c>).</summary>
    OneOrMore = 3
}
