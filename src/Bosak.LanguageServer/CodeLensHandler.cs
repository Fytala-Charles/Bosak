// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 August 2026
// PURPOSE              : Provides code lenses for XPath documents showing the evaluated result.
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
using System.Threading;
using System.Threading.Tasks;
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Bosak.LanguageServer;

/// <summary>
/// Provides code lenses for XPath documents by evaluating the expression and
/// showing the serialized result above the document.
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
        if (!IsXPathDocument(uri))
            return Task.FromResult<CodeLensContainer?>(new CodeLensContainer(Array.Empty<CodeLens>()));

        if (!_documents.TryGet(uri.ToString(), out var text))
            return Task.FromResult<CodeLensContainer?>(new CodeLensContainer(Array.Empty<CodeLens>()));

        var (result, error) = EvaluateXPath(text);
        var title = error is null ? $"= {Truncate(result!, MaxTitleLength)}" : $"XPath error: {Truncate(error, MaxTitleLength)}";

        var lenses = new List<CodeLens>
        {
            new()
            {
                Range = new Range(new Position(0, 0), new Position(0, 0)),
                Command = new Command
                {
                    Title = title,
                    Name = "bosak.evaluateXPath",
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
                new TextDocumentFilter { Pattern = "**/*.xpath" }),
            ResolveProvider = false,
        };
    }

    private static bool IsXPathDocument(DocumentUri uri)
    {
        var path = uri.Path?.ToLowerInvariant() ?? string.Empty;
        return path.EndsWith(".xpath");
    }

    private static (string? Result, string? Error) EvaluateXPath(string text)
    {
        try
        {
            var ctx = new EvaluationContext();
            FunctionLibrary.Populate(ctx);
            var value = XPath31Expression.Compile(text).Evaluate(ctx);
            return (value.ToString(), null);
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
