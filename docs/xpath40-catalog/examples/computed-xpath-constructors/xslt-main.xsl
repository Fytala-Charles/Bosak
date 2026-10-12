<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Project a supplier invoice into an ERP receipt using portable XSLT 3.0.
SPECIAL NOTES: Alternative to deferred computed XPath node constructors; no external I/O.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation

Input contract: invoice/@id, customer text, amount text and amount/@currency.
The stylesheet projects supplied strings; it does not validate currency or totals.
XSLT node construction escapes XML characters through the serializer.
Empty customer content creates an empty customer element. Attribute instructions
precede child nodes. No computed XPath constructor expression is required.
-->
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" omit-xml-declaration="yes" indent="no"/>
  <xsl:template match="/invoice">
    <xsl:element name="receipt">
      <xsl:attribute name="id" select="string(@id)"/>
      <customer><xsl:value-of select="customer"/></customer>
      <total><xsl:attribute name="currency" select="string(amount/@currency)"/><xsl:value-of select="amount"/></total>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
