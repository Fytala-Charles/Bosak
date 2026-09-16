// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Options for XsltExecutable.TransformStreaming (burst-mode streaming input)
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
namespace Bosak.Xslt.Api;

/// <summary>
/// Options controlling how <see cref="XsltExecutable.TransformStreaming"/> reads the
/// streamed source document.
/// </summary>
public sealed class StreamingTransformOptions
{
    /// <summary>
    /// Gets or sets the base URI reported for the streamed document; also used as its
    /// document URI. Used to resolve relative URIs (for example by <c>fn:doc</c>).
    /// </summary>
    public string? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="System.Xml.XmlReaderSettings"/> used to create the
    /// stream reader. When null, safe defaults are used
    /// (<see cref="System.Xml.DtdProcessing.Prohibit"/>, whitespace preserved).
    /// </summary>
    public System.Xml.XmlReaderSettings? ReaderSettings { get; set; }

    /// <summary>
    /// Gets or sets whether the stylesheet's <c>xsl:strip-space</c>/<c>xsl:preserve-space</c>
    /// rules are applied to each streamed record as it is materialized. Default true.
    /// </summary>
    public bool HonorWhitespaceRules { get; set; } = true;
}
