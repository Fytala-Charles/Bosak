// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 October 2026
// PURPOSE              : Validate and replace one literal text body with exact bytes and complete graph partitions.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 18-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Buffers;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Bosak.Xslt.Authoring;

internal static class TextBodyReplacementPipeline
{
    private static readonly XNamespace Xsl = "http://www.w3.org/1999/XSL/Transform";
    private const int Limit = 8_388_608;

    internal static AuthoringTextBodyReplacementTargetResult Target(AuthoringSnapshot snapshot,
        AuthoringNodeDescriptor instruction, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            if (!ReferenceEquals(snapshot.FindNodeById(instruction.Id), instruction))
                return new(null, Fail(AuthoringTextBodyReplacementFailureKind.ForeignSnapshot, "The instruction belongs to another snapshot."));
            if (instruction.Kind != AuthoringNodeKind.Element || instruction.ElementName?.NamespaceUri != Xsl.NamespaceName
                || instruction.ElementName.LocalName != "text")
                return new(null, Fail(AuthoringTextBodyReplacementFailureKind.IneligibleInstruction, "An actual XSLT text instruction is required."));
            var failure = Scope(snapshot, instruction, token, out var element);
            if (failure is not null) return new(null, failure);
            var source = snapshot.SourceFor(snapshot.PrincipalModule.ModuleUri);
            var start = source.CoordinateMap.CharOffsetFromLineColumn(instruction.Range.StartLine, instruction.Range.StartColumn);
            var end = SourceTagScanner.FindElementEnd(source.Text, start, out var bodyStart, out var selfClosed);
            if (selfClosed) return new(null, Fail(AuthoringTextBodyReplacementFailureKind.SelfClosingInstruction, "Explicit paired text tags are required."));
            if (source.CoordinateMap.RangeFromCharOffsets(start, end) != instruction.Range)
                return new(null, Alignment());
            var bodyEnd = source.Text.LastIndexOf('<', end - 1, end - start);
            if (bodyEnd < bodyStart || !source.Text.AsSpan(bodyEnd).StartsWith("</", StringComparison.Ordinal))
                return new(null, Alignment());
            var body = source.CoordinateMap.RangeFromCharOffsets(bodyStart, bodyEnd);
            var expand = EffectiveExpand(element!);
            if (expand is not (null or "no" or "false" or "0")
                || (string?)element!.Attribute("disable-output-escaping") is not (null or "no")
                || element.Nodes().Any(n => n is not XText || n is XCData)
                || source.Text.AsSpan(bodyStart, bodyEnd - bodyStart).Contains('<'))
                return new(null, Fail(AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage, "Only ordinary non-expanding text without output escaping or body markup is supported."));
            var payload = instruction.Children.Count == 1 ? instruction.Children[0] : null;
            if (bodyStart == bodyEnd ? instruction.Children.Count != 0
                : payload?.Kind != AuthoringNodeKind.Text || payload.Range != body || payload.Text != element.Value)
                return new(null, Alignment());
            var parents = ValueOfInsertionPipeline.Nodes(snapshot.PrincipalModule.Root).Where(n => n.Children.Contains(instruction)).Take(2).ToArray();
            if (parents.Length != 1) return new(null, Alignment());
            var parent = parents[0];
            failure = Validate(snapshot, false, token);
            token.ThrowIfCancellationRequested();
            return failure is null ? new(new(snapshot, instruction, parent, body, payload, bodyStart, bodyEnd), null) : new(null, failure);
        }
        catch (OperationCanceledException) { return new(null, Cancelled()); }
    }

    internal static AuthoringTextBodyReplacementResult Run(AuthoringSnapshot snapshot,
        AuthoringTextBodyReplacementProposal proposal, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            var target = proposal.Target;
            if (!ReferenceEquals(target.Owner, snapshot))
                return Refuse(Fail(AuthoringTextBodyReplacementFailureKind.ForeignSnapshot, "The target belongs to another snapshot."));
            var discovery = Target(snapshot, target.Instruction, token);
            if (!discovery.IsSuccess) return Refuse(discovery.Failure!);
            var checkedTarget = discovery.Target!;
            if (target.OriginalBodyRange != checkedTarget.OriginalBodyRange || target.OriginalTextValue != checkedTarget.OriginalTextValue)
                return Refuse(Fail(AuthoringTextBodyReplacementFailureKind.UnknownTarget, "The body authority no longer aligns."));
            var text = proposal.TextValue;
            // Validate all scalars before equal-intent handling. Do not count hypothetical canonical bytes yet.
            for (var i = 0; i < text.Length;)
            {
                token.ThrowIfCancellationRequested();
                if (Rune.DecodeFromUtf16(text.AsSpan(i), out var rune, out var used) != OperationStatus.Done
                    || rune.IsBmp && !XmlConvert.IsXmlChar((char)rune.Value))
                    return Refuse(Fail(AuthoringTextBodyReplacementFailureKind.InvalidPayload, "The literal intent contains an illegal XML scalar or unpaired surrogate."));
                i += used;
            }
            token.ThrowIfCancellationRequested();
            if (string.Equals(target.OriginalTextValue, text, StringComparison.Ordinal))
            {
                token.ThrowIfCancellationRequested();
                return new(null, new(target, text), null);
            }
            var source = snapshot.SourceFor(target.ModuleUri);
            long length = 0, chars = 0;
            for (var i = 0; i < text.Length;)
            {
                token.ThrowIfCancellationRequested();
                Rune.DecodeFromUtf16(text.AsSpan(i), out var rune, out var used);
                var escaped = Escape(rune);
                chars += escaped?.Length ?? used;
                length += escaped is not null ? source.Encoding.GetByteCount(escaped)
                    : source.Encoding.CodePage == 65001 ? rune.Utf8SequenceLength : used * 2;
                if (source.OriginalBytes.Length - target.OriginalBodyRange.ByteLength + length > Limit)
                    return Refuse(Fail(AuthoringTextBodyReplacementFailureKind.SizeLimit, "The complete encoded candidate exceeds the physical byte bound."));
                i += used;
            }
            var builder = new StringBuilder(checked((int)chars));
            for (var i = 0; i < text.Length;)
            {
                token.ThrowIfCancellationRequested();
                Rune.DecodeFromUtf16(text.AsSpan(i), out var rune, out var used);
                var escaped = Escape(rune);
                if (escaped is not null) builder.Append(escaped); else builder.Append(text.AsSpan(i, used));
                i += used;
            }
            var bodyText = builder.ToString();
            var encoded = source.Encoding.GetBytes(bodyText);
            var original = source.OriginalBytes.Span;
            var a = checked((int)target.OriginalBodyRange.StartByteOffset);
            var b = checked((int)(target.OriginalBodyRange.StartByteOffset + target.OriginalBodyRange.ByteLength));
            var bytes = new byte[checked(original.Length - (b - a) + encoded.Length)];
            original[..a].CopyTo(bytes); encoded.CopyTo(bytes, a); original[b..].CopyTo(bytes.AsSpan(a + encoded.Length));
            token.ThrowIfCancellationRequested();
            if (!AuthoringSource.TryCreate(bytes, target.ModuleUri, out var newSource, out var sourceFailure))
                return Refuse(Fail(AuthoringTextBodyReplacementFailureKind.InvalidCandidate, sourceFailure!.Message));
            var inspected = XsltAuthoring.Inspect(newSource!, new DenyResolver(), new() { AttemptCompilation = false, MaxModuleCount = 1 });
            token.ThrowIfCancellationRequested();
            if (!inspected.IsSuccess) return Refuse(Fail(AuthoringTextBodyReplacementFailureKind.InvalidCandidate, inspected.Failure!.Message));
            var candidate = inspected.Snapshot!;
            var body = newSource!.CoordinateMap.RangeFromCharOffsets(target.Start, target.Start + bodyText.Length);
            var oldStart = source.CoordinateMap.CharOffsetFromLineColumn(target.Instruction.Range.StartLine, target.Instruction.Range.StartColumn);
            var oldEnd = source.CoordinateMap.CharOffsetFromLineColumn(target.Instruction.Range.EndLine, target.Instruction.Range.EndColumn);
            var range = newSource.CoordinateMap.RangeFromCharOffsets(oldStart, oldEnd + bodyText.Length - (target.End - target.Start));
            var matches = ValueOfInsertionPipeline.Nodes(candidate.PrincipalModule.Root).Where(n => n.Kind == AuthoringNodeKind.Element && n.Range == range).ToArray();
            if (matches.Length != 1) return Refuse(Alignment());
            var edited = matches[0];
            var payload = edited.Children.Count == 1 ? edited.Children[0] : null;
            if (text.Length == 0 ? edited.Children.Count != 0
                : payload?.Kind != AuthoringNodeKind.Text || payload.Text != text || payload.Range != body)
                return Refuse(Alignment());
            var failure = Validate(candidate, true, token);
            if (failure is not null) return Refuse(failure);
            var map = Correspondence(target, candidate, newSource, bodyText.Length, edited, payload, token);
            if (map is null) return Refuse(Alignment());
            var all = ValueOfInsertionPipeline.Nodes(candidate.PrincipalModule.Root).ToDictionary(n => n.Id);
            if (!map.TryGetValue(target.Parent.Id, out var parentId) || !map.TryGetValue(target.Instruction.Id, out var editedId)
                || editedId != edited.Id || !all[parentId].Children.Contains(edited)) return Refuse(Alignment());
            var result = new AuthoringTextBodyReplacementCandidate(target, candidate.WithSuccessfulCompilation(), text, body,
                edited, all[parentId], payload, map);
            token.ThrowIfCancellationRequested();
            return new(result, null, null);
        }
        catch (OperationCanceledException) { return Refuse(Cancelled()); }
    }

    private static IReadOnlyDictionary<int, int>? Correspondence(AuthoringTextBodyReplacementTarget target,
        AuthoringSnapshot candidate, AuthoringSource newSource, int newChars, AuthoringNodeDescriptor edited,
        AuthoringNodeDescriptor? payload, CancellationToken token)
    {
        var oldSource = target.Owner.SourceFor(target.ModuleUri);
        var olds = ValueOfInsertionPipeline.Nodes(target.Owner.PrincipalModule.Root).ToArray();
        var news = ValueOfInsertionPipeline.Nodes(candidate.PrincipalModule.Root).ToArray();
        if (olds.Select(n => n.Id).Distinct().Count() != olds.Length || news.Select(n => n.Id).Distinct().Count() != news.Length) return null;
        var byId = news.ToDictionary(n => n.Id);
        var available = news.Where(n => !ReferenceEquals(n, payload)).GroupBy(n => (n.Kind, n.Range)).ToDictionary(g => g.Key, g => g.ToArray());
        var map = new Dictionary<int, int>();
        var used = new HashSet<int>();
        var delta = newChars - (target.End - target.Start);
        foreach (var old in olds)
        {
            token.ThrowIfCancellationRequested();
            if (ReferenceEquals(old, target.OriginalPayloadNode)) continue;
            var start = oldSource.CoordinateMap.CharOffsetFromLineColumn(old.Range.StartLine, old.Range.StartColumn);
            var end = oldSource.CoordinateMap.CharOffsetFromLineColumn(old.Range.EndLine, old.Range.EndColumn);
            // Container extents include the entire replaced body. Other nodes must lie wholly outside it.
            if (start < target.Start && end > target.End) end += delta;
            else if (start >= target.End) { start += delta; end += delta; }
            else if (end > target.Start) return null;
            var range = newSource.CoordinateMap.RangeFromCharOffsets(start, end);
            if (!available.TryGetValue((old.Kind, range), out var matches) || matches.Length != 1) return null;
            var fresh = matches[0];
            if (!used.Add(fresh.Id) || old.ElementName != fresh.ElementName || old.Text != fresh.Text
                || old.Attributes.Count != fresh.Attributes.Count) return null;
            for (var i = 0; i < old.Attributes.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                var a = old.Attributes[i]; var b = fresh.Attributes[i];
                if ((a.Name, a.Prefix, a.NamespaceUri, a.LocalName, a.SlotKind, a.RawLiteral, a.ExpandedValue)
                    != (b.Name, b.Prefix, b.NamespaceUri, b.LocalName, b.SlotKind, b.RawLiteral, b.ExpandedValue)) return null;
                // Attribute bytes may move with their owner but must retain exact lexical ranges.
                foreach (var pair in new[] { (a.FullRange, b.FullRange), (a.ValueRange, b.ValueRange) })
                {
                    var x = oldSource.CoordinateMap.CharOffsetFromLineColumn(pair.Item1.StartLine, pair.Item1.StartColumn);
                    var y = oldSource.CoordinateMap.CharOffsetFromLineColumn(pair.Item1.EndLine, pair.Item1.EndColumn);
                    if (x >= target.End) { x += delta; y += delta; }
                    if (newSource.CoordinateMap.RangeFromCharOffsets(x, y) != pair.Item2) return null;
                }
            }
            if (!map.TryAdd(old.Id, fresh.Id)) return null;
        }
        if (used.Count != news.Length - (payload is null ? 0 : 1)) return null;
        foreach (var old in olds.Where(n => !ReferenceEquals(n, target.OriginalPayloadNode)))
        {
            token.ThrowIfCancellationRequested();
            var fresh = byId[map[old.Id]];
            var expected = old.Children.Where(n => !ReferenceEquals(n, target.OriginalPayloadNode)).Select(n => map[n.Id]);
            var actual = fresh.Children.Where(n => !ReferenceEquals(n, payload)).Select(n => n.Id);
            if (!expected.SequenceEqual(actual)) return null;
        }
        if (edited.ElementName != target.Instruction.ElementName) return null;
        return new ReadOnlyDictionary<int, int>(map);
    }

    private static AuthoringTextBodyReplacementFailure? Scope(AuthoringSnapshot snapshot, AuthoringNodeDescriptor instruction,
        CancellationToken token, out XElement? retained)
    {
        retained = null;
        var module = snapshot.PrincipalModule;
        if (snapshot.Modules.Count != 1 || module.Edges.Count != 0)
            return Fail(AuthoringTextBodyReplacementFailureKind.UnsupportedModule, "Only a single principal module is supported.");
        var source = snapshot.SourceFor(module.ModuleUri);
        if (source.OriginalBytes.Length > Limit)
            return Fail(AuthoringTextBodyReplacementFailureKind.SizeLimit, "The original source exceeds the insertion byte limit.");
        XDocument document;
        try { document = ReadDocument(source); }
        catch (XmlException ex) { return Fail(AuthoringTextBodyReplacementFailureKind.InvalidOriginal, ex.Message); }
        token.ThrowIfCancellationRequested();
        var root = document.Root;
        if (root is null || root.Name.Namespace != Xsl || root.Name.LocalName is not ("stylesheet" or "transform")
            || (string?)root.Attribute("version") is not ("3.0" or "4.0"))
            return Fail(AuthoringTextBodyReplacementFailureKind.UnsupportedProfile, "An explicit XSLT 3.0 or 4.0 stylesheet is required.");
        if (!EncodingSupported(source, root.Document?.Declaration))
            return Fail(AuthoringTextBodyReplacementFailureKind.UnsupportedEncoding, "Insertion requires UTF-8 or BOM-marked UTF-16 with a matching declaration.");
        foreach (var element in root.DescendantsAndSelf())
        {
            token.ThrowIfCancellationRequested();
            if (element.Name.Namespace == Xsl && element.Name.LocalName is "include" or "import" or "use-package" or "package" or "import-schema")
                return Fail(AuthoringTextBodyReplacementFailureKind.UnsupportedModule, "Module, package and schema dependencies are outside insertion coverage.");
            // These constructs can execute user code during compiler preprocessing. Refuse before compilation,
            // including literal-looking shadow attributes, rather than evaluating a caller's static expressions.
            if (element.Attributes().Where(a => IsXsltAttribute(element, a)).Any(a => a.Name.LocalName is "fixed-namespaces" or "type-namespaces" or "extension-element-prefixes"
                || a.Name.LocalName.StartsWith('_') && element.Name.Namespace == Xsl && a.Name.NamespaceName.Length == 0
                || a.Name.LocalName == "static" && a.Value is not ("no" or "false" or "0")
                || a.Name.LocalName == "use-when"))
                return Fail(AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage, "Static evaluation and fixed/type namespace contexts are outside insertion coverage.");
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
            return Fail(AuthoringTextBodyReplacementFailureKind.IneligibleInstruction, "Only a literal-result-only chain beneath a top-level template is supported.");
        if (target.AncestorsAndSelf().Any(e => e.Attributes().Any(a =>
            (a.Name == "version" && e.Name.Namespace == Xsl || a.Name == Xsl + "version") && a.Value is not ("3.0" or "4.0"))))
            return Fail(AuthoringTextBodyReplacementFailureKind.UnsupportedProfile, "Effective versions must be 3.0 or 4.0.");
        foreach (var e in root.DescendantsAndSelf())
        {
            token.ThrowIfCancellationRequested();
            if (e.Attributes().Where(a => IsXsltAttribute(e, a)).Any(a => a.Name.LocalName is "as" or "type" or "validation" or "mode" or "default-mode" or "use-attribute-sets"))
                return Fail(AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage, "Type/component annotations are outside captured coverage.");
            if (EffectiveExpand(e) is "yes" or "true" or "1" && e.Nodes().OfType<XText>().Any(t => t.Value.Any(c => c is not (' ' or '\t' or '\r' or '\n'))))
                return Fail(AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage, "Expanded text requires additional static coverage.");
        }
        retained = target;
        return null;
    }

    private static string? EffectiveExpand(XElement e) => e.AncestorsAndSelf().Select(a =>
        (string?)a.Attribute(a.Name.Namespace == Xsl ? XName.Get("expand-text") : Xsl + "expand-text")).FirstOrDefault(v => v is not null);

    private static AuthoringTextBodyReplacementFailure? Validate(AuthoringSnapshot snapshot, bool candidate, CancellationToken token)
    {
        var failure = ValueOfInsertionPipeline.Validate(snapshot, candidate, token);
        if (failure is null) return null;
        var kind = failure.Kind switch
        {
            AuthoringInsertionFailureKind.UnsupportedCoverage => AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage,
            AuthoringInsertionFailureKind.ResourceDenied => AuthoringTextBodyReplacementFailureKind.ResourceDenied,
            AuthoringInsertionFailureKind.Cancelled => AuthoringTextBodyReplacementFailureKind.Cancelled,
            _ => candidate ? AuthoringTextBodyReplacementFailureKind.InvalidCandidate : AuthoringTextBodyReplacementFailureKind.InvalidOriginal,
        };
        return new(kind, failure.Message, failure.ErrorCode);
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

    private static bool IsXsltAttribute(XElement owner, XAttribute attribute) =>
        attribute.Name.Namespace == Xsl || owner.Name.Namespace == Xsl && attribute.Name.NamespaceName.Length == 0;

    private static string? Escape(Rune rune) => rune.Value switch
    { '&' => "&amp;", '<' => "&lt;", '>' => "&gt;", '\r' => "&#xD;", '\n' => "&#xA;", '\t' => "&#x9;", _ => null };
    private static AuthoringTextBodyReplacementFailure Fail(AuthoringTextBodyReplacementFailureKind kind, string message) => new(kind, message);
    private static AuthoringTextBodyReplacementFailure Alignment() => Fail(AuthoringTextBodyReplacementFailureKind.SourceAlignmentFailure, "Complete body and survivor evidence could not be proven.");
    private static AuthoringTextBodyReplacementFailure Cancelled() => Fail(AuthoringTextBodyReplacementFailureKind.Cancelled, "Replacement was cancelled; no candidate returned.");
    private static AuthoringTextBodyReplacementResult Refuse(AuthoringTextBodyReplacementFailure failure) => new(null, null, failure);
    private sealed class DenyResolver : IAuthoringModuleResolver
    {
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) => null;
    }
}
