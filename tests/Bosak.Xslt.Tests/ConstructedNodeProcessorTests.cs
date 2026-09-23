// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 september 2026
// PURPOSE              : Unit tests for the REQ-098 seam H3 constructed-node processors on EvaluationContext
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for the REQ-098 seam H3 hooks: <see cref="EvaluationContext.ConstructedElementProcessor"/>
/// fires once per constructed element after content completion, and
/// <see cref="EvaluationContext.ConstructedDocumentProcessor"/> fires at the result-document
/// boundary. With the processors unset (or no-op) the serialized result is byte-identical.
/// </summary>
public class ConstructedNodeProcessorTests
{
    private static IXdmNode DummySource()
        => new XDocumentNode(new XDocument(new XElement("dummy")));

    private static List<IXdmNode> ElementChildren(IXdmNode node)
    {
        var children = new List<IXdmNode>();
        foreach (var item in node.Axis(XdmAxis.Child))
        {
            if (item.IsNode && item.NodeValue is { NodeKind: XdmNodeKind.Element })
                children.Add(item.NodeValue);
        }
        return children;
    }

    // ----- ConstructedElementProcessor: xsl:element -----

    [Fact]
    public void XslElement_ProcessorFiresOncePerElement_AfterContentCompletion()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template name='main'>
                <out><xsl:element name='inner'><xsl:element name='leaf'>text</xsl:element></xsl:element></out>
            </xsl:template>
        </xsl:stylesheet>";

        var fired = new List<IXdmNode>();
        var context = new EvaluationContext { ConstructedElementProcessor = fired.Add };
        var executable = new Xslt.Api.XsltCompiler().Compile(xsl, "file:///test.xsl");
        executable.Transform(DummySource(), context, initialTemplate: "main");

        Assert.Equal(3, fired.Count);
        // Bottom-up, innermost first: leaf, then inner, then the literal out.
        Assert.Equal(new[] { "leaf", "inner", "out" }, fired.Select(n => n.LocalName).ToArray());
        // Content was already complete when each processor ran.
        var leaf = fired[0];
        Assert.Equal("text", leaf.StringValue);
        var inner = fired[1];
        Assert.Equal(new[] { "leaf" }, ElementChildren(inner).Select(n => n.LocalName).ToArray());
        Assert.Equal("text", ElementChildren(inner)[0].StringValue);
        var out_ = fired[2];
        Assert.Equal(new[] { "inner" }, ElementChildren(out_).Select(n => n.LocalName).ToArray());
    }

    // ----- ConstructedElementProcessor: literal result elements -----

    [Fact]
    public void LiteralResultElement_ProcessorFiresOncePerElement_AfterContentCompletion()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template name='main'>
                <out a='1'><b>text</b></out>
            </xsl:template>
        </xsl:stylesheet>";

        var fired = new List<IXdmNode>();
        var context = new EvaluationContext { ConstructedElementProcessor = fired.Add };
        var executable = new Xslt.Api.XsltCompiler().Compile(xsl, "file:///test.xsl");
        executable.Transform(DummySource(), context, initialTemplate: "main");

        Assert.Equal(2, fired.Count);
        Assert.Equal(new[] { "b", "out" }, fired.Select(n => n.LocalName).ToArray());
        var b = fired[0];
        Assert.Equal("text", b.StringValue);
        var out_ = fired[1];
        Assert.Single(ElementChildren(out_));
        Assert.Equal("b", ElementChildren(out_)[0].LocalName);
    }

    // ----- ConstructedElementProcessor: xsl:copy (element branch) -----

    [Fact]
    public void XslCopyElement_ProcessorFiresAfterCopiedContentIsAttached()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='root'>
                <xsl:copy><child/></xsl:copy>
            </xsl:template>
        </xsl:stylesheet>";
        var source = new XDocumentNode(new XDocument(new XElement("root", new XAttribute("id", "7"))));

        var fired = new List<IXdmNode>();
        var context = new EvaluationContext { ConstructedElementProcessor = fired.Add };
        var executable = new Xslt.Api.XsltCompiler().Compile(xsl, "file:///test.xsl");
        executable.Transform(source, context);

        // The LRE child fires first; then the copied root with attribute and child attached.
        Assert.Equal(2, fired.Count);
        Assert.Equal(new[] { "child", "root" }, fired.Select(n => n.LocalName).ToArray());
        var copied = fired[1];
        Assert.Single(ElementChildren(copied));
        Assert.Equal("child", ElementChildren(copied)[0].LocalName);
    }

    // ----- ConstructedDocumentProcessor: result-document boundary -----

    [Fact]
    public void ConstructedDocumentProcessor_FiresAtResultDocumentBoundary()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template name='main'>
                <out>x</out>
            </xsl:template>
        </xsl:stylesheet>";

        var fired = new List<IXdmNode>();
        var context = new EvaluationContext { ConstructedDocumentProcessor = fired.Add };
        var executable = new Xslt.Api.XsltCompiler().Compile(xsl, "file:///test.xsl");
        executable.Transform(DummySource(), context, initialTemplate: "main");

        Assert.Single(fired);
        Assert.Equal(XdmNodeKind.Document, fired[0].NodeKind);
        var roots = ElementChildren(fired[0]);
        Assert.Single(roots);
        Assert.Equal("out", roots[0].LocalName);
        Assert.Equal("x", roots[0].StringValue);
    }

    // ----- default behavior is bit-identical -----

    [Fact]
    public void UnsetOrNoOpProcessors_ProduceByteIdenticalOutput()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template name='main'>
                <out a='1'><b>text</b><c><xsl:element name='d'>more</xsl:element></c></out>
            </xsl:template>
        </xsl:stylesheet>";
        var executable = new Xslt.Api.XsltCompiler().Compile(xsl, "file:///test.xsl");

        var baseline = executable.TransformToString(DummySource(), initialTemplate: "main");
        var unsetContext = new EvaluationContext();
        var withUnset = executable.TransformToString(DummySource(), unsetContext, initialTemplate: "main");
        var noOpContext = new EvaluationContext
        {
            ConstructedElementProcessor = _ => { },
            ConstructedDocumentProcessor = _ => { },
        };
        var withNoOp = executable.TransformToString(DummySource(), noOpContext, initialTemplate: "main");

        Assert.Equal(baseline, withUnset);
        Assert.Equal(baseline, withNoOp);
        Assert.Contains("<b>text</b>", baseline);
    }

    // ----- empty result produces no spurious calls -----

    [Fact]
    public void EmptyResult_ProducesNoProcessorCalls()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template name='main'>
                <xsl:sequence select='()'/>
            </xsl:template>
        </xsl:stylesheet>";

        var elementCalls = 0;
        var documentCalls = 0;
        var context = new EvaluationContext
        {
            ConstructedElementProcessor = _ => elementCalls++,
            ConstructedDocumentProcessor = _ => documentCalls++,
        };
        var executable = new Xslt.Api.XsltCompiler().Compile(xsl, "file:///test.xsl");
        var result = executable.Transform(DummySource(), context, initialTemplate: "main");

        Assert.Equal(0, elementCalls);
        Assert.Equal(0, documentCalls);
        Assert.True(result.IsUndefined);
    }
}
