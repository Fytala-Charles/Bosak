# Selected bounded original-node profile

The first grammar accepts only the trimmed complete strings `nilled($node)` and `fn:nilled($node)`, with the standard builtin fn binding or an explicitly captured fn binding to `http://www.w3.org/2005/xpath-functions`. A foreign fn binding refuses before assessor Bind. Internal whitespace variants, comments, arbitrary aliases, EQNames, zero-argument calls, focus, paths, operators, data and constructors are outside this first grammar. This finite grammar is not a claim of general XPath syntax coverage.

Compilation through existing `SchemaQuerySession.CompileXPath` requires a non-null supported assessor to select this new profile. Existing no-assessor behavior remains unchanged. Capture/freeze supported CompileOptions with the established policy; identify the canonical operation as fn:nilled, include all five explicit limits and the immutable static-context fingerprint, exact-source/facts policy, no-typed-read policy, copied optional-Boolean result, no-cache policy and refusal mapping in the separate descriptor. Preserve all existing identity/numeric/ordinary descriptor meanings.

Evaluation requires exactly one present, unprefixed `node` variable, no focus and no extra variables, services, runtime namespace bindings or populated execution state. The value is a direct node or explicitly bound `XdmValue.Undefined` as empty sequence. A missing variable is not empty input. Sequence wrappers and arbitrary enumerable values refuse without enumeration. Extend capture only under this explicit profile; do not globally admit Undefined or node semantics to the scalar identity route.

| Input | Required access/proof | Result |
|---|---|---|
| Explicit empty binding | Current raw operation and selected borrowed advisor; no source/node/facts getter | `XdmValue.Undefined` |
| Direct non-element node | Exactly one current admitted source owner before NodeKind; valid known node kind; annotated-facts channel untouched | `XdmValue.Undefined` |
| Original annotated element | Selected original assessment, facts issuance and exact node/facts/original-input/origin tuple; metadata/type ownership and lifetime checks | Plain copied Boolean from original IsNilled |
| Element missing annotated facts capability/original assessment | No fabricated type or nilled facts | `NotSupportedException` |
| Foreign/ambiguous owner, null/unissued/copied facts, wrong origin/type/declaration, malformed node kind or contradictory facts | Reject before relevant getters or delivery | `ArgumentException` under existing invalid-evidence policy |

Resolve exactly one owner across the operation and identify the selected snapshot before reading NodeKind or OriginalSchemaFacts. For elements, read the annotated-node property under source/binding/lifetime guards, then authenticate issuance/tuple before any facts property. Non-elements require no original assessment or facts channel; absence of element facts never establishes an untyped false result. Do not read legacy IXdmNode.IsNilled, string type annotations or xsi:nil attributes as authority.

A metadata-only reader checks original governing type, optional declaration, origin and independent nilled/no-value flags without ReadTypedValue, result Kind/Value, container, iterator, actual-type relation or resource access. Nilled plus absent typed value remains malformed under the existing facts consistency policy. The view retains full proof/currentness checks and expires at raw completion; the returned Boolean/empty value is copied data and remains readable. No scalar-cache slot or persistent facts cache is created. Repeated public queries may reread metadata; record their cost explicitly rather than assuming a persistent cached view.

Unsupported compile syntax/options or provider binding use explicit Unsupported outcomes before activation; unsupported input/capability during Evaluate throws NotSupportedException. Existing cancellation, disposal, invalid evidence, acquisition rollback and ordered cleanup behavior remains. These boundaries do not promise a hard time/provider-graph budget.

Atomization is deliberately later: data requires decisions and controls for ordered marked atomic/list outputs, cache accounting, result lifetime and provenance. Absent typed value must ultimately raise FOTY0012 in an atomization profile; it must never be encoded as empty. This nilled profile does not expose data or infer its support from the existing scalar ReadTypedValue method.
