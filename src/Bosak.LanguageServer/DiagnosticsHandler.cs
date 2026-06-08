// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 08 June 2026
// PURPOSE              : Publishes LSP diagnostics for XPath and XSLT documents using the Bosak engine.
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Compiler;
using Bosak.XPath.Parser;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Bosak.LanguageServer;

/// <summary>
/// Publishes diagnostics for open documents by leveraging the Bosak
/// XPath parser and XSLT compiler.
/// </summary>
public class DiagnosticsHandler : DocumentDiagnosticHandlerBase
{
    private readonly DocumentManager _documents;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsHandler"/> class.
    /// </summary>
    /// <param name="documents">The document manager holding open document contents.</param>
    public DiagnosticsHandler(DocumentManager documents)
    {
        _documents = documents;
    }

    /// <summary>
    /// Handles a request for document diagnostics.
    /// </summary>
    /// <param name="request">The document diagnostic parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A diagnostic report for the requested document.</returns>
    public override Task<RelatedDocumentDiagnosticReport> Handle(
        DocumentDiagnosticParams request,
        CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri;
        var diagnostics = GetDiagnostics(uri);

        var report = new RelatedFullDocumentDiagnosticReport
        {
            Items = new Container<Diagnostic>(diagnostics),
        };

        return Task.FromResult<RelatedDocumentDiagnosticReport>(report);
    }

    private List<Diagnostic> GetDiagnostics(DocumentUri uri)
    {
        var diagnostics = new List<Diagnostic>();
        if (!_documents.TryGet(uri.ToString(), out var text))
            return diagnostics;

        var path = uri.Path?.ToLowerInvariant() ?? string.Empty;

        if (path.EndsWith(".xsl") || path.EndsWith(".xslt"))
        {
            ValidateXslt(text, diagnostics);
        }
        else if (path.EndsWith(".xpath"))
        {
            ValidateXPath(text, diagnostics);
        }

        return diagnostics;
    }

    private static void ValidateXPath(string text, List<Diagnostic> diagnostics)
    {
        try
        {
            XPath31Expression.Compile(text);
        }
        catch (ParseException ex)
        {
            diagnostics.Add(CreateDiagnostic(ex.Message, 0, 0, DiagnosticSeverity.Error));
        }
        catch (Exception ex)
        {
            diagnostics.Add(CreateDiagnostic(ex.Message, 0, 0, DiagnosticSeverity.Error));
        }
    }

    private static void ValidateXslt(string text, List<Diagnostic> diagnostics)
    {
        // First check well-formedness
        XDocument? doc = null;
        try
        {
            doc = XDocument.Parse(text, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        }
        catch (XmlException ex)
        {
            var line = Math.Max(0, (ex.LineNumber) - 1);
            var col = Math.Max(0, (ex.LinePosition) - 1);
            diagnostics.Add(CreateDiagnostic(ex.Message, line, col, DiagnosticSeverity.Error));
            return;
        }

        if (doc?.Root == null)
            return;

        // Validate xsl:stylesheet / xsl:transform root
        var root = doc.Root;
        var xslNs = "http://www.w3.org/1999/XSL/Transform";
        if (root.Name.NamespaceName != xslNs ||
            (root.Name.LocalName != "stylesheet" && root.Name.LocalName != "transform"))
        {
            diagnostics.Add(CreateDiagnostic(
                "Expected xsl:stylesheet or xsl:transform as the root element.",
                0, 0, DiagnosticSeverity.Error));
        }

        // Validate XPath expressions in select, test, match attributes
        foreach (var element in root.DescendantsAndSelf())
        {
            if (element.Name.NamespaceName != xslNs)
                continue;

            foreach (var attr in element.Attributes())
            {
                var attrName = attr.Name.LocalName;
                if (attrName == "select" || attrName == "test" || attrName == "match" || attrName == "use-when")
                {
                    try
                    {
                        XPath31Expression.Compile(attr.Value);
                    }
                    catch (Exception ex)
                    {
                        var lineInfo = attr as IXmlLineInfo;
                        var line = (lineInfo?.LineNumber ?? 1) - 1;
                        var col = (lineInfo?.LinePosition ?? 1) - 1;
                        diagnostics.Add(CreateDiagnostic(
                            $"{attrName}: {ex.Message}", line, col, DiagnosticSeverity.Error));
                    }
                }
            }
        }
    }

    private static Diagnostic CreateDiagnostic(string message, int line, int character, DiagnosticSeverity severity)
    {
        return new Diagnostic
        {
            Message = message,
            Severity = severity,
            Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                new Position(line, character),
                new Position(line, character + 1)),
            Source = "bosak",
        };
    }

    /// <summary>
    /// Creates the registration options used when advertising this handler to the client.
    /// </summary>
    /// <param name="capability">The client's diagnostic capability.</param>
    /// <param name="clientCapabilities">The full client capabilities.</param>
    /// <returns>Registration options for the diagnostic provider.</returns>
    protected override DiagnosticsRegistrationOptions CreateRegistrationOptions(
        DiagnosticClientCapabilities capability,
        ClientCapabilities clientCapabilities)
    {
        return new DiagnosticsRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.xsl" },
                new TextDocumentFilter { Pattern = "**/*.xslt" },
                new TextDocumentFilter { Pattern = "**/*.xpath" }
            ),
            Identifier = "bosak",
        };
    }
}
