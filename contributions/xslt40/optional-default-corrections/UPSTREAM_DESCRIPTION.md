Corrects two existing optional/default function tests against the current XSLT/XPath 4.0 drafts:

- function-40-012: the optional default selected $data/a/@code, while the body evaluates $node ! @code. An attribute has no attribute children, so this returns empty instead of the expected blue. Select $data/a so the body returns the element's code attribute. The existing blue assertion is unchanged.
- function-40-903: replace numeric default 1e7 with QName('urn:invalid', 'invalid'), which cannot be coerced to xs:integer*. Current XPath 4.0 permits numeric-to-numeric coercion, so the old numeric value no longer reliably represents an invalid default. Update the expected argument conversion error from XTTE0600 to XPTY0004, matching the XPath rules used for both defaults and supplied arguments and the neighboring implicit-default case 904. Add modification metadata and clarify the description.

Specification basis:
- XSLT 4.0 10.3.2 delegates parameter conversion/errors to XPath: https://qt4cg.org/specifications/xslt-40/Overview.html#function-arguments
- XPath 4.0 4.6.1.2 applies argument coercion to defaults as well as supplied values: https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-eval-static-function-call
- XPath 4.0 3.4/3.4.2 allows numeric conversions and defines incompatible item/cardinality errors: https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-coercion-rules

Validation: catalog schema passes; corrected 012/903 and unchanged 904 each pass in Bosak source candidate 1.3.1 after its default-conversion error fix. Reverting only the default in 012 reproduces its empty-result failure; replacing the invalid QName with valid integer 7 makes 903 fail its expected-error assertion. Original corrected fixtures restored before submission. The corrected function-40 sample is 28 pass / 8 fail (remaining no-namespace cases are unsupported in Bosak). This is local validation; independent processor confirmation is pending.

This correction is separate from new test PR #31. Prepared from refreshed upstream master 91328fbf0fed4a5d2813eb3661808ee53cb961ba; upstream fetched again immediately before submission to avoid stale-base application conflicts.