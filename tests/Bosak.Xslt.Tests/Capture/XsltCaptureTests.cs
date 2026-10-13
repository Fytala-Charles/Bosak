// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Contract tests for REQ-125 Slice C: captured output identities, bytes, bounds and classified failures (AC-06, AC-07).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice C)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Capture;
using Xunit;

namespace Bosak.Xslt.Tests.Capture;

/// <summary>
/// AC-06/AC-07 contract tests for the captured output contract:
/// AC-06 — "Public preview captures principal plus multiple secondary outputs, correct resolved
/// URIs and serialization bytes; no output files are created."
/// AC-07 — "Duplicate output URIs, message floods, per-output/count/aggregate overflow and partial
/// failures produce classified non-success outcomes."
/// </summary>
public class XsltCaptureTests
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    private static XsltExecutable Compile(string xsl)
        => new XsltCompiler().Compile(xsl, "file:///capture/main.xsl");

    private static XDocumentNode Source()
        => new(new XDocument(new XElement("doc")));

    private static string TempBaseUri(out string localDir)
    {
        localDir = Path.Combine(Path.GetTempPath(), "bosak-capture-tests", Guid.NewGuid().ToString("N"));
        return new Uri(localDir + Path.DirectorySeparatorChar).AbsoluteUri;
    }

    private static string Decode(XsltCapturedOutput output)
        => Decode(output, output.OutputProperties.Encoding);

    private static string Decode(XsltCapturedOutput output, string encodingName)
    {
        var encoding = Encoding.GetEncoding(encodingName);
        var preamble = encoding.GetPreamble();
        var offset = preamble.Length > 0 && output.Bytes.Count >= preamble.Length
            ? output.Bytes.Take(preamble.Length).SequenceEqual(preamble) ? preamble.Length : 0
            : 0;
        return encoding.GetString(output.Bytes.Skip(offset).ToArray());
    }

    // ------------------------------------------------------------------
    // AC-06: identities + bytes, principal only
    // ------------------------------------------------------------------

    [Fact]
    public void Capture_PrincipalOnly_ReturnsBaseOutputUriIdentityAndBytes()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out>hi</out>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/principal.xml",
        });

        Assert.True(result.IsSuccess, result.ToString());
        Assert.Equal(XsltCaptureOutcome.Success, result.Outcome);
        Assert.NotNull(result.Principal);
        Assert.True(result.Principal!.IsPrincipal);
        // The principal output carries the base output URI identity even though the
        // stylesheet never names it.
        Assert.Equal("file:///out/principal.xml", result.Principal.ResolvedUri);
        Assert.Empty(result.SecondaryOutputs);
        Assert.Empty(result.Failures);
        var text = Decode(result.Principal);
        Assert.Contains("<out>hi</out>", text);
    }

    [Fact]
    public void Capture_PrincipalWithoutBaseOutputUri_HasEmptyIdentity()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source());

        Assert.True(result.IsSuccess, result.ToString());
        Assert.Equal(string.Empty, result.Principal!.ResolvedUri);
    }

    // ------------------------------------------------------------------
    // AC-06: identities + bytes, multiple secondaries
    // ------------------------------------------------------------------

    [Fact]
    public void Capture_MultipleSecondaries_ReturnsResolvedUrisAndBytesInProductionOrder()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='a.xml'><a>first</a></xsl:result-document>
    <xsl:result-document href='sub/b.xml'><b>second</b></xsl:result-document>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/base.xml",
        });

        Assert.True(result.IsSuccess, result.ToString());
        Assert.Equal("file:///out/base.xml", result.Principal!.ResolvedUri);
        Assert.Equal(2, result.SecondaryOutputs.Count);
        Assert.Equal("file:///out/a.xml", result.SecondaryOutputs[0].ResolvedUri);
        Assert.False(result.SecondaryOutputs[0].IsPrincipal);
        Assert.Equal("file:///out/sub/b.xml", result.SecondaryOutputs[1].ResolvedUri);
        Assert.Contains("<a>first</a>", Decode(result.SecondaryOutputs[0]));
        Assert.Contains("<b>second</b>", Decode(result.SecondaryOutputs[1]));
    }

    [Fact]
    public void Capture_SecondaryOverrides_OutputDefinitionCarriesEffectiveProperties()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='s.xml' method='text' encoding='UTF-16'>plain text</xsl:result-document>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/base.xml",
        });

        Assert.True(result.IsSuccess, result.ToString());
        var secondary = Assert.Single(result.SecondaryOutputs);
        Assert.Equal("text", secondary.OutputProperties.Method);
        Assert.Equal("UTF-16", secondary.OutputProperties.Encoding);
        // Effective encoding fidelity: the bytes are UTF-16, not a UTF-8 relabeling of the string.
        Assert.True(secondary.Bytes.Count % 2 == 0);
        Assert.Equal("plain text", Decode(secondary));
    }

    // ------------------------------------------------------------------
    // AC-06: no output files are created
    // ------------------------------------------------------------------

    [Fact]
    public void Capture_WritesNothingToDisk_PathsOnMissingDrive()
    {
        // The strongest no-disk-write proof: every output URI points into a directory on a
        // drive that does not exist. Any accidental disk write would throw (or create the
        // directory); capture must succeed and leave nothing behind.
        var drive = MissingDriveRoot();
        var dir = $"{drive}bosak-capture-proof-{Guid.NewGuid():N}";
        var baseUri = $"file:///{dir}/base.xml";

        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='secondary/out.xml'><s/></xsl:result-document>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions { BaseOutputUri = baseUri });

        Assert.True(result.IsSuccess, result.ToString());
        Assert.Equal(baseUri, result.Principal!.ResolvedUri);
        Assert.False(File.Exists($"{dir}/base.xml"));
        Assert.False(File.Exists($"{dir}/secondary/out.xml"));
        Assert.False(Directory.Exists(dir));
    }

    [Fact]
    public void Capture_CollidingPath_PreExistingContentIsUntouched()
    {
        var baseUri = TempBaseUri(out var dir);
        var existingPath = Path.Combine(dir, "existing.xml");
        Directory.CreateDirectory(dir);
        File.WriteAllText(existingPath, "PRE-EXISTING", Utf8NoBom);

        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='existing.xml'><new-content/></xsl:result-document>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions { BaseOutputUri = baseUri });

        Assert.True(result.IsSuccess, result.ToString());
        Assert.Equal("PRE-EXISTING", File.ReadAllText(existingPath));
        Assert.False(File.Exists(Path.Combine(dir, "base.xml")));
    }

    [Fact]
    public void DefaultTransform_WithoutCapture_StillWritesSecondaryDocumentsToDisk()
    {
        var baseUri = TempBaseUri(out var dir);

        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='disk.xml'><on-disk/></xsl:result-document>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        // Default behavior is bit-identical: no capture requested → the legacy disk write happens.
        executable.Transform(Source(), baseOutputUri: baseUri);

        var written = Path.Combine(dir, "disk.xml");
        Assert.True(File.Exists(written));
        Assert.Contains("<on-disk/>", File.ReadAllText(written));
    }

    // ------------------------------------------------------------------
    // AC-06: byte fidelity — encoding, byte-order mark, method
    // ------------------------------------------------------------------

    [Fact]
    public void Capture_Utf16ByteOrderMark_EmittedAsPreambleBytes_ForSecondary()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='u.xml' encoding='UTF-16' byte-order-mark='yes'><u>uni</u></xsl:result-document>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/base.xml",
        });

        Assert.True(result.IsSuccess, result.ToString());
        var secondary = Assert.Single(result.SecondaryOutputs);
        Assert.True(secondary.OutputProperties.ByteOrderMark);
        Assert.True(secondary.Bytes.Count >= 2);
        Assert.Equal(0xFF, secondary.Bytes[0]); // UTF-16LE preamble FF FE
        Assert.Equal(0xFE, secondary.Bytes[1]);
        var text = Encoding.Unicode.GetString(secondary.Bytes.Skip(2).ToArray());
        Assert.Contains("<u>uni</u>", text);
        Assert.Contains("encoding=\"UTF-16\"", text);
    }

    [Fact]
    public void Capture_Utf16Principal_HonorsStylesheetOutputDefinition()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:output encoding='UTF-16' byte-order-mark='yes'/>
  <xsl:template match='/'>
    <p>principal-16</p>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/p.xml",
        });

        Assert.True(result.IsSuccess, result.ToString());
        Assert.True(result.Principal!.OutputProperties.ByteOrderMark);
        Assert.Equal(0xFF, result.Principal.Bytes[0]);
        Assert.Equal(0xFE, result.Principal.Bytes[1]);
        var text = Encoding.Unicode.GetString(result.Principal.Bytes.Skip(2).ToArray());
        Assert.Contains("<p>principal-16</p>", text);
    }

    [Fact]
    public void Capture_Utf8Default_NoBomBytes()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out>plain</out>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source());

        Assert.True(result.IsSuccess, result.ToString());
        Assert.Equal("UTF-8", result.Principal!.OutputProperties.Encoding);
        Assert.NotEqual(0xEF, result.Principal.Bytes[0]);
        Assert.Contains("<out>plain</out>", Utf8NoBom.GetString(result.Principal.Bytes.ToArray()));
    }

    [Fact]
    public void Capture_MethodFidelity_XmlHtmlTextJson()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='x.xml'><x>xml</x></xsl:result-document>
    <xsl:result-document href='h.html' method='html'><html><body><p>html</p></body></html></xsl:result-document>
    <xsl:result-document href='t.txt' method='text'>raw &amp; text</xsl:result-document>
    <xsl:result-document href='j.json' method='json'><xsl:sequence select=""map{'k':'v'}""/></xsl:result-document>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/base.xml",
        });

        Assert.True(result.IsSuccess, result.ToString());
        Assert.Equal(4, result.SecondaryOutputs.Count);
        var xml = result.SecondaryOutputs[0];
        var html = result.SecondaryOutputs[1];
        var text = result.SecondaryOutputs[2];
        var json = result.SecondaryOutputs[3];

        Assert.Equal("xml", xml.OutputProperties.Method);
        Assert.StartsWith("<?xml", Decode(xml));

        Assert.Equal("html", html.OutputProperties.Method);
        Assert.Contains("<p>html</p>", Decode(html));

        Assert.Equal("text", text.OutputProperties.Method);
        Assert.Equal("raw & text", Decode(text));

        Assert.Equal("json", json.OutputProperties.Method);
        var jsonText = Decode(json).TrimStart();
        Assert.True(jsonText.StartsWith("{") || jsonText.StartsWith("["), jsonText);
        Assert.Contains("\"k\"", jsonText);
    }

    // ------------------------------------------------------------------
    // AC-07: bounded production — count / per-output / aggregate
    // ------------------------------------------------------------------

    [Fact]
    public void Capture_SecondaryCountBoundExceeded_TerminatesWithClassifiedFailure()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='one.xml'><one/></xsl:result-document>
    <xsl:result-document href='two.xml'><two/></xsl:result-document>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/base.xml",
            MaxSecondaryOutputCount = 1,
        });

        Assert.Equal(XsltCaptureOutcome.Partial, result.Outcome);
        var retained = Assert.Single(result.SecondaryOutputs);
        Assert.Equal("file:///out/one.xml", retained.ResolvedUri);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(XsltCaptureFailureKind.OutputCountLimitExceeded, failure.Kind);
        Assert.Equal("XV0006", failure.Code);
        Assert.Equal("file:///out/two.xml", failure.ResolvedUri);
    }

    [Fact]
    public void Capture_PerOutputByteBoundExceeded_DropsOffendingOutputAndTerminates()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='small.xml'><s/></xsl:result-document>
    <xsl:result-document href='big.xml'><b><xsl:value-of select='string-join(for $i in 1 to 100 return ""x"", """")'/></b></xsl:result-document>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/base.xml",
            MaxBytesPerOutput = 100,
        });

        Assert.Equal(XsltCaptureOutcome.Partial, result.Outcome);
        var retained = Assert.Single(result.SecondaryOutputs);
        Assert.Equal("file:///out/small.xml", retained.ResolvedUri);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(XsltCaptureFailureKind.OutputSizeLimitExceeded, failure.Kind);
        Assert.Equal("XV0007", failure.Code);
        Assert.Equal("file:///out/big.xml", failure.ResolvedUri);
    }

    [Fact]
    public void Capture_AggregateByteBoundExceeded_DropsOffendingOutputAndTerminates()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='first.xml'><f/></xsl:result-document>
    <xsl:result-document href='second.xml'><s><xsl:value-of select='string-join(for $i in 1 to 100 return ""y"", """")'/></s></xsl:result-document>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/base.xml",
            MaxTotalBytes = 120,
        });

        Assert.Equal(XsltCaptureOutcome.Partial, result.Outcome);
        var retained = Assert.Single(result.SecondaryOutputs);
        Assert.Equal("file:///out/first.xml", retained.ResolvedUri);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(XsltCaptureFailureKind.AggregateSizeLimitExceeded, failure.Kind);
        Assert.Equal("XV0008", failure.Code);
        Assert.Equal("file:///out/second.xml", failure.ResolvedUri);
    }

    [Fact]
    public void Capture_PrincipalByteBoundExceeded_PrincipalDroppedWithClassifiedFailure()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <p><xsl:value-of select='string-join(for $i in 1 to 100 return ""z"", """")'/></p>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/base.xml",
            MaxBytesPerOutput = 100,
        });

        // The transformation itself completed; only the principal output breached the bound.
        Assert.Equal(XsltCaptureOutcome.Partial, result.Outcome);
        Assert.Null(result.Principal);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(XsltCaptureFailureKind.OutputSizeLimitExceeded, failure.Kind);
        Assert.Equal("XV0007", failure.Code);
        Assert.Equal("file:///out/base.xml", failure.ResolvedUri);
    }

    [Fact]
    public void Capture_NegativeBounds_Rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new XsltCaptureOptions { MaxSecondaryOutputCount = -1 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new XsltCaptureOptions { MaxBytesPerOutput = -1 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new XsltCaptureOptions { MaxTotalBytes = -1 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new XsltCaptureOptions { MaxMessages = -1 });
    }

    // ------------------------------------------------------------------
    // AC-07: duplicate output URIs → XTDE1490 collision
    // ------------------------------------------------------------------

    [Fact]
    public void Capture_DuplicateUri_ReportsCollisionWithSpecCodeAndRetainsEarlierOutput()
    {
        var baseUri = TempBaseUri(out var dir);

        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='dup.xml'><first/></xsl:result-document>
    <xsl:result-document href='dup.xml'><second/></xsl:result-document>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions { BaseOutputUri = baseUri });

        Assert.Equal(XsltCaptureOutcome.Partial, result.Outcome);
        var retained = Assert.Single(result.SecondaryOutputs);
        var collidingUri = baseUri + "dup.xml";
        Assert.Equal(collidingUri, retained.ResolvedUri);
        Assert.Equal("first", XElement.Parse(Decode(retained)).Name.LocalName);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(XsltCaptureFailureKind.UriCollision, failure.Kind);
        Assert.Equal("XTDE1490", failure.Code);
        // The engine's XTDE1490 fires at the colliding write and does not attribute a single
        // output, so the failure carries no resolved identity.
        Assert.Null(failure.ResolvedUri);
        // The colliding write never reached the file system.
        Assert.False(File.Exists(Path.Combine(dir, "dup.xml")));
    }

    // ------------------------------------------------------------------
    // AC-07: partial failures — transform abort retains earlier outputs
    // ------------------------------------------------------------------

    [Fact]
    public void Capture_TransformFailure_RetainsEarlierOutputsWithClassifiedFailure()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='kept.xml'><kept/></xsl:result-document>
    <xsl:message terminate='yes'>boom</xsl:message>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/base.xml",
        });

        Assert.Equal(XsltCaptureOutcome.Partial, result.Outcome);
        var retained = Assert.Single(result.SecondaryOutputs);
        Assert.Equal("file:///out/kept.xml", retained.ResolvedUri);
        Assert.Null(result.Principal);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(XsltCaptureFailureKind.TransformFailure, failure.Kind);
        Assert.Equal("XTMM9000", failure.Code);
    }

    [Fact]
    public void Capture_TransformFailureBeforeAnyOutput_IsFailed()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:message terminate='yes'>immediate</xsl:message>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/base.xml",
        });

        Assert.Equal(XsltCaptureOutcome.Failed, result.Outcome);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Principal);
        Assert.Empty(result.SecondaryOutputs);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(XsltCaptureFailureKind.TransformFailure, failure.Kind);
        Assert.Equal("XTMM9000", failure.Code);
    }

    // ------------------------------------------------------------------
    // AC-07: message floods — separate bounded channel
    // ------------------------------------------------------------------

    [Fact]
    public void Capture_MessageBoundExceeded_ClassifiedAndChannelStaysBounded()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:for-each select='1 to 5'>
      <xsl:message select='concat(""msg-"", .)'/>
    </xsl:for-each>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/base.xml",
            MaxMessages = 2,
        });

        Assert.Equal(XsltCaptureOutcome.Partial, result.Outcome);
        Assert.Equal(2, result.Messages.Count);
        Assert.Equal("msg-1", result.Messages[0]);
        Assert.Equal("msg-2", result.Messages[1]);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(XsltCaptureFailureKind.MessageCountLimitExceeded, failure.Kind);
        Assert.Equal("XV0010", failure.Code);
        // The transformation itself completed; only the message channel overflowed.
        Assert.NotNull(result.Principal);
    }

    [Fact]
    public void Capture_Messages_SeparateFromOutputBytes()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:message>channel-check</xsl:message>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source());

        Assert.True(result.IsSuccess, result.ToString());
        Assert.Equal(new[] { "channel-check" }, result.Messages);
        Assert.DoesNotContain("channel-check", Decode(result.Principal!));
    }

    // ------------------------------------------------------------------
    // AC-09: result state isolation
    // ------------------------------------------------------------------

    [Fact]
    public void Capture_RunToRun_StateIsIsolated()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='a.xml'><a/></xsl:result-document>
    <principal/>
  </xsl:template>
