<?xml version="1.0" encoding="UTF-8"?>
<!--
  AUTHOR        : Charles Korthout
  CREATE DATE   : 11 October 2026
  PURPOSE       : Map supplier invoices into simplified ERP import and exception documents.
  SPECIAL NOTES : Reference-guide example using standard XSLT 3.0 / XPath 3.1.
                  Synthetic vocabularies; no industry-standard or tax-law compliance implied.
                  See README.md in this directory for fixtures, expected outputs, and tests.
  COPYRIGHT     : Fytala
  LICENSE       : license.md (Apache-2.0)
  SPDX-License-Identifier: Apache-2.0

  Change history:
    Charles Korthout | 0.1 | 11-10-2026 | Creation: verified invoice reference-guide case.
    Charles Korthout | 0.2 | 11-10-2026 | Add standalone contract, helper, and template documentation.

  INPUT CONTRACT
    Document root: s:batch in urn:example:supplier:invoice.
    Each s:invoice carries @id, @currency, supplier-id, issue-date (DD/MM/YYYY),
    optional purchase-order, and one or more s:line children.
    Lines carry @id, sku, quantity, unit-price, and tax-rate (percentage).
    Fields are single-valued in this simplified vocabulary. The stylesheet checks
    selected business values; it is not a substitute for validating the full structure.

  OUTPUT AND HOST CONTRACT
    Principal: erp:import in urn:example:erp:import.
    Secondary: erp:exceptions, href exceptions.xml, resolved against the host's
    base output URI. The chapter's host uses CaptureTransform: both documents
    are retained in memory, with no output writes. Other hosts choose their own
    output delivery and resource/execution policies.
    Invalid headers exclude the whole invoice. Invalid lines are omitted from
    its mapped lines and produce exceptions. An invoice with omitted lines is
    review-required; its totals describe accepted lines only. Transformation
    success does not mean that the invoice is ready to post to an ERP.

  CALCULATION POLICY
    Decimal quantity times price; line net rounded half-to-even to two places.
    Tax uses summed rounded line nets and is rounded once per numeric rate group.
    Gross is accepted net plus group tax. No discounts, credits, exchange rates,
    duplicate-ID detection, or reconciliation against supplier-declared totals.
    Adapt these example decisions to the receiving application's actual contract.
