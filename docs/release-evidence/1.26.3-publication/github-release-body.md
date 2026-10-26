Bosak 1.26.3 preserves supported integer results in numeric standard functions without changing public APIs or expanding numeric storage.

- abs handles Int64.MinValue through existing decimal-backed integer storage and preserves supported wide integer identity.
- floor and ceiling retain existing decimal-backed integer values and annotations unchanged. Plain decimal/floating behavior is retained.
- The floor/ceiling argument audit verifies cardinality, recursive arrays, untyped/node conversion and required errors across public call forms. It passed without an engine correction.

The single [Release workflow](https://github.com/Fytala-Charles/Bosak/actions/runs/40322323357) passes **21,798 solution tests** and acknowledges **ten uploads**. All ten public NuGet packages pass signature and exact-source verification. Fresh nuget.org-only consumers pass **153 broad and 11,175 focused checks**, including all **3,061 added source cases**, with 19 matching assembly identities and signed archive/cache/output provenance. Neither dependency audit reports vulnerable packages. Counts overlap.

Candidate/integration validation also passes 84 LSP tests. Frozen QT3 remains 31,142/0/679 and qt4 8,436/0/1,894 across 224 unchanged sets. Current-native Unix retains known negative-domain failures (22/2/0); REQ-126 remains partial. General constructors/casts, fixed numeric storage, idiv/schema-list/coercion and Unix limits remain explicit. Braid's previously captured local automated 1.26.2 adoption report establishes no 1.26.3 adoption/new native acceptance. [Candidate scope and limitations](https://github.com/Fytala-Charles/Bosak/blob/1e376158f4b98e0b26e98cc794388cdb39f896b0/docs/RELEASE-1.26.3-CANDIDATE.md).

Install: `dotnet add package Bosak.XPath.Api --version 1.26.3` or `dotnet add package Bosak.Xslt --version 1.26.3`.

Approved immutable source: `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f`. One annotated tag push and one tag-triggered Release run; no retag or duplicate dispatch.
