<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak optional XSLT function arguments">
  <br><br><h1>Optional XSLT function arguments</h1>
  <p>Bounded source slice - 14 October 2026</p>
</div>

Source candidate **1.3.0** supports optional/default parameters on ordinary XSLT 4.0 stylesheet functions. This is new functionality, so the coordinated library source version advances by a minor version. Published libraries remain 1.2.1; Marketplace extension 0.1.6 is separate. No release is authorized or published by this implementation step.

```xml
<xsl:function name="f:join">
  <xsl:param name="a"/>
  <xsl:param name="b" required="no" select="'B'"/>
  <xsl:param name="c" required="no" select="'C'"/>
  <xsl:sequence select="$a || '|' || $b || '|' || $c"/>
</xsl:function>
```

With `f` bound to the declaration namespace, `f:join('A')` returns `A|B|C`, while `f:join(a := 'A', c := 'Y')` returns `A|B|Y`. Explicit `()` is a supplied value and does not request a default. Optional parameters form a trailing suffix; the function is available at every arity from its required count to its total count. Fully optional functions can have arity zero. Positional partial applications, named references and `function-lookup` expose the reduced forms. Existing rejection of combining argument placeholders with keyword arguments remains a separate boundary.

Defaults use the parameter declaration's applicable namespaces, base URI and globals, with the caller's item, position and size. Function items retain their creation focus. Defaults cannot reference function parameters or caller locals. The body runs with absent focus after parameter binding. A missing `select` and empty content yields the empty sequence; content defaults retain sequence-constructor/document wrapping and whitespace-preservation rules. Supplied arguments skip their defaults. Default type conversion reports XTTE0600; supplied arguments keep the existing function conversion path. Memoization uses completed arguments, preventing focus-dependent defaults from sharing a cache entry based only on supplied arguments.

Earlier effective stylesheet profiles reject optional function parameters. Required declarations with defaults retain XTSE0760; required parameters after optional parameters raise XTSE0761. Overlapping active ranges at the same precedence raise XTSE0770; partially overriding import ranges raise XTSE0769. Fully eclipsed lower ranges are ignored by range validation. The schema-constructor collision check includes arity one within an optional range.

Authoring captures immutable parameter identities, required counts and available arities from retained module contexts. Validation and supported expression edits compile keyword omissions without evaluating defaults or making new resource requests. Parameter defaults are validated in global variable scope; function bodies retain parameter scope. Conditional/shadow/ambiguous signatures and refused graphs remain coverage boundaries. Existing source preservation and public APIs are retained.

Used-package optional function ranges (including private optional declarations inside a used package) and optional `xsl:override` linkage explicitly report `NotSupportedException`; their accept/expose/override rules are outside this slice. Ordinary imports/includes and principal stylesheet declarations are covered. General package linkage, constructor keyword arguments, keyword placeholders and broader draft conformance remain separate work. REQ-126 stays In Progress.

Verification passes 8,219 solution tests (XSLT 5,333), 77 separate language-server tests and 154 related focused checks, including 50 new optional cases. Initial baseline was 3 pass/22 fail across the original 25 cases. QT3 remains 31,142/0/679; qt4 gate-only remains 8,278/0/1,894; native fixed-namespaces remains 18/0/0. The existing public initial-function API also executes a reduced-arity call successfully. Exact source/specification hashes and logs are recorded in [the slice receipt](XSLT40-OPTIONAL-FUNCTION-ARGUMENTS.json). Original upstream fixtures and contribution receipts remain unchanged; no new upstream submission in this step. Frozen gate membership is unchanged.

The implementation follows the current [XSLT 4.0 function parameter rules](https://qt4cg.org/specifications/xslt-40/Overview.html#stylesheet-functions). A downloaded draft snapshot and SHA-256 are retained in the receipt so later draft changes can be distinguished from this implementation target.
