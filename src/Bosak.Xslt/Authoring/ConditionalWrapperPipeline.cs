// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 October 2026
// PURPOSE              : Bounded conditional wrapper lifecycle and complete source provenance.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Bosak.Xslt.Authoring;

internal static class ConditionalWrapperPipeline
{
    internal const string Xsl = "http://www.w3.org/1999/XSL/Transform";
    internal static AuthoringConditionalFailure Failure(AuthoringConditionalFailureKind kind, string message) => new(kind, message);
    internal static AuthoringConditionalFailure Convert(string kind, string message) => Failure(
        Enum.TryParse<AuthoringConditionalFailureKind>(kind, out var result) ? result : AuthoringConditionalFailureKind.UnsupportedCoverage, message);

    internal static AuthoringConditionalFailure? Check(AuthoringSnapshot snapshot, bool candidate, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var f = LiteralElementLifecyclePipeline.Preflight(snapshot, token);
        if (f is not null) return Convert(f.Kind.ToString(), f.Message);
        // Independently align every original descriptor, including markup/test that Unwrap removes.
        // Comparing only survivors would miss forged metadata on the removed wrapper itself.
        var aligned = Inspect(snapshot.SourceFor(snapshot.PrincipalModule.ModuleUri), token);
        if (!aligned.IsSuccess || !Align(snapshot.PrincipalModule.Root, aligned.Snapshot!.PrincipalModule.Root, token))
            return Failure(AuthoringConditionalFailureKind.SourceAlignmentFailure, "Complete descriptors and attributes must match an independent ordinary source inspection.");
        var scope = LiteralAttributeLifecyclePipeline.Scope(snapshot, null, token, out _);
        if (scope is not null) return Convert(scope.Kind.ToString(), scope.Message);
        var validation = LiteralAttributeLifecyclePipeline.Validate(snapshot, candidate, token);
        token.ThrowIfCancellationRequested();
        return validation is null ? null : Convert(validation.Kind.ToString(), validation.Message);
    }

    private static bool Align(AuthoringNodeDescriptor old, AuthoringNodeDescriptor actual, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if ((old.Kind, old.ElementName, old.Text, old.Range) != (actual.Kind, actual.ElementName, actual.Text, actual.Range)
            || old.Children.Count != actual.Children.Count || old.Attributes.Count != actual.Attributes.Count) return false;
        for (var i = 0; i < old.Attributes.Count; i++)
        {
            token.ThrowIfCancellationRequested(); var a = old.Attributes[i]; var b = actual.Attributes[i];
            if ((a.Name,a.Prefix,a.NamespaceUri,a.LocalName,a.SlotKind,a.RawLiteral,a.ExpandedValue,a.FullRange,a.ValueRange)
                != (b.Name,b.Prefix,b.NamespaceUri,b.LocalName,b.SlotKind,b.RawLiteral,b.ExpandedValue,b.FullRange,b.ValueRange)) return false;
        }
        for (var i = 0; i < old.Children.Count; i++) if (!Align(old.Children[i], actual.Children[i], token)) return false;
        return true;
    }

