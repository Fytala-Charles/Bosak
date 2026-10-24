// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 October 2026
// PURPOSE              : Prove exact element rename and subtree deletion with exhaustive graph provenance.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 23-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

//                      | Charles Korthout | 0.2   | 24-10-2026     | Share unchanged closed preflight with REQ-137 reorder proof. |

using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Bosak.Xslt.Authoring;

internal static class LiteralElementLifecyclePipeline
{
    private const string Xsl = "http://www.w3.org/1999/XSL/Transform";
    // Closed whole-module vocabulary, checked before any compiler is entered.
    private static readonly Dictionary<string, string[]> Instructions = new(StringComparer.Ordinal)
    {
        ["stylesheet"] = ["version", "id", "exclude-result-prefixes"],
        ["transform"] = ["version", "id", "exclude-result-prefixes"],
        ["output"] = ["method", "version", "encoding", "omit-xml-declaration", "standalone", "indent", "media-type"],
        ["template"] = ["match", "name", "priority", "version", "exclude-result-prefixes"],
        ["param"] = ["name", "select", "required", "tunnel"],
        ["variable"] = ["name", "select"],
        ["function"] = ["name", "override"],
        ["value-of"] = ["select", "separator"],
        ["sequence"] = ["select"],
        ["text"] = [],
        ["if"] = ["test"],
        ["choose"] = [],
        ["when"] = ["test"],
        ["otherwise"] = [],
        ["for-each"] = ["select"],
        ["comment"] = ["select"],
        ["processing-instruction"] = ["name", "select"],
    };

    private static AuthoringLiteralElementFailure Fail(AuthoringSnapshot s, AuthoringLiteralElementFailureKind k, string m, string? code = null) =>
        new(k, m, s.PrincipalModule.ModuleUri, code);
    private static AuthoringLiteralElementFailure Convert(AuthoringSnapshot s, AuthoringLiteralAttributeFailure f) =>
        Fail(s, Enum.TryParse<AuthoringLiteralElementFailureKind>(f.Kind.ToString(), out var k) ? k : AuthoringLiteralElementFailureKind.UnsupportedCoverage, f.Message, f.Code);

    internal static (LiteralElementTargetData? Data, AuthoringLiteralElementFailure? Failure) Discover(
        AuthoringSnapshot s, AuthoringNodeDescriptor node, CancellationToken t)
    {
        try
        {
            t.ThrowIfCancellationRequested();
            var f = Preflight(s, t);
            if (f is not null) return (null, f);
            var nodes = Traverse(s.PrincipalModule.Root, t).ToArray();
            if (!nodes.Any(n => ReferenceEquals(n, node)))
                return (null, Fail(s, AuthoringLiteralElementFailureKind.ForeignSnapshot, "Actual descriptor identity from this snapshot is required."));
            if (node.Kind != AuthoringNodeKind.Element || node.ElementName?.NamespaceUri != "")
                return (null, Fail(s, AuthoringLiteralElementFailureKind.IneligibleTarget, "An ordinary no-namespace literal result element is required."));
            var parent = nodes.SingleOrDefault(n => n.Children.Any(c => ReferenceEquals(c, node)));
            if (parent is null) return (null, Fail(s, AuthoringLiteralElementFailureKind.IneligibleTarget, "The module root cannot be changed."));
            // Reuse constant-attribute recognition, lexical alignment, retained host and restricted compilation.
            var a = LiteralAttributeLifecyclePipeline.Discover(s, node, null, t);
            if (a.Failure is not null) return (null, Convert(s, a.Failure));
            var source = s.SourceFor(s.PrincipalModule.ModuleUri);
            var start = Offset(source, node.Range, false);
            var end = SourceTagScanner.FindElementEnd(source.Text, start, out var openingEnd, out var self);
            if (source.CoordinateMap.RangeFromCharOffsets(start, end) != node.Range)
                return (null, Fail(s, AuthoringLiteralElementFailureKind.SourceAlignmentFailure, "Selected element source does not align."));
            t.ThrowIfCancellationRequested();
            return (new(s, node, parent, start, end, openingEnd, self), null);
        }
        catch (OperationCanceledException) { return (null, Fail(s, AuthoringLiteralElementFailureKind.Cancelled, "Cancelled without authority.")); }
    }

