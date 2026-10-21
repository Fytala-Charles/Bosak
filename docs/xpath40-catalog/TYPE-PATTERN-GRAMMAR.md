<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Types and structured-value pattern grammar">
  <h1>Types and structured-value pattern grammar</h1>
  <p>21 October 2026</p>
</div>

**Compound-pattern follow-up supplied — 21 October 2026:** [New bounded mapping](COMPOUND-PATTERN-GRAMMAR.md) now covers set/union grouping, priority and rule identity, with a separately labelled current-draft union-priority gap. Original counts/queues below describe their earlier checkpoint.


This bounded REQ-126 D3 inventory documents existing public **1.23.2** behavior. It distinguishes stylesheet aliases, structural annotation, named-record identity and map/array/record pattern syntax. No engine, API, C#, harness, version, host default or gate membership changes are included.

PR #402 is reviewed and merged as `9ddf46b0b5232cc69ea845014eb252ef6a7ee925` after exact-head CI39633664285 passed10,368 solution +84 language-server tests. Its switch/array/select slice remains intact. This next slice adds40 manifest checks; all2,673 prior rows remain unchanged. Catalog totals become **70 entries;2,713 feature +24 chapter checks =2,737**, with four worked chapters and117 assigned function names unchanged.

## Complete intake example

An [intake report](examples/intake-types/README.md) turns namespaced XML orders into line maps or bundle arrays, sorts unique order IDs, and emits one explicit ship/bundle/review decision per order. Portable3.0 code uses a named template and explicit predicates; the4.0 version uses an item-type alias and structured-value template patterns. Normal/reordered input includes a zero-quantity line and an empty bundle; both are reviewed. Empty input emits `orders=0`. Six complete reports are paired across versions. Inputs are synthetic; line quantities must parse as integers. This is projection/classification, not schema validation or an industry logistics format.

## Current primary source and delivery provenance

