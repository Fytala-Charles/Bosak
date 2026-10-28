// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 28 October 2026
// PURPOSE              : Verify construction typing, namespace-sensitive copying, source isolation and module scope.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 28-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XQuery.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Xunit;

namespace Bosak.XQuery.Tests;

/// <summary>Verify construction typing, namespace-sensitive copying, source isolation and module scope.</summary>
public sealed class ConstructionModeTests
{
    private const string Xs="http://www.w3.org/2001/XMLSchema";
    private static string Mode(bool preserve)=>"declare construction "+(preserve?"preserve":"strip")+"; ";
    private static IXdmNode Input(string sensitive="")
    {
        string schema="<xs:schema xmlns:xs='"+Xs+"' targetNamespace='urn:c34' xmlns:t='urn:c34' elementFormDefault='qualified'>"+
            "<xs:notation name='graphic' public='urn:c34:notation'/><xs:simpleType name='N'><xs:restriction base='xs:NOTATION'><xs:enumeration value='t:graphic'/></xs:restriction></xs:simpleType>"+
            "<xs:simpleType name='Names'><xs:list itemType='xs:QName'/></xs:simpleType><xs:complexType name='Root'><xs:sequence>"+
            "<xs:element name='count' type='xs:int'/><xs:element name='refs' type='xs:IDREFS'/><xs:element name='nil' type='xs:string' nillable='true'/>"+
            "<xs:element name='name' type='xs:QName' minOccurs='0'/><xs:element name='names' type='t:Names' minOccurs='0'/><xs:element name='notation' type='t:N' minOccurs='0'/>"+
            "</xs:sequence><xs:attribute name='key' type='xs:ID'/><xs:attribute name='q' type='xs:QName'/></xs:complexType><xs:element name='root' type='t:Root'/></xs:schema>";
        string xml="<t:root xmlns:t='urn:c34' xmlns:p='urn:c34:values' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' key='A' "+(sensitive=="attribute"?"q='p:entry'":"")+">"+
            "<t:count>7</t:count><t:refs>A A</t:refs><t:nil xsi:nil='true'/>"+(sensitive switch {"element"=>"<t:name>p:entry</t:name>","list"=>"<t:names>p:one p:two</t:names>","notation"=>"<t:notation>t:graphic</t:notation>",_=>""})+"</t:root>";
        var schemas=new XmlSchemaSet();using(var reader=XmlReader.Create(new StringReader(schema)))schemas.Add("urn:c34",reader);schemas.Compile();
        var doc=XDocument.Parse(xml);doc.Validate(schemas,(_,e)=>throw new Exception(e.Message),true);return XDocumentNode.Wrap(doc.Root!);
    }
    private static IXdmNode One(XdmValue value)
    {if(value.IsNode)return value.NodeValue;foreach(var x in XdmSequence.FromSource(value.SequenceValue!))return x.NodeValue;throw new Exception("Expected node");}
    private static IXdmNode Child(IXdmNode parent,string name)
    {foreach(var x in parent.Axis(XdmAxis.Child))if(x.NodeValue.LocalName==name)return x.NodeValue;throw new Exception(name);}
    private static IXdmNode Attribute(IXdmNode parent,string name)
    {foreach(var x in parent.Axis(XdmAxis.Attribute))if(x.NodeValue.LocalName==name)return x.NodeValue;throw new Exception(name);}
    private static int Count(XdmValue value)
    {if(value.IsUndefined)return 0;if(!value.IsSequence)return 1;int n=0;foreach(var _ in XdmSequence.FromSource(value.SequenceValue!))n++;return n;}
    private static IXdmNode Run(string query,IXdmNode source,XQueryCompiler? compiler=null)
        =>One((compiler??new XQueryCompiler()).Compile(query).Evaluate(new XQueryContext().WithVariable("input",XdmValue.FromNode(source))));
    private static void Verify(IXdmNode result,bool preserve)
    {
        Assert.Equal((preserve?"urn:c34":Xs,preserve?"Root":"untyped"),result.SchemaTypeAnnotation);
        Assert.Equal(preserve?"int":"untyped",Child(result,"count").SchemaTypeAnnotation!.Value.LocalName);
        Assert.Equal(preserve?2:1,Count(Child(result,"refs").TypedValue));Assert.Equal(preserve,Child(result,"refs").IsIdref);
        Assert.Equal(preserve,Child(result,"nil").IsNilled);Assert.Equal(preserve?0:1,Count(Child(result,"nil").TypedValue));
        Assert.Equal(preserve,Attribute(result,"key").IsId);Assert.Equal(preserve?"ID":"untypedAtomic",Attribute(result,"key").SchemaTypeAnnotation!.Value.LocalName);
        Assert.Equal(preserve,result.SchemaElementDeclaration is not null);
    }

