// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 November 2026
// PURPOSE              : Tests specification-mapped namespace assessment through the public API.
// SPECIAL NOTES        : Bounded native decimal profile; published Core 1.81.2; no general XSD conformance claim.
//
// COPYRIGHT            : Fytala
// LICENSE              : LICENSE.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text.Json;
using System.Xml.Linq;
using Bosak.Schema;
using Xunit;

namespace Bosak.Schema.PublicConsumer.Tests;

/// <summary>Exercises authored specification-mapped namespace cases through public APIs.</summary>
public sealed class NativeNamespaceConsumerTests
{
    /// <summary>Gets immutable authored cases with stable IDs independent of runner display truncation.</summary>
    /// <returns>Case identity and JSON payload pairs.</returns>
    public static IEnumerable<object[]> Cases()
    {
        using var input = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"TestData","Namespaces","m1-cases.json")));
        foreach (var row in input.RootElement.EnumerateArray()) yield return [row.GetProperty("id").GetString()!, row.GetRawText()];
    }

    /// <summary>Separates valid, invalid and unsupported results without dropping namespace identity.</summary>
    /// <param name="id">Stable authored normative regression ID.</param>
    /// <param name="payload">Schema, instance and explicit expected outcomes.</param>
    [Theory]
    [MemberData(nameof(Cases))]
    public void NamespaceAssessment_MappedCase_PreservesCompleteOutcome(string id, string payload)
    {
        Assert.StartsWith("M1-",id);
        using var data=JsonDocument.Parse(payload);var row=data.RootElement;
        var options=new NativeDecimalSessionOptions(256,256,1,4,1,256,256);
        var created=NativeDecimalSession.Create(row.GetProperty("schema").GetString()!,options);
        Assert.Equal(row.GetProperty("expectedCompile").GetString(),created.Outcome.ToString());
        if(created.Outcome!=NativeDecimalOutcome.Valid)
        {
            Assert.Null(created.Value);Assert.Equal("BSK-NATIVE-SCHEMA-UNSUPPORTED",Assert.Single(created.Diagnostics).Code);
            Assert.Equal(row.GetProperty("unsupportedReason").GetString(),created.Diagnostics[0].UnsupportedReason.ToString());return;
        }
        using var session=created.Value!;
        string xml=row.GetProperty("instance").GetString()!;
        var result=session.Assess(xml,new NativeDecimalAssessmentContext(new DateTimeOffset(2026,11,19,0,0,0,TimeSpan.Zero),0));
        Assert.Equal(row.GetProperty("expectedAssessment").GetString(),result.Outcome.ToString());
        if(result.Outcome!=NativeDecimalOutcome.Valid)
        {
            Assert.Null(result.Value);Assert.Single(result.Diagnostics);
            Assert.Equal("BSK-NATIVE-INSTANCE-"+result.Outcome.ToString().ToUpperInvariant(),result.Diagnostics[0].Code);
            if(result.Outcome==NativeDecimalOutcome.Unsupported) Assert.Equal(row.GetProperty("unsupportedReason").GetString(),result.Diagnostics[0].UnsupportedReason.ToString());
            return;
        }
        Assert.Empty(result.Diagnostics);using var document=result.Value!;
        var values=row.GetProperty("values").EnumerateArray().Select(x=>x.GetString()).ToArray();
        Assert.Equal(values.Length,document.DecimalCount);
        for(int i=0;i<values.Length;i++)Assert.Equal(values[i],document.ReadDecimal(i).ToInvariantString(256));
        var detached=document.ExportXml();Assert.True(XNode.DeepEquals(XDocument.Parse(xml).Root,XDocument.Parse(detached).Root));
        session.Dispose();Assert.Throws<ObjectDisposedException>(()=>document.DecimalCount);
        Assert.NotNull(XDocument.Parse(detached).Root);
    }
}
