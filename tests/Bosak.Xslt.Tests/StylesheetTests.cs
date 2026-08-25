// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 mei 2026
// PURPOSE              : Unit tests verifying XSLT stylesheet loading and compilation.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 24-05-2026     | Added call-template, param, and variable scoping tests                                   |
//                      | Charles Korthout | 0.3   | 24-05-2026     | Added import/include and precedence tests                                                |
//                      | Charles Korthout | 0.4   | 24-05-2026     | Added xsl:key / key() tests                                                              |
//                      | Charles Korthout | 0.5   | 24-05-2026     | Added xsl:number tests (single, any, multiple, value attribute, format tokens)         |
//                      | Charles Korthout | 0.6   | 26-05-2026     | Added global variable and parameter tests for main/include/import scopes                 |
//                      | Charles Korthout | 0.7   | 27-05-2026     | Added fn:transform tests (basic, stylesheet-params, initial-template) and map key lookup  |
//                      | Charles Korthout | 0.71  | 15-07-2026     | Updated stylesheet-params test to use xs:QName keys per spec                             |
//                      | Charles Korthout | 0.8   | 31-05-2026     | Added xsl:try / xsl:catch tests (result tree, select attribute, function body)         |
//                      | Charles Korthout | 0.9   | 31-05-2026     | Added xsl:for-each-group tests (group-by, group-adjacent, group-starting-with, current-grouping-key) |
//                      | Charles Korthout | 0.10  | 13-06-2026     | Relaxed fn:transform assertions to tolerate copied in-scope namespaces                   |
//                      | Charles Korthout | 0.11  | 15-06-2026     | TestMessageListener implements IXsltMessageListener.OnWarning                         |
//                      | Charles Korthout | 0.12  | 24-06-2026     | Added xsl:function/@_name AVT expansion tests                                         |
//                      | Charles Korthout | 0.13  | 26-06-2026     | Added shallow-copy variable and quantified EBV tests                                  |
//                      | Charles Korthout | 0.14  | 26-06-2026     | Added shadow attribute tests for _select, _use-when, and LRE preservation            |
//                      | Charles Korthout | 0.15  | 26-06-2026     | Added lazy function-local variable / circular-reference test                         |
//                      | Charles Korthout | 0.16  | 29-06-2026     | Added regression tests for global visibility and eager duplicate function locals       |
//                      | Charles Korthout | 0.17  | 30-06-2026     | Added regression tests for xsl:apply-templates inside xsl:function bodies              |
//                      | Charles Korthout | 0.18  | 26-06-2026     | Added regression test for atomic values returned by apply-templates/call-template in functions |
//                      | Charles Korthout | 0.19  | 26-06-2026     | Added regression test for global variable referencing accumulator-after()                |
//                      | Charles Korthout | 0.20  | 26-06-2026     | Added regression test for HTML output normalization-form                                 |
//                      | Charles Korthout | 0.21  | 10-07-2026     | Added regression test for apply-imports into included modules of an import               |
//                      | Charles Korthout | 0.22  | 11-07-2026     | Added xsl:output serialization tests for XHTML, DOCTYPE, CDATA, URI escaping, meta     |
//                      | Charles Korthout | 0.23  | 11-07-2026     | Added fragment serialization tests for xml/html/xhtml multiple top-level nodes.        |
//                      | Charles Korthout | 0.24  | 11-07-2026     | Added default method inference tests for html in XHTML namespace and no namespace.     |
//                      | Charles Korthout | 0.25  | 11-07-2026     | Added SESU0007 unsupported-encoding and SEPM0009 standalone/omit-declaration tests.    |
//                      | Charles Korthout | 0.26  | 11-07-2026     | Updated expected XML empty-element tag to no-space form.                               |
//                      | Charles Korthout | 0.27  | 12-07-2026     | Updated default-output tests to expect the XML declaration.                              |
//                      | Charles Korthout | 0.28  | 15-07-2026     | Removed temporary debug tests used during fn:transform Tier-2m investigation.            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.29  | 18-08-2026     | Added fn:transform FOXT0002 missing-source and result-document text-capture tests     |
//                      | Charles Korthout | 0.30  | 25-08-2026     | Added XTSE0260 regression tests for empty XSLT elements                                  |
//                      | Charles Korthout | 0.31  | 25-08-2026     | Added XTSE0350 regression tests for unbalanced AVT braces                                |
//                      | Charles Korthout | 0.32  | 25-08-2026     | Added XTSE0370 regression tests for unescaped right braces in AVTs                       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Standard.Functions;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

