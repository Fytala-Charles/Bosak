// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : The thirteen XPath axes
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// The thirteen XPath axes.
/// </summary>
public enum XdmAxis : byte
{
    /// <summary>The ancestors of the context node (parent, grandparent, and so on).</summary>
    Ancestor,
    /// <summary>The context node and its ancestors.</summary>
    AncestorOrSelf,
    /// <summary>The attributes of the context element.</summary>
    Attribute,
    /// <summary>The children of the context node.</summary>
    Child,
    /// <summary>The descendants of the context node (children, their children, and so on).</summary>
    Descendant,
    /// <summary>The context node and its descendants.</summary>
    DescendantOrSelf,
    /// <summary>All nodes after the context node in document order, excluding descendants and attribute/namespace nodes.</summary>
    Following,
    /// <summary>The following siblings of the context node.</summary>
    FollowingSibling,
    /// <summary>The namespace nodes of the context element.</summary>
    Namespace,
    /// <summary>The parent of the context node.</summary>
    Parent,
    /// <summary>All nodes before the context node in document order, excluding ancestors and attribute/namespace nodes.</summary>
    Preceding,
    /// <summary>The preceding siblings of the context node.</summary>
    PrecedingSibling,
    /// <summary>The context node itself.</summary>
    Self
}
