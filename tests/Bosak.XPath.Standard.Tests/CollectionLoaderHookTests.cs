// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 October 2026
// PURPOSE              : Unit tests for the EvaluationContext.CollectionLoader host hook (fn:collection / fn:uri-collection)
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-10-2026     | Creation (REQ-120 Slice 3)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>
/// Tests for the additive <see cref="EvaluationContext.CollectionLoader"/> hook (REQ-120
/// Slice 3): resolution order (registered collections first), URI presentation (default
/// collection as empty string, relative-URI absolutization, unstripped query components),
/// empty/declining fall-through to FODC0002, member load error mapping, creation-sequence
/// ordering, and the provider-agnostic collection-fragment path for foreign node providers.
/// </summary>
public sealed class CollectionLoaderHookTests
{
    private static XdmValue Evaluate(string xpath, EvaluationContext ctx)
    {
        FunctionLibrary.Populate(ctx);
        return XPath31Expression.Compile(xpath).Evaluate(ctx);
    }

    private static List<IXdmNode> NodeItems(XdmValue value)
    {
        var nodes = new List<IXdmNode>();
        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
            {
                Assert.True(item.IsNode, "expected a node item");
                nodes.Add(item.NodeValue!);
            }
        }
        else
        {
            Assert.True(value.IsNode, "expected a node item");
            nodes.Add(value.NodeValue!);
        }

