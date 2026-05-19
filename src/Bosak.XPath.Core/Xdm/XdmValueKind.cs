// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Discriminates the kind of an XDM value
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
/// Discriminates the kind of an XDM value.
/// </summary>
public enum XdmValueKind : byte
{
    Undefined = 0,
    
    // Atomic types
    String,
    Integer,
    Decimal,
    Double,
    Float,
    Boolean,
    Date,
    Time,
    DateTime,
    Duration,
    QName,
    Uri,
    Binary,
    
    // Complex types
    Node,
    Sequence,
    Function,
    Map,
    Array,
    
    // External opaque .NET object
    External
}
