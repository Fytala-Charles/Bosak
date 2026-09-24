// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 september 2026
// PURPOSE              : Validation-mode service (strict/lax/strip/preserve, named types, document level) for in-memory subtrees
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 23-09-2026     | Creation (REQ-099 seam H4)                                                               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 23-09-2026     | REQ-103 (PB-1): named-simple-type attribute validation passes a real NameTable +       |
//                      |                  |       |                | in-scope namespace resolver — NCName-family datatypes NRE'd on null (import-schema-001) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 24-09-2026     | REQ-105 (PA-3): apply whiteSpace-facet normalization to simple-typed content after     |
//                      |                  |       |                | successful validation (schema-normalized values, XDM 3.3.2; match-136..141)            |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Validation-mode entry points of <see cref="XdmSchemaAnnotator"/> (XSLT
/// <c>validation</c>/<c>[xsl:]type</c> semantics, XSLT 3.0 §25.4). The service is
/// error-code-agnostic: failures are reported through the returned
/// <see cref="XdmSubtreeValidationResult"/> (and the optional handler); hosts map them to
/// their own error codes (XSLT <c>XTTE15xx</c>, XQuery <c>XQDYxxxx</c>).
/// </summary>
public static partial class XdmSchemaAnnotator
{
    private const string XsNamespaceUri = "http://www.w3.org/2001/XMLSchema";
    private const string XsiNamespaceUri = "http://www.w3.org/2001/XMLSchema-instance";

    /// <summary>
    /// Validates (or strips/preserves annotations on) the supplied element subtree against
    /// the compiled schema set, following the XSLT <c>validation</c>/<c>[xsl:]type</c>
    /// semantics of <paramref name="options"/>. PSVI (<see cref="IXmlSchemaInfo"/>)
    /// annotations are attached to the live <see cref="XObject"/>s of the subtree, so every
    /// typed-value surface of the engine reflects the outcome immediately and node identity
    /// is preserved.
    /// </summary>
    /// <param name="element">
    /// The root of the subtree. With <see cref="XdmValidationOptions.DocumentLevel"/> set it
    /// acts as the content container of a document node and must contain exactly one element
    /// child and no text node children. It is annotated in place.
    /// </param>
    /// <param name="schemas">The compiled schema set to validate against.</param>
    /// <param name="options">The validation mode, optional named target type, and document-level flag.</param>
    /// <param name="handler">
    /// Optional additional validation-event handler. It receives every validation event
    /// (errors and warnings) after the event has been recorded in the returned result.
    /// </param>
    /// <param name="throwOnInvalid">
    /// When <c>true</c> and the outcome is invalid, throws an
    /// <see cref="XmlSchemaValidationException"/> carrying the first error instead of
    /// returning the result.
    /// </param>
    /// <returns>A <see cref="XdmSubtreeValidationResult"/> describing the outcome.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="element"/>, <paramref name="schemas"/>, or <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="options"/> names a type that is not present in the schema set and is not a built-in XML Schema type.</exception>
    /// <exception cref="XmlSchemaValidationException"><paramref name="throwOnInvalid"/> is <c>true</c> and the outcome is invalid.</exception>
    public static XdmSubtreeValidationResult Validate(XElement element, XmlSchemaSet schemas,
        XdmValidationOptions options, ValidationEventHandler? handler = null, bool throwOnInvalid = false)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(schemas);
        ArgumentNullException.ThrowIfNull(options);

        switch (options.Mode)
        {
            case XdmValidationMode.Strip:
                StripSchemaAnnotations(element);
                return new XdmSubtreeValidationResult(true, []);
            case XdmValidationMode.Preserve:
                return new XdmSubtreeValidationResult(true, []);
        }

