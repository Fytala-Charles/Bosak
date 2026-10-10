// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Validation, byte-splicing and re-inspection pipeline behind ProposeExpressionEdit.
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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using Bosak.XPath.Api;

namespace Bosak.Xslt.Authoring;

/// <summary>
/// The engine behind <see cref="AuthoringSnapshot.ProposeExpressionEdit"/>. Validates the proposal
/// against the input snapshot, compiles the new expression in the slot's real static context, escapes
/// and encodes it under the module's declared encoding, splices only the attribute-value byte span of
/// the retained original bytes, re-inspects the emitted bytes with the snapshot's own resolver and
/// options, and pairs the two descriptor trees into an old-to-new node identity correspondence. Every
/// refusal is classified data; only violated engine invariants throw.
/// </summary>
internal static class ExpressionEditPipeline
{
    public static AuthoringEditResult Run(AuthoringSnapshot snapshot, AuthoringEditProposal proposal)
    {
        // Resolve the owning node together with its module: ids are unique per snapshot, and the module
        // supplies the retained envelope and coordinate map the splice is computed from.
        AuthoringModuleDescriptor? module = null;
        AuthoringNodeDescriptor? node = null;
        foreach (var candidateModule in snapshot.Modules)
        {
            var found = candidateModule.Root.FindDescendant(n => n.Id == proposal.OwningNodeId);
            if (found is not null)
            {
                module = candidateModule;
                node = found;
                break;
            }
        }

        if (node is null || node.Kind != AuthoringNodeKind.Element)
        {
            return AuthoringEditResult.FailureResult(new AuthoringEditFailure(
                AuthoringEditFailureKind.UnknownNode,
                $"The node id {proposal.OwningNodeId} does not identify an element of this snapshot."));
        }

        var attribute = node.Attributes.FirstOrDefault(a =>
            string.Equals(a.Name, proposal.AttributeName, StringComparison.Ordinal));
        if (attribute is null)
        {
            return AuthoringEditResult.FailureResult(new AuthoringEditFailure(
                AuthoringEditFailureKind.SlotNotEditable,
                $"The element '{node.ElementName?.LexicalForm}' has no attribute '{proposal.AttributeName}'.",
                node.Range));
        }

        if (attribute.SlotKind != AuthoringAttributeSlotKind.Expression)
        {
            return AuthoringEditResult.FailureResult(new AuthoringEditFailure(
                AuthoringEditFailureKind.SlotNotEditable,
                $"The attribute '{attribute.Name}' is classified as {attribute.SlotKind} and is not an editable expression slot in this version.",
                attribute.ValueRange));
        }

        var source = snapshot.SourceFor(module!.ModuleUri);
        var map = source.CoordinateMap;
        var text = map.Text;

        var compileOutcome = ValidateInStaticContext(proposal.NewExpressionText, attribute, node, map);
        if (compileOutcome is not null)
        {
            return compileOutcome;
        }

        var valueStartChar = map.CharOffsetFromLineColumn(
            attribute.ValueRange.StartLine, attribute.ValueRange.StartColumn);
        var quote = text[valueStartChar - 1];
        var escaped = EscapeAttributeValue(proposal.NewExpressionText, quote);

        byte[] encoded;
        try
        {
            var strict = Encoding.GetEncoding(
                source.Encoding.CodePage,
                new EncoderExceptionFallback(),
                DecoderFallback.ReplacementFallback);
            encoded = strict.GetBytes(escaped);
        }
        catch (EncoderFallbackException)
        {
            return AuthoringEditResult.FailureResult(new AuthoringEditFailure(
                AuthoringEditFailureKind.NotRepresentableInEncoding,
                $"The escaped edit cannot be encoded in the module's declared encoding ({source.Encoding.EncodingName}, {source.Encoding.WebName}). " +
                "Declare a broader encoding (for example UTF-8) to allow this value.",
                attribute.ValueRange));
        }

        var original = source.OriginalBytes.Span;
        var startByte = checked((int)attribute.ValueRange.StartByteOffset);
        var oldByteLength = checked((int)attribute.ValueRange.ByteLength);
        var spliced = new byte[original.Length - oldByteLength + encoded.Length];
        original.Slice(0, startByte).CopyTo(spliced);
        encoded.CopyTo(spliced.AsSpan(startByte));
        original.Slice(startByte + oldByteLength).CopyTo(spliced.AsSpan(startByte + encoded.Length));

        // The splice is a single-attribute-value replacement of well-formed source by construction;
        // failure here would mean the engine's own invariant broke, which must not surface as data.
        if (!AuthoringSource.TryCreate(spliced, module.ModuleUri, out var emittedSource, out var createFailure))
        {
            throw new InvalidOperationException(
                $"Re-inspection envelope invariant violated for '{module.ModuleUri}': {createFailure}");
        }

        var reinspection = new AuthoringInspector().Inspect(
            emittedSource!,
            snapshot.ModuleResolver,
            snapshot.InspectionOptions);
        if (!reinspection.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Re-inspection invariant violated for '{module.ModuleUri}': {reinspection.Failure}");
        }

        var candidateSnapshot = reinspection.Snapshot!;
        var correspondence = BuildCorrespondence(snapshot, candidateSnapshot);
        if (!correspondence.TryGetValue(proposal.OwningNodeId, out var newNodeId))
        {
            throw new InvalidOperationException(
                "Re-inspection invariant violated: the edited node has no counterpart in the candidate snapshot.");
        }

        var newNode = candidateSnapshot.FindNodeById(newNodeId)!;
        var newAttribute = newNode.Attributes.First(a =>
            string.Equals(a.Name, proposal.AttributeName, StringComparison.Ordinal));
        var changedSlot = new AuthoringChangedSlot(
            proposal.OwningNodeId,
            proposal.AttributeName,
            attribute.ValueRange,
            newAttribute.ValueRange,
            attribute.RawLiteral,
            newAttribute.RawLiteral);

        return AuthoringEditResult.SuccessResult(new AuthoringEditCandidate(
            spliced,
            new[] { attribute.ValueRange },
            candidateSnapshot,
            correspondence,
            changedSlot));
    }

