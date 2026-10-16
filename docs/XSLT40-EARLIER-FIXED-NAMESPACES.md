# Earlier-version fixed namespace contexts

Milestone2/3, unpublished patch1.16.2. The draft processor now activates module fixed namespaces from its captured capability, independently of declared module version. Legacy retains its earlier module-version behavior; no public API or default changes.

The private draft namespace marker is established on each owned module before fixed bindings are evaluated. Module whitelist admission uses the same activation helper as name binding. Every draft module is loaded through an in-memory XML reader into a separate tree, preserving whitespace, base URI and line information. This prevents loader annotations/caches, use-when pruning, later input changes and compiler reuse from changing earlier executables. Imports/includes/used packages inherit the root profile but keep their own binding maps. Provider provenance annotations are retained; mutable namespace undeclaration and unparsed-entity metadata are copied, while compiler-owned caches are not shared. Legacy loading is unchanged.

The [fixed namespace section](https://qt4cg.org/specifications/xslt-40/Overview.html#fixed-namespaces) and [3.0 compatibility rules](https://qt4cg.org/specifications/xslt-40/Overview.html#backwards-3.0) support selecting processor capability independently of stylesheet version. This corrects namespace activation only; it does not claim all4.0 declaration features already work at every earlier module version.

Initial30-case baseline10pass/20fail. Initial focused137/0 and first full solution9469/0 plus frozen gates are retained under .artifacts/first-verification. Ownership review added provenance controls: original engine against final33-case fixture11pass/22fail; owned copies without provider metadata32pass/1fail (XML1.1 decoding). Final focused140/0 includes33 new plus107 existing controls. Final source is byte-identical to .artifacts/final-tests.txt.

Two exploratory serialized-prefix controls expected p:out but observed q:out even on the original engine. The original-engine diagnostic proves its source and owned copy both already retain OriginalPrefixAnnotation=p. These serialization probes and diagnostics are preserved separately, not reclassified as passing tests. The permanent ownership controls instead verify the copied XDM prefix and XML1.1 decoded name directly, alongside source/profile isolation and base URI. No serializer fix is included or fourth milestone PR planned.

Both-profile native array/record/type-alias selections preserve every case status and failure detail: Legacy155/5/20,29/35/1,23/8/1; Draft158/2/20,29/35/1,23/8/1. All six exits1. No fixture edits, expected-code aliases or gate promotions. Original PR346 full receipts and first ownership attempt remain retained; final receipt hashes and case maps identify fresh runs.

PR347 exact-head CI38946317614 passed at2308de105a1ac789ecaa300255efadb67aad6db9 and merged at375abcee7d025aa39bf5870c892460c8b383d06e. Initial baseline began on the exact precursor; whole precursor/merge trees match15bbedc48a8801f8fb4f8f83bac790e6362762c6 before fast-forward alignment. Only milestone3 remains: unknown future instructions without fallback, then stop and reassess.


All processes completed: Release build0errors/24 existing warnings; solution **9472/0/0** (XSLT6538), language-server **84/0/0**, ten CLI controls pass. Fresh QT3 **31,142/0/679**, qt4 gated **8,278/0/1,894**, both exit0. Verified fixture links removed; canonical documents synchronized and rendered.

Refreshed suite91328fbf0fed4a5d2813eb3661808ee53cb961ba; qtspecs72f222929768b411d8e88d96a1a8740483290078; xslt.xml blob59b9bdc79dca23b0c18fdd3e94130bc59037db3e. No upstream edits, aliases, skips or gate promotions. Source remains unpublished; public versions and independent acceptance unchanged.
