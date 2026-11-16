// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 November 2026
// PURPOSE              : Verifies that shipped editor snippets expand into executable language examples.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-11-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 16-11-2026     | Use the public XDM document adapter for transformation inputs.                            |
//                      | Charles Korthout | 0.3   | 16-11-2026     | Expect standard spacing between adjacent atomic XSLT results.                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.XQuery.Api;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.LanguageServer.Tests;

/// <summary>Executes the default expansions of the shipped language snippets.</summary>
public class SnippetExpansionTests
{
    private static readonly Regex Tokens = new(@"\\[\\$}]|\$\{(?<id>\d+):(?<default>[^{}]*)\}|\$(?<mirror>\d+)");

    /// <summary>Reads each actual contributed snippet rather than a copied source fixture.</summary>
    /// <returns>The language, snippet name and body for each example.</returns>
    public static IEnumerable<object[]> Examples()
    {
        foreach (var language in new[] { "xslt", "xquery", "xpath" })
        {
            using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Snippets", language + ".json")));
            foreach (var snippet in json.RootElement.EnumerateObject())
                yield return new object[] { language, snippet.Name, string.Join("\n", snippet.Value.GetProperty("body").EnumerateArray().Select(x => x.GetString())) };
        }
    }

    /// <summary>Runs all default examples, adding the documented surrounding declaration context.</summary>
    /// <param name="language">The target language.</param>
    /// <param name="name">The snippet name.</param>
    /// <param name="body">The actual packaged snippet body.</param>
    [Theory]
    [MemberData(nameof(Examples))]
    public void DefaultExpansionExecutes(string language, string name, string body)
    {
        var source = Expand(body);
        if (language == "xpath")
        {
            var expression = name switch { "Map" => "(" + source + ")?key", "Array" => "(" + source + ")?2", _ => source };
            var expected = name switch { "For expression" => "1,2,3", "Array" => "2", _ => "1" };
            Assert.Equal(expected, XPath31Expression.Compile("string-join((" + expression + ") ! string(.), ',')").Evaluate(new EvaluationContext()).ToString());
            return;
        }
        if (language == "xquery")
        {
            var compiler = new XQueryCompiler();
            var query = name switch
            {
                "Library module" => "import module namespace example = 'urn:example'; example:identity('ok')",
                "Function" => source + "\nlocal:identity('ok')",
                "Variable" => source + "\n$value",
                "FLWOR" => "string-join((" + source + ") ! string(.), ',')",
                _ => source
            };
            if (name == "Library module") compiler.WithModule("urn:example", source);
            var expected = name switch { "Main module" => "2", "FLWOR" => "1,2,3", "Variable" => "1", _ => "ok" };
            Assert.Equal(expected, compiler.Compile(query).Evaluate(new XQueryContext()).ToString());
            return;
        }

        const string header = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:local='urn:local' exclude-result-prefixes='xs local'>";
        var declaration = name is "Match template" or "Named template" or "Function" or "Parameter" or "Variable" ? source : "";
        var instruction = name switch
        {
            "Match template" => "<xsl:apply-templates select='items/item'/>",
            "Named template" => "<xsl:call-template name='main'/>",
            "Function" => "<xsl:sequence select=\"local:identity('ok')\"/>",
            "Parameter" or "Variable" => "<xsl:sequence select='$value'/>",
            "For each group" => "<xsl:for-each select='items'>" + source + "</xsl:for-each>",
            _ => source
        };
        var stylesheet = name == "XSLT stylesheet" ? source : header + declaration + "<xsl:template match='/'><result>" + instruction + "</result></xsl:template></xsl:stylesheet>";
        var output = new XsltCompiler().Compile(stylesheet).TransformToString(new XDocumentNode(XDocument.Parse("<items><item category='a'>A</item><item category='a'>B</item></items>")));
        var result = XDocument.Parse(output).Root!;
        Assert.Equal("result", result.Name.LocalName);
        var expectedText = name switch { "Match template" or "For each" or "For each group" => "AB", "Function" => "ok", "Parameter" => "default", "Variable" => "1", "Iterate" => "1 2 3", _ => "" };
        Assert.Equal(expectedText, result.Value);
        if (name == "Choose") Assert.NotNull(result.Element("yes"));
        if (name == "Named template") Assert.NotNull(result.Element("result"));
        if (name == "For each group") Assert.Equal(2, result.Elements("item").Count());
    }

    /// <summary>Checks that editing a variable placeholder updates both binding and mirrored use.</summary>
    [Fact]
    public void EditedVariableAndSequencePreserveMirrorsAndLiteralDollars()
    {
        var body = (string)Examples().Single(x => (string)x[0] == "xpath" && (string)x[1] == "For expression")[2];
        var source = Expand(body, new Dictionary<string, string> { ["1"] = "entry", ["2"] = "7, 8" });
        Assert.Equal("for $entry in (7, 8) return $entry", source);
        Assert.Equal("7,8", XPath31Expression.Compile("string-join((" + source + ") ! string(.), ',')").Evaluate(new EvaluationContext()).ToString());
    }

    // Expands only the intentionally restricted snippet syntax used by this collection.
    // VS Code itself supplies interactive tab navigation; this is not an editor UI test.
    private static string Expand(string body, Dictionary<string, string>? edits = null)
    {
        var values = new Dictionary<string, string>();
        foreach (Match token in Tokens.Matches(body))
            if (token.Groups["id"].Success) values.Add(token.Groups["id"].Value, token.Groups["default"].Value);
        if (edits != null) foreach (var edit in edits) values[edit.Key] = edit.Value;
        return Tokens.Replace(body, token => token.Value[0] == '\\' ? token.Value[1..] : token.Groups["id"].Success ? values[token.Groups["id"].Value] : token.Groups["mirror"].Value == "0" ? "" : values[token.Groups["mirror"].Value]);
    }
}
