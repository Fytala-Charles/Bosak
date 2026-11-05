# K04-C27 - Context resources and imported type constraints

28 October 2026. Audit complete against `a061ea52`. **Admission is not qualified.** Candidate **1.30.0** remains unpublished; public **1.26.3**. No production source, routing, dependency guard, fixture/assertion, baseline, package version or release changes.

C26 qualified 31 resource-free originals. This audit exercises the remaining **13 original QT3 cases**, including ten module-mapped cases and three resource-only cases, with their unchanged assertions and actual source/schema/module files. Actual parsed resource paths and hashes are recorded. The copied complete QT3 harness differs only by an isolated context-routing regex edit and its history row. Parsing, environments, eager/guarded schema decisions, registered module sources, comparisons and every other guard retain normal code. Its engine DLLs match the normal DLLs byte-for-byte. Normal repository policy is untouched.

## Fresh bounded outcomes

| Scope | Passed / failed / skipped | Result |
|---|---|---|
| Thirteen originals through current direct executor | **4 / 0 / 9** | Existing four module cases pass; nine remain guarded |
| Same originals through isolated context-routing executor | **11 / 2 / 0** | Only contextDecl-056 and contextDecl-057 fail |
| Complete normal QT3 context-declaration set | **4 / 0 / 40** | Identical retained skip mapping |
| Complete isolated QT3 context-declaration set | **42 / 2 / 0** | All 44 original assertions execute; the two failures remain |
| Complete normal qt4 context-declaration set | **0 / 0 / 53** | XPath-only applicability exclusions retained |
| Independent namespace/type controls | **8 / 4 / 0** | Four failures establish remaining correction boundaries |

The prototype set exits **2**, the runner's documented failed-assertion status. This failure exit and both original failure messages are retained. Successful JSON-probe process exits are separate from assertion outcomes. All **56 final bounded diagnostic processes** finish without timeout: 39 original/resource probes, 12 controls, three complete sets and two routing/filter inventories. Initial control/set receipts are preserved separately. Orchestration now recognizes the real failed-assertion exit code instead of requiring a green prototype.

## Confirmed findings

Both original failures report **XPST0081: Prefix 'u' is not declared**. That prefix belongs to the imported library's schema-type declaration. Adding the same binding to the importer in synthetic diagnostic copies makes both original expressions/assertions pass, with unchanged original resource files. This establishes the missing declaration-context boundary; repeating a library's prefixes in the importer is not an adoption fix.

The compiler currently stores library constraints as bare `ContextItemTypeName` strings and deduplicates them by ordinal text. The executable matches them against the importing runtime context. Namespace bindings and the declaring module's default type namespace are not carried with each constraint.

A separate control demonstrates an unsound success: two libraries declare the same lexical `t:choice` under different namespace URIs. Their union types respectively accept integer 4 (decimal/string) and reject it (date/time). The combined query should raise XPTY0004 but succeeds. A positive first-union witness passes, and a distinct-prefix version raises the expected error. This confirms that lexical-text identity can lose a required module constraint.

Three aliased built-in type controls also fail with XPTY0004, including one whose importer repeats the correct alias. A declaration-context correction must resolve built-in aliases correctly as well as preserve module namespaces. Some simpler negative controls pass on an error and cannot alone prove correct constraint resolution; the positive witnesses and colliding-union control avoid that ambiguity.

The next fix belongs in XQuery constraint resolution. The schema controls use the current XmlSchemaSet path and existing union constructs. Bosak.Schema retains ownership of XSD 1.1 work; this audit changes no backend or capability declaration.

## Routing and evidence limits

Two actual filter/routing passes cover **111 rows each**: all 97 context-declaration originals plus 14 lexical/guard controls. Dependency decisions remain identical. The prototype changes only the 40 QT3 fixture context-route refusals and three context boundary controls. Construction, copy-namespaces, pragmas, mixed guarded constructs, quoted text and comments retain their expected behavior. These metadata observations do not establish full-catalog admission.

No complete prototype frozen-gate run or global unskip projection is warranted while the original set has two failures. The [routing patch](PROTOTYPE-ROUTING.patch) is a diagnostic artifact, not an owner approval proposal. The [next-slice scope](NEXT-SLICE.md) requires the namespace/identity correction before any later guard review.

C26's **23,822 solution passes**, QT3 gate **31,165/0/656**, qt4 gate **8,939/0/1,920**, 230 gated sets and 11 Experimental passes are inherited source-identical validation. No fresh full-solution or full-gate run is claimed. Fresh work here is the bounded audit and complete affected sets.

[Original receipts](evidence/original-receipts.json), [control receipts](evidence/control-receipts.json), [resource identities](evidence/resource-identities.json), [findings](evidence/findings.json), [results](RESULTS.json), [validator](evidence/validate-evidence.py) and [manifest](evidence/manifest.json) retain all failures and actual fixture/module/environment identities. Ten Markdown pages render with unchanged Fytala CSS; HTML generation is not screenshot/PDF acceptance.

REQ-126 remains Complete under bounded owner acceptance. All historical dispositions and 48 unmet batch observations are preserved, along with every C26 artifact. No upstream fixture correction, publication, push, tag or Braid change is included.
