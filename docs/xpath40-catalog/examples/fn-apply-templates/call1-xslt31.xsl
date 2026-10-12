<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify invoice template dispatch and the deferred fn:apply-templates boundary.
SPECIAL NOTES: In-memory catalog example; no streaming or draft function conformance claim.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: invoice batch. Output: exact text report or declared refusal marker.
Empty input returns an empty report; reordered input retains selection order.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:f="urn:invoice" version="3.0" exclude-result-prefixes="xs f">
  <xsl:output method="text"/>
  <xsl:template match="/">
    <xsl:sequence xmlns:fn="http://www.w3.org/2005/xpath-functions" select="fn:apply-templates(invoices/invoice)"/>
  </xsl:template>
</xsl:stylesheet>