    internal static AuthoringLiteralElementRenameResult Rename(AuthoringSnapshot s, AuthoringLiteralElementRenameProposal p, CancellationToken t)
    {
        var r = Run(s, p.Target.Data, p.NewLocalName, t);
        return new(r.Candidate, r.NoChange ? new(p.Target) : null, r.Failure);
    }
    internal static AuthoringLiteralElementDeletionResult Deletion(AuthoringSnapshot s, AuthoringLiteralElementDeletionProposal p, CancellationToken t)
    {
        var r = Run(s, p.Target.Data, null, t);
        return new(r.Candidate, r.Failure);
    }

    private static (AuthoringLiteralElementCandidate? Candidate, bool NoChange, AuthoringLiteralElementFailure? Failure) Run(
        AuthoringSnapshot s, LiteralElementTargetData target, string? name, CancellationToken t)
    {
        (AuthoringLiteralElementCandidate?, bool, AuthoringLiteralElementFailure?) Refuse(AuthoringLiteralElementFailureKind k, string m) => (null, false, Fail(s, k, m));
        try
        {
            t.ThrowIfCancellationRequested();
            if (!ReferenceEquals(target.Snapshot, s)) return Refuse(AuthoringLiteralElementFailureKind.ForeignSnapshot, "The operation target belongs to another snapshot.");
            // Cheap caller-name bound precedes NCName validation or source work.
            if (name?.Length > AuthoringCapabilities.MaxLiteralElementLocalNameUnits)
                return Refuse(AuthoringLiteralElementFailureKind.SizeLimit, "The requested name exceeds the UTF-16 unit limit.");
            var discovery = Discover(s, target.Node, t);
            if (discovery.Failure is not null) return (null, false, discovery.Failure);
            target = discovery.Data!;
            var source = s.SourceFor(s.PrincipalModule.ModuleUri);
            if (name is not null)
            {
                if (name.Length == 0) return Refuse(AuthoringLiteralElementFailureKind.InvalidName, "An empty name is not an NCName.");
                try { XmlConvert.VerifyNCName(name); XmlConvert.VerifyXmlChars(name); }
                catch (XmlException) { return Refuse(AuthoringLiteralElementFailureKind.InvalidName, "An exact unqualified XML NCName is required."); }
                if (source.Encoding.GetByteCount(name) > AuthoringCapabilities.MaxLiteralElementLocalNameBytes)
                    return Refuse(AuthoringLiteralElementFailureKind.SizeLimit, "The encoded name exceeds the physical byte limit.");
                if (string.Equals(name, target.Node.ElementName!.LocalName, StringComparison.Ordinal))
                { t.ThrowIfCancellationRequested(); return (null, true, null); }
            }
            var changes = new List<(int Start, int End, string Replacement)>();
            if (name is null) changes.Add((target.Start, target.End, ""));
            else
            {
                var oldName = target.Node.ElementName!.LocalName;
                changes.Add((target.Start + 1, target.Start + 1 + oldName.Length, name));
                if (!target.SelfClosing)
                {
                    var close = source.Text.LastIndexOf("</", target.End - 1, target.End - target.Start, StringComparison.Ordinal);
                    if (close < target.OpeningEnd || source.Text.AsSpan(close + 2, oldName.Length).SequenceEqual(oldName) == false)
                        return Refuse(AuthoringLiteralElementFailureKind.SourceAlignmentFailure, "The actual closing name token does not align.");
                    changes.Add((close + 2, close + 2 + oldName.Length, name));
                }
            }
            var ranges = changes.Select(c => source.CoordinateMap.RangeFromCharOffsets(c.Start, c.End)).ToArray();
            var replacements = changes.Select(c => source.Encoding.GetBytes(c.Replacement)).ToArray();
            long length = source.OriginalBytes.Length;
            for (var i = 0; i < ranges.Length; i++) length = checked(length - ranges[i].ByteLength + replacements[i].Length);
            if (length > AuthoringCapabilities.MaxLiteralElementLifecycleSourceBytes)
                return Refuse(AuthoringLiteralElementFailureKind.SizeLimit, "Candidate physical source exceeds its independent limit.");
            // Conservative copied payload accounting: retained envelopes plus complete old/new selected
            // extents and both payloads of every edit, even though exports are made only on request.
            long evidence = checked(source.OriginalBytes.Length + length + target.Node.Range.ByteLength * 2 + (name is null ? 0 : length - source.OriginalBytes.Length));
            for (var i = 0; i < ranges.Length; i++) evidence = checked(evidence + ranges[i].ByteLength + replacements[i].Length);
            if (evidence > AuthoringCapabilities.MaxLiteralElementLifecycleEvidenceBytes)
                return Refuse(AuthoringLiteralElementFailureKind.SizeLimit, "Complete review evidence exceeds its independent byte budget.");
            var bytes = new byte[checked((int)length)]; var read = 0; var written = 0;
            for (var i = 0; i < ranges.Length; i++)
            {
                var a = checked((int)ranges[i].StartByteOffset); var b = checked(a + (int)ranges[i].ByteLength);
                Copy(source.OriginalBytes.Span[read..a], bytes.AsSpan(written), t); written += a - read;
                Copy(replacements[i], bytes.AsSpan(written), t); written += replacements[i].Length; read = b;
            }
            Copy(source.OriginalBytes.Span[read..], bytes.AsSpan(written), t);
            if (!AuthoringSource.TryCreate(bytes, source.BaseUri, out var freshSource, out var sf))
                return Refuse(AuthoringLiteralElementFailureKind.InvalidCandidate, sf!.Message);
            var preflight = CheckXml(freshSource!, t);
            if (preflight is not null) return Refuse(preflight.Value.Kind, preflight.Value.Message);
            var inspection = XsltAuthoring.Inspect(freshSource!, new DenyResolver(), new() { AttemptCompilation = false, MaxModuleCount = 1 });
            t.ThrowIfCancellationRequested();
            if (!inspection.IsSuccess) return Refuse(AuthoringLiteralElementFailureKind.InvalidCandidate, inspection.Failure!.Message);
            var fresh = inspection.Snapshot!;
            var f = Preflight(fresh, t);
            if (f is not null) return (null, false, f);
            var scope = LiteralAttributeLifecyclePipeline.Scope(fresh, null, t, out _);
            if (scope is not null) return (null, false, Convert(fresh, scope));
            var validation = LiteralAttributeLifecyclePipeline.Validate(fresh, true, t);
            if (validation is not null) return (null, false, Convert(fresh, validation));
            var proof = new Proof(source, freshSource!, target, name, changes, t);
            if (!proof.Match(s.PrincipalModule.Root, fresh.PrincipalModule.Root))
                return Refuse(AuthoringLiteralElementFailureKind.UnsupportedCorrespondence, "Complete node, attribute and boundary-merge provenance could not be proven.");
            var removed = name is null ? Traverse(target.Node, t).ToArray() : Array.Empty<AuthoringNodeDescriptor>();
            var oldNodes = Traverse(s.PrincipalModule.Root, t).ToArray(); var newNodes = Traverse(fresh.PrincipalModule.Root, t).ToArray();
            var contributors = proof.Groups.SelectMany(g => g.Fragments.Select(x => x.OriginalNode)).ToArray();
            if (proof.Map.Count + removed.Length + contributors.Length != oldNodes.Length
                || proof.Map.Count + proof.Groups.Count != newNodes.Length)
                return Refuse(AuthoringLiteralElementFailureKind.UnsupportedCorrespondence, "Descriptor partitions are not exhaustive.");
            var edits = new List<AuthoringInsertionSourceEdit>(); var delta = 0;
            for (var i = 0; i < changes.Count; i++)
            {
                var c = changes[i]; var a = checked(c.Start + delta);
                edits.Add(new(ranges[i], freshSource!.CoordinateMap.RangeFromCharOffsets(a, checked(a + c.Replacement.Length))));
                delta = checked(delta + c.Replacement.Length - (c.End - c.Start));
            }
            // Replay proof uses all original offsets against the same original, in descending order.
            var replay = source.OriginalBytes.ToArray().ToList();
            for (var i = edits.Count - 1; i >= 0; i--)
            {
                t.ThrowIfCancellationRequested(); var r = edits[i].OriginalRange; var at = checked((int)r.StartByteOffset);
                replay.RemoveRange(at, checked((int)r.ByteLength)); replay.InsertRange(at, replacements[i]);
            }
            if (!replay.SequenceEqual(bytes)) return Refuse(AuthoringLiteralElementFailureKind.SourceAlignmentFailure, "Same-original edit replay does not match the candidate.");
            var result = new AuthoringLiteralElementCandidate(s, fresh.WithSuccessfulCompilation(), name is null ? AuthoringLiteralElementOperation.Delete : AuthoringLiteralElementOperation.Rename,
                target.Node, proof.Map[name is null ? target.Parent : target.Node],
                new ReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor>(proof.Map), Array.AsReadOnly(proof.Attributes.ToArray()),
                Array.AsReadOnly(removed), Array.AsReadOnly(removed.SelectMany(n => n.Attributes).ToArray()),
                Array.AsReadOnly(proof.Groups.ToArray()), Array.AsReadOnly(edits.ToArray()));
            t.ThrowIfCancellationRequested();
            return (result, false, null);
        }
        catch (OperationCanceledException) { return Refuse(AuthoringLiteralElementFailureKind.Cancelled, "Cancelled without candidate or NoChange authority."); }
    }

