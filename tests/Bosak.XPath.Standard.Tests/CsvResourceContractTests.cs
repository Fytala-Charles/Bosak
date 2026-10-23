// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 October 2026
// PURPOSE              : Verifies local csv-doc resources, option defaults and error boundaries.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 23-10-2026     | Creation: public csv-doc URI, options, encoding and resource-error checks.               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies local csv-doc resources, option defaults and error boundaries.</summary>
public class CsvResourceContractTests
{
    /// <summary>Reads absolute and relative local resources with the declared options.</summary>
    /// <param name="relative">Whether the source is relative to the host base URI.</param>
    /// <param name="options">The optional second argument, including its comma.</param>
    /// <param name="expected">The complete row summary.</param>
    [Theory]
    [InlineData(false, "", "sku|qty;A|2;B|3")]
    [InlineData(true, "", "sku|qty;A|2;B|3")]
    [InlineData(false, ", ()", "sku|qty;A|2;B|3")]
    [InlineData(true, ", ()", "sku|qty;A|2;B|3")]
    [InlineData(false, ", map {}", "sku|qty;A|2;B|3")]
    [InlineData(true, ", map {}", "sku|qty;A|2;B|3")]
    [InlineData(false, ", map {'header':true()}", "A|2;B|3")]
    [InlineData(true, ", map {'header':true()}", "A|2;B|3")]
    [InlineData(false, ", map {'header':true(),'select-columns':(2,1)}", "2|A;3|B")]
    [InlineData(true, ", map {'header':true(),'select-columns':(2,1)}", "2|A;3|B")]
    [InlineData(false, ", map {'header':true(),'select-columns':()}", "A|2;B|3")]
    [InlineData(true, ", map {'header':true(),'select-columns':()}", "A|2;B|3")]
    public void LocalResource_UsesOptions(bool relative, string options, string expected)
    {
        WithResource("sku,qty\r\nA,2\r\nB,3\r\n", false, (uri, baseUri) =>
        {
            var source = relative ? "supplier.csv" : uri;
            Assert.Equal(expected, Evaluate(Summary($"csv-doc({Literal(source)}{options})"), baseUri));
        });
    }

    /// <summary>Handles a UTF-8 BOM and empty local files without treating empty text as no resource.</summary>
    /// <param name="text">The local resource content.</param>
    /// <param name="bom">Whether the resource carries a UTF-8 BOM.</param>
    /// <param name="expected">The complete row summary.</param>
    /// <param name="rowCount">The number of rows, distinguishing an empty file from a blank row.</param>
    [Theory]
    [InlineData("A,2", true, "A|2", 1)]
    [InlineData("", false, "", 0)]
    [InlineData("\n", false, "", 1)]
    [InlineData("A,\nB,3", false, "A|;B|3", 2)]
    public void LocalResource_EncodingAndEmptyRows(string text, bool bom, string expected, int rowCount)
    {
        WithResource(text, bom, (uri, baseUri) =>
        {
            Assert.Equal(expected, Evaluate(Summary($"csv-doc({Literal(uri)})"), baseUri));
            Assert.Equal(rowCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Evaluate($"count(csv-doc({Literal(uri)})?rows)", baseUri));
        });
    }

    /// <summary>Preserves parsing and option failures from a readable local resource.</summary>
    /// <param name="text">The resource content.</param>
    /// <param name="options">The optional second argument, including its comma.</param>
    /// <param name="error">The required error code.</param>
    [Theory]
    [InlineData("A,\"2", "", "FOCV0001")]
    [InlineData("\"A\"x,2", "", "FOCV0001")]
    [InlineData("A,2", ", map {'separator':'::'}", "FOCV0002")]
    [InlineData("A,2", ", map {'bogus':true()}", "XPTY0004")]
    [InlineData("", ", map {'bogus':true()}", "XPTY0004")]
    [InlineData("A,2", ", map {'trim-rows':'yes'}", "XPTY0004")]
    public void LocalResource_PreservesErrors(string text, string options, string error)
    {
        WithResource(text, false, (uri, baseUri) =>
        {
            var failure = Assert.ThrowsAny<Exception>(() => Evaluate($"csv-doc({Literal(uri)}{options})", baseUri));
            Assert.Contains(error, failure.Message);
        });
    }

    /// <summary>Rejects a missing local resource through the unparsed-text resource error path.</summary>
    [Fact]
    public void MissingResource_RaisesFout1170()
    {
        WithResource("A,2", false, (_, baseUri) =>
        {
            var failure = Assert.ThrowsAny<Exception>(() => Evaluate("csv-doc('missing.csv')", baseUri));
            Assert.Contains("FOUT1170", failure.Message);
        });
    }

    /// <summary>Uses the registered source/options keyword names with a real local resource.</summary>
    [Fact]
    public void Keywords_ReadLocalResource()
    {
        WithResource("sku;qty\nA;2", false, (uri, baseUri) =>
            Assert.Equal("A|2", Evaluate(Summary($"csv-doc(source := {Literal(uri)}, options := map {{'separator':';','header':true()}})"), baseUri)));
    }

    /// <summary>Distinguishes an absent source from a present empty file before option inspection.</summary>
    [Fact]
    public void EmptySource_BypassesOptionInspection()
        => Assert.Equal("true", Evaluate("empty(csv-doc((), map {'bogus':true()}))", "file:///csv-contract/host.xsl"));

    private static string Evaluate(string code, string baseUri)
        => XPath31Expression.Compile(code, new CompileOptions { Compatibility = XPathCompatibility.XPath40, BaseUri = baseUri })
            .Evaluate(new EvaluationContext { BaseUri = baseUri }).ToString();

    private static string Summary(string call)
        => $"string-join(for $row in ({call})?rows return string-join($row?*, '|'), ';')";

    private static string Literal(string text) => "'" + text.Replace("'", "''", StringComparison.Ordinal) + "'";

    private static void WithResource(string text, bool bom, Action<string, string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "BosakCsvContract-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string file = Path.Combine(directory, "supplier.csv");
        try
        {
            File.WriteAllText(file, text, new UTF8Encoding(bom));
            action(new Uri(file).AbsoluteUri, new Uri(Path.Combine(directory, "host.xsl")).AbsoluteUri);
        }
        finally
        {
            File.Delete(file);
            Directory.Delete(directory);
        }
    }
}
