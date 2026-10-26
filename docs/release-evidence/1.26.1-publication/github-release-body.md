Bosak 1.26.1 corrects atomic argument conversion and cardinality in unary operations, recursive arrays, numeric binary/range operands, abs and rounding functions.

- Repeated unary operators preserve conversion and required errors.
- Recursive singleton arrays count atomic results after empty and nested members are processed; empty numeric/range operands propagate empty.
- abs, round and round-half-to-even enforce their argument contracts across direct and dynamic calls while preserving coerced function names. Integer precision arguments support recursive arrays and untyped nodes.
- No new public API or authoring lifecycle is introduced. Existing numerical/storage/precision limits and REQ-126's partial status remain explicit. Braid's bounded acceptance on public 1.26.0 is unchanged; new Braid adoption/native acceptance is separate.

The single [Release workflow](https://github.com/Fytala-Charles/Bosak/actions/runs/40286511584) passes **14,536 solution tests** and acknowledges **ten uploads**. All ten public NuGet packages pass signature and exact-source verification. Fresh nuget.org-only consumers pass **153 broad and 3,913 focused checks**, including all **2,420 added public regressions/controls**, with all 19 assembly identities and signed archive/cache/output hashes matching. Neither fresh dependency audit reports vulnerable packages. Counts overlap.

Candidate/integration validation also passes 84 LSP tests. Frozen QT3 remains 31,142/0/679 and qt4 8,436/0/1,894 across 224 unchanged sets. Current-native Unix retains its two known negative-domain failures (22/2/0); no full current-draft conformance is claimed. [Candidate scope and limitations](https://github.com/Fytala-Charles/Bosak/blob/47f4e64a47f051bdadf5c3c180576ef097442f78/docs/RELEASE-1.26.1-CANDIDATE.md).

Install: `dotnet add package Bosak.XPath.Api --version 1.26.1` or `dotnet add package Bosak.Xslt --version 1.26.1`.

Approved immutable source: `65417cdb3bff184ca9ff1f624582f55ca4681db9`. One annotated tag push, one tag-triggered Release run, no duplicate dispatch or retag.
