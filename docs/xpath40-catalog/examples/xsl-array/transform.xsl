<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR        : Charles Korthout
  CREATE DATE   : 11 October 2026
  PURPOSE       : Build an array of picking-label product codes.
  SPECIAL NOTES : Bosak-supported XSLT 4.0 draft instruction example; XPath expressions stay 3.1.
  COPYRIGHT     : Fytala
  LICENSE       : license.md (Apache-2.0)
  SPDX-License-Identifier: Apache-2.0
  Change history: Charles Korthout | 0.1 | 11-10-2026 | Creation of verified catalog example.
  Template /: input task document -> plain-text demonstration result.
  Contract: With select, each selected item is one member; explicit array-member can retain a sequence as a member.
-->
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:array="http://www.w3.org/2005/xpath-functions/array"
    xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="array map">
  <xsl:output method="text" encoding="UTF-8"/>
  <xsl:template match="/"><xsl:variable name="skus" as="array(*)"><xsl:array select="/task/sku/string()"/></xsl:variable><xsl:value-of select="array:size($skus)"/></xsl:template>
</xsl:stylesheet>
