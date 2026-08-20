// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 August 2026
// PURPOSE              : Provides LSP semantic tokens for XPath, XQuery, and XSLT documents.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-08-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Bosak.LanguageServer;

/// <summary>
/// Provides semantic tokens for XPath, XQuery, and XSLT documents.
/// Highlights function names, variables, XSLT instructions, XQuery keywords,
/// namespace prefixes, and type names.
/// </summary>
public class SemanticTokensHandler : SemanticTokensHandlerBase
{
    private static readonly Regex FunctionCallRegex = new(
        @"(?<prefix>[a-zA-Z_][a-zA-Z0-9._-]*):(?<local>[a-zA-Z_][a-zA-Z0-9._-]*)\s*\(",
        RegexOptions.Compiled);

    private static readonly Regex VariableRegex = new(
        @"\$(?<name>[a-zA-Z_][a-zA-Z0-9._-]*(?::[a-zA-Z_][a-zA-Z0-9._-]*)?)",
        RegexOptions.Compiled);

    private static readonly Regex XsltInstructionRegex = new(
        @"</?(?<prefix>xsl):(?<local>[a-zA-Z_][a-zA-Z0-9._-]*)",
        RegexOptions.Compiled);

    private static readonly Regex XQueryKeywordRegex = new(
        @"\b(for|let|where|order\s+by|group\s+by|count|return|declare|import|module|namespace|schema|element|attribute|document|text|comment|processing-instruction|switch|typeswitch|try|catch|if|then|else|some|every|in|as|at|ascending|descending|stable|unordered|empty|collation|boundary-space|base-uri|default|function|variable|option|ordering)\b",
        RegexOptions.Compiled);

    private static readonly Regex TypeNameRegex = new(
        @"\b(xs|xdt):(?<local>[a-zA-Z_][a-zA-Z0-9._-]*)\b",
        RegexOptions.Compiled);

    private static readonly Regex NumberLiteralRegex = new(
        @"(?<![\w.])-?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?(?![\w.])",
        RegexOptions.Compiled);

    private readonly DocumentManager _documents;

    /// <summary>
    /// Initializes a new instance of the <see cref="SemanticTokensHandler"/> class.
    /// </summary>
    /// <param name="documents">The document manager holding open document contents.</param>
    public SemanticTokensHandler(DocumentManager documents)
    {
        _documents = documents;
    }

