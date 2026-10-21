<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR        : Charles Korthout
  CREATE DATE   : 21 October 2026
  PURPOSE       : Verify dispatch grammar contract: recognized-select-4.
  SPECIAL NOTES : Synthetic REQ-126 example; stylesheet version 4.0; default Bosak Legacy host.
  COPYRIGHT     : Fytala
  LICENSE       : license.md (Apache-2.0)
  SPDX-License-Identifier: Apache-2.0
  Change history: Charles Korthout | 0.1 | 21-10-2026 | Creation.
  Template /: dispatch document -> exact text, or the explicitly declared error.
  Preconditions: no external resources or host parameters; negative fixtures are intentional.
-->
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:array="http://www.w3.org/2005/xpath-functions/array"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:d="urn:fytala:dispatch"
  exclude-result-prefixes="#all">
  <xsl:output method="text" encoding="UTF-8"/>

  <xsl:template match="/">
<xsl:select>1+1</xsl:select>
  </xsl:template>
</xsl:stylesheet>
