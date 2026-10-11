<?xml version="1.0" encoding="UTF-8"?>
<!-- Copyright Fytala. License: license.md (Apache-2.0). SPDX-License-Identifier: Apache-2.0 -->
<xsl:stylesheet version="3.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:s="urn:example:supplier:invoice"
    xmlns:erp="urn:example:erp:import"
    xmlns:f="urn:example:invoice:functions"
    exclude-result-prefixes="xs s f">
  <xsl:output method="xml" encoding="UTF-8" indent="yes" omit-xml-declaration="yes"/>
  <xsl:strip-space elements="*"/>

  <!-- Dates arrive as DD/MM/YYYY; invalid dates never reach a numeric/date cast. -->
  <xsl:function name="f:issue-date" as="xs:date?">
    <xsl:param name="invoice" as="element(s:invoice)"/>
    <xsl:variable name="raw" select="normalize-space($invoice/s:issue-date)"/>
    <xsl:variable name="parts" select="tokenize($raw, '/')"/>
    <xsl:variable name="iso" select="string-join(($parts[3], $parts[2], $parts[1]), '-')"/>
    <xsl:sequence select="if (matches($raw, '^\d{2}/\d{2}/\d{4}$') and $iso castable as xs:date)
                          then xs:date($iso) else ()"/>
  </xsl:function>

  <xsl:function name="f:header-errors" as="xs:string*">
    <xsl:param name="invoice" as="element(s:invoice)"/>
    <xsl:sequence select="(
      if (normalize-space($invoice/@id) = '') then 'missing-invoice-id' else (),
      if (normalize-space($invoice/s:supplier-id) = '') then 'missing-supplier-id' else (),
      if (not(normalize-space($invoice/@currency) = ('EUR', 'USD', 'GBP'))) then 'unsupported-currency' else (),
      if (empty(f:issue-date($invoice))) then 'invalid-issue-date' else (),
      if (empty($invoice/s:line)) then 'missing-lines' else ())"/>
  </xsl:function>

  <!-- One primary reason per line, in this declared validation order. -->
  <xsl:function name="f:line-error" as="xs:string?">
    <xsl:param name="line" as="element(s:line)"/>
    <xsl:sequence select="
      if (normalize-space($line/@id) = '') then 'missing-line-id'
      else if (normalize-space($line/s:sku) = '') then 'missing-sku'
      else if (not($line/s:quantity castable as xs:decimal)) then 'invalid-quantity'
      else if (xs:decimal($line/s:quantity) le 0) then 'invalid-quantity'
      else if (not($line/s:unit-price castable as xs:decimal)) then 'invalid-unit-price'
      else if (xs:decimal($line/s:unit-price) lt 0) then 'invalid-unit-price'
      else if (not($line/s:tax-rate castable as xs:decimal)) then 'invalid-tax-rate'
      else if (xs:decimal($line/s:tax-rate) lt 0 or xs:decimal($line/s:tax-rate) gt 100) then 'invalid-tax-rate'
      else ()"/>
  </xsl:function>

  <xsl:function name="f:net" as="xs:decimal">
    <xsl:param name="line" as="element(s:line)"/>
    <xsl:sequence select="round-half-to-even(xs:decimal($line/s:quantity) * xs:decimal($line/s:unit-price), 2)"/>
  </xsl:function>

  <xsl:function name="f:tax-total" as="xs:decimal">
    <xsl:param name="lines" as="element(s:line)*"/>
    <!-- Tax is rounded once per rate group, not separately per source line. -->
    <xsl:variable name="taxes" as="xs:decimal*">
      <xsl:for-each-group select="$lines" group-by="xs:decimal(s:tax-rate)">
        <xsl:variable name="rate" select="current-grouping-key()"/>
        <xsl:sequence select="round-half-to-even(sum(current-group() ! f:net(.)) * $rate div 100, 2)"/>
      </xsl:for-each-group>
    </xsl:variable>
    <xsl:sequence select="xs:decimal(sum($taxes))"/>
  </xsl:function>

  <xsl:template match="/">
    <xsl:if test="not(s:batch)">
      <xsl:message terminate="yes">Expected supplier batch in urn:example:supplier:invoice.</xsl:message>
    </xsl:if>
    <xsl:result-document href="exceptions.xml">
      <erp:exceptions>
        <xsl:for-each select="s:batch/s:invoice">
          <xsl:variable name="invoice" select="."/>
          <xsl:variable name="header-errors" select="f:header-errors(.)"/>
          <xsl:choose>
            <xsl:when test="exists($header-errors)">
              <xsl:for-each select="$header-errors">
                <erp:exception invoice-id="{$invoice/@id}" scope="invoice" reason="{.}"/>
              </xsl:for-each>
            </xsl:when>
            <xsl:otherwise>
              <xsl:for-each select="s:line[exists(f:line-error(.))]">
                <erp:exception invoice-id="{$invoice/@id}" scope="line" line-id="{@id}" reason="{f:line-error(.)}"/>
              </xsl:for-each>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
      </erp:exceptions>
    </xsl:result-document>
    <erp:import>
      <xsl:apply-templates select="s:batch/s:invoice[empty(f:header-errors(.))]"/>
    </erp:import>
  </xsl:template>

  <xsl:template match="s:invoice">
    <xsl:variable name="accepted" select="s:line[empty(f:line-error(.))]"/>
    <xsl:variable name="rejected" select="s:line[exists(f:line-error(.))]"/>
    <xsl:variable name="net" select="sum($accepted ! f:net(.))"/>
    <xsl:variable name="tax" select="f:tax-total($accepted)"/>
    <erp:invoice id="{@id}" supplier="{normalize-space(s:supplier-id)}" currency="{normalize-space(@currency)}"
        issue-date="{f:issue-date(.)}" status="{if (exists($rejected)) then 'review-required' else 'ready'}">
      <xsl:if test="normalize-space(s:purchase-order) ne ''">
        <erp:purchase-order><xsl:value-of select="normalize-space(s:purchase-order)"/></erp:purchase-order>
      </xsl:if>
      <erp:lines>
        <xsl:apply-templates select="$accepted"/>
      </erp:lines>
      <erp:tax-summary>
        <xsl:for-each-group select="$accepted" group-by="xs:decimal(s:tax-rate)">
          <xsl:sort select="xs:decimal(s:tax-rate)" data-type="number"/>
          <xsl:variable name="rate" select="current-grouping-key()"/>
          <xsl:variable name="taxable" select="sum(current-group() ! f:net(.))"/>
          <erp:tax rate="{$rate}" net="{format-number($taxable, '0.00')}"
              amount="{format-number(round-half-to-even($taxable * $rate div 100, 2), '0.00')}"/>
        </xsl:for-each-group>
      </erp:tax-summary>
      <erp:totals accepted-lines="{count($accepted)}" rejected-lines="{count($rejected)}"
          net="{format-number($net, '0.00')}" tax="{format-number($tax, '0.00')}" gross="{format-number($net + $tax, '0.00')}"/>
    </erp:invoice>
  </xsl:template>

  <xsl:template match="s:line">
    <erp:line id="{@id}" sku="{normalize-space(s:sku)}" quantity="{xs:decimal(s:quantity)}"
        unit-price="{format-number(xs:decimal(s:unit-price), '0.00')}" net="{format-number(f:net(.), '0.00')}"/>
  </xsl:template>
</xsl:stylesheet>
