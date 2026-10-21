<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR        : Charles Korthout
  CREATE DATE   : 21 October 2026
  PURPOSE       : Verify intake type/pattern contract: array-member-plus.
  SPECIAL NOTES : Synthetic REQ-126 example; stylesheet4.0, default Bosak Legacy host.
  COPYRIGHT     : Fytala
  LICENSE       : license.md (Apache-2.0)
  SPDX-License-Identifier: Apache-2.0
  Change history: Charles Korthout | 0.1 | 21-10-2026 | Creation.
  Template /: namespaced intake document -> exact report or a declared diagnostic.
  Preconditions: unique order IDs; line quantities parse as integers; no external resources.
  Negative fixtures intentionally violate the declared syntax or type contract.
-->
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:types"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map"
  xmlns:array="http://www.w3.org/2005/xpath-functions/array" xmlns:d="urn:fytala:intake"
  exclude-result-prefixes="#all">
  <xsl:output method="text" encoding="UTF-8"/>

<xsl:template match="/" priority="10"><xsl:apply-templates select="[(1,2),3]" /></xsl:template><xsl:template match="array(atomic(xs:integer)+)" priority="5">match</xsl:template><xsl:template match="." priority="-10">fallback</xsl:template>
</xsl:stylesheet>
