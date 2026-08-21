// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 21 augustus 2026
// PURPOSE              : Verifies XML 1.1-only name characters survive element construction and serialization.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 21-08-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

public class Xml11NameTests
{
    [Fact]
    public void ConstructedElement_Xml11NameStartChar_Preserved()
    {
        // U+037F is allowed at the start of a Name in XML 1.0 5th edition / XML 1.1,
        // but .NET's XML 1.0 name checker rejects it. The provider encodes it so the
        // element can be stored in an XName, then decodes it on access/serialization.
        string localName = "\u037Fnode";
        var spec = new XdmElementSpec(
            localName,
            null,
            null,
            Array.Empty<XdmAttributeValue>(),
            Array.Empty<XdmContentItem>(),
            null,
            true);

        var node = XDocumentProvider.ConstructElement(spec);

        Assert.Equal(localName, node.LocalName);
        Assert.Equal("<\u037Fnode />", node.ToXmlString());
    }

    [Fact]
    public void ConstructedElement_Xml11NameChar_Preserved()
    {
        // U+017F (long s) is allowed in XML 1.1 names but rejected by .NET's XML 1.0 checker.
        string localName = "egg\u017F";
        var spec = new XdmElementSpec(
            localName,
            null,
            null,
            Array.Empty<XdmAttributeValue>(),
            Array.Empty<XdmContentItem>(),
            null,
            true);

        var node = XDocumentProvider.ConstructElement(spec);

        Assert.Equal(localName, node.LocalName);
        Assert.Equal("<egg\u017F />", node.ToXmlString());
    }

    [Fact]
    public void ConstructedElement_Xml10Name_Unchanged()
    {
        // A plain XML 1.0 name should be unaffected by the XML 1.1 encode/decode path.
        var spec = new XdmElementSpec(
            "normal",
            null,
            null,
            Array.Empty<XdmAttributeValue>(),
            Array.Empty<XdmContentItem>(),
            null,
            true);

        var node = XDocumentProvider.ConstructElement(spec);

        Assert.Equal("normal", node.LocalName);
        Assert.Equal("<normal />", node.ToXmlString());
    }
}
