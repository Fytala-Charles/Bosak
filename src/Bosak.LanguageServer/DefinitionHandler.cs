// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 August 2026
// PURPOSE              : Provides LSP go-to-definition for XSLT user-defined functions, variables, parameters, and templates.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 18-08-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Bosak.LanguageServer;

/// <summary>
/// Provides go-to-definition for user-defined XSLT symbols: xsl:function, xsl:variable,
/// xsl:param, and named xsl:template. Resolves function calls, variable references, and
/// call-template references to their declarations.
/// </summary>
public class DefinitionHandler : DefinitionHandlerBase
{
    private const string XslNamespace = "http://www.w3.org/1999/XSL/Transform";

    private readonly DocumentManager _documents;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefinitionHandler"/> class.
    /// </summary>
    /// <param name="documents">The document manager holding open document contents.</param>
    public DefinitionHandler(DocumentManager documents)
    {
        _documents = documents;
    }

    /// <summary>
    /// Returns the definition location for the symbol at the cursor position.
    /// </summary>
    /// <param name="request">The definition parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The definition location(s), or an empty result when none is found.</returns>
    public override Task<LocationOrLocationLinks?> Handle(DefinitionParams request, CancellationToken cancellationToken)
    {
        if (!_documents.TryGet(request.TextDocument.Uri.ToString(), out var text))
            return Task.FromResult<LocationOrLocationLinks?>(null);

        var path = request.TextDocument.Uri.Path?.ToLowerInvariant() ?? string.Empty;
        bool isXslt = path.EndsWith(".xsl") || path.EndsWith(".xslt");
        bool isXQuery = path.EndsWith(".xq") || path.EndsWith(".xqy") || path.EndsWith(".xquery");
        if (!isXslt && !isXQuery)
            return Task.FromResult<LocationOrLocationLinks?>(null);

        var word = HoverHandler.GetWordAt(text, request.Position.Line, request.Position.Character);
        if (string.IsNullOrEmpty(word))
            return Task.FromResult<LocationOrLocationLinks?>(null);

        var location = isXQuery
            ? FindXQueryDefinition(text, word, request.TextDocument.Uri)
            : FindDefinition(text, word, request.TextDocument.Uri);
        if (location is null)
            return Task.FromResult<LocationOrLocationLinks?>(null);

        return Task.FromResult<LocationOrLocationLinks?>(new LocationOrLocationLinks(location));
    }

    /// <summary>
    /// Finds the definition location of a function or variable in an XQuery document by
    /// scanning for <c>declare function name(</c> and <c>declare variable $name</c>.
    /// </summary>
    internal static Location? FindXQueryDefinition(string text, string word, DocumentUri uri)
    {
        // Variable reference ($name) — search declare variable $name.
        if (word.StartsWith("$", StringComparison.Ordinal))
        {
            var varName = word[1..];
            var match = System.Text.RegularExpressions.Regex.Match(
                text, @"declare\s+variable\s+\$" + System.Text.RegularExpressions.Regex.Escape(varName) + @"\b");
            if (match.Success)
                return OffsetToLocation(text, match.Index, uri);
            return null;
        }

        // Function call — search declare function name(.
        var fnMatch = System.Text.RegularExpressions.Regex.Match(
            text, @"declare\s+function\s+" + System.Text.RegularExpressions.Regex.Escape(word) + @"\s*\(");
        if (fnMatch.Success)
            return OffsetToLocation(text, fnMatch.Index, uri);
        return null;
    }

    private static Location OffsetToLocation(string text, int offset, DocumentUri uri)
    {
        int line = 0, col = 0;
        for (int i = 0; i < offset && i < text.Length; i++)
        {
            if (text[i] == '\n') { line++; col = 0; }
            else col++;
        }
        return new Location
        {
            Uri = uri,
            Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                new Position(line, col), new Position(line, col + 1))
        };
    }

    /// <summary>
    /// Finds the definition location of a symbol in an XSLT document.
    /// </summary>
    internal static Location? FindDefinition(string text, string word, DocumentUri uri)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Parse(text, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        }
        catch (XmlException)
        {
            return null;
        }

        if (doc.Root is null)
            return null;

        // Variable / parameter reference ($name).
        if (word.StartsWith("$", StringComparison.Ordinal))
        {
            var name = word[1..];
            foreach (var el in doc.Descendants())
            {
                if (el.Name.NamespaceName == XslNamespace &&
                    (el.Name.LocalName == "variable" || el.Name.LocalName == "param") &&
                    el.Attribute("name")?.Value == name)
                {
                    return ToLocation(el, uri);
                }
            }
            return null;
        }

        // call-template / function call / named template reference.
        foreach (var el in doc.Descendants())
        {
            if (el.Name.NamespaceName != XslNamespace)
                continue;
            var local = el.Name.LocalName;
            if ((local == "function" || local == "template") && el.Attribute("name")?.Value == word)
                return ToLocation(el, uri);
        }

        return null;
    }

    private static Location ToLocation(XElement element, DocumentUri uri)
    {
        var lineInfo = (IXmlLineInfo)element;
        var line = Math.Max(0, lineInfo.LineNumber - 1);
        var col = Math.Max(0, lineInfo.LinePosition - 1);
        return new Location
        {
            Uri = uri,
            Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                new Position(line, col), new Position(line, col + 1))
        };
    }

    /// <summary>
    /// Creates the registration options used when advertising this handler to the client.
    /// </summary>
    /// <param name="capability">The client's definition capability.</param>
    /// <param name="clientCapabilities">The full client capabilities.</param>
    /// <returns>Registration options for the definition provider.</returns>
    protected override DefinitionRegistrationOptions CreateRegistrationOptions(
        DefinitionCapability capability, ClientCapabilities clientCapabilities)
    {
        return new DefinitionRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.xsl" },
                new TextDocumentFilter { Pattern = "**/*.xslt" },
                new TextDocumentFilter { Pattern = "**/*.xq" },
                new TextDocumentFilter { Pattern = "**/*.xqy" },
                new TextDocumentFilter { Pattern = "**/*.xquery" }
            ),
        };
    }
}
