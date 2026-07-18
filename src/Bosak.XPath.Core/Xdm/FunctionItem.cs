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
//                      | Charles Korthout | 0.2    | 14-07-2026     | NamedFunctionItem.DefiningContext for cross-context function items (fn:transform)      |
//                      | Charles Korthout | 0.3    | 18-07-2026     | Capture creation focus for context-dependent dynamic named-function calls            |
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

    /// <summary>
    /// The evaluation context in which the function item was materialized, if known.
    /// Used as a fallback for signature resolution when the function item crosses
    /// context boundaries (e.g. a function returned by fn:transform with
    /// delivery-format="raw" and invoked in the calling stylesheet). Typed as
    /// <see cref="object"/> to preserve project layering; the runtime casts it to
    /// its EvaluationContext.
    /// </summary>
    public object? DefiningContext { get; init; }

    /// <summary>
    /// The focus (context item, position, size) captured when the function item was
    /// materialized. Context-dependent functions invoked through this function item
    /// use this focus instead of the call-site focus.
    /// </summary>
    public XdmValue CapturedContextItem { get; init; } = XdmValue.Undefined;

    /// <summary>Captured context position from the materialization focus.</summary>
    public int CapturedContextPosition { get; init; }

    /// <summary>Captured context size from the materialization focus.</summary>
    public int CapturedContextSize { get; init; }
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
