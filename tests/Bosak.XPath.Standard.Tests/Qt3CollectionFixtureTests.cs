// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 08 November 2026
// PURPOSE              : Regression tests for QT3 collection fixture file URI registration.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 08-11-2026     | Creation: cross-platform QT3 fixture transport                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.XPath.Conformance;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies collection file fixtures stay local under an HTTP static base URI.</summary>
public sealed class Qt3CollectionFixtureTests
{
    /// <summary>Known file fixtures must become escaped file URIs for named and default collections.</summary>
    /// <param name="collectionUri">The catalog collection identifier.</param>
    [Theory]
    [InlineData("")]
    [InlineData("https://example.test/collection")]
    public void FileFixtureUsesExplicitFileUri(string collectionUri)
    {
        string directory = Path.Combine(Path.GetTempPath(), "Bosak QT3 " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string file = Path.Combine(directory, "bib caf\u00e9 % #.xml");
        File.WriteAllText(file, "<bib/>");
        try
        {
            XNamespace ns = "http://www.w3.org/2010/09/qt-fots-catalog";
            var element = new XElement(ns + "environment",
                new XElement(ns + "static-base-uri", new XAttribute("uri", "http://www.w3.org/catalog/")),
                new XElement(ns + "collection", new XAttribute("uri", collectionUri),
                    new XElement(ns + "source", new XAttribute("file", Path.GetFileName(file)))));
            var environment = TestEnvironment.FromElement(element, directory, directory);
            string member = Assert.Single(environment.Collections[collectionUri]);
            Assert.StartsWith("file:", member);
            Assert.Equal(file, new Uri(member).LocalPath);
            Assert.Contains("%23", member);
            Assert.Contains("%25", member);
            var context = new EvaluationContext { BaseUri = "http://www.w3.org/catalog/" };
            string? loaded = null;
            context.DocumentLoader = uri =>
            {
                loaded = uri;
                return Bosak.XPath.Providers.Xml.XDocumentProvider.LoadFile(new Uri(uri).LocalPath);
            };
            var document = context.LoadDocument(member);
            Assert.NotNull(document);
            Assert.Equal(member, loaded);
        }
        finally
        {
            File.Delete(file);
            Directory.Delete(directory);
        }
    }

    /// <summary>URI-valued members retain URI-reference semantics and missing files stay absent.</summary>
    [Fact]
    public void UriMemberIsUnchangedAndMissingFileIsNotRegistered()
    {
        XNamespace ns = "http://www.w3.org/2010/09/qt-fots-catalog";
        string missing = "missing-" + Guid.NewGuid().ToString("N") + ".xml";
        var element = new XElement(ns + "environment",
            new XElement(ns + "collection", new XAttribute("uri", "urn:uris"),
                new XElement(ns + "source", new XAttribute("uri", "/remote/bib.xml"))),
            new XElement(ns + "collection", new XAttribute("uri", "urn:missing"),
                new XElement(ns + "source", new XAttribute("file", missing))));
        var environment = TestEnvironment.FromElement(element, Path.GetTempPath(), Path.GetTempPath());
        Assert.Equal("/remote/bib.xml", Assert.Single(environment.Collections["urn:uris"]));
        Assert.False(environment.Collections.ContainsKey("urn:missing"));
    }
}
