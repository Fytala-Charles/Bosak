// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 November 2026
// PURPOSE              : Verifies in-record wrapper identity, mutation, typed values and lifetime
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-11-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 16-11-2026     | Preserve method snapshot versus lazy axis mutation semantics.                           |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;
using Bosak.XPath.Providers.Xml;
using Xunit;
namespace Bosak.XPath.Providers.Tests.Streaming;
/// <summary>Verifies singleton atomization with real streaming and typed nodes.</summary>
public sealed class SingletonAtomizationProviderTests
{
    private static readonly Func<XdmValue,XdmValue> Atomize=Assembly.Load("Bosak.XPath.Runtime").GetType("Bosak.XPath.Runtime.Vm.VmEngine",true)!.GetMethod("Atomize",BindingFlags.NonPublic|BindingFlags.Static)!.CreateDelegate<Func<XdmValue,XdmValue>>();
    private static List<XdmValue> Pull(XdmSequence sequence){var list=new List<XdmValue>();foreach(var v in sequence)list.Add(v);return list;}
    /// <summary>Checks schema-typed record child and attribute atomization.</summary>
    [Fact]public void TypedRecord_PreservesPsviValues()
    {
        var schemas = new XmlSchemaSet(); using (var text = XmlReader.Create(new StringReader("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:element name='record'><xs:complexType><xs:sequence><xs:element name='n' type='xs:integer'/></xs:sequence><xs:attribute name='id' type='xs:integer' use='required'/></xs:complexType></xs:element></xs:schema>"))) schemas.Add(null, text); schemas.Compile();
        using var reader = XmlReader.Create(new StringReader("<root><record id='007'><n>009</n></record></root>"));
        var document = XmlStreamingProvider.Load(reader, new StreamingLoadOptions { RecordPostProcessor = (obj, _) => {
            if (obj is XElement el) Assert.True(XdmSchemaAnnotator.ValidateSubtree(el, schemas).IsValid); return true; } });
        var root = Assert.Single(Pull(document.Children(XdmNodeKind.Element))).NodeValue;
        var record = Assert.Single(Pull(root.Children(XdmNodeKind.Element))).NodeValue;
        var child = Assert.Single(Pull(record.Children(XdmNodeKind.Element))).NodeValue;
        var attr = Assert.Single(Pull(record.Attributes())).NodeValue;
        Assert.Equal(XdmValueKind.Integer, child.TypedValue.Kind); Assert.Equal(9L, child.TypedValue.IntegerValue);
        Assert.Equal(XdmValueKind.Integer, attr.TypedValue.Kind); Assert.Equal(7L, attr.TypedValue.IntegerValue);
        Assert.Equal("integer", child.SchemaTypeAnnotation!.Value.LocalName); Assert.Equal("integer", attr.SchemaTypeAnnotation!.Value.LocalName);
        Assert.Same(child, Assert.Single(Pull(record.Axis(XdmAxis.Descendant)).Where(v => v.NodeValue.NodeKind == XdmNodeKind.Element)).NodeValue);
        Assert.Same(attr, Assert.Single(Pull(record.Axis(XdmAxis.Attribute))).NodeValue);
        Assert.Equal(9L,Atomize(XdmValue.FromSequence(record.Children(XdmNodeKind.Element))).IntegerValue);
        Assert.Equal(7L,Atomize(XdmValue.FromSequence(record.Attributes())).IntegerValue);
    }
    /// <summary>Checks held record sequences remain usable after pump advancement without closing the reader.</summary>
    [Fact]public void HeldRecord_PreservesValueAndReaderOwnership()
    {
        using var reader=XmlReader.Create(new StringReader("<root><record><n>9</n></record><record><n>8</n></record></root>"));
        var document=XmlStreamingProvider.Load(reader);var root=Assert.Single(Pull(document.Children(XdmNodeKind.Element))).NodeValue;
        var records=root.Children(XdmNodeKind.Element).GetEnumerator();Assert.True(records.MoveNext());var held=records.Current.NodeValue.Children(XdmNodeKind.Element);
        Assert.Equal("9",Atomize(XdmValue.FromSequence(held)).StringValue);Assert.True(records.MoveNext());
        Assert.Equal("9",Atomize(XdmValue.FromSequence(held)).StringValue);Assert.NotEqual(ReadState.Closed,reader.ReadState);
    }
}
