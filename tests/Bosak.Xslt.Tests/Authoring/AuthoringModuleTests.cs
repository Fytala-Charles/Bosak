// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Tests for include/import module walking, resolution, missing modules and cycles (AC-08).
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

public class AuthoringModuleTests
{
    private static readonly Uri MainUri = new("file:///mods/main.xsl");

    private sealed class MemoryResolver : IAuthoringModuleResolver
    {
        private readonly Dictionary<string, byte[]> _modules = new(StringComparer.OrdinalIgnoreCase);

        public List<(Uri AbsoluteUri, Uri Referencing)> Requests { get; } = new();

        public void Add(string absoluteUri, string text)
            => _modules[absoluteUri] = new UTF8Encoding(false, true).GetBytes(text);

        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Requests.Add((absoluteUri, referencingModuleUri));
            return _modules.TryGetValue(absoluteUri.AbsoluteUri, out var bytes) &&
                   AuthoringSource.TryCreate(bytes, absoluteUri, out var source, out _)
                ? source
                : null;
        }
    }

    private static AuthoringInspectionResult Inspect(string principal, IAuthoringModuleResolver resolver)
    {
        var bytes = new UTF8Encoding(false, true).GetBytes(principal);
        Assert.True(AuthoringSource.TryCreate(bytes, MainUri, out var source, out var failure), failure?.ToString());
        return XsltAuthoring.Inspect(source!, resolver);
    }

    private const string IncludedModule = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template name="included-template"><inc/></xsl:template>
