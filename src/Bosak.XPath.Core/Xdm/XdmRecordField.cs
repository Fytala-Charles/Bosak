// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 08 October 2026
// PURPOSE              : A single field declaration of a structural record type: a name plus its sequence type.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 08-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// A single field declaration within an XPath 4.0 structural record type: the field name
/// and the sequence type the field value must match (defaulting to <c>item()*</c>).
/// </summary>
public sealed class XdmRecordField
{
    /// <summary>
    /// Creates a record-field declaration.
    /// </summary>
    /// <param name="name">The field name (an NCName, or the contents of a string literal).</param>
    /// <param name="sequenceTypeText">The verbatim sequence-type text; <c>"item()"</c> when omitted in the declaration.</param>
    /// <param name="allowsEmpty">Whether the field type permits an empty sequence (its occurrence indicator allows zero).</param>
    public XdmRecordField(string name, string sequenceTypeText, bool allowsEmpty)
    {
        Name = name;
        SequenceTypeText = sequenceTypeText;
        AllowsEmpty = allowsEmpty;
    }

    /// <summary>Gets the field name (an NCName, or the contents of a string literal).</summary>
    public string Name { get; }

    /// <summary>Gets the verbatim sequence-type text for the field value, e.g. <c>xs:string</c> or <c>item()*</c>.</summary>
    public string SequenceTypeText { get; }

    /// <summary>Gets whether the field type permits an empty sequence (occurrence indicator allows zero).</summary>
    public bool AllowsEmpty { get; }
}
