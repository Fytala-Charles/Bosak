// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Inspection engine: parses retained envelopes, builds descriptors, walks modules.
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — source-preserving authoring API for visual designers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 10-10-2026     | REQ-124 Slice B: retain effective resolver/options on the snapshot for re-inspection     |
//                      | Charles Korthout | 0.3   | 10-10-2026     | REQ-124 review finding 4: compile-time resolution bridged through the inspection resolver|
//                      | Charles Korthout | 0.4   | 10-10-2026     | REQ-124 acceptance F1: bridge extracted to shared AuthoringModuleUriResolverBridge       |
//                      | Charles Korthout | 0.5   | 13-10-2026     | Capture applicable static namespace bindings during inspection                         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 13-10-2026     | Capture static context for variable and parameter name slots                           |
//                      | Charles Korthout | 0.7   | 13-10-2026     | Align named template QNames with applicable namespaces.                                  |
//                      | Charles Korthout | 0.8   | 13-10-2026     | Align function QNames with applicable fixed namespaces.                                 |
//                      | Charles Korthout | 0.9   | 13-10-2026     | Capture standalone named-type contexts for authoring validation.                         |
//                      | Charles Korthout | 0.10  | 13-10-2026     | Validate declaration sequence types in captured authoring contexts.                       |
//                      | Charles Korthout | 0.11  | 13-10-2026     | Link captured named types using include and import precedence.                           |
//                      | Charles Korthout | 0.12  | 13-10-2026     | Capture internal contexts for mode names, lists and default-mode references.             |
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Linq;
using Bosak.Xslt.Api;

namespace Bosak.Xslt.Authoring;

/// <summary>
/// Performs source-aware inspection of a retained principal module and, through a caller-supplied
/// <see cref="IAuthoringModuleResolver"/>, the modules it includes or imports. Inspection derives a
/// parsed working document but never re-serializes the source: the original bytes remain the only
/// emission source (see <see cref="AuthoringSnapshot.ExportOriginal"/>). Structural failures
/// (malformed XML, unsupported encodings, invalid bytes) are classified and returned as data; semantic
/// (compilation) outcomes are recorded on the snapshot and never fail inspection. Prefer the static
/// <see cref="XsltAuthoring.Inspect"/> entry point.
/// </summary>
public sealed class AuthoringInspector
{
    private static readonly XNamespace XsltNs = AttributeSlotClassifier.XsltNamespace;
    private static readonly XNamespace XmlNs = XNamespace.Xml;