    /// <summary>Supplies both construction policies, all copy modes and three constructor paths.</summary>
    /// <returns>Preserve types, preserve namespaces, inherit namespaces and constructor selection.</returns>
    public static IEnumerable<object[]> CopyCases()
    {foreach(bool p in new[]{false,true})foreach(bool ns in new[]{false,true})foreach(bool i in new[]{false,true})foreach(int path in new[]{0,1,2})yield return new object[]{p,ns,i,path};}
    /// <summary>Consistently copies or strips complete metadata while preserving the input.</summary>
    /// <param name="preserve">Whether schema annotations are retained.</param>
    /// <param name="namespaces">Whether unused source bindings are retained.</param>
    /// <param name="inherit">Whether parent bindings are inherited.</param>
    /// <param name="path">The direct, computed or document constructor.</param>
    [Theory,MemberData(nameof(CopyCases))]
    public void TypedCopies(bool preserve,bool namespaces,bool inherit,int path)
    {
        var input=Input();string body=path switch{0=>"<copy>{$input}</copy>/*",1=>"(element copy {$input})/*",_=>"(document {$input})/*"};
        var result=Run(Mode(preserve)+$"declare copy-namespaces {(namespaces?"preserve":"no-preserve")}, {(inherit?"inherit":"no-inherit")}; declare variable $input external; "+body,input);
        Assert.False(input.IsSameNode(result));Verify(result,preserve);Verify(input,true);
    }

    /// <summary>Supplies QName, QName-list and NOTATION values under both policies.</summary>
    /// <returns>Preserve types, preserve namespace bindings and sensitive value location.</returns>
    public static IEnumerable<object[]> SensitiveCases()
    {foreach(bool p in new[]{false,true})foreach(bool ns in new[]{false,true})foreach(string kind in new[]{"attribute","element","list","notation"})yield return new object[]{p,ns,kind};}
    /// <summary>Rejects namespace-sensitive values only where the construction contract requires it.</summary>
    /// <param name="preserve">Whether types are retained.</param>
    /// <param name="namespaces">Whether bindings are retained.</param>
    /// <param name="kind">The sensitive typed-value location.</param>
    [Theory,MemberData(nameof(SensitiveCases))]
    public void SensitiveValues(bool preserve,bool namespaces,string kind)
    {
        var input=Input(kind);string q=Mode(preserve)+$"declare copy-namespaces {(namespaces?"preserve":"no-preserve")}, no-inherit; declare variable $input external; <copy>{{$input}}</copy>/*";
        if(preserve&&!namespaces)Assert.Contains("XQTY0086",Assert.ThrowsAny<Exception>(()=>Run(q,input)).Message);else
        {
            var result=Run(q,input);Verify(result,preserve);
            if (preserve)
            {
                var sensitive=kind=="attribute"?Attribute(result,"q"):Child(result,kind=="element"?"name":kind=="list"?"names":"notation");
                var typed=sensitive.TypedValue;
                if (typed.IsSequence) foreach(var item in XdmSequence.FromSource(typed.SequenceValue!)) Assert.Equal("urn:c34:values",item.QNameValue.NamespaceUri);
                else Assert.Equal(kind=="notation"?"urn:c34":"urn:c34:values",typed.QNameValue.NamespaceUri);
            }
        }
        Verify(input,true);
    }