</xsl:stylesheet>
""";

    [Fact]
    public void Ac08_RelativeInclude_ResolvedAgainstModuleBaseUri()
    {
        var resolver = new MemoryResolver();
        resolver.Add("file:///mods/included.xsl", IncludedModule);
        const string principal = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="included.xsl"/>
  <xsl:template match="/"><ok/></xsl:template>
</xsl:stylesheet>
""";

        var result = Inspect(principal, resolver);

        Assert.True(result.IsSuccess, result.Failure?.ToString());
        var snapshot = result.Snapshot!;
        Assert.Equal(2, snapshot.Modules.Count);
        Assert.Equal(MainUri, snapshot.PrincipalModule.ModuleUri);
        Assert.Equal(new Uri("file:///mods/included.xsl"), snapshot.Modules[1].ModuleUri);
        Assert.False(snapshot.Modules[1].IsPrincipal);
        Assert.Equal(1, snapshot.Modules[1].Precedence);

        // The resolver saw the absolute URI, and the referencing module URI.
        Assert.Contains(resolver.Requests, r => r.AbsoluteUri == new Uri("file:///mods/included.xsl") && r.Referencing == MainUri);

        var edge = snapshot.PrincipalModule.Edges.Single();
        Assert.Equal(AuthoringModuleEdgeKind.Include, edge.Kind);
        Assert.Equal("included.xsl", edge.HrefLiteral);
        Assert.Equal(new Uri("file:///mods/included.xsl"), edge.ResolvedUri);
        Assert.Null(edge.Diagnostic);
        Assert.False(edge.ReusesExistingModule);
        Assert.Equal(2, edge.ReferenceRange.StartLine);

        // Both modules export their own exact bytes.
        Assert.True(new UTF8Encoding(false, true).GetBytes(principal).SequenceEqual(snapshot.ExportOriginal(MainUri)));
        Assert.True(new UTF8Encoding(false, true).GetBytes(IncludedModule).SequenceEqual(snapshot.ExportOriginal(new Uri("file:///mods/included.xsl"))));
    }

    [Fact]
    public void Ac08_ImportEdge_ClassifiedAsImport()
    {
        var resolver = new MemoryResolver();
        resolver.Add("file:///mods/imported.xsl", IncludedModule);
        const string principal = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="imported.xsl"/>
  <xsl:template match="/"><ok/></xsl:template>
</xsl:stylesheet>
""";

        var result = Inspect(principal, resolver);

        Assert.True(result.IsSuccess, result.Failure?.ToString());
        var edge = result.Snapshot!.PrincipalModule.Edges.Single();
        Assert.Equal(AuthoringModuleEdgeKind.Import, edge.Kind);
        Assert.Equal(new Uri("file:///mods/imported.xsl"), edge.ResolvedUri);
        Assert.Equal(2, result.Snapshot.Modules.Count);
    }

    [Fact]
    public void Ac08_MissingModule_UnresolvedEdgeWithDiagnostic_PrincipalStillInspectable()
    {
        var resolver = new MemoryResolver();
        const string principal = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="missing.xsl"/>
  <xsl:template match="/"><ok/></xsl:template>
</xsl:stylesheet>
""";

        var result = Inspect(principal, resolver);

        Assert.True(result.IsSuccess, result.Failure?.ToString());
        var snapshot = result.Snapshot!;
        Assert.Single(snapshot.Modules);
        var edge = snapshot.PrincipalModule.Edges.Single();
        Assert.Equal("missing.xsl", edge.HrefLiteral);
        Assert.Equal(new Uri("file:///mods/missing.xsl"), edge.ResolvedUri);
        Assert.NotNull(edge.Diagnostic);
        Assert.Equal(AuthoringFailureKind.ResolverFailure, edge.Diagnostic!.Kind);
        Assert.NotNull(snapshot.PrincipalModule.Root.FindDescendant(n => n.ElementName?.LocalName == "template"));
    }

    [Fact]
    public void Ac08_IncludeCycle_RecordedAsDiagnostic_NoHang()
    {
        var resolver = new MemoryResolver();
        resolver.Add("file:///mods/b.xsl", """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="main.xsl"/>
</xsl:stylesheet>
""");
        const string principal = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="b.xsl"/>
</xsl:stylesheet>
""";

        var result = Inspect(principal, resolver);

        Assert.True(result.IsSuccess, result.Failure?.ToString());
        var snapshot = result.Snapshot!;
        Assert.Equal(2, snapshot.Modules.Count);
        var backEdge = snapshot.Modules[1].Edges.Single();
        Assert.Equal(new Uri("file:///mods/main.xsl"), backEdge.ResolvedUri);
        Assert.NotNull(backEdge.Diagnostic);
        Assert.Equal(AuthoringFailureKind.ResolverFailure, backEdge.Diagnostic!.Kind);
        Assert.Contains("cycle", backEdge.Diagnostic.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ac08_SameAbsoluteUriTwice_ReusesSameEnvelope()
    {
        var resolver = new MemoryResolver();
        resolver.Add("file:///mods/a.xsl", IncludedModule);
        const string principal = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="a.xsl"/>
  <xsl:include href="./a.xsl"/>
</xsl:stylesheet>
""";

        var result = Inspect(principal, resolver);

        Assert.True(result.IsSuccess, result.Failure?.ToString());
        var snapshot = result.Snapshot!;
        Assert.Equal(2, snapshot.Modules.Count);
        var edges = snapshot.PrincipalModule.Edges.ToList();
        Assert.Equal(2, edges.Count);
        Assert.False(edges[0].ReusesExistingModule);
        Assert.True(edges[1].ReusesExistingModule);
        Assert.Equal(new Uri("file:///mods/a.xsl"), edges[0].ResolvedUri);
        Assert.Equal(new Uri("file:///mods/a.xsl"), edges[1].ResolvedUri);
    }

    [Fact]
    public void Ac08_ResolverThrowing_IsRecordedAsDiagnostic()
    {
        var resolver = new ThrowingResolver();
        const string principal = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="anything.xsl"/>
</xsl:stylesheet>
""";

        var result = Inspect(principal, resolver);

        Assert.True(result.IsSuccess, result.Failure?.ToString());
        var edge = result.Snapshot!.PrincipalModule.Edges.Single();
        Assert.NotNull(edge.Diagnostic);
        Assert.Equal(AuthoringFailureKind.ResolverFailure, edge.Diagnostic!.Kind);
    }

    private sealed class ThrowingResolver : IAuthoringModuleResolver
    {
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
            => throw new InvalidOperationException("resolver exploded");
    }
}
