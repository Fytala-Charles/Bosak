<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: archive and partner URI option policy">
  <br><br><h1>Archive and partner URI policy</h1>
  <p>Frozen 4.0 — Boolean options and component precedence</p>
</div>

## 1. Name and Classification

**URI option policy — Frozen 4.0.** [fn:parse-uri](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-parse-uri) and [fn:build-uri](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-build-uri).
Initial engine delivery: [PR #84](https://github.com/Fytala-Charles/Bosak/pull/84), [revision f7f55b4e](https://github.com/Fytala-Charles/Bosak/commit/f7f55b4e3a6e92da26ac607e34f46aede695cb19), REQ-118 4.0-S2.
Definitions reviewed 2026-10-12 against immutable [QT4CG function source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml).
This review does not recover the historical adopted specification snapshot.
General syntax: [RFC 3986](https://www.rfc-editor.org/rfc/rfc3986) and [RFC 3987](https://www.rfc-editor.org/rfc/rfc3987).

## 2. Intent

Interpret a configured network archive as a UNC path and construct an invoice route from a partner endpoint. Choose structured components deliberately when legacy raw fields coexist.

## 3. Also Known As

URI component policy and UNC interpretation are descriptive terms; no aliases are registered.

## 4. History / Supersedes

XPath 3.1 can assemble known routes from host-validated metadata. The comparison below uses separate server/folder/leaf and host attributes; it does not implement generic parsing, percent decoding, UNC detection or password filtering.

## 5. Motivation

Archive paths and HTTP endpoints need different interpretation. A leading double slash normally denotes a relative authority; enabling UNC interpretation instead preserves the server/share text as a file path. Partner configuration may retain obsolete raw authority, path and query fields while a new structured route overrides them.

## 6. Language Alignment

Compile with `XPathCompatibility.XPath40`. The runner refuses successful new-function fixtures in default 3.1 mode. Both functions accept an optional options map; empty options use defaults. Bosak registers keywords `uri`/`options` for parsing and `parts`/`options` for building. The working keyword fixture verifies the parser's actual metadata.

| Option | Reviewed type/default | Effect |
|---|---|---|
| `allow-deprecated-features` | `xs:boolean`, `false()` | Retain nonempty password-bearing structured userinfo |
| `omit-default-ports` | `xs:boolean`, `false()` | Omit recognized scheme-default structured ports |
| `unc-path` | `xs:boolean`, `false()` | Interpret leading slashes as UNC when parsing; select the file prefix when building |

Always supply Boolean values. Bosak currently applies effective Boolean value: the nonempty string `'false'` enables the tested options. These observed fixtures record a validation gap against the Boolean contract.

## 7. Applicability

Use for configured archive paths and route assembly. Parsing does not validate a destination or perform network/file I/O. The synthetic `demo:placeholder` is example data. Password filtering is not redaction: the original URI and raw authority can retain the text.

## 8. Structure

Parse archive with explicit UNC policy; parse partner with default-port omission; build a fresh route from host, scheme, segments and query map. Do not copy raw authority into the structured route.

## 9. Participants

The route XML supplies configuration; component maps carry parsed values; `path-segments` preserves `INV/2048` as one encoded business identifier; the options map supplies Boolean policy.

## 10. Collaborations

`parse-xml`, map lookup, `exists`, `concat` and `encode-for-uri` support the complete report and portable comparison. [Invoice API URIs](invoice-api-uris.md) explains decoding, repeated query keys and textual roundtrip limits.

## 11. Consequences

Structured userinfo/host/port take precedence over raw authority when any is present. Otherwise raw authority is appended unchanged: password/default-port options do not inspect it, as stated by the reviewed draft. Nonempty segment sequences override raw path; an empty sequence falls back, while one empty string is a present segment and can produce an empty path.

A nonempty query map overrides raw query. **Observed draft gap:** an empty map is still a present item under the reviewed rules, but Bosak falls back to raw query. Remove the raw query field when deliberately building with an empty query map.

UNC construction uses a literal `:////` prefix after `file`. The tested leading path slash therefore yields `file://///server/share`; this is verified spelling, not a claim of canonical file-URI representation. Password-free userinfo with an empty password (`demo:`) is retained.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles Boolean deprecated/default-port/UNC policies; Structured versus raw authority/path/query precedence; Nonmap option and malformed authority errors. **Boundaries:** String false currently enables a Boolean option; empty query map currently falls back to raw query. Raw-authority bypass itself is draft-defined. Follow-up groups: F01, F04. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

See [FunctionLibrary](../../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs): `UriBooleanOption`, `ParseUri` and `BuildUri`. The empty-query-map branch and option truthiness are observed implementation limits. Public function conversion rejects non-map options with `XPTY0004`, including empty parse input; unmatched bracket authority raises `FOUR0001`. No engine changes are included here.

**Supplementary contract review (2026-10-12):** Keyword drift: implemented uri differs from pinned value; the [signature audit](../SIGNATURE-CONTRACT-AUDIT.md) publishes paired success/refusal files. Existing positional option-policy examples remain valid.

## 13. Sample Code

Complete synthetic input:

```xml
<routes><archive server="files.example.test" folder="invoices" leaf="October 2026" value="//files.example.test/invoices/October%202026"/><partner host="api.example.test" value="https://demo:placeholder@api.example.test:443/legacy"/></routes>
```

Complete [main fixture](../examples/uri-option-policy/main.xpath):

```xpath
let $r := parse-xml('<routes><archive server="files.example.test" folder="invoices" leaf="October 2026" value="//files.example.test/invoices/October%202026"/><partner host="api.example.test" value="https://demo:placeholder@api.example.test:443/legacy"/></routes>')/routes
return let $archive := parse-uri(string($r/archive/@value), map {'unc-path': true()})
return let $api := parse-uri(string($r/partner/@value), map {'omit-default-ports': true()})
return let $target := build-uri(map {'scheme': $api?scheme, 'host': $api?host, 'port': $api?port, 'authority': 'ignored.example.test:8443', 'path': '/ignored', 'path-segments': ('', 'v2', 'invoices', 'INV/2048'), 'query': 'ignored=1', 'query-parameters': map {'status': 'ready now'}})
return concat('archive-scheme=', $archive?scheme, '; archive=', $archive?filepath, '; partner=', $target, '; userinfo=', exists($api?userinfo))
```

Output:

```text
archive-scheme=file; archive=//files.example.test/invoices/October 2026; partner=https://api.example.test/v2/invoices/INV%2F2048?status=ready%20now; userinfo=false
```

The archive has no parsed host component under UNC policy. Partner password-bearing userinfo is absent and port 443 is omitted; the freshly built route overrides deliberately conflicting raw fields.

### XPath 3.1 comparison

Host-provided normalized archive metadata and validated HTTPS host allow this scoped construction:

```xpath
let $r := parse-xml('<routes><archive server="files.example.test" folder="invoices" leaf="October 2026" value="//files.example.test/invoices/October%202026"/><partner host="api.example.test" value="https://demo:placeholder@api.example.test:443/legacy"/></routes>')/routes
return let $archive := concat('//', $r/archive/@server, '/', $r/archive/@folder, '/', $r/archive/@leaf)
return let $target := concat('https://', $r/partner/@host, '/v2/invoices/', encode-for-uri('INV/2048'), '?status=', encode-for-uri('ready now'))
return concat('archive=', $archive, '; partner=', $target)
```

Output:

```text
archive=//files.example.test/invoices/October 2026; partner=https://api.example.test/v2/invoices/INV%2F2048?status=ready%20now
```

This comparison constructs the two addresses; it does not produce parsed scheme/userinfo evidence. It requires the separate normalized attributes and an explicit host validation policy.

### Complete fixture inventory

The [manifest](../examples/cases.json) runs all 28 cases through [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs).

| Fixture | Exact output or error marker |
|---|---|
| [main](../examples/uri-option-policy/main.xpath) | `archive-scheme=file; archive=//files.example.test/invoices/October 2026; partner=https://api.example.test/v2/invoices/INV%2F2048?status=ready%20now; userinfo=false` |
| [baseline](../examples/uri-option-policy/baseline.xpath) | `archive=//files.example.test/invoices/October 2026; partner=https://api.example.test/v2/invoices/INV%2F2048?status=ready%20now` |
| [unc-default](../examples/uri-option-policy/unc-default.xpath) | `scheme=; host=server; path=/share` |
| [unc-enabled](../examples/uri-option-policy/unc-enabled.xpath) | `file\|//server/share\|false` |
| [unc-build](../examples/uri-option-policy/unc-build.xpath) | `file://///server/share` |
| [unc-build-default](../examples/uri-option-policy/unc-build-default.xpath) | `file:///server/share` |
| [parse-password-default](../examples/uri-option-policy/parse-password-default.xpath) | `false` |
| [parse-password-allow](../examples/uri-option-policy/parse-password-allow.xpath) | `demo:placeholder` |
| [build-password-default](../examples/uri-option-policy/build-password-default.xpath) | `https://host/path` |
| [build-password-allow](../examples/uri-option-policy/build-password-allow.xpath) | `https://demo:placeholder@host/path` |
| [raw-authority-bypass](../examples/uri-option-policy/raw-authority-bypass.xpath) | `https://demo:placeholder@host:443/path` |
| [component-precedence](../examples/uri-option-policy/component-precedence.xpath) | `https://new.example.test/new?key=new` |
| [empty-segments-fallback](../examples/uri-option-policy/empty-segments-fallback.xpath) | `https://host/raw` |
| [empty-string-segment](../examples/uri-option-policy/empty-string-segment.xpath) | `https://host` |
| [empty-query-map-observed](../examples/uri-option-policy/empty-query-map-observed.xpath) | `https://host?raw=1` |
| [query-map-override](../examples/uri-option-policy/query-map-override.xpath) | `https://host?q=A%2BB` |
| [omit-default-port](../examples/uri-option-policy/omit-default-port.xpath) | `https://host` |
| [retain-nondefault-port](../examples/uri-option-policy/retain-nondefault-port.xpath) | `https://host:8443` |
| [empty-password](../examples/uri-option-policy/empty-password.xpath) | `demo:` |
| [nonboolean-parse-observed](../examples/uri-option-policy/nonboolean-parse-observed.xpath) | `demo:placeholder` |
| [nonboolean-build-observed](../examples/uri-option-policy/nonboolean-build-observed.xpath) | `https://demo:placeholder@host` |
| [empty-options](../examples/uri-option-policy/empty-options.xpath) | `false` |
| [invalid-ipv6](../examples/uri-option-policy/invalid-ipv6.xpath) | `FOUR0001` |
| [parse-nonmap-options](../examples/uri-option-policy/parse-nonmap-options.xpath) | `XPTY0004` |
| [build-nonmap-options](../examples/uri-option-policy/build-nonmap-options.xpath) | `XPTY0004` |
| [empty-input-invalid-options](../examples/uri-option-policy/empty-input-invalid-options.xpath) | `XPTY0004` |
| [keyword-options](../examples/uri-option-policy/keyword-options.xpath) | `host` |
| [nonboolean-unc-observed](../examples/uri-option-policy/nonboolean-unc-observed.xpath) | `file` |

Names ending `observed` freeze current behavior rather than certify draft conformance. The empty-input/non-map case verifies argument conversion rejection.

## 14. Performance

Operations process strings and maps in memory. No benchmark, destination access or universal complexity guarantee is claimed.

## 15. Known Uses

This archive/partner case is synthetic integration evidence. Existing [XPath40FunctionTests](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs) include `ParseUri_UserInfoPasswordDiscardedUnlessDeprecatedAllowed`, `ParseUri_OmitDefaultPorts`, `ParseUri_FileScheme_SegmentsAndFilepath`, `ParseUri_UnmatchedBracketInAuthority_RaisesFour0001`, `BuildUri_OmitDefaultPortsOption`, `BuildUri_UncPathOption_DoubleSlashPrefix`, `BuildUri_QueryParameters_EscapeAndEmptyKey` and `BuildUri_PathSegmentsEscapedOnlyWhenHierarchical`.
The [4.0 runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs) has no exact fn-parse-uri or fn-build-uri GatedSets assignment. Named tests and fixtures provide separate evidence; gate membership is unchanged.

## 16. Related Features

[Invoice API URIs](invoice-api-uris.md), [completeness audit](../COMPLETENESS-AUDIT.md) and [supplementary coverage audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md). Option coverage does not complete REQ-126 or certify every URI edge case.
