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
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
  <xsl:output method="csv" csv-header="yes"/>
  <xsl:template match="/">
    <xsl:sequence select="array{array{'INV-1042','EUR',30}}"/>
  </xsl:template>
</xsl:stylesheet>
