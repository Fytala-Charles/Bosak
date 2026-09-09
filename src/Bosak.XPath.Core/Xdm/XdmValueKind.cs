// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Discriminates the kind of an XDM value
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
/// Discriminates the kind of an XDM value.
/// </summary>
public enum XdmValueKind : byte
{
    /// <summary>No value (absent), e.g. for out-of-range lookups.</summary>
    Undefined = 0,

    // Atomic types
    /// <summary>An xs:string or string-derived atomic value.</summary>
    String,
    /// <summary>An xs:integer or integer-derived atomic value.</summary>
    Integer,
    /// <summary>An xs:decimal atomic value.</summary>
    Decimal,
    /// <summary>An xs:double atomic value.</summary>
    Double,
    /// <summary>An xs:float atomic value.</summary>
    Float,
    /// <summary>An xs:boolean atomic value.</summary>
    Boolean,
    /// <summary>An xs:date atomic value.</summary>
    Date,
    /// <summary>An xs:time atomic value.</summary>
    Time,
    /// <summary>An xs:dateTime atomic value (also carries gYear/gMonth-family annotations).</summary>
    DateTime,
    /// <summary>An xs:duration or duration-derived atomic value.</summary>
    Duration,
    /// <summary>An xs:QName atomic value.</summary>
    QName,
    /// <summary>An xs:anyURI atomic value.</summary>
    Uri,
    /// <summary>An xs:hexBinary or xs:base64Binary atomic value.</summary>
    Binary,

    // Complex types
    /// <summary>An XDM node.</summary>
    Node,
    /// <summary>A sequence of items.</summary>
    Sequence,
    /// <summary>A function item.</summary>
    Function,
    /// <summary>An XDM map (also a function item).</summary>
    Map,
    /// <summary>An XDM array (also a function item).</summary>
    Array,

    // External opaque .NET object
    /// <summary>An opaque external .NET object.</summary>
    External
}
