<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR        : Charles Korthout
  CREATE DATE   : 21 October 2026
  PURPOSE       : Verify intake type/pattern contract: enum-allowed.
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
<xsl:item-type name="t:status" as="enum('ready','queued')"/>
  <xsl:template match="/"><xsl:value-of select="'ready' instance of t:status" /></xsl:template>
</xsl:stylesheet>
