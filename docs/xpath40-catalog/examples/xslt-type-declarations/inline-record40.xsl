<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify direct structural record coercion in an XSLT 4.0 variable binding.
SPECIAL NOTES: Synthetic invoice projection; candidate 1.2.0 coerces the declared fields.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Change History: Charles Korthout | 0.2 | 13-10-2026 | Direct structural binding now verified
Input: invoice attributes id, currency and amount. Output: INV-2048|EUR for the valid fixture.
These fixtures do not publish an actual named type API or perform external I/O.
-->
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:erp="urn:example:erp" xmlns:map="http://www.w3.org/2005/xpath-functions/map">
  <xsl:output method="text"/>

  <!-- Context: document node; result: text or error. Local values are evaluated when used. -->
  <xsl:template match="/">
<xsl:variable name="r" as="record(id as xs:string, currency as xs:string)" select="map {'id': string(invoice/@id), 'currency': string(invoice/@currency)}"/><xsl:value-of select="concat($r?id, '|', $r?currency)"/>
  </xsl:template>
</xsl:stylesheet>
