<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR        : Charles Korthout
  CREATE DATE   : 11 October 2026
  PURPOSE       : Print a compact list of product codes for a picking label.
  SPECIAL NOTES : Standard XSLT 3.0 / XPath 3.1 baseline for the catalog comparison.
  COPYRIGHT     : Fytala
  LICENSE       : license.md (Apache-2.0)
  SPDX-License-Identifier: Apache-2.0
  Change history: Charles Korthout | 0.1 | 11-10-2026 | Creation of verified catalog example.
  Template /: input task document -> plain-text demonstration result.
  Contract: The separator is between iterations; emitted text and empty iteration bodies need deliberate handling.
-->
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:array="http://www.w3.org/2005/xpath-functions/array"
    xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="array map">
  <xsl:output method="text" encoding="UTF-8"/>
  <xsl:template match="/"><xsl:value-of select="string-join(/task/sku, ', ')"/></xsl:template>
</xsl:stylesheet>
