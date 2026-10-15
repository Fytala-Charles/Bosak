<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala lexical original binding in escaping inline closures">
  <br><br><h1>Escaping inline original functions</h1>
  <p>Unpublished 1.7.1 correctness fix, 15 October 2026</p>
</div>

## Result

An inline function returned by a concrete overriding function retains the lexical `xsl:original` declaration after the override returns. It also retains that declaration when invoked inside a different override. For example, returning `function($v) { xsl:original($v) }` and later calling it with `'A'` produces the original's `'ABC'`, rather than an unresolved-function error or another override's original.

The bounded correction covers reduced/full optional arities, keywords/arrows, nested closures, and original named/partial items created inside an escaped body. Original defaults re-enter their declaring globals/private helpers. The caller's aliases are restored after normal return and exceptions. Captured variables and absent inline-body focus remain unchanged. Dynamic lookup/availability still hide the pseudo-name; an abstract original still raises compile-time XTSE3075.

## Implementation

`InlineFunctionItem` has an internal alias snapshot. `EvaluationContext` snapshots only hidden aliases whose targets have bound function-item identity; ordinary function registrations are not copied. Closures created without hidden bound aliases receive no alias dictionary; any inline item created while such aliases are registered captures them, even when its body does not reference an original. `VmEngine` captures aliases when creating an inline item, restores them only for that item's body, and restores the caller's aliases in `finally`. No public API is added.

This deliberately avoids capturing the entire package/function registry. General package graph capture, arbitrary ordinary package references in escaping closures, overlapping optional families and editor package signatures remain separate. Source is unpublished 1.7.1 because this corrects lexical binding; public libraries remain 1.7.0. The separately prepared VS Code 0.2.2 candidate remains tied to engine 1.7.0 at its own approved-for-review revision, not this fix.

## Verification

The original eight-probe public-1.7.0 baseline is preserved at **3 pass/5 fail**. The final 18-case fixture against the same public release is **6 pass/12 fail**; this expanded baseline remains separate. All 18 cases pass with the fix, including cross-override retargeting and exception restoration. [Implementation evidence](XSLT40-ESCAPING-INLINE-ORIGINALS.json) records source/fixture/log/TRX hashes.

Full solution **8,483/0/0**, including XSLT **5,597/0/0**; language server **84/0/0**. Pinned QT3 schema fixtures were present before the solution run. Frozen QT3 remains exactly **31,142/0/679**, and qt4 gate-only **8,278/0/1,894**. Incremental solution build has zero errors/two existing warnings; the initial build warnings remain in their own log. Exact committed-revision packages, a 185-case focused consumer (18 new + 167 retained) and head CI are required before integration and recorded with the review PR/delivery receipt.

No public API, REQ status, conformance gate membership, upstream submission or publication change. REQ-126 remains In Progress. [Scope and original public baseline](XSLT40-ESCAPING-INLINE-CLOSURES-SCOPE.md); [XSLT 4.0 lexical component binding](https://qt4cg.org/specifications/xslt-40/Overview.html#overriding-components).
