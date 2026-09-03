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
//                      | Charles Korthout | 0.1   | 27-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 11-07-2026     | Full fn:transform: initial-match-selection/mode, delivery formats, packages, result docs |
//                      | Charles Korthout | 0.3   | 15-07-2026     | stylesheet-location consults ResourceUriMapper so published http: URIs map to local files|
//                      | Charles Korthout | 0.4   | 15-07-2026     | Honor stylesheet-base-uri; stylesheet-text without it has no base URI (XTSE0165)         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 15-07-2026     | fn:transform option handling: params, serialization, base URI, default mode, validation |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 15-07-2026     | global-context-item default wrapper, xslt-version type validation, xslt-version override  |
//                      | Charles Korthout | 0.7   | 21-07-2026     | Set IsXsltMode=true in fn:transform nested EvaluationContext                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 03-08-2026     | fn:stream-available#1: open stream + read to root element; false on any failure          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.9   | 29-08-2026     | Implemented fn:unparsed-entity-uri / fn:unparsed-entity-public-id                      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.10  | 29-08-2026     | Raise XTDE1370/XTDE1380 when unparsed-entity lookup has no document context            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.11  | 29-08-2026     | Throw XsltRuntimeException (not InvalidOperationException) for XTDE1370/XTDE1380         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.12  | 30-08-2026     | Exposed ResolvePackageLocation for xsl:use-package resolution                            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.13  | 01-09-2026     | Strict PackageVersion/PackageVersionRange validation for XTSE0020 (REQ-082)             |
//                      |==================|=======|================|=========================================================================================
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.14  | 02-09-2026     | FOXT0002 when a stylesheet-location resource cannot be retrieved (transform-001); fn:transform with no entry point raises FOXT0002|
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Functions;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Bosak.Xslt.Runtime;

namespace Bosak.Xslt.Api;

/// <summary>
/// Strategy for selecting a package version when multiple registered versions match
/// an <c>xsl:use-package</c> version range.
/// </summary>
public enum PackageVersionResolutionStrategy
{
    /// <summary>Select the highest matching version (XSLT 3.0 default).</summary>
    Highest,

    /// <summary>Select the lowest matching version.</summary>
    Lowest
}

