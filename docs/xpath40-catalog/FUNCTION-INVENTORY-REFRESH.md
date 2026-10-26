<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak current function contract inventory">
  <h1>Function inventory refresh — C01</h1>
  <p>26 October 2026 · published Bosak 1.26.3</p>
</div>

**C01 is complete as an inventory and ownership workpackage. Twelve closure workpackages remain open.** REQ-126 and AC-01–04 remain open/partial; AC-05 remains met for current discoverability. Metadata, availability and declaration comparisons do not accept semantic contracts. Finish REQ-126 before returning to skip fixes, Braid or the VS Code extension.

## Sources and measured scope

The [current machine-readable inventory](CURRENT-FUNCTION-INVENTORY.json) refreshes **316 names**: **119 registered 4.0-only names** (70 non-constructors and 49 focus constructors), plus all 197 retained existing-function names. The previous 117-name inventory omitted `array:members` and `array:of-members`; no retained name disappeared. Both additions already exist in the released engine, so this is an inventory correction rather than a new feature.

Live metadata comes from the released-source `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f` binaries. The capture enumerates all 538 literal standard registrations before restricting the comparison scope. It retains every exact arity, variadic marker, coarse parameter/result kind, precise type metadata when present, profile flags, keyword labels and raw default expressions. Public API lookup probes cover every compared name, all literal arities, finite draft call ranges and representative variadic arities in XPath 3.1, 4.0 and Experimental: **527 requests ×3 =1,581 observations, zero probe errors**. A zero lookup is an observation, not a failing assertion; contextual/host/profile/default rules govern availability.

The current review source is [QT4CG F&O catalog b4723ff5](https://github.com/qt4cg/qtspecs/blob/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xpath-functions-40/src/function-catalog.xml), read from upstream HEAD on 26 October. The prior review [e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml) and [dated F&O 3.1 recommendation](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/) remain distinct retained inputs. All three raw catalogs and their hashes are retained. Neither review revision supplies the **28 missing historical adoption pins**; C08 is unchanged. The current draft is a review input, not an automatic language-profile upgrade.

## Differences and owners

Four raw function declarations differ from the retained draft: `fn:char`, `fn:partial-apply`, `fn:trim-space` and `fn:unix-dateTime`. Ten whole-function XML structure fingerprints differ. These counts include syntax, annotations and prose structure; they establish no count of semantic engine defects. C02 must assess the four declaration changes against frozen/Experimental policy, existing fixes and omitted/empty/error behavior.

The two live metadata changes in previously comparable rows are `fn:round` and `fn:round-half-to-even`, reflecting the delivered numeric contract work. Historical audit rows remain historical and link this refresh instead of being relabeled as fresh behavioral acceptance.

| Inventory observation | Count | Follow-up owner |
|---|---:|---|
| Non-constructor names without keyword metadata | 214 | C02: assess positional fallback, keyword/default behavior and intended support |
| Names whose keyword labels differ from current declared parameter lists | 4 | C02: classify actual call behavior and profile policy |
| Names with a finite current declared arity absent from exact registrations | 14 | C02: check default/variadic call resolution; absence alone is not a defect |
| Names with incomplete precise dispatch sequence-type metadata | 309 | C02 or C03: verify enforcement and document boundaries |
| Focus constructors | 49 | C03: lexical/value/context/coercion/error contracts; declarations are outside the F&O function catalog |
| `fn:transform`, absent from the literal StandardFunctions table | 1 | C06: inspect host registration and host-specific availability |
| Contract-by-contract omitted/empty/error acceptance | 316 names mapped | C02/C03, with host resolution in C06 and final per-entry evidence in C09–C12 |

Each JSON row retains current raw parameters, return types, default attributes, rule-source references and error sections; profile lookup observations; prior evidence/check/behavior mappings; and explicit gap owners. Raw declarations and retained evidence are review leads. Complete default evaluation, default-on-empty, context capture, conversion/error precedence and semantic equivalence remain for those owners. No absent metadata, changed fingerprint, or passing availability probe is silently converted into semantic acceptance.

## Reproduction and preservation

Run `python docs/xpath40-catalog/build-c01-inventory.py` and `python docs/xpath40-catalog/evidence/c01-inventory/validate-evidence.py`. The validator checks immutable inputs, source/binary identity receipts, lookup coverage, deterministic regeneration, owner mappings and the updated closure count. Capture/probe programs and project inputs are retained as text, with no new repository C# source or engine changes.

The engine, tests, package version and frozen gates are unchanged. Published 1.26.3 solution/integration results and the fresh C00 QT3 **31,142/0/679**, gated qt4 **8,436/0/1,894** results remain inherited evidence; this documentation step claims no fresh solution or conformance run. Changed pages are checked as HTML using unchanged Fytala CSS; screenshot/PDF acceptance is not claimed. Original C00, release, adoption and upstream receipts remain intact.
