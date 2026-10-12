<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify invoice processing through ordinary and streaming XSLT paths.
SPECIAL NOTES: Exact report parity does not certify Streaming 4.0 or memory bounds.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: invoice XML. Output: one currency-labelled row per invoice, or the declared error.
Empty batches return empty text; reordered batches retain input order.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" version="4.0" exclude-result-prefixes="xs">
  <xsl:output method="text"/>
  <xsl:template match="/">
    <!-- A forward pass reports every invoice; sorting is a separate buffered task. -->
    <xsl:for-each select="invoices/invoice">
      <xsl:value-of select="concat(@id, '/', @currency, ':', format-number(xs:decimal(@total), '0.00'), ';')"/>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
