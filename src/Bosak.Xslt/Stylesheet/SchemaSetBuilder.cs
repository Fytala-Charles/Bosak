// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 september 2026
// PURPOSE              : Builds the merged compiled XmlSchemaSet for schema-aware XSLT compilation from collected xsl:import-schema declarations.
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
//                      | Charles Korthout | 0.2   | 23-09-2026     | REQ-102 (PA-1): host set now lowest precedence (added after stylesheet winners);        |
//                      |                  |       |                | document-URI dedup instead of namespace-skip (xs:include merge case); namespace-only    |
//                      |                  |       |                | imports are inert until used (XTSE0220 only for locationful failures); schema target    |
//                      |                  |       |                | namespace must match the declaration; predefined XML namespace schema added             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 24-09-2026     | REQ-104 (PA-2): locationless import of the XPath functions namespace binds the          |
//                      |                  |       |                | embedded W3C schema-for-JSON (json-to-xml-typed family)                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Merges the <c>xsl:import-schema</c> declarations collected across a stylesheet
/// import tree (plus any host-supplied pre-built schema set) into one compiled
/// <see cref="XmlSchemaSet"/>, applying XSLT 3.0 import-precedence rules:
/// the declaration at the highest import precedence wins per namespace; two
/// declarations for the same namespace at the same precedence with different
/// schema locations are a static error (XTSE0215); unlocatable or invalid schema
/// documents raise XTSE0220.
/// </summary>
internal static class SchemaSetBuilder
{
    /// <summary>
    /// Builds the compiled schema set for a schema-aware compilation.
    /// Returns null when there is nothing to compile (no declarations and no
    /// host-supplied set).
    /// </summary>
    public static XmlSchemaSet? Build(SchemaImportState state)
    {
        var winners = SelectWinners(state.Declarations);
        if (winners.Count == 0 && state.CompilerSchemaSet is null)
            return null;

        var set = new XmlSchemaSet();
        var addedDocuments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Stylesheet declarations are added first. One winner per namespace by construction
        // (SelectWinners), so no dedup is needed here.
        foreach (var decl in winners)
        {
            var schema = LoadSchema(state, decl);
            if (schema is not null)
            {
                set.Add(schema);
                if (schema.SourceUri is { } uri)
                    addedDocuments.Add(uri);
            }
        }

        // Host-supplied schemas merge alongside the stylesheet declarations (catalog
        // semantics: environment schemas are in scope in addition to the stylesheet's own
        // imports). Dedup is by document URI only — same-namespace companions from an
        // xs:include pair must both survive (import-schema-056); genuine duplicate
        // definitions surface as XTSE0220 at Compile.
        if (state.CompilerSchemaSet is { } hostSet)
        {
            foreach (XmlSchema existing in hostSet.Schemas())
                AddTolerant(set, existing, addedDocuments);
        }

        AddXmlNamespaceSchema(set);

        try
        {
            set.Compile();
        }
        catch (XmlSchemaException ex)
        {
            throw new InvalidOperationException($"XTSE0220: invalid schema for xsl:import-schema: {ex.Message}", ex);
        }
        return set;
    }

    /// <summary>
    /// Merges a stylesheet-compiled schema set into an evaluation context's schema set.
    /// When the context has no set, the compiled set is adopted directly; otherwise a
    /// combined set is built (compiled schemas first, then the context's own).
    /// </summary>
    public static XmlSchemaSet MergeIntoContext(XmlSchemaSet compiled, XmlSchemaSet? contextSet)
    {
        if (contextSet is null || !contextSet.Schemas().Cast<XmlSchema>().Any())
            return compiled;

        var addedDocuments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var combined = new XmlSchemaSet();
        foreach (XmlSchema schema in compiled.Schemas())
            AddTolerant(combined, schema, addedDocuments);
        foreach (XmlSchema schema in contextSet.Schemas())
            AddTolerant(combined, schema, addedDocuments);
        AddXmlNamespaceSchema(combined);
        combined.Compile();
        return combined;
    }

    /// <summary>
    /// The predefined XML namespace (<c>http://www.w3.org/XML/1998/namespace</c>) is implicitly
    /// available in every schema (XSD 1.0 §4.2.6.2), but <see cref="XmlSchemaSet"/> does not
    /// pre-populate it — schemas that reference <c>xml:lang</c> etc. without an explicit import
    /// fail to compile. The canonical attribute declarations are added here instead.
    /// </summary>
    private const string XmlNamespaceUri = "http://www.w3.org/XML/1998/namespace";

