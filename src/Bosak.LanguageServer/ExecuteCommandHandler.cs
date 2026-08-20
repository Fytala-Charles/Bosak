// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 August 2026
// PURPOSE              : Handles workspace/executeCommand requests for code lens and command palette actions.
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
using System.Threading;
using System.Threading.Tasks;
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Bosak.XQuery.Api;
using MediatR;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace Bosak.LanguageServer;

/// <summary>
/// Executes LSP workspace commands such as evaluating the current XPath or XQuery document.
/// </summary>
public class ExecuteCommandHandler : ExecuteCommandHandlerBase
{
    private readonly DocumentManager _documents;
    private readonly IResponseRouter _router;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteCommandHandler"/> class.
    /// </summary>
    /// <param name="documents">The document manager holding open document contents.</param>
    /// <param name="router">The JSON-RPC router used to notify the client.</param>
    public ExecuteCommandHandler(DocumentManager documents, IResponseRouter router)
    {
        _documents = documents;
        _router = router;
    }

    /// <summary>
    /// Executes the requested command by evaluating the document identified by the first argument.
    /// </summary>
    public override Task<Unit> Handle(
        ExecuteCommandParams request, CancellationToken cancellationToken)
    {
        var command = request.Command;
        var args = request.Arguments;

        string? language;
        string? result = null;
        string? error = null;

        if (args is null || args.Count == 0 || args[0] is not JValue { Type: JTokenType.String } value)
        {
            language = null;
            error = "Execute command requires a document URI as the first argument.";
        }
        else
        {
            var uri = value.Value?.ToString() ?? string.Empty;
            if (!_documents.TryGet(uri, out var text))
            {
                language = null;
                error = "Document is not open.";
            }
            else
            {
                language = command switch
                {
                    "bosak.evaluateXPath" => "XPath",
                    "bosak.evaluateXQuery" => "XQuery",
                    _ => null
                };

                if (language is null)
                {
                    error = $"Unknown command: {command}";
                }
                else
                {
                    (result, error) = language == "XPath" ? EvaluateXPath(text) : EvaluateXQuery(text);
                }
            }
        }

        _router.SendNotification("bosak/evaluationResult", new JObject
        {
            ["language"] = language,
            ["result"] = result,
            ["error"] = error
        });

        return Task.FromResult(Unit.Value);
    }

    /// <summary>
    /// Creates the registration options used when advertising this handler to the client.
    /// </summary>
    protected override ExecuteCommandRegistrationOptions CreateRegistrationOptions(
        ExecuteCommandCapability capability, ClientCapabilities clientCapabilities)
    {
        return new ExecuteCommandRegistrationOptions
        {
            Commands = new Container<string>("bosak.evaluateXPath", "bosak.evaluateXQuery")
        };
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

    private static (string? Result, string? Error) EvaluateXQuery(string text)
    {
        try
        {
            var result = new XQueryCompiler().Compile(text).Evaluate(new XQueryContext());
            return (result.ToString(), null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }
}
