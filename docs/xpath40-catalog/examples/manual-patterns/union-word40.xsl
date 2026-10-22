<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR        : Charles Korthout
  CREATE DATE   : 21 October 2026
  PURPOSE       : Verify manual routing/compound pattern contract: union-word40.
  SPECIAL NOTES : Synthetic REQ-126 fixture; stylesheet4.0, default Legacy host.
  COPYRIGHT     : Fytala
  LICENSE       : license.md (Apache-2.0)
  SPDX-License-Identifier: Apache-2.0
  Change history: Charles Korthout | 0.1 | 21-10-2026 | Creation.
  Template /: manual document or isolated root probe -> exact text or declared error.
  Preconditions: unique article IDs; fixed synthetic attributes; no external resources.
  Route templates: one article -> publish/hold/review text, with explicit business priorities.
-->
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:d="urn:fytala:manual"
  exclude-result-prefixes="#all">
  <xsl:output method="text" encoding="UTF-8"/>
<xsl:template match="/" priority="10"><xsl:apply-templates select="/*" /></xsl:template><xsl:template match="root union missing/child">pattern</xsl:template><xsl:template match="." priority="0.25">competitor</xsl:template>
</xsl:stylesheet>
