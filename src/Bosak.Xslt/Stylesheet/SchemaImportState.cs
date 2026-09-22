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

    /// <summary>All import-schema declarations collected across the import tree, in collection order.</summary>
    public List<Declaration> Declarations { get; } = new();

    /// <summary>The merged, compiled schema set; built once by the root module after all modules are loaded.</summary>
    public XmlSchemaSet? CompiledSchemaSet { get; set; }

    private int _documentOrder;

    /// <summary>Records an import-schema declaration from a module.</summary>
    public void Add(string? ns, IReadOnlyList<string> locations, XElement? inlineSchema, int importPrecedence, string? baseUri)
        => Declarations.Add(new Declaration(ns, locations, inlineSchema, importPrecedence, _documentOrder++, baseUri));
}
