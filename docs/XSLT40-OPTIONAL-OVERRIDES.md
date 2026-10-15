<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak optional function override implementation">
  <br><br><h1>Optional function overrides</h1>
  <p>Bounded XSLT 4.0 implementation, 15 October 2026</p>
</div>

## Delivered contract

Unpublished source 1.6.0 supports overriding a public used-package function with trailing optional parameters. The replacement has the same required/total parameter counts and compatible declaration metadata. Its reduced and full arities dispatch consistently through direct calls, arrows, keywords, named references, lookup and positional partial application. Keywords keep the original public parameter names while execution binds the replacement's local parameter names.

Defaults may differ. With original parameters `a, b := 'B', c := 'C'` and replacement parameters `x, y := 'X', z := 'Y'`, `p:f('A')` receives replacement values `A, X, Y`. Inside that replacement, `xsl:original($x)` receives original values `A, B, C`. Defaults and private helpers retain their own declaring scope; an override invoked by a library function returns to the using module's globals, private helpers and decimal formats.

The original pseudo-name is available only inside the overriding function. Original direct/keyword/arrow calls, named references and positional partials cover the original callable range. `xsl:original#1` produces an item named after the original function, with arity one. Such named and partial items remain bound after the override returns and while another override executes. Context-dependent defaults use the call focus, or the function item's creation focus. Dynamic lookup and both `function-available` overloads hide the pseudo-name.

Signature errors preserve XTSE3070, a mismatched component upper bound preserves XTSE3058, and final/non-overridable visibility preserves XTSE3060. Default conversion failures preserve XPTY0004. Optional abstract originals remain an explicit `NotSupportedException` boundary; this slice does not claim their implementation or XTSE3075 coverage.

## Implementation

Package compilation validates the full arity range and installs replacements at every callable arity. Existing adopted-range collision checks remain in place.

At invocation, original signatures are bound to their declaring function and removed from ordinary helper scopes. Two internal runtime signature fields identify the actual function-item target and retain that bound target during dynamic invocation. Ordinary function items continue using the existing resolution path. No public API is added.

Original and overriding functions now use separate declaration identities in the memoization key. This prevents a deterministic original call from receiving a cached replacement result. Package-local dynamic lookup keeps its existing view; its internal original aliases now include reduced arities.

## Verification

The final 41-case regression file is exercised against published 1.5.0 as a preserved baseline and against source 1.6.0. The baseline passes one pre-existing upper-bound error case and fails 40. Source 1.6.0 passes all 41; the combined focused selection covers 74 tests. Final Release verification passes **8,372/0/0**, including **5,486 XSLT tests**, and **84/0/0** language-server tests. QT3 stays exactly **31,142/0/679** and qt4 gate-only stays **8,278/0/1,894**. The Release build has zero errors.

The first 24-case published baseline and intermediate failures are retained separately. Early implementation probes exposed hidden availability, declaration-scope defaults and the need for independent original memoization/lookup aliases. A test rebuild attempted while the full suite held its DLL open failed with a Windows file lock; that attempt is excluded from final verification. The scope probe was run in separate output, the fix was applied, and final verification uses rebuilt outputs.

[Evidence](XSLT40-OPTIONAL-OVERRIDES.json) records test fixture hashes, source-file hashes, published baseline package provenance, log/TRX hashes and final results. The separate [Braid 1.5.0 compatibility result](BRAID-1.5.0-COMPATIBILITY.md) remains evidence for the published release, not a claim that Braid has adopted or accepted this new slice.

## Boundaries

Abstract optional overrides, overlapping adopted/local optional families and broader package-conflict semantics remain outside this slice. Controlled authoring validation still reports package-dependent signature coverage as unsupported. This work adds no module acquisition or package signatures to editor validation.

Escaping named original references and positional partials are covered. Escaping inline closures that defer a direct original call are outside the tested contract; capture a named original item before constructing such a closure. General inline closure/package capture remains separate work.

No public API, conformance gate membership, upstream submission or independent consumer acceptance is added. REQ-126 remains In Progress. Published NuGet packages remain 1.5.0, and VS Code 0.2.0 still bundles engine 1.4.0. The [scoping review](XSLT40-OPTIONAL-OVERRIDE-SCOPE.md) records the draft contract and initial checkpoints. A new release requires its own final package/revision verification and publication approval.