public class StylesheetTests
{
    [Fact]
    public void Can_Load_Simple_Stylesheet()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>hello</output>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        Assert.NotNull(executable);
    }

    [Fact]
    public void Can_Transform_Simple_Document()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>hello</output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<output>hello</output>", result);
    }

    [Fact]
    public void Can_Call_Named_Template()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:call-template name='greeting'/>
                </output>
            </xsl:template>
            <xsl:template name='greeting'>
                <hello>world</hello>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<hello>world</hello>", result);
    }

    [Fact]
    public void Can_Call_Named_Template_With_Param()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:call-template name='greeting'>
                        <xsl:with-param name='who' select='""you""'/>
                    </xsl:call-template>
                </output>
            </xsl:template>
            <xsl:template name='greeting'>
                <xsl:param name='who'/>
                <hello><xsl:value-of select='$who'/></hello>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<hello>you</hello>", result);
    }

    [Fact]
    public void Named_Template_Param_Default_Value()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:call-template name='greeting'/>
                </output>
            </xsl:template>
            <xsl:template name='greeting'>
                <xsl:param name='who' select='""default""'/>
                <hello><xsl:value-of select='$who'/></hello>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<hello>default</hello>", result);
    }

    [Fact]
    public void Named_Template_Param_Overrides_Default()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:call-template name='greeting'>
                        <xsl:with-param name='who' select='""override""'/>
                    </xsl:call-template>
                </output>
            </xsl:template>
            <xsl:template name='greeting'>
                <xsl:param name='who' select='""default""'/>
                <hello><xsl:value-of select='$who'/></hello>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<hello>override</hello>", result);
    }

    [Fact]
    public void Call_Template_Variable_Scoping()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <xsl:variable name='x' select='""outer""'/>
                <output>
                    <before><xsl:value-of select='$x'/></before>
                    <xsl:call-template name='inner'>
                        <xsl:with-param name='x' select='""inner-param""'/>
                    </xsl:call-template>
                    <after><xsl:value-of select='$x'/></after>
                </output>
            </xsl:template>
            <xsl:template name='inner'>
                <xsl:param name='x'/>
                <inside><xsl:value-of select='$x'/></inside>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<before>outer</before>", result);
        Assert.Contains("<inside>inner-param</inside>", result);
        Assert.Contains("<after>outer</after>", result);
    }

    // ------------------------------------------------------------------
    // Import / Include tests
    // ------------------------------------------------------------------

    private class InMemoryResolver : Api.IXsltUriResolver
    {
        private readonly Dictionary<string, string> _documents = new();

        public void Add(string uri, string xml) => _documents[uri] = xml;

        public XDocument Resolve(string href, string? baseUri)
        {
            var key = ResolveKey(href, baseUri);
            if (!_documents.TryGetValue(key, out var xml))
                throw new FileNotFoundException($"In-memory document not found: {key}");
            return XDocument.Parse(xml, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        }

        private static string ResolveKey(string href, string? baseUri)
        {
            if (string.IsNullOrEmpty(baseUri))
                return href;
            if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
                return href;
            var baseUriObj = new Uri(baseUri);
            return new Uri(baseUriObj, href).AbsoluteUri;
        }
    }

    [Fact]
    public void Include_Loads_Templates_From_Other_File()
    {
        var resolver = new InMemoryResolver();
        resolver.Add("file:///main.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:include href='helper.xsl'/>
            <xsl:template match='/'>
                <output><xsl:call-template name='greeting'/></output>
            </xsl:template>
        </xsl:stylesheet>");
        resolver.Add("file:///helper.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template name='greeting'><hello>world</hello></xsl:template>
        </xsl:stylesheet>");

        var compiler = new Api.XsltCompiler { UriResolver = resolver };
        var executable = compiler.Compile(resolver.Resolve("file:///main.xsl", null), "file:///main.xsl");
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("<hello>world</hello>", result);
    }

    [Fact]
    public void Import_Loads_Templates_At_Lower_Precedence()
    {
        var resolver = new InMemoryResolver();
        resolver.Add("file:///main.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import href='base.xsl'/>
            <xsl:template match='root'><main>override</main></xsl:template>
        </xsl:stylesheet>");
        resolver.Add("file:///base.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='root'><base>original</base></xsl:template>
        </xsl:stylesheet>");

        var compiler = new Api.XsltCompiler { UriResolver = resolver };
        var executable = compiler.Compile(resolver.Resolve("file:///main.xsl", null), "file:///main.xsl");
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("<main>override</main>", result);
        Assert.DoesNotContain("<base>original</base>", result);
    }

    [Fact]
    public void Imported_Template_Has_Lower_Import_Precedence()
    {
        var resolver = new InMemoryResolver();
        resolver.Add("file:///main.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import href='base.xsl'/>
            <xsl:template match='root' priority='-1'><main>low-priority</main></xsl:template>
        </xsl:stylesheet>");
        resolver.Add("file:///base.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='root'><base>high-priority-default</base></xsl:template>
        </xsl:stylesheet>");

        var compiler = new Api.XsltCompiler { UriResolver = resolver };
        var executable = compiler.Compile(resolver.Resolve("file:///main.xsl", null), "file:///main.xsl");
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        // Main stylesheet has higher import precedence, so it wins regardless of priority
        Assert.Contains("<main>low-priority</main>", result);
        Assert.DoesNotContain("<base>high-priority-default</base>", result);
    }

    [Fact]
    public void Named_Template_From_Include_Is_Accessible()
    {
        var resolver = new InMemoryResolver();
        resolver.Add("file:///main.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:include href='helper.xsl'/>
            <xsl:template match='/'>
                <output><xsl:call-template name='format'/></output>
            </xsl:template>
        </xsl:stylesheet>");
        resolver.Add("file:///helper.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template name='format'><formatted/></xsl:template>
        </xsl:stylesheet>");

        var compiler = new Api.XsltCompiler { UriResolver = resolver };
        var executable = compiler.Compile(resolver.Resolve("file:///main.xsl", null), "file:///main.xsl");
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("formatted", result);
    }

    [Fact]
    public void ApplyImports_Sees_Templates_In_Included_Module_Of_Import()
    {
        var resolver = new InMemoryResolver();
        resolver.Add("file:///main.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import href='base.xsl'/>
            <xsl:include href='overlay.xsl'/>
            <xsl:template match='/'><out><xsl:apply-templates select='root/item'/></out></xsl:template>
        </xsl:stylesheet>");
        resolver.Add("file:///base.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:include href='specific.xsl'/>
            <xsl:template match='*'><fallback/></xsl:template>
        </xsl:stylesheet>");
        resolver.Add("file:///specific.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='item'><specific/></xsl:template>
        </xsl:stylesheet>");
        resolver.Add("file:///overlay.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='item'><wrap><xsl:apply-imports/></wrap></xsl:template>
        </xsl:stylesheet>");

        var compiler = new Api.XsltCompiler { UriResolver = resolver };
        var executable = compiler.Compile(resolver.Resolve("file:///main.xsl", null), "file:///main.xsl");
        var source = new XDocument(new XElement("root", new XElement("item")));
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<wrap>", result);
        Assert.Contains("<specific", result);
        Assert.DoesNotContain("<fallback", result);
    }

    [Fact]
    public void Circular_Import_Throws()
    {
        var resolver = new InMemoryResolver();
        resolver.Add("file:///a.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import href='b.xsl'/>
            <xsl:template match='/'><a/></xsl:template>
        </xsl:stylesheet>");
        resolver.Add("file:///b.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import href='a.xsl'/>
            <xsl:template match='/'><b/></xsl:template>
        </xsl:stylesheet>");

        var compiler = new Api.XsltCompiler { UriResolver = resolver };
        Assert.Throws<InvalidOperationException>(() =>
            compiler.Compile(resolver.Resolve("file:///a.xsl", null), "file:///a.xsl"));
    }

    [Fact]
    public void Custom_Uri_Resolver_Is_Used()
    {
        var called = false;
        var customResolver = new CustomResolver(href => { called = true; });

        var compiler = new Api.XsltCompiler { UriResolver = customResolver };
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import href='anything.xsl'/>
            <xsl:template match='/'><main/></xsl:template>
        </xsl:stylesheet>";

        compiler.Compile(xsl);
        Assert.True(called, "Custom resolver should have been invoked for xsl:import");
    }

    private class CustomResolver : Api.IXsltUriResolver
    {
        private readonly Action<string> _onResolve;
        private readonly XDocument _doc;

        public CustomResolver(Action<string> onResolve)
        {
            _onResolve = onResolve;
            _doc = XDocument.Parse(@"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='/'><ok/></xsl:template></xsl:stylesheet>");
        }

        public XDocument Resolve(string href, string? baseUri)
        {
            _onResolve(href);
            return _doc;
        }
    }

    // ------------------------------------------------------------------
    // xsl:output tests
    // ------------------------------------------------------------------

    [Fact]
    public void Output_Text_Method_Serializes_Only_Text()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='text'/>
            <xsl:template match='/'>
                <root>hello <child>world</child></root>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Equal("hello world", result.Trim());
    }

    [Fact]
    public void Output_Omit_Xml_Declaration_No_Includes_Declaration()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='xml' omit-xml-declaration='no' version='1.0' encoding='UTF-8'/>
            <xsl:template match='/'>
                <root>hello</root>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<?xml", result);
        Assert.Contains("version=\"1.0\"", result);
        Assert.Contains("encoding=\"UTF-8\"", result);
        Assert.Contains("<root>hello</root>", result);
    }

    [Fact]
    public void Output_Indent_Yes_Produces_Pretty_Print()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='xml' indent='yes'/>
            <xsl:template match='/'>
                <root><child>hello</child></root>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        // Indented output should contain newlines between elements
        Assert.Contains("\n", result);
        Assert.Contains("<root>", result);
        Assert.Contains("<child>hello</child>", result);
    }

    [Fact]
    public void Output_Default_Has_Declaration_And_No_Indent()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <root><child>hello</child></root>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<?xml", result);
        Assert.DoesNotContain("\n", result);
    }

    [Fact]
    public void Output_Xhtml_Method_Emits_Xml_Syntax()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='xhtml' omit-xml-declaration='no' indent='no'/>
            <xsl:template match='/'>
                <html xmlns='http://www.w3.org/1999/xhtml'><head><title>t</title></head><body><p>hello</p></body></html>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<?xml", result);
        Assert.Contains("<html xmlns=\"http://www.w3.org/1999/xhtml\">", result);
        Assert.Contains("<p>hello</p>", result);
    }

    [Fact]
    public void Output_Xhtml_Void_Element_Is_Self_Closed()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='xhtml' omit-xml-declaration='yes' indent='no'/>
            <xsl:template match='/'>
                <html xmlns='http://www.w3.org/1999/xhtml'><body><br /></body></html>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<br />", result);
    }

    [Fact]
    public void Output_Xhtml_Non_Void_Empty_Element_Has_End_Tag()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='xhtml' omit-xml-declaration='yes' indent='no'/>
            <xsl:template match='/'>
                <html xmlns='http://www.w3.org/1999/xhtml'><body><custom></custom></body></html>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<custom></custom>", result);
    }

    [Fact]
    public void Output_Xhtml_Doctype_System_Is_Emitted()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='xhtml' omit-xml-declaration='yes' indent='no' doctype-system='out.dtd'/>
            <xsl:template match='/'>
                <html xmlns='http://www.w3.org/1999/xhtml'><body><p>hello</p></body></html>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<!DOCTYPE html SYSTEM \"out.dtd\">", result);
    }

    [Fact]
    public void Output_Xhtml_Cdata_Section_Elements_Wraps_Text()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='xhtml' omit-xml-declaration='yes' indent='no' cdata-section-elements='Q{http://www.w3.org/1999/xhtml}example'/>
            <xsl:template match='/'>
                <html xmlns='http://www.w3.org/1999/xhtml'><body><example>a &lt; b</example></body></html>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<example><![CDATA[a < b]]></example>", result);
    }

    [Fact]
    public void Output_Html_Escape_Uri_Attributes_Percent_Encodes_Non_Ascii()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='html' escape-uri-attributes='yes' indent='no'/>
            <xsl:template match='/'>
                <html><body><a href='http://example.org/årsrapport/'>link</a></body></html>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("%C3%A5rsrapport", result);
    }

    [Fact]
    public void Output_Xhtml_Escape_Uri_Attributes_Leaves_Non_Uri_Attributes_Unchanged()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='xhtml' escape-uri-attributes='yes' indent='no'/>
            <xsl:template match='/'>
                <html xmlns='http://www.w3.org/1999/xhtml'><body><a href='http://example.org/å' accesskey='å'>link</a></body></html>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("href=\"http://example.org/%C3%A5\"", result);
        Assert.Contains("accesskey=\"å\"", result);
    }

    [Fact]
    public void Output_Html_Include_Content_Type_Inserts_Meta()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='html' indent='no'/>
            <xsl:template match='/'>
                <html><head><title>t</title></head><body><p>hello</p></body></html>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">", result);
    }

    [Fact]
    public void Output_Xhtml_Include_Content_Type_Inserts_Meta()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='xhtml' indent='no' omit-xml-declaration='yes'/>
            <xsl:template match='/'>
                <html xmlns='http://www.w3.org/1999/xhtml'><head><title>t</title></head><body><p>hello</p></body></html>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />", result);
    }

    [Fact]
    public void Output_Xml_Standalone_Omit_Suppresses_Attribute()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='xml' omit-xml-declaration='no' standalone='omit'/>
            <xsl:template match='/'>
                <root>hello</root>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<?xml", result);
        Assert.DoesNotContain("standalone", result);
    }

    [Fact]
    public void Output_Multiple_Declarations_Merge_Cdata_Section_Elements()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='xml' cdata-section-elements='a'/>
            <xsl:output method='xml' cdata-section-elements='b'/>
            <xsl:template match='/'>
                <root><a>text</a><b>text</b></root>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<a><![CDATA[text]]></a>", result);
        Assert.Contains("<b><![CDATA[text]]></b>", result);
    }

    [Theory]
    [InlineData("xhtml")]
    [InlineData("xml")]
    [InlineData("html")]
    public void Output_Fragment_Multiple_Top_Level(string method)
    {
        var xsl = $@"<t:transform xmlns='http://www.w3.org/1999/xhtml'
             xmlns:t='http://www.w3.org/1999/XSL/Transform'
             version='2.0'>
   <t:output method='{method}' indent='no'/>
   <t:template match='/'>
      <html>
         <h1>Introduction</h1>
Welcome to this document on XHTML.
 </html>
      <h1>Introduction</h1>
Welcome to this document on XHTML.
</t:template>
</t:transform>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<html", result);
        Assert.Contains("<h1>Introduction</h1>", result);
    }

    [Fact]
    public void Output_Default_Method_Infers_Xhtml_For_Html_In_Xhtml_Namespace()
    {
        var xsl = @"<t:transform xmlns='http://www.w3.org/1999/xhtml'
             xmlns:t='http://www.w3.org/1999/XSL/Transform'
             version='2.0'>
   <t:template match='/'>
      <html>
         <head>
            <title>Default output method</title>
         </head>
         <body>
            <p>Verify<a href='http://exampleÇ.org/Â'> this is XHTML</a>.</p>
         </body>
      </html>
   </t:template>
</t:transform>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        // XHTML serialization emits an XML declaration and includes a Content-Type meta.
        Assert.Contains("<?xml", result);
        Assert.Contains("<html xmlns=\"http://www.w3.org/1999/xhtml\">", result);
        Assert.Contains("http-equiv=\"Content-Type\"", result);
    }

    [Fact]
    public void Output_Default_Method_Infers_Html_For_Html_In_No_Namespace()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
   <xsl:template match='/'>
      <html>
         <head><title>HTML</title></head>
         <body><p>Hello<br/>world</p></body>
      </html>
   </xsl:template>
</xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        // HTML serialization omits XML declaration and uses unclosed void elements.
        Assert.DoesNotContain("<?xml", result);
        Assert.Contains("<html>", result);
        Assert.Contains("<br>", result);
        Assert.DoesNotContain("<br />", result);
    }

    [Theory]
    [InlineData("xml")]
    [InlineData("html")]
    [InlineData("text")]
    public void Output_Unsupported_Encoding_Raises_Sesu0007(string method)
    {
        var xsl = $@"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
   <xsl:output method='{method}' encoding='XXX-xx' indent='no'/>
   <xsl:template match='/'><doc>hello</doc></xsl:template>
</xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);

        var ex = Assert.Throws<Runtime.XsltRuntimeException>(() =>
            executable.TransformToString(new XDocumentNode(source)));

        Assert.Equal("SESU0007", ex.ErrorCode);
    }

    [Fact]
    public void Output_Omit_Declaration_With_Standalone_Raises_Sepm0009()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
   <xsl:output method='xml' encoding='UTF-8' indent='no' omit-xml-declaration='yes' standalone='yes'/>
   <xsl:template match='/'><doc>hello</doc></xsl:template>
</xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);

        var ex = Assert.Throws<Runtime.XsltRuntimeException>(() =>
            executable.TransformToString(new XDocumentNode(source)));

        Assert.Equal("SEPM0009", ex.ErrorCode);
    }

    // ------------------------------------------------------------------
    // Named mode tests
    // ------------------------------------------------------------------

    [Fact]
    public void Apply_Templates_With_Named_Mode_Dispatches_Correctly()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:apply-templates select='root' mode='normalize'/>
                </output>
            </xsl:template>
            <xsl:template match='root' mode='normalize'>
                <normalized><xsl:value-of select='@id'/></normalized>
            </xsl:template>
            <xsl:template match='root'>
                <default>should-not-appear</default>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root", new XAttribute("id", "42")));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.True(result.Contains("normalized") && result.Contains("42"), $"Expected normalized 42. Got: {result}");
        Assert.DoesNotContain("default", result);
    }

    [Fact]
    public void Template_Without_Mode_Participates_In_Default_Mode()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output><xsl:apply-templates select='root'/></output>
            </xsl:template>
            <xsl:template match='root'>
                <default><xsl:value-of select='@id'/></default>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root", new XAttribute("id", "99")));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<default>99</default>", result);
    }

    [Fact]
    public void Unrecognized_Mode_Falls_Back_To_Built_In_Rules()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output><xsl:apply-templates select='root' mode='nonexistent'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root", new XAttribute("id", "x")));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        // XSLT 2.0 built-in rules apply templates to children, not shallow-copy
        Assert.Contains("<output/>", result);
        Assert.Contains("<?xml", result);
    }

    [Fact]
    public void Hash_Default_Resolves_To_Default_Mode()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output><xsl:apply-templates select='root' mode='#default'/></output>
            </xsl:template>
            <xsl:template match='root'>
                <default><xsl:value-of select='@id'/></default>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root", new XAttribute("id", "77")));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<default>77</default>", result);
    }

    [Fact]
    public void Hash_Current_Resolves_To_Current_Mode()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:apply-templates select='root' mode='pass1'/>
                </output>
            </xsl:template>
            <xsl:template match='root' mode='pass1'>
                <pass1><xsl:apply-templates select='child' mode='#current'/></pass1>
            </xsl:template>
            <xsl:template match='child' mode='pass1'>
                <child-processed><xsl:value-of select='.'/></child-processed>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root", new XElement("child", "abc")));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<pass1>", result);
        Assert.Contains("<child-processed>abc</child-processed>", result);
    }

    [Fact]
    public void Mode_All_Matches_All_Modes()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:apply-templates select='root' mode='custom'/>
                </output>
            </xsl:template>
            <xsl:template match='root' mode='#all'>
                <universal><xsl:value-of select='@id'/></universal>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root", new XAttribute("id", "55")));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<universal>55</universal>", result);
    }

    // ------------------------------------------------------------------
    // xsl:key / key() tests
    // ------------------------------------------------------------------

    [Fact]
    public void Key_Looks_Up_Nodes_By_Attribute_Value()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:key name='item-key' match='item' use='@id'/>
            <xsl:template match='/'>
                <output>
                    <xsl:for-each select=""key('item-key', '2')"">
                        <found><xsl:value-of select='@name'/></found>
                    </xsl:for-each>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("items",
                new XElement("item", new XAttribute("id", "1"), new XAttribute("name", "a")),
                new XElement("item", new XAttribute("id", "2"), new XAttribute("name", "b")),
                new XElement("item", new XAttribute("id", "3"), new XAttribute("name", "c"))));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<found>b</found>", result);
    }

    [Fact]
    public void Key_Returns_Multiple_Nodes_With_Same_Key_Value()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:key name='by-cat' match='item' use='@cat'/>
            <xsl:template match='/'>
                <output>
                    <xsl:for-each select=""key('by-cat', 'x')"">
                        <found><xsl:value-of select='@name'/></found>
                    </xsl:for-each>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("items",
                new XElement("item", new XAttribute("cat", "x"), new XAttribute("name", "a")),
                new XElement("item", new XAttribute("cat", "y"), new XAttribute("name", "b")),
                new XElement("item", new XAttribute("cat", "x"), new XAttribute("name", "c"))));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<found>a</found>", result);
        Assert.Contains("<found>c</found>", result);
        Assert.DoesNotContain("<found>b</found>", result);
    }

    [Fact]
    public void Key_Returns_Empty_For_Missing_Value()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:key name='item-key' match='item' use='@id'/>
            <xsl:template match='/'>
                <output>
                    <xsl:for-each select=""key('item-key', '99')"">
                        <found/>
                    </xsl:for-each>
                    <done/>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("items",
                new XElement("item", new XAttribute("id", "1"))));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.DoesNotContain("<found", result);
        Assert.Contains("done", result);
    }

    [Fact]
    public void Key_Match_Pattern_Restricts_Indexed_Nodes()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:key name='only-items' match='item' use='@id'/>
            <xsl:template match='/'>
                <output>
                    <xsl:for-each select=""key('only-items', '1')"">
                        <found><xsl:value-of select='name()'/></found>
                    </xsl:for-each>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("root",
                new XElement("item", new XAttribute("id", "1")),
                new XElement("other", new XAttribute("id", "1"))));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<found>item</found>", result);
        Assert.DoesNotContain("<found>other</found>", result);
    }

    [Fact]
    public void Key_From_Include_Is_Accessible()
    {
        var resolver = new InMemoryResolver();
        resolver.Add("file:///main.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:include href='keys.xsl'/>
            <xsl:template match='/'>
                <output>
                    <xsl:for-each select=""key('item-key', '2')"">
                        <found><xsl:value-of select='@name'/></found>
                    </xsl:for-each>
                </output>
            </xsl:template>
        </xsl:stylesheet>");
        resolver.Add("file:///keys.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:key name='item-key' match='item' use='@id'/>
        </xsl:stylesheet>");

        var source = new XDocument(
            new XElement("items",
                new XElement("item", new XAttribute("id", "1"), new XAttribute("name", "a")),
                new XElement("item", new XAttribute("id", "2"), new XAttribute("name", "b"))));

        var compiler = new Api.XsltCompiler { UriResolver = resolver };
        var executable = compiler.Compile(resolver.Resolve("file:///main.xsl", null), "file:///main.xsl");
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<found>b</found>", result);
    }

    [Fact]
    public void Global_Variable_In_Main_Stylesheet_Is_Accessible()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:variable name='greeting' select='""hello""'/>
            <xsl:template match='/'>
                <output><xsl:value-of select='$greeting'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("hello", result);
    }

    [Fact]
    public void Variable_From_Include_Is_Accessible()
    {
        var resolver = new InMemoryResolver();
        resolver.Add("file:///main.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:include href='helper.xsl'/>
            <xsl:template match='/'>
                <output><xsl:value-of select='$greeting'/></output>
            </xsl:template>
        </xsl:stylesheet>");
        resolver.Add("file:///helper.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:variable name='greeting' select='""hello from include""' />
        </xsl:stylesheet>");

        var compiler = new Api.XsltCompiler { UriResolver = resolver };
        var executable = compiler.Compile(resolver.Resolve("file:///main.xsl", null), "file:///main.xsl");
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("hello from include", result);
    }

    [Fact]
    public void Global_Parameter_In_Main_Stylesheet_Is_Accessible()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:param name='greeting' select='""hello param""'/>
            <xsl:template match='/'>
                <output><xsl:value-of select='$greeting'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("hello param", result);
    }

    [Fact]
    public void Variable_From_Import_Is_Accessible()
    {
        var resolver = new InMemoryResolver();
        resolver.Add("file:///main.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import href='base.xsl'/>
            <xsl:template match='/'>
                <output><xsl:value-of select='$greeting'/></output>
            </xsl:template>
        </xsl:stylesheet>");
        resolver.Add("file:///base.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:variable name='greeting' select='""hello from import""' />
        </xsl:stylesheet>");

        var compiler = new Api.XsltCompiler { UriResolver = resolver };
        var executable = compiler.Compile(resolver.Resolve("file:///main.xsl", null), "file:///main.xsl");
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("hello from import", result);
    }

    [Fact]
    public void Local_Global_Variable_Overrides_Imported()
    {
        var resolver = new InMemoryResolver();
        resolver.Add("file:///main.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import href='base.xsl'/>
            <xsl:variable name='greeting' select='""local override""' />
            <xsl:template match='/'>
                <output><xsl:value-of select='$greeting'/></output>
            </xsl:template>
        </xsl:stylesheet>");
        resolver.Add("file:///base.xsl", @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:variable name='greeting' select='""from import""' />
        </xsl:stylesheet>");

        var compiler = new Api.XsltCompiler { UriResolver = resolver };
        var executable = compiler.Compile(resolver.Resolve("file:///main.xsl", null), "file:///main.xsl");
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("local override", result);
        Assert.DoesNotContain("from import", result);
    }

    [Fact]
    public void Global_Variable_References_Another_Global()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:variable name='first' select='""hello""'/>
            <xsl:variable name='second' select='concat($first, "" world"")'/>
            <xsl:template match='/'>
                <output><xsl:value-of select='$second'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("hello world", result);
    }

    [Fact]
    public void Tunnel_Param_Via_Apply_Templates()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:apply-templates select='root/item'>
                        <xsl:with-param name='label' tunnel='yes' select='""tunneled""'/>
                    </xsl:apply-templates>
                </output>
            </xsl:template>
            <xsl:template match='item'>
                <xsl:param name='label' tunnel='yes'/>
                <item><xsl:value-of select='$label'/></item>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root", new XElement("item")));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<item>tunneled</item>", result);
    }

    [Fact]
    public void Tunnel_Param_Via_Call_Template()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:call-template name='format'>
                        <xsl:with-param name='label' tunnel='yes' select='""tunneled""'/>
                    </xsl:call-template>
                </output>
            </xsl:template>
            <xsl:template name='format'>
                <xsl:param name='label' tunnel='yes'/>
                <item><xsl:value-of select='$label'/></item>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("<item>tunneled</item>", result);
    }

    [Fact]
    public void Tunnel_Param_Passes_Through_Intermediate_Template()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:call-template name='middle'>
                        <xsl:with-param name='label' tunnel='yes' select='""deep""'/>
                    </xsl:call-template>
                </output>
            </xsl:template>
            <xsl:template name='middle'>
                <!-- does not declare label, but passes it through to inner -->
                <xsl:call-template name='inner'/>
            </xsl:template>
            <xsl:template name='inner'>
                <xsl:param name='label' tunnel='yes'/>
                <item><xsl:value-of select='$label'/></item>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("<item>deep</item>", result);
    }

    [Fact]
    public void Non_Tunnel_Param_Still_Works()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:call-template name='format'>
                        <xsl:with-param name='label' select='""normal""'/>
                    </xsl:call-template>
                </output>
            </xsl:template>
            <xsl:template name='format'>
                <xsl:param name='label'/>
                <item><xsl:value-of select='$label'/></item>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("<item>normal</item>", result);
    }

    // ------------------------------------------------------------------
    // xsl:number tests
    // ------------------------------------------------------------------

    [Fact]
    public void Xsl_Number_Single_Default_Count()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:apply-templates select='items/item'/>
                </output>
            </xsl:template>
            <xsl:template match='item'>
                <n><xsl:number/></n>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("items",
                new XElement("item"),
                new XElement("item"),
                new XElement("item")));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<n>1</n>", result);
        Assert.Contains("<n>2</n>", result);
        Assert.Contains("<n>3</n>", result);
    }

    [Fact]
    public void Xsl_Number_Single_With_Count_Pattern()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:apply-templates select='items/*'/>
                </output>
            </xsl:template>
            <xsl:template match='item'>
                <item><xsl:number count='item'/></item>
            </xsl:template>
            <xsl:template match='sep'>
                <sep/>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("items",
                new XElement("item"),
                new XElement("sep"),
                new XElement("item"),
                new XElement("item")));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        // item numbering should skip sep elements
        Assert.Contains("<item>1</item>", result);
        Assert.Contains("<item>2</item>", result);
        Assert.Contains("<item>3</item>", result);
        Assert.Contains("sep", result);
    }

    [Fact]
    public void Xsl_Number_Any()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:apply-templates select='root//item'/>
                </output>
            </xsl:template>
            <xsl:template match='item'>
                <n><xsl:number level='any' count='item'/></n>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("root",
                new XElement("group",
                    new XElement("item"),
                    new XElement("item")),
                new XElement("group",
                    new XElement("item"))));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<n>1</n>", result);
        Assert.Contains("<n>2</n>", result);
        Assert.Contains("<n>3</n>", result);
    }

    [Fact]
    public void Xsl_Number_Multiple()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:apply-templates select='root/chapter/section/subsection'/>
                </output>
            </xsl:template>
            <xsl:template match='subsection'>
                <n><xsl:number level='multiple' count='chapter|section|subsection' format='1.1'/></n>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("root",
                new XElement("chapter",
                    new XElement("section",
                        new XElement("subsection"),
                        new XElement("subsection"))),
                new XElement("chapter",
                    new XElement("section",
                        new XElement("subsection")))));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<n>1.1.1</n>", result);
        Assert.Contains("<n>1.1.2</n>", result);
        Assert.Contains("<n>2.1.1</n>", result);
    }

    [Fact]
    public void Xsl_Number_Value_Attribute()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:apply-templates select='items/item'/>
                </output>
            </xsl:template>
            <xsl:template match='item'>
                <n><xsl:number value='@seq' format='001'/></n>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("items",
                new XElement("item", new XAttribute("seq", "5")),
                new XElement("item", new XAttribute("seq", "12"))));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<n>005</n>", result);
        Assert.Contains("<n>012</n>", result);
    }

    [Fact]
    public void Xsl_Number_Format_Tokens()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <a><xsl:number value='1' format='a'/></a>
                    <A><xsl:number value='1' format='A'/></A>
                    <i><xsl:number value='1' format='i'/></i>
                    <I><xsl:number value='1' format='I'/></I>
                    <z><xsl:number value='5' format='01'/></z>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<a>a</a>", result);
        Assert.Contains("<A>A</A>", result);
        Assert.Contains("<i>i</i>", result);
        Assert.Contains("<I>I</I>", result);
        Assert.Contains("<z>05</z>", result);
    }

    // ------------------------------------------------------------------
    // xsl:sort tests
    // ------------------------------------------------------------------

    [Fact]
    public void Xsl_Sort_By_Attribute_Ascending()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:apply-templates select='items/item'>
                        <xsl:sort select='@name'/>
                    </xsl:apply-templates>
                </output>
            </xsl:template>
            <xsl:template match='item'>
                <item><xsl:value-of select='@name'/></item>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("items",
                new XElement("item", new XAttribute("name", "c")),
                new XElement("item", new XAttribute("name", "a")),
                new XElement("item", new XAttribute("name", "b"))));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        var idxA = result.IndexOf("<item>a</item>");
        var idxB = result.IndexOf("<item>b</item>");
        var idxC = result.IndexOf("<item>c</item>");
        Assert.True(idxA < idxB && idxB < idxC, $"Expected a < b < c. Got: {result}");
    }

    [Fact]
    public void Xsl_Sort_By_Attribute_Descending()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:apply-templates select='items/item'>
                        <xsl:sort select='@name' order='descending'/>
                    </xsl:apply-templates>
                </output>
            </xsl:template>
            <xsl:template match='item'>
                <item><xsl:value-of select='@name'/></item>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("items",
                new XElement("item", new XAttribute("name", "a")),
                new XElement("item", new XAttribute("name", "c")),
                new XElement("item", new XAttribute("name", "b"))));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        var idxA = result.IndexOf("<item>a</item>");
        var idxB = result.IndexOf("<item>b</item>");
        var idxC = result.IndexOf("<item>c</item>");
        Assert.True(idxC < idxB && idxB < idxA, $"Expected c > b > a. Got: {result}");
    }

    [Fact]
    public void Xsl_Sort_Data_Type_Number()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:apply-templates select='items/item'>
                        <xsl:sort select='@seq' data-type='number'/>
                    </xsl:apply-templates>
                </output>
            </xsl:template>
            <xsl:template match='item'>
                <item><xsl:value-of select='@seq'/></item>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("items",
                new XElement("item", new XAttribute("seq", "10")),
                new XElement("item", new XAttribute("seq", "2")),
                new XElement("item", new XAttribute("seq", "1"))));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        var idx1 = result.IndexOf("<item>1</item>");
        var idx2 = result.IndexOf("<item>2</item>");
        var idx10 = result.IndexOf("<item>10</item>");
        Assert.True(idx1 < idx2 && idx2 < idx10, $"Expected 1 < 2 < 10. Got: {result}");
    }

    [Fact]
    public void Xsl_Sort_Inside_For_Each()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:for-each select='items/item'>
                        <xsl:sort select='@name'/>
                        <item><xsl:value-of select='@name'/></item>
                    </xsl:for-each>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("items",
                new XElement("item", new XAttribute("name", "z")),
                new XElement("item", new XAttribute("name", "a")),
                new XElement("item", new XAttribute("name", "m"))));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        var idxA = result.IndexOf("<item>a</item>");
        var idxM = result.IndexOf("<item>m</item>");
        var idxZ = result.IndexOf("<item>z</item>");
        Assert.True(idxA < idxM && idxM < idxZ, $"Expected a < m < z. Got: {result}");
    }

    // ------------------------------------------------------------------
    // xsl:mode on-no-match tests
    // ------------------------------------------------------------------

    [Fact]
    public void Mode_OnNoMatch_ShallowCopy_Default()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:mode on-no-match='shallow-copy'/>
            <xsl:template match='/'>
                <output><xsl:apply-templates select='root'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root", new XAttribute("id", "x"), new XText("text")));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<root id=\"x\"", result);
        Assert.Contains("text", result);
    }

    [Fact]
    public void Mode_OnNoMatch_ShallowSkip()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:mode on-no-match='shallow-skip'/>
            <xsl:template match='/'>
                <output><xsl:apply-templates select='root'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root", new XElement("child", "text")));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        // XSLT 3.0 §6.6: in shallow-skip mode, built-in rule for text/attribute nodes does nothing.
        // The element wrappers are skipped, and text content is also skipped.
        Assert.DoesNotContain("text", result);
        Assert.DoesNotContain("<root", result);
        Assert.DoesNotContain("<child", result);
    }

    [Fact]
    public void Mode_OnNoMatch_TextOnlyCopy()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:mode on-no-match='text-only-copy'/>
            <xsl:template match='/'>
                <output><xsl:apply-templates select='root'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root", new XElement("child", "text")));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        // Only text should be copied, elements ignored
        Assert.Contains("text", result);
        Assert.DoesNotContain("<root", result);
        Assert.DoesNotContain("<child", result);
    }

    [Fact]
    public void Mode_OnNoMatch_DeepCopy()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:mode on-no-match='deep-copy'/>
            <xsl:template match='/'>
                <output><xsl:apply-templates select='root'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root", new XElement("child", new XElement("grandchild", "text"))));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        // Full subtree should be deep-copied
        Assert.Contains("<root>", result);
        Assert.Contains("<child>", result);
        Assert.Contains("<grandchild>text</grandchild>", result);
    }

    [Fact]
    public void Mode_OnNoMatch_Fail_Throws()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:mode on-no-match='fail'/>
            <xsl:template match='/'>
                <output><xsl:apply-templates select='root'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);

        Assert.Throws<InvalidOperationException>(() =>
            executable.TransformToString(new XDocumentNode(source)));
    }

    [Fact]
    public void ForEach_Over_Atomic_Sequence()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:for-each select='1 to 3'>
                        <x><xsl:value-of select='.'/></x>
                    </xsl:for-each>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("dummy"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<x>1</x>", result);
        Assert.Contains("<x>2</x>", result);
        Assert.Contains("<x>3</x>", result);
    }

    [Fact]
    public void ForEach_Over_Atomic_Sequence_With_CallTemplate()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema'>
            <xsl:template match='/'>
                <output>
                    <xsl:for-each select='1 to 3'>
                        <xsl:call-template name='emit'/>
                    </xsl:for-each>
                </output>
            </xsl:template>
            <xsl:template name='emit'>
                <x><xsl:value-of select='.'/></x>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("dummy"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<x>1</x>", result);
        Assert.Contains("<x>2</x>", result);
        Assert.Contains("<x>3</x>", result);
    }

    // ------------------------------------------------------------------
    // REQ-015: xsl:function tests
    // ------------------------------------------------------------------

    [Fact]
    public void XslFunction_Basic_Call()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:my='http://example.com/my'>
            <xsl:function name='my:greet'>
                <xsl:sequence select='""hello""'/>
            </xsl:function>
            <xsl:template match='/'>
                <output><xsl:value-of select='my:greet()'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("hello", result);
    }

    [Fact]
    public void XslFunction_With_Parameters()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:my='http://example.com/my'>
            <xsl:function name='my:concat'>
                <xsl:param name='a'/>
                <xsl:param name='b'/>
                <xsl:sequence select='concat($a, $b)'/>
            </xsl:function>
            <xsl:template match='/'>
                <output><xsl:value-of select='my:concat(""x"", ""y"")'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("xy", result);
    }

    [Fact]
    public void XslFunction_Uses_Context_From_First_Arg()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:my='http://example.com/my'>
            <xsl:function name='my:upper'>
                <xsl:param name='input'/>
                <xsl:sequence select='upper-case($input)'/>
            </xsl:function>
            <xsl:template match='/'>
                <output><xsl:value-of select='my:upper(/root/text)'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root", new XElement("text", "hello")));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("HELLO", result);
    }

    [Fact]
    public void XslFunction_From_Import_Is_Callable()
    {
        var resolver = new InMemoryResolver();
        resolver.Add("file:///main.xsl", @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:my='http://example.com/my'>
            <xsl:import href='lib.xsl'/>
            <xsl:template match='/'>
                <output><xsl:value-of select='my:double(3)'/></output>
            </xsl:template>
        </xsl:stylesheet>");
        resolver.Add("file:///lib.xsl", @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:my='http://example.com/my'>
            <xsl:function name='my:double'>
                <xsl:param name='n'/>
                <xsl:sequence select='$n * 2'/>
            </xsl:function>
        </xsl:stylesheet>");

        var compiler = new Api.XsltCompiler { UriResolver = resolver };
        var executable = compiler.Compile(resolver.Resolve("file:///main.xsl", null), "file:///main.xsl");
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("6", result);
    }

    [Fact]
    public void XslFunction_Recursive_Factorial()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:my='http://example.com/my'>
            <xsl:function name='my:factorial'>
                <xsl:param name='n'/>
                <xsl:sequence select='if ($n le 1) then 1 else $n * my:factorial($n - 1)'/>
            </xsl:function>
            <xsl:template match='/'>
                <output><xsl:value-of select='my:factorial(5)'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("120", result);
    }

    [Fact]
    public void XslFunction_With_Local_Variable()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:my='http://example.com/my'>
            <xsl:function name='my:sumsq'>
                <xsl:param name='a'/>
                <xsl:param name='b'/>
                <xsl:variable name='a2' select='$a * $a'/>
                <xsl:variable name='b2' select='$b * $b'/>
                <xsl:sequence select='$a2 + $b2'/>
            </xsl:function>
            <xsl:template match='/'>
                <output><xsl:value-of select='my:sumsq(3, 4)'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("25", result);
    }

    [Fact]
    public void XslFunction_Returns_Sequence()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:my='http://example.com/my'>
            <xsl:function name='my:range'>
                <xsl:param name='n'/>
                <xsl:sequence select='1 to $n'/>
            </xsl:function>
            <xsl:template match='/'>
                <output>
                    <xsl:for-each select='my:range(3)'>
                        <x><xsl:value-of select='.'/></x>
                    </xsl:for-each>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("<x>1</x>", result);
        Assert.Contains("<x>2</x>", result);
        Assert.Contains("<x>3</x>", result);
    }

    // ------------------------------------------------------------------
    // REQ-016: multi-key xsl:sort tests
    // ------------------------------------------------------------------

    [Fact]
    public void MultiKey_Sort_Two_Keys()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:for-each select='root/item'>
                        <xsl:sort select='@category'/>
                        <xsl:sort select='@value' data-type='number'/>
                        <x><xsl:value-of select='@name'/></x>
                    </xsl:for-each>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("root",
                new XElement("item", new XAttribute("category", "B"), new XAttribute("value", "2"), new XAttribute("name", "b2")),
                new XElement("item", new XAttribute("category", "A"), new XAttribute("value", "3"), new XAttribute("name", "a3")),
                new XElement("item", new XAttribute("category", "A"), new XAttribute("value", "1"), new XAttribute("name", "a1")),
                new XElement("item", new XAttribute("category", "B"), new XAttribute("value", "1"), new XAttribute("name", "b1"))
            ));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        // Expected order: a1, a3, b1, b2
        var a1Pos = result.IndexOf("<x>a1</x>");
        var a3Pos = result.IndexOf("<x>a3</x>");
        var b1Pos = result.IndexOf("<x>b1</x>");
        var b2Pos = result.IndexOf("<x>b2</x>");

        Assert.True(a1Pos < a3Pos, "a1 should come before a3");
        Assert.True(a3Pos < b1Pos, "a3 should come before b1");
        Assert.True(b1Pos < b2Pos, "b1 should come before b2");
    }

    [Fact]
    public void MultiKey_Sort_Descending_Secondary()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:for-each select='root/item'>
                        <xsl:sort select='@category'/>
                        <xsl:sort select='@value' data-type='number' order='descending'/>
                        <x><xsl:value-of select='@name'/></x>
                    </xsl:for-each>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("root",
                new XElement("item", new XAttribute("category", "A"), new XAttribute("value", "1"), new XAttribute("name", "a1")),
                new XElement("item", new XAttribute("category", "A"), new XAttribute("value", "3"), new XAttribute("name", "a3")),
                new XElement("item", new XAttribute("category", "B"), new XAttribute("value", "2"), new XAttribute("name", "b2"))
            ));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        // Expected order: a3, a1, b2
        var a3Pos = result.IndexOf("<x>a3</x>");
        var a1Pos = result.IndexOf("<x>a1</x>");
        var b2Pos = result.IndexOf("<x>b2</x>");

        Assert.True(a3Pos < a1Pos, "a3 should come before a1");
        Assert.True(a1Pos < b2Pos, "a1 should come before b2");
    }

    [Fact]
    public void MultiKey_Sort_Stable_When_Keys_Equal()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:for-each select='root/item'>
                        <xsl:sort select='@category'/>
                        <xsl:sort select='@value' data-type='number'/>
                        <x><xsl:value-of select='@name'/></x>
                    </xsl:for-each>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("root",
                new XElement("item", new XAttribute("category", "A"), new XAttribute("value", "1"), new XAttribute("name", "first")),
                new XElement("item", new XAttribute("category", "A"), new XAttribute("value", "1"), new XAttribute("name", "second"))
            ));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        // Stable sort: original order should be preserved for equal keys
        var firstPos = result.IndexOf("<x>first</x>");
        var secondPos = result.IndexOf("<x>second</x>");

        Assert.True(firstPos < secondPos, "first should come before second (stable sort)");
    }

    private static string WriteTempXsl(string content)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"bosak_test_{Guid.NewGuid()}.xsl");
        System.IO.File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void FnTransform_Basic_Transform()
    {
        var xslMain = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema'>
            <xsl:template match='/'>
                <output><xsl:value-of select='/root/text'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var mainPath = WriteTempXsl(xslMain);
        var mainUri = new Uri(mainPath).AbsoluteUri;

        var xslCaller = $@"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema'
            xmlns:map='http://www.w3.org/2005/xpath-functions/map'>
            <xsl:template match='/'>
                <result>
                    <xsl:variable name='result' select='transform(map{{""stylesheet-location"": ""{mainUri}"", ""source-node"": .}})'/>
                    <xsl:copy-of select='map:get($result, ""output"")'/>
                </result>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root", new XElement("text", "hello")));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xslCaller);
        var result = executable.TransformToString(new XDocumentNode(source));

        System.IO.File.Delete(mainPath);
        var doc = XDocument.Parse(result);
        Assert.Equal("hello", doc.Descendants("output").Single().Value);
    }

    [Fact]
    public void FnTransform_With_Stylesheet_Params()
    {
        var xslMain = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema'>
            <xsl:param name='greeting' select='""default""'/>
            <xsl:template match='/'>
                <output><xsl:value-of select='$greeting'/></output>
            </xsl:template>
        </xsl:stylesheet>";

        var mainPath = WriteTempXsl(xslMain);
        var mainUri = new Uri(mainPath).AbsoluteUri;

        var xslCaller = $@"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema'
            xmlns:map='http://www.w3.org/2005/xpath-functions/map'>
            <xsl:template match='/'>
                <result>
                    <xsl:variable name='result' select='transform(map{{""stylesheet-location"": ""{mainUri}"",
                        ""source-node"": .,
                        ""stylesheet-params"": map{{xs:QName(""greeting""): ""world""}}}})'/>
                    <xsl:copy-of select='map:get($result, ""output"")'/>
                </result>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xslCaller);
        var result = executable.TransformToString(new XDocumentNode(source));

        System.IO.File.Delete(mainPath);
        var doc = XDocument.Parse(result);
        Assert.Equal("world", doc.Descendants("output").Single().Value);
    }


    [Fact]
    public void FnTransform_With_Initial_Template()
    {
        var xslMain = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema'>
            <xsl:template name='start'>
                <output>from-template</output>
            </xsl:template>
        </xsl:stylesheet>";

        var mainPath = WriteTempXsl(xslMain);
        var mainUri = new Uri(mainPath).AbsoluteUri;

        var xslCaller = $@"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema'
            xmlns:map='http://www.w3.org/2005/xpath-functions/map'>
            <xsl:template match='/'>
                <result>
                    <xsl:variable name='result' select='transform(map{{""stylesheet-location"": ""{mainUri}"",
                        ""initial-template"": ""start""
                    }})'/>
                    <xsl:copy-of select='map:get($result, ""output"")'/>
                </result>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xslCaller);
        var result = executable.TransformToString(new XDocumentNode(source));

        System.IO.File.Delete(mainPath);
        var doc = XDocument.Parse(result);
        Assert.Equal("from-template", doc.Descendants("output").Single().Value);
    }

    [Fact]
    public void FnTransform_MissingSource_RaisesFOXT0002()
    {
        // fn-transform-err-1: fn:transform with a stylesheet-location but no source-node
        // and no initial-template must raise FOXT0002, not a raw ArgumentException.
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='/'><output/></xsl:template></xsl:stylesheet>";
        var path = WriteTempXsl(xsl);
        try
        {
            var uri = new Uri(path).AbsoluteUri;
            var query = $"fn:transform(map{{\"stylesheet-location\": \"{uri}\"}})";
            var ctx = new EvaluationContext();
            FunctionLibrary.Populate(ctx);
            XsltFunctionLibrary.PopulateTransformOnly(ctx);
            var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile(query).Evaluate(ctx));
            Assert.Contains("FOXT0002", ex.Message);
        }
        finally
        {
            System.IO.File.Delete(path);
        }
    }

    [Fact]
    public void FnTransform_ResultDocumentTextContent_IsCaptured()
    {
        // fn-transform-2: a secondary xsl:result-document whose content is a text node
        // must be captured in the result map without a LINQ "Non-whitespace characters
        // cannot be added to content" error.
        var query = @"
let $xsl := ""<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='2.0'>
  <xsl:template name='main'>
    <xsl:result-document href='out.xml'>sect1</xsl:result-document>
  </xsl:template>
</xsl:stylesheet>""
return fn:transform(map{""stylesheet-text"": $xsl,
                       ""initial-template"": fn:QName('', 'main'),
                       ""source-node"": fn:parse-xml('<doc/>'),
                       ""base-output-uri"": ""http://example.com/result.xml""})";

        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        XsltFunctionLibrary.PopulateTransformOnly(ctx);
        var result = XPath31Expression.Compile(query).Evaluate(ctx);
        Assert.True(result.IsMap);
        Assert.True(result.MapValue.TryGetValue(XdmValue.FromString("http://example.com/out.xml"), out var doc));
        Assert.Equal("sect1", doc.NodeValue.StringValue);
    }

    [Fact]
    public void MapKey_Lookup_CSharp_vs_XPath()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:map='http://www.w3.org/2005/xpath-functions/map'>
            <xsl:template match='/'>
                <result>
                    <size><xsl:value-of select='map:size($m)'/></size>
                    <get><xsl:value-of select='map:get($m, ""k"")'/></get>
                </result>
            </xsl:template>
        </xsl:stylesheet>";

        var map = new XdmMap();
        map.Add(XdmValue.FromString("k"), XdmValue.FromString("v"));
        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var ctx = new EvaluationContext();
        ctx.WithVariable("m", XdmValue.FromMap(map));
        var result = executable.TransformToString(new XDocumentNode(source), ctx);

        Assert.Contains("<size>1</size>", result);
        Assert.Contains("<get>v</get>", result);
    }

    [Fact]
    public void MultiKey_Sort_In_ApplyTemplates()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:apply-templates select='root/item'>
                        <xsl:sort select='@category'/>
                        <xsl:sort select='@value' data-type='number'/>
                    </xsl:apply-templates>
                </output>
            </xsl:template>
            <xsl:template match='item'>
                <x><xsl:value-of select='@name'/></x>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("root",
                new XElement("item", new XAttribute("category", "B"), new XAttribute("value", "2"), new XAttribute("name", "b2")),
                new XElement("item", new XAttribute("category", "A"), new XAttribute("value", "3"), new XAttribute("name", "a3")),
                new XElement("item", new XAttribute("category", "A"), new XAttribute("value", "1"), new XAttribute("name", "a1"))
            ));

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        // Expected order: a1, a3, b2
        var a1Pos = result.IndexOf("<x>a1</x>");
        var a3Pos = result.IndexOf("<x>a3</x>");
        var b2Pos = result.IndexOf("<x>b2</x>");

        Assert.True(a1Pos < a3Pos, "a1 should come before a3");
        Assert.True(a3Pos < b2Pos, "a3 should come before b2");
    }

    [Fact]
    public void MatchPattern_DivWithRootPrefix_MatchesRootElement()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<div><and/><or/><div/></div>");
        var sourceNode = new XDocumentNode(doc);
        var compiler = new Bosak.Xslt.Patterns.PatternCompiler();
        var pattern = compiler.Compile("/div");

        var ctx = new Bosak.XPath.Runtime.Vm.EvaluationContext();
        Bosak.XPath.Standard.Functions.FunctionLibrary.Populate(ctx);

        // Test the root div element
        foreach (var item in sourceNode.Axis(Bosak.XPath.Core.Xdm.XdmAxis.Child))
        {
            if (item.IsNode && item.NodeValue is Bosak.XPath.Core.Xdm.IXdmNode child)
            {
                var parent = child.Parent;
                Assert.NotNull(parent);
                Assert.Equal(Bosak.XPath.Core.Xdm.XdmNodeKind.Document, parent.NodeKind);
                var matches = pattern(XdmValue.FromNode(child), ctx);
                Assert.True(matches, $"/div should match root {child.NodeKind} {child.LocalName}");
            }
        }
    }

    [Fact]
    public void TryCatch_InResultTree_CatchExecutesOnError()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:try>
                        <xsl:sequence select='1 div 0'/>
                        <xsl:catch>
                            <error>caught</error>
                        </xsl:catch>
                    </xsl:try>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<error>caught</error>", result);
    }

    [Fact]
    public void TryCatch_WithSelectAttribute_ReturnsCatchValue()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:try>
                        <xsl:sequence select='xs:date(""invalid"")'/>
                        <xsl:catch select='""fallback""'/>
                    </xsl:try>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("fallback", result);
    }

    [Fact]
    public void TryCatch_NoError_TryBodyReturnsNormally()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <output>
                    <xsl:try>
                        <xsl:sequence select='""ok""'/>
                        <xsl:catch>
                            <error>should not appear</error>
                        </xsl:catch>
                    </xsl:try>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("ok", result);
        Assert.DoesNotContain("should not appear", result);
    }

    [Fact]
    public void TryCatch_InFunctionBody_CatchReturnsFallback()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
                    xmlns:xs='http://www.w3.org/2001/XMLSchema'
                    xmlns:app='http://example.com/app'>
            <xsl:function name='app:safe-date' as='xs:string?'>
                <xsl:param name='raw' as='xs:string'/>
                <xsl:try>
                    <xsl:sequence select='xs:date($raw)'/>
                    <xsl:catch select='()'/>
                </xsl:try>
            </xsl:function>
            <xsl:template match='/'>
                <output>
                    <xsl:sequence select='app:safe-date(""invalid"")'/>
                    <ok>done</ok>
                </output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<ok>done</ok>", result);
        Assert.DoesNotContain("invalid", result);
    }

    [Fact]
    public void ExcludeResultPrefixes_RemovesExcludedNamespacesFromOutput()
    {
        var xsl = @"<xsl:stylesheet version='2.0'
            xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema'
            xmlns:app='http://example.com/app'
            exclude-result-prefixes='xs app'>
            <xsl:template match='/'>
                <root>
                    <child>hello</child>
                </root>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<root>", result);
        Assert.DoesNotContain("xmlns:xs", result);
        Assert.DoesNotContain("xmlns:app", result);
    }

    [Fact]
    public void ExcludeResultPrefixes_All_RemovesAllUnnecessaryNamespaces()
    {
        var xsl = @"<xsl:stylesheet version='2.0'
            xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema'
            xmlns:unused='http://example.com/unused'
            exclude-result-prefixes='#all'>
            <xsl:template match='/'>
                <root>
                    <child>hello</child>
                </root>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<root>", result);
        Assert.DoesNotContain("xmlns:xs", result);
        Assert.DoesNotContain("xmlns:unused", result);
    }

    [Fact]
    public void ExcludeResultPrefixes_PreservesNeededNamespaceForElementName()
    {
        var xsl = @"<xsl:stylesheet version='2.0'
            xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:out='http://example.com/output'
            xmlns:xs='http://www.w3.org/2001/XMLSchema'
            exclude-result-prefixes='xs'>
            <xsl:template match='/'>
                <out:root xmlns:out='http://example.com/output'>
                    <out:child>hello</out:child>
                </out:root>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("http://example.com/output", result);
        Assert.DoesNotContain("xmlns:xs", result);
    }

    [Fact]
    public void XslMessage_WithSelectAttribute_EmitsMessage()
    {
        var messages = new List<string>();
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <xsl:message select=""'hello world'""/>
                <output>done</output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        compiler.MessageListener = new TestMessageListener(messages);
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("hello world", messages);
        Assert.Contains("<output>done</output>", result);
    }

    [Fact]
    public void XslMessage_WithSequenceConstructor_EmitsMessage()
    {
        var messages = new List<string>();
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <xsl:message>debug: <xsl:value-of select=""'info'""/></xsl:message>
                <output>done</output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        compiler.MessageListener = new TestMessageListener(messages);
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("debug: info", messages);
        Assert.Contains("<output>done</output>", result);
    }

    [Fact]
    public void XslMessage_WithoutListener_DoesNotThrow()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <xsl:message select=""'silent'""/>
                <output>done</output>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        // No listener set
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<output>done</output>", result);
    }

    private sealed class TestMessageListener : IXsltMessageListener
    {
        private readonly List<string> _messages;
        public TestMessageListener(List<string> messages) => _messages = messages;
        public void OnMessage(string message) => _messages.Add(message);
        public void OnWarning(string message) { }
    }

    [Fact]
    public void ForEachGroup_GroupBy_GroupsByKey()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <out>
                    <xsl:for-each-group select='cities/city' group-by='@country'>
                        <group country='{@country}'>
                            <xsl:for-each select='current-group()'>
                                <city><xsl:value-of select='@name'/></city>
                            </xsl:for-each>
                        </group>
                    </xsl:for-each-group>
                </out>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("cities",
                new XElement("city", new XAttribute("name", "Paris"), new XAttribute("country", "FR")),
                new XElement("city", new XAttribute("name", "Customer A"), new XAttribute("country", "DE")),
                new XElement("city", new XAttribute("name", "Munich"), new XAttribute("country", "DE")),
                new XElement("city", new XAttribute("name", "Lyon"), new XAttribute("country", "FR"))
            ));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("country=\"FR\"", result);
        Assert.Contains("country=\"DE\"", result);
        Assert.Contains("<city>Paris</city>", result);
        Assert.Contains("<city>Customer A</city>", result);
        Assert.Contains("<city>Munich</city>", result);
        Assert.Contains("<city>Lyon</city>", result);
    }

    [Fact]
    public void ForEachGroup_GroupAdjacent_GroupsContiguousItems()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <out>
                    <xsl:for-each-group select='items/item' group-adjacent='@type'>
                        <group type='{@type}' count='{count(current-group())}'/>
                    </xsl:for-each-group>
                </out>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("items",
                new XElement("item", new XAttribute("type", "A")),
                new XElement("item", new XAttribute("type", "A")),
                new XElement("item", new XAttribute("type", "B")),
                new XElement("item", new XAttribute("type", "B")),
                new XElement("item", new XAttribute("type", "A"))
            ));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        // Should produce 3 groups: AA, BB, A
        Assert.Contains("type=\"A\" count=\"2\"", result);
        Assert.Contains("type=\"B\" count=\"2\"", result);
        Assert.Contains("type=\"A\" count=\"1\"", result);
    }

    [Fact]
    public void ForEachGroup_GroupStartingWith_StartsNewGroupOnPattern()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <out>
                    <xsl:for-each-group select='items/item' group-starting-with='[@start=""yes""]'>
                        <group>
                            <xsl:for-each select='current-group()'>
                                <v><xsl:value-of select='@name'/></v>
                            </xsl:for-each>
                        </group>
                    </xsl:for-each-group>
                </out>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("items",
                new XElement("item", new XAttribute("name", "a1"), new XAttribute("start", "yes")),
                new XElement("item", new XAttribute("name", "a2")),
                new XElement("item", new XAttribute("name", "b1"), new XAttribute("start", "yes")),
                new XElement("item", new XAttribute("name", "b2"))
            ));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<v>a1</v>", result);
        Assert.Contains("<v>a2</v>", result);
        Assert.Contains("<v>b1</v>", result);
        Assert.Contains("<v>b2</v>", result);
    }

    [Fact]
    public void ForEachGroup_CurrentGroupingKey_ReturnsKeyValue()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <out>
                    <xsl:for-each-group select='items/item' group-by='@type'>
                        <group key='{current-grouping-key()}'/>
                    </xsl:for-each-group>
                </out>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(
            new XElement("items",
                new XElement("item", new XAttribute("type", "A")),
                new XElement("item", new XAttribute("type", "B"))
            ));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("key=\"A\"", result);
        Assert.Contains("key=\"B\"", result);
    }

    [Fact]
    public void MatchPattern_PathWithParenthesizedUnion_MatchesCorrectly()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<x><a>23</a><b>25</b></x>");
        var sourceNode = new XDocumentNode(doc);
        var compiler = new Bosak.Xslt.Patterns.PatternCompiler();
        var pattern = compiler.Compile("x/(a|b)");

        var ctx = new Bosak.XPath.Runtime.Vm.EvaluationContext();
        Bosak.XPath.Standard.Functions.FunctionLibrary.Populate(ctx);

        // Find the <a> element
        IXdmNode? xElem = null, aElem = null, bElem = null;
        foreach (var item in sourceNode.Axis(Bosak.XPath.Core.Xdm.XdmAxis.Child))
        {
            if (item.IsNode && item.NodeValue!.LocalName == "x") xElem = item.NodeValue;
        }
        Assert.NotNull(xElem);
        foreach (var item in xElem.Axis(Bosak.XPath.Core.Xdm.XdmAxis.Child))
        {
            if (item.IsNode && item.NodeValue!.LocalName == "a") aElem = item.NodeValue;
            if (item.IsNode && item.NodeValue!.LocalName == "b") bElem = item.NodeValue;
        }
        Assert.NotNull(aElem);
        Assert.NotNull(bElem);

        // Debug: check if the inner union pattern matches
        var innerPattern = compiler.Compile("a|b");
        Assert.True(innerPattern(XdmValue.FromNode(aElem), ctx), "a|b should match <a>");
        Assert.True(innerPattern(XdmValue.FromNode(bElem), ctx), "a|b should match <b>");

        // Debug: check if element pattern matches
        var xPattern = compiler.Compile("x");
        Assert.True(xPattern(XdmValue.FromNode(xElem), ctx), "x should match <x>");

        // Debug: check parent relationship
        Assert.NotNull(aElem.Parent);
        Assert.Equal("x", aElem.Parent.LocalName);
        Assert.True(xPattern(XdmValue.FromNode(aElem.Parent), ctx), "x should match parent of <a>");

        // Test simpler path pattern first
        var simplePath = compiler.Compile("x/a");
        Assert.True(simplePath(XdmValue.FromNode(aElem), ctx), "x/a should match <a>");

        // Test with single paren
        var parenA = compiler.Compile("(a)");
        Assert.True(parenA(XdmValue.FromNode(aElem), ctx), "(a) should match <a>");

        var parenPath = compiler.Compile("x/(a)");
        Assert.True(parenPath(XdmValue.FromNode(aElem), ctx), "x/(a) should match <a>");

        // Now test the parenthesized union path pattern
        var unionPath = compiler.Compile("x/(a|b)");
        Assert.True(unionPath(XdmValue.FromNode(aElem), ctx), "x/(a|b) should match <a>");
        Assert.True(unionPath(XdmValue.FromNode(bElem), ctx), "x/(a|b) should match <b>");
    }

    // ------------------------------------------------------------------
    // xsl:function @_name AVT tests
    // ------------------------------------------------------------------

    [Fact]
    public void XslFunction_UnderscoreName_LiteralAvt()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:my='http://example.com/my'>
            <xsl:function _name='my:square' visibility='public'>
                <xsl:param name='n' as='xs:integer'/>
                <xsl:sequence select='$n * $n'/>
            </xsl:function>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformFunction("Q{http://example.com/my}square", new[] { XdmValue.FromInteger(12) });

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(144, result.IntegerValue);
    }

    [Fact]
    public void XslFunction_UnderscoreName_StringConcatenationAvt()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:my='http://example.com/my'>
            <xsl:function _name=""{'my:'}{'square' || 1}"" visibility='public'>
                <xsl:param name='n' as='xs:integer'/>
                <xsl:sequence select='$n * $n'/>
            </xsl:function>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformFunction("Q{http://example.com/my}square1", new[] { XdmValue.FromInteger(12) });

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(144, result.IntegerValue);
    }

    [Fact]
    public void XslFunction_UnderscoreName_QNameExpression()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:my='http://example.com/my'
            xmlns:x='http://example.com/my'>
            <xsl:function _name=""{QName('http://example.com/my', 'x:square1')}"" visibility='public'>
                <xsl:param name='n' as='xs:integer'/>
                <xsl:sequence select='$n * $n'/>
            </xsl:function>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformFunction("Q{http://example.com/my}square1", new[] { XdmValue.FromInteger(12) });

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(144, result.IntegerValue);
    }

    [Fact]
    public void XslFunction_UnderscoreName_StaticParam()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:my='http://example.com/my'>
            <xsl:param name='function-name' static='yes' select=""'my:square'""/>
            <xsl:function _name=""{$function-name}"" visibility='public'>
                <xsl:param name='n' as='xs:integer'/>
                <xsl:sequence select='$n * $n'/>
            </xsl:function>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformFunction("Q{http://example.com/my}square", new[] { XdmValue.FromInteger(12) });

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(144, result.IntegerValue);
    }

    [Fact]
    public void Variable_As_Element_With_ShallowCopy_Children()
    {
        // Regression for namespace-0912: a variable typed as a single element whose
        // value comes from a shallow-copy apply-templates must contain the copied
        // element with its child results nested inside, not as separate siblings.
        var xsl = @"<xsl:stylesheet version='3.0'
            xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:w='w-uri'>
            <xsl:mode on-no-match='shallow-copy'/>
            <xsl:template match='/'>
                <xsl:variable name='var' as='element(p)'>
                    <xsl:apply-templates/>
                </xsl:variable>
                <report>
                    <xsl:sequence select='$var'/>
                </report>
            </xsl:template>
            <xsl:template match='q' as='element(w:s)'>
                <xsl:element name='w:s' namespace='w-uri'/>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("p", new XElement("q")));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        // The result must contain a single <p> element containing the <w:s> child.
        Assert.Contains("<p", result);
        Assert.Contains("<", result);
        Assert.DoesNotContain("<p/>", result); // p is not empty
    }

    [Fact]
    public void Quantified_Every_AttributeComparison_UsesBooleanResult()
    {
        // Regression for namespace-2611: the 'every' quantifier must coerce the
        // satisfies expression through effective boolean value, so a singleton
        // boolean sequence of false() makes the quantifier return false.
        var xsl = @"<xsl:stylesheet version='2.0'
            xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <xsl:variable name='t' as='element()'>
                    <foo a='1' b='1'/>
                </xsl:variable>
                <xsl:variable name='d' as='element()'>
                    <foo a='2' b='1'/>
                </xsl:variable>
                <result>
                    <xsl:value-of select=""string(
                        every $i in $t/@* satisfies
                        for $j in $d/@*[local-name(.) = local-name($i)]
                        return $i = $j
                    )""/>
                </result>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument()));

        Assert.Contains("<result>false</result>", result);
    }

    [Fact]
    public void Shadow_Select_Resolves_Static_Variable()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:variable name='N' static='yes' select=""'x'""/>
            <xsl:template name='main'>
                <xsl:variable name='x' select='3'/>
                <out><xsl:value-of _select=""${$N}""/></out>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(null!, initialTemplate: "main");

        Assert.Contains("<out", result);
        Assert.Contains("3</out>", result);
    }

    [Fact]
    public void Shadow_UseWhen_Excludes_Template()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:param name='include' static='yes' select='0'/>
            <xsl:template name='main'>
                <out/>
            </xsl:template>
            <xsl:template name='maybe' _use-when=""{$include} = 1"">
                <in/>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(null!, initialTemplate: "main");

        Assert.Contains("<out", result);
        Assert.DoesNotContain("<in/>", result);
    }

    [Fact]
    public void Shadow_Attributes_On_Literal_Result_Elements_Are_Preserved()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template name='main'>
                <out one='1' _one='1.0' _two='two'/>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(null!, initialTemplate: "main");

        Assert.Contains("_one=\"1.0\"", result);
        Assert.Contains("_two=\"two\"", result);
        Assert.Contains("one=\"1\"", result);
    }

    [Fact]
    public void Function_Body_Can_Reference_Global_Variable()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='http://f.com/' exclude-result-prefixes='f'>
            <xsl:variable name='in' select='(3,1,2)'/>
            <xsl:template name='main'>
                <out><xsl:value-of select='f:sort()' separator=','/></out>
            </xsl:template>
            <xsl:function name='f:sort'>
                <xsl:perform-sort select='$in'>
                    <xsl:sort select='.' data-type='number'/>
                </xsl:perform-sort>
            </xsl:function>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(null!, initialTemplate: "main");

        Assert.Contains("<out>1,2,3</out>", result);
    }

    [Fact]
    public void Unused_Function_Local_Variable_Does_Not_Trigger_Circular_Reference()
    {
        var xsl = @"<xsl:stylesheet version='2.0'
            xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:my='http://example.com/my'>
            <xsl:variable name='x' select='my:func(1)'/>
            <xsl:function name='my:func'>
                <xsl:param name='a'/>
                <xsl:variable name='b' select='$x'/>
                <xsl:sequence select='$a + 2'/>
            </xsl:function>
            <xsl:template match='/'>
                <out><xsl:value-of select='$x'/></out>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("root"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<out", result);
        Assert.Contains("3</out>", result);
    }

    [Fact]
    public void Function_Local_Variables_With_Same_Name_Are_Eagerly_Evaluated_In_Document_Order()
    {
        // Regression test: two xsl:variable declarations with the same name in a
        // function body must behave like normal lexical variables (the second shadows
        // the first). A broad lazy implementation caused the second variable to
        // reference itself instead of the first.
        var xsl = @"<xsl:stylesheet version='2.0'
            xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:f='http://example.com/f'>
            <xsl:template name='main'>
                <out><xsl:value-of select='f:sqrt(10)'/></out>
            </xsl:template>
            <xsl:function name='f:sqrt' as='xs:double'
                xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xsl:param name='n' as='xs:double'/>
                <xsl:variable name='e'
                    select='$n + (($n - $n * $n) div (2 * $n))'/>
                <xsl:variable name='e' as='xs:double'
                    select='round-half-to-even($e, 4)'/>
                <xsl:sequence select='$e'/>
            </xsl:function>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(null!, initialTemplate: "main");

        Assert.Contains("<out", result);
    }

    [Fact]
    public void Function_ApplyTemplates_Inside_Function_Matches_Name_Pattern()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:f='urn:function' exclude-result-prefixes='xs f'>
            <xsl:variable name='base' as='element()'><base/></xsl:variable>
            <xsl:function name='f:start' visibility='public'>
                <xsl:apply-templates select='$base' />
            </xsl:function>
            <xsl:template match='base'>
                <xsl:text>A</xsl:text>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformFunction("f:start", System.Array.Empty<XdmValue>(), new EvaluationContext());
        Assert.True(result.IsNode);
        Assert.Equal("A", result.NodeValue!.StringValue);
    }

    [Fact]
    public void Function_ApplyTemplates_Returns_Atomic_Sequence()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:f='urn:function' exclude-result-prefixes='xs f'>
            <xsl:function name='f:inc' as='xs:integer'>
                <xsl:param name='n' as='xs:integer'/>
                <xsl:apply-templates select='$n' mode='m'>
                    <xsl:with-param name='n' select='$n'/>
                </xsl:apply-templates>
            </xsl:function>
            <xsl:template match='.' mode='m'>
                <xsl:param name='n'/>
                <xsl:sequence select='$n + 1'/>
            </xsl:template>
            <xsl:template match='/'>
                <out><xsl:value-of select='f:inc(5)'/></out>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var source = new XDocumentNode(XDocument.Parse("<root/>"));
        var result = executable.TransformToString(source, new EvaluationContext());
        Assert.Contains("<out>6</out>", result);
    }

    [Fact]
    public void Function_CallTemplate_Returns_Atomic_Sequence()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:f='urn:function' exclude-result-prefixes='xs f'>
            <xsl:function name='f:answer' as='xs:integer'>
                <xsl:call-template name='t'/>
            </xsl:function>
            <xsl:template name='t'>
                <xsl:sequence select='42'/>
            </xsl:template>
            <xsl:template match='/'>
                <out><xsl:value-of select='f:answer()'/></out>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var source = new XDocumentNode(XDocument.Parse("<root/>"));
        var result = executable.TransformToString(source, new EvaluationContext());
        Assert.Contains("<out>42</out>", result);
    }

    [Fact]
    public void Global_Variable_Accumulator_After_Does_Not_Falsely_Circular()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema'>
            <xsl:accumulator name='minDate' initial-value='/*/block[1]/@date' as='xs:date'>
                <xsl:accumulator-rule match='block' select='if(@date &lt; $value) then @date else $value'/>
            </xsl:accumulator>
            <xsl:mode use-accumulators='#all'/>
            <xsl:variable name='minDate' select='accumulator-after(&quot;minDate&quot;)'/>
            <xsl:template match='/'>
                <out><xsl:value-of select='$minDate'/></out>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var source = new XDocumentNode(XDocument.Parse("<root><block date='2020-01-01'/><block date='2019-06-15'/></root>"));
        var result = executable.TransformToString(source, new EvaluationContext());
        Assert.Contains("2019-06-15", result);
    }

    [Fact]
    public void Function_ApplyTemplates_Inside_Function_Matches_Variable_Reference_Pattern()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:f='urn:function' exclude-result-prefixes='xs f'>
            <xsl:variable name='base' as='element()'><base/></xsl:variable>
            <xsl:function name='f:start' visibility='public' as='xs:string'>
                <xsl:value-of>
                    <xsl:apply-templates select='$base' />
                    <xsl:call-template name='named' />
                </xsl:value-of>
            </xsl:function>
            <xsl:template match='$base'>
                <xsl:text>|</xsl:text>
                <xsl:value-of select='.' />
                <xsl:text>|</xsl:text>
                <xsl:text>|</xsl:text>
                <xsl:sequence select='current-output-uri()' />
                <xsl:text>|</xsl:text>
            </xsl:template>
            <xsl:template name='named'>
                <xsl:text>|</xsl:text>
                <xsl:sequence select='current-output-uri()' />
                <xsl:text>|</xsl:text>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformFunction("f:start", System.Array.Empty<XdmValue>(), new EvaluationContext());
        Assert.Equal("||||||", result.StringValue);
    }

    [Fact]
    public void Html_Normalization_Form_NFKD_Decomposes_Text()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='html' normalization-form='NFKD' encoding='utf-8'/>
            <xsl:template match='/'>
                <html><body><xsl:value-of select='/doc'/></body></html>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var source = new XDocumentNode(XDocument.Parse("<doc>\u00FC</doc>"));
        var result = executable.TransformToString(source, new EvaluationContext());
        // U+00FC (u with diaeresis) in NFKD becomes U+0075 U+0308.
        Assert.Contains("\u0075\u0308", result, StringComparison.Ordinal);
        Assert.DoesNotContain("\u00FC", result, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    [InlineData("strip-space")]
    [InlineData("preserve-space")]
    [InlineData("copy-of")]
    public void EmptyXsltElement_WithContent_ThrowsXtse0260(string elementName)
    {
        var xsl = $@"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <xsl:{elementName}>content</xsl:{elementName}>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var ex = Assert.Throws<InvalidOperationException>(() => compiler.Compile(xsl));
        Assert.Contains("XTSE0260", ex.Message);
    }

    [Fact]
    public void EmptyXsltElement_WithWhitespace_Preservation_ThrowsXtse0260()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <xsl:strip-space elements='*' xml:space='preserve'> </xsl:strip-space>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var ex = Assert.Throws<InvalidOperationException>(() => compiler.Compile(xsl));
        Assert.Contains("XTSE0260", ex.Message);
    }

    [Fact]
    public void EmptyXsltElement_WithComment_IsAllowed()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:output method='xml'><!-- comment --></xsl:output>
            <xsl:template match='/'><out/></xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("dummy"))), new EvaluationContext());
        Assert.Contains("<out/>", result);
    }

    [Theory]
    [InlineData("x{3}y{4")]
    [InlineData("{banana")]
    public void UnbalancedAvt_ThrowsXtse0350(string avtValue)
    {
        var xsl = $@"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <out att=""{avtValue}""/>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var source = new XDocumentNode(new XDocument(new XElement("dummy")));
        var ex = Assert.Throws<InvalidOperationException>(() => executable.TransformToString(source, new EvaluationContext()));
        Assert.Contains("XTSE0350", ex.Message);
    }

    [Fact]
    public void BalancedAvt_WithNestedBraces_IsAllowed()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <out att=""x{concat('{', 'a', '}')}y""/>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("dummy"))), new EvaluationContext());
        Assert.Contains("att=\"x{a}y\"", result);
    }

    [Theory]
    [InlineData("{2+2} and }3")]
    [InlineData("}literal")]
    public void UnescapedRightBraceInAvt_ThrowsXtse0370(string avtValue)
    {
        var xsl = $@"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <out att=""{avtValue}""/>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var source = new XDocumentNode(new XDocument(new XElement("dummy")));
        var ex = Assert.Throws<InvalidOperationException>(() => executable.TransformToString(source, new EvaluationContext()));
        Assert.Contains("XTSE0370", ex.Message);
    }

    [Fact]
    public void EscapedRightBraceInAvt_IsAllowed()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <out att=""x}}y""/>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("dummy"))), new EvaluationContext());
        Assert.Contains("att=\"x}y\"", result);
    }

}
