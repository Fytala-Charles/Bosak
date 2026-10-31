# REQ-126 C12: final semantic and evidence batch

**Complete bounded review — 26 October 2026.** All 76 catalog entries now have C09-C12 batch receipts. C13 remains open for blocker disposition and explicit owner acceptance; REQ-126 is still In Progress.

This final batch reviews 19 entries and 57 contract groups. Fresh catalog 3,148 + API 296 + XSLT 2,151 = **5,595 passing tests**, zero failures/skips. Exact scope: 369 distinct catalog cases and 264 compiled assets. Six linked walkthroughs contain 15 blocks: 12 input/fixture blocks and three family commands whose selected cases are covered by the full catalog run. The 19 Sample Code sections contain no inline fences.

The independent 47 XPath expressions and 52 host requests retain their original C05/C06 expectations. Six choice-coercion controls and two switch-comparison controls remain unmet. Passing membership, explicit cast or ordinary string dispatch does not resolve those gaps. Detailed observations and per-entry context are retained in [the review JSON](SEMANTIC-REVIEW-BATCH-4.json), [decisions](C12-REVIEW-DECISIONS.json) and [evidence manifest](evidence/c12-semantic-batch4/manifest.json).

| Entry | Bounded decision | Cases |
|---|---|---:|
| [rejected/obsolete-quantifier-functions.md](rejected/obsolete-quantifier-functions.md) | Verified rejected names and quantified alternative | 3 |
| [rejected/xpath-switch-expression.md](rejected/xpath-switch-expression.md) | Verified XPath refusal and conditional alternative | 2 |
| [type-system/choice-types.md](type-system/choice-types.md) | Limited membership/cast; matching-coercion blocker | 3 |
| [type-system/enum-types.md](type-system/enum-types.md) | Verified bounded enumeration | 3 |
| [type-system/nominative-records.md](type-system/nominative-records.md) | Verified bounded XSLT nominal identity | 5 |
| [type-system/record-updates.md](type-system/record-updates.md) | Verified bounded annotated reconstruction | 46 |
| [type-system/structural-records.md](type-system/structural-records.md) | Verified bounded conversion and annotation | 3 |
| [type-system/typed-let-coercion.md](type-system/typed-let-coercion.md) | Limited typed binding; choice blocker retained | 3 |
| [xslt-instructions/compound-patterns.md](xslt-instructions/compound-patterns.md) | Verified bounded pattern sets and priorities | 30 |
| [xslt-instructions/instruction-separators.md](xslt-instructions/instruction-separators.md) | Verified bounded iteration separators | 3 |
| [xslt-instructions/module-profiles.md](xslt-instructions/module-profiles.md) | Verified bounded direct module policy | 114 |
| [xslt-instructions/processor-profiles.md](xslt-instructions/processor-profiles.md) | Verified bounded frozen host policy | 66 |
| [xslt-instructions/structured-value-patterns.md](xslt-instructions/structured-value-patterns.md) | Verified bounded structured matching | 40 |
| [xslt-instructions/xsl-array.md](xslt-instructions/xsl-array.md) | Verified bounded sequence-valued members | 3 |
| [xslt-instructions/xsl-if.md](xslt-instructions/xsl-if.md) | Verified bounded conditional instruction | 3 |
| [xslt-instructions/xsl-map.md](xslt-instructions/xsl-map.md) | Verified bounded merge policies | 3 |
| [xslt-instructions/xsl-note.md](xslt-instructions/xsl-note.md) | Verified inert authoring content | 3 |
| [xslt-instructions/xsl-select.md](xslt-instructions/xsl-select.md) | Verified bounded XPath body instruction | 37 |
| [xslt-instructions/xsl-switch.md](xslt-instructions/xsl-switch.md) | Limited selection; incomparable-comparison blocker | 3 |

Current review source is immutable `b4723ff52fd9d4fabf2b4cd33bd44f21939c512c`; it is not a recovered adoption pin. Original adoption fields and C08 dispositions are unchanged. Legacy version policy, frozen Draft XPath, generic JNode/model limitations, nominal context, complete static-detection mode and package/override/transitive graph boundaries remain explicit. Delivered union-priority and type-version-gate corrections remain closed.

Two prose corrections distinguish matching choice coercion from ordered cast, and ordinary switch dispatch from incomparable general comparison. Original entry text is preserved. No engine or executable fixture changed. Source-identical C02 full-solution 21,798 passes and frozen QT3/qt4 gate receipts are inherited; no new full-suite or gate execution is claimed.

Reproduce validation: `python docs/xpath40-catalog/evidence/c12-semantic-batch4/validate-evidence.py --local-artifacts`. Renderer evidence is HTML with unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.

Next: [C13 final closure review](CLOSURE-PLAN.md). Consolidate and resolve or explicitly dispose blocking receipts, then obtain the required owner acceptance of AC-01 through AC-04. Batch completion alone closes none of those criteria.