    /// <summary>
    /// Inspects the principal module and the modules reachable from it.
    /// </summary>
    /// <param name="principal">The retained source envelope of the principal module.</param>
    /// <param name="resolver">The module resolver; <see langword="null"/> uses <see cref="FileSystemAuthoringModuleResolver"/>.</param>
    /// <param name="options">Optional inspection controls.</param>
    /// <returns>A success result carrying the snapshot, or a classified failure.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principal"/> is null.</exception>
    public AuthoringInspectionResult Inspect(
        AuthoringSource principal,
        IAuthoringModuleResolver? resolver = null,
        AuthoringInspectionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(principal);

        options ??= new AuthoringInspectionOptions();
        resolver ??= new FileSystemAuthoringModuleResolver();

        var principalState = ParseModule(principal, out var principalFailure);
        if (principalState is null)
        {
            return AuthoringInspectionResult.FailureResult(principalFailure!);
        }

        var visited = new Dictionary<string, ModuleState>(StringComparer.OrdinalIgnoreCase)
        {
            [IdentityKey(principal.BaseUri)] = principalState,
        };
        var path = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ordered = new List<ModuleState> { principalState };

        VisitModule(principalState);
        void VisitModule(ModuleState state)
        {
            path.Add(IdentityKey(state.Source.BaseUri));
            try
            {
                foreach (var pending in ScanModuleEdges(state))
                {
                    var resolvedUri = pending.ResolvedUri;
                    if (resolvedUri is null)
                    {
                        state.Edges.Add(pending.ToEdge(reusesExisting: false));
                        continue;
                    }

                    var key = IdentityKey(resolvedUri);
                    if (path.Contains(key))
                    {
                        state.Edges.Add(pending.ToEdge(
                            reusesExisting: false,
                            diagnostic: new AuthoringFailure(
                                AuthoringFailureKind.ResolverFailure,
                                $"The include/import from '{state.Source.BaseUri}' to '{resolvedUri}' forms a cycle; the reference was not followed.",
                                state.Source.BaseUri,
                                pending.ReferenceRange)));
                        continue;
                    }

                    if (visited.ContainsKey(key))
                    {
                        state.Edges.Add(pending.ToEdge(reusesExisting: true));
                        continue;
                    }

                    if (ordered.Count >= options.MaxModuleCount)
                    {
                        state.Edges.Add(pending.ToEdge(
                            reusesExisting: false,
                            diagnostic: new AuthoringFailure(
                                AuthoringFailureKind.ResolverFailure,
                                $"The module limit ({options.MaxModuleCount}) was reached; '{resolvedUri}' was not inspected.",
                                state.Source.BaseUri,
                                pending.ReferenceRange)));
                        continue;
                    }

                    AuthoringSource? moduleSource;
                    try
                    {
                        moduleSource = resolver.ResolveModule(resolvedUri, state.Source.BaseUri);
                    }
                    catch (Exception ex)
                    {
                        state.Edges.Add(pending.ToEdge(
                            reusesExisting: false,
                            diagnostic: new AuthoringFailure(
                                AuthoringFailureKind.ResolverFailure,
                                $"The module resolver threw while resolving '{resolvedUri}': {ex.Message}",
                                state.Source.BaseUri,
                                pending.ReferenceRange)));
                        continue;
                    }

                    if (moduleSource is null)
                    {
                        state.Edges.Add(pending.ToEdge(
                            reusesExisting: false,
                            diagnostic: new AuthoringFailure(
                                AuthoringFailureKind.ResolverFailure,
                                $"The module '{resolvedUri}' could not be resolved; the reference was recorded as an unresolved edge.",
                                state.Source.BaseUri,
                                pending.ReferenceRange)));
                        continue;
                    }

                    var moduleState = ParseModule(moduleSource, out var moduleFailure);
                    if (moduleState is null)
                    {
                        state.Edges.Add(pending.ToEdge(reusesExisting: false, diagnostic: moduleFailure!));
                        continue;
                    }

                    visited[key] = moduleState;
                    ordered.Add(moduleState);
                    state.Edges.Add(pending.ToEdge(reusesExisting: false));
                    VisitModule(moduleState);
                }
            }
            finally
            {
                path.Remove(IdentityKey(state.Source.BaseUri));
            }
        }

        var linkedModules = new List<(XElement Root, string BaseUri, int Precedence)>();
        string? graphGap = null;
        int importRank = 0;
        void CaptureModuleOrder(ModuleState state, int precedence)
        {
            if (graphGap is not null || state.Document.Root is not { } moduleRoot) return;
            if (linkedModules.Count >= 10000)
            {
                graphGap = "Named-type module occurrence limit reached; linkage is deferred.";
                return;
            }
            if (moduleRoot.Attribute("use-when")?.Value.Trim() is "false" or "false()") return;
            linkedModules.Add((moduleRoot, state.Source.BaseUri.AbsoluteUri, precedence));
            var references = state.Document.Descendants().Where(e => e.Name.NamespaceName == AttributeSlotClassifier.XsltNamespace
                && e.Name.LocalName is "import" or "include").ToList();
            foreach (var reference in moduleRoot.Elements().Reverse().Where(references.Contains))
            {
                var edge = state.Edges[references.IndexOf(reference)];
                if (reference.Attribute("use-when") is not null || reference.Attribute("_href") is not null ||
                    edge.Diagnostic is not null || edge.ResolvedUri is null || !visited.TryGetValue(IdentityKey(edge.ResolvedUri), out var child))
                {
                    graphGap = "Conditional, shadow, unresolved or cyclic module edges are outside named-type linkage coverage.";
                    return;
                }
                CaptureModuleOrder(child, edge.Kind == AuthoringModuleEdgeKind.Import ? ++importRank : precedence);
            }
        }
        CaptureModuleOrder(principalState, 0);
        var namedTypes = AuthoringNamedTypes.Capture(linkedModules, new AuthoringModuleUriResolverBridge(resolver), graphGap);

        var nodeIds = new Dictionary<XObject, int>();
        var descriptors = new List<AuthoringModuleDescriptor>(ordered.Count);
        var modulesByUri = new Dictionary<Uri, AuthoringModuleDescriptor>();
        var sourcesByUri = new Dictionary<Uri, AuthoringSource>();
        try
        {
            for (var i = 0; i < ordered.Count; i++)
            {
                var state = ordered[i];
                var builder = new DescriptorBuilder(state, nodeIds, resolver, namedTypes);
                var root = builder.BuildDocument(state.Document);
                var version = DetectVersion(state.Document);
                var descriptor = new AuthoringModuleDescriptor(
                    state.Source.BaseUri,
                    isPrincipal: i == 0,
                    state.Source.Encoding.EncodingName,
                    state.Source.OriginalBytes.Length,
                    version,
                    state.Edges,
                    root,
                    precedence: i, namedTypes: builder.NamedTypes);
                descriptors.Add(descriptor);
                modulesByUri[state.Source.BaseUri] = descriptor;
                sourcesByUri[state.Source.BaseUri] = state.Source;
            }
        }
        catch (InvalidDataException ex)
        {
            // The derived document and the retained source could not be aligned; classify, never throw.
            return AuthoringInspectionResult.FailureResult(
                new AuthoringFailure(AuthoringFailureKind.Structure, ex.Message, principal.BaseUri));
        }

        var isCompilable = false;
        var compilationDiagnostics = Array.Empty<string>();
        if (options.AttemptCompilation)
        {
            // Derived compilation must resolve include/import through the SAME caller-controlled
            // resolver inspection used — never implicitly through the file system (REQ-124 review
            // finding 4). The default file-system resolver keeps the compiler's default behavior.
            var compiler = new XsltCompiler();
            if (resolver is not FileSystemAuthoringModuleResolver)
            {
                compiler.UriResolver = new AuthoringModuleUriResolverBridge(resolver);
            }

            try
            {
                _ = compiler.Compile(principalState.Document, principal.BaseUri.AbsoluteUri);
                isCompilable = true;
            }
            catch (Exception ex)
            {
                compilationDiagnostics = new[] { $"{ex.GetType().Name}: {ex.Message}" };
            }
        }

        var snapshot = new AuthoringSnapshot(
            descriptors,
            modulesByUri,
            sourcesByUri,
            nodeIds,
            isCompilable,
            compilationDiagnostics,
            resolver,
            options);
        return AuthoringInspectionResult.SuccessResult(snapshot);
    }

