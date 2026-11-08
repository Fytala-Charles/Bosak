// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 October 2026
// PURPOSE              : Verifies Unicode map element names and literal values survive conversion.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 27-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies Unicode map element names and literal values survive conversion.</summary>
public class ElementMapUnicodeRoundTripTests
{
    private const string Literal = "ǀ0041ǁ";
    /// <summary>Names across the provider boundary, including literal codec-like names.</summary>
    /// <returns>Valid XML local names.</returns>
    public static IEnumerable<object[]> Names()
    {
        foreach (int point in new[] { 65, 960, 0x200c, 0x3001, 0x10000, 119070, 0xeffff }) yield return [char.ConvertFromUtf32(point)];
        yield return [Literal];
    }
    /// <summary>Root name and value remain logical Unicode values.</summary>
    /// <param name="name">The root name.</param>
    [Theory, MemberData(nameof(Names))]
    public void Root_PreservesNameAndValue(string name)
    {
        Assert.Equal(name, Run("name(map-to-element(map{$n: $v}))", name));
        Assert.Equal(Literal, Run("string(map-to-element(map{$n: $v}))", name));
    }
    /// <summary>Child element names do not expose their encoded storage form.</summary>
    /// <param name="name">The child name.</param>
    [Theory, MemberData(nameof(Names))]
    public void Child_PreservesName(string name) => Assert.Equal(name, Run("name(map-to-element(map{'r': map{$n: $v}})/*)", name));
    /// <summary>Attribute names and values decode exactly once.</summary>
    /// <param name="name">The attribute name.</param>
    [Theory, MemberData(nameof(Names))]
    public void Attribute_PreservesNameAndValue(string name)
    {
        Assert.Equal(name, Run("name(map-to-element(map{'r': map{'@' || $n: $v}})/@*)", name));
        Assert.Equal(Literal, Run("string(map-to-element(map{'r': map{'@' || $n: $v}})/@*)", name));
    }
    /// <summary>Conversion back to a map preserves key and literal content.</summary>
    /// <param name="name">The original map key.</param>
    [Theory, MemberData(nameof(Names))]
    public void RoundTrip_PreservesKeyAndValue(string name)
    {
        Assert.Equal(name, Run("string-join(map:keys(element-to-map(map-to-element(map{$n: $v}))), '')", name));
        Assert.Equal(Literal, Run("string(element-to-map(map-to-element(map{$n: $v}))($n))", name));
    }
    /// <summary>Lexical Unicode prefixes and local names retain namespace identity.</summary>
    /// <param name="prefix">The prefix.</param>
    [Theory]
    [InlineData("p")]
    [InlineData("𝄞")]
    [InlineData("ǀ0041ǁ")]
    public void LexicalName_PreservesPrefixAndNamespace(string prefix)
    {
        string local = char.ConvertFromUtf32(119070), uri = "urn:" + Literal;
        var context = Context(prefix + ":" + local).WithNamespace(prefix, uri);
        Assert.Equal(prefix + ":" + local, Evaluate("name(map-to-element(map{$n: $v}, map{'name-format':'lexical'}))", context));
        Assert.Equal(uri, Evaluate("namespace-uri(map-to-element(map{$n: $v}, map{'name-format':'lexical'}))", context));
    }
    /// <summary>Parentless Unicode namespaces retain axis identity and serialization prefixes.</summary>
    /// <param name="prefix">A logical namespace prefix.</param>
    [Theory]
    [InlineData("p")]
    [InlineData("𝄞")]
    [InlineData("ǀ0041ǁ")]
    public void NamespaceAxis_PreservesPrefixAndSerialization(string prefix)
    {
        string local=char.ConvertFromUtf32(119070), uri="urn:"+Literal;
        var context=Context(prefix+":"+local).WithNamespace(prefix,uri).WithVariable("u",XdmValue.FromString(uri));
        string call="map-to-element(map{$n:$v}, map{'name-format':'lexical'})";
        Assert.Equal(prefix,Evaluate("name("+call+"/namespace::*[string(.)=$u])",context));
        string xml=Evaluate("serialize("+call+")",context);
        Assert.Contains(prefix+":"+local,xml);Assert.Contains("xmlns:"+prefix+"=",xml);Assert.Contains(uri,xml);
    }
    /// <summary>XML-layout literals remain distinct from encoded storage names.</summary>
    [Fact]
    public void XmlLayout_PreservesLiteralNamesAndValues()
        => Assert.Equal(Literal, Run("name(map-to-element(map{'r': '<" + Literal + ">' || $v || '</" + Literal + ">'}, map{'plan': map{'r': map{'layout':'xml'}}}))", "unused"));
    private static EvaluationContext Context(string name) => new EvaluationContext().WithVariable("n", XdmValue.FromString(name)).WithVariable("v", XdmValue.FromString(Literal));
    private static string Run(string query, string name) => Evaluate(query, Context(name));
    private static string Evaluate(string query, EvaluationContext context) => XPath31Expression.Compile(query,new CompileOptions{Compatibility=XPathCompatibility.XPath40}).Evaluate(context).ToString();
}