-->
<xsl:stylesheet version="3.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:s="urn:example:supplier:invoice"
    xmlns:erp="urn:example:erp:import"
    xmlns:f="urn:example:invoice:functions"
    exclude-result-prefixes="xs s f">
  <!-- Both documents use readable UTF-8 XML. Strip indentation-only source text:
       these fixtures are business records, not mixed-content publishing documents. -->
  <xsl:output method="xml" encoding="UTF-8" indent="yes" omit-xml-declaration="yes"/>
  <xsl:strip-space elements="*"/>

  <!--
    f:issue-date
    Purpose: convert the supplier's DD/MM/YYYY issue date into an ISO date value.
    Parameter: invoice, one s:invoice element with a single-valued issue-date field.
    Return: xs:date for a matching, valid calendar date; empty sequence otherwise.
    Edge posture: absent, blank, differently formatted, or impossible dates return
    empty. Construct the ISO spelling first and cast only after castability checks;
    31/02/2026 must not be normalized into a different date.
  -->
  <xsl:function name="f:issue-date" as="xs:date?">
    <xsl:param name="invoice" as="element(s:invoice)"/>
    <xsl:variable name="raw" select="normalize-space($invoice/s:issue-date)"/>
    <xsl:variable name="parts" select="tokenize($raw, '/')"/>
    <xsl:variable name="iso" select="string-join(($parts[3], $parts[2], $parts[1]), '-')"/>
    <xsl:sequence select="if (matches($raw, '^\d{2}/\d{2}/\d{4}$') and $iso castable as xs:date)
                          then xs:date($iso) else ()"/>
  </xsl:function>

  <!--
    f:header-errors
    Purpose: identify header defects that prevent mapping the entire invoice.
    Parameter: invoice, one s:invoice element.
    Return: all applicable reason strings in the declared check order, or empty
    for a header accepted by this example's rules.
    Edge posture: missing identifiers, unsupported currency, invalid date, or
    no lines reject the header. Currency is an exact EUR/USD/GBP allow-list;
    purchase-order is optional. A rejected header suppresses line-level checks
    in the exception path so incomplete header data cannot yield a partial import.
  -->
  <xsl:function name="f:header-errors" as="xs:string*">
    <xsl:param name="invoice" as="element(s:invoice)"/>
    <xsl:sequence select="(
      if (normalize-space($invoice/@id) = '') then 'missing-invoice-id' else (),
      if (normalize-space($invoice/s:supplier-id) = '') then 'missing-supplier-id' else (),
      if (not(normalize-space($invoice/@currency) = ('EUR', 'USD', 'GBP'))) then 'unsupported-currency' else (),
      if (empty(f:issue-date($invoice))) then 'invalid-issue-date' else (),
      if (empty($invoice/s:line)) then 'missing-lines' else ())"/>
  </xsl:function>

  <!--
    f:line-error
    Purpose: select the first defect that prevents using this line in totals.
    Parameter: line, one s:line element.
    Return: one primary reason string, or empty when the line is accepted.
    Edge posture: require ID and SKU, positive decimal quantity, non-negative
    decimal price, and a decimal tax percentage between 0 and 100 inclusive.
    Conditional branches test castability before casts; values such as pending
    are business rejections rather than numeric evaluation failures. The check
    order is part of the repair contract: fixing one defect may reveal another.
  -->
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

  <!--
    f:net
    Purpose: compute the accepted line's rounded net amount.
    Parameter: line, one s:line already accepted by f:line-error.
    Return: xs:decimal, quantity times unit price rounded half-to-even to two places.
    Invariant: callers validate first. This helper deliberately performs no further
    rejection checks; calling it on invalid values can raise a conversion error.
    The two-place rounding rule belongs to this example's target contract.
  -->
  <xsl:function name="f:net" as="xs:decimal">
    <xsl:param name="line" as="element(s:line)"/>
    <xsl:sequence select="round-half-to-even(xs:decimal($line/s:quantity) * xs:decimal($line/s:unit-price), 2)"/>
  </xsl:function>

  <!--
    f:tax-total
    Purpose: sum the rounded tax amounts of all accepted rate groups.
    Parameter: lines, zero or more s:line elements already accepted by f:line-error.
    Return: xs:decimal total tax; zero when no lines are accepted.
    Invariants: numeric grouping combines rates such as 21 and 21.00. Each group
    sums rounded line nets before calculating and rounding its tax, matching the
    emitted tax-summary. Two 0.03 lines at 21 percent yield group tax 0.01;
    rounding each line's tax separately would yield 0.02. Tests pin this decision.
  -->
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

  <!--
    Template /
    Purpose: coordinate the two outputs from a supplier batch document.
    Context: source document node; no external parameters or lookup resources.
    Result: secondary exceptions followed by the principal ERP import tree.
    Edge posture: wrong root vocabulary terminates processing. Valid batch
    structure with rejected business values produces explicit exception records.
    Header errors take precedence over line errors; only header-valid invoices
    are dispatched to the invoice template. Retain the invoice node in a variable
    because iterating header-error strings changes the current context item.
  -->
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

  <!--
    Template s:invoice
    Purpose: build one target invoice from a header-valid supplier invoice.
    Context/precondition: s:invoice selected by the root template's header filter.
    Result: identifiers, ISO issue date, optional purchase order, accepted lines
    in source order, numeric-rate tax groups in ascending order, and totals.
    Edge posture: any rejected line makes status review-required. If all lines
    are rejected, accepted totals are zero and the host must still hold the invoice.
    Missing/blank purchase-order is omitted; it does not cause rejection.
    Invariant: invoice tax and tax-summary share the same group-rounding policy.
  -->
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
          <!-- Sort by the representative line's numeric rate; every group member
               has that rate. Capture the group key before calculating its amount. -->
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

  <!--
    Template s:line
    Purpose: map one accepted supplier line into an ERP line.
    Context/precondition: s:line selected from the invoice's accepted sequence.
    Result: source line ID, trimmed SKU, decimal quantity, formatted unit price,
    and the rounded net from f:net. Invoice-level grouping owns tax output.
    Invariant: validation already established numeric values and required fields;
    this template formats them and does not silently supply missing values.
  -->
  <xsl:template match="s:line">
    <erp:line id="{@id}" sku="{normalize-space(s:sku)}" quantity="{xs:decimal(s:quantity)}"
        unit-price="{format-number(xs:decimal(s:unit-price), '0.00')}" net="{format-number(f:net(.), '0.00')}"/>
  </xsl:template>
</xsl:stylesheet>
