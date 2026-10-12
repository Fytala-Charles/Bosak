<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify serialization parameter handling for invoice interchange.
SPECIAL NOTES: In-memory string capture; not octet or canonicalization conformance.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: invoice XML. Output: declared exact text or refusal marker.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0">
  <xsl:output method="text" line-ending="\r\n"/>
  <xsl:template match="/">
    <xsl:value-of select="invoices/invoice/@id" separator="&#10;"/>
  </xsl:template>
</xsl:stylesheet>
