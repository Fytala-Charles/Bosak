# Native XSLT 4.0 contributions

Specification-derived native-suite additions, executed with the Bosak adapter. Upstream acceptance and independent processor confirmation are pending for all five first batches. Counts describe exact new cases; they are not complete family or engine conformance.

| Family | Upstream PR | Exact Bosak outcome | Examples and evidence |
|---|---|---|---|
| array/member | [#22](https://github.com/qt4cg/xslt40-test/pull/22) | 6 pass after fix; original 5 pass/1 failure retained | [Details](array-member-boundaries/README.md) |
| select | [#23](https://github.com/qt4cg/xslt40-test/pull/23) | 2 pass after fix; original 2 failures retained | [Details](select-boundaries/README.md) |
| switch | [#24](https://github.com/qt4cg/xslt40-test/pull/24) | 2 pass / 0 fail | [Details](switch-boundaries/README.md) |
| item-type | [#25](https://github.com/qt4cg/xslt40-test/pull/25) | 0 pass / 2 fail | [Details](item-type-boundaries/README.md) |
| record-type | [#26](https://github.com/qt4cg/xslt40-test/pull/26) | 0 pass / 2 fail | [Details](record-type-boundaries/README.md) |

No second processor is installed or required as a local submission prerequisite. Each PR requests reviewer cross-checking. The six new failures are Bosak implementation findings; expected outputs remain specification-derived. Source candidate 1.1.4 is unpublished; public release remains 1.1.3. Next: respond to upstream reviews and triage select first, then enumeration aliases and named-record constructors.


Select correction: source candidate is now 1.2.0 (unpublished). Two select cases pass; broader seqtor 78/5/9 retains five namespace-declaration failures. Next: enumeration alias triage, then named-record constructors; expression-body namespace support remains a separate follow-up.
