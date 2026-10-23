<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:fytala:telemetry" exclude-result-prefixes="xs t">
  <xsl:output method="text"/>
  <xsl:template match="/">
    <xsl:variable name="rows" select="t:telemetry/t:reading"/>
    <xsl:variable name="ready" select="$rows[(xs:integer(@flags) idiv 4) mod 2 = 1 and xs:integer(@count) ge 1000]"/>
    <xsl:for-each select="$rows">
      <xsl:value-of select="concat(@id, ':', if (. intersect $ready) then 'ready' else 'hold', '|')"/>
    </xsl:for-each>
    <xsl:value-of select="concat('ready=', count($ready), ';total=', sum($rows/@count ! xs:integer(.)))"/>
  </xsl:template>
</xsl:stylesheet>
