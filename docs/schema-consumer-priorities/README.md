# Native Schema coverage and consumer priorities — 29 October 2026

Recommendation: qualify the existing engine against a real persistent-node consumer before adding more native assertion grammar. The reviewed Schema 0.9/v5 slice is complete within its bounds. No inspected consumer dependency establishes an immediate need for sum/average/not, broader Boolean comparisons or more decimal-only predicates.

## Current evidence

| Consumer | Source observation | Consequence |
|---|---|---|
| XmlDb | Xdm project references Core exactly [1.26.2]; PersistentXdmNode implements IXdmNode, returns a string TypedValue, uses snapshot-relative identity and disposal checks. Storage/Core are separate projects. | Qualify actual provider behavior and lifetime before claiming current-core compatibility. This untyped XML consumer does not require native XSD assessment or typed persistence. |
| Braid | Infrastructure imports an immutable source dependency revision 6a3372fc3b91e733b71a426cf40f26feaaeb3bcb, references XSLT, and uses authoring inspection and preview compilation. | Preserve authoring/resource/preview contracts. More native decimal assertions do not establish Braid compatibility. |
| Schema candidate | Native assessor accepts a finite no-target-namespace, anonymous-root decimal sequence; original state/copies and bounded count/presence predicates are tested. | Broader structural coverage and component-universe acceptance remain separate from grammar convenience. |

Inspected heads: XmlDb 3f722daf6ae38f0a5b67de1c678f3175768b28dd, Braid 0f1d949e758645f7944f617f2b4613e49161a195, Schema 4f71de9d3f2e878683d25c0dfb1f1fc6f83f8bbd. [Source identities](evidence/consumer-source-identities.json) record selected file hashes and working-tree observations. Heads alone do not identify dirty file contents. Only paths/hashes and assessment conclusions are retained; sibling source and licensing material are not copied. This is read-only source evidence, not new consumer execution or conformance certification.

## Ranked work

| Priority | Slice | Evidence needed before completion |
|---|---|---|
| 1 | Isolated XmlDb source/provider compatibility qualification | Actual Xdm adapter built against current core in a disposable workspace, differential queries over equivalent XML, identity/order/atomization and disposal observations. Original pins/store remain unchanged. |
| 2 | Isolated Braid source compatibility qualification | Actual source suite and authoring/preview contracts against current engine; unchanged-binary and native host acceptance remain distinct later gates. |
| 3 | Native Schema structural demand inventory | Obtain a concrete public/owner-authorized schema+instance fixture and expected typed-consumer operation. Rank namespaces, named types/facets, nondecimal values and component closure by that fixture rather than inventing demand. No specific structural feature is adopted yet. |
| 4 | Additional assertion functions | Admit only a demonstrated use case with immutable specification adoption, complete grammar bounds, exact expectation migration and native validation/copy regressions. |

Full schema-session API acceptance, compatible-universe handoff, C45 and product releases remain separate decisions. Historical compatibility documents predate the Apache/public transition; their licensing assumptions are not current engine requirements.

## Next slice: isolated XmlDb qualification

1. Read the consumer AGENTS.md before any copied-source execution. Pin current core and XmlDb heads plus selected working-tree source hashes. If XmlDb contains uncommitted edits, explicitly choose committed HEAD for the isolated qualification and record that those edits are excluded. Preserve the original checkout and package references.
2. Prepare an ignored disposable workspace under the core repository. Build the actual committed Xdm/Core/Storage projects with an isolated dependency override to current core. Do not patch production consumer source to make the first build green. Preserve compiler errors and distinguish compatibility failures from environment/restore limitations.
3. Run relevant existing consumer tests and an externally bounded differential probe over identical XML through the persistent and in-memory providers. Cover element/attribute/text atomization, comparisons/arithmetic/errors, XML node-kind values, axes/order/duplicate elimination, repeated wrappers and distinct snapshots. Record both outcomes, including baseline discrepancies; do not label every difference a new regression.
4. Check snapshot disposal before and during deferred enumeration and nested node access; verify the caller retains ownership. Include independent evaluations and cancellation where supported; do not infer concurrent disposal safety.
5. Report source compatibility, observable behavior and lifetime separately. Any finding gets a minimal reproduction and a separately adopted fix plan. Do not upgrade the actual package pin, change disk format, add schema persistence, promise unchanged-binary compatibility or certify all XPath 4.0 semantics.
6. Sync canonical documentation and retain exact source/test receipts. No consumer build or probe has been run in this assessment; the six steps are the next task.

## Qualification and delivery

Production source, versions, dependency pins and prior sealed dossiers are unchanged in this documentation-only step. Schema 451 passes and core 24,214 passes / QT3 31,397/0/424 / qt4 8,941/0/1,918 (230 sets) are inherited from reviewed source-identical checkpoints, not rerun. This assessment adds no specification semantics or REQ completion. It remains uncommitted; next action is the isolated qualification above.
