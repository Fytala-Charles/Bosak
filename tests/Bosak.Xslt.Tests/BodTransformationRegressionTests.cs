// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 September 2026
// PURPOSE              : Regression tests for the Stan BOD→BOD transformation path (xsl:function library, xsl:analyze-string, format-dateTime).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 07-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Generic;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Regression coverage for the Stan project's BOD→BOD transformation, reported unblocked
/// on 2026-09-07 (REQ-015 decision log). Mirrors the consuming architecture: a shared
/// <c>xsl:function</c> helper library pulled in via <c>xsl:include</c>, with EDI date
/// parsing through <c>xsl:analyze-string</c>/<c>regex-group()</c> and output formatting
/// through <c>fn:format-date</c>/<c>fn:format-dateTime</c>, applied to OAGIS-shaped BODs.
/// </summary>
public class BodTransformationRegressionTests
{
    private const string Oagis = "http://www.openapplications.org/oagis/9";
    private const string LibraryUri = "urn:test:bod-function-library";

    /// <summary>
    /// Stand-in for the shared DateFunctions.xsl fragment library: EDI D99A date parsing
    /// (qualifier 102 = CCYYMMDD) and ISO week computation.
    /// </summary>
    private const string FunctionLibrary = @"<xsl:stylesheet version='3.0'
        xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
        xmlns:xs='http://www.w3.org/2001/XMLSchema'
        xmlns:app='http://fytala.com/bosak/test/app'>

        <xsl:function name='app:parse-edidate' as='xs:date?'>
            <xsl:param name='raw' as='xs:string?'/>
            <xsl:param name='qualifier' as='xs:string'/>
            <xsl:analyze-string select='$raw' regex='^(\d{{4}})(\d{{2}})(\d{{2}})$'>
                <xsl:matching-substring>
                    <xsl:sequence select=""xs:date(concat(regex-group(1), '-', regex-group(2), '-', regex-group(3)))""/>
                </xsl:matching-substring>
                <xsl:non-matching-substring>
                    <xsl:sequence select='()'/>
                </xsl:non-matching-substring>
            </xsl:analyze-string>
        </xsl:function>

        <xsl:function name='app:iso-week' as='xs:string'>
            <xsl:param name='date' as='xs:date?'/>
            <xsl:sequence select=""if (empty($date)) then '' else format-date($date, '[Y0001]-[W01]')""/>
        </xsl:function>

        <xsl:function name='app:compact-datetime' as='xs:string'>
            <xsl:param name='dt' as='xs:string'/>
            <xsl:sequence select=""format-dateTime(xs:dateTime($dt), '[Y0001][M01][D01][H01][m01][s01]')""/>
        </xsl:function>
    </xsl:stylesheet>";

    private const string Basesheet = @"<xsl:stylesheet version='3.0'
        xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
        xmlns:xs='http://www.w3.org/2001/XMLSchema'
        xmlns:app='http://fytala.com/bosak/test/app'
        xpath-default-namespace='http://www.openapplications.org/oagis/9'
        exclude-result-prefixes='xs app'>

        <xsl:include href='urn:test:bod-function-library'/>
        <xsl:output method='xml' indent='no'/>

        <xsl:template match='/SyncPurchaseOrder'>
            <ProcessShipment xmlns='http://www.openapplications.org/oagis/9'>
                <ApplicationArea>
                    <CreationDateTime><xsl:value-of select='app:compact-datetime(ApplicationArea/CreationDateTime)'/></CreationDateTime>
                </ApplicationArea>
                <DataArea>
                    <Shipment>
                        <ShipmentHeader>
                            <DocumentID><ID><xsl:value-of select='DataArea/PurchaseOrder/PurchaseOrderHeader/DocumentID/ID'/></ID></DocumentID>
                            <DocumentDate><xsl:value-of select=""format-dateTime(xs:dateTime(DataArea/PurchaseOrder/PurchaseOrderHeader/DocumentDateTime), '[Y0001]-[M01]-[D01]')""/></DocumentDate>
                            <RequestedDeliveryWeek><xsl:value-of select=""app:iso-week(app:parse-edidate(DataArea/PurchaseOrder/PurchaseOrderHeader/RequestedDeliveryDate, '102'))""/></RequestedDeliveryWeek>
                        </ShipmentHeader>
                    </Shipment>
                </DataArea>
            </ProcessShipment>
        </xsl:template>
    </xsl:stylesheet>";

    private sealed class RegistryUriResolver : IXsltUriResolver
    {
        private readonly Dictionary<string, string> _documents = new();
        public void Add(string uri, string content) => _documents[uri] = content;
        public XDocument Resolve(string href, string? baseUri) =>
            XDocument.Parse(_documents[href], LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
    }

    private static string Transform(string sourceXml)
    {
        var resolver = new RegistryUriResolver();
        resolver.Add(LibraryUri, FunctionLibrary);
        var compiler = new XsltCompiler { UriResolver = resolver };
        var executable = compiler.Compile(Basesheet);
        return executable.TransformToString(new XDocumentNode(XDocument.Parse(sourceXml)));
    }

    [Fact]
    public void BodToBod_EdiDatesParsedAndReformatted()
    {
        const string source = @"<SyncPurchaseOrder xmlns='http://www.openapplications.org/oagis/9' releaseID='9.2'>
            <ApplicationArea>
                <CreationDateTime>2026-09-07T13:45:22Z</CreationDateTime>
            </ApplicationArea>
            <DataArea>
                <PurchaseOrder>
                    <PurchaseOrderHeader>
                        <DocumentID><ID>PO-1042</ID></DocumentID>
                        <DocumentDateTime>2026-09-05T08:30:00Z</DocumentDateTime>
                        <RequestedDeliveryDate>20260912</RequestedDeliveryDate>
                    </PurchaseOrderHeader>
                </PurchaseOrder>
            </DataArea>
        </SyncPurchaseOrder>";

        var result = Transform(source);

        // format-dateTime compaction of the BOD creation timestamp
        Assert.Contains("<CreationDateTime>20260907134522</CreationDateTime>", result);
        // format-dateTime date-only projection of the order date
        Assert.Contains("<DocumentDate>2026-09-05</DocumentDate>", result);
        // xsl:analyze-string parse of the EDI CCYYMMDD delivery date, then ISO week 37
        Assert.Contains("<RequestedDeliveryWeek>2026-37</RequestedDeliveryWeek>", result);
        Assert.Contains("<ID>PO-1042</ID>", result);
    }

    [Fact]
    public void BodToBod_UnparseableEdiDate_YieldsEmptyWeek()
    {
        const string source = @"<SyncPurchaseOrder xmlns='http://www.openapplications.org/oagis/9'>
            <ApplicationArea>
                <CreationDateTime>2026-09-07T00:00:00Z</CreationDateTime>
            </ApplicationArea>
            <DataArea>
                <PurchaseOrder>
                    <PurchaseOrderHeader>
                        <DocumentID><ID>PO-2048</ID></DocumentID>
                        <DocumentDateTime>2026-09-05T08:30:00Z</DocumentDateTime>
                        <RequestedDeliveryDate>N/A</RequestedDeliveryDate>
                    </PurchaseOrderHeader>
                </PurchaseOrder>
            </DataArea>
        </SyncPurchaseOrder>";

        var result = Transform(source);

        // The non-matching branch yields the empty sequence; the week renders empty, no error.
        Assert.True(
            result.Contains("<RequestedDeliveryWeek/>") || result.Contains("<RequestedDeliveryWeek></RequestedDeliveryWeek>"),
            $"Expected empty RequestedDeliveryWeek, got: {result}");
    }
}
