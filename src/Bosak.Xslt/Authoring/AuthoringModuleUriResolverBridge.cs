// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Bridges the caller-controlled authoring module resolver into the compiler's URI resolver contract.
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — source-preserving authoring API for visual designers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (extracted from AuthoringInspector for shared inspector/candidate use)          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;

namespace Bosak.Xslt.Authoring;

/// <summary>
/// Bridges a caller-controlled <see cref="IAuthoringModuleResolver"/> into the compiler's
/// <see cref="IXsltUriResolver"/> contract so any derived compilation (inspection-time diagnostics or
/// <see cref="AuthoringEditCandidate.Compile"/>) resolves include/import through the same controlled
/// scope the caller supplied — never implicitly through the file system. Failure behavior mirrors
/// <see cref="FileSystemUriResolver"/> (thrown exceptions, never null), and resolved modules are parsed
/// with the same load options and XML 1.1 idiom. There is no retry and no silent fallback: when the
/// module resolver refuses, returns <see langword="null"/> or fails, resolution fails.
/// </summary>
internal sealed class AuthoringModuleUriResolverBridge : IXsltUriResolver
{
    private readonly IAuthoringModuleResolver _moduleResolver;

    /// <summary>Initializes a new bridge over the given caller-controlled module resolver.</summary>
    /// <param name="moduleResolver">The effective module resolver to route resolution through.</param>
    public AuthoringModuleUriResolverBridge(IAuthoringModuleResolver moduleResolver)
    {
        _moduleResolver = moduleResolver;
    }

    /// <inheritdoc />
    public XDocument Resolve(string href, string? baseUri)
    {
        var absoluteUri = ResolveAbsoluteUri(href, baseUri);
        if (!Uri.TryCreate(absoluteUri, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException($"Cannot resolve stylesheet URI: {href}");
        }

        Uri referencingUri;
        try
        {
            referencingUri = baseUri is not null && Uri.TryCreate(baseUri, UriKind.Absolute, out var baseRef)
                ? baseRef
                : uri;
        }
        catch (UriFormatException)
        {
            referencingUri = uri;
        }

        AuthoringSource? moduleSource;
        try
        {
            moduleSource = _moduleResolver.ResolveModule(uri, referencingUri);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"The module resolver threw while resolving '{uri.AbsoluteUri}': {ex.Message}", ex);
        }

        if (moduleSource is null)
        {
            throw new FileNotFoundException(
                $"Stylesheet module not resolvable through the inspection module resolver: {uri.AbsoluteUri}",
                uri.IsFile ? uri.LocalPath : uri.AbsoluteUri);
        }

        return Xml11Loader.Parse(
            moduleSource.Text,
            LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri,
            moduleSource.BaseUri.AbsoluteUri);
    }

    private static string ResolveAbsoluteUri(string href, string? baseUri)
    {
        if (string.IsNullOrEmpty(baseUri))
        {
            // No base URI: href must be absolute or interpreted as a local path.
            if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
            {
                return href;
            }

            return Path.GetFullPath(href);
        }

        if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
        {
            return href;
        }

        var baseUriObj = new Uri(baseUri);
        var resolved = new Uri(baseUriObj, href);
        return resolved.AbsoluteUri;
    }
}
