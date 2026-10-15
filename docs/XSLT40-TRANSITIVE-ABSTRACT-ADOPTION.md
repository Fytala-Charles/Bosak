<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak transitive abstract optional function adoption">
  <br><br><h1>Transitive abstract function adoption</h1>
  <p>Bounded XSLT 4.0 implementation, 15 October 2026</p>
</div>

## Delivered contract

Unpublished source 1.7.0 supports one optional abstract function family across a three-package chain: A declares `p:f#3` (required `a`, optional `b` and `c`), B explicitly accepts it as abstract, and C supplies the concrete override. Principal calls, calls originating in A or B, reduced/full arities, arrows, keywords, named references and partial applications reach C's replacement. Named/partial items returned from A or B retain that binding after the library function returns. Replacement defaults, private helpers and globals execute in C's declaring scope; public keywords retain A's parameter names.

Abstract declarations remain absent from package-local dynamic lookup in A and B. Principal dynamic lookup returns the concrete replacement. Private A helpers are not exported; explicit hidden adoption and default-hidden adoption cannot be overridden (XTSE3060). An accepted unresolved abstract in the top-level package raises XTSE3080, including acceptance without an explicit arity; invoking a hidden absent implementation through A's wrapper raises XTDE3052.

Compatibility checks retain XTSE3070 for minimum-arity/type/return-type mismatches and XTSE3058 for an incorrect upper-arity identity. References to the abstract original raise XTSE3075 during Compile, including uncalled function bodies, defaults, named/partial items, dead branches and inline function bodies. The principal-package exposure validator continues to reject attempts to expose an abstract declaration as concrete (XTSE3010).

## Implementation

`Stylesheet` implements component selection and binding:

- Abstract declarations participate in the component surface while dynamic lookup keeps its existing abstract filter.
- Adopted abstract components carry the original declaration and their containing-package visibility separately. Override matching and explicit acceptance can resolve that view; hidden views are never exported as callable functions.
- Concrete function contributions are forwarded through an adoption edge only when that edge accepts the matching abstract component as abstract. Local declarations and hidden/private edges preserve their bindings.

The runtime lookup interceptor distinguishes replacements contributed to a package from overrides declared in that package. A concrete override declared in an explicit principal package remains its own dynamic lookup target; a used library still resolves its own public original when another package contributes a replacement.

General public-component re-export, intermediate concrete override chains and arbitrary adoption graphs are not established by this slice. No public API is added. The authoring classifier, runtime public API and conformance gate membership are unchanged.

## Explicit principal-package follow-up

Final review added five cases: four call paths from an explicit principal `xsl:package` and a public-original lookup control in the declaring library. The earlier candidate passed four and failed one of these additions: principal dynamic lookup incorrectly selected the abstract original. The runtime correction preserves both the containing package's own override and the declaring library's public original.

Final repeated verification passes **8,465/0/0** solution tests (XSLT **5,579/0/0**), language server **84/0/0**, and unchanged frozen gates **31,142/0/679** and **8,278/0/1,894**.

The final fixture has 45 cases. Against the unchanged merged baseline it passes five controls and fails 40 feature cases; the combined selection has 167 cases. [Supplemental correction evidence](XSLT40-TRANSITIVE-PRINCIPAL-PACKAGE-FIX.json) preserves these results separately from the 40-case implementation receipt below. Both implicit stylesheet packages and explicit principal packages are covered. No escaping-inline-closure behavior is inferred from these function-item tests.

## Verification and preserved baselines

The original 40-case fixture passes four controls and fails 36 feature probes against exact merged candidate `d6b984cb41ba3bfbb2a0d4aab6c8a162f5f2e6ab` (unpublished 1.7.0). These results are distinct from the earlier eight-probe 1/7 scope baseline. The four existing controls cover the direct path, reduced-upper-bound rejection, hidden absent execution and principal abstract-exposure validation. A public-1.6.0 run of this final fixture is not claimed.

The original 40-case implementation verification passed **8,460/0/0** solution tests, including XSLT **5,574/0/0**, plus language server **84/0/0**. Frozen gates remain QT3 **31,142/0/679** and qt4 gate-only **8,278/0/1,894**.

[Implementation evidence](XSLT40-TRANSITIVE-ABSTRACT-ADOPTION.json) records full-suite/gate results and fixture/source/log/TRX hashes. Earlier baseline and intermediate attempts remain under `.guard-tmp/transitive-implementation-*`; original scope and direct-override receipts are retained unchanged. Exact committed-revision package metadata and CI evidence are recorded with the implementation review PR and `.guard-tmp/transitive-implementation-delivery.json`.

## Boundaries and specification

The [XSLT 4.0 acceptance and binding rules](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 3.4.5.2-3.4.5.5, permit the explicit abstract adoption used here. [Original scope](XSLT40-TRANSITIVE-ABSTRACT-ADOPTION-SCOPE.md).

Escaping inline closures, overlapping adopted/local families, package acquisition and editor package signatures remain separate. This slice does not broaden general used-package exposure validation. REQ-126 stays In Progress. Source 1.7.0 remains unpublished; public libraries and VS Code 0.2.1's engine bundle remain 1.6.0. No upstream test submission, gate promotion or release tag change occurs here.
