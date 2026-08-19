// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 08 June 2026
// PURPOSE              : Provides LSP completion items for XPath functions, axes, keywords and XSLT elements.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 08-06-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bosak.XPath.Standard.Functions;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;

namespace Bosak.LanguageServer;

/// <summary>
/// Provides completion items for XPath functions and XSLT elements.
/// </summary>
public class CompletionHandler : CompletionHandlerBase
{
    private static readonly IReadOnlyList<CompletionItem> XPathFunctions;
    private static readonly IReadOnlyList<CompletionItem> XsltElements;
    private static readonly IReadOnlyList<CompletionItem> XPathAxes;
    private static readonly IReadOnlyList<CompletionItem> XPathKeywords;
    private static readonly IReadOnlyList<CompletionItem> XQueryKeywords;

    static CompletionHandler()
    {
        XPathFunctions = BuildXPathFunctionCompletions();
        XsltElements = BuildXsltElementCompletions();
        XPathAxes = BuildXPathAxisCompletions();
        XPathKeywords = BuildXPathKeywordCompletions();
        XQueryKeywords = BuildXQueryKeywordCompletions();
    }

    /// <summary>
    /// Returns completion items appropriate for the document at the given position.
    /// </summary>
    /// <param name="request">The completion parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of completion items.</returns>
    public override Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        var path = request.TextDocument.Uri.Path?.ToLowerInvariant() ?? string.Empty;
        var items = new List<CompletionItem>();

        if (path.EndsWith(".xsl") || path.EndsWith(".xslt"))
        {
            items.AddRange(XsltElements);
            items.AddRange(XPathFunctions);
            items.AddRange(XPathAxes);
            items.AddRange(XPathKeywords);
        }
        else if (path.EndsWith(".xq") || path.EndsWith(".xqy") || path.EndsWith(".xquery"))
        {
            items.AddRange(XPathFunctions);
            items.AddRange(XPathAxes);
            items.AddRange(XQueryKeywords);
        }
        else if (path.EndsWith(".xpath"))
        {
            items.AddRange(XPathFunctions);
            items.AddRange(XPathAxes);
            items.AddRange(XPathKeywords);
        }