    private const string XmlNamespaceSchema = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='http://www.w3.org/XML/1998/namespace'>
  <xs:attribute name='lang' type='xs:string'/>
  <xs:attribute name='space'>
    <xs:simpleType>
      <xs:restriction base='xs:NCName'>
        <xs:enumeration value='default'/>
        <xs:enumeration value='preserve'/>
      </xs:restriction>
    </xs:simpleType>
  </xs:attribute>
  <xs:attribute name='base' type='xs:anyURI'/>
  <xs:attribute name='id' type='xs:ID'/>
</xs:schema>";

    private static void AddXmlNamespaceSchema(XmlSchemaSet set)
    {
        if (set.Schemas(XmlNamespaceUri).Cast<XmlSchema>().Any())
            return;
        set.Add(ReadSchema(XmlNamespaceSchema, baseUri: null));
    }

    /// <summary>
    /// Applies import-precedence selection per namespace and detects same-precedence
    /// conflicts (XTSE0215). A null namespace groups under the empty string.
    /// </summary>
    private static List<SchemaImportState.Declaration> SelectWinners(List<SchemaImportState.Declaration> declarations)
    {
        var winners = new List<SchemaImportState.Declaration>();
        foreach (var group in declarations.GroupBy(d => d.Namespace ?? string.Empty))
        {
            var ordered = group.OrderByDescending(d => d.ImportPrecedence).ThenBy(d => d.DocumentOrder).ToList();
            var highest = ordered[0];
            // XTSE0215: same namespace, same import precedence, different locations.
            foreach (var other in ordered.Skip(1))
            {
                if (other.ImportPrecedence == highest.ImportPrecedence && !SameLocations(other.Locations, highest.Locations))
                    throw new InvalidOperationException(
                        $"XTSE0215: conflicting xsl:import-schema declarations for namespace '{highest.Namespace}' at the same import precedence");
            }
            winners.Add(highest);
        }
        return winners;
    }

    private static bool SameLocations(IReadOnlyList<string> a, IReadOnlyList<string> b)
        => a.SequenceEqual(b);

    /// <summary>
    /// Loads one schema document: inline schema content, the host resolver, then
    /// schema-location URIs resolved against the declaring module's base URI.
    /// Returns null when the declaration is inert: the namespace is already covered by the
    /// host set, or the declaration is locationless and unresolvable (XSLT 3.0 §3.14.1 — a
    /// namespace-only import raises no error unless components from the namespace are used).
    /// </summary>
    private static XmlSchema? LoadSchema(SchemaImportState state, SchemaImportState.Declaration decl)
    {
        var ns = decl.Namespace ?? string.Empty;

        XmlSchema? schema = null;
        var sawMismatch = false;

        if (decl.InlineSchema is { } inline)
        {
            schema = ReadSchema(inline.ToString(SaveOptions.DisableFormatting), decl.BaseUri);
            // xsl:import-schema without @namespace takes the inline schema's target namespace
            // (the spec example, import-schema-179); with @namespace they must match, and an
            // inline schema has no fallback — XTSE0215 (import-schema-154).
            if (ns.Length > 0 && (schema.TargetNamespace ?? string.Empty) != ns)
                throw new InvalidOperationException(
                    $"XTSE0215: inline schema target namespace '{schema.TargetNamespace ?? ""}' conflicts with the xsl:import-schema namespace '{ns}'");
            return schema;
        }

        if (state.SchemaResolver is { } resolver)
        {
            using var stream = resolver(ns, decl.Locations);
            if (stream is not null)
            {
                var candidate = ReadSchema(stream, baseUri: null);
                // The resolver was asked for namespace ns; a document with a different target
                // namespace is not a schema for that namespace (import-schema-201).
                if ((candidate.TargetNamespace ?? string.Empty) == ns)
                    schema = candidate;
                else
                    sawMismatch = true;
            }
        }

        if (schema is null)
        {
            foreach (var location in decl.Locations)
            {
                var absolute = ResolveLocation(location, decl.BaseUri);
                if (absolute is null)
                    continue;
                try
                {
                    using var stream = OpenLocation(absolute);
                    var candidate = ReadSchema(stream, absolute);
                    // A schema-location is a hint: a document whose target namespace does not
                    // match the declared namespace simply yields nothing (import-schema-186).
                    if ((candidate.TargetNamespace ?? string.Empty) == ns)
                    {
                        schema = candidate;
                        break;
                    }
                    sawMismatch = true;
                }
                catch (System.IO.IOException)
                {
                    // Try the next location hint.
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    // Try the next location hint.
                }
            }
        }

        if (schema is not null)
            return schema;

        if (state.CompilerSchemaSet is { } hostSet && hostSet.Schemas(ns).Cast<XmlSchema>().Any())
            return null; // namespace already supplied by the host set

        if (decl.Locations.Count == 0 && ns == "http://www.w3.org/2005/xpath-functions")
        {
            // The W3C schema-for-JSON ships embedded in the engine (fn:json-to-xml
            // validation): a locationless import of the XPath functions namespace binds
            // it (json-to-xml-typed family). Read a fresh copy so the stylesheet's set
            // owns and compiles its own XmlSchema instance.
            using var jsonStream = Bosak.XPath.Standard.Functions.FunctionLibrary.GetJsonSchemaStream();
            return XmlSchema.Read(jsonStream, null);
        }

        if (decl.Locations.Count == 0 && !sawMismatch)
            return null; // locationless import: inert unless a component from the namespace is used

        throw new InvalidOperationException(sawMismatch
            ? $"XTSE0220: no schema document for namespace '{ns}' — resolved document(s) carry a different target namespace (schema-location: {string.Join(" ", decl.Locations)})"
            : $"XTSE0220: no schema document found for namespace '{ns}' (schema-location: {string.Join(" ", decl.Locations)})");
    }

