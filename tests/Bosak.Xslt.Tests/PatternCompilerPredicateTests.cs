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
//                      | Charles Korthout | 0.2   | 05-06-2026     | Added static validation tests for XTSE0340/XPST0017                                    |
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

namespace Bosak.Xslt.Tests;

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

    [Fact]
    public void DescendantAxisWithPositionPredicate_MatchesCorrectPosition()
    {
        // Pattern: doc/descendant::*[position() mod 2 = 0]
        // Should match even-positioned descendants in document order.
        var evenPattern = _compiler.Compile("doc/descendant::*[position() mod 2 = 0]");
        var oddPattern = _compiler.Compile("doc/descendant::*[position() mod 2 = 1]");

        var source = new XDocument(new XElement("doc",
            new XElement("a", new XAttribute("mark", "a1")),
            new XElement("b", new XElement("bb")),
            new XElement("c", new XAttribute("mark", "c1")),
            new XElement("c", new XAttribute("mark", "c2"))));

        var doc = new XDocumentNode(source);
        var docChildren = GetChildren(doc);
        Assert.Single(docChildren);

        // Collect all element descendants of doc in document order
        var descendants = new List<IXdmNode>();
        CollectElements(docChildren[0], descendants);

        // descendants: doc(root itself is excluded by doc/descendant::), a, b, bb, c, c
        // Wait, docChildren[0] IS the doc element. We need its descendants.
        var allDescendants = new List<IXdmNode>();
        foreach (var child in docChildren[0].Children())
        {
            if (child.IsNode && child.NodeValue is IXdmNode n && n.NodeKind == XdmNodeKind.Element)
            {
                allDescendants.Add(n);
                CollectElements(n, allDescendants);
            }
        }

        // a(1-odd), b(2-even), bb(3-odd), c1(4-even), c2(5-odd)
        Assert.Equal(5, allDescendants.Count);
        Assert.True(oddPattern(allDescendants[0], _ctx));   // a
        Assert.True(evenPattern(allDescendants[1], _ctx));  // b
        Assert.True(oddPattern(allDescendants[2], _ctx));   // bb
        Assert.True(evenPattern(allDescendants[3], _ctx));  // c1
        Assert.True(oddPattern(allDescendants[4], _ctx));   // c2
    }

    private static void CollectElements(IXdmNode node, List<IXdmNode> list)
    {
        foreach (var child in node.Children())
        {
            if (child.IsNode && child.NodeValue is IXdmNode n && n.NodeKind == XdmNodeKind.Element)
            {
                list.Add(n);
                CollectElements(n, list);
            }
        }
    }

    [Fact]
    public void UndeclaredFunctionInPredicate_ThrowsXpst0017()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => _compiler.Compile("*[Q{http://f.com/}special(.)]"));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void DisallowedFunctionAtPatternStart_ThrowsXtse0340()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => _compiler.Compile("copy-of($x)//a"));
        Assert.Contains("XTSE0340", ex.Message);
    }

    [Fact]
    public void KeyNonLiteralArgument_ThrowsXtse0340()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => _compiler.Compile("key('k', 40+2)//a"));
        Assert.Contains("XTSE0340", ex.Message);
    }
}
