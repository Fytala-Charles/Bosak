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
//                      | Charles Korthout | 0.2   | 11-07-2026     | Full fn:transform: initial-match-selection/mode, delivery formats, packages, result docs |
//                      | Charles Korthout | 0.3   | 15-07-2026     | stylesheet-location consults ResourceUriMapper so published http: URIs map to local files|
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 15-07-2026     | Honor stylesheet-base-uri; stylesheet-text without it has no base URI (XTSE0165)         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 15-07-2026     | fn:transform option handling: params, serialization, base URI, default mode, validation |
//                      | Charles Korthout | 0.6   | 15-07-2026     | global-context-item default wrapper, xslt-version type validation, xslt-version override  |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Functions;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;

namespace Bosak.Xslt.Api;

/// <summary>
/// Registers XSLT-specific XPath functions that cannot live in Bosak.XPath.Standard
/// due to project-dependency layering.
/// </summary>
public static class XsltFunctionLibrary
{
    /// <summary>
    /// Registry of packages available to <c>fn:transform</c> via the
    /// <c>package-name</c>/<c>package-version</c> options. Entries map a package name
    /// and version to a stylesheet location (file path or absolute URI).
    /// </summary>
    private static readonly List<(string Name, string Version, string Location)> _packageRegistry = new();

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

    /// <summary>
    /// Registers a named package for later selection by <c>fn:transform</c>.
    /// </summary>
    /// <param name="name">The package name (URI).</param>
    /// <param name="version">The package version string.</param>
    /// <param name="location">A file path or absolute URI from which the package can be loaded.</param>
    public static void RegisterPackage(string name, string version, string location)
    {
        lock (_packageRegistry)
        {
            _packageRegistry.RemoveAll(p => p.Name == name && p.Version == version);
            _packageRegistry.Add((name, version, location));
        }
    }

    /// <summary>Removes all packages registered via <see cref="RegisterPackage"/>.</summary>
    public static void ClearPackages()
    {
        lock (_packageRegistry)
        {
            _packageRegistry.Clear();
        }
    }

    private static XdmValue Transform_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (!args[0].IsMap)
            throw new InvalidOperationException("XPTY0004: fn:transform expects a map argument");

        var options = args[0].MapValue;

        // delivery-format: document (default), raw, or serialized.
        var deliveryFormat = GetStringOption(options, "delivery-format") ?? "document";
        if (deliveryFormat is not ("document" or "raw" or "serialized"))
            throw new InvalidOperationException($"FOXT0004: Invalid delivery-format '{deliveryFormat}'.");

        // initial-template / initial-mode / initial-function are xs:QName values.
        string? initialTemplate = GetQNameOption(options, "initial-template");
        string? initialMode = GetQNameOption(options, "initial-mode");
        string? initialFunction = GetQNameOption(options, "initial-function");

        // source-node (optional).
        IXdmNode? sourceNode = null;
        if (options.TryGetValue(XdmValue.FromString("source-node"), out var sourceValue))
        {
            var first = FirstItem(sourceValue);
            if (first.IsNode && first.NodeValue != null)
                sourceNode = first.NodeValue;
            else if (!first.IsUndefined)
                throw new InvalidOperationException("XPTY0004: fn:transform source-node must be a node");
        }

        // initial-match-selection (optional): any XDM value.
        XdmValue? initialMatchSelection = null;
        if (options.TryGetValue(XdmValue.FromString("initial-match-selection"), out var selectionValue))
            initialMatchSelection = selectionValue;

        // base-output-uri (optional): resolve against the calling static base URI if relative.
        string? baseOutputUri = GetStringOption(options, "base-output-uri");
        if (!string.IsNullOrEmpty(baseOutputUri)
            && !Uri.IsWellFormedUriString(baseOutputUri, UriKind.Absolute)
            && !string.IsNullOrEmpty(ctx.BaseUri))
        {
            baseOutputUri = new Uri(new Uri(ctx.BaseUri), baseOutputUri).AbsoluteUri;
        }

        // xslt-version: extract numeric value and reject non-numeric strings (XPTY0004).
        double? xsltVersion = GetXsltVersion(options);

        // Validate option types and mutually-exclusive combinations.
        ValidateTransformOptions(options, initialTemplate, initialMode, initialFunction,
            sourceNode, initialMatchSelection);

