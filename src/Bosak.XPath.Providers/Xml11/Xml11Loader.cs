// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 July 2026
// PURPOSE              : Loads XML 1.1 documents into an XDocument by encoding invalid name characters.
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 07-07-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 12-07-2026     | Avoid infinite loop when malformed markup yields an empty attribute name                 |
//                      | Charles Korthout | 0.3   | 12-07-2026     | Annotate loaded elements with the original namespace prefix from the XML source.        |
//                      | Charles Korthout | 0.4   | 01-08-2026     | xml:id attribute values whitespace-collapsed at load (ID attribute normalization)       |
//                      | Charles Korthout | 0.5   | 29-08-2026     | Capture DTD unparsed-entity declarations on loaded documents                            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 29-08-2026     | Preserve document base URI on unparsed-entity annotation for relative system IDs        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 31-08-2026     | Annotate elements declared element-only by a DTD for default XSLT whitespace stripping   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// URI placeholder used for XML 1.1 prefixed namespace undeclarations
/// (<c>xmlns:prefix=""</c>) which .NET's XML 1.0 parser cannot represent.
/// </summary>
internal static class Xml11Undeclaration
{
    public const string Prefix = "urn:bosak-xml11-undecl:";

    public static string PlaceholderUri(string prefix) => Prefix + prefix;

    public static bool TryParsePlaceholderUri(string uri, out string prefix)
    {
        prefix = string.Empty;
        if (uri == null || !uri.StartsWith(Prefix, StringComparison.Ordinal))
            return false;
        prefix = uri.Substring(Prefix.Length);
        return true;
    }
}

/// <summary>
/// Loads XML 1.1 documents into .NET's LINQ-to-XML model by:
/// <list type="bullet">
///   <item>rewriting the XML declaration to version="1.0";</item>
///   <item>encoding XML 1.1-only name characters into XML 1.0-compatible forms;</item>
///   <item>disabling character checking so that C0/C1 control references are accepted.</item>
/// </list>
/// </summary>
public static class Xml11Loader
{
    /// <summary>
    /// Loads an XML document from a file, applying XML 1.1 compatibility if the
    /// declaration declares version 1.1.
    /// </summary>
    public static XDocument Load(string filePath, LoadOptions loadOptions)
    {
        var resolvedPath = filePath;
        string baseUri;
        if (Uri.IsWellFormedUriString(filePath, UriKind.Absolute))
        {
            var uri = new Uri(filePath);
            if (uri.IsFile)
            {
                resolvedPath = uri.LocalPath;
                baseUri = uri.AbsoluteUri;
            }
            else
            {
                baseUri = uri.AbsoluteUri;
            }
        }
        else
        {
            resolvedPath = Path.GetFullPath(filePath);
            baseUri = new Uri(resolvedPath).AbsoluteUri;
        }

        var text = ReadAllTextWithDeclaredEncoding(resolvedPath);
        var (rewritten, isXml11) = PrepareXml11Text(text);
        var settings = CreateSettings();
        using var reader = XmlReader.Create(new StringReader(rewritten), settings, baseUri);
        var doc = XDocument.Load(reader, loadOptions);
        AttachUnparsedEntities(doc, text, baseUri);
        AttachDtdElementInfo(doc, baseUri);
        AnnotateOriginalPrefixes(doc, rewritten, baseUri);
        NormalizeXmlIdValues(doc);
        if (isXml11)
        {
            doc.AddAnnotation(Xml11Annotation.Instance);
            FinalizeXml11Document(doc);
        }
        return doc;
    }

