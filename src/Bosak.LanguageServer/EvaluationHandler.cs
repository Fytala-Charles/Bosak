// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 August 2026
// PURPOSE              : Provides custom LSP requests to evaluate XPath expressions and run XSLT transformations.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 18-08-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System;
using System.Threading;
using System.Threading.Tasks;
using Bosak.XPath.Api;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Bosak.Xslt.Api;
using MediatR;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Bosak.LanguageServer;

/// <summary>Parameters for the <c>bosak/evaluateXPath</c> request.</summary>
[Method("bosak/evaluateXPath", Direction.ClientToServer)]
public record EvaluateXPathParams : IRequest<EvaluateXPathResult>
{
    /// <summary>The document containing the XPath expression.</summary>
    public TextDocumentIdentifier TextDocument { get; init; } = null!;
}

/// <summary>The result of an XPath evaluation.</summary>
public record EvaluateXPathResult
{
    /// <summary>The serialized result, when evaluation succeeded.</summary>
    public string? Result { get; init; }
    /// <summary>The error message, when evaluation failed.</summary>
    public string? Error { get; init; }
}

/// <summary>Parameters for the <c>bosak/transformXslt</c> request.</summary>
[Method("bosak/transformXslt", Direction.ClientToServer)]
public record TransformXsltParams : IRequest<TransformXsltResult>
{
    /// <summary>The document containing the XSLT stylesheet.</summary>
    public TextDocumentIdentifier TextDocument { get; init; } = null!;
    /// <summary>The file path of the source XML document to transform.</summary>
    public string? SourcePath { get; init; }
}

/// <summary>The result of an XSLT transformation.</summary>
public record TransformXsltResult
{
    /// <summary>The serialized transformation output, when it succeeded.</summary>
    public string? Result { get; init; }
    /// <summary>The error message, when the transformation failed.</summary>
    public string? Error { get; init; }
}

/// <summary>Parameters for the <c>bosak/runInitialTemplate</c> request.</summary>
[Method("bosak/runInitialTemplate", Direction.ClientToServer)]
public record RunInitialTemplateParams : IRequest<RunInitialTemplateResult>
{
    /// <summary>The document containing the XSLT stylesheet.</summary>
    public TextDocumentIdentifier TextDocument { get; init; } = null!;
    /// <summary>The optional name of the initial template. When omitted, the stylesheet's declared <c>xsl:initial-template</c> is used.</summary>
    public string? InitialTemplate { get; init; }
}

/// <summary>The result of running an XSLT stylesheet from an initial template.</summary>
public record RunInitialTemplateResult
{
    /// <summary>The serialized output, when execution succeeded.</summary>
    public string? Result { get; init; }
    /// <summary>The error message, when execution failed.</summary>
    public string? Error { get; init; }
}

/// <summary>
/// Evaluates the XPath expression contained in a document and returns the serialized result.
/// </summary>
public class EvaluateXPathHandler : IJsonRpcRequestHandler<EvaluateXPathParams, EvaluateXPathResult>
{
    private readonly DocumentManager _documents;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluateXPathHandler"/> class.
    /// </summary>
    /// <param name="documents">The document manager holding open document contents.</param>
    public EvaluateXPathHandler(DocumentManager documents)
    {
        _documents = documents;
    }

    /// <summary>
    /// Evaluates the XPath expression in the requested document.
    /// </summary>
    /// <param name="request">The evaluation parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The evaluation result or an error message.</returns>
    public Task<EvaluateXPathResult> Handle(EvaluateXPathParams request, CancellationToken cancellationToken)
    {
        if (!_documents.TryGet(request.TextDocument.Uri.ToString(), out var text))
            return Task.FromResult(new EvaluateXPathResult { Error = "Document is not open." });

        try
        {
            var ctx = new EvaluationContext();
            FunctionLibrary.Populate(ctx);
            var result = XPath31Expression.Compile(text).Evaluate(ctx);
            return Task.FromResult(new EvaluateXPathResult { Result = result.ToString() });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new EvaluateXPathResult { Error = ex.Message });
        }
    }
}

/// <summary>Parameters for the <c>bosak/evaluateXQuery</c> request.</summary>
[Method("bosak/evaluateXQuery", Direction.ClientToServer)]
public record EvaluateXQueryParams : IRequest<EvaluateXQueryResult>
{
    /// <summary>The document containing the XQuery expression.</summary>
    public TextDocumentIdentifier TextDocument { get; init; } = null!;
}

