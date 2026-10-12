<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify deferred capturing accumulators and in-memory invoice alternatives.
SPECIAL NOTES: Capture attribute acceptance is not snapshot support; no streaming claim.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: batch/invoice id and line children. Output: exact identity/line-count report.
The retained ordinary accumulator value is a source node. Explicit snapshot creates
a new node; it is not the capture rule's special no-accumulator-values contract.
All catalog cases use ordinary in-memory CaptureTransform, not TransformStreaming.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
  <xsl:output method="text"/>
  <xsl:accumulator name="n" initial-value="0">
    <xsl:accumulator-rule match="line" select="$value + 1"/>
  </xsl:accumulator>
  <xsl:mode use-accumulators="n"/>
  <xsl:template match="/">
    <xsl:value-of select="accumulator-after('n')"/>
  </xsl:template>
</xsl:stylesheet>
