// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies raw results from untyped and nested initial templates.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies raw results from untyped and nested initial templates.</summary>
public class RawInitialTemplateTests
{
    private static string Wrap(string body, string declarations = "", string type = "") =>
        "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>" + declarations +
        "<xsl:template name='start' " + (type.Length == 0 ? "" : "as='"+type+"'") + ">" + body + "</xsl:template></xsl:stylesheet>";
    private static XdmValue Raw(string body, string declarations = "", string type = "") =>
        new Api.XsltCompiler().Compile(Wrap(body,declarations,type)).Transform(null,initialTemplate:"start",rawResult:true);
    private static List<XdmValue> Items(XdmValue value)
    {
        var result = new List<XdmValue>();
        if (value.IsUndefined) return result;
        if (!value.IsSequence) { result.Add(value); return result; }
        foreach (var item in XdmSequence.FromSource(value.SequenceValue!)) result.Add(item);
        return result;
    }

    /// <summary>Captures native-style grouped array members without serialization.</summary>
    [Fact]
    public void GroupedArray()
    {
        var value = Raw("""
            <xsl:array><xsl:for-each-group select='0 to 19' group-adjacent='. idiv 4'>
              <xsl:array-member select='current-group()'/>
            </xsl:for-each-group></xsl:array>
            """);
        Assert.True(value.IsArray);
        Assert.Equal(5,value.ArrayValue.Count);
        for (int i=1;i<=5;i++)
            Assert.Equal(Enumerable.Range((i-1)*4,4).Select(n=>(long)n), Items(value.ArrayValue.Get(i)).Select(v=>v.IntegerValue));
    }

    /// <summary>Preserves array and map values from untyped entry templates.</summary>
    /// <param name="select">The selected value.</param>
    /// <param name="kind">The required XDM kind.</param>
    [Theory]
    [InlineData("[(),(1,2)]",XdmValueKind.Array)]
    [InlineData("map{'x':1}",XdmValueKind.Map)]
    [InlineData("function(){1}",XdmValueKind.Function)]
    public void FunctionItems(string select, XdmValueKind kind) => Assert.Equal(kind,Raw(new XElement(XName.Get("sequence","http://www.w3.org/1999/XSL/Transform"),new XAttribute("select",select)).ToString()).Kind);

    /// <summary>Preserves atomics and emptiness instead of producing a document node.</summary>
    /// <param name="select">The selected sequence.</param>
    /// <param name="expected">Its expected atomic strings.</param>
    [Theory]
    [InlineData("()", "")]
    [InlineData("1", "1")]
    [InlineData("1,2,3", "1|2|3")]
    public void AtomicSequences(string select, string expected)
    {
        var values=Items(Raw("<xsl:sequence select='"+select+"'/>"));
        Assert.Equal(expected.Length == 0 ? 0 : expected.Split('|').Length,values.Count);
        Assert.All(values,v=>Assert.Equal(XdmValueKind.Integer,v.Kind));
        Assert.Equal(expected,string.Join("|",values.Select(v=>v.ToString())));
    }

    /// <summary>Preserves interleaved node/atomic order and repeated node identity.</summary>
    [Fact]
    public void MixedOrderAndIdentity()
    {
        var values=Items(Raw("<xsl:variable name='n' as='element()'><n/></xsl:variable><xsl:sequence select='1,$n,2,$n,3'/>"));
        Assert.Equal(5,values.Count);
        Assert.Equal(new long[]{1,2,3},new[]{values[0].IntegerValue,values[2].IntegerValue,values[4].IntegerValue});
        Assert.True(values[1].NodeValue.IsSameNode(values[3].NodeValue));
    }

    /// <summary>Returns constructed element and text nodes without document wrapping.</summary>
    [Fact]
    public void ConstructedNodes()
    {
        Assert.Equal(XdmNodeKind.Element,Raw("<out/>").NodeValue.NodeKind);
        Assert.Equal(XdmNodeKind.Text,Raw("<xsl:text>text</xsl:text>").NodeValue.NodeKind);
    }

    /// <summary>Delivers nested typed template output into its outer raw entry result.</summary>
    /// <param name="type">An optional entry type.</param>
    [Theory]
    [InlineData("")]
    [InlineData("xs:integer+")]
    public void NestedTypedCall(string type) => Assert.Equal(new long[]{1,2,3,4},Items(Raw(
        "<xsl:sequence select='1'/><xsl:call-template name='helper'/><xsl:sequence select='4'/>",
        "<xsl:template name='helper' as='xs:integer+'><xsl:sequence select='2,3'/></xsl:template>",type)).Select(v=>v.IntegerValue));

    /// <summary>Captures only the outer invocation when the initial template recurses.</summary>
    [Fact]
    public void RecursiveEntry() => Assert.Equal(new long[]{2,1,0},Items(Raw("""
        <xsl:param name='n' select='2'/><xsl:sequence select='$n'/>
        <xsl:if test='$n gt 0'><xsl:call-template name='start'><xsl:with-param name='n' select='$n - 1'/></xsl:call-template></xsl:if>
        """,type:"xs:integer*")).Select(v=>v.IntegerValue));

    /// <summary>Retains explicit return-type conversion and body-error propagation.</summary>
    [Fact]
    public void Errors()
    {
        Assert.Contains("XTTE0505",Assert.ThrowsAny<Exception>(()=>Raw("<xsl:sequence select='map{}'/>",type:"xs:integer")).ToString());
        Assert.Contains("boom",Assert.ThrowsAny<Exception>(()=>Raw("<xsl:sequence select='map{}'/><xsl:sequence select=\"error(QName('urn:test','boom'))\"/>")).ToString());
    }

    /// <summary>Leaves normal XML delivery and its serialization restrictions intact.</summary>
    [Fact]
    public void OrdinaryDelivery()
    {
        var compiler=new Api.XsltCompiler();
        var xml=compiler.Compile(Wrap("<out/>"));
        Assert.Equal(XdmNodeKind.Document,xml.Transform(null,initialTemplate:"start").NodeValue.NodeKind);
        Assert.Contains("<out",xml.TransformToString(null,initialTemplate:"start"));
        Assert.Contains("SENR0001",Assert.ThrowsAny<Exception>(()=>compiler.Compile(Wrap("<xsl:array/>"))
            .TransformToString(null,initialTemplate:"start")).ToString());
    }

    /// <summary>Supports a named initial template with a supplied source and match pattern.</summary>
    [Fact]
    public void SuppliedSource()
    {
        var stylesheet=Wrap("<xsl:sequence select='[1,2]'/>").Replace("name='start' ","name='start' match='/' ");
        Assert.True(new Api.XsltCompiler().Compile(stylesheet).Transform(new XDocumentNode(XDocument.Parse("<root/>")),
            initialTemplate:"start",rawResult:true).IsArray);
    }

    /// <summary>Supports the implicit xsl:initial-template entry and repeated executable use.</summary>
    [Fact]
    public void ImplicitAndRepeated()
    {
        var executable=new Api.XsltCompiler().Compile(Wrap("<xsl:sequence select='[1,2]'/>").Replace("name='start'","name='xsl:initial-template'"));
        Assert.True(executable.Transform(null,rawResult:true).IsArray);
        Assert.True(executable.Transform(null,rawResult:true).IsArray);
    }
}