/// <summary>The result of an XQuery evaluation.</summary>
public record EvaluateXQueryResult
{
    /// <summary>The serialized result, when evaluation succeeded.</summary>
    public string? Result { get; init; }
    /// <summary>The error message, when evaluation failed.</summary>
    public string? Error { get; init; }
}

/// <summary>
/// Evaluates the XQuery expression contained in a document and returns the serialized result.
/// </summary>
public class EvaluateXQueryHandler : IJsonRpcRequestHandler<EvaluateXQueryParams, EvaluateXQueryResult>
{
    private readonly DocumentManager _documents;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluateXQueryHandler"/> class.
    /// </summary>
    /// <param name="documents">The document manager holding open document contents.</param>
    public EvaluateXQueryHandler(DocumentManager documents)
    {
        _documents = documents;
    }

    /// <summary>
    /// Evaluates the XQuery expression in the requested document.
    /// </summary>
    /// <param name="request">The evaluation parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The evaluation result or an error message.</returns>
    public Task<EvaluateXQueryResult> Handle(EvaluateXQueryParams request, CancellationToken cancellationToken)
    {
        if (!_documents.TryGet(request.TextDocument.Uri.ToString(), out var text))
            return Task.FromResult(new EvaluateXQueryResult { Error = "Document is not open." });

        try
        {
            var result = new Bosak.XQuery.Api.XQueryCompiler().Compile(text).Evaluate(new Bosak.XQuery.Api.XQueryContext());
            return Task.FromResult(new EvaluateXQueryResult { Result = result.ToString() });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new EvaluateXQueryResult { Error = ex.Message });
        }
    }
}

/// <summary>
/// Runs the XSLT stylesheet contained in a document against a source XML document and
/// returns the serialized result.
/// </summary>
public class TransformXsltHandler : IJsonRpcRequestHandler<TransformXsltParams, TransformXsltResult>
{
    private readonly DocumentManager _documents;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransformXsltHandler"/> class.
    /// </summary>
    /// <param name="documents">The document manager holding open document contents.</param>
    public TransformXsltHandler(DocumentManager documents)
    {
        _documents = documents;
    }

    /// <summary>
    /// Runs the transformation.
    /// </summary>
    /// <param name="request">The transformation parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The transformation result or an error message.</returns>
    public Task<TransformXsltResult> Handle(TransformXsltParams request, CancellationToken cancellationToken)
    {
        if (!_documents.TryGet(request.TextDocument.Uri.ToString(), out var xsltText))
            return Task.FromResult(new TransformXsltResult { Error = "Document is not open." });

        if (string.IsNullOrEmpty(request.SourcePath))
            return Task.FromResult(new TransformXsltResult { Error = "No source document was provided." });

        try
        {
            var executable = new XsltCompiler().Compile(xsltText);
            var source = XDocumentProvider.LoadXml(request.SourcePath);
            var result = executable.TransformToString(source);
            return Task.FromResult(new TransformXsltResult { Result = result });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new TransformXsltResult { Error = ex.Message });
        }
    }
}

/// <summary>
/// Runs an XSLT stylesheet from an initial named template without requiring a source XML document.
/// </summary>
public class RunInitialTemplateHandler : IJsonRpcRequestHandler<RunInitialTemplateParams, RunInitialTemplateResult>
{
    private readonly DocumentManager _documents;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunInitialTemplateHandler"/> class.
    /// </summary>
    /// <param name="documents">The document manager holding open document contents.</param>
    public RunInitialTemplateHandler(DocumentManager documents)
    {
        _documents = documents;
    }

    /// <summary>
    /// Runs the stylesheet starting from the named initial template.
    /// </summary>
    /// <param name="request">The run parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The serialized result or an error message.</returns>
    public Task<RunInitialTemplateResult> Handle(RunInitialTemplateParams request, CancellationToken cancellationToken)
    {
        if (!_documents.TryGet(request.TextDocument.Uri.ToString(), out var xsltText))
            return Task.FromResult(new RunInitialTemplateResult { Error = "Document is not open." });

        try
        {
            var executable = new XsltCompiler().Compile(xsltText);
            var result = executable.TransformToString(source: null, initialTemplate: request.InitialTemplate);
            return Task.FromResult(new RunInitialTemplateResult { Result = result });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new RunInitialTemplateResult { Error = ex.Message });
        }
    }
}
