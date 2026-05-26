// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 mei 2026
// PURPOSE              : An executable, thread-safe XSLT stylesheet that can transform source documents.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Xslt.Api;

/// <summary>
/// Represents a compiled XSLT stylesheet ready for execution.
/// </summary>
public sealed class XsltExecutable
{
    private readonly Stylesheet.Stylesheet _stylesheet;

    internal XsltExecutable(Stylesheet.Stylesheet stylesheet)
    {
        _stylesheet = stylesheet;
    }

    /// <summary>
    /// Transforms the supplied source document using this stylesheet.
    /// </summary>
    /// <param name="source">The source document or node to transform.</param>
    /// <param name="context">Optional evaluation context (variables, parameters, etc.).</param>
    /// <returns>The result of the transformation as an XDM value.</returns>
    public XdmValue Transform(IXdmNode source, EvaluationContext? context = null)
    {
        var engine = new Runtime.TransformEngine(_stylesheet, context);
        return engine.Transform(source);
    }

    /// <summary>
    /// Transforms the supplied source document and serializes the result to a string.
    /// </summary>
    /// <param name="source">The source document or node to transform.</param>
    /// <param name="context">Optional evaluation context.</param>
    /// <returns>The serialized result of the transformation.</returns>
    public string TransformToString(IXdmNode source, EvaluationContext? context = null)
    {
        var result = Transform(source, context);
        return Runtime.ResultTreeSerializer.Serialize(result, _stylesheet.OutputProperties);
    }
}
