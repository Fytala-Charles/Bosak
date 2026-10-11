<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak reference guide worked examples">
  <br><br>
  <h1>Worked Examples and Chapter Plan</h1>
  <p>Learn XPath and XSLT through practical document-processing tasks</p>
</div>

## Reading path

Start with a business problem and follow the transformation from input to expected output. Each chapter has complete runnable files and named verification tests. The [feature catalog](../README.md) supplies detailed syntax and compatibility reference material when you need it.

| Chapter | Status | What it teaches |
|---|---|---|
| [Supplier invoices to ERP XML](invoice-to-erp/README.md) | Implemented; executable fixtures and tests supplied | Namespaces, selection, templates, optional fields, decimal calculations, grouping, exception output; a separate 3.1/4.0 migration pair. |
| [XML manual to HTML](manual-to-html/README.md) | Implemented; fixtures and three checks | Mixed content, template modes, stable heading IDs, internal links, table of contents, and missing-link detection. |
| [Order reconciliation report](order-reconciliation/README.md) | Implemented; fixtures and three checks | Customer/product grouping, lookup joins, missing reference data, and report/exception separation. |

The three chapters cover business mapping, document publishing, and operational reconciliation. Follow them in that order, or start with the task closest to your application. The [catalog examples](../examples/README.md) add 81 focused checks, including earlier-version baselines and compatibility refusals.

## Worked-example contract

1. State the user's problem and the output contract, including what a host must decide.
2. Declare assumptions and simplifications; identify synthetic vocabularies and sample data.
3. Supply complete input, code, expected output, required context, and compatibility level. Each chapter README shows full complete/edge-case input and output documents inline, with links to the executable fixtures.
4. Walk through the solution before discussing internal engine implementation. Explain source-to-target fields, validation and rejection rules, and every XSLT element used; identify the XPath constructs that make the solution work.
5. Explain choices and alternatives, including earlier-version idioms where useful.
6. Include at least one meaningful edge case and executable evidence for every claimed result.
7. Link back to relevant feature entries and forward to the next chapter.

The executable files are part of the teaching material. XSLT files carry a
standalone input/output contract, a change history, documented helper edge cases,
and template preconditions. Follow the [applicable XSLT comment conventions](../../XSLT_STYLE_GUIDE.md).

All three planned chapters are supplied and executable. The catalog has practical cases for its 28 current entries, but the wider inventory of shipped functions and deferred decisions remains incomplete; REQ-126 stays in progress.
