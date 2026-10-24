<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Experimental substring contract correction">
  <h1>Experimental index-of-substring empty-argument correction</h1>
  <p>24 October 2026 — source1.24.1, unpublished</p>
</div>

**Published in 1.25.0 — 24 October 2026:** [Verified publication](../RELEASE-1.25.0-PUBLICATION.md) includes this Experimental-only correction and 40 fresh signed-package substring/host checks. Frozen XPath40/XSLT behavior is unchanged. The source1.24.1/unpublished labels and counts below describe the original PR429 development receipt; they are historical, not the current publication status.


**Final local verification:** All11,173 solution +84 language-server tests pass, zero failures/skips, including38 new API/two host cases. Release build succeeds with the existing CS8604 conformance warning. Fresh QT3 remains31,142/0/679 and frozen qt4 remains8,434/0/1,896. Current native case021 remains an expected frozen-profile diagnostic failure; native skips are distinct from unit skips. Full TRX hashes are retained; temporary fixture junctions are removed.



## Delivered behavior

Source **1.24.1** adopts the newer absent-substring contract only when the caller explicitly selects `XPathCompatibility.XPath40Experimental`. The owner selected this policy on24 October2026. Frozen `XPath40` retains its existing XPTY0004 refusal. Public packages remain **1.24.0**; this source correction is not yet published.

| Call | Frozen XPath40 | XPath40Experimental |
|---|---|---|
| `index-of-substring('abc', ())` | XPTY0004 | `(1,2,3,4)` |
| `index-of-substring((), ())` | XPTY0004 | `1` |
| `index-of-substring('abc', '')` | `(1,2,3,4)` | `(1,2,3,4)` |
| `index-of-substring((), 'x')` | `()` | `()` |
| `index-of-substring('banana', 'ana')` | `(2,4)` | `(2,4)` |

Both arguments are optional strings in the pinned [F&O catalog](https://github.com/qt4cg/qtspecs/blob/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xpath-functions-40/src/function-catalog.xml). An absent string is treated as zero-length; an empty substring occurs at each Unicode code-point position through `string-length($value)+1`. [Upstream commit ea06e342](https://github.com/qt4cg/qt4tests/commit/ea06e342ebe632a8f0d6841ed3e4821982e7f998), committed11 October2026, changes only native case021 from XPTY0004 to `(1,2,3,4)`.

The freshly checked comparison pins are qtspecs `b4723ff52fd9d4fabf2b4cd33bd44f21939c512c` and qt4tests `3a2640cf649a9bff6c7a5b257a1fe7e50450b2c2`. The frozen fixture remains `68910080d2d4d4092a1c6a908998ceec64ffc6d4`. Current source bytes are verified against Git blob IDs and SHA-256 in [the machine receipt](INDEX-OF-SUBSTRING-CONTRACT.json). This correction does not reconstruct missing historical adoption provenance for the composite catalog entry.

## Consumer opt-in

With a1.24.1 source build:

```csharp
var expression = XPath31Expression.Compile(
    "string-join(index-of-substring('abc', ()) ! string(.), ',')",
    new CompileOptions { Compatibility = XPathCompatibility.XPath40Experimental });
var positions = expression.Evaluate(new EvaluationContext()).ToString(); // 1,2,3,4
```

The existing Experimental enum now explicitly documents opting into selected newer contracts for existing functions, as well as draft additions. No new public type, enum value, overload or function arity is introduced. Earlier language profiles still refuse this4.0 function; default3.1 dynamic lookup still reports it absent.

`IndexOfSubstring_2` uses the existing `IsXPath40Experimental` flag to select `RequireString` for the second argument; frozen evaluation still selects `RequireStringRequired`. Matching, overlaps, code-point positions and result construction are unchanged. Tested numeric arguments and multi-item cardinalities retain XPTY0004; other validation follows the existing string helpers. This is not a general coercion, keyword-metadata or Unicode matching rewrite.

Legacy and XSLT4Draft hosts retain frozen selection even for version4.0 stylesheets. Two host checks verify explicit-empty matching and absent-substring refusal. XSLT host selection and XQuery behavior are not expanded by this correction.

## Verification and native profile boundary

The38 public API controls cover direct calls, named references, dynamic lookup, partial application and variables; absent and explicit-empty input; supplementary Unicode; overlapping/no matches; invalid types/cardinality; older-profile refusal; and repeated reuse of one context across Experimental/frozen compilations. Before the fix,28 pass and10 fail; afterwards all38 pass. Both additional XSLT host controls pass. Original failure receipts are preserved separately.

Fresh manual gates remain **QT3 31,142/0/679** and **frozen qt4 8,434/0/1,896**. Gate membership, native assertions, dependencies and the experimental-function retry policy are unchanged. Native skips and eleven experimental-fallback gate passes retain their previous limitations.

| Native substring run | Passed | Failed | Skipped | Exit |
|---|---:|---:|---:|---:|
| Frozen baseline | 21 | 0 | 1 | 0 |
| Current baseline, frozen runner profile | 20 | 1 | 1 | 2 |
| Current after correction, frozen runner profile | 20 | 1 | 1 | 2 |

The current runner deliberately remains frozen for case021: it retries Experimental only for the existing experimental-function XPST0017 gate, not arbitrary XPTY0004 errors. The newer native assertion therefore remains an explicit frozen-profile diagnostic failure. Direct Experimental API checks prove the opted-in result; the native result is not relabeled as current-draft conformance. No default upstream suite is adopted or weakened.

Original [frozen](evidence/substring-empty-contract/frozen-native.txt), [current baseline](evidence/substring-empty-contract/current-native.txt), [current after](evidence/substring-empty-contract/current-native-after.txt), [full frozen gate](evidence/substring-empty-contract/frozen-gate.txt), [skip inventory](evidence/substring-empty-contract/frozen-gate-skips.txt) and [QT3](evidence/substring-empty-contract/qt3.txt) outputs are retained with hashes. The machine receipt records focused and final full-suite verification separately.

REQ-126/D2/D3/D4/AC-03 remain partial; REQ-136 remains consumer-accepted. No release, Braid update or upstream contribution occurs. The other current-contract failure clusters and any future frozen-baseline migration require separate reviews. Independent processor confirmation is not claimed.
