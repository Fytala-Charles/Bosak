<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify deferred enclosing modes and portable invoice export/audit dispatch.
SPECIAL NOTES: Named top-level rules are the supported alternative; no external I/O.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: batch/invoice attributes and ordered line/@sku values. Output: text report.
Invoice and line rules share the export mode. Explicit mode or #current retains
export dispatch during recursion; omitted mode uses the lexical default instead.
These projections do not validate invoice data or enforce enclosing-mode isolation.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:erp="urn:example:erp" xmlns:q="urn:example:erp" version="4.0">
  <xsl:output method="text"/>
  <xsl:mode name="erp:export">
    <xsl:template match="invoice">
      <xsl:value-of select="concat(@id, ':', @currency, ':')"/>
      <xsl:apply-templates select="line"/>
      <xsl:text>;</xsl:text>
    </xsl:template>
    <xsl:template match="line">
      <xsl:value-of select="concat(@sku, '/')"/>
    </xsl:template>
  </xsl:mode>
  <xsl:template match="line" mode="erp:export">
    <xsl:value-of select="concat(@sku, '/')"/>
  </xsl:template>
  <xsl:template match="/">
    <xsl:apply-templates select="batch/invoice" mode="erp:export"/>
  </xsl:template>
</xsl:stylesheet>
