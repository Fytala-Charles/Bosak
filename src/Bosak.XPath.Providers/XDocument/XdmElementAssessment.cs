// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Carries original finite-profile assessment state on XML nodes and verifies content bindings.
// SPECIAL NOTES        : Bounded native XSD 1.1 assessment; decimal-sequence-assertions-v1 profile.
//
// COPYRIGHT            : Fytala
// LICENSE              : LICENSE.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-10-2026     | Creation (REQ-009 bounded document assessment, 0.5 development line)                      |
//                      | Charles Korthout | 0.2   | 29-10-2026     | Retain previous binding on failed attachment and clarify trusted-host contract           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 29-10-2026     | Normalize empty element-only root whitespace without relaxing decimal bindings (1.34.1)  |
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Providers.Xml;

/// <summary>Immutable optional bridge for an anonymous element-only type or a built-in decimal element.</summary>
/// <remarks>The trusted host supplies original owner/state objects. They are carried, never admitted by QName
/// or digest. The host must supply immutable state and the correct assessed value; attachment performs no
/// schema validation. This finite bridge does not support declaration matching or cross-schema reassessment.
/// Exported XML must not be concurrently edited while its assessment is read or attached.</remarks>
public sealed class XdmElementAssessment
{
    private XdmElementAssessment(object owner, object state, bool elementOnly, decimal value)
    {
        ArgumentNullException.ThrowIfNull(owner); ArgumentNullException.ThrowIfNull(state);
        Owner = owner; State = state; HasNoTypedValue = elementOnly;
        TypedValue = elementOnly ? null : XdmValue.FromDecimal(value);
    }
    /// <summary>The original owning assessment, retained strongly through copies and escaped nodes.</summary>
    public object Owner { get; }
    /// <summary>The original immutable element/component assessment state, without reinterpretation.</summary>
    public object State { get; }
    /// <summary>Whether the anonymous element-only type has no typed value.</summary>
    public bool HasNoTypedValue { get; }
    /// <summary>The exact decimal value, or null for an element-only element.</summary>
    public XdmValue? TypedValue { get; }
    /// <summary>Creates an original anonymous element-only assessment carrier.</summary>
    /// <param name="owner">Original owning assessment.</param>
    /// <param name="state">Original declaration/type/element state.</param>
    /// <returns>An immutable anonymous-type carrier; no synthetic type name is assigned.</returns>
    public static XdmElementAssessment AnonymousElementOnly(object owner, object state) => new(owner, state, true, default);
    /// <summary>Creates an original built-in decimal assessment carrier.</summary>
    /// <param name="owner">Original owning assessment.</param>
    /// <param name="state">Original declaration/type/element state.</param>
    /// <param name="value">The exactly assessed decimal.</param>
    /// <returns>An immutable built-in decimal carrier.</returns>
    public static XdmElementAssessment Decimal(object owner, object state, decimal value) => new(owner, state, false, value);

    /// <summary>Binds this assessment to a complete node's current XML content.</summary>
    /// <param name="element">The complete caller-owned exported element.</param>
    /// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
    /// <exception cref="NotSupportedException">The content exceeds the finite bridge bounds.</exception>
    /// <remarks>The trusted host supplies the correct assessed value. Failed content checks retain the previous binding.</remarks>
    public void AttachTo(XElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        var binding = new Binding(this, Shape(element, HasNoTypedValue));
        element.RemoveAnnotations<Binding>();
        element.AddAnnotation(binding);
    }
    /// <summary>Reads the original assessment after checking that its bound content is unchanged.</summary>
    /// <param name="element">The element to inspect.</param>
    /// <returns>The original assessment or null for an unregistered node.</returns>
    /// <exception cref="NotSupportedException">The element's assessed content was edited.</exception>
    public static XdmElementAssessment? Get(XElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        var binding = element.Annotation<Binding>();
        if (binding is null) return null;
        if (element.Document?.DocumentType is not null || element.Document?.Declaration?.Version is not (null or "1.0"))
            throw new NotSupportedException("BSK-XSD-016: Changed document version or DTD cannot reuse the original assessment.");
        if (binding.Shape != Shape(element, binding.Assessment.HasNoTypedValue))
            throw new NotSupportedException("BSK-XSD-016: Edited XML cannot reuse its original assessment; strip or reassess explicitly.");
        return binding.Assessment;
    }
    internal static bool IsAttached(XElement element) => element.Annotation<Binding>() is not null;
    internal static void Remove(XElement element) => element.RemoveAnnotations<Binding>();
    internal static void Copy(XElement source, XElement target)
    {
        var assessment = Get(source);
        target.RemoveAnnotations<Binding>();
        if (assessment is not null)
            target.AddAnnotation(source.Annotation<Binding>()!);
    }
    private static string Shape(XElement element, bool elementOnly)
    {
        if (element.DescendantNodes().Take(257).Count() > 256)
            throw new NotSupportedException("BSK-XSD-016: Assessed XML exceeds the bridge's finite node bound.");
        long characters = 0;
        foreach (var node in element.DescendantNodesAndSelf())
        {
            if (node is XText text) characters += text.Value.Length;
            if (node is XComment comment) characters += comment.Value.Length;
            if (node is XProcessingInstruction instruction) characters += instruction.Data.Length;
            if (node is XElement current)
                foreach (var attribute in current.Attributes()) characters += attribute.Value.Length;
            if (characters > 65536) throw new NotSupportedException("BSK-XSD-016: Edited XML exceeds the finite assessment content bound.");
        }
        var copy = new XElement(element);
        copy.DescendantsAndSelf().Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
        foreach (var parent in copy.DescendantsAndSelf().Where(e => e.HasElements || elementOnly && ReferenceEquals(e, copy)))
            parent.Nodes().OfType<XText>().Where(t => t.Value.All(c => c is ' ' or '\t' or '\r' or '\n')).Remove();
        // LINQ XML can retain an explicit empty-string content marker after removing whitespace.
        // Both lexical empty-element forms represent the same assessed element-only content.
        if (elementOnly && !copy.Nodes().Any()) copy.RemoveNodes();
        var value = copy.ToString(SaveOptions.DisableFormatting);
        if (value.Length > 65536) throw new NotSupportedException("BSK-XSD-016: Assessed XML exceeds the bridge's finite content bound.");
        return value;
    }
    private sealed record Binding(XdmElementAssessment Assessment, string Shape);
}
