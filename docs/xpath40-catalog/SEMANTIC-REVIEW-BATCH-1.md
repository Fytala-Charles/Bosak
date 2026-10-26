<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak first nineteen-entry semantic and applicable-evidence review">
  <h1>Semantic and applicable-evidence review — C09</h1>
  <p>26 October 2026 · batch 1 of 4 · published Bosak 1.26.3</p>
</div>

**C09 completes the prescribed 19-entry review, producing 67 contract-group receipts with explicit limitations and blockers. Four closure workpackages remain open.** [The machine report](SEMANTIC-REVIEW-BATCH-1.json) separates documented behavior, applicable evidence, language drift, unsupported scope and independent current-draft expectations. Review completion is not unconditional entry or owner acceptance.

## Results and sample verification

All **3,447 fresh existing tests pass**: 3,148 catalog cases, 182 standard-library tests and 117 selected host tests. The batch explicitly maps **536 distinct catalog cases** to its entries. Each retains exact mode, host/input, expected result/error and baseline metadata. **555 source fixture/input files match their compiled test assets byte-for-byte**, as does the complete case manifest.

The audit checks **116 Sample Code blocks and eight blocks in the explicitly linked equality walkthrough**. Executable text is an exact fixture, a contextual fragment of an executed fixture, or a separate verbatim probe. Displayed outputs match declared case outputs. Three displayed XML inputs match embedded fixture inputs structurally, ignoring only formatting whitespace in those element-only examples; attribute values and significant text remain compared. No general whitespace or XML equivalence is inferred.

The scan page originally combined two independent queries in one code block. They now have separate fences and explicit Experimental context; each query is executed verbatim, including the empty-seed case. The standalone inline XQuery constructor example is also executed verbatim. [Before/after correction](evidence/c09-semantic-batch1/documentation-corrections.json) preserves the original text. Probe-only navigation authoring errors and their corrections remain separate from engine results.

The additional **28 expressions produce 54 profile controls: 46 match and eight remain unmet**. The eight retain four independent requirements under both 4.0 API profiles:

| Requirement | Observed result | Existing ownership |
|---|---|---|
| JNode satisfies generic `node()` | `false` | C05 K01: known unsupported model remainder |
| `compare(min((3.1,3.1e0)),3.1)` returns `0` | `1` | C02 F06: mixed numeric extrema selection |
| Winning integer in integer/decimal minimum retains integer type | `false` | C02 F06: selected-value/type preservation |
| Current draft ignores nonpositive partial-apply keys | Released profile raises `XPTY0004` | C02 F02: frozen versus current-draft adoption boundary |

These expectations remain independent of the released-result observations. They are not rewritten to pass. A passing ignored-option, refusal or observed-gap fixture is not conformance to the unsupported proposal. CSV index/options, deep-equal options maps, broader alias/nominal contexts and earlier C02/C04/C05/C06 defects remain explicit limits.

## Per-entry receipts