qtspecs HEAD was freshly checked at `3230573afe246f1b502ed2e22d187ce9928155b1`. [Immutable XSLT source](https://raw.githubusercontent.com/qt4cg/qtspecs/3230573afe246f1b502ed2e22d187ce9928155b1/specifications/xslt-40/src/xslt.xml) SHA-256: `6d3038e21291abf761d883dd371cb9885972e0dd1c85b41844df70ca1ede45b8`. Reviewed section IDs: named-item-types, named-record-types, id-map-patterns, id-record-patterns, id-array-patterns and pattern-syntax. This current review pin does not recover historical adopted specification pins or retroactively change the REQ-118 dossier's tier decisions.

| Source delivery | Immutable implementation/test revision |
|---|---|
| Enumeration aliases in XPath type tests | f53bc80f4270a04e12cc0c88a32d4a087373b3d3 |
| Structural record aliases |669c862a42963b98c2d7bb90385e9a8227a23b00 |
| Map entry sequence patterns |55689f8c1fd2d27dfad4bc478f7a4cc0d23405f7 |
| Array member patterns |49717bdc573d3102c00d2da576fb691b058c39f4 |
| Named record patterns |2244c1d71753cbe8d3323aa4a2907c32b80817ee |

These revisions identify delivered source, not the original specification adoption. Three older catalog references and their accumulated implementation logs are preserved as [historical snapshots](../history/REQ126-ITEM-TYPE-SNAPSHOT-20261021.md), with original Git-blob hashes in the [audit](TYPE-PATTERN-GRAMMAR.json). Current pages keep their existing paths so consumer links remain valid. In particular, the `deferred/` path is historical and is not an accurate blanket statement of present XSLT support.

## Grammar positions must not be conflated

| Position / surface | Reviewed contract | New manifest evidence (`intake-types-` prefix) |
|---|---|---|
| `xsl:item-type name='t:status' as="enum('ready','queued')"` | Names an existing type in the stylesheet context; it is not a new nominal identity or generated record constructor. EQNames resolve expanded names. | enum-allowed, enum-rejected, enum-treat-error, alias-expanded-name |
| `as='t:shape'`, shape=`record(x as xs:integer)` | A binding can convert a plain map to an annotated structural record. A subsequent type test observes that annotation. Testing the original plain map does not perform that conversion. | structural-binding; type-structural-annotated/map |
| `xsl:record-type name='t:point' constructor='yes'` | Generated construction supplies named identity. A different declared name with identical fields does not satisfy t:point; a plain map does not satisfy its noncoercing type test. | nominal-own/other/plain-map/no-constructor |
| `match='~t:shape'` / `match='record(t:point)'` | These are template-pattern forms. Matching tests an existing value and does not construct/coerce it. Structural shape and nominal identity remain distinct. | type-structural-annotated/map; type-nominal-own/other |
| `match="{'v':.}"` / `{'v':.,*}` | String-key map patterns are closed unless opened by the trailing wildcard. A value subpattern checks sequence cardinality and each permitted item. Numeric key1 is not string key'1'. | map-closed, map-extra-closed/open, map-numeric-key |
| `{'v':.?}` / `{'v':.+}` | The key must exist even when an empty value is allowed. The occurrence applies to that entry value, not map presence. | map-empty-present/missing, map-singleton-required, map-sequence-allowed |
| `array(atomic(xs:integer)+)` | Each array member is checked as a sequence. This pattern grammar differs from an XPath array sequence type. '+' constrains each member, not the number of members. | array-singletons, array-whole-sequence, array-member-plus, array-wrong-type |
| Empty array versus empty member | An empty array vacuously passes an every-member test; `[()]` contains one empty sequence and can fail a singleton/plus requirement. Explicit business nonempty guard is separate. | array-vacuous, array-empty-member, array-member-star, array-not-map; report4 empty-bundle guard |

## Version and host boundary

The new manifest runner uses the default **Legacy host**. xslt31/xslt40 labels describe stylesheet versions; they are not explicit host selections. In this context, a3.0 item-type declaration raises XTSE0010 and3.0 map/array patterns raise XTSE0340. Successful4.0 examples use frozen XPath40. Existing [ProcessorProfileTests](../../tests/Bosak.Xslt.Tests/ProcessorProfileTests.cs) supply the separate host/effective-version model. No new exhaustive Draft-host, module, package or authoring-context matrix is claimed.

Additional invalid declaration controls verify missing @as (XTSE0010) and duplicate item-type names (XTSE4030); an enum treat mismatch raises XPDY0050 and a disabled generated constructor lookup raises XPST0017. The capture-based manifest checks the diagnostic text, not exact detection timing. No expectation is silently weakened to accommodate a new engine failure.

## Named source/test evidence

[Stylesheet](../../src/Bosak.Xslt/Stylesheet/Stylesheet.cs) registers named types and applies declaration validation; [PatternCompiler](../../src/Bosak.Xslt/Patterns/PatternCompiler.cs), [StringKeyMapPattern](../../src/Bosak.Xslt/Patterns/StringKeyMapPattern.cs), [ArrayPattern](../../src/Bosak.Xslt/Patterns/ArrayPattern.cs) and [RecordPattern](../../src/Bosak.Xslt/Patterns/RecordPattern.cs) provide pattern parsing/matching. Existing tests run with the full solution:

- [Xslt40ItemTypeTests](../../tests/Bosak.Xslt.Tests/Xslt40ItemTypeTests.cs): Enumeration_MembersAndCardinality, Enumeration_NamespaceRebinding, Enumeration_InvalidDeclarations.
- [Xslt40StructuralRecordAliasTests](../../tests/Bosak.Xslt.Tests/Xslt40StructuralRecordAliasTests.cs): RecordAlias_DeclarationNamespacesAndFunctions, RecordAlias_InlineTypeChecks, RecordAlias_NestedTypes.
- [Xslt40MapEntryPatternTests](../../tests/Bosak.Xslt.Tests/Xslt40MapEntryPatternTests.cs): SequenceValues, OuterPredicates, PrioritiesAndNextMatch; [string-key map tests](../../tests/Bosak.Xslt.Tests/Xslt40StringKeyMapPatternTests.cs) distinguish key presence and open/closed maps.
- [Xslt40ArrayPatternTests](../../tests/Bosak.Xslt.Tests/Xslt40ArrayPatternTests.cs): Matching, Profile and existing priority controls.
- [Xslt40RecordPatternTests](../../tests/Bosak.Xslt.Tests/Xslt40RecordPatternTests.cs) and [Xslt40TypePatternTests](../../tests/Bosak.Xslt.Tests/Xslt40TypePatternTests.cs): named identity, structural annotation and existing malformed/priority cases.

New40-case counts overlap the full solution. These are XSLT instruction/pattern examples, not new XPath qt4tests GatedSets members. Native/upstream review remains independent; no submission or independent-processor acceptance is inferred.

## REQ-126 reconciliation and next boundary

| Area | Established here | Remains open |
|---|---|---|
| D3 | Descriptive positions for aliases, structural/named records, type/map/array patterns; Legacy3.0/4.0 controls; existing source mapping | Other grammar and patterns, compound/set/union combinations, complete priorities, declaration/module/package/authoring contexts |
| D4 / AC-01 | Current review pin, delivery revisions and preserved old snapshots | Historical adopted-spec pins and exhaustive delivery/decision reconciliation |
| AC-03 | Runnable examples and applicable XSLT source evidence, with explicit lack of new gate membership | The literal every-entry qt4tests acceptance treatment remains unresolved; no waiver |
| AC-04 | One new16-field pattern entry and refreshed existing alias/record references; older logs moved out of current entry prose | Remaining feature-specific drift/lifecycle review |
| D2 | No new closure | Full function/default/options contracts, including separately scoped deep-equal options |

REQ-126 stays **In Progress**. A suitable next D3 slice maps compound/set/union pattern grammar and priority boundaries against existing tests; inventory comes before any engine decision. No broad conformance, consumer acceptance or feature-tier promotion is claimed here.

**Validation:** Release build passes with the pre-existing conformance Program.cs:1868 CS8604 warning. All10,408 solution tests pass, including7,440 XSLT tests; focused40/0/0 on the first run. Counts overlap. The audit retains all ten TRX hashes, fixture hashes and immutable source pin. All70 entries retain16 fields; new local links/XML/JSON validate. Engine manual gates are not rerun for this documentation/fixture-only diff.
