// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Immutable record of one construct whose static analysis is explicitly deferred.
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — controlled validation and preview API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice A)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Validation;

/// <summary>
/// An immutable, explicit declaration that one construct present in the validated stylesheet was
/// <em>not</em> statically analyzed. Deferred analysis is never presented as a pass: when no
/// diagnostics exist but at least one coverage gap was recorded, the outcome is
/// <see cref="XsltValidationOutcome.UnsupportedCoverage"/> instead of
/// <see cref="XsltValidationOutcome.Valid"/>.
/// </summary>
/// <remarks>
/// Instances are immutable value-like records: they hold no unmanaged resources, need no disposal and
/// are safe to share across threads.
/// </remarks>
public sealed class XsltValidationCoverageGap
{
    /// <summary>Initializes a new coverage gap record.</summary>
    /// <param name="construct">The stylesheet construct whose analysis is deferred (for example <c>xsl:import-schema</c>).</param>
    /// <param name="moduleUri">The absolute URI of the module in which the construct occurs.</param>
    /// <param name="description">The human-readable explanation of what is not analyzed and why.</param>
    /// <exception cref="ArgumentNullException"><paramref name="construct"/>, <paramref name="moduleUri"/> or <paramref name="description"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="construct"/> or <paramref name="description"/> is empty.</exception>
    public XsltValidationCoverageGap(string construct, Uri moduleUri, string description)
    {
        if (string.IsNullOrEmpty(construct))
        {
            throw new ArgumentException("The construct name must not be empty.", nameof(construct));
        }

        if (string.IsNullOrEmpty(description))
        {
            throw new ArgumentException("The description must not be empty.", nameof(description));
        }

        Construct = construct;
        ModuleUri = moduleUri ?? throw new ArgumentNullException(nameof(moduleUri));
        Description = description;
    }

    /// <summary>Gets the stylesheet construct whose analysis is deferred (for example <c>xsl:import-schema</c>).</summary>
    public string Construct { get; }

    /// <summary>Gets the absolute URI of the module in which the construct occurs.</summary>
    public Uri ModuleUri { get; }

    /// <summary>Gets the human-readable explanation of what is not analyzed and why.</summary>
    public string Description { get; }

    /// <inheritdoc />
    public override string ToString() => $"{Construct}: {Description} ({ModuleUri})";
}
