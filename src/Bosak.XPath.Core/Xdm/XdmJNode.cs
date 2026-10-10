// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 oktober 2026
// PURPOSE              : An XDM JNode value (XPath 4.0): a wrapper around a map, array, or any other item carrying its entry key and parent.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation — XPath 4.0 §17.7 JNode model (fn:jtree/fn:jkey/fn:jvalue)                     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// Represents an XPath 4.0 JNode (F&amp;O 4.0 §17.7): a wrapper item around a map, array,
/// or any other value that additionally records the key by which the value was reached
/// from its parent (a map entry key, a 1-based array position, or a sequence position)
/// and a reference to the parent JNode. A root JNode created by <c>fn:jtree</c> has no
/// key and no parent.
/// </summary>
public sealed class XdmJNode
{
    /// <summary>Creates a root JNode wrapping <paramref name="value"/> with no key and no parent.</summary>
    /// <param name="value">The wrapped value (map, array, atomic, node, function item, or sequence).</param>
    public XdmJNode(XdmValue value)
        : this(value, XdmValue.Undefined, null)
    {
    }

    /// <summary>Creates a JNode wrapping <paramref name="value"/> reached via <paramref name="key"/> from <paramref name="parent"/>.</summary>
    /// <param name="value">The wrapped value.</param>
    /// <param name="key">The entry key or 1-based position by which this value was reached; <see cref="XdmValue.Undefined"/> for a root.</param>
    /// <param name="parent">The parent JNode, or null for a root.</param>
    public XdmJNode(XdmValue value, XdmValue key, XdmJNode? parent)
    {
        Value = value;
        Key = key;
        Parent = parent;
    }

    /// <summary>Gets the wrapped value.</summary>
    public XdmValue Value { get; }

    /// <summary>Gets the key by which this value was reached from its parent (map entry key or 1-based position); <see cref="XdmValue.Undefined"/> for a root.</summary>
    public XdmValue Key { get; }

    /// <summary>Gets the parent JNode, or null for a root.</summary>
    public XdmJNode? Parent { get; }

    /// <summary>Returns whether this JNode is a root (no key, no parent).</summary>
    public bool IsRoot => Key.IsUndefined && Parent is null;
}
