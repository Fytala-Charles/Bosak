// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Tests optional XML assessment content bindings independently of any Schema dependency.
// SPECIAL NOTES        : Bounded native XSD 1.1 assessment; decimal-sequence-assertions-v1 profile.
//
// COPYRIGHT            : Fytala
// LICENSE              : LICENSE.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-10-2026     | Creation (REQ-009 bounded document assessment, 0.5 development line)                      |
//                      | Charles Korthout | 0.2   | 29-10-2026     | Verify failed attachment leaves original binding and fresh-node state intact             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.XPath.Providers.Tests;

/// <summary>Checks optional provider ownership and mutation refusal without coupling core to Schema.</summary>
public sealed class XdmElementAssessmentTests
{
    /// <summary>A failed replacement preserves the original binding and its stale-content checks.</summary>
    [Fact]
    public void FailedReplacement_RetainsOriginalBinding()
    {
        var xml = XElement.Parse("<n>2</n>");
        var original = XdmElementAssessment.Decimal(new object(), new object(), 2m);
        original.AttachTo(xml);
        xml.Value = new string('9', 70000);

        var replacement = XdmElementAssessment.Decimal(new object(), new object(), 3m);
        Assert.Throws<NotSupportedException>(() => replacement.AttachTo(xml));
        Assert.Throws<NotSupportedException>(() => XdmElementAssessment.Get(xml));

        xml.Value = "2";
        Assert.Same(original, XdmElementAssessment.Get(xml));
        Assert.Equal(2m, new XDocumentNode(xml).TypedValue.DecimalValue);
    }

    /// <summary>A failed initial attachment never leaves a partially registered node.</summary>
    [Fact]
    public void FailedInitialAttachment_LeavesNodeUnregistered()
    {
        var xml = new XElement("n", new string('9', 70000));
        var assessment = XdmElementAssessment.Decimal(new object(), new object(), 2m);

        Assert.Throws<NotSupportedException>(() => assessment.AttachTo(xml));
        Assert.Null(XdmElementAssessment.Get(xml));
    }

    /// <summary>Absent annotations preserve the established untyped path.</summary>
    [Fact]
    public void OrdinaryNodes_RemainUnregistered()
    {
        var xml=XElement.Parse("<root><n>2</n></root>");
        Assert.Null(XdmElementAssessment.Get(xml));Assert.Null(new XDocumentNode(xml).ElementAssessment);
        Assert.False(new XDocumentNode(xml).HasNoTypedValue);
    }
    /// <summary>The carrier keeps opaque owner/state references rather than manufacturing component names.</summary>
    [Fact]
    public void AnonymousRoot_RetainsOpaqueOriginalObjects()
    {
        var owner=new object();var state=new object();var xml=XElement.Parse("<root><n>2</n></root>");
        XdmElementAssessment.AnonymousElementOnly(owner,state).AttachTo(xml);
        var node=new XDocumentNode(xml);
        Assert.Same(owner,node.ElementAssessment!.Owner);Assert.Same(state,node.ElementAssessment.State);
        Assert.True(node.HasNoTypedValue);Assert.Equal((string.Empty,string.Empty),node.SchemaTypeAnnotation);
        Assert.Throws<NotSupportedException>(()=>node.SchemaElementDeclaration);
    }
    /// <summary>Edits cannot silently serve the original cached decimal.</summary>
    [Fact]
    public void EditedDecimal_RequiresExplicitStripOrReassessment()
    {
        var xml=XElement.Parse("<n>2</n>");XdmElementAssessment.Decimal(new object(),new object(),2m).AttachTo(xml);
        Assert.Equal(2m,new XDocumentNode(xml).TypedValue.DecimalValue);xml.Value="3";
        Assert.Throws<NotSupportedException>(()=>new XDocumentNode(xml).TypedValue);
        XdmSchemaAnnotator.StripSchemaAnnotations(xml);Assert.Null(XdmElementAssessment.Get(xml));
    }
    /// <summary>Provider annotations do not fabricate BCL schema components.</summary>
    [Fact]
    public void RootContentModel_IsElementOnlyWithoutBclSchemaCompilation()
    {
        var xml=XElement.Parse("<root><n>2</n></root>");XdmElementAssessment.AnonymousElementOnly(new object(),new object()).AttachTo(xml);
        Assert.Equal(System.Xml.Schema.XmlSchemaContentType.ElementOnly,XdmSchemaAnnotator.GetSchemaContentModel(xml));
    }
}
