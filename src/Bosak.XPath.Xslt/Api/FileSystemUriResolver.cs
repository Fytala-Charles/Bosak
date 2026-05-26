// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 mei 2026
// PURPOSE              : Default file-system based URI resolver for xsl:import and xsl:include.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 24-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;

namespace Bosak.XPath.Xslt.Api;

/// <summary>
/// Default <see cref="IXsltUriResolver"/> that resolves URIs against the local file system.
/// </summary>
public sealed class FileSystemUriResolver : IXsltUriResolver
{
    /// <summary>
    /// Resolves an href relative to a base URI and loads the file from disk.
    /// </summary>
    public XDocument Resolve(string href, string? baseUri)
    {
        var absoluteUri = ResolveAbsoluteUri(href, baseUri);

        if (!Uri.TryCreate(absoluteUri, UriKind.Absolute, out var uri))
            throw new InvalidOperationException($"Cannot resolve stylesheet URI: {href}");

        if (uri.IsFile)
        {
            var path = uri.LocalPath;
            if (!File.Exists(path))
                throw new FileNotFoundException($"Stylesheet file not found: {path}", path);
            return XDocument.Load(path, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        }

        // For non-file URIs, attempt a web request
        throw new NotSupportedException($"Non-file URI resolution is not yet supported: {absoluteUri}");
    }

    private static string ResolveAbsoluteUri(string href, string? baseUri)
    {
        if (string.IsNullOrEmpty(baseUri))
        {
            // No base URI: href must be absolute or interpreted as a local path
            if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
                return href;
            return Path.GetFullPath(href);
        }

        if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
            return href;

        var baseUriObj = new Uri(baseUri);
        var resolved = new Uri(baseUriObj, href);
        return resolved.AbsoluteUri;
    }
}
