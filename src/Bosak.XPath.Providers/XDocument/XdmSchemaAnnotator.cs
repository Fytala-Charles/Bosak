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
//                      | Charles Korthout | 0.2   | 23-09-2026     | REQ-099 seam H4: class made partial for the validation-mode service (see                 |
//                      |                  |       |                | XdmSchemaAnnotator.Validation.cs)                                                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 24-09-2026     | REQ-105 (PA-3): ValidateSubtree applies whiteSpace-facet normalization to              |
//                      |                  |       |                | simple-typed content after successful validation (match-136..141)                      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 30-09-2026     | REQ-114 (PB-3 C9): CopySchemaAnnotations pairs attributes by XName and ports XSD      |
//                      |                  |       |                | default attributes (clone-only nodes) onto the live tree; ValidateSubtree strips      |
//                      |                  |       |                | element-only whitespace on success (strip-space-007); GetSchemaContentModel helper     |
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
public static partial class XdmSchemaAnnotator
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

        // Validating XDM construction discards whitespace-only text nodes in element-only
        // content (XDM §3.3.1.1) — applied to the live tree, guided by the fresh PSVI, so
        // strip-space rules over an already-validated tree see the canonical shape
        // (strip-space-007; mirrors the ValidateCore treatment).
        if (errors.Count == 0)
            StripElementOnlyContentWhitespace(element);

        // Validating XDM construction records the schema-normalized value for simple-typed
        // content (XDM §3.3.2); apply the governing type's whiteSpace facet to the live tree.
        // The per-node validity check makes this safe for partially validated trees.
        ApplySchemaNormalizedValues(element);

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
    /// live subtree, walking both trees in document order. Attributes are paired by
    /// <see cref="XName"/> (never by position): the validated clone can legitimately carry
    /// MORE attributes than the live element, because <c>XDocument.Validate</c> adds the XSD
    /// default attributes to the clone only. Such clone-only attributes are default
    /// attributes of the governing type: they are ported onto the live element together with
    /// their PSVI annotation, so the live tree reflects the validated infoset exactly
    /// (REQ-114/PB-3 C9; validation-0701, import-schema-048).
    /// </summary>
    private static void CopySchemaAnnotations(XElement annotated, XElement live)
    {
        var info = annotated.GetSchemaInfo();
        if (info != null)
            live.AddAnnotation(info);

        foreach (var attr in annotated.Attributes())
        {
            var liveAttr = live.Attribute(attr.Name);
            if (liveAttr is null)
            {
                // Clone-only namespace declarations are validation-time namespace fixup;
                // they must not leak into the live tree — EXCEPT the XSD 1.1-style fixup
                // declarations pre-injected for a ref use-site default attribute, which are
                // part of the validated infoset (import-schema-164: the default attribute's
                // namespace must be declared and visible on the namespace axis).
                if (attr.IsNamespaceDeclaration)
                {
                    if (attr.Annotation<RefDefaultInjection>() is null)
                        continue;
                    liveAttr = new XAttribute(attr);
                    live.Add(liveAttr);
                    continue;
                }
                // XSD default attribute added to the clone by validation only: port it onto
                // the live element, with its PSVI annotation (validation-0701, import-schema-048).
                liveAttr = new XAttribute(attr);
                live.Add(liveAttr);
            }

            var attrInfo = attr.GetSchemaInfo();
            if (attrInfo != null)
                liveAttr.AddAnnotation(attrInfo);
        }

        using var liveChildEnumerator = live.Elements().GetEnumerator();
        foreach (var child in annotated.Elements())
        {
            if (!liveChildEnumerator.MoveNext())
                break;
            CopySchemaAnnotations(child, liveChildEnumerator.Current);
        }
    }

    /// <summary>
    /// Returns the PSVI content model of the element's governing schema type —
    /// <see cref="XmlSchemaContentType.ElementOnly"/> for element-only content,
    /// <see cref="XmlSchemaContentType.Mixed"/> for mixed content, and
    /// <see cref="XmlSchemaContentType.TextOnly"/> for complex types with simple content.
    /// Whitespace-only text nodes are ignorable only in element-only content (XDM §3.3.1.1);
    /// simple-typed and mixed content preserve them (strip-space-008).
    /// </summary>
    /// <param name="element">The element whose content model is queried.</param>
    /// <returns>
    /// The governing complex type's content type, or <c>null</c> when the element carries
    /// no PSVI annotation, is governed by a simple type, or has no governing schema type —
    /// callers must treat <c>null</c> as "not known to be element-only".
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
    public static XmlSchemaContentType? GetSchemaContentModel(XElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        var info = element.GetSchemaInfo();
        if ((info?.SchemaElement?.ElementSchemaType ?? info?.SchemaType) is not XmlSchemaComplexType complexType)
            return null;
        return complexType.ContentType;
    }
}
