// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Contract tests for byte fidelity (AC-01), retention (AC-02), vocabulary/context (AC-03), failures (AC-07).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

public class AuthoringInspectionTests
{
    private static readonly Uri BaseUri = new("file:///C:/project/main.xsl");

    private static AuthoringSnapshot Inspect(string text, out byte[] bytes, Encoding? encoding = null)
    {
        encoding ??= new UTF8Encoding(false, true);
        bytes = encoding.GetBytes(text);
        Assert.True(AuthoringSource.TryCreate(bytes, BaseUri, out var source, out var failure), failure?.ToString());
        var result = XsltAuthoring.Inspect(source!);
        Assert.True(result.IsSuccess, result.Failure?.ToString());
        return result.Snapshot!;
    }

    private static AuthoringSnapshot Inspect(byte[] bytes)
    {
        Assert.True(AuthoringSource.TryCreate(bytes, BaseUri, out var source, out var failure), failure?.ToString());
        var result = XsltAuthoring.Inspect(source!);
        Assert.True(result.IsSuccess, result.Failure?.ToString());
        return result.Snapshot!;
    }

    private static AuthoringNodeDescriptor FindElement(AuthoringNodeDescriptor root, string localName) =>
        root.FindDescendant(n => n.Kind == AuthoringNodeKind.Element && n.ElementName!.LocalName == localName)
        ?? throw new InvalidOperationException($"Element '{localName}' not found.");

    // ---------------------------------------------------------------------------------------------
    // AC-01: untouched module with BOM, CRLF, quote styles, comments/PIs/entities -> exact export,
    // hand-verified ranges (line/column AND byte offsets).
    // ---------------------------------------------------------------------------------------------

    private static readonly string[] Ac01Lines =
    {
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
        "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">",
        "  <!-- greeting -->",
        "  <?render mode=\"fast\"?>",
        "  <xsl:template match=\"/\">",
        "    <out title='Tom &amp; Jerry' arrow=\"&#x2192;\">Hi &#xE9;tude</out>",
        "  </xsl:template>",
        "</xsl:stylesheet>",
    };

    private static byte[] Ac01Bytes()
    {
        var utf8 = new UTF8Encoding(false, true);
        return new byte[] { 0xEF, 0xBB, 0xBF }.Concat(utf8.GetBytes(string.Join("\r\n", Ac01Lines))).ToArray();
    }

    [Fact]
    public void Ac01_ExportOriginal_ReturnsExactInputBytes()
    {
        var bytes = Ac01Bytes();
        var snapshot = Inspect(bytes);

        var exported = snapshot.ExportOriginal(BaseUri);

        Assert.True(bytes.SequenceEqual(exported), "ExportOriginal must be byte-for-byte identical to the input.");
    }