    internal static AuthoringLiteralElementFailure? Preflight(AuthoringSnapshot s, CancellationToken t)
    {
        if (s.Modules.Count != 1 || s.PrincipalModule.Edges.Count != 0)
            return Fail(s, AuthoringLiteralElementFailureKind.UnsupportedModule, "One dependency-free principal module is required.");
        var source = s.SourceFor(s.PrincipalModule.ModuleUri);
        var xml = CheckXml(source, t);
        if (xml is not null) return Fail(s, xml.Value.Kind, xml.Value.Message);
        var nodes = 0; var attributes = 0;
        var stack = new Stack<(AuthoringNodeDescriptor Node, int Depth)>(); stack.Push((s.PrincipalModule.Root, 0));
        while (stack.TryPop(out var entry))
        {
            t.ThrowIfCancellationRequested(); nodes++; attributes = checked(attributes + entry.Node.Attributes.Count);
            if (nodes > AuthoringCapabilities.MaxLiteralElementLifecycleNodes || attributes > AuthoringCapabilities.MaxLiteralElementLifecycleAttributes
                || entry.Depth > AuthoringCapabilities.MaxLiteralElementLifecycleDepth)
                return Fail(s, AuthoringLiteralElementFailureKind.SizeLimit, "Descriptor count, attribute count or depth exceeds its independent bound.");
            foreach (var child in entry.Node.Children) stack.Push((child, entry.Depth + 1));
        }
        return null;
    }

