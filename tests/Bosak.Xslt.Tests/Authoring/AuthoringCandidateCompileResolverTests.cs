// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Regression tests for REQ-124 acceptance finding F1: candidate compilation routed through the snapshot's module resolver.
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
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

/// <summary>
/// F1 regression evidence: <see cref="AuthoringEditCandidate.Compile"/> must resolve include/import
/// through the snapshot's effective <see cref="IAuthoringModuleResolver"/> — never implicitly through
/// the file system — while preserving the deliberate default-filesystem behavior.
/// </summary>
public class AuthoringCandidateCompileResolverTests
{
    private static readonly Uri BaseUri = new("file:///C:/project/main.xsl");
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private const string InputXml = "<input><name>Braid</name><id>42</id></input>";

    private const string PrincipalWithInclude = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:include href="included.xsl"/>
          <xsl:template match="/">
            <out><xsl:value-of select="/input/name"/><xsl:call-template name="lib"/></out>
          </xsl:template>
        </xsl:stylesheet>
        """;

    private const string PrincipalWithImport = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:import href="imported.xsl"/>
          <xsl:template match="/">
            <out><xsl:value-of select="/input/name"/><xsl:call-template name="lib"/></out>
          </xsl:template>
        </xsl:stylesheet>
        """;

    private static string IncludedModule(string marker) => $"""
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template name="lib"><lib>{marker}</lib></xsl:template>
        </xsl:stylesheet>
        """;

    private const string PrincipalNested = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:include href="sub/level1.xsl"/>
          <xsl:template match="/">
            <out><xsl:value-of select="/input/name"/><xsl:call-template name="deep"/></out>
          </xsl:template>
        </xsl:stylesheet>
        """;

    private const string Level1Module = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:include href="level2.xsl"/>
        </xsl:stylesheet>
        """;

