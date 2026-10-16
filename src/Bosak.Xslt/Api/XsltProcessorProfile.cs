// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Selects processor compatibility independently of stylesheet version.
// SPECIAL NOTES        : Part of the XSLT compilation and execution API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Api;

/// <summary>Selects processor compatibility independently of stylesheet version declarations.</summary>
public enum XsltProcessorProfile
{
    /// <summary>Preserves existing behavior: a 3.0 forward-compatibility ceiling and version-selected XPath.</summary>
    Legacy = 0,

    /// <summary>Opts into the draft 4.0 forward-compatibility ceiling and frozen XPath 4.0 expressions.</summary>
    /// <remarks>This experimental profile does not imply complete XSLT 4.0 conformance.</remarks>
    Xslt40Draft = 1,
}
