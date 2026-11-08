// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 August 2026
// PURPOSE              : Unit tests for the language-server execute-command handler.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-08-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 08-11-2026     | Cover escaped command URIs and missing-document isolation.                               |
// ===========================================================================================================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bosak.LanguageServer;
using MediatR;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Xunit;

namespace Bosak.LanguageServer.Tests;

/// <summary>Verifies command evaluation and open-document URI lookup.</summary>
public class ExecuteCommandHandlerTests
{
    /// <summary>XPath command results are sent to the editor.</summary>
    [Fact]
    public async System.Threading.Tasks.Task ExecuteXPathCommandSendsEvaluationResultNotification()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/expr.xpath").ToString();
        documents.Update(uri, "1 + 2");

        var router = new FakeResponseRouter();
        var handler = new ExecuteCommandHandler(documents, router);
        await handler.Handle(new ExecuteCommandParams
        {
            Command = "bosak.evaluateXPath",
            Arguments = new JArray(uri)
        }, default);

        Assert.Single(router.Notifications);
        Assert.Equal("bosak/evaluationResult", router.Notifications[0].Method);
        Assert.Equal("XPath", router.Notifications[0].Params["language"]?.Value<string>());
        Assert.Equal("3", router.Notifications[0].Params["result"]?.Value<string>());
        Assert.Null(router.Notifications[0].Params["error"]?.Value<string>());
    }

    /// <summary>XQuery command results are sent to the editor.</summary>
    [Fact]
    public async System.Threading.Tasks.Task ExecuteXQueryCommandSendsEvaluationResultNotification()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xq").ToString();
        documents.Update(uri, "1 + 2");

        var router = new FakeResponseRouter();
        var handler = new ExecuteCommandHandler(documents, router);
        await handler.Handle(new ExecuteCommandParams
        {
            Command = "bosak.evaluateXQuery",
            Arguments = new JArray(uri)
        }, default);

        Assert.Single(router.Notifications);
        Assert.Equal("bosak/evaluationResult", router.Notifications[0].Method);
        Assert.Equal("XQuery", router.Notifications[0].Params["language"]?.Value<string>());
        Assert.Equal("3", router.Notifications[0].Params["result"]?.Value<string>());
        Assert.Null(router.Notifications[0].Params["error"]?.Value<string>());
    }

    /// <summary>Unknown commands preserve an explicit error notification.</summary>
    [Fact]
    public async System.Threading.Tasks.Task ExecuteUnknownCommandSendsErrorEvaluationResultNotification()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/expr.xpath").ToString();
        documents.Update(uri, "1 + 2");

        var router = new FakeResponseRouter();
        var handler = new ExecuteCommandHandler(documents, router);
        await handler.Handle(new ExecuteCommandParams
        {
            Command = "bosak.unknown",
            Arguments = new JArray(uri)
        }, default);

        Assert.Single(router.Notifications);
        Assert.Equal("bosak/evaluationResult", router.Notifications[0].Method);
        Assert.Null(router.Notifications[0].Params["language"]?.Value<string>());
        Assert.Null(router.Notifications[0].Params["result"]?.Value<string>());
        Assert.Contains("Unknown command", router.Notifications[0].Params["error"]?.Value<string>());
    }

    /// <summary>Commands without a document argument preserve an explicit error.</summary>
    [Fact]
    public async System.Threading.Tasks.Task ExecuteCommandWithMissingArgumentSendsErrorEvaluationResultNotification()
    {
        var documents = new DocumentManager();
        var router = new FakeResponseRouter();
        var handler = new ExecuteCommandHandler(documents, router);
        await handler.Handle(new ExecuteCommandParams
        {
            Command = "bosak.evaluateXPath"
        }, default);

        Assert.Single(router.Notifications);
        Assert.Equal("bosak/evaluationResult", router.Notifications[0].Method);
        Assert.Null(router.Notifications[0].Params["language"]?.Value<string>());
        Assert.Null(router.Notifications[0].Params["result"]?.Value<string>());
        Assert.Contains("document URI", router.Notifications[0].Params["error"]?.Value<string>());
    }

    /// <summary>Equivalent escaped command URIs locate the synchronized document.</summary>
    /// <param name="command">The registered evaluation command.</param>
    /// <param name="commandUri">The URI supplied by the editor command.</param>
    /// <param name="path">The synchronized document filesystem path.</param>
    /// <returns>The asynchronous test operation.</returns>
    [Theory]
    [InlineData("bosak.evaluateXPath", "file:///d%3A/test/expr.xpath", "D:/test/expr.xpath")]
    [InlineData("bosak.evaluateXQuery", "file:///d%3A/test/query.xq", "D:/test/query.xq")]
    [InlineData("bosak.evaluateXPath", "file:///d%3A/test/a%20b%25%23%3F.xpath", "D:/test/a b%#?.xpath")]
    [InlineData("bosak.evaluateXQuery", "file:///d%3A/test/caf%C3%A9.xq", "D:/test/caf\u00e9.xq")]
    public async Task ExecuteCommandWithEscapedUriLocatesOpenDocument(string command, string commandUri, string path)
    {
        var documents = new DocumentManager();
        documents.Update(DocumentUri.FromFileSystemPath(path).ToString(), "6 * 7");
        var router = new FakeResponseRouter();
        await new ExecuteCommandHandler(documents, router).Handle(new ExecuteCommandParams
        {
            Command = command,
            Arguments = new JArray(commandUri)
        }, default);

        var notification = Assert.Single(router.Notifications);
        Assert.Equal("42", notification.Params["result"]?.Value<string>());
        Assert.Null(notification.Params["error"]?.Value<string>());
    }

    /// <summary>Decoding once does not alias a different or unopened document.</summary>
    /// <param name="commandUri">The URI that must not resolve to the open document.</param>
    /// <returns>The asynchronous test operation.</returns>
    [Theory]
    [InlineData("file:///d%3A/test/other.xpath")]
    [InlineData("file:///d%3A/test/a%2520b.xpath")]
    public async Task ExecuteCommandWithDifferentUriRetainsMissingDocumentError(string commandUri)
    {
        var documents = new DocumentManager();
        documents.Update(DocumentUri.FromFileSystemPath("D:/test/a b.xpath").ToString(), "6 * 7");
        var router = new FakeResponseRouter();
        await new ExecuteCommandHandler(documents, router).Handle(new ExecuteCommandParams
        {
            Command = "bosak.evaluateXPath",
            Arguments = new JArray(commandUri)
        }, default);

        var notification = Assert.Single(router.Notifications);
        Assert.Null(notification.Params["result"]?.Value<string>());
        Assert.Equal("Document is not open.", notification.Params["error"]?.Value<string>());
    }

    private sealed class FakeResponseRouter : IResponseRouter
    {
        public List<(string Method, JObject Params)> Notifications { get; } = new();

        public void SendNotification(string method)
        {
            throw new NotSupportedException();
        }

        public void SendNotification<T>(string method, T @params)
        {
            if (@params is JObject jobject)
            {
                Notifications.Add((method, jobject));
            }
        }

        public void SendNotification<T>(T request) where T : IRequest
        {
            throw new NotSupportedException();
        }

        public void SendNotification(IRequest request)
        {
            throw new NotSupportedException();
        }

        public IResponseRouterReturns SendRequest(string method)
        {
            throw new NotSupportedException();
        }

        public IResponseRouterReturns SendRequest<T>(string method, T @params)
        {
            throw new NotSupportedException();
        }

        public Task<TResponse> SendRequest<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public bool TryGetRequest(long id, out string method, out TaskCompletionSource<JToken> pendingTask)
        {
            throw new NotSupportedException();
        }
    }
}
