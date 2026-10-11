<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR        : Charles Korthout
  CREATE DATE   : 11 October 2026
  PURPOSE       : Route a task by a single priority selector.
  SPECIAL NOTES : Bosak-supported XSLT 4.0 draft instruction example; XPath expressions stay 3.1.
  COPYRIGHT     : Fytala
  LICENSE       : license.md (Apache-2.0)
  SPDX-License-Identifier: Apache-2.0
  Change history: Charles Korthout | 0.1 | 11-10-2026 | Creation of verified catalog example.
  Template /: input task document -> plain-text demonstration result.
  Contract: Tests are values compared with the selector, not boolean guards. The otherwise branch handles absent/unrecognized values.
-->
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:array="http://www.w3.org/2005/xpath-functions/array"
    xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="array map">
  <xsl:output method="text" encoding="UTF-8"/>
  <xsl:template match="/"><xsl:switch select="string(/task/@priority)"><xsl:when test="'urgent'" select="'expedite'"/><xsl:when test="'normal'" select="'standard'"/><xsl:otherwise select="'review'"/></xsl:switch></xsl:template>
</xsl:stylesheet>
