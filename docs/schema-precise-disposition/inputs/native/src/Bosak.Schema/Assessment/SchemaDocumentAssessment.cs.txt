// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Immutable outcomes and original declaration/type state for native document assessment.
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
//                      | Charles Korthout | 0.2   | 29-10-2026     | Expose the supported non-nilled state explicitly |
//                      | Charles Korthout | 0.3   | 29-10-2026     | Preserve absence of a root normalized value and original no-typed-value state           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 29-10-2026     | Bind local occurrence to internal particles; keep original declaration identities (0.6)  |
//                      | Charles Korthout | 0.5   | 29-10-2026     | Store finite local occurrence bounds separately from declaration identity (0.7)          |
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;

namespace Bosak.Schema;

/// <summary>The outcome of native assessment within an explicitly supported profile.</summary>
public enum SchemaAssessmentOutcome
{
    /// <summary>Structure, exact decimal values and every assertion passed.</summary>
    Valid,
    /// <summary>The instance violates a supported structural, lexical or assertion constraint.</summary>
    Invalid,
    /// <summary>The request exceeds implemented schema, instance, expression or representation coverage.</summary>
    Unsupported,
}

/// <summary>Original type component identity, scoped to the compiling assessor for anonymous types.</summary>
public sealed class AssessedSchemaType
{
    internal AssessedSchemaType(XName? name, bool isComplex) { Name = name; IsComplex = isComplex; }
    /// <summary>The expanded type name, or null for the anonymous root complex type.</summary>
    public XName? Name { get; }
    /// <summary>Whether this is the element-only anonymous root complex type.</summary>
    public bool IsComplex { get; }
    /// <summary>Whether elements of this type have a typed value.</summary>
    public bool HasTypedValue => !IsComplex;
}

/// <summary>Original element declaration identity; equal names do not imply shared component authority.</summary>
public sealed class AssessedElementDeclaration
{
    internal AssessedElementDeclaration(XName name, AssessedSchemaType type, string schemaPath)
    { Name = name; Type = type; SchemaPath = schemaPath; }
    /// <summary>The expanded element name.</summary>
    public XName Name { get; }
    /// <summary>The actual declared type component.</summary>
    public AssessedSchemaType Type { get; }
    /// <summary>The declaration's location in the retained single-document schema.</summary>
    public string SchemaPath { get; }
}

/// <summary>Internal local particle binding; occurrence is separate from declaration identity.</summary>
internal readonly record struct AssessmentParticle(AssessedElementDeclaration Declaration, int MinOccurs, int MaxOccurs);

/// <summary>Immutable original assessment state for one present element in the owned snapshot.</summary>
public sealed class AssessedElement
{
    internal AssessedElement(AssessedElementDeclaration declaration, string? normalizedValue, XdmValue? typedValue)
    { Declaration = declaration; NormalizedValue = normalizedValue; TypedValue = typedValue; }
    /// <summary>The governing original declaration and its actual type component.</summary>
    public AssessedElementDeclaration Declaration { get; }
    /// <summary>The schema-normalized decimal lexical value, or null for the element-only root.</summary>
    public string? NormalizedValue { get; }
    /// <summary>The exact typed decimal value, or null for the element-only root (not an empty typed sequence).</summary>
    public XdmValue? TypedValue { get; }
    /// <summary>Whether atomization must fail because the element has no typed value.</summary>
    public bool HasNoTypedValue => !Declaration.Type.HasTypedValue;
    /// <summary>Whether this element is nilled; nil requests are explicitly refused by this profile.</summary>
    public bool IsNilled => false;
}

/// <summary>A complete profile assessment outcome; unsuccessful outcomes expose no assessed document.</summary>
public sealed class SchemaDocumentAssessment
{
    internal SchemaDocumentAssessment(SchemaAssessmentOutcome outcome, IEnumerable<SchemaDiagnostic> diagnostics, AssessedDocument? document = null)
    { Outcome = outcome; Diagnostics = Array.AsReadOnly(diagnostics.ToArray()); Document = document; }
    /// <summary>The successful, invalid or explicitly unsupported outcome.</summary>
    public SchemaAssessmentOutcome Outcome { get; }
    /// <summary>Immutable findings; unsupported coverage is distinct from invalid XML.</summary>
    public IReadOnlyList<SchemaDiagnostic> Diagnostics { get; }
    /// <summary>The immutable owned document and original PSVI, available only on success.</summary>
    public AssessedDocument? Document { get; }
}