    private static string? ResolveLocation(string location, string? baseUri)
    {
        if (Uri.TryCreate(location, UriKind.Absolute, out var absolute))
            return absolute.ToString();
        if (!string.IsNullOrEmpty(baseUri) && Uri.TryCreate(new Uri(baseUri), location, out var resolved))
            return resolved.ToString();
        if (System.IO.File.Exists(location))
            return Path.GetFullPath(location);
        return null;
    }

    private static Stream OpenLocation(string absolute)
        => absolute.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
           absolute.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? OpenHttp(absolute)
            : System.IO.File.OpenRead(Uri.UnescapeDataString(new Uri(absolute).LocalPath));

    private static Stream OpenHttp(string absolute)
    {
        using var http = new System.Net.Http.HttpClient();
        var bytes = http.GetByteArrayAsync(absolute).GetAwaiter().GetResult();
        return new MemoryStream(bytes, writable: false);
    }

    private static XmlSchema ReadSchema(string xml, string? baseUri)
    {
        using var stringReader = new StringReader(xml);
        using var reader = XmlReader.Create(stringReader, ReaderSettings(), baseUri is null ? null : new XmlParserContext(null, null, null, XmlSpace.None) { BaseURI = baseUri });
        return ReadSchema(reader);
    }

    private static XmlSchema ReadSchema(Stream stream, string? baseUri)
    {
        using var reader = XmlReader.Create(stream, ReaderSettings(), baseUri is null ? null : new XmlParserContext(null, null, null, XmlSpace.None) { BaseURI = baseUri });
        return ReadSchema(reader);
    }

    private static XmlSchema ReadSchema(XmlReader reader)
    {
        try
        {
            return XmlSchema.Read(reader, validationEventHandler: null)
                ?? throw new InvalidOperationException("XTSE0220: empty schema document");
        }
        catch (XmlSchemaException ex)
        {
            throw new InvalidOperationException($"XTSE0220: invalid schema document: {ex.Message}", ex);
        }
        catch (XmlException ex)
        {
            throw new InvalidOperationException($"XTSE0220: invalid schema document: {ex.Message}", ex);
        }
    }

    private static XmlReaderSettings ReaderSettings() => new()
    {
        DtdProcessing = DtdProcessing.Ignore,
        XmlResolver = new XmlUrlResolver(),
    };

    /// <summary>
    /// Adds a schema, tolerating documents added twice (e.g. a schema that is both included by
    /// another and listed separately). Dedup is keyed on the document's source URI: multiple
    /// schema documents for the same target namespace are legitimate (xs:include merge), so a
    /// namespace-keyed skip would silently drop their declarations. Schemas without a source
    /// URI (inline content) fall back to a namespace-keyed skip.
    /// </summary>
    private static void AddTolerant(XmlSchemaSet set, XmlSchema schema, HashSet<string> addedDocuments)
    {
        if (schema.SourceUri is { } uri)
        {
            if (!addedDocuments.Add(uri))
                return;
            set.Add(schema);
            return;
        }

        var ns = schema.TargetNamespace ?? string.Empty;
        if (set.Schemas(ns).Cast<XmlSchema>().Any())
            return;
        set.Add(schema);
    }
}
