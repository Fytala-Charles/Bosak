// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 November 2026
// PURPOSE              : Checks direct XQuery and XSLT bridges with typed context isolation and owner refusal.
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
using Bosak.XQuery.Api;
using Bosak.Xslt.Api;
using Xunit;
namespace Bosak.Xslt.Tests;

/// <summary>Verifies direct upper-layer bridge binding without per-evaluation reflection or wrappers.</summary>
public sealed class SchemaHostExecutionTests
{
    private static IXdmNode Doc(string text) => new XDocumentNode(XDocument.Parse("<n>" + text + "</n>"));
    private static string Text(XdmValue value)
    {
        if (!value.IsSequence) return value.ToString();
        var values = new List<string>();
        foreach (var item in XdmSequence.FromSource(value.SequenceValue!)) values.Add(item.ToString());
        return string.Join("|", values);
    }
    private static string Sheet => "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:variable name='g' select='string(/*)'/><xsl:template name='main'><xsl:sequence select=\"concat(string(.),'|',$g)\"/></xsl:template></xsl:stylesheet>";
    /// <summary>Uses the direct XQuery core on a fresh child and preserves caller input.</summary>
    [Fact]
    public void DirectXQueryUsesTypedChildContext()
    {
        var query = new XQueryCompiler().Compile("declare variable $v external;$v");
        var caller = new EvaluationContext().WithVariable("v", XdmValue.FromInteger(42));
        var inputs = caller.CaptureSchemaInputs(_ => false, 16, 16); caller.WithVariable("v", XdmValue.FromInteger(99));
        Assert.Equal("42", Text(SchemaHostExecution.BindXQuery(query)(inputs.CreateContext())));
        Assert.True(caller.TryGetVariable("v", out var value)); Assert.Equal(99L, value.IntegerValue);
    }
    /// <summary>The public XQuery null-argument contract remains intact after factoring.</summary>
    [Fact]
    public void PublicXQueryRetainsNullArgumentName()
        => Assert.Equal("context", Assert.Throws<ArgumentNullException>(() => new XQueryCompiler().Compile("1").Evaluate(null!)).ParamName);
    /// <summary>Separates explicit match/global roots through a direct bound evaluator.</summary>
    [Fact]
    public void DirectTransformUsesExplicitGlobalFocus()
    {
        var a = Doc("A"); var b = Doc("B");
        var evaluate = SchemaHostExecution.BindXslt(new XsltCompiler().Compile(Sheet), a, b, "main", node => ReferenceEquals(node, a) || ReferenceEquals(node, b));
        Assert.Equal("B|B", Text(evaluate(new EvaluationContext())));
    }
    /// <summary>Membership is checked again at execution, before host context mutation.</summary>
    [Fact]
    public void RevokedRootMembershipRefusesExecution()
    {
        var a = Doc("A"); bool admitted = true;
        var evaluate = SchemaHostExecution.BindXslt(new XsltCompiler().Compile(Sheet), a, a, "main", _ => admitted);
        _ = evaluate(new EvaluationContext()); admitted = false;
        var caller = new EvaluationContext().WithFocus(XdmValue.FromInteger(99), 1, 1);
        Assert.Throws<ArgumentException>(() => evaluate(caller)); Assert.Equal(99L, caller.ContextItem.IntegerValue);
    }
    /// <summary>The bounded bridge still refuses a non-document match input.</summary>
    [Fact]
    public void DirectTransformRetainsMatchProfileRefusal()
    {
        var element = new XDocumentNode(XElement.Parse("<n/>"));
        var evaluate = SchemaHostExecution.BindXslt(new XsltCompiler().Compile(Sheet), element, Doc("B"), "main", _ => true);
        Assert.Throws<NotSupportedException>(() => evaluate(new EvaluationContext()));
    }
    /// <summary>Null executable and ownership contracts fail before delegate execution.</summary>
    [Fact]
    public void BindingRequiresExecutableAndOwnership()
    {
        Assert.Throws<ArgumentNullException>(() => SchemaHostExecution.BindXQuery(null!));
        Assert.Throws<ArgumentNullException>(() => SchemaHostExecution.BindXslt(new XsltCompiler().Compile(Sheet), null, null, "main", null!));
    }
}