    /// <summary>Preserves safe free-attribute types and rejects namespace-sensitive ones.</summary>
    /// <param name="preserve">Whether types are retained.</param>
    /// <param name="sensitive">Whether the source attribute has a QName value.</param>
    [Theory][InlineData(false,false)][InlineData(false,true)][InlineData(true,false)][InlineData(true,true)]
    public void AttributeCopies(bool preserve,bool sensitive)
    {
        var input=Input(sensitive?"attribute":"");string name=sensitive?"q":"key";string q=Mode(preserve)+"declare variable $input external; <copy>{$input/@"+name+"}</copy>";
        if(preserve&&sensitive)Assert.Contains("XQTY0086",Assert.ThrowsAny<Exception>(()=>Run(q,input)).Message);
        else{var result=Attribute(Run(q,input),name);Assert.Equal(preserve?"ID":"untypedAtomic",result.SchemaTypeAnnotation!.Value.LocalName);Assert.Equal(preserve,result.IsId);}
    }

    /// <summary>Supplies module policies across functions, variables, closures and dynamic loading.</summary>
    /// <returns>The library construction mode and export access selection.</returns>
    public static IEnumerable<object[]> ModuleCases()
    {foreach(bool p in new[]{false,true})foreach(int path in new[]{0,1,2,3})yield return new object[]{p,path};}
    /// <summary>Uses a declaring module's policy instead of an inverse caller policy.</summary>
    /// <param name="preserve">The library policy.</param>
    /// <param name="access">The export access selection.</param>
    [Theory,MemberData(nameof(ModuleCases))]
    public void ModuleScope(bool preserve,int access)
    {
        var input=Input();string lib="module namespace m='urn:c34:m'; "+Mode(preserve)+"declare variable $m:input external; declare variable $m:v := <copy>{$m:input}</copy>/*; declare function m:make($n){<copy>{$n}</copy>/*}; declare function m:closure(){function($n){<copy>{$n}</copy>/*}};";
        var compiler=new XQueryCompiler().WithModule("urn:c34:m",lib);
        string value=access switch{0=>"m:make($input)",1=>"$m:v",2=>"m:closure()($input)",_=>"load-xquery-module('urn:c34:m',map{'variables':map{QName('urn:c34:m','input'):$input}})('functions')(QName('urn:c34:m','make'))(1)($input)"};
        var ctx=new XQueryContext().WithVariable("input",XdmValue.FromNode(input));
        ctx.EvaluationContext.WithVariable("input",XdmValue.FromNode(input),"urn:c34:m");
        Verify(One(compiler.Compile("import module namespace m='urn:c34:m'; "+Mode(!preserve)+"declare variable $input external; "+value).Evaluate(ctx)),preserve);Verify(input,true);
    }

    /// <summary>Omission preserves deep metadata on private schema-info instances.</summary>
    [Fact]
    public void DefaultPreserveAndPrivateMetadata()
    {
        var input=Input();var copied=Run("declare variable $input external; <copy>{$input}</copy>/*",input);Verify(copied,true);
        var info=((XElement)((XDocumentNode)copied).UnderlyingObject).GetSchemaInfo();Assert.IsType<XmlSchemaInfo>(info).IsNil=true;Assert.False(input.IsNilled);
    }

    /// <summary>Rejects duplicate, malformed and misplaced construction declarations.</summary>
    /// <param name="query">The invalid query.</param>
    /// <param name="code">The required static error code.</param>
    [Theory]
    [InlineData("declare construction strip; declare construction preserve; 1","XQST0067")]
    [InlineData("declare construction wrong; 1","XPST0003")]
    [InlineData("declare construction strip 1","XPST0003")]
    [InlineData("declare variable $v := 1; declare construction strip; $v","XPST0003")]
    [InlineData("declare function local:f(){1}; declare construction preserve; local:f()","XPST0003")]
    [InlineData("declare context item := 1; declare construction strip; .","XPST0003")]
    public void StaticErrors(string query,string code)=>Assert.Contains(code,Assert.ThrowsAny<Exception>(()=>new XQueryCompiler().Compile(query)).Message);

