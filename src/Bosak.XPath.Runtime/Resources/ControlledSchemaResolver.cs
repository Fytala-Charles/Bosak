// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : XmlResolver that routes nested schema document acquisition through the controlled resource policy (REQ-125 Slice B).
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice B)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;

namespace Bosak.XPath.Runtime.Resources;

/// <summary>
/// An <see cref="XmlResolver"/> that answers every schema-document request (nested
/// <c>xs:import</c>/<c>xs:include</c>/<c>xs:redefine</c> targets, and by-URI schema adds)
/// through a <see cref="ControlledResourcePolicy"/>: the host authority supplies the bytes
/// or the request is refused (XV0004/XV0005). No disk or network access is performed.
/// </summary>
/// <remarks>
/// Used by the XSLT schema-aware compilation under the controlled profile in place of the
/// default <see cref="XmlUrlResolver"/>, so the DTD/entity policy and the no-fallback
/// guarantee apply to schema acquisition as they do to every other XML acquisition path.
/// </remarks>
public sealed class ControlledSchemaResolver : XmlResolver
{
    private readonly ControlledResourcePolicy _policy;

    /// <summary>Initializes a new resolver over the supplied policy.</summary>
    /// <param name="policy">The controlling resource policy. Must not be null.</param>
    /// <exception cref="ArgumentNullException"><paramref name="policy"/> is null.</exception>
    public ControlledSchemaResolver(ControlledResourcePolicy policy)
        => _policy = policy ?? throw new ArgumentNullException(nameof(policy));

    /// <summary>
    /// Resolves a schema document by asking the policy's authority for approved bytes.
    /// </summary>
    /// <param name="absoluteUri">The absolute URI of the schema document being resolved.</param>
    /// <param name="role">Unused (schema resolution carries no role).</param>
    /// <param name="ofObjectToReturn">The type of object to return; only streams are supported.</param>
    /// <returns>A read-only stream over the approved bytes.</returns>
    /// <exception cref="InvalidOperationException">The authority refused the acquisition (XV0004/XV0005).</exception>
    /// <exception cref="XmlException">The requested return type is not supported.</exception>
    public override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
    {
        if (ofObjectToReturn is not null && ofObjectToReturn != typeof(Stream))
            throw new XmlException($"The controlled schema resolver only supports streams, not '{ofObjectToReturn}'.");
        var bytes = _policy.AcquireBytes(absoluteUri.AbsoluteUri, ControlledResourceRoute.Schema, out _);
        return new MemoryStream(bytes, writable: false);
    }
}
