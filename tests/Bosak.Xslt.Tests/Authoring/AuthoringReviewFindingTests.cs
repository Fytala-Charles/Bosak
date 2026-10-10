// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Regression tests for REQ-124 Slice A consumer review findings 3 (byte aliasing) and 4 (compile-time resolver bypass).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
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

using System.Text;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

public class AuthoringReviewFindingTests
{
    private static readonly Uri BaseUri = new("file:///C:/project/main.xsl");
    private static readonly UTF8Encoding Utf8 = new(false, true);

    private const string PrincipalWithInclude = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:include href="included.xsl"/>
          <xsl:template match="/">
            <out><xsl:call-template name="lib"/></out>
          </xsl:template>
        </xsl:stylesheet>
        """;

    private const string IncludedModule = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template name="lib"><lib/></xsl:template>
        </xsl:stylesheet>
        """;

    private sealed class MemoryModuleResolver : IAuthoringModuleResolver
    {
        private readonly Dictionary<Uri, AuthoringSource> _modules = new();

        public void Add(Uri absoluteUri, byte[] bytes)
        {
            Assert.True(
                AuthoringSource.TryCreate(bytes, absoluteUri, out var source, out var failure),
                failure?.ToString());
            _modules[absoluteUri] = source!;
        }

        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) =>
            _modules.TryGetValue(absoluteUri, out var source) ? source : null;
    }

    private sealed class DenyAllModuleResolver : IAuthoringModuleResolver
    {
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) => null;
    }

    // ---------------------------------------------------------------------------------------------
    // Finding 3: caller-owned bytes must not alias the retained source envelope.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Finding3_PrincipalBytes_MutationAfterInspect_DoesNotChangeExportOriginal()
    {
        var bytes = Utf8.GetBytes(PrincipalWithInclude);
        var pristine = bytes.ToArray();
        Assert.True(AuthoringSource.TryCreate(bytes, BaseUri, out var source, out var failure), failure?.ToString());
        var result = XsltAuthoring.Inspect(source!);
        Assert.True(result.IsSuccess, result.Failure?.ToString());
        var snapshot = result.Snapshot!;

        // The defect: mutating the caller's array after inspection changed ExportOriginal output.
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (byte)~bytes[i];
        }

        var first = snapshot.ExportOriginal(BaseUri);
        var second = snapshot.ExportOriginal(BaseUri);
        Assert.True(pristine.SequenceEqual(first), "ExportOriginal must return the pre-mutation bytes.");
        Assert.True(pristine.SequenceEqual(second), "ExportOriginal must stay stable across repeated calls.");
    }

    [Fact]
    public void Finding3_PrincipalBytes_MutationBeforeInspect_IsRetainedAsGiven()
    {
        var bytes = Utf8.GetBytes(PrincipalWithInclude.Replace("included.xsl", "other.xsl", StringComparison.Ordinal));
        var mutated = bytes.ToArray();

        Assert.True(AuthoringSource.TryCreate(bytes, BaseUri, out var source, out var failure), failure?.ToString());
        var result = XsltAuthoring.Inspect(source!);
        Assert.True(result.IsSuccess, result.Failure?.ToString());

        Assert.True(mutated.SequenceEqual(result.Snapshot!.ExportOriginal(BaseUri)));
    }

    [Fact]
    public void Finding3_ModuleResolverBytes_MutationAfterInspect_DoesNotChangeModuleExport()
    {
        var principalBytes = Utf8.GetBytes(PrincipalWithInclude);
        var moduleUri = new Uri("file:///C:/project/included.xsl");
        var moduleBytes = Utf8.GetBytes(IncludedModule);
        var pristineModule = moduleBytes.ToArray();

        var resolver = new MemoryModuleResolver();
        resolver.Add(moduleUri, moduleBytes);

        Assert.True(AuthoringSource.TryCreate(principalBytes, BaseUri, out var source, out var failure), failure?.ToString());
        var result = XsltAuthoring.Inspect(source!, resolver);
        Assert.True(result.IsSuccess, result.Failure?.ToString());
        Assert.True(result.Snapshot!.TryGetModule(moduleUri, out _), "The included module must be part of the snapshot.");

        // A consumer mutating the array it handed to the resolver must not affect the envelope.
        for (var i = 0; i < moduleBytes.Length; i++)
        {
            moduleBytes[i] = (byte)~moduleBytes[i];
        }

        var exported = result.Snapshot.ExportOriginal(moduleUri);
        Assert.True(pristineModule.SequenceEqual(exported), "Module ExportOriginal must return the pre-mutation bytes.");
    }

    // ---------------------------------------------------------------------------------------------
    // Finding 4: derived compilation must resolve through the inspection resolver, never implicitly.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Finding4_DenyAllResolver_DoesNotReadExistingFileAtCompileTime()
    {
        var dir = Path.Combine(Path.GetTempPath(), "bosak-req124-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            // The include target EXISTS on disk and is valid: if compilation fell back to the file
            // system (the defect), it would compile successfully and IsCompilable would be true.
            File.WriteAllText(Path.Combine(dir, "included.xsl"), IncludedModule, Utf8);
            var mainUri = new Uri(Path.Combine(dir, "main.xsl")).AbsoluteUri;
            Assert.True(
                AuthoringSource.TryCreate(Utf8.GetBytes(PrincipalWithInclude), new Uri(mainUri), out var source, out var failure),
                failure?.ToString());

            var options = new AuthoringInspectionOptions { AttemptCompilation = true };
            var result = XsltAuthoring.Inspect(source!, new DenyAllModuleResolver(), options);

            Assert.True(result.IsSuccess, result.Failure?.ToString());
            var snapshot = result.Snapshot!;
            Assert.False(snapshot.IsCompilable);
            var diagnostic = Assert.Single(snapshot.CompilationDiagnostics);
            Assert.Contains("resolv", diagnostic, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("included.xsl", diagnostic, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Finding4_MemoryResolver_BridgeResolvesInclude_EndToEnd()
    {
        var moduleUri = new Uri("file:///C:/project/included.xsl");
        var resolver = new MemoryModuleResolver();
        resolver.Add(moduleUri, Utf8.GetBytes(IncludedModule));

        Assert.True(
            AuthoringSource.TryCreate(Utf8.GetBytes(PrincipalWithInclude), BaseUri, out var source, out var failure),
            failure?.ToString());
        var options = new AuthoringInspectionOptions { AttemptCompilation = true };
        var result = XsltAuthoring.Inspect(source!, resolver, options);

        Assert.True(result.IsSuccess, result.Failure?.ToString());
        Assert.True(result.Snapshot!.IsCompilable, string.Join(" | ", result.Snapshot.CompilationDiagnostics));
    }

    [Fact]
    public void Finding4_DefaultFileSystemResolver_BehaviorIsUnchanged()
    {
        var dir = Path.Combine(Path.GetTempPath(), "bosak-req124-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "included.xsl"), IncludedModule, Utf8);
            var mainPath = Path.Combine(dir, "main.xsl");
            File.WriteAllText(mainPath, PrincipalWithInclude, Utf8);
            var mainUri = new Uri(mainPath).AbsoluteUri;

            Assert.True(
                AuthoringSource.TryCreate(File.ReadAllBytes(mainPath), new Uri(mainUri), out var source, out var failure),
                failure?.ToString());
            var options = new AuthoringInspectionOptions { AttemptCompilation = true };
            var result = XsltAuthoring.Inspect(source!, new FileSystemAuthoringModuleResolver(), options);

            Assert.True(result.IsSuccess, result.Failure?.ToString());
            Assert.True(result.Snapshot!.IsCompilable, string.Join(" | ", result.Snapshot.CompilationDiagnostics));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
