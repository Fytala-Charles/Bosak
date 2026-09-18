// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Public contract of a streamed document: per-record and stream-completion hooks
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
//                      | Charles Korthout | 0.2   | 17-09-2026     | EnableReplay lets the XSLT engine opt a not-yet-started stream into record retention    |
//                      |                  |       |                | when the stylesheet contains xsl:fork (si-fork-119/816)                                  |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Providers.Streaming;

/// <summary>
/// Implemented by the document node of a streaming (burst-mode) source. Lets the XSLT
/// engine install per-record and stream-completion callbacks: whitespace stripping per
/// record and push-style accumulator evaluation (Phase B streaming accumulators).
/// </summary>
public interface IStreamingDocument
{
    /// <summary>
    /// Gets or sets the callback invoked on each top-level record node (element, text,
    /// comment, or processing instruction) immediately after it is materialized and
    /// wrapped, before it is exposed to the engine. Seeded from
    /// <see cref="StreamingLoadOptions.RecordPostProcessor"/>; the XSLT engine composes
    /// whitespace stripping and accumulator evaluation into this slot. Returning
    /// <c>false</c> drops the record from the stream (it is never yielded), which is how
    /// <c>xsl:strip-space</c> removes whitespace text records between elements.
    /// </summary>
    Func<System.Xml.Linq.XObject, Bosak.XPath.Core.Xdm.IXdmNode, bool>? RecordPostProcessor { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked once, when the stream has been read to the end
    /// of the root element. Used to fire document-node <c>phase="end"</c> accumulator rules.
    /// </summary>
    Action? StreamCompleted { get; set; }

    /// <summary>
    /// Gets whether the stream can currently be drained: nothing has been pulled yet, or
    /// the stream is already complete. False while another enumeration is mid-flight.
    /// </summary>
    bool CanDrain { get; }

    /// <summary>
    /// Consumes the remainder of the stream without exposing records to the caller
    /// (a grounding operation, e.g. publishing document-level accumulator-after values).
    /// Records still flow through <see cref="RecordPostProcessor"/> and are released.
    /// </summary>
    /// <exception cref="StreamingException">Another enumeration is mid-flight (<see cref="CanDrain"/> is false).</exception>
    void Drain();

    /// <summary>
    /// Enables replay of the record stream: second and later enumerations of the root's
    /// children or descendants then succeed, at the cost of retaining every record in
    /// memory. Returns <c>false</c> when the stream has already been partially consumed
    /// (records yielded before this call cannot be replayed). Idempotent: returns
    /// <c>true</c> when replay was already enabled.
    /// </summary>
    bool EnableReplay();
}
