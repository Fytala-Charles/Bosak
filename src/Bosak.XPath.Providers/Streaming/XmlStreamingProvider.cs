// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Loads an XML source as a forward-only streaming document node (burst-mode input)
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Providers.Streaming;

/// <summary>
/// Presents an XML source as a lazily-read (streaming) document node. The document and
/// root element are available immediately; the root's children — the top-level
/// <em>records</em> — are materialized one at a time as the engine enumerates them, so
/// record-at-a-time processing runs in bounded memory. Within each record all axes work
/// as usual; once the enumeration advances, earlier records are released.
/// </summary>
/// <remarks>
/// The streamed children of the root form a forward-only sequence: they can be
/// enumerated at most once, and a second enumeration throws a <see cref="StreamingException"/>.
/// Navigation that would need earlier records (the <c>preceding</c> axes beyond the
/// current record) or would interleave with the active read (<c>following</c> axes past
/// the current record) also throws. Operations that inherently need the whole input —
/// sorting, grouping, <c>fn:last()</c> — still work but buffer the stream in memory.
/// Comments and processing instructions before the root element and DTD entity
/// declarations are not surfaced.
/// </remarks>
public static class XmlStreamingProvider
{
    /// <summary>
    /// Loads an XML stream as a forward-only streaming document node.
    /// </summary>
    /// <param name="stream">The stream to read. Disposal follows
    /// <see cref="XmlReaderSettings.CloseInput"/>; by default the stream stays open.</param>
    /// <param name="options">Optional load options (base URI, reader settings, record post-processor).</param>
    /// <returns>The streaming document node.</returns>
    /// <exception cref="StreamingException">The source contains no root element.</exception>
    /// <exception cref="XmlException">The source is not well-formed XML.</exception>
    public static IXdmNode Load(Stream stream, StreamingLoadOptions? options = null)
    {
        options ??= new StreamingLoadOptions();
        var settings = options.ReaderSettings ?? CreateDefaultSettings();
        var reader = XmlReader.Create(stream, settings, options.BaseUri ?? string.Empty);
        return new StreamingSource(reader, options, ownsReader: true).DocumentNode;
    }

    /// <summary>
    /// Presents an existing <see cref="XmlReader"/> as a forward-only streaming document
    /// node. The reader should be positioned before the document start; it is not
    /// disposed by the provider.
    /// </summary>
    /// <param name="reader">The reader to pull records from.</param>
    /// <param name="options">Optional load options (base URI, record post-processor).</param>
    /// <returns>The streaming document node.</returns>
    /// <exception cref="StreamingException">The source contains no root element.</exception>
    /// <exception cref="XmlException">The source is not well-formed XML.</exception>
    public static IXdmNode Load(XmlReader reader, StreamingLoadOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return new StreamingSource(reader, options ?? new StreamingLoadOptions(), ownsReader: false).DocumentNode;
    }

    private static XmlReaderSettings CreateDefaultSettings()
        => new()
        {
            DtdProcessing = DtdProcessing.Prohibit,
            IgnoreWhitespace = false,
            IgnoreComments = false,
            IgnoreProcessingInstructions = false,
            CloseInput = false,
        };
}
