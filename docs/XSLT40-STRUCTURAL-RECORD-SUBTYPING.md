# Structural record subtyping in function signatures

Unpublished patch1.16.4; first of at most two engine PRs in the newly authorized record-subtyping milestone. Includes the prior readiness review as historical evidence. Public libraries1.8.0 and VS Code0.2.4/engine1.8.0 remain unchanged.

The VM now compares structural record types by equal, ordinal field-name sets and covariant field sequence types. Field order does not affect subtyping. record(*) is a supertype of specific records; it is not a subtype of a specific record. Existing record parsing is reused. Comparison recurses through nested records, arrays, maps and function signatures. Outer occurrences are separated from record text, and structured strings retain original case through nested function-family comparisons; only the final built-in atomic hierarchy comparison is normalized.

The typed function instance-test dispatcher previously treated function() as T like a generic function test, accepting any function without checking arity/return type. Valid typed signatures now use the existing structural matcher, including zero-argument signatures; function(*) retains its wildcard meaning. This correction is needed to verify record return covariance rather than passing it vacuously. Both XSLT host profiles and direct XPath4 exercise the shared VM path. No parser, public API, map-coercion, record annotation, or legacy processor-selection changes.

The pinned [structural record subtype rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-item-subtype-structural-records) define equal field sets with subtype-compatible field types. Function parameters remain contravariant and results covariant. Record/map/function supertype relationships are deliberately left for the independently baselined second slice.

36 new unit controls;150 focused pass. Initial34-case baseline2pass/32fail preserved. Two item-type alias declarations incorrectly included outer occurrence indicators in that first fixture; the corrected fixture keeps occurrences on the alias references. It also adds atomic zero-argument controls. Corrected36-case baseline5pass/31fail; final fixture byte-identical to corrected-tests.txt. No expected result weakened. Controls cover positive/negative numeric relationships, wildcard/empty records, reordered/missing/extra/case-sensitive fields, quoted names, nested containers/functions, field/outer cardinalities, aliases, both profiles, direct XPath and annotation non-coercion.

Fresh six-selection comparison recovers exactly record-100, record-101, record-104 and record-105 in both profiles. Every remaining status and diagnostic detail is identical. Legacy array155/5/20,record33/31/1,type-alias23/8/1; Draft array158/2/20,record33/31/1,type-alias23/8/1. Draft total214/41/22 across277 selected cases. All six family runs still exit1; no aliases, skips, fixture changes or gate promotions. record-100 retains its upstream exploratory xslt40-not-yet-agreed dependency.

Next: at most one engine PR for independently reproduced record-to-map/function subtyping. Then stop engine work, reassess the remaining classifications and validate exact-candidate packages with fresh consumers. No upstream contribution or publication is authorized by this milestone.


All processes completed: Release build0errors/2 existing warnings; solution **9545/0/0** (XSLT6611), language-server **84/0/0**, ten CLI controls pass. Fresh QT3 **31,142/0/679**, qt4 gated **8,278/0/1,894**, both exit0. Verified fixture links removed; canonical documents synchronized and rendered.

Refreshed suite91328fbf0fed4a5d2813eb3661808ee53cb961ba; qtspecs72f222929768b411d8e88d96a1a8740483290078; xslt.xml blob59b9bdc79dca23b0c18fdd3e94130bc59037db3e. No upstream edits, aliases, skips or gate promotions. Source remains unpublished; public versions and independent acceptance unchanged.