    private static string IdentityKey(Uri uri) => uri.AbsoluteUri;

    private static AuthoringFailure StructureFailure(AuthoringSource source, Exception ex)
    {
        return ex is XmlException xmlException
            ? new AuthoringFailure(
                AuthoringFailureKind.Structure,
                $"The module is not well-formed XML: {xmlException.Message}",
                source.BaseUri,
                MapXmlExceptionRange(source, xmlException))
            : new AuthoringFailure(
                AuthoringFailureKind.Structure,
                $"The module could not be parsed as XML: {ex.Message}",
                source.BaseUri);
    }

    private static SourceRange? MapXmlExceptionRange(AuthoringSource source, XmlException ex)
    {
        try
        {
            var map = source.CoordinateMap;
            // XmlException line/position are 1-based (LineNumber starts at 1), matching IXmlLineInfo.
            var offset = map.CharOffsetFromLineColumn(ex.LineNumber, ex.LinePosition);
            return map.RangeFromCharOffsets(offset, offset);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static ModuleState? ParseModule(AuthoringSource source, out AuthoringFailure? failure)
    {
        try
        {
            var document = XDocument.Parse(
                source.Text,
                LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            failure = null;
            return new ModuleState { Source = source, Document = document };
        }
        catch (Exception ex) when (ex is XmlException or InvalidDataException or ArgumentException)
        {
            failure = StructureFailure(source, ex);
            return null;
        }
    }

    private static string? DetectVersion(XDocument document)
    {
        var root = document.Root;
        if (root is null)
        {
            return null;
        }

        var version = root.Attribute("version") ?? root.Attribute(XsltNs + "version");
        return version?.Value;
    }

    private static Uri ResolveElementBaseUri(XElement element, Uri moduleBaseUri)
    {
        var stack = new List<XElement>();
        for (var current = element; current is not null; current = current.Parent)
        {
            stack.Insert(0, current);
        }

        var baseUri = moduleBaseUri;
        foreach (var ancestor in stack)
        {
            var xmlBase = ancestor.Attribute(XmlNs + "base");
            if (xmlBase is not null)
            {
                baseUri = new Uri(baseUri, xmlBase.Value);
            }
        }

        return baseUri;
    }

    private static List<PendingEdge> ScanModuleEdges(ModuleState state)
    {
        var pendingEdges = new List<PendingEdge>();
        foreach (var element in state.Document.Descendants())
        {
            if (element.Name.NamespaceName != AttributeSlotClassifier.XsltNamespace)
            {
                continue;
            }

            var isImport = element.Name.LocalName == "import";
            if (!isImport && element.Name.LocalName != "include")
            {
                continue;
            }

            var referenceRange = ComputeElementRange(state, element);
            var href = ReadRawAttributeValue(state, referenceRange, "href");
            Uri? resolvedUri = null;
            AuthoringFailure? diagnostic = null;

            if (href is null)
            {
                diagnostic = new AuthoringFailure(
                    AuthoringFailureKind.Structure,
                    $"The <xsl:{element.Name.LocalName}> element has no href attribute.",
                    state.Source.BaseUri,
                    referenceRange);
            }
            else
            {
                try
                {
                    var elementBase = ResolveElementBaseUri(element, state.Source.BaseUri);
                    resolvedUri = new Uri(elementBase, href.Trim());
                }
                catch (UriFormatException)
                {
                    diagnostic = new AuthoringFailure(
                        AuthoringFailureKind.ResolverFailure,
                        $"The href \"{href}\" is not a valid URI reference.",
                        state.Source.BaseUri,
                        referenceRange);
                }
            }

            pendingEdges.Add(new PendingEdge(
                isImport ? AuthoringModuleEdgeKind.Import : AuthoringModuleEdgeKind.Include,
                href ?? string.Empty,
                resolvedUri,
                referenceRange,
                diagnostic));
        }

        return pendingEdges;
    }

    private static SourceRange ComputeElementRange(ModuleState state, XElement element)
    {
        var map = state.Source.CoordinateMap;
        var text = map.Text;
        var lineInfo = (IXmlLineInfo)element;
        // IXmlLineInfo for an element points just past its qualified name; walk back over the name
        // characters to the '<'.
        var offset = map.CharOffsetFromLineColumn(lineInfo.LineNumber, lineInfo.LinePosition);
        while (offset > 0 && IsNameCharacter(text[offset - 1]))
        {
            offset--;
        }

        var startOffset = offset - 1;
        var endOffset = SourceTagScanner.FindElementEnd(text, startOffset, out _, out _);
        return map.RangeFromCharOffsets(startOffset, endOffset);

        static bool IsNameCharacter(char c) =>
            !char.IsWhiteSpace(c) && c is not '<' and not '>' and not '/' and not '=' and not '"' and not '\'';
    }

    private static string? ReadRawAttributeValue(ModuleState state, SourceRange elementRange, string name)
    {
        var map = state.Source.CoordinateMap;
        var text = map.Text;
        var startOffset = map.CharOffsetFromLineColumn(elementRange.StartLine, elementRange.StartColumn);
        var elementEnd = SourceTagScanner.FindElementEnd(text, startOffset, out var startTagEnd, out _);
        foreach (var scanned in SourceTagScanner.ScanAttributes(text, startOffset, startTagEnd))
        {
            if (string.Equals(
                    text.Substring(scanned.NameStart, scanned.NameEnd - scanned.NameStart),
                    name,
                    StringComparison.Ordinal)
                && scanned.ValueStart <= scanned.ValueEnd)
            {
                return text.Substring(scanned.ValueStart, scanned.ValueEnd - scanned.ValueStart);
            }
        }

        return null;
    }

    private sealed class ModuleState
    {
        public required AuthoringSource Source { get; init; }
        public required XDocument Document { get; init; }
        public List<AuthoringModuleEdge> Edges { get; } = new();
    }

    private sealed class PendingEdge
    {
        private readonly AuthoringModuleEdgeKind _kind;
        private readonly string _hrefLiteral;
        private readonly Uri? _resolvedUri;
        private readonly SourceRange _referenceRange;
        private readonly AuthoringFailure? _diagnostic;

        public PendingEdge(AuthoringModuleEdgeKind kind, string hrefLiteral, Uri? resolvedUri, SourceRange referenceRange, AuthoringFailure? diagnostic)
        {
            _kind = kind;
            _hrefLiteral = hrefLiteral;
            _resolvedUri = resolvedUri;
            _referenceRange = referenceRange;
            _diagnostic = diagnostic;
        }

        public SourceRange ReferenceRange => _referenceRange;

        public Uri? ResolvedUri => _resolvedUri;

        public AuthoringModuleEdge ToEdge(bool reusesExisting, AuthoringFailure? diagnostic = null)
            => new(_kind, _hrefLiteral, _resolvedUri, _referenceRange, diagnostic ?? _diagnostic, reusesExisting);
    }

    /// <summary>
    /// Builds the immutable descriptor tree for one module from its derived document and the retained
    /// source text. All ranges are computed through the module's <see cref="SourceCoordinateMap"/> so
    /// line/column and byte coordinates stay consistent.
    /// </summary>
    private sealed class DescriptorBuilder
    {
        private readonly ModuleState _state;
        private readonly SourceCoordinateMap _map;
        private readonly string _text;
        private readonly Dictionary<XObject, int> _nodeIds;
        private int _nextId;
        private readonly AuthoringModuleUriResolverBridge _namespaceResolver;
        private string? _staticNamespaceError;

        public DescriptorBuilder(ModuleState state, Dictionary<XObject, int> nodeIds, IAuthoringModuleResolver resolver, AuthoringNamedTypes namedTypes)
        {
            _namespaceResolver = new AuthoringModuleUriResolverBridge(resolver);
            NamedTypes = namedTypes;
            if (NamedTypes.Error?.StartsWith("XTSE0122", StringComparison.Ordinal) == true && NamedTypes.ErrorElement?.Document == state.Document)
                _staticNamespaceError = NamedTypes.Error;
            _state = state;
            _map = state.Source.CoordinateMap;
            _text = _map.Text;
            _nodeIds = nodeIds;
        }

        internal AuthoringNamedTypes NamedTypes { get; }

        public AuthoringNodeDescriptor BuildDocument(XDocument document)
        {
            // XDocument represents the XML declaration as XDocument.Declaration, not as a node; advance
            // the source cursor past it so node and source stay aligned.
            var cursor = 0;
            if (document.Declaration is not null &&
                _text.StartsWith("<?xml", StringComparison.Ordinal))
            {
                var declarationEnd = _text.IndexOf("?>", StringComparison.Ordinal);
                if (declarationEnd >= 0)
                {
                    cursor = declarationEnd + 2;
                }
            }

            var children = new List<AuthoringNodeDescriptor>();
            foreach (var node in document.Nodes())
            {
                var built = BuildNode(node, ref cursor);
                if (built is not null)
                {
                    children.Add(built);
                }
            }

            var range = _map.RangeFromCharOffsets(0, _text.Length);
            return new AuthoringNodeDescriptor(
                NextId(document),
                AuthoringNodeKind.Document,
                elementName: null,
                text: null,
                range,
                Array.Empty<AuthoringAttributeDescriptor>(),
                children,
                backingObject: null);
        }

        private int NextId(XObject? obj)
        {
            var id = _nextId++;
            if (obj is not null)
            {
                _nodeIds[obj] = id;
            }

            return id;
        }

        private AuthoringNodeDescriptor? BuildNode(XNode node, ref int cursor) => node switch
        {
            XElement element => BuildElement(element, ref cursor),
            XText text => BuildText(text, ref cursor),
            XComment comment => BuildComment(comment, ref cursor),
            XProcessingInstruction pi => BuildProcessingInstruction(pi, ref cursor),
            _ => null,
        };

        private AuthoringNodeDescriptor BuildElement(XElement element, ref int cursor)
        {
            if (cursor >= _text.Length || _text[cursor] != '<')
            {
                throw new InvalidDataException("Derived document and retained source are misaligned.");
            }

            var startOffset = cursor;
            var nameAsWritten = SourceTagScanner.ReadElementName(_text, startOffset);
            var elementEnd = SourceTagScanner.FindElementEnd(_text, startOffset, out var startTagEnd, out _);
            var range = _map.RangeFromCharOffsets(startOffset, elementEnd);

            var children = new List<AuthoringNodeDescriptor>();
            var childCursor = startTagEnd;
            foreach (var child in element.Nodes())
            {
                var built = BuildNode(child, ref childCursor);
                if (built is not null)
                {
                    children.Add(built);
                }
            }

            cursor = elementEnd;
            return new AuthoringNodeDescriptor(
                NextId(element),
                AuthoringNodeKind.Element,
                SplitRawName(nameAsWritten, element.Name.NamespaceName, element.Name.LocalName),
                text: null,
                range,
                BuildAttributes(element, startOffset, startTagEnd),
                children,
                element);
        }

        private IReadOnlyList<AuthoringAttributeDescriptor> BuildAttributes(XElement element, int startOffset, int startTagEnd)
        {
            var elementNameAsWritten = SourceTagScanner.ReadElementName(_text, startOffset);
            var scanned = SourceTagScanner.ScanAttributes(_text, startOffset, startTagEnd);
            if (scanned.Count == 0)
            {
                return Array.Empty<AuthoringAttributeDescriptor>();
            }

            var domAttributes = element.Attributes().ToList();
            var descriptors = new List<AuthoringAttributeDescriptor>(scanned.Count);
            for (var i = 0; i < scanned.Count; i++)
            {
                var scan = scanned[i];
                var rawName = _text.Substring(scan.NameStart, scan.NameEnd - scan.NameStart);
                var dom = i < domAttributes.Count ? domAttributes[i] : null;
                if (dom is not null && !NamesMatch(dom, rawName))
                {
                    dom = domAttributes.FirstOrDefault(a => NamesMatch(a, rawName));
                }

                var rawLiteral = _text.Substring(scan.ValueStart, scan.ValueEnd - scan.ValueStart);
                var split = SplitRawName(rawName, dom?.Name.NamespaceName ?? string.Empty, LocalPartOf(rawName));
                var slotKind = AttributeSlotClassifier.Classify(
                    element.Name.NamespaceName,
                    element.Name.LocalName,
                    split.LocalName,
                    split.NamespaceUri,
                    rawLiteral);

                ExpressionSlotContext? slotContext = slotKind is AuthoringAttributeSlotKind.Expression
                    or AuthoringAttributeSlotKind.Pattern
                    or AuthoringAttributeSlotKind.Avt
                    || (slotKind == AuthoringAttributeSlotKind.QName && element.Name.Namespace == XsltNs
                        && element.Name.LocalName is "variable" or "param" or "with-param" or "template" or "call-template" or "function" or "mode")
                    || (split.LocalName == "mode" && split.NamespaceUri.Length == 0 && element.Name.Namespace == XsltNs
                        && element.Name.LocalName is "template" or "apply-templates")
                    || (split.LocalName == "default-mode" && (element.Name.Namespace == XsltNs
                        ? split.NamespaceUri.Length == 0 : split.NamespaceUri == XsltNs.NamespaceName))
                    || (slotKind == AuthoringAttributeSlotKind.Plain && split.LocalName == "as" && split.NamespaceUri.Length == 0
                        && element.Name.Namespace == XsltNs && AttributeSlotClassifier.HasDeclarationSequenceType(element.Name.LocalName))
                    ? BuildSlotContext(element, rawName, elementNameAsWritten)
                    : null;

                descriptors.Add(new AuthoringAttributeDescriptor(
                    rawName,
                    split.Prefix,
                    split.NamespaceUri,
                    split.LocalName,
                    slotKind,
                    rawLiteral,
                    dom?.Value ?? rawLiteral,
                    _map.RangeFromCharOffsets(scan.ValueStart, scan.ValueEnd),
                    _map.RangeFromCharOffsets(scan.NameStart, scan.FullEnd),
                    slotContext));
            }

            return descriptors;
        }

        private ExpressionSlotContext BuildSlotContext(XElement element, string attributeName, string elementNameAsWritten)
        {
            var stack = new List<XElement>();
            for (var current = element; current is not null; current = current.Parent)
            {
                stack.Insert(0, current);
            }

            var bindings = new List<AuthoringNamespaceBinding>();
            foreach (var ancestor in stack)
            {
                foreach (var attr in ancestor.Attributes())
                {
                    if (attr.IsNamespaceDeclaration)
                    {
                        var prefix = attr.Name.LocalName == "xmlns" ? string.Empty : attr.Name.LocalName;
                        bindings.Add(new AuthoringNamespaceBinding(prefix, attr.Value));
                    }
                }
            }

            if (!bindings.Any(b => b.Prefix == "xml"))
            {
                bindings.Add(new AuthoringNamespaceBinding("xml", "http://www.w3.org/XML/1998/namespace"));
            }

            string? xpathDefaultNamespace = null;
            string? version = null;
            var baseUri = _state.Source.BaseUri;
            foreach (var ancestor in stack)
            {
                var xmlBase = ancestor.Attribute(XmlNs + "base");
                if (xmlBase is not null)
                {
                    baseUri = new Uri(baseUri, xmlBase.Value);
                }
            }

            for (var current = element; current is not null; current = current.Parent)
            {
                xpathDefaultNamespace ??= current.Attribute("xpath-default-namespace")?.Value;
                version ??= (current.Attribute("version") ?? current.Attribute(XsltNs + "version"))?.Value;
            }

            IReadOnlyDictionary<string, string>? staticNamespaces = null;
            if (double.TryParse(version, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var effectiveVersion) && effectiveVersion >= 4.0)
            {
                try
                {
                    if (_staticNamespaceError is null)
                        staticNamespaces = Stylesheet.Stylesheet.GetApplicableStaticNamespaces(
                            element, _namespaceResolver, _state.Source.BaseUri.AbsoluteUri);
                }
                catch (InvalidOperationException ex)
                {
                    // Preserve inspection/source descriptors; validation reports the static error.
                    _staticNamespaceError = ex.Message;
                }
            }

            return new ExpressionSlotContext(
                SplitRawName(elementNameAsWritten, element.Name.NamespaceName, element.Name.LocalName),
                attributeName,
                bindings,
                xpathDefaultNamespace,
                baseUri,
                version ?? "3.0",
                staticNamespaces,
                effectiveVersion >= 4.0 ? _staticNamespaceError : null);
        }

        private AuthoringNodeDescriptor BuildText(XText text, ref int cursor)
        {
            var startOffset = cursor;
            int endOffset;
            if (text is XCData)
            {
                var terminator = _text.IndexOf("]]>", startOffset, StringComparison.Ordinal);
                endOffset = terminator < 0 ? _text.Length : terminator + 3;
            }
            else
            {
                var markup = _text.IndexOf('<', startOffset);
                endOffset = markup < 0 ? _text.Length : markup;
            }

            cursor = endOffset;
            return new AuthoringNodeDescriptor(
                NextId(text),
                AuthoringNodeKind.Text,
                elementName: null,
                text.Value,
                _map.RangeFromCharOffsets(startOffset, endOffset),
                Array.Empty<AuthoringAttributeDescriptor>(),
                Array.Empty<AuthoringNodeDescriptor>(),
                text);
        }

        private AuthoringNodeDescriptor BuildComment(XComment comment, ref int cursor)
        {
            var startOffset = cursor;
            var terminator = _text.IndexOf("-->", startOffset, StringComparison.Ordinal);
            var endOffset = terminator < 0 ? _text.Length : terminator + 3;
            cursor = endOffset;
            return new AuthoringNodeDescriptor(
                NextId(comment),
                AuthoringNodeKind.Comment,
                elementName: null,
                comment.Value,
                _map.RangeFromCharOffsets(startOffset, endOffset),
                Array.Empty<AuthoringAttributeDescriptor>(),
                Array.Empty<AuthoringNodeDescriptor>(),
                comment);
        }

        private AuthoringNodeDescriptor BuildProcessingInstruction(XProcessingInstruction pi, ref int cursor)
        {
            var startOffset = cursor;
            var terminator = _text.IndexOf("?>", startOffset, StringComparison.Ordinal);
            var endOffset = terminator < 0 ? _text.Length : terminator + 2;
            cursor = endOffset;
            return new AuthoringNodeDescriptor(
                NextId(pi),
                AuthoringNodeKind.ProcessingInstruction,
                elementName: null,
                pi.Data,
                _map.RangeFromCharOffsets(startOffset, endOffset),
                Array.Empty<AuthoringAttributeDescriptor>(),
                Array.Empty<AuthoringNodeDescriptor>(),
                pi);
        }

        private static AuthoringQName SplitRawName(string rawName, string namespaceUri, string fallbackLocalName)
        {
            var colon = rawName.IndexOf(':');
            return colon < 0
                ? new AuthoringQName(string.Empty, namespaceUri, fallbackLocalName)
                : new AuthoringQName(rawName[..colon], namespaceUri, rawName[(colon + 1)..]);
        }

        private static string LocalPartOf(string rawName)
        {
            var colon = rawName.IndexOf(':');
            return colon < 0 ? rawName : rawName[(colon + 1)..];
        }

        private static bool NamesMatch(XAttribute attribute, string rawName)
        {
            if (attribute.IsNamespaceDeclaration)
            {
                return rawName == "xmlns" ||
                       (rawName.StartsWith("xmlns:", StringComparison.Ordinal) &&
                        attribute.Name.LocalName == rawName["xmlns:".Length..]);
            }

            var colon = rawName.IndexOf(':');
            if (colon < 0)
            {
                return attribute.Name.NamespaceName.Length == 0 && attribute.Name.LocalName == rawName;
            }

            return attribute.Name.LocalName == rawName[(colon + 1)..];
        }
    }
}
