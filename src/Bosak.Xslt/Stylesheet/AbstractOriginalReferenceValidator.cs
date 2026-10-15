// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 October 2026
// PURPOSE              : Validates static original references in abstract function overrides.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 15-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Parser.Lexer;
using Bosak.Xslt.Authoring;

namespace Bosak.Xslt.Stylesheet;

/// <summary>Rejects static function references to abstract originals before evaluation.</summary>
internal static class AbstractOriginalReferenceValidator
{
    /// <summary>Checks expressions and template interpolations in an overriding function.</summary>
    /// <param name="function">The concrete overriding declaration.</param>
    internal static void Validate(XElement function)
    {
        foreach (var element in function.DescendantsAndSelf())
        {
            foreach (var attribute in element.Attributes())
            {
                if (attribute.IsNamespaceDeclaration) continue;
                var kind = AttributeSlotClassifier.Classify(element.Name.NamespaceName, element.Name.LocalName,
                    attribute.Name.LocalName, attribute.Name.NamespaceName, attribute.Value);
                // Every literal result attribute outside the XSLT namespace is an AVT,
                // including attributes in an application namespace.
                if (element.Name.NamespaceName != Stylesheet.XslNamespace && attribute.Name.NamespaceName != Stylesheet.XslNamespace)
                    kind = AuthoringAttributeSlotKind.Avt;
                if (element.Name.NamespaceName == Stylesheet.XslNamespace && attribute.Name.NamespaceName.Length == 0
                    && IsAdditionalAvt(element.Name.LocalName, attribute.Name.LocalName))
                    kind = AuthoringAttributeSlotKind.Avt;
                if (kind is AuthoringAttributeSlotKind.Expression or AuthoringAttributeSlotKind.Pattern)
                    CheckExpression(attribute.Value.AsSpan(), element);
                else if (kind == AuthoringAttributeSlotKind.Avt)
                    CheckTemplate(attribute.Value.AsSpan(), element);
            }
            var expand = element.AncestorsAndSelf().Select(e => e.Name.NamespaceName == Stylesheet.XslNamespace ? e.Attribute("expand-text") : e.Attribute(XName.Get("expand-text", Stylesheet.XslNamespace))).FirstOrDefault(a => a != null)?.Value;
            if (expand is "yes" or "true" or "1")
                foreach (var text in element.Nodes().OfType<XText>()) CheckTemplate(text.Value.AsSpan(), element);
        }
    }

    private static bool IsAdditionalAvt(string element, string attribute) => (element, attribute) switch
    {
        ("element" or "attribute", "namespace") => true,
        ("value-of" or "attribute" or "for-each" or "for-each-group" or "apply-templates" or "call-template", "separator") => true,
        ("sort", "order" or "data-type" or "case-order" or "collation" or "stable") => true,
        ("number", "letter-value" or "start-at") => true,
        ("message", "terminate" or "error-code") => true,
        ("evaluate", "base-uri" or "schema-aware") => true,
        ("analyze-string", "regex" or "flags") => true,
        ("result-document", _) => true,
        _ => false
    };

    private static void CheckExpression(ReadOnlySpan<char> expression, XElement context)
    {
        var lexer = new XPathLexer(expression, xpath40: true);
        var previous = TokenKind.Eof;
        bool original = false;
        for (var token = lexer.NextToken(); token.Kind != TokenKind.Eof; token = lexer.NextToken())
        {
            if (original && token.Kind is TokenKind.LParen or TokenKind.Hash)
                throw new InvalidOperationException("XTSE3075: An abstract overridden function cannot be referenced using xsl:original.");
            original = token.Kind == TokenKind.Name && previous != TokenKind.Dollar && IsOriginal(token.Text(expression), context);
            if (token.Kind == TokenKind.StringTemplate)
                CheckTemplate(token.Text(expression)[1..^1], context);
            previous = token.Kind;
        }
    }

    private static bool IsOriginal(ReadOnlySpan<char> name, XElement context)
    {
        if (name.StartsWith("Q{"))
            return name.SequenceEqual("Q{" + Stylesheet.XslNamespace + "}original");
        var colon = name.IndexOf(':');
        return colon > 0 && name[(colon + 1)..].SequenceEqual("original")
            && context.GetNamespaceOfPrefix(name[..colon].ToString())?.NamespaceName == Stylesheet.XslNamespace;
    }

    private static void CheckTemplate(ReadOnlySpan<char> template, XElement context)
    {
        for (int i = 0; i < template.Length; i++)
        {
            if (template[i] != '{') continue;
            if (i + 1 < template.Length && template[i + 1] == '{') { i++; continue; }
            var start = i + 1;
            var lexer = new XPathLexer(template[start..], xpath40: true);
            int depth = 1;
            for (var token = lexer.NextToken(); token.Kind != TokenKind.Eof; token = lexer.NextToken())
            {
                if (token.Kind == TokenKind.LBrace) depth++;
                if (token.Kind != TokenKind.RBrace) continue;
                depth--;
                if (depth == 0)
                {
                    CheckExpression(template.Slice(start, token.Start), context);
                    i = start + token.Start;
                    break;
                }
            }
        }
    }
}