    // Streaming XML preflight precedes candidate inspection, DOM construction and resource-capable validation.
    internal static (AuthoringLiteralElementFailureKind Kind, string Message)? CheckXml(AuthoringSource source, CancellationToken t)
    {
        if (source.OriginalBytes.Length > AuthoringCapabilities.MaxLiteralElementLifecycleSourceBytes)
            return (AuthoringLiteralElementFailureKind.SizeLimit, "Physical source exceeds its independent byte limit.");
        try
        {
            using var reader = XmlReader.Create(new StringReader(source.Text), new() { XmlResolver = null, DtdProcessing = DtdProcessing.Prohibit });
            var count = 0; var attributes = 0;
            while (reader.Read())
            {
                t.ThrowIfCancellationRequested(); count++;
                if (reader.Depth >= AuthoringCapabilities.MaxLiteralElementLifecycleDepth || count > AuthoringCapabilities.MaxLiteralElementLifecycleNodes * 2)
                    return (AuthoringLiteralElementFailureKind.SizeLimit, "XML depth or token count exceeds its pre-inspection bound.");
                if (reader.NodeType == XmlNodeType.CDATA)
                    return (AuthoringLiteralElementFailureKind.UnsupportedCoverage, "CDATA is outside element lifecycle coverage.");
                if (reader.NodeType != XmlNodeType.Element) continue;
                attributes = checked(attributes + reader.AttributeCount);
                if (attributes > AuthoringCapabilities.MaxLiteralElementLifecycleAttributes)
                    return (AuthoringLiteralElementFailureKind.SizeLimit, "XML attribute count exceeds its pre-inspection bound.");
                var instruction = reader.NamespaceURI == Xsl; var local = reader.LocalName;
                if (instruction && !Instructions.ContainsKey(local))
                    return (AuthoringLiteralElementFailureKind.UnsupportedCoverage, $"Instruction {local} is outside the closed lifecycle vocabulary.");
                if (!reader.MoveToFirstAttribute()) continue;
                do
                {
                    t.ThrowIfCancellationRequested();
                    if (reader.NamespaceURI == XNamespace.Xmlns.NamespaceName || reader.Name == "xmlns") continue;
                    if (reader.NamespaceURI == XNamespace.Xml.NamespaceName && reader.LocalName == "space")
                    {
                        if (reader.Value != "default") return (AuthoringLiteralElementFailureKind.UnsupportedCoverage, "Preserved-space contexts are outside lifecycle coverage.");
                        continue;
                    }
                    if (instruction && reader.NamespaceURI.Length == 0 && reader.LocalName == "expand-text" && reader.Value is "no" or "false" or "0") continue;
                    if (instruction && reader.NamespaceURI.Length == 0 && reader.LocalName == "disable-output-escaping" && local is "text" or "value-of" && reader.Value == "no") continue;
                    if (instruction && (reader.NamespaceURI.Length != 0 || !Instructions[local].Contains(reader.LocalName, StringComparer.Ordinal)))
                        return (AuthoringLiteralElementFailureKind.UnsupportedCoverage, "An instruction attribute is outside the closed lifecycle vocabulary.");
                    if (!instruction && reader.NamespaceURI == Xsl && reader.LocalName is not ("version" or "exclude-result-prefixes"))
                        return (AuthoringLiteralElementFailureKind.UnsupportedCoverage, "A result-element control attribute is outside lifecycle coverage.");
                } while (reader.MoveToNextAttribute());
                reader.MoveToElement();
            }
            return null;
        }
        catch (XmlException ex) { return (AuthoringLiteralElementFailureKind.InvalidOriginal, ex.Message); }
    }