    /// <summary>Fresh nodes expose the chosen type policy and qualified xml:id semantics.</summary>
    /// <param name="preserve">Whether fresh elements use xs:anyType.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void FreshNodes(bool preserve)
    {
        var node=One(new XQueryCompiler().Compile(Mode(preserve)+"<fresh key='A' xml:id='B'/>").Evaluate(new XQueryContext()));
        Assert.Equal(preserve,((XDocumentNode)node).IsConstructedElement);Assert.Equal((Xs,preserve?"anyType":"untyped"),node.SchemaTypeAnnotation);Assert.Equal("untypedAtomic",node.TypedValue.SchemaTypeName);Assert.False(Attribute(node,"key").IsId);Assert.True(Attribute(node,"id").IsId);Assert.Equal("untypedAtomic",Attribute(node,"key").TypedValue.SchemaTypeName);
    }

    /// <summary>Comments and subsequent static-context clones retain the declared strip policy.</summary>
    [Fact]
    public void CommentsAndContextClones()=>Verify(Run("declare(:a(:b:)a:)construction(:c:)strip; declare namespace p='urn:c34:p'; declare variable $input external; <copy>{$input}</copy>/*",Input()),false);
    /// <summary>Stripped elements remain instances of the universal schema element type.</summary>
    /// <param name="computed">Whether the element uses computed syntax.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void UntypedDerivesFromAnyType(bool computed)
    {string node=computed?"element a {'x'}":"<a>x</a>";Assert.True(new XQueryCompiler().Compile("declare construction strip; ("+node+") instance of element(*,xs:anyType)").Evaluate(new XQueryContext()).BooleanValue);}

    /// <summary>Bare validation cannot be a path/postfix operand, while parenthesized validation parses.</summary>
    /// <param name="tail">The illegal direct postfix or path suffix.</param>
    [Theory][InlineData("/*")][InlineData("[1]")][InlineData(" ! .")]
    public void ValidatePrecedence(string tail)
    {
        Assert.Contains("XPST0003",Assert.ThrowsAny<Exception>(()=>new XQueryCompiler().Compile("validate lax {<a/>}"+tail)).Message);
        var result=new XQueryCompiler().Compile("(validate lax {<a/>})"+tail).Evaluate(new XQueryContext());Assert.False(result.IsUndefined);
    }

    /// <summary>Invalid fresh xml:id values retain their required dynamic error rather than becoming IDs.</summary>
    /// <param name="value">An invalid ID lexical value.</param>
    [Theory][InlineData("789x")][InlineData("")][InlineData("   ")]
    public void InvalidXmlId(string value)=>Assert.Contains("XQDY0091",Assert.ThrowsAny<Exception>(()=>new XQueryCompiler().Compile("<a xml:id='"+value+"'/>").Evaluate(new XQueryContext())).Message);

    /// <summary>A named callback retains its main-module policy while invoked inside an inverse library.</summary>
    /// <param name="preserve">The caller callback's construction policy.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void CallbackScope(bool preserve)
    {
        var compiler=new XQueryCompiler().WithModule("urn:c34:m","module namespace m='urn:c34:m'; "+Mode(!preserve)+"declare function m:invoke($f,$n){$f($n)};");
        string query="import module namespace m='urn:c34:m'; "+Mode(preserve)+"declare variable $input external; declare function local:make($n){<copy>{$n}</copy>/*}; m:invoke(local:make#1,$input)";
        Verify(Run(query,Input(),compiler),preserve);
    }

    /// <summary>Direct provider-hook callers without a mode retain their existing annotation behavior.</summary>
    [Fact]
    public void LegacyProviderHook()
    {
        var node=XDocumentProvider.ConstructElement(new XdmElementSpec("legacy",null,null,Array.Empty<XdmAttributeValue>(),Array.Empty<XdmContentItem>()));
        Assert.True(((XDocumentNode)node).IsConstructedElement);Assert.Null(node.SchemaTypeAnnotation);
    }

}
