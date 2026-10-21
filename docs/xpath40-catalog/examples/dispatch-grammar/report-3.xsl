<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR        : Charles Korthout
  CREATE DATE   : 21 October 2026
  PURPOSE       : Verify dispatch grammar contract: report-3.
  SPECIAL NOTES : Synthetic REQ-126 example; stylesheet version 3.0; default Bosak Legacy host.
  COPYRIGHT     : Fytala
  LICENSE       : license.md (Apache-2.0)
  SPDX-License-Identifier: Apache-2.0
  Change history: Charles Korthout | 0.1 | 21-10-2026 | Creation.
  Template /: dispatch document -> exact text, or the explicitly declared error.
  Preconditions: no external resources or host parameters; negative fixtures are intentional.
-->
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:array="http://www.w3.org/2005/xpath-functions/array"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:d="urn:fytala:dispatch"
  exclude-result-prefixes="#all">
  <xsl:output method="text" encoding="UTF-8"/>

  <xsl:template match="/">
    <!-- Sort a node sequence once so IDs and grouped array members share positions. -->
    <xsl:variable name="ordered" as="element(d:batch)*">
      <xsl:perform-sort select="/d:dispatch/d:batch"><xsl:sort select="@id"/></xsl:perform-sort>
    </xsl:variable>
    <!-- A square constructor wraps each entire SKU sequence, including (). -->
    <xsl:variable name="groups" as="array(*)" select="array:join(for $batch in $ordered return [$batch/d:line/string(@sku)])"/>
    <xsl:variable name="flat" as="array(*)" select="array { $ordered/d:line/string(@sku) }"/>
    <xsl:value-of select="concat('batches=', count($ordered), ';flat=', array:size($flat), ';groups=', array:size($groups))"/>
    <xsl:for-each select="$ordered">
      <xsl:variable name="index" select="position()"/>
      <xsl:variable name="route" as="xs:string">
        <xsl:choose>
          <xsl:when test="@priority = ('urgent', 'rush')"><xsl:sequence select="'expedite'"/></xsl:when>
          <xsl:when test="@priority = 'normal'"><xsl:sequence select="'standard'"/></xsl:when>
          <xsl:otherwise><xsl:sequence select="'review'"/></xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!-- An absent SKU list is visible as '-', rather than losing its batch. -->
      <xsl:variable name="skus" select="array:get($groups, $index)"/>
      <xsl:value-of select="concat('|', @id, ':', $route, ':', if (empty($skus)) then '-' else string-join($skus, '+'))"/>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