    /// <summary>
    /// Parses an XML string, applying XML 1.1 compatibility if the declaration
    /// declares version 1.1.
    /// </summary>
    public static XDocument Parse(string text, LoadOptions loadOptions, string? baseUri = null)
    {
        var (rewritten, isXml11) = PrepareXml11Text(text);
        var settings = CreateSettings();
        using var reader = XmlReader.Create(new StringReader(rewritten), settings, baseUri ?? "");
        var doc = XDocument.Load(reader, loadOptions);
        AttachUnparsedEntities(doc, text, baseUri ?? "");
        AttachDtdElementInfo(doc, baseUri ?? "");
        AnnotateOriginalPrefixes(doc, rewritten, baseUri ?? "");
        NormalizeXmlIdValues(doc);
        if (isXml11)
        {
            doc.AddAnnotation(Xml11Annotation.Instance);
            FinalizeXml11Document(doc);
        }
        return doc;
    }

    /// <summary>
    /// Parses XML text that is known to be XML 1.1 (for example inline source content
    /// marked with an <c>xml-version="1.1"</c> attribute in the test catalog).
    /// </summary>
    public static XDocument ParseXml11(string text, LoadOptions loadOptions, string? baseUri = null)
    {
        var rewritten = RewriteDeclarationAndEncodeNames(text, forceXml11: true);
        var settings = CreateSettings();
        using var reader = XmlReader.Create(new StringReader(rewritten), settings, baseUri ?? "");
        var doc = XDocument.Load(reader, loadOptions);
        AttachUnparsedEntities(doc, text, baseUri ?? "");
        AttachDtdElementInfo(doc, baseUri ?? "");
        AnnotateOriginalPrefixes(doc, rewritten, baseUri ?? "");
        NormalizeXmlIdValues(doc);
        doc.AddAnnotation(Xml11Annotation.Instance);
        FinalizeXml11Document(doc);
        return doc;
    }

