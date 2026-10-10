// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Declares the acquisition routes covered by the controlled resource policy (REQ-125 Slice B).
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

namespace Bosak.XPath.Runtime.Resources;

/// <summary>
/// One acquisition route through which the engine can obtain external resources. The
/// controlled resource policy (see <see cref="ControlledResourcePolicy"/>) tags every
/// acquisition request with a route so the host authority can decide per route, and
/// declares which routes are unsupported — an unsupported route is always refused under
/// the controlled profile, never silently runnable without controls.
/// </summary>
/// <remarks>
/// The route set mirrors REQ-125 §3.2: stylesheet module inclusion, package acquisition,
/// schema acquisition, static evaluation, document/secondary input, text and JSON
/// resources, collections, source documents (including the streaming path), dynamic
/// transform and evaluation contexts. Routes the engine cannot place under policy
/// (IO-capable extension functions) are declared explicitly and refused by declaration.
/// </remarks>
public enum ControlledResourceRoute
{
    /// <summary>An <c>xsl:include</c> module fetched at compile time.</summary>
    ModuleInclude,

    /// <summary>An <c>xsl:import</c> module fetched at compile time.</summary>
    ModuleImport,

    /// <summary>A package acquired through <c>xsl:use-package</c> or the <c>fn:transform</c> package registry.</summary>
    Package,

    /// <summary>A schema document acquired through <c>xsl:import-schema</c> location hints.</summary>
    Schema,

    /// <summary>A resource referenced while a static expression (<c>use-when</c>, static AVT) is evaluated.</summary>
    StaticEvaluation,

    /// <summary>
    /// A secondary XML input: <c>fn:doc</c>, <c>fn:document</c>, <c>fn:doc-available</c> probes,
    /// collection members, compile-time <c>parameter-document</c> resources and
    /// <c>fn:serialize</c> parameter documents.
    /// </summary>
    Document,

    /// <summary>A resource fetched by <c>fn:unparsed-text</c>, <c>fn:unparsed-text-lines</c> or <c>fn:unparsed-text-available</c>.</summary>
    TextResource,

    /// <summary>A resource fetched by <c>fn:json-doc</c>.</summary>
    JsonResource,

    /// <summary>
    /// A collection resolved by <c>fn:collection</c> / <c>fn:uri-collection</c>: the collection
    /// URI itself is authorized first, then every member document is authorized through this
    /// route again before it is loaded.
    /// </summary>
    Collection,

    /// <summary>A document loaded by non-streamed <c>xsl:source-document</c>.</summary>
    SourceDocument,

    /// <summary>A document loaded by streamable (<c>streamable="yes"</c>) <c>xsl:source-document</c>.</summary>
    SourceDocumentStreaming,

    /// <summary>
    /// A stylesheet acquired by <c>fn:transform</c> through <c>stylesheet-location</c> or a
    /// registered package location.
    /// </summary>
    Transform,

    /// <summary>
    /// A resource referenced inside <c>xsl:evaluate</c>. Evaluation inherits the controlling
    /// context's policy, so acquisitions are tagged with their concrete route; this value
    /// documents the nested-evaluation inheritance route itself.
    /// </summary>
    Evaluate,

    /// <summary>
    /// Host-registered extension functions capable of IO. The engine cannot intercept host
    /// code, so this route is declared unsupported and refused by declaration: policy-aware
    /// hosts must not register IO-capable extension functions under the controlled profile.
    /// </summary>
    ExtensionFunction,
}