| Entry | Receipt | Applicable gate / evidence boundary |
|---|---|---|
| [data-model/jnode-navigation.md](data-model/jnode-navigation.md) | Limited navigation contract; 3 contract groups / 3 linked cases | fn-jtree, fn-jkey, fn-jvalue; Generic node()/xnode()/identity and JNode built-in templates remain unsupported; no full-model acceptance. |
| [data-model/jnode-template-boundary.md](data-model/jnode-template-boundary.md) | Known model blocker; 4 contract groups / 19 linked cases | Named tests/refusals/alternatives; no applicable exact XPath gate; C05-K01: current generic-node membership remains unmet in both4 profiles; full model/template acceptance stays blocked. |
| [deferred/capturing-accumulators.md](deferred/capturing-accumulators.md) | Verified unsupported-proposal disposition; 3 contract groups / 16 linked cases | Named tests/refusals/alternatives; no applicable exact XPath gate; Capture semantics, option validation and actual capture streamability remain unsupported; catalog cases are in-memory. |
| [deferred/computed-node-constructors.md](deferred/computed-node-constructors.md) | Verified refusal and host alternative; 4 contract groups / 30 linked cases | Named tests/refusals/alternatives; no applicable exact XPath gate; No XPath constructor adoption or full XQuery4 grammar. XQuery and XPath parser host flags remain distinct. |
| [deferred/enclosing-modes.md](deferred/enclosing-modes.md) | Verified unsupported-proposal disposition; 3 contract groups / 20 linked cases | Named tests/refusals/alternatives; no applicable exact XPath gate; No draft enclosure diagnostic/package/unused-body certification. |
| [deferred/external-input-functions.md](deferred/external-input-functions.md) | Verified absence and narrower alternatives; 3 contract groups / 55 linked cases | Named tests/refusals/alternatives; no applicable exact XPath gate; No third-party service/package or proposed factory is implemented by those alternatives. |
| [deferred/fn-apply-templates.md](deferred/fn-apply-templates.md) | Verified function refusal and instruction alternative; 3 contract groups / 28 linked cases | Named tests/refusals/alternatives; no applicable exact XPath gate; No proposed fn:apply-templates adoption or complete template-environment certification. |
| [deferred/language-streaming-scope.md](deferred/language-streaming-scope.md) | Limited host and streaming contract; 4 contract groups / 28 linked cases | Named tests/refusals/alternatives; no applicable exact XPath gate; No XQuery4/full4.0 streaming conformance or general bounded-memory guarantee. |
| [deferred/serialization-parameters.md](deferred/serialization-parameters.md) | Limited serialization subset / deferred additions; 4 contract groups / 52 linked cases | Named tests/refusals/alternatives; no applicable exact XPath gate; New serialization additions, CSV method and canonicalization remain unsupported; ignored keys are observations. |
| [deferred/xslt-item-type.md](deferred/xslt-item-type.md) | Limited implemented declaration; 3 contract groups / 40 linked cases | Named tests/refusals/alternatives; no applicable exact XPath gate; No complete alias/schema/package/type-conversion acceptance; C03/C05 limits remain. |
| [deferred/xslt-record-type.md](deferred/xslt-record-type.md) | Limited implemented nominal declaration; 3 contract groups / 40 linked cases | Named tests/refusals/alternatives; no applicable exact XPath gate; Broader constructors/defaults/static/package/type conversion remain C03/C05/C06 scope limits. |
| [experimental/fn-scan.md](experimental/fn-scan.md) | Verified bounded Experimental contract; 4 contract groups / 3 linked cases | fn-scan; Experimental gate retry does not promote frozen API availability; no bounded-memory or general callback-conversion promise. |
| [functions/atomic-equal.md](functions/atomic-equal.md) | Limited atomic equality contract; 3 contract groups / 3 linked cases | fn-atomic-equal; Do not generalize to document/map deep equality or all comparator/type combinations. |
| [functions/batch-extrema.md](functions/batch-extrema.md) | Blocked for full current-draft extrema parity; 4 contract groups / 29 linked cases | Named tests/refusals/alternatives; no applicable exact XPath gate; C02-F06/F08: current mixed-type selection/identity and plain-duration ordering are not fully implemented. Original observed fixtures remain observations. |
| [functions/bounded-retries.md](functions/bounded-retries.md) | Verified finite callback contract; 4 contract groups / 25 linked cases | fn-while-do, fn-do-until; No automatic delay, timeout, retry ceiling or unbounded termination guarantee. |
| [functions/configured-label-callbacks.md](functions/configured-label-callbacks.md) | Limited frozen binding / draft adoption boundary; 4 contract groups / 22 linked cases | fn-identity; C02-F01/F02/F08: ignored nonpositive draft keys remain unmet; metadata-dependent conversion timing is not universally eager. No exact fn-partial-apply gate. |
| [functions/consecutive-partition.md](functions/consecutive-partition.md) | Verified bounded consecutive partition contract; 3 contract groups / 3 linked cases | fn-partition; No arbitrary callback conversion or stable grouping after prefiltering guarantee. |
| [functions/csv-parsing.md](functions/csv-parsing.md) | Limited frozen CSV/options contract; 4 contract groups / 102 linked cases | fn-parse-csv, fn-csv-to-xml, fn-csv-doc; C02-F03/F12: namespaced unknown options and newer index/get policy remain unsupported/not adopted; no remote/schema completeness. |
| [functions/customer-reference-matching.md](functions/customer-reference-matching.md) | Limited equality/collation contracts; 4 contract groups / 58 linked cases | fn-compare, fn-contains-token; C02-F01/F09: no deep-equal options maps, complete UCA/normalization, metadata or full comparator acceptance. Other three fn sets are not gated. |

The JSON links each of the 67 contract groups to selected case IDs, independent probes or named inherited evidence. It preserves full context/results rather than treating a test-set name as a certificate. Entries without an applicable XPath gate explicitly use named XSLT/engine tests, versioned refusals or supported alternatives. No gate is invented or promoted. `fn-scan` membership still uses Experimental retry and does not promote frozen API availability.

## Evidence boundaries and reproduction

TRX display names truncate some long theory arguments. The receipt preserves candidate matches without inventing unique row identities. Complete manifest enumeration, equal case/result counts, all-Passed outcomes and exact compiled-asset identity establish execution of the complete source manifest. This is documented in [the case receipts](evidence/c09-semantic-batch1/applicable-cases.json).

Current review source is immutable `b4723ff52fd9d4fabf2b4cd33bd44f21939c512c`, with [exact function/type section fragments](evidence/c09-semantic-batch1/review-sources.json). It is not a reconstructed adoption pin. C08's approved legacy policy remains limited to its original 28 records and does not waive any semantic defect or extend automatically to other entries.

Engine, tests and executable fixtures remain identical to published source `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f`; documentation base is `34080f04`. C02's **21,798 full solution passes** and C00's frozen QT3 **31,142/0/679** and gated qt4 **8,436/0/1,894** remain inherited evidence. No fresh full-suite or manual gate run is claimed. Catalog totals remain 76 entries and 3,148 feature +24 chapter checks. No engine, version, fixture, filter, profile or publication change.

```powershell
python docs/xpath40-catalog/build-c09-review.py
python docs/xpath40-catalog/evidence/c09-semantic-batch1/validate-evidence.py --local-artifacts
```

[The manifest](evidence/c09-semantic-batch1/manifest.json) protects original source/sample snapshots, exact requests/results, TRX, assets, named source identities, current gate membership and fresh logs. C00–C08 raw evidence and historical plans remain unchanged. HTML rendering uses unchanged Fytala CSS; screenshot/PDF acceptance is not claimed. C10's next 19-entry batch is next; C13 retains final owner acceptance and blocker resolution. Skips, Braid and extension work remain deferred.
