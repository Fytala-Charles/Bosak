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
  <!-- Explicit export mode keeps dispatch independent of the caller's mode. -->
  <xsl:mode name="f:export"/>
  <xsl:template match="invoice" mode="f:export">
    <xsl:param name="label" as="xs:string"/>
    <xsl:param name="batch" as="xs:string" tunnel="yes"/>
    <xsl:sequence select="concat($batch, '/', $label, '/', @id, '/', @currency, ':', format-number(sum(line ! (xs:decimal(@quantity) * xs:decimal(@price))), '0.00'), ';')"/>
  </xsl:template>
  <!-- The wrapper is a user function, not registration of the draft fn function. -->
  <xsl:function name="f:export" as="xs:string*">
    <xsl:param name="nodes" as="element(invoice)*"/>
    <xsl:param name="label" as="xs:string"/>
    <xsl:param name="batch" as="xs:string"/>
    <xsl:apply-templates select="$nodes" mode="f:export">
      <xsl:with-param name="label" select="$label"/>
      <xsl:with-param name="batch" select="$batch" tunnel="yes"/>
    </xsl:apply-templates>
  </xsl:function>
  <xsl:template match="/">
    <xsl:value-of select="string-join(f:export(invoices/invoice, 'EXPORT', 'BATCH-7'), '')"/>
  </xsl:template>
</xsl:stylesheet>