        return nodes;
    }

    private static string RootLocalName(IXdmNode document)
    {
        foreach (var child in document.Children(XdmNodeKind.Element))
        {
            Assert.True(child.IsNode);
            return child.NodeValue!.LocalName;
        }

        throw new InvalidOperationException("document has no root element");
    }

    [Fact]
    public void CollectionLoader_MembersResolvedInHookOrder()
    {
        using var setup = new TempCollectionDir(out var tempDir);
        var ctx = new EvaluationContext();
        ctx.CollectionLoader = uri =>
        {
            Assert.Equal("mydb://coll", uri);
            return new[] { setup.FileB, setup.FileA };
        };

        var nodes = NodeItems(Evaluate("collection('mydb://coll')", ctx));

        Assert.Equal(2, nodes.Count);
        Assert.Equal("b", RootLocalName(nodes[0]));
        Assert.Equal("a", RootLocalName(nodes[1]));
    }

    [Fact]
    public void CollectionLoader_DefaultCollectionReceivesEmptyString()
    {
        using var setup = new TempCollectionDir(out _);
        var ctx = new EvaluationContext();
        string? seen = null;
        ctx.CollectionLoader = uri =>
        {
            seen = uri;
            return new[] { setup.FileA };
        };

        var nodes = NodeItems(Evaluate("collection()", ctx));

        Assert.Equal(string.Empty, seen);
        Assert.Single(nodes);
        Assert.Equal("a", RootLocalName(nodes[0]));
    }

    [Fact]
    public void UriCollectionLoader_ReturnsMemberUris()
    {
        using var setup = new TempCollectionDir(out _);
        var ctx = new EvaluationContext();
        ctx.CollectionLoader = _ => new[] { setup.FileB, setup.FileA };

        var value = Evaluate("uri-collection('mydb://coll')", ctx);
        Assert.True(value.IsSequence);
        var uris = new List<string>();
        foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
            uris.Add(item.ToString());

        Assert.Equal(new[]
        {
            new Uri(System.IO.Path.GetFullPath(setup.FileB)).AbsoluteUri,
            new Uri(System.IO.Path.GetFullPath(setup.FileA)).AbsoluteUri,
        }, uris);
    }

    [Fact]
    public void CollectionLoader_EmptyListIsAnEmptyCollection()
    {
        var ctx = new EvaluationContext { CollectionLoader = _ => Array.Empty<string>() };

        var value = Evaluate("collection('mydb://empty')", ctx);
        Assert.True(value.IsSequence);
        Assert.Empty(NodeItems(value));
    }

    [Fact]
    public void CollectionLoader_NullDeclineFallsThroughToFodc0002()
    {
        var ctx = new EvaluationContext { CollectionLoader = _ => null };

        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("collection('mydb://missing')", ctx));
        Assert.Contains("FODC0002", ex.Message);
    }

    [Fact]
    public void CollectionLoader_UnsetHookKeepsExistingErrorContract()
    {
        var ctx = new EvaluationContext();

        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("collection('mydb://missing')", ctx));
        Assert.Contains("FODC0002", ex.Message);
    }

    [Fact]
    public void CollectionLoader_QueryAndFragmentArePassedUnstripped()
    {
        using var setup = new TempCollectionDir(out _);
        var ctx = new EvaluationContext();
        string? seen = null;
        ctx.CollectionLoader = uri =>
        {
            seen = uri;
            return new[] { setup.FileA };
        };

        var nodes = NodeItems(Evaluate("collection('mydb://coll?select=a.xml')", ctx));

        Assert.Equal("mydb://coll?select=a.xml", seen);
        Assert.Single(nodes);
    }

    [Fact]
    public void CollectionLoader_RelativeUriIsAbsolutizedAgainstBaseUri()
    {
        using var setup = new TempCollectionDir(out _);
        var ctx = new EvaluationContext { BaseUri = "http://example.org/base/" };
        string? seen = null;
        ctx.CollectionLoader = uri =>
        {
            seen = uri;
            return new[] { setup.FileA };
        };

        NodeItems(Evaluate("collection('rel-coll')", ctx));

        Assert.Equal("http://example.org/base/rel-coll", seen);
    }

    [Fact]
    public void CollectionLoader_RegisteredCollectionTakesPrecedence()
    {
        using var setup = new TempCollectionDir(out _);
        var ctx = new EvaluationContext();
        ctx.Collections["mine"] = new[] { setup.FileA };
        ctx.CollectionLoader = _ => new[] { setup.FileB };

        var nodes = NodeItems(Evaluate("collection('mine')", ctx));

        Assert.Single(nodes);
        Assert.Equal("a", RootLocalName(nodes[0]));
    }

    [Fact]
    public void CollectionLoader_MemberLoadFailureMapsToFodc0002()
    {
        var ctx = new EvaluationContext();
        ctx.CollectionLoader = _ => new[] { System.IO.Path.Combine(System.IO.Path.GetTempPath(), "does-not-exist-REQ120.xml") };

        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("collection('mydb://coll')", ctx));
        Assert.Contains("FODC0002", ex.Message);
    }

    [Fact]
    public void CollectionLoader_DocumentIdentityIsSharedWithFnDoc()
    {
        using var setup = new TempCollectionDir(out _);
        var uri = setup.FileA.Replace("\\", "/");
        var ctx = new EvaluationContext();
        ctx.CollectionLoader = _ => new[] { uri };

        var result = Evaluate($"collection('mydb://coll')[1] is doc('{uri}')", ctx);

        Assert.Equal("true", result.ToString());
    }

    [Fact]
    public void Collection_FragmentFromForeignProviderIsGrounded()
    {
        var xml = "<doc><a xml:id='frag1'>one</a><a xml:id='frag2'>two</a></doc>";
        var ctx = new EvaluationContext
        {
            DocumentLoader = uri => new ForeignTestNode(XDocument.Parse(xml), uri)
        };
        ctx.Collections["foreign"] = new[] { "test:///doc.xml#frag2" };

        var nodes = NodeItems(Evaluate("collection('foreign')", ctx));

        Assert.Single(nodes);
        var fragment = nodes[0];
        Assert.Equal(XdmNodeKind.Document, fragment.NodeKind);
        Assert.Equal("test:///doc.xml#frag2", fragment.DocumentUri);
        Assert.Equal("a", RootLocalName(fragment));
        string? fragmentText = null;
        foreach (var child in fragment.Children(XdmNodeKind.Element))
        {
            Assert.True(child.IsNode);
            fragmentText = child.NodeValue!.StringValue;
            break;
        }

        Assert.Equal("two", fragmentText);
    }

    /// <summary>
    /// Creates a temporary directory with two collection member files (<c>a.xml</c>,
    /// <c>b.xml</c>) and deletes them on dispose.
    /// </summary>
    private sealed class TempCollectionDir : IDisposable
    {
        public TempCollectionDir(out string dir)
        {
            Dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            dir = Dir;
            System.IO.Directory.CreateDirectory(Dir);
            FileA = System.IO.Path.Combine(Dir, "a.xml");
            FileB = System.IO.Path.Combine(Dir, "b.xml");
            System.IO.File.WriteAllText(FileA, "<a/>");
            System.IO.File.WriteAllText(FileB, "<b/>");
        }

        public string Dir { get; }

        public string FileA { get; }

        public string FileB { get; }

        public void Dispose() => System.IO.Directory.Delete(Dir, true);
    }
}

/// <summary>
/// Minimal foreign <see cref="IXdmNode"/> test double backed by LINQ-to-XML but deliberately
/// NOT an <c>XDocumentNode</c>, so tests exercise the provider-agnostic engine paths
/// (fragment resolution, document-order registration skip). Implements the mandatory
/// contract members only; optional facets keep their interface defaults.
/// </summary>
internal sealed class ForeignTestNode : IXdmNode
{
    private readonly XObject _node;
    private readonly string _documentUri;

    public ForeignTestNode(XObject node, string documentUri)
    {
        _node = node;
        _documentUri = documentUri;
    }

    public XdmNodeKind NodeKind => _node switch
    {
        XDocument => XdmNodeKind.Document,
        XElement => XdmNodeKind.Element,
        XAttribute => XdmNodeKind.Attribute,
        XText => XdmNodeKind.Text,
        XComment => XdmNodeKind.Comment,
        XProcessingInstruction => XdmNodeKind.ProcessingInstruction,
        _ => XdmNodeKind.Text,
    };

    public string LocalName => _node switch
    {
        XElement e => e.Name.LocalName,
        XAttribute a => a.Name.LocalName,
        XProcessingInstruction pi => pi.Target,
        _ => string.Empty,
    };

    public string NamespaceUri => _node switch
    {
        XElement e => e.Name.NamespaceName,
        XAttribute a => a.Name.NamespaceName,
        _ => string.Empty,
    };

