Adds six cases to the native array test set covering empty-member/no-emission distinction, empty sequence-constructor members, retained map/array containers, original source-node identity and order, for-each focus with empty members, and the XTTE4060 mixed JNode/non-JNode rule.

Expected results follow the array-construction rules in XSLT source e7ee4ab8. Existing grouped/nested/JNode cases were reviewed; no established expected result was changed. Catalog additions validate against the suite schema.

Bosak adapter evidence on clean contribution commit: five new cases pass; array-032 fails because Bosak returns <out>A|B</out> instead of raising XTTE4060. The required error expectation is retained. Independent processor confirmation remains outstanding. Aggregate runner counts also contain inherited substring matches, so reported batch counts use the exact six case IDs.

These scenarios arose from Bosak's XSLT 4.0 contribution review. [Bosak](https://github.com/Fytala-Charles/Bosak) provides a catalog adapter and documented specification/consumer examples, including [the contribution queue](https://github.com/Fytala-Charles/Bosak/blob/main/docs/XSLT40_CONTRIBUTION_QUEUE.md). Bosak currently has partial 4.0 coverage; the failure above is recorded explicitly.

This text is a local draft; no upstream PR or maintainer message has been sent.


Candidate update: the Bosak JNode array/member correction now passes all six unchanged cases. The earlier five-pass/one-failure evidence is the before-fix run. See ASSERTION_REVIEW.md and POST_FIX_RESULTS.json. Independent processor confirmation remains outstanding; this draft has not been submitted.
