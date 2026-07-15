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

        XdmExecutableSource executableSource = LoadExecutable(options, ctx);

        // delivery-format: document (default), raw, or serialized.
        var deliveryFormat = GetStringOption(options, "delivery-format") ?? "document";
        if (deliveryFormat is not ("document" or "raw" or "serialized"))
            throw new InvalidOperationException($"FOXT0004: Invalid delivery-format '{deliveryFormat}'.");

        // initial-template / initial-mode are xs:QName values; convert to Clark form.
        string? initialTemplate = GetQNameOption(options, "initial-template");
        string? initialMode = GetQNameOption(options, "initial-mode");

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

        // base-output-uri (optional).
        string? baseOutputUri = GetStringOption(options, "base-output-uri");

        // stylesheet-params (optional) — map of parameter names to values.
        var transformContext = new EvaluationContext();
        if (options.TryGetValue(XdmValue.FromString("stylesheet-params"), out var paramsValue) && paramsValue.IsMap)
        {
            foreach (var kvp in paramsValue.MapValue.Entries)
            {
                var paramName = GetQNameString(kvp.Key);
                if (!string.IsNullOrEmpty(paramName))
                    transformContext.WithVariable(paramName, kvp.Value);
            }
        }

        try
        {
            var result = executableSource.Executable.TransformCaptured(
                sourceNode, initialMatchSelection, transformContext,
                initialTemplate, initialMode, deliveryFormat, baseOutputUri,
                out var secondaryResults);

            var resultMap = new XdmMap();
            resultMap.Add(XdmValue.FromString("output"), result);
            foreach (var (uri, value) in secondaryResults)
                resultMap.Add(XdmValue.FromString(uri), value);
            return XdmValue.FromMap(resultMap);
        }
        catch (Exception ex) when (ex.Message.StartsWith("XTSE", StringComparison.Ordinal)
            || ex.Message.StartsWith("XPST", StringComparison.Ordinal))
        {
            // A static error in the nested stylesheet is reported as FOXT0002.
            throw new InvalidOperationException($"FOXT0002: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads and compiles the stylesheet or package identified by the options map.
    /// </summary>
    private static XdmExecutableSource LoadExecutable(XdmMap options, EvaluationContext ctx)
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

        if (packageName != null)
        {
            var versionRange = GetStringOption(options, "package-version") ?? "*";
            var location = ResolvePackage(packageName, versionRange);
            if (location == null)
                throw new InvalidOperationException(
                    $"FOXT0001: Package '{packageName}' with version '{versionRange}' is not available.");
            return CompileFromLocation(compiler, location, packageName);
        }

        if (stylesheetNodeValue != null)
        {
            var first = FirstItem(stylesheetNodeValue.Value);
            if (!first.IsNode || first.NodeValue is not XDocumentNode xdn)
                throw new InvalidOperationException("XPTY0004: fn:transform stylesheet-node must be a node");
            var nodeDoc = xdn.UnderlyingObject is XDocument doc
                ? doc
                : new XDocument(xdn.UnderlyingObject as XElement
                    ?? throw new InvalidOperationException("XPTY0004: fn:transform stylesheet-node must be a document or element node"));
            var baseUri = first.NodeValue!.BaseUri ?? ctx.BaseUri;
            return new XdmExecutableSource(compiler.Compile(nodeDoc, baseUri));
        }

        if (stylesheetText != null)
        {
            XDocument textDoc;
            try
            {
                textDoc = Xml11Loader.Parse(stylesheetText, LoadOptions.SetBaseUri | LoadOptions.PreserveWhitespace, ctx.BaseUri ?? string.Empty);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"FOXT0002: Failed to parse stylesheet text: {ex.Message}");
            }
            return new XdmExecutableSource(compiler.Compile(textDoc, ctx.BaseUri));
        }

        // stylesheet-location: resolve against the static base URI.
        string resolvedUri = stylesheetLocation!;
        if (!Uri.IsWellFormedUriString(stylesheetLocation!, UriKind.Absolute) && !string.IsNullOrEmpty(ctx.BaseUri))
            resolvedUri = new Uri(new Uri(ctx.BaseUri), stylesheetLocation!).AbsoluteUri;
        // A resource mapper may redirect published (e.g. http:) URIs to local files.
        resolvedUri = ctx.ResourceUriMapper?.Invoke(resolvedUri) ?? resolvedUri;
        return CompileFromLocation(compiler, resolvedUri, resolvedUri);
    }

    private static XdmExecutableSource CompileFromLocation(XsltCompiler compiler, string resolvedUri, string displayName)
    {
        XDocument stylesheetDoc;
        try
        {
            stylesheetDoc = Xml11Loader.Load(resolvedUri, LoadOptions.SetBaseUri | LoadOptions.PreserveWhitespace);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"FOXT0001: Failed to load stylesheet '{displayName}': {ex.Message}");
        }

        try
        {
            return new XdmExecutableSource(compiler.Compile(stylesheetDoc, resolvedUri));
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

    private readonly record struct XdmExecutableSource(XsltExecutable Executable);
}