        if (options.DocumentLevel)
        {
            // Document-node validation (XSLT 3.0 §25.4.2): the content must comprise exactly
            // one element node, no text nodes, and zero or more comment/PI nodes; validation
            // then applies to the single element child.
            XElement? rootChild = null;
            var elementChildren = 0;
            var hasText = false;
            foreach (var node in element.Nodes())
            {
                switch (node)
                {
                    case XElement child:
                        elementChildren++;
                        rootChild = child;
                        break;
                    case XText:
                        hasText = true;
                        break;
                }
            }
            if (elementChildren != 1 || hasText || rootChild is null)
            {
                const string shapeMessage =
                    "The content of a document node to be validated must comprise exactly one element node and no text nodes.";
                var shapeResult = new XdmSubtreeValidationResult(false, [], shapeMessage);
                if (throwOnInvalid)
                    throw new XmlSchemaValidationException(shapeMessage);
                return shapeResult;
            }
            return Validate(rootChild, schemas, options with { DocumentLevel = false }, handler, throwOnInvalid);
        }

        // Named-type validation (XSLT [xsl:]type): resolve the type up-front; unknown types
        // are a usage error of the service (the host maps it to its own static error code).
        XmlSchemaType? namedType = null;
        if (options.TypeName is { } typeName)
        {
            // xs:untyped is an XDM concept without an XSD counterpart; validating against it
            // is defined to behave exactly like validation="strip" (XSLT 3.0 §25.4.1.2).
            if (typeName.Namespace == XsNamespaceUri && typeName.Name == "untyped")
            {
                StripSchemaAnnotations(element);
                return new XdmSubtreeValidationResult(true, []);
            }
            namedType = ResolveSchemaType(schemas, typeName)
                ?? throw new ArgumentException(
                    $"The type '{typeName}' is not defined in the supplied schema set.", nameof(options));

            // xs:untypedAtomic validates as a string (element children make it invalid) and
            // annotates the node as untypedAtomic; it cannot be driven through xsi:type
            // because .NET keeps the type in the XPath datatypes namespace.
            if (IsUntypedAtomicType(namedType))
            {
                StripSchemaAnnotations(element);
                if (element.Elements().Any())
                {
                    const string untypedAtomicMessage =
                        "An element with element children cannot be validated against xs:untypedAtomic.";
                    var untypedAtomicResult = new XdmSubtreeValidationResult(false, [], untypedAtomicMessage);
                    if (throwOnInvalid)
                        throw new XmlSchemaValidationException(untypedAtomicMessage);
                    return untypedAtomicResult;
                }
                element.AddAnnotation(new SimpleTypeSchemaInfo((XmlSchemaSimpleType)namedType, XmlSchemaValidity.Valid));
                return new XdmSubtreeValidationResult(true, []);
            }
        }

        // Strict validation requires a matching top-level element declaration; xsi:type is
        // no substitute (XSLT 3.0 §25.4.1.1 — the host maps this outcome to XTTE1512).
        if (options.Mode == XdmValidationMode.Strict && namedType is null
            && schemas.GlobalElements[new XmlQualifiedName(element.Name.LocalName, element.Name.NamespaceName)] is null)
        {
            var undeclaredResult = new XdmSubtreeValidationResult(false, [],
                $"The element '{element.Name}' has no matching top-level element declaration in the supplied schema set.");
            if (throwOnInvalid)
                throw new XmlSchemaValidationException(undeclaredResult.FailureMessage);
            return undeclaredResult;
        }

        // An attached element must not be mutated by validation (xsi:type injection, or the
        // reparenting implied by wrapping it in an XDocument), so a deep clone is validated
        // and the resulting PSVI is copied back onto the live tree (H3 machinery). The clone
        // receives the in-scope namespace bindings of the live element so that QName-valued
        // content and xsi:type resolve identically during validity assessment.
        var isAttached = element.Document != null || element.Parent != null;
        var clone = isAttached || namedType is not null ? new XElement(element) : element;
        if (!ReferenceEquals(clone, element))
            ImportInScopeNamespaces(element, clone);

        var errors = new List<ValidationEventArgs>();
        ValidationEventHandler recorder = (sender, e) =>
        {
            if (e.Severity == XmlSeverityType.Error)
                errors.Add(e);
            handler?.Invoke(sender, e);
        };

        var effectiveSet = schemas;
        if (options.Mode == XdmValidationMode.Lax && namedType is null)
            effectiveSet = AugmentLaxRootDeclaration(clone, schemas);

