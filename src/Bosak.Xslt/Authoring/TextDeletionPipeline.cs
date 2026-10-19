// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 October 2026
// PURPOSE              : Remove literal text and prove exact byte, payload and bounded merge partitions.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;

namespace Bosak.Xslt.Authoring;

internal static class TextDeletionPipeline
{
    private static readonly XNamespace Xsl = "http://www.w3.org/1999/XSL/Transform";
    private const int Limit = 8_388_608;

    internal static AuthoringTextDeletionTargetResult Target(AuthoringSnapshot snapshot,
        AuthoringNodeDescriptor instruction, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            if (!ReferenceEquals(snapshot.FindNodeById(instruction.Id), instruction))
                return new(null, Fail(AuthoringTextDeletionFailureKind.ForeignSnapshot, "The instruction belongs to another snapshot."));
            if (instruction.Kind != AuthoringNodeKind.Element || instruction.ElementName?.NamespaceUri != Xsl.NamespaceName
                || instruction.ElementName.LocalName != "text")
                return new(null, Fail(AuthoringTextDeletionFailureKind.IneligibleInstruction, "An actual XSLT text instruction is required."));
            var failure = Scope(snapshot, instruction, token, out var element);
            if (failure is not null) return new(null, failure);
            var source = snapshot.SourceFor(snapshot.PrincipalModule.ModuleUri);
            var start = Offset(source, instruction.Range, false);
            var end = SourceTagScanner.FindElementEnd(source.Text, start, out var bodyStart, out var selfClosed);
            if (source.CoordinateMap.RangeFromCharOffsets(start, end) != instruction.Range)
                return new(null, Alignment());
            SourceRange? body = null;
            var bodyEnd = bodyStart;
            if (!selfClosed)
            {
                bodyEnd = source.Text.LastIndexOf('<', end - 1, end - start);
                if (bodyEnd < bodyStart || !source.Text.AsSpan(bodyEnd).StartsWith("</", StringComparison.Ordinal))
                    return new(null, Alignment());
                body = source.CoordinateMap.RangeFromCharOffsets(bodyStart, bodyEnd);
            }
            var expand = EffectiveExpand(element!);
            if (expand is not (null or "no" or "false" or "0")
                || (string?)element!.Attribute("disable-output-escaping") is not (null or "no")
                || element.Nodes().Any(n => n is not XText || n is XCData)
                || !selfClosed && source.Text.AsSpan(bodyStart, bodyEnd - bodyStart).Contains('<'))
                return new(null, Fail(AuthoringTextDeletionFailureKind.UnsupportedCoverage, "Only ordinary non-expanding text without output escaping or body markup is supported."));
            var payload = instruction.Children.Count == 1 ? instruction.Children[0] : null;
            if (selfClosed || bodyStart == bodyEnd ? instruction.Children.Count != 0
                : payload?.Kind != AuthoringNodeKind.Text || payload.Range != body || payload.Text != element.Value
                    || payload.BackingObject is not XText or XCData)
                return new(null, Alignment());
            AuthoringNodeDescriptor? parent = null;
            foreach (var node in ValueOfInsertionPipeline.Nodes(snapshot.PrincipalModule.Root))
            {
                token.ThrowIfCancellationRequested();
                if (!node.Children.Contains(instruction)) continue;
                if (parent is not null) return new(null, Alignment());
                parent = node;
            }
            if (parent is null || element.Parent is null) return new(null, Alignment());
            if (element.Ancestors().Any(e => (string?)e.Attribute(XNamespace.Xml + "space") == "preserve")
                || element.Parent.Nodes().Any(n => n is XCData || n is XText t && t.Value.Any(c => c is not (' ' or '\t' or '\r' or '\n'))))
                return new(null, Fail(AuthoringTextDeletionFailureKind.MixedContent, "Significant direct parent text, CDATA or a preserving ancestor is outside deletion coverage."));
            failure = Validate(snapshot, false, token);
            token.ThrowIfCancellationRequested();
            return failure is null ? new(new(snapshot, instruction, parent, selfClosed, body, payload), null) : new(null, failure);
        }
        catch (OperationCanceledException) { return new(null, Cancelled()); }

