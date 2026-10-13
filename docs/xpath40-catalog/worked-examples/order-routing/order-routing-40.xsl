<!--
AUTHOR: Charles Korthout
CREATE DATE: 11 October 2026
PURPOSE: Validate fulfillment orders and create bounded consecutive dispatch batches.
SPECIAL NOTES: Synthetic orders vocabulary; requires Bosak source version 1.1.1 or later; frozen XPath40.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
CHANGE HISTORY: Charles Korthout | 0.1 | 11-10-2026 | Creation.
INPUT: urn:example:routing orders; unique nonblank IDs, route, one or more lines.
OUTPUT: dispatch.xml principal; exceptions.xml secondary. No network or file reads.
CONTRACT: Source order retained; adjacent accepted orders share a route, at most two per batch.
REFUSAL: Missing or duplicate IDs terminate before either output is constructed.
-->
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:o="urn:example:routing"
  xmlns:f="urn:example:routing:functions" exclude-result-prefixes="xs o f">
  <xsl:output method="xml" indent="yes" omit-xml-declaration="yes"/>

  <!-- Purpose: Validate one line. Input: a line element. Result: boolean.
       Missing SKU, non-integer or non-positive quantity is false. Cast only after castable. -->
  <xsl:function name="f:valid-line" as="xs:boolean">
    <xsl:param name="line" as="element(o:line)"/>
    <xsl:sequence select="normalize-space($line/@sku) ne '' and
      (if ($line/@quantity castable as xs:integer)
       then xs:integer($line/@quantity) gt 0 else false())"/>
  </xsl:function>

  <!-- Purpose: Validate dispatch content. Input: one order. Result: boolean.
       An empty line sequence is rejected explicitly: every((), predicate) alone is true.
       Only NORTH and SOUTH are supported routes; IDs are validated separately for the whole batch. -->
  <xsl:function name="f:valid-order" as="xs:boolean">
    <xsl:param name="order" as="element(o:order)"/>
    <xsl:sequence select="$order/@route = ('NORTH', 'SOUTH') and exists($order/o:line)
      and every($order/o:line, function($line, $position) { f:valid-line($line) })"/>
  </xsl:function>

  <!-- Context: source document. Preconditions: exactly the authored orders root.
       Output: dispatch and exceptions. Duplicate/missing identities refuse the entire run.
       Partition callback receives current partition, next order and accepted-sequence position;
       true starts a new batch BEFORE the next order. It never produces empty arrays. -->
  <xsl:template match="/">
    <xsl:variable name="orders" select="o:orders/o:order"/>
    <xsl:if test="some($orders, function($order, $position) { normalize-space($order/@id) = '' })
      or not(all-different($orders/@id ! string(.)))">
      <xsl:message terminate="yes">ROUTING-ID: order IDs must be nonblank and unique.</xsl:message>
    </xsl:if>
    <xsl:variable name="accepted" select="$orders[f:valid-order(.)]"/>
    <xsl:variable name="batches" select="partition($accepted,
      function($current, $next, $position) {
        count($current) ge 2 or $current[1]/@route ne $next/@route
      })"/>
    <dispatch accepted="{count($accepted)}" rejected="{count($orders) - count($accepted)}">
      <xsl:for-each select="$batches">
        <xsl:variable name="members" select="?*"/>
        <batch number="{position()}" route="{$members[1]/@route}">
          <xsl:for-each select="$members">
            <order id="{@id}" units="{sum(o:line/@quantity ! xs:integer(.))}"/>
          </xsl:for-each>
        </batch>
      </xsl:for-each>
    </dispatch>
    <xsl:result-document href="exceptions.xml">
      <exceptions>
        <xsl:for-each select="$orders[not(f:valid-order(.))]">
          <xsl:variable name="invalid-lines" select="index-where(o:line,
            function($line, $position) { not(f:valid-line($line)) })"/>
          <order id="{@id}" route="{@route}" missing-lines="{empty(o:line)}"
            unsupported-route="{not(@route = ('NORTH', 'SOUTH'))}"
            invalid-line-positions="{string-join($invalid-lines ! string(.), ' ')}"/>
        </xsl:for-each>
      </exceptions>
    </xsl:result-document>
  </xsl:template>
</xsl:stylesheet>
