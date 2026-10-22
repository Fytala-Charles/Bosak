<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR        : Charles Korthout
  CREATE DATE   : 21 October 2026
  PURPOSE       : Verify manual routing/compound pattern contract: report-3.
  SPECIAL NOTES : Synthetic REQ-126 fixture; stylesheet3.0, default Legacy host.
  COPYRIGHT     : Fytala
  LICENSE       : license.md (Apache-2.0)
  SPDX-License-Identifier: Apache-2.0
  Change history: Charles Korthout | 0.1 | 21-10-2026 | Creation.
  Template /: manual document or isolated root probe -> exact text or declared error.
  Preconditions: unique article IDs; fixed synthetic attributes; no external resources.
  Route templates: one article -> publish/hold/review text, with explicit business priorities.
-->
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:d="urn:fytala:manual"
  exclude-result-prefixes="#all">
  <xsl:output method="text" encoding="UTF-8"/>
  <!-- Template / sorts unique IDs; explicit priorities express policy, not default-ranking assumptions. -->
<xsl:template match="/"><xsl:value-of select="concat('articles=',count(/d:manual/d:article))" /><xsl:apply-templates select="/d:manual/d:article" mode="route"><xsl:sort select="@id" /></xsl:apply-templates></xsl:template>
  <!-- One article -> publish text; unmatched inputs retain a visible review route. -->
<xsl:template match="d:article[@state='ready' and @audience='public' and not(@blocked='yes')]" mode="route" priority="10"><xsl:value-of select="concat('|',@id,':publish')" /></xsl:template>
  <!-- One article -> hold text; unmatched inputs retain a visible review route. -->
<xsl:template match="d:article[@blocked='yes' or @state='draft']" mode="route" priority="20"><xsl:value-of select="concat('|',@id,':hold')" /></xsl:template>
  <!-- One article -> review text; unmatched inputs retain a visible review route. -->
<xsl:template match="d:article" mode="route" priority="-10"><xsl:value-of select="concat('|',@id,':review')" /></xsl:template>
</xsl:stylesheet>
