<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak URI and import utility contract reference">
  <br><br><h1>URI and Import Utility Contracts</h1>
  <p>Component links, decoding, collations and import diagnostics</p>
</div>

## Scope and outcome

Reviewed 2026-10-12 against Bosak `9d7e0b8f93594ff2086c5da321cb3fab88b72897`, published version 1.1.3. Five Frozen 4.0 families have 105 new checks: 15 keyword/mode refusals, 82 focused options/record/encoding/type/cardinality checks and eight complete normal/empty/reordered/fallback import reports in XPath 3.1/4.0. **All 37 original absent-keyword families now have targeted review; none await that first review. All 37 still lack keyword metadata.** This milestone does not close D2 or REQ-126. Full conversion/default/options coverage, the exhaustive changed-3.1 inventory and D3/D4 acceptance remain partial. Catalog: 67 entries, 2,093 feature + 24 chapter checks = 2,117; four worked chapters. No engine/API/harness change, gate promotion or release.

Primary contract: [immutable QT4CG F&O catalog e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml). The [raw comparison](audit-signature-contracts.json) links case IDs and preserves build-uri's uri-structure-record type-ref, fields and three boolean options. Its null direct type attribute was not an untyped argument. Review pins do not recover historical adoption. Implementation: [FunctionLibrary](../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs), BuildUri, DecodeFromUriCore, CollationAvailable_1, Identity_1 and IsNaN_1.

## Signatures and keyword availability

| Function | Pinned parameters/default | Result |
|---|---|---|
| `fn:build-uri` | `parts: fn:uri-structure-record, options: map(*)? := {}` | `xs:string` |
| `fn:collation-available` | `collation: xs:string` | `xs:boolean` |
| `fn:decode-from-uri` | `value: xs:string?` | `xs:string` |
| `fn:identity` | `input: item()*` | `item()*` |
| `fn:is-NaN` | `value: xs:anyAtomicType` | `xs:boolean` |

All five keyword expressions refuse 3.1 grammar XPST0003 and 4.0/experimental binding XPST0017. Positional forms remain Frozen 4.0; experimental does not add missing metadata. build-uri's options default to an empty map, including explicit (). The record has fourteen optional/sequence-valued fields; this review is representative rather than an exhaustive record-coercion audit.

## URI assembly and observed option boundaries

Omitted/()/empty-map options produce the same URI. Explicit omit-default-ports=true removes HTTPS 443; an empty option value and numeric zero leave it. Hierarchical path segments protect embedded slash, space and delimiters; path plus remains literal, while query-parameter plus becomes %2B. Fragment slash/question mark remain literal. Raw path/query fields are added without segment/form escaping. Repeated query values yield repeated keys. Tested populated path-segments/query-parameters take precedence over raw fields. Unicode café remains Unicode in the assembled path; build-uri is not a general encode-for-uri replacement.

Password-bearing userinfo is omitted by default and retained with allow-deprecated-features=true in a synthetic fixture. mailto keeps its non-hierarchical path, and file with unc-path=true uses file:////. No network request or credentials are used. Empty records return an empty string; empty/array/multiple-record parts and a boolean options argument refuse XPTY0004.

**Conversion/precedence follow-ups:** the string option 'false' and sequence (true(),false()) both enable default-port omission in tested calls. The implementation uses effective boolean value after atomization rather than enforcing the pinned singleton boolean option contract. Unknown option keys are ignored in the sample; do not infer a standardized unknown-key policy. A string port '443' is accepted; an unknown record field is ignored. query-parameters=42 is silently ignored, a nonempty map whose sole value is () emits '?', and an explicit empty parameter map falls back to raw query. Preserve those results for precise record/option/empty-query precedence review rather than advertising permissive forms.

## Decode once and preserve plus

decode-from-uri(()) returns a zero-length string. %20 becomes space and %2B becomes plus, but literal + stays +. UTF-8 café and the encoded lightbulb decode correctly. Malformed %1X! produces replacement-character plus !; incomplete %, invalid UTF-8 and an XML-null octet yield replacement characters rather than refusing the call, as the pinned rules specify. %252F becomes %2F, not slash: decoding is one pass. Replacement is not validation of the original encoding. Typed anyURI, untyped and node text forms work; tested numeric/map/multi-item arguments refuse XPTY0004. General percent decoding has no direct standard XPath 3.1 counterpart; the paired business baseline uses a host-supplied raw field for these fixtures.

## Collations, identity and numeric diagnostics

collation-available recognizes tested codepoint, HTML ASCII case-insensitive, UCA en/primary and unknown-language UCA fallback=yes candidates. Unknown absolute and relative candidates return false in the fresh no-base probe context. Empty string returns true; empty sequence, numeric and multiple strings refuse XPTY0004; a map refuses FOTY0013. anyURI, untyped and node text candidates work. Availability asks whether a URI is recognized, not whether two values compare equal. Empty/relative URI interpretation remains scoped to this context; full base-resolution/default-collation policy is not certified.

identity preserves empty sequence, source order, duplicates, mixed items, map/array values and XML node identity. Returning a function item still allows invoking it. identity#1 works as a pass-through callback. It does not copy nodes or decode/normalize labels.

is-NaN is true for float/double NaN and number('unknown'); false for finite numbers, either infinity, booleans, QName, plain/untyped 'NaN' and an untyped XML node containing NaN. Empty/multiple atomic inputs refuse XPTY0004, and maps/functions refuse FOTY0013. The tested [1] atomizes and returns false. NaN testing is not lexical validation, finiteness testing or a business-range check; explicitly convert and separately validate quantity policy. Existing NaN extrema-order findings remain separate and unresolved.

## Workflow and remaining work

The [detailed import/link README](examples/utility-contracts/README.md) supplies complete XML, paired code, exact outputs and every fixture. Existing [invoice API URIs](functions/invoice-api-uris.md), [supplier ranking](functions/supplier-offer-ranking.md), [configured callbacks](functions/configured-label-callbacks.md) and [numeric diagnostics](functions/numeric-import-diagnostics.md) retain original delivery/unit/gate evidence. These diagnostics are not another worked chapter.

Next complete D2's exhaustive inventory of changed existing XPath 3.1 function signatures/contracts; the current matrix samples only twenty. Use the frozen Bosak baseline and pinned draft declarations/change notes to identify omissions and prioritize exact conversion/default/options cases. Consolidate the reviewed missing-keyword findings for later engine remediation without treating review as implementation. D3 grammar/pattern, D4 provenance/acceptance, historical adopted pins and AC-03 applicability remain unresolved; no closure or release based on sample coverage.

## Delivery validation

Release solution build passed (12 existing warnings, zero errors); 6,321 tests passed with no failures or skips across ten assemblies. Render checks verified 99 catalog pages and six implementation/canonical pages. Five pinned signatures, URI record/options, exact case IDs, independent full report outputs, complete README expressions, local links, 67 sixteen-field entries, 241 cached primary links/anchors, 28 historical delivery records and 117 assigned names were checked. All 37 reviewed families still lack keyword metadata. No manual engine gates were rerun for this documentation/example scope.
