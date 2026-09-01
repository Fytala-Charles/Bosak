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
//                      | Charles Korthout | 1.3   | 12-07-2026     | Resolve stylesheet-level character maps in OutputProperties; pre-resolved maps win.     |
//                      | Charles Korthout | 1.4   | 13-07-2026     | Stamp EffectiveVersion and ImplicitResultTree for default output-method inference.      |
//                      | Charles Korthout | 1.5   | 13-07-2026     | Raised transform stack to 16MB for deep continuation-style recursion (HOF-068).         |
//                      | Charles Korthout | 1.6    | 14-07-2026     | TransformCaptured: fn:transform entry point with result-document capture and formats.  |
//                      | Charles Korthout | 1.7   | 15-07-2026     | Added serialization-params merge and TransformFunctionCaptured for fn:transform.        |
//                      | Charles Korthout | 1.8   | 15-07-2026     | TransformCaptured/TransformFunctionCaptured accept explicit global context item           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.9   | 28-08-2026     | Added optional serializationParams to TransformToString/TransformFunctionToString.        |
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
    /// Gets the effective stylesheet-level output properties, with named character maps
    /// resolved to a concrete character-to-string table.
    /// </summary>
    public Stylesheet.OutputProperties OutputProperties
    {
        get
        {
            var props = _stylesheet.EffectiveOutputProperties ?? new Stylesheet.OutputProperties();
            if (props.UseCharacterMaps.Count > 0 && props.CharacterMap == null)
            {
                props = props.Clone();
                props.CharacterMap = _stylesheet.ResolveCharacterMap(
                    props.UseCharacterMaps.Select(Stylesheet.Stylesheet.ExpandQName));
            }
            // If the runtime already resolved the maps inside a package scope, keep them.
            return props;
        }
    }

    /// <summary>
    /// Gets the output properties of the principal <c>xsl:result-document</c> produced
    /// during the last transformation, if any.
    /// </summary>
    public Stylesheet.OutputProperties? LastResultDocumentProperties { get; private set; }

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
    private const int DefaultTransformStackSize = 16 * 1024 * 1024;

    public XdmValue Transform(IXdmNode? source, EvaluationContext? context = null, string? initialTemplate = null, string? initialMode = null, bool rawResult = false, string? baseOutputUri = null)
    {
        return RunWithStack(() =>
        {
            var engine = new Runtime.TransformEngine(_stylesheet, context, _messageListener, _treatRecoverableAmbiguousMatchAsError);
            var result = engine.Transform(source, initialTemplate, initialMode, rawResult, baseOutputUri);
            LastResultDocumentProperties = engine.PrincipalResultDocumentProperties;
            return result;
        }, DefaultTransformStackSize);
    }

    /// <summary>
    /// Runs a transformation with an explicit initial match selection, which is applied
    /// to the initial mode when no source node is supplied.
    /// </summary>
    public XdmValue Transform(IXdmNode? source, XdmValue? initialMatchSelection, EvaluationContext? context = null, string? initialTemplate = null, string? initialMode = null, bool rawResult = false, string? baseOutputUri = null)
    {
        return RunWithStack(() =>
        {
            var engine = new Runtime.TransformEngine(_stylesheet, context, _messageListener, _treatRecoverableAmbiguousMatchAsError);
            var result = engine.Transform(source, initialTemplate, initialMode, rawResult, baseOutputUri, initialMatchSelection: initialMatchSelection);
            LastResultDocumentProperties = engine.PrincipalResultDocumentProperties;
            return result;
        }, DefaultTransformStackSize);
    }

    /// <summary>
    /// Runs a transformation on behalf of <c>fn:transform</c>: secondary result
    /// documents are captured rather than written to disk, and both the principal
    /// and the secondary results are post-processed according to the requested
    /// delivery format (document, raw, or serialized).
    /// </summary>
    /// <param name="source">The source node, or null when no source-node is supplied.</param>
    /// <param name="initialMatchSelection">Optional initial match selection applied in the initial mode.</param>
    /// <param name="context">Optional evaluation context (stylesheet parameters).</param>
    /// <param name="initialTemplate">Optional initial named template (lexical or Clark form).</param>
    /// <param name="initialMode">Optional initial mode.</param>
    /// <param name="deliveryFormat">One of <c>document</c>, <c>raw</c>, or <c>serialized</c>.</param>
    /// <param name="baseOutputUri">Optional base output URI for resolving result-document hrefs.</param>
    /// <param name="secondaryResults">The captured secondary result documents, keyed by resolved URI.</param>
    /// <returns>The principal result in the requested delivery format.</returns>
    public XdmValue TransformCaptured(
        IXdmNode? source,
        XdmValue? initialMatchSelection,
        EvaluationContext? context,
        string? initialTemplate,
        string? initialMode,
        string deliveryFormat,
        string? baseOutputUri,
        out IReadOnlyDictionary<string, XdmValue> secondaryResults)
    {
        return TransformCaptured(source, initialMatchSelection, context, initialTemplate, initialMode,
            deliveryFormat, baseOutputUri, serializationParams: null, out secondaryResults);
    }

    public XdmValue TransformCaptured(
        IXdmNode? source,
        XdmValue? initialMatchSelection,
        EvaluationContext? context,
        string? initialTemplate,
        string? initialMode,
        string deliveryFormat,
        string? baseOutputUri,
        Stylesheet.OutputProperties? serializationParams,
        out IReadOnlyDictionary<string, XdmValue> secondaryResults)
        => TransformCaptured(source, initialMatchSelection, context, initialTemplate, initialMode,
            deliveryFormat, baseOutputUri, serializationParams, globalContextItem: null, out secondaryResults);

    public XdmValue TransformCaptured(
        IXdmNode? source,
        XdmValue? initialMatchSelection,
        EvaluationContext? context,
        string? initialTemplate,
        string? initialMode,
        string deliveryFormat,
        string? baseOutputUri,
        Stylesheet.OutputProperties? serializationParams,
        IXdmNode? globalContextItem,
        out IReadOnlyDictionary<string, XdmValue> secondaryResults)
    {
        IReadOnlyDictionary<string, XdmValue> capturedResults = new Dictionary<string, XdmValue>();
        var principalResult = RunWithStack(() =>
        {
            bool raw = deliveryFormat == "raw";
            bool serialized = deliveryFormat == "serialized";
            var engine = new Runtime.TransformEngine(_stylesheet, context, _messageListener, _treatRecoverableAmbiguousMatchAsError);
            var result = engine.Transform(source, initialTemplate, initialMode,
                rawResult: raw, baseOutputUri: baseOutputUri,
                initialMatchSelection: initialMatchSelection,
                captureResultDocuments: true, rawTransformResult: raw,
                globalContextItem: globalContextItem);
            LastResultDocumentProperties = engine.PrincipalResultDocumentProperties;

            var captured = new Dictionary<string, XdmValue>();
            foreach (var (uri, entry) in engine.CapturedResultDocuments)
            {
                var props = entry.Props;
                if (serialized && serializationParams != null)
                {
                    props = props.Clone();
                    Stylesheet.OutputProperties.Merge(props, serializationParams);
                }
                captured[uri] = serialized
                    ? XdmValue.FromString(Runtime.ResultTreeSerializer.Serialize(entry.Value, props))
                    : entry.Value;
            }
            capturedResults = captured;

            if (serialized)
            {
                var outputProperties = engine.PrincipalResultDocumentProperties
                    ?? OutputProperties;
                outputProperties.EffectiveVersion ??= _stylesheet.Version;
                outputProperties.ImplicitResultTree = engine.PrincipalResultDocumentProperties == null;
                if (serializationParams != null)
                {
                    outputProperties = outputProperties.Clone();
                    Stylesheet.OutputProperties.Merge(outputProperties, serializationParams);
                }
                return XdmValue.FromString(Runtime.ResultTreeSerializer.Serialize(result, outputProperties));
            }

            return result;
        }, DefaultTransformStackSize);
        secondaryResults = capturedResults;
        return principalResult;
    }

    /// <summary>
    /// Transforms the supplied source document and serializes the result to a string.
    /// </summary>
    /// <param name="source">The source document or node to transform. May be null for named-template entry points with no initial context item.</param>
    /// <param name="context">Optional evaluation context.</param>
    /// <param name="initialTemplate">Optional name of the initial template to execute.</param>
    /// <param name="initialMode">Optional name of the initial mode to use.</param>
    /// <param name="baseOutputUri">The base output URI for the transformation; used by fn:current-output-uri().</param>
    /// <param name="serializationParams">Optional serialization parameters merged with the effective output properties.</param>
    /// <returns>The serialized result of the transformation.</returns>
    public string TransformToString(IXdmNode? source, EvaluationContext? context = null, string? initialTemplate = null, string? initialMode = null, string? baseOutputUri = null, Stylesheet.OutputProperties? serializationParams = null)
    {
        return RunWithStack(() =>
        {
            var engine = new Runtime.TransformEngine(_stylesheet, context, _messageListener, _treatRecoverableAmbiguousMatchAsError);
            var result = engine.Transform(source, initialTemplate, initialMode, false, baseOutputUri);
            LastResultDocumentProperties = engine.PrincipalResultDocumentProperties;

            // A principal xsl:result-document (no href) supplies the effective output
            // properties, overriding the stylesheet-level xsl:output defaults.
            var outputProperties = engine.PrincipalResultDocumentProperties
                ?? _stylesheet.EffectiveOutputProperties
                ?? new Stylesheet.OutputProperties();

            // Fall back to annotation-based output properties for backward compatibility
            // when the result tree is a wrapper element produced by the legacy path.
            bool implicitResultTree = engine.PrincipalResultDocumentProperties == null;
            if (engine.PrincipalResultDocumentProperties == null && result.IsNode && result.NodeValue is XDocumentNode xdn)
            {
                Stylesheet.OutputProperties? rdProps = null;
                if (xdn.UnderlyingObject is XDocument doc)
                    rdProps = doc.Annotation<Stylesheet.OutputProperties>();
                else if (xdn.UnderlyingObject is XElement elem)
                    rdProps = elem.Annotation<Stylesheet.OutputProperties>();
                if (rdProps != null)
                {
                    outputProperties = rdProps;
                    implicitResultTree = false;
                }
            }

            // Default output-method inference needs the effective stylesheet version
            // and whether the result tree was generated implicitly (XSLT 3.0 §26).
            outputProperties.EffectiveVersion = _stylesheet.Version;
            outputProperties.ImplicitResultTree = implicitResultTree;

            // Resolve named character maps for the principal output if not already done.
            if (outputProperties.UseCharacterMaps.Count > 0 && outputProperties.CharacterMap == null)
            {
                outputProperties = outputProperties.Clone();
                outputProperties.CharacterMap = _stylesheet.ResolveCharacterMap(
                    outputProperties.UseCharacterMaps.Select(Stylesheet.Stylesheet.ExpandQName));
            }

            if (serializationParams != null)
            {
                outputProperties = outputProperties.Clone();
                Stylesheet.OutputProperties.Merge(outputProperties, serializationParams);
            }

            return Runtime.ResultTreeSerializer.Serialize(result, outputProperties);
        }, DefaultTransformStackSize);
    }

    /// <summary>
    /// Transforms the supplied source document (or initial match selection) and serializes
    /// the result to a string.
    /// </summary>
    public string TransformToString(IXdmNode? source, XdmValue? initialMatchSelection, EvaluationContext? context = null, string? initialTemplate = null, string? initialMode = null, string? baseOutputUri = null, Stylesheet.OutputProperties? serializationParams = null)
    {
        return RunWithStack(() =>
        {
            var engine = new Runtime.TransformEngine(_stylesheet, context, _messageListener, _treatRecoverableAmbiguousMatchAsError);
            var result = engine.Transform(source, initialTemplate, initialMode, false, baseOutputUri, initialMatchSelection: initialMatchSelection);
            LastResultDocumentProperties = engine.PrincipalResultDocumentProperties;

            var outputProperties = engine.PrincipalResultDocumentProperties
                ?? _stylesheet.EffectiveOutputProperties
                ?? new Stylesheet.OutputProperties();
            outputProperties.EffectiveVersion = _stylesheet.Version;

            if (serializationParams != null)
            {
                outputProperties = outputProperties.Clone();
                Stylesheet.OutputProperties.Merge(outputProperties, serializationParams);
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
    /// <param name="serializationParams">Optional serialization parameters merged with the effective output properties.</param>
    /// <returns>The serialized result of the function call.</returns>
    public string TransformFunctionToString(string name, XdmValue[] args, EvaluationContext? context = null, Stylesheet.OutputProperties? serializationParams = null)
    {
        var result = TransformFunction(name, args, context);
        var outputProperties = _stylesheet.EffectiveOutputProperties ?? new Stylesheet.OutputProperties();
        outputProperties.EffectiveVersion = _stylesheet.Version;
        outputProperties.ImplicitResultTree = true;
        if (outputProperties.UseCharacterMaps.Count > 0 && outputProperties.CharacterMap == null)
        {
            outputProperties = outputProperties.Clone();
            outputProperties.CharacterMap = _stylesheet.ResolveCharacterMap(
                outputProperties.UseCharacterMaps.Select(Stylesheet.Stylesheet.ExpandQName));
        }
        if (serializationParams != null)
        {
            outputProperties = outputProperties.Clone();
            Stylesheet.OutputProperties.Merge(outputProperties, serializationParams);
        }
        return Runtime.ResultTreeSerializer.Serialize(result, outputProperties);
    }

    /// <summary>
    /// Invokes an <c>xsl:function</c> as the transformation entry point on behalf of
    /// <c>fn:transform</c>, honoring delivery format, base output URI, serialization
    /// parameters, and captured secondary result documents.
    /// </summary>
    /// <param name="name">The expanded function name (EQName form <c>Q{{uri}}local</c>).</param>
    /// <param name="args">Arguments to pass to the function.</param>
    /// <param name="context">Optional evaluation context.</param>
    /// <param name="deliveryFormat">One of <c>document</c>, <c>raw</c>, or <c>serialized</c>.</param>
    /// <param name="baseOutputUri">Optional base output URI for secondary result documents.</param>
    /// <param name="serializationParams">Optional user-supplied serialization parameters.</param>
    /// <param name="secondaryResults">The captured secondary result documents.</param>
    /// <returns>The principal result in the requested delivery format.</returns>
    public XdmValue TransformFunctionCaptured(
        string name,
        XdmValue[] args,
        EvaluationContext? context,
        string deliveryFormat,
        string? baseOutputUri,
        Stylesheet.OutputProperties? serializationParams,
        out IReadOnlyDictionary<string, XdmValue> secondaryResults)
        => TransformFunctionCaptured(name, args, context, deliveryFormat, baseOutputUri, serializationParams, source: null, out secondaryResults);

    public XdmValue TransformFunctionCaptured(
        string name,
        XdmValue[] args,
        EvaluationContext? context,
        string deliveryFormat,
        string? baseOutputUri,
        Stylesheet.OutputProperties? serializationParams,
        IXdmNode? source,
        out IReadOnlyDictionary<string, XdmValue> secondaryResults)
        => TransformFunctionCaptured(name, args, context, deliveryFormat, baseOutputUri, serializationParams, source, globalContextItem: null, out secondaryResults);

    public XdmValue TransformFunctionCaptured(
        string name,
        XdmValue[] args,
        EvaluationContext? context,
        string deliveryFormat,
        string? baseOutputUri,
        Stylesheet.OutputProperties? serializationParams,
        IXdmNode? source,
        IXdmNode? globalContextItem,
        out IReadOnlyDictionary<string, XdmValue> secondaryResults)
    {
        IReadOnlyDictionary<string, XdmValue> capturedResults = new Dictionary<string, XdmValue>();
        var principalResult = RunWithStack(() =>
        {
            bool serialized = deliveryFormat == "serialized";
            var engine = new Runtime.TransformEngine(_stylesheet, context, _messageListener, _treatRecoverableAmbiguousMatchAsError);
            var result = engine.TransformFunction(name, args, captureResultDocuments: true, baseOutputUri: baseOutputUri, source: source, globalContextItem: globalContextItem);
            LastResultDocumentProperties = engine.PrincipalResultDocumentProperties;

            var captured = new Dictionary<string, XdmValue>();
            foreach (var (uri, entry) in engine.CapturedResultDocuments)
            {
                var props = entry.Props;
                if (serialized && serializationParams != null)
                {
                    props = props.Clone();
                    Stylesheet.OutputProperties.Merge(props, serializationParams);
                }
                captured[uri] = serialized
                    ? XdmValue.FromString(Runtime.ResultTreeSerializer.Serialize(entry.Value, props))
                    : entry.Value;
            }
            capturedResults = captured;

            if (deliveryFormat == "serialized")
            {
                var outputProperties = _stylesheet.EffectiveOutputProperties ?? new Stylesheet.OutputProperties();
                outputProperties.EffectiveVersion = _stylesheet.Version;
                outputProperties.ImplicitResultTree = true;
                if (serializationParams != null)
                {
                    outputProperties = outputProperties.Clone();
                    Stylesheet.OutputProperties.Merge(outputProperties, serializationParams);
                }
                ResolveCharacterMapsFor(outputProperties);
                return XdmValue.FromString(Runtime.ResultTreeSerializer.Serialize(result, outputProperties));
            }

            if (deliveryFormat == "document" && !result.IsUndefined)
            {
                if (!result.IsNode || result.NodeValue?.NodeKind != XdmNodeKind.Document)
                {
                    // Wrap a non-document result in a new document node.
                    XDocument wrapper;
                    if (result.IsNode && result.NodeValue != null)
                    {
                        var xdn = (Bosak.XPath.Providers.Xml.XDocumentNode)result.NodeValue;
                        if (xdn.UnderlyingObject is System.Xml.Linq.XElement elem)
                        {
                            wrapper = new System.Xml.Linq.XDocument(elem);
                        }
                        else if (xdn.UnderlyingObject is System.Xml.Linq.XDocument doc)
                        {
                            wrapper = doc;
                        }
                        else
                        {
                            wrapper = new System.Xml.Linq.XDocument();
                            wrapper.Add(new System.Xml.Linq.XText(result.ToString()));
                        }
                    }
                    else
                    {
                        wrapper = new System.Xml.Linq.XDocument();
                        wrapper.Add(new System.Xml.Linq.XText(result.ToString()));
                    }
                    result = XdmValue.FromNode(new Bosak.XPath.Providers.Xml.XDocumentNode(wrapper));
                }
            }

            return result;
        }, DefaultTransformStackSize);
        secondaryResults = capturedResults;
        return principalResult;
    }

    private void ResolveCharacterMapsFor(Stylesheet.OutputProperties outputProperties)
    {
        if (outputProperties.UseCharacterMaps.Count > 0)
        {
            // If the runtime already resolved the maps inside the declaring package scope,
            // use that concrete map instead of re-resolving against the principal stylesheet.
            if (outputProperties.CharacterMap != null && outputProperties.CharacterMap.Count > 0)
            {
                return;
            }

            var resolved = _stylesheet.ResolveCharacterMap(
                outputProperties.UseCharacterMaps.Select(Stylesheet.Stylesheet.ExpandQName));
            if (outputProperties.CharacterMap != null)
            {
                foreach (var kvp in outputProperties.CharacterMap)
                    resolved[kvp.Key] = kvp.Value;
            }
            outputProperties.CharacterMap = resolved;
        }
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
