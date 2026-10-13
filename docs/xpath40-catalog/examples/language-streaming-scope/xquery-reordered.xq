(: XQuery 3.1: select positive invoices and sort by currency then identifier. :)
xquery version "3.1";
let $batch := parse-xml('<invoices><invoice id="INV-1042" currency="EUR" total="30"/><invoice id="INV-1043" currency="USD" total="12"/><invoice id="INV-1044" currency="EUR" total="0"/></invoices>')
return string-join((for $invoice in $batch/invoices/invoice
where xs:decimal($invoice/@total) gt 0
order by string($invoice/@currency), string($invoice/@id)
return concat($invoice/@id, '/', $invoice/@currency, ':', format-number(xs:decimal($invoice/@total), '0.00'))), ';')