        // Named-type validation is driven by an injected xsi:type attribute on the clone
        // root; it is removed before the annotations are copied back so the temporary
        // attribute never leaks into the caller's tree.
        var injectedXsiNs = false;
        string? addedTypePrefix = null;
        if (namedType is not null)
        {
            var (typeNs, typeLocal) = (options.TypeName!.Namespace, options.TypeName.Name);
            if (clone.GetNamespaceOfPrefix("xsi") is null)
            {
                clone.SetAttributeValue(XNamespace.Xmlns + "xsi", XsiNamespaceUri);
                injectedXsiNs = true;
            }
            var typePrefix = clone.Attributes()
                .Where(a => a.IsNamespaceDeclaration && a.Value == typeNs)
                .Select(a => a.Name.LocalName)
                .FirstOrDefault(p => p.Length > 0);
            if (typePrefix is null)
            {
                typePrefix = typeNs == XsNamespaceUri ? "xs" : GenerateUniquePrefix(clone, "t");
                clone.SetAttributeValue(XNamespace.Xmlns + typePrefix, typeNs);
                addedTypePrefix = typePrefix;
            }
            clone.SetAttributeValue(XNamespace.Get(XsiNamespaceUri) + "type", $"{typePrefix}:{typeLocal}");
        }

        var wrapper = new XDocument(clone);
        wrapper.Validate(effectiveSet, recorder, addSchemaInfo: true);

        if (namedType is not null)
        {
            clone.SetAttributeValue(XNamespace.Get(XsiNamespaceUri) + "type", null);
            if (addedTypePrefix is not null)
                clone.SetAttributeValue(XNamespace.Xmlns + addedTypePrefix, null);
            if (injectedXsiNs)
                clone.SetAttributeValue(XNamespace.Xmlns + "xsi", null);
        }

        if (!ReferenceEquals(clone, element))
            CopySchemaAnnotations(clone, element);

        // Validating XDM construction discards whitespace-only text nodes in element-only
        // content (XDM §3.3.1.1). Applied to the live tree, guided by the fresh PSVI.
        if (errors.Count == 0)
            StripElementOnlyContentWhitespace(element);

        // Schema-normalized values (the whiteSpace facet) apply wherever validation
        // succeeded, including in partially validated trees (match-136..141).
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
    /// Validates (or strips/preserves the annotation on) a single attribute node. The
    /// attribute may be parentless or attached to an element; it is never modified, and the
    /// resulting PSVI annotation is attached to it in place.
    /// </summary>
    /// <param name="attribute">The attribute to validate.</param>
    /// <param name="schemas">The compiled schema set to validate against.</param>
    /// <param name="options">
    /// The validation mode and optional named target type. When
    /// <see cref="XdmValidationOptions.TypeName"/> is supplied it must identify a simple type
    /// (validating an attribute against a complex type is rejected with
    /// <see cref="ArgumentException"/>; hosts map it to their own error code).
    /// </param>
    /// <param name="handler">
    /// Optional additional validation-event handler. It receives every event produced by the
    /// schema validator (declaration-driven validation); failures detected without the
    /// validator (named-type datatype checks, missing strict-mode declarations) surface only
    /// through the returned result.
    /// </param>
    /// <param name="throwOnInvalid">
    /// When <c>true</c> and the outcome is invalid, throws an
    /// <see cref="XmlSchemaValidationException"/> carrying the first error instead of
    /// returning the result.
    /// </param>
    /// <returns>A <see cref="XdmSubtreeValidationResult"/> describing the outcome.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="attribute"/>, <paramref name="schemas"/>, or <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="options"/> names a type that is unknown or not a simple type.</exception>
    /// <exception cref="XmlSchemaValidationException"><paramref name="throwOnInvalid"/> is <c>true</c> and the outcome is invalid.</exception>
    public static XdmSubtreeValidationResult ValidateAttribute(XAttribute attribute, XmlSchemaSet schemas,
        XdmValidationOptions options, ValidationEventHandler? handler = null, bool throwOnInvalid = false)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        ArgumentNullException.ThrowIfNull(schemas);
        ArgumentNullException.ThrowIfNull(options);

