// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 August 2026
// PURPOSE              : Unit tests for the language-server code action handler.
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
//                      | Charles Korthout | 0.2   | 20-08-2026     | Added tests for XSLT root rename and version quick fixes                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 20-08-2026     | Added test for XPST0081 diagnostic-driven namespace declaration                         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 20-08-2026     | Added tests for XQST0085 remove-empty-namespace-declaration action                      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 20-08-2026     | Added tests for XQuery import-module-namespace action                                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 20-08-2026     | Added tests for XPath syntax-error close actions                                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 20-08-2026     | Added tests for standard XML namespace URI on xml prefix declarations                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Linq;
using Bosak.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;
using Xunit;

namespace Bosak.LanguageServer.Tests;

public class CodeActionHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task XQueryOffersNamespaceDeclarationForUnknownPrefix()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xq").ToString();
        const string text = "my:foo()";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/query.xq")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext()
        }, default);

        Assert.NotNull(result);
        var action = result!.Select(c => c.CodeAction).FirstOrDefault(a => a?.Title == "Declare namespace 'my'");
        Assert.NotNull(action);
        var edit = action!.Edit!.Changes!.Single();
        Assert.Equal("declare namespace my = \"\";\n", edit.Value.First().NewText);
    }

    [Fact]
    public async System.Threading.Tasks.Task XQueryUsesStandardUriForXmlPrefix()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xq").ToString();
        const string text = "xml:lang";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/query.xq")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext()
        }, default);

        Assert.NotNull(result);
        var action = result!.Select(c => c.CodeAction).FirstOrDefault(a => a?.Title == "Declare namespace 'xml'");
        Assert.NotNull(action);
        var edit = action!.Edit!.Changes!.Single();
        Assert.Equal("declare namespace xml = \"http://www.w3.org/XML/1998/namespace\";\n", edit.Value.First().NewText);
    }

    [Fact]
    public async System.Threading.Tasks.Task XQuerySkipsPrefixAlreadyDeclared()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xq").ToString();
        const string text = "declare namespace my = \"http://example.com\";\nmy:foo()";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/query.xq")),
            Range = new Range(new Position(1, 0), new Position(1, 10)),
            Context = new CodeActionContext()
        }, default);

        Assert.NotNull(result);
        Assert.DoesNotContain(result!, c => c.CodeAction?.Title == "Declare namespace 'my'");
    }

    [Fact]
    public async System.Threading.Tasks.Task XQueryOffersImportModuleNamespaceForFunctionCall()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xq").ToString();
        const string text = "my:foo()";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/query.xq")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext()
        }, default);

        Assert.NotNull(result);
        var action = result!.Select(c => c.CodeAction).FirstOrDefault(a => a?.Title == "Import module namespace 'my'");
        Assert.NotNull(action);
        var edit = action!.Edit!.Changes!.Single().Value.First();
        Assert.Equal("import module namespace my = \"\";\n", edit.NewText);
    }

    [Fact]
    public async System.Threading.Tasks.Task XQueryDoesNotOfferImportModuleForNonFunctionPrefix()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xq").ToString();
        const string text = "$my:var";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/query.xq")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext()
        }, default);

        Assert.NotNull(result);
        Assert.DoesNotContain(result!, c => c.CodeAction?.Title == "Import module namespace 'my'");
    }

    [Fact]
    public async System.Threading.Tasks.Task XQueryOffersRemoveInvalidEmptyNamespaceDeclaration()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xq").ToString();
        const string text = "<root xmlns:p=\"\"></root>";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/query.xq")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext
            {
                Diagnostics = new Container<Diagnostic>(
                    new Diagnostic
                    {
                        Message = "XQST0085: The prefix 'p' cannot be bound to the empty namespace name."
                    })
            }
        }, default);

        Assert.NotNull(result);
        var action = result!.Select(c => c.CodeAction).FirstOrDefault(a => a?.Title == "Remove invalid xmlns:p declaration");
        Assert.NotNull(action);
        var edit = action!.Edit!.Changes!.Single().Value.First();
        Assert.Equal(string.Empty, edit.NewText);
        Assert.Equal(0, edit.Range.Start.Line);
        Assert.Equal(5, edit.Range.Start.Character);
        Assert.Equal(0, edit.Range.End.Line);
        Assert.Equal(16, edit.Range.End.Character);
    }

    [Fact]
    public async System.Threading.Tasks.Task XQueryOffersRemoveInvalidEmptyNamespaceDeclarationSingleQuotes()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/query.xq").ToString();
        const string text = "<root xmlns:p=''></root>";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/query.xq")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext
            {
                Diagnostics = new Container<Diagnostic>(
                    new Diagnostic
                    {
                        Message = "XQST0085: The prefix 'p' cannot be bound to the empty namespace name."
                    })
            }
        }, default);

        Assert.NotNull(result);
        var action = result!.Select(c => c.CodeAction).FirstOrDefault(a => a?.Title == "Remove invalid xmlns:p declaration");
        Assert.NotNull(action);
        var edit = action!.Edit!.Changes!.Single().Value.First();
        Assert.Equal(string.Empty, edit.NewText);
        Assert.Equal(0, edit.Range.Start.Line);
        Assert.Equal(5, edit.Range.Start.Character);
        Assert.Equal(0, edit.Range.End.Line);
        Assert.Equal(16, edit.Range.End.Character);
    }

    [Fact]
    public async System.Threading.Tasks.Task XsltOffersNamespaceDeclarationForUnknownPrefix()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/transform.xslt").ToString();
        const string text = "<root><my:child/></root>";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/transform.xslt")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext()
        }, default);

        Assert.NotNull(result);
        var action = result!.Select(c => c.CodeAction).FirstOrDefault(a => a?.Title == "Declare namespace 'my'");
        Assert.NotNull(action);
        var edit = action!.Edit!.Changes!.Single();
        Assert.Equal(" xmlns:my=\"\"", edit.Value.First().NewText);
    }

    [Fact]
    public async System.Threading.Tasks.Task XsltUsesStandardUriForXmlPrefix()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/transform.xslt").ToString();
        const string text = "<root><xml:child/></root>";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/transform.xslt")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext()
        }, default);

        Assert.NotNull(result);
        var action = result!.Select(c => c.CodeAction).FirstOrDefault(a => a?.Title == "Declare namespace 'xml'");
        Assert.NotNull(action);
        var edit = action!.Edit!.Changes!.Single();
        Assert.Equal(" xmlns:xml=\"http://www.w3.org/XML/1998/namespace\"", edit.Value.First().NewText);
    }

    [Fact]
    public async System.Threading.Tasks.Task XsltSkipsXslPrefix()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/transform.xslt").ToString();
        const string text = "<xsl:stylesheet version=\"3.0\"></xsl:stylesheet>";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/transform.xslt")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext()
        }, default);

        Assert.NotNull(result);
        Assert.DoesNotContain(result!, c => c.CodeAction?.Title.Contains("xsl") == true);
    }

    [Fact]
    public async System.Threading.Tasks.Task XsltOffersRootNamespaceFixFromDiagnostic()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/transform.xslt").ToString();
        const string text = "<stylesheet version=\"3.0\"></stylesheet>";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/transform.xslt")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext
            {
                Diagnostics = new Container<Diagnostic>(
                    new Diagnostic
                    {
                        Message = "Expected xsl:stylesheet root element"
                    })
            }
        }, default);

        Assert.NotNull(result);
        var action = result!.Select(c => c.CodeAction).FirstOrDefault(a => a?.Title == "Add XSLT namespace to root element");
        Assert.NotNull(action);
        var edits = action!.Edit!.Changes!.Single().Value.ToList();
        Assert.Contains("xsl:", edits.Select(e => e.NewText));
        Assert.Contains(" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"", edits.Select(e => e.NewText));
    }

    [Fact]
    public async System.Threading.Tasks.Task XsltOffersVersionFixFromDiagnostic()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/transform.xslt").ToString();
        const string text = "<xsl:stylesheet></xsl:stylesheet>";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/transform.xslt")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext
            {
                Diagnostics = new Container<Diagnostic>(
                    new Diagnostic
                    {
                        Message = "Missing required version attribute on xsl:stylesheet."
                    })
            }
        }, default);

        Assert.NotNull(result);
        var action = result!.Select(c => c.CodeAction).FirstOrDefault(a => a?.Title == "Add XSLT version 3.0 attribute");
        Assert.NotNull(action);
        var edit = action!.Edit!.Changes!.Single().Value.First();
        Assert.Equal(" version=\"3.0\"", edit.NewText);
    }

    [Fact]
    public async System.Threading.Tasks.Task XsltOffersNamespaceDeclarationFromXpst0081Diagnostic()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/transform.xslt").ToString();
        const string text = "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"><xsl:template match=\"my:foo\"/></xsl:stylesheet>";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/transform.xslt")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext
            {
                Diagnostics = new Container<Diagnostic>(
                    new Diagnostic
                    {
                        Message = "match: XPST0081: Prefix 'my' is not declared."
                    })
            }
        }, default);

        Assert.NotNull(result);
        var action = result!.Select(c => c.CodeAction).FirstOrDefault(a => a?.Title == "Declare namespace 'my'");
        Assert.NotNull(action);
        var edit = action!.Edit!.Changes!.Single().Value.First();
        Assert.Equal(" xmlns:my=\"\"", edit.NewText);
    }

    [Fact]
    public async System.Threading.Tasks.Task XPathOffersCloseParenthesis()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/expr.xpath").ToString();
        const string text = "fn:concat('a', 'b'";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/expr.xpath")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext()
        }, default);

        Assert.NotNull(result);
        var action = result!.Select(c => c.CodeAction).FirstOrDefault(a => a?.Title == "Close missing parenthesis");
        Assert.NotNull(action);
        var edit = action!.Edit!.Changes!.Single().Value.First();
        Assert.Equal(")", edit.NewText);
        Assert.Equal(0, edit.Range.Start.Line);
        Assert.Equal(text.Length, edit.Range.Start.Character);
    }

    [Fact]
    public async System.Threading.Tasks.Task XPathOffersCloseSquareBracket()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/expr.xpath").ToString();
        const string text = "a[b";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/expr.xpath")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext()
        }, default);

        Assert.NotNull(result);
        var action = result!.Select(c => c.CodeAction).FirstOrDefault(a => a?.Title == "Close missing square bracket");
        Assert.NotNull(action);
        var edit = action!.Edit!.Changes!.Single().Value.First();
        Assert.Equal("]", edit.NewText);
    }

    [Fact]
    public async System.Threading.Tasks.Task XPathOffersCloseSingleQuote()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/expr.xpath").ToString();
        const string text = "'abc";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/expr.xpath")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext()
        }, default);

        Assert.NotNull(result);
        var action = result!.Select(c => c.CodeAction).FirstOrDefault(a => a?.Title == "Close single-quoted string");
        Assert.NotNull(action);
        var edit = action!.Edit!.Changes!.Single().Value.First();
        Assert.Equal("'", edit.NewText);
    }

    [Fact]
    public async System.Threading.Tasks.Task XPathDoesNotOfferActionForBalancedExpression()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/expr.xpath").ToString();
        const string text = "fn:concat('a', 'b')";
        documents.Update(uri, text);

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/expr.xpath")),
            Range = new Range(new Position(0, 0), new Position(0, text.Length)),
            Context = new CodeActionContext()
        }, default);

        Assert.NotNull(result);
        Assert.DoesNotContain(result!, c => c.CodeAction?.Title.Contains("Close") == true);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReturnsEmptyForUnsupportedDocument()
    {
        var documents = new DocumentManager();
        var uri = DocumentUri.FromFileSystemPath("C:/test/unknown.txt").ToString();
        documents.Update(uri, "my:foo()");

        var handler = new CodeActionHandler(documents);
        var result = await handler.Handle(new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier(DocumentUri.FromFileSystemPath("C:/test/unknown.txt")),
            Range = new Range(new Position(0, 0), new Position(0, 8)),
            Context = new CodeActionContext()
        }, default);

        Assert.NotNull(result);
        Assert.Empty(result!);
    }
}
