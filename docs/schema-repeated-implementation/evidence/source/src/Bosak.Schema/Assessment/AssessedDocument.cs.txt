// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Owns a validated document snapshot and exports explicitly bounded typed or stripped copies.
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
//                      | Charles Korthout | 0.2   | 29-10-2026     | Keep native absence distinct while exporting validated decimal child copies             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 29-10-2026     | Add bounded native document assessment bridge and original-state copy handling           |
//                      | Charles Korthout | 0.4   | 29-10-2026     | Document present-only child state and explicit absent projection refusal (0.6)           |
//                      | Charles Korthout | 0.5   | 29-10-2026     | Refuse ambiguous singular projection rather than choosing a repeated value (0.7)         |
// ===========================================================================================================================================================

using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Providers.Xml;

namespace Bosak.Schema;

/// <summary>Owns a successful assessment snapshot and its original endpoint-bound component state.</summary>
/// <remarks>No caller XML or component collection is retained mutably. Exported XML copies are caller-owned;
/// their subsequent modification does not alter this assessment. Validity is not a live guarantee for edited copies.</remarks>
public sealed class AssessedDocument
{
    private readonly XDocument _snapshot;
    internal AssessedDocument(XsdDocumentAssessor schema, XDocument snapshot, IEnumerable<AssessedElement> elements, Guid? assessmentIdentity = null)
    {
        Schema = schema;
        _snapshot = new XDocument(snapshot);
        Elements = Array.AsReadOnly(elements.ToArray());
        AssessmentIdentity = assessmentIdentity ?? Guid.NewGuid();
        SnapshotIdentity = Guid.NewGuid();
    }
    /// <summary>The original compiled assessor retaining declaration/type component authority.</summary>
    public XsdDocumentAssessor Schema { get; }
    /// <summary>The original assessment-instance label, preserved across explicit assessment copies.</summary>
    public Guid AssessmentIdentity { get; }
    /// <summary>The distinct snapshot-instance label; neither label proves cross-schema compatibility.</summary>
    public Guid SnapshotIdentity { get; }
    /// <summary>Original root and present child PSVI states in instance order, including the anonymous root type.</summary>
    public IReadOnlyList<AssessedElement> Elements { get; }
    /// <summary>Copies the owned snapshot while retaining original PSVI and schema component authority.</summary>
    /// <returns>An independent snapshot with the same original assessment and component identities.</returns>
    public AssessedDocument Copy() => new(Schema, _snapshot, Elements, AssessmentIdentity);

    /// <summary>Creates a caller-owned whole-document copy with original assessment annotations preserved or stripped.</summary>
    /// <param name="preserveTypeAnnotations">Whether to retain original anonymous-root and decimal component state.</param>
    /// <returns>An independent XML document; preserved annotations require the qualified 1.34 provider bridge.</returns>
    public XDocument CreateDocumentCopy(bool preserveTypeAnnotations)
    {
        var result = new XDocument(_snapshot);
        if (preserveTypeAnnotations)
        {
            XdmElementAssessment.AnonymousElementOnly(this, Elements[0]).AttachTo(result.Root!);
            var children = result.Root!.Elements().ToArray();
            for (var i = 0; i < children.Length; i++)
                XdmElementAssessment.Decimal(this, Elements[i + 1], Elements[i + 1].TypedValue!.Value.DecimalValue).AttachTo(children[i]);
        }
        return result;
    }

    /// <summary>Exports a detached decimal child copy with its validated built-in type for legacy Bosak queries.</summary>
    /// <param name="name">The direct child declaration's expanded name.</param>
    /// <returns>A caller-owned detached element annotated with the built-in decimal type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
    /// <exception cref="NotSupportedException">The requested child is absent, ambiguous, or is not a supported assessed decimal child.</exception>
    /// <remarks>This is a built-in typed-value projection, not full local-declaration or anonymous-root interchange.</remarks>
    public XElement CreateTypedElementCopy(XName name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var index = -1;
        for (var i = 1; i < Elements.Count; i++)
            if (Elements[i].Declaration.Name == name)
            {
                if (index >= 0)
                    throw new NotSupportedException("BSK-XSD-016: Singular projection is ambiguous for repeated child occurrences; use the whole-document export.");
                index = i;
            }
        if (index < 0)
            throw new NotSupportedException("BSK-XSD-016: Only assessed decimal child declarations can be projected.");
        var result = new XElement(_snapshot.Root!.Elements().ElementAt(index - 1));
        result.Value = Elements[index].NormalizedValue!;
        XdmSchemaAnnotator.Annotate(result, DecimalSchemaInfo.Instance);
        return result;
    }

    internal sealed class DecimalSchemaInfo : IXmlSchemaInfo
    {
        internal static readonly DecimalSchemaInfo Instance = new();
        private static readonly XmlSchemaSimpleType DecimalType = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Decimal);
        /// <inheritdoc/>
        public XmlSchemaValidity Validity => XmlSchemaValidity.Valid;
        /// <inheritdoc/>
        public bool IsDefault => false;
        /// <inheritdoc/>
        public bool IsNil => false;
        /// <inheritdoc/>
        public XmlSchemaSimpleType? MemberType => null;
        /// <inheritdoc/>
        public XmlSchemaType SchemaType => DecimalType;
        /// <inheritdoc/>
        public XmlSchemaElement? SchemaElement => null;
        /// <inheritdoc/>
        public XmlSchemaAttribute? SchemaAttribute => null;
    }
}
