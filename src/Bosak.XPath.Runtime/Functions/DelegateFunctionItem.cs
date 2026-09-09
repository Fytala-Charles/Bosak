// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 mei 2026
// PURPOSE              : Runtime wrapper for a C# delegate as an XPath function item.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 23-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.11  | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Runtime.Functions;

/// <summary>
/// A function item that wraps a C# delegate, used for dynamically created functions
/// such as the <c>next</c> and <c>permute</c> entries in <c>fn:random-number-generator</c>.
/// </summary>
/// <param name="ArityValue">The arity reported for the wrapped delegate.</param>
/// <param name="Implementation">The wrapped function implementation.</param>
public sealed record DelegateFunctionItem(int ArityValue, XPathFunction Implementation) : FunctionItem
{
    /// <inheritdoc/>
    public override int Arity => ArityValue;
}
