// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 August 2026
// PURPOSE              : Provides code lenses for XPath, XQuery, and XSLT documents.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-08-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 20-08-2026     | Added XQuery document support                                                            |
//                      | Charles Korthout | 0.3   | 20-08-2026     | Added XSLT document support                                                              |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Bosak.XQuery.Api;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Bosak.LanguageServer;

/// <summary>
/// Provides code lenses for XPath, XQuery, and XSLT documents. XPath and XQuery lenses
/// evaluate the document and show the serialized result; XSLT lenses provide a command
/// to run the transformation because XSLT requires an external source document.
/// </summary>
public class CodeLensHandler : CodeLensHandlerBase
{
    private const int MaxTitleLength = 80;

    private readonly DocumentManager _documents;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeLensHandler"/> class.
    /// </summary>
    /// <param name="documents">The document manager holding open document contents.</param>
    public CodeLensHandler(DocumentManager documents)
    {
        _documents = documents;
    }

    /// <summary>
    /// Returns the code lenses available for the requested document.
    /// </summary>
    public override Task<CodeLensContainer?> Handle(
        CodeLensParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri;
        var language = GetDocumentLanguage(uri);
        if (language is null)
            return Task.FromResult<CodeLensContainer?>(new CodeLensContainer(Array.Empty<CodeLens>()));

        if (!_documents.TryGet(uri.ToString(), out var text))
            return Task.FromResult<CodeLensContainer?>(new CodeLensContainer(Array.Empty<CodeLens>()));

        var (title, commandName) = language == "XSLT"
            ? ("Run XSLT transformation", "bosak.transformXslt")
            : GetEvaluatedLens(text, language);

        var lenses = new List<CodeLens>
        {
            new()
            {
                Range = new Range(new Position(0, 0), new Position(0, 0)),
                Command = new Command
                {
                    Title = title,
                    Name = commandName,
                    Arguments = new JArray(uri.ToString()),
                }
            }
        };

        return Task.FromResult<CodeLensContainer?>(new CodeLensContainer(lenses));
    }

    /// <summary>
    /// Resolves a partially-populated code lens by returning it unchanged.
    /// The initial response already contains the fully populated command.
    /// </summary>
    public override Task<CodeLens> Handle(CodeLens request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request);
    }

    /// <summary>
    /// Creates the registration options used when advertising this handler to the client.
    /// </summary>
    protected override CodeLensRegistrationOptions CreateRegistrationOptions(
        CodeLensCapability capability, ClientCapabilities clientCapabilities)
    {
        return new CodeLensRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.{xpath,xq,xqy,xquery,xsl,xslt}" }),
            ResolveProvider = false,
        };
    }

    private static string? GetDocumentLanguage(DocumentUri uri)
    {
        var path = uri.Path?.ToLowerInvariant() ?? string.Empty;
        if (path.EndsWith(".xpath"))
            return "XPath";
        if (path.EndsWith(".xq") || path.EndsWith(".xqy") || path.EndsWith(".xquery"))
            return "XQuery";
        if (path.EndsWith(".xsl") || path.EndsWith(".xslt"))
            return "XSLT";
        return null;
    }

    private static (string Title, string CommandName) GetEvaluatedLens(string text, string language)
    {
        var (result, error) = EvaluateDocument(text, language);
        var title = error is null
            ? $"= {Truncate(result!, MaxTitleLength)}"
            : $"{language} error: {Truncate(error, MaxTitleLength)}";
        var commandName = language == "XPath" ? "bosak.evaluateXPath" : "bosak.evaluateXQuery";
        return (title, commandName);
    }

    private static (string? Result, string? Error) EvaluateDocument(string text, string language)
    {
        try
        {
            if (language == "XPath")
            {
                var ctx = new EvaluationContext();
                FunctionLibrary.Populate(ctx);
                var value = XPath31Expression.Compile(text).Evaluate(ctx);
                return (value.ToString(), null);
            }

            var result = new XQueryCompiler().Compile(text).Evaluate(new XQueryContext());
            return (result.ToString(), null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value ?? string.Empty;
        return value[..(maxLength - 3)] + "...";
    }
}