/// <summary>
/// Represents a parsed XSLT package version as a sequence of numeric components,
/// each with an optional suffix. Suffixes are compared lexicographically, with the
/// empty suffix (release version) considered greater than any non-empty suffix.
/// </summary>
public readonly record struct PackageVersion
{
    /// <summary>A single version component: a numeric part plus an optional suffix.</summary>
    public readonly record struct Component(int Number, string Suffix)
    {
        public int CompareTo(Component other)
        {
            int cmp = Number.CompareTo(other.Number);
            if (cmp != 0) return cmp;
            return CompareSuffix(Suffix, other.Suffix);
        }
    }

    private readonly Component[] _components;

    public PackageVersion(Component[] components)
    {
        _components = components;
    }

    public ReadOnlySpan<Component> Components => _components;

    public int ComponentCount => _components.Length;

    public Component this[int index] => index < _components.Length ? _components[index] : new Component(0, "");

    public static PackageVersion Parse(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return new PackageVersion(Array.Empty<Component>());

        version = version.Trim();
        var parts = version.Split('.');
        var components = new Component[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            int number = 0;
            int j = 0;
            while (j < part.Length && char.IsAsciiDigit(part[j]))
            {
                number = number * 10 + (part[j] - '0');
                j++;
            }
            components[i] = new Component(number, j < part.Length ? part[j..] : "");
        }
        return new PackageVersion(components);
    }

    public static int Compare(PackageVersion a, PackageVersion b)
    {
        int len = Math.Max(a.ComponentCount, b.ComponentCount);
        for (int i = 0; i < len; i++)
        {
            var ca = a[i];
            var cb = b[i];
            int cmp = ca.CompareTo(cb);
            if (cmp != 0) return cmp;
        }
        return 0;
    }

    public int CompareTo(PackageVersion other) => Compare(this, other);

    /// <summary>
    /// Returns true when this version has the same leading components as
    /// <paramref name="prefix"/>, matching both number and suffix. Used for exact
    /// version and wildcard/prefix matching.
    /// </summary>
    public bool StartsWith(PackageVersion prefix)
    {
        if (prefix.ComponentCount == 0) return true;
        if (_components.Length < prefix.ComponentCount) return false;
        for (int i = 0; i < prefix.ComponentCount; i++)
        {
            if (_components[i].CompareTo(prefix[i]) != 0)
                return false;
        }
        return true;
    }

    public bool IsEmpty => _components.Length == 0;

    public override string ToString()
    {
        if (_components.Length == 0) return "";
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < _components.Length; i++)
        {
            if (i > 0) sb.Append('.');
            sb.Append(_components[i].Number);
            sb.Append(_components[i].Suffix);
        }
        return sb.ToString();
    }

    private static int CompareSuffix(string a, string b)
    {
        bool aEmpty = string.IsNullOrEmpty(a);
        bool bEmpty = string.IsNullOrEmpty(b);
        if (aEmpty && bEmpty) return 0;
        if (aEmpty) return 1;
        if (bEmpty) return -1;
        return string.CompareOrdinal(a, b);
    }

    /// <summary>
    /// Returns true when the supplied string is a valid XSLT package version number,
    /// matching the grammar <c>PackageVersion ::= NumericPart ("-" NamePart)?</c> where
    /// <c>NumericPart</c> is dot-separated integer literals and <c>NamePart</c> is an NCName.
    /// Leading and trailing whitespace is ignored.
    /// </summary>
    public static bool IsValidVersion(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return false;
        version = version!.Trim();

        // Separate optional suffix (first hyphen is the version/suffix separator).
        int dash = version.IndexOf('-');
        string numericPart;
        ReadOnlySpan<char> namePart;
        if (dash >= 0)
        {
            numericPart = version[..dash];
            namePart = version.AsSpan(dash + 1);
            if (namePart.IsEmpty)
                return false;
        }
        else
        {
            numericPart = version;
            namePart = ReadOnlySpan<char>.Empty;
        }

        if (string.IsNullOrEmpty(numericPart))
            return false;

        foreach (var token in numericPart.Split('.', StringSplitOptions.None))
        {
            if (string.IsNullOrEmpty(token))
                return false;
            foreach (char c in token)
            {
                if (!char.IsAsciiDigit(c))
                    return false;
            }
        }

        if (!namePart.IsEmpty)
        {
            var suffix = namePart.ToString();
            if (suffix.Length == 0 || suffix.Contains(':'))
                return false;
            var first = Rune.GetRuneAt(suffix, 0);
            var firstCat = Rune.GetUnicodeCategory(first);
            if (firstCat is UnicodeCategory.DecimalDigitNumber or
                UnicodeCategory.ConnectorPunctuation or
                UnicodeCategory.DashPunctuation)
                return false;
            foreach (Rune rune in suffix.EnumerateRunes())
            {
                var category = Rune.GetUnicodeCategory(rune);
                if (category is UnicodeCategory.PrivateUse or
                    UnicodeCategory.Surrogate or
                    UnicodeCategory.Control)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Returns true when the supplied string is a valid XSLT package version range,
    /// matching the grammar for <c>PackageVersionRange</c>: <c>*</c>, a comma-separated
    /// list of exact versions, prefixes (<c>.*</c>), lower-bounds (<c>+</c>), or
    /// <c>to</c> ranges.
    /// </summary>
    public static bool IsValidVersionRange(string? range)
    {
        if (string.IsNullOrWhiteSpace(range))
            return false;
        range = range!.Trim();
        if (range == "*")
            return true;

        foreach (var item in range.Split(','))
        {
            var itemTrim = item.Trim();
            if (string.IsNullOrEmpty(itemTrim))
                return false;

            // VersionPrefix: PackageVersion ".*"
            if (itemTrim.EndsWith(".*"))
            {
                if (!IsValidVersion(itemTrim[..^2]))
                    return false;
                continue;
            }

            // VersionFrom: PackageVersion "+"
            if (itemTrim.EndsWith("+"))
            {
                if (!IsValidVersion(itemTrim[..^1]))
                    return false;
                continue;
            }

            // VersionTo: "to" S (PackageVersion | VersionPrefix)
            if (itemTrim.StartsWith("to ", StringComparison.Ordinal))
            {
                var upper = itemTrim[3..].Trim();
                if (upper.EndsWith(".*"))
                {
                    if (!IsValidVersion(upper[..^2]))
                        return false;
                }
                else if (!IsValidVersion(upper))
                {
                    return false;
                }
                continue;
            }

            // VersionFromTo: PackageVersion S "to" S (PackageVersion | VersionPrefix)
            var toIndex = itemTrim.IndexOf(" to ", StringComparison.Ordinal);
            if (toIndex >= 0)
            {
                var lower = itemTrim[..toIndex].Trim();
                var upper = itemTrim[(toIndex + 4)..].Trim();
                if (string.IsNullOrEmpty(lower) || !IsValidVersion(lower))
                    return false;
                if (upper.EndsWith(".*"))
                {
                    if (!IsValidVersion(upper[..^2]))
                        return false;
                }
                else if (!IsValidVersion(upper))
                {
                    return false;
                }
                continue;
            }

            // Exact package version.
            if (!IsValidVersion(itemTrim))
                return false;
        }

        return true;
    }
}

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
        PopulateTransformOnly(context);
        context.RegisterFunction(new FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "stream-available",
            Arity = 1,
            ParameterTypes = [XdmValueKind.String],
            ReturnType = XdmValueKind.Boolean,
            Implementation = StreamAvailable_1
        });
        // fn:unparsed-entity-uri / fn:unparsed-entity-public-id: lookup unparsed
        // entity declarations from the context item's document (arity 1) or from the
        // supplied node (arity 2).
        context.RegisterFunction(new FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "unparsed-entity-uri",
            Arity = 1,
            ParameterTypes = [XdmValueKind.String],
            ReturnType = XdmValueKind.String,
            Implementation = UnparsedEntity_Uri_1
        });
        context.RegisterFunction(new FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "unparsed-entity-uri",
            Arity = 2,
            ParameterTypes = [XdmValueKind.String, XdmValueKind.Node],
            ReturnType = XdmValueKind.String,
            Implementation = UnparsedEntity_Uri_2
        });
        context.RegisterFunction(new FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "unparsed-entity-public-id",
            Arity = 1,
            ParameterTypes = [XdmValueKind.String],
            ReturnType = XdmValueKind.String,
            Implementation = UnparsedEntity_PublicId_1
        });
        context.RegisterFunction(new FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "unparsed-entity-public-id",
            Arity = 2,
            ParameterTypes = [XdmValueKind.String, XdmValueKind.Node],
            ReturnType = XdmValueKind.String,
            Implementation = UnparsedEntity_PublicId_2
        });
    }

    private static XdmValue UnparsedEntity_Uri_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => UnparsedEntity_Uri(GetContextNode(ctx, "XTDE1370"), AtomizeString(args[0]));

    private static XdmValue UnparsedEntity_Uri_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => UnparsedEntity_Uri(GetNodeArgument(args[1], "XTDE1370"), AtomizeString(args[0]));

    private static XdmValue UnparsedEntity_PublicId_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => UnparsedEntity_PublicId(GetContextNode(ctx, "XTDE1380"), AtomizeString(args[0]));

    private static XdmValue UnparsedEntity_PublicId_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => UnparsedEntity_PublicId(GetNodeArgument(args[1], "XTDE1380"), AtomizeString(args[0]));

    private static XdmValue UnparsedEntity_Uri(IXdmNode node, string name)
    {
        if (!node.TryGetUnparsedEntity(name, out var systemId, out _))
            return XdmValue.Undefined;
        return string.IsNullOrEmpty(systemId) ? XdmValue.Undefined : XdmValue.FromString(systemId, "anyURI");
    }

    private static XdmValue UnparsedEntity_PublicId(IXdmNode node, string name)
    {
        if (!node.TryGetUnparsedEntity(name, out _, out var publicId))
            return XdmValue.Undefined;
        return string.IsNullOrEmpty(publicId) ? XdmValue.Undefined : XdmValue.FromString(publicId);
    }

    private static IXdmNode GetContextNode(EvaluationContext ctx, string errorCode)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new XsltRuntimeException(errorCode, "There is no context item for unparsed-entity lookup.", XdmValue.Undefined);
        var first = FirstItem(item);
        if (!first.IsNode)
            throw new XsltRuntimeException(errorCode, "The context item is not a node.", XdmValue.Undefined);
        var node = first.NodeValue!;
        if (node.Document is null)
            throw new XsltRuntimeException(errorCode, "The root of the tree containing the context item is not a document node.", XdmValue.Undefined);
        return node;
    }

    private static IXdmNode GetNodeArgument(XdmValue value, string errorCode)
    {
        var first = FirstItem(value);
        if (!first.IsNode)
            throw new XsltRuntimeException(errorCode, "The supplied argument is not a node.", XdmValue.Undefined);
        var node = first.NodeValue!;
        if (node.Document is null)
            throw new XsltRuntimeException(errorCode, "The root of the tree containing the supplied node is not a document node.", XdmValue.Undefined);
        return node;
    }

    private static string AtomizeString(XdmValue value)
    {
        var first = FirstItem(value);
        if (first.IsUndefined)
            return string.Empty;
        if (first.IsNode)
            return first.NodeValue?.StringValue ?? string.Empty;
        return first.ToString();
    }

    /// <summary>
    /// Registers only <c>fn:transform</c> (an F&amp;O function available in every host
    /// language). Used where the XSLT-only functions (<c>fn:stream-available</c>,
    /// <c>fn:unparsed-entity-uri</c>, <c>fn:unparsed-entity-public-id</c>) must remain
    /// unavailable (XPST0017 in pure XPath/XQuery contexts).
    /// </summary>
    public static void PopulateTransformOnly(EvaluationContext context)
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
    /// XSLT 3.0 <c>fn:stream-available($uri)</c>: true when the processor can open a
    /// stream to the (resolved) URI and start reading an XML document — that is, the
    /// resource exists and a root element start tag can be read. Any failure (missing
    /// resource, non-XML content, no root element) yields false; the function never
    /// raises an error. Relative URIs resolve against the static base URI.
    /// </summary>
    private static XdmValue StreamAvailable_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (arg.IsUndefined || (arg.IsSequence && arg.SequenceValue is not null &&
            !arg.SequenceValue.GetEnumerator().MoveNext()))
            return XdmValue.FromBoolean(false);
        string uri = arg.ToString();
        if (uri.Length == 0)
            return XdmValue.FromBoolean(false);
        try
        {
            string resolved = uri;
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                if (string.IsNullOrEmpty(ctx.BaseUri))
                    return XdmValue.FromBoolean(false);
                resolved = new Uri(new Uri(ctx.BaseUri), uri).AbsoluteUri;
            }
            var u = new Uri(resolved);
            if (!u.IsFile)
                return XdmValue.FromBoolean(false);
            var path = u.LocalPath;
            if (!File.Exists(path))
                return XdmValue.FromBoolean(false);
            // Read only as far as the root element start; a truncated document still
            // counts as available (stream-available-004), while non-XML content and
            // DTD-only documents do not (003, 006).
            var settings = new System.Xml.XmlReaderSettings
            {
                DtdProcessing = System.Xml.DtdProcessing.Ignore,
                XmlResolver = null
            };
            using var reader = System.Xml.XmlReader.Create(path, settings);
            while (reader.Read())
            {
                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                    return XdmValue.FromBoolean(true);
            }
            return XdmValue.FromBoolean(false);
        }
        catch
        {
            return XdmValue.FromBoolean(false);
        }
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
        transformContext.IsXsltMode = true;
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
            var location = ResolvePackageLocation(packageName, versionRange, PackageVersionResolutionStrategy.Highest);
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
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            // The stylesheet module cannot be retrieved: FOXT0002 (transform-001).
            throw new InvalidOperationException($"FOXT0002: Failed to retrieve stylesheet '{displayName ?? resolvedUri}': {ex.Message}");
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
    /// choosing a matching registered version according to the supplied strategy.
    /// </summary>
    internal static string? ResolvePackageLocation(string name, string versionRange, PackageVersionResolutionStrategy strategy = PackageVersionResolutionStrategy.Highest)
    {
        List<(string Name, string Version, string Location)> snapshot;
        lock (_packageRegistry)
        {
            snapshot = _packageRegistry.Where(p => p.Name == name).ToList();
        }

        string? bestLocation = null;
        PackageVersion? bestVersion = null;
        foreach (var candidate in snapshot)
        {
            if (!VersionMatches(candidate.Version, versionRange))
                continue;
            var parsed = PackageVersion.Parse(candidate.Version);
            if (bestVersion == null ||
                (strategy == PackageVersionResolutionStrategy.Highest && parsed.CompareTo(bestVersion.Value) > 0) ||
                (strategy == PackageVersionResolutionStrategy.Lowest && parsed.CompareTo(bestVersion.Value) < 0))
            {
                bestVersion = parsed;
                bestLocation = candidate.Location;
            }
        }
        return bestLocation;
    }

    /// <summary>
    /// Matches a concrete version against an XSLT package version range
    /// (e.g. <c>1.0.2</c>, <c>1.*</c>, <c>1.0-2.0</c>, <c>1.0 to 2.0</c>,
    /// <c>1.5+</c>, <c>1.0.0, 2.0</c>, <c>*</c>).
    /// </summary>
    private static bool VersionMatches(string version, string range)
    {
        range = range.Trim();
        if (range == "*" || range.Length == 0)
            return true;

        // A package without an explicit version is considered versionless and matches
        // any concrete version request. This aligns with the W3C package-version tests.
        if (string.IsNullOrWhiteSpace(version))
            return true;

        // Comma-separated list of version specifiers (any match wins).
        if (range.Contains(','))
        {
            foreach (var item in range.Split(','))
            {
                if (VersionMatchesSingle(version, item.Trim()))
                    return true;
            }
            return false;
        }

        return VersionMatchesSingle(version, range);
    }

    private static bool VersionMatchesSingle(string version, string range)
    {
        range = range.Trim();
        if (range == "*" || range.Length == 0)
            return true;

        var parsedVersion = PackageVersion.Parse(version);

        // XSLT package version range using "to": "a to b", "to b", "a to".
        var toIndex = range.IndexOf(" to ", StringComparison.Ordinal);
        if (toIndex >= 0)
        {
            var lower = range[..toIndex].Trim();
            var upper = range[(toIndex + 4)..].Trim();
            bool okLower = string.IsNullOrEmpty(lower) || parsedVersion.CompareTo(PackageVersion.Parse(lower)) >= 0;
            bool okUpper = string.IsNullOrEmpty(upper) || parsedVersion.CompareTo(PackageVersion.Parse(upper)) <= 0;
            return okLower && okUpper;
        }

        // "VersionFrom+" form: 1.5+ means >= 1.5.
        if (range.EndsWith('+'))
        {
            var bound = range[..^1].Trim();
            return parsedVersion.CompareTo(PackageVersion.Parse(bound)) >= 0;
        }

        // "to b" form (omitted lower bound).
        if (range.StartsWith("to ", StringComparison.Ordinal))
        {
            var upper = range[3..].Trim();
            return parsedVersion.CompareTo(PackageVersion.Parse(upper)) <= 0;
        }

        // Wildcard prefix: 3.5.* matches any version starting with 3.5.
        if (range.EndsWith(".*", StringComparison.Ordinal))
        {
            var prefix = PackageVersion.Parse(range[..^2]);
            return parsedVersion.StartsWith(prefix);
        }

        // Exact/prefix version: 2.0 matches 2.0.0 etc.
        var exact = PackageVersion.Parse(range);
        return parsedVersion.StartsWith(exact);
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

        // FOXT0002: no primary input at all (no source-node, initial-match-selection,
        // initial-template, initial-mode, or initial-function).
        if (sourceNode == null && initialMatchSelection == null
            && string.IsNullOrEmpty(initialTemplate) && string.IsNullOrEmpty(initialMode) && string.IsNullOrEmpty(initialFunction))
            throw new InvalidOperationException("FOXT0002: fn:transform requires a source-node, initial-match-selection, or initial-template option.");

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
