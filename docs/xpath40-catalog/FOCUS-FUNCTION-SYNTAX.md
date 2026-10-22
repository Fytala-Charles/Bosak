<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala focus-function syntax coverage delivery">
  <h1>Focus-function syntax and pipeline focus</h1>
  <p>22 October 2026 - documentation and executable examples</p>
</div>

The [dedicated 16-field entry](operators-and-syntax/focus-function-syntax.md) separates `fn { ... }` / `function { ... }` focus callbacks from explicit-signature `fn(...)` abbreviation and zero-argument constructors. The [complete order-line task](examples/focus-function-syntax/README.md) compares 3.0/4.0 source, exact normal/empty/reordered results and invalid decimal input. It supplies 30 manifest checks without an engine, runner, version or gate change. Existing pipeline prose is corrected: `->` binds the whole value as context; it does not insert a first argument.

## Contract and provenance

Current immutable review pin: `b4723ff52fd9d4fabf2b4cd33bd44f21939c512c`, refreshed 22 October 2026; expressions sections `id-inline-func`, `id-focus-functions`, `id-pipeline-operator` and InlineFunctionExpr grammar. The [entry](operators-and-syntax/focus-function-syntax.md) links the primary XML. This review pin does not recover historical adoption. Existing focus tests are verified in original S4 delivery PR87 / `ec79355936a29ac70eeea3ae47e4c4e2ee00d813`.

A focus callback takes one item()* argument and sets its body focus to that whole value, position1/size1. An empty argument still results in one callback evaluation. Mapping invokes per item; a whole-sequence pipeline evaluates its right expression once. The probes verify these boundaries, caller focus restoration, lexical capture, nested callbacks, compatible 3.1 forms and explicit refusals. Sixteen successful new-syntax probes additionally check the 3.1 refusal; those assertions overlap the 30-case count.

## Evidence and acceptance

All **10,546 solution tests pass**, including **7,578 XSLT tests** and **2,797 documentation cases** (2,773 catalog plus24 chapter checks). All30 new checks pass; every original manifest row is unchanged. Build succeeds with one pre-existing CS8604 conformance-runner warning. Initial focused execution was29pass/1fail: the caller-restoration probe returned a singleton sequence wrapper that the harness displayed as `(sequence)`. Explicit string-join produces the same expected restored scalar value. The original input, failure and final receipt are retained; this was not an engine focus defect. [Machine-readable evidence](FOCUS-FUNCTION-SYNTAX.json) records case IDs, hashes and scope.

No feature-specific qt4 gated-set assignment or promotion is made. Under the owner-approved AC-03 policy the entry explicitly identifies the absent assignment and supplies named engine tests and runnable public examples. This does not certify full draft conformance or automatically close entry acceptance. Manual engine gates are not rerun for the documentation/example-only diff. HTML rendering with unchanged Fytala CSS is checked separately from screenshot/PDF acceptance.

## Reconciliation and next scope

The reconciliation and AC-03 amendment are committed separately at `73403b918cf8955046986ad675792c12b4cece7c`; this slice builds on that revision and the same engine base. [Current inventory](COVERAGE-RECONCILIATION.json) now covers72 entries and2,797 checks, four chapters and117 assigned function names. The [18-row grammar matrix](GRAMMAR-COVERAGE-MATRIX.json) records the supplied focus entry while preserving unclaimed exhaustive callback/type/host behavior. The initial reconciliation receipt and71-entry snapshot remain in Git history and original local evidence.

REQ-126 stays In Progress, with D2/D3/D4 and AC-03 partial. Next bounded D3 scope: record casts/coercion/updates, distinguishing surplus discard from coercion rejection, missing fields, annotation/identity, field order and but-with recoercion. Binary literals and broader host/declaration boundaries remain separate. Source1.23.5 stays unpublished; public/Braid dependency1.23.4 is unchanged. README install/build instructions remain current. No release or consumer acceptance is inferred.
