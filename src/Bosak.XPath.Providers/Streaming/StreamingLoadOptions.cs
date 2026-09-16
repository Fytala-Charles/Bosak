// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Options for loading an XML source as a forward-only streaming document
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
//                      | Charles Korthout | 0.2   | 16-09-2026     | Phase B: RecordPostProcessor is a node-level Func with drop support (was element Action) |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Providers.Streaming;

/// <summary>
/// Options controlling how <see cref="XmlStreamingProvider"/> exposes an XML source as a
/// forward-only streaming document node.
/// </summary>
public sealed class StreamingLoadOptions
{
    /// <summary>
    /// Gets or sets the base URI reported for the streamed document and its nodes.
    /// Used to resolve relative URIs (for example by <c>fn:doc</c>).
    /// </summary>
    public string? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the document URI reported for the streamed document
    /// (<c>fn:document-uri</c>). Defaults to <see cref="BaseUri"/> when not set.
    /// </summary>
    public string? DocumentUri { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="System.Xml.XmlReaderSettings"/> used to create the
    /// underlying reader when loading from a <see cref="System.IO.Stream"/>. When null,
    /// safe defaults are used (<see cref="System.Xml.DtdProcessing.Prohibit"/>,
    /// whitespace preserved). Ignored when an <see cref="System.Xml.XmlReader"/> is
    /// supplied directly.
    /// </summary>
    public System.Xml.XmlReaderSettings? ReaderSettings { get; set; }

    /// <summary>
    /// Gets or sets an optional callback invoked on each top-level record node
    /// (element, text, comment, or processing instruction) immediately after it is
    /// materialized from the stream and wrapped, before it is exposed to the engine.
    /// Returning <c>false</c> drops the record from the stream. Used by the XSLT layer
    /// to apply <c>xsl:strip-space</c>/<c>xsl:preserve-space</c> rules and accumulator
    /// evaluation per record.
    /// </summary>
    public Func<System.Xml.Linq.XObject, Bosak.XPath.Core.Xdm.IXdmNode, bool>? RecordPostProcessor { get; set; }
}
