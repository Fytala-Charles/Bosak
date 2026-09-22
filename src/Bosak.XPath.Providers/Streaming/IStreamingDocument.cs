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
//                      | Charles Korthout | 0.3   | 21-09-2026     | API freeze stage D: StreamCompleted is an event; EnableReplay -> TryEnableReplay;        |
//                      |                  |       |                | RecordPostProcessor removed (supplied via StreamingLoadOptions; engine composes through  |
//                      |                  |       |                | the internal implementation property)                                                    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Providers.Streaming;

/// <summary>
/// Implemented by the document node of a streaming (burst-mode) source. Lets the XSLT
/// engine install stream-completion callbacks: push-style accumulator evaluation
/// (Phase B streaming accumulators). Per-record processing is supplied at load time via
/// <see cref="StreamingLoadOptions.RecordPostProcessor"/>.
/// </summary>
public interface IStreamingDocument
{
    /// <summary>
    /// Raised once, when the stream has been read to the end of the root element.
    /// Used to fire document-node <c>phase="end"</c> accumulator rules.
    /// </summary>
    event Action? StreamCompleted;

    /// <summary>
    /// Gets whether the stream can currently be drained: nothing has been pulled yet, or
    /// the stream is already complete. False while another enumeration is mid-flight.
    /// </summary>
    bool CanDrain { get; }

    /// <summary>
    /// Consumes the remainder of the stream without exposing records to the caller
    /// (a grounding operation, e.g. publishing document-level accumulator-after values).
    /// Records still flow through <see cref="StreamingLoadOptions.RecordPostProcessor"/> and are released.
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
    bool TryEnableReplay();
}