        return Task.FromResult(new CompletionList(items));
    }

    /// <summary>
    /// Resolves additional information for a completion item.
    /// </summary>
    /// <param name="request">The completion item to resolve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The resolved completion item.</returns>
    public override Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request);
    }

    /// <summary>
    /// Creates the registration options used when advertising this handler to the client.
    /// </summary>
    /// <param name="capability">The client's completion capability.</param>
    /// <param name="clientCapabilities">The full client capabilities.</param>
    /// <returns>Registration options for the completion provider.</returns>
    protected override CompletionRegistrationOptions CreateRegistrationOptions(
        CompletionCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new CompletionRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.xsl" },
                new TextDocumentFilter { Pattern = "**/*.xslt" },
                new TextDocumentFilter { Pattern = "**/*.xpath" },
                new TextDocumentFilter { Pattern = "**/*.xq" },
                new TextDocumentFilter { Pattern = "**/*.xqy" },
                new TextDocumentFilter { Pattern = "**/*.xquery" }
            ),
            TriggerCharacters = new Container<string>("", ":", "$", "("),
        };
    }

    private static List<CompletionItem> BuildXPathFunctionCompletions()
    {
        var items = new List<CompletionItem>();

        // Standard XPath / XQuery functions (fn namespace)
        var fnFunctions = new[]
        {
            ("abs", "numeric?"),
            ("adjust-date-to-timezone", "date?"),
            ("adjust-dateTime-to-timezone", "dateTime?"),
            ("adjust-time-to-timezone", "time?"),
            ("analyze-string", "element"),
            ("available-environment-variables", "xs:string*"),
            ("avg", "atomic?"),
            ("base-uri", "xs:anyURI?"),
            ("boolean", "xs:boolean"),
            ("ceiling", "numeric?"),
            ("codepoint-equal", "xs:boolean?"),
            ("codepoints-to-string", "xs:string"),
            ("collection", "item()*"),
            ("compare", "xs:integer?"),
            ("concat", "xs:string"),
            ("contains", "xs:boolean"),
            ("count", "xs:integer"),
            ("current-date", "xs:date"),
            ("current-dateTime", "xs:dateTime"),
            ("current-time", "xs:time"),
            ("data", "atomic*"),
            ("dateTime", "xs:dateTime?"),
            ("day-from-date", "xs:integer?"),
            ("day-from-dateTime", "xs:integer?"),
            ("deep-equal", "xs:boolean"),
            ("default-collation", "xs:string"),
            ("distinct-values", "atomic*"),
            ("doc", "document-node()?"),
            ("doc-available", "xs:boolean"),
            ("element-with-id", "element*"),
            ("empty", "xs:boolean"),
            ("encode-for-uri", "xs:string"),
            ("ends-with", "xs:boolean"),
            ("environment-variable", "xs:string?"),
            ("error", "none"),
            ("escape-html-uri", "xs:string"),
            ("exactly-one", "item"),
            ("exists", "xs:boolean"),
            ("false", "xs:boolean"),
            ("floor", "numeric?"),
            ("format-date", "xs:string?"),
            ("format-dateTime", "xs:string?"),
            ("format-integer", "xs:string"),
            ("format-number", "xs:string"),
            ("format-time", "xs:string?"),
            ("function-lookup", "function?"),
            ("generate-id", "xs:string"),
            ("has-children", "xs:boolean"),
            ("head", "item()?"),
            ("hours-from-dateTime", "xs:integer?"),
            ("hours-from-time", "xs:integer?"),
            ("id", "element*"),
            ("idref", "node*"),
            ("implicit-timezone", "dayTimeDuration"),
            ("index-of", "xs:integer*"),
            ("innermost", "node*"),
            ("insert-before", "item*"),
            ("iri-to-uri", "xs:string"),
            ("json-doc", "item()?"),
            ("last", "xs:integer"),
            ("local-name", "xs:string"),
            ("local-name-from-QName", "xs:NCName?"),
            ("lower-case", "xs:string"),
            ("matches", "xs:boolean"),
            ("max", "atomic?"),
            ("min", "atomic?"),
            ("minutes-from-dateTime", "xs:integer?"),
            ("minutes-from-time", "xs:integer?"),
            ("month-from-date", "xs:integer?"),
            ("month-from-dateTime", "xs:integer?"),
            ("name", "xs:string"),
            ("namespace-uri", "xs:anyURI"),
            ("namespace-uri-for-prefix", "xs:anyURI?"),
            ("namespace-uri-from-QName", "xs:anyURI?"),
            ("nilled", "xs:boolean?"),
            ("node-name", "QName?"),
            ("normalize-space", "xs:string"),
            ("normalize-unicode", "xs:string"),
            ("not", "xs:boolean"),
            ("number", "xs:double"),
            ("one-or-more", "item+"),
            ("outermost", "node*"),
            ("parse-ietf-date", "dateTime?"),
            ("parse-json", "item()?"),
            ("parse-xml", "document-node()?"),
            ("parse-xml-fragment", "document-node()?"),
            ("position", "xs:integer"),
            ("prefix-from-QName", "xs:NCName?"),
            ("QName", "xs:QName"),
            ("remove", "item*"),
            ("replace", "xs:string"),
            ("resolve-QName", "xs:QName?"),
            ("resolve-uri", "xs:anyURI?"),
            ("reverse", "item*"),
            ("root", "node()?"),
            ("round", "numeric?"),
            ("round-half-to-even", "numeric?"),
            ("seconds-from-dateTime", "xs:decimal?"),
            ("seconds-from-time", "xs:decimal?"),
            ("serialize", "xs:string"),
            ("sort", "item*"),
            ("starts-with", "xs:boolean"),
            ("static-base-uri", "xs:anyURI?"),
            ("string", "xs:string"),
            ("string-join", "xs:string"),
            ("string-length", "xs:integer"),
            ("string-to-codepoints", "xs:integer*"),
            ("subsequence", "item*"),
            ("substring", "xs:string"),
            ("substring-after", "xs:string"),
            ("substring-before", "xs:string"),
            ("sum", "atomic?"),
            ("tail", "item*"),
            ("timezone-from-date", "dayTimeDuration?"),
            ("timezone-from-dateTime", "dayTimeDuration?"),
            ("timezone-from-time", "dayTimeDuration?"),
            ("tokenize", "xs:string*"),
            ("trace", "item*"),
            ("translate", "xs:string"),
            ("true", "xs:boolean"),
            ("unordered", "item*"),
            ("upper-case", "xs:string"),
            ("uri-collection", "xs:anyURI*"),
            ("year-from-date", "xs:integer?"),
            ("year-from-dateTime", "xs:integer?"),
            ("years-from-duration", "xs:integer?"),
            ("zero-or-one", "item?"),
        };

        foreach (var (name, ret) in fnFunctions)
        {
            items.Add(new CompletionItem
            {
                Label = name,
                Kind = CompletionItemKind.Function,
                Detail = $"fn:{name}(...) as {ret}",
                InsertText = $"{name}($0)",
                InsertTextFormat = InsertTextFormat.Snippet,
                Documentation = new MarkupContent { Kind = MarkupKind.Markdown, Value = $"`fn:{name}()` — XPath 3.1 standard function." },
            });
        }

        return items;
    }

    private static List<CompletionItem> BuildXsltElementCompletions()
    {
        var elements = new[]
        {
            "xsl:analyze-string", "xsl:apply-imports", "xsl:apply-templates",
            "xsl:assert", "xsl:attribute", "xsl:attribute-set",
            "xsl:break", "xsl:call-template", "xsl:catch",
            "xsl:character-map", "xsl:choose", "xsl:comment",
            "xsl:copy", "xsl:copy-of", "xsl:decimal-format",
            "xsl:document", "xsl:element", "xsl:evaluate",
            "xsl:expose", "xsl:fallback", "xsl:for-each",
            "xsl:for-each-group", "xsl:function", "xsl:global-context-item",
            "xsl:if", "xsl:import", "xsl:import-schema",
            "xsl:include", "xsl:iterate", "xsl:key",
            "xsl:map", "xsl:map-entry", "xsl:matching-substring",
            "xsl:merge", "xsl:merge-action", "xsl:merge-key",
            "xsl:merge-source", "xsl:message", "xsl:mode",
            "xsl:namespace", "xsl:namespace-alias", "xsl:next-iteration",
            "xsl:next-match", "xsl:non-matching-substring", "xsl:number",
            "xsl:on-completion", "xsl:otherwise", "xsl:output",
            "xsl:override", "xsl:package", "xsl:param",
            "xsl:perform-sort", "xsl:preserve-space", "xsl:processing-instruction",
            "xsl:result-document", "xsl:sequence", "xsl:sort",
            "xsl:source-document", "xsl:strip-space", "xsl:stylesheet",
            "xsl:template", "xsl:text", "xsl:transform",
            "xsl:try", "xsl:value-of", "xsl:variable",
            "xsl:when", "xsl:where-populated", "xsl:with-param",
        };

        return elements.Select(e => new CompletionItem
        {
            Label = e,
            Kind = CompletionItemKind.Keyword,
            Detail = "XSLT 3.0 instruction",
            InsertText = $"{e} $0 />",
            InsertTextFormat = InsertTextFormat.Snippet,
            Documentation = new MarkupContent { Kind = MarkupKind.Markdown, Value = $"`<{e}>` — XSLT 3.0 instruction." },
        }).ToList();
    }

    private static List<CompletionItem> BuildXPathAxisCompletions()
    {
        var axes = new[]
        {
            "ancestor", "ancestor-or-self", "attribute", "child",
            "descendant", "descendant-or-self", "following",
            "following-sibling", "namespace", "parent",
            "preceding", "preceding-sibling", "self",
        };

        return axes.Select(a => new CompletionItem
        {
            Label = $"{a}::",
            Kind = CompletionItemKind.Keyword,
            Detail = "XPath axis",
            InsertText = $"{a}::",
            Documentation = new MarkupContent { Kind = MarkupKind.Markdown, Value = $"`{a}::` — XPath axis." },
        }).ToList();
    }

    private static List<CompletionItem> BuildXPathKeywordCompletions()
    {
        var keywords = new[]
        {
            "and", "or", "div", "idiv", "mod", "union",
            "intersect", "except", "to", "is", "eq", "ne",
            "lt", "le", "gt", "ge", "for", "let", "some",
            "every", "in", "return", "if", "then", "else",
            "satisfies", "instance", "of", "treat", "as",
            "castable", "cast", "switch", "case", "default",
        };

        return keywords.Select(k => new CompletionItem
        {
            Label = k,
            Kind = CompletionItemKind.Keyword,
            Detail = "XPath keyword",
            InsertText = k,
        }).ToList();
    }

    private static List<CompletionItem> BuildXQueryKeywordCompletions()
    {
        var keywords = new[]
        {
            "declare", "xquery", "version", "encoding", "module", "namespace",
            "import", "schema", "default", "element", "function", "variable",
            "option", "collation", "base-uri", "boundary-space", "construction",
            "copy-namespaces", "decimal-format", "empty", "order", "ordering",
            "external", "for", "let", "where", "group", "by", "order",
            "stable", "ascending", "descending", "greatest", "least", "some",
            "every", "satisfies", "return", "if", "then", "else", "switch",
            "case", "typeswitch", "try", "catch", "allowing", "count", "at",
            "in", "as", "instance", "of", "cast", "castable", "treat",
            "validate", "lax", "strict", "skip", "tumbling", "sliding",
            "window", "start", "end", "when", "previous", "next", "current",
            // Constructors
            "element", "attribute", "document", "text", "comment",
            "processing-instruction", "namespace",
        };

        return keywords.Select(k => new CompletionItem
        {
            Label = k,
            Kind = CompletionItemKind.Keyword,
            Detail = "XQuery keyword",
            InsertText = k,
        }).ToList();
    }
}
