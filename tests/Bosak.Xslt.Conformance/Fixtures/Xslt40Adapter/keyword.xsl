<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0">
  <xsl:template name="xsl:initial-template">
    <result><xsl:value-of select="string-join(sort(input := (3,1,2)) ! string(.),',')"/></result>
  </xsl:template>
</xsl:stylesheet>
