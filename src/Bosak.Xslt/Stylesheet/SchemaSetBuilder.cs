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
        if (state.CompilerSchemaSet is { } hostSet)
        {
            foreach (XmlSchema existing in hostSet.Schemas())
                AddTolerant(set, existing);
        }

        foreach (var decl in winners)
        {
            var schema = LoadSchema(state, decl);
            if (schema is not null)
                AddTolerant(set, schema);
        }

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

        var combined = new XmlSchemaSet();
        foreach (XmlSchema schema in compiled.Schemas())
            AddTolerant(combined, schema);
        foreach (XmlSchema schema in contextSet.Schemas())
            AddTolerant(combined, schema);
        combined.Compile();
        return combined;
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
    /// Returns null when the namespace is already satisfied (host set covers it).
    /// </summary>
    private static XmlSchema? LoadSchema(SchemaImportState state, SchemaImportState.Declaration decl)
    {
        var ns = decl.Namespace ?? string.Empty;

        if (decl.InlineSchema is { } inline)
            return ReadSchema(inline.ToString(SaveOptions.DisableFormatting), decl.BaseUri);

        if (state.SchemaResolver is { } resolver)
        {
            using var stream = resolver(ns, decl.Locations);
            if (stream is not null)
                return ReadSchema(stream, baseUri: null);
        }

        foreach (var location in decl.Locations)
        {
            var absolute = ResolveLocation(location, decl.BaseUri);
            if (absolute is null)
                continue;
            try
            {
                using var stream = OpenLocation(absolute);
                return ReadSchema(stream, absolute);
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

        if (state.CompilerSchemaSet is { } hostSet && hostSet.Schemas(ns).Cast<XmlSchema>().Any())
            return null; // namespace already supplied by the host set

        throw new InvalidOperationException(
            $"XTSE0220: no schema document found for namespace '{ns}' (schema-location: {string.Join(" ", decl.Locations)})");
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

    /// <summary>Adds a schema, tolerating duplicate target namespaces (duplicate imports are ignored).</summary>
    private static void AddTolerant(XmlSchemaSet set, XmlSchema schema)
    {
        var ns = schema.TargetNamespace ?? string.Empty;
        if (set.Schemas(ns).Cast<XmlSchema>().Any())
            return;
        set.Add(schema);
    }
}
