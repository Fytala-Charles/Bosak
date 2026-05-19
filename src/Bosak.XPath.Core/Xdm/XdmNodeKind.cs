// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : The seven node kinds in the XQuery Data Model
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
/// The seven node kinds in the XQuery Data Model.
/// </summary>
[Flags]
public enum XdmNodeKind : byte
{
    None = 0,
    Document = 1,
    Element = 2,
    Attribute = 4,
    Text = 8,
    Comment = 16,
    ProcessingInstruction = 32,
    Namespace = 64,
    
    All = Document | Element | Attribute | Text | Comment | ProcessingInstruction | Namespace
}
