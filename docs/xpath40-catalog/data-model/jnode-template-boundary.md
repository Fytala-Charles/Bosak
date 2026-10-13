<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: partial JNode model and invoice navigation">
  <br><br><h1>JNode model and template boundary</h1>
  <p>Deferred remainder; partial Frozen 4.0</p>
</div>

## 1. Name and Classification

**JNode model and template boundary — Deferred remainder; partial Frozen 4.0.** Shipped delivery remains [PR #116](https://github.com/Fytala-Charles/Bosak/pull/116), [revision 345ae58e](https://github.com/Fytala-Charles/Bosak/commit/345ae58e5f8e35d1b504aa0a7df250e3eaf88dd0). Scope review: 2026-10-12, engine 1.1.2, [revision 3c0bd599](https://github.com/Fytala-Charles/Bosak/commit/3c0bd5994baf4fa85b1db5805fc5c081d63c50ee). This reconciles the [original full-model exclusion](../../REQ-118-xpath-xslt-40.md) with partial delivered support.

Primary references: [XDM generic nodes](https://qt4cg.org/specifications/xpath-datamodel-40/Overview.html#generic-nodes), [XPath JNode types](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-jnode-types), [jtree](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-jtree), [jkey](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-jkey), [jvalue](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-jvalue), [XSLT built-in rules](https://qt4cg.org/specifications/xslt-40/Overview.html#built-in-rule). Immutable review sources: [XDM XML at e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-datamodel-40/src/xpath-datamodel.xml), section `generic-nodes`, [function catalog](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml), and [XSLT source](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml). The review pin does not recover the missing historical adopted specification snapshot.

## 2. Intent

Use shipped JSON navigation while keeping the broader generic-node type, identity and stylesheet contracts explicit.

## 3. Also Known As

GNode means a generic node, collectively an XML node (XNode) or JSON wrapper node (JNode) in the reviewed draft. A plain map/array, a Bosak JNode wrapper and an IXdmNode XML adapter remain distinct values.

## 4. History / Supersedes

REQ-118 originally excluded the full parallel model. REQ-123 subsequently delivered XdmJNode, jtree/jkey/jvalue, jnode type tests and navigation. That supersedes blanket wording that all JNode support is out of scope. The full unified model and JNode-specific stylesheet contracts remain uncertified. This review updates the original dossier rather than promoting the remainder.

## 5. Motivation

A partner sends invoices as JSON. The application navigates array entries and nested keys to produce currency-labelled export rows. Parent context can help navigation, but consumers must not assume that XML-oriented node tests and identity operators already apply to every wrapper.

## 6. Language Alignment

| Contract | Verified scope |
|---|---|
| Explicit jtree + child navigation + jvalue | Main, empty and reordered invoice reports work in Frozen 4.0 |
| Plain map/array lookup | Portable 3.1 report produces the same rows for these inputs |
| jkey on array children | 1-based positions `1|2` |
| Parent navigation | Returns the enclosing invoice map; root parent empty |
| jnode() test | Tested wrapper matches |
| node() test on JNode | `false`; does not realize the draft's generic-node test |
| xnode() on parsed XML | `XPST0051` in 3.1, 4.0 and experimental probes |
| JNode identity with `is` | `XPTY0004`, even when both operands are the same wrapper |
| Attribute axis on JNode | Empty in the tested case |
| jvalue on a plain map | `XPTY0004`; unwrap only wrappers |

Successful 4.0 cases retain the harness's default-3.1 refusal check. Error probes assert the declared marker, not a stage-specific diagnosis. This is scoped evidence, not exhaustive model conformance.

## 7. Applicability

Choose 4.0 navigation when the host opts in and the wrapper operations meet the task. Retain portable map/array lookup for 3.1 integration. For stylesheet processing, unwrap JSON values and pass ordinary maps/arrays or construct XML explicitly; certify any additional template/model operation separately.

## 8. Structure

The pinned model introduces a collective generic-node concept with node identity and order; `node()` covers GNodes and `xnode()` distinguishes XML nodes. Bosak stores JNode values separately from IXdmNode XML values. A wrapper records its value, entry key and parent; the root has no key/parent. Navigation support alone does not establish the common type and identity operators.

```xpath
let $tree := jtree(map{'invoice':map{'id':'INV-1042'}})
return jvalue($tree/invoice/id/parent::*)?id
```

This is a complete parent-value operation; it is not an identity comparison or stylesheet rule dispatch.

## 9. Participants

The host selects XPathCompatibility.XPath40. XdmJNode stores wrapper value/key/parent; the runtime navigates wrappers and the standard library creates/unwraps them. The invoice report formats scalar results. XML stylesheet source adapters remain a separate public integration surface.

## 10. Collaborations

The 4.0 expression parses the full JSON, wraps the batch, navigates invoices and array members, then unwraps each invoice map. The portable expression traverses the same input by lookup. Both preserve invoice array order and keep currency with each total. They do not test universal equivalence of wrapper and map semantics.

## 11. Consequences

The shipped feature is useful and should no longer be described as wholly absent. Confirmed broader gaps are node()/xnode() type alignment and JNode `is` identity. Default XSLT JNode rules, full generic-node ordering/union behavior and mixed-model operations are not certified here. No wrapper identity or stable map-key order is inferred from equal report strings.

## 12. Implementation

Reviewed source: [XdmJNode.cs](https://github.com/Fytala-Charles/Bosak/blob/3c0bd5994baf4fa85b1db5805fc5c081d63c50ee/src/Bosak.XPath.Core/Xdm/XdmJNode.cs) stores Value/Key/Parent, while [VmEngine.cs](https://github.com/Fytala-Charles/Bosak/blob/3c0bd5994baf4fa85b1db5805fc5c081d63c50ee/src/Bosak.XPath.Runtime/Vm/VmEngine.cs) has JNode navigation/type branches but `EvaluateNodeComparison` requires IsNode operands. This explains the tested identity refusal. [TransformEngine.cs](https://github.com/Fytala-Charles/Bosak/blob/3c0bd5994baf4fa85b1db5805fc5c081d63c50ee/src/Bosak.Xslt/Runtime/TransformEngine.cs) and XML-oriented source APIs require separate review before claiming all draft JNode built-in template rules.

Named existing evidence in [JNodeTests.cs](https://github.com/Fytala-Charles/Bosak/blob/3c0bd5994baf4fa85b1db5805fc5c081d63c50ee/tests/Bosak.XPath.Standard.Tests/JNodeTests.cs): `Nav_ParentRoundTrip_ReturnsMapJNode`, `Nav_ChildOfArray_PositionKeyedJNodes`, `InstanceOf_Bare_MatchesAnyJNode`, `Nav_AttributeAxisOnJNode_IsEmpty`. Existing gate membership in [ConformanceRunner40.cs](https://github.com/Fytala-Charles/Bosak/blob/3c0bd5994baf4fa85b1db5805fc5c081d63c50ee/tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs) is `fn-jtree`, `fn-jkey`, `fn-jvalue`; membership is not a full generic-model gate. No gate is added or rerun in this documentation-only slice.

The [manifest harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) adds eighteen checks, retaining the earlier `jnode-template-boundary-navigation` fixture. No engine/API/package changes.

## 13. Sample Code

Complete invoice input (totals are already calculated; no tax or exchange conversion):

```json
{"invoices":[{"id":"INV-1042","currency":"EUR","total":30},{"id":"INV-1043","currency":"USD","total":12}]}
```

Complete Frozen 4.0 navigation report:

```xpath
(: Partner invoice batch: navigate JNodes, unwrap each invoice map for reporting. :)
let $tree := jtree(parse-json('{"invoices":[{"id":"INV-1042","currency":"EUR","total":30},{"id":"INV-1043","currency":"USD","total":12}]}'))
return string-join($tree / invoices / * ! (let $invoice := jvalue(.) return concat($invoice?id, '/', $invoice?currency, ':', format-number($invoice?total, '0.00'))), ';')
```

Complete portable 3.1 alternative:

```xpath
(: Portable 3.1: plain map/array lookup, without JNode identity or parent context. :)
let $batch := parse-json('{"invoices":[{"id":"INV-1042","currency":"EUR","total":30},{"id":"INV-1043","currency":"USD","total":12}]}')
return string-join($batch?invoices?* ! concat(?id, '/', ?currency, ':', format-number(?total, '0.00')), ';')
```

Both return exactly:

```text
INV-1042/EUR:30.00;INV-1043/USD:12.00
```

Empty input:

```json
{"invoices":[]}
```

Both complete [navigation](../examples/jnode-scope/navigation-empty.xpath) and [portable](../examples/jnode-scope/portable-empty.xpath) expressions return the empty string. Reordered input:

```json
{"invoices":[{"id":"INV-1043","currency":"USD","total":12},{"id":"INV-1042","currency":"EUR","total":30}]}
```

The complete [navigation](../examples/jnode-scope/navigation-reordered.xpath) and [portable](../examples/jnode-scope/portable-reordered.xpath) expressions return:

```text
INV-1043/USD:12.00;INV-1042/EUR:30.00
```

Confirmed broader identity boundary:

```xpath
let $tree := jtree(map{'id':'INV-1042'}) return $tree is $tree
```

Result: `XPTY0004`. Complete node-type probes:

```xpath
jtree(map{'id':'INV-1042'}) instance of node()
```

Result: `false`.

```xpath
parse-xml('<invoice/>') instance of xnode()
```

Result: `XPST0051`.

All new fixtures are linked below; output uses JSON string notation for empty values. The earlier [navigation fixture](../examples/jnode-template-boundary/navigation.xpath) still returns `R1` at mode 40.

| Fixture | Mode | Exact output or marker |
|---|---|---|
| [navigation-main](../examples/jnode-scope/navigation-main.xpath) | 40 | `"INV-1042/EUR:30.00;INV-1043/USD:12.00"` |
| [portable-main](../examples/jnode-scope/portable-main.xpath) | 31 | `"INV-1042/EUR:30.00;INV-1043/USD:12.00"` |
| [navigation-empty](../examples/jnode-scope/navigation-empty.xpath) | 40 | `""` |
| [portable-empty](../examples/jnode-scope/portable-empty.xpath) | 31 | `""` |
| [navigation-reordered](../examples/jnode-scope/navigation-reordered.xpath) | 40 | `"INV-1043/USD:12.00;INV-1042/EUR:30.00"` |
| [portable-reordered](../examples/jnode-scope/portable-reordered.xpath) | 31 | `"INV-1043/USD:12.00;INV-1042/EUR:30.00"` |
| [array-keys](../examples/jnode-scope/array-keys.xpath) | 40 | `"1|2"` |
| [parent-value](../examples/jnode-scope/parent-value.xpath) | 40 | `"INV-1042"` |
| [root-parent](../examples/jnode-scope/root-parent.xpath) | 40 | `"0"` |
| [missing-key](../examples/jnode-scope/missing-key.xpath) | 40 | `"0"` |
| [type-jnode](../examples/jnode-scope/type-jnode.xpath) | 40 | `"true"` |
| [type-node](../examples/jnode-scope/type-node.xpath) | 40 | `"false"` |
| [attribute-axis](../examples/jnode-scope/attribute-axis.xpath) | 40 | `"0"` |
| [identity](../examples/jnode-scope/identity.xpath) | 40 | `XPTY0004` |
| [xnode-type-31](../examples/jnode-scope/xnode-type-31.xpath) | 31 | `XPST0051` |
| [xnode-type-40](../examples/jnode-scope/xnode-type-40.xpath) | 40 | `XPST0051` |
| [xnode-type-experimental](../examples/jnode-scope/xnode-type-experimental.xpath) | experimental | `XPST0051` |
| [wrong-jvalue](../examples/jnode-scope/wrong-jvalue.xpath) | 40 | `XPTY0004` |


## 14. Performance Notes

Both report paths materialize parsed JSON. Wrapper navigation retains parent context; the portable alternative reads ordinary maps/arrays. No timing, memory bound, streaming or allocation equivalence is asserted.

## 15. Known Uses

The partner invoice report is a realistic integration example, not evidence of a named deployment. The [order-routing chapter](../worked-examples/order-routing/README.md) already demonstrates ordinary map/array-to-stylesheet integration; that path is not proof of JNode built-in template handling.

## 16. Related Features

[JNode navigation](jnode-navigation.md), [external-input boundaries](../deferred/external-input-functions.md), [catalog index](../README.md), [supplementary audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md) and [completeness audit](../COMPLETENESS-AUDIT.md).
