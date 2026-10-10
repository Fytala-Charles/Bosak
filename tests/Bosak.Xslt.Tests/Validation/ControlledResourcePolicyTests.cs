// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Contract tests for REQ-125 Slice B: controlled resource policy compile-time acquisition and receipts (AC-03, AC-05).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice B)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Security.Cryptography;
using System.Text;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Resources;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>
/// AC-03/AC-05 contract tests for the controlled resource policy: explicit include/import
/// bytes compile with no files on disk, approved bytes override conflicting disk content,
/// a denied-but-valid disk alternative is never read, receipts describe the exact effective
/// inputs, and diagnostics never expose credentials embedded in URIs.
/// </summary>
public class ControlledResourcePolicyTests
{
    private static readonly Encoding Utf8 = new UTF8Encoding(false);

    /// <summary>Records every request and answers through a swappable handler; null = abstain.</summary>
    internal sealed class StubAuthority : IControlledResourceAuthority
    {
        private readonly object _gate = new();
        private readonly List<ControlledResourceRequest> _requests = new();

        public Func<ControlledResourceRequest, ControlledResourceResponse?>? Handler { get; set; }

        public IReadOnlyList<ControlledResourceRequest> Requests
        {
            get { lock (_gate) return _requests.ToArray(); }
        }

        public ControlledResourceResponse? Authorize(ControlledResourceRequest request)
        {
            lock (_gate)
                _requests.Add(request);
            return Handler?.Invoke(request);
        }
    }

