// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 september 2026
// PURPOSE              : Validates and annotates in-memory subtrees with schema PSVI for typed-value support
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-09-2026     | Creation (REQ-098 seam H3)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using System.Xml.Schema;

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Provides schema annotation services for in-memory subtrees. Validation attaches PSVI
/// (<see cref="IXmlSchemaInfo"/>) annotations to the live <see cref="XObject"/>s, so every
/// typed-value surface in the engine (<c>TypedValue</c>, <c>SchemaTypeAnnotation</c>,
/// <c>instance of</c> against schema types, ...) works on the annotated nodes immediately,
/// without a serialize/parse round-trip and without changing node identity.
/// </summary>
/// <remarks>
/// This generalizes <see cref="XDocumentProvider.ValidateXDocument"/> from whole documents
/// to arbitrary subtrees, and pairs with the <c>ConstructedElementProcessor</c> /
/// <c>ConstructedDocumentProcessor</c> hooks on the runtime <c>EvaluationContext</c>, which
/// allow a host to annotate XSLT-constructed nodes as they are built.
/// </remarks>
public static class XdmSchemaAnnotator
{
    /// <summary>
    /// Validates the supplied subtree in place against the compiled schema set and attaches
    /// PSVI annotations to every node in it. The subtree is wrapped in a temporary
    /// <see cref="XDocument"/> for validation and the wrapper is discarded afterwards; the
    /// caller's tree is annotated in place and keeps its node identity.
    /// </summary>
    /// <param name="element">The root of the subtree to validate. It is annotated in place.</param>
    /// <param name="schemas">The compiled schema set to validate against.</param>
    /// <param name="handler">
    /// Optional additional validation-event handler. It receives every validation event
    /// (errors and warnings) after the event has been recorded in the returned result.
    /// Because a handler is always supplied to the underlying validator, validation errors
    /// never throw from the validation itself.
    /// </param>
    /// <param name="throwOnInvalid">
    /// When <c>true</c> and validation produced errors, throws an
    /// <see cref="XmlSchemaValidationException"/> carrying the first error instead of
    /// returning the result. Defaults to <c>false</c>: the caller inspects the returned
    /// <see cref="XdmSubtreeValidationResult"/>.
    /// </param>
    /// <returns>
    /// A <see cref="XdmSubtreeValidationResult"/> describing the outcome. Even when invalid,
    /// PSVI annotations (partial or complete) are attached to the live nodes.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="element"/> or <paramref name="schemas"/> is null.</exception>
    /// <exception cref="XmlSchemaValidationException"><paramref name="throwOnInvalid"/> is <c>true</c> and validation produced errors.</exception>
    public static XdmSubtreeValidationResult ValidateSubtree(XElement element, XmlSchemaSet schemas,
        ValidationEventHandler? handler = null, bool throwOnInvalid = false)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(schemas);

        var errors = new List<ValidationEventArgs>();
        // XDocument.Validate requires a document root. When the subtree is already part of
        // a tree (an element child, or the root of an XDocument — note XElement.Parent is
        // null for a document root), wrapping it directly would clone the element (LINQ to
        // XML reparenting semantics) and the PSVI would attach to the clone instead of the
        // caller's tree; so a deep clone is validated and the PSVI annotations are copied
        // back onto the live XObjects, in document order, afterwards. A detached subtree is
        // wrapped directly and annotated live.
        var isAttached = element.Document != null || element.Parent != null;
        var root = isAttached ? new XElement(element) : element;
        var wrapper = new XDocument(root);
        wrapper.Validate(schemas, (sender, e) =>
        {
            if (e.Severity == XmlSeverityType.Error)
                errors.Add(e);
            handler?.Invoke(sender, e);
        }, addSchemaInfo: true);

        if (isAttached)
            CopySchemaAnnotations(root, element);

        var result = new XdmSubtreeValidationResult(errors.Count == 0, errors);
        if (throwOnInvalid && !result.IsValid)
        {
            var first = result.Errors[0];
            throw new XmlSchemaValidationException(
                $"Subtree validation failed against the supplied schema(s): {first.Message}", first.Exception);
        }
        return result;
    }

    /// <summary>
    /// Attaches a host-built <see cref="IXmlSchemaInfo"/> annotation to the node without
    /// performing validation (e.g. declaration-only annotation, nilled elements, nodes
    /// validated elsewhere). The annotation is read by every typed-value surface via
    /// <c>XObject.GetSchemaInfo()</c>.
    /// </summary>
    /// <param name="node">The node to annotate.</param>
    /// <param name="annotation">The schema annotation to attach.</param>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> or <paramref name="annotation"/> is null.</exception>
    public static void Annotate(XObject node, IXmlSchemaInfo annotation)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(annotation);
        node.AddAnnotation(annotation);
    }

    /// <summary>
    /// Copies the PSVI annotations attached by validation from the validated clone onto the
    /// live subtree, walking both trees in document order (attributes in attribute order,
    /// elements in child order).
    /// </summary>
    private static void CopySchemaAnnotations(XElement annotated, XElement live)
    {
        var info = annotated.GetSchemaInfo();
        if (info != null)
            live.AddAnnotation(info);

        using var liveAttrEnumerator = live.Attributes().GetEnumerator();
        foreach (var attr in annotated.Attributes())
        {
            if (!liveAttrEnumerator.MoveNext())
                break;
            var attrInfo = attr.GetSchemaInfo();
            if (attrInfo != null)
                liveAttrEnumerator.Current.AddAnnotation(attrInfo);
        }

        using var liveChildEnumerator = live.Elements().GetEnumerator();
        foreach (var child in annotated.Elements())
        {
            if (!liveChildEnumerator.MoveNext())
                break;
            CopySchemaAnnotations(child, liveChildEnumerator.Current);
        }
    }
}
