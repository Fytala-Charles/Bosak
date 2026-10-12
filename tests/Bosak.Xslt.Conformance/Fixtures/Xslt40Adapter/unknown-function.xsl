<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0">
  <xsl:template name="xsl:initial-template">
    <xsl:sequence select="not-a-registered-function()"/>
  </xsl:template>
</xsl:stylesheet>
