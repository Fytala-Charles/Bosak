// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Public contract of a streamed node: access to the underlying LINQ-to-XML object
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
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Providers.Streaming;

/// <summary>
/// Implemented by nodes of a streaming (burst-mode) source. Exposes the underlying
/// LINQ-to-XML object (mirrors <c>XDocumentNode.UnderlyingObject</c>) so the engine can
/// reach per-node annotations — for example the accumulator values attached by the
/// push-style streaming accumulator driver — without knowing the internal wrapper type.
/// </summary>
public interface IStreamingNode : IXdmNode
{
    /// <summary>Gets the underlying LINQ to XML object of the wrapped node.</summary>
    System.Xml.Linq.XObject UnderlyingXObject { get; }
}
