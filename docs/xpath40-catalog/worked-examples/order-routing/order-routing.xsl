<!--
AUTHOR: Charles Korthout
CREATE DATE: 11 October 2026
PURPOSE: Validate fulfillment orders and create bounded consecutive dispatch batches.
SPECIAL NOTES: Synthetic orders vocabulary; portable XSLT 3.0, supplied XPath40 routing plan.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
CHANGE HISTORY: Charles Korthout | 0.1 | 11-10-2026 | Creation.
INPUT: urn:example:routing orders; plus a required externally supplied plan map.
OUTPUT: dispatch.xml principal; exceptions.xml secondary. No network or file reads.
CONTRACT: Source order retained; adjacent accepted orders share a route, at most two per batch.
REFUSAL: The host rejects missing/duplicate IDs before invoking this stylesheet.
-->
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:o="urn:example:routing"
  exclude-result-prefixes="xs o">
  <xsl:output method="xml" indent="yes" omit-xml-declaration="yes"/>

  <!-- Required host input: the map returned by routing-plan-40.xpath at XPath40.
       The host must refuse planning errors and must not substitute an empty plan. -->
  <xsl:param name="plan" as="map(*)" required="yes"/>

  <!-- Context: original source document. Preconditions: a successful plan for this source.
       Output: dispatch and exceptions; validation belongs to the XPath40 planning stage.
       Each batch is an array; wildcard lookup exposes its order-node members. -->
  <xsl:template match="/">
    <xsl:variable name="orders" select="$plan?orders"/>
    <xsl:variable name="accepted" select="$plan?accepted"/>
    <xsl:variable name="batches" select="$plan?batches"/>
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
        <xsl:for-each select="$plan?exceptions">
          <order id="{?order/@id}" route="{?order/@route}" missing-lines="{?missing-lines}"
            unsupported-route="{?unsupported-route}"
            invalid-line-positions="{string-join(?invalid-lines ! string(.), ' ')}"/>
        </xsl:for-each>
      </exceptions>
    </xsl:result-document>
  </xsl:template>
</xsl:stylesheet>
