// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 August 2026
// PURPOSE              : Provides LSP code actions (quick fixes) for XPath, XQuery, and XSLT documents.
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
//                      | Charles Korthout | 0.2   | 20-08-2026     | Fixed namespace/usings and added code action resolve handler                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 20-08-2026     | Added XSLT root prefix rename and missing version quick fixes                            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 20-08-2026     | Added XPST0081 diagnostic-driven namespace declaration action                            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 20-08-2026     | Added XQST0085 remove-empty-namespace-declaration quick fix                              |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 20-08-2026     | Added import module namespace action for XQuery function calls                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 20-08-2026     | Added XPath syntax-error quick fixes for unclosed brackets and strings                  |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 20-08-2026     | Use standard XML namespace URI for xml prefix declarations                              |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Bosak.LanguageServer;

/// <summary>
/// Provides code actions (quick fixes) for XPath, XQuery, and XSLT documents.
/// Currently supports declaring an undeclared namespace prefix, removing an
/// invalid empty namespace declaration in XQuery, adding the XSLT namespace
/// to a malformed stylesheet root, and adding the required XSLT version
/// attribute.
/// </summary>
public class CodeActionHandler : CodeActionHandlerBase
{
    private static readonly Regex PrefixedNameRegex = new(
        @"(?<![a-zA-Z0-9_:\-])\b(?<prefix>[a-zA-Z_][a-zA-Z0-9._-]*):(?<local>[a-zA-Z_][a-zA-Z0-9._-]*)\b",
        RegexOptions.Compiled);

    private readonly DocumentManager _documents;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeActionHandler"/> class.
    /// </summary>
    /// <param name="documents">The document manager holding open document contents.</param>
    public CodeActionHandler(DocumentManager documents)
    {
        _documents = documents;
    }

    /// <summary>
    /// Returns code actions available for the given range and diagnostics.
    /// </summary>
    public override Task<CommandOrCodeActionContainer?> Handle(
        CodeActionParams request, CancellationToken cancellationToken)
    {
        var actions = new List<CommandOrCodeAction>();
        if (!_documents.TryGet(request.TextDocument.Uri.ToString(), out var text))
            return Task.FromResult<CommandOrCodeActionContainer?>(new CommandOrCodeActionContainer(actions));

        var path = request.TextDocument.Uri.Path?.ToLowerInvariant() ?? string.Empty;
        var range = request.Range;
        var diagnostics = request.Context.Diagnostics?.ToList() ?? new List<Diagnostic>();

        if (path.EndsWith(".xq") || path.EndsWith(".xqy") || path.EndsWith(".xquery"))
        {
            actions.AddRange(GetXQueryCodeActions(text, range, request.TextDocument.Uri, diagnostics));
        }
        else if (path.EndsWith(".xsl") || path.EndsWith(".xslt"))
        {
            actions.AddRange(GetXsltCodeActions(text, range, request.TextDocument.Uri, diagnostics));
        }
        else if (path.EndsWith(".xpath"))
        {
            actions.AddRange(GetXPathCodeActions(text, range, request.TextDocument.Uri));
        }

        return Task.FromResult<CommandOrCodeActionContainer?>(new CommandOrCodeActionContainer(actions));
    }

