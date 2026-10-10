// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Classified inspection failure kinds and the immutable failure record.
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — source-preserving authoring API for visual designers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Authoring;

/// <summary>
/// Classifies why an authoring module could not be inspected or resolved. Structure, encoding and
/// resolver outcomes are deliberately distinct kinds so consumers never have to parse messages.
/// </summary>
public enum AuthoringFailureKind
{
    /// <summary>The module bytes could not be parsed as well-formed XML.</summary>
    Structure,

    /// <summary>The module declares or uses an encoding the authoring boundary does not support.</summary>
    UnsupportedEncoding,

    /// <summary>The module bytes are not valid for the detected/supported encoding.</summary>
    InvalidSourceBytes,

    /// <summary>A module resolver failed, returned nothing, or an include/import cycle was detected.</summary>
    ResolverFailure,
}

/// <summary>
/// An immutable, classified description of why an authoring operation did not succeed for one module.
/// Inspection never throws for bad source; it reports instances of this type instead.
/// </summary>
public sealed class AuthoringFailure
{
    /// <summary>Initializes a new authoring failure.</summary>
    /// <param name="kind">The classified failure kind.</param>
    /// <param name="message">A human-readable description.</param>
    /// <param name="moduleUri">The absolute URI of the module the failure belongs to.</param>
    /// <param name="range">The source range the failure relates to, when known.</param>
    public AuthoringFailure(AuthoringFailureKind kind, string message, Uri moduleUri, SourceRange? range = null)
    {
        Kind = kind;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        ModuleUri = moduleUri ?? throw new ArgumentNullException(nameof(moduleUri));
        Range = range;
    }

    /// <summary>Gets the classified failure kind.</summary>
    public AuthoringFailureKind Kind { get; }

    /// <summary>Gets the human-readable failure description.</summary>
    public string Message { get; }

    /// <summary>Gets the absolute URI of the module the failure belongs to.</summary>
    public Uri ModuleUri { get; }

    /// <summary>Gets the source range the failure relates to, when known.</summary>
    public SourceRange? Range { get; }

    /// <inheritdoc />
    public override string ToString() => $"{Kind}: {Message} ({ModuleUri})";
}
