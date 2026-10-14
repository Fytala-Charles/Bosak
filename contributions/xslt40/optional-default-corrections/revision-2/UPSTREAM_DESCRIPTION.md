Corrects two existing optional/default function tests against the current XSLT/XPath 4.0 drafts:

- function-40-012: the optional default selected $data/a/@code, while the body evaluates $node ! @code. An attribute has no attribute children, so this returns empty instead of the expected blue. Select $data/a so the body returns the element's code attribute. The existing blue assertion is unchanged.
- function-40-903: retain the original double default 1e7 and update the expected error from XTTE0600 to XPTY0004. XPath 4.0 3.4.2 explicitly excludes double-to-integer coercion: implicit numeric casts target primitive decimal/float/double, and promotion is not combined with relabeling. The default remains invalid; function argument coercion applies to defaults and supplied values.

Specification basis:
- XSLT 4.0 10.3.2 delegates parameter conversion/errors to XPath: https://qt4cg.org/specifications/xslt-40/Overview.html#function-arguments
- XPath 4.0 4.6.1.2 applies argument coercion to defaults as well as supplied values: https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-eval-static-function-call
- XPath 4.0 3.4/3.4.2 allows numeric conversions and defines incompatible item/cardinality errors: https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-coercion-rules

Validation: catalog schema passes; corrected 012/903 and unchanged 904 each pass in Bosak source candidate 1.3.2 after its default-conversion error fix. Reverting only the default in 012 reproduces its empty-result failure; replacing the invalid double with valid integer 7 makes 903 fail its expected-error assertion. Original corrected fixtures restored before submission. The corrected function-40 sample is 28 pass / 8 fail (remaining no-namespace cases are unsupported in Bosak). This is local validation; independent processor confirmation is pending.

This correction is separate from new test PR #31. Prepared from refreshed upstream master 91328fbf0fed4a5d2813eb3661808ee53cb961ba; upstream fetched again immediately before submission to avoid stale-base application conflicts.

Correction to the initial proposal: the numeric changelog summary was too broad to justify replacing the fixture. The normative implicit-casting table and its explicit double-to-integer exclusion govern this case; the original fixture is now preserved.