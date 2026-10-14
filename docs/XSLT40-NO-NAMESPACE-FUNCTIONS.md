<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak private namespace-free XSLT functions">
  <br><br><h1>Private namespace-free XSLT functions</h1>
  <p>Bounded XSLT 4.0 declaration, resolution and authoring support</p>
</div>

Source candidate **1.4.0** adds private namespace-free stylesheet functions. Published libraries remain 1.3.2 and the separate Marketplace extension remains 0.1.6. This implementation does not authorize tagging or publication. No new public API or gate membership is introduced.

## Contract

A function declared with an unprefixed name or `Q{}name` is accepted when its effective version is at least 4.0 and its visibility is private, including the default private visibility. Earlier declarations and explicit public/final/abstract visibility retain XTSE0740. Reserved namespaces retain their existing checks. These rules follow [XSLT 4.0 function names](https://qt4cg.org/specifications/xslt-40/Overview.html#stylesheet-functions).

```xml
<xsl:function name="divide" visibility="private">
  <xsl:param name="dividend"/>
  <xsl:param name="divisor"/>
  <xsl:sequence select="$dividend div $divisor"/>
</xsl:function>
```

`divide(9,3)` and `divide(divisor := 3, dividend := 9)` return 3. An unprefixed call or named reference first selects a matching private no-namespace declaration by arity; otherwise it uses the standard fn namespace. Thus a private `sum#3` can coexist with `fn:sum#2`. Explicit `fn:`/standard URI names bypass this preference. Explicit `Q{}` names remain in no namespace and never fall back to fn.

Direct, arrow, keyword, positional partial application, named reference and explicit-QName lookup paths agree. A positional placeholder combined with keywords on the selected no-namespace host function, such as `divide(?, divisor := 3)`, produces a function accepting the dividend. General keyword placeholder handling for other functions remains unchanged.

Optional arity/default behavior from 1.3.2 is retained, including function-item creation focus and explicit empty arguments. Includes share the containing package's function scope. A non-optional private helper inside a used package keeps its implementation when the caller declares the same name, and is not exposed through lookup in the caller. Broader expose/accept/override behavior is not claimed by these two package checks.

Runtime pattern predicates and AVTs receive host signatures. Authoring validation and supported expression edits use captured signatures, with original source bytes retained. Conditional or ambiguous signatures report unsupported coverage instead of silently choosing a standard function. Existing conditional/shadow graph and expression-edit coverage limits remain.

Availability has a distinct name rule: under [fn:function-available](https://qt4cg.org/specifications/xslt-40/Overview.html#function-function-available), an unprefixed string still names a standard fn function. Use `'Q{}divide'` to check the private no-namespace function. `function-lookup(QName('', 'divide'),2)` uses its explicit QName.

## Verification

The eight existing upstream cases `function-40-014` through `021` fail with XTSE0740 against retained released-profile 1.3.2 binaries and pass with this implementation. Their fixtures are unchanged from upstream master. The family sample moves from **28/8/363** to **36/0/363** on the PR #32 correction checkout; its 012/903 fixes remain pending upstream review. This selected sample is not complete XSLT conformance or upstream acceptance.

All **39 focused cases** pass. The solution passes **8,298/0/0**, including **5,412 XSLT tests**; the language server passes **77/0/0**. Frozen QT3 remains exactly **31,142/0/679**, and qt4 gate-only remains **8,278/0/1,894**. Final build succeeds with zero errors and 25 existing warnings. Two previous 4.0 QName tests now check public no-namespace visibility instead of rejecting valid private declarations.

[Verification receipt](XSLT40-NO-NAMESPACE-FUNCTIONS.json) pins implementation revision, current specification snapshot, source/test trees, baseline/final binaries, exact fixture hashes and successful logs. Preliminary failed runs are explicitly excluded. The original contribution receipts and release evidence are preserved.

## Boundaries

Used-package optional ranges and optional `xsl:override` remain explicitly unsupported. General keyword placeholders, broader package expose/accept/override linkage, schema constructor integration, conditional/shadow/ambiguous authoring graphs and general named-type/pattern/AVT replacement remain separate work. REQ-126 stays In Progress. Any upstream correction must use freshly fetched current upstream and be rebased if it advances, to avoid the merge conflicts Michael identified.
