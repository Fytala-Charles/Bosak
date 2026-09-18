// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : The document node of a streamed source; exposes per-record and completion hooks
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
//                      | Charles Korthout | 0.2   | 17-09-2026     | EnableReplay delegates to StreamingSource.EnableReplay (lazy record retention)           |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;

namespace Bosak.XPath.Providers.Streaming;

/// <summary>
/// The document node of a streamed source. Routes the <see cref="IStreamingDocument"/>
/// hook properties to the owning <see cref="StreamingSource"/> so the engine can compose
/// per-record processing and stream-completion work.
/// </summary>
internal sealed class StreamingDocumentNode : StreamingNode, IStreamingDocument
{
    internal StreamingDocumentNode(StreamingSource source, XDocumentNode inner)
        : base(source, inner, StreamingNodeRole.Document, recordIndex: -1)
    {
    }

    /// <inheritdoc/>
    public Func<XObject, Bosak.XPath.Core.Xdm.IXdmNode, bool>? RecordPostProcessor
    {
        get => Source.RecordPostProcessor;
        set => Source.RecordPostProcessor = value;
    }

    /// <inheritdoc/>
    public Action? StreamCompleted
    {
        get => Source.StreamCompleted;
        set => Source.StreamCompleted = value;
    }

    /// <inheritdoc/>
    public bool CanDrain => Source.CanDrain;

    /// <inheritdoc/>
    public void Drain() => Source.Drain();

    /// <inheritdoc/>
    public bool EnableReplay() => Source.EnableReplay();
}
