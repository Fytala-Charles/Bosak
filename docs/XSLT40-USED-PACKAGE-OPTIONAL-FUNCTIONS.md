<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak used-package optional function support">
  <br><br><h1>Optional arguments in used-package functions</h1>
  <p>Bounded XSLT 4.0 slice, 14 October 2026</p>
</div>

## Supported contract

Source candidate 1.5.0 allows functions with trailing optional/default arguments in a used package. The existing package registry and URI resolver still supply the library. Direct, arrow and keyword calls, named references, lookup and positional partial application expose the declared callable arity range. Defaults retain the declaring package's namespaces and globals, with the caller focus required by XSLT 4.0. Function items retain their creation focus. Constructor defaults and XPath conversion errors retain the ordinary function contract; function bodies remain without focus and cannot access caller locals.

Public and final functions execute at reduced arities. Explicit accept/expose names use the upper bound of the arity range as their symbolic component identifier: `p:f#3` identifies the entire family with arities 1 through 3, while `p:f#1` does not identify that component. Accepted-private functions remain callable locally; hiding the component hides all its callable arities. Private optional helpers remain in the declaring package and cannot leak through caller lookup. Nested use-package calls preserve declaring scope. A positional XSLT 3.0 caller can invoke defaults declared by a library with effective XSLT 4.0.

```xml
<xsl:package xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
             xmlns:p="urn:functions" name="urn:library"
             package-version="1.0" version="4.0">
  <xsl:function name="p:join" visibility="public">
    <xsl:param name="value"/>
    <xsl:param name="suffix" required="no" select="'B'"/>
    <xsl:sequence select="$value || $suffix"/>
  </xsl:function>
</xsl:package>
```

After registering and using this package, `p:join('A')` and `p:join(value := 'A')` both return `AB`; `p:join('A','X')` returns `AX`. In accept/expose, the component identifier is `p:join#2`.

## Implementation and boundaries

The blanket rejection at package loading is removed. Existing reduced-arity registration and declaring-package runtime scope perform execution. Arity suffixes are separated before QName validation in accept/expose. Accepted-private functions are included in an implicit caller package's local view while explicit library export views still exclude them. A graph-wide check rejects overlapping adopted optional families before dictionary keys could silently choose a winner; hiding a conflicting family resolves that boundary and disjoint families remain callable.

Optional `xsl:override` and its `xsl:original` interactions remain explicitly unsupported, including inside a transitively used package. Overlapping optional ranges between adopted components or local/adopted definitions remain an explicit `NotSupportedException` boundary, rather than a claim about the draft's eventual required error code. Broader override and package conflict semantics remain separate work. Controlled authoring validation still reports package-dependent signature coverage as unsupported; this slice does not add module acquisition or package signatures to editor validation.

No public API or conformance gate membership changes. REQ-126 remains In Progress. Published NuGet libraries remain 1.4.0. The separate VS Code 0.2.0 candidate is pinned to its verified 1.4.0 extension revision; Marketplace remains 0.1.6 until publication.

## Verification

The first 24 focused cases all fail on retained 1.4.0 at the blanket package guard. The final slice adds 33 cases; the combined optional/private-function regression selection passes 133 tests. Full solution verification passes 8,331 tests, including 5,445 XSLT tests, and the language-server suite passes 84. Frozen QT3 remains 31,142/0/679 and qt4 gate-only remains 8,278/0/1,894. [Evidence](XSLT40-USED-PACKAGE-OPTIONAL-FUNCTIONS.json) records hashes and the bounded sample separately from the full gates.

The contract follows the current [XSLT 4.0 draft](https://qt4cg.org/specifications/xslt-40/Overview.html), particularly component symbolic identifiers (upper arity bound), function parameter defaults and dynamic function access. No new upstream test submission or acceptance is claimed. Existing upstream receipts remain unchanged. Any future correction must start against freshly fetched current upstream and be rebased if upstream advances, avoiding the conflicts Michael identified.

## Integrated revisions

Engine PR #288 merged at `bfb050e5c75752eb324263d456cc8676511c01e2` after [exact-head CI](https://github.com/Fytala-Charles/Bosak/actions/runs/38615928472) passed. Extension PR #287 merged at `ebd37910835f50c83f377d6ad37f06e90fd90913` after its CI passed; a new portable VSIX rebuilt from that exact revision repeats the packaged protocol and actual VS Code valid/error/live-correction checks. [Delivery evidence](XSLT40-USED-PACKAGE-DELIVERY.json) preserves both source trees and final candidate hashes. Original preparation receipts remain unchanged. Both new candidates remain unpublished.
