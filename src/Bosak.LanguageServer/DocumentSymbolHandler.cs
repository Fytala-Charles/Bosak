// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 August 2026
// PURPOSE              : Provides LSP document symbols (outline) for XSLT stylesheets.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 18-08-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 31-08-2026     | Added comprehensive tests for richer XSLT document symbols (REQ-073)                    |
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
/// Provides document symbols (the outline view) for XSLT stylesheets: templates,
/// functions, global variables/parameters, keys, decimal formats, character maps,
/// accumulators, modes, attribute sets, and imports/includes.
/// </summary>
public class DocumentSymbolHandler : DocumentSymbolHandlerBase
{
    private const string XslNamespace = "http://www.w3.org/1999/XSL/Transform";

    private readonly DocumentManager _documents;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentSymbolHandler"/> class.
    /// </summary>
    /// <param name="documents">The document manager holding open document contents.</param>
    public DocumentSymbolHandler(DocumentManager documents)
    {
        _documents = documents;
    }

    /// <summary>
    /// Returns the symbols for the requested document.
    /// </summary>
    /// <param name="request">The document symbol parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A container of symbols or document symbols.</returns>
    public override Task<SymbolInformationOrDocumentSymbolContainer> Handle(
        DocumentSymbolParams request, CancellationToken cancellationToken)
    {
        var symbols = new List<SymbolInformationOrDocumentSymbol>();
        if (!_documents.TryGet(request.TextDocument.Uri.ToString(), out var text))
            return Task.FromResult(new SymbolInformationOrDocumentSymbolContainer(symbols));

        var path = request.TextDocument.Uri.Path?.ToLowerInvariant() ?? string.Empty;
        bool isXslt = path.EndsWith(".xsl") || path.EndsWith(".xslt");
        bool isXQuery = path.EndsWith(".xq") || path.EndsWith(".xqy") || path.EndsWith(".xquery");
        if (!isXslt && !isXQuery)
            return Task.FromResult(new SymbolInformationOrDocumentSymbolContainer(symbols));

        if (isXQuery)
            return Task.FromResult(new SymbolInformationOrDocumentSymbolContainer(GetXQuerySymbols(text)));

        XDocument doc;
        try
        {
            doc = XDocument.Parse(text, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        }
        catch (XmlException)
        {
            return Task.FromResult(new SymbolInformationOrDocumentSymbolContainer(symbols));
        }

        if (doc.Root is null)
            return Task.FromResult(new SymbolInformationOrDocumentSymbolContainer(symbols));

        foreach (var element in doc.Root.Elements().Where(e => e.Name.NamespaceName == XslNamespace))
        {
            var symbol = CreateSymbol(element);
            if (symbol is not null)
                symbols.Add(symbol);
        }

        return Task.FromResult(new SymbolInformationOrDocumentSymbolContainer(symbols));
    }

    /// <summary>
    /// Scans XQuery source text for top-level declarations (module namespace, imports,
    /// functions, variables) and produces outline symbols.
    /// </summary>
    private static List<SymbolInformationOrDocumentSymbol> GetXQuerySymbols(string text)
    {
        var symbols = new List<SymbolInformationOrDocumentSymbol>();
        var declarations = System.Text.RegularExpressions.Regex.Matches(
            text,
            @"^\s*(module\s+namespace|import\s+module|declare\s+function|declare\s+variable|declare\s+namespace|declare\s+default\s+element\s+namespace|declare\s+default\s+collation|declare\s+option)\s+(\$?[a-zA-Z_][a-zA-Z0-9._-]*(?::[a-zA-Z_][a-zA-Z0-9._-]*)?)",
            System.Text.RegularExpressions.RegexOptions.Multiline);
        foreach (System.Text.RegularExpressions.Match m in declarations)
        {
            var kind = m.Groups[1].Value;
            var name = m.Groups[2].Value;
            var display = kind.StartsWith("declare function", StringComparison.Ordinal) ? $"function {name}"
                : kind.StartsWith("declare variable", StringComparison.Ordinal) ? $"variable {name}"
                : kind.StartsWith("module namespace", StringComparison.Ordinal) ? $"module {name}"
                : kind.StartsWith("import module", StringComparison.Ordinal) ? $"import {name}"
                : name;
            var symbolKind = kind.StartsWith("declare function", StringComparison.Ordinal) ? SymbolKind.Function
                : kind.StartsWith("declare variable", StringComparison.Ordinal) ? SymbolKind.Variable
                : kind.StartsWith("module namespace", StringComparison.Ordinal) ? SymbolKind.Module
                : kind.StartsWith("import module", StringComparison.Ordinal) ? SymbolKind.Package
                : SymbolKind.Property;
            symbols.Add(new SymbolInformationOrDocumentSymbol(new DocumentSymbol
            {
                Name = display,
                Kind = symbolKind,
                Range = OffsetToRange(text, m.Index),
                SelectionRange = OffsetToRange(text, m.Index),
            }));
        }
        return symbols;
    }

    private static OmniSharp.Extensions.LanguageServer.Protocol.Models.Range OffsetToRange(string text, int offset)
    {
        int line = 0, col = 0;
        for (int i = 0; i < offset && i < text.Length; i++)
        {
            if (text[i] == '\n') { line++; col = 0; }
            else col++;
        }
        return new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
            new Position(line, col), new Position(line, col + 1));
    }

