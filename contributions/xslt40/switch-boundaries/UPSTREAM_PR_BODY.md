Adds two native switch boundary cases to the existing switch group.

Reviewed switch-001–007: basic expressions/content, multiple alternatives, fallback and no-match behavior exist. New cases add first-match termination with a later failing test, and empty-result termination after an empty candidate test. These are distinct from a switch with no matching branch.

- **switch-008**: Overlapping status alternatives select the first match and do not evaluate later tests. Bosak: PASS switch-008.
- **switch-009**: A matching empty-result branch ends selection instead of falling through. Bosak: PASS switch-009.

Expected outcomes follow the [current xsl-switch rules](https://qt4cg.org/specifications/xslt-40/Overview.html#xsl-switch), reviewed against [pinned source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml). No established case or expected result is changed. The catalog validates against the native schema.

Exactly 2 new cases pass and 0 fail with the Bosak adapter; processor failures are retained, not converted into expected errors. No independent XSLT 4.0 processor was available. Specification-derived assertions have been reviewed, but a reviewer cross-check is requested before acceptance.

These files have no Bosak dependencies. [Bosak](https://github.com/Fytala-Charles/Bosak) provides the adapter and [specification/consumer documentation and contribution queue](https://github.com/Fytala-Charles/Bosak/blob/main/docs/XSLT40_CONTRIBUTION_QUEUE.md); its 4.0 support is partial.
