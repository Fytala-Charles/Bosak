// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 November 2026
// PURPOSE              : Binds direct internal host evaluators for bounded private schema sessions.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 01-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2 | 01-11-2026     | Validate distinct XSLT roles against exact composite operation membership. |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XQuery.Api;

namespace Bosak.Xslt.Api;

/// <summary>Upper-layer direct bridges; callers retain owner/admission and static-capture duties.</summary>
internal static class SchemaHostExecution
{
    internal static Func<EvaluationContext, XdmValue> BindXQuery(XQueryExecutable executable)
    {
        ArgumentNullException.ThrowIfNull(executable);
        return executable.EvaluateIsolatedContext;
    }

    internal static Action<SchemaQuerySession.Operation, SchemaOperationInputs> BindXsltAdmission(IXdmNode? matchInput, IXdmNode? globalInput)
        => (operation, inputs) =>
        {
            operation.RequireNodeInput(inputs, matchInput);
            operation.RequireNodeInput(inputs, globalInput);
        };

    internal static Func<EvaluationContext, XdmValue> BindXslt(XsltExecutable executable,
        IXdmNode? matchInput, IXdmNode? globalInput, string? initialTemplate, Func<IXdmNode, bool> ownsNode)
    {
        ArgumentNullException.ThrowIfNull(executable);
        ArgumentNullException.ThrowIfNull(ownsNode);
        return context =>
        {
            // Establish membership before the transform reads any supplied node's getters.
            if ((matchInput is not null && !ownsNode(matchInput)) || (globalInput is not null && !ownsNode(globalInput)))
                throw new ArgumentException("Transform input belongs to another owner.");
            return executable.TransformWithExplicitContext(matchInput, globalInput, initialTemplate, context);
        };
    }
}
