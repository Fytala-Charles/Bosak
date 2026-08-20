// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 August 2026
// PURPOSE              : Unit tests for the language-server execute-command handler.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
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
using Bosak.LanguageServer;
using MediatR;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Xunit;

namespace Bosak.LanguageServer.Tests;

public class ExecuteCommandHandlerTests
{
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
