<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR        : Charles Korthout
  CREATE DATE   : 11 October 2026
  PURPOSE       : Choose the dispatch queue label from the task priority.
  SPECIAL NOTES : Standard XSLT 3.0 / XPath 3.1 baseline for the catalog comparison.
  COPYRIGHT     : Fytala
  LICENSE       : license.md (Apache-2.0)
  SPDX-License-Identifier: Apache-2.0
  Change history: Charles Korthout | 0.1 | 11-10-2026 | Creation of verified catalog example.
  Template /: input task document -> plain-text demonstration result.
  Contract: This demonstration treats absent or non-urgent priorities as normal; it does not validate a priority vocabulary.
-->
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:array="http://www.w3.org/2005/xpath-functions/array"
    xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="array map">
  <xsl:output method="text" encoding="UTF-8"/>
  <xsl:template match="/"><xsl:choose><xsl:when test="/task/@priority = 'urgent'"><xsl:text>expedite</xsl:text></xsl:when><xsl:otherwise><xsl:text>normal</xsl:text></xsl:otherwise></xsl:choose></xsl:template>
</xsl:stylesheet>
