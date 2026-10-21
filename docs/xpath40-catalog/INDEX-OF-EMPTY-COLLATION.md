<div align="center">
<img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak index-of empty-collation correction">
<h1>Index-of Empty Collation Default</h1>
<p>21 October 2026</p>
</div>

**Verified unpublished source 1.23.2.** An explicitly empty third argument to `fn:index-of` now selects the current evaluation context's default collation in frozen and experimental XPath4.0. XPath3.1 continues to reject the empty third argument with XPTY0004; keyword syntax still refuses XPST0003 there. Public libraries remain1.23.0; no release is performed. The reviewed documentation slice PR398 is merged at `7c33728c7aa34fca5ccdc21d8310f36894d77f09` after successful CI39610977211. Hash-only PR132 remains untouched.

## Contract and narrow implementation

The [pinned F&O4.0 declaration](https://github.com/qt4cg/qtspecs/blob/3230573afe246f1b502ed2e22d187ce9928155b1/specifications/xpath-functions-40/src/function-catalog.xml) makes `$collation` optional and default-on-empty. The [dated3.1 declaration](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/function-catalog.xml) requires a string when the third argument is supplied. The [equality audit](audit-equality-collation-contracts.json) preserves exact source hashes/signatures and the original failing observation. The current pin is a contract review, not a recovered historical adoption revision.

`IndexOf_3` uses `ctx.DefaultCollation` only when `ctx.IsXPath40` and the third argument is the empty sequence. All other values retain `RequireStringRequired`; collation validation and the existing atomic equality/search code run as before. This adds no registration, keyword metadata, public API or gate membership. The context is read on each evaluation rather than captured at compilation. Explicit string collations still override the host default. The existing empty-string path is preserved and is distinct from an empty sequence.

```csharp
var expression = XPath31Expression.Compile(
    "string-join(index-of(('A','a','A'),'A',()), '|')",
    new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
var context = new EvaluationContext
{
    DefaultCollation = "http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive"
};
// expression.Evaluate(context).ToString() is "1|2|3".
```

Imports: `Bosak.XPath.Api` and `Bosak.XPath.Runtime.Vm`. Under codepoint default the same compiled expression returns `1|3`. Switching back to the original context preserves its result. Default XPath31 compilation still raises XPTY0004 when this call is evaluated.

## Regression evidence

The initial27-case run passes15 controls and fails12 new defaulting cases with XPTY0004. After the correction all27 pass. [IndexOfCollationDefaultTests](../../tests/Bosak.XPath.Standard.Tests/IndexOfCollationDefaultTests.cs) exercises:

- Positional, `index-of#3`, `function-lookup`, partial application and keyword calls at both4.0 levels.
- Reuse of one compiled expression with codepoint/ASCII/codepoint evaluation contexts, proving default selection and absence of context leakage.
- The same3.1 call paths, preserving required-collation errors and the earlier keyword grammar boundary.
- Omitted defaults, explicit overrides, unknown collation URIs and invalid nonempty numeric/multiple arguments.
- Empty input with a valid target, required-target refusal, NaN value equality and the existing empty-string behavior.

The catalog changes exactly one existing row: [index-empty-collation](examples/equality-contracts/index-empty-collation.xpath) now returns1 at4.0 and declares baselineError XPTY0004 at3.1. The old observed error is retained as metadata. No new catalog rows are added;68 entries,2,636 feature +24 chapter checks =2,660 remain the documentation totals. Full customer report normal/empty/reordered results are unchanged. The original equality audit stays intact with a separate correction record. Deep-equal options and unrelated keyword findings remain separate.

## Verification and limits

| Check | Result |
|---|---|
| Final incremental Release build |0 warnings/errors after refreshed restore metadata; no nullable warning fix claimed |
| Focused standard-library tests |27/0/0, from15/12/0 before the fix |
| Full solution |10,331/0/0 across all ten projects;27 added tests, counts overlap |
| Frozen QT3 |31,142/0/679 |
| qt4 gated |8,278/0/1,894; membership unchanged |
| Catalog | One corrected outcome; all2,635 other rows retained |

Initial compilation includes the pre-existing conformance Program.cs:1868 CS8604 warning. Cached NU1900 audit-connectivity warnings are retained in the initial logs; a forced no-HTTP-cache restore with network access succeeds without warnings. Neither is a source warning correction. Original build/test/TRX/gate/restore logs remain under `.guard-tmp/index-of-collation-*`. [Machine-readable verification](INDEX-OF-EMPTY-COLLATION.json) records hashes and exact before/after outcomes. No upstream contribution, package publication, Braid runtime change, complete options implementation or REQ-126 closure is claimed by this slice.
