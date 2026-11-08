# REQ-126 C13 acceptance proposal

**Proposal `REQ-126-C13-bounded-catalog-acceptance-v1` — pending owner approval.**

I recommend accepting the documentation catalog for the exact reviewed scope, with all technical gaps retained in the attached follow-up register. This is an explicit scope/disposition decision: it would close REQ-126 documentation work with known limitations. It would not establish full current-draft compliance, fix an engine defect, change a failing expectation or approve a release.

Approval would authorize recording AC-01 through AC-04 as accepted for that bounded catalog, maintaining AC-05, and completing C13/REQ-126. Until then, all four criteria remain unchecked and C13 remains open. If corrections are required first, the existing criteria and open status stay in force.

| Criterion | Proposed owner acceptance |
|---|---|
| AC-01 | Accept landed-slice/name/grammar traceability for the exact bounded catalog and the already-approved C08 historical policy; retain every unsupported/current-draft mismatch as an explicit technical follow-up. |
| AC-02 | Accept the 76 current lifecycle classifications and 19 original/wider disposition groups, preserving historical URLs, refusals, alternatives, non-equivalence and exact legacy history limits. |
| AC-03 | Accept applicable evidence for 235 bounded contract groups across 76 entries and the four worked chapters. Gates apply only where assigned; XSLT/deferred/refused forms use their named tests, exact examples or alternatives. The 48 retained batch unmet observations remain unmet, including two disputed-source observations. |
| AC-04 | Accept the 16-field structure, retained paths, synchronized index and feature-specific language-alignment warnings for the reviewed scope. No universal current-draft parity follows. |
| AC-05 | Maintain the previously met discoverability criterion and synchronized registry/canonical links. |

The 316-name inventory includes 197 existing functions; it is not an exhaustive review of every normative rule. D2/D3/D4 would be accepted for the documented bounded inventory, contracts and dispositions only. Unsupported syntax, generic conversion/model gaps, complete static-detection mode and unreviewed schema/package/host combinations remain explicitly unclaimed.

The existing C08 approval remains exactly 26 legacy exceptions and two non-adoptions, retaining all 28 original null adoption fields. No new exception is requested. The two disputed highest observations retain both sources and require clarification before a semantic correction. No upstream message or issue is submitted.

The 38 proposed disposition records below preserve all 37 C02-C07 source records plus the separate C10 source-consistency question. Overlaps remain visible. Original statuses, owners, required behavior and technical exit criteria are retained verbatim in [the full review JSON](FINAL-CLOSURE-REVIEW.json); its 48 unmet batch observations remain false.

| Record | Subject | Proposed disposition category |
|---|---|---|
| C02-F01 | Keyword metadata and label compatibility | Open technical follow-up |
| C02-F02 | Current-draft declaration adoption boundaries | Open technical follow-up |
| C02-F03 | Option-key and value validation | Open technical follow-up |
| C02-F04 | URI Boolean options and empty-query precedence | Open technical follow-up |
| C02-F05 | Sort default-on-empty and collation/record validation | Open technical follow-up |
| C02-F06 | Exact numeric and NaN ordering across sort/extrema | Open technical follow-up |
| C02-F07 | Map merge/build options and input conversion | Open technical follow-up |
| C02-F08 | Atomic/callback/cardinality conversion boundaries | Open technical follow-up |
| C02-F09 | Deep-equal 4.0 map options | Open technical follow-up |
| C02-F10 | Gregorian construction precision and input validation | Open technical follow-up |
| C02-F11 | Mandatory hash algorithms capability | Open technical follow-up |
| C02-F12 | CSV current-index contract adoption | Open technical follow-up |
| C02-F13 | Full grapheme segmentation capability | Open technical follow-up |
| C02-F14 | XML/map conversion plans and options | Open technical follow-up |
| C03-T01 | Generic integer representation and range/error policy | Documented implementation or context boundary |
| C03-T02 | Decimal precision and numeric consumers | Documented implementation or context boundary |
| C03-T03 | Schema list/union/namespace contexts | Documented implementation or context boundary |
| C03-T04 | Nominal/structural/default/static record context | Documented implementation or context boundary |
| C03-T05 | Function signatures, focus capture and BC propagation | Documented implementation or context boundary |
| C03-F08-atomic-annotation-empty | C02 independently reproduced required-argument defect | Open technical follow-up |
| C04-G01-otherwise-spelling | Current OtherwiseExpr/id-otherwise uses otherwise. | Open technical follow-up |
| C04-G02-mapping-containing-focus | U =!> F(A...) is defined by for $u in U return F($u,A...), preserving containing focus for extra arguments. | Open technical follow-up |
| C04-G03-keyword-placeholders | KeywordArgument uses Argument, which includes ArgumentPlaceholder. | Open technical follow-up |
| C04-G04-late-binding-boundary | Guarding does not waive static errors in a closed static context. | Reviewed host distinction; remaining complete-mode boundary |
| C04-G05-remaining-combinations | Production/semantic/context evidence must match claimed support. | Bounded review and final acceptance obligation |
| C05-K01-generic-jnode-model | The current generic model includes JNodes in node(), supports xnode() for XML and node identity. | Documented implementation or context boundary |
| C05-K02-choice-matching-alternative | If an input already matches an alternative, coerce to the first alternative it matches before trying conversions to other alternatives. | Open technical follow-up |
| C05-K03-record-destructuring-lookup | Map/record destructuring uses lookup semantics; an undeclared field on an annotated record raises XPTY0004. | Open technical follow-up |
| C05-K04-host-context | Host, effective version, type context and declaration/package evidence must match the claimed contract. | Bounded review and final acceptance obligation |
| C05-K05-remaining-combinations | Per-production/current-contract evidence remains necessary; supported examples are not universal conversion or grammar proofs. | Bounded review and final acceptance obligation |
| C06-H01 | Complete static-error detection mode evidence | Open technical follow-up |
| C06-H02 | Switch incomparable atomic values lose general-comparison error | Open technical follow-up |
| C06-H03 | Host loader and current F&O adoption boundaries | Open technical follow-up |
| C06-H04 | Unaccepted host/module/instruction combinations | Bounded review and final acceptance obligation |
| C07-L01 | Per-contract semantic and applicable evidence acceptance | Bounded review and final acceptance obligation |
| C07-L02 | Historical adoption and external rename provenance | Previously approved historical policy |
| C07-L03 | Deferred and partial feature boundaries | Bounded review and final acceptance obligation |
| C10-highest-source-consistency | ranking-published-example | Unresolved specification-source inconsistency |

Approval covers the proposal and all 38 attached catalog dispositions; technical statuses remain open wherever their original exit criteria are unmet. Follow-up implementation requires a separately scoped change with immutable source pins, independent regressions and required gates. Skip investigations, Braid and extension work remain deferred until C13 is actually closed.

Decision requested: **accept this bounded-catalog proposal and its dispositions**, or **require specified technical corrections before acceptance**. The closure plan explicitly requires owner acceptance; continuation instructions and passing tests are not recorded as approval.

[Validation and exact scope](FINAL-CLOSURE-REVIEW.md) · [Machine-readable proposal](C13-ACCEPTANCE-PROPOSAL.json) · [Closure plan](CLOSURE-PLAN.md)
