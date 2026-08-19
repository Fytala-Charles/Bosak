// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 08 June 2026
// PURPOSE              : Entry point for the Bosak Language Server communicating over stdio.
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
//                      | Charles Korthout | 0.2   | 18-08-2026     | Register DocumentSymbolHandler, HoverHandler, and DefinitionHandler                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 18-08-2026     | Register custom EvaluateXPathHandler and TransformXsltHandler requests                     |
//                      |==================|=======|================|=========================================================================================
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Server;

namespace Bosak.LanguageServer;

/// <summary>
/// Entry point for the Bosak Language Server.
/// </summary>
class Program
{
    /// <summary>
    /// Boots the language server listening on standard input and writing to standard output.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    static async Task Main(string[] args)
    {
        var debug = Environment.GetEnvironmentVariable("BOSAK_LSP_DEBUG") == "1";
        var minimumLevel = debug ? LogLevel.Debug : LogLevel.Error;

        var server = await OmniSharp.Extensions.LanguageServer.Server.LanguageServer.From(options =>
        {
            options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .ConfigureLogging(logging => logging.SetMinimumLevel(minimumLevel))
                .AddDefaultLoggingProvider()
                .WithServices(services =>
                {
                    services.AddSingleton<DocumentManager>();
                })
                .WithHandler<TextDocumentSyncHandler>()
                .WithHandler<DiagnosticsHandler>()
                .WithHandler<CompletionHandler>()
                .WithHandler<DocumentSymbolHandler>()
                .WithHandler<HoverHandler>()
                .WithHandler<DefinitionHandler>()
                .WithHandler<EvaluateXPathHandler>()
                .WithHandler<EvaluateXQueryHandler>()
                .WithHandler<TransformXsltHandler>();
        }).ConfigureAwait(false);

        await server.WaitForExit.ConfigureAwait(false);
    }
}
