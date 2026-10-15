<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak abstract optional override implementation">
  <br><br><h1>Abstract optional function overrides</h1>
  <p>Bounded XSLT 4.0 implementation, 15 October 2026</p>
</div>

## Delivered contract

Unpublished source 1.7.0 supports a direct concrete override of one abstract used-package function family with trailing optional parameters. Reduced and full calls, named items, dynamic lookup and keywords dispatch to the replacement. Keywords retain the abstract declaration's parameter names; defaults and private helpers execute in the replacement's declaring scope, including calls originating in the library.

The existing signature validator retains XTSE3070 for incompatible required/total arity, parameter/return types, determinism and applicable streamability. The original upper arity continues to identify the component (XTSE3058). An explicitly accepted unresolved abstract function still raises XTSE3080 in the principal compilation.

A reference to an abstract original raises **XTSE3075 during `XsltCompiler.Compile`**, even if the overriding function is never called. Checks cover calls, named references, partial applications, arrows, inline function bodies, defaults and dead branches. Namespace aliases and EQNames resolve to the XSLT namespace. XPath literals/comments, literal portions of string templates, escaped AVTs and dynamic lookup strings do not constitute references. XPath-bearing attributes, result-attribute AVTs, instruction AVTs and expanded text are checked before evaluation.

## Implementation

`ValidateFunctionOverrides` replaces the previous abstract-optional refusal with static reference validation after compatibility checking. `AbstractOriginalReferenceValidator` uses the shared attribute-slot vocabulary and XPath lexer to distinguish actual function-reference syntax from text, resolve names at the containing element and inspect template interpolations before optimization or execution. Linking overrides directly to the matching original declaration preserves keyword metadata even when the abstract declaration is absent from the exported callable view. No public API is added.

## Verification

The 36-case regression fixture runs against published 1.6.0: **4 pass / 32 fail**. The four passing cases are existing type/minimum-arity/upper-bound compatibility errors and explicitly accepted unresolved-abstract behavior. The final candidate package-only consumer passes **110/0/0** across the new fixture and the retained public-override/used-package fixtures. Its nine engine dependencies come from the isolated candidate feed; archive, cache and test-output DLL bytes match. Original attempts and corrections remain under `.guard-tmp/abstract-optional-*`; the final receipt identifies accepted runs separately.

The source version is 1.7.0; published libraries remain 1.6.0 and VS Code 0.2.1 bundles 1.6.0. Solution **8,408/0/0**, XSLT **5,522/0/0**, language server **84/0/0**; QT3 **31,142/0/679** and qt4 gate-only **8,278/0/1,894**. Results are recorded in [the evidence receipt](XSLT40-ABSTRACT-OPTIONAL-OVERRIDES.json).

## Boundaries

Transitive abstract adoption and escaping inline closures remain separate checkpoints. Overlapping adopted/local families, package acquisition and editor package signatures remain deferred. REQ-126 stays In Progress; no conformance test-set promotion, upstream submission or publication occurs in this slice. Earlier scope and release receipts are preserved.

## Specification

The [XSLT 4.0 draft, overriding components](https://qt4cg.org/specifications/xslt-40/Overview.html#overriding-components), reviewed on 15 October 2026, defines XTSE3075 for references to abstract originals. [Original bounded scope](XSLT40-ABSTRACT-OPTIONAL-OVERRIDE-SCOPE.md).
