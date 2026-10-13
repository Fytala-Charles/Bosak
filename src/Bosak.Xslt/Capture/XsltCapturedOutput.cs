// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Immutable captured output: resolved identity, effective output properties, serialization bytes.
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — controlled validation and preview API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice C)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Capture;

/// <summary>
/// One immutable captured output of a transformation: its resolved identity (absolute URI), whether
/// it is the principal output, its effective serialization properties, and the exact serialization
/// bytes. The bytes are the serialization the output definition produces — encoding, byte-order mark,
/// output method and all other effective properties are honored — and no output is ever written to
/// disk or the network in capture mode.
/// </summary>
/// <remarks>
/// <para>
/// Identity rule: a secondary output's identity is its <c>xsl:result-document</c> href resolved
/// against the base output URI. The principal output always has an identity — the base output URI —
/// even when the stylesheet never names it; when the host supplied no base output URI the identity is
/// the empty string.
/// </para>
/// <para>
/// Byte fidelity: <see cref="Bytes"/> are produced from the same serializer and effective output
/// properties as the engine's captured (<c>fn:transform</c> serialized-delivery) result documents,
/// encoded with the declared output encoding; a declared byte-order mark is emitted as real preamble
/// bytes. The legacy disk-write path for secondary HTML outputs additionally self-closes void elements
/// so the file remains well-formed XML; captured bytes intentionally follow the
/// <c>fn:transform</c> delivery contract instead. Exotic encodings whose byte representation the
/// platform cannot produce are reported as classified failures, never as partial bytes.
/// </para>
/// <para>
/// Instances are immutable value-like records: the byte array is privately retained and exposed only
/// as a read-only view, the type holds no unmanaged resources, needs no disposal and is safe to share
/// across threads.
/// </para>
/// </remarks>
public sealed class XsltCapturedOutput
{
    private readonly byte[] _bytes;

    /// <summary>Initializes a new captured output.</summary>
    /// <param name="resolvedUri">The resolved identity of the output (absolute URI; the principal output's base output URI, possibly empty).</param>
    /// <param name="isPrincipal">Whether this is the principal output of the transformation.</param>
    /// <param name="bytes">The exact serialization bytes of the output. Defensively copied.</param>
    /// <param name="outputProperties">The effective output properties the bytes were serialized with.</param>
    /// <exception cref="ArgumentNullException"><paramref name="resolvedUri"/>, <paramref name="bytes"/> or <paramref name="outputProperties"/> is null.</exception>
    public XsltCapturedOutput(string resolvedUri, bool isPrincipal, byte[] bytes, Stylesheet.OutputProperties outputProperties)
    {
        ResolvedUri = resolvedUri ?? throw new ArgumentNullException(nameof(resolvedUri));
        IsPrincipal = isPrincipal;
        _bytes = (byte[])bytes.Clone() ?? throw new ArgumentNullException(nameof(bytes));
        OutputProperties = outputProperties ?? throw new ArgumentNullException(nameof(outputProperties));
    }

    /// <summary>
    /// Gets the resolved identity of the output: the absolute URI of a secondary
    /// <c>xsl:result-document</c>, or the base output URI for the principal output (possibly the empty
    /// string when the host supplied none).
    /// </summary>
    public string ResolvedUri { get; }

    /// <summary>Gets whether this is the principal output of the transformation.</summary>
    public bool IsPrincipal { get; }

    /// <summary>
    /// Gets the exact serialization bytes of the output, encoded with the effective
    /// <see cref="OutputProperties"/> (including a declared byte-order mark as preamble bytes).
    /// </summary>
    public IReadOnlyList<byte> Bytes => _bytes;

    /// <summary>Gets the effective output properties (method, encoding, byte-order mark, and so on) the bytes were serialized with.</summary>
    public Stylesheet.OutputProperties OutputProperties { get; }

    /// <inheritdoc />
    public override string ToString() =>
        $"{(IsPrincipal ? "principal" : "secondary")} output '{ResolvedUri}' ({_bytes.Length} bytes, method {OutputProperties.Method}, encoding {OutputProperties.Encoding})";
}