    /// <summary>
    /// Resolves a partially-populated code action by returning it unchanged.
    /// Full resolution is not implemented; the initial response already contains the edit.
    /// </summary>
    /// <param name="request">The code action to resolve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The same code action.</returns>
    public override Task<CodeAction> Handle(CodeAction request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request);
    }

    /// <summary>
    /// Creates the registration options used when advertising this handler to the client.
    /// </summary>
    protected override CodeActionRegistrationOptions CreateRegistrationOptions(
        CodeActionCapability capability, ClientCapabilities clientCapabilities)
    {
        return new CodeActionRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.xsl" },
                new TextDocumentFilter { Pattern = "**/*.xslt" },
                new TextDocumentFilter { Pattern = "**/*.xpath" },
                new TextDocumentFilter { Pattern = "**/*.xq" },
                new TextDocumentFilter { Pattern = "**/*.xqy" },
                new TextDocumentFilter { Pattern = "**/*.xquery" }
            ),
            CodeActionKinds = new Container<CodeActionKind>(
                CodeActionKind.QuickFix,
                CodeActionKind.Refactor),
        };
    }

    private static List<CommandOrCodeAction> GetXQueryCodeActions(
        string text, Range range, DocumentUri uri, List<Diagnostic> diagnostics)
    {
        var actions = new List<CommandOrCodeAction>();
        var prefixes = GetPrefixedNamesInRange(text, range);

        foreach (var prefix in prefixes)
        {
            if (text.Contains($"declare namespace {prefix}"))
                continue;

            actions.Add(CreateDeclareNamespaceAction(
                uri,
                prefix,
                isXQuery: true,
                insertPosition: GetXQueryPrologInsertPosition(text)));
        }

        // XQST0085: an XML 1.0 prefixed namespace declaration cannot bind to the
        // empty namespace URI. Offer to remove the offending xmlns:prefix="" attribute.
        foreach (var prefix in GetInvalidNamespaceUndeclarationPrefixes(diagnostics))
        {
            var action = CreateRemoveEmptyNamespaceDeclarationAction(uri, text, prefix);
            if (action != null)
                actions.Add(action);
        }

        // Offer to import a module namespace for prefixes used in function calls.
        foreach (var prefix in GetFunctionCallPrefixesInRange(text, range))
        {
            if (text.Contains($"declare namespace {prefix}") ||
                text.Contains($"import module namespace {prefix}"))
                continue;

            actions.Add(CreateImportModuleNamespaceAction(
                uri,
                prefix,
                GetXQueryPrologInsertPosition(text)));
        }

        return actions;
    }

    private static List<CommandOrCodeAction> GetXPathCodeActions(string text, Range range, DocumentUri uri)
    {
        var actions = new List<CommandOrCodeAction>();
        var endOffset = RangeToOffset(text, range.End);
        var insertPosition = PositionToLineColumn(text, endOffset);

        var (openParens, closeParens, openBrackets, closeBrackets, inSingleQuote, inDoubleQuote) =
            CountBracketsAndQuotes(text, range);

        if (openParens > closeParens)
        {
            actions.Add(CreateInsertTextAction(
                uri,
                "Close missing parenthesis",
                new string(')', openParens - closeParens),
                insertPosition));
        }

        if (openBrackets > closeBrackets)
        {
            actions.Add(CreateInsertTextAction(
                uri,
                "Close missing square bracket",
                new string(']', openBrackets - closeBrackets),
                insertPosition));
        }

        if (inSingleQuote)
        {
            actions.Add(CreateInsertTextAction(
                uri,
                "Close single-quoted string",
                "'",
                insertPosition));
        }

        if (inDoubleQuote)
        {
            actions.Add(CreateInsertTextAction(
                uri,
                "Close double-quoted string",
                "\"",
                insertPosition));
        }

        return actions;
    }

    private static List<CommandOrCodeAction> GetXsltCodeActions(string text, Range range, DocumentUri uri, List<Diagnostic> diagnostics)
    {
        var actions = new List<CommandOrCodeAction>();
        var prefixes = GetPrefixedNamesInRange(text, range);

        // Also react to XPST0081 diagnostics that name an undeclared prefix.
        foreach (var prefix in GetUndeclaredPrefixesFromDiagnostics(diagnostics))
        {
            prefixes.Add(prefix);
        }

        foreach (var prefix in prefixes)
        {
            if (prefix == "xsl")
                continue; // xsl prefix is expected to be declared by the stylesheet itself
            if (text.Contains($"xmlns:{prefix}"))
                continue;

            actions.Add(CreateDeclareNamespaceAction(
                uri,
                prefix,
                isXQuery: false,
                insertPosition: GetXsltRootAttributeInsertPosition(text)));
        }

        // Offer to fix a missing XSLT namespace on the root element when that diagnostic is present.
        if (diagnostics.Any(d => d.Message.Contains("xsl:stylesheet") || d.Message.Contains("xsl:transform")))
        {
            actions.Add(CreateFixXsltNamespaceAction(uri, text));
        }

        // Offer to add the required version attribute when the diagnostic is present.
        if (diagnostics.Any(d => d.Message.Contains("Missing required version attribute")))
        {
            actions.Add(CreateFixXsltVersionAction(uri, text));
        }

        return actions;
    }

    private static HashSet<string> GetPrefixedNamesInRange(string text, Range range)
    {
        var prefixes = new HashSet<string>(StringComparer.Ordinal);
        var lines = text.Split('\n');
        int startOffset = RangeToOffset(text, range.Start);
        int endOffset = RangeToOffset(text, range.End);

        foreach (Match m in PrefixedNameRegex.Matches(text))
        {
            if (m.Index >= startOffset && m.Index < endOffset)
            {
                prefixes.Add(m.Groups["prefix"].Value);
            }
        }

        return prefixes;
    }

    private static HashSet<string> GetFunctionCallPrefixesInRange(string text, Range range)
    {
        var prefixes = new HashSet<string>(StringComparer.Ordinal);
        int startOffset = RangeToOffset(text, range.Start);
        int endOffset = RangeToOffset(text, range.End);

        foreach (Match m in PrefixedNameRegex.Matches(text))
        {
            if (m.Index < startOffset || m.Index >= endOffset)
                continue;

            int after = m.Index + m.Length;
            while (after < text.Length && char.IsWhiteSpace(text[after]))
                after++;

            if (after < text.Length && text[after] == '(')
            {
                prefixes.Add(m.Groups["prefix"].Value);
            }
        }

        return prefixes;
    }

    private static (int OpenParens, int CloseParens, int OpenBrackets, int CloseBrackets, bool InSingleQuote, bool InDoubleQuote)
        CountBracketsAndQuotes(string text, Range range)
    {
        int startOffset = RangeToOffset(text, range.Start);
        int endOffset = RangeToOffset(text, range.End);
        int openParens = 0, closeParens = 0, openBrackets = 0, closeBrackets = 0;
        bool inSingleQuote = false, inDoubleQuote = false;

        for (int i = startOffset; i < endOffset && i < text.Length; i++)
        {
            char c = text[i];
            if (inSingleQuote)
            {
                if (c == '\'')
                {
                    if (i + 1 < text.Length && text[i + 1] == '\'')
                    {
                        i++;
                        continue;
                    }
                    inSingleQuote = false;
                }
                continue;
            }

            if (inDoubleQuote)
            {
                if (c == '\"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '\"')
                    {
                        i++;
                        continue;
                    }
                    inDoubleQuote = false;
                }
                continue;
            }

            if (c == '\'')
            {
                inSingleQuote = true;
                continue;
            }

            if (c == '\"')
            {
                inDoubleQuote = true;
                continue;
            }

            switch (c)
            {
                case '(': openParens++; break;
                case ')': closeParens++; break;
                case '[': openBrackets++; break;
                case ']': closeBrackets++; break;
            }
        }

        return (openParens, closeParens, openBrackets, closeBrackets, inSingleQuote, inDoubleQuote);
    }

    private static CommandOrCodeAction CreateInsertTextAction(
        DocumentUri uri, string title, string newText, Position position)
    {
        return new CommandOrCodeAction(new CodeAction
        {
            Title = title,
            Kind = CodeActionKind.QuickFix,
            Edit = new WorkspaceEdit
            {
                Changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>
                {
                    [uri] = new[]
                    {
                        new TextEdit
                        {
                            NewText = newText,
                            Range = new Range(position, position),
                        }
                    }
                }
            }
        });
    }

    private static HashSet<string> GetUndeclaredPrefixesFromDiagnostics(List<Diagnostic> diagnostics)
    {
        var prefixes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var diagnostic in diagnostics)
        {
            var match = Regex.Match(diagnostic.Message, @"XPST0081[^']*'(?<prefix>[a-zA-Z_][a-zA-Z0-9._-]*)' is not declared");
            if (match.Success)
            {
                prefixes.Add(match.Groups["prefix"].Value);
            }
        }

        return prefixes;
    }

    private static HashSet<string> GetInvalidNamespaceUndeclarationPrefixes(List<Diagnostic> diagnostics)
    {
        var prefixes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var diagnostic in diagnostics)
        {
            var match = Regex.Match(diagnostic.Message, @"XQST0085[^']*'(?<prefix>[a-zA-Z_][a-zA-Z0-9._-]*)' cannot be bound to the empty namespace name");
            if (match.Success)
            {
                prefixes.Add(match.Groups["prefix"].Value);
            }
        }

        return prefixes;
    }

    private static CommandOrCodeAction? CreateRemoveEmptyNamespaceDeclarationAction(
        DocumentUri uri, string text, string prefix)
    {
        // Match the offending attribute, including the leading whitespace so that
        // removing it does not leave double spaces inside the start tag.
        var pattern = $"(?<=\\s)xmlns:{Regex.Escape(prefix)}=\"\"";
        var match = Regex.Match(text, pattern);
        if (!match.Success)
        {
            pattern = $"(?<=\\s)xmlns:{Regex.Escape(prefix)}=''";
            match = Regex.Match(text, pattern);
        }

        if (!match.Success)
            return null;

        var start = PositionToLineColumn(text, match.Index - 1);
        var end = PositionToLineColumn(text, match.Index + match.Length);

        return new CommandOrCodeAction(new CodeAction
        {
            Title = $"Remove invalid xmlns:{prefix} declaration",
            Kind = CodeActionKind.QuickFix,
            Edit = new WorkspaceEdit
            {
                Changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>
                {
                    [uri] = new[]
                    {
                        new TextEdit
                        {
                            NewText = string.Empty,
                            Range = new Range(start, end),
                        }
                    }
                }
            }
        });
    }

    private static CommandOrCodeAction CreateDeclareNamespaceAction(
        DocumentUri uri, string prefix, bool isXQuery, Position insertPosition)
    {
        var uriValue = prefix == "xml"
            ? "http://www.w3.org/XML/1998/namespace"
            : string.Empty;
        string newText = isXQuery
            ? $"declare namespace {prefix} = \"{uriValue}\";\n"
            : $" xmlns:{prefix}=\"{uriValue}\"";

        return new CommandOrCodeAction(new CodeAction
        {
            Title = $"Declare namespace '{prefix}'",
            Kind = CodeActionKind.QuickFix,
            Edit = new WorkspaceEdit
            {
                Changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>
                {
                    [uri] = new[]
                    {
                        new TextEdit
                        {
                            NewText = newText,
                            Range = new Range(insertPosition, insertPosition),
                        }
                    }
                }
            }
        });
    }

    private static CommandOrCodeAction CreateImportModuleNamespaceAction(
        DocumentUri uri, string prefix, Position insertPosition)
    {
        return new CommandOrCodeAction(new CodeAction
        {
            Title = $"Import module namespace '{prefix}'",
            Kind = CodeActionKind.QuickFix,
            Edit = new WorkspaceEdit
            {
                Changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>
                {
                    [uri] = new[]
                    {
                        new TextEdit
                        {
                            NewText = $"import module namespace {prefix} = \"\";\n",
                            Range = new Range(insertPosition, insertPosition),
                        }
                    }
                }
            }
        });
    }

    private static CommandOrCodeAction CreateFixXsltNamespaceAction(DocumentUri uri, string text)
    {
        var edits = new List<TextEdit>();

        // Try to locate the root element start tag, optionally already prefixed.
        var match = Regex.Match(text, @"<([a-zA-Z_][a-zA-Z0-9._-]*:)?(stylesheet|transform)\b");
        if (match.Success)
        {
            var prefixGroup = match.Groups[1];
            var localName = match.Groups[2].Value;
            int tagNameEnd = match.Index + match.Length;

            if (!prefixGroup.Success)
            {
                // The root is <stylesheet> or <transform> without the xsl prefix.
                // Insert "xsl:" immediately after the opening "<".
                edits.Add(new TextEdit
                {
                    NewText = "xsl:",
                    Range = new Range(
                        PositionToLineColumn(text, match.Index + 1),
                        PositionToLineColumn(text, match.Index + 1))
                });

                // Rename the matching closing tag as well.
                var close = Regex.Match(text, $"</{localName}>");
                if (close.Success)
                {
                    edits.Add(new TextEdit
                    {
                        NewText = "xsl:",
                        Range = new Range(
                            PositionToLineColumn(text, close.Index + 2),
                            PositionToLineColumn(text, close.Index + 2))
                    });
                }
            }

            if (!text.Contains("xmlns:xsl"))
            {
                edits.Add(new TextEdit
                {
                    NewText = " xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"",
                    Range = new Range(
                        PositionToLineColumn(text, tagNameEnd),
                        PositionToLineColumn(text, tagNameEnd))
                });
            }
        }
        else
        {
            // Fallback: locate any root element start tag and add the namespace attribute.
            var fallback = Regex.Match(text, @"<([a-zA-Z_][a-zA-Z0-9._-]*)");
            int tagNameEnd = fallback.Index + fallback.Length;
            edits.Add(new TextEdit
            {
                NewText = " xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"",
                Range = new Range(
                    PositionToLineColumn(text, tagNameEnd),
                    PositionToLineColumn(text, tagNameEnd))
            });
        }

        return new CommandOrCodeAction(new CodeAction
        {
            Title = "Add XSLT namespace to root element",
            Kind = CodeActionKind.QuickFix,
            Edit = new WorkspaceEdit
            {
                Changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>
                {
                    [uri] = edits
                }
            }
        });
    }

    private static CommandOrCodeAction CreateFixXsltVersionAction(DocumentUri uri, string text)
    {
        // Find the root element start tag and insert version after the tag name.
        var match = Regex.Match(text, @"<([a-zA-Z_:][a-zA-Z0-9._:-]*)\b");
        int tagNameEnd = match.Index + match.Length;
        var position = PositionToLineColumn(text, tagNameEnd);

        return new CommandOrCodeAction(new CodeAction
        {
            Title = "Add XSLT version 3.0 attribute",
            Kind = CodeActionKind.QuickFix,
            Edit = new WorkspaceEdit
            {
                Changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>
                {
                    [uri] = new[]
                    {
                        new TextEdit
                        {
                            NewText = " version=\"3.0\"",
                            Range = new Range(position, position),
                        }
                    }
                }
            }
        });
    }

    private static Position GetXQueryPrologInsertPosition(string text)
    {
        // Insert at the very beginning if there is no prolog.
        var firstDecl = Regex.Match(text, @"^\s*(module|declare|import)\b", RegexOptions.Multiline);
        if (firstDecl.Success)
        {
            var pos = PositionToLineColumn(text, firstDecl.Index);
            return pos;
        }
        return new Position(0, 0);
    }

    private static Position GetXsltRootAttributeInsertPosition(string text)
    {
        var match = Regex.Match(text, @"<([a-zA-Z_:][a-zA-Z0-9._:-]*)\b[^>]*?(?=>)");
        if (match.Success)
        {
            int tagEnd = match.Index + match.Length;
            return PositionToLineColumn(text, tagEnd);
        }
        return new Position(0, 0);
    }

    /// <summary>
    /// Converts a position to a zero-based offset in the text.
    /// </summary>
    internal static int RangeToOffset(string text, Position position)
    {
        var lines = text.Split('\n');
        int offset = 0;
        for (int i = 0; i < System.Math.Min(position.Line, lines.Length); i++)
            offset += lines[i].Length + 1;
        offset += Math.Min(position.Character, position.Line < lines.Length ? lines[position.Line].Length : 0);
        return offset;
    }

    /// <summary>
    /// Converts a zero-based offset to a position.
    /// </summary>
    internal static Position PositionToLineColumn(string text, int offset)
    {
        int line = 0;
        int col = 0;
        for (int i = 0; i < offset && i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                line++;
                col = 0;
            }
            else
            {
                col++;
            }
        }
        return new Position(line, col);
    }
}
