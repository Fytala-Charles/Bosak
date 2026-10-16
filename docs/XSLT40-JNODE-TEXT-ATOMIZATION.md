# JNode atomization in XSLT text construction

Unpublished source **1.14.7** makes attribute value templates, expanded text, `xsl:value-of`, and selected/sequence-constructed `xsl:attribute` values atomize JNode payloads instead of emitting `(jnode)`. Complete payload sequences are retained, including empty values, nested arrays, nested JNodes and empty strings. Explicit separators are applied to the atomized items.

```xml
<xsl:variable name="city" select="array:members([map{'name':'Moskva'}])[1]"/>
<out name="{$city/name}">{$city/name}</out>
<!-- With expand-text enabled: <out name="Moskva">Moskva</out> -->
```

The existing string atomizer unwraps JNodes recursively. XNodes inside a JNode payload contribute their typed values, including list/sequence values, and element-only nodes without a typed value raise FOTY0012. Wrapped maps/functions raise FOTY0013 even after earlier valid values. A private recursion flag limits typed-node traversal to JNode payloads, preserving ordinary XNode output paths. Value-of and simple-content construction route JNodes through this atomizer just as they already route arrays.

Text-node boundary handling and attribute separator defaults remain in place. In particular, an attribute sequence constructor without `select` defaults to an empty separator, while selected attribute values default to a space. A separate control verifies the former. No public API, JNode representation, general atomizer, serializer, runner, profile selection, pattern grammar or gate membership changes are included.

## Specification and integration base

PR331 passed exact-head CI run38906120624 at0971af0cbb653af801573d812217076fbaa11f2e and merged at `67301df038b799a6d93817c913018a653f4a4d6a`, this branch's base. Worktree: D:/Development/Bosak/.guard-tmp/jnode-text-source; branch: fix/jnode-text-atomization.

Refreshed upstream remains xslt40-test `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. The [simple-content rules](https://qt4cg.org/specifications/xslt-40/Overview.html#constructing-simple-content), [AVT rules](https://qt4cg.org/specifications/xslt-40/Overview.html#attribute-value-templates) and [text-value-template rules](https://qt4cg.org/specifications/xslt-40/Overview.html#text-value-templates) require atomization followed by string conversion. The pinned F&O catalog blob `930edc7ade2bdf7afecdf8019b93e5b450d9f10e` and existing fn:data implementation provide the JNode-payload atomization contract.

## Diagnostic improvement, unchanged native counts

The unchanged standalone probes from the preceding audit were copied into this worktree with only their artifact output paths adjusted. Original fixture base URIs were retained. The isolated root-version4-only array-022 probe now produces **RU TJ**, matching the unchanged assertion's text, where the previous run produced an empty out element. Its original upstream3.0 stylesheet still raises XPST0017 under Bosak's version-selected profile. This diagnostic improvement is **not a newly passing native case**.

The minimal probe now yields children=1, key=name, data=Moskva, avt=Moskva and element text=Moskva. Previously navigation/explicit data worked but the last two values were `(jnode)`. Other original/root4-only probes retain their previous outcomes, including missing current-catalog jposition in array-025.

| Native selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 155/5/20 | 155/5/20 |
| array instruction set | 24/5/8 | 24/5/8 |
| sx-SquareArrayConstructor | 56/0/0 | 56/0/0 |
| arrays type set | 75/0/12 | 75/0/12 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 23/8/1 | 23/8/1 |

All three fresh native family runs exit1 because failures remain; no native gains or losses. Baseline family receipts/logs from engine-equivalent PR330 and preceding audit probe outputs are copied byte-identically into `.artifacts/native-before` and `.artifacts/diagnostic-before`. Current outputs/receipts are preserved separately. No upstream fixture/expectation, error alias, skip or dependency was changed.

## Regression controls and retained attempts

The original52-case fixture and log (0/52) are retained. Its no-typed-value control initially used mixed `xs:anyType`, which does not establish an element-only type. Before implementation, that fixture was corrected to use an explicit element-only schema type; the corrected52-case baseline remained0/52.

The first implementation produced48/4. All four failures were cross-surface attribute-content fixtures that assumed a space default separator. They now request the existing space separator explicitly, retaining their expected strings. A separate default-empty-separator control, an untyped-node lexical guard and two mixed text/JNode boundary controls bring the final result to **56/0**. Original fixtures and logs remain hashed; no engine separator behavior was changed to satisfy those fixtures.

The final controls cover five construction surfaces; singleton/empty/multi-item payloads; nested arrays/JNodes; empty-string join slots; wrapped maps/functions including later invalid items; explicit separators; typed-node canonical values; FOTY0012; untyped-node lexicals; mixed text boundaries; and a deterministic three-city result filter.

## Verification and next step

All local jobs completed: Release build zero errors/22 incremental warnings; solution **9,063/0/0** (XSLT6,129), focused **56/0/0**, language server **84/0/0**, QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, both exit0. The companion JSON records source, fixture, diagnostic tool/output, receipt and log SHA-256 hashes. Canonical docs are synchronized and rendered with markdown-it and canonical Fytala CSS. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance are unchanged. Source1.14.7 is unpublished.

Next: exact-head CI/review before integration, then reproduce the deferred explicit-document multi-atomic spacing observation against the current draft. Processor-capability architecture, legacy pattern/obsolete-function review, broader copy/function fallback and complex-content JNode handling remain separate.
