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
//                      | Charles Korthout | 0.2   | 31-05-2026     | Added IXsltMessageListener pass-through to TransformEngine                              |
//                      | Charles Korthout | 0.3   | 08-06-2026     | Added initialMode parameter to Transform/TransformToString                             |
//                      | Charles Korthout | 0.4   | 24-06-2026     | Added TransformFunction/TransformFunctionToString for xsl:function entry points        |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.Xslt.Api;

/// <summary>
/// Represents a compiled XSLT stylesheet ready for execution.
/// </summary>
public sealed class XsltExecutable
{
    private readonly Stylesheet.Stylesheet _stylesheet;
    private readonly IXsltMessageListener? _messageListener;

    internal XsltExecutable(Stylesheet.Stylesheet stylesheet, IXsltMessageListener? messageListener = null)
    {
        _stylesheet = stylesheet;
        _messageListener = messageListener;
    }

    /// <summary>
    /// Transforms the supplied source document using this stylesheet.
    /// </summary>
    /// <param name="source">The source document or node to transform. May be null for named-template entry points with no initial context item.</param>
    /// <param name="context">Optional evaluation context (variables, parameters, etc.).</param>
    /// <param name="initialTemplate">Optional name of the initial template to execute.</param>
    /// <param name="initialMode">Optional name of the initial mode to use.</param>
    /// <returns>The result of the transformation as an XDM value.</returns>
    public XdmValue Transform(IXdmNode? source, EvaluationContext? context = null, string? initialTemplate = null, string? initialMode = null)
    {
        var engine = new Runtime.TransformEngine(_stylesheet, context, _messageListener);
        return engine.Transform(source, initialTemplate, initialMode);
    }

    /// <summary>
    /// Transforms the supplied source document and serializes the result to a string.
    /// </summary>
    /// <param name="source">The source document or node to transform. May be null for named-template entry points with no initial context item.</param>
    /// <param name="context">Optional evaluation context.</param>
    /// <param name="initialTemplate">Optional name of the initial template to execute.</param>
    /// <param name="initialMode">Optional name of the initial mode to use.</param>
    /// <returns>The serialized result of the transformation.</returns>
    public string TransformToString(IXdmNode? source, EvaluationContext? context = null, string? initialTemplate = null, string? initialMode = null)
    {
        var result = Transform(source, context, initialTemplate, initialMode);
        return Runtime.ResultTreeSerializer.Serialize(result, _stylesheet.OutputProperties);
    }

    /// <summary>
    /// Invokes an <c>xsl:function</c> as the transformation entry point and returns its raw XDM value.
    /// </summary>
    /// <param name="name">The expanded function name (EQName form <c>Q{{uri}}local</c>).</param>
    /// <param name="args">Arguments to pass to the function.</param>
    /// <param name="context">Optional evaluation context.</param>
    /// <returns>The value returned by the function.</returns>
    public XdmValue TransformFunction(string name, XdmValue[] args, EvaluationContext? context = null)
    {
        var engine = new Runtime.TransformEngine(_stylesheet, context, _messageListener);
        return engine.TransformFunction(name, args);
    }

    /// <summary>
    /// Invokes an <c>xsl:function</c> as the transformation entry point and serializes the result to a string.
    /// </summary>
    /// <param name="name">The expanded function name (EQName form <c>Q{{uri}}local</c>).</param>
    /// <param name="args">Arguments to pass to the function.</param>
    /// <param name="context">Optional evaluation context.</param>
    /// <returns>The serialized result of the function call.</returns>
    public string TransformFunctionToString(string name, XdmValue[] args, EvaluationContext? context = null)
    {
        var result = TransformFunction(name, args, context);
        return Runtime.ResultTreeSerializer.Serialize(result, _stylesheet.OutputProperties);
    }
}