    public string Prefix => _node switch
    {
        XElement e => e.GetPrefixOfNamespace(e.Name.NamespaceName) ?? string.Empty,
        XAttribute => string.Empty,
        _ => string.Empty,
    };

    public string StringValue => _node switch
    {
        XElement e => string.Concat(e.Nodes().OfType<XText>().Select(t => t.Value)),
        XAttribute a => a.Value,
        XText t => t.Value,
        XComment c => c.Value,
        XProcessingInstruction pi => pi.Data,
        _ => string.Empty,
    };

    public XdmValue TypedValue => XdmValue.FromString(StringValue);

    public IXdmNode? Parent
    {
        get
        {
            if (_node is XAttribute attr)
                return attr.Parent is { } p ? Wrap(p) : null;
            return _node.Parent is { } parent ? Wrap(parent) : null;
        }
    }

    public IXdmNode? Document => _node.Document is { } doc ? Wrap(doc) : null;

    public long DocumentOrder => 0;

    public string BaseUri => _documentUri;

    public string DocumentUri => _documentUri;

    public XdmSequence Children(XdmNodeKind kind = XdmNodeKind.All)
    {
        if (_node is not XContainer container)
            return XdmSequence.Empty;

        var items = new List<XdmValue>();
        foreach (var child in container.Nodes())
        {
            var node = Wrap(child);
            if (kind == XdmNodeKind.All || node.NodeKind == kind)
                items.Add(XdmValue.FromNode(node));
        }

        return XdmSequence.FromSource(new ListSequence(items));
    }

    public XdmSequence Attributes(string? localName = null, string? namespaceUri = null)
    {
        if (_node is not XElement element)
            return XdmSequence.Empty;

        var items = new List<XdmValue>();
        foreach (var attr in element.Attributes())
        {
            if (localName is not null && attr.Name.LocalName != localName)
                continue;
            if (namespaceUri is not null && attr.Name.NamespaceName != namespaceUri)
                continue;
            items.Add(XdmValue.FromNode(Wrap(attr)));
        }

        return XdmSequence.FromSource(new ListSequence(items));
    }

    public XdmSequence Axis(XdmAxis axis)
    {
        switch (axis)
        {
            case XdmAxis.Self:
                return XdmSequence.FromSource(new ListSequence(new List<XdmValue> { XdmValue.FromNode(this) }));
            case XdmAxis.Child:
                return Children();
            case XdmAxis.Attribute:
                return Attributes();
            case XdmAxis.Descendant:
                return Descendants(includeSelf: false);
            case XdmAxis.DescendantOrSelf:
                return Descendants(includeSelf: true);
            case XdmAxis.Parent:
                return Parent is { } p
                    ? XdmSequence.FromSource(new ListSequence(new List<XdmValue> { XdmValue.FromNode(p) }))
                    : XdmSequence.Empty;
            case XdmAxis.Ancestor:
                return Ancestors(includeSelf: false);
            case XdmAxis.AncestorOrSelf:
                return Ancestors(includeSelf: true);
            default:
                return XdmSequence.Empty;
        }
    }

    public bool IsSameNode(IXdmNode other)
        => other is ForeignTestNode foreign && ReferenceEquals(foreign._node, _node);

    public string ToXmlString() => _node.ToString() ?? string.Empty;

    private ForeignTestNode Wrap(XObject node) => new(node, _documentUri);

    /// <summary>
    /// List-backed <see cref="IXdmSequence"/> so the double can return multi-item
    /// <see cref="XdmSequence"/>s from the axis APIs.
    /// </summary>
    private sealed class ListSequence : IXdmSequence
    {
        private readonly List<XdmValue> _items;

        public ListSequence(List<XdmValue> items) => _items = items;

        public bool TryGetLength(out int length)
        {
            length = _items.Count;
            return true;
        }

        public IXdmSequenceEnumerator GetEnumerator() => new Enumerator(_items);

        private sealed class Enumerator : IXdmSequenceEnumerator
        {
            private readonly List<XdmValue> _items;
            private int _index = -1;

            public Enumerator(List<XdmValue> items) => _items = items;

            public XdmValue Current => _items[_index];

            public bool MoveNext() => ++_index < _items.Count;
        }
    }

    private XdmSequence Descendants(bool includeSelf)
    {
        var items = new List<XdmValue>();
        if (includeSelf)
            items.Add(XdmValue.FromNode(this));
        if (_node is XContainer container)
        {
            foreach (var desc in container.DescendantNodes())
                items.Add(XdmValue.FromNode(Wrap(desc)));
        }

        return XdmSequence.FromSource(new ListSequence(items));
    }

    private XdmSequence Ancestors(bool includeSelf)
    {
        var items = new List<XdmValue>();
        if (includeSelf)
            items.Add(XdmValue.FromNode(this));
        var current = Parent;
        while (current is not null)
        {
            items.Add(XdmValue.FromNode(current));
            current = current.Parent;
        }

        return XdmSequence.FromSource(new ListSequence(items));
    }
}
