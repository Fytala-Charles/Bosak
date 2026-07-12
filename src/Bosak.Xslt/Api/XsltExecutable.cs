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
//                      | Charles Korthout | 0.5   | 25-06-2026     | Added rawResult parameter to Transform for initial-template raw XDM output             |
//                      | Charles Korthout | 0.7   | 26-06-2026     | Added baseOutputUri parameter to Transform/TransformToString                            |
//                      | Charles Korthout | 0.6   | 26-06-2026     | Propagate TreatRecoverableAmbiguousMatchAsError to TransformEngine                      |
//                      | Charles Korthout | 0.8   | 06-07-2026     | Use xsl:result-document output properties in TransformToString                          |
//                      | Charles Korthout | 0.9   | 11-07-2026     | Read xsl:result-document output properties from fragment wrapper elements too.          |
//                      | Charles Korthout | 1.0   | 11-07-2026     | Resolve named character maps for principal and function output before serialization.    |
//                      | Charles Korthout | 1.1   | 11-07-2026     | Merge parameter-document character maps with named character-map resolutions.          |
//                      | Charles Korthout | 1.2   | 12-07-2026     | Use principal xsl:result-document output properties (including JSON) in TransformToString. |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using System.Threading;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.Xslt.Api;

/// <summary>
/// Represents a compiled XSLT stylesheet ready for execution.
/// </summary>
public sealed class XsltExecutable
{
    private readonly Stylesheet.Stylesheet _stylesheet;
    private readonly IXsltMessageListener? _messageListener;
    private readonly bool _treatRecoverableAmbiguousMatchAsError;

    internal XsltExecutable(Stylesheet.Stylesheet stylesheet, IXsltMessageListener? messageListener = null, bool treatRecoverableAmbiguousMatchAsError = false)
    {
        _stylesheet = stylesheet;
        _messageListener = messageListener;
        _treatRecoverableAmbiguousMatchAsError = treatRecoverableAmbiguousMatchAsError;
    }

    /// <summary>
    /// Gets the effective stylesheet-level output properties.
    /// </summary>
    public Stylesheet.OutputProperties OutputProperties => _stylesheet.OutputProperties ?? new Stylesheet.OutputProperties();

    /// <summary>
    /// Transforms the supplied source document using this stylesheet.
    /// </summary>
    /// <param name="source">The source document or node to transform. May be null for named-template entry points with no initial context item.</param>
    /// <param name="context">Optional evaluation context (variables, parameters, etc.).</param>
    /// <param name="initialTemplate">Optional name of the initial template to execute.</param>
    /// <param name="initialMode">Optional name of the initial mode to use.</param>
    /// <param name="baseOutputUri">The base output URI for the transformation; used by fn:current-output-uri().</param>
    /// <param name="rawResult">When true and an initial template is used, returns the raw template result instead of wrapping it in a result document.</param>
    /// <returns>The result of the transformation as an XDM value.</returns>
    /// <summary>
    /// Default stack size (bytes) allocated for the transformation thread.  A larger
    /// stack is required for stylesheets that rely on deep xsl:call-template recursion
    /// (for example the DocBook XSLT 1.0 stylesheets).
    /// </summary>
    private const int DefaultTransformStackSize = 4 * 1024 * 1024;

    public XdmValue Transform(IXdmNode? source, EvaluationContext? context = null, string? initialTemplate = null, string? initialMode = null, bool rawResult = false, string? baseOutputUri = null)
    {
        return RunWithStack(() =>
        {
            var engine = new Runtime.TransformEngine(_stylesheet, context, _messageListener, _treatRecoverableAmbiguousMatchAsError);
            return engine.Transform(source, initialTemplate, initialMode, rawResult, baseOutputUri);
        }, DefaultTransformStackSize);
    }

