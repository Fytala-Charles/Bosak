<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak optional function override scope">
  <br><br><h1>Optional function override scope</h1>
  <p>Proposed bounded follow-up to Bosak 1.5.0, 15 October 2026</p>
</div>

## Current status

The bounded public-original implementation is delivered in unpublished source 1.6.0: see [implementation and verification](XSLT40-OPTIONAL-OVERRIDES.md). The initial scope and checkpoints below are retained as review history. Abstract optional originals, range conflicts and authoring package acquisition remain separate.

## Initial decision and scope

Start with one used package and one public optional function family. Establish executable baseline cases before changing engine behavior. This was the proposed scope before implementation; its bounded delivery is documented above. Published libraries remain 1.5.0, REQ-126 remains In Progress and all existing unsupported boundaries remain in force.

The preceding [Braid verification](BRAID-1.5.0-COMPATIBILITY.md) passes 836 automated tests against public 1.5.0 packages in an isolated consumer adaptation. It does not adopt a new production Braid pin or request an editor package-acquisition API.

## Contract review

Reviewed the [XSLT 4.0 draft](https://qt4cg.org/specifications/xslt-40/Overview.html), dated 13 October 2026, on 15 October 2026. Function override compatibility requires matching arity ranges, parameter/return types and applicable declaration properties. Defaults may differ; keyword binding retains the overridden parameter names. An incompatible signature raises XTSE3070. `xsl:original` supports static calls, partial application and named references, stays unavailable to dynamic lookup, and is invalid for an abstract original (XTSE3075). An original function item may escape its override and remain callable. These constraints come from sections 3.4.5.3–3.4.5.4; implementation choices below are proposed Bosak work.

## Code findings

| Area | Observed implementation | Proposed investigation |
|---|---|---|
| Package compilation | `Stylesheet.cs` explicitly rejects optional families in override processing and validation | Replace each guard only after baseline and compatibility validation exist |
| Signature validation | `ValidateFunctionOverrideSignature` documents the older fixed-arity contract | Audit minimum/maximum arity, declaration metadata and parameter-name handling |
| Original registration | `RegisterXslOriginalFunctions` registers declaration upper arities | Determine how the existing host signature machinery should represent callable ranges |
| Original invocation | `ExecuteXslOriginalFunction` checks `original.Arity == args.Length` and uses an execution stack | Audit default completion and function-item binding; a stack-only target needs investigation for references invoked after the override returns |
| Function execution | `ExecuteXsltFunction` already supplies ordinary optional defaults | Reuse its behavior after proving the correct declaring scope and caller/creation focus reach it |

The two rejection sites, runtime registration and execution paths must be assessed together. Removing the guards alone is insufficient evidence of support. These findings describe the pre-implementation source; the implementation document records the resulting changes.

## Delivery checkpoints

1. Preserve a published-1.5.0 baseline and fixture hashes. Reproduce the current refusal using the public API and package registry. Inventory existing fixed-arity override regressions before adding cases.
2. Build a focused matrix for reduced/full calls, changed defaults, keyword names, private helpers and declaring globals; cover original calls, partials and escaping references. Include caller-focus and nested evaluation cases to expose dispatch mistakes.
3. Separate signature diagnostics from unsupported graph shapes. Keep ambiguity, multi-family overlaps, broader package conflicts and editor signature acquisition outside the initial implementation. Audit transitive packages so the boundary cannot be bypassed.
4. Implement only the proved scope, retaining fixed-arity and earlier-profile behavior. Use a minor source version for new functionality; preserve release tag v1.5.0. Update headers and canonical docs alongside any code change.
5. Require the happy and failure matrix, complete solution/language-server tests, QT3 exactly 31,142/0/679 and zero qt4 gated failures. Keep gate membership unchanged. Recheck a fresh package consumer before proposing another release.

This is an engine-owned continuation under existing REQ-126. It does not register a new Braid requirement or claim a Braid dependency on the proposed feature. No upstream submission is needed to begin local regression work. Any later upstream test correction/addition must use freshly fetched current upstream, preserve original receipts and record its exact base revision.
