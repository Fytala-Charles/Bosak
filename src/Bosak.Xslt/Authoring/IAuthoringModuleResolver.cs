// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Caller-controlled module resolution plus the default file-system resolver.
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — source-preserving authoring API for visual designers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Authoring;

/// <summary>
/// Caller-controlled resolution of <c>xsl:include</c>/<c>xsl:import</c> module references to retained
/// source envelopes. Inspection only ever fetches modules through this interface — it performs no
/// implicit network or disk access beyond what the supplied resolver chooses to do. Returning
/// <see langword="null"/> records an unresolved edge with a diagnostic; the referencing module stays
/// inspectable.
/// </summary>
public interface IAuthoringModuleResolver
{
    /// <summary>
    /// Resolves an include/import reference.
    /// </summary>
    /// <param name="absoluteUri">The absolute URI the href resolved to (after the referencing module's base URI and xml:base chain).</param>
    /// <param name="referencingModuleUri">The absolute URI of the module containing the include/import element.</param>
    /// <returns>The retained source for the module, or <see langword="null"/> when it cannot be supplied.</returns>
    AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri);
}

/// <summary>
/// The default <see cref="IAuthoringModuleResolver"/> that reads module bytes from the local file system
/// using <see cref="Uri.LocalPath"/> of the resolved absolute URI.
/// </summary>
public sealed class FileSystemAuthoringModuleResolver : IAuthoringModuleResolver
{
    /// <inheritdoc />
    public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
    {
        if (!absoluteUri.IsAbsoluteUri || !absoluteUri.IsFile)
        {
            return null;
        }

        try
        {
            var path = absoluteUri.LocalPath;
            if (!File.Exists(path))
            {
                return null;
            }

            var bytes = File.ReadAllBytes(path);
            return AuthoringSource.TryCreate(bytes, absoluteUri, out var source, out _)
                ? source
                : null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }
}
