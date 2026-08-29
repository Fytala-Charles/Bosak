// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 August 2026
// PURPOSE              : Records unparsed entity declarations extracted from a document's DTD.
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-08-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 29-08-2026     | Added BaseUri for resolving relative unparsed-entity system identifiers                  |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Collections.Generic;

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Annotation attached to an <see cref="System.Xml.Linq.XDocument"/> that records
/// unparsed entity declarations extracted from the document's DTD.
/// </summary>
internal sealed class UnparsedEntityAnnotation
{
    /// <summary>
    /// Information recorded for a single unparsed entity declaration.
    /// </summary>
    public sealed class EntityInfo
    {
        /// <summary>System identifier as declared in the DTD (may be relative).</summary>
        public string SystemId { get; set; } = string.Empty;

        /// <summary>Public identifier as declared in the DTD, or empty if none.</summary>
        public string PublicId { get; set; } = string.Empty;

        /// <summary>Name of the notation associated with this unparsed entity.</summary>
        public string NotationName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Unparsed entities keyed by entity name. The first declaration for a given
    /// name wins, matching XML entity declaration semantics.
    /// </summary>
    public Dictionary<string, EntityInfo> Entities { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Base URI of the document that contained the DTD, used to resolve relative
    /// unparsed entity system identifiers.
    /// </summary>
    public string BaseUri { get; set; } = string.Empty;
}
