// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 mei 2026
// PURPOSE              : Registers XSLT-specific XPath functions (fn:transform) on EvaluationContext
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 27-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Functions;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;

namespace Bosak.XPath.Xslt.Api;

/// <summary>
/// Registers XSLT-specific XPath functions that cannot live in Bosak.XPath.Standard
/// due to project-dependency layering.
/// </summary>
public static class XsltFunctionLibrary
{
    /// <summary>
    /// Registers fn:transform and other XSLT-specific functions on the context.
    /// </summary>
    public static void Populate(EvaluationContext context)
    {
        context.RegisterFunction(new FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "transform",
            Arity = 1,
            ParameterTypes = [XdmValueKind.Map],
            ReturnType = XdmValueKind.Map,
            Implementation = Transform_1
        });
    }

    private static XdmValue Transform_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (!args[0].IsMap)
            throw new InvalidOperationException("XPTY0004: fn:transform expects a map argument");

        var options = args[0].MapValue;

        // Extract stylesheet-location
        string? stylesheetLocation = null;
        if (options.TryGetValue(XdmValue.FromString("stylesheet-location"), out var locValue))
            stylesheetLocation = AtomizeValue(locValue).ToString();

        if (string.IsNullOrEmpty(stylesheetLocation))
            throw new InvalidOperationException("FOXT0001: stylesheet-location is required");

        // Resolve relative URI against base URI
        string resolvedUri = stylesheetLocation;
        if (!Uri.IsWellFormedUriString(stylesheetLocation, UriKind.Absolute) && !string.IsNullOrEmpty(ctx.BaseUri))
        {
            resolvedUri = new Uri(new Uri(ctx.BaseUri), stylesheetLocation).AbsoluteUri;
        }

        // Load and compile the stylesheet
        XDocument stylesheetDoc;
        try
        {
            stylesheetDoc = XDocument.Load(resolvedUri);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"FOXT0001: Failed to load stylesheet '{resolvedUri}': {ex.Message}");
        }

        var compiler = new XsltCompiler();
        var executable = compiler.Compile(stylesheetDoc, resolvedUri);

        // Extract source-node (optional)
        IXdmNode? sourceNode = null;
        if (options.TryGetValue(XdmValue.FromString("source-node"), out var sourceValue))
        {
            if (sourceValue.IsNode)
                sourceNode = sourceValue.NodeValue;
        }

        // Default source: empty document
        sourceNode ??= new XDocumentNode(new XDocument(new XElement("root")));

        // Extract initial-template (optional)
        string? initialTemplate = null;
        if (options.TryGetValue(XdmValue.FromString("initial-template"), out var initTemplateValue))
            initialTemplate = AtomizeValue(initTemplateValue).ToString();

        // Extract stylesheet-params (optional) — map of parameter names to values
        var transformContext = new EvaluationContext();
        if (options.TryGetValue(XdmValue.FromString("stylesheet-params"), out var paramsValue) && paramsValue.IsMap)
        {
            foreach (var kvp in paramsValue.MapValue.Entries)
            {
                var paramName = AtomizeValue(kvp.Key).ToString();
                if (!string.IsNullOrEmpty(paramName))
                    transformContext.WithVariable(paramName, kvp.Value);
            }
        }

        // Run the transform
        var result = executable.Transform(sourceNode, transformContext, initialTemplate);

        // Return map { "output": result }
        var resultMap = new XdmMap();
        resultMap.Add(XdmValue.FromString("output"), result);
        return XdmValue.FromMap(resultMap);
    }

    private static XdmValue AtomizeValue(XdmValue value)
    {
        if (value.IsSequence && value.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                return item;
            return XdmValue.Undefined;
        }
        return value;
    }
}