    private static StubAuthority DenyAllAuthority(out ControlledResourcePolicy policy)
    {
        var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Deny("not approved") };
        policy = new ControlledResourcePolicy(authority);
        return authority;
    }

    private static StubAuthority ApproveMapAuthority(
        IReadOnlyDictionary<string, byte[]> approved,
        out ControlledResourcePolicy policy,
        Func<ControlledResourceRequest, ControlledResourceResponse?>? fallback = null)
    {
        var authority = new StubAuthority
        {
            Handler = request => approved.TryGetValue(request.Uri, out var bytes)
                ? ControlledResourceResponse.Approve(bytes)
                : fallback?.Invoke(request),
        };
        policy = new ControlledResourcePolicy(authority);
        return authority;
    }

    private const string PrincipalWithInclude = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:include href="{0}"/>
          <xsl:template match="/">
            <out><xsl:call-template name="inc"/></out>
          </xsl:template>
        </xsl:stylesheet>
        """;

    private const string DiskModule = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template name="inc">DISK</xsl:template>
        </xsl:stylesheet>
        """;

    private const string MemoryModule = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template name="inc">MEMORY</xsl:template>
        </xsl:stylesheet>
        """;

    private static string WriteTempModule()
    {
        var path = Path.Combine(Path.GetTempPath(), $"bosak-req125-{Guid.NewGuid():N}.xsl");
        File.WriteAllText(path, DiskModule, Utf8);
        return path;
    }

    // ------------------------------------------------------------------
    // AC-03: explicit bytes compile with no files on disk
    // ------------------------------------------------------------------

    [Fact]
    public void Include_ExplicitBytes_CompileWithoutAnyFilesOnDisk()
    {
        var approved = new Dictionary<string, byte[]> { ["file:///memory/inc.xsl"] = Utf8.GetBytes(MemoryModule) };
        ApproveMapAuthority(approved, out var policy);

        var compiler = new XsltCompiler { ResourcePolicy = policy };
        var executable = compiler.Compile(string.Format(PrincipalWithInclude, "file:///memory/inc.xsl"), "file:///memory/main.xsl");
        var result = executable.TransformToString(XDocumentProvider.ParseXml("<dummy/>"));

        Assert.Contains("MEMORY", result);
    }

    [Fact]
    public void Import_ExplicitBytes_CompileWithoutAnyFilesOnDisk()
    {
        const string principal = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:import href="file:///memory/imp.xsl"/>
              <xsl:template match="/">
                <out><xsl:call-template name="imp"/></out>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var approved = new Dictionary<string, byte[]>
        {
            ["file:///memory/imp.xsl"] = Utf8.GetBytes("""
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:template name="imp">IMPORT-MEMORY</xsl:template>
                </xsl:stylesheet>
                """),
        };
        ApproveMapAuthority(approved, out var policy);

        var compiler = new XsltCompiler { ResourcePolicy = policy };
        var executable = compiler.Compile(principal, "file:///memory/main.xsl");
        var result = executable.TransformToString(XDocumentProvider.ParseXml("<dummy/>"));

        Assert.Contains("IMPORT-MEMORY", result);
    }

    // ------------------------------------------------------------------
    // AC-03: approved bytes override conflicting disk content
    // ------------------------------------------------------------------

    [Fact]
    public void Include_ApprovedBytes_OverrideConflictingDiskContent()
    {
        var diskPath = WriteTempModule();
        try
        {
            var diskUri = new Uri(diskPath).AbsoluteUri;
            var approved = new Dictionary<string, byte[]> { [diskUri] = Utf8.GetBytes(MemoryModule) };
            ApproveMapAuthority(approved, out var policy);

            var compiler = new XsltCompiler { ResourcePolicy = policy };
            var executable = compiler.Compile(string.Format(PrincipalWithInclude, diskUri), "file:///memory/main.xsl");
            var result = executable.TransformToString(XDocumentProvider.ParseXml("<dummy/>"));

            Assert.Contains("MEMORY", result);
            Assert.DoesNotContain("DISK", result);
        }
        finally
        {
            File.Delete(diskPath);
        }
    }

    // ------------------------------------------------------------------
    // AC-03 (decisive): deny-all + valid file on disk -> refused, not the file
    // ------------------------------------------------------------------

    [Fact]
    public void Include_DenyAll_WithValidFileOnDisk_RefusedNotReadFromDisk()
    {
        var diskPath = WriteTempModule();
        try
        {
            var diskUri = new Uri(diskPath).AbsoluteUri;
            DenyAllAuthority(out var policy);

            var compiler = new XsltCompiler { ResourcePolicy = policy };
            var ex = Assert.Throws<InvalidOperationException>(
                () => compiler.Compile(string.Format(PrincipalWithInclude, diskUri), "file:///memory/main.xsl"));

            Assert.StartsWith("XV0004", ex.Message);
            Assert.Contains("ModuleInclude", ex.Message);
            Assert.Contains(diskUri, ex.Message);
        }
        finally
        {
            File.Delete(diskPath);
        }
    }

    [Fact]
    public void Import_DenyAll_WithValidFileOnDisk_RefusedNotReadFromDisk()
    {
        var diskPath = WriteTempModule();
        try
        {
            var diskUri = new Uri(diskPath).AbsoluteUri;
            const string principal = """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:import href="{0}"/>
                  <xsl:template match="/"><out/></xsl:template>
                </xsl:stylesheet>
                """;
            DenyAllAuthority(out var policy);

            var compiler = new XsltCompiler { ResourcePolicy = policy };
            var ex = Assert.Throws<InvalidOperationException>(
                () => compiler.Compile(string.Format(principal, diskUri), "file:///memory/main.xsl"));

            Assert.StartsWith("XV0004", ex.Message);
            Assert.Contains("ModuleImport", ex.Message);
        }
        finally
        {
            File.Delete(diskPath);
        }
    }

    [Fact]
    public void Include_AbstainingAuthority_WithValidFileOnDisk_RefusedNotReadFromDisk()
    {
        var diskPath = WriteTempModule();
        try
        {
            var diskUri = new Uri(diskPath).AbsoluteUri;
            // Abstain = deny under the controlled profile: the disk file must not be read.
            var authority = new StubAuthority { Handler = _ => null };
            var policy = new ControlledResourcePolicy(authority);

            var compiler = new XsltCompiler { ResourcePolicy = policy };
            var ex = Assert.Throws<InvalidOperationException>(
                () => compiler.Compile(string.Format(PrincipalWithInclude, diskUri), "file:///memory/main.xsl"));

            Assert.StartsWith("XV0004", ex.Message);
        }
        finally
        {
            File.Delete(diskPath);
        }
    }

    [Fact]
    public void Include_ThrowingAuthority_WithValidFileOnDisk_RefusedNotReadFromDisk()
    {
        var diskPath = WriteTempModule();
        try
        {
            var diskUri = new Uri(diskPath).AbsoluteUri;
            var authority = new StubAuthority { Handler = _ => throw new InvalidOperationException("authority boom") };
            var policy = new ControlledResourcePolicy(authority);

            var compiler = new XsltCompiler { ResourcePolicy = policy };
            var ex = Assert.Throws<InvalidOperationException>(
                () => compiler.Compile(string.Format(PrincipalWithInclude, diskUri), "file:///memory/main.xsl"));

            Assert.StartsWith("XV0004", ex.Message);
            Assert.Single(policy.Receipts);
            Assert.Equal(ControlledResourceReceiptOutcome.AuthorityError, policy.Receipts[0].Outcome);
        }
        finally
        {
            File.Delete(diskPath);
        }
    }

    // ------------------------------------------------------------------
    // AC-05: receipts describe the exact effective inputs
    // ------------------------------------------------------------------

    [Fact]
    public void Include_Approval_ReceiptDescribesRequestedUriEffectiveUriPurposeBytesAndHash()
    {
        var bytes = Utf8.GetBytes(MemoryModule);
        var approved = new Dictionary<string, byte[]> { ["file:///memory/inc.xsl"] = bytes };
        ApproveMapAuthority(approved, out var policy);

        var compiler = new XsltCompiler { ResourcePolicy = policy };
        compiler.Compile(string.Format(PrincipalWithInclude, "file:///memory/inc.xsl"), "file:///memory/main.xsl");

        var receipt = Assert.Single(policy.Receipts);
        Assert.Equal(ControlledResourceRoute.ModuleInclude, receipt.Route);
        Assert.Equal("file:///memory/inc.xsl", receipt.RequestedUri);
        Assert.Equal("file:///memory/inc.xsl", receipt.EffectiveUri);
        Assert.Equal(ControlledResourceReceiptOutcome.Approved, receipt.Outcome);
        Assert.Equal(bytes.Length, receipt.ByteCount);
        var expectedHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        Assert.Equal(expectedHash, receipt.Sha256);
    }

    [Fact]
    public void Include_Denial_ReceiptRecordsOutcomeWithoutAcquisitionEvidence()
    {
        DenyAllAuthority(out var policy);

        var compiler = new XsltCompiler { ResourcePolicy = policy };
        Assert.Throws<InvalidOperationException>(
            () => compiler.Compile(string.Format(PrincipalWithInclude, "file:///memory/inc.xsl"), "file:///memory/main.xsl"));

        var receipt = Assert.Single(policy.Receipts);
        Assert.Equal(ControlledResourceRoute.ModuleInclude, receipt.Route);
        Assert.Equal("file:///memory/inc.xsl", receipt.RequestedUri);
        Assert.Equal(ControlledResourceReceiptOutcome.Denied, receipt.Outcome);
        Assert.Null(receipt.ByteCount);
        Assert.Null(receipt.Sha256);
        Assert.NotNull(receipt.Message);
    }

    [Fact]
    public void Include_ApprovalWithEffectiveUri_ReceiptRecordsTheEffectiveUri()
    {
        var bytes = Utf8.GetBytes(MemoryModule);
        var authority = new StubAuthority
        {
            Handler = _ => ControlledResourceResponse.Approve(bytes, effectiveUri: "file:///effective/inc.xsl"),
        };
        var policy = new ControlledResourcePolicy(authority);

        var compiler = new XsltCompiler { ResourcePolicy = policy };
        var executable = compiler.Compile(
            string.Format(PrincipalWithInclude, "file:///memory/inc.xsl"), "file:///memory/main.xsl");
        var result = executable.TransformToString(XDocumentProvider.ParseXml("<dummy/>"));
        Assert.Contains("MEMORY", result);

        var receipt = Assert.Single(policy.Receipts);
        Assert.Equal("file:///memory/inc.xsl", receipt.RequestedUri);
        Assert.Equal("file:///effective/inc.xsl", receipt.EffectiveUri);
    }

    // ------------------------------------------------------------------
    // Credential redaction (diagnostics and receipts must not expose userinfo)
    // ------------------------------------------------------------------

    [Fact]
    public void Include_DeniedUriWithCredentials_MessageAndReceiptAreRedacted()
    {
        DenyAllAuthority(out var policy);

        var compiler = new XsltCompiler { ResourcePolicy = policy };
        var ex = Assert.Throws<InvalidOperationException>(
            () => compiler.Compile(
                string.Format(PrincipalWithInclude, "https://user:secret@example.com/inc.xsl"),
                "file:///memory/main.xsl"));

        Assert.StartsWith("XV0004", ex.Message);
        Assert.DoesNotContain("secret", ex.Message);
        Assert.DoesNotContain("user:secret", ex.Message);
        Assert.Contains("https://example.com/inc.xsl", ex.Message);

        var receipt = Assert.Single(policy.Receipts);
        Assert.Equal("https://example.com/inc.xsl", receipt.RequestedUri);
        Assert.DoesNotContain("secret", receipt.ToString());
    }

    [Fact]
    public void RedactCredentials_StripsUserInfoButKeepsHostPathAndQuery()
    {
        Assert.Equal(
            "https://example.com:8443/a/b?x=1",
            ControlledResourcePolicy.RedactCredentials("https://user:pw@example.com:8443/a/b?x=1"));
        Assert.Equal("file:///plain.xsl", ControlledResourcePolicy.RedactCredentials("file:///plain.xsl"));
        Assert.Equal("urn:test:opaque", ControlledResourcePolicy.RedactCredentials("urn:test:opaque"));
    }

    // ------------------------------------------------------------------
    // Unsupported routes refuse explicitly (never silently runnable)
    // ------------------------------------------------------------------

    [Fact]
    public void UnsupportedRoute_DeclaredOnPolicy_RefusesWithXv0005()
    {
        var approved = new Dictionary<string, byte[]> { ["file:///memory/inc.xsl"] = Utf8.GetBytes(MemoryModule) };
        var authority = new StubAuthority
        {
            Handler = request => approved.TryGetValue(request.Uri, out var bytes)
                ? ControlledResourceResponse.Approve(bytes)
                : null,
        };
        var policy = new ControlledResourcePolicy(authority, unsupportedRoutes: new[] { ControlledResourceRoute.ModuleInclude });
        Assert.False(policy.IsRouteSupported(ControlledResourceRoute.ModuleInclude));
        Assert.True(policy.IsRouteSupported(ControlledResourceRoute.ModuleImport));

        var compiler = new XsltCompiler { ResourcePolicy = policy };
        var ex = Assert.Throws<InvalidOperationException>(
            () => compiler.Compile(
                string.Format(PrincipalWithInclude, "file:///memory/inc.xsl"), "file:///memory/main.xsl"));

        Assert.StartsWith("XV0005", ex.Message);
        Assert.Contains("ModuleInclude", ex.Message);
        var receipt = Assert.Single(policy.Receipts);
        Assert.Equal(ControlledResourceReceiptOutcome.RefusedUnsupported, receipt.Outcome);
        // The authority was never consulted for an unsupported route.
        Assert.Empty(authority.Requests);
    }

    [Fact]
    public void ExtensionFunctionRoute_IsUnsupportedByDefault()
    {
        var policy = new ControlledResourcePolicy(new StubAuthority());
        Assert.False(policy.IsRouteSupported(ControlledResourceRoute.ExtensionFunction));
        Assert.True(policy.IsRouteSupported(ControlledResourceRoute.Document));
        Assert.True(policy.IsRouteSupported(ControlledResourceRoute.Transform));
    }

    // ------------------------------------------------------------------
    // AC-09 posture: existing callers unchanged (default null policy)
    // ------------------------------------------------------------------

    [Fact]
    public void CompilerWithoutPolicy_UsesUriResolverAndDiskAsBefore()
    {
        var diskPath = WriteTempModule();
        try
        {
            var diskUri = new Uri(diskPath).AbsoluteUri;
            var compiler = new XsltCompiler();
            var executable = compiler.Compile(string.Format(PrincipalWithInclude, diskUri), "file:///memory/main.xsl");
            var result = executable.TransformToString(XDocumentProvider.ParseXml("<dummy/>"));
            Assert.Contains("DISK", result);
        }
        finally
        {
            File.Delete(diskPath);
        }
    }

    [Fact]
    public void EvaluationContextWithoutPolicy_LoadDocumentKeepsLegacyBehavior()
    {
        var ctx = new EvaluationContext();
        Assert.Null(ctx.ResourcePolicy);
        // No policy attached: no receipts exist anywhere and the loader contract is untouched.
        Assert.Throws<InvalidOperationException>(() => ctx.LoadDocument("urn:test:anything"));
    }
}