    private const string Level2Module = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template name="deep"><deep>nested</deep></xsl:template>
        </xsl:stylesheet>
        """;

    private sealed class MemoryModuleResolver : IAuthoringModuleResolver
    {
        private readonly Dictionary<Uri, AuthoringSource> _modules = new();

        public void Add(Uri absoluteUri, string text)
        {
            Assert.True(
                AuthoringSource.TryCreate(Utf8.GetBytes(text), absoluteUri, out var source, out var failure),
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

    private static AuthoringEditCandidate ProposeIdEdit(AuthoringSnapshot snapshot)
    {
        var valueOf = snapshot.PrincipalModule.Root.FindDescendant(
            n => n.Kind == AuthoringNodeKind.Element && n.ElementName!.LocalName == "value-of")
            ?? throw new InvalidOperationException("xsl:value-of not found.");
        var result = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(valueOf.Id, "select", "/input/id"));
        Assert.True(result.IsSuccess, result.Failure?.ToString());
        return result.Candidate!;
    }

    private static string Transform(AuthoringEditCandidate candidate) =>
        candidate.Compile().TransformToString(new XDocumentNode(XDocument.Parse(InputXml)));

    // Case 1: in-memory include, no disk module anywhere.
    [Fact]
    public void F1_InMemoryInclude_NoDiskModule_CompilesWithSuppliedBytes()
    {
        var resolver = new MemoryModuleResolver();
        resolver.Add(new Uri("file:///C:/project/included.xsl"), IncludedModule("supplied"));

        Assert.True(AuthoringSource.TryCreate(Utf8.GetBytes(PrincipalWithInclude), BaseUri, out var source, out var failure), failure?.ToString());
        var inspection = XsltAuthoring.Inspect(source!, resolver);
        Assert.True(inspection.IsSuccess, inspection.Failure?.ToString());

        var candidate = ProposeIdEdit(inspection.Snapshot!);
        var output = Transform(candidate);

        Assert.Contains("42", output, StringComparison.Ordinal);
        Assert.Contains("<lib>supplied</lib>", output, StringComparison.Ordinal);
    }

    // Case 2: deny-all resolver, valid disk alternative present — never substituted (decisive).
    [Fact]
    public void F1_DenyAllResolver_NeverSubstitutesValidDiskModule()
    {
        var dir = Path.Combine(Path.GetTempPath(), "bosak-req124-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "included.xsl"), IncludedModule("disk"), Utf8);
            var mainUri = new Uri(Path.Combine(dir, "main.xsl")).AbsoluteUri;
            Assert.True(AuthoringSource.TryCreate(Utf8.GetBytes(PrincipalWithInclude), new Uri(mainUri), out var source, out var failure), failure?.ToString());

            var inspection = XsltAuthoring.Inspect(source!, new DenyAllModuleResolver());
            Assert.True(inspection.IsSuccess, inspection.Failure?.ToString());
            var candidate = ProposeIdEdit(inspection.Snapshot!);

            // Failure-isolation evidence: capture derived state before the failing compile.
            var emittedBefore = candidate.EmittedSource.ToArray();
            var correspondenceBefore = candidate.NodeCorrespondence.ToDictionary(pair => pair.Key, pair => pair.Value);
            var changedSlotBefore = candidate.ChangedSlot.NewValueRange;

            // The disk module is valid, so any filesystem fallback would compile successfully;
            // per resolver policy compilation must fail instead.
            var ex = Assert.ThrowsAny<Exception>(() => candidate.Compile());
            Assert.Contains("included.xsl", ex.Message, StringComparison.OrdinalIgnoreCase);

            Assert.True(emittedBefore.SequenceEqual(candidate.EmittedSource.ToArray()));
            Assert.Equal(correspondenceBefore, candidate.NodeCorrespondence);
            Assert.Equal(changedSlotBefore, candidate.ChangedSlot.NewValueRange);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    // Case 3: supplied module content differs from the disk module (import variant); the executable
    // must follow the supplied module.
    [Fact]
    public void F1_SuppliedModuleContent_WinsOverDiskAlternative_Import()
    {
        var dir = Path.Combine(Path.GetTempPath(), "bosak-req124-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "imported.xsl"), IncludedModule("disk"), Utf8);
            var mainUri = new Uri(Path.Combine(dir, "main.xsl")).AbsoluteUri;

            // The supplied module is keyed by the URI the import actually resolves to — the
            // temp-dir base URI — while its content deliberately differs from the disk file.
            var resolver = new MemoryModuleResolver();
            resolver.Add(new Uri(new Uri(mainUri), "imported.xsl"), IncludedModule("supplied"));

            Assert.True(AuthoringSource.TryCreate(Utf8.GetBytes(PrincipalWithImport), new Uri(mainUri), out var source, out var failure), failure?.ToString());
            var inspection = XsltAuthoring.Inspect(source!, resolver);
            Assert.True(inspection.IsSuccess, inspection.Failure?.ToString());

            var candidate = ProposeIdEdit(inspection.Snapshot!);
            var output = Transform(candidate);

            Assert.Contains("<lib>supplied</lib>", output, StringComparison.Ordinal);
            Assert.DoesNotContain("disk", output, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    // Case 4: relative nested module reference — module-relative base URI chaining through the bridge.
    [Fact]
    public void F1_NestedRelativeModuleReference_BaseUriContractPreserved()
    {
        var resolver = new MemoryModuleResolver();
        resolver.Add(new Uri("file:///C:/project/sub/level1.xsl"), Level1Module);
        resolver.Add(new Uri("file:///C:/project/sub/level2.xsl"), Level2Module);

        Assert.True(AuthoringSource.TryCreate(Utf8.GetBytes(PrincipalNested), BaseUri, out var source, out var failure), failure?.ToString());
        var inspection = XsltAuthoring.Inspect(source!, resolver);
        Assert.True(inspection.IsSuccess, inspection.Failure?.ToString());

        var candidate = ProposeIdEdit(inspection.Snapshot!);
        var output = Transform(candidate);

        Assert.Contains("42", output, StringComparison.Ordinal);
        Assert.Contains("<deep>nested</deep>", output, StringComparison.Ordinal);
    }

    // Case 5: default filesystem resolver — deliberate default-path behavior unchanged.
    [Fact]
    public void F1_DefaultFileSystemResolver_CompileBehaviorUnchanged()
    {
        var dir = Path.Combine(Path.GetTempPath(), "bosak-req124-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "included.xsl"), IncludedModule("disk"), Utf8);
            var mainPath = Path.Combine(dir, "main.xsl");
            File.WriteAllText(mainPath, PrincipalWithInclude, Utf8);
            var mainUri = new Uri(mainPath).AbsoluteUri;

            Assert.True(AuthoringSource.TryCreate(File.ReadAllBytes(mainPath), new Uri(mainUri), out var source, out var failure), failure?.ToString());
            var inspection = XsltAuthoring.Inspect(source!, new FileSystemAuthoringModuleResolver());
            Assert.True(inspection.IsSuccess, inspection.Failure?.ToString());

            var candidate = ProposeIdEdit(inspection.Snapshot!);
            var output = Transform(candidate);

            Assert.Contains("42", output, StringComparison.Ordinal);
            Assert.Contains("<lib>disk</lib>", output, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    // Case 6: snapshot isolation — successful compile leaves bytes, ranges and correspondence unchanged.
    [Fact]
    public void F1_SnapshotIsolation_SuccessfulCompile_ChangesNothing()
    {
        var resolver = new MemoryModuleResolver();
        resolver.Add(new Uri("file:///C:/project/included.xsl"), IncludedModule("supplied"));

        Assert.True(AuthoringSource.TryCreate(Utf8.GetBytes(PrincipalWithInclude), BaseUri, out var source, out var failure), failure?.ToString());
        var inspection = XsltAuthoring.Inspect(source!, resolver);
        Assert.True(inspection.IsSuccess, inspection.Failure?.ToString());
        var snapshot = inspection.Snapshot!;

        var candidate = ProposeIdEdit(snapshot);
        var originalExport = snapshot.ExportOriginal(BaseUri);
        var emittedBefore = candidate.EmittedSource.ToArray();
        var correspondenceBefore = candidate.NodeCorrespondence.ToDictionary(pair => pair.Key, pair => pair.Value);

        _ = Transform(candidate);
        _ = candidate.Compile();

        Assert.True(originalExport.SequenceEqual(snapshot.ExportOriginal(BaseUri)));
        Assert.True(emittedBefore.SequenceEqual(candidate.EmittedSource.ToArray()));
        Assert.Equal(correspondenceBefore, candidate.NodeCorrespondence);
        Assert.Equal("/input/id", candidate.ChangedSlot.NewRawLiteral);
    }
}
