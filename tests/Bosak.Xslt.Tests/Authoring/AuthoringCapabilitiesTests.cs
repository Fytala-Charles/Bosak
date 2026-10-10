// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Contract tests for the AuthoringCapabilities descriptor (AC-11 capability advertisement).
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

public class AuthoringCapabilitiesTests
{
    [Fact]
    public void Capabilities_AdvertiseTheRatifiedSurface()
    {
        Assert.True(AuthoringCapabilities.SupportsInspection);
        Assert.True(AuthoringCapabilities.SupportsExpressionSlotReplacement);
        Assert.False(AuthoringCapabilities.SupportsPatternSlotReplacement);
        Assert.False(AuthoringCapabilities.SupportsAvtSlotReplacement);
        Assert.False(AuthoringCapabilities.SupportsQNameSlotReplacement);

        var fidelity = Assert.Single(AuthoringCapabilities.FidelityModes);
        Assert.Equal(AuthoringFidelityMode.LosslessByteExport, fidelity);

        Assert.Equal(256, AuthoringCapabilities.MaxModuleCount);
        Assert.Equal(256, new AuthoringInspectionOptions().MaxModuleCount);
        Assert.NotEmpty(AuthoringCapabilities.DocumentedLimits);
        Assert.Contains("XSLT 3.0", AuthoringCapabilities.VersionPolicy, StringComparison.Ordinal);
        Assert.False(string.IsNullOrEmpty(AuthoringCapabilities.EngineVersion));
        Assert.False(string.IsNullOrEmpty(AuthoringCapabilities.EngineAssemblyName));
    }

    [Fact]
    public void SupportedEncodings_MatchTryCreate_AcceptanceAndRefusal()
    {
        // The advertised list is exactly the seven canonical encodings detection accepts...
        Assert.Equal(
            new[] { "UTF-8", "UTF-16LE", "UTF-16BE", "UTF-32LE", "UTF-32BE", "ISO-8859-1", "US-ASCII" },
            AuthoringCapabilities.SupportedEncodings);

        // ...and every advertised encoding is actually accepted via a declaration (edge case:
        // "utf-16"/"utf-32" names mean LE), while an unadvertised name is refused as data.
        var declared = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["UTF-8"] = "utf-8",
            ["UTF-16LE"] = "utf-16",
            ["UTF-16BE"] = "utf-16be",
            ["UTF-32LE"] = "utf-32",
            ["UTF-32BE"] = "utf-32be",
            ["ISO-8859-1"] = "iso-8859-1",
            ["US-ASCII"] = "us-ascii",
        };

        foreach (var (canonical, declarationName) in declared)
        {
            var text = $"<?xml version=\"1.0\" encoding=\"{declarationName}\"?><r/>";
            Assert.True(
                AuthoringSource.TryCreate(Encode(canonical, text), new Uri($"file:///C:/p/{canonical}.xml"), out _, out var failure),
                $"{canonical}: {failure}");
        }

        var shiftJis = Encoding.ASCII.GetBytes("<?xml version=\"1.0\" encoding=\"Shift_JIS\"?><r/>");
        Assert.False(AuthoringSource.TryCreate(shiftJis, new Uri("file:///C:/p/x.xml"), out _, out var refused));
        Assert.Equal(AuthoringFailureKind.UnsupportedEncoding, refused!.Kind);
    }

    // UTF-16/UTF-32 are only detectable via byte-order mark, so those fixtures carry a BOM;
    // single-byte encodings are exercised through their declaration, as documented.
    private static byte[] Encode(string canonical, string text)
    {
        Encoding encoding = canonical switch
        {
            "UTF-16LE" => new UnicodeEncoding(false, true, true),
            "UTF-16BE" => new UnicodeEncoding(true, true, true),
            "UTF-32LE" => new UTF32Encoding(false, true, true),
            "UTF-32BE" => new UTF32Encoding(true, true, true),
            "ISO-8859-1" => Encoding.Latin1,
            _ => new UTF8Encoding(false, true),
        };

        return encoding.GetPreamble().Concat(encoding.GetBytes(text)).ToArray();
    }
}
