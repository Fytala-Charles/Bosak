# Candidate migration inventory

| Boundary | Current finding | Required preview/freeze check |
|---|---|---|
| Source defaults | Exact-decimal overload makes FromDecimal(default) CS0121 ambiguous; original compile receipts are pinned in INPUTS.json. Use default(decimal) for the old CLR route, default(XdmExactDecimal) for the new exact carrier. FromInteger(default) remains resolved in those probes. | Compile actual owner source on the selected candidate; decide stable SemVer/source-migration disposition explicitly. Do not claim unconditional source compatibility from retained binary signatures. |
| Numeric access | BigIntegerValue/ExactDecimalValue preserve exact carriers; legacy narrowed access may refuse out-of-range/inexact values. | Exercise actual numeric edges and serialized/interchange routes. The bounded opt-in profile is not arbitrary exact arithmetic everywhere. |
| Typed authority | Annotation strings do not authenticate schema authority. Original-node values require exact source/input/origin issuance. | Keep real provider/source lifetimes and actual native evidence, not copied names. |
| Construction | Only explicit exact $constructed profile, singleton plain selected families, optional exact advisor admission and operation-local receipts. | Keep authorized use inside Run/RunAsync; distinguish Invalid/Unsupported and capacity/malformed exceptions. No retained constructor import or parse-budget promise. |
| Compatibility | Raw compatibilityView remains Unsupported. New engine-issued source handoff/result are proposed, not available. | Disposition CV-ENTRY-01 and qualify actual multi-advisor retention before adopting the public route. |
| Ordinary XML | Ordinary source use stays independent of Schema; no invented assessment/lease ownership. | Verify actual XmlDb generation/read resources and supported cancellation/cleanup/concurrency boundaries. |
| All-host/process | XPath bounded profiles do not imply XQuery/XSLT/Braid protocol parity or live authority serialization. | Qualify explicit source/global/match roles and actual owner process/currentness behavior on one candidate. |
| Packaging | Existing stock items omit root NOTICE; overlay is explicitly evaluated only. | Build immutable committed-source preview, inspect package content/dependency closure and fresh-cache package-only loaded hashes; signing/publication later. |

This is an inventory, not owner sign-off, an accepted migration, a stable SemVer selection or a package execution result. The historical source-default probes are preserved without relabeling them as new tests on 1.66.0.