    /// <summary>
    /// Applies attribute-value normalization for ID-typed attributes: the value of an
    /// <c>xml:id</c> attribute is whitespace-collapsed (leading/trailing whitespace
    /// removed, internal runs collapsed to a single space), matching how an ID-typed
    /// attribute is normalized by a validating processor (fn-doc-37, key-076).
    /// </summary>
    private static void NormalizeXmlIdValues(XDocument doc)
    {
        foreach (var el in doc.Descendants())
        {
            foreach (var attr in el.Attributes().ToList())
            {
                if (attr.Name.LocalName == "id" && attr.Name.NamespaceName == "http://www.w3.org/XML/1998/namespace")
                {
                    var normalized = string.Join(' ', attr.Value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
                    if (normalized != attr.Value)
                        attr.Value = normalized;
                }
            }
        }
    }

    /// <summary>
    /// Returns true when the XML text declares itself as XML 1.1.
    /// </summary>
    public static bool HasXml11Declaration(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;
        var trimmed = text.TrimStart();
        if (!trimmed.StartsWith("<?xml", StringComparison.Ordinal))
            return false;
        int end = trimmed.IndexOf("?>", StringComparison.Ordinal);
        if (end < 0)
            return false;
        var decl = trimmed.Substring(0, end);
        return decl.Contains("version=\"1.1\"") || decl.Contains("version='1.1'");
    }

    /// <summary>
    /// Reads a file honoring the encoding declared in its XML declaration.
    /// This is necessary because <see cref="File.ReadAllText(string)"/> always
    /// uses UTF-8 and would corrupt legacy files such as ISO-8859-1 test cases.
    /// </summary>
    private static string ReadAllTextWithDeclaredEncoding(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var bomLength = GetBomLength(bytes);
        var encoding = DetectEncodingFromBom(bytes);

        // Decode enough of the start to read the XML declaration.
        var headerLength = Math.Min(bytes.Length - bomLength, 512);
        var header = encoding.GetString(bytes, bomLength, headerLength);

        var declaredEncoding = ExtractDeclaredEncoding(header);
        if (!string.IsNullOrEmpty(declaredEncoding))
        {
            try
            {
                encoding = Encoding.GetEncoding(declaredEncoding);
            }
            catch
            {
                // Fall back to the BOM/default encoding.
            }
        }

        return encoding.GetString(bytes, bomLength, bytes.Length - bomLength);
    }

    private static int GetBomLength(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return 3;
        if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
            return 4;
        if (bytes.Length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
            return 4;
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return 2;
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return 2;
        return 0;
    }

    /// <summary>
    /// Walks the freshly loaded document in parallel with an <see cref="XmlReader"/> over
    /// the same XML text and records the original namespace prefix used for each element.
    /// </summary>
    private static void AnnotateOriginalPrefixes(XDocument doc, string xmlText, string baseUri)
    {
        var settings = CreateSettings();
        using var reader = XmlReader.Create(new StringReader(xmlText), settings, baseUri);
        AnnotateElementPrefixes(reader, doc);
    }

    private static void AnnotateElementPrefixes(XmlReader reader, XContainer container)
    {
        foreach (var node in container.Nodes())
        {
            if (node is XElement element)
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                        break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    element.AddAnnotation(new OriginalPrefixAnnotation { Prefix = reader.Prefix ?? string.Empty });
                }

                if (!reader.IsEmptyElement)
                {
                    AnnotateElementPrefixes(reader, element);

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.EndElement)
                            break;
                    }
                }
            }
        }
    }

    private static Encoding DetectEncodingFromBom(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return new UTF8Encoding(false, false);
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return new UnicodeEncoding(false, false, false);
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return new UnicodeEncoding(true, false, false);
        if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
            return new UTF32Encoding(false, false, false);
        if (bytes.Length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
            return new UTF32Encoding(true, false, false);
        return new UTF8Encoding(false, false);
    }

    private static readonly Regex EncodingDeclarationRegex = new(
        @"encoding\s*=\s*[""']([^""']+)[""']",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));

    private static string? ExtractDeclaredEncoding(string header)
    {
        var trimmed = header.TrimStart();
        if (!trimmed.StartsWith("<?xml", StringComparison.Ordinal))
            return null;
        int declEnd = trimmed.IndexOf("?>", StringComparison.Ordinal);
        if (declEnd < 0)
            return null;
        var decl = trimmed.Substring(0, declEnd + 2);
        var match = EncodingDeclarationRegex.Match(decl);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static XmlReaderSettings CreateSettings()
    {
        return new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = new XmlUrlResolver(),
            CheckCharacters = false
        };
    }

    /// <summary>
    /// Extracts unparsed entity declarations from the document's DTD using an
    /// <see cref="XmlDocument"/> pass, and attaches them as an annotation to the
    /// loaded <see cref="XDocument"/>. Entities declared in an external subset are
    /// resolved through the <see cref="XmlUrlResolver"/>.
    /// </summary>
    private static void AttachUnparsedEntities(XDocument doc, string xmlText, string baseUri)
    {
        bool isXml11 = HasXml11Declaration(xmlText);
        // For XML 1.1 source we load the name-encoded form: entity declarations live in
        // the DTD and are unaffected by the element-name encoding.
        string textToParse = isXml11 ? RewriteDeclarationAndEncodeNames(xmlText, forceXml11: true) : xmlText;

        var settings = CreateSettings();
        var xmlDoc = new XmlDocument();
        try
        {
            using var reader = XmlReader.Create(new StringReader(textToParse), settings, baseUri);
            xmlDoc.Load(reader);
        }
        catch
        {
            // If the document cannot be loaded for entity extraction (for example because
            // it contains content that is not well-formed even after XML 1.1 rewriting),
            // simply leave the annotation absent. The loaded XDocument itself is authoritative.
            return;
        }

        if (xmlDoc.DocumentType?.Entities is not { } entities || entities.Count == 0)
            return;

        var annotation = new UnparsedEntityAnnotation { BaseUri = baseUri };
        foreach (XmlEntity entity in entities)
        {
            // Only unparsed entities carry a notation name.
            if (string.IsNullOrEmpty(entity.NotationName))
                continue;

            // The first declaration for a given name is binding; later ones are ignored.
            if (annotation.Entities.ContainsKey(entity.Name))
                continue;

            annotation.Entities[entity.Name] = new UnparsedEntityAnnotation.EntityInfo
            {
                SystemId = entity.SystemId ?? string.Empty,
                PublicId = entity.PublicId ?? string.Empty,
                NotationName = entity.NotationName
            };
        }

        if (annotation.Entities.Count > 0)
            doc.AddAnnotation(annotation);
    }

    /// <summary>
    /// Annotates elements declared with element-only content by the document's DTD.
    /// Both the internal subset and the resolved external subset are inspected.
    /// </summary>
    private static void AttachDtdElementInfo(XDocument doc, string baseUri)
    {
        var dtdText = ReadDtdSubset(doc, baseUri);
        if (string.IsNullOrWhiteSpace(dtdText))
            return;

        var elementOnlyNames = ParseDtdElementDeclarations(dtdText);
        if (elementOnlyNames.Count == 0)
            return;

        foreach (var element in doc.Descendants())
        {
            if (elementOnlyNames.Contains(element.Name.LocalName))
                element.AddAnnotation(new DtdElementOnlyAnnotation());
        }
    }

    /// <summary>
    /// Reads the internal DTD subset and resolves the external subset (if any) into a
    /// single string that can be scanned for element declarations.
    /// </summary>
    private static string ReadDtdSubset(XDocument doc, string baseUri)
    {
        var doctype = doc.DocumentType;
        if (doctype == null)
            return string.Empty;

        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(doctype.InternalSubset))
            sb.Append(doctype.InternalSubset);

        if (!string.IsNullOrWhiteSpace(doctype.SystemId))
        {
            try
            {
                var dtdUri = new Uri(new Uri(baseUri), doctype.SystemId);
                if (dtdUri.IsFile && File.Exists(dtdUri.LocalPath))
                {
                    var encoding = Encoding.UTF8;
                    var bytes = File.ReadAllBytes(dtdUri.LocalPath);
                    if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                        encoding = new UTF8Encoding(false);
                    sb.Append(encoding.GetString(bytes));
                }
            }
            catch
            {
                // Ignore unresolvable external subsets.
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parses element declarations from a DTD subset and returns the names of elements
    /// declared with element-only content. A declaration is element-only when it is
    /// <c>EMPTY</c> or when its content model contains no <c>#PCDATA</c> and is not
    /// <c>ANY</c>.
    /// </summary>
    private static HashSet<string> ParseDtdElementDeclarations(string dtdText)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);

        // Remove DTD comments so they do not interfere with parsing.
        var noComments = Regex.Replace(dtdText, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);

        // Find every <!ELEMENT ...> declaration. The content model may span multiple
        // tokens but always terminates with a right angle bracket.
        foreach (Match match in Regex.Matches(noComments, @"<!ELEMENT\s+(\S+)\s+([^>]+)>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
        {
            var name = match.Groups[1].Value;
            var contentModel = match.Groups[2].Value.Trim();

            if (IsDtdElementOnlyContent(contentModel))
                result.Add(name);
        }

        return result;
    }

    /// <summary>
    /// Determines whether a DTD element content model denotes element-only content.
    /// </summary>
    private static bool IsDtdElementOnlyContent(string contentModel)
    {
        var trimmed = contentModel.Trim();
        if (trimmed.Equals("EMPTY", StringComparison.OrdinalIgnoreCase))
            return true;
        if (trimmed.Equals("ANY", StringComparison.OrdinalIgnoreCase))
            return false;

        // A pure element content model is parenthesised and contains no #PCDATA.
        if (trimmed.StartsWith("("))
            return !trimmed.Contains("#PCDATA", StringComparison.OrdinalIgnoreCase);

        return false;
    }

    private static (string Rewritten, bool IsXml11) PrepareXml11Text(string text)
    {
        bool isXml11 = HasXml11Declaration(text);
        return (RewriteDeclarationAndEncodeNames(text, isXml11), isXml11);
    }

    /// <summary>
    /// Rewrites the XML declaration version to 1.0 and encodes XML 1.1-only characters
    /// that appear in element, attribute, and prefix names.
    /// </summary>
    private static string RewriteDeclarationAndEncodeNames(string text, bool forceXml11)
    {
        bool isXml11 = forceXml11 || HasXml11Declaration(text);
        if (!isXml11)
            return text;

        var sb = new StringBuilder(text.Length + 64);
        int i = 0;
        while (i < text.Length)
        {
            char c = text[i];
            if (c == '<')
            {
                int tagStart = i;
                i++;
                if (i < text.Length && (text[i] == '!' || text[i] == '?'))
                {
                    // Comment, CDATA, DOCTYPE, or processing instruction.
                    if (i + 2 < text.Length && text[i] == '!' && text[i + 1] == '-' && text[i + 2] == '-')
                    {
                        // Comment: copy verbatim
                        int end = text.IndexOf("-->", i + 3, StringComparison.Ordinal);
                        if (end < 0)
                        {
                            sb.Append(text, tagStart, text.Length - tagStart);
                            return sb.ToString();
                        }
                        sb.Append(text, tagStart, end + 3 - tagStart);
                        i = end + 3;
                        continue;
                    }
                    if (i + 7 < text.Length && text[i] == '!' && text.Substring(i, 8) == "![CDATA[")
                    {
                        int end = text.IndexOf("]]>", i + 8, StringComparison.Ordinal);
                        if (end < 0)
                        {
                            sb.Append(text, tagStart, text.Length - tagStart);
                            return sb.ToString();
                        }
                        sb.Append(text, tagStart, end + 3 - tagStart);
                        i = end + 3;
                        continue;
                    }
                    if (i + 7 < text.Length && text[i] == '!' && text.Substring(i, 7) == "!DOCTYPE")
                    {
                        // DOCTYPE: copy verbatim (may contain internal subset with < and >)
                        int end = FindDoctypeEnd(text, tagStart);
                        if (end < 0)
                        {
                            sb.Append(text, tagStart, text.Length - tagStart);
                            return sb.ToString();
                        }
                        sb.Append(text, tagStart, end - tagStart);
                        i = end;
                        continue;
                    }
                    if (text[i] == '?')
                    {
                        // Processing instruction; rewrite XML declaration version if present.
                        int end = text.IndexOf("?>", i + 1, StringComparison.Ordinal);
                        if (end < 0)
                        {
                            sb.Append(text, tagStart, text.Length - tagStart);
                            return sb.ToString();
                        }
                        int len = end + 2 - tagStart;
                        var pi = text.Substring(tagStart, len);
                        if (pi.StartsWith("<?xml", StringComparison.Ordinal))
                        {
                            pi = pi.Replace("version=\"1.1\"", "version=\"1.0\"");
                            pi = pi.Replace("version='1.1'", "version='1.0'");
                        }
                        sb.Append(pi);
                        i = end + 2;
                        continue;
                    }

                    // Unknown declaration-like construct: copy rest of tag without encoding.
                    int close = text.IndexOf('>', i);
                    if (close < 0)
                    {
                        sb.Append(text, tagStart, text.Length - tagStart);
                        return sb.ToString();
                    }
                    sb.Append(text, tagStart, close + 1 - tagStart);
                    i = close + 1;
                    continue;
                }

                // End tag
                if (i < text.Length && text[i] == '/')
                {
                    i++;
                    int nameStart = i;
                    while (i < text.Length && !IsNameTerminator(text[i])) i++;
                    string name = text.Substring(nameStart, i - nameStart);
                    sb.Append("</");
                    sb.Append(Xml11NameCodec.EncodeName(name));
                    // copy remainder of tag
                    while (i < text.Length && text[i] != '>') { sb.Append(text[i]); i++; }
                    if (i < text.Length) { sb.Append('>'); i++; }
                    continue;
                }

                // Start tag
                int startNameStart = i;
                while (i < text.Length && !IsNameTerminator(text[i])) i++;
                string elemName = text.Substring(startNameStart, i - startNameStart);
                sb.Append('<');
                sb.Append(Xml11NameCodec.EncodeName(elemName));

                // Attributes
                while (i < text.Length && text[i] != '>' && !(text[i] == '/' && i + 1 < text.Length && text[i + 1] == '>'))
                {
                    if (char.IsWhiteSpace(text[i]))
                    {
                        sb.Append(text[i]);
                        i++;
                        continue;
                    }

                    // Attribute name
                    int attrNameStart = i;
                    while (i < text.Length && !IsNameTerminator(text[i]) && text[i] != '=' && !char.IsWhiteSpace(text[i]))
                        i++;
                    string attrName = text.Substring(attrNameStart, i - attrNameStart);
                    if (attrName.Length == 0)
                    {
                        // Malformed markup (for example a backslash before the end-tag slash
                        // in a JSON string literal). Stop parsing attributes so the tag-close
                        // loop can copy the remaining characters and the parser can reject the
                        // document normally instead of looping forever.
                        break;
                    }
                    var decodedAttrName = Xml11NameCodec.DecodeName(attrName);
                    sb.Append(Xml11NameCodec.EncodeName(attrName));

                    // Skip whitespace before optional '='
                    while (i < text.Length && char.IsWhiteSpace(text[i])) { sb.Append(text[i]); i++; }

                    if (i < text.Length && text[i] == '=')
                    {
                        sb.Append('=');
                        i++;
                        while (i < text.Length && char.IsWhiteSpace(text[i])) { sb.Append(text[i]); i++; }
                        if (i < text.Length && (text[i] == '\'' || text[i] == '\"'))
                        {
                            char quote = text[i];
                            sb.Append(quote);
                            i++;
                            int valStart = i;
                            while (i < text.Length && text[i] != quote) i++;
                            var attrValue = text.Substring(valStart, i - valStart);
                            if (decodedAttrName.StartsWith("xmlns:", StringComparison.Ordinal) && attrValue.Length == 0)
                            {
                                // XML 1.1 prefixed namespace undeclaration: store a placeholder
                                // so the XML 1.0 parser accepts the document. The placeholder is
                                // replaced with an annotation after parsing.
                                var prefix = decodedAttrName.Substring(6);
                                sb.Append(Xml11Undeclaration.PlaceholderUri(prefix));
                            }
                            else
                            {
                                EncodeInvalidReferencesInRange(text, valStart, i - valStart, sb);
                            }
                            if (i < text.Length)
                            {
                                sb.Append(quote);
                                i++;
                            }
                        }
                    }
                }

                // copy tag close
                while (i < text.Length && text[i] != '>') { sb.Append(text[i]); i++; }
                if (i < text.Length) { sb.Append('>'); i++; }
            }
            else
            {
                // Text content: copy through to the next '<', encoding XML 1.1 character
                // references that are not valid in XML 1.0 (for example C0/C1 controls).
                int textStart = i;
                while (i < text.Length && text[i] != '<')
                {
                    if (text[i] == '&' && TryParseCharacterReference(text, i, out var cp, out var refEnd))
                    {
                        if (Xml11NameCodec.IsInvalidXml10Char(cp))
                        {
                            sb.Append(text, textStart, i - textStart);
                            sb.Append(EncodeInvalidCharacterReference(cp));
                            textStart = refEnd;
                        }
                        i = refEnd;
                    }
                    else
                    {
                        i++;
                    }
                }
                sb.Append(text, textStart, i - textStart);
            }
        }

        return sb.ToString();
    }

    private static bool IsNameTerminator(char c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '>' || c == '/' || c == '=';
    }

    private static void FinalizeXml11Document(XDocument document)
    {
        RestoreNamespaceUndeclarations(document);
        DecodeTextValues(document);
    }

    /// <summary>
    /// Restores sentinel-encoded XML 1.1 control characters in text, attribute,
    /// comment, and processing-instruction values after parsing.
    /// </summary>
    private static void DecodeTextValues(XDocument document)
    {
        foreach (var element in document.Descendants().ToList())
        {
            foreach (var attr in element.Attributes().ToList())
            {
                if (!attr.IsNamespaceDeclaration)
                    attr.Value = Xml11NameCodec.DecodeValue(attr.Value);
            }

            foreach (var node in element.Nodes().ToList())
            {
                switch (node)
                {
                    case XText text:
                        text.Value = Xml11NameCodec.DecodeValue(text.Value);
                        break;
                    case XComment comment:
                        comment.Value = Xml11NameCodec.DecodeValue(comment.Value);
                        break;
                    case XProcessingInstruction pi:
                        pi.Data = Xml11NameCodec.DecodeValue(pi.Data);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Replaces placeholder namespace-declaration values introduced for XML 1.1
    /// prefixed undeclarations with <see cref="PrefixedNamespaceUndeclarations"/>
    /// annotations on the owning element.
    /// </summary>
    private static void EncodeInvalidReferencesInRange(string text, int start, int length, StringBuilder sb)
    {
        int i = start;
        int end = start + length;
        int literalStart = start;
        while (i < end)
        {
            if (text[i] == '&' && TryParseCharacterReference(text, i, out var cp, out var refEnd))
            {
                if (Xml11NameCodec.IsInvalidXml10Char(cp))
                {
                    sb.Append(text, literalStart, i - literalStart);
                    sb.Append(EncodeInvalidCharacterReference(cp));
                    literalStart = refEnd;
                }
                i = refEnd;
            }
            else
            {
                i++;
            }
        }
        sb.Append(text, literalStart, end - literalStart);
    }

    private static bool TryParseCharacterReference(string text, int start, out int codepoint, out int endIndex)
    {
        codepoint = 0;
        endIndex = start;
        if (start + 1 >= text.Length || text[start] != '&' || text[start + 1] != '#')
            return false;

        int i = start + 2;
        bool hex = false;
        if (i < text.Length && (text[i] == 'x' || text[i] == 'X'))
        {
            hex = true;
            i++;
        }

        int valueStart = i;
        while (i < text.Length && text[i] != ';')
            i++;

        if (i >= text.Length || valueStart == i)
            return false;

        var number = text.Substring(valueStart, i - valueStart);
        var style = hex ? System.Globalization.NumberStyles.HexNumber : System.Globalization.NumberStyles.Integer;
        if (!int.TryParse(number, style, System.Globalization.CultureInfo.InvariantCulture, out codepoint))
            return false;

        endIndex = i + 1;
        return true;
    }

    private static string EncodeInvalidCharacterReference(int codepoint)
        => Xml11NameCodec.EncodeValue(char.ConvertFromUtf32(codepoint));

    private static void RestoreNamespaceUndeclarations(XDocument document)
    {
        foreach (var element in document.Descendants().ToList())
        {
            var undeclAttrs = element.Attributes()
                .Where(a => a.IsNamespaceDeclaration &&
                            Xml11Undeclaration.TryParsePlaceholderUri(a.Value, out _))
                .ToList();

            if (undeclAttrs.Count == 0)
                continue;

            var annotation = element.Annotation<PrefixedNamespaceUndeclarations>();
            if (annotation == null)
            {
                annotation = new PrefixedNamespaceUndeclarations();
                element.AddAnnotation(annotation);
            }

            foreach (var attr in undeclAttrs)
            {
                if (Xml11Undeclaration.TryParsePlaceholderUri(attr.Value, out var prefix))
                {
                    annotation.Prefixes.Add(prefix);
                }
                attr.Remove();
            }
        }
    }

    private static int FindDoctypeEnd(string text, int start)
    {
        int i = start;
        bool inString = false;
        char quote = '\0';
        int bracketDepth = 0;
        while (i < text.Length)
        {
            char c = text[i];
            if (inString)
            {
                if (c == quote)
                    inString = false;
            }
            else if (c == '\'' || c == '\"')
            {
                inString = true;
                quote = c;
            }
            else if (c == '[')
            {
                bracketDepth++;
            }
            else if (c == ']')
            {
                bracketDepth--;
            }
            else if (c == '>' && bracketDepth == 0)
            {
                return i + 1;
            }
            i++;
        }
        return -1;
    }
}
