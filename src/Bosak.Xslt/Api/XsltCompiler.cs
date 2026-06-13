// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 mei 2026
// PURPOSE              : Entry point for compiling XSLT stylesheets into executable transform objects.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 24-05-2026     | Added IXsltUriResolver support for xsl:import/xsl:include                              |
//                      | Charles Korthout | 0.3   | 31-05-2026     | Added IXsltMessageListener support for xsl:message                                      |
//                      | Charles Korthout | 0.4   | 11-06-2026     | Resolve external DTDs when compiling stylesheets from strings                           |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;

namespace Bosak.Xslt.Api;

/// <summary>
/// Compiles XSLT 2.0/3.0 stylesheets into <see cref="XsltExecutable"/> instances.
/// </summary>
public sealed class XsltCompiler
{
    /// <summary>
    /// Optional URI resolver for xsl:import and xsl:include. Defaults to <see cref="FileSystemUriResolver"/>.
    /// </summary>
    public IXsltUriResolver? UriResolver { get; set; }

    /// <summary>
    /// Optional listener for xsl:message output. Defaults to <see cref="ConsoleMessageListener"/>.
    /// </summary>
    public IXsltMessageListener? MessageListener { get; set; }

    /// <summary>
    /// Compiles an XSLT stylesheet from an XML string.
    /// </summary>
    /// <param name="xsl">The XSLT stylesheet as an XML string.</param>
    /// <param name="baseUri">Optional base URI for resolving xsl:import and xsl:include.</param>
    /// <returns>An executable stylesheet.</returns>
    public XsltExecutable Compile(string xsl, string? baseUri = null)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = new XmlUrlResolver(),
        };
        using var reader = XmlReader.Create(new StringReader(xsl), settings, baseUri ?? "");
        var doc = XDocument.Load(reader, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        return Compile(doc, baseUri);
    }

    /// <summary>
    /// Compiles an XSLT stylesheet from an <see cref="XDocument"/>.
    /// </summary>
    /// <param name="document">The parsed XSLT stylesheet.</param>
    /// <param name="baseUri">Optional base URI for resolving xsl:import and xsl:include.</param>
    /// <returns>An executable stylesheet.</returns>
    public XsltExecutable Compile(XDocument document, string? baseUri = null)
    {
        var resolver = UriResolver ?? new FileSystemUriResolver();
        var stylesheet = new Stylesheet.Stylesheet(document, baseUri, resolver);
        return new XsltExecutable(stylesheet, MessageListener);
    }

    /// <summary>
    /// Compiles an XSLT stylesheet from an XDM node.
    /// </summary>
    /// <param name="node">The stylesheet node.</param>
    /// <param name="baseUri">Optional base URI for resolving xsl:import and xsl:include.</param>
    /// <returns>An executable stylesheet.</returns>
    public XsltExecutable Compile(IXdmNode node, string? baseUri = null)
    {
        // TODO: Convert IXdmNode back to XDocument for parsing
        throw new NotImplementedException("Compile from IXdmNode is not yet implemented.");
    }
}
