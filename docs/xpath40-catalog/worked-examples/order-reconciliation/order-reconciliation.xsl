<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR        : Charles Korthout
  CREATE DATE   : 11 October 2026
  PURPOSE       : Reconcile ordered and shipped quantities by customer and product.
  SPECIAL NOTES : Synthetic vocabularies; partial report and separate exceptions; XSLT 3.0.
  COPYRIGHT     : Fytala
  LICENSE       : license.md (Apache-2.0)
  SPDX-License-Identifier: Apache-2.0
  Change history:
    Charles Korthout | 0.1 | 11-10-2026 | Creation: executable reference-guide case.
-->
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:o="urn:example:orders"
    xmlns:r="urn:example:reconciliation" xmlns:f="urn:example:reconciliation:functions"
    exclude-result-prefixes="xs o f">
  <xsl:output method="xml" encoding="UTF-8" indent="yes" omit-xml-declaration="yes"/>
  <xsl:strip-space elements="*"/>
  <xsl:key name="customer" match="o:customer" use="@id"/>
  <xsl:key name="product" match="o:product" use="@sku"/>
  <xsl:key name="shipment" match="o:shipment" use="concat(@order-id, '|', @line-id)"/>

  <!-- f:error: line -> primary reason or empty. IDs may not contain the key
       separator '|'. Require unique order IDs and per-order line IDs at the root.
       Customer/product keys must resolve exactly once. Quantities are positive
       integer units; missing, duplicate, or malformed reference data is rejected. -->
  <xsl:function name="f:error" as="xs:string?">
    <xsl:param name="line" as="element(o:line)"/>
    <xsl:variable name="doc" select="root($line)"/>
    <xsl:sequence select="
      if (normalize-space($line/@id) = '' or contains($line/@id, '|')) then 'invalid-line-id'
      else if (count(key('customer', $line/../@customer-id, $doc)) ne 1) then 'unknown-or-duplicate-customer'
      else if (count(key('product', $line/@sku, $doc)) ne 1) then 'unknown-or-duplicate-product'
      else if (not($line/@quantity castable as xs:integer)) then 'invalid-quantity'
      else if (xs:integer($line/@quantity) le 0) then 'invalid-quantity'
      else ()"/>
  </xsl:function>

  <!-- f:shipped: accepted line -> integer shipped units, zero with no matching
       shipments. Only individually positive-integer shipments participate.
       The third key argument explicitly chooses the source document because
       function evaluation does not inherit a usable document focus. -->
  <xsl:function name="f:shipped" as="xs:integer">
    <xsl:param name="line" as="element(o:line)"/>
    <xsl:variable name="shipments" select="key('shipment', concat($line/../@id, '|', $line/@id), root($line))"/>
    <xsl:sequence select="xs:integer(sum(for $s in $shipments return
      if ($s/@quantity castable as xs:integer) then
        (if (xs:integer($s/@quantity) gt 0) then xs:integer($s/@quantity) else 0) else 0))"/>
  </xsl:function>

  <!-- Root: one batch containing reference tables, orders, and shipments.
       Fail on missing/duplicate order or line IDs: ambiguous joins are a batch
       contract failure. Business rejections instead yield exceptions and a
       review-required report; the host decides whether the batch can be posted. -->
  <xsl:template match="/">
    <xsl:if test="not(o:batch) or o:batch/o:orders/o:order[normalize-space(@id) = '' or contains(@id, '|')]
        or count(distinct-values(o:batch/o:orders/o:order/@id)) ne count(o:batch/o:orders/o:order)
        or o:batch/o:orders/o:order[count(distinct-values(o:line/@id)) ne count(o:line)]">
      <xsl:message terminate="yes">Batch requires unambiguous order and line identities.</xsl:message>
    </xsl:if>
    <xsl:variable name="lines" select="o:batch/o:orders/o:order/o:line"/>
    <xsl:variable name="bad-lines" select="$lines[exists(f:error(.))]"/>
    <xsl:variable name="bad-shipments" as="element(o:shipment)*">
      <xsl:for-each select="o:batch/o:shipments/o:shipment">
        <xsl:variable name="order-id" select="@order-id"/>
        <xsl:variable name="line-id" select="@line-id"/>
        <xsl:variable name="matched" select="$lines[../@id = $order-id and @id = $line-id]"/>
        <xsl:if test="if (count($matched) ne 1) then true() else exists(f:error($matched[1]))">
          <xsl:sequence select="."/>
        </xsl:if>
        <xsl:if test="if (count($matched) ne 1) then false() else empty(f:error($matched[1]))">
          <xsl:choose>
            <xsl:when test="not(@quantity castable as xs:integer)"><xsl:sequence select="."/></xsl:when>
            <xsl:when test="xs:integer(@quantity) le 0"><xsl:sequence select="."/></xsl:when>
          </xsl:choose>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:result-document href="exceptions.xml">
      <r:exceptions>
        <xsl:for-each select="$bad-lines"><r:exception scope="line" order-id="{../@id}" line-id="{@id}" reason="{f:error(.)}"/></xsl:for-each>
        <xsl:for-each select="$bad-shipments"><r:exception scope="shipment" order-id="{@order-id}" line-id="{@line-id}" reason="unmatched-or-invalid-shipment"/></xsl:for-each>
      </r:exceptions>
    </xsl:result-document>
    <r:report status="{if (exists(($bad-lines, $bad-shipments))) then 'review-required' else 'ready'}">
      <xsl:for-each-group select="$lines[empty(f:error(.))]" group-by="string(../@customer-id)">
        <xsl:sort select="../@customer-id"/>
        <xsl:variable name="customer" select="current-grouping-key()"/>
        <r:customer id="{$customer}" name="{key('customer', $customer)/@name}">
          <xsl:for-each-group select="current-group()" group-by="string(@sku)">
            <xsl:sort select="@sku"/>
            <xsl:variable name="sku" select="current-grouping-key()"/>
            <xsl:variable name="ordered" select="sum(current-group()/@quantity ! xs:integer(.))"/>
            <xsl:variable name="shipped" select="sum(current-group() ! f:shipped(.))"/>
            <r:product sku="{$sku}" name="{key('product', $sku)/@name}" ordered="{$ordered}" shipped="{$shipped}"
                outstanding="{max((0, $ordered - $shipped))}" over-shipped="{max((0, $shipped - $ordered))}"/>
          </xsl:for-each-group>
        </r:customer>
      </xsl:for-each-group>
    </r:report>
  </xsl:template>
</xsl:stylesheet>
