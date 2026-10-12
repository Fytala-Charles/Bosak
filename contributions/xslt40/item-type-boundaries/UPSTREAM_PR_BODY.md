Adds two native item-type boundary cases to the existing type-alias group.

Reviewed type-alias case descriptions and representative declarations: array/record/union aliases, forward/unresolved/circular references, duplicate names, precedence and package visibility exist. New cases use enumeration membership boundaries and locally rebound QName prefixes with the same local type name. This is a focused audit, not exhaustive absence-of-duplicates certification.

- **type-alias-023**: An enumeration alias accepts status strings and rejects unknown strings and integers. Bosak: FAIL type-alias-023: XPST0051.
- **type-alias-024**: Enumeration aliases with the same local name but distinct expanded names remain distinct. Bosak: FAIL type-alias-024: XPST0051.

Expected outcomes follow the [current named-item-types rules](https://qt4cg.org/specifications/xslt-40/Overview.html#named-item-types), reviewed against [pinned source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml). No established case or expected result is changed. The catalog validates against the native schema.

Exactly 0 new cases pass and 2 fail with the Bosak adapter; processor failures are retained, not converted into expected errors. No independent XSLT 4.0 processor was available. Specification-derived assertions have been reviewed, but a reviewer cross-check is requested before acceptance.

These files have no Bosak dependencies. [Bosak](https://github.com/Fytala-Charles/Bosak) provides the adapter and [specification/consumer documentation and contribution queue](https://github.com/Fytala-Charles/Bosak/blob/main/docs/XSLT40_CONTRIBUTION_QUEUE.md); its 4.0 support is partial.
