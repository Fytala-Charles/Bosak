<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: deferred capturing accumulators and invoice snapshots">
  <br><br><h1>Capturing accumulators</h1>
  <p>Deferred — attribute acceptance does not create a snapshot</p>
</div>

**C09 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-1.md) records 3 contract groups with exact sample/context, applicable evidence and language-alignment limits. Status: **Verified unsupported-proposal disposition**. Capture semantics, option validation and actual capture streamability remain unsupported; catalog cases are in-memory. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**Capturing accumulators — Deferred.** [REQ-118 tier-4 decision](../../REQ-118-xpath-xslt-40.md) records the deferral. Bosak reviewed 2026-10-12 at [revision 7c827ace](https://github.com/Fytala-Charles/Bosak/commit/7c827ace77b917c130bba81f8599984ce54ab809), engine 1.1.2. This is a disposition reference, not implementation of capture semantics.
External reference: [XSLT 4.0 capturing accumulators](https://qt4cg.org/specifications/xslt-40/Overview.html#capturing-accumulators). Immutable review source: [QT4CG xslt.xml at e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml), ID `capturing-accumulators`, issue 211 / PR 717 (2023-09-29). This review does not recover the historical adopted specification pin.

## 2. Intent

Retain an invoice subtree after visiting it, so a later report can inspect its line children without depending on the original streamed node.

## 3. Also Known As

End-phase accumulator capture. It is distinct from ordinary accumulator values, explicit `snapshot()`, result capture through `CaptureTransform`, and authoring snapshots.

## 4. History / Supersedes

The reviewed draft evaluates an end-phase capturing rule against a snapshot copy of a matched element, excluding accumulator values from that copy. It applies in both streamed and ordinary evaluation; non-element matches are unaffected. Bosak's deferral is not superseded by accepting the attribute or by implementing ordinary accumulators/snapshots.

## 5. Motivation

A batch exporter might retain the latest invoice for deferred line inspection. On an in-memory tree, retaining the original node can look correct because its children remain available. Node identity distinguishes that behavior from an actual copied subtree, preventing an in-memory pass from being mistaken for capture support.

## 6. Language Alignment

| Tested form | Observed Bosak outcome |
|---|---|
| `phase="end" capture="yes"`, stylesheet 4.0 or 3.0 | Accumulator value is the original invoice (`is` returns true) |
| Capture absent or `no` | Same source-node identity |
| Invalid capture value, or start-phase capture, at version 4.0 | Tolerated; same source-node identity |
| Ordinary 3.0 accumulator without capture | Retains the source node as declared by `select="."` |
| Explicit `snapshot(.)` in the template | New node identity (`is` returns false), line children preserved |
| Scalar ordinary counter | Three visited line elements in the fixture |

The draft capture rule must use end phase, and capture would change the identity of an element-context copy. The observed ignored-attribute cases are limitations against that contract, not draft conformance. No stream execution is tested by these catalog fixtures.

## 7. Applicability

Use an ordinary accumulator for scalar counts or source-node references on an in-memory tree. Use an explicit snapshot when an independent copied node is needed in this verified in-memory workflow. If only ID/line counts are needed, project those values instead of retaining a subtree. None of these alternatives proves streamable capture or the draft's special exclusion of accumulator annotations.

## 8. Structure

An ordinary accumulator stores the latest invoice at end phase. The invoice template retrieves its after-value and reports ID, identity and line count. A separate snapshot alternative copies the invoice in the template. A projection alternative reports ID/count directly without retaining XML.

## 9. Participants

[AccumulatorDefinition](../../../src/Bosak.Xslt/Stylesheet/AccumulatorDefinition.cs) parses rule match/select/phase; it does not model the capture option. [TransformEngine](../../../src/Bosak.Xslt/Runtime/TransformEngine.cs) walks source nodes and applies ordinary rules using their source-node focus. [FunctionLibrary](../../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs) supplies ordinary snapshot behavior. [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) compiles exact files and runs in-memory capture.

## 10. Collaborations

`use-accumulators`, `accumulator-after`, `snapshot`, node identity `is`, `count` and ordered template application support the example. `CaptureTransform` captures principal result bytes; its name does not indicate capturing-accumulator implementation.

## 11. Consequences

The capture attribute cannot be used as a support detector. Tests with accessible source descendants alone would miss the gap; the identity assertions expose it. Explicit snapshots allocate copies and may carry ordinary accumulator semantics; this example does not verify the capture rule's no-accumulator-values exception. Keeping many snapshots can retain substantial memory. The report projects fixture values and does not validate invoice data.

## 12. Implementation

`AccumulatorRule.FromElement` reads match/select/phase without capture; `ComputeAccumulatorValues` visits source nodes and `ApplyAccumulatorRule` evaluates against that focus. The successful invalid/start-phase probes do not establish draft option/phase validation. Registered-but-unknown accumulator access raises XTDE3341 in the targeted fixture; an accumulator excluded by the mode raises XTDE3362. These are ordinary accumulator boundaries. No engine changes or gate promotions are included.

## 13. Sample Code

Complete [batch input](../examples/capturing-accumulators/input.xml):

```xml
<batch><invoice id="INV-2048" currency="EUR"><line sku="P100"/><line sku="P200"/></invoice><invoice id="INV-2049" currency="USD"><line sku="P300"/></invoice></batch>
```

Complete [capture identity probe](../examples/capturing-accumulators/main-observed.xsl):

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify deferred capturing accumulators and in-memory invoice alternatives.
SPECIAL NOTES: Capture attribute acceptance is not snapshot support; no streaming claim.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: batch/invoice id and line children. Output: exact identity/line-count report.
The retained ordinary accumulator value is a source node. Explicit snapshot creates
a new node; it is not the capture rule's special no-accumulator-values contract.
All catalog cases use ordinary in-memory CaptureTransform, not TransformStreaming.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0">
  <xsl:output method="text"/>
  <xsl:accumulator name="latest" initial-value="()">
    <xsl:accumulator-rule match="invoice" phase="end" capture="yes" select="."/>
  </xsl:accumulator>
  <xsl:mode use-accumulators="latest"/>
  <xsl:template match="/">
    <xsl:apply-templates select="batch/invoice"/>
  </xsl:template>
  <xsl:template match="invoice">
    <xsl:variable name="last" select="accumulator-after('latest')"/>
    <xsl:value-of select="concat(@id, ':source=', $last is ., ':lines=', count($last/line), ';')"/>
  </xsl:template>
</xsl:stylesheet>
```

Observed principal text output:

```text
INV-2048:source=true:lines=2;INV-2049:source=true:lines=1;
```

This is original-node retention, not the draft-required copy. The [version 3.0 probe](../examples/capturing-accumulators/capture31.xsl) has the same observed result; the [ordinary 3.0 form](../examples/capturing-accumulators/ordinary31.xsl) removes the capture attribute and honestly expresses source retention.

### Explicit snapshot alternative

Complete [XSLT 3.0 snapshot stylesheet](../examples/capturing-accumulators/snapshot31.xsl):

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify deferred capturing accumulators and in-memory invoice alternatives.
SPECIAL NOTES: Capture attribute acceptance is not snapshot support; no streaming claim.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: batch/invoice id and line children. Output: exact identity/line-count report.
The retained ordinary accumulator value is a source node. Explicit snapshot creates
a new node; it is not the capture rule's special no-accumulator-values contract.
All catalog cases use ordinary in-memory CaptureTransform, not TransformStreaming.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
  <xsl:output method="text"/>
  <xsl:template match="/">
    <xsl:apply-templates select="batch/invoice"/>
  </xsl:template>
  <xsl:template match="invoice">
    <xsl:variable name="copy" select="snapshot(.)"/>
    <xsl:value-of select="concat(@id, ':source=', $copy is ., ':lines=', count($copy/line), ';')"/>
  </xsl:template>
</xsl:stylesheet>
```

Output:

```text
INV-2048:source=false:lines=2;INV-2049:source=false:lines=1;
```

Line children survive the copy and identity differs. This snapshot is made in an ordinary template using an in-memory source; it does not enable the deferred rule or certify streaming memory behavior.

### Scalar projection alternative

Complete [XSLT 3.0 projection](../examples/capturing-accumulators/projection31.xsl):

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify deferred capturing accumulators and in-memory invoice alternatives.
SPECIAL NOTES: Capture attribute acceptance is not snapshot support; no streaming claim.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: batch/invoice id and line children. Output: exact identity/line-count report.
The retained ordinary accumulator value is a source node. Explicit snapshot creates
a new node; it is not the capture rule's special no-accumulator-values contract.
All catalog cases use ordinary in-memory CaptureTransform, not TransformStreaming.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
  <xsl:output method="text"/>
  <xsl:template match="/">
    <xsl:apply-templates select="batch/invoice"/>
  </xsl:template>
  <xsl:template match="invoice">
    <xsl:value-of select="concat(@id, ':lines=', count(line), ';')"/>
  </xsl:template>
</xsl:stylesheet>
```

Output:

```text
INV-2048:lines=2;INV-2049:lines=1;
```

### Empty and reordered input

Complete empty input:

```xml
<batch/>
```

The capture, snapshot and projection empty fixtures return the empty string. Complete reordered input:

```xml
<batch><invoice id="INV-2049" currency="USD"><line sku="P300"/></invoice><invoice id="INV-2048" currency="EUR"><line sku="P200"/><line sku="P100"/></invoice></batch>
```

Snapshot output:

```text
INV-2049:source=false:lines=1;INV-2048:source=false:lines=2;
```

### Complete fixture inventory

The [manifest](../examples/cases.json) executes 16 cases through the in-memory catalog harness. Errors verify compile/capture markers, without promising a universal detection stage.

| Fixture | Mode | Exact output/error marker |
|---|---|---|
| [main-observed](../examples/capturing-accumulators/main-observed.xsl) | xslt40 | `INV-2048:source=true:lines=2;INV-2049:source=true:lines=1;` |
| [capture-no](../examples/capturing-accumulators/capture-no.xsl) | xslt40 | `INV-2048:source=true:lines=2;INV-2049:source=true:lines=1;` |
| [capture-absent](../examples/capturing-accumulators/capture-absent.xsl) | xslt40 | `INV-2048:source=true:lines=2;INV-2049:source=true:lines=1;` |
| [invalid-capture-observed](../examples/capturing-accumulators/invalid-capture-observed.xsl) | xslt40 | `INV-2048:source=true:lines=2;INV-2049:source=true:lines=1;` |
| [start-capture-observed](../examples/capturing-accumulators/start-capture-observed.xsl) | xslt40 | `INV-2048:source=true:lines=2;INV-2049:source=true:lines=1;` |
| [capture31](../examples/capturing-accumulators/capture31.xsl) | xslt31 | `INV-2048:source=true:lines=2;INV-2049:source=true:lines=1;` |
| [capture-empty](../examples/capturing-accumulators/capture-empty.xsl) | xslt40 | `` |
| [ordinary31](../examples/capturing-accumulators/ordinary31.xsl) | xslt31 | `INV-2048:source=true:lines=2;INV-2049:source=true:lines=1;` |
| [snapshot31](../examples/capturing-accumulators/snapshot31.xsl) | xslt31 | `INV-2048:source=false:lines=2;INV-2049:source=false:lines=1;` |
| [snapshot-empty](../examples/capturing-accumulators/snapshot-empty.xsl) | xslt31 | `` |
| [snapshot-reordered](../examples/capturing-accumulators/snapshot-reordered.xsl) | xslt31 | `INV-2049:source=false:lines=1;INV-2048:source=false:lines=2;` |
| [projection31](../examples/capturing-accumulators/projection31.xsl) | xslt31 | `INV-2048:lines=2;INV-2049:lines=1;` |
| [projection-empty](../examples/capturing-accumulators/projection-empty.xsl) | xslt31 | `` |
| [ordinary-counter](../examples/capturing-accumulators/ordinary-counter.xsl) | xslt31 | `3` |
| [unknown-accumulator](../examples/capturing-accumulators/unknown-accumulator.xsl) | xslt31 | `XTDE3341` |
| [not-applicable](../examples/capturing-accumulators/not-applicable.xsl) | xslt31 | `XTDE3362` |

## 14. Performance

An ordinary node-valued accumulator retains source nodes. Snapshot copies require memory for copied content; retaining snapshots affects lifetime. A scalar projection avoids retained subtree copies. No capturing-accumulator benchmark, bounded-memory guarantee or streaming certification is claimed.

## 15. Known Uses

This invoice case is synthetic integration evidence. Existing [StreamingAccumulatorTests](../../../tests/Bosak.Xslt.Tests/StreamingAccumulatorTests.cs) include `CounterAccumulator_Parity`, `SumWithAccumulatorAfter_Parity`, `UnknownAccumulator_ThrowsXTDE3341` and `InapplicableAccumulator_ThrowsXTDE3362` for ordinary accumulator paths. [StreamingSnapshotTests](../../../tests/Bosak.Xslt.Tests/StreamingSnapshotTests.cs) includes `SnapshotDocumentNode_ParityWithInMemory` and `SnapshotShellRoot_KeepsDocumentParent`. These named streaming tests are separate evidence, not capture-rule conformance or catalog streaming tests. No gate membership changes are included.

## 16. Related Features

[Enclosing modes](enclosing-modes.md), [computed XPath constructors](computed-node-constructors.md), [completeness audit](../COMPLETENESS-AUDIT.md) and [supplementary audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md).