    private static AuthoringEditResult? ValidateInStaticContext(
        string newExpressionText,
        AuthoringAttributeDescriptor attribute,
        AuthoringNodeDescriptor node,
        SourceCoordinateMap map)
    {
        var context = attribute.SlotContext!;
        var namespaces = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var binding in context.InScopeNamespaces)
        {
            // The default (prefixless) declaration is the XPath default element namespace, not a prefix
            // binding; the nearest declaration wins, which the outermost-first ordering makes "last".
            if (binding.Prefix.Length != 0)
            {
                namespaces[binding.Prefix] = binding.Uri;
            }
        }

        var options = new CompileOptions
        {
            Namespaces = namespaces,
            DefaultElementNamespace = context.XpathDefaultNamespace,
            BaseUri = context.BaseUri.AbsoluteUri,
            Compatibility = context.EffectiveVersion.StartsWith("4", StringComparison.Ordinal)
                ? XPathCompatibility.XPath40
                : XPathCompatibility.XPath31,
        };

        try
        {
            _ = XPath31Expression.Compile(newExpressionText, options);
            return null;
        }
        catch (Exception ex) when (IsUndeclaredPrefixError(ex))
        {
            return AuthoringEditResult.FailureResult(new AuthoringEditFailure(
                AuthoringEditFailureKind.RequiresParentChange,
                $"The expression uses a namespace prefix that is not declared in scope at the slot: {ex.Message} " +
                "Declaring it would change the owning element's start tag, which is outside this edit's ownership.",
                attribute.ValueRange,
                StartTagRange(map, node)));
        }
        catch (Exception ex)
        {
            return AuthoringEditResult.FailureResult(new AuthoringEditFailure(
                AuthoringEditFailureKind.ExpressionParseError,
                $"The new expression does not compile in the slot's static context: {ex.Message}",
                attribute.ValueRange));
        }
    }

    private static bool IsUndeclaredPrefixError(Exception ex) =>
        ex.Message.Contains("XPST0081", StringComparison.Ordinal);

    private static SourceRange StartTagRange(SourceCoordinateMap map, AuthoringNodeDescriptor node)
    {
        var elementStart = map.CharOffsetFromLineColumn(node.Range.StartLine, node.Range.StartColumn);
        SourceTagScanner.FindElementEnd(map.Text, elementStart, out var startTagEnd, out _);
        return map.RangeFromCharOffsets(elementStart, startTagEnd);
    }

    private static string EscapeAttributeValue(string value, char quote)
    {
        var escaped = value.Replace("&", "&amp;").Replace("<", "&lt;");
        return quote == '"'
            ? escaped.Replace("\"", "&quot;")
            : escaped.Replace("'", "&apos;");
    }

    private static Dictionary<int, int> BuildCorrespondence(
        AuthoringSnapshot input,
        AuthoringSnapshot candidate)
    {
        // Pairwise-ordinal paired walk of both forests, module index by module index. A pair matches
        // when kinds are equal and, for elements, the resolved qualified name is equal; children pair
        // by ordinal. Unpaired nodes are omitted; an id is never remapped to a different node.
        var correspondence = new Dictionary<int, int>();
        var moduleCount = Math.Min(input.Modules.Count, candidate.Modules.Count);
        for (var i = 0; i < moduleCount; i++)
        {
            Pair(input.Modules[i].Root, candidate.Modules[i].Root, correspondence);
        }

        return correspondence;
    }

    private static void Pair(
        AuthoringNodeDescriptor input,
        AuthoringNodeDescriptor candidate,
        Dictionary<int, int> correspondence)
    {
        if (input.Kind != candidate.Kind)
        {
            return;
        }

        if (input.Kind == AuthoringNodeKind.Element &&
            (input.ElementName is null ||
             candidate.ElementName is null ||
             input.ElementName.NamespaceUri != candidate.ElementName.NamespaceUri ||
             input.ElementName.LocalName != candidate.ElementName.LocalName))
        {
            return;
        }

        correspondence[input.Id] = candidate.Id;

        var childCount = Math.Min(input.Children.Count, candidate.Children.Count);
        for (var i = 0; i < childCount; i++)
        {
            Pair(input.Children[i], candidate.Children[i], correspondence);
        }
    }
}
