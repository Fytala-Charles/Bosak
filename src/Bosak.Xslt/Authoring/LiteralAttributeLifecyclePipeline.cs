// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 October 2026
// PURPOSE              : Prove and emit exact literal attribute lifecycle edits and complete correspondence.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Buffers;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Bosak.Xslt.Authoring;

internal static class LiteralAttributeLifecyclePipeline
{
    private const int Limit = 8_388_608;
    private static readonly XNamespace Xsl = "http://www.w3.org/1999/XSL/Transform";
    private static AuthoringLiteralAttributeFailure Fail(AuthoringSnapshot s, AuthoringLiteralAttributeFailureKind kind, string message, string? code = null) =>
        new(kind, message, s.PrincipalModule.ModuleUri, code);

    internal static (LiteralAttributeTargetData? Data, AuthoringLiteralAttributeFailure? Failure) Discover(
        AuthoringSnapshot snapshot, AuthoringNodeDescriptor owner, AuthoringAttributeDescriptor? attribute, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            if (!ReferenceEquals(snapshot.FindNodeById(owner.Id), owner) || attribute is not null && !owner.Attributes.Contains(attribute))
                return (null, Fail(snapshot, AuthoringLiteralAttributeFailureKind.ForeignSnapshot, "Actual owner and attribute references from this snapshot are required."));
            if (owner.Kind != AuthoringNodeKind.Element || owner.ElementName?.NamespaceUri != "")
                return (null, Fail(snapshot, AuthoringLiteralAttributeFailureKind.IneligibleOwner, "A no-namespace literal result element is required."));
            if (attribute is not null && (attribute.NamespaceUri.Length != 0 || attribute.Prefix.Length != 0 || attribute.Name == "xmlns"))
                return (null, Fail(snapshot, AuthoringLiteralAttributeFailureKind.IneligibleAttribute, "Only unqualified ordinary result attributes are supported."));
            var failure = Scope(snapshot, owner, token, out var document);
            if (failure is not null) return (null, failure);
            var source = snapshot.SourceFor(snapshot.PrincipalModule.ModuleUri);
            var start = Offset(source, owner.Range, false);
            var end = SourceTagScanner.FindElementEnd(source.Text, start, out var tagEnd, out var self);
            if (source.CoordinateMap.RangeFromCharOffsets(start, end) != owner.Range)
                return (null, Fail(snapshot, AuthoringLiteralAttributeFailureKind.SourceAlignmentFailure, "The retained owner extent does not align."));
            var element = document!.Root!.DescendantsAndSelf().SingleOrDefault(e => ElementOffset(source, e) == start);
            if (element is null || element.Name.NamespaceName != owner.ElementName.NamespaceUri || element.Name.LocalName != owner.ElementName.LocalName)
                return (null, Fail(snapshot, AuthoringLiteralAttributeFailureKind.SourceAlignmentFailure, "The actual owner does not align with the retained document."));
            var ancestor = element.Parent;
            while (ancestor is not null && ancestor.Name.Namespace != Xsl) ancestor = ancestor.Parent;
            if (ancestor?.Name != Xsl + "template" || ancestor.Parent != document.Root)
                return (null, Fail(snapshot, AuthoringLiteralAttributeFailureKind.IneligibleOwner, "A principal top-level template and literal-result ancestor chain is required."));
            // Align every actual attribute to retained scanner and independently parsed metadata.
            var scans = SourceTagScanner.ScanAttributes(source.Text, start, tagEnd);
            var actual = element.Attributes().ToArray();
            if (scans.Count != owner.Attributes.Count || actual.Length != scans.Count)
                return (null, Fail(snapshot, AuthoringLiteralAttributeFailureKind.SourceAlignmentFailure, "The complete attribute collection does not align."));
            string? logical = null; char quote = '"';
            for (var i = 0; i < scans.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                var scan = scans[i]; var a = owner.Attributes[i]; var dom = actual[i];
                var rawName = source.Text[scan.NameStart..scan.NameEnd];
                // Namespace aliases may differ; descriptor lexical spelling is checked against the scanner,
                // while expanded identity is checked against the independently parsed attribute.
                if (a.Name != rawName || a.LocalName != dom.Name.LocalName || a.NamespaceUri != dom.Name.NamespaceName
                    || a.Prefix != (rawName.Contains(':') ? rawName[..rawName.IndexOf(':')] : "")
                    || a.SlotKind != AttributeSlotClassifier.Classify(element.Name.NamespaceName, element.Name.LocalName, a.LocalName, a.NamespaceUri, a.RawLiteral)
                    || a.RawLiteral != source.Text[scan.ValueStart..scan.ValueEnd] || a.ExpandedValue != dom.Value
                    || a.FullRange != source.CoordinateMap.RangeFromCharOffsets(scan.NameStart, scan.FullEnd)
                    || a.ValueRange != source.CoordinateMap.RangeFromCharOffsets(scan.ValueStart, scan.ValueEnd))
                    return (null, Fail(snapshot, AuthoringLiteralAttributeFailureKind.SourceAlignmentFailure, "Attribute metadata does not match retained syntax."));
                if (ReferenceEquals(a, attribute)) { logical = Constant(dom.Value, token, out _); quote = scan.Quote; }
            }
            if (attribute is not null && logical is null)
                return (null, Fail(snapshot, AuthoringLiteralAttributeFailureKind.UnsupportedCoverage, "The selected attribute is not proven constant."));
            failure = Validate(snapshot, false, token);
            token.ThrowIfCancellationRequested();
            var nameEnd = start + 1 + owner.ElementName.Prefix.Length + (owner.ElementName.Prefix.Length == 0 ? 0 : 1) + owner.ElementName.LocalName.Length;
            return failure is null ? (new(snapshot, owner, attribute, source.CoordinateMap.RangeFromCharOffsets(start, tagEnd), self, quote, logical, nameEnd), null) : (null, failure);
        }
        catch (OperationCanceledException) { return (null, Fail(snapshot, AuthoringLiteralAttributeFailureKind.Cancelled, "Cancelled without authority.")); }
    }

    internal static AuthoringLiteralAttributeCreationResult Creation(AuthoringSnapshot s, AuthoringLiteralAttributeCreationProposal p, CancellationToken t)
    { var r = Run(s, p.Target.Data, AuthoringLiteralAttributeOperation.Create, p.LocalName, p.Value, t); return new(r.Candidate, r.Failure); }
    internal static AuthoringLiteralAttributeDeletionResult Deletion(AuthoringSnapshot s, AuthoringLiteralAttributeDeletionProposal p, CancellationToken t)
    { var r = Run(s, p.Target.Data, AuthoringLiteralAttributeOperation.Delete, null, null, t); return new(r.Candidate, r.Failure); }
    internal static AuthoringLiteralAttributeUpdateResult Update(AuthoringSnapshot s, AuthoringLiteralAttributeUpdateProposal p, CancellationToken t)
    { var r = Run(s, p.Target.Data, AuthoringLiteralAttributeOperation.Update, null, p.Value, t); return new(r.Candidate, r.NoChange ? new(p.Target) : null, r.Failure); }

    private static (AuthoringLiteralAttributeCandidate? Candidate, bool NoChange, AuthoringLiteralAttributeFailure? Failure) Run(
        AuthoringSnapshot snapshot, LiteralAttributeTargetData target, AuthoringLiteralAttributeOperation operation, string? name, string? value, CancellationToken token)
    {
        (AuthoringLiteralAttributeCandidate?, bool, AuthoringLiteralAttributeFailure?) Refuse(AuthoringLiteralAttributeFailureKind kind, string message) => (null, false, Fail(snapshot, kind, message));
        try
        {
            token.ThrowIfCancellationRequested();
            if (!ReferenceEquals(target.Snapshot, snapshot)) return Refuse(AuthoringLiteralAttributeFailureKind.ForeignSnapshot, "The operation target belongs to another snapshot.");
            var discovery = Discover(snapshot, target.Owner, target.Attribute, token);
            if (discovery.Failure is not null) return (null, false, discovery.Failure);
            target = discovery.Data!;
            var source = snapshot.SourceFor(snapshot.PrincipalModule.ModuleUri);
            if (operation == AuthoringLiteralAttributeOperation.Create)
            {
                if (name!.Length > Limit) return Refuse(AuthoringLiteralAttributeFailureKind.SizeLimit, "The attribute name alone exceeds the source limit.");
                try { if (name.Length == 0 || name == "xmlns") throw new XmlException(); XmlConvert.VerifyNCName(name); }
                catch (XmlException) { return Refuse(AuthoringLiteralAttributeFailureKind.InvalidName, "A nonreserved XML NCName is required."); }
                if (target.Owner.Attributes.Any(a => a.NamespaceUri.Length == 0 && a.LocalName == name))
                    return Refuse(AuthoringLiteralAttributeFailureKind.DuplicateAttribute, "Create requires an absent attribute name.");
            }
            else name = target.Attribute!.LocalName;
            if (value is not null)
            {
                for (var i = 0; i < value.Length;)
                {
                    token.ThrowIfCancellationRequested();
                    if (Rune.DecodeFromUtf16(value.AsSpan(i), out var rune, out var used) != OperationStatus.Done
                        || rune.IsBmp && !XmlConvert.IsXmlChar((char)rune.Value))
                        return Refuse(AuthoringLiteralAttributeFailureKind.InvalidValue, "The logical value contains an illegal XML scalar.");
                    i += used;
                }
            }
            if (operation == AuthoringLiteralAttributeOperation.Update && string.Equals(value, target.LogicalValue, StringComparison.Ordinal))
            { token.ThrowIfCancellationRequested(); return (null, true, null); }
            var aChar = operation == AuthoringLiteralAttributeOperation.Create ? target.NameEnd : Offset(source, operation == AuthoringLiteralAttributeOperation.Update ? target.Attribute!.ValueRange : target.Attribute!.FullRange, false);
            var bChar = operation == AuthoringLiteralAttributeOperation.Create ? aChar : Offset(source, operation == AuthoringLiteralAttributeOperation.Update ? target.Attribute!.ValueRange : target.Attribute!.FullRange, true);
            var originalRange = source.CoordinateMap.RangeFromCharOffsets(aChar, bChar);
            var prefix = operation == AuthoringLiteralAttributeOperation.Create ? " " + name + "=\"" : "";
            var suffix = operation == AuthoringLiteralAttributeOperation.Create ? "\"" : "";
            long byteCount = source.Encoding.GetByteCount(prefix) + source.Encoding.GetByteCount(suffix);
            long charCount = prefix.Length + suffix.Length;
            if (source.OriginalBytes.Length - originalRange.ByteLength + byteCount > Limit)
                return Refuse(AuthoringLiteralAttributeFailureKind.SizeLimit, "The actual candidate exceeds the physical byte limit.");
            if (value is not null)
                for (var i = 0; i < value.Length;)
                {
                    token.ThrowIfCancellationRequested();
                    Rune.DecodeFromUtf16(value.AsSpan(i), out var rune, out var used);
                    var escaped = Escape(rune, target.Quote);
                    charCount = checked(charCount + (escaped?.Length ?? used));
                    byteCount = checked(byteCount + (escaped is not null ? source.Encoding.GetByteCount(escaped) : source.Encoding.CodePage == 65001 ? rune.Utf8SequenceLength : used * 2));
                    if (source.OriginalBytes.Length - originalRange.ByteLength + byteCount > Limit)
                        return Refuse(AuthoringLiteralAttributeFailureKind.SizeLimit, "Escaped candidate growth exceeds the physical byte limit.");
                    i += used;
                }
            var builder = new StringBuilder(checked((int)charCount)); builder.Append(prefix);
            if (value is not null)
                for (var i = 0; i < value.Length;)
                {
                    token.ThrowIfCancellationRequested(); Rune.DecodeFromUtf16(value.AsSpan(i), out var rune, out var used);
                    var escaped = Escape(rune, target.Quote);
                    if (escaped is null) builder.Append(value.AsSpan(i, used)); else builder.Append(escaped);
                    i += used;
                }
            builder.Append(suffix);
            var replacement = builder.ToString(); var encoded = source.Encoding.GetBytes(replacement);
            var bytes = new byte[checked(source.OriginalBytes.Length - (int)originalRange.ByteLength + encoded.Length)];
            var aByte = (int)originalRange.StartByteOffset; var bByte = aByte + (int)originalRange.ByteLength;
            Copy(source.OriginalBytes.Span[..aByte], bytes.AsSpan(), token);
            Copy(encoded, bytes.AsSpan(aByte), token);
            Copy(source.OriginalBytes.Span[bByte..], bytes.AsSpan(aByte + encoded.Length), token);
            if (!AuthoringSource.TryCreate(bytes, target.Snapshot.PrincipalModule.ModuleUri, out var freshSource, out var sourceFailure))
                return Refuse(AuthoringLiteralAttributeFailureKind.InvalidCandidate, sourceFailure!.Message);
            token.ThrowIfCancellationRequested();
            var inspected = XsltAuthoring.Inspect(freshSource!, new DenyResolver(), new() { AttemptCompilation = false, MaxModuleCount = 1 });
            token.ThrowIfCancellationRequested();
            if (!inspected.IsSuccess) return Refuse(AuthoringLiteralAttributeFailureKind.InvalidCandidate, inspected.Failure!.Message);
            var candidate = inspected.Snapshot!;
            var failure = Scope(candidate, null, token, out _);
            failure ??= Validate(candidate, true, token);
            if (failure is not null) return (null, false, failure);
            var proof = Prove(snapshot, candidate, target, operation, name!, value, aChar, bChar, replacement.Length, token);
            if (proof is null) return Refuse(AuthoringLiteralAttributeFailureKind.UnsupportedCorrespondence, "Complete graph and attribute correspondence could not be proven.");
            token.ThrowIfCancellationRequested();
            return (proof, false, null);
        }
        catch (OperationCanceledException) { return Refuse(AuthoringLiteralAttributeFailureKind.Cancelled, "Cancelled without candidate authority."); }
    }

    internal static AuthoringLiteralAttributeCandidate? Prove(AuthoringSnapshot original, AuthoringSnapshot candidate,
        LiteralAttributeTargetData target, AuthoringLiteralAttributeOperation operation, string name, string? value, int a, int b, int replacementChars, CancellationToken token)
    {
        var oldSource = original.SourceFor(original.PrincipalModule.ModuleUri); var newSource = candidate.SourceFor(candidate.PrincipalModule.ModuleUri);
        foreach (var graph in new[] { original, candidate })
        {
            var identities = new HashSet<int>();
            foreach (var node in ValueOfInsertionPipeline.Nodes(graph.PrincipalModule.Root))
            { token.ThrowIfCancellationRequested(); if (!identities.Add(node.Id)) return null; }
        }
        var delta = replacementChars - (b - a);
        var map = new Dictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor>();
        var used = new HashSet<AuthoringNodeDescriptor>();
        var attributes = new List<AuthoringLiteralAttributeCorrespondence>();
        AuthoringAttributeDescriptor? addressed = null;
        SourceRange? Translate(SourceRange range)
        {
            try
            {
                var x = Offset(oldSource, range, false); var y = Offset(oldSource, range, true);
                if (oldSource.CoordinateMap.RangeFromCharOffsets(x, y) != range) return null;
                // Containers enclosing the edit retain left boundary; ordinary spans after it move right.
                if (x < a && y >= b) y += delta;
                else if (x >= b) { x += delta; y += delta; }
                else if (y > a) return null;
                return newSource.CoordinateMap.RangeFromCharOffsets(x, y);
            }
            catch (ArgumentOutOfRangeException) { return null; }
            catch (InvalidDataException) { return null; }
        }
        bool Match(AuthoringNodeDescriptor old, AuthoringNodeDescriptor fresh)
        {
            token.ThrowIfCancellationRequested();
            if (!used.Add(fresh) || !map.TryAdd(old, fresh) || old.Kind != fresh.Kind || old.ElementName != fresh.ElementName
                || old.Text != fresh.Text || Translate(old.Range) != fresh.Range || old.Children.Count != fresh.Children.Count) return false;
            var oa = old.Attributes.Where(x => !ReferenceEquals(old, target.Owner) || !ReferenceEquals(x, target.Attribute)).ToArray();
            var fa = fresh.Attributes.ToList();
            if (ReferenceEquals(old, target.Owner) && operation != AuthoringLiteralAttributeOperation.Delete)
            {
                var hits = fa.Where(x => x.NamespaceUri.Length == 0 && x.LocalName == name && x.Prefix.Length == 0).ToArray();
                if (hits.Length != 1) return false;
                addressed = hits[0]; fa.Remove(addressed);
                if (Constant(addressed.ExpandedValue, token, out _) != value) return false;
                var originalStart = operation == AuthoringLiteralAttributeOperation.Create ? a + 1 : Offset(oldSource, target.Attribute!.FullRange, false);
                var tokenEnd = operation == AuthoringLiteralAttributeOperation.Create ? a + replacementChars : Offset(oldSource, target.Attribute!.FullRange, true) + delta;
                if (addressed.FullRange != newSource.CoordinateMap.RangeFromCharOffsets(originalStart, tokenEnd)) return false;
                if (operation == AuthoringLiteralAttributeOperation.Update)
                {
                    if (old.Attributes.ToList().IndexOf(target.Attribute!) != fresh.Attributes.ToList().IndexOf(addressed)) return false;
                    if ((target.Attribute!.Name, target.Attribute.Prefix, target.Attribute.NamespaceUri, target.Attribute.LocalName)
                        != (addressed.Name, addressed.Prefix, addressed.NamespaceUri, addressed.LocalName)) return false;
                    if (addressed.ValueRange != newSource.CoordinateMap.RangeFromCharOffsets(a, a + replacementChars)) return false;
                }
                else if (!ReferenceEquals(fresh.Attributes[0], addressed)) return false;
                var valueStart = Offset(newSource, addressed.ValueRange, false);
                var valueEnd = Offset(newSource, addressed.ValueRange, true);
                if (addressed.RawLiteral != newSource.Text[valueStart..valueEnd]) return false;
                var actualToken = newSource.Text[originalStart..tokenEnd];
                var parsed = XElement.Parse("<r " + actualToken + "/>").Attribute(name);
                if (parsed is null || parsed.Value != addressed.ExpandedValue) return false;
                if (addressed.SlotKind != AttributeSlotClassifier.Classify(fresh.ElementName!.NamespaceUri, fresh.ElementName.LocalName,
                    addressed.LocalName, addressed.NamespaceUri, addressed.RawLiteral)) return false;
            }
            if (oa.Length != fa.Count) return false;
            var survivors = new Dictionary<AuthoringAttributeDescriptor, AuthoringAttributeDescriptor>();
            for (var i = 0; i < oa.Length; i++)
            {
                token.ThrowIfCancellationRequested(); var x = oa[i]; var y = fa[i];
                if ((x.Name, x.Prefix, x.NamespaceUri, x.LocalName, x.SlotKind, x.RawLiteral, x.ExpandedValue)
                    != (y.Name, y.Prefix, y.NamespaceUri, y.LocalName, y.SlotKind, y.RawLiteral, y.ExpandedValue)
                    || Translate(x.FullRange) != y.FullRange || Translate(x.ValueRange) != y.ValueRange || !survivors.TryAdd(x, y)) return false;
            }
            if (old.Kind == AuthoringNodeKind.Element) attributes.Add(new(old, fresh, survivors));
            for (var i = 0; i < old.Children.Count; i++) if (!Match(old.Children[i], fresh.Children[i])) return false;
            return true;
        }
        if (!Match(original.PrincipalModule.Root, candidate.PrincipalModule.Root)) return null;
        var owner = map[target.Owner];
        var start = Offset(newSource, owner.Range, false);
        SourceTagScanner.FindElementEnd(newSource.Text, start, out var openingEnd, out var self);
        if (self != target.SelfClosing) return null;
        var oldRange = oldSource.CoordinateMap.RangeFromCharOffsets(a, b);
        var newRange = newSource.CoordinateMap.RangeFromCharOffsets(a, a + replacementChars);
        if (!oldSource.OriginalBytes.Span[..(int)oldRange.StartByteOffset].SequenceEqual(newSource.OriginalBytes.Span[..(int)newRange.StartByteOffset])
            || !oldSource.OriginalBytes.Span[(int)(oldRange.StartByteOffset + oldRange.ByteLength)..].SequenceEqual(newSource.OriginalBytes.Span[(int)(newRange.StartByteOffset + newRange.ByteLength)..])) return null;
        return new(original, candidate.WithSuccessfulCompilation(), operation, target.Owner, owner, target.OpeningTag,
            newSource.CoordinateMap.RangeFromCharOffsets(start, openingEnd), new AuthoringQName("", "", name), target.Attribute, addressed,
            target.LogicalValue, value, new ReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor>(map),
            Array.AsReadOnly(attributes.ToArray()), Array.AsReadOnly(new[] { new AuthoringInsertionSourceEdit(oldRange, newRange) }));
    }

    private static AuthoringLiteralAttributeFailure? Scope(AuthoringSnapshot snapshot, AuthoringNodeDescriptor? owner, CancellationToken token, out XDocument? document)
    {
        document = null;
        AuthoringLiteralAttributeFailure Refuse(AuthoringLiteralAttributeFailureKind kind, string message) => Fail(snapshot, kind, message);
        if (snapshot.Modules.Count != 1 || snapshot.PrincipalModule.Edges.Count != 0)
            return Refuse(AuthoringLiteralAttributeFailureKind.UnsupportedModule, "Only the dependency-free principal module is supported.");
        var source = snapshot.SourceFor(snapshot.PrincipalModule.ModuleUri);
        if (source.OriginalBytes.Length > Limit) return Refuse(AuthoringLiteralAttributeFailureKind.SizeLimit, "Original physical bytes exceed the independent limit.");
        using var reader = XmlReader.Create(new StringReader(source.Text), new() { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }, source.BaseUri.AbsoluteUri);
        try { document = XDocument.Load(reader, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo); }
        catch (XmlException ex) { return Refuse(AuthoringLiteralAttributeFailureKind.InvalidOriginal, ex.Message); }
        var root = document.Root;
        if (root is null || root.Name.Namespace != Xsl || root.Name.LocalName is not ("stylesheet" or "transform") || (string?)root.Attribute("version") is not ("3.0" or "4.0"))
            return Refuse(AuthoringLiteralAttributeFailureKind.UnsupportedProfile, "An explicit 3.0 or 4.0 stylesheet/transform is required.");
        var cp = source.Encoding.CodePage; var enc = document.Declaration?.Encoding;
        if (document.Declaration?.Version is not (null or "1.0") || cp is not (65001 or 1200 or 1201)
            || cp != 65001 && source.ByteOffsetBias != 2
            || enc is not null && !(cp == 65001 ? enc.Equals("utf-8", StringComparison.OrdinalIgnoreCase)
                : enc.Equals("utf-16", StringComparison.OrdinalIgnoreCase) || enc.Equals(cp == 1200 ? "utf-16le" : "utf-16be", StringComparison.OrdinalIgnoreCase)))
            return Refuse(AuthoringLiteralAttributeFailureKind.UnsupportedEncoding, "Strict UTF-8 or BOM-marked matching UTF-16 is required.");
        foreach (var e in root.DescendantsAndSelf())
        {
            token.ThrowIfCancellationRequested();
            if (e.Name.Namespace == Xsl && e.Name.LocalName is "include" or "import" or "use-package" or "package" or "import-schema")
                return Refuse(AuthoringLiteralAttributeFailureKind.UnsupportedModule, "Dependencies and schema/package contexts are not acquired.");
            if (e.Nodes().Any(n => n is XCData)) return Refuse(AuthoringLiteralAttributeFailureKind.UnsupportedCoverage, "CDATA coverage is not declared for this operation.");
            foreach (var attr in e.Attributes())
            {
                token.ThrowIfCancellationRequested();
                if (attr.IsNamespaceDeclaration) continue;
                var control = attr.Name.Namespace == Xsl || e.Name.Namespace == Xsl && attr.Name.NamespaceName.Length == 0;
                if (control && (attr.Name.LocalName is "fixed-namespaces" or "type-namespaces" or "extension-element-prefixes" or "use-when" or "as" or "type" or "validation" or "mode" or "default-mode" or "use-attribute-sets"
                    || attr.Name.LocalName.StartsWith('_') || attr.Name.LocalName == "static" && attr.Value is not ("no" or "false" or "0")))
                    return Refuse(AuthoringLiteralAttributeFailureKind.UnsupportedCoverage, "Static preprocessing and component/type/extension contexts are outside coverage.");
                if (control && attr.Name.LocalName == "version" && attr.Value is not ("3.0" or "4.0"))
                    return Refuse(AuthoringLiteralAttributeFailureKind.UnsupportedProfile, "Effective versions must be 3.0 or 4.0.");
                if (control && attr.Name.LocalName == "expand-text" && attr.Value is not ("no" or "false" or "0"))
                    return Refuse(AuthoringLiteralAttributeFailureKind.UnsupportedCoverage, "Expanded text is outside this profile.");
                if (e.Name.Namespace != Xsl && !control)
                {
                    if (attr.Name.Namespace == XNamespace.Xml && attr.Name.LocalName == "space") continue;
                    if (attr.Name.NamespaceName.Length != 0)
                        return Refuse(AuthoringLiteralAttributeFailureKind.UnsupportedCoverage, "Qualified result attributes are outside coverage.");
                    if (Constant(attr.Value, token, out var invalid) is null)
                        return Refuse(invalid ? AuthoringLiteralAttributeFailureKind.InvalidOriginal : AuthoringLiteralAttributeFailureKind.UnsupportedCoverage,
                            "Every result attribute must be a valid, proven constant AVT after XML expansion.");
                }
            }
        }
        return null;
    }

    // A complete recognizer for the literal-only subset of AVTs, on XML-expanded text.
    // Any single opening brace denotes an expression and refuses; unmatched closing braces are invalid.
    private static string? Constant(string text, CancellationToken token, out bool invalid)
    {
        invalid = false; var result = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            token.ThrowIfCancellationRequested(); var c = text[i];
            if (c is '{' or '}')
            {
                if (i + 1 >= text.Length || text[i + 1] != c) { invalid = c == '}'; return null; }
                i++;
            }
            result.Append(c);
        }
        return result.ToString();
    }
    private static string? Escape(Rune r, char quote) => r.Value switch
    { '&' => "&amp;", '<' => "&lt;", '{' => "{{", '}' => "}}", '\t' => "&#x9;", '\r' => "&#xD;", '\n' => "&#xA;", '"' when quote == '"' => "&quot;", '\'' when quote == '\'' => "&apos;", _ => null };
    private static int Offset(AuthoringSource s, SourceRange r, bool end) => s.CoordinateMap.CharOffsetFromLineColumn(end ? r.EndLine : r.StartLine, end ? r.EndColumn : r.StartColumn);
    private static int ElementOffset(AuthoringSource s, XElement e)
    { var info = (IXmlLineInfo)e; var p = s.CoordinateMap.CharOffsetFromLineColumn(info.LineNumber, info.LinePosition); return p > 0 && s.Text[p - 1] == '<' ? p - 1 : p; }
    private static void Copy(ReadOnlySpan<byte> source, Span<byte> destination, CancellationToken token)
    { for (var i = 0; i < source.Length; i += Math.Min(65536, source.Length - i)) { token.ThrowIfCancellationRequested(); var n = Math.Min(65536, source.Length - i); source.Slice(i, n).CopyTo(destination.Slice(i, n)); } }
    private static AuthoringLiteralAttributeFailure? Validate(AuthoringSnapshot snapshot, bool candidate, CancellationToken token)
    {
        var f = ValueOfInsertionPipeline.Validate(snapshot, candidate, token, refusalFilter: ExpectedFailure);
        return f is null ? null : Fail(snapshot, f.Kind switch
        { AuthoringInsertionFailureKind.UnsupportedCoverage => AuthoringLiteralAttributeFailureKind.UnsupportedCoverage,
          AuthoringInsertionFailureKind.ResourceDenied => AuthoringLiteralAttributeFailureKind.ResourceDenied,
          _ => candidate ? AuthoringLiteralAttributeFailureKind.InvalidCandidate : AuthoringLiteralAttributeFailureKind.InvalidOriginal }, f.Message, f.ErrorCode);
    }
    private static bool ExpectedFailure(Exception ex) => ex is XmlException or Bosak.XPath.Parser.XPathParseException or AuthoringFunctionSignatureCoverageException
        || ex is InvalidOperationException && Regex.IsMatch(ex.Message, @"\b(?:XT|XP|XQ|FO)[A-Z]{2}[0-9]{4}\b", RegexOptions.CultureInvariant);
    private sealed class DenyResolver : IAuthoringModuleResolver
    {
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) => null;
    }
}