    [Fact]
    public void Ac01_Ranges_AreHandVerified_LineColumnAndBytes()
    {
        var snapshot = Inspect(Ac01Bytes());
        var root = snapshot.PrincipalModule.Root;

        // Module provenance.
        Assert.Contains("UTF-8", snapshot.PrincipalModule.EncodingName, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(Ac01Bytes().LongLength, snapshot.PrincipalModule.OriginalByteLength);
        Assert.Equal("3.0", snapshot.PrincipalModule.Version);
        Assert.True(snapshot.PrincipalModule.IsPrincipal);
        Assert.True(snapshot.IsCompilable);

        // Line/byte anchors: BOM is 3 bytes; line 2 starts at byte 43. The stylesheet element spans
        // from its start tag on line 2 through its close tag at the end of line 8.
        var stylesheet = FindElement(root, "stylesheet");
        Assert.Equal(new SourceRange(2, 1, 8, 18, 43, 263), stylesheet.Range);

        var comment = root.FindDescendant(n => n.Kind == AuthoringNodeKind.Comment)!;
        Assert.Equal(new SourceRange(3, 3, 3, 20, 126, 17), comment.Range);

        var pi = root.FindDescendant(n => n.Kind == AuthoringNodeKind.ProcessingInstruction)!;
        Assert.Equal(new SourceRange(4, 3, 4, 25, 147, 22), pi.Range);

        var template = FindElement(root, "template");
        Assert.Equal(new SourceRange(5, 3, 7, 18, 173, 114), template.Range);

        var outElement = FindElement(root, "out");
        Assert.Equal(new SourceRange(6, 5, 6, 70, 203, 65), outElement.Range);
    }

    [Fact]
    public void Ac01_AttributeRanges_AreHandVerified_EntitySpellingPreserved()
    {
        var snapshot = Inspect(Ac01Bytes());
        var utf8 = new UTF8Encoding(false, true);
        var exported = snapshot.ExportOriginal(BaseUri);
        string Slice(SourceRange r) => utf8.GetString(exported.Skip((int)r.StartByteOffset).Take((int)r.ByteLength).ToArray());

        var attrOut = FindElement(snapshot.PrincipalModule.Root, "out");
        var title = attrOut.Attributes.Single(a => a.Name == "title");
        Assert.Equal(AuthoringAttributeSlotKind.Plain, title.SlotKind);
        Assert.Equal("Tom &amp; Jerry", title.RawLiteral);
        Assert.Equal("Tom & Jerry", title.ExpandedValue);
        Assert.Equal(new SourceRange(6, 17, 6, 32, 215, 15), title.ValueRange);
        Assert.Equal(new SourceRange(6, 10, 6, 33, 208, 23), title.FullRange);
        Assert.Equal("'Tom &amp; Jerry'", Slice(title.FullRange)[("title=".Length)..]);

        var arrow = attrOut.Attributes.Single(a => a.Name == "arrow");
        Assert.Equal("&#x2192;", arrow.RawLiteral);
        Assert.Equal("→", arrow.ExpandedValue);
        Assert.Equal(new SourceRange(6, 41, 6, 49, 239, 8), arrow.ValueRange);
        Assert.Equal(new SourceRange(6, 34, 6, 50, 232, 16), arrow.FullRange);

        // Text node: raw extent covers the character reference; DOM value is expanded (lossy).
        var text = attrOut.Children.Single(c => c.Kind == AuthoringNodeKind.Text);
        Assert.Equal(new SourceRange(6, 51, 6, 64, 249, 13), text.Range);
        Assert.Equal("Hi étude", text.Text);
        Assert.Equal("Hi &#xE9;tude", Slice(text.Range));
    }

    // ---------------------------------------------------------------------------------------------
    // AC-02: use-when=false branch, shadow attributes, literal-result stylesheet all survive, and the
    // derived document still compiles.
    // ---------------------------------------------------------------------------------------------

    private const string Ac02Stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <xsl:if test="true()">
      <branch use-when="false()"><junk:kept junk:attr="ok" xmlns:junk="urn:junk"/></branch>
      <xsl:element _name="{'item'}"><xsl:value-of _select="42"/></xsl:element>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
""";

    [Fact]
    public void Ac02_UseWhenFalseBranch_AndShadowAttributes_Retained_AndCompilable()
    {
        var snapshot = Inspect(Ac02Stylesheet, out var bytes);

        Assert.True(snapshot.IsCompilable, string.Join(" | ", snapshot.CompilationDiagnostics));
        Assert.True(bytes.SequenceEqual(snapshot.ExportOriginal(BaseUri)));

        var root = snapshot.PrincipalModule.Root;

        // use-when="false()" branch: present with its full extent, junk content and all.
        var branch = FindElement(root, "branch");
        Assert.Equal(new SourceRange(4, 7, 4, 92, 140, 85), branch.Range);
        var useWhen = branch.Attributes.Single(a => a.Name == "use-when");
        Assert.Equal("false()", useWhen.RawLiteral);
        Assert.Equal(AuthoringAttributeSlotKind.Plain, useWhen.SlotKind);
        var kept = FindElement(branch, "kept");
        Assert.Equal("urn:junk", kept.ElementName!.NamespaceUri);
        Assert.Equal("junk", kept.ElementName!.Prefix);

        // Underscore shadow attributes retained verbatim.
        var element = FindElement(root, "element");
        var shadowName = element.Attributes.Single(a => a.Name == "_name");
        Assert.Equal("{'item'}", shadowName.RawLiteral);
        var valueOf = FindElement(root, "value-of");
        var shadowSelect = valueOf.Attributes.Single(a => a.Name == "_select");
        Assert.Equal("42", shadowSelect.RawLiteral);
    }

    [Fact]
    public void Ac02_LiteralResultStylesheet_Retained_AndCompilable()
    {
        const string lre = """
<html xsl:version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <body><p>Hi</p></body>
</html>
""";
        var snapshot = Inspect(lre, out var bytes);

        Assert.True(snapshot.IsCompilable, string.Join(" | ", snapshot.CompilationDiagnostics));
        Assert.True(bytes.SequenceEqual(snapshot.ExportOriginal(BaseUri)));

        var module = snapshot.PrincipalModule;
        Assert.Equal("3.0", module.Version);
        var html = module.Root.Children.Single(c => c.Kind == AuthoringNodeKind.Element);
        Assert.Equal("html", html.ElementName!.LocalName);
        Assert.Equal(string.Empty, html.ElementName!.Prefix);
        Assert.Single(html.Attributes.Where(a => a.Name == "xsl:version"));
    }

    // ---------------------------------------------------------------------------------------------
    // AC-03: template + literal-result constructor + expression slot with real context; opaque
    // xsl:iterate keeps its exact full source extent.
    // ---------------------------------------------------------------------------------------------

    private const string Ac03Stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="urn:default" xpath-default-namespace="urn:xpd" xml:base="sub/">
  <xsl:template match="/input">
    <xsl:result-document href="out.xml" format='f&quot;1'>
      <xsl:element name="wrap"><xsl:value-of select="/input/name"/></xsl:element>
    </xsl:result-document>
    <xsl:iterate select="item">
      <xsl:value-of select="."/>
    </xsl:iterate>
  </xsl:template>
</xsl:stylesheet>
""";

    [Fact]
    public void Ac03_IterateRange_CoversStartTagThroughEndTagExactly()
    {
        var snapshot = Inspect(Ac03Stylesheet, out _);
        var utf8 = new UTF8Encoding(false, true);
        var exported = snapshot.ExportOriginal(BaseUri);

        var iterate = FindElement(snapshot.PrincipalModule.Root, "iterate");
        Assert.Equal(new SourceRange(6, 5, 8, 19, 354, 79), iterate.Range);
        Assert.Equal("<xsl:iterate", utf8.GetString(exported.Skip(354).Take(12).ToArray()));
        Assert.Equal("</xsl:iterate>", utf8.GetString(exported.Skip(354 + 79 - 14).Take(14).ToArray()));
    }

    [Fact]
    public void Ac03_ExpressionSlot_CarriesRealStaticContext()
    {
        var snapshot = Inspect(Ac03Stylesheet, out _);
        var valueOf = FindElement(snapshot.PrincipalModule.Root, "value-of");
        var select = valueOf.Attributes.Single(a => a.Name == "select");

        Assert.Equal(AuthoringAttributeSlotKind.Expression, select.SlotKind);
        var context = select.SlotContext!;
        Assert.Equal("xsl:value-of", context.OwningElement.LexicalForm);
        Assert.Equal("select", context.AttributeName);
        Assert.Equal("/input/name", select.RawLiteral);

        // In-scope namespaces: xsl prefix, default xmlns and the predefined xml binding.
        Assert.Contains(context.InScopeNamespaces, b => b.Prefix == "xsl" && b.Uri == "http://www.w3.org/1999/XSL/Transform");
        Assert.Contains(context.InScopeNamespaces, b => b.Prefix == string.Empty && b.Uri == "urn:default");
        Assert.Contains(context.InScopeNamespaces, b => b.Prefix == "xml" && b.Uri == "http://www.w3.org/XML/1998/namespace");

        Assert.Equal("urn:xpd", context.XpathDefaultNamespace);
        Assert.Equal(new Uri("file:///C:/project/sub/"), context.BaseUri);
        Assert.Equal("3.0", context.EffectiveVersion);
    }

    [Fact]
    public void Ac03_AvtSlots_Classified()
    {
        var snapshot = Inspect(Ac03Stylesheet, out _);
        var resultDocument = FindElement(snapshot.PrincipalModule.Root, "result-document");
        var href = resultDocument.Attributes.Single(a => a.Name == "href");
        Assert.Equal(AuthoringAttributeSlotKind.Avt, href.SlotKind);
        Assert.NotNull(href.SlotContext);

        var format = resultDocument.Attributes.Single(a => a.Name == "format");
        Assert.Equal(AuthoringAttributeSlotKind.Avt, format.SlotKind);
        Assert.Equal("f&quot;1", format.RawLiteral);
        Assert.Equal("f\"1", format.ExpandedValue);

        var element = FindElement(snapshot.PrincipalModule.Root, "element");
        Assert.Equal(AuthoringAttributeSlotKind.Avt, element.Attributes.Single(a => a.Name == "name").SlotKind);

        var template = FindElement(snapshot.PrincipalModule.Root, "template");
        Assert.Equal(AuthoringAttributeSlotKind.Pattern, template.Attributes.Single(a => a.Name == "match").SlotKind);
    }

    // ---------------------------------------------------------------------------------------------
    // AC-07: failure taxonomy — structure / unsupported encoding / invalid bytes / semantic-only.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Ac07_MalformedXml_ReturnsStructureFailureWithPlausibleRange()
    {
        var text = "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\r\n  <xsl:template match=\"/\">\r\n</xsl:stylesheet>";
        var bytes = new UTF8Encoding(false, true).GetBytes(text);
        Assert.True(AuthoringSource.TryCreate(bytes, BaseUri, out var source, out _));

        var result = XsltAuthoring.Inspect(source!);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Failure);
        Assert.Equal(AuthoringFailureKind.Structure, result.Failure!.Kind);
        Assert.Equal(BaseUri, result.Failure.ModuleUri);
        Assert.NotNull(result.Failure.Range);
        Assert.Equal(3, result.Failure.Range!.StartLine);
    }

