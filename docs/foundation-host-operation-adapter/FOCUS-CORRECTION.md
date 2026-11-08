# Correction: source kind and XSLT invocation focus

1 November 2026. Later diagnostic evidence; earlier receipts remain historical and unchanged.

The [31 October host characterization](../foundation-c45-domains-hosts/README.md) used an XElement-backed XDocumentNode for its caught named-template focus control. Its observation was valid for that input. The statement that a supplied source does not supply named-template focus was too broad when read as applying to all node kinds. The [revised proposal](../foundation-c45-revised-shape/README.md) still needs separate match/global inputs, but that separation cannot be justified by assuming every source kind gives the same focus result.

| Actual 1.36.0 call in this fixture | Observed result |
|---|---|
| Public Transform, document source A, named main, raw data(.) | A |
| Public Transform, element source A, same stylesheet and template | Caught XPDY0002 |
| Internal ten-parameter TransformCaptured, document source A, null explicit global input | A |
| Adapter with document match A and explicit document global input B | A\|B, from initial match plus global variable |
| Adapter, named template with only global input B | B from the global variable |
| Adapter, source-free named template returning constant | 42 |
| Adapter, source-free named template evaluating data(.) | Caught XPDY0002 |

The pinned TransformEngine source explicitly computes `effectiveGlobalContextItem = globalContextItem ?? source`. A stylesheet declaring global context absent has a separate branch. An ordinary null argument therefore cannot express independent per-invocation absence while retaining a match source in this bridge. This is an integration gap against the new proposal, not a claim that existing legacy behavior is a specification defect.

The bounded adapter refuses a match input without an explicit global input rather than claiming that it implements independent absence. It also refuses unsupported arbitrary item selection. Future integration needs an explicit presence/absence policy in the engine-owned bridge, with regression coverage preserving legacy fallback. Resolve stylesheet-declared absent/required context and broader item/sequence invocation semantics from pinned XSLT specification sections before changing production behavior; no such semantic adoption or engine patch occurs here.