    /// <summary>
    /// Transforms the supplied source document and serializes the result to a string.
    /// </summary>
    /// <param name="source">The source document or node to transform. May be null for named-template entry points with no initial context item.</param>
    /// <param name="context">Optional evaluation context.</param>
    /// <param name="initialTemplate">Optional name of the initial template to execute.</param>
    /// <param name="initialMode">Optional name of the initial mode to use.</param>
    /// <param name="baseOutputUri">The base output URI for the transformation; used by fn:current-output-uri().</param>
    /// <returns>The serialized result of the transformation.</returns>
    public string TransformToString(IXdmNode? source, EvaluationContext? context = null, string? initialTemplate = null, string? initialMode = null, string? baseOutputUri = null)
    {
        return RunWithStack(() =>
        {
            var engine = new Runtime.TransformEngine(_stylesheet, context, _messageListener, _treatRecoverableAmbiguousMatchAsError);
            var result = engine.Transform(source, initialTemplate, initialMode, false, baseOutputUri);

            // A principal xsl:result-document (no href) supplies the effective output
            // properties, overriding the stylesheet-level xsl:output defaults.
            var outputProperties = engine.PrincipalResultDocumentProperties
                ?? _stylesheet.OutputProperties
                ?? new Stylesheet.OutputProperties();

            // Fall back to annotation-based output properties for backward compatibility
            // when the result tree is a wrapper element produced by the legacy path.
            if (engine.PrincipalResultDocumentProperties == null && result.IsNode && result.NodeValue is XDocumentNode xdn)
            {
                Stylesheet.OutputProperties? rdProps = null;
                if (xdn.UnderlyingObject is XDocument doc)
                    rdProps = doc.Annotation<Stylesheet.OutputProperties>();
                else if (xdn.UnderlyingObject is XElement elem)
                    rdProps = elem.Annotation<Stylesheet.OutputProperties>();
                if (rdProps != null)
                    outputProperties = rdProps;
            }

            // Resolve named character maps for the principal output if not already done.
            if (outputProperties.UseCharacterMaps.Count > 0)
            {
                outputProperties = outputProperties.Clone();
                var resolved = _stylesheet.ResolveCharacterMap(
                    outputProperties.UseCharacterMaps.Select(Stylesheet.Stylesheet.ExpandQName));
                if (outputProperties.CharacterMap != null)
                {
                    // Explicit named character maps override parameter-document defaults.
                    foreach (var kvp in outputProperties.CharacterMap)
                    {
                        if (!resolved.ContainsKey(kvp.Key))
                            resolved[kvp.Key] = kvp.Value;
                    }
                }
                outputProperties.CharacterMap = resolved;
            }

            return Runtime.ResultTreeSerializer.Serialize(result, outputProperties);
        }, DefaultTransformStackSize);
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
        return RunWithStack(() =>
        {
            var engine = new Runtime.TransformEngine(_stylesheet, context, _messageListener, _treatRecoverableAmbiguousMatchAsError);
            return engine.TransformFunction(name, args);
        }, DefaultTransformStackSize);
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
        var outputProperties = _stylesheet.OutputProperties ?? new Stylesheet.OutputProperties();
        if (outputProperties.UseCharacterMaps.Count > 0)
        {
            outputProperties = outputProperties.Clone();
            var resolved = _stylesheet.ResolveCharacterMap(
                outputProperties.UseCharacterMaps.Select(Stylesheet.Stylesheet.ExpandQName));
            if (outputProperties.CharacterMap != null)
            {
                foreach (var kvp in outputProperties.CharacterMap)
                {
                    if (!resolved.ContainsKey(kvp.Key))
                        resolved[kvp.Key] = kvp.Value;
                }
            }
            outputProperties.CharacterMap = resolved;
        }
        return Runtime.ResultTreeSerializer.Serialize(result, outputProperties);
    }

    /// <summary>
    /// Runs the supplied transformation action on a dedicated thread with an enlarged
    /// stack.  This gives deep call-template recursion (such as the DocBook lookup.key
    /// template) enough stack space without requiring tail-call optimisations.
    /// </summary>
    private static T RunWithStack<T>(Func<T> action, int stackSize)
    {
        T? result = default;
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                result = action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }, stackSize);
        thread.Start();
        thread.Join();
        if (exception != null)
            throw exception;
        return result!;
    }
}
