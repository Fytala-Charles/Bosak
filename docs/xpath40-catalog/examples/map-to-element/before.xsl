<?xml version="1.0" encoding="UTF-8"?>
<!-- AUTHOR: Charles Korthout; CREATE DATE: 11 October 2026
     PURPOSE: Construct a product element from map fields using XSLT 3.0.
     SPECIAL NOTES: Synthetic baseline; one product code, no external resources.
     COPYRIGHT: Fytala; LICENSE: license.md (Apache-2.0); SPDX-License-Identifier: Apache-2.0
     Change history: Charles Korthout | 0.1 | 11-10-2026 | Creation.
     Template /: ignore source contents; build and display the agreed sku field. -->
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="text"/>
  <xsl:template match="/"><xsl:variable name="product" select="map{'sku':'FILTER-A'}"/><xsl:variable name="element"><sku><xsl:value-of select="$product?sku"/></sku></xsl:variable><xsl:value-of select="$element/sku"/></xsl:template>
</xsl:stylesheet>
