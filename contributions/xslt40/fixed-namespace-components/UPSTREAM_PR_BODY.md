Adds three focused tests to the existing fixed-namespaces set. Reviewed fixed-namespaces 001-012/901 and key 101/102. Case 010 covers implicit fixed-only modes; the new cases add explicit declarations with local native rebinding, root/scoped defaults with EQName membership, and fixed-bound key names alongside QName values whose lexical prefix is absent from the static context. Existing key QName tests do not combine these fixed/native binding boundaries.

- **fixed-namespaces-013**: explicit mode declaration, template membership and dispatch agree despite native rebinding.
- **fixed-namespaces-014**: root/scoped default-mode names agree with EQName template membership.
- **fixed-namespaces-015**: fixed-bound key declaration/string lookup agrees with a QName lookup carrying an unbound lexical prefix.

Expected results follow [fixed-namespaces](https://qt4cg.org/specifications/xslt-40/Overview.html#fixed-namespaces), [default-mode](https://qt4cg.org/specifications/xslt-40/Overview.html#default-mode) and [fn:key](https://qt4cg.org/specifications/xslt-40/Overview.html#func-key), reviewed against [pinned source 72f22292](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml). The native catalog schema validates. No established case or assertion changes, and no Bosak dependencies are introduced.

All three exact new cases pass using Bosak source revision 32916981 (unpublished candidate 1.2.1; published release remains 1.2.0). The wider set is 11 pass/5 fail/0 skip: existing cases 006-009 and 011 fail on user-defined function keyword parameter metadata (XPST0017). These outcomes are partial implementation evidence.

No independent XSLT 4.0 processor was run; reviewer cross-check is requested before acceptance. Portable patch, examples and exact receipts are recorded in [Bosak contribution evidence](https://github.com/Fytala-Charles/Bosak/tree/test/xslt40-fixed-namespace-contribution/contributions/xslt40/fixed-namespace-components).
