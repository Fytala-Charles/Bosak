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
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:f="urn:invoice" version="4.0" exclude-result-prefixes="xs f">
  <xsl:output method="text"/>
  <!-- Explicit export mode keeps dispatch independent of the caller's mode. -->
  <xsl:mode name="f:export"/>
  <xsl:template match="invoice" mode="f:export">
    <xsl:param name="label" as="xs:string"/>
    <xsl:param name="batch" as="xs:string" tunnel="yes"/>
    <xsl:sequence select="concat($batch, '/', $label, '/', @id, '/', @currency, ':', format-number(sum(line ! (xs:decimal(@quantity) * xs:decimal(@price))), '0.00'), ';')"/>
  </xsl:template>
  <xsl:template match="/">
    <xsl:variable name="rows" as="xs:string*">
      <xsl:apply-templates select="invoices/invoice" mode="f:export">
        <xsl:with-param name="label" select="'EXPORT'"/>
        <xsl:with-param name="batch" select="'BATCH-7'" tunnel="yes"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:value-of select="string-join($rows, '')"/>
  </xsl:template>
</xsl:stylesheet>
