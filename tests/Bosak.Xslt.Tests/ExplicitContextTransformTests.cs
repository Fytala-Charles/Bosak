// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 November 2026
// PURPOSE              : Verifies independent global context and legacy compatibility for internal transforms.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 01-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies the bounded internal context policy without changing legacy invocation.</summary>
public sealed class ExplicitContextTransformTests
{
    private const string Global = "<xsl:variable name='g' select='string(.)'/>";
    private const string SafeGlobal = "<xsl:variable name='g'><xsl:try><xsl:sequence select='string(.)'/><xsl:catch><xsl:sequence select=\"'absent'\"/></xsl:catch></xsl:try></xsl:variable>";

    /// <summary>Exercises distinct inputs, absence, declarations and unsupported match kinds.</summary>
    /// <param name="scenario">The independently specified regression scenario.</param>
    [Theory]
    [InlineData("distinct-documents")]
    [InlineData("element-match-refusal")]
    [InlineData("named-default-global")]
    [InlineData("element-global")]
    [InlineData("match-only")]
    [InlineData("missing-global-reference")]
    [InlineData("missing-named-focus")]
    [InlineData("source-free")]
    [InlineData("global-only")]
    [InlineData("required-global-only")]
    [InlineData("required-match-only")]
    [InlineData("declared-absent")]
    [InlineData("stale-dynamic-focus")]
    [InlineData("template-absent")]
    [InlineData("named-with-match")]
    public void ExplicitInvocationPreservesSelectedContextPolicy(string scenario)
    {
        IXdmNode? match = Document("A");
        IXdmNode? global = Document("B");
        string? initialTemplate = null;
        string declarations = Global;
        string body = Matched("concat(string(.),'|',$g)");
        string expected = "A|B";
        string? errorCode = null;
        bool unsupported = false;
        var context = new EvaluationContext();

        switch (scenario)
        {
            case "distinct-documents": break;
            case "element-match-refusal": match = Element("A"); unsupported = true; break;
            case "named-default-global": initialTemplate = "main"; body = Named("concat(string(.),'|',$g)"); expected = "B|B"; break;
            case "element-global": match = null; global = Element("B"); declarations = ""; initialTemplate = "main"; body = Named("string(.)"); expected = "B"; break;
            case "match-only": global = null; declarations = SafeGlobal; expected = "A|absent"; break;
            case "missing-global-reference": global = null; body = Matched("$g"); errorCode = "XPDY0002"; break;
            case "missing-named-focus": global = null; declarations = ""; initialTemplate = "main"; body = Named("string(.)"); errorCode = "XPDY0002"; break;
            case "source-free": match = null; global = null; declarations = ""; initialTemplate = "main"; body = Named("42"); expected = "42"; break;
            case "global-only": match = null; initialTemplate = "main"; body = Named("$g"); expected = "B"; break;
            case "required-global-only": match = null; initialTemplate = "main"; declarations = "<xsl:global-context-item use='required'/>" + Global; body = Named("$g"); expected = "B"; break;
            case "required-match-only": global = null; declarations = "<xsl:global-context-item use='required'/>"; body = Matched("string(.)"); errorCode = "XTDE3086"; break;
            case "declared-absent": declarations = "<xsl:global-context-item use='absent'/>" + SafeGlobal; expected = "A|absent"; break;
            case "stale-dynamic-focus": global = null; declarations = SafeGlobal; expected = "A|absent"; context.WithFocus(XdmValue.FromNode(Document("C")), 1, 1); break;
            case "template-absent": initialTemplate = "main"; declarations = ""; body = "<xsl:template name='main'><xsl:context-item use='absent'/><xsl:sequence select='string(.)'/></xsl:template>"; errorCode = "XPDY0002"; break;
            case "named-with-match": initialTemplate = "main"; declarations = ""; body = "<xsl:template name='main' match='/'><xsl:sequence select='string(.)'/></xsl:template>"; expected = "B"; break;
            default: throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        var executable = new XsltCompiler().Compile(Sheet(body, declarations));
        XdmValue Transform() => executable.TransformWithExplicitContext(match, global, initialTemplate, context);
        if (unsupported)
            Assert.Throws<NotSupportedException>(() => Transform());
        else if (errorCode is not null)
            Assert.Contains(errorCode, Assert.ThrowsAny<Exception>(() => Transform()).Message);
        else
            Assert.Equal(expected, Text(Transform()));
    }

    /// <summary>Retains the existing document/element distinction in public named-template calls.</summary>
    /// <param name="document">Whether the legacy source is a document rather than an element.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LegacyNamedTemplateRetainsSourceKindBehavior(bool document)
    {
        var executable = new XsltCompiler().Compile(Sheet(Named("string(.)"), ""));
        var source = document ? Document("A") : Element("A");
        if (document)
            Assert.Equal("A", Text(executable.Transform(source, initialTemplate: "main", rawResult: true)));
        else
            Assert.Contains("XPDY0002", Assert.ThrowsAny<Exception>(() => executable.Transform(source, initialTemplate: "main", rawResult: true)).Message);
    }

    private static IXdmNode Document(string value) => new XDocumentNode(XDocument.Parse("<n>" + value + "</n>"));
    private static IXdmNode Element(string value) => new XDocumentNode(XElement.Parse("<n>" + value + "</n>"));
    private static string Sheet(string body, string declarations) => "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" + declarations + body + "</xsl:stylesheet>";
    private static string Named(string expression) => "<xsl:template name='main'><xsl:sequence select=\"" + expression + "\"/></xsl:template>";
    private static string Matched(string expression) => "<xsl:template match='node()'><xsl:sequence select=\"" + expression + "\"/></xsl:template>";
    private static string Text(XdmValue value)
    {
        if (!value.IsSequence) return value.ToString();
        var items = new List<string>();
        foreach (var item in XdmSequence.FromSource(value.SequenceValue!)) items.Add(item.ToString());
        return string.Join("|", items);
    }
}