    private static SymbolInformationOrDocumentSymbol? CreateSymbol(XElement element)
    {
        var localName = element.Name.LocalName;
        var lineInfo = (IXmlLineInfo)element;
        var range = ToRange(lineInfo);

        switch (localName)
        {
            case "template":
            {
                var name = element.Attribute("name")?.Value;
                var match = element.Attribute("match")?.Value;
                var display = name is not null
                    ? $"template {name}"
                    : match is not null
                        ? $"template match=\"{match}\""
                        : "template";
                return new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                {
                    Name = display,
                    Kind = SymbolKind.Method,
                    Range = range,
                    SelectionRange = range,
                });
            }
            case "function":
            {
                var name = element.Attribute("name")?.Value;
                if (name is null) return null;
                return new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                {
                    Name = $"function {name}",
                    Kind = SymbolKind.Function,
                    Range = range,
                    SelectionRange = range,
                });
            }
            case "variable":
            case "param":
            {
                var name = element.Attribute("name")?.Value;
                if (name is null) return null;
                return new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                {
                    Name = name,
                    Kind = localName == "param" ? SymbolKind.Variable : SymbolKind.Variable,
                    Detail = localName,
                    Range = range,
                    SelectionRange = range,
                });
            }
            case "key":
            {
                var name = element.Attribute("name")?.Value;
                if (name is null) return null;
                return new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                {
                    Name = $"key {name}",
                    Kind = SymbolKind.Key,
                    Range = range,
                    SelectionRange = range,
                });
            }
            case "decimal-format":
            {
                var name = element.Attribute("name")?.Value ?? "(default)";
                return new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                {
                    Name = $"decimal-format {name}",
                    Kind = SymbolKind.Number,
                    Range = range,
                    SelectionRange = range,
                });
            }
            case "character-map":
            {
                var name = element.Attribute("name")?.Value;
                if (name is null) return null;
                return new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                {
                    Name = $"character-map {name}",
                    Kind = SymbolKind.String,
                    Range = range,
                    SelectionRange = range,
                });
            }
            case "accumulator":
            {
                var name = element.Attribute("name")?.Value;
                if (name is null) return null;
                return new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                {
                    Name = $"accumulator {name}",
                    Kind = SymbolKind.Object,
                    Range = range,
                    SelectionRange = range,
                });
            }
            case "mode":
            {
                var name = element.Attribute("name")?.Value ?? "(default)";
                return new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                {
                    Name = $"mode {name}",
                    Kind = SymbolKind.Enum,
                    Range = range,
                    SelectionRange = range,
                });
            }
            case "attribute-set":
            {
                var name = element.Attribute("name")?.Value;
                if (name is null) return null;
                return new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                {
                    Name = $"attribute-set {name}",
                    Kind = SymbolKind.Property,
                    Range = range,
                    SelectionRange = range,
                });
            }
            case "output":
            {
                var method = element.Attribute("method")?.Value;
                return new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                {
                    Name = method is not null ? $"output ({method})" : "output",
                    Detail = "xsl:output",
                    Kind = SymbolKind.Interface,
                    Range = range,
                    SelectionRange = range,
                });
            }
            case "import":
            case "include":
            {
                var href = element.Attribute("href")?.Value;
                return new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                {
                    Name = href is not null ? $"{localName} {href}" : localName,
                    Kind = SymbolKind.Package,
                    Range = range,
                    SelectionRange = range,
                });
            }
            default:
                return null;
        }
    }

    private static OmniSharp.Extensions.LanguageServer.Protocol.Models.Range ToRange(IXmlLineInfo lineInfo)
    {
        var line = Math.Max(0, lineInfo.LineNumber - 1);
        var col = Math.Max(0, lineInfo.LinePosition - 1);
        return new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
            new Position(line, col), new Position(line, col + 1));
    }

    /// <summary>
    /// Creates the registration options used when advertising this handler to the client.
    /// </summary>
    /// <param name="capability">The client's document symbol capability.</param>
    /// <param name="clientCapabilities">The full client capabilities.</param>
    /// <returns>Registration options for the document symbol provider.</returns>
    protected override DocumentSymbolRegistrationOptions CreateRegistrationOptions(
        DocumentSymbolCapability capability, ClientCapabilities clientCapabilities)
    {
        return new DocumentSymbolRegistrationOptions
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
