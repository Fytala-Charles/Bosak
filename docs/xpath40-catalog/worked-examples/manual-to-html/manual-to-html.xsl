<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR        : Charles Korthout
  CREATE DATE   : 11 October 2026
  PURPOSE       : Publish a synthetic XML manual as HTML with visible reference failures.
  SPECIAL NOTES : XSLT 3.0 / XPath 3.1; preserve mixed-content whitespace.
  COPYRIGHT     : Fytala
  LICENSE       : license.md (Apache-2.0)
  SPDX-License-Identifier: Apache-2.0
  Change history:
    Charles Korthout | 0.1 | 11-10-2026 | Creation: executable reference-guide case.
-->
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:m="urn:example:manual" exclude-result-prefixes="m">
  <xsl:output method="html" html-version="5.0" include-content-type="no" indent="no"/>
  <xsl:key name="section-by-id" match="m:section" use="@id"/>
  <xsl:mode name="body" on-no-match="shallow-skip"/>
  <xsl:mode name="toc" on-no-match="shallow-skip"/>

  <!-- Root contract: require one manual title and unique, non-empty section IDs.
       IDs are source-owned and stable across runs; reject ambiguity rather than
       manufacture order-dependent anchors. Mixed content is never strip-spaced.
       Output: one HTML document; missing references remain visible in its body. -->
  <xsl:template match="/">
    <xsl:if test="not(m:manual) or count(m:manual/m:title) ne 1
        or m:manual//m:section[normalize-space(@id) = '']
        or count(distinct-values(m:manual//m:section/@id)) ne count(m:manual//m:section)">
      <xsl:message terminate="yes">Manual requires a title and unique non-empty section IDs.</xsl:message>
    </xsl:if>
    <html lang="en">
      <head><title><xsl:value-of select="m:manual/m:title"/></title></head>
      <body>
        <h1><xsl:value-of select="m:manual/m:title"/></h1>
        <nav aria-label="Table of contents"><ul><xsl:apply-templates select="m:manual/m:section" mode="toc"/></ul></nav>
        <main><xsl:apply-templates select="m:manual/m:section" mode="body"/></main>
      </body>
    </html>
  </xsl:template>

  <!-- TOC context: one section. Output: link plus recursively nested child list.
       Labels use plain title text; body inline markup has a separate mode. -->
  <xsl:template match="m:section" mode="toc">
    <li><a href="#{@id}"><xsl:value-of select="m:title"/></a>
      <xsl:if test="m:section"><ul><xsl:apply-templates select="m:section" mode="toc"/></ul></xsl:if>
    </li>
  </xsl:template>

  <!-- Body context: one section; source nesting selects h2 through h6.
       Clamp deeper levels at h6 and preserve source IDs on the section wrapper. -->
  <xsl:template match="m:section" mode="body">
    <section id="{@id}">
      <xsl:element name="h{min((6, count(ancestor::m:section) + 2))}"><xsl:value-of select="m:title"/></xsl:element>
      <xsl:apply-templates select="node()[not(self::m:title)]" mode="body"/>
    </section>
  </xsl:template>

  <!-- Paragraph and emphasis rules process nodes, not only element children:
       text before, between, and after inline markup must retain its order. -->
  <xsl:template match="m:p" mode="body"><p><xsl:apply-templates mode="body"/></p></xsl:template>
  <xsl:template match="m:em" mode="body"><em><xsl:apply-templates mode="body"/></em></xsl:template>
  <xsl:template match="text()" mode="body"><xsl:value-of select="."/></xsl:template>

  <!-- Cross-reference context: m:xref with @target. Use a document-scoped key.
       Result: target-title link, or a visible missing-reference span. A missing
       target is not a successful link and must be reviewed before publication. -->
  <xsl:template match="m:xref" mode="body">
    <xsl:variable name="target" select="key('section-by-id', @target)"/>
    <xsl:choose>
      <xsl:when test="exists($target)"><a href="#{@target}"><xsl:value-of select="$target/m:title"/></a></xsl:when>
      <xsl:otherwise><span class="missing-reference" data-target="{@target}"><xsl:text>[Missing section: </xsl:text><xsl:value-of select="@target"/><xsl:text>]</xsl:text></span></xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
