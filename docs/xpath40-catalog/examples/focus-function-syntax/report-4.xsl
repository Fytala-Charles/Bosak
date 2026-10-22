<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR: Charles Korthout
  CREATE DATE: 22 October 2026
  PURPOSE: Produce an order-line pricing report with explicit focus boundaries.
  COPYRIGHT: Fytala
  LICENSE: license.md (Apache-2.0)
  SPDX-License-Identifier: Apache-2.0
  Change history: Charles Korthout | 0.1 | 22-10-2026 | Creation.
  Contract: urn:fytala:focus orders/line with sku, qty and unit attributes -> text report.
  Preconditions: quantities and unit amounts cast to decimal; one currency; no tax/discount.
  Edge behavior: empty orders yield lines=0;total=0.00; invalid decimal raises FORG0001.
  Simplification: synthetic order vocabulary, not an industry-standard order or accounting model.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:o="urn:fytala:focus"
  exclude-result-prefixes="#all" version="4.0">
  <xsl:output method="text" encoding="UTF-8"/>
  <!-- Template /: one orders document -> source-order labels and one count/total summary. -->
  <xsl:template match="/">
    <xsl:variable name="lines" select="/o:orders/o:line"/>
    <!-- Focus amount helper: one line as the argument/context value -> decimal amount.
         Its implicit signature is function(item()*) as item()*; the task supplies one line. -->
    <xsl:variable name="amount" select="fn { xs:decimal(@qty) * xs:decimal(@unit) }"/>
    <!-- Format helper: SKU string + decimal amount -> SKU=amount; two display decimals only. -->
    <xsl:variable name="format" select="fn($sku as xs:string, $value as xs:decimal) as xs:string {
      concat($sku, '=', format-number($value, '0.00')) }"/>
    <!-- Mapping arrow calls the amount function once per line; the empty input makes no calls. -->
    <xsl:variable name="amounts" select="$lines =!&gt; $amount()"/>
    <!-- Label focus function captures the helper variables; pipeline summary sees ALL amounts. -->
    <xsl:value-of select="string-join((
      $lines =!&gt; fn { $format(string(@sku), $amount(.)) }(),
      $amounts -&gt; concat('lines=', count(.), ';total=', format-number(sum(.), '0.00'))), '|')"/>
  </xsl:template>
</xsl:stylesheet>