    internal static (ConditionalTargetData? Data, AuthoringConditionalFailure? Failure) Discover(
        AuthoringSnapshot snapshot, AuthoringNodeDescriptor node, bool wrap, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            var preflight = LiteralElementLifecyclePipeline.Preflight(snapshot, token);
            if (preflight is not null) return (null, Convert(preflight.Kind.ToString(), preflight.Message));
            var nodes = Nodes(snapshot.PrincipalModule.Root, token).ToArray();
            if (!nodes.Contains(node)) return (null, Failure(AuthoringConditionalFailureKind.ForeignSnapshot, "Actual snapshot-owned descriptor identity is required."));
            if (wrap && (nodes.Length + 1 > AuthoringCapabilities.MaxConditionalNodes
                || nodes.Sum(n => (long)n.Attributes.Count) + 1 > AuthoringCapabilities.MaxConditionalAttributes))
                return (null, Failure(AuthoringConditionalFailureKind.SizeLimit, "The added wrapper/test exceeds the independent candidate graph limits."));
            if (wrap)
            {
                var depths = Depths(snapshot.PrincipalModule.Root, token);
                if (Nodes(node, token).Any(n => depths[n] + 1 > AuthoringCapabilities.MaxConditionalDepth))
                    return (null, Failure(AuthoringConditionalFailureKind.SizeLimit, "Candidate subtree depth exceeds its independent limit."));
            }
            var f = Check(snapshot, false, token); if (f is not null) return (null, f);
            var parent = nodes.SingleOrDefault(n => n.Children.Contains(node));
            if (parent is null || node.Kind != AuthoringNodeKind.Element)
                return (null, Failure(AuthoringConditionalFailureKind.IneligibleTarget, "An eligible contained element is required."));
            var source = snapshot.SourceFor(snapshot.PrincipalModule.ModuleUri);
            var start = Offset(source, node.Range, false);
            var end = SourceTagScanner.FindElementEnd(source.Text, start, out var openingEnd, out var self);
            if (source.CoordinateMap.RangeFromCharOffsets(start, end) != node.Range)
                return (null, Failure(AuthoringConditionalFailureKind.SourceAlignmentFailure, "The actual selected extent does not align."));
            var element = node;
            var closing = self ? end : source.Text.LastIndexOf("</", end - 1, end - start, StringComparison.Ordinal);
            if (!wrap)
            {
                if (node.ElementName?.NamespaceUri != Xsl || node.ElementName.LocalName != "if" || self
                    || node.Attributes.Count != 1 || node.Attributes[0].Name != "test" || node.Attributes[0].NamespaceUri != ""
                    || node.Attributes[0].SlotKind != AuthoringAttributeSlotKind.Expression)
                    return (null, Failure(AuthoringConditionalFailureKind.IneligibleTarget, "Unwrap requires a paired if with only one unqualified test attribute and no declarations."));
                var children = node.Children.Where(n => n.Kind == AuthoringNodeKind.Element).ToArray();
                if (children.Length != 1 || node.Children.Any(n => n.Kind != AuthoringNodeKind.Element && !PlainWhitespace(source, n, token)))
                    return (null, Failure(AuthoringConditionalFailureKind.IneligibleTarget, "Exactly one literal child and optional plain lexical whitespace are required."));
                element = children[0];
                var index = parent.Children.ToList().IndexOf(node);
                foreach (var neighbor in new[] { index > 0 ? parent.Children[index - 1] : null, index + 1 < parent.Children.Count ? parent.Children[index + 1] : null })
                    if (neighbor?.Kind == AuthoringNodeKind.Text && !PlainWhitespace(source, neighbor, token))
                        return (null, Failure(AuthoringConditionalFailureKind.IneligibleTarget, "Adjoining boundary text must have plain whitespace spelling."));
            }
            if (element.Kind != AuthoringNodeKind.Element || element.ElementName?.NamespaceUri != "" || element.BackingObject is not XElement actual
                || parent.BackingObject is not XElement p || !SupportedParent(p))
                return (null, Failure(AuthoringConditionalFailureKind.IneligibleTarget, "A no-namespace literal E under top-level-template/literal-only ancestry is required."));
            // Precisely one approved if edge is bypassed; existing lifecycle ancestry rules are unchanged.
            if (!ReferenceEquals(actual.Parent, wrap ? p : node.BackingObject))
                return (null, Failure(AuthoringConditionalFailureKind.SourceAlignmentFailure, "The retained parent edge does not align."));
            if (actual.AncestorsAndSelf().Select(e => e.Attribute(XNamespace.Xml + "space")).FirstOrDefault(a => a is not null)?.Value == "preserve")
                return (null, Failure(AuthoringConditionalFailureKind.IneligibleTarget, "Effective preserved-space is outside the conditional profile."));
            string? prefix = null;
            if (wrap)
            {
                var bindings = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var e in p.AncestorsAndSelf().Reverse())
                    foreach (var a in e.Attributes().Where(a => a.IsNamespaceDeclaration))
                    { token.ThrowIfCancellationRequested(); bindings[a.Name.LocalName == "xmlns" ? "" : a.Name.LocalName] = a.Value; }
                prefix = bindings.Where(b => b.Key.Length is > 0 and <= 256 && b.Value == Xsl).Select(b => b.Key).Order(StringComparer.Ordinal).FirstOrDefault();
                if (prefix is null) return (null, Failure(AuthoringConditionalFailureKind.NoSafeXsltPrefix, "A bounded nonempty existing XSLT alias in the parent context is required."));
            }
            token.ThrowIfCancellationRequested();
            return (new(snapshot, node, element, parent, start, end, openingEnd, closing, prefix), null);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        { return (null, Failure(AuthoringConditionalFailureKind.Cancelled, "Cancelled without target authority.")); }
    }

    private static bool SupportedParent(XElement parent)
    {
        var current = parent;
        while (current.Name.NamespaceName == "")
        { if (current.Parent is null) return false; current = current.Parent; }
        return current.Name == XName.Get("template", Xsl) && current.Parent?.Name.NamespaceName == Xsl
            && current.Parent.Name.LocalName is "stylesheet" or "transform";
    }

    internal static bool PlainWhitespace(AuthoringSource source, AuthoringNodeDescriptor node, CancellationToken token)
    {
        if (node.Kind != AuthoringNodeKind.Text || node.Text is null) return false;
        var a = Offset(source, node.Range, false); var b = Offset(source, node.Range, true);
        for (var i = a; i < b; i++) { if ((i & 16383) == 0) token.ThrowIfCancellationRequested(); if (source.Text[i] is not (' ' or '\t' or '\r' or '\n')) return false; }
        return true;
    }

    internal static AuthoringConditionalResult Propose(AuthoringSnapshot snapshot, ConditionalTargetData target, string? condition, bool wrap, CancellationToken token)
    {
        AuthoringConditionalResult Refuse(AuthoringConditionalFailureKind kind, string message) => new(null, Failure(kind, message));
        try
        {
            token.ThrowIfCancellationRequested();
            if (!ReferenceEquals(target.Snapshot, snapshot)) return Refuse(AuthoringConditionalFailureKind.ForeignSnapshot, "The operation target belongs to another snapshot.");
            if (condition?.Length > AuthoringCapabilities.MaxConditionalConditionUnits) return Refuse(AuthoringConditionalFailureKind.SizeLimit, "The condition exceeds the UTF-16 unit limit.");
            var discovery = Discover(snapshot, target.Node, wrap, token);
            if (discovery.Failure is not null) return new(null, discovery.Failure);
            target = discovery.Data!;
            var source = snapshot.SourceFor(snapshot.PrincipalModule.ModuleUri);
            string opening = "", closing = "";
            if (wrap)
            {
                if (string.IsNullOrWhiteSpace(condition)) return Refuse(AuthoringConditionalFailureKind.InvalidCondition, "A nonempty XPath expression is required.");
                string escaped;
                try { escaped = EscapeCondition(condition!, source.Encoding, '"', token); }
                catch (XmlException) { return Refuse(AuthoringConditionalFailureKind.InvalidCondition, "The condition contains an invalid XML scalar."); }
                catch (ConditionalLimitException ex) { return Refuse(AuthoringConditionalFailureKind.SizeLimit, ex.Message); }
                opening = "<" + target.Prefix + ":if test=\"" + escaped + "\">";
                closing = "</" + target.Prefix + ":if>";
                if (source.Encoding.GetByteCount(opening) + source.Encoding.GetByteCount(closing) > AuthoringCapabilities.MaxConditionalMarkupBytes)
                    return Refuse(AuthoringConditionalFailureKind.SizeLimit, "Encoded wrapper markup exceeds its independent limit.");
            }
            var changes = wrap ? new[] { new ConditionalChange(target.Start, target.Start, opening), new ConditionalChange(target.End, target.End, closing) }
                : new[] { new ConditionalChange(target.Start, target.OpeningEnd, ""), new ConditionalChange(target.ClosingStart, target.End, "") };
            var emitted = Emit(source, changes, target.Element.Range.ByteLength, token);
            if (emitted.Failure is not null) return new(null, emitted.Failure);
            var inspection = Inspect(emitted.Source!, token);
            if (!inspection.IsSuccess) return Refuse(AuthoringConditionalFailureKind.InvalidCandidate, inspection.Failure!.Message);
            var fresh = inspection.Snapshot!;
            var validation = Check(fresh, true, token);
            if (validation is not null) return new(null, wrap && validation.Kind == AuthoringConditionalFailureKind.InvalidCandidate
                ? Failure(AuthoringConditionalFailureKind.InvalidCondition, validation.Message) : validation);
            var proof = new ConditionalProof(snapshot, fresh, target, wrap, changes, token);
            if (!proof.Prove()) return Refuse(AuthoringConditionalFailureKind.UnsupportedCorrespondence, "Complete descriptor/attribute/reparenting/two-boundary merge proof failed.");
            var selection = wrap ? proof.CreatedNodes.Single() : proof.Map[target.Element];
            var test = wrap ? selection.Attributes.Single() : null;
            if (wrap)
            {
                if (test!.ExpandedValue != condition) return Refuse(AuthoringConditionalFailureKind.InvalidCondition, "The actual candidate slot does not retain the logical condition.");
                var compile = ExpressionEditPipeline.ValidateInStaticContext(fresh, condition!, test, selection, emitted.Source!.CoordinateMap, expectedFailuresOnly: true);
                if (compile is not null) return Refuse(AuthoringConditionalFailureKind.InvalidCondition, compile.Failure!.Message);
                token.ThrowIfCancellationRequested();
            }
            var candidate = new AuthoringConditionalCandidate(snapshot, fresh.WithSuccessfulCompilation(),
                wrap ? AuthoringConditionalOperation.Wrap : AuthoringConditionalOperation.Unwrap, selection, test, proof, emitted.Edits!);
            token.ThrowIfCancellationRequested();
            return new(candidate, null);
        }
        catch (ConditionalLimitException ex) { return Refuse(AuthoringConditionalFailureKind.SizeLimit, ex.Message); }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        { return Refuse(AuthoringConditionalFailureKind.Cancelled, "Cancelled without candidate authority."); }
    }

    internal static string EscapeCondition(string condition, Encoding encoding, char quote, CancellationToken token)
    {
        if (condition.Length > AuthoringCapabilities.MaxConditionalConditionUnits) throw new ConditionalLimitException("Condition UTF-16 units exceed their limit.");
        // Reuse the established expression escape policy, adding references for XML-normalized whitespace.
        // Pre-count without allocating output and validate Unicode before the shared helper is entered.
        long count = 0;
        for (var i = 0; i < condition.Length;)
        {
            token.ThrowIfCancellationRequested();
            var c = condition[i]; var units = char.IsHighSurrogate(c) && i + 1 < condition.Length && char.IsLowSurrogate(condition[i + 1]) ? 2 : 1;
            if (units == 1 && !XmlConvert.IsXmlChar(c)) throw new XmlException("Invalid XML scalar.");
            var escaped = c switch { '&' => "&amp;", '<' => "&lt;", '"' when quote == '"' => "&quot;", '\'' when quote == '\'' => "&apos;", '\t' => "&#x9;", '\r' => "&#xD;", '\n' => "&#xA;", _ => null };
            count = checked(count + (escaped is not null ? encoding.GetByteCount(escaped) : encoding.GetByteCount(condition.AsSpan(i, units))));
            if (count > AuthoringCapabilities.MaxConditionalEscapedConditionBytes) throw new ConditionalLimitException("Encoded escaped condition exceeds its independent limit.");
            i += units;
        }
        token.ThrowIfCancellationRequested();
        var output = new StringBuilder();
        for (var i = 0; i < condition.Length; i += Math.Min(2048, condition.Length - i))
        {
            token.ThrowIfCancellationRequested(); var units = Math.Min(2048, condition.Length - i);
            output.Append(ExpressionEditPipeline.EscapeAttributeValue(condition.Substring(i, units), quote)
                .Replace("\t", "&#x9;").Replace("\r", "&#xD;").Replace("\n", "&#xA;"));
        }
        token.ThrowIfCancellationRequested(); return output.ToString();
    }

    internal static (AuthoringSource? Source, IReadOnlyList<AuthoringInsertionSourceEdit>? Edits, AuthoringConditionalFailure? Failure) Emit(
        AuthoringSource source, IReadOnlyList<ConditionalChange> changes, long selectedBytes, CancellationToken token)
    {
        var ranges = changes.Select(c => source.CoordinateMap.RangeFromCharOffsets(c.Start, c.End)).ToArray();
        long length = source.OriginalBytes.Length;
        for (var i = 0; i < changes.Count; i++) length = checked(length - ranges[i].ByteLength + source.Encoding.GetByteCount(changes[i].Replacement));
        if (length > AuthoringCapabilities.MaxConditionalSourceBytes) return (null, null, Failure(AuthoringConditionalFailureKind.SizeLimit, "Independent candidate source limit exceeded."));
        // Count original/candidate retained copies, full exports, selected extents and complete edit payloads conservatively.
        long evidence = checked(2L * source.OriginalBytes.Length + 3L * length + 2L * selectedBytes);
        for (var i = 0; i < changes.Count; i++) evidence = checked(evidence + 2L * ranges[i].ByteLength + 2L * source.Encoding.GetByteCount(changes[i].Replacement));
        if (evidence > AuthoringCapabilities.MaxConditionalEvidenceBytes) return (null, null, Failure(AuthoringConditionalFailureKind.SizeLimit, "Cumulative complete copied evidence budget exceeded."));
        var bytes = new byte[checked((int)length)]; var read = 0; var written = 0;
        for (var i = 0; i < changes.Count; i++)
        {
            var a = checked((int)ranges[i].StartByteOffset); var b = checked(a + (int)ranges[i].ByteLength);
            Copy(source.OriginalBytes.Span[read..a], bytes.AsSpan(written), token); written += a - read;
            token.ThrowIfCancellationRequested();
            var replacement = source.Encoding.GetBytes(changes[i].Replacement);
            Copy(replacement, bytes.AsSpan(written), token); written += replacement.Length; read = b;
        }
        Copy(source.OriginalBytes.Span[read..], bytes.AsSpan(written), token);
        token.ThrowIfCancellationRequested();
        if (!AuthoringSource.TryCreate(bytes, source.BaseUri, out var fresh, out var sf))
            return (null, null, Failure(AuthoringConditionalFailureKind.InvalidCandidate, sf!.Message));
        var xml = LiteralElementLifecyclePipeline.CheckXml(fresh!, token);
        if (xml is not null) return (null, null, Convert(xml.Value.Kind.ToString(), xml.Value.Message));
        var edits = new List<AuthoringInsertionSourceEdit>(); var delta = 0;
        for (var i = 0; i < changes.Count; i++)
        {
            var c = changes[i]; var a = checked(c.Start + delta);
            edits.Add(new(ranges[i], fresh!.CoordinateMap.RangeFromCharOffsets(a, checked(a + c.Replacement.Length))));
            delta = checked(delta + c.Replacement.Length - (c.End - c.Start));
        }
        return (fresh, Array.AsReadOnly(edits.ToArray()), null);
    }

    internal static AuthoringInspectionResult Inspect(AuthoringSource source, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var result = XsltAuthoring.Inspect(source, new DenyResolver(), new() { AttemptCompilation = false, MaxModuleCount = 1 });
        token.ThrowIfCancellationRequested(); return result;
    }
    internal static int Offset(AuthoringSource source, SourceRange range, bool end) => source.CoordinateMap.CharOffsetFromLineColumn(
        end ? range.EndLine : range.StartLine, end ? range.EndColumn : range.StartColumn);
    internal static IEnumerable<AuthoringNodeDescriptor> Nodes(AuthoringNodeDescriptor root, CancellationToken token)
    {
        var stack = new Stack<AuthoringNodeDescriptor>(); stack.Push(root);
        while (stack.TryPop(out var node))
        { token.ThrowIfCancellationRequested(); yield return node; for (var i = node.Children.Count - 1; i >= 0; i--) stack.Push(node.Children[i]); }
    }
    internal static Dictionary<AuthoringNodeDescriptor, int> Depths(AuthoringNodeDescriptor root, CancellationToken token)
    {
        var result = new Dictionary<AuthoringNodeDescriptor, int>(); var stack = new Stack<(AuthoringNodeDescriptor Node, int Depth)>(); stack.Push((root, 0));
        while (stack.TryPop(out var item)) { token.ThrowIfCancellationRequested(); result.Add(item.Node, item.Depth); foreach (var child in item.Node.Children) stack.Push((child, item.Depth + 1)); }
        return result;
    }
    internal static void Copy(ReadOnlySpan<byte> source, Span<byte> destination, CancellationToken token)
    {
        for (var i = 0; i < source.Length; i += Math.Min(65536, source.Length - i))
        { token.ThrowIfCancellationRequested(); var n = Math.Min(65536, source.Length - i); source.Slice(i, n).CopyTo(destination.Slice(i, n)); }
    }
    private sealed class DenyResolver : IAuthoringModuleResolver
    {
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) => null;
    }
}
internal sealed class ConditionalLimitException(string message) : Exception(message);
internal sealed record ConditionalChange(int Start, int End, string Replacement);
