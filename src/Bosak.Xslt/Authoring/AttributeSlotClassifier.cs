// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Data-driven classification of attribute value roles for XSLT elements.
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

namespace Bosak.Xslt.Authoring;

/// <summary>
/// Classifies an attribute's role from its owning element and raw value. The tables are internal and
/// data-driven so they can grow with the vocabulary; any combination not listed resolves to
/// <see cref="AuthoringAttributeSlotKind.Plain"/> and never throws.
/// </summary>
internal static class AttributeSlotClassifier
{
    internal const string XsltNamespace = "http://www.w3.org/1999/XSL/Transform";

    // Attributes in the XSLT namespace whose role is the same on every instruction that accepts them.
    private static readonly Dictionary<string, AuthoringAttributeSlotKind> XsltUniversalSlots = new(StringComparer.Ordinal)
    {
        ["select"] = AuthoringAttributeSlotKind.Expression,
        ["test"] = AuthoringAttributeSlotKind.Expression,
        ["use-when"] = AuthoringAttributeSlotKind.Expression,
        ["group-by"] = AuthoringAttributeSlotKind.Expression,
        ["count"] = AuthoringAttributeSlotKind.Expression,
        ["from"] = AuthoringAttributeSlotKind.Expression,
        ["value"] = AuthoringAttributeSlotKind.Expression,
        ["use"] = AuthoringAttributeSlotKind.Expression,
        ["initial-value"] = AuthoringAttributeSlotKind.Expression,
        ["match"] = AuthoringAttributeSlotKind.Pattern,
    };

    // (owning element local name, attribute local name) pairs with a specific role.
    private static readonly Dictionary<(string Element, string Attribute), AuthoringAttributeSlotKind> XsltSpecificSlots = new()
    {
        [("element", "name")] = AuthoringAttributeSlotKind.Avt,
        [("attribute", "name")] = AuthoringAttributeSlotKind.Avt,
        [("processing-instruction", "name")] = AuthoringAttributeSlotKind.Avt,
        [("namespace", "name")] = AuthoringAttributeSlotKind.Avt,
        [("result-document", "href")] = AuthoringAttributeSlotKind.Avt,
        [("result-document", "format")] = AuthoringAttributeSlotKind.Avt,
        [("result-document", "output-version")] = AuthoringAttributeSlotKind.Avt,
        [("number", "format")] = AuthoringAttributeSlotKind.Avt,
        [("number", "lang")] = AuthoringAttributeSlotKind.Avt,
        [("sort", "lang")] = AuthoringAttributeSlotKind.Avt,
        [("message", "select")] = AuthoringAttributeSlotKind.Expression,
        [("call-template", "name")] = AuthoringAttributeSlotKind.QName,
        [("with-param", "name")] = AuthoringAttributeSlotKind.QName,
        [("param", "name")] = AuthoringAttributeSlotKind.QName,
        [("variable", "name")] = AuthoringAttributeSlotKind.QName,
        [("function", "name")] = AuthoringAttributeSlotKind.QName,
        [("template", "name")] = AuthoringAttributeSlotKind.QName,
        [("attribute-set", "name")] = AuthoringAttributeSlotKind.QName,
        [("mode", "name")] = AuthoringAttributeSlotKind.QName,
        [("key", "name")] = AuthoringAttributeSlotKind.QName,
        [("decimal-format", "name")] = AuthoringAttributeSlotKind.QName,
        [("output", "name")] = AuthoringAttributeSlotKind.QName,
        [("character-map", "name")] = AuthoringAttributeSlotKind.QName,
        [("import-schema", "namespace")] = AuthoringAttributeSlotKind.Plain,
        [("include", "href")] = AuthoringAttributeSlotKind.Plain,
        [("import", "href")] = AuthoringAttributeSlotKind.Plain,
    };

    /// <summary>
    /// Classifies an attribute slot. For elements outside the XSLT namespace, an attribute is an AVT if
    /// and only if its raw literal contains a <c>{</c> (XSLT attribute value template rules for
    /// literal result elements); otherwise it is plain.
    /// </summary>
    /// <param name="owningElementNamespace">The resolved namespace URI of the owning element.</param>
    /// <param name="owningElementLocalName">The local name of the owning element.</param>
    /// <param name="attributeLocalName">The local name of the attribute.</param>
    /// <param name="attributeNamespace">The resolved namespace URI of the attribute.</param>
    /// <param name="rawLiteral">The attribute value exactly as written in source.</param>
    public static AuthoringAttributeSlotKind Classify(
        string owningElementNamespace,
        string owningElementLocalName,
        string attributeLocalName,
        string attributeNamespace,
        string rawLiteral)
    {
        if (owningElementNamespace != XsltNamespace)
        {
            // Literal result element: any attribute in no namespace containing '{' is an AVT.
            if (attributeNamespace.Length == 0 && rawLiteral.Contains('{'))
            {
                return AuthoringAttributeSlotKind.Avt;
            }

            return AuthoringAttributeSlotKind.Plain;
        }

        if (XsltSpecificSlots.TryGetValue((owningElementLocalName, attributeLocalName), out var specific))
        {
            return specific;
        }

        if (XsltUniversalSlots.TryGetValue(attributeLocalName, out var universal))
        {
            return universal;
        }

        // xsl:template/@mode and friends: a single QName, but #all/#current/#default are keywords.
        if (attributeLocalName == "mode" && attributeNamespace.Length == 0)
        {
            return IsQNameToken(rawLiteral) ? AuthoringAttributeSlotKind.QName : AuthoringAttributeSlotKind.Plain;
        }

        return AuthoringAttributeSlotKind.Plain;
    }

    private static bool IsQNameToken(string raw)
    {
        if (raw.Length == 0 || raw[0] == '#')
        {
            return false;
        }

        foreach (var c in raw)
        {
            if (char.IsWhiteSpace(c) || c == '(' || c == '{' || c == '}')
            {
                return false;
            }
        }

        return true;
    }
}