    [Fact]
    public void Ac07_UnsupportedEncoding_IsClassified()
    {
        var bytes = Encoding.ASCII.GetBytes("<?xml version=\"1.0\" encoding=\"x-bogus-1\"?><r/>");

        var ok = AuthoringSource.TryCreate(bytes, BaseUri, out _, out var failure);

        Assert.False(ok);
        Assert.Equal(AuthoringFailureKind.UnsupportedEncoding, failure!.Kind);
    }

    [Fact]
    public void Ac07_InvalidSourceBytes_AreClassified()
    {
        var bytes = new byte[] { 0x3C, 0x72, 0x3E, 0xC3, 0x28, 0x3C, 0x2F, 0x72, 0x3E };

        var ok = AuthoringSource.TryCreate(bytes, BaseUri, out _, out var failure);

        Assert.False(ok);
        Assert.Equal(AuthoringFailureKind.InvalidSourceBytes, failure!.Kind);
    }

    [Fact]
    public void Ac07_SemanticError_InspectionSucceeds_WithCompilationDiagnostics()
    {
        const string broken = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/"><xsl:frobnicate/></xsl:template>
</xsl:stylesheet>
""";
        var snapshot = Inspect(broken, out _);

        Assert.False(snapshot.IsCompilable);
        Assert.NotEmpty(snapshot.CompilationDiagnostics);
    }

    // ---------------------------------------------------------------------------------------------
    // Depth counting: comments, PIs and CDATA that look like close tags must not confuse the scanner.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Scanner_SkipsCommentsPiAndCdata_WhenMatchingCloseTags()
    {
        const string text = "<r><a>x<!-- </a> --><![CDATA[</a>]]></a><a/></r>";
        var snapshot = Inspect(text, out _);

        var root = snapshot.PrincipalModule.Root;
        var elements = new List<AuthoringNodeDescriptor>();
        Collect(root, elements);

        var firstA = elements.First(e => e.ElementName!.LocalName == "a");
        Assert.Equal(new SourceRange(1, 4, 1, 41, 3, 37), firstA.Range);
        Assert.Equal("<a>x<!-- </a> --><![CDATA[</a>]]></a>", Slice(snapshot, firstA.Range));

        var secondA = elements.Where(e => e.ElementName!.LocalName == "a").Skip(1).First();
        Assert.Equal(new SourceRange(1, 41, 1, 45, 40, 4), secondA.Range);

        static void Collect(AuthoringNodeDescriptor node, List<AuthoringNodeDescriptor> elements)
        {
            if (node.Kind == AuthoringNodeKind.Element)
            {
                elements.Add(node);
            }

            foreach (var child in node.Children)
            {
                Collect(child, elements);
            }
        }

        static string Slice(AuthoringSnapshot snapshot, SourceRange range)
        {
            var all = snapshot.ExportOriginal(BaseUri);
            return new UTF8Encoding(false, true).GetString(all.Skip((int)range.StartByteOffset).Take((int)range.ByteLength).ToArray());
        }
    }

    [Fact]
    public void Snapshot_NodeIds_AreStableWithinSnapshot()
    {
        var snapshot = Inspect(Ac03Stylesheet, out _);
        var valueOf = FindElement(snapshot.PrincipalModule.Root, "value-of");

        Assert.True(snapshot.TryGetNodeId(valueOf.BackingObject!, out var id));
        Assert.Equal(valueOf.Id, id);
        Assert.False(snapshot.TryGetNodeId(new System.Xml.Linq.XElement("foreign"), out _));
    }
}
