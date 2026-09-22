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
//                      | Charles Korthout | 0.3   | 21-09-2026     | API freeze stage D: StreamCompleted is an event; EnableReplay -> TryEnableReplay;        |
//                      |                  |       |                | RecordPostProcessor is internal (no longer an interface member)                          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;

namespace Bosak.XPath.Providers.Streaming;

/// <summary>
/// The document node of a streamed source. Routes the <see cref="IStreamingDocument"/>
/// surface to the owning <see cref="StreamingSource"/> so the engine can compose
/// stream-completion work; the composed per-record callback is set through the internal
/// <see cref="RecordPostProcessor"/> property (per-record processing is seeded from
/// <see cref="StreamingLoadOptions.RecordPostProcessor"/> at load time).
/// </summary>
internal sealed class StreamingDocumentNode : StreamingNode, IStreamingDocument
{
    internal StreamingDocumentNode(StreamingSource source, XDocumentNode inner)
        : base(source, inner, StreamingNodeRole.Document, recordIndex: -1)
    {
    }

    /// <summary>
    /// The composed per-record callback: seeded from
    /// <see cref="StreamingLoadOptions.RecordPostProcessor"/> at load time and replaced by
    /// the XSLT engine once the document is loaded (the composition needs the loaded
    /// document's shell, so it cannot be supplied through the load options). Returning
    /// <c>false</c> drops the record from the stream. Internal rather than interface
    /// surface per the API freeze (consumers pass the callback via the load options).
    /// </summary>
    internal Func<XObject, Bosak.XPath.Core.Xdm.IXdmNode, bool>? RecordPostProcessor
    {
        get => Source.RecordPostProcessor;
        set => Source.RecordPostProcessor = value;
    }

    /// <inheritdoc/>
    public event Action? StreamCompleted
    {
        add => Source.StreamCompleted += value;
        remove => Source.StreamCompleted -= value;
    }

    /// <inheritdoc/>
    public bool CanDrain => Source.CanDrain;

    /// <inheritdoc/>
    public void Drain() => Source.Drain();

    /// <inheritdoc/>
    public bool TryEnableReplay() => Source.TryEnableReplay();
}
