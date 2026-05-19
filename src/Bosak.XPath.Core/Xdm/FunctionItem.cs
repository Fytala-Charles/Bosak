// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Represents a function item in the XQuery Data Model.
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
/// Abstract base for XPath 3.1 function items.
/// </summary>
public abstract record FunctionItem
{
    public abstract int Arity { get; }
}

/// <summary>
/// A reference to a named standard or user-defined function.
/// </summary>
public sealed record NamedFunctionItem(string NamespaceUri, string LocalName, int ArityValue) : FunctionItem
{
    public override int Arity => ArityValue;
}

/// <summary>
/// A partially applied (curried) function.
/// </summary>
public sealed record CurriedFunctionItem(FunctionItem BaseFunction, XdmValue?[] FixedArgs) : FunctionItem
{
    public override int Arity
    {
        get
        {
            int placeholders = 0;
            foreach (var a in FixedArgs)
                if (a is null) placeholders++;
            return placeholders;
        }
    }
}
