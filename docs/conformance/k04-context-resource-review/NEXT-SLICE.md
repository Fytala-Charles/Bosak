# C28 - Preserve library context-type declaration scope

28 October 2026. C27 is review-only; the context routing guard remains required. The full isolated set is 42/2/0 and four independent controls still fail.

1. Record immutable pre-implementation adoption of XQuery 3.1 REC-20170321: module static namespace/type context, context-item declarations and SequenceType matching. Preserve the C27 originals, controls and source/resource identities.
2. Carry each library constraint with its declaring namespace bindings and default type namespace. Preserve all module constraints; lexical type-text equality is insufficient for deduplication. Include namespace URI and name identity when resolving aliases.
3. Resolve library type aliases correctly, including aliases for XML Schema built-ins. Scope any shared matcher change separately if necessary. Avoid requiring caller bindings or leaking a library's namespace settings into the importer. Preserve host context on both successful and failed evaluation.
4. Add public import/module-loader regressions with positive and negative values, missing/conflicting importer aliases, two libraries sharing lexical type text under different namespace URIs, and default type namespace behavior. Retain existing XQST0099/XQST0113 restrictions, typed array/function behavior and C26's focus/cycle tests.
5. Re-run all 13 original resource/module cases, all 12 controls, the full 44-case isolated set, the C26 bounded original/control cohort, the full solution and both exact current frozen gates. A correction is qualified only when the valid original assertions pass and wrong-type constraints still raise their declared errors. Do not rewrite fixtures or weaken assertions.
6. Keep normal routing, dependency filters, KnownSkips, GatedSets and AGENTS baseline unchanged in the engine fix. Only after wider qualification should a separate slice inventory all affected routing cases, rehearse complete normal gates and prepare any exact owner-approved baseline proposal.

No schema backend, XSD 1.1 ownership, release, upstream submission or Braid change is part of this correction. Bosak.Schema owns its XSD 1.1 track. The diagnostic importer-prefix bindings are evidence, not a product workaround.