        AuthoringTextDeletionFailure Fail(AuthoringTextDeletionFailureKind kind, string message) => Failure(snapshot, kind, message);
        AuthoringTextDeletionFailure Alignment() => Fail(AuthoringTextDeletionFailureKind.SourceAlignmentFailure, "The retained instruction and payload do not align.");
        AuthoringTextDeletionFailure Cancelled() => Fail(AuthoringTextDeletionFailureKind.Cancelled, "Deletion discovery was cancelled.");
    }

    internal static AuthoringTextDeletionResult Run(AuthoringSnapshot snapshot,
        AuthoringTextDeletionProposal proposal, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            if (!ReferenceEquals(proposal.Target.Owner, snapshot))
                return Refuse(AuthoringTextDeletionFailureKind.ForeignSnapshot, "The target belongs to another snapshot.");
            var discovered = Target(snapshot, proposal.Target.Instruction, token);
            if (!discovered.IsSuccess) return new(null, discovered.Failure);
            var target = discovered.Target!;
            if (target.OriginalBodyRange != proposal.Target.OriginalBodyRange
                || !ReferenceEquals(target.OriginalPayloadNode, proposal.Target.OriginalPayloadNode)
                || !ReferenceEquals(target.Parent, proposal.Target.Parent) || target.IsSelfClosing != proposal.Target.IsSelfClosing)
                return Refuse(AuthoringTextDeletionFailureKind.UnknownTarget, "The target authority does not align.");
            var source = snapshot.SourceFor(target.ModuleUri);
            var bytes = source.OriginalBytes;
            var a = checked((int)target.OriginalRange.StartByteOffset);
            var b = checked(a + (int)target.OriginalRange.ByteLength);
            if (a < source.ByteOffsetBias || b > bytes.Length || a >= b)
                return Refuse(AuthoringTextDeletionFailureKind.SourceAlignmentFailure, "The removal extent is invalid.");
            var emitted = new byte[bytes.Length - (b - a)];
            Copy(bytes.Span[..a], emitted.AsSpan(0, a), token);
            Copy(bytes.Span[b..], emitted.AsSpan(a), token);
            token.ThrowIfCancellationRequested();
            if (!AuthoringSource.TryCreate(emitted, target.ModuleUri, out var candidateSource, out var sourceFailure))
                return Refuse(AuthoringTextDeletionFailureKind.InvalidCandidate, sourceFailure!.Message);
            var inspected = XsltAuthoring.Inspect(candidateSource!, new DenyResolver(),
                new() { AttemptCompilation = false, MaxModuleCount = 1 });
            token.ThrowIfCancellationRequested();
            if (!inspected.IsSuccess) return Refuse(AuthoringTextDeletionFailureKind.InvalidCandidate, inspected.Failure!.Message);
            var candidate = inspected.Snapshot!;
            var validation = Validate(candidate, true, token);
            if (validation is not null) return new(null, validation);
            var proof = new Proof(source, candidateSource!, target, token);
            if (!proof.Match(snapshot.PrincipalModule.Root, candidate.PrincipalModule.Root)
                || !proof.Map.TryGetValue(target.Parent, out var parent))
                return Refuse(AuthoringTextDeletionFailureKind.UnsupportedCorrespondence, "The complete survivor and fragment relation could not be proven.");
            // The removed payload is the only descendant excluded by recursive subtree removal.
            var oldNodes = new HashSet<AuthoringNodeDescriptor>();
            var newNodes = new HashSet<AuthoringNodeDescriptor>();
            var oldIds = new HashSet<int>();
            var newIds = new HashSet<int>();
            foreach (var n in ValueOfInsertionPipeline.Nodes(snapshot.PrincipalModule.Root))
            {
                token.ThrowIfCancellationRequested();
                if (!oldNodes.Add(n) || !oldIds.Add(n.Id))
                    return Refuse(AuthoringTextDeletionFailureKind.UnsupportedCorrespondence, "Duplicate original descriptor or identity.");
            }
            foreach (var n in ValueOfInsertionPipeline.Nodes(candidate.PrincipalModule.Root))
            {
                token.ThrowIfCancellationRequested();
                if (!newNodes.Add(n) || !newIds.Add(n.Id))
                    return Refuse(AuthoringTextDeletionFailureKind.UnsupportedCorrespondence, "Duplicate candidate descriptor or identity.");
            }
            var originals = proof.Map.Keys.Concat(proof.Groups.SelectMany(g => g.Fragments.Select(f => f.OriginalNode)))
                .Concat(target.OriginalPayloadNode is { } p ? new[] { target.Instruction, p } : new[] { target.Instruction }).ToArray();
            var destinations = proof.Map.Values.Concat(proof.Groups.Select(g => g.CandidateNode)).ToArray();
            if (originals.Distinct().Count() != originals.Length || !oldNodes.SetEquals(originals)
                || destinations.Distinct().Count() != destinations.Length || !newNodes.SetEquals(destinations))
                return Refuse(AuthoringTextDeletionFailureKind.UnsupportedCorrespondence, "Incomplete or overlapping descriptor partitions.");
            var offset = Offset(source, target.OriginalRange, false);
            var anchor = candidateSource!.CoordinateMap.RangeFromCharOffsets(offset, offset);
            var result = new AuthoringTextDeletionCandidate(target, candidate.WithSuccessfulCompilation(), target.Instruction, parent, anchor,
                new ReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor>(proof.Map), proof.Groups);
            token.ThrowIfCancellationRequested();
            return new(result, null);
        }
        catch (OperationCanceledException) { return Refuse(AuthoringTextDeletionFailureKind.Cancelled, "Deletion was cancelled; no candidate returned."); }

        AuthoringTextDeletionResult Refuse(AuthoringTextDeletionFailureKind kind, string message) => new(null, Failure(snapshot, kind, message));
    }

    private static void Copy(ReadOnlySpan<byte> source, Span<byte> destination, CancellationToken token)
    {
        for (var offset = 0; offset < source.Length; offset += 16_384)
        {
            token.ThrowIfCancellationRequested();
            var length = Math.Min(16_384, source.Length - offset);
            source.Slice(offset, length).CopyTo(destination.Slice(offset, length));
        }
    }

    private static AuthoringTextDeletionFailure? Scope(AuthoringSnapshot snapshot, AuthoringNodeDescriptor instruction,
        CancellationToken token, out XElement? retained)
    {
        AuthoringTextDeletionFailure Fail(AuthoringTextDeletionFailureKind kind, string message) => Failure(snapshot, kind, message);
        AuthoringTextDeletionFailure Alignment() => Fail(AuthoringTextDeletionFailureKind.SourceAlignmentFailure, "The retained target does not align.");
        retained = null;
        var module = snapshot.PrincipalModule;
        if (snapshot.Modules.Count != 1 || module.Edges.Count != 0)
            return Fail(AuthoringTextDeletionFailureKind.UnsupportedModule, "Only a single principal module is supported.");
        var source = snapshot.SourceFor(module.ModuleUri);
        if (source.OriginalBytes.Length > Limit)
            return Fail(AuthoringTextDeletionFailureKind.SizeLimit, "The original source exceeds the deletion byte limit.");
        XDocument document;
        try { document = ReadDocument(source); }
        catch (XmlException ex) { return Fail(AuthoringTextDeletionFailureKind.InvalidOriginal, ex.Message); }
        token.ThrowIfCancellationRequested();
        var root = document.Root;
        if (root is null || root.Name.Namespace != Xsl || root.Name.LocalName is not ("stylesheet" or "transform")
            || (string?)root.Attribute("version") is not ("3.0" or "4.0"))
            return Fail(AuthoringTextDeletionFailureKind.UnsupportedProfile, "An explicit XSLT 3.0 or 4.0 stylesheet is required.");
        if (!EncodingSupported(source, root.Document?.Declaration))
            return Fail(AuthoringTextDeletionFailureKind.UnsupportedEncoding, "Deletion requires UTF-8 or BOM-marked UTF-16 with a matching declaration.");
        foreach (var element in root.DescendantsAndSelf())
        {
            token.ThrowIfCancellationRequested();
            if (element.Name.Namespace == Xsl && element.Name.LocalName is "include" or "import" or "use-package" or "package" or "import-schema")
                return Fail(AuthoringTextDeletionFailureKind.UnsupportedModule, "Module, package and schema dependencies are outside deletion coverage.");
            // These constructs can execute user code during compiler preprocessing. Refuse before compilation,
            // including literal-looking shadow attributes, rather than evaluating a caller's static expressions.
            if (element.Attributes().Where(a => IsXsltAttribute(element, a)).Any(a => a.Name.LocalName is "fixed-namespaces" or "type-namespaces" or "extension-element-prefixes"
                || a.Name.LocalName.StartsWith('_') && element.Name.Namespace == Xsl && a.Name.NamespaceName.Length == 0
                || a.Name.LocalName == "static" && a.Value is not ("no" or "false" or "0")
                || a.Name.LocalName == "use-when"))
                return Fail(AuthoringTextDeletionFailureKind.UnsupportedCoverage, "Static evaluation and fixed/type namespace contexts are outside deletion coverage.");
        }
        var start = source.CoordinateMap.CharOffsetFromLineColumn(instruction.Range.StartLine, instruction.Range.StartColumn);
        XElement? target = null;
        foreach (var e in root.DescendantsAndSelf())
        {
            token.ThrowIfCancellationRequested();
            var info = (IXmlLineInfo)e;
            var offset = source.CoordinateMap.CharOffsetFromLineColumn(info.LineNumber, info.LinePosition);
            if (offset > 0 && source.Text[offset - 1] == '<') offset--;
            if (offset == start && e.Name == Xsl + "text") { target = e; break; }
        }
        if (target is null) return Alignment();
        XElement? template = target.Parent;
        while (template is not null && template.Name.Namespace != Xsl) template = template.Parent;
        if (template?.Name != Xsl + "template" || template.Parent != root)
            return Fail(AuthoringTextDeletionFailureKind.IneligibleInstruction, "Only a literal-result-only chain beneath a top-level template is supported.");
        if (target.AncestorsAndSelf().Any(e => e.Attributes().Any(a =>
            (a.Name == "version" && e.Name.Namespace == Xsl || a.Name == Xsl + "version") && a.Value is not ("3.0" or "4.0"))))
            return Fail(AuthoringTextDeletionFailureKind.UnsupportedProfile, "Effective versions must be 3.0 or 4.0.");
        foreach (var e in root.DescendantsAndSelf())
        {
            token.ThrowIfCancellationRequested();
            if (e.Attributes().Where(a => IsXsltAttribute(e, a)).Any(a => a.Name.LocalName is "as" or "type" or "validation" or "mode" or "default-mode" or "use-attribute-sets"))
                return Fail(AuthoringTextDeletionFailureKind.UnsupportedCoverage, "Type/component annotations are outside captured coverage.");
            if (EffectiveExpand(e) is "yes" or "true" or "1" && e.Nodes().OfType<XText>().Any(t => t.Value.Any(c => c is not (' ' or '\t' or '\r' or '\n'))))
                return Fail(AuthoringTextDeletionFailureKind.UnsupportedCoverage, "Expanded text requires additional static coverage.");
        }
        retained = target;
        return null;
    }

    private static string? EffectiveExpand(XElement e) => e.AncestorsAndSelf().Select(a =>
        (string?)a.Attribute(a.Name.Namespace == Xsl ? XName.Get("expand-text") : Xsl + "expand-text")).FirstOrDefault(v => v is not null);

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

    private static bool IsXsltAttribute(XElement owner, XAttribute attribute) =>
        attribute.Name.Namespace == Xsl || owner.Name.Namespace == Xsl && attribute.Name.NamespaceName.Length == 0;

    private static bool Whitespace(AuthoringNodeDescriptor n) => n.Kind == AuthoringNodeKind.Text
        && n.Range.ByteLength > 0 && n.Text is not null && n.Text.All(c => c is ' ' or '\t' or '\r' or '\n')
        && n.BackingObject is XText and not XCData;

    private static int Offset(AuthoringSource source, SourceRange range, bool end) => source.CoordinateMap.CharOffsetFromLineColumn(
        end ? range.EndLine : range.StartLine, end ? range.EndColumn : range.StartColumn);

    internal sealed class Proof(AuthoringSource original, AuthoringSource candidate,
        AuthoringTextDeletionTarget target, CancellationToken token)
    {
        internal Dictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> Map { get; } = new();
        internal List<AuthoringTextMergeGroup> Groups { get; } = new();
        private readonly HashSet<AuthoringNodeDescriptor> _used = new();
        private int A => Offset(original, target.Instruction.Range, false);
        private int B => Offset(original, target.Instruction.Range, true);
        private int Translate(int offset) => offset <= A ? offset : offset >= B ? offset - (B - A) : -1;
        private SourceRange? Range(SourceRange old)
        {
            var start = Translate(Offset(original, old, false));
            var end = Translate(Offset(original, old, true));
            return start < 0 || end < start || end > candidate.Text.Length
                ? null : candidate.CoordinateMap.RangeFromCharOffsets(start, end);
        }

        internal bool Match(AuthoringNodeDescriptor old, AuthoringNodeDescriptor fresh)
        {
            token.ThrowIfCancellationRequested();
            if (ReferenceEquals(old, target.Instruction) || old.Kind != fresh.Kind || old.ElementName != fresh.ElementName
                || old.Text != fresh.Text || Range(old.Range) != fresh.Range || old.Attributes.Count != fresh.Attributes.Count
                || !_used.Add(fresh) || !Map.TryAdd(old, fresh)) return false;
            for (var i = 0; i < old.Attributes.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                var x = old.Attributes[i]; var y = fresh.Attributes[i];
                if (x.Name != y.Name || x.Prefix != y.Prefix || x.LocalName != y.LocalName
                    || x.NamespaceUri != y.NamespaceUri || x.RawLiteral != y.RawLiteral
                    || x.ExpandedValue != y.ExpandedValue || x.SlotKind != y.SlotKind
                    || Range(x.FullRange) != y.FullRange || Range(x.ValueRange) != y.ValueRange) return false;
            }
            var oi = 0; var ni = 0;
            while (oi < old.Children.Count)
            {
                token.ThrowIfCancellationRequested();
                var child = old.Children[oi];
                if (ReferenceEquals(child, target.Instruction)) { oi++; continue; }
                if (ReferenceEquals(old, target.Parent) && oi + 2 < old.Children.Count
                    && ReferenceEquals(old.Children[oi + 1], target.Instruction)
                    && old.Children[oi].Kind == AuthoringNodeKind.Text && old.Children[oi + 2].Kind == AuthoringNodeKind.Text)
                {
                    if (ni >= fresh.Children.Count || !Merge(child, old.Children[oi + 2], fresh.Children[ni])) return false;
                    oi += 3; ni++; continue;
                }
                if (ni >= fresh.Children.Count || !Match(child, fresh.Children[ni])) return false;
                oi++; ni++;
            }
            return ni == fresh.Children.Count;
        }

        private bool Merge(AuthoringNodeDescriptor left, AuthoringNodeDescriptor right, AuthoringNodeDescriptor fresh)
        {
            token.ThrowIfCancellationRequested();
            if (Groups.Count != 0 || !Whitespace(left) || !Whitespace(right) || !Whitespace(fresh)
                || !_used.Add(fresh) || Offset(original, left.Range, true) != A || Offset(original, right.Range, false) != B) return false;
            var l = Range(left.Range); var r = Range(right.Range);
            if (l is null || r is null || l.StartByteOffset != fresh.Range.StartByteOffset
                || l.StartByteOffset + l.ByteLength != r.StartByteOffset
                || r.StartByteOffset + r.ByteLength != fresh.Range.StartByteOffset + fresh.Range.ByteLength) return false;
            foreach (var pair in new[] { (left.Range, l), (right.Range, r) })
            {
                token.ThrowIfCancellationRequested();
                if (!original.OriginalBytes.Span.Slice((int)pair.Item1.StartByteOffset, (int)pair.Item1.ByteLength)
                    .SequenceEqual(candidate.OriginalBytes.Span.Slice((int)pair.Item2.StartByteOffset, (int)pair.Item2.ByteLength))) return false;
            }
            // Fresh ordinary inspection owns parsed Text, including CR/LF and character-reference semantics.
            Groups.Add(new(fresh, new[] { new AuthoringTextMergeFragment(left, left.Range, l), new AuthoringTextMergeFragment(right, right.Range, r) }));
            return true;
        }
    }

    private static AuthoringTextDeletionFailure? Validate(AuthoringSnapshot snapshot, bool candidate, CancellationToken token)
    {
        var failure = ValueOfInsertionPipeline.Validate(snapshot, candidate, token, refusalFilter: ExpectedStaticFailure);
        if (failure is null) return null;
        var kind = failure.Kind switch
        {
            AuthoringInsertionFailureKind.UnsupportedCoverage => AuthoringTextDeletionFailureKind.UnsupportedCoverage,
            AuthoringInsertionFailureKind.ResourceDenied => AuthoringTextDeletionFailureKind.ResourceDenied,
            AuthoringInsertionFailureKind.Cancelled => AuthoringTextDeletionFailureKind.Cancelled,
            _ => candidate ? AuthoringTextDeletionFailureKind.InvalidCandidate : AuthoringTextDeletionFailureKind.InvalidOriginal,
        };
        return Failure(snapshot, kind, failure.Message, failure.ErrorCode);
    }

    // The existing compiler reports static errors through parse/XML exceptions or coded
    // InvalidOperationException diagnostics. Preserve those refusals without hiding engine defects.
    private static bool ExpectedStaticFailure(Exception exception) => exception is XmlException
        or Bosak.XPath.Parser.XPathParseException or AuthoringFunctionSignatureCoverageException
        || exception is InvalidOperationException && System.Text.RegularExpressions.Regex.IsMatch(
            exception.Message, @"\b(?:XT|XP|XQ|FO)[A-Z]{2}[0-9]{4}\b", System.Text.RegularExpressions.RegexOptions.CultureInvariant);

    private static AuthoringTextDeletionFailure Failure(AuthoringSnapshot snapshot, AuthoringTextDeletionFailureKind kind,
        string message, string? code = null) => new(kind, message, code, snapshot.PrincipalModule.ModuleUri, null);

    private sealed class DenyResolver : IAuthoringModuleResolver
    {
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) => null;
    }
}
