// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies implicit document suppression for XSLT 4.0 variable bindings.
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

using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies implicit document suppression for XSLT 4.0 variable bindings.</summary>
public class VariableImplicitDocumentTests
{
    private static XdmValue Run(string body, string declarations = "", string version = "4.0") =>
        new Api.XsltCompiler().Compile("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='" + version + "'>" + declarations +
            "<xsl:template name='start'>" + body + "</xsl:template></xsl:stylesheet>")
            .Transform(null, initialTemplate: "start", rawResult: true);

    /// <summary>Suppresses implicit documents for each directly disqualifying instruction.</summary>
    /// <param name="content">The binding content.</param>
    /// <param name="check">The required value check.</param>
    [Theory]
    [InlineData("<xsl:array select='1,2'/>", "$v instance of array(*) and $v?2 = 2")]
    [InlineData("<xsl:map><xsl:map-entry key='1' select='2'/></xsl:map>", "$v instance of map(*) and $v(1) = 2")]
    [InlineData("<xsl:array-member select='1,2'/>", "count($v) = 1 and not($v instance of document-node())")]
    [InlineData("<xsl:map-entry key='1' select='2'/>", "count($v) = 1 and not($v instance of document-node())")]
    [InlineData("<xsl:select>1,2</xsl:select>", "count($v) = 2 and $v[2] = 2")]
    public void DirectDisqualifiers(string content, string check) =>
        Assert.True(Run("<xsl:variable name='v'>" + content + "</xsl:variable><xsl:sequence select='" + check + "'/>").BooleanValue);

    /// <summary>Preserves multi-item array members from the native array-018 example.</summary>
    [Fact]
    public void NativeArray018()
    {
        var result = Run("<xsl:variable name='v'><xsl:array for-each='1 to 10'><xsl:array-member select='., -.'/></xsl:array></xsl:variable><xsl:sequence select='$v?3'/>");
        Assert.True(result.IsSequence);
        var values = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!)) values.Add(item.ToString());
        Assert.Equal("3|-3", string.Join("|", values));
    }

    /// <summary>Applies the same rule to global variables and global parameter defaults.</summary>
    /// <param name="binding">The declaration kind.</param>
    [Theory]
    [InlineData("variable")]
    [InlineData("param")]
    public void GlobalBindings(string binding) =>
        Assert.True(Run("<xsl:sequence select='$v instance of array(*) and $v?2 = 2'/>",
            "<xsl:" + binding + " name='v'><xsl:array select='1,2'/></xsl:" + binding + ">").BooleanValue);

    /// <summary>Applies the rule to local defaults and supplied parameters including tunnels.</summary>
    /// <param name="supplied">Whether a with-param value is supplied.</param>
    /// <param name="tunnel">Whether the parameter is tunneled.</param>
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void TemplateParameters(bool supplied, bool tunnel)
    {
        var attr = tunnel ? " tunnel='yes'" : "";
        var content = "<xsl:array select='1,2'/>";
        var declarations = "<xsl:template name='callee'><xsl:param name='v'" + attr + ">" + content +
            "</xsl:param><xsl:sequence select='$v instance of array(*) and $v?2 = 2'/></xsl:template>";
        var call = "<xsl:call-template name='callee'>" + (supplied ? "<xsl:with-param name='v'" + attr + ">" + content + "</xsl:with-param>" : "") + "</xsl:call-template>";
        Assert.True(Run(call, declarations).BooleanValue);
    }

    /// <summary>Preserves ordinary temporary documents and direct-child rather than descendant detection.</summary>
    /// <param name="content">The non-disqualifying content.</param>
    [Theory]
    [InlineData("<n/>")]
    [InlineData("<xsl:sequence select='1,2'/>")]
    [InlineData("<xsl:if test='true()'><xsl:select>1,2</xsl:select></xsl:if>")]
    public void OrdinaryDocuments(string content) =>
        Assert.True(Run("<xsl:variable name='v'>" + content + "</xsl:variable><xsl:sequence select='$v instance of document-node()'/>").BooleanValue);

    /// <summary>Does not apply the new rule to older effective versions.</summary>
    /// <param name="version">The stylesheet version.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void OlderVersions(string version) =>
        Assert.True(Run("<xsl:variable name='v'><xsl:select>1,2</xsl:select></xsl:variable><xsl:sequence select='$v instance of document-node()'/>", version: version).BooleanValue);

    /// <summary>Uses the binding's effective version, including local overrides.</summary>
    [Fact]
    public void LocalVersionOverride() =>
        Assert.True(Run("<xsl:variable name='v' version='3.0'><xsl:select>1,2</xsl:select></xsl:variable><xsl:sequence select='$v instance of document-node()'/>").BooleanValue);

    /// <summary>Retains explicit type checking and nested XML construction errors.</summary>
    /// <param name="body">The invalid construction.</param>
    /// <param name="code">The required diagnostic.</param>
    [Theory]
    [InlineData("<xsl:variable name='v' as='document-node()'><xsl:array/></xsl:variable><xsl:sequence select='$v'/>", "XTTE0570")]
    [InlineData("<xsl:variable name='v'><n><xsl:array/></n></xsl:variable><xsl:sequence select='$v'/>", "XTDE0450")]
    [InlineData("<xsl:variable name='v'><xsl:array for-each='1'><xsl:array-member select='1'/><xsl:array-member select='2'/></xsl:array></xsl:variable><xsl:sequence select='$v'/>", "XTTE4060")]
    public void Errors(string body, string code) => Assert.Contains(code, Assert.ThrowsAny<Exception>(() => Run(body)).ToString());

    /// <summary>Preserves JNode kinds instead of atomizing or unwrapping binding values.</summary>
    [Fact]
    public void JNodeKind() => Assert.True(Run("<xsl:variable name='v'><xsl:array-member select='1,2'/></xsl:variable><xsl:sequence select='$v'/>").IsJNode);

    /// <summary>Preserves mixed sequence order and node identity across a disqualified binding.</summary>
    [Fact]
    public void MixedSequence()
    {
        Assert.True(Run("<xsl:variable name='n' as='element()'><n/></xsl:variable><xsl:variable name='v'><xsl:sequence select='$n,1'/><xsl:array/><xsl:sequence select='$n,2'/></xsl:variable><xsl:sequence select='count($v) = 5 and $v[1] is $v[4] and $v[2] = 1 and $v[3] instance of array(*) and $v[5] = 2'/>").BooleanValue);
    }

    /// <summary>Allows a local version override to enable the new binding rule.</summary>
    [Fact]
    public void EnableLocalVersion() => Assert.True(Run("<xsl:variable name='v' version='4.0'><xsl:select>1,2</xsl:select></xsl:variable><xsl:sequence select='count($v) = 2 and $v[2] = 2'/>", version: "3.0").BooleanValue);

    /// <summary>Retains explicit document construction even when its direct child is disqualifying.</summary>
    [Fact]
    public void ExplicitDocument() => Assert.Contains("XTDE0450", Assert.ThrowsAny<Exception>(() => Run("<xsl:variable name='v' as='document-node()'><xsl:document><xsl:array/></xsl:document></xsl:variable><xsl:sequence select='$v'/>")).ToString());
}
