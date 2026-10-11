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
| XML manual to HTML | Planned | Mixed content, template modes, stable heading IDs, internal links, table of contents, and missing-link detection. |
| Order reconciliation report | Planned | Customer/product grouping, lookup joins, missing reference data, and report/exception separation. |

The publishing chapter comes next. Its proposed fixtures include nested sections, inline emphasis, cross-references, and a broken reference. Its verification should inspect generated HTML structure and links, including text order around inline elements. It will establish the template-oriented counterpart to the invoice mapping chapter.

## Worked-example contract

1. State the user's problem and the output contract, including what a host must decide.
2. Declare assumptions and simplifications; identify synthetic vocabularies and sample data.
3. Supply complete input, code, expected output, required context, and compatibility level.
4. Walk through the solution before discussing internal engine implementation.
5. Explain choices and alternatives, including earlier-version idioms where useful.
6. Include at least one meaningful edge case and executable evidence for every claimed result.
7. Link back to relevant feature entries and forward to the next chapter.

The invoice chapter is the first complete worked case. Existing feature-entry syntax snippets remain useful reference probes; converting the remaining entries into worked cases is still REQ-126 work. The book and catalog are not complete.
