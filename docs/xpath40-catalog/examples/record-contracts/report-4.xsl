<?xml version="1.0" encoding="UTF-8"?>
<!--
 AUTHOR: Charles Korthout
 CREATE DATE: 22 October 2026
 PURPOSE: Report typed stock quantity adjustments without changing the input record.
 COPYRIGHT: Fytala
 LICENSE: license.md (Apache-2.0)
 SPDX-License-Identifier: Apache-2.0
 Change history: Charles Korthout | 0.1 | 22-10-2026 | Creation.
 Contract: urn:fytala:stock stock/item with sku, qty and delta -> before/after text report.
 Preconditions: SKU has no report delimiters; qty/delta cast to integer; input order retained.
 Simplification: synthetic inventory, no unit conversion, persistence, concurrent updates or stock policy.
 Edge: empty stock yields items=0;total=0; invalid numeric lexical values raise FORG0001.
-->
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:map="http://www.w3.org/2005/xpath-functions/map"
 xmlns:s="urn:fytala:stock" exclude-result-prefixes="#all">
 <xsl:output method="text" encoding="UTF-8"/>
 <!-- Template /: one stock document -> source-order before/after labels, count and final total. -->
 <xsl:template match="/">
  <!-- Each result row contains original quantity and a separate adjusted map/record; no mutation. -->
  <xsl:variable name="rows" as="map(*)*">
   <xsl:for-each select="/s:stock/s:item">
    <xsl:variable name="raw" select="map{'sku': string(@sku), 'qty': string(@qty), 'trace': 'import-only'}"/>
    <!-- Cast adapter: select declared fields, cast qty, discard trace, create present-but-empty note. -->
    <xsl:variable name="before" select="$raw cast as record(sku as xs:string, qty as xs:integer, note as xs:string?)"/>
    <!-- Update constructs a new record with the same annotation; rhs values are recoerced, not cast. -->
    <xsl:variable name="after" select="$before but with map{'qty': $before?qty + xs:integer(@delta)}"/>
    <xsl:sequence select="map{'before': $before?qty, 'after': $after}"/>
   </xsl:for-each>
  </xsl:variable>
  <xsl:value-of select="string-join((
   for $row in $rows return concat($row?after?sku, ':', $row?before, '-&gt;', $row?after?qty),
   concat('items=', count($rows), ';total=', sum($rows?after?qty))), '|')"/>
 </xsl:template>
</xsl:stylesheet>
