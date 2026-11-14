# Type-matcher closure candidate

Two capturing Any predicates in element/attribute alternative-name matching made ValueMatchesType construct a closure before every match. Ordered loops now perform the same recursive calls in the same order and return on the first match. Namespace/schema context, suffix construction and error order are preserved.

The pre-implementation [adoption record](ADOPTION.json) pins XPath3.1 REC-xpath-31-20170321 sections2.5.5.3,2.5.5.5,3.1.5.2 and2.3.4, with no intentional deviation or new grammar/alternative-name adoption. Development Core/server1.81.11 remains unpublished.

Five new regressions pass, including five primitive type/conversion allocation pairs, element/attribute order and prefix errors, namespace mutation, schema suffix/atomization, choice/empty/namespace-sensitive errors. Six existing typed-list tests pass. Warmed type matching is0 bytes/call; matching conversion40 bytes/call, compared with the earlier observed72 and112. No validation cache, new scalar shortcut or materialization change. These are calling-thread micro-allocation observations, not full-workload speed claims. Full solution, frozen QT3, gatedqt4 and pinned standalone qualification are pending; hosted qualification remains required before merge.
