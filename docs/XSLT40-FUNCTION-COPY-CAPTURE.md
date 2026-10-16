# Function-body copy construction and caller capture

Unpublished source **1.14.13** removes duplicate element/document construction from `CopyNodeForFunctionBody`. The helper inherited raw/typed capture settings, iterated only stylesheet elements (omitting literal text), did not apply the normal conditional/attribute-set paths, and used an actual XDocument for content that can contain text or multiple roots. It could also alter atomic adjacency in a caller that used only the copied node's string value.

Element copies now use `ExecuteSingleCopy` inside an isolated temporary collector, with the outer container/accumulator restored in `finally`; the completed element is detached before return. This reuses capture isolation, literal and conditional text handling, attribute sets, shallow-copy behavior, namespace options, base URI and validation. Document construction is extracted unchanged from normal copying into `ConstructCopiedDocument`, shared by both paths, retaining source metadata, namespace rejection and validation. The helper restores the caller's atomic-adjacency state across every return/error.

```xml
<xsl:function name="f:make" as="item()*">
  <xsl:param name="seed" as="node()"/>
  <xsl:copy select="$seed"><xsl:sequence select="1,2"/></xsl:copy>
</xsl:function>
<!-- The copied content is "1 2", independently of the caller's result capture. -->
```

## Specification and integration base

PR337 passed exact-head CI run38921552083 at `1c815ba990ede3cd50c92713e17296e4e0f0cce9` and merged at `c3936a24d6cf97ead1923c9b84678cf0f3475a98`, this branch's base. Refreshed xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`.

The [shallow-copy rules](https://qt4cg.org/specifications/xslt-40/Overview.html#shallow-copy) and [complex-content rules](https://qt4cg.org/specifications/xslt-40/Overview.html#constructing-complex-content) apply inside stylesheet functions too. Function calls must not change complex-content adjacency in a caller expression.

## Regression controls and fixture correction

Original22-case baseline: **2 passed / 20 failed**. First implementation selection: **166 passed / 1 failed** (22 new +145 previous controls). The remaining failure was in test enumeration: provider Attributes() includes namespace declarations, so the collection included the function namespace as well as the expected ordinary attribute. The fixture now enumerates the XDM attribute axis; its expectation remains one ordinary attribute named new with value ok, with no source attributes/children.

Expanded31-case baseline on the original engine: **6 passed / 25 failed**. Final combined controls: **176/0/0** (31 new +145 previous construction/raw controls). The corrected expanded fixture is byte-identical to the final test source. Original sources/logs and the first implementation snapshot are preserved. The fixture correction is recorded separately from engine failures.

The new cases cover element/document and raw/normal/typed/untyped callers, document text/multiple roots/empty content, illegal maps/functions, caught construction and attribute-set errors, literal/conditional text, outer atomic adjacency, attribute sets and shallow copying, copy-namespaces yes/no, direct/conditional namespace errors, nested-document flattening and XSLT3.0. Previous controls retain source document base URI and exact document-result coverage.

The function-body copy dispatcher itself is unchanged. Select/context focus and current-template-rule restoration are separate reviews. Public APIs, parser/profile selection, general atomization and leaf-node copying retain their paths.

## Native accounting

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 155/5/20 | 155/5/20 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 23/8/1 | 23/8/1 |

All three runs exit1 because failures remain. Exact passing identities are unchanged. PR337 receipts/logs are copied byte-identically under `.artifacts/native-before`; fresh receipts are under `.artifacts/xslt40`. The companion JSON retains source/log/receipt SHA-256 hashes. No upstream expectations, aliases, skips or gate membership changed.

## Verification and next review

All local jobs completed: Release build zero errors/2 warnings; solution **9,223/0/0** (XSLT6,289), focused combined **176/0/0**, language server **84/0/0**, frozen QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, both exit0. All test processes completed; fixture junctions were verified and removed.

Canonical documents are synchronized and rendered using markdown-it and canonical Fytala CSS; renderer receipt is in DOCUMENTATION_RENDERER_TEST.md. Source1.14.13 is unpublished. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance remain unchanged.

Next: exact-head CI/review, then audit copy-select focus/template-rule restoration on success and caught errors in both dispatch paths. Other constructors' namespace handling, nonraw text-only API delivery kind, processor capability, legacy patterns/obsolete functions and complex-content JNode semantics remain separate.
