// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : The seven node kinds in the XQuery Data Model
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// The seven node kinds in the XQuery Data Model.
/// </summary>
[Flags]
public enum XdmNodeKind : byte
{
    /// <summary>No node kind (matches nothing).</summary>
    None = 0,
    /// <summary>A document node.</summary>
    Document = 1,
    /// <summary>An element node.</summary>
    Element = 2,
    /// <summary>An attribute node.</summary>
    Attribute = 4,
    /// <summary>A text node.</summary>
    Text = 8,
    /// <summary>A comment node.</summary>
    Comment = 16,
    /// <summary>A processing-instruction node.</summary>
    ProcessingInstruction = 32,
    /// <summary>A namespace node.</summary>
    Namespace = 64,

    /// <summary>All seven node kinds combined.</summary>
    All = Document | Element | Attribute | Text | Comment | ProcessingInstruction | Namespace
}
