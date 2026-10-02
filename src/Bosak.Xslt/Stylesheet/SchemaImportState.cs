// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 september 2026
// PURPOSE              : Shared compilation state for schema-aware XSLT compilation (xsl:import-schema collection across the import tree).
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-09-2026     | Creation (schema-awareness seam hooks H1/H2, REQ-097)                                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 02-10-2026     | ImportedOnlySchemaSet: declaration-only component scope for construction validation     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 02-10-2026     | EnvironmentSchemaSet: host "secondary" schemas (source-validation only, never part of   |
//                      |                  |       |                | the stylesheet's in-scope definitions for construction/result validation)               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using System.Xml.Schema;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Per-compilation state shared by every stylesheet module in an import tree during
/// schema-aware compilation. Created by the root module and threaded through
/// xsl:import / xsl:include / xsl:use-package children so that
/// <c>xsl:import-schema</c> declarations from all modules aggregate into one
/// compiled schema set with import-precedence rules.
/// </summary>
internal sealed class SchemaImportState
{
    /// <summary>One collected <c>xsl:import-schema</c> declaration.</summary>
    internal sealed record Declaration(
        string? Namespace,
        IReadOnlyList<string> Locations,
        XElement? InlineSchema,
        int ImportPrecedence,
        int DocumentOrder,
        string? BaseUri);

    /// <summary>Whether the host opted in to schema-aware compilation (XsltCompiler.SchemaAware).</summary>
    public bool SchemaAware { get; init; }

    /// <summary>Optional host resolver for schema documents: (target namespace, location hints) -> stream.</summary>
    public Func<string, IReadOnlyList<string>, Stream?>? SchemaResolver { get; init; }

    /// <summary>Optional host-supplied pre-built schema set made in-scope for the compilation.</summary>
    public XmlSchemaSet? CompilerSchemaSet { get; init; }

    /// <summary>
    /// Optional host-supplied schema set holding "secondary" environment schemas: visible
    /// to compilation and source-document validation, but never part of the stylesheet's
    /// own in-scope schema definitions — validation of constructed/result trees does not
    /// see these components (XSLT 3.0 §11.9 scoping; validation-0201).
    /// </summary>
    public XmlSchemaSet? EnvironmentSchemaSet { get; init; }

    /// <summary>All import-schema declarations collected across the import tree, in collection order.</summary>
    public List<Declaration> Declarations { get; } = new();

    /// <summary>The merged, compiled schema set; built once by the root module after all modules are loaded.</summary>
    public XmlSchemaSet? CompiledSchemaSet { get; set; }

    /// <summary>
    /// The compiled schema set containing the components in scope for validation of
    /// constructed/result trees (XSLT 3.0 §11.9): the stylesheet's own
    /// <c>xsl:import-schema</c> declarations plus host <c>stylesheet-import</c> schemas,
    /// but never host <c>secondary</c> environment schemas (<see cref="EnvironmentSchemaSet"/>)
    /// — validation-0201: environment schemas must not add default attributes to a
    /// lax-validated result document.
    /// </summary>
    public XmlSchemaSet? ImportedOnlySchemaSet { get; set; }

    private int _documentOrder;

    /// <summary>Records an import-schema declaration from a module.</summary>
    public void Add(string? ns, IReadOnlyList<string> locations, XElement? inlineSchema, int importPrecedence, string? baseUri)
        => Declarations.Add(new Declaration(ns, locations, inlineSchema, importPrecedence, _documentOrder++, baseUri));
}