    /// <summary>
    /// Creates the registration options used when advertising this handler to the client.
    /// </summary>
    /// <param name="capability">The client's semantic tokens capability.</param>
    /// <param name="clientCapabilities">The full client capabilities.</param>
    /// <returns>Registration options for the semantic tokens provider.</returns>
    protected override SemanticTokensRegistrationOptions CreateRegistrationOptions(
        SemanticTokensCapability capability, ClientCapabilities clientCapabilities)
    {
        return new SemanticTokensRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.xsl" },
                new TextDocumentFilter { Pattern = "**/*.xslt" },
                new TextDocumentFilter { Pattern = "**/*.xpath" },
                new TextDocumentFilter { Pattern = "**/*.xq" },
                new TextDocumentFilter { Pattern = "**/*.xqy" },
                new TextDocumentFilter { Pattern = "**/*.xquery" }
            ),
            Legend = new SemanticTokensLegend
            {
                TokenTypes = new Container<SemanticTokenType>(
                    SemanticTokenType.Function,
                    SemanticTokenType.Variable,
                    SemanticTokenType.Keyword,
                    SemanticTokenType.Type,
                    SemanticTokenType.Namespace,
                    SemanticTokenType.Number,
                    SemanticTokenType.Operator),
                TokenModifiers = new Container<SemanticTokenModifier>(
                    SemanticTokenModifier.Declaration,
                    SemanticTokenModifier.Definition,
                    SemanticTokenModifier.Readonly),
            },
            Full = new SemanticTokensCapabilityRequestFull { Delta = false },
            Range = true,
        };
    }

    /// <summary>
    /// Returns the semantic tokens document used to create builders for the given request.
    /// </summary>
    protected override Task<SemanticTokensDocument> GetSemanticTokensDocument(
        ITextDocumentIdentifierParams @params, CancellationToken cancellationToken)
    {
        var options = CreateRegistrationOptions(new SemanticTokensCapability(), new ClientCapabilities());
        return Task.FromResult(new SemanticTokensDocument(options));
    }

    /// <summary>
    /// Tokenizes the document and pushes tokens onto the builder.
    /// </summary>
    protected override Task Tokenize(
        SemanticTokensBuilder builder, ITextDocumentIdentifierParams identifier, CancellationToken cancellationToken)
    {
        if (!_documents.TryGet(identifier.TextDocument.Uri.ToString(), out var text))
            return Task.CompletedTask;

        var path = identifier.TextDocument.Uri.Path?.ToLowerInvariant() ?? string.Empty;
        bool isXslt = path.EndsWith(".xsl") || path.EndsWith(".xslt");
        bool isXQuery = path.EndsWith(".xq") || path.EndsWith(".xqy") || path.EndsWith(".xquery");
        bool isXPath = path.EndsWith(".xpath") || isXQuery || isXslt;

        var tokens = Tokenize(text, isXslt, isXQuery, isXPath);

        foreach (var token in tokens)
        {
            var (line, col) = GetLineColumn(text, token.Start);
            builder.Push(line, col, token.Length, token.Type);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Tokenizes the document text into a list of semantic token entries.
    /// </summary>
    internal static List<SemanticTokenEntry> Tokenize(string text, bool isXslt, bool isXQuery, bool isXPath)
    {
        var tokens = new List<SemanticTokenEntry>();

        foreach (Match m in FunctionCallRegex.Matches(text))
        {
            var prefix = m.Groups["prefix"];
            var local = m.Groups["local"];
            tokens.Add(new SemanticTokenEntry(prefix.Index, prefix.Length, SemanticTokenType.Namespace));
            tokens.Add(new SemanticTokenEntry(local.Index, local.Length, SemanticTokenType.Function));
        }

        foreach (Match m in VariableRegex.Matches(text))
        {
            tokens.Add(new SemanticTokenEntry(m.Groups["name"].Index - 1, m.Length, SemanticTokenType.Variable));
        }

        if (isXslt)
        {
            foreach (Match m in XsltInstructionRegex.Matches(text))
            {
                tokens.Add(new SemanticTokenEntry(m.Groups["prefix"].Index, m.Groups["prefix"].Length, SemanticTokenType.Namespace));
                tokens.Add(new SemanticTokenEntry(m.Groups["local"].Index, m.Groups["local"].Length, SemanticTokenType.Keyword));
            }
        }

        if (isXQuery || isXPath)
        {
            foreach (Match m in XQueryKeywordRegex.Matches(text))
            {
                tokens.Add(new SemanticTokenEntry(m.Index, m.Length, SemanticTokenType.Keyword));
            }
        }

        foreach (Match m in TypeNameRegex.Matches(text))
        {
            tokens.Add(new SemanticTokenEntry(m.Groups["local"].Index, m.Groups["local"].Length, SemanticTokenType.Type));
        }

        foreach (Match m in NumberLiteralRegex.Matches(text))
        {
            tokens.Add(new SemanticTokenEntry(m.Index, m.Length, SemanticTokenType.Number));
        }

        if (isXPath)
        {
            foreach (Match m in AxisRegex.Matches(text))
            {
                tokens.Add(new SemanticTokenEntry(m.Index, m.Length, SemanticTokenType.Operator));
            }
        }

        tokens.Sort((a, b) =>
        {
            var (lineA, colA) = GetLineColumn(text, a.Start);
            var (lineB, colB) = GetLineColumn(text, b.Start);
            int cmp = lineA.CompareTo(lineB);
            return cmp != 0 ? cmp : colA.CompareTo(colB);
        });

        return tokens;
    }

    private static readonly Regex AxisRegex = new(
        @"::|@|//|/|\\||!|=|!=|<=|>=|<|>|\+|\-|\*|\band\b|\bor\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Converts a zero-based text offset to a (line, column) pair.
    /// </summary>
    internal static (int Line, int Column) GetLineColumn(string text, int offset)
    {
        int line = 0;
        int column = 0;
        for (int i = 0; i < offset && i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                line++;
                column = 0;
            }
            else
            {
                column++;
            }
        }
        return (line, column);
    }
}

/// <summary>
/// A single semantic token entry before LSP encoding.
/// </summary>
internal readonly record struct SemanticTokenEntry(int Start, int Length, string Type);
