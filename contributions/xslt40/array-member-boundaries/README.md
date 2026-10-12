# First upstream batch: array-member boundaries

Six native tests are committed locally on bosak/array-member-review at upstream contribution revision `e72f1b0e8511f78478e66c112201317cfe7b7ad1`, based on xslt40-test `6205faf9f32ace6338ac245c41f8eb9a1447926f`. [Portable patch](0001-array-member-boundaries.patch), [exact evidence](evidence.json), and [upstream description draft](UPSTREAM_PR_DRAFT.md) preserve the work outside the ignored checkout. No upstream push/PR or maintainer message has been sent.

## Source and audit

Expected outcomes derive from the [pinned XSLT e7ee4ab8 array-construction rules](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml) ([rendered section](https://qt4cg.org/specifications/xslt-40/Overview.html#array-construction)). An array-member wraps its entire sequence value, including an empty sequence. Without for-each, the array has one member per emitted item, unwrapping JNode values and retaining order. With for-each, there is one member per selected input item, with its corresponding item/position/size focus. A member result mixing a JNode with another item raises XTTE4060.

The existing native array set has 31 baseline cases. array-004/-006 already cover ordinary grouped sequences, -003/-014/-016 cover nesting, -007/-008 map values with grouping, -025 the JNode result and -026 JNode-based array construction. The broader new-element footprint includes 35 distinct baseline stylesheets, with related array/copy/variable/type cases. These additions target explicit slot cardinality, empty versus no emission, empty nested containers, original-node identity and deliberate source reorder, empty per-item results plus full focus, and the current mixed-result error. They are not simple copies of the grouped-array case. This is a focused review, not certification that no equivalent assertion exists anywhere in the suite or external tests.

The addition changes no established test-case or expected result. It adds one environment, six cases and seven files in tests/insn/array; the root catalog already registers this group.

## Real-world fixture

Two cases use a tiny order-export source:

```xml
<order><line sku="A"/><line sku="B"/><line sku="C"/></order>
```

The source deliberately contains only the fields needed to expose the boundaries. array-030 groups the original C and A line nodes, then B, so identity and order are observable. array-031 emits one member per line, leaves B empty and includes position/last in the other values. Empty output preserves B's slot rather than silently changing row positions. These fixtures do not define pricing, currency or a production order schema.

## Cases and normative outputs

### array-027

An explicit empty member occupies a slot, while an empty sequence emits no member.

Native stylesheet: tests/insn/array/array-027.xsl, included in the patch. Expected result:

```xml
<out members="3" empty="true" last="B" emitted="2" second="B"/>
```

Bosak outcome: **pass**. Native catalog assertion passed. See [array-027 result](array-027-result.txt) and [receipt](array-027-receipt.json).

### array-028

An array-member sequence constructor returning no items still creates an empty member.

Native stylesheet: tests/insn/array/array-028.xsl, included in the patch. Expected result:

```xml
<out members="3" empty="true" grouped="2" values="after|tail"/>
```

Bosak outcome: **pass**. Native catalog assertion passed. See [array-028 result](array-028-result.txt) and [receipt](array-028-receipt.json).

### array-029

Nested map and empty/nonempty array values remain singleton members without flattening.

Native stylesheet: tests/insn/array/array-029.xsl, included in the patch. Expected result:

```xml
<out members="3" map="true" sku="A" lots="0" nested="true" nested-size="2" nested-second="C" empty-array="0" empty-array-is-item="1"/>
```

Bosak outcome: **pass**. Native catalog assertion passed. See [array-029 result](array-029-result.txt) and [receipt](array-029-receipt.json).

### array-030

Order-export members retain original source-node identity and explicit non-document order.

Native stylesheet: tests/insn/array/array-030.xsl, included in the patch. Expected result:

```xml
<out members="2" grouped="2" first-same="true" second-same="true" last-same="true" order="C|A"/>
```

Bosak outcome: **pass**. Native catalog assertion passed. See [array-030 result](array-030-result.txt) and [receipt](array-030-receipt.json).

### array-031

The for-each form retains empty members and evaluates select with per-line focus.

Native stylesheet: tests/insn/array/array-031.xsl, included in the patch. Expected result:

```xml
<out members="3" first="A|1|3" empty="true" last="C|3|3"/>
```

Bosak outcome: **pass**. Native catalog assertion passed. See [array-031 result](array-031-result.txt) and [receipt](array-031-receipt.json).

### array-032

A for-each result mixing an array-member JNode with an atomic item raises XTTE4060.

Native stylesheet: tests/insn/array/array-032.xsl, included in the patch. Expected error:

```text
XTTE4060
```

Bosak outcome: **fail**. Required XTTE4060 is not raised; Bosak returns <out>A|B</out>. See [array-032 result](array-032-result.txt) and [receipt](array-032-receipt.json).

## Execution evidence and limits

Exactly five of the six new cases pass in Bosak; array-032 fails. Its projection consumes values without an XML/JSON serialization error masking the required construction error: Bosak returns <out>A|B</out> instead of raising XTTE4060. The required error remains in the test. This is an engine follow-up, not a change to the expected result.

All six were rerun from the clean committed suite tree. Native catalog XML validates against the suite schema. Independent processor confirmation remains outstanding; source-derived assertions and Bosak evidence are distinguished explicitly.

The adapter's filters are substrings. Selecting array-031 also runs sx-square-array-031 and square-array-031. Portable receipts retain honest whole-run counts; the 5/1 batch tally comes from the exact six PASS/FAIL lines and must not be reconstructed from those aggregate totals. Result extracts contain only the selected case and expected/actual details. Earlier exploratory logs remain ignored local artifacts.

## Reproduce or review

The existing local checkout is already on the contribution commit. Run:

```powershell
pwsh -File ./validate-xslt40-catalog.ps1 -Files tests/insn/array/_array-test-set.xml
pwsh -File ./run-xslt40-tests.ps1 -TestSet array -TestCase array-027 -NoBuild
pwsh -File ./run-xslt40-tests.ps1 -TestSet array -TestCase array-032 -NoBuild
```

The last command should exit 1 with the recorded current gap. Review all six cases individually rather than assuming the full family passes.

On a separate clean suite checkout at the recorded base, apply the mail patch with git am; use a new branch and ensure the base matches:

```powershell
git switch -c bosak/array-member-review 6205faf9f32ace6338ac245c41f8eb9a1447926f
git am D:/Development/Bosak/contributions/xslt40/array-member-boundaries/0001-array-member-boundaries.patch
```

Do not apply the patch a second time to the existing contribution branch. The patch's tree has been checked against the committed source, and its SHA-256 is recorded in evidence.json.

## Next decision

Review the six assertions with another XSLT 4.0 implementation when available and triage Bosak's missing mixed-result error separately. Then submit a small upstream test PR with honest pass/failure evidence. No established expected outcome is changed, so this is an addition rather than an expected-result correction. The local PR text includes a short Bosak/documentation link without implying complete 4.0 conformance.

Queue N01 remains in progress; do not move it to upstream accepted. Source/public version stays 1.1.3 and REQ-126 remains In Progress. No Bosak engine/API/adapter changes or release in this batch.


## Post-fix candidate review

The [assertion review](ASSERTION_REVIEW.md) documents the expected-output derivations and engine correction. [Post-fix results](POST_FIX_RESULTS.json) record six exact passes against the unchanged native commit. Earlier outcomes above are preserved as the before-fix record. Source candidate 1.1.4 is unpublished; independent processor confirmation and upstream submission remain outstanding.


## Submission update

[Upstream PR #22](https://github.com/qt4cg/xslt40-test/pull/22) is open. See [submission details](SUBMISSION.md). Earlier no-submission statements describe the pre-submission state; independent confirmation remains outstanding.