        switch (options.Mode)
        {
            case XdmValidationMode.Strip:
                StripSchemaAnnotations(attribute);
                return new XdmSubtreeValidationResult(true, []);
            case XdmValidationMode.Preserve:
                return new XdmSubtreeValidationResult(true, []);
        }

        var errors = new List<ValidationEventArgs>();
        ValidationEventHandler recorder = (sender, e) =>
        {
            if (e.Severity == XmlSeverityType.Error)
                errors.Add(e);
            handler?.Invoke(sender, e);
        };

        // The attribute may be parented to a constructed element; validate a clone and copy
        // the resulting annotation back so the caller's tree is only annotated, never mutated.
        var clone = new XAttribute(attribute);
        XdmSubtreeValidationResult result;

        if (options.TypeName is { } typeName)
        {
            // Named simple type: validate the attribute value against the type's datatype
            // directly (a free-standing attribute has no declaration context to validate
            // against, and .NET partial validation requires a declared XmlSchemaAttribute).
            // Failures detected here surface only through the result (the handler receives
            // validator-produced events, and the datatype check produces none).
            if (ResolveSchemaType(schemas, typeName) is not { } namedType)
                throw new ArgumentException(
                    $"The type '{typeName}' is not defined in the supplied schema set.", nameof(options));
            if (namedType is not XmlSchemaSimpleType simpleType)
                throw new ArgumentException(
                    $"The type '{typeName}' is a complex type; an attribute can only be validated against a simple type.", nameof(options));

            string? valueError = null;
            try
            {
                // NCName-family datatypes dereference the name table and namespace resolver;
                // passing null crashes inside System.Xml.Schema (import-schema-001 family).
                // The namespace context is the original attribute's in-scope bindings, so a
                // QName-family value would resolve the same prefixes (QName/NOTATION content
                // is rejected upstream with XTTE1545 before reaching this path).
                var nameTable = new NameTable();
                var nsmgr = new XmlNamespaceManager(nameTable);
                for (var scope = attribute.Parent; scope is not null; scope = scope.Parent)
                {
                    foreach (var nsAttr in scope.Attributes())
                    {
                        if (!nsAttr.IsNamespaceDeclaration)
                            continue;
                        var prefix = nsAttr.Name.LocalName == "xmlns" ? string.Empty : nsAttr.Name.LocalName;
                        if (nsmgr.LookupNamespace(prefix) is null)
                            nsmgr.AddNamespace(prefix, nsAttr.Value);
                    }
                }
                simpleType.Datatype?.ParseValue(clone.Value, nameTable, nsmgr);
            }
            catch (Exception ex) when (ex is XmlSchemaException or XmlException or FormatException or OverflowException)
            {
                valueError = ex.Message;
            }
            clone.AddAnnotation(new SimpleTypeSchemaInfo(simpleType,
                valueError is null ? XmlSchemaValidity.Valid : XmlSchemaValidity.Invalid));
            result = valueError is null
                ? new XdmSubtreeValidationResult(true, [])
                : new XdmSubtreeValidationResult(false, [],
                    $"The value '{clone.Value}' is not valid against type '{typeName}': {valueError}");
        }
        else
        {
            // Declaration-driven validation: .NET partial validation of a free-standing
            // attribute requires a parent, so the clone is temporarily hosted.
            var declaration = schemas.GlobalAttributes[
                new XmlQualifiedName(clone.Name.LocalName, clone.Name.NamespaceName)] as XmlSchemaAttribute;
            if (declaration is null)
            {
                // Strict without a declaration is a failure; lax without a declaration
                // leaves the attribute unvalidated (and untyped).
                result = options.Mode == XdmValidationMode.Strict
                    ? new XdmSubtreeValidationResult(false, [],
                        $"The attribute '{clone.Name}' has no matching top-level attribute declaration in the supplied schema set.")
                    : new XdmSubtreeValidationResult(true, []);
            }
            else
            {
                var host = new XElement("h4-attr-host", clone);
                clone.Validate(declaration, schemas, recorder, addSchemaInfo: true);
                clone.Remove();
                host.RemoveAnnotations<object>();
                result = new XdmSubtreeValidationResult(errors.Count == 0, errors);
            }
        }

