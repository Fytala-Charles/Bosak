// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 November 2026
// PURPOSE              : Verifies streaming wrapper cache identity, weak lifetime and hit allocations
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Providers.Streaming;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.XPath.Providers.Tests.Streaming;

/// <summary>Verifies identity, source ownership, collection and cache-hit allocations.</summary>
public class StreamingWrapperCacheTests
{
    private static StreamingSource Source(XmlReader reader)
        => new(reader, new StreamingLoadOptions(), ownsReader: false);

    /// <summary>Repeated keys preserve wrappers while distinct sources and keys remain distinct.</summary>
    [Fact]
    public void CachedWrappersPreserveIdentityAndSource()
    {
        using var reader = XmlReader.Create(new StringReader("<r/>"));
        using var otherReader = XmlReader.Create(new StringReader("<r/>"));
        var source = Source(reader);
        var otherSource = Source(otherReader);
        var inner = XDocumentNode.Wrap(new XElement("record"));
        var first = source.Wrap(inner, 7);
        Assert.Same(first, source.Wrap(inner, 7));
        Assert.Same(source, first.Source);
        Assert.Same(inner.UnderlyingObject, first.UnderlyingXObject);
        Assert.Same(source.RootNode, first.Parent);
        Assert.NotSame(first, source.Wrap(XDocumentNode.Wrap(new XElement("record")), 8));
        var other = otherSource.Wrap(inner, 7);
        Assert.NotSame(first, other);
        Assert.Same(otherSource, other.Source);
        Assert.Same(otherSource.RootNode, other.Parent);
    }

    /// <summary>Concurrent cache misses publish one wrapper for the shared key.</summary>
    [Fact]
    public void RacingLookupsPublishOneWrapper()
    {
        using var reader = XmlReader.Create(new StringReader("<r/>"));
        var source = Source(reader);
        var inner = XDocumentNode.Wrap(new XElement("record"));
        var wrappers = new StreamingNode[256];
        Parallel.For(0, wrappers.Length, i => wrappers[i] = source.Wrap(inner, 3));
        Assert.All(wrappers, wrapper => Assert.Same(wrappers[0], wrapper));
        Assert.Same(source, wrappers[0].Source);
    }

    /// <summary>Warmed cache hits allocate no managed objects on the calling thread.</summary>
    [Fact]
    public void WarmCacheHitsDoNotAllocate()
    {
        using var reader = XmlReader.Create(new StringReader("<r/>"));
        var source = Source(reader);
        var inner = XDocumentNode.Wrap(new XElement("record"));
        var expected = source.Wrap(inner, 4);
        for (var i = 0; i < 10000; i++) _ = source.Wrap(inner, 4);
        var before = GC.GetAllocatedBytesForCurrentThread();
        StreamingNode? last = null;
        for (var i = 0; i < 100000; i++) last = source.Wrap(inner, 4);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Same(expected, last);
        Assert.Equal(0L, allocated);
        GC.KeepAlive(source);
        GC.KeepAlive(inner);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Key, WeakReference Wrapper) ReleasedRecord(StreamingSource source)
    {
        var key = XDocumentNode.Wrap(new XElement("record"));
        var wrapper = source.Wrap(key, 1);
        return (new WeakReference(key), new WeakReference(wrapper));
    }

    /// <summary>A live source does not retain released record keys or their wrappers.</summary>
    [Fact]
    public void LiveSourceDoesNotRetainReleasedRecords()
    {
        using var reader = XmlReader.Create(new StringReader("<r/>"));
        var source = Source(reader);
        var released = ReleasedRecord(source);
        for (var i = 0; i < 10 && (released.Key.IsAlive || released.Wrapper.IsAlive); i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        Assert.False(released.Key.IsAlive);
        Assert.False(released.Wrapper.IsAlive);
        GC.KeepAlive(source);
    }
}