        // global-context-item (XSLT 3.0 only). When absent and source-node is not a
        // document node, the global context item is a parentless document node that
        // has the source node as its only child.
        IXdmNode? globalContextItem = null;
        if (xsltVersion is null or >= 3.0)
        {
            if (options.TryGetValue(XdmValue.FromString("global-context-item"), out var gciValue))
            {
                var first = FirstItem(gciValue);
                if (first.IsNode && first.NodeValue != null)
                    globalContextItem = first.NodeValue;
                else if (!first.IsUndefined)
                    throw new InvalidOperationException("XPTY0004: fn:transform global-context-item must be a node");
            }
        }
        if (globalContextItem == null && sourceNode != null && sourceNode.NodeKind != XdmNodeKind.Document)
            globalContextItem = WrapNodeInDocument(sourceNode);

        // Static parameters are supplied at compile time.
        var staticParams = GetParameterMap(options, "static-params", allowStringKeys: false);
        var staticParameters = staticParams.Count > 0
            ? staticParams.ToDictionary(
                kvp => SplitClarkName(kvp.Key),
                kvp => kvp.Value)
            : null;

        var transformContext = new EvaluationContext();
        XdmExecutableSource executableSource = LoadExecutable(options, ctx, staticParameters);

        // The static base URI inside the nested stylesheet is the stylesheet's own base URI.
        transformContext.BaseUri = executableSource.BaseUri ?? ctx.BaseUri;
        if (xsltVersion.HasValue)
            transformContext.XsltVersion = xsltVersion.Value;

        // Serialization parameters supplied by the caller.
        Stylesheet.OutputProperties? serializationParams = null;
        if (options.TryGetValue(XdmValue.FromString("serialization-params"), out var serValue))
        {
            if (!serValue.IsMap)
                throw new InvalidOperationException("XPTY0004: fn:transform serialization-params must be a map.");
            serializationParams = Stylesheet.OutputProperties.FromMap(serValue.MapValue);
        }

        // Stylesheet-level parameters are ordinary variables in the transform context.
        var stylesheetParams = GetParameterMap(options, "stylesheet-params", allowStringKeys: false);
        foreach (var kvp in stylesheetParams)
        {
            var (local, ns) = SplitClarkName(kvp.Key);
            transformContext.WithVariable(local, kvp.Value, ns);
        }

        // Template/tunnel parameters for the initial named template.
        transformContext.InitialTemplateCallParameters = GetParameterMap(options, "template-params", allowStringKeys: false);
        transformContext.InitialTemplateTunnelParameters = GetParameterMap(options, "tunnel-params", allowStringKeys: false);

        XdmValue result;
        IReadOnlyDictionary<string, XdmValue> secondaryResults;

        try
        {
            if (!string.IsNullOrEmpty(initialFunction))
            {
                var functionParamsValue = XdmValue.Undefined;
                if (options.TryGetValue(XdmValue.FromString("function-params"), out var fpValue))
                    functionParamsValue = fpValue;
                var argsArray = FunctionArgsFromValue(functionParamsValue);
                result = executableSource.Executable.TransformFunctionCaptured(
                    initialFunction, argsArray, transformContext,
                    deliveryFormat, baseOutputUri, serializationParams, sourceNode, globalContextItem, out secondaryResults);
            }
            else
            {
                result = executableSource.Executable.TransformCaptured(
                    sourceNode, initialMatchSelection, transformContext,
                    initialTemplate, initialMode, deliveryFormat, baseOutputUri, serializationParams,
                    globalContextItem, out secondaryResults);
            }
        }
        catch (Exception ex) when (ex.Message.StartsWith("XTSE", StringComparison.Ordinal)
            || ex.Message.StartsWith("XPST", StringComparison.Ordinal))
        {
            // A static error in the nested stylesheet is reported as FOXT0002.
            throw new InvalidOperationException($"FOXT0002: {ex.Message}");
        }

