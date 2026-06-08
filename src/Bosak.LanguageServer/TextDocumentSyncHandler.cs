// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 08 June 2026
// PURPOSE              : Handles LSP text document synchronization notifications (open, change, close, save).
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace Bosak.LanguageServer;

/// <summary>
/// Handles text document open, change, close and save notifications.
/// </summary>
public class TextDocumentSyncHandler : TextDocumentSyncHandlerBase
{
    private readonly DocumentManager _documents;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextDocumentSyncHandler"/> class.
    /// </summary>
    /// <param name="documents">The document manager holding open document contents.</param>
    public TextDocumentSyncHandler(DocumentManager documents)
    {
        _documents = documents;
    }

    /// <summary>
    /// Returns the language identifier for the given document URI.
    /// </summary>
    /// <param name="uri">The document URI.</param>
    /// <returns>Text document attributes including the language id.</returns>
    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
    {
        return new TextDocumentAttributes(uri, GetLanguageId(uri));
    }

    private static string GetLanguageId(DocumentUri uri)
    {
        var path = uri.Path?.ToLowerInvariant() ?? string.Empty;
        if (path.EndsWith(".xsl") || path.EndsWith(".xslt"))
            return "xslt";
        if (path.EndsWith(".xpath"))
            return "xpath";
        return "xml";
    }

    /// <summary>
    /// Handles a text document change notification.
    /// </summary>
    /// <param name="request">The change parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToString();
        if (request.ContentChanges is not null && request.ContentChanges.Any())
        {
            // Full document sync — replace entire content
            var lastChange = request.ContentChanges.LastOrDefault();
            if (lastChange != null)
            {
                _documents.Update(uri, lastChange.Text ?? string.Empty);
            }
        }
        return Unit.Task;
    }

    /// <summary>
    /// Handles a text document open notification.
    /// </summary>
    /// <param name="request">The open parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToString();
        _documents.Update(uri, request.TextDocument.Text ?? string.Empty);
        return Unit.Task;
    }

    /// <summary>
    /// Handles a text document close notification.
    /// </summary>
    /// <param name="request">The close parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToString();
        _documents.Remove(uri);
        return Unit.Task;
    }

    /// <summary>
    /// Handles a text document save notification.
    /// </summary>
    /// <param name="request">The save parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        return Unit.Task;
    }

    /// <summary>
    /// Creates the registration options used when advertising this handler to the client.
    /// </summary>
    /// <param name="capability">The client's text synchronization capability.</param>
    /// <param name="clientCapabilities">The full client capabilities.</param>
    /// <returns>Registration options for the text document sync provider.</returns>
    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(
        TextSynchronizationCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new TextDocumentSyncRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.xsl" },
                new TextDocumentFilter { Pattern = "**/*.xslt" },
                new TextDocumentFilter { Pattern = "**/*.xpath" }
            ),
            Change = TextDocumentSyncKind.Full,
            Save = new SaveOptions { IncludeText = false },
        };
    }
}