        var cloneInfo = clone.GetSchemaInfo();
        if (cloneInfo is not null)
        {
            attribute.RemoveAnnotations(typeof(IXmlSchemaInfo));
            attribute.AddAnnotation(cloneInfo);
        }

        if (throwOnInvalid && !result.IsValid)
            throw new XmlSchemaValidationException(
                $"Attribute validation failed against the supplied schema(s): {result.FailureMessage}");
        return result;
    }

    /// <summary>
    /// Removes every PSVI (<see cref="IXmlSchemaInfo"/>) annotation from the subtree rooted
    /// at <paramref name="root"/> (a document, element, or attribute), leaving all elements
    /// <c>xs:untyped</c>, all attributes <c>xs:untypedAtomic</c>, and the nilled property
    /// false — the XSLT <c>validation="strip"</c> and <c>input-type-annotations="strip"</c>
    /// semantics. Annotations of other kinds are left untouched.
    /// </summary>
    /// <param name="root">The document, element, or attribute whose PSVI annotations are removed.</param>
    /// <exception cref="ArgumentNullException"><paramref name="root"/> is null.</exception>
    public static void StripSchemaAnnotations(XObject root)
    {
        ArgumentNullException.ThrowIfNull(root);

        switch (root)
        {
            case XDocument document:
                foreach (var element in document.Descendants())
                    StripElementAnnotations(element);
                break;
            case XElement element:
                StripElementAnnotations(element);
                foreach (var descendant in element.Descendants())
                    StripElementAnnotations(descendant);
                break;
            case XAttribute attribute:
                attribute.RemoveAnnotations(typeof(IXmlSchemaInfo));
                break;
        }
    }

    /// <summary>
    /// Returns <c>true</c> when the supplied schema type is, or is derived (by restriction,
    /// list, or union) from, the primitive types <c>xs:QName</c> or <c>xs:NOTATION</c>.
    /// Attribute values cannot be validated against such types in XSLT construction because
    /// the namespace bindings they depend on are not available for a parentless attribute
    /// (XSLT 3.0 §25.4.1.1, XTTE1545).
    /// </summary>
    /// <param name="type">The schema type to inspect.</param>
    /// <returns><c>true</c> when the type derives from <c>xs:QName</c> or <c>xs:NOTATION</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
    public static bool IsQNameOrNotationDerived(XmlSchemaType type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return IsQNameOrNotationDerivedCore(type, new HashSet<XmlSchemaType>(ReferenceEqualityComparer.Instance));
    }

    private static bool IsQNameOrNotationDerivedCore(XmlSchemaType type, HashSet<XmlSchemaType> seen)
    {
        if (!seen.Add(type))
            return false;

        var qualifiedName = type.QualifiedName;
        if (qualifiedName.Namespace == XsNamespaceUri && qualifiedName.Name is "QName" or "NOTATION")
            return true;

        if (type is XmlSchemaSimpleType simpleType)
        {
            switch (simpleType.Content)
            {
                case XmlSchemaSimpleTypeUnion union:
                    foreach (var member in union.BaseMemberTypes ?? [])
                    {
                        if (IsQNameOrNotationDerivedCore(member, seen))
                            return true;
                    }
                    break;
                case XmlSchemaSimpleTypeList list:
                    if (list.BaseItemType is { } itemType && IsQNameOrNotationDerivedCore(itemType, seen))
                        return true;
                    break;
            }
        }

        return type.BaseXmlSchemaType is { } baseType && IsQNameOrNotationDerivedCore(baseType, seen);
    }

    /// <summary>
    /// Resolves a named schema type in the schema set, falling back to the XML Schema
    /// built-in types (which are always in scope, even without any imported schema).
    /// <c>xs:untyped</c> has no schema-type counterpart (it is an XDM concept) and resolves
    /// to <c>null</c>; hosts map it to <c>validation="strip"</c> semantics.
    /// </summary>
    /// <param name="schemas">The compiled schema set to search.</param>
    /// <param name="typeName">The expanded type name.</param>
    /// <returns>The schema type, or <c>null</c> when it is not defined.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="schemas"/> or <paramref name="typeName"/> is null.</exception>
    public static XmlSchemaType? ResolveSchemaType(XmlSchemaSet schemas, XmlQualifiedName typeName)
    {
        ArgumentNullException.ThrowIfNull(schemas);
        ArgumentNullException.ThrowIfNull(typeName);
        return ResolveSchemaTypeCore(schemas, typeName);
    }

    private static XmlSchemaType? ResolveSchemaTypeCore(XmlSchemaSet schemas, XmlQualifiedName typeName)
    {
        if (schemas.GlobalTypes[typeName] is XmlSchemaType declared)
            return declared;
        if (typeName.Namespace == XsNamespaceUri)
        {
            if (XmlSchemaType.GetBuiltInSimpleType(typeName) is { } simpleType)
                return simpleType;
            // .NET keeps untypedAtomic in the XPath datatypes namespace rather than xs:.
            if (typeName.Name == "untypedAtomic")
                return XmlSchemaType.GetBuiltInSimpleType(
                    new XmlQualifiedName("untypedAtomic", "http://www.w3.org/2003/11/xpath-datatypes"));
            return XmlSchemaType.GetBuiltInComplexType(typeName);
        }
        return null;
    }

    /// <summary>
    /// Returns <c>true</c> when the type is the untypedAtomic built-in (reported by .NET in
    /// the XPath datatypes namespace).
    /// </summary>
    private static bool IsUntypedAtomicType(XmlSchemaType type)
        => type is XmlSchemaSimpleType
           && type.QualifiedName.Name == "untypedAtomic"
           && type.QualifiedName.Namespace is "http://www.w3.org/2001/XMLSchema" or "http://www.w3.org/2003/11/xpath-datatypes";

    /// <summary>
    /// Lax validation of an element whose name matches no global element declaration (and
    /// which carries no xsi:type): .NET's validator reports an undeclared root as an error,
    /// so a dynamic schema declaring the actual root as xs:anyType is added to a copy of the
    /// schema set. The root is then valid while declared descendants are still validated
    /// (anyType allows any content and processes child elements laxly).
    /// </summary>
    private static XmlSchemaSet AugmentLaxRootDeclaration(XElement root, XmlSchemaSet schemas)
    {
        if (root.Attribute(XNamespace.Get(XsiNamespaceUri) + "type") is not null)
            return schemas;
        if (schemas.GlobalElements[new XmlQualifiedName(root.Name.LocalName, root.Name.NamespaceName)] is not null)
            return schemas;

        var laxSchemaSet = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };
        foreach (XmlSchema existing in schemas.Schemas())
            laxSchemaSet.Add(existing);

        var targetNsAttr = string.IsNullOrEmpty(root.Name.NamespaceName)
            ? ""
            : $" targetNamespace=\"{SecurityElementEscape(root.Name.NamespaceName)}\"";
        var schemaXml = $"<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"{targetNsAttr}><xs:element name=\"{SecurityElementEscape(root.Name.LocalName)}\" type=\"xs:anyType\" /></xs:schema>";
        using var schemaReader = XmlReader.Create(new StringReader(schemaXml));
        laxSchemaSet.Add(null, schemaReader);
        laxSchemaSet.Compile();
        return laxSchemaSet;
    }

    private static string SecurityElementEscape(string value)
        => System.Security.SecurityElement.Escape(value) ?? value;

    /// <summary>
    /// Copies the in-scope namespace bindings of the source element (including inherited
    /// declarations) onto a validation clone, so that QName-typed content and xsi:type
    /// attributes resolve against the same namespace context during validity assessment.
    /// Only bindings the clone does not already declare are added; they are appended after
    /// the clone's own attributes, which keeps the attribute-order pairing used by
    /// <see cref="CopySchemaAnnotations"/> intact.
    /// </summary>
    private static void ImportInScopeNamespaces(XElement source, XElement clone)
    {
        var bindings = new Dictionary<string, string>();
        for (var current = source; current is not null; current = current.Parent)
        {
            foreach (var attr in current.Attributes())
            {
                if (!attr.IsNamespaceDeclaration)
                    continue;
                var prefix = attr.Name.NamespaceName == XNamespace.Xmlns ? attr.Name.LocalName : string.Empty;
                bindings.TryAdd(prefix, attr.Value);
            }
        }

        foreach (var (prefix, uri) in bindings)
        {
            if (prefix is "xml" or "xmlns")
                continue;
            if (string.IsNullOrEmpty(prefix))
            {
                if (clone.Attribute("xmlns") is null && !string.IsNullOrEmpty(uri))
                    clone.SetAttributeValue("xmlns", uri);
            }
            else if (clone.GetNamespaceOfPrefix(prefix) is null)
            {
                clone.SetAttributeValue(XNamespace.Xmlns + prefix, uri);
            }
        }
    }

    /// <summary>
    /// Removes whitespace-only text nodes from elements whose validated schema type has
    /// element-only content. Mirrors
    /// <see cref="XDocumentProvider"/>'s document-level whitespace stripping, but also covers
    /// subtrees validated against a named type (where no element declaration is present).
    /// </summary>
    private static void StripElementOnlyContentWhitespace(XElement root)
    {
        foreach (var element in root.DescendantsAndSelf())
        {
            var info = element.GetSchemaInfo();
            if (info is null)
                continue;
            if ((info.SchemaElement?.ElementSchemaType ?? info.SchemaType) is not XmlSchemaComplexType complexType
                || complexType.ContentType != XmlSchemaContentType.ElementOnly)
                continue;
            foreach (var text in element.Nodes().OfType<XText>()
                         .Where(t => string.IsNullOrWhiteSpace(t.Value))
                         .ToList())
            {
                text.Remove();
            }
        }
    }

    private static void StripElementAnnotations(XElement element)
    {
        element.RemoveAnnotations(typeof(IXmlSchemaInfo));
        foreach (var attribute in element.Attributes())
            attribute.RemoveAnnotations(typeof(IXmlSchemaInfo));
    }

    private static string GenerateUniquePrefix(XElement element, string basePrefix)
    {
        var index = 0;
        string prefix;
        do
        {
            prefix = index == 0 ? basePrefix : $"{basePrefix}{index}";
            index++;
        } while (element.GetNamespaceOfPrefix(prefix) is not null);
        return prefix;
    }

    /// <summary>
    /// Applies the whiteSpace facet of the governing simple type to the text content of
    /// elements and to attribute values in the subtree: validating XDM construction records
    /// the schema-normalized value, so the string-value of a simple-typed node is the
    /// normalized form (XDM §3.3.2; match-136..141). Only nodes whose PSVI validity is
    /// <see cref="XmlSchemaValidity.Valid"/> are rewritten, so partially validated trees
    /// (some siblings invalid) are normalized exactly where validation succeeded. Only
    /// single-text-node simple content is rewritten; mixed or multi-node content is left
    /// untouched.
    /// </summary>
    /// <param name="root">The root of the validated subtree (already PSVI-annotated).</param>
    internal static void ApplySchemaNormalizedValues(XElement root)
    {
        foreach (var element in root.DescendantsAndSelf())
        {
            foreach (var attribute in element.Attributes())
            {
                if (attribute.IsNamespaceDeclaration)
                    continue;
                var attributeInfo = attribute.GetSchemaInfo();
                if (attributeInfo?.Validity == XmlSchemaValidity.Valid
                    && attributeInfo.SchemaType is XmlSchemaSimpleType
                    && GetEffectiveWhiteSpace(attributeInfo.SchemaType) is { } attrWhiteSpace)
                {
                    var normalized = NormalizeWhiteSpace(attribute.Value, attrWhiteSpace);
                    if (normalized != attribute.Value)
                        attribute.Value = normalized;
                }
            }

            if (element.Elements().Any())
                continue; // not simple content
            var elementInfo = element.GetSchemaInfo();
            if (elementInfo?.Validity != XmlSchemaValidity.Valid)
                continue;
            var elementType = elementInfo.SchemaType;
            bool isSimpleContent = elementType is XmlSchemaSimpleType
                || elementType is XmlSchemaComplexType { ContentType: XmlSchemaContentType.TextOnly };
            if (!isSimpleContent || GetEffectiveWhiteSpace(elementType) is not { } whiteSpace)
                continue;
            var textNodes = element.Nodes().OfType<XText>().ToList();
            if (textNodes.Count != 1)
                continue;
            var normalizedText = NormalizeWhiteSpace(textNodes[0].Value, whiteSpace);
            if (normalizedText != textNodes[0].Value)
                textNodes[0].Value = normalizedText;
        }
    }

    /// <summary>
    /// Computes the effective whiteSpace facet of a schema type: an explicit
    /// <c>xs:whiteSpace</c> facet on the nearest restriction wins; list and union types
    /// always collapse; built-in types follow the fixed XSD facet (preserve for
    /// <c>xs:string</c>, replace for <c>xs:normalizedString</c>, collapse otherwise).
    /// </summary>
    /// <param name="type">The governing schema type, or <c>null</c>.</param>
    /// <returns>"preserve", "replace", or "collapse"; <c>null</c> when no facet applies.</returns>
    private static string? GetEffectiveWhiteSpace(XmlSchemaType? type)
    {
        var visited = new HashSet<XmlSchemaType>();
        for (var current = type; current is not null && visited.Add(current);)
        {
            switch (current)
            {
                case XmlSchemaSimpleType simple:
                    if (simple.Content is XmlSchemaSimpleTypeRestriction restriction)
                    {
                        foreach (var facet in restriction.Facets)
                        {
                            if (facet is XmlSchemaWhiteSpaceFacet whiteSpaceFacet)
                                return whiteSpaceFacet.Value;
                        }
                    }
                    else if (simple.Content is XmlSchemaSimpleTypeList or XmlSchemaSimpleTypeUnion)
                    {
                        return "collapse";
                    }
                    if (simple.Content is null && simple.QualifiedName.Namespace == XsNamespaceUri)
                    {
                        return simple.QualifiedName.Name switch
                        {
                            "string" or "anySimpleType" or "anyAtomicType" or "untypedAtomic" => "preserve",
                            "normalizedString" => "replace",
                            _ => "collapse",
                        };
                    }
                    current = simple.BaseXmlSchemaType;
                    break;
                case XmlSchemaComplexType complex:
                    if (complex.ContentType != XmlSchemaContentType.TextOnly)
                        return null;
                    current = complex.BaseXmlSchemaType;
                    break;
            }
        }
        return null;
    }

    /// <summary>
    /// Applies an XSD whiteSpace facet to a value: "replace" turns tab/newline/CR into
    /// spaces; "collapse" additionally merges runs of spaces and trims the ends.
    /// </summary>
    private static string NormalizeWhiteSpace(string value, string whiteSpace)
    {
        if (whiteSpace == "preserve")
            return value;
        var replaced = value.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');
        if (whiteSpace == "replace")
            return replaced;
        // collapse
        var sb = new System.Text.StringBuilder(replaced.Length);
        bool pendingSpace = false;
        foreach (var c in replaced)
        {
            if (c == ' ')
            {
                pendingSpace = sb.Length > 0;
                continue;
            }
            if (pendingSpace)
                sb.Append(' ');
            pendingSpace = false;
            sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Host-built <see cref="IXmlSchemaInfo"/> annotation for attributes validated against a
    /// named simple type (the <c>[xsl:]type</c> path, where no attribute declaration exists).
    /// </summary>
    private sealed class SimpleTypeSchemaInfo(XmlSchemaSimpleType schemaType, XmlSchemaValidity validity) : IXmlSchemaInfo
    {
        public XmlSchemaValidity Validity => validity;
        public bool IsDefault => false;
        public bool IsNil => false;
        public XmlSchemaSimpleType? MemberType => null;
        public XmlSchemaType SchemaType => schemaType;
        public XmlSchemaElement? SchemaElement => null;
        public XmlSchemaAttribute? SchemaAttribute => null;
    }
}
