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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Xunit;

namespace Bosak.XPath.Xslt.Tests;

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(new XDocument(new XElement("root"))));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("<main>override</main>", result);
        Assert.DoesNotContain("<base>original</base>", result);
    }

    [Fact]
    public void Imported_Template_With_Higher_Priority_Wins()
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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(new XDocument(new XElement("root"))));

        // Default priority of "root" (QName) is 0, which is higher than -1
        Assert.Contains("<base>high-priority-default</base>", result);
        Assert.DoesNotContain("<main>low-priority</main>", result);
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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(new XDocument(new XElement("root"))));

        Assert.Contains("formatted", result);
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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

        // Indented output should contain newlines between elements
        Assert.Contains("\n", result);
        Assert.Contains("<root>", result);
        Assert.Contains("<child>hello</child>", result);
    }

    [Fact]
    public void Output_Default_Has_No_Declaration_And_No_Indent()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <root><child>hello</child></root>
            </xsl:template>
        </xsl:stylesheet>";

        var source = new XDocument(new XElement("input"));
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

        Assert.DoesNotContain("<?xml", result);
        Assert.DoesNotContain("\n", result);
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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

        // Built-in rules shallow-copy the element
        Assert.Contains("<root id=\"x\"", result);
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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

        Assert.Contains("<found>b</found>", result);
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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

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
        var result = executable.TransformToString(new Providers.Xml.XDocumentNode(source));

        var idxA = result.IndexOf("<item>a</item>");
        var idxM = result.IndexOf("<item>m</item>");
        var idxZ = result.IndexOf("<item>z</item>");
        Assert.True(idxA < idxM && idxM < idxZ, $"Expected a < m < z. Got: {result}");
    }
}
