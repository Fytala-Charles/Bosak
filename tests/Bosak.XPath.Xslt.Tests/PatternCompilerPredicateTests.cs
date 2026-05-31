// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 31 mei 2026
// PURPOSE              : Unit tests for PatternCompiler predicate support.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 31-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;

namespace Bosak.XPath.Xslt.Tests;

public class PatternCompilerPredicateTests
{
    private readonly Xslt.Patterns.PatternCompiler _compiler = new();
    private readonly EvaluationContext _ctx;

    public PatternCompilerPredicateTests()
    {
        _ctx = new EvaluationContext();
        FunctionLibrary.Populate(_ctx);
    }

    [Theory]
    [InlineData("[@start='yes']")]
    [InlineData("foo[@bar]")]
    [InlineData("*[@start='yes']")]
    [InlineData("foo[@bar='baz']")]
    [InlineData(".[@x]")]
    public void CanCompilePatternsWithPredicates(string pattern)
    {
        var compiled = _compiler.Compile(pattern);
        Assert.NotNull(compiled);
    }

    private static List<IXdmNode> GetChildren(IXdmNode node)
    {
        var list = new List<IXdmNode>();
        foreach (var child in node.Children())
        {
            if (child.IsNode && child.NodeValue is IXdmNode n)
                list.Add(n);
        }
        return list;
    }

    [Fact]
    public void BarePredicate_MatchesNodeWithAttribute()
    {
        var pattern = _compiler.Compile("[@start='yes']");

        var source = new XDocument(new XElement("items",
            new XElement("item", new XAttribute("start", "yes")),
            new XElement("item", new XAttribute("start", "no"))));

        var doc = new XDocumentNode(source);
        var items = GetChildren(GetChildren(doc)[0]);

        Assert.Equal(2, items.Count);
        Assert.True(pattern(items[0], _ctx));
        Assert.False(pattern(items[1], _ctx));
    }

    [Fact]
    public void ElementWithPredicate_MatchesOnlyWhenPredicateTrue()
    {
        var pattern = _compiler.Compile("item[@active='true']");

        var source = new XDocument(new XElement("root",
            new XElement("item", new XAttribute("active", "true")),
            new XElement("item", new XAttribute("active", "false")),
            new XElement("other", new XAttribute("active", "true"))));

        var doc = new XDocumentNode(source);
        var children = GetChildren(GetChildren(doc)[0]);

        Assert.Equal(3, children.Count);
        Assert.True(pattern(children[0], _ctx));
        Assert.False(pattern(children[1], _ctx));
        Assert.False(pattern(children[2], _ctx));
    }

    [Fact]
    public void BarePredicate_MatchesAnyNodeKindWithAttribute()
    {
        var pattern = _compiler.Compile("[@id]");

        var source = new XDocument(new XElement("root",
            new XElement("a", new XAttribute("id", "1")),
            new XElement("b")));

        var doc = new XDocumentNode(source);
        var children = GetChildren(GetChildren(doc)[0]);

        Assert.Equal(2, children.Count);
        Assert.True(pattern(children[0], _ctx));
        Assert.False(pattern(children[1], _ctx));
    }
}