    private sealed class Proof(AuthoringSource original, AuthoringSource candidate, LiteralElementTargetData target,
        string? name, List<(int Start, int End, string Replacement)> changes, CancellationToken token)
    {
        internal Dictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor> Map { get; } = new();
        internal List<AuthoringLiteralAttributeCorrespondence> Attributes { get; } = new();
        internal List<AuthoringTextMergeGroup> Groups { get; } = new();
        private readonly HashSet<AuthoringNodeDescriptor> used = new();
        private int Translate(int value)
        {
            var delta = 0;
            foreach (var c in changes)
            {
                if (value <= c.Start) break;
                if (value < c.End) return -1;
                delta = checked(delta + c.Replacement.Length - (c.End - c.Start));
            }
            return checked(value + delta);
        }
        private SourceRange? Range(SourceRange r)
        {
            var a = Translate(Offset(original, r, false)); var b = Translate(Offset(original, r, true));
            return a < 0 || b < a || b > candidate.Text.Length ? null : candidate.CoordinateMap.RangeFromCharOffsets(a, b);
        }
        internal bool Match(AuthoringNodeDescriptor old, AuthoringNodeDescriptor fresh)
        {
            token.ThrowIfCancellationRequested();
            var expectedName = name is not null && ReferenceEquals(old, target.Node) ? new AuthoringQName("", "", name) : old.ElementName;
            if (name is null && ReferenceEquals(old, target.Node) || old.Kind != fresh.Kind || expectedName != fresh.ElementName
                || old.Text != fresh.Text || Range(old.Range) != fresh.Range || old.Attributes.Count != fresh.Attributes.Count
                || !used.Add(fresh) || !Map.TryAdd(old, fresh)) return false;
            var attrs = new Dictionary<AuthoringAttributeDescriptor, AuthoringAttributeDescriptor>();
            for (var i = 0; i < old.Attributes.Count; i++)
            {
                token.ThrowIfCancellationRequested(); var a = old.Attributes[i]; var b = fresh.Attributes[i];
                if ((a.Name, a.LocalName, a.NamespaceUri, a.Prefix, a.RawLiteral, a.ExpandedValue, a.SlotKind)
                    != (b.Name, b.LocalName, b.NamespaceUri, b.Prefix, b.RawLiteral, b.ExpandedValue, b.SlotKind)
                    || Range(a.FullRange) != b.FullRange || Range(a.ValueRange) != b.ValueRange || !attrs.TryAdd(a, b)) return false;
            }
            if (old.Kind == AuthoringNodeKind.Element) Attributes.Add(new(old, fresh, attrs));
            var oi = 0; var ni = 0;
            while (oi < old.Children.Count)
            {
                token.ThrowIfCancellationRequested(); var child = old.Children[oi];
                if (name is null && ReferenceEquals(child, target.Node)) { oi++; continue; }
                if (name is null && ReferenceEquals(old, target.Parent) && oi + 2 < old.Children.Count
                    && ReferenceEquals(old.Children[oi + 1], target.Node)
                    && child.Kind == AuthoringNodeKind.Text && old.Children[oi + 2].Kind == AuthoringNodeKind.Text)
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
            bool White(AuthoringNodeDescriptor n) => n.Kind == AuthoringNodeKind.Text && n.Text is not null && n.Text.All(c => c is ' ' or '\t' or '\r' or '\n');
            if (Groups.Count != 0 || !White(left) || !White(right) || !White(fresh) || !used.Add(fresh)
                || Offset(original, left.Range, true) != target.Start || Offset(original, right.Range, false) != target.End) return false;
            var l = Range(left.Range); var r = Range(right.Range);
            if (l is null || r is null || l.StartByteOffset != fresh.Range.StartByteOffset
                || l.StartByteOffset + l.ByteLength != r.StartByteOffset || r.StartByteOffset + r.ByteLength != fresh.Range.StartByteOffset + fresh.Range.ByteLength) return false;
            foreach (var pair in new[] { (left.Range, l), (right.Range, r) })
            {
                token.ThrowIfCancellationRequested();
                if (!original.OriginalBytes.Span.Slice(checked((int)pair.Item1.StartByteOffset), checked((int)pair.Item1.ByteLength))
                    .SequenceEqual(candidate.OriginalBytes.Span.Slice(checked((int)pair.Item2.StartByteOffset), checked((int)pair.Item2.ByteLength)))) return false;
            }
            Groups.Add(new(fresh, new[] { new AuthoringTextMergeFragment(left, left.Range, l), new AuthoringTextMergeFragment(right, right.Range, r) }));
            return true;
        }
    }
    private static IEnumerable<AuthoringNodeDescriptor> Traverse(AuthoringNodeDescriptor root, CancellationToken t)
    {
        var stack = new Stack<AuthoringNodeDescriptor>(); stack.Push(root);
        while (stack.TryPop(out var n)) { t.ThrowIfCancellationRequested(); yield return n; for (var i = n.Children.Count - 1; i >= 0; i--) stack.Push(n.Children[i]); }
    }
    private static int Offset(AuthoringSource s, SourceRange r, bool end) => s.CoordinateMap.CharOffsetFromLineColumn(end ? r.EndLine : r.StartLine, end ? r.EndColumn : r.StartColumn);
    private static void Copy(ReadOnlySpan<byte> source, Span<byte> destination, CancellationToken t)
    {
        for (var i = 0; i < source.Length; i += Math.Min(65536, source.Length - i))
        { t.ThrowIfCancellationRequested(); var n = Math.Min(65536, source.Length - i); source.Slice(i, n).CopyTo(destination.Slice(i, n)); }
    }
    private sealed class DenyResolver : IAuthoringModuleResolver
    {
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) => null;
    }
}
