// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 October 2026
// PURPOSE              : Unit tests for xsl:mode same-precedence conflict detection and resolution (XTSE0545)
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 01-10-2026     | Creation (mode-1505/1506/1903 semantics)                                                 |
//                      |==================|=======|================|=========================================================================================
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for XSLT 3.0 §6.6.1 mode-declaration conflict rules: two xsl:mode declarations
/// at the same import precedence that explicitly supply different values for the same
/// attribute are XTSE0545, unless a higher-import-precedence declaration explicitly
/// specifies that attribute (W3C mode-1505 resolves, mode-1506 fails).
/// </summary>
public class ModeConflictResolutionTests
{
    private const string XslNs = "http://www.w3.org/1999/XSL/Transform";

    /// <summary>An in-memory URI resolver serving stylesheet modules by href.</summary>
    private sealed class MemoryResolver : IXsltUriResolver
    {
        private readonly IReadOnlyDictionary<string, System.Xml.Linq.XDocument> _modules;

        internal MemoryResolver(IReadOnlyDictionary<string, System.Xml.Linq.XDocument> modules)
            => _modules = modules;

        public System.Xml.Linq.XDocument Resolve(string href, string? baseUri)
            => _modules.TryGetValue(href, out var doc)
                ? doc
                : throw new InvalidOperationException($"Unresolved href '{href}'");
    }

    private static XsltCompiler CompilerWith(params (string Href, string Content)[] modules)
    {
        var compiler = new XsltCompiler
        {
            UriResolver = new MemoryResolver(modules.ToDictionary(m => m.Href, m => System.Xml.Linq.XDocument.Parse(m.Content)))
        };
        return compiler;
    }

    private const string ImportedA = $"""<xsl:stylesheet version="3.0" xmlns:xsl="{XslNs}"><xsl:mode streamable="no" on-no-match="deep-skip"/></xsl:stylesheet>""";
    private const string ImportedB = $"""<xsl:stylesheet version="3.0" xmlns:xsl="{XslNs}"><xsl:mode streamable="yes" on-no-match="deep-skip"/></xsl:stylesheet>""";
    private const string ImportedC = $"""<xsl:stylesheet version="3.0" xmlns:xsl="{XslNs}"><xsl:mode on-no-match="deep-skip"/></xsl:stylesheet>""";

    [Fact]
    public void SamePrecedenceConflict_UnresolvedByHigherPrecedence_ThrowsXtse0545()
    {
        // mode-1506 shape: the higher-precedence declaration specifies only
        // on-no-match, so the streamable conflict below it is NOT resolved.
        const string principal = $"""
            <xsl:stylesheet version="3.0" xmlns:xsl="{XslNs}">
              <xsl:import href="a.xsl"/>
              <xsl:import href="b.xsl"/>
              <xsl:mode on-no-match="deep-copy"/>
              <xsl:template name="main"><out/></xsl:template>
            </xsl:stylesheet>
            """;
        var compiler = CompilerWith(("a.xsl", ImportedA), ("b.xsl", ImportedB));
        var ex = Assert.Throws<InvalidOperationException>(() => compiler.Compile(System.Xml.Linq.XDocument.Parse(principal), "file:///main.xsl"));
        Assert.StartsWith("XTSE0545", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SamePrecedenceConflict_ResolvedByHigherPrecedenceAttribute_Compiles()
    {
        // mode-1505 shape: the higher-precedence declaration explicitly specifies
        // streamable, resolving the conflict between the imported declarations.
        const string principal = $"""
            <xsl:stylesheet version="3.0" xmlns:xsl="{XslNs}">
              <xsl:import href="a.xsl"/>
              <xsl:import href="b.xsl"/>
              <xsl:mode streamable="no" on-no-match="deep-copy"/>
              <xsl:template name="main"><out/></xsl:template>
            </xsl:stylesheet>
            """;
        var compiler = CompilerWith(("a.xsl", ImportedA), ("b.xsl", ImportedB));
        compiler.Compile(System.Xml.Linq.XDocument.Parse(principal), "file:///main.xsl");
    }

    [Fact]
    public void SamePrecedenceDisjointAttributes_MergeWithoutError()
    {
        // mode-1903 shape: same precedence, different attributes — no conflict.
        const string principal = $"""
            <xsl:stylesheet version="3.0" xmlns:xsl="{XslNs}">
              <xsl:import href="c.xsl"/>
              <xsl:import href="a.xsl"/>
              <xsl:template name="main"><out/></xsl:template>
            </xsl:stylesheet>
            """;
        var compiler = CompilerWith(("a.xsl", ImportedA), ("c.xsl", ImportedC));
        compiler.Compile(System.Xml.Linq.XDocument.Parse(principal), "file:///main.xsl");
    }

    [Fact]
    public void SamePrecedenceSameValue_NoConflict()
    {
        const string principal = $"""
            <xsl:stylesheet version="3.0" xmlns:xsl="{XslNs}">
              <xsl:import href="c1.xsl"/>
              <xsl:import href="c2.xsl"/>
              <xsl:template name="main"><out/></xsl:template>
            </xsl:stylesheet>
            """;
        var compiler = CompilerWith(("c1.xsl", ImportedC), ("c2.xsl", ImportedC));
        compiler.Compile(System.Xml.Linq.XDocument.Parse(principal), "file:///main.xsl");
    }
}
