(: XQuery 3.1: select positive invoices and sort by currency then identifier. :)
xquery version "3.1";
let $batch := parse-xml('<invoices/>')
return string-join((for $invoice in $batch/invoices/invoice
where xs:decimal($invoice/@total) gt 0
order by string($invoice/@currency), string($invoice/@id)
return concat($invoice/@id, '/', $invoice/@currency, ':', format-number(xs:decimal($invoice/@total), '0.00'))), ';')
