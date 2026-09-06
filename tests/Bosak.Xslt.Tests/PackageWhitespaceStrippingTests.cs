// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 September 2026
// PURPOSE              : Unit tests verifying per-package whitespace stripping of documents loaded via fn:document/fn:doc (XSLT 3.0 §2.13.4).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Regression tests for XSLT 3.0 §2.13.4: whitespace stripping of a document loaded by
/// fn:document/fn:doc applies the strip/preserve rules of the package whose code performs
/// the call. The same URI loaded under conflicting rules must yield distinct trees
/// (W3C tests document-2401/document-2402/collection-006).
/// </summary>
public class PackageWhitespaceStrippingTests
{
    private const string PackageUri = "urn:test:package-ws";
    private const string PackageVersion = "1.0";
    private const string PackageLocation = "urn:test:package-ws:1.0";

    // Mirrors doc14.xml from the W3C suite: four whitespace-only text nodes under the root.
    private const string LoadedDocument = "<doc>\n<a> </a>\n<b> </b>\n<c> </c>\n</doc>";

    private static string WriteTempDocument()
    {
        var path = Path.Combine(Path.GetTempPath(), "bosak-ws-" + Guid.NewGuid().ToString("N") + ".xml");
        File.WriteAllText(path, LoadedDocument);
        return new Uri(path).AbsoluteUri;
    }

    private sealed class RegistryUriResolver : Api.IXsltUriResolver
    {
        private readonly Dictionary<string, string> _documents = new();
        public void Add(string uri, string content) => _documents[uri] = content;
        public XDocument Resolve(string href, string? baseUri) =>
            XDocument.Parse(_documents[href], LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
    }

    private static string CompileAndRun(string principalXsl, string docUri, RegistryUriResolver resolver)
    {
        Api.XsltFunctionLibrary.ClearPackages();
        Api.XsltFunctionLibrary.RegisterPackage(PackageUri, PackageVersion, PackageLocation);

        var package = $@"<xsl:package name='{PackageUri}' package-version='{PackageVersion}' version='3.0'
            xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:strip-space elements=''/>
            <xsl:template name='a' visibility='public'>
                <unstripped>
                    <xsl:value-of select=""count(document('{docUri}')/*/text())""/>
                </unstripped>
            </xsl:template>
            <xsl:function name='p:count-doc' visibility='public'
                xmlns:p='urn:test:package-ws'>
                <xsl:sequence select=""count(document('{docUri}')/*/text())""/>
            </xsl:function>
            <xsl:variable name='p:doc-text-count' visibility='public'
                xmlns:p='urn:test:package-ws'
                select=""count(document('{docUri}')/*/text())""/>
        </xsl:package>";

        resolver.Add(PackageLocation, package);

        var compiler = new Api.XsltCompiler { UriResolver = resolver };
        var executable = compiler.Compile(principalXsl);
        return executable.TransformToString(new XDocumentNode(new XDocument(new XElement("root"))));
    }

    [Fact]
    public void Document_Function_In_OverridingAndOverridden_Templates_Uses_Caller_Package_Rules()
    {
        var docUri = WriteTempDocument();
        // Principal strips everything; the used package strips nothing. The overriding
        // template (principal code) must see the stripped tree, xsl:original (package
        // code) the unstripped tree (W3C document-2401).
        var xsl = $@"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:strip-space elements='*'/>
            <xsl:use-package name='{PackageUri}' package-version='{PackageVersion}'>
                <xsl:override>
                    <xsl:template name='a' visibility='public'>
                        <out>
                            <stripped>
                                <xsl:value-of select=""count(document('{docUri}')/*/text())""/>
                            </stripped>
                            <xsl:call-template name='xsl:original'/>
                        </out>
                    </xsl:template>
                </xsl:override>
            </xsl:use-package>
            <xsl:template match='/'>
                <xsl:call-template name='a'/>
            </xsl:template>
        </xsl:stylesheet>";

        var result = CompileAndRun(xsl, docUri, new RegistryUriResolver());

        Assert.Contains("<stripped>0</stripped>", result);
        Assert.Contains("<unstripped>4</unstripped>", result);
    }

    [Fact]
    public void Doc_Function_Loads_Distinct_Trees_Per_Calling_Package()
    {
        var docUri = WriteTempDocument();
        var xsl = $@"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:p='urn:test:package-ws'
            exclude-result-prefixes='xs p'>
            <xsl:strip-space elements='*'/>
            <xsl:use-package name='{PackageUri}' package-version='{PackageVersion}'/>
            <xsl:template match='/'>
                <out>
                    <principal>
                        <xsl:value-of select=""count(doc('{docUri}')/*/text())""/>
                    </principal>
                    <pack-func>
                        <xsl:value-of select='p:count-doc()'/>
                    </pack-func>
                    <pack-global>
                        <xsl:value-of select='$p:doc-text-count'/>
                    </pack-global>
                </out>
            </xsl:template>
        </xsl:stylesheet>";

        var result = CompileAndRun(xsl, docUri, new RegistryUriResolver());

        Assert.Contains("<principal>0</principal>", result);
        Assert.Contains("<pack-func>4</pack-func>", result);
        Assert.Contains("<pack-global>4</pack-global>", result);
    }

    [Fact]
    public void Principal_Caller_Keeps_Unstripped_View_Of_Loaded_Document()
    {
        var docUri = WriteTempDocument();
        // Reverse configuration of document-2401: the principal preserves whitespace
        // while the used package strips. Principal code must see all four text nodes.
        var xsl = $@"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
            xmlns:p='urn:test:package-ws' exclude-result-prefixes='p'>
            <xsl:strip-space elements=''/>
            <xsl:use-package name='{PackageUri}' package-version='{PackageVersion}'/>
            <xsl:template match='/'>
                <out>
                    <principal>
                        <xsl:value-of select=""count(document('{docUri}')/*/text())""/>
                    </principal>
                </out>
            </xsl:template>
        </xsl:stylesheet>";

        var result = CompileAndRun(xsl, docUri, new RegistryUriResolver());

        Assert.Contains("<principal>4</principal>", result);
    }
}