        var resultMap = new XdmMap();
        string principalKey = !string.IsNullOrEmpty(baseOutputUri) ? baseOutputUri : "output";
        if (!IsAbsentPrincipalResult(result, deliveryFormat))
            resultMap.Add(XdmValue.FromString(principalKey), result);
        foreach (var (uri, value) in secondaryResults)
            resultMap.Add(XdmValue.FromString(uri), value);
        return XdmValue.FromMap(resultMap);
    }

    /// <summary>
    /// Loads and compiles the stylesheet or package identified by the options map.
    /// </summary>
    private static XdmExecutableSource LoadExecutable(
        XdmMap options,
        EvaluationContext ctx,
        Dictionary<(string LocalName, string NamespaceUri), XdmValue>? staticParameters = null)
    {
        var stylesheetLocation = GetStringOption(options, "stylesheet-location");
        var stylesheetText = GetStringOption(options, "stylesheet-text");
        var packageName = GetStringOption(options, "package-name");
        XdmValue? stylesheetNodeValue = null;
        if (options.TryGetValue(XdmValue.FromString("stylesheet-node"), out var nodeValue))
            stylesheetNodeValue = nodeValue;

        int sourceCount = (stylesheetLocation != null ? 1 : 0)
            + (stylesheetText != null ? 1 : 0)
            + (stylesheetNodeValue != null ? 1 : 0)
            + (packageName != null ? 1 : 0);
        if (sourceCount != 1)
            throw new InvalidOperationException(
                "FOXT0001: fn:transform requires exactly one of stylesheet-location, stylesheet-node, stylesheet-text, or package-name.");

        var compiler = new XsltCompiler();
        if (staticParameters != null)
            compiler.StaticParameters = staticParameters;

        if (packageName != null)
        {
            var versionRange = GetStringOption(options, "package-version") ?? "*";
            var location = ResolvePackage(packageName, versionRange);
            if (location == null)
                throw new InvalidOperationException(
                    $"FOXT0001: Package '{packageName}' with version '{versionRange}' is not available.");
            return CompileFromLocation(compiler, location, packageName);
        }

        // The stylesheet-base-uri option supplies the static base URI of the principal
        // stylesheet module when it has no base URI of its own (F+O: "This value must
        // be used if no other static base URI is available"). A relative reference is
        // resolved against the static base URI of the fn:transform call (QT3 bug 30023
        // — fn-transform-err-9a).
        var stylesheetBaseUri = GetStringOption(options, "stylesheet-base-uri");
        if (stylesheetBaseUri != null
            && !Uri.IsWellFormedUriString(stylesheetBaseUri, UriKind.Absolute)
            && !string.IsNullOrEmpty(ctx.BaseUri))
        {
            stylesheetBaseUri = new Uri(new Uri(ctx.BaseUri), stylesheetBaseUri).AbsoluteUri;
        }

        if (stylesheetNodeValue != null)
        {
            var first = FirstItem(stylesheetNodeValue.Value);
            if (!first.IsNode || first.NodeValue == null)
                throw new InvalidOperationException("XPTY0004: fn:transform stylesheet-node must be a node");
            XDocument nodeDoc;
            var node = first.NodeValue;
            if (node.NodeKind == XdmNodeKind.Document && node is XDocumentNode docNode && docNode.UnderlyingObject is XDocument doc)
            {
                nodeDoc = doc;
            }
            else if (node.NodeKind == XdmNodeKind.Element && node is XDocumentNode elemNode && elemNode.UnderlyingObject is XElement elem)
            {
                nodeDoc = new XDocument(elem);
            }
            else
            {
                throw new InvalidOperationException("XPTY0004: fn:transform stylesheet-node must be a document or element node");
            }
            var nodeBaseUri = stylesheetBaseUri ?? node.BaseUri ?? ctx.BaseUri;
            // Reparse with the intended base URI so stylesheet-element BaseUri values
            // reflect the published/static base URI rather than the node origin.
            nodeDoc = Xml11Loader.Parse(nodeDoc.ToString(),
                LoadOptions.SetBaseUri | LoadOptions.PreserveWhitespace, nodeBaseUri);
            return new XdmExecutableSource(compiler.Compile(nodeDoc, nodeBaseUri), nodeBaseUri);
        }

        if (stylesheetText != null)
        {
            // stylesheet-text has NO base URI of its own and does not inherit the calling
            // context's static base URI; only the stylesheet-base-uri option supplies one.
            // Without it, relative references (e.g. xsl:include href) cannot be resolved
            // and raise XTSE0165 (fn-transform-err-9).
            XDocument textDoc;
            try
            {
                textDoc = Xml11Loader.Parse(stylesheetText, LoadOptions.SetBaseUri | LoadOptions.PreserveWhitespace, stylesheetBaseUri ?? string.Empty);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"FOXT0002: Failed to parse stylesheet text: {ex.Message}");
            }
            return new XdmExecutableSource(compiler.Compile(textDoc, stylesheetBaseUri), stylesheetBaseUri);
        }

        // stylesheet-location: resolve against the static base URI.
        string originalUri = stylesheetLocation!;
        if (!Uri.IsWellFormedUriString(stylesheetLocation!, UriKind.Absolute) && !string.IsNullOrEmpty(ctx.BaseUri))
            originalUri = new Uri(new Uri(ctx.BaseUri), stylesheetLocation!).AbsoluteUri;
        // A resource mapper may redirect published (e.g. http:) URIs to local files.
        string mappedUri = ctx.ResourceUriMapper?.Invoke(originalUri) ?? originalUri;
        // The static base URI of the stylesheet is the original (published) URI, not the
        // mapped local file path, unless the stylesheet-base-uri option overrides it.
        string baseUri = stylesheetBaseUri ?? originalUri;
        return CompileFromLocation(compiler, mappedUri, baseUri, displayName: originalUri, sourceBaseUri: baseUri);
    }

    private static XdmExecutableSource CompileFromLocation(
        XsltCompiler compiler,
        string resolvedUri,
        string baseUri,
        string? displayName = null,
        string? sourceBaseUri = null)
    {
        string resolvedPath = resolvedUri;
        if (Uri.IsWellFormedUriString(resolvedUri, UriKind.Absolute) && new Uri(resolvedUri).IsFile)
            resolvedPath = new Uri(resolvedUri).LocalPath;

        string stylesheetText;
        try
        {
            stylesheetText = File.ReadAllText(resolvedPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"FOXT0001: Failed to load stylesheet '{displayName ?? resolvedUri}': {ex.Message}");
        }

        XDocument stylesheetDoc;
        try
        {
            stylesheetDoc = Xml11Loader.Parse(stylesheetText,
                LoadOptions.SetBaseUri | LoadOptions.PreserveWhitespace, baseUri);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"FOXT0002: Failed to parse stylesheet '{displayName ?? resolvedUri}': {ex.Message}");
        }

        try
        {
            return new XdmExecutableSource(compiler.Compile(stylesheetDoc, baseUri), sourceBaseUri ?? baseUri);
        }
        catch (Exception ex) when (ex.Message.StartsWith("XTSE", StringComparison.Ordinal)
            || ex.Message.StartsWith("XPST", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"FOXT0002: {ex.Message}");
        }
    }

    /// <summary>
    /// Resolves a package name and version range against the package registry,
    /// choosing the highest matching registered version.
    /// </summary>
    private static string? ResolvePackage(string name, string versionRange)
    {
        List<(string Name, string Version, string Location)> snapshot;
        lock (_packageRegistry)
        {
            snapshot = _packageRegistry.Where(p => p.Name == name).ToList();
        }

        string? bestLocation = null;
        int[]? bestVersion = null;
        foreach (var candidate in snapshot)
        {
            if (!VersionMatches(candidate.Version, versionRange))
                continue;
            var parts = ParseVersion(candidate.Version);
            if (bestVersion == null || CompareVersions(parts, bestVersion) > 0)
            {
                bestVersion = parts;
                bestLocation = candidate.Location;
            }
        }
        return bestLocation;
    }

    /// <summary>
    /// Matches a concrete version against an XSLT package version range
    /// (e.g. <c>1.0.2</c>, <c>1.*</c>, <c>1.0-2.0</c>, <c>*</c>).
    /// </summary>
    private static bool VersionMatches(string version, string range)
    {
        range = range.Trim();
        if (range == "*" || range.Length == 0)
            return true;

        var dashIndex = range.IndexOf('-');
        if (dashIndex > 0)
        {
            var lower = range[..dashIndex];
            var upper = range[(dashIndex + 1)..];
            return VersionAtLeast(version, lower) && VersionAtMost(version, upper);
        }

        if (range.EndsWith(".*", StringComparison.Ordinal))
        {
            var prefix = range[..^2];
            return VersionAtLeast(version, prefix)
                && CompareVersions(ParseVersion(version), ParseVersion(prefix)) is var cmp
                && (cmp == 0 || PrefixContains(prefix, version));
        }

        return CompareVersions(ParseVersion(version), ParseVersion(range)) == 0;
    }

    private static bool PrefixContains(string prefix, string version)
    {
        var prefixParts = ParseVersion(prefix);
        var versionParts = ParseVersion(version);
        if (versionParts.Length < prefixParts.Length)
            return false;
        for (int i = 0; i < prefixParts.Length; i++)
        {
            if (versionParts[i] != prefixParts[i])
                return false;
        }
        return true;
    }

    private static bool VersionAtLeast(string version, string bound)
        => CompareVersions(ParseVersion(version), ParseVersion(bound)) >= 0;

    private static bool VersionAtMost(string version, string bound)
        => CompareVersions(ParseVersion(version), ParseVersion(bound)) <= 0;

    private static int[] ParseVersion(string version)
    {
        var parts = version.Trim().Split('.');
        var result = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], System.Globalization.NumberStyles.None,
                    System.Globalization.CultureInfo.InvariantCulture, out result[i]))
                result[i] = 0;
        }
        return result;
    }

    private static int CompareVersions(int[] a, int[] b)
    {
        int len = Math.Max(a.Length, b.Length);
        for (int i = 0; i < len; i++)
        {
            int av = i < a.Length ? a[i] : 0;
            int bv = i < b.Length ? b[i] : 0;
            if (av != bv)
                return av.CompareTo(bv);
        }
        return 0;
    }

    private static string? GetStringOption(XdmMap options, string key)
    {
        if (!options.TryGetValue(XdmValue.FromString(key), out var value))
            return null;
        var first = FirstItem(value);
        if (first.IsUndefined)
            return null;
        if (first.IsNode && first.NodeValue != null)
            return first.NodeValue.StringValue;
        return first.ToString();
    }

    /// <summary>
    /// Reads a QName-valued option and converts it to Clark notation (<c>{uri}local</c>)
    /// or a plain local name when in no namespace.
    /// </summary>
    private static string? GetQNameOption(XdmMap options, string key)
    {
        if (!options.TryGetValue(XdmValue.FromString(key), out var value))
            return null;
        return GetQNameString(value);
    }

    private static string? GetQNameString(XdmValue value)
    {
        var first = FirstItem(value);
        if (first.IsUndefined)
            return null;
        if (first.Kind == XdmValueKind.QName)
        {
            var qn = first.QNameValue;
            return string.IsNullOrEmpty(qn.NamespaceUri) ? qn.LocalName : $"{{{qn.NamespaceUri}}}{qn.LocalName}";
        }
        if (first.IsNode && first.NodeValue != null)
            return first.NodeValue.StringValue;
        return first.ToString();
    }

    private static XdmValue FirstItem(XdmValue value)
    {
        if (value.IsSequence && value.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                return item;
            return XdmValue.Undefined;
        }
        return value;
    }

    /// <summary>
    /// Extracts the numeric <c>xslt-version</c> option value, raising <c>XPTY0004</c>
    /// for non-numeric or string values.
    /// </summary>
    private static double? GetXsltVersion(XdmMap options)
    {
        if (!options.TryGetValue(XdmValue.FromString("xslt-version"), out var value))
            return null;
        var first = FirstItem(value);
        if (first.IsUndefined)
            return null;
        if (first.Kind == XdmValueKind.Decimal)
            return (double)first.DecimalValue;
        if (first.Kind == XdmValueKind.Integer)
            return first.IntegerValue;
        if (first.Kind is XdmValueKind.Double or XdmValueKind.Float)
            return first.DoubleValue;
        throw new InvalidOperationException("XPTY0004: fn:transform xslt-version must be a numeric value.");
    }

    /// <summary>
    /// Wraps a non-document source node in a parentless document node for use as the
    /// default global context item.
    /// </summary>
    private static IXdmNode WrapNodeInDocument(IXdmNode node)
    {
        if (node is XDocumentNode xdn)
        {
            if (xdn.UnderlyingObject is XElement elem)
                return new XDocumentNode(new XDocument(new XElement(elem)));
            if (xdn.UnderlyingObject is XDocument doc)
                return new XDocumentNode(doc);
        }
        var fallback = new XElement("__wrapper__", node.StringValue);
        return new XDocumentNode(new XDocument(fallback));
    }

    private static void ValidateTransformOptions(
        XdmMap options,
        string? initialTemplate,
        string? initialMode,
        string? initialFunction,
        IXdmNode? sourceNode,
        XdmValue? initialMatchSelection)
    {
        // Mutually exclusive entry-point options.
        if (!string.IsNullOrEmpty(initialMode) && !string.IsNullOrEmpty(initialTemplate))
            throw new InvalidOperationException("XPTY0004: fn:transform options initial-mode and initial-template are mutually exclusive.");

        if (sourceNode != null && initialMatchSelection != null)
            throw new InvalidOperationException("FOXT0002: fn:transform options source-node and initial-match-selection are mutually exclusive.");

        if (!string.IsNullOrEmpty(initialFunction))
        {
            if (!string.IsNullOrEmpty(initialTemplate) || !string.IsNullOrEmpty(initialMode)
                || initialMatchSelection != null)
            {
                throw new InvalidOperationException("FOXT0002: fn:transform option initial-function cannot be combined with initial-template, initial-mode, or initial-match-selection.");
            }
            if (!options.TryGetValue(XdmValue.FromString("function-params"), out _))
                throw new InvalidOperationException("FOXT0002: fn:transform option function-params is required when initial-function is supplied.");
        }

        // Unsupported options: requested-properties and post-process.
        if (options.TryGetValue(XdmValue.FromString("requested-properties"), out var requestedProps))
        {
            var first = FirstItem(requestedProps);
            if (!first.IsUndefined && (!(first.IsMap && first.MapValue.Entries.Count() == 0)))
                throw new InvalidOperationException("FOXT0001: fn:transform requested-properties option is not supported.");
        }
        if (options.TryGetValue(XdmValue.FromString("post-process"), out _))
            throw new InvalidOperationException("FOXT0001: fn:transform post-process option is not supported.");
    }

    /// <summary>
    /// Reads a map-valued option whose keys must be QNames and returns a dictionary
    /// keyed by expanded Clark notation.
    /// </summary>
    private static Dictionary<string, XdmValue> GetParameterMap(XdmMap options, string key, bool allowStringKeys)
    {
        if (!options.TryGetValue(XdmValue.FromString(key), out var value))
            return new Dictionary<string, XdmValue>();
        if (!value.IsMap)
            throw new InvalidOperationException($"XPTY0004: fn:transform option '{key}' must be a map.");

        var result = new Dictionary<string, XdmValue>();
        foreach (var kvp in value.MapValue.Entries)
        {
            string expanded;
            if (kvp.Key.Kind == XdmValueKind.QName)
            {
                var qn = kvp.Key.QNameValue;
                expanded = string.IsNullOrEmpty(qn.NamespaceUri) ? qn.LocalName : $"{{{qn.NamespaceUri}}}{qn.LocalName}";
            }
            else if (allowStringKeys && kvp.Key.Kind == XdmValueKind.String)
            {
                expanded = kvp.Key.StringValue;
            }
            else
            {
                throw new InvalidOperationException($"FOXT0002: fn:transform option '{key}' keys must be QNames.");
            }
            result[expanded] = kvp.Value;
        }
        return result;
    }

    /// <summary>
    /// Splits a Clark-notation QName into its local name and namespace URI components.
    /// </summary>
    private static (string LocalName, string NamespaceUri) SplitClarkName(string name)
    {
        if (name.Length > 2 && name[0] == '{')
        {
            int end = name.IndexOf('}');
            if (end > 0)
                return (name[(end + 1)..], name[1..end]);
        }
        return (name, "");
    }

    private static XdmValue[] FunctionArgsFromValue(XdmValue value)
    {
        if (value.IsUndefined)
            return Array.Empty<XdmValue>();
        if (value.IsArray)
        {
            var list = new List<XdmValue>();
            foreach (var item in value.ArrayValue.Values)
                list.Add(item);
            return list.ToArray();
        }
        if (value.IsSequence && value.SequenceValue is not null)
        {
            var list = new List<XdmValue>();
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                list.Add(item);
            return list.ToArray();
        }
        return [value];
    }

    private static bool IsAbsentPrincipalResult(XdmValue result, string deliveryFormat)
    {
        if (result.IsUndefined)
            return true;
        if (deliveryFormat == "serialized")
            return result.Kind == XdmValueKind.String && result.StringValue.Length == 0;
        if (deliveryFormat == "raw")
        {
            if (result.IsSequence && result.SequenceValue is not null)
            {
                foreach (var _ in XdmSequence.FromSource(result.SequenceValue))
                    return false;
                return true;
            }
            return false;
        }
        // document delivery format
        if (result.IsNode && result.NodeValue != null && result.NodeValue.NodeKind == XdmNodeKind.Document)
        {
            if (result.NodeValue is XDocumentNode xdn && xdn.UnderlyingObject is XDocument doc)
                return !doc.Nodes().Any();
            foreach (var _ in result.NodeValue.Axis(XdmAxis.Child))
                return false;
            return true;
        }
        return false;
    }

    private readonly record struct XdmExecutableSource(XsltExecutable Executable, string? BaseUri);
}
