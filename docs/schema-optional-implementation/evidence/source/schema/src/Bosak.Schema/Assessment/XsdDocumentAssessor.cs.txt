// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Natively assesses one bounded XSD 1.1 structure before evaluating its decimal assertions.
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
//                      | Charles Korthout | 0.2   | 29-10-2026     | Bound input capture and refuse unsupported leaf node kinds |
//                      | Charles Korthout | 0.3   | 29-10-2026     | Retain absent root normalized value in native assessment state                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 29-10-2026     | Assess ordered optional particles and assertion EBV with present-only state (0.6)        |
// ===========================================================================================================================================================

using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.Schema;

/// <summary>Immutable compiled bounded decimal-sequence/assertion assessor; not a complete XSD 1.1 processor.</summary>
public sealed class XsdDocumentAssessor
{
    private readonly IReadOnlyList<XPath31Expression> _assertions;
    private readonly AssessmentParticle[] _particles;
    internal XsdDocumentAssessor(string schemaXml, IEnumerable<SchemaDiagnostic> diagnostics,
        AssessedElementDeclaration? root, IEnumerable<AssessmentParticle> particles, IEnumerable<XPath31Expression> assertions)
    {
        SchemaXml = schemaXml;
        SchemaDigest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(schemaXml))).ToLowerInvariant();
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
        RootDeclaration = root;
        _particles = particles.ToArray();
        ChildDeclarations = Array.AsReadOnly(_particles.Select(p => p.Declaration).ToArray());
        Profile = _particles.Any(p => p.IsOptional) ? "decimal-sequence-assertions-v2" : "decimal-sequence-assertions-v1";
        _assertions = Array.AsReadOnly(assertions.ToArray());
    }
    /// <summary>The exact finite supported profile identifier.</summary>
    public string Profile { get; }
    /// <summary>The explicit schema language version assessed by this service.</summary>
    public XsdLanguageVersion LanguageVersion => XsdLanguageVersion.Xsd11;
    /// <summary>The retained single-document schema XML; imports/includes are never accepted.</summary>
    public string SchemaXml { get; }
    /// <summary>A content label only; equal digests do not admit independent schema authorities.</summary>
    public string SchemaDigest { get; }
    /// <summary>Whether compilation established complete coverage for this finite profile.</summary>
    public bool IsSupported => Diagnostics.Count == 0;
    /// <summary>Immutable compile diagnostics, including every encountered unsupported request.</summary>
    public IReadOnlyList<SchemaDiagnostic> Diagnostics { get; }
    /// <summary>The original global root declaration and anonymous element-only type, or null after refusal.</summary>
    public AssessedElementDeclaration? RootDeclaration { get; }
    /// <summary>All original local declarations in schema order, including omitted optional particles.</summary>
    public IReadOnlyList<AssessedElementDeclaration> ChildDeclarations { get; }

    /// <summary>Checks native structure and exact decimal values, then evaluates every supported assertion.</summary>
    /// <param name="document">Caller-owned instance document, copied before assessment.</param>
    /// <param name="cancellationToken">Cancellation checked before snapshot capture and between bounded operations.</param>
    /// <returns>An outcome exposing an owned assessed document only after every check passes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="document"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Cancellation is requested.</exception>
    public SchemaDocumentAssessment Assess(XDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsSupported) return new(SchemaAssessmentOutcome.Unsupported, Diagnostics);
        if (document.DocumentType is not null || document.Declaration?.Version is not (null or "1.0") ||
            !AssessmentProfileParser.WithinInputBounds(document, 65536) || document.ToString(SaveOptions.DisableFormatting).Length > 65536)
            return Refuse("The instance exceeds this profile's XML version, DTD or size coverage.");
        var snapshot = new XDocument(document);
        var root = snapshot.Root;
        if (root is null || root.Name != RootDeclaration!.Name) return Invalid("The instance root does not match its declaration.");
        var all = root.DescendantsAndSelf().ToArray();
        if (all.Any(e => e.Attributes().Any(a => !a.IsNamespaceDeclaration && a.Name.NamespaceName == "http://www.w3.org/2001/XMLSchema-instance")))
            return Refuse("xsi:type, nil and schema-location requests are not supported by this profile.");
        if (all.Any(e => e.Attributes().Any(a => !a.IsNamespaceDeclaration))) return Invalid("This profile declares no instance attributes.");
        if (root.Nodes().OfType<XText>().Any(t => !AssessmentProfileParser.IsXmlWhitespace(t.Value))) return Invalid("The root permits element-only content.");
        var children = root.Elements().ToArray();
        var presentDeclarations = new List<AssessedElementDeclaration>(children.Length);
        var childIndex = 0;
        foreach (var particle in _particles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (childIndex < children.Length && children[childIndex].Name == particle.Declaration.Name)
            {
                presentDeclarations.Add(particle.Declaration);
                childIndex++;
            }
            else if (!particle.IsOptional)
                return Invalid("A required child is missing or the sequence order violates its declaration.");
        }
        if (childIndex != children.Length)
            return Invalid("Unexpected, repeated or out-of-order child elements violate the particles.");
        var states = new List<AssessedElement> { new(RootDeclaration, null, null) };
        for (var index = 0; index < children.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var child = children[index];
            if (child.HasElements) return Invalid("A decimal child cannot contain child elements.");
            if (child.Nodes().Any(node => node is not XText)) return Refuse("Comments and processing instructions inside decimal leaves are outside this projection profile.");
            if (child.Value.Length > 256) return Refuse("A decimal lexical value exceeds the profile bound.");
            var outcome = AssessmentProfileParser.ParseDecimal(child.Value, out var value, out var normalized);
            if (outcome == SchemaAssessmentOutcome.Invalid) return Invalid("A decimal child has an invalid XSD decimal lexical value.");
            if (outcome == SchemaAssessmentOutcome.Unsupported) return Refuse("A valid XSD decimal cannot be represented exactly; no rounding is permitted.");
            states.Add(new(presentDeclarations[index], normalized, XdmValue.FromDecimal(value)));
            child.Value = normalized;
            XdmSchemaAnnotator.Annotate(child, AssessedDocument.DecimalSchemaInfo.Instance);
        }
        // A parentless assertion candidate sees only the assessed element, not document ancestors.
        var candidate = new XElement(root);
        foreach (var child in candidate.Elements())
            XdmSchemaAnnotator.Annotate(child, AssessedDocument.DecimalSchemaInfo.Instance);
        var context = new EvaluationContext().WithFocus(XdmValue.FromNode(new XDocumentNode(candidate)), 1, 1);
        var findings = new List<SchemaDiagnostic>();
        foreach (var assertion in _assertions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (!assertion.Evaluate(context).GetEffectiveBooleanValue())
                    findings.Add(new(SchemaDiagnosticCodes.AssertionViolated, "An assessed-document assertion was false.", null));
            }
            catch (Exception error) when (error is not OperationCanceledException and not OutOfMemoryException)
            { return Refuse("The assertion engine could not evaluate the supported request: " + error.Message); }
        }
        cancellationToken.ThrowIfCancellationRequested();
        if (findings.Count != 0) return new(SchemaAssessmentOutcome.Invalid, findings);
        return new(SchemaAssessmentOutcome.Valid, [], new AssessedDocument(this, snapshot, states));
    }
    private static SchemaDocumentAssessment Invalid(string message) => new(SchemaAssessmentOutcome.Invalid,
        [new SchemaDiagnostic(SchemaDiagnosticCodes.AssessmentInvalidInstance, message, null)]);
    private static SchemaDocumentAssessment Refuse(string message) => new(SchemaAssessmentOutcome.Unsupported,
        [new SchemaDiagnostic(SchemaDiagnosticCodes.AssessmentUnsupported, message, null)]);
}
