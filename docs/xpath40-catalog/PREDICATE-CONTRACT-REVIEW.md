<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak equality and subsequence predicate review">
  <br><br><h1>Equality and Subsequence Predicate Contracts</h1>
  <p>Shipment-event acceptance and predicate contracts</p>
</div>

## Scope and outcome

Reviewed 2026-10-12 against Bosak `a151fff80e2ef3d3afc226179df673f66165ab4f` (merged PR #189), version 1.1.2. Five Frozen 4.0 families now have representative keyword, collation/default, callback, empty and typed-value evidence. **18 of the original 37 absent-keyword families have targeted behavioral review; 19 remain.** Metadata remains absent. REQ-126 stays In Progress and D2 partial.

102 new checks: 15 explicit keyword/mode refusals, 79 focused positional/default/type/callback checks and eight complete normal/empty/reordered/duplicate-event reports in 3.1/4.0. Catalog: 67 entries, 1,723 feature + 24 chapter checks = 1,747, four worked chapters. No engine/API/harness changes, gate promotion or release.

Primary contract: [immutable QT4CG F&O source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml). [Raw signature comparison](audit-signature-contracts.json) records the five rows' exact case IDs. This review pin does not recover historical adopted specification snapshots. Implementation: [FunctionLibrary](../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs), including AllEqual/AllDifferent, EffectiveCollation and SubsequenceCompareCallback/SubsequenceItemsEqual.

## Declared signatures and keyword forms

| Function | Pinned parameters/defaults | Result |
|---|---|---|
| `fn:all-different` | `values: xs:anyAtomicType*, collation: xs:string? := fn:default-collation()` | `xs:boolean` |
| `fn:all-equal` | `values: xs:anyAtomicType*, collation: xs:string? := fn:default-collation()` | `xs:boolean` |
| `fn:contains-subsequence` | `input: item()*, subsequence: item()*, compare: (fn($item1 as item(), $item2 as item()) as xs:boolean?)? := fn:deep-equal#2` | `xs:boolean` |
| `fn:ends-with-subsequence` | `input: item()*, subsequence: item()*, compare: (fn($item1 as item(), $item2 as item()) as xs:boolean?)? := fn:deep-equal#2` | `xs:boolean` |
| `fn:starts-with-subsequence` | `input: item()*, subsequence: item()*, compare: (fn($item1 as item(), $item2 as item()) as xs:boolean?)? := fn:deep-equal#2` | `xs:boolean` |

The collation argument defaults to the context default, including an explicit empty sequence. The comparator defaults to deep-equal#2, including explicit (). All functions return a singleton boolean. Positional equality calls have arity 1–2 and subsequence calls 2–3. All five keyword calls fail XPST0003 at 3.1 grammar and XPST0017 at 4.0/experimental binding. Experimental mode does not supply missing keyword metadata.

## Equality, distinctness and typed values

all-equal is the pinned distinct-values-count <= 1 rule; all-different compares that count with the supplied value count. Both are true for empty and singleton input. Numeric 1/1.0/1e0 and two NaNs collapse to one distinct value: all-equal true, all-different false. Integer 1 and string '1' remain distinct in the tested case. XML id elements atomize to their text; two A nodes are equal and not distinct.

Default codepoint comparison distinguishes A/a. Explicit () collation has the same result in these fresh contexts; the HTML ASCII case-insensitive collation reverses that decision. Unsupported URI returns FOCH0002. The fixtures do not configure a non-codepoint context default, so they do not certify every context/collation combination.

| Observed boundary | Current outcome | Follow-up |
|---|---|---|
| Map supplied to all-equal/all-different | Both return true for the tested singleton map. | Declared values are anyAtomicType*. Maps cannot satisfy the atomic signature; reconcile atomization/type checking. This fixture records acceptance, not a supported business idiom. |
| Numeric collation 7 | FOCH0002 | The declared argument is string?. Current processing reaches URI validation; review conversion/type checking separately. |
| Empty string collation | Same result as default codepoint in tested A/a calls. | EffectiveCollation treats empty text like an absent argument. The pin's default-on-empty note concerns (), so empty-string URI handling needs explicit collation-rule review. |

## Ordered matching and callback conversion

contains-subsequence finds a contiguous segment, not an ordered selection with gaps. Prefix and suffix checks match the corresponding boundaries. Empty required sequences match any input, including empty input; nonempty requirements do not match empty input or shorter input. Complete A/B matches and A/X/B refuses the A/B requirement in all three functions.

The default compares complete items using deep-equal: maps with the same code and different times do not match. A supplied callback can project and normalize only code, changing the business rule. The callback receives input first and requirement second. starts-with#2 makes Alpha/A true and A/Alpha false, confirming direction for all three predicates.

An empty callback argument selects the default; an empty callback result means false. Arity-one functions, nonfunctions, string 'yes' and multiple boolean results refuse XPTY0004 in the tested cases. An untypedAtomic('true') result converts to true. This is function conversion to a boolean result, not general effective-boolean-value truthiness. Full function subtyping/coercion remains outside this representative review.

## Complete workflow and remaining work

The [shipment acceptance README](examples/predicate-contracts/README.md) gives full XML, paired complete expressions, exact outputs and every focused fixture. [Shipment-event sequences](functions/shipment-event-sequences.md) and [sequence identity checks](functions/sequence-distinctness.md) retain their initial delivery, unit and gate evidence. These supplementary checks do not create another worked chapter or certify new gate sets.

Next review the remaining absent-keyword comparison/selection families: duplicate-values, highest, lowest and index-where. Use a warehouse exception-selection example with explicit collation/default/key/predicate and typed cases. Other missing families, full conversion/options, exhaustive changed-3.1 inventory, D3 grammar/pattern and D4 provenance/acceptance remain. The map/collation boundaries join the existing engine follow-ups; no silent fixes or closure based on sample coverage.


The subsequent [comparison/selection review](SELECTION-CONTRACT-REVIEW.md) brings targeted absent-keyword review to 21 of 37 families, with 16 remaining. Metadata is unchanged. Current catalog: 67 entries, 1,791 feature + 24 chapter checks = 1,815. Earlier delivery counts remain historical. Next: map/array container contracts; D2 stays partial.
