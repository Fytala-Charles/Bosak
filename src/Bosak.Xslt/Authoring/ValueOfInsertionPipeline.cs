// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 October 2026
// PURPOSE              : Validate and splice a single value-of instruction without touching retained source bytes.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 15-10-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 15-10-2026     | Resolve eligibility from retained source rather than a previously compiled backing tree. |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Runtime.Resources;
using Bosak.Xslt.Api;
using Bosak.Xslt.Validation;

namespace Bosak.Xslt.Authoring;

internal static class ValueOfInsertionPipeline
{
    private static readonly XNamespace Xsl = "http://www.w3.org/1999/XSL/Transform";
    private static readonly HashSet<string> CoveredInstructions = new(StringComparer.Ordinal)
    {
        "stylesheet", "transform", "output", "template", "param", "variable", "function",
        "value-of", "sequence", "text", "if", "choose", "when", "otherwise", "for-each",
        "comment", "processing-instruction",
    };

    internal static AuthoringValueOfInsertionTargetResult Target(AuthoringSnapshot snapshot,
        AuthoringNodeDescriptor parent, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            if (!ReferenceEquals(snapshot.FindNodeById(parent.Id), parent))
                return new(null, Failure(AuthoringInsertionFailureKind.ForeignSnapshot, "The parent belongs to another snapshot."));
            var failure = CheckScope(snapshot, parent, token, out var offset, out var contextElement);
            if (failure is not null) return new(null, failure);
            failure = Validate(snapshot, false, token);
            token.ThrowIfCancellationRequested();
            return failure is null ? new(new(snapshot, parent, offset, contextElement!), null) : new(null, failure);
        }
        catch (OperationCanceledException) { return new(null, Cancelled()); }
    }

    internal static AuthoringValueOfInsertionResult Run(AuthoringSnapshot snapshot,
        AuthoringValueOfInsertionProposal proposal, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            var target = proposal.Target;
            if (!ReferenceEquals(target.Owner, snapshot))
                return new(null, Failure(AuthoringInsertionFailureKind.ForeignSnapshot, "The target belongs to another snapshot."));
            var checkedTarget = Target(snapshot, target.Parent, token);
            if (!checkedTarget.IsSuccess) return new(null, checkedTarget.Failure);
            if (target.CharacterOffset != checkedTarget.Target!.CharacterOffset)
                return new(null, Failure(AuthoringInsertionFailureKind.UnknownTarget, "The target no longer aligns with its source."));
            var text = proposal.SelectExpressionText;
            if (string.IsNullOrWhiteSpace(text))
                return new(null, Failure(AuthoringInsertionFailureKind.InvalidExpression, "A select expression is required."));
            try { XmlConvert.VerifyXmlChars(text); }
            catch (XmlException ex) { return new(null, Failure(AuthoringInsertionFailureKind.InvalidExpression, ex.Message)); }

            var parent = checkedTarget.Target.ContextElement;
            var bindings = NamespaceBindings(parent);
            var prefix = bindings.Where(pair => pair.Key.Length != 0 && pair.Value == Xsl.NamespaceName)
                .Select(pair => pair.Key).OrderBy(p => p == "xsl" ? 0 : 1).ThenBy(p => p, StringComparer.Ordinal).FirstOrDefault();
            var declaration = "";
            if (prefix is null)
            {
                prefix = "xsl";
                for (var i = 1; bindings.ContainsKey(prefix); i++) prefix = "xsl" + i;
                declaration = $" xmlns:{prefix}=\"{Xsl}\"";
            }
            var source = snapshot.SourceFor(target.ModuleUri);
            // Count before allocating the escaped string or any emitted byte buffer.
            long encodedLength = source.Encoding.GetByteCount($"<{prefix}:value-of{declaration} select=\"\"/>");
            foreach (var rune in text.EnumerateRunes())
            {
                token.ThrowIfCancellationRequested();
                var escaped = EscapeScalar(rune);
                encodedLength += source.Encoding.GetByteCount(escaped);
                if (encodedLength + source.OriginalBytes.Length > AuthoringCapabilities.MaxValueOfInsertionSourceBytes)
                    return new(null, Failure(AuthoringInsertionFailureKind.SizeLimit, "The candidate exceeds the insertion source byte limit."));
            }
            token.ThrowIfCancellationRequested();
            var escapedText = new StringBuilder();
            foreach (var rune in text.EnumerateRunes()) escapedText.Append(EscapeScalar(rune));
            var instruction = $"<{prefix}:value-of{declaration} select=\"{escapedText}\"/>";
            var insertedBytes = source.Encoding.GetBytes(instruction);
            var position = checked((int)source.CoordinateMap.ByteOffsetFromCharOffset(target.CharacterOffset));
            var emitted = new byte[checked(source.OriginalBytes.Length + insertedBytes.Length)];
            source.OriginalBytes.Span[..position].CopyTo(emitted);
            insertedBytes.CopyTo(emitted, position);
            source.OriginalBytes.Span[position..].CopyTo(emitted.AsSpan(position + insertedBytes.Length));
            token.ThrowIfCancellationRequested();
            if (!AuthoringSource.TryCreate(emitted, target.ModuleUri, out var candidateSource, out var sourceFailure))
                return new(null, Failure(AuthoringInsertionFailureKind.InvalidCandidate, sourceFailure!.Message));
            // Never reuse the caller's mutable inspection options or its resource resolver.
            var inspection = XsltAuthoring.Inspect(candidateSource!, new DenyResolver(),
                new AuthoringInspectionOptions { AttemptCompilation = false, MaxModuleCount = 1 });
            token.ThrowIfCancellationRequested();
            if (!inspection.IsSuccess)
                return new(null, Failure(AuthoringInsertionFailureKind.InvalidCandidate, inspection.Failure!.Message));
            var candidate = inspection.Snapshot!;
            var insertedRange = candidateSource!.CoordinateMap.RangeFromCharOffsets(target.CharacterOffset,
                target.CharacterOffset + instruction.Length);
            var insertedMatches = Nodes(candidate.PrincipalModule.Root).Where(n =>
                n.Kind == AuthoringNodeKind.Element && n.Range == insertedRange).ToArray();
            var inserted = insertedMatches.Length == 1 ? insertedMatches[0] : null;
            var select = inserted?.Attributes.SingleOrDefault(a => a.Name == "select");
            if (select?.SlotKind != AuthoringAttributeSlotKind.Expression || select.ExpandedValue != text)
                return new(null, Failure(AuthoringInsertionFailureKind.SourceAlignmentFailure, "The inserted expression did not round-trip exactly."));
            var validation = Validate(candidate, true, token, select);
            if (validation is not null) return new(null, validation);
            var correspondence = Correspondence(snapshot, candidate, position, insertedBytes.Length, inserted!);
            if (correspondence is null)
                return new(null, Failure(AuthoringInsertionFailureKind.SourceAlignmentFailure, "Node provenance was incomplete or ambiguous."));
            var result = new AuthoringValueOfInsertionCandidate(snapshot, candidate.WithSuccessfulCompilation(),
                source.CoordinateMap.RangeFromCharOffsets(target.CharacterOffset, target.CharacterOffset),
                insertedRange, inserted!, select, correspondence);
            token.ThrowIfCancellationRequested();
            return new(result, null);
        }
        catch (OperationCanceledException) { return new(null, Cancelled()); }
    }

    private static AuthoringInsertionFailure? CheckScope(AuthoringSnapshot snapshot, AuthoringNodeDescriptor parent,
        CancellationToken token, out int offset, out XElement? retainedParent)
    {
        offset = 0;
        retainedParent = null;
        var module = snapshot.PrincipalModule;
        if (snapshot.Modules.Count != 1 || module.Edges.Count != 0)
            return Failure(AuthoringInsertionFailureKind.UnsupportedModule, "Only a single principal module is supported.");
        var source = snapshot.SourceFor(module.ModuleUri);
        if (source.OriginalBytes.Length > AuthoringCapabilities.MaxValueOfInsertionSourceBytes)
            return Failure(AuthoringInsertionFailureKind.SizeLimit, "The original source exceeds the insertion byte limit.");
        XDocument document;
        try { document = ReadDocument(source); }
        catch (XmlException ex) { return Failure(AuthoringInsertionFailureKind.InvalidOriginal, ex.Message); }
        token.ThrowIfCancellationRequested();
        var root = document.Root;
        if (root is null || root.Name.Namespace != Xsl || root.Name.LocalName is not ("stylesheet" or "transform")
            || (string?)root.Attribute("version") is not ("3.0" or "4.0"))
            return Failure(AuthoringInsertionFailureKind.UnsupportedProfile, "An explicit XSLT 3.0 or 4.0 stylesheet is required.");
        if (!EncodingSupported(source, root.Document?.Declaration))
            return Failure(AuthoringInsertionFailureKind.UnsupportedEncoding, "Insertion requires UTF-8 or BOM-marked UTF-16 with a matching declaration.");
        foreach (var element in root.DescendantsAndSelf())
        {
            token.ThrowIfCancellationRequested();
            if (element.Name.Namespace == Xsl && element.Name.LocalName is "include" or "import" or "use-package" or "package" or "import-schema")
                return Failure(AuthoringInsertionFailureKind.UnsupportedModule, "Module, package and schema dependencies are outside insertion coverage.");
            // These constructs can execute user code during compiler preprocessing. Refuse before compilation,
            // including literal-looking shadow attributes, rather than evaluating a caller's static expressions.
            if (element.Attributes().Any(a => a.Name.LocalName is "fixed-namespaces" or "type-namespaces" or "extension-element-prefixes"
                || a.Name.LocalName.StartsWith('_') && element.Name.Namespace == Xsl && a.Name.NamespaceName.Length == 0
                || a.Name.LocalName == "static" && a.Value is not ("no" or "false" or "0")
                || a.Name.LocalName == "use-when"))
                return Failure(AuthoringInsertionFailureKind.UnsupportedCoverage, "Static evaluation and fixed/type namespace contexts are outside insertion coverage.");
        }
        var parentStartOffset = source.CoordinateMap.CharOffsetFromLineColumn(parent.Range.StartLine, parent.Range.StartColumn);
        var target = parent.Kind == AuthoringNodeKind.Element ? root.DescendantsAndSelf().FirstOrDefault(element =>
        {
            token.ThrowIfCancellationRequested();
            var info = (IXmlLineInfo)element;
            var startOffset = source.CoordinateMap.CharOffsetFromLineColumn(info.LineNumber, info.LinePosition);
            if (startOffset > 0 && source.Text[startOffset - 1] == '<') startOffset--;
            return startOffset == parentStartOffset && element.Name.LocalName == parent.ElementName?.LocalName
                && element.Name.NamespaceName == parent.ElementName.NamespaceUri;
        }) : null;
        if (target is null)
            return Failure(AuthoringInsertionFailureKind.IneligibleParent, "The target must be a template or literal result element.");
        XElement? template = target;
        while (template is not null && template.Name.Namespace != Xsl) template = template.Parent;
        if (template is null || template.Name != Xsl + "template" || template.Parent != root)
            return Failure(AuthoringInsertionFailureKind.IneligibleParent, "Only a literal-result-only chain beneath a top-level template is supported.");
        if (target.AncestorsAndSelf().Any(e => e.Attributes().Any(a =>
            (a.Name == "version" && e.Name.Namespace == Xsl || a.Name == Xsl + "version") && a.Value is not ("3.0" or "4.0"))))
            return Failure(AuthoringInsertionFailureKind.UnsupportedProfile, "The effective version must be 3.0 or 4.0.");
        if (target.AncestorsAndSelf().Select(e => (string?)e.Attribute(XNamespace.Xml + "space")).FirstOrDefault(v => v is not null) == "preserve"
            || target.Nodes().Any(n => n is XCData || n is XText t && t.Value.Any(c => c is not (' ' or '\t' or '\r' or '\n'))))
            return Failure(AuthoringInsertionFailureKind.MixedContent, "Significant text, CDATA and xml:space=preserve are unsupported.");
        foreach (var element in root.DescendantsAndSelf())
        {
            token.ThrowIfCancellationRequested();
            if (element.Attributes().Any(a => a.Name.LocalName is "as" or "type" or "validation" or "mode" or "default-mode" or "use-attribute-sets"))
                return Failure(AuthoringInsertionFailureKind.UnsupportedCoverage, "Type and component-reference annotations are outside insertion coverage.");
            var expandText = element.AncestorsAndSelf().Select(e =>
                (string?)e.Attribute(e.Name.Namespace == Xsl ? XName.Get("expand-text") : Xsl + "expand-text"))
                .FirstOrDefault(v => v is not null);
            if (expandText is "yes" or "true" or "1" && element.Nodes().OfType<XText>().Any(t => t.Value.Any(c => c is not (' ' or '\t' or '\r' or '\n'))))
                return Failure(AuthoringInsertionFailureKind.UnsupportedCoverage, "Existing expanded text requires text-value-template static coverage.");
        }
        var start = source.CoordinateMap.CharOffsetFromLineColumn(parent.Range.StartLine, parent.Range.StartColumn);
        var end = SourceTagScanner.FindElementEnd(source.Text, start, out _, out var selfClosed);
        if (selfClosed) return Failure(AuthoringInsertionFailureKind.SelfClosingParent, "The parent requires an explicit closing tag.");
        // FindElementEnd has already traversed nested tags, quoted attributes, CDATA, comments and PIs.
        // The last '<' inside that proven element extent is its actual closing tag.
        offset = source.Text.LastIndexOf('<', end - 1, end - start);
        if (offset <= start || !source.Text.AsSpan(offset).StartsWith("</", StringComparison.Ordinal)
            || source.CoordinateMap.ByteOffsetFromCharOffset(end) != parent.Range.StartByteOffset + parent.Range.ByteLength)
            return Failure(AuthoringInsertionFailureKind.SourceAlignmentFailure, "The parent closing tag does not align with retained source.");
        retainedParent = target;
        return null;
    }

    private static AuthoringInsertionFailure? Validate(AuthoringSnapshot snapshot, bool candidate,
        CancellationToken token, AuthoringAttributeDescriptor? select = null)
    {
        token.ThrowIfCancellationRequested();
        var validation = XsltValidation.Validate(snapshot, new XsltValidationOptions { CancellationToken = token });
        token.ThrowIfCancellationRequested();
        if (!validation.IsValid)
        {
            var diagnostic = validation.Diagnostics.FirstOrDefault();
            var kind = validation.UnsupportedCoverage.Count != 0 ? AuthoringInsertionFailureKind.UnsupportedCoverage
                : !candidate ? AuthoringInsertionFailureKind.InvalidOriginal
                : diagnostic?.Range == select?.ValueRange ? AuthoringInsertionFailureKind.InvalidExpression
                : AuthoringInsertionFailureKind.InvalidCandidate;
            return Failure(kind, diagnostic?.Message ?? "The stylesheet is outside complete static validation coverage.", diagnostic?.Code);
        }
        foreach (var node in Nodes(snapshot.PrincipalModule.Root))
        {
            if (node.ElementName?.NamespaceUri == Xsl.NamespaceName && !CoveredInstructions.Contains(node.ElementName.LocalName))
                return Failure(AuthoringInsertionFailureKind.UnsupportedCoverage, $"Instruction {node.ElementName.LocalName} is outside proven insertion coverage.");
            foreach (var slot in node.Attributes.Where(a => a.SlotContext is not null))
            {
                try { InsertionExpressionCoverage.Check(snapshot, slot, token); }
                catch (OperationCanceledException) { throw; }
                catch (InsertionExpressionCoverage.CoverageException ex)
                { return Failure(AuthoringInsertionFailureKind.UnsupportedCoverage, ex.Message); }
                catch (Exception ex)
                {
                    return Failure(!candidate ? AuthoringInsertionFailureKind.InvalidOriginal
                        : ReferenceEquals(slot, select) ? AuthoringInsertionFailureKind.InvalidExpression
                        : AuthoringInsertionFailureKind.InvalidCandidate, ex.Message,
                        ex.Message.Contains("XPST0081", StringComparison.Ordinal) ? "XPST0081" : null);
                }
            }
        }
        var policy = new ControlledResourcePolicy(new DenyAuthority());
        try
        {
            var source = snapshot.SourceFor(snapshot.PrincipalModule.ModuleUri);
            var document = ReadDocument(source);
            token.ThrowIfCancellationRequested();
            new XsltCompiler { ResourcePolicy = policy, UriResolver = new DenyResolver() }
                .Compile(document, source.BaseUri.AbsoluteUri);
            token.ThrowIfCancellationRequested();
            if (policy.Receipts.Count != 0)
                return Failure(AuthoringInsertionFailureKind.ResourceDenied, "Static compilation attempted resource acquisition.");
            return null;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            token.ThrowIfCancellationRequested();
            return Failure(policy.Receipts.Count != 0 ? AuthoringInsertionFailureKind.ResourceDenied
                : candidate ? AuthoringInsertionFailureKind.InvalidCandidate : AuthoringInsertionFailureKind.InvalidOriginal, ex.Message);
        }
    }

    private static bool EncodingSupported(AuthoringSource source, XDeclaration? declaration)
    {
        var cp = source.Encoding.CodePage;
        if (cp != 65001 && (cp is not (1200 or 1201) || source.ByteOffsetBias != 2)) return false;
        if (declaration?.Version is not (null or "1.0")) return false;
        var name = declaration?.Encoding;
        if (name is null) return true;
        return cp switch
        {
            65001 => name.Equals("utf-8", StringComparison.OrdinalIgnoreCase),
            1200 => name.Equals("utf-16", StringComparison.OrdinalIgnoreCase) || name.Equals("utf-16le", StringComparison.OrdinalIgnoreCase),
            1201 => name.Equals("utf-16", StringComparison.OrdinalIgnoreCase) || name.Equals("utf-16be", StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }

    private static XDocument ReadDocument(AuthoringSource source)
    {
        using var text = new StringReader(source.Text);
        using var reader = XmlReader.Create(text, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }, source.BaseUri.AbsoluteUri);
        return XDocument.Load(reader, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
    }

    private static Dictionary<string, string> NamespaceBindings(XElement element)
    {
        var bindings = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var ancestor in element.AncestorsAndSelf().Reverse())
            foreach (var attribute in ancestor.Attributes().Where(a => a.IsNamespaceDeclaration))
                bindings[attribute.Name.LocalName == "xmlns" ? "" : attribute.Name.LocalName] = attribute.Value;
        return bindings;
    }

    private static string EscapeScalar(Rune rune) => rune.Value switch
    { '&' => "&amp;", '<' => "&lt;", '"' => "&quot;", '\t' => "&#x9;", '\n' => "&#xA;", '\r' => "&#xD;", _ => rune.ToString() };

    private static IReadOnlyDictionary<int, int>? Correspondence(AuthoringSnapshot input, AuthoringSnapshot candidate,
        long position, long delta, AuthoringNodeDescriptor inserted)
    {
        var available = Nodes(candidate.PrincipalModule.Root).Where(n => !ReferenceEquals(n, inserted))
            .GroupBy(Key).ToDictionary(g => g.Key, g => g.ToArray());
        var map = new Dictionary<int, int>();
        var used = new HashSet<int>();
        foreach (var old in Nodes(input.PrincipalModule.Root))
        {
            var start = old.Range.StartByteOffset;
            var length = old.Range.ByteLength;
            if (start >= position) start += delta;
            else if (start + length > position) length += delta;
            var key = (old.Kind, start, length, old.ElementName?.NamespaceUri, old.ElementName?.LocalName);
            if (!available.TryGetValue(key, out var matches) || matches.Length != 1 || !used.Add(matches[0].Id)) return null;
            map.Add(old.Id, matches[0].Id);
        }
        return used.Count == available.Values.Sum(a => a.Length) ? new ReadOnlyDictionary<int, int>(map) : null;
    }

    private static (AuthoringNodeKind, long, long, string?, string?) Key(AuthoringNodeDescriptor node) =>
        (node.Kind, node.Range.StartByteOffset, node.Range.ByteLength, node.ElementName?.NamespaceUri, node.ElementName?.LocalName);

    private static IEnumerable<AuthoringNodeDescriptor> Nodes(AuthoringNodeDescriptor root)
    {
        yield return root;
        foreach (var child in root.Children)
            foreach (var descendant in Nodes(child)) yield return descendant;
    }

    private static AuthoringInsertionFailure Failure(AuthoringInsertionFailureKind kind, string message, string? code = null) => new(kind, message, code);
    private static AuthoringInsertionFailure Cancelled() => Failure(AuthoringInsertionFailureKind.Cancelled, "Insertion was cancelled; no candidate was returned.");

    private sealed class DenyAuthority : IControlledResourceAuthority
    {
        /// <inheritdoc />
        public ControlledResourceResponse Authorize(ControlledResourceRequest request) => ControlledResourceResponse.Deny("Insertion performs no resource access.");
    }

    private sealed class DenyResolver : IAuthoringModuleResolver, IXsltUriResolver
    {
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) => null;
        /// <inheritdoc />
        public XDocument Resolve(string href, string? baseUri) => throw new InvalidOperationException("XV0004: Insertion denies module resolution.");
    }
}