</xsl:stylesheet>");

        var options = new XsltCaptureOptions { BaseOutputUri = "file:///out/base.xml" };
        var first = executable.CaptureTransform(Source(), options);
        var second = executable.CaptureTransform(Source(), new XsltCaptureOptions { BaseOutputUri = "file:///out/base.xml" });

        Assert.True(first.IsSuccess, first.ToString());
        Assert.True(second.IsSuccess, second.ToString());
        Assert.Single(second.SecondaryOutputs);
        Assert.Empty(second.Failures);
        Assert.NotSame(first.SecondaryOutputs, second.SecondaryOutputs);
        Assert.True(((System.Collections.IList)first.SecondaryOutputs).IsReadOnly);
        Assert.True(((System.Collections.IList)first.Failures).IsReadOnly);
        Assert.True(((System.Collections.IList)first.Messages).IsReadOnly);
    }

    private static string MissingDriveRoot()
    {
        var used = System.IO.DriveInfo.GetDrives()
            .Select(d => char.ToUpperInvariant(d.Name[0]))
            .ToHashSet();
        for (char c = 'Z'; c >= 'C'; c--)
        {
            if (!used.Contains(c))
                return $"{c}:/";
        }

        // No free drive letter (practically unreachable): fall back to a guaranteed-absent
        // directory on the temp drive; the no-write assertions still hold for capture mode.
        var fallback = Path.Combine(Path.GetTempPath(), "bosak-capture-missing", Guid.NewGuid().ToString("N"));
        return fallback.Replace('\\', '/') + "/";
    }
}
