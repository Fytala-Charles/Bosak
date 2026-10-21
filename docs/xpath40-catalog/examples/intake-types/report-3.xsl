<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR        : Charles Korthout
  CREATE DATE   : 21 October 2026
  PURPOSE       : Verify intake type/pattern contract: report-3.
  SPECIAL NOTES : Synthetic REQ-126 example; stylesheet3.0, default Bosak Legacy host.
  COPYRIGHT     : Fytala
  LICENSE       : license.md (Apache-2.0)
  SPDX-License-Identifier: Apache-2.0
  Change history: Charles Korthout | 0.1 | 21-10-2026 | Creation.
  Template /: namespaced intake document -> exact report or a declared diagnostic.
  Preconditions: unique order IDs; line quantities parse as integers; no external resources.
  Negative fixtures intentionally violate the declared syntax or type contract.
-->
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:types"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map"
  xmlns:array="http://www.w3.org/2005/xpath-functions/array" xmlns:d="urn:fytala:intake"
  exclude-result-prefixes="#all">
  <xsl:output method="text" encoding="UTF-8"/>

  <!-- Template /: sort unique IDs, create typed payloads, emit one visible decision per order. -->
  <xsl:template match="/">
    <xsl:value-of select="concat('orders=',count(/d:intake/d:order))"/>
    <xsl:for-each select="/d:intake/d:order">
      <xsl:sort select="@id"/>
      <xsl:variable name="payload" as="item()">
        <xsl:choose>
          <xsl:when test="@kind='bundle'"><xsl:sequence select="array { d:sku/string() }"/></xsl:when>
          <xsl:otherwise>
            <xsl:variable name="quantity" as="xs:integer" select="xs:integer(@qty)"/>
            <xsl:sequence select="map{'sku':string(@sku),'qty':$quantity}"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:value-of select="concat('|',@id,':')"/>
      <xsl:call-template name="classify"><xsl:with-param name="payload" select="$payload"/></xsl:call-template>
    </xsl:for-each>
  </xsl:template>
  <!-- Helper classify: one map/array payload -> ship/bundle/review text.
       Parameters: payload is already projected from a line or bundle.
       Edge: reject nonpositive quantities and empty arrays explicitly; no schema validation. -->
  <xsl:template name="classify">
    <xsl:param name="payload" as="item()"/>
    <xsl:choose>
      <xsl:when test="$payload instance of map(*)">
        <xsl:choose>
          <xsl:when test="map:size($payload)=2 and map:contains($payload,'sku') and map:contains($payload,'qty') and $payload?sku instance of xs:string and $payload?qty instance of xs:integer and $payload?qty gt 0"><xsl:text>ship</xsl:text></xsl:when>
          <xsl:otherwise><xsl:text>review</xsl:text></xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$payload instance of array(*)">
        <xsl:choose>
          <xsl:when test="array:size($payload) gt 0 and (every $i in 1 to array:size($payload) satisfies array:get($payload,$i) instance of xs:string+)"><xsl:text>bundle</xsl:text></xsl:when>
          <xsl:otherwise><xsl:text>review</xsl:text></xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise><xsl:text>review</xsl:text></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
