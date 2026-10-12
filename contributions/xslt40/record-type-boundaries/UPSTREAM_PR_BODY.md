Adds two native record-type boundary cases to the existing record group.

Existing record-001–025 and subtype cases primarily use structural record syntax; the only scanned xsl:record-type occurrence is an older atomic-set package using extensible forms. New cases target current nominative declarations with constructor=yes, explicit empty fields, and static initialization excluded from constructor arguments. They do not change old structural/optional-field expectations or certify all nominative semantics.

- **record-026**: Named record constructor uses declared field order and retains an explicit empty field. Bosak: FAIL record-026: XPST0017: Function {urn:order-types}line#3 not found..
- **record-027**: Static field initialization is excluded from constructor parameters and is retained in the record. Bosak: FAIL record-027: XPST0017: Function {urn:order-types}line#2 not found..

Expected outcomes follow the [current named-record-types rules](https://qt4cg.org/specifications/xslt-40/Overview.html#named-record-types), reviewed against [pinned source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml). No established case or expected result is changed. The catalog validates against the native schema.

Exactly 0 new cases pass and 2 fail with the Bosak adapter; processor failures are retained, not converted into expected errors. No independent XSLT 4.0 processor was available. Specification-derived assertions have been reviewed, but a reviewer cross-check is requested before acceptance.

These files have no Bosak dependencies. [Bosak](https://github.com/Fytala-Charles/Bosak) provides the adapter and [specification/consumer documentation and contribution queue](https://github.com/Fytala-Charles/Bosak/blob/main/docs/XSLT40_CONTRIBUTION_QUEUE.md); its 4.0 support is partial.
