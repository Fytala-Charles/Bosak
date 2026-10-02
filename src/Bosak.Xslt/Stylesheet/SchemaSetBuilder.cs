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
//                      | Charles Korthout | 0.4   | 24-09-2026     | REQ-106 (PA-3): locationful nested xs:import/xs:include targets loaded eagerly        |
//                      |                  |       |                | (Compile fetches nothing with a null resolver) — notation-0301 family                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 30-09-2026     | REQ-113 (PB-2): embedded XML-namespace schema declares xml:lang as the W3C xml.xsd      |
//                      |                  |       |                | union of xs:language and the empty string (attribute-1502)                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 30-09-2026     | REQ-114 (PB-3 C9): XmlResolver on the set before Compile replaces the retired           |
//                      |                  |       |                | LoadNestedSchemaDocuments walk — chameleon includes and redefines now resolve           |
//                      |                  |       |                | (import-schema-189/190); IOException/WebException mapped to XTSE0220                  |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 30-09-2026     | REQ-114/PB-3 C9 fix: AddTolerant treats an empty SourceUri (in-memory schemas) as      |
//                      |                  |       |                | absent — XmlSchemaSet.Add(ns, "") throws ArgumentNullException (SchemaAwareCompilation |
//                      |                  |       |                | host-set tests)                                                                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 30-09-2026     | REQ-114/PB-3 C9 fix: host-set merge skips documents whose SourceUri matches a          |
//                      |                  |       |                | shadowed lower-precedence declaration location (import-schema-177); SelectWinners      |
//                      |                  |       |                | records losing declarations' resolved URIs and now orders winners lowest-first        |
//                      |                  |       |                | (the importing module wins, XSLT 3.0 §3.14.1); import-schema-056 include companions    |
//                      |                  |       |                | survive                                                                                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.9   | 02-10-2026     | validation-0201: Build also emits the imported-only set (xsl:import-schema winners    |
//                      |                  |       |                | without host-merge) for construction/result validation scoping (XSLT 3.0 §11.9)        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.10  | 02-10-2026     | validation-0201 role split: CompilerSchemaSet joins both scopes, EnvironmentSchemaSet |
//                      |                  |       |                | compile-time only; imported-only set re-loads fresh XmlSchema instances per winner       |
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
    /// Builds the compiled schema sets for a schema-aware compilation.
    /// Returns null when there is nothing to compile (no declarations and no
    /// host-supplied set); <paramref name="importedOnlySet"/> receives the set containing
    /// only the stylesheet's own xsl:import-schema declarations (null when there are none).
    /// </summary>
    public static XmlSchemaSet? Build(SchemaImportState state, out XmlSchemaSet? importedOnlySet)
    {
        importedOnlySet = null;
        var winners = SelectWinners(state.Declarations, out var shadowedHostUris);
        if (winners.Count == 0 && state.CompilerSchemaSet is null && state.EnvironmentSchemaSet is null)
            return null;

        var set = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };
        var addedDocuments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // Imported-only set: filled in parallel with the winner loop below. It is the
        // construction-validation component scope (XSLT 3.0 §11.9): host-supplied
        // environment schemas are visible to compilation (type constructors, locationless
        // imports) but never to validation of constructed/result trees.
        XmlSchemaSet? importedOnly = null;
        var importedOnlyDocuments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            // Stylesheet declarations are added first. One winner per namespace by
            // construction (SelectWinners), so no dedup is needed here.
            foreach (var decl in winners)
            {
                var schema = LoadSchema(state, decl);
                if (schema is null)
                    continue;
                if (decl.InlineSchema is not null)
                {
                    // Inline content: add by object. Its SourceUri is the declaring
                    // stylesheet's base URI (used to resolve nested xs:import targets at
                    // Compile), not a schema document — a URI add would re-read the
                    // stylesheet itself as a schema document.
                    set.Add(schema);
                    // The imported-only set gets its own freshly-read XmlSchema instance:
                    // a schema object added to (and compiled by) one XmlSchemaSet must not
                    // be shared with another — the second set's Compile silently loses its
                    // declarations (import-schema-081/185b/186/187/202).
                    importedOnly ??= NewSet();
                    importedOnly.Add(LoadSchema(state, decl)!);
                }
                else
                {
                    AddTolerant(set, schema, addedDocuments);
                    AddTolerant(importedOnly ??= NewSet(), LoadSchema(state, decl)!, importedOnlyDocuments);
                }
            }

            // Host-supplied schemas merge alongside the stylesheet declarations (catalog
            // semantics: environment schemas are in scope in addition to the stylesheet's
            // own imports). Dedup is by document URI only — same-namespace companions from
            // an xs:include pair must both survive (import-schema-056); genuine duplicate
            // definitions surface as XTSE0220 at Compile.
            //
            // REQ-114 (PB-3 C9, import-schema-177): a host-set document whose source URI is
            // the resolved location of a shadowed lower-precedence xsl:import-schema
            // declaration re-introduces declarations the import-precedence rules dropped
            // (XTSE0220 duplicate globals at Compile) — it is skipped. Documents merely
            // included by a shadowed schema are unaffected: their URIs are not a
            // declaration location, and the xs:include chain still resolves at Compile.
            //
            // validation-0201 role split: a "stylesheet-import" host schema (CompilerSchemaSet)
            // is part of the stylesheet's in-scope definitions, so it joins BOTH the merged
            // compile-time set and the imported-only construction-validation set
            // (import-schema-081/185b/186/187/202 resolve xsl:type against it). A
            // "secondary" host schema (EnvironmentSchemaSet) is source-validation context
            // only: merged into the compile-time set (static context keeps pre-split
            // behavior) but never into the construction-validation scope.
            if (state.CompilerSchemaSet is { } hostSet)
            {
                foreach (XmlSchema existing in hostSet.Schemas())
                {
                    if (existing.SourceUri is { Length: > 0 } uri && IsShadowedHostUri(uri, shadowedHostUris))
                        continue;
                    AddTolerant(set, existing, addedDocuments);
                    AddTolerant(importedOnly ??= NewSet(), existing, importedOnlyDocuments);
                }
            }

            if (state.EnvironmentSchemaSet is { } envSet)
            {
                // Same shadowed-URI skip as the host merge above (import-schema-177): a
                // secondary document whose URI is the resolved location of a shadowed
                // xsl:import-schema declaration re-introduces dropped declarations.
                foreach (XmlSchema existing in envSet.Schemas())
                {
                    if (existing.SourceUri is { Length: > 0 } uri && IsShadowedHostUri(uri, shadowedHostUris))
                        continue;
                    AddTolerant(set, existing, addedDocuments);
                }
            }

            // Nested locationful xs:import/xs:include/xs:redefine targets are fetched by
            // XmlSchemaSet.Compile itself: the set's XmlResolver (above) resolves them against
            // each schema's SourceUri — including chameleon includes (no targetNamespace) and
            // redefines, which a null resolver dropped silently (REQ-114, PB-3 C9:
            // import-schema-189/190). An unresolvable location is now a hard XTSE0220 instead
            // of a silent skip (the pre-REQ-106 behavior returned by the resolver).
            AddXmlNamespaceSchema(set);

            set.Compile();

            // The imported-only set exists whenever the stylesheet has any import-schema
            // declarations at all (even a locationless one whose document the host does not
            // supply — attribute-1501/1502/1503 import the XML namespace locationlessly and
            // validate against its synthesized declarations) or a stylesheet-import host set.
            if (winners.Count > 0 || state.CompilerSchemaSet is not null)
            {
                importedOnly ??= NewSet();
                AddXmlNamespaceSchema(importedOnly);
                importedOnly.Compile();
                importedOnlySet = importedOnly;
            }
        }
        catch (XmlSchemaException ex)
        {
            throw new InvalidOperationException($"XTSE0220: invalid schema for xsl:import-schema: {ex.Message}", ex);
        }
        catch (System.IO.IOException ex)
        {
            throw new InvalidOperationException($"XTSE0220: unable to load schema for xsl:import-schema: {ex.Message}", ex);
        }
        catch (System.Net.WebException ex)
        {
            throw new InvalidOperationException($"XTSE0220: unable to load schema for xsl:import-schema: {ex.Message}", ex);
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
        var combined = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };
        foreach (XmlSchema schema in compiled.Schemas())
            AddTolerant(combined, schema, addedDocuments);
        foreach (XmlSchema schema in contextSet.Schemas())
            AddTolerant(combined, schema, addedDocuments);
        AddXmlNamespaceSchema(combined);
        combined.Compile();
        return combined;
    }

    private static XmlSchemaSet NewSet() => new() { XmlResolver = new XmlUrlResolver() };

    /// <summary>
    /// The predefined XML namespace (<c>http://www.w3.org/XML/1998/namespace</c>) is implicitly
    /// available in every schema (XSD 1.0 §4.2.6.2), but <see cref="XmlSchemaSet"/> does not
    /// pre-populate it — schemas that reference <c>xml:lang</c> etc. without an explicit import
    /// fail to compile. The canonical attribute declarations are added here instead.
    /// </summary>
    private const string XmlNamespaceUri = "http://www.w3.org/XML/1998/namespace";

    private const string XmlNamespaceSchema = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='http://www.w3.org/XML/1998/namespace'>
  <!-- REQ-113 (PB-2): the W3C xml.xsd declares xml:lang as a union of xs:language and the
       empty string, so xml:lang='!@$%^*' fails validation (attribute-1502) while the empty
       string stays valid. -->
  <xs:attribute name='lang'>
    <xs:simpleType>
      <xs:union memberTypes='xs:language'>
        <xs:simpleType>
          <xs:restriction base='xs:string'>
            <xs:enumeration value=''/>
          </xs:restriction>
        </xs:simpleType>
      </xs:union>
    </xs:simpleType>
  </xs:attribute>
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
    /// <param name="declarations">All collected declarations across the import tree.</param>
    /// <param name="shadowedHostUris">
    /// Receives the resolved location URIs of declarations that lost import-precedence
    /// selection: host-set schema documents with these source URIs must not re-enter the
    /// merged set (REQ-114, PB-3 C9: import-schema-177).
    /// </param>
    private static List<SchemaImportState.Declaration> SelectWinners(
        List<SchemaImportState.Declaration> declarations,
        out HashSet<string> shadowedHostUris)
    {
        shadowedHostUris = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var winners = new List<SchemaImportState.Declaration>();
        foreach (var group in declarations.GroupBy(d => d.Namespace ?? string.Empty))
        {
            // The declaration of the importing module wins: the principal module has
            // import precedence 0 and every xsl:import level adds one, so the lowest
            // number carries the highest import precedence (XSLT 3.0 §3.14.1;
            // import-schema-177 pins the direction — the principal module's schema
            // supplies the namespace).
            var ordered = group.OrderBy(d => d.ImportPrecedence).ThenBy(d => d.DocumentOrder).ToList();
            var highest = ordered[0];
            // XTSE0215: same namespace, same import precedence, different locations.
            foreach (var other in ordered.Skip(1))
            {
                if (other.ImportPrecedence == highest.ImportPrecedence && !SameLocations(other.Locations, highest.Locations))
                    throw new InvalidOperationException(
                        $"XTSE0215: conflicting xsl:import-schema declarations for namespace '{highest.Namespace}' at the same import precedence");
            }
            winners.Add(highest);
            foreach (var other in ordered.Skip(1))
            {
                // Inline schema content always survives: it cannot be URI-deduped, and
                // declarations from different packages (xsl:use-package) do not compete
                // for a namespace — the merged set is their union (override-f-031 and
                // override-v-005 declare same-namespace union types in used and using
                // packages). Same-namespace documents merge at Compile; genuinely
                // duplicate components surface as XTSE0220 there.
                if (other.InlineSchema is not null)
                {
                    winners.Add(other);
                    continue;
                }
                // Location-based losers are shadowed by the winner: their resolved
                // schema locations must not leak back in via the host-set merge.
                if (other.ImportPrecedence == highest.ImportPrecedence)
                    continue; // same-location duplicate of the winner; URI dedup covers it
                foreach (var location in other.Locations)
                {
                    var absolute = ResolveLocation(location, other.BaseUri);
                    if (absolute is not null)
                        shadowedHostUris.Add(absolute);
                }
            }
        }
        return winners;
    }

    /// <summary>
    /// Whether a host-set document URI matches one of the shadowed declaration locations.
    /// Compares raw strings and normalized absolute-URI forms (the harness and the
    /// declaration resolution may render the same file path differently).
    /// </summary>
    private static bool IsShadowedHostUri(string uri, HashSet<string> shadowedHostUris)
    {
        if (shadowedHostUris.Contains(uri))
            return true;
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
            return false;
        var absolute = parsed.AbsoluteUri;
        if (shadowedHostUris.Contains(absolute))
            return true;
        return shadowedHostUris.Any(shadowed =>
            Uri.TryCreate(shadowed, UriKind.Absolute, out var shadowedParsed) &&
            string.Equals(shadowedParsed.AbsoluteUri, absolute, StringComparison.OrdinalIgnoreCase));
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
    /// namespace-keyed skip would silently drop their declarations. Schemas with a source URI
    /// are re-added <em>by URI</em> rather than by object: a URI-keyed add populates the set's
    /// internal schemaLocations table, so <see cref="XmlSchemaSet.Compile"/>'s own
    /// include/import/redefine resolution reuses the already-added document instead of
    /// fetching a duplicate copy (REQ-114, PB-3 C9: import-schema-188/190). Schemas without a
    /// source URI (inline content) fall back to an object add with a namespace-keyed skip.
    /// </summary>
    /// <returns><c>true</c> when the schema was added to the set; <c>false</c> when it was a duplicate.</returns>
    private static bool AddTolerant(XmlSchemaSet set, XmlSchema schema, HashSet<string> addedDocuments)
    {
        // SourceUri is string.Empty (not null) for schemas compiled from in-memory
        // documents — XmlSchemaSet.Add(ns, "") throws ArgumentNullException — so only
        // a non-empty URI takes the by-URI re-add path.
        if (schema.SourceUri is { Length: > 0 } uri)
        {
            if (!addedDocuments.Add(uri))
                return false;
            set.Add(null, uri);
            return true;
        }

        var ns = schema.TargetNamespace ?? string.Empty;
        if (set.Schemas(ns).Cast<XmlSchema>().Any())
            return false;
        set.Add(schema);
        return true;
    }
}
